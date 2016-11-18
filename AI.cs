using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTM
{
    public interface IBot
    {
        void MakeTurn();
        Player Player { get; }
    }

    // Абстрактный бот (как базовый класс для реализаций бота)
    abstract internal class ABot : IBot
    {
        protected Random Rnd = new Random();
        public Player Player { protected set; get; }
        public Player HumanPlayer { protected set; get; }
        public Game Game { protected set; get; }
        public abstract void MakeTurn();
    }

    // Класс бота рекурсивно оценивающее ходы
    internal class RecursionAnalizerBot : ABot, IBot
    {
        // Константы для анализа очков каждой клетки
        const float CostMake3InField = 11f; // Сделать 3 в ряд в свободном поле
        const float CostPrevent3InField = 9f; // Помешать сделать 3 в ряд закрыв клетку
        const float CostMake3InGame = 20f; // Стоимость сделать 3 поля подряд (победа в игре)
        const byte MinFreeCellsToNarrowRecursion = 7; // Минимальное количество свободных клеток для сокращения глубины рекурсии
        const byte MaxFreeCellsToExtandRecursion = 3; // Максимальное количество свободных клеток для расширения глубины рекурсии
        const float WeightMaxScoresFromRecursion = 0.85f; // Вес максимума из рекурсивного вызова
        const float WeightMidScoresFromRecursion = 0.075f; // Вес среднего из рекурсивного вызова
        const float CostNoFreeCellsWithWhenCommonFieldContainsTrue = -35f; // Я не помню что это :D
        const float CostNoFreeCellsWithWhenCommonFieldDontContainsTrue = -20f; // Что это, зачем это тут..
        const float CostFreeCellInNextField = -0f; // Изменение очков в зависимости от количества свободных в следующем поле
        const float CostSendPlayerToOwnedField = 10f; // Стоимость того чтоб отправить противника в уже заполненное поле

        // Первый ход, уровень бота и количество ходов
        private Position FirstTurn;
        public int Level { private set; get; }
        public int TurnsCount { private set; get; }
        // Значения для анализа ходов, зависящие от уровня бота
        private bool InvertCalculation { get; }
        private float PercentOfRandom { get; }
        private int StartDepth { get; }

        // Конструктор
        public RecursionAnalizerBot(Player Player, Player HPlayer, Game Game, int Level)
        {
            this.Level = Level;
            this.Player = Player;
            this.HumanPlayer = HPlayer;
            this.Game = Game;
            this.InvertCalculation = Level < 4;
            this.PercentOfRandom = 1 - Math.Abs(Level - 4) / 6f;
            this.StartDepth = Math.Abs(Level - 5);
        }

        // Проверка 3 позиций на наличие нужной для заполнения (2 из них = Plr, 1 = null)
        private Position check3(Position p1, Position p2, Position p3, Player Plr, Position Field = null)
        {
            if (Field == null)
                Field = Game.CurrentField;

            if (Game.Fields[Field.x, Field.y].Cells[p1.x, p1.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p2.x, p2.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p3.x, p3.y].Owner == null)
                return p3;

            if (Game.Fields[Field.x, Field.y].Cells[p1.x, p1.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p2.x, p2.y].Owner == null &&
                Game.Fields[Field.x, Field.y].Cells[p3.x, p3.y].Owner == Plr)
                return p2;

            if (Game.Fields[Field.x, Field.y].Cells[p1.x, p1.y].Owner == null &&
                Game.Fields[Field.x, Field.y].Cells[p2.x, p2.y].Owner == Plr &&
                Game.Fields[Field.x, Field.y].Cells[p3.x, p3.y].Owner == Plr)
                return p1;

            return null;
        }

        // Возвращает булевую матрицу (развёрнутую в массив) полей, которые достаточно заполнить Plr'у для победы
        private bool[] check3F(Player Plr)
        {
            Position Current;

            List<Position> ResultList = new List<Position>(9);
            bool[] Result = new bool[9];

            for (int i = 0; i < 3; i++)
            {
                Current = _check3F(new Position(0, i), new Position(1, i), new Position(2, i), Plr);
                if (Current != null) ResultList.Add(Current);
                Current = _check3F(new Position(i, 0), new Position(i, 1), new Position(i, 2), Plr);
                if (Current != null) ResultList.Add(Current);
            }
            Current = _check3F(new Position(0, 0), new Position(1, 1), new Position(2, 2), Plr);
            if (Current != null) ResultList.Add(Current);
            Current = _check3F(new Position(2, 0), new Position(1, 1), new Position(0, 2), Plr);
            if (Current != null) ResultList.Add(Current);

            for (int i = 0; i < ResultList.Count; i++)
                Result[ResultList[i].x + ResultList[i].y * 3] = true;

            return Result;
        }

        // Проверка 3 полей в указанных позициях, аналогично check3
        private Position _check3F(Position p1, Position p2, Position p3, Player Plr)
        {
            if (Game.Fields[p1.x, p1.y].Owner == Plr &&
                Game.Fields[p2.x, p2.y].Owner == Plr &&
                Game.Fields[p3.x, p3.y].Owner == null)
                return p3;

            if (Game.Fields[p1.x, p1.y].Owner == Plr &&
                Game.Fields[p2.x, p2.y].Owner == null &&
                Game.Fields[p3.x, p3.y].Owner == Plr)
                return p2;

            if (Game.Fields[p1.x, p1.y].Owner == null &&
                Game.Fields[p2.x, p2.y].Owner == Plr &&
                Game.Fields[p3.x, p3.y].Owner == Plr)
                return p1;

            return null;
        }
        
        // Возвращаемая рекурсией структура
        private struct CalculatedScoresData
        {
            public float[] Scores { get; }
            public bool[] DeniedCells { get; }
            public byte FreeCells { get; }
            public float MaxScore { get; }
            public float MidScore { get; }
            public int Length { get; }
            public CalculatedScoresData(float[] Scores, bool[] DeniedCells)
            {
                if (Scores.Length != DeniedCells.Length)
                    throw new ArgumentException("Массив очков и свободных ячеек в структуре должен иметь одинаковый размер");
                this.Scores = Scores;
                this.DeniedCells = DeniedCells;
                this.FreeCells = (byte)DeniedCells.Count(c => c);
                this.Length = Scores.Length;
                this.MaxScore = float.MinValue;
                this.MidScore = 0;
                for (int i = 0; i < Scores.Length; i++)
                {
                    if (!DeniedCells[i])
                    {
                        if (MaxScore < Scores[i])
                            MaxScore = Scores[i];
                        MidScore += Scores[i];
                    }
                }
                MidScore /= FreeCells;
            }
        }

        // Рекурсивный подсчёт очков
        private CalculatedScoresData CalculateScores(Position Field = null, int Depth = 0, int MaxDepth = -1)
        {
            // Отключение вызова событий
            Game.SilentMode = true;

            // Указываем текущее поле
            if (Field == null)
                Field = Game.CurrentField;

            // Устанавливаем максимальную глубину рекурсии
            if (MaxDepth == -1)
                MaxDepth = StartDepth;

            // Инициализируем массив 9 элементов для ячеек
            float[] Scores = new float[9];
            bool[] StepDeny = new bool[9];

            // Добавляем очки к ячейке, в которую нужно пойти чтоб сделать 3 в ряд
            if (Game.Fields[Field.x, Field.y].Owner == null)
                find3RowOrColumn(Field, ref Scores, Depth);

            // Подсчитываем количество свободных ячеек и изменяем максимальную глубину рекурсии при необходимости
            var AllowedCellsHere = CalculateAllowedCells(Field, ref StepDeny, ref MaxDepth);

            // Подсчёт для общего поля, уменьшаем очки в ячейке, которая переместит противника в поле, которое осталось заполнить до победы
            var CommonField = check3F(Depth % 2 == 1 ? Player : HumanPlayer);
            for (int i = 0; i < 9; i++)
            {
                if (CommonField[i])
                    Scores[i] -= CostMake3InGame;
            }

            // Не даём ходить в поле первого хода
            if (Depth == 0 && FirstTurn != null)
                Scores[FirstTurn.x + FirstTurn.y * 3] -= Math.Max(5 - TurnsCount, 0);

            // Рекурсивно подсчитываем для каждого поля куда можем послать противника количество очков
            if (Depth < MaxDepth)
            {
                float[] MaxScoresFromRecursion = new float[9];
                float[] MidScoresFromRecursion = new float[9];
                var GameState = Game.GetStateCode();

                for (int i = 0; i < 9; i++)
                {
                    if (!StepDeny[i])
                    {
                        // Делаем ход, чтоб отменить его на игровом поле для рекурсии
                        var TurnPos = new Position(Field.x * 3 + i % 3, Field.y * 3 + i / 3);
                        Game.Turn(TurnPos, Depth % 2 == 0 ? Player : HumanPlayer);
                        var CalculatedScores = CalculateScores(new Position(i % 3, i / 3), Depth + 1);

                        // Находим максимум и среднее значение очков по каждому полю, куда будет ходить противник
                        MaxScoresFromRecursion[i] = CalculatedScores.MaxScore;
                        MidScoresFromRecursion[i] = CalculatedScores.MidScore;

                        if (!InvertCalculation)
                        {
                            Scores[i] -= MaxScoresFromRecursion[i] * WeightMaxScoresFromRecursion
                                + MidScoresFromRecursion[i] * WeightMidScoresFromRecursion;
                        }
                        else
                            Scores[i] += MaxScoresFromRecursion[i] * WeightMaxScoresFromRecursion
                                + MidScoresFromRecursion[i] * WeightMidScoresFromRecursion;

                        // Вычисляем количество свободных ячеек и занятость поля, и учитываем это в очках ячейки
                        if (CalculatedScores.FreeCells == 0)
                        {
                            if (CommonField.Contains(true))
                                Scores[i] += CostNoFreeCellsWithWhenCommonFieldContainsTrue;
                            else
                                Scores[i] += CostNoFreeCellsWithWhenCommonFieldDontContainsTrue;
                        }
                        else
                        {
                            if (Game.Fields[i % 3, i / 3].Owner == null)
                                Scores[i] += (9 - CalculatedScores.FreeCells) / 9f * CostFreeCellInNextField; // Чем меньше в поле противника свободных ячеек тем лучше
                            else //Owner != null
                                Scores[i] += CostSendPlayerToOwnedField; // Если ячейка уже занята, то посылаем туда противника
                        }
                    }
                }
                // Возвращаем исходное состояние
                Game.UpdateFromStateCode(GameState, HumanPlayer, Player);
            }

            // Возвращаем обработку событий игры
            if (Depth == 0)
            {
                Game.SilentMode = false;
                var MaxShift = Scores.Max() * 0.9;
                for (int i = 0; i < Scores.Length; i++)
                {
                    Scores[i] = Scores[i] * (1 - PercentOfRandom) +
                        + (float)(Rnd.NextDouble() * MaxShift * PercentOfRandom);

                    if (InvertCalculation)
                        Scores[i] = 10 - Scores[i];
                }
            }

            return new CalculatedScoresData(Scores, StepDeny);
        }

        // Подсчёт количества свободных ячеек, куда можно ходить с изменением максимальной глубины рекурсии
        private byte CalculateAllowedCells(Position Field, ref bool[] StepDeny, ref int MaxDepth)
        {
            // Подсчитываем количество свободных ячеек
            byte AllowedCellsHere = 0;
            for (int i = 0; i < 9; i++)
            {
                if (Game[Field.x, Field.y, i % 3, i / 3].Owner != null)
                    StepDeny[i] = true;
                else
                    AllowedCellsHere++;
            }

            // Изменяем максимальную глубину рекурсии если ячеек слишком много или слишком мало
            if (AllowedCellsHere >= MinFreeCellsToNarrowRecursion)
                MaxDepth--;
            else if (AllowedCellsHere <= MaxFreeCellsToExtandRecursion)
                MaxDepth++;

            return AllowedCellsHere;
        }

        // Находит где можно сделать третью для 3 в ряд и добавляет очков к этой ячейке
        private void find3RowOrColumn(Position Field, ref float[] Scores, int Depth)
        {
            // Создаём список позиций куда надо поставить чтоб было 3 подряд
            Position Current;
            List<Position> AddScoresBot = new List<Position>(9);
            List<Position> AddScoresHuman = new List<Position>(9);

            for (int i = 0; i < 3; i++)
            {
                Current = check3(new Position(0, i), new Position(1, i), new Position(2, i), Player, Field);
                if (Current != null) AddScoresBot.Add(Current);
                Current = check3(new Position(0, i), new Position(1, i), new Position(2, i), HumanPlayer, Field);
                if (Current != null) AddScoresHuman.Add(Current);
                Current = check3(new Position(i, 0), new Position(i, 1), new Position(i, 2), Player, Field);
                if (Current != null) AddScoresBot.Add(Current);
                Current = check3(new Position(i, 0), new Position(i, 1), new Position(i, 2), HumanPlayer, Field);
                if (Current != null) AddScoresHuman.Add(Current);
            }
            Current = check3(new Position(0, 0), new Position(1, 1), new Position(2, 2), Player, Field);
            if (Current != null) AddScoresBot.Add(Current);
            Current = check3(new Position(2, 0), new Position(1, 1), new Position(0, 2), Player, Field);
            if (Current != null) AddScoresBot.Add(Current);
            Current = check3(new Position(0, 0), new Position(1, 1), new Position(2, 2), HumanPlayer, Field);
            if (Current != null) AddScoresHuman.Add(Current);
            Current = check3(new Position(2, 0), new Position(1, 1), new Position(0, 2), HumanPlayer, Field);
            if (Current != null) AddScoresHuman.Add(Current);

            // Добавляем к очкам ячеек позиции из списка выше (Заполнить поле лучше чем помещать противнику)
            for (int i = 0; i < AddScoresBot.Count; i++)
            {
                if (Depth % 2 == 0)
                    Scores[AddScoresBot[i].x + AddScoresBot[i].y * 3] += CostMake3InField;
                else
                    Scores[AddScoresBot[i].x + AddScoresBot[i].y * 3] += CostPrevent3InField;
            }
            for (int i = 0; i < AddScoresHuman.Count; i++)
            {
                if (Depth % 2 == 1)
                    Scores[AddScoresHuman[i].x + AddScoresHuman[i].y * 3] += CostMake3InField;
                else
                    Scores[AddScoresHuman[i].x + AddScoresHuman[i].y * 3] += CostPrevent3InField;
            }
        }

        // Поиск лучшей позиции для хода по всей игре (в случае хода в заполненную ячейку)
        protected Position findBetterGlobalPos()
        {
            // Генерируем матрицу всех очков
            var GlobalScores = new double[9, 9];
            for (int i = 0; i < 9; i++)
            {
                var CalculatedScores = CalculateScores(new Position(i / 3, i % 3));
                for (int j = 0; j < 9; j++)
                {
                    GlobalScores[(i / 3) * 3 + j / 3, (i % 3) * 3 + j % 3] = CalculatedScores.Scores[j];
                    if (CalculatedScores.DeniedCells[j])
                        GlobalScores[(i / 3) * 3 + j / 3, (i % 3) * 3 + j % 3] = -1000;
                }
            }

            var MaxX = 0;
            var MaxY = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (GlobalScores[MaxX, MaxY] < GlobalScores[i, j])
                    {
                        MaxX = i;
                        MaxY = j;
                    }
                }
            }
            return new Position(MaxX, MaxY);
        }

        // Поиск лучшей позиции для хода
        protected Position FindBetterPos()
        {
            // Получает очки для всех ячеек и выкидываем те, куда ходить нельзя
            var CalculatedScores = CalculateScores();
            var Scores = CalculatedScores.Scores;
            for (int i = 0; i < 9; i++)
            {
                if (CalculatedScores.DeniedCells[i])
                    Scores[i] = -1000;
            }

            // Ищем индекс с максимальным счётом
            List<int> Max = new List<int>();
            Max.Add(0);
            int Min = 0;
            for (int i = 0; i < 9; i++)
            {
                if (Scores[i] < Scores[Min])
                    Min = i;
                if (Scores[i] == Scores[Max[0]])
                    Max.Add(i);
                if (Scores[i] > Scores[Max[0]])
                {
                    Max.Clear();
                    Max.Add(i);
                }
            }

            // Если разницы между ячейками нет то возвращаем null
            if (Scores[Max[0]] == Scores[Min])
                return null;

            // Из получившегося списка берём рандомный элемент
            var TurnIndex = Max[Rnd.Next(Max.Count)];

            // Возвращаем его
            return new Position(TurnIndex % 3, TurnIndex / 3);
        }

        // Сделать ход
        public override void MakeTurn()
        {
            if (Game.Finished)
                return;

            if (TurnsCount == 0)
                FirstTurn = Position.GetFieldFrom9x9(Game.History.First());

            Position Field = Game.CurrentField;
            int x = 0, y = 0;
            if (!Game.Fields[Field.x, Field.y].Full) // Если поле не заполненно полностью
            {
                Position finded = FindBetterPos(); // Ищем лучший ход
                if (finded != null) // Если нашли - ходим туда
                {
                    x = Field.x * 3 + finded.x;
                    y = Field.y * 3 + finded.y;
                }
                else // Иначе ходим в любую другую
                {
                    do
                    {
                        x = Rnd.Next(0, 3);
                        y = Rnd.Next(0, 3);
                    }
                    while (Game.Fields[Game.CurrentField.x, Game.CurrentField.y].Cells[x, y].Owner != null);
                    x += Field.x * 3;
                    y += Field.y * 3;
                }
            }
            else // Если же поле было заполнено
            {
                Position finded = findBetterGlobalPos(); // Находим лучший вариант хода по всему полю
                if (finded != null) // Если нашли - ходим туда
                {
                    x = finded.x;
                    y = finded.y;
                }
                else // Иначе ходим в любую свободную
                {
                    do
                    {
                        x = Rnd.Next(0, 9);
                        y = Rnd.Next(0, 9);
                        if (Game.Fields[x / 3, y / 3].Owner != null)
                            continue;
                    }
                    while (Game.Fields[x / 3, y / 3].Cells[x % 3, y % 3].Owner != null);
                }
            }
            // Делаем ход и увеличиваем количество сделанных ходов
            if (!Game.Turn(new Position(x, y), Player))
                throw new Exception("Бот не смог сделать ход"); // Паровозик который не смог
            TurnsCount++;
        }
    }
}
