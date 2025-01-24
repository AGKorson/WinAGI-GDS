namespace WinAGI.Editor {
    partial class frmReserved {
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
            reservedgrid = new WinAGIGrid();
            colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            cmCopy = new System.Windows.Forms.ToolStripMenuItem();
            cmReset = new System.Windows.Forms.ToolStripMenuItem();
            btnSave = new System.Windows.Forms.Button();
            btnReset = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)reservedgrid).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // reservedgrid
            // 
            reservedgrid.AllowUserToAddRows = false;
            reservedgrid.AllowUserToDeleteRows = false;
            reservedgrid.AllowUserToResizeColumns = false;
            reservedgrid.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            reservedgrid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            reservedgrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            reservedgrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            reservedgrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            reservedgrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colName, colValue });
            reservedgrid.ContextMenuStrip = contextMenuStrip1;
            reservedgrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            reservedgrid.Location = new System.Drawing.Point(0, 0);
            reservedgrid.MultiSelect = false;
            reservedgrid.Name = "reservedgrid";
            reservedgrid.RowHeadersVisible = false;
            reservedgrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            reservedgrid.ShowCellErrors = false;
            reservedgrid.ShowCellToolTips = false;
            reservedgrid.ShowEditingIcon = false;
            reservedgrid.ShowRowErrors = false;
            reservedgrid.Size = new System.Drawing.Size(395, 310);
            reservedgrid.TabIndex = 0;
            reservedgrid.CellFormatting += reservedgrid_CellFormatting;
            reservedgrid.CellMouseDown += reservedgrid_CellMouseDown;
            reservedgrid.CellValidated += reservedgrid_CellValidated;
            reservedgrid.CellValidating += reservedgrid_CellValidating;
            // 
            // colName
            // 
            colName.HeaderText = "Name";
            colName.Name = "colName";
            colName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colName.Width = 250;
            // 
            // colValue
            // 
            colValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            colValue.HeaderText = "Value";
            colValue.Name = "colValue";
            colValue.ReadOnly = true;
            colValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cmCopy, cmReset });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(199, 48);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // cmCopy
            // 
            cmCopy.Name = "cmCopy";
            cmCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            cmCopy.Size = new System.Drawing.Size(198, 22);
            cmCopy.Text = "Copy Name";
            cmCopy.Click += cmCopy_Click;
            // 
            // cmReset
            // 
            cmReset.Name = "cmReset";
            cmReset.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            cmReset.Size = new System.Drawing.Size(198, 22);
            cmReset.Text = "Reset to Default";
            cmReset.Click += cmReset_Click;
            // 
            // btnSave
            // 
            btnSave.Enabled = false;
            btnSave.Location = new System.Drawing.Point(12, 330);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(79, 25);
            btnSave.TabIndex = 1;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new System.Drawing.Point(158, 330);
            btnReset.Name = "btnReset";
            btnReset.Size = new System.Drawing.Size(79, 25);
            btnReset.TabIndex = 2;
            btnReset.Text = "Reset";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(304, 330);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(79, 25);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmReserved
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(395, 367);
            Controls.Add(btnCancel);
            Controls.Add(btnReset);
            Controls.Add(btnSave);
            Controls.Add(reservedgrid);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Name = "frmReserved";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Reserved Defines Editor";
            Load += frmReserved_Load;
            ((System.ComponentModel.ISupportInitialize)reservedgrid).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private WinAGIGrid reservedgrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem cmCopy;
        private System.Windows.Forms.ToolStripMenuItem cmReset;
    }
}