using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTTM
{
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
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
