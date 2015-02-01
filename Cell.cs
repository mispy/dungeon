using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dungeon {
    /// <summary>
    /// A Cell represents a single gameplay x,y position, containing 
    /// multiple Tiles layered on top of each other.
    /// </summary>
    public class Cell {
        public int X;
        public int Y;

        /// <summary>
        /// Graphical tiles that make up the cell, in order from bottom to top.
        /// </summary>
        public List<Tile> Tiles;

        /// <summary>
        /// Items currently occupying the cell.
        /// </summary>
        public List<Item> Items;

        /// <summary>
        /// Creatures currently occupying the cell.
        /// </summary>
        public List<Creature> Creatures;

        public Map Map;

        public Cell(Map map, int x, int y) {
            Map = map;
            X = x;
            Y = y;
            Tiles = new List<Tile>();
            Items = new List<Item>();
            Creatures = new List<Creature>();
        }

        public bool IsOpaque() {
            foreach (var tile in Tiles) {
                if (tile.Flags.Obstacle) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find all cells immediately adjacent to this one.
        /// </summary>
        public List<Cell> FindNeighbors() {
            var cells = new List<Cell>();

            for (var i = X - 1; i <= X + 1; i++) {
                for (var j = Y - 1; j <= Y + 1; j++) {
                    if (i > 0 && j > 0 && i < Map.Width && j < Map.Height) {
                        cells.Add(Map.Cells[i, j]);
                    }                    
                }
            }

            return cells;
        }

        /// <summary>
        /// Find the minimum distance between this cell and another cell.
        /// </summary>
        public int DistanceTo(Cell cell) {
            return (int)Math.Sqrt(Math.Pow(cell.X - X, 2) + Math.Pow(cell.Y - Y, 2));
        }
    }
}
