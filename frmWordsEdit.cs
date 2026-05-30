using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmWordsEdit : ClipboardMonitor {
        #region Members
        private bool GroupMode = true;
        public bool InGame;
        public bool IsChanged;
        public WordList EditWordList;
        private string Filename;
        private bool RefreshLogics = false;
        private int EditGroupIndex, EditGroupNumber;
        private int EditWordIndex, EditWordGroupIndex;
        private string EditWordText;
        private bool EditingWord = false, EditingGroup = false;
        private bool AddNewWord = false;
        private bool AddNewGroup = false;
        private bool FirstFind = false;
        private bool Recurse = false;
        private Font defaultfont;
        private Font boldfont;
        private Stack<WordsUndo> UndoCol = new();
        private DataGridViewCellStyle warningstyle;
        private DataGridViewCellStyle reservedstyle;
        // StatusStrip Items
        internal ToolStripStatusLabel spGroupCount;
        internal ToolStripStatusLabel spWordCount;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        #endregion

        public static frmWordsEdit DragSourceForm {
            get; private set;
        }

        public frmWordsEdit() {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
            InitializeComponent();
            // make sure edit boxes are at top of zorder
            txtGroupEdit.BringToFront();
            txtWordEdit.BringToFront();

            InitStatusStrip();
            InitFonts();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // command keys need to be intercepted and rerouted
            switch (keyData) {
            case Keys.Enter:
            case Keys.Tab:
            case Keys.Escape:
                if (txtGroupEdit.Visible) {
                    txtGroupEdit_KeyDown(txtGroupEdit, new KeyEventArgs(keyData));
                    return true;
                }
                if (txtWordEdit.Visible) {
                    txtWordEdit_KeyDown(txtGroupEdit, new KeyEventArgs(keyData));
                    return true;
                }
                break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnClipboardChanged() {
            base.OnClipboardChanged();
            tbbPaste.Enabled = CanPaste();
        }

        private void frmWordsEdit_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible) {
                if (FindingForm.rtfReplace.Visible) {
                    FindingForm.SetForm(FindFormFunction.ReplaceWord, InGame);
                }
                else {
                    FindingForm.SetForm(FindFormFunction.FindWord, InGame);
                }
            }
            if (MDIMain.infoGridScope == InfoGridScope.SelectedResource) {
                MDIMain.RefreshInfoGrid();
            }
        }

        private void frmWordsEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            e.Cancel = !AskClose();
        }

        private void frmWordsEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // ensure object is cleared and dereferenced

            if (EditWordList is not null) {
                EditWordList.Unload();
                EditWordList = null;
            }
            if (InGame) {
                // form stays in MDIChild collection until AFTER
                // FormClosed is complete; to avoid problems with 
                // warnings filter, need to set InUse to false BEFORE 
                // refreshing filters
                WEInUse = false;
                WordEditor = null;
                if (MDIMain.infoGridScope == InfoGridScope.OpenResources) {
                    MDIMain.RefreshInfoGrid();
                }
            }
        }

        private void frmWordsEdit_Resize(object sender, EventArgs e) {
            int newWidth = (ClientSize.Width - 15) / 2;

            dgGroups.Width = newWidth;
            dgWords.Width = newWidth;
            dgWords.Left = dgGroups.Right + 5;
            label1.Left = 5 + (dgGroups.Width - label1.Width) / 2;
            label2.Left = dgWords.Left + (dgWords.Width - label2.Width) / 2;
            dgWords.Height = ClientSize.Height - dgWords.Top - 5;
            if (GroupMode) {
                dgGroups.Height = dgWords.Height;
            }
        }

        private void frmWordsEdit_HelpRequested(object sender, HelpEventArgs hlpevent) {
            ShowHelp();
            hlpevent.Handled = true;
        }
        #endregion

        #region Menu Event Handlers
        internal void SetResourceMenu() {
            mnuRSave.Enabled = IsChanged;
            MDIMain.mnuRSep2.Visible = true;
            MDIMain.mnuRSep3.Visible = true;
            if (EditGame is null) {
                // no game is open
                MDIMain.mnuRImport.Enabled = false;
                mnuRExport.Text = "Save As ...";
                // mnuRProperties no change
                // mnuRMerge no change
                mnuRGroupCheck.Enabled = false;
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export WORDS.TOK" : "Save As ...";
                // mnuRProperties no change
                // mnuRMerge no change
                mnuRGroupCheck.Enabled = InGame;
            }
        }

        /// <summary>
        /// Resets all resource menu items so shortcut keys can work correctly.
        /// </summary>
        internal void ResetResourceMenu() {
            mnuRSave.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRProperties.Enabled = true;
            mnuRMerge.Enabled = true;
            mnuRGroupCheck.Enabled = true;
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            if (IsChanged) {
                SaveWords();
            }
        }

        internal void mnuRExport_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            ExportWords();
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            EditProperties();
        }

        private void mnuRMerge_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            // merge Words.tok from file
            bool RepeatAnswer = false;
            DialogResult MergeReplace = DialogResult.No;
            int addcount = 0, replacecount = 0;

            MDIMain.OpenDlg.Title = "Merge Vocabulary Words File";
            MDIMain.OpenDlg.Filter = "WORDS.TOK file|WORDS.TOK|All files (*.*)|*.*";
            MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Words", sOPENFILTER, 1);
            MDIMain.OpenDlg.DefaultExt = "";
            MDIMain.OpenDlg.FileName = "";
            MDIMain.OpenDlg.InitialDirectory = DefaultResDir;
            if (MDIMain.OpenDlg.ShowDialog() != DialogResult.OK) {
                return;
            }
            WinAGISettingsFile.WriteSetting("Words", sOPENFILTER, MDIMain.OpenDlg.FilterIndex);
            DefaultResDir = Path.GetDirectoryName(MDIMain.OpenDlg.FileName);

            ProgressWin = new(this) {
                Text = "Merging from File"
            };

            WordList MergeList;
            ProgressWin.lblProgress.Text = "Merging...";
            ProgressWin.pgbStatus.Maximum = 0;
            ProgressWin.pgbStatus.Value = 0;
            ProgressWin.Show();
            ProgressWin.Refresh();
            MDIMain.UseWaitCursor = true;

            // load the merge list
            try {
                MergeList = new(MDIMain.OpenDlg.FileName, false);
                ProgressWin.pgbStatus.Maximum = MergeList.WordCount;
            }
            catch (Exception ex) {
                ProgressWin.Close();
                ErrMsgBox(ex,
                    "An error occurred while trying to load " +
                    Path.GetFileName(MDIMain.OpenDlg.FileName) + ":",
                    ex.StackTrace + "Unable to merge the file.",
                    "Merge Word List Error");
                MDIMain.UseWaitCursor = false;
                return;
            }
            for (int i = 0; i < MergeList.WordCount; i++) {
                // get word and group
                int GroupNum = MergeList[i].Group;
                string MergeWord = MergeList[i].WordText;
                if (EditWordList.WordExists(MergeWord)) {
                    int OldGroup = EditWordList[MergeWord].Group;
                    if (OldGroup != GroupNum) {
                        if (!RepeatAnswer) {
                            // get decision from user
                            MergeReplace = MsgBoxEx.Show(MDIMain,
                                '"' + MergeWord + "\" already exists in Group " + OldGroup + ". Do you want to move it to group " + GroupNum + "?",
                                "Replace Word?",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                "Repeat this answer for all duplicate words", ref RepeatAnswer,
                                WinAGIHelp, "htm\\winagi\\editor_words.htm#merge");
                        }
                        if (MergeReplace == DialogResult.Yes) {
                            // remove it from previous group
                            EditWordList.RemoveWord(MergeWord);
                            // add it to new group
                            EditWordList.AddWord(MergeWord, GroupNum);
                            replacecount++;
                        }
                    }
                }
                else {
                    // not in current list- ok to add
                    EditWordList.AddWord(MergeWord, GroupNum);
                    addcount++;
                }
                ProgressWin.pgbStatus.Value++;
                ProgressWin.Refresh();
            }
            MDIMain.UseWaitCursor = false;
            ProgressWin.Close();
            string msg = "";
            if (addcount > 0) {
                // refresh form
                dgGroups.Rows.Clear();
                dgWords.Rows.Clear();
                if (GroupMode) {
                    for (int i = 0; i < EditWordList.GroupCount; i++) {
                        dgGroups.Rows.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                    }
                }
                else {
                    foreach (AGIWord word in EditWordList) {
                        AddWordToGrid(word.WordText);
                    }
                }
                UpdateSelection(0, 0, true);
                msg = "Added " + addcount + " words. Replaced " + replacecount + " words.";
                MarkAsChanged();
            }
            else {
                msg = "No new words from the merge list were added.";
            }
            MessageBox.Show(MDIMain,
                msg,
                "Merge Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void mnuRGroupCheck_Click(object sender, EventArgs e) {
            //  word usage check- find words not used in a game
            if (EditingGroup || EditingWord) {
                return;
            }
            if (!InGame) {
                return;
            }

            bool dontAsk = false;
            AskOption repeatAnswer = AskOption.Ask;
            DialogResult rtn = DialogResult.No;
            bool[] GroupUsed = [];
            int UnusedCount = 0;

            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.IsChanged) {
                        switch (repeatAnswer) {
                        case AskOption.Ask:
                            rtn = MsgBoxEx.Show(MDIMain,
                            "Do you want to save this logic before checking word usage?",
                            "Update " + ResourceName(frm.EditLogic, true, true) + "?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            "Repeat this answer for all other open logics.", ref dontAsk);
                            if (dontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    repeatAnswer = AskOption.Yes;
                                }
                                else if (rtn == DialogResult.No) {
                                    repeatAnswer = AskOption.No;
                                }
                            }
                            break;
                        case AskOption.No:
                            rtn = DialogResult.No;
                            break;
                        case AskOption.Yes:
                            rtn = DialogResult.Yes;
                            break;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            frm.Select();
                            frm.SaveLogicSource();
                            break;
                        }
                    }
                }
            }
            MDIMain.UseWaitCursor = true;
            Array.Resize(ref GroupUsed, EditWordList.GroupCount);
            foreach (Logic tmpLogic in EditGame.Logics) {
                bool tmpLoad = tmpLogic.Loaded;
                if (!tmpLoad) {
                    tmpLogic.Load();
                }
                // find all said commands, mark words/groups as in use
                if (tmpLogic.SourceText.Contains("said")) {
                    AGIToken token = WinAGIFCTB.TokenFromPos(tmpLogic.SourceText, 0);
                    do {
                        if (token.Text != "said") {
                            token = WinAGIFCTB.NextToken(tmpLogic.SourceText, token);
                            continue;
                        }
                        token = WinAGIFCTB.NextToken(tmpLogic.SourceText, token);
                        if (token.Text != "(") {
                            token = WinAGIFCTB.NextToken(tmpLogic.SourceText, token);
                            continue;
                        }
                        do {
                            // get word arguments; skip commas
                            token = WinAGIFCTB.NextToken(tmpLogic.SourceText, token);
                            switch (token.Text) {
                            case ",":
                                continue;
                            case ")":
                                continue;
                            default:
                                if (int.TryParse(token.Text, out int group)) {
                                    if (EditWordList.GroupExists(group)) {
                                        GroupUsed[EditWordList.GroupIndexFromNumber(group)] = true;
                                    }
                                }
                                else {
                                    // expect word in quotes
                                    if (token.Text.Length > 2) {
                                        string word = token.Text;
                                        if (word[0] == '"') {
                                            word = word[1..];
                                            if (word[^1] == '"') {
                                                word = word[..^1];
                                                if (EditWordList.WordExists(word)) {
                                                    GroupUsed[EditWordList.GroupIndexFromNumber(EditWordList[word].Group)] = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        } while (token.Type != AGITokenType.None && token.Text != ")");
                    } while (token.Type != AGITokenType.None);
                }
                if (!tmpLoad) {
                    tmpLogic.Unload();
                }
            }
            List<string> stlOutput = new();
            //  go through all groups, make list of any that are unused
            for (int i = 0; i < GroupUsed.Length; i++) {
                if (!GroupUsed[i]) {
                    // skip 0, 1, 9999
                    switch (EditWordList.GroupByIndex(i).GroupNum) {
                    case 0 or 1 or 9999:
                        break;
                    default:
                        stlOutput.Add(EditWordList.GroupByIndex(i).GroupNum.ToString().PadLeft(5) + "  " + EditWordList.GroupByIndex(i).GroupName);
                        UnusedCount++;
                        break;
                    }
                }
            }
            stlOutput.Add("");
            MDIMain.UseWaitCursor = false;
            if (UnusedCount == 0) {
                MessageBox.Show(MDIMain,
                    "All word groups are used in this game.",
                    "No Unused Words",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else {
                Clipboard.Clear();
                Clipboard.SetText(stlOutput.Text(Environment.NewLine), TextDataFormat.UnicodeText);
                string counttext;
                if (UnusedCount == 1) {
                    counttext = "is one unused word group";
                }
                else {
                    counttext = "are " + UnusedCount + " unused word groups";
                }
                MessageBox.Show(MDIMain,
                    "There " + counttext + " in this game. \r\r" +
                    "The full list has been copied to the clipboard.",
                    "Unused Word Groups Check Results",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void SetEditMenu() {
            mnuEUndo.Visible = true;
            mnuESep1.Visible = true;
            if (UndoCol.Count > 0) {
                mnuEUndo.Enabled = true;
                mnuEUndo.Text = "Undo " + EditorResourceByNum(WORDSUNDOTEXT + (int)UndoCol.Peek().Action);
            }
            else {
                mnuEUndo.Enabled = false;
                mnuEUndo.Text = "Undo";
            }
            // cut is enabled if mode is group or word, and a group or word is selected
            //   NOT grp 0, 1, 1999
            mnuECut.Visible = true;
            if (dgGroups.Focused) {
                mnuECut.Text = "Cut Group";
                mnuECut.Enabled = EditGroupNumber != -1 &&
                    EditGroupNumber != 0 &&
                    EditGroupNumber != 1 &&
                    EditGroupNumber != 9999;
                mnuECopy.Text = "Copy Group";
                mnuECopy.Enabled = (EditGroupNumber != -1);
                mnuEDelete.Enabled = mnuECut.Enabled;
                mnuEDelete.Text = "Delete Group";
            }
            else {
                mnuECut.Text = "Cut Word";
                mnuECut.Enabled = true;
                // for group 0, 1, 9999 disable if no words in group
                if (EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) {
                    if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                        mnuECut.Enabled = false;
                    }
                }
                mnuECopy.Text = "Copy Word";
                mnuECopy.Enabled = EditWordIndex != -1;
                mnuEDelete.Text = "Delete Word";
                mnuEDelete.Enabled = mnuECut.Enabled;
            }
            // paste if something on clipboard
            if (EditingGroup || EditingWord) {
                mnuEPaste.Enabled = false;
                mnuEPaste.Text = "Paste";
            }
            else if (dgGroups.Focused) {
                mnuEPaste.Enabled = Clipboard.ContainsData(WORDSTOK_CB_FMT);
                mnuEPaste.Text = "Paste Group";
            }
            else {
                if (Clipboard.ContainsData(WORDSTOK_CB_FMT)) {
                    mnuEPaste.Enabled = true;
                    mnuEPaste.Text = "Paste Word";
                }
                else if (Clipboard.ContainsText()) {
                    mnuEPaste.Enabled = true;
                    mnuEPaste.Text = "Paste As Word";
                }
                else {
                    mnuEPaste.Enabled = false;
                    mnuEPaste.Text = "Paste Word";
                }
                // EXCEPT no pasting to group 1 or 9999 if already one word
                if (EditGroupNumber == 1 || EditGroupNumber == 9999) {
                    if (EditWordList.GroupByNumber(EditGroupNumber).WordCount >= 1) {
                        mnuEPaste.Enabled = false;
                    }
                }
            }
            // clear - always available
            mnuEInsertGroup.Visible = dgGroups.Focused;
            mnuEInsertWord.Visible = dgWords.Focused;
            mnuEInsertWord.Enabled = true;
            // if group 1 or 9999 insert only allowed if empty
            if (EditGroupNumber == 1 || EditGroupNumber == 9999) {
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount != 0) {
                    mnuEInsertWord.Enabled = false;
                }
            }
            // find again depends on search status
            mnuEFindAgain.Enabled = GFindText.Length > 0;
            if (dgGroups.Focused) {
                mnuEditItem.Text = "Renumber Group";
                mnuEditItem.Enabled = EditGroupNumber > 1 &&
                    EditGroupNumber != 9999;
            }
            else {
                mnuEditItem.Text = "Edit Word";
                if (EditGroupNumber == -1) {
                    mnuEditItem.Enabled = false;
                }
                else if (EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) {
                    mnuEditItem.Enabled = EditWordList.GroupByNumber(EditGroupNumber).WordCount > 0;
                }
                else {
                    mnuEditItem.Enabled = true;
                }
            }
            if (dgGroups.Focused) {
                mnuEFindInLogic.Enabled = EditGroupNumber >= 0;
            }
            else {
                mnuEFindInLogic.Enabled = EditWordText.Length > 0;
            }
            if (GroupMode) {
                mnuEditMode.Text = "Display By Word";
            }
            else {
                mnuEditMode.Text = "Display By Group";
            }
        }

        private void ResetEditMenu() {
            mnuEUndo.Enabled = true;
            mnuECut.Enabled = true;
            mnuECopy.Enabled = true;
            mnuEPaste.Enabled = true;
            mnuEDelete.Enabled = true;
            mnuEClear.Enabled = true;
            mnuEInsertGroup.Enabled = true;
            mnuEInsertWord.Enabled = true;
            mnuEFind.Enabled = true;
            mnuEFindAgain.Enabled = true;
            mnuEReplace.Enabled = true;
            mnuEditItem.Enabled = true;
            mnuEFindInLogic.Enabled = true;
            mnuEditMode.Enabled = true;
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            mnuEdit.DropDownItems.AddRange([mnuEUndo, mnESep0, mnuECut, mnuECopy, mnuEPaste, mnuEDelete, mnuEClear, mnuEInsertGroup, mnuEInsertWord, mnuESep1, mnuEFind, mnuEFindAgain, mnuEReplace, mnuESep2, mnuEditItem, mnuEFindInLogic, mnuEditMode]);
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            cmLists.Items.AddRange([mnuEUndo, mnESep0, mnuECut, mnuECopy, mnuEPaste, mnuEDelete, mnuEClear, mnuEInsertGroup, mnuEInsertWord, mnuESep1, mnuEFind, mnuEFindAgain, mnuEReplace, mnuESep2, mnuEditItem, mnuEFindInLogic, mnuEditMode]);
            ResetEditMenu();
        }

        private void cmWords_Opening(object sender, CancelEventArgs e) {
            if (EditingGroup || EditingWord) {
                e.Cancel = true;
                return;
            }
            SetEditMenu();
        }

        private void cmWords_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEUndo_Click(object sender, EventArgs e) {
            if (UndoCol.Count == 0) {
                return;
            }
            WordsUndo NextUndo = UndoCol.Pop();
            tbbUndo.Enabled = UndoCol.Count > 0;
            switch (NextUndo.Action) {
            case WordsUndo.ActionType.AddGroup:
            case WordsUndo.ActionType.PasteGroup:
                DeleteGroup(NextUndo.GroupNo, true);
                if (EditGroupNumber == NextUndo.GroupNo) {
                    UpdateSelection(0, 0, true);
                }
                break;
            case WordsUndo.ActionType.DelGroup:
            case WordsUndo.ActionType.CutGroup:
                AddGroup(NextUndo.GroupNo, true);
                for (int i = 0; i < NextUndo.Group.Length; i++) {
                    int index = EditWordList.AddWord(NextUndo.Group[i], NextUndo.GroupNo);
                    if (!GroupMode) {
                        AddWordToGrid(NextUndo.Group[i], index);
                    }
                }
                UpdateSelection(NextUndo.GroupNo, 0);
                break;
            case WordsUndo.ActionType.AddWord:
            case WordsUndo.ActionType.PasteWord:
                DeleteWord(NextUndo.Word, true);
                if (NextUndo.OldGroupNo != -1) {
                    if (!EditWordList.GroupExists(NextUndo.OldGroupNo)) {
                        AddGroup(NextUndo.OldGroupNo, true);
                    }
                    AddWord(NextUndo.OldGroupNo, NextUndo.Word, true);
                }
                UpdateSelection(NextUndo.GroupNo, 0);
                UpdateGroupName(NextUndo.GroupNo);
                break;
            case WordsUndo.ActionType.DelWord:
            case WordsUndo.ActionType.CutWord:
                if (!EditWordList.GroupExists(NextUndo.GroupNo)) {
                    AddGroup(NextUndo.GroupNo, true);
                }
                if (EditWordList.GroupByNumber(NextUndo.GroupNo).WordCount == 0) {
                    if (GroupMode) {
                        dgWords.Rows.Clear();
                    }
                }
                AddWord(NextUndo.GroupNo, NextUndo.Word, true);
                UpdateSelection(NextUndo.Word);
                break;
            case WordsUndo.ActionType.MoveWord:
                MoveWord(NextUndo.Word, NextUndo.OldGroupNo, true);
                UpdateSelection(NextUndo.Word, true);
                break;
            case WordsUndo.ActionType.Renumber:
                RenumberGroup(NextUndo.GroupNo, NextUndo.OldGroupNo, true);
                if (NextUndo.GroupNo == EditGroupNumber) {
                    UpdateSelection(NextUndo.OldGroupNo, EditWordGroupIndex, true);
                }
                else {
                    UpdateSelection(NextUndo.OldGroupNo, 0, true);
                }
                break;
            case WordsUndo.ActionType.ChangeWord:
            case WordsUndo.ActionType.Replace:
                EditWord(NextUndo.Word, NextUndo.OldWord, NextUndo.GroupNo, true);
                if (NextUndo.OldGroupNo != -1) {
                    if (!EditWordList.GroupExists(NextUndo.OldGroupNo)) {
                        AddGroup(NextUndo.OldGroupNo, true);
                    }
                    AddWord(NextUndo.OldGroupNo, NextUndo.Word, true);
                }
                UpdateSelection(NextUndo.OldWord, true);
                break;
            case WordsUndo.ActionType.ReplaceAll:
                // the number of replaced words is stored in the GroupNo field
                int count = NextUndo.GroupNo;
                for (int i = 0; i < count; i++) {
                    NextUndo = UndoCol.Pop();
                    EditWord(NextUndo.Word, NextUndo.OldWord, NextUndo.GroupNo, true);
                    if (NextUndo.OldGroupNo != -1) {
                        if (!EditWordList.GroupExists(NextUndo.OldGroupNo)) {
                            AddGroup(NextUndo.OldGroupNo, true);
                        }
                        AddWord(NextUndo.OldGroupNo, NextUndo.Word, true);
                    }
                }
                UpdateSelection(0, true);
                break;
            case WordsUndo.ActionType.Clear:
                EditWordList.Clear();
                for (int i = 0; i < NextUndo.Group.Length; i++) {
                    string[] words = NextUndo.Group[i].Split("|");
                    int groupnum = int.Parse(words[0]);
                    for (int j = 1; j < words.Length; j++) {
                        EditWordList.AddWord(words[j], groupnum);
                    }
                }
                dgGroups.Rows.Clear();
                dgWords.Rows.Clear();
                if (GroupMode) {
                    for (int i = 0; i < EditWordList.GroupCount; i++) {
                        dgGroups.Rows.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                    }
                }
                else {
                    foreach (AGIWord word in EditWordList) {
                        AddWordToGrid(word.WordText);
                    }
                }
                UpdateSelection(0, 0, true);
                break;
            }
            MarkAsChanged();
        }

        private void mnuECut_Click(object sender, EventArgs e) {
            if (CanCut()) {
                mnuECopy_Click(sender, e);
                DeleteGroupOrWord();
                if (UndoCol.Peek().Action == WordsUndo.ActionType.DelGroup) {
                    UndoCol.Peek().Action = WordsUndo.ActionType.CutGroup;
                }
                else if (UndoCol.Peek().Action == WordsUndo.ActionType.DelWord) {
                    UndoCol.Peek().Action = WordsUndo.ActionType.CutWord;
                }
            }
        }

        private bool CanCut(bool confirmspecial = false) {
            // cut(delete) is enabled if mode is group or word, and a group or word is selected
            //   NOT grp 0, 1, 1999
            if (EditingGroup || EditingWord) {
                return false;
            }
            if (dgGroups.Focused) {
                return EditGroupNumber != -1 &&
                    EditGroupNumber != 0 &&
                    EditGroupNumber != 1 &&
                    EditGroupNumber != 9999;
            }
            else {
                if (EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) {
                    // for group 0, 1, 9999 disable if no words in group
                    if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                        return false;
                    }
                    // for group 0, a and i are special
                    // for group 1 and 9999 confirm before deleting default placeholder
                    switch (EditGroupNumber) {
                    case 0:
                        // 'a' and 'i' are special
                        if (EditWordText == "a" || EditWordText == "i") {
                            if (MDIMain.MsgBoxWithHelp(
                                $"The word '{EditWordText}' is usually associated with group 0. Are " +
                                "you sure you want to delete it?",
                                "Delete Group 0 Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                "htm\\winagi\\editor_words.htm#reserved") == DialogResult.No) {
                                return false;
                            }
                        }
                        break;
                    case 1:
                        // 'anyword' is special
                        if (EditWordText == "anyword") {
                            if (MDIMain.MsgBoxWithHelp(
                                $"The word 'anyword' is the Sierra default placeholder word for reserved group 1. " +
                                "Deleting it is not advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 1 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                "htm\\winagi\\editor_words.htm#reserved") == DialogResult.No) {
                                return false;
                            }
                        }
                        else if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 1) {
                            if (MDIMain.MsgBoxWithHelp(
                                $"Group '1' is a reserved group. Deleting its placeholder is not " +
                                "advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 1 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                "htm\\winagi\\editor_words.htm#reserved") == DialogResult.No) {
                                return false;
                            }
                        }
                        break;
                    case 9999:
                        // 'rol' is special
                        if (EditWordText == "rol") {
                            if (MDIMain.MsgBoxWithHelp(
                                $"The word 'rol' is the Sierra default placeholder word for reserved group 9999. " +
                                "Deleting it is not advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 9999 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                "htm\\winagi\\editor_words.htm#reserved") == DialogResult.No) {
                                return false;
                            }
                        }
                        else if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 1) {
                            if (MDIMain.MsgBoxWithHelp(
                                $"Group '9999' is a reserved group. Deleting its placeholder is not " +
                                "advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 9999 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                "htm\\winagi\\editor_words.htm#reserved") == DialogResult.No) {
                                return false;
                            }
                        }
                        break;
                    }
                }
                return true;
            }
        }

        private void mnuECopy_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord ||
                EditGroupNumber == -1 || EditWordIndex == -1) {
                return;
            }
            if (dgGroups.Focused) {
                // add a word group as custom data
                WordClipboardData groupdata = new();
                groupdata.IsGroup = true;
                groupdata.GroupNumber = EditGroupNumber;
                groupdata.Words = new string[EditWordList.GroupByNumber(EditGroupNumber).WordCount];
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount > 0) {
                    int i;
                    for (i = 0; i < EditWordList.GroupByNumber(EditGroupNumber).WordCount; i++) {
                        groupdata.Words[i] = EditWordList.GroupByNumber(EditGroupNumber).Words[i];
                    }
                }
                // add custom data to clipboard
                DataObject dataObject = new();

                // add group number as text
                dataObject.SetData(DataFormats.Text, EditGroupNumber.ToString());

                // add group and words as a csv list
                string grpdata = EditGroupNumber.ToString();
                // add line returns for each word
                for (int i = 0; i < groupdata.Words.Length; i++) {
                    grpdata += "\r\n" + groupdata.Words[i];
                }
                // Convert the CSV text to a UTF-8 byte stream before adding it to the container object.
                var bytes = System.Text.Encoding.UTF8.GetBytes(grpdata);
                var stream = new System.IO.MemoryStream(bytes);
                dataObject.SetData(DataFormats.CommaSeparatedValue, stream);

                // add group data as custom format
                dataObject.SetData(WORDSTOK_CB_FMT, groupdata);

                // now add the combined clipboard data object
                Clipboard.SetDataObject(dataObject, true);
            }
            else if (dgWords.Focused) {
                // add word as  custom data to clipboard
                DataObject dataObject = new();
                WordClipboardData worddata = new();
                worddata.IsGroup = false;
                worddata.WordText = EditWordText;
                dataObject.SetData(DataFormats.Text, '"' + EditWordText + '"');
                dataObject.SetData(WORDSTOK_CB_FMT, worddata);
                Clipboard.SetDataObject(dataObject, true);
            }
        }

        private void mnuEPaste_Click(object sender, EventArgs e) {
            string msgtext = "";
            if (CanPaste()) {
                if (dgGroups.Focused) {
                    WordClipboardData groupdata = Clipboard.GetData(WORDSTOK_CB_FMT) as WordClipboardData;
                    if (groupdata is null) {
                        // no custom clipboard data
                        MessageBox.Show(MDIMain,
                            "The clipboard doesn't contain a valid group.",
                            "Nothing to Paste",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else {
                        // clipboard contains a single group
                        WordsUndo NextUndo = new() {
                            Action = WordsUndo.ActionType.PasteGroup,
                            Group = []
                        };
                        for (int i = 0; i < groupdata.Words.Length; i++) {
                            if (EditWordList.WordExists(groupdata.Words[i])) {
                                // word is in use
                                msgtext += "\r\t" + groupdata.Words[i] + " (in group " + EditWordList[groupdata.Words[i]].Group + ")";
                            }
                            else {
                                // add it to undo object
                                Array.Resize(ref NextUndo.Group, NextUndo.Group.Length + 1);
                                NextUndo.Group[^1] = groupdata.Words[i];
                            }
                        }
                        if (NextUndo.Group.Length == 0) {
                            // nothing to add
                            MessageBox.Show(MDIMain,
                                "No words on the clipboard could be pasted; all of them are already in this word list.",
                                "Nothing to Paste",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            return;
                        }
                        int newgroupnum;
                        if (EditWordList.GroupExists(groupdata.GroupNumber)) {
                            newgroupnum = NextGrpNum();
                        }
                        else {
                            newgroupnum = groupdata.GroupNumber;
                        }
                        // add group without undo
                        AddGroup(newgroupnum, true);
                        for (int i = 0; i < NextUndo.Group.Length; i++) {
                            // add word (without undo)
                            AddWord(newgroupnum, NextUndo.Group[i], true);
                        }
                        NextUndo.GroupNo = newgroupnum;
                        // the words added aren't needed
                        NextUndo.Group = [];
                        AddUndo(NextUndo);
                        if (msgtext.Length > 0) {
                            MessageBox.Show(MDIMain,
                                "The following words were not added because they already exist in another group: " + msgtext,
                                "Paste Group from Clipboard",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        // select the pasted group
                        UpdateSelection(newgroupnum, 0);
                    }
                    return;
                }
                if (dgWords.Focused) {
                    string word = "";
                    // check clipboard for custom data
                    WordClipboardData worddata = Clipboard.GetData(WORDSTOK_CB_FMT) as WordClipboardData;
                    string clipboardword = "";
                    if (worddata is not null && !worddata.IsGroup) {
                        // clipboard contains a single word
                        clipboardword = worddata.WordText;
                    }
                    else if (Clipboard.ContainsText(TextDataFormat.UnicodeText)) {
                        word = Clipboard.GetText(TextDataFormat.UnicodeText).LowerAGI();
                        if (word.Contains('\n') || word.Contains("\r")) {
                            word = "";
                        }
                        if (word.Length > 0 && word[0] == '"') {
                            word = word[1..];
                        }
                        if (word.Length > 0 && word[^1] == '"') {
                            word = word[..^1];
                        }
                        if (word.Length > 0) {
                            word = CheckWord(word);
                        }
                        if (word.Length > 0) {
                            // this word is acceptable
                            clipboardword = word;
                        }
                        else {
                            MessageBox.Show(MDIMain,
                                "The clipboard doesn't contain a valid word.",
                                "Nothing to Paste",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                    if (clipboardword.Length > 0) {
                        int oldgroupnum;
                        if (EditWordList.WordExists(clipboardword)) {
                            oldgroupnum = EditWordList[clipboardword].Group;
                            if (oldgroupnum == EditGroupNumber) {
                                MessageBox.Show(MDIMain,
                                    "'" + clipboardword + "' already exists in this group.",
                                    "Unable to Paste",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                return;
                            }
                            // word is in another group- ask if word should be moved
                            if (MessageBox.Show("'" + clipboardword + "' already exists (in group " + oldgroupnum + "). Do you want to move it to this group?",
                                "Move Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.No) {
                                return;
                            }
                            // delete word from other group
                            DeleteWord(clipboardword, true);
                        }
                        else {
                            oldgroupnum = -1;
                        }
                        // add word to this group
                        AddWord(EditGroupNumber, clipboardword, true);

                        // add undo
                        WordsUndo NextUndo = new();
                        NextUndo.Action = WordsUndo.ActionType.PasteWord;
                        NextUndo.GroupNo = EditGroupNumber;
                        NextUndo.OldGroupNo = oldgroupnum;
                        NextUndo.Word = clipboardword;
                        AddUndo(NextUndo);

                        // select the pasted word
                        UpdateSelection(clipboardword);
                    }
                }
            }

            static string CheckWord(string wordtext) {
                string retval = wordtext.SingleSpace().Trim().LowerAGI();
                if ("!\"'(),-.:;?[]`{}".Any(retval.Contains)) {
                    return "";
                }
                if ("#$%&*+/0123456789<=>@\\^_|~".Contains(retval[0])) {
                    // not allowed unless supporting power pack
                    if (EditGame is null || !EditGame.PowerPack) {
                        return "";
                    }
                }
                // extended characters
                if (retval.Any(c => c > 127)) {
                    // not allowed unless supporting power pack
                    if (EditGame is null || !EditGame.PowerPack) {
                        return "";
                    }
                }
                // new word is ok
                return retval;
            }
        }

        private bool CanPaste() {
            if (EditingGroup || EditingWord) {
                return false;
            }
            if (dgGroups.Focused) {
                return Clipboard.ContainsData(WORDSTOK_CB_FMT);
            }
            else {
                return Clipboard.ContainsText() || Clipboard.ContainsData(WORDSTOK_CB_FMT);
            }
        }

        private void mnuEDelete_Click(object sender, EventArgs e) {
            if (CanCut()) {
                DeleteGroupOrWord();
            }
        }

        private void mnuEClear_Click(object sender, EventArgs e) {
            ClearWordList();
        }

        private void mnuEAddGroup_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            NewGroup();
        }

        private void mnuEAddWord_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord ||
                EditGroupNumber == -1) {
                return;
            }
            NewWord(EditGroupNumber);
        }

        private void mnuEditItem_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            if (sender == tbbRenumber || dgGroups.Focused) {
                dgGroups_DoubleClick(sender, e);
            }
            if (dgWords.Focused) {
                dgWords_DoubleClick(sender, e);
            }
        }

        private void mnuEFind_Click(object sender, EventArgs e) {
            StartSearch(FindFormFunction.FindWord);
        }

        private void mnuEFindAgain_Click(object sender, EventArgs e) {
            if (GFindText.Length == 0) {
                StartSearch(FindFormFunction.FindWord);
            }
            else {
                FindInWords(GFindText, GFindDir, GMatchWord);
            }
        }

        private void mnuEReplace_Click(object sender, EventArgs e) {
            StartSearch(FindFormFunction.ReplaceWord);
        }

        private void mnuEFindLogic_Click(object sender, EventArgs e) {
            if (!InGame || EditWordText.Length == 0) {
                return;
            }
            FindingForm.ResetSearch();
            FirstFind = false;
            GFindDir = FindDirection.All;
            GMatchWord = true;
            GMatchCase = true;
            GLogFindLoc = FindLocation.All;
            GFindSynonym = dgGroups.Focused;
            GFindGrpNum = EditGroupNumber;
            if (EditGame is null || !EditGame.SierraSyntax) {
                GFindText = '"' + EditWordText + '"';
            }
            else {
                GFindText = EditWordText.Replace(' ', '$');
            }
            SearchType = AGIResType.Words;
            FindingForm.SetForm(FindFormFunction.FindWordsLogic, true);
            // to avoid unwanted change in form function, don't assign text
            // cmbFind directly
            FindingForm.SetFindText(GFindText);
            if (!FindingForm.Visible) {
                FindingForm.Visible = true;
            }
            FindingForm.Select();
        }

        private void mnuEMode_Click(object sender, EventArgs e) {
            byte[] buttonicon;

            FindingForm.ResetSearch();
            FirstFind = false;
            GroupMode = !GroupMode;
            if (GroupMode) {
                label1.Text = "Groups";
                dgGroups.Height = dgWords.Height;
                dgGroups.Rows.Clear();
                for (int i = 0; i < EditWordList.GroupCount; i++) {
                    dgGroups.Rows.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                }
                dgGroups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                SelectGroup(EditGroupIndex);
                dgWords.Rows.Clear();
                for (int i = 0; i < EditWordList.GroupByIndex(EditGroupIndex).WordCount; i++) {
                    AddWordToGrid(EditWordList.GroupByIndex(EditGroupIndex).Words[i]);
                }
                SelectWord(EditWordText);
                buttonicon = (byte[])EditorResources.ResourceManager.GetObject("ewi_bygroup");
            }
            else {
                label1.Text = "Group Number:";
                dgGroups.Rows.Clear();
                dgGroups.Height = txtGroupEdit.Height + 4;
                dgGroups.SelectionMode = DataGridViewSelectionMode.CellSelect;
                dgWords.Rows.Clear();
                foreach (AGIWord word in EditWordList) {
                    AddWordToGrid(word.WordText);
                }
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                    dgGroups.Rows.Add("");
                    UpdateSelection(EditWordList[0].WordText, true);
                }
                else {
                    dgGroups.Rows.Add((EditGroupNumber.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName);
                }
                SelectWord(EditWordText);
                buttonicon = (byte[])EditorResources.ResourceManager.GetObject("ewi_byword");
            }
            Stream stream = new MemoryStream(buttonicon);
            tbbMode.Image = (Bitmap)Image.FromStream(stream);
            label1.Left = 5 + (dgGroups.Width - label1.Width) / 2;
        }

        private void cmUndo_Click(object sender, EventArgs e) {
            if (sender == cmGroupEdit) {
                txtGroupEdit.Undo();
            }
            else {
                txtWordEdit.Undo();
            }
        }

        private void cmCut_Click(object sender, EventArgs e) {
            if (sender == cmGroupEdit) {
                txtGroupEdit.Cut();
            }
            else {
                txtWordEdit.Cut();
            }
        }

        private void cmCopy_Click(object sender, EventArgs e) {
            if (sender == cmGroupEdit) {
                txtGroupEdit.Copy();
            }
            else {
                txtWordEdit.Copy();
            }
        }

        private void cmPaste_Click(object sender, EventArgs e) {
            if (sender == cmGroupEdit) {
                txtGroupEdit.Paste();
            }
            else {
                txtWordEdit.Paste();
            }
        }

        private void cmDelete_Click(object sender, EventArgs e) {
            TextBox textbox;
            if (sender == cmGroupEdit) {
                textbox = txtGroupEdit;
            }
            else {
                textbox = txtWordEdit;
            }
            if (textbox.SelectionLength > 0) {
                textbox.SelectedText = "";
            }
            else {
                if (textbox.SelectionStart < textbox.Text.Length) {
                    int oldsel = textbox.SelectionStart;
                    textbox.Text = textbox.Text[..oldsel] + textbox.Text[(oldsel + 1)..];
                    textbox.SelectionStart = oldsel;
                }
            }
        }

        private void cmCharMap_Click(object sender, EventArgs e) {
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
                    txtWordEdit.SelectedText = CharPicker.InsertString;
                }
            }
            CharPicker.Close();
            CharPicker.Dispose();
        }

        private void cmSelectAll_Click(object sender, EventArgs e) {
            if (sender != cmGroupEdit) {
                txtGroupEdit.SelectAll();
            }
            else {
                txtWordEdit.SelectAll();
            }
        }

        private void cmCancel_Click(object sender, EventArgs e) {
            if (sender == cmGroupEdit) {
                FinishGroupEdit();
            }
            else {
                FinishWordEdit();
            }

        }
        #endregion

        #region Control Event Handlers
        private void dgGroups_DoubleClick(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            // groups 0, 1, 9999 cannot be edited
            if (EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) {
                return;
            }
            int index;
            if (GroupMode) {
                index = EditGroupIndex;
            }
            else {
                index = 0;
            }
            EditingGroup = true;
            // configure the TextBox for in-place editing
            txtGroupEdit.Text = EditGroupNumber.ToString();
            Point location = dgGroups.GetCellDisplayRectangle(0, index, true).Location;
            location.Offset(dgGroups.Location + new Size(3, 2));
            txtGroupEdit.Location = location;
            txtGroupEdit.Size = dgGroups.GetCellDisplayRectangle(0, index, true).Size;
            dgGroups.SelectedCells[0].Style.SelectionBackColor = SystemColors.Window;
            txtGroupEdit.Visible = true;
            txtGroupEdit.Select();
        }

        private void dgGroups_Enter(object sender, EventArgs e) {
            label1.Font = boldfont;
            SetToolbarStatus();
        }

        private void dgGroups_Leave(object sender, EventArgs e) {
            if (dgWords.Focused) {
                label1.Font = defaultfont;
            }
        }

        private void dgGroups_MouseDown(object sender, MouseEventArgs e) {
            if (GroupMode) {
                DataGridView.HitTestInfo info = dgGroups.HitTest(e.X, e.Y);
                if (info is not null) {
                    int selitem = info.RowIndex;
                    if (selitem >= dgGroups.Rows.Count) {
                        selitem = dgGroups.Rows.Count - 1;
                    }
                    if (selitem < 0) {
                        return;
                    }
                    if (EditGroupIndex != selitem) {
                        UpdateSelection(EditWordList.GroupByIndex(selitem).GroupNum, 0, true);
                        // always reset search
                        StartWord = -1;
                        StartGrp = -1;
                    }
                }
            }
            if (e.Button == MouseButtons.Right) {
                dgGroups.Select();
            }
        }

        private void dgGroups_MouseUp(object sender, MouseEventArgs e) {
            if (GroupMode && dgGroups.SelectedRows[0].Index != EditGroupIndex) {
                UpdateSelection(EditWordList.GroupByIndex(dgGroups.SelectedRows[0].Index).GroupNum, 0, true);
            }
        }

        private void dgGroups_DragEnter(object sender, DragEventArgs e) {
            if (GroupMode && DragWord) {
                if (!this.Focused) {
                    this.Select();
                }
                e.Effect = DragDropEffects.Move;
            }
            else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void dgGroups_DragOver(object sender, DragEventArgs e) {
            if (GroupMode && DragWord) {
                // convert screen coordinates to dgGroup coordinates
                Point cP = dgGroups.PointToClient(new Point(e.X, e.Y));
                DataGridView.HitTestInfo info = dgGroups.HitTest(cP.X, cP.Y);
                int selitem;
                if (info is not null) {
                    // handle scrolling manually
                    if (cP.Y < 3) {
                        if (dgGroups.FirstDisplayedScrollingRowIndex == 0) {
                            return;
                        }
                        selitem = dgGroups.FirstDisplayedScrollingRowIndex - 1;
                        dgGroups.FirstDisplayedScrollingRowIndex = selitem;
                    }
                    else if (cP.Y > dgGroups.ClientRectangle.Height - 8) {
                        selitem = dgGroups.FirstDisplayedScrollingRowIndex + dgGroups.DisplayedRowCount(true);
                        if (selitem >= dgGroups.Rows.Count) {
                            return;
                        }
                        dgGroups.FirstDisplayedScrollingRowIndex++;
                    }
                    else {
                        // ignore it not on a cell
                        if (info.Type != DataGridViewHitTestType.Cell) {
                            return;
                        }
                        selitem = info.RowIndex;
                    }
                    if (selitem < 0 || selitem >= dgGroups.Rows.Count) {
                        return;
                    }
                    if (selitem != dgGroups.SelectedRows[0].Index) {
                        dgGroups.Rows[selitem].Selected = true;
                    }
                    // don't allow dropping on group 1 or 9999
                    if (selitem == 1 || EditWordList.GroupByIndex(selitem).GroupNum == 9999) {
                        e.Effect = DragDropEffects.None;
                    }
                    else {
                        e.Effect = DragDropEffects.Move;
                    }
                }
            }
        }

        private void dgGroups_DragLeave(object sender, EventArgs e) {
            Debug.Print("leave drag");
            if (GroupMode && DragWord) {
                if (dgGroups.SelectedRows[0].Index != EditGroupIndex) {
                    //SelectGroup(EditGroupIndex);
                }
            }
        }

        private void dgGroups_DragDrop(object sender, DragEventArgs e) {
            if (GroupMode) {
                string dropword = (string)e.Data.GetData(DataFormats.Text);
                if (dropword.Length > 0) {
                    int groupnum = EditWordList.GroupByIndex(dgGroups.SelectedRows[0].Index).GroupNum;
                    if (this == DragSourceForm) {
                        if (EditWordList[dropword].Group != groupnum) {
                            // check for last word of group 1 or 9999
                            bool ok2move = true;
                            int oldgroup = EditWordList[dropword].Group;
                            if (dropword == "anyword" && oldgroup == 1) {
                                if (MessageBox.Show(MDIMain,
                                    "The word 'anyword' is the Sierra default placeholder for group 1. " +
                                    "Do you want to move it to this group?",
                                    "Move Word",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No) {
                                    ok2move = false;
                                }
                            }
                            if (dropword == "rol" && oldgroup == 9999) {
                                if (MessageBox.Show(MDIMain,
                                    "The word 'rol' is the Sierra default placeholder for group 9999. " +
                                    "Do you want to move it to this group?",
                                    "Move Word",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No) {
                                    ok2move = false;
                                }
                            }
                            if (ok2move) {
                                MoveWord(dropword, groupnum);
                                UpdateSelection(dropword, true);
                            }
                            else {
                                SelectGroup(EditGroupIndex);
                            }
                        }
                    }
                    else {
                        // validate
                        switch (IsValidWord(dropword)) {
                        case 0:
                            break;
                        case 1:
                            MDIMain.MsgBoxWithHelp(
                                "This word contains one or more invalid characters.",
                                "Invalid Word",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                "htm\\agi\\words.htm#charlimits");
                            DragSourceForm = null;
                            return;
                        case 2:
                            // not allowed unless supporting power pack
                            if (!EditGame.PowerPack) {
                                MDIMain.MsgBoxWithHelp(
                                    "This word begins with invalid characters.",
                                    "Invalid Word",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error,
                                    "htm\\agi\\words.htm#charlimits");
                                DragSourceForm = null;
                                return;
                            }
                            break;
                        }
                        if (EditWordList.WordExists(dropword)) {
                            MessageBox.Show(MDIMain,
                                $"'{dropword}' is already present in this list. ",
                                "Duplicate Word",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            SelectGroup(EditGroupIndex);
                        }
                        else {
                            AddWord(groupnum, dropword);
                            UpdateSelection(dropword, true);
                        }
                    }
                }
                DragSourceForm = null;
            }
        }

        private void dgGroups_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
            if (GroupMode) {
                // format each row
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++) {
                    FormatGroupRow(dgGroups.Rows[i]);
                }
            }
        }

        private void txtGroupEdit_Validating(object sender, CancelEventArgs e) {
            if (!EditingGroup) {
                return;
            }
            int newgroup = ValidateGroup(txtGroupEdit.Text);
            if (newgroup == EditGroupNumber) {
                // no change
                FinishGroupEdit();
                return;
            }
            if (newgroup > 0) {
                RenumberGroup(EditGroupNumber, newgroup);
                EditingGroup = false;
            }
            else {
                // not a valid group number
                e.Cancel = true;
            }
        }

        private void txtGroupEdit_KeyDown(object sender, KeyEventArgs e) {
            // only numbers, backspace, enter, escape and delete are allowed
            switch (e.KeyCode) {
            case Keys.Enter:
                // validation will handle the group renumbering
                txtGroupEdit.Visible = false;
                if (EditingGroup) {
                    // validation failed
                    txtGroupEdit.Visible = true;
                    txtGroupEdit.SelectAll();
                    return;
                }
                dgGroups.SelectedCells[0].Style.SelectionBackColor = SystemColors.Highlight;
                dgGroups.Select();
                return;
            case Keys.Escape:
                FinishGroupEdit();
                break;
            case >= Keys.D0 and <= Keys.D9:
            case Keys.Back:
            case Keys.Left:
            case Keys.Right:
                break;
            default:
                e.SuppressKeyPress = true;
                e.Handled = true;
                break;
            }
        }

        private void txtGroupEdit_TextChanged(object sender, EventArgs e) {
            if (txtGroupEdit.Text.Length > 0 && !txtGroupEdit.Text.IsNumeric()) {
                txtGroupEdit.Text = EditGroupNumber.ToString();
            }
        }

        private void dgWords_DoubleClick(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            // 'a' or 'i' in grp 0, any word in grp 1, any word in grp 9999
            // normally not edited
            string msg = "";
            switch (EditGroupNumber) {
            case -1:
                // should not possible
                break;
            case 0:
                if (EditWordText == "a" || EditWordText == "i") {
                    msg = "The single letter words 'a' and 'i' should be left as is. Are you " +
                        "sure you want to edit this word?";
                }
                break;
            case 1:
                if (EditWordText == "anyword") {
                    msg = "'anyword' is the standard Sierra placeholder for group 1. Are you " +
                        "sure you want to edit this word?";
                }
                break;
            case 9999:
                if (EditWordText == "rol") {
                    msg = "'rol' is the standard Sierra placeholder for group 9999. Are you " +
                        "sure you want to edit this word?";
                }
                break;
            }
            if (msg.Length > 0) {
                if (MessageBox.Show(MDIMain,
                    msg,
                    "Edit Word",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No) {
                    return;
                }
            }
            BeginWordEdit();
        }

        private void dgWords_Enter(object sender, EventArgs e) {
            label2.Font = boldfont;
            SetToolbarStatus();
        }

        private void dgWords_Leave(object sender, EventArgs e) {
            if (dgGroups.Focused) {
                label2.Font = defaultfont;
            }
        }

        private void dgWords_MouseDown(object sender, MouseEventArgs e) {
            DataGridView.HitTestInfo info = dgWords.HitTest(e.X, e.Y);
            if (info is not null && info.Type == DataGridViewHitTestType.Cell) {
                int selitem = info.RowIndex;
                if (selitem >= dgWords.Rows.Count) {
                    selitem = dgWords.Rows.Count - 1;
                }
                if ((EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) &&
                    EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                    return;
                }
                // in groupmode, selindex corresponds to EditWordGroupIndex
                // in wordmode, selindex corresponds to EditWordIndex
                if (GroupMode) {
                    if (EditWordGroupIndex != selitem) {
                        UpdateSelection((string)dgWords.Rows[selitem].Cells[0].Value);
                        // always reset search
                        StartWord = -1;
                        StartGrp = -1;
                    }
                }
                else {
                    if (EditWordIndex != selitem) {
                        UpdateSelection((string)dgWords.Rows[selitem].Cells[0].Value);
                        // always reset search
                        StartWord = -1;
                        StartGrp = -1;
                    }
                }
            }
            if (e.Button == MouseButtons.Right) {
                dgWords.Select();
            }
        }

        private void dgWords_MouseUp(object sender, MouseEventArgs e) {
            if (GroupMode) {
                if (EditWordGroupIndex != dgWords.SelectedRows[0].Index) {
                    UpdateSelection((string)dgWords.Rows[dgWords.SelectedRows[0].Index].Cells[0].Value);
                }
            }
            else {
                if (EditWordIndex != dgWords.SelectedRows[0].Index) {
                    UpdateSelection((string)dgWords.Rows[dgWords.SelectedRows[0].Index].Cells[0].Value);
                }
            }
        }

        private void dgWords_MouseMove(object sender, MouseEventArgs e) {
            if (GroupMode && !DragWord && e.Button == MouseButtons.Left) {
                if (EditGroupNumber == 1 || EditGroupNumber == 9999) {
                    return;
                }
                if (EditGroupNumber == 0) {
                    if (EditWordText == "a" || EditWordText == "i") {
                        return;
                    }
                }
                DragWord = true;
                DragSourceForm = this;
                dgWords.DoDragDrop(EditWordText, DragDropEffects.Move);
            }
        }

        private void dgWords_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
            if (e.Action == DragAction.Drop) {
                DragWord = false;
            }
            else {
                // check if drag is close to either top or bottom edge
                // of the group list; if not, reset selection
                Point mp = new(MousePosition.X, MousePosition.Y);
                mp = dgGroups.PointToClient(mp);
                if (mp.X < 0 || mp.X > dgGroups.ClientRectangle.Width ||
                    mp.Y < -50 || mp.Y > dgGroups.ClientRectangle.Height + 50) {
                    if (dgGroups.SelectedRows[0].Index != EditGroupIndex) {
                        SelectGroup(EditGroupIndex);
                    }
                }
            }
        }

        private void txtWordEdit_Validating(object sender, CancelEventArgs e) {
            bool skipundo = false;

            if (AddNewGroup) {
                // skip undo if edit happens after adding a group
                skipundo = true;
                AddNewGroup = false;
            }
            string newword = ValidateWord(txtWordEdit.Text);
            if (newword == EditWordText) {
                // no change
                FinishWordEdit();
                return;
            }
            switch (newword) {
            case "":
                // same as delete
                EditingWord = false;
                txtWordEdit.Visible = false;
                DeleteWord(EditWordText);
                return;
            case "!":
                // invalid word
                e.Cancel = true;
                return;
            default:
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                    // it's group 0/1/9999 with no word
                    if (GroupMode) {
                        dgWords.Rows.Clear();
                    }
                    AddWord(EditGroupNumber, newword);
                }
                else {
                    // word is ok to change (if adding a new group, skip the undo)
                    EditWord(EditWordText, newword, EditGroupNumber, skipundo);
                }
                FinishWordEdit();
                break;
            }
        }

        private void txtWordEdit_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
            case Keys.Enter:
                // validation will handle the word modification
                txtWordEdit.Visible = false;
                if (EditingWord) {
                    // validation failed
                    txtWordEdit.Visible = true;
                    txtWordEdit.SelectAll();
                    return;
                }
                dgWords.SelectedCells[0].Style.SelectionBackColor = SystemColors.Highlight;
                dgWords.Select();
                return;
            case Keys.Escape:
                FinishWordEdit();
                break;
            case Keys.Back:
            case Keys.Left:
            case Keys.Right:
                break;
            default:
                break;
            }
        }

        private void txtWordEdit_KeyPress(object sender, KeyPressEventArgs e) {
            switch ((int)e.KeyChar) {
            case 8:
            case >= 97 and <= 122:
                // these are allowed
                break;
            case >= 65 and <= 90:
                // force lower case
                e.KeyChar += (char)32;
                break;
            case 32:
                // space not allowed if it's first character
                if (txtWordEdit.SelectionStart == 0) {
                    e.Handled = true;
                }
                break;
            case 33 or 34 or 39 or 40 or 41 or 44 or 45 or 46:
            case 58 or 59 or 63 or 91 or 93 or 96 or 123 or 125:
                //     !"'(),-.:;?[]`{}
                // NEVER allowed; these values get removed by the input function
                e.Handled = true;
                break;
            case >= 35 and <= 38 or 42 or 43 or >= 47 and <= 57:
            case 60 or 61 or 62 or 64 or 92 or 94 or 95 or 124:
            case 126 or 127:
                // these characters:
                //     #$%&*+/0123456789<=>@\^_|~
                // NOT allowed as first char
                if (txtWordEdit.SelectionStart == 0) {
                    // UNLESS supporting Power Pack mod
                    if (EditGame is null || !EditGame.PowerPack) {
                        e.Handled = true;
                    }
                }
                break;
            case > 127:
                // extended chars not allowed
                // UNLESS supporting the Power Pack mod
                if (EditGame is null || !EditGame.PowerPack) {
                    e.Handled = true;
                }
                break;
            }
        }
        #endregion

        #region Methods
        private void InitStatusStrip() {
            spGroupCount = new ToolStripStatusLabel();
            spWordCount = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;
            // 
            // spGroupCount
            // 
            spGroupCount.AutoSize = false;
            spGroupCount.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spGroupCount.BorderStyle = Border3DStyle.SunkenInner;
            spGroupCount.Name = "spGroupCount";
            spGroupCount.Size = new System.Drawing.Size(140, 18);
            spGroupCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spWordCount
            // 
            spWordCount.AutoSize = false;
            spWordCount.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spWordCount.BorderStyle = Border3DStyle.SunkenInner;
            spWordCount.Name = "spWordCount";
            spWordCount.Size = new System.Drawing.Size(140, 18);
            spWordCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        internal void InitFonts() {
            defaultfont = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            boldfont = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, FontStyle.Bold);
            label1.Font = defaultfont;
            label2.Font = defaultfont;
            txtGroupEdit.Font = defaultfont;
            txtWordEdit.Font = defaultfont;
            dgGroups.Font = defaultfont;
            dgWords.Font = defaultfont;
            dgGroups.Top = label1.Bottom;
            txtGroupEdit.Top = dgWords.Top = dgGroups.Top;
            warningstyle = dgWords.DefaultCellStyle.Clone();
            warningstyle.ForeColor = Color.Red;
            warningstyle.SelectionForeColor = Color.LightPink;
            reservedstyle = dgWords.DefaultCellStyle.Clone();
            reservedstyle.ForeColor = Color.DarkGray;
            reservedstyle.SelectionForeColor = Color.LightGray;
            Font f = new(warningstyle.Font, FontStyle.Bold);
            warningstyle.Font = reservedstyle.Font = f;

        }

        public bool LoadWords(WordList loadwords) {
            InGame = loadwords.InGame;
            try {
                if (InGame) {
                    loadwords.Load();
                }
            }
            catch (Exception ex) {
                // unhandled error
                ErrMsgBox(ex,
                    "Something went wrong. Unable to load WORDS.TOK",
                    ex.StackTrace,
                    "Load WORDS.TOK Failed");
                return false;
            }
            if (loadwords.Error != ResourceErrorType.NoError) {
                switch (loadwords.Error) {
                case ResourceErrorType.WordsTokNoFile:
                    MDIMain.MsgBoxWithHelp(
                        "WORDS.TOK is missing from this game's directory. A blank " +
                        "file will be created.",
                        "Missing WORDS.TOK",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        "htm\\winagi\\errors\\re19.htm");
                    break;
                case ResourceErrorType.WordsTokIsReadOnly:
                    MDIMain.MsgBoxWithHelp(
                        "WORDS.TOK is tagged as readonly. It cannot be edited " +
                        "unless full access is allowed.",
                        "Readonly WORDS.TOK",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\winagi\\errors\\re20.htm");
                    return false;
                case ResourceErrorType.WordsTokAccessError:
                    MDIMain.MsgBoxWithHelp(
                        "Unable to access WORDS.TOK. It cannot be edited.",
                        "File Access Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\winagi\\errors\\re21.htm");
                    return false;
                case ResourceErrorType.WordsTokNoData:
                    MDIMain.MsgBoxWithHelp(
                        "WORDS.TOK file has no data. A blank " +
                        "file will be created.",
                        "Empyt WORDS.TOK",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        "htm\\winagi\\errors\\re22.htm");
                    break;
                case ResourceErrorType.WordsTokBadIndex:
                    MDIMain.MsgBoxWithHelp(
                        "WORDS.TOK file is corrupted. A blank " +
                        "file will be created.",
                        "Invalid WORDS.TOK",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning,
                        "htm\\winagi\\errors\\re23.htm");
                    break;
                }
            }
            IsChanged = loadwords.IsChanged;
            EditWordList = loadwords.Clone();
            Filename = loadwords.ResFile;

            dgGroups.Rows.Clear();
            for (int i = 0; i < EditWordList.GroupCount; i++) {
                dgGroups.Rows.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
            }
            UpdateSelection(0, 0, true);
            spGroupCount.Text = "Group Count: " + EditWordList.GroupCount;
            spWordCount.Text = "Word Count: " + EditWordList.WordCount;

            // caption
            Text = "WORDS.TOK Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                if (Filename.Length > 0) {
                    Text += CompactPath(Filename, 75);
                }
                else {
                    WordEdCount++;
                    Text += "NewWords" + WordEdCount.ToString();
                }
            }
            if (IsChanged) {
                Text = CHG_MARKER + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.btnSaveResource.Enabled = !IsChanged;
            return true;
        }

        /// <summary>
        /// Loads importfile, and replaces existing WORDS.TOK in this editor.
        /// </summary>
        public void ImportWords(string importfile) {
            WordList tmpList;

            MDIMain.UseWaitCursor = true;
            bool sierrasrc = Path.GetFileName(importfile).Equals("words.txt", StringComparison.OrdinalIgnoreCase);
            tmpList = new(importfile, sierrasrc);
            if (sierrasrc) {
                if (tmpList.Error != ResourceErrorType.NoError) {
                    MDIMain.MsgBoxWithHelp(
                        tmpList.ErrData[0],
                        "Unable to Import Sierra Words Source File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\commands\\sierra_wordcompiler.htm");
                    return;
                }
                else if (tmpList.WarnData[0].Length > 0) {
                    MDIMain.MsgBoxWithHelp(
                        tmpList.WarnData[0],
                        "Abnomalies detected in Sierra Words Source File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\commands\\sierra_wordcompiler.htm");
                }
            }
            else {
                if (tmpList.Error != ResourceErrorType.NoError) {
                    string errmsg = "";
                    switch (tmpList.Error) {
                    case ResourceErrorType.WordsTokNoFile:
                        errmsg = "RE19: " + EngineResources.RE19;
                        break;
                    case ResourceErrorType.WordsTokIsReadOnly:
                        errmsg = "RE20: " + EngineResources.RE20;
                        break;
                    case ResourceErrorType.WordsTokAccessError:
                        errmsg = "RE21: " + EngineResources.RE21.Replace(
                        ARG1, tmpList.ErrData[0]);
                        break;
                    case ResourceErrorType.WordsTokNoData:
                        errmsg = "RE22: " + EngineResources.RE22;
                        break;
                    case ResourceErrorType.WordsTokBadIndex:
                        errmsg = "RE23: " + EngineResources.RE23;
                        break;
                    }
                    MDIMain.MsgBoxWithHelp(
                        errmsg,
                        "Unable to Open WORDS.TOK File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\winagi\\errors_resource.htm");
                    return;
                }
            }
            MDIMain.UseWaitCursor = true;
            // replace current wordlist
            EditWordList = tmpList;
            Filename = importfile;
            dgGroups.Rows.Clear();
            for (int i = 0; i < EditWordList.GroupCount; i++) {
                dgGroups.Rows.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
            }
            UpdateSelection(0, true);
            UndoCol = new();
            tbbUndo.Enabled = false;
            MarkAsChanged();
            MDIMain.UseWaitCursor = false;
            if (tmpList.Warnings != 0) {
                string warnmsg = "Anomalies were detected in this WORDS.TOK file:\n";
                if ((tmpList.Warnings & 1) == 1) {
                    warnmsg += "\nRW20: " + EngineResources.RW22;
                }
                if ((tmpList.Warnings & 2) == 2) {
                    warnmsg += "\nRW21: " + EngineResources.RW23;
                }
                if ((tmpList.Warnings & 4) == 4) {
                    warnmsg += "\nRW22: " + EngineResources.RW24;
                }
                if ((tmpList.Warnings & 8) == 8) {
                    warnmsg += "\nRW23: " + EngineResources.RW25;
                }
                if ((tmpList.Warnings & 16) == 16) {
                    warnmsg += "\nRW24: " + EngineResources.RW26;
                }
                if ((tmpList.Warnings & 32) == 32) {
                    warnmsg += "\nRW25: " + EngineResources.RW27;
                }
                if ((tmpList.Warnings & 64) == 64) {
                    warnmsg += "\nRW26: " + EngineResources.RW28;
                }
                MDIMain.MsgBoxWithHelp(
                    warnmsg,
                    "Words.Tok File Import Warnings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    "htm\\winagi\\warnings_resource.htm");
            }
        }

        public void SaveWords() {

            if (InGame) {
                MDIMain.UseWaitCursor = true;
                EditGame.WordList.Load();
                EditGame.WordList.CloneFrom(EditWordList);
                try {
                    EditGame.WordList.Save();
                }
                catch (Exception ex) {
                    ErrMsgBox(ex,
                        "Error during WORDS.TOK compilation: ",
                        ex.StackTrace + "Existing WORDS.TOK has not been modified.",
                        "WORDS.TOK Compile Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                if (RefreshLogics) {
                    // mark all logics that contain at least one 'said' command
                    foreach (Logic logic in EditGame.Logics) {
                        try {
                            bool loaded = logic.Loaded;
                            if (!loaded) {
                                logic.LoadSource();
                            }
                            int pos = logic.SourceText.IndexOf("said");
                            while (pos != -1) {
                                // confirm it's a test cmd
                                if (WinAGIFCTB.TokenFromPos(logic.SourceText, pos).Type == AGITokenType.Identifier) {
                                    EditGame.Logics.MarkAsChanged(logic.Number);
                                    // update reslist
                                    RefreshTree(AGIResType.Logic, logic.Number);
                                    break;
                                }
                                pos = logic.SourceText.IndexOf("said", ++pos);
                            }
                            if (!loaded) {
                                logic.Unload();
                            }
                        }
                        catch {
                            // if error, ignore it
                        }
                    }
                    RefreshLogics = false;
                }
                RefreshTree(AGIResType.Words, 0);
                // clear errors and warnings
                MDIMain.ClearWarnings(AGIResType.Words, 0);
                if (EditGame.WordList.Warnings != 0) {
                    WinAGIEventInfo warnInfo = new() {
                        ResType = AGIResType.Words,
                        ResNum = 0,
                        Line = -1,
                        Type = EventType.ResourceWarning,
                    };
                    if ((EditGame.WordList.Warnings & 4) == 4) {
                        warnInfo.ID = "RW21";
                        warnInfo.Text = EngineResources.RW23;
                        MDIMain.AddWarning(warnInfo);
                    }
                    if ((EditGame.WordList.Warnings & 8) == 8) {
                        warnInfo.ID = "RW22";
                        warnInfo.Text = EngineResources.RW24;
                        MDIMain.AddWarning(warnInfo);
                    }
                    if ((EditGame.WordList.Warnings & 32) == 32) {
                        warnInfo.Type = EventType.ResourceWarning;
                        warnInfo.ID = "RW24";
                        warnInfo.Text = EngineResources.RW26;
                        MDIMain.AddWarning(warnInfo);
                    }
                    if ((EditGame.WordList.Warnings & 64) == 64) {
                        warnInfo.Type = EventType.ResourceWarning;
                        warnInfo.ID = "RW25";
                        warnInfo.Text = EngineResources.RW27;
                        MDIMain.AddWarning(warnInfo);
                    }
                    MDIMain.UpdateGridCounts();
                }
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditWordList.ResFile.Length == 0) {
                    ExportWords();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    try {
                        EditWordList.Save();
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex,
                            "An Error occurred while saving this wordlist:",
                            ex.StackTrace + "\n\nExisting wordlist file has not been modified.",
                            "Wordlist Save Error");
                        MDIMain.UseWaitCursor = false;
                        return;
                    }
                    MDIMain.UseWaitCursor = false;
                }
            }
            MarkAsSaved();
        }

        public void ExportWords() {
            bool retval = Base.ExportWords(EditWordList, InGame);
            if (!InGame && retval) {
                Filename = EditWordList.ResFile;
                MarkAsSaved();
            }
        }

        public void EditProperties() {
            string description = EditWordList.Description;
            string id = "";
            if (GetNewResID(AGIResType.Words, -1, ref id, ref description, InGame, 2)) {
                EditWordList.Description = description;
                MDIMain.RefreshPropertyGrid(AGIResType.Words, 0);
            }
        }

        private void ClearWordList(bool DefaultWords = true) {
            WordsUndo NextUndo = new();
            NextUndo.Action = WordsUndo.ActionType.Clear;
            NextUndo.Group = new string[EditWordList.GroupCount];
            // add each group of words to the undo list, with groupnumber first, and words after
            // separated by a pipe character
            for (int i = 0; i < EditWordList.GroupCount; i++) {
                string output = EditWordList.GroupByIndex(i).GroupNum.ToString();
                for (int j = 0; j < EditWordList.GroupByIndex(i).WordCount; j++) {
                    output += "|" + EditWordList.GroupByIndex(i).Words[j];
                }
                NextUndo.Group[i] = output;
            }
            AddUndo(NextUndo);
            //  force logics refresh
            RefreshLogics = true;

            EditWordList.Clear();
            if (DefaultWords) {
                // add "a" and "rol" and "anyword"
                EditWordList.AddWord("a", 0);
                EditWordList.AddWord("anyword", 1);
                EditWordList.AddWord("rol", 9999);
            }
            dgGroups.Rows.Clear();
            dgWords.Rows.Clear();
            if (GroupMode) {
                for (int i = 0; i < EditWordList.GroupCount; i++) {
                    dgGroups.Rows.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                }
            }
            else {
                foreach (AGIWord word in EditWordList) {
                    AddWordToGrid(word.WordText);
                }
            }
            UpdateSelection(0, 0);
        }

        private void AddWordToGrid(string word, int index = -1) {
            int newrow;
            if (index >= 0) {
                newrow = index;
                dgWords.Rows.Insert(index, word);
            }
            else {
                newrow = dgWords.Rows.Add(word);
            }
            // check for invalid characters ,.?!();:[]{} and '`-"
            string inv = ",.?!();:[]{}'`-\"";
            if (word.Any(inv.Contains)) {
                dgWords.Rows[newrow].DefaultCellStyle = warningstyle;
            }
            // check for irregular start character
            else if (word[0] < 'a' || word[0] > 'z') {
                dgWords.Rows[newrow].DefaultCellStyle = warningstyle;
            }
            else {
                switch (EditGroupNumber) {
                case 0:
                    // 'a' and 'i' are reserved
                    if (word == "a" || word == "i") {
                        dgWords.Rows[newrow].DefaultCellStyle = reservedstyle;
                    }
                    break;
                case 1:
                    // 'anyword' is reserved
                    if (word == "anyword") {
                        dgWords.Rows[newrow].DefaultCellStyle = reservedstyle;
                    }
                    break;
                case 9999:
                    // 'rol' is reserved
                    if (word == "rol") {
                        dgWords.Rows[newrow].DefaultCellStyle = reservedstyle;
                    }
                    break;
                }
            }
        }

        public void UpdateSelection(int newwordindex, bool force = false) {
            int group = EditWordList[newwordindex].Group;
            UpdateSelection(group, EditWordList.GroupByNumber(group).Words.IndexOf(EditWordList[newwordindex].WordText), force);
        }

        private void UpdateSelection(string newword, bool force = false) {
            int group = EditWordList[newword].Group;
            UpdateSelection(group, EditWordList.GroupByNumber(group).Words.IndexOf(newword), force);
        }

        public void UpdateSelection(int newgroup, int newwordgindex, bool force = false) {
            EditGroupNumber = newgroup;
            EditGroupIndex = EditWordList.GroupIndexFromNumber(EditGroupNumber);
            EditWordGroupIndex = newwordgindex;

            if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                EditWordText = "";
                EditWordIndex = -1;
                Debug.Assert(GroupMode);

            }
            else {
                EditWordText = EditWordList.GroupByIndex(EditGroupIndex)[EditWordGroupIndex];
                EditWordIndex = EditWordList.WordIndex(EditWordText);
            }
            if (GroupMode) {
                if (force || dgGroups.SelectedRows[0].Index != EditGroupIndex) {
                    dgWords.Rows.Clear();
                    foreach (string word in EditWordList.GroupByNumber(EditGroupNumber)) {
                        AddWordToGrid(word);
                    }
                    if (dgWords.Rows.Count == 0) {
                        if (EditGroupNumber == 0) {
                            dgWords.Rows.Add("<group 0: null words>");
                        }
                        if (EditGroupNumber == 1) {
                            dgWords.Rows.Add("<group 1: any word>");
                        }
                        else if (EditGroupNumber == 9999) {
                            dgWords.Rows.Add("<group 9999: rest of line>");
                        }
                    }
                }
                SelectGroup(EditGroupIndex);
                if (EditWordGroupIndex >= 0) {
                    SelectWord(EditWordGroupIndex);
                }
            }
            else {
                dgGroups.Rows[0].Cells[0].Value = (EditGroupNumber.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName;
                SelectWord(EditWordIndex);
            }
        }

        public void SelectGroup(int groupindex) {
            dgGroups.Rows[groupindex].Selected = true;
            if (groupindex >= dgGroups.FirstDisplayedScrollingRowIndex &&
                groupindex < dgGroups.FirstDisplayedScrollingRowIndex + dgGroups.DisplayedRowCount(false)) {
                return;
            }
            dgGroups.FirstDisplayedScrollingRowIndex = groupindex;
        }

        public void SelectWord(int wordindex) {
            if (dgWords.Rows.Count == 0) {
                Debug.Assert(wordindex == -1);
                return;
            }
            dgWords.Rows[wordindex].Selected = true;
            if (wordindex >= dgWords.FirstDisplayedScrollingRowIndex &&
                wordindex < dgWords.FirstDisplayedScrollingRowIndex + dgWords.DisplayedRowCount(false)) {
                return;
            }
            dgWords.FirstDisplayedScrollingRowIndex = wordindex;
        }

        public void SelectWord(string word) {
            for (int i = 0; i < dgWords.Rows.Count; i++) {
                if ((string)dgWords.Rows[i].Cells[0].Value == EditWordText) {
                    dgWords.Rows[i].Selected = true;
                    if (i >= dgWords.FirstDisplayedScrollingRowIndex &&
                        i < dgWords.FirstDisplayedScrollingRowIndex + dgWords.DisplayedRowCount(false)) {
                        return;
                    }
                    dgWords.FirstDisplayedScrollingRowIndex = i;
                    break;
                }
            }
        }

        private void NewGroup() {
            // inserts a new group, with next available group number
            // and a new default word

            string newword = GetUniqueWord();
            if (newword.Length == 0) {
                return;
            }
            int newgroup = NextGrpNum();
            AddGroup(newgroup);
            AddWord(newgroup, newword, true);
            AddNewGroup = true;
            EditWordText = newword;
            BeginWordEdit();
        }

        private int ValidateGroup(string groupnumtext) {
            if (!groupnumtext.IsNumeric()) {
                MDIMain.MsgBoxWithHelp(
                    "Group number must be numeric.",
                    "Renumber Group Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    "htm\\agi\\words.htm#16bit");
                return -1;
            }
            int newgroupnum = groupnumtext.IntVal();
            if (newgroupnum > 65535) {
                MDIMain.MsgBoxWithHelp(
                    "Invalid group number. Must be less than 65536.",
                    "Renumber Group Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    "htm\\agi\\words.htm#16bit");
                return -1;
            }
            if (newgroupnum != EditGroupNumber) {
                // if the new number is already in use
                if (EditWordList.GroupExists(newgroupnum)) {
                    // not a valid new group number
                    MessageBox.Show(MDIMain,
                        "Group " + newgroupnum + " is already in use. Choose another number.",
                        "Renumber Group Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return -1;
                }
            }
            return newgroupnum;
        }

        private void DeleteGroupOrWord() {
            if (dgGroups.Focused) {
                if (EditGroupNumber != -1) {
                    int groupindex = EditGroupIndex;
                    int wordindex = EditWordIndex;
                    DeleteGroup(EditGroupNumber);
                    if (GroupMode) {
                        if (groupindex == dgGroups.Rows.Count) {
                            groupindex--;
                        }
                        UpdateSelection(EditWordList.GroupByIndex(groupindex).GroupNum, 0, true);
                    }
                    else {
                        if (wordindex == dgWords.Rows.Count) {
                            wordindex--;
                        }
                        UpdateSelection(wordindex, true);
                    }
                }
            }
            if (dgWords.Focused) {
                if (EditGroupNumber == -1) {
                    return;
                }
                DeleteWord(EditWordText);
                // select next word, or if the grp is
                // gone select next group
                if (!GroupMode || EditWordList.GroupExists(EditGroupNumber)) {
                    if (GroupMode) {
                        if (EditWordGroupIndex == dgWords.Rows.Count) {
                            EditWordGroupIndex--;
                        }
                        UpdateSelection(EditGroupNumber, EditWordGroupIndex, true);
                    }
                    else {
                        if (EditWordIndex == dgWords.Rows.Count) {
                            EditWordIndex--;
                        }
                        UpdateSelection(EditWordIndex, true);
                    }
                }
                else {
                    if (EditGroupIndex == dgGroups.Rows.Count) {
                        EditGroupIndex--;
                    }
                    UpdateSelection(EditWordList.GroupByIndex(EditGroupIndex).GroupNum, 0, true);
                }
            }
        }

        private void DeleteGroup(int group, bool DontUndo = false) {
            if (group < 2 || group == 9999) {
                if (!DontUndo) {
                    MDIMain.MsgBoxWithHelp(
                        $"Group '{EditGroupNumber}' is  a reserved group " +
                        " and cannot be deleted.",
                        "Reserved Word Group",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\winagi\\editor_words.htm#reserved");
                }
                return;
            }
            WordsUndo NextUndo = null;
            if (!DontUndo) {
                NextUndo = new() {
                    Action = WordsUndo.ActionType.DelGroup,
                    GroupNo = group
                };
                string[] words = new string[EditWordList.GroupByNumber(group).WordCount];
                for (int i = 0; i < words.Length; i++) {
                    words[i] = EditWordList.GroupByNumber(group).Words[i];
                }
                NextUndo.Group = words;
            }
            RemoveAGroup(group);
            if (!DontUndo) {
                AddUndo(NextUndo);
            }
        }

        private void RenumberGroup(int OldGroupNo, int NewGroupNo, bool DontUndo = false) {

            EditWordList.RenumberGroup(OldGroupNo, NewGroupNo);
            if (GroupMode) {
                dgGroups.Rows.RemoveAt(EditGroupIndex);
                EditGroupIndex = EditWordList.GroupIndexFromNumber(NewGroupNo);
                EditGroupNumber = NewGroupNo;
                dgGroups.Rows.Insert(EditGroupIndex, (EditGroupNumber.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName);
                UpdateSelection(EditGroupNumber, EditWordGroupIndex);
                SelectGroup(EditGroupIndex);
            }
            else {
                EditGroupIndex = EditWordList.GroupIndexFromNumber(NewGroupNo);
                UpdateSelection(NewGroupNo, 0, true);
            }
            dgGroups.Refresh();
            if (!DontUndo) {
                WordsUndo NextUndo = new();
                NextUndo.Action = WordsUndo.ActionType.Renumber;
                NextUndo.OldGroupNo = OldGroupNo;
                NextUndo.GroupNo = NewGroupNo;
                // add it
                AddUndo(NextUndo);
            }
            // force logics refresh
            RefreshLogics = true;
            return;
        }

        private void BeginWordEdit() {
            int index = dgWords.SelectedRows[0].Index;
            EditingWord = true;
            // configure the TextBox for in-place editing
            txtWordEdit.Text = EditWordText;
            Point location = dgWords.GetCellDisplayRectangle(0, index, true).Location;
            location.Offset(dgWords.Location + new Size(3, 2));
            txtWordEdit.Location = location;
            txtWordEdit.Size = dgWords.GetCellDisplayRectangle(0, index, true).Size;
            dgWords.SelectedCells[0].Style.SelectionBackColor = SystemColors.Window;
            txtWordEdit.Visible = true;
            txtWordEdit.Select();
        }

        public void NewWord(int group) {
            string newword = GetUniqueWord();
            if (newword.Length == 0) {
                return;
            }
            if ((group == 0 || group == 1 || group == 9999) && EditWordList.GroupByNumber(group).WordCount == 0) {
                // remove the placeholder
                if (GroupMode) {
                    dgWords.Rows.RemoveAt(0);
                }
            }
            AddWord(group, newword);
            AddNewWord = true;
            BeginWordEdit();
        }

        private string GetUniqueWord() {
            int newindex = 0;
            string newword;
            do {
                newindex++;
                newword = "new word " + newindex;
                if (!EditWordList.WordExists(newword)) {
                    break;
                }
            } while (newindex < 1000);
            if (newindex == 1000) {
                MessageBox.Show(MDIMain,
                    "You already have 1,000 new word entries. Try changing a few of those before adding more.",
                    "Too Many Default Words",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return "";
            }
            return newword;
        }

        private string ValidateWord(string CheckWord, bool Quiet = false) {
            string retval = CheckWord.SingleSpace().Trim().LowerAGI();
            if (retval.Length == 0) {
                // ok; it will be deleted
                return "";
            }
            // need to check for invalid characters (in case something 
            // was pasted)
            if ("!\"'(),-.:;?[]`{}".Any(retval.Contains)) {
                if (!Quiet) {
                    MDIMain.MsgBoxWithHelp(
                        "This word contains one or more invalid characters.",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\agi\\words.htm#charlimits");
                }
                return "!";
            }
            if (retval[0] < 97 || retval[0] > 122) {
                // not allowed unless supporting power pack
                if (EditGame is null || !EditGame.PowerPack) {
                    if (!Quiet) {
                        MDIMain.MsgBoxWithHelp(
                        "This word begins with invalid characters.",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\agi\\words.htm#charlimits");
                    }
                    return "!";
                }
            }
            // extended characters
            if (retval.Any(c => c > 127)) {
                // not allowed unless supporting power pack
                if (EditGame is null || !EditGame.PowerPack) {
                    if (!Quiet) {
                        MDIMain.MsgBoxWithHelp(
                        "This word contains one or more extended characters (> 128).",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\agi\\words.htm#charlimits");
                    }
                    return "!";
                }
            }
            bool exists = EditWordList.WordExists(retval);
            if (exists) {
                if (EditWordList[retval].Group == EditGroupNumber) {
                    if (!Quiet) {
                        MessageBox.Show(MDIMain,
                        "The word '" + retval + "' already exists in this group.",
                        "Duplicate Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    }
                    return "!";
                }
                else {
                    if (!Quiet) {
                        // special checks for 'a', 'i', 'anyword', 'rol'
                        switch (retval) {
                        case "a" or "i":
                            if (EditGroupNumber != 0) {
                                if (MessageBox.Show(MDIMain,
                                    "The word '" + retval + "' is usually assigned to group 0. " +
                                    "Do you want to move it to this group?",
                                    "Move Word",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No) {
                                    return "!";
                                }
                            }
                            break;
                        case "anyword":
                            if (EditGroupNumber != 1) {
                                if (MessageBox.Show(MDIMain,
                                    "The word 'anyword' is the Sierra default placeholder for group 1. " +
                                    "Do you want to move it to this group?",
                                    "Move Word",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No) {
                                    return "!";
                                }
                            }
                            break;
                        case "rol":
                            if (EditGroupNumber != 1) {
                                if (MessageBox.Show(MDIMain,
                                    "The word 'rol' is the Sierra default placeholder for group 9999. " +
                                    "Do you want to move it to this group?",
                                    "Move Word",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.No) {
                                    return "!";
                                }
                            }
                            break;
                        default:
                            if (MessageBox.Show(MDIMain,
                                "The word '" + retval + "' already exists in another group. " +
                                "Do you want to move it to this group?",
                                "Move Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.No) {
                                return "!";
                            }
                            break;
                        }
                    }
                }
            }
            else {
                if (!Quiet) {
                    // special checks for 'a', 'i', 'anyword', 'rol'
                    switch (retval) {
                    case "a" or "i":
                        if (EditGroupNumber != 0) {
                            if (MessageBox.Show(MDIMain,
                                "The word '" + retval + "' is usually assigned to group 0. " +
                                "Do you want to add it to this group?",
                                "Change Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.No) {
                                return "!";
                            }
                        }
                        break;
                    case "anyword":
                        if (EditGroupNumber != 1) {
                            if (MessageBox.Show(MDIMain,
                                "The word 'anyword' is the Sierra default placeholder for group 1. " +
                                "Do you want to add it to this group?",
                                "Change Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.No) {
                                return "!";
                            }
                        }
                        break;
                    case "rol":
                        if (EditGroupNumber != 9999) {
                            if (MessageBox.Show(MDIMain,
                                "The word 'rol' is the Sierra default placeholder for group 9999. " +
                                "Do you want to add it to this group?",
                                "Change Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.No) {
                                return "!";
                            }
                        }
                        break;
                    }
                }
            }
            // new word is ok
            return retval;
        }

        private int IsValidWord(string wordtext) {
            if ("!\"'(),-.:;?[]`{}".Any(wordtext.Contains)) {
                return 1;
            }
            if (wordtext[0] < 97 || wordtext[0] > 122) {
                // not allowed unless supporting power pack
                return 2;
            }
            return 0;
        }

        private bool EditWord(string oldWord, string newWord, int thisGroup, bool DontUndo = false) {
            bool isFirst = EditWordList.GroupByNumber(thisGroup).GroupName == oldWord;
            bool moveword = EditWordList.WordExists(newWord);
            int oldgroupnum;

            if (moveword) {
                oldgroupnum = EditWordList[newWord].Group;
                // if this is first word in the OLD group
                bool delFirst = (EditWordList.GroupByNumber(oldgroupnum).GroupName == newWord);
                // delete new word from its old group (so it can be added to new group)
                if (EditWordList.GroupByNumber(oldgroupnum).WordCount <= 1) {
                    // only one word, so delete entire group
                    RemoveAGroup(oldgroupnum);
                }
                else {
                    // remove only this word
                    RemoveAWord(newWord);
                    // if it was first word
                    if (delFirst) {
                        UpdateGroupName(oldgroupnum);
                    }
                }
            }
            else {
                // the new word doesn't exist in any other group
                oldgroupnum = -1;
            }
            RemoveAWord(oldWord);
            InsertAWord(newWord, thisGroup);
            // if this is the first word in the group OR old word was first word in the group
            if ((isFirst || EditWordList.GroupByNumber(thisGroup).GroupName == newWord)) {
                UpdateGroupName(thisGroup);
            }

            // if not skipping undo
            if (!DontUndo) {
                if (AddNewWord) {
                    // change last undo object
                    UndoCol.Peek().Word = newWord;
                    UndoCol.Peek().OldGroupNo = oldgroupnum;
                }
                else {
                    // create undo object
                    WordsUndo NextUndo = new WordsUndo {
                        Action = WordsUndo.ActionType.ChangeWord,
                        GroupNo = thisGroup,
                        Word = newWord,
                        OldWord = oldWord,
                        OldGroupNo = oldgroupnum
                    };
                    AddUndo(NextUndo);
                    //  force logics refresh
                    RefreshLogics = true;
                }
                // select the new word
                UpdateSelection(newWord, true);
                AddNewWord = false;
            }
            // return success
            return true;
        }

        private void MoveWord(string WordText, int NewGrpNum, bool DontUndo = false) {
            // moves a word from one group to another

            int OldGrpNum = EditWordList[WordText].Group;
            if (OldGrpNum == NewGrpNum) {
                return;
            }
            bool isFirst = EditWordList.GroupByNumber(OldGrpNum).GroupName == WordText;
            if ((OldGrpNum == 0 || OldGrpNum == 1 || OldGrpNum == 9999) && EditWordList.GroupByNumber(OldGrpNum).WordCount == 1) {
                EditWordList.RemoveWord(WordText);
            }
            else if (EditWordList.GroupByNumber(OldGrpNum).WordCount > 1) {
                RemoveAWord(WordText);
                if (isFirst) {
                    UpdateGroupName(OldGrpNum);
                }
            }
            else {
                RemoveAGroup(OldGrpNum);
            }
            // if undoing, new group may not exist
            if (DontUndo) {
                if (!EditWordList.GroupExists(NewGrpNum)) {
                    AddGroup(NewGrpNum, true);
                }
            }
            InsertAWord(WordText, NewGrpNum);
            if (!DontUndo) {
                WordsUndo NextUndo = new();
                NextUndo.Action = WordsUndo.ActionType.MoveWord;
                NextUndo.GroupNo = NewGrpNum;
                NextUndo.OldGroupNo = OldGrpNum;
                NextUndo.Word = WordText;
                AddUndo(NextUndo);
                // force logic refresh
                RefreshLogics = true;
            }
            if (EditWordList.GroupByNumber(NewGrpNum).GroupName == WordText) {
                UpdateGroupName(NewGrpNum);
            }
            if (GroupMode) {
                FormatGroupRow(dgGroups.Rows[EditWordList.GroupIndexFromNumber(OldGrpNum)]);
                FormatGroupRow(dgGroups.Rows[EditWordList.GroupIndexFromNumber(NewGrpNum)]);
            }
        }

        private void DeleteWord(string word, bool DontUndo = false) {
            int group = EditWordList[word].Group;

            if (EditWordList.GroupByNumber(group).WordCount == 1 &&
                group != 0 && group != 1 && group != 9999) {
                RemoveAGroup(group);
            }
            else {
                RemoveAWord(word);
                // groups 1 and 9999 use a placeholder if no words left
                if (group == 1 && dgWords.Rows.Count == 0) {
                    dgWords.Rows.Add("<group 1: any word>");
                }
                if (group == 9999 && dgWords.Rows.Count == 0) {
                    dgWords.Rows.Add("<group 9999: rest of line>");
                }
            }
            if (GroupMode) {
                FormatGroupRow(dgGroups.Rows[EditWordList.GroupIndexFromNumber(group)]);
            }
            if (!DontUndo) {
                WordsUndo NextUndo = new();
                NextUndo.Action = WordsUndo.ActionType.DelWord;
                NextUndo.Word = word;
                NextUndo.GroupNo = group;
                AddUndo(NextUndo);
            }
        }

        private int AddGroup(int NewGroupNo, bool DontUndo = false) {
            // adds a new group, and returns the index of the group in the list

            EditWordList.AddGroup(NewGroupNo);
            int i = 0;
            if (GroupMode) {
                for (i = 0; i < dgGroups.Rows.Count; i++) {
                    // if the new group is less than or equal to current group number
                    if (NewGroupNo <= EditWordList.GroupByIndex(i).GroupNum) {
                        // this is where to insert it
                        break;
                    }
                }
                dgGroups.Rows.Insert(i, (NewGroupNo.ToString() + ":").PadRight(6) + EditWordList.GroupByNumber(NewGroupNo).GroupName);
            }
            if (!DontUndo) {
                // create undo object
                WordsUndo NextUndo = new();
                NextUndo.Action = WordsUndo.ActionType.AddGroup;
                NextUndo.GroupNo = NewGroupNo;
                AddUndo(NextUndo);
                UpdateSelection(NewGroupNo, -1);
            }
            // return the index number
            return i;
        }

        private void AddWord(int GroupNo, string NewWord, bool DontUndo = false) {
            string firstword = "";

            // groups 0, 1, and 9999 always use default groupname, so ignore check for change
            if (GroupNo != 0 && GroupNo != 1 && GroupNo != 9999) {
                // any other group will always have at least one word
                firstword = EditWordList.GroupByNumber(GroupNo).GroupName;
            }
            InsertAWord(NewWord, GroupNo);
            if (GroupMode) {
                FormatGroupRow(dgGroups.Rows[EditWordList.GroupIndexFromNumber(GroupNo)]);
            }
            if (EditWordList.GroupByNumber(GroupNo).GroupName != firstword) {
                UpdateGroupName(GroupNo);
            }
            if (!DontUndo) {
                WordsUndo NextUndo = new() {
                    Action = WordsUndo.ActionType.AddWord,
                    GroupNo = EditWordList[NewWord].Group,
                    OldGroupNo = -1, // NextUndo.GroupNo;
                    Word = NewWord
                };
                AddUndo(NextUndo);
                UpdateSelection(NewWord);
            }
        }

        private void InsertAWord(string NewWord, int group) {
            WordList.AGIWordComparer comparer = new();

            EditWordList.AddWord(NewWord, group);
            if ((GroupMode && EditGroupNumber == group) || !GroupMode) {
                int i;
                for (i = 0; i < dgWords.Rows.Count; i++) {
                    if (comparer.Compare(NewWord, (string)dgWords.Rows[i].Cells[0].Value) < 0) {
                        break;
                    }
                }
                AddWordToGrid(NewWord, i);
            }
        }

        private void RemoveAGroup(int group) {
            // this also removes all words from the group
            if (GroupMode) {
                dgGroups.Rows.RemoveAt(EditWordList.GroupIndexFromNumber(group));
            }
            else {
                foreach (string word in EditWordList.GroupByNumber(group)) {
                    dgWords.Rows.RemoveAt(IndexFromString(word));
                }
            }
            EditWordList.RemoveGroup(group);
        }

        private void RemoveAWord(string word) {
            // update the list boxes
            if (GroupMode) {
                if (EditGroupNumber == EditWordList[word].Group) {
                    dgWords.Rows.RemoveAt(IndexFromString(word));
                }
            }
            else {
                dgWords.Rows.RemoveAt(IndexFromString(word));
            }
            // update the list
            EditWordList.RemoveWord(word);
        }

        private int IndexFromString(string word) {
            for (int i = 0; i < dgWords.Rows.Count; i++) {
                if ((string)dgWords.Rows[i].Cells[0].Value == word) {
                    return i;
                }
            }
            return -1;
        }

        private void UpdateGroupName(int GroupNo) {
            // updates the group list for the correct name

            // should never happen but...
            if (!EditWordList.GroupExists(GroupNo)) {
                return;
            }
            switch (GroupNo) {
            case 0:
            case 1:
            case 9999:
                // never change the group name for these groups
                return;
            }
            if (GroupMode) {
                dgGroups.Rows[EditWordList.GroupIndexFromNumber(GroupNo)].Cells[0].Value = (GroupNo.ToString() + ":").PadRight(6) + EditWordList.GroupByNumber(GroupNo).GroupName;
            }
            else {
                if (GroupNo == EditGroupNumber) {
                    dgGroups.Rows[0].Cells[0].Value = (GroupNo.ToString() + ":").PadRight(6) + EditWordList.GroupByNumber(GroupNo).GroupName;
                }
            }
        }

        private int NextGrpNum() {
            // search forward
            for (int i = EditGroupNumber + 1; i < 65536; i++) {
                if (!EditWordList.GroupExists(i)) {
                    return i;
                }
            }
            // if not found, try going backwards
            for (int i = EditGroupNumber - 1; i >= 0; i--) {
                if (!EditWordList.GroupExists(i)) {
                    return i;
                }
            }
            // if still not found, means user has 64K words! IMPOSSIBLE I SAY!
            return -1;
        }

        private void StartSearch(FindFormFunction formFunction) {
            string searchtext = GFindText;
            if (searchtext.Length > 1) {
                if (searchtext[0] == '"') {
                    searchtext = searchtext[1..];
                }
                if (searchtext[^1] == '"') {
                    searchtext = searchtext[..^1];
                }
            }
            FindingForm.SetForm(formFunction, InGame);
            FindingForm.SetFindText(searchtext);
            if (!FindingForm.Visible) {
                FindingForm.Visible = true;
            }
            FindingForm.Select();
            FindingForm.cmbFind.Select();
        }

        public void FindInWords(string FindText, FindDirection FindDir, bool MatchWord, bool Replacing = false, string ReplaceText = "") {
            int FoundWord, FoundGrp;

            if (Replacing && FindText.Equals(ReplaceText, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            if (Replacing && (ReplaceText.Length == 0)) {
                return;
            }
            if (Replacing && ReplaceText.Length == 0) {
                MessageBox.Show(MDIMain,
                    "Blank replacement text is not allowed in word replacement.",
                    "Replace in Word List",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            if (EditWordList.WordCount == 0) {
                MessageBox.Show(MDIMain,
                    "There are no words in this list.",
                    "Find in Word List",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // if replacing and searching up        start at next word
            // if replacing and searching down      start at current word
            // if not replacing and searching up    start at current word
            // if not replacing and searching down  start at next word

            MDIMain.UseWaitCursor = true;
            int SearchGrp = EditGroupIndex;
            int SearchWord;
            if (GroupMode) {
                SearchWord = EditWordGroupIndex;
            }
            else {
                SearchWord = EditWordIndex;
            }
            if (SearchWord == -1) {
                SearchWord = 0;
            }
            if ((Replacing && FindDir == FindDirection.Up) || (!Replacing && FindDir != FindDirection.Up)) {
                SearchWord++;
                if (GroupMode) {
                    if (SearchWord == EditWordList.GroupByIndex(SearchGrp).WordCount) {
                        // use first word
                        SearchWord = 0;
                        // of next group
                        SearchGrp++;
                        if (SearchGrp == EditWordList.GroupCount) {
                            SearchGrp = 0;
                        }
                    }
                }
                else {
                    if (SearchWord == EditWordList.WordCount) {
                        // use first word of list
                        SearchWord = 0;
                        SearchGrp = EditWordList[0].Group;
                    }
                }
            }
            else {
                // if already AT beginning of search, the replace function will mistakenly
                // think the find operation is complete and stop
                if (Replacing && (SearchWord == StartWord && SearchGrp == StartGrp)) {
                    // reset search
                    FindingForm.ResetSearch();
                }
            }

            // main search loop
            do {
                if (FindDir == FindDirection.Up) {
                    if (GroupMode) {
                        // iterate backwards until word found or GrpFound=-1
                        FoundWord = SearchWord - 1;
                        FoundGrp = SearchGrp;
                        // if at top of this group,get the last word of previous group
                        if (FoundWord < 0) {
                            FoundGrp--;
                            if (FoundGrp != -1) {
                                FoundWord = EditWordList.GroupByIndex(FoundGrp).WordCount - 1;
                            }
                        }
                        while (FoundGrp != -1) {
                            // skip groups with no words
                            if (EditWordList.GroupByIndex(FoundGrp).WordCount != 0) {
                                if (MatchWord) {
                                    if (EditWordList.GroupByIndex(FoundGrp).Words[FoundWord].Equals(FindText, StringComparison.OrdinalIgnoreCase)) {
                                        // found
                                        break; // exit do
                                    }
                                }
                                else {
                                    if (EditWordList.GroupByIndex(FoundGrp).Words[FoundWord].Contains(FindText, StringComparison.OrdinalIgnoreCase)) {
                                        // found
                                        break; // exit do
                                    }
                                }
                            }
                            // not found, move to previous word
                            FoundWord--;
                            if (GroupMode) {
                                if (FoundWord < 0) {
                                    FoundGrp--;
                                    if (FoundGrp != -1) {
                                        FoundWord = EditWordList.GroupByIndex(FoundGrp).WordCount - 1;
                                    }
                                }
                            }
                            else {
                                if (FoundWord < 0) {
                                    FoundGrp = -1;
                                }
                                else {
                                    FoundGrp = EditWordList.GroupIndexFromNumber(EditWordList[FoundWord].Group);
                                }
                            }
                        }
                        // reset search to last group/last word + 1
                        SearchGrp = EditWordList.GroupCount - 1;
                        SearchWord = EditWordList.GroupByIndex(SearchGrp).WordCount;
                    }
                    else {
                        // iterate backwards until word found or start of wordlist reached
                        FoundWord = SearchWord - 1;
                        if (FoundWord < 0) {
                            FoundGrp = -1;
                        }
                        else {
                            FoundGrp = EditWordList[FoundWord].Group;
                        }
                        while (FoundGrp != -1) {
                            if (MatchWord) {
                                if (EditWordList[FoundWord].WordText.Equals(FindText, StringComparison.OrdinalIgnoreCase)) {
                                    // found
                                    break; // exit do
                                }
                            }
                            else {
                                if (EditWordList[FoundWord].WordText.Contains(FindText, StringComparison.OrdinalIgnoreCase)) {
                                    // found
                                    break; // exit do
                                }
                            }
                            // not found, move to previous word
                            FoundWord--;
                            if (FoundWord < 0) {
                                FoundGrp = -1;
                            }
                            else {
                                FoundGrp = EditWordList.GroupIndexFromNumber(EditWordList[FoundWord].Group);
                            }
                        }
                        // reset to last word + 1
                        SearchWord = EditWordList.WordCount;
                        // (set group to match last word)
                        SearchGrp = EditWordList.GroupIndexFromNumber(EditWordList[SearchWord - 1].Group);
                    }
                }
                else {
                    if (GroupMode) {
                        // iterate forward until word found or foundgrp=groupcount
                        FoundGrp = SearchGrp;
                        FoundWord = SearchWord;
                        do {
                            // skip groups with no words
                            if (EditWordList.GroupByIndex(FoundGrp).WordCount != 0) {
                                if (MatchWord) {
                                    if (EditWordList.GroupByIndex(FoundGrp).Words[FoundWord].Equals(FindText, StringComparison.OrdinalIgnoreCase)) {
                                        // found
                                        break; // exit do
                                    }
                                }
                                else {
                                    if (EditWordList.GroupByIndex(FoundGrp).Words[FoundWord].Contains(FindText, StringComparison.OrdinalIgnoreCase)) {
                                        // found
                                        break; // exit do
                                    }
                                }
                            }
                            FoundWord++;
                            if (GroupMode) {
                                if (FoundWord >= EditWordList.GroupByIndex(FoundGrp).WordCount) {
                                    FoundWord = 0;
                                    FoundGrp++;
                                }
                            }
                            else {
                                if (FoundWord == EditWordList.WordCount) {
                                    FoundGrp = EditWordList.GroupCount;
                                }
                                else {
                                    FoundGrp = EditWordList.GroupIndexFromNumber(EditWordList[FoundWord].Group);
                                }
                            }
                        } while (FoundGrp != EditWordList.GroupCount);
                        // reset search to first word of first group
                        SearchGrp = 0;
                        SearchWord = 0;
                    }
                    else {
                        // iterate forward until word found or foundgrp=groupcount
                        FoundWord = SearchWord;
                        FoundGrp = SearchGrp;
                        do {
                            if (MatchWord) {
                                if (EditWordList[FoundWord].WordText.Equals(FindText, StringComparison.OrdinalIgnoreCase)) {
                                    // found
                                    break; // exit do
                                }
                            }
                            else {
                                if (EditWordList[FoundWord].WordText.Contains(FindText, StringComparison.OrdinalIgnoreCase)) {
                                    // found
                                    break; // exit do
                                }
                            }
                            FoundWord++;
                            if (GroupMode) {
                                if (FoundWord >= EditWordList.GroupByIndex(FoundGrp).WordCount) {
                                    FoundWord = 0;
                                    FoundGrp++;
                                }
                            }
                            else {
                                if (FoundWord == EditWordList.WordCount) {
                                    FoundGrp = EditWordList.GroupCount;
                                }
                                else {
                                    FoundGrp = EditWordList.GroupIndexFromNumber(EditWordList[FoundWord].Group);
                                }
                            }
                        } while (FoundGrp != EditWordList.GroupCount);
                        // reset search to first word of list
                        SearchWord = 0;
                        SearchGrp = EditWordList.GroupIndexFromNumber(EditWordList[SearchWord].Group);
                    }
                }
                // if found, group will be valid
                if (FoundGrp >= 0 && FoundGrp < EditWordList.GroupCount) {
                    // if back at start (grp and word same as start)
                    if (FoundWord == StartWord && FoundGrp == StartGrp) {
                        // rest found position so search will end
                        FoundWord = -1;
                        FoundGrp = -1;
                    }
                    break;
                }
                // if not found, action depends on search mode
                if (FindDir == FindDirection.Up) {
                    if (!RestartSearch) {
                        DialogResult rtn;
                        if (Recurse) {
                            rtn = DialogResult.No;
                        }
                        else {
                            rtn = MessageBox.Show(MDIMain,
                                "Beginning of search scope reached. Do you want to continue from the end?",
                                "Find in Word List",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                        }
                        if (rtn == DialogResult.No) {
                            // reset search
                            FindingForm.ResetSearch();
                            MDIMain.UseWaitCursor = false;
                            return;
                        }
                    }
                    else {
                        // entire scope already searched; exit
                        break; // exit do
                    }
                }
                else if (FindDir == FindDirection.Down) {
                    if (!RestartSearch) {
                        DialogResult rtn;
                        if (Recurse) {
                            // just say no
                            rtn = DialogResult.No;
                        }
                        else {
                            rtn = MessageBox.Show(MDIMain,
                                "End of search scope reached. Do you want to continue from the beginning?",
                                "Find in Word List",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                        }
                        if (rtn == DialogResult.No) {
                            // reset search
                            FindingForm.ResetSearch();
                            MDIMain.UseWaitCursor = false;
                            return;
                        }
                    }
                    else {
                        // entire scope already searched; exit
                        break; // exit do
                    }
                }
                else {
                    // search direction is All
                    if (RestartSearch) {
                        break; // exit do
                    }
                }
                // reset search so when we get back to start, search will end
                RestartSearch = true;
                // loop is exited by finding the searchtext or reaching end of search area
            } while (true);
            if (FoundGrp >= 0 && FoundGrp < EditWordList.GroupCount) {
                if (!FirstFind) {
                    // save this position
                    FirstFind = true;
                    StartWord = FoundWord;
                    StartGrp = FoundGrp;
                }
                if (GroupMode) {
                    UpdateSelection(EditWordList.GroupByIndex(FoundGrp).GroupNum, FoundWord, true);
                }
                else {
                    UpdateSelection(EditWordList[FoundWord].WordText, true);
                }
                if (Replacing) {
                    string fullword;
                    if (!MatchWord) {
                        // update replacetext to include the full word with the replaced section
                        if (GroupMode) {
                            ReplaceText = EditWordList.GroupByIndex(FoundGrp).Words[FoundWord].Replace(FindText, ReplaceText);
                            fullword = EditWordList.GroupByIndex(FoundGrp).Words[FoundWord];
                        }
                        else {
                            ReplaceText = EditWordList[FoundWord].WordText.Replace(FindText, ReplaceText);
                            fullword = EditWordList[FoundWord].WordText;
                        }
                    }
                    else {
                        fullword = FindText;
                    }
                    // now try to edit the word
                    if (EditWord(fullword, ReplaceText, EditWordList.GroupByNumber(FoundGrp).GroupNum)) {
                        // change undo
                        UndoCol.Peek().Action = WordsUndo.ActionType.Replace;
                        UpdateSelection(ReplaceText, true);
                        // always reset search when replacing, because
                        // word index almost always changes
                        FindingForm.ResetSearch();

                        // recurse the find method to get the next occurence
                        Recurse = true;
                        FindInWords(FindText, FindDir, MatchWord, false);
                        Recurse = false;
                    }
                }
            }
            else {
                // not found - if not recursing, show a msg
                if (!Recurse) {
                    if (FirstFind) {
                        MessageBox.Show(MDIMain,
                            "The specified region has been searched.",
                            "Find in Word List",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        FirstFind = false;
                    }
                    else {
                        MessageBox.Show(MDIMain,
                            "Search text not found.",
                            "Find in Word List",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                // reset search flags
                FindingForm.ResetSearch();
            }
            MDIMain.UseWaitCursor = false;
        }

        public void ReplaceAll(string FindText, string ReplaceText, bool MatchWord) {

            if (FindText == ReplaceText) {
                return;
            }
            if (ReplaceText.Length == 0) {
                // blank replace text not allowed for words
                MessageBox.Show(MDIMain,
                    "Blank replacement text is not allowed.",
                    "Replace All",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            if (EditWordList.WordCount == 0) {
                // nothing in wordlist,
                MessageBox.Show(MDIMain,
                    "Word list is empty.",
                    "Replace All",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            if (MatchWord) {
                // words are unique, so if replacing entire word, 
                // ReplaceAll is the same as Replace
                FindInWords(FindText, FindDirection.All, true, true, ReplaceText);
                return;
            }

            // replace all occurrences of FindText with ReplaceText
            MDIMain.UseWaitCursor = true;

            int replacecount = 0;
            string findWord, replaceWord;
            int badcount = 0;

            int i = 0;
            do {
                if (EditWordList[i].WordText.Contains(FindText)) {
                    findWord = EditWordList[i].WordText;
                    replaceWord = EditWordList[i].WordText.Replace(FindText, ReplaceText);
                    if (ValidateWord(replaceWord, true) != "!") {
                        if (EditWordList.WordExists(replaceWord)) {
                            EditWord(findWord, replaceWord, EditWordList[i].Group);
                            // only advance if the replaced word is now the current word
                            if (EditWordList.WordIndex(replaceWord) == i) {
                                i++;
                            }
                        }
                        else {
                            EditWord(findWord, replaceWord, EditWordList[i].Group);
                            // only advance if replaced word is current word or
                            // replaced word precedes current word
                            if (EditWordList.WordIndex(replaceWord) <= i) {
                                i++;
                            }
                        }
                        replacecount++;
                    }
                    else {
                        badcount++;
                        i++;
                    }
                }
                else {
                    i++;
                }
            } while (i < EditWordList.WordCount);

            if (replacecount == 0) {
                if (badcount > 0) {
                    MessageBox.Show(MDIMain,
                        "Search text found, but no valid replacements could be made.",
                        "Replace All",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else {
                    MessageBox.Show(MDIMain,
                        "Search text not found.",
                        "Replace All",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            else {
                // create new undo object
                WordsUndo NextUndo = new() {
                    Action = WordsUndo.ActionType.ReplaceAll,
                    GroupNo = replacecount
                };
                // add undo
                AddUndo(NextUndo);
                string msg = "The specified region has been searched. " + replacecount + " replacements were made.";
                switch (badcount) {
                case 0:
                    break;
                case 1:
                    msg += " One replacement which would have caused an invalid word was skipped.";
                    break;
                default:
                    msg += badcount + "replacements which would have caused invalid words were skipped.";
                    break;
                }
                MessageBox.Show(MDIMain,
                 msg,
                "Replace All",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            }
            MDIMain.UseWaitCursor = false;
        }

        private void AddUndo(WordsUndo NextUndo) {
            UndoCol.Push(NextUndo);
            MarkAsChanged();
            FindingForm.ResetSearch();
            FirstFind = false;
        }

        private void SetToolbarStatus() {
            if (dgGroups.Focused) {
                tbbRenumber.Enabled = EditGroupNumber > 1 && EditGroupNumber != 9999;
                tbbCut.Enabled = (EditGroupNumber != -1 &&
                    EditGroupNumber != 0 &&
                    EditGroupNumber != 1 &&
                    EditGroupNumber != 9999);
                tbbDelete.Enabled = tbbCut.Enabled;
                tbbCopy.Enabled = (EditGroupNumber != -1);
            }
            if (dgWords.Focused) {
                tbbRenumber.Enabled = EditWordList.GroupByNumber(EditGroupNumber).WordCount > 0;
                tbbCut.Enabled = (EditWordIndex != 0);
                // for group 0, 1, 9999 disable if no words in group
                if (EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) {
                    if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                        tbbCut.Enabled = false;
                    }
                }
                tbbCopy.Enabled = (EditWordIndex != 0);
                tbbDelete.Enabled = (EditWordIndex != 0);
            }
            tbbPaste.Enabled = Clipboard.ContainsText(TextDataFormat.UnicodeText);
            tbbAddWord.Enabled = EditGroupNumber != 1 && EditGroupNumber != 9999 || EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0;
        }

        private void FinishGroupEdit() {
            EditingGroup = false;
            txtGroupEdit.Visible = false;
            dgGroups.SelectedCells[0].Style.SelectionBackColor = SystemColors.Highlight;
            dgGroups.Select();
        }

        private void FinishWordEdit() {
            EditingWord = false;
            txtWordEdit.Visible = false;
            dgWords.SelectedCells[0].Style.SelectionBackColor = SystemColors.Highlight;
            dgWords.Select();
        }

        private void FormatGroupRow(DataGridViewRow row) {
            string inv = ",.?!();:[]{}'`-\"";
            foreach (string word in EditWordList.GroupByIndex(row.Index).Words) {
                if (word.Any(inv.Contains)) {
                    row.DefaultCellStyle = warningstyle;
                    return;
                }
                // check for irregular start character
                else if (word[0] < 'a' || word[0] > 'z') {
                    row.DefaultCellStyle = warningstyle;
                    return;
                }
            }
            row.DefaultCellStyle = dgGroups.DefaultCellStyle;
        }

        internal void ShowHelp() {
            string topic = "htm\\winagi\\editor_words.htm";
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
            return;
        }

        private bool AskClose() {
            //if (EditWordList.Error != ResourceErrorType.NoError) {
            if (!Visible) {
                // if exiting due to error on form load (form
                // won't be visible)
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this WORDS.TOK file?",
                    "Save WORDS.TOK",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveWords();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "WORDS.TOK file not saved. Continue closing anyway?",
                            "Save WORDS.TOK",
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
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
            tbbUndo.Enabled = UndoCol.Count > 0;
            spGroupCount.Text = "Group Count: " + EditWordList.GroupCount;
            spWordCount.Text = "Word Count: " + EditWordList.WordCount;
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = "Words Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                Text += CompactPath(Filename, 75);
            }
            mnuRSave.Enabled = false;
            MDIMain.btnSaveResource.Enabled = false;
        }
        #endregion
    }

    [Serializable]
    public class WordClipboardData {
        public string WordText { get; set; } = "";
        public string[] Words { get; set; } = [];
        public int GroupNumber { get; set; } = -1;
        public bool IsGroup { get; set; } = false;
    }
}
