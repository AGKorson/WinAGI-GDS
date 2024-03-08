
namespace WinAGI.Editor
{
  partial class frmGetResourceNum
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
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
        private void InitializeComponent()
        {
            txtID = new System.Windows.Forms.TextBox();
            chkRoom = new System.Windows.Forms.CheckBox();
            txtDescription = new System.Windows.Forms.TextBox();
            chkIncludePic = new System.Windows.Forms.CheckBox();
            chkOpenRes = new System.Windows.Forms.CheckBox();
            lstResNum = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnOpen = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txtID
            // 
            txtID.Location = new System.Drawing.Point(250, 40);
            txtID.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            txtID.Name = "txtID";
            txtID.Size = new System.Drawing.Size(68, 23);
            txtID.TabIndex = 0;
            // 
            // chkRoom
            // 
            chkRoom.AutoSize = true;
            chkRoom.Location = new System.Drawing.Point(250, 158);
            chkRoom.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkRoom.Name = "chkRoom";
            chkRoom.Size = new System.Drawing.Size(55, 19);
            chkRoom.TabIndex = 1;
            chkRoom.Text = "room";
            chkRoom.UseVisualStyleBackColor = true;
            // 
            // txtDescription
            // 
            txtDescription.Location = new System.Drawing.Point(250, 83);
            txtDescription.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new System.Drawing.Size(99, 23);
            txtDescription.TabIndex = 2;
            // 
            // chkIncludePic
            // 
            chkIncludePic.AutoSize = true;
            chkIncludePic.Location = new System.Drawing.Point(250, 182);
            chkIncludePic.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkIncludePic.Name = "chkIncludePic";
            chkIncludePic.Size = new System.Drawing.Size(105, 19);
            chkIncludePic.TabIndex = 3;
            chkIncludePic.Text = "include picture";
            chkIncludePic.UseVisualStyleBackColor = true;
            // 
            // chkOpenRes
            // 
            chkOpenRes.AutoSize = true;
            chkOpenRes.Location = new System.Drawing.Point(11, 141);
            chkOpenRes.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkOpenRes.Name = "chkOpenRes";
            chkOpenRes.Size = new System.Drawing.Size(71, 19);
            chkOpenRes.TabIndex = 4;
            chkOpenRes.Text = "open res";
            chkOpenRes.UseVisualStyleBackColor = true;
            // 
            // lstResNum
            // 
            lstResNum.FormattingEnabled = true;
            lstResNum.ItemHeight = 15;
            lstResNum.Location = new System.Drawing.Point(12, 40);
            lstResNum.Name = "lstResNum";
            lstResNum.Size = new System.Drawing.Size(120, 94);
            lstResNum.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(250, 24);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 15);
            label1.TabIndex = 6;
            label1.Text = "Resource ID";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(250, 67);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(67, 15);
            label2.TabIndex = 7;
            label2.Text = "Description";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 24);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(102, 15);
            label3.TabIndex = 8;
            label3.Text = "Resource Number";
            // 
            // btnOK
            // 
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Location = new System.Drawing.Point(11, 179);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 9;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnOpen
            // 
            btnOpen.Location = new System.Drawing.Point(92, 179);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new System.Drawing.Size(134, 23);
            btnOpen.TabIndex = 10;
            btnOpen.Text = "Open Don't Import";
            btnOpen.UseVisualStyleBackColor = true;
            // 
            // frmGetResourceNum
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(431, 211);
            Controls.Add(btnOpen);
            Controls.Add(btnOK);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lstResNum);
            Controls.Add(chkOpenRes);
            Controls.Add(chkIncludePic);
            Controls.Add(txtDescription);
            Controls.Add(chkRoom);
            Controls.Add(txtID);
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            Name = "frmGetResourceNum";
            Text = "frmGetResourceNum";
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnOpen;
    }
}