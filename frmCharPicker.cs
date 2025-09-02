using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WinAGI.Common;

namespace WinAGI.Editor {
    public partial class frmCharPicker : Form {
        public bool Cancel;
        public string InsertString = "";
        private readonly Encoding CodePage;
        private GridChar SelChar, CurChar;
        private int SelStart, SelEnd, CursorPos, DragPos;
        private bool blnCursor, DragSel, MakeSel;
        private readonly Bitmap chargrid;
        private readonly Bitmap invchargrid;
        private Point lastMousePosition;
        bool showtip = false;

        private struct GridChar {
            public byte Value = 0;
            public int X {
                get {
                    return Value % 16;
                }
            }
            public int Y {
                get {
                    return Value / 16;
                }
            }

            public GridChar(byte value) {
                Value = value;
            }

            public GridChar(char value) {
                Value = (byte)value;
            }

            public GridChar() {
            }
        }

        public frmCharPicker(int codepage) {
            InitializeComponent();
            CodePage = Encoding.GetEncoding(codepage);
            byte[] obj = (byte[])EditorResources.ResourceManager.GetObject("CP" + codepage);
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
        }

        #region Event Handlers
        private void frmCharPicker_Load(object sender, EventArgs e) {
            // select char 1
            SelChar.Value = 1;
        }

        private void btnInsert_Click(object sender, EventArgs e) {
            // conver the string to the correct codepage
            byte[] strdat = new byte[InsertString.Length];
            for (int i = 0; i < InsertString.Length; i++) {
                strdat[i] = (byte)InsertString[i];
            }
            InsertString = CodePage.GetString(strdat);
            Cancel = false;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            Cancel = true;
            Hide();
        }

        private void picSelect_Leave(object sender, EventArgs e) {
            // always hide the tooltip
            lblTip.Visible = false;
        }

        private void picSelect_MouseClick(object sender, MouseEventArgs e) {
            int CurX, CurY;

            CurX = e.X / 21;
            CurY = e.Y / 21;

            if (CurX < 0) CurX = 0;
            if (CurX > 15) CurX = 15;
            if (CurY < 0) CurY = 0;
            if (CurY > 15) CurY = 15;
            if (CurX == 0 && CurY == 0) return;
            if (SelChar.Value != CurX + CurY * 16) {
                SelChar.Value = (byte)(CurX + CurY * 16);
                picSelect.Invalidate();
            }
        }

        private void picSelect_MouseDoubleClick(object sender, MouseEventArgs e) {
            int intChar;
            int CurX, CurY;

            CurX = e.X / 21;
            CurY = e.Y / 21;

            if (CurX < 0) CurX = 0;
            if (CurX > 15) CurX = 15;
            if (CurY < 0) CurY = 0;
            if (CurY > 15) CurY = 15;
            intChar = CurX + CurY * 16;
            switch (intChar) {
            case 0:
                // nothing to add
                return;
            default:
                SelChar.Value = (byte)intChar;
                switch (intChar) {
                case 13:
                    // add slash code for newline
                    AddChar(92);
                    AddChar(110);
                    break;
                case < 32:
                case 127:
                case 255:
                    // add slash-x codes for these chars
                    AddChar(92);
                    AddChar(120);
                    // add char as two-digit hex string
                    if (intChar / 16 > 9) {
                        AddChar(intChar / 16 + 55);
                    }
                    else {
                        AddChar(intChar / 16 + 48);
                    }
                    if (intChar % 16 > 9) {
                        AddChar(intChar % 16 + 55);
                    }
                    else {
                        AddChar(intChar % 16 + 48);
                    }
                    break;
                default:
                    AddChar(intChar);
                    break;
                }
                picInsert.Invalidate();
                break;
            }
        }

        private void picSelect_Paint(object sender, PaintEventArgs e) {
            DrawGrid(e.Graphics);
        }

