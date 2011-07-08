using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Maze_Game.StageObjects;

namespace Maze_Game.Camera {

    /// <summary>
    /// Handles rendering all the objects a stage manages to the screen.  Implements
    /// features such as tracking the player, shaking the screen, and any other
    /// future enhancements.
    /// </summary>
    public class StageCamera {

        #region Attributes, Properties, and Constructos

        private StageEntity m_entityImFollowing;
        private int m_stageWidth, m_stageHeight;
        private int m_screenWidth, m_screenHeight;
        private int m_x_screenMiddle, m_y_screenMiddle;

        private float X_Offset {
            get {
                float xOffset = 0;
                float xPos = Func.Center(m_entityImFollowing.Bounds).X;

                // Ensure that if the player is near one of the edges of the screen
                // the camera wont move off the screen.  Anything pase the stage's edge
                // shouldn't be shown
                if (xPos > (m_screenWidth / 2)) {  // the camera is away from the left side of the screen
                    if (m_stageWidth - xPos > m_x_screenMiddle)   // the camera is away from the right side of the screen, so center on the entity
                        xOffset = m_x_screenMiddle - xPos;
                    else                                          // the camera is against the right side of teh screen, so stop moving the camera
                        xOffset = m_screenWidth - m_stageWidth;
                }
                
                return xOffset;
            }
        }

        private float Y_Offset {
            get {
                float yOffset = 0;
                float yPos = Func.Center(m_entityImFollowing.Bounds).Y;

                // Ensure that if the player is near one of the edges of the screen
                // the camera wont move off the screen.  Anything pase the stage's edge
                // shouldn't be shown
                if (yPos > (m_screenHeight / 2)) {  // the camera is away from the top of the screen
                    if (m_stageHeight - yPos > m_y_screenMiddle)   // the camera is away from the bottom of the screen, so center on the entity
                        yOffset = m_y_screenMiddle - yPos;
                    else
                        yOffset = m_screenHeight - m_stageHeight;
                }

                return yOffset;
            }
        }

        /// <summary>
        /// Constructs the camera object.
        /// </summary>
        /// <param name="stageWidth">The total width of the stage in pixels.</param>
        /// <param name="stageHeight">The total height of the stage in pixels.</param>
        public StageCamera(int stageWidth, int stageHeight, int screenWidth, int screenHeight) {
            m_stageWidth = stageWidth;
            m_stageHeight = stageHeight;
            m_screenWidth = screenWidth;
            m_screenHeight = screenHeight;

            m_x_screenMiddle = (int)(m_screenWidth / 2);
            m_y_screenMiddle = (int)(m_screenHeight / 2);
        }

        #endregion

        #region Important Methods

        public void FollowEntity(StageEntity entity) {
            m_entityImFollowing = entity;
        }

        public Vector2 ToCameraPosition(Vector2 position) {
            return new Vector2(position.X + X_Offset, position.Y + Y_Offset);
        }       

        #endregion
    }
}