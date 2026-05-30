using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.ArgType;

namespace WinAGI.Editor {
    public partial class frmGlobals : Form {

        #region Structs
        public struct DelDefine {
            public string Name;
            public string Value;
        }
        #endregion

        #region Members
        public bool InGame = false;
        public bool IsChanged = false;
        public string Filename = "";
        private Define EditDefine;
        private bool Inserting = false;
        private Stack<GlobalsUndo> UndoCol = [];
        private List<DelDefine> DeletedDefines = [];
        private const string DEF_MARKER = "#define ";
        private TextBox EditTextBox = null;
        private readonly char[] invalidall;
        private readonly char[] invalid1st;
        private const int ARGTYPE_COL = 0;
        private const int DEFNAME_COL = 1;
        private const int DEFVALUE_COL = 2;
        private const int NAME_COL = 3;
        private const int VALUE_COL = 4;
        internal const int COMMENT_COL = 5;
        private const int NAMETYPE_COL = 6;
        private const int VALUETYPE_COL = 7;

        // support for row dragging/dropping; these all need to be
        // available to all methods because the values are needed
        // in multiple events; also, start/end/target can't be reset
        // until after the PostPaint event, whichis the last method
        // in the drag operation that needs them
        private int dragStartRow = -1, dragEndRow = -1;
        private int targetRowIndex = -1;
        private bool canceldrag = false, dropping = false;
        private int UID = 0; // used to track original rows for undo when dragging/sorting
        private Timer dragTimer;
        private int dragDirection = 0; // -1 is up, 1 is down, 0 is no movement
        private List<int> currentOrder = [];
        // fonts used in the grid
        Font commonFont;
        Font boldFont;
        Font italicFont;

        // StatusStrip Items
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        #endregion

        #region Constructors
        /// <summary>
        /// A blank, default globals editor.
        /// </summary>
        public frmGlobals() {
            InitializeComponent();
            globalsgrid.QueryContinueDrag += globalsgrid_QueryContinueDrag;
            dragTimer = new Timer();
            dragTimer.Interval = 100;
            dragTimer.Tick += DragTimer_Tick;

            InitStatusStrip();
            MdiParent = MDIMain;
            InitFonts();
            invalid1st = INVALID_FIRST_CHARS;
            invalidall = INVALID_DEFINE_CHARS;
            DataGridViewRow template = new();
            template.CreateCells(globalsgrid);
            template.Cells[ARGTYPE_COL].Value = None;
            template.Cells[DEFNAME_COL].Value = "";
            template.Cells[DEFVALUE_COL].Value = "";
            template.Cells[NAME_COL].Value = "";
            template.Cells[VALUE_COL].Value = "";
            template.Cells[COMMENT_COL].Value = "";
            template.Cells[NAMETYPE_COL].Value = DefineNameCheck.OK;
            template.Cells[VALUETYPE_COL].Value = DefineValueCheck.OK;
            globalsgrid.RowTemplate = template;

            // initialize top row
            globalsgrid.Rows[0].Cells[ARGTYPE_COL].Value = None;
            globalsgrid.Rows[0].Cells[DEFNAME_COL].Value = "";
            globalsgrid.Rows[0].Cells[DEFVALUE_COL].Value = "";
            globalsgrid.Rows[0].Cells[NAME_COL].Value = "";
            globalsgrid.Rows[0].Cells[VALUE_COL].Value = "";
            globalsgrid.Rows[0].Cells[COMMENT_COL].Value = "";
            globalsgrid.Rows[0].Cells[NAMETYPE_COL].Value = DefineNameCheck.OK;
            globalsgrid.Rows[0].Cells[VALUETYPE_COL].Value = DefineValueCheck.OK;
        }
        #endregion

