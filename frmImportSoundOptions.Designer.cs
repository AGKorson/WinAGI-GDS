
namespace WinAGI.Editor {
    partial class frmImportSoundOptions {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmImportSoundOptions));
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            txtChannel1 = new NumericTextBox();
            txtChannel2 = new NumericTextBox();
            txtChannel3 = new NumericTextBox();
            txtChannel4 = new NumericTextBox();
            chkExactTempo = new System.Windows.Forms.CheckBox();
            label3 = new System.Windows.Forms.Label();
            txtForcedNotes = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            txtInstShifts = new System.Windows.Forms.TextBox();
            chkPoly = new System.Windows.Forms.CheckBox();
            chkRemap = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            txtAutoDrum = new NumericTextBox();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(225, 12);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(81, 25);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += OKButton_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(225, 44);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(81, 25);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += CancelButton_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 11);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(59, 15);
            label1.TabIndex = 9;
            label1.Text = "Channels:";
            // 
            // txtChannel1
            // 
            txtChannel1.Location = new System.Drawing.Point(12, 34);
            txtChannel1.MaxValue = 127;
            txtChannel1.MinValue = -1;
            txtChannel1.Name = "txtChannel1";
            txtChannel1.Size = new System.Drawing.Size(30, 23);
            txtChannel1.TabIndex = 10;
            txtChannel1.Text = "-1";
            txtChannel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtChannel1.Value = -1;
            // 
            // txtChannel2
            // 
            txtChannel2.Location = new System.Drawing.Point(62, 34);
            txtChannel2.MaxValue = 127;
            txtChannel2.MinValue = -1;
            txtChannel2.Name = "txtChannel2";
            txtChannel2.Size = new System.Drawing.Size(30, 23);
            txtChannel2.TabIndex = 11;
            txtChannel2.Text = "-1";
            txtChannel2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtChannel2.Value = -1;
            // 
            // txtChannel3
            // 
            txtChannel3.Location = new System.Drawing.Point(112, 34);
            txtChannel3.MaxValue = 127;
            txtChannel3.MinValue = -1;
            txtChannel3.Name = "txtChannel3";
            txtChannel3.Size = new System.Drawing.Size(30, 23);
            txtChannel3.TabIndex = 12;
            txtChannel3.Text = "-1";
            txtChannel3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtChannel3.Value = -1;
            // 
            // txtChannel4
            // 
            txtChannel4.Location = new System.Drawing.Point(164, 34);
            txtChannel4.MaxValue = 127;
            txtChannel4.MinValue = -1;
            txtChannel4.Name = "txtChannel4";
            txtChannel4.Size = new System.Drawing.Size(30, 23);
            txtChannel4.TabIndex = 13;
            txtChannel4.Text = "-1";
            txtChannel4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtChannel4.Value = -1;
            // 
            // chkExactTempo
            // 
            chkExactTempo.AutoSize = true;
            chkExactTempo.Location = new System.Drawing.Point(13, 67);
            chkExactTempo.Name = "chkExactTempo";
            chkExactTempo.Size = new System.Drawing.Size(154, 19);
            chkExactTempo.TabIndex = 14;
            chkExactTempo.Text = "Use Exact Source Tempo";
            chkExactTempo.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 128);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(141, 15);
            label3.TabIndex = 15;
            label3.Text = "Instrument Forced Notes:";
            // 
            // txtForcedNotes
            // 
            txtForcedNotes.Location = new System.Drawing.Point(13, 146);
            txtForcedNotes.Name = "txtForcedNotes";
            txtForcedNotes.Size = new System.Drawing.Size(181, 23);
            txtForcedNotes.TabIndex = 16;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(16, 179);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(100, 15);
            label4.TabIndex = 17;
            label4.Text = "Instrument Shifts:";
            // 
            // txtInstShifts
            // 
            txtInstShifts.Location = new System.Drawing.Point(12, 197);
            txtInstShifts.Name = "txtInstShifts";
            txtInstShifts.Size = new System.Drawing.Size(181, 23);
            txtInstShifts.TabIndex = 18;
            // 
            // chkPoly
            // 
            chkPoly.AutoSize = true;
            chkPoly.Checked = true;
            chkPoly.CheckState = System.Windows.Forms.CheckState.Checked;
            chkPoly.Location = new System.Drawing.Point(12, 67);
            chkPoly.Name = "chkPoly";
            chkPoly.Size = new System.Drawing.Size(120, 19);
            chkPoly.TabIndex = 19;
            chkPoly.Text = "Polyphonic Mode";
            chkPoly.UseVisualStyleBackColor = true;
            // 
            // chkRemap
            // 
            chkRemap.AutoSize = true;
            chkRemap.Checked = true;
            chkRemap.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRemap.Location = new System.Drawing.Point(12, 97);
            chkRemap.Name = "chkRemap";
            chkRemap.Size = new System.Drawing.Size(143, 19);
            chkRemap.TabIndex = 20;
            chkRemap.Text = "Remap Noise Channel";
            chkRemap.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 98);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(129, 15);
            label2.TabIndex = 21;
            label2.Text = "Auto Drum Off Length:";
            // 
            // txtAutoDrum
            // 
            txtAutoDrum.Location = new System.Drawing.Point(147, 95);
            txtAutoDrum.MaxValue = 127;
            txtAutoDrum.MinValue = 0;
            txtAutoDrum.Name = "txtAutoDrum";
            txtAutoDrum.Size = new System.Drawing.Size(47, 23);
            txtAutoDrum.TabIndex = 22;
            txtAutoDrum.Text = "0";
            txtAutoDrum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtAutoDrum.Value = 0;
            // 
            // frmImportSoundOptions
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(316, 233);
            ControlBox = false;
            Controls.Add(txtAutoDrum);
            Controls.Add(label2);
            Controls.Add(chkRemap);
            Controls.Add(chkPoly);
            Controls.Add(txtInstShifts);
            Controls.Add(label4);
            Controls.Add(txtForcedNotes);
            Controls.Add(label3);
            Controls.Add(chkExactTempo);
            Controls.Add(txtChannel4);
            Controls.Add(txtChannel3);
            Controls.Add(txtChannel2);
            Controls.Add(txtChannel1);
            Controls.Add(label1);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmImportSoundOptions";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Impulse Tracker Import Options";
            Load += frmImportSoundOptions_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private NumericTextBox txtChannel1;
        private NumericTextBox txtChannel2;
        private NumericTextBox txtChannel3;
        private NumericTextBox txtChannel4;
        private System.Windows.Forms.CheckBox chkExactTempo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtForcedNotes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtInstShifts;
        private System.Windows.Forms.CheckBox chkPoly;
        private System.Windows.Forms.CheckBox chkRemap;
        private System.Windows.Forms.Label label2;
        private NumericTextBox txtAutoDrum;
    }
}