using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTM
{
    ////public class Settings
    //{
    //    /*
    //     * Colors
    //     * Default Name(s)
    //     * ???
    //     * save()
    //     * load()
    //     */
    //}

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

        // Переопределение операторов %, /, метода сравнения и получения хэш-кода
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
    
    // Класс, реализующий одиночную игру с другим игроком
    class SinglePlayerWithFriend
    {
        // Свойства
        private Game game;
        private Player Player1, Player2;
        public Player CurrentPlayer;
        public event EventHandler<Game.GameEndArgs> SomebodyWins;
        public event EventHandler NobodyWins;
        public event EventHandler<Player> ChangeTurn;
        public event EventHandler<Position> IncorrectTurn;

        // Конструктор
        public SinglePlayerWithFriend(string player1, string player2)
        {
            game = new Game();
            game.StartGame();
            Player1 = new Player(player1);
            Player2 = new Player(player2);
            CurrentPlayer = Player1;
            game.GameEnds += GameEnds;
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
        public void ClickOn(int i, int j)
        {
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
                    res[i,j] = (game[i / 3, j / 3, i % 3, j % 3].Owner?.Id ?? 0);

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

        // Сохранение/загрузка
        public string Save()
        {
            return game.GetStateCode();
        }

        public void Load(string State)
        {
            var sb = new StringBuilder("2100200000");
            game.UpdateFromStateCode("100010002" + sb.Append('0', 9*8), Player1, Player2);
        }
    }

    // Класс содержащий матрицу полей с ячейками, историю ходов и проверяющий корректность хода
    class Game
    {
        // Свойства
        public GameField[,] Fields { private set; get; }
        public Position CurrentField { private set; get; }
        public List<Position> History = new List<Position>();

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
            History.Clear();
            foreach (var item in Fields)
            {
                item.Clear();
            }
        }
        public bool Turn(Position pos, Player player)
        {
            // Ход вне диапазона
            if (pos.x < 0 || pos.x > 8 || pos.y < 0 || pos.y > 8)
                return false;

            // Проверка верности хода
            Position Cell = Position.GetCellFrom9x9(pos);
            Position Field = Position.GetFieldFrom9x9(pos);

            bool notSameField = !Field.Equals(CurrentField);
            bool fieldIsNotFull = !Fields[Field.x, Field.y].Full;

            if (notSameField && CurrentField != null && fieldIsNotFull)
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
                    if (Fields[i, j].Owner == null)
                        full = false;

            if (full)
            {
                // Конец игры из-за заполненности всех полей
                GameEnds?.Invoke(this, new GameEndArgs(null));
            }
        }
        private void FieldChanged(object sender, EventArgs e)
        {
            if (checkNeighbors(((GameField)sender).Pos.x, ((GameField)sender).Pos.y))
            {
                // Конец игры из-за победы одного из игроков
                GameEnds?.Invoke(this, new GameEndArgs(((GameField)sender).Owner));
            }
        }

        // Проверка соседних с [i, j] полей на нахождение ряда из трёх
        private bool checkNeighbors(int i, int j)
        {
            if (Fields[i, 0].Owner == Fields[i, 1].Owner && Fields[i, 0].Owner == Fields[i, 2].Owner && Fields[i, 0].Owner != null)
                return true;
            if (Fields[0, j].Owner == Fields[1, j].Owner && Fields[0, j].Owner == Fields[2, j].Owner && Fields[0, j].Owner != null)
                return true;
            if ((i == 0 || i == 2) && (j == 0 || j == 2))
                if (((Fields[0, 0].Owner == Fields[1, 1].Owner && Fields[0, 0].Owner == Fields[2, 2].Owner)
                || (Fields[0, 2].Owner == Fields[1, 1].Owner && Fields[0, 0].Owner == Fields[2, 0].Owner)) && Fields[0, 0].Owner != null)
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

            if (CurrentField != null)
                res.Append(CurrentField.x * 3 + CurrentField.y);
            else
                res.Append('X'); // Не забудь обработать

            return res.ToString();
        }
        public bool UpdateFromStateCode(string State, Player p1, Player p2)
        {
            if (State.Length != 91)
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

                if (State[90] == 'X')
                    CurrentField = null;
                else
                {
                    int temp = int.Parse(State[90].ToString());
                    CurrentField = new Position(temp / 3, temp % 3);
                }

                return true;
            }
            catch
            {
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
    class GameCell : IGameObjectAsCell
    {
        // Свойства
        public Position Pos { private set; get; }
        public Player Owner { get; private set; }

        // Событие
        public event EventHandler Changed;

        // Конструктор
        public GameCell(int X, int Y, Player P = null)
        {
            Pos = new Position(X, Y);
            Owner = P;
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

        // Очищение ячейки
        public void Clear()
        {
            Owner = null;
        }    
    }

    // Игровое поле 3х3
    class GameField : IGameObjectAsCell
    {
        // Свойства
        public bool Full { private set; get; } = false;
        public GameCell[,] Cells { private set; get; }
        public Position Pos { private set; get; }
        public Player Owner { private set; get; }

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
        public GameField(int X, int Y)
        {
            Pos = new Position(X, Y);
            Cells = new GameCell[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Cells[i, j] = new GameCell(i, j);
                    Cells[i, j].Changed += CellChanged;
                }
        }

        // Проверка соседних клеток с [i, j] на нахождение ряда из трёх
        private bool checkNeighbors(int i, int j)
        {
            if (Cells[i, 0].Owner == Cells[i, 1].Owner && Cells[i, 0].Owner == Cells[i, 2].Owner && Cells[i, 0].Owner != null)
                return true;
            if (Cells[0, j].Owner == Cells[1, j].Owner && Cells[0, j].Owner == Cells[2, j].Owner && Cells[0, j].Owner != null)
                return true;
            if ((i == 0 || i == 2) && (j == 0 || j == 2))
                if (((Cells[0, 0].Owner == Cells[1, 1].Owner && Cells[0, 0].Owner == Cells[2, 2].Owner)
                || (Cells[0, 2].Owner == Cells[1, 1].Owner && Cells[0, 0].Owner == Cells[2, 0].Owner)) && Cells[0, 0].Owner != null)
                    return true;

            return false;
        }

        //Обработка изменения ячейки
        private void CellChanged(object sender, EventArgs e)
        {
            // Проверка на победу в этом поле
            if (Owner == null)
                if (checkNeighbors(((GameCell)sender).Pos.x, ((GameCell)sender).Pos.y))
                {
                    Owner = ((GameCell)sender).Owner;
                    // Фу, скатился, отписка
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            Cells[i, j].Changed -= CellChanged;

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
                Filled?.Invoke(this, e);
            }
        }

        public void _Bind(Player P)
        {
            Owner = P;
        }

        // Очищение поля
        public void Clear()
        {
            foreach (var item in Cells)
            {
                item.Clear();
                Full = false;
                Owner = null;
            }
        }
    }

    // Интерфейсы
    interface IGameObjectAsCell
    {
        Position Pos { get; }
        Player Owner { get; }
        void Clear();
    }
}
