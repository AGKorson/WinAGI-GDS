using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Common.Base;
using System.IO;
using System.Diagnostics;

namespace WinAGI.Editor {
    public partial class frmImportProperties : Form {
        public string ImportDir = "";
        public int NewCodePage;

        public frmImportProperties() {
            InitializeComponent();
            // load code pages
            cmbCodePage.Items.Add("437 - U.S. English (Default)");
            cmbCodePage.Items.Add("737 - Greek II");
            cmbCodePage.Items.Add("775 - Baltic Rim");
            cmbCodePage.Items.Add("850 - MultiLingual (Latin I");
            cmbCodePage.Items.Add("858 - MultiLingual (Latin I) with Euro Sign");
            cmbCodePage.Items.Add("852 - Slavic/Eastern Europe (Latin II)");
            cmbCodePage.Items.Add("855 - Cyrillic I");
            cmbCodePage.Items.Add("857 - Turkish");
            cmbCodePage.Items.Add("860 - Portuguese");
            cmbCodePage.Items.Add("861 - Icelandic");
            cmbCodePage.Items.Add("862 - Hebrew");
            cmbCodePage.Items.Add("863 - Canadian-French");
            cmbCodePage.Items.Add("865 - Nordic");
            cmbCodePage.Items.Add("866 - Russian (Cyrillic II)");
            cmbCodePage.Items.Add("869 - Greek");
            cmbCodePage.SelectedIndex = 0;

            txtResDir.Text = WinAGI.Engine.Base.DefResDir;
            txtSrcExt.Text = LogicDecoder.DefaultSrcExt;
            // include options
            chkResourceIDs.Checked = WinAGISettings.DefIncludeIDs.Value;
            chkResDefs.Checked = WinAGISettings.DefIncludeReserved.Value;
            chkGlobals.Checked = WinAGISettings.DefIncludeGlobals.Value;
            // code page starts at default
            for (int i = 0; i < cmbCodePage.Items.Count; i++) {
                if (int.Parse(((string)cmbCodePage.Items[i])[..3]) == Engine.Base.CodePage) {
                    cmbCodePage.SelectedIndex = i;
                    break;
                }
            }
            NewCodePage = Engine.Base.CodePage;
        }

        #region Event Handlers
        private void btnAdvanced_Click(object sender, EventArgs e) {
            btnAdvanced.Visible = false;
            Height = 348;
            chkSierraSyntax.Visible = true;
            cmbCodePage.Visible = true;
        }

        private void frmGameProperties_HelpRequested(object sender, HelpEventArgs hlpevent) {
            string topic = @"htm\winagi\managinggames.htm#import";
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
            hlpevent.Handled = true;
        }

        private void btnOK_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void txtGameDir_DoubleClick(object sender, EventArgs e) {
            // same as clicking button
            ChangeDisplayGameDir();
        }

        private void btnGameDir_Click(object sender, EventArgs e) {
            // change directory to import from
            ChangeDisplayGameDir();
        }

        private void txtResDir_TextChanged(object sender, EventArgs e) {
            // enable ok if a directory has been chosen
            btnOK.Enabled = ImportDir.Length > 0;
        }

        private void txtResDir_Validating(object sender, CancelEventArgs e) {
            // no file/folder prohibited characters, only standard ascii
            string dirtext = "";
            foreach (char c in txtResDir.Text) {
                if (c > 32 && c < 127) {
                    // no invalid path chars
                    if (!(Path.GetInvalidFileNameChars()).Contains(c)) {
                        dirtext += c;
                    }
                }
            }
            txtResDir.Text = dirtext;
        }

        private void txtResDir_KeyPress(object sender, KeyPressEventArgs e) {
            // validate- can't have  "\/:*?<>|
            switch ((int)e.KeyChar) {
            case 13:
                // enter is same as tabbing to next control
                e.Handled = true;
                txtSrcExt.Select();
                break;
            case <= 7:
            case >= 9 and <= 32:
            case 34 or 42 or 47 or 58 or 60 or 62 or 63 or 92 or 124:
            case > 126:
                // not allowed
                e.Handled = true;
                break;
            }
        }

        private void txtSrcExt_TextChanged(object sender, EventArgs e) {
            // only 4 characters
            if (txtSrcExt.TextLength > 4) {
                txtSrcExt.Text = txtSrcExt.Text[..4];
            }
            // enable ok if a directory has been chosen
            btnOK.Enabled = ImportDir.Length > 0;
        }

