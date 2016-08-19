using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    /*
     * TODO:
     * 
     * Форматировать порт (tryParse), форматировать IP
     * Запрещать одинаковые цвета и ники
     */

    public partial class FormSettings : Form
    {
        Settings settings;

        public FormSettings(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            textBox1.Text = settings.DefaultName1;
            textBox3.Text = settings.DefaultName2;
            textBox2.Text = settings.MpIP;
            textBox4.Text = settings.MpPort.ToString();
            panel1.BackColor = settings.PlayerColor2;
            panel2.BackColor = settings.PlayerColor1;
            panel3.BackColor = settings.SmallGrid;
            panel4.BackColor = settings.BigGrid;
            panel5.BackColor = settings.IncorrectTurn;
            panel6.BackColor = settings.BackgroundColor;
            panel7.BackColor = settings.FilledField;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            settings.DefaultName1 = textBox1.Text;
            settings.DefaultName2 = textBox3.Text;
            settings.MpIP = textBox2.Text;
            settings.MpPort = int.Parse(textBox4.Text);
            settings.BackgroundColor = panel6.BackColor;
            settings.IncorrectTurn = panel5.BackColor;
            settings.BigGrid = panel4.BackColor;
            settings.SmallGrid = panel3.BackColor;
            settings.PlayerColor1 = panel2.BackColor;
            settings.PlayerColor2 = panel1.BackColor;
            settings.FilledField = panel7.BackColor;
            DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void panel_Click(object sender, EventArgs e)
        {
            colorDialog.Color = (sender as Panel).BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
                (sender as Panel).BackColor = colorDialog.Color;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Игрок 1";
            textBox3.Text = "Игрок 2";
            panel2.BackColor = Color.Red;
            panel1.BackColor = Color.FromArgb(0, 192, 0);
            panel3.BackColor = Color.DodgerBlue;
            panel4.BackColor = Color.FromArgb(255, 128, 0);
            panel5.BackColor = Color.Yellow;
            panel6.BackColor = Color.White;
            panel7.BackColor = Color.LightGray;
            textBox2.Text = "127.0.0.1";
            textBox4.Text = "7890";
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;

            UInt16 value;
            textBox4.Text = UInt16.TryParse(textBox4.Text, out value) ? value.ToString() : "7890";
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            UInt16 value;
            textBox4.Text = UInt16.TryParse(textBox4.Text, out value) ? value.ToString() : "7890";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //if (!Regex.IsMatch(textBox2.Text, @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
            //    textBox2.Undo();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text == textBox1.Text || textBox3.Text.Contains('='))
                textBox3.Undo();

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.Text = "Игрок 1";
                textBox3.SelectAll();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text == textBox1.Text || textBox1.Text.Contains('='))
                textBox1.Undo();

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Игрок 2";
                textBox1.SelectAll();
            }
        }
    }
}
