using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TTTM
{
    /*
     * 3-его бота доделать норм
     * TODO для ботов:
     * - сделать анализ ячеек и оценивание их (в виде дерева)
     * - каждой ячейке соответствует количество очков
     * - выигрыш при ходе в ту ячейку + к очкам
     * - закрытие выигрыша противнику + к очкам
     * задать настройку бота которую можно взять из файла:
     * - указаны в проценташ шансы того что он прибавит к очкам за то или иное соответствие
     * - указано количество очков
     * - указан шанс что бот будет выбирать лучшее значение (например 50% что наилучший ход, 30% что второй, 15% что третий, 5% что последний)
     */
    
    class LowLevelConnection
    {
        // Состояние
        public enum State : int
        {
            Off, Listening, Connected
        }
        public State state { private set; get; } // Текущее состояние

        private Thread ListeningThread; // Топок, в котором будет прослушиваться порт
        private TcpListener Listener; // Прослушивание порта для ожидания противника
        private Thread WorkWithClient; // Поток обрабатывающий приходящие от клиента/сервера данные
        private Socket Handler; // Противник
        
        bool stoping = false;

        // События
        public event EventHandler AnotherPlayerConnected; // Серверное
        public event EventHandler AnotherPlayerDisconnected; // Серверное
        public event EventHandler<ReceivedDataEventArgs> ReceivedData;

        public class ReceivedDataEventArgs : EventArgs
        {
            public string Data { private set; get; }
            public ReceivedDataEventArgs(string Data)
            {
                this.Data = Data;
            }
        }

        // Остановка прослушивания порта
        public void StopServerListening()
        {
            if (state != State.Listening)
                return;

            Listener.Stop();
            ListeningThread.Abort();
            state = State.Off;
        }

        // Старт прослушивания порта
        public void StartServerListening(int port)
        {
            IPAddress ipAddr = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);
            Listener = new TcpListener(ipEndPoint);

            try
            {
                Listener.Start();
            }
            catch (SocketException e)
            {
                throw e;
            }

            ListeningThread = new Thread(Listening);
            ListeningThread.IsBackground = true;
            ListeningThread.Start();
        }

        // Подключение к серверу
        public void ConnectToServer(string ip, int port)
        {
            IPAddress ipAddr;
            IPAddress.TryParse(ip, out ipAddr);

            Handler = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Handler.Connect(ip, port);
            }
            catch (SocketException e)
            {
                state = State.Off;
                throw e;
            }

            Handler.ReceiveBufferSize = 1024;
            state = State.Connected;
            WorkWithClient = new Thread(new ParameterizedThreadStart(ProccessReceivedData));
            WorkWithClient.Start(Handler);
        }

        public void Disconnect()
        {
            if (state == State.Connected)
            {
                //WorkWithClient.Abort();
                Handler.Close();
                stoping = true;
                state = State.Off;
            }
        }

        // Прослушивание
        private void Listening()
        {
            if (Listener == null)
                return;

            state = State.Listening;
            while (!Listener.Pending())
                Thread.Sleep(250);

            Handler = Listener.AcceptSocket();
            Listener.Stop();
            state = State.Connected;
            AnotherPlayerConnected(this, new EventArgs());
            WorkWithClient = new Thread(new ParameterizedThreadStart(ProccessReceivedData));
            WorkWithClient.Start(Handler);
        }

        // Обработка приходящих данных
        private void ProccessReceivedData(object ohandler)
        {
            // Принимаем хэндлер
            Socket handler = (Socket)ohandler;
            string data = "";
            stoping = false;

            // Зацикливаем, пока приходят данные
            while (!stoping && handler != null)
            {
                // Пыдаемся получить данные и вылетаем из цикла, если отключился
                byte[] bytes = new byte[handler.ReceiveBufferSize];

                if (!handler.IsConnected())
                    break;
                else
                {
                    // Приём данных
                    if (handler.Available > 0)
                    {
                        int bytesRec = handler.Receive(bytes);
                        if (bytesRec == 0)
                            continue;

                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        ReceivedData?.Invoke(this, new ReceivedDataEventArgs(data));
                        data = "";
                    }
                    else
                        Thread.Sleep(100);
                }
            }
            handler?.Close();
            AnotherPlayerDisconnected(this, new EventArgs());
        }

        // Отправка данных
        public void Send(string Data)
        {
            if (state == State.Connected)
            Handler.Send(Encoding.UTF8.GetBytes(Data), Encoding.UTF8.GetByteCount(Data), SocketFlags.None);
        }
    }
    
    public class Connection
    {
        // Состояние
        public enum State : int
        {
            Off, Listening, Connected, WaitForStartFromAnother, WaitForStartFromMe, Game
        }
        public State state { private set; get; } // Текущее состояние
        public bool? Host { private set; get; }
        LowLevelConnection LLCon = new LowLevelConnection();

        // События
        public event EventHandler AnotherPlayerConnected; // Серверное
        public event EventHandler AnotherPlayerDisconnected; // Серверное
        public event EventHandler GameStarts;
        public event EventHandler GameEnds;

        public event EventHandler<string> ReceivedChat;
        public event EventHandler<string> ReceivedTurn;
        public event EventHandler ConnectingRejected;
        public event EventHandler<IAMEventArgs> ReceivedIAM;

        public class IAMEventArgs : EventArgs
        {
            public string Nick { private set; get; }
            public Color Color { private set; get; }

            public IAMEventArgs(string Data)
            {
                string[] Strings = Data.Split('\n');
                Nick = Strings[1];
                Color = Color.FromArgb(int.Parse(Strings[2]));
            }
        }

        public Connection()
        {
            LLCon.AnotherPlayerConnected += LLCon_AnotherPlayerConnected;
            LLCon.AnotherPlayerDisconnected += LLCon_AnotherPlayerDisconnected;
            LLCon.ReceivedData += LLCon_ReceivedData;
        }

        ~Connection()
        {
            LLCon.AnotherPlayerConnected -= LLCon_AnotherPlayerConnected;
            LLCon.AnotherPlayerDisconnected -= LLCon_AnotherPlayerDisconnected;
            LLCon.ReceivedData -= LLCon_ReceivedData;
            LLCon.StopServerListening();
        }

        private void LLCon_ReceivedData(object sender, LowLevelConnection.ReceivedDataEventArgs e)
        {
            string Data = e.Data;

            switch (Data.Substring(0, 3))
            {
                case "TRN": // Ход
                    ReceivedTurn?.Invoke(this, null); // заменить null на ход
                    break;
                case "CHT": // Сообщение в чате
                    ReceivedChat?.Invoke(this, Data.Substring(3)); // заменить
                    break;
                case "IAM": // Представление
                    ReceivedIAM?.Invoke(this, new IAMEventArgs(Data));
                    break;
                case "GAM": // Game - начало игры
                    if (state == State.Connected)
                        state = State.WaitForStartFromMe;
                    else if (state == State.WaitForStartFromAnother)
                    {
                        state = State.Game;
                        GameStarts(this, new EventArgs());
                    }
                    break;
                case "END": // Противник прервал игру
                    GameEnds(this, new EventArgs());
                    break;
                case "RJC":
                    ConnectingRejected.Invoke(this, new EventArgs());
                    break;
                default:
                    throw new Exception("Неверный заголовой данных: " + Data.Substring(0, 3));
            }
        }

        private void LLCon_AnotherPlayerDisconnected(object sender, EventArgs e)
        {
            AnotherPlayerDisconnected?.Invoke(this, e);
            if (Host.Value)
                state = State.Listening;
            else
                state = State.Off;
        }
        private void LLCon_AnotherPlayerConnected(object sender, EventArgs e)
        {
            AnotherPlayerConnected?.Invoke(this, e);
            state = State.Connected;
        }

        public void SendTurn(Position Turn)
        {
            string Data = "TRN" + Turn.x.ToString() + Turn.y.ToString();
            LLCon.Send(Data);
        }
        public void SendIAM(string Nick, Color Color)
        {
            string Data = "IAM" + '\n' + Nick + '\n' + Color.ToArgb() + '\n';
            LLCon.Send(Data);
        }
        public void SendStartGame()
        {
            LLCon.Send("GAM");
            if (state == State.WaitForStartFromMe)
            {
                state = State.Game;
                GameStarts(this, new EventArgs());
            }
            else if (state == State.Connected)
                state = State.WaitForStartFromAnother;
        }
        public void SendEndGame()
        {
            LLCon.Send("END");
        }
        public void SendReject()
        {
            LLCon.Send("RJC");
        }
        public void SendChat(string Text)
        {
            LLCon.Send("CHT" + Text);
        }

        // Пересылаемые на подключение методы
        public void ConnectToServer(string IP, int port)
        {
            LLCon.ConnectToServer(IP, port);
            state = State.Connected;
            Host = false;
        }
        public void StopServerListening()
        {
            LLCon.StopServerListening();
            state = (State)(byte)LLCon.state;
        }
        public void Disconnect()
        {
            LLCon.Disconnect();
            state = (State)(byte)LLCon.state;
        }
        public void StartServerListening(int Port)
        {
            try
            {
                LLCon.StartServerListening(Port);
                state = State.Listening;
                Host = true;
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }



    // Абстрактный бот (как базовый класс для реализаций бота)
    abstract public class ABot
    {
        protected Random rnd = new Random();
        public Player Player { protected set; get; }
        public Player HumanPlayer { protected set; get; }
        public Game Game { protected set; get; }
        public abstract void makeTurn();
    }

    // Класс рандомного бота
    public class StupidBot : ABot
    {
        public StupidBot(Player player, Game game)
        {
            Player = player;
            Game = game;
        }

        public override void makeTurn()
        {
            Position Field = Game.CurrentField;
            int x, y;
            if (!Game.Fields[Field.x, Field.y].Full)
            {
                do
                {
                    x = rnd.Next(0, 3);
                    y = rnd.Next(0, 3);
                }
                while (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[x, y].Owner != null);
                x += Field.x * 3;
                y += Field.y * 3;
            }
            else
            {
                do
                {
                    x = rnd.Next(0, 9);
                    y = rnd.Next(0, 9);
                }
                while (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[x, y].Owner != null);
            }
            if (!Game.Turn(new Position(x, y), Player))
                throw new Exception("Бот не смог сделать ход");
        }
    }

    // Класс чуть более умного бота
    public class SomeMoreCleverBot : ABot
    {
        public SomeMoreCleverBot(Player player, Game game)
        {
            Player = player;
            Game = game;
        }

        public SomeMoreCleverBot(Player player, Player hplayer, Game game)
        {
            Player = player;
            HumanPlayer = hplayer;
            Game = game;
        }

        private Position check3(Position p1, Position p2, Position p3, Player Plr)
        {
            if (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p1.x, p1.y].Owner == Plr &&
                Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p2.x, p2.y].Owner == Plr &&
                Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p3.x, p3.y].Owner == null)
                return p3;

            if (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p1.x, p1.y].Owner == Plr &&
                Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p2.x, p2.y].Owner == null &&
                Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p3.x, p3.y].Owner == Plr)
                return p2;

            if (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p1.x, p1.y].Owner == null &&
                Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p2.x, p2.y].Owner == Plr &&
                Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[p3.x, p3.y].Owner == Plr)
                return p1;

            return null;
        }

        protected virtual Position findBetterPos()
        {
            List<Position> Results = new List<Position>();
            Position Current;
            for (int i = 0; i < 3; i++)
            {
                Current = check3(new Position(0, i), new Position(1, i), new Position(2, i), Player);
                if (Current != null) Results.Add(Current);
                Current = check3(new Position(i, 0), new Position(i, 1), new Position(i, 2), Player);
                if (Current != null) Results.Add(Current);
                Current = check3(new Position(0, i), new Position(1, i), new Position(2, i), HumanPlayer);
                if (Current != null) Results.Add(Current);
                Current = check3(new Position(i, 0), new Position(i, 1), new Position(i, 2), HumanPlayer);
                if (Current != null) Results.Add(Current);
            }
            Current = check3(new Position(0, 0), new Position(1, 1), new Position(2, 2), Player);
            if (Current != null) Results.Add(Current);
            Current = check3(new Position(2, 0), new Position(1, 1), new Position(0, 2), Player);
            if (Current != null) Results.Add(Current);

            Current = check3(new Position(0, 0), new Position(1, 1), new Position(2, 2), HumanPlayer);
            if (Current != null) Results.Add(Current);
            Current = check3(new Position(2, 0), new Position(1, 1), new Position(0, 2), HumanPlayer);
            if (Current != null) Results.Add(Current);

            if (Results.Count > 0)
                return Results[rnd.Next(Results.Count)];
            else
                return null;
        }

        public override void makeTurn()
        {
            Position Field = Game.CurrentField;
            int x = 0, y = 0;
            if (!Game.Fields[Field.x, Field.y].Full)
            {
                Position finded = findBetterPos();
                if (finded == null)
                {
                    do
                    {
                        x = rnd.Next(0, 3);
                        y = rnd.Next(0, 3);
                    }
                    while (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[x, y].Owner != null);
                    x += Field.x * 3;
                    y += Field.y * 3;
                }
                else
                {
                    x = Field.x * 3 + finded.x;
                    y = Field.y * 3 + finded.y;
                }
            }
            else
            {
                do
                {
                    x = rnd.Next(0, 9);
                    y = rnd.Next(0, 9);
                    if (Game.Fields[x / 3, y / 3].Owner != null)
                        continue;
                }
                while (Game.Fields[x/3, y/3].Cells[x%3, y%3].Owner != null);
            }
            if (!Game.Turn(new Position(x, y), Player))
                throw new Exception("Бот не смог сделать ход");
        }
    }

    public class Bot3 : SomeMoreCleverBot
    {
        public Bot3(Player player, Player hplayer, Game game) : base(player, hplayer, game)
        {
        }

        private Position check3(Position p1, Position p2, Position p3, Player Plr, Position Field = null)
        {
            if (Field == null)
                Field = Game.CurrentField;
            if (Game.Fields[Field.x, Field.y].Cells[p1.x, p1.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p2.x, p2.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p3.x, p3.y].Owner == null)
                return p3;

            if (Game.Fields[Field.x, Field.y].Cells[p1.x, p1.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p2.x, p2.y].Owner == null &&
                Game.Fields[Field.x, Field.y].Cells[p3.x, p3.y].Owner == Plr)
                return p2;

            if (Game.Fields[Field.x, Field.y].Cells[p1.x, p1.y].Owner == null &&
                Game.Fields[Field.x, Field.y].Cells[p2.x, p2.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p3.x, p3.y].Owner == Plr)
                return p1;

            return null;
        }

        private float[] CalculateScores(Position Field = null, int Depth = 0)
        {
            if (Field == null)
                Field = Game.CurrentField;

            // Инициализируем массив 9 элементов для ячеек
            float[] Scores = new float[9];
            bool[] StepDeny = new bool[9];

            // Ценность центра и углов выше
            /*Scores[0] = 0.1f;
            Scores[2] = 0.1f;
            Scores[6] = 0.1f;
            Scores[8] = 0.1f;
            Scores[4] = 0.3f;*/

            // Создаём список позиций куда надо поставить чтоб было 3 подряд
            Position Current;
            List<Position> AddScoresBot = new List<Position>();
            List<Position> AddScoresHuman = new List<Position>();
            for (int i = 0; i < 3; i++)
            {
                Current = check3(new Position(0, i), new Position(1, i), new Position(2, i), Player, Field);
                if (Current != null) AddScoresBot.Add(Current);
                Current = check3(new Position(0, i), new Position(1, i), new Position(2, i), HumanPlayer, Field);
                if (Current != null) AddScoresHuman.Add(Current);
                Current = check3(new Position(i, 0), new Position(i, 1), new Position(i, 2), Player, Field);
                if (Current != null) AddScoresBot.Add(Current);
                Current = check3(new Position(i, 0), new Position(i, 1), new Position(i, 2), HumanPlayer, Field);
                if (Current != null) AddScoresHuman.Add(Current);
            }
            Current = check3(new Position(0, 0), new Position(1, 1), new Position(2, 2), Player, Field);
            if (Current != null) AddScoresBot.Add(Current);
            Current = check3(new Position(2, 0), new Position(1, 1), new Position(0, 2), Player, Field);
            if (Current != null) AddScoresBot.Add(Current);
            Current = check3(new Position(0, 0), new Position(1, 1), new Position(2, 2), HumanPlayer, Field);
            if (Current != null) AddScoresHuman.Add(Current);
            Current = check3(new Position(2, 0), new Position(1, 1), new Position(0, 2), HumanPlayer, Field);
            if (Current != null) AddScoresHuman.Add(Current);

            // Добавляем к очкам ячеек позиции из списка выше (Заполнить поле лучше чем помещать противнику)
            foreach (var item in AddScoresBot)
            {
                if (Depth % 2 == 0)
                    Scores[item.x + item.y * 3] += 10;
                else
                    Scores[item.x + item.y * 3] += 8;
            }
            foreach (var item in AddScoresHuman)
            {
                if (Depth % 2 == 1)
                    Scores[item.x + item.y * 3] += 10;
                else
                    Scores[item.x + item.y * 3] += 8;
            }

            // ЗАМЕНИТЬ
            for (int i = 0; i < 9; i++)
            {
                if (Game[Field.x, Field.y, i % 3, i / 3].Owner != null)
                    StepDeny[i] = true;
            }

            // Оценка свободных ячеек в следующем поле
            for (int i = 0; i < 9; i++)
            {
                var FreeCells = 0;
                for (int j = 0; j < 9; j++)
                {
                    if (Game.Fields[i % 3, i / 3].Cells[j % 3, j / 3].Owner == null)
                        FreeCells += 1;
                }

                if (Game.Fields[i % 3, i / 3].Owner == null)
                {
                    if (FreeCells != 0)
                        Scores[i] += (9 - FreeCells) / 10f; // Чем меньше в поле противника свободных ячеек тем лучше
                        //Scores[i] += (FreeCells) / 4f; // Чем больше в поле противника свободных ячеек тем лучше
                    else
                        Scores[i] -= 20; // Если заняты все - то это плохая идея
                }
                else //Owner != null
                {
                    if (FreeCells != 0)
                        Scores[i] += 22;
                    else
                        Scores[i] -= 40;
                }
            }

            float[] MaxScoresFromRecursion = new float[9];
            float[] MidScoresFromRecursion = new float[9];
            if (Depth < 5)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (!StepDeny[i])
                    {
                        var CalculatedScores = CalculateScores(new Position(i % 3, i / 3), Depth + 1);
                        MaxScoresFromRecursion[i] = CalculatedScores.Max();
                        MidScoresFromRecursion[i] = CalculatedScores.Sum() / 9f;
                    }
                    if (Depth == 0 && StepDeny[i])
                        Scores[i] -= 1000;
                    Scores[i] -= MaxScoresFromRecursion[i] * 0.75f + MidScoresFromRecursion[i] / 10f;
                }
            }
            return Scores;
        }

        protected override Position findBetterPos()
        {
            var Scores = CalculateScores();

            // Ищем индекс с максимальным счётом
            List<int> Max = new List<int>();
            Max.Add(0);
            int Min = 0;
            for (int i = 0; i < 9; i++)
            {
                if (Scores[i] < Scores[Min])
                    Min = i;
                if (Scores[i] == Scores[Max[0]])
                    Max.Add(i);
                if (Scores[i] > Scores[Max[0]])
                {
                    Max.Clear();
                    Max.Add(i);
                }
            }

            // Если разницы между ячейками нет то возвращаем null
            if (Scores[Max[0]] == Scores[Min])
                return null;

            // Из получившегося списка берём рандомный элемент
            var TurnIndex = Max[rnd.Next(Max.Count)];

            // Возвращаем его
            return new Position(TurnIndex % 3, TurnIndex / 3);
        }

        public override void makeTurn()
        {
            Position Field = Game.CurrentField;
            int x = 0, y = 0;
            if (!Game.Fields[Field.x, Field.y].Full)
            {
                Position finded = findBetterPos();
                if (finded == null)
                {
                    do
                    {
                        x = rnd.Next(0, 3);
                        y = rnd.Next(0, 3);
                    }
                    while (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[x, y].Owner != null);
                    x += Field.x * 3;
                    y += Field.y * 3;
                }
                else
                {
                    x = Field.x * 3 + finded.x;
                    y = Field.y * 3 + finded.y;
                }
            }
            else
            {
                do
                {
                    x = rnd.Next(0, 9);
                    y = rnd.Next(0, 9);
                    if (Game.Fields[x / 3, y / 3].Owner != null)
                        continue;
                }
                while (Game.Fields[x / 3, y / 3].Cells[x % 3, y % 3].Owner != null);
            }
            if (!Game.Turn(new Position(x, y), Player))
                throw new Exception("Бот не смог сделать ход");
        }
    }

    // Класс настроек
    public class Settings
    {
        public Color BackgroundColor;
        public Color SmallGrid;
        public Color BigGrid;
        public Color PlayerColor1;
        public Color PlayerColor2;
        public Color IncorrectTurn;
        public Color FilledField = Color.Gray;
        public string DefaultName1 = "";
        public string DefaultName2 = "";
        public string MpIP = "";
        public int MpPort = 0;
        public Color HelpColor;
        public int HelpCellsAlpha;
        public int HelpLinesAlpha;
        public int HelpShow; //(bool)
        public int GraphicsLevel;


        // Сохранение настроек в файл
        public static bool Save(string fname, Settings settings)
        {
            // Выход если настроек нет
            if (settings == null)
                return false;

            using (StreamWriter sw = new StreamWriter(fname))
            {
                // Заголовок файла
                sw.WriteLine("[Tic Tac Toe Settings]");
                // Получаем массив полей класса Settings
                FieldInfo[] fis = typeof(Settings).GetFields();
                // Проходим по всем полям
                foreach (var fi in fis)
                {
                    // Записываем "Имя поля = "
                    sw.Write(fi.Name + "=");
                    // Записываем значение в зависимости от его типа
                    object val = fi.GetValue(settings);
                    if (val is String || val is int)
                        sw.WriteLine(val);
                    else if (val is Color)
                        sw.WriteLine(((Color)val).ToArgb());
                    else if (val == null)
                        throw new Exception("Попытка сохранить настройки с полями со значениями null");
                }
                return true;
            }
        }
        public static bool Load(string fname, out Settings settings)
        {
            // Создаём объект настроек, в любом случае пригодится, да и возвращать что-то надо
            settings = new Settings();

            // Выход если файл отсутствует
            if (!File.Exists(fname))
            {
                settings.SetDefaults();
                return false;
            }

            using (StreamReader sr = new StreamReader(fname))
            {
                // Пропускаем заголовок
                sr.ReadLine();
                // Читаем строки до конца файла
                while (!sr.EndOfStream)
                {
                    string str = sr.ReadLine();

                    // Вытаскиваем из строки название поля и его значение
                    string field = str.Substring(0, str.IndexOf('='));
                    dynamic val = str.Substring(str.IndexOf('=') + 1);

                    // Color
                    if (typeof(Settings).GetFields().First(f => f.Name == field).FieldType == typeof(Color))
                    {
                        int temp;
                        int.TryParse(val, out temp);
                        val = Color.FromArgb(temp);
                    }
                    else // int
                    if (typeof(Settings).GetFields().First(f => f.Name == field).FieldType == typeof(int))
                    {
                        int temp;
                        int.TryParse(val, out temp);
                        val = temp;
                    }

                    // Записываем значение в настройки
                    typeof(Settings).GetFields().First(f => f.Name == field).SetValue(settings, val);
                }
                return true;
            }
        }

        public void SetDefaults()
        {
            // Сделать выполнение этого в настройках
            BackgroundColor = Color.Black;
            SmallGrid = Color.FromArgb(0, 200, 0);
            BigGrid = Color.Lime;
            PlayerColor1 = Color.Red;
            PlayerColor2 = Color.FromArgb(100, 100, 255);
            IncorrectTurn = Color.Orange;
            FilledField = Color.Gray;
            DefaultName1 = "Игрок 1";
            DefaultName2 = "Игрок 2";
            MpIP = "127.0.0.1";
            MpPort = 7890;
            HelpColor = Color.Aqua;
            HelpCellsAlpha = 180;
            HelpLinesAlpha = 40;
            HelpShow = 1;
            GraphicsLevel = 1;
        }
    }

    // Класс координат для игры
    public class Position
    {
        // Свойства
        public int x { private set; get; }
        public int y { private set; get; }

        // Преобразование координаты ячейки/поля
        public static Position GetCellFrom9x9(Position pos)
        {
            return pos % 3;
        }
        public static Position GetFieldFrom9x9(Position pos)
        {
            return pos / 3;
        }

        // Конструктор
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Переопределение операторов %, /, *, метода сравнения и получения хэш-кода
        public override bool Equals(object another)
        {
            if (another is Position)
                return x == ((Position)another).x && y == ((Position)another).y;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static Position operator% (Position pos, int number)
        {
            return new Position(pos.x % number, pos.y % number);
        }
        public static Position operator/ (Position pos, int number)
        {
            return new Position(pos.x / number, pos.y / number);
        }
        public static Position operator* (Position pos, int number)
        {
            return new Position(pos.x * number, pos.y * number);
        }
    }

    // Класс хранящий состояние поля 3х3
    public struct FieldState
    {
        public Player Owner;
        public int OwnerID;
        public bool Filled;

        public FieldState(Player Owner, bool Filled)
        {
            this.Owner = Owner;
            OwnerID = Owner?.Id ?? 0;
            this.Filled = Filled;
        }
    }

    // Менеджер для одиночной игры с ботом
    public class GameManagerWithBot : GameManagerWthFriend
    {
        public ABot Bot { private set; get; }
        public override event EventHandler<Player> ChangeTurn;
        public override event EventHandler<Position> IncorrectTurn;
        public override event EventHandler NobodyWins;
        public GameManagerWithBot(string player1, string player2, int botType = 3) : base(player1, player2)
        {
            // BASE CTOR HERE
            switch (botType)
            {
                case 1:
                    Bot = new StupidBot(Player2, game);
                    break;
                case 2:
                    Bot = new SomeMoreCleverBot(Player2, Player1, game);
                    break;
                case 3:
                    Bot = new Bot3(Player2, Player1, game);
                    break;
                default:
                    break;
            }
        }

        public void BotTurn()
        {
            // Вылет если ход не бота
            if (CurrentPlayer != Player2)
                return;

            Bot.makeTurn();
            CurrentPlayer = Player1;

            // Вызов события об обновлении хода
            ChangeTurn(this, CurrentPlayer);
        }

        public override void ClickOn(int i, int j)
        {
            // Вылет если игра закончена
            if (game.Finished)
                return;

            // Вылет если сейчас ход бота
            if (CurrentPlayer != Player1)
                return;

            // Выполнение хода
            bool res = game.Turn(new Position(i, j), CurrentPlayer);

            // Если ход выполнен успешно (ячейка не занята и ход туда разрешён) - свап игрока
            if (res)
            {
                // Передача хода боту
                CurrentPlayer = Player2;

                // Вызов события об обновлении хода
                ChangeTurn(this, CurrentPlayer);
            }
            else
                // Вызов события некорректного хода
                IncorrectTurn?.Invoke(this, game.CurrentField);
        }
    }

    // Абстрактная реализация менеджера игры
    public class GameManagerWthFriend : IDisposable
    {
        // Свойства
        protected Game game;// { private set; get; }
        protected Player Player1;// { private set; get; }
        protected Player Player2;// { private set; get; }
        public Player CurrentPlayer;
        public event EventHandler<Game.GameEndArgs> SomebodyWins;
        public virtual event EventHandler NobodyWins;
        public virtual event EventHandler<Player> ChangeTurn;
        public virtual event EventHandler<Position> IncorrectTurn;

        // Конструктор
        public GameManagerWthFriend(string player1, string player2)
        {
            game = new Game();
            game.StartGame();
            Player1 = new Player(player1);
            Player2 = new Player(player2);
            CurrentPlayer = Player1;
            game.GameEnds += GameEnds;
        }

        public void Dispose()
        {
            Player1?.Dispose();
            Player2?.Dispose();
        }

        // Обработка конца игры
        private void GameEnds(object sender, Game.GameEndArgs e)
        {
            if (e.Winner != null)
                SomebodyWins?.Invoke(this, e);
            else
                NobodyWins?.Invoke(this, new EventArgs());
        }

        // Обработка клика по полю
        public virtual void ClickOn(int i, int j)
        {
            // Вылет если игра закончена
            if (game.Finished)
                return;

            // Выполнение хода
            bool res = game.Turn(new Position(i, j), CurrentPlayer);

            // Если ход выполнен успешно (ячейка не занята и ход туда разрешён) - свап игрока
            if (res)
            {
                if (CurrentPlayer == Player1)
                    CurrentPlayer = Player2;
                else
                    CurrentPlayer = Player1;

                // Вызов события об обновлении хода
                ChangeTurn(this, CurrentPlayer);
            }
            else
                // Вызов события некорректного хода
                IncorrectTurn?.Invoke(this, game.CurrentField);
        }

        // Методы возвращающие состоящие игровых полей и ячеек
        public int[,] State()
        {
            int[,] res = new int[9, 9];
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    res[i, j] = (game[i / 3, j / 3, i % 3, j % 3].Owner?.Id ?? 0);

            return res;
        }
        public FieldState[,] FieldsState()
        {
            FieldState[,] res = new FieldState[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res[i, j] = new FieldState(game[i, j].Owner, game[i, j].Full);

            return res;
        }

        //// Класс, реализующий одиночную игру с другим игроком
        //class SinglePlayerGame : IDisposable
        //{
        //    // Свойства
        //    public Game game { private set; get; }
        //    public Player Player1 { private set; get; }
        //    public Player Player2 { private set; get; }
        //    public Player CurrentPlayer;
        //    public event EventHandler<Game.GameEndArgs> SomebodyWins;
        //    public event EventHandler NobodyWins;
        //    public event EventHandler<Player> ChangeTurn;
        //    public event EventHandler<Position> IncorrectTurn;

        //    // Конструктор
        //    public SinglePlayerGame(string player1, string player2)
        //    {
        //        game = new Game();
        //        game.StartGame();
        //        Player1 = new Player(player1);
        //        Player2 = new Player(player2);
        //        CurrentPlayer = Player1;
        //        game.GameEnds += GameEnds;
        //    }

        //    public void Dispose()
        //    {
        //        Player1?.Dispose();
        //        Player2?.Dispose();
        //    }

        //    // Обработка конца игры
        //    private void GameEnds(object sender, Game.GameEndArgs e)
        //    {
        //        if (e.Winner != null)
        //            SomebodyWins?.Invoke(this, e);
        //        else
        //            NobodyWins?.Invoke(this, new EventArgs());
        //    }

        //    // Обработка клика по полю
        //    public void ClickOn(int i, int j)
        //    {
        //        // Выполнение хода
        //        bool res = game.Turn(new Position(i, j), CurrentPlayer);

        //        // Если ход выполнен успешно (ячейка не занята и ход туда разрешён) - свап игрока
        //        if (res)
        //        {
        //            if (CurrentPlayer == Player1)
        //                CurrentPlayer = Player2;
        //            else
        //                CurrentPlayer = Player1;

        //            // Вызов события об обновлении хода
        //            ChangeTurn(this, CurrentPlayer);
        //        }
        //        else
        //            // Вызов события некорректного хода
        //            IncorrectTurn?.Invoke(this, game.CurrentField);
        //    }

        //    // Методы возвращающие состоящие игровых полей и ячеек
        //    public int[,] State()
        //    {
        //        int[,] res = new int[9, 9];
        //        for (int i = 0; i < 9; i++)
        //            for (int j = 0; j < 9; j++)
        //                res[i,j] = (game[i / 3, j / 3, i % 3, j % 3].Owner?.Id ?? 0);

        //        return res;
        //    }
        //    public FieldState[,] FieldsState()
        //    {
        //        FieldState[,] res = new FieldState[3, 3];
        //        for (int i = 0; i < 3; i++)
        //            for (int j = 0; j < 3; j++)
        //                res[i, j] = new FieldState(game[i, j].Owner, game[i, j].Full);

        //        return res;
        //    }


        // Сохранение/загрузка
        public string Save()
        {
            return game.GetStateCode();
        }
        public void Load(string State)
        {
            game.UpdateFromStateCode(State, Player1, Player2, ref CurrentPlayer);
            //ChangeTurn(this, CurrentPlayer);
        }
    }

    // Класс содержащий матрицу полей с ячейками, историю ходов и проверяющий корректность хода
    public class Game
    {
        // Свойства
        public bool Finished = false;
        public GameField[,] Fields { private set; get; }
        public Position CurrentField { private set; get; }
        public List<Position> History { private set; get; } = new List<Position>();

        // Событие конца игры
        public event EventHandler<GameEndArgs> GameEnds;
        public class GameEndArgs : EventArgs
        {
            public Player Winner { private set; get; }
            public GameEndArgs(Player Winner)
            {
                this.Winner = Winner;
            }
        }

        // Состояние полей в виде двумерного массива владельцев
        public Player[,] FieldsState()
        {
            Player[,] res = new Player[3,3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res[i, j] = (Fields[i, j].Owner);

            return res;
        }

        // Индексаторы для обращения к полю/ячейке
        public GameCell this[int i, int j, int k, int l]
        {
            get
            {
                return Fields[i, j].Cells[k, l];
            }
        }
        public GameField this[int i, int j]
        {
            get
            {
                return Fields[i, j];
            }
        }

        //Игровые действия
        public void StartGame()
        {
            Finished = false;
            History.Clear();
            foreach (var item in Fields)
            {
                item.Clear();
            }
        }
        public bool Turn(Position pos, Player player)
        {
            // Игра была закончена
            if (Finished)
                return true;

            // Ход вне диапазона
            if (pos.x < 0 || pos.x > 8 || pos.y < 0 || pos.y > 8)
                return false;

            // Проверка верности хода
            Position Cell = Position.GetCellFrom9x9(pos);
            Position Field = Position.GetFieldFrom9x9(pos);

            bool notSameField = !Field.Equals(CurrentField); //Зачем?
            bool fieldIsNotFull = (CurrentField != null) ? !Fields[CurrentField.x, CurrentField.y].Full : true;

            if (notSameField && CurrentField != null && fieldIsNotFull)
                return false;
            
            // Ход
            if (Fields[Field.x, Field.y][Cell.x, Cell.y].Bind(player))
            {
                History.Add(pos);
                CurrentField = Cell;
                return true;
            }
            else
                return false;
        }

        // Конструктор
        public Game()
        {
            Fields = new GameField[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Fields[i, j] = new GameField(i, j);
                    Fields[i, j].Changed += FieldChanged;
                    Fields[i, j].Filled += FieldFilled;
                }
        }

        // Обработка заполнения и победы поля
        private void FieldFilled(object sender, EventArgs e)
        {
            // Проверка на заполненность
            bool full = true;
            for (int i = 0; i < 3 && full; i++)
                for (int j = 0; j < 3 && full; j++)
                    if (Fields[i, j].Owner == null && !Fields[i,j].Full)
                        full = false;

            if (full)
            {
                // Конец игры из-за заполненности всех полей
                Finished = true;
                GameEnds?.Invoke(this, new GameEndArgs(null));
            }
        }
        private void FieldChanged(object sender, EventArgs e)
        {
            if (checkNeighbors(((GameField)sender).Pos))
            {
                // Конец игры из-за победы одного из игроков
                GameEnds?.Invoke(this, new GameEndArgs(((GameField)sender).Owner));
                Finished = true;
            }
        }

        // Проверка соседних с [i, j] полей на нахождение ряда из трёх
        private bool checkNeighbors(Position Pos)
        {
            int i = Pos.x;
            int j = Pos.y;
            if (Fields[i, 0].Owner == Fields[i, 1].Owner && Fields[i, 0].Owner == Fields[i, 2].Owner && Fields[i, 0].Owner != null)
                return true;
            if (Fields[0, j].Owner == Fields[1, j].Owner && Fields[0, j].Owner == Fields[2, j].Owner && Fields[0, j].Owner != null)
                return true;
            if (((i == 0 || i == 2) && (j == 0 || j == 2)) || (i == 1 && j == 1))
                if (((Fields[0, 0].Owner == Fields[1, 1].Owner && Fields[0, 0].Owner == Fields[2, 2].Owner)
                || (Fields[0, 2].Owner == Fields[1, 1].Owner && Fields[1, 1].Owner == Fields[2, 0].Owner)) && Fields[1, 1].Owner != null)
                    return true;

            return false;
        }

        //Сохранение в строку
        public string GetStateCode()
        {
            StringBuilder res = new StringBuilder();

            // Запись полей
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res.Append(Fields[i, j].Owner?.Id ?? 0);

            // Запись ячеек
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    res.Append(Fields[i / 3, j / 3][i % 3, j % 3].Owner?.Id ?? 0);

            // Куда должен совершаться ход
            if (CurrentField == null)
                res.Append('X');
            else
                res.Append(CurrentField.x * 3 + CurrentField.y);

            // ID игрока сделавшего последний ход
            if (CurrentField == null)
                res.Append('X');
            else
            {
                Position lastfield = Position.GetFieldFrom9x9(History.Last());
                int lastPlayerID = Fields[lastfield.x, lastfield.y].Cells[CurrentField.x, CurrentField.y].Owner.Id;
                res.Append(lastPlayerID);
            }

            return res.ToString();
        }
        public bool UpdateFromStateCode(string State, Player p1, Player p2, ref Player currentPlayer)
        {
            string Backup = GetStateCode();

            if (State.Length != 92)
                return false;

            foreach (var item in Fields)
            {
                item.Clear();
            }

            try
            {
                int k = 0;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++, k++)
                    {
                        if (State[k] == '1')
                            Fields[i, j]._Bind(p1);
                        if (State[k] == '2')
                            Fields[i, j]._Bind(p2);
                    }

                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++, k++)
                    {
                        if (State[k] == '1')
                            Fields[i / 3, j / 3][i % 3, j % 3].Bind(p1);
                        if (State[k] == '2')
                            Fields[i / 3, j / 3][i % 3, j % 3].Bind(p2);
                    }

                if (State[90] == 'X')
                    CurrentField = null;
                else
                {
                    int temp = int.Parse(State[90].ToString());
                    CurrentField = new Position(temp / 3, temp % 3);
                }

                if (State[91] == '1')
                    currentPlayer = p2;
                else if (State[91] == '2')
                    currentPlayer = p1;

                Finished = false;
                return true;
            }
            catch
            {
                UpdateFromStateCode(Backup, p1, p2, ref currentPlayer);
                return false;
            }
        }
    }

    // Класс игрока
    public class Player : IDisposable
    {
        // Счётчик созданных игроков для присвоения уникального идентификатора 
        private static int Players = 0;

        // Свойства
        public int Id { private set; get; }
        public string Name { private set; get; }

        // Конструктор
        public Player(string Name)
        {
            Id = ++Players;
            this.Name = Name;
        }

        // Освобождение памяти после игрока
        public void Dispose()
        {
            Players--;
        }
    }

    // Игровая ячейка
    public class GameCell : AGameCell
    {
        // Событие
        public event EventHandler Changed;

        // Конструктор
        public GameCell(int X, int Y) : base(X, Y)
        {
        }

        // Ход
        public bool Bind(Player P)
        {
            if (Owner != null || P == null)
                return false;
            else
            {
                Owner = P;
                Changed?.Invoke(this, new EventArgs());
            }

            return true;
        }
    }

    // Игровое поле 3х3
    public class GameField : AGameCell
    {
        // Свойства
        public bool Full { private set; get; } = false;
        public GameCell[,] Cells { private set; get; }

        // Событие
        public event EventHandler Changed;
        public event EventHandler Filled;

        // Индексатор для обращения к ячейкам
        public GameCell this[int i,int j]
        {
            get
            {
                return Cells?[i, j];
            }
        }

        // Конструктор
        public GameField(int X, int Y) : base(X, Y)
        {
            Cells = new GameCell[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Cells[i, j] = new GameCell(i, j);
                    Cells[i, j].Changed += CellChanged;
                }
        }

        // Проверка соседних клеток с [i, j] на нахождение ряда из трёх
        private bool checkNeighbors(Position Pos)
        {
            int i = Pos.x;
            int j = Pos.y;
            if (Cells[i, 0].Owner == Cells[i, 1].Owner && Cells[i, 0].Owner == Cells[i, 2].Owner && Cells[i, 0].Owner != null)
                return true;
            if (Cells[0, j].Owner == Cells[1, j].Owner && Cells[0, j].Owner == Cells[2, j].Owner && Cells[0, j].Owner != null)
                return true;
            if (((i == 0 || i == 2) && (j == 0 || j == 2)) || (i == 1 && j == 1))
                if (((Cells[0, 0].Owner == Cells[1, 1].Owner && Cells[0, 0].Owner == Cells[2, 2].Owner)
                || (Cells[0, 2].Owner == Cells[1, 1].Owner && Cells[1, 1].Owner == Cells[2, 0].Owner)) && Cells[1, 1].Owner != null)
                    return true;

            return false;
        }

        // Обработка изменения ячейки
        private void CellChanged(object sender, EventArgs e)
        {
            // Проверка на победу в этом поле
            if (Owner == null)
                if (checkNeighbors(((GameCell)sender).Pos))
                {
                    Owner = ((GameCell)sender).Owner;

                    Changed?.Invoke(this, new EventArgs());
                }

            // Проверка на заполненность
            bool full = true;
            for (int i = 0; i < 3 && full; i++)
                for (int j = 0; j < 3 && full; j++)
                    if (Cells[i, j].Owner == null)
                        full = false;
            
            if (full)
            {
                Full = true;

                // Фу, скатился, отписка
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        Cells[i, j].Changed -= CellChanged;

                Filled?.Invoke(this, e);
            }
        }


        public void _Bind(Player P)
        {
            Owner = P;
        }

        // Очищение поля
        public override void Clear()
        {
            base.Clear();
            if (Full)
            {
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        Cells[i, j].Changed += CellChanged;
            }
            Full = false;
            foreach (var item in Cells)
            {
                item.Clear();
            }
        }
    }

    public abstract class AGameCell
    {
        // Свойства
        public Position Pos { protected set; get; }
        public Player Owner { protected set; get; }

        public AGameCell(int X, int Y)
        {
            Pos = new Position(X, Y);
        }

        // Очищение ячейки
        public virtual void Clear()
        {
            Owner = null;
        }
    }
}
