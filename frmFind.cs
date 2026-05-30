using System;
using System.Diagnostics;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmFind : Form {
        #region Members
        // determines if form is find or replace
        public FindFormFunction FormFunction;
        // the form action is used by the editors
        // to take the correct action when starting a search
        public FindFormAction FormAction;
        private bool SettingForm = false;
        #endregion

        public frmFind() {
            InitializeComponent();
            cmbDirection.SelectedIndex = 0;
            MDIMain.AddOwnedForm(this);
            this.Owner = MDIMain;
        }

        #region Event Handlers
        private void frmFind_FormClosing(object sender, FormClosingEventArgs e) {
            // never close the form, just hide it
            e.Cancel = true;
            Hide();
        }

        private void frmFind_HelpRequested(object sender, HelpEventArgs hlpevent) {
            ShowHelp();
            hlpevent.Handled = true;
        }

        private void chkMatchWord_CheckedChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            // always update form value
            if (GMatchWord != chkMatchWord.Checked) {
                ResetSearch();
            }
            GMatchWord = chkMatchWord.Checked;
            // if editing a word or object logic search, reset form
            switch (FormFunction) {
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceWordsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                SetForm(FindFormFunction.FindLogic, true);
                break;
            }
        }

        private void chkSynonyms_CheckedChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            if (GFindSynonym != chkSynonyms.Checked) {
                ResetSearch();
            }
            // save synonym search value
            GFindSynonym = chkSynonyms.Checked;
        }

        private void chkMatchCase_CheckedChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            // always update form value
            if (GMatchCase != chkMatchCase.Checked) {
                ResetSearch();
            }
            // save form matchcase value
            GMatchCase = chkMatchCase.Checked;
            // always reset synonym search
            SettingForm = true;
            chkSynonyms.Checked = false;
            SettingForm = false;
            GFindSynonym = false;
            // if editing a word or object logic search, reset form
            switch (FormFunction) {
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceWordsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                SetForm(FindFormFunction.FindLogic, true);
                break;
            }
        }

        private void cmbDirection_SelectedIndexChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            // always update form value
            if ((int)GFindDir != cmbDirection.SelectedIndex) {
                ResetSearch();
                GFindDir = (FindDirection)cmbDirection.SelectedIndex;
            }
            // if editing a word or object logic search, reset form
            switch (FormFunction) {
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceWordsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                SetForm(FindFormFunction.FindLogic, true);
                break;
            }
        }

        private void optCurrentLogic_CheckedChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            if (GLogFindLoc != FindLocation.Current) {
                ResetSearch();
            }
            GLogFindLoc = FindLocation.Current;
            // if editing a word or object logic search, reset form
            switch (FormFunction) {
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceWordsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                SetForm(FindFormFunction.FindLogic, true);
                break;
            }
        }

        private void optAllOpenLogics_CheckedChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            if (GLogFindLoc != FindLocation.Open) {
                ResetSearch();
            }
            GLogFindLoc = FindLocation.Open;
            // if editing a word or object logic search, reset form
            switch (FormFunction) {
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceWordsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                SetForm(FindFormFunction.FindLogic, true);
                break;
            }
        }

        private void optAllGameLogics_CheckedChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            if (GLogFindLoc != FindLocation.All) {
                ResetSearch();
            }
            GLogFindLoc = FindLocation.All;
            // if editing a word or object logic search, reset form
            switch (FormFunction) {
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceWordsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                SetForm(FindFormFunction.FindLogic, true);
                break;
            }
        }

        private void cmdFind_Click(object sender, EventArgs e) {
            BeginSearch(FindFormAction.Find, SearchType);
        }

        private void cmdReplace_Click(object sender, EventArgs e) {
            if (MDIMain.ActiveMdiChild is null) {
                Debug.Assert(false);
                return;
            }
            switch (FormFunction) {
            case FindFormFunction.FindLogic:
                SetForm(FindFormFunction.ReplaceLogic, optAllGameLogics.Enabled);
                break;
            case FindFormFunction.FindObject:
                SetForm(FindFormFunction.ReplaceObject, false);
                break;
            case FindFormFunction.FindWord:
                SetForm(FindFormFunction.ReplaceWord, false);
                break;
            case FindFormFunction.FindText:
                SetForm(FindFormFunction.ReplaceText, false);
                break;
            case FindFormFunction.FindObjsLogic:
                SetForm(FindFormFunction.ReplaceObjsLogic, false);
                break;
            case FindFormFunction.FindWordsLogic:
                SetForm(FindFormFunction.ReplaceWordsLogic, false);
                break;
            default:
                if (optCurrentLogic.Checked) {
                    GLogFindLoc = FindLocation.Current;
                }
                else if (optAllOpenLogics.Checked) {
                    GLogFindLoc = FindLocation.Open;
                }
                else if (optAllGameLogics.Checked) {
                    GLogFindLoc = FindLocation.All;
                }
                BeginSearch(FindFormAction.Replace, SearchType);
                return;
            }
            rtfReplace.Select();
        }

        private void cmdReplaceAll_Click(object sender, EventArgs e) {
            if (MDIMain.ActiveMdiChild is null) {
                Debug.Assert(false);
                return;
            }
            BeginSearch(FindFormAction.ReplaceAll, SearchType);
        }

        private void cmdCancel_Click(object sender, EventArgs e) {
            FormAction = FindFormAction.Cancel;
            Visible = false;
        }

        private void cmbFind_DropDownClosed(object sender, EventArgs e) {
            if (cmbFind.SelectedIndex == -1) {
                return;
            }
            // copy selected text to find box
            txtFind.Text = (string)cmbFind.SelectedItem;
        }

        private void cmbFind_DropDown(object sender, EventArgs e) {
            // deselect text nothing is default selected when opening dropdown
            cmbFind.SelectedIndex = -1;
        }

        private void rtfReplace_TextChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }
            if (GReplaceText != rtfReplace.Text) {
                ResetSearch();
            }
            GReplaceText = rtfReplace.Text;
        }

        private void rtfReplace_Enter(object sender, EventArgs e) {
            rtfReplace.SelectAll();
        }

        private void txtFind_Enter(object sender, EventArgs e) {
            txtFind.SelectAll();
        }

        private void txtFind_TextChanged(object sender, EventArgs e) {
            if (SettingForm) {
                return;
            }

            // if more than one line, expand
            // if only one line, shrink
            if (txtFind.Lines.Length > 1) {
                if (txtFind.Height != 40) {
                    txtFind.Height = 40;
                    // a bit taller if replacing
                    Height = 206 + (Text[0] == 'R' ? 33 : 0);
                }
            }
            else {
                if (txtFind.Height != 20) {
                    txtFind.Height = 20;
                    // a bit taller if replacing
                    Height = 182 + (Text[0] == 'R' ? 33 : 0);
                }
            }

            cmdFind.Enabled = txtFind.Text.Length > 0;
            cmdReplace.Enabled = txtFind.Enabled || FormFunction == FindFormFunction.FindLogic;
            cmdReplaceAll.Enabled = txtFind.Enabled;
            // always update form value
            if (GFindText != txtFind.Text) {
                ResetSearch();
            }
            GFindText = txtFind.Text;
            // if editing a word or object search, reset form
            if (FormFunction == FindFormFunction.FindWordsLogic ||
                FormFunction == FindFormFunction.FindObjsLogic) {
                SetForm(FindFormFunction.FindLogic, true);
            }
        }

        private void txtFind_KeyDown(object sender, KeyEventArgs e) {
            // treat ENTER as clicking the Find button
            if (e.KeyCode == Keys.Enter && cmdFind.Enabled) {
                e.Handled = true;
                cmdFind.PerformClick();
            }
            // treat ESC as clicking the Cancel button
            else if (e.KeyCode == Keys.Escape) {
                e.Handled = true;
                cmdCancel.PerformClick();
            }
            // treat CTRL+ENTER as adding a new line to the find box
            else if (e.KeyCode == Keys.Enter && e.Control) {
                int selStart = txtFind.SelectionStart;
                txtFind.Text = txtFind.Text.Insert(selStart, Environment.NewLine);
                txtFind.SelectionStart = selStart + Environment.NewLine.Length;
                e.Handled = true;
            }
        }
        #endregion

        #region Methods
        internal void SetForm(FindFormFunction newfunction, bool ingame) {
            SettingForm = true;

            FormFunction = newfunction;
            // reset controls to defaults
            optAllGameLogics.Enabled = true;
            chkSynonyms.Visible = false;
            chkMatchCase.Enabled = true;
            chkMatchWord.Enabled = true;

            chkMatchWord.Checked = GMatchWord;
            cmbDirection.SelectedIndex = (int)GFindDir;
            rtfReplace.Text = GReplaceText;
            chkMatchCase.Checked = GMatchCase;
            txtFind.Text = GFindText;

            switch (FormFunction) {
            case FindFormFunction.FindLogic:
            case FindFormFunction.FindText:
                SearchType = AGIResType.None;
                if (FormFunction == FindFormFunction.FindLogic) {
                    Text = "Find in Logics";
                    optCurrentLogic.Text = "Current Logic";
                }
                else {
                    Text = "Find in Text Files";
                    optCurrentLogic.Text = "Current File";
                }
                fraLogic.Visible = true;
                switch (GLogFindLoc) {
                case FindLocation.Current:
                    optCurrentLogic.Checked = true;
                    break;
                case FindLocation.Open:
                    optAllOpenLogics.Checked = true;
                    break;
                case FindLocation.All:
                    optAllGameLogics.Checked = true;
                    break;
                }
                // 'AllGameLogics' only allowed if searching  InGame logics
                // (InGame will never be True for text file searches)
                if (EditGame is not null && ingame) {
                    optAllGameLogics.Enabled = true;
                }
                else {
                    if (optAllGameLogics.Checked) {
                        // since it's not a valid option if 'AllGameLogics'
                        // is selected, change it to 'AllOpenLogics'
                        // change to all open logics
                        GLogFindLoc = FindLocation.Open;
                        optAllOpenLogics.Checked = true;
                    }
                    optAllGameLogics.Enabled = false;
                }
                chkMatchWord.Enabled = true;
                break;
            case FindFormFunction.ReplaceLogic:
            case FindFormFunction.ReplaceText:
                SearchType = AGIResType.None;
                if (FormFunction == FindFormFunction.ReplaceLogic) {
                    Text = "Replace in Logics";
                    optCurrentLogic.Text = "Current Logic";
                }
                else {
                    Text = "Replace in Text Files";
                    optCurrentLogic.Text = "Current File";
                }
                fraLogic.Visible = true;
                switch (GLogFindLoc) {
                case FindLocation.Current:
                    optCurrentLogic.Checked = true;
                    break;
                case FindLocation.Open:
                    optAllOpenLogics.Checked = true;
                    break;
                case FindLocation.All:
                    optAllGameLogics.Checked = true;
                    break;
                }
                // 'AllGameLogics' only allowed if searching  InGame logics
                // (InGame will never be True for text file searches)
                if (EditGame is not null && ingame) {
                    optAllGameLogics.Enabled = true;
                }
                else {
                    if (optAllGameLogics.Checked) {
                        // since it's not a valid option if 'AllGameLogics'
                        // is selected, change it to 'AllOpenLogics'
                        // change to all open logics
                        GLogFindLoc = FindLocation.Open;
                        optAllOpenLogics.Checked = true;
                    }
                    optAllGameLogics.Enabled = false;
                }
                chkMatchWord.Enabled = true;
                break;
            case FindFormFunction.FindObject:
                SearchType = AGIResType.None;
                Text = "Find in OBJECT File";
                fraLogic.Visible = false;
                chkMatchWord.Enabled = true;
                break;
            case FindFormFunction.ReplaceObject:
                SearchType = AGIResType.None;
                Text = "Replace in OBJECT File";
                fraLogic.Visible = false;
                chkMatchWord.Enabled = true;
                break;
            case FindFormFunction.FindWord:
                SearchType = AGIResType.None;
                Text = "Find in WORDS.TOK File";
                fraLogic.Visible = false;
                chkMatchWord.Enabled = true;
                chkMatchCase.Enabled = false;
                break;
            case FindFormFunction.ReplaceWord:
                SearchType = AGIResType.None;
                Text = "Replace in WORDS.TOK File";
                fraLogic.Visible = false;
                chkMatchWord.Enabled = true;
                chkMatchCase.Enabled = false;
                break;
            case FindFormFunction.FindObjsLogic:
                SearchType = AGIResType.Objects;
                Text = "Find Inventory Object in Logics";
                fraLogic.Visible = true;
                optAllGameLogics.Checked = true;
                GLogFindLoc = FindLocation.All;
                chkMatchWord.Enabled = true;
                break;
            case FindFormFunction.FindWordsLogic:
                SearchType = AGIResType.Words;
                Text = "Find Word in Logics";
                fraLogic.Visible = true;
                optAllGameLogics.Checked = true;
                GLogFindLoc = FindLocation.All;
                chkMatchWord.Enabled = true;
                chkSynonyms.Visible = true;
                chkSynonyms.Checked = GFindSynonym;
                break;
            case FindFormFunction.ReplaceObjsLogic:
                SearchType = AGIResType.Objects;
                Text = "Replace Inventory Object in Logics";
                fraLogic.Visible = true;
                optAllGameLogics.Checked = true;
                GLogFindLoc = FindLocation.All;
                chkMatchWord.Enabled = true;
                break;
            case FindFormFunction.ReplaceWordsLogic:
                SearchType = AGIResType.Words;
                Text = "Replace Word in Logics";
                fraLogic.Visible = true;
                optAllGameLogics.Checked = true;
                GLogFindLoc = FindLocation.All;
                chkMatchWord.Enabled = true;
                chkSynonyms.Visible = true;
                chkSynonyms.Checked = GFindSynonym;
                break;
            }

            // adjust height based on selection
            switch (FormFunction) {
            case FindFormFunction.FindLogic:
            case FindFormFunction.FindObject:
            case FindFormFunction.FindWord:
            case FindFormFunction.FindText:
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
                rtfReplace.Visible = false;
                lblReplace.Visible = false;
                cmdReplaceAll.Visible = false;
                Height = 182 + (txtFind.Height == 40 ? 20 : 0);
                break;
            default:
                rtfReplace.Visible = true;
                lblReplace.Visible = true;
                cmdReplaceAll.Visible = true;
                Height = 215 + (txtFind.Height == 40 ? 20 : 0);
                break;
            }
            SettingForm = false;
        }

        internal void ResetSearch() {
            // reset search parameters

            // parameters for all searches
            FoundOnce = false;
            RestartSearch = false;
            // parameters for logic/text editors
            SearchStartLog = -1;
            SearchStartPos = -1;
            ClosedLogics = false;
            // object editor search parameters
            ObjStartPos = -1;
            // word editor search parameters
            StartWord = -1;
            StartGrp = -1;
        }

        private void BeginSearch(FindFormAction action, AGIResType type) {
            MDIMain.UseWaitCursor = true;
            FormAction = action;
            SearchType = type;
            SearchStartDlg = true;
            UpdateSearchList();
            switch (FormFunction) {
            case FindFormFunction.FindLogic:
            case FindFormFunction.ReplaceLogic:
            case FindFormFunction.FindText:
            case FindFormFunction.ReplaceText:
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceWordsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                Form startsearchform;
                // confirm starting from a logic
                if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit") {
                    startsearchform = MDIMain.ActiveMdiChild;
                }
                else {
                    startsearchform = MDIMain;
                }
                switch (FindingForm.FormAction) {
                case FindFormAction.Find:
                    FindInLogic(startsearchform, GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc);
                    break;
                case FindFormAction.Replace:
                    FindInLogic(startsearchform, GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc, true, GReplaceText);
                    break;
                case FindFormAction.ReplaceAll:
                    ReplaceAll(startsearchform, GFindText, GReplaceText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc, type);
                    break;
                }
                break;
            case FindFormFunction.FindWord:
            case FindFormFunction.ReplaceWord:
                if (MDIMain.ActiveMdiChild is frmWordsEdit wordeditform) {
                    switch (FindingForm.FormAction) {
                    case FindFormAction.Find:
                        wordeditform.FindInWords(GFindText, GFindDir, GMatchWord);
                        break;
                    case FindFormAction.Replace:
                        wordeditform.FindInWords(GFindText, GFindDir, GMatchWord, true, GReplaceText);
                        break;
                    case FindFormAction.ReplaceAll:
                        wordeditform.ReplaceAll(GFindText, GReplaceText, GMatchWord);
                        break;
                    }
                }
                break;
            case FindFormFunction.FindObject:
            case FindFormFunction.ReplaceObject:
                if (MDIMain.ActiveMdiChild is frmObjectEdit objeditform) {
                    switch (FindingForm.FormAction) {
                    case FindFormAction.Find:
                        objeditform.FindInObjects(GFindText, GFindDir, GMatchWord, GMatchCase);
                        break;
                    case FindFormAction.Replace:
                        objeditform.FindInObjects(GFindText, GFindDir, GMatchWord, GMatchCase, true, GReplaceText);
                        break;
                    case FindFormAction.ReplaceAll:
                        objeditform.ReplaceAll(GFindText, GReplaceText, GMatchWord, GMatchCase);
                        break;
                    }
                }
                break;
            case FindFormFunction.FindNone:
                break;
            }
            // ALWAYS reset the start-in-dialog flag
            SearchStartDlg = false;
            MDIMain.UseWaitCursor = false;
        }

        internal void UpdateSearchList() {
            // if multiline, the newline characters won't be visible in the dropdown,
            // so the displayed text might look a bit strange; maybe in the future
            // we could replace newlines with something like \n in the dropdown,
            // but for now just add the text as is
            if (!cmbFind.Items.Contains(txtFind.Text)) {
                cmbFind.Items.Insert(0, txtFind.Text);
            }
            if (cmbFind.Items.Count == 12) {
                cmbFind.Items.RemoveAt(11);
            }
        }

        public void SetFindText(string text) {
            if (txtFind.Text != text) {
                SettingForm = true;
                txtFind.Text = text;
                SettingForm = false;
            }
        }

        internal void ShowHelp() {
            string topic = "";
            switch (FormFunction) {
            case FindFormFunction.FindLogic:
            case FindFormFunction.ReplaceLogic:
            case FindFormFunction.FindText:
            case FindFormFunction.ReplaceText:
                topic = "htm\\winagi\\editor_logic.htm#find";
                break;
            case FindFormFunction.FindObject:
            case FindFormFunction.ReplaceObject:
                topic = "htm\\winagi\\editor_object.htm#find";
                break;
            case FindFormFunction.FindObjsLogic:
            case FindFormFunction.ReplaceObjsLogic:
                topic = "htm\\winagi\\editor_object.htm#findinlogics";
                break;
            case FindFormFunction.FindWord:
            case FindFormFunction.ReplaceWord:
                topic = "htm\\winagi\\editor_words.htm#find";
                break;
            case FindFormFunction.FindWordsLogic:
            case FindFormFunction.ReplaceWordsLogic:
                topic = "htm\\winagi\\editor_words.htm#findinlogics";
                break;
            case FindFormFunction.FindNone:
                topic = "htm\\winagi\\editor_logic.htm#find";
                break;
            }
            Help.ShowHelp(HelpParent, WinAGIHelp, topic);
        }
        #endregion
    }
}
