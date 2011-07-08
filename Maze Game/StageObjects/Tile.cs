using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Maze_Game.Camera;

namespace Maze_Game.StageObjects {

    /// <summary>
    /// A tile on the tilemap.
    /// </summary>
    public class Tile : StageEntity {
        private Sprite m_texture;
        private Rectangle m_bounds;
        private bool m_canCollide;

        public Vector2 Position { get { return new Vector2(m_bounds.X, m_bounds.Y); } }
        public Rectangle Bounds { get { return m_bounds; } }
        public bool CanCollide { get { return m_canCollide; } }

        public Tile(Sprite texture, Rectangle bounds, bool canCollide) {
            m_texture = texture;
            m_bounds = bounds;
            m_canCollide = canCollide;
        }

        public void Draw(SpriteBatch batch, StageCamera camera) {
            m_texture.Draw(batch,
                           camera.ToCameraPosition(new Vector2(m_bounds.X, m_bounds.Y)),
                           0);
        }
    }
}