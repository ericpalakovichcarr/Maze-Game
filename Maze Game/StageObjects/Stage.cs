using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Net;
using Maze_Game.Controller;
using Maze_Game.Camera;
using Maze_Game.StateManagement;
using Maze_Game.GUI;

namespace Maze_Game.StageObjects {

    public struct NetInfo {
        public FacingDirection facing;
        public Vector2 position;
        public byte direction;

        public override string ToString() {
            string s = "Position: {0}  Facing: {1}  Direction: {2}";
            List<string> dirString = new List<string>();
            if ((direction & PlayerState.DIRECTION_HARD_UP) > 0)
                dirString.Add("HARD_UP");
            if ((direction & PlayerState.DIRECTION_HARD_DOWN) > 0)
                dirString.Add("HARD_DOWN");
            if ((direction & PlayerState.DIRECTION_HARD_LEFT) > 0)
                dirString.Add("HARD_LEFT");
            if ((direction & PlayerState.DIRECTION_HARD_RIGHT) > 0)
                dirString.Add("HARD_RIGHT");
            if ((direction & PlayerState.DIRECTION_SOFT_UP) > 0)
                dirString.Add("SOFT_UP");
            if ((direction & PlayerState.DIRECTION_SOFT_DOWN) > 0)
                dirString.Add("SOFT_DOWN");
            if ((direction & PlayerState.DIRECTION_SOFT_LEFT) > 0)
                dirString.Add("SOFT_LEFT");
            if ((direction & PlayerState.DIRECTION_SOFT_RIGHT) > 0)
                dirString.Add("SOFT_RIGHT");
            if (direction == PlayerState.DIRECTION_STOPPED)
                dirString.Add("STOPPED");

            return string.Format(s, position.ToString(), facing.ToString(), string.Join("|", dirString.ToArray()));
        }
    }

	/// <summary>
	/// Represents a single stage.  Handles orgnazing all the objects that exist in the maze, including
    /// players, enemies, traps, floor, walls, and anything else that interacts or composes the maze.
    /// Does not include the GUI for the players.
	/// </summary>
	public class Stage : DrawableGameComponent {

		#region Attributes, Properties and Constructors

		private Tile[,] m_tiles;
		private SpriteBatch m_batch;
		private StageContentManager m_content;
        private StageCamera m_camera;
        private Dictionary<NetworkGamer, Player> m_players;
        private Player m_player;
        private GameController m_controller;
        private Random rand;
        
        private NetworkDisplayGUI m_networkGUI;

        private bool HIGHLIGHT_SURROUNDING_TILES = false;

		private int NumTilesX {
			get {
				return m_tiles.GetLength(0);
			}
		}

		private int NumTilesY {
			get {
				return m_tiles.GetLength(1);
			}
		}

		private int TileWidth {
			get {
				return m_tiles[0, 0].Bounds.Width;
			}
		}

		private int TileHeight {
			get {
				return m_tiles[0, 0].Bounds.Height;
			}
		}

		private int StageWidth {
			get {
				return TileWidth * NumTilesX;
			}
		}

		private int StageHeight {
			get {
				return TileHeight * NumTilesY;
			}
		}

		public Stage(Game game, StageContentManager content, NetworkDisplayGUI networkGUI)
			: base(game)
        {
			m_batch = new SpriteBatch(game.GraphicsDevice);
			m_content = content;
            m_players = new Dictionary<NetworkGamer, Player>();
            Global.networkSession.GamerJoined += networkSession_GamerJoined;
            Global.networkSession.GamerLeft += networkSession_GamerLeft;
            m_networkGUI = networkGUI;

            rand = new Random(50);  // Let's make it always the same for now
		}

        #endregion

        #region Tile Methods

		public void CenterInTile(int x, int y, Player player) {
            player.Position = new Vector2((x * TileWidth) + (TileWidth / 2) - (player.Bounds.Width / 2),
											(y * TileHeight) + (TileHeight / 2) - (player.Bounds.Height / 2));
		}