        private void picSelect_MouseMove(object sender, MouseEventArgs e) {
            int CurX, CurY;
            if (showtip) {
                showtip = false;
                return;
            }
            // ignore if any button is pressed or if not focused
            if (!picSelect.Focused || e.Button != MouseButtons.None) {
                return;
            }
            CurX = e.X / 21;
            CurY = e.Y / 21;
            if (CurX < 0) CurX = 0;
            if (CurX > 15) CurX = 15;
            if (CurY < 0) CurY = 0;
            if (CurY > 15) CurY = 15;
            CurChar.Value = (byte)(CurX + CurY * 16);
            if (e.Location != lastMousePosition) {
                lastMousePosition = e.Location;
                hoverTimer.Stop();
                hoverTimer.Start();
            }
            lblTip.Hide();

            // set cursor to nodrop if on char 0
            if (CurChar.Value == 0) {
                this.Cursor = Cursors.No;
            }
            else {
                this.Cursor = Cursors.Default;
            }
        }

        private void picSelect_KeyDown(object sender, KeyEventArgs e) {
            int CurX, CurY;

            switch (e.Modifiers) {
            case Keys.None:
                switch (e.KeyCode) {
                case Keys.Up:
                    if (SelChar.Y == 0) {
                        return;
                    }
                    CurY = SelChar.Y - 1;
                    CurX = SelChar.X;
                    break;
                case Keys.Down:
                    if (SelChar.Y == 15) {
                        return;
                    }
                    CurY = SelChar.Y + 1;
                    CurX = SelChar.X;
                    break;
                case Keys.Right:
                    if (SelChar.X == 15) {
                        return;
                    }
                    CurX = SelChar.X + 1;
                    CurY = SelChar.Y;
                    break;
                case Keys.Left:
                    if (SelChar.X == 0) {
                        return;
                    }
                    CurX = SelChar.X - 1;
                    CurY = SelChar.Y;
                    break;
                default:
                    return;
                }
                SelChar.Value = (byte)(CurX + 16 * CurY);
                break;
            }
            picSelect.Invalidate();
        }

        private void picInsert_KeyDown(object sender, KeyEventArgs e) {
            bool blnUpdate = false;

            if (e.Modifiers == Keys.None) {
                switch (e.KeyCode) {
                case Keys.Right:
                    // if there is a selection, collapse to right
                    if (SelStart != SelEnd) {
                        CursorPos = SelEnd;
                        SelStart = SelEnd;
                        blnUpdate = true;
                    }
                    else {
                        // move cursor one place to right
                        if (CursorPos < InsertString.Length) {
                            CursorPos++;
                            SelStart = CursorPos;
                            SelEnd = CursorPos;
                            blnUpdate = true;
                        }
                    }
                    break;
                case Keys.Left:
                    // if there is a selection, collapse to left
                    if (SelStart != SelEnd) {
                        CursorPos = SelStart;
                        SelEnd = SelStart;
                        blnUpdate = true;
                    }
                    else {
                        // move cursor one place to left
                        if (CursorPos > 0) {
                            CursorPos--;
                            SelStart = CursorPos;
                            SelEnd = CursorPos;
                            blnUpdate = true;
                        }
                    }
                    break;
                case Keys.Delete:
                    // if there is a selection, delete it
                    if (SelStart != SelEnd) {
                        // delete the selection, and collapse to start
                        InsertString = InsertString.Left(SelStart) + InsertString.Right(InsertString.Length - SelEnd);
                        CursorPos = SelStart;
                        SelEnd = SelStart;
                        blnUpdate = true;
                    }
                    else {
                        // delete char after cursor, cursor stays the same
                        if (CursorPos < InsertString.Length) {
                            InsertString = InsertString.Left(CursorPos) + InsertString.Right(InsertString.Length - CursorPos - 1);
                            blnUpdate = true;
                        }
                    }
                    break;
                default:
                    return;
                }
            }
            else if (e.Modifiers == Keys.Shift) {
                switch (e.KeyCode) {
                case Keys.Right:
                    // if there is a selection, expand or shrink depending on cursorpos
                    if (SelStart != SelEnd) {
                        if (CursorPos == SelStart) {
                            // shrink the selection
                            CursorPos++;
                            SelStart = CursorPos;
                            blnUpdate = true;
                        }
                        else {
                            // expand the selection if there is room
                            if (CursorPos < InsertString.Length) {
                                CursorPos++;
                                SelEnd = CursorPos;
                                blnUpdate = true;
                            }
                        }
                    }
                    else {
                        // expand the selection if there is room
                        if (CursorPos < InsertString.Length) {
                            CursorPos++;
                            SelEnd = CursorPos;
                            blnUpdate = true;
                        }
                    }
                    break;
                case Keys.Left:
                    // if there is a selection,
                    // expand or shrink depending on cursorpos
                    if (SelStart != SelEnd) {
                        if (CursorPos == SelEnd) {
                            // shrink the selection
                            CursorPos--;
                            SelEnd = CursorPos;
                            blnUpdate = true;
                        }
                        else {
                            // expand the selection if there is room
                            if (CursorPos > 0) {
                                CursorPos--;
                                SelStart = CursorPos;
                                blnUpdate = true;
                            }
                        }
                    }
                    else {
                        // expand the selection if there is room
                        if (CursorPos > 0) {
                            CursorPos--;
                            SelStart = CursorPos;
                            blnUpdate = true;
                        }
                    }
                    break;
                default:
                    return;
                }
            }
            // did anything change?
            if (blnUpdate) {
                picInsert.Invalidate();
                e.Handled = true;
            }
        }

