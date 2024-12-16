
namespace WinAGI.Editor {
    partial class frmPicEdit {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPicEdit));
            picVisual = new System.Windows.Forms.PictureBox();
            trackBar1 = new System.Windows.Forms.TrackBar();
            cmbTransCol = new System.Windows.Forms.ComboBox();
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
            mnuRBackground = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            button1 = new System.Windows.Forms.Button();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            spStatus = new System.Windows.Forms.ToolStripStatusLabel();
            spCurX = new System.Windows.Forms.ToolStripStatusLabel();
            spCurY = new System.Windows.Forms.ToolStripStatusLabel();
            spScale = new System.Windows.Forms.ToolStripStatusLabel();
            spMode = new System.Windows.Forms.ToolStripStatusLabel();
            spTool = new System.Windows.Forms.ToolStripStatusLabel();
            spAnchor = new System.Windows.Forms.ToolStripStatusLabel();
            spBlock = new System.Windows.Forms.ToolStripStatusLabel();
            spPriBand = new System.Windows.Forms.ToolStripStatusLabel();
            spCapsLock = new System.Windows.Forms.ToolStripStatusLabel();
            spNumLock = new System.Windows.Forms.ToolStripStatusLabel();
            spInsLock = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)picVisual).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // picVisual
            // 
            picVisual.Location = new System.Drawing.Point(12, 12);
            picVisual.Name = "picVisual";
            picVisual.Size = new System.Drawing.Size(640, 336);
            picVisual.TabIndex = 0;
            picVisual.TabStop = false;
            picVisual.Click += picVisual_Click;
            // 
            // trackBar1
            // 
            trackBar1.Location = new System.Drawing.Point(11, 395);
            trackBar1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new System.Drawing.Size(484, 45);
            trackBar1.TabIndex = 2;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // cmbTransCol
            // 
            cmbTransCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbTransCol.FormattingEnabled = true;
            cmbTransCol.Location = new System.Drawing.Point(530, 397);
            cmbTransCol.Name = "cmbTransCol";
            cmbTransCol.Size = new System.Drawing.Size(121, 23);
            cmbTransCol.TabIndex = 3;
            cmbTransCol.SelectionChangeCommitted += cmbTransCol_SelectionChangeCommitted;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 4;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRExport, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportLoopGIF, mnuRBackground });
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
            mnuROpenRes.Size = new System.Drawing.Size(274, 22);
            mnuROpenRes.Text = "open res";
            // 
            // mnuRSave
            // 
            mnuRSave.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSave.MergeIndex = 4;
            mnuRSave.Name = "mnuRSave";
            mnuRSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuRSave.Size = new System.Drawing.Size(274, 22);
            mnuRSave.Text = "&Save Picture";
            mnuRSave.Click += mnuRSave_Click;
            // 
            // mnuRExport
            // 
            mnuRExport.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRExport.MergeIndex = 5;
            mnuRExport.Name = "mnuRExport";
            mnuRExport.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            mnuRExport.Size = new System.Drawing.Size(274, 22);
            mnuRExport.Text = "export res";
            mnuRExport.Click += mnuRExport_Click;
            // 
            // mnuRInGame
            // 
            mnuRInGame.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRInGame.MergeIndex = 7;
            mnuRInGame.Name = "mnuRInGame";
            mnuRInGame.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.A;
            mnuRInGame.Size = new System.Drawing.Size(274, 22);
            mnuRInGame.Text = "ToggleInGame";
            mnuRInGame.Click += mnuRInGame_Click;
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRRenumber.MergeIndex = 8;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.N;
            mnuRRenumber.Size = new System.Drawing.Size(274, 22);
            mnuRRenumber.Text = "Renumber Picture";
            mnuRRenumber.Click += mnuRRenumber_Click;
            // 
            // mnuRProperties
            // 
            mnuRProperties.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRProperties.MergeIndex = 9;
            mnuRProperties.Name = "mnuRProperties";
            mnuRProperties.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D;
            mnuRProperties.Size = new System.Drawing.Size(274, 22);
            mnuRProperties.Text = "I&D/Description ...";
            mnuRProperties.Click += mnuRProperties_Click;
            // 
            // mnuRCompile
            // 
            mnuRCompile.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRCompile.MergeIndex = 11;
            mnuRCompile.Name = "mnuRCompile";
            mnuRCompile.Size = new System.Drawing.Size(274, 22);
            mnuRCompile.Text = "compilelogic";
            // 
            // mnuRSavePicImage
            // 
            mnuRSavePicImage.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSavePicImage.MergeIndex = 11;
            mnuRSavePicImage.Name = "mnuRSavePicImage";
            mnuRSavePicImage.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            mnuRSavePicImage.Size = new System.Drawing.Size(274, 22);
            mnuRSavePicImage.Text = "S&ave Picture Image As ...";
            mnuRSavePicImage.Click += mnuRSavePicImage_Click;
            // 
            // mnuRExportLoopGIF
            // 
            mnuRExportLoopGIF.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRExportLoopGIF.MergeIndex = 12;
            mnuRExportLoopGIF.Name = "mnuRExportLoopGIF";
            mnuRExportLoopGIF.Size = new System.Drawing.Size(274, 22);
            mnuRExportLoopGIF.Text = "export loop gif";
            // 
            // mnuRBackground
            // 
            mnuRBackground.Name = "mnuRBackground";
            mnuRBackground.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.B;
            mnuRBackground.Size = new System.Drawing.Size(274, 22);
            mnuRBackground.Text = "Background";
            mnuRBackground.Click += mnuRBackground_Click;
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
            // button1
            // 
            button1.Location = new System.Drawing.Point(26, 368);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(59, 23);
            button1.TabIndex = 5;
            button1.Text = "Clear";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { spScale, spMode, spTool, spAnchor, spBlock, spStatus, spCurX, spCurY, spPriBand, spCapsLock, spNumLock, spInsLock });
            statusStrip1.Location = new System.Drawing.Point(41, 214);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            statusStrip1.Size = new System.Drawing.Size(719, 23);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.Visible = false;
            // 
            // spStatus
            // 
            spStatus.MergeAction = System.Windows.Forms.MergeAction.Replace;
            spStatus.MergeIndex = 5;
            spStatus.Name = "spStatus";
            spStatus.Size = new System.Drawing.Size(224, 18);
            spStatus.Spring = true;
            spStatus.Text = "pic status";
            spStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spCurX
            // 
            spCurX.AutoSize = false;
            spCurX.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spCurX.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spCurX.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spCurX.MergeIndex = 6;
            spCurX.Name = "spCurX";
            spCurX.Size = new System.Drawing.Size(70, 18);
            spCurX.Text = "picX";
            // 
            // spCurY
            // 
            spCurY.AutoSize = false;
            spCurY.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spCurY.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spCurY.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spCurY.MergeIndex = 7;
            spCurY.Name = "spCurY";
            spCurY.Size = new System.Drawing.Size(70, 18);
            spCurY.Text = "picY";
            // 
            // spScale
            // 
            spScale.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spScale.MergeIndex = 0;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(52, 18);
            spScale.Text = "pic scale";
            // 
            // spMode
            // 
            spMode.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spMode.MergeIndex = 1;
            spMode.Name = "spMode";
            spMode.Size = new System.Drawing.Size(57, 18);
            spMode.Text = "pic mode";
            // 
            // spTool
            // 
            spTool.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spTool.MergeIndex = 2;
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(47, 18);
            spTool.Text = "pic tool";
            // 
            // spAnchor
            // 
            spAnchor.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spAnchor.MergeIndex = 3;
            spAnchor.Name = "spAnchor";
            spAnchor.Size = new System.Drawing.Size(63, 18);
            spAnchor.Text = "pic anchor";
            // 
            // spBlock
            // 
            spBlock.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spBlock.MergeIndex = 4;
            spBlock.Name = "spBlock";
            spBlock.Size = new System.Drawing.Size(55, 18);
            spBlock.Text = "pic block";
            // 
            // spPriBand
            // 
            spPriBand.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spPriBand.MergeIndex = 8;
            spPriBand.Name = "spPriBand";
            spPriBand.Size = new System.Drawing.Size(67, 18);
            spPriBand.Text = "pic priband";
            // 
            // spCapsLock
            // 
            spCapsLock.MergeAction = System.Windows.Forms.MergeAction.Remove;
            spCapsLock.MergeIndex = 9;
            spCapsLock.Name = "spCapsLock";
            spCapsLock.Size = new System.Drawing.Size(0, 18);
            // 
            // spNumLock
            // 
            spNumLock.MergeAction = System.Windows.Forms.MergeAction.Remove;
            spNumLock.MergeIndex = 9;
            spNumLock.Name = "spNumLock";
            spNumLock.Size = new System.Drawing.Size(0, 18);
            // 
            // spInsLock
            // 
            spInsLock.MergeAction = System.Windows.Forms.MergeAction.Remove;
            spInsLock.MergeIndex = 9;
            spInsLock.Name = "spInsLock";
            spInsLock.Size = new System.Drawing.Size(0, 18);
            // 
            // frmPicEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(statusStrip1);
            Controls.Add(button1);
            Controls.Add(menuStrip1);
            Controls.Add(cmbTransCol);
            Controls.Add(trackBar1);
            Controls.Add(picVisual);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmPicEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmPicEdit";
            FormClosing += frmPicEdit_FormClosing;
            FormClosed += frmPicEdit_FormClosed;
            Load += frmPicEdit_Load;
            ((System.ComponentModel.ISupportInitialize)picVisual).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox picVisual;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.ComboBox cmbTransCol;
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
        private System.Windows.Forms.ToolStripMenuItem mnuRBackground;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel spStatus;
        private System.Windows.Forms.ToolStripStatusLabel spCurX;
        private System.Windows.Forms.ToolStripStatusLabel spCurY;
        private System.Windows.Forms.ToolStripStatusLabel spScale;
        private System.Windows.Forms.ToolStripStatusLabel spMode;
        private System.Windows.Forms.ToolStripStatusLabel spTool;
        private System.Windows.Forms.ToolStripStatusLabel spAnchor;
        private System.Windows.Forms.ToolStripStatusLabel spBlock;
        private System.Windows.Forms.ToolStripStatusLabel spPriBand;
        private System.Windows.Forms.ToolStripStatusLabel spCapsLock;
        private System.Windows.Forms.ToolStripStatusLabel spNumLock;
        private System.Windows.Forms.ToolStripStatusLabel spInsLock;
    }
}