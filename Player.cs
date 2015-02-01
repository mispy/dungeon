using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dungeon
{
    public class Player : Creature
    {
        public static Player current;
        public List<Cell> CurrentPath;
        public bool[,] Memory;

        public Player() : base() {
            current = this;

            var map = DungeonGame.current.Map;
            Memory = new bool[map.Width, map.Height];
        }

        /// <summary>
        /// Checks if the cell is inside the player's FOV.
        /// </summary>
        public bool CanSee(Cell cell) {
            if (Cell.DistanceTo(cell) < 5) {
                Memory[cell.X, cell.Y] = true;
                return true;
            } else {
                return false;
            }
        }

        public bool RemembersCell(Cell cell) {
            return Memory[cell.X, cell.Y];
        }
    }
}