        private void picInsert_KeyPress(object sender, KeyPressEventArgs e) {
            int intChar = e.KeyChar;
            bool blnUpdate = false;
            if (Control.ModifierKeys == Keys.None) {
                switch (intChar) {
                case 8:
                    // backspace
                    if (SelStart != SelEnd) {
                        // delete the selection, and collapse to start
                        InsertString = InsertString.Left(SelStart) + InsertString.Right(InsertString.Length - SelEnd);
                        CursorPos = SelStart;
                        SelEnd = SelStart;
                        blnUpdate = true;
                    }
                    else {
                        // delete char in front of cursor, and move cursor
                        if (CursorPos > 0) {
                            InsertString = InsertString.Left(CursorPos - 1) + InsertString.Right(InsertString.Length - CursorPos);
                            CursorPos = CursorPos - 1;
                            SelStart = CursorPos;
                            SelEnd = CursorPos;
                            blnUpdate = true;
                        }
                    }
                    break;
                case < 31:
                case > 254:
                    // do nothing
                    break;
                default:
                    // add the character
                    AddChar(intChar);
                    blnUpdate = true;
                    break;
                }
            }
            // did anything change?
            if (blnUpdate) {
                picInsert.Invalidate();
            }
        }

        private void picInsert_MouseDown(object sender, MouseEventArgs e) {
            // only care about left button
            if (e.Button != MouseButtons.Left) {
                return;
            }
            // use empirical offset to get best rounding
            // to determine which position was clicked
            int CurX = (e.X + 1) / 17;

            // limit cursor position to allowable values
            if (CurX < 0) CurX = 0;
            if (CurX > InsertString.Length) CurX = InsertString.Length;
            // if in a selection,  start drag
            if (CurX >= SelStart && CurX < SelEnd) {
                DragSel = true;
            }
            // otherwise, make a new selection
            else {
                // put cursor at this spot
                CursorPos = SelStart = SelEnd = CurX;
                picInsert.Invalidate();
                // enable mouse selection
                MakeSel = true;
                StopCursor();
                return;
            }
        }

