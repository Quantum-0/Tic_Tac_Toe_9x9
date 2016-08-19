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
    /*
     * TODO:
     * 
     * Картинку для менюшки
     * Попробовать переделать в WPF
     * 
     */

    public partial class FormMainMenu : Form
    {
        Settings settings;

        public FormMainMenu()
        {
            InitializeComponent();

            // Создаём настройки
            if (!Settings.Load("Settings.cfg", out settings))
                settings = new Settings();
        }

        private void buttonSingle_Click(object sender, EventArgs e)
        {
            // Открытие формы одиночной игры, подписка на событие о её закрытии и скрывание текущей формы
            FormSingle GameForm = new FormSingle(settings);
            Hide();
            GameForm.Closed += (sndr, args) => { Show(); };
            GameForm.Show();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
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
    }
}