        /// <summary>
        /// Returns a 3x3 matrix of tiles surrounding a bounding box.  The tile 
        /// at [1,1] is the tile the bounds are contained in.  If the bounds
        /// specified overlaps with multiple tiles the tile containing the largest
        /// area of the bounds will be used as the center tile.
        /// </summary>
        /// <param name="bounds">A rectangle on the tile map.  Must be smaller
        /// than the size of a tile.</param>
        /// <returns></returns>
        public Tile[,] GetSurroundingTiles(Rectangle bounds) {
            Tile[,] surroundingTiles = new Tile[3, 3];
            
            // Determine which tiles contain each point that makes the rectangle of the specified bounds.
            // Ignore points that are outside of the number of tiles in the stage.
            Vector2[] cTiles = new Vector2[4];
            int[] cAreas = new int[4];
            SetTilePositionAndArea(0, cTiles, cAreas, bounds, bounds.Left, bounds.Top);
            SetTilePositionAndArea(1, cTiles, cAreas, bounds, bounds.Right, bounds.Top);
            SetTilePositionAndArea(2, cTiles, cAreas, bounds, bounds.Left, bounds.Bottom);
            SetTilePositionAndArea(3, cTiles, cAreas, bounds, bounds.Right, bounds.Bottom);

            // Determine which tile contains the most of the bounds
            int cIndex = 0;
            for (int i = 1; i < 4; i++) {
                if (cAreas[i] > cAreas[cIndex])
                    cIndex = i;
            }

            // Get each tile surrounding the tile
            Vector2 stp = cTiles[cIndex];
            surroundingTiles[0, 0] = stp.X > 0 && stp.Y > 0 ?
                m_tiles[(int)stp.X - 1, (int)stp.Y - 1] : null;
            surroundingTiles[1, 0] = stp.Y > 0 ?
                m_tiles[(int)stp.X, (int)stp.Y - 1] : null;
            surroundingTiles[2, 0] = stp.X + 1 < NumTilesX && stp.Y > 0 ?
                m_tiles[(int)stp.X + 1, (int)stp.Y - 1] : null;

            surroundingTiles[0, 1] = stp.X > 0 ? m_tiles[(int)stp.X - 1, (int)stp.Y] : null;
            surroundingTiles[1, 1] = m_tiles[(int)stp.X, (int)stp.Y];
            surroundingTiles[2, 1] = stp.X + 1 < NumTilesX ? m_tiles[(int)stp.X + 1, (int)stp.Y] : null;

            surroundingTiles[0, 2] = stp.X > 0 && stp.Y + 1 < NumTilesY ?
                m_tiles[(int)stp.X - 1, (int)stp.Y + 1] : null;
            surroundingTiles[1, 2] = stp.Y + 1 < NumTilesY ?
                m_tiles[(int)stp.X, (int)stp.Y + 1] : null;
            surroundingTiles[2, 2] = stp.X + 1 < NumTilesX && stp.Y + 1 < NumTilesY ?
                m_tiles[(int)stp.X + 1, (int)stp.Y + 1] : null;

            return surroundingTiles;
        }

        private void SetTilePositionAndArea(int cIndex, Vector2[] cTiles, int[] cAreas, Rectangle bounds, int boundsX, int boundsY) {
            int tile_x, tile_y;

            // Get the x,y coordinate of the tile containing the point specified
            tile_x = (int)Math.Floor((double)boundsX / TileWidth);
            tile_y = (int)Math.Floor((double)boundsY / TileHeight);

            if (tile_x < NumTilesX && tile_y < NumTilesY) {
                cTiles[cIndex] = new Vector2(tile_x, tile_y);
                cAreas[cIndex] = GetAreaOfIntersection(GetTile(cTiles[cIndex]).Bounds, bounds);
            }
            else {
                cTiles[cIndex] = new Vector2(0, 0);
                cAreas[cIndex] = 0;
            }
        }

        public Tile GetTile(Vector2 position) {
            return m_tiles[(int)position.X, (int)position.Y];
        }

