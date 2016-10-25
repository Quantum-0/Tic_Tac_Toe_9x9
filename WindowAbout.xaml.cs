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
using System.Windows.Shapes;

namespace Tic_Tac_Toe_WPF_Remake
{
    /// <summary>
    /// Логика взаимодействия для WindowAbout.xaml
    /// </summary>
    public partial class WindowAbout : Window
    {
        public WindowAbout()
        {
            InitializeComponent();
        }
        private void buttonRules_Click(object sender, RoutedEventArgs e)
        {
            if (buttonRules.Content.ToString() == "Правила тип")
            {
                var uri = new Uri("pack://application:,,,/Resources/About_2.jpg");
                ImageSource a = new BitmapImage(uri);
                grid.Background = new ImageBrush(a);
                buttonRules.Content = "Ну назад тип";
            }
            else
            {
                var uri = new Uri("pack://application:,,,/Resources/About_1.jpg");
                ImageSource a = new BitmapImage(uri);
                grid.Background = new ImageBrush(a);
                buttonRules.Content = "Правила тип";
            }
        }
    }
}
