using System;
using System.Windows.Forms;

namespace WinAGI.Editor {
    public partial class frmExportLogicOptions : Form {
        #region Fields
        public int FormMode;
        #endregion

        #region Constructors
        public frmExportLogicOptions() {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void OKButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Visible = false;
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            // canceled..
            DialogResult = DialogResult.Cancel;
            Visible = false;
        }

        private void frmExportLogicOptions_HelpRequested(object sender, HelpEventArgs hlpevent) {
            Help.ShowHelp(Base.HelpParent, Base.WinAGIHelp, "htm\\winagi\\resource_export.htm#exportlogics");
            hlpevent.Handled = true;
        }
        #endregion
    }
}
