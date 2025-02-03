using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
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
        private bool FirstFind = false;
        private Font formFont;
        private Font dupFont;

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

        private void frmObjectEdit_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible) {
                if (FindingForm.rtfReplace.Visible) {
                    FindingForm.SetForm(FindFormFunction.ReplaceObject, InGame);
                }
                else {
                    FindingForm.SetForm(FindFormFunction.FindObject, InGame);
                }
            }
            this.statusStrip1.Items["spCount"].Text = "Object Count: " + EditInvList.Count;
            this.statusStrip1.Items["spEncrypt"].Text = EditInvList.Encrypted ? "Encrypted" : "Not Encrypted";
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

        private void mnuRAmigaOBJ_Click(object sender, EventArgs e) {
            // convert an Amiga format OBJECT file to DOS format
            Debug.Assert(!EditInvList.AmigaOBJ);
            if (MessageBox.Show(MDIMain,
                "Your current OBJECT file will be saved as 'OBJECT.amg'. Continue with the conversion?",
                "Convert AMIGA Object File",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) == DialogResult.OK) {
                if (IsChanged) {
                    if (MessageBox.Show(MDIMain,
                        "The OBJECT file needs to be saved before converting. OK to save and convert?",
                        "Save OBJECT File",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) == DialogResult.OK) {
                        return;
                    }
                }
                EditInvList.AmigaOBJ = false;
            }
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
            mnuEdit.DropDownItems.AddRange([mnuEUndo, toolStripSeparator1, mnuEDelete, mnuEClear, mnuEInsert, toolStripSeparator2, mnuEFind, mnuEFindAgain, mnuEReplace, toolStripSeparator3, mnuEditItem, mnuEFindInLogic]);
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            cmGrid.Items.AddRange([mnuEUndo, toolStripSeparator1, mnuEDelete, mnuEClear, mnuEInsert, toolStripSeparator2, mnuEFind, mnuEFindAgain, mnuEReplace, toolStripSeparator3, mnuEditItem, mnuEFindInLogic]);
            ResetEditMenu();
        }

        private void cmGrid_Opening(object sender, CancelEventArgs e) {
            if (fgObjects.IsCurrentCellInEditMode) {
                e.Cancel = true;
                return;
            }
            SetEditMenu();
        }

        private void cmGrid_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
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
            case ObjectsUndo.ActionType.ModifyItem:
            case Replace:
                fgObjects[1, NextUndo.UDObjectNo].Value = NextUndo.UDObjectText;
                EditInvList[NextUndo.UDObjectNo].ItemName = NextUndo.UDObjectText;
                fgObjects[1, NextUndo.UDObjectNo].Selected = true;
                break;
            case ModifyRoom:
                fgObjects[2, NextUndo.UDObjectNo].Value = NextUndo.UDObjectRoom;
                EditInvList[NextUndo.UDObjectNo].Room = NextUndo.UDObjectRoom;
                fgObjects[2, NextUndo.UDObjectNo].Selected = true;
                break;
            case ChangeMaxObj:
                // old Max is stored in room variable
                ModifyMax(NextUndo.UDObjectRoom, true);
                return;
            case TglEncrypt:
                SetEncryption(NextUndo.UDObjectRoom == 1, true);
                break;
            case Clear:
                // restore Max objects
                ModifyMax(NextUndo.UDObjectRoom, true);
                // restore encryption
                SetEncryption(NextUndo.UDObjectNo == 1, true);
                // split out items
                string[] strObjs = NextUndo.UDObjectText.Split('\r');
                string[] item = strObjs[0].Split('|');
                // item0 is always present
                fgObjects[0, 0].Value = 0;
                fgObjects[1, 0].Value = item[0];
                fgObjects[2, 0].Value = byte.Parse(item[1]);
                EditInvList[0].ItemName = item[0];
                EditInvList[0].Room = byte.Parse(item[1]);
                // add remaining items
                for (int i = 1; i < strObjs.Length; i++) {
                    item = strObjs[i].Split('|');
                    int newrow = fgObjects.Rows.Add();
                    fgObjects[0, newrow].Value = EditInvList.Count;
                    fgObjects[1, newrow].Value = item[0];
                    fgObjects[2, newrow].Value = byte.Parse(item[1]);
                    EditInvList.Add(item[0], byte.Parse(item[1]));
                }
                break;
            case ObjectsUndo.ActionType.ReplaceAll:
                // udstring has previous items in pipe-delimited string
                strObjs = NextUndo.UDObjectText.Split('\r');
                // object numbers and old values are in pairs
                for (int i = 0; i < strObjs.Length; i++) {
                    item = strObjs[i].Split('|');
                    // first element is obj number, second is old obj desc
                    EditInvList[byte.Parse(item[0])].ItemName = item[1];
                    fgObjects[1, int.Parse(item[0])].Value = item[1];
                }
                fgObjects[1, 0].Selected = true;
                break;
            }
            //ensure selected row is visible
            if (!fgObjects.CurrentRow.Displayed) {
                if (fgObjects.CurrentRow.Index < 2) {
                    fgObjects.FirstDisplayedScrollingRowIndex = 0;
                }
                else {
                    fgObjects.FirstDisplayedScrollingRowIndex = fgObjects.CurrentRow.Index - 2;
                }
            }
            MarkAsChanged();
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
            for (int i = 1; i < EditInvList.Count; i++) {
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
            StartSearch(FindFormFunction.FindObject);
        }

        private void mnuEFindAgain_Click(object sender, EventArgs e) {
            if (GFindText.Length == 0) {
                StartSearch(FindFormFunction.FindObject);
            }
            else {
                FindInObjects(GFindText, GFindDir, GMatchWord, GMatchCase);
            }
        }

        private void mnuEReplace_Click(object sender, EventArgs e) {
            StartSearch(FindFormFunction.ReplaceObject);
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
            string strObj = (string)fgObjects[1, fgObjects.CurrentCell.RowIndex].Value;
            if (strObj == "?") {
                return;
            }
            FindingForm.ResetSearch();
            FirstFind = false;
            GFindText = '"' + strObj.Replace("\"", "\\\"") + '"';
            GFindDir = FindDirection.All;
            GMatchWord = true;
            GMatchCase = true;
            GLogFindLoc = FindLocation.All;
            GFindSynonym = false;
            SearchType = AGIResType.Objects;
            if (FindingForm.Visible) {
                // set it to match desired search parameters
                FindingForm.SetForm(FindFormFunction.FindLogic, true);
            }
            FindInLogic(this, GFindText, FindDirection.All, true, false, FindLocation.All, false, "");
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
                if (EditTextBox.ContextMenuStrip != cmCel) {
                    EditTextBox.ContextMenuStrip = cmCel;
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
                if (e.ColumnIndex < 1 || e.RowIndex < 0 || !fgObjects[e.ColumnIndex, e.RowIndex].IsInEditMode) {
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
            if (e.ColumnIndex == 1) {
                if (EditInvList[e.RowIndex].Unique) {
                    e.CellStyle.Font = formFont;
                }
                else {
                    e.CellStyle.ForeColor = Color.OrangeRed;
                    e.CellStyle.Font = dupFont;
                }
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
            if (e.RowIndex > 0 && e.RowIndex == fgObjects.NewRowIndex && e.RowCount == 1) {
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
        #endregion

        #region Control Events
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
                    EditInvList[fgObjects.CurrentRow.Index].ItemName = EditTextBox.Text;
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
                    EditInvList[fgObjects.CurrentRow.Index].Room = EditItem.Room;
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
                    if (Adding || e.KeyCode == Keys.Tab) {
                        fgObjects.CurrentCell = fgObjects[2, fgObjects.CurrentCell.RowIndex];
                        if (Adding) {
                            fgObjects.BeginEdit(true);
                        }
                    }
                    else {
                        fgObjects.CurrentCell = fgObjects[1, fgObjects.CurrentCell.RowIndex + 1];
                    }
                }
                else {
                    fgObjects.CurrentCell = fgObjects[1, fgObjects.CurrentCell.RowIndex + 1];
                    if (Adding) {
                        EditInvList.Add(EditItem);
                        Adding = false;
                    }
                }
                fgObjects.Refresh();
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

        #region Methods
        private void StartSearch(FindFormFunction formfunction) {
            string searchtext;
            if (fgObjects.CurrentCell == null || fgObjects.CurrentCell.RowIndex < 0 ||
                fgObjects.CurrentCell.RowIndex == fgObjects.NewRowIndex) {
                searchtext = "";
            }
            else {
                searchtext = (string)fgObjects[1, fgObjects.CurrentCell.RowIndex].Value;
            }
            // TODO: should I remove quotes, if the current search string includes them?

            //default to matchcase, and wholeword
            GMatchCase = true;
            GMatchWord = true;
            FindingForm.SetForm(formfunction, InGame);
            FindingForm.cmbFind.Text = searchtext;
            if (!FindingForm.Visible) {
                FindingForm.Visible = true;
            }
            FindingForm.Select();
            FindingForm.cmbFind.Select();
        }

        public void FindInObjects(string FindText, FindDirection FindDir, bool MatchWord, bool MatchCase, bool Replacing = false, string ReplaceText = "") {
            bool blnRecurse = false;
            StringComparison vbcComp = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (Replacing && FindText.Equals(ReplaceText, vbcComp)) {
                return;
            }
            if (EditInvList.Count == 0) {
                MessageBox.Show(MDIMain,
                    "No inventory objects in list.",
                    "Find in Object List",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            MDIMain.UseWaitCursor = true;
            // if replacing and searching up   searchrow = currentrow +1
            // if replacing and searching down searchrow = currentrow
            // if not repl  and searching up   searchrow = currentrow
            // if not repl  and searching down searchrow = currentrow +1
            int searchrow = fgObjects.CurrentCell.RowIndex;
            int foundrow = -1;
            // adjust to next row per replace/direction selections
            if ((Replacing && FindDir == FindDirection.Up) || (!Replacing && FindDir != FindDirection.Up)) {
                searchrow += 1;
                if (searchrow >= EditInvList.Count) {
                    searchrow = 0;
                }
            }
            else {
                // if already at beginning of search, the replace function will mistakenly
                // think the find operation is complete and stop
                if (Replacing && (searchrow == ObjStartPos)) {
                    FindingForm.ResetSearch();
                    FirstFind = false;
                }
            }
            //main search loop
            do {
                if (FindDir == FindDirection.Up) {
                    // iterate backwards until word found or foundrow=-1
                    foundrow = searchrow - 1;
                    while (foundrow != -1) {
                        if (MatchWord) {
                            if (EditInvList[foundrow].ItemName.Equals(FindText, vbcComp)) {
                                // found
                                break;
                            }
                        }
                        else {
                            if (EditInvList[foundrow].ItemName.Contains(FindText, vbcComp)) {
                                // found
                                break;
                            }
                        }
                        foundrow--;
                    }
                    // reset searchpos
                    searchrow = EditInvList.Count;
                }
                else {
                    // iterate forward until word found or end reached (foundrow=objcount)
                    foundrow = searchrow;
                    do {
                        if (MatchWord) {
                            if (EditInvList[foundrow].ItemName.Equals(FindText, vbcComp)) {
                                // found
                                break;
                            }
                        }
                        else {
                            if (EditInvList[foundrow].ItemName.Contains(FindText, vbcComp)) {
                                // found
                                break;
                            }
                        }
                        foundrow++;
                    } while (foundrow < EditInvList.Count);
                    // reset searchrow
                    searchrow = 0;
                }
                // found?
                if (foundrow >= 0 && foundrow < EditInvList.Count) {
                    if (foundrow == ObjStartPos) {
                        foundrow = -1;
                    }
                    break;
                }
                // if not found, action depends on search mode
                switch (FindDir) {
                case FindDirection.Up:
                    if (!RestartSearch) {
                        DialogResult rtn;
                        if (blnRecurse) {
                            // just say no
                            rtn = DialogResult.No;
                        }
                        else {
                            rtn = MessageBox.Show(MDIMain,
                                "Beginning of search scope reached. Do you want to continue from the end?",
                                "Find in Object List",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                        }
                        if (rtn == DialogResult.No) {
                            // reset search
                            FindingForm.ResetSearch();
                            FirstFind = false;
                            MDIMain.UseWaitCursor = false;
                            return;
                        }
                    }
                    else {
                        // entire scope already searched; exit DO
                        break;
                    }
                    break;
                case FindDirection.Down:
                    if (!RestartSearch) {
                        DialogResult rtn;
                        if (blnRecurse) {
                            // just say no
                            rtn = DialogResult.No;
                        }
                        else {
                            rtn = MessageBox.Show(MDIMain,
                                "End of search scope reached. Do you want to continue from the beginning?",
                                "Find in Object List",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                        }
                        if (rtn == DialogResult.No) {
                            FindingForm.ResetSearch();
                            FirstFind = false;
                            MDIMain.UseWaitCursor = false;
                            return;
                        }
                    }
                    else {
                        // entire scope already searched; exit DO
                        break;
                    }
                    break;
                case FindDirection.All:
                    if (RestartSearch) {
                        // exit DO
                        break;
                    }
                    break;
                }
                if (RestartSearch) {
                    break;
                }
                RestartSearch = true;
            } while (true);
            // loop is exited by finding the searchtext or reaching end of search area

            if (foundrow >= 0 && foundrow < EditInvList.Count) {
                if (!FirstFind) {
                    //save this position
                    FirstFind = true;
                    ObjStartPos = foundrow;
                }
                fgObjects[1, foundrow].Selected = true;
                if (!fgObjects.Rows[foundrow].Displayed) {
                    fgObjects.FirstDisplayedScrollingRowIndex = foundrow - 2;
                }
                if (Replacing) {
                    if (MatchWord) {
                        ModifyItem(foundrow, ReplaceText);
                    }
                    else {
                        ModifyItem(foundrow, EditInvList[foundrow].ItemName.Replace(FindText, ReplaceText, vbcComp));
                    }
                    // adjust undoobject
                    UndoCol.Peek().UDAction = Replace;
                    //recurse the find method to get next occurrence
                    // !!!!!!!!ACK!!!!!!!!!!!!
                    // GET RID OF THIS RECURSION
                    blnRecurse = true;
                    FindInObjects(FindText, FindDir, MatchWord, MatchCase, false);
                    blnRecurse = false;
                }
            }
            else {
                if (!blnRecurse) {
                    if (FirstFind) {
                        // search complete; no more instances found
                        MessageBox.Show(MDIMain,
                            "The specified region has been searched.",
                            "Find in Object List",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else {
                        MessageBox.Show(MDIMain,
                            "Search text not found.",
                            "Find in Object List",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                FindingForm.ResetSearch();
                FirstFind = false;
            }
            fgObjects.Focus();
            MDIMain.UseWaitCursor = false;
        }

        public void ReplaceAll(string FindText, string ReplaceText, bool MatchWord, bool MatchCase) {
            StringComparison vbcComp = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int lngCount = 0;

            if (FindText.Equals(ReplaceText, vbcComp)) {
                return;
            }
            if (EditInvList.Count == 0) {
                MessageBox.Show(MDIMain,
                    "No inventory objects in list.",
                    "Find in Object List",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            MDIMain.UseWaitCursor = true;
            ObjectsUndo NextUndo = new() {
                UDAction = ObjectsUndo.ActionType.ReplaceAll
            };
            if (MatchWord) {
                for (int i = 0; i < EditInvList.Count; i++) {
                    if (EditInvList[i].ItemName.Equals(FindText, vbcComp)) {
                        // add  word being replaced to undo
                        if (lngCount == 0) {
                            NextUndo.UDObjectText = i + "|" + EditInvList[i].ItemName;
                        }
                        else {
                            NextUndo.UDObjectText += "\r" + i + "|" + EditInvList[i].ItemName;
                        }
                        EditInvList[i].ItemName = ReplaceText;
                        fgObjects[1, i].Value = ReplaceText;
                        lngCount++;
                    }
                }
            }
            else {
                for (int i = 0; i < EditInvList.Count; i++) {
                    if (EditInvList[i].ItemName.Contains(FindText, vbcComp)) {
                        // add  word being replaced to undo
                        if (lngCount == 0) {
                            NextUndo.UDObjectText = i + "|" + EditInvList[i].ItemName;
                        }
                        else {
                            NextUndo.UDObjectText += "\r" + i + "|" + EditInvList[i].ItemName;
                        }
                        EditInvList[i].ItemName = EditInvList[i].ItemName.Replace(FindText, ReplaceText, vbcComp);
                        fgObjects[1, i].Value = EditInvList[i].ItemName;
                        lngCount++;
                    }
                }
            }
            if (lngCount == 0) {
                MessageBox.Show(MDIMain,
                    "Search text not found.",
                    "Replace All",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else {
                AddUndo(NextUndo);
                MessageBox.Show(MDIMain,
                "The specified region has been searched. " + lngCount + " replacements were made.",
                "Replace All",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            }
            MDIMain.UseWaitCursor = false;
        }

        private void AddUndo(ObjectsUndo NextUndo) {
            UndoCol.Push(NextUndo);
            MarkAsChanged();
            FindingForm.ResetSearch();
            FirstFind = false;
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
            DialogResult rtn = DialogResult.No;
            if (InGame) {
                bool blnDontAsk = false;
                if (WinAGISettings.AutoUpdateObjects.Value == Common.Base.AskOption.Ask) {
                    rtn = MsgBoxEx.Show(MDIMain,
                       "Do you want to update all game logics with the changes made in the object list?",
                       "Update Logics?",
                       MessageBoxButtons.YesNo,
                       MessageBoxIcon.Question,
                       "Always take this action when saving the object list.", ref blnDontAsk);
                    if (blnDontAsk) {
                        if (rtn == DialogResult.Yes) {
                            WinAGISettings.AutoUpdateObjects.Value = Common.Base.AskOption.Yes;
                        }
                        else {
                            WinAGISettings.AutoUpdateObjects.Value = Common.Base.AskOption.No;
                        }
                        WinAGISettings.AutoUpdateObjects.WriteSetting(WinAGISettingsFile);
                    }
                }
                else {
                    if (WinAGISettings.AutoUpdateObjects.Value == Common.Base.AskOption.Yes) {
                        rtn = DialogResult.Yes;
                    }
                    else {
                        rtn = DialogResult.No;
                    }
                }
                if (rtn == DialogResult.Yes) {
                    // test cmds that use IObj:
                    //   has, obj.in.room
                    //
                    // action cmds that use IObj:
                    //   get, drop, put
                    FindingForm.Visible = false;
                    MDIMain.UseWaitCursor = true;
                    ProgressWin = new() {
                        Text = "Updating Inventory Objects in Logics"
                    };
                    ProgressWin.lblProgress.Text = "Locating modified item entries...";
                    ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count + LogicEditors.Count + 1;
                    ProgressWin.pgbStatus.Value = 0;
                    ProgressWin.Show(MDIMain);
                    ProgressWin.Refresh();

                    string FindText = "", replacetext = "", pattern;

                    foreach (frmLogicEdit loged in LogicEditors) {
                        if (loged.FormMode == LogicFormMode.Logic && loged.InGame) {
                            bool textchanged = false;
                            // run through all objects in the current object list
                            for (int i = 0; i < EditGame.InvObjects.Count; i++) {
                                bool replaceitem = false;
                                if (EditGame.InvObjects[i].ItemName != "?") {
                                    if (i >= EditInvList.Count) {
                                        if (EditGame.InvObjects[i].ItemName != "?") {
                                            // replace old item name with argument marker
                                            FindText = '"' + EditGame.InvObjects[i].ItemName + '"';
                                            replacetext = "i" + i.ToString();
                                            replaceitem = true;
                                        }
                                    }
                                    else {
                                        if (EditInvList[i].ItemName == "?" && EditGame.InvObjects[i].ItemName != "?") {
                                            // replace old item name with argument marker
                                            FindText = '"' + EditGame.InvObjects[i].ItemName + '"';
                                            replacetext = "i" + i.ToString();
                                            replaceitem = true;
                                        }
                                        else if (EditInvList[i].ItemName != EditGame.InvObjects[i].ItemName) {
                                            // replaceold item name with new name
                                            FindText = '"' + EditGame.InvObjects[i].ItemName + '"';
                                            replacetext = '"' + EditInvList[i].ItemName + '"';
                                            replaceitem = true;
                                        }
                                    }
                                    if (replaceitem) {
                                        pattern = Regex.Escape(FindText);
                                        MatchCollection mc = Regex.Matches(loged.fctb.Text, pattern);
                                        if (mc.Count > 0) {
                                            ProgressWin.lblProgress.Text = "Updating editor for " + loged.EditLogic.ID;
                                            ProgressWin.Refresh();
                                            for (int m = mc.Count - 1; m >= 0; m--) {
                                                Place pl = loged.fctb.PositionToPlace(mc[m].Index);
                                                AGIToken token = loged.fctb.TokenFromPos(pl);
                                                // test cmds that use IObj:
                                                //   has, obj.in.room
                                                //
                                                // action cmds that use IObj:
                                                //   get, drop, put
                                                AGIToken token2 = loged.fctb.PreviousToken(token);
                                                if (token2.Text == "(") {
                                                    token2 = loged.fctb.PreviousToken(token2);
                                                    if (token2.Text == "has" || token2.Text == "obj.in.room" ||
                                                        token2.Text == "get" || token2.Text == "drop" || token2.Text == "put") {
                                                        loged.fctb.ReplaceToken(token, replacetext);
                                                        textchanged = true;
                                                    }
                                                }
                                            }
                                            ProgressWin.lblProgress.Text = "Locating modified define names...";
                                            ProgressWin.Refresh();
                                        }
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
                        // run through all objects in the current object list
                        for (int i = 0; i < EditGame.InvObjects.Count; i++) {
                            bool replaceitem = false;
                            if (EditGame.InvObjects[i].ItemName != "?") {
                                if (i >= EditInvList.Count) {
                                    if (EditGame.InvObjects[i].ItemName != "?") {
                                        // replace old item name with argument marker
                                        FindText = '"' + EditGame.InvObjects[i].ItemName + '"';
                                        replacetext = "i" + i.ToString();
                                        replaceitem = true;
                                    }
                                }
                                else {
                                    if (EditInvList[i].ItemName == "?" && EditGame.InvObjects[i].ItemName != "?") {
                                        // replace old item name with argument marker
                                        FindText = '"' + EditGame.InvObjects[i].ItemName + '"';
                                        replacetext = "i" + i.ToString();
                                        replaceitem = true;
                                    }
                                    else if (EditInvList[i].ItemName != EditGame.InvObjects[i].ItemName) {
                                        // replaceold item name with new name
                                        FindText = '"' + EditGame.InvObjects[i].ItemName + '"';
                                        replacetext = '"' + EditInvList[i].ItemName + '"';
                                        replaceitem = true;
                                    }
                                }
                                if (replaceitem) {
                                    // load first
                                    if (unload) {
                                        logic.Load();
                                    }
                                    pattern = Regex.Escape(FindText);
                                    MatchCollection mc = Regex.Matches(logic.SourceText, pattern);
                                    if (mc.Count > 0) {
                                        ProgressWin.lblProgress.Text = "Updating source code for " + logic.ID;
                                        ProgressWin.Refresh();
                                        for (int m = mc.Count - 1; m >= 0; m--) {
                                            AGIToken token = WinAGIFCTB.TokenFromPos(logic.SourceText, mc[m].Index);
                                            // test cmds that use IObj:
                                            //   has, obj.in.room
                                            //
                                            // action cmds that use IObj:
                                            //   get, drop, put
                                            AGIToken token2 = WinAGIFCTB.PreviousToken(logic.SourceText, token);
                                            if (token2.Text == "(") {
                                                token2 = WinAGIFCTB.PreviousToken(logic.SourceText, token2);
                                                if (token2.Text == "has" || token2.Text == "obj.in.room" ||
                                                    token2.Text == "get" || token2.Text == "drop" || token2.Text == "put") {
                                                    logic.SourceText = logic.SourceText.ReplaceFirst(FindText, replacetext, mc[m].Index);
                                                    textchanged = true;
                                                }
                                            }
                                        }
                                        ProgressWin.lblProgress.Text = "Locating modified define names...";
                                        ProgressWin.Refresh();
                                    }
                                }
                            }
                        }
                        if (textchanged) {
                            logic.SaveSource();
                            //update reslist
                            RefreshTree(AGIResType.Logic, logic.Number);
                        }
                        if (unload) {
                            logic.Unload();
                        }
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.Refresh();
                    }
                    ProgressWin.Close();
                    MDIMain.UseWaitCursor = false;
                }
            }
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

        internal void InitFonts() {
            formFont = new(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            dupFont = new(formFont, FontStyle.Bold);
            Label1.Font = formFont;
            txtMaxScreenObjs.Font = formFont;
            Label1.Left = txtMaxScreenObjs.Left - Label1.Width - 1;
            fgObjects.Top = txtMaxScreenObjs.Height;
            fgObjects.Font = formFont;
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

        private void ModifyItem(int index, string NewItem, bool DontUndo = false) {
            if (EditInvList[index].ItemName == NewItem) {
                return;
            }
            if (!DontUndo) {
                ObjectsUndo NextUndo = new() {
                    UDAction = ObjectsUndo.ActionType.ModifyItem,
                    UDObjectNo = index,
                    UDObjectText = EditInvList[index].ItemName
                };
                AddUndo(NextUndo);
            }
            EditInvList[index].ItemName = NewItem;
            fgObjects[1, index].Value = NewItem;
            MarkAsChanged();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // Enter key is usually captured by the grid, but we want it to go to the textbox
            switch (keyData) {
            case Keys.Enter:
                if (EditTextBox.Focused) {
                    EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Enter));
                    return true;
                }
                break;
            case Keys.Tab:
                if (EditTextBox.Focused) {
                    //if (fgObjects.IsCurrentCellInEditMode) {
                    EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Tab));
                    return true;
                }
                break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
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
            statusStrip1.Items["spCount"].Text = "Object Count: " + EditInvList.Count;
            statusStrip1.Items["spEncrypt"].Text = EditInvList.Encrypted ? "Encrypted" : "Not Encrypted";
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
#endregion
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
