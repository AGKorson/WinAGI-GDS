
namespace WinAGI.Editor {
    partial class frmLayout {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLayout));
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            spStatus = new System.Windows.Forms.ToolStripStatusLabel();
            spCurX = new System.Windows.Forms.ToolStripStatusLabel();
            spCurY = new System.Windows.Forms.ToolStripStatusLabel();
            spScale = new System.Windows.Forms.ToolStripStatusLabel();
            spTool = new System.Windows.Forms.ToolStripStatusLabel();
            spID = new System.Windows.Forms.ToolStripStatusLabel();
            spType = new System.Windows.Forms.ToolStripStatusLabel();
            spRoom1 = new System.Windows.Forms.ToolStripStatusLabel();
            spRoom2 = new System.Windows.Forms.ToolStripStatusLabel();
            spCapsLock = new System.Windows.Forms.ToolStripStatusLabel();
            spNumLock = new System.Windows.Forms.ToolStripStatusLabel();
            spInsLock = new System.Windows.Forms.ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { spScale, spTool, spID, spType, spRoom1, spRoom2, spStatus, spCurX, spCurY, spCapsLock, spNumLock, spInsLock });
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
            spStatus.MergeIndex = 6;
            spStatus.Name = "spStatus";
            spStatus.Size = new System.Drawing.Size(158, 18);
            spStatus.Spring = true;
            spStatus.Text = "layout status";
            spStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spCurX
            // 
            spCurX.AutoSize = false;
            spCurX.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spCurX.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spCurX.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spCurX.MergeIndex = 7;
            spCurX.Name = "spCurX";
            spCurX.Size = new System.Drawing.Size(70, 18);
            spCurX.Text = "layoutX";
            // 
            // spCurY
            // 
            spCurY.AutoSize = false;
            spCurY.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spCurY.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spCurY.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spCurY.MergeIndex = 8;
            spCurY.Name = "spCurY";
            spCurY.Size = new System.Drawing.Size(70, 18);
            spCurY.Text = "layoutY";
            // 
            // spScale
            // 
            spScale.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spScale.MergeIndex = 0;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(66, 18);
            spScale.Text = "layoutscale";
            // 
            // spTool
            // 
            spTool.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spTool.MergeIndex = 1;
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(61, 18);
            spTool.Text = "layouttool";
            // 
            // spID
            // 
            spID.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spID.MergeIndex = 2;
            spID.Name = "spID";
            spID.Size = new System.Drawing.Size(51, 18);
            spID.Text = "layoutID";
            // 
            // spType
            // 
            spType.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spType.MergeIndex = 3;
            spType.Name = "spType";
            spType.Size = new System.Drawing.Size(63, 18);
            spType.Text = "layouttype";
            // 
            // spRoom1
            // 
            spRoom1.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spRoom1.MergeIndex = 4;
            spRoom1.Name = "spRoom1";
            spRoom1.Size = new System.Drawing.Size(78, 18);
            spRoom1.Text = "layoutRoom1";
            // 
            // spRoom2
            // 
            spRoom2.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spRoom2.MergeIndex = 5;
            spRoom2.Name = "spRoom2";
            spRoom2.Size = new System.Drawing.Size(78, 18);
            spRoom2.Text = "layoutRoom2";
            // 
            // spCapsLock
            // 
            spCapsLock.MergeAction = System.Windows.Forms.MergeAction.Remove;
            spCapsLock.MergeIndex = 9;
            spCapsLock.Name = "spCapsLock";
            spCapsLock.Size = new System.Drawing.Size(0, 18);
            // 
            // spNumLock
            // 
            spNumLock.MergeAction = System.Windows.Forms.MergeAction.Remove;
            spNumLock.MergeIndex = 9;
            spNumLock.Name = "spNumLock";
            spNumLock.Size = new System.Drawing.Size(0, 18);
            // 
            // spInsLock
            // 
            spInsLock.MergeAction = System.Windows.Forms.MergeAction.Remove;
            spInsLock.MergeIndex = 9;
            spInsLock.Name = "spInsLock";
            spInsLock.Size = new System.Drawing.Size(10, 18);
            spInsLock.Text = " ";
            // 
            // frmLayout
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmLayout";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmLayout";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel spStatus;
        private System.Windows.Forms.ToolStripStatusLabel spCurX;
        private System.Windows.Forms.ToolStripStatusLabel spCurY;
        private System.Windows.Forms.ToolStripStatusLabel spScale;
        private System.Windows.Forms.ToolStripStatusLabel spTool;
        private System.Windows.Forms.ToolStripStatusLabel spID;
        private System.Windows.Forms.ToolStripStatusLabel spType;
        private System.Windows.Forms.ToolStripStatusLabel spRoom1;
        private System.Windows.Forms.ToolStripStatusLabel spRoom2;
        private System.Windows.Forms.ToolStripStatusLabel spCapsLock;
        private System.Windows.Forms.ToolStripStatusLabel spNumLock;
        private System.Windows.Forms.ToolStripStatusLabel spInsLock;
    }
}