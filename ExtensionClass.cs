using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Tic_Tac_Toe_WPF_Remake
{
    public static class ExtensionsClass
    {
        // Рисование диагональной штриховки
        public static void DrawDiagonalyLines(this Graphics gfx, Pen pen, Rectangle Rect)
        {
            for (int i = 0; i <= Rect.Width + Rect.Height; i += 8)
            {
                PointF p1 = new PointF(Math.Min(i, Rect.Width) + Rect.Left, Math.Max(i - Rect.Width, 0) + Rect.Top);
                PointF p2 = new PointF(Math.Max(i - Rect.Height, 0) + Rect.Left, Math.Min(i, Rect.Height) + Rect.Top);
                gfx.DrawLine(pen, p1, p2);
            }
        }

        // Есть ли соединение у сокета
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                if (!socket.Connected)
                    return false;
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        // Рисование RectangleF
        public static void DrawRectangle(this Graphics gfx, Pen pen, RectangleF Rect)
        {
            gfx.DrawRectangle(pen, Rect.X, Rect.Y, Rect.Width, Rect.Height);
        }

        // Получение цвета шейпа
        public static Color GetShapeColor(this System.Windows.Shapes.Shape Shape)
        {
            var C = ((System.Windows.Media.SolidColorBrush)(Shape.Fill)).Color;
            return Color.FromArgb(C.R, C.G, C.B);
        }

        // Установка цвета шейпа
        public static void SetShapeColor(this System.Windows.Shapes.Shape Shape, Color Color)
        {
            Shape.Fill = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = Color.R, G = Color.G, B = Color.B });
        }

        // Выбор цвета
        public static Color SelectColor(this System.Windows.Shapes.Rectangle Rect)
        {
            var cd = new System.Windows.Forms.ColorDialog();
            cd.Color = Rect.GetShapeColor();
            cd.ShowDialog();
            return cd.Color;
        }

        // Разница между двумя цветами
        public static double DifferenceWith(this Color clr1, Color clr2)
        {
            double r1 = clr1.R;
            double r2 = clr2.R;
            double g1 = clr1.G;
            double g2 = clr2.G;
            double b1 = clr1.B;
            double b2 = clr2.B;

            double diff = Math.Sqrt((r1 - r2) * (r1 - r2) + (g1 - g2) * (g1 - g2) + (b1 - b2) * (b1 - b2));

            return diff;
        }
    }
}