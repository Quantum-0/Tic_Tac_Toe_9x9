﻿using System;
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

        public StartMultiplayerGame(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            connection = new Connection();
            connection.ReceivedIAM += Connection_ReceivedIAM;
            connection.GameStarts += Connection_GameStarts;
            connection.ConnectingRejected += Connection_ConnectingRejected;
        }

        private void Connection_ConnectingRejected(object sender, EventArgs e)
        {
            Action d = delegate
            {
                labelConnectedPlayer.Visible = false;
                labelConnectedPlayerNick.Visible = false;
                panel2.Visible = false;
                toolStripStatusLabel.Text = "Владелец сервера отклонил запрос";
                buttonCancel.Visible = false;
                buttonStart.Text = "Подключится";
                buttonStart.Enabled = true;
            };
            Invoke(d);
            connection.Disconnect();
        }

        private void Connection_GameStarts(object sender, EventArgs e)
        {
            Action d = delegate
            {
                toolStripStatusLabel.Text = "Игра началась";
                FormMultiplayer MPForm = new FormMultiplayer(settings, connection, textBoxNick.Text, labelConnectedPlayerNick.Text, panel1.BackColor, panel2.BackColor);
                MPForm.Show();
                MPForm.FormClosed += delegate { this.Visible = true; };
                this.Visible = false;
            };
            Invoke(d);
        }

        private void Connection_ReceivedIAM(object sender, Connection.IAMEventArgs e)
        {
            Action d = delegate
            {
                labelConnectedPlayer.Visible = true;
                labelConnectedPlayerNick.Text = e.Nick;
                labelConnectedPlayerNick.Visible = true;
                panel2.BackColor = e.Color;
                panel2.Visible = true;
                buttonStart.Enabled = true;
                buttonStart.Text = "Начать игру";

                if (connection.Host.Value) buttonCancel.Text = "Отклонить";
            };

            Invoke(d);
            // Проверять, если e.Nick == ник сервера или e.Color близко к цвет сервера => Reject и StartListening снова
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
            if (connection.state == Connection.State.Off)
            {
                interfaceNetConfig = interfaceNetConfigState.AllDisabled;
                interfacePlayerSettings = false;
                interfaceServerClientSwitcher = false;

                if (radioButtonServer.Checked)
                {
                    startServer();
                    buttonStart.Text = "Начать игру";
                }
                else
                    startClient();
            }
            else if (connection.state == Connection.State.Connected || connection.state == Connection.State.WaitForStartFromMe)
            {
                connection.SendStartGame();
                buttonStart.Enabled = false;
                toolStripStatusLabel.Text = "Ожидание начала игры от другого игрока.";
            }
        }

        private void startClient()
        {
            toolStripStatusLabel.Text = "Попытка подключения..";
            interfaceBtnStart = interfaceButtonState.Disabled;
            Refresh();
            try
            {
                connection.ConnectToServer(textBoxIP.Text, 7890);
                connection.SendIAM(textBoxNick.Text, panel1.BackColor);
                interfaceBtnCancel = interfaceButtonState.Enabled;
                buttonCancel.Text = "Отключиться";
                buttonStart.Text = "Начать игру";
                toolStripStatusLabel.Text = "Подключено к серверу";
            }
            catch (SocketException e)
            {
                stopConnecting();
                if (e.ErrorCode == 10061)
                    toolStripStatusLabel.Text = "Сервер отверг подключение";
                else
                    toolStripStatusLabel.Text = "Ошибка " + e.ErrorCode;
            }
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


            else if (connection.state == Connection.State.Connected || connection.state == Connection.State.WaitForStartFromAnother || connection.state == Connection.State.WaitForStartFromMe)
            {
                // Если сервер
                if (connection.Host.Value)
                {
                    connection.AnotherPlayerDisconnected -= ConnectionServer_AnotherPlayerDisconnected;
                    connection.SendReject();
                    connection.Disconnect();
                    startServer();
                }
                else // Если клиент
                {
                    connection.Disconnect();
                    toolStripStatusLabel.Text = "Отключено";
                    buttonStart.Text = "Подключиться";
                }
            }
        }

        private void startServer()
        {
            try
            {
                connection.StopServerListening();
                toolStripStatusLabel.Text = "Запуск сервера";
                connection.StartServerListening(7890);
                connection.AnotherPlayerConnected += ConnectionServer_AnotherPlayerConnected;
                toolStripStatusLabel.Text = "Ожидание входящего подключения";
                interfaceBtnCancel = interfaceButtonState.Enabled;
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

        private void ConnectionServer_AnotherPlayerDisconnected(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "Клиент отключился";
            connection.AnotherPlayerDisconnected -= ConnectionServer_AnotherPlayerDisconnected;

            Action d = delegate
            {
                labelConnectedPlayer.Visible = false;
                labelConnectedPlayerNick.Visible = false;
                panel2.Visible = false;
                buttonStart.Enabled = false;
            };
            this?.Invoke(d);

            d = startServer;
            this?.Invoke(d);
        }

        private void ConnectionServer_AnotherPlayerConnected(object sender, EventArgs e)
        {
            connection.SendIAM(textBoxNick.Text, panel1.BackColor);
            toolStripStatusLabel.Text = "Клиент подключён";
            connection.AnotherPlayerDisconnected += ConnectionServer_AnotherPlayerDisconnected;
            connection.AnotherPlayerConnected -= ConnectionServer_AnotherPlayerConnected;
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                (sender as Panel).BackColor = colorDialog1.Color;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            labelConnectedPlayer.Visible = false;
            labelConnectedPlayerNick.Visible = false;
            panel2.Visible = false;
            stopConnecting();
        }

        private void StartMultiplayerGame_FormClosed(object sender, FormClosedEventArgs e)
        {
            connection.Disconnect();
            connection.StopServerListening();
        }
    }
}
