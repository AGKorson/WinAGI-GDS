
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
      this.txtID = new System.Windows.Forms.TextBox();
      this.chkRoom = new System.Windows.Forms.CheckBox();
      this.txtDescription = new System.Windows.Forms.TextBox();
      this.chkIncludePic = new System.Windows.Forms.CheckBox();
      this.chkOpenRes = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // txtID
      // 
      this.txtID.Location = new System.Drawing.Point(142, 107);
      this.txtID.Name = "txtID";
      this.txtID.Size = new System.Drawing.Size(122, 39);
      this.txtID.TabIndex = 0;
      // 
      // chkRoom
      // 
      this.chkRoom.AutoSize = true;
      this.chkRoom.Location = new System.Drawing.Point(142, 270);
      this.chkRoom.Name = "chkRoom";
      this.chkRoom.Size = new System.Drawing.Size(103, 36);
      this.chkRoom.TabIndex = 1;
      this.chkRoom.Text = "room";
      this.chkRoom.UseVisualStyleBackColor = true;
      // 
      // txtDescription
      // 
      this.txtDescription.Location = new System.Drawing.Point(148, 173);
      this.txtDescription.Name = "txtDescription";
      this.txtDescription.Size = new System.Drawing.Size(181, 39);
      this.txtDescription.TabIndex = 2;
      // 
      // chkIncludePic
      // 
      this.chkIncludePic.AutoSize = true;
      this.chkIncludePic.Location = new System.Drawing.Point(142, 323);
      this.chkIncludePic.Name = "chkIncludePic";
      this.chkIncludePic.Size = new System.Drawing.Size(205, 36);
      this.chkIncludePic.TabIndex = 3;
      this.chkIncludePic.Text = "include picture";
      this.chkIncludePic.UseVisualStyleBackColor = true;
      // 
      // chkOpenRes
      // 
      this.chkOpenRes.AutoSize = true;
      this.chkOpenRes.Location = new System.Drawing.Point(142, 377);
      this.chkOpenRes.Name = "chkOpenRes";
      this.chkOpenRes.Size = new System.Drawing.Size(139, 36);
      this.chkOpenRes.TabIndex = 4;
      this.chkOpenRes.Text = "open res";
      this.chkOpenRes.UseVisualStyleBackColor = true;
      // 
      // frmGetResourceNum
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.chkOpenRes);
      this.Controls.Add(this.chkIncludePic);
      this.Controls.Add(this.txtDescription);
      this.Controls.Add(this.chkRoom);
      this.Controls.Add(this.txtID);
      this.Name = "frmGetResourceNum";
      this.Text = "frmGetResourceNum";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    public System.Windows.Forms.TextBox txtID;
    public System.Windows.Forms.CheckBox chkRoom;
    public System.Windows.Forms.TextBox txtDescription;
    public System.Windows.Forms.CheckBox chkIncludePic;
    public System.Windows.Forms.CheckBox chkOpenRes;
  }
}