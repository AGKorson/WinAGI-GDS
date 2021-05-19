﻿namespace WinAGI.Editor
{
  using System.Windows.Forms;
  partial class frmMDIMain
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMDIMain));
      System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Logics");
      System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Pictures");
      System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Sounds");
      System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Views");
      System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Objects");
      System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Words");
      System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("AGIGAME", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6});
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.mnuGame = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGNew = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGNewTemplate = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGNewBlank = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGImport = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGClose = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGSep1 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuGCompile = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGCompileTo = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGRebuild = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGCompileDirty = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGSep2 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuGRun = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGSep3 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuGProperties = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGMRUBar = new System.Windows.Forms.ToolStripSeparator();
      this.mnuGMRU0 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGMRU1 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGMRU2 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGMRU3 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuGSep5 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuGExit = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuResources = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRNew = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRNLogic = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRNPicture = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRNSound = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRNView = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuRNObjects = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRNWords = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuRNText = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuROpen = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuROLogic = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuROPicture = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuROSound = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuROView = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuROObjects = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuROWords = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuROText = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRImport = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRILogic = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRIPicture = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRISound = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRIView = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuRIObjects = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRIWords = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuRSave = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRExport = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuRAddRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRRenumber = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuRIDDesc = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuRPrint = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTools = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTSettings = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTSep1 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuTLayout = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTMenuEditor = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTGlobals = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuReserved = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTSnippets = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTPalette = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTSep2 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuTCustom1 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTCustom2 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTCustom3 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTCustom4 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTCustom5 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTCustom6 = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuTSep3 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuTCustomize = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuWindow = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuWCascade = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuWTileV = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuWTileH = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuWArrange = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuWMinimize = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuWClose = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuHContents = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuHIndex = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuHSep1 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuHCommands = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuHReference = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuHSep2 = new System.Windows.Forms.ToolStripSeparator();
      this.mnuHAbout = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.btnNewRes = new System.Windows.Forms.ToolStripSplitButton();
      this.btnNewLogic = new System.Windows.Forms.ToolStripMenuItem();
      this.btnNewPicture = new System.Windows.Forms.ToolStripMenuItem();
      this.btnNewSound = new System.Windows.Forms.ToolStripMenuItem();
      this.btnNewView = new System.Windows.Forms.ToolStripMenuItem();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.StatusPanel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.springLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.CapsLockLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.NumLockLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.InsertLockLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.btnOpenGame = new System.Windows.Forms.ToolStripButton();
      this.btnCloseGame = new System.Windows.Forms.ToolStripButton();
      this.btnRun = new System.Windows.Forms.ToolStripButton();
      this.btnSep1 = new System.Windows.Forms.ToolStripSeparator();
      this.btnPrint = new System.Windows.Forms.ToolStripButton();
      this.btnOpenRes = new System.Windows.Forms.ToolStripSplitButton();
      this.btnOpenLogic = new System.Windows.Forms.ToolStripMenuItem();
      this.btnOpenPicture = new System.Windows.Forms.ToolStripMenuItem();
      this.btnOpenSound = new System.Windows.Forms.ToolStripMenuItem();
      this.btnOpenView = new System.Windows.Forms.ToolStripMenuItem();
      this.btnImportRes = new System.Windows.Forms.ToolStripSplitButton();
      this.btnImportLogic = new System.Windows.Forms.ToolStripMenuItem();
      this.btnImportPicture = new System.Windows.Forms.ToolStripMenuItem();
      this.btnImportSound = new System.Windows.Forms.ToolStripMenuItem();
      this.btnImportView = new System.Windows.Forms.ToolStripMenuItem();
      this.btnSep2 = new System.Windows.Forms.ToolStripSeparator();
      this.btnWords = new System.Windows.Forms.ToolStripButton();
      this.btnOjects = new System.Windows.Forms.ToolStripButton();
      this.btnSep3 = new System.Windows.Forms.ToolStripSeparator();
      this.btnSaveResource = new System.Windows.Forms.ToolStripButton();
      this.btnAddRemove = new System.Windows.Forms.ToolStripButton();
      this.btnExportRes = new System.Windows.Forms.ToolStripButton();
      this.btnSep4 = new System.Windows.Forms.ToolStripSeparator();
      this.btnLayoutEd = new System.Windows.Forms.ToolStripButton();
      this.btnMenuEd = new System.Windows.Forms.ToolStripButton();
      this.btnGlobals = new System.Windows.Forms.ToolStripButton();
      this.btnSep5 = new System.Windows.Forms.ToolStripSeparator();
      this.btnHelp = new System.Windows.Forms.ToolStripButton();
      this.toolStripSplitButton2 = new System.Windows.Forms.ToolStripSplitButton();
      this.toolStripSplitButton3 = new System.Windows.Forms.ToolStripSplitButton();
      this.toolStripSplitButton4 = new System.Windows.Forms.ToolStripSplitButton();
      this.pnlResources = new System.Windows.Forms.Panel();
      this.splResource = new System.Windows.Forms.SplitContainer();
      this.tvwResources = new System.Windows.Forms.TreeView();
      this.cmdBack = new System.Windows.Forms.Button();
      this.cmdForward = new System.Windows.Forms.Button();
      this.lstResources = new System.Windows.Forms.ListView();
      this.cmbResType = new System.Windows.Forms.ComboBox();
      this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
      this.picProperties = new System.Windows.Forms.PictureBox();
      this.fsbProperty = new System.Windows.Forms.VScrollBar();
      this.picNavList = new System.Windows.Forms.PictureBox();
      this.lstProperty = new System.Windows.Forms.ListBox();
      this.splitResource = new System.Windows.Forms.Splitter();
      this.pnlWarnings = new System.Windows.Forms.Panel();
      this.fgWarnings = new System.Windows.Forms.DataGridView();
      this.colResType = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colIDK = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colWarning = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colLogic = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colLIne = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colModule = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.splitWarning = new System.Windows.Forms.Splitter();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.tmrNavList = new System.Windows.Forms.Timer(this.components);
      this.FolderDlg = new System.Windows.Forms.FolderBrowserDialog();
      this.OpenDlg = new System.Windows.Forms.OpenFileDialog();
      this.SaveDlg = new System.Windows.Forms.SaveFileDialog();
      this.imlPropButtons = new System.Windows.Forms.ImageList(this.components);
      this.menuStrip1.SuspendLayout();
      this.contextMenuStrip1.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      this.pnlResources.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splResource)).BeginInit();
      this.splResource.Panel1.SuspendLayout();
      this.splResource.Panel2.SuspendLayout();
      this.splResource.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picProperties)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.picNavList)).BeginInit();
      this.pnlWarnings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.fgWarnings)).BeginInit();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGame,
            this.mnuResources,
            this.mnuTools,
            this.mnuWindow,
            this.mnuHelp});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.MdiWindowListItem = this.mnuWindow;
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new System.Drawing.Size(642, 24);
      this.menuStrip1.TabIndex = 0;
      this.menuStrip1.Text = "menuStrip1";
      this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
      // 
      // mnuGame
      // 
      this.mnuGame.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGNew,
            this.mnuGOpen,
            this.mnuGImport,
            this.mnuGClose,
            this.mnuGSep1,
            this.mnuGCompile,
            this.mnuGCompileTo,
            this.mnuGRebuild,
            this.mnuGCompileDirty,
            this.mnuGSep2,
            this.mnuGRun,
            this.mnuGSep3,
            this.mnuGProperties,
            this.mnuGMRUBar,
            this.mnuGMRU0,
            this.mnuGMRU1,
            this.mnuGMRU2,
            this.mnuGMRU3,
            this.mnuGSep5,
            this.mnuGExit});
      this.mnuGame.Name = "mnuGame";
      this.mnuGame.Size = new System.Drawing.Size(50, 20);
      this.mnuGame.Text = "&Game";
      // 
      // mnuGNew
      // 
      this.mnuGNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGNewTemplate,
            this.mnuGNewBlank});
      this.mnuGNew.Name = "mnuGNew";
      this.mnuGNew.Size = new System.Drawing.Size(261, 22);
      this.mnuGNew.Text = "&New Game";
      // 
      // mnuGNewTemplate
      // 
      this.mnuGNewTemplate.Name = "mnuGNewTemplate";
      this.mnuGNewTemplate.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
      this.mnuGNewTemplate.Size = new System.Drawing.Size(196, 22);
      this.mnuGNewTemplate.Text = "From &Template";
      // 
      // mnuGNewBlank
      // 
      this.mnuGNewBlank.Name = "mnuGNewBlank";
      this.mnuGNewBlank.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
      this.mnuGNewBlank.Size = new System.Drawing.Size(196, 22);
      this.mnuGNewBlank.Text = "&Blank";
      // 
      // mnuGOpen
      // 
      this.mnuGOpen.Name = "mnuGOpen";
      this.mnuGOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.mnuGOpen.Size = new System.Drawing.Size(261, 22);
      this.mnuGOpen.Text = "&Open Game";
      this.mnuGOpen.Click += new System.EventHandler(this.mnuGOpen_Click);
      // 
      // mnuGImport
      // 
      this.mnuGImport.Name = "mnuGImport";
      this.mnuGImport.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
      this.mnuGImport.Size = new System.Drawing.Size(261, 22);
      this.mnuGImport.Text = "&Import Game";
      this.mnuGImport.Click += new System.EventHandler(this.mnuGImport_Click);
      // 
      // mnuGClose
      // 
      this.mnuGClose.Name = "mnuGClose";
      this.mnuGClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
      this.mnuGClose.Size = new System.Drawing.Size(261, 22);
      this.mnuGClose.Text = "C&lose Game";
      this.mnuGClose.Click += new System.EventHandler(this.mnuGClose_Click);
      // 
      // mnuGSep1
      // 
      this.mnuGSep1.Name = "mnuGSep1";
      this.mnuGSep1.Size = new System.Drawing.Size(258, 6);
      // 
      // mnuGCompile
      // 
      this.mnuGCompile.Name = "mnuGCompile";
      this.mnuGCompile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
      this.mnuGCompile.Size = new System.Drawing.Size(261, 22);
      this.mnuGCompile.Text = "&Compile Game";
      // 
      // mnuGCompileTo
      // 
      this.mnuGCompileTo.Name = "mnuGCompileTo";
      this.mnuGCompileTo.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.B)));
      this.mnuGCompileTo.Size = new System.Drawing.Size(261, 22);
      this.mnuGCompileTo.Text = "Compile &To ...";
      // 
      // mnuGRebuild
      // 
      this.mnuGRebuild.Name = "mnuGRebuild";
      this.mnuGRebuild.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
      this.mnuGRebuild.Size = new System.Drawing.Size(261, 22);
      this.mnuGRebuild.Text = "Rebuild &VOL Files";
      // 
      // mnuGCompileDirty
      // 
      this.mnuGCompileDirty.Name = "mnuGCompileDirty";
      this.mnuGCompileDirty.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D)));
      this.mnuGCompileDirty.Size = new System.Drawing.Size(261, 22);
      this.mnuGCompileDirty.Text = "Complile &Dirty Logics";
      // 
      // mnuGSep2
      // 
      this.mnuGSep2.Name = "mnuGSep2";
      this.mnuGSep2.Size = new System.Drawing.Size(258, 6);
      // 
      // mnuGRun
      // 
      this.mnuGRun.Name = "mnuGRun";
      this.mnuGRun.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
      this.mnuGRun.Size = new System.Drawing.Size(261, 22);
      this.mnuGRun.Text = "&Run";
      // 
      // mnuGSep3
      // 
      this.mnuGSep3.Name = "mnuGSep3";
      this.mnuGSep3.Size = new System.Drawing.Size(258, 6);
      // 
      // mnuGProperties
      // 
      this.mnuGProperties.Name = "mnuGProperties";
      this.mnuGProperties.ShortcutKeys = System.Windows.Forms.Keys.F4;
      this.mnuGProperties.Size = new System.Drawing.Size(261, 22);
      this.mnuGProperties.Text = "&Properties ...";
      // 
      // mnuGMRUBar
      // 
      this.mnuGMRUBar.Name = "mnuGMRUBar";
      this.mnuGMRUBar.Size = new System.Drawing.Size(258, 6);
      this.mnuGMRUBar.Visible = false;
      // 
      // mnuGMRU0
      // 
      this.mnuGMRU0.Name = "mnuGMRU0";
      this.mnuGMRU0.Size = new System.Drawing.Size(261, 22);
      this.mnuGMRU0.Tag = "0";
      this.mnuGMRU0.Text = "mru1";
      this.mnuGMRU0.Visible = false;
      this.mnuGMRU0.Click += new System.EventHandler(this.mnuGMRU_Click);
      // 
      // mnuGMRU1
      // 
      this.mnuGMRU1.Name = "mnuGMRU1";
      this.mnuGMRU1.Size = new System.Drawing.Size(261, 22);
      this.mnuGMRU1.Tag = "1";
      this.mnuGMRU1.Text = "mru2";
      this.mnuGMRU1.Visible = false;
      this.mnuGMRU1.Click += new System.EventHandler(this.mnuGMRU_Click);
      // 
      // mnuGMRU2
      // 
      this.mnuGMRU2.Name = "mnuGMRU2";
      this.mnuGMRU2.Size = new System.Drawing.Size(261, 22);
      this.mnuGMRU2.Tag = "2";
      this.mnuGMRU2.Text = "mru3";
      this.mnuGMRU2.Visible = false;
      this.mnuGMRU2.Click += new System.EventHandler(this.mnuGMRU_Click);
      // 
      // mnuGMRU3
      // 
      this.mnuGMRU3.Name = "mnuGMRU3";
      this.mnuGMRU3.Size = new System.Drawing.Size(261, 22);
      this.mnuGMRU3.Tag = "3";
      this.mnuGMRU3.Text = "mru4";
      this.mnuGMRU3.Visible = false;
      this.mnuGMRU3.Click += new System.EventHandler(this.mnuGMRU_Click);
      // 
      // mnuGSep5
      // 
      this.mnuGSep5.Name = "mnuGSep5";
      this.mnuGSep5.Size = new System.Drawing.Size(258, 6);
      // 
      // mnuGExit
      // 
      this.mnuGExit.Name = "mnuGExit";
      this.mnuGExit.ShortcutKeyDisplayString = "Alt+F4";
      this.mnuGExit.Size = new System.Drawing.Size(261, 22);
      this.mnuGExit.Text = "E&xit";
      this.mnuGExit.Click += new System.EventHandler(this.mnuGExit_Click);
      // 
      // mnuResources
      // 
      this.mnuResources.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRNew,
            this.mnuROpen,
            this.mnuRImport,
            this.toolStripSeparator2,
            this.mnuRSave,
            this.mnuRExport,
            this.toolStripSeparator3,
            this.mnuRAddRemove,
            this.mnuRRenumber,
            this.mnuRIDDesc,
            this.toolStripSeparator4,
            this.mnuRPrint});
      this.mnuResources.Name = "mnuResources";
      this.mnuResources.Size = new System.Drawing.Size(72, 20);
      this.mnuResources.Text = "&Resources";
      // 
      // mnuRNew
      // 
      this.mnuRNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRNLogic,
            this.mnuRNPicture,
            this.mnuRNSound,
            this.mnuRNView,
            this.toolStripSeparator6,
            this.mnuRNObjects,
            this.mnuRNWords,
            this.toolStripSeparator5,
            this.mnuRNText});
      this.mnuRNew.Image = ((System.Drawing.Image)(resources.GetObject("mnuRNew.Image")));
      this.mnuRNew.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.mnuRNew.Name = "mnuRNew";
      this.mnuRNew.Size = new System.Drawing.Size(305, 22);
      this.mnuRNew.Text = "&New Resource";
      // 
      // mnuRNLogic
      // 
      this.mnuRNLogic.Name = "mnuRNLogic";
      this.mnuRNLogic.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
      this.mnuRNLogic.Size = new System.Drawing.Size(200, 22);
      this.mnuRNLogic.Text = "&Logic";
      this.mnuRNLogic.Click += new System.EventHandler(this.mnuRNLogic_Click);
      // 
      // mnuRNPicture
      // 
      this.mnuRNPicture.Name = "mnuRNPicture";
      this.mnuRNPicture.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D2)));
      this.mnuRNPicture.Size = new System.Drawing.Size(200, 22);
      this.mnuRNPicture.Text = "&Picture";
      this.mnuRNPicture.Click += new System.EventHandler(this.mnuRNPicture_Click);
      // 
      // mnuRNSound
      // 
      this.mnuRNSound.Name = "mnuRNSound";
      this.mnuRNSound.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D3)));
      this.mnuRNSound.Size = new System.Drawing.Size(200, 22);
      this.mnuRNSound.Text = "&Sound";
      this.mnuRNSound.Click += new System.EventHandler(this.mnuRNSound_Click);
      // 
      // mnuRNView
      // 
      this.mnuRNView.Name = "mnuRNView";
      this.mnuRNView.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D4)));
      this.mnuRNView.Size = new System.Drawing.Size(200, 22);
      this.mnuRNView.Text = "&View";
      this.mnuRNView.Click += new System.EventHandler(this.mnuRNView_Click);
      // 
      // toolStripSeparator6
      // 
      this.toolStripSeparator6.Name = "toolStripSeparator6";
      this.toolStripSeparator6.Size = new System.Drawing.Size(197, 6);
      // 
      // mnuRNObjects
      // 
      this.mnuRNObjects.Name = "mnuRNObjects";
      this.mnuRNObjects.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D5)));
      this.mnuRNObjects.Size = new System.Drawing.Size(200, 22);
      this.mnuRNObjects.Text = "&OBJECT File";
      this.mnuRNObjects.Click += new System.EventHandler(this.mnuRNObjects_Click);
      // 
      // mnuRNWords
      // 
      this.mnuRNWords.Name = "mnuRNWords";
      this.mnuRNWords.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D6)));
      this.mnuRNWords.Size = new System.Drawing.Size(200, 22);
      this.mnuRNWords.Text = "&WORDS.TOK File";
      this.mnuRNWords.Click += new System.EventHandler(this.mnuRNWords_Click);
      // 
      // toolStripSeparator5
      // 
      this.toolStripSeparator5.Name = "toolStripSeparator5";
      this.toolStripSeparator5.Size = new System.Drawing.Size(197, 6);
      // 
      // mnuRNText
      // 
      this.mnuRNText.Name = "mnuRNText";
      this.mnuRNText.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D7)));
      this.mnuRNText.Size = new System.Drawing.Size(200, 22);
      this.mnuRNText.Text = "&Text File";
      this.mnuRNText.Click += new System.EventHandler(this.mnuRNText_Click);
      // 
      // mnuROpen
      // 
      this.mnuROpen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuROLogic,
            this.mnuROPicture,
            this.mnuROSound,
            this.mnuROView,
            this.toolStripSeparator13,
            this.mnuROObjects,
            this.mnuROWords,
            this.toolStripSeparator14,
            this.mnuROText});
      this.mnuROpen.Image = ((System.Drawing.Image)(resources.GetObject("mnuROpen.Image")));
      this.mnuROpen.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.mnuROpen.Name = "mnuROpen";
      this.mnuROpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.mnuROpen.Size = new System.Drawing.Size(305, 22);
      this.mnuROpen.Text = "&Open Resource";
      // 
      // mnuROLogic
      // 
      this.mnuROLogic.Name = "mnuROLogic";
      this.mnuROLogic.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D1)));
      this.mnuROLogic.Size = new System.Drawing.Size(196, 22);
      this.mnuROLogic.Text = "&Logic";
      this.mnuROLogic.Click += new System.EventHandler(this.mnuROLogic_Click);
      // 
      // mnuROPicture
      // 
      this.mnuROPicture.Name = "mnuROPicture";
      this.mnuROPicture.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D2)));
      this.mnuROPicture.Size = new System.Drawing.Size(196, 22);
      this.mnuROPicture.Text = "&Picture";
      this.mnuROPicture.Click += new System.EventHandler(this.mnuROPicture_Click);
      // 
      // mnuROSound
      // 
      this.mnuROSound.Name = "mnuROSound";
      this.mnuROSound.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D3)));
      this.mnuROSound.Size = new System.Drawing.Size(196, 22);
      this.mnuROSound.Text = "&Sound";
      this.mnuROSound.Click += new System.EventHandler(this.mnuROSound_Click);
      // 
      // mnuROView
      // 
      this.mnuROView.Name = "mnuROView";
      this.mnuROView.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D4)));
      this.mnuROView.Size = new System.Drawing.Size(196, 22);
      this.mnuROView.Text = "&View";
      this.mnuROView.Click += new System.EventHandler(this.mnuROView_Click);
      // 
      // toolStripSeparator13
      // 
      this.toolStripSeparator13.Name = "toolStripSeparator13";
      this.toolStripSeparator13.Size = new System.Drawing.Size(193, 6);
      // 
      // mnuROObjects
      // 
      this.mnuROObjects.Name = "mnuROObjects";
      this.mnuROObjects.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D5)));
      this.mnuROObjects.Size = new System.Drawing.Size(196, 22);
      this.mnuROObjects.Text = "&OBJECT File";
      this.mnuROObjects.Click += new System.EventHandler(this.mnuROObjects_Click);
      // 
      // mnuROWords
      // 
      this.mnuROWords.Name = "mnuROWords";
      this.mnuROWords.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D6)));
      this.mnuROWords.Size = new System.Drawing.Size(196, 22);
      this.mnuROWords.Text = "&WORDS.TOK File";
      this.mnuROWords.Click += new System.EventHandler(this.mnuROWords_Click);
      // 
      // toolStripSeparator14
      // 
      this.toolStripSeparator14.Name = "toolStripSeparator14";
      this.toolStripSeparator14.Size = new System.Drawing.Size(193, 6);
      // 
      // mnuROText
      // 
      this.mnuROText.Name = "mnuROText";
      this.mnuROText.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D7)));
      this.mnuROText.Size = new System.Drawing.Size(196, 22);
      this.mnuROText.Text = "&Text File";
      this.mnuROText.Click += new System.EventHandler(this.mnuROText_Click);
      // 
      // mnuRImport
      // 
      this.mnuRImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRILogic,
            this.mnuRIPicture,
            this.mnuRISound,
            this.mnuRIView,
            this.toolStripSeparator15,
            this.mnuRIObjects,
            this.mnuRIWords});
      this.mnuRImport.Name = "mnuRImport";
      this.mnuRImport.Size = new System.Drawing.Size(305, 22);
      this.mnuRImport.Text = "&Import Resource";
      // 
      // mnuRILogic
      // 
      this.mnuRILogic.Name = "mnuRILogic";
      this.mnuRILogic.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D1)));
      this.mnuRILogic.Size = new System.Drawing.Size(223, 22);
      this.mnuRILogic.Text = "&Logic";
      this.mnuRILogic.Click += new System.EventHandler(this.mnuRILogic_Click);
      // 
      // mnuRIPicture
      // 
      this.mnuRIPicture.Name = "mnuRIPicture";
      this.mnuRIPicture.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D2)));
      this.mnuRIPicture.Size = new System.Drawing.Size(223, 22);
      this.mnuRIPicture.Text = "&Picture";
      this.mnuRIPicture.Click += new System.EventHandler(this.mnuRIPicture_Click);
      // 
      // mnuRISound
      // 
      this.mnuRISound.Name = "mnuRISound";
      this.mnuRISound.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D3)));
      this.mnuRISound.Size = new System.Drawing.Size(223, 22);
      this.mnuRISound.Text = "&Sound";
      this.mnuRISound.Click += new System.EventHandler(this.mnuRISound_Click);
      // 
      // mnuRIView
      // 
      this.mnuRIView.Name = "mnuRIView";
      this.mnuRIView.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D4)));
      this.mnuRIView.Size = new System.Drawing.Size(223, 22);
      this.mnuRIView.Text = "&View";
      this.mnuRIView.Click += new System.EventHandler(this.mnuRIView_Click);
      // 
      // toolStripSeparator15
      // 
      this.toolStripSeparator15.Name = "toolStripSeparator15";
      this.toolStripSeparator15.Size = new System.Drawing.Size(220, 6);
      // 
      // mnuRIObjects
      // 
      this.mnuRIObjects.Name = "mnuRIObjects";
      this.mnuRIObjects.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D5)));
      this.mnuRIObjects.Size = new System.Drawing.Size(223, 22);
      this.mnuRIObjects.Text = "&OBJECT File";
      this.mnuRIObjects.Click += new System.EventHandler(this.mnuRIObjects_Click);
      // 
      // mnuRIWords
      // 
      this.mnuRIWords.Name = "mnuRIWords";
      this.mnuRIWords.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D6)));
      this.mnuRIWords.Size = new System.Drawing.Size(223, 22);
      this.mnuRIWords.Text = "&WORDS.TOK File";
      this.mnuRIWords.Click += new System.EventHandler(this.mnuRIWords_Click);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(302, 6);
      // 
      // mnuRSave
      // 
      this.mnuRSave.Image = ((System.Drawing.Image)(resources.GetObject("mnuRSave.Image")));
      this.mnuRSave.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.mnuRSave.Name = "mnuRSave";
      this.mnuRSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
      this.mnuRSave.Size = new System.Drawing.Size(305, 22);
      this.mnuRSave.Text = "&Save Resource";
      // 
      // mnuRExport
      // 
      this.mnuRExport.Name = "mnuRExport";
      this.mnuRExport.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
      this.mnuRExport.Size = new System.Drawing.Size(305, 22);
      this.mnuRExport.Text = "&Export Resource";
      // 
      // toolStripSeparator3
      // 
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new System.Drawing.Size(302, 6);
      // 
      // mnuRAddRemove
      // 
      this.mnuRAddRemove.Name = "mnuRAddRemove";
      this.mnuRAddRemove.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
      this.mnuRAddRemove.Size = new System.Drawing.Size(305, 22);
      this.mnuRAddRemove.Text = "Remove Resource from &Game";
      // 
      // mnuRRenumber
      // 
      this.mnuRRenumber.Name = "mnuRRenumber";
      this.mnuRRenumber.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.N)));
      this.mnuRRenumber.Size = new System.Drawing.Size(305, 22);
      this.mnuRRenumber.Text = "&Renumber Resource";
      this.mnuRRenumber.Click += new System.EventHandler(this.mnuRRenumber_Click);
      // 
      // mnuRIDDesc
      // 
      this.mnuRIDDesc.Image = ((System.Drawing.Image)(resources.GetObject("mnuRIDDesc.Image")));
      this.mnuRIDDesc.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.mnuRIDDesc.Name = "mnuRIDDesc";
      this.mnuRIDDesc.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
      this.mnuRIDDesc.Size = new System.Drawing.Size(305, 22);
      this.mnuRIDDesc.Text = "I&D/Description ...";
      // 
      // toolStripSeparator4
      // 
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      this.toolStripSeparator4.Size = new System.Drawing.Size(302, 6);
      // 
      // mnuRPrint
      // 
      this.mnuRPrint.Image = ((System.Drawing.Image)(resources.GetObject("mnuRPrint.Image")));
      this.mnuRPrint.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.mnuRPrint.Name = "mnuRPrint";
      this.mnuRPrint.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
      this.mnuRPrint.Size = new System.Drawing.Size(305, 22);
      this.mnuRPrint.Text = "&Print";
      // 
      // mnuTools
      // 
      this.mnuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuTSettings,
            this.mnuTSep1,
            this.mnuTLayout,
            this.mnuTMenuEditor,
            this.mnuTGlobals,
            this.mnuReserved,
            this.mnuTSnippets,
            this.mnuTPalette,
            this.mnuTSep2,
            this.mnuTCustom1,
            this.mnuTCustom2,
            this.mnuTCustom3,
            this.mnuTCustom4,
            this.mnuTCustom5,
            this.mnuTCustom6,
            this.mnuTSep3,
            this.mnuTCustomize});
      this.mnuTools.MergeIndex = 1;
      this.mnuTools.Name = "mnuTools";
      this.mnuTools.Size = new System.Drawing.Size(46, 20);
      this.mnuTools.Text = "&Tools";
      // 
      // mnuTSettings
      // 
      this.mnuTSettings.Name = "mnuTSettings";
      this.mnuTSettings.ShortcutKeys = System.Windows.Forms.Keys.F2;
      this.mnuTSettings.Size = new System.Drawing.Size(234, 22);
      this.mnuTSettings.Text = "&Settings";
      // 
      // mnuTSep1
      // 
      this.mnuTSep1.Name = "mnuTSep1";
      this.mnuTSep1.Size = new System.Drawing.Size(231, 6);
      // 
      // mnuTLayout
      // 
      this.mnuTLayout.Name = "mnuTLayout";
      this.mnuTLayout.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
      this.mnuTLayout.Size = new System.Drawing.Size(234, 22);
      this.mnuTLayout.Text = "Room &Layout Editor";
      // 
      // mnuTMenuEditor
      // 
      this.mnuTMenuEditor.Name = "mnuTMenuEditor";
      this.mnuTMenuEditor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
      this.mnuTMenuEditor.Size = new System.Drawing.Size(234, 22);
      this.mnuTMenuEditor.Text = "&Menu Editor";
      // 
      // mnuTGlobals
      // 
      this.mnuTGlobals.Name = "mnuTGlobals";
      this.mnuTGlobals.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
      this.mnuTGlobals.Size = new System.Drawing.Size(234, 22);
      this.mnuTGlobals.Text = "&Global Defines ...";
      // 
      // mnuReserved
      // 
      this.mnuReserved.Name = "mnuReserved";
      this.mnuReserved.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
      this.mnuReserved.Size = new System.Drawing.Size(234, 22);
      this.mnuReserved.Text = "&Reserved Defines ...";
      // 
      // mnuTSnippets
      // 
      this.mnuTSnippets.Name = "mnuTSnippets";
      this.mnuTSnippets.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.T)));
      this.mnuTSnippets.Size = new System.Drawing.Size(234, 22);
      this.mnuTSnippets.Text = "Code &Snippets ...";
      // 
      // mnuTPalette
      // 
      this.mnuTPalette.Name = "mnuTPalette";
      this.mnuTPalette.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
      this.mnuTPalette.Size = new System.Drawing.Size(234, 22);
      this.mnuTPalette.Text = "Color &Palette ...";
      // 
      // mnuTSep2
      // 
      this.mnuTSep2.Name = "mnuTSep2";
      this.mnuTSep2.Size = new System.Drawing.Size(231, 6);
      // 
      // mnuTCustom1
      // 
      this.mnuTCustom1.Name = "mnuTCustom1";
      this.mnuTCustom1.Size = new System.Drawing.Size(234, 22);
      this.mnuTCustom1.Text = "tool1";
      // 
      // mnuTCustom2
      // 
      this.mnuTCustom2.Name = "mnuTCustom2";
      this.mnuTCustom2.Size = new System.Drawing.Size(234, 22);
      this.mnuTCustom2.Text = "tool2";
      // 
      // mnuTCustom3
      // 
      this.mnuTCustom3.Name = "mnuTCustom3";
      this.mnuTCustom3.Size = new System.Drawing.Size(234, 22);
      this.mnuTCustom3.Text = "tool3";
      // 
      // mnuTCustom4
      // 
      this.mnuTCustom4.Name = "mnuTCustom4";
      this.mnuTCustom4.Size = new System.Drawing.Size(234, 22);
      this.mnuTCustom4.Text = "tool4";
      // 
      // mnuTCustom5
      // 
      this.mnuTCustom5.Name = "mnuTCustom5";
      this.mnuTCustom5.Size = new System.Drawing.Size(234, 22);
      this.mnuTCustom5.Text = "tool5";
      // 
      // mnuTCustom6
      // 
      this.mnuTCustom6.Name = "mnuTCustom6";
      this.mnuTCustom6.Size = new System.Drawing.Size(234, 22);
      this.mnuTCustom6.Text = "tool6";
      // 
      // mnuTSep3
      // 
      this.mnuTSep3.Name = "mnuTSep3";
      this.mnuTSep3.Size = new System.Drawing.Size(231, 6);
      // 
      // mnuTCustomize
      // 
      this.mnuTCustomize.Name = "mnuTCustomize";
      this.mnuTCustomize.ShortcutKeys = System.Windows.Forms.Keys.F6;
      this.mnuTCustomize.Size = new System.Drawing.Size(234, 22);
      this.mnuTCustomize.Text = "&Customize Tool Menu ...";
      // 
      // mnuWindow
      // 
      this.mnuWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuWCascade,
            this.mnuWTileV,
            this.mnuWTileH,
            this.mnuWArrange,
            this.mnuWMinimize,
            this.toolStripSeparator8,
            this.mnuWClose});
      this.mnuWindow.Name = "mnuWindow";
      this.mnuWindow.Size = new System.Drawing.Size(63, 20);
      this.mnuWindow.Text = "&Window";
      this.mnuWindow.DropDownOpening += new System.EventHandler(this.mnuWindow_DropDownOpening);
      // 
      // mnuWCascade
      // 
      this.mnuWCascade.Name = "mnuWCascade";
      this.mnuWCascade.Size = new System.Drawing.Size(150, 22);
      this.mnuWCascade.Text = "Cascade";
      this.mnuWCascade.Click += new System.EventHandler(this.mnuWCascade_Click);
      // 
      // mnuWTileV
      // 
      this.mnuWTileV.Name = "mnuWTileV";
      this.mnuWTileV.Size = new System.Drawing.Size(150, 22);
      this.mnuWTileV.Text = "Tile Vertical";
      this.mnuWTileV.Click += new System.EventHandler(this.mnuWTileV_Click);
      // 
      // mnuWTileH
      // 
      this.mnuWTileH.Name = "mnuWTileH";
      this.mnuWTileH.Size = new System.Drawing.Size(150, 22);
      this.mnuWTileH.Text = "Tile Horizontal";
      this.mnuWTileH.Click += new System.EventHandler(this.mnuWTileH_Click);
      // 
      // mnuWArrange
      // 
      this.mnuWArrange.Name = "mnuWArrange";
      this.mnuWArrange.Size = new System.Drawing.Size(150, 22);
      this.mnuWArrange.Text = "Arrange Icons";
      this.mnuWArrange.Click += new System.EventHandler(this.mnuWArrange_Click);
      // 
      // mnuWMinimize
      // 
      this.mnuWMinimize.Name = "mnuWMinimize";
      this.mnuWMinimize.Size = new System.Drawing.Size(150, 22);
      this.mnuWMinimize.Text = "Minimize All";
      this.mnuWMinimize.Click += new System.EventHandler(this.mnuWMinimize_Click);
      // 
      // toolStripSeparator8
      // 
      this.toolStripSeparator8.Name = "toolStripSeparator8";
      this.toolStripSeparator8.Size = new System.Drawing.Size(147, 6);
      // 
      // mnuWClose
      // 
      this.mnuWClose.Name = "mnuWClose";
      this.mnuWClose.Size = new System.Drawing.Size(150, 22);
      this.mnuWClose.Text = "Close Window";
      this.mnuWClose.Click += new System.EventHandler(this.mnuWClose_Click);
      // 
      // mnuHelp
      // 
      this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHContents,
            this.mnuHIndex,
            this.mnuHSep1,
            this.mnuHCommands,
            this.mnuHReference,
            this.mnuHSep2,
            this.mnuHAbout});
      this.mnuHelp.Name = "mnuHelp";
      this.mnuHelp.Size = new System.Drawing.Size(44, 20);
      this.mnuHelp.Text = "&Help";
      // 
      // mnuHContents
      // 
      this.mnuHContents.Name = "mnuHContents";
      this.mnuHContents.ShortcutKeys = System.Windows.Forms.Keys.F1;
      this.mnuHContents.Size = new System.Drawing.Size(238, 22);
      this.mnuHContents.Text = "&Contents";
      // 
      // mnuHIndex
      // 
      this.mnuHIndex.Name = "mnuHIndex";
      this.mnuHIndex.Size = new System.Drawing.Size(238, 22);
      this.mnuHIndex.Text = "&Index";
      // 
      // mnuHSep1
      // 
      this.mnuHSep1.Name = "mnuHSep1";
      this.mnuHSep1.Size = new System.Drawing.Size(235, 6);
      // 
      // mnuHCommands
      // 
      this.mnuHCommands.Name = "mnuHCommands";
      this.mnuHCommands.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F1)));
      this.mnuHCommands.Size = new System.Drawing.Size(238, 22);
      this.mnuHCommands.Text = "&Logic Commands Help";
      // 
      // mnuHReference
      // 
      this.mnuHReference.Name = "mnuHReference";
      this.mnuHReference.ShortcutKeys = System.Windows.Forms.Keys.F11;
      this.mnuHReference.Size = new System.Drawing.Size(238, 22);
      this.mnuHReference.Text = "AGI &Reference";
      // 
      // mnuHSep2
      // 
      this.mnuHSep2.Name = "mnuHSep2";
      this.mnuHSep2.Size = new System.Drawing.Size(235, 6);
      // 
      // mnuHAbout
      // 
      this.mnuHAbout.Name = "mnuHAbout";
      this.mnuHAbout.Size = new System.Drawing.Size(238, 22);
      this.mnuHAbout.Text = "&About WinAGI GDS...";
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(112, 26);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(111, 22);
      this.toolStripMenuItem1.Text = "menu1";
      // 
      // btnNewRes
      // 
      this.btnNewRes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnNewRes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNewLogic,
            this.btnNewPicture,
            this.btnNewSound,
            this.btnNewView});
      this.btnNewRes.Image = ((System.Drawing.Image)(resources.GetObject("btnNewRes.Image")));
      this.btnNewRes.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnNewRes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnNewRes.Name = "btnNewRes";
      this.btnNewRes.Size = new System.Drawing.Size(64, 52);
      this.btnNewRes.Text = "&New Resource";
      // 
      // btnNewLogic
      // 
      this.btnNewLogic.Image = ((System.Drawing.Image)(resources.GetObject("btnNewLogic.Image")));
      this.btnNewLogic.Name = "btnNewLogic";
      this.btnNewLogic.Size = new System.Drawing.Size(138, 22);
      this.btnNewLogic.Text = "New Logic";
      this.btnNewLogic.Click += new System.EventHandler(this.btnNewLogic_Click);
      // 
      // btnNewPicture
      // 
      this.btnNewPicture.Image = ((System.Drawing.Image)(resources.GetObject("btnNewPicture.Image")));
      this.btnNewPicture.Name = "btnNewPicture";
      this.btnNewPicture.Size = new System.Drawing.Size(138, 22);
      this.btnNewPicture.Text = "New Picture";
      this.btnNewPicture.Click += new System.EventHandler(this.btnNewPicture_Click);
      // 
      // btnNewSound
      // 
      this.btnNewSound.Image = ((System.Drawing.Image)(resources.GetObject("btnNewSound.Image")));
      this.btnNewSound.Name = "btnNewSound";
      this.btnNewSound.Size = new System.Drawing.Size(138, 22);
      this.btnNewSound.Text = "New Sound";
      this.btnNewSound.Click += new System.EventHandler(this.btnNewSound_Click);
      // 
      // btnNewView
      // 
      this.btnNewView.Image = ((System.Drawing.Image)(resources.GetObject("btnNewView.Image")));
      this.btnNewView.Name = "btnNewView";
      this.btnNewView.Size = new System.Drawing.Size(138, 22);
      this.btnNewView.Text = "New View";
      this.btnNewView.Click += new System.EventHandler(this.btnNewView_Click);
      // 
      // statusStrip1
      // 
      this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusPanel1,
            this.springLabel,
            this.CapsLockLabel,
            this.NumLockLabel,
            this.InsertLockLabel});
      this.statusStrip1.Location = new System.Drawing.Point(0, 332);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
      this.statusStrip1.Size = new System.Drawing.Size(642, 46);
      this.statusStrip1.TabIndex = 2;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // StatusPanel1
      // 
      this.StatusPanel1.Name = "StatusPanel1";
      this.StatusPanel1.Size = new System.Drawing.Size(0, 41);
      // 
      // springLabel
      // 
      this.springLabel.Name = "springLabel";
      this.springLabel.Size = new System.Drawing.Size(406, 41);
      this.springLabel.Spring = true;
      // 
      // CapsLockLabel
      // 
      this.CapsLockLabel.AutoSize = false;
      this.CapsLockLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
      this.CapsLockLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
      this.CapsLockLabel.Name = "CapsLockLabel";
      this.CapsLockLabel.Size = new System.Drawing.Size(74, 41);
      this.CapsLockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // NumLockLabel
      // 
      this.NumLockLabel.AutoSize = false;
      this.NumLockLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
      this.NumLockLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
      this.NumLockLabel.Name = "NumLockLabel";
      this.NumLockLabel.Size = new System.Drawing.Size(74, 41);
      this.NumLockLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // InsertLockLabel
      // 
      this.InsertLockLabel.AutoSize = false;
      this.InsertLockLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
      this.InsertLockLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
      this.InsertLockLabel.Name = "InsertLockLabel";
      this.InsertLockLabel.Size = new System.Drawing.Size(74, 41);
      // 
      // toolStrip1
      // 
      this.toolStrip1.AllowItemReorder = true;
      this.toolStrip1.ImageScalingSize = new System.Drawing.Size(48, 48);
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenGame,
            this.btnCloseGame,
            this.btnRun,
            this.btnSep1,
            this.btnPrint,
            this.btnNewRes,
            this.btnOpenRes,
            this.btnImportRes,
            this.btnSep2,
            this.btnWords,
            this.btnOjects,
            this.btnSep3,
            this.btnSaveResource,
            this.btnAddRemove,
            this.btnExportRes,
            this.btnSep4,
            this.btnLayoutEd,
            this.btnMenuEd,
            this.btnGlobals,
            this.btnSep5,
            this.btnHelp});
      this.toolStrip1.Location = new System.Drawing.Point(0, 24);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 1, 2, 1);
      this.toolStrip1.Size = new System.Drawing.Size(642, 60);
      this.toolStrip1.TabIndex = 3;
      this.toolStrip1.Text = "toolStrip1";
      this.toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip1_ItemClicked);
      // 
      // btnOpenGame
      // 
      this.btnOpenGame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnOpenGame.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenGame.Image")));
      this.btnOpenGame.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnOpenGame.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnOpenGame.Name = "btnOpenGame";
      this.btnOpenGame.Size = new System.Drawing.Size(52, 52);
      this.btnOpenGame.Text = "&Open";
      this.btnOpenGame.Click += new System.EventHandler(this.btnOpenGame_Click);
      // 
      // btnCloseGame
      // 
      this.btnCloseGame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnCloseGame.Image = ((System.Drawing.Image)(resources.GetObject("btnCloseGame.Image")));
      this.btnCloseGame.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnCloseGame.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnCloseGame.Name = "btnCloseGame";
      this.btnCloseGame.Size = new System.Drawing.Size(52, 52);
      this.btnCloseGame.Text = "&Close";
      this.btnCloseGame.Click += new System.EventHandler(this.mnuGClose_Click);
      // 
      // btnRun
      // 
      this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
      this.btnRun.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnRun.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnRun.Name = "btnRun";
      this.btnRun.Size = new System.Drawing.Size(52, 52);
      this.btnRun.Text = "&Run";
      // 
      // btnSep1
      // 
      this.btnSep1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
      this.btnSep1.Name = "btnSep1";
      this.btnSep1.Size = new System.Drawing.Size(6, 58);
      // 
      // btnPrint
      // 
      this.btnPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnPrint.Image = ((System.Drawing.Image)(resources.GetObject("btnPrint.Image")));
      this.btnPrint.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnPrint.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnPrint.Name = "btnPrint";
      this.btnPrint.Size = new System.Drawing.Size(52, 52);
      this.btnPrint.Text = "&Print";
      // 
      // btnOpenRes
      // 
      this.btnOpenRes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnOpenRes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenLogic,
            this.btnOpenPicture,
            this.btnOpenSound,
            this.btnOpenView});
      this.btnOpenRes.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenRes.Image")));
      this.btnOpenRes.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnOpenRes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnOpenRes.Name = "btnOpenRes";
      this.btnOpenRes.Size = new System.Drawing.Size(64, 52);
      this.btnOpenRes.Text = "Open Resource";
      // 
      // btnOpenLogic
      // 
      this.btnOpenLogic.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenLogic.Image")));
      this.btnOpenLogic.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnOpenLogic.Name = "btnOpenLogic";
      this.btnOpenLogic.Size = new System.Drawing.Size(143, 22);
      this.btnOpenLogic.Text = "Open Logic";
      this.btnOpenLogic.Click += new System.EventHandler(this.btnOpenLogic_Click);
      // 
      // btnOpenPicture
      // 
      this.btnOpenPicture.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenPicture.Image")));
      this.btnOpenPicture.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnOpenPicture.Name = "btnOpenPicture";
      this.btnOpenPicture.Size = new System.Drawing.Size(143, 22);
      this.btnOpenPicture.Text = "Open Picture";
      this.btnOpenPicture.Click += new System.EventHandler(this.btnOpenPicture_Click);
      // 
      // btnOpenSound
      // 
      this.btnOpenSound.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenSound.Image")));
      this.btnOpenSound.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnOpenSound.Name = "btnOpenSound";
      this.btnOpenSound.Size = new System.Drawing.Size(143, 22);
      this.btnOpenSound.Text = "Open Sound";
      this.btnOpenSound.Click += new System.EventHandler(this.btnOpenSound_Click);
      // 
      // btnOpenView
      // 
      this.btnOpenView.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenView.Image")));
      this.btnOpenView.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnOpenView.Name = "btnOpenView";
      this.btnOpenView.Size = new System.Drawing.Size(143, 22);
      this.btnOpenView.Text = "Open View";
      this.btnOpenView.Click += new System.EventHandler(this.btnOpenView_Click);
      // 
      // btnImportRes
      // 
      this.btnImportRes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnImportRes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnImportLogic,
            this.btnImportPicture,
            this.btnImportSound,
            this.btnImportView});
      this.btnImportRes.Image = ((System.Drawing.Image)(resources.GetObject("btnImportRes.Image")));
      this.btnImportRes.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnImportRes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnImportRes.Name = "btnImportRes";
      this.btnImportRes.Size = new System.Drawing.Size(64, 52);
      this.btnImportRes.Text = "Import Resource";
      // 
      // btnImportLogic
      // 
      this.btnImportLogic.Image = ((System.Drawing.Image)(resources.GetObject("btnImportLogic.Image")));
      this.btnImportLogic.Name = "btnImportLogic";
      this.btnImportLogic.Size = new System.Drawing.Size(150, 22);
      this.btnImportLogic.Text = "Import Logic";
      this.btnImportLogic.Click += new System.EventHandler(this.btnImportLogic_Click);
      // 
      // btnImportPicture
      // 
      this.btnImportPicture.Image = ((System.Drawing.Image)(resources.GetObject("btnImportPicture.Image")));
      this.btnImportPicture.Name = "btnImportPicture";
      this.btnImportPicture.Size = new System.Drawing.Size(150, 22);
      this.btnImportPicture.Text = "Import Picture";
      this.btnImportPicture.Click += new System.EventHandler(this.btnImportPicture_Click);
      // 
      // btnImportSound
      // 
      this.btnImportSound.Image = ((System.Drawing.Image)(resources.GetObject("btnImportSound.Image")));
      this.btnImportSound.Name = "btnImportSound";
      this.btnImportSound.Size = new System.Drawing.Size(150, 22);
      this.btnImportSound.Text = "Import Sound";
      this.btnImportSound.Click += new System.EventHandler(this.btnImportSound_Click);
      // 
      // btnImportView
      // 
      this.btnImportView.Image = ((System.Drawing.Image)(resources.GetObject("btnImportView.Image")));
      this.btnImportView.Name = "btnImportView";
      this.btnImportView.Size = new System.Drawing.Size(150, 22);
      this.btnImportView.Text = "Import View";
      this.btnImportView.Click += new System.EventHandler(this.btnImportView_Click);
      // 
      // btnSep2
      // 
      this.btnSep2.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
      this.btnSep2.Name = "btnSep2";
      this.btnSep2.Size = new System.Drawing.Size(6, 58);
      // 
      // btnWords
      // 
      this.btnWords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnWords.Image = ((System.Drawing.Image)(resources.GetObject("btnWords.Image")));
      this.btnWords.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnWords.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnWords.Name = "btnWords";
      this.btnWords.Size = new System.Drawing.Size(52, 52);
      this.btnWords.Text = "WORDS.TOK";
      this.btnWords.Click += new System.EventHandler(this.btnWords_Click);
      // 
      // btnOjects
      // 
      this.btnOjects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnOjects.Image = ((System.Drawing.Image)(resources.GetObject("btnOjects.Image")));
      this.btnOjects.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnOjects.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnOjects.Name = "btnOjects";
      this.btnOjects.Size = new System.Drawing.Size(52, 52);
      this.btnOjects.Text = "OBJECT File";
      this.btnOjects.Click += new System.EventHandler(this.btnOjects_Click);
      // 
      // btnSep3
      // 
      this.btnSep3.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
      this.btnSep3.Name = "btnSep3";
      this.btnSep3.Size = new System.Drawing.Size(6, 58);
      // 
      // btnSaveResource
      // 
      this.btnSaveResource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnSaveResource.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveResource.Image")));
      this.btnSaveResource.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnSaveResource.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnSaveResource.Name = "btnSaveResource";
      this.btnSaveResource.Size = new System.Drawing.Size(52, 52);
      this.btnSaveResource.Text = "Save Resource";
      // 
      // btnAddRemove
      // 
      this.btnAddRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnAddRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnAddRemove.Image")));
      this.btnAddRemove.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnAddRemove.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnAddRemove.Name = "btnAddRemove";
      this.btnAddRemove.Size = new System.Drawing.Size(52, 52);
      this.btnAddRemove.Text = "Add/Remove Resource";
      // 
      // btnExportRes
      // 
      this.btnExportRes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnExportRes.Image = ((System.Drawing.Image)(resources.GetObject("btnExportRes.Image")));
      this.btnExportRes.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnExportRes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnExportRes.Name = "btnExportRes";
      this.btnExportRes.Size = new System.Drawing.Size(52, 52);
      this.btnExportRes.Text = "Export Resource";
      // 
      // btnSep4
      // 
      this.btnSep4.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
      this.btnSep4.Name = "btnSep4";
      this.btnSep4.Size = new System.Drawing.Size(6, 58);
      // 
      // btnLayoutEd
      // 
      this.btnLayoutEd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnLayoutEd.Image = ((System.Drawing.Image)(resources.GetObject("btnLayoutEd.Image")));
      this.btnLayoutEd.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnLayoutEd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnLayoutEd.Name = "btnLayoutEd";
      this.btnLayoutEd.Size = new System.Drawing.Size(52, 52);
      this.btnLayoutEd.Text = "Layout Editor";
      // 
      // btnMenuEd
      // 
      this.btnMenuEd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnMenuEd.Image = ((System.Drawing.Image)(resources.GetObject("btnMenuEd.Image")));
      this.btnMenuEd.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnMenuEd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnMenuEd.Name = "btnMenuEd";
      this.btnMenuEd.Size = new System.Drawing.Size(52, 52);
      this.btnMenuEd.Text = "Menu Editor";
      // 
      // btnGlobals
      // 
      this.btnGlobals.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnGlobals.Image = ((System.Drawing.Image)(resources.GetObject("btnGlobals.Image")));
      this.btnGlobals.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnGlobals.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnGlobals.Name = "btnGlobals";
      this.btnGlobals.Size = new System.Drawing.Size(52, 52);
      this.btnGlobals.Text = "Global Defines";
      // 
      // btnSep5
      // 
      this.btnSep5.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
      this.btnSep5.Name = "btnSep5";
      this.btnSep5.Size = new System.Drawing.Size(6, 58);
      // 
      // btnHelp
      // 
      this.btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.btnHelp.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp.Image")));
      this.btnHelp.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
      this.btnHelp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 4);
      this.btnHelp.Name = "btnHelp";
      this.btnHelp.Size = new System.Drawing.Size(52, 52);
      this.btnHelp.Text = "Help";
      // 
      // toolStripSplitButton2
      // 
      this.toolStripSplitButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton2.Image")));
      this.toolStripSplitButton2.Name = "toolStripSplitButton2";
      this.toolStripSplitButton2.Size = new System.Drawing.Size(180, 22);
      this.toolStripSplitButton2.Text = "toolStripMenuItem3";
      // 
      // toolStripSplitButton3
      // 
      this.toolStripSplitButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton3.Image")));
      this.toolStripSplitButton3.Name = "toolStripSplitButton3";
      this.toolStripSplitButton3.Size = new System.Drawing.Size(180, 22);
      this.toolStripSplitButton3.Text = "toolStripMenuItem4";
      // 
      // toolStripSplitButton4
      // 
      this.toolStripSplitButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton4.Image")));
      this.toolStripSplitButton4.Name = "toolStripSplitButton4";
      this.toolStripSplitButton4.Size = new System.Drawing.Size(180, 22);
      this.toolStripSplitButton4.Text = "toolStripMenuItem5";
      // 
      // pnlResources
      // 
      this.pnlResources.Controls.Add(this.splResource);
      this.pnlResources.Dock = System.Windows.Forms.DockStyle.Left;
      this.pnlResources.Location = new System.Drawing.Point(0, 84);
      this.pnlResources.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
      this.pnlResources.Name = "pnlResources";
      this.pnlResources.Size = new System.Drawing.Size(157, 248);
      this.pnlResources.TabIndex = 16;
      // 
      // splResource
      // 
      this.splResource.Cursor = System.Windows.Forms.Cursors.Default;
      this.splResource.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splResource.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splResource.Location = new System.Drawing.Point(0, 0);
      this.splResource.Margin = new System.Windows.Forms.Padding(2);
      this.splResource.Name = "splResource";
      this.splResource.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splResource.Panel1
      // 
      this.splResource.Panel1.Controls.Add(this.tvwResources);
      this.splResource.Panel1.Controls.Add(this.cmdBack);
      this.splResource.Panel1.Controls.Add(this.cmdForward);
      this.splResource.Panel1.Controls.Add(this.lstResources);
      this.splResource.Panel1.Controls.Add(this.cmbResType);
      this.splResource.Panel1.Cursor = System.Windows.Forms.Cursors.Default;
      this.splResource.Panel1.Resize += new System.EventHandler(this.splResource_Panel1_Resize);
      // 
      // splResource.Panel2
      // 
      this.splResource.Panel2.Controls.Add(this.propertyGrid1);
      this.splResource.Panel2.Controls.Add(this.picProperties);
      this.splResource.Panel2.Controls.Add(this.fsbProperty);
      this.splResource.Panel2.Cursor = System.Windows.Forms.Cursors.Default;
      this.splResource.Size = new System.Drawing.Size(157, 248);
      this.splResource.SplitterDistance = 25;
      this.splResource.TabIndex = 0;
      this.splResource.TabStop = false;
      // 
      // tvwResources
      // 
      this.tvwResources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tvwResources.HideSelection = false;
      this.tvwResources.Location = new System.Drawing.Point(0, 26);
      this.tvwResources.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.tvwResources.Name = "tvwResources";
      treeNode1.Name = "logics";
      treeNode1.Text = "Logics";
      treeNode2.Name = "pictures";
      treeNode2.Text = "Pictures";
      treeNode3.Name = "sounds";
      treeNode3.Text = "Sounds";
      treeNode4.Name = "views";
      treeNode4.Text = "Views";
      treeNode5.Name = "objects";
      treeNode5.Text = "Objects";
      treeNode6.Name = "words";
      treeNode6.Text = "Words";
      treeNode7.Name = "Node0";
      treeNode7.Text = "AGIGAME";
      this.tvwResources.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode7});
      this.tvwResources.Size = new System.Drawing.Size(152, 0);
      this.tvwResources.TabIndex = 25;
      this.tvwResources.Visible = false;
      this.tvwResources.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.tvwResources_AfterCollapse);
      this.tvwResources.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwResources_AfterSelect);
      this.tvwResources.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvwResources_NodeMouseClick);
      this.tvwResources.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvwResources_NodeMouseDoubleClick);
      // 
      // cmdBack
      // 
      this.cmdBack.Image = ((System.Drawing.Image)(resources.GetObject("cmdBack.Image")));
      this.cmdBack.Location = new System.Drawing.Point(0, 0);
      this.cmdBack.Name = "cmdBack";
      this.cmdBack.Size = new System.Drawing.Size(80, 26);
      this.cmdBack.TabIndex = 2;
      this.cmdBack.UseVisualStyleBackColor = true;
      this.cmdBack.Click += new System.EventHandler(this.cmdBack_Click);
      this.cmdBack.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cmdBack_MouseDown);
      // 
      // cmdForward
      // 
      this.cmdForward.Image = ((System.Drawing.Image)(resources.GetObject("cmdForward.Image")));
      this.cmdForward.Location = new System.Drawing.Point(80, 0);
      this.cmdForward.Name = "cmdForward";
      this.cmdForward.Size = new System.Drawing.Size(80, 26);
      this.cmdForward.TabIndex = 3;
      this.cmdForward.UseVisualStyleBackColor = true;
      this.cmdForward.Click += new System.EventHandler(this.cmdForward_Click);
      this.cmdForward.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cmdForward_MouseDown);
      // 
      // lstResources
      // 
      this.lstResources.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lstResources.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.lstResources.FullRowSelect = true;
      this.lstResources.HideSelection = false;
      this.lstResources.Location = new System.Drawing.Point(0, 46);
      this.lstResources.Margin = new System.Windows.Forms.Padding(2);
      this.lstResources.MultiSelect = false;
      this.lstResources.Name = "lstResources";
      this.lstResources.ShowGroups = false;
      this.lstResources.Size = new System.Drawing.Size(150, 0);
      this.lstResources.TabIndex = 27;
      this.lstResources.UseCompatibleStateImageBehavior = false;
      this.lstResources.View = System.Windows.Forms.View.Details;
      this.lstResources.SelectedIndexChanged += new System.EventHandler(this.lstResources_SelectedIndexChanged);
      this.lstResources.DoubleClick += new System.EventHandler(this.lstResources_DoubleClick);
      // 
      // cmbResType
      // 
      this.cmbResType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cmbResType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbResType.FormattingEnabled = true;
      this.cmbResType.Items.AddRange(new object[] {
            "agi",
            "LOGICS",
            "PICTURES",
            "SOUNDS",
            "VIEWS",
            "OBJECT",
            "WORDS.TOK"});
      this.cmbResType.Location = new System.Drawing.Point(0, 26);
      this.cmbResType.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.cmbResType.Name = "cmbResType";
      this.cmbResType.Size = new System.Drawing.Size(156, 23);
      this.cmbResType.TabIndex = 26;
      this.cmbResType.Visible = false;
      this.cmbResType.SelectedIndexChanged += new System.EventHandler(this.cmbResType_SelectedIndexChanged);
      // 
      // propertyGrid1
      // 
      this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.propertyGrid1.CommandsVisibleIfAvailable = false;
      this.propertyGrid1.HelpVisible = false;
      this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
      this.propertyGrid1.Margin = new System.Windows.Forms.Padding(2);
      this.propertyGrid1.Name = "propertyGrid1";
      this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
      this.propertyGrid1.Size = new System.Drawing.Size(156, 196);
      this.propertyGrid1.TabIndex = 28;
      this.propertyGrid1.ToolbarVisible = false;
      this.propertyGrid1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMDIMain_KeyDown);
      this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
      this.propertyGrid1.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(this.propertyGrid1_SelectedGridItemChanged);
      this.propertyGrid1.Click += new System.EventHandler(this.propertyGrid1_Click);
      this.propertyGrid1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.propertyGrid1_MouseClick);
      this.propertyGrid1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.propertyGrid1_PreviewKeyDown);
      // 
      // picProperties
      // 
      this.picProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.picProperties.BackColor = System.Drawing.SystemColors.Window;
      this.picProperties.Location = new System.Drawing.Point(0, 0);
      this.picProperties.Margin = new System.Windows.Forms.Padding(2);
      this.picProperties.Name = "picProperties";
      this.picProperties.Size = new System.Drawing.Size(113, 131);
      this.picProperties.TabIndex = 25;
      this.picProperties.TabStop = false;
      this.picProperties.Paint += new System.Windows.Forms.PaintEventHandler(this.picProperties_Paint);
      this.picProperties.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.picProperties_PreviewKeyDown);
      this.picProperties.Resize += new System.EventHandler(this.picProperties_Resize);
      // 
      // fsbProperty
      // 
      this.fsbProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fsbProperty.LargeChange = 3;
      this.fsbProperty.Location = new System.Drawing.Point(0, 0);
      this.fsbProperty.Name = "fsbProperty";
      this.fsbProperty.Size = new System.Drawing.Size(24, 74);
      this.fsbProperty.TabIndex = 24;
      this.fsbProperty.Scroll += new System.Windows.Forms.ScrollEventHandler(this.fsbProperty_Scroll);
      this.fsbProperty.ValueChanged += new System.EventHandler(this.fsbProperty_ValueChanged);
      // 
      // picNavList
      // 
      this.picNavList.BackColor = System.Drawing.SystemColors.Window;
      this.picNavList.Location = new System.Drawing.Point(360, 66);
      this.picNavList.Margin = new System.Windows.Forms.Padding(2);
      this.picNavList.Name = "picNavList";
      this.picNavList.Size = new System.Drawing.Size(74, 78);
      this.picNavList.TabIndex = 24;
      this.picNavList.TabStop = false;
      this.picNavList.Visible = false;
      this.picNavList.Paint += new System.Windows.Forms.PaintEventHandler(this.picNavList_Paint);
      this.picNavList.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picNavList_MouseMove);
      this.picNavList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picNavList_MouseUp);
      // 
      // lstProperty
      // 
      this.lstProperty.FormattingEnabled = true;
      this.lstProperty.ItemHeight = 15;
      this.lstProperty.Location = new System.Drawing.Point(172, 253);
      this.lstProperty.Margin = new System.Windows.Forms.Padding(2);
      this.lstProperty.Name = "lstProperty";
      this.lstProperty.Size = new System.Drawing.Size(98, 19);
      this.lstProperty.TabIndex = 26;
      this.lstProperty.Visible = false;
      // 
      // splitResource
      // 
      this.splitResource.Location = new System.Drawing.Point(157, 84);
      this.splitResource.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
      this.splitResource.Name = "splitResource";
      this.splitResource.Size = new System.Drawing.Size(2, 248);
      this.splitResource.TabIndex = 18;
      this.splitResource.TabStop = false;
      this.splitResource.Visible = false;
      // 
      // pnlWarnings
      // 
      this.pnlWarnings.Controls.Add(this.fgWarnings);
      this.pnlWarnings.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlWarnings.Location = new System.Drawing.Point(159, 246);
      this.pnlWarnings.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
      this.pnlWarnings.Name = "pnlWarnings";
      this.pnlWarnings.Size = new System.Drawing.Size(483, 86);
      this.pnlWarnings.TabIndex = 20;
      this.pnlWarnings.Visible = false;
      // 
      // fgWarnings
      // 
      this.fgWarnings.AllowUserToAddRows = false;
      this.fgWarnings.AllowUserToDeleteRows = false;
      this.fgWarnings.AllowUserToResizeRows = false;
      this.fgWarnings.BorderStyle = System.Windows.Forms.BorderStyle.None;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.fgWarnings.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.fgWarnings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.fgWarnings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colResType,
            this.colIDK,
            this.colWarning,
            this.colDesc,
            this.colLogic,
            this.colLIne,
            this.colModule});
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.fgWarnings.DefaultCellStyle = dataGridViewCellStyle2;
      this.fgWarnings.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fgWarnings.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
      this.fgWarnings.Location = new System.Drawing.Point(0, 0);
      this.fgWarnings.Margin = new System.Windows.Forms.Padding(2);
      this.fgWarnings.Name = "fgWarnings";
      this.fgWarnings.ReadOnly = true;
      this.fgWarnings.RowHeadersVisible = false;
      this.fgWarnings.RowHeadersWidth = 82;
      this.fgWarnings.RowTemplate.Height = 41;
      this.fgWarnings.RowTemplate.ReadOnly = true;
      this.fgWarnings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.fgWarnings.ShowCellErrors = false;
      this.fgWarnings.ShowEditingIcon = false;
      this.fgWarnings.ShowRowErrors = false;
      this.fgWarnings.Size = new System.Drawing.Size(483, 86);
      this.fgWarnings.StandardTab = true;
      this.fgWarnings.TabIndex = 0;
      // 
      // colResType
      // 
      this.colResType.HeaderText = "restype";
      this.colResType.MinimumWidth = 10;
      this.colResType.Name = "colResType";
      this.colResType.ReadOnly = true;
      this.colResType.Width = 200;
      // 
      // colIDK
      // 
      this.colIDK.HeaderText = "idontknow";
      this.colIDK.MinimumWidth = 10;
      this.colIDK.Name = "colIDK";
      this.colIDK.ReadOnly = true;
      this.colIDK.Width = 200;
      // 
      // colWarning
      // 
      this.colWarning.HeaderText = "Warning";
      this.colWarning.MinimumWidth = 10;
      this.colWarning.Name = "colWarning";
      this.colWarning.ReadOnly = true;
      this.colWarning.Width = 200;
      // 
      // colDesc
      // 
      this.colDesc.HeaderText = "Description";
      this.colDesc.MinimumWidth = 10;
      this.colDesc.Name = "colDesc";
      this.colDesc.ReadOnly = true;
      this.colDesc.Width = 200;
      // 
      // colLogic
      // 
      this.colLogic.HeaderText = "Logic#";
      this.colLogic.MinimumWidth = 10;
      this.colLogic.Name = "colLogic";
      this.colLogic.ReadOnly = true;
      this.colLogic.Width = 200;
      // 
      // colLIne
      // 
      this.colLIne.HeaderText = "Line#";
      this.colLIne.MinimumWidth = 10;
      this.colLIne.Name = "colLIne";
      this.colLIne.ReadOnly = true;
      this.colLIne.Width = 200;
      // 
      // colModule
      // 
      this.colModule.HeaderText = "Module";
      this.colModule.MinimumWidth = 10;
      this.colModule.Name = "colModule";
      this.colModule.ReadOnly = true;
      this.colModule.Width = 200;
      // 
      // splitWarning
      // 
      this.splitWarning.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.splitWarning.Location = new System.Drawing.Point(159, 244);
      this.splitWarning.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
      this.splitWarning.Name = "splitWarning";
      this.splitWarning.Size = new System.Drawing.Size(483, 2);
      this.splitWarning.TabIndex = 22;
      this.splitWarning.TabStop = false;
      this.splitWarning.Visible = false;
      // 
      // imageList1
      // 
      this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList1.Images.SetKeyName(0, "opengame");
      this.imageList1.Images.SetKeyName(1, "closegame");
      this.imageList1.Images.SetKeyName(2, "rungame");
      this.imageList1.Images.SetKeyName(3, "print");
      this.imageList1.Images.SetKeyName(4, "newlogic");
      this.imageList1.Images.SetKeyName(5, "newpicture");
      this.imageList1.Images.SetKeyName(6, "newsound");
      this.imageList1.Images.SetKeyName(7, "newview");
      this.imageList1.Images.SetKeyName(8, "openlogic");
      this.imageList1.Images.SetKeyName(9, "openpicture");
      this.imageList1.Images.SetKeyName(10, "opensound");
      this.imageList1.Images.SetKeyName(11, "openview");
      this.imageList1.Images.SetKeyName(12, "importlogic");
      this.imageList1.Images.SetKeyName(13, "importpicture");
      this.imageList1.Images.SetKeyName(14, "importsound");
      this.imageList1.Images.SetKeyName(15, "importview");
      this.imageList1.Images.SetKeyName(16, "words");
      this.imageList1.Images.SetKeyName(17, "objects");
      this.imageList1.Images.SetKeyName(18, "saveres");
      this.imageList1.Images.SetKeyName(19, "addres");
      this.imageList1.Images.SetKeyName(20, "removeres");
      this.imageList1.Images.SetKeyName(21, "exportres");
      this.imageList1.Images.SetKeyName(22, "layout");
      this.imageList1.Images.SetKeyName(23, "menu");
      this.imageList1.Images.SetKeyName(24, "globals");
      this.imageList1.Images.SetKeyName(25, "help");
      // 
      // tmrNavList
      // 
      this.tmrNavList.Interval = 125;
      this.tmrNavList.Tick += new System.EventHandler(this.SplashTimer);
      // 
      // FolderDlg
      // 
      this.FolderDlg.ShowNewFolderButton = false;
      // 
      // imlPropButtons
      // 
      this.imlPropButtons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
      this.imlPropButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlPropButtons.ImageStream")));
      this.imlPropButtons.TransparentColor = System.Drawing.Color.Transparent;
      this.imlPropButtons.Images.SetKeyName(0, "dropdown_u.bmp");
      this.imlPropButtons.Images.SetKeyName(1, "dropover_u.bmp");
      this.imlPropButtons.Images.SetKeyName(2, "dropdialog_u.bmp");
      this.imlPropButtons.Images.SetKeyName(3, "dropdown_d.bmp");
      this.imlPropButtons.Images.SetKeyName(4, "dropover_d.bmp");
      this.imlPropButtons.Images.SetKeyName(5, "dropdialog_d.bmp");
      // 
      // frmMDIMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.ClientSize = new System.Drawing.Size(642, 378);
      this.Controls.Add(this.lstProperty);
      this.Controls.Add(this.picNavList);
      this.Controls.Add(this.splitWarning);
      this.Controls.Add(this.pnlWarnings);
      this.Controls.Add(this.splitResource);
      this.Controls.Add(this.pnlResources);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.statusStrip1);
      this.Controls.Add(this.menuStrip1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.IsMdiContainer = true;
      this.KeyPreview = true;
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "frmMDIMain";
      this.Text = "WinAGI GDS";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMDIMain_FormClosing);
      this.Load += new System.EventHandler(this.frmMDIMain_Load);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMDIMain_KeyDown);
      this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frmMDIMain_KeyPress);
      this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.frmMDIMain_PreviewKeyDown);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.contextMenuStrip1.ResumeLayout(false);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.pnlResources.ResumeLayout(false);
      this.splResource.Panel1.ResumeLayout(false);
      this.splResource.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splResource)).EndInit();
      this.splResource.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.picProperties)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.picNavList)).EndInit();
      this.pnlWarnings.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.fgWarnings)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private MenuStrip menuStrip1;
    private ContextMenuStrip contextMenuStrip1;
    private StatusStrip statusStrip1;
    private ToolStrip toolStrip1;
    private ToolStripStatusLabel CapsLockLabel;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem newToolStripMenuItem;
    private ToolStripMenuItem mnuROpen;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem saveAsToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripMenuItem printToolStripMenuItem;
    private ToolStripMenuItem mnuR;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem editToolStripMenuItem;
    private ToolStripMenuItem toolsToolStripMenuItem;
    private ToolStripMenuItem customizeToolStripMenuItem;
    private ToolStripMenuItem optionsToolStripMenuItem;
    private ToolStripMenuItem mnuHelp;
    private ToolStripMenuItem contentsToolStripMenuItem;
    private ToolStripMenuItem indexToolStripMenuItem;
    private ToolStripMenuItem searchToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator7;
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
    private ToolStripSeparator toolStripSeparator10;
    private ToolStripMenuItem mnuGRun;
    private ToolStripSeparator toolStripSeparator11;
    private ToolStripMenuItem mnuGProperties;
    private ToolStripSeparator toolStripSeparator12;
    private ToolStripMenuItem mnuGExit;
    private ToolStripMenuItem mnuResources;
    private ToolStripMenuItem mnuTools;
    private ToolStripMenuItem mnuWindow;
    private ToolStripMenuItem mnuWCascade;
    private ToolStripMenuItem mnuWTileV;
    private ToolStripMenuItem mnuWTileH;
    private ToolStripMenuItem mnuWArrange;
    private ToolStripMenuItem mnuWMinimize;
    private ToolStripSeparator toolStripSeparator8;
    private ToolStripMenuItem mnuWClose;
    private Splitter splitResource;
    private Splitter splitWarning;
    private ToolStripMenuItem toolStripMenuItem1;
    private ToolStripMenuItem mnuRNew;
    private ToolStripMenuItem mnuRSave;
    private ToolStripMenuItem mnuRExport;
    private ToolStripMenuItem mnuRIDDesc;
    private ToolStripMenuItem mnuRPrint;
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
    private ToolStripMenuItem mnuTLayout;
    private ToolStripMenuItem mnuTMenuEditor;
    private ToolStripMenuItem mnuTGlobals;
    private ToolStripMenuItem mnuReserved;
    private ToolStripMenuItem mnuTSnippets;
    private ToolStripMenuItem mnuTPalette;
    private ToolStripSeparator mnuTSep2;
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
    private ToolStripButton btnPrint;
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
    private VScrollBar fsbProperty;
    public ListBox lstProperty;
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
    private DataGridViewTextBoxColumn colResType;
    private DataGridViewTextBoxColumn colIDK;
    private DataGridViewTextBoxColumn colWarning;
    private DataGridViewTextBoxColumn colDesc;
    private DataGridViewTextBoxColumn colLogic;
    private DataGridViewTextBoxColumn colLIne;
    private DataGridViewTextBoxColumn colModule;
    private PictureBox picNavList;
    public FolderBrowserDialog FolderDlg;
    public PictureBox picProperties;
    private SplitContainer splResource;
    public TreeView tvwResources;
    public ListView lstResources;
    public ComboBox cmbResType;
    private SplitContainer splitContainer1;
    private PropertyGrid propertyGrid1;
  }
}

