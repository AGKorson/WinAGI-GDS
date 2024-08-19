using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Common.Base;
using System.IO;

namespace WinAGI.Editor {
    public partial class frmGameProperties : Form {
        private GameSettingFunction WindowFunction;
        public string NewPlatformFile;
        public Encoding NewCodePage;
        public string DisplayDir;
        public bool UseSierraSyntax;

        public frmGameProperties(GameSettingFunction mode) {
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

            // load versions
            cmbVersion.Items.Add("2.089");
            cmbVersion.Items.Add("2.272");
            cmbVersion.Items.Add("2.411");
            cmbVersion.Items.Add("2.425");
            cmbVersion.Items.Add("2.426");
            cmbVersion.Items.Add("2.435");
            cmbVersion.Items.Add("2.439");
            cmbVersion.Items.Add("2.440");
            cmbVersion.Items.Add("2.903");
            cmbVersion.Items.Add("2.911");
            cmbVersion.Items.Add("2.912");
            cmbVersion.Items.Add("2.915");
            cmbVersion.Items.Add("2.917");
            cmbVersion.Items.Add("2.936");
            cmbVersion.Items.Add("3.002086");
            cmbVersion.Items.Add("3.002098");
            cmbVersion.Items.Add("3.002102");
            cmbVersion.Items.Add("3.002107");
            cmbVersion.Items.Add("3.002149");

            // make sure form uses same font settings as the text boxes
            this.Font = txtGameDir.Font;

            SetForm(mode);
        }

        private void SetForm(GameSettingFunction mode) {
            switch (mode) {
            case GameSettingFunction.gsEdit:
                break;
            case GameSettingFunction.gsNew:
                break;
            }
            switch (WindowFunction) {
            case GameSettingFunction.gsEdit:
                // get values from open game
                txtGameAuthor.Text = EditGame.GameAuthor;
                txtGameVersion.Text = EditGame.GameVersion;
                txtGameAbout.Text = EditGame.GameAbout;
                txtGameDescription.Text = EditGame.GameDescription;

                // version
                for (int i = 0; i < cmbVersion.Items.Count; i++) {
                    if ((string)cmbVersion.Items[i] == EditGame.InterpreterVersion) {
                        cmbVersion.SelectedIndex = i;
                        break;
                    }
                }
                // assign gameid
                txtGameID.Text = EditGame.GameID;

                DisplayDir = EditGame.GameDir;
                txtGameDir.Text = DisplayDir;

                // game dir is read only when editing
                txtGameDir.Enabled = false;
                btnGameDir.Enabled = false;

                // resdir is just the directory by itself, not entire path
                txtResDir.Text = EditGame.ResDirName;

                // don't show leading '.' characters in extension
                txtSrcExt.Text = EditGame.Logics.SourceFileExt[1..];

                // select the button for platform
                switch (EditGame.PlatformType) {
                case Engine.PlatformTypeEnum.None:
                    // None - do nothing; form is already correct
                    break;
                case Engine.PlatformTypeEnum.DosBox:
                    optDosBox.Checked = true;
                    lblExec.Enabled = true;
                    btnPlatformFile.Enabled = true;
                    txtPlatformFile.Enabled = true;
                    lblOptions.Enabled = true;
                    txtOptions.Enabled = true;
                    lblExec.Enabled = true;
                    txtExec.Enabled = true;
                    break;
                case Engine.PlatformTypeEnum.ScummVM:
                    optScummVM.Checked = true;
                    lblExec.Enabled = true;
                    btnPlatformFile.Enabled = true;
                    txtPlatformFile.Enabled = true;
                    lblOptions.Enabled = true;
                    txtOptions.Enabled = true;
                    break;
                case Engine.PlatformTypeEnum.NAGI:
                    optNAGI.Checked = true;
                    // no options for NAGI
                    // and platform file is always "n.exe" in game dir
                    break;
                case Engine.PlatformTypeEnum.Other:
                    optOther.Checked = true;
                    lblExec.Enabled = true;
                    btnPlatformFile.Enabled = true;
                    txtPlatformFile.Enabled = true;
                    lblOptions.Enabled = true;
                    txtOptions.Enabled = true;
                    break;
                }

                // if platform is nothing, skip directory
                if (txtPlatformFile.Enabled) {
                    NewPlatformFile = EditGame.Platform;
                    txtPlatformFile.Text = NewPlatformFile;
                }
                else {
                    NewPlatformFile = "";
                }

                if (txtOptions.Enabled) {
                    txtOptions.Text = EditGame.PlatformOpts;
                }
                if (txtExec.Enabled) {
                    txtExec.Text = EditGame.DOSExec;
                }
                // resdefines
                chkUseReserved.Checked = LogicCompiler.UseReservedNames;

                // layout editor
                chkUseLE.Checked = EditGame.UseLE;

                // code page
                for (int i = 0; i < cmbCodePage.Items.Count; i++) {
                    if (((string)cmbCodePage.Items[i])[..3] == EditGame.CodePage.CodePage.ToString()) {
                        cmbCodePage.SelectedIndex = i;
                        break;
                    }
                }

                // sierra syntax option
                if (EditGame.SierraSyntax) {
                    chkSierraSyntax.Checked = true;
                }

                // set caption
                Text = "Edit Game Properties";
                break;
            case GameSettingFunction.gsNew:
                // default values
                txtGameAuthor.Text = "";
                txtGameVersion.Text = "AGI Game version 0.0";
                txtGameAbout.Text = "AGI Game by <author>";
                txtGameDescription.Text = "new agi game";
                cmbVersion.SelectedItem = "2.917";
                txtGameID.Text = "AGI";
                // game dir is editable when creating new
                txtGameDir.Enabled = true;
                btnGameDir.Enabled = true;
                DisplayDir = "";
                txtGameDir.Text = "";
                txtResDir.Text = WinAGI.Engine.Base.DefResDir;
                // don't show leading '.' characters in extension
                txtSrcExt.Text = LogicDecoder.DefaultSrcExt[1..];

                // resdefines
                chkUseReserved.Checked = WinAGISettings.DefUseResDef;
                // layout editor
                chkUseLE.Checked = WinAGISettings.DefUseLE;

                // code page starts at default
                for (int i = 0; i < cmbCodePage.Items.Count; i++) {
                    if ((int)cmbCodePage.Items[i] == WinAGISettings.DefCP) {
                        cmbCodePage.SelectedIndex = i;
                        break;
                    }
                }
                // sierra syntax is off by default
                chkSierraSyntax.Checked = false;

                // set caption
                Text = "New Game Properties";
                break;
            }

            // disable OK until a change is made
            btnOK.Enabled = false;
        }