        public Tile GetTileAt(Vector2 coordinates) {
            // TODO: Yeah yeah, brute force algorithm.  I'm lazy alright?  Fix this if things slow down.
            Tile retTile = null;
            //Point coord = new Point((int)coordinates.X, (int)coordinates.Y);

            //foreach (Tile tile in m_tiles) {
            //    if (tile.Bounds.Contains(coord); {
            //        retTile = tile;
            //        break;
            //    }
            //}

            return retTile;
        }

        /// <summary>
        /// Returns the area of the rectangle that is the intersection between two rectangles.
        /// Assumes rect1 and rect2 already intersect
        /// </summary>
        public int GetAreaOfIntersection(Rectangle rect1, Rectangle rect2) {
            int x = Math.Max(rect1.Left, rect2.Left);
            int y = Math.Max(rect1.Top, rect2.Top);
            int width = Math.Min(rect1.Right, rect2.Right) - x;
            int height = Math.Min(rect1.Bottom, rect2.Bottom) - y;
	        Rectangle intersection = new Rectangle(x, y, width, height);

            return intersection.Width * intersection.Height;
        }

		#endregion

		#region Events

		/// <summary>
		/// Allows the game component to perform any initialization it needs to before starting
		/// to run.  This is where it can query for any required services and load content.
		/// </summary>
		public override void Initialize() {
			LoadContent();

			// Set all the wall and floor tiles
			m_tiles = new Tile[40, 40];
			for (int y = 0; y < 40; y++) {
				for (int x = 0; x < 40; x++) {
                    int width = m_content.TileWidth;
                    int height = m_content.TileHeight;
                    int x_pos = x * m_content.TileWidth;
                    int y_pos = y * m_content.TileHeight;
                    int random = rand.Next(10) + 1;
                    if (random != 10) {
                        m_tiles[x, y] = new Tile(
                            m_content.GroundSprite,
                            new Rectangle(x_pos, y_pos, width, height),
                            false);
                    }
                    else {
                        m_tiles[x, y] = new Tile(
                            m_content.WallSprite,
                            new Rectangle(x_pos, y_pos, width, height),
                            true);
                    }
				}
			}

            // Initialize the camera
            m_camera = new StageCamera(StageWidth, StageHeight, Global.screenWidth, Global.screenHeight);
            m_camera.FollowEntity(m_player);

            base.Initialize();
		}

		protected override void LoadContent() {
			base.LoadContent();
		}

		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime) {
            if (m_player != null) {
                if (!m_player.Active) {
                    CenterInTile(0, 0, m_player);
                    m_player.Active = true;
                }
                m_controller.Update();
                m_player.State.DirectionFlags = PlayerState.DirectionToFlags(m_controller.MoveDirection);
                SendPlayerState();

                Global.networkSession.Update();
                if (Global.networkSession == null)
                    return;

                // Update remote players
                ReadPlayerStates();
                foreach (NetworkGamer gamer in m_players.Keys) {
                    Player player = gamer.Tag as Player;
                    if (player.Active)
                        UpdatePlayer(gamer.Tag as Player, gameTime);
                }
            }

			base.Update(gameTime);
		}

        public void UpdatePlayer(Player player, GameTime gameTime) {
            Rectangle prevBounds = player.Bounds;
            if (player.MovingVelocity != Vector2.Zero)
                player.SetVelocity(player.MovingVelocity);
            player.ApplyVelocity();
            
            bool movingLeft = player.Velocity.X < 0;
            bool movingRight = player.Velocity.X > 0;
            bool movingUp = player.Velocity.Y < 0;
            bool movingDown = player.Velocity.Y > 0;
            bool moving = (movingLeft || movingRight || movingUp || movingDown);

            // Check for tile and stage collision
            if (moving) {
                CheckStageBoundryCollision(player);
                CheckTileCollision(player, prevBounds, moving, movingLeft, movingRight, movingUp, movingDown);
                //UnstickPlayerFromCorners(player, prevBounds, moving, movingLeft, movingRight, movingUp, movingDown);
            }

            player.Update(gameTime);
        }

        #region Collision Detection Methods

