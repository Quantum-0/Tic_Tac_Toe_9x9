using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    public partial class FormMultiplayer : Form
    {
        Settings settings;
        Connection connection;
        string MyNick, OpponentNick;
        Color MyColor, OpponentColor;

        public FormMultiplayer(Settings settings, Connection connection, string MyNick, string OpponentNick, Color MyColor, Color OpponentColor)
        {
            InitializeComponent();
            this.settings = settings;
            this.connection = connection;
            this.MyColor = MyColor;
            this.OpponentColor = OpponentColor;
            this.MyNick = MyNick;
            this.OpponentNick = OpponentNick;
            connection.ReceivedChat += Connection_ReceivedChat;
        }

        private void Connection_ReceivedChat(object sender, string e)
        {
            Action d = new Action(delegate
            {
                AddMessageToChat(OpponentNick, e);
            });

            Invoke(d);
        }

        private void AddMessageToChat(string PlayerName, string Text)
        {
            textBoxChat.AppendText('[' + DateTime.Now.ToShortTimeString() + "] " + PlayerName + ": " + Text + "\r\n");
        }

        private void textBoxChatInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && !string.IsNullOrWhiteSpace(textBoxChatInput.Text))
            {
                AddMessageToChat(MyNick, textBoxChatInput.Text);
                connection.SendChat(textBoxChatInput.Text);
                textBoxChatInput.Clear();
                e.Handled = true;
            }
        }

        private void textBoxChatInput_Leave(object sender, EventArgs e)
        {
            textBoxChatInput.ForeColor = Color.Gray;
            if (textBoxChatInput.Text == "")
                textBoxChatInput.Text = "Сообщение сопернику";
        }

        private void textBoxChat_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void FormMultiplayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход", MessageBoxButtons.YesNo) != DialogResult.Yes)
                e.Cancel = true;
            else
            {
                connection.SendEndGame();
                connection.Disconnect();
            }
        }

        private void textBoxChatInput_Enter(object sender, EventArgs e)
        {
            textBoxChatInput.ForeColor = Color.Black;
            if (textBoxChatInput.Text == "Сообщение сопернику")
                textBoxChatInput.Text = "";
        }

        private void textBoxChatInput_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
