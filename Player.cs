using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dungeon
{
    public class Player : Creature
    {
        public static Player current;
        public List<Cell> CurrentPath;
        public int max_health = 15;
        public int cur_health = 15;

        public Player() : base() {
            current = this;
        }

        /// <summary>
        /// Checks if the cell is inside the player's FOV.
        /// </summary>
        public bool CanSee(Cell cell) {
            return Cell.DistanceTo(cell) < 5;
        }

        public void hurt_player(int damage){
            cur_health -= damage;
            DungeonGame.current.PrintMessage("You lost " + damage + " hit points.");
        }

        public void heal_player(int amount)
        {
            if (cur_health == max_health)
            {
                DungeonGame.current.PrintMessage("You are already at full health!");
            }
            else if (cur_health + amount > max_health)
            {
                cur_health = max_health;
                DungeonGame.current.PrintMessage("You've been restored by " + amount + " hit points.");
                DungeonGame.current.PrintMessage("You have been restored to full health.");
            }
            else
            {
                cur_health += amount;
                DungeonGame.current.PrintMessage("You've been restored by " + amount + " hit points.");
            }
        }

        public bool isDead()
        {
            if (cur_health <= 0)
            {
                return true;
            }
            return false;
        }


    }
}
