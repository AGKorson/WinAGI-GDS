namespace WinAGI.Editor {
    partial class frmImportProperties {
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
            chkResourceIDs = new System.Windows.Forms.CheckBox();
            chkResDefs = new System.Windows.Forms.CheckBox();
            btnGameDir = new System.Windows.Forms.Button();
            chkGlobals = new System.Windows.Forms.CheckBox();
            txtSrcExt = new System.Windows.Forms.TextBox();
            txtResDir = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            txtGameDir = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            btnCancel = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            cmbCodePage = new System.Windows.Forms.ComboBox();
            chkSierraSyntax = new System.Windows.Forms.CheckBox();
            btnAdvanced = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // chkResourceIDs
            // 
            chkResourceIDs.AutoSize = true;
            chkResourceIDs.Checked = true;
            chkResourceIDs.CheckState = System.Windows.Forms.CheckState.Checked;
            chkResourceIDs.Location = new System.Drawing.Point(12, 129);
            chkResourceIDs.Name = "chkResourceIDs";
            chkResourceIDs.Size = new System.Drawing.Size(215, 19);
            chkResourceIDs.TabIndex = 22;
            chkResourceIDs.Tag = "#useresnames";
            chkResourceIDs.Text = "Automatically Include Resource IDs ";
            chkResourceIDs.UseVisualStyleBackColor = true;
            chkResourceIDs.Click += chkResourceIDs_Click;
            // 
            // chkResDefs
            // 
            chkResDefs.AutoSize = true;
            chkResDefs.Checked = true;
            chkResDefs.CheckState = System.Windows.Forms.CheckState.Checked;
            chkResDefs.Location = new System.Drawing.Point(12, 149);
            chkResDefs.Name = "chkResDefs";
            chkResDefs.Size = new System.Drawing.Size(269, 19);
            chkResDefs.TabIndex = 23;
            chkResDefs.Tag = "#useresnames";
            chkResDefs.Text = "Automatically Include Reserved Define Names";
            chkResDefs.UseVisualStyleBackColor = true;
            chkResDefs.Click += chkUseReserved_Click;
            // 
            // btnGameDir
            // 
            btnGameDir.Location = new System.Drawing.Point(338, 29);
            btnGameDir.Name = "btnGameDir";
            btnGameDir.Size = new System.Drawing.Size(26, 23);
            btnGameDir.TabIndex = 17;
            btnGameDir.Tag = "#gamedir";
            btnGameDir.Text = "...";
            btnGameDir.UseVisualStyleBackColor = true;
            btnGameDir.Click += btnGameDir_Click;
            // 
            // chkGlobals
            // 
            chkGlobals.AutoSize = true;
            chkGlobals.Checked = true;
            chkGlobals.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGlobals.Location = new System.Drawing.Point(12, 169);
            chkGlobals.Name = "chkGlobals";
            chkGlobals.Size = new System.Drawing.Size(221, 19);
            chkGlobals.TabIndex = 24;
            chkGlobals.Tag = "#useresnames";
            chkGlobals.Text = "Automatically Include Global Defines";
            chkGlobals.UseVisualStyleBackColor = true;
            chkGlobals.Click += chkGlobals_Click;
            // 
            // txtSrcExt
            // 
            txtSrcExt.Location = new System.Drawing.Point(213, 85);
            txtSrcExt.Name = "txtSrcExt";
            txtSrcExt.Size = new System.Drawing.Size(88, 23);
            txtSrcExt.TabIndex = 21;
            txtSrcExt.Tag = "#defext";
            txtSrcExt.TextChanged += txtSrcExt_TextChanged;
            txtSrcExt.KeyPress += txtSrcExt_KeyPress;
            txtSrcExt.Leave += txtSrcExt_Leave;
            txtSrcExt.Validating += txtSrcExt_Validating;
            // 
            // txtResDir
            // 
            txtResDir.Location = new System.Drawing.Point(12, 85);
            txtResDir.Name = "txtResDir";
            txtResDir.Size = new System.Drawing.Size(152, 23);
            txtResDir.TabIndex = 19;
            txtResDir.Tag = "#resdir";
            txtResDir.TextChanged += txtResDir_TextChanged;
            txtResDir.KeyPress += txtResDir_KeyPress;
            txtResDir.Validating += txtResDir_Validating;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(213, 67);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(152, 15);
            label5.TabIndex = 20;
            label5.Text = "Logic Source File Extension:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 67);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(144, 15);
            label4.TabIndex = 18;
            label4.Text = "Resource Directory Name:";
            // 
            // txtGameDir
            // 
            txtGameDir.Location = new System.Drawing.Point(12, 29);
            txtGameDir.Name = "txtGameDir";
            txtGameDir.Size = new System.Drawing.Size(353, 23);
            txtGameDir.TabIndex = 16;
            txtGameDir.Tag = "#gamedir";
            txtGameDir.DoubleClick += txtGameDir_DoubleClick;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 10);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(97, 15);
            label3.TabIndex = 15;
            label3.Text = "Import Directory:";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.Location = new System.Drawing.Point(283, 267);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(77, 34);
            btnCancel.TabIndex = 27;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.Location = new System.Drawing.Point(184, 267);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(84, 34);
            btnOK.TabIndex = 26;
            btnOK.Text = "Import";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // cmbCodePage
            // 
            cmbCodePage.FormattingEnabled = true;
            cmbCodePage.Location = new System.Drawing.Point(12, 218);
            cmbCodePage.Name = "cmbCodePage";
            cmbCodePage.Size = new System.Drawing.Size(345, 23);
            cmbCodePage.TabIndex = 28;
            cmbCodePage.Tag = "#codepage";
            cmbCodePage.Visible = false;
            cmbCodePage.SelectionChangeCommitted += cmbCodePage_SelectionChangeCommitted;
            // 
            // chkSierraSyntax
            // 
            chkSierraSyntax.AutoSize = true;
            chkSierraSyntax.Location = new System.Drawing.Point(12, 247);
            chkSierraSyntax.Name = "chkSierraSyntax";
            chkSierraSyntax.Size = new System.Drawing.Size(114, 19);
            chkSierraSyntax.TabIndex = 29;
            chkSierraSyntax.Tag = "#sierrasrc";
            chkSierraSyntax.Text = "Use Sierra Syntax";
            chkSierraSyntax.UseVisualStyleBackColor = true;
            chkSierraSyntax.Visible = false;
            chkSierraSyntax.Click += chkSierraSyntax_Click;
            // 
            // btnAdvanced
            // 
            btnAdvanced.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAdvanced.Location = new System.Drawing.Point(12, 277);
            btnAdvanced.Name = "btnAdvanced";
            btnAdvanced.Size = new System.Drawing.Size(84, 24);
            btnAdvanced.TabIndex = 30;
            btnAdvanced.Text = "Advanced";
            btnAdvanced.UseVisualStyleBackColor = true;
            btnAdvanced.Click += btnAdvanced_Click;
            // 
            // frmImportProperties
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(376, 309);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(btnAdvanced);
            Controls.Add(chkSierraSyntax);
            Controls.Add(cmbCodePage);
            Controls.Add(chkResourceIDs);
            Controls.Add(chkResDefs);
            Controls.Add(btnGameDir);
            Controls.Add(chkGlobals);
            Controls.Add(txtSrcExt);
            Controls.Add(txtResDir);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(txtGameDir);
            Controls.Add(label3);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            HelpButton = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmImportProperties";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Import AGI Game";
            HelpRequested += frmGameProperties_HelpRequested;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        internal System.Windows.Forms.CheckBox chkResourceIDs;
        internal System.Windows.Forms.CheckBox chkResDefs;
        internal System.Windows.Forms.Button btnGameDir;
        internal System.Windows.Forms.CheckBox chkGlobals;
        internal System.Windows.Forms.TextBox txtSrcExt;
        internal System.Windows.Forms.TextBox txtResDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtGameDir;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnOK;
        internal System.Windows.Forms.ComboBox cmbCodePage;
        internal System.Windows.Forms.CheckBox chkSierraSyntax;
        internal System.Windows.Forms.Button btnAdvanced;
    }
}