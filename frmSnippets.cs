using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace WinAGI.Editor {
    public partial class frmSnippets : Form {
        bool blnAddSnip, IsChanged = false;
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
            InitFonts();
            rtfSnipValue.TabLength = WinAGISettings.LogicTabWidth.Value;
            rtfSnipValue.Text = "";

            if (newsnippet) {
                AddNewSnippet(value);
            }
        }

        #region Event Handlers
        private void frmSnippets_FormClosing(object sender, FormClosingEventArgs e) {
            if (!txtSnipName.ReadOnly) {
                if (MessageBox.Show(MDIMain,
                    "You are currently editing a snippet.  Do you want to save your changes?",
                    "Save Changes?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes) {
                    e.Cancel = !SaveEdit();
                }
            }
            if (!e.Cancel) {
                // save the list
                SaveSnippets();
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
            this.Dispose();
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            // delete the snippet from the listbox
            lstSnippets.Items.RemoveAt(lstSnippets.SelectedIndex);
            //select nothing
            lstSnippets.SelectedIndex = -1;
            IsChanged = true;
        }

        private void btnSave_Click(object sender, EventArgs e) {
            _ = SaveEdit();
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
            if (blnAddSnip) {
                lstSnippets.Items.RemoveAt(lstSnippets.SelectedIndex);
                lstSnippets.SelectedIndex = -1;
                lstSnippets.Enabled = true;
                lstSnippets.Select();
                btnAdd.Enabled = true;
                btnClose.Enabled = true;
                blnAddSnip = false;
                return;
            }
            // restore original snippet, in view mode
            txtSnipName.Text = lstSnippets.Text;
            rtfSnipValue.Text = ((Snippet)lstSnippets.Items[lstSnippets.SelectedIndex]).Value;
            txtArgTips.Text = ((Snippet)lstSnippets.Items[lstSnippets.SelectedIndex]).ArgTips;
            lblArgTips.Visible = ((Snippet)lstSnippets.Items[lstSnippets.SelectedIndex]).ArgTips.Length > 0;
            txtArgTips.Visible = lblArgTips.Visible;
            txtSnipName.ReadOnly = true;
            rtfSnipValue.ReadOnly = true;
            btnEdit.Visible = true;
            btnSave.Visible = false;
            btnDelete.Visible = true;
            btnCancel.Visible = false;
            btnAdd.Enabled = true;
            btnClose.Enabled = true;
            lblArgTips.Enabled = false;
            txtArgTips.ReadOnly = true;
            lstSnippets.Enabled = true;
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
                txtSnipName.ReadOnly = true;
                rtfSnipValue.Text = snippet.Value;
                rtfSnipValue.ReadOnly = false;
                //'argument tips
                txtArgTips.Text = snippet.ArgTips;
                txtArgTips.Visible = lblArgTips.Visible = txtArgTips.Text.Length > 0;
                txtArgTips.ReadOnly = true;
                lblArgTips.Enabled = false;
                btnSave.Visible = false;
                btnEdit.Visible = true;
                btnCancel.Visible = false;
                btnDelete.Visible = true;
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

        private void txtArgTips_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Return) {
                e.Handled = true;
                rtfSnipValue.Select();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            if (txtSnipName.ReadOnly) {
                e.Cancel = true;
                return;
            }
            RefreshEditMenu();
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
            if (EditGame != null) {
                codepage = EditGame.CodePage;
            }
            else {
                codepage = CodePage;
            }
            frmCharPicker CharPicker = new(codepage);
            CharPicker.ShowDialog(MDIMain);
            if (!CharPicker.Cancel) {
                if (CharPicker.InsertString.Length > 0) {
                    rtfSnipValue.InsertText(CharPicker.InsertString, true);
                }
            }
            CharPicker.Close();
            CharPicker.Dispose();
        }

        #endregion

        internal void InitFonts() {
            rtfSnipValue.ForeColor = WinAGISettings.SyntaxStyle[0].Color.Value;
            rtfSnipValue.BackColor = WinAGISettings.EditorBackColor.Value;
            int red = 255 - WinAGISettings.EditorBackColor.Value.R;
            int green = 255 - WinAGISettings.EditorBackColor.Value.G;
            int blue = 255 - WinAGISettings.EditorBackColor.Value.B;
            rtfSnipValue.SelectionColor = System.Drawing.Color.FromArgb(128, red, green, blue);
            rtfSnipValue.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            rtfSnipValue.DefaultStyle = new TextStyle(rtfSnipValue.DefaultStyle.ForeBrush, rtfSnipValue.DefaultStyle.BackgroundBrush, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            if (WinAGISettings.HighlightLogic.Value) {
                RefreshSyntaxStyles();
                AGISyntaxHighlight(rtfSnipValue.Range);
            }
            else {
                //clear all styles of changed range
                rtfSnipValue.Range.ClearStyle(StyleIndex.All);
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

        private void RefreshEditMenu() {
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

        public void AGISyntaxHighlight(FastColoredTextBoxNS.Range range) {
            //clear all styles of changed range
            range.ClearStyle(StyleIndex.All);
            range.SetStyle(CommentStyle, CommentStyleRegEx1, RegexOptions.Multiline);
            range.SetStyle(CommentStyle, CommentStyleRegEx2, RegexOptions.Multiline);
            range.SetStyle(StringStyle, StringStyleRegEx);
            range.SetStyle(KeyWordStyle, KeyWordStyleRegEx);
            range.SetStyle(InvalidCmdStyle, InvalidCmdStyleRegEx);
            range.SetStyle(TestCmdStyle, TestCmdStyleRegex);
            range.SetStyle(ActionCmdStyle, ActionCmdStyleRegEx);
            range.SetStyle(NumberStyle, NumberStyleRegEx);
            range.SetStyle(ArgIdentifierStyle, ArgIdentifierStyleRegEx);
            range.SetStyle(DefIdentifierStyle, DefIdentifierStyleRegEx);

            //clear folding markers
            range.ClearFoldingMarkers();

            //set folding markers
            range.SetFoldingMarkers("{", "}");
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
            int lngID = 0;
            do {
                lngID++;
                tmpName = "snippet" + lngID;
            }
            while (!CheckName(tmpName));
            newsnippet.Name = tmpName;
            newsnippet.Value = value;
            int index = lstSnippets.Items.Add(newsnippet);
            lstSnippets.SelectedIndex = index;
            txtArgTips.Text = "";
            blnAddSnip = true;
            lstSnippets.Sorted = false;
            lstSnippets.Sorted = true;
            BeginEdit(txtSnipName);
            txtSnipName.SelectAll();
        }

        private void BeginEdit(object sender) {
            lstSnippets.Enabled = false;
            btnAdd.Enabled = false;
            btnClose.Enabled = false;
            btnEdit.Visible = false;
            btnDelete.Visible = false;
            btnSave.Visible = true;
            btnCancel.Visible = true;
            lblArgTips.Visible = true;
            txtArgTips.Visible = true;
            lblArgTips.Enabled = true;
            txtArgTips.ReadOnly = false;
            txtSnipName.ReadOnly = false;
            rtfSnipValue.ReadOnly = false;
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
            string strValue = rtfSnipValue.Text;
            if (strValue.Length == 0) {
                MessageBox.Show(MDIMain,
                    "Snippet value can't be blank.",
                    "Blank Snippet Value Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return false;
            }
            // snippet is OK
            Snippet newsnippet = new Snippet();
            newsnippet.Name = txtSnipName.Text.Trim();
            newsnippet.Value = strValue;
            newsnippet.ArgTips = txtArgTips.Text;
            lstSnippets.Items[lstSnippets.SelectedIndex] = newsnippet;
            lstSnippets.Refresh();
            lstSnippets.Enabled = true;
            btnAdd.Enabled = true;
            btnClose.Enabled = true;
            txtSnipName.ReadOnly = false;
            rtfSnipValue.ReadOnly = false;
            btnEdit.Visible = true;
            btnSave.Visible = false;
            btnDelete.Visible = true;
            btnCancel.Visible = false;
            lblArgTips.Visible = txtArgTips.Text.Length > 0;
            txtArgTips.Visible = lblArgTips.Visible;
            lblArgTips.Enabled = false;
            txtArgTips.ReadOnly = true;
            lstSnippets.Select();
            lstSnippets.Sorted = false;
            lstSnippets.Sorted = true;
            blnAddSnip = false;
            IsChanged = true;
            return true;
        }

        private string EncodeSnippet(string value) {
            // encode the snippet value
            value = Regex.Replace(value, @"(?!%\d)%", "\u0018");
            value = value.Replace("\r\n", "%n");
            value = value.Replace("\n", "%n");
            value = value.Replace("\r", "%n");
            value = value.Replace("\"", "%q");
            // tab require more work
            int lngPos = 0, tablength = WinAGISettings.LogicTabWidth.Value;
            string tab = new(' ', tablength);
            do {
                // check for spaces at start of this line
                if (value.Mid(lngPos, tablength) == tab) {
                    //replace the spaces with a tab
                    value = value.Left(lngPos) + "%t" + value.Right(value.Length - lngPos - tablength);
                    lngPos += 2;
                }
                else {
                    // no more tabs for this line; get start of next line
                    lngPos = value.IndexOf("%n", lngPos);
                    if (lngPos < 0) {
                        break;
                    }
                    lngPos += 2;
                }
            }
            while (lngPos > 0);
            value = value.Replace("\u0018", "%%");
            return value;
        }

        private void SaveSnippets() {
            // if changed save the list
            if (!IsChanged) {
                return;
            }
            SettingsFile SnipList = new(ProgramDir + "snippets.txt", System.IO.FileMode.Create);
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

        #region temp code
        void tmpcode() {
            /*
rivate Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

  'always check for help first
  If Shift = 0 And KeyCode = vbKeyF1 Then
    Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\winagi\snippets.htm");
    KeyCode = 0
    Exit Sub
  End If

End Sub
            */
        }
        #endregion

        private void txtArgTips_DoubleClick(object sender, EventArgs e) {
            if (txtSnipName.ReadOnly) {
                if (lstSnippets.SelectedIndex != -1) {
                    BeginEdit(txtArgTips);
                }
            }
        }
    }
}
