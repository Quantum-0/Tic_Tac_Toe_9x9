using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TTTM
{
    /*
     * сделать штриховку полупрозрачной
     * звук приходящих сообщений
     * ДОБАВИТЬ ЗВУКИ
     * разделить класс гейм хд
     * 
     * 
     * добавить правила к игре, чтоб убрать гарантированный способ победить в самом начале игры
     * 1)ставим в ячейке ход обратно в неё
     * 2)до заполнения кидаем туда противника
     * 3)когда кончаются ячейик - ходим в центр
     * 4)повторяем до заполнения
     * 5)провоцируем дать ход в любое место и закрываем чтоб не дать 3 в ряд
     * 6)доигрываем игру
     */

    

    

    // Класс настроек
    public class Settings
    {
        public Color BackgroundColor;
        public Color SmallGrid;
        public Color BigGrid;
        public Color PlayerColor1;
        public Color PlayerColor2;
        public Color IncorrectTurn;
        public Color FilledField = Color.Gray;
        public string DefaultName1 = "";
        public string DefaultName2 = "";
        public string MpIP = "";
        public int MpPort = 0;
        public Color HelpColor;
        public int HelpCellsAlpha;
        public int HelpLinesAlpha;
        public int HelpShow; //(bool)
        public int GraphicsLevel;


        // Сохранение настроек в файл
        public static bool Save(string fname, Settings settings)
        {
            // Выход если настроек нет
            if (settings == null)
                return false;

            using (StreamWriter sw = new StreamWriter(fname))
            {
                // Заголовок файла
                sw.WriteLine("[Tic Tac Toe Settings]");
                // Получаем массив полей класса Settings
                FieldInfo[] fis = typeof(Settings).GetFields();
                // Проходим по всем полям
                foreach (var fi in fis)
                {
                    // Записываем "Имя поля = "
                    sw.Write(fi.Name + "=");
                    // Записываем значение в зависимости от его типа
                    object val = fi.GetValue(settings);
                    if (val is String || val is int)
                        sw.WriteLine(val);
                    else if (val is Color)
                        sw.WriteLine(((Color)val).ToArgb());
                    else if (val == null)
                        throw new Exception("Попытка сохранить настройки с полями со значениями null");
                }
                return true;
            }
        }
        public static bool Load(string fname, out Settings settings)
        {
            // Создаём объект настроек, в любом случае пригодится, да и возвращать что-то надо
            settings = new Settings();

            // Выход если файл отсутствует
            if (!File.Exists(fname))
            {
                settings.SetDefaults();
                return false;
            }

            using (StreamReader sr = new StreamReader(fname))
            {
                // Пропускаем заголовок
                sr.ReadLine();
                // Читаем строки до конца файла
                while (!sr.EndOfStream)
                {
                    string str = sr.ReadLine();

                    // Вытаскиваем из строки название поля и его значение
                    string field = str.Substring(0, str.IndexOf('='));
                    dynamic val = str.Substring(str.IndexOf('=') + 1);

                    // Color
                    if (typeof(Settings).GetFields().First(f => f.Name == field).FieldType == typeof(Color))
                    {
                        int temp;
                        int.TryParse(val, out temp);
                        val = Color.FromArgb(temp);
                    }
                    else // int
                    if (typeof(Settings).GetFields().First(f => f.Name == field).FieldType == typeof(int))
                    {
                        int temp;
                        int.TryParse(val, out temp);
                        val = temp;
                    }

                    // Записываем значение в настройки
                    typeof(Settings).GetFields().First(f => f.Name == field).SetValue(settings, val);
                }
                return true;
            }
        }

        public void SetDefaults()
        {
            BackgroundColor = Color.Black;
            SmallGrid = Color.FromArgb(0, 200, 0);
            BigGrid = Color.Lime;
            PlayerColor1 = Color.Red;
            PlayerColor2 = Color.FromArgb(100, 100, 255);
            IncorrectTurn = Color.Orange;
            FilledField = Color.Gray;
            DefaultName1 = "Игрок 1";
            DefaultName2 = "Игрок 2";
            MpIP = "127.0.0.1";
            MpPort = 7890;
            HelpColor = Color.Aqua;
            HelpCellsAlpha = 180;
            HelpLinesAlpha = 40;
            HelpShow = 1;
            GraphicsLevel = 2;
        }
    }

    // Класс координат для игры
    public class Position
    {
        // Свойства
        public int x { private set; get; }
        public int y { private set; get; }

        // Преобразование координаты ячейки/поля
        public static Position GetCellFrom9x9(Position pos)
        {
            return pos % 3;
        }
        public static Position GetFieldFrom9x9(Position pos)
        {
            return pos / 3;
        }

        // Конструктор
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Переопределение операторов %, /, *, метода сравнения и получения хэш-кода
        public override bool Equals(object another)
        {
            if (another is Position)
                return x == ((Position)another).x && y == ((Position)another).y;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static Position operator% (Position pos, int number)
        {
            return new Position(pos.x % number, pos.y % number);
        }
        public static Position operator/ (Position pos, int number)
        {
            return new Position(pos.x / number, pos.y / number);
        }
        public static Position operator* (Position pos, int number)
        {
            return new Position(pos.x * number, pos.y * number);
        }
    }

    // Класс хранящий состояние поля 3х3
    public struct FieldState
    {
        public Player Owner;
        public int OwnerID;
        public bool Filled;

        public FieldState(Player Owner, bool Filled)
        {
            this.Owner = Owner;
            OwnerID = Owner?.Id ?? 0;
            this.Filled = Filled;
        }
    }

    // Менеджер для одиночной игры с ботом
    public class GameManagerWithBot : GameManagerWthFriend
    {
        public ABot Bot { private set; get; }
        public override event EventHandler<Player> ChangeTurn;
        public override event EventHandler<Position> IncorrectTurn;
        public override event EventHandler NobodyWins;
        public GameManagerWithBot(string player1, string player2, int botType = 3) : base(player1, player2)
        {
            // BASE CTOR HERE
            switch (botType)
            {
                case 1:
                    Bot = new StupidBot(Player2, game);
                    break;
                case 2:
                    Bot = new SomeMoreCleverBot(Player2, Player1, game);
                    break;
                case 3:
                    Bot = new RecursionAnalizerBot(Player2, Player1, game);
                    break;
                default:
                    throw new Exception("Не удалось создать бота");
            }
        }

        public async void BotTurn()
        {
            // Вылет если ход не бота
            if (CurrentPlayer != Player2)
                return;

            //await Task.Run((Action)Bot.makeTurn);
            Bot.makeTurn();
            CurrentPlayer = Player1;

            // Вызов события об обновлении хода
            ChangeTurn(this, CurrentPlayer);
        }

        public override void ClickOn(int i, int j)
        {
            // Вылет если игра закончена
            if (game.Finished)
                return;

            // Вылет если сейчас ход бота
            if (CurrentPlayer != Player1)
                return;

            // Выполнение хода
            bool res = game.Turn(new Position(i, j), CurrentPlayer);

            // Если ход выполнен успешно (ячейка не занята и ход туда разрешён) - свап игрока
            if (res)
            {
                // Передача хода боту
                CurrentPlayer = Player2;

                // Вызов события об обновлении хода
                ChangeTurn(this, CurrentPlayer);
            }
            else
                // Вызов события некорректного хода
                IncorrectTurn?.Invoke(this, game.CurrentField);
        }
    }

    // Абстрактная реализация менеджера игры
    public class GameManagerWthFriend : IDisposable
    {
        // Свойства
        protected Game game;// { private set; get; }
        protected Player Player1;// { private set; get; }
        protected Player Player2;// { private set; get; }
        public Player CurrentPlayer;
        public event EventHandler<Game.GameEndArgs> SomebodyWins;
        public virtual event EventHandler NobodyWins;
        public virtual event EventHandler<Player> ChangeTurn;
        public virtual event EventHandler<Position> IncorrectTurn;

        // Конструктор
        public GameManagerWthFriend(string player1, string player2)
        {
            game = new Game();
            game.StartGame();
            Player1 = new Player(player1);
            Player2 = new Player(player2);
            CurrentPlayer = Player1;
            game.GameEnds += GameEnds;
        }

        public void Dispose()
        {
            Player1?.Dispose();
            Player2?.Dispose();
        }

        // Обработка конца игры
        private void GameEnds(object sender, Game.GameEndArgs e)
        {
            if (e.Winner != null)
                SomebodyWins?.Invoke(this, e);
            else
                NobodyWins?.Invoke(this, new EventArgs());
        }

        // Обработка клика по полю
        public virtual void ClickOn(int i, int j)
        {
            // Вылет если игра закончена
            if (game.Finished)
                return;

            // Выполнение хода
            bool res = game.Turn(new Position(i, j), CurrentPlayer);

            // Если ход выполнен успешно (ячейка не занята и ход туда разрешён) - свап игрока
            if (res)
            {
                if (CurrentPlayer == Player1)
                    CurrentPlayer = Player2;
                else
                    CurrentPlayer = Player1;

                // Вызов события об обновлении хода
                ChangeTurn(this, CurrentPlayer);
            }
            else
                // Вызов события некорректного хода
                IncorrectTurn?.Invoke(this, game.CurrentField);
        }

        // Методы возвращающие состоящие игровых полей и ячеек
        public int[,] State()
        {
            int[,] res = new int[9, 9];
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    res[i, j] = (game[i / 3, j / 3, i % 3, j % 3].Owner?.Id ?? 0);

            return res;
        }
        public FieldState[,] FieldsState()
        {
            FieldState[,] res = new FieldState[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res[i, j] = new FieldState(game[i, j].Owner, game[i, j].Full);

            return res;
        }

        //// Класс, реализующий одиночную игру с другим игроком
        //class SinglePlayerGame : IDisposable
        //{
        //    // Свойства
        //    public Game game { private set; get; }
        //    public Player Player1 { private set; get; }
        //    public Player Player2 { private set; get; }
        //    public Player CurrentPlayer;
        //    public event EventHandler<Game.GameEndArgs> SomebodyWins;
        //    public event EventHandler NobodyWins;
        //    public event EventHandler<Player> ChangeTurn;
        //    public event EventHandler<Position> IncorrectTurn;

        //    // Конструктор
        //    public SinglePlayerGame(string player1, string player2)
        //    {
        //        game = new Game();
        //        game.StartGame();
        //        Player1 = new Player(player1);
        //        Player2 = new Player(player2);
        //        CurrentPlayer = Player1;
        //        game.GameEnds += GameEnds;
        //    }

        //    public void Dispose()
        //    {
        //        Player1?.Dispose();
        //        Player2?.Dispose();
        //    }

        //    // Обработка конца игры
        //    private void GameEnds(object sender, Game.GameEndArgs e)
        //    {
        //        if (e.Winner != null)
        //            SomebodyWins?.Invoke(this, e);
        //        else
        //            NobodyWins?.Invoke(this, new EventArgs());
        //    }

        //    // Обработка клика по полю
        //    public void ClickOn(int i, int j)
        //    {
        //        // Выполнение хода
        //        bool res = game.Turn(new Position(i, j), CurrentPlayer);

        //        // Если ход выполнен успешно (ячейка не занята и ход туда разрешён) - свап игрока
        //        if (res)
        //        {
        //            if (CurrentPlayer == Player1)
        //                CurrentPlayer = Player2;
        //            else
        //                CurrentPlayer = Player1;

        //            // Вызов события об обновлении хода
        //            ChangeTurn(this, CurrentPlayer);
        //        }
        //        else
        //            // Вызов события некорректного хода
        //            IncorrectTurn?.Invoke(this, game.CurrentField);
        //    }

        //    // Методы возвращающие состоящие игровых полей и ячеек
        //    public int[,] State()
        //    {
        //        int[,] res = new int[9, 9];
        //        for (int i = 0; i < 9; i++)
        //            for (int j = 0; j < 9; j++)
        //                res[i,j] = (game[i / 3, j / 3, i % 3, j % 3].Owner?.Id ?? 0);

        //        return res;
        //    }
        //    public FieldState[,] FieldsState()
        //    {
        //        FieldState[,] res = new FieldState[3, 3];
        //        for (int i = 0; i < 3; i++)
        //            for (int j = 0; j < 3; j++)
        //                res[i, j] = new FieldState(game[i, j].Owner, game[i, j].Full);

        //        return res;
        //    }


        // Сохранение/загрузка
        public string Save()
        {
            return game.GetStateCode();
        }
        public void Load(string State)
        {
            game.UpdateFromStateCode(State, Player1, Player2, CurrentPlayer);
            //ChangeTurn(this, CurrentPlayer);
        }
    }

    // Класс содержащий матрицу полей с ячейками, историю ходов и проверяющий корректность хода
    public class Game
    {
        // Свойства
        public bool SilentMode = false;
        public bool Finished = false;
        public GameField[,] Fields { private set; get; }
        public Position CurrentField { private set; get; }
        public List<Position> History { private set; get; } = new List<Position>();

        // Событие конца игры
        public event EventHandler<GameEndArgs> GameEnds;
        public class GameEndArgs : EventArgs
        {
            public Player Winner { private set; get; }
            public GameEndArgs(Player Winner)
            {
                this.Winner = Winner;
            }
        }

        // Состояние полей в виде двумерного массива владельцев
        public Player[,] FieldsState()
        {
            Player[,] res = new Player[3,3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res[i, j] = (Fields[i, j].Owner);

            return res;
        }

        // Индексаторы для обращения к полю/ячейке
        public GameCell this[int i, int j, int k, int l]
        {
            get
            {
                return Fields[i, j].Cells[k, l];
            }
        }
        public GameField this[int i, int j]
        {
            get
            {
                return Fields[i, j];
            }
        }

        //Игровые действия
        public void StartGame()
        {
            Finished = false;
            History.Clear();
            foreach (var item in Fields)
            {
                item.Clear();
            }
        }
        public bool Turn(Position pos, Player player)
        {
            // Игра была закончена
            if (Finished)
                return true;

            // Ход вне диапазона
            if (pos.x < 0 || pos.x > 8 || pos.y < 0 || pos.y > 8)
                return false;

            // Проверка верности хода
            Position Cell = Position.GetCellFrom9x9(pos);
            Position Field = Position.GetFieldFrom9x9(pos);

            bool notThatField = !Field.Equals(CurrentField);
            bool fieldIsNotFull = (CurrentField != null) ? !Fields[CurrentField.x, CurrentField.y].Full : true;

            if (notThatField && CurrentField != null && fieldIsNotFull)
                return false;
            
            // Ход
            if (Fields[Field.x, Field.y][Cell.x, Cell.y].Bind(player))
            {
                History.Add(pos);
                CurrentField = Cell;
                return true;
            }
            else
                return false;
        }

        // Конструктор
        public Game()
        {
            Fields = new GameField[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Fields[i, j] = new GameField(i, j);
                    Fields[i, j].Changed += FieldChanged;
                    Fields[i, j].Filled += FieldFilled;
                }
        }

        // Обработка заполнения и победы поля
        private void FieldFilled(object sender, EventArgs e)
        {
            // Проверка на заполненность
            bool full = true;
            for (int i = 0; i < 3 && full; i++)
                for (int j = 0; j < 3 && full; j++)
                    if (Fields[i, j].Owner == null && !Fields[i,j].Full)
                        full = false;

            if (full && !SilentMode)
            {
                // Конец игры из-за заполненности всех полей
                Finished = true;
                GameEnds?.Invoke(this, new GameEndArgs(null));
            }
        }
        private void FieldChanged(object sender, EventArgs e)
        {
            if (SilentMode)
                return;

            if (checkNeighbors(((GameField)sender).Pos))
            {
                // Конец игры из-за победы одного из игроков
                GameEnds?.Invoke(this, new GameEndArgs(((GameField)sender).Owner));
                Finished = true;
            }
        }

        // Проверка соседних с [i, j] полей на нахождение ряда из трёх
        private bool checkNeighbors(Position Pos)
        {
            int i = Pos.x;
            int j = Pos.y;
            if (Fields[i, 0].Owner == Fields[i, 1].Owner && Fields[i, 0].Owner == Fields[i, 2].Owner && Fields[i, 0].Owner != null)
                return true;
            if (Fields[0, j].Owner == Fields[1, j].Owner && Fields[0, j].Owner == Fields[2, j].Owner && Fields[0, j].Owner != null)
                return true;
            if (((i == 0 || i == 2) && (j == 0 || j == 2)) || (i == 1 && j == 1))
                if (((Fields[0, 0].Owner == Fields[1, 1].Owner && Fields[0, 0].Owner == Fields[2, 2].Owner)
                || (Fields[0, 2].Owner == Fields[1, 1].Owner && Fields[1, 1].Owner == Fields[2, 0].Owner)) && Fields[1, 1].Owner != null)
                    return true;

            return false;
        }

        //Сохранение в строку
        public string GetStateCode()
        {
            StringBuilder res = new StringBuilder();

            // Запись полей
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res.Append(Fields[i, j].Owner?.Id ?? 0);

            // Запись ячеек
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    res.Append(Fields[i / 3, j / 3][i % 3, j % 3].Owner?.Id ?? 0);

            // Последний ход
            if (History.Count == 0)
                res.Append("XX");
            else
            {
                res.Append(History.Last().x);
                res.Append(History.Last().y);
            }

            // ID игрока сделавшего последний ход
            /*if (CurrentField == null)
                res.Append('X');
            else
            {
                var LastPos = History.Last();
                var LastField = LastPos / 3;
                var LastCell = LastPos % 3;
                var lastPlayerID = Fields[LastField.x, LastField.y].Cells[LastCell.x, LastCell.y].Owner.Id;
                res.Append(lastPlayerID);
            }*/

            return res.ToString();
        }
        public bool UpdateFromStateCode(string State, Player p1, Player p2, Player currentPlayer)
        {
            string Backup = GetStateCode();

            if (State.Length != 92)
                return false;

            foreach (var item in Fields)
            {
                item.Clear();
            }

            try
            {
                int k = 0;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++, k++)
                    {
                        if (State[k] == '1')
                            Fields[i, j]._Bind(p1);
                        if (State[k] == '2')
                            Fields[i, j]._Bind(p2);
                    }

                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++, k++)
                    {
                        if (State[k] == '1')
                            Fields[i / 3, j / 3][i % 3, j % 3].Bind(p1);
                        if (State[k] == '2')
                            Fields[i / 3, j / 3][i % 3, j % 3].Bind(p2);
                    }

                History.Clear();

                if (State[90] == 'X')
                    CurrentField = null;
                else
                {
                    var LastPos = new Position(int.Parse(State[90].ToString()), int.Parse(State[91].ToString()));
                    CurrentField = LastPos % 3;
                    if (Fields[(LastPos / 3).x, (LastPos / 3).y].Cells[CurrentField.x, CurrentField.y].Owner?.Id == 2)
                        currentPlayer = p2;
                    else
                        currentPlayer = p1;
                }

                Finished = false;
                return true;
            }
            catch
            {
                UpdateFromStateCode(Backup, p1, p2, currentPlayer);
                return false;
            }
        }
    }

    // Класс игрока
    public class Player : IDisposable
    {
        // Счётчик созданных игроков для присвоения уникального идентификатора 
        private static int Players = 0;

        // Свойства
        public int Id { private set; get; }
        public string Name { private set; get; }

        // Конструктор
        public Player(string Name)
        {
            Id = ++Players;
            this.Name = Name;
        }

        // Освобождение памяти после игрока
        public void Dispose()
        {
            Players--;
        }
    }

    // Игровая ячейка
    public class GameCell : AGameCell
    {
        // Событие
        public event EventHandler Changed;

        // Конструктор
        public GameCell(int X, int Y) : base(X, Y)
        {
        }

        // Ход
        public bool Bind(Player P)
        {
            if (Owner != null || P == null)
                return false;
            else
            {
                Owner = P;
                Changed?.Invoke(this, new EventArgs());
            }

            return true;
        }
    }

    // Игровое поле 3х3
    public class GameField : AGameCell
    {
        // Свойства
        public bool Full { private set; get; } = false;
        public GameCell[,] Cells { private set; get; }

        // Событие
        public event EventHandler Changed;
        public event EventHandler Filled;

        // Индексатор для обращения к ячейкам
        public GameCell this[int i,int j]
        {
            get
            {
                return Cells?[i, j];
            }
        }

        // Конструктор
        public GameField(int X, int Y) : base(X, Y)
        {
            Cells = new GameCell[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Cells[i, j] = new GameCell(i, j);
                    Cells[i, j].Changed += CellChanged;
                }
        }

        // Проверка соседних клеток с [i, j] на нахождение ряда из трёх
        private bool checkNeighbors(Position Pos)
        {
            int i = Pos.x;
            int j = Pos.y;
            if (Cells[i, 0].Owner == Cells[i, 1].Owner && Cells[i, 0].Owner == Cells[i, 2].Owner && Cells[i, 0].Owner != null)
                return true;
            if (Cells[0, j].Owner == Cells[1, j].Owner && Cells[0, j].Owner == Cells[2, j].Owner && Cells[0, j].Owner != null)
                return true;
            if (((i == 0 || i == 2) && (j == 0 || j == 2)) || (i == 1 && j == 1))
                if (((Cells[0, 0].Owner == Cells[1, 1].Owner && Cells[0, 0].Owner == Cells[2, 2].Owner)
                || (Cells[0, 2].Owner == Cells[1, 1].Owner && Cells[1, 1].Owner == Cells[2, 0].Owner)) && Cells[1, 1].Owner != null)
                    return true;

            return false;
        }

        // Обработка изменения ячейки
        private void CellChanged(object sender, EventArgs e)
        {
            // Проверка на победу в этом поле
            if (Owner == null)
                if (checkNeighbors(((GameCell)sender).Pos))
                {
                    Owner = ((GameCell)sender).Owner;

                    Changed?.Invoke(this, new EventArgs());
                }

            // Проверка на заполненность
            bool full = true;
            for (int i = 0; i < 3 && full; i++)
                for (int j = 0; j < 3 && full; j++)
                    if (Cells[i, j].Owner == null)
                        full = false;
            
            if (full)
            {
                Full = true;

                // Фу, скатился, отписка
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        Cells[i, j].Changed -= CellChanged;

                Filled?.Invoke(this, e);
            }
        }

        public void _Bind(Player P)
        {
            Owner = P;
        }

        // Очищение поля
        public override void Clear()
        {
            base.Clear();
            if (Full)
            {
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        Cells[i, j].Changed += CellChanged;
            }
            Full = false;
            foreach (var item in Cells)
            {
                item.Clear();
            }
        }
    }

    public abstract class AGameCell
    {
        // Свойства
        public Position Pos { protected set; get; }
        public Player Owner { protected set; get; }

        public AGameCell(int X, int Y)
        {
            Pos = new Position(X, Y);
        }

        // Очищение ячейки
        public virtual void Clear()
        {
            Owner = null;
        }
    }
}
