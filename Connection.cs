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
using Tic_Tac_Toe_WPF_Remake;

namespace TTTM
{
    public class Connection
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

        public static Connection Current { get; private set; }
        public static void CreateConnection()
        {
            Current = new Connection(Settings.Current);
        }
        
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
        Settings settings;
        private bool Stop_CheckItself;
        private bool Stop_CheckServerReady;
        private bool Stop_CheckClientEP;

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
        
        // Null => Off :D
        public Connection(Settings settings)
        {
            this.settings = settings;
        }

        private void Send(string Data)
        {
            if (state < State.Connected)
                throw new Exception("Неверное состояние соединения");

            Client.Peer?.Send(Encoding.UTF8.GetBytes(Data), SendOptions.ReliableOrdered);
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
        public void SendIAM()
        {
            ServerLog?.Invoke(this, "Отправка данных IAM");
            string Data = "IAM" + '\n' + IAM.Nick + '\n' + IAM.Color.ToArgb() + '\n';
            Send(Data);
        }

        // Connecting => Establishing => Connected / Created (сервер)
        private void CheckClientEP()
        {
            if (state != State.Connecting)
                throw new Exception("Неверное состояние соединения");

            var TryCount = 0;
            while (TryCount++ < 100 && !Stop_CheckClientEP)
            {
                Thread.Sleep(100);
                RemoteEP = ServerList.ReadClientEP(AccessKey);
                if (RemoteEP != null)
                {
                    ServerLog?.Invoke(this, "Получен адрес клиента");
                    state = State.Establishing;
                    var con = TryToConnect(LocalEP, RemoteEP); // state = State.Connected;
                    ServerList.Clear(AccessKey);
                    if (con != null)
                    {
                        ServerLog?.Invoke(this, "Ожидание данных IAM от клиента");
                    }
                    else
                    {
                        state = State.Created;
                        ServerList.Clear(AccessKey);
                        CheckItselfTask = Task.Run((Action)CheckItself);
                    }

                    return;
                }
            }

            if (!Stop_CheckClientEP)
            {
                ServerLog?.Invoke(this, "Истекло время ожидание ответа от клиента");
                state = State.Created;
                ServerList.Clear(AccessKey);
                CheckItselfTask = Task.Run((Action)CheckItself);
            }
            else
                Stop_CheckClientEP = false;
        }

        // Preconnecting => Connecting => Establishing => Connected / Off (клиент)
        private void CheckServerReady()
        {
            WaitServerResponseTimeout = 0;
            while (!Stop_CheckServerReady)
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
                        Thread.Sleep(100);
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
            Stop_CheckServerReady = false;
        }

        // Обработка приходящий данных
        private void Listener_NetworkReceiveEvent(NetPeer peer, LiteNetLib.Utils.NetDataReader reader)
        {
            if (reader.AvailableBytes == 0)
                return;
            
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
                        // state = State.Connected;
                        ConnectingRejected.Invoke(this, new EventArgs());
                        if (Role == NetworkRole.Client)
                            BreakAnyConnection();
                    }
                    break;
                default:
                    throw new Exception("Неверный заголовой данных: " + Data.Substring(0, 3));
            }
        }

        // Created => Connecting (сервер)
        private void CheckItself()
        {
            if (state != State.Created)
                throw new Exception("Неверное состояние соединения");

            while (!Stop_CheckItself)
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
            Stop_CheckItself = false;
        }

        // Establishing => Connection
        private Tuple<NetClient, EventBasedNetListener> TryToConnect(IPEndPoint LocalEP, IPEndPoint RemoteEP)
        {
            ServerLog?.Invoke(this, "Соединение со вторым игроком..");
            Listener = new EventBasedNetListener();
            Client = new NetClient(Listener, "tttp2pcon");
            Listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            Client.PeerToPeerMode = true;
            try
            {
                Client.Start(LocalEP.Port);
                Client.Connect(RemoteEP.Address.ToString(), RemoteEP.Port);
                Client.PollEvents();
                while(Client.IsRunning && !Client.IsConnected)
                    Thread.Sleep(50);
                if (Client.IsConnected)
                {
                    state = State.Connected;
                    Task.Run(() =>
                    {
                        while (Client.IsConnected)
                        {
                            Client.PollEvents();
                            Thread.Sleep(100);
                        }
                        if (state != State.Off)
                            OpponentDisconnected?.Invoke(this, new EventArgs());
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
            catch (Exception e)
            {
                Client.Stop();
                throw new Exception("Ошибка инициализации клиента. Возможно порт уже используется.", e);
            }
        }

        // Получение своего внешнего айпи и порта
        private Tuple<IPEndPoint,IPEndPoint> GetEndPoints()
        {
            ServerLog?.Invoke(this, "Получение Public IPEndPoint с помощью STUN сервера..");
            IPEndPoint RemoteEP = null;
            IPEndPoint LocalEP = new IPEndPoint(IPAddress.Any, settings.MpPort == 0 ? 15678 + DateTime.Now.Second + DateTime.Now.Minute * 60 : settings.MpPort);
            using (StringReader sr = new StringReader(Tic_Tac_Toe_WPF_Remake.Properties.Resources.Stun_servers))
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
                state = State.Created;
                CheckItselfTask = Task.Run((Action)CheckItself);
                Role = NetworkRole.Server;
                ServerLog?.Invoke(this, "Сервер создан и зарегистрирован");
                return true;
            }
        }

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
        public void RejectOpponent()
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
            Stop_CheckClientEP = true;
            Stop_CheckServerReady = true;
            Stop_CheckItself = true;
            Thread.Sleep(5000);

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
            Client?.Stop();
            if (Listener != null)
                Listener.NetworkReceiveEvent -= Listener_NetworkReceiveEvent;
            
            Stop_CheckClientEP = false;
            Stop_CheckServerReady = false;
            Stop_CheckItself = false;
        }

        public void DisconnetClientFromServerAndContinueListening()
        {
            state = State.Created;
            Client?.Stop();
            ServerList.Clear(AccessKey);
            CheckItselfTask = Task.Run((Action)CheckItself);
            Role = NetworkRole.Server;
        }

        // Экспорт DLL-ки для р2р подключения
        public void ExportLNLDLL()
        {
            if (!File.Exists("LiteNetLib.dll"))
                File.WriteAllBytes("LiteNetLib.dll", Tic_Tac_Toe_WPF_Remake.Properties.Resources.LiteNetLib);
        }
    }
}
