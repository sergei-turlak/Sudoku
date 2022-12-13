using System;
using Sudoku.Game;

namespace Sudoku.Data
{
    internal class History
    {
        public int ID { get; }
        public Level Level { get; }
        public byte AllowableMistakesLeft { get; }
        public TimeSpan PlayingTime { get; }
        public bool IsGameSuccessfullySolved { get; }
        public DateTime Date { get; }

        public History(GameSession game)
        {
            ID = game.ID;
            Level = game.Level;
            AllowableMistakesLeft = game.AllowableMistakesLeft;
            PlayingTime = game.PlayingTime.Value;
            IsGameSuccessfullySolved = game.IsGameSuccessfullySolved.Value;
            Date = game.Date.Value;
        }
    }
}