        private void picInsert_MouseMove(object sender, MouseEventArgs e) {
            // use empirical offset to get best rounding
            // to determine which position was clicked
            int CurX = (e.X + 1) / 17;
            // limit cursor position to allowable values
            if (CurX < 0) CurX = 0;
            if (CurX > InsertString.Length) CurX = InsertString.Length;
            switch (e.Button) {
            case MouseButtons.None:
                // if no button, adjust mouse pointer
                if (SelStart != SelEnd) {
                    if (CurX >= SelStart && CurX < SelEnd) {
                        picInsert.Cursor = Cursors.Arrow;
                    }
                    else {
                        picInsert.Cursor = Cursors.IBeam;
                    }
                }
                else {
                    picInsert.Cursor = Cursors.IBeam;
                }
                return;
            case MouseButtons.Left:
                // if selecting, adjust selection as appropriate
                if (MakeSel) {
                    if (CurX < CursorPos) {
                        SelStart = CurX;
                        SelEnd = CursorPos;
                    }
                    else {
                        SelStart = CursorPos;
                        SelEnd = CurX;
                    }
                    picInsert.Invalidate();
                }

                else if (DragSel) {
                    // if dragging, show drag cursor (default is arrow)
                    picInsert.Cursor = Cursors.Default;
                    // position drag point
                    DragPos = CurX;
                    picInsert.Invalidate();
                }
                break;
            }
        }

        private void picInsert_MouseUp(object sender, MouseEventArgs e) {
            // only care about left button
            if (e.Button != MouseButtons.Left) {
                return;
            }
            // use empirical offset to get best rounding
            // to determine which position was clicked
            int CurX = (e.X + 1) / 17;
            // limit cursor position to allowable values
            if (CurX < 0) CurX = 0;
            if (CurX > InsertString.Length) CurX = InsertString.Length;
            // if selecting, stop selecting
            if (MakeSel) {
                MakeSel = false;
                StartCursor();
                return;
            }
            // if dragging, finish the drag
            if (DragSel) {
                DragSel = false;
                // if cursor is within selection, do nothing
                if (CurX >= SelStart && CurX < SelEnd) {
                    // show the arrow cursor
                    picInsert.Cursor = Cursors.Arrow;
                }
                else {
                    // drop the selected text at drag pos
                    if (DragPos < SelStart) {
                        InsertString = InsertString.Left(DragPos) + InsertString.Mid(SelStart, SelEnd - SelStart) + InsertString.Mid(DragPos, SelStart - DragPos) + InsertString.Right(InsertString.Length - SelEnd);
                        CursorPos = DragPos + SelEnd - SelStart;
                        SelStart = SelEnd = CursorPos;
                    }
                    else {
                        InsertString = InsertString.Left(SelStart) + InsertString.Mid(SelEnd, DragPos - SelEnd) + InsertString.Mid(SelStart, SelEnd - SelStart) + InsertString.Right(InsertString.Length - DragPos);
                        CursorPos = DragPos;
                        SelStart = SelEnd = CursorPos;
                    }
                    picInsert.Invalidate();
                    // restore cursor
                    picInsert.Cursor = Cursors.IBeam;
                }
            }
        }

        private void picInsert_Paint(object sender, PaintEventArgs e) {
            int lngSelLen;
            GridChar intChar;

            e.Graphics.Clear(picInsert.BackColor);
            if (InsertString.Length == 0) {
                return;
            }
            Rectangle r1 = new( 16, 16, 16, 16);
            lngSelLen = SelEnd - SelStart;

            // draw each char, one at a time
            for (int i = 0; i < InsertString.Length; i++) {
                intChar = new(InsertString[i]);
                Rectangle charsource = new(intChar.X * 16, intChar.Y * 16, 16, 16);
                if (lngSelLen > 0 && i >= SelStart && i < SelEnd) {
                    e.Graphics.DrawImage(invchargrid, i * 17 + 1, 2, charsource, GraphicsUnit.Pixel);
                    // if not at end of selection, extend selection past the cursor space
                    if (i < SelEnd) {
                        Point pt1, pt2;
                        pt1 = pt2 = new Point((i + 1) * 17, 2);
                        pt2.Offset(0, 15);
                        e.Graphics.DrawLine(Pens.Black, pt1, pt2);
                    }
                }
                else {
                    e.Graphics.DrawImage(chargrid, i * 17 + 1, 2, charsource, GraphicsUnit.Pixel);
                }
            }
            // adjust cursor based on mode
            if (DragSel) {
                // show the drag cursor at drag point
                e.Graphics.DrawLine(Pens.Black, DragPos * 17 + 1, 0, DragPos * 17 + 1, 19);
                StopCursor();
            }
            else {
                StartCursor();
            }
        }

