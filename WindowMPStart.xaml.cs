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
        public WindowMPStart()
        {
            InitializeComponent();
            Connection.CreateConnection();
            textBoxNick.Text = Settings.Current.DefaultName1;
            CheckCorrectNick();
            RectColor.SetShapeColor(Settings.Current.PlayerColor1);
            //tabControl.TabPages.Remove(tabPageStartGame);
            //Connection.Current.OpponentConnected += Connection_OpponentConnected;
            //Connection.Current.ServerIsntReady += Connection_ServerIsntReady;
        }

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

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            (new WindowSettings()).ShowDialog();
        }

        private void SelectColor_Click(object sender, MouseButtonEventArgs e)
        {
            var c = RectColor.SelectColor();
            RectColor.SetShapeColor(c);
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Task.Run((Action)RefreshServerList);
        }

        private void RefreshServerList()
        {
            Action a = () => { buttonRefresh.IsEnabled = false; buttonRefresh.Content = "Обновление..."; };
            this.Dispatcher.Invoke(a);

            var Servers = ServerList.GetServers();
            
            a = delegate
            {
                dataGrid.ItemsSource = Servers;

                

                /*dataGrid.Items.Clear();
                foreach (var s in Servers)
                {
                    dataGrid.Items.Add(s);
                } */
                //dataGridView1.RowCount = Servers.Count;
                /*for (int i = 0; i < Servers.Count; i++)
                {
                    dataGridView1[0, i].Value = Servers[i].ServerName;
                    dataGridView1[1, i].Value = Servers[i].Name;
                    dataGridView1[2, i].Style.BackColor = Color.FromArgb(int.Parse(Servers[i].Color));
                    dataGridView1[3, i].Value = Servers[i].IP;
                }

    */
                buttonRefresh.IsEnabled = true;
                buttonRefresh.Content = "Обновить";

                buttonConnect.IsEnabled = (Connection.Current.state == Connection.State.Off) && (Servers.Count > 0);
            };

            this.Dispatcher.Invoke(a);
        }
    }
}
