using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using TTTM;

namespace Tic_Tac_Toe_WPF_Remake
{
    /// <summary>
    /// Логика взаимодействия для WindowSingle.xaml
    /// </summary>
    public partial class WindowSingle : System.Windows.Window
    {
        ABot Bot;
        Position IncorrectTurn;
        GameManager game;
        private BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        BufferedGraphics BufGFX;
        Graphics gfx;
        Pen penc1, penc2;
        string pl1, pl2;
        Rectangle[,] Zones = new Rectangle[9, 9];
        Rectangle[,] FieldZones = new Rectangle[3, 3];
        Point CellUnderMouse;
        event EventHandler MouseMovedToAnotherCell;
        //System.Timers.Timer timerRefreshView = new System.Timers.Timer(50);
        Task ViewRefreshing;
        System.Timers.Timer timerResizing = new System.Timers.Timer(200);
        int IncorrectTurnAlpha = 255;
        int HelpAlpha = 255;
        bool Redrawing;
        bool StopingRedrawing, DontRedraw = true;

        public WindowSingle()
        {
            InitializeComponent();
            
            ViewRefreshing = Task.Run((Action)GameRedrawing);
            timerResizing.Elapsed += TimerResizing_Tick;
            this.MouseMovedToAnotherCell += delegate { HelpAlpha = 255; };
        }

        private void GameRedrawing()
        {
            Redrawing = false;
            while (true)
            {
                Thread.Sleep(50);
                if (StopingRedrawing)
                    break;
                if (!DontRedraw)
                {
                    Redrawing = true;
                    if (IncorrectTurnAlpha > 0)
                        IncorrectTurnAlpha -= 5;
                    if (HelpAlpha > 0)
                        HelpAlpha -= 5;


                    RedrawGame();
                }
                Redrawing = false;
            }
        }

        private void TimerResizing_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            RedrawGame(true);
            DontRedraw = false;
        }

        // Перерисовка игры
        private void RedrawGame(bool refreshGraphics = false)
        {
            this.Dispatcher.Invoke(
                delegate
                {
                    if (!this.IsVisible)
                        return;

                    if (gfx == null && !refreshGraphics)
                        return;

                    // Обновление графики (нужно при ресайзе)
                    if (refreshGraphics)
                    {
                        var hwndsource = (HwndSource)HwndSource.FromVisual(canvas);
                        var pnt = new Point((int)canvas.Margin.Left, (int)canvas.Margin.Top);
                        var sz = new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight);
                        BufGFX = context.Allocate(Graphics.FromHwnd(hwndsource.Handle), new Rectangle(pnt, sz));
                        gfx = BufGFX.Graphics;
                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    }

                    // Выход при попытке редравнуть несуществующую игру
                    if (game == null)
                        return;
                    
                    // Цвета
                    Brush Background = new SolidBrush(Settings.Current.BackgroundColor);
                    Pen SmallGrid = new Pen(Settings.Current.SmallGrid);
                    Pen BigGrid = new Pen(Settings.Current.BigGrid, 3);
                    Pen pIncorrectTurn = new Pen(Color.FromArgb(IncorrectTurnAlpha, Settings.Current.IncorrectTurn), 4);
                    Pen FilledField = new Pen(Color.FromArgb(192, Settings.Current.FilledField));
                    Pen HelpPen = new Pen(Color.FromArgb(Settings.Current.HelpCellsAlpha * HelpAlpha / 255, Settings.Current.HelpColor));
                    Pen HelpLinesPen = new Pen(Color.FromArgb(Settings.Current.HelpLinesAlpha * HelpAlpha / 255, Settings.Current.HelpColor));

                    // Ширина Высота и координаты
                    var w = (float)canvas.ActualWidth;
                    var h = (float)canvas.ActualHeight;
                    var cx = (float)canvas.Margin.Left;
                    var cy = (float)canvas.Margin.Top;

                    // Фон
                    gfx.FillRectangle(Background, 0, 0, w, h);

                    //Линии
                    for (int i = 1; i <= 10; i++)
                    {
                        gfx.DrawLine(SmallGrid, new PointF(cx + w * i / 11f, cy + h / 11f), new PointF(cx + w * i / 11f, cy + h * 10 / 11f));
                        gfx.DrawLine(SmallGrid, new PointF(cx + w / 11f, cy + h * i / 11f), new PointF(cx + w * 10 / 11f, cy + h * i / 11f));
                    }

                    // Вывод игрового состояния
                    int[,] State = game.State();
                    var brshc1 = new SolidBrush(penc1.Color);
                    var brshc2 = new SolidBrush(penc2.Color);
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            RectangleF rect = new RectangleF((float)(cx + (w * (i + 1.1)) / 11f), (float)(cy + (h * (j + 1.1)) / 11f), (float)(0.8 * w / 11f), (float)(0.8 * h / 11f));
                            RectangleF rect2 = new RectangleF((float)(cx + (w * (i + 1.3)) / 11f), (float)(cy + (h * (j + 1.3)) / 11f), (float)(0.4 * w / 11f), (float)(0.4 * h / 11f));
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
                    var alphaPenC1 = new Pen(Color.FromArgb(222, penc1.Color));
                    var alphaPenC2 = new Pen(Color.FromArgb(222, penc2.Color));
                    FieldState[,] FState = game.FieldsState();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            RectangleF rect = new RectangleF((float)(cx + (w * (i * 3 + 1)) / 11f), (float)(cy + (h * (j * 3 + 1)) / 11f), (float)(3 * w / 11f), (float)(3 * h / 11f));
                            gfx.DrawRectangle(BigGrid, rect);
                            if (FState[i, j].Filled && FState[i, j].Owner == null)
                                gfx.DrawDiagonalyLines(FilledField, Rectangle.Round(rect));
                            if (FState[i, j].OwnerID == 1)
                                gfx.DrawDiagonalyLines(alphaPenC1, Rectangle.Round(rect));
                            if (FState[i, j].OwnerID == 2)
                                gfx.DrawDiagonalyLines(alphaPenC2, Rectangle.Round(rect));
                        }
                    }

