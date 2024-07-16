using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor
{
    public partial class frmCompStatus : Form {
        int WindowFunction;
        internal bool CompCanceled = false;
        internal int Warnings = 0;
        internal int Errors = 0;

        public frmCompStatus(int mode) {
            InitializeComponent();
            switch (mode) {
            case 0:
                // full compile
                Text = "Compile Game";
                lblStatus.Text = "Compiling game...";
                pgbStatus.Maximum = 3 + EditGame.Logics.Count + EditGame.Pictures.Count + EditGame.Views.Count + EditGame.Sounds.Count;
                break;
            case 1:
                // rebuild vol only
                Text = "Rebuild VOL Files";
                lblStatus.Text = "Rebuilding VOL files...";
                pgbStatus.Maximum = 1 + EditGame.Logics.Count + EditGame.Pictures.Count + EditGame.Views.Count + EditGame.Sounds.Count;
                break;
            case 2:
                // dirty logics only
                Text = "Compile Dirty Logics";
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

        private void btnCancel_Click(object sender, EventArgs e) {
            // cancel the compile
            CompCanceled = true;
        }

        private void frmCompStatus_KeyDown(object sender, KeyEventArgs e) {
            // esc and enter same as clicking button
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape) {
                // cancel the compile
                CompCanceled = true;
            }
        }
    }
}
