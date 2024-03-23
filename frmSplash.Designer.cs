
namespace WinAGI.Editor
{
  partial class frmSplash
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSplash));
            pictureBox1 = new System.Windows.Forms.PictureBox();
            label5 = new System.Windows.Forms.Label();
            lblCopyright = new System.Windows.Forms.Label();
            lblVersion = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new System.Drawing.Point(6, 7);
            pictureBox1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(437, 161);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 18;
            pictureBox1.TabStop = false;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Arial", 8.25F);
            label5.Location = new System.Drawing.Point(8, 203);
            label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(85, 14);
            label5.TabIndex = 17;
            label5.Text = "Andrew Korson";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCopyright
            // 
            lblCopyright.Font = new System.Drawing.Font("Arial", 8.25F);
            lblCopyright.Location = new System.Drawing.Point(8, 189);
            lblCopyright.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new System.Drawing.Size(111, 14);
            lblCopyright.TabIndex = 16;
            lblCopyright.Text = "Copyright ";
            lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVersion
            // 
            lblVersion.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            lblVersion.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblVersion.Location = new System.Drawing.Point(143, 194);
            lblVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new System.Drawing.Size(300, 23);
            lblVersion.TabIndex = 15;
            lblVersion.Text = "Version";
            lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // frmSplash
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(451, 223);
            ControlBox = false;
            Controls.Add(pictureBox1);
            Controls.Add(label5);
            Controls.Add(lblCopyright);
            Controls.Add(lblVersion);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmSplash";
            Padding = new System.Windows.Forms.Padding(6);
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblVersion;
    }
}