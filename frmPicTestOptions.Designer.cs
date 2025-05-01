using System;

namespace WinAGI.Editor {
    partial class frmPicTestOptions {
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
            groupBox1 = new System.Windows.Forms.GroupBox();
            chkIgnoreBlocks = new System.Windows.Forms.CheckBox();
            chkIgnoreHorizon = new System.Windows.Forms.CheckBox();
            txtHorizon = new NumericTextBox();
            label1 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            optWater = new System.Windows.Forms.RadioButton();
            optLand = new System.Windows.Forms.RadioButton();
            optAnything = new System.Windows.Forms.RadioButton();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            cmbCel = new System.Windows.Forms.ComboBox();
            cmbLoop = new System.Windows.Forms.ComboBox();
            cmbPriority = new System.Windows.Forms.ComboBox();
            cmbSpeed = new System.Windows.Forms.ComboBox();
            chkCycleAtRest = new System.Windows.Forms.CheckBox();
            cmdOK = new System.Windows.Forms.Button();
            cmdCancel = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkIgnoreBlocks);
            groupBox1.Controls.Add(chkIgnoreHorizon);
            groupBox1.Controls.Add(txtHorizon);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(139, 118);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Horizon";
            // 
            // chkIgnoreBlocks
            // 
            chkIgnoreBlocks.AutoSize = true;
            chkIgnoreBlocks.Location = new System.Drawing.Point(15, 87);
            chkIgnoreBlocks.Name = "chkIgnoreBlocks";
            chkIgnoreBlocks.Size = new System.Drawing.Size(97, 19);
            chkIgnoreBlocks.TabIndex = 3;
            chkIgnoreBlocks.Text = "Ignore Blocks";
            chkIgnoreBlocks.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreHorizon
            // 
            chkIgnoreHorizon.AutoSize = true;
            chkIgnoreHorizon.Location = new System.Drawing.Point(15, 62);
            chkIgnoreHorizon.Name = "chkIgnoreHorizon";
            chkIgnoreHorizon.Size = new System.Drawing.Size(105, 19);
            chkIgnoreHorizon.TabIndex = 2;
            chkIgnoreHorizon.Text = "Ignore Horizon";
            chkIgnoreHorizon.UseVisualStyleBackColor = true;
            // 
            // txtHorizon
            // 
            txtHorizon.Location = new System.Drawing.Point(65, 26);
            txtHorizon.MaxValue = 166;
            txtHorizon.MinValue = 1;
            txtHorizon.Name = "txtHorizon";
            txtHorizon.Size = new System.Drawing.Size(57, 23);
            txtHorizon.TabIndex = 1;
            txtHorizon.Text = "36";
            txtHorizon.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            txtHorizon.Value = 36;
            txtHorizon.Validating += txtHorizon_Validating;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(15, 28);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(35, 15);
            label1.TabIndex = 0;
            label1.Text = "Value";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(optWater);
            groupBox2.Controls.Add(optLand);
            groupBox2.Controls.Add(optAnything);
            groupBox2.Location = new System.Drawing.Point(12, 158);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(139, 107);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Object Restrictions";
            // 
            // optWater
            // 
            optWater.AutoSize = true;
            optWater.Location = new System.Drawing.Point(10, 74);
            optWater.Name = "optWater";
            optWater.Size = new System.Drawing.Size(111, 19);
            optWater.TabIndex = 2;
            optWater.TabStop = true;
            optWater.Text = "Object on Water";
            optWater.UseVisualStyleBackColor = true;
            // 
            // optLand
            // 
            optLand.AutoSize = true;
            optLand.Location = new System.Drawing.Point(10, 49);
            optLand.Name = "optLand";
            optLand.Size = new System.Drawing.Size(106, 19);
            optLand.TabIndex = 1;
            optLand.TabStop = true;
            optLand.Text = "Object on Land";
            optLand.UseVisualStyleBackColor = true;
            // 
            // optAnything
            // 
            optAnything.AutoSize = true;
            optAnything.Location = new System.Drawing.Point(10, 24);
            optAnything.Name = "optAnything";
            optAnything.Size = new System.Drawing.Size(129, 19);
            optAnything.TabIndex = 0;
            optAnything.TabStop = true;
            optAnything.Text = "Object on Anything";
            optAnything.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(cmbCel);
            groupBox3.Controls.Add(cmbLoop);
            groupBox3.Controls.Add(cmbPriority);
            groupBox3.Controls.Add(cmbSpeed);
            groupBox3.Controls.Add(chkCycleAtRest);
            groupBox3.Location = new System.Drawing.Point(163, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(132, 253);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Object Control";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(15, 178);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(27, 15);
            label5.TabIndex = 6;
            label5.Text = "Cel:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(14, 125);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(37, 15);
            label4.TabIndex = 4;
            label4.Text = "Loop:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(14, 72);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(86, 15);
            label3.TabIndex = 2;
            label3.Text = "Object Priority:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(14, 19);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(42, 15);
            label2.TabIndex = 0;
            label2.Text = "Speed:";
            // 
            // cmbCel
            // 
            cmbCel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbCel.FormattingEnabled = true;
            cmbCel.Location = new System.Drawing.Point(14, 196);
            cmbCel.Name = "cmbCel";
            cmbCel.Size = new System.Drawing.Size(104, 23);
            cmbCel.TabIndex = 7;
            cmbCel.SelectedIndexChanged += cmbCel_SelectedIndexChanged;
            // 
            // cmbLoop
            // 
            cmbLoop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLoop.FormattingEnabled = true;
            cmbLoop.Location = new System.Drawing.Point(14, 143);
            cmbLoop.Name = "cmbLoop";
            cmbLoop.Size = new System.Drawing.Size(104, 23);
            cmbLoop.TabIndex = 5;
            cmbLoop.SelectedIndexChanged += cmbLoop_SelectedIndexChanged;
            // 
            // cmbPriority
            // 
            cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbPriority.FormattingEnabled = true;
            cmbPriority.Items.AddRange(new object[] { "Automatic", "15", "14", "13", "12", "11", "10", "9", "8", "7", "6", "5", "4" });
            cmbPriority.Location = new System.Drawing.Point(14, 87);
            cmbPriority.Name = "cmbPriority";
            cmbPriority.Size = new System.Drawing.Size(104, 23);
            cmbPriority.TabIndex = 3;
            // 
            // cmbSpeed
            // 
            cmbSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSpeed.FormattingEnabled = true;
            cmbSpeed.Items.AddRange(new object[] { "Slow", "Normal", "Fast", "Fastest" });
            cmbSpeed.Location = new System.Drawing.Point(14, 37);
            cmbSpeed.Name = "cmbSpeed";
            cmbSpeed.Size = new System.Drawing.Size(104, 23);
            cmbSpeed.TabIndex = 1;
            // 
            // chkCycleAtRest
            // 
            chkCycleAtRest.AutoSize = true;
            chkCycleAtRest.Location = new System.Drawing.Point(15, 228);
            chkCycleAtRest.Name = "chkCycleAtRest";
            chkCycleAtRest.Size = new System.Drawing.Size(93, 19);
            chkCycleAtRest.TabIndex = 8;
            chkCycleAtRest.Text = "Cycle at Rest";
            chkCycleAtRest.UseVisualStyleBackColor = true;
            // 
            // cmdOK
            // 
            cmdOK.Location = new System.Drawing.Point(51, 273);
            cmdOK.Name = "cmdOK";
            cmdOK.Size = new System.Drawing.Size(79, 29);
            cmdOK.TabIndex = 3;
            cmdOK.Text = "OK";
            cmdOK.UseVisualStyleBackColor = true;
            cmdOK.Click += cmdOK_Click;
            // 
            // cmdCancel
            // 
            cmdCancel.Location = new System.Drawing.Point(176, 273);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new System.Drawing.Size(79, 29);
            cmdCancel.TabIndex = 4;
            cmdCancel.Text = "Cancel";
            cmdCancel.UseVisualStyleBackColor = true;
            cmdCancel.Click += cmdCancel_Click;
            // 
            // frmPicTestOptions
            // 
            AcceptButton = cmdOK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cmdCancel;
            ClientSize = new System.Drawing.Size(307, 311);
            ControlBox = false;
            Controls.Add(cmdCancel);
            Controls.Add(cmdOK);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPicTestOptions";
            ShowInTaskbar = false;
            Text = "Picture Editor View Test Options";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.CheckBox chkIgnoreBlocks;
        private System.Windows.Forms.CheckBox chkIgnoreHorizon;
        private WinAGI.Editor.NumericTextBox txtHorizon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton optWater;
        private System.Windows.Forms.RadioButton optLand;
        private System.Windows.Forms.RadioButton optAnything;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbCel;
        private System.Windows.Forms.ComboBox cmbLoop;
        private System.Windows.Forms.ComboBox cmbPriority;
        private System.Windows.Forms.ComboBox cmbSpeed;
        private System.Windows.Forms.CheckBox chkCycleAtRest;
    }
}