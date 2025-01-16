using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.ArgType;
using static WinAGI.Editor.Base;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text.RegularExpressions;

namespace WinAGI.Editor {
    public partial class frmGlobals : Form {
        public bool InGame = false; // dynamic
        public bool IsChanged = false; // dynamic
        public string FileName = "";
        private bool closing = false;
        private TDefine EditDefine;
        private bool Inserting = false;
        private Stack<GlobalsUndo> UndoCol = [];
        private List<DelDefine> DeletedDefines = [];
        private const string DEF_MARKER = "#define ";
        private TextBox EditTextBox = null;
        private char[] invalidall, invalid1st;
        private string s_invalidall, s_invalid1st;
        private const int TypeCol = 0;
        private const int DefaultCol = 1;
        private const int NameCol = 2;
        private const int ValueCol = 3;
        private const int CommentCol = 4;
        private const int NameCheckCol = 5;
        private const int ValueCheckCol = 6;

        public struct DelDefine {
            public string Name;
            public string Value;
        }

        // a blank, default globals editor
        public frmGlobals() {
            InitializeComponent();
            MdiParent = MDIMain;
            InitFonts();
            if (EditGame == null || !EditGame.SierraSyntax) {
                invalid1st = INVALID_FIRST_CHARS;
                invalidall = INVALID_DEFINE_CHARS;
            }
            else {
                invalidall = INVALID_SIERRA_CHARS;
                invalid1st = INVALID_SIERRA_1ST_CHARS;
            }
            s_invalid1st = new string(invalid1st);
            s_invalidall = new string(invalidall);
            DataGridViewRow template = new();
            template.CreateCells(fgGlobals);
            template.Cells[0].Value = ArgType.None;
            template.Cells[1].Value = "";
            template.Cells[2].Value = "";
            template.Cells[3].Value = "";
            template.Cells[4].Value = "";
            template.Cells[5].Value = DefineNameCheck.OK;
            template.Cells[6].Value = DefineValueCheck.OK;
            fgGlobals.RowTemplate = template;

            fgGlobals.Rows[0].Cells[0].Value = ArgType.None;
            fgGlobals.Rows[0].Cells[1].Value = "";
            fgGlobals.Rows[0].Cells[2].Value = "";
            fgGlobals.Rows[0].Cells[3].Value = "";
            fgGlobals.Rows[0].Cells[4].Value = "";
            fgGlobals.Rows[0].Cells[5].Value = DefineNameCheck.OK;
            fgGlobals.Rows[0].Cells[6].Value = DefineValueCheck.OK;


        }

        #region Event Handlers

        private void frmGlobals_FormClosing(object sender, FormClosingEventArgs e) {
            // cancel editing
            if (fgGlobals.IsCurrentCellInEditMode) {
                fgGlobals.CancelEdit();
            }
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmGlobals_FormClosed(object sender, FormClosedEventArgs e) {
            if (InGame) {
                GEInUse = false;
            }
        }

        /// <summary>
        /// Dynamic function to set up the resource menu.
        /// </summary>
        public void SetResourceMenu() {
            mnuRSave.Enabled = IsChanged;
        }

        /// <summary>
        /// Dynamic function to handle the menu-save click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void mnuRSave_Click(object sender, EventArgs e) {
            MenuClickSave();
        }

        /// <summary>
        /// Dynamic function to handle the menu-export click event. (For
        /// defines list files, it's 'Save As')
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuRExport_Click(object sender, EventArgs e) {
            string filename = NewSaveFileName();
            if (filename.Length != 0) {
                MenuClickSave(filename);
            }
        }

        /// <summary>
        /// Dynamic function to handle the menu-ingame click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void mnuRInGame_Click(object sender, EventArgs e) {
            //TODO: add ingame functionality for defines lists
            //ToggleInGame();
        }

        private void mnuRAddFile_Click(object sender, EventArgs e) {
            AddGlobalsFromFile();
        }

        private void SetEditMenu() {
            if (fgGlobals.IsCurrentCellInEditMode) {
                mnuEUndo.Enabled = false;
                mnuECut.Enabled = false;
                mnuECopy.Enabled = false;
                mnuEPaste.Enabled = false;
                mnuEDelete.Enabled = false;
                mnuEClear.Enabled = false;
                mnuEInsert.Enabled = false;
                mnuESelectAll.Enabled = false;
                mnuEFindInLogics.Enabled = false;
                return;
            }
            mnuEUndo.Enabled = UndoCol.Count > 0;
            if (mnuEUndo.Enabled) {
                mnuEUndo.Text = "Undo " + Editor.Base.LoadResString(GLBUNDOTEXT + (int)UndoCol.Peek().UDAction);
            }
            else {
                mnuEUndo.Text = "Undo";
            }
            mnuECut.Text = "Cut ";
            mnuECopy.Text = "Copy ";
            mnuEDelete.Text = "Delete ";
            if (fgGlobals.SelectionMode == DataGridViewSelectionMode.CellSelect) {
                mnuECut.Enabled = false;
                mnuECopy.Enabled = fgGlobals.CurrentRow.Index != fgGlobals.NewRowIndex;
                mnuEDelete.Enabled = false;
                switch (fgGlobals.CurrentCell.ColumnIndex) {
                case 2:
                    mnuECut.Text += "Name";
                    mnuECopy.Text += "Name";
                    mnuEDelete.Text += "Name";
                    break;
                case 3:
                    mnuECut.Text += "Value";
                    mnuECopy.Text += "Value";
                    mnuEDelete.Text += "Value";
                    break;
                case 4:
                    mnuECut.Text += "Comment";
                    mnuECopy.Text += "Comment";
                    mnuEDelete.Text += "Comment";
                    break;
                }
            }
            else {
                mnuECut.Enabled = mnuECopy.Enabled = mnuEDelete.Enabled = (fgGlobals.CurrentRow.Index != fgGlobals.NewRowIndex);
                mnuECut.Text += "Row";
                mnuECopy.Text += "Row";
                mnuEDelete.Text += "Row";
                if (fgGlobals.SelectedRows.Count > 1) {
                    mnuECut.Text += "s";
                    mnuECopy.Text += "s";
                    mnuEDelete.Text += "s";
                }
            }
            if (Clipboard.ContainsText(TextDataFormat.Text)) {
                // if text on clipboard has globals format (#define ....)
                string strTemp = Clipboard.GetText(TextDataFormat.Text);
                // check for define marker
                if (strTemp.Left(8) == DEF_MARKER) {
                    mnuEPaste.Enabled = true;
                }
                else {
                    mnuEPaste.Enabled = false;
                }
            }
            else {
                mnuEPaste.Enabled = false;
            }
            //mnuEInsert.Enabled = true; // always available
            //mnuESelectAll.Enabled = true; // always available
            mnuEFindInLogics.Visible = mnuESep1.Visible = EditGame != null;
            if (mnuESep1.Visible) {
                if (fgGlobals.SelectionMode == DataGridViewSelectionMode.CellSelect) {
                    mnuEFindInLogics.Enabled = fgGlobals.CurrentCell.ColumnIndex == NameCol;
                }
                else {
                    mnuEFindInLogics.Enabled = (fgGlobals.SelectedRows.Count == 1 && fgGlobals.CurrentRow.Index != fgGlobals.NewRowIndex);
                }
            }
        }

