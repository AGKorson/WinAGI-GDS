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
    public partial class frmGameProperties : Form {
        public string NewPlatformFile = "";
        public int NewCodePage;
        public string DisplayDir;
        public bool UseSierraSyntax;

        public frmGameProperties(GameSettingFunction mode, string starttab = "", string startprop = "") {
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
            for (int i = 0; i < Engine.Base.IntVersions.Length; i++) {
                cmbVersion.Items.Add(Engine.Base.IntVersions[i]);
            }

            // make sure form uses same font settings as the text boxes
            this.Font = txtGameDir.Font;

            SetForm(mode);
            if (starttab.Length > 0) {
                try {
                    tabControl1.SelectedTab = tabControl1.TabPages[starttab];
                }
                catch {
                    // ignore errors
                    Debug.Assert(false);
                }
            }
            if (startprop.Length > 0) {
                try {
                    tabControl1.SelectedTab.Controls[startprop].Select();
                }
                catch {
                    // ignore errors
                    Debug.Assert(false);
                }
            }
        }

        #region Event Handlers
        #region Form and Button Event Handlers
        private void frmGameProperties_HelpRequested(object sender, HelpEventArgs hlpevent) {
            string topic = @"htm\winagi\properties.htm" + (string)((Control)sender).Tag;
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
            hlpevent.Handled = true;
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

        #endregion

        #region General Tab Event Handlers
        private void txtGameID_TextChanged(object sender, EventArgs e) {
            // during setup, form is not visible, so don't need to validate the ID
            if (!this.Visible) {
                return;
            }
            ValidateIDText();
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameID_KeyPress(object sender, KeyPressEventArgs e) {
            // allow alpha numeric only, limit of five characters

            switch ((int)e.KeyChar) {
            case 8:
                // backspace is OK
                return;
            case 13:
                // enter is same as tabbing to next control
                e.Handled = true;
                cmbVersion.Select();
                return;
            case <= 7:
            case >= 9 and <= 47:
            case >= 91 and <= 96:
            case >= 123:
                // not allowed
                e.Handled = true;
                return;
            }
            if (txtGameID.SelectionLength == 0 && txtGameID.Text.Length >= 5) {
                e.Handled = true;
            }
        }

        private void txtGameID_Validating(object sender, CancelEventArgs e) {
            // enforce character limits- 5 char max, only basic ascii,
            // no file/folder prohibited characters
            ValidateIDText();
        }

        private void cmbVersion_SelectionChangeCommitted(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameDir_DoubleClick(object sender, EventArgs e) {
            // same as clicking button
            ChangeDisplayGameDir();
        }

        private void btnGameDir_Click(object sender, EventArgs e) {
            // change/create directory that is displayed (doesn't change actual game directory)
            ChangeDisplayGameDir();
        }

        private void txtResDir_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
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

        private void chkUseLE_CheckedChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void chkResourceIDs_CheckedChanged(object sender, EventArgs e) {
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void chkUseReserved_CheckedChanged(object sender, EventArgs e) {
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void chkGlobals_CheckedChanged(object sender, EventArgs e) {
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        #endregion

        #region Version Tab Event Handlers
        private void txtGameAuthor_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtGameAuthor_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtGameDescription.Select();
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
                txtGameAbout.Select();
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
                txtGameVersion.Select();
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
                btnOK.Select();
            }
        }

        #endregion

        #region Platform Tab Event Handlers
        private void optNone_CheckedChanged(object sender, EventArgs e) {
            if (optNone.Checked) {
                // clear all platform properties
                lblPlatformFile.Enabled = false;
                btnPlatformFile.Enabled = false;
                txtPlatformFile.Enabled = false;
                txtPlatformFile.Text = "";
                lblOptions.Enabled = false;
                lblOptions.Text = "";
                lblExec.Enabled = false;
                txtExec.Enabled = false;
                txtExec.Text = "";
                NewPlatformFile = "";
                // enable ok if an ID and directory have been chosen
                btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
            }
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

        private void btnPlatformFile_Click(object sender, EventArgs e) {
            ChangePlatformApp();
        }

        private void txtExec_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtExec_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtPlatformFile.Select();
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
                btnOK.Select();
            }
        }

        private void txtPlatformFile_DoubleClick(object sender, EventArgs e) {
            ChangePlatformApp();
        }

        private void txtPlatformFile_KeyPress(object sender, KeyPressEventArgs e) {
            // enter is same as tabbing to next control
            if (e.KeyChar == 13) {
                e.Handled = true;
                txtOptions.Select();
            }
        }

        private void txtPlatformFile_TextChanged(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void txtPlatformFile_Validating(object sender, CancelEventArgs e) {
            NewPlatformFile = txtPlatformFile.Text;
        }

        #endregion

        #region Advanced Tab Event Handlers
        private void chkSierraSyntax_Click(object sender, EventArgs e) {
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        private void cmbCodePage_SelectionChangeCommitted(object sender, EventArgs e) {
            // change codepage
            try {
                NewCodePage = int.Parse(((string)cmbCodePage.SelectedItem)[..3]);
            }
            catch {
                MessageBox.Show("NOPE");
            }
            // enable ok if an ID and directory have been chosen
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        #endregion
        #endregion

        private void SetForm(GameSettingFunction mode) {
            switch (mode) {
            case GameSettingFunction.Edit:
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

                txtSrcExt.Text = EditGame.SourceExt;

                // select the button for platform
                switch (EditGame.PlatformType) {
                case Engine.PlatformType.None:
                    // None - do nothing; form is already correct
                    break;
                case Engine.PlatformType.DosBox:
                    optDosBox.Checked = true;
                    lblExec.Enabled = true;
                    btnPlatformFile.Enabled = true;
                    txtPlatformFile.Enabled = true;
                    lblOptions.Enabled = true;
                    txtOptions.Enabled = true;
                    lblExec.Enabled = true;
                    txtExec.Enabled = true;
                    break;
                case Engine.PlatformType.ScummVM:
                    optScummVM.Checked = true;
                    lblExec.Enabled = true;
                    btnPlatformFile.Enabled = true;
                    txtPlatformFile.Enabled = true;
                    lblOptions.Enabled = true;
                    txtOptions.Enabled = true;
                    break;
                case Engine.PlatformType.NAGI:
                    optNAGI.Checked = true;
                    // no options for NAGI
                    // and platform file is always "n.exe" in game dir
                    break;
                case Engine.PlatformType.Other:
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
                // include options 
                chkResourceIDs.Checked = EditGame.IncludeIDs;
                chkResDefs.Checked = EditGame.IncludeReserved;
                chkGlobals.Checked = EditGame.IncludeGlobals;

                // layout editor
                chkUseLE.Checked = EditGame.UseLE;

                // code page
                for (int i = 0; i < cmbCodePage.Items.Count; i++) {
                    if (((string)cmbCodePage.Items[i])[..3] == EditGame.CodePage.ToString()) {
                        cmbCodePage.SelectedIndex = i;
                        break;
                    }
                }
                NewCodePage = EditGame.CodePage;

                // sierra syntax option
                if (EditGame.SierraSyntax) {
                    chkSierraSyntax.Checked = true;
                }

                // set caption
                Text = "Edit Game Properties";
                break;
            case GameSettingFunction.New:
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
                txtSrcExt.Text = LogicDecoder.DefaultSrcExt;
                // include options
                chkResourceIDs.Checked = WinAGISettings.DefIncludeIDs.Value;
                chkResDefs.Checked = WinAGISettings.DefIncludeReserved.Value;
                chkGlobals.Checked = WinAGISettings.DefIncludeGlobals.Value;
                // layout editor
                chkUseLE.Checked = WinAGISettings.DefUseLE.Value;
                // platform- check for autofill platform property
                if (WinAGISettings.AutoFill.Value) {
                    switch (WinAGISettings.PlatformType.Value) {
                    case 1:
                        // DOSBox
                        optDosBox.Checked = true;
                        txtExec.Text = WinAGISettings.DOSExec.Value;
                        break;
                    case 2:
                        // ScummVM
                        optScummVM.Checked = true;
                        break;
                    case 3:
                        // NAGI
                        optNAGI.Checked = true;
                        break;
                    case 4:
                        // Other
                        optOther.Checked = true;
                        break;
                    }
                    txtPlatformFile.Text = WinAGISettings.PlatformFile.Value;
                    txtOptions.Text = WinAGISettings.PlatformOpts.Value;
                }
                // code page starts at default
                for (int i = 0; i < cmbCodePage.Items.Count; i++) {
                    if (int.Parse(((string)cmbCodePage.Items[i])[..3]) == Engine.Base.CodePage) {
                        cmbCodePage.SelectedIndex = i;
                        break;
                    }
                }
                NewCodePage = Engine.Base.CodePage;
                // sierra syntax is off by default
                chkSierraSyntax.Checked = false;

                // set caption
                Text = "New Game Properties";
                break;
            }
            // disable OK until a change is made
            btnOK.Enabled = false;
        }

        private void ChangePlatformApp() {
            // get a platform executable that will run a game
            if (NewPlatformFile.Length == 0) {
                MDIMain.OpenDlg.FileName = "";
                if (EditGame is not null) {
                    MDIMain.OpenDlg.InitialDirectory = EditGame.GameDir;
                }
                else {
                    MDIMain.OpenDlg.InitialDirectory = BrowserStartDir;
                }
            }
            else {
                MDIMain.OpenDlg.FileName = Path.GetFileName(NewPlatformFile);
                MDIMain.OpenDlg.InitialDirectory = Path.GetDirectoryName(NewPlatformFile);
            }
            MDIMain.OpenDlg.Title = "Choose Platform Application";
            MDIMain.OpenDlg.ShowReadOnly = false;
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

        private void ValidateIDText() {
            int selpos = txtGameID.SelectionStart;
            if (selpos > 5) {
                selpos = 5;
            }
            // all versions limit to five characters
            if (txtGameID.TextLength > 5) {
                txtGameID.Text = txtGameID.Text[..5];
            }
            // no file/folder prohibited characters, only standard ascii
            string idtext = "";
            foreach (char c in txtGameID.Text) {
                if (c > 32 && c < 127) {
                    // no invalid path chars
                    if (!(Path.GetInvalidFileNameChars()).Contains(c)) {
                        idtext += c;
                    }
                }
            }
            txtGameID.Text = idtext;
            if (selpos >= txtGameID.Text.Length) {
                selpos = txtGameID.Text.Length;
            }
            txtGameID.SelectionStart = selpos;
        }

        private void ChangeDisplayGameDir() {
            if (DisplayDir.Length > 0) {
                BrowserStartDir = DisplayDir;
            }
            // get a directory from which to load a game
            MDIMain.FolderDlg.Description = "Select a directory for this new game:";
            MDIMain.FolderDlg.AddToRecent = false;
            MDIMain.FolderDlg.InitialDirectory = BrowserStartDir;
            MDIMain.FolderDlg.SelectedPath = "";
            //MDIMain.FolderDlg.OkRequiresInteraction = true;
            MDIMain.FolderDlg.ShowNewFolderButton = true;

            if (MDIMain.FolderDlg.ShowDialog() == DialogResult.OK) {
                DisplayDir = FullDir(MDIMain.FolderDlg.SelectedPath);
                txtGameDir.Text = DisplayDir;
                BrowserStartDir = DisplayDir;
            }
            btnOK.Enabled = (txtGameID.TextLength > 0 && DisplayDir.Length > 0);
        }

        /*
Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
// TODO: add help 

If Shift = 0 And KeyCode = vbKeyF1 Then
'help with game properties
strTopic = @"htm\winagi\properties.htm"

switch (ActiveControl.Name) {
case "cmbVersion":
  strTopic += "#intversion";
        break;
case "picGameDir" or "txtGameDir":
  strTopic += "#gamedir"
        break;
case "picPlatformFile":
        case "txtPlatformFile":
        case "txtExec":
        case "txtOptions":
        case "optOther":
        case "optDosBox":
        case: "optScummVM":
        case "optNAGI":
  strTopic += "#executable";
        break;
case "txtGameAbout":
  strTopic += "#about";
        break;
case "txtGameAuthor":
  strTopic += "#author";
        break;
case "txtGameDescription":
  strTopic += "#description";
        break;
case "txtGameID":
  strTopic += "#gameid";
        break;
case "txtGameVersion";
  strTopic += "#gameversion";
        break;
case "txtResDir":
  strTopic += "#resdir";
        break;
case "cmbCodePage":
  strTopic += "#codepage";
        break;
}

Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, strTopic);
KeyCode = 0
End If
End Sub
*/
    }
}
