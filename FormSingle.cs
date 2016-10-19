using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    public partial class FormSingle : Form
    {
        ABot Bot;
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
        Point CellUnderMouse;
        event EventHandler MouseMovedToAnotherCell;

        // Для графики > 1
        int IncorrectTurnAlpha = 255;
        int HelpAlpha = 255;

        // Конструктор
        public FormSingle(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            if (settings.GraphicsLevel == 2)
            { 
                this.MouseMovedToAnotherCell += delegate { HelpAlpha = 255; };
                timerRefreshView.Start();
            }
            else
            {
                this.MouseMovedToAnotherCell += delegate { RedrawGame(); };
            }
        }
        
        // Перерисовка игры
        private void RedrawGame(bool refreshGraphics = false)
        {
            if (gfx == null && !refreshGraphics)
                return;

            // Обновление графики (нужно при ресайзе)
            if (refreshGraphics)
            {
                BufGFX = context.Allocate(Graphics.FromHwnd(pictureBox1.Handle), new Rectangle(new Point(), pictureBox1.Size));
                gfx = BufGFX.Graphics;
            }

            // Выход при попытке редравнуть несуществующую игру (чтоб избежать эксепшна)
            if (game == null)
                return;

            // Цвета
            Brush Background = new SolidBrush(settings.BackgroundColor);
            Pen SmallGrid = new Pen(settings.SmallGrid);
            Pen BigGrid = new Pen(settings.BigGrid, 3);
            Pen pIncorrectTurn = new Pen(Color.FromArgb(IncorrectTurnAlpha,settings.IncorrectTurn), 4);
            Pen FilledField = new Pen(Color.FromArgb(192, settings.FilledField));
            Pen HelpPen = new Pen(Color.FromArgb(settings.HelpCellsAlpha * HelpAlpha / 255, settings.HelpColor));
            Pen HelpLinesPen = new Pen(Color.FromArgb(settings.HelpLinesAlpha * HelpAlpha / 255, settings.HelpColor));

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
            var brshc1 = new SolidBrush(penc1.Color);
            var brshc2 = new SolidBrush(penc2.Color);
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Rectangle rect = new Rectangle((int)((w * (i + 1.1)) / 11f), (int)((h * (j + 1.1)) / 11f), (int)(0.8 * w / 11f), (int)(0.8 * h / 11f));
                    Rectangle rect2 = new Rectangle((int)((w * (i + 1.3)) / 11f), (int)((h * (j + 1.3)) / 11f), (int)(0.4 * w / 11f), (int)(0.4 * h / 11f));
                    if (State[i, j] == 1)
                    {
                        gfx.DrawEllipse(penc1, rect);
                        gfx.FillEllipse(brshc1, rect2);
                    }
                    if (State[i, j] == 2)
                    {
                        gfx.DrawEllipse(penc2, rect);
                        gfx.FillEllipse(brshc2, rect2);
                    }
                }
            }

            // Закрашивание полей
            var alphaPenC1 = new Pen(Color.FromArgb(192, penc1.Color));
            var alphaPenC2 = new Pen(Color.FromArgb(192, penc2.Color));
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
                        gfx.DrawDiagonalyLines(alphaPenC1, rect);
                    if (FState[i, j].OwnerID == 2)
                        gfx.DrawDiagonalyLines(alphaPenC2, rect);
                }
            }

            // Выделение поля, куда нужно ходить, при попытке пойти нетуда
            if (IncorrectTurn != null)
            {
                gfx.DrawRectangle(pIncorrectTurn, new Rectangle((int)((w * (IncorrectTurn.x * 3 + 1)) / 11f), (int)((h * (IncorrectTurn.y * 3 + 1)) / 11f), (int)(w * 3 / 11f), (int)(h * 3 / 11f)));
            }

            // Соответствие ячеек и полей
            if (settings.HelpShow == 1)
            {
                var r1 = new RectangleF(w * (CellUnderMouse.X + 1) / 11f, h * (CellUnderMouse.Y + 1) / 11f, w / 11f, h / 11f);
                var r2 = new RectangleF(w * (((CellUnderMouse.X % 3 * 3) / 3 * 3) + 1) / 11f, h * (((CellUnderMouse.Y % 3 * 3) / 3 * 3) + 1) / 11f, 3 * w / 11f, 3 * h / 11f);
                gfx.DrawRectangle(HelpPen, r1);
                gfx.DrawRectangle(HelpPen, r2);
                gfx.DrawLine(HelpLinesPen, r1.Location, r2.Location);
                gfx.DrawLine(HelpLinesPen, r1.Left, r1.Bottom, r2.Left, r2.Bottom);
                gfx.DrawLine(HelpLinesPen, r1.Right, r1.Bottom, r2.Right, r2.Bottom);
                gfx.DrawLine(HelpLinesPen, r1.Right, r1.Top, r2.Right, r2.Top);
            }

            // Рендеринг полученной графики
            BufGFX.Render();
        }
        
        // Создание новой игры
        private void NewGame()
        {
            // Ввод настроек для новой игры
            StartSinlgeGame frm = new StartSinlgeGame(settings);
            if (frm.ShowDialog() != DialogResult.OK)
                return;

            // Удаление и отписка от событий уже имеющейся
            if (game != null)
            {
                game.ChangeTurn -= Game_ChangeTurn;
                game.IncorrectTurn -= Game_IncorrectTurn;
                game.SomebodyWins -= Game_SomebodyWins;
                game.NobodyWins -= Game_NobodyWins;
                game.Dispose();
            }

            // Чтение информации из формы настроек новой игры
            pl1 = frm.textBox1.Text;
            pl2 = frm.textBox2.Text + (frm.checkBox1.Checked ? " [Бот]" : "");
            penc1 = new Pen(frm.panel1.BackColor);
            penc2 = new Pen(frm.panel2.BackColor);
            labelCurrentTurn.Text = pl1;
            var BotLevel = frm.comboBox1.SelectedIndex + 1;

            // Создание GameManeger'a
            if (frm.checkBox1.Checked)
            {
                game = new GameManagerWithBot(pl1, pl2, BotLevel);
                Bot = (game as GameManagerWithBot).Bot;
            }
            else
            {
                game = new GameManagerWthFriend(pl1, pl2);
            }
            
            // Подписка на события новой игры
            game.ChangeTurn += Game_ChangeTurn;
            game.IncorrectTurn += Game_IncorrectTurn;
            game.SomebodyWins += Game_SomebodyWins;
            game.NobodyWins += Game_NobodyWins;

            // Перерисовка графики и включение кнопок сохранения и загрузки
            RedrawGame(true);
            buttonLoadGame.Enabled = true;
            buttonSaveGame.Enabled = true;
        }
        
        // Обработчики событий для перерисовки игры
        private void FormSingle_ResizeEnd_And_SizeChanged(object sender, EventArgs e)
        {
            // Чтоб при разворачивании окна оно сперва обновило размер элементов, а затем уже прорисовало графику на них
            if (WindowState == FormWindowState.Maximized)
                Refresh();

            RedrawGame(true);
        }
        private void FormSingle_Paint(object sender, PaintEventArgs e)
        {
            RedrawGame();
        }

        // Обработка игровых событий
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
            // Перерисовка
            RedrawGame();
            labelCurrentTurn.Text = e.Name;

            // Запуск таймера хода бота
            if (game is GameManagerWithBot)
            {
                // Изменить, чтоб время на ход было фиксированным, а не уменьшалось
                timerBotTurn.Interval = 1;// new Random().Next(500, 1000);
                timerBotTurn.Start();
            }
        }

        private void Game_IncorrectTurn(object sender, Position e)
        {
            if (settings.GraphicsLevel == 2)
                IncorrectTurnAlpha = 255;
            if (e != null)
                IncorrectTurn = e;
        }

        // Ход бота после ожидания таймера
        private void timerBotTurn_Tick(object sender, EventArgs e)
        {
            (game as GameManagerWithBot).BotTurn();
            timerBotTurn.Stop();
        }

        // События взаимодействия пользователя с формой
        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            NewGame();
        }
        private void buttonSettings_Click(object sender, EventArgs e)
        {
            (new FormSettings(settings)).ShowDialog();
        }
        private void buttonLoadGame_Click(object sender, EventArgs e)
        {
            // Открытие файла
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Filter = "Tic Tac Toe Save File|*.ttts|Все файлы|*.*";
            if (OpenDialog.ShowDialog() == DialogResult.OK)
            {
                // Отписываемся от событий и удаляем текущую игру
                if (game != null)
                {
                    game.ChangeTurn -= Game_ChangeTurn;
                    game.IncorrectTurn -= Game_IncorrectTurn;
                    game.SomebodyWins -= Game_SomebodyWins;
                    game.NobodyWins -= Game_NobodyWins;
                    game.Dispose();
                }

                // Считываем данные загружаемой игры
                using (StreamReader sr = new StreamReader(OpenDialog.FileName))
                {
                    pl1 = sr.ReadLine();
                    pl2 = sr.ReadLine();
                    var type = sr.ReadLine();
                    if (type == "WF ")
                        game = new GameManagerWthFriend(pl1, pl2);
                    else if (type.Substring(0,2) == "WB")
                    {
                        game = new GameManagerWithBot(pl1, pl2, int.Parse(type[3].ToString()));
                        Bot = (game as GameManagerWithBot).Bot;
                    }
                    else
                    {
                        MessageBox.Show("Не определён тип игры");
                        game.Dispose();
                        return;
                    }
                    var c1 = sr.ReadLine();
                    var c2 = sr.ReadLine();
                    int i1, i2;
                    if (!int.TryParse(c1, out i1) || !int.TryParse(c2, out i2))
                    {
                        MessageBox.Show("Не удалось считать числовые значения из файла сохранения");
                        game.Dispose();
                        return;
                    }
                    penc1 = new Pen(Color.FromArgb(i1));                    
                    penc2 = new Pen(Color.FromArgb(i2));
                    game.Load(sr.ReadLine());
                }

                // Подписываемся на события
                labelCurrentTurn.Text = pl1;
                game.ChangeTurn += Game_ChangeTurn;
                game.IncorrectTurn += Game_IncorrectTurn;
                game.SomebodyWins += Game_SomebodyWins;
                game.NobodyWins += Game_NobodyWins;
                buttonSaveGame.Enabled = true;
            }
            RedrawGame(true);
        }
        private void buttonSaveGame_Click(object sender, EventArgs e)
        {
            // Если игры нет то вываливаемся
            if (game == null)
                return;

            // Ход НЕ БОТА
            if (game.CurrentPlayer.Id == 2 && game is GameManagerWithBot)
                return;

            // Сохраняем файл
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Filter = "Tic Tac Toe Save File|*.ttts|Все файлы|*.*";
            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(SaveDialog.FileName))
                {
                    // Записываем данные в файл:
                    sw.WriteLine(pl1); // Имена
                    sw.WriteLine(pl2);
                    if (game is GameManagerWithBot) // Тип игры
                    {
                        sw.Write("WB");
                        if (Bot is StupidBot)
                            sw.Write("1\n");
                        if (Bot is SomeMoreCleverBot)
                            sw.Write("2\n");
                        if (Bot is RecursionAnalizerBot)
                            sw.Write("3\n");
                    }
                    else
                        sw.WriteLine("WF ");
                    sw.Write(penc1.Color.ToArgb()); // Цвет игроков
                    sw.WriteLine();
                    sw.Write(penc2.Color.ToArgb());
                    sw.WriteLine();
                    sw.WriteLine(game.Save()); // Игровое поле
                }
            }
        }
        private void FormSingle_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Спрашиваем подтверждение на выход
            // (Ибо НЕКОТОРЫЕ не хотят проиграть и кликают по крестику хд)
            if (game != null)
                if (MessageBox.Show("Вы действительно хотите выйти?", "Выход", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    e.Cancel = true;

            // Сохранять тут и при запуске новой игры спрашивать, восстановить ли предыдущую
            game?.Dispose();
        }

        private Point MouseOnGameBoard()
        {
            var m = PointToClient(MousePosition);
            var x = m.X - pictureBox1.Left;
            var y = m.Y - pictureBox1.Top;
            if (x < 0 || x > pictureBox1.Width || y < 0 || y > pictureBox1.Height)
                return new Point(-1, -1);
            x = (int)Math.Floor(x * 11f / pictureBox1.Width - 1);
            y = (int)Math.Floor(y * 11f / pictureBox1.Height - 1);

            return new Point(x, y);
        }

        private void timerRefreshView_Tick(object sender, EventArgs e)
        {
            if (IncorrectTurnAlpha > 0)
                IncorrectTurnAlpha -= 5;
            if (HelpAlpha > 0)
                HelpAlpha -= 5;
            RedrawGame();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            var p = MouseOnGameBoard();
            if (p != CellUnderMouse && p.X >= 0 && p.Y >= 0 && p.X < 9 && p.Y < 9)
            {
                CellUnderMouse = p;
                MouseMovedToAnotherCell(this, new EventArgs());
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Создание новой игры если игра не была создана
            if (game == null)
                NewGame();
            else // Иначе выполнение хода
            {
                var p = MouseOnGameBoard();
                RedrawGame();
                if (settings.GraphicsLevel < 2)
                    IncorrectTurn = null;
                game.ClickOn(p.X, p.Y);
            }
        }
    }

    // Методы расширений (Socket.IsConnected и Graphics.DrawDiagonalyLines)
    public static class ExtensionsClass
    {
        public static void DrawDiagonalyLines(this Graphics gfx, Pen pen, Rectangle Rect)
        {
            for (int i = 0; i <= Rect.Width + Rect.Height; i += 8)
            {
                PointF p1 = new PointF(Math.Min(i, Rect.Width) + Rect.Left, Math.Max(i - Rect.Width, 0) + Rect.Top);
                PointF p2 = new PointF(Math.Max(i - Rect.Height, 0) + Rect.Left, Math.Min(i, Rect.Height) + Rect.Top);
                gfx.DrawLine(pen, p1, p2);
            }
        }
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
        public static void DrawRectangle(this Graphics gfx, Pen pen, RectangleF Rect)
        {
            gfx.DrawRectangle(pen, Rect.X, Rect.Y, Rect.Width, Rect.Height);
        }
        public static double DifferenceWith(this Color clr1, Color clr2)
        {
            /*double h1 = clr1.GetHue() / 360d;
            double h2 = clr2.GetHue() / 360d;
            double b1 = clr1.GetBrightness() / 1d;
            double b2 = clr2.GetBrightness() / 1d;
            double s1 = clr1.GetSaturation() / 1d;
            double s2 = clr2.GetSaturation() / 1d;

            double diff = Math.Abs(h1 * Math.Abs(4*b1*(b1-1) * s1) - h2 * Math.Abs(4 * b2 * (b2 - 1) * s2));
            diff = Math.Min(diff, Math.Abs(1 + h1 * Math.Abs(4 * b1 * (b1 - 1) * s1) - h2 * Math.Abs(4 * b2 * (b2 - 1) * s2)));
            diff = Math.Min(diff, Math.Abs(-1 + h1 * Math.Abs(4 * b1 * (b1 - 1) * s1) - h2 * Math.Abs(4 * b2 * (b2 - 1) * s2)));
            */

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
