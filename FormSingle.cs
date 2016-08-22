using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * TODO:
 * 
 * Add settings +
 * Replace panel by picturebox +
 * Incorrect turn +
 * Buffered Rendering +
 * Field Scalling +
 * Comments +/-
 * Сохранять в GameState айдишник текущего хода +
 * Доделать сохранение/загрузку в логике (в файл)
 * Доделать сохранение/загрузку в форме
 * Глючит последний столбец / протестить и понять что с ним было не так
 * И если заполнено ходить в другую +
 * Make settings +
 * Сделать мультиплеер
 * Отрисовывать нормально, а не как сейчас +/-
 * Нормально отрисовывать диагональные линии
 */

namespace TTTM
{
    public partial class FormSingle : Form
    {
        ABot Bot;
        bool WithBot;
        Settings settings;
        GameManagerWthFriend game;
        Position IncorrectTurn;
        private BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        BufferedGraphics BufGFX;
        Graphics gfx;
        Pen penc1, penc2;
        string pl1, pl2;
        Rectangle[,] Zones = new Rectangle[9, 9];
        Rectangle[,] FieldZones = new Rectangle[3, 3];

        public FormSingle(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
        }

        private void Game_IncorrectTurn(object sender, Position e)
        {
            if (e != null)
                IncorrectTurn = e;
        }

        private void RedrawGame(bool refreshGraphics = false)
        {
            // Цвета
            Brush Background = new SolidBrush(settings.BackgroundColor);
            Pen SmallGrid = new Pen(settings.SmallGrid);
            Pen BigGrid = new Pen(settings.BigGrid, 3);
            Pen pIncorrectTurn = new Pen(settings.IncorrectTurn, 4);
            Pen FilledField = new Pen(settings.FilledField);

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
            gfx.FillRectangle(Background, 0, 0, w, h);

            //Линии
            for (int i = 1; i <= 10; i++)
            {
                gfx.DrawLine(SmallGrid, new PointF(w * i / 11f, h / 11f), new PointF(w * i / 11f, h * 10 / 11f));
                gfx.DrawLine(SmallGrid, new PointF(w / 11f, h * i / 11f), new PointF(w * 10 / 11f, h * i / 11f));
            }

            // Вывод игрового состояния
            int[,] State = game.State();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Rectangle rect = new Rectangle((int)((w * (i + 1.1)) / 11f), (int)((h * (j + 1.1)) / 11f), (int)(0.8 * w / 11f), (int)(0.8 * h / 11f));
                    if (State[i, j] == 1)
                        gfx.DrawEllipse(penc1, rect);
                    if (State[i, j] == 2)
                        gfx.DrawEllipse(penc2, rect);
                }
            }

