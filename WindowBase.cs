
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using TTTM;

namespace Tic_Tac_Toe_WPF_Remake
{
    public class WindowBase : Window
    {
        protected Position IncorrectTurn;
        protected GameManager game;
        protected BufferedGraphicsContext context = BufferedGraphicsManager.Current;
        protected BufferedGraphics BufGFX;
        protected Graphics gfx;
        protected Pen penc1, penc2;
        protected string pl1, pl2;
        protected Rectangle[,] Zones = new Rectangle[9, 9];
        protected Rectangle[,] FieldZones = new Rectangle[3, 3];
        protected System.Drawing.Point CellUnderMouse;
        protected event EventHandler MouseMovedToAnotherCell;
        protected Task ViewRefreshing;
        protected System.Timers.Timer timerResizing = new System.Timers.Timer(200);
        protected int IncorrectTurnAlpha = 255;
        protected int HelpAlpha = 255;
        protected bool Redrawing;
        protected bool StopingRedrawing, DontRedraw = true;
        protected Canvas canvas;

        public WindowBase()
        {
            ViewRefreshing = Task.Run((Action)GameRedrawing);
            timerResizing.Elapsed += TimerResizing_Tick;
            this.MouseMovedToAnotherCell += delegate { HelpAlpha = 255; };
        }

        protected System.Drawing.Point MouseOnGameBoard()
        {
            var m = Mouse.GetPosition(canvas);
            var x = (int)Math.Round(m.X);
            var y = (int)Math.Round(m.Y);
            if (x < 0 || x > canvas.ActualWidth || y < 0 || y > canvas.ActualHeight)
                return new System.Drawing.Point(-1, -1);
            x = (int)Math.Floor(x * 11f / canvas.ActualWidth - 1);
            y = (int)Math.Floor(y * 11f / canvas.ActualHeight - 1);

            return new System.Drawing.Point(x, y);
        }

        protected void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var p = MouseOnGameBoard();
            if (p != CellUnderMouse && p.X >= 0 && p.Y >= 0 && p.X < 9 && p.Y < 9)
            {
                CellUnderMouse = p;
                MouseMovedToAnotherCell(this, new EventArgs());
            }
        }

        private void TimerResizing_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            RedrawGame(true);
            DontRedraw = false;
        }

        protected void GameRedrawing()
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

        protected void Window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            DontRedraw = true;
            timerResizing.Stop();
            timerResizing.Start();
            RedrawGame(true);
        }

        protected void RedrawGame(bool refreshGraphics = false)
        {
            this.Dispatcher.Invoke(
                delegate
                {
                    if (!this.IsVisible)
                        return;

                    if (canvas == null || (gfx == null && !refreshGraphics))
                        return;

                    // Обновление графики (нужно при ресайзе)
                    if (refreshGraphics)
                    {
                        var hwndsource = (HwndSource)HwndSource.FromVisual(canvas);
                        var pnt = new System.Drawing.Point((int)canvas.Margin.Left, (int)canvas.Margin.Top);
                        var sz = new System.Drawing.Size((int)canvas.ActualWidth, (int)canvas.ActualHeight);
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
                    gfx.FillRectangle(Background, 0, 0, cx+w, cy+h);

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
    }
}
