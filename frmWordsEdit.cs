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
        public bool InGame;
        public bool IsChanged;
        public WordList EditWordList;
        private bool closing = false;
        private string EditWordListFilename;
        private bool blnRefreshLogics = false;
        private bool GroupMode = true;
        private int EditGroupIndex, EditGroupNumber;
        private int EditWordIndex, EditWordGroupIndex;
        private string EditWordText;
        private bool EditingWord = false, EditingGroup = false;
        private Font defaultfont;
        private Font boldfont;
        private Stack<WordsUndo> UndoCol = new();
        private bool FirstFind = false;
        private bool AddNewWord = false;
        private bool AddNewGroup = false;
        public static frmWordsEdit DragSourceForm { get; private set; }

        public frmWordsEdit() {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
            InitializeComponent();

            InitFonts();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers
        private void frmWordsEdit_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible) {
                if (FindingForm.rtfReplace.Visible) {
                    FindingForm.SetForm(FindFormFunction.ReplaceObject, InGame);
                }
                else {
                    FindingForm.SetForm(FindFormFunction.FindObject, InGame);
                }
            }
            spGroupCount.Text = "Group Count: " + EditWordList.GroupCount;
            spWordCount.Text = "Word Count: " + EditWordList.WordCount;
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

        public void mnuRSave_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord) {
                return;
            }
            SaveWords();
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

            ProgressWin = new() {
                Text = "Merging from File"
            };

            WordList MergeList;
            ProgressWin.lblProgress.Text = "Merging...";
            ProgressWin.pgbStatus.Maximum = 0;
            ProgressWin.pgbStatus.Value = 0;
            ProgressWin.Show(MDIMain);
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
            bool blnDontAsk = false;
            AskOption RepeatAnswer = AskOption.Ask;
            DialogResult rtn = DialogResult.No;
            bool[] GroupUsed = [];
            int UnusedCount = 0;

            if (!InGame) {
                return;
            }
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
                            frm.Focus();
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
            this.mnuESep1.Visible = true;
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
                mnuECut.Enabled = (EditGroupNumber != -1 &&
                    EditGroupNumber != 0 &&
                    EditGroupNumber != 1 &&
                    EditGroupNumber != 9999);
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
                mnuECopy.Enabled = (EditWordIndex != 0);
                mnuEDelete.Text = "Delete Word";
                mnuEDelete.Enabled = (EditWordIndex != 0);
            }
            // paste if something on clipboard
            mnuEPaste.Enabled = Clipboard.ContainsText(TextDataFormat.Text);
            if (lstGroups.Focused) {
                mnuEPaste.Text = "Paste As Group";
            }
            else {
                mnuEPaste.Text = "Paste As Word";
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
                //// description field true if only one word
                //if (NextUndo.Description != "0") {
                //    // restore a single word

                //    // remove existing word
                //    EditWordList.RemoveWord(NextUndo.Word);
                //    // restore previous word
                //    EditWordList.AddWord(NextUndo.OldWord, NextUndo.GroupNo);
                //    // ensure group name is correct
                //    UpdateGroupName(NextUndo.GroupNo);
                //    // if the existing word was in a different group
                //    if (NextUndo.OldGroupNo != -1) {
                //        // add word back to its old group
                //        EditWordList.AddWord(NextUndo.Word, NextUndo.OldGroupNo);
                //        // ensure groupname is correct
                //        UpdateGroupName(NextUndo.OldGroupNo);
                //    }
                //    UpdateSelection(NextUndo.OldGroupNo, 0);
                //}
                //else {
                //    // show wait cursor
                //    MDIMain.UseWaitCursor = true;
                //    // restore a bunch of words
                //    string[] strWords = NextUndo.Word.Split("|");
                //    for (int i = 0; i < strWords.Length; i += 4) {
                //        // first element is group where word will be restored
                //        // second element is old group (if the new word was in a different group)
                //        // third element is old word being restored
                //        // fourth element is new word being 'undone'
                //        int group = int.Parse(strWords[i]);
                //        // remove new word
                //        EditWordList.RemoveWord(strWords[i + 3]);
                //        // add old word to this group
                //        EditWordList.AddWord(strWords[i + 2], group);
                //        // update groupname
                //        UpdateGroupName(group);
                //        // if oldgroup is valid
                //        int oldgroup = int.Parse(strWords[i + 1]);
                //        if (oldgroup != -1) {
                //            // restore word to original group
                //            EditWordList.AddWord(strWords[i + 3], oldgroup);
                //            // update the originalgroup name
                //            UpdateGroupName(oldgroup);
                //        }
                //    }
                //    MDIMain.UseWaitCursor = false;
                //}
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
            if (EditingGroup || EditingWord ||
                EditGroupNumber == -1 || EditWordIndex == -1) {
                return;
            }
            mnuECopy_Click(sender, e);
            mnuEDelete_Click(sender, e);
            if (UndoCol.Peek().Action == WordsUndo.ActionType.DelGroup) {
                UndoCol.Peek().Action = WordsUndo.ActionType.CutGroup;
            }
            else if (UndoCol.Peek().Action == WordsUndo.ActionType.DelWord) {
                UndoCol.Peek().Action = WordsUndo.ActionType.CutWord;
            }
        }

        private void mnuECopy_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord ||
                EditGroupNumber == -1 || EditWordIndex == -1) {
                return;
            }
            if (lstGroups.Focused) {
                string copytext = EditGroupNumber.ToString();
                Clipboard.SetText(copytext);
                WordsClipboard.Action = WordsUndo.ActionType.DelGroup;
                WordsClipboard.GroupNo = EditGroupNumber;
                WordsClipboard.Group = new string[EditWordList.GroupByNumber(EditGroupNumber).WordCount];
                if (EditWordList.GroupByNumber(EditGroupNumber).WordCount > 0) {
                    int i;
                    for (i = 0; i < EditWordList.GroupByNumber(EditGroupNumber).WordCount; i++) {
                        copytext += "|" + EditWordList.GroupByNumber(EditGroupNumber).Words[i];
                        WordsClipboard.Group[i] = EditWordList.GroupByNumber(EditGroupNumber).Words[i];
                    }
                }
            }
            else if (lstWords.Focused) {
                Clipboard.SetText('"' + EditWordText + '"');
                WordsClipboard.Action = WordsUndo.ActionType.DelWord;
                WordsClipboard.Word = EditWordText;
            }
        }

        private void mnuEPaste_Click(object sender, EventArgs e) {
            string strMsg = "";

            if (lstGroups.Focused) {
                // only allow pasting if the custom clipboard is set OR if a valid word is on the clipboard
                if (WordsClipboard.Action == WordsUndo.ActionType.DelGroup) {
                    // clipboard contains a single group
                    WordsUndo NextUndo = new();
                    NextUndo.Action = WordsUndo.ActionType.PasteGroup;
                    NextUndo.Group = [];
                    for (int i = 0; i < WordsClipboard.Group.Length; i++) {
                        if (EditWordList.WordExists(WordsClipboard.Group[i])) {
                            // word is in use
                            strMsg += "\r\t" + WordsClipboard.Group[i] + " (in group " + EditWordList[WordsClipboard.Group[i]].Group + ")";
                        }
                        else {
                            // add it to undo object
                            Array.Resize(ref NextUndo.Group, NextUndo.Group.Length + 1);
                            NextUndo.Group[^1] = WordsClipboard.Group[i];
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
                    if (lstGroups.Focused) {
                        // add a new group (without undo)
                        int lngNewGrpNo = NextGrpNum();
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
                    }
                    else {
                        // add to selected group?
                        Debug.Assert(false);
                    }
                    return;
                }
                else {
                    MessageBox.Show(MDIMain,
                        "There are no valid words on the clipboard to paste as a new group.",
                        "Nothing to Paste",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                return;
            }
            if (lstWords.Focused) {
                // check regular clipboard for a word
                string strWord = "";
                if (Clipboard.ContainsText(TextDataFormat.Text)) {
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
                        // this word is acceptable; put it on custom clipboard
                        WordsClipboard.Word = strWord;
                        WordsClipboard.Action = WordsUndo.ActionType.DelWord;
                        Clipboard.SetText('"' + strWord + '"');
                    }
                    else {
                        MessageBox.Show(MDIMain,
                            "The clipboard doesn't contain a valid word.",
                            "Nothing to Paste",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }

                // check custom clipboard
                if (WordsClipboard.Action == WordsUndo.ActionType.DelWord) {
                    // clipboard contains a single word
                    int lngOldGroupNo;
                    if (EditWordList.WordExists(WordsClipboard.Word)) {
                        lngOldGroupNo = EditWordList[WordsClipboard.Word].Group;
                        if (lngOldGroupNo == EditGroupNumber) {
                            MessageBox.Show(MDIMain,
                                "'" + WordsClipboard.Word + "' already exists in this group.",
                                "Unable to Paste",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            return;
                        }
                        // word is in another group- ask if word should be moved
                        if (MessageBox.Show("'" + WordsClipboard.Word + "' already exists (in group " + lngOldGroupNo + "). Do you want to move it to this group?",
                            "Move Word",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.No) {
                            return;
                        }
                        // delete word from other group
                        DeleteWord(WordsClipboard.Word, true);
                    }
                    else {
                        lngOldGroupNo = -1;
                    }
                    // add word to this group
                    AddWord((EditGroupNumber), WordsClipboard.Word, true);

                    // add undo
                    WordsUndo NextUndo = new();
                    NextUndo.Action = WordsUndo.ActionType.PasteWord;
                    NextUndo.GroupNo = EditGroupNumber;
                    //if (lngOldGroupNo == -1) {
                    //    NextUndo.OldGroupNo = NextUndo.GroupNo;
                    //}
                    //else {
                    NextUndo.OldGroupNo = lngOldGroupNo;
                    //}
                    NextUndo.Word = WordsClipboard.Word;
                    AddUndo(NextUndo);

                    //select the pasted word
                    UpdateSelection(WordsClipboard.Word);
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

        private void mnuEDelete_Click(object sender, EventArgs e) {
            if (EditingGroup || EditingWord ||
                EditGroupNumber == -1 || EditWordIndex == -1) {
                return;
            }
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

        }

        private void mnuEFindAgain_Click(object sender, EventArgs e) {

        }

        private void mnuEReplace_Click(object sender, EventArgs e) {

        }

        private void mnuEMode_Click(object sender, EventArgs e) {
            byte[] buttonicon;
            // only if not editing a word
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

        private void mnuEFindLogic_Click(object sender, EventArgs e) {

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
            txtGroupEdit.Focus();
        }

        private void lstGroups_Enter(object sender, EventArgs e) {
            //int top = lstGroups.TopIndex;
            //lstGroups.BorderStyle = BorderStyle.Fixed3D;
            //lstGroups.TopIndex = top;
            label1.Font = boldfont;
            SetToolbarStatus();
        }

        private void lstGroups_Leave(object sender, EventArgs e) {
            if (lstWords.Focused) {
                //int top = lstGroups.TopIndex;
                //lstGroups.BorderStyle = BorderStyle.FixedSingle;
                //lstGroups.TopIndex = top;
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
                }
            }
            if (e.Button == MouseButtons.Right) {
                lstGroups.Focus();
            }
        }

        private void lstGroups_MouseUp(object sender, MouseEventArgs e) {
            if (lstGroups.SelectedIndex != EditGroupIndex) {
                UpdateSelection(EditWordList.GroupByIndex(lstGroups.SelectedIndex).GroupNum, 0, true);
            }
        }

        private void lstGroups_DragEnter(object sender, DragEventArgs e) {
            if (GroupMode && DragWord) {
                if (!this.Focused) {
                    this.Focus();
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
                lstGroups.Focus();
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

        private void lstWords_SelectedIndexChanged(object sender, EventArgs e) {

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
            //int top = lstWords.TopIndex;
            //lstWords.BorderStyle = BorderStyle.Fixed3D;
            //lstWords.TopIndex = top;
            label2.Font = boldfont;
            SetToolbarStatus();
        }

        private void lstWords_Leave(object sender, EventArgs e) {
            if (lstGroups.Focused) {
                //int top = lstWords.TopIndex;
                //lstWords.BorderStyle = BorderStyle.FixedSingle;
                //lstWords.TopIndex = top;
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
                }
            }
            else {
                if (EditWordIndex != selitem) {
                    UpdateSelection((string)lstWords.Items[selitem]);
                }
            }
            if (e.Button == MouseButtons.Right) {
                lstWords.Focus();
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
                Debug.Print("stop drag");
                DragWord = false;
            }
            if (e.Action == DragAction.Cancel) {
                Debug.Print("cancel... ");
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
                lstWords.Focus();
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

        private void txtWordEdit_TextChanged(object sender, EventArgs e) {
            // if not valid, reset to default??????
        }
        #endregion

        #region temp code
        void findstuff() {
            /*
private void FindWordInLogic(ByVal SearchWord As String, ByVal FindSynonyms As Boolean)

  // call findinlogic with current word
  // or word group
  
  // find synonym option currently does nothing
  
  // reset logic search
  FindingForm.ResetSearch
    
  // set global search values
  GFindText = QUOTECHAR & SearchWord & QUOTECHAR
  GFindDir = fdAll
  GMatchWord = true
  GMatchCase = false
  GLogFindLoc = flAll
  GFindSynonym = FindSynonyms
  GFindGrpNum = EditWordList.Group(lstGroups.ListIndex).GroupNum
  SearchType = rtWords
  
  // set it to match desired search parameters
    // set find dialog to find textinlogic mode
    FindingForm.SetForm ffFindWordsLogic, true
    
    // show the form
    FindingForm.Show , frmMDIMain
  
  // ensure this form is the search form
  SearchForm = Me
  
return;
}

private void ReplaceAll(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal ReplaceText As String)

  // replace all occurrences of FindText with ReplaceText
  
  Dim i As Long, j As Long
  Dim lngCount As Long
  Dim lngGroup As Long, lngOldGrp As Long
  Dim NextUndo As WordsUndo
  Dim strFindWord As String, strReplaceWord As String
  
  // if replacing and new text is the same
  if ( StrComp(FindText, ReplaceText, vbTextCompare) = 0 ) {
    // exit
    return;
  }
  
  // blank replace text not allowed for words
  if ( LenB(ReplaceText) = 0 ) {
    MsgBox "Blank replacement text is not allowed.", vbInformation + vbOKOnly, "Replace All"
    return;
  }
  
  // validate replace word (no special characters, etc)
  // what characters are allowed for words????
  // ####
  
  
  // if nothing in wordlist,
  if ( EditWordList.WordCount = 0 ) {
    MsgBox "Word list is empty.", vbOKOnly + vbInformation, "Replace All"
    return;
  }
  
  // show wait cursor
  MDIMain.UseWaitCursor = true;
  
  // create new undo object
  NextUndo = new WordsUndo
  NextUndo.Action = ReplaceAll
  // use description property to indicate replace mode
  NextUndo.Description = CStr(MatchWord)
  
  if ( MatchWord ) {
    // does findtext exist?
    lngGroup = EditWordList(FindText).Group
    
    // if no error, then searchword exists
    if ( Err.Number = 0 ) {
      // i is group where replacement is occurring
      Err.Clear
      // add info to undo object
      NextUndo.GroupNo = lngGroup
      NextUndo.OldWord = FindText
      NextUndo.Word = ReplaceText
      
      // assume replacement word does not exist in a different group
      // validate the assumption by trying to get the groupnumber directly
      lngOldGrp = EditWordList(ReplaceText).Group
      // if no error,
      if ( Err.Number = 0 ) {
        // remove the replacement word
        EditWordList.RemoveWord ReplaceText
      } else {
        // not found; reset old group num
        lngOldGrp = -1
      }
      
      // save oldgroup in undo object
      NextUndo.OldGroupNo = lngOldGrp
      
      // change word in this group by deleting findword
      EditWordList.RemoveWord FindText
      // and adding replaceword
      EditWordList.AddWord ReplaceText, lngGroup
      // ensure group lngGroup has correct name
      UpdateGroupName lngGroup
      // if word was removed from another group
      // (by checking if lngOldGrp is a valid group index)
      if ( lngOldGrp != -1 ) {
        // ensure group lngOldGrp has correct name
        UpdateGroupName lngOldGrp
      }
      
      // update list boxes
      if ( Val(lstGroups.Text) = lngGroup Or (Val(lstGroups.Text) = lngOldGrp And lngOldGrp != -1) ) {
        UpdateWordList true
      }
      // set Count to one
      lngCount = 1
    }
  } else {
    // need to step through all groups and all words
    // remove old words if the replace creates duplicates,
    // and create undo object that holds all this...
  
  
    // step through all groups
    for ( i = 0 To EditWordList.GroupCount - 1) {
      // step through all words
      for ( j = 0 To EditWordList.Group(i).WordCount - 1) {
        // need to manually check for end of group Count
        // because wordcount may change dynamically as words
        // are added and removed based on the changes
        if ( j > EditWordList.Group(i).WordCount - 1 ) {
          break;
        }
        
        // if there is a match
        if ( InStr(1, EditWordList.Group(i).Word[j], FindText, vbTextCompare) != 0 ) {
          strFindWord = EditWordList.Group(i).Word[j]
          strReplaceWord = Replace(EditWordList.Group(i).Word[j], FindText, ReplaceText)
          
          // i is the group INDEX not the group NUMBER;
          // get group number
          lngGroup = EditWordList.Group(i).GroupNum
          
          // assume replacement word does not exist in another group
          // validate assumption by trying to get the groupnumber directly
          lngOldGrp = EditWordList(strReplaceWord).Group
          // if no error,
          if ( Err.Number = 0 ) {
            // remove the replacement word
            EditWordList.RemoveWord strReplaceWord
          } else {
            // not found; reset old group num
            lngOldGrp = -1
          }
          Err.Clear
          
          // change word in this group by deleting findword
          EditWordList.RemoveWord strFindWord
          // and adding replaceword
          EditWordList.AddWord strReplaceWord, lngGroup
          // ensure group i has correct name
          UpdateGroupName lngGroup
          
          // if word came from a different group
          if ( lngOldGrp != -1 ) {
            // update that groupname too
            UpdateGroupName lngOldGrp
          }
          
          // add to undo
          if ( lngCount = 0 ) {
            // add group, oldgroup, word, oldword
            NextUndo.Word = CStr(lngGroup) & "|" & CStr(lngOldGrp) & "|" & strFindWord & "|" & strReplaceWord
          } else {
             NextUndo.Word = NextUndo.Word & "|" & CStr(lngGroup) & "|" & CStr(lngOldGrp) & "|" & strFindWord & "|" & strReplaceWord
          }
          // increment counter
          lngCount = lngCount + 1
          // need to restart search at beginning of word
          // because changes in words will affect the
          // order;
          // set j to -1 so the next j statement
          // resets it to 0
          j = -1
        }
      }
    }
    
    // if something found,
    if ( lngCount != 0 ) {
      // force update
      UpdateWordList true
    }
  }
  
  // if nothing found,
  if ( lngCount = 0 ) {
    MsgBox "Search text not found.", vbInformation, "Replace All"
  } else {
    // add undo
    AddUndo NextUndo
    
    // show how many replacements made
    MsgBox "The specified region has been searched. " & CStr(lngCount) & " replacements were made.", vbInformation, "Replace All"
  }
  
  // restore mousepointer
  Screen.MousePointer = vbDefault
return;
}

public void BeginFind()

  // each form has slightly different search parameters
  // and procedure; so each form will get what it needs
  // from the form, and update the global search parameters
  // as needed
  // 
  // that's why each search form cheks for changes, and
  // sets the global values, instead of doing it once inside
  // the FindingForm code
  
  // when searching for a word in a logic, the first time the
  // user presses the 'find' button, the SearchForm is the
  // Words Editor; so we need to send this search request
  // to the FindInLogics function; after that, if found,
  // the SearchForm will switch to the LogicEditor, and
  // future presses of the Find button will search logics
  // as expected
  
    // if searching in logics
    if ( FindingForm.FormFunction = ffFindWordsLogic ) {
      // begin a logic search
      FindInLogic GFindText, fdAll, true, false, flAll, false, vbNullString
      return;
    }
        
    switch ( FindingForm.FormAction) {
    case faFind:
      FindInWords GFindText, GMatchWord, GFindDir
    case faReplace:
      FindInWords GFindText, GMatchWord, GFindDir, true, GReplaceText
    case faReplaceAll:
      ReplaceAll GFindText, GMatchWord, GReplaceText
    case faCancel:
      // don't do anything
    }
}

public void MenuClickReplace()

 // replace text
 // use menuclickfind in replace mode
  MenuClickFind ffReplaceWord
}

public void MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindWord)

 // set form defaults
    if ( !FindingForm.Visible ) {
      if ( Len(GFindText) > 0 ) {
       // if it has quotes, remove them
        if ( Asc(GFindText) = 34 ) {
          GFindText = Right$(GFindText, Len(GFindText) - 1)
        }
        if ( Right$(GFindText, 1) = QUOTECHAR ) {
          GFindText = Left$(GFindText, Len(GFindText) - 1)
        }
      }
    }
    
   // set find dialog to word mode
    FindingForm.SetForm ffValue, false
    
   // show the form
    FindingForm.Show , frmMDIMain
  
   // always highlight search text
    FindingForm.rtfFindText.Selection.Range.StartPos = 0
    FindingForm.rtfFindText.Selection.Range.EndPos = Len(.rtfFindText.Text)
    FindingForm.rtfFindText.SetFocus
    
   // ensure this form is the search form
    SearchForm = Me
return;
}

private void FindInWords(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal FindDir As FindDirection, Optional ByVal Replacing As Boolean = false, Optional ByVal ReplaceText As String = vbNullString)

  Dim SearchWord As Long, FoundWord As Long
  Dim SearchGrp As Long, FoundGrp As Long
  Dim rtn As VbMsgBoxResult, strFullWord As String
  
  switch ( Mode) {
  case wtByGroup:
   // if editor mode is none (no group/word selected)
    if ( SelMode = smNone ) {
      return;
    }
    
   // if replacing and new text is the same
    if ( Replacing And (StrComp(FindText, ReplaceText, vbTextCompare) = 0) ) {
     // exit
      return;
    }
    
   // blank replace text not allowed for words
    if ( Replacing And LenB(ReplaceText) = 0 ) {
      MsgBox "Blank replacement text is not allowed.", vbInformation + vbOKOnly, "Replace in Word List"
      return;
    }
    
   // if no words in the list
    if ( EditWordList.WordCount = 0 ) {
      MsgBox "There are no words in this list.", vbOKOnly + vbInformation, "Find in Word List"
      return;
    }
    
   // show wait cursor
    MDIMain.UseWaitCursor = true;
    
   // if replacing and searching up,  start at next word
   // if replacing and searching down start at current word
   // if not repl  and searching up   start at current word
   // if not repl  and searching down start at next word
    
   // set searchwd and searchgrp to current word
    SearchGrp = lstGroups.ListIndex
    SearchWord = lstWords.ListIndex
   // if no word selected, start with first word
    if ( SearchWord = -1 ) {
      SearchWord = 0
    }
    
   // adjust to next word per replace/direction selections
    if ( (Replacing And FindDir = fdUp) Or (!Replacing And FindDir != fdUp) ) {
     // if at end;
      if ( SearchWord >= lstWords.ListCount - 1 ) {
       // use first word
        SearchWord = 0
       // of next group
        SearchGrp = SearchGrp + 1
        if ( SearchGrp >= lstGroups.ListCount ) {
          SearchGrp = 0
        }
      } else {
        SearchWord = SearchWord + 1
      }
    } else {
     // if already AT beginning of search, the replace function will mistakenly
     // think the find operation is complete and stop
      if ( Replacing And (SearchWord = StartWord And SearchGrp = StartGrp) ) {
       // reset search
        FindingForm.ResetSearch
      }
    }
    
   // main search loop
    do {
     // if direction is up
      if ( FindDir = fdUp ) {
       // iterate backwards until word found or GrpFound=-1
        FoundWord = SearchWord - 1
        FoundGrp = SearchGrp
       // if at top of this group,
       // get the last word of previous group
        if ( FoundWord < 0 ) {
          FoundGrp = FoundGrp - 1
          if ( FoundGrp != -1 ) {
            FoundWord = EditWordList.Group(FoundGrp).WordCount - 1
          }
        }
        
        while (FoundGrp != -1) {
         // skip groups with no words
          if ( EditWordList.Group(FoundGrp).WordCount != 0 ) {
            if ( MatchWord ) {
              if ( StrComp(EditWordList.Group(FoundGrp).Word(FoundWord), FindText, vbTextCompare) = 0 ) {
               // found
                break; // exit do
              }
            } else {
              if ( InStr(1, EditWordList.Group(FoundGrp).Word(FoundWord), FindText, vbTextCompare) != 0 ) {
               // found
                break; // exit do
              }
            }
          }
         // decrement word
          FoundWord = FoundWord - 1
          if ( FoundWord < 0 ) {
            FoundGrp = FoundGrp - 1
            if ( FoundGrp != -1 ) {
              FoundWord = EditWordList.Group(FoundGrp).WordCount - 1
            }
          }
        }
        
       // reset search to last group/last word+1
        SearchGrp = EditWordList.GroupCount - 1
        SearchWord = EditWordList.Group(SearchGrp).WordCount //  - 1
      } else {
       // iterate forward until word found or foundgrp=groupcount
        FoundWord = SearchWord
        FoundGrp = SearchGrp
        
        do {
         // skip groups with no words
          if ( EditWordList.Group(FoundGrp).WordCount != 0 ) {
            if ( MatchWord ) {
              if ( StrComp(EditWordList.Group(FoundGrp).Word(FoundWord), UnicodeToCP(FindText, SessionCodePage), vbTextCompare) = 0 ) {
               // found
                break; // exit do
              }
            } else {
              if ( InStr(1, EditWordList.Group(FoundGrp).Word(FoundWord), UnicodeToCP(FindText, SessionCodePage), vbTextCompare) != 0 ) {
               // found
                break; // exit do
              }
            }
          }
         // increment word
          FoundWord = FoundWord + 1
          if ( FoundWord >= EditWordList.Group(FoundGrp).WordCount ) {
            FoundWord = 0
            FoundGrp = FoundGrp + 1
          }
          
        } while (FoundGrp != EditWordList.GroupCount);
       // reset search
        SearchGrp = 0
        SearchWord = 0
      }
      
     // if found, group will be valid
      if ( FoundGrp >= 0 And FoundGrp < EditWordList.GroupCount ) {
       // if back at start (grp and word same as start)
        if ( FoundWord = StartWord And FoundGrp = StartGrp ) {
         // rest found position so search will end
          FoundWord = -1
          FoundGrp = -1
        }
        break; // exit do
      }
      
     // if not found, action depends on search mode
      switch ( FindDir) {
      case fdUp:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing,
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdDown:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdAll:
        if ( RestartSearch ) {
          break; // exit do
        }
        
      }
      
     // reset search so when we get back to start, search will end
      RestartSearch = true
    
   // loop is exited by finding the searchtext or reaching end of search area
    } while (true);
    
   // if search string found
    if ( FoundGrp >= 0 And FoundGrp < EditWordList.GroupCount ) {
     // if this is first occurrence
      if ( !FirstFind ) {
       // save this position
        FirstFind = true
        StartWord = FoundWord
        StartGrp = FoundGrp
      }
      
     // highlight Word
      lstGroups.ListIndex = FoundGrp
      lstWords.ListIndex = FoundWord
      
     // if replacing
      if ( Replacing ) {
       // if not replacing entire word
        if ( !MatchWord ) {
         // calculate new findtext and replacetext
          ReplaceText = Replace(EditWordList.Group(FoundGrp).Word(FoundWord), FindText, ReplaceText)
          strFullWord = EditWordList.Group(FoundGrp).Word(FoundWord)
        } else {
          strFullWord = FindText
        }
        
       // now try to edit the word
        if ( EditWord(strFullWord, ReplaceText) ) {
         // change undo
          UndoCol(UndoCol.Count).Action = Replace
          frmMDIMain.mnuEUndo.Text = "&Undo Replace" & vbTab & "Ctrl+Z"
         // select the word
          UpdateWordList true
          switch ( Mode) {
          case wtByGroup:
            lstWords.Text = ReplaceText
          }
         // always reset search when replacing, because
         // word index almost always changes
          FindingForm.ResetSearch
          
         // recurse the find method to get the next occurence
          blnRecurse = true
          FindInWords FindText, MatchWord, FindDir, false
          blnRecurse = false
        }
      }
    
   // if search string NOT found
    } else {
     // if not recursing, show a msg
      if ( !blnRecurse ) {
       // if something was previously found
        if ( FirstFind ) {
         // search complete; no new instances found
          MsgBox "The specified region has been searched.", vbInformation, "Find in Word List"
        } else {
         // show not found msg
          MsgBox "Search text not found.", vbInformation, "Find in Word List"
        }
      }
      
     // reset search flags
      FindingForm.ResetSearch
    }
    
   // need to always make sure right form has focus; if finding a word
   // causes the group list to change, VB puts the wordeditor form in focus
   //  but we want focus to match the starting form
    if ( SearchStartDlg ) {
      FindingForm.SetFocus
    } else {
      Me.SetFocus
    }
    
   // reset cursor
    Screen.MousePointer = vbDefault
    
  case wtByWord:
   // edit mode is n/a, and never replacing
    
   // if no words in the list
    if ( EditWordList.WordCount = 0 ) {
      MsgBox "There are no words in this list.", vbOKOnly + vbInformation, "Find in Word List"
      return;
    }
    
   // show wait cursor
    MDIMain.UseWaitCursor = true;
    
   // if searching up   start at current word
   // if searching down start at next word
    
   // set searchwd to current word
    SearchWord = lstWords.ListIndex
   // if no word selected, start with first word
    if ( SearchWord = -1 ) {
      SearchWord = 0
    }
    
   // adjust to next word per direction selection
    if ( FindDir != fdUp ) {
     // if at end;
      if ( SearchWord >= lstWords.ListCount - 1 ) {
        if ( FindDir = fdDown ) {
         // done
          SearchWord = EditWordList.WordCount
        } else {
         // use first word
          SearchWord = 0
        }
      } else {
        SearchWord = SearchWord + 1
      }
    }
    
   // main search loop
    do {
     // if direction is up
      if ( FindDir = fdUp ) {
       // iterate backwards until word found or back to start
        FoundWord = SearchWord - 1
        
        while  ( FoundWord >= 0) {
         // search all words
          if ( MatchWord ) {
            if ( StrComp(EditWordList(FoundWord).WordText, FindText, vbTextCompare) = 0 ) {
             // found
              break; // exit do
            }
          } else {
            if ( InStr(1, EditWordList(FoundWord).WordText, FindText, vbTextCompare) != 0 ) {
             // found
              break; // exit do
            }
          }
         // decrement word
          FoundWord = FoundWord - 1
          if ( FoundWord < 0 ) {
            break; // exit do
          }
        }
        
       // reset search
        SearchWord = EditWordList.WordCount
      } else {
       // iterate forward until word found or foundword=wordcount
        FoundWord = SearchWord
        
        while (FoundWord != EditWordList.WordCount) {
          if ( MatchWord ) {
            if ( StrComp(EditWordList(FoundWord).WordText, UnicodeToCP(FindText, SessionCodePage), vbTextCompare) = 0 ) {
             // found
              break; // exit do
            }
          } else {
            if ( InStr(1, EditWordList(FoundWord).WordText, UnicodeToCP(FindText, SessionCodePage), vbTextCompare) != 0 ) {
             // found
              break; // exit do
            }
          }
         // increment word
          FoundWord = FoundWord + 1
        }
       // reset search
        SearchWord = 0
      }
      
     // if found, word will be valid
      if ( FoundWord >= 0 And FoundWord < EditWordList.WordCount ) {
       // if back at start (word same as start)
        if ( FoundWord = StartWord ) {
         // rest found position to force search to end
          FoundWord = -1
        }
        break; // exit do
      }
      
     // if not found, action depends on search mode
      switch ( FindDir) {
      case fdUp:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing,
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdDown:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdAll:
        if ( RestartSearch ) {
          break; // exit do
        }
        
      }
      
     // reset search so when we get back to start, search will end
      RestartSearch = true
    
   // loop is exited by finding the searchtext or reaching end of search area
    } while (true);
    
   // if search string found
    if ( FoundWord >= 0 And FoundWord < EditWordList.WordCount ) {
     // if this is first occurrence
      if ( !FirstFind ) {
       // save this position
        FirstFind = true
        StartWord = FoundWord
      }
      
     // highlight Word
      lstWords.ListIndex = FoundWord
      
   // if search string NOT found
    } else {
     // if not recursing, show a msg
      if ( !blnRecurse ) {
       // if something was previously found
        if ( FirstFind ) {
         // search complete; no new instances found
          MsgBox "The specified region has been searched.", vbInformation, "Find in Word List"
        } else {
         // show not found msg
          MsgBox "Search text not found.", vbInformation, "Find in Word List"
        }
      }
      
     // reset search flags
      FindingForm.ResetSearch
    }
    
   // need to always make sure right form has focus; if finding a word
   // causes the group list to change, VB puts the wordeditor form in focus
   //  but we want focus to match the starting form
    if ( SearchStartDlg ) {
      FindingForm.SetFocus
    } else {
      Me.SetFocus
    }
    
   // reset cursor
    Screen.MousePointer = vbDefault
  }
return;
}

public void MenuClickFindAgain()

 // if nothing in find form textbox
  if ( LenB(GFindText) != 0 ) {
    FindInWords GFindText, GMatchWord, GFindDir
  } else {
   // show find form
    MenuClickFind
  }
return;
}

public void findinlogic()

 // if a word is selected
  if ( lstWords.ListIndex != -1 ) {
   // use this word; assume not looking for synonyms
    FindWordInLogic CPToUnicode(EditWordText, SessionCodePage), false
 // if a group is selected
  } else if ( lstGroups.ListIndex != -1 ) {
   // use groupname; assume looking for synonyms
    FindWordInLogic CPToUnicode(EditWordList.Group(lstGroups.ListIndex).GroupName, SessionCodePage), true
  }
}

            */

        }
        void wordsfrmcode() {
            /*

  
  private DraggingWord As Boolean
  
  private blnRecurse As Boolean
  // YES refresh all logics:
  //  x deleting words/groups
  //  x renumber group
  //  x move word
  //  x edit word
  // NO don't refresh:
  //   adding new words
  //   adding new groups
  //   updating description
  private blnRefreshLogics As Boolean
  
public void MenuClickHelp()
  
  // help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Words_Editor.htm"
return;

}

// private void AutoUpdate()
//   // build array of changed words
//   // then step through all logics; find
//   // all commands with word arguments;
//   // compare arguments against changed word list and
//   // make changes in sourcecode as necessary, then save the source
// 
//   Dim tmpLogic As AGILogic, i As Long, j As Long
//   Dim strOld() As String, strNew() As String
//   Dim blnUnloadRes As Boolean
//   Dim lngPos1 As Long, lngPos2 As Long
// 
//   // hide find form
//   FindingForm.Visible = false
// 
//   // show wait cursor
//   MDIMain.UseWaitCursor = true;
//   // show progress form
//   frmProgress.Text = "Updating Words in Logics"
//   frmProgress.lblProgress.Text = "Searching..."
//   frmProgress.pgbStatus.Max = Logics.Count
//   frmProgress.pgbStatus.Value = 0
//   frmProgress.Show vbModeless, frmMDIMain
//   frmProgress.Refresh
// 
//   for ( i = 0 To VocabularyWords.GroupCount - 1) {
//     // get group number
//     lngGrp = VocabularyWords.Group(i).GroupNum
// 
//     // check if this group exists in new list
//     if ( EditWordList.GroupN(lngGrp)  == null ) {
//       // group is deleted
//       blnDeleted = true
//       Err.Clear
//     // OR if group has no name
//     } else if ( LenB(EditWordList.GroupN(lngGrp).GroupName) = 0 ) {
//       blnDeleted = true
//     }
// 
//     // if an error,
//     if ( Err.Number != 0 ) {
// 
//     }
// 
//     // if deleted
//     if ( blnDeleted ) {
//       // add to update list
//       ReDim Preserve strOld[j]
//       ReDim Preserve strNew[j]
//       strOld[j] = QUOTECHAR & VocabularyWords.Group(i).GroupName & QUOTECHAR
//       strNew[j] = CStr(lngGroup)
//       j = j + 1
//     // if group name as changed
//     } else if ( EditWordList.GroupN(lngGrp).GroupName != VocabularyWords.GroupN(lngGrp).GroupName ) {
//       // add to update list
//       ReDim Preserve strOld[j]
//       ReDim Preserve strNew[j]
//       // change to new word group name
//       strOld[j] = QUOTECHAR & VocabularyWords.GroupN(lngGrp).GroupName & QUOTECHAR
//       strNew[j] = QUOTECHAR & EditWordList.GroupN(lngGrp).GroupName & QUOTECHAR
//       j = j + 1
//     }
// 
//     // reset deleted flag
//     blnDeleted = false
//   }
// 
//   // step through all logics
//   foreach (tmpLogic In Logics) {
//     // open if necessary
//     blnUnloadRes = !tmpLogic.Loaded
//     if ( blnUnloadRes ) {
//       tmpLogic.Load
//     }
// 
//     // step through code, looking for 'said' and 'word.to.string' commands
//     lngPos2 = FindNextToken(tmpLogic.SourceText, lngPos1, "said", true, true)
// 
//     while (lngPos != 0) {
//       // said' syntax is : said(word1, word2, word3,...)
//       // words can be numeric word values OR string values
//     }
// 
//     // unload if necessary
//     if ( blnUnloadRes ) {
//       tmpLogic.Unload
//     }
//  }
//   // close the progress form
//   Unload frmProgress
// 
//   // re-enable form
//   frmMDIMain.Enabled = true
//   Screen.MousePointer = vbDefault
//   frmMDIMain.SetFocus
// }
// 
            */
        }

        #endregion

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
            //rtfWord.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            //txtGrpNum.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
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
            };
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
            txtWordEdit.Focus();
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

        private string ValidateWord(string CheckWord) {
            string retval = CheckWord.SingleSpace().Trim().LowerAGI();
            if (retval.Length == 0) {
                // ok; it will be deleted
                return "";
            }
            // need to check for invalid characters (in case something 
            // was pasted)
            if ("!\"'(),-.:;?[]`{}".Any(retval.Contains)) {
                MessageBox.Show(MDIMain,
                    "This word contains one or more invalid characters.",
                    "Invalid Word",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, 0, 0,
                    WinAGIHelp, "htm\\agi\\words.htm#charlimits");
                return "!";
            }
            if ("#$%&*+/0123456789<=>@\\^_|~".Contains(retval[0])) {
                // not allowed unless supporting power pack
                if (EditGame == null || !EditGame.PowerPack) {
                    MessageBox.Show(MDIMain,
                        "This word begins with invalid characters.",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp, "htm\\agi\\words.htm#charlimits");
                    return "!";
                }
            }
            // extended characters
            if (retval.Any(c => c > 127)) {
                // not allowed unless supporting power pack
                if (EditGame == null || !EditGame.PowerPack) {
                    MessageBox.Show(MDIMain,
                        "This word contains one or more extended characters (> 128).",
                        "Invalid Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp, "htm\\agi\\words.htm#charlimits");
                    return "!";
                }
            }
            bool exists = EditWordList.WordExists(retval);
            if (exists) {
                if (EditWordList[retval].Group == EditGroupNumber) {
                    MessageBox.Show(MDIMain,
                        "The word '" + retval + "' already exists in this group.",
                        "Duplicate Word",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return "!";
                }
                else {
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
            else {
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
            lstGroups.Focus();
        }

        private void FinishWordEdit() {
            EditingWord = false;
            txtWordEdit.Visible = false;
            lstWords.Focus();
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
    }

    public class WordsUndo {
        public ActionType Action;
        public string[] Group;
        public int GroupNo;
        public int OldGroupNo;
        public string Word;
        public string OldWord;
        public string Description;


        public enum ActionType {
            AddGroup,   // store group number that was added
            DelGroup,   // store group number AND group object that was deleted
            Renumber,   // store old group number AND new group number
            AddWord,    // store group number AND new word that was added
            DelWord,    // store group number AND old word that was deleted
            MoveWord,   // store old group number, new group number and word that was moved
            ChangeWord, // store old word and new word
            CutWord,    // same as delete
            CutGroup,   // same as delete
            PasteWord,  // store group number and old word that was pasted over
            PasteGroup, // store group number and list of old words that were pasted over
            Replace,    // same as change word
            ReplaceAll, // store list of all words changed
            Clear,      // store all words and their groups
        }

        public WordsUndo() {
            Group = Array.Empty<string>();
        }
    }
}
