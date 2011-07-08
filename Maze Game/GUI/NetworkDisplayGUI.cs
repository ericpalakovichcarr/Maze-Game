using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Maze_Game.StageObjects;

namespace Maze_Game.GUI {

    public class NetworkDisplayGUI : DrawableGameComponent, DisplayGUI
    {
        private SpriteBatch m_batch;
        private StageContentManager m_content;
        private List<string> messages;

        public NetworkDisplayGUI(Game game, StageContentManager content)
            : base(game)
        {
            m_batch = new SpriteBatch(game.GraphicsDevice);
            m_content = content;
            messages = new List<string>();
        }

        public void AddMessage(string message) {
            messages.Add(message);
        }

        public override void Initialize() {
            base.Initialize();
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime) {
            Color drawColor = Color.White;
            int y = 0;

            m_batch.Begin();

            // Write XBL info to the screen
            string genInfo = string.Format("SessionState({0})  NumPlayer({1})", Global.networkSession.SessionState, Global.networkSession.AllGamers.Count);
            m_batch.DrawString(m_content.Font, genInfo, new Vector2(0, y), drawColor);
            y += m_content.Font.LineSpacing;
            foreach (NetworkGamer gamer in Global.networkSession.AllGamers) {
                m_batch.DrawString(m_content.Font, gamer.Gamertag, new Vector2(0, y), drawColor);
                y += m_content.Font.LineSpacing;
            }

            // Write any custom messages to the screen            
            foreach (string message in messages) {
                m_batch.DrawString(m_content.Font, message, new Vector2(0, y), drawColor);
                y += m_content.Font.LineSpacing;
            }
            messages.Clear();

            m_batch.End();

            base.Draw(gameTime);
        }
    }
}