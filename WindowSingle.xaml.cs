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
    public partial class WindowSingle : WindowBase
    {
        IBot Bot;
        //Position IncorrectTurn;
        //GameManager game;
        //private BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        //BufferedGraphics BufGFX;
        //Graphics gfx;
        //Pen penc1, penc2;
        //string pl1, pl2;
        //Rectangle[,] Zones = new Rectangle[9, 9];
        //Rectangle[,] FieldZones = new Rectangle[3, 3];
        //Point CellUnderMouse;
        //System.Timers.Timer timerRefreshView = new System.Timers.Timer(50);
        //Task ViewRefreshing;
        //System.Timers.Timer timerResizing = new System.Timers.Timer(200);
        //int IncorrectTurnAlpha = 255;
        //int HelpAlpha = 255;
        //bool Redrawing;
        //bool StopingRedrawing, DontRedraw = true;

        public WindowSingle() : base()
        {
            InitializeComponent();
            base.canvas = canvas;
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
            var BotLevel = (int)Math.Round(wnd.sliderBotLevel.Value);

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
                        game = new GameManagerWithBot(pl1, pl2, int.Parse(type.Substring(2)));
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
                        sw.Write((Bot as RecursionAnalizerBot).Level.ToString() + "\n");
                    }
                    else
                        sw.Write("WF \n");
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

        protected void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
    }
}
