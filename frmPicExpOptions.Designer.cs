
namespace WinAGI.Editor
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
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.fraChoice.SuspendLayout();
      this.fraImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udZoom)).BeginInit();
      this.SuspendLayout();
      // 
      // fraChoice
      // 
      this.fraChoice.Controls.Add(this.optImage);
      this.fraChoice.Controls.Add(this.optResource);
      this.fraChoice.Location = new System.Drawing.Point(30, 30);
      this.fraChoice.Margin = new System.Windows.Forms.Padding(6);
      this.fraChoice.Name = "fraChoice";
      this.fraChoice.Padding = new System.Windows.Forms.Padding(6);
      this.fraChoice.Size = new System.Drawing.Size(492, 105);
      this.fraChoice.TabIndex = 0;
      this.fraChoice.TabStop = false;
      // 
      // optImage
      // 
      this.optImage.AutoSize = true;
      this.optImage.Location = new System.Drawing.Point(267, 38);
      this.optImage.Margin = new System.Windows.Forms.Padding(6);
      this.optImage.Name = "optImage";
      this.optImage.Size = new System.Drawing.Size(191, 36);
      this.optImage.TabIndex = 1;
      this.optImage.TabStop = true;
      this.optImage.Text = "Picture Image";
      this.optImage.UseVisualStyleBackColor = true;
      this.optImage.CheckedChanged += new System.EventHandler(this.optImageFormat);
      // 
      // optResource
      // 
      this.optResource.AutoSize = true;
      this.optResource.Location = new System.Drawing.Point(30, 38);
      this.optResource.Margin = new System.Windows.Forms.Padding(6);
      this.optResource.Name = "optResource";
      this.optResource.Size = new System.Drawing.Size(185, 36);
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
      this.fraImage.Location = new System.Drawing.Point(30, 154);
      this.fraImage.Margin = new System.Windows.Forms.Padding(6);
      this.fraImage.Name = "fraImage";
      this.fraImage.Padding = new System.Windows.Forms.Padding(6);
      this.fraImage.Size = new System.Drawing.Size(299, 190);
      this.fraImage.TabIndex = 1;
      this.fraImage.TabStop = false;
      this.fraImage.Text = "Image to export";
      // 
      // optBoth
      // 
      this.optBoth.AutoSize = true;
      this.optBoth.Location = new System.Drawing.Point(45, 132);
      this.optBoth.Margin = new System.Windows.Forms.Padding(6);
      this.optBoth.Name = "optBoth";
      this.optBoth.Size = new System.Drawing.Size(95, 36);
      this.optBoth.TabIndex = 2;
      this.optBoth.TabStop = true;
      this.optBoth.Text = "Both";
      this.optBoth.UseVisualStyleBackColor = true;
      this.optBoth.CheckedChanged += new System.EventHandler(this.optImageType_CheckedChanged);
      // 
      // optPriority
      // 
      this.optPriority.AutoSize = true;
      this.optPriority.Location = new System.Drawing.Point(45, 85);
      this.optPriority.Margin = new System.Windows.Forms.Padding(6);
      this.optPriority.Name = "optPriority";
      this.optPriority.Size = new System.Drawing.Size(200, 36);
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
      this.optVisual.Location = new System.Drawing.Point(45, 38);
      this.optVisual.Margin = new System.Windows.Forms.Padding(6);
      this.optVisual.Name = "optVisual";
      this.optVisual.Size = new System.Drawing.Size(188, 36);
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
      this.cmbFormat.Location = new System.Drawing.Point(30, 384);
      this.cmbFormat.Margin = new System.Windows.Forms.Padding(6);
      this.cmbFormat.Name = "cmbFormat";
      this.cmbFormat.Size = new System.Drawing.Size(296, 40);
      this.cmbFormat.TabIndex = 2;
      // 
      // lblFormat
      // 
      this.lblFormat.AutoSize = true;
      this.lblFormat.Location = new System.Drawing.Point(30, 350);
      this.lblFormat.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.lblFormat.Name = "lblFormat";
      this.lblFormat.Size = new System.Drawing.Size(167, 32);
      this.lblFormat.TabIndex = 3;
      this.lblFormat.Text = "Image Format:";
      // 
      // lblScale
      // 
      this.lblScale.AutoSize = true;
      this.lblScale.Location = new System.Drawing.Point(371, 256);
      this.lblScale.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.lblScale.Name = "lblScale";
      this.lblScale.Size = new System.Drawing.Size(74, 32);
      this.lblScale.TabIndex = 4;
      this.lblScale.Text = "Scale:";
      // 
      // udZoom
      // 
      this.udZoom.Location = new System.Drawing.Point(451, 252);
      this.udZoom.Margin = new System.Windows.Forms.Padding(6);
      this.udZoom.Name = "udZoom";
      this.udZoom.Size = new System.Drawing.Size(132, 39);
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
      this.lblBoth.Location = new System.Drawing.Point(371, 341);
      this.lblBoth.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
      this.lblBoth.Name = "lblBoth";
      this.lblBoth.Size = new System.Drawing.Size(329, 105);
      this.lblBoth.TabIndex = 6;
      this.lblBoth.Text = "File name for the Priority Screen will be the same as the Visual Screen with \"_P\"" +
    " appended.";
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(579, 26);
      this.btnOK.Margin = new System.Windows.Forms.Padding(6);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(150, 53);
      this.btnOK.TabIndex = 7;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.OKButton_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(579, 94);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(6);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(150, 53);
      this.btnCancel.TabIndex = 8;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.CancelButton_Click);
      // 
      // frmPicExpOptions
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(752, 489);
      this.ControlBox = false;
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.lblBoth);
      this.Controls.Add(this.udZoom);
      this.Controls.Add(this.lblScale);
      this.Controls.Add(this.lblFormat);
      this.Controls.Add(this.cmbFormat);
      this.Controls.Add(this.fraImage);
      this.Controls.Add(this.fraChoice);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(6);
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
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    public System.Windows.Forms.RadioButton optPriority;
    public System.Windows.Forms.RadioButton optVisual;
    public System.Windows.Forms.ComboBox cmbFormat;
    public System.Windows.Forms.NumericUpDown udZoom;
  }
}