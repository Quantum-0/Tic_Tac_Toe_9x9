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
using TTTM;

namespace Tic_Tac_Toe_WPF_Remake
{
    /// <summary>
    /// Логика взаимодействия для WindowSingleStart.xaml
    /// </summary>
    public partial class WindowSingleStart : Window
    {
        public WindowSingleStart()
        {
            InitializeComponent();
            if (Settings.Current != null)
            {
                textBoxPlayer1.Text = Settings.Current.DefaultName1;
                textBoxPlayer2.Text = Settings.Current.DefaultName2;
                RectColor1.SetShapeColor(Settings.Current.PlayerColor1);
                RectColor2.SetShapeColor(Settings.Current.PlayerColor2);
            }
        }

        private void ColorSelect_Click(object sender, MouseButtonEventArgs e)
        {
            var c = ((Rectangle)sender).SelectColor();
            ((Rectangle)sender).SetShapeColor(c);
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Current.BackgroundColor.DifferenceWith(RectColor1.GetShapeColor()) < 50 || Settings.Current.BackgroundColor.DifferenceWith(RectColor2.GetShapeColor()) < 50)
            {
                MessageBox.Show("Цвета не должны быть близки к фоновому цвету", "Ошибка создания игры", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RectColor1.GetShapeColor().DifferenceWith(RectColor2.GetShapeColor()) < 100)
            {
                MessageBox.Show("Слишком похожие цвета, выберите другие", "Ошибка создания игры", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (textBoxPlayer1.Text == textBoxPlayer2.Text)
            {
                MessageBox.Show("Имена игроков не могут совпадать", "Ошибка создания игры", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxPlayer1.Text) || string.IsNullOrWhiteSpace(textBoxPlayer2.Text))
            {
                MessageBox.Show("Имена игроков не могут быть пустыми", "Ошибка создания игры", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            /*if (comboBoxLevel.SelectedIndex == -1 && checkBoxPlayWithComputer.IsChecked == true)
            {
                MessageBox.Show("Выберите сложность", "Ошибка создания игры", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }*/

            DialogResult = true;
            Close();
        }

        private void checkBoxPlayWithComputer_Checked(object sender, RoutedEventArgs e)
        {
            sliderBotLevel.IsEnabled = checkBoxPlayWithComputer.IsChecked == true;
        }
    }
}
