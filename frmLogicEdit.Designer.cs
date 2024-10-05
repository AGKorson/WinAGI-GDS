namespace WinAGI.Editor
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
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogicEdit));
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            mnuLResource = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnCut = new System.Windows.Forms.ToolStripButton();
            btnCopy = new System.Windows.Forms.ToolStripButton();
            btnPaste = new System.Windows.Forms.ToolStripButton();
            btnDelete = new System.Windows.Forms.ToolStripButton();
            btnSep1 = new System.Windows.Forms.ToolStripSeparator();
            btnUndo = new System.Windows.Forms.ToolStripButton();
            btnRedo = new System.Windows.Forms.ToolStripButton();
            btnFind = new System.Windows.Forms.ToolStripButton();
            btnSep2 = new System.Windows.Forms.ToolStripSeparator();
            btnComment = new System.Windows.Forms.ToolStripButton();
            btnUncomment = new System.Windows.Forms.ToolStripButton();
            btnSep3 = new System.Windows.Forms.ToolStripSeparator();
            btnCompile = new System.Windows.Forms.ToolStripButton();
            btnMsgClean = new System.Windows.Forms.ToolStripButton();
            rtfLogic = new FastColoredTextBoxNS.FastColoredTextBox();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rtfLogic).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuLResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(431, 19);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuLResource
            // 
            mnuLResource.DoubleClickEnabled = true;
            mnuLResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2 });
            mnuLResource.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            mnuLResource.MergeIndex = 1;
            mnuLResource.Name = "mnuLResource";
            mnuLResource.Size = new System.Drawing.Size(72, 17);
            mnuLResource.Text = "Resources";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
            toolStripMenuItem1.Text = "logic res1";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(124, 22);
            toolStripMenuItem2.Text = "logic res2";
            // 
            // mnuEdit
            // 
            mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuECut, mnuECopy, mnuEPaste });
            mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            mnuEdit.MergeIndex = 2;
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new System.Drawing.Size(48, 17);
            mnuEdit.Text = "Edit L";
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
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnCut, btnCopy, btnPaste, btnDelete, btnSep1, btnUndo, btnRedo, btnFind, btnSep2, btnComment, btnUncomment, btnSep3, btnCompile, btnMsgClean });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(548, 39);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnCut
            // 
            btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCut.Image = (System.Drawing.Image)resources.GetObject("btnCut.Image");
            btnCut.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnCut.Name = "btnCut";
            btnCut.Size = new System.Drawing.Size(36, 36);
            btnCut.Text = "Cut";
            // 
            // btnCopy
            // 
            btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCopy.Image = (System.Drawing.Image)resources.GetObject("btnCopy.Image");
            btnCopy.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new System.Drawing.Size(36, 36);
            btnCopy.Text = "Copy";
            // 
            // btnPaste
            // 
            btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnPaste.Image = (System.Drawing.Image)resources.GetObject("btnPaste.Image");
            btnPaste.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new System.Drawing.Size(36, 36);
            btnPaste.Text = "Paste";
            // 
            // btnDelete
            // 
            btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnDelete.Image = (System.Drawing.Image)resources.GetObject("btnDelete.Image");
            btnDelete.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(36, 36);
            btnDelete.Text = "Delete";
            // 
            // btnSep1
            // 
            btnSep1.Name = "btnSep1";
            btnSep1.Size = new System.Drawing.Size(6, 39);
            // 
            // btnUndo
            // 
            btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnUndo.Image = (System.Drawing.Image)resources.GetObject("btnUndo.Image");
            btnUndo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new System.Drawing.Size(36, 36);
            btnUndo.Text = "Undo";
            // 
            // btnRedo
            // 
            btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnRedo.Image = (System.Drawing.Image)resources.GetObject("btnRedo.Image");
            btnRedo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnRedo.Name = "btnRedo";
            btnRedo.Size = new System.Drawing.Size(36, 36);
            btnRedo.Text = "Redo";
            // 
            // btnFind
            // 
            btnFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnFind.Image = (System.Drawing.Image)resources.GetObject("btnFind.Image");
            btnFind.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnFind.Name = "btnFind";
            btnFind.Size = new System.Drawing.Size(36, 36);
            btnFind.Text = "Find";
            // 
            // btnSep2
            // 
            btnSep2.Name = "btnSep2";
            btnSep2.Size = new System.Drawing.Size(6, 39);
            // 
            // btnComment
            // 
            btnComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnComment.Image = (System.Drawing.Image)resources.GetObject("btnComment.Image");
            btnComment.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnComment.Name = "btnComment";
            btnComment.Size = new System.Drawing.Size(36, 36);
            btnComment.Text = "Comment";
            // 
            // btnUncomment
            // 
            btnUncomment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnUncomment.Image = (System.Drawing.Image)resources.GetObject("btnUncomment.Image");
            btnUncomment.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnUncomment.Name = "btnUncomment";
            btnUncomment.Size = new System.Drawing.Size(36, 36);
            btnUncomment.Text = "Uncomment";
            // 
            // btnSep3
            // 
            btnSep3.Name = "btnSep3";
            btnSep3.Size = new System.Drawing.Size(6, 39);
            // 
            // btnCompile
            // 
            btnCompile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCompile.Image = (System.Drawing.Image)resources.GetObject("btnCompile.Image");
            btnCompile.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnCompile.Name = "btnCompile";
            btnCompile.Size = new System.Drawing.Size(36, 36);
            btnCompile.Text = "Compile";
            // 
            // btnMsgClean
            // 
            btnMsgClean.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnMsgClean.Image = (System.Drawing.Image)resources.GetObject("btnMsgClean.Image");
            btnMsgClean.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnMsgClean.Name = "btnMsgClean";
            btnMsgClean.Size = new System.Drawing.Size(36, 36);
            btnMsgClean.Text = "Message Cleanup";
            // 
            // rtfLogic
            // 
            rtfLogic.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rtfLogic.AutoCompleteBracketsList = new char[]
    {
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
    };
            rtfLogic.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
            rtfLogic.AutoScrollMinSize = new System.Drawing.Size(99, 14);
            rtfLogic.BackBrush = null;
            rtfLogic.CharHeight = 14;
            rtfLogic.CharWidth = 8;
            rtfLogic.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            rtfLogic.Font = new System.Drawing.Font("Courier New", 9.75F);
            rtfLogic.Hotkeys = resources.GetString("rtfLogic.Hotkeys");
            rtfLogic.IsReplaceMode = false;
            rtfLogic.Location = new System.Drawing.Point(0, 42);
            rtfLogic.Name = "rtfLogic";
            rtfLogic.Paddings = new System.Windows.Forms.Padding(0);
            rtfLogic.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            rtfLogic.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("rtfLogic.ServiceColors");
            rtfLogic.Size = new System.Drawing.Size(548, 337);
            rtfLogic.TabIndex = 2;
            rtfLogic.Text = "return();";
            rtfLogic.Zoom = 100;
            // 
            // frmLogicEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(548, 380);
            Controls.Add(rtfLogic);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            Name = "frmLogicEdit";
            Text = "Form1";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)rtfLogic).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
        internal FastColoredTextBoxNS.FastColoredTextBox rtfLogic;
    }
}