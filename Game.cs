using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTM
{
    public class Position
    {
        public int x, y;
        public bool WithoutValue = true;

        public static Position GetCellFrom9x9(Position pos)
        {
            return pos % 3;
        }
        public static Position GetFieldFrom9x9(Position pos)
        {
            return pos / 3;
        }

        public Position(int x, int y)
        {
            this.x = x; this.y = y;
            WithoutValue = false;
        }
        public override bool Equals(object another)
        {
            if (another is Position)
                return x == ((Position)another).x && y == ((Position)another).y;
            else
                return false;
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
    /*public class Pos
    {
        public int x, y;
        public Pos(int i, int j)
        {
            x = i;
            y = j;
        }

        public override bool Equals(object another)
        {
            if (another is Pos)
                return x == ((Pos)another).x && y == ((Pos)another).y;
            else
                return false;
        }

        /*public static bool operator== (Pos a, Pos b)
        {
            return a?.Equals(b) ?? false;
        }

        public static bool operator!= (Pos a, Pos b)
        {
            return !(a == b);
        }
    }

    public class DoublePos
    {
        public Pos FieldPos;
        public Pos CellPos;
    }*/

    enum WhosTurn
    {
        Player1,
        Player2,
        Mine,
        Comp,
        OnlinePlayer
    }

    class SinglePlayerWithFriend
    {
        Game game;
        Player Player1, Player2;
        public WhosTurn Turn = WhosTurn.Player1;
        public Player CurrentPlayer;
        public event EventHandler SomebodyWins;
        public event EventHandler NobodyWins;
        public event EventHandler<WhosTurn> ChangeTurn;
        public event EventHandler<Position> IncorrectTurn;
        //public event EventHandler<Position> FieldBinded;

        public SinglePlayerWithFriend(string player1, string player2)
        {
            game = new Game();
            game.StartGame();
            Player1 = new Player(player1);
            Player2 = new Player(player2);
            CurrentPlayer = Player1;
        }

        /*private void GameFieldBinded(object sender, Position e)
        {
            FieldBinded(this, e);
        }*/

        public void ClickOn(int i, int j)
        {
            bool res = game.Turn(new Position(i, j), CurrentPlayer);

            if (res)
            {
                if (Turn == WhosTurn.Player1)
                {
                    Turn = WhosTurn.Player2;
                    CurrentPlayer = Player2;
                }
                else
                {
                    Turn = WhosTurn.Player1;
                    CurrentPlayer = Player1;
                }

                ChangeTurn(this, Turn);
            }
            else
                IncorrectTurn?.Invoke(this, game.CurrentField ?? new Position(-1, -1));
        }

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

        /*public bool[,] FilledFields()
        {
            return game.FilledFields();
        }*/
    }


    class Game
    {
        //Player PlayerA, PlayerB;
        //public Player CurrentPlayer { private set; get; }
        public event EventHandler GameEnds;
        public event EventHandler GameStateChanged;
        //public event EventHandler<Position> GameFieldBinded;
        public GameField[,] Fields { private set; get; }
        public List<Position> History = new List<Position>();

        //Added
        public Player[,] FieldsState()
        {
            Player[,] res = new Player[3,3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res[i, j] = (Fields[i, j].Owner);

            return res;
        }
        public Position CurrentField { private set; get; }
        /*public bool[,] FilledFields()
        {
            bool[,] res = new bool[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    res[i, j] = Fields[i, j].Full;

            return res;
        }*/

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
        /*private void SwapPlayer()
        {
            if (CurrentPlayer == PlayerA)
                CurrentPlayer = PlayerB;
            else
                CurrentPlayer = PlayerA;
        }*/
        public bool Turn(Position pos, Player player)
        {
            if (pos.x < 0 || pos.x > 8 || pos.y < 0 || pos.y > 8)
                return false;

            Position Cell = Position.GetCellFrom9x9(pos);
            Position Field = Position.GetFieldFrom9x9(pos);

            bool notSameField = !Field.Equals(CurrentField);
            bool fieldIsNotFull = !Fields[Field.x, Field.y].Full;

            if (notSameField && CurrentField != null && fieldIsNotFull)
                return false;
            
            if (Fields[Field.x, Field.y][Cell.x, Cell.y].Bind(player))
            {
                History.Add(pos);
                CurrentField = Cell;
                return true;
            }
            else
                return false;
        }

        //Конструктор
        public Game()
        {
            Fields = new GameField[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Fields[i, j] = new GameField(i, j);
                    //Fields[i, j].Changed += FieldChanged;
                    //Fields[i, j].Binded += FieldBinded;
                }
        }

        //События
        /*private void FieldBinded(object sender, EventArgs e)
        {
            GameFieldBinded?.Invoke(this, new Position(((GameField)sender).PosX, ((GameField)sender).PosY));
        }*/
        /*private void FieldChanged(object sender, EventArgs e)
        {
            GameStateChanged?.Invoke(sender, e);
        }*/

        //Сохранение в строку
        public string GetStateCode()
        {
            return "";
        }
        public bool UpdateFromStateCode()
        {
            return false;//если неудалось распарсить
        }
    }

    public class Player : IDisposable
    {
        private static int Players = 0;
        public int Id { private set; get; }
        public string Name { private set; get; }

        public Player(string Name)
        {
            Id = ++Players;
            this.Name = Name;
        }
        public void Dispose()
        {
            Players--;
        }
    }

    class GameCell
    {
        public Position Pos;
        //public int PosX { private set; get; }
        //public int PosY { private set; get; }
        public Player Owner { get; private set; }
        public event EventHandler Changed;

        public GameCell(int X, int Y, Player P = null)
        {
            Pos = new Position(X, Y);
            Owner = P;
        }

        public bool Bind(Player P)
        {
            if (Owner != null)
                return false;
            else
            {
                Owner = P;
                Changed?.Invoke(this, new EventArgs());
            }

            return true;
        }
        public void Clear()
        {
            Owner = null;
        }    
    }

    class GameField
    {
        public bool Full { private set; get; } = false;
        public GameCell[,] Cells { private set; get; }
        public int PosX { private set; get; }
        public int PosY { private set; get; }
        public Player Owner { private set; get; }
        public GameCell this[int i,int j]
        {
            get
            {
                return Cells?[i, j];
            }
        }

        public GameField(int X, int Y)
        {
            PosX = X;
            PosY = Y;
            Cells = new GameCell[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Cells[i, j] = new GameCell(i, j);
                    Cells[i, j].Changed += CellChanged;
                }
        }

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

        private void CellChanged(object sender, EventArgs e)
        {
            if (Owner == null)
                if (checkNeighbors(((GameCell)sender).Pos.x, ((GameCell)sender).Pos.y))
                {
                    Owner = ((GameCell)sender).Owner;
                    //Binded(this, e);
                    //Binded = true;
                }

            bool full = true;
            for (int i = 0; i < 3 && full; i++)
                for (int j = 0; j < 3 && full; j++)
                    if (Cells[i, j].Owner == null)
                        full = false;

            //Changed?.Invoke(this, e);
            if (full)
            {
                Full = true;
                //Filled?.Invoke(this, e);
            }
        }

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
}
