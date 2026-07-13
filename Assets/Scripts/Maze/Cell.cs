using System.Collections.Generic;

namespace Maze
{
    public class Cell
    {
        public Cell(CellKey key)
        {
            Key = key;
        }

        public CellKey Key { get; }
        public List<Wall> Walls { get; } = new(3);
    }
}
