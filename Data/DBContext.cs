using System.Collections.Generic;

namespace Sudoku.Data
{
    internal static class DBContext
    {
        public static List<History> Histories { get; set; }

        static DBContext()
        {
            Histories = new List<History>();
        }
    }
}
