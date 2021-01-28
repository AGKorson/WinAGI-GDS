
namespace WinAGI.Editor
{
  partial class frmViewEdit
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
      this.picCel = new System.Windows.Forms.PictureBox();
      this.chkTrans = new System.Windows.Forms.CheckBox();
      this.cmbLoop = new System.Windows.Forms.ComboBox();
      this.cmbCel = new System.Windows.Forms.ComboBox();
      this.cmbView = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.button1 = new System.Windows.Forms.Button();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.picCel)).BeginInit();
      this.SuspendLayout();
      // 
      // picCel
      // 
      this.picCel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
      this.picCel.Location = new System.Drawing.Point(41, 32);
      this.picCel.Name = "picCel";
      this.picCel.Size = new System.Drawing.Size(640, 336);
      this.picCel.TabIndex = 0;
      this.picCel.TabStop = false;
      // 
      // chkTrans
      // 
      this.chkTrans.AutoSize = true;
      this.chkTrans.Location = new System.Drawing.Point(498, 389);
      this.chkTrans.Name = "chkTrans";
      this.chkTrans.Size = new System.Drawing.Size(95, 19);
      this.chkTrans.TabIndex = 1;
      this.chkTrans.Text = "Transparency";
      this.chkTrans.UseVisualStyleBackColor = true;
      this.chkTrans.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
      // 
      // cmbLoop
      // 
      this.cmbLoop.FormattingEnabled = true;
      this.cmbLoop.Location = new System.Drawing.Point(171, 394);
      this.cmbLoop.Name = "cmbLoop";
      this.cmbLoop.Size = new System.Drawing.Size(107, 23);
      this.cmbLoop.TabIndex = 2;
      this.cmbLoop.SelectedIndexChanged += new System.EventHandler(this.cmbLoop_SelectedIndexChanged);
      // 
      // cmbCel
      // 
      this.cmbCel.FormattingEnabled = true;
      this.cmbCel.Location = new System.Drawing.Point(309, 394);
      this.cmbCel.Name = "cmbCel";
      this.cmbCel.Size = new System.Drawing.Size(107, 23);
      this.cmbCel.TabIndex = 3;
      this.cmbCel.SelectedIndexChanged += new System.EventHandler(this.cmbCel_SelectedIndexChanged);
      // 
      // cmbView
      // 
      this.cmbView.FormattingEnabled = true;
      this.cmbView.Location = new System.Drawing.Point(41, 394);
      this.cmbView.Name = "cmbView";
      this.cmbView.Size = new System.Drawing.Size(107, 23);
      this.cmbView.TabIndex = 4;
      this.cmbView.SelectionChangeCommitted += new System.EventHandler(this.cmbView_SelectionChangeCommitted);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(38, 376);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(32, 15);
      this.label1.TabIndex = 5;
      this.label1.Text = "View";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(171, 376);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(34, 15);
      this.label2.TabIndex = 6;
      this.label2.Text = "Loop";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(309, 376);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(24, 15);
      this.label3.TabIndex = 7;
      this.label3.Text = "Cel";
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(470, 416);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(69, 26);
      this.button1.TabIndex = 8;
      this.button1.Text = "Start";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // timer1
      // 
      this.timer1.Interval = 75;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // frmViewEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cmbView);
      this.Controls.Add(this.cmbCel);
      this.Controls.Add(this.cmbLoop);
      this.Controls.Add(this.chkTrans);
      this.Controls.Add(this.picCel);
      this.Name = "frmViewEdit";
      this.Text = "frmViewEdit";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmViewEdit_FormClosing);
      this.Load += new System.EventHandler(this.frmViewEdit_Load);
      ((System.ComponentModel.ISupportInitialize)(this.picCel)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox picCel;
    private System.Windows.Forms.CheckBox chkTrans;
    private System.Windows.Forms.ComboBox cmbLoop;
    private System.Windows.Forms.ComboBox cmbCel;
    private System.Windows.Forms.ComboBox cmbView;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Timer timer1;
  }
}