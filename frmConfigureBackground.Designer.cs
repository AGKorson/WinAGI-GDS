namespace WinAGI.Editor {
    partial class frmConfigureBackground {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConfigureBackground));
            pnlBackSurface = new System.Windows.Forms.Panel();
            picCorner = new System.Windows.Forms.PictureBox();
            HScroll1 = new System.Windows.Forms.HScrollBar();
            VScroll1 = new System.Windows.Forms.VScrollBar();
            picExample = new TransparentPictureBox();
            picBackground = new System.Windows.Forms.PictureBox();
            cmdCancel = new System.Windows.Forms.Button();
            cmdOK = new System.Windows.Forms.Button();
            cmdFull = new System.Windows.Forms.Button();
            cmdStretch = new System.Windows.Forms.Button();
            sldTrans = new System.Windows.Forms.TrackBar();
            label1 = new System.Windows.Forms.Label();
            chkVisual = new System.Windows.Forms.CheckBox();
            chkPriority = new System.Windows.Forms.CheckBox();
            txtTransparency = new System.Windows.Forms.TextBox();
            cmdLoad = new System.Windows.Forms.Button();
            chkDefaultVis = new System.Windows.Forms.CheckBox();
            udScale = new System.Windows.Forms.DomainUpDown();
            pnlBackSurface.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCorner).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picExample).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBackground).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sldTrans).BeginInit();
            SuspendLayout();
            // 
            // pnlBackSurface
            // 
            pnlBackSurface.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnlBackSurface.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlBackSurface.Controls.Add(picCorner);
            pnlBackSurface.Controls.Add(HScroll1);
            pnlBackSurface.Controls.Add(VScroll1);
            pnlBackSurface.Controls.Add(picExample);
            pnlBackSurface.Controls.Add(picBackground);
            pnlBackSurface.Location = new System.Drawing.Point(10, 9);
            pnlBackSurface.Name = "pnlBackSurface";
            pnlBackSurface.Size = new System.Drawing.Size(481, 349);
            pnlBackSurface.TabIndex = 0;
            // 
            // picCorner
            // 
            picCorner.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            picCorner.Location = new System.Drawing.Point(463, 331);
            picCorner.Name = "picCorner";
            picCorner.Size = new System.Drawing.Size(16, 16);
            picCorner.TabIndex = 8;
            picCorner.TabStop = false;
            picCorner.Visible = false;
            // 
            // HScroll1
            // 
            HScroll1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            HScroll1.Location = new System.Drawing.Point(1, 331);
            HScroll1.Name = "HScroll1";
            HScroll1.Size = new System.Drawing.Size(356, 16);
            HScroll1.TabIndex = 1;
            HScroll1.Visible = false;
            HScroll1.Scroll += HScroll1_Scroll;
            // 
            // VScroll1
            // 
            VScroll1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            VScroll1.Location = new System.Drawing.Point(463, 1);
            VScroll1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            VScroll1.Name = "VScroll1";
            VScroll1.Size = new System.Drawing.Size(16, 176);
            VScroll1.TabIndex = 0;
            VScroll1.Visible = false;
            VScroll1.Scroll += VScroll1_Scroll;
            // 
            // picExample
            // 
            picExample.BackColor = System.Drawing.Color.Transparent;
            picExample.Location = new System.Drawing.Point(73, 90);
            picExample.Name = "picExample";
            picExample.Opacity = 0;
            picExample.Size = new System.Drawing.Size(320, 168);
            picExample.TabIndex = 12;
            picExample.TabStop = false;
            picExample.MouseDown += picExample_MouseDown;
            picExample.MouseMove += picExample_MouseMove;
            picExample.MouseUp += picExample_MouseUp;
            // 
            // picBackground
            // 
            picBackground.Location = new System.Drawing.Point(29, 29);
            picBackground.Name = "picBackground";
            picBackground.Size = new System.Drawing.Size(410, 287);
            picBackground.TabIndex = 0;
            picBackground.TabStop = false;
            picBackground.MouseDown += picBackground_MouseDown;
            picBackground.MouseMove += picBackground_MouseMove;
            picBackground.MouseUp += picBackground_MouseUp;
            // 
            // cmdCancel
            // 
            cmdCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cmdCancel.Location = new System.Drawing.Point(512, 353);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new System.Drawing.Size(107, 30);
            cmdCancel.TabIndex = 11;
            cmdCancel.Text = "Cancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // cmdOK
            // 
            cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cmdOK.Location = new System.Drawing.Point(512, 317);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(107, 30);
            cmdOK.TabIndex = 10;
            cmdOK.Text = "OK";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // cmdFull
            // 
            cmdFull.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cmdFull.Location = new System.Drawing.Point(512, 81);
            cmdFull.Name = "cmdFull";
            cmdFull.Size = new System.Drawing.Size(107, 30);
            cmdFull.TabIndex = 6;
            cmdFull.Text = "Full Size";
            cmdFull.UseVisualStyleBackColor = true;
            cmdFull.Click += cmdFull_Click;
            // 
            // cmdStretch
            // 
            cmdStretch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cmdStretch.Location = new System.Drawing.Point(512, 45);
            cmdStretch.Name = "cmdStretch";
            cmdStretch.Size = new System.Drawing.Size(107, 30);
            cmdStretch.TabIndex = 5;
            cmdStretch.Text = "Stretch to Fit";
            cmdStretch.UseVisualStyleBackColor = true;
            cmdStretch.Click += cmdStretch_Click;
            // 
            // sldTrans
            // 
            sldTrans.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            sldTrans.LargeChange = 10;
            sldTrans.Location = new System.Drawing.Point(545, 139);
            sldTrans.Maximum = 100;
            sldTrans.Name = "sldTrans";
            sldTrans.Orientation = System.Windows.Forms.Orientation.Vertical;
            sldTrans.Size = new System.Drawing.Size(45, 132);
            sldTrans.TabIndex = 8;
            sldTrans.TickFrequency = 10;
            sldTrans.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            sldTrans.Value = 50;
            sldTrans.Scroll += sldTrans_Scroll;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(525, 124);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(80, 15);
            label1.TabIndex = 7;
            label1.Text = "Transparency:";
            // 
            // chkVisual
            // 
            chkVisual.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            chkVisual.AutoSize = true;
            chkVisual.Checked = true;
            chkVisual.CheckState = System.Windows.Forms.CheckState.Checked;
            chkVisual.Location = new System.Drawing.Point(73, 364);
            chkVisual.Name = "chkVisual";
            chkVisual.Size = new System.Drawing.Size(102, 19);
            chkVisual.TabIndex = 1;
            chkVisual.Text = "Show in Visual";
            chkVisual.UseVisualStyleBackColor = true;
            chkVisual.CheckedChanged += chkVisual_CheckedChanged;
            // 
            // chkPriority
            // 
            chkPriority.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            chkPriority.AutoSize = true;
            chkPriority.Location = new System.Drawing.Point(179, 364);
            chkPriority.Name = "chkPriority";
            chkPriority.Size = new System.Drawing.Size(109, 19);
            chkPriority.TabIndex = 2;
            chkPriority.Text = "Show in Priority";
            chkPriority.UseVisualStyleBackColor = true;
            chkPriority.CheckedChanged += chkPriority_CheckedChanged;
            // 
            // txtTransparency
            // 
            txtTransparency.Location = new System.Drawing.Point(545, 270);
            txtTransparency.Name = "txtTransparency";
            txtTransparency.Size = new System.Drawing.Size(45, 23);
            txtTransparency.TabIndex = 9;
            txtTransparency.Text = "50";
            txtTransparency.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtTransparency.Enter += txtTransparency_Enter;
            txtTransparency.KeyDown += txtTransparency_KeyDown;
            txtTransparency.KeyPress += txtTransparency_KeyPress;
            txtTransparency.Leave += txtTransparency_Leave;
            txtTransparency.Validating += txtTransparency_Validating;
            // 
            // cmdLoad
            // 
            cmdLoad.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cmdLoad.Location = new System.Drawing.Point(512, 9);
            cmdLoad.Name = "cmdLoad";
            cmdLoad.Size = new System.Drawing.Size(107, 30);
            cmdLoad.TabIndex = 4;
            cmdLoad.Text = "Load Image";
            cmdLoad.UseVisualStyleBackColor = true;
            cmdLoad.Click += cmdLoad_Click;
            // 
            // chkDefaultVis
            // 
            chkDefaultVis.AutoSize = true;
            chkDefaultVis.Location = new System.Drawing.Point(295, 364);
            chkDefaultVis.Name = "chkDefaultVis";
            chkDefaultVis.Size = new System.Drawing.Size(203, 19);
            chkDefaultVis.TabIndex = 3;
            chkDefaultVis.Text = "Default Colors Aways Transparent";
            chkDefaultVis.UseVisualStyleBackColor = true;
            chkDefaultVis.Click += chkDefaultVis_Click;
            // 
            // udScale
            // 
            udScale.Items.Add("300%");
            udScale.Items.Add("200%");
            udScale.Items.Add("100%");
            udScale.Location = new System.Drawing.Point(12, 364);
            udScale.Name = "udScale";
            udScale.ReadOnly = true;
            udScale.Size = new System.Drawing.Size(52, 23);
            udScale.TabIndex = 12;
            udScale.Text = "100%";
            udScale.SelectedItemChanged += udScale_SelectedItemChanged;
            // 
            // frmConfigureBackground
            // 
            AcceptButton = cmdOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cmdCancel;
            ClientSize = new System.Drawing.Size(628, 392);
            Controls.Add(udScale);
            Controls.Add(chkDefaultVis);
            Controls.Add(txtTransparency);
            Controls.Add(chkPriority);
            Controls.Add(chkVisual);
            Controls.Add(label1);
            Controls.Add(sldTrans);
            Controls.Add(cmdStretch);
            Controls.Add(cmdFull);
            Controls.Add(cmdOK);
            Controls.Add(cmdCancel);
            Controls.Add(cmdLoad);
            Controls.Add(pnlBackSurface);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmConfigureBackground";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "frmConfigureBackground";
            VisibleChanged += frmConfigureBackground_VisibleChanged;
            pnlBackSurface.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCorner).EndInit();
            ((System.ComponentModel.ISupportInitialize)picExample).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBackground).EndInit();
            ((System.ComponentModel.ISupportInitialize)sldTrans).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlBackSurface;
        private System.Windows.Forms.PictureBox picBackground;
        private System.Windows.Forms.PictureBox picCorner;
        private System.Windows.Forms.VScrollBar VScroll1;
        private System.Windows.Forms.HScrollBar HScroll1;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdFull;
        private System.Windows.Forms.Button cmdStretch;
        private System.Windows.Forms.TrackBar sldTrans;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkVisual;
        private System.Windows.Forms.CheckBox chkPriority;
        private System.Windows.Forms.TextBox txtTransparency;
        private System.Windows.Forms.Button cmdLoad;
        private TransparentPictureBox picExample;
        private System.Windows.Forms.CheckBox chkDefaultVis;
        private System.Windows.Forms.DomainUpDown udScale;
    }
}