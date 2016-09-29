using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;

namespace TTTM
{
    class AdvancedComboBox : ComboBox
    {
        new public System.Windows.Forms.DrawMode DrawMode { get; set; }
        public Color HighlightColor { get; set; }

        public AdvancedComboBox()
        {
            base.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.HighlightColor = Color.Gray;
            //this.DrawItem += new DrawItemEventHandler(AdvancedComboBox_DrawItem);
        }

        /*protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            e.DrawBackground();
            ComboBoxItem item = (ComboBoxItem)this.Items[e.Index];
            Brush brush = new SolidBrush(item.ForeColor);
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            { brush = Brushes.Yellow; }
            e.Graphics.DrawString(item.Text, this.Font, brush, e.Bounds.X, e.Bounds.Y);
        }*/

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            e.DrawBackground();
            if (e.Index >= 0)
            {
                object current = this.Items[e.Index];
                string text = current.ToString();
                Color clr = current is AdvancedComboBoxItem ? ((AdvancedComboBoxItem)current).Color : Color.Black;

                using (Brush brush = new SolidBrush(clr))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds.X + 1, e.Bounds.Y + 1, 13, 13);
                }

                using (Brush brush = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(text, e.Font, brush, e.Bounds.Left + 16, e.Bounds.Top);
                }
            }
        }

        public class AdvancedComboBoxItem
        {
            public AdvancedComboBoxItem()
            { }

            public AdvancedComboBoxItem(string Text, Color Color)
            {
                this.Text = Text;
                this.Color = Color;
            }

            public string Text { set; get; }
            public Color Color { set; get; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
