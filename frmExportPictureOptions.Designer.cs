
namespace WinAGI.Editor {
    partial class frmExportPictureOptions {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExportPictureOptions));
            fraChoice = new System.Windows.Forms.GroupBox();
            optImage = new System.Windows.Forms.RadioButton();
            optResource = new System.Windows.Forms.RadioButton();
            fraImage = new System.Windows.Forms.GroupBox();
            optBoth = new System.Windows.Forms.RadioButton();
            optPriority = new System.Windows.Forms.RadioButton();
            optVisual = new System.Windows.Forms.RadioButton();
            cmbFormat = new System.Windows.Forms.ComboBox();
            lblFormat = new System.Windows.Forms.Label();
            lblScale = new System.Windows.Forms.Label();
            udZoom = new System.Windows.Forms.NumericUpDown();
            lblBoth = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            fraChoice.SuspendLayout();
            fraImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)udZoom).BeginInit();
            SuspendLayout();
            // 
            // fraChoice
            // 
            fraChoice.Controls.Add(optImage);
            fraChoice.Controls.Add(optResource);
            fraChoice.Location = new System.Drawing.Point(16, 14);
            fraChoice.Name = "fraChoice";
            fraChoice.Size = new System.Drawing.Size(265, 49);
            fraChoice.TabIndex = 0;
            fraChoice.TabStop = false;
            // 
            // optImage
            // 
            optImage.AutoSize = true;
            optImage.Location = new System.Drawing.Point(144, 18);
            optImage.Name = "optImage";
            optImage.Size = new System.Drawing.Size(98, 19);
            optImage.TabIndex = 1;
            optImage.TabStop = true;
            optImage.Text = "Picture Image";
            optImage.UseVisualStyleBackColor = true;
            optImage.CheckedChanged += optImageFormat;
            // 
            // optResource
            // 
            optResource.AutoSize = true;
            optResource.Location = new System.Drawing.Point(16, 18);
            optResource.Name = "optResource";
            optResource.Size = new System.Drawing.Size(95, 19);
            optResource.TabIndex = 0;
            optResource.TabStop = true;
            optResource.Text = "AGI Resource";
            optResource.UseVisualStyleBackColor = true;
            optResource.CheckedChanged += optImageFormat;
            // 
            // fraImage
            // 
            fraImage.Controls.Add(optBoth);
            fraImage.Controls.Add(optPriority);
            fraImage.Controls.Add(optVisual);
            fraImage.Location = new System.Drawing.Point(16, 72);
            fraImage.Name = "fraImage";
            fraImage.Size = new System.Drawing.Size(161, 89);
            fraImage.TabIndex = 1;
            fraImage.TabStop = false;
            fraImage.Text = "Image to export";
            // 
            // optBoth
            // 
            optBoth.AutoSize = true;
            optBoth.Checked = true;
            optBoth.Location = new System.Drawing.Point(24, 62);
            optBoth.Name = "optBoth";
            optBoth.Size = new System.Drawing.Size(50, 19);
            optBoth.TabIndex = 2;
            optBoth.TabStop = true;
            optBoth.Text = "Both";
            optBoth.UseVisualStyleBackColor = true;
            optBoth.CheckedChanged += optImageType_CheckedChanged;
            // 
            // optPriority
            // 
            optPriority.AutoSize = true;
            optPriority.Location = new System.Drawing.Point(24, 40);
            optPriority.Name = "optPriority";
            optPriority.Size = new System.Drawing.Size(103, 19);
            optPriority.TabIndex = 1;
            optPriority.Text = "Priority Picture";
            optPriority.UseVisualStyleBackColor = true;
            optPriority.CheckedChanged += optImageType_CheckedChanged;
            optPriority.Click += optImageType_CheckedChanged;
            // 
            // optVisual
            // 
            optVisual.AutoSize = true;
            optVisual.Location = new System.Drawing.Point(24, 18);
            optVisual.Name = "optVisual";
            optVisual.Size = new System.Drawing.Size(96, 19);
            optVisual.TabIndex = 0;
            optVisual.Text = "Visual Picture";
            optVisual.UseVisualStyleBackColor = true;
            optVisual.CheckedChanged += optImageType_CheckedChanged;
            optVisual.Click += optImageType_CheckedChanged;
            // 
            // cmbFormat
            // 
            cmbFormat.FormattingEnabled = true;
            cmbFormat.Items.AddRange(new object[] { "Bitmap", "JPG", "GIF", "TIF", "PNG" });
            cmbFormat.Location = new System.Drawing.Point(16, 180);
            cmbFormat.Name = "cmbFormat";
            cmbFormat.Size = new System.Drawing.Size(161, 23);
            cmbFormat.TabIndex = 2;
            // 
            // lblFormat
            // 
            lblFormat.AutoSize = true;
            lblFormat.Location = new System.Drawing.Point(16, 164);
            lblFormat.Name = "lblFormat";
            lblFormat.Size = new System.Drawing.Size(84, 15);
            lblFormat.TabIndex = 3;
            lblFormat.Text = "Image Format:";
            // 
            // lblScale
            // 
            lblScale.AutoSize = true;
            lblScale.Location = new System.Drawing.Point(180, 84);
            lblScale.Name = "lblScale";
            lblScale.Size = new System.Drawing.Size(37, 15);
            lblScale.TabIndex = 4;
            lblScale.Text = "Scale:";
            // 
            // udZoom
            // 
            udZoom.Location = new System.Drawing.Point(223, 82);
            udZoom.Name = "udZoom";
            udZoom.Size = new System.Drawing.Size(71, 23);
            udZoom.TabIndex = 5;
            udZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            udZoom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblBoth
            // 
            lblBoth.Location = new System.Drawing.Point(192, 160);
            lblBoth.Name = "lblBoth";
            lblBoth.Size = new System.Drawing.Size(177, 49);
            lblBoth.TabIndex = 6;
            lblBoth.Text = "File name for the Priority Screen will be the same as the Visual Screen with \"_P\" appended.";
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(312, 12);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(81, 25);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += OKButton_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(312, 44);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(81, 25);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += CancelButton_Click;
            // 
            // frmExportPictureOptions
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(415, 215);
            ControlBox = false;
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(lblBoth);
            Controls.Add(udZoom);
            Controls.Add(lblScale);
            Controls.Add(lblFormat);
            Controls.Add(cmbFormat);
            Controls.Add(fraImage);
            Controls.Add(fraChoice);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmExportPictureOptions";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Export Picture Format";
            fraChoice.ResumeLayout(false);
            fraChoice.PerformLayout();
            fraImage.ResumeLayout(false);
            fraImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)udZoom).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox fraChoice;
        private System.Windows.Forms.GroupBox fraImage;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.Label lblBoth;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.RadioButton optPriority;
        public System.Windows.Forms.RadioButton optVisual;
        public System.Windows.Forms.ComboBox cmbFormat;
        public System.Windows.Forms.NumericUpDown udZoom;
        public System.Windows.Forms.RadioButton optImage;
        public System.Windows.Forms.RadioButton optResource;
        public System.Windows.Forms.RadioButton optBoth;
    }
}