using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using System.IO;
using WinAGI.Common;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Globalization;

namespace WinAGI.Editor {
    public partial class frmTextScreenEdit : ClipboardMonitor {

        private const int PE_MARGIN = 5;

        #region Enums
        private enum TSAction {
            None,
            Select,
            Move,
            Drag,
        }

        private enum TSCursor {
            Default,        // default pointer
            Move,           // move (fourarrow)
            Insert,         // insertion caret
            DragSurface,    // hand (drag surface)
            Select          // selection
        }
        #endregion

        #region Structs
        #endregion

        #region Members
        internal string FileName = "";
        internal bool IsChanged;
        private int ScreenCodePage;
        private int ScreenWidth, CharWidth;
        private float ScaleFactor;
        private readonly Bitmap chargrid;
        private readonly Bitmap invchargrid;
        internal EGAColors EditPalette = DefaultPalette.Clone();
        private bool ShowTextMarks;
        // color index of current selection
        private AGIColorIndex TextBG, TextFG;
        // default color indices (when nothing is selected)
        private AGIColorIndex DefBG, DefFG;
        // screen data
        private TextChar[,] TextData;
        private Point DragPT, Offset;
        private TSAction EditAction;
        private bool OverwriteMode;
        private Rectangle Selection;
        private Point AnchorPT;
        private TSCursor CurCursor;
        private bool CursorOn;
        private AGIColorIndex SelColor;
        private TextChar[,] SelData, TempData;
        private int MarginLeft;
        private Point mPT;
        private int dashdistance = 6;
        readonly Pen dash1 = new(Color.Black), dash2 = new(Color.White);

        // StatusStrip Items
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spMode;
        internal ToolStripStatusLabel spRow;
        internal ToolStripStatusLabel spCol;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        #endregion

        public frmTextScreenEdit() {
            InitializeComponent();
            InitStatusStrip();
            MdiParent = MDIMain;
            picScreen.MouseWheel += picScreen_MouseWheel;
            picScreen.KeyDown += picScreen_KeyDown;
            picScreen.KeyPress += picScreen_KeyPress;
            picScreen.KeyUp += picScreen_KeyUp;
            hsbScreen.Minimum = hsbScreen.Value = -PE_MARGIN;
            vsbScreen.Minimum = vsbScreen.Value = -PE_MARGIN;
            picScreen.Location = new(PE_MARGIN, PE_MARGIN);
            dash1.DashPattern = [3, 3];
            dash2.DashPattern = [3, 3];
            dash2.DashOffset = 3;
            InitForm();
            // load correct charmap
            if (EditGame is not null) {
                ScreenCodePage = EditGame.CodePage;
            }
            else {
                ScreenCodePage = 437;
            }
            byte[] obj = (byte[])EditorResources.ResourceManager.GetObject("CP" + ScreenCodePage);
            Stream stream = new MemoryStream(obj);
            chargrid = (Bitmap)Image.FromStream(stream);
            invchargrid = new Bitmap(chargrid.Width, chargrid.Height);
            for (int x = 0; x < chargrid.Width; x++) {
                for (int y = 0; y < chargrid.Height; y++) {
                    Color clrPixel = chargrid.GetPixel(x, y);
                    clrPixel = Color.FromArgb(255 - clrPixel.R, 255 - clrPixel.G, 255 - clrPixel.B);
                    invchargrid.SetPixel(x, y, clrPixel);
                }
            }
            Text = "Text Screen Designer";
        }

        #region Event Handlers
        #region Form Events
        protected override void OnClipboardChanged() {
            base.OnClipboardChanged();
            btnPaste.Enabled = Clipboard.ContainsText(TextDataFormat.UnicodeText) ||
                Clipboard.ContainsData(TXTSCREEN_CB_FMT);
        }

        private void frmTextScreenEdit_Resize(object sender, EventArgs e) {
            if (picPalette.Visible) {
                picPalette.Refresh();
            }
        }

        private void frmTextScreenEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            e.Cancel = !AskClose();
        }

        private void frmTextScreenEdit_FormClosed(object sender, FormClosedEventArgs e) {
            TextScreenEditor = null;
            TSEInUse = false;
            // dispose of graphics objects
            chargrid?.Dispose();
            dash1?.Dispose();
            dash2?.Dispose();
        }
        #endregion

        #region Menu Events
        internal void SetResourceMenu() {
            MDIMain.mnuRSep2.Visible = false;
            MDIMain.mnuRSep3.Visible = false;
        }

        internal void ResetResourceMenu() {
            // no action
        }

        private void mnuROpen_Click(object sender, EventArgs e) {
            LoadScreenTextFile();
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            SaveTextScreen(FileName);
        }

