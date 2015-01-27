using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace Dungeon {
    /// <summary>
    /// This class represents a game map, providing a common interface to
    /// the underlying Tiled map data.
    /// </summary>
    public class Map {
        /// <summary>
        /// Width of the map, in number of tiles
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the map, in number of tiles
        /// </summary>
        public int Height;

        /// <summary>
        /// Width of a tile, in pixels (presumably same as height)
        /// </summary>
        public int TileWidth;

        /// <summary>
        /// Height of a tile, in pixels (presumably same as width)
        /// </summary>
        public int TileHeight;

        /// <summary>
        /// List of all the individual types of tile we find in the map
        /// </summary>
        public List<Tile> TileTypes;

        /// <summary>
        /// 2D array of cells that contains most of the data
        /// </summary>
        public Cell[,] Cells;

        /// <summary>
        /// Finds all the creatures on the map.
        /// TODO (Mispy): Make this one list that is updated, for performance reasons.
        /// </summary>
        public List<Creature> Creatures {
            get {
                var creatures = new List<Creature>();

                for (var i = 0; i < Width; i++) {
                    for (var j = 0; j < Height; j++) {
                        creatures = creatures.Concat(Cells[i, j].Creatures).ToList();
                    }
                }

                return creatures;
            }
        }

        public MapRenderer Renderer;

        public Map() {
            Renderer = new MapRenderer(this);
            TileTypes = new List<Tile>();
        }

        public void LoadTMX(TmxMap tmx) {
            Width = tmx.Width;
            Height = tmx.Height;
            TileWidth = tmx.TileWidth;
            TileHeight = tmx.TileHeight;

            // We map tmx tile gid => Tile so we can put the tile properties in
            var tileTypesById = new Dictionary<int, Tile>();
            tileTypesById[0] = Tile.Blank;

            foreach (TmxTileset ts in tmx.Tilesets) {
                var tileSheet = GetTileSheet(ts.Image.Source);

                // Loop over the tilesheet and calculate rectangles indicating where
                // each Tile resides. Note that we need to account for Tiled's margin
                // and spacing settings
                var wStart = ts.Margin;
                var wInc = ts.TileWidth + ts.Spacing;
                var wEnd = ts.Image.Width - (ts.Image.Width % (ts.TileWidth + ts.Spacing));
                
                var hStart = ts.Margin;
                var hInc = ts.TileHeight + ts.Spacing;
                var hEnd = ts.Image.Height - (ts.Image.Height % (ts.TileHeight + ts.Spacing));

                var id = ts.FirstGid;
                for (var h = hStart; h < hEnd; h += hInc) {
                    for (var w = wStart; w < wEnd; w += wInc) {                        
                        var tile = new Tile();
                        tile.TileSheet = tileSheet;
                        tile.Rectangle = new Rectangle(w, h, ts.TileWidth, ts.TileHeight);
                        tileTypesById[id] = tile;

                        TileTypes.Add(tile);
                        id += 1;
                    }
                }
   
                foreach (TmxTilesetTile tile in ts.Tiles) {
                     tileTypesById[ts.FirstGid+tile.Id].InitProps(tile.Properties);
                }
            }

            // Now that we know what all the tiles look like, we can
            // start splitting up the layers and packing them into cells
            Cells = new Cell[Width, Height];

            for (var i = 0; i < Width; i++) {
                for (var j = 0; j < Height; j++) {
                    Cells[i, j] = new Cell(this, i, j);
                }
            }

            foreach (TmxLayer tmxLayer in tmx.Layers) {
                foreach (TmxLayerTile tmxTile in tmxLayer.Tiles) {
                    var tileType = tileTypesById[tmxTile.Gid];

                    if (tileType.Flags.Creature) {
                        // We found a creature!
                        var cre = new Creature();

                        // Find the creature's second animation tile by looking one row down in the tileset
                        var secondTile = tileTypesById[tmxTile.Gid + (tileType.TileSheet.Width / TileWidth)];
                        cre.Tiles = new List<Tile>() { tileType, secondTile };

                        cre.Move(Cells[tmxTile.X, tmxTile.Y]);
                    } else {
                        Cells[tmxTile.X, tmxTile.Y].Tiles.Add(tileType);
                    }
                }
            }
        }

        /// <summary>
        /// Loads a tilesheet into a Texture2D.
        /// </summary>
        /// <param name="filepath">Path to image file.</param>
        /// <returns></returns>
        public Texture2D GetTileSheet(string filepath) {
            Texture2D newSheet;
            Stream imgStream;

            imgStream = File.OpenRead(filepath);

            newSheet = Texture2D.FromStream(DungeonGame.current.Graphics, imgStream);
            return newSheet;
        }
    }
}