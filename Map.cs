using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace Dungeon {
    // <summary>
    // Map component which draws a Map on the screen.
    // </summary>
    public class MapRenderer {
        private Map Map;

        public MapRenderer(Map map) {
            Map = map;
        }

        // <summary>
        // Draws the map.
        // <param name="batch">SpriteBatch on which to draw.</param>
        // <param name="viewport">Rectangle of the map to draw. Specifed in pixel, not tile coordinates, to enable smooth scrolling.</param>
        // </summary>
        public void Draw(SpriteBatch batch, Rectangle viewport) {

            // Convert pixel coordinates to the corresponding tiles
            int iStart, iEnd, jStart, jEnd;
            if (viewport.X >= 0) {
                iStart = (int)Math.Floor((double)viewport.X / Map.TileWidth);
                iEnd = iStart + (int)Math.Floor((double)viewport.Width / Map.TileWidth) + 1;
            } else {
                iStart = (int)Math.Ceiling((double)viewport.X / Map.TileWidth);
                iEnd = iStart + (int)Math.Ceiling((double)viewport.Width / Map.TileWidth) + 1;
            }

            if (viewport.Y >= 0) {
                jStart = (int)Math.Floor((double)viewport.Y / Map.TileHeight);
                jEnd = jStart + (int)Math.Floor((double)viewport.Height / Map.TileHeight) + 1;
            }
            else {
                jStart = (int)Math.Ceiling((double)viewport.Y / Map.TileHeight);
                jEnd = jStart + (int)Math.Ceiling((double)viewport.Height / Map.TileHeight) + 1;
            }

            // Ensure we're actually drawing stuff that's inside the map
            iEnd = Math.Min(iEnd, Map.Width);
            jEnd = Math.Min(jEnd, Map.Height);
            
            // How much of a tile at the edge of the screen is missing
            var xOffset = -1 * viewport.X % Map.TileWidth;
            var yOffset = -1 * viewport.Y % Map.TileHeight;

            Console.WriteLine("{0} {1} {2} {3}", iStart, iEnd, viewport.X, xOffset);

            // Draw tiles inside canvas
            foreach (var layer in Map.Layers) {
                for (var i = iStart; i < iEnd; i++) {
                    for (var j = jStart; j < jEnd; j++) {
                        if (i < 0 || j < 0) continue;
                        var tile = layer[i,j];

                        // Empty tiles have no graphical information                       
                        if (tile.TileSheet == null) {
                            continue;
                        }

                        var position = new Vector2(
                            Map.TileWidth * (i - iStart) + xOffset,
                            Map.TileHeight * (j - jStart) + yOffset);
                        
                        batch.Draw(tile.TileSheet, position,
                                   tile.Rectangle, tile.Flags.Obstacle ? Color.Red : Color.White, 0.0f, new Vector2(0, 0),
                                   1, SpriteEffects.None, 0);
                    }
                }
            }

            for (var i = iStart; i < iEnd; i++) {
                for (var j = jStart; j < jEnd; j++) {
                    if (i < 0 || j < 0) continue;
                    var cell = Map.Cells[i, j];
                    cell.x = i;
                    cell.y = j;

                    foreach (var cre in cell.creatures) {
                        var tile = cre.tile;

                        var position = new Vector2(
                            Map.TileWidth * (i - iStart) + xOffset,
                            Map.TileHeight * (j - jStart) + yOffset);

                        var effects = SpriteEffects.None;

                        if (cre.facing == Direction.Right) {
                            effects = SpriteEffects.FlipHorizontally;
                        }

                        batch.Draw(tile.TileSheet, position,
                                   tile.Rectangle, Color.White, 0.0f, new Vector2(0, 0),
                                   1, effects, 0);
                    }
                }
            }
        }
    }

    // <summary>
    // This class represents a game map, providing a common interface to
    // the underlying Tiled map data.
    // </summary>
    public class Map {
        // Width and height, in number of tiles
        public int Width;
        public int Height;

        // Width and height of each tile, in pixels
        public int TileWidth;
        public int TileHeight;

        public List<Tile[,]> Layers; // Tile layers, from bottom to top
        public List<Tile> TileTypes; // List of all tiletypes in the map
        public Cell[,] Cells;

        public MapRenderer Renderer;

        public Map() {
            Renderer = new MapRenderer(this);
        }

        public void Initialize(TmxMap tmx) {
            Width = tmx.Width;
            Height = tmx.Height;
            TileWidth = tmx.TileWidth;
            TileHeight = tmx.TileHeight;

            // Temporary tmx tile id => Tile
            var tileTypes = new Dictionary<int, Tile>();
            TileTypes = new List<Tile>(); // TODO make this less confusing

            // tileTypes[0] is the default "air" tile
            // We currently convey this by setting the TileSheet to null
            var emptyTile = new Tile();
            emptyTile.TileSheet = null;
            tileTypes[0] = emptyTile;

            var sheetIndex = 0;
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
                        tileTypes[id] = tile;
                        TileTypes.Add(tile);
                        id += 1;
                    }
                }
   
                foreach (TmxTilesetTile tile in ts.Tiles) {
                     tileTypes[ts.FirstGid+tile.Id].InitProps(tile.Properties);
                }

                sheetIndex += 1;
            }

            // Compute map structure and gameplay tiles
            // Individual layers are used for rendering
            // The combined layer represents the topmost tiles (i.e. those with no other tiles in front of them)

            Layers = new List<Tile[,]>();
            Cells = new Cell[Width, Height]; // Gameplay cells

            for (var i = 0; i < Width; i++) {
                for (var j = 0; j < Height; j++) {
                    Cells[i, j] = new Cell();
                }
            }

            foreach (TmxLayer tmxLayer in tmx.Layers) {
                var layer = new Tile[Width, Height];

                foreach (TmxLayerTile tile in tmxLayer.Tiles) {
                    var tileType = tileTypes[tile.Gid];
                    layer[tile.X, tile.Y] = tileType;
                    Cells[tile.X, tile.Y].tiles.Add(tileType);
                }

                Layers.Add(layer);
            }
        }

        public Texture2D GetTileSheet(string filepath) {
            Texture2D newSheet;
            Stream imgStream;

            imgStream = File.OpenRead(filepath);

            newSheet = Texture2D.FromStream(DungeonGame.current.Graphics, imgStream);
            return newSheet;
        }
    }
}