        private void cmGrid_Opening(object sender, CancelEventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode) {
                e.Cancel = true;
                return;
            }
            SetEditMenu();
        }

        private void cmGrid_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            // instead of managing each menu item individually
            // all items are enabled when menu is closed so
            // all shortcut keys will always be available
            //
            // this means each menu item must verify its option is
            // actually available before it runs its code
            mnuEClear.Enabled = true;
            mnuECopy.Enabled = true;
            mnuECut.Enabled = true;
            mnuEDelete.Enabled = true;
            mnuEFindInLogics.Enabled = true;
            mnuEInsert.Enabled = true;
            mnuEPaste.Enabled = true;
            mnuESelectAll.Enabled = true;
            mnuEUndo.Enabled = true;
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            // move menu items to edit menu
            mnuEUndo.Owner = mnuEdit.DropDown;
            mnuESep0.Owner = mnuEdit.DropDown;
            mnuECut.Owner = mnuEdit.DropDown;
            mnuECopy.Owner = mnuEdit.DropDown;
            mnuEPaste.Owner = mnuEdit.DropDown;
            mnuEDelete.Owner = mnuEdit.DropDown;
            mnuEClear.Owner = mnuEdit.DropDown;
            mnuEInsert.Owner = mnuEdit.DropDown;
            mnuESelectAll.Owner = mnuEdit.DropDown;
            mnuESep1.Owner = mnuEdit.DropDown;
            mnuEFindInLogics.Owner = mnuEdit.DropDown;
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            // return menu items to context menu
            mnuEUndo.Owner = cmGrid;
            mnuESep0.Owner = cmGrid;
            mnuECut.Owner = cmGrid;
            mnuECopy.Owner = cmGrid;
            mnuEPaste.Owner = cmGrid;
            mnuEDelete.Owner = cmGrid;
            mnuEClear.Owner = cmGrid;
            mnuEInsert.Owner = cmGrid;
            mnuESelectAll.Owner = cmGrid;
            mnuESep1.Owner = cmGrid;
            mnuEFindInLogics.Owner = cmGrid;

            mnuEClear.Enabled = true;
            mnuECopy.Enabled = true;
            mnuECut.Enabled = true;
            mnuEDelete.Enabled = true;
            mnuEFindInLogics.Enabled = true;
            mnuEInsert.Enabled = true;
            mnuEPaste.Enabled = true;
            mnuESelectAll.Enabled = true;
            mnuEUndo.Enabled = true;

        }

        private void mnuEUndo_Click(object sender, EventArgs e) {
            int validname;

            if (UndoCol.Count == 0 || fgGlobals.IsCurrentCellInEditMode) {
                return;
            }
            GlobalsUndo NextUndo = UndoCol.Pop();
            switch (NextUndo.UDAction) {
            case GlobalsUndo.udgActionType.udgAddDefine:
                // remove the define values that was added
                fgGlobals.Rows.RemoveAt(NextUndo.UDPos);
                break;
            case GlobalsUndo.udgActionType.udgImportDefines:
            case GlobalsUndo.udgActionType.udgPasteDefines:
                // remove the define values were added
                // if there is a value, the define was a replace;
                // if no value, it was added as a new row 

                // step through the defines list in reverse
                // order to preserve the previous state
                for (int i = NextUndo.UDCount - 1; i >= 0; i--) {
                    int rownum = int.Parse(NextUndo.UDDefine[i].Name);
                    string value = NextUndo.UDDefine[i].Value;
                    if (value.Length > 0) {
                        // undo a replace
                        fgGlobals[ValueCol, rownum].Value = value;
                        fgGlobals[TypeCol, rownum].Value = NextUndo.UDDefine[i].Type;
                        fgGlobals[CommentCol, rownum].Value = NextUndo.UDDefine[i].Comment;
                    }
                    else {
                        // undo an addition
                        fgGlobals.Rows.RemoveAt(rownum);
                    }
                }
                break;
            case GlobalsUndo.udgActionType.udgDeleteDefine:
            case GlobalsUndo.udgActionType.udgCutDefine:
            case GlobalsUndo.udgActionType.udgClearList:
                // add back the items removed and select them
                fgGlobals.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                fgGlobals.Rows.Insert(NextUndo.UDPos, NextUndo.UDCount);
                for (int i = 0; i < NextUndo.UDCount; i++) {
                    // set values for hidden columns first
                    fgGlobals[TypeCol, NextUndo.UDPos + i].Value = NextUndo.UDDefine[i].Type;
                    fgGlobals[NameCheckCol, NextUndo.UDPos + i].Value = NextUndo.UDDefine[i].NameCheck;
                    fgGlobals[ValueCheckCol, NextUndo.UDPos + i].Value = NextUndo.UDDefine[i].ValueCheck;
                    fgGlobals[DefaultCol, NextUndo.UDPos + i].Value = NextUndo.UDDefine[i].Default;
                    // visible columns will trigger format event
                    fgGlobals[NameCol, NextUndo.UDPos + i].Value = NextUndo.UDDefine[i].Name;
                    fgGlobals[ValueCol, NextUndo.UDPos + i].Value = NextUndo.UDDefine[i].Value;
                    fgGlobals[CommentCol, NextUndo.UDPos + i].Value = NextUndo.UDDefine[i].Comment;
                    fgGlobals.Rows[NextUndo.UDPos + i].Selected = true;
                    if (NextUndo.UDDefine[i].Default.Length > 0) {
                        DelDefine delDefine = new() {
                            Name = NextUndo.UDDefine[i].Default,
                            Value = NextUndo.UDDefine[i].Value
                        };
                        DeletedDefines.Remove(delDefine);
                    }
                }
                if (!fgGlobals.Rows[NextUndo.UDPos].Displayed) {
                    fgGlobals.FirstDisplayedScrollingRowIndex = fgGlobals.Rows[NextUndo.UDPos].Index;
                }
                break;
            case GlobalsUndo.udgActionType.udgEditName:
                validname = (int)ValidateDefineName(NextUndo.UDText);
                fgGlobals[NameCol, NextUndo.UDPos].Value = NextUndo.UDText;
                fgGlobals[NameCol, NextUndo.UDPos].Selected = true;
                break;
            case GlobalsUndo.udgActionType.udgEditValue:
                fgGlobals[ValueCol, NextUndo.UDPos].Value = NextUndo.UDText;
                ArgType type = ArgType.None;
                DefineValueCheck valcheck = ValidateDefineValue(NextUndo.UDText, ref type);
                fgGlobals[ValueCheckCol, NextUndo.UDPos].Value = valcheck;
                fgGlobals[TypeCol, NextUndo.UDPos].Value = type;
                fgGlobals[ValueCol, NextUndo.UDPos].Selected = true;
                break;
            case GlobalsUndo.udgActionType.udgEditComment:
                fgGlobals[CommentCol, NextUndo.UDPos].Value = NextUndo.UDText;
                fgGlobals[CommentCol, NextUndo.UDPos].Selected = true;
                break;
            case GlobalsUndo.udgActionType.udgSort:
                // restore the previous sort order
                //SortGlobals(-1, NextUndo);
                break;
            }
            MarkAsChanged();
            fgGlobals.Refresh();
        }

        private void mnuECut_Click(object sender, EventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode ||
                fgGlobals.SelectionMode == DataGridViewSelectionMode.CellSelect ||
                fgGlobals.CurrentRow.Index == fgGlobals.NewRowIndex) {
                return;
            }
            // copy
            mnuECopy_Click(sender, e);
            // then delete
            RemoveRows(fgGlobals.SelectedRows[0].Index, fgGlobals.SelectedRows[^1].Index);
            // rename last Undo object
            UndoCol.Peek().UDAction = GlobalsUndo.udgActionType.udgCutDefine;
        }

        private void mnuECopy_Click(object sender, EventArgs e) {
            //  copies the selected cell or the selected rows to the clipboard
            //  the normal clipboard gets the name, value and comment fields
            //  formatted as 'define' lines;
            //  a duplicate internal clipboard is used that also tracks the
            //  hidden 'original name' column
            string strData = "";
            int TopRow, BtmRow;

            if (fgGlobals.IsCurrentCellInEditMode ||
                fgGlobals.CurrentRow.Index == fgGlobals.NewRowIndex) {
                return;
            }
            if (fgGlobals.SelectionMode == DataGridViewSelectionMode.FullRowSelect) {
                TopRow = fgGlobals.SelectedRows[0].Index;
                BtmRow = fgGlobals.SelectedRows[^1].Index;
                if (BtmRow < TopRow) {
                    int swap = BtmRow;
                    BtmRow = TopRow;
                    TopRow = swap;
                }
                for (int i = TopRow; i <= BtmRow; i++) {
                    // add to normal clipboard
                    strData += DEF_MARKER + (string)fgGlobals[NameCol, i].Value + " " + (string)fgGlobals[ValueCol, i].Value;
                    if ((string)fgGlobals[CommentCol, i].Value != "") {
                        strData += " " + (string)fgGlobals[CommentCol, i].Value;
                    }
                    if (i != BtmRow) {
                        strData += "\n";
                    }
                }
            }
            else {
                // select just this cell text
                strData = (string)fgGlobals.CurrentCell.Value;
            }

            // put selected text on clipboard
            Clipboard.Clear();
            Clipboard.SetText(strData);
        }

        private void mnuEPaste_Click(object sender, EventArgs e) {
            bool enabled;

            if (fgGlobals.IsCurrentCellInEditMode) {
                enabled = false;
            }
            else {
                if (Clipboard.ContainsText(TextDataFormat.Text)) {
                    // if text on clipboard has globals format (#define ....)
                    string strTemp = Clipboard.GetText(TextDataFormat.Text);
                    // check for define marker
                    if (strTemp.Left(8) == DEF_MARKER) {
                        enabled = true;
                    }
                    else {
                        enabled = false;
                    }
                }
                else {
                    enabled = false;
                }
            }
            if (!enabled) {
                return;
            }
            bool blnErrors = false;
            GlobalsUndo NextUndo = new();
            NextUndo.UDAction = GlobalsUndo.udgActionType.udgPasteDefines;
            NextUndo.UDPos = fgGlobals.CurrentRow.Index;
            TDefine[] PasteDefines;
            PasteDefines = ReadDefines(Clipboard.GetText(TextDataFormat.Text), ref blnErrors);
            if (PasteDefines.Length == 0) {
                // nothing to paste
                MessageBox.Show(MDIMain,
                    "There are no valid define entries on the clipboard.",
                    "Unable to Paste Defines",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            // add the defines at the insert position
            TDefine[] undodata = InsertDefines(PasteDefines, fgGlobals.CurrentRow.Index, ref blnErrors);

            // if nothing was added, no undo data exists
            if (undodata.Length == 0) {
                MessageBox.Show(MDIMain,
                    "There were no usable data on the clipboard, so nothing was pasted.",
                    "Nothing to Paste",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else {
                NextUndo.UDCount = undodata.Length;
                for (int i = 0; i < undodata.Length; i++) {
                    NextUndo.UDDefine[i] = undodata[i];
                }
                AddUndo(NextUndo);
            }
            if (blnErrors) {
                MessageBox.Show(MDIMain,
                    "Some entries could not be pasted because of formatting or syntax errors.",
                    "Paste Errors",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void mnuEDelete_Click(object sender, EventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode ||
                fgGlobals.SelectionMode == DataGridViewSelectionMode.CellSelect ||
                fgGlobals.CurrentRow.Index == fgGlobals.NewRowIndex) {
                return;
            }
            RemoveRows(fgGlobals.SelectedRows[0].Index, fgGlobals.SelectedRows[^1].Index);
        }

        private void mnuEClear_Click(object sender, EventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode) {
                return;
            }
            if (MessageBox.Show(MDIMain,
                "All global defines will be discarded. Do you want to continue?",
                "Clear Global Defines",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) == DialogResult.Cancel) {
                return;
            }
            GlobalsUndo NextUndo = new();
            NextUndo.UDAction = GlobalsUndo.udgActionType.udgClearList;
            NextUndo.UDCount = fgGlobals.RowCount;
            for (int i = 0; i < fgGlobals.RowCount - 1; i++) {
                TDefine tmpdef = new TDefine();
                tmpdef.Type = (ArgType)fgGlobals[TypeCol, i].Value;
                tmpdef.Default = (string)fgGlobals[DefaultCol, i].Value;
                tmpdef.Name = (string)fgGlobals[NameCol, i].Value;
                tmpdef.Value = (string)fgGlobals[ValueCol, i].Value;
                tmpdef.Comment = (string)fgGlobals[CommentCol, i].Value;
                NextUndo.UDDefine[i] = tmpdef;
            }
            UndoCol.Push(NextUndo);
            // clear grid by deleting all rows
            fgGlobals.Rows.Clear();
            fgGlobals[NameCol, 0].Selected = true;
        }

        private void mnuEInsert_Click(object sender, EventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode) {
                return;
            }
            EditDefine.Type = ArgType.None;
            EditDefine.Default = "";
            EditDefine.Name = "";
            EditDefine.Value = "";
            EditDefine.Comment = "";
            if (fgGlobals.NewRowIndex != fgGlobals.CurrentRow.Index) {
                if (fgGlobals.SelectionMode != DataGridViewSelectionMode.CellSelect) {
                    fgGlobals.SelectionMode = DataGridViewSelectionMode.CellSelect;
                }
                DataGridViewRow newRow = new DataGridViewRow();
                newRow.CreateCells(fgGlobals);
                newRow.Cells[0].Value = ArgType.None;
                newRow.Cells[1].Value = "";
                newRow.Cells[2].Value = "";
                newRow.Cells[3].Value = "";
                newRow.Cells[4].Value = "";
                fgGlobals.Rows.Insert(fgGlobals.CurrentRow.Index, newRow);
                fgGlobals[NameCol, fgGlobals.CurrentRow.Index - 1].Selected = true;
            }
            else {
                // force column to name for new rows
                if (fgGlobals.CurrentCell.ColumnIndex != NameCol) {
                    fgGlobals[NameCol, fgGlobals.CurrentCell.RowIndex].Selected = true;
                }
            }
            fgGlobals.Refresh();
            Inserting = true;
            fgGlobals.BeginEdit(true);
        }

        private void mnuESelectAll_Click(object sender, EventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode) {
                return;
            }
            fgGlobals.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            fgGlobals.MultiSelect = true;
            fgGlobals.CurrentCell = fgGlobals[NameCol, 0];
            fgGlobals.SelectAll();
            fgGlobals.Refresh();
        }

        private void mnuEFindInLogics_Click(object sender, EventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode ||
            (fgGlobals.SelectionMode == DataGridViewSelectionMode.CellSelect && fgGlobals.CurrentCell.ColumnIndex != NameCol) ||
            (fgGlobals.SelectionMode != DataGridViewSelectionMode.CellSelect && (fgGlobals.SelectedRows.Count == 0 || fgGlobals.CurrentRow.Index == fgGlobals.NewRowIndex))
            ) {
                return;
            }
            GFindDir = FindDirection.All;
            GMatchWord = true;
            GMatchCase = true;
            GLogFindLoc = FindLocation.All;
            GFindSynonym = false;
            FindingForm.ResetSearch();
            string searchtext = (string)fgGlobals[NameCol, fgGlobals.CurrentRow.Index].Value;
            FindInLogic(this, searchtext, FindDirection.All, true, true, FindLocation.All);
        }

        private void fgGlobals_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.Value == null || e.RowIndex == fgGlobals.NewRowIndex ||
                fgGlobals[TypeCol, e.RowIndex].Value == null) {
                return;
            }
            // first determine if tooltip is needed
            DataGridViewCell cell = fgGlobals.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string text = (string)e.Value;
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            // get size
            Size szText = TextRenderer.MeasureText(fgGlobals.CreateGraphics(), text, e.CellStyle.Font, proposedSize, flags);
            if (szText.Width > cell.Size.Width - 8) {
                cell.ToolTipText = text;
            }
            else {
                cell.ToolTipText = "";
            }

            // next apply formatting based on validatecheck status
            switch (e.ColumnIndex) {
            case NameCol:
                if (fgGlobals[NameCheckCol, e.RowIndex].Value != null) {
                    switch ((DefineNameCheck)fgGlobals[NameCheckCol, e.RowIndex].Value) {
                    case DefineNameCheck.OK:
                        // default style
                        if (e.CellStyle.Font.Name != fgGlobals.DefaultCellStyle.Font.Name ||
                            e.CellStyle.Font.Bold) {
                            e.CellStyle.ForeColor = Color.Black;
                            e.CellStyle.Font = new(fgGlobals.DefaultCellStyle.Font, FontStyle.Regular);
                        }
                        break;
                    case DefineNameCheck.Empty:
                    case DefineNameCheck.Numeric:
                    case DefineNameCheck.ActionCommand:
                    case DefineNameCheck.TestCommand:
                    case DefineNameCheck.KeyWord:
                    case DefineNameCheck.ArgMarker:
                    case DefineNameCheck.BadChar:
                    case DefineNameCheck.Global:
                        // error values that never occur in a cell
                        break;
                    case DefineNameCheck.ReservedVar:
                    case DefineNameCheck.ReservedFlag:
                    case DefineNameCheck.ReservedNum:
                    case DefineNameCheck.ReservedObj:
                    case DefineNameCheck.ReservedStr:
                    case DefineNameCheck.ReservedMsg:
                    case DefineNameCheck.ResourceID:
                        // use override style
                        if (!e.CellStyle.Font.Bold) {
                            e.CellStyle.ForeColor = Color.Red;
                            e.CellStyle.Font = new(fgGlobals.DefaultCellStyle.Font, FontStyle.Bold);
                        }
                        break;
                    }
                }
                break;
            case ValueCol:
                // TODO: format based on token type
                switch ((DefineValueCheck)fgGlobals[ValueCheckCol, e.RowIndex].Value) {
                case DefineValueCheck.OK:
                    switch ((ArgType)fgGlobals[TypeCol, e.RowIndex].Value) {
                    case ArgType.None:
                        break;
                    case ArgType.Num:
                        // i.e. numeric Value
                        if (e.CellStyle.ForeColor != Color.Black) {
                            e.CellStyle.ForeColor = Color.Black;
                        }
                        break;
                    case ArgType.Var:
                    case ArgType.Flag:
                    case ArgType.Msg:
                    case ArgType.SObj:
                    case ArgType.InvItem:
                    case ArgType.Str:
                    case ArgType.Word:
                    case ArgType.Ctrl:
                        // argument markers
                        if (e.CellStyle.ForeColor != Color.DarkOrange) {
                            e.CellStyle.ForeColor = Color.DarkOrange;
                        }
                        break;
                    case ArgType.DefStr:
                        // string
                        if (e.CellStyle.ForeColor != Color.Green) {
                            e.CellStyle.ForeColor = Color.Green;
                        }
                        break;
                    case ArgType.VocWrd:
                        // not available in defines?
                        break;
                    case ArgType.ActionCmd:
                    case ArgType.TestCmd:
                    case ArgType.Obj:
                    case ArgType.View:
                        // TODO: SierraSyntax values
                        break;
                    }
                    break;
                case DefineValueCheck.Empty:
                case DefineValueCheck.OutofBounds:
                    // error values that never occur in a cell
                    break;
                case DefineValueCheck.BadArgNumber:
                    if (e.CellStyle.ForeColor != Color.Red) {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new(fgGlobals.DefaultCellStyle.Font, FontStyle.Italic);
                    }
                    break;
                case DefineValueCheck.NotAValue:
                    // non-argument (probably error or re-define)
                    if (e.CellStyle.ForeColor != Color.DarkRed) {
                        e.CellStyle.ForeColor = Color.DarkRed;
                    }
                    break;
                case DefineValueCheck.Reserved:
                case DefineValueCheck.Global:
                    // overrides
                    if (e.CellStyle.ForeColor != Color.Red) {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = new(fgGlobals.DefaultCellStyle.Font, FontStyle.Bold);
                    }
                    break;
                }
                break;
            case CommentCol:
                if (!e.CellStyle.Font.Italic) {
                    e.CellStyle.ForeColor = Color.FromArgb(0x50, 0x50, 0x50);
                    e.CellStyle.Font = new(fgGlobals.DefaultCellStyle.Font, FontStyle.Italic);
                }
                break;
            }
        }

        private void fgGlobals_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) {
                return;
            }
            DataGridViewCell cell = fgGlobals.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.ToolTipText.Length > 0) {
                fgGlobals.ShowCellToolTips = true;
            }
        }

        private void fgGlobals_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            fgGlobals.ShowCellToolTips = false;
        }

        private void fgGlobals_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {

        }

        private void fgGlobals_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode) {
                return;
            }
            if (fgGlobals.SelectionMode == DataGridViewSelectionMode.FullRowSelect && e.Button == MouseButtons.Right) {
                if (e.RowIndex == -1 || fgGlobals.Rows[e.RowIndex].Selected) {
                    return;
                }
            }
            if (e.ColumnIndex == -1) {
                if (fgGlobals.SelectionMode != DataGridViewSelectionMode.FullRowSelect) {
                    fgGlobals.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    fgGlobals.MultiSelect = true;
                    if (e.Button == MouseButtons.Right) {
                        // force selection
                        if (e.RowIndex != -1) {
                            fgGlobals.CurrentCell = fgGlobals[NameCol, e.RowIndex];
                            fgGlobals.CurrentRow.Selected = true;
                            fgGlobals.Refresh();
                        }
                    }
                }
            }
            else {
                fgGlobals.SelectionMode = DataGridViewSelectionMode.CellSelect;
                fgGlobals.MultiSelect = false;
                if (e.Button == MouseButtons.Right) {
                    // force selection
                    fgGlobals.CurrentCell = fgGlobals[e.ColumnIndex, e.RowIndex];
                    fgGlobals.CurrentCell.Selected = true;
                    fgGlobals.Refresh();
                }
            }
        }

        private void fgGlobals_SelectionChanged(object sender, EventArgs e) {
            if (fgGlobals.SelectedRows.Count > 0) {
                fgGlobals.MultiSelect = true;
                if (fgGlobals.SelectedRows.Count > 1) {
                    if (fgGlobals.Rows[^1].Selected) {
                        fgGlobals.Rows[^1].Selected = false;
                    }
                }
            }
            else {
                fgGlobals.MultiSelect = false;
            }
        }

        private void fgGlobals_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
            if (e.Control is TextBox) {
                // need to set MultiLine to true so the ENTER key can be captured by
                // KeyPress event
                EditTextBox = e.Control as TextBox;
                if (EditTextBox.ContextMenuStrip != cmCel) {
                    EditTextBox.ContextMenuStrip = cmCel;
                    EditTextBox.Multiline = true;
                    EditTextBox.AcceptsReturn = true;
                    EditTextBox.AcceptsTab = true;
                    EditTextBox.Validating += EditTextBox_Validating;
                    EditTextBox.KeyDown += EditTextBox_KeyDown;
                }
                else {
                    EditTextBox.Multiline = true;
                    EditTextBox.AcceptsReturn = true;
                    EditTextBox.AcceptsTab = true;
                }
            }
        }

        private void fgGlobals_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode) {
                e.Cancel = true;
            }
        }

        private void fgGlobals_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
            if (fgGlobals.CurrentRow.Index == fgGlobals.NewRowIndex) {
                fgGlobals.Rows[fgGlobals.NewRowIndex].Cells[0].Value = ArgType.None;
                fgGlobals.Rows[fgGlobals.NewRowIndex].Cells[1].Value = "";
                fgGlobals.Rows[fgGlobals.NewRowIndex].Cells[2].Value = "";
                fgGlobals.Rows[fgGlobals.NewRowIndex].Cells[3].Value = "";
                fgGlobals.Rows[fgGlobals.NewRowIndex].Cells[4].Value = "";
                fgGlobals.Rows[fgGlobals.NewRowIndex].Cells[5].Value = DefineNameCheck.OK;
                fgGlobals.Rows[fgGlobals.NewRowIndex].Cells[6].Value = DefineValueCheck.OK;

                if (!Inserting) {
                    Inserting = true;
                }
            }
        }

        private void fgGlobals_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            Debug.Assert(fgGlobals.CurrentCell.Value != null);
            if (fgGlobals.CurrentRow.Index == fgGlobals.NewRowIndex || fgGlobals.CurrentCell.Value == null) {
                // only start new define in first column
                if (e.ColumnIndex != NameCol) {
                    return;
                }
                EditDefine.Type = ArgType.None;
                EditDefine.Default = "";
                EditDefine.Name = "";
                EditDefine.Value = "";
                EditDefine.Comment = "";
            }
            else {
                EditDefine.Type = (ArgType)fgGlobals[TypeCol, fgGlobals.CurrentRow.Index].Value;
                EditDefine.Default = (string)fgGlobals[DefaultCol, fgGlobals.CurrentRow.Index].Value;
                EditDefine.Name = (string)fgGlobals[NameCol, fgGlobals.CurrentRow.Index].Value;
                EditDefine.Value = (string)fgGlobals[ValueCol, fgGlobals.CurrentRow.Index].Value;
                EditDefine.Comment = (string)fgGlobals[CommentCol, fgGlobals.CurrentRow.Index].Value;
            }
            fgGlobals.BeginEdit(true);
        }

        private void fgGlobals_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyData == (Keys.E | Keys.Alt) && !fgGlobals.IsCurrentCellInEditMode) {
                if (fgGlobals.CurrentRow.Index == fgGlobals.NewRowIndex) {
                    EditDefine.Type = ArgType.None;
                    EditDefine.Default = "";
                    EditDefine.Name = "";
                    EditDefine.Value = "";
                    EditDefine.Comment = "";
                }
                else {
                    EditDefine.Type = (ArgType)fgGlobals[TypeCol, fgGlobals.CurrentRow.Index].Value;
                    EditDefine.Default = (string)fgGlobals[DefaultCol, fgGlobals.CurrentRow.Index].Value;
                    EditDefine.Name = (string)fgGlobals[NameCol, fgGlobals.CurrentRow.Index].Value;
                    EditDefine.Value = (string)fgGlobals[ValueCol, fgGlobals.CurrentRow.Index].Value;
                    EditDefine.Comment = (string)fgGlobals[CommentCol, fgGlobals.CurrentRow.Index].Value;
                }
                fgGlobals.BeginEdit(true);
            }
        }

        private void EditTextBox_Validating(object sender, CancelEventArgs e) {
            // textbox Validating event ignores Cancel property, use CellValidate
        }

        private void EditTextBox_KeyDown(object sender, KeyEventArgs e) {
            // pressing enter should move to next COLUMN, not next ROW
            // (unless it's at end of row)
            string message = "";
            bool blnNoWarn = false, forceedit = false;
            DialogResult rtn = DialogResult.No;

            if (e.KeyCode == Keys.Escape) {
                // normally just cancel, and restore previous value
                // but if quitting on value when adding a new line,
                // entire line needs to be deleted
                fgGlobals.CancelEdit();
                fgGlobals.EndEdit();
                if ((string)fgGlobals.CurrentCell.Value == "") {
                    // delete current row if not insert row
                    if (!fgGlobals.CurrentRow.IsNewRow) {
                        fgGlobals.Rows.RemoveAt(fgGlobals.CurrentRow.Index);
                    }
                    return;
                }
            }
            if (e.KeyValue == (int)Keys.Enter || e.KeyValue == (int)Keys.Tab) {
                e.Handled = true;
                e.SuppressKeyPress = true;
                EditTextBox.Text = EditTextBox.Text.Trim();
                GlobalsUndo NextUndo;
                // validate the input
                switch (fgGlobals.CurrentCell.ColumnIndex) {
                case NameCol:
                    if (EditDefine.Name == EditTextBox.Text) {
                        // no change
                        fgGlobals.EndEdit();
                        if (EditDefine.Name.Length == 0) {
                            //cancel an add
                            fgGlobals.Rows.RemoveAt(fgGlobals.Rows.Count - 1);
                        }
                        return;
                    }
                    // can't use the compiler validation checks, because they won't catch
                    // changes that are in the current modified globals list
                    DefineNameCheck namecheck = ValidateDefineName(EditTextBox.Text);
                    // first eight codes are error
                    if (namecheck is >= (DefineNameCheck)1 and <= (DefineNameCheck)8) {
                        switch (namecheck) {
                        case DefineNameCheck.Empty:
                            // 1 = no name
                            switch (WinAGISettings.DelBlankG.Value) {
                            case AskOption.Ask:
                                // get user's response
                                rtn = MsgBoxEx.Show(MDIMain,
                                    "Blank global define names are not allowed. Delete this global define?",
                                    "Delete Blank Define",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    "Always take this action", ref blnNoWarn,
                                    WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                                if (blnNoWarn) {
                                    if (rtn == DialogResult.No) WinAGISettings.DelBlankG.Value = AskOption.No;
                                    if (rtn == DialogResult.Yes) WinAGISettings.DelBlankG.Value = AskOption.Yes;
                                    WinAGISettings.DelBlankG.WriteSetting(WinAGISettingsFile);
                                }
                                break;
                            case AskOption.No:
                                rtn = DialogResult.No;
                                break;
                            case AskOption.Yes:
                                rtn = DialogResult.Yes;
                                break;
                            }
                            if (rtn == DialogResult.Yes) {
                                // OK to delete this row - first, cancel and end edit
                                fgGlobals.CancelEdit();
                                fgGlobals.EndEdit();
                                // now delete current row
                                RemoveRow(fgGlobals.CurrentRow.Index);
                            }
                            return;
                        case DefineNameCheck.Numeric:
                            // 2 = name is numeric
                            message = "Define names cannot be numeric.";
                            break;
                        case DefineNameCheck.ActionCommand:
                            // 3 = name is command
                            message = "'" + EditTextBox.Text + "' is an AGI command, and cannot be redefined.";
                            break;
                        case DefineNameCheck.TestCommand:
                            // 4 = name is test command
                            message = "'" + EditTextBox.Text + "' is an AGI test command, and cannot be redefined.";
                            break;
                        case DefineNameCheck.KeyWord:
                            // 5 = name is a compiler keyword
                            message = "'" + EditTextBox.Text + "' is a compiler reserved word, and cannot be redefined.";
                            break;
                        case DefineNameCheck.ArgMarker:
                            // 6 = name is an argument marker
                            message = "Invalid define name - define names cannot be argument markers.";
                            break;
                        case DefineNameCheck.BadChar:
                            // 7 = name contains improper character
                            string bad1st = "", badchars = "";
                            // build list of the bad chars in the name
                            for (int i = 0; i < invalid1st.Length; i++) {
                                if (invalid1st[i] == EditTextBox.Text[0]) {
                                    bad1st = invalid1st[i].ToString();
                                    break;
                                }
                            }
                            for (int i = 0; i < invalidall.Length; i++) {
                                for (int j = 1; j < EditTextBox.TextLength; j++) {
                                    if (invalidall[i] == EditTextBox.Text[j]) {
                                        if (!badchars.Contains(invalidall[i])) {
                                            badchars += invalidall[i].ToString();
                                        }
                                        break;
                                    }
                                }
                            }
                            if (bad1st.Length > 0) {
                                message = "Invalid starting character in this define name.";
                            }
                            if (badchars.Length > 0) {
                                if (message.Length > 0) {
                                    message += "\n\n";
                                }
                                message += "One or more invalid characters in this define ({badchars}).";
                            }
                            if (message.Length == 0) {
                                message = "One or more invalid characters in this define (<= 32 or > 127)";
                            }
                            break;
                        case DefineNameCheck.Global:
                            // 8 = name is already globally defined
                            message = "'" + EditTextBox.Text + "' is already in use as a global define.";
                            break;
                        }
                        // when a messagebox is displayed the textbox adds a newline
                        // even though the key is supposed to be ignored- I don't
                        // know why- this hack (cancelling multiline, then restoring
                        // it after the messagebox) handles this relatively seamlessly
                        EditTextBox.Multiline = false;
                        MessageBox.Show(MDIMain,
                            message,
                            "Invalid Define Name",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, 0, 0,
                            WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                        EditTextBox.Multiline = true;
                        EditTextBox.SelectAll();
                        return;
                    }
                    // rest of codes can be overridden
                    else if (namecheck > (DefineNameCheck)8) {
                        string nametype = "";
                        switch (namecheck) {
                        case DefineNameCheck.ReservedVar:
                            // 9 = name is reserved variable name
                            nametype = "reserved variable";
                            break;
                        case DefineNameCheck.ReservedFlag:
                            // 10 = name is reserved flag name
                            nametype = "reserved flag";
                            break;
                        case DefineNameCheck.ReservedNum:
                            // 11 = name is reserved number constant
                            nametype = "reserved number constant";
                            break;
                        case DefineNameCheck.ReservedObj:
                            // 12 = name is reserved object
                            nametype = "reserved object";
                            break;
                        case DefineNameCheck.ReservedStr:
                            // 13 = name is reserved string
                            nametype = "reserved string";
                            break;
                        case DefineNameCheck.ReservedMsg:
                            // 14 = name is reserved message
                            nametype = "reserved message";
                            break;
                        case DefineNameCheck.ResourceID:
                            // 15 = name is a resourceID
                            nametype = "resource ID";
                            break;
                        }
                        blnNoWarn = false;
                        if (WinAGISettings.WarnResOverride.Value) {
                            EditTextBox.Multiline = false;
                            rtn = MsgBoxEx.Show(MDIMain,
                               "'" + EditTextBox.Text + "' is a " + nametype + ". Are you sure you want to override it?",
                               "Validate Global Define",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question,
                               "Don't show this warning again.", ref blnNoWarn,
                               WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                            WinAGISettings.WarnResOverride.Value = !blnNoWarn;
                            EditTextBox.Multiline = true;
                        }
                        else {
                            rtn = DialogResult.Yes;
                        }
                        if (rtn == DialogResult.No) {
                            EditTextBox.SelectAll();
                            return;
                        }
                    }
                    EditDefine.Name = EditTextBox.Text;
                    if (!Inserting) {
                        NextUndo = new() {
                            UDAction = GlobalsUndo.udgActionType.udgEditName,
                            UDCount = 1,
                            UDPos = fgGlobals.CurrentRow.Index,
                            UDText = fgGlobals.CurrentCell.Value.ToString()
                        };
                        AddUndo(NextUndo);
                    }
                    fgGlobals[NameCheckCol, fgGlobals.CurrentRow.Index].Value = namecheck;
                    break;
                case ValueCol:
                    if (EditDefine.Value == EditTextBox.Text) {
                        // no change
                        fgGlobals.EndEdit();
                        return;
                    }
                    DefineValueCheck valuecheck = ValidateDefineValue(EditTextBox.Text, ref EditDefine.Type);
                    if (valuecheck is >= (DefineValueCheck)1 and <= (DefineValueCheck)2) {
                        //errors
                        switch (valuecheck) {
                        case DefineValueCheck.Empty:
                            // 1 = no Value
                            message = "Define values cannot be blank.";
                            break;
                        case DefineValueCheck.OutofBounds:
                            // 2 = Value is not byte(0-255) or marker value is not byte
                            message = "Argument Value out of range (must be 0 - 255)";
                            break;
                        }
                        EditTextBox.Multiline = false;
                        MessageBox.Show(MDIMain,
                            message,
                            "Invalid Define Value",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        EditTextBox.Multiline = true;
                        EditTextBox.SelectAll();
                        return;
                    }
                    else {
                        // warnings
                        switch (valuecheck) {
                        case DefineValueCheck.BadArgNumber:
                            // 3 = Value contains an invalid argument Value (controller, string, word)
                            switch (EditTextBox.Text[0]) {
                            case 'c':
                                if (WinAGISettings.WarnInvalidCtlVal.Value) {
                                    EditTextBox.Multiline = false;
                                    rtn = MsgBoxEx.Show(MDIMain,
                                        "'" + EditTextBox.Text + "' is an invalid controller value (limit is c0 - c49). " +
                                        "Do you really want to have an invalid controller definition?",
                                        "Invalid Controller Value",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question,
                                        "Don't show this warning again.", ref blnNoWarn,
                                        WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                                    EditTextBox.Multiline = true;
                                    WinAGISettings.WarnInvalidCtlVal.Value = !blnNoWarn;
                                }
                                else {
                                    rtn = DialogResult.Yes;
                                }
                                if (rtn == DialogResult.No) {
                                    EditTextBox.SelectAll();
                                    return;
                                }
                                break;
                            case 's':
                                if (WinAGISettings.WarnInvalidStrVal.Value) {
                                    string slimit = "s23";
                                    if (EditGame != null) {
                                        if (EditGame.InterpreterVersion == "2.089" || EditGame.InterpreterVersion == "2.272" || EditGame.InterpreterVersion == "3.002149") {
                                            slimit = "s12";
                                        }
                                    }
                                    EditTextBox.Multiline = false;
                                    rtn = MsgBoxEx.Show(MDIMain,
                                        "'" + EditTextBox.Text + "' is an invalid string value (limit is s0 - " + slimit + "). " +
                                        "Do you really want to have an invalid string definition?",
                                        "Invalid String Value",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question,
                                        "Don't show this warning again.", ref blnNoWarn,
                                        WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                                    EditTextBox.Multiline = true;
                                    WinAGISettings.WarnInvalidStrVal.Value = !blnNoWarn;
                                }
                                else {
                                    rtn = DialogResult.Yes;
                                }
                                if (rtn == DialogResult.No) {
                                    EditTextBox.SelectAll();
                                    return;
                                }
                                break;
                            case 'w':
                                EditTextBox.Multiline = false;
                                rtn = MessageBox.Show(MDIMain,
                                    "'" + EditTextBox.Text + "' is an invalid word argument value (limit is w1 - w10). " +
                                    "Do you really want to have an invalid word argument definition?",
                                    "Invalid Word Argument Value",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question, 0, 0,
                                    WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                                EditTextBox.Multiline = true;
                                if (rtn == DialogResult.No) {
                                    EditTextBox.SelectAll();
                                    return;
                                }
                                break;
                            }
                            break;
                        case DefineValueCheck.NotAValue:
                            // 4 = Value is not a string, number or argument marker
                            EditTextBox.Multiline = false;
                            rtn = MessageBox.Show(MDIMain,
                                "'" + EditTextBox.Text + "' is not a number, argument marker or text string." +
                                "Do you really want to assign this define value?",
                                "Unknown Define Value Type",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, 0, 0,
                                WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                            EditTextBox.Multiline = true;
                            if (rtn == DialogResult.No) {
                                EditTextBox.SelectAll();
                                return;
                            }
                            break;
                        case DefineValueCheck.Reserved:
                        // 5 = Value is already defined by a reserved name
                        case DefineValueCheck.Global:
                            // 6 = Value is already defined by a global name
                            if (WinAGISettings.WarnDupGVal.Value) {
                                EditTextBox.Multiline = false;
                                rtn = MsgBoxEx.Show(MDIMain,
                                    "'" + EditTextBox.Text + "' is already defined in this list, or as a reserved variable, " +
                                    "flag or constant. " +
                                    "Do you really want to have duplicate definitions for this Value?",
                                    "Duplicate Define Value",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    "Don't show this warning again.", ref blnNoWarn,
                                    WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                                EditTextBox.Multiline = true;
                                WinAGISettings.WarnDupGVal.Value = !blnNoWarn;
                            }
                            else {
                                rtn = DialogResult.Yes;
                            }
                            if (rtn == DialogResult.No) {
                                EditTextBox.SelectAll();
                                return;
                            }
                            break;
                        }
                    }
                    EditDefine.Value = EditTextBox.Text;
                    forceedit = Inserting;
                    if (!Inserting) {
                        NextUndo = new();
                        NextUndo.UDAction = GlobalsUndo.udgActionType.udgEditValue;
                        NextUndo.UDCount = 1;
                        NextUndo.UDPos = fgGlobals.CurrentRow.Index;
                        NextUndo.UDText = fgGlobals.CurrentCell.Value.ToString();
                        AddUndo(NextUndo);
                    }
                    fgGlobals[ValueCheckCol, fgGlobals.CurrentRow.Index].Value = valuecheck;
                    fgGlobals[TypeCol, fgGlobals.CurrentRow.Index].Value = EditDefine.Type;
                    break;
                case CommentCol:
                    // make sure the edit marker is present
                    if (EditDefine.Comment == EditTextBox.Text && !Inserting) {
                        // no change
                        fgGlobals.EndEdit();
                        return;
                    }
                    if (EditTextBox.Text.Length > 0) {
                        if (EditTextBox.Text[0] != '[' && EditTextBox.Text.Left(2) != "//") {
                            EditTextBox.Text = "[ " + EditTextBox.Text;
                        }
                    }
                    EditDefine.Comment = EditTextBox.Text;
                    NextUndo = new();
                    if (Inserting) {
                        NextUndo.UDAction = GlobalsUndo.udgActionType.udgAddDefine;
                        fgGlobals.CurrentRow.Cells[TypeCol].Value = EditDefine.Type;
                        fgGlobals.CurrentRow.Cells[DefaultCol].Value = "";
                        Inserting = false;
                    }
                    else {
                        NextUndo.UDAction = GlobalsUndo.udgActionType.udgEditComment;
                        NextUndo.UDText = fgGlobals.CurrentCell.Value.ToString();
                    }
                    NextUndo.UDCount = 1;
                    NextUndo.UDPos = fgGlobals.CurrentRow.Index;
                    AddUndo(NextUndo);
                    break;
                }
                fgGlobals.EndEdit();
                if (fgGlobals.CurrentCell.ColumnIndex < CommentCol) {
                    fgGlobals.CurrentCell = fgGlobals[fgGlobals.CurrentCell.ColumnIndex + 1, fgGlobals.CurrentCell.RowIndex];
                }
                else {
                    fgGlobals.CurrentCell = fgGlobals[NameCol, fgGlobals.CurrentCell.RowIndex + 1];
                }
                //if (forceedit) {
                if (Inserting) {
                    fgGlobals.BeginEdit(true);
                }
                MarkAsChanged();
                return;
            }
            if (fgGlobals.CurrentCell.ColumnIndex == NameCol) {
                // check for and ignore invalid characters
                if (invalidall.Contains((char)e.KeyValue)) {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                if (EditTextBox.SelectionStart == 0) {
                    if (invalid1st.Contains((char)e.KeyValue)) {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        return;
                    }
                }
                if (e.KeyValue > 122) {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyValue < 33 && e.KeyValue != 13 && e.KeyValue != 8) {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }
        }

        private void cmCel_Opening(object sender, CancelEventArgs e) {
            mnuCelUndo.Enabled = EditTextBox.CanUndo;
            mnuCelCut.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCopy.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelPaste.Enabled = Clipboard.ContainsText();
            mnuCelDelete.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCharMap.Visible = fgGlobals.CurrentCell.ColumnIndex != NameCol;
            mnuCelSelectAll.Enabled = EditTextBox.TextLength > 0;
        }

        private void mnuCelUndo_Click(object sender, EventArgs e) {
            if (EditTextBox.CanUndo) {
                EditTextBox.Undo();
            }
        }

        private void mnuCelCut_Click(object sender, EventArgs e) {
            if (EditTextBox.SelectionLength > 0) {
                EditTextBox.Cut();
            }
        }

        private void mnuCelCopy_Click(object sender, EventArgs e) {
            if (EditTextBox.SelectionLength > 0) {
                EditTextBox.Copy();
            }
        }

        private void mnuCelPaste_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText()) {
                EditTextBox.Paste();
                if (EditTextBox.Text.Contains("\r\n")) {
                    EditTextBox.Text = EditTextBox.Text.Replace("\r\n", "");
                }
                if (EditTextBox.Text.Contains('\r')) {
                    EditTextBox.Text = EditTextBox.Text.Replace("\r", "");
                }
                if (EditTextBox.Text.Contains('\n')) {
                    EditTextBox.Text = EditTextBox.Text.Replace("\n", "");
                }
            }
        }

        private void mnuCelDelete_Click(object sender, EventArgs e) {
            if (EditTextBox.SelectionLength > 0) {
                EditTextBox.SelectedText = "";
            }
        }

        private void mnuCelCharMap_Click(object sender, EventArgs e) {
            frmCharPicker CharPicker;
            if (EditGame != null) {
                CharPicker = new(EditGame.CodePage.CodePage);
            }
            else {
                CharPicker = new(WinAGISettings.DefCP.Value);
            }
            CharPicker.ShowDialog(MDIMain);
            if (!CharPicker.Cancel) {
                if (CharPicker.InsertString.Length > 0) {
                    EditTextBox.SelectedText = CharPicker.InsertString;
                }
            }
            CharPicker.Close();
            CharPicker.Dispose();
        }

        private void mnuCelSelectAll_Click(object sender, EventArgs e) {
            if (EditTextBox.TextLength > 0) {
                EditTextBox.SelectAll();
            }
        }

        private void mnuCelCancel_Click(object sender, EventArgs e) {
            EditTextBox.Hide();
            bool canx = fgGlobals.CancelEdit();
            // cancel alone doesn't work (the cell remains in edit mode)
            // but calling EndEdit immediately after seems to work
            fgGlobals.EndEdit();
        }
        #endregion

        /// <summary>
        /// Dynamic function to handle changes in displayed fonts used 
        /// by the editor.
        /// </summary>
        internal void InitFonts() {
            Font commonFont = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            fgGlobals.Font = commonFont;
            fgGlobals.ColumnHeadersDefaultCellStyle.Font = commonFont;
            fgGlobals.AlternatingRowsDefaultCellStyle.Font = commonFont;
            fgGlobals.AlternatingRowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        /// <summary>
        /// loads a defines list into the grid for editing
        /// </summary>
        /// <param name="GlobalFile"></param>
        /// <param name="ClearAll"></param>
        public bool LoadGlobalDefines(string GlobalFile, bool ingame) {
            bool blnError = false;
            string definetext;

            if (!File.Exists(GlobalFile)) {
                return false;
            }
            MDIMain.UseWaitCursor = true;
            try {
                // open file for input
                definetext = File.ReadAllText(GlobalFile);
            }
            catch (Exception) {
                //if error opening file, just exit
                MessageBox.Show(MDIMain,
                    "An error occurred while accessing this defines file. No defines were loaded.",
                    "Defines File Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            TDefine[] defines = ReadDefines(definetext, ref blnError);
            for (int i = 0; i < defines.Length; i++) {
                fgGlobals.Rows.Add(defines[i].Type, defines[i].Name, defines[i].Name, defines[i].Value, defines[i].Comment, defines[i].NameCheck, defines[i].ValueCheck);
            }
            Text = "Defines Editor for " + Path.GetFileName(GlobalFile);
            IsChanged = false;
            FileName = GlobalFile;
            MDIMain.UseWaitCursor = false;
            if (blnError) {
                // inform user
                MessageBox.Show(MDIMain,
                    "One or more define entries were improperly formatted. Some entries may be missing or incorrect.",
                    "Global Defines File Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            InGame = ingame;
            return true;
        }

        private TDefine[] ReadDefines(string definetext, ref bool errors) {
            errors = false;
            StringList strings = new StringList();
            string strLine;
            TDefine[] retval = [];
            TDefine tmpDef = new();
            strings.Add(definetext);
            for (int i = 0; i < strings.Count; i++) {
                strLine = strings[i].Replace((char)Keys.Tab, ' ').Trim();
                //trim it - also, skip comments
                string cmt = "";
                strLine = StripComments(strLine, ref cmt, true);
                //ignore blanks
                if (strLine.Length != 0) {
                    AGIToken token = WinAGIFCTB.TokenFromPos(strLine, 0);
                    if (token.Text.Equals("#define", StringComparison.OrdinalIgnoreCase)) {
                        token = WinAGIFCTB.NextToken(strLine, token);
                        tmpDef.Name = token.Text;
                        tmpDef.Value = WinAGIFCTB.NextToken(strLine, token).Text;
                        tmpDef.Comment = cmt;
                        DefineNameCheck validatename = ValidateDefineName(tmpDef.Name);
                        ArgType deftype = ArgType.None;
                        DefineValueCheck validatevalue = ValidateDefineValue(tmpDef.Value, ref deftype);
                        if ((validatename != DefineNameCheck.OK && validatename < DefineNameCheck.Global) ||
                            (validatevalue != DefineValueCheck.OK && validatevalue < DefineValueCheck.BadArgNumber)) {
                            // something wrong with this entry
                            errors = true;
                        }
                        else {
                            tmpDef.Type = deftype;
                            tmpDef.NameCheck = validatename;
                            tmpDef.ValueCheck = validatevalue;
                            Array.Resize(ref retval, retval.Length + 1);
                            retval[^1] = tmpDef;
                        }
                    }
                    else {
                        // something wrong with this entry
                        errors = true;
                    }
                }
            }
            return retval;
        }

        public void MenuClickSave(string savefile = "") {
            // save list of globals
            bool blnDirty = false;

            if (fgGlobals.IsCurrentCellInEditMode) {
                return;
            }
            if (savefile.Length == 0) {
                if (FileName.Length == 0) {
                    savefile = NewSaveFileName(this.FileName);
                }
                else {
                    savefile = FileName;
                }
            }
            if (savefile.Length == 0) {
                return;
            }
            MDIMain.UseWaitCursor = true;
            if (InGame) {
                // replace any changed defines with new names
                DialogResult rtn = DialogResult.No;
                bool blnDontAsk = false;
                switch (WinAGISettings.AutoUpdateDefines.Value) {
                case AskOption.Ask:
                    MDIMain.UseWaitCursor = false;
                    // get user's response
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to update all logics with any define names that have been changed?",
                        "Update Logics?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        "Always take this action when saving the global defines list.", ref blnDontAsk,
                        WinAGIHelp, "htm\\winagi\\editingdefines.htm#edit");
                    if (blnDontAsk) {
                        if (rtn == DialogResult.Yes) {
                            WinAGISettings.AutoUpdateDefines.Value = AskOption.Yes;
                        }
                        else {
                            WinAGISettings.AutoUpdateDefines.Value = AskOption.No;
                        }
                        WinAGISettings.AutoUpdateDefines.WriteSetting(WinAGISettingsFile);
                    }
                    break;
                case AskOption.No:
                    rtn = DialogResult.No;
                    break;
                case AskOption.Yes:
                    rtn = DialogResult.Yes;
                    break;
                }
                MDIMain.UseWaitCursor = true;
                // TODO: do I need error handling for load/source changes?
                if (rtn == DialogResult.Yes) {
                    // step through all defines; if the current name is different than
                    // the original name, use replaceall to make the change
                    ProgressWin = new() {
                        Text = "Updating Global Defines",
                    };
                    ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count + LogicEditors.Count + 1;
                    ProgressWin.pgbStatus.Value = 0;
                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                    ProgressWin.Show(MDIMain);
                    ProgressWin.Refresh();
                    foreach (frmLogicEdit loged in LogicEditors) {
                        if (loged.FormMode == LogicFormMode.Logic && loged.InGame) {
                            bool textchanged = false;
                            // check for deleted define names
                            for (int i = 0; i < DeletedDefines.Count; i++) {
                                // for regex to work, '$' chars have to be escaped
                                string FindText = DeletedDefines[i].Name.Replace("$", "\\$");
                                // this pattern ensures only whole tokens are found
                                string pattern = $@"(?<=^|[\n\r" + s_invalid1st + "])" + FindText + $@"(?=[" + s_invalidall + $@"\n\r]|$)";
                                if (Regex.IsMatch(loged.fctb.Text, pattern)) {
                                    textchanged = true;
                                    ProgressWin.lblProgress.Text = "Updating editor for " + loged.EditLogic.ID;
                                    ProgressWin.Refresh();
                                    loged.fctb.UpdateText(Regex.Replace(loged.fctb.Text, pattern, DeletedDefines[i].Value));
                                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                                    ProgressWin.Refresh();
                                }

                            }
                            for (int i = 0; i < fgGlobals.RowCount - 1; i++) {
                                // if name has changed
                                if (fgGlobals[DefaultCol, i].Value != fgGlobals[NameCol, i].Value && (string)fgGlobals[DefaultCol, i].Value != "") {
                                    // for regex to work, '$' chars have to be escaped
                                    string FindText = ((string)fgGlobals[DefaultCol, i].Value).Replace("$", "\\$");
                                    // this pattern ensures only whole tokens are found
                                    string pattern = $@"(?<=^|[\n\r" + s_invalid1st + "])" + FindText + $@"(?=[" + s_invalidall + $@"\n\r]|$)";
                                    if (Regex.IsMatch(loged.fctb.Text, pattern)) {
                                        textchanged = true;
                                        ProgressWin.lblProgress.Text = "Updating editor for " + loged.EditLogic.ID;
                                        ProgressWin.Refresh();
                                        //loged.fctb.Text = Regex.Replace(loged.fctb.Text, pattern, (string)fgGlobals[NameCol, i].Value);
                                        loged.fctb.UpdateText(Regex.Replace(loged.fctb.Text, pattern, (string)fgGlobals[NameCol, i].Value));
                                        ProgressWin.lblProgress.Text = "Locating modified define names...";
                                        ProgressWin.Refresh();
                                    }
                                }
                                // if the define value is a standard arg identifier
                                if ((ArgType)fgGlobals[TypeCol, i].Value >= (ArgType)1 &&
                                    (ArgType)fgGlobals[TypeCol, i].Value <= (ArgType)8) {
                                    string pattern = $@"\b" + (string)fgGlobals[ValueCol, i].Value + $@"\b";
                                    string replacetext = (string)fgGlobals[NameCol, i].Value;
                                    if (Regex.IsMatch(loged.fctb.Text, pattern)) {
                                        textchanged = true;
                                        ProgressWin.lblProgress.Text = "Updating editor for " + loged.EditLogic.ID;
                                        ProgressWin.Refresh();
                                        //loged.fctb.Text = Regex.Replace(loged.fctb.Text, pattern, replacetext, RegexOptions.None);
                                        loged.fctb.UpdateText(Regex.Replace(loged.fctb.Text, pattern, replacetext, RegexOptions.None));
                                        ProgressWin.lblProgress.Text = "Locating modified define names...";
                                        ProgressWin.Refresh();
                                    }
                                }
                            }
                            if (textchanged) {
                                // ? who cares for editors...
                            }
                        }
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.Refresh();
                    }
                    foreach (Logic logic in EditGame.Logics) {
                        bool unload = !logic.Loaded;
                        bool textchanged = false;
                        // check for deleted define names
                        for (int i = 0; i < DeletedDefines.Count; i++) {
                            if (unload) {
                                logic.Load();
                            }
                            // for regex to work, '$' chars have to be escaped
                            string FindText = DeletedDefines[i].Name.Replace("$", "\\$");
                            // this pattern ensures only whole tokens are found
                            string pattern = $@"(?<=^|[\n\r" + s_invalid1st + "])" + FindText + $@"(?=[" + s_invalidall + $@"\n\r]|$)";
                            if (Regex.IsMatch(logic.SourceText, pattern)) {
                                textchanged = true;
                                ProgressWin.lblProgress.Text = "Updating source code for " + logic.ID;
                                ProgressWin.Refresh();
                                unload = logic.Loaded;
                                logic.SourceText = Regex.Replace(logic.SourceText, pattern, DeletedDefines[i].Value);
                                ProgressWin.lblProgress.Text = "Locating modified define names...";
                                ProgressWin.Refresh();
                            }
                        }
                        for (int i = 0; i < fgGlobals.RowCount - 1; i++) {
                            // if name has changed
                            if (fgGlobals[DefaultCol, i].Value != fgGlobals[NameCol, i].Value && (string)fgGlobals[DefaultCol, i].Value != "") {
                                // load first
                                if (unload) {
                                    logic.Load();
                                }
                                // for regex to work, '$' chars have to be escaped
                                string FindText = ((string)fgGlobals[DefaultCol, i].Value).Replace("$", "\\$");
                                // this pattern ensures only whole tokens are found
                                string pattern = $@"(?<=^|[\n\r" + s_invalid1st + "])" + FindText + $@"(?=[" + s_invalidall + $@"\n\r]|$)";
                                if (Regex.IsMatch(logic.SourceText, pattern)) {
                                    textchanged = true;
                                    ProgressWin.lblProgress.Text = "Updating source code for " + logic.ID;
                                    ProgressWin.Refresh();
                                    unload = logic.Loaded;
                                    logic.SourceText = Regex.Replace(logic.SourceText, pattern, (string)fgGlobals[NameCol, i].Value);
                                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                                    ProgressWin.Refresh();
                                }
                            }
                            // if the define value is a standard arg identifier
                            if ((ArgType)fgGlobals[TypeCol, i].Value >= (ArgType)1 &&
                                    (ArgType)fgGlobals[TypeCol, i].Value <= (ArgType)8) {
                                if (unload) {
                                    logic.Load();
                                }
                                string pattern = $@"\b" + (string)fgGlobals[ValueCol, i].Value + $@"\b";
                                string replacetext = (string)fgGlobals[NameCol, i].Value;
                                if (Regex.IsMatch(logic.SourceText, pattern)) {
                                    textchanged = true;
                                    ProgressWin.lblProgress.Text = "Updating source code for " + logic.ID;
                                    ProgressWin.Refresh();
                                    logic.SourceText = Regex.Replace(logic.SourceText, pattern, replacetext, RegexOptions.None);
                                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                                    ProgressWin.Refresh();
                                }
                            }
                        }
                        if (textchanged) {
                            logic.SaveSource();
                            if (unload) {
                                logic.Unload();
                            }
                            //update reslist
                            RefreshTree(AGIResType.Logic, logic.Number);
                        }
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.Refresh();
                    }
                    for (int i = 0; i < fgGlobals.RowCount - 1; i++) {
                        if (fgGlobals[DefaultCol, i].Value != fgGlobals[NameCol, i].Value) {
                            fgGlobals[DefaultCol, i].Value = fgGlobals[NameCol, i].Value;
                        }
                    }
                    DeletedDefines.Clear();
                    ProgressWin.Text = "Save Defines List";
                }
                else {
                    ProgressWin = new() {
                        Text = "Save Defines List",
                    };
                    ProgressWin.Show(MDIMain);
                }
            }
            else {
                // not in a game; just save it
                ProgressWin = new() {
                    Text = "Save Defines List",
                };
                ProgressWin.Show(MDIMain);
            }
            // done updating logics, now save the list
            //ProgressWin.Text = "Save Defines List";
            ProgressWin.lblProgress.Text = "Saving defines to file ...";
            ProgressWin.pgbStatus.Value = 0;
            ProgressWin.pgbStatus.Maximum = fgGlobals.RowCount;
            ProgressWin.Refresh();
            StringList stlGlobals = BuildGlobalsFile(InGame, true);
            try {
                File.WriteAllLines(savefile, stlGlobals);
            }
            catch (Exception e) {
                ErrMsgBox(e, "Unable to save due to file error.", "", "File Error");
            }
            FileName = savefile;
            MarkAsSaved();
            // some extra things to do when saving an InGame list
            if (InGame) {
                // if at least one update
                if (blnDirty) {
                    MakeAllChanged();
                    //  TODO: need a strategy to ONLY mark affected logics;
                    //  should be able to track what gets added, removed or
                    //  updated
                }
            }
            ProgressWin.Close();
            ProgressWin.Dispose();
            MDIMain.UseWaitCursor = false;
        }

        private string NewSaveFileName(string filename = "") {
            DialogResult rtn;

            if (filename.Length != 0) {
                MDIMain.SaveDlg.Title = "Save Defines List";
                MDIMain.SaveDlg.FileName = Path.GetFileName(filename);
            }
            else {
                MDIMain.SaveDlg.Title = "Save Defines List As";
                MDIMain.SaveDlg.FileName = "";
            }
            MDIMain.SaveDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            MDIMain.SaveDlg.FilterIndex = 1;
            MDIMain.SaveDlg.DefaultExt = "txt";
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.OkRequiresInteraction = true;
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            if (rtn == DialogResult.Cancel) {
                // nothing selected
                return "";
            }
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            return MDIMain.SaveDlg.FileName;
        }

        private void AddUndo(GlobalsUndo NextUndo) {
            if (!IsChanged) {
                MarkAsChanged();
            }
            //// remove old undo items until there is room for this one
            //// to be added
            //if (Settings.GlobalUndo > 0) {
            //    while (UndoCol.Count >= Settings.GlobalUndo) {
            //        UndoCol.Remove(0);
            //    }
            //}
            UndoCol.Push(NextUndo);
        }

        public void AddGlobalsFromFile() {
            // adds to the existing globals list, instead of replacing it
            if (fgGlobals.IsCurrentCellInEditMode) {
                return;
            }
            // get a file
            MDIMain.OpenDlg.CheckPathExists = true;
            MDIMain.OpenDlg.FileName = "";
            MDIMain.OpenDlg.InitialDirectory = DefaultResDir;

            MDIMain.OpenDlg.Title = "Select Defines File to Import";
            MDIMain.OpenDlg.ShowReadOnly = false;
            MDIMain.OpenDlg.RestoreDirectory = true;
            MDIMain.OpenDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            MDIMain.OpenDlg.FilterIndex = 1;
            if (MDIMain.OpenDlg.ShowDialog(MDIMain) != DialogResult.OK) {
                return;
            }
            DefaultResDir = JustPath(MDIMain.OpenDlg.FileName);

            bool blnError = false;
            string definetext;
            try {
                //open file for input
                using FileStream fsGlobal = new(MDIMain.OpenDlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader srGlobal = new(fsGlobal);

                char[] chText = new char[fsGlobal.Length];
                srGlobal.ReadBlock(chText, 0, (int)fsGlobal.Length);
                fsGlobal.Dispose();
                srGlobal.Dispose();
                definetext = new string(chText);
            }
            catch (Exception) {
                //if error opening file, just exit
                MessageBox.Show(MDIMain,
                    "An error occurred while accessing this defines file. No defines were imported.",
                    "Defines File Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            GlobalsUndo NextUndo = new();
            NextUndo.UDAction = GlobalsUndo.udgActionType.udgImportDefines;
            NextUndo.UDPos = fgGlobals.NewRowIndex;
            TDefine[] addDefines = ReadDefines(definetext, ref blnError);
            if (addDefines.Length == 0) {
                // nothing to paste
                MessageBox.Show(MDIMain,
                    "There are no valid define entries on the clipboard.",
                    "Unable to Paste Defines",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            TDefine[] undodata = InsertDefines(addDefines, fgGlobals.NewRowIndex, ref blnError);
            // if nothing was added, no undo data exists
            if (undodata.Length == 0) {
                MessageBox.Show(MDIMain,
                    "There were no usable data in the file, so nothing was pasted.",
                    "Nothing to Paste",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else {
                NextUndo.UDCount = undodata.Length;
                for (int i = 0; i < undodata.Length; i++) {
                    NextUndo.UDDefine[i] = undodata[i];
                }
                AddUndo(NextUndo);
            }
            MDIMain.UseWaitCursor = false;
            if (blnError) {
                MessageBox.Show(MDIMain,
                    "Some entries could not be added because of formatting or syntax errors.",
                    "Paste Errors",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private TDefine[] InsertDefines(TDefine[] PasteDefines, int insertrow, ref bool errors) {
            TDefine tmpdef;
            TDefine[] retval = [];
            int replacerow;

            errors = false;
            for (int i = 0; i < PasteDefines.Length; i++) {
                // coming from internal clipboard means no concerns
                //  about formatting, so just validate and add it

                DefineNameCheck nameCheck = ValidateDefineName(PasteDefines[i].Name);
                ArgType t = ArgType.None;
                DefineValueCheck valueCheck = ValidateDefineValue(PasteDefines[i].Value, ref t);
                if (((int)nameCheck > 0 && (int)nameCheck <= 7) || ((int)valueCheck > 0 && (int)valueCheck <= 3)) {
                    errors = true;
                }
                else {
                    // check for defines that replace existing defines
                    if (nameCheck == DefineNameCheck.Global) {
                        string oldval = "";
                        for (replacerow = 0; replacerow < fgGlobals.RowCount; replacerow++) {
                            if ((string)fgGlobals[NameCol, replacerow].Value == PasteDefines[i].Name) {
                                oldval = (string)fgGlobals[ValueCol, replacerow].Value;
                                break;
                            }
                        }
                        if (oldval == PasteDefines[i].Value) {
                            // if the replacement has same value, just skip it
                            continue;
                        }
                        DialogResult rtn = DialogResult.Yes;
                        bool dontask = false;
                        switch (WinAGISettings.WarnDupGName.Value) {
                        case AskOption.Ask:
                            rtn = MsgBoxEx.Show(MDIMain,
                                "'" + PasteDefines[i].Name + "' is already defined in this list. Do you want to replace it with this define?",
                                "Duplicate Global Define",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                "Don't show this warning again.", ref dontask,
                                WinAGIHelp, "htm\\winagi\\Global Defines.htm#syntax");
                            if (dontask) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.WarnDupGName.Value = AskOption.Yes;
                                }
                                else {
                                    WinAGISettings.WarnDupGName.Value = AskOption.No;
                                }
                                WinAGISettings.WarnDupGName.WriteSetting(WinAGISettingsFile);
                            }
                            break;
                        case AskOption.Yes:
                            rtn = DialogResult.Yes;
                            break;
                        case AskOption.No:
                            rtn = DialogResult.No;
                            break;
                        }
                        if (rtn == DialogResult.Yes) {
                            // add a modified undo item to indicate the replacement
                            tmpdef = new();
                            tmpdef.Type = (ArgType)fgGlobals[TypeCol, replacerow].Value;
                            tmpdef.Name = replacerow.ToString();
                            tmpdef.Value = oldval;
                            tmpdef.Comment = (string)fgGlobals[CommentCol, replacerow].Value;
                            Array.Resize(ref retval, retval.Length + 1);
                            retval[^1] = tmpdef;
                            // make the replacement
                            fgGlobals[TypeCol, replacerow].Value = PasteDefines[i].Type;
                            fgGlobals[ValueCol, replacerow].Value = PasteDefines[i].Value;
                            fgGlobals[CommentCol, replacerow].Value = PasteDefines[i].Comment;
                        }
                        continue;
                    }
                    // any other condition is ok
                    tmpdef = new();
                    tmpdef.Name = insertrow.ToString();
                    Array.Resize(ref retval, retval.Length + 1);
                    retval[^1] = tmpdef;
                    // add a new row
                    fgGlobals.Rows.Insert(insertrow++,
                        PasteDefines[i].Type,
                        PasteDefines[i].Default,
                        PasteDefines[i].Name,
                        PasteDefines[i].Value,
                        PasteDefines[i].Comment,
                        PasteDefines[i].NameCheck,
                        PasteDefines[i].ValueCheck);
                }
            }
            return retval;
        }

        #region temp code
        /*
public void MenuClickHelp() {
  
  On Error GoTo ErrHandler
  
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\editingdefines.htm"
}

private void SortGlobals(ByVal lngCol As Long, NextUndo As GlobalsUndo)

  // sorts the global list by column; if column is -1, it means restore
  // the list from the current Undo object;
  Dim i As Long, j As Long
  Dim lngSwapRow As Long, lngCount As Long
  Dim strTemp As String, lngTemp As Long, blnBold As Boolean
  Dim lngOrder() As Long
  Dim tmpDefine As TDefine, blnSorted As Boolean
  
  On Error GoTo ErrHandler
  
  // number of items
  lngCount = fgGlobals.Rows - 2
  
  // if nothing to sort (need at least two)
  if (lngCount < 2) {
    return;
  }
  
  #if (DEBUGMODE != 1) {
    SendMessage fgGlobals.hWnd, WM_SETREDRAW, 0, 0
  #}
  
  // show wait cursor
  WaitCursor
  
  // use a temp array to track original order
  ReDim lngOrder(lngCount - 1)
  // if NOT resetting from an Undo object,
  if (lngCol != -1) {
    // create the undo object
    NextUndo = New GlobalsUndo
      NextUndo.UDAction = udgSort
      // can't access the members of the
      // array directly, so we will use
      // a local number array
      For i = 0 To lngCount - 1
        lngOrder(i) = i
      Next i
  } else {
    For i = 0 To lngCount - 1
      lngOrder(i) = NextUndo.UDDefine(i).Type
    Next i
  }
  
  // step through all rows except last
  For i = 1 To lngCount - 1
    // set swap row to starting row
    lngSwapRow = i
    // compare the swap row to all rows past this one
    For j = i + 1 To lngCount
      // should row j be above swaprow?
      Select Case lngCol
      Case ctName
        if (StrComp(fgGlobals.TextMatrix(j, ctName), fgGlobals.TextMatrix(lngSwapRow, ctName), vbTextCompare) = -1) {
          // j is new swap row
          lngSwapRow = j
        }
        
      Case ctValue
        // values are trickier to sort; need to take into account presence of plain numbers
        // as well as normal argument types and literal strings
        
        // if isnumeric,
        if (IsNumeric(fgGlobals.TextMatrix(j, ctValue))) {
          // if test row is a number, sort depends on what
          // the current swap row is;
          // if current swap row is NOT a number, always move the
          // number up
          // if current swap row IS a number, only move up if
          // the test row is less than swap row
          if (IsNumeric(fgGlobals.TextMatrix(lngSwapRow, ctValue))) {
            if (Val(fgGlobals.TextMatrix(j, ctValue)) < Val(fgGlobals.TextMatrix(lngSwapRow, ctValue))) {
              lngSwapRow = j
            }
          } else {
            lngSwapRow = j
          }
        } else {
          // if test row is NOT a number, sort depends on what the
          // current swap row is;
          // if current swap row is a number, don't swap
          // if current swap row is NOT a number, do a text compare
          // BUT, within each arg type, we want to sort by number value; not string value
          // (i.e. we want v1,v2,v11,v20... NOT v1,v11,v2,v20...
          if (Not IsNumeric(fgGlobals.TextMatrix(lngSwapRow, ctValue))) {
            // check first letter ONLY at first
            if (StrComp(Left$(fgGlobals.TextMatrix(j, ctValue), 1), Left$(fgGlobals.TextMatrix(lngSwapRow, ctValue), 1), vbTextCompare) = -1) {
              // swap
              lngSwapRow = j
              
            // if both first letters are the same (this will also handle string assignments)
            } else if ( StrComp(Left$(fgGlobals.TextMatrix(j, ctValue), 1), Left$(fgGlobals.TextMatrix(lngSwapRow, ctValue), 1), vbTextCompare) = 0) {
              // are both numeric?
              if (IsNumeric(Right$(fgGlobals.TextMatrix(j, ctValue), Len(fgGlobals.TextMatrix(j, ctValue)) - 1)) And IsNumeric(Right$(fgGlobals.TextMatrix(lngSwapRow, ctValue), Len(fgGlobals.TextMatrix(lngSwapRow, ctValue)) - 1))) {
                // swap only if Value of j row is less than Value of swap row
                if (Val(Right$(fgGlobals.TextMatrix(j, ctValue), Len(fgGlobals.TextMatrix(j, ctValue)) - 1)) < Val(Right$(fgGlobals.TextMatrix(lngSwapRow, ctValue), Len(fgGlobals.TextMatrix(lngSwapRow, ctValue)) - 1))) {
                  lngSwapRow = j
                }
              } else {
                // swap if string is less than
                if (StrComp(fgGlobals.TextMatrix(j, ctValue), fgGlobals.TextMatrix(lngSwapRow, ctValue), vbTextCompare) = -1) {
                  lngSwapRow = j
                }
              }
            }
          }
        }
       
      Case -1
        // when restoring from undo, we only care about what original order was - much easier to determine
        // if swap is required!
        if (lngOrder(j - 1) < lngOrder(lngSwapRow - 1)) {
          // j is the new swap row
          lngSwapRow = j
        }
      End Select
    Next j
    
    // if rows need to be swapped
    if (lngSwapRow != i) {
        // swap name
        strTemp = fgGlobals.TextMatrix(i, ctName)
        fgGlobals.TextMatrix(i, ctName) = fgGlobals.TextMatrix(lngSwapRow, ctName)
        fgGlobals.TextMatrix(lngSwapRow, ctName) = strTemp
        // swap Value
        strTemp = fgGlobals.TextMatrix(i, ctValue)
        fgGlobals.TextMatrix(i, ctValue) = fgGlobals.TextMatrix(lngSwapRow, ctValue)
        fgGlobals.TextMatrix(lngSwapRow, ctValue) = strTemp
        // swap original name
        strTemp = fgGlobals.TextMatrix(i, 0)
        fgGlobals.TextMatrix(i, 0) = fgGlobals.TextMatrix(lngSwapRow, 0)
        fgGlobals.TextMatrix(lngSwapRow, 0) = strTemp
        // swap comment
        strTemp = fgGlobals.TextMatrix(i, ctComment)
        fgGlobals.TextMatrix(i, ctComment) = fgGlobals.TextMatrix(lngSwapRow, ctComment)
        fgGlobals.TextMatrix(lngSwapRow, ctComment) = strTemp
        // also swap the order list
        lngTemp = lngOrder(i - 1)
        lngOrder(i - 1) = lngOrder(lngSwapRow - 1)
        lngOrder(lngSwapRow - 1) = lngTemp
        
        // if either row, but not both, contain
        // an override, then the color and
        // bold status of the two rows needs to
        // be swapped
        // (only name column gets highlighted)
        fgGlobals.Col = ctName
        
        // check lngSwapRow first
        fgGlobals.Row = lngSwapRow
        // note whether this cell is an override or not
        blnBold = .CellFontBold
        
        // now select i row
        fgGlobals.Row = i
        // if override status is different
        // then we need to swap them
        if (.CellFontBold != blnBold) {
          // make this row match blnBold
          fgGlobals.CellFontBold = blnBold
          if (blnBold) {
            fgGlobals.CellForeColor = vbRed
          } else {
            fgGlobals.CellForeColor = vbBlack
          }
          // then toggle the swaprow
          fgGlobals.Row = lngSwapRow
          fgGlobals.CellFontBold = Not blnBold
          if (Not blnBold) {
            fgGlobals.CellForeColor = vbRed
          } else {
            fgGlobals.CellForeColor = vbBlack
          }
        }
      
      blnSorted = true
      MarkAsChanged
    }
  Next i
  
  // as long as something was sorted, continue
  if (blnSorted) {
    // select first item
    fgGlobals.Row = 1
    fgGlobals.Col = ctName
    if (Not fgGlobals.RowIsVisible(1)) {
      fgGlobals.TopRow = fgGlobals.Row
    }
    
    // if saving for an undo,
    if (lngCol != -1) {
      // copy the results of the sort into the undo object
        NextUndo.UDCount = lngCount
        For i = 0 To lngCount - 1
          tmpDefine.Type = lngOrder(i)
          NextUndo.UDDefine(i) = tmpDefine
        Next i
      // add the undo
      AddUndo NextUndo
    }
  }
  
  #if (DEBUGMODE != 1) {
    SendMessage fgGlobals.hWnd, WM_SETREDRAW, 1, 0
  #}
  fgGlobals.Refresh
  NextUndo = Nothing
  Screen.MousePointer = vbDefault
return;

ErrHandler:
  // *'Debug.Assert false
  Resume Next
}
        */
        #endregion

        public DefineNameCheck ValidateDefineName(string checkname) {
            // basic checks
            // if no name,
            if (checkname.Length == 0) {
                return DefineNameCheck.Empty;
            }
            // name can't be numeric
            if (checkname.IsNumeric()) {
                return DefineNameCheck.Numeric;
            }
            // check name against improper character list
            if (invalid1st.Any(ch => ch == checkname[0])) {
                return DefineNameCheck.BadChar;
            }
            if (checkname[1..].Any(invalidall.Contains)) {
                return DefineNameCheck.BadChar;
            }
            if (checkname.Any(ch => ch > 127 || ch < 32)) {
                return DefineNameCheck.BadChar;
            }
            // check against regular commands
            for (int i = 0; i < ActionCount; i++) {
                if (checkname == ActionCommands[i].Name) {
                    return DefineNameCheck.ActionCommand;
                }
            }
            // check against test commands
            for (int i = 0; i < TestCount; i++) {
                if (checkname == TestCommands[i].Name) {
                    return DefineNameCheck.TestCommand;
                }
            }
            // check against compiler keywords
            if (checkname is "if" or "else" or "goto") {
                return DefineNameCheck.KeyWord;
            }
            // if the name starts with any of these letters
            // (OK for sierra syntax)
            if (EditGame == null || (EditGame != null && EditGame.SierraSyntax)) {
                if ("vfmoiswc".Any(checkname.StartsWith)) {
                    if (checkname.Right(checkname.Length - 1).IsNumeric()) {
                        // can't have a name that's a valid marker
                        return DefineNameCheck.ArgMarker;
                    }
                }
            }
            // check against globals in this list
            for (int i = 0; i < fgGlobals.Rows.Count; i++) {
                // skip empty rows
                if (i != fgGlobals.NewRowIndex) {
                    if (i != fgGlobals.CurrentRow.Index && fgGlobals[NameCol, i].Value.ToString() == checkname)
                        return DefineNameCheck.Global;
                }
            }
            // check against basic reserved
            if (EditGame is not null && EditGame.IncludeReserved) {
                // reserved variables
                TDefine[] tmpDefines = LogicCompiler.ReservedDefines(Var);
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedVar;
                    }
                }
                // reserved flags
                tmpDefines = LogicCompiler.ReservedDefines(Flag);
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedFlag;
                    }
                }
                // reserved numbers
                tmpDefines = LogicCompiler.ReservedDefines(Num);
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedNum;
                    }
                }
                // reserved objects
                tmpDefines = LogicCompiler.ReservedDefines(SObj);
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedObj;
                    }
                }
                // reserved strings
                tmpDefines = LogicCompiler.ReservedDefines(Str);
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedStr;
                    }
                }
                // game-specific:
                for (int i = 0; i < EditGame.ReservedGameDefines.Length; i++) {
                    if (checkname == EditGame.ReservedGameDefines[i].Name)
                        // invobj count is number; rest are msgstrings
                        return i == 3 ? DefineNameCheck.ReservedNum : DefineNameCheck.ReservedMsg;
                }
            }
            // resourceIDs
            if (EditGame != null && EditGame.IncludeIDs) {
                //if (!blnSetIDs) {
                //    SetResourceIDs(EditGame);
                //}
                foreach (Logic logic in EditGame.Logics) {
                    if (checkname == logic.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
                foreach (Picture picture in EditGame.Pictures) {
                    if (checkname == picture.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
                foreach (Sound sound in EditGame.Sounds) {
                    if (checkname == sound.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
                foreach (Engine.View view in EditGame.Views) {
                    if (checkname == view.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        public DefineValueCheck ValidateDefineValue(string checkvalue, ref ArgType type) {
            // default type
            type = ArgType.None;

            if (checkvalue.Length == 0) {
                return DefineValueCheck.Empty;
            }
            // values must be an AGI argument marker (variable/flag/etc), string, or a number

            // if NOT a number:
            if (!int.TryParse(checkvalue, out int intVal)) {
                if ("vfmoiswc".Contains(checkvalue[0])) {
                    string strVal = checkvalue[1..];
                    if (int.TryParse(strVal, out intVal)) {
                        if (intVal < 0 || intVal > 255) {
                            return DefineValueCheck.OutofBounds;
                        }
                        // determine type
                        switch (checkvalue[0]) {
                        case 'f':
                            type = Flag;
                            break;
                        case 'v':
                            type = Var;
                            break;
                        case 'm':
                            type = Msg;
                            break;
                        case 'o':
                            type = SObj;
                            break;
                        case 'i':
                            type = InvItem;
                            break;
                        case 's':
                            type = Str;
                            break;
                        case 'w':
                            type = Word;
                            break;
                        case 'c':
                            type = Ctrl;
                            break;
                        }
                        // check defined globals
                        for (int i = 0; i < fgGlobals.Rows.Count; i++) {
                            if (i != fgGlobals.NewRowIndex) {
                                if (i != fgGlobals.CurrentRow.Index && fgGlobals[ValueCol, i].Value.ToString() == checkvalue)
                                    return DefineValueCheck.Global;
                            }
                        }
                        // check reserved
                        if (EditGame != null && EditGame.IncludeReserved) {
                            switch (type) {
                            case Flag:
                                if (intVal <= 15)
                                    return DefineValueCheck.Reserved;
                                if (intVal == 20) {
                                    switch (EditGame.InterpreterVersion) {
                                    case "3.002.098" or "3.002.102" or "3.002.107" or "3.002.149":
                                        return DefineValueCheck.Reserved;
                                    }
                                }
                                break;
                            case Var:
                                if (intVal <= 26)
                                    return DefineValueCheck.Reserved;
                                break;
                            case Msg:
                                break;
                            case SObj:
                                if (intVal == 0)
                                    return DefineValueCheck.Reserved;
                                break;
                            case InvItem:
                                break;
                            case Str:
                                if (intVal > 23 || (intVal > 11 &&
                                    (EditGame.InterpreterVersion == "2.089" ||
                                    EditGame.InterpreterVersion == "2.272" ||
                                    EditGame.InterpreterVersion == "3.002149"))) {
                                    return DefineValueCheck.BadArgNumber;
                                }

                                break;
                            case Word:
                                // valid from w1 to w10
                                // applies to fanAGI syntax only;
                                // base is 1 because of how msg formatting
                                // uses words; compiler will automatically
                                // convert it to base zero when used - see
                                // WinAGI help file for more details
                                if (intVal < 1 || intVal > 10) {
                                    return DefineValueCheck.BadArgNumber;
                                }
                                break;
                            case Ctrl:
                                // controllers limited to 0-49
                                if (intVal > 49) {
                                    return DefineValueCheck.BadArgNumber;
                                }
                                break;
                            }
                        }
                        // not already defined - value is OK
                        return DefineValueCheck.OK;
                    }
                }
                // non-numeric, non-marker and most likely a string
                if (LogicCompiler.IsAGIString(checkvalue)) {
                    type = DefStr;
                    return DefineValueCheck.OK;
                }
                else {
                    return DefineValueCheck.NotAValue;
                }
            }
            else {
                // numeric
                // unsigned byte (0-255) or signed byte (-128 to 127) are OK
                if (intVal > -128 && intVal < 256) {
                    type = Num;
                    return DefineValueCheck.OK;
                }
                else {
                    return DefineValueCheck.OutofBounds;
                }
            }
        }

        public StringList BuildGlobalsFile(bool UpdateLookup, bool Progress = false) {
            string strName, strValue, strComment;
            int lngMaxLen, lngMaxV = 0;
            TDefine tmpDef = new();
            StringList tmpStrList;

            // determine longest name length to facilitate aligning values
            lngMaxLen = 0;
            for (int i = 0; i < fgGlobals.Rows.Count - 1; i++) {
                if (((string)fgGlobals[NameCol, i].Value).Length > lngMaxLen) {
                    lngMaxLen = ((string)fgGlobals[NameCol, i].Value).Length;
                }
                // right-align non-strings; need to know length of longest
                // non-string
                if (((string)fgGlobals[ValueCol, i].Value)[0] != '"') {
                    if (((string)fgGlobals[ValueCol, i].Value).Length > lngMaxV) {
                        lngMaxV = ((string)fgGlobals[ValueCol, i].Value).Length;
                    }
                }
            }
            // if also updating the logic tooltip lookup list
            if (UpdateLookup) {
                // if no defines
                if (fgGlobals.Rows.Count == 0) {
                    GDefLookup = Array.Empty<TDefine>();
                }
                else {
                    GDefLookup = new TDefine[fgGlobals.Rows.Count - 1];
                }
            }
            tmpStrList = [];
            // add a useful header
            tmpStrList.Add("[");
            tmpStrList.Add("[ global defines file for " + EditGame.GameID);
            tmpStrList.Add("[");
            if (Progress) {
                ProgressWin.pgbStatus.Value = 1;
            }
            for (int i = 0; i < fgGlobals.Rows.Count - 1; i++) {
                // get name and Value
                strName = ((string)fgGlobals[NameCol, i].Value).PadRight(lngMaxLen);
                strValue = ((string)fgGlobals[ValueCol, i].Value).PadLeft(4);
                // right align non-strings
                if (strValue[0] != '"') {
                    strValue = strValue.PadLeft(lngMaxV);
                }
                strComment = (string)fgGlobals[CommentCol, i].Value;
                if (strComment.Length > 0) {
                    strComment = " " + strComment;
                }
                tmpStrList.Add("#define " + strName + "  " + strValue + strComment);
                if (UpdateLookup) {
                    // update the global lookup list, if this list is part of a game
                    tmpDef.Type = (ArgType)fgGlobals[TypeCol, i].Value;
                    tmpDef.Default = (string)fgGlobals[NameCol, i].Value;
                    tmpDef.Name = (string)fgGlobals[NameCol, i].Value;
                    tmpDef.Value = (string)fgGlobals[ValueCol, i].Value;
                    tmpDef.NameCheck = (DefineNameCheck)fgGlobals[NameCheckCol, i].Value;
                    tmpDef.ValueCheck = (DefineValueCheck)fgGlobals[ValueCheckCol, i].Value;
                    GDefLookup[i] = tmpDef;
                }
                if (Progress) {
                    ProgressWin.pgbStatus.Value++;
                    ProgressWin.Refresh();
                }
            }
            return tmpStrList;
        }

        private void RemoveRows(int TopRow, int BtmRow, bool DontUndo = false) {
            if (BtmRow < TopRow) {
                int swap = BtmRow;
                BtmRow = TopRow;
                TopRow = swap;
            }
            if (!DontUndo) {
                GlobalsUndo NextUndo = new GlobalsUndo();
                TDefine tmpDef = new();
                NextUndo.UDAction = GlobalsUndo.udgActionType.udgDeleteDefine;
                NextUndo.UDCount = BtmRow - TopRow + 1;
                NextUndo.UDPos = TopRow;
                for (int i = 0; i <= BtmRow - TopRow; i++) {
                    tmpDef.Type = (ArgType)fgGlobals[TypeCol, TopRow + i].Value;
                    tmpDef.Default = (string)fgGlobals[DefaultCol, TopRow + i].Value;
                    tmpDef.Name = (string)fgGlobals[NameCol, TopRow + i].Value;
                    tmpDef.Value = (string)fgGlobals[ValueCol, TopRow + i].Value;
                    tmpDef.Comment = (string)fgGlobals[CommentCol, TopRow + i].Value;
                    NextUndo.UDDefine[i] = tmpDef;
                    if (tmpDef.Default.Length > 0) {
                        DelDefine deldef = new DelDefine {
                            Name = tmpDef.Default,
                            Value = tmpDef.Value
                        };
                        DeletedDefines.Add(deldef);
                    }
                }
                // add to undo
                AddUndo(NextUndo);
            }
            // delete the selected rows
            // (go backwards so rows are deleted correctly)
            for (int i = BtmRow; i >= TopRow; i--) {
                fgGlobals.Rows.RemoveAt(i);
            }
            fgGlobals.Refresh();
        }

        private void RemoveRow(int RowIndex, bool DontUndo = false) {
            if (!DontUndo) {
                GlobalsUndo NextUndo = new GlobalsUndo();
                NextUndo.UDAction = GlobalsUndo.udgActionType.udgDeleteDefine;
                NextUndo.UDCount = 1;
                NextUndo.UDPos = RowIndex;
                TDefine tmpDef = new();
                tmpDef.Type = (ArgType)fgGlobals[TypeCol, RowIndex].Value;
                tmpDef.Default = (string)fgGlobals[DefaultCol, RowIndex].Value;
                tmpDef.Name = (string)fgGlobals[NameCol, RowIndex].Value;
                tmpDef.Value = (string)fgGlobals[ValueCol, RowIndex].Value;
                tmpDef.Comment = (string)fgGlobals[CommentCol, RowIndex].Value;
                NextUndo.UDDefine[0] = tmpDef;
                AddUndo(NextUndo);
                if (tmpDef.Default.Length > 0) {
                    DelDefine deldef = new DelDefine();
                    deldef.Name = tmpDef.Default;
                    deldef.Value = tmpDef.Default;
                    DeletedDefines.Add(deldef);
                }
            }
            fgGlobals.Rows.RemoveAt(RowIndex);
            MarkAsChanged();
        }

        private bool AskClose() {
            //if (forcing) {
            //    // force shutdown
            //    return true;
            //}
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save this defines list?",
                    "Save Defines List",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    //SaveDefinesList();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "File not saved. Continue closing anyway?",
                            "Save Defines List",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        return rtn == DialogResult.Yes;
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
                case DialogResult.No:
                    return true;
                }
            }
            return true;
        }

        public void UpdateStatusBar() {

            // TODO: status bars...
        }

        void MarkAsChanged() {
            // ignore when loading (not visible yet)
            if (!Visible) {
                return;
            }
            if (!IsChanged) {
                IsChanged = true;
                mnuRSave.Enabled = true;
                MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = true;
                Text = sDM + Text;
            }
            FindingForm.ResetSearch();
        }

        private void MarkAsSaved() {
            Text = "Defines Editor for " + Path.GetFileName(FileName);
            IsChanged = false;
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
        }

        private void fgGlobals_Scroll(object sender, ScrollEventArgs e) {
            if (fgGlobals.IsCurrentCellInEditMode) {
                e.NewValue = e.OldValue;
            }
        }

        private void fgGlobals_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {

            if (fgGlobals[TypeCol, e.RowIndex].Value == null) {
                fgGlobals[TypeCol, e.RowIndex].Value = ArgType.None;
            }
            if (fgGlobals[DefaultCol, e.RowIndex].Value == null) {
                fgGlobals[DefaultCol, e.RowIndex].Value = "";
            }
            if (fgGlobals[NameCheckCol, e.RowIndex].Value == null) {
                fgGlobals[NameCheckCol, e.RowIndex].Value = DefineNameCheck.OK;
            }
            if (fgGlobals[ValueCheckCol, e.RowIndex].Value == null) {
                fgGlobals[ValueCheckCol, e.RowIndex].Value = DefineValueCheck.OK;
            }
        }
    }

    public class DefineColumn : DataGridViewColumn {
        public DefineColumn() : base(new DefineCell()) {
        }

        public override DataGridViewCell CellTemplate {
            get {
                return base.CellTemplate;
            }
            set {
                if (value != null &&
                    !value.GetType().IsAssignableFrom(typeof(DefineCell))) {
                    throw new InvalidCastException("Must be a DefineCell");
                }
                base.CellTemplate = value;
            }
        }
    }

    public class DefineCell : DataGridViewTextBoxCell {

        public DefineCell()
            : base() {
            // no custom initialization needed
        }

        public override void InitializeEditingControl(int rowIndex, object
            initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
            // Set the value of the editing control to the current cell value.
            base.InitializeEditingControl(rowIndex, initialFormattedValue,
                dataGridViewCellStyle);
            DefineEditingControl ctl =
                DataGridView.EditingControl as DefineEditingControl;
            // Use the default row value when Value property is null.
            if (this.Value == null) {
                ctl.Text = "";
            }
            else {
                ctl.Text = (string)this.Value;
            }
        }

        public override Type EditType {
            get {
                return typeof(DefineEditingControl);
            }
        }

        public override Type ValueType {
            get {
                return typeof(string);
            }
        }

        public override object DefaultNewRowValue {
            get {
                return "";
            }
        }
    }

    class DefineEditingControl : TextBox, IDataGridViewEditingControl {
        DataGridView dataGridView;
        private bool valueChanged = false;
        int rowIndex;

        public DefineEditingControl() {
            base.Multiline = true;  
            base.AcceptsReturn = true;
            base.AcceptsTab = true;
        }

        // Implements the IDataGridViewEditingControl.EditingControlFormattedValue
        // property.
        public object EditingControlFormattedValue {
            get {
                return this.Text;
            }
            set {
                if (value is String) {
                    try {
                        this.Text = (string)value;
                    }
                    catch {
                        // In the case of an exception, just use the
                        // default value so we're not left with a null
                        // value.
                        this.Text = "";
                    }
                }
            }
        }

        // Implements the
        // IDataGridViewEditingControl.GetEditingControlFormattedValue method.
        public object GetEditingControlFormattedValue(
            DataGridViewDataErrorContexts context) {
            return EditingControlFormattedValue;
        }

        // Implements the
        // IDataGridViewEditingControl.ApplyCellStyleToEditingControl method.
        public void ApplyCellStyleToEditingControl(
            DataGridViewCellStyle dataGridViewCellStyle) {
            this.Font = dataGridViewCellStyle.Font;
        }

        // Implements the IDataGridViewEditingControl.EditingControlRowIndex
        // property.
        public int EditingControlRowIndex {
            get {
                return rowIndex;
            }
            set {
                rowIndex = value;
            }
        }

        // Implements the IDataGridViewEditingControl.EditingControlWantsInputKey
        // method.
        public bool EditingControlWantsInputKey(
            Keys key, bool dataGridViewWantsInputKey) {
            // Let the DateTimePicker handle the keys listed.
            switch (key & Keys.KeyCode) {
            case Keys.Tab:
            case Keys.Enter:
            case Keys.Escape:
            case Keys.Left:
            case Keys.Up:
            case Keys.Down:
            case Keys.Right:
            case Keys.Home:
            case Keys.End:
            case Keys.PageDown:
            case Keys.PageUp:
                return true;
            default:
                return !dataGridViewWantsInputKey;
            }
        }

        // Implements the IDataGridViewEditingControl.PrepareEditingControlForEdit
        // method.
        public void PrepareEditingControlForEdit(bool selectAll) {
            // No preparation needs to be done.
        }

        // Implements the IDataGridViewEditingControl
        // .RepositionEditingControlOnValueChange property.
        public bool RepositionEditingControlOnValueChange {
            get {
                return false;
            }
        }

        // Implements the IDataGridViewEditingControl
        // .EditingControlDataGridView property.
        public DataGridView EditingControlDataGridView {
            get {
                return dataGridView;
            }
            set {
                dataGridView = value;
            }
        }

        // Implements the IDataGridViewEditingControl.EditingControlValueChanged property.
        public bool EditingControlValueChanged {
            get {
                return valueChanged;
            }
            set {
                valueChanged = value;
            }
        }

        // Implements the IDataGridViewEditingControl.EditingPanelCursor property.
        public Cursor EditingPanelCursor {
            get {
                return base.Cursor;
            }
        }

        protected override void OnTextChanged(EventArgs eventargs) {
            // Notify the DataGridView that the contents of the cell
            // have changed.
            valueChanged = true;
            this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            base.OnTextChanged(eventargs);
        }
    }
}
