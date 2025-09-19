
using System;

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWordsEdit));
            label1 = new System.Windows.Forms.Label();
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
            cmLists = new System.Windows.Forms.ContextMenuStrip(components);
            mnuEUndo = new System.Windows.Forms.ToolStripMenuItem();
            mnESep0 = new System.Windows.Forms.ToolStripSeparator();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuEDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuEClear = new System.Windows.Forms.ToolStripMenuItem();
            mnuEInsertGroup = new System.Windows.Forms.ToolStripMenuItem();
            mnuEInsertWord = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuEFind = new System.Windows.Forms.ToolStripMenuItem();
            mnuEFindAgain = new System.Windows.Forms.ToolStripMenuItem();
            mnuEReplace = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuEditItem = new System.Windows.Forms.ToolStripMenuItem();
            mnuEFindInLogic = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditMode = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tbbUndo = new System.Windows.Forms.ToolStripButton();
            tbbMode = new System.Windows.Forms.ToolStripButton();
            tbbCut = new System.Windows.Forms.ToolStripButton();
            tbbCopy = new System.Windows.Forms.ToolStripButton();
            tbbPaste = new System.Windows.Forms.ToolStripButton();
            tbbDelete = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tbbAddGroup = new System.Windows.Forms.ToolStripButton();
            tbbAddWord = new System.Windows.Forms.ToolStripButton();
            tbbRenumber = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tbbFind = new System.Windows.Forms.ToolStripButton();
            tbbFindLogic = new System.Windows.Forms.ToolStripButton();
            label2 = new System.Windows.Forms.Label();
            txtGroupEdit = new System.Windows.Forms.TextBox();
            cmGroupEdit = new System.Windows.Forms.ContextMenuStrip(components);
            cmgUndo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            cmgCut = new System.Windows.Forms.ToolStripMenuItem();
            cmgCopy = new System.Windows.Forms.ToolStripMenuItem();
            cmgPaste = new System.Windows.Forms.ToolStripMenuItem();
            cmgDelete = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            cmgSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            cmgCancel = new System.Windows.Forms.ToolStripMenuItem();
            txtWordEdit = new System.Windows.Forms.TextBox();
            cmWordEdit = new System.Windows.Forms.ContextMenuStrip(components);
            cmwUndo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            cmwCut = new System.Windows.Forms.ToolStripMenuItem();
            cmwCopy = new System.Windows.Forms.ToolStripMenuItem();
            cmwPaste = new System.Windows.Forms.ToolStripMenuItem();
            cmwDelete = new System.Windows.Forms.ToolStripMenuItem();
            cmwCharMap = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            cmwSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            cmwCancel = new System.Windows.Forms.ToolStripMenuItem();
            dgGroups = new System.Windows.Forms.DataGridView();
            groups = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dgWords = new System.Windows.Forms.DataGridView();
            words = new System.Windows.Forms.DataGridViewTextBoxColumn();
            menuStrip1.SuspendLayout();
            cmLists.SuspendLayout();
            toolStrip1.SuspendLayout();
            cmGroupEdit.SuspendLayout();
            cmWordEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgGroups).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgWords).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(96, 27);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(45, 15);
            label1.TabIndex = 0;
            label1.Text = "Groups";
            label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(684, 24);
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
            mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            mnuEdit.MergeIndex = 2;
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new System.Drawing.Size(39, 22);
            mnuEdit.Text = "&Edit";
            mnuEdit.DropDownClosed += mnuEdit_DropDownClosed;
            mnuEdit.DropDownOpening += mnuEdit_DropDownOpening;
            // 
            // cmLists
            // 
            cmLists.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuEUndo, mnESep0, mnuECut, mnuECopy, mnuEPaste, mnuEDelete, mnuEClear, mnuEInsertGroup, mnuEInsertWord, mnuESep1, mnuEFind, mnuEFindAgain, mnuEReplace, mnuESep2, mnuEditItem, mnuEFindInLogic, mnuEditMode });
            cmLists.Name = "cmWords";
            cmLists.Size = new System.Drawing.Size(235, 330);
            cmLists.Closed += cmWords_Closed;
            cmLists.Opening += cmWords_Opening;
            // 
            // mnuEUndo
            // 
            mnuEUndo.Name = "mnuEUndo";
            mnuEUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuEUndo.Size = new System.Drawing.Size(234, 22);
            mnuEUndo.Text = "Undo";
            mnuEUndo.Click += mnuEUndo_Click;
            // 
            // mnESep0
            // 
            mnESep0.Name = "mnESep0";
            mnESep0.Size = new System.Drawing.Size(231, 6);
            // 
            // mnuECut
            // 
            mnuECut.Name = "mnuECut";
            mnuECut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuECut.Size = new System.Drawing.Size(234, 22);
            mnuECut.Text = "Cut";
            mnuECut.Click += mnuECut_Click;
            // 
            // mnuECopy
            // 
            mnuECopy.Name = "mnuECopy";
            mnuECopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuECopy.Size = new System.Drawing.Size(234, 22);
            mnuECopy.Text = "Copy";
            mnuECopy.Click += mnuECopy_Click;
            // 
            // mnuEPaste
            // 
            mnuEPaste.Name = "mnuEPaste";
            mnuEPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuEPaste.Size = new System.Drawing.Size(234, 22);
            mnuEPaste.Text = "Paste";
            mnuEPaste.Click += mnuEPaste_Click;
            // 
            // mnuEDelete
            // 
            mnuEDelete.Name = "mnuEDelete";
            mnuEDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuEDelete.Size = new System.Drawing.Size(234, 22);
            mnuEDelete.Text = "Delete";
            mnuEDelete.Click += mnuEDelete_Click;
            // 
            // mnuEClear
            // 
            mnuEClear.Name = "mnuEClear";
            mnuEClear.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete;
            mnuEClear.Size = new System.Drawing.Size(234, 22);
            mnuEClear.Text = "Clear Word List";
            mnuEClear.Click += mnuEClear_Click;
            // 
            // mnuEInsertGroup
            // 
            mnuEInsertGroup.Name = "mnuEInsertGroup";
            mnuEInsertGroup.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            mnuEInsertGroup.Size = new System.Drawing.Size(234, 22);
            mnuEInsertGroup.Text = "Insert Group";
            mnuEInsertGroup.Click += mnuEAddGroup_Click;
            // 
            // mnuEInsertWord
            // 
            mnuEInsertWord.Name = "mnuEInsertWord";
            mnuEInsertWord.Size = new System.Drawing.Size(234, 22);
            mnuEInsertWord.Text = "Insert Word";
            mnuEInsertWord.Click += mnuEAddWord_Click;
            // 
            // mnuESep1
            // 
            mnuESep1.Name = "mnuESep1";
            mnuESep1.Size = new System.Drawing.Size(231, 6);
            // 
            // mnuEFind
            // 
            mnuEFind.Name = "mnuEFind";
            mnuEFind.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            mnuEFind.Size = new System.Drawing.Size(234, 22);
            mnuEFind.Text = "Find";
            mnuEFind.Click += mnuEFind_Click;
            // 
            // mnuEFindAgain
            // 
            mnuEFindAgain.Name = "mnuEFindAgain";
            mnuEFindAgain.ShortcutKeys = System.Windows.Forms.Keys.F3;
            mnuEFindAgain.Size = new System.Drawing.Size(234, 22);
            mnuEFindAgain.Text = "Find Again";
            mnuEFindAgain.Click += mnuEFindAgain_Click;
            // 
            // mnuEReplace
            // 
            mnuEReplace.Name = "mnuEReplace";
            mnuEReplace.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R;
            mnuEReplace.Size = new System.Drawing.Size(234, 22);
            mnuEReplace.Text = "Replace";
            mnuEReplace.Click += mnuEReplace_Click;
            // 
            // mnuESep2
            // 
            mnuESep2.Name = "mnuESep2";
            mnuESep2.Size = new System.Drawing.Size(231, 6);
            // 
            // mnuEditItem
            // 
            mnuEditItem.Name = "mnuEditItem";
            mnuEditItem.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Enter;
            mnuEditItem.Size = new System.Drawing.Size(234, 22);
            mnuEditItem.Text = "Edit Group Number";
            mnuEditItem.Click += mnuEditItem_Click;
            // 
            // mnuEFindInLogic
            // 
            mnuEFindInLogic.Name = "mnuEFindInLogic";
            mnuEFindInLogic.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F;
            mnuEFindInLogic.Size = new System.Drawing.Size(234, 22);
            mnuEFindInLogic.Text = "Find In Logics";
            mnuEFindInLogic.Click += mnuEFindLogic_Click;
            // 
            // mnuEditMode
            // 
            mnuEditMode.Name = "mnuEditMode";
            mnuEditMode.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.M;
            mnuEditMode.Size = new System.Drawing.Size(234, 22);
            mnuEditMode.Text = "Display by Words";
            mnuEditMode.Click += mnuEMode_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tbbUndo, tbbMode, tbbCut, tbbCopy, tbbPaste, tbbDelete, toolStripSeparator1, tbbAddGroup, tbbAddWord, tbbRenumber, toolStripSeparator2, tbbFind, tbbFindLogic });
            toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(684, 25);
            toolStrip1.Stretch = true;
            toolStrip1.TabIndex = 13;
            toolStrip1.Text = "toolStrip1";
            // 
            // tbbUndo
            // 
            tbbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbUndo.Enabled = false;
            tbbUndo.Image = (System.Drawing.Image)resources.GetObject("tbbUndo.Image");
            tbbUndo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            tbbUndo.Name = "tbbUndo";
            tbbUndo.Size = new System.Drawing.Size(23, 22);
            tbbUndo.ToolTipText = "Undo";
            tbbUndo.Click += mnuEUndo_Click;
            // 
            // tbbMode
            // 
            tbbMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbMode.Image = (System.Drawing.Image)resources.GetObject("tbbMode.Image");
            tbbMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbMode.Name = "tbbMode";
            tbbMode.Size = new System.Drawing.Size(23, 22);
            tbbMode.ToolTipText = "Display Mode";
            tbbMode.Click += mnuEMode_Click;
            // 
            // tbbCut
            // 
            tbbCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbCut.Image = (System.Drawing.Image)resources.GetObject("tbbCut.Image");
            tbbCut.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            tbbCut.Name = "tbbCut";
            tbbCut.Size = new System.Drawing.Size(23, 22);
            tbbCut.ToolTipText = "Cut";
            tbbCut.Click += mnuECut_Click;
            // 
            // tbbCopy
            // 
            tbbCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbCopy.Image = (System.Drawing.Image)resources.GetObject("tbbCopy.Image");
            tbbCopy.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            tbbCopy.Name = "tbbCopy";
            tbbCopy.Size = new System.Drawing.Size(23, 22);
            tbbCopy.ToolTipText = "Copy";
            tbbCopy.Click += mnuECopy_Click;
            // 
            // tbbPaste
            // 
            tbbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbPaste.Image = (System.Drawing.Image)resources.GetObject("tbbPaste.Image");
            tbbPaste.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            tbbPaste.Name = "tbbPaste";
            tbbPaste.Size = new System.Drawing.Size(23, 22);
            tbbPaste.ToolTipText = "Paste";
            tbbPaste.Click += mnuEPaste_Click;
            // 
            // tbbDelete
            // 
            tbbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbDelete.Image = (System.Drawing.Image)resources.GetObject("tbbDelete.Image");
            tbbDelete.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            tbbDelete.Name = "tbbDelete";
            tbbDelete.Size = new System.Drawing.Size(23, 22);
            tbbDelete.ToolTipText = "Delete";
            tbbDelete.Click += mnuEDelete_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tbbAddGroup
            // 
            tbbAddGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbAddGroup.Image = (System.Drawing.Image)resources.GetObject("tbbAddGroup.Image");
            tbbAddGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbAddGroup.Name = "tbbAddGroup";
            tbbAddGroup.Size = new System.Drawing.Size(23, 22);
            tbbAddGroup.ToolTipText = "Add Group";
            tbbAddGroup.Click += mnuEAddGroup_Click;
            // 
            // tbbAddWord
            // 
            tbbAddWord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbAddWord.Image = (System.Drawing.Image)resources.GetObject("tbbAddWord.Image");
            tbbAddWord.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbAddWord.Name = "tbbAddWord";
            tbbAddWord.Size = new System.Drawing.Size(23, 22);
            tbbAddWord.ToolTipText = "Add Word";
            tbbAddWord.Click += mnuEAddWord_Click;
            // 
            // tbbRenumber
            // 
            tbbRenumber.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbRenumber.Image = (System.Drawing.Image)resources.GetObject("tbbRenumber.Image");
            tbbRenumber.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbRenumber.Name = "tbbRenumber";
            tbbRenumber.Size = new System.Drawing.Size(23, 22);
            tbbRenumber.ToolTipText = "Renumber Group";
            tbbRenumber.Click += mnuEditItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tbbFind
            // 
            tbbFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbFind.Image = (System.Drawing.Image)resources.GetObject("tbbFind.Image");
            tbbFind.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbFind.Name = "tbbFind";
            tbbFind.Size = new System.Drawing.Size(23, 22);
            tbbFind.ToolTipText = "Find";
            tbbFind.Click += mnuEFind_Click;
            // 
            // tbbFindLogic
            // 
            tbbFindLogic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbFindLogic.Image = (System.Drawing.Image)resources.GetObject("tbbFindLogic.Image");
            tbbFindLogic.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbFindLogic.Name = "tbbFindLogic";
            tbbFindLogic.Size = new System.Drawing.Size(23, 22);
            tbbFindLogic.ToolTipText = "Find in Logic";
            tbbFindLogic.Click += mnuEFindLogic_Click;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(509, 27);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(41, 15);
            label2.TabIndex = 15;
            label2.Text = "Words";
            label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtGroupEdit
            // 
            txtGroupEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtGroupEdit.ContextMenuStrip = cmGroupEdit;
            txtGroupEdit.Location = new System.Drawing.Point(5, 45);
            txtGroupEdit.Name = "txtGroupEdit";
            txtGroupEdit.Size = new System.Drawing.Size(186, 16);
            txtGroupEdit.TabIndex = 16;
            txtGroupEdit.Visible = false;
            txtGroupEdit.TextChanged += txtGroupEdit_TextChanged;
            txtGroupEdit.KeyDown += txtGroupEdit_KeyDown;
            txtGroupEdit.Validating += txtGroupEdit_Validating;
            // 
            // cmGroupEdit
            // 
            cmGroupEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cmgUndo, toolStripSeparator6, cmgCut, cmgCopy, cmgPaste, cmgDelete, toolStripSeparator7, cmgSelectAll, toolStripSeparator8, cmgCancel });
            cmGroupEdit.Name = "cmGroupEdit";
            cmGroupEdit.Size = new System.Drawing.Size(165, 176);
            // 
            // cmgUndo
            // 
            cmgUndo.Name = "cmgUndo";
            cmgUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            cmgUndo.Size = new System.Drawing.Size(164, 22);
            cmgUndo.Text = "Undo";
            cmgUndo.Click += cmUndo_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(161, 6);
            // 
            // cmgCut
            // 
            cmgCut.Name = "cmgCut";
            cmgCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            cmgCut.Size = new System.Drawing.Size(164, 22);
            cmgCut.Text = "Cut";
            cmgCut.Click += cmCut_Click;
            // 
            // cmgCopy
            // 
            cmgCopy.Name = "cmgCopy";
            cmgCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            cmgCopy.Size = new System.Drawing.Size(164, 22);
            cmgCopy.Text = "Copy";
            cmgCopy.Click += cmCopy_Click;
            // 
            // cmgPaste
            // 
            cmgPaste.Name = "cmgPaste";
            cmgPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            cmgPaste.Size = new System.Drawing.Size(164, 22);
            cmgPaste.Text = "Paste";
            cmgPaste.Click += cmPaste_Click;
            // 
            // cmgDelete
            // 
            cmgDelete.Name = "cmgDelete";
            cmgDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            cmgDelete.Size = new System.Drawing.Size(164, 22);
            cmgDelete.Text = "Delete";
            cmgDelete.Click += cmDelete_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(161, 6);
            // 
            // cmgSelectAll
            // 
            cmgSelectAll.Name = "cmgSelectAll";
            cmgSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            cmgSelectAll.Size = new System.Drawing.Size(164, 22);
            cmgSelectAll.Text = "Select All";
            cmgSelectAll.Click += cmSelectAll_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(161, 6);
            // 
            // cmgCancel
            // 
            cmgCancel.Name = "cmgCancel";
            cmgCancel.Size = new System.Drawing.Size(164, 22);
            cmgCancel.Text = "Cancel";
            cmgCancel.Click += cmCancel_Click;
            // 
            // txtWordEdit
            // 
            txtWordEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtWordEdit.ContextMenuStrip = cmWordEdit;
            txtWordEdit.Location = new System.Drawing.Point(342, 45);
            txtWordEdit.Name = "txtWordEdit";
            txtWordEdit.Size = new System.Drawing.Size(221, 16);
            txtWordEdit.TabIndex = 17;
            txtWordEdit.Visible = false;
            txtWordEdit.KeyDown += txtWordEdit_KeyDown;
            txtWordEdit.KeyPress += txtWordEdit_KeyPress;
            txtWordEdit.Validating += txtWordEdit_Validating;
            // 
            // cmWordEdit
            // 
            cmWordEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cmwUndo, toolStripSeparator3, cmwCut, cmwCopy, cmwPaste, cmwDelete, cmwCharMap, toolStripSeparator4, cmwSelectAll, toolStripSeparator5, cmwCancel });
            cmWordEdit.Name = "cmWordEdit";
            cmWordEdit.Size = new System.Drawing.Size(216, 198);
            // 
            // cmwUndo
            // 
            cmwUndo.Name = "cmwUndo";
            cmwUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            cmwUndo.Size = new System.Drawing.Size(215, 22);
            cmwUndo.Text = "Undo";
            cmwUndo.Click += cmUndo_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(212, 6);
            // 
            // cmwCut
            // 
            cmwCut.Name = "cmwCut";
            cmwCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            cmwCut.Size = new System.Drawing.Size(215, 22);
            cmwCut.Text = "Cut";
            cmwCut.Click += cmCut_Click;
            // 
            // cmwCopy
            // 
            cmwCopy.Name = "cmwCopy";
            cmwCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            cmwCopy.Size = new System.Drawing.Size(215, 22);
            cmwCopy.Text = "Copy";
            cmwCopy.Click += cmCopy_Click;
            // 
            // cmwPaste
            // 
            cmwPaste.Name = "cmwPaste";
            cmwPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            cmwPaste.Size = new System.Drawing.Size(215, 22);
            cmwPaste.Text = "Paste";
            cmwPaste.Click += cmPaste_Click;
            // 
            // cmwDelete
            // 
            cmwDelete.Name = "cmwDelete";
            cmwDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            cmwDelete.Size = new System.Drawing.Size(215, 22);
            cmwDelete.Text = "Delete";
            cmwDelete.Click += cmDelete_Click;
            // 
            // cmwCharMap
            // 
            cmwCharMap.Name = "cmwCharMap";
            cmwCharMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert;
            cmwCharMap.Size = new System.Drawing.Size(215, 22);
            cmwCharMap.Text = "Character Map";
            cmwCharMap.Click += cmCharMap_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(212, 6);
            // 
            // cmwSelectAll
            // 
            cmwSelectAll.Name = "cmwSelectAll";
            cmwSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            cmwSelectAll.Size = new System.Drawing.Size(215, 22);
            cmwSelectAll.Text = "Select All";
            cmwSelectAll.Click += cmSelectAll_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(212, 6);
            // 
            // cmwCancel
            // 
            cmwCancel.Name = "cmwCancel";
            cmwCancel.Size = new System.Drawing.Size(215, 22);
            cmwCancel.Text = "Cancel";
            cmwCancel.Click += cmCancel_Click;
            // 
            // dgGroups
            // 
            dgGroups.AllowDrop = true;
            dgGroups.AllowUserToAddRows = false;
            dgGroups.AllowUserToDeleteRows = false;
            dgGroups.AllowUserToResizeColumns = false;
            dgGroups.AllowUserToResizeRows = false;
            dgGroups.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dgGroups.BackgroundColor = System.Drawing.SystemColors.Window;
            dgGroups.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dgGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgGroups.ColumnHeadersVisible = false;
            dgGroups.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { groups });
            dgGroups.ContextMenuStrip = cmLists;
            dgGroups.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dgGroups.Location = new System.Drawing.Point(5, 45);
            dgGroups.MultiSelect = false;
            dgGroups.Name = "dgGroups";
            dgGroups.RowHeadersVisible = false;
            dgGroups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgGroups.ShowEditingIcon = false;
            dgGroups.Size = new System.Drawing.Size(331, 266);
            dgGroups.TabIndex = 7;
            dgGroups.DragDrop += dgGroups_DragDrop;
            dgGroups.DragEnter += dgGroups_DragEnter;
            dgGroups.DragOver += dgGroups_DragOver;
            dgGroups.DragLeave += dgGroups_DragLeave;
            dgGroups.DoubleClick += dgGroups_DoubleClick;
            dgGroups.Enter += dgGroups_Enter;
            dgGroups.Leave += dgGroups_Leave;
            dgGroups.MouseDown += dgGroups_MouseDown;
            dgGroups.MouseUp += dgGroups_MouseUp;
            // 
            // groups
            // 
            groups.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            groups.HeaderText = "Column1";
            groups.MinimumWidth = 50;
            groups.Name = "groups";
            groups.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            groups.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgWords
            // 
            dgWords.AllowUserToAddRows = false;
            dgWords.AllowUserToDeleteRows = false;
            dgWords.AllowUserToResizeColumns = false;
            dgWords.AllowUserToResizeRows = false;
            dgWords.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            dgWords.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dgWords.BackgroundColor = System.Drawing.SystemColors.Window;
            dgWords.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dgWords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgWords.ColumnHeadersVisible = false;
            dgWords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { words });
            dgWords.ContextMenuStrip = cmLists;
            dgWords.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            dgWords.Location = new System.Drawing.Point(342, 45);
            dgWords.MultiSelect = false;
            dgWords.Name = "dgWords";
            dgWords.RowHeadersVisible = false;
            dgWords.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgWords.Size = new System.Drawing.Size(337, 266);
            dgWords.TabIndex = 8;
            dgWords.QueryContinueDrag += dgWords_QueryContinueDrag;
            dgWords.DoubleClick += dgWords_DoubleClick;
            dgWords.Enter += dgWords_Enter;
            dgWords.Leave += dgWords_Leave;
            dgWords.MouseDown += dgWords_MouseDown;
            dgWords.MouseMove += dgWords_MouseMove;
            dgWords.MouseUp += dgWords_MouseUp;
            // 
            // words
            // 
            words.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            words.HeaderText = "Column1";
            words.Name = "words";
            words.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // frmWordsEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(684, 311);
            Controls.Add(txtWordEdit);
            Controls.Add(dgWords);
            Controls.Add(txtGroupEdit);
            Controls.Add(dgGroups);
            Controls.Add(label2);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            Controls.Add(label1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmWordsEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmWordsEdit";
            Activated += frmWordsEdit_Activated;
            FormClosing += frmWordsEdit_FormClosing;
            FormClosed += frmWordsEdit_FormClosed;
            Resize += frmWordsEdit_Resize;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            cmLists.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            cmGroupEdit.ResumeLayout(false);
            cmWordEdit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgGroups).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgWords).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.ToolStripMenuItem mnuRGroupCheck;
        private System.Windows.Forms.ContextMenuStrip cmLists;
        private System.Windows.Forms.ToolStripMenuItem mnuEUndo;
        private System.Windows.Forms.ToolStripSeparator mnESep0;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuEDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuEClear;
        private System.Windows.Forms.ToolStripMenuItem mnuEInsertGroup;
        private System.Windows.Forms.ToolStripMenuItem mnuEInsertWord;
        private System.Windows.Forms.ToolStripSeparator mnuESep1;
        private System.Windows.Forms.ToolStripMenuItem mnuEFind;
        private System.Windows.Forms.ToolStripMenuItem mnuEFindAgain;
        private System.Windows.Forms.ToolStripMenuItem mnuEReplace;
        private System.Windows.Forms.ToolStripSeparator mnuESep2;
        private System.Windows.Forms.ToolStripMenuItem mnuEditItem;
        private System.Windows.Forms.ToolStripMenuItem mnuEFindInLogic;
        private System.Windows.Forms.ToolStripMenuItem mnuEditMode;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripButton tbbUndo;
        private System.Windows.Forms.ToolStripButton tbbMode;
        private System.Windows.Forms.ToolStripButton tbbCut;
        private System.Windows.Forms.ToolStripButton tbbCopy;
        private System.Windows.Forms.ToolStripButton tbbPaste;
        private System.Windows.Forms.ToolStripButton tbbDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tbbAddGroup;
        private System.Windows.Forms.ToolStripButton tbbAddWord;
        private System.Windows.Forms.ToolStripButton tbbRenumber;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tbbFind;
        private System.Windows.Forms.ToolStripButton tbbFindLogic;
        private System.Windows.Forms.TextBox txtGroupEdit;
        private System.Windows.Forms.TextBox txtWordEdit;
        private System.Windows.Forms.ContextMenuStrip cmGroupEdit;
        private System.Windows.Forms.ContextMenuStrip cmWordEdit;
        private System.Windows.Forms.ToolStripMenuItem cmwUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem cmwCut;
        private System.Windows.Forms.ToolStripMenuItem cmwCopy;
        private System.Windows.Forms.ToolStripMenuItem cmwPaste;
        private System.Windows.Forms.ToolStripMenuItem cmwDelete;
        private System.Windows.Forms.ToolStripMenuItem cmwCharMap;
        private System.Windows.Forms.ToolStripMenuItem cmgUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem cmgCut;
        private System.Windows.Forms.ToolStripMenuItem cmgCopy;
        private System.Windows.Forms.ToolStripMenuItem cmgPaste;
        private System.Windows.Forms.ToolStripMenuItem cmgDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem cmgSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem cmwSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem cmwCancel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem cmgCancel;
        private System.Windows.Forms.DataGridView dgGroups;
        private System.Windows.Forms.DataGridViewTextBoxColumn groups;
        private System.Windows.Forms.DataGridView dgWords;
        private System.Windows.Forms.DataGridViewTextBoxColumn words;
    }
}