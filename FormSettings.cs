using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    public partial class FormSettings : Form
    {
        Settings settings;

        public FormSettings(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            ReadValuesFromSettings(settings);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (panel1.BackColor.DifferenceWith(panel2.BackColor) < 100)
            {
                MessageBox.Show("Слишком похожие цвета, выберите другие", "Ошибка сохранения настроек", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (textBox1.Text == textBox3.Text)
            {
                MessageBox.Show("Имена игроков не могут совпадать", "Ошибка сохранения настроек", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Имена игроков не могут быть пустыми", "Ошибка сохранения настроек", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (settings.BackgroundColor.DifferenceWith(panel2.BackColor) < 50 || settings.BackgroundColor.DifferenceWith(panel1.BackColor) < 50)
            {
                MessageBox.Show("Цвета не должны быть близки к фоновому цвету", "Ошибка сохранения настроек", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ushort port;
            if (!ushort.TryParse(' ' + textBox4.Text + ' ', out port))
            {
                MessageBox.Show("Некорректный порт", "Ошибка сохранения настроек", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            settings.DefaultName1 = textBox1.Text;
            settings.DefaultName2 = textBox3.Text;
            settings.MasterServerAPIUrl = textBox2.Text;
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
            settings.GraphicsLevel = (int)numericUpDown1.Value;
            settings.CheckForUpdates = checkBox2.Checked ? 1 : 0;

            MasterServer.ChangeAPIUrl(settings.MasterServerAPIUrl);
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
            ReadValuesFromSettings(DefaultSettings);
        }

        private void ReadValuesFromSettings(Settings s)
        {
            textBox1.Text = s.DefaultName1;
            textBox3.Text = s.DefaultName2;
            textBox2.Text = s.MasterServerAPIUrl;
            textBox4.Text = s.MpPort.ToString();
            panel1.BackColor = s.PlayerColor2;
            panel2.BackColor = s.PlayerColor1;
            panel3.BackColor = s.SmallGrid;
            panel4.BackColor = s.BigGrid;
            panel5.BackColor = s.IncorrectTurn;
            panel6.BackColor = s.BackgroundColor;
            panel7.BackColor = s.FilledField;
            panel8.BackColor = s.HelpColor;
            trackBar1.Value = s.HelpCellsAlpha;
            trackBar2.Value = s.HelpLinesAlpha;
            checkBox1.Checked = (s.HelpShow == 1);
            numericUpDown1.Value = s.GraphicsLevel;
            checkBox2.Checked = s.CheckForUpdates == 1;
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
            if (textBox3.Text.Contains('='))
                textBox3.Undo();

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.Text = "Игрок 1";
                textBox3.SelectAll();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains('='))
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
