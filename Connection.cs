using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using LiteNetLib;
using System.Threading.Tasks;

namespace TTTM
{
    public class Connection2
    {
        // состояние подключения для клиента

        /*
         * Принцип соединения:
         * Начальное состояние |off, off|
         * Server |Created,off| - RegisterOnTheWeb
         * Client |Created, PreConnecting| -  WantToConnect
         * Server |Created, PreConnectin| - CheckWhoWant
         * Server |Connecting, PreConnectin| - WriteReady
         * Client |Connecting, Connecting| - ReadReady
         * Client |Connecting, Establishing| - WriteClientEP
         * Server |Establishing, Establishing| - ReadClientEP
         * Client TryConnect =>>
         * Server TryConnect =>>
         * Client SendIAM
         * Server SendIAM
         * Server & Client |Connected, Connected|
         * Client |Connecten, WaitForStartFromAnother| => SendStart
         * Server |WaitForStartFromMe, WaitForStartFromAnother| <= SendStart
         * Server |Game, WaitForStartFromAnother| => SendStart
         * Client |Game, Game| <= SendStart
         * 
         */
        public enum State : int
        {
            Off, // Сервер выключен
            Created, // Сервер добавлен в список серверов на сайте (периодически опрашивать сайт о том желает ли кто подключится)
            PreConnecting, // Клиент ждём готовности к подключению от сервера
            Connecting, // Сервер и клиент готовы к соединению
            Establishing, // Соединение (одновременное
            Connected, // Соединение установлено
            WaitForStartFromAnother, // Ожидание начала игры от другого игрока
            WaitForStartFromMe, // Ожидание начала игры от меня
            Game // Игра
        }
        public enum NetworkRole
        { Nope, Server, Client }
        public struct IAMData
        {
            public string Nick;
            public Color Color;

            public IAMData(string Nick, Color Color)
            {
                this.Nick = Nick;
                this.Color = Color;
            }
        }
        public class IAMEventArgs : EventArgs
        {
            public string Nick { private set; get; }
            public Color Color { private set; get; }

            public IAMEventArgs(string Data)
            {
                string[] Strings = Data.Split('\n');
                if (Strings.Length != 4)
                    throw new Exception("Прислана некорректная информация об игроке");
                Nick = Strings[1];
                Color = Color.FromArgb(int.Parse(Strings[2]));
            }
        }
        public State state { private set; get; }
        public NetworkRole Role;
        private string AccessKey, PublicKey;
        private IPEndPoint LocalEP, PublicEP, RemoteEP;
        NetClient Client;
        public IAMData IAM;
        EventBasedNetListener Listener;
        int WaitServerResponseTimeout;
        Task CheckItselfTask;
        Task CheckServerReadyTask;
        Task CheckClientEPTask;

        public event EventHandler ConnectingRejected;
        public event EventHandler ServerIsntReady;
        public event EventHandler<IAMEventArgs> OpponentConnected;
        public event EventHandler OpponentDisconnected;
        public event EventHandler<bool> GameStarts; // true if I first
        public event EventHandler GameEnds;
        public event EventHandler<string> ServerLog;
        public event EventHandler RestartGame;
        public event EventHandler RestartRejected;
        public event EventHandler<string> ReceivedChat;
        public event EventHandler<ReceivedTurnEventArgs> ReceivedTurn;
        public class ReceivedTurnEventArgs : EventArgs
        {
            public Position Turn { private set; get; }

            public ReceivedTurnEventArgs(char X, char Y)
            {
                if (!char.IsDigit(X) || !char.IsDigit(Y))
                    throw new Exception("Получена неверная информация о ходе соперника");

                Turn = new Position(int.Parse(X.ToString()), int.Parse(Y.ToString()));
            }
        }


        // Connected => Created / Off
        public void ExitGame()
        {
            throw new NotImplementedException();
        }

