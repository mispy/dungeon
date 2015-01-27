using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dungeon {
    // <summary>
    // Map component which draws a Map on the screen.
    // </summary>
    public class MapRenderer {
        private Map Map;

        public MapRenderer(Map map) {
            Map = map;
        }

        /// <summary>
        /// Draws the map.
        /// <param name="batch">SpriteBatch on which to draw.</param>
        /// <param name="viewport">Rectangle of the map to draw. Specifed in pixel, not tile coordinates, to enable smooth scrolling.</param>
        /// </summary>
        public void Draw(SpriteBatch batch, Rectangle viewport) {

            // Convert pixel coordinates to the corresponding tiles
            int iStart, iEnd, jStart, jEnd;
            if (viewport.X >= 0) {
                iStart = (int)Math.Floor((double)viewport.X / Map.TileWidth);
                iEnd = iStart + (int)Math.Floor((double)viewport.Width / Map.TileWidth) + 1;
            }
            else {
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
            //iStart = Math.Max(iStart, 0);
            //jStart = Math.Max(jStart, 0);
            iEnd = Math.Min(iEnd, Map.Width);
            jEnd = Math.Min(jEnd, Map.Height);

            // Find how much of tiles at the edges of the screen will be missing
            var xOffset = -1 * viewport.X % Map.TileWidth;
            var yOffset = -1 * viewport.Y % Map.TileHeight;

            //Console.WriteLine("{0} {1} {2} {3}", iStart, iEnd, viewport.X, xOffset);

            for (var i = iStart; i < iEnd; i++) {
                for (var j = jStart; j < jEnd; j++) {
                    if (i < 0 || j < 0) continue;
                    var cell = Map.Cells[i, j];

                    var position = new Vector2(
                        Map.TileWidth * (i - iStart) + xOffset,
                        Map.TileHeight * (j - jStart) + yOffset);

                    foreach (var tile in cell.Tiles) {
                        if (tile.TileSheet == null) {
                            // Just an empty tile
                            continue;
                        }

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