        internal void mnuRSaveAs_Click(object sender, EventArgs e) {
            SaveTextScreen();
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            mnuEdit.DropDownItems.AddRange([mnuMode, cmSep1, mnuCut,
                mnuCopy, mnuPaste, mnuDelete, mnuClear, mnuSelectAll,
                cmSep2, mnuCharMap, mnuToggleTextMarks, mnuChangeScreenSize,
                mnuCopyCommands, mnuPasteCommands, cmSep3, mnuScale]);
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            contextMenuStrip1.Items.AddRange([mnuMode, cmSep1, mnuCut,
                mnuCopy, mnuPaste, mnuDelete, mnuClear, mnuSelectAll,
                cmSep2, mnuCharMap, mnuToggleTextMarks, mnuChangeScreenSize,
                mnuCopyCommands, mnuPasteCommands, cmSep3, mnuScale]);
            ResetEditMenu();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            SetEditMenu();
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void SetEditMenu() {
            // mnuMode
            mnuMode.Text = "Typing Mode: " + (OverwriteMode ? "Overwrite" : "Insert");

            // mnuCut
            mnuCut.Enabled = Selection.Width > 0;

            // mnuCopy
            mnuCopy.Enabled = Selection.Width > 0;

            // mnuPaste
            mnuPaste.Enabled = Clipboard.ContainsText(TextDataFormat.UnicodeText) ||
                Clipboard.ContainsData(TXTSCREEN_CB_FMT);

            // mnuDelete
            // no change

            // mnuClear
            mnuClear.Enabled = Selection.Width > 0;

            // mnuSelectAll
            // no changes

            // mnuCopyCommands
            mnuCopyCommands.Enabled = Selection.Width > 0;

            // mnuPasteCommands
            // no changes

            // mnuCharMap
            // no changes

            // mnuToggleTextMarks
            mnuToggleTextMarks.Text = (ShowTextMarks ? "Hide" : "Show") + "Text Marks";

            // mnuChangeScreenSize
            if (EditGame is null || !EditGame.PowerPack) {
                mnuChangeScreenSize.Visible = false;
            }
            else {
                mnuChangeScreenSize.Visible = true;
            }
        }

        private void ResetEditMenu() {
            mnuMode.Enabled = true;
            mnuCut.Enabled = true;
            mnuCopy.Enabled = true;
            mnuPaste.Enabled = true;
            mnuDelete.Enabled = true;
            mnuClear.Enabled = true;
            mnuSelectAll.Enabled = true;
            mnuCopyCommands.Enabled = true;
            mnuPasteCommands.Enabled = true;
            mnuCharMap.Enabled = true;
            mnuToggleTextMarks.Enabled = true;
            mnuChangeScreenSize.Enabled = true;
        }

        private void mnuMode_Click(object sender, EventArgs e) {
            // IMPORTANT!!!! 
            // The Insert Key CANNOT be assigned to any menu or
            // button on this form. It is used to toggle the 
            // Overwrite form, and is handled by the OnIdle method
            // of the main form (frmMDIMain). 
            // Using Insert as a Shortcut key will 


            // use the out-of-thread toggle function to simulate
            // a keypress of the INS button
            // !! this doesn't seem to be working anymore - use
            // SendKeys.Send instead
            //Task.Run(ToggleInsertKey);

            SendKeys.Send("{INSERT}");
        }

        private void mnuCut_Click(object sender, EventArgs e) {
            CutSelection();
        }

        private void mnuCopy_Click(object sender, EventArgs e) {
            CopySelection();
        }

        private void mnuPaste_Click(object sender, EventArgs e) {
            // check for custom clipboard data first
            TextscreenClipboardData txtdata = Clipboard.GetData(TXTSCREEN_CB_FMT) as TextscreenClipboardData;
            if (txtdata is not null) {
                // paste the new data
                PasteData(txtdata);
                return;
            }
            // try plain string
            string clipboardtext = Clipboard.GetText(TextDataFormat.UnicodeText);
            if (clipboardtext != String.Empty) {
                // paste it
                PasteText(clipboardtext);
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e) {
            DeleteText();
        }

        private void mnuClear_Click(object sender, EventArgs e) {
            ClearSelection();
        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {
            Selection = new();
            SetSelection(new Rectangle(0, 0, ScreenWidth, 25));
            MarginLeft = 0;
            picScreen.Invalidate();
        }

        private void mnuZoomIn_Click(object sender, EventArgs e) {
            ChangeScale(1);
        }

        private void mnuZoomOut_Click(object sender, EventArgs e) {
            ChangeScale(-1);
        }

        private void mnuCharMap_Click(object sender, EventArgs e) {
            ShowCharPicker();
        }

        private void mnuCopyCommands_Click(object sender, EventArgs e) {
            if (Selection.Width == 0) {
                // nothing to copy
                return;
            }
            CopyAsCommands(Selection);
        }

        private void mnuPasteCommands_Click(object sender, EventArgs e) {
            string clipboardtext = Clipboard.GetText(TextDataFormat.UnicodeText);
            if (clipboardtext != String.Empty) {
                // paste it
                PasteCommands(clipboardtext);
            }
        }

        private void mnuToggleTextMarks_Click(object sender, EventArgs e) {
            // toggle the flag
            ShowTextMarks = !ShowTextMarks;

            // refresh to update
            picScreen.Invalidate();
        }

        private void mnuChangeScreenSize_Click(object sender, EventArgs e) {
            //swap screen size if powerpack is active

            if (EditGame is null || !EditGame.PowerPack) {
                return;
            }
            // always confirm
            if (MessageBox.Show(MDIMain,
                "Change screen size to " + (120 - ScreenWidth) + " columns?",
                "Toggle Screen Size",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes) {
                ScreenWidth = 120 - ScreenWidth;
                CharWidth = 12 - CharWidth;
                TextChar[,] newarray = new TextChar[ScreenWidth, 25];
                // regardless of direction, only copy first 40 columns
                for (int i = 0; i < 40; i++) {
                    for (int j = 0; j < 25; j++) {
                        newarray[i, j] = TextData[i, j];
                    }
                }
                // if new array is wider, populate the default values
                if (ScreenWidth == 80) {
                    for (int i = 40; i < 80; i++) {
                        for (int j = 0; j < 25; j++) {
                            newarray[i, j].BG = DefBG;
                            newarray[i, j].FG = DefFG;
                        }
                    }
                }
                TextData = newarray;
                // reset anchor and selection
                Selection = new();
                SetSelection(Selection);
                MarginLeft = 0;
                DrawScreen();
            }
        }
        #endregion

        #region Control Events
        private void picPalette_MouseDown(object sender, MouseEventArgs e) {
            AGIColorIndex tmpBG = AGIColorIndex.None;
            AGIColorIndex tmpFG = AGIColorIndex.None;
            switch (ModifierKeys) {
            case Keys.None:
                // determine color from x,Y position
                int index = 8 * (e.Y / 17) + (e.X / (int)(picPalette.Width / 8f));
                switch (e.Button) {
                case MouseButtons.Left:
                    // background
                    tmpBG = (AGIColorIndex)index;
                    break;
                case MouseButtons.Right:
                    // foreground
                    tmpFG = (AGIColorIndex)index;
                    break;
                }
                if (Selection.Width == 0 && Selection.Height == 0) {
                    if (tmpBG != AGIColorIndex.None) {
                        DefBG = tmpBG;
                    }
                    else if (tmpFG != AGIColorIndex.None) {
                        DefFG = tmpFG;
                    }
                }
                else {
                    if (tmpBG != AGIColorIndex.None) {
                        TextBG = tmpBG;
                    }
                    else if (tmpFG != AGIColorIndex.None) {
                        TextFG = tmpFG;
                    }
                    // update selection to match color choice
                    ChangeColor(Selection, tmpBG, tmpFG);
                }
                picPalette.Invalidate();
                break;
            }
        }

        private void picPalette_Paint(object sender, PaintEventArgs e) {
            // draw the choice of colors into the palette box
            float dblWidth = picPalette.Width / 8;
            float dblHeight = picPalette.Height / 2;
            Graphics g = e.Graphics;

            // color area
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 8; j++) {
                    Color color = EditPalette[i * 8 + j];
                    g.FillRectangle(new SolidBrush(color), j * dblWidth, i * dblHeight, dblWidth, dblHeight);
                }
            }
            Color textcolor;
            Font font = new Font("Arial", 10, FontStyle.Bold);

            // if no selection, use default colors
            if (EditAction == TSAction.None && Selection.Width == 0) {
                // use default text colors
                if ((int)DefBG > 9) {
                    textcolor = Color.Black;
                }
                else {
                    textcolor = Color.White;
                }
                g.DrawString("B", font, new SolidBrush(textcolor), dblWidth * ((int)DefBG % 8) + 3, 17 * ((int)DefBG / 8));
                if ((int)DefFG > 9) {
                    textcolor = Color.Black;
                }
                else {
                    textcolor = Color.White;
                }
                g.DrawString("F", font, new SolidBrush(textcolor), dblWidth * (((int)DefFG % 8) + 1) - 13, 17 * ((int)DefFG / 8));
            }
            else {
                // use current text colors
                if (TextBG != AGIColorIndex.None) {
                    if ((int)TextBG > 9) {
                        textcolor = Color.Black;
                    }
                    else {
                        textcolor = Color.White;
                    }
                    g.DrawString("b", font, new SolidBrush(textcolor), dblWidth * ((int)TextBG % 8) + 3, 17 * ((int)TextBG / 8));
                }
                if (TextFG != AGIColorIndex.None) {
                    if ((int)TextFG > 9) {
                        textcolor = Color.Black;
                    }
                    else {
                        textcolor = Color.White;
                    }
                    g.DrawString("f", font, new SolidBrush(textcolor), dblWidth * (((int)TextFG % 8) + 1) - 13, 17 * ((int)TextFG / 8));
                }
            }
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e) {
            // de-select - this is only way to clear a 'select-all'
            // situation
            SetSelection(new Point());
        }

        private void panel1_Resize(object sender, EventArgs e) {
            SetScrollbars();
        }

        private void picScreen_KeyDown(object sender, KeyEventArgs e) {

            switch (e.Modifiers) {
            case Keys.Control:
                // nothing
                break;
            case Keys.None:
                // no shift, ctrl, alt

                // only in default mode
                if (EditAction == TSAction.None) {
                    switch (e.KeyCode) {
                    case Keys.Up or Keys.Left or Keys.Down or Keys.Right:
                        // if no selection, or only one char
                        if (Selection.Width <= 1 && Selection.Height <= 1) {
                            // move anchor
                            switch (e.KeyCode) {
                            case Keys.Up:
                                if (Selection.Y > 0) {
                                    Selection.Y--;
                                }
                                break;
                            case Keys.Left:
                                if (Selection.X > 0) {
                                    Selection.X--;
                                }
                                break;
                            case Keys.Down:
                                if (Selection.Y < 24) {
                                    Selection.Y++;
                                }
                                break;
                            case Keys.Right:
                                if (Selection.X < ScreenWidth - 1) {
                                    Selection.X++;
                                }
                                break;
                            }
                            SetSelection(Selection.Location);
                            picScreen.Invalidate();
                        }
                        break;
                    }
                }
                break;
            case Keys.Shift:
                if (Selection.Width == 0) {
                    return;
                }
                if (e.KeyCode == Keys.ShiftKey) {
                    // start move
                    EditAction = TSAction.Move;
                    StartMove();
                }
                else {
                    switch (EditAction) {
                    case TSAction.None:
                        break;
                    case TSAction.Move:
                        switch (e.KeyCode) {
                        case Keys.Up or Keys.Left or Keys.Down or Keys.Right:
                            // if something selected
                            Point oldLoc = Selection.Location;
                            Point newLoc = oldLoc;
                            // move, if there's room
                            switch (e.KeyCode) {
                            case Keys.Up:
                                if (Selection.Y > 0) {
                                    newLoc.Y--;
                                }
                                else {
                                    return;
                                }
                                break;
                            case Keys.Left:
                                if (Selection.X > 0) {
                                    newLoc.X--;
                                }
                                else {
                                    return;
                                }
                                break;
                            case Keys.Down:
                                if (Selection.Bottom < 25) {
                                    newLoc.Y++;
                                }
                                else {
                                    return;
                                }
                                break;
                            case Keys.Right:
                                if (Selection.Right < ScreenWidth) {
                                    newLoc.X++;
                                }
                                else {
                                    return;
                                }
                                break;
                            }
                            // adjust seletion
                            Selection.Location = newLoc;
                            // and finish move
                            MoveSel(oldLoc);
                            break;
                        }
                        break;
                    }
                    break;
                }
                break;
            }
        }

        private void picScreen_KeyUp(object sender, KeyEventArgs e) {

            // cancel movement if shift key goes up
            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.ShiftKey) {
                EditAction = TSAction.None;
            }
        }

        private void picScreen_KeyPress(object sender, KeyPressEventArgs e) {
            TextChar tmpChar = new() {
                BG = DefBG,
                FG = DefFG,
                CharVal = 32
            };

            switch ((int)e.KeyChar) {
            case >= 1 and <= 7:
            case 11:
            case 12:
            case >= 14 and <= 26:
            case >= 28 and <= 31:
                // control codes- ignore
                break;
            case 27:
                // ESC
                // cancel selection
                if (Selection.Width > 0) {
                    SetSelection(new Point());
                }
                break;
            case 8:
                // backspace
                // if a selection, same as clear
                if (Selection.Width > 0) {
                    ClearSelection();
                }
                else {
                    // can't backup if at beginning of row
                    if (Selection.X > 0) {
                        // delete previous char
                        TextData[Selection.X - 1, Selection.Y] = tmpChar;
                        if (!OverwriteMode) {
                            for (int i = Selection.X; i < ScreenWidth; i++) {
                                TextData[i - 1, Selection.Y] = TextData[i, Selection.Y];
                            }
                            TextData[ScreenWidth - 1, Selection.Y] = tmpChar;
                        }
                        // move anchor to left one space
                        Selection.X--;
                        DrawScreen(Selection.X, Selection.Y, !OverwriteMode ? ScreenWidth - Selection.X : 1, 1);
                        MarkAsChanged();
                    }
                }
                break;
            case 9:
                // tab
                // if a selection, clear it and advance
                if (Selection.Width > 0) {
                    if (!OverwriteMode) {
                        DeleteText(true);
                    }
                    else {
                        ClearSelection(true);
                    }
                    // add spaces equal to tabwidth
                    AddText(Selection.Location, "".PadRight(WinAGISettings.LogicTabWidth.Value), DefBG, DefFG);
                    MarkAsChanged();
                }
                else {
                    // insert spaces equal to logic tab width
                    AddText(Selection.Location, "".PadLeft(WinAGISettings.LogicTabWidth.Value), DefBG, DefFG);
                    MarkAsChanged();
                }
                break;
            case 10 or 13:
                // enter
                // if a selection, same as clear
                if (Selection.Width > 0) {
                    ClearSelection();
                    // collapse
                }
                else {
                    if (OverwriteMode) {
                        // just move down one
                        if (Selection.Y < 24) {
                            Selection.Y++;
                        }
                    }
                    else {
                        // move rows below current row down one
                        for (int j = 24; j > Selection.Y + 1; j--) {
                            for (int i = MarginLeft; i < ScreenWidth; i++) {
                                TextData[i, j] = TextData[i, j - 1];
                            }
                        }
                        // clear current line to end
                        for (int i = Selection.X; i < ScreenWidth; i++) {
                            TextData[i, Selection.Y] = tmpChar;
                        }
                        // move anchor to next line
                        if (Selection.Y < 24) {
                            Selection.Y++;
                        }
                        Selection.X = MarginLeft;
                        // redraw affected rows (including original row before the newline
                        DrawScreen(Selection.Left, Selection.Top - 1, ScreenWidth - Selection.Left, 25 - Selection.Top - 1);
                        MarkAsChanged();

                    }
                }
                break;
            default:
                // if one char selected, just replace it
                if (Selection.Height == 1 && Selection.Width == 1) {
                    byte[] chars = Encoding.GetEncoding(ScreenCodePage).GetBytes(e.KeyChar.ToString());
                    tmpChar.CharVal = chars[0];
                    TextData[Selection.X, Selection.Y] = tmpChar;
                    DrawScreen(Selection.X, Selection.Y, 1, 1);
                    if (Selection.X < ScreenWidth - 1) {
                        Selection.X++;
                    }
                }
                else {
                    // if a selection, clear it
                    if (Selection.Width > 1 || Selection.Height > 1) {
                        if (!OverwriteMode) {
                            DeleteText(true);
                        }
                        else {
                            ClearSelection(true);
                        }
                    }
                    // collapse selection
                    Selection.Size = new();
                    // if inserting, only move chars if no selection
                    if (!OverwriteMode && Selection.Width == 0) {
                        for (int i = ScreenWidth - 1; i > Selection.X; i--) {
                            TextData[i, Selection.Y] = TextData[i - 1, Selection.Y];
                        }
                    }
                    // add char at current position, using default colors
                    byte[] chars = Encoding.GetEncoding(ScreenCodePage).GetBytes(e.KeyChar.ToString());
                    tmpChar.CharVal = chars[0];
                    TextData[Selection.X, Selection.Y] = tmpChar;
                    Rectangle update = Selection;
                    update.Width = !OverwriteMode ? ScreenWidth - Selection.X : 1;
                    update.Height = 1;
                    Selection.X++;
                    // if at end of row, advance to next line
                    if (Selection.X == ScreenWidth) {
                        Selection.X = 0;
                        if (Selection.Y < 24) {
                            Selection.Y++;
                        }
                    }
                    // redraw affected area
                    DrawScreen(update);
                }
                MarkAsChanged();
                break;
            }
            e.Handled = true;
            picScreen.Invalidate();
        }

        private void picScreen_MouseDown(object sender, MouseEventArgs e) {
            // check for drag operation before anything else
            if (ModifierKeys == Keys.Shift) {
                // drag the picture
                //if (hsbScreen.Visible || vsbScreen.Visible) {
                if (e.Button == MouseButtons.Left) {
                    StartDrag(e.Location);
                }
                //}
                return;
            }

            // calculate AGI pixel position
            Point tmpPT = new((int)(e.X / (2 * ScaleFactor)), (int)(e.Y / ScaleFactor));

            // convert to col/row
            Point ScreenPT = new(tmpPT.X * 2 / CharWidth, tmpPT.Y / 8);

            switch (ModifierKeys) {
            case Keys.None:
                switch (EditAction) {
                case TSAction.None:
                    // if within selection bounds, begin movement
                    if (Selection.Contains(ScreenPT)) {
                        if (e.Button == MouseButtons.Left) {
                            // set offsets and begin moving selection
                            Offset.X = Selection.Left - ScreenPT.X;
                            Offset.Y = Selection.Top - ScreenPT.Y;
                            StartMove();
                        }
                    }
                    else {
                        // reposition caret
                        SetSelection(ScreenPT);
                        MarginLeft = Selection.X;
                        picPalette.Invalidate();
                    }
                    break;
                case TSAction.Drag:
                    StartDrag(e.Location);
                    break;
                }
                break;
            }
        }

        private void picScreen_MouseMove(object sender, MouseEventArgs e) {
            // check for drag before anything else
            if (EditAction == TSAction.Drag) {
                int newL = picScreen.Left + e.X - DragPT.X;
                if (hsbScreen.Visible) {
                    if (newL < -(hsbScreen.Maximum - hsbScreen.LargeChange + 1)) {
                        newL = -(hsbScreen.Maximum - hsbScreen.LargeChange + 1);
                    }
                    if (newL > 5) {
                        newL = 5;
                    }
                    picScreen.Left = newL;
                    hsbScreen.Value = -newL;
                }
                int newT = picScreen.Top + e.Y - DragPT.Y;
                if (vsbScreen.Visible) {
                    if (newT < -(vsbScreen.Maximum - vsbScreen.LargeChange + 1)) {
                        newT = -(vsbScreen.Maximum - vsbScreen.LargeChange + 1);
                    }
                    if (newT > 5) {
                        newT = 5;
                    }
                    picScreen.Top = newT;
                    vsbScreen.Value = -newT;
                }
                return;
            }

            // calculate AGI pixel position
            Point tmpPT = new((int)(e.X / (2 * ScaleFactor)), (int)(e.Y / ScaleFactor));

            // if no movement, do nothing
            if (tmpPT == mPT) {
                return;
            }
            // update cached coordinates
            mPT = tmpPT;
            // bound position
            if (tmpPT.X < 0) {
                tmpPT.X = 0;
            }
            else if (tmpPT.X > 159) {
                tmpPT.X = 159;
            }
            if (tmpPT.Y < 0) {
                tmpPT.Y = 0;
            }
            else if (tmpPT.Y > 199) {
                tmpPT.Y = 199;
            }
            // conveet to col/row
            Point ScreenPT = new(tmpPT.X * 2 / CharWidth, tmpPT.Y / 8);

            switch (e.Button) {
            case MouseButtons.None:
                // set cursor depending on what's under mouse
                if (Selection.Width > 0 && Selection.Contains(ScreenPT)) {
                    // show move cursor
                    SetCursor(TSCursor.Move);
                }
                else {
                    // show insert cursor
                    SetCursor(TSCursor.Insert);
                }
                break;
            case MouseButtons.Left:
                // action depends on tool
                switch (EditAction) {
                case TSAction.None:
                    if (Selection.Width > 0 && Selection.Contains(ScreenPT)) {
                        // over a selection, move it
                        EditAction = TSAction.Move;
                    }
                    else {
                        // not over selection, start selecting
                        SetCursor(TSCursor.Default);
                        //// assume over anchor
                        //if (Selection.Location == ScreenPT) {
                        AnchorPT = ScreenPT;
                        SetSelection(new Rectangle(AnchorPT, new(1, 1)));
                        EditAction = TSAction.Select;
                        //}
                        //else {
                        //    Debug.Print("not on anchor????");
                        //}
                    }
                    break;
                case TSAction.Select:
                    // if selecting, grow/shrink as appropriate
                    Rectangle rect = new();
                    if (AnchorPT.X < ScreenPT.X) {
                        rect.X = AnchorPT.X;
                    }
                    else {
                        rect.X = ScreenPT.X;
                    }
                    rect.Width = Math.Abs(AnchorPT.X - ScreenPT.X) + 1;
                    if (AnchorPT.Y < ScreenPT.Y) {
                        rect.Y = AnchorPT.Y;
                    }
                    else {
                        rect.Y = ScreenPT.Y;
                    }
                    rect.Height = Math.Abs(AnchorPT.Y - ScreenPT.Y) + 1;
                    if (rect != Selection) {
                        Selection = rect;
                        picScreen.Invalidate();
                    }
                    break;
                case TSAction.Move:
                    // limit movement so selection stays within screen
                    int tmpLeft = Offset.X + ScreenPT.X;
                    if (tmpLeft < 0) {
                        tmpLeft = 0;
                    }
                    if (tmpLeft + Selection.Width > ScreenWidth) {
                        tmpLeft = ScreenWidth - Selection.Width;
                    }
                    int tmpTop = Offset.Y + ScreenPT.Y;
                    if (tmpTop < 0) {
                        tmpTop = 0;
                    }
                    if (tmpTop + Selection.Height > 25) {
                        tmpTop = 25 - Selection.Height;
                    }
                    // did selection move?
                    if (tmpLeft != Selection.Left || tmpTop != Selection.Top) {
                        Point oldpos = Selection.Location;
                        Selection.X = tmpLeft;
                        Selection.Y = tmpTop;
                        MoveSel(oldpos);
                    }
                    break;
                }
                break;
            }
            spRow.Text = "R: " + ScreenPT.Y;
            spCol.Text = "C: " + ScreenPT.X;
        }

        private void picScreen_MouseUp(object sender, MouseEventArgs e) {
            if (EditAction == TSAction.Drag) {
                EditAction = TSAction.None;
                SetCursor(TSCursor.Default);
                return;
            }
            // calculate AGI pixel position
            Point tmpPT = new((int)(e.X / (2 * ScaleFactor)), (int)(e.Y / ScaleFactor));
            // bound position
            if (tmpPT.X < 0) {
                tmpPT.X = 0;
            }
            else if (tmpPT.X > 159) {
                tmpPT.X = 159;
            }
            if (tmpPT.Y < 0) {
                tmpPT.Y = 0;
            }
            else if (tmpPT.Y > 199) {
                tmpPT.Y = 199;
            }
            // convert to col/row
            Point ScreenPT = new(tmpPT.X * 2 / CharWidth, tmpPT.Y / 8);

            switch (EditAction) {
            case TSAction.None:
            case TSAction.Select:
                // if no selection
                if (Selection.Width == 0) {
                    TextBG = TextData[Selection.X, Selection.Y].BG;
                    TextFG = TextData[Selection.X, Selection.Y].FG;
                }
                // if only one char selected, update colors
                else if (Selection.Width == 1 && Selection.Height == 1) {
                    TextBG = TextData[Selection.X, Selection.Y].BG;
                    TextFG = TextData[Selection.X, Selection.Y].FG;
                }
                else {
                    // check for multi-colors
                    TextBG = TextData[Selection.X, Selection.Y].BG;
                    TextFG = TextData[Selection.X, Selection.Y].FG;
                    for (int i = Selection.Left; i < Selection.Right; i++) {
                        for (int j = Selection.Top; j < Selection.Bottom; j++) {
                            if (TextBG != TextData[i, j].BG) {
                                // multiple background colors
                                TextBG = AGIColorIndex.None;
                            }
                            if (TextFG != TextData[i, j].FG) {
                                // multiple foreground colors
                                TextFG = AGIColorIndex.None;
                            }
                            if (TextFG == AGIColorIndex.None && TextBG == AGIColorIndex.None) {
                                break;
                            }
                        }
                        if (TextFG == AGIColorIndex.None && TextBG == AGIColorIndex.None) {
                            break;
                        }
                    }
                }
                picPalette.Invalidate();
                EditAction = TSAction.None;
                break;
            case TSAction.Move:
                EditAction = TSAction.None;
                break;
            }
        }

        private void picScreen_MouseLeave(object sender, EventArgs e) {
            spRow.Text = "--";
            spCol.Text = "--";
        }

        private void picScreen_MouseWheel(object sender, MouseEventArgs e) {
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }
            Point screenPt = panel1.PointToClient(Cursor.Position);
            if (!panel1.ClientRectangle.Contains(screenPt)) {
                return;
            }
            ChangeScale(e.Delta, true);
        }

