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
    /// Логика взаимодействия для WindowSettings.xaml
    /// </summary>
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();
            ReadValuesFromSettings(Settings.Current);
        }

        private void ReadValuesFromSettings(Settings s)
        {
            textBoxPlayer1.Text = s.DefaultName1;
            textBoxPlayer2.Text = s.DefaultName2;
            textBoxServerAPI.Text = s.MasterServerAPIUrl;
            textBoxPort.Text = s.MpPort.ToString();
            RectColor1.SetShapeColor(s.PlayerColor1);
            RectColor2.SetShapeColor(s.PlayerColor2);
            RectColorSmallGrid.SetShapeColor(s.SmallGrid);
            RectColorBigGrid.SetShapeColor(s.BigGrid);
            RectColorIncorrectTurn.SetShapeColor(s.IncorrectTurn);
            RectColorBackground.SetShapeColor(s.BackgroundColor);
            RectColorNobodysField.SetShapeColor(s.FilledField);
            RectColorHelpColor.SetShapeColor(s.HelpColor);
            sliderLinesAlpha.Value = s.HelpLinesAlpha;
            sliderSquaresAlpha.Value = s.HelpCellsAlpha;
            checkBoxShowHelp.IsChecked = s.HelpShow == 1;
            checkBoxCheckForUpdates.IsChecked = s.CheckForUpdates == 1;
            textBoxServerName.Text = s.DefaultServerName;
        }

        private void ColorSelect_Click(object sender, MouseButtonEventArgs e)
        {
            var c = ((Rectangle)sender).SelectColor();
            ((Rectangle)sender).SetShapeColor(c);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (RectColor1.GetShapeColor().DifferenceWith(RectColor2.GetShapeColor()) < 100)
            {
                MessageBox.Show("Слишком похожие цвета, выберите другие", "Ошибка сохранения настроек", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (textBoxPlayer1.Text == textBoxPlayer2.Text)
            {
                MessageBox.Show("Имена игроков не могут совпадать", "Ошибка сохранения настроек", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxPlayer1.Text) || string.IsNullOrWhiteSpace(textBoxPlayer2.Text))
            {
                MessageBox.Show("Имена игроков не могут быть пустыми", "Ошибка сохранения настроек", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (RectColorBackground.GetShapeColor().DifferenceWith(RectColor2.GetShapeColor()) < 50 || RectColorBackground.GetShapeColor().DifferenceWith(RectColor1.GetShapeColor()) < 50)
            {
                MessageBox.Show("Цвета не должны быть близки к фоновому цвету", "Ошибка сохранения настроек", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ushort port;
            if (!ushort.TryParse(' ' + textBoxPort.Text + ' ', out port))
            {
                MessageBox.Show("Некорректный порт", "Ошибка сохранения настроек", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Settings.Current.DefaultName1 = textBoxPlayer1.Text;
            Settings.Current.DefaultName2 = textBoxPlayer2.Text;
            Settings.Current.MasterServerAPIUrl = textBoxServerAPI.Text;
            Settings.Current.MpPort = int.Parse(textBoxPort.Text);
            Settings.Current.BackgroundColor = RectColorBackground.GetShapeColor();
            Settings.Current.IncorrectTurn = RectColorIncorrectTurn.GetShapeColor();
            Settings.Current.BigGrid = RectColorBigGrid.GetShapeColor();
            Settings.Current.SmallGrid = RectColorSmallGrid.GetShapeColor();
            Settings.Current.PlayerColor1 = RectColor1.GetShapeColor();
            Settings.Current.PlayerColor2 = RectColor2.GetShapeColor();
            Settings.Current.FilledField = RectColorNobodysField.GetShapeColor();
            Settings.Current.HelpColor = RectColorHelpColor.GetShapeColor();
            Settings.Current.HelpCellsAlpha = (int)Math.Round(sliderSquaresAlpha.Value);
            Settings.Current.HelpLinesAlpha = (int)Math.Round(sliderLinesAlpha.Value);
            Settings.Current.HelpShow = checkBoxShowHelp.IsChecked == true ? 1 : 0;
            //Settings.Current.GraphicsLevel = (int)numericUpDown1.Value;
            Settings.Current.CheckForUpdates = checkBoxCheckForUpdates.IsChecked == true ? 1 : 0;
            Settings.Current.DefaultServerName = textBoxServerName.Text;

            MasterServer.ChangeAPIUrl(Settings.Current.MasterServerAPIUrl);
            DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void buttonDefaults_Click(object sender, RoutedEventArgs e)
        {
            Settings DefaultSettings = new Settings();
            DefaultSettings.SetDefaults();
            ReadValuesFromSettings(DefaultSettings);
        }
    }
}
