
namespace WinAGI.Editor {
    partial class frmMenuEdit {
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMenuEdit));
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            mnuMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuInsert = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuReset = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            mnuResource = new System.Windows.Forms.ToolStripMenuItem();
            mnuROpen = new System.Windows.Forms.ToolStripMenuItem();
            mnuUpdateLogic = new System.Windows.Forms.ToolStripMenuItem();
            mnuSaveAsDefault = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            mnuRRemove = new System.Windows.Forms.ToolStripMenuItem();
            mnuRRenumber = new System.Windows.Forms.ToolStripMenuItem();
            mnuRIDDesc = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            mnuRCompileLogic = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSavePic = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportGif = new System.Windows.Forms.ToolStripMenuItem();
            mnuBackground = new System.Windows.Forms.ToolStripMenuItem();
            mnuHotKeys = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            dgProps = new System.Windows.Forms.DataGridView();
            propName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            propValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            tvwMenu = new System.Windows.Forms.TreeView();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnInsert = new System.Windows.Forms.ToolStripButton();
            btnDelete = new System.Windows.Forms.ToolStripButton();
            btnMoveUp = new System.Windows.Forms.ToolStripButton();
            btnMoveDown = new System.Windows.Forms.ToolStripButton();
            btnCopyMenu = new System.Windows.Forms.ToolStripButton();
            picBackground = new System.Windows.Forms.PictureBox();
            cmCel = new System.Windows.Forms.ContextMenuStrip(components);
            mnuCelUndo = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelSep0 = new System.Windows.Forms.ToolStripSeparator();
            mnuCelCut = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelCharMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelSep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuCelSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelSep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuCelCancel = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgProps).BeginInit();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBackground).BeginInit();
            cmCel.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuMoveUp, mnuMoveDown, mnuDelete, mnuInsert, mnuCopy, mnuReset });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(214, 136);
            // 
            // mnuMoveUp
            // 
            mnuMoveUp.Name = "mnuMoveUp";
            mnuMoveUp.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U;
            mnuMoveUp.Size = new System.Drawing.Size(213, 22);
            mnuMoveUp.Text = "Move Menu Up";
            mnuMoveUp.Click += mnuMoveUp_Click;
            // 
            // mnuMoveDown
            // 
            mnuMoveDown.Name = "mnuMoveDown";
            mnuMoveDown.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D;
            mnuMoveDown.Size = new System.Drawing.Size(213, 22);
            mnuMoveDown.Text = "Move Menu Down";
            mnuMoveDown.Click += mnuMoveDown_Click;
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuDelete.Size = new System.Drawing.Size(213, 22);
            mnuDelete.Text = "Delete Menu";
            mnuDelete.Click += mnuDelete_Click;
            // 
            // mnuInsert
            // 
            mnuInsert.Name = "mnuInsert";
            mnuInsert.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            mnuInsert.Size = new System.Drawing.Size(213, 22);
            mnuInsert.Text = "Insert Menu";
            mnuInsert.Click += mnuInsert_Click;
            // 
            // mnuCopy
            // 
            mnuCopy.Name = "mnuCopy";
            mnuCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuCopy.Size = new System.Drawing.Size(213, 22);
            mnuCopy.Text = "Copy to Clipboard";
            mnuCopy.Click += mnuCopy_Click;
            // 
            // mnuReset
            // 
            mnuReset.Name = "mnuReset";
            mnuReset.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuReset.Size = new System.Drawing.Size(213, 22);
            mnuReset.Text = "Reset to Default";
            mnuReset.Click += mnuReset_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(802, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpen, mnuUpdateLogic, mnuSaveAsDefault, toolStripSeparator1, mnuRRemove, mnuRRenumber, mnuRIDDesc, toolStripSeparator2, mnuRCompileLogic, mnuRSavePic, mnuExportGif, mnuBackground, mnuHotKeys });
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
            mnuROpen.Size = new System.Drawing.Size(229, 22);
            mnuROpen.Text = "openres";
            // 
            // mnuUpdateLogic
            // 
            mnuUpdateLogic.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuUpdateLogic.MergeIndex = 4;
            mnuUpdateLogic.Name = "mnuUpdateLogic";
            mnuUpdateLogic.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U;
            mnuUpdateLogic.Size = new System.Drawing.Size(229, 22);
            mnuUpdateLogic.Text = "Update Source Logic";
            mnuUpdateLogic.Click += mnuUpdateLogic_Click;
            // 
            // mnuSaveAsDefault
            // 
            mnuSaveAsDefault.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuSaveAsDefault.MergeIndex = 5;
            mnuSaveAsDefault.Name = "mnuSaveAsDefault";
            mnuSaveAsDefault.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuSaveAsDefault.Size = new System.Drawing.Size(229, 22);
            mnuSaveAsDefault.Text = "Save As Default Menu";
            mnuSaveAsDefault.Click += mnuRSaveDefault_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.MergeAction = System.Windows.Forms.MergeAction.Remove;
            toolStripSeparator1.MergeIndex = 6;
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(226, 6);
            // 
            // mnuRRemove
            // 
            mnuRRemove.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRemove.MergeIndex = 6;
            mnuRRemove.Name = "mnuRRemove";
            mnuRRemove.Size = new System.Drawing.Size(229, 22);
            mnuRRemove.Text = "remove";
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRenumber.MergeIndex = 6;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.Size = new System.Drawing.Size(229, 22);
            mnuRRenumber.Text = "renumber";
            // 
            // mnuRIDDesc
            // 
            mnuRIDDesc.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRIDDesc.MergeIndex = 6;
            mnuRIDDesc.Name = "mnuRIDDesc";
            mnuRIDDesc.Size = new System.Drawing.Size(229, 22);
            mnuRIDDesc.Text = "iddesc";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.MergeAction = System.Windows.Forms.MergeAction.Remove;
            toolStripSeparator2.MergeIndex = 6;
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(226, 6);
            // 
            // mnuRCompileLogic
            // 
            mnuRCompileLogic.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRCompileLogic.MergeIndex = 6;
            mnuRCompileLogic.Name = "mnuRCompileLogic";
            mnuRCompileLogic.Size = new System.Drawing.Size(229, 22);
            mnuRCompileLogic.Text = "compile";
            // 
            // mnuRSavePic
            // 
            mnuRSavePic.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRSavePic.MergeIndex = 6;
            mnuRSavePic.Name = "mnuRSavePic";
            mnuRSavePic.Size = new System.Drawing.Size(229, 22);
            mnuRSavePic.Text = "savepic";
            // 
            // mnuExportGif
            // 
            mnuExportGif.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuExportGif.MergeIndex = 6;
            mnuExportGif.Name = "mnuExportGif";
            mnuExportGif.Size = new System.Drawing.Size(229, 22);
            mnuExportGif.Text = "exportgif";
            // 
            // mnuBackground
            // 
            mnuBackground.Name = "mnuBackground";
            mnuBackground.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.B;
            mnuBackground.Size = new System.Drawing.Size(229, 22);
            mnuBackground.Text = "Change Background";
            mnuBackground.Click += mnuBackground_Click;
            // 
            // mnuHotKeys
            // 
            mnuHotKeys.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuHotKeys.Name = "mnuHotKeys";
            mnuHotKeys.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H;
            mnuHotKeys.Size = new System.Drawing.Size(229, 22);
            mnuHotKeys.Text = "Right Align Hot Keys";
            mnuHotKeys.Click += mnuHotKeys_Click;
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
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new System.Drawing.Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(dgProps);
            splitContainer1.Panel1.Controls.Add(tvwMenu);
            splitContainer1.Panel1.Controls.Add(toolStrip1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(picBackground);
            splitContainer1.Size = new System.Drawing.Size(802, 376);
            splitContainer1.SplitterDistance = 184;
            splitContainer1.TabIndex = 2;
            // 
            // dgProps
            // 
            dgProps.AllowUserToAddRows = false;
            dgProps.AllowUserToDeleteRows = false;
            dgProps.AllowUserToResizeColumns = false;
            dgProps.AllowUserToResizeRows = false;
            dgProps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgProps.ColumnHeadersVisible = false;
            dgProps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { propName, propValue });
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgProps.DefaultCellStyle = dataGridViewCellStyle3;
            dgProps.Dock = System.Windows.Forms.DockStyle.Bottom;
            dgProps.GridColor = System.Drawing.SystemColors.Control;
            dgProps.Location = new System.Drawing.Point(0, 328);
            dgProps.MultiSelect = false;
            dgProps.Name = "dgProps";
            dgProps.RowHeadersVisible = false;
            dgProps.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgProps.RowTemplate.Height = 20;
            dgProps.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            dgProps.ScrollBars = System.Windows.Forms.ScrollBars.None;
            dgProps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgProps.ShowCellErrors = false;
            dgProps.ShowCellToolTips = false;
            dgProps.ShowEditingIcon = false;
            dgProps.ShowRowErrors = false;
            dgProps.Size = new System.Drawing.Size(184, 48);
            dgProps.TabIndex = 3;
            dgProps.CellClick += dgProps_CellClick;
            dgProps.CellValidated += dgProps_CellValidated;
            dgProps.CellValidating += dgProps_CellValidating;
            dgProps.EditingControlShowing += dgProps_EditingControlShowing;
            // 
            // propName
            // 
            propName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            propName.DefaultCellStyle = dataGridViewCellStyle1;
            propName.FillWeight = 80F;
            propName.HeaderText = "name";
            propName.Name = "propName";
            propName.ReadOnly = true;
            propName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            propName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // propValue
            // 
            propValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            propValue.DefaultCellStyle = dataGridViewCellStyle2;
            propValue.FillWeight = 120F;
            propValue.HeaderText = "value";
            propValue.Name = "propValue";
            propValue.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            propValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tvwMenu
            // 
            tvwMenu.ContextMenuStrip = contextMenuStrip1;
            tvwMenu.Dock = System.Windows.Forms.DockStyle.Top;
            tvwMenu.HideSelection = false;
            tvwMenu.Location = new System.Drawing.Point(0, 31);
            tvwMenu.Name = "tvwMenu";
            tvwMenu.Size = new System.Drawing.Size(184, 293);
            tvwMenu.TabIndex = 1;
            tvwMenu.BeforeCollapse += tvwMenu_BeforeCollapse;
            tvwMenu.AfterSelect += tvwMenu_AfterSelect;
            tvwMenu.DoubleClick += tvwMenu_DoubleClick;
            tvwMenu.MouseDown += tvwMenu_MouseDown;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnInsert, btnDelete, btnMoveUp, btnMoveDown, btnCopyMenu });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(184, 31);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnInsert
            // 
            btnInsert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnInsert.Image = (System.Drawing.Image)resources.GetObject("btnInsert.Image");
            btnInsert.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new System.Drawing.Size(28, 28);
            btnInsert.Text = "Insert Menu";
            btnInsert.Click += mnuInsert_Click;
            // 
            // btnDelete
            // 
            btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnDelete.Image = (System.Drawing.Image)resources.GetObject("btnDelete.Image");
            btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(28, 28);
            btnDelete.Text = "Delete Menu";
            btnDelete.Click += mnuDelete_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnMoveUp.Image = (System.Drawing.Image)resources.GetObject("btnMoveUp.Image");
            btnMoveUp.ImageTransparentColor = System.Drawing.Color.Silver;
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new System.Drawing.Size(28, 28);
            btnMoveUp.Text = "Move Menu Up";
            btnMoveUp.Click += mnuMoveUp_Click;
            // 
            // btnMoveDown
            // 
            btnMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnMoveDown.Image = (System.Drawing.Image)resources.GetObject("btnMoveDown.Image");
            btnMoveDown.ImageTransparentColor = System.Drawing.Color.Silver;
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new System.Drawing.Size(28, 28);
            btnMoveDown.Text = "Move Menu Down";
            btnMoveDown.Click += mnuMoveDown_Click;
            // 
            // btnCopyMenu
            // 
            btnCopyMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCopyMenu.Image = (System.Drawing.Image)resources.GetObject("btnCopyMenu.Image");
            btnCopyMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCopyMenu.Name = "btnCopyMenu";
            btnCopyMenu.Size = new System.Drawing.Size(28, 28);
            btnCopyMenu.Text = "Copy Menu";
            btnCopyMenu.Click += mnuCopy_Click;
            // 
            // picBackground
            // 
            picBackground.ContextMenuStrip = contextMenuStrip1;
            picBackground.Location = new System.Drawing.Point(0, 0);
            picBackground.Name = "picBackground";
            picBackground.Size = new System.Drawing.Size(611, 400);
            picBackground.TabIndex = 0;
            picBackground.TabStop = false;
            picBackground.Paint += picBackground_Paint;
            picBackground.DoubleClick += picBackground_DoubleClick;
            picBackground.MouseDown += picBackground_MouseDown;
            // 
            // cmCel
            // 
            cmCel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuCelUndo, mnuCelSep0, mnuCelCut, mnuCelCopy, mnuCelPaste, mnuCelDelete, mnuCelCharMap, mnuCelSep1, mnuCelSelectAll, mnuCelSep2, mnuCelCancel });
            cmCel.Name = "cmCel";
            cmCel.Size = new System.Drawing.Size(225, 198);
            cmCel.Closed += cmCel_Closed;
            cmCel.Opening += cmCel_Opening;
            // 
            // mnuCelUndo
            // 
            mnuCelUndo.Name = "mnuCelUndo";
            mnuCelUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuCelUndo.Size = new System.Drawing.Size(224, 22);
            mnuCelUndo.Text = "Undo";
            mnuCelUndo.Click += mnuCelUndo_Click;
            // 
            // mnuCelSep0
            // 
            mnuCelSep0.Name = "mnuCelSep0";
            mnuCelSep0.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuCelCut
            // 
            mnuCelCut.Name = "mnuCelCut";
            mnuCelCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuCelCut.Size = new System.Drawing.Size(224, 22);
            mnuCelCut.Text = "Cut";
            mnuCelCut.Click += mnuCelCut_Click;
            // 
            // mnuCelCopy
            // 
            mnuCelCopy.Name = "mnuCelCopy";
            mnuCelCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuCelCopy.Size = new System.Drawing.Size(224, 22);
            mnuCelCopy.Text = "Copy";
            mnuCelCopy.Click += mnuCelCopy_Click;
            // 
            // mnuCelPaste
            // 
            mnuCelPaste.Name = "mnuCelPaste";
            mnuCelPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuCelPaste.Size = new System.Drawing.Size(224, 22);
            mnuCelPaste.Text = "Paste";
            mnuCelPaste.Click += mnuCelPaste_Click;
            // 
            // mnuCelDelete
            // 
            mnuCelDelete.Name = "mnuCelDelete";
            mnuCelDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuCelDelete.Size = new System.Drawing.Size(224, 22);
            mnuCelDelete.Text = "Delete";
            mnuCelDelete.Click += mnuCelDelete_Click;
            // 
            // mnuCelCharMap
            // 
            mnuCelCharMap.Name = "mnuCelCharMap";
            mnuCelCharMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert;
            mnuCelCharMap.Size = new System.Drawing.Size(224, 22);
            mnuCelCharMap.Text = "Character Map...";
            mnuCelCharMap.Click += mnuCelCharMap_Click;
            // 
            // mnuCelSep1
            // 
            mnuCelSep1.Name = "mnuCelSep1";
            mnuCelSep1.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuCelSelectAll
            // 
            mnuCelSelectAll.Name = "mnuCelSelectAll";
            mnuCelSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuCelSelectAll.Size = new System.Drawing.Size(224, 22);
            mnuCelSelectAll.Text = "Select All";
            mnuCelSelectAll.Click += mnuCelSelectAll_Click;
            // 
            // mnuCelSep2
            // 
            mnuCelSep2.Name = "mnuCelSep2";
            mnuCelSep2.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuCelCancel
            // 
            mnuCelCancel.Name = "mnuCelCancel";
            mnuCelCancel.ShowShortcutKeys = false;
            mnuCelCancel.Size = new System.Drawing.Size(224, 22);
            mnuCelCancel.Text = "Cancel                     Esc";
            mnuCelCancel.Click += mnuCelCancel_Click;
            // 
            // frmMenuEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(802, 400);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "frmMenuEdit";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "frmMenuEdit";
            FormClosing += frmMenuEdit_FormClosing;
            FormClosed += frmMenuEdit_FormClosed;
            Load += frmMenuEdit_Load;
            Leave += frmMenuEdit_Leave;
            contextMenuStrip1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgProps).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picBackground).EndInit();
            cmCel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuResource;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuROpen;
        private System.Windows.Forms.ToolStripMenuItem mnuRRemove;
        private System.Windows.Forms.ToolStripMenuItem mnuRRenumber;
        private System.Windows.Forms.ToolStripMenuItem mnuRIDDesc;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuRCompileLogic;
        private System.Windows.Forms.ToolStripMenuItem mnuRSavePic;
        private System.Windows.Forms.ToolStripMenuItem mnuExportGif;
        private System.Windows.Forms.ToolStripMenuItem mnuBackground;
        private System.Windows.Forms.ToolStripMenuItem mnuHotKeys;
        private System.Windows.Forms.ToolStripMenuItem mnuReset;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuInsert;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuMoveUp;
        private System.Windows.Forms.ToolStripMenuItem mnuMoveDown;
        internal System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnInsert;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripButton btnMoveUp;
        private System.Windows.Forms.ToolStripButton btnMoveDown;
        private System.Windows.Forms.ToolStripButton btnCopyMenu;
        private System.Windows.Forms.TreeView tvwMenu;
        private System.Windows.Forms.PictureBox picBackground;
        private System.Windows.Forms.ToolStripMenuItem mnuUpdateLogic;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveAsDefault;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgProps;
        private System.Windows.Forms.DataGridViewTextBoxColumn propName;
        private System.Windows.Forms.DataGridViewTextBoxColumn propValue;
        private System.Windows.Forms.ContextMenuStrip cmCel;
        private System.Windows.Forms.ToolStripMenuItem mnuCelUndo;
        private System.Windows.Forms.ToolStripSeparator mnuCelSep0;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCut;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuCelPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuCelDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCharMap;
        private System.Windows.Forms.ToolStripSeparator mnuCelSep1;
        private System.Windows.Forms.ToolStripMenuItem mnuCelSelectAll;
        private System.Windows.Forms.ToolStripSeparator mnuCelSep2;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCancel;
    }
}