
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPreview));
      this.pnlLogic = new System.Windows.Forms.Panel();
      this.rtfLogPrev = new System.Windows.Forms.RichTextBox();
      this.pnlPicture = new System.Windows.Forms.Panel();
      this.imgPicture = new System.Windows.Forms.PictureBox();
      this.optPriority = new System.Windows.Forms.RadioButton();
      this.optVisual = new System.Windows.Forms.RadioButton();
      this.udPZoom = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.pnlSound = new System.Windows.Forms.Panel();
      this.pnlView = new System.Windows.Forms.Panel();
      this.trbSpeed = new System.Windows.Forms.TrackBar();
      this.cmbMotion = new System.Windows.Forms.ComboBox();
      this.btnVPlay = new System.Windows.Forms.Button();
      this.udCel = new System.Windows.Forms.DomainUpDown();
      this.udLoop = new System.Windows.Forms.DomainUpDown();
      this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
      this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.tsViewPrev = new System.Windows.Forms.ToolStrip();
      this.tbbZoomIn = new System.Windows.Forms.ToolStripButton();
      this.tbbZoomOut = new System.Windows.Forms.ToolStripButton();
      this.tsSep1 = new System.Windows.Forms.ToolStripSeparator();
      this.tbbHAlign = new System.Windows.Forms.ToolStripSplitButton();
      this.tbbAlignLeft = new System.Windows.Forms.ToolStripMenuItem();
      this.tbbAlignCenter = new System.Windows.Forms.ToolStripMenuItem();
      this.tbbAlignRight = new System.Windows.Forms.ToolStripMenuItem();
      this.tbbVAlign = new System.Windows.Forms.ToolStripSplitButton();
      this.tbbTop = new System.Windows.Forms.ToolStripMenuItem();
      this.tbbMiddle = new System.Windows.Forms.ToolStripMenuItem();
      this.tbbBottom = new System.Windows.Forms.ToolStripMenuItem();
      this.tbbTrans = new System.Windows.Forms.ToolStripButton();
      this.tmrMotion = new System.Windows.Forms.Timer(this.components);
      this.Timer1 = new System.Windows.Forms.Timer(this.components);
      this.pnlLogic.SuspendLayout();
      this.pnlPicture.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.imgPicture)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udPZoom)).BeginInit();
      this.pnlView.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trbSpeed)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.tsViewPrev.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlLogic
      // 
      this.pnlLogic.Controls.Add(this.rtfLogPrev);
      this.pnlLogic.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlLogic.Location = new System.Drawing.Point(0, 0);
      this.pnlLogic.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.pnlLogic.Name = "pnlLogic";
      this.pnlLogic.Size = new System.Drawing.Size(583, 423);
      this.pnlLogic.TabIndex = 0;
      this.pnlLogic.Visible = false;
      // 
      // rtfLogPrev
      // 
      this.rtfLogPrev.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rtfLogPrev.DetectUrls = false;
      this.rtfLogPrev.Location = new System.Drawing.Point(5, 5);
      this.rtfLogPrev.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.rtfLogPrev.Name = "rtfLogPrev";
      this.rtfLogPrev.ReadOnly = true;
      this.rtfLogPrev.Size = new System.Drawing.Size(574, 415);
      this.rtfLogPrev.TabIndex = 0;
      this.rtfLogPrev.Text = "";
      // 
      // pnlPicture
      // 
      this.pnlPicture.Controls.Add(this.imgPicture);
      this.pnlPicture.Controls.Add(this.optPriority);
      this.pnlPicture.Controls.Add(this.optVisual);
      this.pnlPicture.Controls.Add(this.udPZoom);
      this.pnlPicture.Controls.Add(this.label1);
      this.pnlPicture.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlPicture.Location = new System.Drawing.Point(0, 0);
      this.pnlPicture.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.pnlPicture.Name = "pnlPicture";
      this.pnlPicture.Size = new System.Drawing.Size(583, 423);
      this.pnlPicture.TabIndex = 1;
      this.pnlPicture.Visible = false;
      // 
      // imgPicture
      // 
      this.imgPicture.Image = ((System.Drawing.Image)(resources.GetObject("imgPicture.Image")));
      this.imgPicture.Location = new System.Drawing.Point(16, 70);
      this.imgPicture.Name = "imgPicture";
      this.imgPicture.Size = new System.Drawing.Size(320, 168);
      this.imgPicture.TabIndex = 4;
      this.imgPicture.TabStop = false;
      // 
      // optPriority
      // 
      this.optPriority.AutoSize = true;
      this.optPriority.Location = new System.Drawing.Point(75, 40);
      this.optPriority.Name = "optPriority";
      this.optPriority.Size = new System.Drawing.Size(63, 19);
      this.optPriority.TabIndex = 3;
      this.optPriority.Text = "Priority";
      this.optPriority.UseVisualStyleBackColor = true;
      // 
      // optVisual
      // 
      this.optVisual.AutoSize = true;
      this.optVisual.Checked = true;
      this.optVisual.Location = new System.Drawing.Point(75, 14);
      this.optVisual.Name = "optVisual";
      this.optVisual.Size = new System.Drawing.Size(56, 19);
      this.optVisual.TabIndex = 2;
      this.optVisual.TabStop = true;
      this.optVisual.Text = "Visual";
      this.optVisual.UseVisualStyleBackColor = true;
      this.optVisual.CheckedChanged += new System.EventHandler(this.optVisual_CheckedChanged);
      // 
      // udPZoom
      // 
      this.udPZoom.Location = new System.Drawing.Point(9, 28);
      this.udPZoom.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
      this.udPZoom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udPZoom.Name = "udPZoom";
      this.udPZoom.Size = new System.Drawing.Size(40, 23);
      this.udPZoom.TabIndex = 1;
      this.udPZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.udPZoom.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udPZoom.ValueChanged += new System.EventHandler(this.udPZoom_ValueChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(8, 7);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(42, 15);
      this.label1.TabIndex = 0;
      this.label1.Text = "Zoom:";
      // 
      // pnlSound
      // 
      this.pnlSound.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlSound.Location = new System.Drawing.Point(0, 0);
      this.pnlSound.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.pnlSound.Name = "pnlSound";
      this.pnlSound.Size = new System.Drawing.Size(583, 423);
      this.pnlSound.TabIndex = 2;
      this.pnlSound.Visible = false;
      // 
      // pnlView
      // 
      this.pnlView.Controls.Add(this.trbSpeed);
      this.pnlView.Controls.Add(this.cmbMotion);
      this.pnlView.Controls.Add(this.btnVPlay);
      this.pnlView.Controls.Add(this.udCel);
      this.pnlView.Controls.Add(this.udLoop);
      this.pnlView.Controls.Add(this.hScrollBar1);
      this.pnlView.Controls.Add(this.vScrollBar1);
      this.pnlView.Controls.Add(this.pictureBox1);
      this.pnlView.Controls.Add(this.tsViewPrev);
      this.pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlView.Location = new System.Drawing.Point(0, 0);
      this.pnlView.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.pnlView.Name = "pnlView";
      this.pnlView.Size = new System.Drawing.Size(583, 423);
      this.pnlView.TabIndex = 3;
      this.pnlView.Visible = false;
      // 
      // trbSpeed
      // 
      this.trbSpeed.Location = new System.Drawing.Point(175, 272);
      this.trbSpeed.Name = "trbSpeed";
      this.trbSpeed.Size = new System.Drawing.Size(100, 45);
      this.trbSpeed.TabIndex = 10;
      // 
      // cmbMotion
      // 
      this.cmbMotion.FormattingEnabled = true;
      this.cmbMotion.Location = new System.Drawing.Point(92, 274);
      this.cmbMotion.Name = "cmbMotion";
      this.cmbMotion.Size = new System.Drawing.Size(66, 23);
      this.cmbMotion.TabIndex = 9;
      // 
      // btnVPlay
      // 
      this.btnVPlay.Location = new System.Drawing.Point(9, 273);
      this.btnVPlay.Name = "btnVPlay";
      this.btnVPlay.Size = new System.Drawing.Size(73, 29);
      this.btnVPlay.TabIndex = 8;
      this.btnVPlay.Text = "button1";
      this.btnVPlay.UseVisualStyleBackColor = true;
      // 
      // udCel
      // 
      this.udCel.Location = new System.Drawing.Point(127, 26);
      this.udCel.Name = "udCel";
      this.udCel.Size = new System.Drawing.Size(111, 23);
      this.udCel.TabIndex = 7;
      this.udCel.Text = "domainUpDown2";
      // 
      // udLoop
      // 
      this.udLoop.Location = new System.Drawing.Point(9, 26);
      this.udLoop.Name = "udLoop";
      this.udLoop.Size = new System.Drawing.Size(112, 23);
      this.udLoop.TabIndex = 6;
      this.udLoop.Text = "domainUpDown1";
      // 
      // hScrollBar1
      // 
      this.hScrollBar1.Location = new System.Drawing.Point(23, 244);
      this.hScrollBar1.Name = "hScrollBar1";
      this.hScrollBar1.Size = new System.Drawing.Size(157, 16);
      this.hScrollBar1.TabIndex = 3;
      // 
      // vScrollBar1
      // 
      this.vScrollBar1.Location = new System.Drawing.Point(260, 55);
      this.vScrollBar1.Name = "vScrollBar1";
      this.vScrollBar1.Size = new System.Drawing.Size(16, 185);
      this.vScrollBar1.TabIndex = 2;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Location = new System.Drawing.Point(9, 55);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(150, 186);
      this.pictureBox1.TabIndex = 1;
      this.pictureBox1.TabStop = false;
      // 
      // tsViewPrev
      // 
      this.tsViewPrev.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbbZoomIn,
            this.tbbZoomOut,
            this.tsSep1,
            this.tbbHAlign,
            this.tbbVAlign,
            this.tbbTrans});
      this.tsViewPrev.Location = new System.Drawing.Point(0, 0);
      this.tsViewPrev.Name = "tsViewPrev";
      this.tsViewPrev.Size = new System.Drawing.Size(583, 25);
      this.tsViewPrev.TabIndex = 0;
      this.tsViewPrev.Text = "toolStrip1";
      // 
      // tbbZoomIn
      // 
      this.tbbZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbbZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("tbbZoomIn.Image")));
      this.tbbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbbZoomIn.Name = "tbbZoomIn";
      this.tbbZoomIn.Size = new System.Drawing.Size(23, 22);
      this.tbbZoomIn.Text = "toolStripButton1";
      // 
      // tbbZoomOut
      // 
      this.tbbZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbbZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("tbbZoomOut.Image")));
      this.tbbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbbZoomOut.Name = "tbbZoomOut";
      this.tbbZoomOut.Size = new System.Drawing.Size(23, 22);
      this.tbbZoomOut.Text = "toolStripButton2";
      // 
      // tsSep1
      // 
      this.tsSep1.Name = "tsSep1";
      this.tsSep1.Size = new System.Drawing.Size(6, 25);
      // 
      // tbbHAlign
      // 
      this.tbbHAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbbHAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbbAlignLeft,
            this.tbbAlignCenter,
            this.tbbAlignRight});
      this.tbbHAlign.Image = ((System.Drawing.Image)(resources.GetObject("tbbHAlign.Image")));
      this.tbbHAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbbHAlign.Name = "tbbHAlign";
      this.tbbHAlign.Size = new System.Drawing.Size(32, 22);
      this.tbbHAlign.Text = "toolStripSplitButton1";
      // 
      // tbbAlignLeft
      // 
      this.tbbAlignLeft.Image = ((System.Drawing.Image)(resources.GetObject("tbbAlignLeft.Image")));
      this.tbbAlignLeft.Name = "tbbAlignLeft";
      this.tbbAlignLeft.Size = new System.Drawing.Size(109, 22);
      this.tbbAlignLeft.Text = "Left";
      // 
      // tbbAlignCenter
      // 
      this.tbbAlignCenter.Image = ((System.Drawing.Image)(resources.GetObject("tbbAlignCenter.Image")));
      this.tbbAlignCenter.Name = "tbbAlignCenter";
      this.tbbAlignCenter.Size = new System.Drawing.Size(109, 22);
      this.tbbAlignCenter.Text = "Center";
      // 
      // tbbAlignRight
      // 
      this.tbbAlignRight.Image = ((System.Drawing.Image)(resources.GetObject("tbbAlignRight.Image")));
      this.tbbAlignRight.Name = "tbbAlignRight";
      this.tbbAlignRight.Size = new System.Drawing.Size(109, 22);
      this.tbbAlignRight.Text = "Right";
      // 
      // tbbVAlign
      // 
      this.tbbVAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.tbbVAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbbTop,
            this.tbbMiddle,
            this.tbbBottom});
      this.tbbVAlign.Image = ((System.Drawing.Image)(resources.GetObject("tbbVAlign.Image")));
      this.tbbVAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbbVAlign.Name = "tbbVAlign";
      this.tbbVAlign.Size = new System.Drawing.Size(32, 22);
      this.tbbVAlign.Text = "toolStripSplitButton2";
      // 
      // tbbTop
      // 
      this.tbbTop.Image = ((System.Drawing.Image)(resources.GetObject("tbbTop.Image")));
      this.tbbTop.Name = "tbbTop";
      this.tbbTop.Size = new System.Drawing.Size(114, 22);
      this.tbbTop.Text = "Top";
      // 
      // tbbMiddle
      // 
      this.tbbMiddle.Image = ((System.Drawing.Image)(resources.GetObject("tbbMiddle.Image")));
      this.tbbMiddle.Name = "tbbMiddle";
      this.tbbMiddle.Size = new System.Drawing.Size(114, 22);
      this.tbbMiddle.Text = "Middle";
      // 
      // tbbBottom
      // 
      this.tbbBottom.Image = ((System.Drawing.Image)(resources.GetObject("tbbBottom.Image")));
      this.tbbBottom.Name = "tbbBottom";
      this.tbbBottom.Size = new System.Drawing.Size(114, 22);
      this.tbbBottom.Text = "Bottom";
      // 
      // tbbTrans
      // 
      this.tbbTrans.Image = ((System.Drawing.Image)(resources.GetObject("tbbTrans.Image")));
      this.tbbTrans.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.tbbTrans.Name = "tbbTrans";
      this.tbbTrans.Size = new System.Drawing.Size(86, 22);
      this.tbbTrans.Text = "Show Trans";
      this.tbbTrans.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
      // 
      // tmrMotion
      // 
      this.tmrMotion.Tick += new System.EventHandler(this.tmrMotion_Tick);
      // 
      // frmPreview
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(583, 423);
      this.Controls.Add(this.pnlPicture);
      this.Controls.Add(this.pnlLogic);
      this.Controls.Add(this.pnlSound);
      this.Controls.Add(this.pnlView);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.Name = "frmPreview";
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.frmPreview_Load);
      this.pnlLogic.ResumeLayout(false);
      this.pnlPicture.ResumeLayout(false);
      this.pnlPicture.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.imgPicture)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udPZoom)).EndInit();
      this.pnlView.ResumeLayout(false);
      this.pnlView.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trbSpeed)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.tsViewPrev.ResumeLayout(false);
      this.tsViewPrev.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel pnlLogic;
    private System.Windows.Forms.RichTextBox rtfLogPrev;
    private System.Windows.Forms.Panel pnlPicture;
    private System.Windows.Forms.Panel pnlSound;
    private System.Windows.Forms.Panel pnlView;
    private System.Windows.Forms.ToolStrip tsViewPrev;
    private System.Windows.Forms.ToolStripButton tbbZoomIn;
    private System.Windows.Forms.ToolStripButton tbbZoomOut;
    private System.Windows.Forms.ToolStripSeparator tsSep1;
    private System.Windows.Forms.ToolStripSplitButton tbbHAlign;
    private System.Windows.Forms.ToolStripMenuItem tbbAlignLeft;
    private System.Windows.Forms.ToolStripMenuItem tbbAlignCenter;
    private System.Windows.Forms.ToolStripMenuItem tbbAlignRight;
    private System.Windows.Forms.ToolStripSplitButton tbbVAlign;
    private System.Windows.Forms.ToolStripMenuItem tbbTop;
    private System.Windows.Forms.ToolStripMenuItem tbbMiddle;
    private System.Windows.Forms.ToolStripMenuItem tbbBottom;
    private System.Windows.Forms.ToolStripButton tbbTrans;
    private System.Windows.Forms.HScrollBar hScrollBar1;
    private System.Windows.Forms.VScrollBar vScrollBar1;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.TrackBar trbSpeed;
    private System.Windows.Forms.ComboBox cmbMotion;
    private System.Windows.Forms.Button btnVPlay;
    private System.Windows.Forms.DomainUpDown udCel;
    private System.Windows.Forms.DomainUpDown udLoop;
    private System.Windows.Forms.Timer tmrMotion;
    private System.Windows.Forms.Timer Timer1;
    private System.Windows.Forms.PictureBox imgPicture;
    private System.Windows.Forms.RadioButton optPriority;
    private System.Windows.Forms.RadioButton optVisual;
    private System.Windows.Forms.NumericUpDown udPZoom;
    private System.Windows.Forms.Label label1;
  }
}