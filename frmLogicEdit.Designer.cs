namespace WinAGI_GDS
{
    partial class frmLogicEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogicEdit));
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.mnuLResource = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuECut = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.btnCut = new System.Windows.Forms.ToolStripButton();
      this.btnCopy = new System.Windows.Forms.ToolStripButton();
      this.btnPaste = new System.Windows.Forms.ToolStripButton();
      this.btnDelete = new System.Windows.Forms.ToolStripButton();
      this.btnSep1 = new System.Windows.Forms.ToolStripSeparator();
      this.btnUndo = new System.Windows.Forms.ToolStripButton();
      this.btnRedo = new System.Windows.Forms.ToolStripButton();
      this.btnFind = new System.Windows.Forms.ToolStripButton();
      this.btnSep2 = new System.Windows.Forms.ToolStripSeparator();
      this.btnComment = new System.Windows.Forms.ToolStripButton();
      this.btnUncomment = new System.Windows.Forms.ToolStripButton();
      this.btnSep3 = new System.Windows.Forms.ToolStripSeparator();
      this.btnCompile = new System.Windows.Forms.ToolStripButton();
      this.btnMsgClean = new System.Windows.Forms.ToolStripButton();
      this.rtfLogic = new System.Windows.Forms.RichTextBox();
      this.menuStrip1.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLResource,
            this.mnuEdit});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
      this.menuStrip1.Size = new System.Drawing.Size(431, 19);
      this.menuStrip1.TabIndex = 0;
      this.menuStrip1.Text = "menuStrip1";
      this.menuStrip1.Visible = false;
      // 
      // mnuLResource
      // 
      this.mnuLResource.DoubleClickEnabled = true;
      this.mnuLResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
      this.mnuLResource.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
      this.mnuLResource.MergeIndex = 1;
      this.mnuLResource.Name = "mnuLResource";
      this.mnuLResource.Size = new System.Drawing.Size(72, 17);
      this.mnuLResource.Text = "Resources";
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
      this.toolStripMenuItem1.Text = "logic res1";
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(124, 22);
      this.toolStripMenuItem2.Text = "logic res2";
      // 
      // mnuEdit
      // 
      this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuECut,
            this.mnuECopy,
            this.mnuEPaste});
      this.mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
      this.mnuEdit.MergeIndex = 2;
      this.mnuEdit.Name = "mnuEdit";
      this.mnuEdit.Size = new System.Drawing.Size(48, 17);
      this.mnuEdit.Text = "Edit L";
      // 
      // mnuECut
      // 
      this.mnuECut.Name = "mnuECut";
      this.mnuECut.Size = new System.Drawing.Size(108, 22);
      this.mnuECut.Text = "Cut l";
      // 
      // mnuECopy
      // 
      this.mnuECopy.Name = "mnuECopy";
      this.mnuECopy.Size = new System.Drawing.Size(108, 22);
      this.mnuECopy.Text = "Copy l";
      // 
      // mnuEPaste
      // 
      this.mnuEPaste.Name = "mnuEPaste";
      this.mnuEPaste.Size = new System.Drawing.Size(108, 22);
      this.mnuEPaste.Text = "Paste l";
      // 
      // toolStrip1
      // 
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCut,
            this.btnCopy,
            this.btnPaste,
            this.btnDelete,
            this.btnSep1,
            this.btnUndo,
            this.btnRedo,
            this.btnFind,
            this.btnSep2,
            this.btnComment,
            this.btnUncomment,
            this.btnSep3,
            this.btnCompile,
            this.btnMsgClean});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(548, 39);
      this.toolStrip1.TabIndex = 1;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // btnCut
      // 
      this.btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnCut.Image = ((System.Drawing.Image)(resources.GetObject("btnCut.Image")));
      this.btnCut.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnCut.Name = "btnCut";
      this.btnCut.Size = new System.Drawing.Size(36, 36);
      this.btnCut.Text = "Cut";
      // 
      // btnCopy
      // 
      this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnCopy.Image = ((System.Drawing.Image)(resources.GetObject("btnCopy.Image")));
      this.btnCopy.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.Size = new System.Drawing.Size(36, 36);
      this.btnCopy.Text = "Copy";
      // 
      // btnPaste
      // 
      this.btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnPaste.Image = ((System.Drawing.Image)(resources.GetObject("btnPaste.Image")));
      this.btnPaste.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnPaste.Name = "btnPaste";
      this.btnPaste.Size = new System.Drawing.Size(36, 36);
      this.btnPaste.Text = "Paste";
      // 
      // btnDelete
      // 
      this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
      this.btnDelete.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(36, 36);
      this.btnDelete.Text = "Delete";
      // 
      // btnSep1
      // 
      this.btnSep1.Name = "btnSep1";
      this.btnSep1.Size = new System.Drawing.Size(6, 39);
      // 
      // btnUndo
      // 
      this.btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnUndo.Image = ((System.Drawing.Image)(resources.GetObject("btnUndo.Image")));
      this.btnUndo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnUndo.Name = "btnUndo";
      this.btnUndo.Size = new System.Drawing.Size(36, 36);
      this.btnUndo.Text = "Undo";
      // 
      // btnRedo
      // 
      this.btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnRedo.Image = ((System.Drawing.Image)(resources.GetObject("btnRedo.Image")));
      this.btnRedo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnRedo.Name = "btnRedo";
      this.btnRedo.Size = new System.Drawing.Size(36, 36);
      this.btnRedo.Text = "Redo";
      // 
      // btnFind
      // 
      this.btnFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnFind.Image = ((System.Drawing.Image)(resources.GetObject("btnFind.Image")));
      this.btnFind.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnFind.Name = "btnFind";
      this.btnFind.Size = new System.Drawing.Size(36, 36);
      this.btnFind.Text = "Find";
      // 
      // btnSep2
      // 
      this.btnSep2.Name = "btnSep2";
      this.btnSep2.Size = new System.Drawing.Size(6, 39);
      // 
      // btnComment
      // 
      this.btnComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnComment.Image = ((System.Drawing.Image)(resources.GetObject("btnComment.Image")));
      this.btnComment.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnComment.Name = "btnComment";
      this.btnComment.Size = new System.Drawing.Size(36, 36);
      this.btnComment.Text = "Comment";
      // 
      // btnUncomment
      // 
      this.btnUncomment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnUncomment.Image = ((System.Drawing.Image)(resources.GetObject("btnUncomment.Image")));
      this.btnUncomment.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnUncomment.Name = "btnUncomment";
      this.btnUncomment.Size = new System.Drawing.Size(36, 36);
      this.btnUncomment.Text = "Uncomment";
      // 
      // btnSep3
      // 
      this.btnSep3.Name = "btnSep3";
      this.btnSep3.Size = new System.Drawing.Size(6, 39);
      // 
      // btnCompile
      // 
      this.btnCompile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnCompile.Image = ((System.Drawing.Image)(resources.GetObject("btnCompile.Image")));
      this.btnCompile.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnCompile.Name = "btnCompile";
      this.btnCompile.Size = new System.Drawing.Size(36, 36);
      this.btnCompile.Text = "Compile";
      // 
      // btnMsgClean
      // 
      this.btnMsgClean.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnMsgClean.Image = ((System.Drawing.Image)(resources.GetObject("btnMsgClean.Image")));
      this.btnMsgClean.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
      this.btnMsgClean.Name = "btnMsgClean";
      this.btnMsgClean.Size = new System.Drawing.Size(36, 36);
      this.btnMsgClean.Text = "Message Cleanup";
      // 
      // rtfLogic
      // 
      this.rtfLogic.DetectUrls = false;
      this.rtfLogic.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtfLogic.HideSelection = false;
      this.rtfLogic.Location = new System.Drawing.Point(0, 39);
      this.rtfLogic.Margin = new System.Windows.Forms.Padding(0);
      this.rtfLogic.Name = "rtfLogic";
      this.rtfLogic.ShowSelectionMargin = true;
      this.rtfLogic.Size = new System.Drawing.Size(548, 341);
      this.rtfLogic.TabIndex = 2;
      this.rtfLogic.Text = "";
      this.rtfLogic.WordWrap = false;
      this.rtfLogic.DoubleClick += new System.EventHandler(this.rtfLogic_DoubleClick);
      // 
      // frmLogicEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(548, 380);
      this.Controls.Add(this.rtfLogic);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.menuStrip1);
      this.MainMenuStrip = this.menuStrip1;
      this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.Name = "frmLogicEdit";
      this.Text = "Form1";
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuLResource;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnCut;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnPaste;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripSeparator btnSep1;
        private System.Windows.Forms.ToolStripButton btnUndo;
        private System.Windows.Forms.ToolStripButton btnRedo;
        private System.Windows.Forms.ToolStripButton btnFind;
        private System.Windows.Forms.ToolStripSeparator btnSep2;
        private System.Windows.Forms.ToolStripButton btnComment;
        private System.Windows.Forms.ToolStripButton btnUncomment;
        private System.Windows.Forms.ToolStripSeparator btnSep3;
        private System.Windows.Forms.ToolStripButton btnCompile;
        private System.Windows.Forms.ToolStripButton btnMsgClean;
        public System.Windows.Forms.RichTextBox rtfLogic;
    }
}