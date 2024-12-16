
namespace WinAGI.Editor {
    partial class frmExportViewLoopOptions {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExportViewLoopOptions));
            picGrid = new System.Windows.Forms.PictureBox();
            picCel = new System.Windows.Forms.PictureBox();
            cmdOK = new System.Windows.Forms.Button();
            cmdCancel = new System.Windows.Forms.Button();
            HScroll1 = new System.Windows.Forms.HScrollBar();
            VScroll1 = new System.Windows.Forms.VScrollBar();
            chkTrans = new System.Windows.Forms.CheckBox();
            chkLoop = new System.Windows.Forms.CheckBox();
            udScale = new System.Windows.Forms.NumericUpDown();
            lblScale = new System.Windows.Forms.Label();
            udDelay = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tbAlignHorizontal = new System.Windows.Forms.ToolStripDropDownButton();
            tbAlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            tbAlignRight = new System.Windows.Forms.ToolStripMenuItem();
            tbAlignVertical = new System.Windows.Forms.ToolStripDropDownButton();
            tbAlignTop = new System.Windows.Forms.ToolStripMenuItem();
            tbAlignBottom = new System.Windows.Forms.ToolStripMenuItem();
            lblAlign = new System.Windows.Forms.Label();
            timer1 = new System.Windows.Forms.Timer(components);
            cmbLoop = new System.Windows.Forms.ComboBox();
            label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)picGrid).BeginInit();
            picGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)udScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)udDelay).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // picGrid
            // 
            picGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picGrid.Controls.Add(picCel);
            picGrid.Location = new System.Drawing.Point(22, 33);
            picGrid.Name = "picGrid";
            picGrid.Size = new System.Drawing.Size(322, 265);
            picGrid.TabIndex = 0;
            picGrid.TabStop = false;
            // 
            // picCel
            // 
            picCel.Location = new System.Drawing.Point(4, 4);
            picCel.Name = "picCel";
            picCel.Size = new System.Drawing.Size(90, 123);
            picCel.TabIndex = 0;
            picCel.TabStop = false;
            // 
            // cmdOK
            // 
            cmdOK.Location = new System.Drawing.Point(371, 297);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(64, 24);
            cmdOK.TabIndex = 1;
            cmdOK.Text = "OK";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // cmdCancel
            // 
            cmdCancel.Location = new System.Drawing.Point(482, 298);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new System.Drawing.Size(61, 23);
            cmdCancel.TabIndex = 2;
            cmdCancel.Text = "Cancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // HScroll1
            // 
            HScroll1.LargeChange = 101;
            HScroll1.Location = new System.Drawing.Point(22, 301);
            HScroll1.Minimum = -4;
            HScroll1.Name = "HScroll1";
            HScroll1.Size = new System.Drawing.Size(320, 23);
            HScroll1.SmallChange = 64;
            HScroll1.TabIndex = 1;
            HScroll1.ValueChanged += HScroll1_ValueChanged;
            // 
            // VScroll1
            // 
            VScroll1.Location = new System.Drawing.Point(346, 33);
            VScroll1.Minimum = -4;
            VScroll1.Name = "VScroll1";
            VScroll1.Size = new System.Drawing.Size(21, 265);
            VScroll1.TabIndex = 3;
            VScroll1.ValueChanged += VScroll1_ValueChanged;
            // 
            // chkTrans
            // 
            chkTrans.AutoSize = true;
            chkTrans.Location = new System.Drawing.Point(393, 97);
            chkTrans.Name = "chkTrans";
            chkTrans.Size = new System.Drawing.Size(95, 19);
            chkTrans.TabIndex = 4;
            chkTrans.Text = "Transparency";
            chkTrans.UseVisualStyleBackColor = true;
            chkTrans.CheckedChanged += chkTrans_CheckedChanged;
            // 
            // chkLoop
            // 
            chkLoop.AutoSize = true;
            chkLoop.Location = new System.Drawing.Point(393, 122);
            chkLoop.Name = "chkLoop";
            chkLoop.Size = new System.Drawing.Size(118, 19);
            chkLoop.TabIndex = 5;
            chkLoop.Text = "Continuous Loop";
            chkLoop.UseVisualStyleBackColor = true;
            chkLoop.CheckedChanged += chkLoop_CheckedChanged;
            // 
            // udScale
            // 
            udScale.Location = new System.Drawing.Point(425, 167);
            udScale.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            udScale.Name = "udScale";
            udScale.Size = new System.Drawing.Size(52, 23);
            udScale.TabIndex = 1;
            udScale.Value = new decimal(new int[] { 1, 0, 0, 0 });
            udScale.ValueChanged += udScale_ValueChanged;
            // 
            // lblScale
            // 
            lblScale.AutoSize = true;
            lblScale.Location = new System.Drawing.Point(371, 169);
            lblScale.Name = "lblScale";
            lblScale.Size = new System.Drawing.Size(37, 15);
            lblScale.TabIndex = 6;
            lblScale.Text = "Scale:";
            // 
            // udDelay
            // 
            udDelay.Location = new System.Drawing.Point(425, 215);
            udDelay.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            udDelay.Name = "udDelay";
            udDelay.Size = new System.Drawing.Size(51, 23);
            udDelay.TabIndex = 7;
            udDelay.Value = new decimal(new int[] { 1, 0, 0, 0 });
            udDelay.ValueChanged += udDelay_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(379, 220);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(39, 15);
            label1.TabIndex = 8;
            label1.Text = "Delay:";
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(482, 215);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(66, 36);
            label2.TabIndex = 9;
            label2.Text = "0.01 sec increments";
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tbAlignHorizontal, tbAlignVertical });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(557, 25);
            toolStrip1.TabIndex = 10;
            toolStrip1.Text = "toolStrip1";
            // 
            // tbAlignHorizontal
            // 
            tbAlignHorizontal.AutoToolTip = false;
            tbAlignHorizontal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbAlignHorizontal.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tbAlignLeft, tbAlignRight });
            tbAlignHorizontal.Image = (System.Drawing.Image)resources.GetObject("tbAlignHorizontal.Image");
            tbAlignHorizontal.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbAlignHorizontal.Name = "tbAlignHorizontal";
            tbAlignHorizontal.Size = new System.Drawing.Size(29, 22);
            // 
            // tbAlignLeft
            // 
            tbAlignLeft.Image = (System.Drawing.Image)resources.GetObject("tbAlignLeft.Image");
            tbAlignLeft.Name = "tbAlignLeft";
            tbAlignLeft.Size = new System.Drawing.Size(102, 22);
            tbAlignLeft.Text = "Left";
            tbAlignLeft.Click += tbAlignLeft_Click;
            // 
            // tbAlignRight
            // 
            tbAlignRight.Image = (System.Drawing.Image)resources.GetObject("tbAlignRight.Image");
            tbAlignRight.Name = "tbAlignRight";
            tbAlignRight.Size = new System.Drawing.Size(102, 22);
            tbAlignRight.Text = "Right";
            tbAlignRight.Click += tbAlignRight_Click;
            // 
            // tbAlignVertical
            // 
            tbAlignVertical.AutoToolTip = false;
            tbAlignVertical.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbAlignVertical.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tbAlignTop, tbAlignBottom });
            tbAlignVertical.Image = (System.Drawing.Image)resources.GetObject("tbAlignVertical.Image");
            tbAlignVertical.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbAlignVertical.Name = "tbAlignVertical";
            tbAlignVertical.Size = new System.Drawing.Size(29, 22);
            // 
            // tbAlignTop
            // 
            tbAlignTop.Image = (System.Drawing.Image)resources.GetObject("tbAlignTop.Image");
            tbAlignTop.Name = "tbAlignTop";
            tbAlignTop.Size = new System.Drawing.Size(114, 22);
            tbAlignTop.Text = "Top";
            tbAlignTop.Click += tbAlignTop_Click;
            // 
            // tbAlignBottom
            // 
            tbAlignBottom.Image = (System.Drawing.Image)resources.GetObject("tbAlignBottom.Image");
            tbAlignBottom.Name = "tbAlignBottom";
            tbAlignBottom.Size = new System.Drawing.Size(114, 22);
            tbAlignBottom.Text = "Bottom";
            tbAlignBottom.Click += tbAlignBottom_Click;
            // 
            // lblAlign
            // 
            lblAlign.AutoSize = true;
            lblAlign.Location = new System.Drawing.Point(220, 9);
            lblAlign.Name = "lblAlign";
            lblAlign.Size = new System.Drawing.Size(115, 15);
            lblAlign.TabIndex = 11;
            lblAlign.Text = "Align: Bottom, Right";
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // cmbLoop
            // 
            cmbLoop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLoop.FormattingEnabled = true;
            cmbLoop.Location = new System.Drawing.Point(393, 50);
            cmbLoop.Name = "cmbLoop";
            cmbLoop.Size = new System.Drawing.Size(150, 23);
            cmbLoop.TabIndex = 12;
            cmbLoop.SelectedIndexChanged += cmbLoop_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(393, 32);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(88, 15);
            label3.TabIndex = 13;
            label3.Text = "Loop to Export:";
            // 
            // frmExportViewLoopOptions
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(557, 333);
            Controls.Add(label3);
            Controls.Add(cmbLoop);
            Controls.Add(lblAlign);
            Controls.Add(toolStrip1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(udDelay);
            Controls.Add(lblScale);
            Controls.Add(udScale);
            Controls.Add(chkLoop);
            Controls.Add(chkTrans);
            Controls.Add(VScroll1);
            Controls.Add(HScroll1);
            Controls.Add(cmdCancel);
            Controls.Add(cmdOK);
            Controls.Add(picGrid);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Name = "frmExportViewLoopOptions";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "frmViewGifOptions";
            FormClosing += frmExportViewLoopOptions_FormClosing;
            ((System.ComponentModel.ISupportInitialize)picGrid).EndInit();
            picGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCel).EndInit();
            ((System.ComponentModel.ISupportInitialize)udScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)udDelay).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox picGrid;
        private System.Windows.Forms.PictureBox picCel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.HScrollBar HScroll1;
        private System.Windows.Forms.VScrollBar VScroll1;
        private System.Windows.Forms.CheckBox chkTrans;
        private System.Windows.Forms.CheckBox chkLoop;
        private System.Windows.Forms.NumericUpDown udScale;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.NumericUpDown udDelay;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Label lblAlign;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripDropDownButton tbAlignHorizontal;
        private System.Windows.Forms.ToolStripMenuItem tbAlignLeft;
        private System.Windows.Forms.ToolStripMenuItem tbAlignRight;
        private System.Windows.Forms.ToolStripDropDownButton tbAlignVertical;
        private System.Windows.Forms.ToolStripMenuItem tbAlignTop;
        private System.Windows.Forms.ToolStripMenuItem tbAlignBottom;
        private System.Windows.Forms.ComboBox cmbLoop;
        private System.Windows.Forms.Label label3;
    }
}