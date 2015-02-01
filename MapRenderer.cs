using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dungeon {
    /// <summary>
    /// Map component which draws a Map on the screen.
    /// </summary>
    public class MapRenderer {
        private Map Map;

        public MapRenderer(Map map) {
            Map = map;
        }

        public int xStart;
        public int yStart;
        public int xEnd;
        public int yEnd;
        public int xOffset;
        public int yOffset;

        /// <summary>
        /// Find the map cell at the given pixel coordinates.
        /// </summary>
        public Cell CellAt(int px, int py) {
            var x = xStart + ((px - xOffset) / Map.TileWidth);
            var y = yStart + ((py - yOffset) / Map.TileHeight);

            if (x < 0 || y < 0 || x >= Map.Width || y >= Map.Height) {
                throw new OutOfBoundsException();
            }

            return Map.Cells[x, y];            
        }

        /// <summary>
        /// Draws the map.
        /// <param name="batch">SpriteBatch on which to draw.</param>
        /// <param name="viewport">Rectangle of the map to draw. Specifed in pixel, not tile coordinates, to enable smooth scrolling.</param>
        /// </summary>
        public void Draw(SpriteBatch batch, Rectangle viewport) {

            // Convert pixel coordinates to the corresponding tiles
            if (viewport.X >= 0) {
                xStart = (int)Math.Floor((double)viewport.X / Map.TileWidth);
                xEnd = xStart + (int)Math.Floor((double)viewport.Width / Map.TileWidth) + 1;
            }
            else {
                xStart = (int)Math.Ceiling((double)viewport.X / Map.TileWidth);
                xEnd = xStart + (int)Math.Ceiling((double)viewport.Width / Map.TileWidth) + 1;
            }

            if (viewport.Y >= 0) {
                yStart = (int)Math.Floor((double)viewport.Y / Map.TileHeight);
                yEnd = yStart + (int)Math.Floor((double)viewport.Height / Map.TileHeight) + 1;
            }
            else {
                yStart = (int)Math.Ceiling((double)viewport.Y / Map.TileHeight);
                yEnd = yStart + (int)Math.Ceiling((double)viewport.Height / Map.TileHeight) + 1;
            }

            // Ensure we're actually drawing stuff that's inside the map
            //iStart = Math.Max(iStart, 0);
            //jStart = Math.Max(jStart, 0);

            // Find how much of tiles at the edges of the screen will be missing
            xOffset = -1 * viewport.X % Map.TileWidth;
            yOffset = -1 * viewport.Y % Map.TileHeight;

            //Console.WriteLine("{0} {1} {2} {3}", iStart, iEnd, viewport.X, xOffset);

            for (var i = xStart; i < xEnd; i++) {
                for (var j = yStart; j < yEnd; j++) {
                    if (i < 0 || j < 0 || i >= Map.Width || j >= Map.Height) continue;
                    var cell = Map.Cells[i, j];

                    var position = new Vector2(
                        Map.TileWidth * (i - xStart) + xOffset,
                        Map.TileHeight * (j - yStart) + yOffset);

                    if (!Player.current.CanSee(cell)) {
                        continue;
                    }

                    foreach (var tile in cell.Tiles) {
                        if (tile.TileSheet == null) {
                            // Just an empty tile
                            continue;
                        }

                        batch.Draw(tile.TileSheet, position,
                                tile.Rectangle, Color.White, 0.0f, new Vector2(0, 0),
                                1, SpriteEffects.None, 0);
                    }

                    foreach (var item in cell.Items) {
                        var tile = item.Tile;

                        batch.Draw(tile.TileSheet, position,
                                   tile.Rectangle, Color.White, 0.0f, new Vector2(0, 0),
                                   1, SpriteEffects.None, 0);
                    }

                    foreach (var cre in cell.Creatures) {
                        var tile = cre.Tile;

                        var effects = SpriteEffects.None;
                        if (cre.Facing == Direction.Right) {
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
}
