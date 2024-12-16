using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    public partial class frmExportLogicOptions : Form {
        public int FormMode;

        public frmExportLogicOptions() {
            InitializeComponent();
        }

        #region Event Handlers
        private void OKButton_Click(object sender, EventArgs e) {
            //ok!
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
