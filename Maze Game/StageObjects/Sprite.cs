using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Maze_Game.Camera;

namespace Maze_Game.StageObjects {

	public class Sprite {

		#region Attributes and Constructors

		protected Texture2D m_sprite;
		protected List<Rectangle> m_sourceRects;

		public Sprite(Texture2D sourceSprite, List<Rectangle> sourceRects) {
			m_sprite = sourceSprite;
            m_sourceRects = sourceRects;
		}

		#endregion

		#region Properties

		public Texture2D Texture {
            get {
                return m_sprite;
            }
        }

        public int NumFrames {
            get {
                return m_sourceRects.Count;
            }
        }

		#endregion

		#region Methods

        public Rectangle GetSourceFrame(int frameIndex) {
            return m_sourceRects[frameIndex];
        }

        public int GetWidth(int frameIndex) {
            return m_sourceRects[frameIndex].Width;
        }

        public int GetHeight(int frameIndex) {
            return m_sourceRects[frameIndex].Height;
        }

		public virtual void Draw(SpriteBatch batch, Vector2 position, int frameIndex) {
			batch.Draw(m_sprite, position, m_sourceRects[frameIndex], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}

        public virtual void Draw(SpriteBatch batch, Vector2 position, int frameIndex, StageCamera camera) {
            batch.Draw(m_sprite, camera.ToCameraPosition(position), m_sourceRects[frameIndex], Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

		#endregion
	}
}
