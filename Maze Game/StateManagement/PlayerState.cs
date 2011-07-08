using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Maze_Game.StateManagement {

    public enum FacingDirection :byte { North, South, East, West }

    public class PlayerState {

        #region Static Attributes and Methods

        // Define direction flags specifying the direction a player is moving.
        // A player can move in 16 directions.
        public const byte DIRECTION_STOPPED = 0;
        public const byte DIRECTION_SOFT_UP = 1;
        public const byte DIRECTION_HARD_UP = 2;
        public const byte DIRECTION_SOFT_LEFT = 4;
        public const byte DIRECTION_HARD_LEFT = 8;
        public const byte DIRECTION_SOFT_DOWN = 16;
        public const byte DIRECTION_HARD_DOWN = 32;
        public const byte DIRECTION_SOFT_RIGHT = 64;
        public const byte DIRECTION_HARD_RIGHT = 128;

        public static byte DirectionToFlags(Vector2 direction) {
            byte flags = 0;
            double lowerThreshold = 25.0;
            double upperThreshold = 55.0;
            double ToDegrees = 180 / Math.PI;
            double angle = 0;

            direction.Normalize();

            // Determine the direction they are moving
            if (direction.Y > 0) {
                #region Bottom Half
                if (direction.X > 0) {
                    #region Bottom Right
                    angle = Math.Atan(direction.Y / direction.X) * ToDegrees;
                    if (angle >= lowerThreshold && angle <= upperThreshold) {
                        flags |= DIRECTION_HARD_DOWN;
                        flags |= DIRECTION_HARD_RIGHT;
                    }
                    else if (direction.Y > direction.X) {
                        flags |= DIRECTION_HARD_DOWN;
                        flags |= DIRECTION_SOFT_RIGHT;
                    }
                    else {
                        flags |= DIRECTION_SOFT_DOWN;
                        flags |= DIRECTION_HARD_RIGHT;
                    }
                    #endregion
                }
                else if (direction.X < 0) {
                    #region Bottom Left
                    angle = Math.Atan(direction.Y / Math.Abs(direction.X)) * ToDegrees;
                    if (angle >= lowerThreshold && angle <= upperThreshold) {
                        flags |= DIRECTION_HARD_DOWN;
                        flags |= DIRECTION_HARD_LEFT;
                    }
                    else if (direction.Y > Math.Abs(direction.X)) {
                        flags |= DIRECTION_HARD_DOWN;
                        flags |= DIRECTION_SOFT_LEFT;
                    }
                    else {
                        flags |= DIRECTION_SOFT_DOWN;
                        flags |= DIRECTION_HARD_LEFT;
                    }
                    #endregion
                }
                else
                    flags |= DIRECTION_HARD_DOWN;
                #endregion
            }
            else if (direction.Y < 0) {
                #region Top Half
                if (direction.X > 0) {
                    #region Top Right
                    angle = Math.Atan(Math.Abs(direction.Y) / direction.X) * ToDegrees;
                    if (angle >= lowerThreshold && angle <= upperThreshold) {
                        flags |= DIRECTION_HARD_UP;
                        flags |= DIRECTION_HARD_RIGHT;
                    }
                    else if (Math.Abs(direction.Y) > direction.X) {
                        flags |= DIRECTION_HARD_UP;
                        flags |= DIRECTION_SOFT_RIGHT;
                    }
                    else {
                        flags |= DIRECTION_SOFT_UP;
                        flags |= DIRECTION_HARD_RIGHT;
                    }
                    #endregion
                }
                else if (direction.X < 0) {
                    #region Top Left
                    angle = Math.Atan(Math.Abs(direction.Y) / Math.Abs(direction.X)) * ToDegrees;
                    if (angle >= lowerThreshold && angle <= upperThreshold) {
                        flags |= DIRECTION_HARD_UP;
                        flags |= DIRECTION_HARD_LEFT;
                    }
                    else if (Math.Abs(direction.Y) > Math.Abs(direction.X)) {
                        flags |= DIRECTION_HARD_UP;
                        flags |= DIRECTION_SOFT_LEFT;
                    }
                    else {
                        flags |= DIRECTION_SOFT_UP;
                        flags |= DIRECTION_HARD_LEFT;
                    }
                    #endregion
                }
                else
                    flags |= DIRECTION_HARD_UP;
                #endregion
            }
            else if (direction.X > 0)
                flags |= DIRECTION_HARD_RIGHT;
            else if (direction.X < 0)
                flags |= DIRECTION_HARD_LEFT;

            return flags;
        }

        public static FacingDirection GetFacingDirectionFromFlags(byte directionFlags) {
            FacingDirection facing;

            if ((directionFlags & DIRECTION_HARD_LEFT) > 0)
                facing = FacingDirection.East;
            else if ((directionFlags & DIRECTION_HARD_UP) > 0)
                facing = FacingDirection.North;
            else if ((directionFlags & DIRECTION_HARD_RIGHT) > 0)
                facing = FacingDirection.West;
            else
                facing = FacingDirection.South;

            return facing;
        }

        #endregion

        #region Non-static Class stuff

        private byte m_direction;
        private FacingDirection m_facing;
        private bool m_active;

        /// <summary>
        /// Creates the player's state.
        /// </summary>
        /// <param name="direction">The direction the player is facing.</param>
        /// <param name="facing">The direction the player is moving.</param>
        public PlayerState(Vector2 direction, FacingDirection facing) {
            m_direction = DirectionToFlags(direction);
            m_facing = facing;
            m_active = false;
        }

        /// <summary>
        /// Creates the player's state, calculating the direction the player is
        /// facing based on the direction he's moving.
        /// </summary>
        /// <param name="direction">The direction the player is moving.</param>
        public PlayerState(Vector2 direction) {
            m_direction = DirectionToFlags(direction);
            m_facing = GetFacingDirectionFromFlags(m_direction);
            m_active = false;
        }

        /// <summary>
        /// Creates the player's state, calculation the direction the player is
        /// facing based on the direction he's moving.
        /// </summary>
        /// <param name="directionFlags"></param>
        public PlayerState(byte directionFlags) {
            m_direction = directionFlags;
            m_facing = GetFacingDirectionFromFlags(m_direction);
            m_active = false;
        }

        /// <summary>
        /// Creates the player's state.
        /// </summary>
        /// <param name="directionFlags">The DIRECTION_X flags all OR'd together.</param>
        /// <param name="facing">The direction the player is facing</param>
        public PlayerState(byte directionFlags, FacingDirection facing) {
            m_direction = directionFlags;
            m_facing = facing;
            m_active = false;
        }

        /// <summary>
        /// Gets the current direction the player is heading
        /// </summary>
        public byte DirectionFlags {
            get { return m_direction; }
            set {
                m_direction = value;
                if (m_direction != DIRECTION_STOPPED)
                    m_facing = GetFacingDirectionFromFlags(m_direction);
            }
        }

        /// <summary>
        /// Gets the current direction the player is facing.
        /// </summary>
        public FacingDirection Facing {
            get { return m_facing; }
            set { m_facing = value; }
        }

        public bool Active {
            get { return m_active; }
            set { m_active = value; }
        }

        #endregion
    }
}
