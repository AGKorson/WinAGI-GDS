
namespace WinAGI.Editor {
    partial class frmSoundEdit {
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("End");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Track 0", new System.Windows.Forms.TreeNode[] { treeNode1 });
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("End");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Track 1", new System.Windows.Forms.TreeNode[] { treeNode3 });
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("End");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Track 2", new System.Windows.Forms.TreeNode[] { treeNode5 });
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("End");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Noise", new System.Windows.Forms.TreeNode[] { treeNode7 });
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("soundid", new System.Windows.Forms.TreeNode[] { treeNode2, treeNode4, treeNode6, treeNode8 });
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSoundEdit));
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
            mnuRPlaySound = new System.Windows.Forms.ToolStripMenuItem();
            mnuPlaybackMode = new System.Windows.Forms.ToolStripMenuItem();
            mnuPCSpeaker = new System.Windows.Forms.ToolStripMenuItem();
            mnuPCjr = new System.Windows.Forms.ToolStripMenuItem();
            mnuMIDI = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer3 = new System.Windows.Forms.SplitContainer();
            tvwSound = new MultiNodeTreeview();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuUndo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            mnuCut = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuClear = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            mnuToneUp = new System.Windows.Forms.ToolStripMenuItem();
            mnuToneDown = new System.Windows.Forms.ToolStripMenuItem();
            mnuVolumeUp = new System.Windows.Forms.ToolStripMenuItem();
            mnuVolumeDown = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            mnuKeyboard = new System.Windows.Forms.ToolStripMenuItem();
            mnuNoKybdSound = new System.Windows.Forms.ToolStripMenuItem();
            mnuOneTrack = new System.Windows.Forms.ToolStripMenuItem();
            propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            btnStop = new System.Windows.Forms.Button();
            btnPlay = new System.Windows.Forms.Button();
            toolStrip2 = new System.Windows.Forms.ToolStrip();
            tsbToneUp = new System.Windows.Forms.ToolStripButton();
            tsbToneDown = new System.Windows.Forms.ToolStripButton();
            tsbVolumeUp = new System.Windows.Forms.ToolStripButton();
            tsbVolumeDown = new System.Windows.Forms.ToolStripButton();
            tsbDelete = new System.Windows.Forms.ToolStripButton();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbZoomIn = new System.Windows.Forms.ToolStripButton();
            tsbZoomOut = new System.Windows.Forms.ToolStripButton();
            tsbCut = new System.Windows.Forms.ToolStripButton();
            tsbCopy = new System.Windows.Forms.ToolStripButton();
            tsbPaste = new System.Windows.Forms.ToolStripButton();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            vsbStaff3 = new System.Windows.Forms.VScrollBar();
            btnMute3 = new System.Windows.Forms.Button();
            btnMute2 = new System.Windows.Forms.Button();
            btnMute1 = new System.Windows.Forms.Button();
            btnMute0 = new System.Windows.Forms.Button();
            vsbStaff2 = new System.Windows.Forms.VScrollBar();
            vsbStaff1 = new System.Windows.Forms.VScrollBar();
            vsbStaff0 = new System.Windows.Forms.VScrollBar();
            picStaff0 = new SelectablePictureBox();
            picStaff1 = new SelectablePictureBox();
            picStaff2 = new SelectablePictureBox();
            picStaff3 = new SelectablePictureBox();
            hsbStaff = new System.Windows.Forms.HScrollBar();
            picKeyboard = new System.Windows.Forms.PictureBox();
            picDuration = new System.Windows.Forms.PictureBox();
            btnDurationDown = new System.Windows.Forms.Button();
            btnDurationUp = new System.Windows.Forms.Button();
            btnKybdRight = new System.Windows.Forms.Button();
            btnKybdLeft = new System.Windows.Forms.Button();
            tmrStaffScroll = new System.Windows.Forms.Timer(components);
            tmrKeyboardScroll = new System.Windows.Forms.Timer(components);
            tmrCursor = new System.Windows.Forms.Timer(components);
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            toolStrip2.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picStaff0).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picStaff1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picStaff2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picStaff3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picKeyboard).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picDuration).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(556, 24);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRExport, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportLoopGIF, mnuRPlaySound, mnuPlaybackMode });
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
            mnuRSave.Text = "&Save Sound";
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
            mnuRRenumber.Text = "Renumber Sound";
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
            mnuRExportLoopGIF.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRExportLoopGIF.MergeIndex = 11;
            mnuRExportLoopGIF.Name = "mnuRExportLoopGIF";
            mnuRExportLoopGIF.Size = new System.Drawing.Size(225, 22);
            mnuRExportLoopGIF.Text = "export loop gif";
            // 
            // mnuRPlaySound
            // 
            mnuRPlaySound.Name = "mnuRPlaySound";
            mnuRPlaySound.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Enter;
            mnuRPlaySound.Size = new System.Drawing.Size(225, 22);
            mnuRPlaySound.Text = "Play Sound";
            mnuRPlaySound.Click += mnuRPlaySound_Click;
            // 
            // mnuPlaybackMode
            // 
            mnuPlaybackMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuPCSpeaker, mnuPCjr, mnuMIDI });
            mnuPlaybackMode.Name = "mnuPlaybackMode";
            mnuPlaybackMode.Size = new System.Drawing.Size(225, 22);
            mnuPlaybackMode.Text = "Playback Mode";
            // 
            // mnuPCSpeaker
            // 
            mnuPCSpeaker.Enabled = false;
            mnuPCSpeaker.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuPCSpeaker.Name = "mnuPCSpeaker";
            mnuPCSpeaker.Size = new System.Drawing.Size(184, 22);
            mnuPCSpeaker.Text = "PC Speaker Emulator";
            mnuPCSpeaker.Click += mnuPCSpeaker_Click;
            // 
            // mnuPCjr
            // 
            mnuPCjr.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuPCjr.Name = "mnuPCjr";
            mnuPCjr.Size = new System.Drawing.Size(184, 22);
            mnuPCjr.Text = "PC Jr Emulator";
            mnuPCjr.Click += mnuPCjr_Click;
            // 
            // mnuMIDI
            // 
            mnuMIDI.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuMIDI.Name = "mnuMIDI";
            mnuMIDI.Size = new System.Drawing.Size(184, 22);
            mnuMIDI.Text = "MIDI Conversion";
            mnuMIDI.Click += mnuMIDI_Click;
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
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer1.Location = new System.Drawing.Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer3);
            splitContainer1.Panel1.Controls.Add(btnStop);
            splitContainer1.Panel1.Controls.Add(btnPlay);
            splitContainer1.Panel1.Controls.Add(toolStrip2);
            splitContainer1.Panel1.Controls.Add(toolStrip1);
            splitContainer1.Panel1MinSize = 130;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new System.Drawing.Size(556, 280);
            splitContainer1.SplitterDistance = 130;
            splitContainer1.TabIndex = 6;
            splitContainer1.TabStop = false;
            splitContainer1.MouseUp += splitContainer1_MouseUp;
            // 
            // splitContainer3
            // 
            splitContainer3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer3.Location = new System.Drawing.Point(0, 75);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(tvwSound);
            splitContainer3.Panel1MinSize = 50;
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(propertyGrid1);
            splitContainer3.Panel2MinSize = 50;
            splitContainer3.Size = new System.Drawing.Size(130, 205);
            splitContainer3.SplitterDistance = 81;
            splitContainer3.TabIndex = 8;
            splitContainer3.TabStop = false;
            splitContainer3.MouseUp += splitContainer3_MouseUp;
            // 
            // tvwSound
            // 
            tvwSound.ContextMenuStrip = contextMenuStrip1;
            tvwSound.Dock = System.Windows.Forms.DockStyle.Fill;
            tvwSound.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            tvwSound.HideSelection = false;
            tvwSound.Location = new System.Drawing.Point(0, 0);
            tvwSound.Name = "tvwSound";
            treeNode1.Name = "Node5";
            treeNode1.Text = "End";
            treeNode2.Name = "Node1";
            treeNode2.Text = "Track 0";
            treeNode3.Name = "Node8";
            treeNode3.Text = "End";
            treeNode4.Name = "Node2";
            treeNode4.Text = "Track 1";
            treeNode5.Name = "Node7";
            treeNode5.Text = "End";
            treeNode6.Name = "Node3";
            treeNode6.Text = "Track 2";
            treeNode7.Name = "Node6";
            treeNode7.Text = "End";
            treeNode8.Name = "Node4";
            treeNode8.Text = "Noise";
            treeNode9.Name = "Node0";
            treeNode9.Text = "soundid";
            tvwSound.Nodes.AddRange(new System.Windows.Forms.TreeNode[] { treeNode9 });
            tvwSound.NoSelection = false;
            tvwSound.SelectedNodes = (System.Collections.Generic.List<System.Windows.Forms.TreeNode>)resources.GetObject("tvwSound.SelectedNodes");
            tvwSound.ShowRootLines = false;
            tvwSound.Size = new System.Drawing.Size(130, 81);
            tvwSound.TabIndex = 3;
            tvwSound.AfterCollapse += tvwSound_After;
            tvwSound.AfterExpand += tvwSound_After;
            tvwSound.NodeMouseClick += tvwSound_NodeMouseClick;
            tvwSound.NodeMouseDoubleClick += tvwSound_NodeMouseDoubleClick;
            tvwSound.KeyDown += tvwSound_KeyDown;
            tvwSound.KeyPress += tvwSound_KeyPress;
            tvwSound.KeyUp += tvwSound_KeyUp;
            tvwSound.MouseUp += tvwSound_MouseUp;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuUndo, toolStripSeparator1, mnuCut, mnuCopy, mnuPaste, mnuDelete, mnuSelectAll, mnuClear, toolStripSeparator2, mnuToneUp, mnuToneDown, mnuVolumeUp, mnuVolumeDown, toolStripSeparator3, mnuKeyboard, mnuNoKybdSound, mnuOneTrack });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(253, 330);
            contextMenuStrip1.Closed += contextMenuStrip1_Closed;
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // mnuUndo
            // 
            mnuUndo.Name = "mnuUndo";
            mnuUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuUndo.Size = new System.Drawing.Size(252, 22);
            mnuUndo.Text = "Undo";
            mnuUndo.Click += mnuUndo_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(249, 6);
            // 
            // mnuCut
            // 
            mnuCut.Name = "mnuCut";
            mnuCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuCut.Size = new System.Drawing.Size(252, 22);
            mnuCut.Text = "Cut";
            mnuCut.Click += mnuCut_Click;
            // 
            // mnuCopy
            // 
            mnuCopy.Name = "mnuCopy";
            mnuCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuCopy.Size = new System.Drawing.Size(252, 22);
            mnuCopy.Text = "Copy";
            mnuCopy.Click += mnuCopy_Click;
            // 
            // mnuPaste
            // 
            mnuPaste.Name = "mnuPaste";
            mnuPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuPaste.Size = new System.Drawing.Size(252, 22);
            mnuPaste.Text = "Paste";
            mnuPaste.Click += mnuPaste_Click;
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuDelete.Size = new System.Drawing.Size(252, 22);
            mnuDelete.Text = "Delete";
            mnuDelete.Click += mnuDelete_Click;
            // 
            // mnuSelectAll
            // 
            mnuSelectAll.Name = "mnuSelectAll";
            mnuSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuSelectAll.Size = new System.Drawing.Size(252, 22);
            mnuSelectAll.Text = "Select All";
            mnuSelectAll.Click += mnuSelectAll_Click;
            // 
            // mnuClear
            // 
            mnuClear.Name = "mnuClear";
            mnuClear.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete;
            mnuClear.Size = new System.Drawing.Size(252, 22);
            mnuClear.Text = "Clear";
            mnuClear.Click += mnuClear_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(249, 6);
            // 
            // mnuToneUp
            // 
            mnuToneUp.Name = "mnuToneUp";
            mnuToneUp.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U;
            mnuToneUp.Size = new System.Drawing.Size(252, 22);
            mnuToneUp.Text = "Shift Up";
            mnuToneUp.Click += mnuToneUp_Click;
            // 
            // mnuToneDown
            // 
            mnuToneDown.Name = "mnuToneDown";
            mnuToneDown.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D;
            mnuToneDown.Size = new System.Drawing.Size(252, 22);
            mnuToneDown.Text = "Shift Down";
            mnuToneDown.Click += mnuToneDown_Click;
            // 
            // mnuVolumeUp
            // 
            mnuVolumeUp.Name = "mnuVolumeUp";
            mnuVolumeUp.ShortcutKeyDisplayString = "+";
            mnuVolumeUp.Size = new System.Drawing.Size(252, 22);
            mnuVolumeUp.Text = "Volume Up";
            mnuVolumeUp.Click += mnuVolumeUp_Click;
            // 
            // mnuVolumeDown
            // 
            mnuVolumeDown.Name = "mnuVolumeDown";
            mnuVolumeDown.ShortcutKeyDisplayString = "-";
            mnuVolumeDown.Size = new System.Drawing.Size(252, 22);
            mnuVolumeDown.Text = "Volume Down";
            mnuVolumeDown.Click += mnuVolumeDown_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(249, 6);
            // 
            // mnuKeyboard
            // 
            mnuKeyboard.Name = "mnuKeyboard";
            mnuKeyboard.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K;
            mnuKeyboard.Size = new System.Drawing.Size(252, 22);
            mnuKeyboard.Text = "Hide Keyboard";
            mnuKeyboard.Click += mnuKeyboard_Click;
            // 
            // mnuNoKybdSound
            // 
            mnuNoKybdSound.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuNoKybdSound.Name = "mnuNoKybdSound";
            mnuNoKybdSound.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M;
            mnuNoKybdSound.Size = new System.Drawing.Size(252, 22);
            mnuNoKybdSound.Text = "Keyboard Sound";
            mnuNoKybdSound.Click += mnuNoKybdSound_Click;
            // 
            // mnuOneTrack
            // 
            mnuOneTrack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuOneTrack.Name = "mnuOneTrack";
            mnuOneTrack.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            mnuOneTrack.Size = new System.Drawing.Size(252, 22);
            mnuOneTrack.Text = "Show Selected Track Only";
            mnuOneTrack.Click += mnuOneTrack_Click;
            // 
            // propertyGrid1
            // 
            propertyGrid1.CanShowVisualStyleGlyphs = false;
            propertyGrid1.CommandsVisibleIfAvailable = false;
            propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            propertyGrid1.HelpVisible = false;
            propertyGrid1.Location = new System.Drawing.Point(0, 0);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            propertyGrid1.Size = new System.Drawing.Size(130, 120);
            propertyGrid1.TabIndex = 8;
            propertyGrid1.ToolbarVisible = false;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Image = (System.Drawing.Image)resources.GetObject("btnStop.Image");
            btnStop.Location = new System.Drawing.Point(65, 50);
            btnStop.Name = "btnStop";
            btnStop.Size = new System.Drawing.Size(66, 22);
            btnStop.TabIndex = 5;
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnPlay
            // 
            btnPlay.Image = (System.Drawing.Image)resources.GetObject("btnPlay.Image");
            btnPlay.Location = new System.Drawing.Point(1, 50);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new System.Drawing.Size(66, 22);
            btnPlay.TabIndex = 4;
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // toolStrip2
            // 
            toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbToneUp, tsbToneDown, tsbVolumeUp, tsbVolumeDown, tsbDelete });
            toolStrip2.Location = new System.Drawing.Point(0, 25);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new System.Drawing.Size(130, 25);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // tsbToneUp
            // 
            tsbToneUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbToneUp.Image = (System.Drawing.Image)resources.GetObject("tsbToneUp.Image");
            tsbToneUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbToneUp.Name = "tsbToneUp";
            tsbToneUp.Size = new System.Drawing.Size(23, 22);
            tsbToneUp.Text = "toolStripButton6";
            tsbToneUp.ToolTipText = "Shift Up";
            tsbToneUp.Click += mnuToneUp_Click;
            // 
            // tsbToneDown
            // 
            tsbToneDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbToneDown.Image = (System.Drawing.Image)resources.GetObject("tsbToneDown.Image");
            tsbToneDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbToneDown.Name = "tsbToneDown";
            tsbToneDown.Size = new System.Drawing.Size(23, 22);
            tsbToneDown.Text = "toolStripButton7";
            tsbToneDown.ToolTipText = "Shift Down";
            tsbToneDown.Click += mnuToneDown_Click;
            // 
            // tsbVolumeUp
            // 
            tsbVolumeUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbVolumeUp.Image = (System.Drawing.Image)resources.GetObject("tsbVolumeUp.Image");
            tsbVolumeUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbVolumeUp.Name = "tsbVolumeUp";
            tsbVolumeUp.Size = new System.Drawing.Size(23, 22);
            tsbVolumeUp.Text = "toolStripButton8";
            tsbVolumeUp.ToolTipText = "Volume Up";
            tsbVolumeUp.Click += mnuVolumeUp_Click;
            // 
            // tsbVolumeDown
            // 
            tsbVolumeDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbVolumeDown.Image = (System.Drawing.Image)resources.GetObject("tsbVolumeDown.Image");
            tsbVolumeDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbVolumeDown.Name = "tsbVolumeDown";
            tsbVolumeDown.Size = new System.Drawing.Size(23, 22);
            tsbVolumeDown.Text = "toolStripButton9";
            tsbVolumeDown.ToolTipText = "Volume Down";
            tsbVolumeDown.Click += mnuVolumeDown_Click;
            // 
            // tsbDelete
            // 
            tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbDelete.Image = (System.Drawing.Image)resources.GetObject("tsbDelete.Image");
            tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbDelete.Name = "tsbDelete";
            tsbDelete.Size = new System.Drawing.Size(23, 22);
            tsbDelete.Text = "toolStripButton10";
            tsbDelete.ToolTipText = "Delete";
            tsbDelete.Click += mnuDelete_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbZoomIn, tsbZoomOut, tsbCut, tsbCopy, tsbPaste });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(130, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbZoomIn
            // 
            tsbZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbZoomIn.Image = (System.Drawing.Image)resources.GetObject("tsbZoomIn.Image");
            tsbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomIn.Name = "tsbZoomIn";
            tsbZoomIn.Size = new System.Drawing.Size(23, 22);
            tsbZoomIn.Text = "toolStripButton1";
            tsbZoomIn.ToolTipText = "Zoom In";
            tsbZoomIn.Click += tsbZoomIn_Click;
            // 
            // tsbZoomOut
            // 
            tsbZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbZoomOut.Image = (System.Drawing.Image)resources.GetObject("tsbZoomOut.Image");
            tsbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomOut.Name = "tsbZoomOut";
            tsbZoomOut.Size = new System.Drawing.Size(23, 22);
            tsbZoomOut.Text = "toolStripButton2";
            tsbZoomOut.ToolTipText = "Zoom Out";
            tsbZoomOut.Click += tsbZoomOut_Click;
            // 
            // tsbCut
            // 
            tsbCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbCut.Image = (System.Drawing.Image)resources.GetObject("tsbCut.Image");
            tsbCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCut.Name = "tsbCut";
            tsbCut.Size = new System.Drawing.Size(23, 22);
            tsbCut.Text = "toolStripButton3";
            tsbCut.ToolTipText = "Cut";
            tsbCut.Click += mnuCut_Click;
            // 
            // tsbCopy
            // 
            tsbCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbCopy.Image = (System.Drawing.Image)resources.GetObject("tsbCopy.Image");
            tsbCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCopy.Name = "tsbCopy";
            tsbCopy.Size = new System.Drawing.Size(23, 22);
            tsbCopy.Text = "toolStripButton4";
            tsbCopy.ToolTipText = "Copy";
            tsbCopy.Click += mnuCopy_Click;
            // 
            // tsbPaste
            // 
            tsbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbPaste.Image = (System.Drawing.Image)resources.GetObject("tsbPaste.Image");
            tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbPaste.Name = "tsbPaste";
            tsbPaste.Size = new System.Drawing.Size(23, 22);
            tsbPaste.Text = "toolStripButton5";
            tsbPaste.ToolTipText = "Paste";
            tsbPaste.Click += mnuPaste_Click;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer2.IsSplitterFixed = true;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(vsbStaff3);
            splitContainer2.Panel1.Controls.Add(btnMute3);
            splitContainer2.Panel1.Controls.Add(btnMute2);
            splitContainer2.Panel1.Controls.Add(btnMute1);
            splitContainer2.Panel1.Controls.Add(btnMute0);
            splitContainer2.Panel1.Controls.Add(vsbStaff2);
            splitContainer2.Panel1.Controls.Add(vsbStaff1);
            splitContainer2.Panel1.Controls.Add(vsbStaff0);
            splitContainer2.Panel1.Controls.Add(picStaff0);
            splitContainer2.Panel1.Controls.Add(picStaff1);
            splitContainer2.Panel1.Controls.Add(picStaff2);
            splitContainer2.Panel1.Controls.Add(picStaff3);
            splitContainer2.Panel1.Controls.Add(hsbStaff);
            splitContainer2.Panel1.Resize += splitContainer2_Panel1_Resize;
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(picKeyboard);
            splitContainer2.Panel2.Controls.Add(picDuration);
            splitContainer2.Panel2.Controls.Add(btnDurationDown);
            splitContainer2.Panel2.Controls.Add(btnDurationUp);
            splitContainer2.Panel2.Controls.Add(btnKybdRight);
            splitContainer2.Panel2.Controls.Add(btnKybdLeft);
            splitContainer2.Size = new System.Drawing.Size(422, 280);
            splitContainer2.SplitterDistance = 210;
            splitContainer2.TabIndex = 0;
            splitContainer2.TabStop = false;
            splitContainer2.MouseUp += splitContainer2_MouseUp;
            // 
            // vsbStaff3
            // 
            vsbStaff3.Location = new System.Drawing.Point(397, 0);
            vsbStaff3.Name = "vsbStaff3";
            vsbStaff3.Size = new System.Drawing.Size(17, 43);
            vsbStaff3.TabIndex = 10;
            vsbStaff3.ValueChanged += vsbStaff_ValueChanged;
            // 
            // btnMute3
            // 
            btnMute3.Image = (System.Drawing.Image)resources.GetObject("btnMute3.Image");
            btnMute3.Location = new System.Drawing.Point(0, 0);
            btnMute3.Name = "btnMute3";
            btnMute3.Size = new System.Drawing.Size(24, 24);
            btnMute3.TabIndex = 8;
            btnMute3.UseVisualStyleBackColor = true;
            btnMute3.Click += btnMute_Click;
            // 
            // btnMute2
            // 
            btnMute2.Image = (System.Drawing.Image)resources.GetObject("btnMute2.Image");
            btnMute2.Location = new System.Drawing.Point(0, 0);
            btnMute2.Name = "btnMute2";
            btnMute2.Size = new System.Drawing.Size(24, 24);
            btnMute2.TabIndex = 8;
            btnMute2.UseVisualStyleBackColor = true;
            btnMute2.Click += btnMute_Click;
            // 
            // btnMute1
            // 
            btnMute1.Image = (System.Drawing.Image)resources.GetObject("btnMute1.Image");
            btnMute1.Location = new System.Drawing.Point(0, 0);
            btnMute1.Name = "btnMute1";
            btnMute1.Size = new System.Drawing.Size(24, 24);
            btnMute1.TabIndex = 8;
            btnMute1.UseVisualStyleBackColor = true;
            btnMute1.Click += btnMute_Click;
            // 
            // btnMute0
            // 
            btnMute0.Image = (System.Drawing.Image)resources.GetObject("btnMute0.Image");
            btnMute0.Location = new System.Drawing.Point(-1, 0);
            btnMute0.Name = "btnMute0";
            btnMute0.Size = new System.Drawing.Size(24, 24);
            btnMute0.TabIndex = 7;
            btnMute0.UseVisualStyleBackColor = true;
            btnMute0.Click += btnMute_Click;
            // 
            // vsbStaff2
            // 
            vsbStaff2.Location = new System.Drawing.Point(367, 0);
            vsbStaff2.Name = "vsbStaff2";
            vsbStaff2.Size = new System.Drawing.Size(17, 43);
            vsbStaff2.TabIndex = 9;
            vsbStaff2.ValueChanged += vsbStaff_ValueChanged;
            // 
            // vsbStaff1
            // 
            vsbStaff1.Location = new System.Drawing.Point(337, 0);
            vsbStaff1.Name = "vsbStaff1";
            vsbStaff1.Size = new System.Drawing.Size(17, 43);
            vsbStaff1.TabIndex = 8;
            vsbStaff1.ValueChanged += vsbStaff_ValueChanged;
            // 
            // vsbStaff0
            // 
            vsbStaff0.Location = new System.Drawing.Point(307, 0);
            vsbStaff0.Name = "vsbStaff0";
            vsbStaff0.Size = new System.Drawing.Size(17, 43);
            vsbStaff0.TabIndex = 7;
            vsbStaff0.ValueChanged += vsbStaff_ValueChanged;
            // 
            // picStaff0
            // 
            picStaff0.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            picStaff0.BackColor = System.Drawing.Color.White;
            picStaff0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            picStaff0.ContextMenuStrip = contextMenuStrip1;
            picStaff0.Location = new System.Drawing.Point(0, 0);
            picStaff0.Name = "picStaff0";
            picStaff0.ShowFocusRectangle = false;
            picStaff0.Size = new System.Drawing.Size(422, 55);
            picStaff0.TabIndex = 1;
            picStaff0.Visible = false;
            picStaff0.Paint += picStaff_Paint;
            picStaff0.MouseDoubleClick += picStaff_MouseDoubleClick;
            picStaff0.MouseDown += picStaff_MouseDown;
            picStaff0.MouseLeave += picStaff_MouseLeave;
            picStaff0.MouseMove += picStaff_MouseMove;
            picStaff0.MouseUp += picStaff_MouseUp;
            // 
            // picStaff1
            // 
            picStaff1.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            picStaff1.BackColor = System.Drawing.Color.White;
            picStaff1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            picStaff1.ContextMenuStrip = contextMenuStrip1;
            picStaff1.Location = new System.Drawing.Point(0, 43);
            picStaff1.Name = "picStaff1";
            picStaff1.ShowFocusRectangle = false;
            picStaff1.Size = new System.Drawing.Size(422, 55);
            picStaff1.TabIndex = 2;
            picStaff1.Visible = false;
            picStaff1.Paint += picStaff_Paint;
            picStaff1.MouseDoubleClick += picStaff_MouseDoubleClick;
            picStaff1.MouseDown += picStaff_MouseDown;
            picStaff1.MouseLeave += picStaff_MouseLeave;
            picStaff1.MouseMove += picStaff_MouseMove;
            picStaff1.MouseUp += picStaff_MouseUp;
            // 
            // picStaff2
            // 
            picStaff2.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            picStaff2.BackColor = System.Drawing.Color.White;
            picStaff2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            picStaff2.ContextMenuStrip = contextMenuStrip1;
            picStaff2.Location = new System.Drawing.Point(0, 98);
            picStaff2.Name = "picStaff2";
            picStaff2.ShowFocusRectangle = false;
            picStaff2.Size = new System.Drawing.Size(422, 55);
            picStaff2.TabIndex = 3;
            picStaff2.Visible = false;
            picStaff2.Paint += picStaff_Paint;
            picStaff2.MouseDoubleClick += picStaff_MouseDoubleClick;
            picStaff2.MouseDown += picStaff_MouseDown;
            picStaff2.MouseLeave += picStaff_MouseLeave;
            picStaff2.MouseMove += picStaff_MouseMove;
            picStaff2.MouseUp += picStaff_MouseUp;
            // 
            // picStaff3
            // 
            picStaff3.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            picStaff3.BackColor = System.Drawing.Color.White;
            picStaff3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            picStaff3.ContextMenuStrip = contextMenuStrip1;
            picStaff3.Location = new System.Drawing.Point(0, 150);
            picStaff3.Name = "picStaff3";
            picStaff3.ShowFocusRectangle = false;
            picStaff3.Size = new System.Drawing.Size(422, 55);
            picStaff3.TabIndex = 4;
            picStaff3.Visible = false;
            picStaff3.Paint += picStaff_Paint;
            picStaff3.MouseDoubleClick += picStaff_MouseDoubleClick;
            picStaff3.MouseDown += picStaff_MouseDown;
            picStaff3.MouseLeave += picStaff_MouseLeave;
            picStaff3.MouseMove += picStaff_MouseMove;
            picStaff3.MouseUp += picStaff_MouseUp;
            // 
            // hsbStaff
            // 
            hsbStaff.Dock = System.Windows.Forms.DockStyle.Bottom;
            hsbStaff.Location = new System.Drawing.Point(0, 193);
            hsbStaff.Name = "hsbStaff";
            hsbStaff.Size = new System.Drawing.Size(422, 17);
            hsbStaff.TabIndex = 0;
            hsbStaff.ValueChanged += hsbStaff_ValueChanged;
            // 
            // picKeyboard
            // 
            picKeyboard.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            picKeyboard.BackColor = System.Drawing.Color.White;
            picKeyboard.Location = new System.Drawing.Point(34, 0);
            picKeyboard.Name = "picKeyboard";
            picKeyboard.Size = new System.Drawing.Size(334, 66);
            picKeyboard.TabIndex = 5;
            picKeyboard.TabStop = false;
            picKeyboard.Paint += picKeyboard_Paint;
            picKeyboard.MouseDown += picKeyboard_MouseDown;
            picKeyboard.MouseUp += picKeyboard_MouseUp;
            picKeyboard.Resize += picKeyboard_Resize;
            // 
            // picDuration
            // 
            picDuration.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            picDuration.BackColor = System.Drawing.Color.White;
            picDuration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picDuration.Location = new System.Drawing.Point(369, 0);
            picDuration.Name = "picDuration";
            picDuration.Size = new System.Drawing.Size(38, 66);
            picDuration.TabIndex = 4;
            picDuration.TabStop = false;
            picDuration.MouseClick += picDuration_MouseClick;
            picDuration.MouseDoubleClick += picDuration_MouseDoubleClick;
            // 
            // btnDurationDown
            // 
            btnDurationDown.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnDurationDown.BackgroundImage = (System.Drawing.Image)resources.GetObject("btnDurationDown.BackgroundImage");
            btnDurationDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            btnDurationDown.Location = new System.Drawing.Point(405, 33);
            btnDurationDown.Name = "btnDurationDown";
            btnDurationDown.Size = new System.Drawing.Size(17, 33);
            btnDurationDown.TabIndex = 3;
            btnDurationDown.UseVisualStyleBackColor = true;
            btnDurationDown.Click += btnDurationDown_Click;
            // 
            // btnDurationUp
            // 
            btnDurationUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnDurationUp.BackgroundImage = (System.Drawing.Image)resources.GetObject("btnDurationUp.BackgroundImage");
            btnDurationUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            btnDurationUp.Location = new System.Drawing.Point(405, 0);
            btnDurationUp.Name = "btnDurationUp";
            btnDurationUp.Size = new System.Drawing.Size(17, 33);
            btnDurationUp.TabIndex = 2;
            btnDurationUp.UseVisualStyleBackColor = true;
            btnDurationUp.Click += btnDurationUp_Click;
            // 
            // btnKybdRight
            // 
            btnKybdRight.Dock = System.Windows.Forms.DockStyle.Left;
            btnKybdRight.Image = (System.Drawing.Image)resources.GetObject("btnKybdRight.Image");
            btnKybdRight.Location = new System.Drawing.Point(17, 0);
            btnKybdRight.Name = "btnKybdRight";
            btnKybdRight.Size = new System.Drawing.Size(17, 66);
            btnKybdRight.TabIndex = 1;
            btnKybdRight.UseVisualStyleBackColor = true;
            btnKybdRight.MouseDown += btnKybdRight_MouseDown;
            btnKybdRight.MouseUp += btnKybdRight_MouseUp;
            // 
            // btnKybdLeft
            // 
            btnKybdLeft.Dock = System.Windows.Forms.DockStyle.Left;
            btnKybdLeft.Image = (System.Drawing.Image)resources.GetObject("btnKybdLeft.Image");
            btnKybdLeft.Location = new System.Drawing.Point(0, 0);
            btnKybdLeft.Name = "btnKybdLeft";
            btnKybdLeft.Size = new System.Drawing.Size(17, 66);
            btnKybdLeft.TabIndex = 0;
            btnKybdLeft.UseVisualStyleBackColor = true;
            btnKybdLeft.MouseDown += btnKybdLeft_MouseDown;
            btnKybdLeft.MouseUp += btnKybdLeft_MouseUp;
            // 
            // tmrStaffScroll
            // 
            tmrStaffScroll.Tick += tmrStaffScroll_Tick;
            // 
            // tmrKeyboardScroll
            // 
            tmrKeyboardScroll.Interval = 400;
            tmrKeyboardScroll.Tick += tmrKeyboardScroll_Tick;
            // 
            // tmrCursor
            // 
            tmrCursor.Interval = 400;
            tmrCursor.Tick += tmrCursor_Tick;
            // 
            // frmSoundEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(556, 304);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            Name = "frmSoundEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "Sound Editor";
            FormClosing += frmSoundEdit_FormClosing;
            FormClosed += frmSoundEdit_FormClosed;
            Enter += frmSoundEdit_Enter;
            Leave += frmSoundEdit_Leave;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picStaff0).EndInit();
            ((System.ComponentModel.ISupportInitialize)picStaff1).EndInit();
            ((System.ComponentModel.ISupportInitialize)picStaff2).EndInit();
            ((System.ComponentModel.ISupportInitialize)picStaff3).EndInit();
            ((System.ComponentModel.ISupportInitialize)picKeyboard).EndInit();
            ((System.ComponentModel.ISupportInitialize)picDuration).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
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
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbZoomIn;
        private System.Windows.Forms.ToolStripButton tsbZoomOut;
        private System.Windows.Forms.ToolStripButton tsbCut;
        private System.Windows.Forms.ToolStripButton tsbCopy;
        private System.Windows.Forms.ToolStripButton tsbPaste;
        private System.Windows.Forms.ToolStripButton tsbToneUp;
        private System.Windows.Forms.ToolStripButton tsbToneDown;
        private System.Windows.Forms.ToolStripButton tsbVolumeUp;
        private System.Windows.Forms.ToolStripButton tsbVolumeDown;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.Button btnKybdLeft;
        private System.Windows.Forms.Button btnKybdRight;
        private System.Windows.Forms.Button btnDurationDown;
        private System.Windows.Forms.Button btnDurationUp;
        private System.Windows.Forms.PictureBox picKeyboard;
        private System.Windows.Forms.PictureBox picDuration;
        private SelectablePictureBox picStaff0;
        private SelectablePictureBox picStaff1;
        private SelectablePictureBox picStaff2;
        private SelectablePictureBox picStaff3;
        private System.Windows.Forms.HScrollBar hsbStaff;
        private System.Windows.Forms.VScrollBar vsbStaff0;
        private System.Windows.Forms.VScrollBar vsbStaff2;
        private System.Windows.Forms.VScrollBar vsbStaff1;
        private System.Windows.Forms.Button btnMute3;
        private System.Windows.Forms.Button btnMute2;
        private System.Windows.Forms.Button btnMute1;
        private System.Windows.Forms.Button btnMute0;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.VScrollBar vsbStaff3;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private WinAGI.Editor.MultiNodeTreeview tvwSound;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuCut;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuToneUp;
        private System.Windows.Forms.ToolStripMenuItem mnuToneDown;
        private System.Windows.Forms.ToolStripMenuItem mnuVolumeUp;
        private System.Windows.Forms.ToolStripMenuItem mnuVolumeDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuKeyboard;
        private System.Windows.Forms.ToolStripMenuItem mnuOneTrack;
        private System.Windows.Forms.ToolStripMenuItem mnuRPlaySound;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaybackMode;
        private System.Windows.Forms.ToolStripMenuItem mnuPCSpeaker;
        private System.Windows.Forms.ToolStripMenuItem mnuPCjr;
        private System.Windows.Forms.ToolStripMenuItem mnuMIDI;
        private System.Windows.Forms.Timer tmrStaffScroll;
        private System.Windows.Forms.ToolStripMenuItem mnuNoKybdSound;
        private System.Windows.Forms.Timer tmrKeyboardScroll;
        private System.Windows.Forms.ToolStripMenuItem mnuClear;
        private System.Windows.Forms.Timer tmrCursor;
    }
}