        #region Event Handlers
        #region Form Events
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // Enter key is usually captured by the grid, but we want it to go to the textbox
            if (keyData == Keys.Enter && EditTextBox is not null) {
                if (EditTextBox.Focused) {
                    EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Enter));
                    return true;
                }
            }
            // Escape key is usually captured by the grid, but we want it to go to the textbox
            if (keyData == Keys.Escape) {
                if (EditTextBox.Focused) {
                    EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Escape));
                    return true;
                }
            }
            if (keyData == Keys.Tab) {
                if (EditTextBox.Focused) {
                    if (globalsgrid.IsCurrentCellInEditMode) {
                        EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Tab));
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void frmGlobals_Leave(object sender, EventArgs e) {
            // if editing, need to cancel; otherwise, the edit text box
            // control stays active, and any other form will not be able
            // to ediit its grid cells
            if (globalsgrid.IsCurrentCellInEditMode) {
                // same as pressing Escape
                EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Escape));
            }
        }

        private void frmGlobals_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible) {
                FindingForm.Visible = false;
            }
            if (MDIMain.infoGridScope == InfoGridScope.SelectedResource) {
                MDIMain.RefreshInfoGrid();
            }
        }

        private void frmGlobals_FormClosing(object sender, FormClosingEventArgs e) {
            // cancel editing
            if (globalsgrid.IsCurrentCellInEditMode) {
                globalsgrid.CancelEdit();
            }
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            e.Cancel = !AskClose();
        }

        private void frmGlobals_FormClosed(object sender, FormClosedEventArgs e) {
            // save column fraction values
            int width;
            if (globalsgrid.Columns[COMMENT_COL].Visible) {
                width = globalsgrid.Columns[NAME_COL].Width + globalsgrid.Columns[VALUE_COL].Width + globalsgrid.Columns[COMMENT_COL].Width;
            }
            else {
                width = globalsgrid.Columns[NAME_COL].Width + globalsgrid.Columns[VALUE_COL].Width;
            }
            double namefrac = (double)globalsgrid.Columns[NAME_COL].Width / width;
            double valuefrac = (double)globalsgrid.Columns[VALUE_COL].Width / width;
            if (WinAGISettings.GEShowComment.Value) {
                // save both fractions, adjusting if comments currently hidden
                if ((double)globalsgrid.Columns[COMMENT_COL].Width / width < 0.05) {
                    // assume comments will be 40% when reopened
                    namefrac = namefrac * 0.6 / (namefrac + valuefrac);
                    valuefrac = 0.6 - namefrac;
                }
            }
            else {
                // only save name fraction
                valuefrac = 0;
            }
            WinAGISettings.GENameFrac.Value = Math.Round(namefrac, 4);
            WinAGISettings.GEValFrac.Value = Math.Round(valuefrac, 4);
            WinAGISettings.GENameFrac.WriteSetting(WinAGISettingsFile);
            WinAGISettings.GEValFrac.WriteSetting(WinAGISettingsFile);
            if (InGame) {
                GEInUse = false;
                GlobalsEditor = null;
            }
            EditTextBox?.Dispose();
            EditTextBox = null;
        }

        private void frmGlobals_HelpRequested(object sender, HelpEventArgs hlpevent) {
            ShowHelp();
            hlpevent.Handled = true;
        }
        #endregion

        #region Menu Events
        /// <summary>
        /// Configures the resource menu prior to displaying it.
        /// </summary>
        internal void SetResourceMenu() {
            MDIMain.mnuRSep2.Visible = true;
            MDIMain.mnuRSep3.Visible = true;
            mnuRInGame.Enabled = false;
            mnuRInGame.Text = InGame ? "Remove from Game" : "Add to Game";
            mnuRSave.Enabled = IsChanged;
        }

        /// <summary>
        /// Resets all resource menu items so shortcut keys can work correctly.
        /// </summary>
        internal void ResetResourceMenu() {
            mnuRSave.Enabled = true;
            mnuRSaveAs.Enabled = true;
            mnuRInGame.Enabled = true;
            mnuRAddFile.Enabled = true;
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            if (IsChanged) {
                SaveDefinesList();
            }
        }

        internal void mnuRSaveAs_Click(object sender, EventArgs e) {
            string filename = NewSaveFileName();
            if (filename.Length != 0) {
                SaveDefinesList(filename);
            }
        }

        internal void mnuRInGame_Click(object sender, EventArgs e) {
            // TODO: add ingame functionality for defines lists
            //ToggleInGame();
        }

        private void mnuRAddFile_Click(object sender, EventArgs e) {
            AddGlobalsFromFile();
        }

        private void SetEditMenu() {
            if (globalsgrid.IsCurrentCellInEditMode) {
                mnuUndo.Enabled = false;
                mnuCut.Enabled = false;
                mnuCopy.Enabled = false;
                mnuPaste.Enabled = false;
                mnuDelete.Enabled = false;
                mnuClear.Enabled = false;
                mnuInsert.Enabled = false;
                mnuSelectAll.Enabled = false;
                mnuFindInLogics.Enabled = false;
                mnuEditItem.Enabled = false;
                return;
            }
            mnuUndo.Enabled = UndoCol.Count > 0;
            if (mnuUndo.Enabled) {
                mnuUndo.Text = "Undo " + Editor.Base.EditorResourceByNum(GLBUNDOTEXT + (int)UndoCol.Peek().Action);
            }
            else {
                mnuUndo.Text = "Undo";
            }
            mnuCut.Text = "Cut ";
            mnuCopy.Text = "Copy ";
            mnuDelete.Text = "Delete ";
            if (globalsgrid.SelectionMode == DataGridViewSelectionMode.CellSelect) {
                mnuCut.Enabled = false;
                mnuCopy.Enabled = globalsgrid.CurrentRow.Index != globalsgrid.NewRowIndex;
                mnuDelete.Enabled = false;
                switch (globalsgrid.CurrentCell.ColumnIndex) {
                case 2:
                    mnuCut.Text += "Name";
                    mnuCopy.Text += "Name";
                    mnuDelete.Text += "Name";
                    break;
                case 3:
                    mnuCut.Text += "Value";
                    mnuCopy.Text += "Value";
                    mnuDelete.Text += "Value";
                    break;
                case 4:
                    mnuCut.Text += "Comment";
                    mnuCopy.Text += "Comment";
                    mnuDelete.Text += "Comment";
                    break;
                }
            }
            else {
                mnuCut.Enabled = mnuCopy.Enabled = mnuDelete.Enabled = (globalsgrid.CurrentRow.Index != globalsgrid.NewRowIndex);
                mnuCut.Text += "Row";
                mnuCopy.Text += "Row";
                mnuDelete.Text += "Row";
                if (globalsgrid.SelectedRows.Count > 1) {
                    mnuCut.Text += "s";
                    mnuCopy.Text += "s";
                    mnuDelete.Text += "s";
                }
            }
            if (Clipboard.ContainsText(TextDataFormat.UnicodeText)) {
                // if text on clipboard has globals format (#define ....)
                string temp = Clipboard.GetText(TextDataFormat.UnicodeText);
                // check for define marker
                if (temp.Left(8) == DEF_MARKER) {
                    mnuPaste.Enabled = true;
                }
                else {
                    mnuPaste.Enabled = false;
                }
            }
            else {
                mnuPaste.Enabled = false;
            }
            mnuInsert.Enabled = true;
            mnuSelectAll.Enabled = true; // always available
            mnuFindInLogics.Visible = mnuSep1.Visible = EditGame is not null;
            if (mnuSep1.Visible) {
                if (globalsgrid.SelectionMode == DataGridViewSelectionMode.CellSelect) {
                    mnuFindInLogics.Enabled = globalsgrid.CurrentCell.ColumnIndex == NAME_COL;
                }
                else {
                    mnuFindInLogics.Enabled = (globalsgrid.SelectedRows.Count == 1 && globalsgrid.CurrentRow.Index != globalsgrid.NewRowIndex);
                }
            }
            mnuEditItem.Enabled = true;
            mnuEditItem.Text = "Edit ";
            switch (globalsgrid.CurrentCell.ColumnIndex) {
            case NAME_COL:
                mnuEditItem.Text += "Name";
                break;
            case VALUE_COL:
                mnuEditItem.Text += "Value";
                break;
            case COMMENT_COL:
                mnuEditItem.Text += "Comment";
                break;
            }
            if (globalsgrid.Columns[COMMENT_COL].Visible) {
                mnuToggleComments.Text = "Hide Comments";
            }
            else {
                mnuToggleComments.Text = "Show Comments";
            }
        }

        private void ResetEditMenu() {
            // enable all items so shortcut keys are always available
            mnuClear.Enabled = true;
            mnuCopy.Enabled = true;
            mnuCut.Enabled = true;
            mnuDelete.Enabled = true;
            mnuFindInLogics.Enabled = true;
            mnuInsert.Enabled = true;
            mnuPaste.Enabled = true;
            mnuSelectAll.Enabled = true;
            mnuUndo.Enabled = true;
            mnuEditItem.Enabled = true;
        }

        private void cmGrid_Opening(object sender, CancelEventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                e.Cancel = true;
                return;
            }
            SetEditMenu();
        }

        private void cmGrid_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            // move menu items to edit menu
            mnuEdit.DropDownItems.AddRange([
                mnuUndo, mnuSep0, mnuCut, mnuCopy, mnuPaste,
                mnuDelete, mnuClear, mnuInsert, mnuSelectAll,
                mnuSep1, mnuFindInLogics, mnuEditItem, mnuToggleComments]);
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            // return menu items to context menu
            cmGrid.Items.AddRange([
                mnuUndo, mnuSep0, mnuCut, mnuCopy, mnuPaste,
                mnuDelete, mnuClear, mnuInsert, mnuSelectAll,
                mnuSep1, mnuFindInLogics, mnuEditItem, mnuToggleComments]);
            ResetEditMenu();

        }

        private void mnuUndo_Click(object sender, EventArgs e) {

            if (UndoCol.Count == 0 || globalsgrid.IsCurrentCellInEditMode) {
                return;
            }
            GlobalsUndo NextUndo = UndoCol.Pop();
            switch (NextUndo.Action) {
            case GlobalsUndo.GlobalUndoAction.AddDefine:
                // remove the define values that was added
                globalsgrid.Rows.RemoveAt(NextUndo.Pos);
                break;
            case GlobalsUndo.GlobalUndoAction.ImportDefines:
            case GlobalsUndo.GlobalUndoAction.PasteDefines:
                // remove the define values were added
                // if there is a value, the define was a replace;
                // if no value, it was added as a new row 

                // step through the defines list in reverse
                // order to preserve the previous state
                for (int i = NextUndo.Count - 1; i >= 0; i--) {
                    int rownum = int.Parse(NextUndo.UDDefine[i].Name);
                    string value = NextUndo.UDDefine[i].Value;
                    if (value.Length > 0) {
                        // undo a replace
                        globalsgrid[VALUE_COL, rownum].Value = value;
                        globalsgrid[ARGTYPE_COL, rownum].Value = NextUndo.UDDefine[i].Type;
                        globalsgrid[COMMENT_COL, rownum].Value = NextUndo.UDDefine[i].Comment;
                    }
                    else {
                        // undo an addition
                        globalsgrid.Rows.RemoveAt(rownum);
                    }
                }
                break;
            case GlobalsUndo.GlobalUndoAction.DeleteDefine:
            case GlobalsUndo.GlobalUndoAction.CutDefine:
            case GlobalsUndo.GlobalUndoAction.ClearList:
                // add back the items removed and select them
                globalsgrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                globalsgrid.SuspendLayout();
                globalsgrid.Rows.Insert(NextUndo.Pos, NextUndo.Count);
                for (int i = 0; i < NextUndo.Count; i++) {
                    // set values for hidden columns first
                    globalsgrid[ARGTYPE_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].Type;
                    globalsgrid[NAMETYPE_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].NameType;
                    globalsgrid[VALUETYPE_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].ValueType;
                    globalsgrid[DEFNAME_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].DefaultName;
                    globalsgrid[DEFVALUE_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].DefaultValue;
                    // visible columns will trigger format event
                    globalsgrid[NAME_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].Name;
                    globalsgrid[VALUE_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].Value;
                    globalsgrid[COMMENT_COL, NextUndo.Pos + i].Value = NextUndo.UDDefine[i].Comment;
                    globalsgrid.Rows[NextUndo.Pos + i].Selected = true;
                    globalsgrid.Rows[NextUndo.Pos + i].Tag = NextUndo.UDDefine[i].UID;
                    if (NextUndo.UDDefine[i].DefaultName.Length > 0) {
                        DelDefine delDefine = new() {
                            Name = NextUndo.UDDefine[i].DefaultName,
                            Value = NextUndo.UDDefine[i].Value
                        };
                        DeletedDefines.Remove(delDefine);
                    }
                }
                globalsgrid.ResumeLayout();
                if (!globalsgrid.Rows[NextUndo.Pos].Displayed) {
                    if (globalsgrid.Rows[NextUndo.Pos].Index < 2) {
                        globalsgrid.FirstDisplayedScrollingRowIndex = 0;
                    }
                    else {
                        globalsgrid.FirstDisplayedScrollingRowIndex = globalsgrid.Rows[NextUndo.Pos].Index - 2;
                    }
                }
                break;
            case GlobalsUndo.GlobalUndoAction.EditName:
                globalsgrid[NAME_COL, NextUndo.Pos].Value = NextUndo.Text;
                globalsgrid[NAME_COL, NextUndo.Pos].Selected = true;
                break;
            case GlobalsUndo.GlobalUndoAction.EditValue:
                globalsgrid[VALUE_COL, NextUndo.Pos].Value = NextUndo.Text;
                ArgType argtype = ArgType.None;
                DefineValueCheck valuetype = GetValueType(NextUndo.Text, ref argtype);
                globalsgrid[VALUETYPE_COL, NextUndo.Pos].Value = valuetype;
                globalsgrid[ARGTYPE_COL, NextUndo.Pos].Value = argtype;
                globalsgrid[VALUE_COL, NextUndo.Pos].Selected = true;
                break;
            case GlobalsUndo.GlobalUndoAction.EditComment:
                globalsgrid[COMMENT_COL, NextUndo.Pos].Value = NextUndo.Text;
                globalsgrid[COMMENT_COL, NextUndo.Pos].Selected = true;
                break;
            case GlobalsUndo.GlobalUndoAction.MoveRows:
                // move rows back to original position
                dragStartRow = NextUndo.Start;
                dragEndRow = NextUndo.End;
                MoveRows(NextUndo.Pos, true);
                break;
            case GlobalsUndo.GlobalUndoAction.SortList:
                // old sort list is in undo object
                RestoreSortOrder(NextUndo.SortOrder);
                break;
            }
            MarkAsChanged();
            globalsgrid.Refresh();
        }

        private void mnuCut_Click(object sender, EventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode ||
                globalsgrid.SelectionMode == DataGridViewSelectionMode.CellSelect ||
                globalsgrid.CurrentRow.Index == globalsgrid.NewRowIndex) {
                return;
            }
            // copy
            mnuCopy_Click(sender, e);
            // then delete
            RemoveRows(globalsgrid.SelectedRows[0].Index, globalsgrid.SelectedRows[^1].Index);
            // rename last Undo object
            UndoCol.Peek().Action = GlobalsUndo.GlobalUndoAction.CutDefine;
        }

        private void mnuCopy_Click(object sender, EventArgs e) {
            //  copies the selected cell or the selected rows to the clipboard
            //  the normal clipboard gets the name, value and comment fields
            //  formatted as 'define' lines;
            //  a duplicate internal clipboard is used that also tracks the
            //  hidden 'original name' column
            string ouputData = "";
            int topRow, bottomRow;

            if (globalsgrid.IsCurrentCellInEditMode ||
                globalsgrid.CurrentRow.Index == globalsgrid.NewRowIndex) {
                return;
            }
            if (globalsgrid.SelectionMode == DataGridViewSelectionMode.FullRowSelect) {
                topRow = globalsgrid.SelectedRows[0].Index;
                bottomRow = globalsgrid.SelectedRows[^1].Index;
                if (bottomRow < topRow) {
                    int swap = bottomRow;
                    bottomRow = topRow;
                    topRow = swap;
                }
                for (int i = topRow; i <= bottomRow; i++) {
                    // add to normal clipboard
                    ouputData += DEF_MARKER + (string)globalsgrid[NAME_COL, i].Value + " " + (string)globalsgrid[VALUE_COL, i].Value;
                    if ((string)globalsgrid[COMMENT_COL, i].Value != "") {
                        ouputData += " " + (string)globalsgrid[COMMENT_COL, i].Value;
                    }
                    if (i != bottomRow) {
                        ouputData += "\n";
                    }
                }
            }
            else {
                // select just this cell text
                ouputData = (string)globalsgrid.CurrentCell.Value;
            }

            // put selected text on clipboard
            Clipboard.Clear();
            Clipboard.SetText(ouputData);
        }

        private void mnuPaste_Click(object sender, EventArgs e) {
            bool enabled;

            if (globalsgrid.IsCurrentCellInEditMode) {
                enabled = false;
            }
            else {
                if (Clipboard.ContainsText(TextDataFormat.UnicodeText)) {
                    // if text on clipboard has globals format (#define ....)
                    string cbData = Clipboard.GetText(TextDataFormat.UnicodeText);
                    // check for define marker
                    if (cbData.Left(8) == DEF_MARKER) {
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
            bool errors = false;
            GlobalsUndo NextUndo = new();
            NextUndo.Action = GlobalsUndo.GlobalUndoAction.PasteDefines;
            NextUndo.Pos = globalsgrid.CurrentRow.Index;
            Define[] PasteDefines;
            PasteDefines = ReadDefines(Clipboard.GetText(TextDataFormat.UnicodeText), ref errors);
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
            Define[] undodata = InsertDefines(PasteDefines, globalsgrid.CurrentRow.Index, ref errors);

            // if nothing was added, no undo data exists
            if (undodata.Length == 0) {
                MessageBox.Show(MDIMain,
                    "There were no usable data on the clipboard, so nothing was pasted.",
                    "Nothing to Paste",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else {
                NextUndo.Count = undodata.Length;
                for (int i = 0; i < undodata.Length; i++) {
                    NextUndo.UDDefine[i] = undodata[i];
                }
                AddUndo(NextUndo);
            }
            if (errors) {
                MessageBox.Show(MDIMain,
                    "Some entries could not be pasted because of formatting or syntax errors.",
                    "Paste Errors",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode ||
                globalsgrid.SelectionMode == DataGridViewSelectionMode.CellSelect ||
                globalsgrid.CurrentRow.Index == globalsgrid.NewRowIndex) {
                return;
            }
            RemoveRows(globalsgrid.SelectedRows[0].Index, globalsgrid.SelectedRows[^1].Index);
        }

        private void mnuClear_Click(object sender, EventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                return;
            }
            if (MessageBox.Show(MDIMain,
                "All global defines will be discarded. Do you want to continue?",
                "Clear Global Defines",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) == DialogResult.Cancel) {
                return;
            }
            GlobalsUndo NextUndo = new() {
                Action = GlobalsUndo.GlobalUndoAction.ClearList,
                Count = globalsgrid.RowCount
            };
            for (int i = 0; i < globalsgrid.RowCount - 1; i++) {
                Define tmpdef = new() {
                    Type = (ArgType)globalsgrid[ARGTYPE_COL, i].Value,
                    DefaultName = (string)globalsgrid[DEFNAME_COL, i].Value,
                    DefaultValue = (string)globalsgrid[DEFVALUE_COL, i].Value,
                    Name = (string)globalsgrid[NAME_COL, i].Value,
                    Value = (string)globalsgrid[VALUE_COL, i].Value,
                    Comment = (string)globalsgrid[COMMENT_COL, i].Value,
                    UID = (int)globalsgrid.Rows[i].Tag
                };
                NextUndo.UDDefine[i] = tmpdef;
            }
            UndoCol.Push(NextUndo);
            // clear grid by deleting all rows
            globalsgrid.Rows.Clear();
            globalsgrid[NAME_COL, 0].Selected = true;
        }

        private void mnuInsert_Click(object sender, EventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                return;
            }
            EditDefine.Type = None;
            EditDefine.DefaultName = "";
            EditDefine.DefaultValue = "";
            EditDefine.Name = "";
            EditDefine.Value = "";
            EditDefine.Comment = "";
            if (globalsgrid.NewRowIndex != globalsgrid.CurrentRow.Index) {
                if (globalsgrid.SelectionMode != DataGridViewSelectionMode.CellSelect) {
                    globalsgrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                }
                DataGridViewRow newRow = new DataGridViewRow();
                newRow.CreateCells(globalsgrid);
                newRow.Cells[ARGTYPE_COL].Value = None;
                newRow.Cells[DEFNAME_COL].Value = "";
                newRow.Cells[DEFVALUE_COL].Value = "";
                newRow.Cells[NAME_COL].Value = "";
                newRow.Cells[VALUE_COL].Value = "";
                newRow.Cells[COMMENT_COL].Value = "";
                globalsgrid.Rows.Insert(globalsgrid.CurrentRow.Index, newRow);
                globalsgrid[NAME_COL, globalsgrid.CurrentRow.Index - 1].Selected = true;
            }
            else {
                // force column to name for new rows
                if (globalsgrid.CurrentCell.ColumnIndex != NAME_COL) {
                    globalsgrid[NAME_COL, globalsgrid.CurrentCell.RowIndex].Selected = true;
                }
            }
            globalsgrid.Refresh();
            Inserting = true;
            globalsgrid.BeginEdit(true);
        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                return;
            }
            globalsgrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            globalsgrid.MultiSelect = true;
            globalsgrid.CurrentCell = globalsgrid[NAME_COL, 0];
            globalsgrid.SelectAll();
            globalsgrid.Refresh();
        }

        private void mnuEditItem_Click(object sender, EventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                return;
            }
            EditCell(globalsgrid.CurrentCell.ColumnIndex);
        }

        private void mnuFindInLogics_Click(object sender, EventArgs e) {

            if (globalsgrid.IsCurrentCellInEditMode ||
            (globalsgrid.SelectionMode == DataGridViewSelectionMode.CellSelect && globalsgrid.CurrentCell.ColumnIndex != NAME_COL) ||
            (globalsgrid.SelectionMode != DataGridViewSelectionMode.CellSelect && (globalsgrid.SelectedRows.Count == 0 || globalsgrid.CurrentRow.Index == globalsgrid.NewRowIndex))
            ) {
                return;
            }
            if (InGame) {
                string searchtext = (string)globalsgrid[NAME_COL, globalsgrid.CurrentRow.Index].Value;
                GFindText = searchtext;
                GFindDir = FindDirection.All;
                GMatchWord = true;
                GMatchCase = true;
                GLogFindLoc = FindLocation.All;
                GFindSynonym = false;
                FindingForm.ResetSearch();
                FindInLogic(this, searchtext, FindDirection.All, true, true, FindLocation.All);
            }
        }

        private void mnuToggleComments_Click(object sender, EventArgs e) {
            if (globalsgrid.Columns[COMMENT_COL].Visible) {
                globalsgrid.Columns[COMMENT_COL].Visible = false;
            }
            else {
                // currently hidden, so show
                int totalwidth = globalsgrid.Columns[NAME_COL].Width + globalsgrid.Columns[VALUE_COL].Width;
                globalsgrid.Columns[COMMENT_COL].Visible = true;
                globalsgrid.Columns[COMMENT_COL].Width = (int)(totalwidth * 0.3);
            }
        }

        private void cmCel_Opening(object sender, CancelEventArgs e) {
            mnuCelUndo.Enabled = EditTextBox.CanUndo;
            mnuCelCut.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCopy.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelPaste.Enabled = Clipboard.ContainsText();
            mnuCelDelete.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCharMap.Visible = globalsgrid.CurrentCell.ColumnIndex != NAME_COL;
            mnuCelSelectAll.Enabled = EditTextBox.TextLength > 0;
        }

        private void cmCel_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            mnuCelUndo.Enabled = true;
            mnuCelCut.Enabled = true;
            mnuCelCopy.Enabled = true;
            mnuCelPaste.Enabled = true;
            mnuCelDelete.Enabled = true;
            mnuCelSelectAll.Enabled = true;
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
            else if (EditTextBox.SelectionStart < EditTextBox.Text.Length) {
                int oldsel = EditTextBox.SelectionStart;
                EditTextBox.Text = EditTextBox.Text[..oldsel] + EditTextBox.Text[(oldsel + 1)..];
                EditTextBox.SelectionStart = oldsel;
            }
        }

        private void mnuCelCharMap_Click(object sender, EventArgs e) {
            if (globalsgrid.CurrentCell.ColumnIndex == NAME_COL) {
                return;
            }
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
            globalsgrid.CancelEdit();
            // cancel alone doesn't work (the cell remains in edit mode)
            // but calling EndEdit immediately after seems to work
            globalsgrid.EndEdit();
        }
        #endregion

        #region Grid Events
        private void globalsgrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
            if (globalsgrid[ARGTYPE_COL, e.RowIndex].Value is null) {
                globalsgrid[ARGTYPE_COL, e.RowIndex].Value = None;
            }
            if (globalsgrid[DEFNAME_COL, e.RowIndex].Value is null) {
                globalsgrid[DEFNAME_COL, e.RowIndex].Value = "";
            }
            if (globalsgrid[DEFVALUE_COL, e.RowIndex].Value is null) {
                globalsgrid[DEFVALUE_COL, e.RowIndex].Value = "";
            }
            if (globalsgrid[NAMETYPE_COL, e.RowIndex].Value is null) {
                globalsgrid[NAMETYPE_COL, e.RowIndex].Value = DefineNameCheck.OK;
            }
            if (globalsgrid[VALUETYPE_COL, e.RowIndex].Value is null) {
                globalsgrid[VALUETYPE_COL, e.RowIndex].Value = DefineValueCheck.OK;
            }
        }

        private void globalsgrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.Value is null || e.RowIndex == globalsgrid.NewRowIndex ||
                globalsgrid[ARGTYPE_COL, e.RowIndex].Value is null) {
                return;
            }
            // determine if tooltip is needed
            DataGridViewCell cell = globalsgrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string text = (string)e.Value;
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            // get size
            Size szText = TextRenderer.MeasureText(globalsgrid.CreateGraphics(), text, e.CellStyle.Font, proposedSize, flags);
            if (szText.Width > cell.Size.Width - 8) {
                cell.ToolTipText = text;
            }
            else {
                cell.ToolTipText = "";
            }

            // next apply formatting based on validatecheck status
            switch (e.ColumnIndex) {
            case NAME_COL:
                if (globalsgrid[NAMETYPE_COL, e.RowIndex].Value is not null) {
                    switch ((DefineNameCheck)globalsgrid[NAMETYPE_COL, e.RowIndex].Value) {
                    case DefineNameCheck.OK:
                        // use default style
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
                        e.CellStyle.ForeColor = Color.DarkOrange;
                        e.CellStyle.BackColor = Color.LightYellow;
                        e.CellStyle.Font = boldFont;
                        break;
                    }
                }
                break;
            case VALUE_COL:
                if ((DefineValueCheck)globalsgrid[VALUETYPE_COL, e.RowIndex].Value is DefineValueCheck.Reserved or
                    DefineValueCheck.Global) {
                    // overrides are rare - reconfirm them when they are encountered
                    ArgType at = ArgType.None;
                    var dvt = GetValueType((string)globalsgrid.Rows[e.RowIndex].Cells[VALUE_COL].Value, ref at);
                    if (dvt == DefineValueCheck.OK) {
                        globalsgrid.Rows[e.RowIndex].Cells[VALUETYPE_COL].Value = dvt;
                        globalsgrid.Rows[e.RowIndex].Cells[ARGTYPE_COL].Value = at;
                        SetValueColor();
                    }
                    else {
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.Font = boldFont;
                    }
                }
                else {
                    SetValueColor();
                }
                break;
            case COMMENT_COL:
                e.CellStyle.ForeColor = Color.FromArgb(0x50, 0x50, 0x50);
                e.CellStyle.Font = italicFont;
                break;
            }

            void SetValueColor() {
                switch ((DefineValueCheck)globalsgrid[VALUETYPE_COL, e.RowIndex].Value) {
                case DefineValueCheck.OK:
                    switch ((ArgType)globalsgrid[ARGTYPE_COL, e.RowIndex].Value) {
                    case ArgType.None:
                        break;
                    case ArgType.Num:
                        // i.e. numeric Value
                        // use default
                        break;
                    case ArgType.Var:
                    case ArgType.Flag:
                    case ArgType.MsgNum:
                    case ArgType.SObj:
                    case ArgType.InvItem:
                    case ArgType.Str:
                    case ArgType.Word:
                    case ArgType.Ctrl:
                        // argument markers
                        e.CellStyle.ForeColor = Color.Blue;
                        break;
                    case ArgType.DefStr:
                        // string
                        e.CellStyle.ForeColor = Color.Green;
                        break;
                    case ArgType.VocWrd:
                        // not available in defines?
                        break;
                    case ArgType.ActionCmd:
                    case ArgType.TestCmd:
                    case ArgType.Object:
                    case ArgType.View:
                        // sierrasyntax not applicable (global editor
                        // not used)
                        break;
                    }
                    e.CellStyle.Font = boldFont;
                    break;
                case DefineValueCheck.Empty:
                case DefineValueCheck.OutofBounds:
                    // error values that never occur in a cell
                    break;
                case DefineValueCheck.BadArgNumber:
                    e.CellStyle.ForeColor = Color.DarkOrange;
                    e.CellStyle.BackColor = Color.LightYellow;
                    e.CellStyle.Font = boldFont;
                    break;
                case DefineValueCheck.NotAValue:
                    // non-argument (probably error or re-define)
                    e.CellStyle.ForeColor = Color.DarkRed;
                    e.CellStyle.BackColor = Color.LightPink;
                    break;
                }
            }
        }

        private void globalsgrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) {
                return;
            }
            DataGridViewCell cell = globalsgrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.ToolTipText.Length > 0) {
                globalsgrid.ShowCellToolTips = true;
            }
        }

        private void globalsgrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            globalsgrid.ShowCellToolTips = false;
        }

        private void globalsgrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                if (e.ColumnIndex < 2 || e.RowIndex < 0 || !globalsgrid[e.ColumnIndex, e.RowIndex].IsInEditMode) {
                    if (e.Button == MouseButtons.Right) {
                        // same as ESCAPE
                        EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Escape));
                    }
                    else {
                        // same as pressing ENTER
                        EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Enter));
                        return;
                    }
                }
            }
            if (e.RowIndex == -1) {
                return;
            }
            if (globalsgrid.SelectionMode == DataGridViewSelectionMode.FullRowSelect) {
                if (e.Button == MouseButtons.Right) {
                    if (globalsgrid.Rows[e.RowIndex].Selected) {
                        return;
                    }
                }
                else {
                    // only if full row selected AND mouse is over the selected row's header
                    if (e.ColumnIndex == -1 && globalsgrid.SelectedRows.Count >= 1) {
                        int startrow = globalsgrid.SelectedRows[0].Index;
                        int endrow = globalsgrid.SelectedRows[^1].Index;
                        if (startrow > endrow) {
                            (startrow, endrow) = (endrow, startrow);
                        }
                        // check if mouse is over one of the selected rows
                        if (e.RowIndex >= startrow &&
                            e.RowIndex <= endrow &&
                            !globalsgrid.Rows[e.RowIndex].IsNewRow) {
                            // begin dragging 
                            dragStartRow = startrow;
                            dragEndRow = endrow;
                            globalsgrid.DoDragDrop(globalsgrid.Rows[dragStartRow], DragDropEffects.Move);
                            return;
                        }
                    }
                }
            }
            if (e.ColumnIndex == -1) {
                if (globalsgrid.SelectionMode != DataGridViewSelectionMode.FullRowSelect) {
                    globalsgrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    globalsgrid.MultiSelect = true;
                    if (e.Button == MouseButtons.Right) {
                        // force selection
                        if (e.RowIndex != -1) {
                            globalsgrid.CurrentCell = globalsgrid[NAME_COL, e.RowIndex];
                            globalsgrid.CurrentRow.Selected = true;
                            globalsgrid.Refresh();
                        }
                    }
                }
            }
            else {
                globalsgrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                globalsgrid.MultiSelect = false;
                if (e.Button == MouseButtons.Right) {
                    // force selection
                    globalsgrid.CurrentCell = globalsgrid[e.ColumnIndex, e.RowIndex];
                    globalsgrid.CurrentCell.Selected = true;
                    globalsgrid.Refresh();
                }
            }
        }

        private void globalsgrid_SelectionChanged(object sender, EventArgs e) {
            if (globalsgrid.SelectedRows.Count > 0) {
                globalsgrid.MultiSelect = true;
                if (globalsgrid.SelectedRows.Count > 1) {
                    if (globalsgrid.Rows[^1].Selected) {
                        globalsgrid.Rows[^1].Selected = false;
                    }
                }
            }
            else {
                globalsgrid.MultiSelect = false;
            }
        }

        private void globalsgrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
            if (e.Control is TextBox) {
                // need to set MultiLine to true so the ENTER key can be captured by
                // KeyPress event
                EditTextBox = e.Control as TextBox;
                if (EditTextBox.ContextMenuStrip != cmCel) {
                    EditTextBox.ContextMenuStrip = cmCel;
                    EditTextBox.Validating += EditTextBox_Validating;
                    EditTextBox.KeyDown += EditTextBox_KeyDown;
                    EditTextBox.KeyPress += EditTextBox_KeyPress;
                }
                EditTextBox.Multiline = true;
                EditTextBox.AcceptsReturn = true;
                EditTextBox.AcceptsTab = true;
            }
        }

        private void globalsgrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                e.Cancel = true;
            }
        }

        private void globalsgrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
            if (globalsgrid.CurrentRow.Index == globalsgrid.NewRowIndex) {
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[ARGTYPE_COL].Value = None;
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[DEFNAME_COL].Value = "";
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[DEFVALUE_COL].Value = "";
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[NAME_COL].Value = "";
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[VALUE_COL].Value = "";
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[COMMENT_COL].Value = "";
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[NAMETYPE_COL].Value = DefineNameCheck.OK;
                globalsgrid.Rows[globalsgrid.NewRowIndex].Cells[VALUETYPE_COL].Value = DefineValueCheck.OK;

                if (!Inserting) {
                    Inserting = true;
                }
            }
        }

        private void globalsgrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            EditCell(e.ColumnIndex);
        }

        private void globalsgrid_Scroll(object sender, ScrollEventArgs e) {
            if (globalsgrid.IsCurrentCellInEditMode) {
                e.NewValue = e.OldValue;
            }
        }

        private void globalsgrid_KeyDown(object sender, KeyEventArgs e) {
            // F3 causes sort order change without triggering the form's
            // sort method- don't know why
            if (e.KeyData == Keys.F3) {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            // ALT-E begins edit if in an existing row
            // ENTER begins edit if on new row
            if (!globalsgrid.IsCurrentCellInEditMode) {
                if (e.KeyData == (Keys.E | Keys.Alt) && globalsgrid.CurrentRow.Index != globalsgrid.NewRowIndex) {
                    EditDefine.Type = (ArgType)globalsgrid[ARGTYPE_COL, globalsgrid.CurrentRow.Index].Value;
                    EditDefine.DefaultName = (string)globalsgrid[DEFNAME_COL, globalsgrid.CurrentRow.Index].Value;
                    EditDefine.DefaultValue = (string)globalsgrid[DEFVALUE_COL, globalsgrid.CurrentRow.Index].Value;
                    EditDefine.Name = (string)globalsgrid[NAME_COL, globalsgrid.CurrentRow.Index].Value;
                    EditDefine.Value = (string)globalsgrid[VALUE_COL, globalsgrid.CurrentRow.Index].Value;
                    EditDefine.Comment = (string)globalsgrid[COMMENT_COL, globalsgrid.CurrentRow.Index].Value;
                    globalsgrid.BeginEdit(true);
                    e.Handled = true;
                }
                else if (e.KeyData == Keys.Enter && globalsgrid.CurrentRow.Index == globalsgrid.NewRowIndex) {
                    EditDefine.Type = None;
                    EditDefine.DefaultName = "";
                    EditDefine.DefaultValue = "";
                    EditDefine.Name = "";
                    EditDefine.Value = "";
                    EditDefine.Comment = "";
                    globalsgrid.BeginEdit(true);
                    e.Handled = true;
                }
            }
        }

        private void globalsgrid_DragOver(object sender, DragEventArgs e) {
            Point clientPoint = globalsgrid.PointToClient(new Point(e.X, e.Y));
            var hit = globalsgrid.HitTest(clientPoint.X, clientPoint.Y);
            int SCROLL_REGION = globalsgrid.Rows[0].Height;

            // enable scrolling if dragging near top or bottom edge
            if (clientPoint.Y < SCROLL_REGION) {
                if (globalsgrid.FirstDisplayedScrollingRowIndex > 0) {
                    dragDirection = -1;
                    dragTimer.Start();
                }
            }
            else if (clientPoint.Y > globalsgrid.Height - SCROLL_REGION) {
                if (!IsScrolledToBottom()) {
                    // scroll down
                    dragDirection = 1;
                    dragTimer.Start();
                }
            }
            else {
                dragDirection = 0;
                dragTimer.Stop();
            }

            // allow drop if not on current selection
            if (hit.RowIndex >= dragStartRow && hit.RowIndex <= dragEndRow) {
                e.Effect = DragDropEffects.None;
            }
            else if (hit.RowIndex == -1) {
                e.Effect = DragDropEffects.None;
            }
            else {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void globalsgrid_DragDrop(object sender, DragEventArgs e) {
            if (canceldrag) {
                // ignore if dropping on original location, or if 
                // canceled for any other reason
                return;
            }
            Point clientPoint = globalsgrid.PointToClient(new Point(e.X, e.Y));
            var hit = globalsgrid.HitTest(clientPoint.X, clientPoint.Y);
            // QueryContinueDrag should confirm this location is valid; if not
            // the cancel flag gets set and this code gets skipped
            Debug.Assert(dragStartRow >= 0);
            Debug.Assert(dragEndRow >= 0);
            Debug.Assert(hit.RowIndex >= 0);
            Debug.Assert(hit.RowIndex < dragStartRow || hit.RowIndex > dragEndRow);
            Debug.Assert(hit.RowIndex != dragEndRow + 1);
            MoveRows(hit.RowIndex);
        }

        private void globalsgrid_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Move;
        }

        private void globalsgrid_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {

            switch (e.Action) {
            case DragAction.Cancel:
                // Drag-and-drop was canceled (e.g., user pressed Esc)
                canceldrag = true;
                break;
            case DragAction.Continue:
                break;
            case DragAction.Drop:
                // if on selection, cancel the move
                Point clientPoint = globalsgrid.PointToClient(MousePosition);
                var hit = globalsgrid.HitTest(clientPoint.X, clientPoint.Y);
                if (hit.RowIndex >= dragStartRow && hit.RowIndex <= dragEndRow) {
                    canceldrag = true;
                }
                // if on row directly beneath, also cancel, because the rows
                // won't actually change position
                if (hit.RowIndex == dragEndRow + 1) {
                    canceldrag = true;
                }
                break;
            }
        }

        private void globalsgrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e) {
            // This was the only event I could find that reliably occurs after the
            // last SelectionChanged event that happens when a row is moved. Something
            // forces the Selection to change back to the original draggedRowIndex.
            // It happens after all mouse related events. This event happens after
            // that. 

            // The dropping flag is used force the selection to change.
            if (dropping) {
                dropping = false;
                // IMPORTANT! must set current cell to a cell in the target row,
                // otherwise the selection won't update to the target row
                if (targetRowIndex < dragStartRow) {
                    //newstart=tgt
                    //newend = newstart + end - start;

                    globalsgrid.CurrentCell = globalsgrid.Rows[targetRowIndex].Cells[NAME_COL];
                    for (int i = targetRowIndex; i <= targetRowIndex + dragEndRow - dragStartRow; i++) {
                        globalsgrid.Rows[i].Selected = true;
                    }
                }
                else {
                    //newstart = tgt - count
                    //newend = tgt - 1
                    //count=end-st+1

                    int i = targetRowIndex - (dragEndRow - dragStartRow + 1);
                    globalsgrid.CurrentCell = globalsgrid.Rows[i].Cells[NAME_COL];
                    for (; i <= targetRowIndex - 1; i++) {
                        globalsgrid.Rows[i].Selected = true;
                    }
                }
                dragStartRow = -1;
                dragEndRow = -1;
            }
            else if (canceldrag) {
                canceldrag = false;
                // force selection back to original dragged rows
                globalsgrid.CurrentCell = globalsgrid.Rows[dragStartRow].Cells[NAME_COL];
                for (int i = dragStartRow; i <= dragEndRow; i++) {
                    globalsgrid.Rows[i].Selected = true;
                }
            }
            targetRowIndex = -1;
        }

        private void globalsgrid_MouseClick(object sender, MouseEventArgs e) {
            // Check for a click on the column header, which may be the start
            // of a sort operation. If so, capture the current order of the
            // list for undo purposes.
            var hit = globalsgrid.HitTest(e.X, e.Y);
            if (hit.Type == DataGridViewHitTestType.ColumnHeader) {
                foreach (DataGridViewRow row in globalsgrid.Rows) {
                    if (row.Index != globalsgrid.NewRowIndex) {
                        Debug.Assert(row.Tag is not null, "Row tag should not be null");
                        currentOrder.Add((int)row.Tag);
                    }
                }
            }
        }

        private void globalsgrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            // indicates a sor has occurred, so save the new order for undo purposes
            GlobalsUndo NextUndo = new() {
                Action = GlobalsUndo.GlobalUndoAction.SortList,
                SortOrder = currentOrder
            };
            AddUndo(NextUndo);
        }

        private void globalsgrid_VisibleChanged(object sender, EventArgs e) {
            // should only happen after form finishes loading
            if (globalsgrid.Visible) {
                // set columns to default widths
                int width = globalsgrid.Columns[NAME_COL].Width + globalsgrid.Columns[VALUE_COL].Width + globalsgrid.Columns[COMMENT_COL].Width;
                double namefraction;
                double valuefraction;

                // if showing comments:
                if (WinAGISettings.GEShowComment.Value) {
                    // check fraction values;
                    namefraction = WinAGISettings.GENameFrac.Value;
                    if (namefraction < 0.1) {
                        namefraction = 0.1;
                    }
                    valuefraction = WinAGISettings.GEValFrac.Value;
                    if (valuefraction == 0) {
                        // means previous opening was with comments hidden;
                        // set value fraction to 1-name fraction
                        valuefraction = 1 - namefraction;
                        // then use 70% of these values
                        namefraction *= 0.7;
                        valuefraction *= 0.7;
                    }
                    else if (valuefraction < 0.1) {
                        valuefraction = 0.1;
                    }
                    // set name column
                    globalsgrid.Columns[NAME_COL].Width = (int)(namefraction * width);
                    // set value column
                    globalsgrid.Columns[VALUE_COL].Width = (int)(valuefraction * width);
                }
                else {
                    // only show two columns
                    globalsgrid.Columns[COMMENT_COL].Visible = false;
                    namefraction = WinAGISettings.GENameFrac.Value;
                    if (namefraction < 0.1) {
                        namefraction = 0.1;
                    }
                    valuefraction = WinAGISettings.GEValFrac.Value;
                    if (valuefraction != 0) {
                        // means previous opening was with comments visible
                        // readjust name to correct ratio
                        if (valuefraction < 0.1) {
                            valuefraction = 0.1;
                        }
                        namefraction = namefraction / (namefraction + valuefraction);
                    }
                    else {
                        // means previous opening was with comments hidden;
                        // expected
                    }
                    globalsgrid.Columns[NAME_COL].Width = (int)(namefraction * width);
                }
            }
        }

        private void globalsgrid_MouseUp(object sender, MouseEventArgs e) {
            if (globalsgrid.Visible) {
                if (globalsgrid.Columns[COMMENT_COL].Visible) {
                    // if comment column is currently shown and user just
                    // resized it to be very small, hide it again
                    if (globalsgrid.Columns[COMMENT_COL].Width <= 5) {
                        globalsgrid.Columns[COMMENT_COL].Visible = false;
                    }
                }
            }
        }
        #endregion

        #region EditTextBox Events
        private void EditTextBox_Validating(object sender, CancelEventArgs e) {
            // textbox Validating event ignores Cancel property, use CellValidate
        }

        private void EditTextBox_KeyDown(object sender, KeyEventArgs e) {
            // pressing tab or enter should move to next COLUMN, not next ROW
            // (unless it's at end of row)
            string message = "";
            bool noWarn = false;
            DialogResult rtn = DialogResult.No;

            if (e.KeyCode == Keys.Escape) {
                // normally just cancel, and restore previous value
                // but if quitting on value when adding a new line,
                // entire line needs to be deleted
                globalsgrid.CancelEdit();
                globalsgrid.EndEdit();
                if ((string)globalsgrid.CurrentCell.Value == "") {
                    // delete current row if not insert row
                    if (!globalsgrid.CurrentRow.IsNewRow) {
                        globalsgrid.Rows.RemoveAt(globalsgrid.CurrentRow.Index);
                    }
                }
                return;
            }
            if (e.KeyValue == (int)Keys.Enter || e.KeyValue == (int)Keys.Tab) {
                e.Handled = true;
                e.SuppressKeyPress = true;
                EditTextBox.Text = EditTextBox.Text.Trim();
                GlobalsUndo NextUndo;
                // validate the input
                switch (globalsgrid.CurrentCell.ColumnIndex) {
                case NAME_COL:
                    if (EditDefine.Name == EditTextBox.Text) {
                        // no change
                        globalsgrid.EndEdit();
                        return;
                    }
                    // can't use the compiler validation checks, because they won't catch
                    // changes that are in the current modified globals list
                    DefineNameCheck nametype = GetNameType(EditTextBox.Text);
                    // first eight codes are error
                    if (nametype is >= (DefineNameCheck)1 and <= (DefineNameCheck)8) {
                        switch (nametype) {
                        case DefineNameCheck.Empty:
                            // 1 = no name
                            switch (WinAGISettings.DelBlankG.Value) {
                            case AskOption.Ask:
                                // get user's response
                                MDIMain.MsgBoxWithHelp(
                                    message,
                                    "Invalid Define Name",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error,
                                    "htm\\winagi\\globaldefines.htm#syntax");
                                rtn = MsgBoxEx.Show(MDIMain,
                                    "Blank global define names are not allowed. Delete this global define?",
                                    "Delete Blank Define",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    "Always take this action", ref noWarn,
                                    WinAGIHelp, "htm\\winagi\\globaldefines.htm#syntax");
                                if (noWarn) {
                                    if (rtn == DialogResult.No)
                                        WinAGISettings.DelBlankG.Value = AskOption.No;
                                    if (rtn == DialogResult.Yes)
                                        WinAGISettings.DelBlankG.Value = AskOption.Yes;
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
                                globalsgrid.CancelEdit();
                                globalsgrid.EndEdit();
                                // now delete current row
                                RemoveRow(globalsgrid.CurrentRow.Index);
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
                        MDIMain.MsgBoxWithHelp(
                            message,
                            "Invalid Define Name",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            "htm\\winagi\\globaldefines.htm#syntax");
                        EditTextBox.Multiline = true;
                        EditTextBox.SelectAll();
                        return;
                    }
                    // rest of codes can be overridden
                    else if (nametype >= DefineNameCheck.ReservedVar) {
                        string nametext = "";
                        switch (nametype) {
                        case DefineNameCheck.ReservedVar:
                            // 9 = name is reserved variable name
                            nametext = "reserved variable";
                            break;
                        case DefineNameCheck.ReservedFlag:
                            // 10 = name is reserved flag name
                            nametext = "reserved flag";
                            break;
                        case DefineNameCheck.ReservedNum:
                            // 11 = name is reserved number constant
                            nametext = "reserved number constant";
                            break;
                        case DefineNameCheck.ReservedObj:
                            // 12 = name is reserved object
                            nametext = "reserved object";
                            break;
                        case DefineNameCheck.ReservedStr:
                            // 13 = name is reserved string
                            nametext = "reserved string";
                            break;
                        case DefineNameCheck.ReservedMsg:
                            // 14 = name is reserved message
                            nametext = "reserved message";
                            break;
                        case DefineNameCheck.ResourceID:
                            // 15 = name is a resourceID
                            nametext = "resource ID";
                            break;
                        }
                        noWarn = false;
                        if (WinAGISettings.WarnResOverride.Value) {
                            EditTextBox.Multiline = false;
                            rtn = MsgBoxEx.Show(MDIMain,
                               "'" + EditTextBox.Text + "' is a " + nametext + ". Are you sure you want to override it?",
                               "Validate Global Define",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question,
                               "Don't show this warning again.", ref noWarn,
                               WinAGIHelp, "htm\\winagi\\globaldefines.htm#syntax");
                            WinAGISettings.WarnResOverride.Value = !noWarn;
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
                            Action = GlobalsUndo.GlobalUndoAction.EditName,
                            Count = 1,
                            Pos = globalsgrid.CurrentRow.Index,
                            Text = globalsgrid.CurrentCell.Value.ToString()
                        };
                        AddUndo(NextUndo);
                    }
                    globalsgrid[NAMETYPE_COL, globalsgrid.CurrentRow.Index].Value = nametype;
                    break;
                case VALUE_COL:
                    if (!Inserting && EditDefine.Value == EditTextBox.Text) {
                        // no change
                        globalsgrid.EndEdit();
                        return;
                    }
                    DefineValueCheck valuetype = GetValueType(EditTextBox.Text, ref EditDefine.Type);
                    if (valuetype == DefineValueCheck.Empty || valuetype == DefineValueCheck.OutofBounds) {
                        // errors
                        switch (valuetype) {
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
                        switch (valuetype) {
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
                                        "Don't show this warning again.", ref noWarn,
                                        WinAGIHelp, "htm\\winagi\\globaldefines.htm#syntax");
                                    EditTextBox.Multiline = true;
                                    WinAGISettings.WarnInvalidCtlVal.Value = !noWarn;
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
                                    if (EditGame is not null) {
                                        if (EditGame.InterpreterVersion.Index == AGIVersion.v2089 ||
                                            EditGame.InterpreterVersion.Index == AGIVersion.v2272 ||
                                            EditGame.InterpreterVersion.Index == AGIVersion.v3002149) {
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
                                        "Don't show this warning again.", ref noWarn,
                                        WinAGIHelp, "htm\\winagi\\globaldefines.htm#syntax");
                                    EditTextBox.Multiline = true;
                                    WinAGISettings.WarnInvalidStrVal.Value = !noWarn;
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
                                rtn = MDIMain.MsgBoxWithHelp(
                                    "'" + EditTextBox.Text + "' is an invalid word argument value (limit is w1 - w10). " +
                                    "Do you really want to have an invalid word argument definition?",
                                    "Invalid Word Argument Value",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    "htm\\winagi\\globaldefines.htm#syntax");
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
                            MDIMain.MsgBoxWithHelp(
                                "'" + EditTextBox.Text + "' is not a number, argument marker or text string. ",
                                "Unknown Define Value Type",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                "htm\\winagi\\globaldefines.htm#syntax");
                            EditTextBox.Multiline = true;
                            EditTextBox.SelectAll();
                            return;
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
                                    "Don't show this warning again.", ref noWarn,
                                    WinAGIHelp, "htm\\winagi\\globaldefines.htm#syntax");
                                EditTextBox.Multiline = true;
                                WinAGISettings.WarnDupGVal.Value = !noWarn;
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
                    if (!Inserting) {
                        NextUndo = new() {
                            Action = GlobalsUndo.GlobalUndoAction.EditValue,
                            Count = 1,
                            Pos = globalsgrid.CurrentRow.Index,
                            Text = globalsgrid.CurrentCell.Value.ToString()
                        };
                        AddUndo(NextUndo);
                    }
                    globalsgrid[VALUETYPE_COL, globalsgrid.CurrentRow.Index].Value = valuetype;
                    globalsgrid[ARGTYPE_COL, globalsgrid.CurrentRow.Index].Value = EditDefine.Type;
                    break;
                case COMMENT_COL:
                    // make sure the edit marker is present
                    if (EditDefine.Comment == EditTextBox.Text && !Inserting) {
                        // no change
                        globalsgrid.EndEdit();
                        return;
                    }
                    if (EditTextBox.Text.Length > 0) {
                        if (EditTextBox.Text[0] != '[' && EditTextBox.Text.Left(2) != "//") {
                            EditTextBox.Text = "[ " + EditTextBox.Text;
                        }
                    }
                    EditDefine.Comment = EditTextBox.Text;
                    if (!Inserting) {
                        NextUndo = new() {
                            Action = GlobalsUndo.GlobalUndoAction.EditComment,
                            Text = globalsgrid.CurrentCell.Value.ToString(),
                            Count = 1,
                            Pos = globalsgrid.CurrentRow.Index
                        };
                        AddUndo(NextUndo);
                    }
                    break;
                }
                globalsgrid.EndEdit();
                // if clearing a comment, the cell will use a null value; need
                // to force it to empty string
                if (globalsgrid.CurrentCell.Value is null) {
                    Debug.Assert(globalsgrid.CurrentCell.ColumnIndex == COMMENT_COL);
                    globalsgrid.CurrentCell.Value = "";
                }
                DataGridViewRow editrow = globalsgrid.CurrentRow;
                // INSERTING  COL      CMTVIS    KEY      ACTION
                // TRUE       NAME     TRUE    TAB      MOVE TO VALUE, BEGIN EDIT
                //                             ENTER    MOVE TO VALUE, BEGIN EDIT
                //                     FALSE   TAB      MOVE TO VALUE, BEGIN EDIT
                //                             ENTER    MOVE TO VALUE, BEGIN EDIT
                //            VALUE    TRUE    TAB      MOVE TO COMMENT, BEGIN EDIT
                //                             ENTER    MOVE TO NEXTROW NAME, SAVE
                //                     FALSE   TAB      MOVE TO NEXTROW NAME, SAVE
                //                             ENTER    MOVE TO NEXTROW NAME, SAVE
                //            COMMENT  TRUE    TAB      MOVE TO NEXTROW NAME, SAVE, BEGIN EDIT
                //                             ENTER    MOVE TO NEXTROW NAME, SAVE
                //                     FALSE   TAB      --
                //                             ENTER    --
                // FALSE      NAME     TRUE    TAB      MOVE TO VALUE
                //                             ENTER    MOVE TO NEXTROW NAME
                //                     FALSE   TAB      MOVE TO VALUE
                //                             ENTER    MOVE TO NEXTROW NAME
                //            VALUE    TRUE    TAB      MOVE TO COMMENT
                //                             ENTER    MOVE TO NEXTROW VALUE
                //                     FALSE   TAB      MOVE TO NEXTROW NAME
                //                             ENTER    MOVE TO NEXTROW VALUE
                //            COMMENT  TRUE    TAB      MOVE TO NEXTROW NAME
                //                             ENTER    MOVE TO NEXTOROW COMMENT
                //                     FALSE   TAB      --
                //                             ENTER    --

                int col = globalsgrid.CurrentCell.ColumnIndex;
                int row = globalsgrid.CurrentCell.RowIndex;
                bool commentVisible = globalsgrid.Columns[COMMENT_COL].Visible;
                bool isTab = e.KeyCode == Keys.Tab;
                if (Inserting) {
                    switch (col) {
                    case NAME_COL:
                        MoveTo(VALUE_COL, row, beginEdit: true);
                        break;

                    case VALUE_COL:
                        if (commentVisible && isTab) {
                            MoveTo(COMMENT_COL, row, beginEdit: true);
                        }
                        else {
                            FinishInsertAndMoveNext(NAME_COL, row + 1);
                        }
                        break;

                    case COMMENT_COL:
                        FinishInsertAndMoveNext(NAME_COL, row + 1, beginEdit: isTab);
                        break;
                    }
                }
                else {
                    switch (col) {
                    case NAME_COL:
                        MoveTo(isTab ? VALUE_COL : NAME_COL, isTab ? row : row + 1);
                        break;

                    case VALUE_COL:
                        if (isTab) {
                            MoveTo(commentVisible ? COMMENT_COL : NAME_COL,
                                   commentVisible ? row : row + 1);
                        }
                        else {
                            MoveTo(VALUE_COL, row + 1);
                        }
                        break;

                    case COMMENT_COL:
                        MoveTo(isTab ? NAME_COL : COMMENT_COL,
                               isTab ? row + 1 : row + 1);
                        break;
                    }
                }
                MarkAsChanged();
                return;

                void MoveTo(int col, int row, bool beginEdit = false) {
                    globalsgrid.CurrentCell = globalsgrid[col, row];
                    if (beginEdit)
                        globalsgrid.BeginEdit(true);
                }

                void FinishInsertAndMoveNext(int nextCol, int nextRow, bool beginEdit = false) {
                    MoveTo(nextCol, nextRow, beginEdit);

                    NextUndo = new() {
                        Action = GlobalsUndo.GlobalUndoAction.AddDefine,
                        Count = 1,
                        Pos = editrow.Index
                    };

                    editrow.Cells[ARGTYPE_COL].Value = EditDefine.Type;
                    editrow.Cells[DEFNAME_COL].Value = "";
                    editrow.Cells[DEFVALUE_COL].Value = "";
                    editrow.Tag = NextUID();

                    Inserting = false;
                    AddUndo(NextUndo);
                }
            }
        }

        private void EditTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (globalsgrid.CurrentCell.ColumnIndex == NAME_COL) {
                // check for and ignore invalid characters
                if (invalidall.Contains(e.KeyChar)) {
                    e.Handled = true;
                    return;
                }
                if (EditTextBox.SelectionStart == 0) {
                    if (invalid1st.Contains(e.KeyChar)) {
                        e.Handled = true;
                        return;
                    }
                }
                if (e.KeyChar > 122) {
                    e.Handled = true;
                    return;
                }
                if (e.KeyChar < 33 && e.KeyChar != 13 && e.KeyChar != 8) {
                    e.Handled = true;
                    return;
                }
            }
        }
        #endregion

        private void DragTimer_Tick(object sender, EventArgs e) {
            switch (dragDirection) {
            case -1:
                if (globalsgrid.FirstDisplayedScrollingRowIndex > 0) {
                    globalsgrid.FirstDisplayedScrollingRowIndex--;
                }
                else {
                    dragTimer.Stop();
                }
                break;
            case 1:
                if (!IsScrolledToBottom()) {
                    globalsgrid.FirstDisplayedScrollingRowIndex++;
                }
                else {
                    dragTimer.Stop();
                }
                break;
            case 0:
                dragTimer.Stop();
                break;
            }
        }
        #endregion

        #region Methods
        private void InitStatusStrip() {
            spStatus = MDIMain.spStatus;
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;
        }

        /// <summary>
        /// Initializes or updates the displayed fonts used by the editor.
        /// </summary>
        internal void InitFonts() {
            commonFont = new(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            boldFont = new(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, FontStyle.Bold);
            italicFont = new(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, FontStyle.Italic);
            globalsgrid.Font = commonFont;
            globalsgrid.ColumnHeadersDefaultCellStyle.Font = commonFont;
            globalsgrid.AlternatingRowsDefaultCellStyle.Font = commonFont;
            globalsgrid.AlternatingRowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }

        /// <summary>
        /// loads a defines list into the grid for editing
        /// </summary>
        /// <param name="GlobalFile"></param>
        /// <param name="ClearAll"></param>
        public bool LoadGlobalDefines(string GlobalFile, bool ingame) {
            bool error = false;
            string filetext;

            if (!File.Exists(GlobalFile)) {
                if (ingame) {
                    // create a new file
                    MDIMain.MsgBoxWithHelp(
                        "globals.txt is missing from this game's source directory. " +
                        "A blank file will be created.",
                        "Missing globals.txt",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        "htm\\winagi\\errors\\re24.htm");
                    File.WriteAllText(GlobalFile, "[\r\n[ global defines file for " +
                        EditGame.GameID + "\r\n[\r\n");
                }
                else {
                    // cancel
                    MessageBox.Show(MDIMain,
                        "File not found.",
                        "Defines Editor Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }
            }
            MDIMain.UseWaitCursor = true;
            // check for readonly
            if ((File.GetAttributes(GlobalFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                if (ingame) {
                    MDIMain.MsgBoxWithHelp(
                        "globals.txt is tagged as readonly. It cannot be edited " +
                        "unless full access is allowed.",
                        "Readonly globals.txt",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\winagi\\errors\\re25.htm");
                }
                else {
                    MessageBox.Show(MDIMain,
                        "This file is tagged as readonly. It cannot be edited " +
                        "unless full access is allowed.",
                        "Readonly Defines File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                return false;
            }
            try {
                // open file for input
                filetext = File.ReadAllText(GlobalFile);
            }
            catch (Exception) {
                // if error opening file, just exit
                if (ingame) {
                    MessageBox.Show(MDIMain,
                        "An error occurred while accessing globals.txt. " +
                        "It cannot be edited.",
                        "globals.txt File Access Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else {
                    MessageBox.Show(MDIMain,
                        "An error occurred while accessing this defines file. " +
                        "It cannot be edited.",
                        "Defines File Access Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                }
                return false;
            }
            Define[] defines = ReadDefines(filetext, ref error);
            for (int i = 0; i < defines.Length; i++) {
                int rownum = globalsgrid.Rows.Add(
                    defines[i].Type,
                    defines[i].Name, // default name
                    defines[i].Value, // default value
                    defines[i].Name,
                    defines[i].Value,
                    defines[i].Comment,
                    defines[i].NameType,
                    defines[i].ValueType);
                globalsgrid.Rows[rownum].Tag = NextUID();
            }
            InGame = ingame;
            if (InGame) {
                Text = "Defines Editor for " + EditGame.GameID;
            }
            else {
                Text = "Defines Editor - " + CompactPath(GlobalFile, 75);
            }
            IsChanged = false;
            Filename = GlobalFile;
            MDIMain.UseWaitCursor = false;
            if (error) {
                // inform user
                MessageBox.Show(MDIMain,
                    "One or more define entries were improperly formatted. Some entries may be missing or incorrect.",
                    "Global Defines File Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return true;
        }

        private Define[] ReadDefines(string definetext, ref bool errors) {
            errors = false;
            List<string> strings = [];
            string linetext;
            List<Define> retval = [];
            Define tmpDef = new();
            strings.AddLines(definetext);
            for (int i = 0; i < strings.Count; i++) {
                linetext = strings[i].Replace((char)Keys.Tab, ' ').Trim();
                // trim it - also, skip comments
                string cmt = "";
                linetext = StripComments(linetext, ref cmt, true);
                // ignore blanks
                if (linetext.Length != 0) {
                    AGIToken token = WinAGIFCTB.TokenFromPos(linetext, 0);
                    if (token.Text.Equals("#define", StringComparison.OrdinalIgnoreCase)) {
                        token = WinAGIFCTB.NextToken(linetext, token);
                        tmpDef.Name = token.Text;
                        tmpDef.Value = WinAGIFCTB.NextToken(linetext, token).Text;
                        tmpDef.Comment = cmt;
                        DefineNameCheck nametype = GetNameType(tmpDef.Name);
                        ArgType deftype = ArgType.None;
                        DefineValueCheck valuetype = GetValueType(tmpDef.Value, ref deftype);
                        if ((nametype != DefineNameCheck.OK && nametype < DefineNameCheck.Global) ||
                            (valuetype != DefineValueCheck.OK && valuetype < DefineValueCheck.BadArgNumber)) {
                            // something wrong with this entry
                            errors = true;
                        }
                        else {
                            tmpDef.Type = deftype;
                            tmpDef.NameType = nametype;
                            tmpDef.ValueType = valuetype;
                            retval.Add(tmpDef);
                        }
                    }
                    else {
                        // something wrong with this entry
                        errors = true;
                    }
                }
            }
            return retval.ToArray();
        }

        private static Regex BuildWholeTokenRegex(string token, string invalidChars) {
            string escapedInvalid = Regex.Escape(invalidChars);
            string pattern = $@"(?<![^{escapedInvalid}]){Regex.Escape(token)}(?![^{escapedInvalid}])";
            return new Regex(pattern);
        }

        private static bool ReplaceInEditor(WinAGIFCTB fctb, Regex pattern,
            Func<AGIToken, bool> tokenFilter,
            Func<string> replacementProvider) {
            bool changed = false;
            MatchCollection mc = pattern.Matches(fctb.Text);

            for (int i = mc.Count - 1; i >= 0; i--) {
                Place pl = fctb.PositionToPlace(mc[i].Index);
                AGIToken token = fctb.TokenFromPos(pl);

                if (tokenFilter(token)) {
                    fctb.ReplaceToken(token, replacementProvider());
                    changed = true;
                }
            }
            return changed;
        }

        private static bool ReplaceInSource(ref string source, Regex pattern,
            Func<AGIToken, bool> tokenFilter,
            Func<string> replacementProvider) {
            bool changed = false;
            MatchCollection mc = pattern.Matches(source);

            for (int i = mc.Count - 1; i >= 0; i--) {
                AGIToken token = WinAGIFCTB.TokenFromPos(source, mc[i].Index);

                if (tokenFilter(token)) {
                    source = source.ReplaceFirst(token.Text, replacementProvider(), mc[i].Index);
                    changed = true;
                }
            }

            return changed;
        }

        public void SaveDefinesList(string savefile = "") {
            // save list of globals
            if (globalsgrid.IsCurrentCellInEditMode) {
                return;
            }
            if (savefile.Length == 0) {
                if (Filename.Length == 0) {
                    savefile = NewSaveFileName(Filename);
                }
                else {
                    savefile = Filename;
                }
            }
            if (savefile.Length == 0) {
                return;
            }
            if (InGame) {
                // replace any changed defines with new names
                DialogResult rtn = DialogResult.No;
                bool dontAsk = false;
                switch (WinAGISettings.AutoUpdateDefines.Value) {
                case AskOption.Ask:
                    MDIMain.UseWaitCursor = false;
                    // get user's response
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to update all logics with any define names that have been changed?",
                        "Update Logics?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        "Always take this action when saving the global defines list.",
                        ref dontAsk,
                        WinAGIHelp, "htm\\winagi\\editingdefines.htm#edit");
                    if (dontAsk) {
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
                // build a list of defines with changed names
                // build a list of defines with changed values
                List<(string, string)> changedNames = [];
                List<string> changedValues = [];
                foreach (DataGridViewRow row in globalsgrid.Rows) {
                    string oldName = (string)row.Cells[DEFNAME_COL].Value;
                    string newName = (string)row.Cells[NAME_COL].Value;
                    if (oldName != newName && oldName.Length > 0) {
                        changedNames.Add((oldName, newName));
                    }
                    if ((string)row.Cells[DEFVALUE_COL].Value != "") {
                        if ((string)row.Cells[DEFVALUE_COL].Value != (string)row.Cells[VALUE_COL].Value) {
                            changedValues.Add((string)row.Cells[NAME_COL].Value);
                        }
                    }
                }
                if (rtn == DialogResult.Yes) {
                    // step through all defines; if the current name is different than
                    // the original name, change it in all logics
                    ProgressWin = new(this) {
                        Text = "Updating Global Defines",
                    };
                    ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count + LogicEditors.Count + 1;
                    ProgressWin.pgbStatus.Value = 0;
                    ProgressWin.lblProgress.Text = "Locating modified define names...";
                    ProgressWin.Show();
                    ProgressWin.Refresh();
                    foreach (frmLogicEdit loged in LogicEditors) {
                        if (loged.FormMode != LogicFormMode.Logic || !loged.InGame) {
                            continue;
                        }

                        // Deleted defines
                        foreach (var def in DeletedDefines) {
                            Regex rx = BuildWholeTokenRegex(def.Name, s_INVALID_DEFINE_CHARS);

                            ReplaceInEditor(
                                loged.fctb,
                                rx,
                                token => token.Type == AGITokenType.Identifier && token.Text == def.Name,
                                () => def.Value
                            );
                        }

                        // Renamed defines
                        foreach (var (oldName, newName) in changedNames) {
                            if (oldName != newName && oldName.Length > 0) {
                                Regex rx = BuildWholeTokenRegex(oldName, s_INVALID_DEFINE_CHARS);
                                ReplaceInEditor(
                                    loged.fctb,
                                    rx,
                                    token => token.Type == AGITokenType.Identifier && token.Text == oldName,
                                    () => newName
                                );
                            }
                        }
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.Refresh();
                    }

                    foreach (Logic logic in EditGame.Logics) {
                        bool unload = !logic.Loaded;
                        if (unload) {
                            logic.Load();
                        }
                        bool changed = false;
                        string src = logic.SourceText;

                        // Deleted defines
                        foreach (var def in DeletedDefines) {
                            Regex rx = BuildWholeTokenRegex(def.Name, s_INVALID_DEFINE_CHARS);

                            changed |= ReplaceInSource(
                                ref src,
                                rx,
                                token => token.Type == AGITokenType.Identifier && token.Text == def.Name,
                                () => def.Value
                            );
                        }

                        // Renamed defines
                        foreach (var (oldName, newName) in changedNames) {
                            Regex rx = BuildWholeTokenRegex(oldName, s_INVALID_DEFINE_CHARS);
                            changed |= ReplaceInSource(
                                ref src,
                                rx,
                                token => token.Type == AGITokenType.Identifier && token.Text == oldName,
                                () => newName
                            );
                        }

                        if (changed) {
                            logic.SourceText = src;
                            logic.SaveSource();
                            RefreshTree(AGIResType.Logic, logic.Number);
                        }
                        else {
                            // changed values (only if logic not already marked as changing
                            foreach (string defname in changedValues) {
                                int pos = src.IndexOf(defname);
                                while (pos != -1) {
                                    AGIToken token = WinAGIFCTB.TokenFromPos(src, pos);
                                    if (token.Text == defname && token.Type == AGITokenType.Identifier) {
                                        // found it
                                        break;
                                    }
                                    pos = src.IndexOf(defname, ++pos);
                                }
                                if (pos >= 0) {
                                    // mark this logic as changed
                                    changed = true;
                                    break;
                                }
                            }
                            if (changed) {
                                EditGame.Logics.MarkAsChanged(logic.Number);
                                RefreshTree(AGIResType.Logic, logic.Number);
                            }
                        }

                        if (unload) {
                            logic.Unload();
                        }
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.Refresh();
                    }

                    for (int i = 0; i < globalsgrid.RowCount - 1; i++) {
                        if (globalsgrid[DEFNAME_COL, i].Value != globalsgrid[NAME_COL, i].Value) {
                            globalsgrid[DEFNAME_COL, i].Value = globalsgrid[NAME_COL, i].Value;
                        }
                    }
                    DeletedDefines.Clear();
                    ProgressWin.Text = "Save Defines List";
                }
                else {
                    ProgressWin = new(this) {
                        Text = "Save Defines List",
                    };
                    ProgressWin.Show();
                }
            }
            else {
                // not in a game; just save it
                ProgressWin = new(this) {
                    Text = "Save Defines List",
                };
                ProgressWin.Show();
            }
            // done updating logics, now save the list
            ProgressWin.lblProgress.Text = "Saving defines to file ...";
            ProgressWin.pgbStatus.Value = 0;
            ProgressWin.pgbStatus.Maximum = globalsgrid.Rows.Count + 1;
            ProgressWin.Refresh();

            // update the globallist object
            if (InGame) {
                bool warning = false;
                DefinesList newlist = new();
                for (int i = 0; i < globalsgrid.Rows.Count - 1; i++) {
                    // confirm value is either an arg marker, number or literal string
                    if (!DefineValueIsValid((string)globalsgrid[VALUE_COL, i].Value)) {
                        // bad define - determine what to do
                        if (!warning) {
                            ProgressWin.Hide();
                            if (MDIMain.MsgBoxWithHelp(
                                        "One or more define entries are invalid. Do you want to save the list anyway?",
                                        "Invalid Define Name",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Error,
                                        "htm\\commands\\globaldefines.htm#errors") == DialogResult.No) {
                                // cancel the save
                                ProgressWin.Close();
                                ProgressWin.Dispose();
                                MDIMain.UseWaitCursor = false;
                                return;
                            }
                            ProgressWin.Show();
                            warning = true;
                        }
                    }
                    newlist.Add(
                        (string)globalsgrid[NAME_COL, i].Value,
                        (string)globalsgrid[VALUE_COL, i].Value,
                        (string)globalsgrid[COMMENT_COL, i].Value,
                        (ArgType)globalsgrid[ARGTYPE_COL, i].Value
                    );
                    ProgressWin.pgbStatus.Value += 1;
                    ProgressWin.Refresh();
                }
                // replace global list with the new list
                EditGame.GlobalDefines.Clone(newlist);
                EditGame.GlobalDefines.Save();
                MDIMain.ClearWarnings(AGIResType.Globals, 0);
                if (warning) {
                    WinAGIEventInfo warnInfo = new() {
                        ResType = AGIResType.Globals,
                        ResNum = 0,
                        Line = -1,
                        Type = EventType.ResourceWarning,
                        ID = "RW28",
                        Text = EngineResources.RW30,
                    };
                    MDIMain.AddWarning(warnInfo);
                    // then let open logic editors know
                    LogicListChange();
                }
                MarkAsSaved();
            }
            else {
                // save the file
                List<string> stlGlobals = BuildGlobalsFile(true);
                // if canceled, do nothing
                if (stlGlobals is not null) {
                    try {
                        File.WriteAllLines(savefile, stlGlobals);
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex,
                            "Unable to save due to file error.",
                            ex.StackTrace,
                            "File Error");
                    }
                    Filename = savefile;
                    MarkAsSaved();
                }
            }
            ProgressWin.Close();
            ProgressWin.Dispose();
            MDIMain.UseWaitCursor = false;
        }

        private string NewSaveFileName(string filename = "") {
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
            DialogResult rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            if (rtn == DialogResult.Cancel) {
                // nothing selected
                return "";
            }
            DefaultResDir = Path.GetDirectoryName(MDIMain.SaveDlg.FileName);
            return MDIMain.SaveDlg.FileName;
        }

        private void EditCell(int columnindex) {
            if (globalsgrid.CurrentRow.Index == globalsgrid.NewRowIndex || globalsgrid.CurrentCell.Value is null) {
                // only start new define in first column
                if (columnindex != NAME_COL) {
                    return;
                }
                EditDefine.Type = None;
                EditDefine.DefaultName = "";
                EditDefine.Name = "";
                EditDefine.Value = "\"\"";
                EditDefine.Comment = "";
            }
            else {
                EditDefine.Type = (ArgType)globalsgrid[ARGTYPE_COL, globalsgrid.CurrentRow.Index].Value;
                EditDefine.DefaultName = (string)globalsgrid[DEFNAME_COL, globalsgrid.CurrentRow.Index].Value;
                EditDefine.DefaultValue = (string)globalsgrid[DEFVALUE_COL, globalsgrid.CurrentRow.Index].Value;
                EditDefine.Name = (string)globalsgrid[NAME_COL, globalsgrid.CurrentRow.Index].Value;
                EditDefine.Value = (string)globalsgrid[VALUE_COL, globalsgrid.CurrentRow.Index].Value;
                EditDefine.Comment = (string)globalsgrid[COMMENT_COL, globalsgrid.CurrentRow.Index].Value;
            }
            globalsgrid.BeginEdit(true);
        }

        private void AddUndo(GlobalsUndo NextUndo) {
            if (!IsChanged) {
                MarkAsChanged();
            }
            UndoCol.Push(NextUndo);
        }

        public void AddGlobalsFromFile() {
            // adds to the existing globals list, instead of replacing it
            if (globalsgrid.IsCurrentCellInEditMode) {
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
            DefaultResDir = Path.GetDirectoryName(MDIMain.OpenDlg.FileName);

            bool error = false;
            string definetext;
            try {
                // open file for input
                using FileStream fsGlobal = new(MDIMain.OpenDlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader srGlobal = new(fsGlobal);

                char[] chText = new char[fsGlobal.Length];
                srGlobal.ReadBlock(chText, 0, (int)fsGlobal.Length);
                fsGlobal.Dispose();
                srGlobal.Dispose();
                definetext = new string(chText);
            }
            catch (Exception) {
                // if error opening file, just exit
                MessageBox.Show(MDIMain,
                    "An error occurred while accessing this defines file. No defines were imported.",
                    "Defines File Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            GlobalsUndo NextUndo = new();
            NextUndo.Action = GlobalsUndo.GlobalUndoAction.ImportDefines;
            NextUndo.Pos = globalsgrid.NewRowIndex;
            Define[] addDefines = ReadDefines(definetext, ref error);
            if (addDefines.Length == 0) {
                // nothing to paste
                MessageBox.Show(MDIMain,
                    "There are no valid define entries on the clipboard.",
                    "Unable to Paste Defines",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            Define[] undodata = InsertDefines(addDefines, globalsgrid.NewRowIndex, ref error);
            // if nothing was added, no undo data exists
            if (undodata.Length == 0) {
                MessageBox.Show(MDIMain,
                    "There were no usable data in the file, so nothing was pasted.",
                    "Nothing to Paste",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else {
                NextUndo.Count = undodata.Length;
                for (int i = 0; i < undodata.Length; i++) {
                    NextUndo.UDDefine[i] = undodata[i];
                }
                AddUndo(NextUndo);
            }
            MDIMain.UseWaitCursor = false;
            if (error) {
                MessageBox.Show(MDIMain,
                    "Some entries could not be added because of formatting or syntax errors.",
                    "Paste Errors",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private Define[] InsertDefines(Define[] PasteDefines, int insertrow, ref bool errors) {
            Define tmpdef;
            Define[] retval = [];
            int replacerow;

            errors = false;
            for (int i = 0; i < PasteDefines.Length; i++) {
                // coming from internal clipboard means no concerns
                //  about formatting, so just validate and add it

                DefineNameCheck nametype = GetNameType(PasteDefines[i].Name);
                ArgType t = ArgType.None;
                DefineValueCheck valuetype = GetValueType(PasteDefines[i].Value, ref t);
                if (((int)nametype > 0 && (int)nametype <= 7) || ((int)valuetype > 0 && (int)valuetype <= 3)) {
                    errors = true;
                }
                else {
                    // check for defines that replace existing defines
                    if (nametype == DefineNameCheck.Global) {
                        string oldval = "";
                        for (replacerow = 0; replacerow < globalsgrid.RowCount; replacerow++) {
                            if ((string)globalsgrid[NAME_COL, replacerow].Value == PasteDefines[i].Name) {
                                oldval = (string)globalsgrid[VALUE_COL, replacerow].Value;
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
                                WinAGIHelp, "htm\\winagi\\globaldefines.htm#syntax");
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
                            tmpdef.Type = (ArgType)globalsgrid[ARGTYPE_COL, replacerow].Value;
                            tmpdef.Name = replacerow.ToString();
                            tmpdef.Value = oldval;
                            tmpdef.Comment = (string)globalsgrid[COMMENT_COL, replacerow].Value;
                            tmpdef.UID = (int)globalsgrid.Rows[replacerow].Tag;
                            Array.Resize(ref retval, retval.Length + 1);
                            retval[^1] = tmpdef;
                            // make the replacement
                            globalsgrid[ARGTYPE_COL, replacerow].Value = PasteDefines[i].Type;
                            globalsgrid[VALUE_COL, replacerow].Value = PasteDefines[i].Value;
                            globalsgrid[COMMENT_COL, replacerow].Value = PasteDefines[i].Comment;
                        }
                        continue;
                    }
                    // any other condition is ok
                    tmpdef = new();
                    tmpdef.Name = insertrow.ToString();
                    Array.Resize(ref retval, retval.Length + 1);
                    retval[^1] = tmpdef;
                    // add a new row
                    globalsgrid.Rows.Insert(insertrow,
                        PasteDefines[i].Type,
                        PasteDefines[i].DefaultName,
                        PasteDefines[i].Name,
                        PasteDefines[i].Value,
                        PasteDefines[i].Comment,
                        PasteDefines[i].NameType,
                        PasteDefines[i].ValueType);
                    globalsgrid.Rows[insertrow++].Tag = NextUID();
                }
            }
            return retval;
        }

        public DefineNameCheck GetNameType(string checkname) {
            // use a separate check than the one in DefinesList because the 
            // entries in the grid are not put into the defines list until the
            // user saves the file

            // basic checks
            DefineNameCheck retval = BaseNameCheck(checkname, false);
            if (retval != DefineNameCheck.OK) {
                return retval;
            }
            // check against globals in this list
            for (int i = 0; i < globalsgrid.Rows.Count; i++) {
                // skip empty rows
                if (i != globalsgrid.NewRowIndex) {
                    if (i != globalsgrid.CurrentRow.Index && globalsgrid[NAME_COL, i].Value.ToString() == checkname)
                        return DefineNameCheck.Global;
                }
            }
            // resourceIDs
            if (EditGame is not null && EditGame.IncludeIDs) {
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
            // check against basic reserved
            if (EditGame is not null && EditGame.IncludeReserved) {
                // reserved variables
                Define[] tmpDefines = EditGame.ReservedDefines.ReservedVariables;
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedVar;
                    }
                }
                // reserved flags
                tmpDefines = EditGame.ReservedDefines.ReservedFlags;
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedFlag;
                    }
                }
                // reserved numbers
                tmpDefines = EditGame.ReservedDefines.ByArgType(Num);
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedNum;
                    }
                }
                // reserved objects
                tmpDefines = EditGame.ReservedDefines.ReservedObjects;
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedObj;
                    }
                }
                // reserved strings
                tmpDefines = EditGame.ReservedDefines.ReservedStrings;
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (checkname == tmpDefines[i].Name) {
                        return DefineNameCheck.ReservedStr;
                    }
                }
                // game info:
                for (int i = 0; i < EditGame.ReservedDefines.GameInfo.Length; i++) {
                    if (checkname == EditGame.ReservedDefines.GameInfo[i].Name)
                        // invobj count is number; rest are msgstrings
                        return i == 3 ? DefineNameCheck.ReservedNum : DefineNameCheck.ReservedMsg;
                }
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        public DefineValueCheck GetValueType(string checkvalue, ref ArgType type) {
            // use a separate check than the one in DefinesList because the 
            // entries in the grid are not put into the defines list until the
            // user saves the file

            // default type
            type = None;

            if (checkvalue.Length == 0) {
                return DefineValueCheck.Empty;
            }
            // values must be an AGI argument marker (variable/flag/etc),
            // literal string, or a number

            if (int.TryParse(checkvalue, out int argvalue)) {
                // numeric
                // unsigned byte (0-255) or signed byte (-128 to 127) are OK
                if (argvalue > -128 && argvalue < 256) {
                    type = Num;
                    return DefineValueCheck.OK;
                }
                else {
                    return DefineValueCheck.OutofBounds;
                }
            }
            else {
                if ("vfmoiswc".Contains(checkvalue[0])) {
                    string valueText = checkvalue[1..];
                    if (int.TryParse(valueText, out argvalue)) {
                        if (argvalue < 0 || argvalue > 255) {
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
                            type = MsgNum;
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
                        for (int i = 0; i < globalsgrid.Rows.Count; i++) {
                            if (i != globalsgrid.NewRowIndex) {
                                if (i != globalsgrid.CurrentRow.Index && globalsgrid[VALUE_COL, i].Value.ToString() == checkvalue)
                                    return DefineValueCheck.Global;
                            }
                        }
                        // check reserved
                        if (EditGame is not null && EditGame.IncludeReserved) {
                            switch (type) {
                            case Flag:
                                if (argvalue <= 15)
                                    return DefineValueCheck.Reserved;
                                if (argvalue == 20) {
                                    switch (EditGame.InterpreterVersion.Index) {
                                    case AGIVersion.v3002098:
                                    case AGIVersion.v3002102:
                                    case AGIVersion.v3002107:
                                    case AGIVersion.v3002149:
                                        return DefineValueCheck.Reserved;
                                    }
                                }
                                break;
                            case Var:
                                if (argvalue <= 26)
                                    return DefineValueCheck.Reserved;
                                break;
                            case MsgNum:
                                break;
                            case SObj:
                                if (argvalue == 0)
                                    return DefineValueCheck.Reserved;
                                break;
                            case InvItem:
                                break;
                            case Str:
                                if (argvalue == 0)
                                    return DefineValueCheck.Reserved;
                                if (argvalue > 23 || (argvalue > 11 &&
                                    (EditGame.InterpreterVersion.Index == AGIVersion.v2089 ||
                                    EditGame.InterpreterVersion.Index == AGIVersion.v2272 ||
                                    EditGame.InterpreterVersion.Index == AGIVersion.v3002149))) {
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
                                if (argvalue < 1 || argvalue > 10) {
                                    return DefineValueCheck.BadArgNumber;
                                }
                                break;
                            case Ctrl:
                                // controllers limited to 0-49
                                if (argvalue > 49) {
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
                if (FanLogicCompiler.IsAGIString(checkvalue)) {
                    type = DefStr;
                    return DefineValueCheck.OK;
                }
                else {
                    return DefineValueCheck.NotAValue;
                }
            }
        }

        private bool DefineValueIsValid(string definevalue) {
            if (definevalue.Length == 0) {
                return false;
            }
            // values must be an AGI argument marker (variable/flag/etc),
            // literal string, or a number

            if (int.TryParse(definevalue, out int numvalue)) {
                // numeric
                // unsigned byte (0-255) or signed byte (-128 to 127) are OK
                if (numvalue >= -128 && numvalue < 256) {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                // non-numeric, check for arg marker
                if ("vfmoiswc".Contains(definevalue[0])) {
                    string valueText = definevalue[1..];
                    if (int.TryParse(valueText, out numvalue)) {
                        return numvalue >= 0 && numvalue <= 255;
                    }
                }
                // non-numeric, non-marker and most likely a string
                if (FanLogicCompiler.IsAGIString(definevalue)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        public List<string> BuildGlobalsFile(bool Progress = false) {
            string name, value, comment;
            int maxLength, maxValLen = 0;
            Define tmpDef = new();
            List<string> output;

            // determine longest name length to facilitate aligning values
            maxLength = 0;
            for (int i = 0; i < globalsgrid.Rows.Count - 1; i++) {
                if (((string)globalsgrid[NAME_COL, i].Value).Length > maxLength) {
                    maxLength = ((string)globalsgrid[NAME_COL, i].Value).Length;
                }
                // right-align non-strings; need to know length of longest
                // non-string
                if (((string)globalsgrid[VALUE_COL, i].Value)[0] != '"') {
                    if (((string)globalsgrid[VALUE_COL, i].Value).Length > maxValLen) {
                        maxValLen = ((string)globalsgrid[VALUE_COL, i].Value).Length;
                    }
                }
            }
            output = [];
            // add a useful header
            output.Add("[");
            output.Add("[ global defines file for " + EditGame.GameID);
            output.Add("[");
            if (Progress) {
                ProgressWin.pgbStatus.Value = 1;
                ProgressWin.pgbStatus.Invalidate();
                ProgressWin.pgbStatus.Update();
                ProgressWin.pgbStatus.Refresh();
                System.Threading.Thread.Sleep(1); // tiny yield, no UI interaction
                ProgressWin.Refresh();
            }
            bool warning = false;
            for (int i = 0; i < globalsgrid.Rows.Count - 1; i++) {
                // get name and Value
                name = ((string)globalsgrid[NAME_COL, i].Value).PadRight(maxLength);
                value = ((string)globalsgrid[VALUE_COL, i].Value);
                // confirm value is either an arg marker, number or literal string
                if (!DefineValueIsValid(value)) {
                    // bad define - determine what to do
                    if (!warning) {
                        if (MDIMain.MsgBoxWithHelp(
                                    "One or more define entries are invalid. Do you want to save the list anyway?",
                                    "Invalid Define Name",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Error,
                                    "htm\\commands\\globaldefines.htm#errors") == DialogResult.No) {
                            // cancel the save
                            return null;
                        }
                        warning = true;
                    }
                }
                value = value.PadLeft(4);
                // right align non-strings
                if (value[0] != '"') {
                    value = value.PadLeft(maxValLen);
                }
                comment = (string)globalsgrid[COMMENT_COL, i].Value;
                if (comment.Length > 0) {
                    comment = " " + comment;
                }
                output.Add("#define " + name + "  " + value + comment);
                if (Progress) {
                    ProgressWin.pgbStatus.Value++;
                    ProgressWin.pgbStatus.Invalidate();
                    ProgressWin.pgbStatus.Update();
                    ProgressWin.pgbStatus.Refresh();
                    ProgressWin.Refresh();
                    System.Threading.Thread.Sleep(1); // tiny yield, no UI interaction
                }
            }
            return output;
        }

        private void RemoveRows(int TopRow, int BtmRow, bool DontUndo = false) {
            if (BtmRow < TopRow) {
                int swap = BtmRow;
                BtmRow = TopRow;
                TopRow = swap;
            }
            if (!DontUndo) {
                GlobalsUndo NextUndo = new GlobalsUndo();
                Define tmpDef = new();
                NextUndo.Action = GlobalsUndo.GlobalUndoAction.DeleteDefine;
                NextUndo.Count = BtmRow - TopRow + 1;
                NextUndo.Pos = TopRow;
                for (int i = 0; i <= BtmRow - TopRow; i++) {
                    tmpDef.Type = (ArgType)globalsgrid[ARGTYPE_COL, TopRow + i].Value;
                    tmpDef.DefaultName = (string)globalsgrid[DEFNAME_COL, TopRow + i].Value;
                    tmpDef.DefaultValue = (string)globalsgrid[DEFVALUE_COL, TopRow + i].Value;
                    tmpDef.Name = (string)globalsgrid[NAME_COL, TopRow + i].Value;
                    tmpDef.Value = (string)globalsgrid[VALUE_COL, TopRow + i].Value;
                    tmpDef.Comment = (string)globalsgrid[COMMENT_COL, TopRow + i].Value;
                    tmpDef.UID = (int)globalsgrid.Rows[TopRow + i].Tag;
                    NextUndo.UDDefine[i] = tmpDef;
                    if (tmpDef.DefaultName.Length > 0) {
                        DelDefine deldef = new() {
                            Name = tmpDef.DefaultName,
                            Value = tmpDef.DefaultValue,
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
                globalsgrid.Rows.RemoveAt(i);
            }
            globalsgrid.Refresh();
        }

        private void RemoveRow(int RowIndex, bool DontUndo = false) {
            if (!DontUndo) {
                GlobalsUndo NextUndo = new() {
                    Action = GlobalsUndo.GlobalUndoAction.DeleteDefine,
                    Count = 1,
                    Pos = RowIndex
                };
                Define tmpDef = new() {
                    Type = (ArgType)globalsgrid[ARGTYPE_COL, RowIndex].Value,
                    DefaultName = (string)globalsgrid[DEFNAME_COL, RowIndex].Value,
                    DefaultValue = (string)globalsgrid[DEFVALUE_COL, RowIndex].Value,
                    Name = (string)globalsgrid[NAME_COL, RowIndex].Value,
                    Value = (string)globalsgrid[VALUE_COL, RowIndex].Value,
                    Comment = (string)globalsgrid[COMMENT_COL, RowIndex].Value,
                    UID = (int)globalsgrid.Rows[RowIndex].Tag
                };
                NextUndo.UDDefine[0] = tmpDef;
                AddUndo(NextUndo);
                if (tmpDef.DefaultName.Length > 0) {
                    DelDefine deldef = new() {
                        Name = tmpDef.DefaultName,
                        Value = tmpDef.DefaultValue
                    };
                    DeletedDefines.Add(deldef);
                }
            }
            globalsgrid.Rows.RemoveAt(RowIndex);
            MarkAsChanged();
        }

        private void MoveRows(int tgtrow, bool DontUndo = false) {
            int undostart, undoend, undotarget;
            int count = dragEndRow - dragStartRow + 1;

            // move the rows
            targetRowIndex = tgtrow;
            if (targetRowIndex < dragStartRow) {
                // calculate undo values
                undostart = targetRowIndex;
                undoend = undostart + count - 1;
                undotarget = dragEndRow + 1;

                int rownum = targetRowIndex;
                for (int i = dragStartRow; i <= dragEndRow; i++) {
                    var row = globalsgrid.Rows[i];
                    globalsgrid.Rows.Remove(row);
                    globalsgrid.Rows.Insert(rownum++, row);
                }
            }
            else {
                // calculate undo values
                undostart = targetRowIndex - count;
                undoend = targetRowIndex - 1;
                undotarget = dragStartRow;

                for (int i = dragStartRow; i <= dragEndRow; i++) {
                    var row = globalsgrid.Rows[dragStartRow];
                    globalsgrid.Rows.Remove(row);
                    globalsgrid.Rows.Insert(targetRowIndex - 1, row);
                }
            }

            // let the PrePaint event know that the rows
            // need to be re-selected
            dropping = true;
            if (!DontUndo) {
                // add to undo
                GlobalsUndo NextUndo = new GlobalsUndo();
                NextUndo.Action = GlobalsUndo.GlobalUndoAction.MoveRows;
                NextUndo.Start = undostart;
                NextUndo.End = undoend;
                NextUndo.Pos = undotarget;
                AddUndo(NextUndo);
            }
        }

        /// <summary>
        /// Gets the next unique ID number, used to manage sorting undo ops.
        /// </summary>
        /// <returns></returns>
        private int NextUID() {
            return UID++;
        }

        private void RestoreSortOrder(List<int> oldOrder) {
            // mapp from UID to row
            var rowMap = new Dictionary<int, DataGridViewRow>();
            foreach (DataGridViewRow row in globalsgrid.Rows) {
                if (!row.IsNewRow && row.Tag is int uid) {
                    rowMap[uid] = row;
                }
            }

            // remove all rows except the new row
            for (int i = globalsgrid.Rows.Count - 1; i >= 0; i--) {
                if (!globalsgrid.Rows[i].IsNewRow) {
                    globalsgrid.Rows.RemoveAt(i);
                }
            }

            // re-insert rows in the saved order
            foreach (int uid in oldOrder) {
                if (rowMap.TryGetValue(uid, out var row)) {
                    // Clone the row to avoid issues with row state
                    var newRow = (DataGridViewRow)row.Clone();
                    for (int i = 0; i < row.Cells.Count; i++) {
                        newRow.Cells[i].Value = row.Cells[i].Value;
                    }
                    newRow.Tag = uid;
                    globalsgrid.Rows.Insert(globalsgrid.Rows.Count - 1, newRow);
                }
            }
        }

        bool IsScrolledToBottom() {
            if (globalsgrid.RowCount == 0) {
                return true;
            }

            int lastVisibleRowIndex = globalsgrid.FirstDisplayedScrollingRowIndex + globalsgrid.DisplayedRowCount(false) - 1;
            int lastRowIndex = globalsgrid.RowCount - 1;
            return lastVisibleRowIndex >= lastRowIndex;
        }

        internal void ShowHelp() {
            string topic = "htm\\winagi\\editor_globals.htm";
            if (EditTextBox is not null && EditTextBox.Visible) {
                if (Inserting) {
                    topic += "#new";
                }
                else {
                    topic += "#edit";
                }
            }
            else {
                topic += "#editor";
            }
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }

        private bool AskClose() {
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save this defines list?",
                    "Save Defines List",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveDefinesList();
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

        private void MarkAsChanged() {
            // ignore when loading (not visible yet)
            if (!Visible) {
                return;
            }
            if (!IsChanged) {
                IsChanged = true;
                mnuRSave.Enabled = true;
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
            FindingForm.ResetSearch();
        }

        private void MarkAsSaved() {
            if (InGame) {
                Text = "Defines Editor for " + EditGame.GameID;
            }
            else {
                Text = "Defines Editor - " + CompactPath(Filename, 75);
            }
            IsChanged = false;
            mnuRSave.Enabled = false;
            MDIMain.btnSaveResource.Enabled = false;
        }
    }
    #endregion
}
