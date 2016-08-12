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
        public StartSinlgeGame()
        {
            InitializeComponent();
        }
        private void panel_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
                (sender as Panel).BackColor = colorDialog.Color;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (panel1.BackColor == panel2.BackColor || textBox1.Text == textBox2.Text || string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
                return;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
