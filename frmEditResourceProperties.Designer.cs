namespace WinAGI.Editor {
    partial class frmEditResourceProperties {
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
            btnCancel = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            lblDescription = new System.Windows.Forms.Label();
            lblID = new System.Windows.Forms.Label();
            chkUpdate = new System.Windows.Forms.CheckBox();
            txtDescription = new System.Windows.Forms.TextBox();
            txtID = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(209, 182);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 25);
            btnCancel.TabIndex = 20;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Enabled = false;
            btnOK.Location = new System.Drawing.Point(12, 182);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 25);
            btnOK.TabIndex = 19;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new System.Drawing.Point(12, 46);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(70, 15);
            lblDescription.TabIndex = 18;
            lblDescription.Text = "Description:";
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Location = new System.Drawing.Point(12, 7);
            lblID.Name = "lblID";
            lblID.Size = new System.Drawing.Size(72, 15);
            lblID.TabIndex = 17;
            lblID.Text = "Resource ID:";
            // 
            // chkUpdate
            // 
            chkUpdate.AutoSize = true;
            chkUpdate.Location = new System.Drawing.Point(183, 23);
            chkUpdate.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkUpdate.Name = "chkUpdate";
            chkUpdate.Size = new System.Drawing.Size(101, 19);
            chkUpdate.TabIndex = 16;
            chkUpdate.Text = "Update Logics";
            chkUpdate.UseVisualStyleBackColor = true;
            chkUpdate.Visible = false;
            // 
            // txtDescription
            // 
            txtDescription.Location = new System.Drawing.Point(12, 62);
            txtDescription.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtDescription.Size = new System.Drawing.Size(272, 116);
            txtDescription.TabIndex = 15;
            txtDescription.TextChanged += txtDescription_TextChanged;
            // 
            // txtID
            // 
            txtID.Location = new System.Drawing.Point(12, 23);
            txtID.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            txtID.Name = "txtID";
            txtID.Size = new System.Drawing.Size(167, 23);
            txtID.TabIndex = 14;
            txtID.WordWrap = false;
            txtID.TextChanged += txtID_TextChanged;
            txtID.KeyPress += txtID_KeyPress;
            // 
            // frmEditDescription
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(293, 214);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(lblDescription);
            Controls.Add(lblID);
            Controls.Add(chkUpdate);
            Controls.Add(txtDescription);
            Controls.Add(txtID);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmEditDescription";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Edit ID and Description";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblID;
        public System.Windows.Forms.CheckBox chkUpdate;
        public System.Windows.Forms.TextBox txtDescription;
        public System.Windows.Forms.TextBox txtID;
    }
}