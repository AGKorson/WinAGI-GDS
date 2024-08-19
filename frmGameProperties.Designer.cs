namespace WinAGI.Editor {
    partial class frmGameProperties {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGameProperties));
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            btnGameDir = new System.Windows.Forms.Button();
            chkUseLE = new System.Windows.Forms.CheckBox();
            chkUseReserved = new System.Windows.Forms.CheckBox();
            txtSrcExt = new System.Windows.Forms.TextBox();
            txtResDir = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            txtGameDir = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            cmbVersion = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            txtGameID = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tabPage2 = new System.Windows.Forms.TabPage();
            txtGameVersion = new System.Windows.Forms.TextBox();
            txtGameAbout = new System.Windows.Forms.TextBox();
            txtGameDescription = new System.Windows.Forms.TextBox();
            txtGameAuthor = new System.Windows.Forms.TextBox();
            label9 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            tabPage3 = new System.Windows.Forms.TabPage();
            btnPlatformFile = new System.Windows.Forms.Button();
            txtOptions = new System.Windows.Forms.TextBox();
            txtPlatformFile = new System.Windows.Forms.TextBox();
            lblOptions = new System.Windows.Forms.Label();
            lblPlatformFile = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            txtExec = new System.Windows.Forms.TextBox();
            lblExec = new System.Windows.Forms.Label();
            optOther = new System.Windows.Forms.RadioButton();
            optNAGI = new System.Windows.Forms.RadioButton();
            optScummVM = new System.Windows.Forms.RadioButton();
            optDosBox = new System.Windows.Forms.RadioButton();
            tabPage4 = new System.Windows.Forms.TabPage();
            chkSierraSyntax = new System.Windows.Forms.CheckBox();
            cmbCodePage = new System.Windows.Forms.ComboBox();
            lblSierraSyntax = new System.Windows.Forms.Label();
            lblCodePage = new System.Windows.Forms.Label();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.Location = new System.Drawing.Point(222, 378);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(84, 34);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.Location = new System.Drawing.Point(321, 378);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(77, 34);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new System.Drawing.Point(4, 4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(398, 372);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(btnGameDir);
            tabPage1.Controls.Add(chkUseLE);
            tabPage1.Controls.Add(chkUseReserved);
            tabPage1.Controls.Add(txtSrcExt);
            tabPage1.Controls.Add(txtResDir);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(txtGameDir);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(cmbVersion);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(txtGameID);
            tabPage1.Controls.Add(label1);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3);
            tabPage1.Size = new System.Drawing.Size(390, 344);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "General";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnGameDir
            // 
            btnGameDir.Enabled = false;
            btnGameDir.Location = new System.Drawing.Point(345, 128);
            btnGameDir.Name = "btnGameDir";
            btnGameDir.Size = new System.Drawing.Size(26, 23);
            btnGameDir.TabIndex = 6;
            btnGameDir.Text = "...";
            btnGameDir.UseVisualStyleBackColor = true;
            btnGameDir.Click += btnGameDir_Click;
            // 
            // chkUseLE
            // 
            chkUseLE.AutoSize = true;
            chkUseLE.Location = new System.Drawing.Point(19, 310);
            chkUseLE.Name = "chkUseLE";
            chkUseLE.Size = new System.Drawing.Size(118, 19);
            chkUseLE.TabIndex = 12;
            chkUseLE.Text = "Use Layout Editor";
            chkUseLE.UseVisualStyleBackColor = true;
            chkUseLE.CheckedChanged += chkUseLE_CheckedChanged;
            // 
            // chkUseReserved
            // 
            chkUseReserved.AutoSize = true;
            chkUseReserved.Location = new System.Drawing.Point(19, 283);
            chkUseReserved.Name = "chkUseReserved";
            chkUseReserved.Size = new System.Drawing.Size(172, 19);
            chkUseReserved.TabIndex = 11;
            chkUseReserved.Text = "Use Reserved Define Names";
            chkUseReserved.UseVisualStyleBackColor = true;
            chkUseReserved.CheckedChanged += chkUseReserved_CheckedChanged;
            // 
            // txtSrcExt
            // 
            txtSrcExt.Location = new System.Drawing.Point(220, 198);
            txtSrcExt.Name = "txtSrcExt";
            txtSrcExt.Size = new System.Drawing.Size(88, 23);
            txtSrcExt.TabIndex = 10;
            txtSrcExt.TextChanged += txtSrcExt_TextChanged;
            txtSrcExt.KeyPress += txtSrcExt_KeyPress;
            txtSrcExt.Leave += txtSrcExt_Leave;
            txtSrcExt.Validating += txtSrcExt_Validating;
            // 
            // txtResDir
            // 
            txtResDir.Location = new System.Drawing.Point(19, 198);
            txtResDir.Name = "txtResDir";
            txtResDir.Size = new System.Drawing.Size(152, 23);
            txtResDir.TabIndex = 8;
            txtResDir.TextChanged += txtResDir_TextChanged;
            txtResDir.KeyPress += txtResDir_KeyPress;
            txtResDir.Validating += txtResDir_Validating;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(220, 180);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(153, 15);
            label5.TabIndex = 9;
            label5.Text = "Logic Source File Extension:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(19, 180);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(144, 15);
            label4.TabIndex = 7;
            label4.Text = "Resource Directory Name:";
            // 
            // txtGameDir
            // 
            txtGameDir.Location = new System.Drawing.Point(19, 128);
            txtGameDir.Name = "txtGameDir";
            txtGameDir.Size = new System.Drawing.Size(352, 23);
            txtGameDir.TabIndex = 5;
            txtGameDir.DoubleClick += txtGameDir_DoubleClick;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(19, 109);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(92, 15);
            label3.TabIndex = 4;
            label3.Text = "Game Directory:";
            // 
            // cmbVersion
            // 
            cmbVersion.FormattingEnabled = true;
            cmbVersion.Location = new System.Drawing.Point(220, 37);
            cmbVersion.Name = "cmbVersion";
            cmbVersion.Size = new System.Drawing.Size(151, 23);
            cmbVersion.TabIndex = 3;
            cmbVersion.SelectionChangeCommitted += cmbVersion_SelectionChangeCommitted;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(220, 19);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(141, 15);
            label2.TabIndex = 2;
            label2.Text = "Target Interpreter Version:";
            // 
            // txtGameID
            // 
            txtGameID.Location = new System.Drawing.Point(19, 37);
            txtGameID.Name = "txtGameID";
            txtGameID.Size = new System.Drawing.Size(87, 23);
            txtGameID.TabIndex = 1;
            txtGameID.TextChanged += txtGameID_TextChanged;
            txtGameID.KeyPress += txtGameID_KeyPress;
            txtGameID.Validating += txtGameID_Validating;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(19, 19);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(52, 15);
            label1.TabIndex = 0;
            label1.Text = "GameID:";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(txtGameVersion);
            tabPage2.Controls.Add(txtGameAbout);
            tabPage2.Controls.Add(txtGameDescription);
            tabPage2.Controls.Add(txtGameAuthor);
            tabPage2.Controls.Add(label9);
            tabPage2.Controls.Add(label8);
            tabPage2.Controls.Add(label7);
            tabPage2.Controls.Add(label6);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(390, 344);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Version";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtGameVersion
            // 
            txtGameVersion.Location = new System.Drawing.Point(19, 283);
            txtGameVersion.Multiline = true;
            txtGameVersion.Name = "txtGameVersion";
            txtGameVersion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtGameVersion.Size = new System.Drawing.Size(352, 50);
            txtGameVersion.TabIndex = 7;
            txtGameVersion.TextChanged += txtGameVersion_TextChanged;
            txtGameVersion.KeyPress += txtGameVersion_KeyPress;
            // 
            // txtGameAbout
            // 
            txtGameAbout.Location = new System.Drawing.Point(19, 195);
            txtGameAbout.Multiline = true;
            txtGameAbout.Name = "txtGameAbout";
            txtGameAbout.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtGameAbout.Size = new System.Drawing.Size(352, 60);
            txtGameAbout.TabIndex = 5;
            txtGameAbout.TextChanged += txtGameAbout_TextChanged;
            txtGameAbout.KeyPress += txtGameAbout_KeyPress;
            // 
            // txtGameDescription
            // 
            txtGameDescription.Location = new System.Drawing.Point(19, 79);
            txtGameDescription.Multiline = true;
            txtGameDescription.Name = "txtGameDescription";
            txtGameDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtGameDescription.Size = new System.Drawing.Size(352, 85);
            txtGameDescription.TabIndex = 3;
            txtGameDescription.TextChanged += txtGameDescription_TextChanged;
            txtGameDescription.KeyPress += txtGameDescription_KeyPress;
            // 
            // txtGameAuthor
            // 
            txtGameAuthor.Location = new System.Drawing.Point(19, 26);
            txtGameAuthor.Name = "txtGameAuthor";
            txtGameAuthor.Size = new System.Drawing.Size(352, 23);
            txtGameAuthor.TabIndex = 1;
            txtGameAuthor.TextChanged += txtGameAuthor_TextChanged;
            txtGameAuthor.KeyPress += txtGameAuthor_KeyPress;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(19, 265);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(79, 15);
            label9.TabIndex = 6;
            label9.Text = "Game Version";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(19, 177);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(74, 15);
            label8.TabIndex = 4;
            label8.Text = "Game About";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(19, 61);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(67, 15);
            label7.TabIndex = 2;
            label7.Text = "Description";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(19, 7);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(44, 15);
            label6.TabIndex = 0;
            label6.Text = "Author";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(btnPlatformFile);
            tabPage3.Controls.Add(txtOptions);
            tabPage3.Controls.Add(txtPlatformFile);
            tabPage3.Controls.Add(lblOptions);
            tabPage3.Controls.Add(lblPlatformFile);
            tabPage3.Controls.Add(groupBox1);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new System.Windows.Forms.Padding(3);
            tabPage3.Size = new System.Drawing.Size(390, 344);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Platform";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnPlatformFile
            // 
            btnPlatformFile.Enabled = false;
            btnPlatformFile.Location = new System.Drawing.Point(345, 229);
            btnPlatformFile.Name = "btnPlatformFile";
            btnPlatformFile.Size = new System.Drawing.Size(26, 23);
            btnPlatformFile.TabIndex = 3;
            btnPlatformFile.Text = "...";
            btnPlatformFile.UseVisualStyleBackColor = true;
            btnPlatformFile.Click += btnPlatformFile_Click;
            // 
            // txtOptions
            // 
            txtOptions.Location = new System.Drawing.Point(19, 291);
            txtOptions.Name = "txtOptions";
            txtOptions.Size = new System.Drawing.Size(352, 23);
            txtOptions.TabIndex = 5;
            txtOptions.TextChanged += txtOptions_TextChanged;
            txtOptions.KeyPress += txtOptions_KeyPress;
            // 
            // txtPlatformFile
            // 
            txtPlatformFile.Location = new System.Drawing.Point(14, 229);
            txtPlatformFile.Name = "txtPlatformFile";
            txtPlatformFile.Size = new System.Drawing.Size(357, 23);
            txtPlatformFile.TabIndex = 2;
            txtPlatformFile.DoubleClick += txtPlatformFile_DoubleClick;
            txtPlatformFile.KeyPress += txtPlatformFile_KeyPress;
            // 
            // lblOptions
            // 
            lblOptions.AutoSize = true;
            lblOptions.Location = new System.Drawing.Point(19, 273);
            lblOptions.Name = "lblOptions";
            lblOptions.Size = new System.Drawing.Size(187, 15);
            lblOptions.TabIndex = 4;
            lblOptions.Text = "Optional Command Line Switches";
            // 
            // lblPlatformFile
            // 
            lblPlatformFile.AutoSize = true;
            lblPlatformFile.Location = new System.Drawing.Point(19, 211);
            lblPlatformFile.Name = "lblPlatformFile";
            lblPlatformFile.Size = new System.Drawing.Size(113, 15);
            lblPlatformFile.TabIndex = 1;
            lblPlatformFile.Text = "Platform Executable";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtExec);
            groupBox1.Controls.Add(lblExec);
            groupBox1.Controls.Add(optOther);
            groupBox1.Controls.Add(optNAGI);
            groupBox1.Controls.Add(optScummVM);
            groupBox1.Controls.Add(optDosBox);
            groupBox1.Location = new System.Drawing.Point(19, 17);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(352, 170);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Platform";
            // 
            // txtExec
            // 
            txtExec.Location = new System.Drawing.Point(129, 43);
            txtExec.Name = "txtExec";
            txtExec.Size = new System.Drawing.Size(89, 23);
            txtExec.TabIndex = 3;
            txtExec.TextChanged += txtExec_TextChanged;
            txtExec.KeyPress += txtExec_KeyPress;
            // 
            // lblExec
            // 
            lblExec.AutoSize = true;
            lblExec.Location = new System.Drawing.Point(34, 45);
            lblExec.Name = "lblExec";
            lblExec.Size = new System.Drawing.Size(90, 15);
            lblExec.TabIndex = 2;
            lblExec.Text = "DOS Executable";
            // 
            // optOther
            // 
            optOther.AutoSize = true;
            optOther.Location = new System.Drawing.Point(16, 132);
            optOther.Name = "optOther";
            optOther.Size = new System.Drawing.Size(55, 19);
            optOther.TabIndex = 6;
            optOther.TabStop = true;
            optOther.Text = "Other";
            optOther.UseVisualStyleBackColor = true;
            optOther.CheckedChanged += optOther_CheckedChanged;
            // 
            // optNAGI
            // 
            optNAGI.AutoSize = true;
            optNAGI.Location = new System.Drawing.Point(16, 103);
            optNAGI.Name = "optNAGI";
            optNAGI.Size = new System.Drawing.Size(53, 19);
            optNAGI.TabIndex = 5;
            optNAGI.TabStop = true;
            optNAGI.Text = "NAGI";
            optNAGI.UseVisualStyleBackColor = true;
            optNAGI.CheckedChanged += optNAGI_CheckedChanged;
            // 
            // optScummVM
            // 
            optScummVM.AutoSize = true;
            optScummVM.Location = new System.Drawing.Point(16, 74);
            optScummVM.Name = "optScummVM";
            optScummVM.Size = new System.Drawing.Size(84, 19);
            optScummVM.TabIndex = 4;
            optScummVM.TabStop = true;
            optScummVM.Text = "ScummVM";
            optScummVM.UseVisualStyleBackColor = true;
            optScummVM.CheckedChanged += optScummVM_CheckedChanged;
            // 
            // optDosBox
            // 
            optDosBox.AutoSize = true;
            optDosBox.Location = new System.Drawing.Point(16, 21);
            optDosBox.Name = "optDosBox";
            optDosBox.Size = new System.Drawing.Size(68, 19);
            optDosBox.TabIndex = 1;
            optDosBox.TabStop = true;
            optDosBox.Text = "DOSBox";
            optDosBox.UseVisualStyleBackColor = true;
            optDosBox.CheckedChanged += optDosBox_CheckedChanged;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(chkSierraSyntax);
            tabPage4.Controls.Add(cmbCodePage);
            tabPage4.Controls.Add(lblSierraSyntax);
            tabPage4.Controls.Add(lblCodePage);
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new System.Windows.Forms.Padding(3);
            tabPage4.Size = new System.Drawing.Size(390, 344);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Advanced";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // chkSierraSyntax
            // 
            chkSierraSyntax.AutoSize = true;
            chkSierraSyntax.Location = new System.Drawing.Point(19, 324);
            chkSierraSyntax.Name = "chkSierraSyntax";
            chkSierraSyntax.Size = new System.Drawing.Size(115, 19);
            chkSierraSyntax.TabIndex = 3;
            chkSierraSyntax.Text = "Use Sierra Syntax";
            chkSierraSyntax.UseVisualStyleBackColor = true;
            chkSierraSyntax.Click += chkSierraSyntax_Click;
            // 
            // cmbCodePage
            // 
            cmbCodePage.FormattingEnabled = true;
            cmbCodePage.Location = new System.Drawing.Point(19, 158);
            cmbCodePage.Name = "cmbCodePage";
            cmbCodePage.Size = new System.Drawing.Size(345, 23);
            cmbCodePage.TabIndex = 1;
            cmbCodePage.SelectionChangeCommitted += cmbCodePage_SelectionChangeCommitted;
            // 
            // lblSierraSyntax
            // 
            lblSierraSyntax.Location = new System.Drawing.Point(6, 195);
            lblSierraSyntax.Name = "lblSierraSyntax";
            lblSierraSyntax.Size = new System.Drawing.Size(378, 126);
            lblSierraSyntax.TabIndex = 2;
            lblSierraSyntax.Text = resources.GetString("lblSierraSyntax.Text");
            // 
            // lblCodePage
            // 
            lblCodePage.Location = new System.Drawing.Point(6, 12);
            lblCodePage.Name = "lblCodePage";
            lblCodePage.Size = new System.Drawing.Size(378, 143);
            lblCodePage.TabIndex = 0;
            lblCodePage.Text = resources.GetString("lblCodePage.Text");
            // 
            // frmGameProperties
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(404, 417);
            Controls.Add(tabControl1);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmGameProperties";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "frmGameProperties";
            Load += frmGameProperties_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtGameDir;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.TextBox txtGameID;
        internal System.Windows.Forms.ComboBox cmbVersion;
        internal System.Windows.Forms.TextBox txtResDir;
        internal System.Windows.Forms.TextBox txtSrcExt;
        internal System.Windows.Forms.CheckBox chkUseLE;
        internal System.Windows.Forms.CheckBox chkUseReserved;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox4;
        internal System.Windows.Forms.TextBox txtGameAuthor;
        internal System.Windows.Forms.TextBox txtGameDescription;
        internal System.Windows.Forms.TextBox txtGameAbout;
        internal System.Windows.Forms.TextBox txtGameVersion;
        private System.Windows.Forms.Label lblOptions;
        private System.Windows.Forms.Label lblPlatformFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.Label lblExec;
        internal System.Windows.Forms.RadioButton optDosBox;
        internal System.Windows.Forms.RadioButton optNAGI;
        internal System.Windows.Forms.RadioButton optScummVM;
        internal System.Windows.Forms.RadioButton optOther;
        internal System.Windows.Forms.TextBox txtPlatformFile;
        internal System.Windows.Forms.TextBox txtOptions;
        internal System.Windows.Forms.TextBox txtExec;
        internal System.Windows.Forms.ComboBox cmbCodePage;
        private System.Windows.Forms.Label lblSierraSyntax;
        private System.Windows.Forms.Label lblCodePage;
        internal System.Windows.Forms.CheckBox chkSierraSyntax;
        internal System.Windows.Forms.Button btnPlatformFile;
        internal System.Windows.Forms.Button btnGameDir;
        internal System.Windows.Forms.Button btnOK;
        internal System.Windows.Forms.TabControl tabControl1;
    }
}