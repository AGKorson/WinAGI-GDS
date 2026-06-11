using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.WinAGIFCTB;

namespace WinAGI.Editor {
    public partial class frmMenuEdit : Form {

        #region Structs
        struct MenuData {
            public string Controller = "";
            public string Condition = "";
            public MenuData() {
            }
        }
        #endregion

        #region Fields
        internal bool IsChanged;
        internal int PicScale;
        private readonly Bitmap chargrid;
        private readonly Bitmap invchargrid;
        private int BkgdPicNum = -1;
        internal int MenuLogic = -1;
        private readonly string[] messages = new string[256];
        private readonly List<KeyValuePair<string, string>> defines = [];
        private bool gotMsgs = false;
        internal bool Canceled = false;
        private int clickMenu, clickItem;
        private readonly DataGridViewCell captionLabel;
        private readonly DataGridViewCell captionValue;
        private readonly DataGridViewCell ctrlLabel;
        private readonly DataGridViewCell ctrlValue;
        private readonly DataGridViewCell condLabel;
        private readonly DataGridViewCell condValue;
        private readonly DataGridViewCellStyle defaultStyle;
        private readonly DataGridViewCellStyle highlightStyle;
        private TextBox EditTextBox = null;
        private bool EditingCaption = false;
        private bool WideScreen = EditGame is not null && EditGame.PowerPack;
        private int MaxW = EditGame is not null && EditGame.PowerPack ? 80 : 40;
        private int CharW = EditGame is not null && EditGame.PowerPack ? 4 : 8;
        private readonly int CodePage = EditGame is not null && EditGame.PowerPack ? EditGame.CodePage : 437;
        // ToolStrip Items
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        #endregion

        #region Constructors
        public frmMenuEdit(int logicnum) {
            InitializeComponent();
            InitToolStrip();
            MdiParent = MDIMain;
            // mark editor as inuse
            MEInUse = true;
            MenuLogic = logicnum;
            // default codepage
            int codepage = 437;

            // set width so test menu will fit to right of treeview list
            // choose a scale such that window is approximately 1/2 of screen
            int borders = Width - ClientRectangle.Width;
            PicScale = Screen.PrimaryScreen.WorkingArea.Width / (320 + borders) / 2;
            // never less than one
            if (PicScale < 1) {
                PicScale = 1;
            }
            if (MenuLogic >= 0) {
                // load a logic (means EditGame is not null)
                codepage = EditGame.CodePage;
                // attempt to extract the menu
                if (!ExtractMenu(EditGame.Logics[MenuLogic].SourceText)) {
                    MDIMain.MsgBoxWithHelp(
                        "Menu not found in logic " + MenuLogic + ". Default menu will be loaded.",
                        "No Menu Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        "htm\\winagi\\editor_menu.htm");
                    DefaultMenu();
                }
            }
            else {
                // either no EditGame, or choosing default
                DefaultMenu();
            }
            // verify menu is not too long
            if (MenuBarWidth() > MaxW) {
                MDIMain.MsgBoxWithHelp(
                    $"The total width of the menu exceeds the AGI maximum of {MaxW}. " +
                    "You should remove or reduce the length of menus to get " +
                    $"under the {MaxW} character limit.",
                    "Menu Too Long",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    "htm\\agi\\menu.htm#displaymenus");
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

            // set up datagrid as a property grid
            dgProps.Rows.Add("Caption", "cap");
            dgProps.Rows.Add("Controller", "ctrl");
            dgProps.Rows.Add("Condition", "cond");
            captionLabel = dgProps.Rows[0].Cells[0];
            captionValue = dgProps.Rows[0].Cells[1];
            ctrlLabel = dgProps.Rows[1].Cells[0];
            ctrlValue = dgProps.Rows[1].Cells[1];
            condLabel = dgProps.Rows[2].Cells[0];
            condValue = dgProps.Rows[2].Cells[1];
            defaultStyle = dgProps.Columns[0].DefaultCellStyle;
            highlightStyle = defaultStyle.Clone();
            highlightStyle.SelectionBackColor = highlightStyle.BackColor = SystemColors.Highlight;
            highlightStyle.SelectionForeColor = highlightStyle.ForeColor = SystemColors.HighlightText;

            // select first menu item
            tvwMenu.SelectedNode = tvwMenu.Nodes[0];
            tvwMenu.ExpandAll();
        }
        #endregion

        #region Event Handlers
        #region Form Events
        private void frmMenuEdit_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible) {
                FindingForm.Visible = false;
            }
            if (MDIMain.infoGridScope == InfoGridScope.SelectedResource) {
                MDIMain.RefreshInfoGrid();
            }
        }

