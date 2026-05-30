
namespace WinAGI.Editor {
    partial class frmPalette {
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
            picPalette = new System.Windows.Forms.PictureBox();
            btnLoad = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            btnDefColors = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            picColChange = new System.Windows.Forms.PictureBox();
            label1 = new System.Windows.Forms.Label();
            btnColorDlg = new System.Windows.Forms.Button();
            lblCurColor = new System.Windows.Forms.Label();
            cdColors = new System.Windows.Forms.ColorDialog();
            lbl24Bit = new System.Windows.Forms.Label();
            lbl18Bit = new System.Windows.Forms.Label();
            txt24Bit = new System.Windows.Forms.TextBox();
            txt18Bit = new System.Windows.Forms.TextBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuPaste = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)picPalette).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picColChange).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // picPalette
            // 
            picPalette.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            picPalette.Location = new System.Drawing.Point(10, 10);
            picPalette.Name = "picPalette";
            picPalette.Size = new System.Drawing.Size(196, 196);
            picPalette.TabIndex = 0;
            picPalette.TabStop = false;
            picPalette.Paint += picPalette_Paint;
            picPalette.MouseDoubleClick += picPalette_DoubleClick;
            picPalette.MouseDown += picPalette_MouseDown;
            // 
            // btnLoad
            // 
            btnLoad.Location = new System.Drawing.Point(217, 12);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new System.Drawing.Size(200, 39);
            btnLoad.TabIndex = 1;
            btnLoad.Text = "Load Palette";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new System.Drawing.Point(217, 65);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(200, 39);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save Palette";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnDefColors
            // 
            btnDefColors.Location = new System.Drawing.Point(217, 118);
            btnDefColors.Name = "btnDefColors";
            btnDefColors.Size = new System.Drawing.Size(200, 39);
            btnDefColors.TabIndex = 3;
            btnDefColors.Text = "Restore Default Palette";
            btnDefColors.UseVisualStyleBackColor = true;
            btnDefColors.Click += btnDefColors_Click;
            // 
            // btnOK
            // 
            btnOK.Enabled = false;
            btnOK.Location = new System.Drawing.Point(18, 265);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(80, 29);
            btnOK.TabIndex = 4;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(118, 265);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(80, 29);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // picColChange
            // 
            picColChange.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            picColChange.Location = new System.Drawing.Point(75, 212);
            picColChange.Name = "picColChange";
            picColChange.Size = new System.Drawing.Size(66, 30);
            picColChange.TabIndex = 6;
            picColChange.TabStop = false;
            picColChange.Paint += picColChange_Paint;
            picColChange.DoubleClick += picColChange_DoubleClick;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(10, 216);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(65, 22);
            label1.TabIndex = 7;
            label1.Text = "Default:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnColorDlg
            // 
            btnColorDlg.Enabled = false;
            btnColorDlg.Location = new System.Drawing.Point(146, 212);
            btnColorDlg.Name = "btnColorDlg";
            btnColorDlg.Size = new System.Drawing.Size(30, 30);
            btnColorDlg.TabIndex = 8;
            btnColorDlg.Text = "...";
            btnColorDlg.UseVisualStyleBackColor = true;
            btnColorDlg.Click += btnColorDlg_Click;
            // 
            // lblCurColor
            // 
            lblCurColor.Location = new System.Drawing.Point(10, 245);
            lblCurColor.Name = "lblCurColor";
            lblCurColor.Size = new System.Drawing.Size(196, 15);
            lblCurColor.TabIndex = 9;
            lblCurColor.Text = " ";
            lblCurColor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl24Bit
            // 
            lbl24Bit.AutoSize = true;
            lbl24Bit.Location = new System.Drawing.Point(270, 192);
            lbl24Bit.Name = "lbl24Bit";
            lbl24Bit.Size = new System.Drawing.Size(99, 15);
            lbl24Bit.TabIndex = 10;
            lbl24Bit.Text = "24 Bit Color Value";
            // 
            // lbl18Bit
            // 
            lbl18Bit.AutoSize = true;
            lbl18Bit.Location = new System.Drawing.Point(270, 246);
            lbl18Bit.Name = "lbl18Bit";
            lbl18Bit.Size = new System.Drawing.Size(99, 15);
            lbl18Bit.TabIndex = 11;
            lbl18Bit.Text = "18 Bit Color Value";
            // 
            // txt24Bit
            // 
            txt24Bit.BackColor = System.Drawing.SystemColors.Window;
            txt24Bit.Font = new System.Drawing.Font("Courier New", 12F);
            txt24Bit.Location = new System.Drawing.Point(271, 210);
            txt24Bit.Name = "txt24Bit";
            txt24Bit.ReadOnly = true;
            txt24Bit.Size = new System.Drawing.Size(97, 26);
            txt24Bit.TabIndex = 12;
            txt24Bit.Text = "000000";
            txt24Bit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txt18Bit
            // 
            txt18Bit.BackColor = System.Drawing.SystemColors.Window;
            txt18Bit.Font = new System.Drawing.Font("Courier New", 12F);
            txt18Bit.Location = new System.Drawing.Point(270, 264);
            txt18Bit.Name = "txt18Bit";
            txt18Bit.ReadOnly = true;
            txt18Bit.Size = new System.Drawing.Size(97, 26);
            txt18Bit.TabIndex = 13;
            txt18Bit.Text = "1F1F1F";
            txt18Bit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuCopy, mnuPaste });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(240, 70);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // mnuCopy
            // 
            mnuCopy.Name = "mnuCopy";
            mnuCopy.Size = new System.Drawing.Size(239, 22);
            mnuCopy.Text = "Copy Power Pack Palette String";
            mnuCopy.Click += mnuCopy_Click;
            // 
            // mnuPaste
            // 
            mnuPaste.Name = "mnuPaste";
            mnuPaste.Size = new System.Drawing.Size(239, 22);
            mnuPaste.Text = "Paste Power Pack Palette String";
            mnuPaste.Click += mnuPaste_Click;
            // 
            // frmPalette
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(427, 310);
            ControlBox = false;
            Controls.Add(txt18Bit);
            Controls.Add(txt24Bit);
            Controls.Add(lbl18Bit);
            Controls.Add(lbl24Bit);
            Controls.Add(lblCurColor);
            Controls.Add(btnColorDlg);
            Controls.Add(label1);
            Controls.Add(picColChange);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(btnDefColors);
            Controls.Add(btnSave);
            Controls.Add(btnLoad);
            Controls.Add(picPalette);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            KeyPreview = true;
            Name = "frmPalette";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Modify Color Palette";
            HelpRequested += frmPalette_HelpRequested;
            KeyDown += frmPalette_KeyDown;
            ((System.ComponentModel.ISupportInitialize)picPalette).EndInit();
            ((System.ComponentModel.ISupportInitialize)picColChange).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox picPalette;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDefColors;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox picColChange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnColorDlg;
        private System.Windows.Forms.Label lblCurColor;
        private System.Windows.Forms.ColorDialog cdColors;
        private System.Windows.Forms.Label lbl24Bit;
        private System.Windows.Forms.Label lbl18Bit;
        private System.Windows.Forms.TextBox txt24Bit;
        private System.Windows.Forms.TextBox txt18Bit;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuPaste;
    }
}