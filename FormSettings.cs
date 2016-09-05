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
            panel8.BackColor = settings.HelpColor;
            trackBar1.Value = settings.HelpCellsAlpha;
            trackBar2.Value = settings.HelpLinesAlpha;
            checkBox1.Checked = (settings.HelpShow == 1);
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
            settings.HelpColor = panel8.BackColor;
            settings.HelpCellsAlpha = trackBar1.Value;
            settings.HelpLinesAlpha = trackBar2.Value;
            settings.HelpShow = checkBox1.Checked ? 1 : 0;

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
            Settings DefaultSettings = new Settings();
            DefaultSettings.SetDefaults();

            textBox1.Text = DefaultSettings.DefaultName1;
            textBox3.Text = DefaultSettings.DefaultName2;
            textBox2.Text = DefaultSettings.MpIP;
            textBox4.Text = DefaultSettings.MpPort.ToString();
            panel1.BackColor = DefaultSettings.PlayerColor2;
            panel2.BackColor = DefaultSettings.PlayerColor1;
            panel3.BackColor = DefaultSettings.SmallGrid;
            panel4.BackColor = DefaultSettings.BigGrid;
            panel5.BackColor = DefaultSettings.IncorrectTurn;
            panel6.BackColor = DefaultSettings.BackgroundColor;
            panel7.BackColor = DefaultSettings.FilledField;
            panel8.BackColor = DefaultSettings.HelpColor;
            trackBar1.Value = DefaultSettings.HelpCellsAlpha;
            trackBar2.Value = DefaultSettings.HelpLinesAlpha;
            checkBox1.Checked = (DefaultSettings.HelpShow == 1);

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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = checkBox1.Checked;
            trackBar2.Enabled = checkBox1.Checked;
            panel8.Enabled = checkBox1.Checked;
        }
    }
}
