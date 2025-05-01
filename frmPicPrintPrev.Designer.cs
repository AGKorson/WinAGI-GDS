namespace WinAGI.Editor {
    partial class frmPicPrintPrev {
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
            components = new System.ComponentModel.Container();
            label1 = new System.Windows.Forms.Label();
            txtMessage = new System.Windows.Forms.TextBox();
            cmMsg = new System.Windows.Forms.ContextMenuStrip(components);
            cmiUndo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            cmiCut = new System.Windows.Forms.ToolStripMenuItem();
            cmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            cmiPaste = new System.Windows.Forms.ToolStripMenuItem();
            cmiDelete = new System.Windows.Forms.ToolStripMenuItem();
            cmiCharMap = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            cmiSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            optPrint = new System.Windows.Forms.RadioButton();
            optPrintAt = new System.Windows.Forms.RadioButton();
            optDisplay = new System.Windows.Forms.RadioButton();
            rtfLine1 = new System.Windows.Forms.TextBox();
            cmdCopy = new System.Windows.Forms.Button();
            cmdOK = new System.Windows.Forms.Button();
            cmdCancel = new System.Windows.Forms.Button();
            lblRow = new System.Windows.Forms.Label();
            lblCol = new System.Windows.Forms.Label();
            lblMW = new System.Windows.Forms.Label();
            lblOffset = new System.Windows.Forms.Label();
            txtOffset = new NumericTextBox();
            txtMW = new NumericTextBox();
            txtCol = new NumericTextBox();
            txtRow = new NumericTextBox();
            cmbFG = new System.Windows.Forms.ComboBox();
            lblFG = new System.Windows.Forms.Label();
            lblBG = new System.Windows.Forms.Label();
            cmbBG = new System.Windows.Forms.ComboBox();
            rtfLine2 = new System.Windows.Forms.TextBox();
            cmMsg.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(15, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(56, 15);
            label1.TabIndex = 0;
            label1.Text = "Message:";
            // 
            // txtMessage
            // 
            txtMessage.ContextMenuStrip = cmMsg;
            txtMessage.Location = new System.Drawing.Point(12, 28);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtMessage.Size = new System.Drawing.Size(435, 57);
            txtMessage.TabIndex = 1;
            txtMessage.TextChanged += txtMessage_TextChanged;
            // 
            // cmMsg
            // 
            cmMsg.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cmiUndo, toolStripSeparator1, cmiCut, cmiCopy, cmiPaste, cmiDelete, cmiCharMap, toolStripSeparator2, cmiSelectAll });
            cmMsg.Name = "cmMsg";
            cmMsg.Size = new System.Drawing.Size(176, 170);
            cmMsg.Closed += cmMsg_Closed;
            cmMsg.Opening += cmMsg_Opening;
            // 
            // cmiUndo
            // 
            cmiUndo.Name = "cmiUndo";
            cmiUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U;
            cmiUndo.Size = new System.Drawing.Size(175, 22);
            cmiUndo.Text = "Undo";
            cmiUndo.Click += cmiUndo_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(172, 6);
            // 
            // cmiCut
            // 
            cmiCut.Name = "cmiCut";
            cmiCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            cmiCut.Size = new System.Drawing.Size(175, 22);
            cmiCut.Text = "Cut";
            cmiCut.Click += cmiCut_Click;
            // 
            // cmiCopy
            // 
            cmiCopy.Name = "cmiCopy";
            cmiCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            cmiCopy.Size = new System.Drawing.Size(175, 22);
            cmiCopy.Text = "Copy";
            cmiCopy.Click += cmiCopy_Click;
            // 
            // cmiPaste
            // 
            cmiPaste.Name = "cmiPaste";
            cmiPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            cmiPaste.Size = new System.Drawing.Size(175, 22);
            cmiPaste.Text = "Paste";
            cmiPaste.Click += cmiPaste_Click;
            // 
            // cmiDelete
            // 
            cmiDelete.Name = "cmiDelete";
            cmiDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            cmiDelete.Size = new System.Drawing.Size(175, 22);
            cmiDelete.Text = "Delete";
            cmiDelete.Click += cmiDelete_Click;
            // 
            // cmiCharMap
            // 
            cmiCharMap.Name = "cmiCharMap";
            cmiCharMap.ShortcutKeyDisplayString = "Ctrl+Ins";
            cmiCharMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert;
            cmiCharMap.Size = new System.Drawing.Size(175, 22);
            cmiCharMap.Text = "Char Map";
            cmiCharMap.Click += cmiCharMap_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(172, 6);
            // 
            // cmiSelectAll
            // 
            cmiSelectAll.Name = "cmiSelectAll";
            cmiSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            cmiSelectAll.Size = new System.Drawing.Size(175, 22);
            cmiSelectAll.Text = "Select All";
            cmiSelectAll.Click += cmiSelectAll_Click;
            // 
            // optPrint
            // 
            optPrint.AutoSize = true;
            optPrint.Checked = true;
            optPrint.Location = new System.Drawing.Point(13, 91);
            optPrint.Name = "optPrint";
            optPrint.Size = new System.Drawing.Size(50, 19);
            optPrint.TabIndex = 2;
            optPrint.TabStop = true;
            optPrint.Text = "print";
            optPrint.UseVisualStyleBackColor = true;
            optPrint.CheckedChanged += optPrint_CheckedChanged;
            // 
            // optPrintAt
            // 
            optPrintAt.AutoSize = true;
            optPrintAt.Location = new System.Drawing.Point(13, 114);
            optPrintAt.Name = "optPrintAt";
            optPrintAt.Size = new System.Drawing.Size(63, 19);
            optPrintAt.TabIndex = 3;
            optPrintAt.Text = "print.at";
            optPrintAt.UseVisualStyleBackColor = true;
            optPrintAt.CheckedChanged += optPrintAt_CheckedChanged;
            // 
            // optDisplay
            // 
            optDisplay.AutoSize = true;
            optDisplay.Location = new System.Drawing.Point(13, 137);
            optDisplay.Name = "optDisplay";
            optDisplay.Size = new System.Drawing.Size(62, 19);
            optDisplay.TabIndex = 4;
            optDisplay.Text = "display";
            optDisplay.UseVisualStyleBackColor = true;
            optDisplay.CheckedChanged += optDisplay_CheckedChanged;
            // 
            // rtfLine1
            // 
            rtfLine1.BackColor = System.Drawing.SystemColors.Control;
            rtfLine1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtfLine1.Font = new System.Drawing.Font("Consolas", 12F);
            rtfLine1.Location = new System.Drawing.Point(11, 179);
            rtfLine1.Name = "rtfLine1";
            rtfLine1.Size = new System.Drawing.Size(436, 19);
            rtfLine1.TabIndex = 17;
            rtfLine1.Text = "line 1";
            // 
            // cmdCopy
            // 
            cmdCopy.Location = new System.Drawing.Point(11, 243);
            cmdCopy.Name = "cmdCopy";
            cmdCopy.Size = new System.Drawing.Size(128, 29);
            cmdCopy.TabIndex = 18;
            cmdCopy.Text = "Copy Command";
            cmdCopy.UseVisualStyleBackColor = true;
            cmdCopy.Click += cmdCopy_Click;
            // 
            // cmdOK
            // 
            cmdOK.Location = new System.Drawing.Point(375, 243);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(75, 29);
            cmdOK.TabIndex = 20;
            cmdOK.Text = "OK";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // cmdCancel
            // 
            cmdCancel.Location = new System.Drawing.Point(294, 243);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new System.Drawing.Size(75, 29);
            cmdCancel.TabIndex = 19;
            cmdCancel.Text = "Cancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // lblRow
            // 
            lblRow.AutoSize = true;
            lblRow.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblRow.Location = new System.Drawing.Point(119, 102);
            lblRow.Name = "lblRow";
            lblRow.Size = new System.Drawing.Size(23, 21);
            lblRow.TabIndex = 5;
            lblRow.Text = "R:";
            lblRow.Visible = false;
            // 
            // lblCol
            // 
            lblCol.AutoSize = true;
            lblCol.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblCol.Location = new System.Drawing.Point(210, 102);
            lblCol.Name = "lblCol";
            lblCol.Size = new System.Drawing.Size(23, 21);
            lblCol.TabIndex = 7;
            lblCol.Text = "C:";
            lblCol.Visible = false;
            // 
            // lblMW
            // 
            lblMW.AutoSize = true;
            lblMW.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblMW.Location = new System.Drawing.Point(321, 102);
            lblMW.Name = "lblMW";
            lblMW.Size = new System.Drawing.Size(86, 21);
            lblMW.TabIndex = 9;
            lblMW.Text = "Min Width:";
            lblMW.Visible = false;
            // 
            // lblOffset
            // 
            lblOffset.AutoSize = true;
            lblOffset.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblOffset.Location = new System.Drawing.Point(334, 102);
            lblOffset.Name = "lblOffset";
            lblOffset.Size = new System.Drawing.Size(79, 21);
            lblOffset.TabIndex = 11;
            lblOffset.Text = "Pic Offset:";
            lblOffset.Visible = false;
            // 
            // txtOffset
            // 
            txtOffset.Font = new System.Drawing.Font("Segoe UI", 12F);
            txtOffset.Location = new System.Drawing.Point(413, 99);
            txtOffset.MaxValue = 4;
            txtOffset.MinValue = 0;
            txtOffset.Name = "txtOffset";
            txtOffset.Size = new System.Drawing.Size(33, 29);
            txtOffset.TabIndex = 12;
            txtOffset.Text = "4";
            txtOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtOffset.Value = 4;
            txtOffset.Visible = false;
            txtOffset.TextChanged += TextBox_TextChanged;
            txtOffset.Validating += txtOffset_Validating;
            // 
            // txtMW
            // 
            txtMW.Font = new System.Drawing.Font("Segoe UI", 12F);
            txtMW.Location = new System.Drawing.Point(406, 99);
            txtMW.MaxValue = 39;
            txtMW.MinValue = 0;
            txtMW.Name = "txtMW";
            txtMW.Size = new System.Drawing.Size(38, 29);
            txtMW.TabIndex = 10;
            txtMW.Text = "0";
            txtMW.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtMW.Value = 0;
            txtMW.Visible = false;
            txtMW.TextChanged += TextBox_TextChanged;
            txtMW.Validating += txtMW_Validating;
            // 
            // txtCol
            // 
            txtCol.Font = new System.Drawing.Font("Segoe UI", 12F);
            txtCol.Location = new System.Drawing.Point(237, 99);
            txtCol.MaxValue = 39;
            txtCol.MinValue = 0;
            txtCol.Name = "txtCol";
            txtCol.Size = new System.Drawing.Size(38, 29);
            txtCol.TabIndex = 8;
            txtCol.Text = "0";
            txtCol.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtCol.Value = 0;
            txtCol.Visible = false;
            txtCol.TextChanged += TextBox_TextChanged;
            txtCol.Validating += txtCol_Validating;
            // 
            // txtRow
            // 
            txtRow.Font = new System.Drawing.Font("Segoe UI", 12F);
            txtRow.Location = new System.Drawing.Point(146, 99);
            txtRow.MaxLength = 2;
            txtRow.MaxValue = 24;
            txtRow.MinValue = 0;
            txtRow.Name = "txtRow";
            txtRow.Size = new System.Drawing.Size(38, 29);
            txtRow.TabIndex = 6;
            txtRow.Text = "0";
            txtRow.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtRow.Value = 0;
            txtRow.Visible = false;
            txtRow.TextChanged += TextBox_TextChanged;
            txtRow.Validating += txtRow_Validating;
            // 
            // cmbFG
            // 
            cmbFG.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbFG.FormattingEnabled = true;
            cmbFG.Items.AddRange(new object[] { "Black", "Blue", "Green", "Cyan", "Red", "Magenta", "Brown", "Lt Gray", "Dk Gray", "Lt Blue", "Lt Green", "Lt Cyan", "Lt Red", "Lt Magenta", "Yellow", "White" });
            cmbFG.Location = new System.Drawing.Point(157, 141);
            cmbFG.Name = "cmbFG";
            cmbFG.Size = new System.Drawing.Size(118, 23);
            cmbFG.TabIndex = 14;
            cmbFG.Visible = false;
            cmbFG.SelectedIndexChanged += cmbFG_SelectedIndexChanged;
            // 
            // lblFG
            // 
            lblFG.AutoSize = true;
            lblFG.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblFG.Location = new System.Drawing.Point(119, 141);
            lblFG.Name = "lblFG";
            lblFG.Size = new System.Drawing.Size(32, 21);
            lblFG.TabIndex = 13;
            lblFG.Text = "FG:";
            lblFG.Visible = false;
            // 
            // lblBG
            // 
            lblBG.AutoSize = true;
            lblBG.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblBG.Location = new System.Drawing.Point(295, 141);
            lblBG.Name = "lblBG";
            lblBG.Size = new System.Drawing.Size(33, 21);
            lblBG.TabIndex = 15;
            lblBG.Text = "BG:";
            lblBG.Visible = false;
            // 
            // cmbBG
            // 
            cmbBG.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbBG.FormattingEnabled = true;
            cmbBG.Items.AddRange(new object[] { "Black", "Blue", "Green", "Cyan", "Red", "Magenta", "Brown", "Lt Gray", "Dk Gray", "Lt Blue", "Lt Green", "Lt Cyan", "Lt Red", "Lt Magenta", "Yellow", "White" });
            cmbBG.Location = new System.Drawing.Point(328, 141);
            cmbBG.Name = "cmbBG";
            cmbBG.Size = new System.Drawing.Size(118, 23);
            cmbBG.TabIndex = 16;
            cmbBG.Visible = false;
            cmbBG.SelectedIndexChanged += cmbBG_SelectedIndexChanged;
            // 
            // rtfLine2
            // 
            rtfLine2.BackColor = System.Drawing.SystemColors.Control;
            rtfLine2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtfLine2.Font = new System.Drawing.Font("Consolas", 12F);
            rtfLine2.Location = new System.Drawing.Point(12, 204);
            rtfLine2.Name = "rtfLine2";
            rtfLine2.Size = new System.Drawing.Size(436, 19);
            rtfLine2.TabIndex = 21;
            rtfLine2.Text = "line 2";
            // 
            // frmPicPrintPrev
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(457, 280);
            ControlBox = false;
            Controls.Add(rtfLine2);
            Controls.Add(cmbBG);
            Controls.Add(lblBG);
            Controls.Add(lblFG);
            Controls.Add(cmbFG);
            Controls.Add(txtRow);
            Controls.Add(txtCol);
            Controls.Add(txtMW);
            Controls.Add(txtOffset);
            Controls.Add(lblOffset);
            Controls.Add(lblMW);
            Controls.Add(lblCol);
            Controls.Add(lblRow);
            Controls.Add(cmdCancel);
            Controls.Add(cmdOK);
            Controls.Add(cmdCopy);
            Controls.Add(rtfLine1);
            Controls.Add(optDisplay);
            Controls.Add(optPrintAt);
            Controls.Add(optPrint);
            Controls.Add(txtMessage);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPicPrintPrev";
            ShowInTaskbar = false;
            Text = "Picture Editor Print Test Options";
            cmMsg.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox txtMessage;
        public System.Windows.Forms.RadioButton optPrint;
        public System.Windows.Forms.RadioButton optPrintAt;
        public System.Windows.Forms.RadioButton optDisplay;
        private System.Windows.Forms.Button cmdCopy;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblRow;
        private System.Windows.Forms.Label lblCol;
        private System.Windows.Forms.Label lblMW;
        private System.Windows.Forms.Label lblOffset;
        public WinAGI.Editor.NumericTextBox txtOffset;
        public WinAGI.Editor.NumericTextBox txtMW;
        public WinAGI.Editor.NumericTextBox txtCol;
        public WinAGI.Editor.NumericTextBox txtRow;
        public System.Windows.Forms.ComboBox cmbFG;
        private System.Windows.Forms.Label lblFG;
        private System.Windows.Forms.Label lblBG;
        public System.Windows.Forms.ComboBox cmbBG;
        public System.Windows.Forms.TextBox rtfLine1;
        public System.Windows.Forms.TextBox rtfLine2;
        private System.Windows.Forms.ContextMenuStrip cmMsg;
        private System.Windows.Forms.ToolStripMenuItem cmiUndo;
        private System.Windows.Forms.ToolStripMenuItem cmiCut;
        private System.Windows.Forms.ToolStripMenuItem cmiCopy;
        private System.Windows.Forms.ToolStripMenuItem cmiPaste;
        private System.Windows.Forms.ToolStripMenuItem cmiDelete;
        private System.Windows.Forms.ToolStripMenuItem cmiCharMap;
        private System.Windows.Forms.ToolStripMenuItem cmiSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}