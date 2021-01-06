
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
      this.picVisual = new System.Windows.Forms.PictureBox();
      this.trackBar1 = new System.Windows.Forms.TrackBar();
      ((System.ComponentModel.ISupportInitialize)(this.picVisual)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
      this.SuspendLayout();
      // 
      // picVisual
      // 
      this.picVisual.Location = new System.Drawing.Point(22, 26);
      this.picVisual.Margin = new System.Windows.Forms.Padding(6);
      this.picVisual.Name = "picVisual";
      this.picVisual.Size = new System.Drawing.Size(1189, 717);
      this.picVisual.TabIndex = 0;
      this.picVisual.TabStop = false;
      this.picVisual.Click += new System.EventHandler(this.picVisual_Click);
      // 
      // trackBar1
      // 
      this.trackBar1.Location = new System.Drawing.Point(465, 826);
      this.trackBar1.Name = "trackBar1";
      this.trackBar1.Size = new System.Drawing.Size(899, 90);
      this.trackBar1.TabIndex = 2;
      this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
      // 
      // frmPicEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1486, 960);
      this.Controls.Add(this.trackBar1);
      this.Controls.Add(this.picVisual);
      this.Margin = new System.Windows.Forms.Padding(6);
      this.Name = "frmPicEdit";
      this.Text = "frmPicEdit";
      this.Load += new System.EventHandler(this.frmPicEdit_Load);
      ((System.ComponentModel.ISupportInitialize)(this.picVisual)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox picVisual;
    private System.Windows.Forms.TrackBar trackBar1;
  }
}