using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Maze_Game.Camera;

namespace Maze_Game.StageObjects {

    public enum PlayerAnimation {
        IdleUp, IdleDown, IdleLeft, IdleRight,
        WalkingUp, WalkingDown, WalkingLeft, WalkingRight,
        AllFrames
    }

    public class PlayerFrame {
        private int m_frameIndex;
        private int m_xoffset;
        private int m_yoffset;
        private bool m_mirrored;

        public PlayerFrame(int frameIndex, int xoffset, int yoffset, bool mirrored) {
            m_frameIndex = frameIndex;
            m_xoffset = xoffset;
            m_yoffset = yoffset;
            m_mirrored = mirrored;
        }

        public Vector2 Adjust(Vector2 position) {
            position.X += m_xoffset;
            position.Y += m_yoffset;

            return position;
        }

        public int FrameIndex {
            get { return m_frameIndex; }
        }

        public bool Mirrored {
            get { return m_mirrored; }
        }

        public int XOffset {
            get { return m_xoffset; }
        }

        public int YOffset {
            get { return m_yoffset; }
        }
    }

    public class PlayerSprite {

        #region Attributes and Constructors

        private Sprite m_sprite;
        private Dictionary<PlayerAnimation, int> Animation_To_Index;
        private List<List<PlayerFrame>> m_animations;
        private List<PlayerFrame> m_currentAnimation;
        private int m_currentFrame;
        private TimeSpan m_timeBetweenFrames;
        private TimeSpan m_timeSinceLastFrame;
        private bool m_animationHasntChanged;

        public PlayerSprite(Sprite sourceSprite)
        {
            m_sprite = sourceSprite;

            // Create the slots for each animation
            m_animations = new List<List<PlayerFrame>>();
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());
            m_animations.Add(new List<PlayerFrame>());

            // Match each animation enumeration to a slot
            Animation_To_Index = new Dictionary<PlayerAnimation, int>();
            Animation_To_Index.Add(PlayerAnimation.IdleDown, 0);
            Animation_To_Index.Add(PlayerAnimation.IdleUp, 1);
            Animation_To_Index.Add(PlayerAnimation.IdleLeft, 2);
            Animation_To_Index.Add(PlayerAnimation.IdleRight, 3);
            Animation_To_Index.Add(PlayerAnimation.WalkingDown, 4);
            Animation_To_Index.Add(PlayerAnimation.WalkingUp, 5);
            Animation_To_Index.Add(PlayerAnimation.WalkingLeft, 6);
            Animation_To_Index.Add(PlayerAnimation.WalkingRight, 7);
            Animation_To_Index.Add(PlayerAnimation.AllFrames, 8);

            // Setup the idle animation
            AddFrame(PlayerAnimation.IdleDown, 1, 0, 0, false);
            AddFrame(PlayerAnimation.IdleUp, 2, 0, 0, false);
            AddFrame(PlayerAnimation.IdleLeft, 3, 0, 0, false);
            AddFrame(PlayerAnimation.IdleRight, 3, 0, 0, true);
            
            // Setup the moving down animation
            AddFrame(PlayerAnimation.WalkingDown, 6, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingDown, 7, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingDown, 8, 0, -1, false);
            AddFrame(PlayerAnimation.WalkingDown, 9, 0, -1, false);
            AddFrame(PlayerAnimation.WalkingDown, 10, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingDown, 12, 0, -1, false);
            AddFrame(PlayerAnimation.WalkingDown, 13, 0, -1, false);

            // Setup the moving left animation
            AddFrame(PlayerAnimation.WalkingLeft, 15, -1, 0, false);
            AddFrame(PlayerAnimation.WalkingLeft, 16, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingLeft, 17, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingLeft, 18, -1, 0, false);
            AddFrame(PlayerAnimation.WalkingLeft, 19, 0, 0, false);

            // Setup the moving up animation
            AddFrame(PlayerAnimation.WalkingUp, 47, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingUp, 48, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingUp, 49, 0, -1, false);
            AddFrame(PlayerAnimation.WalkingUp, 50, 0, -1, false);
            AddFrame(PlayerAnimation.WalkingUp, 51, 0, 0, false);
            AddFrame(PlayerAnimation.WalkingUp, 52, 0, -1, false);
            AddFrame(PlayerAnimation.WalkingUp, 53, 0, -1, false);
            AddFrame(PlayerAnimation.WalkingUp, 54, 0, 0, false);

