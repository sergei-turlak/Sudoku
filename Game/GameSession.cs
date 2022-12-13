using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sudoku.Game
{
    public enum Level : byte { Easy = 12, Medium = 5, Hard = 2 }
    public delegate void GameResultHandler();

    public class GameSession
    {
        public event GameResultHandler OnSuccessfulSolution;
        public event GameResultHandler OnFailedSolution;

        private readonly byte n;
        private byte[,] baseField;
        private DateTime? startTime;

        public int ID { get; }
        public byte[,] DraftField { get; private set; }
        public Level Level { get; }
        public byte AllowableMistakesLeft { get; private set; }
        public TimeSpan? PlayingTime { get; private set; }
        public bool? IsGameSuccessfullySolved { get; private set; }
        public DateTime? Date { get; private set; }

        public GameSession(Level level, byte n)
        {
            ID = Guid.NewGuid().GetHashCode();
            this.n = n;
            baseField = new byte[n, n];
            DraftField = new byte[n, n];
            Level = level;
            AllowableMistakesLeft = (byte)level;

            do GeneratePossibleBaseField();
            while (!IsFieldFilled(baseField));

            FillDraftField();
        }

        public void StartTimer()
        {
            if (startTime == null)
                startTime = DateTime.Now;
        }

        public bool WriteNumInDraft(byte number, int i, int j)
        {
            if (IsGameSuccessfullySolved.HasValue) throw new Exception("Поточна гра вже закінчилася");

            if (number == baseField[i, j])
            {
                DraftField[i, j] = number;

                if (IsFieldFilled(DraftField))
                {
                    PlayingTime = DateTime.Now - startTime.Value;
                    Date = DateTime.Now;
                    IsGameSuccessfullySolved = true;
                    OnSuccessfulSolution();
                }
                return true;
            }

            AllowableMistakesLeft--;

            if (AllowableMistakesLeft == 0)
            {
                PlayingTime = DateTime.Now - startTime.Value;
                Date = DateTime.Now;
                IsGameSuccessfullySolved = false;
                OnFailedSolution();
            }

            return false;
        }

        private void FillDraftField()
        {
            int primaryKnownElements = (byte)Level + 28;
            Point[] shuffledOrder = GetShuffledPointsOrder(n, n);
            for (int i = 0; i < primaryKnownElements; i++)
                DraftField[shuffledOrder[i].X, shuffledOrder[i].Y] = baseField[shuffledOrder[i].X, shuffledOrder[i].Y];
        }

        private void GeneratePossibleBaseField()
        {
            //shuffling order of adding numbers to field 
            Point[] shuffledOrder = GetShuffledPointsOrder(n, n);

            baseField = new byte[n, n];    //cleaning base field
            for (int i = 0; i < shuffledOrder.Length; i++)
            {
                for (byte k = 1; k < n + 1; k++)
                {
                    if (isNumberSuitable(k, shuffledOrder[i].X, shuffledOrder[i].Y))
                    {
                        baseField[shuffledOrder[i].X, shuffledOrder[i].Y] = k;
                        break;
                    }
                }
            }
        }

        private bool IsFieldFilled(byte[,] field)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (field[i, j] == 0)
                        return false;
            return true;
        }

        private Point[] GetShuffledPointsOrder(int rows, int cols)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            Point[] shuffledOrder = new Point[rows * cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    shuffledOrder[i * cols + j] = new Point(i, j);
            return shuffledOrder.OrderBy(x => rand.Next()).ToArray();
        }

        private bool isNumberSuitable(int number, int i, int j)
        {
            for (int x = 0; x < n; x++)   // checking row
                if (baseField[i, x] == 0)
                    continue;
                else if (number == baseField[i, x] && x != j)
                    return false;

            for (int x = 0; x < n; x++)   //checking column
                if (baseField[x, j] == 0)
                    continue;
                else if (number == baseField[x, j] && x != i)
                    return false;

            //checking inner square 3x3
            byte size = (byte)Math.Sqrt(n);   // size of small 3x3 inner square 
            int innerSquare_i = i / size;     //calculatin coordinates of inner square of current element
            int innerSquare_j = j / size;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    int current_i = innerSquare_i * size + x;
                    int current_j = innerSquare_j * size + y;
                    if (baseField[current_i, current_j] == 0)
                        continue;
                    else if (number == baseField[current_i, current_j] && (current_i != i || current_j != j))
                        return false;
                }
            return true;
        }
    }
}