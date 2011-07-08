using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Maze_Game.StateManagement {
    
    public struct PlayerStateChange {
        #region Static Attributes and Methods

        // Define header flags specifying what has changed
        public const byte HEADER_DIRECTION_CHANGED = 1;
        public const byte HEADER_UPDATE_POSITION = 2;
        public const byte HEADER_UPDATE_FACING = 4;

        public static byte CreateChangeHeader(bool directionChanged, bool updatePosition, bool updateFacing) {
            byte flags = 0;

            if (directionChanged) flags |= HEADER_DIRECTION_CHANGED;
            if (updatePosition)   flags |= HEADER_UPDATE_POSITION;
            if (updateFacing)     flags |= HEADER_UPDATE_FACING;

            return flags;
        }

        #endregion

        #region Class Attributes and Methods and Properties

        private byte m_header;

        public PlayerStateChange(bool directionChanged, bool updatePosition, bool updateFacing) {
            m_header = PlayerStateChange.CreateChangeHeader(directionChanged, updatePosition, updateFacing);
        }
        public PlayerStateChange(byte headerData) {
            m_header = headerData;
        }

        public byte DataToBeSent {
            get { return m_header; }
        }

        public bool DirectionChanged {
            get { return (m_header & HEADER_DIRECTION_CHANGED) > 0; }
        }
        public bool UpdatePosition {
            get { return (m_header & HEADER_UPDATE_POSITION) > 0; }
        }
        public bool UpdateFacing {
            get { return (m_header & HEADER_UPDATE_FACING) > 0; }
        }

        #endregion
    }
}