        private void CheckStageBoundryCollision(Player player) {
            // Stage boundry Collision for player
            float xPos = player.Position.X;
            float yPos = player.Position.Y;
            if (player.Bounds.Left < 0)
                xPos = 0;
            if (player.Bounds.Top < 0)
                yPos = 0;
            if (player.Bounds.Right > StageWidth)
                xPos = StageWidth - player.Bounds.Width;
            if (player.Bounds.Bottom > StageHeight)
                yPos = StageHeight - player.Bounds.Height;
            player.SetPos((int)xPos, (int)yPos);
        }

        private void CheckTileCollision(Player player, Rectangle prevBounds, bool moving,
                                        bool movingLeft, bool movingRight, bool movingUp, bool movingDown) {
            foreach (Tile tile in m_tiles) {
                if (tile.CanCollide && Func.Collided(tile.Bounds, player.Bounds)) {
                    // Place the player outside of the tile based on the direction the player was heading
                    if (movingRight && player.Bounds.Right > tile.Bounds.Left && prevBounds.Right <= tile.Bounds.Left)
                        player.SetXPos(tile.Bounds.Left - player.Bounds.Width);
                    else if (movingLeft && player.Bounds.Left < tile.Bounds.Right && prevBounds.Left >= tile.Bounds.Right)
                        player.SetXPos(tile.Bounds.Right);
                    if (movingDown && player.Bounds.Bottom > tile.Bounds.Top && prevBounds.Bottom <= tile.Bounds.Top)
                        player.SetYPos(tile.Bounds.Top - player.Bounds.Height);
                    else if (movingUp && player.Bounds.Top < tile.Bounds.Bottom && prevBounds.Top >= tile.Bounds.Bottom)
                        player.SetYPos(tile.Bounds.Bottom);
                }
            }
        }

        private void UnstickPlayerFromCorners(Player player, Rectangle prevBounds, bool moving,
                                        bool movingLeft, bool movingRight, bool movingUp, bool movingDown)
        {
            //// If the player is stuck, allow him to move in a direction that isn't blocking his path
            //if (player.Bounds == prevBounds) {
            //    int left = player.Bounds.X; int right = (player.Bounds.X + player.Bounds.Width) - 1;
            //    int top = player.Bounds.Y; int bottom = (player.Bounds.Y + player.Bounds.Height) - 1;
            //    left += (int)player.Velocity.X; right += (int)player.Velocity.X;
            //    top += (int)player.Velocity.Y; bottom += (int)player.Velocity.Y;

            //    Tile topLeft = GetTileAt(new Vector2(left, top));
            //    Tile topRight = GetTileAt(new Vector2(right, top));
            //    Tile bottomLeft = GetTileAt(new Vector2(left, bottom));
            //    Tile bottomRight = GetTileAt(new Vector2(right, bottom));

            //    // Check the corner's of the player's collision box, allowing the player to move that direction if no walls are in it's way
                
            //    if (movingUp) {
            //        if (topLeft != null && topRight != null && !topLeft.CanCollide && !topRight.CanCollide) {
            //            player.SetVelocityX(0);
            //            player.ApplyVelocity();
            //        }
            //    }
            //    else if (movingRight) {
            //        if (sTiles[2, 1] != null && !sTiles[2, 1].CanCollide) {
            //            player.SetVelocityY(0);
            //            player.ApplyVelocity();
            //        }
            //    }
            //    else if (movingDown) {
            //        if (sTiles[1, 2] != null && !sTiles[1, 2].CanCollide) {
            //            player.SetVelocityX(0);
            //            player.ApplyVelocity();
            //        }
            //    }
            //    else if (movingLeft) {
            //        if (sTiles[0, 1] != null && !sTiles[0, 1].CanCollide) {
            //            player.SetVelocityY(0);
            //            player.ApplyVelocity();
            //        }
            //    }
            //}
        }

        #endregion