        private void frmMenuEdit_Load(object sender, EventArgs e) {
            // set propertygrid height/width
            dgProps.Height = 3 * dgProps.Rows[0].Height + 3;
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

        private void frmMenuEdit_HelpRequested(object sender, HelpEventArgs hlpevent) {
            ShowHelp();
            hlpevent.Handled = true;
        }
        #endregion

        #region Menu Events
        internal void SetResourceMenu() {
            // end any editing that may be occuring
            if (EditTextBox is not null && EditTextBox.Visible) {
                EditTextBox.Hide();
                dgProps.CancelEdit();
                // cancel alone doesn't work (the cell remains in edit mode)
                // but calling EndEdit immediately after seems to work
                dgProps.EndEdit();
            }
            MDIMain.mnuRSep2.Visible = false;
            MDIMain.mnuRSep3.Visible = true;
            mnuUpdateLogic.Enabled = EditGame is not null && MenuLogic >= 0 && IsChanged;
            mnuSaveAsDefault.Enabled = true;
            mnuBackground.Enabled = EditGame is not null && EditGame.Pictures.Count > 0;
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
            // end any editing that may be occuring
            if (EditTextBox is not null && EditTextBox.Visible) {
                EditTextBox.Hide();
                dgProps.CancelEdit();
                // cancel alone doesn't work (the cell remains in edit mode)
                // but calling EndEdit immediately after seems to work
                dgProps.EndEdit();
            }
            SetEditMenu();
            mnuEdit.DropDownItems.AddRange([
                mnuMoveUp,
                mnuMoveDown,
                mnuESep1,
                mnuDelete,
                mnuInsert,
                mnuESep1,
                mnuCopy,
                mnuReset,
                mnuESep2,
                mnuHotKeys,
                mnuChangeScreenSize]);
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            contextMenuStrip1.Items.AddRange([
                mnuMoveUp,
                mnuMoveDown,
                mnuESep0,
                mnuDelete,
                mnuInsert,
                mnuESep1,
                mnuCopy,
                mnuReset,
                mnuESep2,
                mnuHotKeys,
                mnuChangeScreenSize]);
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
                mnuInsert.Enabled = MenuBarWidth() < MaxW - 1;
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
            mnuHotKeys.Checked = WinAGISettings.AutoAlignHotKey.Value;
            mnuChangeScreenSize.Visible = EditGame is not null && EditGame.PowerPack;

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
                // rare, but if too many items and line 22  is moved up, adjust colors
                if (tvwMenu.SelectedNode.Index == 22) {
                    tvwMenu.SelectedNode.ForeColor = ((MenuData)tvwMenu.SelectedNode.Tag).Controller.Length > 0 ? Color.Black : Color.Red;
                    tvwMenu.SelectedNode.PrevNode.ForeColor = Color.Red;
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
                // rare, but if too many items and line 21  is moved down, adjust colors
                if (tvwMenu.SelectedNode.Index == 21) {
                    tvwMenu.SelectedNode.NextNode.ForeColor = ((MenuData)tvwMenu.SelectedNode.Tag).Controller.Length > 0 ? Color.Black : Color.Red;
                    tvwMenu.SelectedNode.ForeColor = Color.Red;
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
            int count = tvwMenu.SelectedNode.Parent.Nodes.Count;
            tvwMenu.Nodes.Remove(tvwMenu.SelectedNode);
            if (count >= 23) {
                // rare, but if too many items before deleting,
                // adjust color of item 21 (in case a  invalid line
                // got bumped up)
                tmpNode.Parent.Nodes[21].ForeColor = ((MenuData)tmpNode.Parent.Nodes[21].Tag).Controller.Length > 0 ? Color.Black : Color.Red;
            }
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
                    if (menulength > MaxW - 2) {
                        // not enough room
                        MDIMain.MsgBoxWithHelp(
                            "There is not enough room to add another menu. Remove " +
                             "or shorten some of the existing menus, and try again.",
                             "Insert Menu",
                             MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                             "htm\\agi\\menu.htm#displaymenus");
                        return;
                    }
                    else if (menulength > MaxW - 5) {
                        // room for four or less characters
                        MDIMain.MsgBoxWithHelp(
                            "There is only room for a menu that is " + (MaxW - 1 - menulength) +
                            " characters.",
                            "Menu Bar Size Limit Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning,
                            "htm\\agi\\menu.htm#displaymenus");
                    }

                    // add an menu to end
                    tvwMenu.Nodes.Add("").Expand();
                    tvwMenu.Nodes[^1].Tag = new MenuData();
                    // add a default item
                    tvwMenu.Nodes[^1].Nodes.Add("item").Tag = new MenuData();
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
                    if (tvwMenu.SelectedNode.Parent.Nodes.Count >= 22) {
                        // not enough room
                        MDIMain.MsgBoxWithHelp(
                            "This menu already has the maximium number of items allowed.",
                            "Insert Menu Item",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\agi\\menu.htm#displaymenus");
                        return;
                    }
                    // add an menu item to end
                    tvwMenu.SelectedNode.Parent.Nodes.Add("").Tag = new MenuData();
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

        private void mnuChangeScreenSize_Click(object sender, EventArgs e) {
            //swap screen size if powerpack is active

            if (EditGame is null || !EditGame.PowerPack) {
                return;
            }
            WideScreen = !WideScreen;
            MaxW = WideScreen ? 80 : 40;
            CharW = WideScreen ? 4 : 8;
            // redraw menu to update character widths
            picBackground.Invalidate();
        }
        #endregion

        #region EditTextBox Menu Events
        private void cmCel_Opening(object sender, CancelEventArgs e) {
            mnuCelUndo.Enabled = EditTextBox.CanUndo;
            mnuCelCut.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCopy.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelPaste.Enabled = Clipboard.ContainsText();
            mnuCelDelete.Enabled = EditTextBox.SelectionLength > 0;
            mnuCelCharMap.Visible = dgProps.CurrentCell.RowIndex == 0 &&
                EditGame is not null && EditGame.PowerPack;
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
            using (frmCharPicker CharPicker = EditGame is not null ?
                new(EditGame.CodePage) : new(WinAGISettings.DefCP.Value)) {
                CharPicker.ShowDialog(MDIMain);
                if (CharPicker.DialogResult == DialogResult.OK) {
                    if (CharPicker.InsertString.Length > 0) {
                        EditTextBox.SelectedText = CharPicker.InsertString;
                    }
                }
            }
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
                // caption and condition always set
                captionValue.Value = tvwMenu.SelectedNode.Text;
                condValue.Value = ((MenuData)tvwMenu.SelectedNode.Tag).Condition;
                switch (tvwMenu.SelectedNode.Level) {
                case 0:
                    // menu - hide controller
                    dgProps.Rows[1].Visible = false;
                    break;
                case 1:
                    // menu item
                    ctrlValue.Value = ((MenuData)tvwMenu.SelectedNode.Tag).Controller;
                    dgProps.Rows[1].Visible = true;
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

        private void tvwMenu_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
            case Keys.Left:
                // move one menu to the left if possible
                if (tvwMenu.SelectedNode.Level == 1) {
                    // move to parent node
                    tvwMenu.SelectedNode = tvwMenu.SelectedNode.Parent;
                }
                else {
                    if (tvwMenu.SelectedNode.Index > 0) {
                        tvwMenu.SelectedNode = tvwMenu.SelectedNode.PrevNode;
                    }
                }
                break;
            case Keys.Right:
                // move one menu to the right if possible
                if (tvwMenu.SelectedNode.Level == 1) {
                    // move to parent node
                    tvwMenu.SelectedNode = tvwMenu.SelectedNode.Parent;
                }
                else {
                    if (tvwMenu.SelectedNode.Index < tvwMenu.Nodes.Count - 1) {
                        tvwMenu.SelectedNode = tvwMenu.SelectedNode.NextNode;
                    }
                }
                break;
            case Keys.Up:
                // move to previous item, if possible
                if (tvwMenu.SelectedNode.Level == 1) {
                    if (tvwMenu.SelectedNode.Index > 0) {
                        tvwMenu.SelectedNode = tvwMenu.SelectedNode.PrevNode;
                    }
                }
                break;
            case Keys.Down:
                // move to next item, if possible
                if (tvwMenu.SelectedNode.Level == 0) {
                    if (tvwMenu.SelectedNode.Nodes.Count > 0) {
                        tvwMenu.SelectedNode = tvwMenu.SelectedNode.FirstNode;
                    }
                }
                else {
                    if (tvwMenu.SelectedNode.Index < tvwMenu.SelectedNode.Parent.Nodes.Count - 1) {
                        tvwMenu.SelectedNode = tvwMenu.SelectedNode.NextNode;
                    }
                }
                break;
            }
            // disable all other keyhandling
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void picBackground_MouseDown(object sender, MouseEventArgs e) {
            // end any editing that may be occuring
            if (EditTextBox is not null && EditTextBox.Visible) {
                dgProps.CancelEdit();
                // cancel alone doesn't work (the cell remains in edit mode)
                // but calling EndEdit immediately after seems to work
                dgProps.EndEdit();
                EditTextBox.Hide();
            }

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
                    //EditTextBox.Validating += EditTextBox_Validating;
                    EditTextBox.KeyDown += EditTextBox_KeyDown;
                }
                EditTextBox.Multiline = true;
                EditTextBox.AcceptsReturn = true;
                EditTextBox.AcceptsTab = true;
            }
        }

        private void dgProps_CellClick(object sender, DataGridViewCellEventArgs e) {
            // highlight correct label
            switch (e.RowIndex) {
            case 0:
                captionLabel.Style = highlightStyle;
                ctrlLabel.Style = defaultStyle;
                condLabel.Style = defaultStyle;
                break;
            case 1:
                captionLabel.Style = defaultStyle;
                ctrlLabel.Style = highlightStyle;
                condLabel.Style = defaultStyle;
                break;
            case 2:
                captionLabel.Style = defaultStyle;
                ctrlLabel.Style = defaultStyle;
                condLabel.Style = highlightStyle;
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
                switch (e.RowIndex) {
                case 0:
                    // caption
                    if (!UpdateCaption((string)e.FormattedValue)) {
                        // cancel the edit
                        e.Cancel = true;
                    }
                    break;
                case 2:
                    // condition
                    UpdateCondition((string)e.FormattedValue);
                    break;
                }
                break;
            case 1:
                switch (e.RowIndex) {
                case 0:
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
                    break;
                case 1:
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
                    break;
                case 2:
                    // condition
                    UpdateCondition((string)e.FormattedValue);
                    break;
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
                switch (dgProps.CurrentCell.RowIndex) {
                case 0:
                    dgProps.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = tvwMenu.SelectedNode.Text;
                    break;
                case 1:
                    dgProps.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ((MenuData)tvwMenu.SelectedNode.Tag).Controller;
                    break;
                case 2:
                    dgProps.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ((MenuData)tvwMenu.SelectedNode.Tag).Condition;
                    break;
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
                btnInsert.Enabled = MenuBarWidth() < MaxW - 1;
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

        /// <summary>
        /// Calculates the total width of the menu bar.
        /// </summary>
        /// <returns></returns>
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

            bool loaded;
            int menuPos = 0, submitPos = 0;

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
            string menutext = CreateMenu();

            Debug.Assert(MenuLogic != -1);
            if (!EditGame.Logics.Contains(MenuLogic)) {
                // logic not found; may have been deleted from game
                MessageBox.Show(MDIMain,
                    "The logic that this menu came from is no longer part of " +
                     "your game. You can copy the menu structure to the " +
                     "clipboard and  manually paste it into a different logic.",
                    "Save Menu Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                MenuLogic = -1;
                return;
            }
            // find the logic
            Logic menulogic = EditGame.Logics[MenuLogic];
            loaded = menulogic.Loaded;
            if (!loaded) {
                menulogic.Load();
            }
            // if existing text has the menu editor header
            if (menulogic.SourceText.Contains(EditorResourceByNum(102), StringComparison.CurrentCulture)) {
                // remove it
                menulogic.SourceText = menulogic.SourceText.Replace(EditorResourceByNum(102), "");
            }
            // find the starting and ending pos of the menu
            if (HasMenu(menulogic.SourceText, ref menuPos, ref submitPos)) {
                // move start and end positions to line up with newlines
                menuPos = menulogic.SourceText.LastIndexOf(Environment.NewLine, menuPos + 1) + 2;
                submitPos = menulogic.SourceText.IndexOf(Environment.NewLine, submitPos) + 2;

                // add new menu in place of existing source
                menulogic.SourceText = menulogic.SourceText.Left(menuPos) + menutext + menulogic.SourceText.Right(menulogic.SourceText.Length - submitPos);
                menulogic.SaveSource();
            }
            else {
                // add new menu to beginning of source (skipping any comments or blank lines)
                menuPos = 0;
                submitPos = 0;
                AGIToken next;
                do {
                    next = NextToken(menulogic.SourceText, submitPos, true);
                    if (next.Type != AGITokenType.LineBreak && next.Type != AGITokenType.Comment) {
                        break;
                    }
                    submitPos = next.EndPos;
                } while (next.Type != AGITokenType.None);
                // submitPos is where to put the thing
                menulogic.SourceText = menulogic.SourceText.Left(submitPos) + menutext + menulogic.SourceText.Right(menulogic.SourceText.Length - submitPos + 1);
                menulogic.SaveSource();
            }
            if (!loaded) {
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
                        if (fctb.Text.Contains(EditorResourceByNum(102), StringComparison.CurrentCulture)) {
                            // remove it
                            fctb.Text = fctb.Text.Replace(EditorResourceByNum(102), "");
                        }
                        // find the starting and ending pos of the menu
                        if (HasMenu(fctb.Text, ref menuPos, ref submitPos)) {
                            // move start and end positions to line up with newlines
                            menuPos = fctb.Text.LastIndexOf(Environment.NewLine, menuPos + 1) + 2;
                            submitPos = fctb.Text.IndexOf(Environment.NewLine, submitPos);
                            // replace old menu with new menu

                            fctb.Text = fctb.Text.Left(menuPos) + menutext + fctb.Text.Right(fctb.Text.Length - submitPos);
                        }
                        else {
                            // add new menu to beginning of source (skipping any comments or blank lines)
                            fctb.Text = menutext + fctb.Text;
                            menuPos = fctb.TextLength;
                        }
                        fctb.Selection.Start = start;
                        fctb.Selection.End = end;
                        fctb.DoRangeVisible(vr);
                        break;
                    }
                }
            }
            // reset changed flag
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
                    if (((MenuData)item.Tag).Controller.Length == 0) {
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
            string menutext = CreateMenu();

            // delete existing default menu file
            SafeFileDelete(Path.Combine(AppDataDir, "default_menu.txt"));

            try {
                // use a text writer to save the new file
                using FileStream fs = new(Path.Combine(AppDataDir, "default_menu.txt"), FileMode.Create, FileAccess.ReadWrite);
                fs.Write(Encoding.Default.GetBytes(menutext));
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

        private bool ExtractMenu(string source) {
            int menuPos = 0, submitPos = 0;
            bool counterr = false;

            tvwMenu.Nodes.Clear();

            if (!HasMenu(source, ref menuPos, ref submitPos)) {
                return false;
            }

            string caption = "";
            string condition = "", menuCondition = "", itemCondition = "";
            TreeNode tmpNode = null;

            // read tokens starting at msgpos until at or past submitpos
            AGIToken token = TokenFromPos(source, menuPos);
            while (token.StartPos < submitPos) {
                switch (token.Type) {
                case AGITokenType.Symbol:
                    if (token.Text == "}") {
                        if (menuCondition.Length > 0) {
                            // reset condition
                            menuCondition = "";
                        }
                        if (itemCondition.Length > 0) {
                            // reset condition
                            itemCondition = "";
                        }
                        condition = "";
                    }
                    break;
                case AGITokenType.Identifier:
                    switch (token.Text) {
                    case "set.menu":
                        // should not have a item condition
                        Debug.Assert(itemCondition == "");
                        itemCondition = "";
                        if (condition.Length > 0) {
                            menuCondition = condition;
                            condition = "";
                        }
                        // get menu text from next token
                        token = NextToken(source, token);
                        // skip '('
                        if (token.Text == "(") {
                            token = NextToken(source, token);
                        }
                        // get menu text from this token
                        if (token.Type == AGITokenType.String || token.Type == AGITokenType.Identifier) {
                            caption = token.Text;
                            if (token.Type == AGITokenType.Identifier) {
                                caption = GetMsgText(caption, source);
                            }
                            if (caption[0] == '\"') {
                                caption = caption[1..^1];
                            }
                        }
                        if (caption.Length > 0) {
                            // add menu to tree
                            tmpNode = tvwMenu.Nodes.Add(caption);
                            tmpNode.Tag = new MenuData() { Condition = menuCondition };
                        }
                        break;
                    case "set.menu.item":
                        caption = "";
                        if (condition.Length > 0) {
                            itemCondition = condition;
                            condition = "";
                        }
                        // skip '('
                        token = NextToken(source, token);
                        if (token.Text == "(") {
                            token = NextToken(source, token);
                        }
                        // get menu item text from this token
                        if (token.Type == AGITokenType.String || token.Type == AGITokenType.Identifier) {
                            caption = token.Text;
                            if (token.Type == AGITokenType.Identifier) {
                                caption = GetMsgText(token.Text, source);
                            }
                            if (caption[0] == '\"') {
                                caption = token.Text[1..^1];
                            }
                        }
                        token = NextToken(source, token);
                        // skip comma
                        if (token.Text == ",") {
                            token = NextToken(source, token);
                        }
                        string controller = "";
                        // controller is next; should be an identifier
                        if (token.Type == AGITokenType.Identifier) {
                            controller = token.Text;
                        }
                        if (tmpNode is not null) {
                            tmpNode.Nodes.Add(caption).Tag = new MenuData() {
                                Controller = controller,
                                Condition = itemCondition
                            };
                            tmpNode.Nodes[^1].ForeColor = controller.Length > 0 ? Color.Black : Color.Red;
                            // rare, but if too many, make note
                            if (tmpNode.Nodes.Count >= 23) {
                                tmpNode.Nodes[^1].ForeColor = Color.Red;
                                counterr = true;
                            }
                        }
                        break;
                    case "submit.menu":
                        // force ending
                        token.StartPos = submitPos;
                        continue;
                    case "if":
                        // find a condition
                        token = NextToken(source, token);
                        if (token.Text == "(") {
                            int parenCount = 1;
                            condition = "";
                            while (parenCount > 0) {
                                token = NextToken(source, token);
                                if (token.Text == "(") {
                                    parenCount++;
                                }
                                else if (token.Text == ")") {
                                    parenCount--;
                                }
                                if (parenCount > 0) {
                                    if (token.Type == AGITokenType.Symbol) {
                                        condition += token.Text switch {
                                            "(" or ")" or "!" => token.Text,
                                            _ => " " + token.Text + " ",
                                        };
                                    }
                                    else {
                                        condition += token.Text;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    break;
                }
                // get next token
                token = NextToken(source, token);
            }

            // if nothing added, return false
            if (tvwMenu.Nodes.Count == 0) {
                return false;
            }
            if (counterr) {
                MessageBox.Show(MDIMain,
                    "One or more menus have too many items to display correctly. " +
                    "You should remove entries from the affected menus so there " +
                    "are no more than 22 entries.",
                    "Too Many Menu Items",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            return true;
        }

        private string GetMsgText(string msgID, string source) {
            int msgNum, pos;

            // if first time through, extract messages
            if (!gotMsgs) {
                // if section start found, extract out messages
                if (source.Contains("#message") || source.Contains("#define")) {
                    string[] sourceLines = source.Split('\r');
                    string msgText;

                    for (int i = 0; i < sourceLines.Length; i++) {
                        if (sourceLines[i].Contains("#message")) {
                            // eliminate tab characters and trim the string
                            msgText = sourceLines[i].Replace('\t', ' ');
                            msgText = msgText.Replace("\n", "").Trim();
                            // use do loop to skip to next line if current line isn't a msg declaration
                            do {
                                pos = msgText.IndexOf("#message");
                                if (pos != 0) {
                                    break;
                                }
                                msgText = msgText.Right(msgText.Length - 8).Trim();
                                // find next space to strip off the number
                                pos = msgText.IndexOf(' ');
                                if (pos == -1) {
                                    break;
                                }
                                msgNum = msgText.Left(pos).IntVal();
                                if (msgNum <= 0 || msgNum > 255) {
                                    break;
                                }
                                msgText = msgText.Right(msgText.Length - pos).Trim();
                                if (msgText.Length == 0) {
                                    break;
                                }
                                if (msgText.Length < 2 || msgText[0] != '\"' || msgText[^1] != '\"') {
                                    // it might be a define
                                    if (EditGame is not null && EditGame.IncludeGlobals &&
                                        EditGame.GlobalDefines.ContainsName(msgText)) {
                                        msgText = EditGame.GlobalDefines[msgText].Value;
                                        if (msgText.Length < 2 || msgText[0] != '\"' || msgText[^1] != '\"') {
                                            break;
                                        }
                                    }
                                    else {
                                        break;
                                    }
                                }
                                msgText = msgText[1..^1];
                                messages[msgNum] = msgText;
                            } while (false);
                        }
                        if (sourceLines[i].Contains("#define")) {
                            // eliminate tab characters and trim the string
                            msgText = sourceLines[i].Replace('\t', ' ');
                            msgText = msgText.Replace("\n", "").Trim();
                            // use do loop to skip to next line if current line isn't a define
                            do {
                                pos = msgText.IndexOf("#define");
                                if (pos != 0) {
                                    break;
                                }
                                msgText = msgText.Right(msgText.Length - 7).Trim();
                                // find next space to strip off the name
                                pos = msgText.IndexOf(' ');
                                if (pos == -1) {
                                    break;
                                }
                                string defineName = msgText.Left(pos).Trim();
                                if (defineName.Length == 0) {
                                    break;
                                }
                                string definevalue = msgText.Right(msgText.Length - pos).Trim();
                                if (definevalue.Length == 0) {
                                    break;
                                }
                                // only literal strings and message identifiers need to be tracked
                                if (definevalue.Length > 1 && definevalue[0] == '\"' && definevalue[^1] == '\"') {
                                    // literal string
                                    defines.Add(new(defineName, definevalue));
                                    break;
                                }
                                else {
                                    // non-literal string; add number if a message identifier
                                    if (definevalue.Length > 1 && definevalue[0] == 'm' && definevalue[1..].IntVal() > 0 && definevalue[1..].IntVal() <= 255) {
                                        defines.Add(new(defineName, definevalue[1..]));
                                    }
                                }
                            } while (false);
                        }
                    }
                    // if any defines are message pointers, replace them with their string value
                    for (int i = defines.Count - 1; i >= 0; i--) {
                        if (defines[i].Value[0] != '\"') {
                            int defineMsgNum = defines[i].Value.IntVal();
                            if (messages[defineMsgNum].Length > 0) {
                                defines[i] = new(defines[i].Key, messages[defineMsgNum]);
                            }
                        }
                        else {
                            defines[i] = new(defines[i].Key, defines[i].Value[1..^1]);
                        }
                    }
                }
                // got messages!
                gotMsgs = true;
            }
            // if a msg variable
            if (msgID[0] == 'm') {
                msgNum = msgID[1..].IntVal();
                if (msgNum > 0 && msgNum <= 255) {
                    if (messages[msgNum] is not null) {
                        return messages[msgNum];
                    }
                    else {
                        return msgID;
                    }
                }
            }
            // check local defines for a message declaration
            foreach (var define in defines) {
                if (define.Key == msgID) {
                    return define.Value;
                }
            }

            // if not a local variable, check globals (if a game is loaded)
            string retval = msgID;
            if (EditGame is not null) {
                // check globals list
                retval = ArgFromToken(msgID);
            }
            if (retval != msgID) {
                return retval;
            }
            else {
                // no match found; return original msgID
                return msgID;
            }
        }

        private void ResizeMenuItems(TreeNode menuNode) {
            // first, determine Max length; start with first node
            TreeNode tmpNode = menuNode.FirstNode;
            int max = tmpNode.Text.Length;
            while (tmpNode is not null) {
                // reset the hotkey flag
                bool hasKey = false;
                SplitMenuItem(tmpNode.Text, out string menuText, out string hotkeyText);
                if (tvwMenu.SelectedNode != tmpNode) {
                    if (WinAGISettings.AutoAlignHotKey.Value) {
                        // if the item has a shortcut key
                        if (hotkeyText.Length > 0) {
                            hasKey = true;
                        }
                    }
                }
                if (hasKey) {
                    // don't use entire length; trim out the excess spaces
                    // between the menu item and its shortcut key definition
                    if (menuText.Length + hotkeyText.Length + 1 > max) {
                        max = menuText.Length + hotkeyText.Length + 1;
                    }
                }
                else {
                    // if menu item is NOT a spacer (all dashes)
                    if (tmpNode.Text != new string('-', tmpNode.Text.Length)) {
                        if (tvwMenu.SelectedNode == tmpNode) {
                            // ALWAYS use entire length
                            // (allows user to pad menu items manually)
                            int test = tmpNode.Text.Length;
                            if (test > max) {
                                max = test;
                            }
                        }
                        else {
                            // trim end if this is not current line
                            int test = tmpNode.Text.Trim().Length;
                            if (test > max) {
                                max = test;
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
                    tmpNode.Text = new string('-', max);
                }
                else {
                    // pad it with spaces
                    tmpNode.Text = PadItem(tmpNode.Text, max);
                }
                tmpNode = tmpNode.NextNode;
            }
        }

        private static string PadItem(string ItemText, int MaxLen) {
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

        private static void SplitMenuItem(string ItemText, out string MenuText, out string HotKeyText) {
            // splits a menu item into its menu text and its shortcut key text
            // if no shortcut key text, HotKeyText is empty
            // look for last '<' character
            int pos = ItemText.LastIndexOf('<');
            if (pos > 0) {
                // if there is a matching '>' at the end of the string (not including spaces)
                if (ItemText.TrimEnd()[^1] == '>') {
                    MenuText = ItemText.Left(pos).TrimEnd();
                    HotKeyText = ItemText[pos..].Trim();
                    return;
                }
            }
            // if not found, return entire string as menu text
            MenuText = ItemText;
            HotKeyText = "";
        }

        private string CreateMenu() {
            // create menu structure based on treelist entries
            const string sMenu = "set.menu(\"";
            string sMenuEnd = "\");" + Environment.NewLine;
            const string sItem = "set.menu.item(\"";
            const string sComma = "\", ";
            string sItemEnd = ");" + Environment.NewLine;

            // add header
            string retval = EditorResourceByNum(102);
            string menuCondition = "", itemCondition = "";
            string tab = "".PadRight(WinAGISettings.LogicTabWidth.Value);
            string menutab = "", itemtab = "";
            foreach (TreeNode node in tvwMenu.Nodes) {
                // check for change in condition
                string newCondition = ((MenuData)node.Tag).Condition;
                if (menuCondition != newCondition) {
                    if (menuCondition.Length > 0) {
                        retval += tab + "}" + Environment.NewLine;
                    }
                    if (newCondition.Length > 0) {
                        retval += "if (" + newCondition + ")" + Environment.NewLine +
                            tab + "{" + Environment.NewLine;
                        menutab = tab;
                    }
                    else {
                        menutab = "";
                    }
                    menuCondition = newCondition;
                }

                // add menu text
                retval += menutab + sMenu + node.Text + sMenuEnd;
                // add menu items
                foreach (TreeNode itemNode in node.Nodes) {
                    // check for change in condition
                    newCondition = ((MenuData)itemNode.Tag).Condition;
                    if (itemCondition != newCondition) {
                        if (itemCondition.Length > 0) {
                            retval += menutab + tab + "}" + Environment.NewLine;
                        }
                        if (newCondition.Length > 0) {
                            retval += menutab + "if (" + newCondition + ")" + Environment.NewLine;
                            retval += menutab + tab + "{" + Environment.NewLine;
                            itemtab = tab;
                        }
                        else {
                            itemtab = "";
                        }
                        itemCondition = newCondition;
                    }
                    retval += menutab + itemtab + sItem + itemNode.Text + sComma + ((MenuData)itemNode.Tag).Controller + sItemEnd;
                }
                if (itemCondition.Length > 0) {
                    retval += menutab + tab + "}" + Environment.NewLine;
                    itemCondition = "";
                }
            }
            if (itemCondition.Length > 0) {
                retval += menutab + tab + "}" + Environment.NewLine;
            }
            if (menuCondition.Length > 0) {
                retval += tab + "}" + Environment.NewLine;
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
                node.Tag = new MenuData();
                node.Nodes.Add("About     ").Tag = new MenuData() { Controller = "c21" };
                node.Nodes.Add("Help  <F1>").Tag = new MenuData() { Controller = "c2" };
                node.Nodes.Add("Debug Help").Tag = new MenuData() { Controller = "c33" };

                node = tvwMenu.Nodes.Add("File");
                node.Tag = new MenuData();
                node.Nodes.Add("Save     <F5>").Tag = new MenuData() { Controller = "c3" };
                node.Nodes.Add("Restore  <F7>").Tag = new MenuData() { Controller = "c5" };
                node.Nodes.Add("-------------").Tag = new MenuData() { Controller = "c20" };
                node.Nodes.Add("Restart  <F9>").Tag = new MenuData() { Controller = "c7" };
                node.Nodes.Add("Quit  <Alt-Z>").Tag = new MenuData() { Controller = "c1" };

                node = tvwMenu.Nodes.Add("Action");
                node.Tag = new MenuData();
                node.Nodes.Add("See Object <F4>").Tag = new MenuData() { Controller = "c22" };
                node.Nodes.Add("Inventory <Tab>").Tag = new MenuData() { Controller = "c10" };

                node = tvwMenu.Nodes.Add("Special");
                node.Tag = new MenuData();
                node.Nodes.Add("Sound On/Off      <F2>").Tag = new MenuData() { Controller = "c16" };
                node.Nodes.Add("Color/BW      <Ctrl R>").Tag = new MenuData() {
                    Controller = "c6",
                    Condition = "v20 == 0 && v26 < 2"
                };
                node.Nodes.Add("Clock On/Off      <F6>").Tag = new MenuData() { Controller = "c12" };
                node.Nodes.Add("Joystick      <Ctrl J>").Tag = new MenuData() { Controller = "c15" };
                node.Nodes.Add("Pause            <Esc>").Tag = new MenuData() { Controller = "c18" };

                node = tvwMenu.Nodes.Add("Speed");
                node.Tag = new MenuData();
                node.Nodes.Add("Normal ").Tag = new MenuData() { Controller = "c24" };
                node.Nodes.Add("Slow   ").Tag = new MenuData() { Controller = "c25" };
                node.Nodes.Add("Fast   ").Tag = new MenuData() { Controller = "c23" };
                node.Nodes.Add("Fastest").Tag = new MenuData() { Controller = "c28" };

                node = tvwMenu.Nodes.Add("Debug");
                node.Tag = new MenuData() { Condition = "isset(debug)" };
                node.Nodes.Add("Ego Info   <Alt-E>").Tag = new MenuData() { Controller = "c29" };
                node.Nodes.Add("Pri Screen <Alt-P>").Tag = new MenuData() { Controller = "c13" };
                node.Nodes.Add("Memory     <Alt-M>").Tag = new MenuData() { Controller = "c11" };
                node.Nodes.Add("Obj Info   <Alt-I>").Tag = new MenuData() { Controller = "c36" };
                node.Nodes.Add("Coords     <Alt-X>").Tag = new MenuData() { Controller = "c31" };
                node.Nodes.Add("Get All           ").Tag = new MenuData() { Controller = "c32" };
            }
            // select the first menu
            tvwMenu.SelectedNode = tvwMenu.Nodes[0];
            tvwMenu.ExpandAll();
            MarkAsChanged();
        }

        private bool LoadDefaultMenu() {

            // attempts to load a default menu from text file
            // if no default menu file,
            if (!File.Exists(Path.Combine(AppDataDir, "default_menu.txt"))) {
                return false;
            }
            try {
                using StreamReader sr = new(new FileStream(Path.Combine(AppDataDir, "default_menu.txt"), FileMode.Open, FileAccess.Read));
                // extract menu; return success if extract works
                return ExtractMenu(sr.ReadToEnd());
            }
            catch {
                return false;
            }
        }

        private void DefaultBackground() {
            bool loaded;
            if (EditGame is not null) {
                if (EditGame.Pictures.Count != 0) {
                    // get first valid picture
                    for (int i = 0; i < 256; i++) {
                        if (EditGame.Pictures.Contains(i)) {
                            loaded = EditGame.Pictures[i].Loaded;
                            if (!loaded) {
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
            using (frmGetResourceNum frm = new(GetRes.MenuBkgd, AGIResType.Picture)) {
                if (BkgdPicNum != -1) {
                    frm.OldResNum = (byte)BkgdPicNum;
                }
                if (frm.ShowDialog() == DialogResult.OK) {
                    BkgdPicNum = frm.NewResNum;
                    return true;
                }
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
                bool loaded = EditGame.Pictures[BkgdPicNum].Loaded;
                if (!loaded) {
                    EditGame.Pictures[BkgdPicNum].Load();
                }
                g.DrawImage(EditGame.Pictures[BkgdPicNum].VisualBMP, 0, 8 * PicScale, 320 * PicScale, 168 * PicScale);
                if (!loaded) {
                    EditGame.Pictures[BkgdPicNum].Unload();
                }
            }
            else {
                // write sample picture message
                // adjust offset by width of one character
                for (int i = 0; i < "sample AGI display".Length; i++) {
                    char charval = "sample AGI display"[i];
                    g.DrawImage(invchargrid, TargetCharPos(11, 11 + i), SourceCharPos(charval), GraphicsUnit.Pixel);
                }
            }
            g.DrawImage(invchargrid, TargetCharPos(22, 0), SourceCharPos('>'), GraphicsUnit.Pixel);
            picBackground.Image = bmp;
        }

        private Rectangle SourceCharPos(char charval) {
            // convert charval to correct codepage
            byte ch = Encoding.GetEncoding(CodePage).GetBytes([charval])[0];
            return new(16 * (ch % 16), 16 * (ch / 16), 16, 16);
        }

        private Rectangle TargetCharPos(int row, int col) {
            return new(col * CharW * PicScale, row * 8 * PicScale, CharW * PicScale, 8 * PicScale);
        }

        private void DrawMenu(Graphics g, int menu, int item) {
            // clear top line
            SolidBrush b = new(Color.White);
            g.FillRectangle(b, 0, 0, picBackground.Width, 8 * PicScale);
            int menucol = 0;
            if (MaxW == 80) {
                menucol++;
            }
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
            if (MaxW == 80) {
                width += 2;
            }
            // right align menu if it does not fit
            if (left + width > MaxW) {
                left = MaxW - width;
            }
            // draw menu area
            g.FillRectangle(bw, CharW * left * PicScale, 8 * PicScale, CharW * PicScale * width, 8 * PicScale * height);
            // draw menu border
            g.FillRectangle(bb, ((CharW * left) + 2) * PicScale, 9 * PicScale,
                ((CharW * width) - 4 - 1) * PicScale, PicScale);
            g.FillRectangle(bb, ((CharW * left) + 2) * PicScale, ((8 * height) + 6) * PicScale,
                ((CharW * width) - 4 - 1) * PicScale, PicScale);
            g.FillRectangle(bb, ((CharW * left) + 2) * PicScale, 9 * PicScale,
                2 * PicScale, ((8 * height) - 2) * PicScale);
            g.FillRectangle(bb, (CharW * (left + width) - 4) * PicScale, 9 * PicScale,
                2 * PicScale, ((8 * height) - 2) * PicScale);

            // now add the menu item captions
            left++;
            top++;
            int itemcol, itemrow = top;
            foreach (TreeNode node in tvwMenu.Nodes[menu].Nodes) {
                itemcol = left;
                if (MaxW == 80) {
                    itemcol++;
                }
                for (int i = 0; i < node.Text.Length; i++) {
                    g.DrawImage(node.Index == item ? invchargrid : chargrid,
                        TargetCharPos(itemrow, itemcol++),
                        SourceCharPos(node.Text[i]),
                        GraphicsUnit.Pixel);
                }
                itemrow++;
                if (itemrow >= 25) {
                    break;
                }
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
            int col = pos.X / CharW / PicScale;
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
            if (end > MaxW - 1) {
                start = MaxW - 1 - menunode.Nodes[0].Text.Length;
                end = MaxW - 1;
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

            if (NewCaption == oldCaption) {
                return true;
            }
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
                    if (menulength > MaxW) {
                        MDIMain.MsgBoxWithHelp(
                            "Menu text is too long for all menus to fit on screen. Shorten or " +
                            "delete one of the other menus to make additional space available " +
                            "for this menu.",
                            "Menu Caption Too Long",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\agi\\menu.htm#displaymenus");
                        return false;
                    }
                }
                tvwMenu.SelectedNode.Text = NewCaption;
            }
            else {
                // menu item
                if (NewCaption.Length > MaxW - 2) {
                    MDIMain.MsgBoxWithHelp(
                        "Menu item text is too long to fit on screen.",
                        "Menu Item Text Too Long",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\agi\\menu.htm#displaymenus");
                    return false;
                }
                tvwMenu.SelectedNode.Text = NewCaption;
                // adjust menu items
                ResizeMenuItems(tvwMenu.SelectedNode.Parent);
            }
            MarkAsChanged();
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
            if (((MenuData)tvwMenu.SelectedNode.Tag).Controller != NewCtrl) {
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
                MenuData md = (MenuData)tvwMenu.SelectedNode.Tag;
                md.Controller = NewCtrl;
                tvwMenu.SelectedNode.Tag = md;
                tvwMenu.SelectedNode.ForeColor = NewCtrl.Length > 0 ? Color.Black : Color.Red;
                MarkAsChanged();
            }
            return true;
        }

        private void UpdateCondition(string NewCondition) {
            NewCondition = NewCondition.Trim();
            string oldCondition = tvwMenu.SelectedNode.Text;

            if (NewCondition == oldCondition) {
                return;
            }

            if (tvwMenu.SelectedNode.Level == 0) {
                // menu
                tvwMenu.SelectedNode.Tag = new MenuData() { Condition = NewCondition };
            }
            else {
                // menu item (need to retain controller value)
                MenuData md = (MenuData)tvwMenu.SelectedNode.Tag;
                md.Condition = NewCondition;
                tvwMenu.SelectedNode.Tag = md;
            }
            MarkAsChanged();
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
                        if (((MenuData)tvwMenu.Nodes[i].Nodes[j].Tag).Controller == controller) {
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
            if (dgProps.Focused) {
                if (dgProps.CurrentRow.Index == 2) {
                    topic += "#conditions";
                }
                else {
                    topic += "#properties";
                }
            }
            else if (EditTextBox.Focused) {
                if (dgProps.CurrentRow.Index == 2) {
                    topic += "#conditions";
                }
                else {
                    topic += "#edit";
                }
            }
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
                // set changed flag
                IsChanged = true;

                // enable menu and toolbar button
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
        }
        #endregion
    }
}
