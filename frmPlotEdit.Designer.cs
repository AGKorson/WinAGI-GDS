namespace WinAGI.Editor {
    partial class frmPlotEdit {
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
            components = new System.ComponentModel.Container();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            picPlot = new System.Windows.Forms.PictureBox();
            lblX = new System.Windows.Forms.Label();
            lblY = new System.Windows.Forms.Label();
            txtX = new System.Windows.Forms.TextBox();
            txtY = new System.Windows.Forms.TextBox();
            udPattern = new System.Windows.Forms.NumericUpDown();
            lblPattern = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            lblWarning = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)picPlot).BeginInit();
            ((System.ComponentModel.ISupportInitialize)udPattern).BeginInit();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(192, 9);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(72, 26);
            btnOK.TabIndex = 6;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(192, 41);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(72, 26);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // picPlot
            // 
            picPlot.Location = new System.Drawing.Point(14, 60);
            picPlot.Name = "picPlot";
            picPlot.Size = new System.Drawing.Size(150, 145);
            picPlot.TabIndex = 2;
            picPlot.TabStop = false;
            picPlot.Paint += picPlot_Paint;
            // 
            // lblX
            // 
            lblX.AutoSize = true;
            lblX.Font = new System.Drawing.Font("Segoe UI", 14F);
            lblX.Location = new System.Drawing.Point(2, 22);
            lblX.Name = "lblX";
            lblX.Size = new System.Drawing.Size(27, 25);
            lblX.TabIndex = 0;
            lblX.Text = "X:";
            // 
            // lblY
            // 
            lblY.AutoSize = true;
            lblY.Font = new System.Drawing.Font("Segoe UI", 14F);
            lblY.Location = new System.Drawing.Point(93, 22);
            lblY.Name = "lblY";
            lblY.Size = new System.Drawing.Size(27, 25);
            lblY.TabIndex = 2;
            lblY.Text = "Y:";
            // 
            // txtX
            // 
            txtX.Font = new System.Drawing.Font("Segoe UI", 14F);
            txtX.Location = new System.Drawing.Point(26, 19);
            txtX.Name = "txtX";
            txtX.Size = new System.Drawing.Size(55, 32);
            txtX.TabIndex = 1;
            txtX.Text = "159";
            txtX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtX.TextChanged += txtX_TextChanged;
            txtX.Enter += txtX_Enter;
            txtX.KeyPress += txtX_KeyPress;
            txtX.Leave += txtX_Leave;
            txtX.Validating += txtX_Validating;
            // 
            // txtY
            // 
            txtY.Font = new System.Drawing.Font("Segoe UI", 14F);
            txtY.Location = new System.Drawing.Point(119, 19);
            txtY.Name = "txtY";
            txtY.Size = new System.Drawing.Size(55, 32);
            txtY.TabIndex = 3;
            txtY.Text = "159";
            txtY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtY.TextChanged += txtY_TextChanged;
            txtY.Enter += txtY_Enter;
            txtY.KeyPress += txtY_KeyPress;
            txtY.Leave += txtY_Leave;
            txtY.Validating += txtY_Validating;
            // 
            // udPattern
            // 
            udPattern.Font = new System.Drawing.Font("Segoe UI", 14F);
            udPattern.Location = new System.Drawing.Point(189, 126);
            udPattern.Maximum = new decimal(new int[] { 119, 0, 0, 0 });
            udPattern.Name = "udPattern";
            udPattern.Size = new System.Drawing.Size(60, 32);
            udPattern.TabIndex = 5;
            udPattern.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            udPattern.Value = new decimal(new int[] { 119, 0, 0, 0 });
            udPattern.ValueChanged += udPattern_ValueChanged;
            udPattern.Enter += udPattern_Enter;
            udPattern.Leave += udPattern_Leave;
            // 
            // lblPattern
            // 
            lblPattern.AutoSize = true;
            lblPattern.Location = new System.Drawing.Point(173, 108);
            lblPattern.Name = "lblPattern";
            lblPattern.Size = new System.Drawing.Size(92, 15);
            lblPattern.TabIndex = 4;
            lblPattern.Text = "Pattern Number";
            // 
            // timer1
            // 
            timer1.Interval = 10;
            timer1.Tick += timer1_Tick;
            // 
            // lblWarning
            // 
            lblWarning.BackColor = System.Drawing.Color.White;
            lblWarning.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lblWarning.ForeColor = System.Drawing.Color.Red;
            lblWarning.Location = new System.Drawing.Point(181, 168);
            lblWarning.Name = "lblWarning";
            lblWarning.Size = new System.Drawing.Size(74, 32);
            lblWarning.TabIndex = 8;
            lblWarning.Text = "RIGHT EDGE OVERLAP";
            lblWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblWarning.Visible = false;
            // 
            // frmPlotEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(271, 216);
            Controls.Add(lblWarning);
            Controls.Add(lblPattern);
            Controls.Add(udPattern);
            Controls.Add(txtY);
            Controls.Add(txtX);
            Controls.Add(lblY);
            Controls.Add(lblX);
            Controls.Add(picPlot);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPlotEdit";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Edit Plot Point";
            ((System.ComponentModel.ISupportInitialize)picPlot).EndInit();
            ((System.ComponentModel.ISupportInitialize)udPattern).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox picPlot;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.TextBox txtX;
        private System.Windows.Forms.TextBox txtY;
        private System.Windows.Forms.NumericUpDown udPattern;
        private System.Windows.Forms.Label lblPattern;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblWarning;
    }
}