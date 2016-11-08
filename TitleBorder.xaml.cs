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

namespace Tic_Tac_Toe_WPF_Remake
{
    /// <summary>
    /// Логика взаимодействия для TitleBorder.xaml
    /// </summary>
    public partial class TitleBorder : UserControl
    {
        Window ParentWindow;
        public TitleBorder()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { ParentWindow = Window.GetWindow(this); };
        }

        private void Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ParentWindow.DragMove();
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ParentWindow.Close();
        }
    }
}
