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

    // <summary>
    // A Creature is any kind of living (or undead?) entity, like the player or an NPC.
    // </summary>
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

        public int MaxHealth = 15;
        public int Health = 15;

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

            SendMessage("You picked up an item.");
            if (!(this is Player)) {
                Player.current.SendMessage("A creature picked up an item.");
            }
        }

        /// <summary>
        /// Determines whether another creature should be considered an enemy.
        /// </summary>
        public bool IsEnemy(Creature cre) {
            if (this is Player) {
                return !(cre is Player);
            } else {
                return cre is Player;
            }
        }

        /// <summary>
        /// Sends message to a creature. If the creature is the player, it will be printed.
        /// </summary>
        public void SendMessage(string msg) {
            if (this is Player) {
                DungeonGame.current.PrintMessage(msg);
            }            
        }

        public void Attack(Creature cre) {
            var damage = 3;
            cre.Health -= damage;
            cre.SendMessage("You lost " + damage + " hit points.");
        }

        public void Heal(int amount) {
            if (Health == MaxHealth) {
                SendMessage("You are already at full health!");
            } else if (Health + amount > MaxHealth) {
                Health = MaxHealth;
                SendMessage("You've been restored by " + amount + " hit points.");
                SendMessage("You have been restored to full health.");
            } else {
                Health += amount;
                SendMessage("You've been restored by " + amount + " hit points.");
            }
        }

        public bool IsDead() {
            return (Health <= 0);
        }

        public void TakeTurn() {
            // Find any adjacent enemies to attack
            var enemies = new List<Creature>();
            foreach (var cell in Cell.FindNeighbors()) {
                foreach (var cre in cell.Creatures) {
                    if (IsEnemy(cre)) {
                        enemies.Add(cre);
                    }
                }
            }

            if (enemies.Count > 0) {
                // TODO: prioritize if there is more than one enemy
                Attack(enemies[0]);
            } else { 
                // TODO: move towards any visible enemies
                
                // Choose a random cell to move to
                var cells = Cell.FindNeighbors().FindAll((Cell cell) => CanPass(cell)).ToList();
                if (cells.Count > 0) {
                    //Console.WriteLine("{0}", cells.Count);
                    Move(cells[DungeonGame.Random.Next(0, cells.Count)]);
                }            
            }
        }
    }
}
