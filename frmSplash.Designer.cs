
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
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSplash));
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.lblVersion = new System.Windows.Forms.Label();
      this.lblCopyright = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Arial", 32F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label1.Location = new System.Drawing.Point(243, 47);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(349, 99);
      this.label1.TabIndex = 1;
      this.label1.Text = "WinAGI";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.label2.Location = new System.Drawing.Point(282, 188);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(658, 56);
      this.label2.TabIndex = 2;
      this.label2.Text = "Game Development System";
      // 
      // lblVersion
      // 
      this.lblVersion.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
      this.lblVersion.Location = new System.Drawing.Point(264, 363);
      this.lblVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblVersion.Name = "lblVersion";
      this.lblVersion.Size = new System.Drawing.Size(576, 41);
      this.lblVersion.TabIndex = 3;
      this.lblVersion.Text = "Version";
      this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // lblCopyright
      // 
      this.lblCopyright.AutoSize = true;
      this.lblCopyright.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.lblCopyright.Location = new System.Drawing.Point(540, 465);
      this.lblCopyright.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblCopyright.Name = "lblCopyright";
      this.lblCopyright.Size = new System.Drawing.Size(159, 25);
      this.lblCopyright.TabIndex = 4;
      this.lblCopyright.Text = "Copyright 2021";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.label5.Location = new System.Drawing.Point(540, 495);
      this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(160, 25);
      this.label5.TabIndex = 5;
      this.label5.Text = "Andrew Korson";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.pictureBox1);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.lblCopyright);
      this.groupBox1.Controls.Add(this.lblVersion);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(19, 9);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(6);
      this.groupBox1.Size = new System.Drawing.Size(877, 576);
      this.groupBox1.TabIndex = 6;
      this.groupBox1.TabStop = false;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(36, 104);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(215, 215);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 6;
      this.pictureBox1.TabStop = false;
      // 
      // frmSplash
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(887, 575);
      this.ControlBox = false;
      this.Controls.Add(this.label2);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Margin = new System.Windows.Forms.Padding(6);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmSplash";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label lblVersion;
    private System.Windows.Forms.Label lblCopyright;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}