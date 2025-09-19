
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Cel 0", 2, 2);
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("End", 3, 3);
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Loop 0", 1, 1, new System.Windows.Forms.TreeNode[] { treeNode1, treeNode2 });
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("End", 3, 3);
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("View", 0, 0, new System.Windows.Forms.TreeNode[] { treeNode3, treeNode4 });
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
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbTool = new System.Windows.Forms.ToolStripDropDownButton();
            tstEdit = new System.Windows.Forms.ToolStripMenuItem();
            tstSelect = new System.Windows.Forms.ToolStripMenuItem();
            tstPencil = new System.Windows.Forms.ToolStripMenuItem();
            tstLine = new System.Windows.Forms.ToolStripMenuItem();
            tstRectangle = new System.Windows.Forms.ToolStripMenuItem();
            tstRectangleSolid = new System.Windows.Forms.ToolStripMenuItem();
            tstFill = new System.Windows.Forms.ToolStripMenuItem();
            tstErase = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsbZoomIn = new System.Windows.Forms.ToolStripButton();
            tsbZoomOut = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tsbUndo = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            tsbCut = new System.Windows.Forms.ToolStripButton();
            tsbCopy = new System.Windows.Forms.ToolStripButton();
            tsbPaste = new System.Windows.Forms.ToolStripButton();
            tsbDelete = new System.Windows.Forms.ToolStripButton();
            tsbInsert = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            tsbFlipH = new System.Windows.Forms.ToolStripButton();
            tsbFlipV = new System.Windows.Forms.ToolStripButton();
            picPalette = new System.Windows.Forms.PictureBox();
            splitForm = new System.Windows.Forms.SplitContainer();
            propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            tvwView = new System.Windows.Forms.TreeView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuUndo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            mnuCut = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuClear = new System.Windows.Forms.ToolStripMenuItem();
            mnuInsert = new System.Windows.Forms.ToolStripMenuItem();
            mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            mnuFlipH = new System.Windows.Forms.ToolStripMenuItem();
            mnuFlipV = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            mnuTogglePreview = new System.Windows.Forms.ToolStripMenuItem();
            mnuToggleGrid = new System.Windows.Forms.ToolStripMenuItem();
            mnuToggleSelectionMode = new System.Windows.Forms.ToolStripMenuItem();
            imageList1 = new System.Windows.Forms.ImageList(components);
            splitCanvas = new System.Windows.Forms.SplitContainer();
            picCelCorner = new System.Windows.Forms.PictureBox();
            picCel = new SelectablePictureBox();
            vsbCel = new System.Windows.Forms.VScrollBar();
            hsbCel = new System.Windows.Forms.HScrollBar();
            picPreviewCorner = new System.Windows.Forms.PictureBox();
            trkSpeed = new System.Windows.Forms.TrackBar();
            toolStrip3 = new System.Windows.Forms.ToolStrip();
            tspCycle = new System.Windows.Forms.ToolStripButton();
            tspMode = new System.Windows.Forms.ToolStripComboBox();
            toolStrip2 = new System.Windows.Forms.ToolStrip();
            tspZoomIn = new System.Windows.Forms.ToolStripButton();
            tspZoomOut = new System.Windows.Forms.ToolStripButton();
            tspAlignH = new System.Windows.Forms.ToolStripDropDownButton();
            tspHLeft = new System.Windows.Forms.ToolStripMenuItem();
            tspHCenter = new System.Windows.Forms.ToolStripMenuItem();
            tspHRight = new System.Windows.Forms.ToolStripMenuItem();
            tspAlignV = new System.Windows.Forms.ToolStripDropDownButton();
            tspVTop = new System.Windows.Forms.ToolStripMenuItem();
            tspVMiddle = new System.Windows.Forms.ToolStripMenuItem();
            tspVBottom = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            tspTransparency = new System.Windows.Forms.ToolStripButton();
            vsbPreview = new System.Windows.Forms.VScrollBar();
            hsbPreview = new System.Windows.Forms.HScrollBar();
            pnlPreview = new System.Windows.Forms.Panel();
            picPreview = new SelectablePictureBox();
            tmrMotion = new System.Windows.Forms.Timer(components);
            tmrSelect = new System.Windows.Forms.Timer(components);
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPalette).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitForm).BeginInit();
            splitForm.Panel1.SuspendLayout();
            splitForm.Panel2.SuspendLayout();
            splitForm.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitCanvas).BeginInit();
            splitCanvas.Panel1.SuspendLayout();
            splitCanvas.Panel2.SuspendLayout();
            splitCanvas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCelCorner).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCel).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPreviewCorner).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkSpeed).BeginInit();
            toolStrip3.SuspendLayout();
            toolStrip2.SuspendLayout();
            pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(704, 24);
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
            mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            mnuEdit.MergeIndex = 2;
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new System.Drawing.Size(39, 22);
            mnuEdit.Text = "&Edit";
            mnuEdit.DropDownClosed += mnuEdit_DropDownClosed;
            mnuEdit.DropDownOpening += mnuEdit_DropDownOpening;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbTool, toolStripSeparator1, tsbZoomIn, tsbZoomOut, toolStripSeparator2, tsbUndo, toolStripSeparator3, tsbCut, tsbCopy, tsbPaste, tsbDelete, tsbInsert, toolStripSeparator4, tsbFlipH, tsbFlipV });
            toolStrip1.Location = new System.Drawing.Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(704, 31);
            toolStrip1.Stretch = true;
            toolStrip1.TabIndex = 10;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbTool
            // 
            tsbTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbTool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tstEdit, tstSelect, tstPencil, tstLine, tstRectangle, tstRectangleSolid, tstFill, tstErase });
            tsbTool.Image = (System.Drawing.Image)resources.GetObject("tsbTool.Image");
            tsbTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbTool.Name = "tsbTool";
            tsbTool.Size = new System.Drawing.Size(37, 28);
            tsbTool.Text = "Draw Tool";
            tsbTool.DropDownOpening += tsbTool_DropDownOpening;
            tsbTool.DropDownItemClicked += tsbTool_DropDownItemClicked;
            // 
            // tstEdit
            // 
            tstEdit.AutoSize = false;
            tstEdit.Checked = true;
            tstEdit.CheckState = System.Windows.Forms.CheckState.Checked;
            tstEdit.Image = (System.Drawing.Image)resources.GetObject("tstEdit.Image");
            tstEdit.Name = "tstEdit";
            tstEdit.Size = new System.Drawing.Size(34, 30);
            tstEdit.Tag = ViewEditToolType.Edit;
            // 
            // tstSelect
            // 
            tstSelect.AutoSize = false;
            tstSelect.Image = (System.Drawing.Image)resources.GetObject("tstSelect.Image");
            tstSelect.Name = "tstSelect";
            tstSelect.Size = new System.Drawing.Size(34, 30);
            tstSelect.Tag = ViewEditToolType.Select;
            // 
            // tstPencil
            // 
            tstPencil.AutoSize = false;
            tstPencil.Image = (System.Drawing.Image)resources.GetObject("tstPencil.Image");
            tstPencil.Name = "tstPencil";
            tstPencil.Size = new System.Drawing.Size(34, 30);
            tstPencil.Tag = ViewEditToolType.Draw;
            // 
            // tstLine
            // 
            tstLine.AutoSize = false;
            tstLine.Image = (System.Drawing.Image)resources.GetObject("tstLine.Image");
            tstLine.Name = "tstLine";
            tstLine.Size = new System.Drawing.Size(34, 30);
            tstLine.Tag = ViewEditToolType.Line;
            // 
            // tstRectangle
            // 
            tstRectangle.AutoSize = false;
            tstRectangle.Image = (System.Drawing.Image)resources.GetObject("tstRectangle.Image");
            tstRectangle.Name = "tstRectangle";
            tstRectangle.Size = new System.Drawing.Size(34, 30);
            tstRectangle.Tag = ViewEditToolType.Rectangle;
            // 
            // tstRectangleSolid
            // 
            tstRectangleSolid.AutoSize = false;
            tstRectangleSolid.Image = (System.Drawing.Image)resources.GetObject("tstRectangleSolid.Image");
            tstRectangleSolid.Name = "tstRectangleSolid";
            tstRectangleSolid.Size = new System.Drawing.Size(34, 30);
            tstRectangleSolid.Tag = ViewEditToolType.BoxFill;
            // 
            // tstFill
            // 
            tstFill.AutoSize = false;
            tstFill.Image = (System.Drawing.Image)resources.GetObject("tstFill.Image");
            tstFill.Name = "tstFill";
            tstFill.Size = new System.Drawing.Size(34, 30);
            tstFill.Tag = ViewEditToolType.Paint;
            // 
            // tstErase
            // 
            tstErase.AutoSize = false;
            tstErase.Image = (System.Drawing.Image)resources.GetObject("tstErase.Image");
            tstErase.Name = "tstErase";
            tstErase.Size = new System.Drawing.Size(34, 30);
            tstErase.Tag = ViewEditToolType.Erase;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbZoomIn
            // 
            tsbZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbZoomIn.Image = (System.Drawing.Image)resources.GetObject("tsbZoomIn.Image");
            tsbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomIn.Name = "tsbZoomIn";
            tsbZoomIn.Size = new System.Drawing.Size(28, 28);
            tsbZoomIn.Text = "Zoom In";
            tsbZoomIn.Click += tsbZoomIn_Click;
            // 
            // tsbZoomOut
            // 
            tsbZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbZoomOut.Image = (System.Drawing.Image)resources.GetObject("tsbZoomOut.Image");
            tsbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomOut.Name = "tsbZoomOut";
            tsbZoomOut.Size = new System.Drawing.Size(28, 28);
            tsbZoomOut.Text = "Zoom Out";
            tsbZoomOut.Click += tsbZoomOut_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbUndo
            // 
            tsbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbUndo.Enabled = false;
            tsbUndo.Image = (System.Drawing.Image)resources.GetObject("tsbUndo.Image");
            tsbUndo.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            tsbUndo.Name = "tsbUndo";
            tsbUndo.Size = new System.Drawing.Size(28, 28);
            tsbUndo.Text = "Undo";
            tsbUndo.Click += mnuUndo_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbCut
            // 
            tsbCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbCut.Image = (System.Drawing.Image)resources.GetObject("tsbCut.Image");
            tsbCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCut.Name = "tsbCut";
            tsbCut.Size = new System.Drawing.Size(28, 28);
            tsbCut.Text = "Cut";
            tsbCut.Click += mnuCut_Click;
            // 
            // tsbCopy
            // 
            tsbCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbCopy.Image = (System.Drawing.Image)resources.GetObject("tsbCopy.Image");
            tsbCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCopy.Name = "tsbCopy";
            tsbCopy.Size = new System.Drawing.Size(28, 28);
            tsbCopy.Text = "Copy";
            tsbCopy.Click += mnuCopy_Click;
            // 
            // tsbPaste
            // 
            tsbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbPaste.Image = (System.Drawing.Image)resources.GetObject("tsbPaste.Image");
            tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbPaste.Name = "tsbPaste";
            tsbPaste.Size = new System.Drawing.Size(28, 28);
            tsbPaste.Text = "Paste";
            tsbPaste.Click += mnuPaste_Click;
            // 
            // tsbDelete
            // 
            tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbDelete.Image = (System.Drawing.Image)resources.GetObject("tsbDelete.Image");
            tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbDelete.Name = "tsbDelete";
            tsbDelete.Size = new System.Drawing.Size(28, 28);
            tsbDelete.Text = "Delete";
            tsbDelete.Click += mnuDelete_Click;
            // 
            // tsbInsert
            // 
            tsbInsert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbInsert.Image = (System.Drawing.Image)resources.GetObject("tsbInsert.Image");
            tsbInsert.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbInsert.Name = "tsbInsert";
            tsbInsert.Size = new System.Drawing.Size(28, 28);
            tsbInsert.Click += mnuInsert_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbFlipH
            // 
            tsbFlipH.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbFlipH.Image = (System.Drawing.Image)resources.GetObject("tsbFlipH.Image");
            tsbFlipH.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            tsbFlipH.Name = "tsbFlipH";
            tsbFlipH.Size = new System.Drawing.Size(28, 28);
            tsbFlipH.Text = "Flip Horizontal";
            tsbFlipH.Click += mnuFlipH_Click;
            // 
            // tsbFlipV
            // 
            tsbFlipV.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbFlipV.Image = (System.Drawing.Image)resources.GetObject("tsbFlipV.Image");
            tsbFlipV.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            tsbFlipV.Name = "tsbFlipV";
            tsbFlipV.Size = new System.Drawing.Size(28, 28);
            tsbFlipV.Text = "Flip Vertical";
            tsbFlipV.Click += mnuFlipV_Click;
            // 
            // picPalette
            // 
            picPalette.Dock = System.Windows.Forms.DockStyle.Bottom;
            picPalette.Location = new System.Drawing.Point(0, 312);
            picPalette.Name = "picPalette";
            picPalette.Size = new System.Drawing.Size(704, 32);
            picPalette.TabIndex = 12;
            picPalette.TabStop = false;
            picPalette.Paint += picPalette_Paint;
            picPalette.MouseDown += picPalette_MouseDown;
            // 
            // splitForm
            // 
            splitForm.Dock = System.Windows.Forms.DockStyle.Fill;
            splitForm.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitForm.Location = new System.Drawing.Point(0, 55);
            splitForm.Name = "splitForm";
            // 
            // splitForm.Panel1
            // 
            splitForm.Panel1.Controls.Add(propertyGrid1);
            splitForm.Panel1.Controls.Add(tvwView);
            splitForm.Panel1MinSize = 60;
            // 
            // splitForm.Panel2
            // 
            splitForm.Panel2.Controls.Add(splitCanvas);
            splitForm.Size = new System.Drawing.Size(704, 257);
            splitForm.SplitterDistance = 133;
            splitForm.TabIndex = 13;
            splitForm.TabStop = false;
            splitForm.MouseUp += splitForm_MouseUp;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = System.Windows.Forms.DockStyle.Bottom;
            propertyGrid1.HelpVisible = false;
            propertyGrid1.Location = new System.Drawing.Point(0, 190);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            propertyGrid1.Size = new System.Drawing.Size(133, 67);
            propertyGrid1.TabIndex = 1;
            propertyGrid1.ToolbarVisible = false;
            // 
            // tvwView
            // 
            tvwView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tvwView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            tvwView.ContextMenuStrip = contextMenuStrip1;
            tvwView.HideSelection = false;
            tvwView.ImageIndex = 3;
            tvwView.ImageList = imageList1;
            tvwView.Location = new System.Drawing.Point(0, 0);
            tvwView.Name = "tvwView";
            treeNode1.ImageIndex = 2;
            treeNode1.Name = "loop0cel0";
            treeNode1.SelectedImageIndex = 2;
            treeNode1.Text = "Cel 0";
            treeNode2.ImageIndex = 3;
            treeNode2.Name = "loop0end";
            treeNode2.SelectedImageIndex = 3;
            treeNode2.Text = "End";
            treeNode3.ImageIndex = 1;
            treeNode3.Name = "loop0";
            treeNode3.SelectedImageIndex = 1;
            treeNode3.Text = "Loop 0";
            treeNode4.ImageIndex = 3;
            treeNode4.Name = "viewend";
            treeNode4.SelectedImageIndex = 3;
            treeNode4.Text = "End";
            treeNode5.ImageIndex = 0;
            treeNode5.Name = "viewnode";
            treeNode5.SelectedImageIndex = 0;
            treeNode5.Text = "View";
            tvwView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] { treeNode5 });
            tvwView.SelectedImageIndex = 0;
            tvwView.ShowRootLines = false;
            tvwView.Size = new System.Drawing.Size(133, 184);
            tvwView.TabIndex = 0;
            tvwView.NodeMouseClick += tvwView_NodeMouseClick;
            tvwView.KeyPress += tvwView_KeyPress;
            tvwView.KeyUp += tvwView_KeyUp;
            tvwView.MouseDown += tvwView_MouseDown;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuUndo, toolStripSeparator6, mnuCut, mnuCopy, mnuPaste, mnuDelete, mnuClear, mnuInsert, mnuSelectAll, toolStripSeparator7, mnuFlipH, mnuFlipV, toolStripSeparator8, mnuTogglePreview, mnuToggleGrid, mnuToggleSelectionMode });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(225, 308);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // mnuUndo
            // 
            mnuUndo.Name = "mnuUndo";
            mnuUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuUndo.Size = new System.Drawing.Size(224, 22);
            mnuUndo.Text = "Undo";
            mnuUndo.Click += mnuUndo_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuCut
            // 
            mnuCut.Name = "mnuCut";
            mnuCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuCut.Size = new System.Drawing.Size(224, 22);
            mnuCut.Text = "Cut";
            mnuCut.Click += mnuCut_Click;
            // 
            // mnuCopy
            // 
            mnuCopy.Name = "mnuCopy";
            mnuCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuCopy.Size = new System.Drawing.Size(224, 22);
            mnuCopy.Text = "Copy";
            mnuCopy.Click += mnuCopy_Click;
            // 
            // mnuPaste
            // 
            mnuPaste.Name = "mnuPaste";
            mnuPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuPaste.Size = new System.Drawing.Size(224, 22);
            mnuPaste.Text = "Paste";
            mnuPaste.Click += mnuPaste_Click;
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuDelete.Size = new System.Drawing.Size(224, 22);
            mnuDelete.Text = "Delete";
            mnuDelete.Click += mnuDelete_Click;
            // 
            // mnuClear
            // 
            mnuClear.Name = "mnuClear";
            mnuClear.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete;
            mnuClear.Size = new System.Drawing.Size(224, 22);
            mnuClear.Text = "Clear";
            mnuClear.Click += mnuClear_Click;
            // 
            // mnuInsert
            // 
            mnuInsert.Name = "mnuInsert";
            mnuInsert.ShortcutKeyDisplayString = "Shift+Ins";
            mnuInsert.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Insert;
            mnuInsert.Size = new System.Drawing.Size(224, 22);
            mnuInsert.Text = "Insert";
            mnuInsert.Click += mnuInsert_Click;
            // 
            // mnuSelectAll
            // 
            mnuSelectAll.Name = "mnuSelectAll";
            mnuSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuSelectAll.Size = new System.Drawing.Size(224, 22);
            mnuSelectAll.Text = "Select All";
            mnuSelectAll.Click += mnuSelectAll_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuFlipH
            // 
            mnuFlipH.Name = "mnuFlipH";
            mnuFlipH.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H;
            mnuFlipH.Size = new System.Drawing.Size(224, 22);
            mnuFlipH.Text = "Flip Horizontal";
            mnuFlipH.Click += mnuFlipH_Click;
            // 
            // mnuFlipV
            // 
            mnuFlipV.Name = "mnuFlipV";
            mnuFlipV.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.V;
            mnuFlipV.Size = new System.Drawing.Size(224, 22);
            mnuFlipV.Text = "Flip Vertical";
            mnuFlipV.Click += mnuFlipV_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuTogglePreview
            // 
            mnuTogglePreview.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuTogglePreview.Name = "mnuTogglePreview";
            mnuTogglePreview.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.P;
            mnuTogglePreview.Size = new System.Drawing.Size(224, 22);
            mnuTogglePreview.Text = "Show Preview";
            mnuTogglePreview.Click += mnuTogglePreview_Click;
            // 
            // mnuToggleGrid
            // 
            mnuToggleGrid.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuToggleGrid.Name = "mnuToggleGrid";
            mnuToggleGrid.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.G;
            mnuToggleGrid.Size = new System.Drawing.Size(224, 22);
            mnuToggleGrid.Text = "Show Grid";
            mnuToggleGrid.Click += mnuToggleGrid_Click;
            // 
            // mnuToggleSelectionMode
            // 
            mnuToggleSelectionMode.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuToggleSelectionMode.Name = "mnuToggleSelectionMode";
            mnuToggleSelectionMode.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.T;
            mnuToggleSelectionMode.Size = new System.Drawing.Size(224, 22);
            mnuToggleSelectionMode.Text = "Transparent Selection";
            mnuToggleSelectionMode.Click += mnuToggleSelectionMode_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "view");
            imageList1.Images.SetKeyName(1, "loop");
            imageList1.Images.SetKeyName(2, "cel");
            imageList1.Images.SetKeyName(3, "end");
            // 
            // splitCanvas
            // 
            splitCanvas.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            splitCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            splitCanvas.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitCanvas.Location = new System.Drawing.Point(0, 0);
            splitCanvas.Name = "splitCanvas";
            // 
            // splitCanvas.Panel1
            // 
            splitCanvas.Panel1.BackColor = System.Drawing.SystemColors.Control;
            splitCanvas.Panel1.ContextMenuStrip = contextMenuStrip1;
            splitCanvas.Panel1.Controls.Add(picCelCorner);
            splitCanvas.Panel1.Controls.Add(picCel);
            splitCanvas.Panel1.Controls.Add(vsbCel);
            splitCanvas.Panel1.Controls.Add(hsbCel);
            splitCanvas.Panel1.Resize += splitCanvas_Panel1_Resize;
            splitCanvas.Panel1MinSize = 200;
            // 
            // splitCanvas.Panel2
            // 
            splitCanvas.Panel2.Controls.Add(picPreviewCorner);
            splitCanvas.Panel2.Controls.Add(trkSpeed);
            splitCanvas.Panel2.Controls.Add(toolStrip3);
            splitCanvas.Panel2.Controls.Add(toolStrip2);
            splitCanvas.Panel2.Controls.Add(vsbPreview);
            splitCanvas.Panel2.Controls.Add(hsbPreview);
            splitCanvas.Panel2.Controls.Add(pnlPreview);
            splitCanvas.Panel2.DoubleClick += preview_DoubleClick;
            splitCanvas.Panel2.Resize += splitCanvas_Panel2_Resize;
            splitCanvas.Panel2MinSize = 200;
            splitCanvas.Size = new System.Drawing.Size(567, 257);
            splitCanvas.SplitterDistance = 363;
            splitCanvas.TabIndex = 0;
            splitCanvas.TabStop = false;
            splitCanvas.MouseUp += splitCanvas_MouseUp;
            // 
            // picCelCorner
            // 
            picCelCorner.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            picCelCorner.Location = new System.Drawing.Point(342, 236);
            picCelCorner.Name = "picCelCorner";
            picCelCorner.Size = new System.Drawing.Size(16, 16);
            picCelCorner.TabIndex = 3;
            picCelCorner.TabStop = false;
            picCelCorner.Visible = false;
            // 
            // picCel
            // 
            picCel.ContextMenuStrip = contextMenuStrip1;
            picCel.Location = new System.Drawing.Point(3, 3);
            picCel.Name = "picCel";
            picCel.ShowFocusRectangle = false;
            picCel.Size = new System.Drawing.Size(155, 158);
            picCel.TabIndex = 0;
            picCel.Paint += picCel_Paint;
            picCel.MouseDoubleClick += picCel_MouseDoubleClick;
            picCel.MouseDown += picCel_MouseDown;
            picCel.MouseLeave += picCel_MouseLeave;
            picCel.MouseMove += picCel_MouseMove;
            picCel.MouseUp += picCel_MouseUp;
            // 
            // vsbCel
            // 
            vsbCel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            vsbCel.Location = new System.Drawing.Point(342, 0);
            vsbCel.Minimum = -3;
            vsbCel.Name = "vsbCel";
            vsbCel.Size = new System.Drawing.Size(16, 261);
            vsbCel.TabIndex = 2;
            vsbCel.Visible = false;
            vsbCel.Scroll += vsbCel_Scroll;
            // 
            // hsbCel
            // 
            hsbCel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            hsbCel.Location = new System.Drawing.Point(0, 248);
            hsbCel.Minimum = -3;
            hsbCel.Name = "hsbCel";
            hsbCel.Size = new System.Drawing.Size(342, 16);
            hsbCel.TabIndex = 1;
            hsbCel.Visible = false;
            hsbCel.Scroll += hsbCel_Scroll;
            // 
            // picPreviewCorner
            // 
            picPreviewCorner.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            picPreviewCorner.Location = new System.Drawing.Point(180, 212);
            picPreviewCorner.Name = "picPreviewCorner";
            picPreviewCorner.Size = new System.Drawing.Size(16, 16);
            picPreviewCorner.TabIndex = 6;
            picPreviewCorner.TabStop = false;
            picPreviewCorner.Visible = false;
            // 
            // trkSpeed
            // 
            trkSpeed.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            trkSpeed.AutoSize = false;
            trkSpeed.BackColor = System.Drawing.SystemColors.Control;
            trkSpeed.Location = new System.Drawing.Point(134, 205);
            trkSpeed.Maximum = 15;
            trkSpeed.Minimum = 1;
            trkSpeed.Name = "trkSpeed";
            trkSpeed.Size = new System.Drawing.Size(56, 20);
            trkSpeed.TabIndex = 2;
            trkSpeed.TabStop = false;
            trkSpeed.TickStyle = System.Windows.Forms.TickStyle.None;
            trkSpeed.Value = 5;
            trkSpeed.ValueChanged += trkSpeed_ValueChanged;
            // 
            // toolStrip3
            // 
            toolStrip3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            toolStrip3.AutoSize = false;
            toolStrip3.BackColor = System.Drawing.SystemColors.Control;
            toolStrip3.Dock = System.Windows.Forms.DockStyle.None;
            toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tspCycle, tspMode });
            toolStrip3.Location = new System.Drawing.Point(0, 228);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Size = new System.Drawing.Size(196, 25);
            toolStrip3.TabIndex = 1;
            toolStrip3.Text = "toolStrip3";
            // 
            // tspCycle
            // 
            tspCycle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tspCycle.Image = (System.Drawing.Image)resources.GetObject("tspCycle.Image");
            tspCycle.ImageTransparentColor = System.Drawing.Color.Magenta;
            tspCycle.Name = "tspCycle";
            tspCycle.Size = new System.Drawing.Size(23, 22);
            tspCycle.Click += tspCycle_Click;
            // 
            // tspMode
            // 
            tspMode.AutoSize = false;
            tspMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tspMode.DropDownWidth = 80;
            tspMode.Items.AddRange(new object[] { "normal", "reverse", "end.of.loop", "reverse.loop" });
            tspMode.Name = "tspMode";
            tspMode.Size = new System.Drawing.Size(90, 23);
            tspMode.SelectedIndexChanged += tspMode_SelectedIndexChanged;
            // 
            // toolStrip2
            // 
            toolStrip2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            toolStrip2.AutoSize = false;
            toolStrip2.BackColor = System.Drawing.SystemColors.Control;
            toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tspZoomIn, tspZoomOut, tspAlignH, tspAlignV, toolStripSeparator5, tspTransparency });
            toolStrip2.Location = new System.Drawing.Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new System.Drawing.Size(196, 25);
            toolStrip2.TabIndex = 0;
            toolStrip2.Text = "toolStrip2";
            // 
            // tspZoomIn
            // 
            tspZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tspZoomIn.Image = (System.Drawing.Image)resources.GetObject("tspZoomIn.Image");
            tspZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            tspZoomIn.Name = "tspZoomIn";
            tspZoomIn.Size = new System.Drawing.Size(23, 22);
            tspZoomIn.Click += tspZoomIn_Click;
            // 
            // tspZoomOut
            // 
            tspZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tspZoomOut.Image = (System.Drawing.Image)resources.GetObject("tspZoomOut.Image");
            tspZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tspZoomOut.Name = "tspZoomOut";
            tspZoomOut.Size = new System.Drawing.Size(23, 22);
            tspZoomOut.Click += tspZoomOut_Click;
            // 
            // tspAlignH
            // 
            tspAlignH.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tspAlignH.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tspHLeft, tspHCenter, tspHRight });
            tspAlignH.Image = (System.Drawing.Image)resources.GetObject("tspAlignH.Image");
            tspAlignH.ImageTransparentColor = System.Drawing.Color.Magenta;
            tspAlignH.Name = "tspAlignH";
            tspAlignH.Size = new System.Drawing.Size(29, 22);
            tspAlignH.DropDownOpening += tspAlignH_DropDownOpening;
            tspAlignH.DropDownItemClicked += tspAlignH_DropDownItemClicked;
            // 
            // tspHLeft
            // 
            tspHLeft.AutoSize = false;
            tspHLeft.Checked = true;
            tspHLeft.CheckState = System.Windows.Forms.CheckState.Checked;
            tspHLeft.Image = (System.Drawing.Image)resources.GetObject("tspHLeft.Image");
            tspHLeft.Name = "tspHLeft";
            tspHLeft.Size = new System.Drawing.Size(24, 22);
            tspHLeft.Tag = 0;
            // 
            // tspHCenter
            // 
            tspHCenter.AutoSize = false;
            tspHCenter.Image = (System.Drawing.Image)resources.GetObject("tspHCenter.Image");
            tspHCenter.Name = "tspHCenter";
            tspHCenter.Size = new System.Drawing.Size(24, 22);
            tspHCenter.Tag = 1;
            // 
            // tspHRight
            // 
            tspHRight.AutoSize = false;
            tspHRight.Image = (System.Drawing.Image)resources.GetObject("tspHRight.Image");
            tspHRight.Name = "tspHRight";
            tspHRight.Size = new System.Drawing.Size(24, 22);
            tspHRight.Tag = 2;
            // 
            // tspAlignV
            // 
            tspAlignV.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tspAlignV.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tspVTop, tspVMiddle, tspVBottom });
            tspAlignV.Image = (System.Drawing.Image)resources.GetObject("tspAlignV.Image");
            tspAlignV.ImageTransparentColor = System.Drawing.Color.Magenta;
            tspAlignV.Name = "tspAlignV";
            tspAlignV.Size = new System.Drawing.Size(29, 22);
            tspAlignV.DropDownOpening += tspAlignV_DropDownOpening;
            tspAlignV.DropDownItemClicked += tspAlignV_DropDownItemClicked;
            // 
            // tspVTop
            // 
            tspVTop.AutoSize = false;
            tspVTop.Image = (System.Drawing.Image)resources.GetObject("tspVTop.Image");
            tspVTop.Name = "tspVTop";
            tspVTop.Size = new System.Drawing.Size(24, 22);
            tspVTop.Tag = 0;
            // 
            // tspVMiddle
            // 
            tspVMiddle.AutoSize = false;
            tspVMiddle.Image = (System.Drawing.Image)resources.GetObject("tspVMiddle.Image");
            tspVMiddle.Name = "tspVMiddle";
            tspVMiddle.Size = new System.Drawing.Size(24, 22);
            tspVMiddle.Tag = 1;
            // 
            // tspVBottom
            // 
            tspVBottom.AutoSize = false;
            tspVBottom.Checked = true;
            tspVBottom.CheckState = System.Windows.Forms.CheckState.Checked;
            tspVBottom.Image = (System.Drawing.Image)resources.GetObject("tspVBottom.Image");
            tspVBottom.Name = "tspVBottom";
            tspVBottom.Size = new System.Drawing.Size(24, 22);
            tspVBottom.Tag = 2;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // tspTransparency
            // 
            tspTransparency.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tspTransparency.Image = (System.Drawing.Image)resources.GetObject("tspTransparency.Image");
            tspTransparency.ImageTransparentColor = System.Drawing.Color.Magenta;
            tspTransparency.Name = "tspTransparency";
            tspTransparency.Size = new System.Drawing.Size(29, 22);
            tspTransparency.Text = "ON";
            tspTransparency.ToolTipText = "Transparency";
            tspTransparency.Click += tspTransparency_Click;
            // 
            // vsbPreview
            // 
            vsbPreview.Anchor = System.Windows.Forms.AnchorStyles.Top;
            vsbPreview.Location = new System.Drawing.Point(180, 25);
            vsbPreview.Minimum = -3;
            vsbPreview.Name = "vsbPreview";
            vsbPreview.Size = new System.Drawing.Size(16, 211);
            vsbPreview.TabIndex = 5;
            vsbPreview.Visible = false;
            vsbPreview.Scroll += vsbPreview_Scroll;
            // 
            // hsbPreview
            // 
            hsbPreview.Anchor = System.Windows.Forms.AnchorStyles.Left;
            hsbPreview.Location = new System.Drawing.Point(0, 224);
            hsbPreview.Minimum = -3;
            hsbPreview.Name = "hsbPreview";
            hsbPreview.Size = new System.Drawing.Size(180, 16);
            hsbPreview.TabIndex = 4;
            hsbPreview.Visible = false;
            hsbPreview.Scroll += hsbPreview_Scroll;
            // 
            // pnlPreview
            // 
            pnlPreview.Controls.Add(picPreview);
            pnlPreview.Location = new System.Drawing.Point(3, 28);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new System.Drawing.Size(117, 160);
            pnlPreview.TabIndex = 3;
            pnlPreview.DoubleClick += preview_DoubleClick;
            pnlPreview.MouseDown += preview_MouseDown;
            pnlPreview.MouseMove += preview_MouseMove;
            pnlPreview.MouseUp += preview_MouseUp;
            // 
            // picPreview
            // 
            picPreview.Location = new System.Drawing.Point(0, 0);
            picPreview.Name = "picPreview";
            picPreview.ShowFocusRectangle = false;
            picPreview.Size = new System.Drawing.Size(56, 71);
            picPreview.TabIndex = 0;
            picPreview.DoubleClick += preview_DoubleClick;
            picPreview.MouseDown += preview_MouseDown;
            picPreview.MouseMove += preview_MouseMove;
            picPreview.MouseUp += preview_MouseUp;
            // 
            // tmrMotion
            // 
            tmrMotion.Tick += tmrMotion_Tick;
            // 
            // tmrSelect
            // 
            tmrSelect.Tick += tmrSelect_Tick;
            // 
            // frmViewEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(704, 344);
            Controls.Add(splitForm);
            Controls.Add(picPalette);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "frmViewEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmViewEdit";
            FormClosing += frmViewEdit_FormClosing;
            FormClosed += frmViewEdit_FormClosed;
            KeyDown += frmViewEdit_KeyDown;
            Resize += frmViewEdit_Resize;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPalette).EndInit();
            splitForm.Panel1.ResumeLayout(false);
            splitForm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitForm).EndInit();
            splitForm.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            splitCanvas.Panel1.ResumeLayout(false);
            splitCanvas.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitCanvas).EndInit();
            splitCanvas.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCelCorner).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCel).EndInit();
            ((System.ComponentModel.ISupportInitialize)picPreviewCorner).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkSpeed).EndInit();
            toolStrip3.ResumeLayout(false);
            toolStrip3.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
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
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton tsbTool;
        private System.Windows.Forms.ToolStripMenuItem tstEdit;
        private System.Windows.Forms.ToolStripMenuItem tstSelect;
        private System.Windows.Forms.ToolStripMenuItem tstPencil;
        private System.Windows.Forms.ToolStripMenuItem tstLine;
        private System.Windows.Forms.ToolStripMenuItem tstRectangle;
        private System.Windows.Forms.ToolStripMenuItem tstRectangleSolid;
        private System.Windows.Forms.ToolStripMenuItem tstFill;
        private System.Windows.Forms.ToolStripMenuItem tstErase;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbZoomIn;
        private System.Windows.Forms.ToolStripButton tsbZoomOut;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbCut;
        private System.Windows.Forms.ToolStripButton tsbCopy;
        private System.Windows.Forms.ToolStripButton tsbPaste;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tsbFlipH;
        private System.Windows.Forms.ToolStripButton tsbFlipV;
        private System.Windows.Forms.PictureBox picPalette;
        private System.Windows.Forms.HScrollBar hsbCel;
        private System.Windows.Forms.VScrollBar vsbCel;
        private System.Windows.Forms.VScrollBar vsbPreview;
        private System.Windows.Forms.HScrollBar hsbPreview;
        private System.Windows.Forms.SplitContainer splitForm;
        private System.Windows.Forms.SplitContainer splitCanvas;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton tspZoomIn;
        private System.Windows.Forms.ToolStripButton tspZoomOut;
        private System.Windows.Forms.ToolStripDropDownButton tspAlignH;
        private System.Windows.Forms.ToolStripDropDownButton tspAlignV;
        private System.Windows.Forms.ToolStripButton tspTransparency;
        private System.Windows.Forms.ToolStripMenuItem tspHLeft;
        private System.Windows.Forms.ToolStripMenuItem tspHCenter;
        private System.Windows.Forms.ToolStripMenuItem tspHRight;
        private System.Windows.Forms.ToolStripMenuItem tspVTop;
        private System.Windows.Forms.ToolStripMenuItem tspVMiddle;
        private System.Windows.Forms.ToolStripMenuItem tspVBottom;
        private System.Windows.Forms.ToolStripButton tspCycle;
        private System.Windows.Forms.ToolStripComboBox tspMode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.TrackBar trkSpeed;
        private System.Windows.Forms.Panel pnlPreview;
        private SelectablePictureBox picCel;
        private SelectablePictureBox picPreview;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuCut;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuClear;
        private System.Windows.Forms.ToolStripMenuItem mnuInsert;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem mnuFlipH;
        private System.Windows.Forms.ToolStripMenuItem mnuFlipV;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem mnuTogglePreview;
        private System.Windows.Forms.ToolStripMenuItem mnuToggleGrid;
        private System.Windows.Forms.ToolStripMenuItem mnuToggleSelectionMode;
        private System.Windows.Forms.Timer tmrMotion;
        internal System.Windows.Forms.TreeView tvwView;
        private System.Windows.Forms.PictureBox picCelCorner;
        private System.Windows.Forms.PictureBox picPreviewCorner;
        private System.Windows.Forms.ToolStripButton tsbInsert;
        private System.Windows.Forms.Timer tmrSelect;
    }
}