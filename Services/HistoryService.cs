using Sudoku.Data;
using Sudoku.Services.Base;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Game;

namespace Sudoku.Services
{
    internal class HistoryService : IHistoryService
    {
        public void Add(GameSession game)
        {
            DBContext.Histories.Add(new History(game));
        }

        public History Get(int id)
        {
            return DBContext.Histories.Where(h => h.ID == id).First();
        }

        public List<History> GetAll()
        {
            return DBContext.Histories;
        }
    }
}