        /*
        // (Only Client)
        public void BreakConnection()
        {
            if (state >= State.Connected)
                throw new Exception("Невозможно прервать соединение, т.е. оно не было инициализировано, или уже выполнено. Для отключения используйте Disconnect / StopServer");

            
        }

        // Connected+ => Off
        public void Disconnect()
        {
            if (state < State.Connected)
                throw new Exception("Невозможно отключить игру, которая не была начата");
            Client?.Stop();
            Client = null;
            state = State.Off;
        }
        */

        // Null => Off :D
        public Connection2()
        {
            /*CheckItselfTimer.Tick += CheckItself;
            CheckServerReadyTimer.Tick += CheckServerReady;
            CheckClientEPTimer.Tick += CheckClientEP;*/
        }

        private void Send(string Data)
        {
            if (state < State.Connected)
                throw new Exception("Неверное состояние соединения");

            Client.Peer.Send(Encoding.UTF8.GetBytes(Data), SendOptions.ReliableOrdered);
        }
        public void SendIAM()
        {
            ServerLog?.Invoke(this, "Отправка данных IAM");
            string Data = "IAM" + '\n' + IAM.Nick + '\n' + IAM.Color.ToArgb() + '\n';
            Send(Data);
        }
        // Connected => WaitForStartFromAnother | WaitForStartFromMe => Game
        public void SendStartGame()
        {

            Send("GAM");

            if (state == State.Connected)
            {
                state = State.WaitForStartFromAnother;
            }
            else if (state == State.WaitForStartFromMe)
            {
                state = State.Game;
                GameStarts(this, Role == NetworkRole.Server);
            }
            else if (state != State.Game)
                throw new Exception("Если вылезла эта ошибка то стукните Тш и заставьте пофиксить");
        }
        public void SendTurn(Position Turn)
        {
            if (Turn.x < 0 || Turn.y < 0)
                Turn = new Position(9, 9);
            string Data = "TRN" + Turn.x.ToString() + Turn.y.ToString();
            Send(Data);
        }
        public void SendEndGame()
        {
            Send("END");
        }
        public void SendReject()
        {
            Send("RJC");
        }
        public void SendChat(string Text)
        {
            Send("CHT" + Text.Substring(0, Math.Min(256, Text.Length)));
        }

        // Connecting => Establishing => Connected / Created (сервер)
        private void CheckClientEP(object sender, EventArgs e)
        {
            if (state != State.Connecting)
                throw new Exception("Неверное состояние соединения");

            RemoteEP = ServerList.ReadClientEP(AccessKey);
            if (RemoteEP != null)
            {
                ServerLog?.Invoke(this, "Получен адрес клиента");
                //CheckClientEPTimer.Stop();
                state = State.Establishing;
                var con = TryToConnect(LocalEP, RemoteEP);
                ServerList.Clear(AccessKey);
                if (con != null)
                {
                    state = State.Connected;
                    ServerLog?.Invoke(this, "Ожидание данных IAM от клиента");
                }
                else
                {
                    state = State.Created;
                    ServerList.Clear(AccessKey);
                    CheckItselfTask = Task.Run((Action)CheckItself);
                }
            }
        }
        private void CheckClientEP()
        {
            if (state != State.Connecting)
                throw new Exception("Неверное состояние соединения");

            while (true)
            {
                RemoteEP = ServerList.ReadClientEP(AccessKey);
                if (RemoteEP != null)
                {
                    ServerLog?.Invoke(this, "Получен адрес клиента");
                    state = State.Establishing;
                    var con = TryToConnect(LocalEP, RemoteEP);
                    ServerList.Clear(AccessKey);
                    if (con != null)
                    {
                        state = State.Connected;
                        ServerLog?.Invoke(this, "Ожидание данных IAM от клиента");
                    }
                    else
                    {
                        state = State.Created;
                        ServerList.Clear(AccessKey);
                        CheckItselfTask = Task.Run((Action)CheckItself);
                    }

                    break;
                }
            }
        }

