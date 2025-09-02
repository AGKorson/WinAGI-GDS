using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {

    public partial class frmTextScreenEdit : Form {
        #region Members
        public bool IsChanged;

        // StatusStrip Items
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spMode;
        internal ToolStripStatusLabel spRow;
        internal ToolStripStatusLabel spCol;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;
        #endregion

        public frmTextScreenEdit() {
            InitializeComponent();
            InitStatusStrip();
        }

        #region Event Handlers
        internal void SetResourceMenu() {
        }

        internal void ResetResourceMenu() {
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        internal void mnuRSaveAs_Click(object sender, EventArgs e) {
            throw new NotImplementedException();
        }
        #endregion

        #region Methods
        private void InitStatusStrip() {
            spScale = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
            spMode = new ToolStripStatusLabel();
            spRow = new ToolStripStatusLabel();
            spCol = new ToolStripStatusLabel();
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;

            // 
            // spScale
            // 
            spScale.AutoSize = false;
            spScale.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spScale.BorderStyle = Border3DStyle.SunkenInner;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(70, 18);
            spScale.Text = "txt scale";
            // 
            // spMode
            // 
            spMode.AutoSize = false;
            spMode.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spMode.BorderStyle = Border3DStyle.SunkenInner;
            spMode.Name = "spMode";
            spMode.Size = new System.Drawing.Size(70, 18);
            spMode.Text = "txt mode";
            // 
            // spRow
            // 
            spRow.Name = "spRow";
            spRow.Size = new System.Drawing.Size(44, 18);
            spRow.Text = "row";
            // 
            // spCol
            // 
            spRow.Name = "spCol";
            spRow.Size = new System.Drawing.Size(44, 18);
            spRow.Text = "col";
        }

        internal void ShowHelp() {
            string topic = "htm\\winagi\\textscreen.htm";

            // TODO: add context sensitive help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }
        #endregion
    }
}
