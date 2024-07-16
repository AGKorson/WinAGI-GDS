
namespace WinAGI.Editor
{
  partial class frmProgress
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
      this.lblProgress = new System.Windows.Forms.Label();
      this.pgbStatus = new System.Windows.Forms.ProgressBar();
      this.SuspendLayout();
            // 
            // lblProgress
            // 
      this.lblProgress.AutoEllipsis = true;
      this.lblProgress.Location = new System.Drawing.Point(20, 14);
      this.lblProgress.Name = "lblProgress";
      this.lblProgress.Size = new System.Drawing.Size(454, 68);
      this.lblProgress.TabIndex = 0;
      this.lblProgress.Text = "progress...";
      this.lblProgress.UseWaitCursor = true;
            // 
            // pgbStatus
            // 
      this.pgbStatus.Location = new System.Drawing.Point(23, 89);
      this.pgbStatus.Name = "pgbStatus";
      this.pgbStatus.Size = new System.Drawing.Size(439, 44);
      this.pgbStatus.TabIndex = 1;
      this.pgbStatus.UseWaitCursor = true;
      this.pgbStatus.Value = 50;
            // 
            // frmProgress
            // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(487, 189);
      this.ControlBox = false;
      this.Controls.Add(this.pgbStatus);
      this.Controls.Add(this.lblProgress);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "frmProgress";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "progress";
      this.TopMost = true;
      this.UseWaitCursor = true;
      this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblProgress;
    public System.Windows.Forms.ProgressBar pgbStatus;
    }
}