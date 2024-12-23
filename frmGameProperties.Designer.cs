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
            General = new System.Windows.Forms.TabPage();
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
            Version = new System.Windows.Forms.TabPage();
            txtGameVersion = new System.Windows.Forms.TextBox();
            txtGameAbout = new System.Windows.Forms.TextBox();
            txtGameDescription = new System.Windows.Forms.TextBox();
            txtGameAuthor = new System.Windows.Forms.TextBox();
            label9 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            Platform = new System.Windows.Forms.TabPage();
            btnPlatformFile = new System.Windows.Forms.Button();
            txtOptions = new System.Windows.Forms.TextBox();
            txtPlatformFile = new System.Windows.Forms.TextBox();
            lblOptions = new System.Windows.Forms.Label();
            lblPlatformFile = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            optNone = new System.Windows.Forms.RadioButton();
            txtExec = new System.Windows.Forms.TextBox();
            lblExec = new System.Windows.Forms.Label();
            optOther = new System.Windows.Forms.RadioButton();
            optNAGI = new System.Windows.Forms.RadioButton();
            optScummVM = new System.Windows.Forms.RadioButton();
            optDosBox = new System.Windows.Forms.RadioButton();
            Advanced = new System.Windows.Forms.TabPage();
            chkSierraSyntax = new System.Windows.Forms.CheckBox();
            cmbCodePage = new System.Windows.Forms.ComboBox();
            lblSierraSyntax = new System.Windows.Forms.Label();
            lblCodePage = new System.Windows.Forms.Label();
            tabControl1.SuspendLayout();
            General.SuspendLayout();
            Version.SuspendLayout();
            Platform.SuspendLayout();
            groupBox1.SuspendLayout();
            Advanced.SuspendLayout();
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
            tabControl1.Controls.Add(General);
            tabControl1.Controls.Add(Version);
            tabControl1.Controls.Add(Platform);
            tabControl1.Controls.Add(Advanced);
            tabControl1.Location = new System.Drawing.Point(4, 4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(398, 372);
            tabControl1.TabIndex = 0;
            // 
            // General
            // 
            General.Controls.Add(btnGameDir);
            General.Controls.Add(chkUseLE);
            General.Controls.Add(chkUseReserved);
            General.Controls.Add(txtSrcExt);
            General.Controls.Add(txtResDir);
            General.Controls.Add(label5);
            General.Controls.Add(label4);
            General.Controls.Add(txtGameDir);
            General.Controls.Add(label3);
            General.Controls.Add(cmbVersion);
            General.Controls.Add(label2);
            General.Controls.Add(txtGameID);
            General.Controls.Add(label1);
            General.Location = new System.Drawing.Point(4, 24);
            General.Name = "General";
            General.Padding = new System.Windows.Forms.Padding(3);
            General.Size = new System.Drawing.Size(390, 344);
            General.TabIndex = 0;
            General.Text = "General";
            General.UseVisualStyleBackColor = true;
            // 
            // btnGameDir
            // 
            btnGameDir.Enabled = false;
            btnGameDir.Location = new System.Drawing.Point(345, 128);
            btnGameDir.Name = "btnGameDir";
            btnGameDir.Size = new System.Drawing.Size(26, 23);
            btnGameDir.TabIndex = 6;
            btnGameDir.Tag = "#gamedir";
            btnGameDir.Text = "...";
            btnGameDir.UseVisualStyleBackColor = true;
            btnGameDir.Click += btnGameDir_Click;
            btnGameDir.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // chkUseLE
            // 
            chkUseLE.AutoSize = true;
            chkUseLE.Location = new System.Drawing.Point(19, 310);
            chkUseLE.Name = "chkUseLE";
            chkUseLE.Size = new System.Drawing.Size(118, 19);
            chkUseLE.TabIndex = 12;
            chkUseLE.Tag = "#uselayouted";
            chkUseLE.Text = "Use Layout Editor";
            chkUseLE.UseVisualStyleBackColor = true;
            chkUseLE.CheckedChanged += chkUseLE_CheckedChanged;
            chkUseLE.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // chkUseReserved
            // 
            chkUseReserved.AutoSize = true;
            chkUseReserved.Location = new System.Drawing.Point(19, 283);
            chkUseReserved.Name = "chkUseReserved";
            chkUseReserved.Size = new System.Drawing.Size(172, 19);
            chkUseReserved.TabIndex = 11;
            chkUseReserved.Tag = "#useresnames";
            chkUseReserved.Text = "Use Reserved Define Names";
            chkUseReserved.UseVisualStyleBackColor = true;
            chkUseReserved.CheckedChanged += chkUseReserved_CheckedChanged;
            chkUseReserved.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // txtSrcExt
            // 
            txtSrcExt.Location = new System.Drawing.Point(220, 198);
            txtSrcExt.Name = "txtSrcExt";
            txtSrcExt.Size = new System.Drawing.Size(88, 23);
            txtSrcExt.TabIndex = 10;
            txtSrcExt.Tag = "#defext";
            txtSrcExt.TextChanged += txtSrcExt_TextChanged;
            txtSrcExt.HelpRequested += frmGameProperties_HelpRequested;
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
            txtResDir.Tag = "#resdir";
            txtResDir.TextChanged += txtResDir_TextChanged;
            txtResDir.HelpRequested += frmGameProperties_HelpRequested;
            txtResDir.KeyPress += txtResDir_KeyPress;
            txtResDir.Validating += txtResDir_Validating;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(220, 180);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(152, 15);
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
            txtGameDir.Tag = "#gamedir";
            txtGameDir.HelpRequested += frmGameProperties_HelpRequested;
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
            cmbVersion.Tag = "#intversion";
            cmbVersion.SelectionChangeCommitted += cmbVersion_SelectionChangeCommitted;
            cmbVersion.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(220, 19);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(142, 15);
            label2.TabIndex = 2;
            label2.Text = "Target Interpreter Version:";
            // 
            // txtGameID
            // 
            txtGameID.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            txtGameID.Location = new System.Drawing.Point(19, 37);
            txtGameID.Name = "txtGameID";
            txtGameID.Size = new System.Drawing.Size(87, 23);
            txtGameID.TabIndex = 1;
            txtGameID.Tag = "#gameid";
            txtGameID.TextChanged += txtGameID_TextChanged;
            txtGameID.HelpRequested += frmGameProperties_HelpRequested;
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
            // Version
            // 
            Version.Controls.Add(txtGameVersion);
            Version.Controls.Add(txtGameAbout);
            Version.Controls.Add(txtGameDescription);
            Version.Controls.Add(txtGameAuthor);
            Version.Controls.Add(label9);
            Version.Controls.Add(label8);
            Version.Controls.Add(label7);
            Version.Controls.Add(label6);
            Version.Location = new System.Drawing.Point(4, 24);
            Version.Name = "Version";
            Version.Padding = new System.Windows.Forms.Padding(3);
            Version.Size = new System.Drawing.Size(390, 344);
            Version.TabIndex = 1;
            Version.Text = "Version";
            Version.UseVisualStyleBackColor = true;
            // 
            // txtGameVersion
            // 
            txtGameVersion.Location = new System.Drawing.Point(19, 283);
            txtGameVersion.Multiline = true;
            txtGameVersion.Name = "txtGameVersion";
            txtGameVersion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtGameVersion.Size = new System.Drawing.Size(352, 50);
            txtGameVersion.TabIndex = 7;
            txtGameVersion.Tag = "#gameversion";
            txtGameVersion.TextChanged += txtGameVersion_TextChanged;
            txtGameVersion.HelpRequested += frmGameProperties_HelpRequested;
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
            txtGameAbout.Tag = "#about";
            txtGameAbout.TextChanged += txtGameAbout_TextChanged;
            txtGameAbout.HelpRequested += frmGameProperties_HelpRequested;
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
            txtGameDescription.Tag = "#description";
            txtGameDescription.TextChanged += txtGameDescription_TextChanged;
            txtGameDescription.HelpRequested += frmGameProperties_HelpRequested;
            txtGameDescription.KeyPress += txtGameDescription_KeyPress;
            // 
            // txtGameAuthor
            // 
            txtGameAuthor.Location = new System.Drawing.Point(19, 26);
            txtGameAuthor.Name = "txtGameAuthor";
            txtGameAuthor.Size = new System.Drawing.Size(352, 23);
            txtGameAuthor.TabIndex = 1;
            txtGameAuthor.Tag = "#author";
            txtGameAuthor.TextChanged += txtGameAuthor_TextChanged;
            txtGameAuthor.HelpRequested += frmGameProperties_HelpRequested;
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
            // Platform
            // 
            Platform.Controls.Add(btnPlatformFile);
            Platform.Controls.Add(txtOptions);
            Platform.Controls.Add(txtPlatformFile);
            Platform.Controls.Add(lblOptions);
            Platform.Controls.Add(lblPlatformFile);
            Platform.Controls.Add(groupBox1);
            Platform.Location = new System.Drawing.Point(4, 24);
            Platform.Name = "Platform";
            Platform.Padding = new System.Windows.Forms.Padding(3);
            Platform.Size = new System.Drawing.Size(390, 344);
            Platform.TabIndex = 2;
            Platform.Text = "Platform";
            Platform.UseVisualStyleBackColor = true;
            // 
            // btnPlatformFile
            // 
            btnPlatformFile.Enabled = false;
            btnPlatformFile.Location = new System.Drawing.Point(345, 241);
            btnPlatformFile.Name = "btnPlatformFile";
            btnPlatformFile.Size = new System.Drawing.Size(26, 23);
            btnPlatformFile.TabIndex = 3;
            btnPlatformFile.Tag = "#executable";
            btnPlatformFile.Text = "...";
            btnPlatformFile.UseVisualStyleBackColor = true;
            btnPlatformFile.Click += btnPlatformFile_Click;
            btnPlatformFile.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // txtOptions
            // 
            txtOptions.Enabled = false;
            txtOptions.Location = new System.Drawing.Point(19, 303);
            txtOptions.Name = "txtOptions";
            txtOptions.Size = new System.Drawing.Size(352, 23);
            txtOptions.TabIndex = 5;
            txtOptions.Tag = "#executable";
            txtOptions.TextChanged += txtOptions_TextChanged;
            txtOptions.HelpRequested += frmGameProperties_HelpRequested;
            txtOptions.KeyPress += txtOptions_KeyPress;
            // 
            // txtPlatformFile
            // 
            txtPlatformFile.Enabled = false;
            txtPlatformFile.Location = new System.Drawing.Point(14, 241);
            txtPlatformFile.Name = "txtPlatformFile";
            txtPlatformFile.Size = new System.Drawing.Size(357, 23);
            txtPlatformFile.TabIndex = 2;
            txtPlatformFile.Tag = "#executable";
            txtPlatformFile.TextChanged += txtPlatformFile_TextChanged;
            txtPlatformFile.HelpRequested += frmGameProperties_HelpRequested;
            txtPlatformFile.DoubleClick += txtPlatformFile_DoubleClick;
            txtPlatformFile.KeyPress += txtPlatformFile_KeyPress;
            txtPlatformFile.Validating += txtPlatformFile_Validating;
            // 
            // lblOptions
            // 
            lblOptions.AutoSize = true;
            lblOptions.Location = new System.Drawing.Point(19, 285);
            lblOptions.Name = "lblOptions";
            lblOptions.Size = new System.Drawing.Size(187, 15);
            lblOptions.TabIndex = 4;
            lblOptions.Text = "Optional Command Line Switches";
            // 
            // lblPlatformFile
            // 
            lblPlatformFile.AutoSize = true;
            lblPlatformFile.Location = new System.Drawing.Point(19, 223);
            lblPlatformFile.Name = "lblPlatformFile";
            lblPlatformFile.Size = new System.Drawing.Size(112, 15);
            lblPlatformFile.TabIndex = 1;
            lblPlatformFile.Text = "Platform Executable";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(optNone);
            groupBox1.Controls.Add(txtExec);
            groupBox1.Controls.Add(lblExec);
            groupBox1.Controls.Add(optOther);
            groupBox1.Controls.Add(optNAGI);
            groupBox1.Controls.Add(optScummVM);
            groupBox1.Controls.Add(optDosBox);
            groupBox1.Location = new System.Drawing.Point(19, 17);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(352, 190);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Platform";
            // 
            // optNone
            // 
            optNone.AutoSize = true;
            optNone.Location = new System.Drawing.Point(16, 20);
            optNone.Name = "optNone";
            optNone.Size = new System.Drawing.Size(54, 19);
            optNone.TabIndex = 0;
            optNone.TabStop = true;
            optNone.Tag = "#executable";
            optNone.Text = "None";
            optNone.UseVisualStyleBackColor = true;
            optNone.CheckedChanged += optNone_CheckedChanged;
            // 
            // txtExec
            // 
            txtExec.Location = new System.Drawing.Point(129, 71);
            txtExec.Name = "txtExec";
            txtExec.Size = new System.Drawing.Size(89, 23);
            txtExec.TabIndex = 3;
            txtExec.Tag = "#executable";
            txtExec.TextChanged += txtExec_TextChanged;
            txtExec.HelpRequested += frmGameProperties_HelpRequested;
            txtExec.KeyPress += txtExec_KeyPress;
            // 
            // lblExec
            // 
            lblExec.AutoSize = true;
            lblExec.Location = new System.Drawing.Point(34, 73);
            lblExec.Name = "lblExec";
            lblExec.Size = new System.Drawing.Size(89, 15);
            lblExec.TabIndex = 2;
            lblExec.Text = "DOS Executable";
            // 
            // optOther
            // 
            optOther.AutoSize = true;
            optOther.Location = new System.Drawing.Point(16, 160);
            optOther.Name = "optOther";
            optOther.Size = new System.Drawing.Size(55, 19);
            optOther.TabIndex = 6;
            optOther.TabStop = true;
            optOther.Tag = "#executable";
            optOther.Text = "Other";
            optOther.UseVisualStyleBackColor = true;
            optOther.CheckedChanged += optOther_CheckedChanged;
            optOther.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // optNAGI
            // 
            optNAGI.AutoSize = true;
            optNAGI.Location = new System.Drawing.Point(16, 131);
            optNAGI.Name = "optNAGI";
            optNAGI.Size = new System.Drawing.Size(53, 19);
            optNAGI.TabIndex = 5;
            optNAGI.TabStop = true;
            optNAGI.Tag = "#executable";
            optNAGI.Text = "NAGI";
            optNAGI.UseVisualStyleBackColor = true;
            optNAGI.CheckedChanged += optNAGI_CheckedChanged;
            optNAGI.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // optScummVM
            // 
            optScummVM.AutoSize = true;
            optScummVM.Location = new System.Drawing.Point(16, 102);
            optScummVM.Name = "optScummVM";
            optScummVM.Size = new System.Drawing.Size(84, 19);
            optScummVM.TabIndex = 4;
            optScummVM.TabStop = true;
            optScummVM.Tag = "#executable";
            optScummVM.Text = "ScummVM";
            optScummVM.UseVisualStyleBackColor = true;
            optScummVM.CheckedChanged += optScummVM_CheckedChanged;
            optScummVM.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // optDosBox
            // 
            optDosBox.AutoSize = true;
            optDosBox.Location = new System.Drawing.Point(16, 49);
            optDosBox.Name = "optDosBox";
            optDosBox.Size = new System.Drawing.Size(67, 19);
            optDosBox.TabIndex = 1;
            optDosBox.TabStop = true;
            optDosBox.Tag = "#executable";
            optDosBox.Text = "DOSBox";
            optDosBox.UseVisualStyleBackColor = true;
            optDosBox.CheckedChanged += optDosBox_CheckedChanged;
            optDosBox.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // Advanced
            // 
            Advanced.Controls.Add(chkSierraSyntax);
            Advanced.Controls.Add(cmbCodePage);
            Advanced.Controls.Add(lblSierraSyntax);
            Advanced.Controls.Add(lblCodePage);
            Advanced.Location = new System.Drawing.Point(4, 24);
            Advanced.Name = "Advanced";
            Advanced.Padding = new System.Windows.Forms.Padding(3);
            Advanced.Size = new System.Drawing.Size(390, 344);
            Advanced.TabIndex = 3;
            Advanced.Text = "Advanced";
            Advanced.UseVisualStyleBackColor = true;
            // 
            // chkSierraSyntax
            // 
            chkSierraSyntax.AutoSize = true;
            chkSierraSyntax.Location = new System.Drawing.Point(19, 324);
            chkSierraSyntax.Name = "chkSierraSyntax";
            chkSierraSyntax.Size = new System.Drawing.Size(114, 19);
            chkSierraSyntax.TabIndex = 3;
            chkSierraSyntax.Tag = "#sierrasrc";
            chkSierraSyntax.Text = "Use Sierra Syntax";
            chkSierraSyntax.UseVisualStyleBackColor = true;
            chkSierraSyntax.Click += chkSierraSyntax_Click;
            chkSierraSyntax.HelpRequested += frmGameProperties_HelpRequested;
            // 
            // cmbCodePage
            // 
            cmbCodePage.FormattingEnabled = true;
            cmbCodePage.Location = new System.Drawing.Point(19, 158);
            cmbCodePage.Name = "cmbCodePage";
            cmbCodePage.Size = new System.Drawing.Size(345, 23);
            cmbCodePage.TabIndex = 1;
            cmbCodePage.Tag = "#codepage";
            cmbCodePage.SelectionChangeCommitted += cmbCodePage_SelectionChangeCommitted;
            cmbCodePage.HelpRequested += frmGameProperties_HelpRequested;
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
            HelpButton = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmGameProperties";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "frmGameProperties";
            tabControl1.ResumeLayout(false);
            General.ResumeLayout(false);
            General.PerformLayout();
            Version.ResumeLayout(false);
            Version.PerformLayout();
            Platform.ResumeLayout(false);
            Platform.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            Advanced.ResumeLayout(false);
            Advanced.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabPage General;
        private System.Windows.Forms.TabPage Version;
        private System.Windows.Forms.TabPage Platform;
        private System.Windows.Forms.TabPage Advanced;
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
        internal System.Windows.Forms.TextBox txtGameAuthor;
        internal System.Windows.Forms.TextBox txtGameDescription;
        internal System.Windows.Forms.TextBox txtGameAbout;
        internal System.Windows.Forms.TextBox txtGameVersion;
        private System.Windows.Forms.Label lblOptions;
        private System.Windows.Forms.Label lblPlatformFile;
        private System.Windows.Forms.GroupBox groupBox1;
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
        internal System.Windows.Forms.RadioButton optNone;
    }
}