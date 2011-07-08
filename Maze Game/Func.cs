using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Maze_Game {
    
    public class Global {
        public const int screenWidth = 1067;
        public const int screenHeight = 600;
        public const bool fullscreen = false;
        public static NetworkSession networkSession;
        public static PacketWriter packetWriter = new PacketWriter();
        public static PacketReader packetReader = new PacketReader();
    }

    public class Func {

        public static Vector2 Center(Rectangle rect) {
            return new Vector2((rect.Width / 2) + rect.X,
                               (rect.Height / 2) + rect.Y);
        }

        /// <summary>
        /// Determines if two rectangles have collided with each other.
        /// </summary>
        public static bool Collided(Rectangle rect1, Rectangle rect2) {
            // Test if the first rectangle is outside of the second rectangle
            if( rect1.Top > rect2.Bottom )
                return false;
            if( rect1.Bottom < rect2.Top )
                return false;
            if( rect1.Left > rect2.Right )
                return false;
            if (rect1.Right < rect2.Left)
                return false;

            // since the first rectangle isn't outside of the second, then a collision must have occurred
            return true;
        }

        public static Rectangle Intersection(Rectangle rect1, Rectangle rect2) {
            Rectangle intersection = new Rectangle(0, 0, 0, 0);

            if (rect1.Intersects(rect2)) {

            }

            return intersection;
        }
    }
}
