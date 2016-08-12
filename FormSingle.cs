using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Add settings +
 * Replace panel by picturebox +
 * Incorrect turn
 * Comments
 * Make settings
 * Replace grid by tic tac toe fields
 */

namespace TTTM
{
    public partial class FormSingle : Form
    {
        SinglePlayerWithFriend game;
        Position IncorrectTurn;
        Graphics graphics;
        Pen penc1, penc2;

        public FormSingle()
        {
            InitializeComponent();
            graphics = Graphics.FromHwnd(pictureBox1.Handle);
        }

        private void FormSingle_Load(object sender, EventArgs e)
        {
            StartSinlgeGame frm = new StartSinlgeGame();
            if (frm.ShowDialog() != DialogResult.OK)
            {
                Close();
                return;
            }
            string pl1, pl2;
            pl1 = frm.textBox1.Text;
            pl2 = frm.textBox2.Text;
            penc1 = new Pen(frm.panel1.BackColor);
            penc2 = new Pen(frm.panel2.BackColor);
            game = new SinglePlayerWithFriend(pl1, pl2);
            label2.Text = pl1;
            game.ChangeTurn += (s, ev) => { RedrawGame(); if (label2.Text == pl1) label2.Text = pl2; else label2.Text = pl1; };
            game.IncorrectTurn += Game_IncorrectTurn;
        }

        private void Game_IncorrectTurn(object sender, Position e)
        {
            if (e != null)
                IncorrectTurn = e;
        }

        private PointF[] DiagonalyLines(Rectangle Rect)
        {
            List<PointF> l = new List<PointF>();
            for (int i = 0; i <= Math.Max(Rect.Width, Rect.Height); i+=8)
            { // x + w - min(w,i) = x + w + max(-w,-i) = x + max(0,w-i)
                l.Add(new PointF(Rect.X + Math.Min(Rect.Width, i), Rect.Y));
                l.Add(new PointF(Rect.X, Rect.Y + Math.Min(Rect.Height, i)));
                l.Add(new PointF(Rect.X + Math.Max(0, Rect.Width - i), Rect.Y + Rect.Height));
                l.Add(new PointF(Rect.X + Rect.Width, Rect.Y + Math.Max(0, Rect.Height - i)));
            }
            return l.ToArray();
        }

        private void RedrawGame()
        {
            Rectangle rectangle = new Rectangle(0, 0, this.Width - 17, this.Height - 40);
            //graphics.DrawLine(Pens.Blue, new Point(rectangle.Width / 3, 10), new Point(rectangle.Width / 3, rectangle.Height - 10));
            //graphics.DrawLine(Pens.Blue, new Point(rectangle.Width *2 / 3, 10), new Point(rectangle.Width*2 / 3, rectangle.Height - 10));
            //graphics.DrawEllipse(System.Drawing.Pens.Black, rectangle);
            //graphics.DrawRectangle(System.Drawing.Pens.Red, rectangle);

            //graphics.FillRectangle(Brushes.Black, rectangle);

            for (int i = 0; i <= 9; i++)
            {
                graphics.DrawLine(Pens.Blue, new Point(i * 20, 0), new Point(i * 20, 20 * 9));
                graphics.DrawLine(Pens.Blue, new Point(0, i * 20), new Point(20 * 9, i * 20));
            }
            

            //game.Turn(new Pos(DateTime.Now.Second % 3, DateTime.Now.Millisecond % 3), new Pos(0, 0));
            //StringBuilder sb = new StringBuilder();
            int[,] State = game.State();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (State[i,j] == 1)
                        graphics.DrawEllipse(penc1, i * 20 + 2, j * 20 + 2, 16, 16);
                    if (State[i,j] == 2)
                        graphics.DrawEllipse(penc2, i * 20 + 2, j * 20 + 2, 16, 16);
                    //sb.Append(State[i,j].ToString());
                }
                //sb.Append('\n');
            }

            FieldState[,] FState = game.FieldsState();
            //bool[,] Filled = game.FilledFields();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    graphics.DrawRectangle(Pens.Orange, i * 60 + 1, j * 60 + 1, 58, 58);
                    if (FState[i, j].Filled)
                        graphics.DrawLines(Pens.Gray, DiagonalyLines(new Rectangle(i * 60, j * 60, 60, 60)));
                    if (FState[i, j].OwnerID == 1)
                        //graphics.DrawEllipse(Pens.Red, j * 60 + 2, i * 60 + 2, 60-4, 60-4);
                        graphics.DrawLines(penc1, DiagonalyLines(new Rectangle(i * 60, j * 60, 60, 60)));
                        //DiagonalyLines(graphics, new Rectangle(j * 60, i * 60, 60, 60));
                    if (FState[i, j].OwnerID == 2)
                        graphics.DrawLines(penc2, DiagonalyLines(new Rectangle(i * 60, j * 60, 60, 60)));
                    //graphics.DrawLines(Pens.Green, DiagonalyLines(new Rectangle(j * 60, i * 60, 20, 20)));
                }
            }

            if (IncorrectTurn != null)
                graphics.DrawRectangle(Pens.Yellow, IncorrectTurn.x * 60 + 0, IncorrectTurn.y * 60 + 0, 60, 60);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            game.ClickOn((PointToClient(MousePosition).X - pictureBox1.Left) / 20, (PointToClient(MousePosition).Y - pictureBox1.Top) / 20);
            RedrawGame();
            IncorrectTurn = null;
        }

        private void FormSingle_Paint(object sender, PaintEventArgs e)
        {
            graphics.FillRectangle(Brushes.White, 0, 0, this.Width, this.Height);
            RedrawGame();
        }
    }
}
