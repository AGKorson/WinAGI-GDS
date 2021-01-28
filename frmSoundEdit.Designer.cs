
namespace WinAGI.Editor
{
  partial class frmSoundEdit
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
      this.button1 = new System.Windows.Forms.Button();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(68, 114);
      this.button1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(117, 43);
      this.button1.TabIndex = 0;
      this.button1.Text = "play";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.ItemHeight = 15;
      this.listBox1.Location = new System.Drawing.Point(60, 33);
      this.listBox1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(140, 49);
      this.listBox1.TabIndex = 1;
      this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
      this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
      // 
      // frmSoundEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(431, 211);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.button1);
      this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
      this.Name = "frmSoundEdit";
      this.Text = "Sound Editor";
      this.Load += new System.EventHandler(this.frmSoundEdit_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ListBox listBox1;
  }
}