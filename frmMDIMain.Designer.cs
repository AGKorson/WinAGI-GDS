﻿namespace WinAGI.Editor {
    using System.Windows.Forms;
    partial class frmMDIMain {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMDIMain));
            TreeNode treeNode8 = new TreeNode("Logics");
            TreeNode treeNode9 = new TreeNode("Pictures");
            TreeNode treeNode10 = new TreeNode("Sounds");
            TreeNode treeNode11 = new TreeNode("Views");
            TreeNode treeNode12 = new TreeNode("Objects");
            TreeNode treeNode13 = new TreeNode("Words");
            TreeNode treeNode14 = new TreeNode("AGIGAME", new TreeNode[] { treeNode8, treeNode9, treeNode10, treeNode11, treeNode12, treeNode13 });
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            menuStrip1 = new MenuStrip();
            mnuGame = new ToolStripMenuItem();
            mnuGNew = new ToolStripMenuItem();
            mnuGNewTemplate = new ToolStripMenuItem();
            mnuGNewBlank = new ToolStripMenuItem();
            mnuGOpen = new ToolStripMenuItem();
            mnuGImport = new ToolStripMenuItem();
            mnuGClose = new ToolStripMenuItem();
            mnuGSep1 = new ToolStripSeparator();
            mnuGCompile = new ToolStripMenuItem();
            mnuGCompileTo = new ToolStripMenuItem();
            mnuGRebuild = new ToolStripMenuItem();
            mnuGCompileDirty = new ToolStripMenuItem();
            mnuGSep2 = new ToolStripSeparator();
            mnuGRun = new ToolStripMenuItem();
            mnuGSep3 = new ToolStripSeparator();
            mnuGProperties = new ToolStripMenuItem();
            mnuGMRUBar = new ToolStripSeparator();
            mnuGMRU0 = new ToolStripMenuItem();
            mnuGMRU1 = new ToolStripMenuItem();
            mnuGMRU2 = new ToolStripMenuItem();
            mnuGMRU3 = new ToolStripMenuItem();
            mnuGSep5 = new ToolStripSeparator();
            mnuGExit = new ToolStripMenuItem();
            mnuResources = new ToolStripMenuItem();
            mnuRNew = new ToolStripMenuItem();
            mnuRNLogic = new ToolStripMenuItem();
            mnuRNPicture = new ToolStripMenuItem();
            mnuRNSound = new ToolStripMenuItem();
            mnuRNView = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            mnuRNObjects = new ToolStripMenuItem();
            mnuRNWords = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            mnuRNText = new ToolStripMenuItem();
            mnuROpen = new ToolStripMenuItem();
            mnuROLogic = new ToolStripMenuItem();
            mnuROPicture = new ToolStripMenuItem();
            mnuROSound = new ToolStripMenuItem();
            mnuROView = new ToolStripMenuItem();
            toolStripSeparator13 = new ToolStripSeparator();
            mnuROObjects = new ToolStripMenuItem();
            mnuROWords = new ToolStripMenuItem();
            toolStripSeparator14 = new ToolStripSeparator();
            mnuROText = new ToolStripMenuItem();
            mnuRImport = new ToolStripMenuItem();
            mnuRILogic = new ToolStripMenuItem();
            mnuRIPicture = new ToolStripMenuItem();
            mnuRISound = new ToolStripMenuItem();
            mnuRIView = new ToolStripMenuItem();
            toolStripSeparator15 = new ToolStripSeparator();
            mnuRIObjects = new ToolStripMenuItem();
            mnuRIWords = new ToolStripMenuItem();
            mnuRSeparator1 = new ToolStripSeparator();
            mnuROpenRes = new ToolStripMenuItem();
            mnuRSave = new ToolStripMenuItem();
            mnuRExport = new ToolStripMenuItem();
            mnuRSeparator2 = new ToolStripSeparator();
            mnuRAddRemove = new ToolStripMenuItem();
            mnuRRenumber = new ToolStripMenuItem();
            mnuRIDDesc = new ToolStripMenuItem();
            mnuRSeparator3 = new ToolStripSeparator();
            mnuRCompileLogic = new ToolStripMenuItem();
            mnuRSavePicImage = new ToolStripMenuItem();
            mnuRExportGIF = new ToolStripMenuItem();
            mnuTools = new ToolStripMenuItem();
            mnuTSettings = new ToolStripMenuItem();
            mnuTSep1 = new ToolStripSeparator();
            mnuTLayout = new ToolStripMenuItem();
            mnuTMenuEditor = new ToolStripMenuItem();
            mnuTGlobals = new ToolStripMenuItem();
            mnuReserved = new ToolStripMenuItem();
            mnuTSnippets = new ToolStripMenuItem();
            mnuTPalette = new ToolStripMenuItem();
            mnuTSep2 = new ToolStripSeparator();
            mnuTCustom1 = new ToolStripMenuItem();
            mnuTCustom2 = new ToolStripMenuItem();
            mnuTCustom3 = new ToolStripMenuItem();
            mnuTCustom4 = new ToolStripMenuItem();
            mnuTCustom5 = new ToolStripMenuItem();
            mnuTCustom6 = new ToolStripMenuItem();
            mnuTSep3 = new ToolStripSeparator();
            mnuTCustomize = new ToolStripMenuItem();
            mnuWindow = new ToolStripMenuItem();
            mnuWCascade = new ToolStripMenuItem();
            mnuWTileV = new ToolStripMenuItem();
            mnuWTileH = new ToolStripMenuItem();
            mnuWArrange = new ToolStripMenuItem();
            mnuWMinimize = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            mnuWClose = new ToolStripMenuItem();
            mnuHelp = new ToolStripMenuItem();
            mnuHContents = new ToolStripMenuItem();
            mnuHIndex = new ToolStripMenuItem();
            mnuHSep1 = new ToolStripSeparator();
            mnuHCommands = new ToolStripMenuItem();
            mnuHReference = new ToolStripMenuItem();
            mnuHSep2 = new ToolStripSeparator();
            mnuHAbout = new ToolStripMenuItem();
            btnNewRes = new ToolStripSplitButton();
            btnNewLogic = new ToolStripMenuItem();
            btnNewPicture = new ToolStripMenuItem();
            btnNewSound = new ToolStripMenuItem();
            btnNewView = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            StatusPanel1 = new ToolStripStatusLabel();
            springLabel = new ToolStripStatusLabel();
            CapsLockLabel = new ToolStripStatusLabel();
            NumLockLabel = new ToolStripStatusLabel();
            InsertLockLabel = new ToolStripStatusLabel();
            toolStrip1 = new ToolStrip();
            btnOpenGame = new ToolStripButton();
            btnCloseGame = new ToolStripButton();
            btnRun = new ToolStripButton();
            btnSep1 = new ToolStripSeparator();
            btnOpenRes = new ToolStripSplitButton();
            btnOpenLogic = new ToolStripMenuItem();
            btnOpenPicture = new ToolStripMenuItem();
            btnOpenSound = new ToolStripMenuItem();
            btnOpenView = new ToolStripMenuItem();
            btnImportRes = new ToolStripSplitButton();
            btnImportLogic = new ToolStripMenuItem();
            btnImportPicture = new ToolStripMenuItem();
            btnImportSound = new ToolStripMenuItem();
            btnImportView = new ToolStripMenuItem();
            btnSep2 = new ToolStripSeparator();
            btnWords = new ToolStripButton();
            btnOjects = new ToolStripButton();
            btnSep3 = new ToolStripSeparator();
            btnSaveResource = new ToolStripButton();
            btnAddRemove = new ToolStripButton();
            btnExportRes = new ToolStripButton();
            btnSep4 = new ToolStripSeparator();
            btnLayoutEd = new ToolStripButton();
            btnMenuEd = new ToolStripButton();
            btnTextEd = new ToolStripButton();
            btnGlobals = new ToolStripButton();
            btnSep5 = new ToolStripSeparator();
            btnHelp = new ToolStripButton();
            toolStripSplitButton2 = new ToolStripSplitButton();
            toolStripSplitButton3 = new ToolStripSplitButton();
            toolStripSplitButton4 = new ToolStripSplitButton();
            pnlResources = new Panel();
            splResource = new SplitContainer();
            tvwResources = new TreeView();
            cmbResType = new ComboBox();
            lstResources = new ListView();
            columnHeader1 = new ColumnHeader();
            cmdBack = new Button();
            cmdForward = new Button();
            propertyGrid1 = new PropertyGrid();
            cmsGrid = new ContextMenuStrip(components);
            cmiDismiss = new ToolStripMenuItem();
            cmiDismissAll = new ToolStripMenuItem();
            cmiErrorHelp = new ToolStripMenuItem();
            picNavList = new PictureBox();
            splitResource = new Splitter();
            pnlWarnings = new Panel();
            fgWarnings = new DataGridView();
            colWarning = new DataGridViewTextBoxColumn();
            colDesc = new DataGridViewTextBoxColumn();
            colResNum = new DataGridViewTextBoxColumn();
            colLIne = new DataGridViewTextBoxColumn();
            colModule = new DataGridViewTextBoxColumn();
            splitWarning = new Splitter();
            imageList1 = new ImageList(components);
            tmrNavList = new Timer(components);
            FolderDlg = new FolderBrowserDialog();
            OpenDlg = new OpenFileDialog();
            SaveDlg = new SaveFileDialog();
            imlPropButtons = new ImageList(components);
            hlpWinAGI = new HelpProvider();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            pnlResources.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splResource).BeginInit();
            splResource.Panel1.SuspendLayout();
            splResource.Panel2.SuspendLayout();
            splResource.SuspendLayout();
            cmsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picNavList).BeginInit();
            pnlWarnings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)fgWarnings).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuGame, mnuResources, mnuTools, mnuWindow, mnuHelp });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.MdiWindowListItem = mnuWindow;
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1282, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // mnuGame
            // 
            mnuGame.DropDownItems.AddRange(new ToolStripItem[] { mnuGNew, mnuGOpen, mnuGImport, mnuGClose, mnuGSep1, mnuGCompile, mnuGCompileTo, mnuGRebuild, mnuGCompileDirty, mnuGSep2, mnuGRun, mnuGSep3, mnuGProperties, mnuGMRUBar, mnuGMRU0, mnuGMRU1, mnuGMRU2, mnuGMRU3, mnuGSep5, mnuGExit });
            mnuGame.Name = "mnuGame";
            mnuGame.Size = new System.Drawing.Size(50, 20);
            mnuGame.Text = "&Game";
            mnuGame.DropDownOpening += mnuGame_DropDownOpening;
            // 
            // mnuGNew
            // 
            mnuGNew.DropDownItems.AddRange(new ToolStripItem[] { mnuGNewTemplate, mnuGNewBlank });
            mnuGNew.Name = "mnuGNew";
            mnuGNew.Size = new System.Drawing.Size(261, 22);
            mnuGNew.Text = "&New Game";
            // 
            // mnuGNewTemplate
            // 
            mnuGNewTemplate.Name = "mnuGNewTemplate";
            mnuGNewTemplate.ShortcutKeys = Keys.Control | Keys.N;
            mnuGNewTemplate.Size = new System.Drawing.Size(196, 22);
            mnuGNewTemplate.Text = "From &Template";
            mnuGNewTemplate.Click += mnuGNewTemplate_Click;
            // 
            // mnuGNewBlank
            // 
            mnuGNewBlank.Name = "mnuGNewBlank";
            mnuGNewBlank.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
            mnuGNewBlank.Size = new System.Drawing.Size(196, 22);
            mnuGNewBlank.Text = "&Blank";
            mnuGNewBlank.Click += mnuGNewBlank_Click;
            // 
            // mnuGOpen
            // 
            mnuGOpen.Name = "mnuGOpen";
            mnuGOpen.ShortcutKeys = Keys.Control | Keys.O;
            mnuGOpen.Size = new System.Drawing.Size(261, 22);
            mnuGOpen.Text = "&Open Game";
            mnuGOpen.Click += mnuGOpen_Click;
            // 
            // mnuGImport
            // 
            mnuGImport.Name = "mnuGImport";
            mnuGImport.ShortcutKeys = Keys.Control | Keys.I;
            mnuGImport.Size = new System.Drawing.Size(261, 22);
            mnuGImport.Text = "&Import Game";
            mnuGImport.Click += mnuGImport_Click;
            // 
            // mnuGClose
            // 
            mnuGClose.Name = "mnuGClose";
            mnuGClose.ShortcutKeys = Keys.Alt | Keys.X;
            mnuGClose.Size = new System.Drawing.Size(261, 22);
            mnuGClose.Text = "C&lose Game";
            mnuGClose.Click += mnuGClose_Click;
            // 
            // mnuGSep1
            // 
            mnuGSep1.Name = "mnuGSep1";
            mnuGSep1.Size = new System.Drawing.Size(258, 6);
            // 
            // mnuGCompile
            // 
            mnuGCompile.Name = "mnuGCompile";
            mnuGCompile.ShortcutKeys = Keys.Control | Keys.B;
            mnuGCompile.Size = new System.Drawing.Size(261, 22);
            mnuGCompile.Text = "&Compile Game";
            mnuGCompile.Click += mnuGCompile_Click;
            // 
            // mnuGCompileTo
            // 
            mnuGCompileTo.Name = "mnuGCompileTo";
            mnuGCompileTo.ShortcutKeys = Keys.Control | Keys.Shift | Keys.B;
            mnuGCompileTo.Size = new System.Drawing.Size(261, 22);
            mnuGCompileTo.Text = "Compile &To ...";
            mnuGCompileTo.Click += mnuGCompileTo_Click;
            // 
            // mnuGRebuild
            // 
            mnuGRebuild.Name = "mnuGRebuild";
            mnuGRebuild.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
            mnuGRebuild.Size = new System.Drawing.Size(261, 22);
            mnuGRebuild.Text = "Rebuild &VOL Files";
            mnuGRebuild.Click += mnuGRebuild_Click;
            // 
            // mnuGCompileDirty
            // 
            mnuGCompileDirty.Name = "mnuGCompileDirty";
            mnuGCompileDirty.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
            mnuGCompileDirty.Size = new System.Drawing.Size(261, 22);
            mnuGCompileDirty.Text = "Complile &Dirty Logics";
            mnuGCompileDirty.Click += mnuGCompileDirty_Click;
            // 
            // mnuGSep2
            // 
            mnuGSep2.Name = "mnuGSep2";
            mnuGSep2.Size = new System.Drawing.Size(258, 6);
            // 
            // mnuGRun
            // 
            mnuGRun.Name = "mnuGRun";
            mnuGRun.ShortcutKeys = Keys.Control | Keys.R;
            mnuGRun.Size = new System.Drawing.Size(261, 22);
            mnuGRun.Text = "&Run";
            mnuGRun.Click += mnuGRun_Click;
            // 
            // mnuGSep3
            // 
            mnuGSep3.Name = "mnuGSep3";
            mnuGSep3.Size = new System.Drawing.Size(258, 6);
            // 
            // mnuGProperties
            // 
            mnuGProperties.Name = "mnuGProperties";
            mnuGProperties.ShortcutKeys = Keys.F4;
            mnuGProperties.Size = new System.Drawing.Size(261, 22);
            mnuGProperties.Text = "&Properties ...";
            mnuGProperties.Click += mnuGProperties_Click;
            // 
            // mnuGMRUBar
            // 
            mnuGMRUBar.Name = "mnuGMRUBar";
            mnuGMRUBar.Size = new System.Drawing.Size(258, 6);
            mnuGMRUBar.Visible = false;
            // 
            // mnuGMRU0
            // 
            mnuGMRU0.Name = "mnuGMRU0";
            mnuGMRU0.Size = new System.Drawing.Size(261, 22);
            mnuGMRU0.Tag = "0";
            mnuGMRU0.Text = "mru1";
            mnuGMRU0.Visible = false;
            mnuGMRU0.Click += mnuGMRU_Click;
            // 
            // mnuGMRU1
            // 
            mnuGMRU1.Name = "mnuGMRU1";
            mnuGMRU1.Size = new System.Drawing.Size(261, 22);
            mnuGMRU1.Tag = "1";
            mnuGMRU1.Text = "mru2";
            mnuGMRU1.Visible = false;
            mnuGMRU1.Click += mnuGMRU_Click;
            // 
            // mnuGMRU2
            // 
            mnuGMRU2.Name = "mnuGMRU2";
            mnuGMRU2.Size = new System.Drawing.Size(261, 22);
            mnuGMRU2.Tag = "2";
            mnuGMRU2.Text = "mru3";
            mnuGMRU2.Visible = false;
            mnuGMRU2.Click += mnuGMRU_Click;
            // 
            // mnuGMRU3
            // 
            mnuGMRU3.Name = "mnuGMRU3";
            mnuGMRU3.Size = new System.Drawing.Size(261, 22);
            mnuGMRU3.Tag = "3";
            mnuGMRU3.Text = "mru4";
            mnuGMRU3.Visible = false;
            mnuGMRU3.Click += mnuGMRU_Click;
            // 
            // mnuGSep5
            // 
            mnuGSep5.Name = "mnuGSep5";
            mnuGSep5.Size = new System.Drawing.Size(258, 6);
            // 
            // mnuGExit
            // 
            mnuGExit.Name = "mnuGExit";
            mnuGExit.ShortcutKeyDisplayString = "Alt+F4";
            mnuGExit.Size = new System.Drawing.Size(261, 22);
            mnuGExit.Text = "E&xit";
            mnuGExit.Click += mnuGExit_Click;
            // 
            // mnuResources
            // 
            mnuResources.DropDownItems.AddRange(new ToolStripItem[] { mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF });
            mnuResources.ImageScaling = ToolStripItemImageScaling.None;
            mnuResources.Name = "mnuResources";
            mnuResources.Size = new System.Drawing.Size(72, 20);
            mnuResources.Text = "&Resources";
            mnuResources.DropDownClosed += mnuResources_DropDownClosed;
            mnuResources.DropDownOpening += mnuResources_DropDownOpening;
            // 
            // mnuRNew
            // 
            mnuRNew.DropDownItems.AddRange(new ToolStripItem[] { mnuRNLogic, mnuRNPicture, mnuRNSound, mnuRNView, toolStripSeparator6, mnuRNObjects, mnuRNWords, toolStripSeparator5, mnuRNText });
            mnuRNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            mnuRNew.Name = "mnuRNew";
            mnuRNew.Size = new System.Drawing.Size(305, 22);
            mnuRNew.Text = "&New Resource";
            // 
            // mnuRNLogic
            // 
            mnuRNLogic.Name = "mnuRNLogic";
            mnuRNLogic.ShortcutKeys = Keys.Control | Keys.D1;
            mnuRNLogic.Size = new System.Drawing.Size(200, 22);
            mnuRNLogic.Text = "&Logic";
            mnuRNLogic.Click += mnuRNLogic_Click;
            // 
            // mnuRNPicture
            // 
            mnuRNPicture.Name = "mnuRNPicture";
            mnuRNPicture.ShortcutKeys = Keys.Control | Keys.D2;
            mnuRNPicture.Size = new System.Drawing.Size(200, 22);
            mnuRNPicture.Text = "&Picture";
            mnuRNPicture.Click += mnuRNPicture_Click;
            // 
            // mnuRNSound
            // 
            mnuRNSound.Name = "mnuRNSound";
            mnuRNSound.ShortcutKeys = Keys.Control | Keys.D3;
            mnuRNSound.Size = new System.Drawing.Size(200, 22);
            mnuRNSound.Text = "&Sound";
            mnuRNSound.Click += mnuRNSound_Click;
            // 
            // mnuRNView
            // 
            mnuRNView.Name = "mnuRNView";
            mnuRNView.ShortcutKeys = Keys.Control | Keys.D4;
            mnuRNView.Size = new System.Drawing.Size(200, 22);
            mnuRNView.Text = "&View";
            mnuRNView.Click += mnuRNView_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(197, 6);
            // 
            // mnuRNObjects
            // 
            mnuRNObjects.Name = "mnuRNObjects";
            mnuRNObjects.ShortcutKeys = Keys.Control | Keys.D5;
            mnuRNObjects.Size = new System.Drawing.Size(200, 22);
            mnuRNObjects.Text = "&OBJECT File";
            mnuRNObjects.Click += mnuRNObjects_Click;
            // 
            // mnuRNWords
            // 
            mnuRNWords.Name = "mnuRNWords";
            mnuRNWords.ShortcutKeys = Keys.Control | Keys.D6;
            mnuRNWords.Size = new System.Drawing.Size(200, 22);
            mnuRNWords.Text = "&WORDS.TOK File";
            mnuRNWords.Click += mnuRNWords_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(197, 6);
            // 
            // mnuRNText
            // 
            mnuRNText.Name = "mnuRNText";
            mnuRNText.ShortcutKeys = Keys.Control | Keys.D7;
            mnuRNText.Size = new System.Drawing.Size(200, 22);
            mnuRNText.Text = "&Text File";
            mnuRNText.Click += mnuRNText_Click;
            // 
            // mnuROpen
            // 
            mnuROpen.DropDownItems.AddRange(new ToolStripItem[] { mnuROLogic, mnuROPicture, mnuROSound, mnuROView, toolStripSeparator13, mnuROObjects, mnuROWords, toolStripSeparator14, mnuROText });
            mnuROpen.Image = (System.Drawing.Image)resources.GetObject("mnuROpen.Image");
            mnuROpen.ImageScaling = ToolStripItemImageScaling.None;
            mnuROpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            mnuROpen.Name = "mnuROpen";
            mnuROpen.ShortcutKeys = Keys.Control | Keys.O;
            mnuROpen.Size = new System.Drawing.Size(305, 22);
            mnuROpen.Text = "&Open Resource";
            // 
            // mnuROLogic
            // 
            mnuROLogic.Name = "mnuROLogic";
            mnuROLogic.ShortcutKeys = Keys.Alt | Keys.D1;
            mnuROLogic.Size = new System.Drawing.Size(196, 22);
            mnuROLogic.Text = "&Logic";
            mnuROLogic.Click += mnuROLogic_Click;
            // 
            // mnuROPicture
            // 
            mnuROPicture.Name = "mnuROPicture";
            mnuROPicture.ShortcutKeys = Keys.Alt | Keys.D2;
            mnuROPicture.Size = new System.Drawing.Size(196, 22);
            mnuROPicture.Text = "&Picture";
            mnuROPicture.Click += mnuROPicture_Click;
            // 
            // mnuROSound
            // 
            mnuROSound.Name = "mnuROSound";
            mnuROSound.ShortcutKeys = Keys.Alt | Keys.D3;
            mnuROSound.Size = new System.Drawing.Size(196, 22);
            mnuROSound.Text = "&Sound";
            mnuROSound.Click += mnuROSound_Click;
            // 
            // mnuROView
            // 
            mnuROView.Name = "mnuROView";
            mnuROView.ShortcutKeys = Keys.Alt | Keys.D4;
            mnuROView.Size = new System.Drawing.Size(196, 22);
            mnuROView.Text = "&View";
            mnuROView.Click += mnuROView_Click;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new System.Drawing.Size(193, 6);
            // 
            // mnuROObjects
            // 
            mnuROObjects.Name = "mnuROObjects";
            mnuROObjects.ShortcutKeys = Keys.Alt | Keys.D5;
            mnuROObjects.Size = new System.Drawing.Size(196, 22);
            mnuROObjects.Text = "&OBJECT File";
            mnuROObjects.Click += mnuROObjects_Click;
            // 
            // mnuROWords
            // 
            mnuROWords.Name = "mnuROWords";
            mnuROWords.ShortcutKeys = Keys.Alt | Keys.D6;
            mnuROWords.Size = new System.Drawing.Size(196, 22);
            mnuROWords.Text = "&WORDS.TOK File";
            mnuROWords.Click += mnuROWords_Click;
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new System.Drawing.Size(193, 6);
            // 
            // mnuROText
            // 
            mnuROText.Name = "mnuROText";
            mnuROText.ShortcutKeys = Keys.Alt | Keys.D7;
            mnuROText.Size = new System.Drawing.Size(196, 22);
            mnuROText.Text = "&Text File";
            mnuROText.Click += mnuROText_Click;
            // 
            // mnuRImport
            // 
            mnuRImport.DropDownItems.AddRange(new ToolStripItem[] { mnuRILogic, mnuRIPicture, mnuRISound, mnuRIView, toolStripSeparator15, mnuRIObjects, mnuRIWords });
            mnuRImport.Name = "mnuRImport";
            mnuRImport.Size = new System.Drawing.Size(305, 22);
            mnuRImport.Text = "&Import Resource";
            // 
            // mnuRILogic
            // 
            mnuRILogic.Name = "mnuRILogic";
            mnuRILogic.ShortcutKeys = Keys.Control | Keys.Alt | Keys.D1;
            mnuRILogic.Size = new System.Drawing.Size(223, 22);
            mnuRILogic.Text = "&Logic";
            mnuRILogic.Click += mnuRILogic_Click;
            // 
            // mnuRIPicture
            // 
            mnuRIPicture.Name = "mnuRIPicture";
            mnuRIPicture.ShortcutKeys = Keys.Control | Keys.Alt | Keys.D2;
            mnuRIPicture.Size = new System.Drawing.Size(223, 22);
            mnuRIPicture.Text = "&Picture";
            mnuRIPicture.Click += mnuRIPicture_Click;
            // 
            // mnuRISound
            // 
            mnuRISound.Name = "mnuRISound";
            mnuRISound.ShortcutKeys = Keys.Control | Keys.Alt | Keys.D3;
            mnuRISound.Size = new System.Drawing.Size(223, 22);
            mnuRISound.Text = "&Sound";
            mnuRISound.Click += mnuRISound_Click;
            // 
            // mnuRIView
            // 
            mnuRIView.Name = "mnuRIView";
            mnuRIView.ShortcutKeys = Keys.Control | Keys.Alt | Keys.D4;
            mnuRIView.Size = new System.Drawing.Size(223, 22);
            mnuRIView.Text = "&View";
            mnuRIView.Click += mnuRIView_Click;
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new System.Drawing.Size(220, 6);
            // 
            // mnuRIObjects
            // 
            mnuRIObjects.Name = "mnuRIObjects";
            mnuRIObjects.ShortcutKeys = Keys.Control | Keys.Alt | Keys.D5;
            mnuRIObjects.Size = new System.Drawing.Size(223, 22);
            mnuRIObjects.Text = "&OBJECT File";
            mnuRIObjects.Click += mnuRIObjects_Click;
            // 
            // mnuRIWords
            // 
            mnuRIWords.Name = "mnuRIWords";
            mnuRIWords.ShortcutKeys = Keys.Control | Keys.Alt | Keys.D6;
            mnuRIWords.Size = new System.Drawing.Size(223, 22);
            mnuRIWords.Text = "&WORDS.TOK File";
            mnuRIWords.Click += mnuRIWords_Click;
            // 
            // mnuRSeparator1
            // 
            mnuRSeparator1.Name = "mnuRSeparator1";
            mnuRSeparator1.Size = new System.Drawing.Size(302, 6);
            // 
            // mnuROpenRes
            // 
            mnuROpenRes.Name = "mnuROpenRes";
            mnuROpenRes.Size = new System.Drawing.Size(305, 22);
            mnuROpenRes.Text = "Open Resource";
            // 
            // mnuRSave
            // 
            mnuRSave.Image = (System.Drawing.Image)resources.GetObject("mnuRSave.Image");
            mnuRSave.ImageScaling = ToolStripItemImageScaling.None;
            mnuRSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            mnuRSave.Name = "mnuRSave";
            mnuRSave.ShortcutKeys = Keys.Control | Keys.S;
            mnuRSave.Size = new System.Drawing.Size(305, 22);
            mnuRSave.Text = "&Save Resource";
            // 
            // mnuRExport
            // 
            mnuRExport.Name = "mnuRExport";
            mnuRExport.ShortcutKeys = Keys.Control | Keys.E;
            mnuRExport.Size = new System.Drawing.Size(305, 22);
            mnuRExport.Text = "&Export Resource";
            // 
            // mnuRSeparator2
            // 
            mnuRSeparator2.Name = "mnuRSeparator2";
            mnuRSeparator2.Size = new System.Drawing.Size(302, 6);
            // 
            // mnuRAddRemove
            // 
            mnuRAddRemove.Name = "mnuRAddRemove";
            mnuRAddRemove.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            mnuRAddRemove.Size = new System.Drawing.Size(305, 22);
            mnuRAddRemove.Text = "Remove Resource from &Game";
            mnuRAddRemove.Click += mnuRAddRemove_Click;
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.ShortcutKeys = Keys.Alt | Keys.N;
            mnuRRenumber.Size = new System.Drawing.Size(305, 22);
            mnuRRenumber.Text = "&Renumber Resource";
            mnuRRenumber.Click += mnuRRenumber_Click;
            // 
            // mnuRIDDesc
            // 
            mnuRIDDesc.Image = (System.Drawing.Image)resources.GetObject("mnuRIDDesc.Image");
            mnuRIDDesc.ImageScaling = ToolStripItemImageScaling.None;
            mnuRIDDesc.ImageTransparentColor = System.Drawing.Color.Magenta;
            mnuRIDDesc.Name = "mnuRIDDesc";
            mnuRIDDesc.ShortcutKeys = Keys.Control | Keys.D;
            mnuRIDDesc.Size = new System.Drawing.Size(305, 22);
            mnuRIDDesc.Text = "I&D/Description ...";
            mnuRIDDesc.Click += mnuRIDDesc_Click;
            // 
            // mnuRSeparator3
            // 
            mnuRSeparator3.Name = "mnuRSeparator3";
            mnuRSeparator3.Size = new System.Drawing.Size(302, 6);
            // 
            // mnuRCompileLogic
            // 
            mnuRCompileLogic.Name = "mnuRCompileLogic";
            mnuRCompileLogic.Size = new System.Drawing.Size(305, 22);
            mnuRCompileLogic.Text = "Compile This Logic";
            // 
            // mnuRSavePicImage
            // 
            mnuRSavePicImage.Name = "mnuRSavePicImage";
            mnuRSavePicImage.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            mnuRSavePicImage.Size = new System.Drawing.Size(305, 22);
            mnuRSavePicImage.Text = "Save Picture Image As...";
            // 
            // mnuRExportGIF
            // 
            mnuRExportGIF.Name = "mnuRExportGIF";
            mnuRExportGIF.Size = new System.Drawing.Size(305, 22);
            mnuRExportGIF.Text = "Export Loop as GIF...";
            // 
            // mnuTools
            // 
            mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuTSettings, mnuTSep1, mnuTLayout, mnuTMenuEditor, mnuTGlobals, mnuReserved, mnuTSnippets, mnuTPalette, mnuTSep2, mnuTCustom1, mnuTCustom2, mnuTCustom3, mnuTCustom4, mnuTCustom5, mnuTCustom6, mnuTSep3, mnuTCustomize });
            mnuTools.MergeIndex = 1;
            mnuTools.Name = "mnuTools";
            mnuTools.Size = new System.Drawing.Size(46, 20);
            mnuTools.Text = "&Tools";
            mnuTools.DropDownOpening += mnuTools_DropDownOpening;
            // 
            // mnuTSettings
            // 
            mnuTSettings.Name = "mnuTSettings";
            mnuTSettings.ShortcutKeys = Keys.F2;
            mnuTSettings.Size = new System.Drawing.Size(234, 22);
            mnuTSettings.Text = "&Settings";
            mnuTSettings.Click += mnuTSettings_Click;
            // 
            // mnuTSep1
            // 
            mnuTSep1.Name = "mnuTSep1";
            mnuTSep1.Size = new System.Drawing.Size(231, 6);
            // 
            // mnuTLayout
            // 
            mnuTLayout.Name = "mnuTLayout";
            mnuTLayout.ShortcutKeys = Keys.Control | Keys.L;
            mnuTLayout.Size = new System.Drawing.Size(234, 22);
            mnuTLayout.Text = "Room &Layout Editor";
            // 
            // mnuTMenuEditor
            // 
            mnuTMenuEditor.Name = "mnuTMenuEditor";
            mnuTMenuEditor.ShortcutKeys = Keys.Control | Keys.M;
            mnuTMenuEditor.Size = new System.Drawing.Size(234, 22);
            mnuTMenuEditor.Text = "&Menu Editor";
            // 
            // mnuTGlobals
            // 
            mnuTGlobals.Name = "mnuTGlobals";
            mnuTGlobals.ShortcutKeys = Keys.Control | Keys.G;
            mnuTGlobals.Size = new System.Drawing.Size(234, 22);
            mnuTGlobals.Text = "&Global Defines ...";
            mnuTGlobals.Click += mnuTGlobals_Click;
            // 
            // mnuReserved
            // 
            mnuReserved.Name = "mnuReserved";
            mnuReserved.ShortcutKeys = Keys.Control | Keys.W;
            mnuReserved.Size = new System.Drawing.Size(234, 22);
            mnuReserved.Text = "&Reserved Defines ...";
            // 
            // mnuTSnippets
            // 
            mnuTSnippets.Name = "mnuTSnippets";
            mnuTSnippets.ShortcutKeys = Keys.Control | Keys.Shift | Keys.T;
            mnuTSnippets.Size = new System.Drawing.Size(234, 22);
            mnuTSnippets.Text = "Code &Snippets ...";
            // 
            // mnuTPalette
            // 
            mnuTPalette.Name = "mnuTPalette";
            mnuTPalette.ShortcutKeys = Keys.Control | Keys.Shift | Keys.P;
            mnuTPalette.Size = new System.Drawing.Size(234, 22);
            mnuTPalette.Text = "Color &Palette ...";
            // 
            // mnuTSep2
            // 
            mnuTSep2.Name = "mnuTSep2";
            mnuTSep2.Size = new System.Drawing.Size(231, 6);
            mnuTSep2.Visible = false;
            // 
            // mnuTCustom1
            // 
            mnuTCustom1.Name = "mnuTCustom1";
            mnuTCustom1.Size = new System.Drawing.Size(234, 22);
            mnuTCustom1.Text = "tool1";
            mnuTCustom1.Visible = false;
            mnuTCustom1.Click += mnuTCustom_Click;
            // 
            // mnuTCustom2
            // 
            mnuTCustom2.Name = "mnuTCustom2";
            mnuTCustom2.Size = new System.Drawing.Size(234, 22);
            mnuTCustom2.Text = "tool2";
            mnuTCustom2.Visible = false;
            mnuTCustom2.Click += mnuTCustom_Click;
            // 
            // mnuTCustom3
            // 
            mnuTCustom3.Name = "mnuTCustom3";
            mnuTCustom3.Size = new System.Drawing.Size(234, 22);
            mnuTCustom3.Text = "tool3";
            mnuTCustom3.Visible = false;
            mnuTCustom3.Click += mnuTCustom_Click;
            // 
            // mnuTCustom4
            // 
            mnuTCustom4.Name = "mnuTCustom4";
            mnuTCustom4.Size = new System.Drawing.Size(234, 22);
            mnuTCustom4.Text = "tool4";
            mnuTCustom4.Visible = false;
            mnuTCustom4.Click += mnuTCustom_Click;
            // 
            // mnuTCustom5
            // 
            mnuTCustom5.Name = "mnuTCustom5";
            mnuTCustom5.Size = new System.Drawing.Size(234, 22);
            mnuTCustom5.Text = "tool5";
            mnuTCustom5.Visible = false;
            mnuTCustom5.Click += mnuTCustom_Click;
            // 
            // mnuTCustom6
            // 
            mnuTCustom6.Name = "mnuTCustom6";
            mnuTCustom6.Size = new System.Drawing.Size(234, 22);
            mnuTCustom6.Text = "tool6";
            mnuTCustom6.Visible = false;
            mnuTCustom6.Click += mnuTCustom_Click;
            // 
            // mnuTSep3
            // 
            mnuTSep3.Name = "mnuTSep3";
            mnuTSep3.Size = new System.Drawing.Size(231, 6);
            // 
            // mnuTCustomize
            // 
            mnuTCustomize.Name = "mnuTCustomize";
            mnuTCustomize.ShortcutKeys = Keys.F6;
            mnuTCustomize.Size = new System.Drawing.Size(234, 22);
            mnuTCustomize.Text = "&Customize Tool Menu ...";
            mnuTCustomize.Click += mnuTCustomize_Click;
            // 
            // mnuWindow
            // 
            mnuWindow.DropDownItems.AddRange(new ToolStripItem[] { mnuWCascade, mnuWTileV, mnuWTileH, mnuWArrange, mnuWMinimize, toolStripSeparator8, mnuWClose });
            mnuWindow.Name = "mnuWindow";
            mnuWindow.Size = new System.Drawing.Size(63, 20);
            mnuWindow.Text = "&Window";
            mnuWindow.DropDownOpening += mnuWindow_DropDownOpening;
            // 
            // mnuWCascade
            // 
            mnuWCascade.Name = "mnuWCascade";
            mnuWCascade.Size = new System.Drawing.Size(150, 22);
            mnuWCascade.Text = "Cascade";
            mnuWCascade.Click += mnuWCascade_Click;
            // 
            // mnuWTileV
            // 
            mnuWTileV.Name = "mnuWTileV";
            mnuWTileV.Size = new System.Drawing.Size(150, 22);
            mnuWTileV.Text = "Tile Vertical";
            mnuWTileV.Click += mnuWTileV_Click;
            // 
            // mnuWTileH
            // 
            mnuWTileH.Name = "mnuWTileH";
            mnuWTileH.Size = new System.Drawing.Size(150, 22);
            mnuWTileH.Text = "Tile Horizontal";
            mnuWTileH.Click += mnuWTileH_Click;
            // 
            // mnuWArrange
            // 
            mnuWArrange.Name = "mnuWArrange";
            mnuWArrange.Size = new System.Drawing.Size(150, 22);
            mnuWArrange.Text = "Arrange Icons";
            mnuWArrange.Click += mnuWArrange_Click;
            // 
            // mnuWMinimize
            // 
            mnuWMinimize.Name = "mnuWMinimize";
            mnuWMinimize.Size = new System.Drawing.Size(150, 22);
            mnuWMinimize.Text = "Minimize All";
            mnuWMinimize.Click += mnuWMinimize_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(147, 6);
            // 
            // mnuWClose
            // 
            mnuWClose.Name = "mnuWClose";
            mnuWClose.Size = new System.Drawing.Size(150, 22);
            mnuWClose.Text = "Close Window";
            mnuWClose.Click += mnuWClose_Click;
            // 
            // mnuHelp
            // 
            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { mnuHContents, mnuHIndex, mnuHSep1, mnuHCommands, mnuHReference, mnuHSep2, mnuHAbout });
            mnuHelp.Name = "mnuHelp";
            mnuHelp.Size = new System.Drawing.Size(44, 20);
            mnuHelp.Text = "&Help";
            // 
            // mnuHContents
            // 
            mnuHContents.Name = "mnuHContents";
            mnuHContents.ShortcutKeys = Keys.F1;
            mnuHContents.Size = new System.Drawing.Size(238, 22);
            mnuHContents.Text = "&Contents";
            mnuHContents.Click += mnuHContents_Click;
            // 
            // mnuHIndex
            // 
            mnuHIndex.Name = "mnuHIndex";
            mnuHIndex.Size = new System.Drawing.Size(238, 22);
            mnuHIndex.Text = "&Index";
            mnuHIndex.Click += mnuHIndex_Click;
            // 
            // mnuHSep1
            // 
            mnuHSep1.Name = "mnuHSep1";
            mnuHSep1.Size = new System.Drawing.Size(235, 6);
            // 
            // mnuHCommands
            // 
            mnuHCommands.Name = "mnuHCommands";
            mnuHCommands.ShortcutKeys = Keys.Alt | Keys.F1;
            mnuHCommands.Size = new System.Drawing.Size(238, 22);
            mnuHCommands.Text = "&Logic Commands Help";
            mnuHCommands.Click += mnuHCommands_Click;
            // 
            // mnuHReference
            // 
            mnuHReference.Name = "mnuHReference";
            mnuHReference.ShortcutKeys = Keys.F11;
            mnuHReference.Size = new System.Drawing.Size(238, 22);
            mnuHReference.Text = "AGI &Reference";
            mnuHReference.Click += mnuHReference_Click;
            // 
            // mnuHSep2
            // 
            mnuHSep2.Name = "mnuHSep2";
            mnuHSep2.Size = new System.Drawing.Size(235, 6);
            // 
            // mnuHAbout
            // 
            mnuHAbout.Name = "mnuHAbout";
            mnuHAbout.Size = new System.Drawing.Size(238, 22);
            mnuHAbout.Text = "&About WinAGI GDS...";
            mnuHAbout.Click += mnuHAbout_Click;
            // 
            // btnNewRes
            // 
            btnNewRes.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnNewRes.DropDownItems.AddRange(new ToolStripItem[] { btnNewLogic, btnNewPicture, btnNewSound, btnNewView });
            btnNewRes.Image = (System.Drawing.Image)resources.GetObject("btnNewRes.Image");
            btnNewRes.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnNewRes.Margin = new Padding(2, 2, 2, 4);
            btnNewRes.Name = "btnNewRes";
            btnNewRes.Size = new System.Drawing.Size(40, 28);
            btnNewRes.Text = "&New Resource";
            // 
            // btnNewLogic
            // 
            btnNewLogic.Image = (System.Drawing.Image)resources.GetObject("btnNewLogic.Image");
            btnNewLogic.Name = "btnNewLogic";
            btnNewLogic.Size = new System.Drawing.Size(138, 22);
            btnNewLogic.Text = "New Logic";
            btnNewLogic.Click += btnNewLogic_Click;
            // 
            // btnNewPicture
            // 
            btnNewPicture.Image = (System.Drawing.Image)resources.GetObject("btnNewPicture.Image");
            btnNewPicture.Name = "btnNewPicture";
            btnNewPicture.Size = new System.Drawing.Size(138, 22);
            btnNewPicture.Text = "New Picture";
            btnNewPicture.Click += btnNewPicture_Click;
            // 
            // btnNewSound
            // 
            btnNewSound.Image = (System.Drawing.Image)resources.GetObject("btnNewSound.Image");
            btnNewSound.Name = "btnNewSound";
            btnNewSound.Size = new System.Drawing.Size(138, 22);
            btnNewSound.Text = "New Sound";
            btnNewSound.Click += btnNewSound_Click;
            // 
            // btnNewView
            // 
            btnNewView.Image = (System.Drawing.Image)resources.GetObject("btnNewView.Image");
            btnNewView.Name = "btnNewView";
            btnNewView.Size = new System.Drawing.Size(138, 22);
            btnNewView.Text = "New View";
            btnNewView.Click += btnNewView_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new ToolStripItem[] { StatusPanel1, springLabel, CapsLockLabel, NumLockLabel, InsertLockLabel });
            statusStrip1.Location = new System.Drawing.Point(0, 355);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 13, 0);
            statusStrip1.Size = new System.Drawing.Size(1282, 23);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // StatusPanel1
            // 
            StatusPanel1.Name = "StatusPanel1";
            StatusPanel1.Size = new System.Drawing.Size(0, 18);
            // 
            // springLabel
            // 
            springLabel.Name = "springLabel";
            springLabel.Size = new System.Drawing.Size(1046, 18);
            springLabel.Spring = true;
            // 
            // CapsLockLabel
            // 
            CapsLockLabel.AutoSize = false;
            CapsLockLabel.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            CapsLockLabel.BorderStyle = Border3DStyle.SunkenInner;
            CapsLockLabel.Name = "CapsLockLabel";
            CapsLockLabel.Size = new System.Drawing.Size(74, 18);
            CapsLockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NumLockLabel
            // 
            NumLockLabel.AutoSize = false;
            NumLockLabel.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            NumLockLabel.BorderStyle = Border3DStyle.SunkenInner;
            NumLockLabel.Name = "NumLockLabel";
            NumLockLabel.Size = new System.Drawing.Size(74, 18);
            NumLockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // InsertLockLabel
            // 
            InsertLockLabel.AutoSize = false;
            InsertLockLabel.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            InsertLockLabel.BorderStyle = Border3DStyle.SunkenInner;
            InsertLockLabel.Name = "InsertLockLabel";
            InsertLockLabel.Size = new System.Drawing.Size(74, 18);
            // 
            // toolStrip1
            // 
            toolStrip1.AllowItemReorder = true;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnOpenGame, btnCloseGame, btnRun, btnSep1, btnNewRes, btnOpenRes, btnImportRes, btnSep2, btnWords, btnOjects, btnSep3, btnSaveResource, btnAddRemove, btnExportRes, btnSep4, btnLayoutEd, btnMenuEd, btnTextEd, btnGlobals, btnSep5, btnHelp });
            toolStrip1.Location = new System.Drawing.Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(0, 1, 2, 1);
            toolStrip1.Size = new System.Drawing.Size(1282, 36);
            toolStrip1.TabIndex = 3;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnOpenGame
            // 
            btnOpenGame.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnOpenGame.Image = (System.Drawing.Image)resources.GetObject("btnOpenGame.Image");
            btnOpenGame.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnOpenGame.Margin = new Padding(2, 2, 2, 4);
            btnOpenGame.Name = "btnOpenGame";
            btnOpenGame.Size = new System.Drawing.Size(28, 28);
            btnOpenGame.Text = "&Open";
            btnOpenGame.Click += btnOpenGame_Click;
            // 
            // btnCloseGame
            // 
            btnCloseGame.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnCloseGame.Enabled = false;
            btnCloseGame.Image = (System.Drawing.Image)resources.GetObject("btnCloseGame.Image");
            btnCloseGame.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnCloseGame.Margin = new Padding(2, 2, 2, 4);
            btnCloseGame.Name = "btnCloseGame";
            btnCloseGame.Size = new System.Drawing.Size(28, 28);
            btnCloseGame.Text = "&Close";
            btnCloseGame.Click += mnuGClose_Click;
            // 
            // btnRun
            // 
            btnRun.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnRun.Enabled = false;
            btnRun.Image = (System.Drawing.Image)resources.GetObject("btnRun.Image");
            btnRun.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnRun.Margin = new Padding(2, 2, 2, 4);
            btnRun.Name = "btnRun";
            btnRun.Size = new System.Drawing.Size(28, 28);
            btnRun.Text = "&Run";
            btnRun.Click += mnuGRun_Click;
            // 
            // btnSep1
            // 
            btnSep1.Margin = new Padding(8, 0, 8, 0);
            btnSep1.Name = "btnSep1";
            btnSep1.Size = new System.Drawing.Size(6, 34);
            // 
            // btnOpenRes
            // 
            btnOpenRes.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnOpenRes.DropDownItems.AddRange(new ToolStripItem[] { btnOpenLogic, btnOpenPicture, btnOpenSound, btnOpenView });
            btnOpenRes.Image = (System.Drawing.Image)resources.GetObject("btnOpenRes.Image");
            btnOpenRes.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnOpenRes.Margin = new Padding(2, 2, 2, 4);
            btnOpenRes.Name = "btnOpenRes";
            btnOpenRes.Size = new System.Drawing.Size(40, 28);
            btnOpenRes.Text = "Open Resource";
            // 
            // btnOpenLogic
            // 
            btnOpenLogic.Image = (System.Drawing.Image)resources.GetObject("btnOpenLogic.Image");
            btnOpenLogic.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnOpenLogic.Name = "btnOpenLogic";
            btnOpenLogic.Size = new System.Drawing.Size(143, 22);
            btnOpenLogic.Text = "Open Logic";
            btnOpenLogic.Click += btnOpenLogic_Click;
            // 
            // btnOpenPicture
            // 
            btnOpenPicture.Image = (System.Drawing.Image)resources.GetObject("btnOpenPicture.Image");
            btnOpenPicture.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnOpenPicture.Name = "btnOpenPicture";
            btnOpenPicture.Size = new System.Drawing.Size(143, 22);
            btnOpenPicture.Text = "Open Picture";
            btnOpenPicture.Click += btnOpenPicture_Click;
            // 
            // btnOpenSound
            // 
            btnOpenSound.Image = (System.Drawing.Image)resources.GetObject("btnOpenSound.Image");
            btnOpenSound.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnOpenSound.Name = "btnOpenSound";
            btnOpenSound.Size = new System.Drawing.Size(143, 22);
            btnOpenSound.Text = "Open Sound";
            btnOpenSound.Click += btnOpenSound_Click;
            // 
            // btnOpenView
            // 
            btnOpenView.Image = (System.Drawing.Image)resources.GetObject("btnOpenView.Image");
            btnOpenView.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnOpenView.Name = "btnOpenView";
            btnOpenView.Size = new System.Drawing.Size(143, 22);
            btnOpenView.Text = "Open View";
            btnOpenView.Click += btnOpenView_Click;
            // 
            // btnImportRes
            // 
            btnImportRes.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnImportRes.DropDownItems.AddRange(new ToolStripItem[] { btnImportLogic, btnImportPicture, btnImportSound, btnImportView });
            btnImportRes.Enabled = false;
            btnImportRes.Image = (System.Drawing.Image)resources.GetObject("btnImportRes.Image");
            btnImportRes.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnImportRes.Margin = new Padding(2, 2, 2, 4);
            btnImportRes.Name = "btnImportRes";
            btnImportRes.Size = new System.Drawing.Size(40, 28);
            btnImportRes.Text = "Import Resource";
            // 
            // btnImportLogic
            // 
            btnImportLogic.Image = (System.Drawing.Image)resources.GetObject("btnImportLogic.Image");
            btnImportLogic.Name = "btnImportLogic";
            btnImportLogic.Size = new System.Drawing.Size(150, 22);
            btnImportLogic.Text = "Import Logic";
            btnImportLogic.Click += btnImportLogic_Click;
            // 
            // btnImportPicture
            // 
            btnImportPicture.Image = (System.Drawing.Image)resources.GetObject("btnImportPicture.Image");
            btnImportPicture.Name = "btnImportPicture";
            btnImportPicture.Size = new System.Drawing.Size(150, 22);
            btnImportPicture.Text = "Import Picture";
            btnImportPicture.Click += btnImportPicture_Click;
            // 
            // btnImportSound
            // 
            btnImportSound.Image = (System.Drawing.Image)resources.GetObject("btnImportSound.Image");
            btnImportSound.Name = "btnImportSound";
            btnImportSound.Size = new System.Drawing.Size(150, 22);
            btnImportSound.Text = "Import Sound";
            btnImportSound.Click += btnImportSound_Click;
            // 
            // btnImportView
            // 
            btnImportView.Image = (System.Drawing.Image)resources.GetObject("btnImportView.Image");
            btnImportView.Name = "btnImportView";
            btnImportView.Size = new System.Drawing.Size(150, 22);
            btnImportView.Text = "Import View";
            btnImportView.Click += btnImportView_Click;
            // 
            // btnSep2
            // 
            btnSep2.Margin = new Padding(8, 0, 8, 0);
            btnSep2.Name = "btnSep2";
            btnSep2.Size = new System.Drawing.Size(6, 34);
            // 
            // btnWords
            // 
            btnWords.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnWords.Image = (System.Drawing.Image)resources.GetObject("btnWords.Image");
            btnWords.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnWords.Margin = new Padding(2, 2, 2, 4);
            btnWords.Name = "btnWords";
            btnWords.Size = new System.Drawing.Size(28, 28);
            btnWords.Text = "WORDS.TOK";
            btnWords.Click += btnWords_Click;
            // 
            // btnOjects
            // 
            btnOjects.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnOjects.Image = (System.Drawing.Image)resources.GetObject("btnOjects.Image");
            btnOjects.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnOjects.Margin = new Padding(2, 2, 2, 4);
            btnOjects.Name = "btnOjects";
            btnOjects.Size = new System.Drawing.Size(28, 28);
            btnOjects.Text = "OBJECT File";
            btnOjects.Click += btnOjects_Click;
            // 
            // btnSep3
            // 
            btnSep3.Margin = new Padding(8, 0, 8, 0);
            btnSep3.Name = "btnSep3";
            btnSep3.Size = new System.Drawing.Size(6, 34);
            // 
            // btnSaveResource
            // 
            btnSaveResource.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnSaveResource.Enabled = false;
            btnSaveResource.Image = (System.Drawing.Image)resources.GetObject("btnSaveResource.Image");
            btnSaveResource.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnSaveResource.Margin = new Padding(2, 2, 2, 4);
            btnSaveResource.Name = "btnSaveResource";
            btnSaveResource.Size = new System.Drawing.Size(28, 28);
            btnSaveResource.Text = "Save Resource";
            btnSaveResource.Click += btnSaveResource_Click;
            // 
            // btnAddRemove
            // 
            btnAddRemove.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnAddRemove.Enabled = false;
            btnAddRemove.Image = (System.Drawing.Image)resources.GetObject("btnAddRemove.Image");
            btnAddRemove.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnAddRemove.Margin = new Padding(2, 2, 2, 4);
            btnAddRemove.Name = "btnAddRemove";
            btnAddRemove.Size = new System.Drawing.Size(28, 28);
            btnAddRemove.Text = "Add/Remove Resource";
            // 
            // btnExportRes
            // 
            btnExportRes.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnExportRes.Enabled = false;
            btnExportRes.Image = (System.Drawing.Image)resources.GetObject("btnExportRes.Image");
            btnExportRes.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnExportRes.Margin = new Padding(2, 2, 2, 4);
            btnExportRes.Name = "btnExportRes";
            btnExportRes.Size = new System.Drawing.Size(28, 28);
            btnExportRes.Text = "Export Resource";
            // 
            // btnSep4
            // 
            btnSep4.Margin = new Padding(8, 0, 8, 0);
            btnSep4.Name = "btnSep4";
            btnSep4.Size = new System.Drawing.Size(6, 34);
            // 
            // btnLayoutEd
            // 
            btnLayoutEd.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnLayoutEd.Enabled = false;
            btnLayoutEd.Image = (System.Drawing.Image)resources.GetObject("btnLayoutEd.Image");
            btnLayoutEd.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnLayoutEd.Margin = new Padding(2, 2, 2, 4);
            btnLayoutEd.Name = "btnLayoutEd";
            btnLayoutEd.Size = new System.Drawing.Size(28, 28);
            btnLayoutEd.Text = "Layout Editor";
            // 
            // btnMenuEd
            // 
            btnMenuEd.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnMenuEd.Image = (System.Drawing.Image)resources.GetObject("btnMenuEd.Image");
            btnMenuEd.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnMenuEd.Margin = new Padding(2, 2, 2, 4);
            btnMenuEd.Name = "btnMenuEd";
            btnMenuEd.Size = new System.Drawing.Size(28, 28);
            btnMenuEd.Text = "Menu Editor";
            // 
            // btnTextEd
            // 
            btnTextEd.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnTextEd.Image = (System.Drawing.Image)resources.GetObject("btnTextEd.Image");
            btnTextEd.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnTextEd.Name = "btnTextEd";
            btnTextEd.Size = new System.Drawing.Size(28, 31);
            btnTextEd.Text = "Text Mode Editor";
            // 
            // btnGlobals
            // 
            btnGlobals.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnGlobals.Image = (System.Drawing.Image)resources.GetObject("btnGlobals.Image");
            btnGlobals.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnGlobals.Margin = new Padding(2, 2, 2, 4);
            btnGlobals.Name = "btnGlobals";
            btnGlobals.Size = new System.Drawing.Size(28, 28);
            btnGlobals.Text = "Global Defines";
            // 
            // btnSep5
            // 
            btnSep5.Margin = new Padding(8, 0, 8, 0);
            btnSep5.Name = "btnSep5";
            btnSep5.Size = new System.Drawing.Size(6, 34);
            // 
            // btnHelp
            // 
            btnHelp.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnHelp.Image = (System.Drawing.Image)resources.GetObject("btnHelp.Image");
            btnHelp.ImageTransparentColor = System.Drawing.Color.FromArgb(236, 233, 216);
            btnHelp.Margin = new Padding(2, 2, 2, 4);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new System.Drawing.Size(28, 28);
            btnHelp.Text = "Help";
            // 
            // toolStripSplitButton2
            // 
            toolStripSplitButton2.Image = (System.Drawing.Image)resources.GetObject("toolStripSplitButton2.Image");
            toolStripSplitButton2.Name = "toolStripSplitButton2";
            toolStripSplitButton2.Size = new System.Drawing.Size(180, 22);
            toolStripSplitButton2.Text = "toolStripMenuItem3";
            // 
            // toolStripSplitButton3
            // 
            toolStripSplitButton3.Image = (System.Drawing.Image)resources.GetObject("toolStripSplitButton3.Image");
            toolStripSplitButton3.Name = "toolStripSplitButton3";
            toolStripSplitButton3.Size = new System.Drawing.Size(180, 22);
            toolStripSplitButton3.Text = "toolStripMenuItem4";
            // 
            // toolStripSplitButton4
            // 
            toolStripSplitButton4.Image = (System.Drawing.Image)resources.GetObject("toolStripSplitButton4.Image");
            toolStripSplitButton4.Name = "toolStripSplitButton4";
            toolStripSplitButton4.Size = new System.Drawing.Size(180, 22);
            toolStripSplitButton4.Text = "toolStripMenuItem5";
            // 
            // pnlResources
            // 
            pnlResources.Controls.Add(splResource);
            pnlResources.Dock = DockStyle.Left;
            pnlResources.Location = new System.Drawing.Point(0, 60);
            pnlResources.Margin = new Padding(1, 0, 1, 0);
            pnlResources.Name = "pnlResources";
            pnlResources.Size = new System.Drawing.Size(157, 295);
            pnlResources.TabIndex = 16;
            // 
            // splResource
            // 
            splResource.Dock = DockStyle.Fill;
            splResource.FixedPanel = FixedPanel.Panel2;
            splResource.Location = new System.Drawing.Point(0, 0);
            splResource.Margin = new Padding(2);
            splResource.Name = "splResource";
            splResource.Orientation = Orientation.Horizontal;
            // 
            // splResource.Panel1
            // 
            splResource.Panel1.Controls.Add(tvwResources);
            splResource.Panel1.Controls.Add(cmbResType);
            splResource.Panel1.Controls.Add(lstResources);
            splResource.Panel1.Controls.Add(cmdBack);
            splResource.Panel1.Controls.Add(cmdForward);
            splResource.Panel1.Resize += splResource_Panel1_Resize;
            // 
            // splResource.Panel2
            // 
            splResource.Panel2.Controls.Add(propertyGrid1);
            splResource.Size = new System.Drawing.Size(157, 295);
            splResource.SplitterDistance = 145;
            splResource.TabIndex = 0;
            splResource.TabStop = false;
            splResource.SplitterMoved += splResource_SplitterMoved;
            splResource.SizeChanged += splResource_SizeChanged;
            // 
            // tvwResources
            // 
            tvwResources.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hlpWinAGI.SetHelpKeyword(tvwResources, "htm\\winagi\\restree.htm#restree");
            hlpWinAGI.SetHelpNavigator(tvwResources, HelpNavigator.Topic);
            hlpWinAGI.SetHelpString(tvwResources, "");
            tvwResources.HideSelection = false;
            tvwResources.Location = new System.Drawing.Point(0, 26);
            tvwResources.Margin = new Padding(2, 1, 2, 1);
            tvwResources.Name = "tvwResources";
            treeNode8.Name = "logics";
            treeNode8.Text = "Logics";
            treeNode9.Name = "pictures";
            treeNode9.Text = "Pictures";
            treeNode10.Name = "sounds";
            treeNode10.Text = "Sounds";
            treeNode11.Name = "views";
            treeNode11.Text = "Views";
            treeNode12.Name = "objects";
            treeNode12.Text = "Objects";
            treeNode13.Name = "words";
            treeNode13.Text = "Words";
            treeNode14.Name = "Node0";
            treeNode14.Text = "AGIGAME";
            tvwResources.Nodes.AddRange(new TreeNode[] { treeNode14 });
            hlpWinAGI.SetShowHelp(tvwResources, true);
            tvwResources.Size = new System.Drawing.Size(157, 118);
            tvwResources.TabIndex = 25;
            tvwResources.AfterCollapse += tvwResources_AfterCollapse;
            tvwResources.AfterSelect += tvwResources_AfterSelect;
            tvwResources.NodeMouseClick += tvwResources_NodeMouseClick;
            tvwResources.NodeMouseDoubleClick += tvwResources_NodeMouseDoubleClick;
            tvwResources.MouseDown += tvwResources_MouseDown;
            // 
            // cmbResType
            // 
            cmbResType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbResType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbResType.FormattingEnabled = true;
            cmbResType.Items.AddRange(new object[] { "agi", "LOGICS", "PICTURES", "SOUNDS", "VIEWS", "OBJECT", "WORDS.TOK" });
            cmbResType.Location = new System.Drawing.Point(0, 26);
            cmbResType.Margin = new Padding(2, 1, 2, 1);
            cmbResType.Name = "cmbResType";
            cmbResType.Size = new System.Drawing.Size(156, 23);
            cmbResType.TabIndex = 26;
            cmbResType.Visible = false;
            cmbResType.SelectedIndexChanged += cmbResType_SelectedIndexChanged;
            // 
            // lstResources
            // 
            lstResources.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstResources.BorderStyle = BorderStyle.FixedSingle;
            lstResources.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            lstResources.FullRowSelect = true;
            lstResources.HeaderStyle = ColumnHeaderStyle.None;
            lstResources.Location = new System.Drawing.Point(0, 52);
            lstResources.Margin = new Padding(2, 1, 2, 1);
            lstResources.MultiSelect = false;
            lstResources.Name = "lstResources";
            lstResources.ShowGroups = false;
            lstResources.Size = new System.Drawing.Size(157, 86);
            lstResources.TabIndex = 27;
            lstResources.UseCompatibleStateImageBehavior = false;
            lstResources.View = View.Details;
            lstResources.SelectedIndexChanged += lstResources_SelectedIndexChanged;
            lstResources.SizeChanged += lstResources_SizeChanged;
            lstResources.DoubleClick += lstResources_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "";
            // 
            // cmdBack
            // 
            cmdBack.Image = (System.Drawing.Image)resources.GetObject("cmdBack.Image");
            cmdBack.Location = new System.Drawing.Point(0, 0);
            cmdBack.Name = "cmdBack";
            cmdBack.Size = new System.Drawing.Size(80, 26);
            cmdBack.TabIndex = 2;
            cmdBack.UseVisualStyleBackColor = true;
            cmdBack.Click += cmdBack_Click;
            cmdBack.MouseDown += cmdBack_MouseDown;
            // 
            // cmdForward
            // 
            cmdForward.Image = (System.Drawing.Image)resources.GetObject("cmdForward.Image");
            cmdForward.Location = new System.Drawing.Point(80, 0);
            cmdForward.Name = "cmdForward";
            cmdForward.Size = new System.Drawing.Size(80, 26);
            cmdForward.TabIndex = 3;
            cmdForward.UseVisualStyleBackColor = true;
            cmdForward.Click += cmdForward_Click;
            cmdForward.MouseDown += cmdForward_MouseDown;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGrid1.CommandsVisibleIfAvailable = false;
            propertyGrid1.HelpVisible = false;
            propertyGrid1.Location = new System.Drawing.Point(0, 2);
            propertyGrid1.Margin = new Padding(2);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.PropertySort = PropertySort.NoSort;
            propertyGrid1.Size = new System.Drawing.Size(156, 144);
            propertyGrid1.TabIndex = 28;
            propertyGrid1.ToolbarVisible = false;
            propertyGrid1.MouseWheel += propertyGrid1_MouseWheel;
            // 
            // cmsGrid
            // 
            cmsGrid.Items.AddRange(new ToolStripItem[] { cmiDismiss, cmiDismissAll, cmiErrorHelp });
            cmsGrid.Name = "cmsGrid";
            cmsGrid.Size = new System.Drawing.Size(176, 70);
            // 
            // cmiDismiss
            // 
            cmiDismiss.Name = "cmiDismiss";
            cmiDismiss.Size = new System.Drawing.Size(175, 22);
            cmiDismiss.Text = "Dismiss";
            // 
            // cmiDismissAll
            // 
            cmiDismissAll.Name = "cmiDismissAll";
            cmiDismissAll.Size = new System.Drawing.Size(175, 22);
            cmiDismissAll.Text = "Dismiss All";
            // 
            // cmiErrorHelp
            // 
            cmiErrorHelp.Name = "cmiErrorHelp";
            cmiErrorHelp.Size = new System.Drawing.Size(175, 22);
            cmiErrorHelp.Text = "Help with this Error";
            // 
            // picNavList
            // 
            picNavList.BackColor = System.Drawing.SystemColors.Window;
            picNavList.Location = new System.Drawing.Point(360, 66);
            picNavList.Margin = new Padding(2);
            picNavList.Name = "picNavList";
            picNavList.Size = new System.Drawing.Size(74, 78);
            picNavList.TabIndex = 24;
            picNavList.TabStop = false;
            picNavList.Visible = false;
            picNavList.Paint += picNavList_Paint;
            picNavList.MouseMove += picNavList_MouseMove;
            picNavList.MouseUp += picNavList_MouseUp;
            // 
            // splitResource
            // 
            splitResource.Location = new System.Drawing.Point(157, 60);
            splitResource.Margin = new Padding(1, 0, 1, 0);
            splitResource.Name = "splitResource";
            splitResource.Size = new System.Drawing.Size(2, 295);
            splitResource.TabIndex = 18;
            splitResource.TabStop = false;
            splitResource.Visible = false;
            // 
            // pnlWarnings
            // 
            pnlWarnings.Controls.Add(fgWarnings);
            pnlWarnings.Dock = DockStyle.Bottom;
            pnlWarnings.Location = new System.Drawing.Point(159, 269);
            pnlWarnings.Margin = new Padding(1, 0, 1, 0);
            pnlWarnings.Name = "pnlWarnings";
            pnlWarnings.Size = new System.Drawing.Size(1123, 86);
            pnlWarnings.TabIndex = 20;
            // 
            // fgWarnings
            // 
            fgWarnings.AllowUserToAddRows = false;
            fgWarnings.AllowUserToDeleteRows = false;
            fgWarnings.AllowUserToResizeRows = false;
            fgWarnings.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ActiveCaption;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            fgWarnings.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            fgWarnings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            fgWarnings.Columns.AddRange(new DataGridViewColumn[] { colWarning, colDesc, colResNum, colLIne, colModule });
            fgWarnings.ContextMenuStrip = cmsGrid;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            fgWarnings.DefaultCellStyle = dataGridViewCellStyle4;
            fgWarnings.Dock = DockStyle.Fill;
            fgWarnings.EditMode = DataGridViewEditMode.EditOnEnter;
            fgWarnings.Location = new System.Drawing.Point(0, 0);
            fgWarnings.Margin = new Padding(2);
            fgWarnings.Name = "fgWarnings";
            fgWarnings.ReadOnly = true;
            fgWarnings.RowHeadersVisible = false;
            fgWarnings.RowHeadersWidth = 82;
            fgWarnings.RowTemplate.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            fgWarnings.RowTemplate.Height = 28;
            fgWarnings.RowTemplate.ReadOnly = true;
            fgWarnings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            fgWarnings.ShowCellErrors = false;
            fgWarnings.ShowCellToolTips = false;
            fgWarnings.ShowEditingIcon = false;
            fgWarnings.ShowRowErrors = false;
            fgWarnings.Size = new System.Drawing.Size(1123, 86);
            fgWarnings.StandardTab = true;
            fgWarnings.TabIndex = 0;
            fgWarnings.ColumnHeaderMouseClick += fgWarnings_ColumnHeaderMouseClick;
            fgWarnings.SortCompare += fgWarnings_SortCompare;
            fgWarnings.Sorted += fgWarnings_Sorted;
            // 
            // colWarning
            // 
            colWarning.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colWarning.FillWeight = 20F;
            colWarning.HeaderText = "Warning";
            colWarning.MinimumWidth = 10;
            colWarning.Name = "colWarning";
            colWarning.ReadOnly = true;
            // 
            // colDesc
            // 
            colDesc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDesc.FillWeight = 50F;
            colDesc.HeaderText = "Description";
            colDesc.MinimumWidth = 10;
            colDesc.Name = "colDesc";
            colDesc.ReadOnly = true;
            // 
            // colResNum
            // 
            colResNum.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colResNum.FillWeight = 10F;
            colResNum.HeaderText = "Res#";
            colResNum.MinimumWidth = 10;
            colResNum.Name = "colResNum";
            colResNum.ReadOnly = true;
            // 
            // colLIne
            // 
            colLIne.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colLIne.FillWeight = 10F;
            colLIne.HeaderText = "Line#";
            colLIne.MinimumWidth = 10;
            colLIne.Name = "colLIne";
            colLIne.ReadOnly = true;
            // 
            // colModule
            // 
            colModule.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colModule.FillWeight = 20F;
            colModule.HeaderText = "Module";
            colModule.MinimumWidth = 10;
            colModule.Name = "colModule";
            colModule.ReadOnly = true;
            // 
            // splitWarning
            // 
            splitWarning.Dock = DockStyle.Bottom;
            splitWarning.Location = new System.Drawing.Point(159, 267);
            splitWarning.Margin = new Padding(1, 0, 1, 0);
            splitWarning.Name = "splitWarning";
            splitWarning.Size = new System.Drawing.Size(1123, 2);
            splitWarning.TabIndex = 22;
            splitWarning.TabStop = false;
            splitWarning.Visible = false;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth8Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "opengame");
            imageList1.Images.SetKeyName(1, "closegame");
            imageList1.Images.SetKeyName(2, "rungame");
            imageList1.Images.SetKeyName(3, "print");
            imageList1.Images.SetKeyName(4, "newlogic");
            imageList1.Images.SetKeyName(5, "newpicture");
            imageList1.Images.SetKeyName(6, "newsound");
            imageList1.Images.SetKeyName(7, "newview");
            imageList1.Images.SetKeyName(8, "openlogic");
            imageList1.Images.SetKeyName(9, "openpicture");
            imageList1.Images.SetKeyName(10, "opensound");
            imageList1.Images.SetKeyName(11, "openview");
            imageList1.Images.SetKeyName(12, "importlogic");
            imageList1.Images.SetKeyName(13, "importpicture");
            imageList1.Images.SetKeyName(14, "importsound");
            imageList1.Images.SetKeyName(15, "importview");
            imageList1.Images.SetKeyName(16, "words");
            imageList1.Images.SetKeyName(17, "objects");
            imageList1.Images.SetKeyName(18, "saveres");
            imageList1.Images.SetKeyName(19, "addres");
            imageList1.Images.SetKeyName(20, "removeres");
            imageList1.Images.SetKeyName(21, "exportres");
            imageList1.Images.SetKeyName(22, "layout");
            imageList1.Images.SetKeyName(23, "menu");
            imageList1.Images.SetKeyName(24, "globals");
            imageList1.Images.SetKeyName(25, "help");
            // 
            // tmrNavList
            // 
            tmrNavList.Interval = 125;
            tmrNavList.Tick += SplashTimer;
            // 
            // FolderDlg
            // 
            FolderDlg.ShowNewFolderButton = false;
            // 
            // imlPropButtons
            // 
            imlPropButtons.ColorDepth = ColorDepth.Depth8Bit;
            imlPropButtons.ImageStream = (ImageListStreamer)resources.GetObject("imlPropButtons.ImageStream");
            imlPropButtons.TransparentColor = System.Drawing.Color.Transparent;
            imlPropButtons.Images.SetKeyName(0, "dropdown_u.bmp");
            imlPropButtons.Images.SetKeyName(1, "dropover_u.bmp");
            imlPropButtons.Images.SetKeyName(2, "dropdialog_u.bmp");
            imlPropButtons.Images.SetKeyName(3, "dropdown_d.bmp");
            imlPropButtons.Images.SetKeyName(4, "dropover_d.bmp");
            imlPropButtons.Images.SetKeyName(5, "dropdialog_d.bmp");
            // 
            // hlpWinAGI
            // 
            hlpWinAGI.HelpNamespace = "C:\\Users\\Andy\\OneDrive\\AGI Stuff\\WinAGI GDS Files\\Visual Studio Projects\\WinAGI\\WinAGI GDS\\WinAGI.chm";
            // 
            // frmMDIMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(1282, 378);
            Controls.Add(picNavList);
            Controls.Add(splitWarning);
            Controls.Add(pnlWarnings);
            Controls.Add(splitResource);
            Controls.Add(pnlResources);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            IsMdiContainer = true;
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "frmMDIMain";
            Text = "WinAGI GDS";
            FormClosing += frmMDIMain_FormClosing;
            Load += frmMDIMain_Load;
            KeyDown += frmMDIMain_KeyDown;
            KeyPress += frmMDIMain_KeyPress;
            PreviewKeyDown += frmMDIMain_PreviewKeyDown;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            pnlResources.ResumeLayout(false);
            splResource.Panel1.ResumeLayout(false);
            splResource.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splResource).EndInit();
            splResource.ResumeLayout(false);
            cmsGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picNavList).EndInit();
            pnlWarnings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)fgWarnings).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripStatusLabel CapsLockLabel;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem mnuROpen;
        private ToolStripSeparator mnuRSeparator1;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripSeparator mnuRSeparator2;
        private ToolStripMenuItem printToolStripMenuItem;
        private ToolStripMenuItem mnuR;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem customizeToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem mnuHelp;
        private ToolStripMenuItem contentsToolStripMenuItem;
        private ToolStripMenuItem indexToolStripMenuItem;
        private ToolStripMenuItem searchToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripButton btnOpen;
        private ToolStripButton btnClose;
        private ToolStripButton saveToolStripButton;
        private ToolStripButton printToolStripButton;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripButton cutToolStripButton;
        private ToolStripButton btnOject;
        private ToolStripSeparator btnSep3;
        private ToolStripButton helpToolStripButton;
        private ToolStripMenuItem mnuGNew;
        private ToolStripMenuItem mnuGNewTemplate;
        private ToolStripMenuItem mnuGOpen;
        private ToolStripMenuItem mnuGImport;
        private ToolStripSeparator mnuGSep1;
        private ToolStripMenuItem mnuGCompile;
        private ToolStripMenuItem mnuGRun;
        private ToolStripMenuItem mnuGProperties;
        private ToolStripMenuItem mnuGExit;
        private ToolStripMenuItem mnuResources;
        private ToolStripMenuItem mnuWindow;
        private ToolStripMenuItem mnuWCascade;
        private ToolStripMenuItem mnuWTileV;
        private ToolStripMenuItem mnuWTileH;
        private ToolStripMenuItem mnuWArrange;
        private ToolStripMenuItem mnuWMinimize;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem mnuWClose;
        public Splitter splitResource;
        public Splitter splitWarning;
        private ToolStripMenuItem mnuRNew;
        private ToolStripMenuItem mnuRSave;
        private ToolStripMenuItem mnuRExport;
        private ToolStripMenuItem mnuRIDDesc;
        private ToolStripMenuItem mnuRNLogic;
        private ToolStripMenuItem mnuRNPicture;
        private ToolStripMenuItem mnuRNSound;
        private ToolStripMenuItem mnuRNView;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem mnuRNObjects;
        private ToolStripMenuItem mnuRNWords;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem mnuRNText;
        private ToolStripMenuItem mnuROLogic;
        private ToolStripMenuItem mnuROPicture;
        private ToolStripMenuItem mnuROSound;
        private ToolStripMenuItem mnuROView;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripMenuItem mnuROObjects;
        private ToolStripMenuItem mnuROWords;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripMenuItem mnuROText;
        private ToolStripMenuItem mnuRILogic;
        private ToolStripMenuItem mnuRIPicture;
        private ToolStripMenuItem mnuRISound;
        private ToolStripMenuItem mnuRIView;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripMenuItem mnuRIObjects;
        private ToolStripMenuItem mnuRIWords;
        private ToolStripSeparator mnuGSep2;
        private ToolStripSeparator mnuGSep3;
        private ToolStripSeparator mnuGSep5;
        private ToolStripMenuItem mnuTSettings;
        private ToolStripSeparator mnuTSep1;
        private ToolStripMenuItem mnuTMenuEditor;
        private ToolStripMenuItem mnuReserved;
        private ToolStripMenuItem mnuTSnippets;
        private ToolStripMenuItem mnuTPalette;
        private ToolStripMenuItem mnuTCustom1;
        private ToolStripMenuItem mnuTCustom2;
        private ToolStripMenuItem mnuTCustom3;
        private ToolStripMenuItem mnuTCustom4;
        private ToolStripMenuItem mnuTCustom5;
        private ToolStripMenuItem mnuTCustom6;
        private ToolStripSeparator mnuTSep3;
        private ToolStripMenuItem mnuTCustomize;
        private ToolStripMenuItem mnuHContents;
        private ToolStripMenuItem mnuHIndex;
        private ToolStripSeparator mnuHSep1;
        private ToolStripMenuItem mnuHCommands;
        private ToolStripMenuItem mnuHReference;
        private ToolStripSeparator mnuHSep2;
        private ToolStripMenuItem mnuHAbout;
        private ImageList imageList1;
        private ToolStripButton btnOpenGame;
        private ToolStripButton btnCloseGame;
        private ToolStripButton btnRun;
        private ToolStripSeparator btnSep1;
        private ToolStripSplitButton btnNewRes;
        private ToolStripSplitButton btnOpenRes;
        private ToolStripMenuItem btnOpenLogic;
        private ToolStripMenuItem btnOpenPicture;
        private ToolStripMenuItem btnOpenSound;
        private ToolStripMenuItem btnOpenView;
        private ToolStripSplitButton btnImportRes;
        private ToolStripSeparator btnSep2;
        private ToolStripMenuItem btnImportLogic;
        private ToolStripMenuItem btnImportPicture;
        private ToolStripMenuItem btnImportSound;
        private ToolStripMenuItem btnImportView;
        private ToolStripButton btnWords;
        private ToolStripButton btnOjects;
        private ToolStripButton btnSaveResource;
        private ToolStripButton btnAddRemove;
        private ToolStripButton btnExportRes;
        private ToolStripSeparator btnSep4;
        private ToolStripButton btnLayoutEd;
        private ToolStripButton btnMenuEd;
        private ToolStripButton btnGlobals;
        private ToolStripSeparator btnSep5;
        private ToolStripButton btnHelp;
        private ToolStripMenuItem btnNewLogic;
        private ToolStripMenuItem btnNewPicture;
        private ToolStripMenuItem btnNewSound;
        private ToolStripMenuItem btnNewView;
        private Timer tmrNavList;
        private ToolStripSplitButton toolStripSplitButton2;
        private ToolStripSplitButton toolStripSplitButton3;
        private ToolStripSplitButton toolStripSplitButton4;
        public Button cmdForward;
        public Button cmdBack;
        internal Panel pnlResources;
        public OpenFileDialog OpenDlg;
        public SaveFileDialog SaveDlg;
        public Panel pnlWarnings;
        public ToolStripSeparator mnuGMRUBar;
        public ToolStripMenuItem mnuGMRU0;
        public ToolStripMenuItem mnuGMRU1;
        public ToolStripMenuItem mnuGMRU2;
        public ToolStripMenuItem mnuGMRU3;
        public ToolStripMenuItem mnuGame;
        public ImageList imlPropButtons;
        public ToolStripMenuItem mnuRAddRemove;
        public ToolStripMenuItem mnuGCompileTo;
        public ToolStripMenuItem mnuGCompileDirty;
        public ToolStripMenuItem mnuGNewBlank;
        public ToolStripMenuItem mnuGRebuild;
        public ToolStripMenuItem mnuRImport;
        public ToolStripMenuItem mnuGClose;
        public ToolStripMenuItem mnuRRenumber;
        private ToolStripStatusLabel Label1;
        private ToolStripStatusLabel Label2;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel springLabel;
        private ToolStripStatusLabel NumLockLabel;
        private ToolStripStatusLabel InsertLockLabel;
        private ToolStripStatusLabel StatusPanel1;
        private DataGridView fgWarnings;
        private PictureBox picNavList;
        public FolderBrowserDialog FolderDlg;
        private SplitContainer splResource;
        public TreeView tvwResources;
        public ListView lstResources;
        public ComboBox cmbResType;
        private SplitContainer splitContainer1;
        private PropertyGrid propertyGrid1;
        private ColumnHeader columnHeader1;
        private DataGridViewTextBoxColumn colWarning;
        private DataGridViewTextBoxColumn colDesc;
        private DataGridViewTextBoxColumn colResNum;
        private DataGridViewTextBoxColumn colLIne;
        private DataGridViewTextBoxColumn colModule;
        private ToolStripMenuItem mnuROpenRes;
        private ToolStripSeparator mnuRSeparator3;
        private ToolStripMenuItem mnuRSavePicImage;
        private ToolStripMenuItem mnuRExportGIF;
        private ToolStripMenuItem mnuRCompileLogic;
        private ContextMenuStrip cmsGrid;
        private ToolStripMenuItem cmiDismiss;
        private ToolStripMenuItem cmiDismissAll;
        private ToolStripMenuItem cmiErrorHelp;
        public HelpProvider hlpWinAGI;
        internal ToolStripMenuItem mnuTools;
        internal ToolStripSeparator mnuTSep2;
        internal ToolStrip toolStrip1;
        private ToolStripButton btnTextEd;
        internal ToolStripMenuItem mnuTLayout;
        internal ToolStripMenuItem mnuTGlobals;
        public StatusStrip statusStrip1;
    }
}