        private void picScreen_Paint(object sender, PaintEventArgs e) {

            // text marks indicate where text characters are drawn
            if (ShowTextMarks) {
                Pen marker = new(Color.FromArgb(170, 170, 0));
                for (int j = 1; j <= 25; j++) {
                    for (int i = 0; i < ScreenWidth; i++) {
                        int x = (int)(i * CharWidth * ScaleFactor);
                        int y = (int)(j * 8 * ScaleFactor - 1);
                        e.Graphics.DrawLine(marker, x, y, (int)(x + ScaleFactor), y);
                        e.Graphics.DrawLine(marker, x, y, x, (int)(y - ScaleFactor));
                    }
                }
            }

            // if selection is non-zero size, draw selection frame
            if (Selection.Width > 0) {
                Rectangle rgn = ColRowToScreen(Selection);
                //rgn.X += 1; rgn.Y +=1;
                rgn.Width -= 1; rgn.Height -= 1;
                e.Graphics.DrawRectangle(dash1, rgn);
                e.Graphics.DrawRectangle(dash2, rgn);
            }
            else {
                if (OverwriteMode) {
                    // display a flashing box
                    Color c = Color.FromArgb(160, EditPalette[(int)SelColor].R, EditPalette[(int)SelColor].G, EditPalette[(int)SelColor].B);
                    SolidBrush b = new SolidBrush(c);
                    Rectangle rect = new(ColRowToScreen(Selection.Location),
                        new((int)(CharWidth * ScaleFactor), (int)(8 * ScaleFactor)));
                    e.Graphics.FillRectangle(b, rect);
                }
                else {
                    // display a flashing insert line
                    Pen pen = new(EditPalette[(int)SelColor]);
                    Point start, end;
                    start = end = ColRowToScreen(Selection.Location);
                    end.Y += (int)(8 * ScaleFactor);
                    e.Graphics.DrawLine(pen, start, end);
                }
            }
        }

        private void tmrSelect_Tick(object sender, EventArgs e) {
            if (Selection.Width > 0) {
                // decrement for clockwise movement, increment for 
                // counterclockwise movement
                dashdistance -= 1;
                if (dashdistance == 0) dashdistance = 6;
                dash1.DashOffset = dashdistance;
                dash2.DashOffset = dashdistance - 3;

                // force refresh
                picScreen.Invalidate();
            }
        }

        private void tmrCursor_Tick(object sender, EventArgs e) {
            // toggle the cursor at current insertion point
            CursorOn = !CursorOn;
            SelColor++;
            if (SelColor == AGIColorIndex.None) {
                SelColor = AGIColorIndex.Black;
            }
            picScreen.Invalidate();
        }

        private void hsbScreen_Scroll(object sender, ScrollEventArgs e) {
            if (picScreen.Left != -hsbScreen.Value) {
                picScreen.Left = -hsbScreen.Value;
            }
        }

        private void hsbScreen_Enter(object sender, EventArgs e) {
            picScreen.Select();
        }

        private void vsbScreen_Scroll(object sender, ScrollEventArgs e) {
            if (picScreen.Top != -vsbScreen.Value) {
                picScreen.Top = -vsbScreen.Value;
            }
        }

