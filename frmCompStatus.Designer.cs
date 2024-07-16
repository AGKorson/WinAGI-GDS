
namespace WinAGI.Editor {
    partial class frmCompStatus {
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
            lblStatus = new System.Windows.Forms.Label();
            pgbStatus = new System.Windows.Forms.ProgressBar();
            btnCancel = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lblErrors = new System.Windows.Forms.Label();
            lblWarnings = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoEllipsis = true;
            lblStatus.Location = new System.Drawing.Point(11, 7);
            lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(244, 19);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Adding resource: PICTURE.059";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblStatus.UseWaitCursor = true;
            // 
            // pgbStatus
            // 
            pgbStatus.Location = new System.Drawing.Point(11, 94);
            pgbStatus.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pgbStatus.Name = "pgbStatus";
            pgbStatus.Size = new System.Drawing.Size(236, 21);
            pgbStatus.TabIndex = 1;
            pgbStatus.UseWaitCursor = true;
            pgbStatus.Value = 50;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(84, 123);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(94, 25);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.UseWaitCursor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // label1
            // 
            label1.AutoEllipsis = true;
            label1.Location = new System.Drawing.Point(11, 35);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(186, 19);
            label1.TabIndex = 3;
            label1.Text = "Number of errors encountered:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label1.UseWaitCursor = true;
            // 
            // label2
            // 
            label2.AutoEllipsis = true;
            label2.Location = new System.Drawing.Point(11, 54);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(186, 19);
            label2.TabIndex = 4;
            label2.Text = "Number of warnings generated:";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label2.UseWaitCursor = true;
            // 
            // lblErrors
            // 
            lblErrors.AutoEllipsis = true;
            lblErrors.Location = new System.Drawing.Point(201, 35);
            lblErrors.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblErrors.Name = "lblErrors";
            lblErrors.Size = new System.Drawing.Size(46, 19);
            lblErrors.TabIndex = 5;
            lblErrors.Text = "0";
            lblErrors.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblErrors.UseWaitCursor = true;
            // 
            // lblWarnings
            // 
            lblWarnings.AutoEllipsis = true;
            lblWarnings.Location = new System.Drawing.Point(201, 54);
            lblWarnings.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblWarnings.Name = "lblWarnings";
            lblWarnings.Size = new System.Drawing.Size(46, 19);
            lblWarnings.TabIndex = 6;
            lblWarnings.Text = "0";
            lblWarnings.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblWarnings.UseWaitCursor = true;
            // 
            // frmCompStatus
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(262, 153);
            ControlBox = false;
            Controls.Add(lblWarnings);
            Controls.Add(lblErrors);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnCancel);
            Controls.Add(pgbStatus);
            Controls.Add(lblStatus);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            Name = "frmCompStatus";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            Text = "Rebuilding VOL files";
            TopMost = true;
            UseWaitCursor = true;
            KeyDown += frmCompStatus_KeyDown;
            ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.Label lblStatus;
        public System.Windows.Forms.ProgressBar pgbStatus;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label lblErrors;
        public System.Windows.Forms.Label lblWarnings;
    }
}