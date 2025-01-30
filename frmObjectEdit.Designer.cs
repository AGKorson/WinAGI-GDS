
namespace WinAGI.Editor {
    partial class frmObjectEdit {
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmObjectEdit));
            Label1 = new System.Windows.Forms.Label();
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
            mnuRToggleEncrypt = new System.Windows.Forms.ToolStripMenuItem();
            mnuRAmigaOBJ = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            cmGrid = new System.Windows.Forms.ContextMenuStrip(components);
            mnuEUndo = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            mnuEDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuEClear = new System.Windows.Forms.ToolStripMenuItem();
            mnuEInsert = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            mnuEFind = new System.Windows.Forms.ToolStripMenuItem();
            mnuEFindAgain = new System.Windows.Forms.ToolStripMenuItem();
            mnuEReplace = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            mnuEFindInLogic = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditItem = new System.Windows.Forms.ToolStripMenuItem();
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
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            spStatus = new System.Windows.Forms.ToolStripStatusLabel();
            spCount = new System.Windows.Forms.ToolStripStatusLabel();
            spEncrypt = new System.Windows.Forms.ToolStripStatusLabel();
            txtMaxScreenObjs = new System.Windows.Forms.TextBox();
            fgObjects = new System.Windows.Forms.DataGridView();
            colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colRoom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            menuStrip1.SuspendLayout();
            cmGrid.SuspendLayout();
            cmCel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)fgObjects).BeginInit();
            SuspendLayout();
            // 
            // Label1
            // 
            Label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            Label1.AutoSize = true;
            Label1.Location = new System.Drawing.Point(542, 3);
            Label1.Name = "Label1";
            Label1.Size = new System.Drawing.Size(145, 15);
            Label1.TabIndex = 1;
            Label1.Text = "Maximum Screen Objects:";
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(691, 24);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRExport, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportLoopGIF, mnuRToggleEncrypt, mnuRAmigaOBJ });
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
            mnuRSave.Text = "&Save OBJECT";
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
            // mnuRToggleEncrypt
            // 
            mnuRToggleEncrypt.Checked = true;
            mnuRToggleEncrypt.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuRToggleEncrypt.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            mnuRToggleEncrypt.Name = "mnuRToggleEncrypt";
            mnuRToggleEncrypt.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.E;
            mnuRToggleEncrypt.Size = new System.Drawing.Size(274, 22);
            mnuRToggleEncrypt.Text = "En&crypt";
            mnuRToggleEncrypt.Click += mnuRToggleEncrypt_Click;
            // 
            // mnuRAmigaOBJ
            // 
            mnuRAmigaOBJ.Name = "mnuRAmigaOBJ";
            mnuRAmigaOBJ.Size = new System.Drawing.Size(274, 22);
            mnuRAmigaOBJ.Text = "Convert AMIGA Format to DOS";
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
            // cmGrid
            // 
            cmGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuEUndo, toolStripSeparator1, mnuEDelete, mnuEClear, mnuEInsert, toolStripSeparator2, mnuEFind, mnuEFindAgain, mnuEReplace, toolStripSeparator3, mnuEFindInLogic, mnuEditItem });
            cmGrid.Name = "cmEdit";
            cmGrid.Size = new System.Drawing.Size(215, 242);
            cmGrid.Closed += cmEdit_Closed;
            cmGrid.Opening += cmEdit_Opening;
            // 
            // mnuEUndo
            // 
            mnuEUndo.Name = "mnuEUndo";
            mnuEUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuEUndo.Size = new System.Drawing.Size(214, 22);
            mnuEUndo.Text = "Undo";
            mnuEUndo.Click += mnuEUndo_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(211, 6);
            // 
            // mnuEDelete
            // 
            mnuEDelete.Name = "mnuEDelete";
            mnuEDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuEDelete.Size = new System.Drawing.Size(214, 22);
            mnuEDelete.Text = "Delete Item";
            mnuEDelete.Click += mnuEDelete_Click;
            // 
            // mnuEClear
            // 
            mnuEClear.Name = "mnuEClear";
            mnuEClear.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete;
            mnuEClear.Size = new System.Drawing.Size(214, 22);
            mnuEClear.Text = "Clear List";
            mnuEClear.Click += mnuEClear_Click;
            // 
            // mnuEInsert
            // 
            mnuEInsert.Name = "mnuEInsert";
            mnuEInsert.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Insert;
            mnuEInsert.Size = new System.Drawing.Size(214, 22);
            mnuEInsert.Text = "Add Item";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(211, 6);
            // 
            // mnuEFind
            // 
            mnuEFind.Name = "mnuEFind";
            mnuEFind.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            mnuEFind.Size = new System.Drawing.Size(214, 22);
            mnuEFind.Text = "Find";
            mnuEFind.Click += mnuEFind_Click;
            // 
            // mnuEFindAgain
            // 
            mnuEFindAgain.Name = "mnuEFindAgain";
            mnuEFindAgain.ShortcutKeys = System.Windows.Forms.Keys.F3;
            mnuEFindAgain.Size = new System.Drawing.Size(214, 22);
            mnuEFindAgain.Text = "Find Again";
            mnuEFindAgain.Click += mnuEFindAgain_Click;
            // 
            // mnuEReplace
            // 
            mnuEReplace.Name = "mnuEReplace";
            mnuEReplace.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H;
            mnuEReplace.Size = new System.Drawing.Size(214, 22);
            mnuEReplace.Text = "Replace";
            mnuEReplace.Click += mnuEReplace_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(211, 6);
            // 
            // mnuEFindInLogic
            // 
            mnuEFindInLogic.Name = "mnuEFindInLogic";
            mnuEFindInLogic.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F;
            mnuEFindInLogic.Size = new System.Drawing.Size(214, 22);
            mnuEFindInLogic.Text = "Find in Logic";
            mnuEFindInLogic.Click += mnuEFindInLogic_Click;
            // 
            // mnuEditItem
            // 
            mnuEditItem.Name = "mnuEditItem";
            mnuEditItem.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Enter;
            mnuEditItem.Size = new System.Drawing.Size(214, 22);
            mnuEditItem.Text = "Edit Item";
            mnuEditItem.Click += mnuEditItem_Click;
            // 
            // cmCel
            // 
            cmCel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuCelUndo, mnuCelSep0, mnuCelCut, mnuCelCopy, mnuCelPaste, mnuCelDelete, mnuCelCharMap, mnuCelSep1, mnuCelSelectAll, mnuCelSep2, mnuCelCancel });
            cmCel.Name = "cmCel";
            cmCel.Size = new System.Drawing.Size(216, 198);
            cmCel.Closed += cmCel_Closed;
            cmCel.Opening += cmCel_Opening;
            // 
            // mnuCelUndo
            // 
            mnuCelUndo.Name = "mnuCelUndo";
            mnuCelUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuCelUndo.Size = new System.Drawing.Size(215, 22);
            mnuCelUndo.Text = "Undo";
            mnuCelUndo.Click += mnuCelUndo_Click;
            // 
            // mnuCelSep0
            // 
            mnuCelSep0.Name = "mnuCelSep0";
            mnuCelSep0.Size = new System.Drawing.Size(212, 6);
            // 
            // mnuCelCut
            // 
            mnuCelCut.Name = "mnuCelCut";
            mnuCelCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuCelCut.Size = new System.Drawing.Size(215, 22);
            mnuCelCut.Text = "Cut";
            mnuCelCut.Click += mnuCelCut_Click;
            // 
            // mnuCelCopy
            // 
            mnuCelCopy.Name = "mnuCelCopy";
            mnuCelCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuCelCopy.Size = new System.Drawing.Size(215, 22);
            mnuCelCopy.Text = "Copy";
            mnuCelCopy.Click += mnuCelCopy_Click;
            // 
            // mnuCelPaste
            // 
            mnuCelPaste.Name = "mnuCelPaste";
            mnuCelPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuCelPaste.Size = new System.Drawing.Size(215, 22);
            mnuCelPaste.Text = "Paste";
            mnuCelPaste.Click += mnuCelPaste_Click;
            // 
            // mnuCelDelete
            // 
            mnuCelDelete.Name = "mnuCelDelete";
            mnuCelDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuCelDelete.Size = new System.Drawing.Size(215, 22);
            mnuCelDelete.Text = "Delete";
            mnuCelDelete.Click += mnuCelDelete_Click;
            // 
            // mnuCelCharMap
            // 
            mnuCelCharMap.Name = "mnuCelCharMap";
            mnuCelCharMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert;
            mnuCelCharMap.Size = new System.Drawing.Size(215, 22);
            mnuCelCharMap.Text = "Character Map";
            mnuCelCharMap.Click += mnuCelCharMap_Click;
            // 
            // mnuCelSep1
            // 
            mnuCelSep1.Name = "mnuCelSep1";
            mnuCelSep1.Size = new System.Drawing.Size(212, 6);
            // 
            // mnuCelSelectAll
            // 
            mnuCelSelectAll.Name = "mnuCelSelectAll";
            mnuCelSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuCelSelectAll.Size = new System.Drawing.Size(215, 22);
            mnuCelSelectAll.Text = "Select All";
            mnuCelSelectAll.Click += mnuCelSelectAll_Click;
            // 
            // mnuCelSep2
            // 
            mnuCelSep2.Name = "mnuCelSep2";
            mnuCelSep2.Size = new System.Drawing.Size(212, 6);
            // 
            // mnuCelCancel
            // 
            mnuCelCancel.Name = "mnuCelCancel";
            mnuCelCancel.Size = new System.Drawing.Size(215, 22);
            mnuCelCancel.Text = "Cancel";
            mnuCelCancel.Click += mnuCelCancel_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Location = new System.Drawing.Point(41, 214);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            statusStrip1.Size = new System.Drawing.Size(719, 23);
            statusStrip1.TabIndex = 9;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.Visible = false;
            // 
            // spStatus
            // 
            spStatus.Name = "spStatus";
            spStatus.Size = new System.Drawing.Size(23, 23);
            // 
            // spCount
            // 
            spCount.Name = "spCount";
            spCount.Size = new System.Drawing.Size(23, 23);
            // 
            // spEncrypt
            // 
            spEncrypt.Name = "spEncrypt";
            spEncrypt.Size = new System.Drawing.Size(23, 23);
            // 
            // txtMaxScreenObjs
            // 
            txtMaxScreenObjs.BackColor = System.Drawing.SystemColors.Control;
            txtMaxScreenObjs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtMaxScreenObjs.Dock = System.Windows.Forms.DockStyle.Right;
            txtMaxScreenObjs.Location = new System.Drawing.Point(691, 0);
            txtMaxScreenObjs.MaxLength = 3;
            txtMaxScreenObjs.Name = "txtMaxScreenObjs";
            txtMaxScreenObjs.Size = new System.Drawing.Size(45, 23);
            txtMaxScreenObjs.TabIndex = 2;
            txtMaxScreenObjs.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            txtMaxScreenObjs.TextChanged += txtMaxScreenObjs_TextChanged;
            txtMaxScreenObjs.Enter += txtMaxScreenObjs_Enter;
            txtMaxScreenObjs.KeyDown += txtMaxScreenObjs_KeyDown;
            txtMaxScreenObjs.KeyPress += txtMaxScreenObjs_KeyPress;
            txtMaxScreenObjs.Leave += txtMaxScreenObjs_Leave;
            txtMaxScreenObjs.Validating += txtMaxScreenObjs_Validating;
            // 
            // fgObjects
            // 
            fgObjects.AllowUserToDeleteRows = false;
            fgObjects.AllowUserToResizeColumns = false;
            fgObjects.AllowUserToResizeRows = false;
            fgObjects.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            fgObjects.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            fgObjects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            fgObjects.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colIndex, colDescription, colRoom });
            fgObjects.ContextMenuStrip = cmGrid;
            fgObjects.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            fgObjects.Location = new System.Drawing.Point(0, 24);
            fgObjects.MultiSelect = false;
            fgObjects.Name = "fgObjects";
            fgObjects.RowHeadersVisible = false;
            fgObjects.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            fgObjects.ShowCellErrors = false;
            fgObjects.ShowCellToolTips = false;
            fgObjects.ShowRowErrors = false;
            fgObjects.Size = new System.Drawing.Size(736, 176);
            fgObjects.TabIndex = 0;
            fgObjects.CellDoubleClick += fgObjects_CellDoubleClick;
            fgObjects.CellFormatting += fgObjects_CellFormatting;
            fgObjects.CellMouseDown += fgObjects_CellMouseDown;
            fgObjects.CellMouseEnter += fgObjects_CellMouseEnter;
            fgObjects.CellMouseLeave += fgObjects_CellMouseLeave;
            fgObjects.CellValidated += fgObjects_CellValidated;
            fgObjects.CellValidating += fgObjects_CellValidating;
            fgObjects.EditingControlShowing += fgObjects_EditingControlShowing;
            fgObjects.RowsAdded += fgObjects_RowsAdded;
            fgObjects.KeyDown += fgObjects_KeyDown;
            // 
            // colIndex
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle1.Format = "0'. '";
            colIndex.DefaultCellStyle = dataGridViewCellStyle1;
            colIndex.FillWeight = 80F;
            colIndex.HeaderText = "Item #";
            colIndex.Name = "colIndex";
            colIndex.ReadOnly = true;
            colIndex.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // colDescription
            // 
            colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            colDescription.HeaderText = "Item Description";
            colDescription.Name = "colDescription";
            colDescription.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // colRoom
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            colRoom.DefaultCellStyle = dataGridViewCellStyle2;
            colRoom.FillWeight = 80F;
            colRoom.HeaderText = "Room #";
            colRoom.Name = "colRoom";
            colRoom.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // frmObjectEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(736, 192);
            Controls.Add(Label1);
            Controls.Add(menuStrip1);
            Controls.Add(statusStrip1);
            Controls.Add(txtMaxScreenObjs);
            Controls.Add(fgObjects);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmObjectEdit";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmObjectEdit";
            FormClosing += frmObjectEdit_FormClosing;
            FormClosed += frmObjectEdit_FormClosed;
            Load += frmObjectEdit_Load;
            Leave += frmObjectEdit_Leave;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            cmGrid.ResumeLayout(false);
            cmCel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)fgObjects).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label Label1;
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
        private System.Windows.Forms.ToolStripMenuItem mnuRToggleEncrypt;
        private System.Windows.Forms.ToolStripMenuItem mnuRAmigaOBJ;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;

        private System.Windows.Forms.ContextMenuStrip cmGrid;
        private System.Windows.Forms.ToolStripMenuItem mnuEUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuEDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuEClear;
        private System.Windows.Forms.ToolStripMenuItem mnuEInsert;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuEFind;
        private System.Windows.Forms.ToolStripMenuItem mnuEFindAgain;
        private System.Windows.Forms.ToolStripMenuItem mnuEReplace;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuEFindInLogic;

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
        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel spStatus;
        private System.Windows.Forms.ToolStripStatusLabel spCount;
        private System.Windows.Forms.ToolStripStatusLabel spEncrypt;
        private System.Windows.Forms.TextBox txtMaxScreenObjs;
        private System.Windows.Forms.DataGridView fgObjects;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRoom;
        private System.Windows.Forms.ToolStripMenuItem mnuEditItem;
    }
}