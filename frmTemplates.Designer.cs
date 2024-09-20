namespace WinAGI.Editor {
    partial class frmTemplates {
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
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            lstTemplates = new System.Windows.Forms.ListBox();
            txtVersion = new System.Windows.Forms.TextBox();
            txtDescription = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 12);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(111, 15);
            label1.TabIndex = 0;
            label1.Text = "Avalable Templates:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(154, 12);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(70, 15);
            label2.TabIndex = 2;
            label2.Text = "AGI Version:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(154, 47);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(121, 15);
            label3.TabIndex = 4;
            label3.Text = "Template Description:";
            // 
            // btnOK
            // 
            btnOK.Enabled = false;
            btnOK.Location = new System.Drawing.Point(56, 130);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 6;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(189, 130);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lstTemplates
            // 
            lstTemplates.FormattingEnabled = true;
            lstTemplates.ItemHeight = 15;
            lstTemplates.Location = new System.Drawing.Point(14, 28);
            lstTemplates.Name = "lstTemplates";
            lstTemplates.Size = new System.Drawing.Size(117, 94);
            lstTemplates.TabIndex = 1;
            lstTemplates.SelectedIndexChanged += lstTemplates_SelectedIndexChanged;
            // 
            // txtVersion
            // 
            txtVersion.Location = new System.Drawing.Point(242, 9);
            txtVersion.Name = "txtVersion";
            txtVersion.Size = new System.Drawing.Size(62, 23);
            txtVersion.TabIndex = 3;
            // 
            // txtDescription
            // 
            txtDescription.Location = new System.Drawing.Point(154, 65);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new System.Drawing.Size(150, 57);
            txtDescription.TabIndex = 5;
            // 
            // frmTemplates
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(314, 163);
            Controls.Add(txtDescription);
            Controls.Add(txtVersion);
            Controls.Add(lstTemplates);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmTemplates";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Select a Template";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.ListBox lstTemplates;
        public System.Windows.Forms.TextBox txtVersion;
        public System.Windows.Forms.TextBox txtDescription;
    }
}