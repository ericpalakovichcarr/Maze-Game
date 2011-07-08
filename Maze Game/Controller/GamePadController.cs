using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace Maze_Game.Controller {
	
	public class GamePadController : GameController {

        private bool m_leftBumperDown;
        private bool m_rightBumperDown;

		public GamePadController(PlayerIndex playerIndex) : base (playerIndex) {
            m_leftBumperDown = false;
            m_rightBumperDown = false;
        }

        public override bool PausePressed {
			get {
				return GamePad.GetState(Player).IsButtonDown(Buttons.Start);
			}
		}

		public override Vector2 MoveDirection {
			get {
                Vector2 velocity = GamePad.GetState(Player).ThumbSticks.Left;
                velocity.Y = -velocity.Y;
                return velocity;
			}
		}

        public override void Update() {
            if (m_leftBumperDown && GamePad.GetState(Player).IsButtonUp(Buttons.LeftShoulder))
                m_leftBumperDown = false;
            if (m_rightBumperDown && GamePad.GetState(Player).IsButtonUp(Buttons.RightShoulder))
                m_rightBumperDown = false;
        }

        public override bool PrevPlayer {
            get {
                if (!m_leftBumperDown) {
                    m_leftBumperDown = GamePad.GetState(Player).IsButtonDown(Buttons.LeftShoulder);
                    return m_leftBumperDown;
                }
                else
                    return false;
            }
        }
        public override bool NextPlayer {
            get {
                if (!m_rightBumperDown) {
                    m_rightBumperDown = GamePad.GetState(Player).IsButtonDown(Buttons.RightShoulder);
                    return m_rightBumperDown;
                }
                else
                    return false;
            }
        }
	}
}
