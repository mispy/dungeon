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
    public class OutOfBoundsException : Exception {
    }

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
                        var cre = tileType.Flags.Player ? new Player() : new Creature();

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

        public List<Cell> PathBetween(Cell start, Cell end, Func<Cell, bool> passable) {
            // nodes that have already been analyzed and have a path from the start to them
            var closedSet = new List<Cell>();
            // nodes that have been identified as a neighbor of an analyzed node, but have 
            // yet to be fully analyzed
            var openSet = new List<Cell> { start };
            // a dictionary identifying the optimal origin Cell to each node. this is used 
            // to back-track from the end to find the optimal path
            var cameFrom = new Dictionary<Cell, Cell>();
            // a dictionary indicating how far each analyzed node is from the start
            var currentDistance = new Dictionary<Cell, int>();
            // a dictionary indicating how far it is expected to reach the end, if the path 
            // travels through the specified node. 
            var predictedDistance = new Dictionary<Cell, float>();

            // initialize the start node as having a distance of 0, and an estmated distance 
            // of y-distance + x-distance, which is the optimal path in a square grid that 
            // doesn't allow for diagonal movement
            currentDistance.Add(start, 0);
            predictedDistance.Add(
                start,
                0 + +Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y)
                );

            // if there are any unanalyzed nodes, process them
            while (openSet.Count > 0) {
                // get the node with the lowest estimated cost to finish

                var current = (
                    from p in openSet orderby predictedDistance[p] ascending select p
                ).First();

                // if it is the finish, return the path
                if (current.X == end.X && current.Y == end.Y) {
                    // generate the found path
                    return ReconstructPath(cameFrom, end);
                }

                // move current node from open to closed
                openSet.Remove(current);
                closedSet.Add(current);

                // process each valid node around the current node
                foreach (var neighbor in current.FindNeighbors()) {
                    if (!passable(neighbor)) {
                        continue;
                    }

                    var tempCurrentDistance = currentDistance[current] + 1;

                    // if we already know a faster way to this neighbor, use that route and 
                    // ignore this one
                    if (closedSet.Contains(neighbor)
                        && tempCurrentDistance >= currentDistance[neighbor]) {
                        continue;
                    }

                    // if we don't know a route to this neighbor, or if this is faster, 
                    // store this route
                    if (!closedSet.Contains(neighbor)
                        || tempCurrentDistance < currentDistance[neighbor]) {
                        if (cameFrom.Keys.Contains(neighbor)) {
                            cameFrom[neighbor] = current;
                        }
                        else {
                            cameFrom.Add(neighbor, current);
                        }

                        currentDistance[neighbor] = tempCurrentDistance;
                        predictedDistance[neighbor] =
                            currentDistance[neighbor]
                            + Math.Abs(neighbor.X - end.X)
                                + Math.Abs(neighbor.Y - end.Y);

                        // if this is a new node, add it to processing
                        if (!openSet.Contains(neighbor)) {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            // unable to figure out a path, abort.
            return null;
        }

        /// <summary>
        /// Process a list of valid paths generated by the Pathfind function and return 
        /// a coherent path to current.
        /// </summary>
        /// <param name="cameFrom">A list of nodes and the origin to that node.</param>
        /// <param name="current">The destination node being sought out.</param>
        /// <returns>The shortest path from the start to the destination node.</returns>
        private List<Cell> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell current) {
            if (!cameFrom.Keys.Contains(current)) {
                return new List<Cell> { current };
            }

            var path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }
    }
}