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

    /// <summary>
    /// A Creature is any kind of living (or undead?) entity, like the player or an NPC.
    /// </summary>
    public class Creature {
        /// <summary>
        /// Cell on the map currently containing this creature
        /// </summary>
        public Cell Cell;

        /// <summary>
        /// List of tiles representing the creature. Will be cycled through to animate.
        /// </summary>
        public List<Tile> Tiles;

        /// <summary>
        /// Which tile in the creature's list of tiles is active.
        /// </summary>
        public int TileIndex;

        public List<Item> Inventory;

        /// <summary>
        /// Gets the currently active tile.
        /// </summary>
        public Tile Tile {
            get { return Tiles[TileIndex]; }
        }

        /// <summary>
        /// Which of the eight directions the creature is facing (only meaningful for Right/Left atm)
        /// </summary>
        public Direction Facing;

        public Creature() {
            Tiles = new List<Tile> { Tile.Blank };
            Facing = Direction.Right;
            Inventory = new List<Item>();

            // Keep Cell null so you can make creatures before deciding where they go
            Cell = null;
        }

        public void Bobble() {
            TileIndex += 1;
            if (TileIndex >= Tiles.Count) {
                TileIndex = 0;
            }
        }

        /// <summary>
        /// Determines whether this creature can move through a given cell.
        /// </summary>
        public bool CanPass(Cell cell) {
            foreach (var tile in cell.Tiles) {
                if (tile.Flags.Obstacle) {
                    return false;
                }
            }

            if (cell.Creatures.Count > 0) {
                // Can't walk through other creatures!
                return false;
            }

            return true;
        }

        /// <summary>
        /// Puts the creature into a cell, removing from old if needed.
        /// </summary>
        public void Move(Cell newCell) {
            if (Cell != null) {
                Cell.Creatures.Remove(this);
            }
            newCell.Creatures.Add(this);
            Cell = newCell;

            if (newCell.Items.Count > 0) {
                foreach (var item in newCell.Items.ToArray()) {
                    PickUp(newCell, item);
                }
            }
        }

        /// <summary>
        /// Place an item in the creature's inventory.
        /// </summary>
        public void PickUp(Cell cell, Item item) {
            cell.Items.Remove(item);
            Inventory.Add(item);
            if (this is Player)
            {
                DungeonGame.current.PrintMessage("You picked up an item.");
            }
            else
            {
                DungeonGame.current.PrintMessage("A creature picked up an item.");
            }
        }

        public void TakeTurn() {
            // Choose a random cell to move to
            var cells = Cell.FindNeighbors().FindAll((Cell cell) => CanPass(cell)).ToList();
            if (cells.Count > 0) {
                Console.WriteLine("{0}", cells.Count);
                Move(cells[DungeonGame.Random.Next(0, cells.Count)]);
            }
        }
    }
}
