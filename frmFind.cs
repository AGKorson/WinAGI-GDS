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
using WinAGI.Common;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmFind : Form {
        // determines if form is find or replace
        public FindFormFunction FormFunction;
        // the form action is used by the editors
        // to take the correct action when starting a search
        public FindFormAction FormAction;

        public frmFind() {
            InitializeComponent();
            cmbDirection.SelectedIndex = 0;
            MDIMain.AddOwnedForm(this);
            this.Owner = MDIMain;
        }
        #region Event Handlers
        private void chkMatchWord_CheckedChanged(object sender, EventArgs e) {
            // always update form value
            if (GMatchWord != chkMatchWord.Checked) {
                ResetSearch();
            }
            GMatchWord = chkMatchWord.Checked;
        }

        private void chkMatchCase_CheckedChanged(object sender, EventArgs e) {
            // if searching for words in logics, chkMatchCase is used
            // to look for synonyms
            if (FormFunction == FindFormFunction.FindWordsLogic) {
                // never match case when searching for words
                if (GFindSynonym != chkMatchCase.Checked) {
                    ResetSearch();
                }
                // save synonym search value
                GFindSynonym = chkMatchCase.Checked;
            }
            else {
                // always update form value
                if (GMatchCase != chkMatchCase.Checked) {
                    ResetSearch();
                }
                // save form matchcase value
                GMatchCase = chkMatchCase.Checked;
                // always reset synonym search
                GFindSynonym = false;
            }
        }

        private void cmbDirection_SelectedIndexChanged(object sender, EventArgs e) {
            // always update form value
            if ((int)GFindDir != cmbDirection.SelectedIndex) {
                ResetSearch();
                GFindDir = (FindDirection)cmbDirection.SelectedIndex;
            }
        }

        private void optCurrentLogic_CheckedChanged(object sender, EventArgs e) {
            if (GLogFindLoc != FindLocation.Current) {
                ResetSearch();
            }
            GLogFindLoc = FindLocation.Current;
        }

        private void optAllOpenLogics_CheckedChanged(object sender, EventArgs e) {
            if (GLogFindLoc != FindLocation.Open) {
                ResetSearch();
            }
            GLogFindLoc = FindLocation.Open;
        }

        private void optAllGameLogics_CheckedChanged(object sender, EventArgs e) {
            if (GLogFindLoc != FindLocation.All) {
                ResetSearch();
            }
            GLogFindLoc = FindLocation.All;
        }

        private void cmdFind_Click(object sender, EventArgs e) {
            BeginSearch(FindFormAction.Find);
        }

        private void cmdReplace_Click(object sender, EventArgs e) {
            if (MDIMain.ActiveMdiChild == null) {
                Debug.Assert(false);
                return;
            }
            if (FormFunction == FindFormFunction.FindLogic) {
                // switch to replace
                SetForm(FindFormFunction.ReplaceLogic, optAllGameLogics.Enabled);
                rtfReplace.Select();
            }
            else if (FormFunction == FindFormFunction.FindObject) {
                SetForm(FindFormFunction.ReplaceObject, false);
                rtfReplace.Select();
            }
            else if (FormFunction == FindFormFunction.FindWord) {
                SetForm(FindFormFunction.ReplaceWord, false);
                rtfReplace.Select();
            }
            else {
                if (optCurrentLogic.Checked) {
                    GLogFindLoc = FindLocation.Current;
                }
                else if (optAllOpenLogics.Checked) {
                    GLogFindLoc = FindLocation.Open;
                }
                else if (optAllGameLogics.Checked) {
                    GLogFindLoc = FindLocation.All;
                }
                BeginSearch(FindFormAction.Replace);
            }
        }

        private void cmdReplaceAll_Click(object sender, EventArgs e) {
            if (MDIMain.ActiveMdiChild == null) {
                Debug.Assert(false);
                return;
            }
            BeginSearch(FindFormAction.ReplaceAll);
        }

        private void cmdCancel_Click(object sender, EventArgs e) {
            FormAction = FindFormAction.Cancel;
            Visible = false;
        }

        private void rtfReplace_KeyPress(object sender, KeyPressEventArgs e) {

        }

        private void rtfReplace_TextChanged(object sender, EventArgs e) {
            if (GReplaceText != rtfReplace.Text) {
                ResetSearch();
            }
            GReplaceText = rtfReplace.Text;
        }

        private void rtfReplace_Enter(object sender, EventArgs e) {
            rtfReplace.SelectAll();
        }

        private void cmbFind_TextChanged(object sender, EventArgs e) {
            cmdFind.Enabled = cmbFind.Text.Length > 0;
            cmdReplace.Enabled = cmdFind.Enabled || FormFunction == FindFormFunction.FindLogic;
            cmdReplaceAll.Enabled = cmdFind.Enabled;
            // always update form value
            if (GFindText != cmbFind.Text) {
                ResetSearch();
            }
            GFindText = cmbFind.Text;
        }

        private void cmbFind_Enter(object sender, EventArgs e) {
            cmbFind.SelectAll();
        }

        private void cmbFind_KeyPress(object sender, KeyPressEventArgs e) {

        }

        #endregion

        internal void SetForm(FindFormFunction newfunction, bool ingame) {
            FormFunction = newfunction;
            // reset controls to defaults
            optCurrentLogic.Enabled = true;
            optAllOpenLogics.Enabled = true;
            cmbFind.Enabled = true;
            cmbDirection.Enabled = false;
            chkMatchWord.Enabled = true;
            chkMatchCase.Text = "Match Case";
            //if (!Visible) {
                cmbDirection.SelectedIndex = (int)GFindDir;
                chkMatchWord.Checked = GMatchWord;
                rtfReplace.Text = GReplaceText;
                chkMatchCase.Checked = GMatchCase;
            //}
            cmbFind.Text = GFindText;

            switch (FormFunction) {
            case FindFormFunction.FindLogic or FindFormFunction.FindText:
                // hide replace controls (except the replace button- just change caption)
                cmdReplace.Visible = true;
                cmdReplace.Text = "Replace";
                cmdReplaceAll.Visible = false;
                rtfReplace.Visible = false;
                lblReplace.Visible = false;
                // direction is available for logic searches
                cmbDirection.Enabled = true;
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
                if (EditGame != null && ingame) {
                    optAllGameLogics.Enabled = true;
                }
                else {
                    if (optAllGameLogics.Checked) {
                        // since it's not a valid option if 'AllGameLogics'
                        // is selected, change it to 'AllOpenLogics'
                        // change to all open logics
                        optAllOpenLogics.Checked = true;
                    }
                    optAllGameLogics.Enabled = false;
                }
                // adjust text of option button and caption
                if (FormFunction == FindFormFunction.FindLogic) {
                    // search logic
                    Text = "Find in Logics";
                    optCurrentLogic.Text = "Current Logic";
                }
                else {
                    // search text
                    Text = "Find in Text Files";
                    optCurrentLogic.Text = "Current File";
                }
                chkMatchCase.Visible = true;
                // never search for synonyms
                GFindSynonym = false;
                break;
            case FindFormFunction.FindObject:
                // hide replace controls (except the replace button- just change caption)
                cmdReplace.Visible = true;
                cmdReplace.Text = "Replace";
                cmdReplaceAll.Visible = false;
                rtfReplace.Visible = false;
                lblReplace.Visible = false;
                fraLogic.Visible = false;
                Text = "Find in OBJECT File";
                chkMatchCase.Visible = true;
                cmbDirection.Enabled = true;
                // never search for synonyms
                GFindSynonym = false;
                break;
            case FindFormFunction.FindWord:
                // hide replace controls (except the replace button- just change caption)
                cmdReplace.Visible = true;
                cmdReplace.Text = "Replace";
                cmdReplaceAll.Visible = false;
                rtfReplace.Visible = false;
                lblReplace.Visible = false;
                Text = "Find in WORDS.TOK File";
                fraLogic.Visible = false;
                chkMatchCase.Visible = false;
                cmbDirection.Enabled = true;
                // never search for synonyms
                GFindSynonym = false;
                break;
            case FindFormFunction.ReplaceLogic or FindFormFunction.ReplaceText:
                cmdReplace.Visible = true;
                cmdReplace.Text = "Replace";
                cmdReplaceAll.Visible = true;
                rtfReplace.Visible = true;
                lblReplace.Visible = true;
                cmbDirection.Enabled = true;
                chkMatchCase.Visible = true;
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
                if (EditGame == null || !ingame) {
                    if (optAllGameLogics.Checked) {
                        // since it's not a valid option if 'AllGameLogics'
                        // is selected, change it to 'AllOpenLogics'
                        // change to all open logics
                        optAllOpenLogics.Checked = true;
                    }
                }
                if (FormFunction == FindFormFunction.ReplaceLogic) {
                    Text = "Replace in Logics";
                    optCurrentLogic.Text = "Current Logic";
                }
                else {
                    Text = "Replace in Text Files";
                    optCurrentLogic.Text = "Current File";
                }
                optAllGameLogics.Enabled = EditGame != null;
                // never search for synonyms
                GFindSynonym = false;
                break;
            case FindFormFunction.ReplaceObject:
                cmdReplace.Visible = true;
                cmdReplace.Text = "Replace";
                cmdReplaceAll.Visible = true;
                rtfReplace.Visible = true;
                lblReplace.Visible = true;
                chkMatchCase.Visible = true;
                fraLogic.Visible = false;
                Text = "Replace in OBJECT File";
                cmbDirection.Enabled = true;
                // never search for synonyms
                GFindSynonym = false;
                break;
            case FindFormFunction.ReplaceWord:
                cmdReplace.Visible = true;
                cmdReplace.Text = "Replace";
                cmdReplaceAll.Visible = true;
                rtfReplace.Visible = true;
                lblReplace.Visible = true;
                chkMatchCase.Visible = false;
                fraLogic.Visible = false;
                Text = "Replace in WORDS.TOK File";
                cmbDirection.Enabled = true;
                // never search for synonyms
                GFindSynonym = false;
                break;
            case FindFormFunction.FindWordsLogic:
                cmdReplace.Visible = false;
                cmdReplace.Enabled = false;
                cmdReplaceAll.Visible = false;
                rtfReplace.Visible = false;
                lblReplace.Visible = false;
                // show logic frame, but disable everything
                fraLogic.Visible = true;
                optAllGameLogics.Checked = true;
                optCurrentLogic.Enabled = false;
                optAllOpenLogics.Enabled = false;
                cmbFind.Enabled = false;
                cmbDirection.Enabled = false;
                chkMatchWord.Enabled = false;
                Text = "Find Word in Logics";
                chkMatchCase.Visible = true;
                chkMatchCase.Text = "Include Synonyms";
                chkMatchCase.Checked = GFindSynonym;
                break;
                // currently, 'find objs in logic' mode is not coded
                // object search uses the normal 'find in logic' mode
                //case ffFindObjsLogic:
                //    break;
            }
            // adjust height based on selection
            switch (FormFunction) {
            case FindFormFunction.FindLogic:
            case FindFormFunction.FindObject:
            case FindFormFunction.FindWord:
            case FindFormFunction.FindText:
                cmdCancel.Top = 144 - 33;
                chkMatchWord.Top = 121 - 33;
                chkMatchCase.Top = 146 - 33;
                lblDirection.Top = 87 - 33;
                cmbDirection.Top = 84 - 33;
                fraLogic.Top = 77 - 33;
                Height = 215 - 33;
                break;
            default:
                cmdCancel.Top = 144;
                chkMatchWord.Top = 121;
                chkMatchCase.Top = 146;
                lblDirection.Top = 87;
                cmbDirection.Top = 84;
                fraLogic.Top = 77;
                Height = 215;
                break;
            }
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
            ReplaceCount = 0;
            // object editor search parameters
            ObjStartPos = -1;
            // word editor search parameters
            StartWord = -1;
            StartGrp = -1;
        }

        private void BeginSearch(FindFormAction action) {
            MDIMain.UseWaitCursor = true;
            FormAction = action;
            // search Type can only be set by beginning a search
            // from the objects or words form; force searchtype
            // back to none
            SearchType = Engine.AGIResType.None;
            SearchStartDlg = true;
            UpdateSearchList();
            switch (FormFunction) {
            case FindFormFunction.FindLogic:
            case FindFormFunction.ReplaceLogic:
                Form startsearchform;
                // always reset the synonym search
                GFindSynonym = false;
                // confirm starting frm a logic
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
                    ReplaceAll(startsearchform, GFindText, GReplaceText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc);
                    break;
                }
                break;
            case FindFormFunction.FindWordsLogic:
                break;
            case FindFormFunction.FindObjsLogic:
                break;
            case FindFormFunction.FindWord:
            case FindFormFunction.ReplaceWord:
                break;
            case FindFormFunction.FindObject:
            case FindFormFunction.ReplaceObject:
                if (MDIMain.ActiveMdiChild is frmObjectEdit) {
                    switch (FindingForm.FormAction) {
                    case FindFormAction.Find:
                        ((frmObjectEdit)(MDIMain.ActiveMdiChild)).FindInObjects(GFindText, GFindDir, GMatchWord, GMatchCase);
                        break;
                    case FindFormAction.Replace:
                        ((frmObjectEdit)(MDIMain.ActiveMdiChild)).FindInObjects(GFindText, GFindDir, GMatchWord, GMatchCase, true, GReplaceText);
                        break;
                    case FindFormAction.ReplaceAll:
                        ((frmObjectEdit)(MDIMain.ActiveMdiChild)).ReplaceAll(GFindText, GReplaceText, GMatchWord, GMatchCase);
                        break;
                    }
                }
                break;
            case FindFormFunction.FindText:
            case FindFormFunction.ReplaceText:
                break;
            case FindFormFunction.FindNone:
                break;
            }
            // ALWAYS reset the start-in-dialog flag
            SearchStartDlg = false;
            MDIMain.UseWaitCursor = false;
        }

        internal void UpdateSearchList() {
            if (!cmbFind.Items.Contains(cmbFind.Text)) {
                cmbFind.Items.Insert(0, cmbFind.Text);
            }
            if (cmbFind.Items.Count == 12) {
                cmbFind.Items.RemoveAt(11);
            }
        }

        private void frmFind_FormClosing(object sender, FormClosingEventArgs e) {
            // never close the form, just hide it
            e.Cancel = true;
            Hide();
        }

        /*
Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
  
  'check for help key
  If Shift = 0 And KeyCode = vbKeyF1 Then
    frmMDIMain.mnuHContents_Click
    KeyCode = 0
  End If
  
  'check for 'find next key
  If Shift = 0 And KeyCode = vbKeyF3 Then
    'assume logic or text editor has focus
    If frmMDIMain.ActiveForm.Name = "frmLogicEdit" Or frmMDIMain.ActiveForm.Name = "frmTextEdit" Then
      frmMDIMain.ActiveForm.MenuClickFindAgain
    End If
  End If
End Sub

*/
    }
}