        // Preconnecting => Connecting => Establishing => Connected / Off (клиент)
        /*private void CheckServerReady(object sender, EventArgs e)
        {
            if (state != State.PreConnecting)
                throw new Exception("Неверное состояние соединения");

            // Таймаут подключения
            if (WaitServerResponseTimeout++ > 6)
            {
                CheckServerReadyTimer.Stop();
                state = State.Off;
                ServerIsntReady(this, new EventArgs());
            }

            RemoteEP = ServerList.ReadReady(PublicKey);
            if (RemoteEP != null)
            {
                CheckServerReadyTimer.Stop();
                state = State.Establishing;
                var EPs = GetEndPoints();
                if (EPs == null)
                {
                    // Не удалось подключиться к STUN
                    return;
                }
                LocalEP = EPs.Item1;
                PublicEP = EPs.Item2;
                ServerList.WriteClientEP(PublicKey, PublicEP);
                Thread.Sleep(100);
                var con = TryToConnect(LocalEP, RemoteEP); // Establishing
                if (con != null)
                {
                    state = State.Connected;
                    SendIAM();
                }
                else
                {
                    state = State.Off;
                }
            }
        }*/
        private void CheckServerReady()
        {
            while (true)
            {
                Thread.Sleep(1000);

                if (state != State.PreConnecting)
                    throw new Exception("Неверное состояние соединения");

                // Таймаут подключения
                if (WaitServerResponseTimeout++ > 6)
                {
                    state = State.Off;
                    ServerIsntReady(this, new EventArgs());
                    break;
                }

                RemoteEP = ServerList.ReadReady(PublicKey);
                if (RemoteEP != null)
                {
                    state = State.Establishing;
                    var EPs = GetEndPoints();
                    if (EPs == null)
                    {
                        // Не удалось подключиться к STUN
                        ConnectingRejected(this, new EventArgs());
                        return;
                    }
                    LocalEP = EPs.Item1;
                    PublicEP = EPs.Item2;
                    ServerList.WriteClientEP(PublicKey, PublicEP);
                    Thread.Sleep(100);
                    var con = TryToConnect(LocalEP, RemoteEP); // Establishing
                    if (con != null)
                    {
                        state = State.Connected;
                        SendIAM();
                    }
                    else
                    {
                        ConnectingRejected(this, new EventArgs());
                        state = State.Off;
                    }

                    break;
                }
            }
        }

        // Обработка приходящий данных
        private void Listener_NetworkReceiveEvent(NetPeer peer, LiteNetLib.Utils.NetDataReader reader)
        {
            if (reader.AvailableBytes == 0)
                return;

            // 508 байт (RFC 791)
            var Data = Encoding.UTF8.GetString(reader.Data);
            
            switch (Data.Substring(0, 3))
            {
                case "TRN": // Ход
                    ReceivedTurn?.Invoke(this, new ReceivedTurnEventArgs(Data[3], Data[4]));
                    break;
                case "CHT": // Сообщение в чате
                    ReceivedChat?.Invoke(this, Data.Substring(3));
                    break;
                case "IAM": // Представление
                    var OpponentIAM = new IAMEventArgs(Data);
                    if (OpponentIAM.Nick == IAM.Nick)
                    {
                        ServerLog?.Invoke(this, "Подключившийся клиент имел такой же ник и был отклонён");
                        RejectOpponent();
                        return;
                    }
                    if (OpponentIAM.Color.DifferenceWith(IAM.Color) < 50)
                    {
                        ServerLog?.Invoke(this, "Подключившийся клиент выбрал похожий цвет и был отклонён");
                        RejectOpponent();
                        return;                        
                    }
                    OpponentConnected.Invoke(this, OpponentIAM);
                    if (Role == NetworkRole.Server)
                        SendIAM();
                    break;
                case "GAM": // Game - начало игры
                    if (state == State.Connected)
                        state = State.WaitForStartFromMe;
                    else if (state == State.WaitForStartFromAnother)
                    {
                        state = State.Game;
                        GameStarts(this, Role == NetworkRole.Server);
                    }
                    else if (state == State.Game)
                        RestartGame(this, new EventArgs());
                    break;
                case "END": // Противник прервал игру
                    GameEnds?.Invoke(this, new EventArgs());
                    BreakAnyConnection();
                    break;
                case "RJC":
                    if (state == State.Game)
                        RestartRejected.Invoke(this, new EventArgs());
                    else
                    {
                        state = State.Connected;
                        ConnectingRejected.Invoke(this, new EventArgs());
                        BreakAnyConnection();
                    }
                    break;
                default:
                    throw new Exception("Неверный заголовой данных: " + Data.Substring(0, 3));
            }
        }

