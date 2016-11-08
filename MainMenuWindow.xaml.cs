using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public MainWindow()
        {
            InitializeComponent();

            Task.Run((Action)TaskOpening);

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
                                //Settings.Save("Settings.cfg", Settings.Current);
                                Application.Current.Shutdown();
                            }
                        };
                        UpdatingSystem.Update();
                    }
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

        private void TaskOpening()
        {
            for (int i = 0; i < 70; i++)
            {
                Thread.Sleep(15);
                this.Dispatcher.Invoke(delegate
                {
                    this.Opacity += 0.015;
                });
            }
        }

        private void TaskClosing()
        {
            int vSpeed = -10;
            for (int i = 0; i < 35; i++)
            {
                Thread.Sleep(20);
                this.Dispatcher.Invoke(delegate
                {
                    this.Opacity -= 0.03;
                    this.Top += vSpeed;
                });
                vSpeed += 1;
            }
            Settings.Save("Settings.cfg");
            this.Dispatcher.Invoke(delegate
            {
                Application.Current.Shutdown();
            });
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
            if (this.Opacity >= 1)
                Task.Run((Action)TaskClosing);
        }
    }
}
