using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dungeon {
    public enum Direction {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }

    public class Creature {
        public Cell cell;
        public Tile tile;
        public Direction facing;
        public Creature() {
            facing = Direction.Right;
        }

        public bool CanPass(Cell cell) {
            foreach (var tile in cell.tiles) {
                if (tile.Flags.Obstacle) {
                    return false;
                }
            }

            return true;
        }

        public void Move(Cell newCell) {
            if (cell != null) {
                cell.creatures.Remove(this);
            }
            newCell.creatures.Add(this);
            cell = newCell;
        }
    }
}
