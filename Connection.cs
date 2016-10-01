﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LiteNetLib;
using System.Timers;
//using System.Threading;

namespace TTTM
{
    class Connection2
        // mb static ?
    {
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
         * Server & Client |Connected, Connected|
         * 
         * 
         * 
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
        IAMData IAM;
        EventBasedNetListener Listener;
        System.Windows.Forms.Timer CheckItselfTimer = new System.Windows.Forms.Timer() { Interval = 5000 };
        System.Windows.Forms.Timer CheckServerReadyTimer = new System.Windows.Forms.Timer() { Interval = 2000 };
        System.Windows.Forms.Timer CheckClientEPTimer = new System.Windows.Forms.Timer() { Interval = 1000 };


        public event EventHandler OpponentConnected; // сделать с WhoIs

        // Null => Off :D
        public Connection2()
        {
            CheckItselfTimer.Tick += CheckItself;
            CheckServerReadyTimer.Tick += CheckServerReady;
            CheckClientEPTimer.Tick += CheckClientEP;
        }

        private void Send(string Data)
        {
            if (state != State.Connected)
                throw new Exception("Неверное состояние соединения");

            Client.Peer.Send(Encoding.UTF8.GetBytes(Data), SendOptions.ReliableOrdered);
        }
        public void SendIAM()
        {
            string Data = "IAM" + '\n' + IAM.Nick + '\n' + IAM.Color.ToArgb() + '\n';
            Send(Data);
        }

        // Connecting => Establishing => Connected / Created
        private void CheckClientEP(object sender, EventArgs e)
        {
            if (state != State.Connecting)
                throw new Exception("Неверное состояние соединения");

            RemoteEP = ServerList.ReadClientEP(AccessKey);
            if (RemoteEP != null)
            {
                CheckClientEPTimer.Stop();
                state = State.Establishing;
                var con = TryToConnect(LocalEP, RemoteEP);
                ServerList.Clear(AccessKey);
                if (con != null)
                {
                    Client = con.Item1;
                    Listener = con.Item2;
                    Listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
                    state = State.Connected;
                    Role = NetworkRole.Server;
                }
                else
                {
                    state = State.Created;
                }
            }
        }

        // Preconnecting => Connecting => Establishing => Connected / Off
        private void CheckServerReady(object sender, EventArgs e)
        {
            if (state != State.PreConnecting)
                throw new Exception("Неверное состояние соединения");

            RemoteEP = ServerList.ReadReady(PublicKey);
            if (RemoteEP != null)
            {
                CheckServerReadyTimer.Stop();
                state = State.Establishing;
                var EPs = GetEndPoints();
                LocalEP = EPs.Item1;
                PublicEP = EPs.Item2;
                ServerList.WriteClientEP(PublicKey, PublicEP);
                Thread.Sleep(1000);
                var con = TryToConnect(LocalEP, RemoteEP);
                if (con != null)
                {
                    Client = con.Item1;
                    Listener = con.Item2;
                    Listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
                    state = State.Connected;
                    Role = NetworkRole.Client;
                    SendIAM();
                }
                else
                {
                    state = State.Off;
                }
            }
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, LiteNetLib.Utils.NetDataReader reader)
        {
            if (reader.AvailableBytes == 0)
                return;

            // 508 байт (RFC 791)
            var Data = Encoding.UTF8.GetString(reader.Data);
            
            switch (Data.Substring(0, 3))
            {
                case "TRN": // Ход
                    //ReceivedTurn?.Invoke(this, new ReceivedTurnEventArgs(Data[3], Data[4])); // заменить null на ход
                    break;
                case "CHT": // Сообщение в чате
                    //ReceivedChat?.Invoke(this, Data.Substring(3)); // заменить
                    break;
                case "IAM": // Представление
                    OpponentConnected.Invoke(this, new IAMEventArgs(Data));
                    if (Role == NetworkRole.Server)
                        SendIAM();
                    break;
                case "GAM": // Game - начало игры
                    if (state == State.Connected)
                        state = State.WaitForStartFromMe;
                    else if (state == State.WaitForStartFromAnother)
                    {
                        state = State.Game;
                        //GameStarts(this, new EventArgs());
                    }
                    //else if (state == State.Game)
                        //RestartGame(this, new EventArgs());
                    break;
                case "END": // Противник прервал игру
                    //GameEnds?.Invoke(this, new EventArgs());
                    break;
                case "RJC":
                    /*if (state == State.Game)
                        RestartRejected.Invoke(this, new EventArgs());
                    else
                        ConnectingRejected.Invoke(this, new EventArgs());*/
                    break;
                default:
                    throw new Exception("Неверный заголовой данных: " + Data.Substring(0, 3));
            }
        }

        // Created => Connecting
        private void CheckItself(object sender, EventArgs e)
        {
            if (state != State.Created)
                throw new Exception("Неверное состояние соединения");

            if (ServerList.CheckWhoWant(AccessKey))
            {
                var EPs = GetEndPoints();
                LocalEP = EPs.Item1;
                PublicEP = EPs.Item2;
                if (ServerList.WriteReady(AccessKey, PublicEP.Port))
                {
                    CheckItselfTimer.Stop();
                    CheckClientEPTimer.Start();
                    state = State.Connecting;
                }
            }
        }

        // Establishing => Connection
        private Tuple<NetClient, EventBasedNetListener> TryToConnect(IPEndPoint LocalEP, IPEndPoint RemoteEP)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            NetClient client = new NetClient(listener, "tttp2pcon");
            client.PeerToPeerMode = true;
            try
            {
                client.Start(LocalEP.Port);
                client.Connect(RemoteEP.Address.ToString(), RemoteEP.Port);
                client.PollEvents();
                while (client.IsRunning && !client.IsConnected)
                    Thread.Sleep(250);
                if (client.IsConnected)
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        while (client.IsConnected)
                        {
                            client.PollEvents();
                            Thread.Sleep(500);
                        }
                    });
                    return Tuple.Create(client, listener);
                }
            }
            catch (Exception)
            {
                client.Stop();
                throw;
            }
            return null;
        }

        // Получение своего внешнего айпи и порта
        private Tuple<IPEndPoint,IPEndPoint> GetEndPoints()
        {
            IPEndPoint RemoteEP = null;
            IPEndPoint LocalEP = new IPEndPoint(IPAddress.Any, 15678 + DateTime.Now.Second + DateTime.Now.Minute * 60);
            using (StringReader sr = new StringReader(Properties.Resources.Stun_servers))
            {
                while (RemoteEP == null)
                {
                    string str = sr.ReadLine();
                    if (str == null)
                        throw new Exception("Not found any working STUN server");
                    string[] parts = str.Split(':');
                    string site = parts[0];
                    int port = 3478;
                    if (parts.Length == 2)
                        port = int.Parse(parts[1]);
                    try
                    {
                        RemoteEP = LumiSoft_edited.STUN_Client.GetPublicEP(site, port, LocalEP);
                    }
                    catch { }
                }
            }
            return Tuple.Create(LocalEP, RemoteEP);
        }

        // Off => Created
        public bool StartServer(string ServerName, IAMData IAM)
        {
            // Проверка исходного состояния
            if (state != State.Off)
                return false;

            this.IAM = IAM;

            // Регистрация сервера
            AccessKey = ServerList.RegisterOnTheWeb(IAM.Nick, ServerName, IAM.Color);
            if (AccessKey == "")
                return false;
            else
            {
                CheckItselfTimer.Start();
                state = State.Created;
                return true;
            }
        }

        // Created => Off
        public void StopServer()
        {
            // Проверка исходного состояния
            if (state != State.Created)
                return;

            // Удаление сервера из списка
            ServerList.RemoveFromTheWeb(AccessKey);
        }

        // Off => PreConnecting
        public bool ConnectTo(string publicKey, IAMData IAM)
        {
            // Проверка исходного состояния
            if (state != State.Off)
                return false;

            this.IAM = IAM;

            // Отправка "желания подключиться" :D
            if (ServerList.WantToConnect(publicKey))
            {
                CheckServerReadyTimer.Start();
                PublicKey = publicKey;
                state = State.PreConnecting;
                return true;
            }
            return false;
        }
    }



    /*class LowLevelConnectionStarter
    {
        private int Id; //My unique code
        private byte[] buffer = new byte[0x10000]; // Reveived Data
    }

    class LowLevelConnection2
    {
        
    }*/

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
}