                    // Выделение поля, куда нужно ходить, при попытке пойти нетуда
                    if (IncorrectTurn != null)
                    {
                        gfx.DrawRectangle(pIncorrectTurn, new RectangleF((float)(cx + (w * (IncorrectTurn.x * 3 + 1)) / 11f), (float)(cy + (h * (IncorrectTurn.y * 3 + 1)) / 11f), (float)(w * 3 / 11f), (float)(h * 3 / 11f)));
                    }

                    // Соответствие ячеек и полей
                    if (Settings.Current.HelpShow == 1)
                    {
                        var r1 = new RectangleF(cx + w * (CellUnderMouse.X + 1) / 11f, cy + h * (CellUnderMouse.Y + 1) / 11f, w / 11f, h / 11f);
                        var r2 = new RectangleF(cx + w * (((CellUnderMouse.X % 3 * 3) / 3 * 3) + 1) / 11f, cy + h * (((CellUnderMouse.Y % 3 * 3) / 3 * 3) + 1) / 11f, 3 * w / 11f, 3 * h / 11f);
                        gfx.DrawRectangle(HelpPen, r1);
                        gfx.DrawRectangle(HelpPen, r2);
                        gfx.DrawLine(HelpLinesPen, r1.Location, r2.Location);
                        gfx.DrawLine(HelpLinesPen, r1.Left, r1.Bottom, r2.Left, r2.Bottom);
                        gfx.DrawLine(HelpLinesPen, r1.Right, r1.Bottom, r2.Right, r2.Bottom);
                        gfx.DrawLine(HelpLinesPen, r1.Right, r1.Top, r2.Right, r2.Top);
                    }

