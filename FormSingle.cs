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
 * Buffered Rendering
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
        private BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        BufferedGraphics BufGFX;
        Graphics gfx;
        Pen penc1, penc2;
        string pl1, pl2;
        Rectangle[,] Zones = new Rectangle[9, 9];
        Rectangle[,] FieldZones = new Rectangle[3, 3];

        public FormSingle()
        {
            InitializeComponent();
        }
        
        private void Game_IncorrectTurn(object sender, Position e)
        {
            if (e != null)
                IncorrectTurn = e;
        }

        private PointF[] DiagonalyLines(Rectangle Rect) // FIX IT!1
        {
            List<PointF> l = new List<PointF>();
            // x + w - min(w,i) = x + w + max(-w,-i) = x + max(0,w-i)
            for (int i = 0; i <= Math.Max(Rect.Width, Rect.Height); i+=8)
            { 
                l.Add(new PointF(Rect.X + Math.Min(Rect.Width, i), Rect.Y));
                l.Add(new PointF(Rect.X, Rect.Y + Math.Min(Rect.Height, i)));
                //l.Add(new PointF(Rect.X + Math.Max(0, Rect.Width - i), Rect.Y + Rect.Height));
                //l.Add(new PointF(Rect.X + Rect.Width, Rect.Y + Math.Max(0, Rect.Height - i)));
            }
            return l.ToArray();
        }

        private void RedrawGame(bool refreshGraphics = false)
        {
            // Обновление графики (нужно при ресайзе)
            if (refreshGraphics)
            {
                BufGFX = context.Allocate(Graphics.FromHwnd(pictureBox1.Handle), new Rectangle(new Point(), pictureBox1.Size));
                gfx = BufGFX.Graphics;
            }

            // Выход при попытке редравнуть несуществующую игру (чтоб избежать эксепшна)
            if (game == null)
                return;

            // Ширина Высота
            float w = pictureBox1.Width;
            float h = pictureBox1.Height;

            // Фон
            gfx.FillRectangle(Brushes.White, 0, 0, w, h);

            /* Не оч выглядит 
            //gfx.DrawLine(Pens.Blue, (float)(1 + 3)/11 * w, (float)1/11 * h, (float)(1+3)/11 * w, (float)10/11 * h);
            //gfx.DrawLine(Pens.Blue, (float)(1 + 3 + 3)/11 * w, (float)1/11 * h, (float)(1+6)/11 * w, (float)10/11 * h);
            //gfx.DrawLine(Pens.Blue, (float)1/11 * w, (float)4/11 * h, (float)10/11 * w, (float)4/11 * h);
            //gfx.DrawLine(Pens.Blue, (float)1/11 * w, (float)7/11 * h, (float)10/11 * w, (float)7/11 * h);

            //for (int i = 0; i < 3; i++)
            //    for (int j = 0; j < 3; j++)
            //    {
            //        for (int ii = 0; ii < 3; ii++)
            //            for (int jj = 0; jj < 3; jj++)
            //            {
            //                double BIGx = (1 + 3 * i) / 11.0;
            //                double SMALLx = (1 + 3 + 3 * ii) / 11.0;
            //                float x = (float)(w * (BIGx + SMALLx * 3 / 11));

            //                double BIGy = (1 + 3 * j) / 11.0;
            //                double SMALLy = (1 + 3 + 3 * jj) / 11.0;
            //                float y = (float)(h * (BIGy + SMALLy * 3 / 11));

            //                double BIGx1 = (1 + 3 * i) / 11.0;
            //                double SMALLx1 = 1.0 / 11;
            //                float x1 = (float)(w * (BIGx1 + SMALLx1 * 3 / 11));
            //                double BIGx2 = (1 + 3 * i) / 11.0;
            //                double SMALLx2 = 10.0 / 11;
            //                float x2 = (float)(w * (BIGx2 + SMALLx2 * 3 / 11));

            //                double BIGy1 = (1 + 3 * j) / 11.0;
            //                double SMALLy1 = 1.0 / 11;
            //                float y1 = (float)(h * (BIGy1 + SMALLy1 * 3 / 11));
            //                double BIGy2 = (1 + 3 * j) / 11.0;
            //                double SMALLy2 = 10.0 / 11;
            //                float y2 = (float)(h * (BIGy2 + SMALLy2 * 3 / 11));

            //                if (ii != 2 && jj != 2)
            //                {
            //                    gfx.DrawLine(Pens.Red, x, y1, x, y2);
            //                    gfx.DrawLine(Pens.Red, x1, y, x2, y);
            //                }

            //                Zones[i * 3 + ii, j * 3 + jj] = new Rectangle((int)(x - w * 9 / 121), (int)(y - h * 9 / 121), (int)w * 9 / 121, (int)h * 9 / 121);
            //            }
            //        FieldZones[i, j] = new Rectangle((int)w * (1 + 3 * i) / 11, (int)h * (1 + 3 * j) / 11, (int)w * 3 / 11, (int)h * 3 / 11);
            //    }

            */

            for (int i = 1; i <= 10; i++)
            {
                gfx.DrawLine(Pens.Blue, new PointF(w * i / 11f, h / 11f), new PointF(w * i / 11f, h * 10 / 11f));
                gfx.DrawLine(Pens.Blue, new PointF(w / 11f, h * i / 11f), new PointF(w * 10 / 11f, h * i / 11f));
            }
            
            int[,] State = game.State();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Rectangle rect = new Rectangle((int)((w * (i + 1)) / 11f), (int)((h * (j + 1)) / 11f), (int)(w / 11f), (int)(h / 11f));
                    if (State[i,j] == 1)
                        gfx.DrawEllipse(penc1, rect);
                    if (State[i,j] == 2)
                        gfx.DrawEllipse(penc2, rect);
                }
            }

            FieldState[,] FState = game.FieldsState();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Rectangle rect = new Rectangle((int)((w * (i * 3 + 1)) / 11f), (int)((h * (j * 3 + 1)) / 11f), (int)(3 * w / 11f), (int)(3 * h / 11f));
                    gfx.DrawRectangle(Pens.Orange, rect);
                    if (FState[i, j].Filled)
                        gfx.DrawLines(Pens.Gray, DiagonalyLines(rect));
                    if (FState[i, j].OwnerID == 1)
                        gfx.DrawLines(penc1, DiagonalyLines(rect));
                    if (FState[i, j].OwnerID == 2)
                        gfx.DrawLines(penc2, DiagonalyLines(rect));
                }
            }

            if (IncorrectTurn != null)
                gfx.DrawRectangle(Pens.Yellow, new Rectangle((int)((w * (IncorrectTurn.x * 3 + 1)) / 11f), (int)((h * (IncorrectTurn.y * 3 + 1)) / 11f), (int)(w * 3/ 11f), (int)(h * 3/ 11f)));

            BufGFX.Render();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (game == null)
                NewGame();
            else
            {
                Point pnt = new Point(PointToClient(MousePosition).X - pictureBox1.Left, PointToClient(MousePosition).Y - pictureBox1.Top);
                PointF pntf = new PointF(pnt.X * 11f / pictureBox1.Width - 1, pnt.Y * 11f / pictureBox1.Height - 1);
                game.ClickOn((int)pntf.X, (int)pntf.Y);
                //game.ClickOn((PointToClient(MousePosition).X - pictureBox1.Left) / 20, (PointToClient(MousePosition).Y - pictureBox1.Top) / 20);
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
            RedrawGame(true);
        }

        private void Game_ChangeTurn(object sender, Player e)
        {
            RedrawGame();
            if (labelCurrentTurn.Text == pl1)
                labelCurrentTurn.Text = pl2;
            else
                labelCurrentTurn.Text = pl1;
        }

        private void FormSingle_ResizeEnd(object sender, EventArgs e)
        {
            // Чтоб при разворачивании окна было норм
            if (WindowState == FormWindowState.Maximized)
                Refresh();

            
            RedrawGame(true);
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
            //RedrawGame();
        }
    }
}
