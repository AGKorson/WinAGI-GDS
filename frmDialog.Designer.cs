namespace WinAGI.Editor {
    partial class frmDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
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
        private void InitializeComponent() {
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            cmdHelp = new System.Windows.Forms.Button();
            Check1 = new System.Windows.Forms.CheckBox();
            Image1 = new System.Windows.Forms.PictureBox();
            Message = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)Image1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(39, 60);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(88, 31);
            button1.TabIndex = 1;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(151, 60);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 31);
            button2.TabIndex = 2;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            button2.Visible = false;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(245, 60);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(88, 31);
            button3.TabIndex = 3;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            button3.Visible = false;
            button3.Click += button3_Click;
            // 
            // cmdHelp
            // 
            cmdHelp.Location = new System.Drawing.Point(39, 12);
            cmdHelp.Name = "cmdHelp";
            cmdHelp.Size = new System.Drawing.Size(88, 31);
            cmdHelp.TabIndex = 4;
            cmdHelp.Text = "Help";
            cmdHelp.UseVisualStyleBackColor = true;
            cmdHelp.Visible = false;
            cmdHelp.Click += cmdHelp_Click;
            // 
            // Check1
            // 
            Check1.AutoSize = true;
            Check1.Location = new System.Drawing.Point(76, 114);
            Check1.Name = "Check1";
            Check1.Size = new System.Drawing.Size(178, 19);
            Check1.TabIndex = 5;
            Check1.Text = "Don't ask this question again";
            Check1.UseVisualStyleBackColor = true;
            // 
            // Image1
            // 
            Image1.Location = new System.Drawing.Point(11, 11);
            Image1.Name = "Image1";
            Image1.Size = new System.Drawing.Size(32, 32);
            Image1.TabIndex = 5;
            Image1.TabStop = false;
            Image1.Visible = false;
            // 
            // Message
            // 
            Message.AutoSize = true;
            Message.Location = new System.Drawing.Point(15, 11);
            Message.MaximumSize = new System.Drawing.Size(500, 0);
            Message.Name = "Message";
            Message.Size = new System.Drawing.Size(38, 15);
            Message.TabIndex = 0;
            Message.Text = "label1";
            // 
            // frmDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(436, 90);
            ControlBox = false;
            Controls.Add(Message);
            Controls.Add(Image1);
            Controls.Add(Check1);
            Controls.Add(cmdHelp);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmDialog";
            Text = "frmDialog";
            KeyDown += frmDialog_KeyDown;
            ((System.ComponentModel.ISupportInitialize)Image1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button cmdHelp;
        internal System.Windows.Forms.CheckBox Check1;
        private System.Windows.Forms.PictureBox Image1;
        internal System.Windows.Forms.Label Message;
    }
}