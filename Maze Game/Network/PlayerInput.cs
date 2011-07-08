using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Maze_Game.Network {
    
    public class PlayerInput {
        public Vector2 movement;

        public PlayerInput(Vector2 movement) {
            this.movement = movement;
        }
    }
}