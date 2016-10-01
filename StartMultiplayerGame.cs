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
            //EnabledPages[tabPageMainSettings] = true;
            //EnabledPages[tabPageServerList] = false;
            //EnabledPages[tabPageStartServer] = false;
            //EnabledPages[tabPageStartGame] = false;
            tabControl.TabPages.Remove(tabPageStartGame);
            connection.OpponentConnected += Connection_OpponentConnected;

        }

        // Переключение вкладок
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            //if (e.TabPage == tabPageStartGame || tabControl.SelectedTab == tabPageStartGame)
            //    e.Cancel = true;
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
            if (dataGridView1.SelectedCells.Count > 0)
                ConnectToServer(ServerList.Servers[dataGridView1.SelectedCells[0].RowIndex].PublicKey);
        }

        #endregion
        #region Создание сервера
        
        // Редактирование поля "Название сервера"
        private void textBoxServerName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                textBoxServerName_Leave(this, null);
            }
        }
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
            buttonStartServer.Enabled = false;

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

            buttonStartServer.Enabled = true;
        }

        // Создание сервера
        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            StartServer();
            buttonStartServer.Enabled = false;
            DontAllowChangeTab = true;
        }

        #endregion
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
            };

            this.Invoke(a);
        }
        private void StartServer()
        {
            connection.StartServer(textBoxServerName.Text, new Connection2.IAMData(textBoxMyNick.Text, panelPlayerColor.BackColor));
        }
        private void ConnectToServer(string PublicKey)
        {
            if (connection.ConnectTo(PublicKey, new Connection2.IAMData(textBoxMyNick.Text, panelPlayerColor.BackColor)))
                buttonConnect.Enabled = false;
        }
        private void Connection_OpponentConnected(object sender, EventArgs e)
        {
            Action a = delegate
            {
                if (tabControl.TabPages.Contains(tabPageStartGame))
                    tabControl.TabPages.Remove(tabPageStartGame);

                DontAllowChangeTab = false;
                tabControl.TabPages.Add(tabPageStartGame);
                tabControl.SelectTab(tabPageStartGame);
                DontAllowChangeTab = true;
            };

            this.Invoke(a);
        }
        #endregion
    }
}
