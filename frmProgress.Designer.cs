
namespace WinAGI.Editor {
    partial class frmProgress {
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
            lblProgress = new System.Windows.Forms.Label();
            pgbStatus = new System.Windows.Forms.ProgressBar();
            SuspendLayout();
            // 
            // lblProgress
            // 
            lblProgress.AutoEllipsis = true;
            lblProgress.Location = new System.Drawing.Point(11, 7);
            lblProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new System.Drawing.Size(244, 32);
            lblProgress.TabIndex = 0;
            lblProgress.Text = "progress...";
            lblProgress.UseWaitCursor = true;
            // 
            // pgbStatus
            // 
            pgbStatus.Location = new System.Drawing.Point(12, 42);
            pgbStatus.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pgbStatus.Name = "pgbStatus";
            pgbStatus.Size = new System.Drawing.Size(236, 21);
            pgbStatus.TabIndex = 1;
            pgbStatus.UseWaitCursor = true;
            pgbStatus.Value = 50;
            // 
            // frmProgress
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(262, 89);
            ControlBox = false;
            Controls.Add(pgbStatus);
            Controls.Add(lblProgress);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            Name = "frmProgress";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "progress";
            TopMost = true;
            UseWaitCursor = true;
            ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.Label lblProgress;
        public System.Windows.Forms.ProgressBar pgbStatus;
    }
}