using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.frmPicEdit.DrawFunction;
using static WinAGI.Editor.PictureUndo.ActionType;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    public partial class frmPicEdit : ClipboardMonitor, IMessageFilter {
        #region Picture Editor Structs
        /// <summary>
        /// Struct to hold information about the current command being edited.
        /// </summary>
        public struct CommandInfo {
            public int Index;
            public int Position;
            public DrawFunction Type;
            public PenStatus Pen;
            public List<Point> Coords;
            public int SelectedCoordIndex;
            public Point SelectedCoord {
                get {
                    if (SelectedCoordIndex < 0 || SelectedCoordIndex >= Coords.Count) return new(-1, -1);
                    return Coords[SelectedCoordIndex];
                }
            }
            public int SelectedCoordPos {
                get {
                    return CoordPos(SelectedCoordIndex);
                }
            }
            public int CoordPos(int index) {
                if (index == 0) return Position + 1;
                if (index < 0) return Position;
                // allow going one past last (same as endpos + 1)
                if (index > Coords.Count) index = Coords.Count;
                switch (Type) {
                case YCorner:
                case XCorner:
                case RelLine:
                    return Position + 2 + index;
                case AbsLine:
                case Fill:
                    return Position + (2 * index) + 1;
                case PlotPen:
                    if (Pen.PlotStyle == PlotStyle.Splatter) {
                        return Position + (3 * index) + 1;
                    }
                    else {
                        return Position + (2 * index) + 1;
                    }
                }
                // to keep compiler happy
                return -1;
            }
            public bool IsLine { get => Type >= YCorner && Type <= RelLine; }
            public bool IsPen { get => Type <= DisablePri || Type == ChangePen; }
            public int EndPos {
                get {
                    if (Coords.Count == 0) return Position;
                    switch (Type) {
                    case XCorner:
                    case YCorner:
                    case RelLine:
                        return Position + 2 + Coords.Count;
                    case AbsLine:
                    case Fill:
                        return Position + (2 * Coords.Count);
                    case PlotPen:
                        if (Pen.PlotStyle == PlotStyle.Splatter) {
                            return Position + (3 * Coords.Count);
                        }
                        else {
                            return Position + (2 * Coords.Count);
                        }
                    }
                    return Position;
                }
            }
            public CommandInfo() {
                Index = -1;
                Position = -1;
                Type = End;
                Pen = new();
                Coords = [];
                SelectedCoordIndex = -1;
            }
        }

        /// <summary>
        /// Struct to hold information about the current print/display test settings.
        /// </summary>
        public struct PrintTestInfo {
            // read-only (calculated) properties
            private bool isChanged = false;

            private int tErrLevel = 0;
            public int ErrLevel {
                get {
                    if (isChanged) {
                        if (tMode == PrintTestMode.Display) {
                            tData = CompileMsg(tText);
                        }
                        else {
                            tData = FormatMsg(tText);
                        }
                        isChanged = false;
                    }
                    return tErrLevel;
                }
            }

            private byte[] tData = [];
            public byte[] Data {
                get {
                    if (isChanged) {
                        if (tMode == PrintTestMode.Display) {
                            tData = CompileMsg(tText);
                        }
                        else {
                            tData = FormatMsg(tText);
                        }
                        isChanged = false;
                    }
                    return tData;
                }
            }

            private int tTop = 0;
            public int Top {
                get {
                    if (isChanged) {
                        if (tMode == PrintTestMode.Display) {
                            tData = CompileMsg(tText);
                        }
                        else {
                            tData = FormatMsg(tText);
                        }
                        isChanged = false;
                    }
                    return tTop;
                }
            }

            private int tLeft = 0;
            public int Left {
                get {
                    if (isChanged) {
                        if (tMode == PrintTestMode.Display) {
                            tData = CompileMsg(tText);
                        }
                        else {
                            tData = FormatMsg(tText);
                        }
                        isChanged = false;
                    }
                    return tLeft;
                }
            }

            private int tWidth = 0;
            public int Width {
                get {
                    if (isChanged) {
                        if (tMode == PrintTestMode.Display) {
                            tData = CompileMsg(tText);
                        }
                        else {
                            tData = FormatMsg(tText);
                        }
                        isChanged = false;
                    }
                    return tWidth;
                }
            }

            private int tHeight = 0;
            public int Height {
                get {
                    if (isChanged) {
                        if (tMode == PrintTestMode.Display) {
                            tData = CompileMsg(tText);
                        }
                        else {
                            tData = FormatMsg(tText);
                        }
                        isChanged = false;
                    }
                    return tHeight;
                }
            }

            // user-adjustable properties
            public int CodePage = 437;

            private PrintTestMode tMode = PrintTestMode.Print;
            public PrintTestMode Mode {
                get => tMode;
                set {
                    tMode = value;
                    isChanged = true;
                }
            }

            private int tPicOffset = 1;
            public int PicOffset {
                get => tPicOffset;
                set {
                    tPicOffset = value;
                    isChanged = true;
                }
            }

            private int tMaxWidth = 30;
            public int MaxWidth {
                get => tMaxWidth;
                set {
                    tMaxWidth = value;
                    isChanged = true;
                }
            }

            private string tText = "";
            public string Text {
                get => tText;
                set {
                    tText = value;
                    isChanged = true;
                }
            }

            private int tStartCol = 0;
            public int StartCol {
                get => tStartCol;
                set {
                    tStartCol = value;
                    isChanged = true;
                }
            }
            
            private int tStartRow = 0;
            public int StartRow {
                get => tStartRow;
                set {
                    tStartRow = value;
                    isChanged = true;
                }
            }
            
            private int tBGColor = 0;
            public int BGColor {
                get => tBGColor;
                set {
                    tBGColor = value;
                    isChanged = true;
                }
            }
            
            private int tFGColor = 15;
            public int FGColor {
                get => tFGColor;
                set {
                    tFGColor = value;
                    isChanged = true;
                }
            }
            
            private int tMaxCol = 39;
            public int MaxCol {
                get => tMaxCol;
                set {
                    tMaxCol = value;
                    isChanged = true;
                }
            }
            
            private int tCharWidth = 8;
            public int CharWidth {
                get => tCharWidth;
                set {
                    tCharWidth = value;
                    isChanged = true;
                }
            }

            // constructors
            public PrintTestInfo() {
            }

            public PrintTestInfo(int codepage) {
                CodePage = codepage;
            }

            public PrintTestInfo(PrintTestInfo source) {
                // copy all non-calculated properties and  
                // mark as changed
                CodePage = source.CodePage;
                tMode = source.tMode;
                tPicOffset = source.tPicOffset;
                tMaxWidth = source.tMaxWidth;
                tText = source.tText;
                tStartRow = source.tStartRow;
                tStartCol = source.tStartCol;
                tBGColor = source.BGColor;
                tFGColor = source.FGColor;
                tMaxCol = source.tMaxCol;
                tCharWidth = source.tCharWidth;
                isChanged = true;
            }

            // methods
            private byte[] FormatMsg(string msg) {
                // formats a message string to fit in a print box

                int lineWidth = 0, brk = 0, NewMax = 0;
                bool blnESC = false;

                int intMode = 0;
                //  0= normal text
                //  1= % format code

                // first, replace compiler format codes
                tData = CompileMsg(msg);

                // then, format to fit width
                List<byte> output = [];
                int rowcount = 1;
                for (int i = 0; i < tData.Length; i++) {
                    byte intChar = tData[i];
                    switch (intMode) {
                    case 0:
                        // normal
                        if (blnESC) {
                            blnESC = false;
                            // always add the char
                            output.Add(intChar);
                            // increment width
                            lineWidth++;
                            if (intChar == 32) {
                                // update the break
                                brk = lineWidth;
                            }
                        }
                        else {
                            switch (intChar) {
                            case 10:
                                // newline
                                // add the char
                                output.Add(intChar);
                                // increment row
                                rowcount++;
                                // update actual max
                                if (lineWidth > NewMax) {
                                    NewMax = lineWidth;
                                }
                                // reset width
                                lineWidth = 0;
                                // reset break
                                brk = 0;
                                break;
                            case 32:
                                // space
                                // add the char
                                output.Add(intChar);
                                // increment width
                                lineWidth++;
                                // update the break
                                brk = lineWidth;
                                break;

                            case 37:
                                // %
                                // check for percent format code
                                intMode = 1;
                                break; // exit do
                            case 92:
                                // \
                                // esc code
                                blnESC = true;
                                break; // exit do
                            default:
                                // add the char
                                output.Add(intChar);
                                // increment width
                                lineWidth++;
                                break;
                            }
                        }
                        break;
                    case 1:
                        // % format code
                        intMode = 0;
                        // TODO: need to add support for format codes
                        break;
                    }
                    if (lineWidth == tMaxWidth) {
                        if (brk == 0) {
                            // add break here
                            output.Add(10);
                            rowcount++;
                            // update actual max
                            if (lineWidth > NewMax) {
                                NewMax = lineWidth;
                            }
                            // reset width
                            lineWidth = 0;
                        }
                        else {
                            // insert cr at break, clearing ending space
                            output.Insert(output.Count - lineWidth + brk - 1, 10);
                            rowcount++;
                            // update actual max
                            if (brk - 1 > NewMax) {
                                NewMax = brk - 1;
                            }
                            // reset width
                            lineWidth = lineWidth - brk;
                            // reset break
                            brk = 0;
                        }
                    }
                }
                // update actual max on last row
                if (lineWidth > NewMax) {
                    NewMax = lineWidth;
                }
                // save height/width
                tHeight = (byte)rowcount;
                tWidth = (byte)NewMax;
                if (tMode == PrintTestMode.Print) {
                    // calculate msgbox top/left
                    // texttop=(maxH-1-height)/2+1
                    tTop = (byte)(((20 - 1 - rowcount) / 2) + 1);
                    // textleft=(screenwidth-textwidth)/2
                    tLeft = (byte)((tMaxCol + 1 - NewMax) / 2);
                }
                else {
                    // use starting values
                    tTop = tStartRow;
                    tLeft = tStartCol;
                }
                isChanged = false;
                tErrLevel = 0;
                // validate position
                if (tLeft < 2) {
                    tErrLevel = 1;
                }
                if (tLeft + tWidth > (tMaxCol - 1)) {
                    tErrLevel += 2;
                }
                if (tTop < 1) {
                    tErrLevel += 4;
                }
                if (tTop + tHeight > 20) {
                    tErrLevel += 8;
                }
                // return formatted string
                return output.ToArray();
            }

            private byte[] CompileMsg(string msg) {
                List<byte> output = [];
                int intMode = 0;
                byte[] msginput;
                msginput = Encoding.GetEncoding(CodePage).GetBytes(msg);
                for (int i = 0; i < msginput.Length; i++) {
                    int intChar = msginput[i];
                    switch (intMode) {
                    case 0:
                        // normal
                        if (intChar == 92) {
                            intMode = 1;
                        }
                        else {
                            // add the char
                            output.Add((byte)intChar);
                        }
                        break;
                    case 1:
                        // back slash
                        intMode = 0;
                        switch (intChar) {
                        case 78 or 110:
                            //  \n
                            // add cr
                            output.Add(10);
                            break;
                        default:
                            // includes \\, \"
                            // add the char
                            output.Add((byte)intChar);

                            // TODO: need to add support for \x codes
                            // and others...
                            break;
                        }
                        break;
                    }
                }
                isChanged = false;
                tErrLevel = 0;
                if (tMode == PrintTestMode.Display) {
                    tTop = tStartRow;
                    tLeft = tStartRow;
                    // validate position
                    if (tTop < tPicOffset) {
                        tErrLevel |= 0x100;
                    }
                    if (tTop > 20 + tPicOffset) {
                        tErrLevel |= 0x200;
                    }
                    // only concerned about height if picoffset
                    // is less than 4 and ending row is greater
                    // than last row of pic image
                    if (tPicOffset < 4) {
                        // find end row
                        int endcol = tLeft, endrow = tTop;
                        for (int i = 0; i < output.Count; i++) {
                            if (output[i] == 10) {
                                // cr
                                endrow++;
                                endcol = 0;
                            }
                            else {
                                endcol++;
                                // check for overflow
                                if (endcol > tMaxCol) {
                                    endrow++;
                                    endcol = 0;
                                }
                            }
                        }
                        // now check end row
                        if (endrow > 20 + tPicOffset) {
                            tErrLevel |= 0x400;
                        }
                    }
                }
                return output.ToArray();
            }
        }
        #endregion

        #region Picture Editor Enums
        /// <summary>
        /// Enum to indicate the current mode of the picture editor.
        /// </summary>
        public enum PicEditorMode {
            Edit,
            ViewTest,
            PrintTest
        }

        /// <summary>
        /// Enum to indicate the current drawing operation.
        /// </summary>
        public enum PicDrawOp {
            None = 0,          // indicates no drawing op is in progress; mouse operations generally don't do anything
            Line = 1,          // lines being drawn; mouse ops set start/end points
            Fill = 2,          // fill or plot commands being drawn; mouse ops set starting point of fill operations
            Shape = 3,         // shape being drawn; mouse ops set bounds of the shape
            SelectArea = 4,    // to select an area of the graphic
            MoveCmds = 5,      // to move a set of commands
            MovePoint = 6,     // to move a single coordinate point
        }

        /// <summary>
        /// Enum that includes all valid AGI picture drawing function command codes.
        /// </summary>
        public enum DrawFunction {
            EnableVis = 0xf0,    // Change picture color and enable picture draw.
            DisableVis = 0xF1,   // Disable picture draw.
            EnablePri = 0xF2,    // Change priority color and enable priority draw.
            DisablePri = 0xF3,   // Disable priority draw.
            YCorner = 0xF4,      // Draw a Y corner.
            XCorner = 0xF5,      // Draw an X corner.
            AbsLine = 0xF6,      // Absolute line (long lines).
            RelLine = 0xF7,      // Relative line (short lines).
            Fill = 0xF8,         // Fill.
            ChangePen = 0xF9,    // Change pen size and style.
            PlotPen = 0xFA,      // Plot with pen.
            End = 0xFF           // end of drawing
        }

        /// <summary>
        /// Enum to indicate the current drawing tool.
        /// </summary>
        private enum PicToolType {
            Edit = 0,        // indicates edit tool is selected; mouse ops move coords and commands
            SetPen = 1,      // not used, but included for possible updated capabilities
            Line = 2,        // line drawing tool; mouse ops set start/end points
            ShortLine = 3,   // short line tool; mouse ops set start/end points
            StepLine = 4,    // corner line tool; mouse ops set start/end points
            Fill = 5,        // fill tool; mouse ops set starting point of fill operations
            Plot = 6,        // plot tool
            Rectangle = 7,   // for drawing rectangles
            Trapezoid = 8,   // for drawing trapezoids
            Ellipse = 9,     // for drawing ellipses
            SelectArea = 10, // for selecting bitmap areas of the Image
        }

        /// <summary>
        /// Enum to indicate the current style used for highlighting selected coordinates.
        /// </summary>
        public enum CoordinateHighlightType {
            FlashBox,
            XMode,
        }

        /// <summary>
        /// Enum to indicate the cursor currently in use. It tracks with the selected tool.
        /// </summary>
        private enum PicCursor {
            Edit,
            Cross,
            Move,
            NoOp,
            Default,
            Paint,
            Brush,
            Select,
            DragSurface,
            SelectImage,
        }

        /// <summary>
        /// Enum to indicate the display mode of the status bar.
        /// </summary>
        private enum PicStatusMode {
            Pixel,
            Coord,
            Text,
        }

        /// <summary>
        /// Enum to indicate which draw surface windows are currently active.
        /// </summary>
        private enum WindowMode {
            Visual = 0x1,
            Priority = 0x2,
            Both = 0x3,
        }

        /// <summary>
        /// Enum to indicate the type of print test being performed.
        /// </summary>
        public enum PrintTestMode {
            Print,
            PrintAt,
            Display,
        }
        #endregion

        #region Picture Editor Members
        public int PictureNumber;
        public Picture EditPicture;
        public bool InGame;
        public bool IsChanged;
        private bool closing = false;
        private PicEditorMode PicMode;
        internal EGAColors EditPalette = DefaultPalette.Clone();
        private Stack<PictureUndo> UndoCol = [];
        private bool priorityActive = false;

        // tool selection/manipulation/use
        private PicDrawOp PicDrawMode;
        private PicToolType SelectedTool;
        private Rectangle SelectedRegion = new(0, 0, 0, 0);
        private Point DragPT, AnchorPT, Delta;
        private Point PicPt; // the current mouse location in agi coordinates
        private Point EditPt = new(-1, -1), CoordPt = new(-1, -1);
        private bool Inserting = false; // used to indicate if a coordinate is being inserted
        private bool CancelContextMenu = false;
        private CommandInfo SelectedCmd;
        private int SelectedCmdCount;
        private DrawFunction EditCmd;
        private Point[] ArcPts;

        // graphics/display
        private WindowMode OneWindow;
        private bool TooSmall = false;
        private Color VCColor; // color of 'x's in 'x' cursor mode for visual
        private Color PCColor; // color of 'x's in 'x' cursor mode for priority
        private AGIColorIndex CursorColorIndex = AGIColorIndex.Black;
        public CoordinateHighlightType CursorMode;
        private bool OnPoint;
        private float CursorSize = 0;
        public float ScaleFactor;
        public bool ShowBands = false;
        public int OldPri;
        public bool ShowTextMarks = false;
        private const int PE_MARGIN = 5;
        public Bitmap BkgdImage;
        private PicCursor CurCursor;
        private bool blnDragging;
        private int dashdistance = 6;
        readonly Pen dash1 = new(Color.Black), dash2 = new(Color.White);

        // view testing
        private Engine.View TestView;
        private PicStatusMode StatusMode = PicStatusMode.Pixel;
        private int StopReason;
        private Point TestCelPos;
        private bool ShowTestCel;
        private ObjDirection TestDir;
        private byte TestViewNum = 0;
        private string TestViewFile;
        private byte CurTestLoop, CurTestCel, CurTestLoopCount;
        private byte[,] TestCelData;
        private byte CelHeight, CelWidth;
        private AGIColorIndex CelTrans;
        private PicTestInfo TestSettings = new();

        // text screen testing
        private bool ShowPrintTest = false;
        private PrintTestInfo PTInfo;
        private readonly Bitmap chargrid;

        // StatusStrip Items
        internal ToolStripStatusLabel spCurX;
        internal ToolStripStatusLabel spCurY;
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spMode;
        internal ToolStripStatusLabel spTool;
        internal ToolStripStatusLabel spAnchor;
        internal ToolStripStatusLabel spBlock;
        internal ToolStripStatusLabel spPriBand;
        internal ToolStripStatusLabel spStatus;
        #endregion

        /// <summary>
        /// Constructor for the picture editor form.
        /// </summary>
        public frmPicEdit() {
            InitializeComponent();

            Application.AddMessageFilter(this);
            Disposed += (sender, e) => Application.RemoveMessageFilter(this);
            InitFonts();

            MdiParent = MDIMain;
            tsbMode.DropDown.AutoSize = false;
            tsbMode.DropDown.Width = 34;
            tsbTool.DropDown.AutoSize = false;
            tsbTool.DropDown.Width = 34;
            tsbPlotStyle.DropDown.AutoSize = false;
            tsbPlotStyle.DropDown.Width = 34;
            tsbPlotSize.DropDown.AutoSize = false;
            tsbPlotSize.DropDown.Width = 34;
            tsbCircleSolid.Tag = 0x00;
            tsbSquareSolid.Tag = 0x10;
            tsbCircleSplat.Tag = 0x20;
            tsbSquareSplat.Tag = 0x30;
            tsbSize0.Tag = 0;
            tsbSize1.Tag = 1;
            tsbSize2.Tag = 2;
            tsbSize3.Tag = 3;
            tsbSize4.Tag = 4;
            tsbSize5.Tag = 5;
            tsbSize6.Tag = 6;
            tsbSize7.Tag = 7;

            // other initializations:
            picVisual.MouseWheel += picVisual_MouseWheel;
            picPriority.MouseWheel += picPriority_MouseWheel;

            dash1.DashPattern = [3, 3];
            dash2.DashPattern = [3, 3];
            dash2.DashOffset = 3;

            ScaleFactor = WinAGISettings.PicScaleEdit.Value;
            // validate
            if (ScaleFactor < 1) {
                ScaleFactor = 1;
            }
            else if (ScaleFactor <= 3) {
                ScaleFactor = (int)(ScaleFactor * 4) / 4f;
            }
            else if (ScaleFactor < 8) {
                ScaleFactor = (int)(ScaleFactor * 2) / 2f;
            }
            else if (ScaleFactor < 20) {
                ScaleFactor = (int)(ScaleFactor);
            }
            else {
                ScaleFactor = 20;
            }
            InitStatusStrip();

            picVisual.Width = picPriority.Width = (int)(ScaleFactor * 320);
            picVisual.Height = picPriority.Height = (int)(ScaleFactor * 168);
            ShowBands = WinAGISettings.ShowBands.Value;
            if (WinAGISettings.SplitWindow.Value) {
                OneWindow = WindowMode.Both;
                splitImages.SplitterDistance = splitImages.Height / 2;
            }
            else {
                OneWindow = WindowMode.Visual;
                splitImages.SplitterDistance = splitImages.Height - splitImages.SplitterWidth;
            }
            CursorMode = (CoordinateHighlightType)WinAGISettings.CursorMode.Value;
            PicMode = PicEditorMode.Edit;
            SelectedTool = PicToolType.Edit;

            // get default pic test settings
            TestSettings.ObjSpeed.ReadSetting(WinAGISettingsFile);
            if (TestSettings.ObjSpeed.Value < 0) {
                TestSettings.ObjSpeed.Value = 0;
            }
            else if (TestSettings.ObjSpeed.Value > 3) {
                TestSettings.ObjSpeed.Value = 3;
            }
            TestSettings.ObjPriority.ReadSetting(WinAGISettingsFile);
            if (TestSettings.ObjPriority.Value < 4) {
                TestSettings.ObjPriority.Value = 4;
            }
            else if (TestSettings.ObjPriority.Value > 16) {
                TestSettings.ObjPriority.Value = 16;
            }
            TestSettings.ObjRestriction.ReadSetting(WinAGISettingsFile);
            if (TestSettings.ObjRestriction.Value < 0) {
                TestSettings.ObjRestriction.Value = 0;
            }
            else if (TestSettings.ObjRestriction.Value > 2) {
                TestSettings.ObjRestriction.Value = 2;
            }
            TestSettings.Horizon.ReadSetting(WinAGISettingsFile);
            if (TestSettings.Horizon.Value < 0) {
                TestSettings.Horizon.Value = 0;
            }
            if (TestSettings.Horizon.Value > 167) {
                TestSettings.Horizon.Value = 167;
            }
            TestSettings.IgnoreHorizon.ReadSetting(WinAGISettingsFile);
            TestSettings.IgnoreBlocks.ReadSetting(WinAGISettingsFile);
            TestSettings.CycleAtRest.ReadSetting(WinAGISettingsFile);
            TestSettings.TestCel = -1;
            TestSettings.TestLoop = -1;
            // set timer based on speed
            switch (TestSettings.ObjSpeed.Value) {
            case 0:
                // slow
                tmrTest.Interval = 200;
                break;
            case 1:
                // normal
                tmrTest.Interval = 50;
                break;
            case 2:
                // fast
                tmrTest.Interval = 20;
                break;
            case 3:
                // fastest
                tmrTest.Interval = 1;
                break;
            }
            int codepage;
            if (InGame) {
                codepage = EditGame.CodePage;
            }
            else {
                codepage = WinAGISettings.DefCP.Value;
            }
            PTInfo = new(codepage);
            if (InGame && EditGame.PowerPack) {
                // default to 80 in powerpack mode
                PTInfo.MaxCol = 79;
                PTInfo.CharWidth = 4;
            }
            else {
                PTInfo.MaxCol = 39;
                PTInfo.CharWidth = 8;
            }

            // create a bitmap for the character grid
            byte[] obj = (byte[])EditorResources.ResourceManager.GetObject("CP" + codepage);
            Stream stream = new MemoryStream(obj);
            chargrid = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(chargrid)) {
                g.DrawImage((Bitmap)Image.FromStream(stream), 0, 0, 256, 256);
            }
        }

        protected override void OnClipboardChanged() {
            base.OnClipboardChanged();
            if (PicMode == PicEditorMode.Edit) {
                if (lstCommands.SelectedItems.Count == 0 ||
                    lstCoords.SelectedItems.Count == 0 ||
                    lstCommands.SelectedItems[0].Text[..3] == "Set") {
                    tsbPaste.Enabled = Clipboard.ContainsData(PICTURE_CB_FMT);
                }
                else {
                    tsbPaste.Enabled = false;
                }
            }
            else {
                tsbPaste.Enabled = false;
            }
        }

        #region Event Handlers
        #region Form Event Handlers
        /// <summary>
        /// Pre-filters the message to catch mouse wheel events.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool PreFilterMessage(ref Message m) {
            // in splitcontainers, mousewheel auto-scrolls the vertical scrollbar
            // only way to stop it is to catch the mousewheel message
            // and handle it manually
            const int WM_MOUSEWHEEL = 0x020A;
            if (m.Msg == WM_MOUSEWHEEL) {
                if (Control.FromHandle(m.HWnd) is Control control) {
                    if (control == picVisual) {
                        int fwKeys = (int)m.WParam & 0xffff;
                        int zDelta = (int)((int)m.WParam & 0xffff0000) >> 16;
                        int xPos = (int)m.LParam & 0xffff;
                        int yPos = (int)((int)m.LParam & 0xffff0000) >> 16;
                        picVisual_MouseWheel(control, new MouseEventArgs(MouseButtons.None, 0, xPos, yPos, zDelta));
                    }
                    else if (control == picPriority) {
                        int fwKeys = (int)m.WParam & 0xffff;
                        int zDelta = (int)((int)m.WParam & 0xffff0000);
                        int xPos = (int)m.LParam & 0xffff;
                        int yPos = (int)((int)m.LParam & 0xffff0000);
                        picPriority_MouseWheel(control, new MouseEventArgs(MouseButtons.None, 0, xPos, yPos, zDelta));
                    }
                    else {
                        return false;
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        private void frmPicEdit_FormClosing(object sender, FormClosingEventArgs e) {
            // if the form is closing because the MDI parent is closing, don't ask to close
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmPicEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // update background settings
            if (InGame) {
                // copy properties back to actual picture resource
                EditGame.Pictures[PictureNumber].BackgroundSettings = EditPicture.BackgroundSettings;
                EditGame.Pictures[PictureNumber].SaveProps();
            }
            // dereference picture
            EditPicture?.Unload();
            EditPicture = null;
            // remove from PicEditor collection
            foreach (frmPicEdit frm in PictureEditors) {
                if (frm == this) {
                    PictureEditors.Remove(frm);
                    break;
                }
            }

            // if a test view is currently loaded,
            if (TestView is not null) {
                // unload it and release it
                TestView.Unload();
                TestView = null;
            }

            // dispose of graphics objects
            BkgdImage?.Dispose();
            dash1?.Dispose();
            dash2?.Dispose();
            chargrid?.Dispose();
        }

        private void frmPicEdit_Resize(object sender, EventArgs e) {
            if (picPalette.Visible) {
                picPalette.Refresh();
            }
            if (Height < 220) {
                // if the form is too small, collapse the split windows
                if (!TooSmall) {
                    TooSmall = true;
                    // force to just one window- whichever is larger (or visual, if in
                    // printtest mode)
                    if (splitImages.Panel1.Height >= splitImages.Panel2.Height ||
                        PicMode == PicEditorMode.PrintTest) {
                        splitImages.Panel2Collapsed = true;
                    }
                    else {
                        splitImages.Panel1Collapsed = true;
                    }
                }
            }
            else {
                // if the form is large enough, expand the split windows
                if (TooSmall) {
                    TooSmall = false;
                    splitImages.Panel1Collapsed = false;
                    splitImages.Panel2Collapsed = false;
                }
            }
        }

        private void frmPicEdit_KeyDown(object sender, KeyEventArgs e) {
            if (e.Alt && e.KeyCode == Keys.S) {
                mnuToggleScreen.PerformClick();
                e.Handled = true;
            }
            switch (PicMode) {
            case PicEditorMode.Edit:
                switch (e.KeyData) {
                case Keys.Escape:
                    // if a coord is selected, unselect it
                    if (lstCoords.SelectedItems.Count != 0) {
                        SelectCommand(SelectedCmd.Index, 1, true);
                    }
                    break;
                case Keys.Space:
                    if (ShowTextMarks) {
                        // toggle status mode  between pixel and text row/col
                        if (StatusMode == PicStatusMode.Pixel) {
                            StatusMode = PicStatusMode.Text;
                            if (picVisual.ClientRectangle.Contains(picVisual.PointToClient(Control.MousePosition)) ||
                                picPriority.ClientRectangle.Contains(picPriority.PointToClient(Control.MousePosition))) {
                                // text row/col (note row/col swap 'x/y')
                                spCurX.Text = "R: " + (PicPt.Y / 8).ToString();
                                spCurY.Text = "C: " + (PicPt.X / (PTInfo.CharWidth / 2)).ToString();
                            }
                        }
                        else {
                            StatusMode = PicStatusMode.Pixel;
                            if (picVisual.ClientRectangle.Contains(picVisual.PointToClient(Control.MousePosition)) ||
                                picPriority.ClientRectangle.Contains(picPriority.PointToClient(Control.MousePosition))) {
                                spCurX.Text = "X: " + PicPt.X.ToString();
                                spCurY.Text = "Y: " + PicPt.Y.ToString();
                            }
                        }
                    }
                    break;
                }
                break;
            case PicEditorMode.ViewTest:
                switch (e.KeyData) {
                case Keys.Escape:
                    // hide the test cel
                    if (ShowTestCel) {
                        ShowTestCel = false;
                        picVisual.Refresh();
                        picPriority.Refresh();
                    }
                    break;
                case Keys.Space:
                    // toggle status mode between edit(pixel) and test location(coord)
                    if (StatusMode == PicStatusMode.Pixel) {
                        StatusMode = PicStatusMode.Coord;
                        spCurX.Text = "vX: " + TestCelPos.X;
                        spCurY.Text = "vY: " + TestCelPos.Y;
                        int CelPriority;
                        // set priority (if in auto, get priority from current band)
                        if (TestSettings.ObjPriority.Value < 16) {
                            CelPriority = TestSettings.ObjPriority.Value;
                        }
                        else {
                            CelPriority = GetPriBand((byte)TestCelPos.Y, EditPicture.PriBase);
                        }
                        spPriBand.Text = "vBand: " + CelPriority;
                        Bitmap bitmap = new(12, 12);
                        using Graphics sg = Graphics.FromImage(bitmap);
                        sg.Clear(EditPalette[CelPriority]);
                        spPriBand.Image = bitmap;
                    }
                    else {
                        StatusMode = PicStatusMode.Pixel;
                        if (picVisual.ClientRectangle.Contains(picVisual.PointToClient(Control.MousePosition)) ||
                            picPriority.ClientRectangle.Contains(picPriority.PointToClient(Control.MousePosition))) {
                            spCurX.Text = "X: " + PicPt.X.ToString();
                            spCurY.Text = "Y: " + PicPt.Y.ToString();
                            int NewPri = GetPriBand((byte)PicPt.Y, EditPicture.PriBase);
                            spPriBand.Text = "Band: " + NewPri;
                            Bitmap bitmap = new(12, 12);
                            using Graphics g = Graphics.FromImage(bitmap);
                            g.Clear(EditPalette[NewPri]);
                            spPriBand.Image = bitmap;
                        }
                        else {
                            spCurX.Text = "";
                            spCurY.Text = "";
                            spPriBand.Text = "";
                            spPriBand.Image = null;
                            OldPri = -1;
                        }
                    }
                    break;
                default:
                    // in test mode, handle key events to move the test cel
                    if (ChangeDir(e.KeyData)) {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;
                }
                break;
            case PicEditorMode.PrintTest:
                switch (e.KeyData) {
                case Keys.Escape:
                    // hide the test display text
                    if (ShowPrintTest) {
                        ShowPrintTest = false;
                        picVisual.Refresh();
                    }
                    break;
                case Keys.Space:
                    if (ShowTextMarks) {
                        // toggle status mode  between pixel and text row/col
                        if (StatusMode == PicStatusMode.Pixel) {
                            StatusMode = PicStatusMode.Text;
                            if (picVisual.ClientRectangle.Contains(picVisual.PointToClient(Control.MousePosition)) ||
                                picPriority.ClientRectangle.Contains(picPriority.PointToClient(Control.MousePosition))) {
                                // text row/col (note row/col swap 'x/y')
                                spCurX.Text = "R: " + (PicPt.Y / 8).ToString();
                                spCurY.Text = "C: " + (PicPt.X / (PTInfo.CharWidth / 2)).ToString();
                            }
                        }
                        else {
                            StatusMode = PicStatusMode.Pixel;
                            if (picVisual.ClientRectangle.Contains(picVisual.PointToClient(Control.MousePosition)) ||
                                picPriority.ClientRectangle.Contains(picPriority.PointToClient(Control.MousePosition))) {
                                spCurX.Text = "X: " + PicPt.X.ToString();
                                spCurY.Text = "Y: " + PicPt.Y.ToString();
                            }
                        }
                    }
                    break;
                }
                break;
            }
        }
        #endregion

        #region Menu Event Handlers
        /// <summary>
        /// Configures the resource menu prior to displaying it.
        /// </summary>
        internal void SetResourceMenu() {

            mnuRSave.Enabled = IsChanged;
            MDIMain.mnuRSep2.Visible = true;
            MDIMain.mnuRSep3.Visible = true;
            if (EditGame is null) {
                // no game is open
                MDIMain.mnuRImport.Enabled = false;
                mnuRExport.Text = "Save As ...";
                mnuRInGame.Enabled = false;
                mnuRInGame.Text = "Add Picture to Game";
                mnuRRenumber.Enabled = false;
                // mnuRProperties no change
                // mnuRSavePicImage no change
                // mnuRExportGIF no change
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export Picture" : "Save As ...";
                mnuRInGame.Enabled = true;
                mnuRInGame.Text = InGame ? "Remove from Game" : "Add to Game";
                mnuRRenumber.Enabled = InGame;
                // mnuRProperties no change
                // mnuRSavePicImage no change
                // mnuRExportGIF no change
            }
        }

        /// <summary>
        /// Resets all resource menu items so shortcut keys can work correctly.
        /// </summary>
        internal void ResetResourceMenu() {
            // always reenable all items so shortcuts work
            mnuRSave.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRInGame.Enabled = true;
            mnuRRenumber.Enabled = true;
            mnuRProperties.Enabled = true;
            mnuRSavePicImage.Enabled = true;
            mnuRExportGIF.Enabled = true;
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            if (IsChanged) {
                SavePicture();
            }
        }

        internal void mnuRExport_Click(object sender, EventArgs e) {
            ExportPicture();
        }

        internal void mnuRInGame_Click(object sender, EventArgs e) {
            if (EditGame is not null) {
                ToggleInGame();
            }
        }

        internal void mnuRRenumber_Click(object sender, EventArgs e) {
            if (InGame) {
                RenumberPicture();
            }
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            EditPictureProperties(1);
        }

        private void mnuRSavePicImage_Click(object sender, EventArgs e) {
            ExportOnePicImg(EditPicture);
        }

        private void mnuRExportGIF_Click(object sender, EventArgs e) {
            ExportPicAsGif(EditPicture);
        }

        private void cmEdit_Opening(object sender, CancelEventArgs e) {
            if (CancelContextMenu) {
                e.Cancel = true;
                CancelContextMenu = false;
                return;
            }
            SetEditMenu();
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            // move edit menu items  to the form's edit menu
            mnuEdit.DropDownItems.AddRange([mnuUndo, mnuESep0, mnuCut, mnuCopy,
                mnuPaste, mnuPastePen, mnuDelete, mnuClearPicture, mnuSelectAll, mnuESep1,
                mnuInsertCoord, mnuSplitCommand, mnuJoinCommands, mnuFlipH, mnuFlipV, mnuESep2,
                mnuEditMode, mnuViewTestMode, mnuTextTestMode, mnuESep3,
                mnuSetTestView, mnuTestViewOptions, mnuTestTextOptions,
                mnuTextScreenSize, mnuESep4,mnuToggleScreen, mnuToggleBands,
                mnuEditPriBase,mnuToggleTextMarks, mnuESep5, mnuToggleBackground,
                mnuEditBackground, mnuRemoveBackground]);
            SetEditMenu();
        }

        private void SetEditMenu() {
            // enable/disable edit menu items before displaying
            mnuEditMode.Checked = PicMode == PicEditorMode.Edit;
            mnuESep3.Visible = !(PicMode == PicEditorMode.Edit);
            mnuViewTestMode.Checked = PicMode == PicEditorMode.ViewTest;
            mnuTextTestMode.Checked = PicMode == PicEditorMode.PrintTest;
            mnuSetTestView.Visible =
                mnuTestViewOptions.Visible =
                PicMode == PicEditorMode.ViewTest;
            mnuTestTextOptions.Visible =
                mnuTextScreenSize.Visible =
                PicMode == PicEditorMode.PrintTest;
            mnuTextScreenSize.Visible = PicMode == PicEditorMode.PrintTest && InGame && EditGame.PowerPack;
            if (mnuTextScreenSize.Visible) {
                mnuTextScreenSize.Text = "Text Screen Size: " + (PTInfo.MaxCol + 1).ToString();
            }
            // background and overlays
            if (BkgdImage is null) {
                mnuEditBackground.Text = "Set Background Image...";
                mnuToggleBackground.Visible = false;
                mnuRemoveBackground.Visible = false;
            }
            else {
                mnuEditBackground.Text = "Background Settings...";
                mnuToggleBackground.Visible = true;
                if (EditPicture.BkgdVisible) {
                    mnuToggleBackground.Text = "Hide Background";
                }
                else {
                    mnuToggleBackground.Text = "Show Background";
                }
                mnuRemoveBackground.Visible = true;
            }
            if (ShowBands) {
                mnuToggleBands.Text = "Hide Priority Bands";
            }
            else {
                mnuToggleBands.Text = "Show Priority Bands";
            }
            mnuEditPriBase.Visible = !InGame || Array.IndexOf(IntVersions, EditGame.InterpreterVersion) >= 13;
            if (ShowTextMarks) {
                mnuToggleTextMarks.Text = "Hide Text Marks";
            }
            else {
                mnuToggleTextMarks.Text = "Show Text Marks";
            }
            // screen toggle
            if (TooSmall) {
                if (splitImages.Panel1Collapsed) {
                    mnuToggleScreen.Text = "Show Priority Screen";
                }
                else {
                    mnuToggleScreen.Text = "Show Visual Screen";
                }
            }
            else {
                if (OneWindow != WindowMode.Both) {
                    mnuToggleScreen.Visible = true;
                    if (OneWindow == WindowMode.Visual) {
                        mnuToggleScreen.Text = "Show Priority Screen";
                    }
                    else {
                        mnuToggleScreen.Text = "Show Visual Screen";
                    }
                }
                else {
                    mnuToggleScreen.Visible = false;
                }
            }
            mnuToggleScreen.Enabled = PicMode != PicEditorMode.PrintTest;

            // mode dependent items
            switch (PicMode) {
            case PicEditorMode.PrintTest or PicEditorMode.ViewTest:
                // disable undo, cut, copy, paste, select all
                mnuUndo.Enabled = false;
                mnuUndo.Text = "Undo";
                mnuCut.Enabled = false;
                mnuCut.Text = "Cut";
                mnuCopy.Enabled = false;
                mnuCopy.Text = "&Copy";
                mnuPaste.Enabled = mnuPastePen.Visible = false;
                mnuPaste.Text = "Paste";
                mnuClearPicture.Visible = false;
                mnuSelectAll.Enabled = false;
                mnuSelectAll.Text = "Select All";
                mnuDelete.Visible = false;
                mnuInsertCoord.Visible = false;
                mnuSplitCommand.Visible = false;
                mnuJoinCommands.Visible = false;
                mnuFlipH.Visible = false;
                mnuFlipV.Visible = false;
                mnuESep2.Visible = false;
                break;
            case PicEditorMode.Edit:
                if (UndoCol.Count != 0) {
                    mnuUndo.Enabled = true;
                    mnuUndo.Text = "Undo " + Editor.Base.LoadResString(PICUNDOTEXT + (int)UndoCol.Peek().Action);
                    // some commands need 's' added to end if more than one command to undo
                    switch (UndoCol.Peek().Action) {
                    case DelCmd or AddCmd or CutCmds or PasteCmds or MoveCmds or FlipH or FlipV:
                        if (UndoCol.Peek().CoordIndex > 1) {
                            mnuUndo.Text += "s";
                        }
                        break;
                    }
                }
                else {
                    mnuUndo.Enabled = false;
                    mnuUndo.Text = "Undo";
                }
                mnuDelete.Visible = true;
                mnuInsertCoord.Visible = true;
                if (SelectedTool == PicToolType.SelectArea) {
                    // area selection - no editing commands are enabled,
                    // only copy is available
                    mnuCut.Enabled = false;
                    mnuCut.Text = "Cut";
                    mnuPaste.Enabled = mnuPastePen.Visible = false;
                    mnuPaste.Text = "Paste";
                    mnuDelete.Enabled = false;
                    mnuDelete.Text = "Delete";
                    mnuInsertCoord.Enabled = false;
                    mnuInsertCoord.Text = "Insert Coordinate";
                    mnuSplitCommand.Visible = false;
                    mnuJoinCommands.Visible = false;
                    mnuFlipH.Visible = false;
                    mnuFlipV.Visible = false;
                    // copy is enabled if something selected
                    mnuCopy.Enabled = (SelectedRegion.Width > 0) && (SelectedRegion.Height > 0);
                    mnuCopy.Text = "Copy Selection";
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 ||
                    SelectedCmd.IsPen) {
                    // no coordinate is selected - set editing commands to 
                    // handle the selected drawing commands
                    mnuCut.Enabled = SelectedCmd.Type != End;
                    mnuCut.Text = "Cut Command";
                    mnuCopy.Enabled = mnuCut.Enabled;
                    mnuCopy.Text = "Copy Command";
                    mnuDelete.Enabled = mnuCut.Enabled;
                    mnuDelete.Text = "Delete Command";
                    if (SelectedCmdCount > 1) {
                        mnuCut.Text += "s";
                        mnuCopy.Text += "s";
                        mnuDelete.Text += "s";
                    }
                    mnuPaste.Enabled = mnuPastePen.Visible = Clipboard.ContainsData(PICTURE_CB_FMT);
                    mnuPaste.Text = "Paste";
                    if (mnuPaste.Enabled) {
                        PictureClipboardData pastedata = Clipboard.GetData(PICTURE_CB_FMT) as PictureClipboardData;
                        if (pastedata is null) {
                            mnuPaste.Enabled = mnuPastePen.Visible = false;
                        }
                        else if (pastedata.CmdCount > 1) {
                            mnuPaste.Text += " Commands";
                        }
                        else {
                            mnuPaste.Text += " " + ((DrawFunction)pastedata.Data[0]).CommandName();
                        }
                    }
                    if (SelectedTool == PicToolType.Edit && SelectedCmd.Type == PlotPen || SelectedCmd.Type == Fill) {
                        mnuInsertCoord.Enabled = true;
                    }
                    else if ((SelectedTool == PicToolType.Plot && SelectedCmd.Type == PlotPen) ||
                             (SelectedTool == PicToolType.Fill && SelectedCmd.Type == Fill)) {
                        mnuInsertCoord.Enabled = true;
                    }
                    else {
                        mnuInsertCoord.Enabled = false;
                    }
                    mnuInsertCoord.Text = "Insert Coordinate";
                    mnuSplitCommand.Visible = false;
                    mnuJoinCommands.Visible = CanJoinCommands(SelectedCmd.Index);
                    mnuFlipH.Visible = (SelectedRegion.Width > 1);
                    mnuFlipV.Visible = (SelectedRegion.Height > 1);
                    mnuESep2.Visible = true;
                }
                else {
                    // a coordinate is selected, set editing commands to
                    // handle coordinate editing
                    mnuCut.Enabled = false;
                    mnuCut.Text = "Cut";
                    mnuCopy.Enabled = false;
                    mnuCopy.Text = "Copy";
                    // insert always available for absline, relline, fill, plot
                    // delete always available for absline, fill, plot
                    // insert/delete only available for other commands if on last coord
                    mnuDelete.Text = "Delete Coordinate";
                    switch (SelectedCmd.Type) {
                    case AbsLine:
                    case Fill:
                    case PlotPen:
                        mnuDelete.Enabled = true;
                        mnuInsertCoord.Enabled = true;
                        break;
                    default:
                        // Corner lines or relative lines
                        mnuDelete.Enabled = mnuInsertCoord.Enabled = SelectedCmd.SelectedCoordIndex == 0 || SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1;
                        break;
                    }
                    // disable split if not just one cmd selected OR
                    // cmd is set color pen or set plot pen OR
                    // only one coordinate OR
                    // no coord selected
                    if (SelectedCmdCount != 1 ||
                        SelectedCmd.IsPen ||
                        SelectedCmd.SelectedCoordIndex < 0) {
                        mnuSplitCommand.Visible = false;
                    }
                    else {
                        // if on a line, fill, or plot cmd
                        switch (SelectedCmd.Type) {
                        case AbsLine or RelLine or XCorner or YCorner:
                            // only if three or more, and not on either end
                            if (SelectedCmd.Coords.Count < 3 || SelectedCmd.SelectedCoordIndex == 0 || SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1) {
                                mnuSplitCommand.Visible = false;
                            }
                            else {
                                mnuSplitCommand.Visible = true;
                            }
                            break;
                        case Fill or PlotPen:
                            // only if not on first coordinate
                            mnuSplitCommand.Visible = SelectedCmd.SelectedCoordIndex != 0;
                            break;
                        }
                    }
                    mnuJoinCommands.Visible = false;
                    mnuFlipH.Visible = false;
                    mnuFlipV.Visible = false;
                }
                mnuClearPicture.Visible = true;
                mnuClearPicture.Enabled = lstCommands.Items.Count > 1;
                mnuSelectAll.Enabled = lstCommands.Items.Count > 1;
                break;
            }
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            // move the edit menu items back to the context menu
            cmEdit.Items.AddRange([mnuUndo, mnuESep0, mnuCut, mnuCopy,
                mnuPaste, mnuPastePen, mnuDelete, mnuClearPicture, mnuSelectAll, mnuESep1,
                mnuInsertCoord, mnuSplitCommand, mnuJoinCommands, mnuFlipH, mnuFlipV, mnuESep2,
                mnuEditMode, mnuViewTestMode, mnuTextTestMode, mnuESep3,
                mnuSetTestView, mnuTestViewOptions, mnuTestTextOptions,
                mnuTextScreenSize, mnuESep4, mnuToggleScreen, mnuToggleBands,
                mnuEditPriBase, mnuToggleTextMarks, mnuESep5, mnuToggleBackground,
                mnuEditBackground, mnuRemoveBackground]);
            ResetEditMenu();
        }

        private void ResetEditMenu() {
            // always reenable all items so shortcuts work
            foreach (ToolStripItem itm in cmEdit.Items) {
                itm.Enabled = true;
            }
        }

        private void mnuEditMode_Click(object sender, EventArgs e) {

            if (PicMode != PicEditorMode.Edit) {
                SetMode(PicEditorMode.Edit);
            }
        }

        private void mnuViewTestMode_Click(object sender, EventArgs e) {
            if (PicMode != PicEditorMode.ViewTest) {
                SetMode(PicEditorMode.ViewTest);
            }
        }

        private void mnuTextTestMode_Click(object sender, EventArgs e) {
            if (PicMode != PicEditorMode.PrintTest) {
                SetMode(PicEditorMode.PrintTest);
            }
        }

        private void mnuUndo_Click(object sender, EventArgs e) {
            if (CanUndo()) {
                PictureUndo NextUndo = UndoCol.Pop();
                // always cancel draw operations and shift to
                // edit tool when undoing
                if (PicDrawMode != PicDrawOp.None) {
                    PicDrawMode = PicDrawOp.None;
                }
                if (SelectedTool != PicToolType.Edit) {
                    SelectTool(PicToolType.Edit);
                }
                // undo the action
                switch (NextUndo.Action) {
                case ChangeColor:
                    ChangePenColor(NextUndo.CmdIndex, (AGIColorIndex)NextUndo.Data[0], true);
                    SelectCommand(NextUndo.CmdIndex, 1, true);
                    break;
                case ChangePlotPen:
                    ChangePenSettings(NextUndo.CmdIndex, NextUndo.Data[0], true);
                    // check for any pattern changes
                    UndoPlotAdjust();
                    // force update
                    SelectCommand(NextUndo.CmdIndex, 1, true);
                    break;
                case DelCmd or CutCmds:
                    // insert the data
                    EditPicture.InsertData(NextUndo.Data, NextUndo.PicPos);
                    int insertindex = NextUndo.CmdIndex - NextUndo.CmdCount + 1;
                    // adjust positions
                    UpdatePosValues(insertindex, NextUndo.Data.Length);
                    // update command list
                    bool blnSetPen = false;
                    for (int i = 0; i < NextUndo.Data.Length; i++) {
                        if (NextUndo.Data[i] >= 0xF0) {
                            lstCommands.Items.Insert(insertindex++, ((DrawFunction)NextUndo.Data[i]).CommandName()).Tag = NextUndo.PicPos + i;
                            CmdColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                            if (NextUndo.Data[i] == (byte)ChangePen) {
                                blnSetPen = true;
                            }
                        }
                    }
                    if (blnSetPen) {
                        UndoPlotAdjust();
                    }
                    // now select the added commands
                    SelectCommand(NextUndo.CmdIndex, NextUndo.CmdCount, true);
                    break;
                case Clear:
                    // insert the data
                    EditPicture.InsertData(NextUndo.Data, NextUndo.PicPos);
                    // build command list
                    insertindex = 0;
                    for (int i = 0; i < NextUndo.Data.Length; i++) {
                        if (NextUndo.Data[i] >= 0xF0) {
                            lstCommands.Items.Insert(insertindex++, ((DrawFunction)NextUndo.Data[i]).CommandName()).Tag = NextUndo.PicPos + i;
                        }
                    }
                    // update and select End cmd
                    lstCommands.Items[^1].Tag = EditPicture.Data.Length - 1;
                    CmdColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    SelectCommand(insertindex, 1, true);
                    break;
                case PasteCmds:
                    // remove the pasted data from the resource
                    EditPicture.RemoveData(NextUndo.PicPos, NextUndo.ByteCount);
                    // remove the cmds from the cmd list
                    for (int i = 0; i < NextUndo.CmdCount; i++) {
                        lstCommands.Items.RemoveAt(NextUndo.CmdIndex);
                    }
                    UpdatePosValues(NextUndo.CmdIndex, -NextUndo.ByteCount);
                    UndoPlotAdjust();
                    // select the cmd after the deleted data
                    SelectCommand(NextUndo.CmdIndex, 1, true);
                    break;
                case AddCmd or PictureUndo.ActionType.Rectangle or Trapezoid:
                    // delete the command
                    DeleteCommands(NextUndo.CmdIndex, 1, true);
                    if (NextUndo.DrawCommand == ChangePen) {
                        // check for any pattern changes
                        UndoPlotAdjust();
                    }
                    // force update
                    SelectCommand(NextUndo.CmdIndex, 1, true);
                    break;
                case Ellipse:
                    // delete this command, and next three commands
                    DeleteCommands(NextUndo.CmdIndex + 3, 4, true);
                    // force update
                    SelectCommand(NextUndo.CmdIndex, 1, false);
                    break;
                case AddCoord:
                    // select cmd first (without redrawing)
                    SelectedCmd = GetCommand(NextUndo.CmdIndex);
                    if (SelectedCmd.Type == XCorner || SelectedCmd.Type == YCorner) {
                        if (NextUndo.CoordIndex != SelectedCmd.Coords.Count - 1) {
                            // need to flip the type
                            if (SelectedCmd.Type == XCorner) {
                                lstCommands.Items[SelectedCmd.Index].Text = YCorner.CommandName();
                            }
                            else {
                                lstCommands.Items[SelectedCmd.Index].Text = XCorner.CommandName();
                            }
                            // delete first coord instead of coord 1
                            NextUndo.CoordIndex--;
                        }
                    }
                    // delete the coordinate
                    DeleteCoordinate(NextUndo.CoordIndex, true);
                    // force update
                    SelectCommand(NextUndo.CmdIndex, 1, true);
                    break;
                case DelCoord:
                    // select the cmd
                    SelectCommand(NextUndo.CmdIndex, 1, false);
                    // reinsert the data
                    EditPicture.InsertData(NextUndo.Data, NextUndo.PicPos);
                    if (NextUndo.CoordIndex == 0) {
                        switch (NextUndo.DrawCommand) {
                        case RelLine:
                            // need to restore Y0 and D1
                            // reminder: X0 is NextUndo.Coord.X
                            //           Y0 is NextUndo.Coord.Y
                            //           X1 is at data offset[1]
                            //           Y1 is at data offset[2]
                            int d1 = 0;
                            if (EditPicture.Data[NextUndo.PicPos + 1] - NextUndo.Coord.X < 0) {
                                d1 = 0x80;
                            }
                            d1 += Math.Abs(EditPicture.Data[NextUndo.PicPos + 1] - NextUndo.Coord.X) * 0x10;
                            if (EditPicture.Data[NextUndo.PicPos + 2] - NextUndo.Coord.Y < 0) {
                                d1 += 0x08;
                            }
                            d1 += Math.Abs(EditPicture.Data[NextUndo.PicPos + 2] - NextUndo.Coord.Y);
                            EditPicture.Data[NextUndo.PicPos + 1] = (byte)NextUndo.Coord.Y;
                            EditPicture.Data[NextUndo.PicPos + 2] = (byte)d1;
                            break;
                        case XCorner:
                            // need to swap Y0 and X1 back and change type to XCorner
                            EditPicture.Data[NextUndo.PicPos - 1] = (byte)XCorner;
                            EditPicture.Data[NextUndo.PicPos + 2] = EditPicture.Data[NextUndo.PicPos + 1];
                            EditPicture.Data[NextUndo.PicPos + 1] = (byte)NextUndo.Coord.Y;
                            SelectedCmd.Type = XCorner;
                            lstCommands.Items[SelectedCmd.Index].Text = XCorner.CommandName();
                            break;
                        case YCorner:
                            // need to change type to YCorner (remember PicPos is offset by 1)
                            SelectedCmd.Type = YCorner;
                            EditPicture.Data[NextUndo.PicPos - 2] = (byte)YCorner;
                            lstCommands.Items[SelectedCmd.Index].Text = YCorner.CommandName();
                            break;
                        }
                    }
                    UpdatePosValues(SelectedCmd.Index + 1, NextUndo.Data.Length);
                    SelectedCmd.Coords.Insert(NextUndo.CoordIndex, NextUndo.Coord);
                    lstCoords.Items.Insert(NextUndo.CoordIndex, CoordText(NextUndo.Coord));
                    CoordColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

                    // force reselection
                    SelectCoordinate(NextUndo.CoordIndex, true);
                    break;
                case MoveCoord:
                case InsertCoord:
                    // restore pic data
                    for (int i = 0; i < NextUndo.Data.Length; i++) {
                        EditPicture.Data[NextUndo.PicPos + i] = NextUndo.Data[i];
                    }
                    // if undoing an insert, also delete current coordinate
                    if (NextUndo.Action == InsertCoord) {
                        // select cmd first (without redrawing)
                        SelectedCmd = GetCommand(NextUndo.CmdIndex);
                        DeleteCoordinate(NextUndo.CoordIndex, true);
                        // force update
                        SelectCommand(NextUndo.CmdIndex, 1, true);
                    }
                    else {
                        // force update
                        SelectCommand(NextUndo.CmdIndex, 1, true);
                        SelectCoordinate(NextUndo.CoordIndex);
                    }
                    lstCoords.Focus();
                    break;
                case EditCoord:
                    for (int i = 0; i < NextUndo.Data.Length; i++) {
                        EditPicture.Data[NextUndo.PicPos + i] = NextUndo.Data[i];
                    }
                    SelectCommand(NextUndo.CmdIndex, 1, true);
                    SelectCoordinate(NextUndo.CoordIndex);
                    break;
                case SplitCmd:
                    // select the cmd
                    SelectCommand(NextUndo.CmdIndex);
                    // now rejoin the commands
                    JoinCommands(true);
                    break;
                case JoinCmds:
                    // select the cmd and coord
                    SelectCommand(NextUndo.CmdIndex);
                    SelectedCmd.SelectedCoordIndex = NextUndo.CoordIndex;
                    // now split it
                    SplitCommand(true);
                    // check for (rare) case where X/YCorner type changed
                    if (NextUndo.DrawCommand == XCorner || NextUndo.DrawCommand == YCorner) {
                        // force the previous command to match
                        EditPicture.Data[NextUndo.PicPos] = (byte)NextUndo.DrawCommand;
                        lstCommands.Items[NextUndo.CmdIndex].Text = NextUndo.DrawCommand.CommandName();
                    }
                    if (NextUndo.Data.Length > 0) {
                        // need to repair a split x/ycorner that had an extended line joined
                        EditPicture.Data[NextUndo.PicPos] = NextUndo.Data[0];
                        if (SelectedCmd.Type == XCorner) {
                            EditPicture.Data[SelectedCmd.Position] = (byte)YCorner;
                            lstCommands.Items[SelectedCmd.Index].Text = YCorner.CommandName();
                            EditPicture.InsertData(NextUndo.Data[0], NextUndo.PicPos + 3);
                            UpdatePosValues(SelectedCmd.Index + 1, 1);
                        }
                        else {
                            EditPicture.Data[SelectedCmd.Position] = (byte)XCorner;
                            lstCommands.Items[SelectedCmd.Index].Text = XCorner.CommandName();
                            EditPicture.InsertData(EditPicture.Data[SelectedCmd.Position + 1], NextUndo.PicPos + 4);
                            EditPicture.Data[SelectedCmd.Position + 1] = NextUndo.Data[0];
                            UpdatePosValues(SelectedCmd.Index + 1, 1);
                        }
                        // force selection
                        SelectCommand(SelectedCmd.Index, 1, true);
                    }
                    break;
                case MoveCmds:
                    SelectCommand(NextUndo.CmdIndex, NextUndo.CoordIndex);
                    byte dX = NextUndo.Data[0];
                    byte dY = NextUndo.Data[1];

                    // move cmds back
                    MoveCommands(NextUndo.CmdIndex, NextUndo.CoordIndex, -dX, -dY, true);

                    SelectCommand(NextUndo.CmdIndex, NextUndo.CoordIndex, true);
                    if (NextUndo.CoordIndex == 1) {
                        ClearSelectionBounds();
                    }
                    break;
                case FlipH:
                    FlipHorizontal(NextUndo.CmdIndex, NextUndo.CmdCount, true);
                    // force re-selection
                    SelectCommand(SelectedCmd.Index, SelectedCmdCount, true);
                    SetSelectionBounds(true);
                    picVisual.Refresh();
                    picPriority.Refresh();
                    break;
                case FlipV:
                    FlipVertical(NextUndo.CmdIndex, NextUndo.CmdCount, true);
                    // force re-selection
                    SelectCommand(SelectedCmd.Index, SelectedCmdCount, true);
                    SetSelectionBounds(true);
                    picVisual.Refresh();
                    picPriority.Refresh();
                    break;
                case SetPriBase:
                    // change pribase back
                    EditPicture.PriBase = (byte)NextUndo.CmdIndex;
                    // if showing priority lines
                    if (ShowBands) {
                        switch (OneWindow) {
                        case 0:
                            picVisual.Invalidate();
                            picPriority.Invalidate();
                            break;
                        case WindowMode.Visual:
                            picVisual.Invalidate();
                            break;
                        case WindowMode.Priority:
                            picPriority.Invalidate();
                            break;
                        }
                    }
                    break;
                }
                MarkAsChanged();
            }
        }

        private bool CanUndo() {
            if (PicMode == PicEditorMode.Edit) {
                return UndoCol.Count != 0;
            }
            else {
                return false;
            }
        }

        private void mnuCut_Click(object sender, EventArgs e) {
            if (CanCut()) {
                int oldundocount = UndoCol.Count;
                // copy, then delete
                mnuCopy_Click(sender, e);
                mnuDelete_Click(sender, e);
                // update undo only if something deleted
                if (UndoCol.Count > oldundocount) {
                    UndoCol.Peek().Action = CutCmds;
                }
            }
        }

        private bool CanCut() {
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    return false;
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 || SelectedCmd.IsPen) {
                    return SelectedCmd.Type != End;
                }
                else {
                    return false;
                }
            }
            else {
                return false;
            }
        }

        private void mnuCopy_Click(object sender, EventArgs e) {
            if (CanCopy()) {
                // if editing, tool is editselect, and a selection is visible
                // which means selwidth/height != 0)
                if ((PicMode == PicEditorMode.Edit) && (SelectedTool == PicToolType.SelectArea) && (SelectedRegion.Width != 0) && (SelectedRegion.Height != 0)) {
                    // Get the bounds of the selected region
                    // Create a bitmap with the size of the selected region
                    Bitmap bitmap = new Bitmap(SelectedRegion.Width, SelectedRegion.Height);
                    // Create a graphics object from the bitmap
                    using Graphics g = Graphics.FromImage(bitmap);
                    // Draw the specified region of the PictureBox onto the bitmap
                    g.DrawImage(priorityActive ? EditPicture.PriorityBMP : EditPicture.VisualBMP, new Rectangle(0, 0, bitmap.Width, bitmap.Height), SelectedRegion, GraphicsUnit.Pixel);
                    // Set the bitmap to the clipboard
                    Clipboard.SetImage(bitmap);
                    priorityActive = false;
                    return;
                }

                // if one or more commands selected
                if (lstCommands.SelectedItems.Count >= 1 && lstCoords.SelectedItems.Count == 0 && !lstCommands.Items[^1].Selected) {
                    PictureClipboardData picCB = new();
                    picCB.CmdCount = SelectedCmdCount;
                    // get starting position of resource data
                    int startPos = (int)lstCommands.Items[SelectedCmd.Index - SelectedCmdCount + 1].Tag;
                    int endPos = ((int)lstCommands.Items[SelectedCmd.Index + 1].Tag);
                    byte[] bytData = EditPicture.Data[startPos..endPos];
                    picCB.Data = bytData;
                    picCB.StartPen = EditPicture.GetPenStatus(startPos);
                    picCB.EndPen = EditPicture.GetPenStatus(endPos - 1);
                    // Check for pen settings that need to be enforced. If the 
                    // group of commands begins with any pen settings, then those
                    // pen types (vis, pri, plot) don't need to be included. Any
                    // pens that are not set before first draw command must be
                    // included. Also, if there are no plot commands, plot pen
                    // settings don't need to be included.
                    picCB.IncludeVisPen = true;
                    picCB.IncludePriPen = true;
                    picCB.IncludePlotPen = true;
                    bool hasPlot = false;
                    int cmdStart = -1, cmdEnd = -1;
                    for (int i = 0; i < SelectedCmdCount; i++) {
                        DrawFunction cmd = (DrawFunction)EditPicture.Data[(int)lstCommands.Items[SelectedCmd.Index - SelectedCmdCount + 1 + i].Tag];
                        switch (cmd) {
                        case EnableVis or DisableVis:
                            if (cmdStart == -1) {
                                // pen set before first command, so it doesn't need
                                // to be included
                                picCB.IncludeVisPen = false;
                            }
                            break;
                        case EnablePri or DisablePri:
                            if (cmdStart == -1) {
                                // pen set before first command, so it doesn't need
                                // to be included
                                picCB.IncludePriPen = false;
                            }
                            break;
                        case ChangePen:
                            if (!hasPlot) {
                                // pen set before first plot command, so it doesn't need
                                // to be included
                                picCB.IncludePlotPen = false;
                                picCB.HasPenChange = true;
                            }
                            break;
                        default:
                            // draw command found
                            if (cmdStart == -1) {
                                cmdStart = i;
                            }
                            cmdEnd = i + 1;
                            if (cmd == PlotPen) {
                                hasPlot = true;
                            }
                            break;
                        }
                    }
                    // if no plot commmand plot pen does not need to be included
                    picCB.HasPlotCmds = hasPlot;
                    if (!hasPlot) {
                        picCB.IncludePlotPen = false;
                    }
                    int sp = (int)lstCommands.Items[SelectedCmd.Index - SelectedCmdCount + 1 + cmdStart].Tag;
                    int ep = (int)lstCommands.Items[SelectedCmd.Index - SelectedCmdCount + 1 + cmdEnd].Tag - 1;
                    picCB.DrawCmdStartPen = EditPicture.GetPenStatus(sp);
                    picCB.DrawCmdEndPen = EditPicture.GetPenStatus(ep);
                    picCB.DrawCmdStart = cmdStart;
                    picCB.DrawCmdCount = cmdEnd - cmdStart;
                    picCB.DrawByteStart = sp - startPos;
                    picCB.DrawByteCount = ep - sp + 1;
                    DataObject dataObject = new();
                    dataObject.SetData(PICTURE_CB_FMT, picCB);
                    // add data as CSV list of bytes
                    string csvData = bytData[0].ToString();
                    // add line returns for each change in cmd
                    for (int i = 1; i < bytData.Length; i++) {
                        // if this is a new command
                        if (bytData[i] >= 240) {
                            csvData += "\r\n" + bytData[i];
                        }
                        else {
                            csvData += ", " + bytData[i];
                        }
                    }
                    // Convert the CSV text to a UTF-8 byte stream before adding it to the container object.
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
                    var stream = new System.IO.MemoryStream(bytes);
                    dataObject.SetData(DataFormats.CommaSeparatedValue, stream);
                    Clipboard.SetDataObject(dataObject, true);
                }
            }
        }

        private bool CanCopy() {
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    // copy is enabled if something selected
                    return (SelectedRegion.Width > 0) && (SelectedRegion.Height > 0);
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 || SelectedCmd.IsPen) {
                    return SelectedCmd.Type != End;
                }
                else {
                    return false;
                }
            }
            else {
                return false;
            }
        }

        private void mnuPaste_Click(object sender, EventArgs e) {
            int InsertIndex, InsertPos;

            if (PicMode != PicEditorMode.Edit) {
                // not in edit mode
                return;
            }
            if (SelectedTool == PicToolType.SelectArea) {
                return;
            }
            if (!Clipboard.ContainsData(PICTURE_CB_FMT)) {
                return;
            }
            PictureClipboardData picCBData = Clipboard.GetData(PICTURE_CB_FMT) as PictureClipboardData;
            if (picCBData is null) {
                return;
            }
            if (SelectedTool != PicToolType.Edit) {
                // always set tool to select
                SelectTool(PicToolType.Edit);
            }
            // if more than one command selected
            if (SelectedCmdCount > 1) {
                // paste after the last selected item
                InsertIndex = SelectedCmd.Index + 1;
            }
            else {
                // get current index position
                InsertIndex = SelectedCmd.Index;
            }
            InsertPos = (int)lstCommands.Items[InsertIndex].Tag;
            PictureUndo NextUndo = new();
            NextUndo.Action = PasteCmds;
            NextUndo.CmdIndex = InsertIndex;
            NextUndo.PicPos = InsertPos;

            /*
            // an alternate paste method that adds pen status commands to pasted
            // data instead of adjusting pen styles

            // use all data
            NextUndo.CmdCount = picCBData.CmdCount;
            NextUndo.ByteCount = picCBData.Data.Length;
            if (picCBData.HasPlotCmds || picCBData.HasPenChange) {
                // need to confirm the pasted commands won't cause error
                // due to plot style mismatch
                PenStatus currentPen = EditPicture.GetPenStatus(InsertPos);
                bool addpen = false;
                if (picCBData.HasPlotCmds) {

                    // IncludePlotPen: false means first plot cmd comes AFTER the first
                    // pen set, so the plot pen does not need to be included/checked

                    // IncludePlotPen: true means first plot cmd comes BEFORE the first
                    // pen set, so the plot pen must be included/checked

                    if (picCBData.IncludePlotPen) {
                        // check if the plot style is different
                        // if so, then we need to add a set plot pen command
                        // before the first draw command
                        if (currentPen.PlotStyle != picCBData.StartPen.PlotStyle) {
                            addpen = true;
                            bool dontwarn = !WinAGISettings.WarnPlotPaste.Value;
                            if (!dontwarn && MsgBoxEx.Show(MDIMain,
                                "The clipboard contains plot commands that are a style mismatch. " +
                                "Pen changes will be inserted to avoid errors. OK to continue?",
                                "Plot Pen Style Mismatch",
                                MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question,
                                "Don't show this warning again.", ref dontwarn,
                                WinAGIHelp, "htm\\winagi\\editor_picture.htm#plotpaste") != DialogResult.OK) {
                                // cancel the paste
                                return;
                            }
                            if (dontwarn) {
                                // update settings
                                WinAGISettings.WarnPlotPaste.Value = false;
                            }
                            // add a plot pen with correct style
                            NextUndo.CmdCount++;
                            NextUndo.ByteCount += 2;
                            byte newpendata = (byte)(currentPen.PlotSize + 0x10 * (int)currentPen.PlotShape + 0x20 * (int)picCBData.StartPen.PlotStyle);
                            byte[] bytes = [0xF9, newpendata];
                            EditPicture.InsertData(bytes, InsertPos);
                            InsertPos += 2;
                        }
                    }
                }
                // now paste the clipboard data
                EditPicture.InsertData(picCBData.Data, InsertPos);
                InsertPos += picCBData.Data.Length;
                // if the pasted data includes a pen change?
                // or if a pen change was already forced?
                if (addpen || picCBData.HasPenChange) {
                    // check that end pen matches current pen
                    // but... endpen isn't really endpen unless
                    // the last command is a draw command- how do
                    // I tell for certain what real endpen is?
                    // answer: check EditPicture after the insert!
                    PenStatus endPen = EditPicture.GetPenStatus(InsertPos + picCBData.Data.Length);
                    if (currentPen.PlotStyle != endPen.PlotStyle) {
                        bool dontwarn = !WinAGISettings.WarnPlotPaste.Value || addpen;
                        // show warning dialog
                        if (!dontwarn && MsgBoxEx.Show(MDIMain,
                            "The clipboard contains plot commands that are a style mismatch. " +
                            "Pen changes will be inserted to avoid errors. OK to continue?",
                            "Plot Pen Style Mismatch",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question,
                            "Don't show this warning again.", ref dontwarn,
                                WinAGIHelp, "htm\\winagi\\editor_picture.htm#plotpaste") != DialogResult.OK) {
                            // cancel the paste
                            return;
                        }
                        if (dontwarn && !addpen) {
                            // update settings
                            WinAGISettings.WarnPlotPaste.Value = false;
                        }
                        // restore pen
                        NextUndo.CmdCount++;
                        NextUndo.ByteCount += 2;
                        byte newpendata = (byte)(currentPen.PlotSize + 0x10 * (int)currentPen.PlotShape + 0x20 * (int)currentPen.PlotStyle);
                        byte[] bytes = [0xF9, newpendata];
                        EditPicture.InsertData(bytes, InsertPos);
                    }
                }
            }
            else {
                // OK to paste the clipboard data as-is
                // insert the data
                EditPicture.InsertData(picCBData.Data, InsertPos);
            }
            // rebuild cmd list
            AdjustCommandList(InsertIndex, NextUndo.ByteCount);
            // select the commands that were pasted
            SelectCommand(InsertIndex + NextUndo.CmdCount - 1, NextUndo.CmdCount, true);
            AddUndo(NextUndo);
            */

            NextUndo.CmdCount = picCBData.CmdCount;
            // need to confirm the pasted commands won't cause errors
            // due to plot style mismatch
            PenStatus currentPen = EditPicture.GetPenStatus(InsertPos);
            // adjust plot commands before adding the pasted commands
            if (picCBData.HasPenChange) {
                // check pen staus at end of paste data
                if (picCBData.EndPen.PlotStyle != currentPen.PlotStyle) {
                    // adjust plot pattern starting with next command after the ones being pasted
                    ReadjustPlotCoordinates(InsertIndex + picCBData.CmdCount, picCBData.DrawCmdEndPen.PlotStyle);
                }
            }
            // insert the data
            if (picCBData.HasPlotCmds) {
                if (picCBData.StartPen.PlotStyle != currentPen.PlotStyle) {

                    // start at beginning of paste data, step through all
                    // commands until another setplotpen command or end is reached;
                    // any plot commands identified during search are checked to
                    // see if they match format of desired plot pen style (solid or
                    // splatter); if they don't match, they are adjusted (by adding
                    // or removing the pattern byte)

                    List<byte> bytTemp = [.. picCBData.Data];

                    for (int i = 0; i < bytTemp.Count; i++) {
                        // check for plot command or change plot pen command
                        switch ((DrawFunction)bytTemp[i]) {
                        case PlotPen:
                            // if style is splatter
                            if (currentPen.PlotStyle == PlotStyle.Splatter) {
                                // set insertpos in front of first coord x Value
                                int pos = ++i;
                                do {
                                    // get random pattern
                                    // add it to paste data
                                    bytTemp.Insert(pos, (byte)(GetRandomByte(0, 119) * 2));
                                    // adjust pos
                                    pos += 3;
                                    i = pos;
                                } while (pos < bytTemp.Count && bytTemp[pos] < 0xF0);
                            }
                            else {
                                // delete pattern bytes
                                // set start pos for first coord
                                int pos = ++i;
                                do {
                                    // remove from picture resource
                                    bytTemp.RemoveAt(pos);
                                    // adjust pos
                                    pos += 2;
                                    i = pos;
                                } while (pos < bytTemp.Count && bytTemp[pos] < 0xF0);
                            }
                            break;
                        case ChangePen:
                            // set pen
                            // can exit here because this pen command
                            // ensures future plot commands are correct
                            i = bytTemp.Count;
                            break; // exit do
                        }
                    }
                    NextUndo.ByteCount = bytTemp.Count;
                    EditPicture.InsertData(bytTemp.ToArray(), InsertPos);
                }
                else {
                    // use all data
                    NextUndo.ByteCount = picCBData.Data.Length;
                    EditPicture.InsertData(picCBData.Data, InsertPos);
                }
            }
            else {
                // use all data
                NextUndo.ByteCount = picCBData.Data.Length;
                EditPicture.InsertData(picCBData.Data, InsertPos);
            }
            // rebuild cmd list
            AdjustCommandList(InsertIndex, NextUndo.ByteCount);
            AddUndo(NextUndo);
            // select the commands that were pasted
            SelectCommand(InsertIndex + NextUndo.CmdCount - 1, NextUndo.CmdCount, true);

        }

        private void mnuPastePen_Click(object sender, EventArgs e) {
            int InsertIndex, InsertPos;

            if (PicMode != PicEditorMode.Edit) {
                // not in edit mode
                return;
            }
            if (SelectedTool == PicToolType.SelectArea) {
                return;
            }
            if (!Clipboard.ContainsData(PICTURE_CB_FMT)) {
                return;
            }
            PictureClipboardData picCBData = Clipboard.GetData(PICTURE_CB_FMT) as PictureClipboardData;
            if (picCBData is null) {
                return;
            }
            if (SelectedTool != PicToolType.Edit) {
                // always set tool to select
                SelectTool(PicToolType.Edit);
            }
            // if more than one command selected
            if (SelectedCmdCount > 1) {
                // paste after the last selected item
                InsertIndex = SelectedCmd.Index + 1;
            }
            else {
                // get current index position
                InsertIndex = SelectedCmd.Index;
            }
            InsertPos = (int)lstCommands.Items[InsertIndex].Tag;
            PictureUndo NextUndo = new();
            NextUndo.Action = PasteCmds;
            NextUndo.PicPos = InsertPos;
            NextUndo.CmdIndex = InsertIndex;
            // only use draw cmd count
            NextUndo.CmdCount = picCBData.DrawCmdCount;
            NextUndo.ByteCount = picCBData.DrawByteCount;

            PenStatus currentPen = EditPicture.GetPenStatus(InsertPos);
            // first determine if pens need to be added
            if (picCBData.IncludeVisPen && currentPen.VisColor != picCBData.DrawCmdStartPen.VisColor) {
                NextUndo.CmdCount++;
                if (picCBData.DrawCmdStartPen.VisColor == AGIColorIndex.None) {
                    // add vis OFF
                    EditPicture.InsertData(0xF1, InsertPos++);
                    NextUndo.ByteCount++;
                }
                else {
                    // add vis ON
                    byte[] bytes = [0xF0, (byte)picCBData.DrawCmdStartPen.VisColor];
                    EditPicture.InsertData(bytes, InsertPos);
                    InsertPos += 2;
                    NextUndo.ByteCount += 2;
                }
            }
            if (picCBData.IncludePriPen && currentPen.PriColor != picCBData.DrawCmdStartPen.PriColor) {
                NextUndo.CmdCount++;
                if (picCBData.DrawCmdStartPen.PriColor == AGIColorIndex.None) {
                    // add pri OFF
                    EditPicture.InsertData(0xF3, InsertPos++);
                    NextUndo.ByteCount++;
                }
                else {
                    // add pri ON
                    byte[] bytes = [0xF2, (byte)picCBData.DrawCmdStartPen.PriColor];
                    EditPicture.InsertData(bytes, InsertPos);
                    InsertPos += 2;
                    NextUndo.ByteCount += 2;
                }
            }
            if (picCBData.IncludePlotPen && (currentPen.PlotShape != picCBData.DrawCmdStartPen.PlotShape ||
                currentPen.PlotStyle != picCBData.DrawCmdStartPen.PlotStyle || currentPen.PlotSize != picCBData.DrawCmdStartPen.PlotSize)) {
                NextUndo.CmdCount++;
                NextUndo.ByteCount += 2;
                byte newpendata = (byte)(picCBData.DrawCmdStartPen.PlotSize + 0x10 * (int)picCBData.DrawCmdStartPen.PlotShape + 0x20 * (int)picCBData.DrawCmdStartPen.PlotStyle);
                byte[] bytes = [0xF9, newpendata];
                EditPicture.InsertData(bytes, InsertPos);
                InsertPos += 2;
            }
            // insert the data
            EditPicture.InsertData(picCBData.Data[picCBData.DrawByteStart..(picCBData.DrawByteStart + picCBData.DrawByteCount)], InsertPos);
            InsertPos += picCBData.DrawByteCount;
            // now check if pens need to be restored
            if (currentPen.VisColor != picCBData.DrawCmdEndPen.VisColor) {
                NextUndo.CmdCount++;
                if (currentPen.VisColor == AGIColorIndex.None) {
                    // add vis OFF
                    EditPicture.InsertData(0xF1, InsertPos++);
                    NextUndo.ByteCount++;
                }
                else {
                    // add vis ON
                    byte[] bytes = [0xF0, (byte)currentPen.VisColor];
                    EditPicture.InsertData(bytes, InsertPos);
                    NextUndo.ByteCount += 2;
                }
            }
            if (currentPen.PriColor != picCBData.DrawCmdEndPen.PriColor) {
                NextUndo.CmdCount++;
                if (currentPen.PriColor == AGIColorIndex.None) {
                    // add pri OFF
                    EditPicture.InsertData(0xF3, InsertPos++);
                    NextUndo.ByteCount++;
                }
                else {
                    // add pri ON
                    byte[] bytes = [0xF2, (byte)currentPen.PriColor];
                    EditPicture.InsertData(bytes, InsertPos);
                    NextUndo.ByteCount += 2;
                    InsertPos += 2;
                }
            }
            if ((currentPen.PlotShape != picCBData.DrawCmdEndPen.PlotShape ||
                currentPen.PlotStyle != picCBData.DrawCmdEndPen.PlotStyle ||
                currentPen.PlotSize != picCBData.DrawCmdEndPen.PlotSize)) {
                NextUndo.CmdCount++;
                NextUndo.ByteCount += 2;
                byte newpendata = (byte)(currentPen.PlotSize + 0x10 * (int)currentPen.PlotShape + 0x20 * (int)currentPen.PlotStyle);
                byte[] bytes = [0xF9, newpendata];
                EditPicture.InsertData(bytes, InsertPos);
            }
            // rebuild cmd list
            AdjustCommandList(InsertIndex, NextUndo.ByteCount);
            AddUndo(NextUndo);
            // select the commands that were pasted
            SelectCommand(InsertIndex + NextUndo.CmdCount - 1, NextUndo.CmdCount, true);
        }

        private void mnuDelete_Click(object sender, EventArgs e) {
            // delete a coordinate or command
            if (CanDelete()) {
                if (SelectedCmd.SelectedCoordIndex == -1) {
                    DeleteCommands(SelectedCmd.Index, SelectedCmdCount);
                    // select the cmd that is just after the deleted items
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                else {
                    // fill, plot and absolute lines allow deleting of any coordinate
                    // only last coordinate of other commands can be deleted
                    switch (SelectedCmd.Type) {
                    case AbsLine:
                    case Fill:
                    case PlotPen:
                        DeleteCoordinate(SelectedCmd.SelectedCoordIndex);
                        Debug.Assert(SelectedCmd.Coords.Count != 0);
                        if (SelectedCmd.SelectedCoordIndex > SelectedCmd.Coords.Count - 1) {
                            SelectedCmd.SelectedCoordIndex = SelectedCmd.Coords.Count - 1;
                        }
                        SelectCoordinate(SelectedCmd.SelectedCoordIndex, true);
                        break;
                    default:
                        if (SelectedCmd.SelectedCoordIndex == 0 || SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1) {
                            DeleteCoordinate(SelectedCmd.SelectedCoordIndex);
                            Debug.Assert(SelectedCmd.Coords.Count != 0);
                            if (SelectedCmd.SelectedCoordIndex > SelectedCmd.Coords.Count - 1) {
                                SelectedCmd.SelectedCoordIndex = SelectedCmd.Coords.Count - 1;
                            }
                            SelectCoordinate(SelectedCmd.SelectedCoordIndex, true);
                        }
                        break;
                    }
                }
            }
        }

        private bool CanDelete() {

            // mode dependent items
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    return false;
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 || SelectedCmd.IsPen) {
                    return SelectedCmd.Type != End;
                }
                else {
                    // a coordinate is selected
                    // delete always available for absline, fill, plot
                    // delete only available for other commands if on last coord
                    switch (SelectedCmd.Type) {
                    case AbsLine:
                    case Fill:
                    case PlotPen:
                        return true;
                    default:
                        // Corner lines or relative lines
                        return SelectedCmd.SelectedCoordIndex == 0 || SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1;
                    }
                }
            }
            else {
                return false;
            }
        }

        private void mnuClearPicture_Click(object sender, EventArgs e) {
            if (PicMode != PicEditorMode.Edit || lstCommands.Items.Count == 1) {
                return;
            }
            // clears the entire picture (don't forget to leave the 'End' command)
            DeleteCommands(lstCommands.Items.Count - 2, lstCommands.Items.Count - 1);
            UndoCol.Peek().Action = Clear;
            // select the End cmd that's left
            SelectCommand(0, 1, true);
        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {
            // selects all commands if in edit mode,
            // or select entire picture if tool is select-area

            if (PicMode != PicEditorMode.Edit || lstCommands.Items.Count == 1) {
                return;
            }

            // if editselect tool is chosen, change selection to cover entire area
            switch (SelectedTool) {
            case PicToolType.SelectArea:
                SelectedRegion.X = 0;
                SelectedRegion.Y = 0;
                SelectedRegion.Width = 160;
                SelectedRegion.Height = 168;
                spAnchor.Text = "Anchor: 0, 0";
                spBlock.Text = "Block: 0, 0, 159, 167";
                picVisual.Invalidate();
                picPriority.Invalidate();

                return;
            case PicToolType.Edit:
                // if nothing to select
                if (lstCommands.Items.Count == 1) {
                    return;
                }
                lstCommands.BeginUpdate();
                SelectCommand(lstCommands.Items.Count - 2, lstCommands.Items.Count - 1);
                lstCommands.EndUpdate();
                // if more than one cmd
                if (lstCommands.SelectedItems.Count > 1) {
                    // get bounds, and select the cmds
                    SetSelectionBounds(true);
                }
                break;
            }
        }

        private void mnuInsertCoord_Click(object sender, EventArgs e) {
            if (CanInsertCoord()) {
                InsertCoordinate();
            }
        }

        private bool CanInsertCoord() {
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    // area selection - no editing commands are enabled,
                    // only copy is available
                    return false;
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 || SelectedCmd.IsPen) {
                    // no coordinate is selected
                    if (SelectedTool == PicToolType.Edit && SelectedCmd.Type == PlotPen || SelectedCmd.Type == Fill) {
                        return true;
                    }
                    else if ((SelectedTool == PicToolType.Plot && SelectedCmd.Type == PlotPen) ||
                             (SelectedTool == PicToolType.Fill && SelectedCmd.Type == Fill)) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                else {
                    switch (SelectedCmd.Type) {
                    case AbsLine:
                    case Fill:
                    case PlotPen:
                        return true;
                    default:
                        // Corner lines or relative lines
                        return SelectedCmd.SelectedCoordIndex == 0 || SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1;
                    }
                }
            }
            else {
                return false;
            }
        }

        private void mnuSplitCommand_Click(object sender, EventArgs e) {
            // splits a command into two separate commands of the same Type
            if (CanSplit()) {
                SplitCommand();
            }
        }

        private bool CanSplit() {
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    // area selection - no editing commands are enabled,
                    return false;
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 || SelectedCmd.IsPen) {
                    return false;
                }
                else {
                    // a coordinate is selected
                    // disable split if not just one cmd selected OR
                    // cmd is set color pen or set plot pen OR
                    // only one coordinate OR
                    // no coord selected
                    if (SelectedCmdCount != 1 ||
                        SelectedCmd.IsPen ||
                        SelectedCmd.SelectedCoordIndex < 0) {
                        return false;
                    }
                    else {
                        // if on a line, fill, or plot cmd
                        switch (SelectedCmd.Type) {
                        case AbsLine or RelLine or XCorner or YCorner:
                            // only if three or more, and not on either end
                            if (SelectedCmd.Coords.Count < 3 || SelectedCmd.SelectedCoordIndex == 0 || SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1) {
                                return false;
                            }
                            else {
                                return true;
                            }
                        case Fill or PlotPen:
                            // only if not on first coordinate
                            return SelectedCmd.SelectedCoordIndex != 0;
                        }
                        return false;
                    }
                }
            }
            else {
                return false;
            }
        }

        private void mnuJoinCommands_Click(object sender, EventArgs e) {
            // joins two commands that are adjacent, where first coord of
            // second command is same point as end of previous command
            // or if both cmds are plots or fills
            if (CanJoin()) {
                JoinCommands();
            }
        }

        private bool CanJoin() {
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    return false;
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 && !SelectedCmd.IsPen) {
                    // no coordinate is selected
                    return CanJoinCommands(SelectedCmd.Index);
                }
                else {
                    // a coordinate is selected or it's a pen
                    return false;
                }
            }
            else {
                return false;
            }
        }

        private void mnuFlipH_Click(object sender, EventArgs e) {
            // flip Horizontal
            if (CanFlipH()) {
                FlipHorizontal(SelectedCmd.Index, SelectedCmdCount);
                // force re-selection
                SelectCommand(SelectedCmd.Index, SelectedCmdCount, true);
                SetSelectionBounds(true);
                picVisual.Refresh();
                picPriority.Refresh();
            }
        }

        private bool CanFlipH() {
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    return false;
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 || SelectedCmd.IsPen) {
                    return (SelectedRegion.Width > 1);
                }
                else {
                    // a coordinate is selected
                    return false;
                }
            }
            else {
                return false;
            }
        }

        private void mnuFlipV_Click(object sender, EventArgs e) {
            // flip Vertical
            if (CanFlipV()) {
                FlipVertical(SelectedCmd.Index, SelectedCmdCount);
                // force re-selection
                SelectCommand(SelectedCmd.Index, SelectedCmdCount, true);
                SetSelectionBounds(true);
                picVisual.Refresh();
                picPriority.Refresh();
            }
        }

        private bool CanFlipV() {
            if (PicMode == PicEditorMode.Edit) {
                if (SelectedTool == PicToolType.SelectArea) {
                    return false;
                }
                else if (SelectedCmd.SelectedCoordIndex < 0 || SelectedCmd.IsPen) {
                    return (SelectedRegion.Height > 1);
                }
                else {
                    // a coordinate is selected
                    return false;
                }
            }
            else {
                return false;
            }
        }

        private void mnuSetTestView_Click(object sender, EventArgs e) {
            if (PicMode == PicEditorMode.ViewTest) {
                GetTestView();
                DrawPicture();
            }
        }

        private void mnuTestViewOptions_Click(object sender, EventArgs e) {
            if (PicMode == PicEditorMode.ViewTest) {
                // stop motion and stop cycling
                if (TestDir != ObjDirection.odStopped) {
                    TestDir = ObjDirection.odStopped;
                }
                tmrTest.Enabled = false;
                if (TestView is null) {
                    // load one first
                    GetTestView();
                    // if still no testview
                    if (TestView is null) {
                        // exit
                        return;
                    }
                }
                frmPicTestOptions frmTest = new(TestView, TestSettings);

                // if not canceled
                if (frmTest.ShowDialog(this) == DialogResult.OK) {
                    // Retrieve option values safely by copying TestInfo to a local variable
                    var testInfoCopy = frmTest.TestInfo;
                    TestSettings = testInfoCopy.Clone();

                    // if test loop and/or cel are NOT auto, force current loop/cel
                    if (TestSettings.TestLoop != -1) {
                        CurTestLoop = (byte)TestSettings.TestLoop;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        // just in case, check current cel; if it exceeds
                        // loop count, reset it to zero
                        if (CurTestCel > CurTestLoopCount - 1) {
                            CurTestCel = (byte)(CurTestLoopCount - 1);
                        }
                        if (TestSettings.TestCel != -1) {
                            CurTestCel = (byte)TestSettings.TestCel;
                        }
                        // if either loop or cel is forced, update cel data
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    // update cel height/width/transcolor
                    CelWidth = TestView[CurTestLoop][CurTestCel].Width;
                    CelHeight = TestView[CurTestLoop][CurTestCel].Height;
                    CelTrans = TestView[CurTestLoop][CurTestCel].TransColor;
                    // set timer based on speed
                    switch (TestSettings.ObjSpeed.Value) {
                    case 0:
                        // slow
                        tmrTest.Interval = 200;
                        break;
                    case 1:
                        // normal
                        tmrTest.Interval = 50;
                        break;
                    case 2:
                        // fast
                        tmrTest.Interval = 20;
                        break;
                    case 3:
                        // fastest
                        tmrTest.Interval = 1;
                        break;
                    }
                    // redraw cel at current position
                    DrawPicture();
                }
                frmTest.Dispose();
                // set timer as needed
                tmrTest.Enabled = TestSettings.CycleAtRest.Value;
            }
        }

        private void mnuTestTextOptions_Click(object sender, EventArgs e) {
            if (PicMode == PicEditorMode.PrintTest) {
                GetTextOptions();
            }
        }

        private void mnuTextScreenSize_Click(object sender, EventArgs e) {
            if (PicMode == PicEditorMode.PrintTest && InGame && EditGame.PowerPack) {
                ToggleTextScreenSize();
            }
        }

        private void mnuToggleScreen_Click(object sender, EventArgs e) {
            // not available if text testing, or if both are visible
            if (OneWindow == WindowMode.Both || PicMode == PicEditorMode.PrintTest) {
                return;
            }

            if (TooSmall) {
                if (splitImages.Panel2Collapsed) {
                    splitImages.Panel1Collapsed = true;
                    splitImages.Panel2Collapsed = false;
                }
                else {
                    splitImages.Panel2Collapsed = true;
                    splitImages.Panel1Collapsed = false;
                }
            }
            else {
                if (OneWindow == WindowMode.Visual) {
                    OneWindow = WindowMode.Priority;
                    splitImages.SplitterDistance = 0;
                }
                else if (OneWindow == WindowMode.Priority) {
                    OneWindow = WindowMode.Visual;
                    splitImages.SplitterDistance = splitImages.Height - splitImages.SplitterWidth;
                }
            }
        }

        private void mnuToggleBands_Click(object sender, EventArgs e) {
            // toggles the priority bands
            ShowBands = !ShowBands;
            picVisual.Invalidate();
            picPriority.Invalidate();
        }

        private void mnuEditPriBase_Click(object sender, EventArgs e) {
            // allows user to set the priority base value
            // only available for v2.936 or greater game pictures (or if not in game)
            if (!InGame || Array.IndexOf(IntVersions, EditGame.InterpreterVersion) >= 13) {
                byte oldBase = EditPicture.PriBase;
                string newBaseText = oldBase.ToString();
                byte newBase;
                do {
                    bool cancel = ShowInputDialog(this, "Set Priority Base", "Enter new priority base value: ", ref newBaseText) != DialogResult.OK;

                    if (cancel || newBaseText.Length == 0) {
                        // canceled, or if empty string
                        return;
                    }
                    // validate
                    if (!byte.TryParse(newBaseText, out newBase)) {
                        MessageBox.Show(MDIMain,
                            "You must enter an integer number between 0 and 158",
                            "Invalid Base Value",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\winagi\\editor_picture.htm#pribands");
                    }
                    else if (newBase < 0 || newBase > 158) {
                        // invalid
                        MessageBox.Show(MDIMain,
                            "You must enter a value between 0 and 158",
                            "Invalid Base Value",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\winagi\\editor_picture.htm#pribands");
                    }
                    else {
                        // OK!
                        break;
                    }
                } while (true);

                // set new pri base
                EditPicture.PriBase = newBase;
                if (ShowBands) {
                    // redraw bands
                    picVisual.Invalidate();
                    picPriority.Invalidate();
                }
                PictureUndo NextUndo = new();
                NextUndo.Action = SetPriBase;
                // use cmdIndex for old base
                NextUndo.CmdIndex = oldBase;
                AddUndo(NextUndo);
            }
        }

        private void mnuToggleTextMarks_Click(object sender, EventArgs e) {
            // toggles the text marks
            ShowTextMarks = !ShowTextMarks;
            picVisual.Invalidate();
            picPriority.Invalidate();
        }

        private void mnuToggleBackground_Click(object sender, EventArgs e) {
            // toggles the background image if one is set
            if (BkgdImage is not null) {
                UpdateBkgd(!EditPicture.BkgdVisible);
            }
        }

        private void mnuEditBackground_Click(object sender, EventArgs e) {
            UpdateBkgd(true, true);
        }

        private void mnuRemoveBackground_Click(object sender, EventArgs e) {
            // removes the background image and clears background settings
            if (BkgdImage is not null) {
                UpdateBkgd(false);
            }
            BkgdImage = null;
            EditPicture.BackgroundSettings = new();
            if (InGame) {
                // update the resource
                EditGame.Pictures[PictureNumber].BackgroundSettings = new();
                EditGame.Pictures[PictureNumber].SaveProps();
            }
        }
        #endregion

        #region Toolbar Event Handlers
        private void tsbMode_DropDownOpening(object sender, EventArgs e) {
            tsbEditMode.Checked = PicMode == PicEditorMode.Edit;
            tsbViewTest.Checked = PicMode == PicEditorMode.ViewTest;
            tsbPrintTest.Checked = PicMode == PicEditorMode.PrintTest;
        }

        private void tsbTool_DropDownOpening(object sender, EventArgs e) {
            tsbEditTool.Checked = SelectedTool == PicToolType.Edit;
            tsbImageSelect.Checked = SelectedTool == PicToolType.SelectArea;
            tsbLine.Checked = SelectedTool == PicToolType.Line;
            tsbShortLine.Checked = SelectedTool == PicToolType.ShortLine;
            tsbStepLine.Checked = SelectedTool == PicToolType.StepLine;
            tsbRectangle.Checked = SelectedTool == PicToolType.Rectangle;
            tsbTrapezoid.Checked = SelectedTool == PicToolType.Trapezoid;
            tsbEllipse.Checked = SelectedTool == PicToolType.Ellipse;
            tsbFill.Checked = SelectedTool == PicToolType.Fill;
            tsbPlot.Checked = SelectedTool == PicToolType.Plot;
        }

        private void tsbPlotSize_DropDownOpening(object sender, EventArgs e) {
            tsbSize0.Checked = SelectedCmd.Pen.PlotSize == 0;
            tsbSize1.Checked = SelectedCmd.Pen.PlotSize == 1;
            tsbSize2.Checked = SelectedCmd.Pen.PlotSize == 2;
            tsbSize3.Checked = SelectedCmd.Pen.PlotSize == 3;
            tsbSize4.Checked = SelectedCmd.Pen.PlotSize == 4;
            tsbSize5.Checked = SelectedCmd.Pen.PlotSize == 5;
            tsbSize6.Checked = SelectedCmd.Pen.PlotSize == 6;
            tsbSize7.Checked = SelectedCmd.Pen.PlotSize == 7;
        }

        private void tsbPlotStyle_DropDownOpening(object sender, EventArgs e) {
            tsbCircleSolid.Checked = SelectedCmd.Pen.PlotShape == PlotShape.Circle && SelectedCmd.Pen.PlotStyle == PlotStyle.Solid;
            tsbSquareSolid.Checked = SelectedCmd.Pen.PlotShape == PlotShape.Square && SelectedCmd.Pen.PlotStyle == PlotStyle.Solid;
            tsbCircleSplat.Checked = SelectedCmd.Pen.PlotShape == PlotShape.Circle && SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter;
            tsbSquareSplat.Checked = SelectedCmd.Pen.PlotShape == PlotShape.Square && SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter;
        }

        private void tsbZoomIn_Click(object sender, EventArgs e) {
            ChangeScale(1);
        }

        private void tsbZoomOut_Click(object sender, EventArgs e) {
            ChangeScale(-1);
        }

        private void tsbFullDraw_Click(object sender, EventArgs e) {
            EditPicture.StepDraw = !tsbFullDraw.Checked;
            DrawPicture();
        }

        private void tsbEditTool_Click(object sender, EventArgs e) {
            // Edit Command tool
            SelectTool(PicToolType.Edit);
        }

        private void tsbImageSelect_Click(object sender, EventArgs e) {
            // Select Image Area tool
            SelectTool(PicToolType.SelectArea);
        }

        private void tsbLine_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.Line);
        }

        private void tsbShortLine_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.ShortLine);
        }

        private void tsbStepLine_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.StepLine);
        }

        private void tsbRectangle_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.Rectangle);
        }

        private void tsbTrapezoid_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.Trapezoid);
        }

        private void tsbEllipse_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.Ellipse);
        }

        private void tsbFill_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.Fill);
        }

        private void tsbPlot_Click(object sender, EventArgs e) {
            SelectTool(PicToolType.Plot);
        }

        private void tsbPlotStyle_Click(object sender, EventArgs e) {
            tsbPlotStyle.Image = ((ToolStripMenuItem)sender).Image;
            UpdatePlotPen((byte)((int)((ToolStripMenuItem)sender).Tag + SelectedCmd.Pen.PlotSize));
        }

        private void tsbPlotSize_Click(object sender, EventArgs e) {
            tsbPlotSize.Image = ((ToolStripMenuItem)sender).Image;
            int newsize = (int)((ToolStripMenuItem)sender).Tag;
            byte newplotdata = (byte)((int)SelectedCmd.Pen.PlotStyle * 0x20 + (int)SelectedCmd.Pen.PlotShape * 0x10 + newsize);
            UpdatePlotPen(newplotdata);
        }
        #endregion

        #region Control Event Handlers
        private void vsbVisual_Scroll(object sender, ScrollEventArgs e) {
            picVisual.Top = -vsbVisual.Value;
        }

        private void hsbVisual_Scroll(object sender, ScrollEventArgs e) {
            picVisual.Left = -hsbVisual.Value;
        }

        private void vsbPriority_Scroll(object sender, ScrollEventArgs e) {
            picPriority.Top = -vsbPriority.Value;
        }

        private void hsbPriority_Scroll(object sender, ScrollEventArgs e) {
            picPriority.Left = -hsbPriority.Value;
        }

        private void splitImages_Resize(object sender, EventArgs e) {
            SetScrollbars();
        }

        private void DrawSurface_MouseDown(object sender, MouseEventArgs e) {
            priorityActive = (sender == picPriority);
            byte[] bytData;
            byte bytPattern;

            // right-click cancels drawing, or displays context menu if not
            // currently drawing
            if (e.Button == MouseButtons.Right) {
                // if currently drawing something, right-click ends it
                if (PicDrawMode != PicDrawOp.None) {
                    StopDrawing();
                    // cancel context menu
                    CancelContextMenu = true;
                }
                return;
            }
            // calculate cursor position in agi units
            PicPt = new((int)(e.X / (2 * ScaleFactor)), (int)(e.Y / ScaleFactor));
            if (PicPt.X < 0) {
                PicPt.X = 0;
            }
            else if (PicPt.X > 159) {
                PicPt.X = 159;
            }
            if (PicPt.Y < 0) {
                PicPt.Y = 0;
            }
            else if (PicPt.Y > 167) {
                PicPt.Y = 167;
            }

            // take action based on mode/tool
            switch (PicMode) {
            case PicEditorMode.Edit:
                if (SelectedTool != PicToolType.Edit) {
                    if (ModifierKeys == Keys.Shift) {
                        // shift-click to drag the picture
                        StartDrag((PictureBox)sender, e.Location);
                        return;
                    }
                }
                if (PicDrawMode != PicDrawOp.None) {
                    // finish drawing function
                    EndDraw(PicPt);
                    return;
                }
                // what to do depends primarily on what the selected tool is:
                switch (SelectedTool) {
                case PicToolType.Edit:
                    // no tool selected; check for a coordinate being moved or group of commands being moved
                    // if none of those apply, drag the drawing surface

                    // first, see if we need to select the current coordinate:
                    if (CursorMode == CoordinateHighlightType.XMode) {
                        if (CurCursor == PicCursor.Cross && (SelectedCmd.SelectedCoord != PicPt)) {
                            // we are on a coordinate that is NOT the currently selected coordinate!
                            // select it, and then continue
                            for (int i = 0; i < SelectedCmd.Coords.Count; i++) {
                                if (SelectedCmd.Coords[i] == PicPt) {
                                    SelectCoordinate(i);
                                    break;
                                }
                            }
                        }
                    }
                    if (PicPt == SelectedCmd.SelectedCoord) {
                        // three cases; if on any coord and SHIFT key is pressed, then move entire command
                        //              if on any coord and CTRL key is pressed, add a new coord, then begin moving it
                        //              if on any coord and no key is pressed, begin moving just the coord
                        //              (if combo of keys pressed, just ignore)
                        switch (ModifierKeys) {
                        case Keys.None:
                            // begin editing the coordinate
                            PicDrawMode = PicDrawOp.MovePoint;
                            EditPt = PicPt;
                            // get edit cmd
                            EditCmd = SelectedCmd.Type;
                            // turn off cursor flasher
                            tmrSelect.Enabled = false;
                            return;
                        case Keys.Control:
                            // insert a new coord, then begin moving it
                            InsertCoordinate();
                            // begin editing the coordinate
                            PicDrawMode = PicDrawOp.MovePoint;
                            Inserting = true;
                            EditPt = PicPt;
                            EditCmd = SelectedCmd.Type;
                            tmrSelect.Enabled = false;
                            return;
                        case Keys.Shift:
                            // set draw mode to move cmd
                            PicDrawMode = PicDrawOp.MoveCmds;
                            AnchorPT = PicPt;
                            // get start and end coords of selection and show selection
                            SetSelectionBounds(true);
                            // get delta from current point to selstart
                            Delta = PicPt;
                            Delta.Offset(-SelectedRegion.X, -SelectedRegion.Y);
                            SetCursors(PicCursor.Move);
                            return;

                        default:
                            // ignore
                            return;
                        }
                    }
                    else if ((SelectedRegion.Width > 0) && (SelectedRegion.Height > 0)) {
                        // if multiple cmds selected (i.e. the selection size is >0), begin moving them
                        // (need to make sure cmds are selected, and NOT showing a screen grab selection

                        // is cursor within the shape?
                        if (SelectedRegion.Contains(PicPt)) {
                            // set draw mode to move cmd
                            PicDrawMode = PicDrawOp.MoveCmds;
                            // get start and end coords of selection, then draw box around them
                            SetSelectionBounds(true);
                            // set anchor
                            AnchorPT = PicPt;
                            // get delta from current point to selstart
                            Delta = PicPt;
                            Delta.Offset(-SelectedRegion.X, -SelectedRegion.Y);
                            return;
                        }
                        else {
                            if (SelectedCmdCount == 1 && tmrSelect.Enabled) {
                                ClearSelectionBounds();
                                picVisual.Refresh();
                                picPriority.Refresh();
                            }
                            // not moving commands; drag the picture
                            StartDrag((PictureBox)sender, e.Location);
                            return;
                        }
                    }
                    else {
                        // not moving commands; drag the picture
                        StartDrag((PictureBox)sender, e.Location);
                        return;
                    }
                case PicToolType.Line or PicToolType.ShortLine or PicToolType.StepLine:
                    // begin draw operation based on selected tool
                    BeginDraw(SelectedTool, PicPt);
                    break;
                case PicToolType.Fill:
                    // if on a Fill cmd
                    if (SelectedCmd.Type == Fill) {
                        // if cursor hasn't moved, just exit
                        if (AnchorPT == PicPt) {
                            return;
                        }
                        // add this coordinate at current coordindex
                        bytData = new byte[2];
                        bytData[0] = (byte)PicPt.X;
                        bytData[1] = (byte)PicPt.Y;
                        SelectCoordinate(AddCoordToPic(bytData, PicPt, SelectedCmd.SelectedCoordIndex));
                        // save point as anchor
                        AnchorPT = PicPt;
                    }
                    else if (SelectedCmd.Index > 0 && EditPicture.Data[(int)lstCommands.Items[SelectedCmd.Index - 1].Tag] == (byte)Fill) {
                        // add this coordinate to end of preceding fill command
                        bytData = new byte[2];
                        bytData[0] = (byte)PicPt.X;
                        bytData[1] = (byte)PicPt.Y;
                        // adjust selectedcmd to previous command
                        SelectedCmd = GetCommand(SelectedCmd.Index - 1);
                        AddCoordToPic(bytData, PicPt, SelectedCmd.SelectedCoordIndex);
                        // select the previous command (now current)
                        SelectCommand(SelectedCmd.Index, 1, true);
                    }
                    else {
                        // add fill command
                        bytData = new byte[3];
                        bytData[0] = (byte)Fill;
                        bytData[1] = (byte)PicPt.X;
                        bytData[2] = (byte)PicPt.Y;
                        SelectCommand(InsertCommand(bytData, SelectedCmd.Index), 1, true);
                        // save point as anchor
                        AnchorPT = PicPt;
                    }
                    // redraw
                    DrawPicture();
                    break;
                case PicToolType.Plot:
                    // need to bound the x value (AGI has a bug which actually allows
                    // X values to be +1 more than they should; WinAGI enforces the
                    // the actual boundary (other boundaries aren't checked because
                    // AGI automatically adjusts plots away from borders as needed)
                    // TODO: maybe let bug this happen, but provide a warning? 
                    if (PicPt.X > 159 - SelectedCmd.Pen.PlotSize / 2) {
                        PicPt.X = 159 - SelectedCmd.Pen.PlotSize / 2;
                    }
                    // if on a coordinate that is part of a plot cmd
                    if (SelectedCmd.Type == PlotPen) {
                        // only need to add the plot coordinate
                        if (SelectedCmd.Pen.PlotStyle == PlotStyle.Solid) {
                            bytData = new byte[2];
                            bytData[0] = (byte)PicPt.X;
                            bytData[1] = (byte)PicPt.Y;
                        }
                        else {
                            // include a random pattern
                            bytPattern = (byte)(2 * GetRandomByte(0, 119));
                            bytData = new byte[3];
                            bytData[0] = bytPattern;
                            bytData[1] = (byte)PicPt.X;
                            bytData[2] = (byte)PicPt.Y;
                        }
                        // add this coordinate at current coordindex
                        SelectCoordinate(AddCoordToPic(bytData, PicPt, SelectedCmd.SelectedCoordIndex));
                    }
                    else if (SelectedCmd.Index > 0 && EditPicture.Data[(int)lstCommands.Items[SelectedCmd.Index - 1].Tag] == (byte)PlotPen) {
                        // add this coordinate to end of preceding plot command
                        if (SelectedCmd.Pen.PlotStyle == PlotStyle.Solid) {
                            bytData = new byte[2];
                            bytData[0] = (byte)PicPt.X;
                            bytData[1] = (byte)PicPt.Y;
                        }
                        else {
                            // include a random pattern
                            bytPattern = (byte)(2 * GetRandomByte(0, 119));
                            bytData = new byte[3];
                            bytData[0] = bytPattern;
                            bytData[1] = (byte)PicPt.X;
                            bytData[2] = (byte)PicPt.Y;
                        }
                        // adjust selectedcmd to previous command
                        SelectedCmd = GetCommand(SelectedCmd.Index - 1);
                        SelectedCmd.SelectedCoordIndex = SelectedCmd.Coords.Count - 1;
                        AddCoordToPic(bytData, PicPt, SelectedCmd.SelectedCoordIndex);
                        // select the previuos command (now current)
                        SelectCommand(SelectedCmd.Index, 1, true);
                    }
                    else {
                        // if not already in a plot command, the plot command
                        // needs to be included with the plot coordinates
                        if (SelectedCmd.Pen.PlotStyle == PlotStyle.Solid) {
                            bytData = new byte[3];
                            bytData[0] = (byte)PlotPen;
                            bytData[1] = (byte)PicPt.X;
                            bytData[2] = (byte)PicPt.Y;
                        }
                        else {
                            bytData = new byte[4];
                            // include a random pattern
                            bytPattern = (byte)(2 * GetRandomByte(0, 119));
                            bytData[0] = (byte)PlotPen;
                            bytData[1] = bytPattern;
                            bytData[2] = (byte)PicPt.X;
                            bytData[3] = (byte)PicPt.Y;
                        }
                        // addand select new command
                        SelectCommand(InsertCommand(bytData, SelectedCmd.Index), 1, true);
                        // save point as anchor
                        AnchorPT = PicPt;
                    }
                    DrawPicture();
                    break;
                case PicToolType.Rectangle:
                case PicToolType.Trapezoid:
                    // set anchor
                    AnchorPT = PicPt;
                    // set mode
                    PicDrawMode = PicDrawOp.Shape;
                    break;
                case PicToolType.Ellipse:
                    // set anchor
                    AnchorPT = PicPt;
                    // set mode
                    PicDrawMode = PicDrawOp.Shape;
                    // reset arc segments
                    ArcPts = [];
                    break;
                case PicToolType.SelectArea:
                    // if shift key, drag the picture
                    switch (ModifierKeys) {
                    case Keys.None:
                        // begin selecting an area
                        PicDrawMode = PicDrawOp.SelectArea;
                        // reset selection
                        ClearSelectionBounds();
                        AnchorPT = PicPt;
                        SelectedRegion.Location = PicPt;
                        SelectedRegion.Size = new(1, 1);
                        tmrSelect.Enabled = true;
                        spAnchor.Text = "Anchor: " + SelectedRegion.X + ", " + SelectedRegion.Y;
                        spBlock.Text = "Block: " + SelectedRegion.X + ", " + SelectedRegion.Y + ", " + (SelectedRegion.Right - 1) + ", " + (SelectedRegion.Bottom - 1);
                        spAnchor.Visible = true;
                        spBlock.Visible = true;
                        picVisual.Refresh();
                        picPriority.Refresh();
                        break;
                    case Keys.Shift:
                        // not moving commands; drag the picture
                        StartDrag((PictureBox)sender, e.Location);
                        break;
                    }
                    break;
                }
                break;
            case PicEditorMode.ViewTest:
                // if shift key is down, start drag
                if (ModifierKeys == Keys.Shift) {
                    StartDrag((PictureBox)sender, e.Location);
                    return;
                }
                // stop testview object motion
                TestDir = 0;
                tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                // if above top edge OR
                //    (NOT ignoring horizon AND above horizon ) OR
                //    (on water AND restricting to land) OR
                //    (NOT on water AND restricting to water)
                // PicTest.ObjRestriction: 0 = no restriction, 1 = restrict to water, 2 = restrict to land
                if ((PicPt.Y - (CelHeight - 1) < 0) || (PicPt.Y < TestSettings.Horizon.Value && !TestSettings.IgnoreHorizon.Value) ||
                   (TestSettings.ObjRestriction.Value == 2 && EditPicture.ObjOnWater(PicPt, CelWidth)) ||
                  (TestSettings.ObjRestriction.Value == 1 && !EditPicture.ObjOnWater(PicPt, CelWidth))) {
                    return;
                }
                // draw testview in new location
                TestCelPos = PicPt;
                ShowTestCel = true;
                picVisual.Refresh();
                picPriority.Refresh();
                break;
            case PicEditorMode.PrintTest:
                // if shift key is down, start drag
                if (ModifierKeys == Keys.Shift) {
                    StartDrag((PictureBox)sender, e.Location);
                    return;
                }
                if (ShowPrintTest) {
                    ShowPrintTest = false;
                    picVisual.Refresh();
                }
                break;
            }
        }

        private void DrawSurface_MouseMove(object sender, MouseEventArgs e) {
            Point tmpPt = new(0, 0), SelAnchor = new(0, 0);
            if (blnDragging) {
                switch (((PictureBox)sender).Name) {
                case "picVisual":
                    int newL = picVisual.Left + e.X - DragPT.X;
                    if (hsbVisual.Visible) {
                        if (newL < -(hsbVisual.Maximum - hsbVisual.LargeChange + 1)) {
                            newL = -(hsbVisual.Maximum - hsbVisual.LargeChange + 1);
                        }
                        if (newL > 5) {
                            newL = 5;
                        }
                        picVisual.Left = newL;
                        hsbVisual.Value = -newL;
                    }
                    int newT = picVisual.Top + e.Y - DragPT.Y;
                    if (vsbVisual.Visible) {
                        if (newT < -(vsbVisual.Maximum - vsbVisual.LargeChange + 1)) {
                            newT = -(vsbVisual.Maximum - vsbVisual.LargeChange + 1);
                        }
                        if (newT > 5) {
                            newT = 5;
                        }
                        picVisual.Top = newT;
                        vsbVisual.Value = -newT;
                    }
                    return;
                case "picPriority":
                    newL = picPriority.Left + e.X - DragPT.X;
                    if (hsbPriority.Visible) {
                        if (newL < -(hsbPriority.Maximum - hsbPriority.LargeChange + 1)) {
                            newL = -(hsbPriority.Maximum - hsbPriority.LargeChange + 1);
                        }
                        if (newL > 5) {
                            newL = 5;
                        }
                        picPriority.Left = newL;
                        hsbPriority.Value = -newL;
                    }
                    newT = picPriority.Top + e.Y - DragPT.Y;
                    if (vsbPriority.Visible) {
                        if (newT < -(vsbPriority.Maximum - vsbPriority.LargeChange + 1)) {
                            newT = -(vsbPriority.Maximum - vsbPriority.LargeChange + 1);
                        }
                        if (newT > 5) {
                            newT = 5;
                        }
                        picPriority.Top = newT;
                        vsbPriority.Value = -newT;
                    }
                    return;
                }
            }

            // calculate agi coordinates
            tmpPt.X = (int)(e.X / (2 * ScaleFactor));
            tmpPt.Y = (int)(e.Y / ScaleFactor);
            if (tmpPt.X < 0) {
                tmpPt.X = 0;
            }
            else if (tmpPt.X > 159) {
                tmpPt.X = 159;
            }
            if (tmpPt.Y < 0) {
                tmpPt.Y = 0;
            }
            else if (tmpPt.Y > 167) {
                tmpPt.Y = 167;
            }
            // if no change, do nothing
            if (tmpPt == PicPt) {
                return;
            }
            PicPt = tmpPt;

            switch (PicMode) {
            case PicEditorMode.Edit:
                // in edit mode, action taken depends primarily on
                // current draw operation mode
                switch (PicDrawMode) {
                case PicDrawOp.None:
                    // not drawing anything
                    switch (SelectedTool) {
                    case PicToolType.Edit:
                        // not drawing anything, but there could be a highlighted coordinate
                        // or a selected group of commands- cursor will depend on which of
                        // those states exist

                        // if selection is visible and tool=none, then must be moving cmds
                        if ((SelectedRegion.Width > 0) && (SelectedRegion.Height > 0)) {
                            // is cursor over cmds?
                            if (SelectedRegion.Contains(PicPt)) {
                                // use move cursor
                                SetCursors(PicCursor.Move);
                            }
                            else {
                                // use normal 'edit' cursor
                                SetCursors(PicCursor.Default);
                            }
                        }
                        else {
                            // check for editing coordinate
                            if (SelectedCmd.SelectedCoord == PicPt) {
                                SetCursors(PicCursor.Cross);
                                OnPoint = true;
                                if (CursorMode == CoordinateHighlightType.FlashBox) {
                                    tmrSelect.Enabled = false;
                                }
                                else {
                                    tmrSelect.Enabled = true;
                                }
                            }
                            else {
                                if (CursorMode == CoordinateHighlightType.XMode) {
                                    // check to see if cursor is over one of the other coordinates
                                    // if cursor is over one of the coord points
                                    bool found = false;
                                    for (int i = 0; i < SelectedCmd.Coords.Count; i++) {
                                        if (PicPt == SelectedCmd.Coords[i]) {
                                            // this is one of the vertices; can't be the currently selected one - that would
                                            // have been detected already
                                            SetCursors(PicCursor.Cross);
                                            found = true;  // so we can tell if loop exited due to finding a point
                                            break; // exit for
                                        }
                                    }
                                    if (!found) {
                                        // nothing going on- use normal cursor
                                        SetCursors(PicCursor.Default);
                                        OnPoint = false;
                                    }
                                }
                                else {
                                    // nothing going on- use normal cursor
                                    SetCursors(PicCursor.Default);
                                    OnPoint = false;
                                }
                            }
                        }
                        break;
                    }
                    break;
                case PicDrawOp.SelectArea:
                    // adjust selection bounds to match current mouse location
                    SelAnchor.X = PicPt.X < AnchorPT.X ? PicPt.X : AnchorPT.X;
                    SelAnchor.Y = PicPt.Y < AnchorPT.Y ? PicPt.Y : AnchorPT.Y;
                    // set selection parameters to match current selected area
                    SelectedRegion.Location = SelAnchor;
                    SelectedRegion.Width = Math.Abs(PicPt.X - AnchorPT.X) + 1;
                    SelectedRegion.Height = Math.Abs(PicPt.Y - AnchorPT.Y) + 1;
                    picVisual.Refresh();
                    picPriority.Refresh();
                    spAnchor.Text = "Anchor: " + SelectedRegion.X + ", " + SelectedRegion.Y;
                    spBlock.Text = "Block: " + SelectedRegion.X + ", " + SelectedRegion.Y + ", " + (SelectedRegion.Right - 1) + ", " + (SelectedRegion.Bottom - 1);
                    break;
                case PicDrawOp.Line:
                case PicDrawOp.Shape:
                case PicDrawOp.MoveCmds:
                case PicDrawOp.MovePoint:
                    // only need to do something if cursor position has changed
                    // since last time drawing surface was updated

                    // if coordinates have changed,
                    if (EditPt != PicPt) {
                        // take action as appropriate
                        switch (PicDrawMode) {
                        case PicDrawOp.Line:
                            // action to take depends on what Type of line is being drawn
                            switch (SelectedTool) {
                            case PicToolType.Line:
                                break;
                            case PicToolType.ShortLine:
                                // validate x and Y
                                // (note that delta x is limited to -6 to avoid
                                // values above 0xF0, which would mistakenly be interpreted
                                // as a new command)
                                if (PicPt.X > AnchorPT.X + 7) {
                                    PicPt.X = AnchorPT.X + 7;
                                }
                                else if (PicPt.X < AnchorPT.X - 6) {
                                    PicPt.X = AnchorPT.X - 6;
                                }
                                if (PicPt.Y > AnchorPT.Y + 7) {
                                    PicPt.Y = AnchorPT.Y + 7;
                                }
                                else if (PicPt.Y < AnchorPT.Y - 7) {
                                    PicPt.Y = AnchorPT.Y - 7;
                                }
                                break;
                            case PicToolType.StepLine:
                                // if drawing second point
                                if (lstCoords.Items.Count == 1) {
                                    // if mostly vertical
                                    if (Math.Abs(PicPt.X - AnchorPT.X) < Math.Abs(PicPt.Y - AnchorPT.Y)) {
                                        // change command to Y corner
                                        if (SelectedCmd.Type != YCorner) {
                                            SelectedCmd.Type = YCorner;
                                            lstCommands.Items[SelectedCmd.Index].Text = "Y Corner";
                                            EditPicture.Data[SelectedCmd.Position] = (byte)YCorner;
                                            // correct last undo
                                            UndoCol.Peek().DrawCommand = YCorner;
                                        }
                                        // limit change to vertical direction only
                                        PicPt.X = AnchorPT.X;
                                    }
                                    else {
                                        // command should be X corner
                                        if (SelectedCmd.Type != XCorner) {
                                            SelectedCmd.Type = XCorner;
                                            lstCommands.Items[SelectedCmd.Index].Text = "X Corner";
                                            EditPicture.Data[SelectedCmd.Position] = (byte)XCorner;
                                            // correct last undo
                                            UndoCol.Peek().DrawCommand = XCorner;
                                        }
                                        // limit change to horizontal direction only
                                        PicPt.Y = AnchorPT.Y;
                                    }
                                }
                                else {
                                    // determine which direction to allow movement
                                    if ((SelectedCmd.Type == XCorner && lstCoords.Items.Count.IsEven()) ||
                                        (SelectedCmd.Type == YCorner && lstCoords.Items.Count.IsOdd())) {
                                        // limit change to vertical direction
                                        PicPt.X = AnchorPT.X;
                                    }
                                    else {
                                        // limit change to horizontal direction
                                        PicPt.Y = AnchorPT.Y;
                                    }
                                }
                                break;
                            }
                            break;
                        case PicDrawOp.Shape:
                            if (SelectedTool == PicToolType.Ellipse) {
                                if (EditPt != PicPt) {
                                    // rebuild arc segments
                                    ArcPts = BuildCircleArcs(AnchorPT, PicPt);
                                }
                            }
                            break;
                        case PicDrawOp.MoveCmds:
                            // limit selection box movement to stay within picture bounds
                            if (EditPt.X - Delta.X < 0) {
                                EditPt.X = Delta.X;
                            }
                            else if (EditPt.X - Delta.X + SelectedRegion.Width > 160) {
                                EditPt.X = 160 - SelectedRegion.Width + Delta.X;
                            }
                            if (EditPt.Y - Delta.Y < 0) {
                                EditPt.Y = Delta.Y;
                            }
                            else if (EditPt.Y - Delta.Y + SelectedRegion.Height > 168) {
                                EditPt.Y = 168 - SelectedRegion.Height + Delta.Y;
                            }
                            // now adjust selection start pos to match new location, then move selection box
                            SelectedRegion.X = EditPt.X - Delta.X;
                            SelectedRegion.Y = EditPt.Y - Delta.Y;
                            break;
                        case PicDrawOp.MovePoint:
                            // limit motion of short lines
                            if (SelectedCmd.Type == RelLine) {
                                // validate x and Y
                                if (SelectedCmd.Coords.Count > 1) {
                                    // if not first point
                                    if (SelectedCmd.SelectedCoordIndex > 0) {
                                        // validate against previous point
                                        Point tmpPrevPT = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex - 1];
                                        // validate x and Y against previous pt
                                        // (note that delta x is limited to -6 to avoid
                                        // values above 0xF0, which would mistakenly be interpreted
                                        // as a new command)
                                        if (PicPt.X > tmpPrevPT.X + 7) {
                                            PicPt.X = tmpPrevPT.X + 7;
                                        }
                                        else if (PicPt.X < tmpPrevPT.X - 6) {
                                            PicPt.X = tmpPrevPT.X - 6;
                                        }
                                        if (PicPt.Y > tmpPrevPT.Y + 7) {
                                            PicPt.Y = tmpPrevPT.Y + 7;
                                        }
                                        else if (PicPt.Y < tmpPrevPT.Y - 7) {
                                            PicPt.Y = tmpPrevPT.Y - 7;
                                        }
                                    }
                                    // if not last point (next pt is not a new cmd)
                                    if (SelectedCmd.SelectedCoordIndex < SelectedCmd.Coords.Count - 1) {
                                        // validate against next point
                                        // note that delta x is limited to +6 (swapped because we are
                                        // comparing against NEXT vs. PREVIOUS coordinate)
                                        // for same reason as given above
                                        Point tmpNextPT = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex + 1];
                                        if (PicPt.X > tmpNextPT.X + 6) {
                                            PicPt.X = tmpNextPT.X + 6;
                                        }
                                        else if (PicPt.X < tmpNextPT.X - 7) {
                                            PicPt.X = tmpNextPT.X - 7;
                                        }
                                        if (PicPt.Y > tmpNextPT.Y + 7) {
                                            PicPt.Y = tmpNextPT.Y + 7;
                                        }
                                        else if (PicPt.Y < tmpNextPT.Y - 7) {
                                            PicPt.Y = tmpNextPT.Y - 7;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                        // if move actions still result in a change of the current
                        // edit point, draw surface needs to be refreshed
                        if (EditPt != PicPt) {
                            EditPt = PicPt;
                            //// refresh pictures
                            picVisual.Refresh();
                            picPriority.Refresh();
                        }
                    }
                    break;
                case PicDrawOp.Fill:
                    // no action required
                    break;
                }
                break;
            case PicEditorMode.ViewTest:
                // in test mode, all we need to do is provide feeback to user
                // on whether or not it's OK to drop the test object at this location

                // if above top edge OR
                //    (NOT ignoring horizon AND above horizon ) OR
                //    (on water AND restricting to land) OR
                //    (NOT on water AND restricting to water)
                if ((PicPt.Y - (CelHeight - 1) < 0) || (PicPt.Y < TestSettings.Horizon.Value && !TestSettings.IgnoreHorizon.Value) ||
                   (EditPicture.ObjOnWater(PicPt, CelWidth) && TestSettings.ObjRestriction.Value == 2) ||
                  (!EditPicture.ObjOnWater(PicPt, CelWidth) && TestSettings.ObjRestriction.Value == 1)) {
                    // set cursor to NO
                    SetCursors(PicCursor.NoOp);
                }
                else {
                    // set cursor to normal
                    SetCursors(PicCursor.Default);
                }
                break;
            }

            // when moving mouse, we always need to update the status bar
            // if NOT in test mode OR status bar is NOT testcoord mode
            if (PicMode == PicEditorMode.Edit || StatusMode != PicStatusMode.Coord) {
                int NewPri = 0;
                switch (StatusMode) {
                case PicStatusMode.Pixel:
                    //  normal pixel mode
                    spCurX.Text = "X: " + PicPt.X;
                    spCurY.Text = "Y: " + PicPt.Y;
                    NewPri = GetPriBand((byte)PicPt.Y, EditPicture.PriBase);
                    spPriBand.Text = "Band: " + NewPri;
                    if (SelectedTool == PicToolType.SelectArea) {
                        if (SelectedRegion.Width > 0 && SelectedRegion.Height > 0) {
                            if (e.Button == MouseButtons.Left) {
                                spAnchor.Text = "Anchor: " + SelectedRegion.X + ", " + SelectedRegion.Y;
                                spBlock.Text = "Block: " + SelectedRegion.X + ", " + SelectedRegion.Y + ", " + (SelectedRegion.Right - 1) + ", " + (SelectedRegion.Bottom - 1);
                            }
                        }
                    }
                    break;
                case PicStatusMode.Coord:
                    //  testcoord mode (never updated by mousemove)
                    break;
                case PicStatusMode.Text:
                    //  text row/col mode
                    spCurX.Text = "R: " + (PicPt.Y / 8).ToString();
                    spCurY.Text = "C: " + (PicPt.X / (PTInfo.CharWidth / 2)).ToString();
                    NewPri = GetPriBand((byte)PicPt.Y, EditPicture.PriBase);
                    spPriBand.Text = "Band: " + NewPri;
                    break;
                }

                // if priority has changed, update the color box
                if (NewPri != OldPri) {
                    Bitmap bitmap = new(12, 12);
                    using Graphics g = Graphics.FromImage(bitmap);
                    g.Clear(EditPalette[NewPri]);
                    spPriBand.Image = bitmap;
                }
                OldPri = NewPri;
            }
        }

        private void DrawSurface_MouseUp(object sender, MouseEventArgs e) {
            if (blnDragging) {
                blnDragging = false;
                SetCursors(PicCursor.Default);
                return;
            }

            // calculate position
            Point PicPt = new(0, 0);
            PicPt.X = (int)(e.X / (2 * ScaleFactor));
            PicPt.Y = (int)(e.Y / ScaleFactor);
            if (PicPt.X < 0) {
                PicPt.X = 0;
            }
            else if (PicPt.X > 159) {
                PicPt.X = 159;
            }
            if (PicPt.Y < 0) {
                PicPt.Y = 0;
            }
            else if (PicPt.Y > 167) {
                PicPt.Y = 167;
            }
            // if print preview, ignore
            if (PicMode == PicEditorMode.PrintTest) {
                return;
            }
            // how to handle mouseup event depends primarily on what was being drawn (or not)
            switch (PicDrawMode) {
            //case PicDrawOp.None:
                // no action required
                //break;
            //case Line or Fill or Shape:
                // lines and shapes are not completed on mouse_up actions; they
                // are done by clicking to start, then clicking again to end
                // so it's the mouse-down action that both starts and ends the operation
                // that's why we don't need to check for them here in the MouseUp event
                //break;
            case PicDrawOp.SelectArea:
                // reset the draw mode
                PicDrawMode = PicDrawOp.None;
                break;
            case PicDrawOp.MovePoint:
                // editing a coordinate
                // reset drawmode
                PicDrawMode = PicDrawOp.None;
                if (PicPt == SelectedCmd.SelectedCoord) {
                    // no change; just exit
                    return;
                }
                // edit the coordinate
                int index = SelectedCmd.SelectedCoordIndex;
                EndEditCoord(EditCmd, index, PicPt);
                // update by re-building coordlist, and selecting
                SelectCommand(SelectedCmd.Index, 1, true);
                SelectCoordinate(index);
                break;
            case PicDrawOp.MoveCmds:
                // reset drawmode
                PicDrawMode = PicDrawOp.None;
                // limit selection box movement to stay within picture bounds
                if (PicPt.X - Delta.X < 0) {
                    PicPt.X = Delta.X;
                }
                else if (PicPt.X - Delta.X + SelectedRegion.Width > 160) {
                    PicPt.X = 160 - SelectedRegion.Width + Delta.X;
                }
                if (PicPt.Y - Delta.Y < 0) {
                    PicPt.Y = Delta.Y;
                }
                else if (PicPt.Y - Delta.Y + SelectedRegion.Height > 168) {
                    PicPt.Y = 168 - SelectedRegion.Height + Delta.Y;
                }
                if (PicPt.X - AnchorPT.X != 0 || PicPt.Y - AnchorPT.Y != 0) {
                    // move the command(s)
                    MoveCommands(SelectedCmd.Index, SelectedCmdCount, PicPt.X - AnchorPT.X, PicPt.Y - AnchorPT.Y);
                    // if a single cmd was being moved,
                    if (SelectedCmdCount == 1) {
                        // update by re-building coordlist, and selecting
                        SelectCommand(SelectedCmd.Index, SelectedCmdCount, true);
                        // keep highlighting single commands until something else selected
                        SetSelectionBounds(true);
                    }
                    else {
                        // update by redrawing
                        DrawPicture();
                        // reselect commands, then show selection box
                        SetSelectionBounds(true);
                    }
                }
                picVisual.Refresh();
                picPriority.Refresh();
                // restore cursor
                SetCursors(PicCursor.Default);
                break;
            }
        }

        private void DrawSurface_MouseLeave(object sender, EventArgs e) {
            // hide cursor location information when mouse is not over
            // the draw surface
            if (StatusMode != PicStatusMode.Coord) {
                spPriBand.Text = "";
                spPriBand.Image = null;
                spCurX.Text = "";
                spCurY.Text = "";
            }
        }

        private void picVisual_DoubleClick(object sender, EventArgs e) {
            // in print-test mode, double-click to show the options dialog
            if (PicMode == PicEditorMode.PrintTest) {
                GetTextOptions();
            }
        }

        private void picVisual_Paint(object sender, PaintEventArgs e) {
            // add supporting information to the visual picture (priority 
            // bands, text marks, temporary lines, etc

            // but only if being displayed
            if ((OneWindow & WindowMode.Visual) != WindowMode.Visual) {
                return;
            }
            Graphics g = e.Graphics;
            switch (PicMode) {
            case PicEditorMode.Edit:
                // first, draw temporary lines to support current edit mode/tool
                if (SelectedCmd.Pen.VisColor < AGIColorIndex.None) {
                    if (SelectedCmd.SelectedCoordIndex >= 0 && SelectedCmd.IsLine) {
                        DrawTempSegments(g, EditPalette[(int)SelectedCmd.Pen.VisColor]);
                    }
                    else {
                        switch (PicDrawMode) {
                        case PicDrawOp.Line:
                            DrawLineOnImage(g, EditPalette[(int)SelectedCmd.Pen.VisColor], SelectedCmd.Coords[^1], PicPt);
                            break;
                        case PicDrawOp.Shape:
                            switch (SelectedTool) {
                            case PicToolType.Rectangle:
                                // simulate a rectangle
                                DrawBox(g, EditPalette[(int)SelectedCmd.Pen.VisColor], AnchorPT, EditPt);
                                break;
                            case PicToolType.Trapezoid:
                                // simulate a trapezoid
                                DrawTrapezoid(g, EditPalette[(int)SelectedCmd.Pen.VisColor], AnchorPT, EditPt);
                                break;
                            case PicToolType.Ellipse:
                                // simulate circle
                                DrawCircle(g, EditPalette[(int)SelectedCmd.Pen.VisColor], AnchorPT, EditPt);
                                break;
                            }
                            break;
                        }
                    }
                }
                break;
            case PicEditorMode.ViewTest:
                // add test cel if in preview mode, and a test view is loaded
                if (TestView is not null && ShowTestCel) {
                    AddCelToPic(g, true);
                }
                break;
            case PicEditorMode.PrintTest:
                // add print tests after bands and text marks
                break;
            }

            // priority bands are added next
            if (ShowBands) {
                for (int rtn = 5; rtn <= 14; rtn++) {
                    int yp = (int)((int)(Math.Ceiling((rtn - 5) / 10.0 * (168 - EditPicture.PriBase)) + EditPicture.PriBase) * ScaleFactor - 1);
                    g.DrawLine(new(EditPalette[rtn]), 0, yp, picVisual.Width, yp);
                }
            }

            // text marks indicate where text characters are drawn
            if (ShowTextMarks) {
                for (int j = 1; j <= 21; j++) {
                    for (int i = 0; i <= PTInfo.MaxCol; i++) {
                        int x = (int)(i * PTInfo.CharWidth * ScaleFactor);
                        int y = (int)(j * 8 * ScaleFactor - 1);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, (int)(x + ScaleFactor), y);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, x, (int)(y - ScaleFactor));
                    }
                }
            }

            // if selection region is non-zero, show an animated border around
            // the selection; or if coordinates need to be highlighted,
            // then highlight them
            if (SelectedRegion.Width > 0 && SelectedRegion.Height > 0) {
                Rectangle rgn = new((int)(SelectedRegion.X * ScaleFactor * 2),
                    (int)(SelectedRegion.Y * ScaleFactor),
                    (int)(SelectedRegion.Width * ScaleFactor * 2) - 1,
                    (int)(SelectedRegion.Height * ScaleFactor) - 1);
                g.DrawRectangle(dash1, rgn);
                g.DrawRectangle(dash2, rgn);
            }
            else {
                // if only one selected command AND it has coords AND tool is
                // 'none' AND in edit mode
                if (
                    PicMode == PicEditorMode.Edit &&
                    PicDrawMode == PicDrawOp.None &&
                    SelectedCmdCount == 1 &&
                    SelectedCmd.Type > DisablePri &&
                    SelectedCmd.Type != ChangePen &&
                    SelectedCmd.Coords.Count > 0 &&
                    SelectedTool == PicToolType.Edit &&
                    SelectedCmd.Pen.VisColor < AGIColorIndex.None) {
                    HighlightCoords(g, VCColor);
                }
            }
            // finally, if displaying any preview text, add it over all
            // other graphics elements
            if (PicMode == PicEditorMode.PrintTest) {
                if (ShowPrintTest) {
                    DrawPrintTest(g);
                }
            }
        }

        private void picPriority_Paint(object sender, PaintEventArgs e) {
            // add supporting information to the priority picture (priority 
            // bands, text marks, temporary lines, etc

            // but only if being displayed
            if ((OneWindow & WindowMode.Priority) != WindowMode.Priority) {
                return;
            }
            Graphics g = e.Graphics;
            switch (PicMode) {
            case PicEditorMode.Edit:
                // first, draw temporary lines to support current edit mode/tool
                if (SelectedCmd.Pen.PriColor < AGIColorIndex.None) {
                    if (SelectedCmd.SelectedCoordIndex >= 0 && SelectedCmd.IsLine) {
                        DrawTempSegments(g, EditPalette[(int)SelectedCmd.Pen.PriColor]);
                    }
                    else {
                        switch (PicDrawMode) {
                        case PicDrawOp.Line:
                            DrawLineOnImage(g, EditPalette[(int)SelectedCmd.Pen.PriColor], SelectedCmd.Coords[^1], PicPt);
                            break;
                        case PicDrawOp.Shape:
                            switch (SelectedTool) {
                            case PicToolType.Rectangle:
                                // simulate a rectangle
                                DrawBox(g, EditPalette[(int)SelectedCmd.Pen.PriColor], AnchorPT, EditPt);
                                break;
                            case PicToolType.Trapezoid:
                                // simulate a trapezoid
                                DrawTrapezoid(g, EditPalette[(int)SelectedCmd.Pen.PriColor], AnchorPT, EditPt);
                                break;
                            case PicToolType.Ellipse:
                                // simulate circle
                                DrawCircle(g, EditPalette[(int)SelectedCmd.Pen.PriColor], AnchorPT, EditPt);
                                break;
                            }
                            break;
                        }
                    }
                }
                break;
            case PicEditorMode.ViewTest:
                // add test cel if in preview mode, and a test view is loaded
                if (TestView is not null && ShowTestCel) {
                    AddCelToPic(g, false);
                }
                break;
            case PicEditorMode.PrintTest:
                // no action required
                break;
            }

            // priority bands are added next
            if (ShowBands) {
                for (int rtn = 5; rtn <= 14; rtn++) {
                    int yp = (int)((int)(Math.Ceiling((rtn - 5) / 10.0 * (168 - EditPicture.PriBase)) + EditPicture.PriBase) * ScaleFactor - 1);
                    g.DrawLine(new(EditPalette[rtn]), 0, yp, picPriority.Width, yp);
                }
            }

            // text marks indicate where text characters are drawn
            if (ShowTextMarks) {
                for (int j = 1; j <= 21; j++) {
                    for (int i = 0; i <= PTInfo.MaxCol; i++) {
                        int x = (int)(i * PTInfo.CharWidth * ScaleFactor);
                        int y = (int)(j * 8 * ScaleFactor - 1);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, (int)(x + ScaleFactor), y);
                        g.DrawLine(new(Color.FromArgb(170, 170, 0)), x, y, x, (int)(y - ScaleFactor));
                    }
                }
            }

            // if selection region is non-zero, show an animated border around
            // the selection; or if coordinates need to be highlighted,
            // then highlight them
            if (SelectedRegion.Width > 0 && SelectedRegion.Height > 0) {
                Rectangle rgn = new((int)(SelectedRegion.X * ScaleFactor * 2),
                    (int)(SelectedRegion.Y * ScaleFactor),
                    (int)(SelectedRegion.Width * ScaleFactor * 2) - 1,
                    (int)(SelectedRegion.Height * ScaleFactor) - 1);
                g.DrawRectangle(dash1, rgn);
                g.DrawRectangle(dash2, rgn);
            }
            else {
                // if only one selected command AND it has coords AND tool is
                // 'none' AND in edit mode
                if (
                      PicMode == PicEditorMode.Edit &&
                      PicDrawMode == PicDrawOp.None &&
                      SelectedCmdCount == 1 &&
                      SelectedCmd.Type > DisablePri &&
                      SelectedCmd.Type != ChangePen &&
                      SelectedCmd.Coords.Count > 0 &&
                      SelectedTool == PicToolType.Edit &&
                      SelectedCmd.Pen.PriColor < AGIColorIndex.None) {
                    HighlightCoords(g, PCColor);
                }
            }
            // preview text is never displayed on priority screen
        }

        private void lstCommands_SizeChanged(object sender, EventArgs e) {
            if (!Visible) {
                return;
            }
            lstCommands.BeginUpdate();
            lstCommands.Columns[0].Width = lstCommands.ClientSize.Width;
            lstCommands.EndUpdate();
        }

        private void lstCommands_MouseClick(object sender, MouseEventArgs e) {
            int clickeditem = (int)(lstCommands.HitTest(new(e.X, e.Y)).Item?.Index);

            // multiple selections must always be sequential;
            // if any multiple selection is made to select them
            // out of order, de-select all but the last one selected

            if (lstCommands.SelectedItems.Count > 1) {
                int index = lstCommands.SelectedIndices[0];
                for (int i = 1; i < lstCommands.SelectedItems.Count; i++) {
                    if (lstCommands.SelectedIndices[i] != index + i) {
                        // non-sequential, deselect all but the one clicked
                        lstCommands.SelectedItems.Clear();
                        lstCommands.Items[clickeditem].Selected = true;
                        break;
                    }
                }
            }

            // end command not allowed in multiple selections
            if (lstCommands.SelectedItems.Count > 1 && lstCommands.Items[^1].Selected) {
                lstCommands.Items[^1].Selected = false;
            }

            // plots and fills need to force selection if respective tool is active
            bool force = false;
            if (SelectedCmdCount == 1) {
                if ((SelectedTool == PicToolType.Plot && EditPicture.Data[(int)lstCommands.SelectedItems[0].Tag] == (int)PlotPen) ||
                    (SelectedTool == PicToolType.Fill && EditPicture.Data[(int)lstCommands.SelectedItems[0].Tag] == (int)Fill)) {
                    force = true;
                }
            }
            // update draw surfaces to reflect selected commands
            UpdateCmdSelection(lstCommands.SelectedItems[^1].Index, force);
            // always clear the coordinate list when a command is selected from
            // the command list
            if (SelectedCmd.SelectedCoordIndex >= 0) {
                SelectedCmd.SelectedCoordIndex = -1;
                lstCoords.SelectedItems.Clear();
            }
        }

        private void lstCommands_MouseUp(object sender, MouseEventArgs e) {
            // if user clicks outside the boundary of the control's column
            // the selection gets cleared, so we need to force the control
            // to re-select the currently focused item
            if (lstCommands.SelectedItems.Count == 0) {
                lstCommands.FocusedItem.Selected = true;
                UpdateCmdSelection(lstCommands.SelectedItems[^1].Index);
            }
        }

        private void lstCommands_MouseDoubleClick(object sender, MouseEventArgs e) {
            // on double-click, draw the selection rectangle around the entire
            // command
            SetSelectionBounds(true);
            picVisual.Refresh();
            picPriority.Refresh();
        }

        private void lstCommands_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore all keypresses
            e.Handled = true;
        }

        private void lstCommands_KeyUp(object sender, KeyEventArgs e) {
            // when using keyboard to move the selection, the 
            // draw surfaces also need to be updated
            switch (e.KeyCode) {
            case Keys.Down:
            case Keys.Up:
            case Keys.PageDown:
            case Keys.PageUp:
            case Keys.Home:
            case Keys.End:
                UpdateCmdSelection(lstCommands.SelectedIndices[^1]);
                break;
            }
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void lstCoords_MouseUp(object sender, MouseEventArgs e) {
            // if user clicks outside the boundary of the control's column
            // the selection gets cleared, so we need to force the control
            // to re-select the currently focused item
            if (lstCoords.SelectedItems.Count == 0) {
                if (lstCoords.FocusedItem is not null) {
                    lstCoords.FocusedItem.Selected = true;
                }
                else if (lstCoords.Items.Count > 0) {
                    lstCoords.Items[0].Selected = true;
                }
            }
        }

        private void lstCoords_MouseClick(object sender, MouseEventArgs e) {
            // if a coordinate is clicked (which can only happen on 
            // non-pen commands) select it
            if (!SelectedCmd.IsPen && lstCoords.SelectedItems.Count != 0) {
                SelectCoordinate(lstCoords.SelectedItems[0].Index);
            }
        }

        private void lstCoords_MouseDoubleClick(object sender, MouseEventArgs e) {
            // double-clicking a coordinate will display the coordinate edit form
            // so user can set values manually

            byte[] bytOldCoord = [];
            byte bytNewPattern = 0;

            if (lstCoords.SelectedItems.Count != 1) {
                return;
            }

            // editable coords include Plot, Abs Line, X Corner, Y Corner, Fill
            switch (SelectedCmd.Type) {
            case XCorner:
            case YCorner:
            case AbsLine:
            case Fill:
            case PlotPen:
                // ok to edit
                break;
            case RelLine:
                // can't edit rel lines, because it would be too hard to
                // enforce distance limits
                MessageBox.Show(MDIMain,
                    "Relative Line coordinates cannot be manually edited because " +
                    "of the need to enforce distance limits.",
                    "Edit Coordinates",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\pictures.htm#f7");
                return;
            default:
                // any other type, just exit
                return;
            }
            // configure the plot edit form
            frmPlotEdit frm = new(SelectedCmd, EditPicture);
            if (frm.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return;
            }
            // get the new cursor point
            Point newPT = frm.NewCoord;
            // retrieve the new plot pattern Value
            bytNewPattern = (byte)(frm.NewPattern * 2);
            frm.Dispose();
            // create undo
            PictureUndo NextUndo = new() {
                Action = EditCoord,
                DrawCommand = SelectedCmd.Type,
                PicPos = SelectedCmd.SelectedCoordPos,
                CmdIndex = SelectedCmd.Index,
                CoordIndex = SelectedCmd.SelectedCoordIndex,
            };
            switch (SelectedCmd.Type) {
            case XCorner:
                if (SelectedCmd.SelectedCoordIndex == 0) {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.X,
                                   (byte)SelectedCmd.SelectedCoord.Y];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.X;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos + 1] = (byte)newPT.Y;
                }
                else if (SelectedCmd.SelectedCoordIndex.IsOdd()) {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.Y,
                                   (byte)SelectedCmd.SelectedCoord.X];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos - 1] = (byte)newPT.Y;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.X;
                    NextUndo.PicPos -= 1;
                }
                else {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.X,
                                   (byte)SelectedCmd.SelectedCoord.Y];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos - 1] = (byte)newPT.X;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.Y;
                    NextUndo.PicPos -= 1;
                }
                break;
            case YCorner:
                if (SelectedCmd.SelectedCoordIndex == 0) {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.X,
                                   (byte)SelectedCmd.SelectedCoord.Y];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.X;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos + 1] = (byte)newPT.Y;
                }
                else if (SelectedCmd.SelectedCoordIndex == 1) {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.X,
                                   EditPicture.Data[SelectedCmd.SelectedCoordPos - 1],
                                   (byte)SelectedCmd.SelectedCoord.Y];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos - 2] = (byte)newPT.X;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.Y;
                    NextUndo.PicPos -= 2;
                }
                else if (SelectedCmd.SelectedCoordIndex.IsEven()) {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.Y,
                                   (byte)SelectedCmd.SelectedCoord.X];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos - 1] = (byte)newPT.Y;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.X;
                    NextUndo.PicPos--;
                }
                else {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.X,
                                   (byte)SelectedCmd.SelectedCoord.Y];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos - 1] = (byte)newPT.X;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.Y;
                    NextUndo.PicPos--;
                }
                break;
            case AbsLine:
            case Fill:
                bytOldCoord = [(byte)SelectedCmd.SelectedCoord.X, (byte)SelectedCmd.SelectedCoord.Y];
                EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.X;
                EditPicture.Data[SelectedCmd.SelectedCoordPos + 1] = (byte)newPT.Y;
                break;
            case PlotPen:
                if (SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    bytOldCoord = [EditPicture.Data[SelectedCmd.SelectedCoordPos],
                        (byte)SelectedCmd.SelectedCoord.X, (byte)SelectedCmd.SelectedCoord.Y];
                    // change the plot pattern data
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = bytNewPattern;
                    // update the coords
                    EditPicture.Data[SelectedCmd.SelectedCoordPos + 1] = (byte)newPT.X;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos + 2] = (byte)newPT.Y;
                }
                else {
                    bytOldCoord = [(byte)SelectedCmd.SelectedCoord.X, (byte)SelectedCmd.SelectedCoord.Y];
                    EditPicture.Data[SelectedCmd.SelectedCoordPos] = (byte)newPT.X;
                    EditPicture.Data[SelectedCmd.SelectedCoordPos + 1] = (byte)newPT.Y;
                }
                break;
            }
            // add data, then push the undo
            NextUndo.Data = bytOldCoord;
            AddUndo(NextUndo);
            // reselect to refresh
            int oldcpi = SelectedCmd.SelectedCoordIndex;
            SelectCommand(SelectedCmd.Index, 1, true);
            SelectCoordinate(oldcpi);
        }

        private void lstCoords_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore all keypresses
            e.Handled = true;
        }

        private void lstCoords_KeyUp(object sender, KeyEventArgs e) {
            // when using keyboard to move the selection, the 
            // draw surfaces also need to be updated
            switch (e.KeyCode) {
            case Keys.Down:
            case Keys.Up:
            case Keys.PageDown:
            case Keys.PageUp:
            case Keys.Home:
            case Keys.End:
                if (!SelectedCmd.IsPen && lstCoords.SelectedItems.Count != 0) {
                    SelectCoordinate(lstCoords.SelectedItems[0].Index);
                }
                break;
            }
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void picPalette_Paint(object sender, PaintEventArgs e) {
            // update the palette to show available colors, as well as the current
            // visual and priority pen colors

            float dblWidth = picPalette.Width / 9;
            float dblHeight = picPalette.Height / 2;
            Graphics g = e.Graphics;

            // disabled brush area
            g.FillRectangle(Brushes.White, 0, 0, dblWidth, dblHeight * 2);
            Pen pen = new(Color.Black) {
                Width = 2
            };
            g.DrawLine(pen, 0, 0, dblWidth, dblHeight * 2);
            g.DrawLine(pen, dblWidth, 0, 0, dblHeight * 2);

            // color area
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 8; j++) {
                    Color color = EditPalette[i * 8 + j];
                    g.FillRectangle(new SolidBrush(color), (j + 1) * dblWidth, i * dblHeight, dblWidth, dblHeight);
                }
            }
            Color textcolor;
            Font font = new Font("Arial", 10, FontStyle.Bold);

            // add 'V' for current selected visual color
            if (SelectedCmd.Pen.VisColor < AGIColorIndex.None) {
                if ((int)SelectedCmd.Pen.VisColor > 9) {
                    textcolor = Color.Black;
                }
                else {
                    textcolor = Color.White;
                }
                // set x and Y to position 'v' over correct color
                g.DrawString("V", font, new SolidBrush(textcolor), dblWidth * (((int)SelectedCmd.Pen.VisColor % 8) + 1) + 3, 17 * ((int)SelectedCmd.Pen.VisColor / 8) - 1);
            }
            else {
                // put 'v' in disabled square
                g.DrawString("V", font, Brushes.Black, 3, 7);
            }
            // add 'P' for current selected priority color
            if (SelectedCmd.Pen.PriColor < AGIColorIndex.None) {
                if ((int)SelectedCmd.Pen.PriColor > 9) {
                    textcolor = Color.Black;
                }
                else {
                    textcolor = Color.White;
                }
                // set x and Y to position 'P' over correct color
                g.DrawString("P", font, new SolidBrush(textcolor), dblWidth * (((int)SelectedCmd.Pen.PriColor % 8) + 2) - 13, 17 * ((int)SelectedCmd.Pen.PriColor / 8) - 1);
            }
            else {
                // put 'P' in disabled square
                g.DrawString("P", font, Brushes.Black, dblWidth - 12, 7);
            }
        }

        private void picPalette_MouseDown(object sender, MouseEventArgs e) {
            // determine the selected color from X,Y position
            int dblWidth = picPalette.Width / 9;
            int bytNewCol = (9 * (e.Y / 16)) + (e.X / dblWidth);

            // adjust to account for the disabled block
            if (bytNewCol == 0 || bytNewCol == 9) {
                // color disable was chosen
                bytNewCol = 16;
            }
            else {
                if (bytNewCol < 9) {
                    bytNewCol--;
                }
                else {
                    bytNewCol -= 2;
                }
            }
            switch (ModifierKeys) {
            case Keys.None:
            case Keys.Shift:
                if (PicMode != PicEditorMode.Edit) {
                    return;
                }
                if (PicDrawMode != PicDrawOp.None) {
                    return;
                }
                switch (e.Button) {
                case MouseButtons.Left:
                    UpdateVisPen((AGIColorIndex)bytNewCol, ModifierKeys == Keys.Shift);
                    break;
                case MouseButtons.Right:
                    UpdatePriPen((AGIColorIndex)bytNewCol, ModifierKeys == Keys.Shift);
                    break;
                }
                picPalette.Invalidate();
                break;
            case Keys.Control:
                // change cursor color, (but can't select 'no color')
                if (bytNewCol < 16) {
                    if (e.Button == MouseButtons.Left) {
                        VCColor = EditPalette[bytNewCol];
                    }
                    else if (e.Button == MouseButtons.Right) {
                        PCColor = EditPalette[bytNewCol];
                    }
                    else {
                        // ignore any other button scenarios
                        return;
                    }
                    picVisual.Refresh();
                    picPriority.Refresh();
                }
                break;
            }
        }

        private void picPalette_MouseEnter(object sender, EventArgs e) {
            // set cursor depending on mode

            if (PicMode == PicEditorMode.Edit) {
                picPalette.Cursor = Cursors.Default;
            }
            else {
                picPalette.Cursor = Cursors.No;
            }
        }

        private void picVisual_MouseWheel(object sender, MouseEventArgs e) {
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }
            Point panelPt = splitImages.Panel1.PointToClient(Cursor.Position);
            if (!splitImages.Panel1.ClientRectangle.Contains(panelPt)) {
                return;
            }
            ChangeScale(e.Delta, true);
        }

        private void picPriority_MouseWheel(object sender, MouseEventArgs e) {
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }
            Point panelPt = splitImages.Panel2.PointToClient(Cursor.Position);
            if (!splitImages.Panel2.ClientRectangle.Contains(panelPt)) {
                return;
            }
            ChangeScale(e.Delta, true);
        }

        private void splitForm_SplitterMoving(object sender, SplitterCancelEventArgs e) {
            if (Cursor.Current != Cursors.VSplit) {
                Cursor.Current = Cursors.VSplit;
            }
        }

        private void splitForm_SplitterMoved(object sender, SplitterEventArgs e) {
            Cursor.Current = Cursors.Default;
            lstCommands.Select();
        }

        private void splitForm_MouseUp(object sender, MouseEventArgs e) {
            lstCommands.Select();
        }

        private void splitImages_SplitterMoving(object sender, SplitterCancelEventArgs e) {
            if (e.MouseCursorY < 45) {
                if (splitImages.Cursor != Cursors.UpArrow) {
                    splitImages.Cursor = Cursors.UpArrow;
                    e.SplitX = 0;
                }
            }
            else if (e.MouseCursorY > splitImages.Height - 45) {
                if (splitImages.Cursor.Tag is null) {
                    MemoryStream msCursor;
                    msCursor = new(EditorResources.downarrow);
                    splitImages.Cursor = new Cursor(msCursor);
                    splitImages.Cursor.Tag = "down";
                }
            }
            else {
                if (splitImages.Cursor != Cursors.HSplit) {
                    splitImages.Cursor = Cursors.HSplit;
                }
            }
        }

        private void splitImages_SplitterMoved(object sender, SplitterEventArgs e) {
            if (!Visible) {
                // avoid issues when splitter is set during form setup;
                // (the form will not be visible then)
                return;
            }
            splitImages.Cursor = Cursors.Default;
            if (!TooSmall) {
                if (splitImages.SplitterDistance < 45) {
                    // don't allow visual to disappear if in print test mode
                    if (PicMode == PicEditorMode.PrintTest) {
                        splitImages.SplitterDistance = 45;
                    }
                    else {
                        splitImages.SplitterDistance = 0;
                        OneWindow = WindowMode.Priority;
                    }
                }
                else if (splitImages.Panel2.Height < 45) {
                    splitImages.SplitterDistance = splitImages.ClientSize.Height - splitImages.SplitterWidth; ;
                    OneWindow = WindowMode.Visual;
                }
                else {
                    OneWindow = WindowMode.Both;
                    DrawPicture();
                }
            }
            lstCommands.Select();
            SetScrollbars();
        }

        private void splitImages_MouseUp(object sender, MouseEventArgs e) {
            // always make sure the command list has focus (is selected)
            lstCommands.Select();
        }

        private void splitLists_SplitterMoving(object sender, SplitterCancelEventArgs e) {
            if (Cursor.Current != Cursors.HSplit) {
                Cursor.Current = Cursors.HSplit;
            }
        }

        private void splitLists_SplitterMoved(object sender, SplitterEventArgs e) {
            Cursor.Current = Cursors.Default;
            lstCommands.Select();
        }

        private void splitLists_MouseUp(object sender, MouseEventArgs e) {
            // always make sure command list has focus (is selected)
            lstCommands.Select();
        }

        private void tmrTest_Tick(object sender, EventArgs e) {
            // controls test view movement and cycling
            AGIColorIndex ControlLine = 0;
            byte NewX = (byte)TestCelPos.X;
            byte NewY = (byte)TestCelPos.Y;
            int DX = 0;
            int DY = 0;
            bool OnWater = false;
            bool skipmove = false;

            if (TestSettings.TestCel == -1) {
                CurTestCel++;
                // if at loopcount, reset back to zero
                if (CurTestCel == CurTestLoopCount) {
                    CurTestCel = 0;
                }
                TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
            }
            // set cel height/width/transcolor
            CelWidth = TestView[CurTestLoop][CurTestCel].Width;
            CelHeight = TestView[CurTestLoop][CurTestCel].Height;
            CelTrans = TestView[CurTestLoop][CurTestCel].TransColor;

            // check for special case of no motion
            if (TestDir == ObjDirection.odStopped) {
                // cycle in place
                skipmove = true;
            }

            // not stopped, check for hitting an edge
            if (!skipmove) {
                // calculate dX and dY based on direction
                // (these are empirical formulas based on relationship between direction and change in X/Y)
                DX = Math.Sign(5 - (int)TestDir) * Math.Sign((int)TestDir - 1);
                DY = Math.Sign(3 - (int)TestDir) * Math.Sign((int)TestDir - 7);

                // test for edges
                switch (TestDir) {
                case ObjDirection.odUp:
                    if ((NewY == TestSettings.Horizon.Value) && !TestSettings.IgnoreHorizon.Value) {
                        StopReason = 7;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    if (NewY - (CelHeight - 1) <= 0) {
                        StopReason = 8;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    ControlLine = EditPicture.ControlPixel(NewX, (byte)(NewY - 1), CelWidth);
                    break;
                case ObjDirection.odUpRight:
                    if ((NewY == TestSettings.Horizon.Value) && !TestSettings.IgnoreHorizon.Value) {
                        // dont go
                        StopReason = 7;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    if (NewY - (CelHeight - 1) <= 0) {
                        StopReason = 8;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    if (NewX + CelWidth - 1 >= 159) {
                        StopReason = 9;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    // get controlline status
                    ControlLine = EditPicture.ControlPixel((byte)(NewX + 1), (byte)(NewY - 1), CelWidth);
                    break;
                case ObjDirection.odRight:
                    if (NewX + CelWidth - 1 >= 159) {
                        StopReason = 9;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    // get controlline status
                    ControlLine = EditPicture.ControlPixel((byte)(NewX + CelWidth), (byte)NewY);
                    break;
                case ObjDirection.odDownRight:
                    if (NewY == 167) {
                        StopReason = 10;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    if (NewX + CelWidth - 1 == 159) {
                        StopReason = 9;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    ControlLine = EditPicture.ControlPixel((byte)(NewX + 1), (byte)(NewY + 1), CelWidth);
                    break;
                case ObjDirection.odDown:
                    if (NewY == 167) {
                        StopReason = 10;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    ControlLine = EditPicture.ControlPixel(NewX, (byte)(NewY + 1), CelWidth);
                    break;
                case ObjDirection.odDownLeft:
                    if (NewY == 167) {
                        StopReason = 10;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    if (NewX == 0) {
                        StopReason = 11;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    ControlLine = EditPicture.ControlPixel((byte)(NewX - 1), (byte)(NewY + 1), CelWidth);
                    break;
                case ObjDirection.odLeft:
                    if (NewX == 0) {
                        StopReason = 11;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    ControlLine = EditPicture.ControlPixel((byte)(NewX - 1), NewY);
                    break;
                case ObjDirection.odUpLeft:
                    if (((NewY == TestSettings.Horizon.Value) && !TestSettings.IgnoreHorizon.Value)) {
                        StopReason = 7;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    if (NewY - (CelHeight - 1) <= 0) {
                        StopReason = 8;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    if (NewX == 0) {
                        StopReason = 11;
                        TestDir = ObjDirection.odStopped;
                        tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                        skipmove = true;
                        break;
                    }
                    ControlLine = EditPicture.ControlPixel((byte)(NewX - 1), (byte)(NewY - 1), CelWidth);
                    break;
                }
            }

            // if not on an edge, check for restrictions
            if (!skipmove) {
                OnWater = EditPicture.ObjOnWater(new(NewX + DX, NewY + DY), CelWidth);
                // if at an obstacle line OR (at a conditional obstacle line AND NOT blocking)
                if (((int)ControlLine <= 1) && !TestSettings.IgnoreBlocks.Value) {
                    StopReason = (int)ControlLine + 1;
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                    skipmove = true;
                }
                // if restricting access to land AND on water!
                if ((TestSettings.ObjRestriction.Value == 2) && OnWater) {
                    StopReason = 5;
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                    skipmove = true;
                }
                // if restricting access to water AND at land edge
                if ((TestSettings.ObjRestriction.Value == 1) && !OnWater) {
                    StopReason = 6;
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                    skipmove = true;
                }
            }

            // all checks complete- OK to move?
            if (!skipmove) {
                TestCelPos.X = (byte)(NewX + DX);
                TestCelPos.Y = (byte)(NewY + DY);

                // if at an alarm line
                if (ControlLine == AGIColorIndex.Green) {
                    StopReason = 3;
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    // if on water, set status
                    StopReason = OnWater ? 4 : 0;
                }
            }

            // force update
            picVisual.Refresh();
            picPriority.Refresh();

            if (StopReason != 0) {
                spStatus.Text = Editor.Base.LoadResString(STOPREASONTEXT + StopReason);
                StopReason = 0;
            }
            else {
                if (TestDir != ObjDirection.odStopped) {
                    // clear tool panel
                    spStatus.Text = "";
                }
            }
        }

        private void tmrSelect_Tick(object sender, EventArgs e) {
            // cursor and selection timer
            // if a selection is visible, the selection boundary is cycled to show a
            // moving dashed line
            // if no selection, the current pixel is cycled through all AGI colors
            // to make it more visible

            if (SelectedRegion.Width > 0 && SelectedRegion.Height > 0 || SelectedTool == PicToolType.SelectArea) {
                // decrement for clockwise movement, increment for 
                // counterclockwise movement
                dashdistance -= 1;
                if (dashdistance == 0) dashdistance = 6;
                dash1.DashOffset = dashdistance;
                dash2.DashOffset = dashdistance - 3;

                Rectangle rgn = new((int)(SelectedRegion.X * ScaleFactor * 2),
                    (int)(SelectedRegion.Y * ScaleFactor),
                    (int)((SelectedRegion.Width) * ScaleFactor * 2) - 1,
                    (int)((SelectedRegion.Height) * ScaleFactor) - 1);
                if ((OneWindow & WindowMode.Visual) == WindowMode.Visual) {
                    using Graphics gv = picVisual.CreateGraphics();
                    gv.DrawRectangle(dash1, rgn);
                    gv.DrawRectangle(dash2, rgn);
                }
                if ((OneWindow & WindowMode.Priority) == WindowMode.Priority) {
                    using Graphics gp = picPriority.CreateGraphics();
                    gp.DrawRectangle(dash1, rgn);
                    gp.DrawRectangle(dash2, rgn);
                }
            }
            else {
                // toggle cursor color
                CursorColorIndex += 1;
                if (CursorColorIndex >= AGIColorIndex.None) {
                    CursorColorIndex -= 16;
                }
            }
            // if using flashing box cursor
            if (CursorMode == CoordinateHighlightType.FlashBox && !OnPoint) {
                CursorSize += 0.5f;
                if (CursorSize > 1) {
                    CursorSize = 0;
                }
                if ((OneWindow & WindowMode.Visual) == WindowMode.Visual && SelectedCmd.Pen.VisColor != AGIColorIndex.None) {
                    using Graphics gv = picVisual.CreateGraphics();
                    SolidBrush vb = new(EditPalette[(int)CursorColorIndex]);
                    gv.FillRectangle(vb, (SelectedCmd.SelectedCoord.X - CursorSize + 0.5f) * ScaleFactor * 2, (SelectedCmd.SelectedCoord.Y - CursorSize + 0.5f) * ScaleFactor, 2 * CursorSize * ScaleFactor * 2, 2 * CursorSize * ScaleFactor);
                }
                if ((OneWindow & WindowMode.Priority) == WindowMode.Priority && SelectedCmd.Pen.PriColor != AGIColorIndex.None) {
                    using Graphics gp = picPriority.CreateGraphics();
                    SolidBrush pb = new(EditPalette[(int)CursorColorIndex]);
                    gp.FillRectangle(pb, (SelectedCmd.SelectedCoord.X - CursorSize + 0.5f) * ScaleFactor * 2, (SelectedCmd.SelectedCoord.Y - CursorSize + 0.5f) * ScaleFactor, 2 * CursorSize * ScaleFactor * 2, 2 * CursorSize * ScaleFactor);
                }
            }
            else {
                // if using 'x' marks, draw a box
                float cOfX = (float)(1.5 / Math.Sqrt(ScaleFactor));
                float cOfY = cOfX * 2; // 3 / ScaleFactor ^ 0.5
                float cSzX = cOfX * 2; // 3 / ScaleFactor ^ 0.5
                float cSzY = cOfY * 2; // 6 / ScaleFactor ^ 0.5
                if ((OneWindow & WindowMode.Visual) == WindowMode.Visual && SelectedCmd.Pen.VisColor != AGIColorIndex.None) {
                    using Graphics gv = picVisual.CreateGraphics();
                    Pen vp = new(EditPalette[(int)CursorColorIndex]);
                    gv.DrawRectangle(vp, (float)((SelectedCmd.SelectedCoord.X + 0.5 - (cOfX * 0.5)) * ScaleFactor * 2),
                                                (float)((SelectedCmd.SelectedCoord.Y + 0.5 - (cOfY * 0.5)) * ScaleFactor),
                                                (float)(0.5 * cSzX * ScaleFactor * 2),
                                                (float)(0.5 * cSzY * ScaleFactor));
                }
                if ((OneWindow & WindowMode.Priority) == WindowMode.Priority && SelectedCmd.Pen.PriColor != AGIColorIndex.None) {
                    using Graphics gp = picPriority.CreateGraphics();
                    Pen pp = new(EditPalette[(int)CursorColorIndex]);
                    gp.DrawRectangle(pp, (float)((SelectedCmd.SelectedCoord.X + 0.5 - (cOfX * 0.5)) * ScaleFactor * 2),
                                                (float)((SelectedCmd.SelectedCoord.Y + 0.5 - (cOfY * 0.5)) * ScaleFactor),
                                                (float)(0.5 * cSzX * ScaleFactor * 2),
                                                (float)(0.5 * cSzY * ScaleFactor));
                }
            }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the status strip with the appropriate labels.
        /// </summary>
        private void InitStatusStrip() {
            spScale = new ToolStripStatusLabel();
            spMode = new ToolStripStatusLabel();
            spTool = new ToolStripStatusLabel();
            spAnchor = new ToolStripStatusLabel();
            spBlock = new ToolStripStatusLabel();
            spCurX = new ToolStripStatusLabel();
            spCurY = new ToolStripStatusLabel();
            spPriBand = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
            //
            // spScale
            // 
            spScale.AutoSize = false;
            spScale.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spScale.BorderStyle = Border3DStyle.SunkenInner;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(80, 18);
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";
            spScale.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spMode
            // 
            spMode.AutoSize = false;
            spMode.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spMode.BorderStyle = Border3DStyle.SunkenInner;
            spMode.Name = "spMode";
            spMode.Size = new System.Drawing.Size(60, 18);
            spMode.Text = "Edit";
            spMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spTool
            // 
            spTool.AutoSize = false;
            spTool.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spTool.BorderStyle = Border3DStyle.SunkenInner;
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(120, 18);
            spTool.Text = "";
            spTool.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spAnchor
            // 
            spAnchor.AutoSize = false;
            spAnchor.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spAnchor.BorderStyle = Border3DStyle.SunkenInner;
            spAnchor.Name = "spAnchor";
            spAnchor.Size = new System.Drawing.Size(120, 18);
            spAnchor.Text = "";
            spAnchor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            spAnchor.Visible = false;
            // 
            // spBlock
            // 
            spBlock.AutoSize = false;
            spBlock.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spBlock.BorderStyle = Border3DStyle.SunkenInner;
            spBlock.Name = "spBlock";
            spBlock.Size = new System.Drawing.Size(160, 18);
            spBlock.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            spBlock.Text = "";
            spBlock.Visible = false;
            // 
            // spCurX
            // 
            spCurX.AutoSize = false;
            spCurX.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spCurX.BorderStyle = Border3DStyle.SunkenInner;
            spCurX.Name = "spCurX";
            spCurX.Size = new System.Drawing.Size(70, 18);
            spCurX.Text = "";
            // 
            // spCurY
            // 
            spCurY.AutoSize = false;
            spCurY.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spCurY.BorderStyle = Border3DStyle.SunkenInner;
            spCurY.Name = "spCurY";
            spCurY.Size = new System.Drawing.Size(70, 18);
            spCurY.Text = "";
            // 
            // spPriBand
            // 
            spPriBand.AutoSize = false;
            spPriBand.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spPriBand.BorderStyle = Border3DStyle.SunkenInner;
            spPriBand.Name = "spPriBand";
            spPriBand.Size = new System.Drawing.Size(67, 18);
            spPriBand.Text = "";
            spPriBand.Image = new Bitmap(12, 12);
            spPriBand.ImageAlign = ContentAlignment.MiddleCenter;
            spPriBand.ImageScaling = ToolStripItemImageScaling.None;

            //
            // spStatus
            //
            spStatus.Text = "";

        }

        /// <summary>
        /// Initializes the fonts for the command and coordinate lists.
        /// </summary>
        internal void InitFonts() {
            lstCommands.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
            lstCoords.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
        }

        /// <summary>
        /// Loads the specified picture resource into the editor.  
        /// </summary>
        /// <param name="loadpic"></param>
        /// <returns>True if picture loads successfully. False if unable to load 
        /// due to error.</returns>
        public bool LoadPicture(Picture loadpic, bool quiet = false) {
            InGame = loadpic.InGame;
            if (InGame) {
                PictureNumber = loadpic.Number;
            }
            else {
                // use a number that can never match
                // when searches for open pictures are made
                PictureNumber = 256;
            }
            try {
                loadpic.Load();
            }
            catch (Exception ex) {
                // unhandled error
                if (!quiet) {
                    string resid = InGame ? "Picture " + PictureNumber : loadpic.ID;
                    ErrMsgBox(ex,
                        "Something went wrong. Unable to load " + resid,
                        ex.StackTrace,
                        "Load Picture Failed");
                }
                return false;
            }
            if (loadpic.Error != ResourceErrorType.NoError) {
                if (!quiet) {
                    if (InGame) {
                        switch (loadpic.Error) {
                        case ResourceErrorType.FileNotFound:
                            // should not be possible unless volfile deleted after
                            // the game was loaded
                            MessageBox.Show(MDIMain,
                                $"The VOL file with Picture {loadpic.Number} is missing.",
                                "Missing VOL File",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning, 0, 0,
                                WinAGIHelp, "htm\\winagi\\errors\\re01.htm");
                            break;
                        case ResourceErrorType.FileIsReadonly:
                            // should not be possible unless volfile properties were
                            // changed after the game was loaded
                            MessageBox.Show(MDIMain,
                                $"Picture {loadpic.Number} is in a VOL file tagged as readonly. " +
                                "It cannot be edited unless full access is allowed.",
                                "Readonly VOL File",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, 0, 0,
                                WinAGIHelp, "htm\\winagi\\errors\\re02.htm");
                            break;
                        case ResourceErrorType.FileAccessError:
                            MessageBox.Show(MDIMain,
                                $"A file access error in the VOL file with Picture {loadpic.Number} " +
                                "is preventing the picture from being edited. ",
                                "VOL File Access Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, 0, 0,
                                WinAGIHelp, "htm\\winagi\\errors\\re03.htm");
                            break;
                        //case ResourceErrorType.InvalidLocation:
                        //case ResourceErrorType.InvalidHeader:
                        //case ResourceErrorType.DecompressionError:
                        default:
                            // should not be possible
                            Debug.Assert(false);
                            MessageBox.Show(MDIMain,
                            "Something went wrong. Unable to load Picture " + PictureNumber,
                            "Load Picture Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                            break;
                        }
                    }
                    else {
                        // show a generic message
                        MessageBox.Show(MDIMain,
                            "Unable to open Picture " + PictureNumber + ":\n\n LoadError " +
                            loadpic.Error.ToString(),
                            "Picture Resource Load Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                return false;
            }
            EditPicture = loadpic.Clone();
            EditPalette = loadpic.Palette.Clone();
            VCColor = EditPalette[4]; // red
            PCColor = EditPalette[3]; // cyan
            picVisual.BackColor = EditPalette[15];
            picPriority.BackColor = EditPalette[4];
            if (!InGame && EditPicture.ID == "NewPicture") {
                PicCount++;
                EditPicture.ID = "NewPicture" + PicCount;
                IsChanged = true;
            }
            else {
                // unlike views/sounds, if picture has errors in it don't
                // force a save because user has to manually edit the bad
                // data to repair it
                IsChanged = EditPicture.IsChanged;
            }
            Text = sPICED + ResourceName(EditPicture, InGame, true);
            if (IsChanged) {
                Text = CHG_MARKER + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.btnSaveResource.Enabled = !IsChanged;

            // enable stepdrawing
            EditPicture.StepDraw = true;
            // enable editing
            PicMode = PicEditorMode.Edit;
            // check for a saved background image
            if (EditPicture.BkgdFileName.Length != 0) {
                try {
                    BkgdImage = new(Path.GetFullPath(EditPicture.BkgdFileName, EditGame.ResDir));
                    if (EditPicture.BkgdVisible) {
                        tsbBackground.Checked = true;
                    }
                }
                catch (Exception ex) {
                    string strMsg;
                    // if error is file not found, let user know
                    if (ex.HResult == 999) {
                        strMsg = "Background file not found. ";
                    }
                    else {
                        strMsg = "Error loading background image.";
                    }
                    // inform user
                    MessageBox.Show(MDIMain,
                        strMsg + "\n\nThe background settings for this picture will be cleared.",
                        "Picture Background Image Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    // clear picedit background properties
                    EditPicture.BackgroundSettings = new();
                    // clear ingame resource background properties
                    EditGame.Pictures[PictureNumber].BackgroundSettings = new();
                    EditGame.Pictures[PictureNumber].SaveProps();
                    BkgdImage = null;
                }
            }
            // populate cmd list with commands (which updates the draw images)
            if (!LoadCmdList()) {
                // error- stop the form loading process
                MessageBox.Show(MDIMain,
                    "This picture has corrupt or invalid data. Unable to open it for editing.",
                    "Picture Data Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                EditPicture.Unload();
                EditPicture = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Imports the picture from the specified file and replaces the current
        /// picture resource in the editor.
        /// </summary>
        /// <param name="importfile"></param>
        public void ImportPicture(string importfile) {
            MDIMain.UseWaitCursor = true;
            Picture tmpPicture = new();
            if (!Base.ImportPicture(importfile, tmpPicture)) {
                return;
            }
            // copy only the resource data
            EditPicture.ReplaceData(tmpPicture.Data.ToArray());
            EditPicture.ResetPicture();
            MarkAsChanged();
            // redraw by selecting the end coordinate
            SelectCommand(lstCommands.Items.Count - 1, 1, true);
            MDIMain.UseWaitCursor = false;
        }

        /// <summary>
        /// Save the current picture resource.
        /// </summary>
        public void SavePicture() {
            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool blnLoaded = EditGame.Pictures[PictureNumber].Loaded;
                if (!blnLoaded) {
                    EditGame.Pictures[PictureNumber].Load();
                }
                EditGame.Pictures[PictureNumber].CloneFrom(EditPicture);
                // always default to full draw
                EditGame.Pictures[PictureNumber].StepDraw = false;
                EditGame.Pictures[PictureNumber].Save();
                if (!blnLoaded) {
                    EditGame.Pictures[PictureNumber].Unload();
                }
                RefreshTree(AGIResType.Picture, PictureNumber);
                if (WinAGISettings.AutoExport.Value) {
                    EditPicture.Export(EditGame.ResDir + EditPicture.ID + ".agp");
                    // reset ID (non-game id gets saved by export...)
                    EditPicture.ID = EditGame.Pictures[PictureNumber].ID;
                }
                if (LEInUse && EditGame.Logics.Contains(PictureNumber) &&
                    EditGame.Logics[PictureNumber].IsRoom) {
                    LayoutEditor.DrawLayout(LayoutSelection.Room, PictureNumber);
                }
                MarkAsSaved();
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditPicture.ResFile.Length == 0) {
                    ExportPicture();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    EditPicture.Save();
                    MarkAsSaved();
                    MDIMain.UseWaitCursor = false;
                }
            }
        }

        /// <summary>
        /// Exports the current picture resource to a file.
        /// </summary>
        private void ExportPicture() {
            int retval = Base.ExportPicture(EditPicture, InGame);
            if (InGame) {
                // because EditPicture is not the actual ingame picture its
                // ID needs to be reset back to the ingame value
                EditPicture.ID = EditGame.Pictures[PictureNumber].ID;
            }
            else {
                if (retval == 1) {
                    MarkAsSaved();
                }
            }
        }

        /// <summary>
        /// Toggles the InGame state of the picture resource currently being edited.
        /// </summary>
        public void ToggleInGame() {
            DialogResult rtn;
            string strExportName;
            bool blnDontAsk = false;

            if (InGame) {
                if (WinAGISettings.AskExport.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to export '" + EditPicture.ID + "' before removing it from your game?",
                        "Don't ask this question again",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        "Export Picture Before Removal", ref blnDontAsk);
                    WinAGISettings.AskExport.Value = !blnDontAsk;
                    if (!WinAGISettings.AskExport.Value) {
                        WinAGISettings.AskExport.WriteSetting(WinAGISettingsFile);
                    }
                }
                else {
                    // dont ask; assume no
                    rtn = DialogResult.No;
                }
                switch (rtn) {
                case DialogResult.Cancel:
                    return;
                case DialogResult.Yes:
                    // get a filename for the export
                    strExportName = NewResourceName(EditPicture, InGame);
                    if (strExportName.Length > 0) {
                        EditPicture.Export(strExportName);
                    }
                    break;
                case DialogResult.No:
                    // nothing to do
                    break;
                }
                // confirm removal
                if (WinAGISettings.AskRemove.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Removing '" + EditPicture.ID + "' from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                        "Remove Picture From Game",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        "Don't ask this question again", ref blnDontAsk);
                    WinAGISettings.AskRemove.Value = !blnDontAsk;
                    if (!WinAGISettings.AskRemove.Value) {
                        WinAGISettings.AskRemove.WriteSetting(WinAGISettingsFile);
                    }
                }
                else {
                    rtn = DialogResult.OK;
                }
                if (rtn == DialogResult.Cancel) {
                    return;
                }
                // remove the picture (force-closes this editor)
                RemovePicture((byte)PictureNumber);
            }
            else {
                // add to game 
                if (EditGame is null) {
                    return;
                }
                using frmGetResourceNum frmGetNum = new(GetRes.AddInGame, AGIResType.Picture, 0);
                if (frmGetNum.ShowDialog(MDIMain) != DialogResult.Cancel) {
                    PictureNumber = frmGetNum.NewResNum;
                    // change id before adding to game
                    EditPicture.ID = frmGetNum.txtID.Text;
                    AddNewPicture((byte)PictureNumber, EditPicture);
                    EditGame.Pictures[PictureNumber].Load();
                    // copy the picture back (to ensure internal variables are copied)
                    EditPicture.CloneFrom(EditGame.Pictures[PictureNumber]);
                    EditPalette = EditPicture.Palette.Clone();
                    // now we can unload the newly added picture;
                    EditGame.Pictures[PictureNumber].Unload();
                    MarkAsSaved();
                    InGame = true;
                    MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[20];
                    MDIMain.btnAddRemove.Text = "Remove Picture";
                }
            }
        }

        /// <summary>
        /// Renumbers the picture resource currently being edited, if it is an
        /// ingame resource.
        /// </summary>
        public void RenumberPicture() {
            if (!InGame) {
                return;
            }
            string oldid = EditPicture.ID;
            int oldnum = PictureNumber;
            byte NewResNum = GetNewNumber(AGIResType.Picture, (byte)PictureNumber);
            if (NewResNum != PictureNumber) {
                // update ID (it may have changed if using default ID)
                EditPicture.ID = EditGame.Pictures[NewResNum].ID;
                PictureNumber = NewResNum;
                Text = sPICED + ResourceName(EditPicture, InGame, true);
                if (IsChanged) {
                    Text = CHG_MARKER + Text;
                }
                if (EditPicture.ID != oldid) {
                    if (File.Exists(EditGame.ResDir + oldid + ".agp")) {
                        SafeFileMove(EditGame.ResDir + oldid + ".agp", EditGame.ResDir + EditGame.Pictures[NewResNum].ID + ".agp", true);
                    }
                }
            }
        }

        /// <summary>
        /// Displays the picture resource property dialog for the picture resource
        /// currently being edited.
        /// </summary>
        /// <param name="FirstProp"></param>
        public void EditPictureProperties(int FirstProp) {
            string id = EditPicture.ID;
            string description = EditPicture.Description;
            if (GetNewResID(AGIResType.Picture, PictureNumber, ref id, ref description, InGame, FirstProp)) {
                if (EditPicture.Description != description) {
                    EditPicture.Description = description;
                }
                if (EditPicture.ID != id) {
                    EditPicture.ID = id;
                    Text = sPICED + ResourceName(EditPicture, InGame, true);
                    if (IsChanged) {
                        Text = CHG_MARKER + Text;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the picture data into the command list.
        /// </summary>
        /// <returns></returns>
        private bool LoadCmdList() {
            byte bytCmd = 0;
            int lngPos = 0;
            bool blnErrors = false;
            bool exitdo = false;

            // clear the listbox
            lstCommands.Items.Clear();
            while (lngPos < EditPicture.Data.Length && !exitdo) {
                bytCmd = EditPicture.Data[lngPos];
                lstCommands.Items.Add(((DrawFunction)bytCmd).CommandName());
                switch (bytCmd) {
                case 0xFF:
                    //  end of file
                    lstCommands.Items[^1].Tag = lngPos;
                    exitdo = true;
                    break;
                case >= 0xF0 and <= 0xFA:
                    // pen color commands
                    lstCommands.Items[^1].Tag = lngPos;
                    switch (bytCmd) {
                    case 0xF0:
                        // enable visual
                        lngPos += 2;
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    case 0xF2:
                        // enable priority
                        lngPos += 2;
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    case 0xF1 or 0xF3:
                        // Disable draw.
                        lngPos++;
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    case 0xF4 or 0xF5 or 0xF6 or 0xF7 or 0xF8 or 0xFA:
                        do {
                            // read in data until another command is found or until
                            // end is reached
                            lngPos++;
                            if (lngPos >= EditPicture.Data.Length) {
                                break;
                            }
                            bytCmd = EditPicture.Data[lngPos];
                        } while (bytCmd < 0xF0);
                        break;
                    case 0xF9:
                        lstCommands.Items[^1].Text = "Set Plot Pen";
                        lngPos += 2;
                        if (lngPos >= EditPicture.Data.Length) {
                            break;
                        }
                        // get next command
                        bytCmd = EditPicture.Data[lngPos];
                        break;
                    }
                    break;
                default:
                    //  < 0xF0 or > 0xFA
                    // invalid command  - note it
                    lstCommands.Items.Add("ERR: (0x" + bytCmd.ToString("x2") + ")");
                    lstCommands.Items[^1].Tag = lngPos;
                    blnErrors = true;
                    break;
                }
            }
            // if end cmd not found, need to add it (and let user know)
            if (bytCmd != 0xFF) {
                // add missing end
                EditPicture.WriteByte(0xFF);
                // add 'end' node to list
                lstCommands.Items.Add(Editor.Base.LoadResString(DRAWFUNCTIONTEXT + 11));
                lstCommands.Items[^1].Tag = lngPos;
                MarkAsChanged();
                // restore cursor
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    "Picture is missing end-of-resource marker; marker has been added and picture  loaded, but picture data may be corrupt.",
                    "Missing End Command in Picture",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\pictures.htm#ff");
            }
            else if (lngPos < EditPicture.Data.Length - 1) {
                // remove the extra data
                EditPicture.RemoveData(lngPos + 1, EditPicture.Data.Length - 1 - lngPos);
                MarkAsChanged();
                // restore cursor
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    "Picture has unused data after the end-of-resource marker. The unused data have been removed but the picture data may be corrupt.",
                    "Missing End Command in Picture",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\pictures.htm#ff");
            }
            // if any bad commands or colors encountered
            if (blnErrors) {
                // restore cursor
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    "One or more invalid commands and/or colors encountered; they are marked with 'ERR'. This picture data may be corrupt.",
                    "Anomaly Found in Picture Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\winagi\\editor_pictures.htm#picerrors");
            }
            CmdColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            // select end cmd
            SelectCommand(lstCommands.Items.Count - 1, 1, true);
            return true;
        }

        /// <summary>
        /// Updates the command listbox when new commands are added to
        /// the picture resource.
        /// </summary>
        /// <param name="insertindex"></param>
        /// <param name="bytecount"></param>
        private void AdjustCommandList(int insertindex, int bytecount) {
            int startPos = (int)lstCommands.Items[insertindex].Tag;
            int endPos = startPos + bytecount;

            // first adjust commands from insertpos to end to new pos values
            UpdatePosValues(insertindex, bytecount);
            // then add new commands based on inserted data
            for (int i = startPos; i < endPos; i++) {
                if (EditPicture.Data[i] >= 0xF0) {
                    // add new command to list
                    // set tag to position in data
                    lstCommands.Items.Insert(insertindex++, ((DrawFunction)EditPicture.Data[i]).CommandName()).Tag = i;
                }
            }
            CmdColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        /// <summary>
        /// When opening editor, the selected command doesn't move into view unless
        /// the EnsureVisible method is called after the form is made visible and
        /// refreshed.
        /// </summary>
        public void ForceRefresh() {
           lstCommands.SelectedItems[0].EnsureVisible();
        }

        /// <summary>
        /// This method changes the editor mode between edit, view test 
        /// and print/displat test mode.
        /// </summary>
        /// <param name="newMode"></param>
        public void SetMode(PicEditorMode newMode) {
            // always cancel any drawing operation
            PicDrawMode = PicDrawOp.None;
            switch (newMode) {
            case PicEditorMode.Edit:
                PicMode = newMode;
                tsbMode.Image = tsbEditMode.Image;

                // disable any view movement
                TestDir = 0;
                tmrTest.Enabled = false;
                // reset cursor to match selected tool
                switch (SelectedTool) {
                case PicToolType.Edit:
                    // arrow cursor
                    SetCursors(PicCursor.Default);
                    break;
                case PicToolType.Fill:
                    SetCursors(PicCursor.Paint);
                    break;
                case PicToolType.Plot:
                    SetCursors(PicCursor.Brush);
                    break;
                default:
                    SetCursors(PicCursor.Select);
                    break;
                }
                // set status bar depending on grid mode
                if (ShowTextMarks) {
                    // force text row/col
                    StatusMode = PicStatusMode.Text;
                }
                else {
                    // force normal pixel coordinates
                    StatusMode = PicStatusMode.Pixel;
                }
                // reset status bar panels
                spMode.Text = "Edit";
                spStatus.Text = "";
                spTool.Text = "Tool: " + SelectedTool.ToString();
                break;
            case PicEditorMode.ViewTest:
                // get a test view if not yet assigned
                if (TestView is null) {
                    GetTestView();
                }
                // if still no view
                if (TestView is null) {
                    // cancel the mode change
                    return;
                }
                PicMode = newMode;
                tsbMode.Image = tsbViewTest.Image;
                // no multiple selections
                if (SelectedCmdCount > 1) {
                    SelectCommand(SelectedCmd.Index, 1, true);
                }
                // if doing anything, cancel it
                if (lstCoords.SelectedItems.Count != 0) {
                    lstCoords.SelectedItems.Clear();
                }
                SelectedCmd.SelectedCoordIndex = -1;
                if (StatusMode == PicStatusMode.Coord) {
                    StatusMode = PicStatusMode.Pixel;
                }
                spMode.Text = "View Test";
                break;
            case PicEditorMode.PrintTest:
                PicMode = newMode;
                tsbMode.Image = tsbPrintTest.Image;
                // clear coordinates list 
                if (lstCoords.SelectedItems.Count != 0) {
                    lstCoords.SelectedItems.Clear();
                }
                // disable view movement
                TestDir = 0;
                // no multiple selections
                if (SelectedCmdCount > 1) {
                    SelectCommand(SelectedCmd.Index, 1, true);
                }
                // if showing normal pixel, go to row/col mode
                if (StatusMode == PicStatusMode.Pixel) {
                    StatusMode = PicStatusMode.Text;
                }
                spMode.Text = "Print Test";
                // if single-window, only show the visual window
                if (TooSmall) {
                    if (splitImages.Panel1Collapsed) {
                        splitImages.Panel2Collapsed = true;
                        splitImages.Panel1Collapsed = false;
                    }
                }
                else {
                    if (OneWindow == WindowMode.Priority) {
                        OneWindow = WindowMode.Visual;
                        splitImages.SplitterDistance = splitImages.Height - splitImages.SplitterWidth;
                    }
                }
                GetTextOptions();
                break;
            }
            UpdateToolbar();
            EnableCoordList();
            picVisual.Refresh();
            picPriority.Refresh();
        }

        /// <summary>
        /// Enables or disables all toolbar items based on current
        /// state of the editor.
        /// </summary>
        private void UpdateToolbar() {
            // drawing tools
            if (PicMode == PicEditorMode.Edit) {
                tsbTool.Enabled = true;
                tsbPlotStyle.Enabled = SelectedTool != PicToolType.SelectArea;
                tsbPlotSize.Enabled = SelectedTool != PicToolType.SelectArea;
                tsbUndo.Enabled = UndoCol.Count > 0;
                tsbCut.Enabled = true;
                if (SelectedTool == PicToolType.SelectArea) {
                    // area selection - no editing commands are enabled,
                    // only copy is available
                    tsbCut.Enabled = false;
                    tsbPaste.Enabled = false;
                    tsbDelete.Enabled = false;
                    tsbFlipH.Enabled = false;
                    tsbFlipV.Enabled = false;
                    // copy is enabled if something selected
                    tsbCopy.Enabled = (SelectedRegion.Width > 0) && (SelectedRegion.Height > 0);
                }
                else if (lstCommands.SelectedItems.Count == 0 ||
                    lstCoords.SelectedItems.Count == 0 ||
                    lstCommands.SelectedItems[0].Text[..3] == "Set") {
                    // no coordinate is selected - set editing commands to 
                    // handle the selected drawing commands
                    tsbCut.Enabled = SelectedCmd.Index != lstCommands.Items[^1].Index;
                    tsbCopy.Enabled = tsbCut.Enabled;
                    tsbDelete.Enabled = tsbCut.Enabled;
                    tsbPaste.Enabled = Clipboard.ContainsData(PICTURE_CB_FMT);
                    tsbFlipH.Enabled = (SelectedRegion.Width > 1);
                    tsbFlipV.Enabled = (SelectedRegion.Height > 1);
                }
                else {
                    // a coordinate is selected, set editing commands to
                    // handle coordinate editing
                    tsbPaste.Enabled = false;
                    tsbCut.Enabled = false;
                    tsbCopy.Enabled = false;
                    switch (lstCommands.SelectedItems[0].Text[..4]) {
                    case "Line" or "Fill" or "Plot":
                        tsbDelete.Enabled = true;
                        break;
                    case "Vis " or "Pri " or "Set ":
                        tsbDelete.Enabled = false;
                        break;
                    default:
                        tsbDelete.Enabled = lstCoords.Items[0].Selected;
                        break;
                    }
                    tsbFlipH.Enabled = false;
                    tsbFlipV.Enabled = false;
                }
            }
            else {
                tsbTool.Enabled = false;
                tsbPlotStyle.Enabled = false;
                tsbPlotSize.Enabled = false;
                tsbCut.Enabled = false;
                tsbCopy.Enabled = false;
                tsbPaste.Enabled = false;
                tsbDelete.Enabled = false;
                tsbFlipH.Enabled = false;
                tsbFlipV.Enabled = false;
            }
        }

        /// <summary>
        /// Converts a picture's Y coordinate into the appropriate priority band.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="priBase"></param>
        /// <returns></returns>
        private byte GetPriBand(byte y, byte priBase = 48) {
            if (y < priBase) {
                return 4;
            }
            else {
                return (byte)((y - priBase) / (168f - priBase) * 10 + 5);
            }
        }

        /// <summary>
        /// Sets the cursors for the draw surfaces based on current mode and tool.
        /// </summary>
        /// <param name="NewCursor"></param>
        private void SetCursors(PicCursor NewCursor) {
            MemoryStream msCursor;

            if (CurCursor == NewCursor) {
                return;
            }
            CurCursor = NewCursor;
            switch (NewCursor) {
            case PicCursor.Edit:
                msCursor = new(EditorResources.EPC_EDIT);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case PicCursor.Cross:
                picVisual.Cursor = Cursors.Cross;
                picPriority.Cursor = Cursors.Cross;
                break;
            case PicCursor.Move:
                picVisual.Cursor = Cursors.SizeAll;
                picPriority.Cursor = Cursors.SizeAll;
                break;
            case PicCursor.Default:
                msCursor = new(EditorResources.EPC_EDIT);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case PicCursor.NoOp:
                picVisual.Cursor = Cursors.No;
                picPriority.Cursor = Cursors.No;
                break;
            case PicCursor.Paint:
                msCursor = new(EditorResources.EPC_PAINT);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case PicCursor.Brush:
                msCursor = new(EditorResources.EPC_BRUSH);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case PicCursor.Select:
                msCursor = new(EditorResources.EPC_SELECT);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case PicCursor.DragSurface:
                msCursor = new(EditorResources.EPC_MOVE);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            case PicCursor.SelectImage:
                msCursor = new(EditorResources.EPC_EDITSEL);
                picVisual.Cursor = picPriority.Cursor = new Cursor(msCursor);
                break;
            }
        }

        /// <summary>
        /// This draws the static image and background representing current
        /// draw state. If in full draw mode, then no need to determine draw
        /// point, but in step draw mode, the draw point is determined by
        /// the currently selected command and coordinate.
        /// </summary>
        private void DrawPicture() {
            if (EditPicture.StepDraw) {
                // determine the draw position for the current
                // command, coordinate and selected tool
                if (SelectedCmd.SelectedCoordIndex == -1) {
                    // no coordinat is selected
                    switch (PicMode) {
                    case PicEditorMode.ViewTest or PicEditorMode.PrintTest:
                        EditPicture.DrawPos = SelectedCmd.EndPos;
                        break;
                    case PicEditorMode.Edit:
                        switch (SelectedTool) {
                        case PicToolType.Edit:
                        case PicToolType.SelectArea:
                            EditPicture.DrawPos = SelectedCmd.EndPos;
                            break;
                        case PicToolType.Fill:
                            if (SelectedCmd.Type == Fill) {
                                EditPicture.DrawPos = SelectedCmd.EndPos;
                            }
                            else {
                                EditPicture.DrawPos = SelectedCmd.Position;
                            }
                            break;
                        case PicToolType.Plot:
                            if (SelectedCmd.Type == PlotPen) {
                                EditPicture.DrawPos = SelectedCmd.EndPos;
                            }
                            else {
                                EditPicture.DrawPos = SelectedCmd.Position;
                            }
                            break;
                        case PicToolType.Line:
                        case PicToolType.ShortLine:
                        case PicToolType.StepLine:
                            if (PicDrawMode == PicDrawOp.None) {
                                EditPicture.DrawPos = SelectedCmd.Position;
                            }
                            else {
                                EditPicture.DrawPos = SelectedCmd.EndPos;
                            }
                            break;
                        default:
                            // rectangle, trapezoid, ellipse
                            EditPicture.DrawPos = SelectedCmd.Position;
                            break;
                        }
                        break;
                    }
                }
                else {
                    // when a coordinate is selected, draw position
                    // is calculated differently
                    switch (PicMode) {
                    case PicEditorMode.ViewTest:
                    case PicEditorMode.PrintTest:
                        EditPicture.DrawPos = SelectedCmd.SelectedCoordPos;
                        break;
                    case PicEditorMode.Edit:
                        switch (SelectedTool) {
                        case PicToolType.Edit:
                            switch (SelectedCmd.Type) {
                            case AbsLine:
                            case RelLine:
                                EditPicture.DrawPos = SelectedCmd.CoordPos(SelectedCmd.SelectedCoordIndex - 1);
                                break;
                            case XCorner:
                            case YCorner:
                                EditPicture.DrawPos = SelectedCmd.CoordPos(SelectedCmd.SelectedCoordIndex - 2);
                                break;
                            case Fill:
                            case PlotPen:
                                EditPicture.DrawPos = SelectedCmd.SelectedCoordPos;
                                break;
                            }
                            break;
                        case PicToolType.SelectArea:
                            EditPicture.DrawPos = SelectedCmd.SelectedCoordPos;
                            break;
                        case PicToolType.Fill:
                        case PicToolType.Plot:
                            EditPicture.DrawPos = SelectedCmd.CoordPos(SelectedCmd.SelectedCoordIndex - 1);
                            break;
                        }
                        break;
                    }
                }
            }
            // draw the visual image
            Graphics gv = null;
            if ((OneWindow & WindowMode.Visual) == WindowMode.Visual) {
                int bWidth = (int)(320 * ScaleFactor), bHeight = (int)(168 * ScaleFactor);
                picVisual.Image = new Bitmap(bWidth, bHeight);
                gv = Graphics.FromImage(picVisual.Image);
                // draw background first, if it is visible
                if (EditPicture.BkgdVisible && EditPicture.BkgdShowVis) {
                    // start with clear background
                    gv.Clear(picVisual.BackColor);
                    // draw the background on visual surface, positioning it
                    // based on background settings
                    Rectangle dest = new(0, 0, picVisual.Width, picVisual.Height);
                    RectangleF src = EditPicture.BackgroundSettings.SourceRegion;
                    double HScale = src.Width / 320.0;
                    double VScale = src.Height / 168.0;
                    if (src.X < 0) {
                        dest.X = (int)(-EditPicture.BackgroundSettings.TargetPos.X * ScaleFactor);
                        src.X = 0;
                    }
                    if (src.Y < 0) {
                        dest.Y = (int)(-EditPicture.BackgroundSettings.TargetPos.Y * ScaleFactor);
                        src.Y = 0;
                    }
                    if (src.Right > BkgdImage.Width) {
                        dest.Width = (int)((320 - (src.Right - BkgdImage.Width) / HScale) * ScaleFactor);
                        src.Width = BkgdImage.Width - src.X;
                    }
                    if (src.Bottom > BkgdImage.Height) {
                        dest.Height = (int)((168 - (src.Bottom - BkgdImage.Height) / VScale) * ScaleFactor);
                        src.Height = BkgdImage.Height - src.Y;
                    }
                    gv.DrawImage(BkgdImage, dest, src, GraphicsUnit.Pixel);
                }
                // now draw the visual image, with scaling mode set to
                // give crisp pixel edges
                gv.InterpolationMode = InterpolationMode.NearestNeighbor;
                gv.PixelOffsetMode = PixelOffsetMode.Half;
                gv.DrawImage(EditPicture.VisualBMP, 0, 0, bWidth, bHeight);
            }
            // next draw the priority image
            Graphics gp = null;
            if ((OneWindow & WindowMode.Priority) == WindowMode.Priority) {
                int bWidth = (int)(320 * ScaleFactor), bHeight = (int)(168 * ScaleFactor);
                picPriority.Image = new Bitmap(bWidth, bHeight);
                gp = Graphics.FromImage(picPriority.Image);
                // draw background first, if it is visible
                if (EditPicture.BkgdVisible && EditPicture.BkgdShowPri) {
                    // start with clear background
                    gp.Clear(picPriority.BackColor);
                    // draw the background on priority surface, positioning it
                    // based on background settings
                    Rectangle dest = new(0, 0, picPriority.Width, picPriority.Height);
                    RectangleF src = EditPicture.BackgroundSettings.SourceRegion;
                    double HScale = src.Width / 320.0;
                    double VScale = src.Height / 168.0;
                    if (src.X < 0) {
                        dest.X = (int)(-EditPicture.BackgroundSettings.TargetPos.X * ScaleFactor);
                        src.X = 0;
                    }
                    if (src.Y < 0) {
                        dest.Y = (int)(-EditPicture.BackgroundSettings.TargetPos.Y * ScaleFactor);
                        src.Y = 0;
                    }
                    if (src.Right > BkgdImage.Width) {
                        dest.Width = (int)((320 - src.X * HScale) * ScaleFactor);
                        src.Width = BkgdImage.Width - src.X;
                    }
                    if (src.Top > BkgdImage.Height) {
                        dest.Height = (int)((168 - src.Y * VScale) * ScaleFactor);
                        src.Height = BkgdImage.Height - src.Y;
                    }
                    gp.DrawImage(BkgdImage, dest, src, GraphicsUnit.Pixel);
                }
                // now draw the priority image, with scaling mode set to
                // give crisp pixel edges
                gp.InterpolationMode = InterpolationMode.NearestNeighbor;
                gp.PixelOffsetMode = PixelOffsetMode.Half;
                gp.DrawImage(EditPicture.PriorityBMP, 0, 0, bWidth, bHeight);
            }
            // in edit mode and a coordinate is selected, line segments around
            // the selected coordinate that won't change when the coordinate is
            // edited are also added to the draw surface
            if (SelectedCmd.IsLine && EditPicture.StepDraw && SelectedCmd.SelectedCoordIndex >= 0) {
                int next = 1;
                if (SelectedCmd.Type == XCorner || SelectedCmd.Type == YCorner) {
                    next++;
                }
                for (int i = SelectedCmd.SelectedCoordIndex + next; i < SelectedCmd.Coords.Count - 1; i++) {
                    if (gv is not null && SelectedCmd.Pen.VisColor < AGIColorIndex.None) {
                        DrawLineOnImage(gv, EditPalette[(int)SelectedCmd.Pen.VisColor], SelectedCmd.Coords[i], SelectedCmd.Coords[i + 1]);
                    }
                    if (gp is not null && SelectedCmd.Pen.PriColor < AGIColorIndex.None) {
                        DrawLineOnImage(gp, EditPalette[(int)SelectedCmd.Pen.PriColor], SelectedCmd.Coords[i], SelectedCmd.Coords[i + 1]);
                    }
                }
            }
            gv?.Dispose();
            gp?.Dispose();
            // refresh so temporary elements get added to the images
            picVisual.Refresh();
            picPriority.Refresh();
        }

        /// <summary>
        /// Refreshes the palette for this editor and redraws the images.
        /// </summary>
        public void RefreshPic() {
            // update palette and redraw
            if (InGame) {
                EditPicture.Palette = EditGame.Palette.Clone();
            }
            else {
                EditPicture.Palette = DefaultPalette.Clone();
            }
            EditPalette = EditPicture.Palette.Clone();
            EditPicture.ResetPicture();
            DrawPicture();
        }

        /// <summary>
        /// Populates a CommandInfo object with the command data corresponding
        /// to the passed command index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private CommandInfo GetCommand(int index) {
            CommandInfo retval = new();
            if (index < 0 || index >= lstCommands.Items.Count) {
                return retval;
            }
            retval.Index = index;
            retval.Position = (int)lstCommands.Items[index].Tag;
            retval.Type = (DrawFunction)EditPicture.Data[retval.Position];
            if (SelectedTool != PicToolType.Edit && SelectedTool != PicToolType.SelectArea) {
                if (retval.Position - 1 < 0) {
                    retval.Pen = EditPicture.GetPenStatus(0);
                }
                else {
                    retval.Pen = EditPicture.GetPenStatus(retval.Position - 1);
                }
            }
            else {
                retval.Pen = EditPicture.GetPenStatus(retval.Position + 1);
            }
            int pos = retval.Position + 1;
            int x, y;
            switch (retval.Type) {
            case YCorner:
            case XCorner:
                bool relX = (retval.Type == XCorner);
                x = EditPicture.Data[pos++];
                if (x >= 0xF0) break;
                y = EditPicture.Data[pos++];
                if (y >= 0xF0) break;
                retval.Coords.Add(new(x, y));
                int p = EditPicture.Data[pos++];
                while (p < 0xF0) {
                    if (relX) {
                        x = p;
                    }
                    else {
                        y = p;
                    }
                    relX = !relX;
                    retval.Coords.Add(new(x, y));
                    p = EditPicture.Data[pos++];
                }
                break;
            case RelLine:
                int dx, dy;
                x = EditPicture.Data[pos++];
                if (x >= 0xF0) break;
                y = EditPicture.Data[pos++];
                if (y >= 0xF0) break;
                retval.Coords.Add(new(x, y));
                int d = EditPicture.Data[pos++];
                while (d < 0xF0) {
                    // if horizontal negative bit set
                    if ((d & 0x80) == 0x80) {
                        dx = -((d & 0x70) / 0x10);
                    }
                    else {
                        dx = ((d & 0x70) / 0x10);
                    }
                    // if vertical negative bit is set
                    if ((d & 0x8) == 0x8) {
                        dy = -(d & 0x7);
                    }
                    else {
                        dy = (d & 0x7);
                    }
                    x += dx;
                    y += dy;
                    retval.Coords.Add(new(x, y));
                    d = EditPicture.Data[pos++];
                }
                break;
            case AbsLine:
            case Fill:
                x = EditPicture.Data[pos++];
                if (x >= 0xF0) break;
                y = EditPicture.Data[pos++];
                if (y >= 0xF0) break;
                retval.Coords.Add(new(x, y));
                x = EditPicture.Data[pos++];
                while (x < 0xF0) {
                    y = EditPicture.Data[pos++];
                    if (y >= 0xF0) break;
                    retval.Coords.Add(new(x, y));
                    x = EditPicture.Data[pos++];
                }
                break;
            case PlotPen:
                x = EditPicture.Data[pos++];
                if (x >= 0xF0) break;
                if (retval.Pen.PlotStyle == PlotStyle.Splatter) {
                    // skip pattern
                    x = EditPicture.Data[pos++];
                    if (x >= 0xF0) break;
                }
                y = EditPicture.Data[pos++];
                if (y >= 0xF0) break;
                retval.Coords.Add(new(x, y));
                x = EditPicture.Data[pos++];
                while (x < 0xF0) {
                    if (retval.Pen.PlotStyle == PlotStyle.Splatter) {
                        x = EditPicture.Data[pos++];
                        if (x > 0xF0) break;
                    }
                    y = EditPicture.Data[pos++];
                    if (y >= 0xF0) break;
                    retval.Coords.Add(new(x, y));
                    x = EditPicture.Data[pos++];
                }
                break;
            case EnableVis:
                break;
            case EnablePri:
                break;
            default:
                retval.Coords = [];
                break;
            }
            return retval;
        }

        /// <summary>
        /// Draws highlight indicators on the given graphics obect over
        /// current commands unselected coordinates in the format specified
        /// by the form's CursorMode property.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="coordcolor"></param>
        private void HighlightCoords(Graphics g, Color coordcolor) {
            switch (CursorMode) {
            case CoordinateHighlightType.FlashBox:
                // currently, only the selected coordinate is highlighted in
                // FlashBox mode
                return;
            case CoordinateHighlightType.XMode:
                float cOfX = (float)(1.5 / Math.Sqrt(ScaleFactor));
                float cOfY = cOfX * 2; // 3 / ScaleFactor ^ 0.5
                float cSzX = cOfX * 2; // 3 / ScaleFactor ^ 0.5
                float cSzY = cOfY * 2; // 6 / ScaleFactor ^ 0.5

                // if any coords are in the list highlight them
                //   (lines need all coords highlighted all the time;
                //   plots and fills only highlight up to selected
                //   coord when step-draw is true)
                int lngCount;
                if (SelectedCmd.SelectedCoordIndex >= 0 && (SelectedCmd.Type == Fill || SelectedCmd.Type == PlotPen) && EditPicture.StepDraw) {
                    lngCount = SelectedCmd.SelectedCoordIndex + 1;
                }
                else {
                    lngCount = SelectedCmd.Coords.Count;
                }
                Point tmpPT = new();
                if (SelectedCmd.SelectedCoordIndex >= 0) {
                    tmpPT = SelectedCmd.SelectedCoord;
                }
                else {
                    // set to invalid value so it'll never match
                    tmpPT.X = 255;
                }
                for (int i = 0; i < lngCount; i++) {
                    Pen coordpen = new(coordcolor);
                    if (SelectedCmd.Coords[i] == tmpPT) {
                        // draw a box
                        g.DrawRectangle(coordpen, (float)((SelectedCmd.Coords[i].X + 0.5 - (cOfX * 0.5)) * ScaleFactor * 2),
                                                    (float)((SelectedCmd.Coords[i].Y + 0.5 - (cOfY * 0.5)) * ScaleFactor),
                                                    (float)(0.5 * cSzX * ScaleFactor * 2),
                                                    (float)(0.5 * cSzY * ScaleFactor));
                    }
                    else {
                        // highlight this coord with an X
                        g.DrawLine(coordpen, (float)((SelectedCmd.Coords[i].X + 0.5 - cOfX) * ScaleFactor * 2),
                                            (float)((SelectedCmd.Coords[i].Y + 0.5 - cOfY) * ScaleFactor),
                                            (float)((SelectedCmd.Coords[i].X + 0.5 + cOfX) * ScaleFactor * 2),
                                            (float)((SelectedCmd.Coords[i].Y + 0.5 + cOfY) * ScaleFactor));
                        g.DrawLine(coordpen, (float)((SelectedCmd.Coords[i].X + 0.5 - cOfX) * ScaleFactor * 2),
                                            (float)((SelectedCmd.Coords[i].Y + 0.5 + cOfY) * ScaleFactor),
                                            (float)((SelectedCmd.Coords[i].X + 0.5 + cOfX) * ScaleFactor * 2),
                                            (float)((SelectedCmd.Coords[i].Y + 0.5 - cOfY) * ScaleFactor));
                    }
                }
                break;
            }
        }

        /// <summary>
        /// Updates the picture resource with the passed plot pen setting
        /// by either modifying an existing set-plot command at the current
        /// position, or adding a new set-plot command.
        /// </summary>
        /// <param name="newpendata"></param>
        /// <param name="Force"></param>
        private void UpdatePlotPen(byte newpendata, bool Force = false) {
            //  if tool is edit or select area:
            //      if no intervening draw commands, edit the existing cmd or prior
            //      or following, otherwise insert a new cmd
            //  if tool is anything else:
            //      if no intervening draw commands, edit only prior (not current or
            //      following), otherwise insert a new cmd
            //  if a new cmd is added, select next cmd
            //  if cmd is edited and tool is edit or select area, select the edited
            //  cmd; if tool is anything else, select original cmd
            byte currentpendata = SelectedCmd.Pen.PlotData;
            PlotStyle newstyle = (PlotStyle)(newpendata & 0x20);
            int selectedindex = SelectedCmd.Index;

            if (currentpendata != newpendata || Force) {
                int PenCmdIndex = -1;
                if (!Force) {
                    // if command is a change plot pen command, or if a command nearby is
                    // a change plot pen command with no intervening draw commands, use that
                    // command as the vis pen command
                    // check here first
                    int checkindex = SelectedCmd.Index - 1;
                    if (SelectedCmd.Type == ChangePen &&
                        (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea)) {
                        PenCmdIndex = SelectedCmd.Index;
                    }
                    else {
                        // then check above
                        while (checkindex >= 0) {
                            switch ((DrawFunction)EditPicture.Data[(int)lstCommands.Items[checkindex].Tag]) {
                            case ChangePen:
                                PenCmdIndex = checkindex;
                                checkindex = 0;
                                break;
                            case EnableVis:
                            case DisableVis:
                            case EnablePri:
                            case DisablePri:
                                break;
                            default:
                                checkindex = 0;
                                break;
                            }
                            checkindex--;
                        }
                        if (PenCmdIndex == -1 && SelectedCmd.IsPen) {
                            if (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea) {
                                // not found above and selected cmd is not a draw cmd
                                // so check below
                                checkindex = SelectedCmd.Index + 1;
                                while (checkindex < lstCommands.Items.Count) {
                                    switch ((DrawFunction)EditPicture.Data[(int)lstCommands.Items[checkindex].Tag]) {
                                    case ChangePen:
                                        PenCmdIndex = checkindex;
                                        checkindex = lstCommands.Items.Count;
                                        break;
                                    case EnableVis:
                                    case DisableVis:
                                    case EnablePri:
                                    case DisablePri:
                                        break;
                                    default:
                                        checkindex = lstCommands.Items.Count;
                                        break;
                                    }
                                    checkindex++;
                                }
                            }
                        }
                    }
                }
                if (SelectedCmd.Pen.PlotStyle != newstyle) {
                    // adjust plot commands
                    ReadjustPlotCoordinates(SelectedCmd.Index, newstyle);
                }
                if (PenCmdIndex == -1 || Force) {
                    PenCmdIndex = SelectedCmd.Index;
                    InsertCommand([(byte)ChangePen, newpendata], SelectedCmd.Index);
                    SelectCommand(selectedindex + 1, 1, true);
                }
                else {
                    // update existing pen command
                    ChangePenSettings(PenCmdIndex, newpendata);
                    if (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea) {
                        SelectCommand(PenCmdIndex, 1, true);
                    }
                    else {
                        SelectCommand(selectedindex + 1, 1, true);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the picture resource with the passed visual pen 
        /// setting by either modifying an existing visual pen command
        /// at the current position, or adding a new visual pen command.
        /// </summary>
        /// <param name="newcolor"></param>
        /// <param name="Force"></param>
        private void UpdateVisPen(AGIColorIndex newcolor, bool Force = false) {
            //  if tool is edit or select area:
            //      if no intervening draw commands, edit the existing cmd or prior
            //      or following, otherwise insert a new cmd
            //  if tool is anything else:
            //      if no intervening draw commands, edit only prior (not current or
            //      following), otherwise insert a new cmd
            //  if a new cmd is added, select next cmd
            //  if cmd is edited and tool is edit or select area, select the edited
            //  cmd; if tool is anything else, select original cmd
            int selectedindex = SelectedCmd.Index;

            if (SelectedCmd.Pen.VisColor != newcolor || Force) {
                int VisCmdIndex = -1;
                if (!Force) {
                    // if command is a vis pen command, or if a command nearby is
                    // a vis pen command with no intervening draw commands, use that
                    // command as the vis pen command
                    // check here first
                    int checkindex = SelectedCmd.Index - 1;
                    if (SelectedCmd.Type == EnableVis || SelectedCmd.Type == DisableVis &&
                        (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea)) {
                        VisCmdIndex = SelectedCmd.Index;
                    }
                    else {
                        // then check above
                        while (checkindex >= 0) {
                            switch ((DrawFunction)EditPicture.Data[(int)lstCommands.Items[checkindex].Tag]) {
                            case EnableVis:
                            case DisableVis:
                                VisCmdIndex = checkindex;
                                checkindex = 0;
                                break;
                            case EnablePri:
                            case DisablePri:
                            case ChangePen:
                                break;
                            default:
                                checkindex = 0;
                                break;
                            }
                            checkindex--;
                        }
                        if (VisCmdIndex == -1 && SelectedCmd.IsPen) {
                            if (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea) {
                                // not found above and selected cmd is not a draw cmd
                                // so check below
                                checkindex = SelectedCmd.Index + 1;
                                while (checkindex < lstCommands.Items.Count) {
                                    switch ((DrawFunction)EditPicture.Data[(int)lstCommands.Items[checkindex].Tag]) {
                                    case EnableVis:
                                    case DisableVis:
                                        VisCmdIndex = checkindex;
                                        checkindex = lstCommands.Items.Count;
                                        break;
                                    case EnablePri:
                                    case DisablePri:
                                    case ChangePen:
                                        break;
                                    default:
                                        checkindex = lstCommands.Items.Count;
                                        break;
                                    }
                                    checkindex++;
                                }
                            }
                        }
                    }
                }
                if (VisCmdIndex == -1 || Force) {
                    // if pen is being turned off
                    if (newcolor == AGIColorIndex.None) {
                        // insert a visual disable cmd
                        InsertCommand([(byte)DisableVis], SelectedCmd.Index);
                    }
                    else {
                        // insert a visual enable cmd
                        InsertCommand([(byte)EnableVis, (byte)newcolor], SelectedCmd.Index);
                    }
                    SelectCommand(selectedindex + 1, 1, true);
                }
                else {
                    // update existing vis cmd
                    ChangePenColor(VisCmdIndex, newcolor);
                    if (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea) {
                        SelectCommand(VisCmdIndex, 1, true);
                    }
                    else {
                        SelectCommand(selectedindex + 1, 1, true);
                    }
                }
                picPalette.Refresh();
            }
        }

        /// <summary>
        /// Updates the picture resource with the passed priority pen 
        /// setting by either modifying an existing priority pen command
        /// at the current position, or adding a new priority pen command.
        /// </summary>
        /// <param name="newcolor"></param>
        /// <param name="Force"></param>
        private void UpdatePriPen(AGIColorIndex newcolor, bool Force = false) {
            //  if tool is edit or select area:
            //      if no intervening draw commands, edit the existing cmd or prior
            //      or following, otherwise insert a new cmd
            //  if tool is anything else:
            //      if no intervening draw commands, edit only prior (not current or
            //      following), otherwise insert a new cmd
            //  if a new cmd is added, select next cmd
            //  if cmd is edited and tool is edit or select area, select the edited
            //  cmd; if tool is anything else, select original cmd

            int selectedindex = SelectedCmd.Index;

            if (SelectedCmd.Pen.PriColor != newcolor || Force) {
                int PriCmdIndex = -1;
                if (!Force) {
                    // if command is a pri pen command, or if a command nearby is
                    // a pri pen command with no intervening draw commands, use that
                    // command as the pri pen command
                    // check here first
                    int checkindex = SelectedCmd.Index - 1;
                    if (SelectedCmd.Type == EnablePri || SelectedCmd.Type == DisablePri &&
                        (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea)) {
                        PriCmdIndex = SelectedCmd.Index;
                    }
                    else {
                        // then check above
                        while (checkindex >= 0) {
                            switch ((DrawFunction)EditPicture.Data[(int)lstCommands.Items[checkindex].Tag]) {
                            case EnablePri:
                            case DisablePri:
                                PriCmdIndex = checkindex;
                                checkindex = 0;
                                break;
                            case EnableVis:
                            case DisableVis:
                            case ChangePen:
                                break;
                            default:
                                checkindex = 0;
                                break;
                            }
                            checkindex--;
                        }
                        if (PriCmdIndex == -1 && SelectedCmd.IsPen) {
                            if (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea) {
                                // not found above and selected cmd is not a draw cmd
                                // so check below
                                checkindex = SelectedCmd.Index + 1;
                                while (checkindex < lstCommands.Items.Count) {
                                    switch ((DrawFunction)EditPicture.Data[(int)lstCommands.Items[checkindex].Tag]) {
                                    case EnablePri:
                                    case DisablePri:
                                        PriCmdIndex = checkindex;
                                        checkindex = lstCommands.Items.Count;
                                        break;
                                    case EnableVis:
                                    case DisableVis:
                                    case ChangePen:
                                        break;
                                    default:
                                        checkindex = lstCommands.Items.Count;
                                        break;
                                    }
                                    checkindex++;
                                }
                            }
                        }
                    }
                }
                if (PriCmdIndex == -1 || Force) {
                    // if pen is being turned off
                    if (newcolor == AGIColorIndex.None) {
                        // insert a priority disable cmd
                        InsertCommand([(byte)DisablePri], SelectedCmd.Index);
                    }
                    else {
                        // insert a Priority enable command
                        InsertCommand([(byte)EnablePri, (byte)newcolor], SelectedCmd.Index);
                    }
                    SelectCommand(selectedindex + 1, 1, true);
                }
                else {
                    // update existing pri cmd
                    ChangePenColor(PriCmdIndex, newcolor);
                    if (SelectedTool == PicToolType.Edit || SelectedTool == PicToolType.SelectArea) {
                        SelectCommand(PriCmdIndex, 1, true);
                    }
                    else {
                        SelectCommand(selectedindex + 1, 1, true);
                    }
                }
            }
        }

        /// <summary>
        /// Modifies an existing visual or priority pen command to a new color
        /// value and updates the drawing surfaces.
        /// </summary>
        /// <param name="CmdIndex"></param>
        /// <param name="NewColor"></param>
        /// <param name="DontUndo"></param>
        private void ChangePenColor(int CmdIndex, AGIColorIndex NewColor, bool DontUndo = false) {
            AGIColorIndex OldColor;
            int lngPos = (int)lstCommands.Items[CmdIndex].Tag;

            // get color of current command
            if (EditPicture.Data[lngPos].IsOdd()) {
                OldColor = AGIColorIndex.None;
            }
            else {
                OldColor = (AGIColorIndex)EditPicture.Data[lngPos + 1];
            }
            // it is possible that a change request is made
            // even though colors are the same
            if (OldColor == NewColor) {
                // just exit
                return;
            }

            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = ChangeColor;
                NextUndo.PicPos = lngPos;
                NextUndo.CmdIndex = CmdIndex;
                NextUndo.Data = [(byte)OldColor];
                AddUndo(NextUndo);
            }

            // if old color is none
            if (OldColor == AGIColorIndex.None) {
                // change command to enable
                EditPicture.Data[lngPos]--;
                lstCommands.Items[CmdIndex].Text = ((DrawFunction)EditPicture.Data[lngPos]).CommandName();
                // insert color
                EditPicture.InsertData((byte)NewColor, lngPos + 1);
                // update all following commands
                UpdatePosValues(CmdIndex + 1, 1);
            }
            else if (NewColor == AGIColorIndex.None) {
                // change command to disable by adding one
                EditPicture.Data[lngPos]++;
                lstCommands.Items[CmdIndex].Text = ((DrawFunction)EditPicture.Data[lngPos]).CommandName();
                // delete color byte
                EditPicture.RemoveData(lngPos + 1);
                // update all following commands
                UpdatePosValues(CmdIndex + 1, -1);
            }
            else {
                // change color byte
                EditPicture.Data[lngPos + 1] = (byte)NewColor;
            }
        }

        /// <summary>
        /// Modifies an existing change-pen command to a new settings value
        /// and updates the drawing surfaces.
        /// </summary>
        /// <param name="CmdIndex"></param>
        /// <param name="NewPenData"></param>
        /// <param name="DontUndo"></param>
        private void ChangePenSettings(int CmdIndex, byte NewPenData, bool DontUndo = false) {
            // changes the pen settings for this command

            int lngPos = (int)lstCommands.Items[CmdIndex].Tag + 1;
            // it is possible that a change request is made
            // even though pen settings are the same
            if (EditPicture.Data[lngPos] == NewPenData) {
                // just exit
                return;
            }
            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = PictureUndo.ActionType.ChangePlotPen;
                NextUndo.PicPos = lngPos;
                NextUndo.CmdIndex = CmdIndex;
                NextUndo.Data = [EditPicture.Data[lngPos]];
                AddUndo(NextUndo);
            }
            // change pen byte
            EditPicture.Data[lngPos] = NewPenData;

        }

        /// <summary>
        /// Determines the starting (upper left corner) and the size of
        /// the region bounded by the coordinates of currently selected
        /// commands, and sets selection box to match. Optionally draws
        /// a selection box on the drawing surfaces to highlight the
        /// selection.
        /// </summary>
        /// <param name="ShowBox"></param>
        private void SetSelectionBounds(bool ShowBox = false) {
            int lngPos;
            byte bytX, bytY, bytCmd;
            int xdisp, ydisp;
            bool blnRelX = false;
            Rectangle retval = new(0, 0, -1, -1);

            // go through each cmd; check it for coordinates
            // if coordinates are found, step through them to
            // determine if any coords expand the selected area

            // NOTE: for plots, need to be aware of pen status
            // so coordinate values get extracted correctly
            PlotStyle tmpPlotStyle = EditPicture.GetPenStatus((int)lstCommands.SelectedItems[0].Tag).PlotStyle;
            byte[] bytData = EditPicture.Data;
            foreach (ListViewItem item in lstCommands.SelectedItems) {
                // set starting pos for this cmd
                lngPos = (int)item.Tag;
                bytCmd = bytData[lngPos++];
                // parse coords based on cmdtype
                switch (bytCmd) {
                case (byte)ChangePen:
                    // update plot parameters
                    bytCmd = bytData[lngPos++];
                    tmpPlotStyle = (PlotStyle)((bytCmd & 0x20) / 0x20);
                    break;
                case (byte)YCorner or (byte)XCorner:
                    // set initial direction
                    blnRelX = (bytCmd == 0xF5);
                    // get coordinates
                    bytX = bytData[lngPos++];
                    if (bytX >= 0xF0) {
                        break;
                    }
                    bytY = bytData[lngPos++];
                    if (bytX >= 0xF0) {
                        break;
                    }
                    retval = retval.Expand(bytX, bytY);
                    bytCmd = bytData[lngPos++];
                    while (bytCmd < 0xF0) {
                        if (blnRelX) {
                            bytX = bytCmd;
                        }
                        else {
                            bytY = bytCmd;
                        }
                        blnRelX = !blnRelX;
                        retval = retval.Expand(bytX, bytY);
                        bytCmd = bytData[lngPos++];
                    }
                    break;
                case (byte)AbsLine or (byte)Fill:
                    do {
                        bytX = bytData[lngPos++];
                        if (bytX >= 0xF0) {
                            break;
                        }
                        bytY = bytData[lngPos++];
                        if (bytX >= 0xF0) {
                            break;
                        }
                        // compare to start/end
                        retval = retval.Expand(bytX, bytY);
                    } while (true);
                    break;
                case (byte)RelLine:
                    // get coordinates
                    bytX = bytData[lngPos++];
                    if (bytX >= 0xF0) {
                        break;
                    }
                    bytY = bytData[lngPos++];
                    if (bytY >= 0xF0) {
                        break;
                    }
                    retval = retval.Expand(bytX, bytY);
                    bytCmd = bytData[lngPos++];
                    while (bytCmd < 0xF0) {
                        // if horizontal negative bit set
                        if ((bytCmd & 0x80) > 0) {
                            xdisp = -((bytCmd & 0x70) / 0x10);
                        }
                        else {
                            xdisp = (bytCmd & 0x70) / 0x10;
                        }
                        // if vertical negative bit is set
                        if ((bytCmd & 0x8) > 1) {
                            ydisp = -(bytCmd & 0x7);
                        }
                        else {
                            ydisp = bytCmd & 0x7;
                        }
                        bytX = (byte)(bytX + xdisp);
                        bytY = (byte)(bytY + ydisp);
                        // compare to start/end
                        retval = retval.Expand(bytX, bytY);
                        // read in next command
                        bytCmd = bytData[lngPos++];
                    }
                    break;
                case (byte)PlotPen:
                    do {
                        // if brush is splatter
                        if (tmpPlotStyle > 0) {
                            // skip splatter byte
                            bytCmd = bytData[lngPos++];
                            if (bytCmd >= 0xF0) {
                                break;
                            }
                        }
                        // get coordinates
                        bytX = bytData[lngPos++];
                        if (bytX >= 0xF0) {
                            break;
                        }
                        bytY = bytData[lngPos++];
                        if (bytY >= 0xF0) {
                            break;
                        }
                        // compare to start/end
                        retval = retval.Expand(bytX, bytY);
                    } while (true);
                    break;
                }
            }
            if (retval.Width != -1) {
                retval.Width += 1;
                retval.Height += 1;
            }
            SelectedRegion = retval;
            if (SelectedRegion.Width > 0 && SelectedRegion.Height > 0) {
                tmrSelect.Enabled = true;
                spAnchor.Visible = true;
                spBlock.Visible = true;
                spAnchor.Text = "Anchor: " + SelectedRegion.X + ", " + SelectedRegion.Y;
                spBlock.Text = "Block: " + SelectedRegion.X + ", " + SelectedRegion.Y + ", " + SelectedRegion.Right + ", " + SelectedRegion.Bottom;
            }
            else {
                tmrSelect.Enabled = false;
                spAnchor.Visible = false;
                spBlock.Visible = false;
            }
            tsbFlipH.Enabled = SelectedRegion.Width > 1;
            tsbFlipV.Enabled = SelectedRegion.Height > 1;
        }

        /// <summary>
        /// Builds the coordinate list for the currently selected command
        /// and adds the coordinates to the coordinate listbox.
        /// </summary>
        private void BuildCoordList() {
            lstCoords.Items.Clear();
            switch (SelectedCmd.Type) {
            case DisableVis:
            case DisablePri:
                lblCoords.Text = "Parameter";
                break;
            case EnableVis:
                lblCoords.Text = "Parameter";
                byte vc = EditPicture.Data[SelectedCmd.Position + 1];
                if (vc > 15) {
                    lstCoords.Items.Add("ERR(0x" + vc.ToString("x2") + ")");
                }
                else {
                    lstCoords.Items.Add(((AGIColorIndex)vc).ToString());
                }
                break;
            case EnablePri:
                lblCoords.Text = "Parameter";
                byte pc = EditPicture.Data[SelectedCmd.Position + 1];
                if (pc > 15) {
                    lstCoords.Items.Add("ERR(0x" + pc.ToString("x2") + ")");
                }
                else {
                    lstCoords.Items.Add(((AGIColorIndex)pc).ToString());
                }
                break;
            case ChangePen:
                lblCoords.Text = "Parameters";
                lstCoords.Items.Add(SelectedCmd.Pen.PlotStyle.ToString());
                lstCoords.Items.Add(SelectedCmd.Pen.PlotShape.ToString());
                lstCoords.Items.Add("Size: " + SelectedCmd.Pen.PlotSize.ToString());
                break;
            default:
                lblCoords.Text = "Coordinates";
                if (SelectedCmd.Type == PlotPen) {
                    for (int i = 0; i < SelectedCmd.Coords.Count; i++) {
                        if (SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                            lstCoords.Items.Add((EditPicture.Data[SelectedCmd.Position + 3 * i + 1] / 2).ToString() + " -- " + CoordText(SelectedCmd.Coords[i]));
                        }
                        else {
                            lstCoords.Items.Add(CoordText(SelectedCmd.Coords[i]));
                        }
                    }
                }
                else {
                    for (int i = 0; i < SelectedCmd.Coords.Count; i++) {
                        lstCoords.Items.Add(CoordText(SelectedCmd.Coords[i]));
                    }
                }
                break;
            }
            CoordColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        /// <summary>
        /// Enables or disables the coordinate listbox based on the currently
        /// selected command and tool.
        /// </summary>
        private void EnableCoordList() {
            switch (PicMode) {
            case PicEditorMode.Edit:
                switch (SelectedTool) {
                case PicToolType.Edit:
                    lstCoords.Enabled = SelectedCmd.Type > DisablePri && SelectedCmd.Type != ChangePen;
                    break;
                case PicToolType.Fill:
                case PicToolType.Plot:
                    switch (SelectedCmd.Type) {
                    case Fill:
                        lstCoords.Enabled = SelectedTool == PicToolType.Fill;
                        break;
                    case PlotPen:
                        lstCoords.Enabled = SelectedTool == PicToolType.Plot;
                        break;
                    default:
                        lstCoords.Enabled = false;
                        break;
                    }
                    break;
                default:
                    lstCoords.Enabled = false;
                    break;
                }
                break;
            default:
                lstCoords.Enabled = false;
                break;
            }
            if (!lstCoords.Enabled) {
                lstCoords.SelectedItems.Clear();
            }
        }

        /// <summary>
        /// Converts an x/y coordinate pair into a text string in the
        /// format '(x, y)'
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public string CoordText(byte X, byte Y) {
            return "(" + X + ", " + Y + ")";
        }

        /// <summary>
        /// Converts a Point object into a text string in the format
        /// '(x, y)'.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public string CoordText(Point pos) {
            // this function creates the coordinate text in the form
            //     (X, Y)
            return "(" + pos.X + ", " + pos.Y + ")";
        }

        /// <summary>
        /// Adjusts the scale factor of the draw images up or down one 
        /// level and redraws the images.
        /// </summary>
        /// <param name="Dir"></param>
        /// <param name="useanchor"></param>
        public void ChangeScale(int Dir, bool useanchor = false) {
            // valid scale factors are:
            // 100%, 125%, 150%, 175%, 200%, 225%, 250%, 275%, 300%,
            // 350%, 400%, 450%, 500%, 550%, 600%, 650%, 700%, 750%, 800%,
            // 900%, 1000%, 1100%, 1200%, 1300%, 1400%, 1500%, 1600%, 1700%, 1800%, 1900%, 2000%
            float oldscale = 0;
            if (useanchor) {
                oldscale = ScaleFactor;
            }
            switch (Dir) {
            case > 0:
                if (ScaleFactor < 3) {
                    ScaleFactor += 0.25f;
                }
                else if (ScaleFactor < 8) {
                    ScaleFactor += 0.5f;
                }
                else if (ScaleFactor < 20) {
                    ScaleFactor += 1;
                }
                break;
            case < 0:
                if (ScaleFactor > 8) {
                    ScaleFactor -= 1;
                }
                else if (ScaleFactor > 3) {
                    ScaleFactor -= 0.5f;
                }
                else if (ScaleFactor > 1) {
                    ScaleFactor -= 0.25f;
                }
                break;
            }
            if (oldscale == ScaleFactor) {
                return;
            }
            _ = SendMessage(splitImages.Panel1.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picVisual.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picPriority.Handle, WM_SETREDRAW, false, 0);
            // resize images
            picVisual.Width = picPriority.Width = (int)(320 * ScaleFactor);
            picVisual.Height = picPriority.Height = (int)(168 * ScaleFactor);
            // then set the scrollbars
            SetScrollbars(oldscale);
            // redraw pictures at new scale
            DrawPicture();
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";
            _ = SendMessage(picPriority.Handle, WM_SETREDRAW, true, 0);
            _ = SendMessage(picVisual.Handle, WM_SETREDRAW, true, 0);
            _ = SendMessage(splitImages.Panel1.Handle, WM_SETREDRAW, true, 0);
            splitImages.Refresh();
        }

        /// <summary>
        /// Shows or hides scrollbars for the draw surfaces when the form
        /// is resized, split fraction changes or image scale is changed.
        /// </summary>
        /// <param name="oldscale"></param>
        private void SetScrollbars(float oldscale = 0) {
            bool showHSB = false, showVSB = false;

            if (!splitImages.Visible) {
                // skip during form load to avoid errors
                // (the form is not visible at that point)
                return;
            }
            if (splitImages.Panel1.Height > 16) {
                // determine if scrollbars are necessary
                showHSB = picVisual.Width > (splitImages.Panel1.ClientSize.Width - 2 * PE_MARGIN);
                showVSB = picVisual.Height > (splitImages.Panel1.ClientSize.Height - 2 * PE_MARGIN - (showHSB ? hsbVisual.Height : 0));
                // check horizontal again(in case addition of vert scrollbar forces it to be shown)
                showHSB = picVisual.Width > (splitImages.Panel1.ClientSize.Width - 2 * PE_MARGIN - (showVSB ? vsbVisual.Width : 0));
                // initial positions
                hsbVisual.Top = splitImages.Panel1.ClientSize.Height - hsbVisual.Height;
                vsbVisual.Left = splitImages.Panel1.ClientSize.Width - vsbVisual.Width;
                hsbVisual.Width = splitImages.Panel1.ClientSize.Width;
                vsbVisual.Height = splitImages.Panel1.ClientSize.Height;
                if (showHSB && showVSB) {
                    // allow for corner
                    picCornerVis.Left = vsbVisual.Left;
                    picCornerVis.Top = hsbVisual.Top;
                    vsbVisual.Height -= hsbVisual.Height;
                    hsbVisual.Width -= vsbVisual.Width;
                    picCornerVis.Visible = true;
                }
                else {
                    picCornerVis.Visible = false;
                }
                hsbVisual.Visible = showHSB;
                vsbVisual.Visible = showVSB;
            }
            else {
                hsbVisual.Visible = false;
                vsbVisual.Visible = false;
            }

            if (splitImages.Panel2.Height > 16) {
                // determine if scrollbars are necessary
                showHSB = picPriority.Width > (splitImages.Panel2.ClientSize.Width - 2 * PE_MARGIN);
                showVSB = picPriority.Height > (splitImages.Panel2.ClientSize.Height - 2 * PE_MARGIN - (showHSB ? hsbPriority.Height : 0));
                // check horizontal again(in case addition of vert scrollbar forces it to be shown)
                showHSB = picPriority.Width > (splitImages.Panel2.ClientSize.Width - 2 * PE_MARGIN - (showVSB ? vsbPriority.Width : 0));
                // initial positions
                hsbPriority.Top = splitImages.Panel2.ClientSize.Height - hsbPriority.Height;
                vsbPriority.Left = splitImages.Panel2.ClientSize.Width - vsbPriority.Width;
                hsbPriority.Width = splitImages.Panel2.ClientSize.Width;
                vsbPriority.Height = splitImages.Panel2.ClientSize.Height;
                if (showHSB && showVSB) {
                    // allow for corner
                    picCornerPri.Left = vsbPriority.Left;
                    picCornerPri.Top = hsbPriority.Top;
                    vsbPriority.Height -= hsbPriority.Height;
                    hsbPriority.Width -= vsbPriority.Width;
                    picCornerPri.Visible = true;
                }
                else {
                    picCornerPri.Visible = false;
                }
                hsbPriority.Visible = showHSB;
                vsbPriority.Visible = showVSB;
            }
            else {
                hsbPriority.Visible = false;
                vsbPriority.Visible = false;
            }

            Point anchorpt = new(-1, -1);
            if (oldscale > 0) {
                // if using anchor point need to determine which image the
                // cursor is in
                Point cp = splitImages.Panel1.PointToClient(Cursor.Position);
                if (splitImages.Panel1.ClientRectangle.Contains(cp)) {
                    // use this anchor
                    anchorpt = cp;
                }
                else {
                    cp = splitImages.Panel2.PointToClient(Cursor.Position);
                    if (splitImages.Panel2.ClientRectangle.Contains(cp)) {
                        // use this anchor
                        anchorpt = cp;
                    }
                    else {
                        // not a valid anchor
                        oldscale = 0;
                    }
                }
            }
            // now adjust all scrollbar parameters as needed
            AdjustScrollbars(oldscale, anchorpt, hsbVisual, vsbVisual, picVisual, splitImages.Panel1);
            AdjustScrollbars(oldscale, anchorpt, hsbPriority, vsbPriority, picPriority, splitImages.Panel2);
        }

        /// <summary>
        /// Sets the scrollbar properties for the passed draw surface so
        /// the image can be properly scrolled by dragging and/or using
        /// the scrollbar buttons.
        /// </summary>
        /// <param name="oldscale"></param>
        /// <param name="anchor"></param>
        /// <param name="hsb"></param>
        /// <param name="vsb"></param>
        /// <param name="image"></param>
        /// <param name="panel"></param>
        private void AdjustScrollbars(double oldscale, Point anchor, HScrollBar hsb, VScrollBar vsb, PictureBox image, SplitterPanel panel) {
            // Scrollbar math:
            // ACT_SZ = size of the area being scrolled; usually the image size + margins
            // WIN_SZ = size of the window area; the container's client size
            // SV_MAX = maximum value that scrollbar can have; this puts the scroll bar
            //          and scrolled image at farthest position
            // LG_CHG = LargeChange property of the scrollbar
            // SB_MAX = actual Maximum property of the scrollbar, to avoid out-of-bounds errors
            //
            //      SV_MAX = ACT_SZ - WIN_SZ 
            //      SB_MAX = SV_MAX + LG_CHG + 1
            //
            // when including margins, the calculations are modified to:
            //      ACT_SZ = MGN + IMG_SZ + MGN
            //      SB_MIN = -MGN
            //      SV_MAX = ACT_SZ - WIN_SZ + SB_MIN
            //             = MGN + IMG_SZ + MGN + SB_MIN - WIN_SZ
            //             = MGN + IMG_SZ + MGN - MGN - WIN_SZ
            //      SV_MAX = IMG_SZ - WIN_SZ + MGN

            if (hsb.Visible) {
                // (LargeChange value can't exceed Max value, so set Max to high enough
                // value so it can be calculated correctly later)
                hsb.Maximum = image.Width;
                hsb.LargeChange = (int)(panel.ClientSize.Width * LG_SCROLL);
                hsb.SmallChange = (int)(panel.ClientSize.Width * SM_SCROLL);
                // calculate actual max (when image is fully scrolled to right)
                int SV_MAX = image.Width - (panel.ClientSize.Width - (vsb.Visible ? vsb.Width : 0)) + PE_MARGIN;
                // control MAX value equals actual Max + LargeChange - 1
                hsb.Maximum = SV_MAX + hsb.LargeChange - 1;
                int newscroll;
                if (oldscale > 0) {
                    // if cursor is over the image, use cursor pos as anchor point
                    // the correct algebra to make this work is:
                    //         SB1 = SB0 + (SB0 + WAN - MGN) * (SF1 / SF0 - 1)
                    // SB = scrollbar value
                    // WAN = panel client window anchor point (get from cursor pos)
                    // MGN is the left/top margin
                    // SF = scale factor (as calculated above)
                    // -0 = previous values
                    // -1 = new (desired) values
                    newscroll = (int)(hsb.Value + (hsb.Value + anchor.X - PE_MARGIN) * (ScaleFactor / oldscale - 1));
                }
                else {
                    newscroll = hsb.Value;
                }
                if (newscroll < -PE_MARGIN) {
                    hsb.Value = -PE_MARGIN;
                }
                else if (newscroll > SV_MAX) {
                    hsb.Value = SV_MAX;
                }
                else {
                    hsb.Value = newscroll;
                }
            }
            else {
                // reset to default
                hsb.Value = -PE_MARGIN;
            }
            // readjust picture position
            image.Left = -hsb.Value;

            // repeat for vertical scrollbar
            if (vsb.Visible) {
                vsb.Maximum = image.ClientSize.Height;
                vsb.LargeChange = (int)(panel.ClientSize.Height * LG_SCROLL);
                vsb.SmallChange = (int)(panel.ClientSize.Height * SM_SCROLL);
                int SV_MAX = image.Height - (panel.ClientSize.Height - (hsb.Visible ? hsb.Height : 0)) + PE_MARGIN;
                vsb.Maximum = SV_MAX + vsb.LargeChange - 1;
                int newscroll;
                if (oldscale > 0) {
                    newscroll = (int)(vsb.Value + (vsb.Value + anchor.Y - PE_MARGIN) * (ScaleFactor / oldscale - 1));
                }
                else {
                    newscroll = vsb.Value;
                }
                if (newscroll < -PE_MARGIN) {
                    vsb.Value = -PE_MARGIN;
                }
                else if (newscroll > SV_MAX) {
                    vsb.Value = SV_MAX;
                }
                else {
                    vsb.Value = newscroll;
                }
            }
            else {
                vsb.Value = -PE_MARGIN;
            }
            image.Top = -vsb.Value;
        }

        /// <summary>
        /// Draws the line defined by start and end x/y coordinate pairs
        /// on the passed draw surface using AGI's line drawing algorithm. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="linecolor"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        private void DrawLineOnImage(Graphics g, Color linecolor, int x1, int y1, int x2, int y2) {
            // draw a line on the image
            // this function is used to draw the lines for the
            // AGI line drawing functions
            Point p1 = new(x1, y1);
            Point p2 = new(x2, y2);
            DrawLineOnImage(g, linecolor, p1, p2);
        }
        
        /// <summary>
        /// Draws the line defined by start and end Point values on the
        /// passed draw surface using AGI's line drawing algorithm.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="linecolor"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        private void DrawLineOnImage(Graphics g, Color linecolor, Point p1, Point p2) {
            int xPos, yPos, XC, YC, MaxDelta;
            Pen lc = new(linecolor);
            SolidBrush lb = new(linecolor);

            // determine height/width  
            int DY = p2.Y - p1.Y;
            int vDir = Math.Sign(DY);
            int DX = p2.X - p1.X;
            int hDir = Math.Sign(DX);
            // check for a point, vertical line, or horizontal line  
            if (DY == 0 || DX == 0) {
                // convert line to top-bottom/left-right format so graphics methods  
                // work correctly  
                if (p2.Y < p1.Y) {
                    // swap  
                    yPos = p1.Y;
                    p1.Y = p2.Y;
                    p2.Y = yPos;
                    DY *= -1;
                }
                if (p2.X < p1.X) {
                    xPos = p1.X;
                    p1.X = p2.X;
                    p2.X = xPos;
                    DX *= -1;
                }
                if (ScaleFactor == 1) {
                    g.DrawLine(lc, p1.X * 2, p1.Y, p2.X * 2, p2.Y);
                }
                else {
                    g.FillRectangle(lb, p1.X * ScaleFactor * 2, p1.Y * ScaleFactor, (DX + 1) * ScaleFactor * 2, (DY + 1) * ScaleFactor);
                }
            }
            else {
                // this line drawing function EXACTLY matches the Sierra drawing function  

                // set the starting point  
                if (ScaleFactor == 1) {
                    g.DrawLine(lc, p1.X * 2, p1.Y, p1.X * 2 + 1, p1.Y);
                }
                else {
                    g.FillRectangle(lb, p1.X * ScaleFactor * 2, p1.Y * ScaleFactor, ScaleFactor * 2, ScaleFactor);
                }
                xPos = p1.X;
                yPos = p1.Y;

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
                for (int i = 1; i <= MaxDelta; i++) {
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
                    if (ScaleFactor == 1) {
                        g.DrawLine(lc, xPos * 2, yPos, xPos * 2 + 2, yPos);
                    }
                    else {
                        g.FillRectangle(lb, xPos * ScaleFactor * 2, yPos * ScaleFactor, ScaleFactor * 2, ScaleFactor);
                    }
                }
            }
        }

        /// <summary>
        /// Draws tempoary line segments based around the currently selected 
        /// coordinate as it gets moved during editing or drawing operations.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="linecolor"></param>
        private void DrawTempSegments(Graphics g, Color linecolor) {
            Point coord;
            if (PicDrawMode == PicDrawOp.MovePoint) {
                coord = EditPt;
            }
            else {
                coord = CoordPt;
            }
            switch (SelectedCmd.Type) {
            case XCorner:
            case YCorner:
                bool horizontalsegment = SelectedCmd.SelectedCoordIndex.IsEven();
                Point cp = new(0, 0);
                if (SelectedCmd.Type == YCorner) {
                    horizontalsegment = !horizontalsegment;
                }
                if (SelectedCmd.SelectedCoordIndex - 2 >= 0) {
                    if (horizontalsegment) {
                        cp.X = coord.X;
                        cp.Y = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex - 1].Y;
                    }
                    else {
                        cp.X = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex - 1].X;
                        cp.Y = coord.Y;
                    }
                    DrawLineOnImage(g, linecolor, SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex - 2], cp);
                }
                if (SelectedCmd.SelectedCoordIndex - 1 >= 0) {
                    if (horizontalsegment) {
                        cp.X = coord.X;
                        cp.Y = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex - 1].Y;
                    }
                    else {
                        cp.X = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex - 1].X;
                        cp.Y = coord.Y;
                    }
                    DrawLineOnImage(g, linecolor, cp, coord);
                }
                if (SelectedCmd.SelectedCoordIndex + 1 < SelectedCmd.Coords.Count) {
                    if (horizontalsegment) {
                        cp.X = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex + 1].X;
                        cp.Y = coord.Y;
                    }
                    else {
                        cp.X = coord.X;
                        cp.Y = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex + 1].Y;
                    }
                    DrawLineOnImage(g, linecolor, coord, cp);
                }
                if (SelectedCmd.SelectedCoordIndex + 2 < SelectedCmd.Coords.Count) {
                    if (horizontalsegment) {
                        cp.X = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex + 2].X;
                        cp.Y = coord.Y;
                    }
                    else {
                        cp.X = coord.X;
                        cp.Y = SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex + 2].Y;
                    }
                    DrawLineOnImage(g, linecolor, cp, SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex + 2]);
                }
                break;
            default:
                // must be rel line or abs line
                if (SelectedCmd.SelectedCoordIndex - 1 >= 0) {
                    DrawLineOnImage(g, linecolor, SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex - 1], coord);
                }
                if (SelectedCmd.SelectedCoordIndex + 1 < SelectedCmd.Coords.Count) {
                    DrawLineOnImage(g, linecolor, coord, SelectedCmd.Coords[SelectedCmd.SelectedCoordIndex + 1]);
                }
                break;
            }
        }

        /// <summary>
        /// Creates the arc segment lines needed to draw an ellipse that
        /// is bounded by the passed coordinates.
        /// </summary>
        /// <param name="beginpt"></param>
        /// <param name="endpt"></param>
        /// <returns></returns>
        private Point[] BuildCircleArcs(Point beginpt, Point endpt) {
            int segmentcount = 0;
            Point[] arcpts;
            int DX, DY;
            double a, b, a2b2, cy, cx, s1, s2;
            bool DelPt;

            // ensure we are in a upperleft-lower right configuration
            if (beginpt.X > endpt.X) {
                int tmp = beginpt.X;
                beginpt.X = endpt.X;
                endpt.X = tmp;
            }
            if (beginpt.Y > endpt.Y) {
                int tmp = beginpt.Y;
                beginpt.Y = endpt.Y;
                endpt.Y = tmp;
            }
            DX = endpt.X - beginpt.X;
            DY = endpt.Y - beginpt.Y;
            if (DX == 0 || DY == 0) {
                // no arcs, just a line
                segmentcount = 0;
                return [];
            }
            if (DX == 1 || DY == 1) {
                // no arcs, draw a simple box
                segmentcount = 2;
                arcpts = new Point[2];
                arcpts[0].X = DX / 2;
                arcpts[0].Y = 0;
                arcpts[1].X = 0;
                arcpts[1].Y = DY / 2;
                return arcpts;
            }
            // set array size large enough to ensure
            // no out of bounds errors
            arcpts = new Point[DX + DY];
            a = (int)(DX / 2);
            b = (int)(DY / 2);
            a2b2 = (a * a) / (b * b);
            // start with Y values;
            // increment until slope is >=1
            int i = 0;
            do {
                arcpts[i].Y = i;
                // calculate x Value for this Y
                cx = a * Math.Sqrt(1 - (arcpts[i].Y * arcpts[i].Y / (b * b)));
                // round it (offset by 0.3 - this is an empirical value
                // that seems to result in more accurate circles)
                arcpts[i].X = (int)Math.Round(cx - 0.3);
                // if past limit
                if (i / cx * a2b2 >= 1) {
                    break; // exit do
                }
                // increment Y
                i++;
                // continue until last point reached
                // (necessary in case tall skinny oval
                // is drawn; slope won't reach 1 before last point)
            } while (i < b);
            // start with last x
            int j = arcpts[i - 1].X;
            // now, decrement x until we get to zero
            do {
                arcpts[i].X = j;
                // calculate Y Value for this x
                cy = b * Math.Sqrt(1 - arcpts[i].X * arcpts[i].X / (a * a));
                // round it (offset by 0.3 - this is an empirical value
                // that seems to result in more accurate circles)
                arcpts[i].Y = (int)Math.Round(cy - 0.3);
                // decrement x, increment counter
                j--;
                i++;
            } while (j >= 0);

            segmentcount = i;
            // strip out any zero delta points
            // and any points that match slope on both sides
            i = 1;
            do {
                if (arcpts[i] == arcpts[i - 1]) {
                    // points are the same
                    DelPt = true;
                }
                else if (arcpts[i].X == arcpts[i - 1].X && arcpts[i].X == arcpts[i + 1].X) {
                    // point i-1 to i+1 is a horizontal line
                    DelPt = true;
                }
                else if (arcpts[i].Y == arcpts[i - 1].Y && arcpts[i].Y == arcpts[i + 1].Y) {
                    // point i-1 to i+1 is a vertical line
                    DelPt = true;
                }
                else if ((arcpts[i].X - arcpts[i - 1].X == arcpts[i - 1].Y - arcpts[i].Y) &&
                         (arcpts[i + 1].X - arcpts[i].X == arcpts[i].Y - arcpts[i + 1].Y)) {
                    // point i-1 to i+1 is a constant slope of 1
                    DelPt = true;
                }
                else {
                    DelPt = false;
                }
                if (DelPt) {
                    // move all segments down one space
                    for (int k = i + 1; k < segmentcount; k++) {
                        arcpts[k - 1] = arcpts[k];
                    }
                    arcpts[segmentcount - 1].X = 0;
                    arcpts[segmentcount - 1].Y = 0;
                    segmentcount--;
                    i--;
                }
                i++;
            } while (i < segmentcount - 1);

            // if more than two segments
            if (segmentcount > 2) {
                // strip out any points that create uneven slopes
                i = 1;
                do {
                    if (arcpts[i - 1].X == arcpts[i].X) {
                        s1 = -160;
                    }
                    else {
                        s1 = (double)(arcpts[i - 1].Y - arcpts[i].Y) / (arcpts[i - 1].X - arcpts[i].X);
                    }
                    if (arcpts[i].X == arcpts[i + 1].X) {
                        s2 = -160;
                    }
                    else {
                        s2 = (double)(arcpts[i].Y - arcpts[i + 1].Y) / (arcpts[i].X - arcpts[i + 1].X);
                    }
                    if (s1 >= s2 || arcpts[i].X < arcpts[i + 1].X) {
                        for (int k = i + 1; k < segmentcount; k++) {
                            arcpts[k - 1] = arcpts[k];
                        }
                        arcpts[segmentcount - 1].X = 0;
                        arcpts[segmentcount - 1].Y = 0;
                        segmentcount--;
                        // back up to recheck slope of altered segment
                        i--;
                    }
                    i++;
                } while (i < segmentcount - 1);
            }
            Array.Resize(ref arcpts, segmentcount);
            return arcpts;
        }

        /// <summary>
        /// Draws a circle/ellipse that is bounded by start/end points
        /// using the arc segments in ArcPts array.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pencolor"></param>
        /// <param name="beginpt"></param>
        /// <param name="endpt"></param>
        private void DrawCircle(Graphics g, Color pencolor, Point beginpt, Point endpt) {
            int DX, DY;

            // ensure we are in a upperleft-lower right configuration
            if (beginpt.X > endpt.X) {
                (endpt.X, beginpt.X) = (beginpt.X, endpt.X);
            }
            if (beginpt.Y > endpt.Y) {
                (endpt.Y, beginpt.Y) = (beginpt.Y, endpt.Y);
            }
            DX = endpt.X - beginpt.X;
            DY = endpt.Y - beginpt.Y;
            if (DX == 0 || DY == 0) {
                // no arcs, just a line
                DrawLineOnImage(g, pencolor, beginpt.X, beginpt.Y, endpt.X, endpt.Y);
                return;
            }
            if (DX == 1 || DY == 1) {
                // no arcs, draw a simple box
                DrawLineOnImage(g, pencolor, beginpt.X, beginpt.Y, endpt.X, beginpt.Y);
                DrawLineOnImage(g, pencolor, endpt.X, beginpt.Y, endpt.X, endpt.Y);
                DrawLineOnImage(g, pencolor, endpt.X, endpt.Y, beginpt.X, endpt.Y);
                DrawLineOnImage(g, pencolor, beginpt.X, endpt.Y, beginpt.X, beginpt.Y);
                return;
            }
            // draw the ellipse as four 90 degree arcs, using the arc segments
            // that have been previously calculated
            int pX = beginpt.X;
            int pY = beginpt.Y + ArcPts[^1].Y;
            for (int i = 1; i < ArcPts.Length; i++) {
                DrawLineOnImage(g, pencolor, pX, pY, beginpt.X + ArcPts[0].X - ArcPts[i].X, beginpt.Y + ArcPts[^1].Y - ArcPts[i].Y);
                pX = beginpt.X + ArcPts[0].X - ArcPts[i].X;
                pY = beginpt.Y + ArcPts[^1].Y - ArcPts[i].Y;
            }
            for (int i = ArcPts.Length - 1; i >= 0; i--) {
                DrawLineOnImage(g, pencolor, endpt.X - ArcPts[0].X + ArcPts[i].X, beginpt.Y + ArcPts[^1].Y - ArcPts[i].Y, pX, pY);
                pX = endpt.X - ArcPts[0].X + ArcPts[i].X;
                pY = beginpt.Y + ArcPts[^1].Y - ArcPts[i].Y;
            }
            for (int i = 0; i < ArcPts.Length; i++) {
                DrawLineOnImage(g, pencolor, pX, pY, endpt.X - ArcPts[0].X + ArcPts[i].X, endpt.Y - ArcPts[^1].Y + ArcPts[i].Y);
                pX = endpt.X - ArcPts[0].X + ArcPts[i].X;
                pY = endpt.Y - ArcPts[^1].Y + ArcPts[i].Y;
            }
            for (int i = ArcPts.Length - 1; i >= 0; i--) {
                DrawLineOnImage(g, pencolor, beginpt.X + ArcPts[0].X - ArcPts[i].X, endpt.Y - ArcPts[^1].Y + ArcPts[i].Y, pX, pY);
                pX = beginpt.X + ArcPts[0].X - ArcPts[i].X;
                pY = endpt.Y - ArcPts[^1].Y + ArcPts[i].Y;
            }
        }

        /// <summary>
        /// Draws a rectangular box on the draw surfaces.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pencolor"></param>
        /// <param name="anchorpt"></param>
        /// <param name="endpt"></param>
        private void DrawBox(Graphics g, Color pencolor, Point anchorpt, Point endpt) {
            DrawLineOnImage(g, pencolor, anchorpt.X, anchorpt.Y, endpt.X, anchorpt.Y);
            DrawLineOnImage(g, pencolor, endpt.X, anchorpt.Y, endpt.X, endpt.Y);
            DrawLineOnImage(g, pencolor, anchorpt.X, anchorpt.Y, anchorpt.X, endpt.Y);
            DrawLineOnImage(g, pencolor, anchorpt.X, endpt.Y, endpt.X, endpt.Y);
        }

        /// <summary>
        /// Draws a trapezoid shape on the draw surfaces.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pencolor"></param>
        /// <param name="anchorpt"></param>
        /// <param name="endpt"></param>
        private void DrawTrapezoid(Graphics g, Color pencolor, Point anchorpt, Point endpt) {
            Point tp = endpt;
            DrawLineOnImage(g, pencolor, anchorpt.X, anchorpt.Y, 159 - anchorpt.X, anchorpt.Y);
            // ensure sloping side is on same side of picture
            if ((anchorpt.X < 80 && endpt.X < 80) || (anchorpt.X >= 80 && endpt.X >= 80)) {
                DrawLineOnImage(g, pencolor, 159 - anchorpt.X, anchorpt.Y, 159 - tp.X, tp.Y);
                DrawLineOnImage(g, pencolor, 159 - tp.X, tp.Y, tp.X, tp.Y);
                DrawLineOnImage(g, pencolor, tp.X, tp.Y, anchorpt.X, anchorpt.Y);
            }
            else {
                DrawLineOnImage(g, pencolor, 159 - anchorpt.X, anchorpt.Y, tp.X, tp.Y);
                DrawLineOnImage(g, pencolor, tp.X, tp.Y, 159 - tp.X, tp.Y);
                DrawLineOnImage(g, pencolor, 159 - tp.X, tp.Y, anchorpt.X, anchorpt.Y);
            }
        }

        /// <summary>
        /// Cancels a drawing action without adding a command or coordinate.
        /// </summary>
        private void StopDrawing() {
            PicDrawMode = PicDrawOp.None;
            SelectCommand(SelectedCmd.Index + 1);
            // force redraw
            DrawPicture();
        }

        /// <summary>
        /// Sets the background visible property to match newval and
        /// loads a background if one is needed.
        /// </summary>
        /// <param name="NewVal"></param>
        /// <param name="ShowConfig"></param>
        private void UpdateBkgd(bool NewVal, bool ShowConfig = false) {
            // if switching to ON AND there is not a picture OR if forcing config
            if ((NewVal && BkgdImage is null) || ShowConfig) {
                // use configure screen, which will load a background
                if (!ConfigureBackground()) {
                    return;
                }
            }
            EditPicture.BkgdVisible = NewVal;
            tsbBackground.Checked = NewVal;
            DrawPicture();
        }

        /// <summary>
        /// Shows background configuration dialog (which will automatically
        /// get a background image if no image is loaded yet)
        /// </summary>
        /// <returns></returns>
        private bool ConfigureBackground() {
            frmConfigureBackground frm = new(this);
            if (frm.DialogResult == DialogResult.Cancel) {
                frm.Dispose();
                return false;
            }
            if (frm.ShowDialog(MDIMain) == DialogResult.Cancel) {
                frm.Dispose();
                return false;
            }
            BkgdImage = frm.BkgdImage;
            EditPicture.BackgroundSettings = frm.bkgdSettings;
            if (InGame) {
                // copy properties back to actual picture resource
                EditGame.Pictures[PictureNumber].BackgroundSettings = frm.bkgdSettings;
                EditGame.Pictures[PictureNumber].SaveProps();
            }
            return true;
        }

        /// <summary>
        /// Clears the selection region and removes it from the draw surfaces.
        /// </summary>
        private void ClearSelectionBounds() {
            SelectedRegion = new();
            tmrSelect.Enabled = false;
            // anchor and block status items not used when 
            // selection region is cleared
            spAnchor.Visible = false;
            spBlock.Visible = false;
        }

        /// <summary>
        /// Updates the draw surfaces and toolbars to show the currently selected
        /// command in the command list. Note that the commands are already
        /// selected in lstCommands.
        /// </summary>
        private void UpdateCmdSelection(int cmdpos, bool force = false) {
            // only if forcing, OR if selection has changed
            if (force || SelectedCmd.Index != cmdpos || SelectedCmdCount != lstCommands.SelectedItems.Count) {
                SelectedCmd = GetCommand(cmdpos);
                SelectedCmdCount = lstCommands.SelectedItems.Count;
                if (SelectedCmdCount > 1) {
                    lstCoords.Items.Clear();
                    lblCoords.Text = "";
                    SetSelectionBounds(true);
                }
                else if (lstCommands.SelectedItems.Count == 1) {
                    ClearSelectionBounds();
                    BuildCoordList();
                }
                else {
                    // not allowed- something must always be selected
                    Debug.Assert(false);
                }
                lstCommands.SelectedItems[^1].Focused = true;
                EnableCoordList();
                if (EditPicture.StepDraw) {
                    // redraw picture
                    DrawPicture();
                }
                else {
                    if (!EditPicture.StepDraw && !EditPicture.BMPSet) {
                        DrawPicture();
                    }
                    else {
                        picVisual.Refresh();
                        picPriority.Refresh();
                    }
                }
            }

            // set selected tools to match current
            if (SelectedCmd.Pen.PlotShape == PlotShape.Circle) {
                if (SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    tsbPlotStyle.Image = tsbCircleSplat.Image;
                }
                else {
                    tsbPlotStyle.Image = tsbCircleSolid.Image;
                }
            }
            else {
                if (SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    tsbPlotStyle.Image = tsbSquareSplat.Image;
                }
                else {
                    tsbPlotStyle.Image = tsbSquareSolid.Image;
                }
            }
            switch (SelectedCmd.Pen.PlotSize) {
            case 0:
                tsbPlotSize.Image = tsbSize0.Image;
                break;
            case 1:
                tsbPlotSize.Image = tsbSize1.Image;
                break;
            case 2:
                tsbPlotSize.Image = tsbSize2.Image;
                break;
            case 3:
                tsbPlotSize.Image = tsbSize3.Image;
                break;
            case 4:
                tsbPlotSize.Image = tsbSize4.Image;
                break;
            case 5:
                tsbPlotSize.Image = tsbSize5.Image;
                break;
            case 6:
                tsbPlotSize.Image = tsbSize6.Image;
                break;
            case 7:
                tsbPlotSize.Image = tsbSize7.Image;
                break;
            }
            picPalette.Refresh();

            // always cancel any drawing operation
            PicDrawMode = PicDrawOp.None;

            // always set cursor highlighting to match selection status
            tmrSelect.Enabled = (SelectedRegion.Width > 0 && SelectedRegion.Height > 0);

            // update toolbar
            UpdateToolbar();
        }

        /// <summary>
        /// Returns true if the first coordinate of the command at the
        /// passed index location matches the last coordinate of the 
        /// command immediately preceding.
        /// </summary>
        /// <param name="ListPos"></param>
        /// <returns></returns>
        public bool MatchPoints(int ListPos) {

            byte bytCmd;
            byte bytX = 0, bytY = 0;
            int xdisp, ydisp;
            byte[] bytData = EditPicture.Data;

            // set starting pos for the previous cmd
            int lngPos = (int)lstCommands.Items[ListPos - 1].Tag;

            // get previous command Type
            bytCmd = bytData[lngPos++];
            switch (bytCmd) {
            case 0xF4 or 0xF5:
                // Draw an X or Y corner.
                // set initial direction
                bool blnRelX = bytCmd == 0xF5;
                bool nextIsX = bytData[(int)lstCommands.Items[ListPos].Tag] == 0xF5;
                bytX = bytData[lngPos++];
                bytY = bytData[lngPos++];
                bytCmd = bytData[lngPos++];
                while (bytCmd < 0xF0) {
                    if (blnRelX) {
                        bytX = bytCmd;
                    }
                    else {
                        bytY = bytCmd;
                    }
                    blnRelX = !blnRelX;
                    bytCmd = bytData[lngPos++];
                }
                break;
            case 0xF6:
                // Absolute line (long lines).
                bytX = bytData[lngPos++];
                bytY = bytData[lngPos++];
                bytCmd = bytData[lngPos++];
                while (bytCmd < 0xF0) {
                    bytX = bytCmd;
                    bytY = bytData[lngPos++];
                    bytCmd = bytData[lngPos++];
                }
                break;
            case 0xF7:
                // Relative line (short lines).
                bytX = bytData[lngPos++];
                bytY = bytData[lngPos++];
                bytCmd = bytData[lngPos++];
                while (bytCmd < 0xF0) {
                    if ((bytCmd & 0x80) == 0x80) {
                        xdisp = -((bytCmd & 0x70) / 0x10);
                    }
                    else {
                        xdisp = (bytCmd & 0x70) / 0x10;
                    }
                    if ((bytCmd & 0x8) == 0x8) {
                        ydisp = -(bytCmd & 0x7);
                    }
                    else {
                        ydisp = bytCmd & 0x7;
                    }
                    bytX = (byte)(bytX + xdisp);
                    bytY = (byte)(bytY + ydisp);
                    bytCmd = bytData[lngPos++];
                }
                break;
            }
            // bytx and byty are now set to last coord of previous cmd
            //  if they match first coordinate value of current command
            //  (current pos +1/+2) then return true
            int pos = (int)lstCommands.Items[ListPos].Tag;
            return bytData[pos + 1] == bytX && bytData[pos + 2] == bytY;
        }

        /// <summary>
        /// Selects one or more commands programmatically. Performs the same selection
        /// actions as clicking on lstCommand items.
        /// </summary>
        /// <param name="cmdpos"></param>
        /// <param name="count"></param>
        /// <param name="force"></param>
        private void SelectCommand(int cmdpos, int count = 1, bool force = false) {
            if (cmdpos < 0 || cmdpos >= lstCommands.Items.Count || count < 1) {
                return;
            }
            // update listboxes
            // disable painting of listbox until all done
            lstCommands.BeginUpdate();
            lstCommands.SelectedItems.Clear();
            do {
                lstCommands.Items[cmdpos - --count].Selected = true;
                lstCommands.FocusedItem = lstCommands.Items[cmdpos];
            } while (count > 0);
            lstCommands.Items[cmdpos].Focused = true;
            lstCommands.Items[cmdpos].EnsureVisible();
            SelectedCmdCount = lstCommands.SelectedItems.Count;
            // restore painting of listbox
            lstCommands.EndUpdate();
            lstCommands.Refresh();
            // update selection
            UpdateCmdSelection(cmdpos, force);
        }

        /// <summary>
        /// Selects the specified coordinate of the passed command.
        /// </summary>
        /// <param name="cmdindex"></param>
        /// <param name="coordindex"></param>
        /// <param name="force"></param>
        private void SelectCoordinate(int cmdindex, int coordindex, bool force = false) {
            SelectedCmd = GetCommand(cmdindex);
            if (SelectedCmdCount != 1 || lstCommands.SelectedItems[0].Index != cmdindex) {
                lstCommands.SelectedItems.Clear();
                lstCommands.Items[cmdindex].Selected = true;
                lstCommands.SelectedItems[0].Focused = true;
                lstCommands.Items[cmdindex].EnsureVisible();
                BuildCoordList();
                EnableCoordList();
            }
            SelectCoordinate(coordindex, force);
        }

        /// <summary>
        /// Selects the specified coordinate of the current command.
        /// </summary>
        /// <param name="coordindex"></param>
        /// <param name="force"></param>
        private void SelectCoordinate(int coordindex, bool force = false) {
            // always cancel any drawing operation
            PicDrawMode = PicDrawOp.None;

            if (force || SelectedCmd.SelectedCoordIndex != coordindex) {
                // set selection to nothing
                ClearSelectionBounds();
                // enable cursor highlighting if edit tool selected
                tmrSelect.Enabled = (SelectedTool == PicToolType.Edit);
                // get coordinate number
                if (coordindex < 0)
                    coordindex = 0;
                if (coordindex > SelectedCmd.Coords.Count - 1)
                    coordindex = SelectedCmd.Coords.Count - 1;
                SelectedCmd.SelectedCoordIndex = coordindex;
                CoordPt = SelectedCmd.SelectedCoord;
                lstCoords.Items[coordindex].Selected = true;
                lstCoords.Items[coordindex].Focused = true;
                lstCoords.SelectedItems[0].EnsureVisible();
                lstCoords.Focus();
                // draw picture
                DrawPicture();
            }
        }

        /// <summary>
        /// Selects the specified tool and configures the editor for the 
        /// asscocated drawing operation.
        /// </summary>
        /// <param name="newtool"></param>
        private void SelectTool(PicToolType newtool) {

            if (SelectedTool == newtool) {
                return;
            }
            PicToolType previoustool = SelectedTool;
            SelectedTool = newtool;
            // show/hide anchor and block status panels
            spAnchor.Visible = SelectedTool == PicToolType.SelectArea;
            spBlock.Visible = SelectedTool == PicToolType.SelectArea;
            if (SelectedTool == PicToolType.SelectArea) {
                spAnchor.Text = "Anchor: ";
                spBlock.Text = "Block: ";
            }

            switch (newtool) {
            case PicToolType.SelectArea:
                tsbTool.Image = tsbImageSelect.Image;
                SetCursors(PicCursor.SelectImage);
                if (previoustool != PicToolType.Edit) {
                    // changing FROM draw tool to Select Area tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                    picPalette.Refresh();
                }
                else {
                    if (SelectedCmdCount > 1) {
                        SelectCommand(SelectedCmd.Index, 1, true);
                    }
                }
                picVisual.Refresh();
                picPriority.Refresh();
                break;
            case PicToolType.Edit:
                tsbTool.Image = tsbEditTool.Image;
                SetCursors(PicCursor.Default);
                if (previoustool != PicToolType.SelectArea) {
                    // changing FROM draw tool to edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                else if (SelectedCmdCount > 1) {
                    SelectCommand(SelectedCmd.Index, 1);
                }
                else {
                    ClearSelectionBounds();
                    picVisual.Refresh();
                    picPriority.Refresh();
                }
                break;
            case PicToolType.Line:
                tsbTool.Image = tsbLine.Image;
                SetCursors(PicCursor.Select);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            case PicToolType.ShortLine:
                tsbTool.Image = tsbShortLine.Image;
                SetCursors(PicCursor.Select);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            case PicToolType.StepLine:
                tsbTool.Image = tsbStepLine.Image;
                SetCursors(PicCursor.Select);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            case PicToolType.Rectangle:
                tsbTool.Image = tsbRectangle.Image;
                SetCursors(PicCursor.Select);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            case PicToolType.Trapezoid:
                tsbTool.Image = tsbTrapezoid.Image;
                SetCursors(PicCursor.Select);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            case PicToolType.Ellipse:
                tsbTool.Image = tsbEllipse.Image;
                SetCursors(PicCursor.Select);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            case PicToolType.Fill:
                tsbTool.Image = tsbFill.Image;
                SetCursors(PicCursor.Paint);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            case PicToolType.Plot:
                tsbTool.Image = tsbPlot.Image;
                SetCursors(PicCursor.Brush);
                if (previoustool == PicToolType.Edit || previoustool == PicToolType.SelectArea) {
                    // changing TO a draw tool from edit tool
                    SelectCommand(SelectedCmd.Index - SelectedCmdCount + 1, 1, true);
                }
                break;
            }
            spTool.Text = "Tool: " + SelectedTool.ToString();
            EnableCoordList();
            UpdateToolbar();
            lstCommands.Focus();
        }

        /// <summary>
        /// Finishes a draw command using the passed cursor position,
        /// updating the picture resource and redrawing the draw surfaces
        /// if needed.
        /// </summary>
        /// <param name="PicPt"></param>
        private void EndDraw(Point PicPt) {
            byte[] bytData = [];

            // if cursor hasn't moved, just exit
            if (PicPt == AnchorPT) {
                return;
            }

            switch (PicDrawMode) {
            case PicDrawOp.Line:
                // depending on line Type, need to complete the current line segment
                // (note that we don't end the draw mode; this let's us continue adding more
                // line segments to this command)
                switch (SelectedTool) {
                case PicToolType.Line:
                    // set data to add this point
                    bytData = [(byte)PicPt.X, (byte)PicPt.Y];
                    break;
                case PicToolType.ShortLine:
                    // validate x and y
                    // (note that delta x is limited to -6 to avoid
                    // values above 0xF0, which would mistakenly be interpreted
                    // as a new command)
                    if (PicPt.X < AnchorPT.X - 6) {
                        PicPt.X = AnchorPT.X - 6;
                    }
                    else if (PicPt.X > AnchorPT.X + 7) {
                        PicPt.X = AnchorPT.X + 7;
                    }
                    if (PicPt.Y < AnchorPT.Y - 7) {
                        PicPt.Y = AnchorPT.Y - 7;
                    }
                    else if (PicPt.Y > AnchorPT.Y + 7) {
                        PicPt.Y = AnchorPT.Y + 7;
                    }
                    // calculate delta to this point
                    bytData = [0];
                    if (PicPt.X - AnchorPT.X < 0) {
                        bytData[0] = 0x80;
                    }
                    else {
                        bytData[0] = 0;
                    }
                    bytData[0] += (byte)(Math.Abs(PicPt.X - AnchorPT.X) * 0x10);
                    if (PicPt.Y - AnchorPT.Y < 0) {
                        bytData[0] += 0x08;
                    }
                    bytData[0] += (byte)(Math.Abs(PicPt.Y - AnchorPT.Y));
                    break;
                case PicToolType.StepLine:
                    // get insert pos
                    int lngInsertPos = SelectedCmd.Position;
                    // if drawing second point
                    if (SelectedCmd.Coords.Count == 1) {
                        // if mostly vertical
                        if (Math.Abs(PicPt.X - AnchorPT.X) < Math.Abs(PicPt.Y - AnchorPT.Y)) {
                            // command should be Y corner
                            if (SelectedCmd.Type != YCorner) {
                                lstCommands.Items[SelectedCmd.Index].Text = YCorner.CommandName();
                                EditPicture.Data[lngInsertPos] = (byte)YCorner;
                            }
                            // limit change to vertical direction only
                            PicPt.X = AnchorPT.X;
                            bytData = [(byte)PicPt.Y];
                        }
                        else {
                            // command should be X corner
                            if (SelectedCmd.Type != XCorner) {
                                lstCommands.Items[SelectedCmd.Index].Text = XCorner.CommandName();
                                EditPicture.Data[lngInsertPos] = (byte)XCorner;
                            }
                            // limit change to horizontal direction only
                            PicPt.Y = AnchorPT.Y;
                            bytData = [(byte)PicPt.X];
                        }
                    }
                    else {
                        // determine which direction to allow movement
                        if ((SelectedCmd.Type == XCorner && SelectedCmd.Coords.Count.IsEven()) ||
                           (SelectedCmd.Type == YCorner && SelectedCmd.Coords.Count.IsOdd())) {
                            // limit change to vertical direction
                            PicPt.X = AnchorPT.X;
                            bytData = [(byte)PicPt.Y];
                        }
                        else {
                            // limit change to horizontal direction
                            PicPt.Y = AnchorPT.Y;
                            bytData = [(byte)PicPt.X];
                        }
                    }
                    break;
                }
                // if adjusted cursor hasn't moved, just exit
                if (AnchorPT == PicPt) {
                    return;
                }
                // set anchor to new point
                AnchorPT = PicPt;
                // insert coordinate
                AddCoordToPic(bytData, PicPt, SelectedCmd.Coords.Count);
                DrawPicture();
                break;
            case PicDrawOp.Shape:
                // depending on shape Type, add appropriate commands to add the selected element
                // (note that when shapes are completed, we go back to 'none' as the draw mode
                // each shape is drawn as a separate action)
                switch (SelectedTool) {
                case PicToolType.Rectangle:
                    // finish drawing box
                    bytData = [(byte)XCorner, (byte)AnchorPT.X, (byte)AnchorPT.Y,
                               (byte)PicPt.X, (byte)PicPt.Y, (byte)AnchorPT.X, (byte)AnchorPT.Y];
                    // add command
                    InsertCommand(bytData, SelectedCmd.Index);
                    // select the next command
                    SelectCommand(SelectedCmd.Index + 1);

                    // adjust last undo text
                    UndoCol.Peek().Action = PictureUndo.ActionType.Rectangle;
                    break;
                case PicToolType.Trapezoid:
                    // finish drawing trapezoid
                    bytData = new byte[11];
                    bytData[0] = (byte)AbsLine;
                    bytData[1] = (byte)AnchorPT.X;
                    bytData[2] = (byte)AnchorPT.Y;
                    bytData[3] = (byte)(159 - AnchorPT.X);
                    bytData[4] = (byte)AnchorPT.Y;
                    // ensure sloping side is on same side of picture
                    if ((AnchorPT.X < 80 && PicPt.X < 80) || (AnchorPT.X >= 80 && PicPt.X >= 80)) {
                        bytData[5] = (byte)(159 - PicPt.X);
                        bytData[6] = (byte)PicPt.Y;
                        bytData[7] = (byte)PicPt.X;
                        bytData[8] = (byte)PicPt.Y;
                    }
                    else {
                        bytData[5] = (byte)PicPt.X;
                        bytData[6] = (byte)PicPt.Y;
                        bytData[7] = (byte)(159 - PicPt.X);
                        bytData[8] = (byte)PicPt.Y;
                    }
                    bytData[9] = (byte)AnchorPT.X;
                    bytData[10] = (byte)AnchorPT.Y;
                    // add command
                    InsertCommand(bytData, SelectedCmd.Index);
                    // select next command
                    SelectCommand(SelectedCmd.Index + 1);
                    // adjust last undo text
                    UndoCol.Peek().Action = Trapezoid;
                    break;
                case PicToolType.Ellipse:
                    // finish drawing ellipse

                    // if both height and width are one pixel
                    if (AnchorPT == PicPt) {
                        // draw just a single pixel
                        bytData = [(byte)AbsLine, (byte)AnchorPT.X, (byte)AnchorPT.Y];
                        // insert the command
                        InsertCommand(bytData, SelectedCmd.Index);
                        // select next command
                        SelectCommand(SelectedCmd.Index + 1);
                    }
                    else if (AnchorPT.Y - PicPt.Y == 0) {
                        //  height is one pixel, just draw a horizontal line
                        bytData = [(byte)XCorner, (byte)AnchorPT.X, (byte)AnchorPT.Y, (byte)PicPt.X];
                        // add command
                        InsertCommand(bytData, SelectedCmd.Index);
                        // select next command
                        SelectCommand(SelectedCmd.Index + 1);
                    }
                    else if (AnchorPT.X - PicPt.X == 0) {
                        // if width is one pixel, just draw a vertical line
                        bytData = [(byte)YCorner, (byte)AnchorPT.X, (byte)AnchorPT.Y, (byte)PicPt.Y];
                        // add command
                        InsertCommand(bytData, SelectedCmd.Index);
                        // select next command
                        SelectCommand(SelectedCmd.Index + 1);
                    }
                    else {
                        // ensure we are in a upperleft-lower right configuration
                        if (AnchorPT.X > PicPt.X) {
                            (PicPt.X, AnchorPT.X) = (AnchorPT.X, PicPt.X);
                        }
                        if (AnchorPT.Y > PicPt.Y) {
                            (PicPt.Y, AnchorPT.Y) = (AnchorPT.Y, PicPt.Y);
                        }
                        bytData = new byte[ArcPts.Length * 2 + 1];
                        bytData[0] = (byte)AbsLine;
                        // now draw the arc segments:
                        // add first arc (skip undo)
                        for (int i = 0; i < ArcPts.Length; i++) {
                            bytData[i * 2 + 1] = (byte)(AnchorPT.X + ArcPts[0].X - ArcPts[i].X);
                            bytData[i * 2 + 2] = (byte)(AnchorPT.Y + ArcPts[^1].Y - ArcPts[i].Y);
                        }
                        InsertCommand(bytData, SelectedCmd.Index, true);
                        // add second arc (skip undo)
                        for (int i = 0; i < ArcPts.Length; i++) {
                            bytData[2 * i + 1] = (byte)(PicPt.X - ArcPts[0].X + ArcPts[i].X);
                            bytData[2 * i + 2] = (byte)(AnchorPT.Y + ArcPts[^1].Y - ArcPts[i].Y);
                        }
                        InsertCommand(bytData, SelectedCmd.Index, true);
                        // add third arc (skip undo)
                        for (int i = 0; i < ArcPts.Length; i++) {
                            bytData[2 * i + 1] = (byte)(PicPt.X - ArcPts[0].X + ArcPts[i].X);
                            bytData[2 * i + 2] = (byte)(PicPt.Y - ArcPts[^1].Y + ArcPts[i].Y);
                        }
                        InsertCommand(bytData, SelectedCmd.Index, true);
                        // add fourth arc (ADD undo)
                        for (int i = 0; i < ArcPts.Length; i++) {
                            bytData[2 * i + 1] = (byte)(AnchorPT.X + ArcPts[0].X - ArcPts[i].X);
                            bytData[2 * i + 2] = (byte)(PicPt.Y - ArcPts[^1].Y + ArcPts[i].Y);
                        }
                        InsertCommand(bytData, SelectedCmd.Index);
                        // select the following command
                        SelectCommand(SelectedCmd.Index + 4);
                        // adjust last undo text
                        UndoCol.Peek().Action = Ellipse;
                    }
                    break;
                }
                // end draw mode
                StopDrawing();
                break;
            }
        }

        /// <summary>
        /// Finishes an edit coordinate operation, updating the picture
        /// resources and draw surfaces as necessary.
        /// </summary>
        /// <param name="CmdType"></param>
        /// <param name="CoordIndex"></param>
        /// <param name="newPt"></param>
        private void EndEditCoord(DrawFunction CmdType, int CoordIndex, Point newPt) {
            int lngPos = SelectedCmd.CoordPos(SelectedCmd.SelectedCoordIndex);
            byte[] bytData = [];
            int picpos = lngPos;

            // validate for Type of node being edited
            switch (CmdType) {
            case AbsLine:
            case Fill:
                bytData = new byte[2];
                bytData[0] = EditPicture.Data[lngPos];
                bytData[1] = EditPicture.Data[lngPos + 1];
                // update resource data
                EditPicture.Data[lngPos] = (byte)newPt.X;
                EditPicture.Data[lngPos + 1] = (byte)newPt.Y;
                break;
            case PlotPen:
                // if this node includes a pattern command,
                if (SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    // adjust resource pos by 1
                    lngPos++;
                }
                bytData = new byte[2];
                bytData[0] = EditPicture.Data[lngPos];
                bytData[1] = EditPicture.Data[lngPos];
                // update resource data
                EditPicture.Data[lngPos] = (byte)newPt.X;
                EditPicture.Data[lngPos + 1] = (byte)newPt.Y;
                break;
            case RelLine:
                if (SelectedCmd.Coords.Count == 1) {
                    bytData = new byte[2];
                    bytData[0] = EditPicture.Data[lngPos];
                    bytData[1] = EditPicture.Data[lngPos + 1];
                    // no verification nedded
                    EditPicture.Data[lngPos] = (byte)newPt.X;
                    EditPicture.Data[lngPos + 1] = (byte)newPt.Y;
                    return;
                }
                Point tmpPrevPT = new(0, 0), tmpNextPT = new(0, 0);
                // if not first point
                if (CoordIndex > 0) {
                    // validate against previous point
                    tmpPrevPT = SelectedCmd.Coords[CoordIndex - 1];
                    // validate x and Y against previous pt
                    // (note that delta x is limited to -6 to avoid
                    // values above 0xF0, which would mistakenly be interpreted
                    // as a new command)
                    if (newPt.X > tmpPrevPT.X + 7) {
                        newPt.X = tmpPrevPT.X + 7;
                    }
                    else if (newPt.X < tmpPrevPT.X - 6) {
                        newPt.X = tmpPrevPT.X - 6;
                    }
                    if (newPt.Y > tmpPrevPT.Y + 7) {
                        newPt.Y = tmpPrevPT.Y + 7;
                    }
                    else if (newPt.Y < tmpPrevPT.Y - 7) {
                        newPt.Y = tmpPrevPT.Y - 7;
                    }
                }
                // if not last point (next pt is not a new cmd)
                if (CoordIndex < SelectedCmd.Coords.Count - 1) {
                    // validate against next point
                    // note that delta x is limited to +6 (swapped because we are
                    // comparing against NEXT vs. PREVIOUS coordinate)
                    // for same reason as given above
                    tmpNextPT = SelectedCmd.Coords[CoordIndex + 1];
                    if (newPt.X > tmpNextPT.X + 6) {
                        newPt.X = tmpNextPT.X + 6;
                    }
                    else if (newPt.X < tmpNextPT.X - 7) {
                        newPt.X = tmpNextPT.X - 7;
                    }
                    if (newPt.Y > tmpNextPT.Y + 7) {
                        newPt.Y = tmpNextPT.Y + 7;
                    }
                    else if (newPt.Y < tmpNextPT.Y - 7) {
                        newPt.Y = tmpNextPT.Y - 7;
                    }
                }
                // if first coordinate
                if (CoordIndex == 0) {
                    bytData = new byte[3];
                    bytData[0] = EditPicture.Data[lngPos];
                    bytData[1] = EditPicture.Data[lngPos + 1];
                    bytData[2] = EditPicture.Data[lngPos + 2];
                    EditPicture.Data[lngPos] = (byte)newPt.X;
                    EditPicture.Data[lngPos + 1] = (byte)newPt.Y;
                    // recalculate delta to second point
                    EditPicture.Data[lngPos + 2] = (byte)(Math.Abs(tmpNextPT.X - newPt.X) * 16 + (Math.Sign(tmpNextPT.X - newPt.X) == -1 ? 128 : 0) + Math.Abs(tmpNextPT.Y - newPt.Y) + (Math.Sign(tmpNextPT.Y - newPt.Y) == -1 ? 8 : 0));
                }
                else {
                    bytData = new byte[1];
                    bytData[0] = EditPicture.Data[lngPos];
                    // calculate new relative change in x and Y between previous pt and this point
                    EditPicture.Data[lngPos] = (byte)(Math.Abs(newPt.X - tmpPrevPT.X) * 16 + (Math.Sign(newPt.X - tmpPrevPT.X) == -1 ? 128 : 0) + Math.Abs(newPt.Y - tmpPrevPT.Y) + (Math.Sign(newPt.Y - tmpPrevPT.Y) == -1 ? 8 : 0));
                    // if not last point
                    if (CoordIndex < SelectedCmd.Coords.Count - 1) {
                        Array.Resize(ref bytData, 2);
                        bytData[1] = EditPicture.Data[lngPos + 1];
                        // calculate new relative change in x and Y between next pt and this point
                        EditPicture.Data[lngPos + 1] = (byte)(Math.Abs(tmpNextPT.X - newPt.X) * 16 + (Math.Sign(tmpNextPT.X - newPt.X) == -1 ? 128 : 0) + Math.Abs(tmpNextPT.Y - newPt.Y) + (Math.Sign(tmpNextPT.Y - newPt.Y) == -1 ? 8 : 0));
                    }
                }
                break;
            case XCorner:
                // if editing first point,
                if (CoordIndex == 0) {
                    bytData = new byte[2];
                    bytData[0] = EditPicture.Data[lngPos];
                    bytData[1] = EditPicture.Data[lngPos + 1];
                    // update resource data
                    EditPicture.Data[lngPos] = (byte)newPt.X;
                    EditPicture.Data[lngPos + 1] = (byte)newPt.Y;
                }
                else {
                    bytData = new byte[2];
                    bytData[0] = EditPicture.Data[lngPos - 1];
                    bytData[1] = EditPicture.Data[lngPos];
                    picpos--;
                    // if odd
                    if (CoordIndex.IsOdd()) {
                        // x Value is at lngPos; Y Value is at lngPos-1
                        EditPicture.Data[lngPos] = (byte)newPt.X;
                        EditPicture.Data[lngPos - 1] = (byte)newPt.Y;
                    }
                    else {
                        // x Value is at lngPos-1, Y Value is at lngPos
                        EditPicture.Data[lngPos - 1] = (byte)newPt.X;
                        EditPicture.Data[lngPos] = (byte)newPt.Y;
                    }
                }
                break;
            case YCorner:
                // if editing first point,
                if (CoordIndex == 0) {
                    bytData = new byte[2];
                    bytData[0] = EditPicture.Data[lngPos];
                    bytData[1] = EditPicture.Data[lngPos + 1];
                    // update resource data
                    EditPicture.Data[lngPos] = (byte)newPt.X;
                    EditPicture.Data[lngPos + 1] = (byte)newPt.Y;
                }
                else {
                    // if even
                    if (CoordIndex.IsEven()) {
                        bytData = new byte[2];
                        bytData[0] = EditPicture.Data[lngPos - 1];
                        bytData[1] = EditPicture.Data[lngPos];
                        picpos--;
                        // x Value is lngpos, Y Value is at lngpos-1
                        EditPicture.Data[lngPos] = (byte)newPt.X;
                        EditPicture.Data[lngPos - 1] = (byte)newPt.Y;
                    }
                    else {
                        // special check for Y lines; for the second coord, the x Value is actaully
                        // two bytes in front of the edited coord (since cmd gives first coord
                        // as two bytes, then shifts to single byte per coord; Y Value is at lngpos
                        if (CoordIndex == 1) {
                            bytData = new byte[3];
                            bytData[0] = EditPicture.Data[lngPos - 2];
                            bytData[1] = EditPicture.Data[lngPos - 1];
                            bytData[2] = EditPicture.Data[lngPos];
                            picpos -= 2;
                            // x Value is at lngPos-2
                            EditPicture.Data[lngPos - 2] = (byte)newPt.X;
                        }
                        else {
                            bytData = new byte[2];
                            bytData[0] = EditPicture.Data[lngPos - 1];
                            bytData[1] = EditPicture.Data[lngPos];
                            picpos--;
                            // x Value is at lngPos-1
                            EditPicture.Data[lngPos - 1] = (byte)newPt.X;
                        }
                        EditPicture.Data[lngPos] = (byte)newPt.Y;
                    }
                }
                break;
            }
            PictureUndo NextUndo = new();
            if (Inserting) {
                // replace previous 'add coord' undo
                // with an 'insert coord' undo
                UndoCol.Pop();
                NextUndo.Action = InsertCoord;
            }
            else {
                // create move undo object
                NextUndo.Action = MoveCoord;
            }
            NextUndo.CoordIndex = CoordIndex;
            NextUndo.CmdIndex = SelectedCmd.Index;
            NextUndo.PicPos = picpos;
            NextUndo.Data = bytData;
            AddUndo(NextUndo);
            // need to force refresh so picture will update
            EditPicture.ForceRefresh();
            // reset edit mode
            PicDrawMode = PicDrawOp.None;

            // begin highlighting selected coord again
            tmrSelect.Enabled = true;
        }

        /// <summary>
        /// Moves the specified commands to new position based on delta values,
        /// updating the picture resource and draw surfaces as needed.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="dX"></param>
        /// <param name="dY"></param>
        /// <param name="DontUndo"></param>
        private void MoveCommands(int index, int count, int dX, int dY, bool DontUndo = false) {
            if (dX == 0 && dY == 0) {
                return;
            }

            // if more than one command selected, MoveCmd is the LAST command in the group of selected commands!
            int first = index - count + 1;

            // we need to know the pen style in case a plot command is being moved
            // and make sure we get FIRST command, not the last one
            PlotStyle plotStyle = EditPicture.GetPenStatus((int)lstCommands.Items[first].Tag).PlotStyle;
            for (int i = 0; i < count; i++) {
                int lngPos = (int)lstCommands.Items[first + i].Tag;
                switch (EditPicture.Data[lngPos]) {
                case (byte)RelLine:
                    // only first pt needs to be changed (if there is a first point)
                    if (EditPicture.Data[lngPos + 1] < 0xF0 && EditPicture.Data[lngPos + 2] < 0xF0) {
                        EditPicture.Data[lngPos + 1] += (byte)dX;
                        EditPicture.Data[lngPos + 2] += (byte)dY;
                    }
                    break;
                case (byte)AbsLine or (byte)Fill:
                    // each pair of coordinates are adjusted for offset
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        if (EditPicture.Data[lngPos + 1] < 0xF0) {
                            EditPicture.Data[lngPos] += (byte)dX;
                            EditPicture.Data[lngPos + 1] += (byte)dY;
                        }
                        else {
                            // end found
                            break;
                        }
                        // get next cmd pair
                        lngPos += 2;
                    }
                    break;
                case (byte)ChangePen:
                    // need to make sure we keep up with any plot style changes
                    lngPos++;
                    if ((EditPicture.Data[lngPos] & 0x20) == 0) {
                        // solid
                        plotStyle = PlotStyle.Solid;
                    }
                    else {
                        plotStyle = PlotStyle.Splatter;
                    }
                    break;
                case (byte)PlotPen:
                    // each group of coordinates are adjusted for offset
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // if splattering, skip the splatter code
                        if (plotStyle == PlotStyle.Splatter) {
                            lngPos++;
                        }
                        if (EditPicture.Data[lngPos] < 0xF0 && EditPicture.Data[lngPos + 1] < 0xF0) {
                            EditPicture.Data[lngPos] += (byte)dX;
                            EditPicture.Data[lngPos + 1] += (byte)dY;
                        }
                        else {
                            // end found 
                            break;
                        }
                        // get next cmd pair
                        lngPos += 2;
                    }
                    break;
                case (byte)XCorner or (byte)YCorner:
                    // if this is a 'x' corner, then next coord is a 'x' Value
                    // (make sure to check this BEFORE incrementing lngPos)
                    bool blnX = EditPicture.Data[lngPos] == (byte)XCorner;
                    // move pointer to first coordinate pair
                    lngPos++;
                    if (EditPicture.Data[lngPos] < 0xF0 && EditPicture.Data[lngPos + 1] < 0xF0) {
                        // move first coordinate
                        EditPicture.Data[lngPos] += (byte)dX;
                        EditPicture.Data[lngPos + 1] += (byte)dY;
                        // move pointer to next coordinate point
                        lngPos += 2;
                    }
                    else {
                        // no coordinates 
                        break;
                    }
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // if this is a 'x' point
                        if (blnX) {
                            // add delta x
                            EditPicture.Data[lngPos] += (byte)dX;
                        }
                        else {
                            // add delta y
                            EditPicture.Data[lngPos] += (byte)dY;
                        }
                        // toggle next coord Type
                        blnX = !blnX;
                        // increment pointer
                        lngPos++;
                    }
                    break;
                }
            }

            // add undo (if necessary)
            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = MoveCmds;
                NextUndo.CmdIndex = index;
                NextUndo.CoordIndex = count;
                NextUndo.Data = [(byte)dX, (byte)dY];
                AddUndo(NextUndo);
            }
            // force picture update
            EditPicture.ForceRefresh();
        }

        /// <summary>
        /// Flips the selected commands horizontally, updating the picture resource
        /// and draw surfaces. Note that FlipCmd is the index of the last command 
        /// (largest index value) in the selection. 
        /// </summary>
        /// <param name="FlipCmd"></param>
        /// <param name="Count"></param>
        /// <param name="DontUndo"></param>
        private void FlipHorizontal(int FlipCmd, int Count, bool DontUndo = false) {

            // update selected region bounds
            SetSelectionBounds(true);

            // get current pen status for first command
            PenStatus tmpStyle = EditPicture.GetPenStatus((int)lstCommands.Items[FlipCmd - Count + 1].Tag);

            // to calculate the new X values, do some geometry - we get:
            // pX' = 2*rX + rW - pX - 1
            //    pX = coord pt
            //    pX' = reflected point
            //    rX = region left point
            //    rW = region width

            // step through each cmd
            for (int i = 0; i < Count; i++) {
                int lngPos = (int)lstCommands.Items[FlipCmd - Count + 1 + i].Tag;

                // each cmd handles flip differently
                switch (EditPicture.Data[lngPos]) {
                case (byte)RelLine:
                    // increment position marker
                    int lngStartPos = ++lngPos;

                    // when flipping relative lines horizontally, need to flip the actual order
                    // of coordinates; this ensures that we avoid situations where the flipped
                    // line creates data bytes that the interpreter might confuse as commands
                    // (remember that the delta x offset is the four highest bits of the data
                    // byte; bit 7 is set if the delta is negative; bits 6-5-4 determine Value;
                    // if the delta amount is -7, this means the data byte will be >=0xF0; this
                    // is read by the interpreter as a new cmd; not as a delt of -7; so for
                    // rel lines, the delta x Value is limited to -6; when flipping, we can't just
                    // flip the first coord, then change the direction of the x-delta values;
                    // there may be some +7 delta values that will result in errors when converted
                    // to -7 delta x values

                    // solution is to build the command backwards; start with the LAST point in
                    // the command; then build the line backwards to finish the swap

                    // if at least one valid coordinate
                    if (EditPicture.Data[lngPos] < 0xF0) {
                        // build an list of coordinates
                        List<Point> coords = new();
                        byte bytX = EditPicture.Data[lngPos++];
                        byte bytY = EditPicture.Data[lngPos++];
                        coords.Add(new Point(bytX, bytY));
                        while (EditPicture.Data[lngPos] < 0xF0) {
                            // add deltax
                            if ((EditPicture.Data[lngPos] & 0x80) > 0) {
                                bytX -= (byte)((EditPicture.Data[lngPos] & 0x70) / 0x10);
                            }
                            else {
                                bytX += (byte)((EditPicture.Data[lngPos] & 0x70) / 0x10);
                            }
                            // add deltay
                            if ((EditPicture.Data[lngPos] & 0x8) > 0) {
                                bytY -= (byte)(EditPicture.Data[lngPos] & 0x7);
                            }
                            else {
                                bytY += (byte)(EditPicture.Data[lngPos] & 0x7);
                            }
                            coords.Add(new Point(bytX, bytY));
                            // get next delta Value
                            lngPos++;
                        }
                        // flip the x Value of all points
                        for (int j = 0; j < coords.Count; j++) {
                            // flip the x coordinate
                            coords[j] = new Point((2 * SelectedRegion.X) + SelectedRegion.Width - coords[j].X - 1, coords[j].Y);
                        }
                        // save ending point position (remember to back up one unit)
                        int lngOldPos = lngPos - 1;
                        // move pointer to first coordinate pair
                        lngPos = lngStartPos;
                        // now rebuild the command, backwards
                        EditPicture.Data[lngPos++] = (byte)coords[^1].X;
                        EditPicture.Data[lngPos++] = (byte)coords[^1].Y;
                        for (int j = coords.Count - 2; j >= 0; j--) {
                            if (coords[j].X < coords[j + 1].X) {
                                EditPicture.Data[lngPos] = 0x80;
                            }
                            else {
                                EditPicture.Data[lngPos] = 0x00;
                            }
                            EditPicture.Data[lngPos] += (byte)(Math.Abs(coords[j].X - coords[j + 1].X) * 16);
                            if (coords[j].Y < coords[j + 1].Y) {
                                EditPicture.Data[lngPos] += 0x08;
                            }
                            EditPicture.Data[lngPos] += (byte)(Math.Abs(coords[j].Y - coords[j + 1].Y));
                            lngPos++;
                        }
                    }
                    break;
                case (byte)AbsLine:
                case (byte)Fill:
                    // each pair of coordinates are adjusted for flip
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // first byte is X, flip it
                        EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.X + SelectedRegion.Width - EditPicture.Data[lngPos] - 1);
                        // second byte is Y, skip it
                        if (EditPicture.Data[++lngPos] >= 0xF0) {
                            // end found
                            break;
                        }
                        // get next X
                        lngPos++;
                    }
                    break;
                case (byte)PlotPen:
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // if pen is splatter
                        if (tmpStyle.PlotStyle == PlotStyle.Splatter) {
                            // skip first byte; its the splatter Value
                            lngPos++;
                        }
                        // first byte is X, flip it
                        EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.X + SelectedRegion.Width - EditPicture.Data[lngPos] - 1);
                        // second byte is Y, skip it
                        if (EditPicture.Data[++lngPos] >= 0xF0) {
                            // end found
                            break;
                        }
                        // get next X
                        lngPos++;
                    }
                    break;
                case (byte)XCorner:
                case (byte)YCorner:
                    bool blnX = EditPicture.Data[lngPos++] == (byte)XCorner;
                    if (EditPicture.Data[lngPos] >= 0xF0) {
                        break;
                    }
                    // first byte is X, flip it
                    EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.X + SelectedRegion.Width - EditPicture.Data[lngPos] - 1);
                    if (EditPicture.Data[++lngPos] >= 0xF0) {
                        // end found
                        break;
                    }
                    // second byte is Y, skip it
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // if this is a 'X' point, flip it
                        if (blnX) {
                            EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.X + SelectedRegion.Width - EditPicture.Data[lngPos] - 1);
                        }
                        // toggle next coord Type
                        blnX = !blnX;
                        // increment pointer
                        lngPos++;
                    }
                    break;
                case (byte)ChangePen:
                    tmpStyle.PlotStyle = (PlotStyle)((EditPicture.Data[lngPos + 1] & 0x20) / 0x20);
                    break;
                }
            }
            // edits are made directly to data array so manually force pics to redraw
            EditPicture.ForceRefresh();
            // if not skipping undo
            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = PictureUndo.ActionType.FlipH;
                NextUndo.CmdIndex = FlipCmd;
                NextUndo.CmdCount = Count;
                AddUndo(NextUndo);
            }
        }

        /// <summary>
        /// Flips the selected commands vertically, updating the picture resource
        /// and draw surfaces. Note that FlipCmd is the index of the last command 
        /// (largest index value) in the selection. 
        /// </summary>
        /// <param name="FlipCmd"></param>
        /// <param name="Count"></param>
        /// <param name="DontUndo"></param>
        private void FlipVertical(int FlipCmd, int Count, bool DontUndo = false) {

            // update selected region bounds
            SetSelectionBounds(true);

            // get current pen status for first command
            PenStatus tmpStyle = EditPicture.GetPenStatus((int)lstCommands.Items[FlipCmd - Count + 1].Tag);

            // to calculate the new X values, do some geometry - we get:
            // pY' = 2*rY + rH - pY - 1
            //    pY = coord pt
            //    pY' = reflected point
            //    rY = region top point
            //    rH = region height

            // step through each cmd
            for (int i = 0; i < Count; i++) {
                int lngPos = (int)lstCommands.Items[FlipCmd - Count + 1 + i].Tag;

                // each cmd handles flip differently
                switch (EditPicture.Data[lngPos]) {
                case (byte)RelLine:
                    // when flipping the y axis, we don't need to worry about
                    // the swap causing errors in the delta values; all we need
                    // to do is just swap the first coordinate, and then change the
                    // y direction of all delta values
                    // skip initial x
                    if (EditPicture.Data[++lngPos] >= 0xF0) {
                        break;
                    }
                    // flip initial y
                    if (EditPicture.Data[++lngPos] >= 0xF0) {
                        break;
                    }
                    EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.Y + SelectedRegion.Height - EditPicture.Data[lngPos] - 1);
                    // increment position marker
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // toggle direction bit for y displacement

                        // if the delta y Value is currently negative
                        if ((EditPicture.Data[lngPos] & 0x8) == 0x8) {
                            // clear the bit to make the direction positive
                            EditPicture.Data[lngPos] = (byte)(EditPicture.Data[lngPos] & 0xF7);
                        }
                        else {
                            // if the delta Value is not zero
                            if ((EditPicture.Data[lngPos] & 0x7) > 0) {
                                // set the bit
                                EditPicture.Data[lngPos] = (byte)(EditPicture.Data[lngPos] | 0x8);
                            }
                        }

                        // neyt byte
                        lngPos++;
                    }
                    break;
                case (byte)AbsLine:
                case (byte)Fill:
                    // each pair of coordinates are adjusted for flip
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // first byte is X, skip it
                        if (EditPicture.Data[++lngPos] >= 0xF0) {
                            // end found
                            break;
                        }
                        // second byte is Y, flip it
                        EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.Y + SelectedRegion.Height - EditPicture.Data[lngPos] - 1);
                        // get next x
                        lngPos++;
                    }
                    break;
                case (byte)PlotPen:
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // if pen is splatter
                        if (tmpStyle.PlotStyle == PlotStyle.Splatter) {
                            // skip first byte; it's the splatter Value
                            if (EditPicture.Data[++lngPos] >= 0xF0) {
                                // end found
                                break;
                            }
                        }
                        // first byte is X, skip it
                        if (EditPicture.Data[++lngPos] >= 0xF0) {
                            // end found
                            break;
                        }
                        // second byte is Y, flip it
                        EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.Y + SelectedRegion.Height - EditPicture.Data[lngPos] - 1);
                        // get next X
                        lngPos++;
                    }
                    break;
                case (byte)YCorner:
                case (byte)XCorner:
                    bool blnX = EditPicture.Data[lngPos++] == (byte)XCorner;
                    if (EditPicture.Data[lngPos] >= 0xF0) {
                        break;
                    }
                    // first byte is X, skip it
                    if (EditPicture.Data[++lngPos] >= 0xF0) {
                        break;
                    }
                    // second byte is Y, flip it
                    EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.Y + SelectedRegion.Height - EditPicture.Data[lngPos] - 1);
                    lngPos++;
                    while (EditPicture.Data[lngPos] < 0xF0) {
                        // if this is a 'Y', flip it
                        if (!blnX) {
                            // skip it
                            EditPicture.Data[lngPos] = (byte)(2 * SelectedRegion.Y + SelectedRegion.Height - EditPicture.Data[lngPos] - 1);
                        }
                        // toggle next coord Type
                        blnX = !blnX;
                        // increment pointer
                        lngPos++;
                    }
                    break;
                case (byte)ChangePen:
                    tmpStyle.PlotStyle = (PlotStyle)((EditPicture.Data[lngPos + 1] & 0x20) / 0x20);
                    break;
                }
            }
            // edits are made directly to data array so manually force pics to redraw
            EditPicture.ForceRefresh();
            // if not skipping undo
            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = PictureUndo.ActionType.FlipV;
                NextUndo.CmdIndex = FlipCmd;
                NextUndo.CmdCount = Count;
                AddUndo(NextUndo);
            }
        }

        /// <summary>
        /// Joins the currently selected command with the previous command into
        /// a single command. This method assumes the two commands have already
        /// been checked to make sure they can be joined.
        /// </summary>
        /// <param name="DontUndo"></param>
        private void JoinCommands(bool DontUndo = false) {
            int index1 = SelectedCmd.Index - 1;
            int index2 = SelectedCmd.Index;
            int pos1 = (int)lstCommands.Items[index1].Tag;
            int pos2 = SelectedCmd.Position;
            DrawFunction cmd1 = (DrawFunction)EditPicture.Data[pos1];
            DrawFunction cmd2 = SelectedCmd.Type;
            int lngCount = 0;

            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = JoinCmds;
                // if cmd requires one byte per coord pair
                // need to move picpos marker back one
                switch (cmd2) {
                case XCorner:
                case YCorner:
                case RelLine:
                    // coordindex is count of previous cmd coords minus 1
                    NextUndo.CoordIndex = pos2 - pos1 - 3;
                    if (NextUndo.CoordIndex < 0) {
                        NextUndo.CoordIndex = 0;
                    }
                    break;
                default:
                    switch (cmd2) {
                    case AbsLine:
                        // coordindex is count of previous cmd coords
                        NextUndo.CoordIndex = (pos2 - pos1 - 1) / 2 - 1;
                        break;
                    case Fill:
                        // coordindex is count of previous cmd coords minus 1
                        NextUndo.CoordIndex = (pos2 - pos1 - 1) / 2 - 1;
                        break;
                    case PlotPen:
                        // coordindex is count of previous cmd coords
                        if (SelectedCmd.Pen.PlotStyle == PlotStyle.Solid) {
                            NextUndo.CoordIndex = (pos2 - pos1 - 1) / 2;
                        }
                        else {
                            NextUndo.CoordIndex = (pos2 - pos1 - 1) / 3;
                        }
                        break;
                    }
                    break;
                }
                NextUndo.CmdIndex = index1;
                AddUndo(NextUndo);
            }

            switch (cmd2) {
            case Fill:
            case PlotPen:
                // just delete the command byte
                lngCount = 1;
                break;
            case XCorner:
            case YCorner:
                // get orientation of last line of first cmd
                bool IsVertical = (SelectedCmd.Position - (int)lstCommands.Items[index1].Tag).IsOdd();
                if (EditPicture.Data[(int)lstCommands.Items[index1].Tag] == (byte)YCorner) {
                    // flip it
                    IsVertical = !IsVertical;
                }
                // if either line has only one coordinate, set IsVert to force alignment
                if (SelectedCmd.Coords.Count == 1 || pos2 - pos1 - 2 == 1) {
                    IsVertical = cmd2 == XCorner;
                    // further, if first cmd has only one coord need to ensure the
                    // cmd type matches the second cmd
                    if (pos2 - pos1 - 2 == 1) {
                        EditPicture.Data[pos1] = (byte)cmd2;
                        lstCommands.Items[index1].Text = cmd2.CommandName();
                        if (!DontUndo) {
                            UndoCol.Peek().DrawCommand = cmd1;
                            UndoCol.Peek().PicPos = pos1;
                        }
                    }
                }
                // if orientation of last line of first cmd is same as first line of this cmd
                if (((cmd2 == XCorner) && !IsVertical) ||
                    ((cmd2 == YCorner) && IsVertical)) {
                    // delete last coordinate from previous cmd AND command and first coordinate
                    pos2 -= 1;
                    lngCount = 4;
                    if (!DontUndo) {
                        // need to let undo know that the join extends a segment instead
                        // of just appending 
                        UndoCol.Peek().Data = [EditPicture.Data[pos2]];
                        UndoCol.Peek().PicPos = pos2;
                    }
                }
                else {
                    // delete command and first coordinate
                    lngCount = 3;
                }
                break;
            default:
                // delete command and first coordinate
                lngCount = 3;
                break;
            }
            // delete the data
            EditPicture.RemoveData(pos2, lngCount);
            // remove the second command from list
            lstCommands.Items.RemoveAt(index2);
            // update follow on cmds
            UpdatePosValues(index2, -lngCount);
            // reselect first command
            SelectCommand(index1, 1, true);
        }

        /// <summary>
        /// Splits the currently selected command into two commands at the 
        /// current coordinate position.
        /// </summary>
        /// <param name="DontUndo"></param>
        private void SplitCommand(bool DontUndo = false) {
            int lngPos, lngCount = 0;
            byte[] bytData;

            int lngCmdIndex = SelectedCmd.Index;
            Point tmpPT = SelectedCmd.SelectedCoord;
            DrawFunction CmdType = SelectedCmd.Type;
            // get insert pos
            if (CmdType == Fill || CmdType == PlotPen) {
                lngPos = SelectedCmd.SelectedCoordPos;
            }
            else {
                // insertion point is NEXT coord
                lngPos = SelectedCmd.CoordPos(SelectedCmd.SelectedCoordIndex + 1);
            }
            // insert a new command in listbox
            lstCommands.Items.Insert(lngCmdIndex + 1, CmdType.CommandName()).Tag = lngPos;
            CmdColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

            // add picture data based on command type
            switch (CmdType) {
            case XCorner:
            case YCorner:
                // get orientation of line being split
                bool IsVertical = SelectedCmd.SelectedCoordIndex.IsOdd();
                // if cmd is a yCorner
                if (CmdType == YCorner) {
                    // flip it
                    IsVertical = !IsVertical;
                }

                // if splitting a vertical line,
                if (IsVertical) {
                    // if inserted byte is not a Ycorner
                    if (CmdType != YCorner) {
                        // change inserted cmd to YCorner
                        CmdType = YCorner;
                        lstCommands.Items[lngCmdIndex + 1].Text = YCorner.CommandName();
                    }
                }
                else {
                    // if inserted byte is not a Xcorner
                    if (CmdType != XCorner) {
                        // change inserted cmd to XCorner
                        CmdType = XCorner;
                        lstCommands.Items[lngCmdIndex + 1].Text = XCorner.CommandName();
                    }
                }

                // insert starting point in resource
                bytData = [(byte)CmdType, (byte)tmpPT.X, (byte)tmpPT.Y];
                EditPicture.InsertData(bytData, lngPos);
                // three bytes inserted
                lngCount = 3;
                break;
            case AbsLine:
            case RelLine:
                // insert starting point into new cmd
                bytData = [(byte)CmdType, (byte)tmpPT.X, (byte)tmpPT.Y];
                EditPicture.InsertData(bytData, lngPos);
                // three bytes inserted
                lngCount = 3;
                break;
            case Fill:
            case PlotPen:
                EditPicture.InsertData((byte)CmdType, lngPos);
                // only one byte inserted
                lngCount = 1;
                break;
            }
            // update positions for cmds AFTER the new cmd
            UpdatePosValues(lngCmdIndex + 2, lngCount);
            // select the newly added command
            SelectCommand(lngCmdIndex + 1, 1, true);
            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = SplitCmd;
                NextUndo.PicPos = lngPos;
                NextUndo.CmdIndex = lngCmdIndex + 1;
                AddUndo(NextUndo);
            }
        }

        /// <summary>
        /// Deletes the specified commands from the picture resource and updates
        /// the draw surfaces.
        /// </summary>
        /// <param name="endcmdindex"></param>
        /// <param name="cmdcount"></param>
        /// <param name="DontUndo"></param>
        private void DeleteCommands(int endcmdindex, int cmdcount, bool DontUndo = false) {
            PenStatus startPen = EditPicture.GetPenStatus((int)lstCommands.Items[endcmdindex - cmdcount + 1].Tag);
            PenStatus endPen = EditPicture.GetPenStatus((int)lstCommands.Items[endcmdindex + 1].Tag - 1);
            if (startPen.PlotStyle != endPen.PlotStyle && !DontUndo) {
                // adjust plot pattern starting with next command after the pasted commands
                // (this must be done BEFORE the resource is modified, otherwise the undo
                // feature won't work correctly)
                ReadjustPlotCoordinates(endcmdindex + 1, startPen.PlotStyle);
            }
            // save position of first command that is selected
            int lngStartPos = (int)lstCommands.Items[endcmdindex - cmdcount + 1].Tag;
            int bytecount = (int)lstCommands.Items[endcmdindex + 1].Tag - lngStartPos;

            if (!DontUndo) {
                PictureUndo NextUndo = new();
                NextUndo.Action = PictureUndo.ActionType.DelCmd;
                // save position of first command that is selected
                NextUndo.PicPos = lngStartPos;
                // save cmd location and Count of commands
                NextUndo.CmdIndex = endcmdindex;
                NextUndo.CmdCount = cmdcount;
                NextUndo.Text = "Command";
                NextUndo.Data = EditPicture.Data[NextUndo.PicPos..(int)lstCommands.Items[endcmdindex + 1].Tag];
                // add to undo
                AddUndo(NextUndo);
            }
            // delete data from array
            EditPicture.RemoveData(lngStartPos, bytecount);
            // adjust commands AFTER deleted commands
            UpdatePosValues(endcmdindex + 1, -bytecount);
            // now delete the command box entries
            for (int i = 0; i < cmdcount; i++) {
                lstCommands.Items.RemoveAt(endcmdindex - cmdcount + 1);
            }
        }

        /// <summary>
        /// When pen commands are added or changed, all plot commands following
        /// the change must be adjusted to match the new plot style (by adding or
        /// removing splatter data). This method makes the necessary adjustments.
        /// </summary>
        /// <param name="StartIndex"></param>
        /// <param name="NewPlotStyle"></param>
        private void ReadjustPlotCoordinates(int StartIndex, PlotStyle NewPlotStyle) {
            // starting at command in list at StartIndex, step through all
            // commands until another setplotpen command or end is reached;
            // any plot commands identified during search are checked to
            // see if they match format of desired plot pen style (solid or
            // splatter); if they don't match, they are adjusted (by adding
            // or removing the pattern byte)

            byte[] bytTemp;
            int StopIndex = lstCommands.Items.Count - 1;
            int i = StartIndex;
            do {
                // check for plot command or change plot pen command
                switch ((DrawFunction)EditPicture.Data[(int)lstCommands.Items[i].Tag]) {
                case PlotPen:
                    // if style is splatter
                    if (NewPlotStyle == PlotStyle.Splatter) {
                        // need to set tmp byte array so addpatterndata method
                        // will know to create the random bytes for this set of coordinates
                        bytTemp = [0xFF];
                        // add pattern bytes (use a temp array as place holder for byte array argument)
                        AddPatternData(i, bytTemp);
                    }
                    else {
                        // delete pattern bytes
                        DelPatternData(i);
                    }
                    break;
                case ChangePen:
                    // set pen
                    // can exit here because this pen command
                    // ensures future plot commands are correct
                    i = StopIndex;
                    break; // exit do
                }
                // get next cmd
                i++;
            } while (i <= StopIndex);
        }

        /// <summary>
        /// Adds new splatter data to the plot command at the specified index
        /// location. If skipping undo, pattern values will be passed in
        /// bytPatDat; if not skipping undo generate random pattern data.
        /// </summary>
        /// <param name="tmpIndex"></param>
        /// <param name="bytPatDat"></param>
        /// <param name="DontUndo"></param>
        private void AddPatternData(int tmpIndex, byte[] bytPatDat, bool DontUndo = false) {
            byte bytPattern;
            int count = 0;

            // set insertpos so first iteration will add
            // pattern data in front of first coord x Value
            int lngNewPos = (int)lstCommands.Items[tmpIndex].Tag + 1;
            do {
                // if skipping undo
                if (DontUndo) {
                    // if first byte of array is 255,
                    if (bytPatDat[0] == 0xFF) {
                        // need to provide the random bytes for this set of coordinates
                        bytPattern = (byte)(GetRandomByte(0, 119) * 2);
                    }
                    else {
                        // get pattern from array
                        bytPattern = bytPatDat[count];
                    }
                }
                else {
                    // get random pattern
                    bytPattern = (byte)(GetRandomByte(0, 119) * 2);
                }
                // add it to resource
                EditPicture.InsertData(bytPattern, lngNewPos);
                // adjust pos
                lngNewPos += 3;
                count++;
            } while (EditPicture.Data[lngNewPos] < 0xF0);

            // adjust positions (i equals number of bytes added)
            UpdatePosValues(tmpIndex + 1, count);

            // if not skipping undo
            if (!DontUndo) {
                // save undo info
                PictureUndo NextUndo = new();
                NextUndo.Action = PictureUndo.ActionType.AddPlotPattern;
                NextUndo.PicPos = (int)lstCommands.Items[tmpIndex].Tag;
                NextUndo.CmdIndex = tmpIndex;
                // add the undo object without setting edit menu
                AddUndo(NextUndo);
            }
        }

        /// <summary>
        /// Removes splatter data from the specified plot command.
        /// </summary>
        /// <param name="tmpIndex"></param>
        /// <param name="DontUndo"></param>
        private void DelPatternData(int tmpIndex, bool DontUndo = false) {
            byte bytPattern;
            int count = 0;
            List<byte> bytPatDat = new();

            // set start pos so first iteration will select pattern byte for first coord
            int lngNewPos = (int)lstCommands.Items[tmpIndex].Tag + 1;
            do {
                // if not skipping undo
                if (!DontUndo) {
                    bytPattern = EditPicture.Data[lngNewPos];
                    // save to array
                    bytPatDat.Add(bytPattern);
                }
                // remove from picture resource
                EditPicture.RemoveData(lngNewPos);
                count++;
                // adjust pos
                lngNewPos += 2;
            } while (EditPicture.Data[lngNewPos] < 0xF0);

            // adjust positions of follow on commands (i now equals number of bytes removed)
            UpdatePosValues(tmpIndex + 1, -count);

            // if not skipping undo
            if (!DontUndo) {
                // save undo info
                PictureUndo NextUndo = new();
                NextUndo.Action = Editor.PictureUndo.ActionType.DelPlotPattern;
                NextUndo.PicPos = (int)lstCommands.Items[tmpIndex].Tag;
                NextUndo.CmdIndex = tmpIndex;
                NextUndo.Data = bytPatDat.ToArray();
                // add the undo object without setting edit menu
                AddUndo(NextUndo);
            }
        }

        /// <summary>
        /// This method scans the undo collection and adds or removes plot
        /// splatter data as needed until all plot pattern adjustments are 
        /// undone.
        /// </summary>
        private void UndoPlotAdjust() {
            if (UndoCol.Count > 0) {
                while (UndoCol.Peek().Action == AddPlotPattern || UndoCol.Peek().Action == DelPlotPattern) {
                    PictureUndo tmpUndo = UndoCol.Pop();
                    if (tmpUndo.Action == AddPlotPattern) {
                        // remove this pattern data
                        DelPatternData(tmpUndo.CmdIndex, true);
                    }
                    else {
                        // add this pattern data
                        AddPatternData(tmpUndo.CmdIndex, tmpUndo.Data, true);
                    }
                    if (UndoCol.Count == 0) {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the tag values in the command list to point to the correct
        /// data position in the picture resource when commands are added or
        /// removed.
        /// </summary>
        /// <param name="cmdindex"></param>
        /// <param name="offset"></param>
        private void UpdatePosValues(int cmdindex, int offset) {
            for (int i = cmdindex; i < lstCommands.Items.Count; i++) {
                lstCommands.Items[i].Tag = (int)lstCommands.Items[i].Tag + offset;
            }
        }

        /// <summary>
        /// Deletes the specified coordinate from the current command.
        /// </summary>
        /// <param name="delcoordindex"></param>
        /// <param name="DontUndo"></param>
        public void DeleteCoordinate(int delcoordindex, bool DontUndo = false) {

            int DelCount;
            int DelPos;
            byte[] bytUndoData;

            // if this is last item
            if (SelectedCmd.Coords.Count == 1) {
                // send focus to cmd list
                lstCommands.Focus();
                // use command delete
                DeleteCommands(SelectedCmd.Index, 1);
                SelectCommand(SelectedCmd.Index, 1, true);
                return;
            }
            else {
                // remove the coordinates at this position
                DelPos = SelectedCmd.CoordPos(delcoordindex);
                if (SelectedCmd.Type == YCorner && delcoordindex == 0) {
                    // delete the second byte (the Y value)
                    DelPos++;
                }
                // if deleting a plot point in splatter mode
                if (SelectedCmd.Type == PlotPen && SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    DelCount = 3;
                    // if deleting a relative line, or a step line
                }
                else if (SelectedCmd.Type == RelLine || SelectedCmd.Type == XCorner || SelectedCmd.Type == YCorner) {

                    DelCount = 1;
                }
                else {
                    DelCount = 2;
                }

                // if not skipping undo
                if (!DontUndo) {
                    // create new undo object
                    PictureUndo NextUndo = new();
                    NextUndo.Action = PictureUndo.ActionType.DelCoord;
                    NextUndo.DrawCommand = SelectedCmd.Type;
                    NextUndo.PicPos = DelPos;
                    NextUndo.CmdIndex = SelectedCmd.Index;
                    NextUndo.CoordIndex = delcoordindex;
                    NextUndo.Coord = SelectedCmd.Coords[delcoordindex];
                    bytUndoData = new byte[DelCount];
                    for (int i = 0; i < DelCount; i++) {
                        bytUndoData[i] = EditPicture.Data[DelPos + i];
                    }
                    NextUndo.Data = bytUndoData;
                    // add to undo
                    AddUndo(NextUndo);
                }
                // adjust the resource
                if (delcoordindex == 0) {
                    switch (SelectedCmd.Type) {
                    case RelLine:
                        // deleting first coord of a RelLine means the second byte (Y0)
                        // has to be changed to X1 and third byte (delta1) has to be 
                        // changed to Y1
                        EditPicture.Data[DelPos + 1] = (byte)SelectedCmd.Coords[1].X;
                        EditPicture.Data[DelPos + 2] = (byte)SelectedCmd.Coords[1].Y;
                        break;
                    case XCorner:
                        // deleting first coord of an XCorner means the second and third
                        // bytes (Y0, X1) need to be swapped and command type swapped to
                        // YCorner
                        EditPicture.Data[DelPos - 1] = (byte)YCorner;
                        EditPicture.Data[DelPos + 1] = (byte)SelectedCmd.Coords[1].X;
                        EditPicture.Data[DelPos + 2] = (byte)SelectedCmd.Coords[0].Y;
                        SelectedCmd.Type = YCorner;
                        if (!DontUndo) {
                            lstCommands.Items[SelectedCmd.Index].Text = YCorner.CommandName();
                        }
                        break;
                    case YCorner:
                        // deleting first coord of a YCorner means only remove Y0 (done
                        // by incrementing DelPos above) swapping command type to XCorner
                        EditPicture.Data[DelPos - 2] = (byte)XCorner;
                        SelectedCmd.Type = XCorner;
                        if (!DontUndo) {
                            lstCommands.Items[SelectedCmd.Index].Text = XCorner.CommandName();
                        }
                        break;
                    }
                }
                EditPicture.RemoveData(DelPos, DelCount);
                // adjust position values to account for deleted data
                UpdatePosValues(SelectedCmd.Index + 1, -DelCount);
                // remove from coord list
                SelectedCmd.Coords.RemoveAt(delcoordindex);
                if (!DontUndo) {
                    lstCoords.Items.RemoveAt(delcoordindex);
                }
            }
            MarkAsChanged();
        }

        /// <summary>
        /// Inserts a new coordinate in the current command at the current
        /// coordinate position.
        /// </summary>
        private void InsertCoordinate() {
            byte[] coorddata = [];
            int newindex;
            if (PicMode != PicEditorMode.Edit || SelectedTool == PicToolType.SelectArea) {
                return;
            }

            if (SelectedCmd.SelectedCoordIndex == -1) {
                if (SelectedTool == PicToolType.Edit && SelectedCmd.Type == PlotPen || SelectedCmd.Type == Fill) {
                    SelectedCmd.SelectedCoordIndex = SelectedCmd.Coords.Count - 1;
                }
                else if ((SelectedTool == PicToolType.Plot && SelectedCmd.Type == PlotPen) ||
                         (SelectedTool == PicToolType.Fill && SelectedCmd.Type == Fill)) {
                    SelectedCmd.SelectedCoordIndex = SelectedCmd.Coords.Count - 1;
                }
                else {
                    // no coord selected
                    return;
                }
            }
            else {
                switch (SelectedCmd.Type) {
                case AbsLine:
                case Fill:
                case PlotPen:
                    // ok
                    break;
                default:
                    // Corner lines or relative lines
                    if (SelectedCmd.SelectedCoordIndex == 0 || SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1) {
                        // ok
                        break;
                    }
                    // otherwise not ok
                    return;
                }
            }
            newindex = SelectedCmd.SelectedCoordIndex + 1;

            // add a new coordinate after this coordinate
            switch (SelectedCmd.Type) {
            case AbsLine:
            case Fill:
                coorddata = [(byte)SelectedCmd.SelectedCoord.X, (byte)SelectedCmd.SelectedCoord.Y];
                break;
            case PlotPen:
                // depends on pen- if splatter, we need an extra data element
                if (SelectedCmd.Pen.PlotStyle == PlotStyle.Solid) {
                    coorddata = [(byte)SelectedCmd.SelectedCoord.X, (byte)SelectedCmd.SelectedCoord.Y];
                }
                else {
                    coorddata = [(byte)(2 * GetRandomByte(0, 119)), (byte)SelectedCmd.SelectedCoord.X, (byte)SelectedCmd.SelectedCoord.Y];
                }
                break;
            case RelLine:
                // insert zero offest coordinate AFTER selected coordinate
                coorddata = [0];
                break;
            case XCorner:
                // can only add to end or beginning
                if (SelectedCmd.SelectedCoordIndex == 0) {
                    // change to YCorner, and add new Y value that is same as current
                    // before: x0 y0 -- x1 y2 ...
                    // after:  x0 y0 y0 x1 y2 ...
                    coorddata = [(byte)SelectedCmd.SelectedCoord.Y];
                    EditPicture.Data[SelectedCmd.Position] = (byte)YCorner;
                    SelectedCmd.Type = YCorner;
                    lstCommands.Items[SelectedCmd.Index].Text = YCorner.CommandName();
                }
                else {
                    Debug.Assert(SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1);
                    if (SelectedCmd.SelectedCoordIndex.IsOdd()) {
                        // adding a new Y
                        coorddata = [(byte)SelectedCmd.SelectedCoord.Y];
                    }
                    else {
                        // adding a new X
                        coorddata = [(byte)SelectedCmd.SelectedCoord.X];
                    }
                }
                break;
            case YCorner:
                // can only add to end or beginning
                if (SelectedCmd.SelectedCoordIndex == 0) {
                    // change to XCorner, and add new X value that is same as current
                    // before: x0 y0 -- y1 x2 y3 ...
                    // after:  x0 y0 x0 y1 x2 y3 ...
                    coorddata = [(byte)SelectedCmd.SelectedCoord.X];
                    EditPicture.Data[SelectedCmd.Position] = (byte)XCorner;
                    SelectedCmd.Type = XCorner;
                    lstCommands.Items[SelectedCmd.Index].Text = XCorner.CommandName();
                }
                else {
                    Debug.Assert(SelectedCmd.SelectedCoordIndex == SelectedCmd.Coords.Count - 1);
                    if (SelectedCmd.SelectedCoordIndex.IsEven()) {
                        // adding a new Y
                        coorddata = [(byte)SelectedCmd.SelectedCoord.Y];
                    }
                    else {
                        // adding a new x
                        coorddata = [(byte)SelectedCmd.SelectedCoord.X];
                    }
                }
                break;
            }
            // add it
            AddCoordToPic(coorddata, SelectedCmd.SelectedCoord, newindex);
            // select it
            SelectCoordinate(newindex, true);
        }

        /// <summary>
        /// Returns true if the passed command is eligible to be joined
        /// to the preceding command (i.e. both are the same command,
        /// and ending coordinates match if the command is a line).
        /// </summary>
        /// <param name="secondCmdIndex"></param>
        /// <returns></returns>
        private bool CanJoinCommands(int secondCmdIndex) {
            if (SelectedCmd.IsPen || SelectedCmdCount != 2) {
                return false;
            }
            int firstCmdIndex = secondCmdIndex - 1;
            // if the two commands are same type OR
            // both types are X/Y lines
            if (SelectedCmd.Type == (DrawFunction)EditPicture.Data[(int)lstCommands.Items[firstCmdIndex].Tag] ||
                ((SelectedCmd.Type == XCorner || SelectedCmd.Type == YCorner) &&
                (EditPicture.Data[(int)lstCommands.SelectedItems[0].Tag] == (byte)XCorner ||
                EditPicture.Data[(int)lstCommands.SelectedItems[0].Tag] == (byte)YCorner))) {
                switch (SelectedCmd.Type) {
                case PlotPen:
                case Fill:
                    // ok to join
                    return true;
                case RelLine:
                case AbsLine:
                    // end coordinates have to match
                    if (MatchPoints(SelectedCmd.Index)) {
                        return true;
                    }
                    break;
                default:
                    // end coordinates have to match
                    if (MatchPoints(SelectedCmd.Index)) {
                        return true;

                        // alternate method of checking for join of
                        // X/Ycorners; it enforces line direction (i.e.
                        // if last leg of first command is vertical, first
                        // leg of second command must be horizontal and 
                        // vice versa
                        /*
                        // if either cmd is exactly one coord ( three bytes in the cmd)
                        if (SelectedCmd.Coords.Count == 1 || (SelectedCmd.Position - (int)lstCommands.SelectedItems[0].Tag) == 3) {
                            return true;
                        }
                        // line direction must be opposite
                        bool isvert1 = SelectedCmd.Coords.Count.IsEven();
                        if (SelectedCmd.Type == XCorner) {
                            isvert1 = !isvert1;
                        }
                        bool isvert2 = (SelectedCmd.Position - (int)lstCommands.SelectedItems[0].Tag).IsOdd();
                        if (EditPicture.Data[(int)lstCommands.SelectedItems[0].Tag] == (byte)XCorner) {
                            isvert2 = !isvert2;
                        }
                        return isvert1 != isvert2;
                        */
                    }
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// Initiates a draw operation using the specified tool.
        /// </summary>
        /// <param name="CurrentTool"></param>
        /// <param name="PicPt"></param>
        private void BeginDraw(PicToolType CurrentTool, Point PicPt) {
            byte[] bytData = [];

            AnchorPT = PicPt;
            switch (CurrentTool) {
            case PicToolType.Line:
                bytData = [(byte)AbsLine, (byte)PicPt.X, (byte)PicPt.Y];
                break;
            case PicToolType.ShortLine:
                bytData = [(byte)RelLine, (byte)PicPt.X, (byte)PicPt.Y];
                break;
            case PicToolType.StepLine:
                // default to xcorner
                bytData = [(byte)XCorner, (byte)PicPt.X, (byte)PicPt.Y];
                break;
            }
            // insert command
            InsertCommand(bytData, SelectedCmd.Index);
            // select this cmd
            SelectCommand(SelectedCmd.Index, 1, true);
            // now set mode (do it AFTER selecting command otherwise
            // draw mode will get canceled)
            PicDrawMode = PicDrawOp.Line;
        }

        /// <summary>
        /// Inserts a new command into the picture resource and updates
        /// the command listbox.
        /// </summary>
        /// <param name="newdata"></param>
        /// <param name="insertindex"></param>
        /// <param name="DontUndo"></param>
        /// <returns></returns>
        private int InsertCommand(byte[] newdata, int insertindex, bool DontUndo = false) {
            int insertpos = (int)lstCommands.Items[insertindex].Tag;

            // if not skipping undo
            if (!DontUndo) {
                // create new undo object
                PictureUndo NextUndo = new();
                NextUndo.Action = AddCmd;
                NextUndo.PicPos = insertpos;
                NextUndo.CmdIndex = insertindex;
                NextUndo.DrawCommand = (DrawFunction)newdata[0];
                NextUndo.CmdCount = 1;
                NextUndo.ByteCount = newdata.Length;
                AddUndo(NextUndo);
            }
            // insert data
            EditPicture.InsertData(newdata, insertpos);
            // insert into cmd list
            lstCommands.Items.Insert(insertindex, ((DrawFunction)newdata[0]).CommandName()).Tag = insertpos;
            CmdColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            // update position values in rest of tree
            UpdatePosValues(insertindex + 1, newdata.Length);
            return insertindex;
        }

        /// <summary>
        ///  Updates the picture resource with the new coordinate data and
        ///  refreshes the command and coordinate listboxes.
        /// </summary>
        /// <param name="NewData"></param>
        /// <param name="location"></param>
        /// <param name="insertindex"></param>
        /// <param name="DontUndo"></param>
        /// <returns></returns>
        public int AddCoordToPic(byte[] NewData, Point location, int insertindex, bool DontUndo = false) {
            int insertpos;

            if (insertindex == -1) {
                // insert at end
                insertpos = SelectedCmd.EndPos + 1;
                insertindex = SelectedCmd.Coords.Count;
            }
            else {
                // insert at current coord index
                insertpos = SelectedCmd.CoordPos(insertindex);
            }

            // if not skipping undo
            if (!DontUndo) {
                // create new undo object
                PictureUndo NextUndo = new();
                NextUndo.Action = AddCoord;
                NextUndo.PicPos = insertpos;
                NextUndo.ByteCount = NewData.Length;
                NextUndo.CmdIndex = SelectedCmd.Index;
                NextUndo.CoordIndex = insertindex;
                // add to undo
                AddUndo(NextUndo);
            }
            // insert data
            EditPicture.InsertData(NewData, insertpos);
            SelectedCmd.Coords.Insert(insertindex, location);
            // insert coord text (if selected cmd matches lstCommand.SelectedIndex)
            if (lstCommands.SelectedIndices[^1] == SelectedCmd.Index) {
                if (SelectedCmd.Type == PlotPen && SelectedCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    lstCoords.Items.Insert(insertindex, (EditPicture.Data[SelectedCmd.CoordPos(insertindex)] / 2).ToString() + " -- " + CoordText(location));
                }
                else {
                    lstCoords.Items.Insert(insertindex, CoordText(location));
                }
                CoordColumnHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            // update position values in rest of cmd list
            UpdatePosValues(SelectedCmd.Index + 1, NewData.Length);
            return insertindex + 1 < lstCoords.Items.Count ? insertindex + 1 : -1;
        }

        /// <summary>
        /// Begins a drag operation for the specified draw surface.
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="startpos"></param>
        private void StartDrag(PictureBox pic, Point startpos) {
            switch (pic.Name) {
            case "picVisual":
                if (hsbVisual.Visible || vsbVisual.Visible) {
                    DragPT = startpos;
                    blnDragging = true;
                    SetCursors(PicCursor.DragSurface);
                }
                break;
            case "picPriority":
                if (hsbPriority.Visible || vsbPriority.Visible) {
                    DragPT = startpos;
                    blnDragging = true;
                    SetCursors(PicCursor.DragSurface);
                }
                break;
            }
        }

        /// <summary>
        /// Gets a test view from the current game (if a game is loaded) or
        /// from a file (if no game is loaded) for use as the test view for
        /// this editor.
        /// </summary>
        private void GetTestView() {
            // get a test view to use in test mode

            // if game is loaded
            if (EditGame is not null) {
                // use the get resource form
                frmGetResourceNum frmNew = new frmGetResourceNum(GetRes.TestView, AGIResType.View);
                frmNew.OldResNum = TestViewNum;
                // if canceled, unload and exit
                DialogResult result = frmNew.ShowDialog(this);
                byte num = frmNew.NewResNum;
                frmNew.Dispose();
                if (result == DialogResult.OK) {
                    // set testview id
                    TestViewNum = num;
                }
                else {
                    return;
                }
            }
            else {
                // get test view from file
                MDIMain.OpenDlg.FileName = "";
                MDIMain.OpenDlg.InitialDirectory = DefaultResDir;
                MDIMain.OpenDlg.Title = "Choose Test Vie";
                MDIMain.OpenDlg.Filter = "WinAGI View Resource files (*.agv)|*.agv|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Views", sOPENFILTER, 1);
                if (MDIMain.OpenDlg.ShowDialog() == DialogResult.Cancel) {
                    // user canceled
                    return;
                }
                else {
                    string section = "";
                    section = "Views";
                    WinAGISettingsFile.WriteSetting(section, sOPENFILTER, MDIMain.OpenDlg.FilterIndex);
                    DefaultResDir = JustPath(MDIMain.OpenDlg.FileName);
                }
                TestViewFile = MDIMain.OpenDlg.FileName;
            }

            // reload testview
            if (LoadTestView(TestViewNum)) {
                if (TestDir != ObjDirection.odStopped) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
            }
        }

        /// <summary>
        /// Loads the specified view, then clones it for use as the
        /// test view for this editor.
        /// </summary>
        /// <returns></returns>
        private bool LoadTestView(byte loadviewnum) {
            if (TestView is not null) {
                // unload it and release it
                TestView.Unload();
                TestView = null;
                //  disable test cel drawing
                ShowTestCel = false;
            }

            TestView = new Engine.View();

            try {
                if (EditGame is not null) {
                    // copy from game
                    bool loaded = EditGame.Views[loadviewnum].Loaded;
                    if (!loaded) {
                        EditGame.Views[loadviewnum].Load();
                    }
                    TestView.CloneFrom(EditGame.Views[loadviewnum]);
                    if (!loaded) {
                        EditGame.Views[loadviewnum].Unload();
                    }
                }
                else {
                    // load from file
                    TestView.Import(TestViewFile);
                }
            }
            catch (Exception ex) {
                ErrMsgBox(ex, "Unable to load view resource due to error:",
                    ex.StackTrace + "\n\nTest view not set.",
                    "Test View Error");
                TestView = null;
                return false;
            }
            // reset to defaults
            CurTestLoop = 0;
            TestSettings.TestLoop = -1;
            CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
            CurTestCel = 0;
            TestSettings.TestCel = -1;
            TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
            TestDir = 0;
            // set cel height/width/transcolor
            CelWidth = TestView[CurTestLoop][CurTestCel].Width;
            CelHeight = TestView[CurTestLoop][CurTestCel].Height;
            CelTrans = TestView[CurTestLoop][CurTestCel].TransColor;
            // disable drawing until user actually places the cel
            ShowTestCel = false;

            // if already in test mode (and changing the view being used)
            if (PicMode == PicEditorMode.ViewTest) {
                // redraw picture to clear old testview
                DrawPicture();
            }
            return true;
        }

        /// <summary>
        ///  Draws the current test view cel onto the specified draw surface.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="onVis"></param>
        private void AddCelToPic(Graphics g, bool onVis) {
            int CelPriority;
            Cel TestCel = TestView[CurTestLoop][CurTestCel];

            // set priority (if in auto, get priority from current band)
            if (TestSettings.ObjPriority.Value < 16) {
                CelPriority = TestSettings.ObjPriority.Value;
            }
            else {
                CelPriority = GetPriBand((byte)TestCelPos.Y, EditPicture.PriBase);
            }
            // verify position
            if (TestCelPos.Y - (CelHeight - 1) < 0) {
                TestCelPos.Y = CelHeight - 1;
            }
            if (TestCelPos.X + CelWidth > 160) {
                TestCelPos.X = 160 - CelWidth;
            }

            for (int i = 0; i < CelWidth; i++) {
                for (int j = 0; j < CelHeight; j++) {
                    byte cX = (byte)(TestCelPos.X + i);
                    byte cY = (byte)(TestCelPos.Y - (CelHeight - 1) + j);
                    // get cel pixel color
                    int CelPixelColor = TestCelData[i, j];
                    // if not a transparent cel
                    if (CelPixelColor != (byte)CelTrans) {
                        // get pixelpriority of the cel
                        int PixelPriority = (int)EditPicture.PixelPriority(cX, cY);
                        if (onVis) {
                            // if priority of cel is equal to or higher than priority of pixel
                            if (CelPriority >= PixelPriority) {
                                // set this pixel on visual screen
                                if (ScaleFactor == 1) {
                                    Pen lc = new(EditPalette[CelPixelColor]);
                                    g.DrawLine(lc, cX * 2, cY, cX * 2, cY);
                                }
                                else {
                                    SolidBrush lb = new(EditPalette[CelPixelColor]);
                                    g.FillRectangle(lb, cX * ScaleFactor * 2, cY * ScaleFactor, ScaleFactor * 2, ScaleFactor);
                                }
                            }
                        }
                        else {
                            int PriPixelColor = (int)EditPicture.PriPixelColor(cX, cY);
                            if (CelPriority >= PixelPriority && PriPixelColor >= 3) {
                                // set this pixel on priority screen
                                if (ScaleFactor == 1) {
                                    Pen lc = new(EditPalette[CelPriority]);
                                    g.DrawLine(lc, cX * 2, cY, cX * 2, cY);
                                }
                                else {
                                    SolidBrush lb = new(EditPalette[CelPriority]);
                                    g.FillRectangle(lb, cX * ScaleFactor * 2, cY * ScaleFactor, ScaleFactor * 2, ScaleFactor);
                                }
                            }
                        }
                    }
                }
            }
            // if status bar is showing object info
            if (StatusMode == PicStatusMode.Coord) {
                // use test object position
                spCurX.Text = "vX: " + TestCelPos.X;
                spCurY.Text = "vY: " + TestCelPos.Y;
                spPriBand.Text = "vBand: " + CelPriority;
                Bitmap bitmap = new(12, 12);
                using Graphics sg = Graphics.FromImage(bitmap);
                sg.Clear(EditPalette[CelPriority]);
                spPriBand.Image = bitmap;
            }
        }

        /// <summary>
        /// Update the test view object's direction based on the passed
        /// keycode.
        /// </summary>
        /// <param name="KeyCode"></param>
        /// <returns></returns>
        public bool ChangeDir(Keys KeyCode) {

            // if view is not on picture
            if (!ShowTestCel) {
                return false;
            }

            // takes a keycode as the input, and changes direction if appropriate
            switch (KeyCode) {
            case Keys.Up:
            case Keys.NumPad8:
                if (TestDir == ObjDirection.odUp) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odUp;
                    // set loop to 3, if there are four AND loop is not 3 AND in auto
                    if (TestView.Loops.Count >= 4 && CurTestLoop != 3 && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 3;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.PageUp:
            case Keys.NumPad9:
                if (TestDir == ObjDirection.odUpRight) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odUpRight;
                    // set loop to 0, if not already 0 AND in auto
                    if (CurTestLoop != 0 && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 0;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.Right:
            case Keys.NumPad6:
                if (TestDir == ObjDirection.odRight) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odRight;
                    // set loop to 0, if not already 0 AND in auto
                    if (CurTestLoop != 0 && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 0;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.PageDown:
            case Keys.NumPad3:
                if (TestDir == ObjDirection.odDownRight) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odDownRight;
                    // set loop to 0, if not already 0 AND in auto
                    if (CurTestLoop != 0 && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 0;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.Down:
            case Keys.NumPad2:
                if (TestDir == ObjDirection.odDown) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odDown;
                    // set loop to 2, if there are four AND loop is not 2 AND in auto
                    if (CurTestLoop != 2 && TestView.Loops.Count >= 4 && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 2;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.End:
            case Keys.NumPad1:
                if (TestDir == ObjDirection.odDownLeft) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odDownLeft;
                    // set loop to 1, if  at least 2 loops, and not already 1 AND in auto
                    if ((CurTestLoop != 1) && (TestView.Loops.Count >= 2) && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 1;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.Left:
            case Keys.NumPad4:
                if (TestDir == ObjDirection.odLeft) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odLeft;
                    // set loop to 1, if  at least 2 loops, and not already 1 AND in auto
                    if ((CurTestLoop != 1) && (TestView.Loops.Count >= 2) && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 1;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.Home:
            case Keys.NumPad7:
                if (TestDir == ObjDirection.odUpLeft) {
                    TestDir = ObjDirection.odStopped;
                    tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                }
                else {
                    TestDir = ObjDirection.odUpLeft;
                    // set loop to 1, if  at least 2 loops, and not already 1 AND in auto
                    if ((CurTestLoop != 1) && (TestView.Loops.Count >= 2) && (TestSettings.TestLoop == -1)) {
                        CurTestLoop = 1;
                        CurTestLoopCount = (byte)TestView[CurTestLoop].Cels.Count;
                        CurTestCel = 0;
                        TestCelData = TestView[CurTestLoop][CurTestCel].AllCelData;
                    }
                    tmrTest.Enabled = true;
                }
                break;
            case Keys.NumPad5:
                // always stop
                TestDir = 0;
                tmrTest.Enabled = TestSettings.CycleAtRest.Value;
                break;
            default:
                return false;
            }
            return true;
        }

        /// <summary>
        /// Displays the Print/Display Preview options dialog.
        /// </summary>
        private void GetTextOptions() {
            // show print options dialog
            frmPicPrintPrev frm = new(PTInfo, InGame);

            if (frm.ShowDialog(this) == DialogResult.OK) {
                PTInfo = new(frm.PTInfo);
                // show the print/display text on screen if there
                // are no errors in the display text options
                ShowPrintTest = PTInfo.ErrLevel == 0;
                if (ShowPrintTest) {
                    picVisual.Refresh();
                }
            }
            frm.Dispose();
        }

        /// <summary>
        /// Displays the current print/display message text on the 
        /// visual screen to mimic AGI behavior.
        /// </summary>
        /// <param name="g"></param>
        private void DrawPrintTest(Graphics g) {

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            switch (PTInfo.Mode) {
            case PrintTestMode.Print:
            case PrintTestMode.PrintAt:
                // draw the white bounding box
                Brush white = new SolidBrush(EditPalette[15]);
                g.FillRectangle(white, (PTInfo.Left * PTInfo.CharWidth - 10) * ScaleFactor, (PTInfo.Top * 8 - 5) * ScaleFactor, (PTInfo.Width * PTInfo.CharWidth + 20) * ScaleFactor, (PTInfo.Height * 8 + 10) * ScaleFactor);

                // draw the red border
                Brush red = new SolidBrush(EditPalette[4]);
                g.FillRectangle(red, (PTInfo.Left * PTInfo.CharWidth - 8) * ScaleFactor, (PTInfo.Top * 8 - 4) * ScaleFactor, (PTInfo.Width * PTInfo.CharWidth + 15) * ScaleFactor, ScaleFactor);
                g.FillRectangle(red, (PTInfo.Left * PTInfo.CharWidth - 8) * ScaleFactor, ((PTInfo.Top + PTInfo.Height) * 8 + 3) * ScaleFactor, (PTInfo.Width * PTInfo.CharWidth + 15) * ScaleFactor, ScaleFactor);
                g.FillRectangle(red, (PTInfo.Left * PTInfo.CharWidth - 8) * ScaleFactor, ((PTInfo.Top) * 8 - 4) * ScaleFactor, ScaleFactor * 2, (PTInfo.Height * 8 + 8) * ScaleFactor);
                g.FillRectangle(red, ((PTInfo.Left + PTInfo.Width) * PTInfo.CharWidth + 6) * ScaleFactor, (PTInfo.Top * 8 - 4) * ScaleFactor, ScaleFactor * 2, (PTInfo.Height * 8 + 8) * ScaleFactor);

                // draw text
                int tmpRow = PTInfo.Top;
                int tmpCol = PTInfo.Left;
                for (int i = 0; i < PTInfo.Data.Length; i++) {
                    byte charval = PTInfo.Data[i];
                    if (charval == 10) {
                        // move to next line
                        tmpRow++;
                        tmpCol = PTInfo.Left;
                    }
                    else {
                        // draw the character on vis only
                        Rectangle charsource = new((charval % 16) * 16, (int)(charval / 16) * 16, 16, 16);
                        Rectangle chardest = new((int)(tmpCol * ScaleFactor * PTInfo.CharWidth), (int)(tmpRow * ScaleFactor * 8), (int)(PTInfo.CharWidth * ScaleFactor), (int)(8 * ScaleFactor));
                        g.DrawImage(chargrid, chardest, charsource, GraphicsUnit.Pixel);
                        tmpCol++;
                    }
                }
                break;
            case PrintTestMode.Display:
                // display
                Rectangle srcRect;
                int destX = (int)(PTInfo.Left * ScaleFactor * PTInfo.CharWidth);
                int destY = (int)((PTInfo.Top - PTInfo.PicOffset) * ScaleFactor * 8);

                for (int Pos = 0; Pos < PTInfo.Data.Length; Pos++) {
                    byte charval = PTInfo.Data[Pos];
                    if (charval == 10) {
                        destX = 0;
                        destY += (int)(ScaleFactor * 8);
                    }
                    else {
                        // draw the character on vis
                        srcRect = new Rectangle((charval % 16) * 16, (charval / 16) * 16, 16, 16);
                        // Create a new bitmap for the character glyph
                        using (Bitmap glyph = chargrid.Clone(srcRect, chargrid.PixelFormat)) {
                            // Recolor the glyph
                            RecolorGlyph(glyph, EditPalette[PTInfo.FGColor], EditPalette[PTInfo.BGColor]);
                            // Calculate the destination rectangle for the scaled glyph
                            Rectangle destRect = new Rectangle(destX, destY, (int)(PTInfo.CharWidth * ScaleFactor), (int)(8 * ScaleFactor));
                            // Draw the recolored and scaled glyph
                            g.DrawImage(glyph, destRect);
                        }
                        // advance cursor
                        destX += (int)(ScaleFactor * PTInfo.CharWidth);
                        if (destX >= 320 * ScaleFactor) {
                            destX = 0;
                            destY += (int)(ScaleFactor * 8);
                        }
                    }
                    // if on actual bottom row (when offset is at max, and picture
                    // row is 20) don't advance (this mimics AGI behavior)
                    if (destY > (24 - PTInfo.PicOffset) * 8 * ScaleFactor) {
                        destY = (int)((24 - PTInfo.PicOffset) * ScaleFactor * 8);
                    }
                }
                break;
            }
        }

        /// <summary>
        /// Converts a black and white character glyph into the desired 
        /// background and foreground colors.
        /// </summary>
        /// <param name="glyph"></param>
        /// <param name="fgColor"></param>
        /// <param name="bgColor"></param>
        private void RecolorGlyph(Bitmap glyph, Color fgColor, Color bgColor) {
            for (int y = 0; y < glyph.Height; y++) {
                for (int x = 0; x < glyph.Width; x++) {
                    Color pixelColor = glyph.GetPixel(x, y);

                    // Replace black pixels with the foreground color
                    if (pixelColor.R == 0 && pixelColor.G == 0 && pixelColor.B == 0) {
                        glyph.SetPixel(x, y, fgColor);
                    }
                    // Replace white pixels with the background color
                    else if (pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255) {
                        glyph.SetPixel(x, y, bgColor);
                    }
                }
            }
        }

        /// <summary>
        /// If using the AGI Power Pack, this method will toggle the text
        /// screen size between 40 and 80 columns, and redraw the text 
        /// marks if they are currently visible.
        /// </summary>
        private void ToggleTextScreenSize() {
            // toggle width (if powerpack is enabled)
            // always confirm
            if (MessageBox.Show(MDIMain,
                "Change screen size to " + (PTInfo.MaxCol == 39, "80", "40") + " columns?",
                "Toggle Screen Size",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes) {
                if (PTInfo.MaxCol == 39) {
                    PTInfo.MaxCol = 79;
                    PTInfo.CharWidth = 4;
                }
                else {
                    PTInfo.MaxCol = 39;
                    PTInfo.CharWidth = 8;
                }
                if (ShowTextMarks) {
                    // redraw
                    picVisual.Refresh();
                    picPriority.Refresh();
                }
            }
        }

        /// <summary>
        /// Adds an undo object to the undo collection.
        /// </summary>
        /// <param name="NextUndo"></param>
        private void AddUndo(PictureUndo NextUndo) {
            if (!IsChanged) {
                MarkAsChanged();
            }
            UndoCol.Push(NextUndo);
            tsbUndo.Enabled = true;
        }

        internal void ShowHelp() {
            switch (PicMode) {
            case PicEditorMode.Edit:
                Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\editor_picture.htm");
                break;
            case PicEditorMode.PrintTest:
                Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\pictestmode.htm#textprint");
                break;
            case PicEditorMode.ViewTest:
                Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\pictestmode.htm");
                break;
            }
        }
        /// <summary>
        /// When closing the picture editor, this method gives the user
        /// an opportunity to save the resource before closing, or to 
        /// cancel the close operation.
        /// </summary>
        /// <returns></returns>

        private bool AskClose() {
            if (EditPicture.Error != ResourceErrorType.NoError) {
                // if an error occurs on form load, always close
                return true;
            }
            if (PictureNumber == -1) {
                // if forcing close (indicated by setting number to -1,
                // force close
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this picture resource?",
                    "Save Picture Resource",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SavePicture();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "Resource not saved. Continue closing anyway?",
                            "Save Picture Resource",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        return rtn == DialogResult.Yes;
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
                case DialogResult.No:
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// When changes are made to the picture resource, this method can
        /// be called to update the form caption and save-game toolbar
        /// button to indicate as such.
        /// </summary>
        void MarkAsChanged() {
            if (!Visible) {
                // ignore if the form is still loading (it will not be
                // visible yet)
                return;
            }
            if (!IsChanged) {
                IsChanged = true;
                mnuRSave.Enabled = true;
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
        }

        /// <summary>
        /// Resets the form caption and save-game toolbar button to indicate
        /// the resource being edited has been saved to its normal location.
        /// </summary>
        private void MarkAsSaved() {
            IsChanged = false;
            Text = sPICED + ResourceName(EditPicture, InGame, true);
            MDIMain.btnSaveResource.Enabled = false;
        }
        #endregion
    }
}
