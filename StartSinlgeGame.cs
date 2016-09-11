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
    public partial class StartSinlgeGame : Form
    {
        Settings settings;
        public StartSinlgeGame(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            if (settings != null)
            {
                textBox1.Text = settings.DefaultName1;
                textBox2.Text = settings.DefaultName2;
                panel1.BackColor = settings.PlayerColor1;
                panel2.BackColor = settings.PlayerColor2;
            }
        }
        private void panel_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
                (sender as Panel).BackColor = colorDialog.Color;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (panel1.BackColor == panel2.BackColor || textBox1.Text == textBox2.Text || string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || (comboBox1.SelectedIndex == -1 && checkBox1.Checked))
                return;
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = checkBox1.Checked;
        }
    }
}
