
namespace WinAGI_GDS
{
  partial class frmPreview
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
      if (disposing && (components != null))
      {
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
      this.pnlLogic = new System.Windows.Forms.Panel();
      this.pnlPicture = new System.Windows.Forms.Panel();
      this.pnlSound = new System.Windows.Forms.Panel();
      this.pnlView = new System.Windows.Forms.Panel();
      this.richTextBox1 = new System.Windows.Forms.RichTextBox();
      this.pnlLogic.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlLogic
      // 
      this.pnlLogic.Controls.Add(this.richTextBox1);
      this.pnlLogic.Location = new System.Drawing.Point(12, 12);
      this.pnlLogic.Name = "pnlLogic";
      this.pnlLogic.Size = new System.Drawing.Size(396, 426);
      this.pnlLogic.TabIndex = 0;
      // 
      // pnlPicture
      // 
      this.pnlPicture.Location = new System.Drawing.Point(553, 52);
      this.pnlPicture.Name = "pnlPicture";
      this.pnlPicture.Size = new System.Drawing.Size(294, 204);
      this.pnlPicture.TabIndex = 1;
      // 
      // pnlSound
      // 
      this.pnlSound.Location = new System.Drawing.Point(705, 282);
      this.pnlSound.Name = "pnlSound";
      this.pnlSound.Size = new System.Drawing.Size(230, 141);
      this.pnlSound.TabIndex = 2;
      // 
      // pnlView
      // 
      this.pnlView.Location = new System.Drawing.Point(414, 307);
      this.pnlView.Name = "pnlView";
      this.pnlView.Size = new System.Drawing.Size(241, 107);
      this.pnlView.TabIndex = 3;
      // 
      // richTextBox1
      // 
      this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.richTextBox1.DetectUrls = false;
      this.richTextBox1.Location = new System.Drawing.Point(10, 10);
      this.richTextBox1.Name = "richTextBox1";
      this.richTextBox1.ReadOnly = true;
      this.richTextBox1.Size = new System.Drawing.Size(376, 406);
      this.richTextBox1.TabIndex = 0;
      this.richTextBox1.Text = "";
      // 
      // frmPreview
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.pnlSound);
      this.Controls.Add(this.pnlView);
      this.Controls.Add(this.pnlPicture);
      this.Controls.Add(this.pnlLogic);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "frmPreview";
      this.Text = "Form1";
      this.pnlLogic.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel pnlLogic;
    private System.Windows.Forms.RichTextBox richTextBox1;
    private System.Windows.Forms.Panel pnlPicture;
    private System.Windows.Forms.Panel pnlSound;
    private System.Windows.Forms.Panel pnlView;
  }
}