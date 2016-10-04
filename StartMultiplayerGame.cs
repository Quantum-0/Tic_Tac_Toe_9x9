using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    public partial class StartMultiplayerGame : Form
    {
        Settings settings;
        Connection2 connection;
        bool DontAllowChangeTab;
        const bool ShowServerLog = true;

        public StartMultiplayerGame(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
        }

        #region Контроль управления вводом формы
        #region Общее
        
        // Загрузка формы
        private void StartMultiplayerGame_Load(object sender, EventArgs e)
        {
            connection = new Connection2();
            textBoxMyNick.Text = settings.DefaultName1;
            textBoxMyNick_Leave(this, null);
            panelPlayerColor.BackColor = settings.PlayerColor1;
            tabControl.TabPages.Remove(tabPageStartGame);
            connection.OpponentConnected += Connection_OpponentConnected;

        }

        // Переключение вкладок
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (DontAllowChangeTab)
                e.Cancel = true;

            if (e.TabPage == tabPageServerList)
                new Thread(RefreshServerList).Start();
        }

        #endregion
        #region Общие настройки

        // Редактирование поля MyNick
        private void textBoxMyNick_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                textBoxMyNick_Leave(this, null);
            }
        }
        private void textBoxMyNick_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxMyNick.Text))
            {
                textBoxMyNick.Text = "Пожалуйста, укажите Ваш ник";
                textBoxMyNick.ForeColor = Color.Gray;
                DontAllowChangeTab = true;
                return;
            }

            if (textBoxMyNick.Text.Contains('<') || textBoxMyNick.Text.Contains('>'))
            {
                textBoxMyNick.Text = "Ник не должен содержать запрещённые символы";
                textBoxMyNick.ForeColor = Color.Red;
                DontAllowChangeTab = true;
                return;
            }

            DontAllowChangeTab = false;
        }
        private void textBoxMyNick_Enter(object sender, EventArgs e)
        {
            textBoxMyNick.ForeColor = Color.Black;
            if (textBoxMyNick.Text == "Пожалуйста, укажите Ваш ник" || textBoxMyNick.Text == "Ник не должен содержать запрещённые символы")
            {
                textBoxMyNick.Text = "";
            }
        }
        
        // Настройки
        private void buttonOpenSettings_Click(object sender, EventArgs e)
        {
            (new FormSettings(settings)).ShowDialog();
        }

        // Выбор цвета
        private void panel1_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelPlayerColor.ForeColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                (sender as Panel).BackColor = colorDialog1.Color;
        }

        #endregion
        #region Список серверов

        // Обновление списка
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            // Ой всё не получается у меня с async await
            new Thread(RefreshServerList).Start();
        }

        // Подключение
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            var index = dataGridView1.SelectedCells[0].RowIndex;
            if (index == -1)
                return;
            if (dataGridView1.SelectedCells.Count > 0)
                ConnectToServer(ServerList.Servers[index].PublicKey);
            labelServerName.Text = ServerList.Servers[index].ServerName;
        }

        #endregion
        #region Создание сервера
        
        // Редактирование поля "Название сервера"
        private void textBoxServerName_Enter(object sender, EventArgs e)
        {
            textBoxServerName.ForeColor = Color.Black;
            if (textBoxServerName.Text == "Укажите название сервера" || textBoxServerName.Text == "Название сервера не должно содержать запрещённые символы")
            {
                textBoxServerName.Text = "";
            }
        }
        private void textBoxServerName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxServerName.Text))
            {
                textBoxServerName.Text = "Укажите название сервера";
                textBoxServerName.ForeColor = Color.Gray;
                return;
            }

            if (textBoxServerName.Text.Contains('<') || textBoxServerName.Text.Contains('>'))
            {
                textBoxServerName.Text = "Название сервера не должно содержать запрещённые символы";
                textBoxServerName.ForeColor = Color.Red;
                return;
            }
        }
        private void textBoxServerName_TextChanged(object sender, EventArgs e)
        {
            buttonStartServer.Enabled = (!string.IsNullOrWhiteSpace(textBoxServerName.Text) &&
                !textBoxServerName.Text.Contains('<') && !textBoxServerName.Text.Contains('>'));
        }

        // Создание/остановка сервера
        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            StartServer();
        }
        private void buttonCloseServer_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        #endregion

        private void buttonStartGame_Click(object sender, EventArgs e)
        {
            buttonStartGame.Enabled = false;
            connection.SendStartGame();
        }
        #endregion

        #region Логика
        private void RefreshServerList()
        {
            Action a = () => { buttonRefresh.Enabled = false; buttonRefresh.Text = "Обновление..."; };
            this.Invoke(a);

            var Servers = ServerList.GetServers();

            a = delegate
            {
                dataGridView1.RowCount = Servers.Count;
                for (int i = 0; i < Servers.Count; i++)
                {
                    dataGridView1[0, i].Value = Servers[i].ServerName;
                    dataGridView1[1, i].Value = Servers[i].Name;
                    dataGridView1[2, i].Style.BackColor = Color.FromArgb(int.Parse(Servers[i].Color));
                    dataGridView1[3, i].Value = Servers[i].IP;
                }

                buttonRefresh.Enabled = true;
                buttonRefresh.Text = "Обновить";

                buttonConnect.Enabled = Servers.Count > 0;
            };

            this.Invoke(a);
        }
        private void StartServer()
        {
            textBoxServerLog.Visible = ShowServerLog;
            buttonStartServer.Enabled = false;
            textBoxServerName.ReadOnly = true;
            buttonCloseServer.Visible = true;
            DontAllowChangeTab = true;
            labelServerName.Text = textBoxServerName.Text;
            connection.ServerLog += Connection_ServerLog;
            connection.StartServer(textBoxServerName.Text, new Connection2.IAMData(textBoxMyNick.Text, panelPlayerColor.BackColor));
        }
        private void Connection_ServerLog(object sender, string e)
        {
            Action a = delegate
            {
                textBoxServerLog.Text += e + "\r\n";
            };

            this.Invoke(a);
        }
        private void StopServer()
        {
            textBoxServerLog.Visible = false;
            buttonStartServer.Enabled = true;
            textBoxServerName.ReadOnly = false;
            buttonCloseServer.Visible = false;
            DontAllowChangeTab = false;
            //connection.StopServer();
            connection.BreakAnyConnection();
            connection.ServerLog -= Connection_ServerLog;
        }
        private void ConnectToServer(string PublicKey)
        {
            if (connection.ConnectTo(PublicKey, new Connection2.IAMData(textBoxMyNick.Text, panelPlayerColor.BackColor)))
                buttonConnect.Enabled = false;
        }
        private void Connection_OpponentConnected(object sender, Connection2.IAMEventArgs e)
        {
            Action a = delegate
            {
                if (tabControl.TabPages.Contains(tabPageStartGame))
                    tabControl.TabPages.Remove(tabPageStartGame);

                DontAllowChangeTab = false;
                tabControl.TabPages.Add(tabPageStartGame);
                tabControl.SelectTab(tabPageStartGame);
                DontAllowChangeTab = true;

                labelPlayer2Nick.Text = e.Nick;
                labelPlayer1Nick.Text = connection.IAM.Nick;
                panelPlayer2.BackColor = e.Color;
                panelPlayer1.BackColor = connection.IAM.Color;

                connection.GameStarts += Connection_GameStarts;
            };

            this.Invoke(a);
        }
        private void Connection_GameStarts(object sender, bool e)
        {
            Action a = delegate
            {
                connection.GameStarts -= Connection_GameStarts;
                DontAllowChangeTab = false;
                tabControl.SelectTab(tabPageServerList);
                tabControl.TabPages.Remove(tabPageStartGame);
                var GameForm = new FormMultiplayer(settings, connection, labelPlayer1Nick.Text, labelPlayer2Nick.Text, panelPlayer1.BackColor, panelPlayer2.BackColor);
                GameForm.Show();
                GameForm.FormClosed += (s,ee) => this.Visible = true;
                this.Visible = false;
            };

            this.Invoke(a);
        }
        #endregion


    }
}
