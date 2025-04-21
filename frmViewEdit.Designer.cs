
namespace WinAGI.Editor {
    partial class frmViewEdit {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmViewEdit));
            picCel = new System.Windows.Forms.PictureBox();
            chkTrans = new System.Windows.Forms.CheckBox();
            cmbLoop = new System.Windows.Forms.ComboBox();
            cmbCel = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            timer1 = new System.Windows.Forms.Timer(components);
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            mnuResource = new System.Windows.Forms.ToolStripMenuItem();
            mnuROpenRes = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSave = new System.Windows.Forms.ToolStripMenuItem();
            mnuRExport = new System.Windows.Forms.ToolStripMenuItem();
            mnuRInGame = new System.Windows.Forms.ToolStripMenuItem();
            mnuRRenumber = new System.Windows.Forms.ToolStripMenuItem();
            mnuRProperties = new System.Windows.Forms.ToolStripMenuItem();
            mnuRCompile = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSavePicImage = new System.Windows.Forms.ToolStripMenuItem();
            mnuRExportLoopGIF = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)picCel).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // picCel
            // 
            picCel.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            picCel.Location = new System.Drawing.Point(41, 32);
            picCel.Name = "picCel";
            picCel.Size = new System.Drawing.Size(640, 336);
            picCel.TabIndex = 0;
            picCel.TabStop = false;
            // 
            // chkTrans
            // 
            chkTrans.AutoSize = true;
            chkTrans.Location = new System.Drawing.Point(498, 389);
            chkTrans.Name = "chkTrans";
            chkTrans.Size = new System.Drawing.Size(96, 19);
            chkTrans.TabIndex = 1;
            chkTrans.Text = "Transparency";
            chkTrans.UseVisualStyleBackColor = true;
            chkTrans.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // cmbLoop
            // 
            cmbLoop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLoop.FormattingEnabled = true;
            cmbLoop.Location = new System.Drawing.Point(171, 394);
            cmbLoop.Name = "cmbLoop";
            cmbLoop.Size = new System.Drawing.Size(107, 23);
            cmbLoop.TabIndex = 2;
            cmbLoop.SelectedIndexChanged += cmbLoop_SelectedIndexChanged;
            // 
            // cmbCel
            // 
            cmbCel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbCel.FormattingEnabled = true;
            cmbCel.Location = new System.Drawing.Point(309, 394);
            cmbCel.Name = "cmbCel";
            cmbCel.Size = new System.Drawing.Size(107, 23);
            cmbCel.TabIndex = 3;
            cmbCel.SelectedIndexChanged += cmbCel_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(171, 376);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(34, 15);
            label2.TabIndex = 6;
            label2.Text = "Loop";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(309, 376);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(24, 15);
            label3.TabIndex = 7;
            label3.Text = "Cel";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(470, 416);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(69, 26);
            button1.TabIndex = 8;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // timer1
            // 
            timer1.Interval = 75;
            timer1.Tick += timer1_Tick;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 9;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRExport, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportLoopGIF });
            mnuResource.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            mnuResource.MergeIndex = 1;
            mnuResource.Name = "mnuResource";
            mnuResource.Size = new System.Drawing.Size(67, 22);
            mnuResource.Text = "Resource";
            mnuResource.Visible = false;
            // 
            // mnuROpenRes
            // 
            mnuROpenRes.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuROpenRes.MergeIndex = 4;
            mnuROpenRes.Name = "mnuROpenRes";
            mnuROpenRes.Size = new System.Drawing.Size(225, 22);
            mnuROpenRes.Text = "open res";
            // 
            // mnuRSave
            // 
            mnuRSave.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSave.MergeIndex = 4;
            mnuRSave.Name = "mnuRSave";
            mnuRSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuRSave.Size = new System.Drawing.Size(225, 22);
            mnuRSave.Text = "&Save View";
            mnuRSave.Click += mnuRSave_Click;
            // 
            // mnuRExport
            // 
            mnuRExport.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRExport.MergeIndex = 5;
            mnuRExport.Name = "mnuRExport";
            mnuRExport.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            mnuRExport.Size = new System.Drawing.Size(225, 22);
            mnuRExport.Text = "export res";
            mnuRExport.Click += mnuRExport_Click;
            // 
            // mnuRInGame
            // 
            mnuRInGame.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRInGame.MergeIndex = 7;
            mnuRInGame.Name = "mnuRInGame";
            mnuRInGame.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.A;
            mnuRInGame.Size = new System.Drawing.Size(225, 22);
            mnuRInGame.Text = "ToggleInGame";
            mnuRInGame.Click += mnuRInGame_Click;
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRRenumber.MergeIndex = 8;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.N;
            mnuRRenumber.Size = new System.Drawing.Size(225, 22);
            mnuRRenumber.Text = "Renumber Picture";
            mnuRRenumber.Click += mnuRRenumber_Click;
            // 
            // mnuRProperties
            // 
            mnuRProperties.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRProperties.MergeIndex = 9;
            mnuRProperties.Name = "mnuRProperties";
            mnuRProperties.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D;
            mnuRProperties.Size = new System.Drawing.Size(225, 22);
            mnuRProperties.Text = "I&D/Description ...";
            mnuRProperties.Click += mnuRProperties_Click;
            // 
            // mnuRCompile
            // 
            mnuRCompile.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRCompile.MergeIndex = 11;
            mnuRCompile.Name = "mnuRCompile";
            mnuRCompile.Size = new System.Drawing.Size(225, 22);
            mnuRCompile.Text = "compilelogic";
            // 
            // mnuRSavePicImage
            // 
            mnuRSavePicImage.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRSavePicImage.MergeIndex = 11;
            mnuRSavePicImage.Name = "mnuRSavePicImage";
            mnuRSavePicImage.Size = new System.Drawing.Size(225, 22);
            mnuRSavePicImage.Text = "save pic image";
            // 
            // mnuRExportLoopGIF
            // 
            mnuRExportLoopGIF.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRExportLoopGIF.MergeIndex = 11;
            mnuRExportLoopGIF.Name = "mnuRExportLoopGIF";
            mnuRExportLoopGIF.Size = new System.Drawing.Size(225, 22);
            mnuRExportLoopGIF.Text = "Export Loop As GIF";
            mnuRExportLoopGIF.Click += mnuRExportLoopGIF_Click;
            // 
            // mnuEdit
            // 
            mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuECut, mnuECopy, mnuEPaste });
            mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            mnuEdit.MergeIndex = 2;
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new System.Drawing.Size(39, 22);
            mnuEdit.Text = "&Edit";
            // 
            // mnuECut
            // 
            mnuECut.Name = "mnuECut";
            mnuECut.Size = new System.Drawing.Size(108, 22);
            mnuECut.Text = "Cut l";
            // 
            // mnuECopy
            // 
            mnuECopy.Name = "mnuECopy";
            mnuECopy.Size = new System.Drawing.Size(108, 22);
            mnuECopy.Text = "Copy l";
            // 
            // mnuEPaste
            // 
            mnuEPaste.Name = "mnuEPaste";
            mnuEPaste.Size = new System.Drawing.Size(108, 22);
            mnuEPaste.Text = "Paste l";
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(57, 381);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(62, 41);
            button2.TabIndex = 10;
            button2.Text = "clear";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // frmViewEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(button2);
            Controls.Add(menuStrip1);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(cmbCel);
            Controls.Add(cmbLoop);
            Controls.Add(chkTrans);
            Controls.Add(picCel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmViewEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmViewEdit";
            FormClosing += frmViewEdit_FormClosing;
            FormClosed += frmViewEdit_FormClosed;
            Load += frmViewEdit_Load;
            ((System.ComponentModel.ISupportInitialize)picCel).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox picCel;
        private System.Windows.Forms.CheckBox chkTrans;
        private System.Windows.Forms.ComboBox cmbLoop;
        private System.Windows.Forms.ComboBox cmbCel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuResource;
        private System.Windows.Forms.ToolStripMenuItem mnuROpenRes;
        private System.Windows.Forms.ToolStripMenuItem mnuRSave;
        private System.Windows.Forms.ToolStripMenuItem mnuRExport;
        private System.Windows.Forms.ToolStripMenuItem mnuRInGame;
        private System.Windows.Forms.ToolStripMenuItem mnuRRenumber;
        private System.Windows.Forms.ToolStripMenuItem mnuRProperties;
        private System.Windows.Forms.ToolStripMenuItem mnuRCompile;
        private System.Windows.Forms.ToolStripMenuItem mnuRSavePicImage;
        private System.Windows.Forms.ToolStripMenuItem mnuRExportLoopGIF;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.Button button2;
    }
}