
namespace WinAGI.Editor {
    partial class frmGlobals {
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGlobals));
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            spStatus = new System.Windows.Forms.ToolStripStatusLabel();
            globalsgrid = new System.Windows.Forms.DataGridView();
            colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDefault = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colName = new DefineColumn();
            colValue = new DefineColumn();
            colComment = new DefineColumn();
            NameCheck = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ValueCheck = new System.Windows.Forms.DataGridViewTextBoxColumn();
            cmGrid = new System.Windows.Forms.ContextMenuStrip(components);
            mnuEUndo = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep0 = new System.Windows.Forms.ToolStripSeparator();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuEDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuEClear = new System.Windows.Forms.ToolStripMenuItem();
            mnuEInsert = new System.Windows.Forms.ToolStripMenuItem();
            mnuESelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuEFindInLogics = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            mnuResource = new System.Windows.Forms.ToolStripMenuItem();
            mnuROpenRes = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSave = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            mnuRInGame = new System.Windows.Forms.ToolStripMenuItem();
            mnuRRenumber = new System.Windows.Forms.ToolStripMenuItem();
            mnuRProperties = new System.Windows.Forms.ToolStripMenuItem();
            mnuRCompile = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSavePicImage = new System.Windows.Forms.ToolStripMenuItem();
            mnuRExportLoopGIF = new System.Windows.Forms.ToolStripMenuItem();
            mnuRAddFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            cmCel = new System.Windows.Forms.ContextMenuStrip(components);
            mnuCelUndo = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelSep0 = new System.Windows.Forms.ToolStripSeparator();
            mnuCelCut = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelSep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuCelSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelSep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuCelCancel = new System.Windows.Forms.ToolStripMenuItem();
            mnuCelCharMap = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)globalsgrid).BeginInit();
            cmGrid.SuspendLayout();
            menuStrip1.SuspendLayout();
            cmCel.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { spStatus });
            statusStrip1.Location = new System.Drawing.Point(41, 214);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            statusStrip1.Size = new System.Drawing.Size(719, 23);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.Visible = false;
            // 
            // spStatus
            // 
            spStatus.MergeAction = System.Windows.Forms.MergeAction.Replace;
            spStatus.MergeIndex = 0;
            spStatus.Name = "spStatus";
            spStatus.Size = new System.Drawing.Size(705, 18);
            spStatus.Spring = true;
            spStatus.Text = "globals edit status";
            spStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // globalsgrid
            // 
            globalsgrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.ControlLight;
            globalsgrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            globalsgrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            globalsgrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            globalsgrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            globalsgrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colType, colDefault, colName, colValue, colComment, NameCheck, ValueCheck });
            globalsgrid.ContextMenuStrip = cmGrid;
            globalsgrid.Dock = System.Windows.Forms.DockStyle.Fill;
            globalsgrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            globalsgrid.Location = new System.Drawing.Point(0, 0);
            globalsgrid.Name = "globalsgrid";
            globalsgrid.RowHeadersWidth = 24;
            globalsgrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            globalsgrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            globalsgrid.ShowCellErrors = false;
            globalsgrid.ShowCellToolTips = false;
            globalsgrid.Size = new System.Drawing.Size(765, 359);
            globalsgrid.TabIndex = 9;
            globalsgrid.CellBeginEdit += globalsgrid_CellBeginEdit;
            globalsgrid.CellDoubleClick += globalsgrid_CellDoubleClick;
            globalsgrid.CellFormatting += globalsgrid_CellFormatting;
            globalsgrid.CellMouseDown += globalsgrid_CellMouseDown;
            globalsgrid.CellMouseEnter += globalsgrid_CellMouseEnter;
            globalsgrid.CellMouseLeave += globalsgrid_CellMouseLeave;
            globalsgrid.CellValidating += globalsgrid_CellValidating;
            globalsgrid.EditingControlShowing += globalsgrid_EditingControlShowing;
            globalsgrid.RowsAdded += globalsgrid_RowsAdded;
            globalsgrid.Scroll += globalsgrid_Scroll;
            globalsgrid.SelectionChanged += globalsgrid_SelectionChanged;
            globalsgrid.KeyDown += globalsgrid_KeyDown;
            // 
            // colType
            // 
            colType.HeaderText = "type";
            colType.Name = "colType";
            colType.Visible = false;
            // 
            // colDefault
            // 
            colDefault.HeaderText = "default";
            colDefault.Name = "colDefault";
            colDefault.Visible = false;
            // 
            // colName
            // 
            colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            colName.FillWeight = 45F;
            colName.HeaderText = "Name";
            colName.Name = "colName";
            colName.Width = 200;
            // 
            // colValue
            // 
            colValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            colValue.FillWeight = 65F;
            colValue.HeaderText = "Value";
            colValue.Name = "colValue";
            colValue.Width = 250;
            // 
            // colComment
            // 
            colComment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            colComment.FillWeight = 30F;
            colComment.HeaderText = "Comment";
            colComment.Name = "colComment";
            // 
            // NameCheck
            // 
            NameCheck.HeaderText = "namecheck";
            NameCheck.Name = "NameCheck";
            NameCheck.Visible = false;
            // 
            // ValueCheck
            // 
            ValueCheck.HeaderText = "valuecheck";
            ValueCheck.Name = "ValueCheck";
            ValueCheck.Visible = false;
            // 
            // cmGrid
            // 
            cmGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuEUndo, mnuESep0, mnuECut, mnuECopy, mnuEPaste, mnuEDelete, mnuEClear, mnuEInsert, mnuESelectAll, mnuESep1, mnuEFindInLogics });
            cmGrid.Name = "contextMenuStrip1";
            cmGrid.Size = new System.Drawing.Size(220, 214);
            cmGrid.Closed += cmGrid_Closed;
            cmGrid.Opening += cmGrid_Opening;
            // 
            // mnuEUndo
            // 
            mnuEUndo.Name = "mnuEUndo";
            mnuEUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuEUndo.Size = new System.Drawing.Size(219, 22);
            mnuEUndo.Text = "Undo";
            mnuEUndo.Click += mnuEUndo_Click;
            // 
            // mnuESep0
            // 
            mnuESep0.Name = "mnuESep0";
            mnuESep0.Size = new System.Drawing.Size(216, 6);
            // 
            // mnuECut
            // 
            mnuECut.Name = "mnuECut";
            mnuECut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuECut.Size = new System.Drawing.Size(219, 22);
            mnuECut.Text = "Cut";
            mnuECut.Click += mnuECut_Click;
            // 
            // mnuECopy
            // 
            mnuECopy.Name = "mnuECopy";
            mnuECopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuECopy.Size = new System.Drawing.Size(219, 22);
            mnuECopy.Text = "Copy";
            mnuECopy.Click += mnuECopy_Click;
            // 
            // mnuEPaste
            // 
            mnuEPaste.Name = "mnuEPaste";
            mnuEPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuEPaste.Size = new System.Drawing.Size(219, 22);
            mnuEPaste.Text = "Paste";
            mnuEPaste.Click += mnuEPaste_Click;
            // 
            // mnuEDelete
            // 
            mnuEDelete.Name = "mnuEDelete";
            mnuEDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuEDelete.Size = new System.Drawing.Size(219, 22);
            mnuEDelete.Text = "Delete";
            mnuEDelete.Click += mnuEDelete_Click;
            // 
            // mnuEClear
            // 
            mnuEClear.Name = "mnuEClear";
            mnuEClear.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete;
            mnuEClear.Size = new System.Drawing.Size(219, 22);
            mnuEClear.Text = "Clear List";
            mnuEClear.Click += mnuEClear_Click;
            // 
            // mnuEInsert
            // 
            mnuEInsert.Name = "mnuEInsert";
            mnuEInsert.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Insert;
            mnuEInsert.Size = new System.Drawing.Size(219, 22);
            mnuEInsert.Text = "Insert Row";
            mnuEInsert.Click += mnuEInsert_Click;
            // 
            // mnuESelectAll
            // 
            mnuESelectAll.Name = "mnuESelectAll";
            mnuESelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuESelectAll.Size = new System.Drawing.Size(219, 22);
            mnuESelectAll.Text = "Select All";
            mnuESelectAll.Click += mnuESelectAll_Click;
            // 
            // mnuESep1
            // 
            mnuESep1.Name = "mnuESep1";
            mnuESep1.Size = new System.Drawing.Size(216, 6);
            // 
            // mnuEFindInLogics
            // 
            mnuEFindInLogics.Name = "mnuEFindInLogics";
            mnuEFindInLogics.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F;
            mnuEFindInLogics.Size = new System.Drawing.Size(219, 22);
            mnuEFindInLogics.Text = "Find in Logics";
            mnuEFindInLogics.Click += mnuEFindInLogics_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(765, 24);
            menuStrip1.TabIndex = 10;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpenRes, mnuRSave, mnuRSaveAs, mnuRInGame, mnuRRenumber, mnuRProperties, mnuRCompile, mnuRSavePicImage, mnuRExportLoopGIF, mnuRAddFile });
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
            mnuROpenRes.Size = new System.Drawing.Size(193, 22);
            mnuROpenRes.Text = "open res";
            // 
            // mnuRSave
            // 
            mnuRSave.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSave.MergeIndex = 4;
            mnuRSave.Name = "mnuRSave";
            mnuRSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuRSave.Size = new System.Drawing.Size(193, 22);
            mnuRSave.Text = "Save";
            mnuRSave.Click += mnuRSave_Click;
            // 
            // mnuRSaveAs
            // 
            mnuRSaveAs.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSaveAs.MergeIndex = 5;
            mnuRSaveAs.Name = "mnuRSaveAs";
            mnuRSaveAs.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            mnuRSaveAs.Size = new System.Drawing.Size(193, 22);
            mnuRSaveAs.Text = "Save As...";
            mnuRSaveAs.Click += mnuRExport_Click;
            // 
            // mnuRInGame
            // 
            mnuRInGame.Enabled = false;
            mnuRInGame.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRInGame.MergeIndex = 7;
            mnuRInGame.Name = "mnuRInGame";
            mnuRInGame.Size = new System.Drawing.Size(193, 22);
            mnuRInGame.Text = "Add to Game";
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRenumber.MergeIndex = 8;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.Size = new System.Drawing.Size(193, 22);
            mnuRRenumber.Text = "renumber";
            // 
            // mnuRProperties
            // 
            mnuRProperties.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRProperties.MergeIndex = 8;
            mnuRProperties.Name = "mnuRProperties";
            mnuRProperties.Size = new System.Drawing.Size(193, 22);
            mnuRProperties.Text = "id-desc";
            // 
            // mnuRCompile
            // 
            mnuRCompile.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRCompile.MergeIndex = 9;
            mnuRCompile.Name = "mnuRCompile";
            mnuRCompile.Size = new System.Drawing.Size(193, 22);
            mnuRCompile.Text = "compile";
            // 
            // mnuRSavePicImage
            // 
            mnuRSavePicImage.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRSavePicImage.MergeIndex = 9;
            mnuRSavePicImage.Name = "mnuRSavePicImage";
            mnuRSavePicImage.Size = new System.Drawing.Size(193, 22);
            mnuRSavePicImage.Text = "save pic image";
            // 
            // mnuRExportLoopGIF
            // 
            mnuRExportLoopGIF.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRExportLoopGIF.MergeIndex = 9;
            mnuRExportLoopGIF.Name = "mnuRExportLoopGIF";
            mnuRExportLoopGIF.Size = new System.Drawing.Size(193, 22);
            mnuRExportLoopGIF.Text = "export loop gif";
            // 
            // mnuRAddFile
            // 
            mnuRAddFile.Name = "mnuRAddFile";
            mnuRAddFile.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F;
            mnuRAddFile.Size = new System.Drawing.Size(193, 22);
            mnuRAddFile.Text = "Add From File...";
            mnuRAddFile.Click += mnuRAddFile_Click;
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
            // cmCel
            // 
            cmCel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuCelUndo, mnuCelSep0, mnuCelCut, mnuCelCopy, mnuCelPaste, mnuCelDelete, mnuCelCharMap, mnuCelSep1, mnuCelSelectAll, mnuCelSep2, mnuCelCancel });
            cmCel.Name = "cmCel";
            cmCel.Size = new System.Drawing.Size(225, 220);
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
            // mnuCelCharMap
            // 
            mnuCelCharMap.Name = "mnuCelCharMap";
            mnuCelCharMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert;
            mnuCelCharMap.Size = new System.Drawing.Size(224, 22);
            mnuCelCharMap.Text = "Character Map...";
            mnuCelCharMap.Click += mnuCelCharMap_Click;
            // 
            // frmGlobals
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(765, 359);
            Controls.Add(menuStrip1);
            Controls.Add(globalsgrid);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmGlobals";
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmGlobals";
            FormClosing += frmGlobals_FormClosing;
            FormClosed += frmGlobals_FormClosed;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)globalsgrid).EndInit();
            cmGrid.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            cmCel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel spStatus;
        private System.Windows.Forms.DataGridView globalsgrid;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuResource;
        private System.Windows.Forms.ToolStripMenuItem mnuROpenRes;
        private System.Windows.Forms.ToolStripMenuItem mnuRSave;
        private System.Windows.Forms.ToolStripMenuItem mnuRSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuRInGame;
        private System.Windows.Forms.ToolStripMenuItem mnuRRenumber;
        private System.Windows.Forms.ToolStripMenuItem mnuRProperties;
        private System.Windows.Forms.ToolStripMenuItem mnuRCompile;
        private System.Windows.Forms.ToolStripMenuItem mnuRSavePicImage;
        private System.Windows.Forms.ToolStripMenuItem mnuRExportLoopGIF;
        private System.Windows.Forms.ToolStripMenuItem mnuRAddFile;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ContextMenuStrip cmGrid;
        private System.Windows.Forms.ToolStripMenuItem mnuEUndo;
        private System.Windows.Forms.ToolStripSeparator mnuESep0;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuEDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuEClear;
        private System.Windows.Forms.ToolStripMenuItem mnuEInsert;
        private System.Windows.Forms.ToolStripMenuItem mnuESelectAll;
        private System.Windows.Forms.ToolStripSeparator mnuESep1;
        private System.Windows.Forms.ToolStripMenuItem mnuEFindInLogics;
        private System.Windows.Forms.ContextMenuStrip cmCel;
        private System.Windows.Forms.ToolStripMenuItem mnuCelUndo;
        private System.Windows.Forms.ToolStripSeparator mnuCelSep0;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCut;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuCelPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuCelDelete;
        private System.Windows.Forms.ToolStripSeparator mnuCelSep1;
        private System.Windows.Forms.ToolStripMenuItem mnuCelSelectAll;
        private System.Windows.Forms.ToolStripSeparator mnuCelSep2;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefault;
        private DefineColumn colName;
        private DefineColumn colValue;
        private DefineColumn colComment;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValueCheck;
        private System.Windows.Forms.ToolStripMenuItem mnuCelCharMap;
    }
}