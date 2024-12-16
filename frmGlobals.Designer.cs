
namespace WinAGI.Editor {
    partial class frmGlobals {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGlobals));
            lstGlobals = new System.Windows.Forms.ListBox();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            spStatus = new System.Windows.Forms.ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lstGlobals
            // 
            lstGlobals.Dock = System.Windows.Forms.DockStyle.Fill;
            lstGlobals.FormattingEnabled = true;
            lstGlobals.ItemHeight = 15;
            lstGlobals.Location = new System.Drawing.Point(0, 0);
            lstGlobals.Name = "lstGlobals";
            lstGlobals.Size = new System.Drawing.Size(800, 450);
            lstGlobals.TabIndex = 0;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { spStatus });
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
            spStatus.MergeIndex = 0;
            spStatus.Name = "spStatus";
            spStatus.Size = new System.Drawing.Size(565, 18);
            spStatus.Spring = true;
            spStatus.Text = "globals edit status";
            spStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmGlobals
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(statusStrip1);
            Controls.Add(lstGlobals);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmGlobals";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmGlobals";
            FormClosing += frmGlobals_FormClosing;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox lstGlobals;
        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel spStatus;
    }
}