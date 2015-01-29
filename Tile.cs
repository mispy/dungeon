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
        public bool Player = false;
        public bool Item = false;
    }

    /// <summary>
    /// Represents a single kind of tile on a tilesheet.
    /// </summary>
    /// 
    public class Tile {
        /// <summary>
        /// The tilesheet image containing this tile.
        /// </summary>
        public Texture2D TileSheet;

        /// <summary>
        /// A rectangle specifying where on the tilesheet this tile sits.
        /// </summary>
        public Rectangle Rectangle;

        /// <summary>
        /// Boolean tile flags read from the Tiled properties.
        /// </summary>
        public TileFlags Flags;

        /// <summary>
        /// A blank tile, for use as a placeholder.
        /// </summary>
        public static Tile Blank = new Tile();

        public Tile() {
            Flags = new TileFlags();
        }

        public void InitProps(PropertyDict tmxProps) {            
            foreach (var pair in tmxProps) {
                if (pair.Key == "obstacle") {
                    Flags.Obstacle = true;
                } else if (pair.Key == "creature") {
                    Flags.Creature = true;
                } else if (pair.Key == "player") {
                    Flags.Player = true;
                } else if (pair.Key == "item") {
                    Flags.Item = true;
                }
            }
        }
    }
}
