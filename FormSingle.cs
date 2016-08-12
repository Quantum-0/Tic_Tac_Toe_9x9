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
 * Incorrect turn +
 * Field Scalling
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
        string pl1, pl2;

        public FormSingle()
        {
            InitializeComponent();
            graphics = Graphics.FromHwnd(pictureBox1.Handle);
        }

        private void FormSingle_Load(object sender, EventArgs e)
        {
            
        }
        /*
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112) // WM_SYSCOMMAND
            {
                // Check your window state here
                if (m.WParam == new IntPtr(0xF030)) // Maximize event - SC_MAXIMIZE from Winuser.h
                {
                    FormSingle_ResizeEnd(null, null);
                }
            }
            base.WndProc(ref m);
        }
        */
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
            //Rectangle rectangle = new Rectangle(0, 0, this.Width - 17, this.Height - 40);
            float w = pictureBox1.Width;
            float h = pictureBox1.Height;

            graphics.DrawLine(Pens.Blue, (float)(1 + 3)/11 * w, (float)1/11 * h, (float)(1+3)/11 * w, (float)10/11 * h);
            graphics.DrawLine(Pens.Blue, (float)(1 + 3 + 3)/11 * w, (float)1/11 * h, (float)(1+6)/11 * w, (float)10/11 * h);
            graphics.DrawLine(Pens.Blue, (float)1/11 * w, (float)4/11 * h, (float)10/11 * w, (float)4/11 * h);
            graphics.DrawLine(Pens.Blue, (float)1/11 * w, (float)7/11 * h, (float)10/11 * w, (float)7/11 * h);

            Rectangle[,] Zones = new Rectangle[9, 9];
            for (int i = 0; i < 3; i++)
                for (int ii = 0; ii < 3; ii++)
                    for (int j = 0; j < 3; j++)
                        for (int jj = 0; jj < 3; jj++)
                        {
                            double BIGx = (1 + 3 * i) / 11.0;
                            double SMALLx = (1 + 3 + 3 * ii) / 11.0;
                            float x = (float)(w * (BIGx + SMALLx * 3 / 11));

                            double BIGy = (1 + 3 * j) / 11.0;
                            double SMALLy = (1 + 3 + 3 * jj) / 11.0;
                            float y = (float)(h * (BIGy + SMALLy * 3 / 11));

                            double BIGx1 = (1 + 3 * i) / 11.0;
                            double SMALLx1 = 1.0 / 11;
                            float x1 = (float)(w * (BIGx1 + SMALLx1 * 3 / 11));
                            double BIGx2 = (1 + 3 * i) / 11.0;
                            double SMALLx2 = 10.0 / 11;
                            float x2 = (float)(w * (BIGx2 + SMALLx2 * 3 / 11));

                            double BIGy1 = (1 + 3 * j) / 11.0;
                            double SMALLy1 = 1.0 / 11;
                            float y1 = (float)(h * (BIGy1 + SMALLy1 * 3 / 11));
                            double BIGy2 = (1 + 3 * j) / 11.0;
                            double SMALLy2 = 10.0 / 11;
                            float y2 = (float)(h * (BIGy2 + SMALLy2 * 3 / 11));

                            if (ii != 3 && jj != 3)
                            {
                                graphics.DrawLine(Pens.Red, x, y1, x, y2);
                                graphics.DrawLine(Pens.Red, x1, y, x2, y);
                            }

                            Zones[i * 3 + ii, j * 3 + jj] = new Rectangle((int)(x-w*9/121), (int)(y-h*9/121), (int)w*9/121, (int)h*9/121);
                        }

            //for (int i = 0; i <= 9; i++)
            //{
            //    graphics.DrawLine(Pens.Blue, new Point(i * 20, 0), new Point(i * 20, 20 * 9));
            //    graphics.DrawLine(Pens.Blue, new Point(0, i * 20), new Point(20 * 9, i * 20));
            //}
            graphics.DrawRectangle(Pens.Lime, Zones[3, 3]);
            return;

            int[,] State = game.State();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (State[i,j] == 1)
                        graphics.DrawEllipse(penc1, i * 20 + 2, j * 20 + 2, 16, 16);
                    if (State[i,j] == 2)
                        graphics.DrawEllipse(penc2, i * 20 + 2, j * 20 + 2, 16, 16);
                }
            }

            FieldState[,] FState = game.FieldsState();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    graphics.DrawRectangle(Pens.Orange, i * 60 + 1, j * 60 + 1, 58, 58);
                    if (FState[i, j].Filled)
                        graphics.DrawLines(Pens.Gray, DiagonalyLines(new Rectangle(i * 60, j * 60, 60, 60)));
                    if (FState[i, j].OwnerID == 1)
                        graphics.DrawLines(penc1, DiagonalyLines(new Rectangle(i * 60, j * 60, 60, 60)));
                    if (FState[i, j].OwnerID == 2)
                        graphics.DrawLines(penc2, DiagonalyLines(new Rectangle(i * 60, j * 60, 60, 60)));
                }
            }

            if (IncorrectTurn != null)
                graphics.DrawRectangle(Pens.Yellow, IncorrectTurn.x * 60 + 0, IncorrectTurn.y * 60 + 0, 60, 60);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (game == null)
                NewGame();
            else
            {
                game.ClickOn((PointToClient(MousePosition).X - pictureBox1.Left) / 20, (PointToClient(MousePosition).Y - pictureBox1.Top) / 20);
                RedrawGame();
                IncorrectTurn = null;
            }
        }

        private void NewGame()
        {
            StartSinlgeGame frm = new StartSinlgeGame();
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            pl1 = frm.textBox1.Text;
            pl2 = frm.textBox2.Text;
            penc1 = new Pen(frm.panel1.BackColor);
            penc2 = new Pen(frm.panel2.BackColor);
            game = new SinglePlayerWithFriend(pl1, pl2);
            labelCurrentTurn.Text = pl1;
            game.ChangeTurn += Game_ChangeTurn; 
            game.IncorrectTurn += Game_IncorrectTurn;
        }

        private void Game_ChangeTurn(object sender, WhosTurn e)
        {
            RedrawGame();
            if (labelCurrentTurn.Text == pl1)
                labelCurrentTurn.Text = pl2;
            else
                labelCurrentTurn.Text = pl1;
        }

        private void FormSingle_ResizeEnd(object sender, EventArgs e)
        {
            graphics = Graphics.FromHwnd(pictureBox1.Handle);
            graphics.FillRectangle(Brushes.White, 0, 0, this.Width, this.Height);
            RedrawGame();
        }

        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            if (game != null)
            {
                game.ChangeTurn -= Game_ChangeTurn;
                game.IncorrectTurn -= Game_IncorrectTurn;
                game = null;
            }
            NewGame();
        }

        private void FormSingle_Paint(object sender, PaintEventArgs e)
        {
            if (game == null)
                return;

            graphics.FillRectangle(Brushes.White, 0, 0, this.Width, this.Height);
            RedrawGame();
        }
    }
}