        // Created => Connecting (сервер)
        private void CheckItself()//(object sender, EventArgs e)
        {
            if (state != State.Created)
                throw new Exception("Неверное состояние соединения");

            while (true)
            {
                Thread.Sleep(3000);

                if (ServerList.CheckWhoWant(AccessKey))
                {
                    ServerLog?.Invoke(this, "Обнаружен запрос на подключение");
                    var EPs = GetEndPoints();
                    if (EPs == null)
                    {
                        // Не удалось подключиться к STUN
                        return;
                    }
                    LocalEP = EPs.Item1;
                    PublicEP = EPs.Item2;
                    if (ServerList.WriteReady(AccessKey, PublicEP.Port))
                    {
                        ServerLog?.Invoke(this, "Отправлен ответ на запрос");
                        CheckClientEPTask = Task.Run((Action)CheckClientEP);
                        state = State.Connecting;

                        break;
                    }
                }
            }
        }

        // Establishing => Connection
        private Tuple<NetClient, EventBasedNetListener> TryToConnect(IPEndPoint LocalEP, IPEndPoint RemoteEP)
        {
            ServerLog?.Invoke(this, "Соединение со вторым игроком..");
            //EventBasedNetListener Listener = new EventBasedNetListener();
            Listener = new EventBasedNetListener();
            Client = new NetClient(Listener, "tttp2pcon");
            Listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            Client.PeerToPeerMode = true;
            try
            {
                Client.Start(LocalEP.Port);
                Client.Connect(RemoteEP.Address.ToString(), RemoteEP.Port);
                Client.PollEvents();
                while (Client.IsRunning && !Client.IsConnected)
                    Thread.Sleep(50);
                if (Client.IsConnected)
                {
                    Task.Run(() =>
                    {
                        while (Client.IsConnected)
                        {
                            Client.PollEvents();
                            Thread.Sleep(100);
                        }
                        if (state != State.Off)
                            OpponentDisconnected(this, new EventArgs());
                    });
                    ServerLog?.Invoke(this, "Соединение выполнено");
                    return Tuple.Create(Client, Listener);
                }
                else
                {
                    ServerLog?.Invoke(this, "Не удалось соединиться");
                    return null;
                }
            }
            catch (Exception)
            {
                Client.Stop();
                throw;
            }
            //return null;
        }

        // Получение своего внешнего айпи и порта
        private Tuple<IPEndPoint,IPEndPoint> GetEndPoints()
        {
            ServerLog?.Invoke(this, "Получение Public IPEndPoint с помощью STUN сервера..");
            IPEndPoint RemoteEP = null;
            IPEndPoint LocalEP = new IPEndPoint(IPAddress.Any, 15678 + DateTime.Now.Second + DateTime.Now.Minute * 60);
            using (StringReader sr = new StringReader(Properties.Resources.Stun_servers))
            {
                ServerLog?.Invoke(this, "- LocalEP = " + LocalEP.ToString());
                while (RemoteEP == null)
                {
                    string str = sr.ReadLine();
                    if (str == null)
                    {
                        ServerLog?.Invoke(this, "Нет доступных STUN-серверов. Сервер будет выключен.");
                        BreakAnyConnection();
                        return null;
                    }
                    string[] parts = str.Split(':');
                    string site = parts[0];
                    int port = 3478;
                    if (parts.Length == 2)
                        port = int.Parse(parts[1]);
                    try
                    {
                        ServerLog?.Invoke(this, "- STUN \"" + str + "\" - попытка подключения");
                        RemoteEP = LumiSoft_edited.STUN_Client.GetPublicEP(site, port, LocalEP);
                    }
                    catch { }
                }
                ServerLog?.Invoke(this, "PublicEP получен: " + RemoteEP);
            }
            return Tuple.Create(LocalEP, RemoteEP);
        }

