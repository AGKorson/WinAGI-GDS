
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
      this.cmbTransCol = new System.Windows.Forms.ComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.picVisual)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
      this.SuspendLayout();
      // 
      // picVisual
      // 
      this.picVisual.Location = new System.Drawing.Point(12, 12);
      this.picVisual.Name = "picVisual";
      this.picVisual.Size = new System.Drawing.Size(640, 336);
      this.picVisual.TabIndex = 0;
      this.picVisual.TabStop = false;
      this.picVisual.Click += new System.EventHandler(this.picVisual_Click);
      // 
      // trackBar1
      // 
      this.trackBar1.Location = new System.Drawing.Point(11, 395);
      this.trackBar1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.trackBar1.Name = "trackBar1";
      this.trackBar1.Size = new System.Drawing.Size(484, 45);
      this.trackBar1.TabIndex = 2;
      this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
      // 
      // cmbTransCol
      // 
      this.cmbTransCol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbTransCol.FormattingEnabled = true;
      this.cmbTransCol.Location = new System.Drawing.Point(530, 397);
      this.cmbTransCol.Name = "cmbTransCol";
      this.cmbTransCol.Size = new System.Drawing.Size(121, 23);
      this.cmbTransCol.TabIndex = 3;
      this.cmbTransCol.SelectionChangeCommitted += new System.EventHandler(this.cmbTransCol_SelectionChangeCommitted);
      // 
      // frmPicEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.cmbTransCol);
      this.Controls.Add(this.trackBar1);
      this.Controls.Add(this.picVisual);
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
    private System.Windows.Forms.ComboBox cmbTransCol;
  }
}