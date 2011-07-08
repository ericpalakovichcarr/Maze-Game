using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Maze_Game.Camera;

namespace Maze_Game.StageObjects {

    /// <summary>
    /// An object that can be drawn onto a stage
    /// </summary>
    public interface StageEntity {

        /// <summary>
        /// The position of the graphic in stage units (not screen units).
        /// </summary>
        Vector2 Position {
            get;
        }

        /// <summary>
        /// The boudns of the graphic in stage units (not screen units).
        /// </summary>
        Rectangle Bounds {
            get;
        }

        /// <summary>
        /// Draws the object to the screen.
        /// </summary>
        /// <param name="batch">The sprite batch the entity should be drawn with.</param>
        void Draw(SpriteBatch batch, StageCamera camera);
    }
}