        private void vsbScreen_Enter(object sender, EventArgs e) {
            picScreen.Select();
        }
        #endregion
        #endregion

        #region Methods
        private void InitStatusStrip() {
            spScale = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
            spMode = new ToolStripStatusLabel();
            spRow = new ToolStripStatusLabel();
            spCol = new ToolStripStatusLabel();
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;

            // 
            // spScale
            // 
            spScale.AutoSize = false;
            spScale.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spScale.BorderStyle = Border3DStyle.SunkenInner;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(70, 18);
            spScale.Text = "txt scale";
            // 
            // spMode
            // 
            spMode.AutoSize = false;
            spMode.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spMode.BorderStyle = Border3DStyle.SunkenInner;
            spMode.Name = "spMode";
            spMode.Size = new System.Drawing.Size(70, 18);
            spMode.Text = "txt mode";
            // 
            // spRow
            // 
            spRow.Name = "spRow";
            spRow.AutoSize = false;
            spRow.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spRow.BorderStyle = Border3DStyle.SunkenInner;
            spRow.Size = new System.Drawing.Size(80, 18);
            spRow.Text = "--";
            // 
            // spCol
            // 
            spCol.Name = "spCol";
            spCol.AutoSize = false;
            spCol.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spCol.BorderStyle = Border3DStyle.SunkenInner;
            spCol.Size = new System.Drawing.Size(80, 18);
            spCol.Text = "--";
        }

        private void InitForm() {
            hsbScreen.Minimum = -PE_MARGIN;
            vsbScreen.Minimum = -PE_MARGIN;
            // default scale uses PicEdit value
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
            picScreen.Height = (int)(200 * ScaleFactor);
            picScreen.Width = (int)(320 * ScaleFactor);
            picScreen.Image = new Bitmap((int)(320 * ScaleFactor), (int)(200 * ScaleFactor));
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";
            // text marks start enabled
            ShowTextMarks = true;
            // selection bar color
            SelColor = AGIColorIndex.Black;
            // default colors are white on black
            DefFG = AGIColorIndex.White;
            DefBG = AGIColorIndex.Black;
            SetSelection(new Point());
            // show insert cursor
            SetCursor(TSCursor.Insert);
            tmrCursor.Enabled = true;
            // begin in default (edit) mode
            EditAction = TSAction.None;
            // set columns and character widths
            if (EditGame is not null && EditGame.PowerPack) {
                ScreenWidth = 80;
                CharWidth = 4;
            }
            else {
                ScreenWidth = 40;
                CharWidth = 8;
            }
            TextData = new TextChar[ScreenWidth, 25];
            for (int i = 0; i < ScreenWidth; i++) {
                for (int j = 0; j < 25; j++) {
                    TextData[i, j].FG = DefFG;
                    TextData[i, j].BG = DefBG;
                }
            }
            OverwriteMode = IsKeyLocked(Keys.Insert);
            if (EditGame is not null) {
                // TODO: need to add palette update method in case
                // user changes palette
                EditPalette = EditGame.Palette;
            }
        }

        private Rectangle ColRowToScreen(Rectangle rect) {
            return new((int)(rect.X * CharWidth * ScaleFactor),
                    (int)(rect.Y * 8 * ScaleFactor), (int)(rect.Width * CharWidth * ScaleFactor),
                    (int)(Selection.Height * 8 * ScaleFactor));
        }

        private Point ColRowToScreen(Point pos) {
            return new((int)(pos.X * CharWidth * ScaleFactor),
                    (int)(pos.Y * 8 * ScaleFactor));
        }

        private RectangleF TargetCharPos(int col, int row) {
            return new(col * CharWidth * ScaleFactor, row * 8 * ScaleFactor, CharWidth * ScaleFactor, 8 * ScaleFactor);
        }

        private static RectangleF SourceCharPos(int CharVal) {
            return new(16 * (CharVal % 16), 16 * (CharVal / 16), 16, 16);
        }

        private void DrawScreen() {
            DrawScreen(0, 0, ScreenWidth, 25);
        }

        private void DrawScreen(Rectangle rect) {
            DrawScreen(rect.X, rect.Y, rect.Width, rect.Height);
        }

        private void DrawScreen(int left, int top, int width, int height) {
            // get the image from picScreen, but don't clear it
            Graphics g = Graphics.FromImage(picScreen.Image);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            for (int j = top; j < top + height; j++) {
                for (int i = left; i < left + width; i++) {
                    if (TextData[i, j].CharVal > 0 && TextData[i, j].CharVal != 32) {
                        // white on black and black on white are simple
                        if (TextData[i, j].BG == AGIColorIndex.Black && TextData[i, j].FG == AGIColorIndex.White) {
                            g.DrawImage(invchargrid,
                                TargetCharPos(i, j),
                                SourceCharPos(TextData[i, j].CharVal),
                                GraphicsUnit.Pixel);
                        }
                        else if (TextData[i, j].BG == AGIColorIndex.White && TextData[i, j].FG == AGIColorIndex.Black) {
                            g.DrawImage(chargrid,
                                TargetCharPos(i, j),
                                SourceCharPos(TextData[i, j].CharVal),
                                GraphicsUnit.Pixel);
                        }
                        else {
                            // create a new character bitmap with 
                            // correct colors (by changing palette)
                            Bitmap colorchar = chargrid.Clone(SourceCharPos(TextData[i, j].CharVal), chargrid.PixelFormat);
                            // Access the palette
                            ColorPalette palette = colorchar.Palette;
                            // Modify the two colors in the palette
                            palette.Entries[0] = EditPalette[(int)TextData[i, j].FG];
                            palette.Entries[1] = EditPalette[(int)TextData[i, j].BG];
                            // Apply the modified palette back to the bitmap
                            colorchar.Palette = palette;
                            // draw the colorized character
                            g.DrawImage(colorchar, TargetCharPos(i, j));
                        }
                    }
                    else {
                        // char zero or space means clear this postion to BG
                        Brush b = new SolidBrush(EditPalette[(int)TextData[i, j].BG]);
                        g.FillRectangle(b, i * CharWidth * ScaleFactor, j * 8 * ScaleFactor, ScaleFactor * CharWidth, ScaleFactor * 8);
                    }
                }
            }
            picScreen.Refresh();
        }

        private void CutSelection() {
            // copy
            CopySelection();
            // then delete
            DeleteText();
        }

        private void CopySelection() {
            if (Selection.Width == 0) {
                return;
            }
            // Copy the selection as text, and as TextChar data array.

            string copytext = "";
            TextscreenClipboardData copydata = new() {
                Data = new TextChar[Selection.Width, Selection.Height],
                Size = Selection.Size
            };
            for (int j = Selection.Top; j < Selection.Bottom; j++) {
                for (int i = Selection.Left; i < Selection.Right; i++) {
                    if (TextData[i, j].CharVal == 0) {
                        copytext += " ";
                    }
                    else {
                        if (TextData[i, j].CharVal < 128) {
                            copytext += (char)TextData[i, j].CharVal;
                        }
                        else {
                            // convert using correct codepage
                            byte[] CharVal = [TextData[i, j].CharVal];
                            copytext += Encoding.GetEncoding(ScreenCodePage).GetString(CharVal); ;
                        }
                    }
                    copydata.Data[i - Selection.X, j - Selection.Y] = TextData[i, j];
                }
            }
            // add text and custom data to clipboard
            DataObject dataObject = new();
            dataObject.SetData(DataFormats.UnicodeText, copytext);
            dataObject.SetData(TXTSCREEN_CB_FMT, copydata);
            Clipboard.Clear();
            Clipboard.SetDataObject(dataObject, true);
        }

        private void PasteData(TextscreenClipboardData textdata) {
            // pastes formatted textscreen data

            if (Selection.Width > 0) {
                // collapse selection
                Selection.Size = new();

                //// clear selection
                //if (OverwriteMode) {
                //    ClearSelection(true);
                //}
                //else {
                //    DeleteText(true);
                //}
            }
            // add the data at current anchor point
            for (int j = 0; j < textdata.Size.Height; j++) {
                // if it fits, add it
                if (Selection.Y + j < 25) {
                    if (!OverwriteMode) {
                        // move row over, respecting bounds
                        for (int i = ScreenWidth - 1 - Selection.X; i >= 0; i--) {
                            // if newval fits, move it
                            if (Selection.X + i + textdata.Size.Width < ScreenWidth) {
                                TextData[Selection.X + i + textdata.Size.Width, Selection.Y + j] = TextData[Selection.X + i, Selection.Y + j];
                            }
                        }
                    }
                    // now add new data
                    for (int i = 0; i < textdata.Size.Width; i++) {
                        if (Selection.X + i < ScreenWidth) {
                            TextData[Selection.X + i, Selection.Y + j] = textdata.Data[i, j];
                        }
                    }
                }
            }
            // adjust selection to pasted area
            Selection.Height = textdata.Size.Height;
            if (Selection.Bottom > 25) {
                Selection.Height = 25 - Selection.Top;
            }
            Selection.Width = textdata.Size.Width;
            if (Selection.Right > ScreenWidth) {
                Selection.Width = ScreenWidth - Selection.Left;
            }
            SetSelection(Selection);
            // redraw affected area
            Rectangle rect = Selection;
            if (!OverwriteMode) {
                // in insertmode it's all the way to end of row
                rect.Width = ScreenWidth - rect.Left;
            }
            DrawScreen(rect);
            MarkAsChanged();
        }

        private void PasteText(string pastetext) {
            // pastes plain text

            if (Selection.Width > 0) {
                // collapse current selection
                Selection.Size = new();

                //// clear selection
                //if (OverwriteMode) {
                //    ClearSelection(true);
                //}
                //else {
                //    DeleteText(true);
                //}
            }
            AddText(Selection.Location, pastetext, DefBG, DefFG);
            MarkAsChanged();
        }

