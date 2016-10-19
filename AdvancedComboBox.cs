using System.Drawing;
using System.Windows.Forms;

namespace TTTM
{
    class AdvancedComboBox : ComboBox
    {
        new public DrawMode DrawMode { get; set; }
        public Color HighlightColor { get; set; }

        public AdvancedComboBox()
        {
            base.DrawMode = DrawMode.OwnerDrawFixed;
            this.HighlightColor = Color.Gray;
        }

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
