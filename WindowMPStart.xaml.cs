using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TTTM;

namespace Tic_Tac_Toe_WPF_Remake
{
    /// <summary>
    /// Логика взаимодействия для WindowMPStart.xaml
    /// </summary>
    public partial class WindowMPStart : Window
    {
        const bool ShowServerLog = true;

        public WindowMPStart()
        {
            InitializeComponent();
            Connection.CreateConnection();
            textBoxNick.Text = Settings.Current.DefaultName1;
            CheckCorrectNick();
            RectColor.SetShapeColor(Settings.Current.PlayerColor1);
            Connection.Current.OpponentConnected += Connection_OpponentConnected;
            Connection.Current.ServerIsntReady += Connection_ServerIsntReady;
            Connection.Current.ConnectingRejected += Connection_ConnectingRejected;
        }

        private void Connection_ConnectingRejected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                if (Connection.Current.state == Connection.State.Connected ||
                    Connection.Current.state == Connection.State.WaitForStartFromMe ||
                    Connection.Current.state == Connection.State.WaitForStartFromAnother)
                {
                    System.Windows.Forms.MessageBox.Show("Противник отклонил игру");
                    if (Connection.Current.Role == Connection.NetworkRole.Server)
                    {
                        Connection.Current.OpponentDisconnected -= Current_OpponentDisconnected;
                        Connection.Current.DisconnetClientFromServerAndContinueListening();
                        CreateServerGrid.Visibility = Visibility.Visible;
                        ConnectedGrid.Visibility = Visibility.Hidden;
                    }
                    else if (Connection.Current.Role == Connection.NetworkRole.Client)
                    {
                        Connection.Current.OpponentDisconnected -= Current_OpponentDisconnected;
                        ConnectedGrid.Visibility = Visibility.Hidden;
                        ServerListGrid.Visibility = Visibility.Visible;
                    }
                    else throw new ArgumentException("Некорректно указана роль после отключения от игры");
                }
                else
                {
                    buttonConnect.IsEnabled = true;
                    System.Windows.Forms.MessageBox.Show("Не удалось подключиться к серверу");
                }
            });
        }

        private void Connection_ServerIsntReady(object sender, EventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                buttonConnect.IsEnabled = true;
                MessageBox.Show("Сервер не отвечает", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        private void Connection_OpponentConnected(object sender, Connection.IAMEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                labelPlayer2.Content = e.Nick;
                labelPlayer1.Content = Connection.Current.IAM.Nick;
                RectColor2.SetShapeColor(e.Color);
                RectColor1.SetShapeColor(Connection.Current.IAM.Color);
                buttonStartGame.IsEnabled = true;
                ServerListGrid.Visibility = Visibility.Hidden;
                CreateServerGrid.Visibility = Visibility.Hidden;
                ConnectedGrid.Visibility = Visibility.Visible;
                Connection.Current.GameStarts += Connection_GameStarts;
                Connection.Current.OpponentDisconnected += Current_OpponentDisconnected;
            });
        }

        private void Current_OpponentDisconnected(object sender, EventArgs e)
        {
            Connection.Current.OpponentDisconnected -= Current_OpponentDisconnected;
            if (Connection.Current.state == Connection.State.Connected ||
                    Connection.Current.state == Connection.State.WaitForStartFromMe ||
                    Connection.Current.state == Connection.State.WaitForStartFromAnother)
            {
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Соединение было разорвано");
            }
        }

        #region Настройки

        // textBoxNick Processing
        private bool CheckCorrectNick()
        {
            if (string.IsNullOrWhiteSpace(textBoxNick.Text))
            {
                textBoxNick.Text = "Пожалуйста, укажите Ваш ник";
                textBoxNick.Foreground = new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 });
                return false;
            }
            else
                textBoxNick.Foreground = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 0 });
            return true;
        }
        private void textBoxNick_GotFocus(object sender, RoutedEventArgs e)
        {
            textBoxNick.Foreground = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 0 });
            if (textBoxNick.Text == "Пожалуйста, укажите Ваш ник" || textBoxNick.Text == "Ник не должен содержать запрещённые символы")
            {
                textBoxNick.Text = "";
            }
        }
        private void textBoxNick_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                CheckCorrectNick();
            }
        }
        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            (new WindowSettings()).ShowDialog();
        }

        private void SelectColor_Click(object sender, MouseButtonEventArgs e)
        {
            var c = RectColor.SelectColor();
            RectColor.SetShapeColor(c);
        }
        #endregion
        #region Список серверов
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Task.Run((Action)RefreshServerList);
        }
        private void RefreshServerList()
        {
            this.Dispatcher.Invoke(delegate
            {
                buttonRefresh.IsEnabled = false;
                buttonRefresh.Content = "Обновление...";
            });

            var Servers = ServerList.GetServers();

            this.Dispatcher.Invoke( delegate
            {
                dataGrid.ItemsSource = Servers;
                buttonRefresh.IsEnabled = true;
                buttonRefresh.Content = "Обновить";
                buttonConnect.IsEnabled = (Connection.Current.state == Connection.State.Off) && (Servers.Count > 0);
            });
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItems.Count != 1)
                return;

            var server = (ServerList.ServerRecord)dataGrid.SelectedItem;

            if (textBoxNick.Text == server.Name)
            {
                MessageBox.Show("Выберите другой никнейм", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (System.Drawing.Color.FromArgb(int.Parse(server.Color)).DifferenceWith(RectColor.GetShapeColor()) < 50)
            {
                MessageBox.Show("Ваш цвет не должен быть похож на цвет оппонента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            ConnectToServer(server.PublicKey);
            labelServerName.Content = server.ServerName;
        }
        #endregion

        #region Создание сервера
        private void textBoxServername_GotFocus(object sender, RoutedEventArgs e)
        {
            textBoxNick.Foreground = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 0 });
            if (textBoxNick.Text == "Укажите название сервера" || textBoxNick.Text == "Название сервера не должно содержать запрещённые символы")
            {
                textBoxNick.Text = "";
            }
        }
        private void textBoxServername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxServerName.Text))
            {
                textBoxServerName.Text = "Укажите название сервера";
                textBoxServerName.Foreground = new SolidColorBrush(new Color() { A = 255, R = 255, G = 0, B = 0 });
                return;
            }
        }
        private void textBoxServername_KeyUp(object sender, KeyEventArgs e)
        {
            buttonStartServer.IsEnabled = (!string.IsNullOrWhiteSpace(textBoxServerName.Text));
        }
        private void buttonStartServer_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }
        private void buttonStopServer_Click(object sender, RoutedEventArgs e)
        {
            StopServer();
        }
        #endregion

        private void StartServer()
        {
            textBoxServerLog.Visibility = ShowServerLog ? Visibility.Visible : Visibility.Hidden;
            buttonStartServer.IsEnabled = false;
            textBoxServerName.IsReadOnly = true;
            buttonStopServer.Visibility = Visibility.Visible;
            labelServerName.Content = textBoxServerName.Text;
            Connection.Current.ServerLog += Connection_ServerLog;
            Connection.Current.StartServer(textBoxServerName.Text, new Connection.IAMData(textBoxNick.Text, RectColor.GetShapeColor()));
        }
        private void Connection_ServerLog(object sender, string e)
        {
            Dispatcher.Invoke(delegate
            {
                textBoxServerLog.Text += e + "\r\n";
            });
        }
        private void StopServer()
        {
            textBoxServerLog.Visibility = Visibility.Hidden;
            buttonStartServer.IsEnabled = true;
            textBoxServerName.IsReadOnly = false;
            buttonStopServer.Visibility = Visibility.Hidden;
            Connection.Current.BreakAnyConnection();
            Connection.Current.ServerLog -= Connection_ServerLog;
        }
        private void ConnectToServer(string PublicKey)
        {
            if (Connection.Current.ConnectTo(PublicKey, new Connection.IAMData(textBoxNick.Text, RectColor.GetShapeColor())))
                buttonConnect.IsEnabled = false;
        }
        private void Connection_GameStarts(object sender, bool e)
        {
            Dispatcher.Invoke(delegate
            {
                Connection.Current.GameStarts -= Connection_GameStarts;
                ConnectedGrid.Visibility = Visibility.Hidden;
                var GameForm = new WindowMultiplayer(labelPlayer1.Content.ToString(), labelPlayer2.Content.ToString(), RectColor1.GetShapeColor(), RectColor1.GetShapeColor());
                GameForm.Show();
                GameForm.Closed += (s, ee) => this.Visibility = Visibility.Visible;
                this.Visibility = Visibility.Hidden;
            });
        }

        private void buttonStartGame_Click(object sender, RoutedEventArgs e)
        {
            buttonStartGame.IsEnabled = false;
            Connection.Current.SendStartGame();
        }

        private void buttonDisconnectGame_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(delegate
            {
                ConnectedGrid.Visibility = Visibility.Hidden;
                if (Connection.Current.Role == Connection.NetworkRole.Client)
                {
                    ServerListGrid.Visibility = Visibility.Visible;
                }
                else if (Connection.Current.Role == Connection.NetworkRole.Server)
                {
                    CreateServerGrid.Visibility = Visibility.Visible;
                }
                else throw new ArgumentException("Некорректно указана роль при попытке отключения от игры");

                Connection.Current.RejectOpponent();
            });
        }

        private void buttonGotoCreateServer_Click(object sender, RoutedEventArgs e)
        {
            CreateServerGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            RectColor.Visibility = Visibility.Hidden;
        }

        private void buttonGotoServerList_Click(object sender, RoutedEventArgs e)
        {
            ServerListGrid.Visibility = Visibility.Visible;
            MainGrid.Visibility = Visibility.Hidden;
            RectColor.Visibility = Visibility.Hidden;
            RefreshServerList();
        }

        private void BucketColorSelecter_Click(object sender, RoutedEventArgs e)
        {
            SelectColor_Click(RectColor, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MainGrid.Visibility == Visibility.Hidden)
            {
                Connection.Current.BreakAnyConnection();
                ServerListGrid.Visibility = Visibility.Hidden;
                CreateServerGrid.Visibility = Visibility.Hidden;
                MainGrid.Visibility = Visibility.Visible;
                RectColor.Visibility = Visibility.Visible;
                e.Cancel = true;
            }
        }
    }
}
