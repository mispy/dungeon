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
        public bool[,] FOV;

        public Player() : base() {
            current = this;

            var map = DungeonGame.current.Map;
            Memory = new bool[map.Width, map.Height];
        }

        public void ComputeFOV() {
            var map = DungeonGame.current.Map;
            FOV = new bool[map.Width, map.Height];

            ShadowCaster.ComputeFieldOfViewWithShadowCasting(Cell.X, Cell.Y, SightDistance,
                (x, y) => {
                    if (map.WithinBounds(x, y)) {
                        return map.Cells[x, y].IsOpaque();
                    } else {
                        return true;
                    }
                },
                (x, y) => {
                    if (map.WithinBounds(x, y)) {
                        FOV[x, y] = true;
                        Memory[x, y] = true;
                    }
                }
            );
        }

        /// <summary>
        /// Checks if the cell is inside the player's FOV.
        /// </summary>
        public bool CanSee(Cell cell) {
            if (FOV[cell.X, cell.Y]) {
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
