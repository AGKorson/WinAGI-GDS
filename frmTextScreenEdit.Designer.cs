namespace WinAGI.Editor {
    partial class frmTextScreenEdit {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTextScreenEdit));
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuMode = new System.Windows.Forms.ToolStripMenuItem();
            cmSep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuCut = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuClear = new System.Windows.Forms.ToolStripMenuItem();
            mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            cmSep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuCharMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuToggleTextMarks = new System.Windows.Forms.ToolStripMenuItem();
            mnuChangeScreenSize = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopyCommands = new System.Windows.Forms.ToolStripMenuItem();
            mnuPasteCommands = new System.Windows.Forms.ToolStripMenuItem();
            cmSep3 = new System.Windows.Forms.ToolStripSeparator();
            mnuScale = new System.Windows.Forms.ToolStripMenuItem();
            mnuZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            mnuZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            picPalette = new System.Windows.Forms.PictureBox();
            panel1 = new System.Windows.Forms.Panel();
            picCorner = new System.Windows.Forms.PictureBox();
            vsbScreen = new System.Windows.Forms.VScrollBar();
            hsbScreen = new System.Windows.Forms.HScrollBar();
            picScreen = new SelectablePictureBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            mnuResource = new System.Windows.Forms.ToolStripMenuItem();
            mnuROpen = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSave = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            mnuRRemove = new System.Windows.Forms.ToolStripMenuItem();
            mnuRRenumber = new System.Windows.Forms.ToolStripMenuItem();
            mnuRIDDesc = new System.Windows.Forms.ToolStripMenuItem();
            mnuRCompileLogic = new System.Windows.Forms.ToolStripMenuItem();
            mnuRSavePic = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportGif = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            tmrCursor = new System.Windows.Forms.Timer(components);
            tmrSelect = new System.Windows.Forms.Timer(components);
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnMode = new System.Windows.Forms.ToolStripButton();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            btnCut = new System.Windows.Forms.ToolStripButton();
            btnCopy = new System.Windows.Forms.ToolStripButton();
            btnPaste = new System.Windows.Forms.ToolStripButton();
            btnDelete = new System.Windows.Forms.ToolStripButton();
            btnClear = new System.Windows.Forms.ToolStripButton();
            tsSep2 = new System.Windows.Forms.ToolStripSeparator();
            btnCharMap = new System.Windows.Forms.ToolStripButton();
            btnCopyCommand = new System.Windows.Forms.ToolStripButton();
            tsSep3 = new System.Windows.Forms.ToolStripSeparator();
            btnZoomIn = new System.Windows.Forms.ToolStripButton();
            btnZoomOut = new System.Windows.Forms.ToolStripButton();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPalette).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCorner).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picScreen).BeginInit();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuMode, cmSep1, mnuCut, mnuCopy, mnuPaste, mnuDelete, mnuClear, mnuSelectAll, cmSep2, mnuCharMap, mnuToggleTextMarks, mnuChangeScreenSize, mnuCopyCommands, mnuPasteCommands, cmSep3, mnuScale });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(280, 308);
            contextMenuStrip1.Closed += contextMenuStrip1_Closed;
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // mnuMode
            // 
            mnuMode.Name = "mnuMode";
            mnuMode.ShortcutKeyDisplayString = "Ins";
            mnuMode.Size = new System.Drawing.Size(279, 22);
            mnuMode.Text = "Mode";
            mnuMode.Click += mnuMode_Click;
            // 
            // cmSep1
            // 
            cmSep1.Name = "cmSep1";
            cmSep1.Size = new System.Drawing.Size(276, 6);
            // 
            // mnuCut
            // 
            mnuCut.Name = "mnuCut";
            mnuCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuCut.Size = new System.Drawing.Size(279, 22);
            mnuCut.Text = "Cut";
            mnuCut.Click += mnuCut_Click;
            // 
            // mnuCopy
            // 
            mnuCopy.Name = "mnuCopy";
            mnuCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuCopy.Size = new System.Drawing.Size(279, 22);
            mnuCopy.Text = "Copy";
            mnuCopy.Click += mnuCopy_Click;
            // 
            // mnuPaste
            // 
            mnuPaste.Name = "mnuPaste";
            mnuPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuPaste.Size = new System.Drawing.Size(279, 22);
            mnuPaste.Text = "Paste";
            mnuPaste.Click += mnuPaste_Click;
            // 
            // mnuDelete
            // 
            mnuDelete.Name = "mnuDelete";
            mnuDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuDelete.Size = new System.Drawing.Size(279, 22);
            mnuDelete.Text = "Delete";
            mnuDelete.Click += mnuDelete_Click;
            // 
            // mnuClear
            // 
            mnuClear.Name = "mnuClear";
            mnuClear.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete;
            mnuClear.Size = new System.Drawing.Size(279, 22);
            mnuClear.Text = "Clear";
            mnuClear.Click += mnuClear_Click;
            // 
            // mnuSelectAll
            // 
            mnuSelectAll.Name = "mnuSelectAll";
            mnuSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuSelectAll.Size = new System.Drawing.Size(279, 22);
            mnuSelectAll.Text = "Select All";
            mnuSelectAll.Click += mnuSelectAll_Click;
            // 
            // cmSep2
            // 
            cmSep2.Name = "cmSep2";
            cmSep2.Size = new System.Drawing.Size(276, 6);
            // 
            // mnuCharMap
            // 
            mnuCharMap.Name = "mnuCharMap";
            mnuCharMap.ShortcutKeyDisplayString = "Shift+Ins";
            mnuCharMap.ShortcutKeys = System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Insert;
            mnuCharMap.Size = new System.Drawing.Size(279, 22);
            mnuCharMap.Text = "Character Map";
            mnuCharMap.Click += mnuCharMap_Click;
            // 
            // mnuToggleTextMarks
            // 
            mnuToggleTextMarks.Name = "mnuToggleTextMarks";
            mnuToggleTextMarks.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.T;
            mnuToggleTextMarks.Size = new System.Drawing.Size(279, 22);
            mnuToggleTextMarks.Text = "Toggle Text Marks";
            mnuToggleTextMarks.Click += mnuToggleTextMarks_Click;
            // 
            // mnuChangeScreenSize
            // 
            mnuChangeScreenSize.Name = "mnuChangeScreenSize";
            mnuChangeScreenSize.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.W;
            mnuChangeScreenSize.Size = new System.Drawing.Size(279, 22);
            mnuChangeScreenSize.Text = "Change Screen Size";
            mnuChangeScreenSize.Click += mnuChangeScreenSize_Click;
            // 
            // mnuCopyCommands
            // 
            mnuCopyCommands.Name = "mnuCopyCommands";
            mnuCopyCommands.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.C;
            mnuCopyCommands.Size = new System.Drawing.Size(279, 22);
            mnuCopyCommands.Text = "Copy As AGI Commands";
            mnuCopyCommands.Click += mnuCopyCommands_Click;
            // 
            // mnuPasteCommands
            // 
            mnuPasteCommands.Name = "mnuPasteCommands";
            mnuPasteCommands.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.V;
            mnuPasteCommands.Size = new System.Drawing.Size(279, 22);
            mnuPasteCommands.Text = "Paste As AGI Commands";
            mnuPasteCommands.Click += mnuPasteCommands_Click;
            // 
            // cmSep3
            // 
            cmSep3.Name = "cmSep3";
            cmSep3.Size = new System.Drawing.Size(276, 6);
            // 
            // mnuScale
            // 
            mnuScale.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuZoomIn, mnuZoomOut });
            mnuScale.Name = "mnuScale";
            mnuScale.Size = new System.Drawing.Size(279, 22);
            mnuScale.Text = "Scale";
            // 
            // mnuZoomIn
            // 
            mnuZoomIn.Name = "mnuZoomIn";
            mnuZoomIn.Size = new System.Drawing.Size(129, 22);
            mnuZoomIn.Text = "Zoom In";
            mnuZoomIn.Click += mnuZoomIn_Click;
            // 
            // mnuZoomOut
            // 
            mnuZoomOut.Name = "mnuZoomOut";
            mnuZoomOut.Size = new System.Drawing.Size(129, 22);
            mnuZoomOut.Text = "Zoom Out";
            mnuZoomOut.Click += mnuZoomOut_Click;
            // 
            // picPalette
            // 
            picPalette.Dock = System.Windows.Forms.DockStyle.Bottom;
            picPalette.Location = new System.Drawing.Point(0, 351);
            picPalette.Name = "picPalette";
            picPalette.Size = new System.Drawing.Size(800, 32);
            picPalette.TabIndex = 13;
            picPalette.TabStop = false;
            picPalette.Paint += picPalette_Paint;
            picPalette.MouseDown += picPalette_MouseDown;
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel1.Controls.Add(picCorner);
            panel1.Controls.Add(vsbScreen);
            panel1.Controls.Add(hsbScreen);
            panel1.Controls.Add(picScreen);
            panel1.Location = new System.Drawing.Point(0, 28);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(800, 323);
            panel1.TabIndex = 22;
            panel1.MouseClick += panel1_MouseClick;
            panel1.Resize += panel1_Resize;
            // 
            // picCorner
            // 
            picCorner.Location = new System.Drawing.Point(775, 292);
            picCorner.Name = "picCorner";
            picCorner.Size = new System.Drawing.Size(16, 16);
            picCorner.TabIndex = 22;
            picCorner.TabStop = false;
            picCorner.Visible = false;
            // 
            // vsbScreen
            // 
            vsbScreen.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            vsbScreen.Location = new System.Drawing.Point(775, 0);
            vsbScreen.Minimum = -5;
            vsbScreen.Name = "vsbScreen";
            vsbScreen.Size = new System.Drawing.Size(16, 164);
            vsbScreen.TabIndex = 25;
            vsbScreen.Visible = false;
            vsbScreen.Scroll += vsbScreen_Scroll;
            vsbScreen.Enter += vsbScreen_Enter;
            // 
            // hsbScreen
            // 
            hsbScreen.Location = new System.Drawing.Point(0, 292);
            hsbScreen.Minimum = -5;
            hsbScreen.Name = "hsbScreen";
            hsbScreen.Size = new System.Drawing.Size(308, 16);
            hsbScreen.TabIndex = 24;
            hsbScreen.Visible = false;
            hsbScreen.Scroll += hsbScreen_Scroll;
            hsbScreen.Enter += hsbScreen_Enter;
            // 
            // picScreen
            // 
            picScreen.BackColor = System.Drawing.Color.Black;
            picScreen.ContextMenuStrip = contextMenuStrip1;
            picScreen.Location = new System.Drawing.Point(0, 0);
            picScreen.Name = "picScreen";
            picScreen.ShowFocusRectangle = false;
            picScreen.Size = new System.Drawing.Size(341, 161);
            picScreen.TabIndex = 23;
            picScreen.Paint += picScreen_Paint;
            picScreen.MouseDown += picScreen_MouseDown;
            picScreen.MouseLeave += picScreen_MouseLeave;
            picScreen.MouseMove += picScreen_MouseMove;
            picScreen.MouseUp += picScreen_MouseUp;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuResource, mnuEdit });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 26;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // mnuResource
            // 
            mnuResource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuROpen, mnuRSave, mnuRSaveAs, mnuRRemove, mnuRRenumber, mnuRIDDesc, mnuRCompileLogic, mnuRSavePic, mnuExportGif });
            mnuResource.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            mnuResource.MergeIndex = 1;
            mnuResource.Name = "mnuResource";
            mnuResource.Size = new System.Drawing.Size(67, 20);
            mnuResource.Text = "Resource";
            // 
            // mnuROpen
            // 
            mnuROpen.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuROpen.MergeIndex = 4;
            mnuROpen.Name = "mnuROpen";
            mnuROpen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.O;
            mnuROpen.Size = new System.Drawing.Size(241, 22);
            mnuROpen.Text = "Open Text Layout";
            mnuROpen.Click += mnuROpen_Click;
            // 
            // mnuRSave
            // 
            mnuRSave.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSave.MergeIndex = 5;
            mnuRSave.Name = "mnuRSave";
            mnuRSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuRSave.Size = new System.Drawing.Size(241, 22);
            mnuRSave.Text = "Save Text Layout";
            mnuRSave.Click += mnuRSave_Click;
            // 
            // mnuRSaveAs
            // 
            mnuRSaveAs.MergeAction = System.Windows.Forms.MergeAction.Replace;
            mnuRSaveAs.MergeIndex = 6;
            mnuRSaveAs.Name = "mnuRSaveAs";
            mnuRSaveAs.Size = new System.Drawing.Size(241, 22);
            mnuRSaveAs.Text = "Save Text Layout As ...";
            mnuRSaveAs.Click += mnuRSaveAs_Click;
            // 
            // mnuRRemove
            // 
            mnuRRemove.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRemove.MergeIndex = 8;
            mnuRRemove.Name = "mnuRRemove";
            mnuRRemove.Size = new System.Drawing.Size(241, 22);
            mnuRRemove.Text = "remove";
            // 
            // mnuRRenumber
            // 
            mnuRRenumber.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRRenumber.MergeIndex = 8;
            mnuRRenumber.Name = "mnuRRenumber";
            mnuRRenumber.Size = new System.Drawing.Size(241, 22);
            mnuRRenumber.Text = "renumber";
            // 
            // mnuRIDDesc
            // 
            mnuRIDDesc.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRIDDesc.MergeIndex = 8;
            mnuRIDDesc.Name = "mnuRIDDesc";
            mnuRIDDesc.Size = new System.Drawing.Size(241, 22);
            mnuRIDDesc.Text = "iddesc";
            // 
            // mnuRCompileLogic
            // 
            mnuRCompileLogic.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRCompileLogic.MergeIndex = 9;
            mnuRCompileLogic.Name = "mnuRCompileLogic";
            mnuRCompileLogic.Size = new System.Drawing.Size(241, 22);
            mnuRCompileLogic.Text = "compile";
            // 
            // mnuRSavePic
            // 
            mnuRSavePic.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuRSavePic.MergeIndex = 9;
            mnuRSavePic.Name = "mnuRSavePic";
            mnuRSavePic.Size = new System.Drawing.Size(241, 22);
            mnuRSavePic.Text = "savepic";
            // 
            // mnuExportGif
            // 
            mnuExportGif.MergeAction = System.Windows.Forms.MergeAction.Remove;
            mnuExportGif.MergeIndex = 9;
            mnuExportGif.Name = "mnuExportGif";
            mnuExportGif.Size = new System.Drawing.Size(241, 22);
            mnuExportGif.Text = "exportgif";
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
            // tmrCursor
            // 
            tmrCursor.Interval = 120;
            tmrCursor.Tick += tmrCursor_Tick;
            // 
            // tmrSelect
            // 
            tmrSelect.Tick += tmrSelect_Tick;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnMode, tsSep1, btnCut, btnCopy, btnPaste, btnDelete, btnClear, tsSep2, btnCharMap, btnCopyCommand, tsSep3, btnZoomIn, btnZoomOut });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(800, 31);
            toolStrip1.Stretch = true;
            toolStrip1.TabIndex = 23;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnMode
            // 
            btnMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnMode.Image = EditorResources.ET_INS;
            btnMode.Name = "btnMode";
            btnMode.Size = new System.Drawing.Size(28, 28);
            btnMode.Text = "Mode";
            btnMode.Click += mnuMode_Click;
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new System.Drawing.Size(6, 31);
            // 
            // btnCut
            // 
            btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCut.Image = (System.Drawing.Image)resources.GetObject("btnCut.Image");
            btnCut.Name = "btnCut";
            btnCut.Size = new System.Drawing.Size(28, 28);
            btnCut.Text = "Cut";
            btnCut.Click += mnuCut_Click;
            // 
            // btnCopy
            // 
            btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCopy.Image = (System.Drawing.Image)resources.GetObject("btnCopy.Image");
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new System.Drawing.Size(28, 28);
            btnCopy.Text = "Copy";
            btnCopy.Click += mnuCopy_Click;
            // 
            // btnPaste
            // 
            btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnPaste.Image = (System.Drawing.Image)resources.GetObject("btnPaste.Image");
            btnPaste.Name = "btnPaste";
            btnPaste.Size = new System.Drawing.Size(28, 28);
            btnPaste.Text = "Paste";
            btnPaste.Click += mnuPaste_Click;
            // 
            // btnDelete
            // 
            btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnDelete.Image = (System.Drawing.Image)resources.GetObject("btnDelete.Image");
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(28, 28);
            btnDelete.Text = "Delete";
            btnDelete.Click += mnuDelete_Click;
            // 
            // btnClear
            // 
            btnClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnClear.Image = (System.Drawing.Image)resources.GetObject("btnClear.Image");
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(28, 28);
            btnClear.Text = "Clear";
            btnClear.Click += mnuClear_Click;
            // 
            // tsSep2
            // 
            tsSep2.Name = "tsSep2";
            tsSep2.Size = new System.Drawing.Size(6, 31);
            // 
            // btnCharMap
            // 
            btnCharMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCharMap.Image = (System.Drawing.Image)resources.GetObject("btnCharMap.Image");
            btnCharMap.Name = "btnCharMap";
            btnCharMap.Size = new System.Drawing.Size(28, 28);
            btnCharMap.Text = "Character Map";
            btnCharMap.Click += mnuCharMap_Click;
            // 
            // btnCopyCommand
            // 
            btnCopyCommand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCopyCommand.Image = (System.Drawing.Image)resources.GetObject("btnCopyCommand.Image");
            btnCopyCommand.Name = "btnCopyCommand";
            btnCopyCommand.Size = new System.Drawing.Size(28, 28);
            btnCopyCommand.Text = "Copy Command";
            btnCopyCommand.Click += mnuCopyCommands_Click;
            // 
            // tsSep3
            // 
            tsSep3.Name = "tsSep3";
            tsSep3.Size = new System.Drawing.Size(6, 31);
            // 
            // btnZoomIn
            // 
            btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnZoomIn.Image = (System.Drawing.Image)resources.GetObject("btnZoomIn.Image");
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new System.Drawing.Size(28, 28);
            btnZoomIn.Text = "Zoom In";
            btnZoomIn.Click += mnuZoomIn_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnZoomOut.Image = (System.Drawing.Image)resources.GetObject("btnZoomOut.Image");
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new System.Drawing.Size(28, 28);
            btnZoomOut.Text = "Zoom Out";
            btnZoomOut.Click += mnuZoomOut_Click;
            // 
            // frmTextScreenEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 383);
            Controls.Add(toolStrip1);
            Controls.Add(panel1);
            Controls.Add(picPalette);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "frmTextScreenEdit";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            Text = "frmTextScreenEdit";
            FormClosing += frmTextScreenEdit_FormClosing;
            FormClosed += frmTextScreenEdit_FormClosed;
            Resize += frmTextScreenEdit_Resize;
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPalette).EndInit();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCorner).EndInit();
            ((System.ComponentModel.ISupportInitialize)picScreen).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox picPalette;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.VScrollBar vsbScreen;
        private System.Windows.Forms.HScrollBar hsbScreen;
        private SelectablePictureBox picScreen;
        private System.Windows.Forms.PictureBox picCorner;
        private System.Windows.Forms.Timer tmrCursor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuMode;
        private System.Windows.Forms.ToolStripSeparator cmSep1;
        private System.Windows.Forms.ToolStripMenuItem mnuCut;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuClear;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectAll;
        private System.Windows.Forms.ToolStripSeparator cmSep2;
        private System.Windows.Forms.ToolStripMenuItem mnuCharMap;
        private System.Windows.Forms.ToolStripMenuItem mnuToggleTextMarks;
        private System.Windows.Forms.ToolStripMenuItem mnuChangeScreenSize;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyCommands;
        private System.Windows.Forms.ToolStripMenuItem mnuPasteCommands;
        private System.Windows.Forms.ToolStripSeparator cmSep3;
        private System.Windows.Forms.ToolStripMenuItem mnuScale;
        private System.Windows.Forms.ToolStripMenuItem mnuZoomIn;
        private System.Windows.Forms.ToolStripMenuItem mnuZoomOut;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnMode;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton btnCut;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnPaste;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripButton btnClear;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripButton btnCharMap;
        private System.Windows.Forms.ToolStripButton btnCopyCommand;
        private System.Windows.Forms.ToolStripSeparator tsSep3;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuResource;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuROpen;
        private System.Windows.Forms.ToolStripMenuItem mnuRSave;
        private System.Windows.Forms.ToolStripMenuItem mnuRSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuRRemove;
        private System.Windows.Forms.ToolStripMenuItem mnuRRenumber;
        private System.Windows.Forms.ToolStripMenuItem mnuRIDDesc;
        private System.Windows.Forms.ToolStripMenuItem mnuRCompileLogic;
        private System.Windows.Forms.ToolStripMenuItem mnuRSavePic;
        private System.Windows.Forms.ToolStripMenuItem mnuExportGif;
        private System.Windows.Forms.Timer tmrSelect;
    }
}