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

        private enum interfaceNetConfigState : byte
        {
            AllDisabled, OnlyPort, OnlyIP, AllEnabled
        }
        private enum interfaceButtonState : byte
        {
            Hidden = 0, Disabled = 1, Enabled = 2
        }

        private bool interfaceServerClientSwitcher
        {
            get
            {
                if (radioButtonClient.Enabled != radioButtonServer.Enabled)
                    throw new Exception("Некорректное состояние интерфейса");
                else
                    return radioButtonClient.Enabled;
            }
            set
            {
                radioButtonClient.Enabled = value;
                radioButtonServer.Enabled = value;
            }
        }
        private bool interfacePlayerSettings
        {
            set
            {
                if (textBoxNick.Enabled && !value)
                    panel1.Click -= panel1_Click;

                if (!textBoxNick.Enabled && value)
                    panel1.Click += panel1_Click;

                textBoxNick.Enabled = value;
            }
            get
            {
                return textBoxNick.Enabled;
            }
        }
        private interfaceNetConfigState interfaceNetConfig
        {
            set
            {
                switch (value)
                {
                    case interfaceNetConfigState.AllDisabled:
                        textBoxPort.Enabled = false;
                        textBoxIP.Enabled = false;
                        break;
                    case interfaceNetConfigState.OnlyPort:
                        textBoxPort.Enabled = true;
                        textBoxIP.Enabled = false;
                        break;
                    case interfaceNetConfigState.OnlyIP:
                        textBoxPort.Enabled = false;
                        textBoxIP.Enabled = true;
                        break;
                    case interfaceNetConfigState.AllEnabled:
                        textBoxPort.Enabled = true;
                        textBoxIP.Enabled = true;
                        break;
                    default:
                        break;
                }
            }
        }
        private interfaceButtonState interfaceBtnStart
        {
            set
            {
                buttonStart.Enabled = (value == interfaceButtonState.Enabled);
                buttonStart.Visible = (value >= interfaceButtonState.Disabled);
            }
            get
            {
                if (buttonStart.Visible)
                {
                    if (buttonStart.Enabled)
                        return interfaceButtonState.Enabled;
                    else
                        return interfaceButtonState.Disabled;
                }
                else
                    return interfaceButtonState.Hidden;
            }
        }
        private interfaceButtonState interfaceBtnCancel
        {
            set
            {
                buttonCancel.Enabled = (value == interfaceButtonState.Enabled);
                buttonCancel.Visible = (value >= interfaceButtonState.Disabled);
            }
            get
            {
                if (buttonCancel.Visible)
                {
                    if (buttonCancel.Enabled)
                        return interfaceButtonState.Enabled;
                    else
                        return interfaceButtonState.Disabled;
                }
                else
                    return interfaceButtonState.Hidden;
            }
        }

        private void changeInterface(bool? ServerOrClientSwither, bool? PlayerSettings, bool? PortChanging,
            bool? IPChanging, string StartButtonCaption, bool? StartButtonVisible, bool? StartButtonEnabled,
            string CancelButtonCaption, bool? CancelButtonVisible, bool? CancelButtonEnabled)
        {

        }

        public StartMultiplayerGame(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            connection = new Connection();
        }

        private void radioButtonServer_CheckedChanged(object sender, EventArgs e)
        {
            interfaceNetConfig = interfaceNetConfigState.OnlyPort;
            buttonStart.Text = "Создать сервер";
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
            interfaceNetConfig = interfaceNetConfigState.AllEnabled;
            buttonStart.Text = "Подключится";
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            interfaceNetConfig = interfaceNetConfigState.AllDisabled;
            interfacePlayerSettings = false;
            interfaceServerClientSwitcher = false;

            if (radioButtonServer.Checked)
                startServer();
            else
                startClient();
        }

        private void startClient()
        {
            toolStripStatusLabel.Text = "Попытка подключения..";
            interfaceBtnStart = interfaceButtonState.Disabled;
            Refresh();
            try
            {
                connection.ConnectToServer(textBoxIP.Text, 7890);
            }
            catch (SocketException e)
            {
                stopConnecting();
                if (e.ErrorCode == 10061)
                    toolStripStatusLabel.Text = "Сервер отверг подключение";
                return;
            }

            interfaceBtnCancel = interfaceButtonState.Enabled;
            buttonCancel.Text = "Отключиться";
            toolStripStatusLabel.Text = "Подключено к серверу";

            // После этого ждём чтоб сервер прислал ник/цвет игрока с сервера
        }

        private void stopConnecting()
        {
            interfaceServerClientSwitcher = true;
            interfaceNetConfig = (radioButtonClient.Checked) ?
                interfaceNetConfigState.AllEnabled : interfaceNetConfigState.OnlyPort;
            interfacePlayerSettings = true;
            interfaceBtnStart = interfaceButtonState.Enabled;
            interfaceBtnCancel = interfaceButtonState.Hidden;

            if (connection.state == Connection.State.Listening)
            {
                toolStripStatusLabel.Text = "Сервер остановлен";
                connection.StopServerListening();
                buttonStart.Text = "Создать сервер";
            }


            if (connection.state == Connection.State.Connected)
            {
                connection.Disconnect();
                toolStripStatusLabel.Text = "Отключено";
            }
        }

        private void startServer()
        {
            try
            {
                toolStripStatusLabel.Text = "Запуск сервера";
                connection.StartServerListening(7890);
                connection.AnotherPlayerConnected += ConnectionServer_AnotherPlayerConnected;
                toolStripStatusLabel.Text = "Ожидание входящего подключения";
                interfaceBtnCancel = interfaceButtonState.Enabled;
                buttonStart.Text = "Начать игру";
                interfaceBtnStart = interfaceButtonState.Disabled;
                buttonCancel.Text = "Отключить сервер";
                Refresh();
            }
            catch (SocketException e)
            {
                toolStripStatusLabel.Text = "Порт уже занят";
                interfacePlayerSettings = true;
                interfaceServerClientSwitcher = true;
                interfaceNetConfig = (radioButtonClient.Checked) ?
                    interfaceNetConfigState.AllEnabled : interfaceNetConfigState.OnlyPort;
            }
        }

        private void ConnectionServer_AnotherPlayerConnected(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "Клиент подключён";
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
