
using FastColoredTextBoxNS;

namespace WinAGI.Editor {
    partial class frmPreview {
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPreview));
            pnlLogic = new System.Windows.Forms.Panel();
            rtfLogPrev = new FastColoredTextBox();
            cmsLogic = new System.Windows.Forms.ContextMenuStrip(components);
            cmiSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            cmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            pnlPicture = new System.Windows.Forms.Panel();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            pnlPicHeader = new System.Windows.Forms.Panel();
            optPriority = new System.Windows.Forms.RadioButton();
            optVisual = new System.Windows.Forms.RadioButton();
            udPZoom = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            pnlPicImage = new System.Windows.Forms.Panel();
            fraPCorner = new System.Windows.Forms.PictureBox();
            vsbPic = new System.Windows.Forms.VScrollBar();
            hsbPic = new System.Windows.Forms.HScrollBar();
            imgPicture = new System.Windows.Forms.PictureBox();
            pnlSound = new System.Windows.Forms.Panel();
            optMIDI = new System.Windows.Forms.RadioButton();
            optPCjr = new System.Windows.Forms.RadioButton();
            pnlProgressBar = new System.Windows.Forms.Panel();
            picProgress = new System.Windows.Forms.PictureBox();
            lblFormat = new System.Windows.Forms.Label();
            btnStop = new System.Windows.Forms.Button();
            imageList1 = new System.Windows.Forms.ImageList(components);
            btnPlay = new System.Windows.Forms.Button();
            lblLength = new System.Windows.Forms.Label();
            cmdReset = new System.Windows.Forms.Button();
            cmbInst2 = new System.Windows.Forms.ComboBox();
            cmbInst1 = new System.Windows.Forms.ComboBox();
            cmbInst0 = new System.Windows.Forms.ComboBox();
            chkTrack3 = new System.Windows.Forms.CheckBox();
            chkTrack2 = new System.Windows.Forms.CheckBox();
            chkTrack1 = new System.Windows.Forms.CheckBox();
            chkTrack0 = new System.Windows.Forms.CheckBox();
            pnlView = new System.Windows.Forms.Panel();
            picTrans = new System.Windows.Forms.PictureBox();
            chkTrans = new System.Windows.Forms.CheckBox();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            pnlViewHdr = new System.Windows.Forms.Panel();
            dCel = new System.Windows.Forms.Button();
            uCel = new System.Windows.Forms.Button();
            uLoop = new System.Windows.Forms.Button();
            dLoop = new System.Windows.Forms.Button();
            udCel = new System.Windows.Forms.Label();
            udLoop = new System.Windows.Forms.Label();
            pnlViewFtr = new System.Windows.Forms.Panel();
            sldSpeed = new System.Windows.Forms.TrackBar();
            cmbMotion = new System.Windows.Forms.ComboBox();
            cmdVPlay = new System.Windows.Forms.Button();
            pnlCel = new System.Windows.Forms.Panel();
            fraVCorner = new System.Windows.Forms.PictureBox();
            vsbView = new System.Windows.Forms.VScrollBar();
            hsbView = new System.Windows.Forms.HScrollBar();
            picCel = new System.Windows.Forms.PictureBox();
            tsViewPrev = new System.Windows.Forms.ToolStrip();
            tbbZoomIn = new System.Windows.Forms.ToolStripButton();
            tbbZoomOut = new System.Windows.Forms.ToolStripButton();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            HAlign = new System.Windows.Forms.ToolStripSplitButton();
            tbbAlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            tbbAlignCenter = new System.Windows.Forms.ToolStripMenuItem();
            tbbAlignRight = new System.Windows.Forms.ToolStripMenuItem();
            VAlign = new System.Windows.Forms.ToolStripSplitButton();
            tbbTop = new System.Windows.Forms.ToolStripMenuItem();
            tbbMiddle = new System.Windows.Forms.ToolStripMenuItem();
            tbbBottom = new System.Windows.Forms.ToolStripMenuItem();
            tmrMotion = new System.Windows.Forms.Timer(components);
            tmrSound = new System.Windows.Forms.Timer(components);
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            tspPreviewText = new System.Windows.Forms.ToolStripStatusLabel();
            pnlLogic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rtfLogPrev).BeginInit();
            cmsLogic.SuspendLayout();
            pnlPicture.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            pnlPicHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)udPZoom).BeginInit();
            pnlPicImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)fraPCorner).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imgPicture).BeginInit();
            pnlSound.SuspendLayout();
            pnlProgressBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picProgress).BeginInit();
            pnlView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picTrans).BeginInit();
            tableLayoutPanel2.SuspendLayout();
            pnlViewHdr.SuspendLayout();
            pnlViewFtr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sldSpeed).BeginInit();
            pnlCel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)fraVCorner).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCel).BeginInit();
            tsViewPrev.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLogic
            // 
            pnlLogic.Controls.Add(rtfLogPrev);
            pnlLogic.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLogic.Location = new System.Drawing.Point(0, 0);
            pnlLogic.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pnlLogic.Name = "pnlLogic";
            pnlLogic.Size = new System.Drawing.Size(541, 392);
            pnlLogic.TabIndex = 0;
            pnlLogic.Visible = false;
            // 
            // rtfLogPrev
            // 
            rtfLogPrev.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rtfLogPrev.AutoCompleteBracketsList = new char[]
    {
    '(',
    ')',
    '{',
    '}',
    '"',
    '"'
    };
            rtfLogPrev.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
            rtfLogPrev.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            rtfLogPrev.BackBrush = null;
            rtfLogPrev.CharHeight = 14;
            rtfLogPrev.CharWidth = 8;
            rtfLogPrev.ContextMenuStrip = cmsLogic;
            rtfLogPrev.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            rtfLogPrev.Font = new System.Drawing.Font("Courier New", 9.75F);
            rtfLogPrev.Hotkeys = resources.GetString("rtfLogPrev.Hotkeys");
            rtfLogPrev.IsReplaceMode = false;
            rtfLogPrev.Location = new System.Drawing.Point(0, 0);
            rtfLogPrev.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            rtfLogPrev.Name = "rtfLogPrev";
            rtfLogPrev.Paddings = new System.Windows.Forms.Padding(0);
            rtfLogPrev.ReadOnly = true;
            rtfLogPrev.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            rtfLogPrev.ServiceColors = (ServiceColors)resources.GetObject("rtfLogPrev.ServiceColors");
            rtfLogPrev.Size = new System.Drawing.Size(537, 389);
            rtfLogPrev.TabIndex = 0;
            rtfLogPrev.Zoom = 100;
            rtfLogPrev.DoubleClick += rtfLogPrev_DoubleClick;
            rtfLogPrev.KeyDown += rtfLogPrev_KeyDown;
            rtfLogPrev.MouseDown += rtfLogPrev_MouseDown;
            // 
            // cmsLogic
            // 
            cmsLogic.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cmiSelectAll, cmiCopy });
            cmsLogic.Name = "contextMenuStrip1";
            cmsLogic.Size = new System.Drawing.Size(165, 48);
            // 
            // cmiSelectAll
            // 
            cmiSelectAll.Name = "cmiSelectAll";
            cmiSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            cmiSelectAll.Size = new System.Drawing.Size(164, 22);
            cmiSelectAll.Text = "Select All";
            cmiSelectAll.Click += cmiSelectAll_Click;
            // 
            // cmiCopy
            // 
            cmiCopy.Name = "cmiCopy";
            cmiCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            cmiCopy.Size = new System.Drawing.Size(164, 22);
            cmiCopy.Text = "Copy";
            cmiCopy.Click += cmiCopy_Click;
            // 
            // pnlPicture
            // 
            pnlPicture.Controls.Add(tableLayoutPanel1);
            pnlPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlPicture.Location = new System.Drawing.Point(0, 0);
            pnlPicture.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pnlPicture.Name = "pnlPicture";
            pnlPicture.Size = new System.Drawing.Size(541, 392);
            pnlPicture.TabIndex = 1;
            pnlPicture.Visible = false;
            pnlPicture.Leave += pnlPicture_Leave;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pnlPicHeader, 0, 0);
            tableLayoutPanel1.Controls.Add(pnlPicImage, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(541, 392);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // pnlPicHeader
            // 
            pnlPicHeader.Controls.Add(optPriority);
            pnlPicHeader.Controls.Add(optVisual);
            pnlPicHeader.Controls.Add(udPZoom);
            pnlPicHeader.Controls.Add(label1);
            pnlPicHeader.Location = new System.Drawing.Point(3, 3);
            pnlPicHeader.Name = "pnlPicHeader";
            pnlPicHeader.Size = new System.Drawing.Size(259, 31);
            pnlPicHeader.TabIndex = 0;
            // 
            // optPriority
            // 
            optPriority.AutoSize = true;
            optPriority.Location = new System.Drawing.Point(172, 7);
            optPriority.Name = "optPriority";
            optPriority.Size = new System.Drawing.Size(63, 19);
            optPriority.TabIndex = 7;
            optPriority.Text = "Priority";
            optPriority.UseVisualStyleBackColor = true;
            // 
            // optVisual
            // 
            optVisual.AutoSize = true;
            optVisual.Checked = true;
            optVisual.Location = new System.Drawing.Point(100, 7);
            optVisual.Name = "optVisual";
            optVisual.Size = new System.Drawing.Size(56, 19);
            optVisual.TabIndex = 6;
            optVisual.TabStop = true;
            optVisual.Text = "Visual";
            optVisual.UseVisualStyleBackColor = true;
            optVisual.CheckedChanged += optVisual_CheckedChanged;
            // 
            // udPZoom
            // 
            udPZoom.Location = new System.Drawing.Point(50, 7);
            udPZoom.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            udPZoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            udPZoom.Name = "udPZoom";
            udPZoom.ReadOnly = true;
            udPZoom.Size = new System.Drawing.Size(40, 23);
            udPZoom.TabIndex = 5;
            udPZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            udPZoom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            udPZoom.ValueChanged += udPZoom_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(2, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 15);
            label1.TabIndex = 4;
            label1.Text = "Zoom:";
            // 
            // pnlPicImage
            // 
            pnlPicImage.Controls.Add(fraPCorner);
            pnlPicImage.Controls.Add(vsbPic);
            pnlPicImage.Controls.Add(hsbPic);
            pnlPicImage.Controls.Add(imgPicture);
            pnlPicImage.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlPicImage.Location = new System.Drawing.Point(3, 40);
            pnlPicImage.Name = "pnlPicImage";
            pnlPicImage.Size = new System.Drawing.Size(535, 349);
            pnlPicImage.TabIndex = 1;
            pnlPicImage.Resize += pnlPicImage_Resize;
            // 
            // fraPCorner
            // 
            fraPCorner.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            fraPCorner.Location = new System.Drawing.Point(522, 339);
            fraPCorner.Name = "fraPCorner";
            fraPCorner.Size = new System.Drawing.Size(11, 9);
            fraPCorner.TabIndex = 8;
            fraPCorner.TabStop = false;
            fraPCorner.Visible = false;
            // 
            // vsbPic
            // 
            vsbPic.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            vsbPic.Location = new System.Drawing.Point(522, 0);
            vsbPic.Minimum = -4;
            vsbPic.Name = "vsbPic";
            vsbPic.Size = new System.Drawing.Size(20, 338);
            vsbPic.TabIndex = 7;
            vsbPic.Scroll += vsbPic_Scroll;
            // 
            // hsbPic
            // 
            hsbPic.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            hsbPic.Location = new System.Drawing.Point(0, 339);
            hsbPic.Minimum = -4;
            hsbPic.Name = "hsbPic";
            hsbPic.Size = new System.Drawing.Size(485, 20);
            hsbPic.TabIndex = 6;
            hsbPic.Scroll += hsbPic_Scroll;
            // 
            // imgPicture
            // 
            imgPicture.Location = new System.Drawing.Point(0, 0);
            imgPicture.Name = "imgPicture";
            imgPicture.Size = new System.Drawing.Size(320, 168);
            imgPicture.TabIndex = 5;
            imgPicture.TabStop = false;
            imgPicture.DoubleClick += imgPicture_DoubleClick;
            imgPicture.MouseDown += imgPicture_MouseDown;
            imgPicture.MouseLeave += imgPicture_MouseLeave;
            imgPicture.MouseMove += imgPicture_MouseMove;
            imgPicture.MouseUp += imgPicture_MouseUp;
            imgPicture.MouseWheel += imgPicture_MouseWheel;
            imgPicture.Validated += imgPicture_Validated;
            // 
            // pnlSound
            // 
            pnlSound.Controls.Add(optMIDI);
            pnlSound.Controls.Add(optPCjr);
            pnlSound.Controls.Add(pnlProgressBar);
            pnlSound.Controls.Add(lblFormat);
            pnlSound.Controls.Add(btnStop);
            pnlSound.Controls.Add(btnPlay);
            pnlSound.Controls.Add(lblLength);
            pnlSound.Controls.Add(cmdReset);
            pnlSound.Controls.Add(cmbInst2);
            pnlSound.Controls.Add(cmbInst1);
            pnlSound.Controls.Add(cmbInst0);
            pnlSound.Controls.Add(chkTrack3);
            pnlSound.Controls.Add(chkTrack2);
            pnlSound.Controls.Add(chkTrack1);
            pnlSound.Controls.Add(chkTrack0);
            pnlSound.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlSound.Location = new System.Drawing.Point(0, 0);
            pnlSound.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pnlSound.Name = "pnlSound";
            pnlSound.Size = new System.Drawing.Size(541, 392);
            pnlSound.TabIndex = 2;
            pnlSound.Visible = false;
            pnlSound.DoubleClick += pnlSound_DoubleClick;
            // 
            // optMIDI
            // 
            optMIDI.AutoSize = true;
            optMIDI.Location = new System.Drawing.Point(393, 85);
            optMIDI.Name = "optMIDI";
            optMIDI.Size = new System.Drawing.Size(87, 19);
            optMIDI.TabIndex = 15;
            optMIDI.TabStop = true;
            optMIDI.Text = "MIDI Sound";
            optMIDI.UseVisualStyleBackColor = true;
            optMIDI.CheckedChanged += optMIDI_CheckedChanged;
            // 
            // optPCjr
            // 
            optPCjr.AutoSize = true;
            optPCjr.Checked = true;
            optPCjr.Location = new System.Drawing.Point(393, 60);
            optPCjr.Name = "optPCjr";
            optPCjr.Size = new System.Drawing.Size(84, 19);
            optPCjr.TabIndex = 14;
            optPCjr.TabStop = true;
            optPCjr.Text = "PCjr Sound";
            optPCjr.UseVisualStyleBackColor = true;
            optPCjr.CheckedChanged += optPCjr_CheckedChanged;
            // 
            // pnlProgressBar
            // 
            pnlProgressBar.BackColor = System.Drawing.SystemColors.Info;
            pnlProgressBar.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pnlProgressBar.Controls.Add(picProgress);
            pnlProgressBar.Location = new System.Drawing.Point(23, 192);
            pnlProgressBar.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pnlProgressBar.Name = "pnlProgressBar";
            pnlProgressBar.Size = new System.Drawing.Size(346, 16);
            pnlProgressBar.TabIndex = 13;
            // 
            // picProgress
            // 
            picProgress.BackColor = System.Drawing.SystemColors.Highlight;
            picProgress.Location = new System.Drawing.Point(0, 0);
            picProgress.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            picProgress.Name = "picProgress";
            picProgress.Size = new System.Drawing.Size(0, 12);
            picProgress.TabIndex = 0;
            picProgress.TabStop = false;
            // 
            // lblFormat
            // 
            lblFormat.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            lblFormat.Location = new System.Drawing.Point(23, 7);
            lblFormat.Name = "lblFormat";
            lblFormat.Size = new System.Drawing.Size(343, 27);
            lblFormat.TabIndex = 12;
            lblFormat.Text = "label3";
            lblFormat.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.ImageIndex = 9;
            btnStop.ImageList = imageList1;
            btnStop.Location = new System.Drawing.Point(197, 211);
            btnStop.Name = "btnStop";
            btnStop.Size = new System.Drawing.Size(95, 28);
            btnStop.TabIndex = 11;
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "eli_zoomout.ico");
            imageList1.Images.SetKeyName(1, "eli_zoomin.ico");
            imageList1.Images.SetKeyName(2, "HAlignL.ico");
            imageList1.Images.SetKeyName(3, "HAlignC.ico");
            imageList1.Images.SetKeyName(4, "HAlignR.ico");
            imageList1.Images.SetKeyName(5, "ValignT.ico");
            imageList1.Images.SetKeyName(6, "VAlignM.ico");
            imageList1.Images.SetKeyName(7, "ValignB.ico");
            imageList1.Images.SetKeyName(8, "sndplay.ico");
            imageList1.Images.SetKeyName(9, "sndstop.ico");
            // 
            // btnPlay
            // 
            btnPlay.ImageIndex = 8;
            btnPlay.ImageList = imageList1;
            btnPlay.Location = new System.Drawing.Point(98, 211);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new System.Drawing.Size(95, 28);
            btnPlay.TabIndex = 10;
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // lblLength
            // 
            lblLength.Location = new System.Drawing.Point(23, 174);
            lblLength.Name = "lblLength";
            lblLength.Size = new System.Drawing.Size(344, 15);
            lblLength.TabIndex = 8;
            lblLength.Text = "0.0 seconds";
            lblLength.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cmdReset
            // 
            cmdReset.Location = new System.Drawing.Point(211, 128);
            cmdReset.Name = "cmdReset";
            cmdReset.Size = new System.Drawing.Size(155, 32);
            cmdReset.TabIndex = 7;
            cmdReset.Text = "Reset Instruments";
            cmdReset.UseVisualStyleBackColor = true;
            cmdReset.Click += cmdReset_Click;
            // 
            // cmbInst2
            // 
            cmbInst2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbInst2.FormattingEnabled = true;
            cmbInst2.Location = new System.Drawing.Point(153, 95);
            cmbInst2.Name = "cmbInst2";
            cmbInst2.Size = new System.Drawing.Size(214, 23);
            cmbInst2.TabIndex = 6;
            cmbInst2.SelectionChangeCommitted += cmbInst2_SelectionChangeCommitted;
            // 
            // cmbInst1
            // 
            cmbInst1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbInst1.FormattingEnabled = true;
            cmbInst1.Location = new System.Drawing.Point(153, 64);
            cmbInst1.Name = "cmbInst1";
            cmbInst1.Size = new System.Drawing.Size(214, 23);
            cmbInst1.TabIndex = 5;
            cmbInst1.SelectionChangeCommitted += cmbInst1_SelectionChangeCommitted;
            // 
            // cmbInst0
            // 
            cmbInst0.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbInst0.FormattingEnabled = true;
            cmbInst0.Location = new System.Drawing.Point(153, 35);
            cmbInst0.Name = "cmbInst0";
            cmbInst0.Size = new System.Drawing.Size(214, 23);
            cmbInst0.TabIndex = 4;
            cmbInst0.SelectionChangeCommitted += cmbInst0_SelectionChangeCommitted;
            // 
            // chkTrack3
            // 
            chkTrack3.AutoSize = true;
            chkTrack3.Location = new System.Drawing.Point(21, 136);
            chkTrack3.Name = "chkTrack3";
            chkTrack3.Size = new System.Drawing.Size(128, 19);
            chkTrack3.TabIndex = 3;
            chkTrack3.Text = "Track 3: Noise Track";
            chkTrack3.UseVisualStyleBackColor = true;
            chkTrack3.CheckedChanged += chkTrack3_CheckedChanged;
            // 
            // chkTrack2
            // 
            chkTrack2.AutoSize = true;
            chkTrack2.Location = new System.Drawing.Point(21, 97);
            chkTrack2.Name = "chkTrack2";
            chkTrack2.Size = new System.Drawing.Size(126, 19);
            chkTrack2.TabIndex = 2;
            chkTrack2.Text = "Track 2 Instrument:";
            chkTrack2.UseVisualStyleBackColor = true;
            chkTrack2.CheckedChanged += chkTrack2_CheckedChanged;
            // 
            // chkTrack1
            // 
            chkTrack1.AutoSize = true;
            chkTrack1.Location = new System.Drawing.Point(21, 66);
            chkTrack1.Name = "chkTrack1";
            chkTrack1.Size = new System.Drawing.Size(126, 19);
            chkTrack1.TabIndex = 1;
            chkTrack1.Text = "Track 1 Instrument:";
            chkTrack1.UseVisualStyleBackColor = true;
            chkTrack1.CheckedChanged += chkTrack1_CheckedChanged;
            // 
            // chkTrack0
            // 
            chkTrack0.AutoSize = true;
            chkTrack0.Location = new System.Drawing.Point(21, 37);
            chkTrack0.Name = "chkTrack0";
            chkTrack0.Size = new System.Drawing.Size(126, 19);
            chkTrack0.TabIndex = 0;
            chkTrack0.Text = "Track 0 Instrument:";
            chkTrack0.UseVisualStyleBackColor = true;
            chkTrack0.CheckedChanged += chkTrack0_CheckedChanged;
            // 
            // pnlView
            // 
            pnlView.Controls.Add(picTrans);
            pnlView.Controls.Add(chkTrans);
            pnlView.Controls.Add(tableLayoutPanel2);
            pnlView.Controls.Add(tsViewPrev);
            pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlView.Location = new System.Drawing.Point(0, 0);
            pnlView.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            pnlView.Name = "pnlView";
            pnlView.Size = new System.Drawing.Size(541, 392);
            pnlView.TabIndex = 3;
            pnlView.Visible = false;
            // 
            // picTrans
            // 
            picTrans.Location = new System.Drawing.Point(235, 4);
            picTrans.Margin = new System.Windows.Forms.Padding(2);
            picTrans.Name = "picTrans";
            picTrans.Size = new System.Drawing.Size(19, 17);
            picTrans.TabIndex = 10;
            picTrans.TabStop = false;
            // 
            // chkTrans
            // 
            chkTrans.AutoSize = true;
            chkTrans.Location = new System.Drawing.Point(137, 4);
            chkTrans.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            chkTrans.Name = "chkTrans";
            chkTrans.Size = new System.Drawing.Size(95, 19);
            chkTrans.TabIndex = 10;
            chkTrans.Text = "Transparency";
            chkTrans.UseVisualStyleBackColor = true;
            chkTrans.Click += chkTrans_Click;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(pnlViewHdr, 0, 0);
            tableLayoutPanel2.Controls.Add(pnlViewFtr, 0, 2);
            tableLayoutPanel2.Controls.Add(pnlCel, 0, 1);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(0, 25);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            tableLayoutPanel2.Size = new System.Drawing.Size(541, 367);
            tableLayoutPanel2.TabIndex = 13;
            // 
            // pnlViewHdr
            // 
            pnlViewHdr.Controls.Add(dCel);
            pnlViewHdr.Controls.Add(uCel);
            pnlViewHdr.Controls.Add(uLoop);
            pnlViewHdr.Controls.Add(dLoop);
            pnlViewHdr.Controls.Add(udCel);
            pnlViewHdr.Controls.Add(udLoop);
            pnlViewHdr.Location = new System.Drawing.Point(3, 3);
            pnlViewHdr.Name = "pnlViewHdr";
            pnlViewHdr.Size = new System.Drawing.Size(398, 19);
            pnlViewHdr.TabIndex = 9;
            // 
            // dCel
            // 
            dCel.BackgroundImage = (System.Drawing.Image)resources.GetObject("dCel.BackgroundImage");
            dCel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            dCel.Location = new System.Drawing.Point(256, 0);
            dCel.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            dCel.Name = "dCel";
            dCel.Size = new System.Drawing.Size(19, 17);
            dCel.TabIndex = 19;
            dCel.UseVisualStyleBackColor = true;
            dCel.Click += dCel_Click;
            // 
            // uCel
            // 
            uCel.BackgroundImage = (System.Drawing.Image)resources.GetObject("uCel.BackgroundImage");
            uCel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            uCel.Location = new System.Drawing.Point(275, 0);
            uCel.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            uCel.Name = "uCel";
            uCel.Size = new System.Drawing.Size(19, 17);
            uCel.TabIndex = 18;
            uCel.UseVisualStyleBackColor = true;
            uCel.Click += uCel_Click;
            // 
            // uLoop
            // 
            uLoop.BackgroundImage = (System.Drawing.Image)resources.GetObject("uLoop.BackgroundImage");
            uLoop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            uLoop.Location = new System.Drawing.Point(124, 0);
            uLoop.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            uLoop.Name = "uLoop";
            uLoop.Size = new System.Drawing.Size(19, 17);
            uLoop.TabIndex = 17;
            uLoop.UseVisualStyleBackColor = true;
            uLoop.Click += uLoop_Click;
            // 
            // dLoop
            // 
            dLoop.BackgroundImage = (System.Drawing.Image)resources.GetObject("dLoop.BackgroundImage");
            dLoop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            dLoop.Location = new System.Drawing.Point(105, 0);
            dLoop.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            dLoop.Name = "dLoop";
            dLoop.Size = new System.Drawing.Size(19, 17);
            dLoop.TabIndex = 16;
            dLoop.UseVisualStyleBackColor = true;
            dLoop.Click += dLoop_Click;
            // 
            // udCel
            // 
            udCel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            udCel.Location = new System.Drawing.Point(151, 0);
            udCel.Margin = new System.Windows.Forms.Padding(3);
            udCel.Name = "udCel";
            udCel.Size = new System.Drawing.Size(105, 18);
            udCel.TabIndex = 15;
            udCel.Text = "Cel 0 / 0";
            udCel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // udLoop
            // 
            udLoop.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            udLoop.Location = new System.Drawing.Point(0, 0);
            udLoop.Margin = new System.Windows.Forms.Padding(3);
            udLoop.Name = "udLoop";
            udLoop.Size = new System.Drawing.Size(105, 18);
            udLoop.TabIndex = 9;
            udLoop.Text = "Loop 255 / 255";
            udLoop.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlViewFtr
            // 
            pnlViewFtr.Controls.Add(sldSpeed);
            pnlViewFtr.Controls.Add(cmbMotion);
            pnlViewFtr.Controls.Add(cmdVPlay);
            pnlViewFtr.Location = new System.Drawing.Point(3, 331);
            pnlViewFtr.Name = "pnlViewFtr";
            pnlViewFtr.Size = new System.Drawing.Size(276, 33);
            pnlViewFtr.TabIndex = 10;
            // 
            // sldSpeed
            // 
            sldSpeed.Location = new System.Drawing.Point(181, 3);
            sldSpeed.Maximum = 12;
            sldSpeed.Minimum = 1;
            sldSpeed.Name = "sldSpeed";
            sldSpeed.Size = new System.Drawing.Size(100, 45);
            sldSpeed.TabIndex = 13;
            sldSpeed.Value = 1;
            sldSpeed.ValueChanged += sldSpeed_ValueChanged;
            // 
            // cmbMotion
            // 
            cmbMotion.FormattingEnabled = true;
            cmbMotion.Items.AddRange(new object[] { "normal", "reverse", "end of loop", "reverse loop" });
            cmbMotion.Location = new System.Drawing.Point(61, 7);
            cmbMotion.Name = "cmbMotion";
            cmbMotion.Size = new System.Drawing.Size(93, 23);
            cmbMotion.TabIndex = 12;
            // 
            // cmdVPlay
            // 
            cmdVPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            cmdVPlay.Location = new System.Drawing.Point(5, 5);
            cmdVPlay.Name = "cmdVPlay";
            cmdVPlay.Size = new System.Drawing.Size(50, 23);
            cmdVPlay.TabIndex = 11;
            cmdVPlay.UseVisualStyleBackColor = true;
            cmdVPlay.Click += cmdVPlay_Click;
            // 
            // pnlCel
            // 
            pnlCel.Controls.Add(fraVCorner);
            pnlCel.Controls.Add(vsbView);
            pnlCel.Controls.Add(hsbView);
            pnlCel.Controls.Add(picCel);
            pnlCel.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCel.Location = new System.Drawing.Point(3, 28);
            pnlCel.Name = "pnlCel";
            pnlCel.Size = new System.Drawing.Size(535, 297);
            pnlCel.TabIndex = 11;
            pnlCel.Paint += pnlCel_Paint;
            pnlCel.DoubleClick += picCel_DoubleClick;
            pnlCel.Resize += pnlCel_Resize;
            // 
            // fraVCorner
            // 
            fraVCorner.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            fraVCorner.Location = new System.Drawing.Point(526, 289);
            fraVCorner.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            fraVCorner.Name = "fraVCorner";
            fraVCorner.Size = new System.Drawing.Size(20, 20);
            fraVCorner.TabIndex = 9;
            fraVCorner.TabStop = false;
            fraVCorner.Visible = false;
            // 
            // vsbView
            // 
            vsbView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            vsbView.Location = new System.Drawing.Point(522, 0);
            vsbView.Minimum = -4;
            vsbView.Name = "vsbView";
            vsbView.Size = new System.Drawing.Size(16, 185);
            vsbView.TabIndex = 5;
            vsbView.Visible = false;
            vsbView.Scroll += vsbView_Scroll;
            // 
            // hsbView
            // 
            hsbView.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            hsbView.Location = new System.Drawing.Point(0, 289);
            hsbView.Minimum = -4;
            hsbView.Name = "hsbView";
            hsbView.Size = new System.Drawing.Size(157, 16);
            hsbView.TabIndex = 6;
            hsbView.Visible = false;
            hsbView.Scroll += hsbView_Scroll;
            // 
            // picCel
            // 
            picCel.Location = new System.Drawing.Point(0, 0);
            picCel.Name = "picCel";
            picCel.Size = new System.Drawing.Size(72, 71);
            picCel.TabIndex = 8;
            picCel.TabStop = false;
            picCel.DoubleClick += picCel_DoubleClick;
            picCel.MouseDown += picCel_MouseDown;
            picCel.MouseMove += picCel_MouseMove;
            picCel.MouseUp += picCel_MouseUp;
            // 
            // tsViewPrev
            // 
            tsViewPrev.ImageScalingSize = new System.Drawing.Size(32, 32);
            tsViewPrev.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tbbZoomIn, tbbZoomOut, tsSep1, HAlign, VAlign });
            tsViewPrev.Location = new System.Drawing.Point(0, 0);
            tsViewPrev.Name = "tsViewPrev";
            tsViewPrev.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            tsViewPrev.Size = new System.Drawing.Size(541, 25);
            tsViewPrev.TabIndex = 0;
            tsViewPrev.Text = "toolStrip1";
            // 
            // tbbZoomIn
            // 
            tbbZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbZoomIn.Image = (System.Drawing.Image)resources.GetObject("tbbZoomIn.Image");
            tbbZoomIn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbZoomIn.Name = "tbbZoomIn";
            tbbZoomIn.Size = new System.Drawing.Size(23, 22);
            tbbZoomIn.Text = "toolStripButton1";
            tbbZoomIn.Click += tbbZoomIn_Click;
            // 
            // tbbZoomOut
            // 
            tbbZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tbbZoomOut.Image = (System.Drawing.Image)resources.GetObject("tbbZoomOut.Image");
            tbbZoomOut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            tbbZoomOut.Name = "tbbZoomOut";
            tbbZoomOut.Size = new System.Drawing.Size(23, 22);
            tbbZoomOut.Text = "toolStripButton2";
            tbbZoomOut.Click += tbbZoomOut_Click;
            // 
            // tsSep1
            // 
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new System.Drawing.Size(6, 25);
            // 
            // HAlign
            // 
            HAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            HAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tbbAlignLeft, tbbAlignCenter, tbbAlignRight });
            HAlign.Image = (System.Drawing.Image)resources.GetObject("HAlign.Image");
            HAlign.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            HAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            HAlign.Name = "HAlign";
            HAlign.Size = new System.Drawing.Size(32, 22);
            // 
            // tbbAlignLeft
            // 
            tbbAlignLeft.Image = (System.Drawing.Image)resources.GetObject("tbbAlignLeft.Image");
            tbbAlignLeft.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbAlignLeft.Name = "tbbAlignLeft";
            tbbAlignLeft.Size = new System.Drawing.Size(109, 22);
            tbbAlignLeft.Text = "Left";
            tbbAlignLeft.Click += tbbAlignLeft_Click;
            // 
            // tbbAlignCenter
            // 
            tbbAlignCenter.Image = (System.Drawing.Image)resources.GetObject("tbbAlignCenter.Image");
            tbbAlignCenter.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbAlignCenter.Name = "tbbAlignCenter";
            tbbAlignCenter.Size = new System.Drawing.Size(109, 22);
            tbbAlignCenter.Text = "Center";
            tbbAlignCenter.Click += tbbAlignCenter_Click;
            // 
            // tbbAlignRight
            // 
            tbbAlignRight.Image = (System.Drawing.Image)resources.GetObject("tbbAlignRight.Image");
            tbbAlignRight.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbAlignRight.Name = "tbbAlignRight";
            tbbAlignRight.Size = new System.Drawing.Size(109, 22);
            tbbAlignRight.Text = "Right";
            tbbAlignRight.Click += tbbAlignRight_Click;
            // 
            // VAlign
            // 
            VAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            VAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tbbTop, tbbMiddle, tbbBottom });
            VAlign.Image = (System.Drawing.Image)resources.GetObject("VAlign.Image");
            VAlign.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            VAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            VAlign.Name = "VAlign";
            VAlign.Size = new System.Drawing.Size(32, 22);
            // 
            // tbbTop
            // 
            tbbTop.Image = (System.Drawing.Image)resources.GetObject("tbbTop.Image");
            tbbTop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbTop.Name = "tbbTop";
            tbbTop.Size = new System.Drawing.Size(114, 22);
            tbbTop.Text = "Top";
            tbbTop.Click += tbbTop_Click;
            // 
            // tbbMiddle
            // 
            tbbMiddle.Image = (System.Drawing.Image)resources.GetObject("tbbMiddle.Image");
            tbbMiddle.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbMiddle.Name = "tbbMiddle";
            tbbMiddle.Size = new System.Drawing.Size(114, 22);
            tbbMiddle.Text = "Middle";
            tbbMiddle.Click += tbbMiddle_Click;
            // 
            // tbbBottom
            // 
            tbbBottom.Image = (System.Drawing.Image)resources.GetObject("tbbBottom.Image");
            tbbBottom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tbbBottom.Name = "tbbBottom";
            tbbBottom.Size = new System.Drawing.Size(114, 22);
            tbbBottom.Text = "Bottom";
            tbbBottom.Click += tbbBottom_Click;
            // 
            // tmrMotion
            // 
            tmrMotion.Tick += tmrMotion_Tick;
            // 
            // tmrSound
            // 
            tmrSound.Interval = 1;
            tmrSound.Tick += Timer1_Tick;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tspPreviewText });
            statusStrip1.Location = new System.Drawing.Point(0, 372);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 8, 0);
            statusStrip1.Size = new System.Drawing.Size(541, 20);
            statusStrip1.TabIndex = 12;
            statusStrip1.Text = "statusStrip1";
            statusStrip1.Visible = false;
            // 
            // tspPreviewText
            // 
            tspPreviewText.BorderStyle = System.Windows.Forms.Border3DStyle.Sunken;
            tspPreviewText.MergeIndex = 1;
            tspPreviewText.Name = "tspPreviewText";
            tspPreviewText.Size = new System.Drawing.Size(532, 15);
            tspPreviewText.Spring = true;
            tspPreviewText.Text = "blah";
            tspPreviewText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmPreview
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(541, 392);
            ControlBox = false;
            Controls.Add(pnlLogic);
            Controls.Add(pnlSound);
            Controls.Add(pnlView);
            Controls.Add(pnlPicture);
            Controls.Add(statusStrip1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPreview";
            ShowInTaskbar = false;
            Text = "Form1";
            Activated += frmPreview_Activated;
            Deactivate += frmPreview_Deactivate;
            FormClosing += frmPreview_FormClosing;
            Load += frmPreview_Load;
            VisibleChanged += frmPreview_VisibleChanged;
            KeyDown += frmPreview_KeyDown;
            KeyPress += frmPreview_KeyPress;
            PreviewKeyDown += frmPreview_PreviewKeyDown;
            pnlLogic.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rtfLogPrev).EndInit();
            cmsLogic.ResumeLayout(false);
            pnlPicture.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            pnlPicHeader.ResumeLayout(false);
            pnlPicHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)udPZoom).EndInit();
            pnlPicImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)fraPCorner).EndInit();
            ((System.ComponentModel.ISupportInitialize)imgPicture).EndInit();
            pnlSound.ResumeLayout(false);
            pnlSound.PerformLayout();
            pnlProgressBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picProgress).EndInit();
            pnlView.ResumeLayout(false);
            pnlView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picTrans).EndInit();
            tableLayoutPanel2.ResumeLayout(false);
            pnlViewHdr.ResumeLayout(false);
            pnlViewFtr.ResumeLayout(false);
            pnlViewFtr.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sldSpeed).EndInit();
            pnlCel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)fraVCorner).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCel).EndInit();
            tsViewPrev.ResumeLayout(false);
            tsViewPrev.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlLogic;
        private FastColoredTextBox rtfLogPrev;
        private System.Windows.Forms.Panel pnlPicture;
        private System.Windows.Forms.Panel pnlSound;
        private System.Windows.Forms.Panel pnlView;
        private System.Windows.Forms.ToolStrip tsViewPrev;
        private System.Windows.Forms.ToolStripButton tbbZoomIn;
        private System.Windows.Forms.ToolStripButton tbbZoomOut;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripSplitButton HAlign;
        private System.Windows.Forms.ToolStripMenuItem tbbAlignLeft;
        private System.Windows.Forms.ToolStripMenuItem tbbAlignCenter;
        private System.Windows.Forms.ToolStripMenuItem tbbAlignRight;
        private System.Windows.Forms.ToolStripSplitButton VAlign;
        private System.Windows.Forms.ToolStripMenuItem tbbTop;
        private System.Windows.Forms.ToolStripMenuItem tbbMiddle;
        private System.Windows.Forms.ToolStripMenuItem tbbBottom;
        private System.Windows.Forms.Timer tmrMotion;
        private System.Windows.Forms.Timer tmrSound;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pnlPicHeader;
        private System.Windows.Forms.RadioButton optPriority;
        private System.Windows.Forms.RadioButton optVisual;
        private System.Windows.Forms.NumericUpDown udPZoom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlPicImage;
        private System.Windows.Forms.VScrollBar vsbPic;
        private System.Windows.Forms.HScrollBar hsbPic;
        private System.Windows.Forms.PictureBox imgPicture;
        private System.Windows.Forms.PictureBox fraPCorner;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.Button cmdReset;
        private System.Windows.Forms.ComboBox cmbInst2;
        private System.Windows.Forms.ComboBox cmbInst1;
        private System.Windows.Forms.ComboBox cmbInst0;
        private System.Windows.Forms.CheckBox chkTrack3;
        private System.Windows.Forms.CheckBox chkTrack2;
        private System.Windows.Forms.CheckBox chkTrack1;
        private System.Windows.Forms.CheckBox chkTrack0;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel pnlViewHdr;
        private System.Windows.Forms.Label udCel;
        private System.Windows.Forms.Label udLoop;
        private System.Windows.Forms.Panel pnlViewFtr;
        private System.Windows.Forms.TrackBar sldSpeed;
        private System.Windows.Forms.ComboBox cmbMotion;
        private System.Windows.Forms.Button cmdVPlay;
        private System.Windows.Forms.Panel pnlCel;
        private System.Windows.Forms.HScrollBar hsbView;
        private System.Windows.Forms.VScrollBar vsbView;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.Button dCel;
        private System.Windows.Forms.Button uCel;
        private System.Windows.Forms.Button uLoop;
        private System.Windows.Forms.Button dLoop;
        private System.Windows.Forms.CheckBox chkTrans;
        private System.Windows.Forms.PictureBox picTrans;
        private System.Windows.Forms.PictureBox picCel;
        private System.Windows.Forms.PictureBox fraVCorner;
        private System.Windows.Forms.Panel pnlProgressBar;
        private System.Windows.Forms.PictureBox picProgress;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tspPreviewText;
        private System.Windows.Forms.RadioButton optMIDI;
        private System.Windows.Forms.RadioButton optPCjr;
        private System.Windows.Forms.ContextMenuStrip cmsLogic;
        private System.Windows.Forms.ToolStripMenuItem cmiSelectAll;
        private System.Windows.Forms.ToolStripMenuItem cmiCopy;
    }
}