            // Setup the moving right animation
            AddFrame(PlayerAnimation.WalkingRight, 15, -1, 0, true);
            AddFrame(PlayerAnimation.WalkingRight, 16, 0, 0, true);
            AddFrame(PlayerAnimation.WalkingRight, 17, 0, 0, true);
            AddFrame(PlayerAnimation.WalkingRight, 18, -1, 0, true);
            AddFrame(PlayerAnimation.WalkingRight, 19, 0, 0, true);

            for (int i = 0; i < m_sprite.NumFrames; i++)
                AddFrame(PlayerAnimation.AllFrames, i, 0, 0, false);

            // Setup the general settings for animations
            m_currentAnimation = m_animations[AtoI(PlayerAnimation.IdleDown)];
            m_currentFrame = 0;
            m_timeSinceLastFrame = new TimeSpan(0);
            m_timeBetweenFrames = new TimeSpan(0, 0, 0, 0, 60);
            m_animationHasntChanged = false;
        }

        #endregion

        #region Animation Methods

        private int AtoI(PlayerAnimation animation) {
            return Animation_To_Index[animation];
        }

        private void AddFrame(PlayerAnimation animation, int cellIndex, int xoffset, int yoffset, bool mirrored) {
            PlayerFrame frame = new PlayerFrame(cellIndex, xoffset, yoffset, mirrored);
            m_animations[Animation_To_Index[animation]].Add(frame);
        }

        public void SetAnimation(PlayerAnimation animation) {
            if (m_currentAnimation != m_animations[AtoI(animation)]) {
                m_currentAnimation = m_animations[AtoI(animation)];
                m_currentFrame = 0;
                m_timeSinceLastFrame = new TimeSpan(0);
                m_animationHasntChanged = false;
            }
        }

        /// <summary>
        /// Moves to the next frame in the currently running animation
        /// </summary>
        public void NextAnimationFrame() {
            m_currentFrame++;
            if (m_currentFrame >= m_currentAnimation.Count)
                m_currentFrame = 0;
        }
        /// <summary>
        /// Moves to the previous frame in the currently running animation
        /// </summary>
        public void PrevAnimationFrame() {
            m_currentFrame--;
            if (m_currentFrame < 0)
                m_currentFrame = m_currentAnimation.Count - 1;
        }

        /// <summary>
        /// Updates the animation based on the time that has elapsed in the
        /// framerate of the game.
        /// </summary>
        /// <param name="timeElapsed"></param>
        public void Update(TimeSpan timeElapsed) {
            m_timeSinceLastFrame += timeElapsed;

            // Go to the next frame of animation (or skip frames if that much time has passed)
            if (m_animationHasntChanged && m_timeSinceLastFrame > m_timeBetweenFrames) {
                do {
                    m_timeSinceLastFrame -= m_timeBetweenFrames;
                    NextAnimationFrame();
                } while (m_timeSinceLastFrame > m_timeBetweenFrames);
            }

            m_animationHasntChanged = true;
        }

        public virtual void Draw(SpriteBatch batch, Vector2 position) {
            position.X += m_currentAnimation[m_currentFrame].XOffset;
            position.Y += m_currentAnimation[m_currentFrame].YOffset;
            SpriteEffects effects = m_currentAnimation[m_currentFrame].Mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            batch.Draw(m_sprite.Texture,
                       position,
                       m_sprite.GetSourceFrame(m_currentAnimation[m_currentFrame].FrameIndex),
                       Color.White, 0, Vector2.Zero, 1, effects, 0);
        }

        public virtual void Draw(SpriteBatch batch, Vector2 position, StageCamera camera) {
            position.X += m_currentAnimation[m_currentFrame].XOffset;
            position.Y += m_currentAnimation[m_currentFrame].YOffset;
            SpriteEffects effects = m_currentAnimation[m_currentFrame].Mirrored ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            batch.Draw(m_sprite.Texture,
                       camera.ToCameraPosition(position),
                       m_sprite.GetSourceFrame(m_currentAnimation[m_currentFrame].FrameIndex),
                       Color.White, 0, Vector2.Zero, 1, effects, 0);
        }

        #endregion
    }
}