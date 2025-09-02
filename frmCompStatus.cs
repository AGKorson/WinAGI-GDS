using System;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmCompStatus : Form {
        CompileMode WindowFunction;
        internal bool CompCanceled = false;
        internal int Warnings = 0;
        internal int Errors = 0;
        public TWinAGIEventInfo FatalError = new();

        public frmCompStatus(CompileMode mode) {
            InitializeComponent();
            switch (mode) {
            case CompileMode.Full:
                // full compile
                Text = "Compile Game";
                lblStatus.Text = "Compiling game...";
                pgbStatus.Maximum = 3 + EditGame.Logics.Count + EditGame.Pictures.Count + EditGame.Views.Count + EditGame.Sounds.Count;
                break;
            case CompileMode.RebuildOnly:
                // rebuild vol only
                Text = "Rebuild VOL Files";
                lblStatus.Text = "Rebuilding VOL files...";
                pgbStatus.Maximum = 1 + EditGame.Logics.Count + EditGame.Pictures.Count + EditGame.Views.Count + EditGame.Sounds.Count;
                break;
            case CompileMode.ChangedLogics:
                // changed logics only
                Text = "Compile Changed Logics";
                lblStatus.Text = "Compiling Logics";
                pgbStatus.Maximum = 1 + EditGame.Logics.Count;
                // no cancel button
                btnCancel.Visible = false;
                break;
            }
            pgbStatus.Value = 0;
            lblErrors.Text = "0";
            lblWarnings.Text = "0";
            WindowFunction = mode;
        }

        #region Event Handlers
        private void frmCompStatus_KeyDown(object sender, KeyEventArgs e) {
            // esc and enter same as clicking button
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape) {
                CompCanceled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            CompCanceled = true;
        }
        #endregion
    }
}
