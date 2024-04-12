using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine {
    public static partial class Base {
        public enum CornerDirection {
            cdX,
            cdY,
        }
        // picture resource variables
        private static byte[] VisBuildData, PriBuildData, agPicData;
        private static int lngPos;
        private static int lngEndPos;
        private static short bytIn;
        private static PenStatus CurrentPen, SavePen;
        private static int[] Queue = new int[26880];
        private static readonly byte[] CircleData = new byte[64];
        private static bool InitPlotData;

        /// <summary>
        /// Returns the picture pen status as of the last drawn command.
        /// This method should ONLY be called by code that just completed 
        /// a BuildBMPs call; otherwise the results are meaningless.
        /// </summary>
        /// <returns></returns>
        internal static PenStatus GetToolStatus() {
            return SavePen;
        }

        /// <summary>
        /// Converts the data extracted from the picture resource into 
        /// a 256 bit color DIBitmap.
        /// </summary>
        /// <param name="VisData"></param>
        /// <param name="PriData"></param>
        /// <param name="bytPicData"></param>
        /// <param name="EndPos"></param>
        /// <param name="StatusPos"></param>
        /// <returns>0 if successful, no errors/warnings<br />
        /// non-zero for error/warning:<br />
        ///  1 = no EOP marker<br />
        ///  2 = bad vis color data<br />
        ///  4 = invalid command byte<br />
        ///  8 = other error</returns>
        internal static int BuildBMPs(ref byte[] VisData, ref byte[] PriData, byte[] bytPicData, int EndPos, int StatusPos) {
            // assume ok
            int retval = 0;
            if (!InitPlotData) {
                InitializePlotData();
            }
            if (EndPos == -1) {
                // use all data
                EndPos = bytPicData.Length - 1;
            }
            if (StatusPos == -1) {
                // use end pos
                StatusPos = EndPos;
            }

            // save endpos locally
            lngEndPos = EndPos;
            // pointer to input data so other draw functions can reach it
            agPicData = bytPicData;

            // set starting/default pen status
            CurrentPen.VisColor = AGIColorIndex.agNone;
            CurrentPen.PriColor = AGIColorIndex.agNone;
            CurrentPen.PlotSize = 0;
            CurrentPen.PlotShape = EPlotShape.psCircle;
            CurrentPen.PlotStyle = EPlotStyle.psSolid;

            //clear the working byte arrays
            VisBuildData = new byte[26880];
            PriBuildData = new byte[26880];
            for (int i = 0; i < 26880; i++) {
                VisBuildData[i] = 15; //white
                PriBuildData[i] = 4;  //red
            }

            lngPos = 0;
            bytIn = bytPicData[lngPos++];
            try {
                do {
                    switch (bytIn) {
                    case 0xF6:
                        // Absolute line (long lines).
                        DrawAbsLine();
                        break;
                    case 0xF7:
                        // Relative line (short lines).
                        DrawRelLine();
                        break;
                    case 0xF8:
                        // Fill.
                        PicFloodFill();
                        break;
                    case 0xF4:
                        // Draw a Y corner.
                        DrawCorner(CornerDirection.cdY);
                        break;
                    case 0xF5:
                        // Draw an X corner.
                        DrawCorner(CornerDirection.cdX);
                        break;
                    case 0xF9:
                        // Change pen size and style.
                        bytIn = bytPicData[lngPos++];
                        CurrentPen.PlotStyle = (EPlotStyle)((bytIn & 0x20) / 0x20);
                        CurrentPen.PlotShape = (EPlotShape)((bytIn & 0x10) / 0x10);
                        CurrentPen.PlotSize = bytIn & 0x7;
                        bytIn = bytPicData[lngPos++];
                        break;
                    case 0xFA:
                        // Plot with pen.
                        BrushPlot();
                        break;
                    case 0xF0:
                        // Change picture color and enable picture draw.
                        bytIn = bytPicData[lngPos++];
                        CurrentPen.VisColor = (AGIColorIndex)(bytIn & 0xF);
                        // AGI has a slight bug; if color is > 15, the
                        // upper nibble will overwrite the priority color
                        if (bytIn > 15) {
                            // pass upper nibble to priority
                            CurrentPen.PriColor |= (AGIColorIndex)(bytIn / 16);
                            retval |= 2;
                        }
                        bytIn = bytPicData[lngPos++];
                        break;
                    case 0xF1:
                        // Disable visual draw.
                        CurrentPen.VisColor = AGIColorIndex.agNone;
                        bytIn = bytPicData[lngPos++];
                        break;
                    case 0xF2:
                        // Change priority color and enable priority draw.
                        bytIn = bytPicData[lngPos++];
                        // AGI uses ONLY priority color; if the passed value is
                        // greater than 15, the upper nibble gets ignored
                        CurrentPen.PriColor = ((AGIColorIndex)(bytIn & 0xF));
                        bytIn = bytPicData[lngPos++];
                        break;
                    case 0xF3:
                        // Disable priority draw.
                        CurrentPen.PriColor = AGIColorIndex.agNone;
                        bytIn = bytPicData[lngPos];
                        lngPos++;
                        break;
                    case 0xFF:
                        // end of drawing
                        if (StatusPos == EndPos) {
                            // save pen status
                            SavePen = CurrentPen;
                        }
                        break;
                    default:
                        // if expecting a command, and byte is <240 but >250 (not 255)
                        // just ignore it
                        // get next command byte
                        bytIn = bytPicData[lngPos++];
                        retval |= 4;
                        break;
                    }
                    if (lngPos > StatusPos) {
                        SavePen = CurrentPen;
                        // reset statuspos to avoid changing save status
                        StatusPos = 0x7FFFFFFF;
                    }
                    // if end byte found
                    if (bytIn == 0xFF) {
                        break;
                    }
                }
                while (lngPos <= EndPos);
            }
            catch (Exception e) {
                // depending on error, set warning level
                if (e is IndexOutOfRangeException) {
                    // subscript error- caused when a draw function expects
                    // another byte of data, but end of data is reached
                    // confirm it
                    if (lngPos > EndPos) {
                        // set warning flag
                        retval |= 1;
                    }
                    else {
                        // something else
                        retval |= 8;
                    }
                }
                else {
                    // any other error- just pass it along
                    retval |= 8;
                }
            }
            // if at end of resource, was last command end-of-resource flag?
            if (lngPos >= bytPicData.Length) {
                if (bytPicData[^1] != 0xFF) {
                    // set warning flag
                    retval |= 1;
                }
            }
            // copy resulting data back to calling function
            VisData = VisBuildData;
            PriData = PriBuildData;
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void InitializePlotData() {
            //circle data used by the
            //brush drawing functions to paint
            //pictures
            CircleData[0] = 0x80;
            CircleData[1] = 0xC0;
            CircleData[2] = 0xC0;
            CircleData[3] = 0xC0;
            CircleData[4] = 0x40;
            CircleData[5] = 0xE0;
            CircleData[6] = 0xE0;
            CircleData[7] = 0xE0;
            CircleData[8] = 0x40;
            CircleData[9] = 0x60;
            CircleData[10] = 0x60;
            CircleData[11] = 0xF0;
            CircleData[12] = 0xF0;
            CircleData[13] = 0xF0;
            CircleData[14] = 0x60;
            CircleData[15] = 0x60;
            CircleData[16] = 0x20;
            CircleData[17] = 0x70;
            CircleData[18] = 0xF8;
            CircleData[19] = 0xF8;
            CircleData[20] = 0xF8;
            CircleData[21] = 0xF8;
            CircleData[22] = 0xF8;
            CircleData[23] = 0x70;
            CircleData[24] = 0x20;
            CircleData[25] = 0x30;
            CircleData[26] = 0x78;
            CircleData[27] = 0x78;
            CircleData[28] = 0x78;
            CircleData[29] = 0xFC;
            CircleData[30] = 0xFC;
            CircleData[31] = 0xFC;
            CircleData[32] = 0x78;
            CircleData[33] = 0x78;
            CircleData[34] = 0x78;
            CircleData[35] = 0x30;
            CircleData[36] = 0x38;
            CircleData[37] = 0x7C;
            CircleData[38] = 0x7C;
            CircleData[39] = 0x7C;
            CircleData[40] = 0xFE;
            CircleData[41] = 0xFE;
            CircleData[42] = 0xFE;
            CircleData[43] = 0xFE;
            CircleData[44] = 0xFE;
            CircleData[45] = 0x7C;
            CircleData[46] = 0x7C;
            CircleData[47] = 0x7C;
            CircleData[48] = 0x38;
            CircleData[49] = 0x18;
            CircleData[50] = 0x3C;
            CircleData[51] = 0x7E;
            CircleData[52] = 0x7E;
            CircleData[53] = 0x7E;
            CircleData[54] = 0xFF;
            CircleData[55] = 0xFF;
            CircleData[56] = 0xFF;
            CircleData[57] = 0xFF;
            CircleData[58] = 0xFF;
            CircleData[59] = 0x7E;
            CircleData[60] = 0x7E;
            CircleData[61] = 0x7E;
            CircleData[62] = 0x3C;
            CircleData[63] = 0x18;
            InitPlotData = true;
        }

        /// <summary>
        /// Adds the color to visual and priority picture based on current pen status.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        private static void DrawPixel(int xPos, int yPos) {
            int lngIndex = xPos + yPos * 160;
            if (lngIndex <= 26879) {
                if (CurrentPen.VisColor < AGIColorIndex.agNone) {
                    VisBuildData[lngIndex] = (byte)CurrentPen.VisColor;
                }
                if (CurrentPen.PriColor < AGIColorIndex.agNone) {
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
        private static void DrawLine(int X1, int Y1, int X2, int Y2) {
            // this method mirrors the AGI MSDOS draw function so 
            // lines are guaranteed to be an exact match.
            int xPos, yPos;
            int DY, DX;
            int vDir, hDir;
            int XC, YC, MaxDelta;
            int i;
            // Sierra sucked at checking for overflows; if a bad value for
            // coords was used that overflowed the designated picture buffer,
            // AGI didn't care; it would just draw the pixelin invalid memory
            // and plod on. To mimic that we just ignore invalid pixels and
            //plod on too. ugh...
            // (would be nice if there was a way to warn the user; not sure
            // I can do that though)

            // determine deltaX/deltaY and direction
            DY = Y2 - Y1;
            vDir = Math.Sign(DY);
            DX = X2 - X1;
            hDir = Math.Sign(DX);

            if (DY == 0 && DX == 0) {
                DrawPixel(X1, Y1);
            }
            // if horizontal
            else if (DY == 0) {
                for (i = X1; i != X2; i += hDir) {
                    DrawPixel(i, Y1);
                }
                DrawPixel(X2, Y1);
            }
            // if vertical
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

        /// <summary>
        /// Draws a series of alternating horizontal and vertical lines.
        /// </summary>
        /// <param name="CurAxis"></param>
        private static void DrawCorner(CornerDirection CurAxis) {
            byte X1, Y1, X2, Y2;
            // read in start coordinates
            X1 = agPicData[lngPos++];
            Y1 = agPicData[lngPos++];
            // draw first pixel
            DrawPixel(X1, Y1);
            //get next byte
            bytIn = agPicData[lngPos++];
            while (bytIn < 0xF0 && lngPos <= lngEndPos)
            {
                if (CurAxis == CornerDirection.cdX) {
                    X2 = (byte)bytIn;
                    Y2 = Y1;
                    DrawLine(X1, Y1, X2, Y2);
                    CurAxis = CornerDirection.cdY;
                    X1 = X2;
                }
                else {
                    Y2 = (byte)bytIn;
                    X2 = X1;
                    DrawLine(X1, Y1, X2, Y2);
                    CurAxis = CornerDirection.cdX;
                    Y1 = Y2;
                }
                bytIn = agPicData[lngPos++];
            }
        }

        /// <summary>
        /// Draws a series of absolute lines.
        /// </summary>
        private static void DrawAbsLine() {
            byte X1, Y1, X2, Y2;

            X1 = agPicData[lngPos++];
            Y1 = agPicData[lngPos++];
            DrawPixel(X1, Y1);
            //get next potential coordinate
            bytIn = agPicData[lngPos++];

            while (bytIn < 0xF0 && lngPos <= lngEndPos) {
                X2 = (byte)bytIn;
                Y2 = agPicData[lngPos++];
                DrawLine(X1, Y1, X2, Y2);
                X1 = X2;
                Y1 = Y2;
                bytIn = agPicData[lngPos++];
            }
        }
        
        /// <summary>
        /// Draws a series of relative lines.
        /// </summary>
        private static void DrawRelLine() {
            short xdisp, ydisp;
            byte X1, Y1;

            X1 = agPicData[lngPos++];
            Y1 = agPicData[lngPos++];
            DrawPixel(X1, Y1);
            bytIn = agPicData[lngPos++];
            while (bytIn < 0xF0 && lngPos <= lngEndPos) {
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
                bytIn = agPicData[lngPos++];
            }
        }
        
        /// <summary>
        /// Draws a series of plot patterns using current pen settings. 
        /// </summary>
        private static void BrushPlot() {
            int PlotX, PlotY;
            byte PatternNum = 0;
            int X, Y;

            // get next value (Xpos or splatter code)
            bytIn = agPicData[lngPos++];

            while (bytIn < 0xF0 && lngPos <= lngEndPos) {
                if (CurrentPen.PlotStyle == EPlotStyle.psSplatter) {
                    PatternNum = (byte)(bytIn | 1);
                    // next byte is Xpos
                    bytIn = agPicData[lngPos++];
                    if (bytIn >= 0xF0) {
                        // exit if a draw command is found
                        break;
                    }
                }
                PlotX = bytIn;
                bytIn = agPicData[lngPos++];
                if (bytIn >= 0xF0) {
                    // exit if a draw command is found
                    break;
                }
                PlotY = bytIn;
                // adjust coordinates to upper/left values
                // needed based on pen size
                PlotX -= (CurrentPen.PlotSize + 1) / 2;
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
                PlotY -= CurrentPen.PlotSize;
                if (PlotY < 0) {
                    PlotY = 0;
                }
                else if (PlotY > 167 - CurrentPen.PlotSize) {
                    PlotY = 167 - CurrentPen.PlotSize;
                }
                if (CurrentPen.PlotShape == EPlotShape.psCircle) {
                    for (Y = 0; Y <= CurrentPen.PlotSize * 2; Y++) {
                        for (X = 0; X <= CurrentPen.PlotSize; X++) {
                            if ((CircleData[(CurrentPen.PlotSize * CurrentPen.PlotSize) + Y] & (1 << (7 - X))) == (1 << (7 - X))) {
                                if (CurrentPen.PlotStyle == EPlotStyle.psSplatter) {
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
                            if (CurrentPen.PlotStyle == EPlotStyle.psSplatter) {
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
                //get next byte
                bytIn = agPicData[lngPos++];
            }
        }

        /// <summary>
        /// Performs a series of flood-fill actions.
        /// </summary>
        private static void PicFloodFill() {
            int QueueStart, QueueEnd, lngOffset;
            byte X, Y;

            bytIn = agPicData[lngPos++];
            while (bytIn < 0xF0 && lngPos <= lngEndPos) {
                X = (byte)bytIn;
                Y = agPicData[lngPos++];
                // if visual OR priority but not both
                if ((CurrentPen.VisColor < AGIColorIndex.agNone) ^ (CurrentPen.PriColor < AGIColorIndex.agNone)) {
                    if (CurrentPen.VisColor < AGIColorIndex.agNone) {
                        // fill visual screen
                        if (CurrentPen.VisColor != AGIColorIndex.agWhite && (AGIColorIndex)VisBuildData[X + 160 * Y] == AGIColorIndex.agWhite) {
                            QueueStart = 0;
                            QueueEnd = 1;
                            lngOffset = Y * 160 + X;
                            Queue[QueueStart] = lngOffset;
                            VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                            do {
                                lngOffset = Queue[QueueStart++];
                                X = (byte)(lngOffset % 160);
                                Y = (byte)(lngOffset / 160);
                                // check above
                                if (Y > 0) {
                                    lngOffset = (Y - 1) * 160 + X;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                                // check left
                                if (X > 0) {
                                    lngOffset = Y * 160 + X - 1;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                                // check right
                                if (X < 159) {
                                    lngOffset = Y * 160 + X + 1;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                                // check below
                                if (Y < 167) {
                                    lngOffset = (Y + 1) * 160 + X;
                                    if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                            }
                            while (QueueStart != QueueEnd);
                        }
                    }
                    else {
                        // fill priority screen
                        if (CurrentPen.PriColor != AGIColorIndex.agRed && (AGIColorIndex)PriBuildData[X + 160 * Y] == AGIColorIndex.agRed) {
                            QueueStart = 0;
                            QueueEnd = 1;
                            lngOffset = Y * 160 + X;
                            Queue[QueueStart] = lngOffset;
                            PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                            do {
                                lngOffset = Queue[QueueStart++];
                                X = (byte)(lngOffset % 160);
                                Y = (byte)(lngOffset / 160);
                                // check above
                                if (Y > 0) {
                                    lngOffset = (Y - 1) * 160 + X;
                                    if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.agRed) {
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                                // check left
                                if (X > 0) {
                                    lngOffset = Y * 160 + X - 1;
                                    if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.agRed) {
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                                // check right
                                if (X < 159) {
                                    lngOffset = Y * 160 + X + 1;
                                    if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.agRed) {
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                                // check below
                                if (Y < 167) {
                                    lngOffset = (Y + 1) * 160 + X;
                                    if ((AGIColorIndex)PriBuildData[lngOffset] == AGIColorIndex.agRed) {
                                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                        Queue[QueueEnd++] = lngOffset;
                                    }
                                }
                            }
                            while (QueueStart != QueueEnd);
                        }
                    }
                }
                else if ((CurrentPen.VisColor < AGIColorIndex.agNone) && (CurrentPen.VisColor < AGIColorIndex.agNone)) {
                    // drawing both
                    if (CurrentPen.VisColor != AGIColorIndex.agWhite && (AGIColorIndex)VisBuildData[X + 160 * Y] == AGIColorIndex.agWhite) {
                        QueueStart = 0;
                        QueueEnd = 1;
                        lngOffset = Y * 160 + X;
                        Queue[QueueStart] = lngOffset;
                        VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                        PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;

                        do {
                            lngOffset = Queue[QueueStart++];
                            X = (byte)(lngOffset % 160);
                            Y = (byte)(lngOffset / 160);
                            // check above
                            if (Y > 0) {
                                lngOffset = (Y - 1) * 160 + X;
                                if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                    VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                    PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                    Queue[QueueEnd++] = lngOffset;
                                }
                            }
                            // check left
                            if (X > 0) {
                                lngOffset = Y * 160 + X - 1;
                                if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                    VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                    PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                    Queue[QueueEnd++] = lngOffset;
                                }
                            }
                            // check right
                            if (X < 159) {
                                lngOffset = Y * 160 + X + 1;
                                if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                    VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                    PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                    Queue[QueueEnd++] = lngOffset;
                                }
                            }
                            // check below
                            if (Y < 167) {
                                lngOffset = (Y + 1) * 160 + X;
                                if ((AGIColorIndex)VisBuildData[lngOffset] == AGIColorIndex.agWhite) {
                                    VisBuildData[lngOffset] = (byte)CurrentPen.VisColor;
                                    PriBuildData[lngOffset] = (byte)CurrentPen.PriColor;
                                    Queue[QueueEnd++] = lngOffset;
                                }
                            }
                        }
                        while (QueueStart != QueueEnd);
                    }
                }
                bytIn = agPicData[lngPos++];
            }
        }
        
        /// <summary>
        /// This method attempts to convert a long color Value
        /// into the corresponding AGI color index with the least
        /// amount of calculations. It is an empirically derived
        /// algorithm.
        /// </summary>
        /// <param name="lngEGAIn"></param>
        /// <returns></returns>
        public static int GetColVal(int lngEGAIn) {
            // If this method is called with a color Value
            // other than the defined EGA color values for AGI, then
            // I have absolutely no idea what the return Value may
            // look like.
            int cR, cG, cB, vR, vG, vB;
            int retval;

            //split the color up
            cR = lngEGAIn % 256;
            cG = lngEGAIn / 256 % 256;
            cB = lngEGAIn / 65536 % 256;
            //convert to component numbers
            if (cR == 0xFF) {
                vR = 32;
            }
            else if (cR == 0) {
                vR = 0;
            }
            else {
                vR = 16;
            }
            if (cG == 0xFF) {
                vG = 32;
            }
            else if (cG == 0) {
                vG = 0;
            }
            else {
                vG = 16;
            }
            if (cB == 0xFF) {
                vB = 32;
            }
            else if (cB == 0) {
                vB = 0;
            }
            else {
                vB = 16;
            }
            //build composite
            retval = (vB + vG * 2 + vR * 4) / 16;
            if (retval < 5) {
                return retval;
            }
            if (retval > 9) {
                return retval++;
            }
            if (cR > 0x50) {
                return retval;
            }
            // three cases left:
            // 7,8,5 corresponding to 8,9,10
            if (retval > 6) {
                return retval++;
            }
            // only one left is light green
            return 10;
        }
    }
}
