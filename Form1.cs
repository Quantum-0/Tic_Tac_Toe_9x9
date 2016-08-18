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
    public partial class FormMainMenu : Form
    {
        Settings settings;

        public FormMainMenu()
        {
            InitializeComponent();
        }

        private void buttonSingle_Click(object sender, EventArgs e)
        {
            if (!Settings.Load("Settings.cfg", out settings))
                settings = new Settings();
            FormSingle Game = new FormSingle(settings);
            Hide();
            Game.Closed += (sndr, args) => { Show(); };
            Game.Show();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Settings.Save("Settings.cfg", settings);
            Application.Exit();
        }
    }
}
