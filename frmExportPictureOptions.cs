using System;
using System.Windows.Forms;

namespace WinAGI.Editor {
    public partial class frmExportPictureOptions : Form {
        public int FormMode;

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

        #region Event Handlers
        private void OKButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            this.Visible = false;
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            // canceled..
            DialogResult = DialogResult.Cancel;
            this.Visible = false;
        }

        private void optImageFormat(object sender, EventArgs e) {
            // shrink/expand form as needed
            if (optResource.Checked) {
                this.Height = 115;
            }
            else {
                this.Height = 254;
            }
        }

        private void optImageType_CheckedChanged(object sender, EventArgs e) {
            // show priority image tip label if both is selected
            lblBoth.Visible = optBoth.Checked;
        }
        #endregion
    }
}
