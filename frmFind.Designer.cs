
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
            fraLogic = new System.Windows.Forms.GroupBox();
            optAllGameLogics = new System.Windows.Forms.RadioButton();
            optAllOpenLogics = new System.Windows.Forms.RadioButton();
            optCurrentLogic = new System.Windows.Forms.RadioButton();
            lblDirection = new System.Windows.Forms.Label();
            cmbDirection = new System.Windows.Forms.ComboBox();
            chkMatchWord = new System.Windows.Forms.CheckBox();
            chkMatchCase = new System.Windows.Forms.CheckBox();
            cmdFind = new System.Windows.Forms.Button();
            cmdReplace = new System.Windows.Forms.Button();
            cmdReplaceAll = new System.Windows.Forms.Button();
            cmdCancel = new System.Windows.Forms.Button();
            cmbFind = new System.Windows.Forms.ComboBox();
            fraLogic.SuspendLayout();
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
            lblReplace.AutoSize = true;
            lblReplace.Location = new System.Drawing.Point(11, 51);
            lblReplace.Name = "lblReplace";
            lblReplace.Size = new System.Drawing.Size(77, 15);
            lblReplace.TabIndex = 2;
            lblReplace.Text = "Replace with:";
            // 
            // rtfReplace
            // 
            rtfReplace.Location = new System.Drawing.Point(94, 48);
            rtfReplace.Name = "rtfReplace";
            rtfReplace.Size = new System.Drawing.Size(258, 23);
            rtfReplace.TabIndex = 3;
            rtfReplace.TextChanged += rtfReplace_TextChanged;
            rtfReplace.Enter += rtfReplace_Enter;
            rtfReplace.KeyPress += rtfReplace_KeyPress;
            // 
            // fraLogic
            // 
            fraLogic.Controls.Add(optAllGameLogics);
            fraLogic.Controls.Add(optAllOpenLogics);
            fraLogic.Controls.Add(optCurrentLogic);
            fraLogic.Location = new System.Drawing.Point(12, 77);
            fraLogic.Name = "fraLogic";
            fraLogic.Size = new System.Drawing.Size(162, 92);
            fraLogic.TabIndex = 4;
            fraLogic.TabStop = false;
            fraLogic.Text = "Search";
            // 
            // optAllGameLogics
            // 
            optAllGameLogics.AutoSize = true;
            optAllGameLogics.Location = new System.Drawing.Point(19, 68);
            optAllGameLogics.Name = "optAllGameLogics";
            optAllGameLogics.Size = new System.Drawing.Size(123, 19);
            optAllGameLogics.TabIndex = 2;
            optAllGameLogics.TabStop = true;
            optAllGameLogics.Text = "All Logics in Game";
            optAllGameLogics.UseVisualStyleBackColor = true;
            optAllGameLogics.CheckedChanged += optAllGameLogics_CheckedChanged;
            // 
            // optAllOpenLogics
            // 
            optAllOpenLogics.AutoSize = true;
            optAllOpenLogics.Location = new System.Drawing.Point(19, 44);
            optAllOpenLogics.Name = "optAllOpenLogics";
            optAllOpenLogics.Size = new System.Drawing.Size(97, 19);
            optAllOpenLogics.TabIndex = 1;
            optAllOpenLogics.TabStop = true;
            optAllOpenLogics.Text = "All Open Files";
            optAllOpenLogics.UseVisualStyleBackColor = true;
            optAllOpenLogics.CheckedChanged += optAllOpenLogics_CheckedChanged;
            // 
            // optCurrentLogic
            // 
            optCurrentLogic.AutoSize = true;
            optCurrentLogic.Location = new System.Drawing.Point(19, 20);
            optCurrentLogic.Name = "optCurrentLogic";
            optCurrentLogic.Size = new System.Drawing.Size(97, 19);
            optCurrentLogic.TabIndex = 0;
            optCurrentLogic.TabStop = true;
            optCurrentLogic.Text = "Current Logic";
            optCurrentLogic.UseVisualStyleBackColor = true;
            optCurrentLogic.CheckedChanged += optCurrentLogic_CheckedChanged;
            // 
            // lblDirection
            // 
            lblDirection.AutoSize = true;
            lblDirection.Location = new System.Drawing.Point(206, 87);
            lblDirection.Name = "lblDirection";
            lblDirection.Size = new System.Drawing.Size(58, 15);
            lblDirection.TabIndex = 5;
            lblDirection.Text = "Direction:";
            // 
            // cmbDirection
            // 
            cmbDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDirection.FormattingEnabled = true;
            cmbDirection.Items.AddRange(new object[] { "All", "Down", "Up" });
            cmbDirection.Location = new System.Drawing.Point(280, 84);
            cmbDirection.Name = "cmbDirection";
            cmbDirection.Size = new System.Drawing.Size(72, 23);
            cmbDirection.TabIndex = 6;
            cmbDirection.SelectedIndexChanged += cmbDirection_SelectedIndexChanged;
            // 
            // chkMatchWord
            // 
            chkMatchWord.AutoSize = true;
            chkMatchWord.Location = new System.Drawing.Point(206, 121);
            chkMatchWord.Name = "chkMatchWord";
            chkMatchWord.Size = new System.Drawing.Size(125, 19);
            chkMatchWord.TabIndex = 7;
            chkMatchWord.Text = "Match whole word";
            chkMatchWord.UseVisualStyleBackColor = true;
            chkMatchWord.CheckedChanged += chkMatchWord_CheckedChanged;
            // 
            // chkMatchCase
            // 
            chkMatchCase.AutoSize = true;
            chkMatchCase.Location = new System.Drawing.Point(206, 146);
            chkMatchCase.Name = "chkMatchCase";
            chkMatchCase.Size = new System.Drawing.Size(86, 19);
            chkMatchCase.TabIndex = 8;
            chkMatchCase.Text = "Match case";
            chkMatchCase.UseVisualStyleBackColor = true;
            chkMatchCase.CheckedChanged += chkMatchCase_CheckedChanged;
            // 
            // cmdFind
            // 
            cmdFind.Location = new System.Drawing.Point(381, 13);
            cmdFind.Name = "cmdFind";
            cmdFind.Size = new System.Drawing.Size(102, 25);
            cmdFind.TabIndex = 9;
            cmdFind.Text = "Find";
            cmdFind.UseVisualStyleBackColor = true;
            cmdFind.Click += cmdFind_Click;
            // 
            // cmdReplace
            // 
            cmdReplace.Location = new System.Drawing.Point(381, 46);
            cmdReplace.Name = "cmdReplace";
            cmdReplace.Size = new System.Drawing.Size(102, 25);
            cmdReplace.TabIndex = 10;
            cmdReplace.Text = "Replace";
            cmdReplace.UseVisualStyleBackColor = true;
            cmdReplace.Click += cmdReplace_Click;
            // 
            // cmdReplaceAll
            // 
            cmdReplaceAll.Location = new System.Drawing.Point(382, 79);
            cmdReplaceAll.Name = "cmdReplaceAll";
            cmdReplaceAll.Size = new System.Drawing.Size(102, 25);
            cmdReplaceAll.TabIndex = 11;
            cmdReplaceAll.Text = "Replace All";
            cmdReplaceAll.UseVisualStyleBackColor = true;
            cmdReplaceAll.Click += cmdReplaceAll_Click;
            // 
            // cmdCancel
            // 
            cmdCancel.Location = new System.Drawing.Point(382, 144);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new System.Drawing.Size(102, 25);
            cmdCancel.TabIndex = 12;
            cmdCancel.Text = "Close";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // cmbFind
            // 
            cmbFind.FormattingEnabled = true;
            cmbFind.Location = new System.Drawing.Point(94, 15);
            cmbFind.Name = "cmbFind";
            cmbFind.Size = new System.Drawing.Size(258, 23);
            cmbFind.TabIndex = 1;
            cmbFind.TextChanged += cmbFind_TextChanged;
            cmbFind.Enter += cmbFind_Enter;
            cmbFind.KeyPress += cmbFind_KeyPress;
            // 
            // frmFind
            // 
            AcceptButton = cmdFind;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cmdCancel;
            ClientSize = new System.Drawing.Size(493, 176);
            Controls.Add(cmbFind);
            Controls.Add(cmdCancel);
            Controls.Add(cmdReplaceAll);
            Controls.Add(cmdReplace);
            Controls.Add(cmdFind);
            Controls.Add(chkMatchCase);
            Controls.Add(chkMatchWord);
            Controls.Add(cmbDirection);
            Controls.Add(lblDirection);
            Controls.Add(fraLogic);
            Controls.Add(rtfReplace);
            Controls.Add(lblReplace);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmFind";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Find";
            FormClosing += frmFind_FormClosing;
            fraLogic.ResumeLayout(false);
            fraLogic.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblReplace;
        private System.Windows.Forms.TextBox rtfReplace;
        private System.Windows.Forms.GroupBox fraLogic;
        private System.Windows.Forms.RadioButton optAllGameLogics;
        private System.Windows.Forms.RadioButton optAllOpenLogics;
        private System.Windows.Forms.RadioButton optCurrentLogic;
        private System.Windows.Forms.Label lblDirection;
        private System.Windows.Forms.ComboBox cmbDirection;
        private System.Windows.Forms.CheckBox chkMatchWord;
        private System.Windows.Forms.CheckBox chkMatchCase;
        private System.Windows.Forms.Button cmdFind;
        private System.Windows.Forms.Button cmdReplace;
        private System.Windows.Forms.Button cmdReplaceAll;
        private System.Windows.Forms.Button cmdCancel;
        public System.Windows.Forms.ComboBox cmbFind;
    }
}