        // Off => Created (сервер)
        public bool StartServer(string ServerName, IAMData IAM)
        {
            // Проверка исходного состояния
            if (state != State.Off)
                throw new Exception("Невозможно создать сервер при состоянии соединения отличном от OFF");

            this.IAM = IAM;
            ExportLNLDLL();

            // Регистрация сервера
            AccessKey = ServerList.RegisterOnTheWeb(IAM.Nick, ServerName, IAM.Color);
            if (AccessKey == "")
            {
                ServerLog?.Invoke(this, "Не удалось создать сервер. Проверьте подключение к интернету и настройки центрального сервера");
                return false;
            }
            else
            {
                CheckItselfTask = Task.Run((Action)CheckItself);
                state = State.Created;
                Role = NetworkRole.Server;
                ServerLog?.Invoke(this, "Сервер создан и зарегистрирован");
                return true;
            }
        }

        /*
        // Created => Off (сервер)
        public void StopServer()
        {
            // Проверка исходного состояния
            if (state != State.Created && state != State.Connecting && state != State.Establishing)
                throw new Exception("Остановка сервера в некорректном состоянии");

            // Удаление сервера из списка
            ServerList.RemoveFromTheWeb(AccessKey);
            ServerLog?.Invoke(this, "Сервер остановлен и удалён из списка");
            state = State.Off;
            Role = NetworkRole.Nope;                
        }
        */

        // Off => PreConnecting (клиент)
        public bool ConnectTo(string publicKey, IAMData IAM)
        {
            // Проверка исходного состояния
            if (state != State.Off)
                return false;

            this.IAM = IAM;
            ExportLNLDLL();

            // Отправка "желания подключиться" :D
            if (ServerList.WantToConnect(publicKey))
            {
                //CheckServerReadyTimer.Start();
                CheckServerReadyTask = Task.Run((Action)CheckServerReady);
                PublicKey = publicKey;
                state = State.PreConnecting;
                Role = NetworkRole.Client;
                return true;
            }
            else
            {
                ServerLog?.Invoke(this, "Не удалось подключиться отправить запрос на подключение к серверу.");
                // Если: 1) нет инета; 2) нет подключения к мастер-серверу 3) сервер отсутствует в базе данных (устарел?)
                return false;
            }
        }

        // Connected / WaitForStartFromMe => Created
        private void RejectOpponent()
        {
            if (state != State.Connected && state != State.WaitForStartFromMe)
                throw new Exception("Нельзя отлонить игру, если она не была предложена");
            SendReject();
            if (Role == NetworkRole.Server)
            {
                ServerList.Clear(AccessKey);
                state = State.Created;
                CheckItselfTask = Task.Run((Action)CheckItself);
            }
            else
            {
                BreakAnyConnection();
            }
        }