            // Закрашивание полей
            FieldState[,] FState = game.FieldsState();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Rectangle rect = new Rectangle((int)((w * (i * 3 + 1)) / 11f), (int)((h * (j * 3 + 1)) / 11f), (int)(3 * w / 11f), (int)(3 * h / 11f));
                    gfx.DrawRectangle(BigGrid, rect);
                    if (FState[i, j].Filled)
                        gfx.DrawDiagonalyLines(FilledField, rect);
                    if (FState[i, j].OwnerID == 1)
                        gfx.DrawDiagonalyLines(penc1, rect);
                    if (FState[i, j].OwnerID == 2)
                        gfx.DrawDiagonalyLines(penc2, rect);
                }
            }

            // Выделение поля, куда нужно ходить, при попытке пойти нетуда
            if (IncorrectTurn != null)
                gfx.DrawRectangle(pIncorrectTurn, new Rectangle((int)((w * (IncorrectTurn.x * 3 + 1)) / 11f), (int)((h * (IncorrectTurn.y * 3 + 1)) / 11f), (int)(w * 3 / 11f), (int)(h * 3 / 11f)));

            // Рендеринг полученной графики
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
            StartSinlgeGame frm = new StartSinlgeGame(settings);
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            if (game != null)
            {
                game.ChangeTurn -= Game_ChangeTurn;
                game.IncorrectTurn -= Game_IncorrectTurn;
                game.SomebodyWins -= Game_SomebodyWins;
                game.NobodyWins -= Game_NobodyWins;
                game.Dispose();
            }
            pl1 = frm.textBox1.Text;
            pl2 = frm.textBox2.Text;
            penc1 = new Pen(frm.panel1.BackColor);
            penc2 = new Pen(frm.panel2.BackColor);

            WithBot = frm.checkBox1.Checked;
            if (WithBot)
            {
                game = new GameManagerWithBot(pl1, pl2, 2);
                Bot = (game as GameManagerWithBot).Bot;
            }
            else
            {
                game = new GameManagerWthFriend(pl1, pl2);
            }

            labelCurrentTurn.Text = pl1;
            game.ChangeTurn += Game_ChangeTurn;
            game.IncorrectTurn += Game_IncorrectTurn;
            game.SomebodyWins += Game_SomebodyWins;
            game.NobodyWins += Game_NobodyWins;
            RedrawGame(true);
            buttonLoadGame.Enabled = true;
            buttonSaveGame.Enabled = true;
        }

        private void Game_NobodyWins(object sender, EventArgs e)
        {
            MessageBox.Show("Игра окончена. Ничья");
            buttonSaveGame.Enabled = false;
        }

        private void Game_SomebodyWins(object sender, Game.GameEndArgs e)
        {
            MessageBox.Show("Игра окончена.\nПобедитель: " + e.Winner.Name);
            buttonSaveGame.Enabled = false;
        }

        private void Game_ChangeTurn(object sender, Player e)
        {
            //if (e.Id == 2 && WithBot)
            //{
            //    // вставить тут остановку выполнения на секунду, тип бот думает
            //    Bot.makeTurn();
            //    game.CurrentPlayer = game.Player1;
            //}

            // Перерисовка
            RedrawGame();
            labelCurrentTurn.Text = e.Name;
            if (WithBot)
            {
                timerBotTurn.Interval = new Random().Next(500, 2500);
                timerBotTurn.Start();
            }
        }

        private void FormSingle_ResizeEnd_And_SizeChanged(object sender, EventArgs e)
        {
            // Чтоб при разворачивании окна было норм
            if (WindowState == FormWindowState.Maximized)
                Refresh();

            //// Чтоб отображалось поле норм, но как-то не очень работает
            //this.Width = (int)Math.Floor(this.Width / 9.0) * 9;
            //this.Height = (int)Math.Floor(this.Height / 9.0) * 9;

            RedrawGame(true);
        }

        private void buttonSaveGame_Click(object sender, EventArgs e)
        {
            if (game == null)
                return;

            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Filter = "Tic Tac Toe Save File|*.ttts|Все файлы|*.*";
            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(SaveDialog.FileName))
                {
                    sw.WriteLine(pl1);
                    sw.WriteLine(pl2);
                    sw.WriteLine(game.Save());
                }
            }
        }
        /// <summary>
        /// ////////////////////////починить тута
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoadGame_Click(object sender, EventArgs e)
        {
            if (game == null)
                return;

            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Filter = "Tic Tac Toe Save File|*.ttts|Все файлы|*.*";
            if (OpenDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(OpenDialog.FileName))
                {
                    pl1 = sr.ReadLine();
                    pl2 = sr.ReadLine();
                    game.Dispose();
                    //game = new GameManagerWthFriend / new GameManagerWthBot в зависимости от того, с ботом эта игра или нет
                    game.Load(sr.ReadLine());
                }
            }
            RedrawGame();
        }

        private void FormSingle_Load(object sender, EventArgs e)
        {
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            (new FormSettings(settings)).ShowDialog();
        }

        private void timerBotTurn_Tick(object sender, EventArgs e)
        {
            (game as GameManagerWithBot).BotTurn();
            timerBotTurn.Stop();
        }

        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        private void FormSingle_Paint(object sender, PaintEventArgs e)
        {
            RedrawGame();
        }
    }

    public static class ExtensionsClass
    {
        public static void DrawDiagonalyLines(this Graphics gfx, Pen pen, Rectangle Rect)
        {
            for (int i = 0; i <= Rect.Width + Rect.Height; i += 12)
            {
                PointF p1 = new PointF(Math.Min(i, Rect.Width) + Rect.Left, Math.Max(i - Rect.Width, 0) + Rect.Top);
                PointF p2 = new PointF(Math.Max(i - Rect.Height, 0) + Rect.Left, Math.Min(i, Rect.Height) + Rect.Top);
                gfx.DrawLine(pen, p1, p2);
            }
        }
    }
}
