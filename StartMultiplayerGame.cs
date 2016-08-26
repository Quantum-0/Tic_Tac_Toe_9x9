using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    public partial class StartMultiplayerGame : Form
    {
        Connection connection;
        Settings settings;
        public StartMultiplayerGame(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            connection = new Connection();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void radioButtonServer_CheckedChanged(object sender, EventArgs e)
        {
            buttonStart.Text = "Создать сервер";
            textBoxIP.Enabled = false;
        }

        private void StartMultiplayerGame_Load(object sender, EventArgs e)
        {
            textBoxNick.Text = settings.DefaultName1;
            panel1.BackColor = settings.PlayerColor1;
            textBoxIP.Text = settings.MpIP;
            textBoxPort.Text = settings.MpPort.ToString();
        }

        private void radioButtonClient_CheckedChanged(object sender, EventArgs e)
        {
            buttonStart.Text = "Подключится";
            textBoxIP.Enabled = true;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            radioButtonServer.Enabled = false;
            radioButtonClient.Enabled = false;
            textBoxIP.Enabled = false;
            textBoxPort.Enabled = false;
            textBoxNick.Enabled = false;
            panel1.Click -= panel1_Click;

            if (radioButtonServer.Checked)
                startServer();
            else
                startClient();
        }

        private void startClient()
        {
            toolStripStatusLabel1.Text = "Попытка подключения..";
            buttonStart.Enabled = false;
            Refresh();
            try
            {
                connection.ConnectToServer(textBoxIP.Text, 7890);
            }
            catch (SocketException e)
            {
                stopConnecting();
                if (e.ErrorCode == 10061)
                    toolStripStatusLabel1.Text = "Сервер отверг подключение";
                return;
            }

            buttonCancel.Visible = true;
            buttonCancel.Text = "Отключиться";
            toolStripStatusLabel1.Text = "Подключено к серверу";

            // После этого ждём чтоб сервер прислал ник/цвет игрока с сервера
        }

        private void stopConnecting()
        {
            radioButtonServer.Enabled = true;
            radioButtonClient.Enabled = true;
            textBoxIP.Enabled = radioButtonClient.Checked;
            textBoxPort.Enabled = true;
            textBoxNick.Enabled = true;
            panel1.Click += panel1_Click;
            buttonCancel.Visible = false;
            buttonStart.Enabled = true;

            if (connection.state == Connection.State.Listening)
            {
                toolStripStatusLabel1.Text = "Сервер остановлен";
                connection.StopServerListening();
                buttonStart.Text = "Создать сервер";
            }


            if (connection.state == Connection.State.Connected)
            {
                toolStripStatusLabel1.Text = "Отключено";
                connection.Disconnect();
            }
        }

        private void startServer()
        {
            try
            {
                toolStripStatusLabel1.Text = "Запуск сервера";
                connection.StartServerListening(7890);
                connection.AnotherPlayerConnected += ConnectionServer_AnotherPlayerConnected;
                toolStripStatusLabel1.Text = "Ожидание входящего подключения";
                buttonCancel.Visible = true;
                buttonStart.Text = "Начать игру";
                buttonStart.Enabled = false;
                buttonCancel.Text = "Отключить сервер";
                Refresh();
            }
            catch (SocketException e)
            {
                toolStripStatusLabel1.Text = "Порт уже занят";
                radioButtonClient.Enabled = true;
                radioButtonServer.Enabled = true;
                textBoxNick.Enabled = true;
                textBoxPort.Enabled = true;
                panel1.Click += panel1_Click;
            }
        }

        private void ConnectionServer_AnotherPlayerConnected(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Клиент подключён";
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                (sender as Panel).BackColor = colorDialog1.Color;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            stopConnecting();
        }
    }
}
