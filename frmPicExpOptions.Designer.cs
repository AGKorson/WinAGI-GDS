
namespace WinAGI_GDS
{
  partial class frmPicExpOptions
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPicExpOptions));
      this.fraChoice = new System.Windows.Forms.GroupBox();
      this.optImage = new System.Windows.Forms.RadioButton();
      this.optResource = new System.Windows.Forms.RadioButton();
      this.fraImage = new System.Windows.Forms.GroupBox();
      this.optBoth = new System.Windows.Forms.RadioButton();
      this.optPriority = new System.Windows.Forms.RadioButton();
      this.optVisual = new System.Windows.Forms.RadioButton();
      this.cmbFormat = new System.Windows.Forms.ComboBox();
      this.lblFormat = new System.Windows.Forms.Label();
      this.lblScale = new System.Windows.Forms.Label();
      this.udZoom = new System.Windows.Forms.NumericUpDown();
      this.lblBoth = new System.Windows.Forms.Label();
      this.OKButton = new System.Windows.Forms.Button();
      this.CancelButton = new System.Windows.Forms.Button();
      this.fraChoice.SuspendLayout();
      this.fraImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udZoom)).BeginInit();
      this.SuspendLayout();
      // 
      // fraChoice
      // 
      this.fraChoice.Controls.Add(this.optImage);
      this.fraChoice.Controls.Add(this.optResource);
      this.fraChoice.Location = new System.Drawing.Point(16, 14);
      this.fraChoice.Name = "fraChoice";
      this.fraChoice.Size = new System.Drawing.Size(265, 49);
      this.fraChoice.TabIndex = 0;
      this.fraChoice.TabStop = false;
      // 
      // optImage
      // 
      this.optImage.AutoSize = true;
      this.optImage.Location = new System.Drawing.Point(144, 18);
      this.optImage.Name = "optImage";
      this.optImage.Size = new System.Drawing.Size(98, 19);
      this.optImage.TabIndex = 1;
      this.optImage.TabStop = true;
      this.optImage.Text = "Picture Image";
      this.optImage.UseVisualStyleBackColor = true;
      this.optImage.CheckedChanged += new System.EventHandler(this.optImageFormat);
      // 
      // optResource
      // 
      this.optResource.AutoSize = true;
      this.optResource.Location = new System.Drawing.Point(16, 18);
      this.optResource.Name = "optResource";
      this.optResource.Size = new System.Drawing.Size(95, 19);
      this.optResource.TabIndex = 0;
      this.optResource.TabStop = true;
      this.optResource.Text = "AGI Resource";
      this.optResource.UseVisualStyleBackColor = true;
      this.optResource.CheckedChanged += new System.EventHandler(this.optImageFormat);
      // 
      // fraImage
      // 
      this.fraImage.Controls.Add(this.optBoth);
      this.fraImage.Controls.Add(this.optPriority);
      this.fraImage.Controls.Add(this.optVisual);
      this.fraImage.Location = new System.Drawing.Point(16, 72);
      this.fraImage.Name = "fraImage";
      this.fraImage.Size = new System.Drawing.Size(161, 89);
      this.fraImage.TabIndex = 1;
      this.fraImage.TabStop = false;
      this.fraImage.Text = "Image to export";
      // 
      // optBoth
      // 
      this.optBoth.AutoSize = true;
      this.optBoth.Location = new System.Drawing.Point(24, 62);
      this.optBoth.Name = "optBoth";
      this.optBoth.Size = new System.Drawing.Size(50, 19);
      this.optBoth.TabIndex = 2;
      this.optBoth.TabStop = true;
      this.optBoth.Text = "Both";
      this.optBoth.UseVisualStyleBackColor = true;
      this.optBoth.CheckedChanged += new System.EventHandler(this.optImageType_CheckedChanged);
      // 
      // optPriority
      // 
      this.optPriority.AutoSize = true;
      this.optPriority.Location = new System.Drawing.Point(24, 40);
      this.optPriority.Name = "optPriority";
      this.optPriority.Size = new System.Drawing.Size(103, 19);
      this.optPriority.TabIndex = 1;
      this.optPriority.TabStop = true;
      this.optPriority.Text = "Priority Picture";
      this.optPriority.UseVisualStyleBackColor = true;
      this.optPriority.CheckedChanged += new System.EventHandler(this.optImageType_CheckedChanged);
      this.optPriority.Click += new System.EventHandler(this.optImageType_CheckedChanged);
      // 
      // optVisual
      // 
      this.optVisual.AutoSize = true;
      this.optVisual.Location = new System.Drawing.Point(24, 18);
      this.optVisual.Name = "optVisual";
      this.optVisual.Size = new System.Drawing.Size(96, 19);
      this.optVisual.TabIndex = 0;
      this.optVisual.TabStop = true;
      this.optVisual.Text = "Visual Picture";
      this.optVisual.UseVisualStyleBackColor = true;
      this.optVisual.CheckedChanged += new System.EventHandler(this.optImageType_CheckedChanged);
      this.optVisual.Click += new System.EventHandler(this.optImageType_CheckedChanged);
      // 
      // cmbFormat
      // 
      this.cmbFormat.FormattingEnabled = true;
      this.cmbFormat.Items.AddRange(new object[] {
            "Bitmap",
            "JPG",
            "GIF",
            "TIF",
            "PNG"});
      this.cmbFormat.Location = new System.Drawing.Point(16, 180);
      this.cmbFormat.Name = "cmbFormat";
      this.cmbFormat.Size = new System.Drawing.Size(161, 23);
      this.cmbFormat.TabIndex = 2;
      // 
      // lblFormat
      // 
      this.lblFormat.AutoSize = true;
      this.lblFormat.Location = new System.Drawing.Point(16, 164);
      this.lblFormat.Name = "lblFormat";
      this.lblFormat.Size = new System.Drawing.Size(84, 15);
      this.lblFormat.TabIndex = 3;
      this.lblFormat.Text = "Image Format:";
      // 
      // lblScale
      // 
      this.lblScale.AutoSize = true;
      this.lblScale.Location = new System.Drawing.Point(200, 120);
      this.lblScale.Name = "lblScale";
      this.lblScale.Size = new System.Drawing.Size(37, 15);
      this.lblScale.TabIndex = 4;
      this.lblScale.Text = "Scale:";
      // 
      // udZoom
      // 
      this.udZoom.Location = new System.Drawing.Point(243, 118);
      this.udZoom.Name = "udZoom";
      this.udZoom.Size = new System.Drawing.Size(71, 23);
      this.udZoom.TabIndex = 5;
      this.udZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.udZoom.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // lblBoth
      // 
      this.lblBoth.Location = new System.Drawing.Point(200, 160);
      this.lblBoth.Name = "lblBoth";
      this.lblBoth.Size = new System.Drawing.Size(177, 49);
      this.lblBoth.TabIndex = 6;
      this.lblBoth.Text = "File name for the Priority Screen will be the same as the Visual Screen with \"_P\"" +
    " appended.";
      // 
      // OKButton
      // 
      this.OKButton.Location = new System.Drawing.Point(312, 12);
      this.OKButton.Name = "OKButton";
      this.OKButton.Size = new System.Drawing.Size(81, 25);
      this.OKButton.TabIndex = 7;
      this.OKButton.Text = "OK";
      this.OKButton.UseVisualStyleBackColor = true;
      this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
      // 
      // CancelButton
      // 
      this.CancelButton.Location = new System.Drawing.Point(312, 44);
      this.CancelButton.Name = "CancelButton";
      this.CancelButton.Size = new System.Drawing.Size(81, 25);
      this.CancelButton.TabIndex = 8;
      this.CancelButton.Text = "Cancel";
      this.CancelButton.UseVisualStyleBackColor = true;
      this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
      // 
      // frmPicExpOptions
      // 
      this.AcceptButton = this.OKButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.CancelButton;
      this.ClientSize = new System.Drawing.Size(405, 229);
      this.ControlBox = false;
      this.Controls.Add(this.CancelButton);
      this.Controls.Add(this.OKButton);
      this.Controls.Add(this.lblBoth);
      this.Controls.Add(this.udZoom);
      this.Controls.Add(this.lblScale);
      this.Controls.Add(this.lblFormat);
      this.Controls.Add(this.cmbFormat);
      this.Controls.Add(this.fraImage);
      this.Controls.Add(this.fraChoice);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "frmPicExpOptions";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "frmPicExpOptions";
      this.fraChoice.ResumeLayout(false);
      this.fraChoice.PerformLayout();
      this.fraImage.ResumeLayout(false);
      this.fraImage.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udZoom)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox fraChoice;
    private System.Windows.Forms.RadioButton optImage;
    private System.Windows.Forms.RadioButton optResource;
    private System.Windows.Forms.GroupBox fraImage;
    private System.Windows.Forms.RadioButton optBoth;
    private System.Windows.Forms.Label lblFormat;
    private System.Windows.Forms.Label lblScale;
    private System.Windows.Forms.Label lblBoth;
    private System.Windows.Forms.Button OKButton;
    private System.Windows.Forms.Button CancelButton;
    public System.Windows.Forms.RadioButton optPriority;
    public System.Windows.Forms.RadioButton optVisual;
    public System.Windows.Forms.ComboBox cmbFormat;
    public System.Windows.Forms.NumericUpDown udZoom;
  }
}