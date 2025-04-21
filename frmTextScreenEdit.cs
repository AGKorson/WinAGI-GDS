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
        // StatusStrip Items
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spMode;
        internal ToolStripStatusLabel spTool;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;

        public frmTextScreenEdit() {
            InitializeComponent();
            InitStatusStrip();
        }

        private void InitStatusStrip() {
            spScale = new ToolStripStatusLabel();
            spMode = new ToolStripStatusLabel();
            spTool = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
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
            // spTool
            // 
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(44, 18);
            spTool.Text = "txt tool";
        }
    }
}
