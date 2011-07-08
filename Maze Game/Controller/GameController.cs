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
	
	public abstract class GameController {

		#region Attributes, Properties, and Constructors

        public static byte MOVING_UP = 1;
        public static byte MOVING_RIGHT = 2;
        public static byte SOFT_LEFT = 4;
        public static byte HARD_LEFT = 8;
        public static byte SOFT_DOWN = 16;
        public static byte HARD_DOWN = 32;
        public static byte SOFT_RIGHT = 64;
        public static byte HARD_RIGHT = 128;

		public static GameController player1, player2, player3, player4;
        private PlayerIndex _playerIndex;
        
		public PlayerIndex Player {
			get {
				return _playerIndex;
			}
		}

        public static GameController Get(int index) {
            switch (index) {
                case 0:
                    return player1;
                case 1:
                    return player2;
                case 2:
                    return player3;
                case 3:
                    return player4;
                default:
                    throw new IndexOutOfRangeException(
                        string.Format("Player {0} is an invalid player.", index + 1));
            }
        }

		public GameController(PlayerIndex playerIndex) {
			_playerIndex = playerIndex;
		}

		static GameController() {
			player1 = new GamePadController(PlayerIndex.One);
			player2 = new GamePadController(PlayerIndex.Two);
			player3 = new GamePadController(PlayerIndex.Three);
			player4 = new GamePadController(PlayerIndex.Four);
		}

        public abstract void Update();
		public abstract bool PausePressed { get; }
		public abstract Vector2 MoveDirection { get; }
        public abstract bool NextPlayer { get; }
        public abstract bool PrevPlayer { get; }

		#endregion
	}
}
