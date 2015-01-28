using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dungeon
{
    public class Player : Creature
    {
        public List<Cell> CurrentPath;

        public Player() : base() {
        }

    }
}
