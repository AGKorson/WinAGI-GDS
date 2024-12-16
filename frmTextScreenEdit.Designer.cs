namespace WinAGI.Editor {
    partial class frmTextScreenEdit {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTextScreenEdit));
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            spStatus = new System.Windows.Forms.ToolStripStatusLabel();
            spScale = new System.Windows.Forms.ToolStripStatusLabel();
            spMode = new System.Windows.Forms.ToolStripStatusLabel();
            spTool = new System.Windows.Forms.ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { spScale, spMode, spTool, spStatus });
            statusStrip1.Location = new System.Drawing.Point(41, 214);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            statusStrip1.Size = new System.Drawing.Size(719, 23);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.Visible = false;
            // 
            // spStatus
            // 
            spStatus.MergeAction = System.Windows.Forms.MergeAction.Replace;
            spStatus.MergeIndex = 3;
            spStatus.Name = "spStatus";
            spStatus.Size = new System.Drawing.Size(521, 18);
            spStatus.Spring = true;
            spStatus.Text = "text screen status";
            spStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spScale
            // 
            spScale.AutoSize = false;
            spScale.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spScale.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spScale.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spScale.MergeIndex = 0;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(70, 18);
            spScale.Text = "txt scale";
            // 
            // spMode
            // 
            spMode.AutoSize = false;
            spMode.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spMode.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spMode.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spMode.MergeIndex = 1;
            spMode.Name = "spMode";
            spMode.Size = new System.Drawing.Size(70, 18);
            spMode.Text = "txt mode";
            // 
            // spTool
            // 
            spTool.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spTool.MergeIndex = 2;
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(44, 18);
            spTool.Text = "txt tool";
            // 
            // frmTextScreenEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmTextScreenEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmTextScreenEdit";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel spStatus;
        private System.Windows.Forms.ToolStripStatusLabel spScale;
        private System.Windows.Forms.ToolStripStatusLabel spMode;
        private System.Windows.Forms.ToolStripStatusLabel spTool;
    }
}