        private void frmGameProperties_Load(object sender, EventArgs e) {

        }

        private void chkSierraSyntax_Click(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void chkUseLE_CheckedChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void chkUseReserved_CheckedChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void cmbCodePage_SelectionChangeCommitted(object sender, EventArgs e) {
            // change codepage
            try {
                NewCodePage = Encoding.GetEncoding(int.Parse(((string)cmbCodePage.SelectedItem)[..3]));
            }
            catch {
                MessageBox.Show("NOPE");
            }
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void cmbVersion_SelectionChangeCommitted(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void btnOK_Click(object sender, EventArgs e) {
            // validate input

            // must select a directory
            if (DisplayDir.Length == 0) {
                MessageBox.Show(MDIMain, "Choose a directory for this new game.", "New Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void optDosBox_CheckedChanged(object sender, EventArgs e) {
            if (optDosBox.Checked) {
                // dir and options are allowed for dosbox
                lblPlatformFile.Enabled = true;
                btnPlatformFile.Enabled = true;
                txtPlatformFile.Enabled = true;
                lblOptions.Enabled = true;
                txtOptions.Enabled = true;
                lblExec.Enabled = true;
                txtExec.Enabled = true;
                // clear the dir and options boxes
                txtOptions.Text = "";
                NewPlatformFile = "";
                txtPlatformFile.Text = "";
                // enable ok if an ID and directory have been chosen
                btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
            }
        }

        private void optScummVM_CheckedChanged(object sender, EventArgs e) {
            if (optScummVM.Checked) {
                // dir and options are allowed for scummvm
                lblPlatformFile.Enabled = true;
                btnPlatformFile.Enabled = true;
                txtPlatformFile.Enabled = true;
                lblOptions.Enabled = true;
                txtOptions.Enabled = true;
                // dos executable is NA
                lblExec.Enabled = false;
                txtExec.Enabled = false;
                // clear the dir and options boxes
                txtOptions.Text = "";
                NewPlatformFile = "";
                txtPlatformFile.Text = "";
                // enable ok if an ID and directory have been chosen
                btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
            }
        }

        private void optNAGI_CheckedChanged(object sender, EventArgs e) {
            if (optNAGI.Checked) {
                // platform dir is automatically the game dir
                lblPlatformFile.Enabled = false;
                btnPlatformFile.Enabled = false;
                // dos executable is NA
                lblExec.Enabled = false;
                txtExec.Enabled = false;
                // save this directory/filename combo
                NewPlatformFile = EditGame.GameDir + "n.exe";
                txtPlatformFile.Text = NewPlatformFile;
                txtPlatformFile.Enabled = false;
                btnPlatformFile.Enabled = false;
                // no options for nagi
                lblOptions.Enabled = false;
                txtOptions.Enabled = false;
                txtOptions.Text = "";
                // enable ok if an ID and directory have been chosen
                btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
            }
        }

        private void optOther_CheckedChanged(object sender, EventArgs e) {
            if (optOther.Checked) {
                // dir and options are allowed for dosbox
                lblPlatformFile.Enabled = true;
                btnPlatformFile.Enabled = true;
                txtPlatformFile.Enabled = true;
                lblOptions.Enabled = true;
                txtOptions.Enabled = true;
                // dos executable is NA
                lblExec.Enabled = false;
                txtExec.Enabled = false;
                // clear everything; this is easiest way to 'reset'
                txtPlatformFile.Text = "";
                txtOptions.Text = "";
                txtExec.Text = "";
                NewPlatformFile = "";
                // enable ok if an ID and directory have been chosen
                btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
            }
        }

        private void btnGameDir_Click(object sender, EventArgs e) {
            // change/create directory that is displayed (doesn't change actual game directory)
            ChangeDisplayGameDir();
        }

        private void btnPlatformFile_Click(object sender, EventArgs e) {
            ChangePlatformApp();
        }

        private void ChangePlatformApp() {
            // get a platform executable that will run a game
            if (NewPlatformFile.Length == 0) {
                MDIMain.OpenDlg.FileName = "";
            }
            else {
                MDIMain.OpenDlg.FileName = NewPlatformFile;
            }
            MDIMain.OpenDlg.Title = "Choose Platform Application";
            MDIMain.OpenDlg.ShowReadOnly = false;
            MDIMain.OpenDlg.InitialDirectory = EditGame.GameDir;
            MDIMain.OpenDlg.RestoreDirectory = true;
            MDIMain.OpenDlg.Filter = "Executables (*.exe; *.com)|*.exe;*.com|All files (*.*)|*.*";
            MDIMain.OpenDlg.FilterIndex = 1;
            if (MDIMain.OpenDlg.ShowDialog(MDIMain) == DialogResult.OK) {
                // save this directory/filename combo
                NewPlatformFile = MDIMain.OpenDlg.FileName;
                txtPlatformFile.Text = NewPlatformFile;
            }
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtExec_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtExec_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtPlatformFile.Focus();
            }
        }

        private void txtGameAuthor_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameAuthor_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtGameDescription.Focus();
            }
        }

        private void txtGameDescription_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameDescription_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtGameAbout.Focus();
            }
        }

