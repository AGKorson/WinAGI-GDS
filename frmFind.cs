using System;
using System.Diagnostics;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmFind : Form {
        #region Fields
        // none
        #endregion

        #region Constructors
        public frmFind() {
            InitializeComponent();
            cmbFind.DataSource = Search.SearchTerms;
            cmbDirection.SelectedIndex = 0;
            MDIMain.AddOwnedForm(this);
            Owner = MDIMain;
        }
        #endregion

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

        private void chkMatchWord_Click(object sender, EventArgs e) {
            // toggle case
            Search.MatchWord = !Search.MatchWord;
        }

        private void chkSynonyms_Click(object sender, EventArgs e) {
            Search.FindSynonym = !Search.FindSynonym;
        }

        private void chkMatchCase_Click(object sender, EventArgs e) {
            // toggle case
            Search.MatchCase = !Search.MatchCase;

            // always reset synonym search
            Search.FindSynonym = false;
        }

        private void cmbDirection_SelectionChangeCommitted(object sender, EventArgs e) {
            switch (cmbDirection.SelectedIndex) {
            case 0:
                Search.Direction = SearchDirection.Next;
                break;
            case 1:
                Search.Direction = SearchDirection.Previous;
                break;
            }
        }

        private void optCurrent_Click(object sender, EventArgs e) {
            Search.Scope = SearchScope.Current;
        }

        private void optOpen_Click(object sender, EventArgs e) {
            Search.Scope = SearchScope.Open;
        }

        private void optProject_Click(object sender, EventArgs e) {
            Search.Scope = SearchScope.All;
        }

        private void btnFind_Click(object sender, EventArgs e) {
            if (txtFind.Text.Length == 0) {
                return;
            }
            Search.FindText = txtFind.Text;
            BeginSearch(Search, FindAction.Find);
        }

        private void btnFindAll_Click(object sender, EventArgs e) {
            if (txtFind.Text.Length == 0) {
                return;
            }
            Search.FindText = txtFind.Text;
            var results = FindAll(Search);
            MDIMain.AddFindAllItems(results);
        }

        private void btnReplace_Click(object sender, EventArgs e) {
            if (MDIMain.ActiveMdiChild is null) {
                Debug.Assert(false);
                return;
            }
            switch (Search.Mode) {
            case SearchMode.FindLogic:
                Search.Mode = SearchMode.ReplaceLogic;
                break;
            case SearchMode.FindObject:
                Search.Mode = SearchMode.ReplaceObject;
                break;
            case SearchMode.FindWord:
                Search.Mode = SearchMode.ReplaceWord;
                break;
            case SearchMode.FindText:
                Search.Mode = SearchMode.ReplaceText;
                break;
            case SearchMode.FindObjsLogic:
                Search.Mode = SearchMode.ReplaceObjsLogic;
                break;
            case SearchMode.FindWordsLogic:
                Search.Mode = SearchMode.ReplaceWordsLogic;
                break;
            default:
                if (txtFind.Text.Length == 0) {
                    return;
                }
                Search.FindText = txtFind.Text;
                BeginSearch(Search, FindAction.Replace);
                return;
            }
            rtfReplace.Select();
        }

        private void btnReplaceAll_Click(object sender, EventArgs e) {
            if (MDIMain.ActiveMdiChild is null) {
                Debug.Assert(false);
                return;
            }
            if (txtFind.Text.Length == 0) {
                return;
            }
            Search.FindText = txtFind.Text;
            BeginSearch(Search, FindAction.ReplaceAll);
        }

        private void btnClose_Click(object sender, EventArgs e) {
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
            Search.ReplaceText = rtfReplace.Text;
        }

        private void rtfReplace_Enter(object sender, EventArgs e) {
            rtfReplace.SelectAll();
        }

        private void txtFind_Enter(object sender, EventArgs e) {
            txtFind.SelectAll();
        }

        private void txtFind_TextChanged(object sender, EventArgs e) {
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
            btnFind.Enabled = txtFind.Text.Length > 0;
            btnReplaceAll.Enabled = btnFind.Enabled;
            btnFindAll.Enabled = btnFind.Enabled;
            // replace never available if searching globals
            btnReplace.Enabled = btnFind.Enabled && Search.Mode != SearchMode.FindGlobals;
        }
        #endregion

        #region Methods
        internal void SetForm(SearchMode newfunction) {
            // reset controls to defaults
            chkSynonyms.Visible = false;
            chkMatchCase.Enabled = true;
            chkMatchWord.Enabled = true;
            btnReplace.Enabled = true;
            btnReplaceAll.Visible = true;
            if (EditGame is not null) {
                optProject.Enabled = true;
            }
            else {
                optProject.Enabled = false;
            }
            switch (newfunction) {
            case SearchMode.FindLogic:
            case SearchMode.FindText:
            case SearchMode.FindObject:
            case SearchMode.FindWord:
            case SearchMode.FindObjsLogic:
            case SearchMode.FindWordsLogic:
            case SearchMode.FindGlobals:
                txtFind.SelectAll();
                txtFind.Select();
                break;
            case SearchMode.ReplaceLogic:
            case SearchMode.ReplaceText:
            case SearchMode.ReplaceObject:
            case SearchMode.ReplaceWord:
            case SearchMode.ReplaceObjsLogic:
            case SearchMode.ReplaceWordsLogic:
                rtfReplace.SelectAll();
                rtfReplace.Select();
                break;
            }
            switch (Search.Mode) {
            case SearchMode.FindLogic:
            case SearchMode.FindText:
                Search.Type = AGIResType.None;
                if (Search.Mode == SearchMode.FindLogic) {
                    Text = "Find in Logics";
                    optCurrent.Text = "Current Logic";
                }
                else {
                    Text = "Find in Text Files";
                    optCurrent.Text = "Current File";
                }
                fraScope.Enabled = true;
                chkMatchWord.Enabled = true;
                break;
            case SearchMode.ReplaceLogic:
            case SearchMode.ReplaceText:
                Search.Type = AGIResType.None;
                if (Search.Mode == SearchMode.ReplaceLogic) {
                    Text = "Replace in Logics";
                    optCurrent.Text = "Current Logic";
                }
                else {
                    Text = "Replace in Text Files";
                    optCurrent.Text = "Current File";
                }
                fraScope.Enabled = true;
                chkMatchWord.Enabled = true;
                break;
            case SearchMode.FindObject:
                Search.Type = AGIResType.None;
                Text = "Find in OBJECT File";
                fraScope.Enabled = false;
                break;
            case SearchMode.ReplaceObject:
                Search.Type = AGIResType.None;
                Text = "Replace in OBJECT File";
                fraScope.Enabled = false;
                chkMatchWord.Enabled = true;
                break;
            case SearchMode.FindWord:
                Search.Type = AGIResType.None;
                Text = "Find in WORDS.TOK File";
                fraScope.Enabled = false;
                chkMatchWord.Enabled = true;
                chkMatchCase.Enabled = false;
                break;
            case SearchMode.ReplaceWord:
                Search.Type = AGIResType.None;
                Text = "Replace in WORDS.TOK File";
                fraScope.Enabled = false;
                chkMatchWord.Enabled = true;
                chkMatchCase.Enabled = false;
                break;
            case SearchMode.FindObjsLogic:
                Search.Type = AGIResType.Objects;
                Text = "Find Inventory Object in Logics";
                fraScope.Enabled = true;
                chkMatchWord.Enabled = true;
                break;
            case SearchMode.FindWordsLogic:
                Search.Type = AGIResType.Words;
                Text = "Find Word in Logics";
                fraScope.Enabled = true;
                chkMatchWord.Enabled = true;
                chkSynonyms.Visible = true;
                break;
            case SearchMode.ReplaceObjsLogic:
                Search.Type = AGIResType.Objects;
                Text = "Replace Inventory Object in Logics";
                fraScope.Enabled = true;
                chkMatchWord.Enabled = true;
                break;
            case SearchMode.ReplaceWordsLogic:
                Search.Type = AGIResType.Words;
                Text = "Replace Word in Logics";
                fraScope.Enabled = true;
                chkMatchWord.Enabled = true;
                chkSynonyms.Visible = true;
                break;
            case SearchMode.FindGlobals:
                Search.Type = AGIResType.None;
                Text = "Find in Globals List";
                fraScope.Enabled = false;
                btnReplace.Enabled = false;
                break;
            }
            if (!fraScope.Enabled) {
                if (Search.Scope != SearchScope.Current) {
                    Search.Scope = SearchScope.Current;
                }
            }
            // adjust height based on selection
            switch (Search.Mode) {
            case SearchMode.FindLogic:
            case SearchMode.FindObject:
            case SearchMode.FindWord:
            case SearchMode.FindText:
            case SearchMode.FindWordsLogic:
            case SearchMode.FindObjsLogic:
            case SearchMode.FindGlobals:
                rtfReplace.Visible = false;
                lblReplace.Visible = false;
                btnReplaceAll.Visible = false;
                Height = 182 + (txtFind.Height == 40 ? 20 : 0);
                break;
            default:
                rtfReplace.Visible = true;
                lblReplace.Visible = true;
                btnReplaceAll.Visible = true;
                Height = 215 + (txtFind.Height == 40 ? 20 : 0);
                break;
            }
        }

        internal void ShowHelp() {
            string topic = "";
            switch (Search.Mode) {
            case SearchMode.FindLogic:
            case SearchMode.ReplaceLogic:
            case SearchMode.FindText:
            case SearchMode.ReplaceText:
                topic = "htm\\winagi\\search.htm#find";
                break;
            case SearchMode.FindObject:
            case SearchMode.ReplaceObject:
                topic = "htm\\winagi\\search.htm#find";
                break;
            case SearchMode.FindObjsLogic:
            case SearchMode.ReplaceObjsLogic:
                topic = "htm\\winagi\\search.htm#findinlogics";
                break;
            case SearchMode.FindWord:
            case SearchMode.ReplaceWord:
                topic = "htm\\winagi\\search.htm#find";
                break;
            case SearchMode.FindWordsLogic:
            case SearchMode.ReplaceWordsLogic:
                topic = "htm\\winagi\\editor_words.htm#findinlogics";
                break;
            }
            Help.ShowHelp(HelpParent, WinAGIHelp, topic);
        }
        #endregion
    }
}
