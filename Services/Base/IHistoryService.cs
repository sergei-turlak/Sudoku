using Sudoku.Data;
using Sudoku.Game;
using System.Collections.Generic;

namespace Sudoku.Services.Base
{
    internal interface IHistoryService
    {
        void Add(GameSession game);
        History Get(int id);
        List<History> GetAll();
    }
}