        private void CopyAsCommands(Rectangle source) {
            // copy selected text to clipboard as agi commands

            int[] colCount = new int[16];
            int blackwhite = 0;
            List<string> output = new();

            // first check is to determine predominant background color
            for (int j = source.Top; j < source.Bottom; j++) {
                for (int i = source.Left; i < source.Right; i++) {
                    // if char is a null or 255 (non-printable) convert it to space
                    switch (TextData[i, j].CharVal) {
                    case 0 or 255:
                        TextData[i, j].CharVal = 32;
                        break;
                    }

                    // increment this background color count (track 
                    // black and white separately from individual colors -
                    // this is needed because PowerPack handles colors
                    // differently)
                    switch (TextData[i, j].BG) {
                    case 0:
                        blackwhite++;
                        break;
                    case (AGIColorIndex)15:
                        blackwhite--;
                        break;
                    }
                    colCount[(int)TextData[i, j].BG]++;
                }
            }

            // clearColor is used for the text.screen/clear.lines/clear.rect 
            // command and determines what the default background color for
            // all printed text will be
            AGIColorIndex clearColor = AGIColorIndex.None;

            // if entire screen is being copied, use text.screen cmd, and 
            // precede it with the domininant background color using 
            // set.text.attribute
            // if not entire screen, use clear.lines or clear.text.rect, BUT
            // keep in mind that on text screen color is ignored (it's always
            // black (unless using PowerPack)

            // approach depends on whether or not PowerPack is active:
            if (EditGame is not null && EditGame.PowerPack) {
                // use the predominant color, if there is one
                // assume none
                clearColor = AGIColorIndex.None;
                for (int i = 0; i < 16; i++) {
                    // if more than half are of one color, use it
                    if (colCount[i] > (source.Width * source.Height) / 2) {
                        clearColor = (AGIColorIndex)i;
                        break;
                    }
                }
            }
            else {
                // use the predominant black/white background
                if (blackwhite >= 0) {
                    clearColor = AGIColorIndex.Black;
                }
                else {
                    clearColor = AGIColorIndex.White;
                }
            }
            // add clear screen/area based on selection size
            if (source.Width == ScreenWidth && source.Height == 25) {
                // entire screen - set a color before calling 'text.screen'
                if (clearColor != AGIColorIndex.None) {
                    // foreground color is not needed, so just re-use background !15-BG?
                    output.Add("set.text.attribute(" + ColorText(clearColor) + ", " + ColorText(clearColor) + ");");
                }
                else {
                    // power pack with no predominant bg - don't set attributes
                }
                // then use text.screen command to clear entire screen
                output.Add("text.screen();");
            }
            else {
                // if no PowerPack, color is irrelevant, so set it to black
                if (EditGame is null || !EditGame.PowerPack && clearColor == AGIColorIndex.White) {
                    clearColor = AGIColorIndex.Black;
                }
                // if clear color is set
                if (clearColor != AGIColorIndex.None) {
                    // colors are not set yet, but clear the background
                    if (source.Width == ScreenWidth) {
                        output.Add("clear.lines(" + source.Top + ", " + (source.Bottom - 1) + ", " + ColorText(clearColor) + ");");
                    }
                    else {
                        output.Add("clear.text.rect(" + source.Top + ", " + source.Left + ", " + (source.Bottom - 1) + ", " + (source.Right - 1) + ", " + ColorText(clearColor) + ");");
                    }
                }
            }

            // track the current (new) color values at each point but...
            AGIColorIndex newFG;
            AGIColorIndex newBG;
            // colors are not set until a set.text.attribute command is added
            AGIColorIndex setFG = AGIColorIndex.None;
            AGIColorIndex setBG = AGIColorIndex.None;
            bool blnColorSet = false;
            bool blnSkip = false;

            // step through each line of output
            for (int j = source.Top; j < source.Bottom; j++) {
                newFG = TextData[source.Left, j].FG;
                newBG = TextData[source.Left, j].BG;
                if (newBG == setBG && newFG == setFG) {
                    blnColorSet = true;
                }
                else {
                    blnColorSet = false;
                }
                // reset display parameters
                int dRow = j;
                int dCol = source.Left;
                string strLine = "";
                int i = source.Left;
                // begin search by skipping over spaces that match clear color
                while (i < source.Right) {
                    if (newBG == clearColor) {
                        while (i < source.Right) {
                            // check for bg change, or non-space char
                            if (TextData[i, j].BG != newBG || TextData[i, j].CharVal != 32) {
                                // non-matching blank found; need to start a 
                                // new string search
                                dRow = j;
                                dCol = i;
                                strLine = "";
                                break;
                            }
                            // at least one space is skipped
                            // (means the clear.text.rect/clear.line cmd is needed
                            // and can't be removed)
                            blnSkip = true;
                            i++;
                        }
                    }
                    // build output, until end of row or until attributes change
                    while (i < source.Right) {
                        // check for color change first
                        if (TextData[i, j].BG != newBG || TextData[i, j].FG != newFG) {
                            // if background is changed OR if char is NOT a space, 
                            // update colors (if needed) add this string, then continue
                            if (TextData[i, j].BG != newBG || TextData[i, j].CharVal != 32) {
                                if (newBG == clearColor) {
                                    // if color matches clearcolor, trailing spaces not needed
                                    strLine = strLine.TrimEnd();
                                }
                                if (strLine.Length > 0) {
                                    // if colors aren't set yet
                                    if (!blnColorSet) {
                                        output.Add("set.text.attribute(" + ColorText(newFG) + ", " + ColorText(newBG) + ");");
                                        setFG = newFG;
                                        setBG = newBG;
                                        blnColorSet = true;
                                    }
                                    // add this line
                                    output.Add("display(" + dRow + ", " + dCol + ", " + "\"" + AddCodes(strLine) + "\"" + ");");
                                }
                                // update colors
                                newFG = TextData[i, j].FG;
                                newBG = TextData[i, j].BG;
                                if (newBG == setBG && newFG == setFG) {
                                    // no need to set colors again
                                    blnColorSet = true;
                                }
                                else {
                                    blnColorSet = false;
                                }
                                // always start a new search if colors change
                                dRow = j;
                                dCol = i;
                                strLine = "";
                                // start a new line
                                break;
                            }
                        }
                        // add this char
                        strLine += new string(Encoding.GetEncoding(ScreenCodePage).GetChars([TextData[i, j].CharVal]));
                        // if bg is clear color, look for 5 or more blank spaces
                        if (newBG == clearColor) {
                            if (strLine.Right(5) == "     ") {
                                // instead of one long string with a lot of blank space
                                // use separate, smaller strings (this only works when
                                // the background color matches the clear color)
                                // add this line, trimmed
                                strLine = strLine.TrimEnd();
                                // at least one space was skipped
                                blnSkip = true;
                                if (strLine.Length > 0) {
                                    if (!blnColorSet) {
                                        output.Add("set.text.attribute(" + ColorText(newFG) + ", " + ColorText(newBG) + ");");
                                        setFG = newFG;
                                        setBG = newBG;
                                        blnColorSet = true;
                                    }
                                    output.Add("display(" + dRow + ", " + dCol + ", " + "\"" + AddCodes(strLine) + "\"" + ");");
                                }
                                // move to next col
                                i++;
                                // begin another search
                                dRow = i;
                                dCol = j;
                                strLine = "";
                                break;
                            }
                        }
                        // move to next character
                        i++;
                    }
                }
                // at right edge; add the line, if one was found
                if (strLine.Length > 0) {
                    if (newBG == clearColor) {
                        // if there are blank spaces at the end, remove them if
                        // the line's color matches the cleared color
                        if (strLine[^1] == ' ') {
                            // trim right edge
                            strLine = strLine.TrimEnd();
                            // (also st the skipped flag since one or
                            // more spaces aren't drawn)
                            blnSkip = true;
                            Debug.Assert(strLine.Length > 0);
                        }
                    }
                    // add this line
                    if (!blnColorSet) {
                        output.Add("set.text.attribute(" + ColorText(newFG) + ", " + ColorText(newBG) + ");");
                        setBG = newBG;
                        setFG = newFG;
                        blnColorSet = true;
                    }
                    output.Add("display(" + dRow + ", " + dCol + ", " + "\"" + AddCodes(strLine) + "\"" + ");");
                }
            }

            // if entire area is covered (no spaces skipped)
            // clearing the area isn't necessary
            if (Selection.Width > 0 && (Selection.Height != 25 || Selection.Width != ScreenWidth)) {
                if (!blnSkip) {
                    if (output.Count > 1) {
                        output.RemoveAt(0);
                    }
                }
            }

            // consolidate colors
            CompactOutput(ref output);

            // add blank line to add trailing crlf
            output.Add("");

            // copy output to clipboard
            Clipboard.Clear();
            Clipboard.SetText(output.Text(Environment.NewLine), TextDataFormat.UnicodeText);
            // provide feedback
            MessageBox.Show(MDIMain,
                "Commands copied to clipboard.",
                "Text Screen Editor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private string AddCodes(string strIn) {
            // this function creates the correct source code message text needed
            // to get strIn to be displayed with 'display' or 'print' command

            // quotes(") with \"
            // percent(%) with \\%
            // backslash(\) with \\\\
            // chars <32 with \x## (hex)
            string retval = "";
            for (int i = 0; i < strIn.Length; i++) {
                char charval = strIn[i];
                switch (charval) {
                case < (char)32:
                    // non-printables use '\x##'
                    retval += "\\x" + ((int)charval).ToString("X2");
                    break;
                case '\\':
                    // '\' uses '\\\\' 
                    // (compiler converts '\\' to '\' so four
                    // slashes become two in the AGI message text
                    // AGI print function conerts '\\' to a
                    // single '\'
                    retval += "\\\\\\\\";
                    break;
                case '%':
                    // '%' uses '\\%'
                    retval += "\\\\%";
                    break;
                default:
                    retval += charval;
                    break;
                }
            }
            return retval;
        }

        private string ColorText(AGIColorIndex index) {
            if (EditGame is null) {
                return ((int)index).ToString();
            }
            else {
                return EditGame.ReservedDefines.ColorNames[(int)index].Name;
            }
        }

        private void CompactOutput(ref List<string> TextIn) {
            // skip text.screen/clear lines
            // find next set.text.attribute
            // find end of display lines
            // then go through rest of lines; look for same color
            // if found, delete the extra attr line,
            // move all display lines up
            //  stop at end
            // repeat

            if (TextIn.Count < 4) {
                return;
            }
            int i = 0, sPos, iPos, mPos;

            while (i < TextIn.Count) {
                // is this a 'set.text.attribute' line?
                if (TextIn[i][0] == 's') {
                    // found a new color line
                    sPos = i;
                    // find the next
                    i++;
                    while (i < TextIn.Count) {
                        if (TextIn[i][0] == 's') {
                            // this is insert location
                            iPos = i;
                            // now continue, looking for matching colors
                            while (i < TextIn.Count) {
                                if (TextIn[sPos] == TextIn[i]) {
                                    // this is matching pos
                                    mPos = i;
                                    // delete this line
                                    TextIn.RemoveAt(mPos);
                                    while (mPos < TextIn.Count) {
                                        // if next line is a display, move it up to iPos
                                        if (TextIn[mPos][0] == 'd') {
                                            // if iPos and mPos are the same, don't need to move the line
                                            if (iPos != mPos) {
                                                // move display lines up to iPos
                                                string strTemp = TextIn[mPos];
                                                TextIn.RemoveAt(mPos);
                                                TextIn.Insert(iPos, strTemp);
                                            }
                                            // move to next line
                                            mPos++;
                                            iPos++;
                                        }
                                        else {
                                            // quit when next attribute line found; move main counter
                                            i = mPos;
                                            break;
                                        }
                                    }
                                }
                                else {
                                    i++;
                                }
                            }
                            // done consolidating this attribute; reset main counter
                            // to insert pos
                            i = iPos;
                            break;
                        }
                        else {
                            // keep looking
                            i++;
                        }
                    }
                }
                else {
                    // move to next line
                    i++;
                }
            }
        }

        private void PasteCommands(string pastetext) {
            // parse and paste clipboard text as agi commands
            // ignore everything except set.text.attribute/display
            // ignore messages if not passed by string

            AGIColorIndex lngFG = DefFG;
            AGIColorIndex lngBG = DefBG;

            // if current selection, collapse it
            if (Selection.Width > 0) {
                Selection.Width = 0;
            }

            List<string> stlText = new();
            stlText.AddLines(pastetext);

            foreach (string textline in stlText) {
                // remove any tab characters and preceding spaces
                string line = textline.Replace('\t', ' ').Trim();
                if (line.Length == 0) {
                    continue;
                }
                // check for text attributes
                if (line.Left(18) == "set.text.attribute") {
                    line = line[18..].Trim();
                    if (line[0] == '(') {
                        line = line[1..].Trim();
                        int lngPos = line.LastIndexOf(')');
                        if (lngPos >= 0) {
                            line = line[..lngPos].Trim();
                            string[] strArgs = line.Split(",");
                            if (strArgs.Length == 2) {
                                AGIColorIndex tmpFG = AGIColorIndex.None;
                                AGIColorIndex tmpBG = AGIColorIndex.None;
                                for (int j = 0; j < 2; j++) {
                                    if (strArgs[j].IsInt()) {
                                        int tmpColor = strArgs[j].IntVal();
                                        if (tmpColor >= 0 && tmpColor <= 15) {
                                            if (j == 0) {
                                                tmpFG = (AGIColorIndex)tmpColor;
                                            }
                                            else {
                                                tmpBG = (AGIColorIndex)tmpColor;
                                            }
                                        }
                                    }
                                    else {
                                        int tmp = -1;
                                        if (EditGame is not null) {
                                            if (EditGame.IncludeReserved) {
                                                // look for matching color name
                                                strArgs[j] = strArgs[j].Trim();
                                                for (int index = 0; index < 16; index++) {
                                                    if (strArgs[j] == EditGame.ReservedDefines.ColorNames[index].Name) {
                                                        tmp = index;
                                                        break;
                                                    }
                                                }
                                                if (tmp == -1) {
                                                    break; // Exit For
                                                }
                                                if (j == 0) {
                                                    tmpFG = (AGIColorIndex)tmp;
                                                }
                                                else {
                                                    tmpBG = (AGIColorIndex)tmp;
                                                }
                                            }
                                        }
                                        else {
                                            // no game- default res?
                                        }
                                    }
                                }
                                if (tmpFG != AGIColorIndex.None && tmpBG != AGIColorIndex.None) {
                                    // use them
                                    lngFG = tmpFG;
                                    lngBG = tmpBG;
                                }
                            }
                        }
                    }
                }
                else if (line.Left(7) == "display") {
                    line = line[7..].Trim();
                    if (line[0] == '(') {
                        line = line[1..].Trim();
                        int lngPos = line.LastIndexOf(')');
                        if (lngPos >= 0) {
                            line = line[..lngPos].Trim();
                            // should have #, #, "msg"
                            string[] strArgs = line.Split(',');
                            if (strArgs.Length == 3) {
                                int tmpRow = -1;
                                int tmpCol = -1;
                                for (int j = 0; j < 2; j++) {
                                    if (strArgs[j].IsInt()) {
                                        int tmp = strArgs[j].IntVal();
                                        if (j == 0) {
                                            if (tmp >= 0 && tmp <= 24) {
                                                tmpRow = tmp;
                                            }
                                        }
                                        else {
                                            if (tmp >= 0 && tmp <= ScreenWidth - 1) {
                                                tmpCol = tmp;
                                            }
                                        }
                                    }
                                }
                                if (tmpRow != -1 && tmpCol != -1) {
                                    // get msg
                                    strArgs[2] = strArgs[2].Trim();
                                    if (strArgs[2].Length > 2 && strArgs[2][0] == '\"' && strArgs[2][^1] == '\"') {
                                        strArgs[2] = AGIFormat(CompilerFormat(strArgs[2][1..^1]));
                                        byte[] chars = Encoding.GetEncoding(ScreenCodePage).GetBytes(strArgs[2]);
                                        TextChar tmp = new() {
                                            FG = lngFG,
                                            BG = lngBG
                                        };
                                        for (int i = 0; i < chars.Length; i++) {
                                            tmp.CharVal = chars[i];
                                            TextData[tmpCol, tmpRow] = tmp;
                                            tmpCol++;
                                            if (tmpCol == ScreenWidth) {
                                                tmpCol = 0;
                                                tmpRow++;
                                                if (tmpRow == 25) {
                                                    break;
                                                }
                                            }
                                        }
                                        MarkAsChanged();
                                    }
                                }
                            }
                        }
                    }
                }
                else if (line.Left(11) == "text.screen") {
                    // reset entire screen to blank
                    TextChar tmp = new();
                    tmp.FG = lngFG;
                    tmp.BG = lngBG;
                    tmp.CharVal = 32;
                    for (int j = 0; j < 25; j++) {
                        for (int i = 0; i < ScreenWidth; i++) {
                            TextData[i, j] = tmp;
                        }
                    }
                    MarkAsChanged();
                }
            }
            // redraw the screen
            DrawScreen();
        }

        private void AddText(Point insertpos, string strText, AGIColorIndex backcolor, AGIColorIndex forecolor) {
            // adds text at current location
            // calling function responsible for clearing any selection
            // before adding text; selection parameters are still set

            // convert crlf to just cr so lines only advance once
            strText = strText.Replace("\r\n", "\r");

            // track starting column for adding multilines
            // this is currently the default; it seems to work
            // but maybe I could add option to wrap to column 0 instead
            int startCol = insertpos.X;
            /*
            the region to be cleared: at LEAST clear the 
            original selection; BUT if nothing selected, bottom
            needs to be adjusted by one so the current row gets
            selected

            if in INS mode, the width needs to be expanded to
            right edge
            if in OVR mode, width is only expanded if any 
            characters are added beyond the original selection
            in height or width
            */


            Rectangle updateregion = Selection;
            Point max = Selection.Location;
            max.X = Selection.Right;
            max.Y = Selection.Bottom;
            // if nothing selected, expand by one character
            if (Selection.Width == 0) {
                max.X++;
                max.Y++;
            }
            if (!OverwriteMode) {
                max.X = ScreenWidth;
            }
            // draw each char, one at a time
            byte[] textchars = Encoding.GetEncoding(ScreenCodePage).GetBytes(strText);
            for (int i = 0; i < textchars.Length; i++) {
                if (insertpos.Y == 25) {
                    break;
                }
                byte intChar = textchars[i];
                switch (intChar) {
                case 8:
                    // backspace
                    // bckup 1 space
                    insertpos.X--;
                    break;
                case 9:
                    //  tab
                    // add spaces equal to tabwidth
                    if (!OverwriteMode) {
                        // advance line by one
                        for (int j = ScreenWidth - 1; j > insertpos.X; i--) {
                            TextData[i, insertpos.Y] = TextData[i - WinAGISettings.LogicTabWidth.Value, insertpos.Y];
                        }
                    }

                    for (int j = 1; j < WinAGISettings.LogicTabWidth.Value; j++) {
                        if (insertpos.X >= ScreenWidth) {
                            break;
                        }
                        if (!OverwriteMode) {
                            // advance line by one
                            AdvanceLine(insertpos);
                        }
                        TextData[insertpos.X, insertpos.Y].BG = backcolor;
                        TextData[insertpos.X, insertpos.Y].FG = forecolor;
                        TextData[insertpos.X, insertpos.Y].CharVal = 32;
                    }
                    break;
                case 10:
                case 13:
                    // line feed or cr
                    insertpos.X = startCol - 1;
                    insertpos.Y++;
                    if (insertpos.Y < 25) {
                        max.Y = Math.Max(max.Y, insertpos.Y + 1);
                    }
                    break;
                default:
                    if (i > 0) {
                        // advance cursor and adjust update region
                        insertpos.X++;
                        if (insertpos.X == ScreenWidth) {
                            // overflow to start of next line
                            // (unless already at bottom)
                            if (insertpos.Y == 24) {
                                insertpos.X = ScreenWidth - 1;
                                break;
                            }
                            insertpos.Y++;
                            max.Y = Math.Max(max.Y, insertpos.Y + 1);
                            // moving to left means update region X val must be
                            // zero while right edge remains the same
                            updateregion.Width = updateregion.Right;
                            updateregion.X = 0;
                            insertpos.X = 0;
                        }
                        max.X = Math.Max(max.X, insertpos.X + 1);
                    }
                    if (!OverwriteMode) {
                        // advance line by one
                        AdvanceLine(insertpos);
                    }
                    TextData[insertpos.X, insertpos.Y].CharVal = intChar;
                    TextData[insertpos.X, insertpos.Y].BG = backcolor;
                    TextData[insertpos.X, insertpos.Y].FG = forecolor;
                    break;
                }
            }
            // redraw affected area
            updateregion = updateregion.Expand(max);
            DrawScreen(updateregion);
            // reposition cursor at end of added text
            insertpos.X++;
            if (insertpos.X == ScreenWidth) {
                if (insertpos.Y < 24) {
                    insertpos.Y++;
                }
                insertpos.X = 0;
            }
            SetSelection(insertpos);
        }

        private void AdvanceLine(Point pos) {
            // moves data in line aRow over one char, starting at aCol
            // chars extending past right edge get dropped

            for (int i = ScreenWidth - 1; i > pos.X; i--) {
                TextData[i, pos.Y] = TextData[i - 1, pos.Y];
            }
        }

        private void ShowCharPicker() {
            // show the char picker, and insert results
            frmCharPicker CharPicker;
            if (EditGame is not null) {
                CharPicker = new(EditGame.CodePage);
            }
            else {
                CharPicker = new(WinAGISettings.DefCP.Value);
            }
            CharPicker.ShowDialog(MDIMain);
            if (!CharPicker.Cancel) {
                if (CharPicker.InsertString.Length > 0) {
                    if (Selection.Width > 0) {
                        // collapse selection
                        Selection.Size = new();

                        //// clear selection
                        //if (OverwriteMode) {
                        //    ClearSelection(true);
                        //}
                        //else {
                        //    DeleteText(true);
                        //}
                    }
                    // if inserted text contains any control codes (\x##, \n, etc)
                    // replace them with correct symbol
                    AddText(Selection.Location, CharPickFormat(CharPicker.InsertString), DefBG, DefFG);
                }
                MarkAsChanged();
            }
            CharPicker.Dispose();
            CharPicker = null;
        }

        private string CharPickFormat(string sInput) {
            // the char picker uses '\x##' codes for non-printable characters
            // so they can be handled by the various WinAGI editors. In AGI,
            // these characters will actually print if used on the text screen
            // so they need to inserted in place of the '\x##' code.
            //
            // Also, backspace (char value '8') is not allowed by the
            // WinAGI editors, as it interfere with AGI's print methods.
            // It gets replaced with single space.

            string retval = "";
            bool skipchar = false;
            // step through all characters in this string
            for (int i = 0; i < sInput.Length; i++) {
                // get ascii code for this character
                int charval = (int)sInput[i];
                switch (charval) {
                case 92:
                    // '\'
                    // check for special codes
                    if (i < sInput.Length - 1) {
                        switch ((int)sInput[i + 1]) {
                        case 110:
                            // '\n' = new line
                            charval = 0x0A;
                            i++;
                            break;
                        case 120:
                            // '\x' = possible hex value
                            // make sure at least two more characters
                            if (i < sInput.Length - 3) {
                                // get next 2 chars and hexify them
                                string strHex = sInput[(i + 2)..(i + 4)] as string;
                                if (int.TryParse(strHex, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexval)) {
                                    if (hexval >= 1 && hexval < 256) {
                                        switch (hexval) {
                                        case 8:
                                            // invalid code, convert to space
                                            charval = 32;
                                            break;
                                        default:
                                            charval = hexval;
                                            break;
                                        }
                                        i += 3;
                                    }
                                }
                            }
                            break;
                        default:
                            // no special char found, the single slash should be dropped
                            skipchar = true;
                            break;
                        }
                    }
                    else {
                        // if the '\' is the last char, skip it
                        skipchar = true;
                    }
                    break;
                }
                if (skipchar) {
                    skipchar = false;
                }
                else {
                    retval += (char)charval;
                }
            }
            return retval;
        }

        private string CompilerFormat(string sInput) {
            // Compiler formatting codes:
            //    '\"' = '"' (single quote)
            //    '\n' and '\r' = newline (0x0A)
            //    '\x##' = char 0x##
            //    '\\' = '\'
            //    '\x' = 'x', x = any character
            //
            // compiler also does not allow backspace character '8'
            // it gets replaced with space ' '

            // replace any 'crlf' with single newline
            sInput = sInput.Replace("\r\n", "\n");
            string retval = "";
            bool skipchar = false;
            // step through all characters in this string
            for (int i = 0; i < sInput.Length; i++) {
                // get ascii code for this character
                int charval = (int)sInput[i];
                switch (charval) {
                case 8:
                    // invalid code, convert it to space
                    charval = 32;
                    break;
                case 13:
                    // newline; only 0x0A is allowed
                    charval = 0x0A;
                    break;
                case 92:
                    // '\'
                    // check for special codes
                    if (i < sInput.Length - 1) {
                        switch ((int)sInput[i + 1]) {
                        case 110:
                            // '\n' = new line
                            charval = 0x0A;
                            i++;
                            break;
                        case 120:
                            // '\x' = possible hex value
                            // make sure at least two more characters
                            if (i < sInput.Length - 3) {
                                // get next 2 chars and hexify them
                                string strHex = sInput[(i + 2)..(i + 4)] as string;
                                if (int.TryParse(strHex, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexval)) {
                                    if (hexval >= 1 && hexval < 256) {
                                        charval = hexval;
                                        i += 3;
                                    }
                                }
                            }
                            break;
                        default:
                            // no special char found, the single slash should be dropped
                            // this catches '\"', '\\' as well as all others
                            skipchar = true;
                            break;
                        }
                    }
                    else {
                        // if the '\' is the last char, skip it
                        skipchar = true;
                    }
                    break;
                }
                if (skipchar) {
                    skipchar = false;
                }
                else {
                    retval += (char)charval;
                }
            }
            return retval;
        }

        private string AGIFormat(string sInput) {
            // AGI print formatting codes:
            // '\x' = 'x' and do not process it if x is '%'
            //        handled)
            // '%g#', '%m#', '%o#', '%s#', '%v#', '%w#' become 
            //        'g#', 'm#', 'o#', 's#', 'v#', 'w#'
            //        where # = zero or more numeric characters
            //        (in AGI, this code is then replaced with the corresponding
            //        value, but in this function we will leave the format code
            //        in place, since we don't have the context required to
            //        replace them)
            // '%x' = '' (x =any other character) both the % and the following
            //        character are skipped

            // replace any 'crlf' with single newline
            sInput = sInput.Replace("\r\n", "\n");
            string retval = "";
            bool skipchar = false;
            // step through all characters in this string
            for (int i = 0; i < sInput.Length; i++) {
                // get ascii code for this character
                int charval = (int)sInput[i];
                switch (charval) {
                case 13:
                    // newline char is always '\n' (0xA) in AGI
                    charval = 0x0A;
                    break;
                case 92:
                    // '\'
                    // check for special codes
                    if (i < sInput.Length - 1) {
                        // drop the slash, add next char
                        i++;
                        charval = (int)sInput[i + 1];
                    }
                    else {
                        // if the '\' is the last char, skip it
                        skipchar = true;
                    }
                    break;
                case 37:
                    // '%'
                    if (i < sInput.Length - 1) {
                        // check for formatting codes
                        switch (sInput[i + 1]) {
                        case 'g' or 'm' or 'o' or 's' or 'v' or 'w':
                            // keep the following letter, but drop 
                            // the '%'
                            skipchar = true;
                            break;
                        default:
                            // drop the '%' and the follow on character
                            i++;
                            skipchar = true;
                            break;
                        }
                    }
                    else {
                        // if the '%' is the last char, skip it
                        skipchar = true;
                    }
                    break;
                }
                if (skipchar) {
                    skipchar = false;
                }
                else {
                    retval += (char)charval;
                }
            }
            return retval;
        }
        /// <summary>
        /// Deletes the text in the current Selection, if there 
        /// is one, otherwise deletes the character at the current
        /// cursor position. If NoDraw is true, the screen is not
        /// immediately redrawn (useful when inserting characters). 
        /// </summary>
        /// <param name="NoDraw"></param>

        private void DeleteText(bool NoDraw = false) {
            // if in overwrite mode, same as clear
            // otherwise characters to right are moved over
            // to fill the hole

            if (OverwriteMode) {
                // in overwrite mode, just delete the current selection
                int i = Selection.Width;
                if (Selection.Width == 0) {
                    // delete one char
                    Selection.Width = 1;
                    Selection.Height = 1;
                }
                ClearSelection(NoDraw);
                // if no selection, also move cursor over one
                if (i == 0) {
                    Selection.Width = 0;
                    Selection.Height = 0;
                    if (!NoDraw) {
                        // move caret
                        Selection.X++;
                        if (Selection.X == ScreenWidth) {
                            Selection.X = ScreenWidth - 1;
                        }
                        Selection.X = Selection.X;
                        picScreen.Invalidate();
                    }
                }
            }
            else {
                // insert mode
                if (Selection.Width == 0) {
                    // delete one char
                    Selection.Height = 1;
                    Selection.Width = 1;
                }
                TextChar tmpChar = new TextChar();
                tmpChar.BG = DefBG;
                tmpChar.FG = DefFG;
                tmpChar.CharVal = 32;
                for (int j = Selection.Top; j < Selection.Bottom; j++) {
                    // move data over
                    for (int i = Selection.Left; i < ScreenWidth - Selection.Width; i++) {
                        TextData[i, j] = TextData[i + Selection.Width, j];
                    }
                    // clear end of line
                    for (int i = ScreenWidth - Selection.Width; i < ScreenWidth; i++) {
                        TextData[i, j] = tmpChar;
                    }
                }
                if (!NoDraw) {
                    int height = Selection.Height;
                    // collapse selection
                    Selection.Width = 0;
                    Selection.Height = 0;
                    tmrCursor.Enabled = true;
                    tmrSelect.Enabled = false;
                    // redraw the screen
                    DrawScreen(Selection.Left, Selection.Top, ScreenWidth - Selection.Left, height);
                }
            }
            MarkAsChanged();
        }

        private void ClearSelection(bool NoDraw = false) {
            if (Selection.Width == 0) {
                return;
            }
            TextChar tmpChar = new();
            tmpChar.BG = DefBG;
            tmpChar.FG = DefFG;
            tmpChar.CharVal = 32;
            for (int j = Selection.Top; j < Selection.Bottom; j++) {
                for (int i = Selection.Left; i < Selection.Right; i++) {
                    TextData[i, j] = tmpChar;
                }
            }
            MarkAsChanged();
            if (!NoDraw) {
                DrawScreen(Selection);
            }
        }

        private void SetSelection(Point pos) {
            Selection = new(pos, new());
            tmrSelect.Enabled = false;
            tmrCursor.Enabled = true;
            UpdateToolbarButtons();
            picPalette.Invalidate();
        }

        private void SetSelection(Rectangle rect) {
            Selection = rect;
            tmrSelect.Enabled = true;
            tmrCursor.Enabled = false;
            UpdateToolbarButtons();
            picPalette.Invalidate();
        }

        private void StartMove() {
            EditAction = TSAction.Move;

            SelData = new TextChar[Selection.Width, Selection.Height];
            TempData = new TextChar[Selection.Width, Selection.Height];

            TextChar tmpTextData = new();
            tmpTextData.BG = DefBG;
            tmpTextData.FG = DefFG;
            tmpTextData.CharVal = 32;

            for (int i = 0; i < Selection.Width; i++) {
                for (int j = 0; j < Selection.Height; j++) {
                    // copy screen data into sel array
                    SelData[i, j] = TextData[Selection.Left + i, Selection.Top + j];
                    // clear the temp array screen data at original location
                    TempData[i, j] = tmpTextData;
                }
            }
        }

        private void MoveSel(Point oldpos) {
            // moves selection to updated position
            for (int i = 0; i < Selection.Width; i++) {
                for (int j = 0; j < Selection.Height; j++) {
                    //restore data at previous location
                    TextData[oldpos.X + i, oldpos.Y + j] = TempData[i, j];
                }
            }
            for (int i = 0; i < Selection.Width; i++) {
                for (int j = 0; j < Selection.Height; j++) {
                    // make temporary copy of data at updated location
                    TempData[i, j] = TextData[Selection.X + i, Selection.Y + j];
                    // store selection data at upated location
                    TextData[Selection.X + i, Selection.Y + j] = SelData[i, j];
                }
            }
            // redraw old location
            DrawScreen(oldpos.X, oldpos.Y, Selection.Width, Selection.Height);
            // redraw current selection
            DrawScreen(Selection);
            MarkAsChanged();
        }

        /// <summary>
        /// Sets the cursor for the text screen based on current mode and tool.
        /// </summary>
        /// <param name="NewCursor"></param>
        private void SetCursor(TSCursor NewCursor) {
            MemoryStream msCursor;

            if (CurCursor == NewCursor) {
                return;
            }
            CurCursor = NewCursor;
            switch (NewCursor) {
            case TSCursor.Default:
                msCursor = new(EditorResources.EPC_EDIT);
                picScreen.Cursor = new Cursor(msCursor);
                break;
            case TSCursor.Move:
                picScreen.Cursor = Cursors.SizeAll;
                break;
            case TSCursor.Insert:
                picScreen.Cursor = Cursors.IBeam;
                break;
            case TSCursor.DragSurface:
                msCursor = new(EditorResources.EPC_MOVE);
                picScreen.Cursor = new Cursor(msCursor);
                break;
            case TSCursor.Select:
                msCursor = new(EditorResources.EPC_SELECT);
                picScreen.Cursor = new Cursor(msCursor);
                break;
            }
        }

        private void StartDrag(Point start) {
            if (hsbScreen.Visible || vsbScreen.Visible) {
                DragPT = start;
                EditAction = TSAction.Drag;
                SetCursor(TSCursor.DragSurface);
            }
        }

        private void ChangeColor(Rectangle rect, AGIColorIndex colorBG, AGIColorIndex colorFG) {
            for (int i = rect.Left; i < rect.Right; i++) {
                for (int j = rect.Top; j < rect.Bottom; j++) {
                    if (colorBG != AGIColorIndex.None) {
                        TextData[i, j].BG = colorBG;
                    }
                    if (colorFG != AGIColorIndex.None) {
                        TextData[i, j].FG = colorFG;
                    }
                }
            }
            // redraw the affected screen portion
            DrawScreen(rect);
            MarkAsChanged();
        }

        public static void ToggleInsertKey() {
            // This is supposed to be a more stable and reliable
            // alternative to the SendKeys.Send method.
            // It worked when I originally added it, but now it
            // doesn't. I don't know why it stopped. So for now
            // I'm going back to SendKeys.Send()...

            INPUT[] inputs = new INPUT[2];

            // Key down
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].ki.wVk = VK_INSERT;
            inputs[0].ki.dwFlags = KEYEVENTF_KEYDOWN;

            // Key up
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].ki.wVk = VK_INSERT;
            inputs[1].ki.dwFlags = KEYEVENTF_KEYUP;

