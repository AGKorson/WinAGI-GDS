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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            fgReserved = new WinAGIGrid();
            colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            btnSave = new System.Windows.Forms.Button();
            btnReset = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)fgReserved).BeginInit();
            SuspendLayout();
            // 
            // fgReserved
            // 
            fgReserved.AllowUserToAddRows = false;
            fgReserved.AllowUserToDeleteRows = false;
            fgReserved.AllowUserToResizeColumns = false;
            fgReserved.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            fgReserved.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            fgReserved.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            fgReserved.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            fgReserved.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            fgReserved.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colName, colValue });
            fgReserved.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            fgReserved.Location = new System.Drawing.Point(0, 0);
            fgReserved.MultiSelect = false;
            fgReserved.Name = "fgReserved";
            fgReserved.RowHeadersVisible = false;
            fgReserved.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            fgReserved.ShowCellErrors = false;
            fgReserved.ShowCellToolTips = false;
            fgReserved.ShowEditingIcon = false;
            fgReserved.ShowRowErrors = false;
            fgReserved.Size = new System.Drawing.Size(395, 310);
            fgReserved.TabIndex = 0;
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
            // btnSave
            // 
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
            Controls.Add(fgReserved);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Name = "frmReserved";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Reserved Defines Editor";
            Load += frmReserved_Load;
            ((System.ComponentModel.ISupportInitialize)fgReserved).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private WinAGIGrid fgReserved;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnCancel;
    }
}