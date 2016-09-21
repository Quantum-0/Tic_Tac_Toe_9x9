using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace TTTM
{
    public partial class FormMainMenu : Form
    {
        Settings settings;
        int vSpeed = 0;

        public FormMainMenu()
        {
            InitializeComponent();
            timerOpacity.Start();
            mainMenuControl.buttonSingleplayer.Click += buttonSingle_Click;
            mainMenuControl.buttonMultiplayer.Click += buttonMulti_Click;
            mainMenuControl.buttonSettings.Click += buttonSettings_Click;
            mainMenuControl.buttonHelp.Click += buttonHelp_Click;
            mainMenuControl.buttonExit.Click += buttonExit_Click;

            // Создаём настройки
            Settings.Load("Settings.cfg", out settings);
        }

        private void buttonHelp_Click(object sender, RoutedEventArgs e)
        {
            (new FormAbout()).ShowDialog();
        }

        private void buttonSingle_Click(object sender, EventArgs e)
        {
            // Открытие формы одиночной игры, подписка на событие о её закрытии и скрывание текущей формы
            FormSingle GameForm = new FormSingle(settings);
            Hide();
            GameForm.Closed += delegate { this.Show(); };
            GameForm.Show();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            // Закрытие формы и выход из игры
            timerOpacity.Stop();
            timerClosing.Start();
            //Close();
        }

        private void FormMainMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Сохранение настроек перед закрытием
            Settings.Save("Settings.cfg", settings);
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            (new FormSettings(settings)).ShowDialog();
        }

        private void buttonMulti_Click(object sender, EventArgs e)
        {
            StartMultiplayerGame smg = new StartMultiplayerGame(settings);
            Hide();
            smg.FormClosed += delegate { this.Show(); };
            smg.Show();
        }

        private void timerOpacity_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.02;
            if (this.Opacity == 100)
            {
                timerOpacity.Stop();
                timerOpacity.Tick -= timerOpacity_Tick;
            }
        }

        private void timerClosing_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.02;
            this.SetDesktopLocation(this.Location.X, this.Location.Y + vSpeed);
            if (Math.Round(Opacity * 100) % 4 == 0)
                vSpeed += 1;
            if (this.Opacity == 0)
            {
                timerClosing.Stop();
                timerClosing.Tick -= timerClosing_Tick;
                Close();
            }
        }
    }
}
