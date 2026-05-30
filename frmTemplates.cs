using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinAGI.Common;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    public partial class frmTemplates : Form {
        public int CodePage;
        public bool IncludeReserved, UseLayoutEd, SierraSyntax;
        public bool IncludeIDs, IncludeGlobals;
        public frmTemplates() {
            InitializeComponent();

            // step through all subdirectories in the templates directory
            // if the directory contains a .wag file,
            // get the description and version

            foreach (string tmpdir in Directory.GetDirectories(Path.Combine(AppDataDir, "Templates"))) {
                // add if it has exactly one wag file
                if (Directory.GetFiles(tmpdir, "*.wag").Length == 1) {
                    lstTemplates.Items.Add(Path.GetFileName(tmpdir));
                }
            }
        }

        #region Event Handlers
        private void frmTemplates_HelpRequested(object sender, HelpEventArgs hlpevent) {
            string topic = @"htm\winagi\templates.htm";
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
            hlpevent.Handled = true;
        }

        private void btnOK_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void lstTemplates_SelectedIndexChanged(object sender, EventArgs e) {
            if (lstTemplates.SelectedIndex >= 0) {
                GetTemplateInfo();
            }
        }
        #endregion

        private void GetTemplateInfo() {
            // extract codepage, WinAGI version and game description from the
            // .wag file located in the template directory currently selected
            // in lstTemplates
            string propvalue;
            SettingsFile WagFile;
            string version, description;

            // clear description and version text boxes
            txtVersion.Text = "";
            txtDescription.Text = "";
            // check for game file
            try {
                WagFile = new SettingsFile(Directory.GetFiles(Path.Combine(AppDataDir, "Templates", lstTemplates.Text), "*.wag")[0], FileMode.Open);
            }
            catch (Exception ex) {
                // problem accessing the template; assume not valid
                ErrMsgBox(ex,
                    "An error occurred trying to validate this template directory: ",
                    ex.StackTrace,
                    "Invalid Template Directory");
                RemoveBadTemplate();
                return;
            }
            // check for readonly (not allowed)
            if ((File.GetAttributes(WagFile.Filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                MDIMain.MsgBoxWithHelp(
                    "Template wag file is marked 'readonly'.",
                    "Invalid Template Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    "htm\\winagi\\templates.htm");
                RemoveBadTemplate();
                return;
            }
            // verify WinAGI version
            propvalue = WagFile.GetSetting("General", "WinAGIVersion", "");
            if (propvalue != WINAGI_VERSION) {
                if (propvalue.Left(4) == "1.2." || (propvalue.Left(2) == "2.")) {
                    // v1.2.x or 2.x is ok, need to be updated first
                    MDIMain.MsgBoxWithHelp(
                        "The WinAGI game file is from an earlier version of WinAGI and must be upgraded.",
                        "Unsupported Template Game Version",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\winagi\\templates.htm#upgrade");
                }
                else {
                    // not valid
                    MDIMain.MsgBoxWithHelp(
                        "The WinAGI game file is corrupt.\nThis is not a valid template directory.",
                        "Invalid Template Directory",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\winagi\\templates.htm");
                }
                RemoveBadTemplate();
                return;
            }
            version = WagFile.GetSetting("General", "Interpreter", "", true);
            description = WagFile.GetSetting("General", "Description", "", true);
            CodePage = WagFile.GetSetting("General", "CodePage", 437, true);
            IncludeIDs = WagFile.GetSetting("Includes", "IncludeIDs", true, true);
            IncludeReserved = WagFile.GetSetting("Includes", "IncludeReserved", true, true);
            IncludeGlobals = WagFile.GetSetting("Includes", "IncludeGlobals", true, true);
            UseLayoutEd = WagFile.GetSetting("General", "UseLE", true, true);
            SierraSyntax = WagFile.GetSetting("General", "SierraSyntax", false, true);

            // version is NOT optional
            if (version.Length == 0) {
                MessageBox.Show(MDIMain,
                    "The interpreter version for this game is missing.\nThis is not a valid template directory.",
                    "Invalid Template Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                RemoveBadTemplate();
                return;
            }
            if (IntVersions.Contains(version)) {
                txtVersion.Text = version;
                txtDescription.Text = description;
                btnOK.Enabled = true;
                return;
            }
            else {
                MDIMain.MsgBoxWithHelp(
                    "The interpreter version for this game does not match known Sierra versions.\nThis is not a valid template directory.",
                    "Invalid Template Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    "htm\\agi\\versions.htm");
                RemoveBadTemplate();
            }

        }

        private void RemoveBadTemplate() {
            // removes a bad template from the list box
            lstTemplates.Items.Remove(lstTemplates.SelectedIndex);
            lstTemplates.SelectedIndex = -1;
            btnOK.Enabled = false;
        }
    }
}
