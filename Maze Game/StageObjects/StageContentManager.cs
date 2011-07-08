using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Maze_Game.StageObjects {
	
	public class StageContentManager {

		#region Attributes and Constructors

		private ContentManager m_content;
        private SpriteFont m_font;
		private List<Sprite> m_sprites;
		private Texture2D m_tileSetTexture;
        private Texture2D m_playerTexture;
        private bool firstLoad;

        private readonly string m_playerAssetName;
        private readonly string m_tileSetAssetName;

		/// <summary>
		/// Creates a stage layout with the specified sprite set and background image.
		/// </summary>
		public StageContentManager(ContentManager content, string playerAssetName, string tileSetAssetName) {
			m_content = content;
            m_playerAssetName = playerAssetName;
            m_tileSetAssetName = tileSetAssetName;
			m_sprites = new List<Sprite>();

            firstLoad = true;
            LoadContent();
		}

		#endregion

		#region Properties

		public int TileWidth {
			get {
				return GroundSprite.GetWidth(0);
			}
		}
		public int TileHeight {
			get {
				return GroundSprite.GetHeight(0);
			}
		}

		#endregion

		#region Methods

		public void LoadContent() {
            m_font = m_content.Load<SpriteFont>("Font");
            m_tileSetTexture = m_content.Load<Texture2D>(m_tileSetAssetName);
            m_playerTexture = m_content.Load<Texture2D>(m_playerAssetName);

            if (firstLoad) {
                // Load the player sprite
                Color[] playerSpriteBuffer = new Color[m_playerTexture.Width * m_playerTexture.Height];
                m_playerTexture.GetData<Color>(playerSpriteBuffer);
                List<Rectangle> playerCells = GetSpriteCells(playerSpriteBuffer, m_playerTexture.Width, m_playerTexture.Height);
                m_sprites.Add(new Sprite(m_playerTexture, playerCells));

                // Load the tile set for the stage
                Color[] stageSpriteBuffer = new Color[m_tileSetTexture.Width * m_tileSetTexture.Height];
                m_tileSetTexture.GetData<Color>(stageSpriteBuffer);
                List<Rectangle> tileCells = GetSpriteCells(stageSpriteBuffer, m_tileSetTexture.Width, m_tileSetTexture.Height);

                List<Rectangle> floorCells = new List<Rectangle>();
                floorCells.Add(tileCells[1]);
                List<Rectangle> wallCells = new List<Rectangle>();
                wallCells.Add(tileCells[2]);
                List<Rectangle> highlightCells = new List<Rectangle>();
                highlightCells.Add(tileCells[3]);

                m_sprites.Add(new Sprite(m_tileSetTexture, floorCells));
                m_sprites.Add(new Sprite(m_tileSetTexture, wallCells));
                m_sprites.Add(new Sprite(m_tileSetTexture, highlightCells));

                firstLoad = false;
            }
		}

        /// <summary>
        /// Finds all the sprites by a Brute force implementation.  Looks at a
        /// pixel and if non-transparent builds a cell box around all pixels
        /// connected to the original.  Repeats, disregarding pixels if they
        /// are contained in a previous cell.
        /// </summary>
        public List<Rectangle> GetSpriteCells(Color[] spriteBuffer, int contentWidth, int contentHeight) {
            List<Rectangle> cells = new List<Rectangle>();
            Point pixelWalker = new Point(0, 0);
            
            // Find a pixel
            while (pixelWalker.X < contentWidth && pixelWalker.Y < contentHeight) {
                Color pixel = spriteBuffer[(contentWidth * (int)pixelWalker.Y) + (int)pixelWalker.X];
                bool pixelAlreadyRead = false;

                // Make sure this pixel wasn't alreay read by another cell
                foreach (Rectangle cell in cells) {
                    if (cell.Contains(pixelWalker.X, pixelWalker.Y)) {
                        pixelAlreadyRead = true;
                        break;
                    }
                }

                // Skip the pixel is it's already been read
                if (!pixelAlreadyRead) {

                    // Is it a real pixel and not a transparent pixel?
                    if (pixel.A != 0) {
                        if (cells.Count == 36)
                            cells.Add(CalcSpriteCellFast(pixelWalker.X, pixelWalker.Y, spriteBuffer, contentWidth, contentHeight));
                        else
                            cells.Add(CalcSpriteCellFast(pixelWalker.X, pixelWalker.Y, spriteBuffer, contentWidth, contentHeight));
                    }
                }

                // Go to the next pixel
                pixelWalker.X++;
                if (pixelWalker.X >= contentWidth) {
                    pixelWalker.X = 0;
                    pixelWalker.Y++;
                }
            }

            return cells;
        }

        /// <summary>
        /// Determines the cell dimensions for a sprite by starting on a single pixel inside
        /// the sprite and finding all the pixels connected.  Start pixel must be at the top
        /// of the sprite (i.e. smallest Y value among all pixels).  This allows the algorithm
        /// to skip looking above the pixel and thus run faster.
        /// </summary>
        public Rectangle CalcSpriteCellFast(int startX, int startY, Color[] buffer, int bufferWidth, int bufferHeight) {
            int cell_x1 = startX, cell_x2 = startX, cell_y1 = startY, cell_y2 = startY;
            bool lookLeft = cell_x1 > 0;
            bool lookRight = cell_x2 < bufferWidth - 1;  // ensure that when we test one pixel next to the starting
            bool lookDown = cell_y2 < bufferHeight - 1;  // pixel won't put us out of the buffer's bounds
            bool looking = lookLeft || lookRight || lookDown;
            int x, y;
            bool foundPixel;

            while (looking) {
                if (lookLeft) {
                    // Look down the left side of the current cell bounds for a new pixel outside of the cell
                    x = cell_x1 - 1;
                    y = cell_y1;
                    foundPixel = false;
                    while (y <= cell_y2 && !foundPixel) {
                        if (pixel(buffer, x, y, bufferWidth).A != 0) {
                            // Go out as far left as possible
                            do { x--; } while (pixel(buffer, x, y, bufferWidth).A != 0);

                            // Expand the cell
                            cell_x1 = x + 1;
                            foundPixel = true;
                        }
                        else
                            y++;  // move down the left side of the box until we hit the bottom
                    }
                    lookLeft = foundPixel && cell_x1 > 0;
                }

                if (lookRight) {
                    // Look down the right side of the current cell bounds for a new pixel outside of the cell
                    x = cell_x2 + 1;
                    y = cell_y1;
                    foundPixel = false;
                    while (y <= cell_y2 && !foundPixel) {
                        if (pixel(buffer, x, y, bufferWidth).A != 0) {
                            // Go out as far right as possible
                            do { x++; } while (pixel(buffer, x, y, bufferWidth).A != 0);

                            // Expand the cell
                            cell_x2 = x - 1;
                            foundPixel = true;
                        }
                        else
                            y++;  // move down the right side of the box until we hit the bottom
                    }
                    lookRight = foundPixel && cell_x2 < bufferWidth - 1;
                }

                lookDown = cell_y2 < bufferHeight - 1;  // always look down each run.  The loop will stop if lookDown = false after this if statement along with lookLeft and lookRight
                if (lookDown) {
                    // Test the left corner.  If something is there then we should check the left side again
                    if (cell_x1 - 1 > 0) {
                        x = cell_x1 - 1;
                        y = cell_y2 + 1;
                        if (pixel(buffer, x, y, bufferWidth).A != 0) {
                            cell_x1 = x;
                            lookLeft = (cell_x1 - 1) > 0;  // check the left side if the left side of the cell isn't out of bounds
                        }
                    }

                    // Test the right corner.  If something is there then we should check the right side again
                    if (cell_x2 + 1 > 0) {
                        x = cell_x2 + 1;
                        y = cell_y2 + 1;
                        if (pixel(buffer, x, y, bufferWidth).A != 0) {
                            cell_x2 = x;
                            lookRight = (cell_x2 + 1) < bufferWidth - 1;  // check the left side if the left side of the cell isn't out of bounds
                        }
                    }

                    // Look along the bottom side of the current cell bounds for a new pixel outside of the cell
                    x = cell_x1;
                    y = cell_y2 + 1;
                    foundPixel = false;
                    while (x <= cell_x2 && !foundPixel) {
                        if (pixel(buffer, x, y, bufferWidth).A != 0) {
                            // Look on the sides again
                            lookLeft = cell_x1 - 1 > 0;
                            lookRight = cell_x2 < bufferWidth - 1;

                            // Go as far down as possible
                            do { y++; } while (pixel(buffer, x, y, bufferWidth).A != 0);

                            // Expand the cell
                            cell_y2 = y - 1;
                            foundPixel = true;
                        }
                        else
                            x++;  // move down the left side of the box until we hit the bottom
                    }
                    lookDown = foundPixel && cell_y2 < bufferHeight - 1;
                }

                looking = lookLeft || lookRight || lookDown;
            }

            return new Rectangle(cell_x1, cell_y1,
                                 (cell_x2 - cell_x1) + 1,
                                 (cell_y2 - cell_y1) + 1);
        }

        public static Color pixel(Color[] buffer, int x, int y, int bufferWidth) {
            return buffer[(bufferWidth * y) + x];
        }

        /// <summary>
        /// Determines the cell dimensions for a sprite by starting on a single pixel inside
        /// the sprite and finding all the pixels connected, using the edge pixels to get
        /// the dimensions of the sprite and thus the cell.
        /// </summary>
        public Rectangle CalcSpriteCell(int startX, int startY, Color[] buffer, int bufferWidth, int bufferHeight) {
            Rectangle cell = new Rectangle(startX, startY, 1, 1);
            int right = startX, bottom = startY;
            List<Point> pixelsToTest = new List<Point>();
            pixelsToTest.Add(new Point(startX, startY));
            int testpixel = 0;

            // Expand the cell if this pixel is outside of the cell's dimensions
            while (testpixel < pixelsToTest.Count) {
                int pixelX = pixelsToTest[testpixel].X;
                int pixelY = pixelsToTest[testpixel].Y;
                testpixel++;

                if (pixelX < cell.X)
                    cell.X = pixelX;
                if (pixelX > right)
                    right = pixelX;
                if (pixelY < cell.Y)
                    cell.Y = pixelY;
                if (pixelY > bottom)
                    bottom = pixelY;

                // Get the surrounding pixels positions (intentionally omitting pixels above since they would have been discovered earlier)
                List<Point> surroundingPixels = new List<Point>();
                surroundingPixels.Add(new Point(pixelX + 1, pixelY));        // right
                surroundingPixels.Add(new Point(pixelX + 1, pixelY + 1));    // down right
                surroundingPixels.Add(new Point(pixelX, pixelY + 1));        // down
                surroundingPixels.Add(new Point(pixelX - 1, pixelY + 1));    // down left
                surroundingPixels.Add(new Point(pixelX - 1, pixelY));        // left

                // If any pixels are connected to this pixel (directly and diagonally)
                // then run this algorithm on them too
                foreach (Point pixel in surroundingPixels) {
                    if (!pixelsToTest.Contains(pixel) &&                    // Ensure the pixel hasn't already been tested or is in the queue to be tested
                        buffer[(bufferWidth * pixel.Y) + pixel.X].A != 0)   // Ensure the pixel isn't a transparent pixel
                    {
                        pixelsToTest.Add(pixel);
                    }
                }
            }

            cell.Width = (right - cell.X) + 1;
            cell.Height = (bottom - cell.Y) + 1;
            return cell;
        }

        public List<Rectangle> GetPartitionedSpriteCells(Color[] spriteBuffer, int bufferWidth, int bufferHeight) {
            List<Rectangle> cells = new List<Rectangle>();
            Color cellBorder = Color.White;
            int gridHeight, gridWidth, cellWidth, cellHeight;
            int x, y;

            // We're assuming that the sprite is partitioned in a grid containg cells of equal width and height.
            // So first we're going to find the bottom of the grid, then the right side, and then we'll
            // figure out the width and height of the first cell.  After that we can calculate the rest
            // of the cells.
            
            // First find the bottom
            for (gridHeight = bufferHeight - 1; gridHeight >= 0; gridHeight--) {
                if (spriteBuffer[(gridHeight * bufferWidth) + 0].Equals(cellBorder)) {
                    gridHeight++;
                    break;
                }
            }
            
            // Then find the right side of the grid
            for (gridWidth = bufferWidth - 1; gridWidth >= 0; gridWidth--) {
                if (spriteBuffer[(gridHeight * bufferWidth) + gridWidth].Equals(cellBorder)) {
                    gridWidth++;
                    break;
                }
            }

            cellWidth = gridWidth;
            cellHeight = gridHeight;

            // Now find the width of the first cell
            for (x = 1; x < gridWidth; x++) {
                if (spriteBuffer[(1 * bufferWidth) + x].Equals(cellBorder)) {
                    cellWidth = x + 1;
                    break;
                }
            }

            // Then find the height of the first cell
            for (y = 1; y < gridHeight; y++) {
                if (spriteBuffer[(y * bufferWidth) + 1].Equals(cellBorder)) {
                    cellHeight = y + 1;
                    break;
                }
            }

            // Finally get all the rectangles defining the innards of each cell in the grid
            x = y = 0;
            while (y < gridHeight - 1) {       // when the x or y land on the last
                while ( x < gridWidth - 1) {   // grid line we're done
                    Rectangle cell = new Rectangle(x + 1, y + 1, cellWidth - 2, cellHeight - 2);  // addittion and subtraction to negate the actual grid lines
                    cells.Add(cell);
                    x += cellWidth - 1;
                }
                x = 0;
                y += cellHeight - 1;
            }

            return cells;
        }

        #endregion

        #region Sprite Getters

        public SpriteFont Font {
            get {
                return m_font;
            }
        }

        public Sprite PlayerSprite {
			get {
				return m_sprites[0];
			}
		}

		public Sprite GroundSprite {
			get {
				return m_sprites[1];
			}
		}

		public Sprite WallSprite {
			get {
				return m_sprites[2];
			}
		}

        public Sprite Highlightsprite {
            get {
                return m_sprites[3];
            }
        }
		#endregion
	}
}
