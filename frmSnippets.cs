using FastColoredTextBoxNS;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    public partial class frmSnippets : Form {
        #region Fields
        bool AddSnip, IsChanged = false;
        // editor syntax styles
        public TextStyle CommentStyle;
        public TextStyle StringStyle;
        public TextStyle KeyWordStyle;
        public TextStyle TestCmdStyle;
        public TextStyle ActionCmdStyle;
        public TextStyle InvalidCmdStyle;
        public TextStyle NumberStyle;
        public TextStyle ArgIdentifierStyle;
        public TextStyle DefIdentifierStyle;
        #endregion

        #region Constructors
        public frmSnippets(bool newsnippet, string value = "") {
            InitializeComponent();
            lstSnippets.Items.Clear();
            if (CodeSnippets.Length > 0) {
                foreach (Snippet snippet in CodeSnippets) {
                    lstSnippets.Items.Add(snippet);
                }
            }
            rtfSnipValue.LeftBracket = '(';
            rtfSnipValue.RightBracket = ')';
            rtfSnipValue.LeftBracket2 = '{';
            rtfSnipValue.RightBracket2 = '}';
            CommentStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[1].Color.Value), null, WinAGISettings.SyntaxStyle[1].FontStyle.Value);
            StringStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[2].Color.Value), null, WinAGISettings.SyntaxStyle[2].FontStyle.Value);
            KeyWordStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[3].Color.Value), null, WinAGISettings.SyntaxStyle[3].FontStyle.Value);
            TestCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[4].Color.Value), null, WinAGISettings.SyntaxStyle[4].FontStyle.Value);
            ActionCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[5].Color.Value), null, WinAGISettings.SyntaxStyle[5].FontStyle.Value);
            InvalidCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[6].Color.Value), null, WinAGISettings.SyntaxStyle[6].FontStyle.Value);
            NumberStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[7].Color.Value), null, WinAGISettings.SyntaxStyle[7].FontStyle.Value);
            ArgIdentifierStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[8].Color.Value), null, WinAGISettings.SyntaxStyle[8].FontStyle.Value);
            DefIdentifierStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[9].Color.Value), null, WinAGISettings.SyntaxStyle[9].FontStyle.Value);
            InitFonts(true);
            rtfSnipValue.TabLength = WinAGISettings.LogicTabWidth.Value;
            rtfSnipValue.Text = "";
            if (newsnippet) {
                AddNewSnippet(value);
            }
            else {
                lstSnippets.Select();
            }
        }
        #endregion

        #region Event Handlers
        #region Form Events
        private void frmSnippets_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            // check for editing mode
            if (!txtSnipName.ReadOnly) {
                if (MessageBox.Show(MDIMain,
                    "You are currently editing a snippet.  Do you want to save your changes to this snippet?",
                    "Save Changes?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes) {
                    e.Cancel = !SaveEdit();
                }
            }
            if (!e.Cancel) {
                // check for changes
                if (IsChanged) {
                    if (MessageBox.Show(MDIMain,
                        "You have made changes to the snippet list.  Do you want to save your changes?",
                        "Save Changes?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        SaveSnippets();
                    }
                }
            }
        }

        private void frmSnippets_HelpRequested(object sender, HelpEventArgs hlpevent) {
            ShowHelp();
            hlpevent.Handled = true;
        }
        #endregion

        #region Context Menu Events
        private void SetEditMenu() {
            // Undo (Ctrl+Z) V; E:if able (is type available?)
            // Redo (Ctrl+Y) V; E:if able (is type available?)
            // ----------- V
            // Cut (Ctrl+X) V; E
            // Copy (Ctrl+C) V; E
            // Delete (Del) V; E
            // Paste (Ctrl+V) V; E
            // Select All (Ctrl+A) V; E
            // ----------- V
            // Block Comment (Alt+B) V; E
            // Unblock Comment (Alt+U) V; E
            // ----------- V
            // Character Map (Ctrl+Ins) V; E;
            mnuEUndo.Enabled = rtfSnipValue.UndoEnabled;
            mnuERedo.Enabled = rtfSnipValue.RedoEnabled;
            mnuECut.Enabled = mnuECopy.Enabled = mnuEDelete.Enabled = rtfSnipValue.SelectionLength > 0;
            mnuEPaste.Enabled = Clipboard.ContainsText();
            mnuESelectAll.Enabled = rtfSnipValue.TextLength > 0;
        }

        private void ResetEditMenu() {
            // enable all items so shortcut keys are always available
            mnuEUndo.Enabled = true;
            mnuERedo.Enabled = true;
            mnuECut.Enabled = true;
            mnuECopy.Enabled = true;
            mnuEDelete.Enabled = true;
            mnuEPaste.Enabled = true;
            mnuESelectAll.Enabled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            if (txtSnipName.ReadOnly) {
                e.Cancel = true;
                return;
            }
            SetEditMenu();
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEUndo_Click(object sender, EventArgs e) {
            rtfSnipValue.Undo();
        }

        private void mnuERedo_Click(object sender, EventArgs e) {
            rtfSnipValue.Redo();
        }

        private void mnuECut_Click(object sender, EventArgs e) {
            if (rtfSnipValue.SelectionLength > 0) {
                rtfSnipValue.Cut();
            }
        }

        private void mnuEDelete_Click(object sender, EventArgs e) {
            if (rtfSnipValue.SelectionLength > 0) {
                rtfSnipValue.SelectedText = "";
            }
        }

        private void mnuECopy_Click(object sender, EventArgs e) {
            if (rtfSnipValue.SelectionLength > 0) {
                rtfSnipValue.Copy();
            }
        }

        private void mnuEPaste_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText()) {
                rtfSnipValue.Paste();
            }
        }

        private void mnuESelectAll_Click(object sender, EventArgs e) {
            rtfSnipValue.SelectAll();
        }

        private void mnuEBlockCmt_Click(object sender, EventArgs e) {
            rtfSnipValue.InsertLinePrefix(rtfSnipValue.CommentPrefix);
        }

        private void mnuEUnblockCmt_Click(object sender, EventArgs e) {
            rtfSnipValue.RemoveLinePrefix(rtfSnipValue.CommentPrefix);
        }

        private void mnuECharMap_Click(object sender, EventArgs e) {
            int codepage;
            if (EditGame is not null) {
                codepage = EditGame.CodePage;
            }
            else {
                codepage = CodePage;
            }
            using (frmCharPicker CharPicker = new(codepage)) {
                if (CharPicker.ShowDialog(MDIMain) == DialogResult.OK) {
                    if (CharPicker.InsertString.Length > 0) {
                        rtfSnipValue.InsertText(CharPicker.InsertString, true);
                    }
                }
            }
        }
        #endregion

        #region Control Events
        private void btnClose_Click(object sender, EventArgs e) {
            Close();
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            // delete the snippet from the listbox
            lstSnippets.Items.RemoveAt(lstSnippets.SelectedIndex);
            // select nothing
            lstSnippets.SelectedIndex = -1;
            IsChanged = true;
        }

        private void btnSave_Click(object sender, EventArgs e) {
            SaveEdit();
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            // add new snip with blank value
            AddNewSnippet("");
        }

        private void btnEdit_Click(object sender, EventArgs e) {
            BeginEdit(txtSnipName);
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            // cancel editing of this snippet
            if (AddSnip) {
                lstSnippets.Items.RemoveAt(lstSnippets.SelectedIndex);
                lstSnippets.SelectedIndex = -1;
                lstSnippets.Enabled = true;
                lstSnippets.Select();
                btnAdd.Enabled = true;
                btnClose.Enabled = true;
                AddSnip = false;
                return;
            }
            txtSnipName.Text = lstSnippets.Text;
            rtfSnipValue.Text = ((Snippet)lstSnippets.Items[lstSnippets.SelectedIndex]).Value;
            txtArgTips.Text = ((Snippet)lstSnippets.Items[lstSnippets.SelectedIndex]).ArgTips;

            // restore original snippet, in select mode
            UpdateForm(true);

            lstSnippets.Select();
        }

        private void lstSnippets_DoubleClick(object sender, EventArgs e) {
            if (lstSnippets.SelectedIndex != -1) {
                BeginEdit(txtSnipName);
            }
        }

        private void lstSnippets_SelectedValueChanged(object sender, EventArgs e) {
            if (lstSnippets.SelectedIndex == -1) {
                btnEdit.Visible = btnSave.Visible = btnDelete.Visible = btnCancel.Visible = false;
                rtfSnipValue.Text = "";
                rtfSnipValue.ReadOnly = true;
                txtSnipName.Text = "";
                txtSnipName.ReadOnly = true;
                lblArgTips.Visible = false;
                txtArgTips.Visible = false;
            }
            else {
                Snippet snippet = (Snippet)lstSnippets.SelectedItem;
                txtSnipName.Text = snippet.Name;
                txtArgTips.Text = snippet.ArgTips;
                rtfSnipValue.Text = snippet.Value;

                // update form to show select mode
                UpdateForm(true);
            }
        }

        private void rtfSnipValue_DoubleClick(object sender, EventArgs e) {
            if (rtfSnipValue.ReadOnly) {
                if (lstSnippets.SelectedIndex != -1) {
                    BeginEdit(sender);
                }
            }
        }

        private void rtfSnipValue_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e) {
            AGISyntaxHighlight(e.ChangedRange);
            if (rtfSnipValue.Text.Length > 0) {
                btnSave.Enabled = true;
            }
            else {
                btnSave.Enabled = false;
            }
        }

        private void txtSnipName_DoubleClick(object sender, EventArgs e) {
            if (txtSnipName.ReadOnly) {
                if (lstSnippets.SelectedIndex != -1) {
                    BeginEdit(txtSnipName);
                }
            }
        }

        private void txtSnipName_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Return) {
                e.Handled = true;
                rtfSnipValue.Select();
            }
        }

        private void txtArgTips_DoubleClick(object sender, EventArgs e) {
            if (txtSnipName.ReadOnly) {
                if (lstSnippets.SelectedIndex != -1) {
                    BeginEdit(txtArgTips);
                }
            }
        }

        private void txtArgTips_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Return) {
                e.Handled = true;
                rtfSnipValue.Select();
            }
        }
        #endregion
        #endregion

        #region Methods
        internal void InitFonts(bool select) {
            if (select) {
                rtfSnipValue.ForeColor = Color.Black;
                rtfSnipValue.BackColor = Color.FromArgb(0xe0, 0xff, 0xe0);
                rtfSnipValue.Range.ClearStyle(StyleIndex.All);
                // clear all styles of changed range
                rtfSnipValue.Range.ClearStyle(StyleIndex.All);
                // hide cursor
            }
            else {
                rtfSnipValue.ForeColor = WinAGISettings.SyntaxStyle[0].Color.Value;
                rtfSnipValue.BackColor = WinAGISettings.EditorBackColor.Value;
                int red = 255 - WinAGISettings.EditorBackColor.Value.R;
                int green = 255 - WinAGISettings.EditorBackColor.Value.G;
                int blue = 255 - WinAGISettings.EditorBackColor.Value.B;
                rtfSnipValue.SelectionColor = Color.FromArgb(128, red, green, blue);
                rtfSnipValue.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
                rtfSnipValue.DefaultStyle = new TextStyle(rtfSnipValue.DefaultStyle.ForeBrush, rtfSnipValue.DefaultStyle.BackgroundBrush, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
                if (WinAGISettings.HighlightLogic.Value) {
                    RefreshSyntaxStyles();
                    AGISyntaxHighlight(rtfSnipValue.Range);
                }
                else {
                    // clear all styles of changed range
                    rtfSnipValue.Range.ClearStyle(StyleIndex.All);
                }
            }
        }

        public void RefreshSyntaxStyles() {
            CommentStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[1].Color.Value);
            CommentStyle.FontStyle = WinAGISettings.SyntaxStyle[1].FontStyle.Value;
            StringStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[2].Color.Value);
            StringStyle.FontStyle = WinAGISettings.SyntaxStyle[2].FontStyle.Value;
            KeyWordStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[3].Color.Value);
            KeyWordStyle.FontStyle = WinAGISettings.SyntaxStyle[3].FontStyle.Value;
            TestCmdStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[4].Color.Value);
            TestCmdStyle.FontStyle = WinAGISettings.SyntaxStyle[4].FontStyle.Value;
            ActionCmdStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[5].Color.Value);
            ActionCmdStyle.FontStyle = WinAGISettings.SyntaxStyle[5].FontStyle.Value;
            InvalidCmdStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[6].Color.Value);
            InvalidCmdStyle.FontStyle = WinAGISettings.SyntaxStyle[6].FontStyle.Value;
            NumberStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[7].Color.Value);
            NumberStyle.FontStyle = WinAGISettings.SyntaxStyle[7].FontStyle.Value;
            ArgIdentifierStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[8].Color.Value);
            ArgIdentifierStyle.FontStyle = WinAGISettings.SyntaxStyle[8].FontStyle.Value;
            DefIdentifierStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[9].Color.Value);
            DefIdentifierStyle.FontStyle = WinAGISettings.SyntaxStyle[9].FontStyle.Value;
        }

        public void AGISyntaxHighlight(FastColoredTextBoxNS.Range range) {
            if (rtfSnipValue.ReadOnly) {
                return;
            }
            // clear all styles of changed range
            range.ClearStyle(StyleIndex.All);
            range.SetStyle(CommentStyle, CommentStyleRegEx1, RegexOptions.Multiline);
            range.SetStyle(CommentStyle, CommentStyleRegEx2, RegexOptions.Multiline);
            range.SetStyle(StringStyle, StringStyleRegEx);
            range.SetStyle(KeyWordStyle, FanKeyWordStyleRegEx);
            range.SetStyle(InvalidCmdStyle, InvalidCmdStyleRegEx);
            range.SetStyle(TestCmdStyle, TestCmdStyleRegEx);
            range.SetStyle(ActionCmdStyle, ActionCmdStyleRegEx);
            range.SetStyle(NumberStyle, NumberStyleRegEx);
            range.SetStyle(ArgIdentifierStyle, ArgIdentifierStyleRegEx);
            range.SetStyle(DefIdentifierStyle, DefIdentifierStyleRegEx);

            // clear folding markers
            range.ClearFoldingMarkers();

            // set folding markers
            range.SetFoldingMarkers("{", "}");
        }

        private void UpdateForm(bool select) {
            // select is true if returning to select mode, false if going to edit mode
            lstSnippets.Enabled = select;
            btnAdd.Enabled = select;
            btnClose.Enabled = select;
            btnEdit.Visible = select;
            btnDelete.Visible = select;
            btnSave.Visible = !select;
            btnCancel.Visible = !select;
            lblArgTips.Visible = txtArgTips.Text.Length > 0 || !select;
            lblArgTips.Enabled = !select;
            txtArgTips.Visible = lblArgTips.Visible;
            txtArgTips.ReadOnly = select;
            txtSnipName.ReadOnly = select;
            rtfSnipValue.ReadOnly = select;

            // refresh font/colors of edit box
            InitFonts(select);
        }

        private bool CheckName(string NewName, int SkipID = -1) {
            // check all snippet names; if checkname already exists, return false
            for (int i = 0; i < lstSnippets.Items.Count; i++) {
                Snippet item = (Snippet)lstSnippets.Items[i];
                if (item.Name == NewName && SkipID != i) {
                    return false;
                }
            }
            // not a duplicate
            return true;
        }

        private void AddNewSnippet(string value) {
            string tmpName;
            Snippet newsnippet = new();

            // get a non-duplicate name
            int idNum = 0;
            do {
                idNum++;
                tmpName = "snippet" + idNum;
            }
            while (!CheckName(tmpName));
            newsnippet.Name = tmpName;
            newsnippet.Value = value;
            int index = lstSnippets.Items.Add(newsnippet);
            lstSnippets.SelectedIndex = index;
            txtArgTips.Text = "";
            AddSnip = true;
            lstSnippets.Sorted = false;
            lstSnippets.Sorted = true;
            BeginEdit(txtSnipName);
            txtSnipName.SelectAll();
        }

        private void BeginEdit(object sender) {
            // update form to show edit mode
            UpdateForm(false);

            rtfSnipValue.ClearUndo();
            ((Control)sender).Select();
        }

        private bool SaveEdit() {
            // validate name
            if (txtSnipName.Text.Trim().Length == 0) {
                MessageBox.Show(MDIMain,
                    "Name can't be blank.",
                    "Blank Snippet Name Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return false;
            }
            if (!CheckName(txtSnipName.Text.Trim(), lstSnippets.SelectedIndex)) {
                MessageBox.Show(MDIMain,
                    "'" + txtSnipName.Text.Trim() + "' is already in use as a snippet name.",
                    "Duplicate Snippet Names Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return false;
            }
            // validate value
            string valuetext = rtfSnipValue.Text;
            if (valuetext.Length == 0) {
                MessageBox.Show(MDIMain,
                    "Snippet value can't be blank.",
                    "Blank Snippet Value Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return false;
            }
            // snippet is OK
            Snippet newsnippet = new() {
                Name = txtSnipName.Text.Trim(),
                Value = valuetext,
                ArgTips = txtArgTips.Text
            };
            lstSnippets.Items[lstSnippets.SelectedIndex] = newsnippet;
            lstSnippets.Refresh();

            // update form to show select mode
            UpdateForm(true);

            lstSnippets.Select();
            lstSnippets.Sorted = false;
            lstSnippets.Sorted = true;
            AddSnip = false;
            IsChanged = true;
            return true;
        }

        /// <summary>
        /// Encode the snippet value for saving to the snippets file.  This replaces
        /// newlines, tabs, quotes, and percent signs with control codes so that the
        /// value can be saved on a single line in the file.  The control codes will
        /// be decoded when loading the snippets file.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string EncodeSnippet(string value) {
            // first, replace all % with a temporary code so we don't double encode
            // the % in our control codes
            value = Regex.Replace(value, @"(?!%\d)%", "\u0018");
            // next replace newlines, tabs, and quotes with control codes
            value = value.Replace("\r\n", "%n");
            value = value.Replace("\n", "%n");
            value = value.Replace("\r", "%n");
            value = value.Replace("\"", "%q");
            // tabs require more work
            int pos = 0, tablength = WinAGISettings.LogicTabWidth.Value;
            string tab = new(' ', tablength);
            do {
                // check for spaces at start of this line
                if (value.Mid(pos, tablength) == tab) {
                    // replace the spaces with a tab
                    value = value.Left(pos) + "%t" + value.Right(value.Length - pos - tablength);
                    pos += 2;
                }
                else {
                    // no more tabs for this line; get start of next line
                    pos = value.IndexOf("%n", pos);
                    if (pos < 0) {
                        break;
                    }
                    pos += 2;
                }
            }
            while (pos > 0);
            // finally, replace the temporary % code with %%
            value = value.Replace("\u0018", "%%");
            return value;
        }

        private void SaveSnippets() {
            // if changed save the list
            if (!IsChanged) {
                return;
            }
            SettingsFile SnipList = new(Path.Combine(AppDataDir, "snippets.txt"), FileMode.Create);
            // add the header
            SnipList.Lines.Add("#");
            SnipList.Lines.Add("# WinAGI Snippets");
            SnipList.Lines.Add("#");
            SnipList.Lines.Add("# control codes:");
            SnipList.Lines.Add("#   %n = new line");
            SnipList.Lines.Add("#   %q = '" + QUOTECHAR + "'");
            SnipList.Lines.Add("#   %t = tab (based on current tab setting)");
            SnipList.Lines.Add("#   %% = '%'");
            SnipList.Lines.Add("#   %1, %2, etc snippet argument value");

            CodeSnippets = new Snippet[lstSnippets.Items.Count];
            for (int i = 0; i < lstSnippets.Items.Count; i++) {
                CodeSnippets[i] = (Snippet)lstSnippets.Items[i];
                SnipList.Lines.Add("");
                SnipList.Lines.Add("[Snippet]");
                SnipList.Lines.Add("   Name = " + CodeSnippets[i].Name);
                SnipList.Lines.Add("   Value = " + EncodeSnippet(CodeSnippets[i].Value));
                if (CodeSnippets[i].ArgTips.Length > 0) {
                    SnipList.Lines.Add("   ArgTips = " + EncodeSnippet(CodeSnippets[i].ArgTips));
                }
            }
            SnipList.Save();
        }

        void ShowHelp() {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\snippets.htm");
        }
        #endregion
    }
}
