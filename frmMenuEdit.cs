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

namespace WinAGI.Editor {
    public partial class frmMenuEdit : Form {

        // ToolStrip Items
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        public frmMenuEdit() {
            InitializeComponent();
            InitToolStrip();
            MdiParent = MDIMain;
        }

        #region Event Handlers
        #endregion

        #region Methods
        private void InitToolStrip() {
            spStatus = MDIMain.spStatus;
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;
        }
        #endregion
    }
}