        private void txtSrcExt_KeyPress(object sender, KeyPressEventArgs e) {
            // validate- can't have  "\/:*?<>|
            switch ((int)e.KeyChar) {
            case 13:
                // enter is same as tabbing to next control
                e.Handled = true;
                chkResourceIDs.Select();
                break;
            case <= 7:
            case >= 9 and <= 32:
            case 34 or 42 or 47 or 58 or 60 or 62 or 63 or 92 or 124:
            case > 126:
                // not allowed
                e.Handled = true;
                break;
            }
        }

        private void txtSrcExt_Leave(object sender, EventArgs e) {
            if (txtSrcExt.Text == "") {
                txtSrcExt.Text = "lgc";
            }
        }

        private void txtSrcExt_Validating(object sender, CancelEventArgs e) {
            // all versions limit to five characters
            if (txtSrcExt.TextLength > 5) {
                txtSrcExt.Text = txtSrcExt.Text[..5];
            }
            // no file/folder prohibited characters, only standard ascii
            string exttext = "";
            foreach (char c in txtSrcExt.Text) {
                if (c > 32 && c < 127) {
                    // no invalid path chars
                    if (!(Path.GetInvalidFileNameChars()).Contains(c)) {
                        exttext += c;
                    }
                }
            }
            if (txtSrcExt.Text == "") {
                txtSrcExt.Text = "lgc";
            }
            txtSrcExt.Text = exttext;
        }

        private void chkUseLE_Click(object sender, EventArgs e) {
            // enable ok if a directory has been chosen
            btnOK.Enabled = ImportDir.Length > 0;
        }

        private void chkResourceIDs_Click(object sender, EventArgs e) {
            btnOK.Enabled = ImportDir.Length > 0;
        }

        private void chkUseReserved_Click(object sender, EventArgs e) {
            btnOK.Enabled = ImportDir.Length > 0;
        }

        private void chkGlobals_Click(object sender, EventArgs e) {
            btnOK.Enabled = ImportDir.Length > 0;
        }
        
        private void chkSierraSyntax_Click(object sender, EventArgs e) {
            // auto-includes not available if using Sierra syntax
            chkGlobals.Enabled = !chkSierraSyntax.Checked;
            chkResDefs.Enabled = !chkSierraSyntax.Checked;
            chkResourceIDs.Enabled = !chkSierraSyntax.Checked;

            // force extension to 'cg', but allow user to change it
            if (chkSierraSyntax.Checked) {
                txtSrcExt.Text = "cg";
            }
            // force resourcedir to 'SRC', and DON'T allow user to change it
            if (chkSierraSyntax.Checked) {
                txtResDir.Text = "SRC";
            }
            txtResDir.Enabled = !chkSierraSyntax.Checked;

            // enable ok if a directory has been chosen
            btnOK.Enabled = ImportDir.Length > 0;
        }

        private void cmbCodePage_SelectionChangeCommitted(object sender, EventArgs e) {
            // change codepage
            NewCodePage = int.Parse(((string)cmbCodePage.SelectedItem)[..3]);
            // enable ok if a directory has been chosen
            btnOK.Enabled = ImportDir.Length > 0;
        }
        #endregion

        private void ChangeDisplayGameDir() {
            if (ImportDir.Length > 0) {
                BrowserStartDir = ImportDir;
            }
            // get a directory from which to load a game
            MDIMain.FolderDlg.Description = "Select a directory for this new game:";
            MDIMain.FolderDlg.AddToRecent = false;
            MDIMain.FolderDlg.InitialDirectory = BrowserStartDir;
            MDIMain.FolderDlg.SelectedPath = "";
            MDIMain.FolderDlg.OkRequiresInteraction = true;
            MDIMain.FolderDlg.ShowNewFolderButton = true;

            if (MDIMain.FolderDlg.ShowDialog() == DialogResult.OK) {
                ImportDir = FullDir(MDIMain.FolderDlg.SelectedPath);
                txtGameDir.Text = ImportDir;
                BrowserStartDir = ImportDir;
            }
            btnOK.Enabled = (txtGameDir.TextLength > 0 && ImportDir.Length > 0);
        }
    }
}
