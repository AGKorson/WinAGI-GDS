namespace WinAGI.Editor {
    partial class frmTools {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTools));
            btnCancel = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            toolgrid = new System.Windows.Forms.DataGridView();
            colNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colCaption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colTarget = new System.Windows.Forms.DataGridViewTextBoxColumn();
            btnBrowse = new System.Windows.Forms.Button();
            toolOpen = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)toolgrid).BeginInit();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(196, 234);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(331, 234);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 11;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // toolgrid
            // 
            toolgrid.AllowDrop = true;
            toolgrid.AllowUserToAddRows = false;
            toolgrid.AllowUserToDeleteRows = false;
            toolgrid.AllowUserToResizeRows = false;
            toolgrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            toolgrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            toolgrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            toolgrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colNum, colCaption, colTarget });
            toolgrid.Dock = System.Windows.Forms.DockStyle.Top;
            toolgrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            toolgrid.Location = new System.Drawing.Point(0, 0);
            toolgrid.Margin = new System.Windows.Forms.Padding(2);
            toolgrid.MultiSelect = false;
            toolgrid.Name = "toolgrid";
            toolgrid.RowHeadersVisible = false;
            toolgrid.RowHeadersWidth = 82;
            toolgrid.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            toolgrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            toolgrid.ShowCellErrors = false;
            toolgrid.ShowCellToolTips = false;
            toolgrid.ShowEditingIcon = false;
            toolgrid.ShowRowErrors = false;
            toolgrid.Size = new System.Drawing.Size(607, 229);
            toolgrid.StandardTab = true;
            toolgrid.TabIndex = 12;
            toolgrid.CellBeginEdit += toolgrid_CellBeginEdit;
            toolgrid.CellClick += toolgrid_CellClick;
            toolgrid.CellDoubleClick += toolgrid_CellDoubleClick;
            toolgrid.CellEndEdit += toolgrid_CellEndEdit;
            toolgrid.CellFormatting += toolgrid_CellFormatting;
            toolgrid.CellMouseEnter += toolgrid_CellMouseEnter;
            toolgrid.CellMouseLeave += toolgrid_CellMouseLeave;
            toolgrid.RowPrePaint += toolgrid_RowPrePaint;
            toolgrid.DragDrop += toolgrid_DragDrop;
            toolgrid.DragEnter += toolgrid_DragEnter;
            toolgrid.DragOver += toolgrid_DragOver;
            toolgrid.KeyDown += toolgrid_KeyDown;
            toolgrid.MouseDown += toolgrid_MouseDown;
            // 
            // colNum
            // 
            colNum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            colNum.FillWeight = 15F;
            colNum.HeaderText = "#";
            colNum.MinimumWidth = 10;
            colNum.Name = "colNum";
            colNum.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            colNum.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colNum.Width = 30;
            // 
            // colCaption
            // 
            colCaption.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            colCaption.FillWeight = 30F;
            colCaption.HeaderText = "Caption";
            colCaption.MinimumWidth = 10;
            colCaption.Name = "colCaption";
            colCaption.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colTarget
            // 
            colTarget.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            colTarget.FillWeight = 50F;
            colTarget.HeaderText = "Target";
            colTarget.MinimumWidth = 10;
            colTarget.Name = "colTarget";
            colTarget.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new System.Drawing.Point(432, 234);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(32, 22);
            btnBrowse.TabIndex = 13;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Visible = false;
            btnBrowse.MouseDown += btnBrowse_MouseDown;
            // 
            // dlgTool
            // 
            toolOpen.Title = "Select Command";
            // 
            // frmTools
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(607, 262);
            Controls.Add(btnBrowse);
            Controls.Add(toolgrid);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmTools";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Custom Tools Editor";
            Load += frmTools_Load;
            HelpRequested += frmTools_HelpRequested;
            ((System.ComponentModel.ISupportInitialize)toolgrid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView toolgrid;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.OpenFileDialog toolOpen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCaption;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTarget;
    }
}