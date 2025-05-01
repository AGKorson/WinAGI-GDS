using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Common.API;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using System.Collections;
using EnvDTE;

namespace WinAGI.Editor {
    public partial class frmWordsEdit : Form {
        private bool GroupMode = true;
        public bool InGame;
        public bool IsChanged;
        public WordList EditWordList;
        private bool closing = false;
        private string EditWordListFilename;
        private bool blnRefreshLogics = false;
        private int EditGroupIndex, EditGroupNumber;
        private int EditWordIndex, EditWordGroupIndex;
        private string EditWordText;
        private bool EditingWord = false, EditingGroup = false;
        private bool AddNewWord = false;
        private bool AddNewGroup = false;
        private bool FirstFind = false;
        private bool blnRecurse = false;
        private Font defaultfont;
        private Font boldfont;
        private Stack<WordsUndo> UndoCol = new();
        // StatusStrip Items
        internal ToolStripStatusLabel spGroupCount;
        internal ToolStripStatusLabel spWordCount;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;

        public static frmWordsEdit DragSourceForm { get; private set; }

        public frmWordsEdit() {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
            InitializeComponent();
            InitStatusStrip();
            InitFonts();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers
        private void frmWordsEdit_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible) {
                if (FindingForm.rtfReplace.Visible) {
                    FindingForm.SetForm(FindFormFunction.ReplaceWord, InGame);
                }
                else {
                    FindingForm.SetForm(FindFormFunction.FindWord, InGame);
                }
            }
            //spGroupCount.Text = "Group Count: " + EditWordList.GroupCount;
            //spWordCount.Text = "Word Count: " + EditWordList.WordCount;
        }

        private void frmWordsEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmWordsEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // ensure object is cleared and dereferenced

