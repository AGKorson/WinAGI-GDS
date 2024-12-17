
namespace WinAGI.Editor {
    partial class frmWordsEdit {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWordsEdit));
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
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
            mnuRMerge = new System.Windows.Forms.ToolStripMenuItem();
            mnuRGroupCheck = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            lstGroups = new System.Windows.Forms.ListBox();
            lstWords = new System.Windows.Forms.ListBox();
            btnClear = new System.Windows.Forms.Button();
            lblGroupCount = new System.Windows.Forms.Label();
            lblWordCount = new System.Windows.Forms.Label();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            spGroupCount = new System.Windows.Forms.ToolStripStatusLabel();
            spWordCount = new System.Windows.Forms.ToolStripStatusLabel();
            spStatus = new System.Windows.Forms.ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(48, 15);
            label1.TabIndex = 0;
            label1.Text = "Groups:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(168, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(126, 15);
            label2.TabIndex = 1;
            label2.Text = "Synonyms for Group #";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 6;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRExport, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportLoopGIF, mnuRMerge, mnuRGroupCheck });
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
            mnuRSave.Text = "&Save WORDS.TOK";
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
            mnuRInGame.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRInGame.MergeIndex = 7;
            mnuRInGame.Name = "mnuRInGame";
            mnuRInGame.Size = new System.Drawing.Size(274, 22);
            mnuRInGame.Text = "toggle ingame";
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRenumber.MergeIndex = 7;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.Size = new System.Drawing.Size(274, 22);
            mnuRRenumber.Text = "renumber";
            // 
            // mnuRProperties
            // 
            mnuRProperties.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRProperties.MergeIndex = 7;
            mnuRProperties.Name = "mnuRProperties";
            mnuRProperties.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D;
            mnuRProperties.Size = new System.Drawing.Size(274, 22);
            mnuRProperties.Text = "Description ...";
            mnuRProperties.Click += mnuRProperties_Click;
            // 
            // mnuRCompile
            // 
            mnuRCompile.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRCompile.MergeIndex = 9;
            mnuRCompile.Name = "mnuRCompile";
            mnuRCompile.Size = new System.Drawing.Size(274, 22);
            mnuRCompile.Text = "compilelogic";
            // 
            // mnuRSavePicImage
            // 
            mnuRSavePicImage.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRSavePicImage.MergeIndex = 9;
            mnuRSavePicImage.Name = "mnuRSavePicImage";
            mnuRSavePicImage.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            mnuRSavePicImage.Size = new System.Drawing.Size(274, 22);
            mnuRSavePicImage.Text = "S&ave Picture Image As ...";
            // 
            // mnuRExportLoopGIF
            // 
            mnuRExportLoopGIF.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRExportLoopGIF.MergeIndex = 9;
            mnuRExportLoopGIF.Name = "mnuRExportLoopGIF";
            mnuRExportLoopGIF.Size = new System.Drawing.Size(274, 22);
            mnuRExportLoopGIF.Text = "export loop gif";
            // 
            // mnuRMerge
            // 
            mnuRMerge.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuRMerge.Name = "mnuRMerge";
            mnuRMerge.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F;
            mnuRMerge.Size = new System.Drawing.Size(274, 22);
            mnuRMerge.Text = "Merge From File ...";
            mnuRMerge.Click += mnuRMerge_Click;
            // 
            // mnuRGroupCheck
            // 
            mnuRGroupCheck.Name = "mnuRGroupCheck";
            mnuRGroupCheck.Size = new System.Drawing.Size(274, 22);
            mnuRGroupCheck.Text = "Unused Word Group Check";
            mnuRGroupCheck.Click += mnuRGroupCheck_Click;
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
            // lstGroups
            // 
            lstGroups.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lstGroups.FormattingEnabled = true;
            lstGroups.HorizontalScrollbar = true;
            lstGroups.ItemHeight = 15;
            lstGroups.Location = new System.Drawing.Point(12, 27);
            lstGroups.Name = "lstGroups";
            lstGroups.Size = new System.Drawing.Size(189, 274);
            lstGroups.TabIndex = 7;
            lstGroups.SelectedIndexChanged += lstGroups_SelectedIndexChanged;
            // 
            // lstWords
            // 
            lstWords.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lstWords.FormattingEnabled = true;
            lstWords.HorizontalScrollbar = true;
            lstWords.ItemHeight = 15;
            lstWords.Location = new System.Drawing.Point(214, 27);
            lstWords.Name = "lstWords";
            lstWords.Size = new System.Drawing.Size(189, 274);
            lstWords.TabIndex = 8;
            // 
            // btnClear
            // 
            btnClear.Location = new System.Drawing.Point(473, 27);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(90, 24);
            btnClear.TabIndex = 9;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // lblGroupCount
            // 
            lblGroupCount.AutoSize = true;
            lblGroupCount.Location = new System.Drawing.Point(474, 71);
            lblGroupCount.Name = "lblGroupCount";
            lblGroupCount.Size = new System.Drawing.Size(89, 15);
            lblGroupCount.TabIndex = 10;
            lblGroupCount.Text = "Group Count: #";
            // 
            // lblWordCount
            // 
            lblWordCount.AutoSize = true;
            lblWordCount.Location = new System.Drawing.Point(473, 97);
            lblWordCount.Name = "lblWordCount";
            lblWordCount.Size = new System.Drawing.Size(75, 15);
            lblWordCount.TabIndex = 11;
            lblWordCount.Text = "Word Count:";
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { spGroupCount, spWordCount, spStatus });
            statusStrip1.Location = new System.Drawing.Point(-3, 144);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            statusStrip1.Size = new System.Drawing.Size(719, 23);
            statusStrip1.TabIndex = 12;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.Visible = false;
            // 
            // spGroupCount
            // 
            spGroupCount.AutoSize = false;
            spGroupCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spGroupCount.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spGroupCount.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spGroupCount.MergeIndex = 0;
            spGroupCount.Name = "spGroupCount";
            spGroupCount.Size = new System.Drawing.Size(140, 18);
            spGroupCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spWordCount
            // 
            spWordCount.AutoSize = false;
            spWordCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            spWordCount.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            spWordCount.MergeAction = System.Windows.Forms.MergeAction.Insert;
            spWordCount.MergeIndex = 1;
            spWordCount.Name = "spWordCount";
            spWordCount.Size = new System.Drawing.Size(140, 18);
            spWordCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spStatus
            // 
            spStatus.MergeAction = System.Windows.Forms.MergeAction.Replace;
            spStatus.MergeIndex = 2;
            spStatus.Name = "spStatus";
            spStatus.Size = new System.Drawing.Size(425, 18);
            spStatus.Spring = true;
            spStatus.Text = "word edit status";
            spStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmWordsEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(713, 311);
            Controls.Add(statusStrip1);
            Controls.Add(lblWordCount);
            Controls.Add(lblGroupCount);
            Controls.Add(btnClear);
            Controls.Add(lstWords);
            Controls.Add(lstGroups);
            Controls.Add(menuStrip1);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmWordsEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmWordsEdit";
            FormClosing += frmWordsEdit_FormClosing;
            FormClosed += frmWordsEdit_FormClosed;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
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
        private System.Windows.Forms.ToolStripMenuItem mnuRMerge;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuRGroupCheck;
        private System.Windows.Forms.ListBox lstGroups;
        private System.Windows.Forms.ListBox lstWords;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblGroupCount;
        private System.Windows.Forms.Label lblWordCount;
        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel spStatus;
        private System.Windows.Forms.ToolStripStatusLabel spGroupCount;
        private System.Windows.Forms.ToolStripStatusLabel spWordCount;
    }
}