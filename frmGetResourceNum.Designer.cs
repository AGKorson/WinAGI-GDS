
namespace WinAGI.Editor {
    partial class frmGetResourceNum {
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
            txtID = new System.Windows.Forms.TextBox();
            chkRoom = new System.Windows.Forms.CheckBox();
            txtDescription = new System.Windows.Forms.TextBox();
            chkIncludePic = new System.Windows.Forms.CheckBox();
            chkOpenRes = new System.Windows.Forms.CheckBox();
            lstResNum = new System.Windows.Forms.ListBox();
            lblID = new System.Windows.Forms.Label();
            lblDescription = new System.Windows.Forms.Label();
            Label1 = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnDont = new System.Windows.Forms.Button();
            Label2 = new System.Windows.Forms.Label();
            lblCurrent = new System.Windows.Forms.Label();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txtID
            // 
            txtID.Location = new System.Drawing.Point(241, 24);
            txtID.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            txtID.Name = "txtID";
            txtID.Size = new System.Drawing.Size(216, 23);
            txtID.TabIndex = 0;
            txtID.WordWrap = false;
            txtID.TextChanged += txtID_TextChanged;
            txtID.KeyPress += txtID_KeyPress;
            // 
            // chkRoom
            // 
            chkRoom.AutoSize = true;
            chkRoom.Location = new System.Drawing.Point(257, 157);
            chkRoom.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkRoom.Name = "chkRoom";
            chkRoom.Size = new System.Drawing.Size(182, 19);
            chkRoom.TabIndex = 1;
            chkRoom.Text = "Include Room Template Code";
            chkRoom.UseVisualStyleBackColor = true;
            chkRoom.Visible = false;
            chkRoom.CheckedChanged += chkRoom_CheckedChanged;
            // 
            // txtDescription
            // 
            txtDescription.Location = new System.Drawing.Point(241, 63);
            txtDescription.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtDescription.Size = new System.Drawing.Size(216, 72);
            txtDescription.TabIndex = 2;
            // 
            // chkIncludePic
            // 
            chkIncludePic.AutoSize = true;
            chkIncludePic.Location = new System.Drawing.Point(257, 137);
            chkIncludePic.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkIncludePic.Name = "chkIncludePic";
            chkIncludePic.Size = new System.Drawing.Size(154, 19);
            chkIncludePic.TabIndex = 3;
            chkIncludePic.Text = "Create Matching Picture";
            chkIncludePic.UseVisualStyleBackColor = true;
            chkIncludePic.Visible = false;
            chkIncludePic.CheckedChanged += chkIncludePic_CheckedChanged;
            // 
            // chkOpenRes
            // 
            chkOpenRes.AutoSize = true;
            chkOpenRes.Location = new System.Drawing.Point(12, 157);
            chkOpenRes.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkOpenRes.Name = "chkOpenRes";
            chkOpenRes.Size = new System.Drawing.Size(186, 19);
            chkOpenRes.TabIndex = 4;
            chkOpenRes.Text = "Open new resource for editing";
            chkOpenRes.UseVisualStyleBackColor = true;
            chkOpenRes.Visible = false;
            // 
            // lstResNum
            // 
            lstResNum.FormattingEnabled = true;
            lstResNum.ItemHeight = 15;
            lstResNum.Location = new System.Drawing.Point(12, 73);
            lstResNum.Name = "lstResNum";
            lstResNum.Size = new System.Drawing.Size(205, 79);
            lstResNum.TabIndex = 5;
            lstResNum.SelectedIndexChanged += lstResNum_SelectedIndexChanged;
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Location = new System.Drawing.Point(241, 8);
            lblID.Name = "lblID";
            lblID.Size = new System.Drawing.Size(72, 15);
            lblID.TabIndex = 6;
            lblID.Text = "Resource ID:";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new System.Drawing.Point(241, 47);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(70, 15);
            lblDescription.TabIndex = 7;
            lblDescription.Text = "Description:";
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Location = new System.Drawing.Point(11, 55);
            Label1.Name = "Label1";
            Label1.Size = new System.Drawing.Size(179, 15);
            Label1.TabIndex = 8;
            Label1.Text = "Select a view for testing pictures:";
            // 
            // btnOK
            // 
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Enabled = false;
            btnOK.Location = new System.Drawing.Point(11, 180);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 25);
            btnOK.TabIndex = 9;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnDont
            //
            btnDont.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnDont.Location = new System.Drawing.Point(92, 180);
            btnDont.Margin = new System.Windows.Forms.Padding(0);
            btnDont.Name = "btnDont";
            btnDont.Size = new System.Drawing.Size(125, 25);
            btnDont.TabIndex = 10;
            btnDont.Text = "Open Don't Import";
            btnDont.UseVisualStyleBackColor = true;
            btnDont.Visible = false;
            btnDont.Click += btnDont_Click;
            // 
            // Label2
            // 
            Label2.Font = new System.Drawing.Font("Segoe UI", 10.125F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            Label2.Location = new System.Drawing.Point(37, 7);
            Label2.Name = "Label2";
            Label2.Size = new System.Drawing.Size(161, 15);
            Label2.TabIndex = 11;
            Label2.Text = "Current Resource:";
            Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCurrent
            // 
            lblCurrent.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblCurrent.Location = new System.Drawing.Point(74, 24);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.Size = new System.Drawing.Size(73, 23);
            lblCurrent.TabIndex = 12;
            lblCurrent.Text = "1";
            lblCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(142, 180);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 25);
            btnCancel.TabIndex = 13;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmGetResourceNum
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(468, 218);
            Controls.Add(btnCancel);
            Controls.Add(lblCurrent);
            Controls.Add(Label2);
            Controls.Add(btnDont);
            Controls.Add(btnOK);
            Controls.Add(Label1);
            Controls.Add(lblDescription);
            Controls.Add(lblID);
            Controls.Add(lstResNum);
            Controls.Add(chkOpenRes);
            Controls.Add(chkIncludePic);
            Controls.Add(txtDescription);
            Controls.Add(chkRoom);
            Controls.Add(txtID);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmGetResourceNum";
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Add View";
            FormClosed += frmGetResourceNum_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public System.Windows.Forms.TextBox txtID;
        public System.Windows.Forms.CheckBox chkRoom;
        public System.Windows.Forms.TextBox txtDescription;
        public System.Windows.Forms.CheckBox chkIncludePic;
        public System.Windows.Forms.CheckBox chkOpenRes;
        private System.Windows.Forms.ListBox lstResNum;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnDont;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Button btnCancel;
    }
}