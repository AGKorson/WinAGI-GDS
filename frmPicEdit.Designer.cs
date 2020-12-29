
namespace WinAGI_GDS
{
  partial class frmPicEdit
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPicEdit));
      this.picVisual = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.picVisual)).BeginInit();
      this.SuspendLayout();
      // 
      // picVisual
      // 
      this.picVisual.Image = ((System.Drawing.Image)(resources.GetObject("picVisual.Image")));
      this.picVisual.Location = new System.Drawing.Point(12, 12);
      this.picVisual.Name = "picVisual";
      this.picVisual.Size = new System.Drawing.Size(640, 336);
      this.picVisual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.picVisual.TabIndex = 0;
      this.picVisual.TabStop = false;
      // 
      // frmPicEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.picVisual);
      this.Name = "frmPicEdit";
      this.Text = "frmPicEdit";
      ((System.ComponentModel.ISupportInitialize)(this.picVisual)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox picVisual;
  }
}