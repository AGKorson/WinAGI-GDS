using System;
using System.Windows.Forms;

namespace WinAGI.Editor {
    partial class frmCharPicker {
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
            picSelect = new SelectablePictureBox();
            picInsert = new SelectablePictureBox();
            btnInsert = new Button();
            btnCancel = new Button();
            tmrCursor = new Timer(components);
            hoverTimer = new Timer(components);
            lblTip = new Label();
            ((System.ComponentModel.ISupportInitialize)picSelect).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picInsert).BeginInit();
            SuspendLayout();
            // 
            // picSelect
            // 
            picSelect.BackColor = System.Drawing.Color.White;
            picSelect.Location = new System.Drawing.Point(0, 0);
            picSelect.Name = "picSelect";
            picSelect.Size = new System.Drawing.Size(337, 337);
            picSelect.TabIndex = 0;
            picSelect.KeyDown += picSelect_KeyDown;
            picSelect.Leave += picSelect_Leave;
            picSelect.Paint += picSelect_Paint;
            picSelect.MouseClick += picSelect_MouseClick;
            picSelect.MouseDoubleClick += picSelect_MouseDoubleClick;
            picSelect.MouseMove += picSelect_MouseMove;
            // 
            // picInsert
            // 
            picInsert.BackColor = System.Drawing.Color.White;
            picInsert.Cursor = Cursors.IBeam;
            picInsert.Location = new System.Drawing.Point(7, 343);
            picInsert.Name = "picInsert";
            picInsert.Size = new System.Drawing.Size(322, 24);
            picInsert.TabIndex = 1;
            picInsert.Paint += picInsert_Paint;
            picInsert.KeyDown += picInsert_KeyDown;
            picInsert.KeyPress += picInsert_KeyPress;
            picInsert.MouseDown += picInsert_MouseDown;
            picInsert.MouseMove += picInsert_MouseMove;
            picInsert.MouseUp += picInsert_MouseUp;
            // 
            // btnInsert
            // 
            btnInsert.Location = new System.Drawing.Point(169, 373);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new System.Drawing.Size(75, 24);
            btnInsert.TabIndex = 2;
            btnInsert.Text = "Insert";
            btnInsert.UseVisualStyleBackColor = true;
            btnInsert.Click += btnInsert_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(250, 373);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 24);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // tmrCursor
            // 
            tmrCursor.Interval = 500;
            tmrCursor.Tick += tmrCursor_Tick;
            tmrCursor.Enabled = true;
            // 
            // hoverTimer
            // 
            hoverTimer.Interval = 750;
            hoverTimer.Tick += HoverTimer_Tick;
            // 
            // lblTip
            // 
            lblTip.AutoSize = true;
            lblTip.BackColor = System.Drawing.SystemColors.Info;
            lblTip.BorderStyle = BorderStyle.FixedSingle;
            lblTip.Location = new System.Drawing.Point(50, 50);
            lblTip.Name = "lblTip";
            lblTip.Size = new System.Drawing.Size(32, 17);
            lblTip.TabIndex = 4;
            lblTip.Text = "0x00";
            // 
            // frmCharPicker
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(337, 404);
            ControlBox = false;
            Controls.Add(lblTip);
            Controls.Add(btnCancel);
            Controls.Add(btnInsert);
            Controls.Add(picInsert);
            Controls.Add(picSelect);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCharPicker";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "frmCharPicker";
            Load += frmCharPicker_Load;
            ((System.ComponentModel.ISupportInitialize)picSelect).EndInit();
            ((System.ComponentModel.ISupportInitialize)picInsert).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SelectablePictureBox picSelect;
        private SelectablePictureBox picInsert;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Timer tmrCursor;
        private System.Windows.Forms.Timer hoverTimer;
        private Label lblTip;
    }
}