using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TTTM;

namespace Tic_Tac_Toe_WPF_Remake
{
    public partial class MainWindow : Window
    {
        System.Timers.Timer timerOpacity = new System.Timers.Timer() { Interval = 20 };
        System.Timers.Timer timerClosing = new System.Timers.Timer() { Interval = 20 };
        int vSpeed = 0;

        public MainWindow()
        {
            InitializeComponent();

            timerOpacity.Elapsed += timerOpacity_Tick;
            timerClosing.Elapsed += timerClosing_Tick;
            timerOpacity.Start();

            // Создаём настройки
            Settings.Load("Settings.cfg");
            MasterServer.ChangeAPIUrl(Settings.Current.MasterServerAPIUrl);
            
            // Проверяем наличие обновлений
            if (Settings.Current.CheckForUpdates == 1)
                if (UpdatingSystem.CheckUpd())
                    if (MessageBox.Show("Скачать обновление?", "Найдено обновление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        UpdatingSystem.UpdatingError += (o, e) => { MessageBox.Show("Не удалось установить обновление"); };
                        UpdatingSystem.ClosingRequest += (o, e) =>
                        {
                            if (MessageBox.Show("Обновление готово к установке. Закрыть приложение?", "Обновление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                Settings.Save("Settings.cfg", Settings.Current);
                                Application.Current.Shutdown();
                            }
                        };
                        UpdatingSystem.Update();
                    }
        }

        private void timerOpacity_Tick(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(delegate {
                this.Opacity += 0.03;

                if (this.Opacity >= 1)
                {
                    timerOpacity.Stop();
                    timerOpacity.Elapsed -= timerOpacity_Tick;
                }
            });
        }

        private void timerClosing_Tick(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                this.Opacity -= 0.03;
                this.Top += vSpeed;
                vSpeed += 1;
                if (this.Opacity <= 0)
                {
                    timerClosing.Stop();
                    timerClosing.Elapsed -= timerClosing_Tick;
                    Application.Current.Shutdown();
                }
            });
        }

        private void buttonSingleplayer_Click(object sender, RoutedEventArgs e)
        {
            // Открытие формы одиночной игры, подписка на событие о её закрытии и скрывание текущей формы
            var GameForm = new WindowSingle();
            this.Visibility = Visibility.Hidden;
            GameForm.Show();
            GameForm.Closed += delegate { this.Visibility = Visibility.Visible; };
        }

        private void buttonMultiplayer_Click(object sender, RoutedEventArgs e)
        {
            var GameForm = new WindowMPStart();
            this.Visibility = Visibility.Hidden;
            GameForm.Show();
            GameForm.Closed += delegate { this.Visibility = Visibility.Visible; };
        }

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            (new WindowSettings()).ShowDialog();
        }

        private void buttonHelp_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            (new WindowAbout()).ShowDialog();
            this.Visibility = Visibility.Visible;
        }

        private void buttonStats_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            // Закрытие формы и выход из игры
            vSpeed = -10;
            timerOpacity.Stop();
            timerClosing.Start();
        }

        private void rectTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MainMenu_Closed(object sender, EventArgs e)
        {
            // Сохранение настроек перед закрытием
            Settings.Save("Settings.cfg");
        }
    }
}
