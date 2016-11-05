using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TTTM;

namespace Tic_Tac_Toe_WPF_Remake
{
    /// <summary>
    /// Логика взаимодействия для WindowMulpiplayer.xaml
    /// </summary>
    public partial class WindowMultiplayer : WindowBase
    {
        bool WantsToRestart = false;

        public WindowMultiplayer(string MyNick, string OpponentNick, System.Drawing.Color MyColor, System.Drawing.Color OpponentColor) : base()
        {
            base.canvas = this.canvas;
            InitializeComponent();

            Connection.Current.ReceivedChat += Connection_ReceivedChat;
            Connection.Current.ReceivedTurn += Connection_ReceivedTurn;
            Connection.Current.GameEnds += Connection_GameEnds;
            Connection.Current.OpponentDisconnected += Connection_AnotherPlayerDisconnected;
            Connection.Current.RestartGame += Connection_ReceivedRestartGame;
            Connection.Current.RestartRejected += Connection_RestartRejected;
            if (Connection.Current.Role == Connection.NetworkRole.Server)
            {
                game = new GameManager(MyNick, OpponentNick); // Я - первый, Он - второй
                this.Title += " (Ваш ход)";
                penc1 = new Pen(MyColor);
                penc2 = new Pen(OpponentColor);
            }
            else
            {
                game = new GameManager(OpponentNick, MyNick); // Я - второй, Он - первый
                penc1 = new Pen(OpponentColor);
                penc2 = new Pen(MyColor);
            }
            labelCurrentTurn.Content = game.CurrentPlayer.Name;

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

            Dispatcher.Invoke(act);
        }

        private void Connection_ReceivedRestartGame(object sender, EventArgs e)
        {
            Action act = delegate
            {
                if (!WantsToRestart)
                {
                    if (MessageBox.Show("Хотите начать игру сначала?", "Противник предложить начать игру заного", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        RestartGame();
                        Connection.Current.SendStartGame();
                    }
                    else
                    {
                        Connection.Current.SendReject();
                    }
                }
                else
                {
                    WantsToRestart = false;
                    RestartGame();
                }
            };

            Dispatcher.Invoke(act);
        }

        private void RestartGame()
        {
            // Отписка от событий старой игры
            game.ChangeTurn -= Game_ChangeTurn;
            game.IncorrectTurn += Game_IncorrectTurn;
            game.SomebodyWins += Game_SomebodyWins;
            game.NobodyWins += Game_NobodyWins;

            game.Dispose();
            if (Connection.Current.Role == Connection.NetworkRole.Server)
                game = new GameManager(pl1, pl2); // Я - первый, Он - второй
            else
                game = new GameManager(pl2, pl1); // Я - второй, Он - первый

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
                this.Closing -= Window_Closing;
                textBoxChatInput.IsEnabled = false;
                buttonRestart.IsEnabled = false;
            };

            Dispatcher.Invoke(act);
        }

        private void Connection_GameEnds(object sender, EventArgs e)
        {
            Action act = delegate
            {
                AddMessageToChat("System", "Противник вышел из игры");
                game.Dispose();
                Connection.Current.OpponentDisconnected -= Connection_AnotherPlayerDisconnected;
                Connection.Current.BreakAnyConnection();
                this.Closing -= Window_Closing;
                textBoxChatInput.IsEnabled = false;
                buttonRestart.IsEnabled = false;
            };

            Dispatcher.Invoke(act);
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
            if (Settings.Current.GraphicsLevel == 2)
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
                labelCurrentTurn.Content = e.Name;
                if (e.Name == pl1)
                {
                    this.Title += " (Ваш ход)";
                    if (this.WindowState == WindowState.Minimized)
                        this.Activate();
                }
                else
                    this.Title = "Мультиплеерная игра";
            };

            Dispatcher.Invoke(act);
        }

        #region Сетевое взаимодействие и чат
        private void Connection_ReceivedTurn(object sender, Connection.ReceivedTurnEventArgs e)
        {
            game.ClickOn(e.Turn.x, e.Turn.y);
        }
        private void Connection_ReceivedChat(object sender, string e)
        {
            Action d = new Action(delegate
            {
                AddMessageToChat(pl2, e);
            });

            Dispatcher.Invoke(d);
        }
        private void AddMessageToChat(string PlayerName, string Text)
        {
            TextRange tr = new TextRange(richTextBoxChat.Document.ContentEnd, richTextBoxChat.Document.ContentEnd);
            tr.Text = '[' + DateTime.Now.ToShortTimeString() + "] ";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gray);
            
            /*
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
            richTextBoxChat.ScrollToCaret();*/
        }
        private void textBoxChatInput_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key ==  Key.Enter && !string.IsNullOrWhiteSpace(textBoxChatInput.Text))
            {
                AddMessageToChat(pl1, textBoxChatInput.Text);
                Connection.Current.SendChat(textBoxChatInput.Text);
                textBoxChatInput.Clear();
                e.Handled = true;
            }
        }
        private void textBoxChatInput_Leave(object sender, RoutedEventArgs e)
        {
            textBoxChatInput.Foreground = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 128, G = 128, B = 128 });
            if (textBoxChatInput.Text == "")
                textBoxChatInput.Text = "Сообщение сопернику";
        }
        private void textBoxChatInput_Enter(object sender, RoutedEventArgs e)
        {
            textBoxChatInput.Foreground = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { A = 255, R = 0, G = 0, B = 0 });
            if (textBoxChatInput.Text == "Сообщение сопернику")
                textBoxChatInput.Text = "";
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                e.Cancel = true;
            else
            {
                Connection.Current.SendEndGame();
                Connection.Current.BreakAnyConnection();
                Connection.Current.ReceivedChat -= Connection_ReceivedChat;
                Connection.Current.ReceivedTurn -= Connection_ReceivedTurn;
                Connection.Current.GameEnds -= Connection_GameEnds;
                Connection.Current.OpponentDisconnected -= Connection_AnotherPlayerDisconnected;
                Connection.Current.RestartGame -= Connection_ReceivedRestartGame;
                Connection.Current.RestartRejected -= Connection_RestartRejected;
            }
        }
    }
}
