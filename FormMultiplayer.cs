﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    public partial class FormMultiplayer : Form
    {
        /* TODO:
         * Добавить обработку обрыва соединения
         * Вынести код из MP и SP в обдельный класс (может быть создать родительскую форму?)
         */

        Settings settings;
        Connection2 connection;
        string MyNick, OpponentNick;
        Pen penc1, penc2;
        GameManagerWthFriend game;
        Position IncorrectTurn;
        private BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        BufferedGraphics BufGFX;
        Graphics gfx;
        Rectangle[,] Zones = new Rectangle[9, 9];
        Rectangle[,] FieldZones = new Rectangle[3, 3];
        Point CellUnderMouse;
        event EventHandler MouseMovedToAnotherCell;
        bool WantsToRestart = false;
        // Для графики > 1
        int IncorrectTurnAlpha = 255;
        int HelpAlpha = 255;

        // Конструктор формы
        public FormMultiplayer(Settings settings, Connection2 connection, string MyNick, string OpponentNick, Color MyColor, Color OpponentColor)
        {
            // Инициализация
            InitializeComponent();
            this.settings = settings;
            this.connection = connection;
            this.MyNick = MyNick;
            this.OpponentNick = OpponentNick;
            richTextBoxChat.ReadOnly = true;
            richTextBoxChat.BackColor = SystemColors.Window;
            connection.ReceivedChat += Connection_ReceivedChat;
            connection.ReceivedTurn += Connection_ReceivedTurn;
            connection.GameEnds += Connection_GameEnds;
            connection.OpponentDisconnected += Connection_AnotherPlayerDisconnected;
            connection.RestartGame += Connection_ReceivedRestartGame;
            connection.RestartRejected += Connection_RestartRejected;
            if (connection.Role == Connection2.NetworkRole.Server)
            {
                game = new GameManagerWthFriend(MyNick, OpponentNick); // Я - первый, Он - второй
                this.Text += " (Ваш ход)";
                penc1 = new Pen(MyColor);
                penc2 = new Pen(OpponentColor);
            }
            else
            {
                game = new GameManagerWthFriend(OpponentNick, MyNick); // Я - второй, Он - первый
                penc1 = new Pen(OpponentColor);
                penc2 = new Pen(MyColor);
            }
            labelCurrentTurn.Text = game.CurrentPlayer.Name;

            if (settings.GraphicsLevel == 2)
            {
                this.MouseMovedToAnotherCell += delegate { HelpAlpha = 255; };
                timerRefreshView.Start();
            }
            else
            {
                this.MouseMovedToAnotherCell += delegate { RedrawGame(); };
            }
            // Подписка на события новой игры
            game.ChangeTurn += Game_ChangeTurn;
            game.IncorrectTurn += Game_IncorrectTurn;
            game.SomebodyWins += Game_SomebodyWins;
            game.NobodyWins += Game_NobodyWins;

            // Перерисовка графики и включение кнопок сохранения и загрузки
            RedrawGame(true);
        }

        private void Connection_RestartRejected(object sender, EventArgs e)
        {
            Action act = delegate
            {
                AddMessageToChat("System", "Противник отказался начинать игру заного");
                WantsToRestart = false;
            };

            this.Invoke(act);
        }

        private void Connection_ReceivedRestartGame(object sender, EventArgs e)
        {
            Action act = delegate
            {
                if (!WantsToRestart)
                {
                    if (MessageBox.Show("Хотите начать игру сначала?", "Противник предложить начать игру заного", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        RestartGame();
                        connection.SendStartGame();
                    }
                    else
                    {
                        connection.SendReject();
                    }
                }
                else
                {
                    WantsToRestart = false;
                    RestartGame();
                }
            };

            this.Invoke(act);
        }

        private void RestartGame()
        {
            // Отписка от событий старой игры
            game.ChangeTurn -= Game_ChangeTurn;
            game.IncorrectTurn += Game_IncorrectTurn;
            game.SomebodyWins += Game_SomebodyWins;
            game.NobodyWins += Game_NobodyWins;

            game.Dispose();
            if (connection.Role == Connection2.NetworkRole.Server)
                game = new GameManagerWthFriend(MyNick, OpponentNick); // Я - первый, Он - второй
            else
                game = new GameManagerWthFriend(OpponentNick, MyNick); // Я - второй, Он - первый

            // Подписка на события новой игры
            game.ChangeTurn += Game_ChangeTurn;
            game.IncorrectTurn += Game_IncorrectTurn;
            game.SomebodyWins += Game_SomebodyWins;
            game.NobodyWins += Game_NobodyWins;

            // Перерисовка графики и включение кнопок сохранения и загрузки
            RedrawGame(true);
        }

        private void Connection_AnotherPlayerDisconnected(object sender, EventArgs e)
        {
            Action act = delegate
            {
                AddMessageToChat("System", "Соединение разорвано");
                this.FormClosing -= FormMultiplayer_FormClosing;
                textBoxChatInput.Enabled = false;
                buttonRestart.Enabled = false;
            };

            this.Invoke(act);
        }

        private void Connection_GameEnds(object sender, EventArgs e)
        {
            Action act = delegate
            {
                AddMessageToChat("System", "Противник вышел из игры");
                game.Dispose();
                connection.OpponentDisconnected -= Connection_AnotherPlayerDisconnected;
                //connection.Disconnect();
                connection.BreakAnyConnection();
                this.FormClosing -= FormMultiplayer_FormClosing;
                textBoxChatInput.Enabled = false;
                buttonRestart.Enabled = false;
            };

            this.Invoke(act);
        }
        private void timerRefreshView_Tick(object sender, EventArgs e)
        {
            if (IncorrectTurnAlpha > 0)
                IncorrectTurnAlpha -= 5;
            if (HelpAlpha > 0)
                HelpAlpha -= 5;
            RedrawGame();
        }

        // Игровые события
        private void Game_NobodyWins(object sender, EventArgs e)
        {
            AddMessageToChat("System", "Игра окончена. Ничья");
        }
        private void Game_SomebodyWins(object sender, Game.GameEndArgs e)
        {
            AddMessageToChat("System", "Игра окончена.\r\n   Победитель: " + e.Winner.Name);
        }
        private void Game_IncorrectTurn(object sender, Position e)
        {
            if (settings.GraphicsLevel == 2)
                IncorrectTurnAlpha = 255;
            if (e != null)
                IncorrectTurn = e;
        }
        private void Game_ChangeTurn(object sender, Player e)
        {
            // Перерисовка
            Action act = delegate
            {
                RedrawGame();
                labelCurrentTurn.Text = e.Name;
                if (e.Name == MyNick)
                {
                    this.Text += " (Ваш ход)";
                    if (this.WindowState == FormWindowState.Minimized)
                        this.Activate();
                }
                else
                    this.Text = "Мультиплеерная игра";
            };

            this.Invoke(act);
        }

        // Перерисовка игрового поля
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
            Pen pIncorrectTurn = new Pen(Color.FromArgb(IncorrectTurnAlpha, settings.IncorrectTurn), 4);
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

        #region Сетевое взаимодействие и чат
        private void Connection_ReceivedTurn(object sender, Connection2.ReceivedTurnEventArgs e)
        {
            game.ClickOn(e.Turn.x, e.Turn.y);
        }
        private void Connection_ReceivedChat(object sender, string e)
        {
            Action d = new Action(delegate
            {
                AddMessageToChat(OpponentNick, e);
            });

            Invoke(d);
        }
        private void AddMessageToChat(string PlayerName, string Text)
        {
            richTextBoxChat.SelectionStart = richTextBoxChat.TextLength;
            richTextBoxChat.SelectionLength = 0;
            richTextBoxChat.SelectionColor = Color.Gray;
            richTextBoxChat.AppendText('[' + DateTime.Now.ToShortTimeString() + "] ");

            richTextBoxChat.SelectionColor = Color.Black;
            
            if (Text.StartsWith("/me "))
            {
                richTextBoxChat.SelectionFont = new Font(richTextBoxChat.SelectionFont, FontStyle.Italic);
                richTextBoxChat.AppendText(" *** " + PlayerName + Text.Substring(3) + " ***\r\n");
            }
            else
            {
                richTextBoxChat.SelectionFont = new Font(richTextBoxChat.SelectionFont, FontStyle.Bold);
                richTextBoxChat.AppendText(PlayerName + ": ");
                richTextBoxChat.SelectionFont = new Font(richTextBoxChat.SelectionFont, 0);
                richTextBoxChat.AppendText(Text + "\r\n");
            }


            richTextBoxChat.ReadOnly = false;
            while (richTextBoxChat.Text.Contains("#луна"))
            {
                int ind = richTextBoxChat.Text.IndexOf("#луна");
                richTextBoxChat.Select(ind, "#луна".Length);
                Clipboard.SetImage((Image)Properties.Resources.SmileMoon);
                richTextBoxChat.Paste();
            }
            richTextBoxChat.ReadOnly = true;
            richTextBoxChat.BackColor = SystemColors.Window;

            richTextBoxChat.SelectionStart = richTextBoxChat.Text.Length;
            richTextBoxChat.ScrollToCaret();
        }
        private void textBoxChatInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && !string.IsNullOrWhiteSpace(textBoxChatInput.Text))
            {
                AddMessageToChat(MyNick, textBoxChatInput.Text);
                connection.SendChat(textBoxChatInput.Text);
                textBoxChatInput.Clear();
                e.Handled = true;
            }
        }
        private void textBoxChatInput_Leave(object sender, EventArgs e)
        {
            textBoxChatInput.ForeColor = Color.Gray;
            if (textBoxChatInput.Text == "")
                textBoxChatInput.Text = "Сообщение сопернику";
        }
        private void textBoxChatInput_Enter(object sender, EventArgs e)
        {
            textBoxChatInput.ForeColor = Color.Black;
            if (textBoxChatInput.Text == "Сообщение сопернику")
                textBoxChatInput.Text = "";
        }
        #endregion

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

        private void FormMultiplayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход", MessageBoxButtons.YesNo) != DialogResult.Yes)
                e.Cancel = true;
            else
            {
                connection.SendEndGame();
                //connection.Disconnect();
                connection.BreakAnyConnection();
                connection.ReceivedChat -= Connection_ReceivedChat;
                connection.ReceivedTurn -= Connection_ReceivedTurn;
                connection.GameEnds -= Connection_GameEnds;
                connection.OpponentDisconnected -= Connection_AnotherPlayerDisconnected;
                connection.RestartGame -= Connection_ReceivedRestartGame;
                connection.RestartRejected -= Connection_RestartRejected;
            }
        }

        private void FormMultiplayer_Paint(object sender, PaintEventArgs e)
        {
            RedrawGame();
        }
        private void FormMultiplayer_ResizeEnd_And_SizeChanged(object sender, EventArgs e)
        {
            // Чтоб при разворачивании окна оно сперва обновило размер элементов, а затем уже прорисовало графику на них
            if (WindowState == FormWindowState.Maximized)
                Refresh();

            RedrawGame(true);
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            connection.SendStartGame();
            WantsToRestart = true;
            buttonRestart.Enabled = false;
            timerRefreshRestart.Start();
        }

        private void timerRefreshRestart_Tick(object sender, EventArgs e)
        {
            buttonRestart.Enabled = true;
            timerRefreshRestart.Stop();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (connection.Role == Connection2.NetworkRole.Server ^ game.CurrentPlayer.Id == 1)
                return;

            var p = MouseOnGameBoard();
            RedrawGame();
            if (settings.GraphicsLevel < 2)
                IncorrectTurn = null;

            connection.SendTurn(new Position(p.X, p.Y));
            game.ClickOn(p.X, p.Y);
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
    }
}
