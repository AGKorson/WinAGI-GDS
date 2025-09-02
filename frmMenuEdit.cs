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

        #region Members
        public bool IsChanged;

        // ToolStrip Items
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        #endregion

        public frmMenuEdit() {
            InitializeComponent();
            InitToolStrip();
            MdiParent = MDIMain;
        }

        #region Event Handlers
        internal void SetResourceMenu() {
        }

        internal void ResetResourceMenu() {
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
        #endregion

        #region Methods
        private void InitToolStrip() {
            spStatus = MDIMain.spStatus;
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;
        }

        internal void ShowHelp() {
            string topic = "htm\\winagi\\Menu_Editor.htm";

            // TODO: add context sensitive help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }
        #endregion
    }
}