        private void txtGameAbout_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameAbout_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtGameVersion.Focus();
            }
        }

        private void txtGameVersion_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameVersion_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                btnOK.Focus();
            }
        }

        private void txtGameDir_DoubleClick(object sender, EventArgs e) {
            // same as clicking button
            ChangeDisplayGameDir();
        }

        private void txtGameID_TextChanged(object sender, EventArgs e) {
            // during setup, form is not visible, and we don't need to validate the ID
            if (!this.Visible) {
                return;
            }
            ValidateIDText();
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void ValidateIDText() {
            // all versions limit to five characters
            if (txtGameID.TextLength > 5) {
                txtGameID.Text = txtGameID.Text[..5];
            }
            // no file/folder prohibited characters, only standard ascii
            string idtext = "";
            foreach (char c in txtGameID.Text) {
                if (c > 32 && c < 127) {
                    //no invalid path chars
                    if (!(Path.GetInvalidFileNameChars()).Contains(c)) {
                        idtext += c;
                    }
                }
            }
            txtGameID.Text = idtext;
        }

        private void txtGameID_KeyPress(object sender, KeyPressEventArgs e) {
            // allow alpha numeric only, lower case allowed
            switch ((int)e.KeyChar) {
            case 13:
                // enter is same as tabbing to next control
                e.Handled = true;
                cmbVersion.Focus();
                break;
            case <= 7:
            case >= 9 and <= 47:
            case >= 91 and <= 96:
            case >= 123:
                // not allowed
                e.Handled = true;
                break;
            }
        }

        private void txtOptions_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtOptions_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                btnOK.Focus();
            }
        }

        private void txtPlatformFile_DoubleClick(object sender, EventArgs e) {
            ChangePlatformApp();
        }

        private void txtPlatformFile_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtOptions.Focus();
            }
        }

        private void txtResDir_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtResDir_KeyPress(object sender, KeyPressEventArgs e) {
            // validate- can't have  "\/:*?<>|
            switch ((int)e.KeyChar) {
            case 13:
                // enter is same as tabbing to next control
                e.Handled = true;
                txtSrcExt.Focus();
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
            // TODO: add validation checks to all text fields
            if (txtSrcExt.TextLength > 4) {
                txtSrcExt.Text = txtSrcExt.Text[..4];
            }
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtSrcExt_KeyPress(object sender, KeyPressEventArgs e) {
            // validate- can't have  "\/:*?<>|
            switch ((int)e.KeyChar) {
            case 13:
                // enter is same as tabbing to next control
                e.Handled = true;
                chkUseReserved.Focus();
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
            // if blank, use default
            if (txtSrcExt.Text == "") {
                txtSrcExt.Text = "lgc";
            }
        }

        private void ChangeDisplayGameDir() {

            string NewDisplayDir;
            int lngDirLen;

            // is there already a directory here?
            if (DisplayDir.Length > 0) {
                BrowserStartDir = DisplayDir;
            }
            // get a directory from which to load a game
            MDIMain.FolderDlg.Description = "Select a directory for this new game:";
            MDIMain.FolderDlg.AddToRecent = false;
            MDIMain.FolderDlg.InitialDirectory = BrowserStartDir;
            MDIMain.FolderDlg.OkRequiresInteraction = true;
            MDIMain.FolderDlg.ShowNewFolderButton = true;

            if (MDIMain.FolderDlg.ShowDialog() == DialogResult.OK) {
                // ensure trailing backslash
                DisplayDir = FullDir(MDIMain.FolderDlg.SelectedPath);
                txtGameDir.Text = DisplayDir;
                // set browser dir
                BrowserStartDir = DisplayDir;
            }
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameID_Validating(object sender, CancelEventArgs e) {
            // enforce character limits- 5 char max, only basic ascii,
            // no file/folder prohibited characters
            ValidateIDText();
        }

        private void txtResDir_Validating(object sender, CancelEventArgs e) {
            // no file/folder prohibited characters, only standard ascii
            string dirtext = "";
            foreach (char c in txtResDir.Text) {
                if (c > 32 && c < 127) {
                    //no invalid path chars
                    if (!(Path.GetInvalidFileNameChars()).Contains(c)) {
                        dirtext += c;
                    }
                }
            }
            txtResDir.Text = dirtext;
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
                    //no invalid path chars
                    if (!(Path.GetInvalidFileNameChars()).Contains(c)) {
                        exttext += c;
                    }
                }
            }
            txtSrcExt.Text = exttext;
        }

        /*
Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
// TODO: add help 
Dim strTopic As String

'check for help key
If Shift = 0 And KeyCode = vbKeyF1 Then
'help with game properties
strTopic = "htm\winagi\Properties.htm"

Select Case ActiveControl.Name
Case "cmbVersion"
strTopic = strTopic & "#intversion"
Case "picGameDir", "txtGameDir"
strTopic = strTopic & "#gamedir"
Case "picPlatformFile", "txtPlatformFile", "txtExec", "txtOptions", "optOther", "optDosBox", "optScummVM", "optNAGI"
strTopic = strTopic & "#executable"
Case "txtGameAbout"
strTopic = strTopic & "#about"
Case "txtGameAuthor"
strTopic = strTopic & "#author"
Case "txtGameDescription"
strTopic = strTopic & "#description"
Case "txtGameID"
strTopic = strTopic & "#gameid"
Case "txtGameVersion"
strTopic = strTopic & "#gameversion"
Case "txtResDir"
strTopic = strTopic & "#resdir"
Case "cmbCodePage"
strTopic = strTopic & "#codepage"
End Select

HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, strTopic
KeyCode = 0
End If
End Sub
*/
    }
}
