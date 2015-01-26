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
        public int x;
        public int y;
        public List<Tile> tiles;
        public List<Creature> creatures;

        public Cell() {
            tiles = new List<Tile>();
            creatures = new List<Creature>();
        }
    }
}
