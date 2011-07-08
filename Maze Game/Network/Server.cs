using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Maze_Game.Network {
    public class Server {
        private static readonly Server I = new Server();
        private NetworkSession m_networkSession;
        private PacketWriter m_packetWriter;
        private PacketReader m_packetReader;

        private List<PlayerInput> m_inputStack;

        protected Server() {
            m_inputStack = new List<PlayerInput>();
        }

        public void Update() {

        }
    }
}