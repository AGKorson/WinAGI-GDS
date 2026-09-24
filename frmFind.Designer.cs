
namespace WinAGI.Editor {
    partial class frmFind {
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
            lblReplace = new System.Windows.Forms.Label();
            rtfReplace = new System.Windows.Forms.TextBox();
            fraScope = new System.Windows.Forms.GroupBox();
            optProject = new System.Windows.Forms.RadioButton();
            optOpen = new System.Windows.Forms.RadioButton();
            optCurrent = new System.Windows.Forms.RadioButton();
            lblDirection = new System.Windows.Forms.Label();
            cmbDirection = new System.Windows.Forms.ComboBox();
            chkMatchWord = new System.Windows.Forms.CheckBox();
            chkMatchCase = new System.Windows.Forms.CheckBox();
            btnFind = new System.Windows.Forms.Button();
            btnReplace = new System.Windows.Forms.Button();
            btnReplaceAll = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            cmbFind = new System.Windows.Forms.ComboBox();
            chkSynonyms = new System.Windows.Forms.CheckBox();
            txtFind = new System.Windows.Forms.TextBox();
            btnFindAll = new System.Windows.Forms.Button();
            fraScope.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(62, 15);
            label1.TabIndex = 0;
            label1.Text = "Find what:";
            // 
            // lblReplace
            // 
            lblReplace.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblReplace.AutoSize = true;
            lblReplace.Location = new System.Drawing.Point(11, 51);
            lblReplace.Name = "lblReplace";
            lblReplace.Size = new System.Drawing.Size(77, 15);
            lblReplace.TabIndex = 2;
            lblReplace.Text = "Replace with:";
            // 
            // rtfReplace
            // 
            rtfReplace.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            rtfReplace.Location = new System.Drawing.Point(94, 48);
            rtfReplace.Name = "rtfReplace";
            rtfReplace.Size = new System.Drawing.Size(258, 23);
            rtfReplace.TabIndex = 3;
            rtfReplace.TextChanged += rtfReplace_TextChanged;
            rtfReplace.Enter += rtfReplace_Enter;
            // 
            // fraLogic
            // 
            fraScope.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            fraScope.Controls.Add(optProject);
            fraScope.Controls.Add(optOpen);
            fraScope.Controls.Add(optCurrent);
            fraScope.Location = new System.Drawing.Point(12, 77);
            fraScope.Name = "fraLogic";
            fraScope.Size = new System.Drawing.Size(162, 92);
            fraScope.TabIndex = 4;
            fraScope.TabStop = false;
            fraScope.Text = "Search";
            // 
            // optProject
            // 
            optProject.AutoSize = true;
            optProject.Location = new System.Drawing.Point(19, 68);
            optProject.Name = "optProject";
            optProject.Size = new System.Drawing.Size(95, 19);
            optProject.TabIndex = 2;
            optProject.TabStop = true;
            optProject.Text = "Entire Project";
            optProject.UseVisualStyleBackColor = true;
            optProject.Click += optProject_Click;
            // 
            // optOpen
            // 
            optOpen.AutoSize = true;
            optOpen.Location = new System.Drawing.Point(19, 44);
            optOpen.Name = "optOpen";
            optOpen.Size = new System.Drawing.Size(97, 19);
            optOpen.TabIndex = 1;
            optOpen.TabStop = true;
            optOpen.Text = "All Open Files";
            optOpen.UseVisualStyleBackColor = true;
            optOpen.Click += optOpen_Click;
            // 
            // optCurrent
            // 
            optCurrent.AutoSize = true;
            optCurrent.Location = new System.Drawing.Point(19, 20);
            optCurrent.Name = "optCurrent";
            optCurrent.Size = new System.Drawing.Size(97, 19);
            optCurrent.TabIndex = 0;
            optCurrent.TabStop = true;
            optCurrent.Text = "Current Logic";
            optCurrent.UseVisualStyleBackColor = true;
            optCurrent.Click += optCurrent_Click;
            // 
            // lblDirection
            // 
            lblDirection.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblDirection.AutoSize = true;
            lblDirection.Location = new System.Drawing.Point(206, 87);
            lblDirection.Name = "lblDirection";
            lblDirection.Size = new System.Drawing.Size(58, 15);
            lblDirection.TabIndex = 5;
            lblDirection.Text = "Direction:";
            // 
            // cmbDirection
            // 
            cmbDirection.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cmbDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDirection.FormattingEnabled = true;
            cmbDirection.Items.AddRange(new object[] { "Next", "Previous" });
            cmbDirection.Location = new System.Drawing.Point(280, 84);
            cmbDirection.Name = "cmbDirection";
            cmbDirection.Size = new System.Drawing.Size(72, 23);
            cmbDirection.TabIndex = 6;
            cmbDirection.SelectionChangeCommitted += cmbDirection_SelectionChangeCommitted;
            // 
            // chkMatchWord
            // 
            chkMatchWord.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkMatchWord.AutoSize = true;
            chkMatchWord.Location = new System.Drawing.Point(206, 121);
            chkMatchWord.Name = "chkMatchWord";
            chkMatchWord.Size = new System.Drawing.Size(125, 19);
            chkMatchWord.TabIndex = 7;
            chkMatchWord.Text = "Match whole word";
            chkMatchWord.UseVisualStyleBackColor = true;
            chkMatchWord.Click += chkMatchWord_Click;
            // 
            // chkMatchCase
            // 
            chkMatchCase.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkMatchCase.AutoSize = true;
            chkMatchCase.Location = new System.Drawing.Point(206, 146);
            chkMatchCase.Name = "chkMatchCase";
            chkMatchCase.Size = new System.Drawing.Size(86, 19);
            chkMatchCase.TabIndex = 8;
            chkMatchCase.Text = "Match case";
            chkMatchCase.UseVisualStyleBackColor = true;
            chkMatchCase.Click += chkMatchCase_Click;
            // 
            // btnFind
            // 
            btnFind.Location = new System.Drawing.Point(381, 13);
            btnFind.Name = "btnFind";
            btnFind.Size = new System.Drawing.Size(102, 25);
            btnFind.TabIndex = 9;
            btnFind.Text = "Find";
            btnFind.UseVisualStyleBackColor = true;
            btnFind.Click += btnFind_Click;
            // 
            // btnReplace
            // 
            btnReplace.Location = new System.Drawing.Point(381, 45);
            btnReplace.Name = "btnReplace";
            btnReplace.Size = new System.Drawing.Size(102, 25);
            btnReplace.TabIndex = 10;
            btnReplace.Text = "Replace";
            btnReplace.UseVisualStyleBackColor = true;
            btnReplace.Click += btnReplace_Click;
            // 
            // btnReplaceAll
            // 
            btnReplaceAll.Location = new System.Drawing.Point(381, 109);
            btnReplaceAll.Name = "btnReplaceAll";
            btnReplaceAll.Size = new System.Drawing.Size(102, 25);
            btnReplaceAll.TabIndex = 11;
            btnReplaceAll.Text = "Replace All";
            btnReplaceAll.UseVisualStyleBackColor = true;
            btnReplaceAll.Click += btnReplaceAll_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnClose.Location = new System.Drawing.Point(381, 141);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(102, 25);
            btnClose.TabIndex = 12;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // cmbFind
            // 
            cmbFind.FormattingEnabled = true;
            cmbFind.Location = new System.Drawing.Point(94, 15);
            cmbFind.Name = "cmbFind";
            cmbFind.Size = new System.Drawing.Size(258, 23);
            cmbFind.TabIndex = 1;
            cmbFind.DropDown += cmbFind_DropDown;
            cmbFind.DropDownClosed += cmbFind_DropDownClosed;
            // 
            // chkSynonyms
            // 
            chkSynonyms.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkSynonyms.AutoSize = true;
            chkSynonyms.Location = new System.Drawing.Point(206, 121);
            chkSynonyms.Name = "chkSynonyms";
            chkSynonyms.Size = new System.Drawing.Size(123, 19);
            chkSynonyms.TabIndex = 13;
            chkSynonyms.Text = "Include Synonyms";
            chkSynonyms.UseVisualStyleBackColor = true;
            chkSynonyms.Visible = false;
            chkSynonyms.Click += chkSynonyms_Click;
            // 
            // txtFind
            // 
            txtFind.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtFind.Location = new System.Drawing.Point(94, 16);
            txtFind.Multiline = true;
            txtFind.Name = "txtFind";
            txtFind.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtFind.Size = new System.Drawing.Size(236, 20);
            txtFind.TabIndex = 14;
            txtFind.TextChanged += txtFind_TextChanged;
            txtFind.Enter += txtFind_Enter;
            // 
            // btnFindAll
            // 
            btnFindAll.Location = new System.Drawing.Point(381, 77);
            btnFindAll.Name = "btnFindAll";
            btnFindAll.Size = new System.Drawing.Size(102, 25);
            btnFindAll.TabIndex = 15;
            btnFindAll.Text = "Find All";
            btnFindAll.UseVisualStyleBackColor = true;
            btnFindAll.Click += btnFindAll_Click;
            // 
            // frmFind
            // 
            AcceptButton = btnFind;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(493, 176);
            Controls.Add(txtFind);
            Controls.Add(chkSynonyms);
            Controls.Add(cmbFind);
            Controls.Add(btnClose);
            Controls.Add(btnReplaceAll);
            Controls.Add(btnReplace);
            Controls.Add(btnFind);
            Controls.Add(chkMatchCase);
            Controls.Add(chkMatchWord);
            Controls.Add(cmbDirection);
            Controls.Add(lblDirection);
            Controls.Add(fraScope);
            Controls.Add(rtfReplace);
            Controls.Add(lblReplace);
            Controls.Add(label1);
            Controls.Add(btnFindAll);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmFind";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Find";
            FormClosing += frmFind_FormClosing;
            HelpRequested += frmFind_HelpRequested;
            fraScope.ResumeLayout(false);
            fraScope.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblReplace;
        private System.Windows.Forms.Label lblDirection;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.ComboBox cmbFind;
        public System.Windows.Forms.TextBox rtfReplace;
        internal System.Windows.Forms.TextBox txtFind;
        private System.Windows.Forms.Button btnFindAll;
        internal System.Windows.Forms.GroupBox fraScope;
        internal System.Windows.Forms.RadioButton optProject;
        internal System.Windows.Forms.RadioButton optOpen;
        internal System.Windows.Forms.RadioButton optCurrent;
        internal System.Windows.Forms.ComboBox cmbDirection;
        internal System.Windows.Forms.CheckBox chkMatchWord;
        internal System.Windows.Forms.CheckBox chkMatchCase;
        internal System.Windows.Forms.CheckBox chkSynonyms;
    }
}