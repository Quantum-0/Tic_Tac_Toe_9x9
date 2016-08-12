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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSingle_Click(object sender, EventArgs e)
        {
            FormSingle Game = new FormSingle();
            Game.Show();
            Game.Closed += (sndr, args) => { Show(); };
            Hide();
        }
    }
}
