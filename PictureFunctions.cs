using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        /// Version is passed as an integer index of the AGI version
        /// (0-18 for v2.089 through v3.002.149) to allow for differences
        /// in brush sizes/shapes.<br/>
        /// Returns a bitfield of warnings that were encountered during
        /// the conversion process.
        /// </summary>
        /// <param name="visData"></param>
        /// <param name="priData"></param>
        /// <param name="picdata"></param>
        /// <param name="endpos"></param>
        /// <param name="statuspos"></param>
        /// <returns> warning status:<br />
        /// = 0 if successful, no warnings<br />
        /// non-zero bitfield for error/warning:<br />
        ///  1 = no EOP marker<br />
        ///  2 = bad vis color data<br />
        ///  4 = invalid command byte<br />
        ///  8 = unused data at end of resource<br />
        internal static int CompilePicData(AGIVersion version, ref byte[] visData, ref byte[] priData, byte[] picdata, int endpos, int statuspos) {
            // picture resource variables
            byte[] visBuildData, priBuildData;
            int pos;
            byte bytevalue;
            PenStatus CurrentPen;
            Queue<int> paintqueue = new();
            byte[][] CircleData =
            [
                [0x80],
                [0xC0, 0xC0, 0xC0],
                [0x40, 0xE0, 0xE0, 0xE0, 0x40],
                [0x60, 0x60, 0xF0, 0xF0, 0xF0, 0x60, 0x60],
                [0x20, 0x70, 0xF8, 0xF8, 0xF8, 0xF8, 0xF8, 0x70, 0x20],
                [0x30, 0x78, 0x78, 0x78, 0xFC, 0xFC, 0xFC, 0x78, 0x78, 0x78, 0x30],
                [0x38, 0x7C, 0x7C, 0x7C, 0xFE, 0xFE, 0xFE,0xFE, 0xFE, 0x7C, 0x7C, 0x7C, 0x38],
                [0x18, 0x3C, 0x7E, 0x7E, 0x7E, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7E, 0x7E, 0x7E, 0x3C, 0x18],
            ];

            // circle brush 1 is different in several versions of AGI
            switch (version) {
            case AGIVersion.v2425:
                CircleData[1] = [0xC0, 0xC0, 0];
                break;
            case AGIVersion.v2426:
            case AGIVersion.v2435:
                CircleData[1] = [0xC0, 0xC0, 0x40];
                break;
            case >= AGIVersion.v3002086:
                // all v3
                CircleData[1] = [0x00, 0xC0, 0x00];
                break;
            }

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
            visBuildData = new byte[26880];
            priBuildData = new byte[26880];
            for (int i = 0; i < 26880; i++) {
                visBuildData[i] = 15; // white
                priBuildData[i] = 4;  // red
            }
            pos = 0;
            bytevalue = picdata[pos++];
            try {
                do {
                    switch (bytevalue) {
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
                        bytevalue = picdata[pos++];
                        switch (version) {
                        case AGIVersion.v2089:
                        case AGIVersion.v2272:
                            // in v2.089 and v2.272, plot commands not supported
                            retval |= 16;
                            break;
                        case AGIVersion.v2411:
                            // in v2.411, plot style is ignored
                            if ((bytevalue & 0x20) == 0x20) {
                                // splatter won't work in 2.411
                                retval |= 32;
                            }
                            break;
                        default:
                            // Change pen size and style
                            CurrentPen.PlotStyle = (PlotStyle)((bytevalue & 0x20) / 0x20);
                            CurrentPen.PlotShape = (PlotShape)((bytevalue & 0x10) / 0x10);
                            CurrentPen.PlotSize = bytevalue & 0x7;
                            break;
                        }
                        bytevalue = picdata[pos++];
                        break;
                    case 0xFA:
                        // Plot with pen
                        if (version == AGIVersion.v2089 || version == AGIVersion.v2272) {
                            // in v2.089 and v2.272, plot commands not supported
                            retval |= 16;
                        }
                        BrushPlot();
                        break;
                    case 0xF0:
                        // Change picture color and enable picture draw
                        bytevalue = picdata[pos++];
                        CurrentPen.VisColor = (AGIColorIndex)(bytevalue & 0xF);
                        // AGI has a slight bug - if color is > 15, the
                        // upper nibble will overwrite the priority color
                        if (bytevalue > 15) {
                            // pass upper nibble to priority
                            CurrentPen.PriColor |= (AGIColorIndex)(bytevalue / 16);
                            retval |= 2;
                        }
                        bytevalue = picdata[pos++];
                        break;
                    case 0xF1:
                        // Disable visual draw
                        CurrentPen.VisColor = AGIColorIndex.None;
                        bytevalue = picdata[pos++];
                        break;
                    case 0xF2:
                        // Change priority color and enable priority draw
                        bytevalue = picdata[pos++];
                        // AGI uses ONLY priority color; if the passed value is
                        // greater than 15, the upper nibble gets ignored
                        CurrentPen.PriColor = ((AGIColorIndex)(bytevalue & 0xF));
                        bytevalue = picdata[pos++];
                        break;
                    case 0xF3:
                        // Disable priority draw
                        CurrentPen.PriColor = AGIColorIndex.None;
                        bytevalue = picdata[pos];
                        pos++;
                        break;
                    case 0xFF:
                        // end of drawing
                        if (statuspos == endpos) {
                            // save pen status
                            SavePen = CurrentPen;
                        }
                        break;
                    default:
                        // if expecting a command, and byte is <240 or >250 (not 255)
                        // just ignore it
                        bytevalue = picdata[pos++];
                        retval |= 4;
                        break;
                    }
                    if (pos > statuspos) {
                        SavePen = CurrentPen;
                        // reset statuspos to avoid changing save status
                        statuspos = 0x7FFFFFFF;
                    }
                    if (bytevalue == 0xFF) {
                        // end byte found
                        break;
                    }
                }
                while (pos <= endpos);
            }
            catch (Exception e) {
                // depending on error, set warning level
                Debug.Assert(e is IndexOutOfRangeException);
                // subscript error- caused when a draw function expects
                // another byte of data, but end of data is reached
                // confirm it
                Debug.Assert(pos > endpos);
                // set warning flag
                retval |= 1;
            }
            // if building entire picture
            if (endpos == picdata.Length - 1) {
                // confirm last command is end-of-picture marker
                if (bytevalue != 0xFF) {
                    // set warning flag
                    retval |= 1;
                }
                // check for unused data after end marker
                if (pos != picdata.Length) {
                    // set warning flag
                    retval |= 8;
                }
            }
            // copy resulting data back to calling function
            visData = visBuildData;
            priData = priBuildData;
            return retval;

            /// <summary>
            /// Adds the color to visual and priority picture based on current pen status.
            /// </summary>
            /// <param name="xPos"></param>
            /// <param name="yPos"></param>
            void DrawPixel(int xPos, int yPos) {
                int index = xPos + yPos * 160;
                if (index <= 26879) {
                    if (CurrentPen.VisColor < AGIColorIndex.None) {
                        visBuildData[index] = (byte)CurrentPen.VisColor;
                    }
                    if (CurrentPen.PriColor < AGIColorIndex.None) {
                        priBuildData[index] = (byte)CurrentPen.PriColor;
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
                // this method duplicates the AGI MSDOS draw function so 
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
                x = picdata[pos++];
                if (x > 239) {
                    bytevalue = x;
                    return false;
                }
                if (x > 159) {
                    x = 159;
                }
                y = picdata[pos++];
                if (y > 239) {
                    bytevalue = y;
                    return false;
                }
                if (y > 167) {
                    y = 167;
                }
                return true;
            }

            /// <summary>
            /// Draws a series of absolute lines.
            /// </summary>
            void DrawAbsLine() {
                byte X1 = 0, Y1 = 0, X2 = 0, Y2 = 0;
                if (!GetNextLocation(ref X1, ref Y1))
                    return;
                DrawPixel(X1, Y1);
                if (pos > endpos)
                    return;
                do {
                    if (!GetNextLocation(ref X2, ref Y2))
                        return;
                    DrawLine(X1, Y1, X2, Y2);
                    X1 = X2;
                    Y1 = Y2;
                } while (pos <= endpos);
            }

            /// <summary>
            /// Draws a series of relative lines.
            /// </summary>
            void DrawRelLine() {
                short xdisp, ydisp;
                byte X1 = 0, Y1 = 0;
                if (!GetNextLocation(ref X1, ref Y1))
                    return;
                DrawPixel(X1, Y1);
                if (pos > endpos)
                    return;
                do {
                    bytevalue = picdata[pos++];
                    if (bytevalue >= 0xF0)
                        return;
                    // check x sign bit
                    if ((bytevalue & 0x80) == 0x80) {
                        // displacement is negative
                        xdisp = (short)(0 - ((bytevalue & 0x70) / 0x10));
                    }
                    else {
                        xdisp = ((short)((bytevalue & 0x70) / 0x10));
                    }
                    // check y sign bit
                    if ((bytevalue & 0x8) == 0x8) {
                        // displacement is negative
                        ydisp = (short)(0 - (bytevalue & 0x7));
                    }
                    else {
                        ydisp = ((short)(bytevalue & 0x7));
                    }
                    DrawLine(X1, Y1, X1 + xdisp, Y1 + ydisp);
                    X1 += (byte)xdisp;
                    Y1 += (byte)ydisp;
                } while (pos <= endpos);
            }

            /// <summary>
            /// Performs a series of flood-fill actions.
            /// </summary>
            void PicFloodFill() {
                int offset;
                byte X = 0, Y = 0;

                do {
                    if (!GetNextLocation(ref X, ref Y))
                        return;
                    // if visual OR priority but not both
                    if ((CurrentPen.VisColor < AGIColorIndex.None) ^ (CurrentPen.PriColor < AGIColorIndex.None)) {
                        if (CurrentPen.VisColor < AGIColorIndex.None) {
                            // fill visual screen
                            if (CurrentPen.VisColor != AGIColorIndex.White && (AGIColorIndex)visBuildData[X + 160 * Y] == AGIColorIndex.White) {
                                offset = Y * 160 + X;
                                paintqueue.Enqueue(offset);
                                visBuildData[offset] = (byte)CurrentPen.VisColor;
                                do {
                                    offset = paintqueue.Dequeue();
                                    X = (byte)(offset % 160);
                                    Y = (byte)(offset / 160);
                                    // check above
                                    if (Y > 0) {
                                        offset = (Y - 1) * 160 + X;
                                        if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                            visBuildData[offset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                    // check left
                                    if (X > 0) {
                                        offset = Y * 160 + X - 1;
                                        if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                            visBuildData[offset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                    // check right
                                    if (X < 159) {
                                        offset = Y * 160 + X + 1;
                                        if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                            visBuildData[offset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                    // check below
                                    if (Y < 167) {
                                        offset = (Y + 1) * 160 + X;
                                        if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                            visBuildData[offset] = (byte)CurrentPen.VisColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                }
                                while (paintqueue.Count > 0);
                            }
                        }
                        else {
                            // fill priority screen
                            if (CurrentPen.PriColor != AGIColorIndex.Red && (AGIColorIndex)priBuildData[X + 160 * Y] == AGIColorIndex.Red) {
                                offset = Y * 160 + X;
                                paintqueue.Enqueue(offset);
                                priBuildData[offset] = (byte)CurrentPen.PriColor;
                                do {
                                    offset = paintqueue.Dequeue();
                                    X = (byte)(offset % 160);
                                    Y = (byte)(offset / 160);
                                    // check above
                                    if (Y > 0) {
                                        offset = (Y - 1) * 160 + X;
                                        if ((AGIColorIndex)priBuildData[offset] == AGIColorIndex.Red) {
                                            priBuildData[offset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                    // check left
                                    if (X > 0) {
                                        offset = Y * 160 + X - 1;
                                        if ((AGIColorIndex)priBuildData[offset] == AGIColorIndex.Red) {
                                            priBuildData[offset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                    // check right
                                    if (X < 159) {
                                        offset = Y * 160 + X + 1;
                                        if ((AGIColorIndex)priBuildData[offset] == AGIColorIndex.Red) {
                                            priBuildData[offset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                    // check below
                                    if (Y < 167) {
                                        offset = (Y + 1) * 160 + X;
                                        if ((AGIColorIndex)priBuildData[offset] == AGIColorIndex.Red) {
                                            priBuildData[offset] = (byte)CurrentPen.PriColor;
                                            paintqueue.Enqueue(offset);
                                        }
                                    }
                                }
                                while (paintqueue.Count > 0);
                            }
                        }
                    }
                    else if ((CurrentPen.VisColor < AGIColorIndex.None) && (CurrentPen.VisColor < AGIColorIndex.None)) {
                        // drawing both
                        if (CurrentPen.VisColor != AGIColorIndex.White && (AGIColorIndex)visBuildData[X + 160 * Y] == AGIColorIndex.White) {
                            offset = Y * 160 + X;
                            paintqueue.Enqueue(offset);
                            visBuildData[offset] = (byte)CurrentPen.VisColor;
                            priBuildData[offset] = (byte)CurrentPen.PriColor;

                            do {
                                offset = paintqueue.Dequeue();
                                X = (byte)(offset % 160);
                                Y = (byte)(offset / 160);
                                // check above
                                if (Y > 0) {
                                    offset = (Y - 1) * 160 + X;
                                    if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                        visBuildData[offset] = (byte)CurrentPen.VisColor;
                                        priBuildData[offset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(offset);
                                    }
                                }
                                // check left
                                if (X > 0) {
                                    offset = Y * 160 + X - 1;
                                    if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                        visBuildData[offset] = (byte)CurrentPen.VisColor;
                                        priBuildData[offset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(offset);
                                    }
                                }
                                // check right
                                if (X < 159) {
                                    offset = Y * 160 + X + 1;
                                    if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                        visBuildData[offset] = (byte)CurrentPen.VisColor;
                                        priBuildData[offset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(offset);
                                    }
                                }
                                // check below
                                if (Y < 167) {
                                    offset = (Y + 1) * 160 + X;
                                    if ((AGIColorIndex)visBuildData[offset] == AGIColorIndex.White) {
                                        visBuildData[offset] = (byte)CurrentPen.VisColor;
                                        priBuildData[offset] = (byte)CurrentPen.PriColor;
                                        paintqueue.Enqueue(offset);
                                    }
                                }
                            }
                            while (paintqueue.Count > 0);
                        }
                    }
                } while (pos <= endpos);
            }

            /// <summary>
            /// Draws a series of alternating horizontal and vertical lines.
            /// </summary>
            /// <param name="CurAxis"></param>
            void DrawCorner(CornerDirection CurAxis) {
                byte X1 = 0, Y1 = 0, X2, Y2;

                if (!GetNextLocation(ref X1, ref Y1))
                    return;
                DrawPixel(X1, Y1);
                if (pos > endpos)
                    return;

                do {
                    if (CurAxis == CornerDirection.cdX) {
                        X2 = picdata[pos++];
                        if (X2 > 239) {
                            bytevalue = X2;
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
                        Y2 = picdata[pos++];
                        if (Y2 > 239) {
                            bytevalue = Y2;
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
                } while (pos <= endpos);
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
                        bytevalue = picdata[pos++];
                        if (bytevalue >= 0xF0)
                            return;
                        PatternNum = (byte)(bytevalue | 1);
                    }
                    if (!GetNextLocation(ref pX, ref pY)) {
                        return;
                    }
                    // adjust coordinates to upper/left values
                    switch (version) {
                    case AGIVersion.v2089:
                    case AGIVersion.v2272:
                        // since plot commands not supported in these versions
                        // so just skip the coordinates
                        continue;
                    case AGIVersion.v2411:
                        // single solid pixel
                        DrawPixel(pX, pY);
                        continue;
                    case AGIVersion.v2425:
                    case AGIVersion.v2426:
                        // offset with error
                        PlotX = pX - (CurrentPen.PlotSize >> 1);
                        PlotY = pY - (CurrentPen.PlotSize >> 1);
                        break;
                    default:
                        // all others use correct offset
                        PlotX = pX - ((CurrentPen.PlotSize + 1) >> 1);
                        PlotY = pY - CurrentPen.PlotSize;
                        break;
                    }
                    // validate starting pos
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
                    if (PlotY < 0) {
                        PlotY = 0;
                    }
                    else if (PlotY > 167 - CurrentPen.PlotSize) {
                        PlotY = 167 - CurrentPen.PlotSize;
                    }
                    if (CurrentPen.PlotShape == PlotShape.Circle) {
                        for (Y = 0; Y <= CurrentPen.PlotSize * 2; Y++) {
                            for (X = 0; X <= CurrentPen.PlotSize; X++) {
                                if ((CircleData[CurrentPen.PlotSize][Y] & (1 << (7 - X))) == (1 << (7 - X))) {
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
                } while (pos <= endpos);
            }
        }
        #endregion
    }
}
