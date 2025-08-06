
namespace WinAGI.Editor {
    partial class frmLayout {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLayout));
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuShowRoom = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditLogic = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditPicture = new System.Windows.Forms.ToolStripMenuItem();
            mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuInsert = new System.Windows.Forms.ToolStripMenuItem();
            mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditSep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuOrder = new System.Windows.Forms.ToolStripMenuItem();
            mnuOrderUp = new System.Windows.Forms.ToolStripMenuItem();
            mnuOrderDown = new System.Windows.Forms.ToolStripMenuItem();
            mnuOrderFront = new System.Windows.Forms.ToolStripMenuItem();
            mnuOrderBack = new System.Windows.Forms.ToolStripMenuItem();
            mnuTogglePicture = new System.Windows.Forms.ToolStripMenuItem();
            mnuToggleGrid = new System.Windows.Forms.ToolStripMenuItem();
            mnuProperties = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            mnuResource = new System.Windows.Forms.ToolStripMenuItem();
            mnuROpen = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSave = new System.Windows.Forms.ToolStripMenuItem();
            mnuRExport = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            mnuRRemove = new System.Windows.Forms.ToolStripMenuItem();
            mnuRRenumber = new System.Windows.Forms.ToolStripMenuItem();
            mnuRIDDesc = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            mnuRCompileLogic = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSavePic = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportGif = new System.Windows.Forms.ToolStripMenuItem();
            mnuRepair = new System.Windows.Forms.ToolStripMenuItem();
            mnuToggleAllPics = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnSelect = new System.Windows.Forms.ToolStripButton();
            btnEdge1 = new System.Windows.Forms.ToolStripButton();
            btnEdge2 = new System.Windows.Forms.ToolStripButton();
            btnEdgeOther = new System.Windows.Forms.ToolStripButton();
            btnAddRoom = new System.Windows.Forms.ToolStripButton();
            btnAddComment = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            btnDelete = new System.Windows.Forms.ToolStripButton();
            btnTransfer = new System.Windows.Forms.ToolStripButton();
            btnShowRoom = new System.Windows.Forms.ToolStripButton();
            btnHideRoom = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            btnFront = new System.Windows.Forms.ToolStripButton();
            btnBack = new System.Windows.Forms.ToolStripButton();
            btnZoomIn = new System.Windows.Forms.ToolStripButton();
            btnZoomOut = new System.Windows.Forms.ToolStripButton();
            picDraw = new System.Windows.Forms.PictureBox();
            tmrScroll = new System.Windows.Forms.Timer(components);
            vScrollBar1 = new VScrollBarMouseAware();
            hScrollBar1 = new HScrollBarMouseAware();
            txtComment = new System.Windows.Forms.TextBox();
            mnuEditSep2 = new System.Windows.Forms.ToolStripSeparator();
            contextMenuStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picDraw).BeginInit();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuShowRoom, mnuEditLogic, mnuEditPicture, mnuDelete, mnuInsert, mnuSelectAll, mnuEditSep1, mnuOrder, mnuTogglePicture, mnuProperties, mnuEditSep2, mnuToggleGrid });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(248, 258);
            contextMenuStrip1.Closed += contextMenuStrip1_Closed;
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // mnuShowRoom
            // 
            mnuShowRoom.Name = "mnuShowRoom";
            mnuShowRoom.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            mnuShowRoom.Size = new System.Drawing.Size(247, 22);
            mnuShowRoom.Text = "Show Room";
            mnuShowRoom.Click += mnuShowRoom_Click;
            // 
            // mnuEditLogic
            // 
            mnuEditLogic.Name = "mnuEditLogic";
            mnuEditLogic.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.L;
            mnuEditLogic.Size = new System.Drawing.Size(247, 22);
            mnuEditLogic.Text = "Edit Logic";
            mnuEditLogic.Click += mnuEditLogic_Click;
            // 
            // mnuEditPicture
            // 
            mnuEditPicture.Name = "mnuEditPicture";
            mnuEditPicture.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.P;
            mnuEditPicture.Size = new System.Drawing.Size(247, 22);
            mnuEditPicture.Text = "Edit Picture";
            mnuEditPicture.Click += mnuEditPicture_Click;
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuDelete.Size = new System.Drawing.Size(247, 22);
            mnuDelete.Text = "Delete";
            mnuDelete.Click += mnuDelete_Click;
            // 
            // mnuInsert
            // 
            mnuInsert.Name = "mnuInsert";
            mnuInsert.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            mnuInsert.Size = new System.Drawing.Size(247, 22);
            mnuInsert.Text = "Insert Transfer";
            mnuInsert.Click += mnuInsert_Click;
            // 
            // mnuSelectAll
            // 
            mnuSelectAll.Name = "mnuSelectAll";
            mnuSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuSelectAll.Size = new System.Drawing.Size(247, 22);
            mnuSelectAll.Text = "Select All";
            mnuSelectAll.Click += mnuSelectAll_Click;
            // 
            // mnuEditSep1
            // 
            mnuEditSep1.Name = "mnuEditSep1";
            mnuEditSep1.Size = new System.Drawing.Size(244, 6);
            // 
            // mnuOrder
            // 
            mnuOrder.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuOrderUp, mnuOrderDown, mnuOrderFront, mnuOrderBack });
            mnuOrder.Name = "mnuOrder";
            mnuOrder.Size = new System.Drawing.Size(247, 22);
            mnuOrder.Text = "Order";
            // 
            // mnuOrderUp
            // 
            mnuOrderUp.Name = "mnuOrderUp";
            mnuOrderUp.Size = new System.Drawing.Size(147, 22);
            mnuOrderUp.Text = "Move Up";
            mnuOrderUp.Click += mnuOrderUp_Click;
            // 
            // mnuOrderDown
            // 
            mnuOrderDown.Name = "mnuOrderDown";
            mnuOrderDown.Size = new System.Drawing.Size(147, 22);
            mnuOrderDown.Text = "Move Down";
            mnuOrderDown.Click += mnuOrderDown_Click;
            // 
            // mnuOrderFront
            // 
            mnuOrderFront.Name = "mnuOrderFront";
            mnuOrderFront.Size = new System.Drawing.Size(147, 22);
            mnuOrderFront.Text = "Bring to Front";
            mnuOrderFront.Click += mnuOrderFront_Click;
            // 
            // mnuOrderBack
            // 
            mnuOrderBack.Name = "mnuOrderBack";
            mnuOrderBack.Size = new System.Drawing.Size(147, 22);
            mnuOrderBack.Text = "Send to Back";
            mnuOrderBack.Click += mnuOrderBack_Click;
            // 
            // mnuTogglePicture
            // 
            mnuTogglePicture.Name = "mnuTogglePicture";
            mnuTogglePicture.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.P;
            mnuTogglePicture.Size = new System.Drawing.Size(247, 22);
            mnuTogglePicture.Text = "Show Room Picture";
            mnuTogglePicture.Click += mnuShowPicture_Click;
            // 
            // mnuToggleGrid
            // 
            mnuToggleGrid.Name = "mnuToggleGrid";
            mnuToggleGrid.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.G;
            mnuToggleGrid.Size = new System.Drawing.Size(247, 22);
            mnuToggleGrid.Text = "Hide Grid Lines";
            mnuToggleGrid.Click += mnuToggleGrid_Click;
            // 
            // mnuProperties
            // 
            mnuProperties.Name = "mnuProperties";
            mnuProperties.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.P;
            mnuProperties.Size = new System.Drawing.Size(247, 22);
            mnuProperties.Text = "Room Properties";
            mnuProperties.Click += mnuProperties_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(815, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpen, mnuRSave, mnuRExport, toolStripSeparator1, mnuRRemove, mnuRRenumber, mnuRIDDesc, toolStripSeparator2, mnuRCompileLogic, mnuRSavePic, mnuExportGif, mnuRepair, mnuToggleAllPics });
            mnuResource.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            mnuResource.MergeIndex = 1;
            mnuResource.Name = "mnuResource";
            mnuResource.Size = new System.Drawing.Size(67, 20);
            mnuResource.Text = "Resource";
            // 
            // mnuROpen
            // 
            mnuROpen.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuROpen.MergeIndex = 4;
            mnuROpen.Name = "mnuROpen";
            mnuROpen.Size = new System.Drawing.Size(206, 22);
            mnuROpen.Text = "open res";
            // 
            // mnuRSave
            // 
            mnuRSave.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSave.MergeIndex = 4;
            mnuRSave.Name = "mnuRSave";
            mnuRSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuRSave.Size = new System.Drawing.Size(206, 22);
            mnuRSave.Text = "Save Layout";
            mnuRSave.Click += mnuRSave_Click;
            // 
            // mnuRExport
            // 
            mnuRExport.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRExport.MergeIndex = 5;
            mnuRExport.Name = "mnuRExport";
            mnuRExport.Size = new System.Drawing.Size(206, 22);
            mnuRExport.Text = "export";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.MergeAction = System.Windows.Forms.MergeAction.Remove;
            toolStripSeparator1.MergeIndex = 5;
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(203, 6);
            // 
            // mnuRRemove
            // 
            mnuRRemove.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRemove.MergeIndex = 5;
            mnuRRemove.Name = "mnuRRemove";
            mnuRRemove.Size = new System.Drawing.Size(206, 22);
            mnuRRemove.Text = "remove";
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRenumber.MergeIndex = 5;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.Size = new System.Drawing.Size(206, 22);
            mnuRRenumber.Text = "renumber";
            // 
            // mnuRIDDesc
            // 
            mnuRIDDesc.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRIDDesc.MergeIndex = 5;
            mnuRIDDesc.Name = "mnuRIDDesc";
            mnuRIDDesc.Size = new System.Drawing.Size(206, 22);
            mnuRIDDesc.Text = "iddesc";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.MergeAction = System.Windows.Forms.MergeAction.Remove;
            toolStripSeparator2.MergeIndex = 5;
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(203, 6);
            // 
            // mnuRCompileLogic
            // 
            mnuRCompileLogic.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRCompileLogic.MergeIndex = 5;
            mnuRCompileLogic.Name = "mnuRCompileLogic";
            mnuRCompileLogic.Size = new System.Drawing.Size(206, 22);
            mnuRCompileLogic.Text = "compile";
            // 
            // mnuRSavePic
            // 
            mnuRSavePic.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRSavePic.MergeIndex = 5;
            mnuRSavePic.Name = "mnuRSavePic";
            mnuRSavePic.Size = new System.Drawing.Size(206, 22);
            mnuRSavePic.Text = "savepic";
            // 
            // mnuExportGif
            // 
            mnuExportGif.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuExportGif.MergeIndex = 5;
            mnuExportGif.Name = "mnuExportGif";
            mnuExportGif.Size = new System.Drawing.Size(206, 22);
            mnuExportGif.Text = "exportgif";
            // 
            // mnuRepair
            // 
            mnuRepair.Name = "mnuRepair";
            mnuRepair.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R;
            mnuRepair.Size = new System.Drawing.Size(206, 22);
            mnuRepair.Text = "Repair Layout";
            mnuRepair.Click += mnuRepair_Click;
            // 
            // mnuToggleAllPics
            // 
            mnuToggleAllPics.Name = "mnuToggleAllPics";
            mnuToggleAllPics.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H;
            mnuToggleAllPics.Size = new System.Drawing.Size(206, 22);
            mnuToggleAllPics.Text = "Hide All Pics";
            mnuToggleAllPics.Click += mnuToggleAllPics_Click;
            // 
            // mnuEdit
            // 
            mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            mnuEdit.MergeIndex = 2;
            mnuEdit.Name = "mnuEdit";
            mnuEdit.Size = new System.Drawing.Size(39, 20);
            mnuEdit.Text = "Edit";
            mnuEdit.DropDownClosed += mnuEdit_DropDownClosed;
            mnuEdit.DropDownOpening += mnuEdit_DropDownOpening;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnSelect, btnEdge1, btnEdge2, btnEdgeOther, btnAddRoom, btnAddComment, toolStripSeparator4, btnDelete, btnTransfer, btnShowRoom, btnHideRoom, toolStripSeparator5, btnFront, btnBack, btnZoomIn, btnZoomOut });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(815, 31);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnSelect
            // 
            btnSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnSelect.Image = (System.Drawing.Image)resources.GetObject("btnSelect.Image");
            btnSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new System.Drawing.Size(28, 28);
            btnSelect.Text = "Select";
            // 
            // btnEdge1
            // 
            btnEdge1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnEdge1.Image = (System.Drawing.Image)resources.GetObject("btnEdge1.Image");
            btnEdge1.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEdge1.Name = "btnEdge1";
            btnEdge1.Size = new System.Drawing.Size(28, 28);
            btnEdge1.Text = "One Way Exit";
            // 
            // btnEdge2
            // 
            btnEdge2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnEdge2.Image = (System.Drawing.Image)resources.GetObject("btnEdge2.Image");
            btnEdge2.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnEdge2.Name = "btnEdge2";
            btnEdge2.Size = new System.Drawing.Size(28, 28);
            btnEdge2.Text = "Two Way Exit";
            // 
            // btnEdgeOther
            // 
            btnEdgeOther.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnEdgeOther.Image = (System.Drawing.Image)resources.GetObject("btnEdgeOther.Image");
            btnEdgeOther.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            btnEdgeOther.Name = "btnEdgeOther";
            btnEdgeOther.Size = new System.Drawing.Size(28, 28);
            btnEdgeOther.Text = "Other Exit";
            // 
            // btnAddRoom
            // 
            btnAddRoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnAddRoom.Image = (System.Drawing.Image)resources.GetObject("btnAddRoom.Image");
            btnAddRoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnAddRoom.Name = "btnAddRoom";
            btnAddRoom.Size = new System.Drawing.Size(28, 28);
            btnAddRoom.Text = "New Room";
            // 
            // btnAddComment
            // 
            btnAddComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnAddComment.Image = (System.Drawing.Image)resources.GetObject("btnAddComment.Image");
            btnAddComment.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnAddComment.Name = "btnAddComment";
            btnAddComment.Size = new System.Drawing.Size(28, 28);
            btnAddComment.Text = "Add Comment";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // btnDelete
            // 
            btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnDelete.Image = (System.Drawing.Image)resources.GetObject("btnDelete.Image");
            btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(28, 28);
            btnDelete.Text = "Delete";
            // 
            // btnTransfer
            // 
            btnTransfer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnTransfer.Image = (System.Drawing.Image)resources.GetObject("btnTransfer.Image");
            btnTransfer.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnTransfer.Name = "btnTransfer";
            btnTransfer.Size = new System.Drawing.Size(28, 28);
            btnTransfer.Text = "Insert Transfer";
            // 
            // btnShowRoom
            // 
            btnShowRoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnShowRoom.Image = (System.Drawing.Image)resources.GetObject("btnShowRoom.Image");
            btnShowRoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnShowRoom.Name = "btnShowRoom";
            btnShowRoom.Size = new System.Drawing.Size(28, 28);
            btnShowRoom.Text = "Show Room";
            // 
            // btnHideRoom
            // 
            btnHideRoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnHideRoom.Image = (System.Drawing.Image)resources.GetObject("btnHideRoom.Image");
            btnHideRoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnHideRoom.Name = "btnHideRoom";
            btnHideRoom.Size = new System.Drawing.Size(28, 28);
            btnHideRoom.Text = "Hide Room";
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 31);
            // 
            // btnFront
            // 
            btnFront.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnFront.Image = (System.Drawing.Image)resources.GetObject("btnFront.Image");
            btnFront.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnFront.Name = "btnFront";
            btnFront.Size = new System.Drawing.Size(28, 28);
            btnFront.Text = "Bring To Front";
            btnFront.Click += mnuOrderFront_Click;
            // 
            // btnBack
            // 
            btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnBack.Image = (System.Drawing.Image)resources.GetObject("btnBack.Image");
            btnBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnBack.Name = "btnBack";
            btnBack.Size = new System.Drawing.Size(28, 28);
            btnBack.Text = "Send To Back";
            btnBack.Click += mnuOrderBack_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnZoomIn.Image = (System.Drawing.Image)resources.GetObject("btnZoomIn.Image");
            btnZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new System.Drawing.Size(28, 28);
            btnZoomIn.Text = "Zoom In";
            btnZoomIn.Click += btnZoomIn_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnZoomOut.Image = (System.Drawing.Image)resources.GetObject("btnZoomOut.Image");
            btnZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new System.Drawing.Size(28, 28);
            btnZoomOut.Text = "Zoom Out";
            btnZoomOut.Click += btnZoomOut_Click;
            // 
            // picDraw
            // 
            picDraw.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            picDraw.BackColor = System.Drawing.Color.FromArgb(255, 255, 254);
            picDraw.ContextMenuStrip = contextMenuStrip1;
            picDraw.Location = new System.Drawing.Point(0, 31);
            picDraw.Name = "picDraw";
            picDraw.Size = new System.Drawing.Size(795, 260);
            picDraw.TabIndex = 3;
            picDraw.TabStop = false;
            picDraw.MouseDoubleClick += picDraw_MouseDoubleClick;
            picDraw.MouseDown += picDraw_MouseDown;
            picDraw.MouseLeave += picDraw_MouseLeave;
            picDraw.MouseMove += picDraw_MouseMove;
            picDraw.MouseUp += picDraw_MouseUp;
            // 
            // tmrScroll
            // 
            tmrScroll.Tick += tmrScroll_Tick;
            // 
            // vScrollBar1
            // 
            vScrollBar1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            vScrollBar1.Location = new System.Drawing.Point(797, 31);
            vScrollBar1.Name = "vScrollBar1";
            vScrollBar1.Size = new System.Drawing.Size(17, 263);
            vScrollBar1.TabIndex = 4;
            vScrollBar1.TabStop = true;
            vScrollBar1.ValueChanged += vScrollBar1_ValueChanged;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            hScrollBar1.Location = new System.Drawing.Point(0, 294);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new System.Drawing.Size(798, 17);
            hScrollBar1.TabIndex = 5;
            hScrollBar1.TabStop = true;
            hScrollBar1.ValueChanged += hScrollBar1_ValueChanged;
            // 
            // txtComment
            // 
            txtComment.AcceptsTab = true;
            txtComment.Anchor = System.Windows.Forms.AnchorStyles.None;
            txtComment.BackColor = System.Drawing.SystemColors.Info;
            txtComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtComment.Location = new System.Drawing.Point(109, 101);
            txtComment.Multiline = true;
            txtComment.Name = "txtComment";
            txtComment.Size = new System.Drawing.Size(124, 126);
            txtComment.TabIndex = 6;
            txtComment.TabStop = false;
            txtComment.Visible = false;
            // 
            // mnuEditSep2
            // 
            mnuEditSep2.Name = "mnuEditSep2";
            mnuEditSep2.Size = new System.Drawing.Size(244, 6);
            // 
            // frmLayout
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(815, 311);
            Controls.Add(txtComment);
            Controls.Add(hScrollBar1);
            Controls.Add(vScrollBar1);
            Controls.Add(picDraw);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "frmLayout";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmLayout";
            FormClosing += frmLayout_FormClosing;
            FormClosed += frmLayout_FormClosed;
            Load += frmLayout_Load;
            VisibleChanged += frmLayout_VisibleChanged;
            Resize += frmLayout_Resize;
            contextMenuStrip1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picDraw).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuResource;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuROpen;
        private System.Windows.Forms.ToolStripMenuItem mnuRSave;
        private System.Windows.Forms.ToolStripMenuItem mnuRExport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuRRemove;
        private System.Windows.Forms.ToolStripMenuItem mnuRRenumber;
        private System.Windows.Forms.ToolStripMenuItem mnuRIDDesc;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuRCompileLogic;
        private System.Windows.Forms.ToolStripMenuItem mnuRSavePic;
        private System.Windows.Forms.ToolStripMenuItem mnuExportGif;
        private System.Windows.Forms.ToolStripMenuItem mnuRepair;
        private System.Windows.Forms.ToolStripMenuItem mnuToggleAllPics;
        private System.Windows.Forms.ToolStripMenuItem mnuShowRoom;
        private System.Windows.Forms.ToolStripMenuItem mnuEditLogic;
        private System.Windows.Forms.ToolStripMenuItem mnuEditPicture;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuInsert;
        private System.Windows.Forms.ToolStripSeparator mnuEditSep1;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectAll;
        private System.Windows.Forms.ToolStripMenuItem mnuTogglePicture;
        private System.Windows.Forms.ToolStripMenuItem mnuProperties;
        private System.Windows.Forms.ToolStripButton btnSelect;
        private System.Windows.Forms.ToolStripButton btnEdge1;
        private System.Windows.Forms.ToolStripButton btnEdge2;
        private System.Windows.Forms.ToolStripButton btnEdgeOther;
        private System.Windows.Forms.ToolStripButton btnAddRoom;
        private System.Windows.Forms.ToolStripButton btnAddComment;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripButton btnTransfer;
        private System.Windows.Forms.ToolStripButton btnShowRoom;
        private System.Windows.Forms.ToolStripButton btnHideRoom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnFront;
        private System.Windows.Forms.ToolStripButton btnBack;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.PictureBox picDraw;
        private System.Windows.Forms.Timer tmrScroll;
        private VScrollBarMouseAware vScrollBar1;
        private HScrollBarMouseAware hScrollBar1;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.ToolStripMenuItem mnuToggleGrid;
        private System.Windows.Forms.ToolStripMenuItem mnuOrder;
        private System.Windows.Forms.ToolStripMenuItem mnuOrderUp;
        private System.Windows.Forms.ToolStripMenuItem mnuOrderDown;
        private System.Windows.Forms.ToolStripMenuItem mnuOrderFront;
        private System.Windows.Forms.ToolStripMenuItem mnuOrderBack;
        private System.Windows.Forms.ToolStripSeparator mnuEditSep2;
    }
}