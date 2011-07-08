using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using Maze_Game.Camera;
using Maze_Game.StageObjects;
using Maze_Game.Controller;
using Maze_Game.StateManagement;

namespace Maze_Game {

	public class Player : StageEntity {

        #region Attributes, Properties, and Constructors

        private PlayerSprite m_playerSprite;
		private Vector2 m_position;
		private Vector2 m_velocity;
        private int m_collissionWidth;
        private int m_collissionHeight;
        private PlayerState m_state;
        private LocalNetworkGamer m_networkGamer;  // should only be set for the local player

        /// <summary>
        /// Gets and sets the current state of the player.
        /// </summary>
        public PlayerState State {
            get { return m_state; }
            set { m_state = value; }
        }
        public FacingDirection Facing { 
            get { return m_state.Facing; }
            set { m_state.Facing = value; }
        }
        public bool Active {
            get { return m_state.Active; }
            set { m_state.Active = value; }
        }
        public Vector2 MovingVelocity {
            get {
                int x = 0, y = 0;
                int maxSpeed = 4, halfSpeed = 2, cornerSpeed = 3;
                byte direction = m_state.DirectionFlags;
                if (direction > 0) {
                    if ((direction & PlayerState.DIRECTION_HARD_DOWN) > 0) y = maxSpeed;
                    if ((direction & PlayerState.DIRECTION_SOFT_DOWN) > 0) y = halfSpeed;
                    if ((direction & PlayerState.DIRECTION_HARD_UP) > 0) y = -maxSpeed;
                    if ((direction & PlayerState.DIRECTION_SOFT_UP) > 0) y = -halfSpeed;
                    if ((direction & PlayerState.DIRECTION_HARD_RIGHT) > 0) x = maxSpeed;
                    if ((direction & PlayerState.DIRECTION_SOFT_RIGHT) > 0) x = halfSpeed;
                    if ((direction & PlayerState.DIRECTION_HARD_LEFT) > 0) x = -maxSpeed;
                    if ((direction & PlayerState.DIRECTION_SOFT_LEFT) > 0) x = -halfSpeed;

                    // if moving hard on both directions, use the middle ground for the speed
                    if (Math.Abs(y) == maxSpeed && Math.Abs(x) == maxSpeed) {
                        y = cornerSpeed * (y < 0 ? -1 : 1);
                        x = cornerSpeed * (x < 0 ? -1 : 1);
                    }
                }

                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Gets and sets the current position of the player.
        /// </summary>
        public Vector2 Position {
            get { return m_position; }
			set {
                m_position = value;
                m_position.X = (int)m_position.X;
                m_position.Y = (int)m_position.Y;
            }
		}

        public Vector2 Velocity {
            get { return m_velocity; }
        }
        public void SetVelocityX(float x) { m_velocity.X = x; }
        public void SetVelocityY(float y) { m_velocity.Y = y; }
        public void SetVelocity(Vector2 v) { m_velocity = v; }
        public void AddVelcotiyX(float x) { m_velocity.X += x; }
        public void AddVelocityY(float y) { m_velocity.Y += y; }
        public void AddVelocity(Vector2 v) { m_velocity += v; }

		public Rectangle Bounds {
			get {
				return new Rectangle((int)Position.X,
									 (int)Position.Y,
									 m_collissionWidth,
									 m_collissionHeight);
			}
		}

		public Vector2 BoundsCenter {
			get {
				return new Vector2(Position.X + (Bounds.Width / 2), Position.Y + (Bounds.Height / 2));
			}
		}

        public LocalNetworkGamer Net {
            get { return m_networkGamer; }
        }

        public Player(PlayerSprite playerSprite) : this(playerSprite, null) { }
		public Player(PlayerSprite playerSprite, LocalNetworkGamer gamer) {
            m_state = new PlayerState(new Vector2(0, 0), FacingDirection.South);
			m_playerSprite = playerSprite;
			Position = new Vector2(1, 1);
            m_velocity = new Vector2(0, 0);
            m_collissionWidth = 32;
            m_collissionHeight = 44;
            m_playerSprite.SetAnimation(PlayerAnimation.IdleDown);
            m_networkGamer = gamer;
            if (gamer != null)
                gamer.Tag = this;
		}

		#endregion

		#region Methods

        public void Move(int x, int y) {
            MoveX(x);
            MoveY(y);
        }

        public void MoveX(int x) {
            m_position.X += x;
        }

        public void MoveY(int y) {
            m_position.Y += y;
        }

        public void SetPos(int x, int y) {
            SetXPos(x);
            SetYPos(y);
        }

        public void SetXPos(int x) {
            m_position.X = x;
        }

        public void SetYPos(int y) {
            m_position.Y = y;
        }

		public void ApplyVelocity() {
            m_position += m_velocity;
            m_position.X = (int)m_position.X;
            m_position.Y = (int)m_position.Y;
		}

        public void Update(GameTime gameTime) {
            if (m_velocity.X != 0 || m_velocity.Y != 0) {
                if (Math.Abs(m_velocity.X) > Math.Abs(m_velocity.Y)) {
                    if (m_velocity.X > 0) {
                        m_playerSprite.SetAnimation(PlayerAnimation.WalkingRight);
                    }
                    else {
                        m_playerSprite.SetAnimation(PlayerAnimation.WalkingLeft);
                    }
                }
                else {
                    if (m_velocity.Y > 0) {
                        m_playerSprite.SetAnimation(PlayerAnimation.WalkingDown);
                    }
                    else {
                        m_playerSprite.SetAnimation(PlayerAnimation.WalkingUp);
                    }
                }
            }
            else {
                if (Facing == FacingDirection.North)
                    m_playerSprite.SetAnimation(PlayerAnimation.IdleUp);
                else if (Facing == FacingDirection.East)
                    m_playerSprite.SetAnimation(PlayerAnimation.IdleLeft);
                else if (Facing == FacingDirection.West)
                    m_playerSprite.SetAnimation(PlayerAnimation.IdleRight);
                else
                    m_playerSprite.SetAnimation(PlayerAnimation.IdleDown);
            }

            m_playerSprite.Update(gameTime.ElapsedGameTime);
        }

		public void Draw(SpriteBatch batch, StageCamera camera) {
            m_playerSprite.Draw(batch, Position, camera);
            m_velocity.X = m_velocity.Y = 0;
		}

		#endregion
	}
}