        public override void Draw(GameTime gameTime) {
            m_batch.Begin();

            for (int y = 0; y < NumTilesY; y++) {
                for (int x = 0; x < NumTilesX; x++) {
                    m_tiles[x, y].Draw(m_batch, m_camera);
                }
            }

            foreach (NetworkGamer gamer in m_players.Keys) {
                Player player = m_players[gamer];
                if (player.Active) {
                    player.Draw(m_batch, m_camera);

                    // highlight surrounding tiles
                    if (HIGHLIGHT_SURROUNDING_TILES) {
                        Tile[,] sTiles = GetSurroundingTiles(player.Bounds);
                        foreach (Tile tile in sTiles) {
                            if (tile != null && tile != sTiles[1, 1])
                                m_content.Highlightsprite.Draw(m_batch, tile.Position, 0, m_camera);
                        }
                    }
                }
            }

            m_batch.End();

			base.Draw(gameTime);
		}

		#endregion

        #region Network Methods

        void networkSession_GamerJoined(object sender, GamerJoinedEventArgs e) {
            AddNetworkGamer(e.Gamer);
        }

        void networkSession_GamerLeft(object sender, GamerLeftEventArgs e) {
            if (m_players.ContainsKey(e.Gamer))
                m_players.Remove(e.Gamer);
        }

        void AddNetworkGamer(NetworkGamer gamer) {
            if (!m_players.ContainsKey(gamer)) {
                if (gamer.IsLocal) {
                    m_player = new Player(new PlayerSprite(m_content.PlayerSprite), (LocalNetworkGamer)gamer);
                    m_controller = GameController.player1;
                    gamer.Tag = m_player;
                }
                else
                    gamer.Tag = new Player(new PlayerSprite(m_content.PlayerSprite));

                m_players.Add(gamer, gamer.Tag as Player);
            }
        }

        public void SendPlayerState() {
            PlayerStateChange stateChange = new PlayerStateChange(true, true, true);

            // Convert/Compress data values for optimal usage of bandwidth
            HalfVector2 position = new HalfVector2(m_player.Position);
             
            // Write the data to each player in the sesion
            Global.packetWriter.Write(stateChange.DataToBeSent);
            Global.packetWriter.Write(m_player.State.DirectionFlags);
            Global.packetWriter.Write(position.PackedValue);
            Global.packetWriter.Write((byte)m_player.Facing);
            m_player.Net.SendData(Global.packetWriter, SendDataOptions.InOrder);

            // Display what was sent in the GUI
            NetInfo info = new NetInfo();
            info.direction = m_player.State.DirectionFlags;
            info.facing = m_player.Facing;
            info.position = m_player.Position;
            m_networkGUI.AddMessage("Sent: " + info);
        }

        void ReadPlayerStates() {
            while (m_player.Net.IsDataAvailable) {
                // Get the new state for a remote player
                NetworkGamer sender;
                m_player.Net.ReceiveData(Global.packetReader, out sender);
                if (sender.IsLocal)
                    continue;

                // Make sure that remote player's are activated for play after getting a packet from them
                Player remotePlayer = sender.Tag as Player;
                remotePlayer.Active = true;

                // Store the players current state, updating each value below if a change came over the network
                byte direction = remotePlayer.State.DirectionFlags;
                Vector2 position = remotePlayer.Position;
                FacingDirection facing = remotePlayer.State.Facing;

                // Update the remote player's state on this machine
                PlayerStateChange changes = new PlayerStateChange(Global.packetReader.ReadByte());
                if (changes.DirectionChanged)
                    direction = Global.packetReader.ReadByte();
                if (changes.UpdatePosition) {
                    HalfVector2 packedVector = new HalfVector2();
                    packedVector.PackedValue = Global.packetReader.ReadUInt32();
                    position = packedVector.ToVector2();
                }
                if (changes.UpdateFacing) {
                    facing = (FacingDirection)Global.packetReader.ReadByte();
                }

                remotePlayer.State.DirectionFlags = direction;
                remotePlayer.Position = position;
                remotePlayer.Facing = facing;
                remotePlayer.Active = true;

                NetInfo netInfo = new NetInfo();
                netInfo.direction = direction;
                netInfo.position = position;
                netInfo.facing = facing;

                m_networkGUI.AddMessage("Receiving: " + netInfo);
            }
        }

        #endregion

    }
}