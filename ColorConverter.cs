using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Tic_Tac_Toe_WPF_Remake
{
    internal class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var rec = (TTTM.ServerList.ServerRecord)value;
            //System.Drawing.Color Color = System.Drawing.Color.FromArgb(int.Parse(rec.Color));
            System.Drawing.Color Color = System.Drawing.Color.FromArgb(int.Parse(value.ToString()));
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(Color.R, Color.G, Color.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
