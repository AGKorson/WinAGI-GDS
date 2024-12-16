
namespace WinAGI.Editor {
    partial class frmExportSoundOptions {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExportSoundOptions));
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            formatoptions = new System.Windows.Forms.GroupBox();
            optASS = new System.Windows.Forms.RadioButton();
            optWAV = new System.Windows.Forms.RadioButton();
            optMidi = new System.Windows.Forms.RadioButton();
            optNative = new System.Windows.Forms.RadioButton();
            formatoptions.SuspendLayout();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(285, 12);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(81, 25);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += OKButton_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(285, 44);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(81, 25);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += CancelButton_Click;
            // 
            // formatoptions
            // 
            formatoptions.Controls.Add(optASS);
            formatoptions.Controls.Add(optWAV);
            formatoptions.Controls.Add(optMidi);
            formatoptions.Controls.Add(optNative);
            formatoptions.Location = new System.Drawing.Point(10, 12);
            formatoptions.Name = "formatoptions";
            formatoptions.Size = new System.Drawing.Size(247, 137);
            formatoptions.TabIndex = 13;
            formatoptions.TabStop = false;
            formatoptions.Text = "Select the format to export:";
            // 
            // optASS
            // 
            optASS.AutoSize = true;
            optASS.Location = new System.Drawing.Point(15, 101);
            optASS.Name = "optASS";
            optASS.Size = new System.Drawing.Size(155, 19);
            optASS.TabIndex = 20;
            optASS.Text = "AGI Sound Script Format";
            optASS.UseVisualStyleBackColor = true;
            // 
            // optWAV
            // 
            optWAV.AutoSize = true;
            optWAV.Location = new System.Drawing.Point(15, 76);
            optWAV.Name = "optWAV";
            optWAV.Size = new System.Drawing.Size(213, 19);
            optWAV.TabIndex = 19;
            optWAV.Text = "PCM/Wave Compatible Conversion";
            optWAV.UseVisualStyleBackColor = true;
            // 
            // optMidi
            // 
            optMidi.AutoSize = true;
            optMidi.Location = new System.Drawing.Point(15, 51);
            optMidi.Name = "optMidi";
            optMidi.Size = new System.Drawing.Size(178, 19);
            optMidi.TabIndex = 18;
            optMidi.Text = "MIDI Compatible Conversion";
            optMidi.UseVisualStyleBackColor = true;
            // 
            // optNative
            // 
            optNative.AutoSize = true;
            optNative.Checked = true;
            optNative.Location = new System.Drawing.Point(15, 26);
            optNative.Name = "optNative";
            optNative.Size = new System.Drawing.Size(169, 19);
            optNative.TabIndex = 17;
            optNative.TabStop = true;
            optNative.Text = "AGI Native Sound Resource";
            optNative.UseVisualStyleBackColor = true;
            // 
            // frmSoundExpOptions
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(386, 159);
            ControlBox = false;
            Controls.Add(formatoptions);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmSoundExpOptions";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Export Sound Format";
            formatoptions.ResumeLayout(false);
            formatoptions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.GroupBox formatoptions;
        public System.Windows.Forms.RadioButton optASS;
        public System.Windows.Forms.RadioButton optWAV;
        public System.Windows.Forms.RadioButton optMidi;
        public System.Windows.Forms.RadioButton optNative;
    }
}