using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.ObjectsUndo.ActionType;
namespace WinAGI.Editor {
    public partial class frmObjectEdit : Form {
        public bool InGame;
        public bool IsChanged;
        public InventoryList EditInvList;
        private string EditInvListFilename;
        private bool closing = false;
        private Stack<ObjectsUndo> UndoCol = [];
        private TextBox EditTextBox = null;
        private InventoryItem EditItem = new();
        private bool Adding = false;
        public frmObjectEdit() {
            InitializeComponent();
            // set default row style
            fgObjects.Columns[0].ValueType = typeof(int);
            fgObjects.Columns[1].ValueType = typeof(string);
            fgObjects.Columns[2].ValueType = typeof(byte);
            InitFonts();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers
        private void frmObjectEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmObjectEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // ensure object is cleared and dereferenced

            if (EditInvList != null) {
                EditInvList.Unload();
                EditInvList = null;
            }
            if (InGame) {
                OEInUse = false;
                ObjectEditor = null;
            }
        }

        private void frmObjectEdit_Leave(object sender, EventArgs e) {
            // if editing, need to cancel; otherwise, the edit text box
            // control stays active, and any other form will not be able
            // to ediit its grid cells
            if (fgObjects.IsCurrentCellInEditMode) {
                // same as pressing Escape
                EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Escape));
            }
        }

        private void frmObjectEdit_Load(object sender, EventArgs e) {

        }
        #endregion

        #region Menu Event Handlers
        internal void SetResourceMenu() {

            mnuRSave.Enabled = IsChanged;
            MDIMain.mnuRSep3.Visible = true;
            if (EditGame is null) {
                // no game is open
                MDIMain.mnuRImport.Enabled = false;
                mnuRExport.Text = "Save As ...";
                // mnuRProperties no change
                mnuRToggleEncrypt.Checked = EditInvList.Encrypted;
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export OBJECT" : "Save As ...";
                // mnuRProperties no change
                mnuRExportLoopGIF.Enabled = true; // = loop or cel selected
            }
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            SaveObjects();
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            ExportObjects();
        }

        public void mnuRProperties_Click(object sender, EventArgs e) {
            EditProperties();
        }

        private void mnuRToggleEncrypt_Click(object sender, EventArgs e) {
            SetEncryption(!EditInvList.Encrypted);
        }

        private void SetEditMenu() {
            if (fgObjects.IsCurrentCellInEditMode) {
                mnuEUndo.Enabled = false;
                mnuEDelete.Enabled = false;
                mnuEClear.Enabled = false;
                mnuEInsert.Enabled = false;
                mnuEFind.Enabled = false;
                mnuEFindAgain.Enabled = false;
                mnuEReplace.Enabled = false;
                mnuEditItem.Enabled = false;
                mnuEFindInLogic.Enabled = false;
                return;
            }
            mnuEUndo.Enabled = UndoCol.Count > 0;
            mnuEUndo.Text = "Undo ";
            if (UndoCol.Count > 0) {
                mnuEUndo.Text += LoadResString((int)(OBJUNDOTEXT + UndoCol.Peek().UDAction));
            }
            // item i0 can not be deleted
            mnuEDelete.Enabled = fgObjects.CurrentCell.RowIndex > 1;
            mnuEClear.Enabled = true;
            mnuEInsert.Enabled = EditInvList.Count < 256;
            mnuEFind.Enabled = true;
            mnuEFindAgain.Enabled = GFindText.Length != 0;
            mnuEReplace.Enabled = true;
            mnuEditItem.Enabled = fgObjects.CurrentRow.Index >= 0 && fgObjects.CurrentRow.Index != fgObjects.NewRowIndex;
            switch (fgObjects.CurrentCell.ColumnIndex) {
            case 0:
                mnuEditItem.Visible = false;
                break;
            case 1:
                mnuEditItem.Visible = true;
                mnuEditItem.Text = "Edit Item";
                break;
            case 2:
                mnuEditItem.Visible = true;
                mnuEditItem.Text = "Edit Room";
                break;
            }
            //enable findinlogics if object is not a ' ? '
            mnuEFindInLogic.Enabled = (string)fgObjects[1, fgObjects.CurrentCell.RowIndex].Value != "?";
        }

        private void ResetEditMenu() {
            mnuEUndo.Enabled = true;
            mnuEDelete.Enabled = true;
            mnuEClear.Enabled = true;
            mnuEInsert.Enabled = true;
            mnuEFind.Enabled = true;
            mnuEFindAgain.Enabled = true;
            mnuEReplace.Enabled = true;
            mnuEditItem.Enabled = true;
            mnuEFindInLogic.Enabled = true;
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuEUndo, toolStripSeparator1, mnuEDelete, mnuEClear, mnuEInsert, toolStripSeparator2, mnuEFind, mnuEFindAgain, mnuEReplace, toolStripSeparator3, mnuEditItem, mnuEFindInLogic });
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            cmGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuEUndo, toolStripSeparator1, mnuEDelete, mnuEClear, mnuEInsert, toolStripSeparator2, mnuEFind, mnuEFindAgain, mnuEReplace, toolStripSeparator3, mnuEditItem, mnuEFindInLogic });
            ResetEditMenu();
        }

        private void cmEdit_Opening(object sender, CancelEventArgs e) {
            if (fgObjects.IsCurrentCellInEditMode) {
                e.Cancel = true;
                return;
            }
            SetEditMenu();
        }

        private void cmEdit_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEUndo_Click(object sender, EventArgs e) {
            if (UndoCol.Count == 0) {
                return;
            }
            ObjectsUndo NextUndo = UndoCol.Pop();
            switch (NextUndo.UDAction) {
            case AddItem:
                fgObjects.Rows.RemoveAt(EditInvList.Count - 1);
                EditInvList.Remove(EditInvList.Count - 1);
                fgObjects[1, EditInvList.Count - 1].Selected = true;
                break;
            case DeleteItem:
                if (NextUndo.UDObjectNo == EditInvList.Count) {
                    EditInvList.Add(NextUndo.UDObjectText, NextUndo.UDObjectRoom);
                    fgObjects.Rows.Add();
                    fgObjects[0, NextUndo.UDObjectNo].Value = NextUndo.UDObjectNo;
                }
                else {
                    EditInvList[NextUndo.UDObjectNo].ItemName = NextUndo.UDObjectText;
                    EditInvList[NextUndo.UDObjectNo].Room = NextUndo.UDObjectRoom;
                }
                fgObjects[1, NextUndo.UDObjectNo].Value = NextUndo.UDObjectText;
                fgObjects[2, NextUndo.UDObjectNo].Value = NextUndo.UDObjectRoom;
                fgObjects[1, NextUndo.UDObjectNo].Selected = true;
                break;
            case ModifyItem:
            case Replace:
                fgObjects[1, NextUndo.UDObjectNo].Value = NextUndo.UDObjectText;
                EditInvList[NextUndo.UDObjectNo].ItemName = NextUndo.UDObjectText;
                break;
            case ModifyRoom:
                fgObjects[2, NextUndo.UDObjectNo].Value = NextUndo.UDObjectRoom;
                EditInvList[NextUndo.UDObjectNo].Room = NextUndo.UDObjectRoom;
                break;
            case ChangeMaxObj:
                // old Max is stored in room variable
                ModifyMax(NextUndo.UDObjectRoom, true);
                return;
            case TglEncrypt:
                SetEncryption(NextUndo.UDObjectRoom == 1, true);
                break;
            case Clear:
                //// if undoing a clear, EditInvList is already empty so don't
                //// need to clear it; grid will already be empty also

                //// restore Max objects
                //ModifyMax(NextUndo.UDObjectRoom, true);
                //// restore encryption
                //ToggleEncryption((bool)NextUndo.UDObjectRoom, true);
                //// split out items
                //string[] strObjs = NextUndo.UDObjectText.Split('\r');
                //for (int i = 0; i < strObjs.Length; i++) {
                //    if (i != 0) {
                //        int newrow = fgObjects.Rows.Add();
                //        fgObjects[0, newrow].Value = EditInvList.Count;
                //        fgObjects[1, newrow].Value = strObjs[i];
                //    }
                //}
                break;
            case ObjectsUndo.ActionType.ReplaceAll:
                //// udstring has previous items in pipe-delimited string
                //string[] strObjs = NextUndo.UDObjectText.Split('|');
                //// object numbers and old values are in pairs
                //for (int i = 0; i < strObjs.Length; i++) {
                //    // first element is obj number, second is old obj desc
                //    EditInvList[byte.Parse(strObjs[i++])].ItemName = strObjs[i];
                //    fgObjects[1, int.Parse(strObjs[i++])].Value = strObjs[i];
                //}
                break;
            }
            //ensure selected row is visible
            fgObjects.FirstDisplayedScrollingRowIndex = fgObjects.CurrentRow.Index;
        }

        private void mnuEDelete_Click(object sender, EventArgs e) {
            if (fgObjects.CurrentRow.Index == fgObjects.NewRowIndex ||
                fgObjects.CurrentRow.Index < 0) {
                return;
            }
            ObjectsUndo NextUndo = new() {
                UDAction = DeleteItem,
                UDObjectNo = fgObjects.CurrentRow.Index,
                UDObjectText = (string)fgObjects[1, fgObjects.CurrentRow.Index].Value,
                UDObjectRoom = (byte)fgObjects[2, fgObjects.CurrentRow.Index].Value
            };
            AddUndo(NextUndo);
            if (fgObjects.CurrentRow.Index == fgObjects.NewRowIndex - 1) {
                EditInvList.Remove(fgObjects.CurrentRow.Index);
                fgObjects.Rows.RemoveAt(fgObjects.CurrentRow.Index);
            }
            else {
                EditInvList[fgObjects.CurrentRow.Index].ItemName = "?";
                EditInvList[fgObjects.CurrentRow.Index].Room = 0;
                fgObjects[1, fgObjects.CurrentRow.Index].Value = "?";
                fgObjects[2, fgObjects.CurrentRow.Index].Value = (byte)0;
            }
            MarkAsChanged();
        }

        private void mnuEClear_Click(object sender, EventArgs e) {
            ObjectsUndo NextUndo = new();
            NextUndo.UDAction = Clear;
            NextUndo.UDObjectRoom = EditInvList.MaxScreenObjects;
            NextUndo.UDObjectNo = EditInvList.Encrypted ? 1 : 0;
            NextUndo.UDObjectText = EditInvList[0].ItemName + '|' + EditInvList[0].Room.ToString();
            for (int i = 1; i < EditInvList.Count - 1; i++) {
                NextUndo.UDObjectText += '\r' + EditInvList[i].ItemName + '|' + EditInvList[i].Room.ToString();
            }
            AddUndo(NextUndo);
            fgObjects.Rows.Clear();
            fgObjects.Rows.Add();
            fgObjects.Rows[0].SetValues(new object[] { 0, "?", 0 });
            fgObjects.Rows[0].Selected = true;
            EditInvList.Clear();
            EditInvList.MaxScreenObjects = WinAGISettings.DefMaxSO.Value;
            EditInvList.Encrypted = false;
            txtMaxScreenObjs.Text = EditInvList.MaxScreenObjects.ToString();
            MarkAsChanged();
        }

        private void mnuEInsert_Click(object sender, EventArgs e) {
            if (fgObjects.IsCurrentCellInEditMode) {
                return;
            }
            EditItem.ItemName = "";
            EditItem.Room = 0;

            // force row to newrow, column to itemname
            fgObjects[1, fgObjects.NewRowIndex].Selected = true;
            fgObjects.Refresh();
            Adding = true;
            fgObjects.BeginEdit(true);
        }

        private void mnuEFind_Click(object sender, EventArgs e) {
            MessageBox.Show("find");
        }

        private void mnuEFindAgain_Click(object sender, EventArgs e) {
            MessageBox.Show("find again");
        }

        private void mnuEReplace_Click(object sender, EventArgs e) {
            MessageBox.Show("replace");
        }

        private void mnuEditItem_Click(object sender, EventArgs e) {
            if (fgObjects.CurrentRow.Index == fgObjects.NewRowIndex) {
                EditItem.ItemName = "";
                EditItem.Room = 0;
                Adding = true;
            }
            else {
                EditItem.ItemName = (string)fgObjects[1, fgObjects.CurrentRow.Index].Value;
                EditItem.Room = (byte)fgObjects[2, fgObjects.CurrentRow.Index].Value;
                Adding = false;
            }
            fgObjects.BeginEdit(true);
        }

        private void mnuEFindInLogic_Click(object sender, EventArgs e) {
            MessageBox.Show("find in logic");
        }

        private void cmCel_Opening(object sender, CancelEventArgs e) {
            mnuCelUndo.Enabled = EditTextBox.CanUndo;
            mnuCelCut.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCopy.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelPaste.Enabled = Clipboard.ContainsText();
            mnuCelDelete.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCharMap.Visible = fgObjects.CurrentCell.ColumnIndex == 1;
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
        }

        private void mnuCelCharMap_Click(object sender, EventArgs e) {
            if (fgObjects.CurrentCell.ColumnIndex != 1) {
                return;
            }
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
            fgObjects.CancelEdit();
            // cancel alone doesn't work (the cell remains in edit mode)
            // but calling EndEdit immediately after seems to work
            fgObjects.EndEdit();
        }
        #endregion

        #region Grid Events
        private void fgObjects_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
            if (e.Control is TextBox) {
                // need to set MultiLine to true so the ENTER key can be captured by
                // KeyPress event
                EditTextBox = e.Control as TextBox;
                if (EditTextBox.Tag == null) {
                    EditTextBox.Tag = "set";
                    EditTextBox.Multiline = true;
                    EditTextBox.AcceptsReturn = true;
                    EditTextBox.AcceptsTab = true;
                    EditTextBox.Validating += EditTextBox_Validating;
                    EditTextBox.KeyDown += EditTextBox_KeyDown;
                    EditTextBox.TextChanged += EditTextBox_TextChanged;
                }
                else {
                    EditTextBox.Multiline = true;
                    EditTextBox.AcceptsReturn = true;
                    EditTextBox.AcceptsTab = true;
                }
            }
        }

        private void fgObjects_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (fgObjects.IsCurrentCellInEditMode) {
                if (e.Button == MouseButtons.Right) {
                    //cmCel.Show();
                }
                else {
                    if (e.ColumnIndex < 2 || e.RowIndex < 0 || !fgObjects[e.ColumnIndex, e.RowIndex].IsInEditMode) {
                        // same as pressing ENTER
                        EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Enter));
                    }
                }
                return;
            }
            if (e.RowIndex == -1) {
                return;
            }
            if (e.ColumnIndex == -1) {
                if (e.Button == MouseButtons.Right) {
                    // force selection
                    if (e.RowIndex != -1) {
                        fgObjects.CurrentCell = fgObjects[1, e.RowIndex];
                        fgObjects.Refresh();
                    }
                }
            }
            else {
                if (e.Button == MouseButtons.Right) {
                    // force selection
                    fgObjects.CurrentCell = fgObjects[e.ColumnIndex, e.RowIndex];
                    fgObjects.Refresh();
                }
            }
        }

        private void fgObjects_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.Value == null || e.RowIndex == fgObjects.NewRowIndex) {
                return;
            }
            // determine if tooltip is needed
            DataGridViewCell cell = fgObjects.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string text = e.Value.ToString();
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            // get size
            Size szText = TextRenderer.MeasureText(fgObjects.CreateGraphics(), text, e.CellStyle.Font, proposedSize, flags);
            if (szText.Width > cell.Size.Width - 8) {
                cell.ToolTipText = text;
            }
            else {
                cell.ToolTipText = "";
            }
        }

        private void fgObjects_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (fgObjects.IsCurrentCellInEditMode) {
                e.Cancel = true;
            }
        }

        private void fgObjects_CellValidated(object sender, DataGridViewCellEventArgs e) {
            // skip if not editing
            if (fgObjects.IsCurrentCellInEditMode) {
                // force blanks to default
                switch (e.ColumnIndex) {
                case 1:
                    // object text
                    if (string.IsNullOrEmpty((string)fgObjects[1, e.RowIndex].Value)) {
                        fgObjects[1, e.RowIndex].Value = "?";
                    }
                    break;
                case 2:
                    // object room number
                    if (string.IsNullOrEmpty((string)fgObjects[2, e.RowIndex].Value)) {
                        fgObjects[2, e.RowIndex].Value = (byte)0;
                    }
                    break;
                }
            }
        }

        private void fgObjects_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyData == (Keys.E | Keys.Alt) && !fgObjects.IsCurrentCellInEditMode) {
                if (fgObjects.CurrentRow.Index == fgObjects.NewRowIndex) {
                    EditItem.ItemName = "";
                    EditItem.Room = 0;
                }
                else {
                    EditItem.ItemName = (string)fgObjects[1, fgObjects.CurrentRow.Index].Value;
                    EditItem.Room = (byte)fgObjects[2, fgObjects.CurrentRow.Index].Value;
                }
                fgObjects.BeginEdit(true);
            }
        }

        private void fgObjects_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) {
                return;
            }
            DataGridViewCell cell = fgObjects.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.ToolTipText.Length > 0) {
                fgObjects.ShowCellToolTips = true;
            }
        }

        private void fgObjects_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            fgObjects.ShowCellToolTips = false;
        }

        private void fgObjects_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            // although not required, object i0 is traditionally set
            // to ' ? ', as are all 'unused' objects
            if (fgObjects.CurrentCell.RowIndex == 0 && WinAGISettings.WarnItem0.Value) {
                bool blnNoWarn = false;
                DialogResult rtn = MsgBoxEx.Show(MDIMain,
                    "Item 0 is usually set to '?'. Editing it is possible, but not normal. Are " +
                    "you sure you want to edit it?",
                    "Editing Item 0",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    "Don't show this warning again.", ref blnNoWarn,
                    WinAGIHelp, "htm\\agi\\object.htm#nullitem");
                if (blnNoWarn) {
                    WinAGISettings.WarnItem0.Value = false;
                    WinAGISettings.WarnItem0.WriteSetting(WinAGISettingsFile);
                }
                if (rtn == DialogResult.No) {
                    return;
                }
            }
            if (fgObjects.CurrentRow.Index == fgObjects.NewRowIndex) {
                EditItem.ItemName = "";
                EditItem.Room = 0;
                Adding = true;
            }
            else {
                EditItem.ItemName = (string)fgObjects[1, fgObjects.CurrentRow.Index].Value;
                EditItem.Room = (byte)fgObjects[2, fgObjects.CurrentRow.Index].Value;
                Adding = false;
            }
            fgObjects.BeginEdit(true);
        }

        private void fgObjects_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
            // ensure the new row is formatted correctly
            if (e.RowIndex == fgObjects.NewRowIndex && e.RowCount == 1) {
                // actual new row is above this row
                fgObjects[0, e.RowIndex - 1].Value = e.RowIndex - 1;
                fgObjects[1, e.RowIndex - 1].Value = "";
                fgObjects[2, e.RowIndex - 1].Value = (byte)0;
            }
        }

        private void txtMaxScreenObjs_KeyDown(object sender, KeyEventArgs e) {
            // ignore everything except numbers, backspace, delete, enter, tab, and escape
            switch (e.KeyCode) {
            case Keys.Enter:
                fgObjects.Focus();
                e.SuppressKeyPress = true;
                break;
            case Keys.Escape:
                txtMaxScreenObjs.Text = EditInvList.MaxScreenObjects.ToString();
                fgObjects.Focus();
                e.SuppressKeyPress = true;
                break;
            }
        }

        private void txtMaxScreenObjs_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore everything except numbers, backspace, delete, enter, tab, and escape
            switch (e.KeyChar) {
            case '\x08':
            case '\x09':
                break;
            case < '0':
            case > '9':
                e.Handled = true;
                break;
            }
        }

        private void txtMaxScreenObjs_TextChanged(object sender, EventArgs e) {
            if (txtMaxScreenObjs.Text.Length > 0 && !txtMaxScreenObjs.Text.IsNumeric()) {
                txtMaxScreenObjs.Text = EditInvList.MaxScreenObjects.ToString();
            }
        }

        private void txtMaxScreenObjs_Leave(object sender, EventArgs e) {
            txtMaxScreenObjs.BackColor = SystemColors.Control;
        }

        private void txtMaxScreenObjs_Enter(object sender, EventArgs e) {
            txtMaxScreenObjs.BackColor = SystemColors.Window;
        }

        private void txtMaxScreenObjs_Validating(object sender, CancelEventArgs e) {

            if (int.TryParse(txtMaxScreenObjs.Text, out int newMax)) {
                if (newMax != EditInvList.MaxScreenObjects) {
                    if (newMax == 0) {
                        if (MessageBox.Show(MDIMain,
                        "Setting MaxScreenObject to 0 means only ego can be animated.\n\nAre you sure you want to set this value?",
                        "Zero Max Screen Objects Count",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, 0, 0,
                        WinAGIHelp, "htm\\agi\\screenobjs.htm#maxscreenobjs") == DialogResult.No) {
                            e.Cancel = true;
                            return;
                        }
                    }
                    else if (newMax < 8) {
                        if (MessageBox.Show(MDIMain,
                        "Less than 8 screen objects is unusually low.\n\nAre you sure you want to set this value?",
                        "Low Max Screen Objects Count",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, 0, 0,
                        WinAGIHelp, "htm\\agi\\screenobjs.htm#maxscreenobjs") == DialogResult.No) {
                            e.Cancel = true;
                            return;
                        }
                    }
                    else if (newMax > 32) {
                        if (newMax > 255) {
                            newMax = 255;
                        }
                        if (MessageBox.Show(MDIMain,
                        "More than 32 screen objects is unusually high, and can affect graphics and memory performance.\n\nAre you sure you want to set this value?",
                        "High Max Screen Objects Count",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, 0, 0,
                        WinAGIHelp, "htm\\agi\\screenobjs.htm#maxscreenobjs") == DialogResult.No) {
                            e.Cancel = true;
                            return;
                        }
                    }
                    ModifyMax((byte)newMax);
                }
            }
            else {
                txtMaxScreenObjs.Text = EditInvList.MaxScreenObjects.ToString();
            }
        }

        private void EditTextBox_TextChanged(object sender, EventArgs e) {
            if (fgObjects.CurrentCell.ColumnIndex == 2) {
                if (EditTextBox.Text.Length > 0 && !EditTextBox.Text.IsNumeric()) {
                    EditTextBox.Text = (string)fgObjects.CurrentCell.Value;
                }
            }
        }

        private void EditTextBox_Validating(object sender, CancelEventArgs e) {
            // textbox Validating event ignores Cancel property, use CellValidate
        }

        private void EditTextBox_KeyDown(object sender, KeyEventArgs e) {
            // pressing enter should move to next COLUMN, not next ROW
            // (unless it's at end of row)

            if (e.KeyCode == Keys.Escape) {
                // normally just cancel, and restore previous value
                // but if quitting on value when adding a new line,
                // entire line needs to be deleted
                fgObjects.CancelEdit();
                fgObjects.EndEdit();
                if (Adding) {
                    Adding = false;
                    //fgObjects.Rows.RemoveAt(fgObjects.CurrentRow.Index);
                }
                return;
            }
            if (e.KeyValue == (int)Keys.Enter || e.KeyValue == (int)Keys.Tab) {
                e.Handled = true;
                e.SuppressKeyPress = true;
                EditTextBox.Text = EditTextBox.Text.Trim();
                ObjectsUndo NextUndo;
                // validate the input
                switch (fgObjects.CurrentCell.ColumnIndex) {
                case 1:
                    // item text
                    if (EditTextBox.Text.Length == 0) {
                        //if adding, a blank means cancel
                        if (Adding) {
                            fgObjects.CancelEdit();
                            fgObjects.EndEdit();
                            Adding = false;
                            fgObjects.Rows.RemoveAt(fgObjects.CurrentRow.Index);
                            return;
                        }
                        // otherwise a blank is same as '?'
                        EditTextBox.Text = "?";
                    }
                    if (EditItem.ItemName == EditTextBox.Text) {
                        // no change
                        fgObjects.CancelEdit();
                        fgObjects.EndEdit();
                        return;
                    }
                    // TODO: check for duplicates
                    if (EditTextBox.Text != "?") {
                        if (WinAGISettings.WarnDupObj.Value) {
                            for (int i = 0; i < EditInvList.Count; i++) {
                                if (i != fgObjects.CurrentCell.RowIndex) {
                                    if (EditInvList[i].ItemName == EditTextBox.Text) {
                                        bool blnNoWarn = false;
                                        DialogResult rtn = MsgBoxEx.Show(MDIMain,
                                            "'" + EditTextBox.Text + "' already exists in this object list.\nDo you want to keep this duplicate object?",
                                             "Duplicate Object",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question,
                                             "Don't show this warning again.", ref blnNoWarn,
                                             WinAGIHelp, "htm\\winagi\\Objects_Editor.htm#duplicates");
                                        WinAGISettings.WarnDupObj.Value = !blnNoWarn;
                                        if (!WinAGISettings.WarnDupObj.Value) {
                                            WinAGISettings.WarnDupObj.WriteSetting(WinAGISettingsFile);
                                        }
                                        if (rtn == DialogResult.No) {
                                            // remain in edit mode
                                            return;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    EditItem.ItemName = EditTextBox.Text;
                    if (!Adding) {
                        NextUndo = new() {
                            UDAction = ObjectsUndo.ActionType.ModifyItem,
                            UDObjectNo = fgObjects.CurrentRow.Index,
                            UDObjectText = fgObjects.CurrentCell.Value.ToString()
                        };
                        AddUndo(NextUndo);
                    }
                    break;
                case 2:
                    // room number
                    if (EditTextBox.Text.Length == 0) {
                        // blank is same as '0'
                        EditTextBox.Text = "0";
                    }
                    if (!Adding && EditItem.Room.ToString() == EditTextBox.Text) {
                        // no change
                        fgObjects.EndEdit();
                        return;
                    }
                    EditItem.Room = byte.Parse(EditTextBox.Text);
                    if (Adding) {
                        NextUndo = new() {
                            UDAction = AddItem,
                            UDObjectNo = fgObjects.CurrentRow.Index,
                        };
                        AddUndo(NextUndo);
                    }
                    else {
                        NextUndo = new() {
                            UDAction = ModifyRoom,
                            UDObjectNo = fgObjects.CurrentRow.Index,
                            UDObjectRoom = byte.Parse(fgObjects.CurrentCell.Value.ToString())
                        };
                        AddUndo(NextUndo);
                    }
                    break;
                }
                fgObjects.EndEdit();
                if (fgObjects.CurrentCell.ColumnIndex == 1) {
                    fgObjects.CurrentCell = fgObjects[2, fgObjects.CurrentCell.RowIndex];
                    if (Adding) {
                        fgObjects.BeginEdit(true);
                    }
                }
                else {
                    fgObjects.CurrentCell = fgObjects[1, fgObjects.CurrentCell.RowIndex + 1];
                    if (Adding) {
                        EditInvList.Add(EditItem);
                        Adding = false;
                    }
                }
                MarkAsChanged();
                return;
            }
            // any char is fine for item text; room number is numeric
            if (fgObjects.CurrentCell.ColumnIndex == 2) {
                // only numbers, backspace, delete
                if (e.KeyValue < 33 && e.KeyValue != 8) {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyValue < 48 || e.KeyValue > 57) {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }
        }

        #endregion

        #region temp code
        /*

  
  Private EditRow As Long, EditCol As ColType
  
Public Sub MenuClickCustom2()
  
  'convert an Amiga format OBJECT file to DOS format
  
  On Error GoTo ErrHandler
  
  'verify it's an Amiga file
  If Not InventoryObjects.AmigaOBJ Then
    'hmm, not sure how we got here
    frmMDIMain.mnuRCustom2.Visible = False
    MsgBox "This is not an AMIGA Object file, so no conversion is necessary.", vbInformation + vbOKOnly, "Convert AMIGA Object File"
    Exit Sub
  End If
  
  'get permission
  If MsgBox("Your current OBJECT file will be saved as 'OBJECT.amg'. Continue with the conversion?", vbQuestion + vbOKCancel, "Convert AMIGA Object File") = vbOK Then

    'if the file is not saved, ask if OK to save it first
    If Me.IsDirty Then
      If MsgBox("The OBJECT file needs to be saved before converting. OK to save and convert?", vbQuestion + vbOKCancel, "Save OBJECT File") = vbCancel Then
        Exit Sub
      End If
    End If
    'file is saved; change the AmigaOBJ property to affect the conversion
    InventoryObjects.AmigaOBJ = False
    'hide the menu
    frmMDIMain.mnuRCustom2.Visible = False
  End If
End Sub

Public Sub MenuClickHelp()
  
  On Error GoTo ErrHandler
  
  'help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Objects_Editor.htm"
End Sub

Public Sub BeginFind()

  'each form has slightly different search parameters
  'and procedure; so each form will get what it needs
  'from the form, and update the global search parameters
  'as needed
  '
  'that's why each search form cheks for changes, and
  'sets the global values, instead of doing it once inside
  'the FindForm code
  
  Select Case FindForm.FormAction
  Case faFind
    'now find the object
    FindInObjects GFindText, GFindDir, GMatchWord, GMatchCase
  Case faReplace
    FindInObjects GFindText, GFindDir, GMatchWord, GMatchCase, True, GReplaceText
  Case faReplaceAll
    ReplaceAll GFindText, GMatchWord, GMatchCase, GReplaceText
  Case faCancel
    'don't do anything
  End Select
End Sub

Private Sub FindInObjects(ByVal FindText As String, ByVal FindDir As FindDirection, ByVal MatchWord As Boolean, ByVal MatchCase As Boolean, Optional ByVal Replacing As Boolean = False, Optional ByVal ReplaceText As String = vbNullString)

  Dim SearchPos As Long, FoundPos As Long
  Dim rtn As VbMsgBoxResult
  Dim vbcComp As VbCompareMethod
  
  On Error GoTo ErrHandler
  
  'if replacing and new text is the same
  If Replacing And (StrComp(FindText, ReplaceText, vbTextCompare) = 0) Then
    'exit
    Exit Sub
  End If
    
  If EditInvList.Count = 0 Then
    MsgBox "No inventory objects in list.", vbOKOnly + vbInformation, "Find in Object List"
    Exit Sub
  End If
  
  'show wait cursor
  WaitCursor
  
  'set comparison method for string search,
  vbcComp = CLng(MatchCase) + 1 ' CLng(True) + 1 = 0 = vbBinaryCompare; Clng(False) + 1 = 1 = vbTextCompare
  
  'if replacing and searching up   searchpos = current pos+1
  'if replacing and searching down searchpos = current pos
  'if not repl  and searching up   searchpos = current pos
  'if not repl  and searching down searchpos = current pos+1
  
  'set searchpos to current item index (current item index is row-1)
  SearchPos = fgObjects.Row - 1
  
  'adjust to next object per replace/direction selections
  If (Replacing And FindDir = fdUp) Or (Not Replacing And FindDir <> fdUp) Then
    'add one to skip current object
    SearchPos = SearchPos + 1
    If SearchPos > EditInvList.Count - 1 Then
      SearchPos = 0
    End If
  Else
    'if already AT beginning of search, the replace function will mistakenly
    'think the find operation is complete and stop
    If Replacing And (SearchPos = ObjStartPos) Then
      'reset search
      FindForm.ResetSearch
    End If
  End If
  
  
  'main search loop
  Do
    'if direction is up
    If FindDir = fdUp Then
      'iterate backwards until word found or foundpos=-1
      FoundPos = SearchPos - 1
      Do Until FoundPos = -1
        If MatchWord Then
          If StrComp(EditInvList(FoundPos).ItemName, FindText, vbcComp) = 0 Then
            'found
            Exit Do
          End If
        Else
          If InStr(1, EditInvList(FoundPos).ItemName, FindText, vbcComp) <> 0 Then
            'found
            Exit Do
          End If
        End If
        FoundPos = FoundPos - 1
      Loop
      'reset searchpos
      SearchPos = EditInvList.Count - 1
    Else
      'iterate forward until word found or foundpos=objcount
      FoundPos = SearchPos
      Do
        If MatchWord Then
          If StrComp(EditInvList(FoundPos).ItemName, FindText, vbcComp) = 0 Then
            'found
            Exit Do
          End If
        Else
          If InStr(1, EditInvList(FoundPos).ItemName, FindText, vbcComp) <> 0 Then
            'found
            Exit Do
          End If
        End If
        FoundPos = FoundPos + 1
      Loop Until FoundPos = EditInvList.Count
      'reset searchpos
      SearchPos = 0
    End If
    
    'if found
    If FoundPos >= 0 And FoundPos < EditInvList.Count Then
      'if back at start
      If FoundPos = ObjStartPos Then
        FoundPos = -1
      End If
      Exit Do
    End If
    
    'if not found, action depends on search mode
    Select Case FindDir
    Case fdUp
      'if not reset yet
      If Not RestartSearch Then
        'if recursing
        If blnRecurse Then
          'just say no
          rtn = vbNo
        Else
          rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Object List")
        End If
        If rtn = vbNo Then
          'reset search
          FindForm.ResetSearch
          Screen.MousePointer = vbDefault
          Exit Sub
        End If
      Else
        'entire scope already searched; exit
        Exit Do
      End If
      
    Case fdDown
      'if not reset yet
      If Not RestartSearch Then
        'if recursing
        If blnRecurse Then
          'just say no
          rtn = vbNo
        Else
          rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Object List")
        End If
        If rtn = vbNo Then
          'reset search
          FindForm.ResetSearch
          Screen.MousePointer = vbDefault
          Exit Sub
        End If
      Else
        'entire scope already searched; exit
        Exit Do
      End If
      
    Case fdAll
      If RestartSearch Then
        Exit Do
      End If
      
    End Select
    
    'reset search so when we get back to start, search will end
    RestartSearch = True
  
  'loop is exited by finding the searchtext or reaching end of search area
  Loop
        
  'if search string found
  If FoundPos >= 0 And FoundPos < EditInvList.Count Then
    'if this is first occurrence
    If Not FirstFind Then
      'save this position
      FirstFind = True
      ObjStartPos = FoundPos
    End If
    
    'highlight object
    fgObjects.Row = FoundPos + 1
    If Not fgObjects.RowIsVisible(FoundPos + 1) Then
      fgObjects.TopRow = FoundPos - 2
    End If
    
    'if replacing
    If Replacing Then
      If MatchWord Then
        ModifyItem FoundPos, ReplaceText
      Else
        ModifyItem FoundPos, Replace(EditInvList(FoundPos).ItemName, FindText, ReplaceText, 1, -1, vbcComp)
      End If
      'change undoobject
      UndoCol(UndoCol.Count).UDAction = Replace
      frmMDIMain.mnuEUndo.Caption = "&Undo Replace" & vbTab & "Ctrl+Z"
      
      'recurs the find method to get next occurrence
      blnRecurse = True
      FindInObjects FindText, FindDir, MatchWord, MatchCase, False
      blnRecurse = False
    End If
    
  Else
    'if not recursing, show a msg
    If Not blnRecurse Then
      'if something was previously found
      If FirstFind Then
        'search complete; no new instances found
        MsgBox "The specified region has been searched.", vbInformation, "Find in Object List"
      Else
        'show not found msg
        MsgBox "Search text not found.", vbInformation, "Find in Object List"
      End If
    End If
    
    'reset search flags
    FindForm.ResetSearch
  End If
  
  fgObjects.SetFocus
  
  'need to always make sure right form has focus; if finding a word
  'causes the group list to change, VB puts the wordeditor form in focus
  ' but we want focus to match the starting form
  If SearchStartDlg Then
    FindForm.SetFocus
  Else
    Me.SetFocus
  End If
  
  'reset cursor
  Screen.MousePointer = vbDefault
End Sub

Public Sub MenuClickReplace()

  'use menuclickfind in replace mode
  MenuClickFind ffReplaceObject
End Sub

Public Sub MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindObject)

  On Error GoTo ErrHandler
  
  With FindForm
    'set form defaults
    If Not .Visible Then
      If Len(GFindText) > 0 Then
        'if it has quotes, remove them
        If Asc(GFindText) = 34 Then
          GFindText = Right$(GFindText, Len(GFindText) - 1)
        End If
        If Right$(GFindText, 1) = QUOTECHAR Then
          GFindText = Left$(GFindText, Len(GFindText) - 1)
        End If
      End If
    End If
    
    'set find dialog to object mode
    .SetForm ffValue, False
    
    'show the form
    .Show , frmMDIMain
  
    'always highlight search text
    .rtfFindText.Selection.Range.StartPos = 0
    .rtfFindText.Selection.Range.EndPos = Len(.rtfFindText.Text)
    .rtfFindText.SetFocus
    
    'ensure this form is the search form
    Set SearchForm = Me
  End With
End Sub

Public Sub MenuClickFindAgain()
  
  On Error GoTo ErrHandler
  
  'if nothing in find form textbox
  If LenB(GFindText) <> 0 Then
    FindInObjects GFindText, GFindDir, GMatchWord, GMatchCase
  Else
    'show find form
    MenuClickFind
  End If
End Sub

Private Sub ReplaceAll(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal MatchCase As Boolean, ByVal ReplaceText As String)

  'replace all occurrences of FindText with ReplaceText
  
  Dim i As Long, vbcComp As VbCompareMethod
  Dim lngCount As Long
  Dim NextUndo As ObjectsUndo
  
  On Error GoTo ErrHandler
  
  'if replacing and new text is the same
  If StrComp(FindText, ReplaceText, vbTextCompare) = 0 Then
    'exit
    Exit Sub
  End If
  
  'if no objects in list
  If EditInvList.Count = 0 Then
    MsgBox "Object list is empty.", vbOKOnly + vbInformation, "Replace All"
    Exit Sub
  End If
  
  'show wait cursor
  WaitCursor
  
  'create new undo object
  Set NextUndo = New ObjectsUndo
  NextUndo.UDAction = ReplaceAll
  
  'set comparison method for string search,
  vbcComp = CLng(MatchCase) + 1 ' CLng(True) + 1 = 0 = vbBinaryCompare; Clng(False) + 1 = 1 = vbTextCompare
  
  If MatchWord Then
    'step through all objects
    For i = 0 To EditInvList.Count - 1
      If StrComp(EditInvList(i).ItemName, FindText, vbcComp) = 0 Then
        'add  word being replaced to undo
        If lngCount = 0 Then
          NextUndo.UDObjectText = CStr(i) & "|" & EditInvList(i).ItemName
        Else
          NextUndo.UDObjectText = NextUndo.UDObjectText & "|" & CStr(i) & "|" & EditInvList(i).ItemName
        End If
        'replace the word
        EditInvList(i).ItemName = ReplaceText
        'update grid
        fgObjects.TextMatrix(i + 1, ctDesc) = ReplaceText
        'increment counter
        lngCount = lngCount + 1
      End If
    Next i
  Else
    For i = 0 To EditInvList.Count - 1
      If InStr(1, EditInvList(i).ItemName, FindText, vbcComp) <> 0 Then
        'add  word being replaced to undo
        If lngCount = 0 Then
          NextUndo.UDObjectText = CStr(i) & "|" & EditInvList(i).ItemName
        Else
          NextUndo.UDObjectText = NextUndo.UDObjectText & "|" & CStr(i) & "|" & EditInvList(i).ItemName
        End If
        'replace the word
        EditInvList(i).ItemName = Replace(EditInvList(i).ItemName, FindText, ReplaceText, 1, -1, vbcComp)
        'update grid
        fgObjects.TextMatrix(i + 1, ctDesc) = Replace(EditInvList(i).ItemName, FindText, ReplaceText)
        'increment counter
        lngCount = lngCount + 1
      End If
    Next i
  End If
  
  'if nothing found,
  If lngCount = 0 Then
    MsgBox "Search text not found.", vbInformation, "Replace All"
  Else
    'add undo
    AddUndo NextUndo
    
    'show how many replacements made
    MsgBox "The specified region has been searched. " & CStr(lngCount) & " replacements were made.", vbInformation, "Replace All"
  End If
  
  Screen.MousePointer = vbDefault
End Sub

Public Sub FindInLogic()
  
  'call findinlogic with current invobj
  
  Dim strObj As String
  
  strObj = fgObjects.TextMatrix(fgObjects.Row, ctDesc)
  
  If strObj <> "?" Then
    'reset logic search
    FindForm.ResetSearch
    
    'set search parameters
    GFindText = QUOTECHAR & strObj & QUOTECHAR
    GFindDir = fdAll
    GMatchWord = True
    GMatchCase = False
    GLogFindLoc = flAll
    GFindSynonym = False
    SearchType = rtObjects
    
    With FindForm
      'if the findform is visible,
      If .Visible Then
      'set it to match desired search parameters
        'set find dialog to find textinlogic mode
        .SetForm ffFindLogic, True
      End If
    End With
    
    'ensure this form is the search form
    Set SearchForm = Me
    
    'now search all logics
    FindInLogic QUOTECHAR & Replace(strObj, QUOTECHAR, "\""") & QUOTECHAR, fdAll, True, False, flAll, False, vbNullString
  End If
End Sub
        */

        #endregion

        private void AddUndo(ObjectsUndo NextUndo) {
            UndoCol.Push(NextUndo);
            MarkAsChanged();
            FindingForm.ResetSearch();
        }

        private void SetEncryption(bool encrypt, bool DontUndo = false) {
            if (encrypt == EditInvList.Encrypted) {
                return;
            }
            if (InGame && !DontUndo) {
                bool blnCorrect = !(EditGame.InterpreterVersion == "2.089" || EditGame.InterpreterVersion == "2.272");
                bool blnNoWarn = false;
                if (blnCorrect != encrypt && WinAGISettings.WarnEncrypt.Value) {
                    DialogResult rtn = MsgBoxEx.Show(MDIMain,
                        "The target Interpreter Version for this game needs the OBJECT file to be " +
                        (blnCorrect ? "ENCRYPTED" : "UNENCRYPTED") + ". Are you sure you want to change it?",
                        "Change OBJECT Encryption",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        "Don't show this warning again.", ref blnNoWarn,
                        WinAGIHelp, "htm\\agi\\object.htm#format");
                    if (blnNoWarn) {
                        WinAGISettings.WarnEncrypt.Value = false;
                        WinAGISettings.WarnEncrypt.WriteSetting(WinAGISettingsFile);
                    }
                    if (rtn == DialogResult.No) {
                        return;
                    }
                }
            }
            if (!DontUndo) {
                ObjectsUndo NextUndo = new();
                NextUndo.UDAction = TglEncrypt;
                NextUndo.UDObjectRoom = (byte)(EditInvList.Encrypted ? 1 : 0);
                AddUndo(NextUndo);
            }
            EditInvList.Encrypted = encrypt;
            MarkAsChanged();
        }

        public bool LoadOBJECT(InventoryList objectobj) {

            InGame = objectobj.InGame;
            IsChanged = objectobj.IsChanged;
            try {
                if (InGame) {
                    objectobj.Load();
                }
            }
            catch {
                return false;
            }
            if (objectobj.ErrLevel < 0) {
                return false; ;
            }
            EditInvList = objectobj.Clone();
            EditInvListFilename = objectobj.ResFile;
            for (int i = 0; i < EditInvList.Count; i++) {
                int currentrow = fgObjects.Rows.Add();
                fgObjects[0, currentrow].Value = i;
                fgObjects[1, currentrow].Value = EditInvList[i].ItemName;
                fgObjects[2, currentrow].Value = EditInvList[i].Room;
            }
            txtMaxScreenObjs.Text = EditInvList.MaxScreenObjects.ToString();

            // statusbar has not been merged yet
            //statusStrip1.Items["spCount"].Text = "Object Count: " + EditInvList.Count;
            //statusStrip1.Items["spEncrypt"].Text = EditInvList.Encrypted ? "Encrypted" : "Not Encrypted";

            Text = "Objects Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                if (EditInvListFilename.Length > 0) {
                    Text += Common.Base.CompactPath(EditInvListFilename, 75);
                }
                else {
                    ObjCount++;
                    Text += "NewObjects" + ObjCount.ToString();
                }
            }
            if (IsChanged) {
                Text = sDM + Text;
            }

            mnuRSave.Enabled = !IsChanged;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = !IsChanged;
            return true;
        }

        public void ImportObjects(string importfile) {
            InventoryList tmpList;

            MDIMain.UseWaitCursor = true;
            try {
                tmpList = new(importfile);
            }
            catch (Exception e) {
                ErrMsgBox(e, "An error occurred during import:", "", "Import Object File Error");
                MDIMain.UseWaitCursor = false;
                return;
            }
            // replace current objectlist
            EditInvList = tmpList;
            EditInvListFilename = importfile;
            fgObjects.Rows.Clear();
            for (int i = 0; i < EditInvList.Count; i++) {
                int currentrow = fgObjects.Rows.Add();
                fgObjects[0, currentrow].Value = i;
                fgObjects[1, currentrow].Value = EditInvList[i].ItemName;
                fgObjects[2, currentrow].Value = EditInvList[i].Room;
            }
            txtMaxScreenObjs.Text = EditInvList.MaxScreenObjects.ToString();
            MarkAsChanged();
            MDIMain.UseWaitCursor = false;
        }

        public void SaveObjects() {
            // TODO: AutoUpdate feature still needs significant work; disable it for now
            //bool blnDontAsk = false;
            //DialogResult rtn = DialogResult.No;
            //if (InGame && WinAGISettings.AutoUpdateObjects != 1) {
            //    if (WinAGISettings.AutoUpdateObjects == 0) {
            //         = MsgBoxEx.Show(MDIMain,
            //            "Do you want to update all game logics with the changes made in the object list?",
            //            "Update Logics?",
            //            MessageBoxButtons.YesNo,
            //            MessageBoxIcon.Question,
            //            "Always take this action when saving the object list.", ref blnDontAsk);
            //        if (blnDontAsk) {
            //            if (rtn == DialogResult.Yes) {
            //                WinAGISettings.AutoUpdateObjects = 2;
            //            }
            //            else {
            //                WinAGISettings.AutoUpdateObjects = 1;
            //            }
            //        }
            //    }
            //    else {
            //        rtn = DialogResult.Yes;
            //    }
            //    if (rtn == DialogResult.Yes) {
            //        // test cmds that use IObj:
            //        //   has, obj.in.room
            //        //
            //        // action cmds that use IObj:
            //        //   get, drop, put
            //        FindForm.Visible = false;
            //        MDIMain.UseWaitCursor = true;
            //        ProgressWin.Text = "Updating Objects in Logics";
            //        ProgressWin.lblProgress.Text = "Searching...";
            //        ProgressWin.pgbStatus.Maximum = EditInvList.Count - 1;
            //        ProgressWin.pgbStatus.Value = 0;
            //        ProgressWin.Show(MDIMain);
            //        ProgressWin.Refresh();
            //        for (int i = 1; i < EditGame.InvObjects.Count; i++) {
            //            if (i >= EditInvList.Count) {
            //                // mark all objects in logics as deleted
            //                ReplaceAll("\"" + EditGame.InvObjects[i].ItemName + "\"", "i" + i.ToString(), fdAll, true, true, flAll, AGIResType.Objects);
            //            }
            //            else {
            //                if (EditInvList[i].ItemName == "?") {
            //                    // mark all objects in logics as deleted
            //                    ReplaceAll("\"" + EditGame.InvObjects[i].ItemName + "\"", "i" + i.ToString(), fdAll, true, true, flAll, AGIResType.Objects);
            //                }
            //                else if (EditInvList[i].ItemName != EditGame.InvObjects[i].ItemName) {
            //                    // change to new object item name
            //                    ReplaceAll("\"" + EditGame.InvObjects[i].ItemName + "\"", "\"" + EditInvList[i].ItemName + "\"", fdAll, true, true, flAll, AGIResType.Objects);
            //                }
            //            }
            //            ProgressWin.pgbStatus.Value = i;
            //            ProgressWin.Refresh();
            //        }
            //        ProgressWin.Close();
            //        MDIMain.UseWaitCursor = false;
            //    }
            //}
            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool loaded = EditGame.InvObjects.Loaded;
                if (!loaded) {
                    EditGame.InvObjects.Load();
                }
                EditGame.InvObjects.CloneFrom(EditInvList);
                try {
                    EditGame.InvObjects.Save();
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "Error during OBJECT compilation: ",
                        "Existing OBJECT has not been modified.",
                        "OBJECT Compile Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                MakeAllChanged();
                RefreshTree(AGIResType.Objects, 0);
                MDIMain.ClearWarnings(AGIResType.Objects, 0);
                if (!loaded) {
                    EditGame.InvObjects.Unload();
                }
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditInvList.ResFile.Length == 0) {
                    ExportObjects();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    try {
                        EditInvList.Save();
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex, "An error occurred while trying to save object list: ",
                            "Existing object list has not been modified.",
                            "Object List Save Error");
                        MDIMain.UseWaitCursor = false;
                        return;
                    }
                    MDIMain.UseWaitCursor = false;
                }
            }
            MarkAsSaved();
        }

        public void ExportObjects() {
            bool retval = Base.ExportObjects(EditInvList, InGame);
            if (!InGame && retval) {
                EditInvListFilename = EditInvList.ResFile;
                MarkAsSaved();
            };
        }

        public void EditProperties() {
            string strDesc = EditInvList.Description;
            string id = "";
            if (GetNewResID(AGIResType.Objects, -1, ref id, ref strDesc, InGame, 2)) {
                EditInvList.Description = strDesc;
                MDIMain.RefreshPropertyGrid(AGIResType.Objects, 0);
            }
        }

        private bool AskClose() {
            if (EditInvList.ErrLevel < 0) {
                // if exiting due to error on form load
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this OBJECT file?",
                    "Save OBJECT",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveObjects();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "OBJECT file not saved. Continue closing anyway?",
                            "Save OBJECT",
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
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = "Objects Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                Text += Common.Base.CompactPath(EditInvListFilename, 75);
            }
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
        }

        internal void InitFonts() {
            Font formfont = new(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            Label1.Font = formfont;
            txtMaxScreenObjs.Font = formfont;
            Label1.Left = txtMaxScreenObjs.Left - Label1.Width - 1;
            fgObjects.Top = txtMaxScreenObjs.Height;
            fgObjects.Font = formfont;
        }

        private void ModifyMax(byte NewMax, bool DontUndo = false) {
            if (EditInvList.MaxScreenObjects == NewMax) {
                return;
            }
            if (!DontUndo) {
                ObjectsUndo NextUndo = new() {
                    UDAction = ChangeMaxObj,
                    UDObjectRoom = EditInvList.MaxScreenObjects
                };
                AddUndo(NextUndo);
            }
            EditInvList.MaxScreenObjects = NewMax;
            txtMaxScreenObjs.Text = EditInvList.MaxScreenObjects.ToString();
            MarkAsChanged();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // Enter key is usually captured by the grid, but we want it to go to the textbox
            if (keyData == Keys.Enter) {
                if (EditTextBox.Focused) {
                    //if (fgObjects.IsCurrentCellInEditMode) {
                    EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Enter));
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    public class ObjectsUndo {
        public ActionType UDAction;
        public byte UDObjectRoom; // also used for Max objects & encryption
        public int UDObjectNo;
        public string UDObjectText = "";

        public enum ActionType {
            AddItem,      // store object number that was added
            DeleteItem,   // store object number, text, and room that was deleted
            ModifyItem,   // store old object number, text
            ModifyRoom,   // store old object number, room
            ChangeDesc,     // store old description
            ChangeMaxObj,   // store old maxobjects
            TglEncrypt,     // store old encryption Value
            Clear,          // store old Objects object
            Replace,    // store old object number, text
            ReplaceAll, // store all old numbers and text
        }

        public ObjectsUndo() {

        }
    }
}