                    // Рендеринг полученной графики
                    BufGFX.Render();
                });
        }
        
        // Создание новой игры
        private void NewGame()
        {
            // Ввод настроек для новой игры
            WindowSingleStart wnd = new WindowSingleStart();
            if (wnd.ShowDialog() != true)
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
            pl1 = wnd.textBoxPlayer1.Text;
            pl2 = wnd.textBoxPlayer2.Text + (wnd.checkBoxPlayWithComputer.IsChecked == true ? " [Бот]" : "");

            penc1 = new Pen(wnd.RectColor1.GetShapeColor());
            penc2 = new Pen(wnd.RectColor2.GetShapeColor());
            labelCurrentTurn.Content = pl1;
            var BotLevel = wnd.comboBoxLevel.SelectedIndex + 1;

            // Создание GameManeger'a
            if (wnd.checkBoxPlayWithComputer.IsChecked == true)
            {
                game = new GameManagerWithBot(pl1, pl2, BotLevel);
                Bot = (game as GameManagerWithBot).Bot;
            }
            else
            {
                game = new GameManager(pl1, pl2);
            }

            // Подписка на события новой игры
            game.ChangeTurn += Game_ChangeTurn;
            game.IncorrectTurn += Game_IncorrectTurn;
            game.SomebodyWins += Game_SomebodyWins;
            game.NobodyWins += Game_NobodyWins;

            // Перерисовка графики и включение кнопок сохранения и загрузки
            RedrawGame(true);
            buttonLoadGame.IsEnabled = true;
            buttonSaveGame.IsEnabled = true;
        }

        // Обработка игровых событий
        private void Game_NobodyWins(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("Игра окончена. Ничья");
            buttonSaveGame.IsEnabled = false;
        }
        private void Game_SomebodyWins(object sender, Game.GameEndArgs e)
        {
            System.Windows.MessageBox.Show("Игра окончена.\nПобедитель: " + e.Winner.Name);
            buttonSaveGame.IsEnabled = false;
        }
        private void Game_ChangeTurn(object sender, Player e)
        {
            ((System.Windows.Window)this).Dispatcher.Invoke(delegate
            {
                // Перерисовка
                RedrawGame();
                labelCurrentTurn.Content = e.Name;

                // Запуск таймера хода бота
                if (game is GameManagerWithBot)
                    (game as GameManagerWithBot).BotTurn();
            });
        }

        private void Game_IncorrectTurn(object sender, Position e)
        {
            if (Settings.Current.GraphicsLevel == 2)
                IncorrectTurnAlpha = 255;
            if (e != null)
                IncorrectTurn = e;
        }

        // События взаимодействия пользователя с формой
        private void buttonNewGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NewGame();
        }
        private void buttonLoadGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Открытие файла
            var OpenDialog = new Microsoft.Win32.OpenFileDialog();
            OpenDialog.Filter = "Tic Tac Toe Save File|*.ttts|Все файлы|*.*";
            if (OpenDialog.ShowDialog() == true)
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
                        game = new GameManager(pl1, pl2);
                    else if (type.Substring(0, 2) == "WB")
                    {
                        game = new GameManagerWithBot(pl1, pl2, int.Parse(type[3].ToString()));
                        Bot = (game as GameManagerWithBot).Bot;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Не определён тип игры");
                        game.Dispose();
                        return;
                    }
                    var c1 = sr.ReadLine();
                    var c2 = sr.ReadLine();
                    int i1, i2;
                    if (!int.TryParse(c1, out i1) || !int.TryParse(c2, out i2))
                    {
                        System.Windows.MessageBox.Show("Не удалось считать числовые значения из файла сохранения");
                        game.Dispose();
                        return;
                    }
                    penc1 = new Pen(Color.FromArgb(i1));
                    penc2 = new Pen(Color.FromArgb(i2));
                    game.Load(sr.ReadLine());
                }

                // Подписываемся на события
                labelCurrentTurn.Content = pl1;
                game.ChangeTurn += Game_ChangeTurn;
                game.IncorrectTurn += Game_IncorrectTurn;
                game.SomebodyWins += Game_SomebodyWins;
                game.NobodyWins += Game_NobodyWins;
                buttonSaveGame.IsEnabled = true;
            }
            RedrawGame(true);
        }
        private void buttonSaveGame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Если игры нет то вываливаемся
            if (game == null)
                return;

            // Ход НЕ БОТА
            if (game.CurrentPlayer.Id == 2 && game is GameManagerWithBot)
                return;

            // Сохраняем файл
            var SaveDialog = new Microsoft.Win32.SaveFileDialog();
            SaveDialog.Filter = "Tic Tac Toe Save File|*.ttts|Все файлы|*.*";
            if (SaveDialog.ShowDialog() == true)
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
        private void buttonSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (new WindowSettings()).ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopingRedrawing = true;

            // Спрашиваем подтверждение на выход
            // (Ибо НЕКОТОРЫЕ не хотят проиграть и кликают по крестику хд)
            if (game != null)
                if (System.Windows.MessageBox.Show("Вы действительно хотите выйти?", "Выход", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    StopingRedrawing = false;
                    ViewRefreshing = Task.Run((Action)GameRedrawing);
                    return;
                }

            // Сохранять тут и при запуске новой игры спрашивать, восстановить ли предыдущую
            game?.Dispose();
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var p = MouseOnGameBoard();
            if (p != CellUnderMouse && p.X >= 0 && p.Y >= 0 && p.X < 9 && p.Y < 9)
            {
                CellUnderMouse = p;
                MouseMovedToAnotherCell(this, new EventArgs());
            }
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Создание новой игры если игра не была создана
            if (game == null)
                NewGame();
            else // Иначе выполнение хода
            {
                var p = MouseOnGameBoard();
                RedrawGame();
                if (Settings.Current.GraphicsLevel < 2)
                    IncorrectTurn = null;
                game.ClickOn(p.X, p.Y);
            }
        }

        private void Window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            DontRedraw = true;
            timerResizing.Stop();
            timerResizing.Start();
            RedrawGame(true);
        }

        private Point MouseOnGameBoard()
        {
            var m = Mouse.GetPosition(canvas);
            //var p = Point.Subtract(new Point((int)m.X, (int)m.Y), new Size((int)canvas.Margin.Left, (int)canvas.Margin.Top));

            var x = (int)Math.Round(m.X);
            var y = (int)Math.Round(m.Y);
            if (x < 0 || x > canvas.ActualWidth || y < 0 || y > canvas.ActualHeight)
                return new Point(-1, -1);
            x = (int)Math.Floor(x * 11f / canvas.ActualWidth - 1);
            y = (int)Math.Floor(y * 11f / canvas.ActualHeight - 1);

            return new Point(x, y);
        }
    }
}
