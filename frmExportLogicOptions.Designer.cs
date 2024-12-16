
namespace WinAGI.Editor {
    partial class frmExportLogicOptions {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExportLogicOptions));
            fraChoice = new System.Windows.Forms.GroupBox();
            optBoth = new System.Windows.Forms.RadioButton();
            optSourceCode = new System.Windows.Forms.RadioButton();
            optResource = new System.Windows.Forms.RadioButton();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            fraChoice.SuspendLayout();
            SuspendLayout();
            // 
            // fraChoice
            // 
            fraChoice.Controls.Add(optBoth);
            fraChoice.Controls.Add(optSourceCode);
            fraChoice.Controls.Add(optResource);
            fraChoice.Location = new System.Drawing.Point(16, 14);
            fraChoice.Name = "fraChoice";
            fraChoice.Size = new System.Drawing.Size(162, 102);
            fraChoice.TabIndex = 0;
            fraChoice.TabStop = false;
            // 
            // optBoth
            // 
            optBoth.AutoSize = true;
            optBoth.Location = new System.Drawing.Point(16, 68);
            optBoth.Name = "optBoth";
            optBoth.Size = new System.Drawing.Size(50, 19);
            optBoth.TabIndex = 2;
            optBoth.Text = "Both";
            optBoth.UseVisualStyleBackColor = true;
            // 
            // optSourceCode
            // 
            optSourceCode.AutoSize = true;
            optSourceCode.Checked = true;
            optSourceCode.Location = new System.Drawing.Point(16, 18);
            optSourceCode.Name = "optSourceCode";
            optSourceCode.Size = new System.Drawing.Size(92, 19);
            optSourceCode.TabIndex = 1;
            optSourceCode.TabStop = true;
            optSourceCode.Text = "Source Code";
            optSourceCode.UseVisualStyleBackColor = true;
            // 
            // optResource
            // 
            optResource.AutoSize = true;
            optResource.Location = new System.Drawing.Point(16, 43);
            optResource.Name = "optResource";
            optResource.Size = new System.Drawing.Size(95, 19);
            optResource.TabIndex = 0;
            optResource.Text = "AGI Resource";
            optResource.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(191, 12);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(81, 25);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += OKButton_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(191, 44);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(81, 25);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += CancelButton_Click;
            // 
            // frmExportLogicOptions
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(285, 124);
            ControlBox = false;
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(fraChoice);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmExportLogicOptions";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Export Logic Format";
            fraChoice.ResumeLayout(false);
            fraChoice.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox fraChoice;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.RadioButton optSourceCode;
        public System.Windows.Forms.RadioButton optResource;
        public System.Windows.Forms.RadioButton optBoth;
    }
}