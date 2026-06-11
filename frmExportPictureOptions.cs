using System;
using System.Windows.Forms;

namespace WinAGI.Editor {
    public partial class frmExportPictureOptions : Form {
        #region Fields
        public int FormMode;
        #endregion

        #region Constructors
        public frmExportPictureOptions(int mode) {
            // mode = 0: allow choice of export
            // mode = 1: force image export only
            InitializeComponent();
            // default is bitmap
            cmbFormat.SelectedIndex = 0;
            FormMode = mode;
            switch (FormMode) {
            case 0:
                // allow choice
                fraChoice.Visible = true;
                optResource.Checked = true;
                Height = 115;
                break;
            case 1:
                // image only
                fraChoice.Visible = false;
                fraImage.Top -= 58;
                lblFormat.Top -= 58;
                cmbFormat.Top -= 58;
                lblScale.Top -= 58;
                udZoom.Top -= 58;
                lblBoth.Top -= 58;
                optImage.Checked = true;
                Height = 196;
                break;
            }
        }
        #endregion

        #region Event Handlers
        private void frmExportPictureOptions_HelpRequested(object sender, HelpEventArgs hlpevent) {
            Help.ShowHelp(Base.HelpParent, Base.WinAGIHelp, "htm\\winagi\\resource_export.htm#exportpics");
            hlpevent.Handled = true;
        }

        private void OKButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Visible = false;
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Visible = false;
        }

        private void optImageFormat(object sender, EventArgs e) {
            // shrink/expand form as needed
            if (optResource.Checked) {
                Height = 115;
            }
            else {
                Height = 254;
            }
        }

        private void optImageType_CheckedChanged(object sender, EventArgs e) {
            // show priority image tip label if both is selected
            lblBoth.Visible = optBoth.Checked;
        }
        #endregion
    }
}
