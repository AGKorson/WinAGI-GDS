using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine {
    /// <summary>
    /// Provides access to the picture functions needed to build bitmaps
    /// from picture resource data.
    /// </summary>
    public static class PictureFunctions {
        #region Structs
        /// <summary>
        /// 
        /// </summary>
        private enum CornerDirection {
            cdX,
            cdY,
        }
        #endregion

        #region Members
        /// <summary>
        /// Holds the current state of the drawing pen (colors, plot
        /// size and shape) at the drawing end point so external programs
        /// can access it.
        /// </summary>
        internal static PenStatus SavePen;
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods
        /// <summary>
        /// Converts the data extracted from the picture resource into 
        /// 256 bit color DIBitmaps for the visual and priority screens.
        /// </summary>
        /// <param name="VisData"></param>
        /// <param name="PriData"></param>
        /// <param name="picdata"></param>
        /// <param name="endpos"></param>
        /// <param name="statuspos"></param>
        /// <returns>0 if successful, no errors/warnings<br />
        /// non-zero bitfield for error/warning:<br />
        ///  1 = no EOP marker<br />
        ///  2 = bad vis color data<br />
        ///  4 = invalid command byte<br />
        ///  8 = unused data at end of resource<br />
        ///  16 = other error</returns>
        internal static int CompilePicData(ref byte[] VisData, ref byte[] PriData, byte[] picdata, int endpos, int statuspos) {
            // picture resource variables
            byte[] VisBuildData, PriBuildData;
            int lngPos;
            byte bytIn;
            PenStatus CurrentPen;
            Queue<int> paintqueue = new();
            byte[] CircleData = [
                0x80, 0xC0, 0xC0, 0xC0, 0x40, 0xE0, 0xE0, 0xE0,
                0x40, 0x60, 0x60, 0xF0, 0xF0, 0xF0, 0x60, 0x60,
                0x20, 0x70, 0xF8, 0xF8, 0xF8, 0xF8, 0xF8, 0x70,
                0x20, 0x30, 0x78, 0x78, 0x78, 0xFC, 0xFC, 0xFC,
                0x78, 0x78, 0x78, 0x30, 0x38, 0x7C, 0x7C, 0x7C,
                0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0x7C, 0x7C, 0x7C,
                0x38, 0x18, 0x3C, 0x7E, 0x7E, 0x7E, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0x7E, 0x7E, 0x7E, 0x3C, 0x18
            ];

            // assume ok
            int retval = 0;
            if (endpos == -1) {
                // use all data
                endpos = picdata.Length - 1;
            }
            if (statuspos == -1) {
                // use end pos
                statuspos = endpos;
            }
            // set starting/default pen status
            CurrentPen.VisColor = AGIColorIndex.None;
            CurrentPen.PriColor = AGIColorIndex.None;
            CurrentPen.PlotSize = 0;
            CurrentPen.PlotShape = PlotShape.Circle;
            CurrentPen.PlotStyle = PlotStyle.Solid;
            // initialize the working byte arrays
            VisBuildData = new byte[26880];
            PriBuildData = new byte[26880];
            for (int i = 0; i < 26880; i++) {
                VisBuildData[i] = 15; // white
                PriBuildData[i] = 4;  // red
            }
            lngPos = 0;
            bytIn = picdata[lngPos++];
            try {
                do {
                    switch (bytIn) {
                    case 0xF6:
                        // Absolute line (long lines)
                        DrawAbsLine();
                        break;
                    case 0xF7:
                        // Relative line (short lines)
                        DrawRelLine();
                        break;
                    case 0xF8:
                        // Fill
                        PicFloodFill();
                        break;
                    case 0xF4:
                        // Draw a Y corner
                        DrawCorner(CornerDirection.cdY);
                        break;
                    case 0xF5:
                        // Draw an X corner
                        DrawCorner(CornerDirection.cdX);
                        break;
                    case 0xF9:
                        // Change pen size and style
                        bytIn = picdata[lngPos++];
                        CurrentPen.PlotStyle = (PlotStyle)((bytIn & 0x20) / 0x20);
                        CurrentPen.PlotShape = (PlotShape)((bytIn & 0x10) / 0x10);
                        CurrentPen.PlotSize = bytIn & 0x7;
                        bytIn = picdata[lngPos++];
                        break;
                    case 0xFA:
                        // Plot with pen
                        BrushPlot();
                        break;
                    case 0xF0:
                        // Change picture color and enable picture draw
                        bytIn = picdata[lngPos++];
                        CurrentPen.VisColor = (AGIColorIndex)(bytIn & 0xF);
                        // AGI has a slight bug - if color is > 15, the
                        // upper nibble will overwrite the priority color
                        if (bytIn > 15) {
                            // pass upper nibble to priority
                            CurrentPen.PriColor |= (AGIColorIndex)(bytIn / 16);
                            retval |= 2;
                        }
                        bytIn = picdata[lngPos++];
                        break;
                    case 0xF1:
                        // Disable visual draw
                        CurrentPen.VisColor = AGIColorIndex.None;
                        bytIn = picdata[lngPos++];
                        break;
                    case 0xF2:
                        // Change priority color and enable priority draw
                        bytIn = picdata[lngPos++];
                        // AGI uses ONLY priority color; if the passed value is
                        // greater than 15, the upper nibble gets ignored
                        CurrentPen.PriColor = ((AGIColorIndex)(bytIn & 0xF));
                        bytIn = picdata[lngPos++];
                        break;
                    case 0xF3:
                        // Disable priority draw
                        CurrentPen.PriColor = AGIColorIndex.None;
                        bytIn = picdata[lngPos];
                        lngPos++;
                        break;
                    case 0xFF:
                        // end of drawing
                        if (statuspos == endpos) {
                            // save pen status
                            SavePen = CurrentPen;
                        }
                        break;
                    default:
                        // if expecting a command, and byte is <240 but >250 (not 255)
                        // just ignore it
                        bytIn = picdata[lngPos++];
                        retval |= 4;
                        break;
                    }
                    if (lngPos > statuspos) {
                        SavePen = CurrentPen;
                        // reset statuspos to avoid changing save status
                        statuspos = 0x7FFFFFFF;
                    }
                    if (bytIn == 0xFF) {
                        // end byte found
                        break;
                    }
                }
                while (lngPos <= endpos);
            }
            catch (Exception e) {
                // depending on error, set warning level
                if (e is IndexOutOfRangeException) {
                    // subscript error- caused when a draw function expects
                    // another byte of data, but end of data is reached
                    // confirm it
                    if (lngPos > endpos) {
                        // set warning flag
                        retval |= 1;
                    }
                    else {
                        // something else
                        retval |= 16;
                    }
                }
                else {
                    // any other error- just pass it along
                    retval |= 16;
                }
            }
            // if building entire picture
            if (endpos == picdata.Length - 1) {
                // confirm last command is end-of-picture marker
                if (bytIn != 0xFF) {
                    // set warning flag
                    retval |= 1;
                }
                // check for unused data after end marker
                if (lngPos != picdata.Length) {
                    // set warning flag
                    retval |= 8;
                }
            }
            // copy resulting data back to calling function
            VisData = VisBuildData;
            PriData = PriBuildData;
            return retval;

            /// <summary>
            /// Adds the color to visual and priority picture based on current pen status.
            /// </summary>
            /// <param name="xPos"></param>
            /// <param name="yPos"></param>
            void DrawPixel(int xPos, int yPos) {
                int lngIndex = xPos + yPos * 160;
                if (lngIndex <= 26879) {
                    if (CurrentPen.VisColor < AGIColorIndex.None) {
                        VisBuildData[lngIndex] = (byte)CurrentPen.VisColor;
                    }
                    if (CurrentPen.PriColor < AGIColorIndex.None) {
                        PriBuildData[lngIndex] = (byte)CurrentPen.PriColor;
                    }
                }
            }

            /// <summary>
            /// Draws a line on visual and priority pictures based on current pen status.
            /// </summary>
            /// <param name="X1"></param>
            /// <param name="Y1"></param>
            /// <param name="X2"></param>
            /// <param name="Y2"></param>
            void DrawLine(int X1, int Y1, int X2, int Y2) {
                // this method mirrors the AGI MSDOS draw function so 
                // lines are guaranteed to be an exact match.
                int xPos, yPos;
                int DY, DX;
                int vDir, hDir;
                int XC, YC, MaxDelta;
                int i;
                // Sierra sucked at checking for overflows; if a bad value for
                // coordinates was used that overflowed the designated picture buffer,
                // AGI didn't care; it would just draw the pixel in invalid memory
                // and plod on. To mimic that we just ignore invalid pixels and
                // plod on too. ugh...

                // determine deltaX/deltaY and direction
                DY = Y2 - Y1;
                vDir = Math.Sign(DY);
                DX = X2 - X1;
                hDir = Math.Sign(DX);
                if (DY == 0 && DX == 0) {
                    DrawPixel(X1, Y1);
                }
                else if (DY == 0) {
                    for (i = X1; i != X2; i += hDir) {
                        DrawPixel(i, Y1);
                    }
                    DrawPixel(X2, Y1);
                }
                else if (DX == 0) {
                    for (i = Y1; i != Y2; i += vDir) {
                        DrawPixel(X1, i);
                    }
                    DrawPixel(X1, Y2);
                }
                else {
                    DrawPixel(X1, Y1);
                    xPos = X1;
                    yPos = Y1;
                    // invert DX and DY if they are negative
                    if (DY < 0) {
                        DY *= -1;
                    }
                    if ((DX < 0)) {
                        DX *= -1;
                    }
                    // set up the loop, depending on which direction is largest
                    if (DX >= DY) {
                        MaxDelta = DX;
                        YC = DX / 2;
                        XC = 0;
                    }
                    else {
                        MaxDelta = DY;
                        XC = DY / 2;
                        YC = 0;
                    }
                    for (i = 1; i <= MaxDelta; i++) {
                        YC += DY;
                        if (YC >= MaxDelta) {
                            YC -= MaxDelta;
                            yPos += vDir;
                        }
                        XC += DX;
                        if (XC >= MaxDelta) {
                            XC -= MaxDelta;
                            xPos += hDir;
                        }
                        DrawPixel(xPos, yPos);
                    }
                }
            }

            bool GetNextLocation(ref byte x, ref byte y) {
                x = picdata[lngPos++];
                if (x > 239) {
                    bytIn = x;
                    return false;
                }
                if (lngPos > endpos) {
                    //return false;
                }
                if (x > 159) {
                    x = 159;
                }
                y = picdata[lngPos++];
                if (y > 239) {
                    bytIn = y;
                    return false;
                }
                if (y > 167) {
                    y = 167;
                }
                if (lngPos > endpos) {
                    //return false;
                }
                return true;
            }

            /// <summary>
            /// Draws a series of absolute lines.
            /// </summary>
            void DrawAbsLine() {
                byte X1 = 0, Y1 = 0, X2 = 0, Y2 = 0;
                if (!GetNextLocation(ref X1, ref Y1)) return;
                DrawPixel(X1, Y1);
                if (lngPos > endpos) return;
                do {
                    if (!GetNextLocation(ref X2, ref Y2)) return;
                    DrawLine(X1, Y1, X2, Y2);
                    X1 = X2;
                    Y1 = Y2;
                } while (lngPos <= endpos);
            }

            /// <summary>
            /// Draws a series of relative lines.
            /// </summary>
            void DrawRelLine() {
                short xdisp, ydisp;
                byte X1 = 0, Y1 = 0;
                if (!GetNextLocation(ref X1, ref Y1)) return;
                DrawPixel(X1, Y1);
                if (lngPos > endpos) return;
                do {
                    bytIn = picdata[lngPos++];
                    if (bytIn >= 0xF0) return;
                    // check x sign bit
                    if ((bytIn & 0x80) == 0x80) {
                        // displacement is negative
                        xdisp = (short)(0 - ((bytIn & 0x70) / 0x10));
                    }
                    else {
                        xdisp = ((short)((bytIn & 0x70) / 0x10));
                    }
                    // check y sign bit
                    if ((bytIn & 0x8) == 0x8) {
                        // displacement is negative
                        ydisp = (short)(0 - (bytIn & 0x7));
                    }
                    else {
                        ydisp = ((short)(bytIn & 0x7));
                    }
                    DrawLine(X1, Y1, X1 + xdisp, Y1 + ydisp);
                    X1 += (byte)xdisp;
                    Y1 += (byte)ydisp;
                } while (lngPos <= endpos);
            }

            /// <summary>
            /// Performs a series of flood-fill actions.
            /// </summary>
            void PicFloodFill() {
                int lngOffset;
                byte X = 0, Y = 0;

                do {
                    if (!GetNextLocation(ref X, ref Y)) return;
                    // if visual OR priority but not both
                    if ((CurrentPen.VisColor < AGIColorIndex.None) ^ (CurrentPen.PriColor < AGIColorIndex.None)) {
                        if (CurrentPen.VisColor < AGIColorIndex.None) {
                            // fill visual screen
                            if (CurrentPen.VisColor != AGIColorIndex.White && (AGIColorIndex)VisBuildData[X + 160 * Y] == AGIColorIndex.White) {
                                lngOffset = Y * 160 + X;
                                paintqueue.Enqueue(lngOffset);
                                VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                do {
                                    lngOffset = paintqueue.Dequeue();
                                    X = (byte)(lngOffset % 160);
                                    Y = (byte)(lngOffset / 160);
                                    // check above
                                    if (Y > 0) {
                                        lngOffset = (Y - 1) * 160 + X;
                                        if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                            VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                    // check left
                                    if (X > 0) {
                                        lngOffset = Y * 160 + X - 1;
                                        if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                            VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                    // check right
                                    if (X < 159) {
                                        lngOffset = Y * 160 + X + 1;
                                        if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                            VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                    // check below
                                    if (Y < 167) {
                                        lngOffset = (Y + 1) * 160 + X;
                                        if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                            VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                }
                                while (paintqueue.Count > 0);
                            }
                        }
                        else {
                            // fill priority screen
                            if (CurrentPen.PriColor != AGIColorIndex.Red && (AGIColorIndex)PriBuildData[X + 160 * Y] == AGIColorIndex.Red) {
                                lngOffset = Y * 160 + X;
                                paintqueue.Enqueue(lngOffset);
                                PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                do {
                                    lngOffset = paintqueue.Dequeue();
                                    X = (byte)(lngOffset % 160);
                                    Y = (byte)(lngOffset / 160);
                                    // check above
                                    if (Y > 0) {
                                        lngOffset = (Y - 1) * 160 + X;
                                        if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.Red) {
                                            PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                    // check left
                                    if (X > 0) {
                                        lngOffset = Y * 160 + X - 1;
                                        if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.Red) {
                                            PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                    // check right
                                    if (X < 159) {
                                        lngOffset = Y * 160 + X + 1;
                                        if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.Red) {
                                            PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                    // check below
                                    if (Y < 167) {
                                        lngOffset = (Y + 1) * 160 + X;
                                        if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.Red) {
                                            PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(lngOffset);
                                        }
                                    }
                                }
                                while (paintqueue.Count > 0);
                            }
                        }
                    }
                    else if ((CurrentPen.VisColor < AGIColorIndex.None) && (CurrentPen.VisColor < AGIColorIndex.None)) {
                        // drawing both
                        if (CurrentPen.VisColor != AGIColorIndex.White && (AGIColorIndex)VisBuildData[X + 160 * Y] == AGIColorIndex.White) {
                            lngOffset = Y * 160 + X;
                            paintqueue.Enqueue(lngOffset);
                            VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                            PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;

                            do {
                                lngOffset = paintqueue.Dequeue();
                                X = (byte)(lngOffset % 160);
                                Y = (byte)(lngOffset / 160);
                                // check above
                                if (Y > 0) {
                                    lngOffset = (Y - 1) * 160 + X;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(lngOffset);
                                    }
                                }
                                // check left
                                if (X > 0) {
                                    lngOffset = Y * 160 + X - 1;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(lngOffset);
                                    }
                                }
                                // check right
                                if (X < 159) {
                                    lngOffset = Y * 160 + X + 1;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(lngOffset);
                                    }
                                }
                                // check below
                                if (Y < 167) {
                                    lngOffset = (Y + 1) * 160 + X;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.White) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(lngOffset);
                                    }
                                }
                            }
                            while (paintqueue.Count > 0);
                        }
                    }
                } while (lngPos <= endpos) ;
            }

            /// <summary>
            /// Draws a series of alternating horizontal and vertical lines.
            /// </summary>
            /// <param name="CurAxis"></param>
            void DrawCorner(CornerDirection CurAxis) {
                byte X1 = 0, Y1 = 0, X2, Y2;

                if (!GetNextLocation(ref X1, ref Y1)) return;
                DrawPixel(X1, Y1);
                if (lngPos > endpos) return;

                do {
                    if (CurAxis == CornerDirection.cdX) {
                        X2 = picdata[lngPos++];
                        if (X2 > 239) {
                            bytIn = X2;
                            return;
                        }
                        if (X2 > 159) {
                            X2 = 159;
                        }
                        Y2 = Y1;
                        DrawLine(X1, Y1, X2, Y2);
                        CurAxis = CornerDirection.cdY;
                        X1 = X2;
                    }
                    else {
                        Y2 = picdata[lngPos++];
                        if (Y2 > 239) {
                            bytIn = Y2;
                            return;
                        }
                        if (Y2 > 167) {
                            Y2 = 167;
                        }
                        X2 = X1;
                        DrawLine(X1, Y1, X2, Y2);
                        CurAxis = CornerDirection.cdX;
                        Y1 = Y2;
                    }
                } while (lngPos <= endpos);
            }

            /// <summary>
            /// Draws a series of plot patterns using current pen settings. 
            /// </summary>
            void BrushPlot() {
                int PlotX = 0, PlotY = 0;
                byte pX = 0, pY = 0, PatternNum = 0;
                int X, Y;

                do {
                    if (CurrentPen.PlotStyle == PlotStyle.Splatter) {
                        bytIn = picdata[lngPos++];
                        if (bytIn >= 0xF0) return;
                        PatternNum = (byte)(bytIn | 1);
                    }
                    if (!GetNextLocation(ref pX, ref pY)) return;
                    // adjust coordinates to upper/left values
                    // needed based on pen size
                    PlotX = pX - (CurrentPen.PlotSize + 1) / 2;
                    if (PlotX < 0) {
                        PlotX = 0;
                    }
                    else if (PlotX > 160 - CurrentPen.PlotSize) {
                        // there is a bug in AGI that uses 160 instead of 159
                        // well, actually, it doubles the X value for the check
                        // and uses a value of 320, but it's the same effect)
                        //
                        // we need to mimic that bug so pictures look
                        // exactly the same
                        PlotX = 160 - CurrentPen.PlotSize;
                    }
                    PlotY = pY - CurrentPen.PlotSize;
                    if (PlotY < 0) {
                        PlotY = 0;
                    }
                    else if (PlotY > 167 - CurrentPen.PlotSize) {
                        PlotY = 167 - CurrentPen.PlotSize;
                    }
                    if (CurrentPen.PlotShape == PlotShape.Circle) {
                        for (Y = 0; Y <= CurrentPen.PlotSize * 2; Y++) {
                            for (X = 0; X <= CurrentPen.PlotSize; X++) {
                                if ((CircleData[(CurrentPen.PlotSize * CurrentPen.PlotSize) + Y] & (1 << (7 - X))) == (1 << (7 - X))) {
                                    if (CurrentPen.PlotStyle == PlotStyle.Splatter) {
                                        // adjust pattern bit using Sierra's algorithm
                                        if ((PatternNum & 1) == 1) {
                                            PatternNum = (byte)((PatternNum / 2) ^ 0xB8);
                                        }
                                        else {
                                            PatternNum /= 2;
                                        }
                                        // only draw if pattern bit is set
                                        if ((PatternNum & 3) == 2) {
                                            DrawPixel(X + PlotX, Y + PlotY);
                                        }
                                    }
                                    else {
                                        // solid - set all pixels
                                        DrawPixel(X + PlotX, Y + PlotY);
                                    }
                                }
                            }
                        }
                    }
                    else {
                        // square
                        for (Y = 0; Y <= CurrentPen.PlotSize * 2; Y++) {
                            for (X = 0; X <= CurrentPen.PlotSize; X++) {
                                if (CurrentPen.PlotStyle == PlotStyle.Splatter) {
                                    if ((PatternNum & 1) == 1) {
                                        PatternNum = (byte)((PatternNum / 2) ^ 0xB8);
                                    }
                                    else {
                                        PatternNum /= 2;
                                    }
                                    if ((PatternNum & 3) == 2) {
                                        DrawPixel(X + PlotX, Y + PlotY);
                                    }
                                }
                                else {
                                    // solid - set all pixels
                                    DrawPixel(X + PlotX, Y + PlotY);
                                }
                            }
                        }
                    }
                } while (lngPos <= endpos);
            }
        }
        #endregion
    }
}
