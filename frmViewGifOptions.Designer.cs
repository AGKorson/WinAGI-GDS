
namespace WinAGI_GDS
{
  partial class frmViewGifOptions
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
      this.components = new System.ComponentModel.Container();
      this.picGrid = new System.Windows.Forms.Panel();
      this.picCel = new System.Windows.Forms.PictureBox();
      this.cmdOK = new System.Windows.Forms.Button();
      this.cmdCancel = new System.Windows.Forms.Button();
      this.HScroll1 = new System.Windows.Forms.HScrollBar();
      this.VScroll1 = new System.Windows.Forms.VScrollBar();
      this.chkTrans = new System.Windows.Forms.CheckBox();
      this.chkLoop = new System.Windows.Forms.CheckBox();
      this.udScale = new System.Windows.Forms.NumericUpDown();
      this.lblScale = new System.Windows.Forms.Label();
      this.udDelay = new System.Windows.Forms.NumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.lblAlign = new System.Windows.Forms.Label();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.picGrid.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picCel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udScale)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // picGrid
      // 
      this.picGrid.Controls.Add(this.picCel);
      this.picGrid.Location = new System.Drawing.Point(22, 33);
      this.picGrid.Name = "picGrid";
      this.picGrid.Size = new System.Drawing.Size(320, 265);
      this.picGrid.TabIndex = 0;
      // 
      // picCel
      // 
      this.picCel.Location = new System.Drawing.Point(47, 42);
      this.picCel.Name = "picCel";
      this.picCel.Size = new System.Drawing.Size(90, 123);
      this.picCel.TabIndex = 0;
      this.picCel.TabStop = false;
      // 
      // cmdOK
      // 
      this.cmdOK.Location = new System.Drawing.Point(384, 24);
      this.cmdOK.Name = "cmdOK";
      this.cmdOK.Size = new System.Drawing.Size(64, 24);
      this.cmdOK.TabIndex = 1;
      this.cmdOK.Text = "OK";
      this.cmdOK.UseVisualStyleBackColor = true;
      // 
      // cmdCancel
      // 
      this.cmdCancel.Location = new System.Drawing.Point(387, 54);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(61, 23);
      this.cmdCancel.TabIndex = 2;
      this.cmdCancel.Text = "Cancel";
      this.cmdCancel.UseVisualStyleBackColor = true;
      // 
      // HScroll1
      // 
      this.HScroll1.Location = new System.Drawing.Point(22, 301);
      this.HScroll1.Name = "HScroll1";
      this.HScroll1.Size = new System.Drawing.Size(320, 16);
      this.HScroll1.TabIndex = 1;
      // 
      // VScroll1
      // 
      this.VScroll1.Location = new System.Drawing.Point(346, 24);
      this.VScroll1.Name = "VScroll1";
      this.VScroll1.Size = new System.Drawing.Size(21, 277);
      this.VScroll1.TabIndex = 3;
      // 
      // chkTrans
      // 
      this.chkTrans.AutoSize = true;
      this.chkTrans.Location = new System.Drawing.Point(371, 110);
      this.chkTrans.Name = "chkTrans";
      this.chkTrans.Size = new System.Drawing.Size(95, 19);
      this.chkTrans.TabIndex = 4;
      this.chkTrans.Text = "Transparency";
      this.chkTrans.UseVisualStyleBackColor = true;
      // 
      // chkLoop
      // 
      this.chkLoop.AutoSize = true;
      this.chkLoop.Location = new System.Drawing.Point(370, 142);
      this.chkLoop.Name = "chkLoop";
      this.chkLoop.Size = new System.Drawing.Size(118, 19);
      this.chkLoop.TabIndex = 5;
      this.chkLoop.Text = "Continuous Loop";
      this.chkLoop.UseVisualStyleBackColor = true;
      // 
      // udScale
      // 
      this.udScale.Location = new System.Drawing.Point(425, 167);
      this.udScale.Name = "udScale";
      this.udScale.Size = new System.Drawing.Size(52, 23);
      this.udScale.TabIndex = 1;
      // 
      // lblScale
      // 
      this.lblScale.AutoSize = true;
      this.lblScale.Location = new System.Drawing.Point(371, 169);
      this.lblScale.Name = "lblScale";
      this.lblScale.Size = new System.Drawing.Size(37, 15);
      this.lblScale.TabIndex = 6;
      this.lblScale.Text = "Scale:";
      // 
      // udDelay
      // 
      this.udDelay.Location = new System.Drawing.Point(425, 215);
      this.udDelay.Name = "udDelay";
      this.udDelay.Size = new System.Drawing.Size(51, 23);
      this.udDelay.TabIndex = 7;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(379, 220);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(39, 15);
      this.label1.TabIndex = 8;
      this.label1.Text = "Delay:";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(482, 215);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(66, 36);
      this.label2.TabIndex = 9;
      this.label2.Text = "0.01 sec increments";
      // 
      // toolStrip1
      // 
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(800, 25);
      this.toolStrip1.TabIndex = 10;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // lblAlign
      // 
      this.lblAlign.AutoSize = true;
      this.lblAlign.Location = new System.Drawing.Point(220, 9);
      this.lblAlign.Name = "lblAlign";
      this.lblAlign.Size = new System.Drawing.Size(115, 15);
      this.lblAlign.TabIndex = 11;
      this.lblAlign.Text = "Align: Bottom, Right";
      // 
      // frmViewGifOptions
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.lblAlign);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.udDelay);
      this.Controls.Add(this.lblScale);
      this.Controls.Add(this.udScale);
      this.Controls.Add(this.chkLoop);
      this.Controls.Add(this.chkTrans);
      this.Controls.Add(this.VScroll1);
      this.Controls.Add(this.HScroll1);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOK);
      this.Controls.Add(this.picGrid);
      this.Name = "frmViewGifOptions";
      this.Text = "frmViewGifOptions";
      this.picGrid.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.picCel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udScale)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udDelay)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel picGrid;
    private System.Windows.Forms.PictureBox picCel;
    private System.Windows.Forms.Button cmdOK;
    private System.Windows.Forms.Button cmdCancel;
    private System.Windows.Forms.HScrollBar HScroll1;
    private System.Windows.Forms.VScrollBar VScroll1;
    private System.Windows.Forms.CheckBox chkTrans;
    private System.Windows.Forms.CheckBox chkLoop;
    private System.Windows.Forms.NumericUpDown udScale;
    private System.Windows.Forms.Label lblScale;
    private System.Windows.Forms.NumericUpDown udDelay;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.Label lblAlign;
    private System.Windows.Forms.Timer timer1;
  }
}