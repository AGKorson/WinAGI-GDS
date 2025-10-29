namespace WinAGI.Editor {
    partial class frmLogicEdit {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogicEdit));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("");
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
            mnuRMsgCleanup = new System.Windows.Forms.ToolStripMenuItem();
            mnuRIsRoom = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            mnuEUndo = new System.Windows.Forms.ToolStripMenuItem();
            mnuERedo = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep0 = new System.Windows.Forms.ToolStripSeparator();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuEDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuESelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuEFind = new System.Windows.Forms.ToolStripMenuItem();
            mnuEFindAgain = new System.Windows.Forms.ToolStripMenuItem();
            mnuEReplace = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuESnippet = new System.Windows.Forms.ToolStripMenuItem();
            mnuEListDefines = new System.Windows.Forms.ToolStripMenuItem();
            mnuEViewSynonym = new System.Windows.Forms.ToolStripMenuItem();
            mnuEBlockCmt = new System.Windows.Forms.ToolStripMenuItem();
            mnuEUnblockCmt = new System.Windows.Forms.ToolStripMenuItem();
            mnuEOpenRes = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep3 = new System.Windows.Forms.ToolStripSeparator();
            mnuEDocumentMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuELineNumbers = new System.Windows.Forms.ToolStripMenuItem();
            mnuECharMap = new System.Windows.Forms.ToolStripMenuItem();
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
            btnCharMap = new System.Windows.Forms.ToolStripButton();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitLogic = new System.Windows.Forms.SplitContainer();
            rtfLogic2 = new WinAGIFCTB();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuEListCommands = new System.Windows.Forms.ToolStripMenuItem();
            rtfLogic1 = new WinAGIFCTB();
            documentMap1 = new FastColoredTextBoxNS.DocumentMap();
            picTip = new System.Windows.Forms.PictureBox();
            lstDefines = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            imageList1 = new System.Windows.Forms.ImageList(components);
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitLogic).BeginInit();
            splitLogic.Panel1.SuspendLayout();
            splitLogic.Panel2.SuspendLayout();
            splitLogic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rtfLogic2).BeginInit();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rtfLogic1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picTip).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 33);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(719, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRExport, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportLoopGIF, mnuRMsgCleanup, mnuRIsRoom });
            mnuResource.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            mnuResource.MergeIndex = 1;
            mnuResource.Name = "mnuResource";
            mnuResource.Size = new System.Drawing.Size(67, 20);
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
            mnuRSave.Text = "save res";
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
            mnuRRenumber.Text = "Renumber Logic";
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
            mnuRCompile.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRCompile.MergeIndex = 11;
            mnuRCompile.Name = "mnuRCompile";
            mnuRCompile.ShortcutKeys = System.Windows.Forms.Keys.F8;
            mnuRCompile.Size = new System.Drawing.Size(225, 22);
            mnuRCompile.Text = "Compile ...";
            mnuRCompile.Click += mnuRCompile_Click;
            // 
            // mnuRSavePicImage
            // 
            mnuRSavePicImage.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRSavePicImage.MergeIndex = 12;
            mnuRSavePicImage.Name = "mnuRSavePicImage";
            mnuRSavePicImage.Size = new System.Drawing.Size(225, 22);
            mnuRSavePicImage.Text = "save pic image";
            // 
            // mnuRExportLoopGIF
            // 
            mnuRExportLoopGIF.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRExportLoopGIF.MergeIndex = 12;
            mnuRExportLoopGIF.Name = "mnuRExportLoopGIF";
            mnuRExportLoopGIF.Size = new System.Drawing.Size(225, 22);
            mnuRExportLoopGIF.Text = "export loop gif";
            // 
            // mnuRMsgCleanup
            // 
            mnuRMsgCleanup.Name = "mnuRMsgCleanup";
            mnuRMsgCleanup.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.M;
            mnuRMsgCleanup.Size = new System.Drawing.Size(225, 22);
            mnuRMsgCleanup.Text = "Message Cleanup";
            mnuRMsgCleanup.Click += mnuRMsgCleanup_Click;
            // 
            // mnuRIsRoom
            // 
            mnuRIsRoom.Checked = true;
            mnuRIsRoom.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuRIsRoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuRIsRoom.Name = "mnuRIsRoom";
            mnuRIsRoom.Size = new System.Drawing.Size(225, 22);
            mnuRIsRoom.Text = "Is Room";
            mnuRIsRoom.Click += mnuRIsRoom_Click;
            // 
            // mnuEdit
            // 
            mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            mnuEdit.MergeIndex = 2;
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new System.Drawing.Size(39, 20);
            mnuEdit.Text = "&Edit";
            mnuEdit.DropDownClosed += mnuEdit_DropDownClosed;
            mnuEdit.DropDownOpening += mnuEdit_DropDownOpening;
            // 
            // mnuEUndo
            // 
            mnuEUndo.Name = "mnuEUndo";
            mnuEUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuEUndo.Size = new System.Drawing.Size(262, 22);
            mnuEUndo.Text = "Undo";
            mnuEUndo.Click += mnuEUndo_Click;
            // 
            // mnuERedo
            // 
            mnuERedo.Name = "mnuERedo";
            mnuERedo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y;
            mnuERedo.Size = new System.Drawing.Size(262, 22);
            mnuERedo.Text = "Redo";
            mnuERedo.Click += mnuERedo_Click;
            // 
            // mnuESep0
            // 
            mnuESep0.Name = "mnuESep0";
            mnuESep0.Size = new System.Drawing.Size(259, 6);
            // 
            // mnuECut
            // 
            mnuECut.Name = "mnuECut";
            mnuECut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuECut.Size = new System.Drawing.Size(262, 22);
            mnuECut.Text = "Cut";
            mnuECut.Click += mnuECut_Click;
            // 
            // mnuEDelete
            // 
            mnuEDelete.Name = "mnuEDelete";
            mnuEDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuEDelete.Size = new System.Drawing.Size(262, 22);
            mnuEDelete.Text = "Delete";
            mnuEDelete.Click += mnuEDelete_Click;
            // 
            // mnuECopy
            // 
            mnuECopy.Name = "mnuECopy";
            mnuECopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuECopy.Size = new System.Drawing.Size(262, 22);
            mnuECopy.Text = "Copy";
            mnuECopy.Click += mnuECopy_Click;
            // 
            // mnuEPaste
            // 
            mnuEPaste.Name = "mnuEPaste";
            mnuEPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuEPaste.Size = new System.Drawing.Size(262, 22);
            mnuEPaste.Text = "Paste";
            mnuEPaste.Click += mnuEPaste_Click;
            // 
            // mnuESelectAll
            // 
            mnuESelectAll.Name = "mnuESelectAll";
            mnuESelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuESelectAll.Size = new System.Drawing.Size(262, 22);
            mnuESelectAll.Text = "Select All";
            mnuESelectAll.Click += mnuESelectAll_Click;
            // 
            // mnuESep1
            // 
            mnuESep1.Name = "mnuESep1";
            mnuESep1.Size = new System.Drawing.Size(259, 6);
            // 
            // mnuEFind
            // 
            mnuEFind.Name = "mnuEFind";
            mnuEFind.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            mnuEFind.Size = new System.Drawing.Size(262, 22);
            mnuEFind.Text = "Find";
            mnuEFind.Click += mnuEFind_Click;
            // 
            // mnuEFindAgain
            // 
            mnuEFindAgain.Name = "mnuEFindAgain";
            mnuEFindAgain.ShortcutKeys = System.Windows.Forms.Keys.F3;
            mnuEFindAgain.Size = new System.Drawing.Size(262, 22);
            mnuEFindAgain.Text = "Find Next";
            mnuEFindAgain.Click += mnuEFindAgain_Click;
            // 
            // mnuEReplace
            // 
            mnuEReplace.Name = "mnuEReplace";
            mnuEReplace.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H;
            mnuEReplace.Size = new System.Drawing.Size(262, 22);
            mnuEReplace.Text = "Replace";
            mnuEReplace.Click += mnuEReplace_Click;
            // 
            // mnuESep2
            // 
            mnuESep2.Name = "mnuESep2";
            mnuESep2.Size = new System.Drawing.Size(259, 6);
            // 
            // mnuESnippet
            // 
            mnuESnippet.Name = "mnuESnippet";
            mnuESnippet.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.T;
            mnuESnippet.Size = new System.Drawing.Size(262, 22);
            mnuESnippet.Text = "Insert Snippet";
            mnuESnippet.Click += mnuESnippet_Click;
            // 
            // mnuEListDefines
            // 
            mnuEListDefines.Name = "mnuEListDefines";
            mnuEListDefines.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.J;
            mnuEListDefines.Size = new System.Drawing.Size(262, 22);
            mnuEListDefines.Text = "List Defines";
            mnuEListDefines.Click += mnuEListDefines_Click;
            // 
            // mnuEViewSynonym
            // 
            mnuEViewSynonym.Name = "mnuEViewSynonym";
            mnuEViewSynonym.Size = new System.Drawing.Size(262, 22);
            mnuEViewSynonym.Text = "View Synonyms for word";
            mnuEViewSynonym.Click += mnuEViewSynonym_Click;
            // 
            // mnuEBlockCmt
            // 
            mnuEBlockCmt.Name = "mnuEBlockCmt";
            mnuEBlockCmt.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.B;
            mnuEBlockCmt.Size = new System.Drawing.Size(262, 22);
            mnuEBlockCmt.Text = "Block Comment";
            mnuEBlockCmt.Click += mnuEBlockCmt_Click;
            // 
            // mnuEUnblockCmt
            // 
            mnuEUnblockCmt.Name = "mnuEUnblockCmt";
            mnuEUnblockCmt.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U;
            mnuEUnblockCmt.Size = new System.Drawing.Size(262, 22);
            mnuEUnblockCmt.Text = "Unblock Comment";
            mnuEUnblockCmt.Click += mnuEUnblockCmt_Click;
            // 
            // mnuEOpenRes
            // 
            mnuEOpenRes.Name = "mnuEOpenRes";
            mnuEOpenRes.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.E;
            mnuEOpenRes.Size = new System.Drawing.Size(262, 22);
            mnuEOpenRes.Text = "Open res for Editing";
            mnuEOpenRes.Click += mnuEOpenRes_Click;
            // 
            // mnuESep3
            // 
            mnuESep3.Name = "mnuESep3";
            mnuESep3.Size = new System.Drawing.Size(259, 6);
            // 
            // mnuEDocumentMap
            // 
            mnuEDocumentMap.Name = "mnuEDocumentMap";
            mnuEDocumentMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.M;
            mnuEDocumentMap.Size = new System.Drawing.Size(262, 22);
            mnuEDocumentMap.Text = "Hide Document Map";
            mnuEDocumentMap.Click += mnuEDocumentMap_Click;
            // 
            // mnuELineNumbers
            // 
            mnuELineNumbers.Name = "mnuELineNumbers";
            mnuELineNumbers.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.N;
            mnuELineNumbers.Size = new System.Drawing.Size(262, 22);
            mnuELineNumbers.Text = "toolStripMenuItem1";
            mnuELineNumbers.Click += mnuELineNumbers_Click;
            // 
            // mnuECharMap
            // 
            mnuECharMap.Name = "mnuECharMap";
            mnuECharMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert;
            mnuECharMap.Size = new System.Drawing.Size(262, 22);
            mnuECharMap.Text = "Character Map...";
            mnuECharMap.Click += mnuECharMap_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnCut, btnCopy, btnPaste, btnDelete, btnSep1, btnUndo, btnRedo, btnFind, btnSep2, btnComment, btnUncomment, btnCharMap, btnSep3, btnMsgClean, btnCompile });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(0, 1, 2, 1);
            toolStrip1.Size = new System.Drawing.Size(719, 33);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnCut
            // 
            btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCut.Image = (System.Drawing.Image)resources.GetObject("btnCut.Image");
            btnCut.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnCut.Name = "btnCut";
            btnCut.Size = new System.Drawing.Size(28, 28);
            btnCut.Text = "Cut";
            btnCut.Click += mnuECut_Click;
            // 
            // btnCopy
            // 
            btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCopy.Image = (System.Drawing.Image)resources.GetObject("btnCopy.Image");
            btnCopy.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new System.Drawing.Size(28, 28);
            btnCopy.Text = "Copy";
            btnCopy.Click += mnuECopy_Click;
            // 
            // btnPaste
            // 
            btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnPaste.Image = (System.Drawing.Image)resources.GetObject("btnPaste.Image");
            btnPaste.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new System.Drawing.Size(28, 28);
            btnPaste.Text = "Paste";
            btnPaste.Click += mnuEPaste_Click;
            // 
            // btnDelete
            // 
            btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnDelete.Image = (System.Drawing.Image)resources.GetObject("btnDelete.Image");
            btnDelete.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(28, 28);
            btnDelete.Text = "Delete";
            btnDelete.Click += mnuEDelete_Click;
            // 
            // btnSep1
            // 
            btnSep1.Name = "btnSep1";
            btnSep1.Size = new System.Drawing.Size(6, 31);
            // 
            // btnUndo
            // 
            btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnUndo.Image = (System.Drawing.Image)resources.GetObject("btnUndo.Image");
            btnUndo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new System.Drawing.Size(28, 28);
            btnUndo.Text = "Undo";
            btnUndo.Click += mnuEUndo_Click;
            // 
            // btnRedo
            // 
            btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnRedo.Image = (System.Drawing.Image)resources.GetObject("btnRedo.Image");
            btnRedo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnRedo.Name = "btnRedo";
            btnRedo.Size = new System.Drawing.Size(28, 28);
            btnRedo.Text = "Redo";
            btnRedo.Click += mnuERedo_Click;
            // 
            // btnFind
            // 
            btnFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnFind.Image = (System.Drawing.Image)resources.GetObject("btnFind.Image");
            btnFind.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnFind.Name = "btnFind";
            btnFind.Size = new System.Drawing.Size(28, 28);
            btnFind.Text = "Find";
            btnFind.Click += mnuEFind_Click;
            // 
            // btnSep2
            // 
            btnSep2.Name = "btnSep2";
            btnSep2.Size = new System.Drawing.Size(6, 31);
            // 
            // btnComment
            // 
            btnComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnComment.Image = (System.Drawing.Image)resources.GetObject("btnComment.Image");
            btnComment.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnComment.Name = "btnComment";
            btnComment.Size = new System.Drawing.Size(28, 28);
            btnComment.Text = "Comment";
            btnComment.Click += mnuEBlockCmt_Click;
            // 
            // btnUncomment
            // 
            btnUncomment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnUncomment.Image = (System.Drawing.Image)resources.GetObject("btnUncomment.Image");
            btnUncomment.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnUncomment.Name = "btnUncomment";
            btnUncomment.Size = new System.Drawing.Size(28, 28);
            btnUncomment.Text = "Uncomment";
            btnUncomment.Click += mnuEUnblockCmt_Click;
            // 
            // btnSep3
            // 
            btnSep3.Name = "btnSep3";
            btnSep3.Size = new System.Drawing.Size(6, 31);
            // 
            // btnCompile
            // 
            btnCompile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCompile.Image = (System.Drawing.Image)resources.GetObject("btnCompile.Image");
            btnCompile.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnCompile.Name = "btnCompile";
            btnCompile.Size = new System.Drawing.Size(28, 28);
            btnCompile.Text = "Compile";
            btnCompile.Click += mnuRCompile_Click;
            // 
            // btnMsgClean
            // 
            btnMsgClean.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnMsgClean.Image = (System.Drawing.Image)resources.GetObject("btnMsgClean.Image");
            btnMsgClean.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnMsgClean.Name = "btnMsgClean";
            btnMsgClean.Size = new System.Drawing.Size(28, 28);
            btnMsgClean.Text = "Message Cleanup";
            btnMsgClean.Click += mnuRMsgCleanup_Click;
            // 
            // btnCharMap
            // 
            btnCharMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCharMap.Image = (System.Drawing.Image)resources.GetObject("btnCharMap.Image");
            btnCharMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCharMap.Name = "btnCharMap";
            btnCharMap.Size = new System.Drawing.Size(28, 28);
            btnCharMap.Text = "toolStripButton1";
            btnCharMap.Click += mnuECharMap_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 33);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitLogic);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(documentMap1);
            splitContainer1.Panel2MinSize = 75;
            splitContainer1.Size = new System.Drawing.Size(719, 309);
            splitContainer1.SplitterDistance = 600;
            splitContainer1.TabIndex = 4;
            splitContainer1.TabStop = false;
            // 
            // splitLogic
            // 
            splitLogic.Dock = System.Windows.Forms.DockStyle.Fill;
            splitLogic.Location = new System.Drawing.Point(0, 0);
            splitLogic.Name = "splitLogic";
            splitLogic.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitLogic.Panel1
            // 
            splitLogic.Panel1.Controls.Add(rtfLogic2);
            splitLogic.Panel1MinSize = 0;
            // 
            // splitLogic.Panel2
            // 
            splitLogic.Panel2.Controls.Add(rtfLogic1);
            splitLogic.Panel2MinSize = 0;
            splitLogic.Size = new System.Drawing.Size(600, 309);
            splitLogic.SplitterDistance = 147;
            splitLogic.TabIndex = 4;
            splitLogic.TabStop = false;
            // 
            // rtfLogic2
            // 
            rtfLogic2.AutoCompleteBracketsList = new char[]
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
            rtfLogic2.AutoIndentCharsPatterns = "";
            rtfLogic2.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            rtfLogic2.BackBrush = null;
            rtfLogic2.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            rtfLogic2.CharHeight = 14;
            rtfLogic2.CharWidth = 8;
            rtfLogic2.CommentPrefix = "[";
            rtfLogic2.ContextMenuStrip = contextMenuStrip1;
            rtfLogic2.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            rtfLogic2.Dock = System.Windows.Forms.DockStyle.Fill;
            rtfLogic2.FindEndOfFoldingBlockStrategy = FastColoredTextBoxNS.FindEndOfFoldingBlockStrategy.Strategy2;
            rtfLogic2.Hotkeys = resources.GetString("rtfLogic2.Hotkeys");
            rtfLogic2.IsReplaceMode = false;
            rtfLogic2.LineNumberStartValue = 0U;
            rtfLogic2.Location = new System.Drawing.Point(0, 0);
            rtfLogic2.Name = "rtfLogic2";
            rtfLogic2.NoMouse = false;
            rtfLogic2.Paddings = new System.Windows.Forms.Padding(0);
            rtfLogic2.SelectionColor = System.Drawing.Color.FromArgb(60, 255, 255, 255);
            rtfLogic2.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("rtfLogic2.ServiceColors");
            rtfLogic2.ShowCaretWhenInactive = true;
            rtfLogic2.ShowFoldingLines = true;
            rtfLogic2.Size = new System.Drawing.Size(600, 147);
            rtfLogic2.SourceTextBox = rtfLogic1;
            rtfLogic2.TabIndex = 1;
            rtfLogic2.Zoom = 100;
            rtfLogic2.ToolTipNeeded += fctb_ToolTipNeeded;
            rtfLogic2.TextChanged += fctb_TextChanged;
            rtfLogic2.SelectionChanged += fctb_SelectionChanged;
            rtfLogic2.KeyPressed += fctb_KeyPressed;
            rtfLogic2.Enter += fctb_Enter;
            rtfLogic2.KeyUp += fctb_KeyUp;
            rtfLogic2.MouseDown += fctb_MouseDown;
            rtfLogic2.MouseUp += fctb_MouseUp;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuEUndo, mnuERedo, mnuESep0, mnuECut, mnuEDelete, mnuECopy, mnuEPaste, mnuESelectAll, mnuESep1, mnuEFind, mnuEFindAgain, mnuEReplace, mnuESep2, mnuEListDefines, mnuEListCommands, mnuEViewSynonym, mnuESnippet, mnuEBlockCmt, mnuEUnblockCmt, mnuEOpenRes, mnuESep3, mnuEDocumentMap, mnuELineNumbers, mnuECharMap });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(263, 468);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // mnuEListCommands
            // 
            mnuEListCommands.Name = "mnuEListCommands";
            mnuEListCommands.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.J;
            mnuEListCommands.Size = new System.Drawing.Size(262, 22);
            mnuEListCommands.Text = "List Commands";
            mnuEListCommands.Click += mnuEListCommands_Click;
            // 
            // rtfLogic1
            // 
            rtfLogic1.AutoCompleteBracketsList = new char[]
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
            rtfLogic1.AutoIndentCharsPatterns = "";
            rtfLogic1.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            rtfLogic1.BackBrush = null;
            rtfLogic1.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            rtfLogic1.CharHeight = 14;
            rtfLogic1.CharWidth = 8;
            rtfLogic1.CommentPrefix = "[";
            rtfLogic1.ContextMenuStrip = contextMenuStrip1;
            rtfLogic1.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            rtfLogic1.Dock = System.Windows.Forms.DockStyle.Fill;
            rtfLogic1.FindEndOfFoldingBlockStrategy = FastColoredTextBoxNS.FindEndOfFoldingBlockStrategy.Strategy2;
            rtfLogic1.Hotkeys = resources.GetString("rtfLogic1.Hotkeys");
            rtfLogic1.IsReplaceMode = false;
            rtfLogic1.LineNumberStartValue = 0U;
            rtfLogic1.Location = new System.Drawing.Point(0, 0);
            rtfLogic1.Name = "rtfLogic1";
            rtfLogic1.NoMouse = false;
            rtfLogic1.Paddings = new System.Windows.Forms.Padding(0);
            rtfLogic1.SelectionColor = System.Drawing.Color.FromArgb(60, 255, 255, 255);
            rtfLogic1.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("rtfLogic1.ServiceColors");
            rtfLogic1.ShowCaretWhenInactive = true;
            rtfLogic1.Size = new System.Drawing.Size(600, 158);
            rtfLogic1.TabIndex = 0;
            rtfLogic1.Zoom = 100;
            rtfLogic1.ToolTipNeeded += fctb_ToolTipNeeded;
            rtfLogic1.TextChanged += fctb_TextChanged;
            rtfLogic1.SelectionChanged += fctb_SelectionChanged;
            rtfLogic1.KeyPressed += fctb_KeyPressed;
            rtfLogic1.Enter += fctb_Enter;
            rtfLogic1.KeyUp += fctb_KeyUp;
            rtfLogic1.MouseDown += fctb_MouseDown;
            rtfLogic1.MouseUp += fctb_MouseUp;
            // 
            // documentMap1
            // 
            documentMap1.Dock = System.Windows.Forms.DockStyle.Fill;
            documentMap1.ForeColor = System.Drawing.Color.Maroon;
            documentMap1.Location = new System.Drawing.Point(0, 0);
            documentMap1.Name = "documentMap1";
            documentMap1.Size = new System.Drawing.Size(115, 309);
            documentMap1.TabIndex = 4;
            documentMap1.Target = rtfLogic1;
            documentMap1.Text = "documentMap1";
            // 
            // picTip
            // 
            picTip.BackColor = System.Drawing.SystemColors.Info;
            picTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picTip.Location = new System.Drawing.Point(377, 67);
            picTip.Name = "picTip";
            picTip.Size = new System.Drawing.Size(161, 25);
            picTip.TabIndex = 5;
            picTip.TabStop = false;
            picTip.Visible = false;
            picTip.Paint += picTip_Paint;
            // 
            // lstDefines
            // 
            lstDefines.AutoArrange = false;
            lstDefines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstDefines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1 });
            lstDefines.FullRowSelect = true;
            lstDefines.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            lstDefines.HideSelection = true;
            lstDefines.Items.AddRange(new System.Windows.Forms.ListViewItem[] { listViewItem1, listViewItem2 });
            lstDefines.Location = new System.Drawing.Point(408, 44);
            lstDefines.MultiSelect = false;
            lstDefines.Name = "lstDefines";
            lstDefines.ShowGroups = false;
            lstDefines.ShowItemToolTips = true;
            lstDefines.Size = new System.Drawing.Size(233, 210);
            lstDefines.SmallImageList = imageList1;
            lstDefines.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lstDefines.TabIndex = 6;
            lstDefines.TabStop = false;
            lstDefines.UseCompatibleStateImageBehavior = false;
            lstDefines.View = System.Windows.Forms.View.Details;
            lstDefines.Visible = false;
            lstDefines.VisibleChanged += lstDefines_VisibleChanged;
            lstDefines.KeyDown += lstDefines_KeyDown;
            lstDefines.KeyPress += lstDefines_KeyPress;
            lstDefines.MouseDoubleClick += lstDefines_MouseDoubleClick;
            lstDefines.PreviewKeyDown += lstDefines_PreviewKeyDown;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 233;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "def_lnum.ico");
            imageList1.Images.SetKeyName(1, "def_lvar.ico");
            imageList1.Images.SetKeyName(2, "def_lflag.ico");
            imageList1.Images.SetKeyName(3, "def_lmsg.ico");
            imageList1.Images.SetKeyName(4, "def_lsobj.ico");
            imageList1.Images.SetKeyName(5, "def_liobj.ico");
            imageList1.Images.SetKeyName(6, "def_lstr.ico");
            imageList1.Images.SetKeyName(7, "def_lword.ico");
            imageList1.Images.SetKeyName(8, "def_lctrl.ico");
            imageList1.Images.SetKeyName(9, "def_ldefstr.ico");
            imageList1.Images.SetKeyName(10, "def_lvocwrd.ico");
            imageList1.Images.SetKeyName(11, "def_gnum.ico");
            imageList1.Images.SetKeyName(12, "def_gvar.ico");
            imageList1.Images.SetKeyName(13, "def_gflag.ico");
            imageList1.Images.SetKeyName(14, "def_gmsg.ico");
            imageList1.Images.SetKeyName(15, "def_gsobj.ico");
            imageList1.Images.SetKeyName(16, "def_giobj.ico");
            imageList1.Images.SetKeyName(17, "def_gstr.ico");
            imageList1.Images.SetKeyName(18, "def_gword.ico");
            imageList1.Images.SetKeyName(19, "def_gctrl.ico");
            imageList1.Images.SetKeyName(20, "def_gdefstr.ico");
            imageList1.Images.SetKeyName(21, "def_gvocwrd.ico");
            imageList1.Images.SetKeyName(22, "def_glogic.ico");
            imageList1.Images.SetKeyName(23, "def_gpicture.ico");
            imageList1.Images.SetKeyName(24, "def_gsound.ico");
            imageList1.Images.SetKeyName(25, "def_gview.ico");
            imageList1.Images.SetKeyName(26, "def_rnum.ico");
            imageList1.Images.SetKeyName(27, "def_rvar.ico");
            imageList1.Images.SetKeyName(28, "def_rflag.ico");
            imageList1.Images.SetKeyName(29, "def_rmsg.ico");
            imageList1.Images.SetKeyName(30, "def_rsobj.ico");
            imageList1.Images.SetKeyName(31, "def_riobj.ico");
            imageList1.Images.SetKeyName(32, "def_rstr.ico");
            imageList1.Images.SetKeyName(33, "def_rword.ico");
            imageList1.Images.SetKeyName(34, "def_rctrl.ico");
            imageList1.Images.SetKeyName(35, "def_rdefstr.ico");
            imageList1.Images.SetKeyName(36, "def_rvocwrd.ico");
            // 
            // frmLogicEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(719, 342);
            Controls.Add(lstDefines);
            Controls.Add(picTip);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Controls.Add(toolStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            Name = "frmLogicEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "Form1";
            Activated += frmLogicEdit_Activated;
            FormClosing += frmLogicEdit_FormClosing;
            FormClosed += frmLogicEdit_FormClosed;
            KeyDown += frmLogicEdit_KeyDown;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitLogic.Panel1.ResumeLayout(false);
            splitLogic.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitLogic).EndInit();
            splitLogic.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rtfLogic2).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rtfLogic1).EndInit();
            ((System.ComponentModel.ISupportInitialize)picTip).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuResource;
        private System.Windows.Forms.ToolStripMenuItem mnuRInGame;
        private System.Windows.Forms.ToolStripMenuItem mnuRRenumber;
        private System.Windows.Forms.ToolStripMenuItem mnuRProperties;
        private System.Windows.Forms.ToolStripMenuItem mnuRCompile;
        private System.Windows.Forms.ToolStripMenuItem mnuRSavePicImage;
        private System.Windows.Forms.ToolStripMenuItem mnuRExportLoopGIF;
        private System.Windows.Forms.ToolStripMenuItem mnuRMsgCleanup;
        private System.Windows.Forms.ToolStripMenuItem mnuRIsRoom;
        private System.Windows.Forms.ToolStripMenuItem mnuROpenRes;
        private System.Windows.Forms.ToolStripMenuItem mnuRSave;
        private System.Windows.Forms.ToolStripMenuItem mnuRExport;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuEUndo;
        private System.Windows.Forms.ToolStripMenuItem mnuERedo;
        private System.Windows.Forms.ToolStripSeparator mnuESep0;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem mnuEDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuESelectAll;
        private System.Windows.Forms.ToolStripSeparator mnuESep1;
        private System.Windows.Forms.ToolStripMenuItem mnuEFind;
        private System.Windows.Forms.ToolStripMenuItem mnuEFindAgain;
        private System.Windows.Forms.ToolStripMenuItem mnuEReplace;
        private System.Windows.Forms.ToolStripSeparator mnuESep2;
        private System.Windows.Forms.ToolStripMenuItem mnuEListDefines;
        private System.Windows.Forms.ToolStripMenuItem mnuEViewSynonym;
        private System.Windows.Forms.ToolStripMenuItem mnuESnippet;
        private System.Windows.Forms.ToolStripMenuItem mnuEBlockCmt;
        private System.Windows.Forms.ToolStripMenuItem mnuEUnblockCmt;
        private System.Windows.Forms.ToolStripMenuItem mnuEOpenRes;
        private System.Windows.Forms.ToolStripSeparator mnuESep3;
        private System.Windows.Forms.ToolStripMenuItem mnuEDocumentMap;
        private System.Windows.Forms.ToolStripMenuItem mnuELineNumbers;
        private System.Windows.Forms.ToolStripMenuItem mnuECharMap;
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
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FastColoredTextBoxNS.DocumentMap documentMap1;
        private System.Windows.Forms.SplitContainer splitLogic;
        internal WinAGIFCTB rtfLogic1;
        internal WinAGIFCTB rtfLogic2;
        private System.Windows.Forms.PictureBox picTip;
        private System.Windows.Forms.ListView lstDefines;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ToolStripMenuItem mnuEListCommands;
        private System.Windows.Forms.ToolStripButton btnCharMap;
    }
}