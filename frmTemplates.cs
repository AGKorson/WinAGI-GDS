using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmTemplates : Form {
        private string[] strVersions = new string[19];
        public int CodePage;
        public bool UseResDef, UseLayoutEd, SierraSyntax;

        public frmTemplates() {
            InitializeComponent();

            // load versions
            strVersions[0] = "2.089";
            strVersions[1] = "2.272";
            strVersions[2] = "2.411";
            strVersions[3] = "2.425";
            strVersions[4] = "2.426";
            strVersions[5] = "2.435";
            strVersions[6] = "2.439";
            strVersions[7] = "2.440";
            strVersions[8] = "2.903";
            strVersions[9] = "2.911";
            strVersions[10] = "2.912";
            strVersions[11] = "2.915";
            strVersions[12] = "2.917";
            strVersions[13] = "2.936";
            strVersions[14] = "3.002086";
            strVersions[15] = "3.002098";
            strVersions[16] = "3.002102";
            strVersions[17] = "3.002107";
            strVersions[18] = "3.002149";

            // step through all subdirectories in the templates directory
            // if the directory contains a .wag file,
            // get the description and version

            foreach (string tmpdir in Directory.GetDirectories(Application.StartupPath + "Templates")) {
                // add if it has exactly one wag file
                if (Directory.GetFiles(tmpdir, "*.wag").Length == 1) {
                    lstTemplates.Items.Add(Path.GetFileName(tmpdir));
                }
            }
        }

        #region Event Handlers
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
            // extract codepage, game version and game description from the
            // .wag file located in the template directory currently selected
            // in lstTemplates
            string strValue;
            SettingsFile WagFile;
            string strVersion, strDescription;

            // clear description and version text boxes
            txtVersion.Text = "";
            txtDescription.Text = "";
            // check for game file
            try {
                WagFile = new SettingsFile(Directory.GetFiles(Application.StartupPath + "\\Templates\\" + lstTemplates.Text, "*.wag")[0], FileMode.Open);
            }
            catch (Exception e) {
                // problem accessing the template; assume not valid
                ErrMsgBox(e, "An error occurred trying to validate this template directory: ",
                    "", "Invalid Template Directory");
                //MessageBox.Show(MDIMain,
                //    "Error occurred trying to validate this template directory.",
                //    "Invalid Template Directory", MessageBoxButtons.OK, MessageBoxIcon.Error,
                //    0, 0, WinAGIHelp, "htm\\winagi\\Templates.htm");
                RemoveBadTemplate();
                return;
            }
            // check for readonly (not allowed)
            if ((File.GetAttributes(WagFile.Filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                MessageBox.Show(MDIMain,
                    "Template wag file is marked 'readonly'.",
                    "Invalid Template Directory",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    0, 0, WinAGIHelp, "htm\\winagi\\Templates.htm");
                RemoveBadTemplate();
                return;
            }
            // verify WinAGI version
            strValue = WagFile.GetSetting("General", "WinAGIVersion", "");
            if (strValue != WINAGI_VERSION) {
                if (strValue.Left(4) == "1.2." || (strValue.Left(2) == "2.")) {
                    // any v1.2.x or 2.x is ok, will get updated
                    // when game is created
                }
                else {
                    // not valid
                    MessageBox.Show(MDIMain,
                        "The WinAGI game file is corrupt.\nThis is not a valid template directory.",
                        "Invalid Template Directory",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        0, 0, WinAGIHelp, "htm\\winagi\\Templates.htm");
                    RemoveBadTemplate();
                    return;
                }
            }
            strVersion = WagFile.GetSetting("General", "Interpreter", "", true);
            strDescription = WagFile.GetSetting("General", "Description", "", true);
            CodePage = WagFile.GetSetting("General", "CodePage", 437, true);
            UseResDef = WagFile.GetSetting("General", "UseResNames", true, true);
            UseLayoutEd = WagFile.GetSetting("General", "UseLE", true, true);
            SierraSyntax = WagFile.GetSetting("General", "SierraSyntax", false, true);

            // version is NOT optional
            if (strVersion.Length == 0) {
                MessageBox.Show(MDIMain,
                    "The interpreter version for this game is missing.\nThis is not a valid template directory.",
                    "Invalid Template Directory",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    0, 0, WinAGIHelp, "htm\\winagi\\Templates.htm");
                RemoveBadTemplate();
                return;
            }
            if (strVersions.Contains(strVersion)) {
                txtVersion.Text = strVersion;
                txtDescription.Text = strDescription;
                btnOK.Enabled = true;
                return;
            }
            else {
                MessageBox.Show(MDIMain,
                    "The interpreter version for this game does not match known Sierra versions.\nThis is not a valid template directory.",
                    "Invalid Template Directory",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    0, 0, WinAGIHelp, "htm\\winagi\\Templates.htm");
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