            // Send the input
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public void ToggleINS() {
            // toggle insert/overwrite mode
            OverwriteMode = !OverwriteMode;
            btnMode.Image = OverwriteMode ? EditorResources.ET_OVR : EditorResources.ET_INS;
        }

        private void UpdateToolbarButtons() {
            // set toolbar buttons to match menu items
            btnCut.Enabled = Selection.Width > 0;
            btnCopy.Enabled = Selection.Width > 0;
            //btnPaste is handled by the clipboard monitor
            btnDelete.Enabled = true;
            btnClear.Enabled = Selection.Width > 0;
        }

        /// <summary>
        /// Shows or hides scrollbars for the screen surface when the form
        /// is resized or image scale is changed.
        /// </summary>
        /// <param name="oldscale"></param>
        private void SetScrollbars(float oldscale = 0) {
            // determine if scrollbars are necessary
            bool showHSB = picScreen.Width > (panel1.ClientSize.Width - 2 * PE_MARGIN);
            bool showVSB = picScreen.Height > (panel1.ClientSize.Height - 2 * PE_MARGIN - (showHSB ? hsbScreen.Height : 0));
            // check horizontal again(in case addition of vert scrollbar forces it to be shown)
            showHSB = picScreen.Width > (panel1.ClientSize.Width - 2 * PE_MARGIN - (showVSB ? vsbScreen.Width : 0));
            // initial positions
            hsbScreen.Top = panel1.ClientSize.Height - hsbScreen.Height;
            vsbScreen.Left = panel1.ClientSize.Width - vsbScreen.Width;
            hsbScreen.Width = panel1.ClientSize.Width;
            vsbScreen.Height = panel1.ClientSize.Height;
            if (showHSB && showVSB) {
                // allow for corner
                picCorner.Left = vsbScreen.Left;
                picCorner.Top = hsbScreen.Top;
                vsbScreen.Height -= hsbScreen.Height;
                hsbScreen.Width -= vsbScreen.Width;
                picCorner.Visible = true;
            }
            else {
                picCorner.Visible = false;
            }
            hsbScreen.Visible = showHSB;
            vsbScreen.Visible = showVSB;

            Point anchorpt = new(-1, -1);
            if (oldscale > 0) {
                // if using anchor point need to determine which image the
                // cursor is in
                Point cp = panel1.PointToClient(Cursor.Position);
                if (panel1.ClientRectangle.Contains(cp)) {
                    // use this anchor
                    anchorpt = cp;
                }
                else {
                    // not a valid anchor
                    oldscale = 0;
                }
            }
            // now adjust all scrollbar parameters as needed
            AdjustScrollbars(oldscale, anchorpt, hsbScreen, vsbScreen, picScreen);
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
        private void AdjustScrollbars(double oldscale, Point anchor, HScrollBar hsb, VScrollBar vsb, PictureBox image) {
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
                hsb.LargeChange = (int)(panel1.ClientRectangle.Width * LG_SCROLL);
                hsb.SmallChange = (int)(panel1.ClientRectangle.Width * SM_SCROLL);
                // calculate actual max (when image is fully scrolled to right)
                int SV_MAX = image.Width - (panel1.ClientRectangle.Width - (vsb.Visible ? vsb.Width : 0)) + PE_MARGIN;
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
                vsb.LargeChange = (int)(panel1.ClientRectangle.Height * LG_SCROLL);
                vsb.SmallChange = (int)(panel1.ClientRectangle.Height * SM_SCROLL);
                int SV_MAX = image.Height - (panel1.ClientRectangle.Height - (hsb.Visible ? hsb.Height : 0)) + PE_MARGIN;
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
            _ = SendMessage(panel1.Handle, WM_SETREDRAW, false, 0);
            _ = SendMessage(picScreen.Handle, WM_SETREDRAW, false, 0);
            // resize images
            picScreen.Width = (int)(320 * ScaleFactor);
            picScreen.Height = (int)(200 * ScaleFactor);
            picScreen.Image = new Bitmap((int)(320 * ScaleFactor), (int)(200 * ScaleFactor));
            // then set the scrollbars
            SetScrollbars(oldscale);
            // redraw screen at new scale
            DrawScreen();
            spScale.Text = "Scale: " + (ScaleFactor * 100) + "%";
            _ = SendMessage(picScreen.Handle, WM_SETREDRAW, true, 0);
            _ = SendMessage(panel1.Handle, WM_SETREDRAW, true, 0);
            //picScreen.Invalidate();
            hsbScreen.Invalidate();
            vsbScreen.Invalidate();
            picCorner.Invalidate();
            panel1.Invalidate();
        }

        private void LoadScreenTextFile() {
            // gets a text screen file and loads it

            if (IsChanged) {
                switch (MessageBox.Show(MDIMain,
                    "Do you want to save the current text screen first?",
                    "Save Text Screen Layout",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question)) {
                case DialogResult.Cancel:
                    return;
                case DialogResult.Yes:
                    SaveTextScreen(FileName);
                    if (IsChanged) {
                        // save was canceled
                        return;
                    }
                    break;
                }
            }
            // get a file to open
            MDIMain.OpenDlg.Title = "Open Text Screen Layout File";
            MDIMain.OpenDlg.ShowHiddenFiles = false;
            MDIMain.OpenDlg.ShowReadOnly = false;
            MDIMain.OpenDlg.DefaultExt = "tsd";
            MDIMain.OpenDlg.Filter = "Text Screen files (*.tsd)|*.tsd|All files (*.*)|*.*";
            MDIMain.OpenDlg.FilterIndex = 1;
            MDIMain.OpenDlg.FileName = "";
            MDIMain.OpenDlg.InitialDirectory = DefaultResDir;
            if (MDIMain.OpenDlg.ShowDialog() == DialogResult.Cancel) {
                return;
            }
            DefaultResDir = JustPath(MDIMain.OpenDlg.FileName);
            ExtractScreenData(MDIMain.OpenDlg.FileName);
        }

        private void ExtractScreenData(string loadfile) {
            FileStream fs = null;
            bool invalid = false, updateversion = false;
            byte[] data = null;
            try {
                do {
                    fs = new(loadfile, FileMode.Open, FileAccess.Read);
                    byte[] buffer = new byte[6];
                    if (fs.Read(buffer, 0, 6) != 6) {
                        // invalid
                        invalid = true;
                        break;
                    }
                    string marker = Encoding.UTF8.GetString(buffer);
                    if (marker[..4] != "WATS") {
                        // invalid
                        invalid = true;
                        break;
                    }
                    int width = marker[4..].IntVal();
                    switch (width) {
                    case 39:
                    case 40:
                        // 40 col - always ok; confirm file length
                        if (fs.Length != 2006) {
                            // invalid
                            invalid = true;
                            break;
                        }
                        // force 40col mode
                        ScreenWidth = 40;
                        CharWidth = 8;
                        data = new byte[2000];
                        if (fs.Read(data, 0, 2000) != 2000) {
                            // invalid
                            invalid = true;
                            break;
                        }
                        if (width == 39) {
                            updateversion = true;
                        }
                        break;
                    case 79:
                    case 80:
                        // 80 col - not valid if not using powerpack
                        if (EditGame is null || !EditGame.PowerPack) {
                            MessageBox.Show("80 column text screens are only valid if Power Pack is enabled.",
                                "Unsupported File Version",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            break;
                        }
                        // confirm file length
                        if (fs.Length != 4006) {
                            // invalid
                            invalid = true;
                            break;
                        }
                        // force 80col mode
                        ScreenWidth = 80;
                        CharWidth = 4;
                        data = new byte[4000];
                        if (fs.Read(data, 0, 4000) != 4000) {
                            // invalid
                            invalid = true;
                            break;
                        }
                        if (width == 79) {
                            updateversion = true;
                        }
                        break;
                    default:
                        // invalid
                        invalid = true;
                        break;
                    }
                } while (false);
            }
            catch (Exception ex) {
                ErrMsgBox(ex,
                    "Unable to open file.",
                    ex.StackTrace,
                    "File Access Error");
            }
            finally {
                fs?.Close();
            }
            // if no data, it means an 80 col file was tried without
            // PowerPack support
            if (data is null) {
                return;
            }
            // if invalid, inform user
            if (invalid) {
                MessageBox.Show(MDIMain,
                    "File is not a WinAGI Text Screen data file.",
                    "Invalid File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            // read the screen data
            for (int j = 0; j < 25; j++) {
                for (int i = 0; i < ScreenWidth; i++) {
                    TextData[i, j].BG = (AGIColorIndex)(data[(j * ScreenWidth + i) * 2] >> 4);
                    TextData[i, j].FG = (AGIColorIndex)(data[(j * ScreenWidth + i) * 2] & 15);
                    TextData[i, j].CharVal = (byte)(data[(j * ScreenWidth + i) * 2 + 1] + 0);
                }
            }
            // reset cursor and redraw
            SetSelection(new Point());
            MarginLeft = 0;
            DrawScreen();
            // update filename and status
            FileName = loadfile;
            if (updateversion) {
                // this is a v2 file, so mark it to be updated
                MarkAsChanged();
            }
            else {
                MarkAsSaved();
            }
        }

        private string NewSaveFileName(string filename = "") {
            if (filename.Length != 0) {
                MDIMain.SaveDlg.Title = "Save Text Screen Layout";
                MDIMain.SaveDlg.FileName = Path.GetFileName(filename);
            }
            else {
                MDIMain.SaveDlg.Title = "Save Text Screen Layout As";
                MDIMain.SaveDlg.FileName = "";
            }
            MDIMain.SaveDlg.Filter = "Text Screen files (*.tsd)|*.tsd|All files (*.*)|*.*";
            MDIMain.SaveDlg.FilterIndex = 1;
            MDIMain.SaveDlg.DefaultExt = "tsd";
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.OkRequiresInteraction = true;
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            DialogResult rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            if (rtn == DialogResult.Cancel) {
                // nothing selected
                return "";
            }
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            return MDIMain.SaveDlg.FileName;
        }

        private void SaveTextScreen(string filename = "") {
            // if no filename passed, get a name from user
            if (filename.Length == 0) {
                // get a file name for this layout
                filename = NewSaveFileName();
                if (filename.Length == 0) {
                    // user canceled
                    return;
                }
            }
            FileStream fs = null;
            try {
                fs = new FileStream(filename, FileMode.Create);
                // save file marker first ('WATS')
                fs.Write([87, 65, 84, 83, (byte)(ScreenWidth == 40 ? 52 : 56), 48]);
                // then add data
                for (int j = 0; j < 25; j++) {
                    for (int i = 0; i < ScreenWidth; i++) {
                        fs.Write(
                            [(byte)(16 * (byte)TextData[i, j].BG + TextData[i, j].FG),
                            TextData[i, j].CharVal]);
                    }
                }
            }
            catch (Exception ex) {
                ErrMsgBox(ex,
                    "Error while saving:",
                    ex.StackTrace,
                    "File Save Error");
            }
            finally {
                fs?.Dispose();
            }
            FileName = filename;
            MarkAsSaved();
        }

        internal void ShowHelp() {
            string topic = "htm\\winagi\\textscreen.htm";

            // TODO: add context sensitive help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }

        private bool AskClose() {
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save this screen layout?",
                    "Save Text Screen Layout",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveTextScreen(FileName);
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "Resource not saved. Continue closing anyway?",
                            "File Save Error",
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

        private void MarkAsChanged() {
            // ignore when loading (not visible yet)
            if (!Visible) {
                return;
            }
            if (!IsChanged) {
                IsChanged = true;
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = "Text Screen Designer - " + Path.GetFileName(FileName);
            MDIMain.btnSaveResource.Enabled = false;
        }
        #endregion
    }

    [Serializable]
    public class TextscreenClipboardData {
        public Size Size { get; set; } = new();
        public TextChar[,] Data { get; set; } = new TextChar[0, 0];
    }
}