        // * => Off
        public void BreakAnyConnection()
        {
            switch (state)
            {
                case State.Off:
                    // Nothing o.o
                    break;
                case State.Created: // Only Server
                    ServerList.RemoveFromTheWeb(AccessKey);
                    ServerLog?.Invoke(this, "Сервер остановлен и удалён из списка");
                    break;
                case State.PreConnecting: // Only Client

                    break;
                case State.Connecting:

                    break;
                case State.Establishing:
                    break;
                case State.Connected:
                    SendReject();
                    break;
                case State.WaitForStartFromAnother:
                    SendReject();
                    break;
                case State.WaitForStartFromMe:
                    SendReject();
                    break;
                case State.Game:
                    SendEndGame();
                    break;
                default:
                    throw new NotImplementedException();
            }

            PublicKey = "";
            AccessKey = "";
            Role = NetworkRole.Nope;
            state = State.Off;
            //      Stop all   !!!!!!
            /*CheckClientEPTimer.Stop();
            CheckServerReadyTimer.Stop();
            CheckItselfTimer.Stop();*/
            Client?.Stop();
            if (Listener != null)
                Listener.NetworkReceiveEvent -= Listener_NetworkReceiveEvent;
        }

        public void ExportLNLDLL()
        {
            if (!File.Exists("LiteNetLib.dll"))
                File.WriteAllBytes("LiteNetLib.dll", Properties.Resources.LiteNetLib);
        }
    }

    /*
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

        // Прослушивание (для потока)
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
        private int? Port;

        // События
        public event EventHandler AnotherPlayerConnected; // Серверное
        public event EventHandler AnotherPlayerDisconnected; // Серверное
        public event EventHandler GameStarts;
        public event EventHandler GameEnds;
        
        public event EventHandler RestartGame;
        public event EventHandler RestartRejected;

        public event EventHandler<string> ReceivedChat;
        public event EventHandler<ReceivedTurnEventArgs> ReceivedTurn;
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

        public class ReceivedTurnEventArgs : EventArgs
        {
            public Position Turn { private set; get; }

            public ReceivedTurnEventArgs(char X, char Y)
            {
                if (!char.IsDigit(X) || !char.IsDigit(Y))
                    throw new Exception("Получена неверная информация о ходе соперника");

                Turn = new Position(int.Parse(X.ToString()), int.Parse(Y.ToString()));
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

        public bool TryToClosePort(int Port)
        {
            try
            {
                var upnpnat = new NATUPNPLib.UPnPNAT();
                var mappings = upnpnat.StaticPortMappingCollection;
                if (mappings == null)
                    return false;
                mappings.Remove(Port, "TCP");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryToOpenPort(int Port)
        {
            try
            {
                var upnpnat = new NATUPNPLib.UPnPNAT();
                var mappings = upnpnat.StaticPortMappingCollection;
                if (mappings == null)
                    return false;
                mappings.Add(Port, "TCP", Port, GetLocalAdress().ToString(), true, "Quantum0's Tic Tac Toe Server");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static IPAddress GetLocalAdress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {

                IPInterfaceProperties properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)//вся магия вот в этой строке
                    continue;

                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {

                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    return address.Address;
                }
            }
            return default(IPAddress);
        }

        private void LLCon_ReceivedData(object sender, LowLevelConnection.ReceivedDataEventArgs e)
        {
            string Data = e.Data;

            switch (Data.Substring(0, 3))
            {
                case "TRN": // Ход
                    ReceivedTurn?.Invoke(this, new ReceivedTurnEventArgs(Data[3], Data[4])); // заменить null на ход
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
                    else if (state == State.Game)
                        RestartGame(this, new EventArgs());
                    break;
                case "END": // Противник прервал игру
                    GameEnds?.Invoke(this, new EventArgs());
                    break;
                case "RJC":
                    if (state == State.Game)
                        RestartRejected.Invoke(this, new EventArgs());
                    else
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
            if (Turn.x < 0 || Turn.y < 0)
                Turn = new Position(9, 9);
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
            if (Port.HasValue)
            {
                TryToClosePort(Port.Value);
                Port = null;
            }
            LLCon.Disconnect();
            state = (State)(byte)LLCon.state;
        }
        public void StartServerListening(int Port)
        {
            try
            {
                TryToOpenPort(Port);
                LLCon.StartServerListening(Port);
                state = State.Listening;
                Host = true;
                this.Port = Port;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
    */
}