            if (EditWordList != null) {
                EditWordList.Unload();
                EditWordList = null;
            }
            if (InGame) {
                WEInUse = false;
                WordEditor = null;
            }
        }

        private void frmWordsEdit_Resize(object sender, EventArgs e) {
            //_ = SendMessage(this.Handle, WM_SETREDRAW, false, 0);
            //_ = SendMessage(lstGroups.Handle, WM_SETREDRAW, false, 0);
            //_ = SendMessage(lstWords.Handle, WM_SETREDRAW, false, 0);

            int newWidth = (this.ClientSize.Width - 15) / 2;
            lstGroups.Width = newWidth;
            lstWords.Width = newWidth;
            lstWords.Left = lstGroups.Right + 5;
            label1.Left = 5 + (lstGroups.Width - label1.Width) / 2;
            label2.Left = lstWords.Left + (lstWords.Width - label2.Width) / 2;
            lstWords.Height = ClientSize.Height - lstWords.Top - 5;
            if (GroupMode) {
                lstGroups.Height = lstWords.Height;
            }
            //_ = SendMessage(lstWords.Handle, WM_SETREDRAW, true, 0);
            //_ = SendMessage(lstGroups.Handle, WM_SETREDRAW, true, 0);
            //_ = SendMessage(this.Handle, WM_SETREDRAW, true, 0);
            //this.Refresh();
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
        /// Dynamic function to reset the resource menu.
        /// </summary>
        public void ResetResourceMenu() {
            mnuRSave.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRProperties.Enabled = true;
            mnuRMerge.Enabled = true;
            mnuRGroupCheck.Enabled = true;
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            if (IsChanged) {
                SaveWords();
            }
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            ExportWords();
        }

        public void mnuRProperties_Click(object sender, EventArgs e) {
            EditProperties();
        }

        private void mnuRMerge_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            // merge Words.tok from file
            bool RepeatAnswer = false;
            DialogResult MergeReplace = DialogResult.No;
            int lngCount = 0, lngRepl = 0;

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
            DefaultResDir = JustPath(MDIMain.OpenDlg.FileName);

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
                MergeList = new(MDIMain.OpenDlg.FileName);
                ProgressWin.pgbStatus.Maximum = MergeList.WordCount;
            }
            catch (Exception ex) {
                ProgressWin.Close();
                ErrMsgBox(ex, "An error occurred while trying to load " + Path.GetFileName(MDIMain.OpenDlg.FileName) + ":",
                "Unable to merge the file.",
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
                                WinAGIHelp, "htm\\winagi\\Words_Editor.htm#merge");
                        }
                        if (MergeReplace == DialogResult.Yes) {
                            // remove it from previous group
                            EditWordList.RemoveWord(MergeWord);
                            // add it to new group
                            EditWordList.AddWord(MergeWord, GroupNum);
                            lngRepl++;
                        }
                    }
                }
                else {
                    // not in current list- ok to add
                    EditWordList.AddWord(MergeWord, GroupNum);
                    lngCount++;
                }
                ProgressWin.pgbStatus.Value++;
                ProgressWin.Refresh();
            }
            MDIMain.UseWaitCursor = false;
            ProgressWin.Close();
            string msg = "";
            if (lngCount > 0) {
                // refresh form
                lstGroups.Items.Clear();
                lstWords.Items.Clear();
                if (GroupMode) {
                    for (int i = 0; i < EditWordList.GroupCount; i++) {
                        lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                    }
                }
                else {
                    foreach (AGIWord word in EditWordList) {
                        lstWords.Items.Add(word.WordText);
                    }
                }
                UpdateSelection(0, 0, true);
                msg = "Added " + lngCount + " words. Replaced " + lngRepl + " words.";
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

            bool blnDontAsk = false;
            AskOption RepeatAnswer = AskOption.Ask;
            DialogResult rtn = DialogResult.No;
            bool[] GroupUsed = [];
            int UnusedCount = 0;

            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.IsChanged) {
                        switch (RepeatAnswer) {
                        case AskOption.Ask:
                            rtn = MsgBoxEx.Show(MDIMain,
                            "Do you want to save this logic before checking word usage?",
                            "Update " + ResourceName(frm.EditLogic, true, true) + "?",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            "Repeat this answer for all other open logics.", ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    RepeatAnswer = AskOption.Yes;
                                }
                                else if (rtn == DialogResult.No) {
                                    RepeatAnswer = AskOption.No;
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
            StringList stlOutput = new();
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
                Clipboard.SetText(stlOutput.Text, TextDataFormat.Text);
                string strCount;
                if (UnusedCount == 1) {
                    strCount = "is one unused word group";
                }
                else {
                    strCount = "are " + UnusedCount + " unused word groups";
                }
                MessageBox.Show(MDIMain,
                    "There " + strCount + " in this game. \r\r" +
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
                mnuEUndo.Text = "Undo " + LoadResString(WORDSUNDOTEXT + (int)UndoCol.Peek().Action);
            }
            else {
                mnuEUndo.Enabled = false;
                mnuEUndo.Text = "Undo";
            }
            // cut is enabled if mode is group or word, and a group or word is selected
            //   NOT grp 0, 1, 1999
            mnuECut.Visible = true;
            if (lstGroups.Focused) {
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
            else if (lstGroups.Focused) {
                mnuEPaste.Enabled = Clipboard.ContainsData(WORDSTOK_CB_FMT);
                mnuEPaste.Text = "Paste Group";
            }
            else {
                if (Clipboard.ContainsData(WORDSTOK_CB_FMT)) {
                    mnuEPaste.Enabled =  true;
                    mnuEPaste.Text = "Paste Word";
                }
                else if (Clipboard.ContainsText()) {
                    mnuEPaste.Enabled =  true;
                    mnuEPaste.Text = "Paste As Word";
                }
                else {
                    mnuEPaste.Enabled =  false;
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
            mnuEInsertGroup.Visible = lstGroups.Focused;
            mnuEInsertWord.Visible = lstWords.Focused;
            mnuEInsertWord.Enabled = true;
            // if group 1 or 9999 insert only allowed if empty
            if (EditGroupNumber == 1 || EditGroupNumber == 9999) {
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount != 0) {
                    mnuEInsertWord.Enabled = false;
                }
            }
            // ffind again depends on search status
            mnuEFindAgain.Enabled = GFindText.Length > 0;
            if (lstGroups.Focused) {
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
            if (lstGroups.Focused) {
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
                        lstWords.Items.Insert(index, NextUndo.Group[i]);
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
                        lstWords.Items.Clear();
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
                    string strTemp = NextUndo.Group[i];
                    string[] strWords = strTemp.Split("|");
                    int lngGroup = int.Parse(strWords[0]);
                    for (int j = 1; j < strWords.Length; j++) {
                        EditWordList.AddWord(strWords[j], lngGroup);
                    }
                }
                lstGroups.Items.Clear();
                lstWords.Items.Clear();
                if (GroupMode) {
                    for (int i = 0; i < EditWordList.GroupCount; i++) {
                        lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                    }
                }
                else {
                    foreach (AGIWord word in EditWordList) {
                        lstWords.Items.Add(word.WordText);
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
                mnuEDelete_Click(sender, e);
                if (UndoCol.Peek().Action == WordsUndo.ActionType.DelGroup) {
                    UndoCol.Peek().Action = WordsUndo.ActionType.CutGroup;
                }
                else if (UndoCol.Peek().Action == WordsUndo.ActionType.DelWord) {
                    UndoCol.Peek().Action = WordsUndo.ActionType.CutWord;
                }
            }
        }

        private bool CanCut() {
            // cut(delete) is enabled if mode is group or word, and a group or word is selected
            //   NOT grp 0, 1, 1999
            if (EditingGroup || EditingWord) {
                return false;
            }
            if (lstGroups.Focused) {
                return EditGroupNumber != -1 &&
                    EditGroupNumber != 0 &&
                    EditGroupNumber != 1 &&
                    EditGroupNumber != 9999;
            }
            else {
                // for group 0, 1, 9999 disable if no words in group
                if (EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) {
                    if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                        return false;
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
            if (lstGroups.Focused) {
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
            else if (lstWords.Focused) {
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
            string strMsg = "";
            if (CanPaste()) {
                if (lstGroups.Focused) {
                    WordClipboardData groupdata = Clipboard.GetData(WORDSTOK_CB_FMT) as WordClipboardData;
                    if (groupdata == null) {
                        // no custom clipboard data
                        MessageBox.Show(MDIMain,
                            "The clipboard doesn't contain a valid group.",
                            "Nothing to Paste",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else {
                        // clipboard contains a single group
                        WordsUndo NextUndo = new();
                        NextUndo.Action = WordsUndo.ActionType.PasteGroup;
                        NextUndo.Group = [];
                        for (int i = 0; i < groupdata.Words.Length; i++) {
                            if (EditWordList.WordExists(groupdata.Words[i])) {
                                // word is in use
                                strMsg += "\r\t" + groupdata.Words[i] + " (in group " + EditWordList[groupdata.Words[i]].Group + ")";
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
                        int lngNewGrpNo;
                        if (EditWordList.GroupExists(groupdata.GroupNumber)) {
                            lngNewGrpNo = NextGrpNum();
                        }
                        else {
                            lngNewGrpNo = groupdata.GroupNumber;
                        }
                        // add group without undo
                        AddGroup(lngNewGrpNo, true);
                        for (int i = 0; i < NextUndo.Group.Length; i++) {
                            // add word (without undo)
                            AddWord(lngNewGrpNo, NextUndo.Group[i], true);
                        }
                        NextUndo.GroupNo = lngNewGrpNo;
                        // the words added aren't needed
                        NextUndo.Group = [];
                        AddUndo(NextUndo);
                        if (strMsg.Length > 0) {
                            MessageBox.Show(MDIMain,
                                "The following words were not added because they already exist in another group: " + strMsg,
                                "Paste Group from Clipboard",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        // select the pasted group
                        UpdateSelection(lngNewGrpNo, 0);
                    }
                    return;
                }
                if (lstWords.Focused) {
                    string strWord = "";
                    // check clipboard for custom data
                    WordClipboardData worddata = Clipboard.GetData(WORDSTOK_CB_FMT) as WordClipboardData;
                    string clipboardword = "";
                    if (worddata != null && !worddata.IsGroup) {
                        // clipboard contains a single word
                        clipboardword = worddata.WordText;
                    }
                    else if (Clipboard.ContainsText(TextDataFormat.Text)) {
                        strWord = Clipboard.GetText(TextDataFormat.Text).ToLower();
                        if (strWord.Contains('\n') || strWord.Contains("\r")) {
                            strWord = "";
                        }
                        if (strWord.Length > 0 && strWord[0] == '"') {
                            strWord = strWord[1..];
                        }
                        if (strWord.Length > 0 && strWord[^1] == '"') {
                            strWord = strWord[..^1];
                        }
                        if (strWord.Length > 0) {
                            strWord = CheckWord(strWord);
                        }
                        if (strWord.Length > 0) {
                            // this word is acceptable
                            clipboardword = strWord;
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
                        int lngOldGroupNo;
                        if (EditWordList.WordExists(clipboardword)) {
                            lngOldGroupNo = EditWordList[clipboardword].Group;
                            if (lngOldGroupNo == EditGroupNumber) {
                                MessageBox.Show(MDIMain,
                                    "'" + clipboardword + "' already exists in this group.",
                                    "Unable to Paste",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                return;
                            }
                            // word is in another group- ask if word should be moved
                            if (MessageBox.Show("'" + clipboardword + "' already exists (in group " + lngOldGroupNo + "). Do you want to move it to this group?",
                                "Move Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.No) {
                                return;
                            }
                            // delete word from other group
                            DeleteWord(clipboardword, true);
                        }
                        else {
                            lngOldGroupNo = -1;
                        }
                        // add word to this group
                        AddWord(EditGroupNumber, clipboardword, true);

                        // add undo
                        WordsUndo NextUndo = new();
                        NextUndo.Action = WordsUndo.ActionType.PasteWord;
                        NextUndo.GroupNo = EditGroupNumber;
                        NextUndo.OldGroupNo = lngOldGroupNo;
                        NextUndo.Word = clipboardword;
                        AddUndo(NextUndo);

                        //select the pasted word
                        UpdateSelection(clipboardword);
                    }
                }
            }

            static string CheckWord(string strWord) {
                string retval = strWord.SingleSpace().Trim().LowerAGI();
                if ("!\"'(),-.:;?[]`{}".Any(retval.Contains)) {
                    return "";
                }
                if ("#$%&*+/0123456789<=>@\\^_|~".Contains(retval[0])) {
                    // not allowed unless supporting power pack
                    if (EditGame == null || !EditGame.PowerPack) {
                        return "";
                    }
                }
                // extended characters
                if (retval.Any(c => c > 127)) {
                    // not allowed unless supporting power pack
                    if (EditGame == null || !EditGame.PowerPack) {
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
            if (lstGroups.Focused) {
                return Clipboard.ContainsData(WORDSTOK_CB_FMT);
            }
            else {
                return Clipboard.ContainsText() || Clipboard.ContainsData(WORDSTOK_CB_FMT);
            }
        }

        private void mnuEDelete_Click(object sender, EventArgs e) {
            if (CanCut()) {
                if (lstGroups.Focused) {
                    if (EditGroupNumber != -1) {
                        int groupindex = EditGroupIndex;
                        int wordindex = EditWordIndex;
                        DeleteGroup(EditGroupNumber);
                        if (GroupMode) {
                            if (groupindex == lstGroups.Items.Count) {
                                groupindex--;
                            }
                            UpdateSelection(EditWordList.GroupByIndex(groupindex).GroupNum, 0, true);
                        }
                        else {
                            if (wordindex == lstWords.Items.Count) {
                                wordindex--;
                            }
                            UpdateSelection(wordindex, true);
                        }
                    }
                }
                if (lstWords.Focused) {
                    if (EditGroupNumber == -1) {
                        return;
                    }
                    switch (EditGroupNumber) {
                    case 0:
                        //'a' and 'i' are special
                        if (EditWordText == "a" || EditWordText == "i") {
                            if (MessageBox.Show(MDIMain,
                                $"The word '{EditWordText}' is usually associated with group 0. Are " +
                                "you sure you want to delete it?",
                                "Delete Group 0 Word",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, 0, 0,
                                WinAGIHelp, "htm\\winagi\\Words_Editor.htm#reserved") == DialogResult.Yes) {
                                DeleteWord(EditWordText);
                            }
                        }
                        else {
                            // more than one, allow it
                            DeleteWord(EditWordText);
                        }
                        break;
                    case 1:
                        // 'anyword' is special
                        if (EditWordText == "anyword") {
                            if (MessageBox.Show(MDIMain,
                                $"The word 'anyword' is the Sierra default placeholder word for reserved group 1. " +
                                "Deleting it is not advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 1 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, 0, 0,
                                WinAGIHelp, "htm\\winagi\\Words_Editor.htm#reserved") == DialogResult.Yes) {
                                DeleteWord(EditWordText);
                            }
                        }
                        else if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 1) {
                            if (MessageBox.Show(MDIMain,
                                $"Group '1' is a reserved group. Deleting its placeholder is not " +
                                "advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 1 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, 0, 0,
                                WinAGIHelp, "htm\\winagi\\Words_Editor.htm#reserved") == DialogResult.Yes) {
                                DeleteWord(EditWordText);
                            }
                        }
                        else {
                            // more than one?? allow it
                            DeleteWord(EditWordText);
                        }
                        break;
                    case 9999:
                        // 'rol' is special
                        if (EditWordText == "rol") {
                            if (MessageBox.Show(MDIMain,
                                $"The word 'rol' is the Sierra default placeholder word for reserved group 9999. " +
                                "Deleting it is not advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 9999 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, 0, 0,
                                WinAGIHelp, "htm\\winagi\\Words_Editor.htm#reserved") == DialogResult.Yes) {
                                DeleteWord(EditWordText);
                            }
                        }
                        else if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 1) {
                            if (MessageBox.Show(MDIMain,
                                $"Group '9999' is a reserved group. Deleting its placeholder is not " +
                                "advised.\n\nAre you sure you want to delete it?",
                                "Delete Group 9999 Placeholder",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question, 0, 0,
                                WinAGIHelp, "htm\\winagi\\Words_Editor.htm#reserved") == DialogResult.Yes) {
                                DeleteWord(EditWordText);
                            }
                        }
                        else {
                            // more than one?? allow it
                            DeleteWord(EditWordText);
                        }
                        break;
                    default:
                        DeleteWord(EditWordText);
                        break;
                    }
                    // select next word, or if the grp is
                    // gone select next group
                    if (!GroupMode || EditWordList.GroupExists(EditGroupNumber)) {
                        if (GroupMode) {
                            if (EditWordGroupIndex == lstWords.Items.Count) {
                                EditWordGroupIndex--;
                            }
                            UpdateSelection(EditGroupNumber, EditWordGroupIndex, true);
                        }
                        else {
                            if (EditWordIndex == lstWords.Items.Count) {
                                EditWordIndex--;
                            }
                            UpdateSelection(EditWordIndex, true);
                        }
                    }
                    else {
                        if (EditGroupIndex == lstGroups.Items.Count) {
                            EditGroupIndex--;
                        }
                        UpdateSelection(EditWordList.GroupByIndex(EditGroupIndex).GroupNum, 0, true);
                    }
                }
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
            if (sender == tbbRenumber || lstGroups.Focused) {
                lstGroups_DoubleClick(sender, e);
            }
            if (lstWords.Focused) {
                lstWords_DoubleClick(sender, e);
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
            GMatchWord = true;
            GFindSynonym = lstGroups.Focused;
            GFindGrpNum = EditGroupNumber;
            FindingForm.SetForm(FindFormFunction.FindWordsLogic, true);
            GFindText = '"' + EditWordText + '"';
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
                //string curword = lstWords.Text;
                label1.Text = "Groups";
                lstGroups.Height = lstWords.Height;
                lstGroups.Items.Clear();
                for (int i = 0; i < EditWordList.GroupCount; i++) {
                    lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                }
                lstGroups.SelectionMode = SelectionMode.One;
                lstGroups.SelectedIndex = EditGroupIndex;
                lstWords.Items.Clear();
                for (int i = 0; i < EditWordList.GroupByIndex(EditGroupIndex).WordCount; i++) {
                    lstWords.Items.Add(EditWordList.GroupByIndex(EditGroupIndex).Words[i]);
                }
                lstWords.Text = EditWordText;
                buttonicon = (byte[])EditorResources.ResourceManager.GetObject("ewi_bygroup");
            }
            else {
                //string curword = lstWords.Text;
                label1.Text = "Group Number:";
                lstGroups.Items.Clear();
                lstGroups.Height = txtGroupEdit.Height + 4;
                lstGroups.SelectionMode = SelectionMode.None;
                lstWords.Items.Clear();
                foreach (AGIWord word in EditWordList) {
                    lstWords.Items.Add(word.WordText);
                }
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                    lstGroups.Items.Add("");
                    UpdateSelection(EditWordList[0].WordText, true);
                }
                else {
                    lstGroups.Items.Add((EditGroupNumber.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName);
                }
                lstWords.Text = EditWordText;
                buttonicon = (byte[])EditorResources.ResourceManager.GetObject("ewi_byword");
            }
            Stream stream = new MemoryStream(buttonicon);
            tbbMode.Image = (Bitmap)Image.FromStream(stream);
            label1.Left = 5 + (lstGroups.Width - label1.Width) / 2;
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
            if (EditGame != null) {
                CharPicker = new(EditGame.CodePage.CodePage);
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
        private void lstGroups_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void lstGroups_DoubleClick(object sender, EventArgs e) {
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
            Point location = lstGroups.GetItemRectangle(index).Location;
            location.Offset(lstGroups.Location + new Size(3, 1));
            txtGroupEdit.Location = location;
            txtGroupEdit.Size = lstGroups.GetItemRectangle(index).Size;
            txtGroupEdit.Visible = true;
            txtGroupEdit.Select();
        }

        private void lstGroups_Enter(object sender, EventArgs e) {
            label1.Font = boldfont;
            SetToolbarStatus();
        }

        private void lstGroups_Leave(object sender, EventArgs e) {
            if (lstWords.Focused) {
                label1.Font = defaultfont;
            }
        }

        private void lstGroups_MouseDown(object sender, MouseEventArgs e) {
            if (lstGroups.SelectionMode != SelectionMode.None) {
                int selitem = (e.Y / lstGroups.ItemHeight) + lstGroups.TopIndex;
                if (selitem >= lstGroups.Items.Count) {
                    selitem = lstGroups.Items.Count - 1;
                }
                if (EditGroupIndex != selitem) {
                    UpdateSelection(EditWordList.GroupByIndex(selitem).GroupNum, 0, true);
                    // always reset search
                    StartWord = -1;
                    StartGrp = -1;
                }
            }
            if (e.Button == MouseButtons.Right) {
                lstGroups.Select();
            }
        }

        private void lstGroups_MouseUp(object sender, MouseEventArgs e) {
            if (GroupMode && lstGroups.SelectedIndex != EditGroupIndex) {
                UpdateSelection(EditWordList.GroupByIndex(lstGroups.SelectedIndex).GroupNum, 0, true);
            }
        }

        private void lstGroups_DragEnter(object sender, DragEventArgs e) {
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

        private void lstGroups_DragOver(object sender, DragEventArgs e) {
            if (GroupMode && DragWord) {
                // convert screen coordinates to lstGroup coordinates
                Point cP = lstGroups.PointToClient(new Point(e.X, e.Y));

                int selitem = (cP.Y / lstGroups.ItemHeight) + lstGroups.TopIndex;
                if (selitem < 0) {
                    selitem = 0;
                }
                if (selitem >= lstGroups.Items.Count) {
                    selitem = lstGroups.Items.Count - 1;
                }
                if (selitem != lstGroups.SelectedIndex) {
                    lstGroups.SelectedIndex = selitem;
                }
            }
        }

        private void lstGroups_DragLeave(object sender, EventArgs e) {
            if (GroupMode && DragWord) {
                if (lstGroups.SelectedIndex != EditGroupIndex) {
                    lstGroups.SelectedIndex = EditGroupIndex;
                }
            }
        }

        private void lstGroups_DragDrop(object sender, DragEventArgs e) {
            if (GroupMode) {
                string dropword = (string)e.Data.GetData(DataFormats.Text);
                if (dropword.Length > 0) {
                    int groupnum = EditWordList.GroupByIndex(lstGroups.SelectedIndex).GroupNum;
                    if (this == DragSourceForm) {
                        if (EditWordList[dropword].Group != groupnum) {
                            // check for last word of group 1,9999
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
                                lstGroups.SelectedIndex = EditGroupIndex;
                            }
                        }
                    }
                    else {
                        if (EditWordList.WordExists(dropword)) {
                            MessageBox.Show(MDIMain,
                                $"'{dropword}' is already present in this list. ",
                                "Duplicate Word",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            lstGroups.SelectedIndex = EditGroupIndex;
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
                lstGroups.Select();
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

        private void lstWords_DoubleClick(object sender, EventArgs e) {
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

        private void lstWords_Enter(object sender, EventArgs e) {
            label2.Font = boldfont;
            SetToolbarStatus();
        }

        private void lstWords_Leave(object sender, EventArgs e) {
            if (lstGroups.Focused) {
                label2.Font = defaultfont;
            }
        }

        private void lstWords_MouseDown(object sender, MouseEventArgs e) {
            int selitem = (e.Y / lstWords.ItemHeight) + lstWords.TopIndex;
            if (selitem >= lstWords.Items.Count) {
                selitem = lstWords.Items.Count - 1;
            }
            if ((EditGroupNumber == 0 || EditGroupNumber == 1 || EditGroupNumber == 9999) &&
                EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                selitem = -1;
            }
            // in groupmode, selindex corresponds to EditWordGroupIndex
            // in wordmode, selindex corresponds to EditWordIndex
            if (GroupMode) {
                if (EditWordGroupIndex != selitem) {
                    UpdateSelection((string)lstWords.Items[selitem]);
                    // always reset search
                    StartWord = -1;
                    StartGrp = -1;
                }
            }
            else {
                if (EditWordIndex != selitem) {
                    UpdateSelection((string)lstWords.Items[selitem]);
                    // always reset search
                    StartWord = -1;
                    StartGrp = -1;
                }
            }
            if (e.Button == MouseButtons.Right) {
                lstWords.Select();
            }
        }

        private void lstWords_MouseUp(object sender, MouseEventArgs e) {
            if (GroupMode) {
                if (EditWordGroupIndex != lstWords.SelectedIndex) {
                    UpdateSelection((string)lstWords.Items[lstWords.SelectedIndex]);
                }
            }
            else {
                if (EditWordIndex != lstWords.SelectedIndex) {
                    UpdateSelection((string)lstWords.Items[lstWords.SelectedIndex]);
                }
            }
            Debug.Print("mouse up");
        }

        private void lstWords_MouseMove(object sender, MouseEventArgs e) {
            if (GroupMode && !DragWord && e.Button == MouseButtons.Left) {
                DragWord = true;
                DragSourceForm = this;
                lstWords.DoDragDrop(EditWordText, DragDropEffects.Move);
            }
        }

        private void lstWords_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
            if (e.Action == DragAction.Drop) {
                DragWord = false;
            }
            if (e.Action == DragAction.Cancel) {
            }
        }

        private void txtWordEdit_Validating(object sender, CancelEventArgs e) {
            if (!EditingWord || txtWordEdit.Text.Trim() == EditWordText) {
                if (AddNewGroup) {
                    // don't add word undo
                    UndoCol.Pop();
                    AddNewGroup = false;
                }
                FinishWordEdit();
                return;
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
                // TODO: delete word
                FinishWordEdit();
                return;
            case "!":
                // invalid word
                e.Cancel = true;
                return;
            default:
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0) {
                    // it's group 0/1/9999 with no word
                    if (GroupMode) {
                        lstWords.Items.Clear();
                    }
                    AddWord(EditGroupNumber, newword);
                }
                else {
                    // word is ok to change
                    EditWord(EditWordText, newword, EditGroupNumber);
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
                lstWords.Select();
                return;
            case Keys.Escape:
                FinishWordEdit();
                break;
            //case >= Keys.D0 and <= Keys.D9:
            case Keys.Back:
            case Keys.Left:
            case Keys.Right:
                break;
            default:
                //switch (e.KeyValue) {
                //case >= 97 and <= 122:
                //    break;
                //}
                //    e.SuppressKeyPress = true;
                //    e.Handled = true;
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
                    if (EditGame == null || !EditGame.PowerPack) {
                        e.Handled = true;
                    }
                }
                break;
            case > 127:
                // extended chars not allowed
                // UNLESS supporting the Power Pack mod
                if (EditGame == null || !EditGame.PowerPack) {
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
            lstGroups.Font = defaultfont;
            if (GroupMode && (EditGroupNumber == 1 || EditGroupNumber == 9999)) {
                lstWords.Font = boldfont;
                lstWords.ForeColor = Color.DarkGray;
            }
            else {
                lstWords.Font = defaultfont;
                lstWords.ForeColor = Color.Black;
            }
            lstGroups.Top = label1.Bottom;
            txtGroupEdit.Top = lstWords.Top = lstGroups.Top;
        }

        public bool LoadWords(WordList loadwords) {
            InGame = loadwords.InGame;
            IsChanged = loadwords.IsChanged;
            try {
                if (InGame) {
                    loadwords.Load();
                }
            }
            catch {
                return false;
            }
            if (loadwords.ErrLevel < 0) {
                return false;
            }
            EditWordList = loadwords.Clone();
            EditWordListFilename = loadwords.ResFile;

            lstGroups.Items.Clear();
            for (int i = 0; i < EditWordList.GroupCount; i++) {
                lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
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
                if (EditWordListFilename.Length > 0) {
                    Text += Common.Base.CompactPath(EditWordListFilename, 75);
                }
                else {
                    WrdCount++;
                    Text += "NewWords" + WrdCount.ToString();
                }
            }

            mnuRSave.Enabled = !IsChanged;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = !IsChanged;
            return true;
        }

        public void ImportWords(string importfile) {
            // load importfile, and replace existing words.tok

            WordList tmpList;

            MDIMain.UseWaitCursor = true;
            try {
                tmpList = new(importfile);
            }
            catch (Exception e) {
                ErrMsgBox(e, "An error occurred during import:", "", "Import WORDS.TOK File Error");
                MDIMain.UseWaitCursor = false;
                return;
            }
            // replace current wordlist
            EditWordList = tmpList;
            EditWordListFilename = importfile;
            lstGroups.Items.Clear();
            for (int i = 0; i < EditWordList.GroupCount; i++) {
                lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
            }
            UpdateSelection(0, true);
            UndoCol = new();
            tbbUndo.Enabled = false;
            MarkAsChanged();
            MDIMain.UseWaitCursor = false;
        }

        public void SaveWords() {
            // TODO: autoupdate still needs significant work; disable it for now
            //bool blnDontAsk = false;
            //DialogResult rtn = DialogResult.No;
            //if (InGame && WinAGISettings.AutoUpdateWords != 1) {
            //    if (WinAGISettings.AutoUpdateWords == 0) {
            //        rtn = MsgBoxEx.Show(MDIMain,
            //            "Do you want to update all game logics with the changes made in the word list?",
            //            "Update Logics?",
            //            MessageBoxButtons.YesNo,
            //            MessageBoxIcon.Question,
            //            "Always take this action when saving the word list.", ref blnDontAsk);
            //        if (blnDontAsk) {
            //            if (rtn == DialogResult.Yes) {
            //                WinAGISettings.AutoUpdateWords = 2;
            //            }
            //            else {
            //                WinAGISettings.AutoUpdateWords = 1;
            //            }
            //        }
            //    }
            //    else {
            //        rtn = DialogResult.Yes;
            //    }
            //    if (rtn == DialogResult.Yes) {
            //        AutoUpdate();
            //    }
            //}

            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool loaded = EditGame.WordList.Loaded;
                if (!loaded) {
                    EditGame.WordList.Load();
                }
                EditGame.WordList.CloneFrom(EditWordList);
                try {
                    EditGame.WordList.Save();
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "Error during WORDS.TOK compilation: ",
                        "Existing WORDS.TOK has not been modified.",
                        "WORDS.TOK Compile Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                if (blnRefreshLogics) {
                    MakeAllChanged();
                    blnRefreshLogics = false;
                }
                RefreshTree(AGIResType.Words, 0);
                MDIMain.ClearWarnings(AGIResType.Words, 0);
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
                        ErrMsgBox(ex, "An Error occurred while saving this wordlist:",
                            "Existing wordlist file has not been modified.",
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
                EditWordListFilename = EditWordList.ResFile;
                MarkAsSaved();
            }
        }

        public void EditProperties() {
            string strDesc = EditWordList.Description;
            string id = "";
            if (GetNewResID(AGIResType.Words, -1, ref id, ref strDesc, InGame, 2)) {
                EditWordList.Description = strDesc;
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
                string strTemp = EditWordList.GroupByIndex(i).GroupNum.ToString();
                for (int j = 0; j < EditWordList.GroupByIndex(i).WordCount; j++) {
                    strTemp += "|" + EditWordList.GroupByIndex(i).Words[j];
                }
                NextUndo.Group[i] = strTemp;
            }
            AddUndo(NextUndo);
            //  force logics refresh
            blnRefreshLogics = true;

            EditWordList.Clear();
            if (DefaultWords) {
                // add "a" and "rol" and "anyword"
                EditWordList.AddWord("a", 0);
                EditWordList.AddWord("anyword", 1);
                EditWordList.AddWord("rol", 9999);
            }
            lstGroups.Items.Clear();
            lstWords.Items.Clear();
            if (GroupMode) {
                for (int i = 0; i < EditWordList.GroupCount; i++) {
                    lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                }
            }
            else {
                foreach (AGIWord word in EditWordList) {
                    lstWords.Items.Add(word.WordText);
                }
            }
            UpdateSelection(0, 0);
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
                if (force || lstGroups.SelectedIndex != EditGroupIndex) {
                    lstWords.Items.Clear();
                    foreach (string word in EditWordList.GroupByNumber(EditGroupNumber)) {
                        lstWords.Items.Add(word);
                    }
                    if (lstWords.Items.Count == 0) {
                        if (EditGroupNumber == 1) {
                            lstWords.Items.Add("<group 1: any word>");
                        }
                        else if (EditGroupNumber == 9999) {
                            lstWords.Items.Add("<group 9999: rest of line>");
                        }
                    }
                    lstGroups.Items[EditGroupIndex] = (EditGroupNumber.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName;
                }
                lstGroups.SelectedIndex = EditGroupIndex;
                lstWords.SelectedIndex = EditWordGroupIndex;
            }
            else {
                lstWords.SelectedIndex = EditWordIndex;
                lstGroups.Items[0] = (EditGroupNumber.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName;
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
        }

        private int ValidateGroup(string groupnumtext) {
            if (!groupnumtext.IsNumeric()) {
                MessageBox.Show(MDIMain,
                    "Group number must be numeric.",
                    "Renumber Group Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\words.htm#16bit");
                return -1;
            }
            int lngNewGrpNum = groupnumtext.IntVal();
            if (lngNewGrpNum > 65535) {
                MessageBox.Show(MDIMain,
                    "Invalid group number. Must be less than 65536.",
                    "Renumber Group Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp, "htm\\agi\\words.htm#16bit");
                return -1;
            }
            if (lngNewGrpNum != EditGroupNumber) {
                // if the new number is already in use
                if (EditWordList.GroupExists(lngNewGrpNum)) {
                    // not a valid new group number
                    MessageBox.Show(MDIMain,
                        "Group " + lngNewGrpNum + " is already in use. Choose another number.",
                        "Renumber Group Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return -1;
                }
            }
            return lngNewGrpNum;
        }

        private void DeleteGroup(int group, bool DontUndo = false) {
            if (group < 2 || group == 9999) {
                if (!DontUndo) {
                    MessageBox.Show(MDIMain,
                        $"Group '{EditGroupNumber}' is  a reserved group " +
                        " and cannot be deleted.",
                        "Reserved Word Group",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp, "htm\\winagi\\Words_Editor.htm#reserved");
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
                lstGroups.Items.RemoveAt(EditGroupIndex);
                EditGroupIndex = EditWordList.GroupIndexFromNumber(NewGroupNo);
                EditGroupNumber = NewGroupNo;
                lstGroups.Items.Insert(EditGroupIndex, (EditGroupNumber.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName);
                UpdateSelection(EditGroupNumber, EditWordGroupIndex);
            }
            else {
                EditGroupIndex = EditWordList.GroupIndexFromNumber(NewGroupNo);
                UpdateSelection(NewGroupNo, 0, true);
            }
            lstGroups.Refresh();
            if (!DontUndo) {
                WordsUndo NextUndo = new();
                NextUndo.Action = WordsUndo.ActionType.Renumber;
                NextUndo.OldGroupNo = OldGroupNo;
                NextUndo.GroupNo = NewGroupNo;
                // add it
                AddUndo(NextUndo);
            }
            // force logics refresh
            blnRefreshLogics = true;
            return;
        }

        private void BeginWordEdit() {
            int index = lstWords.SelectedIndex;
            EditingWord = true;
            // configure the TextBox for in-place editing
            txtWordEdit.Text = EditWordText;
            Point location = lstWords.GetItemRectangle(index).Location;
            location.Offset(lstWords.Location + new Size(3, 2));
            txtWordEdit.Location = location;
            txtWordEdit.Size = lstWords.GetItemRectangle(index).Size;
            txtWordEdit.Visible = true;
            txtWordEdit.Select();
        }

        public void NewWord(int group) {
            string strNewWord = GetUniqueWord();
            if (strNewWord.Length == 0) {
                return;
            }
            if ((group == 0 || group == 1 || group == 9999) && EditWordList.GroupByNumber(group).WordCount == 0) {
                // remove the placeholder
                if (GroupMode) {
                    lstWords.Items.RemoveAt(0);
                }
            }
            AddWord(group, strNewWord);
            AddNewWord = true;
            BeginWordEdit();
        }

        private string GetUniqueWord() {
            int newindex = 0;
            string strNewWord;
            do {
                newindex++;
                strNewWord = "new word " + newindex;
                if (!EditWordList.WordExists(strNewWord)) {
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
            return strNewWord;
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
                    MessageBox.Show(MDIMain,
                        "This word contains one or more invalid characters.",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp, "htm\\agi\\words.htm#charlimits");
                }
                return "!";
            }
            if ("#$%&*+/0123456789<=>@\\^_|~".Contains(retval[0])) {
                // not allowed unless supporting power pack
                if (EditGame == null || !EditGame.PowerPack) {
                    if (!Quiet) {
                        MessageBox.Show(MDIMain,
                        "This word begins with invalid characters.",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp, "htm\\agi\\words.htm#charlimits");
                    }
                    return "!";
                }
            }
            // extended characters
            if (retval.Any(c => c > 127)) {
                // not allowed unless supporting power pack
                if (EditGame == null || !EditGame.PowerPack) {
                    if (!Quiet) {
                        MessageBox.Show(MDIMain,
                        "This word contains one or more extended characters (> 128).",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp, "htm\\agi\\words.htm#charlimits");
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

        private bool EditWord(string oldWord, string newWord, int thisGroup, bool DontUndo = false) {
            bool blnFirst = EditWordList.GroupByNumber(thisGroup).GroupName == oldWord;
            bool blnMoving = EditWordList.WordExists(newWord);
            int oldgroupnum;

            if (blnMoving) {
                oldgroupnum = EditWordList[newWord].Group;
                // if this is first word in the OLD group
                bool blnDelFirst = (EditWordList.GroupByNumber(oldgroupnum).GroupName == newWord);
                // delete new word from its old group (so it can be added to new group)
                if (EditWordList.GroupByNumber(oldgroupnum).WordCount <= 1) {
                    // only one word, so delete entire group
                    RemoveAGroup(oldgroupnum);
                }
                else {
                    // remove only this word
                    RemoveAWord(newWord);
                    // if it was first word
                    if (blnDelFirst) {
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
            if ((blnFirst || EditWordList.GroupByNumber(thisGroup).GroupName == newWord)) {
                UpdateGroupName(thisGroup);
            }

            // if not skipping undo
            if (!DontUndo) {
                if (AddNewGroup) {
                    // change last undo object
                    UndoCol.Peek().Word = newWord;
                    UndoCol.Peek().OldGroupNo = oldgroupnum;
                }
                else if (AddNewWord) {
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
                    blnRefreshLogics = true;
                }
                // select the new word
                UpdateSelection(newWord, true);
                AddNewWord = false;
                AddNewGroup = false;
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
            bool blnFirst = EditWordList.GroupByNumber(OldGrpNum).GroupName == WordText;
            if ((OldGrpNum == 0 || OldGrpNum == 1 || OldGrpNum == 9999) && EditWordList.GroupByNumber(OldGrpNum).WordCount == 1) {
                EditWordList.RemoveWord(WordText);
            }
            else if (EditWordList.GroupByNumber(OldGrpNum).WordCount > 1) {
                RemoveAWord(WordText);
                if (blnFirst) {
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
                //force logic refresh
                blnRefreshLogics = true;
            }
            if (EditWordList.GroupByNumber(NewGrpNum).GroupName == WordText) {
                UpdateGroupName(NewGrpNum);
            }
            //UpdateSelection(WordText, true);
        }

        private void DeleteWord(string word, bool DontUndo = false) {
            int group = EditWordList[word].Group;

            if (EditWordList.GroupByNumber(group).WordCount == 1 &&
                group != 0 && group != 1 && group != 9999) {
                RemoveAGroup(group);
            }
            else {
                RemoveAWord(word);
                if (group == 1 && lstWords.Items.Count == 0) {
                    lstWords.Items.Add("<group 1: any word>");
                }
                if (group == 9999 && lstWords.Items.Count == 0) {
                    lstWords.Items.Add("<group 9999: rest of line>");
                }
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
                for (i = 0; i < lstGroups.Items.Count; i++) {
                    // if the new group is less than or equal to current group number
                    if (NewGroupNo <= EditWordList.GroupByIndex(i).GroupNum) {
                        // this is where to insert it
                        break;
                    }
                }
                lstGroups.Items.Insert(i, (NewGroupNo.ToString() + ":").PadRight(6) + EditWordList.GroupByNumber(NewGroupNo).GroupName);
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
            string strFirst = "";

            // groups 0, 1, and 9999 never change groupname so no need to capture current groupname
            if (GroupNo != 0 && GroupNo != 1 && GroupNo != 9999) {
                // any other group will always have at least one word
                strFirst = EditWordList.GroupByNumber(GroupNo).GroupName;
            }
            InsertAWord(NewWord, GroupNo);
            if (EditWordList.GroupByNumber(GroupNo).GroupName != strFirst) {
                UpdateGroupName(GroupNo);
            }
            if (!DontUndo) {
                WordsUndo NextUndo = new() {
                    Action = WordsUndo.ActionType.AddWord,
                    GroupNo = EditWordList[NewWord].Group
                };
                NextUndo.OldGroupNo = -1; // NextUndo.GroupNo;
                NextUndo.Word = NewWord;
                AddUndo(NextUndo);
                UpdateSelection(NewWord);
            }
        }

        private void InsertAWord(string NewWord, int group) {
            WordList.AGIWordComparer comparer = new();

            EditWordList.AddWord(NewWord, group);
            if ((GroupMode && EditGroupNumber == group) || !GroupMode) {
                int i;
                for (i = 0; i < lstWords.Items.Count; i++) {
                    if (comparer.Compare(NewWord, (string)lstWords.Items[i]) < 0) {
                        break;
                    }
                }
                lstWords.Items.Insert(i, NewWord);
            }
        }

        private void RemoveAGroup(int group) {
            // this also removes all words from the group
            if (GroupMode) {
                lstGroups.Items.RemoveAt(EditWordList.GroupIndexFromNumber(group));
            }
            else {
                foreach (string word in EditWordList.GroupByNumber(group)) {
                    lstWords.Items.Remove(word);
                }
            }
            EditWordList.RemoveGroup(group);
        }

        private void RemoveAWord(string word) {
            // update the list boxes
            if (GroupMode) {
                if (EditGroupNumber == EditWordList[word].Group) {
                    lstWords.Items.Remove(word);
                }
            }
            else {
                lstWords.Items.Remove(word);
            }
            // update the list
            EditWordList.RemoveWord(word);
        }

        private void UpdateGroupName(int GroupNo) {
            // updates the group list for the correct name

            // should never happen but...
            if (!EditWordList.GroupExists(GroupNo)) {
                return;
            }

            if (GroupMode) {
                lstGroups.Items[EditWordList.GroupIndexFromNumber(GroupNo)] = (GroupNo.ToString() + ":").PadRight(6) + EditWordList.GroupByNumber(GroupNo).GroupName;
            }
            else {
                if (GroupNo == EditGroupNumber) {
                    lstGroups.Items[0] = (GroupNo.ToString() + ":").PadRight(6) + EditWordList.GroupByNumber(GroupNo).GroupName;
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
                        if (blnRecurse) {
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
                        if (blnRecurse) {
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
                    //search direction is All
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
                    string strFullWord;
                    if (!MatchWord) {
                        // update replacetext to include the full word with the replaced section
                        if (GroupMode) {
                            ReplaceText = EditWordList.GroupByIndex(FoundGrp).Words[FoundWord].Replace(FindText, ReplaceText);
                            strFullWord = EditWordList.GroupByIndex(FoundGrp).Words[FoundWord];
                        }
                        else {
                            ReplaceText = EditWordList[FoundWord].WordText.Replace(FindText, ReplaceText);
                            strFullWord = EditWordList[FoundWord].WordText;
                        }
                    }
                    else {
                        strFullWord = FindText;
                    }
                    // now try to edit the word
                    if (EditWord(strFullWord, ReplaceText, EditWordList.GroupByNumber(FoundGrp).GroupNum)) {
                        // change undo
                        UndoCol.Peek().Action = WordsUndo.ActionType.Replace;
                        UpdateSelection(ReplaceText, true);
                        // always reset search when replacing, because
                        // word index almost always changes
                        FindingForm.ResetSearch();

                        // recurse the find method to get the next occurence
                        blnRecurse = true;
                        FindInWords(FindText, FindDir, MatchWord, false);
                        blnRecurse = false;
                    }
                }
            }
            else {
                // not found - if not recursing, show a msg
                if (!blnRecurse) {
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
                            //only advance if the replaced word is now the current word
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
            if (lstGroups.Focused) {
                tbbRenumber.Enabled = EditGroupNumber > 1 && EditGroupNumber != 9999;
                tbbCut.Enabled = (EditGroupNumber != -1 &&
                    EditGroupNumber != 0 &&
                    EditGroupNumber != 1 &&
                    EditGroupNumber != 9999);
                tbbDelete.Enabled = tbbCut.Enabled;
                tbbCopy.Enabled = (EditGroupNumber != -1);
            }
            if (lstWords.Focused) {
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
            tbbPaste.Enabled = Clipboard.ContainsText(TextDataFormat.Text);
            //tbbAddGroup.Enabled = true;
            tbbAddWord.Enabled = EditGroupNumber != 1 && EditGroupNumber != 9999 || EditWordList.GroupByNumber(EditGroupNumber).WordCount == 0;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // command keys need to be intercepted and rerouted
            switch (keyData) {
            case Keys.Enter:
            case Keys.Tab:
            //case Keys.Delete:
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

        private void FinishGroupEdit() {
            EditingGroup = false;
            txtGroupEdit.Visible = false;
            lstGroups.Select();
        }

        private void FinishWordEdit() {
            EditingWord = false;
            txtWordEdit.Visible = false;
            lstWords.Select();
        }

        private bool AskClose() {
            if (EditWordList.ErrLevel < 0) {
                // if exiting due to error on form load
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
                MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = true;
                Text = sDM + Text;
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
                Text += Common.Base.CompactPath(EditWordListFilename, 75);
            }
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
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
