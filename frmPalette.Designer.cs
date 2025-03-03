
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
            picPalette = new System.Windows.Forms.PictureBox();
            cmdLoad = new System.Windows.Forms.Button();
            cmdSave = new System.Windows.Forms.Button();
            cmdDefColors = new System.Windows.Forms.Button();
            cmdOK = new System.Windows.Forms.Button();
            cmdCancel = new System.Windows.Forms.Button();
            picColChange = new System.Windows.Forms.PictureBox();
            label1 = new System.Windows.Forms.Label();
            cmdColorDlg = new System.Windows.Forms.Button();
            lblCurColor = new System.Windows.Forms.Label();
            cdColors = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)picPalette).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picColChange).BeginInit();
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
            // cmdLoad
            // 
            cmdLoad.Location = new System.Drawing.Point(217, 12);
            cmdLoad.Name = "cmdLoad";
            cmdLoad.Size = new System.Drawing.Size(200, 39);
            cmdLoad.TabIndex = 1;
            cmdLoad.Text = "Load Palette";
            cmdLoad.UseVisualStyleBackColor = true;
            cmdLoad.Click += cmdLoad_Click;
            // 
            // cmdSave
            // 
            cmdSave.Location = new System.Drawing.Point(217, 65);
            cmdSave.Name = "cmdSave";
            cmdSave.Size = new System.Drawing.Size(200, 39);
            cmdSave.TabIndex = 2;
            cmdSave.Text = "Save Palette";
            cmdSave.UseVisualStyleBackColor = true;
            cmdSave.Click += cmdSave_Click;
            // 
            // cmdDefColors
            // 
            cmdDefColors.Location = new System.Drawing.Point(217, 118);
            cmdDefColors.Name = "cmdDefColors";
            cmdDefColors.Size = new System.Drawing.Size(200, 39);
            cmdDefColors.TabIndex = 3;
            cmdDefColors.Text = "Restore Default Palette";
            cmdDefColors.UseVisualStyleBackColor = true;
            cmdDefColors.Click += cmdDefColors_Click;
            // 
            // cmdOK
            // 
            cmdOK.Enabled = false;
            cmdOK.Location = new System.Drawing.Point(18, 265);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(80, 29);
            cmdOK.TabIndex = 4;
            cmdOK.Text = "OK";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // cmdCancel
            // 
            cmdCancel.Location = new System.Drawing.Point(118, 265);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new System.Drawing.Size(80, 29);
            cmdCancel.TabIndex = 5;
            cmdCancel.Text = "Cancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
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
            // cmdColorDlg
            // 
            cmdColorDlg.Enabled = false;
            cmdColorDlg.Location = new System.Drawing.Point(146, 212);
            cmdColorDlg.Name = "cmdColorDlg";
            cmdColorDlg.Size = new System.Drawing.Size(30, 30);
            cmdColorDlg.TabIndex = 8;
            cmdColorDlg.Text = "...";
            cmdColorDlg.UseVisualStyleBackColor = true;
            cmdColorDlg.Click += cmdColorDlg_Click;
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
            // frmPalette
            // 
            AcceptButton = cmdOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cmdCancel;
            ClientSize = new System.Drawing.Size(427, 310);
            ControlBox = false;
            Controls.Add(lblCurColor);
            Controls.Add(cmdColorDlg);
            Controls.Add(label1);
            Controls.Add(picColChange);
            Controls.Add(cmdCancel);
            Controls.Add(cmdOK);
            Controls.Add(cmdDefColors);
            Controls.Add(cmdSave);
            Controls.Add(cmdLoad);
            Controls.Add(picPalette);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Name = "frmPalette";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Modify Color Palette";
            ((System.ComponentModel.ISupportInitialize)picPalette).EndInit();
            ((System.ComponentModel.ISupportInitialize)picColChange).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox picPalette;
        private System.Windows.Forms.Button cmdLoad;
        private System.Windows.Forms.Button cmdSave;
        private System.Windows.Forms.Button cmdDefColors;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.PictureBox picColChange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdColorDlg;
        private System.Windows.Forms.Label lblCurColor;
        private System.Windows.Forms.ColorDialog cdColors;
    }
}