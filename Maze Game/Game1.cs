using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using Maze_Game.StageObjects;
using Maze_Game.GUI;

namespace Maze_Game {
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game {
		GraphicsDeviceManager graphics;
        StageContentManager m_content;
        bool m_started = false;

		public Game1() {
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = Global.screenWidth;
			graphics.PreferredBackBufferHeight = Global.screenHeight;
            graphics.IsFullScreen = Global.fullscreen;
			Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize() {
            m_content = new StageContentManager(Content, "player", "tileset");
			base.Initialize();
		}

        protected override void LoadContent() {
            if (m_content != null)
                m_content.LoadContent();
            base.LoadContent();
        }

        /// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
            if (IsActive) {
                if (Gamer.SignedInGamers.Count == 0 && !Guide.IsVisible) {
                    Guide.ShowSignIn(1, false);
                }
                else {
                    // Allows the game to exit
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) {
                        Components.Remove(Components[2]);
                        Components.Remove(Components[1]);
                        Global.networkSession.Dispose();
                        Global.networkSession = null;
                        this.Exit();
                    }
                    else if (!m_started && GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed) {
                        StartGame();
                        m_started = true;
                    }
                }
            }

            base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			graphics.GraphicsDevice.Clear(Color.Black);

            if (!m_started && IsActive && m_content != null) {
                SpriteBatch batch = new SpriteBatch(graphics.GraphicsDevice);
                batch.Begin();
                batch.DrawString(m_content.Font, "Press A to Start", new Vector2(0, 0), Color.White);
                batch.End();
            }
            
			base.Draw(gameTime);
        }

        private void StartMultiplayer() {
            // Join an existing network session if one is found, otherwise start a new session.
            if (Global.networkSession == null) {
                // Join an existing session
                bool noSessionFound = true;
                using (AvailableNetworkSessionCollection availableSessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink, 1, null)) {
                    if (availableSessions.Count > 0) {
                        Global.networkSession = NetworkSession.Join(availableSessions[0]);
                        noSessionFound = false;
                    }
                }

                // Create a session if we didn't find an existing session
                if (noSessionFound) {
                    Global.networkSession = NetworkSession.Create(NetworkSessionType.SystemLink,
                                                       1, 2);
                }
            }
        }

        private void StartGame() {
            StartMultiplayer();

            NetworkDisplayGUI networkGUI = new NetworkDisplayGUI(this, m_content);
            Stage stage = new Stage(this, m_content, networkGUI);
            Components.Add(stage);
            Components.Add(networkGUI);
        }
	}
}
