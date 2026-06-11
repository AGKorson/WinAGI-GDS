using System;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmCompStatus : Form {
        #region Fields
        private readonly CompileMode WindowFunction;
        internal bool CompCanceled;
        internal int Warnings;
        internal int Errors;
        public WinAGIEventInfo FatalError = new();
        #endregion

        #region Constructors
        public frmCompStatus(CompileMode mode) {
            InitializeComponent();
            CompCanceled = false;
            switch (mode) {
            case CompileMode.Full:
                // full compile
                Text = "Compile Game";
                lblStatus.Text = "Compiling game...";
                pgbStatus.Maximum = 3 + EditGame.Logics.Count +
                    EditGame.Pictures.Count +
                    EditGame.Views.Count +
                    EditGame.Sounds.Count;
                break;
            case CompileMode.RebuildOnly:
                // rebuild vol only
                Text = "Rebuild VOL Files";
                lblStatus.Text = "Rebuilding VOL files...";
                pgbStatus.Maximum = 1 + EditGame.Logics.Count +
                    EditGame.Pictures.Count +
                    EditGame.Views.Count +
                    EditGame.Sounds.Count;
                break;
            case CompileMode.ChangedLogics:
                // changed logics only
                Text = "Compile Changed Logics";
                lblStatus.Text = "Compiling Logics";
                pgbStatus.Maximum = 1 + EditGame.Logics.Count;
                // no cancel button
                btnCancel.Visible = false;
                break;
            case CompileMode.SingleLogic:
                // single logic only
                Text = "Compile Logic";
                lblStatus.Text = "Compiling ...";
                pgbStatus.Maximum = 1;
                // no cancel button
                btnCancel.Visible = false;
                break;
            }
            pgbStatus.Value = 0;
            lblErrors.Text = "0";
            lblWarnings.Text = "0";
            WindowFunction = mode;
        }
        #endregion

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