        private void tmrCursor_Tick(object sender, EventArgs e) {
            // if no selection, flash the cursor
            if (SelStart == SelEnd) {
                blnCursor = !blnCursor;
                using Graphics g = picInsert.CreateGraphics();
                if (blnCursor) {
                    // turn it on
                    g.DrawLine(Pens.Black, CursorPos * 17 + 1, 4, CursorPos * 17 + 1, 19);

                }
                else {
                    // turn it off
                    g.DrawLine(Pens.White, CursorPos * 17 + 1, 4, CursorPos * 17 + 1, 19);
                }
            }
        }

        private void HoverTimer_Tick(object sender, EventArgs e) {
            hoverTimer.Stop();
            ShowToolTip();
        }

        #endregion

        private void StartCursor() {
            tmrCursor.Enabled = true;
        }

        private void StopCursor() {
            tmrCursor.Enabled = false;
            // if cursor is on, turn it off
            if (blnCursor) {
                blnCursor = false;
                using Graphics g = picInsert.CreateGraphics();
                g.DrawLine(Pens.White, CursorPos * 17 + 1, 4, CursorPos * 17 + 1, 19);
            }
        }

        private void AddChar(int intChar) {
            AddChar((char)intChar);
        }

        /// <summary>
        /// Adds a character to the InsertString string at the current cursorpos,
        /// and replacing any selection.
        /// </summary>
        /// <param name="intChar"></param>
        private void AddChar(char intChar) {
            if (SelStart != SelEnd) {
                // replace the selection with the new char and reset cursor
                InsertString = InsertString.Left(SelStart) + intChar.ToString() + InsertString.Right(InsertString.Length - SelEnd);
                CursorPos = SelStart + 1;
                SelStart = SelEnd = CursorPos;
            }
            else {
                // insert char at cursor position
                InsertString = InsertString.Left(CursorPos) + intChar.ToString() + InsertString.Right(InsertString.Length - CursorPos);
                SelStart = SelEnd = ++CursorPos;
            }
        }

        /// <summary>
        /// Draws entire charselect grid.
        /// </summary>
        public void DrawGrid(Graphics g) {
            g.Clear(picSelect.BackColor);
            Pen pen = new(Color.Black);

            // draw grid lines
            for (int i = 1; i < 16; i++) {
                g.DrawLine(pen, i * 21, 0, i * 21, 336);
                g.DrawLine(pen, 0, i * 21, 336, i * 21);
            }
            // outside border
            g.DrawRectangle(pen, 0, 0, 336, 336);
            // draw characters
            for (int j = 0; j < 16; j++) {
                for (int i = 0; i < 16; i++) {
                    Rectangle charsource = new(i * 16, j * 16, 16, 16);
                    g.DrawImage(chargrid, i * 21 + 3, j * 21 + 3, charsource, GraphicsUnit.Pixel);
                }
            }
            if (SelChar.Value >= 0) {
                Rectangle rect = new(SelChar.X * 21 + 2, SelChar.Y * 21 + 2, 17, 17);
                g.DrawRectangle(Pens.Red, rect);
            }
        }

        private void ShowToolTip() {
            if (CurChar.Value == 0) return;
            
            Point mp = picSelect.PointToClient(Control.MousePosition);
            if (mp.X < 0) return;
            if (mp.Y < 0) return;
            if (mp.X >= picSelect.Width) return;
            if (mp.Y >= picSelect.Height) return;

            lblTip.Text = "0x" + CurChar.Value.ToString("X2");
            mp.Offset(-lblTip.Width, -lblTip.Height);
            if (mp.X < 0) mp.X = 0;
            if (mp.Y < 0) mp.Y = 0;
            lblTip.Location = mp;
            lblTip.Show();
            showtip = true;
        }

    }
}
