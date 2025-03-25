using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;

namespace WinAGI.Editor {
    public partial class frmExportAnimatedGIF : Form {
        int formMode;
        Engine.View exportview;
        Loop exportloop;
        Picture exportpic;
        public GifOptions SelectedGifOptions;
        bool loaded = false;
        bool blnVisOn, blnXYDraw;
        byte bytCel;
        int lngPos;
        int MaxW, MaxH;
        const int VG_MARGIN = 4;

        public frmExportAnimatedGIF(Engine.View gifview, int loopnum) {
            InitializeComponent();
            InitForm(gifview, loopnum);
        }

        public frmExportAnimatedGIF(Picture picture) {
            InitializeComponent();
            InitForm(picture);
        }

        #region Event Handlers
        private void frmExportViewLoopOptions_FormClosing(object sender, FormClosingEventArgs e) {
            if (formMode == 0) {
                if (!loaded) {
                    exportview.Unload();
                }
            }
            else {
                if (!loaded) {
                    exportpic.Unload();
                }
            }
            timer1.Stop();
        }

        private void cmdCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();

        }

        private void cmdOK_Click(object sender, EventArgs e) {
            if (formMode == 0) {
                // copy new options back to global options
                // (the global variable is used as starting point for the 
                // next export action)
                DefaultVGOptions = SelectedGifOptions;
            }
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void tbAlignLeft_Click(object sender, EventArgs e) {
            tbAlignHorizontal.Image = tbAlignLeft.Image;
            SelectedGifOptions.HAlign = 0;
            UpdateAlignmentLabel();
            DisplayCel();
            timer1.Stop();
            timer1.Start();
        }

        private void tbAlignRight_Click(object sender, EventArgs e) {
            tbAlignHorizontal.Image = tbAlignRight.Image;
            SelectedGifOptions.HAlign = 1;
            UpdateAlignmentLabel();
            DisplayCel();
            timer1.Stop();
            timer1.Start();
        }

        private void tbAlignTop_Click(object sender, EventArgs e) {
            tbAlignVertical.Image = tbAlignTop.Image;
            SelectedGifOptions.VAlign = 0;
            UpdateAlignmentLabel();
            DisplayCel();
            timer1.Stop();
            timer1.Start();
        }

        private void tbAlignBottom_Click(object sender, EventArgs e) {
            tbAlignVertical.Image = tbAlignBottom.Image;
            SelectedGifOptions.VAlign = 1;
            UpdateAlignmentLabel();
            DisplayCel();
            timer1.Stop();
            timer1.Start();
        }

        private void chkTrans_CheckedChanged(object sender, EventArgs e) {
            SelectedGifOptions.Transparency = chkTrans.Checked;
            if (chkTrans.Checked) {
                picCel.BackColor = SystemColors.Control;
            }
            else {
                picCel.BackColor = exportloop.Cels[bytCel].Palette[(int)exportloop.Cels[bytCel].TransColor];
            }
            foreach (Cel cel in exportloop.Cels) {
                cel.Transparency = SelectedGifOptions.Transparency;
            }
            //force update
            DisplayCel();
        }

        private void chkLoop_CheckedChanged(object sender, EventArgs e) {
            SelectedGifOptions.Cycle = chkLoop.Checked;
        }

        private void udScale_ValueChanged(object sender, EventArgs e) {
            SelectedGifOptions.Zoom = (int)udScale.Value;
            if (formMode == 0) {
                picCel.Width = MaxW * SelectedGifOptions.Zoom * 2;
                picCel.Height = MaxH * SelectedGifOptions.Zoom;
                CheckScrollbars();
                DisplayCel();
            }
        }

        private void udScale_Enter(object sender, EventArgs e) {
            cmdOK.Select();
        }

        private void udDelay_ValueChanged(object sender, EventArgs e) {
            SelectedGifOptions.Delay = (int)udDelay.Value;
            timer1.Interval = 10 * (int)udDelay.Value;
        }

        private void udDelay_Enter(object sender, EventArgs e) {
            cmdOK.Select();
        }

        private void VScroll1_Scroll(object sender, ScrollEventArgs e) {
            picCel.Top = -VScroll1.Value;
        }

        private void HScroll1_Scroll(object sender, ScrollEventArgs e) {
            picCel.Left = -HScroll1.Value;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            //advance cel number for this loop
            byte bytCmd;
            switch (formMode) {
            case 0:
                //loop
                bytCel++;
                if (bytCel == exportloop.Cels.Count) {
                    bytCel = 0;
                    if (!SelectedGifOptions.Cycle) {
                        timer1.Interval += 1500;
                    }
                }
                else {
                    timer1.Interval = 10 * SelectedGifOptions.Delay;
                }
                DisplayCel();
                break;
            case 1:
                //picture
                do {
                    lngPos++;
                    if (lngPos >= exportpic.Size) {
                        //reset to beginning of picture
                        lngPos = -1;
                        break;
                    }
                    bytCmd = exportpic.Data[lngPos];

                    switch (bytCmd) {
                    case 240:
                        blnXYDraw = false;
                        blnVisOn = true;
                        lngPos++;
                        break;
                    case 241:
                        blnXYDraw = false;
                        blnVisOn = false;
                        break;
                    case 242:
                        blnXYDraw = false;
                        lngPos++;
                        break;
                    case 243:
                        blnXYDraw = false;
                        break;
                    case 244:
                    case 245:
                        blnXYDraw = true;
                        lngPos += 2;
                        break;
                    case 246:
                    case 247:
                    case 248:
                    case 250:
                        blnXYDraw = false;
                        lngPos += 2;
                        break;
                    case 249:
                        blnXYDraw = false;
                        lngPos++;
                        break;
                    default:
                        //skip second coordinate byte, unless
                        //currently drawing X or Y lines
                        if (!blnXYDraw) {
                            lngPos++;
                        }
                        break;
                    }
                }
                while ((bytCmd >= 240 && bytCmd < 244) || bytCmd == 249 || !blnVisOn);
                //show pic drawn up to this point
                exportpic.DrawPos = lngPos;
                ShowAGIBitmap(picGrid, exportpic.VisualBMP, 1);
                picGrid.Refresh();
                break;
            }
        }

        private void cmbLoop_SelectedIndexChanged(object sender, EventArgs e) {
            exportloop = exportview.Loops[cmbLoop.SelectedIndex];
            foreach (Cel cel in exportloop.Cels) {
                cel.Transparency = SelectedGifOptions.Transparency;
            }
            DisplayCel();
            timer1.Stop();
            timer1.Start();
        }
        #endregion

        private void InitForm(Engine.View view, int startloop) {
            // display the loop, and set preview using default export settings

            Text = "Export Loop As Animated GIF";
            formMode = 0;
            exportview = view;
            loaded = exportview.Loaded;
            if (!loaded) {
                exportview.Load();
            }
            if (startloop < 0) {
                startloop = 0;
            }
            else if (startloop >= view.Loops.Count) {
                startloop = view.Loops.Count - 1;
            }
            exportloop = exportview[startloop];
            cmbLoop.Items.Clear();
            for (int i = 0; i < view.Loops.Count; i++) {
                cmbLoop.Items.Add($"Loop {i}");
            }
            if (view.Loops.Count == 1) {
                cmbLoop.Enabled = false;
            }
            SelectedGifOptions = DefaultVGOptions;
            if (SelectedGifOptions.Cycle) {
                chkLoop.Checked = true;
            }
            else {
                chkLoop.Checked = false;
            }
            udDelay.Text = SelectedGifOptions.Delay.ToString();
            timer1.Interval = 10 * SelectedGifOptions.Delay;
            timer1.Enabled = true;
            UpdateAlignmentLabel();
            if (SelectedGifOptions.HAlign == 1) {
                tbAlignHorizontal.Image = tbAlignRight.Image;
            }
            if (SelectedGifOptions.VAlign == 0) {
                tbAlignVertical.Image = tbAlignTop.Image;
            }
            if (SelectedGifOptions.Transparency) {
                chkTrans.Checked = true;
            }
            else {
                chkTrans.Checked = false;
            }
            if (chkTrans.Checked) {
                DrawTransGrid(picCel, 0, 0);
                DrawTransGrid(picGrid, picCel.Left % 10, picCel.Top % 10);
            }
            else {
                picGrid.CreateGraphics().Clear(BackColor);
            }
            udScale.Text = SelectedGifOptions.Zoom.ToString();
            MaxW = 0;
            MaxH = 0;
            for (int i = 0; i < exportloop.Cels.Count; i++) {
                if (exportloop.Cels[i].Width > MaxW) {
                    MaxW = exportloop.Cels[i].Width;
                }
                if (exportloop.Cels[i].Height > MaxH) {
                    MaxH = exportloop.Cels[i].Height;
                }
            }

            //set size of view holder
            picCel.Width = MaxW * 2 * SelectedGifOptions.Zoom;
            picCel.Height = MaxH * SelectedGifOptions.Zoom;
            //force back to upper, left
            picCel.Top = VG_MARGIN;
            picCel.Left = VG_MARGIN;
            cmbLoop.SelectedIndex = startloop;
            CheckScrollbars();
        }

        private void InitForm(Picture picture) {

            Text = "Export Picture As Animated GIF";
            formMode = 1;
            exportpic = picture;
            loaded = exportpic.Loaded;
            if (!loaded) {
                exportpic.Load();
            }
            exportpic.StepDraw = true;

            // hide the alignment toolbar, scrollbars and transparency options
            toolStrip1.Visible = false;
            chkTrans.Visible = false;
            lblAlign.Visible = false;
            VScroll1.Visible = false;
            HScroll1.Visible = false;
            picCel.Visible = false;
            label3.Visible = false;
            cmbLoop.Visible = false;
            chkLoop.Checked = true;
            lngPos = -1;
            SelectedGifOptions = new();
            SelectedGifOptions.Zoom = 1;
            SelectedGifOptions.Delay = 1;
            SelectedGifOptions.Cycle = true;
            if (SelectedGifOptions.Cycle) {
                chkLoop.Checked = true;
            }
            else {
                chkLoop.Checked = false;
            }
            udScale.Text = SelectedGifOptions.Zoom.ToString();
            udDelay.Text = SelectedGifOptions.Delay.ToString();
            picGrid.Top -= 13;
            picGrid.Height -= 95;
            chkLoop.Top -= 95;
            lblScale.Top -= 95;
            label1.Top -= 95;
            udScale.Top -= 95;
            udDelay.Top -= 95;
            label2.Top -= 95;
            cmdCancel.Top -= 125;
            cmdOK.Top -= 125;
            Height -= 125;

            timer1.Interval = 10 * SelectedGifOptions.Delay;
            timer1.Enabled = true;
        }

        void CheckScrollbars() {
            //shrink grid if cel is small
            if (picCel.Width + 2 * VG_MARGIN < 322) {
                picGrid.Width = picCel.Width + 2 * VG_MARGIN;
            }
            else {
                picGrid.Width = 322;
            }
            if (picCel.Height + 2 * VG_MARGIN < 265) {
                picGrid.Height = picCel.Height + 2 * VG_MARGIN;
            }
            else {
                picGrid.Height = 265;
            }

            // Scrollbar math:
            // ACT_SZ = size of the area being scrolled; usually the image size + margins
            // WIN_SZ = size of the window area; the container's client size
            // SV_MAX = maximum value that scrollbar can have; this puts the scroll bar
            //          and scrolled image at farthest position
            // LG_CHG = LargeChange property of the scrollbar
            // SB_MAX = actual Maximum property of the scrollbar, to avoid out-of-bounds errors
            //
            //      SV_MAX = ACT_SZ - WIN_SZ 
            //      SB_MAX = SV_MAX + LG_CHG + 1
            //
            // when including margins, the calculations are modified to:
            //      ACT_SZ = MGN + IMG_SZ + MGN
            //      SB_MIN = -MGN
            //      SV_MAX = ACT_SZ - WIN_SZ + SB_MIN
            //             = MGN + IMG_SZ + MGN + SB_MIN - WIN_SZ
            //             = MGN + IMG_SZ + MGN - MGN - WIN_SZ
            //      SV_MAX = IMG_SZ - WIN_SZ + MGN

            HScroll1.Visible = picCel.Width > picGrid.Width - 2 * VG_MARGIN;
            VScroll1.Visible = picCel.Height > picGrid.Height - 2 * VG_MARGIN;
            if (HScroll1.Visible) {
                // (LargeChange value can't exceed Max value, so set Max to high enough
                // value so it can be calculated correctly later)
                HScroll1.Maximum = picGrid.Width;
                HScroll1.SmallChange = (int)(picGrid.Width * LG_SCROLL);
                HScroll1.LargeChange = (int)(picGrid.Width * SM_SCROLL);
                int SV_MAX = picCel.Width - picGrid.Width + VG_MARGIN;
                // Max value: = desired actual Max + LargeChange - 1
                HScroll1.Maximum = SV_MAX + HScroll1.LargeChange - 1;
                int newscroll = HScroll1.Value;
                if (newscroll < -VG_MARGIN) {
                    HScroll1.Value = -VG_MARGIN;
                }
                else if (newscroll > SV_MAX) {
                    HScroll1.Value = SV_MAX;
                }
                picCel.Left = -HScroll1.Value;
            }
            else {
                picCel.Left = VG_MARGIN;
                HScroll1.Value = -VG_MARGIN;
            }

            if (VScroll1.Visible) {
                VScroll1.Maximum = picGrid.Height;
                VScroll1.SmallChange = (int)(picGrid.Height * LG_SCROLL);
                VScroll1.LargeChange = (int)(picGrid.Height * SM_SCROLL);
                int SV_MAX = picCel.Height - picGrid.Height + VG_MARGIN;
                VScroll1.Maximum = SV_MAX + VScroll1.LargeChange - 1;
                int newscroll = VScroll1.Value;
                if (newscroll < -VG_MARGIN) {
                    VScroll1.Value = -VG_MARGIN;
                }
                else if (newscroll > SV_MAX) {
                    VScroll1.Value = SV_MAX;
                }
                picCel.Top = -VScroll1.Value;
            }
            else {
                VScroll1.Value = -VG_MARGIN;
                picCel.Top = VG_MARGIN;
            }
            return;
        }

        void DisplayCel() {
            //this function copies the bitmap Image
            //from bytLoop.bytCel into the view Image box,
            //and resizes it to be correct size
            int tgtX, tgtY, tgtH, tgtW;

            tgtW = exportloop.Cels[bytCel].Width * 2 * SelectedGifOptions.Zoom;
            tgtH = exportloop.Cels[bytCel].Height * SelectedGifOptions.Zoom;
            if (SelectedGifOptions.HAlign == 0) {
                tgtX = 0;
            }
            else {
                tgtX = picCel.Width - tgtW;
            }
            if (SelectedGifOptions.VAlign == 0) {
                tgtY = 0;
            }
            else {
                tgtY = picCel.Height - tgtH;
            }
            ShowAGIBitmap(picCel, exportloop.Cels[bytCel].CelBMP, tgtX, tgtY, tgtW, tgtH);
            if (chkTrans.Checked) {
                // draws single pixel dots spaced 10 pixels apart over transparent pixels only
                using Graphics gc = Graphics.FromImage(picCel.Image);
                Bitmap b = new(picCel.Image);
                for (int i = 0; i < picCel.Width; i += 10) {
                    for (int j = 0; j < picCel.Height; j += 10) {
                        if (b.GetPixel(i, j).ToArgb() == picCel.BackColor.ToArgb()) {
                            gc.FillRectangle(Brushes.Black, new Rectangle(i, j, 1, 1));
                        }
                    }
                }
            }
        }

        void UpdateAlignmentLabel() {
            lblAlign.Text = "Align: ";
            if (SelectedGifOptions.VAlign == 0) {
                lblAlign.Text += "Top, ";
            }
            else {
                lblAlign.Text += "Bottom, ";
            }
            if (SelectedGifOptions.HAlign == 0) {
                lblAlign.Text += "Left";
            }
            else {
                lblAlign.Text += "Right";
            }
        }
    }
}
