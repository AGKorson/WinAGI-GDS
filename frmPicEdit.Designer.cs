
using System.Windows.Forms;

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPicEdit));
            menuStrip1 = new MenuStrip();
            mnuResource = new ToolStripMenuItem();
            mnuROpenRes = new ToolStripMenuItem();
            mnuRSave = new ToolStripMenuItem();
            mnuRExport = new ToolStripMenuItem();
            mnuRInGame = new ToolStripMenuItem();
            mnuRRenumber = new ToolStripMenuItem();
            mnuRProperties = new ToolStripMenuItem();
            mnuRCompile = new ToolStripMenuItem();
            mnuRSavePicImage = new ToolStripMenuItem();
            mnuRExportGIF = new ToolStripMenuItem();
            mnuEdit = new ToolStripMenuItem();
            mnuCut = new ToolStripMenuItem();
            mnuCopy = new ToolStripMenuItem();
            mnuPaste = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            tsbMode = new ToolStripDropDownButton();
            tsbEditMode = new ToolStripMenuItem();
            tsbViewTest = new ToolStripMenuItem();
            tsbPrintTest = new ToolStripMenuItem();
            tsbTool = new ToolStripDropDownButton();
            tsbEditTool = new ToolStripMenuItem();
            tsbImageSelect = new ToolStripMenuItem();
            tsbLine = new ToolStripMenuItem();
            tsbShortLine = new ToolStripMenuItem();
            tsbStepLine = new ToolStripMenuItem();
            tsbRectangle = new ToolStripMenuItem();
            tsbTrapezoid = new ToolStripMenuItem();
            tsbEllipse = new ToolStripMenuItem();
            tsbFill = new ToolStripMenuItem();
            tsbPlot = new ToolStripMenuItem();
            tsbSep0 = new ToolStripSeparator();
            tsbFullDraw = new ToolStripButton();
            tsbBackground = new ToolStripButton();
            tsbZoomIn = new ToolStripButton();
            tsbZoomOut = new ToolStripButton();
            tsbSep1 = new ToolStripSeparator();
            tsbUndo = new ToolStripButton();
            tsbCut = new ToolStripButton();
            tsbCopy = new ToolStripButton();
            tsbPaste = new ToolStripButton();
            tsbDelete = new ToolStripButton();
            tsbFlipH = new ToolStripButton();
            tsbFlipV = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            tsbPlotStyle = new ToolStripDropDownButton();
            tsbCircleSolid = new ToolStripMenuItem();
            tsbSquareSolid = new ToolStripMenuItem();
            tsbCircleSplat = new ToolStripMenuItem();
            tsbSquareSplat = new ToolStripMenuItem();
            tsbPlotSize = new ToolStripDropDownButton();
            tsbSize0 = new ToolStripMenuItem();
            tsbSize1 = new ToolStripMenuItem();
            tsbSize2 = new ToolStripMenuItem();
            tsbSize3 = new ToolStripMenuItem();
            tsbSize4 = new ToolStripMenuItem();
            tsbSize5 = new ToolStripMenuItem();
            tsbSize6 = new ToolStripMenuItem();
            tsbSize7 = new ToolStripMenuItem();
            picPalette = new PictureBox();
            splitForm = new SplitContainer();
            splitLists = new SplitContainer();
            label2 = new Label();
            lstCommands = new ListView();
            CmdColumnHeader = new ColumnHeader();
            cmEdit = new ContextMenuStrip(components);
            mnuUndo = new ToolStripMenuItem();
            mnuESep0 = new ToolStripSeparator();
            mnuPastePen = new ToolStripMenuItem();
            mnuDelete = new ToolStripMenuItem();
            mnuClearPicture = new ToolStripMenuItem();
            mnuSelectAll = new ToolStripMenuItem();
            mnuESep1 = new ToolStripSeparator();
            mnuInsertCoord = new ToolStripMenuItem();
            mnuSplitCommand = new ToolStripMenuItem();
            mnuJoinCommands = new ToolStripMenuItem();
            mnuFlipV = new ToolStripMenuItem();
            mnuFlipH = new ToolStripMenuItem();
            mnuESep2 = new ToolStripSeparator();
            mnuEditMode = new ToolStripMenuItem();
            mnuViewTestMode = new ToolStripMenuItem();
            mnuTextTestMode = new ToolStripMenuItem();
            mnuESep3 = new ToolStripSeparator();
            mnuSetTestView = new ToolStripMenuItem();
            mnuTestViewOptions = new ToolStripMenuItem();
            mnuTestTextOptions = new ToolStripMenuItem();
            mnuTestPrintCommand = new ToolStripMenuItem();
            mnuTextScreenSize = new ToolStripMenuItem();
            mnuESep4 = new ToolStripSeparator();
            mnuToggleScreen = new ToolStripMenuItem();
            mnuToggleBands = new ToolStripMenuItem();
            mnuEditPriBase = new ToolStripMenuItem();
            mnuToggleTextMarks = new ToolStripMenuItem();
            mnuESep5 = new ToolStripSeparator();
            mnuToggleBackground = new ToolStripMenuItem();
            mnuEditBackground = new ToolStripMenuItem();
            mnuRemoveBackground = new ToolStripMenuItem();
            lblCoords = new Label();
            lstCoords = new ListView();
            CoordColumnHeader = new ColumnHeader();
            splitImages = new SplitContainer();
            picCornerVis = new PictureBox();
            vsbVisual = new VScrollBar();
            hsbVisual = new HScrollBar();
            picVisual = new PictureBox();
            picCornerPri = new PictureBox();
            vsbPriority = new VScrollBar();
            hsbPriority = new HScrollBar();
            picPriority = new PictureBox();
            toolStripMenuItem9 = new ToolStripMenuItem();
            toolStripMenuItem10 = new ToolStripMenuItem();
            toolStripMenuItem11 = new ToolStripMenuItem();
            toolStripMenuItem12 = new ToolStripMenuItem();
            tmrTest = new Timer(components);
            tmrSelect = new Timer(components);
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPalette).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitForm).BeginInit();
            splitForm.Panel1.SuspendLayout();
            splitForm.Panel2.SuspendLayout();
            splitForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitLists).BeginInit();
            splitLists.Panel1.SuspendLayout();
            splitLists.Panel2.SuspendLayout();
            splitLists.SuspendLayout();
            cmEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitImages).BeginInit();
            splitImages.Panel1.SuspendLayout();
            splitImages.Panel2.SuspendLayout();
            splitImages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCornerVis).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picVisual).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCornerPri).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPriority).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 4;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRExport, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportGIF });
            mnuResource.MergeAction = MergeAction.MatchOnly;
            mnuResource.MergeIndex = 1;
            mnuResource.Name = "mnuResource";
            mnuResource.Size = new System.Drawing.Size(67, 22);
            mnuResource.Text = "Resource";
            mnuResource.Visible = false;
            // 
            // mnuROpenRes
            // 
            mnuROpenRes.MergeAction = MergeAction.Remove;
            mnuROpenRes.MergeIndex = 4;
            mnuROpenRes.Name = "mnuROpenRes";
            mnuROpenRes.Size = new System.Drawing.Size(321, 22);
            mnuROpenRes.Text = "open res";
            // 
            // mnuRSave
            // 
            mnuRSave.MergeAction = MergeAction.Replace;
            mnuRSave.MergeIndex = 4;
            mnuRSave.Name = "mnuRSave";
            mnuRSave.ShortcutKeys = Keys.Control | Keys.S;
            mnuRSave.Size = new System.Drawing.Size(321, 22);
            mnuRSave.Text = "Save Picture";
            mnuRSave.Click += mnuRSave_Click;
            // 
            // mnuRExport
            // 
            mnuRExport.MergeAction = MergeAction.Replace;
            mnuRExport.MergeIndex = 5;
            mnuRExport.Name = "mnuRExport";
            mnuRExport.ShortcutKeys = Keys.Control | Keys.E;
            mnuRExport.Size = new System.Drawing.Size(321, 22);
            mnuRExport.Text = "export res";
            mnuRExport.Click += mnuRExport_Click;
            // 
            // mnuRInGame
            // 
            mnuRInGame.MergeAction = MergeAction.Replace;
            mnuRInGame.MergeIndex = 7;
            mnuRInGame.Name = "mnuRInGame";
            mnuRInGame.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            mnuRInGame.Size = new System.Drawing.Size(321, 22);
            mnuRInGame.Text = "ToggleInGame";
            mnuRInGame.Click += mnuRInGame_Click;
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = MergeAction.Replace;
            mnuRRenumber.MergeIndex = 8;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.ShortcutKeys = Keys.Alt | Keys.N;
            mnuRRenumber.Size = new System.Drawing.Size(321, 22);
            mnuRRenumber.Text = "Renumber Picture";
            mnuRRenumber.Click += mnuRRenumber_Click;
            // 
            // mnuRProperties
            // 
            mnuRProperties.MergeAction = MergeAction.Replace;
            mnuRProperties.MergeIndex = 9;
            mnuRProperties.Name = "mnuRProperties";
            mnuRProperties.ShortcutKeys = Keys.Control | Keys.D;
            mnuRProperties.Size = new System.Drawing.Size(321, 22);
            mnuRProperties.Text = "ID/Description ...";
            mnuRProperties.Click += mnuRProperties_Click;
            // 
            // mnuRCompile
            // 
            mnuRCompile.MergeAction = MergeAction.Remove;
            mnuRCompile.MergeIndex = 11;
            mnuRCompile.Name = "mnuRCompile";
            mnuRCompile.Size = new System.Drawing.Size(321, 22);
            mnuRCompile.Text = "compilelogic";
            // 
            // mnuRSavePicImage
            // 
            mnuRSavePicImage.MergeAction = MergeAction.Replace;
            mnuRSavePicImage.MergeIndex = 11;
            mnuRSavePicImage.Name = "mnuRSavePicImage";
            mnuRSavePicImage.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            mnuRSavePicImage.Size = new System.Drawing.Size(321, 22);
            mnuRSavePicImage.Text = "Save Picture Image As ...";
            mnuRSavePicImage.Click += mnuRSavePicImage_Click;
            // 
            // mnuRExportGIF
            // 
            mnuRExportGIF.MergeAction = MergeAction.Replace;
            mnuRExportGIF.MergeIndex = 12;
            mnuRExportGIF.Name = "mnuRExportGIF";
            mnuRExportGIF.ShortcutKeys = Keys.Control | Keys.Shift | Keys.G;
            mnuRExportGIF.Size = new System.Drawing.Size(321, 22);
            mnuRExportGIF.Text = "Export Picture As Animated GIF...";
            mnuRExportGIF.Click += mnuRExportGIF_Click;
            // 
            // mnuEdit
            // 
            mnuEdit.MergeAction = MergeAction.Insert;
            mnuEdit.MergeIndex = 2;
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new System.Drawing.Size(39, 22);
            mnuEdit.Text = "Edit";
            mnuEdit.DropDownClosed += mnuEdit_DropDownClosed;
            mnuEdit.DropDownOpening += mnuEdit_DropDownOpening;
            // 
            // mnuCut
            // 
            mnuCut.Name = "mnuCut";
            mnuCut.ShortcutKeys = Keys.Control | Keys.X;
            mnuCut.Size = new System.Drawing.Size(248, 22);
            mnuCut.Text = "Cut";
            mnuCut.Click += mnuCut_Click;
            // 
            // mnuCopy
            // 
            mnuCopy.Name = "mnuCopy";
            mnuCopy.ShortcutKeys = Keys.Control | Keys.C;
            mnuCopy.Size = new System.Drawing.Size(248, 22);
            mnuCopy.Text = "Copy";
            mnuCopy.Click += mnuCopy_Click;
            // 
            // mnuPaste
            // 
            mnuPaste.Name = "mnuPaste";
            mnuPaste.ShortcutKeys = Keys.Control | Keys.V;
            mnuPaste.Size = new System.Drawing.Size(248, 22);
            mnuPaste.Text = "Paste";
            mnuPaste.Click += mnuPaste_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbMode, tsbTool, tsbSep0, tsbFullDraw, tsbBackground, tsbZoomIn, tsbZoomOut, tsbSep1, tsbUndo, tsbCut, tsbCopy, tsbPaste, tsbDelete, tsbFlipH, tsbFlipV, toolStripSeparator3, tsbPlotStyle, tsbPlotSize });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(800, 31);
            toolStrip1.TabIndex = 10;
            // 
            // tsbMode
            // 
            tsbMode.AutoSize = false;
            tsbMode.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbMode.DropDownItems.AddRange(new ToolStripItem[] { tsbEditMode, tsbViewTest, tsbPrintTest });
            tsbMode.Image = (System.Drawing.Image)resources.GetObject("tsbMode.Image");
            tsbMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbMode.Name = "tsbMode";
            tsbMode.Size = new System.Drawing.Size(37, 28);
            tsbMode.Text = "Mode";
            tsbMode.DropDownOpening += tsbMode_DropDownOpening;
            // 
            // tsbEditMode
            // 
            tsbEditMode.AutoSize = false;
            tsbEditMode.Checked = true;
            tsbEditMode.CheckState = CheckState.Checked;
            tsbEditMode.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbEditMode.Image = (System.Drawing.Image)resources.GetObject("tsbEditMode.Image");
            tsbEditMode.Name = "tsbEditMode";
            tsbEditMode.Size = new System.Drawing.Size(30, 30);
            tsbEditMode.Text = "Edit";
            tsbEditMode.Click += mnuEditMode_Click;
            // 
            // tsbViewTest
            // 
            tsbViewTest.AutoSize = false;
            tsbViewTest.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbViewTest.Image = (System.Drawing.Image)resources.GetObject("tsbViewTest.Image");
            tsbViewTest.Name = "tsbViewTest";
            tsbViewTest.Size = new System.Drawing.Size(30, 30);
            tsbViewTest.Text = "View Test";
            tsbViewTest.Click += mnuViewTestMode_Click;
            // 
            // tsbPrintTest
            // 
            tsbPrintTest.AutoSize = false;
            tsbPrintTest.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbPrintTest.Image = (System.Drawing.Image)resources.GetObject("tsbPrintTest.Image");
            tsbPrintTest.Name = "tsbPrintTest";
            tsbPrintTest.Size = new System.Drawing.Size(30, 30);
            tsbPrintTest.Text = "Message Test";
            tsbPrintTest.Click += mnuTextTestMode_Click;
            // 
            // tsbTool
            // 
            tsbTool.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbTool.DropDownItems.AddRange(new ToolStripItem[] { tsbEditTool, tsbImageSelect, tsbLine, tsbShortLine, tsbStepLine, tsbRectangle, tsbTrapezoid, tsbEllipse, tsbFill, tsbPlot });
            tsbTool.Image = (System.Drawing.Image)resources.GetObject("tsbTool.Image");
            tsbTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbTool.Name = "tsbTool";
            tsbTool.Size = new System.Drawing.Size(37, 28);
            tsbTool.Text = "Draw Tool";
            tsbTool.DropDownOpening += tsbTool_DropDownOpening;
            // 
            // tsbEditTool
            // 
            tsbEditTool.AutoSize = false;
            tsbEditTool.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbEditTool.Image = (System.Drawing.Image)resources.GetObject("tsbEditTool.Image");
            tsbEditTool.Name = "tsbEditTool";
            tsbEditTool.Size = new System.Drawing.Size(34, 30);
            tsbEditTool.Text = "toolStripMenuItem12";
            tsbEditTool.ToolTipText = "Edit";
            tsbEditTool.Click += tsbEditTool_Click;
            // 
            // tsbImageSelect
            // 
            tsbImageSelect.AutoSize = false;
            tsbImageSelect.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbImageSelect.Image = (System.Drawing.Image)resources.GetObject("tsbImageSelect.Image");
            tsbImageSelect.Name = "tsbImageSelect";
            tsbImageSelect.Size = new System.Drawing.Size(34, 30);
            tsbImageSelect.Text = "toolStripMenuItem13";
            tsbImageSelect.ToolTipText = "Image Select";
            tsbImageSelect.Click += tsbImageSelect_Click;
            // 
            // tsbLine
            // 
            tsbLine.AutoSize = false;
            tsbLine.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbLine.Image = (System.Drawing.Image)resources.GetObject("tsbLine.Image");
            tsbLine.Name = "tsbLine";
            tsbLine.Size = new System.Drawing.Size(34, 30);
            tsbLine.Text = "toolStripMenuItem14";
            tsbLine.ToolTipText = "Line";
            tsbLine.Click += tsbLine_Click;
            // 
            // tsbShortLine
            // 
            tsbShortLine.AutoSize = false;
            tsbShortLine.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbShortLine.Image = (System.Drawing.Image)resources.GetObject("tsbShortLine.Image");
            tsbShortLine.Name = "tsbShortLine";
            tsbShortLine.Size = new System.Drawing.Size(34, 30);
            tsbShortLine.Text = "toolStripMenuItem15";
            tsbShortLine.ToolTipText = "Short Line";
            tsbShortLine.Click += tsbShortLine_Click;
            // 
            // tsbStepLine
            // 
            tsbStepLine.AutoSize = false;
            tsbStepLine.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbStepLine.Image = (System.Drawing.Image)resources.GetObject("tsbStepLine.Image");
            tsbStepLine.Name = "tsbStepLine";
            tsbStepLine.Size = new System.Drawing.Size(34, 30);
            tsbStepLine.Text = "toolStripMenuItem16";
            tsbStepLine.ToolTipText = "Step Line";
            tsbStepLine.Click += tsbStepLine_Click;
            // 
            // tsbRectangle
            // 
            tsbRectangle.AutoSize = false;
            tsbRectangle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbRectangle.Image = (System.Drawing.Image)resources.GetObject("tsbRectangle.Image");
            tsbRectangle.Name = "tsbRectangle";
            tsbRectangle.Size = new System.Drawing.Size(34, 30);
            tsbRectangle.Text = "toolStripMenuItem17";
            tsbRectangle.ToolTipText = "Rectangle";
            tsbRectangle.Click += tsbRectangle_Click;
            // 
            // tsbTrapezoid
            // 
            tsbTrapezoid.AutoSize = false;
            tsbTrapezoid.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbTrapezoid.Image = (System.Drawing.Image)resources.GetObject("tsbTrapezoid.Image");
            tsbTrapezoid.Name = "tsbTrapezoid";
            tsbTrapezoid.Size = new System.Drawing.Size(34, 30);
            tsbTrapezoid.Text = "toolStripMenuItem18";
            tsbTrapezoid.ToolTipText = "Trapezoid";
            tsbTrapezoid.Click += tsbTrapezoid_Click;
            // 
            // tsbEllipse
            // 
            tsbEllipse.AutoSize = false;
            tsbEllipse.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbEllipse.Image = (System.Drawing.Image)resources.GetObject("tsbEllipse.Image");
            tsbEllipse.Name = "tsbEllipse";
            tsbEllipse.Size = new System.Drawing.Size(34, 30);
            tsbEllipse.Text = "toolStripMenuItem19";
            tsbEllipse.ToolTipText = "Ellipse";
            tsbEllipse.Click += tsbEllipse_Click;
            // 
            // tsbFill
            // 
            tsbFill.AutoSize = false;
            tsbFill.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbFill.Image = (System.Drawing.Image)resources.GetObject("tsbFill.Image");
            tsbFill.Name = "tsbFill";
            tsbFill.Size = new System.Drawing.Size(34, 30);
            tsbFill.Text = "toolStripMenuItem20";
            tsbFill.ToolTipText = "Fill";
            tsbFill.Click += tsbFill_Click;
            // 
            // tsbPlot
            // 
            tsbPlot.AutoSize = false;
            tsbPlot.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbPlot.Image = (System.Drawing.Image)resources.GetObject("tsbPlot.Image");
            tsbPlot.Name = "tsbPlot";
            tsbPlot.Size = new System.Drawing.Size(34, 30);
            tsbPlot.Text = "toolStripMenuItem21";
            tsbPlot.ToolTipText = "Plot";
            tsbPlot.Click += tsbPlot_Click;
            // 
            // tsbSep0
            // 
            tsbSep0.Name = "tsbSep0";
            tsbSep0.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbFullDraw
            // 
            tsbFullDraw.CheckOnClick = true;
            tsbFullDraw.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbFullDraw.Image = (System.Drawing.Image)resources.GetObject("tsbFullDraw.Image");
            tsbFullDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbFullDraw.Name = "tsbFullDraw";
            tsbFullDraw.Size = new System.Drawing.Size(28, 28);
            tsbFullDraw.Text = "Full Draw: Off";
            tsbFullDraw.Click += tsbFullDraw_Click;
            // 
            // tsbBackground
            // 
            tsbBackground.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbBackground.Image = (System.Drawing.Image)resources.GetObject("tsbBackground.Image");
            tsbBackground.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbBackground.Name = "tsbBackground";
            tsbBackground.Size = new System.Drawing.Size(28, 28);
            tsbBackground.Text = "Background";
            tsbBackground.Click += mnuToggleBackground_Click;
            // 
            // tsbZoomIn
            // 
            tsbZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbZoomIn.Image = (System.Drawing.Image)resources.GetObject("tsbZoomIn.Image");
            tsbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomIn.Name = "tsbZoomIn";
            tsbZoomIn.Size = new System.Drawing.Size(28, 28);
            tsbZoomIn.Text = "Zoom In";
            tsbZoomIn.Click += tsbZoomIn_Click;
            // 
            // tsbZoomOut
            // 
            tsbZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbZoomOut.Image = (System.Drawing.Image)resources.GetObject("tsbZoomOut.Image");
            tsbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbZoomOut.Name = "tsbZoomOut";
            tsbZoomOut.Size = new System.Drawing.Size(28, 28);
            tsbZoomOut.Text = "Zoom Out";
            tsbZoomOut.Click += tsbZoomOut_Click;
            // 
            // tsbSep1
            // 
            tsbSep1.Name = "tsbSep1";
            tsbSep1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbUndo
            // 
            tsbUndo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbUndo.Image = (System.Drawing.Image)resources.GetObject("tsbUndo.Image");
            tsbUndo.ImageTransparentColor = System.Drawing.Color.Silver;
            tsbUndo.Name = "tsbUndo";
            tsbUndo.Size = new System.Drawing.Size(28, 28);
            tsbUndo.Text = "Undo";
            tsbUndo.Click += mnuUndo_Click;
            // 
            // tsbCut
            // 
            tsbCut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbCut.Image = (System.Drawing.Image)resources.GetObject("tsbCut.Image");
            tsbCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCut.Name = "tsbCut";
            tsbCut.Size = new System.Drawing.Size(28, 28);
            tsbCut.Text = "Cut";
            tsbCut.Click += mnuCut_Click;
            // 
            // tsbCopy
            // 
            tsbCopy.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbCopy.Image = (System.Drawing.Image)resources.GetObject("tsbCopy.Image");
            tsbCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCopy.Name = "tsbCopy";
            tsbCopy.Size = new System.Drawing.Size(28, 28);
            tsbCopy.Text = "Copy";
            tsbCopy.Click += mnuCopy_Click;
            // 
            // tsbPaste
            // 
            tsbPaste.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbPaste.Image = (System.Drawing.Image)resources.GetObject("tsbPaste.Image");
            tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbPaste.Name = "tsbPaste";
            tsbPaste.Size = new System.Drawing.Size(28, 28);
            tsbPaste.Text = "Paste";
            tsbPaste.Click += mnuPaste_Click;
            // 
            // tsbDelete
            // 
            tsbDelete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbDelete.Image = (System.Drawing.Image)resources.GetObject("tsbDelete.Image");
            tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbDelete.Name = "tsbDelete";
            tsbDelete.Size = new System.Drawing.Size(28, 28);
            tsbDelete.Text = "Delete";
            tsbDelete.Click += mnuDelete_Click;
            // 
            // tsbFlipH
            // 
            tsbFlipH.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbFlipH.Image = (System.Drawing.Image)resources.GetObject("tsbFlipH.Image");
            tsbFlipH.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            tsbFlipH.Name = "tsbFlipH";
            tsbFlipH.Size = new System.Drawing.Size(28, 28);
            tsbFlipH.Text = "Flip Horizontal";
            tsbFlipH.Click += mnuFlipH_Click;
            // 
            // tsbFlipV
            // 
            tsbFlipV.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbFlipV.Image = (System.Drawing.Image)resources.GetObject("tsbFlipV.Image");
            tsbFlipV.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            tsbFlipV.Name = "tsbFlipV";
            tsbFlipV.Size = new System.Drawing.Size(28, 28);
            tsbFlipV.Text = "Flip Vertical";
            tsbFlipV.Click += mnuFlipV_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbPlotStyle
            // 
            tsbPlotStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbPlotStyle.DropDownItems.AddRange(new ToolStripItem[] { tsbCircleSolid, tsbSquareSolid, tsbCircleSplat, tsbSquareSplat });
            tsbPlotStyle.Image = (System.Drawing.Image)resources.GetObject("tsbPlotStyle.Image");
            tsbPlotStyle.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbPlotStyle.Name = "tsbPlotStyle";
            tsbPlotStyle.Size = new System.Drawing.Size(37, 28);
            tsbPlotStyle.Text = "Plot Style";
            tsbPlotStyle.DropDownOpening += tsbPlotStyle_DropDownOpening;
            // 
            // tsbCircleSolid
            // 
            tsbCircleSolid.AutoSize = false;
            tsbCircleSolid.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbCircleSolid.Image = (System.Drawing.Image)resources.GetObject("tsbCircleSolid.Image");
            tsbCircleSolid.Name = "tsbCircleSolid";
            tsbCircleSolid.Size = new System.Drawing.Size(34, 30);
            tsbCircleSolid.Text = "Circle Fill";
            tsbCircleSolid.Click += tsbPlotStyle_Click;
            // 
            // tsbSquareSolid
            // 
            tsbSquareSolid.AutoSize = false;
            tsbSquareSolid.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSquareSolid.Image = (System.Drawing.Image)resources.GetObject("tsbSquareSolid.Image");
            tsbSquareSolid.Name = "tsbSquareSolid";
            tsbSquareSolid.Size = new System.Drawing.Size(34, 30);
            tsbSquareSolid.Text = "Square Fill";
            tsbSquareSolid.Click += tsbPlotStyle_Click;
            // 
            // tsbCircleSplat
            // 
            tsbCircleSplat.AutoSize = false;
            tsbCircleSplat.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbCircleSplat.Image = (System.Drawing.Image)resources.GetObject("tsbCircleSplat.Image");
            tsbCircleSplat.Name = "tsbCircleSplat";
            tsbCircleSplat.Size = new System.Drawing.Size(34, 30);
            tsbCircleSplat.Text = "Circle Splatter";
            tsbCircleSplat.Click += tsbPlotStyle_Click;
            // 
            // tsbSquareSplat
            // 
            tsbSquareSplat.AutoSize = false;
            tsbSquareSplat.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSquareSplat.Image = (System.Drawing.Image)resources.GetObject("tsbSquareSplat.Image");
            tsbSquareSplat.Name = "tsbSquareSplat";
            tsbSquareSplat.Size = new System.Drawing.Size(34, 30);
            tsbSquareSplat.Text = "Square Splatter";
            tsbSquareSplat.Click += tsbPlotStyle_Click;
            // 
            // tsbPlotSize
            // 
            tsbPlotSize.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbPlotSize.DropDownItems.AddRange(new ToolStripItem[] { tsbSize0, tsbSize1, tsbSize2, tsbSize3, tsbSize4, tsbSize5, tsbSize6, tsbSize7 });
            tsbPlotSize.Image = (System.Drawing.Image)resources.GetObject("tsbPlotSize.Image");
            tsbPlotSize.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbPlotSize.Name = "tsbPlotSize";
            tsbPlotSize.Size = new System.Drawing.Size(37, 28);
            tsbPlotSize.Text = "Plot Size";
            tsbPlotSize.DropDownOpening += tsbPlotSize_DropDownOpening;
            // 
            // tsbSize0
            // 
            tsbSize0.AutoSize = false;
            tsbSize0.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize0.Image = (System.Drawing.Image)resources.GetObject("tsbSize0.Image");
            tsbSize0.Name = "tsbSize0";
            tsbSize0.Size = new System.Drawing.Size(34, 30);
            tsbSize0.Tag = 0;
            tsbSize0.Click += tsbPlotSize_Click;
            // 
            // tsbSize1
            // 
            tsbSize1.AutoSize = false;
            tsbSize1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize1.Image = (System.Drawing.Image)resources.GetObject("tsbSize1.Image");
            tsbSize1.Name = "tsbSize1";
            tsbSize1.Size = new System.Drawing.Size(34, 30);
            tsbSize1.Tag = 1;
            tsbSize1.Click += tsbPlotSize_Click;
            // 
            // tsbSize2
            // 
            tsbSize2.AutoSize = false;
            tsbSize2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize2.Image = (System.Drawing.Image)resources.GetObject("tsbSize2.Image");
            tsbSize2.Name = "tsbSize2";
            tsbSize2.Size = new System.Drawing.Size(34, 30);
            tsbSize2.Tag = 2;
            tsbSize2.Click += tsbPlotSize_Click;
            // 
            // tsbSize3
            // 
            tsbSize3.AutoSize = false;
            tsbSize3.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize3.Image = (System.Drawing.Image)resources.GetObject("tsbSize3.Image");
            tsbSize3.Name = "tsbSize3";
            tsbSize3.Size = new System.Drawing.Size(34, 30);
            tsbSize3.Tag = 3;
            tsbSize3.Click += tsbPlotSize_Click;
            // 
            // tsbSize4
            // 
            tsbSize4.AutoSize = false;
            tsbSize4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize4.Image = (System.Drawing.Image)resources.GetObject("tsbSize4.Image");
            tsbSize4.Name = "tsbSize4";
            tsbSize4.Size = new System.Drawing.Size(34, 30);
            tsbSize4.Tag = 4;
            tsbSize4.Click += tsbPlotSize_Click;
            // 
            // tsbSize5
            // 
            tsbSize5.AutoSize = false;
            tsbSize5.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize5.Image = (System.Drawing.Image)resources.GetObject("tsbSize5.Image");
            tsbSize5.Name = "tsbSize5";
            tsbSize5.Size = new System.Drawing.Size(34, 30);
            tsbSize5.Tag = 5;
            tsbSize5.Click += tsbPlotSize_Click;
            // 
            // tsbSize6
            // 
            tsbSize6.AutoSize = false;
            tsbSize6.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize6.Image = (System.Drawing.Image)resources.GetObject("tsbSize6.Image");
            tsbSize6.Name = "tsbSize6";
            tsbSize6.Size = new System.Drawing.Size(34, 30);
            tsbSize6.Tag = 6;
            tsbSize6.Click += tsbPlotSize_Click;
            // 
            // tsbSize7
            // 
            tsbSize7.AutoSize = false;
            tsbSize7.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSize7.Image = (System.Drawing.Image)resources.GetObject("tsbSize7.Image");
            tsbSize7.Name = "tsbSize7";
            tsbSize7.Size = new System.Drawing.Size(34, 30);
            tsbSize7.Tag = 7;
            tsbSize7.Click += tsbPlotSize_Click;
            // 
            // picPalette
            // 
            picPalette.Dock = DockStyle.Bottom;
            picPalette.Location = new System.Drawing.Point(0, 440);
            picPalette.Name = "picPalette";
            picPalette.Size = new System.Drawing.Size(800, 32);
            picPalette.TabIndex = 11;
            picPalette.TabStop = false;
            picPalette.Paint += picPalette_Paint;
            picPalette.MouseDown += picPalette_MouseDown;
            picPalette.MouseEnter += picPalette_MouseEnter;
            // 
            // splitForm
            // 
            splitForm.Dock = DockStyle.Fill;
            splitForm.Location = new System.Drawing.Point(0, 31);
            splitForm.Name = "splitForm";
            // 
            // splitForm.Panel1
            // 
            splitForm.Panel1.Controls.Add(splitLists);
            // 
            // splitForm.Panel2
            // 
            splitForm.Panel2.Controls.Add(splitImages);
            splitForm.Size = new System.Drawing.Size(800, 409);
            splitForm.SplitterDistance = 132;
            splitForm.SplitterWidth = 5;
            splitForm.TabIndex = 12;
            splitForm.SplitterMoving += splitForm_SplitterMoving;
            splitForm.SplitterMoved += splitForm_SplitterMoved;
            splitForm.MouseUp += splitForm_MouseUp;
            // 
            // splitLists
            // 
            splitLists.Dock = DockStyle.Fill;
            splitLists.Location = new System.Drawing.Point(0, 0);
            splitLists.Name = "splitLists";
            splitLists.Orientation = Orientation.Horizontal;
            // 
            // splitLists.Panel1
            // 
            splitLists.Panel1.Controls.Add(label2);
            splitLists.Panel1.Controls.Add(lstCommands);
            // 
            // splitLists.Panel2
            // 
            splitLists.Panel2.Controls.Add(lblCoords);
            splitLists.Panel2.Controls.Add(lstCoords);
            splitLists.Size = new System.Drawing.Size(132, 409);
            splitLists.SplitterDistance = 279;
            splitLists.SplitterWidth = 5;
            splitLists.TabIndex = 0;
            splitLists.SplitterMoving += splitLists_SplitterMoving;
            splitLists.SplitterMoved += splitLists_SplitterMoved;
            splitLists.MouseUp += splitLists_MouseUp;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            label2.Location = new System.Drawing.Point(5, 5);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(85, 15);
            label2.TabIndex = 1;
            label2.Text = "Command List";
            // 
            // lstCommands
            // 
            lstCommands.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstCommands.AutoArrange = false;
            lstCommands.BorderStyle = BorderStyle.FixedSingle;
            lstCommands.Columns.AddRange(new ColumnHeader[] { CmdColumnHeader });
            lstCommands.ContextMenuStrip = cmEdit;
            lstCommands.FullRowSelect = true;
            lstCommands.HeaderStyle = ColumnHeaderStyle.None;
            lstCommands.Location = new System.Drawing.Point(0, 24);
            lstCommands.Name = "lstCommands";
            lstCommands.ShowGroups = false;
            lstCommands.ShowItemToolTips = true;
            lstCommands.Size = new System.Drawing.Size(132, 255);
            lstCommands.TabIndex = 0;
            lstCommands.UseCompatibleStateImageBehavior = false;
            lstCommands.View = View.Details;
            lstCommands.KeyPress += lstCommands_KeyPress;
            lstCommands.KeyUp += lstCommands_KeyUp;
            lstCommands.MouseClick += lstCommands_MouseClick;
            lstCommands.MouseDoubleClick += lstCommands_MouseDoubleClick;
            lstCommands.MouseUp += lstCommands_MouseUp;
            // 
            // CmdColumnHeader
            // 
            CmdColumnHeader.Width = 233;
            // 
            // cmEdit
            // 
            cmEdit.Items.AddRange(new ToolStripItem[] { mnuUndo, mnuESep0, mnuCut, mnuCopy, mnuPaste, mnuPastePen, mnuDelete, mnuClearPicture, mnuSelectAll, mnuESep1, mnuInsertCoord, mnuSplitCommand, mnuJoinCommands, mnuFlipV, mnuFlipH, mnuESep2, mnuEditMode, mnuViewTestMode, mnuTextTestMode, mnuESep3, mnuSetTestView, mnuTestViewOptions, mnuTestTextOptions, mnuTestPrintCommand, mnuTextScreenSize, mnuESep4, mnuToggleScreen, mnuToggleBands, mnuEditPriBase, mnuToggleTextMarks, mnuESep5, mnuToggleBackground, mnuEditBackground, mnuRemoveBackground });
            cmEdit.Name = "cmEdit";
            cmEdit.Size = new System.Drawing.Size(249, 656);
            cmEdit.Opening += cmEdit_Opening;
            // 
            // mnuUndo
            // 
            mnuUndo.Name = "mnuUndo";
            mnuUndo.ShortcutKeys = Keys.Control | Keys.Z;
            mnuUndo.Size = new System.Drawing.Size(248, 22);
            mnuUndo.Text = "Undo";
            mnuUndo.Click += mnuUndo_Click;
            // 
            // mnuESep0
            // 
            mnuESep0.Name = "mnuESep0";
            mnuESep0.Size = new System.Drawing.Size(245, 6);
            // 
            // mnuPastePen
            // 
            mnuPastePen.Name = "mnuPastePen";
            mnuPastePen.ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
            mnuPastePen.Size = new System.Drawing.Size(248, 22);
            mnuPastePen.Text = "Pen Aware Paste";
            mnuPastePen.Click += mnuPastePen_Click;
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeys = Keys.Delete;
            mnuDelete.Size = new System.Drawing.Size(248, 22);
            mnuDelete.Text = "Delete";
            mnuDelete.Click += mnuDelete_Click;
            // 
            // mnuClearPicture
            // 
            mnuClearPicture.Name = "mnuClearPicture";
            mnuClearPicture.ShortcutKeys = Keys.Shift | Keys.Delete;
            mnuClearPicture.Size = new System.Drawing.Size(248, 22);
            mnuClearPicture.Text = "Clear Picture";
            mnuClearPicture.Click += mnuClearPicture_Click;
            // 
            // mnuSelectAll
            // 
            mnuSelectAll.Name = "mnuSelectAll";
            mnuSelectAll.ShortcutKeys = Keys.Control | Keys.A;
            mnuSelectAll.Size = new System.Drawing.Size(248, 22);
            mnuSelectAll.Text = "Select All";
            mnuSelectAll.Click += mnuSelectAll_Click;
            // 
            // mnuESep1
            // 
            mnuESep1.Name = "mnuESep1";
            mnuESep1.Size = new System.Drawing.Size(245, 6);
            // 
            // mnuInsertCoord
            // 
            mnuInsertCoord.Name = "mnuInsertCoord";
            mnuInsertCoord.ShortcutKeys = Keys.Shift | Keys.Insert;
            mnuInsertCoord.Size = new System.Drawing.Size(248, 22);
            mnuInsertCoord.Text = "Insert Coordinate";
            mnuInsertCoord.Click += mnuInsertCoord_Click;
            // 
            // mnuSplitCommand
            // 
            mnuSplitCommand.Name = "mnuSplitCommand";
            mnuSplitCommand.ShortcutKeys = Keys.Control | Keys.Shift | Keys.T;
            mnuSplitCommand.Size = new System.Drawing.Size(248, 22);
            mnuSplitCommand.Text = "Split Command";
            mnuSplitCommand.Click += mnuSplitCommand_Click;
            // 
            // mnuJoinCommands
            // 
            mnuJoinCommands.Name = "mnuJoinCommands";
            mnuJoinCommands.ShortcutKeys = Keys.Control | Keys.Shift | Keys.J;
            mnuJoinCommands.Size = new System.Drawing.Size(248, 22);
            mnuJoinCommands.Text = "Join Commands";
            mnuJoinCommands.Click += mnuJoinCommands_Click;
            // 
            // mnuFlipV
            // 
            mnuFlipV.Name = "mnuFlipV";
            mnuFlipV.Size = new System.Drawing.Size(248, 22);
            mnuFlipV.Text = "Flip Vertical";
            mnuFlipV.Click += mnuFlipV_Click;
            // 
            // mnuFlipH
            // 
            mnuFlipH.Name = "mnuFlipH";
            mnuFlipH.Size = new System.Drawing.Size(248, 22);
            mnuFlipH.Text = "Flip Horizontal";
            mnuFlipH.Click += mnuFlipH_Click;
            // 
            // mnuESep2
            // 
            mnuESep2.Name = "mnuESep2";
            mnuESep2.Size = new System.Drawing.Size(245, 6);
            // 
            // mnuEditMode
            // 
            mnuEditMode.Checked = true;
            mnuEditMode.CheckState = CheckState.Checked;
            mnuEditMode.ImageScaling = ToolStripItemImageScaling.None;
            mnuEditMode.Name = "mnuEditMode";
            mnuEditMode.Size = new System.Drawing.Size(248, 22);
            mnuEditMode.Text = "Edit Mode";
            mnuEditMode.Click += mnuEditMode_Click;
            // 
            // mnuViewTestMode
            // 
            mnuViewTestMode.ImageScaling = ToolStripItemImageScaling.None;
            mnuViewTestMode.Name = "mnuViewTestMode";
            mnuViewTestMode.Size = new System.Drawing.Size(248, 22);
            mnuViewTestMode.Text = "View Test Mode";
            mnuViewTestMode.Click += mnuViewTestMode_Click;
            // 
            // mnuTextTestMode
            // 
            mnuTextTestMode.ImageScaling = ToolStripItemImageScaling.None;
            mnuTextTestMode.Name = "mnuTextTestMode";
            mnuTextTestMode.Size = new System.Drawing.Size(248, 22);
            mnuTextTestMode.Text = "Text Test Mode";
            mnuTextTestMode.Click += mnuTextTestMode_Click;
            // 
            // mnuESep3
            // 
            mnuESep3.Name = "mnuESep3";
            mnuESep3.Size = new System.Drawing.Size(245, 6);
            // 
            // mnuSetTestView
            // 
            mnuSetTestView.Name = "mnuSetTestView";
            mnuSetTestView.ShortcutKeys = Keys.Alt | Keys.V;
            mnuSetTestView.Size = new System.Drawing.Size(248, 22);
            mnuSetTestView.Text = "Change Test View...";
            mnuSetTestView.Click += mnuSetTestView_Click;
            // 
            // mnuTestViewOptions
            // 
            mnuTestViewOptions.Name = "mnuTestViewOptions";
            mnuTestViewOptions.ShortcutKeys = Keys.Alt | Keys.O;
            mnuTestViewOptions.Size = new System.Drawing.Size(248, 22);
            mnuTestViewOptions.Text = "Test View Options...";
            mnuTestViewOptions.Click += mnuTestViewOptions_Click;
            // 
            // mnuTestTextOptions
            // 
            mnuTestTextOptions.Name = "mnuTestTextOptions";
            mnuTestTextOptions.ShortcutKeys = Keys.Alt | Keys.D;
            mnuTestTextOptions.Size = new System.Drawing.Size(248, 22);
            mnuTestTextOptions.Text = "Text Test Options...";
            mnuTestTextOptions.Click += mnuTestTextOptions_Click;
            // 
            // mnuTestPrintCommand
            // 
            mnuTestPrintCommand.Name = "mnuTestPrintCommand";
            mnuTestPrintCommand.Size = new System.Drawing.Size(248, 22);
            mnuTestPrintCommand.Text = "Test print() Command...";
            mnuTestPrintCommand.Click += mnuTestPrintCommand_Click;
            // 
            // mnuTextScreenSize
            // 
            mnuTextScreenSize.Name = "mnuTextScreenSize";
            mnuTextScreenSize.ShortcutKeys = Keys.Alt | Keys.W;
            mnuTextScreenSize.Size = new System.Drawing.Size(248, 22);
            mnuTextScreenSize.Text = "Text Screen Size: 40";
            mnuTextScreenSize.Click += mnuTextScreenSize_Click;
            // 
            // mnuESep4
            // 
            mnuESep4.Name = "mnuESep4";
            mnuESep4.Size = new System.Drawing.Size(245, 6);
            // 
            // mnuToggleScreen
            // 
            mnuToggleScreen.Name = "mnuToggleScreen";
            mnuToggleScreen.ShortcutKeys = Keys.Alt | Keys.S;
            mnuToggleScreen.Size = new System.Drawing.Size(248, 22);
            mnuToggleScreen.Text = "Show Priority Screen";
            mnuToggleScreen.Click += mnuToggleScreen_Click;
            // 
            // mnuToggleBands
            // 
            mnuToggleBands.Name = "mnuToggleBands";
            mnuToggleBands.ShortcutKeys = Keys.Alt | Keys.P;
            mnuToggleBands.Size = new System.Drawing.Size(248, 22);
            mnuToggleBands.Text = "Show Priority Bands";
            mnuToggleBands.Click += mnuToggleBands_Click;
            // 
            // mnuEditPriBase
            // 
            mnuEditPriBase.Name = "mnuEditPriBase";
            mnuEditPriBase.ShortcutKeys = Keys.Control | Keys.Alt | Keys.P;
            mnuEditPriBase.Size = new System.Drawing.Size(248, 22);
            mnuEditPriBase.Text = "Edit Priority Base";
            mnuEditPriBase.Click += mnuEditPriBase_Click;
            // 
            // mnuToggleTextMarks
            // 
            mnuToggleTextMarks.Name = "mnuToggleTextMarks";
            mnuToggleTextMarks.ShortcutKeys = Keys.Alt | Keys.T;
            mnuToggleTextMarks.Size = new System.Drawing.Size(248, 22);
            mnuToggleTextMarks.Text = "Show Text Marks";
            mnuToggleTextMarks.Click += mnuToggleTextMarks_Click;
            // 
            // mnuESep5
            // 
            mnuESep5.Name = "mnuESep5";
            mnuESep5.Size = new System.Drawing.Size(245, 6);
            // 
            // mnuToggleBackground
            // 
            mnuToggleBackground.Name = "mnuToggleBackground";
            mnuToggleBackground.ShortcutKeys = Keys.Alt | Keys.B;
            mnuToggleBackground.Size = new System.Drawing.Size(248, 22);
            mnuToggleBackground.Text = "Show Background";
            mnuToggleBackground.Click += mnuToggleBackground_Click;
            // 
            // mnuEditBackground
            // 
            mnuEditBackground.Name = "mnuEditBackground";
            mnuEditBackground.Size = new System.Drawing.Size(248, 22);
            mnuEditBackground.Text = "Background Settings...";
            mnuEditBackground.Click += mnuEditBackground_Click;
            // 
            // mnuRemoveBackground
            // 
            mnuRemoveBackground.Name = "mnuRemoveBackground";
            mnuRemoveBackground.ShortcutKeys = Keys.Control | Keys.Alt | Keys.P;
            mnuRemoveBackground.Size = new System.Drawing.Size(248, 22);
            mnuRemoveBackground.Text = "Remove Background";
            mnuRemoveBackground.Click += mnuRemoveBackground_Click;
            // 
            // lblCoords
            // 
            lblCoords.AutoSize = true;
            lblCoords.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblCoords.Location = new System.Drawing.Point(5, 5);
            lblCoords.Name = "lblCoords";
            lblCoords.Size = new System.Drawing.Size(73, 15);
            lblCoords.TabIndex = 1;
            lblCoords.Text = "Coordinates";
            // 
            // lstCoords
            // 
            lstCoords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstCoords.AutoArrange = false;
            lstCoords.BorderStyle = BorderStyle.FixedSingle;
            lstCoords.Columns.AddRange(new ColumnHeader[] { CoordColumnHeader });
            lstCoords.ContextMenuStrip = cmEdit;
            lstCoords.FullRowSelect = true;
            lstCoords.HeaderStyle = ColumnHeaderStyle.None;
            lstCoords.Location = new System.Drawing.Point(0, 23);
            lstCoords.MultiSelect = false;
            lstCoords.Name = "lstCoords";
            lstCoords.ShowGroups = false;
            lstCoords.ShowItemToolTips = true;
            lstCoords.Size = new System.Drawing.Size(132, 90);
            lstCoords.TabIndex = 0;
            lstCoords.UseCompatibleStateImageBehavior = false;
            lstCoords.View = View.Details;
            lstCoords.KeyPress += lstCoords_KeyPress;
            lstCoords.KeyUp += lstCoords_KeyUp;
            lstCoords.MouseClick += lstCoords_MouseClick;
            lstCoords.MouseDoubleClick += lstCoords_MouseDoubleClick;
            lstCoords.MouseEnter += lstCoords_MouseEnter;
            lstCoords.MouseUp += lstCoords_MouseUp;
            // 
            // CoordColumnHeader
            // 
            CoordColumnHeader.Width = 233;
            // 
            // splitImages
            // 
            splitImages.BorderStyle = BorderStyle.Fixed3D;
            splitImages.Dock = DockStyle.Fill;
            splitImages.Location = new System.Drawing.Point(0, 0);
            splitImages.Name = "splitImages";
            splitImages.Orientation = Orientation.Horizontal;
            // 
            // splitImages.Panel1
            // 
            splitImages.Panel1.Controls.Add(picCornerVis);
            splitImages.Panel1.Controls.Add(vsbVisual);
            splitImages.Panel1.Controls.Add(hsbVisual);
            splitImages.Panel1.Controls.Add(picVisual);
            splitImages.Panel1MinSize = 0;
            // 
            // splitImages.Panel2
            // 
            splitImages.Panel2.Controls.Add(picCornerPri);
            splitImages.Panel2.Controls.Add(vsbPriority);
            splitImages.Panel2.Controls.Add(hsbPriority);
            splitImages.Panel2.Controls.Add(picPriority);
            splitImages.Panel2MinSize = 0;
            splitImages.Size = new System.Drawing.Size(663, 409);
            splitImages.SplitterDistance = 165;
            splitImages.SplitterWidth = 5;
            splitImages.TabIndex = 2;
            splitImages.SplitterMoving += splitImages_SplitterMoving;
            splitImages.SplitterMoved += splitImages_SplitterMoved;
            splitImages.MouseUp += splitImages_MouseUp;
            splitImages.Resize += splitImages_Resize;
            // 
            // picCornerVis
            // 
            picCornerVis.Anchor = AnchorStyles.None;
            picCornerVis.Location = new System.Drawing.Point(643, 147);
            picCornerVis.Name = "picCornerVis";
            picCornerVis.Size = new System.Drawing.Size(16, 16);
            picCornerVis.TabIndex = 5;
            picCornerVis.TabStop = false;
            // 
            // vsbVisual
            // 
            vsbVisual.Anchor = AnchorStyles.Top;
            vsbVisual.Location = new System.Drawing.Point(643, 0);
            vsbVisual.Margin = new Padding(0, 0, 0, 16);
            vsbVisual.Minimum = -5;
            vsbVisual.Name = "vsbVisual";
            vsbVisual.Size = new System.Drawing.Size(16, 100);
            vsbVisual.TabIndex = 4;
            vsbVisual.Value = -5;
            vsbVisual.Scroll += vsbVisual_Scroll;
            // 
            // hsbVisual
            // 
            hsbVisual.Anchor = AnchorStyles.Left;
            hsbVisual.Location = new System.Drawing.Point(0, 147);
            hsbVisual.Minimum = -5;
            hsbVisual.Name = "hsbVisual";
            hsbVisual.Size = new System.Drawing.Size(600, 16);
            hsbVisual.TabIndex = 3;
            hsbVisual.Value = -5;
            hsbVisual.Scroll += hsbVisual_Scroll;
            // 
            // picVisual
            // 
            picVisual.ContextMenuStrip = cmEdit;
            picVisual.Location = new System.Drawing.Point(5, 5);
            picVisual.Name = "picVisual";
            picVisual.Size = new System.Drawing.Size(320, 168);
            picVisual.TabIndex = 2;
            picVisual.TabStop = false;
            picVisual.Paint += picVisual_Paint;
            picVisual.MouseDown += DrawSurface_MouseDown;
            picVisual.MouseLeave += DrawSurface_MouseLeave;
            picVisual.MouseMove += DrawSurface_MouseMove;
            picVisual.MouseUp += DrawSurface_MouseUp;
            // 
            // picCornerPri
            // 
            picCornerPri.Location = new System.Drawing.Point(624, 163);
            picCornerPri.Name = "picCornerPri";
            picCornerPri.Size = new System.Drawing.Size(16, 16);
            picCornerPri.TabIndex = 8;
            picCornerPri.TabStop = false;
            // 
            // vsbPriority
            // 
            vsbPriority.Anchor = AnchorStyles.Top;
            vsbPriority.Location = new System.Drawing.Point(590, 0);
            vsbPriority.Minimum = -5;
            vsbPriority.Name = "vsbPriority";
            vsbPriority.Size = new System.Drawing.Size(16, 111);
            vsbPriority.TabIndex = 7;
            vsbPriority.Value = -5;
            vsbPriority.Scroll += vsbPriority_Scroll;
            // 
            // hsbPriority
            // 
            hsbPriority.Anchor = AnchorStyles.Left;
            hsbPriority.Location = new System.Drawing.Point(0, 132);
            hsbPriority.Minimum = -5;
            hsbPriority.Name = "hsbPriority";
            hsbPriority.Size = new System.Drawing.Size(176, 16);
            hsbPriority.TabIndex = 6;
            hsbPriority.Value = -5;
            hsbPriority.Scroll += hsbPriority_Scroll;
            // 
            // picPriority
            // 
            picPriority.ContextMenuStrip = cmEdit;
            picPriority.Location = new System.Drawing.Point(5, 5);
            picPriority.Name = "picPriority";
            picPriority.Size = new System.Drawing.Size(320, 168);
            picPriority.TabIndex = 2;
            picPriority.TabStop = false;
            picPriority.Paint += picPriority_Paint;
            picPriority.MouseDown += DrawSurface_MouseDown;
            picPriority.MouseLeave += DrawSurface_MouseLeave;
            picPriority.MouseMove += DrawSurface_MouseMove;
            picPriority.MouseUp += DrawSurface_MouseUp;
            // 
            // toolStripMenuItem9
            // 
            toolStripMenuItem9.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripMenuItem9.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem9.Image");
            toolStripMenuItem9.Name = "toolStripMenuItem9";
            toolStripMenuItem9.Size = new System.Drawing.Size(186, 22);
            toolStripMenuItem9.Text = "toolStripMenuItem9";
            // 
            // toolStripMenuItem10
            // 
            toolStripMenuItem10.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripMenuItem10.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem10.Image");
            toolStripMenuItem10.Name = "toolStripMenuItem10";
            toolStripMenuItem10.Size = new System.Drawing.Size(186, 22);
            toolStripMenuItem10.Text = "toolStripMenuItem10";
            // 
            // toolStripMenuItem11
            // 
            toolStripMenuItem11.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripMenuItem11.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem11.Image");
            toolStripMenuItem11.Name = "toolStripMenuItem11";
            toolStripMenuItem11.Size = new System.Drawing.Size(186, 22);
            toolStripMenuItem11.Text = "toolStripMenuItem11";
            // 
            // toolStripMenuItem12
            // 
            toolStripMenuItem12.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripMenuItem12.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem12.Image");
            toolStripMenuItem12.Name = "toolStripMenuItem12";
            toolStripMenuItem12.Size = new System.Drawing.Size(186, 22);
            toolStripMenuItem12.Text = "toolStripMenuItem12";
            // 
            // tmrTest
            // 
            tmrTest.Tick += tmrTest_Tick;
            // 
            // tmrSelect
            // 
            tmrSelect.Tick += tmrSelect_Tick;
            // 
            // frmPicEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 472);
            Controls.Add(splitForm);
            Controls.Add(picPalette);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "frmPicEdit";
            StartPosition = FormStartPosition.WindowsDefaultBounds;
            Text = "frmPicEdit";
            FormClosing += frmPicEdit_FormClosing;
            FormClosed += frmPicEdit_FormClosed;
            KeyDown += frmPicEdit_KeyDown;
            Resize += frmPicEdit_Resize;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPalette).EndInit();
            splitForm.Panel1.ResumeLayout(false);
            splitForm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitForm).EndInit();
            splitForm.ResumeLayout(false);
            splitLists.Panel1.ResumeLayout(false);
            splitLists.Panel1.PerformLayout();
            splitLists.Panel2.ResumeLayout(false);
            splitLists.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitLists).EndInit();
            splitLists.ResumeLayout(false);
            cmEdit.ResumeLayout(false);
            splitImages.Panel1.ResumeLayout(false);
            splitImages.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitImages).EndInit();
            splitImages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCornerVis).EndInit();
            ((System.ComponentModel.ISupportInitialize)picVisual).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCornerPri).EndInit();
            ((System.ComponentModel.ISupportInitialize)picPriority).EndInit();
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
        private System.Windows.Forms.ToolStripMenuItem mnuRExportGIF;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuCut;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuPaste;
        private ToolStripContainer toolStripContainer1;
        private ToolStrip toolStrip1;
        private ToolStripDropDownButton tsbMode;
        private ToolStripDropDownButton tsbTool;
        private ToolStripButton tsbFullDraw;
        private ToolStripSeparator tsbSep0;
        private ToolStripButton tsbZoomIn;
        private ToolStripButton tsbZoomOut;
        private ToolStripSeparator tsbSep1;
        private ToolStripButton tsbUndo;
        private ToolStripButton tsbCut;
        private ToolStripButton tsbCopy;
        private ToolStripButton tsbPaste;
        private ToolStripButton tsbDelete;
        private ToolStripButton tsbFlipH;
        private ToolStripButton tsbFlipV;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripDropDownButton tsbPlotStyle;
        private ToolStripDropDownButton tsbPlotSize;
        private ToolStripButton tsbBackground;
        private ToolStripMenuItem tsbCircleSolid;
        private ToolStripMenuItem tsbSquareSolid;
        private ToolStripMenuItem tsbCircleSplat;
        private ToolStripMenuItem tsbSquareSplat;
        private ToolStripMenuItem tsbSize0;
        private ToolStripMenuItem tsbSize1;
        private ToolStripMenuItem tsbSize2;
        private ToolStripMenuItem tsbSize3;
        private ToolStripMenuItem tsbSize4;
        private ToolStripMenuItem tsbSize5;
        private ToolStripMenuItem tsbSize6;
        private ToolStripMenuItem tsbSize7;
        private ToolStripMenuItem tsbEditMode;
        private ToolStripMenuItem tsbViewTest;
        private ToolStripMenuItem tsbPrintTest;
        private ToolStripMenuItem tsbEditTool;
        private ToolStripMenuItem tsbImageSelect;
        private ToolStripMenuItem tsbLine;
        private ToolStripMenuItem tsbShortLine;
        private ToolStripMenuItem tsbStepLine;
        private ToolStripMenuItem tsbRectangle;
        private ToolStripMenuItem tsbTrapezoid;
        private ToolStripMenuItem tsbEllipse;
        private ToolStripMenuItem tsbFill;
        private ToolStripMenuItem tsbPlot;
        private PictureBox picPalette;
        private SplitContainer splitForm;
        private SplitContainer splitImages;
        private PictureBox picVisual;
        private PictureBox picPriority;
        private SplitContainer splitLists;
        private ListView lstCommands;
        private ListView lstCoords;
        private Label lblCoords;
        private Label label2;
        private ToolStripMenuItem toolStripMenuItem9;
        private ToolStripMenuItem toolStripMenuItem10;
        private ToolStripMenuItem toolStripMenuItem11;
        private ToolStripMenuItem toolStripMenuItem12;
        private ColumnHeader CmdColumnHeader;
        private ColumnHeader CoordColumnHeader;
        private VScrollBar vsbVisual;
        private HScrollBar hsbVisual;
        private PictureBox picCornerVis;
        private PictureBox picCornerPri;
        private VScrollBar vsbPriority;
        private HScrollBar hsbPriority;
        private ToolStripMenuItem mnuUndo;
        private ToolStripSeparator mnuESep0;
        private ToolStripSeparator mnuESep1;
        private ToolStripSeparator mnuESep2;
        private ToolStripSeparator mnuESep3;
        private ToolStripSeparator mnuESep4;
        private ToolStripSeparator mnuESep5;
        private ToolStripMenuItem mnuDelete;
        private ToolStripMenuItem mnuClearPicture;
        private ToolStripMenuItem mnuSelectAll;
        private ToolStripMenuItem mnuInsertCoord;
        private ToolStripMenuItem mnuSplitCommand;
        private ToolStripMenuItem mnuJoinCommands;
        private ToolStripMenuItem mnuEditMode;
        private ToolStripMenuItem mnuViewTestMode;
        private ToolStripMenuItem mnuTextTestMode;
        private ToolStripMenuItem mnuSetTestView;
        private ToolStripMenuItem mnuTestViewOptions;
        private ToolStripMenuItem mnuTestTextOptions;
        private ToolStripMenuItem mnuTextScreenSize;
        private ToolStripMenuItem mnuToggleScreen;
        private ToolStripMenuItem mnuToggleBands;
        private ToolStripMenuItem mnuEditPriBase;
        private ToolStripMenuItem mnuToggleTextMarks;
        private ToolStripMenuItem mnuToggleBackground;
        private ToolStripMenuItem mnuEditBackground;
        private ToolStripMenuItem mnuRemoveBackground;
        private ToolStripMenuItem mnuTestPrintCommand;
        private ContextMenuStrip cmEdit;
        private ToolStripMenuItem mnuFlipV;
        private ToolStripMenuItem mnuFlipH;
        private Timer tmrTest;
        private Timer tmrSelect;
        private ToolStripMenuItem mnuPastePen;
    }
}