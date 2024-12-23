
namespace WinAGI.Editor {
    partial class frmSnippets {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSnippets));
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lblArgTips = new System.Windows.Forms.Label();
            lstSnippets = new System.Windows.Forms.ListBox();
            txtSnipName = new System.Windows.Forms.TextBox();
            txtArgTips = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            rtfSnipValue = new WinAGIFCTB();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuEUndo = new System.Windows.Forms.ToolStripMenuItem();
            mnuERedo = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep0 = new System.Windows.Forms.ToolStripSeparator();
            mnuECut = new System.Windows.Forms.ToolStripMenuItem();
            mnuEDelete = new System.Windows.Forms.ToolStripMenuItem();
            mnuECopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuEPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuESelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuEBlockCmt = new System.Windows.Forms.ToolStripMenuItem();
            mnuEUnblockCmt = new System.Windows.Forms.ToolStripMenuItem();
            mnuESep3 = new System.Windows.Forms.ToolStripSeparator();
            mnuECharMap = new System.Windows.Forms.ToolStripMenuItem();
            btnAdd = new System.Windows.Forms.Button();
            btnEdit = new System.Windows.Forms.Button();
            btnDelete = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)rtfSnipValue).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(52, 15);
            label1.TabIndex = 0;
            label1.Text = "Snippets";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(171, 10);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(85, 15);
            label2.TabIndex = 2;
            label2.Text = "Snippet Name:";
            // 
            // lblArgTips
            // 
            lblArgTips.AutoSize = true;
            lblArgTips.Location = new System.Drawing.Point(393, 10);
            lblArgTips.Name = "lblArgTips";
            lblArgTips.Size = new System.Drawing.Size(89, 15);
            lblArgTips.TabIndex = 4;
            lblArgTips.Text = "Argument Tips:";
            lblArgTips.Visible = false;
            // 
            // lstSnippets
            // 
            lstSnippets.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lstSnippets.FormattingEnabled = true;
            lstSnippets.ItemHeight = 15;
            lstSnippets.Location = new System.Drawing.Point(14, 28);
            lstSnippets.Name = "lstSnippets";
            lstSnippets.Size = new System.Drawing.Size(147, 289);
            lstSnippets.Sorted = true;
            lstSnippets.TabIndex = 1;
            lstSnippets.SelectedValueChanged += lstSnippets_SelectedValueChanged;
            lstSnippets.DoubleClick += lstSnippets_DoubleClick;
            // 
            // txtSnipName
            // 
            txtSnipName.BackColor = System.Drawing.Color.White;
            txtSnipName.ReadOnly = true;
            txtSnipName.Location = new System.Drawing.Point(171, 30);
            txtSnipName.Name = "txtSnipName";
            txtSnipName.Size = new System.Drawing.Size(202, 23);
            txtSnipName.TabIndex = 3;
            txtSnipName.DoubleClick += txtSnipName_DoubleClick;
            txtSnipName.KeyPress += txtSnipName_KeyPress;
            // 
            // txtArgTips
            // 
            txtArgTips.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtArgTips.BackColor = System.Drawing.SystemColors.Info;
            txtArgTips.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtArgTips.Location = new System.Drawing.Point(393, 30);
            txtArgTips.Name = "txtArgTips";
            txtArgTips.Size = new System.Drawing.Size(260, 23);
            txtArgTips.TabIndex = 5;
            txtArgTips.Visible = false;
            txtArgTips.DoubleClick += txtArgTips_DoubleClick;
            txtArgTips.KeyPress += txtArgTips_KeyPress;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(171, 56);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(74, 15);
            label4.TabIndex = 6;
            label4.Text = "Snippet Text:";
            // 
            // rtfSnipValue
            // 
            rtfSnipValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rtfSnipValue.AutoCompleteBracketsList = new char[]
    {
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
    };
            rtfSnipValue.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
            rtfSnipValue.AutoScrollMinSize = new System.Drawing.Size(90, 14);
            rtfSnipValue.BackBrush = null;
            rtfSnipValue.CharHeight = 14;
            rtfSnipValue.CharWidth = 8;
            rtfSnipValue.ContextMenuStrip = contextMenuStrip1;
            rtfSnipValue.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            rtfSnipValue.Font = new System.Drawing.Font("Courier New", 9.75F);
            rtfSnipValue.Hotkeys = resources.GetString("rtfSnipValue.Hotkeys");
            rtfSnipValue.IsReplaceMode = false;
            rtfSnipValue.Location = new System.Drawing.Point(171, 74);
            rtfSnipValue.Name = "rtfSnipValue";
            rtfSnipValue.NoMouse = false;
            rtfSnipValue.Paddings = new System.Windows.Forms.Padding(0);
            rtfSnipValue.ReadOnly = true;
            rtfSnipValue.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            rtfSnipValue.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("rtfSnipValue.ServiceColors");
            rtfSnipValue.ShowCaretWhenInactive = true;
            rtfSnipValue.ShowLineNumbers = false;
            rtfSnipValue.Size = new System.Drawing.Size(482, 243);
            rtfSnipValue.TabIndex = 7;
            rtfSnipValue.Text = "winagifctb1";
            rtfSnipValue.Zoom = 100;
            rtfSnipValue.TextChanged += rtfSnipValue_TextChanged;
            rtfSnipValue.DoubleClick += rtfSnipValue_DoubleClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuEUndo, mnuERedo, mnuESep0, mnuECut, mnuEDelete, mnuECopy, mnuEPaste, mnuESelectAll, mnuESep2, mnuEBlockCmt, mnuEUnblockCmt, mnuESep3, mnuECharMap });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(225, 242);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // mnuEUndo
            // 
            mnuEUndo.Name = "mnuEUndo";
            mnuEUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            mnuEUndo.Size = new System.Drawing.Size(224, 22);
            mnuEUndo.Text = "Undo";
            mnuEUndo.Click += mnuEUndo_Click;
            // 
            // mnuERedo
            // 
            mnuERedo.Name = "mnuERedo";
            mnuERedo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y;
            mnuERedo.Size = new System.Drawing.Size(224, 22);
            mnuERedo.Text = "Redo";
            mnuERedo.Click += mnuERedo_Click;
            // 
            // mnuESep0
            // 
            mnuESep0.Name = "mnuESep0";
            mnuESep0.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuECut
            // 
            mnuECut.Name = "mnuECut";
            mnuECut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            mnuECut.Size = new System.Drawing.Size(224, 22);
            mnuECut.Text = "Cut";
            mnuECut.Click += mnuECut_Click;
            // 
            // mnuEDelete
            // 
            mnuEDelete.Name = "mnuEDelete";
            mnuEDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            mnuEDelete.Size = new System.Drawing.Size(224, 22);
            mnuEDelete.Text = "Delete";
            mnuEDelete.Click += mnuEDelete_Click;
            // 
            // mnuECopy
            // 
            mnuECopy.Name = "mnuECopy";
            mnuECopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuECopy.Size = new System.Drawing.Size(224, 22);
            mnuECopy.Text = "Copy";
            mnuECopy.Click += mnuECopy_Click;
            // 
            // mnuEPaste
            // 
            mnuEPaste.Name = "mnuEPaste";
            mnuEPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            mnuEPaste.Size = new System.Drawing.Size(224, 22);
            mnuEPaste.Text = "Paste";
            mnuEPaste.Click += mnuEPaste_Click;
            // 
            // mnuESelectAll
            // 
            mnuESelectAll.Name = "mnuESelectAll";
            mnuESelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            mnuESelectAll.Size = new System.Drawing.Size(224, 22);
            mnuESelectAll.Text = "Select All";
            mnuESelectAll.Click += mnuESelectAll_Click;
            // 
            // mnuESep2
            // 
            mnuESep2.Name = "mnuESep2";
            mnuESep2.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuEBlockCmt
            // 
            mnuEBlockCmt.Name = "mnuEBlockCmt";
            mnuEBlockCmt.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.B;
            mnuEBlockCmt.Size = new System.Drawing.Size(224, 22);
            mnuEBlockCmt.Text = "Block Comment";
            mnuEBlockCmt.Click += mnuEBlockCmt_Click;
            // 
            // mnuEUnblockCmt
            // 
            mnuEUnblockCmt.Name = "mnuEUnblockCmt";
            mnuEUnblockCmt.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U;
            mnuEUnblockCmt.Size = new System.Drawing.Size(224, 22);
            mnuEUnblockCmt.Text = "Unblock Comment";
            mnuEUnblockCmt.Click += mnuEUnblockCmt_Click;
            // 
            // mnuESep3
            // 
            mnuESep3.Name = "mnuESep3";
            mnuESep3.Size = new System.Drawing.Size(221, 6);
            // 
            // mnuECharMap
            // 
            mnuECharMap.Name = "mnuECharMap";
            mnuECharMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert;
            mnuECharMap.Size = new System.Drawing.Size(224, 22);
            mnuECharMap.Text = "Character Map...";
            mnuECharMap.Click += mnuECharMap_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnAdd.Location = new System.Drawing.Point(12, 332);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(147, 28);
            btnAdd.TabIndex = 8;
            btnAdd.Text = "Add New Snippet";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnEdit
            // 
            btnEdit.Location = new System.Drawing.Point(171, 332);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(100, 28);
            btnEdit.TabIndex = 9;
            btnEdit.Text = "Edit Snippet";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Visible = false;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnDelete.Location = new System.Drawing.Point(277, 332);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(100, 28);
            btnDelete.TabIndex = 11;
            btnDelete.Text = "Delete Snippet";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Visible = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnClose.Location = new System.Drawing.Point(553, 332);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(100, 28);
            btnClose.TabIndex = 12;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnSave.Location = new System.Drawing.Point(171, 332);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(100, 28);
            btnSave.TabIndex = 10;
            btnSave.Text = "Save Snippet";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Visible = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnCancel.Location = new System.Drawing.Point(277, 332);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(100, 28);
            btnCancel.TabIndex = 13;
            btnCancel.Text = "Cancel Edit";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Visible = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmSnippets
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(664, 368);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnClose);
            Controls.Add(btnDelete);
            Controls.Add(btnEdit);
            Controls.Add(btnAdd);
            Controls.Add(rtfSnipValue);
            Controls.Add(label4);
            Controls.Add(txtArgTips);
            Controls.Add(txtSnipName);
            Controls.Add(lstSnippets);
            Controls.Add(lblArgTips);
            Controls.Add(label2);
            Controls.Add(label1);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(1200, 407);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(680, 250);
            Name = "frmSnippets";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Snippet Manager";
            FormClosing += frmSnippets_FormClosing;
            ((System.ComponentModel.ISupportInitialize)rtfSnipValue).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblArgTips;
        private System.Windows.Forms.ListBox lstSnippets;
        private System.Windows.Forms.TextBox txtSnipName;
        private System.Windows.Forms.TextBox txtArgTips;
        private System.Windows.Forms.Label label4;
        private WinAGIFCTB rtfSnipValue;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuEUndo;
        private System.Windows.Forms.ToolStripMenuItem mnuERedo;
        private System.Windows.Forms.ToolStripSeparator mnuESep0;
        private System.Windows.Forms.ToolStripMenuItem mnuECut;
        private System.Windows.Forms.ToolStripMenuItem mnuEDelete;
        private System.Windows.Forms.ToolStripMenuItem mnuECopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuESelectAll;
        private System.Windows.Forms.ToolStripSeparator mnuESep2;
        private System.Windows.Forms.ToolStripMenuItem mnuEBlockCmt;
        private System.Windows.Forms.ToolStripMenuItem mnuEUnblockCmt;
        private System.Windows.Forms.ToolStripSeparator mnuESep3;
        private System.Windows.Forms.ToolStripMenuItem mnuECharMap;
        private System.Windows.Forms.Button btnCancel;
    }
}