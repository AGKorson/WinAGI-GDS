using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.WinAGIFCTB;
using static WinAGI.Common.Base;
using System.ComponentModel;

namespace WinAGI.Editor {
    public partial class frmMenuEdit : Form {

        #region Members
        internal bool IsChanged;
        internal int PicScale;
        private readonly Bitmap chargrid;
        private readonly Bitmap invchargrid;
        private int BkgdPicNum = -1;
        internal int MenuLogic = -1;
        private string[] strMessages = new string[256];
        private bool blnGotMsgs = false;
        internal bool Canceled = false;
        private int clickMenu, clickItem;
        private DataGridViewCell captionLabel, captionValue, ctrlLabel, ctrlValue;
        private DataGridViewCellStyle defaultStyle, highlightStyle;
        private TextBox EditTextBox = null;
        private bool EditingCaption = false;
        // ToolStrip Items
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        #endregion

        public frmMenuEdit() {
            bool blnLoaded;
            bool blnExtracted;

            InitializeComponent();
            InitToolStrip();
            MdiParent = MDIMain;

            // mark editor as inuse
            MEInUse = true;
            // set width so test menu will fit to right of treeview list
            // choose a scale such that window is approximately 1/2 of screen
            int lngBorders = Width - ClientRectangle.Width;
            PicScale = Screen.PrimaryScreen.WorkingArea.Width / (320 + lngBorders) / 2;
            // never less than one
            if (PicScale < 1) {
                PicScale = 1;
            }

            // determine codepage to use
            int codepage;

            // if game is loaded,
            if (EditGame is not null) {
                codepage = EditGame.CodePage;
                do {
                    // choose a logic for extracting a menu
                    frmGetResourceNum frm = new(GetRes.Menu, AGIResType.Logic);
                    if (frm.ShowDialog() == DialogResult.Cancel) {
                        Canceled = true;
                        return;
                    }
                    // load the logic if necessary
                    blnLoaded = EditGame.Logics[frm.NewResNum].Loaded;
                    if (!blnLoaded) {
                        EditGame.Logics[frm.NewResNum].Load();
                    }
                    // note number
                    MenuLogic = frm.NewResNum;
                    // extract the menu (return false if no menu found)
                    blnExtracted = ExtractMenu(EditGame.Logics[frm.NewResNum].SourceText);
                    // unload the resource if that's how it was found
                    if (!blnLoaded) {
                        EditGame.Logics[frm.NewResNum].Unload();
                    }
                    if (!blnExtracted) {
                        MessageBox.Show(MDIMain,
                            "This logic does not have a valid menu structure. Please choose another logic.",
                            "No Menu Found",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                } while (!blnExtracted);
            }
            else {
                // not in a game; create a default menu
                codepage = 437; // default to IBM PC US
                DefaultMenu();
                MenuLogic = -1;
            }
            // load correct charmap
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
            // select a default background
            DefaultBackground();

            // select first menu item
            tvwMenu.SelectedNode = tvwMenu.Nodes[0];
            tvwMenu.ExpandAll();
            // st up datagrid as a property grid
            dgProps.Rows.Add("Caption", "cap");
            dgProps.Rows.Add("Controller", "ctrl");
            captionLabel = dgProps.Rows[0].Cells[0];
            captionValue = dgProps.Rows[0].Cells[1];
            ctrlLabel = dgProps.Rows[1].Cells[0];
            ctrlValue = dgProps.Rows[1].Cells[1];
            defaultStyle = dgProps.Columns[0].DefaultCellStyle;
            highlightStyle = defaultStyle.Clone();
            highlightStyle.SelectionBackColor = highlightStyle.BackColor = SystemColors.Highlight;
            highlightStyle.SelectionForeColor = highlightStyle.ForeColor = SystemColors.HighlightText;
        }

        #region Event Handlers
        #region Form Events
        private void frmMenuEdit_Load(object sender, EventArgs e) {
            // set propertygrid height/width
            dgProps.Height = 2 * dgProps.Rows[0].Height + 3;
            dgProps.Columns[0].Width = (int)(dgProps.Width * 0.4);
            dgProps.Columns[1].Width = (int)(dgProps.Width * 0.6);
            tvwMenu.Height = dgProps.Top - tvwMenu.Top - 5;
            DrawBackground();
        }

        private void frmMenuEdit_Leave(object sender, EventArgs e) {
            // if editing, need to cancel; otherwise, the edit text box
            // control stays active, and any other form will not be able
            // to edit its grid cells
            if (dgProps.IsCurrentCellInEditMode) {
                // same as pressing Escape
                EditTextBox_KeyDown(EditTextBox, new KeyEventArgs(Keys.Escape));
            }
        }

        private void frmMenuEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            e.Cancel = !AskClose();
        }

        private void frmMenuEdit_FormClosed(object sender, FormClosedEventArgs e) {
            EditTextBox?.Dispose();
            EditTextBox = null;
            MenuEditor = null;
            MEInUse = false;
        }
        #endregion

        #region Menu Events
        internal void SetResourceMenu() {
            MDIMain.mnuRSep2.Visible = false;
            MDIMain.mnuRSep3.Visible = true;
            mnuUpdateLogic.Enabled = EditGame is not null && MenuLogic >= 0 && IsChanged;
            mnuSaveAsDefault.Enabled = true;
            mnuBackground.Enabled = EditGame is not null && EditGame.Pictures.Count > 0;
            mnuHotKeys.Checked = WinAGISettings.AutoAlignHotKey.Value;
        }

        internal void ResetResourceMenu() {
            mnuUpdateLogic.Enabled = true;
            mnuSaveAsDefault.Enabled = true;
        }

        internal void mnuUpdateLogic_Click(object sender, EventArgs e) {
            if (EditGame is not null && MenuLogic >= 0 && IsChanged) {
                UpdateSourceLogic();
            }
        }

        internal void mnuRSaveDefault_Click(object sender, EventArgs e) {
            SaveAsDefault();
        }

        private void mnuBackground_Click(object sender, EventArgs e) {
            if (EditGame is not null) {
                if (ChooseBackground()) {
                    DrawBackground();
                }
            }
        }

        private void mnuHotKeys_Click(object sender, EventArgs e) {
            // toggle the auto-align hot key option
            WinAGISettings.AutoAlignHotKey.Value = !WinAGISettings.AutoAlignHotKey.Value;
            WinAGISettings.AutoAlignHotKey.WriteSetting(WinAGISettingsFile);
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            SetEditMenu();
            mnuEdit.DropDownItems.AddRange([
                mnuMoveUp,
                mnuMoveDown,
                mnuDelete,
                mnuInsert,
                mnuCopy,
                mnuReset]);
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            contextMenuStrip1.Items.AddRange([
                mnuMoveUp,
                mnuMoveDown,
                mnuDelete,
                mnuInsert,
                mnuCopy,
                mnuReset]);
            ResetEditMenu();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            SetEditMenu();
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void SetEditMenu() {
            switch (tvwMenu.SelectedNode.Level) {
            case 0:
                mnuInsert.Text = "Insert Menu";
                mnuInsert.Enabled = MenuBarWidth() < 39;
                mnuDelete.Text = "Delete Menu";
                // can't delete last menu
                mnuDelete.Enabled = tvwMenu.Nodes.Count > 1;
                mnuMoveUp.Text = "Move Menu Up";
                // can't move first menu up
                mnuMoveUp.Enabled = tvwMenu.SelectedNode != tvwMenu.Nodes[0];
                mnuMoveDown.Text = "Move Menu Down";
                // can't move last menu down
                mnuMoveDown.Enabled = tvwMenu.SelectedNode != tvwMenu.Nodes[^1];
                break;
            case 1:
                mnuInsert.Text = "Insert Menu Item";
                // limit to 22 items
                mnuInsert.Enabled = tvwMenu.SelectedNode.Parent.Nodes.Count < 22;
                mnuDelete.Text = "Delete Menu Item";
                mnuDelete.Enabled = true;
                mnuMoveUp.Text = "Move Menu Item Up";
                // can't move first item up
                mnuMoveUp.Enabled = tvwMenu.SelectedNode != tvwMenu.SelectedNode.Parent.FirstNode;
                mnuMoveDown.Text = "Move Menu Item Down";
                // can't move last item down
                mnuMoveDown.Enabled = tvwMenu.SelectedNode != tvwMenu.SelectedNode.Parent.LastNode;
                break;
            }
        }

        private void ResetEditMenu() {
            mnuMoveUp.Enabled = true;
            mnuMoveDown.Enabled = true;
            mnuDelete.Enabled = true;
            mnuInsert.Enabled = true;
            mnuCopy.Enabled = true;
            mnuReset.Enabled = true;
        }

        private void mnuMoveUp_Click(object sender, EventArgs e) {
            // shift up
            if (tvwMenu.SelectedNode is null) {
                return;
            }
            switch (tvwMenu.SelectedNode.Level) {
            case 0:
                // shift menu
                if (tvwMenu.SelectedNode == tvwMenu.Nodes[0]) {
                    // can't shift up
                    return;
                }
                (tvwMenu.SelectedNode.Text, tvwMenu.SelectedNode.PrevNode.Text) = (tvwMenu.SelectedNode.PrevNode.Text, tvwMenu.SelectedNode.Text);
                (tvwMenu.SelectedNode.Tag, tvwMenu.SelectedNode.PrevNode.Tag) = (tvwMenu.SelectedNode.PrevNode.Tag, tvwMenu.SelectedNode.Tag);
                // swap the child nodes too
                List<TreeNode> temp = [];
                for (int i = 0; i < tvwMenu.SelectedNode.Nodes.Count; i++) {
                    temp.Add(tvwMenu.SelectedNode.Nodes[i]);
                }
                tvwMenu.SelectedNode.Nodes.Clear();
                for (int i = 0; i < tvwMenu.SelectedNode.PrevNode.Nodes.Count; i++) {
                    tvwMenu.SelectedNode.Nodes.Add((TreeNode)tvwMenu.SelectedNode.PrevNode.Nodes[i].Clone());
                }
                tvwMenu.SelectedNode.PrevNode.Nodes.Clear();
                for (int i = 0; i < temp.Count; i++) {
                    tvwMenu.SelectedNode.PrevNode.Nodes.Add((TreeNode)temp[i].Clone());
                }
                tvwMenu.SelectedNode = tvwMenu.SelectedNode.PrevNode;
                break;
            case 1:
                if (tvwMenu.SelectedNode == tvwMenu.SelectedNode.Parent.FirstNode) {
                    // can't shift up
                    return;
                }
                (tvwMenu.SelectedNode.Text, tvwMenu.SelectedNode.PrevNode.Text) = (tvwMenu.SelectedNode.PrevNode.Text, tvwMenu.SelectedNode.Text);
                (tvwMenu.SelectedNode.Tag, tvwMenu.SelectedNode.PrevNode.Tag) = (tvwMenu.SelectedNode.PrevNode.Tag, tvwMenu.SelectedNode.Tag);
                (tvwMenu.SelectedNode.ForeColor, tvwMenu.SelectedNode.PrevNode.ForeColor) = (tvwMenu.SelectedNode.PrevNode.ForeColor, tvwMenu.SelectedNode.ForeColor);
                tvwMenu.SelectedNode = tvwMenu.SelectedNode.PrevNode;
                break;
            }
            MarkAsChanged();
        }

        private void mnuMoveDown_Click(object sender, EventArgs e) {
            // shift down
            if (tvwMenu.SelectedNode is null) {
                return;
            }
            switch (tvwMenu.SelectedNode.Level) {
            case 0:
                if (tvwMenu.SelectedNode == tvwMenu.Nodes[^1]) {
                    // can't shift down
                    return;
                }
                (tvwMenu.SelectedNode.Text, tvwMenu.SelectedNode.NextNode.Text) = (tvwMenu.SelectedNode.NextNode.Text, tvwMenu.SelectedNode.Text);
                (tvwMenu.SelectedNode.Tag, tvwMenu.SelectedNode.NextNode.Tag) = (tvwMenu.SelectedNode.NextNode.Tag, tvwMenu.SelectedNode.Tag);
                // swap the child nodes too
                List<TreeNode> temp = [];
                for (int i = 0; i < tvwMenu.SelectedNode.Nodes.Count; i++) {
                    temp.Add(tvwMenu.SelectedNode.Nodes[i]);
                }
                tvwMenu.SelectedNode.Nodes.Clear();
                for (int i = 0; i < tvwMenu.SelectedNode.NextNode.Nodes.Count; i++) {
                    tvwMenu.SelectedNode.Nodes.Add((TreeNode)tvwMenu.SelectedNode.NextNode.Nodes[i].Clone());
                }
                tvwMenu.SelectedNode.NextNode.Nodes.Clear();
                for (int i = 0; i < temp.Count; i++) {
                    tvwMenu.SelectedNode.NextNode.Nodes.Add((TreeNode)temp[i].Clone());
                }
                tvwMenu.SelectedNode = tvwMenu.SelectedNode.NextNode;
                break;
            case 1:
                if (tvwMenu.SelectedNode == tvwMenu.SelectedNode.Parent.LastNode) {
                    // can't shift down
                    return;
                }
                (tvwMenu.SelectedNode.Text, tvwMenu.SelectedNode.NextNode.Text) = (tvwMenu.SelectedNode.NextNode.Text, tvwMenu.SelectedNode.Text);
                (tvwMenu.SelectedNode.Tag, tvwMenu.SelectedNode.NextNode.Tag) = (tvwMenu.SelectedNode.NextNode.Tag, tvwMenu.SelectedNode.Tag);
                (tvwMenu.SelectedNode.ForeColor, tvwMenu.SelectedNode.NextNode.ForeColor) = (tvwMenu.SelectedNode.NextNode.ForeColor, tvwMenu.SelectedNode.ForeColor);
                tvwMenu.SelectedNode = tvwMenu.SelectedNode.NextNode;
                break;
            }
            MarkAsChanged();
        }

        private void mnuDelete_Click(object sender, EventArgs e) {
            TreeNode tmpNode = null;

            if (tvwMenu.SelectedNode is null) {
                return;
            }
            switch (tvwMenu.SelectedNode.Level) {
            case 0:
                // can't delete last menu
                if (tvwMenu.Nodes.Count == 1) {
                    return;
                }
                if (tvwMenu.SelectedNode.NextNode is not null) {
                    tmpNode = tvwMenu.SelectedNode.NextNode;
                }
                else {
                    tmpNode = tvwMenu.SelectedNode.PrevNode;
                }
                break;
            case 1:
                // if deleting last item, select the menu
                if (tvwMenu.SelectedNode.Parent.Nodes.Count == 1) {
                    tmpNode = tvwMenu.SelectedNode.Parent;
                }
                else {
                    if (tvwMenu.SelectedNode.NextNode is not null) {
                        tmpNode = tvwMenu.SelectedNode.NextNode;
                    }
                    else {
                        tmpNode = tvwMenu.SelectedNode.PrevNode;
                    }
                }
                break;
            }
            tvwMenu.Nodes.Remove(tvwMenu.SelectedNode);
            tvwMenu.SelectedNode = tmpNode;
            picBackground.Invalidate();
            MarkAsChanged();
        }

        private void mnuInsert_Click(object sender, EventArgs e) {
            if (tvwMenu.SelectedNode is not null) {
                switch (tvwMenu.SelectedNode.Level) {
                case 0:
                    // is there enough room for aat least 1 letter?
                    int menulength = 0;
                    foreach (TreeNode tn in tvwMenu.Nodes) {
                        menulength += tn.Text.Length + 1;
                    }
                    if (menulength > 38) {
                        // not enough room
                        MessageBox.Show(MDIMain,
                            "There is not enough room to add another menu. Remove " +
                             "or shorten some of the existing menus, and try again.",
                             "Insert Menu",
                             MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                             WinAGIHelp, "htm\\agi\\menu.htm#displaymenus");
                        return;
                    }
                    else if (menulength > 35) {
                        // room for four or less characters
                        MessageBox.Show(MDIMain,
                            "There is only room for a menu that is " + (39 - menulength) +
                             " characters.",
                             "Menu Bar Size Limit Warning",
                             MessageBoxButtons.OK,
                            MessageBoxIcon.Warning, 0, 0,
                             WinAGIHelp, "htm\\agi\\menu.htm#displaymenus");
                    }

                    // add an menu to end
                    tvwMenu.Nodes.Add("").Expand();
                    // add a default item
                    tvwMenu.Nodes[^1].Nodes.Add("item").Tag = "";
                    tvwMenu.Nodes[^1].Nodes[0].ForeColor = Color.Red;
                    // select the menu
                    tvwMenu.SelectedNode = tvwMenu.Nodes[^1];
                    tvwMenu.SelectedNode.Expand();
                    // edit it
                    dgProps.Rows[0].Cells[1].Selected = true;
                    dgProps.Select();
                    dgProps.BeginEdit(false);
                    break;
                case 1:
                    if (tvwMenu.SelectedNode.Parent.Nodes.Count > 22) {
                        // not enough room
                        MessageBox.Show(MDIMain,
                            "This menu already has the maximium number of items allowed.",
                             "Insert Menu Item",
                             MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                             WinAGIHelp, "htm\\agi\\menu.htm#displaymenus");
                        return;
                    }
                    // add an menu item to end
                    tvwMenu.SelectedNode.Parent.Nodes.Add("").Tag = "";
                    tvwMenu.SelectedNode.Parent.Nodes[^1].ForeColor = Color.Red;
                    // select the menu item
                    tvwMenu.SelectedNode = tvwMenu.SelectedNode.Parent.Nodes[^1];
                    // edit it
                    dgProps.Rows[0].Cells[1].Selected = true;
                    dgProps.Select();
                    dgProps.BeginEdit(false);
                    break;
                }
            }
        }

        private void mnuCopy_Click(object sender, EventArgs e) {
            CopyMenuToClipboard();
        }

        private void mnuReset_Click(object sender, EventArgs e) {
            ResetToDefault();
        }
        #endregion

        #region EditTextBox Menu Events
        private void cmCel_Opening(object sender, CancelEventArgs e) {
            mnuCelUndo.Enabled = EditTextBox.CanUndo;
            mnuCelCut.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCopy.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelPaste.Enabled = Clipboard.ContainsText();
            mnuCelDelete.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCharMap.Visible = dgProps.CurrentCell.RowIndex == 0;
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
            if (dgProps.CurrentCell.RowIndex != 0) {
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
            dgProps.CancelEdit();
            // cancel alone doesn't work (the cell remains in edit mode)
            // but calling EndEdit immediately after seems to work
            dgProps.EndEdit();
        }
        #endregion

        #region Control Events
        private void tvwMenu_MouseDown(object sender, MouseEventArgs e) {
            // force selection of node on right-click
            if (e.Button == MouseButtons.Right) {
                TreeNode node = tvwMenu.GetNodeAt(e.X, e.Y);
                if (node is not null) {
                    tvwMenu.SelectedNode = node;
                }
            }
        }

        private void tvwMenu_AfterSelect(object sender, TreeViewEventArgs e) {
            // update selected menu/item and redraw
            if (tvwMenu.SelectedNode is not null) {
                switch (tvwMenu.SelectedNode.Level) {
                case 0:
                    // menu
                    captionValue.Value = tvwMenu.SelectedNode.Text;
                    ctrlLabel.Value = "";
                    ctrlLabel.ReadOnly = true;
                    ctrlValue.Value = "";
                    ctrlValue.ReadOnly = true;
                    break;
                case 1:
                    // menu item
                    captionValue.Value = tvwMenu.SelectedNode.Text;
                    ctrlLabel.Value = "Controller";
                    ctrlLabel.ReadOnly = false;
                    ctrlValue.Value = tvwMenu.SelectedNode.Tag;
                    ctrlValue.ReadOnly = false;
                    break;
                }
                // reset propertybox formatting
                captionLabel.Style = defaultStyle;
                ctrlLabel.Style = defaultStyle;
                ConfigureToolbar();
                picBackground.Invalidate();
            }
        }

        private void tvwMenu_DoubleClick(object sender, EventArgs e) {
            // begin editing selected item
            if (tvwMenu.SelectedNode is not null) {
                // ********************************************************
                // Unfortunately there is no way to change the e.Label
                // value which means captions can't be padded after
                // entering. Unless/until that is solved, no in-tree
                // editing of captions is allowed.
                // ********************************************************
                //// disable delete shortcut while editing label
                //mnuDelete.ShortcutKeys = Keys.None;
                //tvwMenu.SelectedNode.BeginEdit();
                //return;
                // ********************************************************
                dgProps.Select();
                dgProps.Rows[0].Cells[1].Selected = true;
                dgProps.BeginEdit(true);
            }
        }

        // ********************************************************
        // Unfortunately there is no way to change the e.Label
        // value which means captions can't be padded after
        // entering. Unless/until that is solved, no in-tree
        // editing of captions is allowed.
        // ********************************************************
        //private void tvwMenu_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
        //    // Unfortunately there is no way to change the e.Label value
        //    // which means captions can't be padded after entering.
        //    // Unless/until that is solved, no in-tree editing of 
        //    // captions is allowed
        //    if (e.Label is null) {
        //        e.CancelEdit = true;
        //    }
        //    else {
        //        switch (tvwMenu.SelectedNode.Level) {
        //        case 0:
        //            if (!UpdateCaption(e.Label)) {
        //                // cancel the edit
        //                e.CancelEdit = true;
        //            }
        //            break;
        //        case 1:
        //            // only caption is editable in the treeview
        //            if (!UpdateCaption(e.Label)) {
        //                // cancel the edit

        //                e.CancelEdit = true;
        //            }
        //            break;
        //        }
        //    }
        //    // restore delete shortcut
        //    mnuDelete.ShortcutKeys = Keys.Delete;
        //}
        // ********************************************************

        private void tvwMenu_BeforeCollapse(object sender, TreeViewCancelEventArgs e) {
            e.Cancel = true;
        }

        private void picBackground_MouseDown(object sender, MouseEventArgs e) {
            //  check if cursor is over a menu or menu item
            MouseOverMenu(e.Location, out clickMenu, out clickItem);
            Debug.Print("Click at {0},{1} over menu {2} item {3}", e.X, e.Y, clickMenu, clickItem);
            if (clickItem >= 0) {
                // menu item clicked
                if (!tvwMenu.Nodes[clickMenu].Nodes[clickItem].IsSelected) {
                    tvwMenu.SelectedNode = tvwMenu.Nodes[clickMenu].Nodes[clickItem];
                }
            }
            else if (clickMenu >= 0) {
                // menu clicked
                if (!tvwMenu.Nodes[clickMenu].IsSelected) {
                    tvwMenu.SelectedNode = tvwMenu.Nodes[clickMenu];
                }
            }
        }

        private void picBackground_DoubleClick(object sender, EventArgs e) {
            if (clickItem >= 0 || clickMenu >= 0) {
                dgProps.Select();
                dgProps.Rows[0].Cells[1].Selected = true;
                dgProps.BeginEdit(true);
            }
            else {
                // choose a different picture for the background
                if (ChooseBackground()) {
                    DrawBackground();
                }
            }
        }

        private void picBackground_Paint(object sender, PaintEventArgs e) {
            if (!Visible) {
                return;
            }
            Debug.Assert(tvwMenu.Nodes.Count != 0);
            Debug.Assert(tvwMenu.SelectedNode is not null);
            int menu = 0, item = -1;
            switch (tvwMenu.SelectedNode.Level) {
            case 0:
                menu = tvwMenu.SelectedNode.Index;
                item = -1;
                break;
            case 1:
                menu = tvwMenu.SelectedNode.Parent.Index;
                item = tvwMenu.SelectedNode.Index;
                break;
            }
            DrawMenu(e.Graphics, menu, item);
        }

        private void dgProps_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
            if (e.Control is TextBox) {
                // need to set MultiLine to true so the ENTER key can be captured by
                // KeyPress event
                EditTextBox = e.Control as TextBox;
                if (EditTextBox.ContextMenuStrip != cmCel) {
                    EditTextBox.ContextMenuStrip = cmCel;
                    EditTextBox.Multiline = true;
                    EditTextBox.AcceptsReturn = true;
                    EditTextBox.AcceptsTab = true;
                    //EditTextBox.Validating += EditTextBox_Validating;
                    EditTextBox.KeyDown += EditTextBox_KeyDown;
                }
                else {
                    EditTextBox.Multiline = true;
                    EditTextBox.AcceptsReturn = true;
                    EditTextBox.AcceptsTab = true;
                }
            }
        }

        private void dgProps_CellClick(object sender, DataGridViewCellEventArgs e) {
            // highlight correct label
            switch (tvwMenu.SelectedNode.Level) {
            case 0:
                // always highlight the caption row label
                captionLabel.Style = highlightStyle;
                break;
            case 1:
                switch (e.RowIndex) {
                case 0:
                    captionLabel.Style = highlightStyle;
                    ctrlLabel.Style = defaultStyle;
                    break;
                case 1:
                    captionLabel.Style = defaultStyle;
                    ctrlLabel.Style = highlightStyle;
                    break;
                }
                break;
            }
        }

        private void dgProps_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            // only if editing
            if (!dgProps.IsCurrentCellInEditMode) {
                return;
            }
            switch (tvwMenu.SelectedNode.Level) {
            case 0:
                // only caption
                if (!UpdateCaption((string)e.FormattedValue)) {
                    // cancel the edit
                    e.Cancel = true;
                }
                break;
            case 1:
                if (e.RowIndex == 0) {
                    // caption
                    if (!UpdateCaption((string)e.FormattedValue)) {
                        // cancel the edit
                        e.Cancel = true;
                    }
                    else {
                        // if edit was OK, need to allow the
                        // post-validation event to update the
                        // caption text after it gets adjusted
                        // for spacing
                        EditingCaption = true;
                    }
                }
                else {
                    // controller
                    if (!UpdateController((string)e.FormattedValue)) {
                        // cancel the edit
                        e.Cancel = true;
                    }
                    else {
                        // if edit was OK, need to allow the
                        // post-validation event to update the
                        // caption text after it gets adjusted
                        // for spacing
                        EditingCaption = true;
                    }
                }
                break;
            }
        }

        private void dgProps_CellValidated(object sender, DataGridViewCellEventArgs e) {
            if (tvwMenu.SelectedNode.Level == 1) {
                if (dgProps.Rows[e.RowIndex].Cells[e.ColumnIndex] != dgProps.CurrentCell) {
                    return;
                }
                if (!EditingCaption) {
                    return;
                }
                EditingCaption = false;
                // if edit was OK, still need to update
                // value of propgrid to the formatted value
                if (dgProps.CurrentCell.RowIndex == 0) {
                    dgProps.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = tvwMenu.SelectedNode.Text;
                }
                else {
                    dgProps.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = (string)tvwMenu.SelectedNode.Tag;
                }
            }
        }
        #endregion

        #region EditTextBox Events
        private void EditTextBox_KeyDown(object sender, KeyEventArgs e) {

            if (e.KeyCode == Keys.Escape) {
                // just cancel, and restore previous value
                dgProps.CancelEdit();
                dgProps.EndEdit();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
        }
        #endregion
        #endregion

        #region Methods
        private void InitToolStrip() {
            spStatus = MDIMain.spStatus;
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;
        }

        private void ConfigureToolbar() {
            // configure toolstrip
            if (tvwMenu.SelectedNode.Level == 0) {
                btnInsert.Enabled = MenuBarWidth() < 39;
                btnInsert.Text = "Insert Menu";
                btnDelete.Enabled = tvwMenu.Nodes.Count > 1;
                btnDelete.Text = "Delete Menu";
                btnMoveUp.Enabled = tvwMenu.SelectedNode != tvwMenu.Nodes[0];
                btnMoveUp.Text = "Move Menu Up";
                btnMoveDown.Enabled = tvwMenu.SelectedNode != tvwMenu.Nodes[^1];
                btnMoveDown.Text = "Move Menu Down";
            }
            else {
                btnInsert.Enabled = tvwMenu.SelectedNode.Parent.Nodes.Count < 22;
                btnInsert.Text = "Insert Item";
                btnDelete.Enabled = true;
                btnDelete.Text = "Delete Item";
                btnMoveUp.Enabled = tvwMenu.SelectedNode != tvwMenu.SelectedNode.Parent.FirstNode;
                btnMoveUp.Text = "Move Item Up";
                btnMoveDown.Enabled = tvwMenu.SelectedNode != tvwMenu.SelectedNode.Parent.LastNode;
                btnMoveDown.Text = "Move Item Down";
            }
        }

        private int MenuBarWidth() {
            int retval = 0;
            foreach (TreeNode node in tvwMenu.Nodes) {
                retval += node.Text.Length + 1;
            }
            return retval;
        }

        private int MenuPos(int menunum) {
            if (menunum < 0 || menunum >= tvwMenu.Nodes.Count) {
                return -1;
            }
            if (menunum == 0) {
                return 1;
            }
            int retval = 0;
            for (int i = 0; i < menunum; i++) {
                retval += tvwMenu.Nodes[i].Text.Length + 1;
            }
            return retval + 1;
        }

        public void UpdateSourceLogic() {

            bool blnLoaded;
            int lngMenuPos = 0, lngSubmitPos = 0;

            if (AnyBlankControllers()) {
                if (MessageBox.Show(MDIMain,
                    "There are one or more blank controllers in this menu. " +
                    "Do you want to correct that before saving?",
                    "Blank Controllers",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes) {
                    return;
                }
            }

            // create the menu
            string strMenu = CreateMenu();

            Debug.Assert(MenuLogic != -1);
            if (!EditGame.Logics.Contains(MenuLogic)) {
                // logic not found; may have been deleted from game
                MessageBox.Show(MDIMain,
                    "The logic that this menu came from is no longer part of " +
                     "your game. You can copy the menu structure the the " +
                     "clipboard and  manually paste it into a different logic.",
                    "Save Menu Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                MenuLogic = -1;
                return;
            }
            // find the logic
            Logic menulogic = EditGame.Logics[MenuLogic];
            blnLoaded = menulogic.Loaded;
            if (!blnLoaded) {
                menulogic.Load();
            }
            // if existing text has the menu editor header
            if (menulogic.SourceText.Contains(LoadResString(102), StringComparison.CurrentCulture)) {
                // remove it
                menulogic.SourceText = menulogic.SourceText.Replace(LoadResString(102), "");
            }
            // find the starting and ending pos of the menu
            if (HasMenu(menulogic.SourceText, ref lngMenuPos, ref lngSubmitPos)) {
                // move start and end positions to line up with newlines
                lngMenuPos = menulogic.SourceText.LastIndexOf(Environment.NewLine, lngMenuPos + 1) + 2;
                lngSubmitPos = menulogic.SourceText.IndexOf(Environment.NewLine, lngSubmitPos) + 2;

                // add new menu in place of existing source
                menulogic.SourceText = menulogic.SourceText.Left(lngMenuPos) + strMenu + menulogic.SourceText.Right(menulogic.SourceText.Length - lngSubmitPos);
                menulogic.SaveSource();
            }
            else {
                // add new menu to beginning of source (skipping any comments or blank lines)
                lngMenuPos = 0;
                lngSubmitPos = 0;
                AGIToken next = new();
                do {
                    next = WinAGIFCTB.NextToken(menulogic.SourceText, 0, true);
                    if (next.Type != AGITokenType.LineBreak && next.Type != AGITokenType.Comment) {
                        break;
                    }
                } while (next.Type != AGITokenType.None);
                // lngSubmitPos-1 is where to put the thing
                lngSubmitPos = next.StartPos;
                menulogic.SourceText = menulogic.SourceText.Left(lngSubmitPos - 1) + strMenu + menulogic.SourceText.Right(menulogic.SourceText.Length - lngSubmitPos + 1);
                menulogic.SaveSource();
            }
            if (!blnLoaded) {
                menulogic.Unload();
            }
            // update preview
            RefreshTree(AGIResType.Logic, MenuLogic);

            // if any logic editor matches this resource
            // (remember to convert cp to uni!)
            for (int i = 0; i < LogicEditors.Count; i++) {
                if (LogicEditors[i].FormMode == LogicFormMode.Logic) {
                    if (LogicEditors[i].LogicNumber == MenuLogic) {
                        // update this one -
                        WinAGIFCTB fctb = LogicEditors[i].fctb;
                        Place start = fctb.Selection.Start;
                        Place end = fctb.Selection.End;
                        FastColoredTextBoxNS.Range vr = fctb.VisibleRange;
                        // if existing text has the menu editor header
                        if (fctb.Text.Contains(LoadResString(102), StringComparison.CurrentCulture)) {
                            // remove it
                            fctb.Text = fctb.Text.Replace(LoadResString(102), "");
                        }
                        // find the starting and ending pos of the menu
                        if (HasMenu(fctb.Text, ref lngMenuPos, ref lngSubmitPos)) {
                            // move start and end positions to line up with newlines
                            lngMenuPos = fctb.Text.LastIndexOf(Environment.NewLine, lngMenuPos + 1) + 2;
                            lngSubmitPos = fctb.Text.IndexOf(Environment.NewLine, lngSubmitPos);
                            // replace old menu with new menu

                            fctb.Text = fctb.Text.Left(lngMenuPos) + strMenu + fctb.Text.Right(fctb.Text.Length - lngSubmitPos);
                        }
                        else {
                            // add new menu to beginning of source (skipping any comments or blank lines)
                            fctb.Text = strMenu + fctb.Text;
                            lngMenuPos = fctb.TextLength;
                        }
                        fctb.Selection.Start = start;
                        fctb.Selection.End = end;
                        fctb.DoRangeVisible(vr);
                        break;
                    }
                }
            }
            // reset dirty flag
            IsChanged = false;
            // set caption
            Text = EditGame.GameID + " - AGI Menu Editor";
            // disable menu and toolbar button
            MDIMain.btnSaveResource.Enabled = false;
        }

        public void CopyMenuToClipboard() {
            if (AnyBlankControllers()) {
                if (MessageBox.Show(MDIMain,
                    "There are one or more blank controllers in this menu. " +
                    "Do you want to correct that before copying?",
                    "Blank Controllers",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes) {
                    return;
                }
            }
            Clipboard.Clear();
            Clipboard.SetText(CreateMenu(), TextDataFormat.UnicodeText);
        }

        private bool AnyBlankControllers() {
            foreach (TreeNode menu in tvwMenu.Nodes) {
                foreach (TreeNode item in menu.Nodes) {
                    if (((string)item.Tag).Length == 0) {
                        return true;
                    }
                }
            }
            // no blanks
            return false;
        }

        public void SaveAsDefault() {
            if (MessageBox.Show("Discard the current default menu, and replace it with this one?",
                "Save Default Menu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.No) {
                return;
            }

            // create the menu
            string strText = CreateMenu();

            // delete existing default menu file
            SafeFileDelete(ProgramDir + "default_menu.txt");

            try {
                // use a text writer to save the new file
                using FileStream fs = new(ProgramDir + "default_menu.txt", FileMode.Create, FileAccess.ReadWrite);
                fs.Write(Encoding.Default.GetBytes(strText));
                fs.Close();
            }
            catch (Exception ex) {
                ErrMsgBox(ex,
                    "Unable to save default menu due to error:",
                    ex.StackTrace,
                    "Default Menu Editor Error");
            }
        }

        public void ResetToDefault() {
            if (MessageBox.Show(MDIMain,
                "Are you sure you want to reset the menu structure to the default state?",
                "Reset to Default Menu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.No) {
                return;
            }
            DefaultMenu();
        }

        private bool ExtractMenu(string strText, bool Silent = false) {

            // extract menu info
            int lngMenuPos = 0, lngSubmitPos = 0;

            tvwMenu.Nodes.Clear();

            if (!HasMenu(strText, ref lngMenuPos, ref lngSubmitPos)) {
                // no menu found here;
                if (!Silent) {
                    // ask if user wants to use a default menu here
                    DialogResult retval = MessageBox.Show(MDIMain,
                        "This logic does not have a valid menu structure. Do " +
                        "you want to create a new menu here?",
                        "No Menu Found",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, 0, 0,
                        WinAGIHelp, "htm\\winagi\\editor_menu.htm");
                    if (retval == DialogResult.Yes) {
                        // use default
                        DefaultMenu();
                    }
                    return retval == DialogResult.Yes;
                }
            }
            // extract all the lines between start and stop
            string[] strLines = strText[lngMenuPos..lngSubmitPos].Split('\r');
            TreeNode tmpNode = null;
            for (int i = 0; i < strLines.Length; i++) {
                string strMenu, strController;
                string[] strItem;
                // remove any tab characters and preceding spaces
                strLines[i] = strLines[i].Replace('\t', ' ');
                strLines[i] = strLines[i].Replace("\n", "").Trim();
                // if line is a set menu:
                if (strLines[i].Left(9) == "set.menu(" || strLines[i].Left(9) == "set.menu ") {
                    if (strLines[i].Length > 10) {
                        // get menu text
                        strMenu = strLines[i].Right(strLines[i].Length - 9);
                        // check for trailing parenthesis or EOL marker (;)
                        if (strMenu[^1] == ';') {
                            strMenu = strMenu[0..^1];
                        }
                        if (strMenu[^1] == ')') {
                            strMenu = strMenu[0..^1].Trim();
                        }
                        // if not enclosed in quotes,
                        if (strMenu[0] != '\"' || strMenu[^1] != '\"') {
                            // must be a msg declaration or a local/global variable
                            strMenu = GetMsgText(strMenu, strText);
                        }
                        else {
                            // strip off quotes
                            strMenu = strMenu[1..^1];
                        }
                        tmpNode = tvwMenu.Nodes.Add(strMenu);
                    }
                    else {
                        // if line contains only the command, just ignore it
                    }
                    // check for submit.menu.item:
                }
                else if (strLines[i].Left(13) == "set.menu.item") {
                    if (strLines[i].Length > 15) {
                        // get menu text
                        strMenu = strLines[i].Right(strLines[i].Length - 14).Trim();
                        // check for trailing parenthesis or EOL marker(;)
                        if (strMenu[^1] == ';') {
                            strMenu = strMenu[0..^1];
                        }
                        if (strMenu[^1] == ')') {
                            strMenu = strMenu[0..^1];
                        }
                        // split into caption and controller
                        strItem = strMenu.Split(',');

                        // get caption
                        strItem[0] = strItem[0].Trim();
                        // if not enclosed in quotes,
                        if (strItem[0][0] != '\"' || strItem[0][^1] != '\"') {
                            // must be a msg declaration or a local/global variable
                            strItem[0] = GetMsgText(strItem[0], strText);
                        }
                        else {
                            // strip off quotes
                            strItem[0] = strItem[0][1..^1];
                        }
                        // get controller
                        if (strItem.Length == 2) {
                            strController = strItem[1].Trim();
                        }
                        else {
                            strController = "";
                        }
                        if (tmpNode is not null) {
                            tmpNode.Nodes.Add(strItem[0]).Tag = strController;
                            tmpNode.Nodes[^1].ForeColor = strController.Length > 0 ? Color.Black : Color.Red;
                        }
                    }
                    else {
                        // in this case, the line has no additional characters other than "set.menu.item("
                        // just ignore line and get next line
                    }
                }
            }

            // step through all menus
            int lngLength = 0;
            tmpNode = tvwMenu.Nodes[0];
            while (tmpNode is not null) {
                // ensure all menu items sized correctly
                ResizeMenuItems(tmpNode);
                // add to total menu length
                lngLength += tmpNode.Text.Length + 1;
                tmpNode = tmpNode.NextNode;
            }

            // if menu size is exceeded
            if (lngLength > 40 && !Silent) {
                MessageBox.Show(MDIMain,
                    "The total width of the menu exceeds the AGI maximum of 40. " +
                    "You should remove or reduce the length of menus to get " +
                    "under the 40 character limit.",
                    "Extract Menu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\menu.htm#displaymenus");
            }
            return tvwMenu.Nodes.Count > 0;
        }

        private string GetMsgText(string strMsgID, string strSource) {
            int lngMsg, lngPos;

            // if first time through, extract messages
            if (!blnGotMsgs) {
                string[] strLines = strSource.Split('\r');
                // locate first message
                string strMsg;
                // if section start found, extract out messages
                if (strSource.Contains("#message")) {
                    for (int i = 0; i < strLines.Length; i++) {
                        // eliminate tab characters and trim the string
                        strMsg = strLines[i].Replace('\t', ' ');
                        strMsg = strMsg.Replace("\n", "").Trim();
                        // use do loop to skip to next line if current line isn't a msg declaration
                        do {
                            lngPos = strMsg.IndexOf("#message");
                            if (lngPos != 1) {
                                break;
                            }
                            strMsg = strMsg.Right(strMsg.Length - 8).Trim();
                            // find next space to strip off the number
                            lngPos = strMsg.IndexOf(" ");
                            if (lngPos == 0) {
                                break;
                            }
                            lngMsg = strMsg.Left(lngPos).IntVal();
                            if (lngMsg <= 0 || lngMsg > 255) {
                                break;
                            }
                            strMsg = strMsg.Right(strMsg.Length - lngPos).Trim();
                            if (strMsg[0] != '\"' || strMsg[^1] != '\"') {
                                break;
                            }
                            strMsg = strMsg[1..^1];
                            strMessages[lngMsg] = strMsg;
                        } while (false);
                    }
                }
                // got messages!
                blnGotMsgs = true;
            }
            // if a msg variable
            if (strMsgID[0] == 'm') {
                lngMsg = strMsgID[1..].IntVal();
                if (lngMsg > 0 && lngMsg <= 255) {
                    return strMessages[lngMsg];
                }
            }
            // if not a msg variable check for local variable definition
            // TBD...

            // if not a local variable, check globals (if a game is loaded)
            string retval = strMsgID;
            if (EditGame is not null) {
                // check globals list
                retval = ArgFromToken(strMsgID);
            }
            if (retval != strMsgID) {
                return retval;
            }
            else {
                // no match found; return empty string
                return "";
            }
        }

        private void ResizeMenuItems(TreeNode menuNode) {
            bool blnKey;
            int lngMax = 0;

            // first, determine Max length; start with first node
            TreeNode tmpNode = menuNode.FirstNode;
            while (tmpNode is not null) {
                // reset the hotkey flag
                blnKey = false;
                SplitMenuItem(tmpNode.Text, out string menuText, out string hotkeyText);
                if (tvwMenu.SelectedNode != tmpNode) {
                    if (WinAGISettings.AutoAlignHotKey.Value) {
                        // if the item has a shortcut key
                        if (hotkeyText.Length > 0) {
                            blnKey = true;
                        }
                    }
                }
                if (blnKey) {
                    // don't use entire length; trim out the excess spaces
                    // between the menu item and its shortcut key definition
                    if (menuText.Length + hotkeyText.Length + 1 > lngMax) {
                        lngMax = menuText.Length + hotkeyText.Length + 1;
                    }
                }
                else {
                    // if menu item is NOT a spacer (all dashes)
                    if (tmpNode.Text != new string('-', tmpNode.Text.Length)) {
                        if (tvwMenu.SelectedNode == tmpNode) {
                            // ALWAYS use entire length
                            // (allows user to pad menu items manually)
                            int test = tmpNode.Text.Length;
                            if (test > lngMax) {
                                lngMax = test;
                            }
                        }
                        else {
                            // trim end if this is not current line
                            int test = tmpNode.Text.Trim().Length;
                            if (test > lngMax) {
                                lngMax = test;
                            }
                        }
                    }
                }
                tmpNode = tmpNode.NextNode;
            }
            // now run through them again, and pad any that
            // are not at Max
            tmpNode = menuNode.FirstNode;
            while (tmpNode is not null) {
                // if it is a separator
                if (tmpNode.Text == new string('-', tmpNode.Text.Length)) {
                    // pad it with dashes
                    tmpNode.Text = new string('-', lngMax);
                }
                else {
                    // pad it with spaces
                    tmpNode.Text = PadItem(tmpNode.Text, lngMax);
                }
                tmpNode = tmpNode.NextNode;
            }
        }

        private string PadItem(string ItemText, int MaxLen) {
            // if itemtext has a shortcut key definition at the end (e.g., <Ctrl X>)
            // insert spaces so shortcut remains right-aligned;
            // if no shortcut key, just add spaces to end

            SplitMenuItem(ItemText, out string menuText, out string hotkeyText);
            if (WinAGISettings.AutoAlignHotKey.Value && hotkeyText.Length > 0) {
                return menuText + "".PadRight(MaxLen - menuText.Length - hotkeyText.Length) + hotkeyText;
            }
            else {
                return ItemText.TrimEnd() + "".PadRight(MaxLen - ItemText.TrimEnd().Length);
            }
        }

        private void SplitMenuItem(string ItemText, out string MenuText, out string HotKeyText) {
            int lngPos;
            // splits a menu item into its menu text and its shortcut key text
            // if no shortcut key text, HotKeyText is empty
            // look for last '<' character
            lngPos = ItemText.LastIndexOf('<');
            if (lngPos > 0) {
                // if there is a matching '>' at the end of the string (not including spaces)
                if (ItemText.TrimEnd()[^1] == '>') {
                    MenuText = ItemText.Left(lngPos).TrimEnd();
                    HotKeyText = ItemText[lngPos..].Trim();
                    return;
                }
            }
            // if not found, return entire string as menu text
            MenuText = ItemText;
            HotKeyText = "";
        }

        private bool HasMenu(string LogicText, ref int StartPos, ref int EndPos) {

            // returns true if LogicText contains a valid menu structure

            // must include at least one each of:
            //   set.menu() command
            //   set.menu.item() command
            //   submit.menu() command

            if (!LogicText.Contains("set.menu") ||
                !LogicText.Contains("set.menu.item") ||
                !LogicText.Contains("submit.menu()")) {
                return false;
            }
            // look for set.menu:
            int lngMenuPos = FindTokenPos(LogicText, "set.menu");
            if (lngMenuPos == -1) {
                // not found
                return false;
            }
            // look for set.menu.item:
            int lngItemPos = FindTokenPos(LogicText, "set.menu.item");
            if (lngItemPos == -1) {
                // not found
                return false;
            }
            // if the menu item cmd occurs BEFORE the set.menu cmd
            if (lngItemPos < lngMenuPos) {
                // bad menu
                return false;
            }
            // look for submit.menu:
            int lngSubmitPos = FindTokenPos(LogicText, "submit.menu");
            if (lngSubmitPos == -1) {
                // not found
                return false;
            }
            // if submit menu cmd occurs BEFORE the menuitem cmd
            if (lngSubmitPos < lngItemPos) {
                // bad menu
                return false;
            }
            // if not exited, must contain a valid menu structure
            StartPos = lngMenuPos;
            EndPos = lngSubmitPos;
            return true;
        }

        private string CreateMenu() {
            // create menu structure based on treelist entries
            const string sMenu = "set.menu(\"";
            string sMenuEnd = "\");" + Environment.NewLine;
            const string sItem = "set.menu.item(\"";
            const string sComma = "\", ";
            string sItemEnd = ");" + Environment.NewLine;

            // add header
            string retval = LoadResString(102);

            foreach (TreeNode node in tvwMenu.Nodes) {
                // add menu text
                retval += sMenu + node.Text + sMenuEnd;
                // add menu items
                foreach (TreeNode itemNode in node.Nodes) {
                    retval += sItem + itemNode.Text + sComma + (string)itemNode.Tag + sItemEnd;
                }
            }
            // add submit
            retval += "submit.menu();" + Environment.NewLine;
            return retval;
        }

        private void DefaultMenu() {
            // creates a default menu

            // if default not loaded from file
            if (!LoadDefaultMenu()) {
                tvwMenu.Nodes.Clear();
                TreeNode node = tvwMenu.Nodes.Add("AGI");
                node.Nodes.Add("About     ").Tag = "c21";
                node.Nodes.Add("Help  <F1>").Tag = "c2";
                node.Nodes.Add("Debug Help").Tag = "c33";
                node.Expand();

                node = tvwMenu.Nodes.Add("File");
                node.Nodes.Add("Save     <F5>").Tag = "c3";
                node.Nodes.Add("Restore  <F7>").Tag = "c5";
                node.Nodes.Add("-------------").Tag = "c20";
                node.Nodes.Add("Restart  <F9>").Tag = "c7";
                node.Nodes.Add("Quit  <Alt-Z>").Tag = "c1";
                node.Expand();

                node = tvwMenu.Nodes.Add("Action");
                node.Nodes.Add("See Object <F4>").Tag = "c22";
                node.Nodes.Add("Inventory <Tab>").Tag = "c10";
                node.Expand();

                node = tvwMenu.Nodes.Add("Special");
                node.Nodes.Add("Sound On/Off      <F2>").Tag = "c16";
                node.Nodes.Add("Color/BW      <Ctrl R>").Tag = "c6";
                node.Nodes.Add("Clock On/Off      <F6>").Tag = "c12";
                node.Nodes.Add("Joystick      <Ctrl J>").Tag = "c15";
                node.Nodes.Add("Pause            <Esc>").Tag = "c18";
                node.Expand();

                node = tvwMenu.Nodes.Add("Speed");
                node.Nodes.Add("Normal ").Tag = "c24";
                node.Nodes.Add("Slow   ").Tag = "c25";
                node.Nodes.Add("Fast   ").Tag = "c23";
                node.Nodes.Add("Fastest").Tag = "c28";
                node.Expand();

                node = tvwMenu.Nodes.Add("Debug");
                node.Nodes.Add("Ego Info   <Alt-E>").Tag = "c29";
                node.Nodes.Add("Pri Screen <Alt-P>").Tag = "c13";
                node.Nodes.Add("Memory     <Alt-M>").Tag = "c11";
                node.Nodes.Add("Obj Info   <Alt-I>").Tag = "c36";
                node.Nodes.Add("Coords     <Alt-X>").Tag = "c31";
                node.Nodes.Add("Get All           ").Tag = "c32";
                node.Expand();
            }
            // select the first menu
            tvwMenu.SelectedNode = tvwMenu.Nodes[0];
            MarkAsChanged();
        }

        private bool LoadDefaultMenu() {

            // attempts to load a default menu from text file
            // if no default menu file,
            if (!File.Exists(ProgramDir + "default_menu.txt")) {
                return false;
            }
            try {
                using StreamReader sr = new(new FileStream(ProgramDir + "default_menu.txt", FileMode.Open, FileAccess.Read));
                sr.Close();
                // extract menu; return success if extract works
                return ExtractMenu(sr.ReadToEnd(), true);
            }
            catch {
                return false;
            }
        }

        private void DefaultBackground() {
            bool blnLoaded;
            if (EditGame is not null) {
                if (EditGame.Pictures.Count != 0) {
                    // get first valid picture
                    for (int i = 0; i < 256; i++) {
                        if (EditGame.Pictures.Contains(i)) {
                            blnLoaded = EditGame.Pictures[i].Loaded;
                            if (!blnLoaded) {
                                EditGame.Pictures[i].Load();
                            }
                            if (EditGame.Pictures[i].Error == ResourceErrorType.NoError ||
                                EditGame.Pictures[i].Error == ResourceErrorType.FileIsReadonly) {
                                BkgdPicNum = i;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool ChooseBackground() {
            if (EditGame is null) {
                return false;
            }

            // choose a picture for background
            frmGetResourceNum frm = new(GetRes.MenuBkgd, AGIResType.Picture);
            if (BkgdPicNum != -1) {
                frm.OldResNum = (byte)BkgdPicNum;
            }
            if (frm.ShowDialog() == DialogResult.OK) {
                BkgdPicNum = frm.NewResNum;
                return true;
            }
            return false;
        }

        private void DrawBackground() {
            // use a default screen
            picBackground.Size = new Size(320 * PicScale, 200 * PicScale);
            Bitmap bmp = new(320 * PicScale, 200 * PicScale);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            if (BkgdPicNum != -1) {
                bool blnLoaded = EditGame.Pictures[BkgdPicNum].Loaded;
                if (!blnLoaded) {
                    EditGame.Pictures[BkgdPicNum].Load();
                }
                g.DrawImage(EditGame.Pictures[BkgdPicNum].VisualBMP, 0, 8 * PicScale, 320 * PicScale, 168 * PicScale);
                if (!blnLoaded) {
                    EditGame.Pictures[BkgdPicNum].Unload();
                }
            }
            else {
                // write sample picture message
                // adjust offset by width of one character
                for (int i = 0; i < "sample AGI display".Length; i++) {
                    int charval = "sample AGI display"[i];
                    g.DrawImage(invchargrid, TargetCharPos(11, 11 + i), SourceCharPos(charval), GraphicsUnit.Pixel);
                }
            }
            g.DrawImage(invchargrid, TargetCharPos(22, 0), SourceCharPos(62), GraphicsUnit.Pixel);
            picBackground.Image = bmp;
        }

        private static Rectangle SourceCharPos(int charval) {
            return new(16 * (charval % 16), 16 * (charval / 16), 16, 16);
        }

        private Rectangle TargetCharPos(int row, int col) {
            return new(col * 8 * PicScale, row * 8 * PicScale, 8 * PicScale, 8 * PicScale);
        }

        private void DrawMenu(Graphics g, int menu, int item) {
            // clear top line
            SolidBrush b = new(Color.White);
            g.FillRectangle(b, 0, 0, picBackground.Width, 8 * PicScale);
            int menucol = 0;
            foreach (TreeNode menunode in tvwMenu.Nodes) {
                for (int i = 0; i < menunode.Text.Length; i++) {
                    g.DrawImage(menunode.Index == menu ? invchargrid : chargrid,
                        TargetCharPos(0, ++menucol),
                        SourceCharPos(menunode.Text[i]),
                        GraphicsUnit.Pixel);
                    if (menunode.Index == menu) {
                        DrawSubMenu(g, menu, item);
                    }
                }
                menucol++;
            }
        }

        private void DrawSubMenu(Graphics g, int menu, int item) {
            // if no items, exit
            if (tvwMenu.Nodes[menu].Nodes.Count == 0) {
                return;
            }

            SolidBrush bw = new(Color.White);
            SolidBrush bb = new(Color.Black);
            int left = MenuPos(menu) - 1;
            int top = 1;
            int width = 2 + tvwMenu.Nodes[menu].Nodes[0].Text.Length;
            int height = 2 + tvwMenu.Nodes[menu].Nodes.Count;
            // right align menu if it does not fit
            if (left + width > 40) {
                left = 40 - width;
            }
            // draw menu area
            g.FillRectangle(bw, 8 * left * PicScale, 8 * PicScale, 8 * PicScale * width, 8 * PicScale * height);
            // draw menu border
            g.FillRectangle(bb, ((8 * left) + 2) * PicScale, 9 * PicScale,
                ((8 * width) - 4 - 1) * PicScale, PicScale);
            g.FillRectangle(bb, ((8 * left) + 2) * PicScale, ((8 * height) + 6) * PicScale,
                ((8 * width) - 4 - 1) * PicScale, PicScale);
            g.FillRectangle(bb, ((8 * left) + 2) * PicScale, 9 * PicScale,
                2 * PicScale, ((8 * height) - 2) * PicScale);
            g.FillRectangle(bb, (8 * (left + width) - 4) * PicScale, 9 * PicScale,
                2 * PicScale, ((8 * height) - 2) * PicScale);

            // now add the menu item captions
            left++;
            top++;
            int itemcol, itemrow = top;
            foreach (TreeNode node in tvwMenu.Nodes[menu].Nodes) {
                itemcol = left;
                for (int i = 0; i < node.Text.Length; i++) {
                    g.DrawImage(node.Index == item ? invchargrid : chargrid,
                        TargetCharPos(itemrow, itemcol++),
                        SourceCharPos(node.Text[i]),
                        GraphicsUnit.Pixel);
                }
                itemrow++;
            }
        }

        private void MouseOverMenu(Point pos, out int clickMenu, out int clickItem) {
            //  sets clickMenu or clickItem if the cursor is over
            //  a menu, or menu item

            // assume nothing
            clickMenu = -1;
            clickItem = -1;

            // convert mouse position into row/col
            int row = pos.Y / 8 / PicScale;
            int col = pos.X / 8 / PicScale;
            int start, end;

            // if on top row:
            if (row == 0) {
                foreach (TreeNode node in tvwMenu.Nodes) {
                    start = MenuPos(node.Index);
                    end = start + node.Text.Length;
                    if (col >= start && col < end) {
                        clickMenu = node.Index;
                        return;
                    }
                }
            }

            // check for submenu (but only is it's expanded)
            TreeNode menunode;
            if (tvwMenu.SelectedNode.Level == 0) {
                menunode = tvwMenu.SelectedNode;
            }
            else {
                menunode = tvwMenu.SelectedNode.Parent;
            }
            start = MenuPos(menunode.Index);
            end = start + menunode.Nodes[0].Text.Length;
            // adjust if menu is right-aligned
            if (end > 39) {
                start = 39 - menunode.Nodes[0].Text.Length;
                end = 39;
            }
            if (col >= start && col < end) {
                if (row >= 2 && row < 2 + menunode.Nodes.Count) {
                    clickMenu = menunode.Index;
                    clickItem = row - 2;
                    return;
                }
            }
        }

        private bool UpdateCaption(string NewCaption) {
            string oldCaption = tvwMenu.SelectedNode.Text;
            if (tvwMenu.SelectedNode.Level == 0) {
                // if making menu longer,
                if (NewCaption.Length > oldCaption.Length) {
                    int menulength = 0;
                    foreach (TreeNode node in tvwMenu.Nodes) {
                        if (node == tvwMenu.SelectedNode) {
                            menulength += NewCaption.Length + 1;
                        }
                        else {
                            menulength += node.Text.Length + 1;
                        }
                    }
                    if (menulength > 40) {
                        MessageBox.Show(MDIMain,
                            "Menu text is too long for all menus to fit on screen. Shorten or " +
                            "delete one of the other menus to make additional space available " +
                            "for this menu.",
                            "Menu Caption Too Long",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\agi\\menu.htm#displaymenus");
                        return false;
                    }
                }
                tvwMenu.SelectedNode.Text = NewCaption;
            }
            else {
                // menu item
                if (NewCaption.Length > 38) {
                    MessageBox.Show(MDIMain,
                        "Menu item text is too long to fit on screen.",
                        "Menu Item Text Too Long",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp, "htm\\agi\\menu.htm#displaymenus");
                    return false;
                }
                tvwMenu.SelectedNode.Text = NewCaption;
                // adjust menu items
                ResizeMenuItems(tvwMenu.SelectedNode.Parent);
            }
            picBackground.Invalidate();
            return true;
        }

        private bool UpdateController(string NewCtrl) {
            NewCtrl = NewCtrl.Trim();
            // if blank, confirm
            if (NewCtrl.Length == 0) {
                if (MessageBox.Show(MDIMain,
                    "Do you want to leave the controller value blank?",
                    "Blank Controller Assignment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No) {
                    // cancel validation
                    return false;
                }
            }
            if ((string)tvwMenu.SelectedNode.Tag != NewCtrl) {
                // validate it
                int i = tvwMenu.SelectedNode.Index;
                int menu = tvwMenu.SelectedNode.Parent.Index;
                if (!CheckController(ref i, ref menu, NewCtrl)) {
                    // error
                    if (MessageBox.Show(MDIMain,
                        "'" + NewCtrl + "' is already assigned to a different menu item (" +
                        tvwMenu.Nodes[i].Text + " | " + tvwMenu.Nodes[i].Nodes[menu].Text + "). " +
                        "Are you sure you want to create a duplicate assignment?",
                        "Duplicate Controller Assignment",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.No) {
                        // cancel validation
                        return false;
                    }
                }
                else {
                    // if a controller argument marker, validate it
                    if (NewCtrl.Length > 0 && NewCtrl[0] == 'c') {
                        if (NewCtrl[1..].IsNumeric()) {
                            if (NewCtrl[1..].IntVal() < 0 || NewCtrl[1..].IntVal() > 24) {
                                if (MessageBox.Show(MDIMain,
                                    "'" + NewCtrl + "' is not a valid controller marker. " +
                                    "Are you sure you want to use this controller value?",
                                    "Invalid Controller Assignment",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No) {
                                    // cancel validation
                                    return false;
                                }
                            }
                        }
                    }
                }
                // change it
                tvwMenu.SelectedNode.Tag = NewCtrl;
                tvwMenu.SelectedNode.ForeColor = NewCtrl.Length > 0 ? Color.Black : Color.Red;
                MarkAsChanged();
            }
            return true;
        }

        private bool CheckController(ref int itemindex, ref int menuindex, string controller) {
            // checks if this controller is in use by an existing menu item
            // and warns user

            // ignore blanks
            if (controller.Length == 0) {
                return true;
            }

            // step through all nodes in the menu structure
            for (int i = 0; i < tvwMenu.Nodes.Count; i++) {
                for (int j = 0; j < tvwMenu.Nodes[i].Nodes.Count; j++) {
                    if (i != itemindex || j != menuindex) {
                        if ((string)tvwMenu.Nodes[i].Nodes[j].Tag == controller) {
                            // match found
                            itemindex = i;
                            menuindex = j;
                            return false;
                        }
                    }
                }
            }
            // no duplicate found; return success
            return true;
        }

        internal void ShowHelp() {
            string topic = "htm\\winagi\\editor_menu.htm";

            // TODO: add context sensitive help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }

        private bool AskClose() {
            DialogResult rtn;
            if (IsChanged) {
                // ask if should save first
                if (EditGame is not null && MenuLogic != -1) {
                    rtn = MessageBox.Show(MDIMain,
                        "Do you want to save this menu structure?",
                        "Menu Editor",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
                else {
                    rtn = MessageBox.Show(MDIMain,
                        "Do you want to copy this menu structure to the clipboard before closing?",
                        "Menu Editor",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
                switch (rtn) {
                case DialogResult.Yes:
                    // save
                    if (EditGame is not null && MenuLogic != -1 && IsChanged) {
                        UpdateSourceLogic();
                    }
                    else {
                        CopyMenuToClipboard();
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
                }
            }
            return true;
        }

        private void MarkAsChanged() {

            if (!IsChanged) {
                // set dirty flag
                IsChanged = true;

                // enable menu and toolbar button
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
        }
        #endregion
    }
}
