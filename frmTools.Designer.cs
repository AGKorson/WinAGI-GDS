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
            fgTools = new System.Windows.Forms.DataGridView();
            colNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colCaption = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colTarget = new System.Windows.Forms.DataGridViewTextBoxColumn();
            btnBrowse = new System.Windows.Forms.Button();
            dlgTool = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)fgTools).BeginInit();
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
            // fgTools
            // 
            fgTools.AllowUserToAddRows = false;
            fgTools.AllowUserToDeleteRows = false;
            fgTools.AllowUserToResizeRows = false;
            fgTools.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            fgTools.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            fgTools.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            fgTools.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colNum, colCaption, colTarget });
            fgTools.Dock = System.Windows.Forms.DockStyle.Top;
            fgTools.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            fgTools.Location = new System.Drawing.Point(0, 0);
            fgTools.Margin = new System.Windows.Forms.Padding(2);
            fgTools.MultiSelect = false;
            fgTools.Name = "fgTools";
            fgTools.RowHeadersVisible = false;
            fgTools.RowHeadersWidth = 82;
            fgTools.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            fgTools.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            fgTools.ShowCellErrors = false;
            fgTools.ShowEditingIcon = false;
            fgTools.ShowRowErrors = false;
            fgTools.Size = new System.Drawing.Size(607, 229);
            fgTools.StandardTab = true;
            fgTools.TabIndex = 12;
            fgTools.CellBeginEdit += fgTools_CellBeginEdit;
            fgTools.CellClick += fgTools_CellClick;
            fgTools.CellDoubleClick += fgTools_CellDoubleClick;
            fgTools.CellEndEdit += fgTools_CellEndEdit;
            fgTools.KeyDown += fgTools_KeyDown;
            fgTools.MouseDown += fgTools_MouseDown;
            fgTools.MouseMove += fgTools_MouseMove;
            fgTools.MouseUp += fgTools_MouseUp;
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
            dlgTool.Title = "Select Command";
            // 
            // frmTools
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(607, 262);
            Controls.Add(btnBrowse);
            Controls.Add(fgTools);
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
            ((System.ComponentModel.ISupportInitialize)fgTools).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView fgTools;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.OpenFileDialog dlgTool;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCaption;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTarget;
    }
}