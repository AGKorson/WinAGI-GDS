using System;
using System.Windows.Forms;

namespace WinAGI.Editor {
    public partial class frmExportLogicOptions : Form {
        public int FormMode;

        public frmExportLogicOptions() {
            InitializeComponent();
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
        #endregion
    }
}
