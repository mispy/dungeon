using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace Dungeon {
    public class TileFlags {
        public bool Obstacle = false;
        public bool Creature = false;
    }

    /// <summary>
    /// Represents a single kind of tile on a tilesheet.
    /// </summary>
    /// 
    public class Tile {
        public Texture2D TileSheet; // Tilesheet containing this tile
        public Rectangle Rectangle; // Where on the tilesheet it is
        public TileFlags Flags;

        public Tile() {
            Flags = new TileFlags();
        }

        public void InitProps(PropertyDict tmxProps) {
            foreach (var pair in tmxProps) {
                if (pair.Key == "obstacle") {
                    Flags.Obstacle = true;
                }
                else if (pair.Key == "creature") {
                    Flags.Creature = true;
                }
            }
        }
    }
}
