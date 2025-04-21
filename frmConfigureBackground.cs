using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Common.API;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmConfigureBackground : Form {
        private frmPicEdit PicEditForm;
        public PictureBackgroundSettings bkgdSettings;
        public Bitmap BkgdImage, example, viscopy;
        // selection/move variables
        private const int RESIZE_BORDER = 5;
        private Point anchor, offset = new(0, 0);
        private MoveMode moveMode;
        private int scalefactor = 1;
        private int minsize = 32, minoverlap = 30;
        private enum MoveMode {
            None,
            MoveExample,
            MoveBkgd,
            SizeBkgdNS,
            SizeBkgdWE,
            SizeBkgdAll,
        }

        public frmConfigureBackground(frmPicEdit owner) {
            InitializeComponent();
            udScale.SelectedIndex = 2;
            InitForm(owner);
        }

        #region Event Handlers
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (txtTransparency.Focused) {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            // use arrow keys to nudge/resize the images
            switch ((int)keyData & 0x70000) {
            case 0:
                // NO modifier key: nudge the example image
                switch (keyData & (Keys)0xffff) {
                case Keys.Left:
                    MoveExample(picExample.Left - 1, picExample.Top);
                    return true;
                case Keys.Right:
                    MoveExample(picExample.Left + 1, picExample.Top);
                    return true;
                case Keys.Up:
                    MoveExample(picExample.Left, picExample.Top - 1);
                    return true;
                case Keys.Down:
                    MoveExample(picExample.Left, picExample.Top + 1);
                    return true;
                }
                break;
            case 0x10000:
                // SHIFT key: nudge the background image
                switch (keyData & (Keys)0xffff) {
                case Keys.Left:
                    MoveBackground(picBackground.Left - 1, picBackground.Top);
                    return true;
                case Keys.Right:
                    MoveBackground(picBackground.Left + 1, picBackground.Top);
                    return true;
                case Keys.Up:
                    MoveBackground(picBackground.Left, picBackground.Top - 1);
                    return true;
                case Keys.Down:
                    MoveBackground(picBackground.Left, picBackground.Top + 1);
                    return true;
                }
                break;
            case 0x20000:
                // CTRL key: resize the background image
                switch (keyData & (Keys)0xffff) {
                case Keys.Left:
                    ChangeBkgdWidth(picBackground.Width - 1);
                    return true;
                case Keys.Right:
                    ChangeBkgdWidth(picBackground.Width + 1);
                    return true;
                case Keys.Up:
                    ChangeBkgdHeight(picBackground.Height - 1);
                    return true;
                case Keys.Down:
                    ChangeBkgdHeight(picBackground.Height + 1);
                    return true;
                }
                break;
            //case 0x40000:
            //    // ALT key
            //    switch (keyData & (Keys)0xffff) {
            //    case Keys.Left:
            //    case Keys.Right:
            //    case Keys.Up:
            //    case Keys.Down:
            //        Debug.Print("   ALT: " + (keyData & (Keys)0xffff).ToString());
            //        return true;
            //    }
            //    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void frmConfigureBackground_VisibleChanged(object sender, EventArgs e) {
            if (Visible) {
                UpdateScrollbars();
            }
        }

        private void chkVisual_CheckedChanged(object sender, EventArgs e) {
            if (!chkVisual.Checked && !chkPriority.Checked) {
                chkPriority.Checked = true;
                bkgdSettings.ShowVis = chkVisual.Checked;
                bkgdSettings.ShowPri = chkPriority.Checked;
            }
        }

        private void chkPriority_CheckedChanged(object sender, EventArgs e) {
            if (!chkVisual.Checked && !chkPriority.Checked) {
                chkVisual.Checked = true;
            }
            bkgdSettings.ShowVis = chkVisual.Checked;
            bkgdSettings.ShowPri = chkPriority.Checked;
        }

        private void chkDefaultVis_Click(object sender, EventArgs e) {
            bkgdSettings.DefaultAlwaysTransparent = chkDefaultVis.Checked;
            if (Visible) {
                picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
            }
        }

        private void cmdOK_Click(object sender, EventArgs e) {
            float HScale = picBackground.Width / (float)BkgdImage.Width;
            float VScale = picBackground.Height / (float)BkgdImage.Height;
            bkgdSettings.SourceRegion.X = (float)((picExample.Left - picBackground.Left) / HScale);
            bkgdSettings.SourceRegion.Y = (float)((picExample.Top - picBackground.Top) / VScale);
            bkgdSettings.SourceRegion.Width = (320f * scalefactor / HScale);
            bkgdSettings.SourceRegion.Height = (168f * scalefactor / VScale);
            bkgdSettings.TargetPos.X = (picExample.Left - picBackground.Left) / scalefactor;
            bkgdSettings.TargetPos.Y = (picExample.Top - picBackground.Top) / scalefactor;
            bkgdSettings.SourceSize.Width = picBackground.Width / scalefactor;
            bkgdSettings.SourceSize.Height = picBackground.Height / scalefactor;
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void cmdCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void cmdLoad_Click(object sender, EventArgs e) {
            if (GetBkgdFile()) {
                // reposition and resize border frames
                picExample.Left = picExample.Top = 0;
                picBackground.Left = picBackground.Top = 0;
                picBackground.Width = 320 * scalefactor;
                picBackground.Height = 168 * scalefactor;
                picBackground.Image = new Bitmap(BkgdImage, 320 * scalefactor, 168 * scalefactor);
                picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
                HScroll1.Visible = VScroll1.Visible = picCorner.Visible = false;
            }
        }

        private void cmdStretch_Click(object sender, EventArgs e) {
            picExample.Left = picExample.Top = 0;
            picBackground.Left = picBackground.Top = 0;
            picBackground.Width = 320 * scalefactor;
            picBackground.Height = 168 * scalefactor;
            picBackground.Image = new Bitmap(BkgdImage, 320 * scalefactor, 168 * scalefactor);
            picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
            HScroll1.Visible = VScroll1.Visible = picCorner.Visible = false;
        }

        private void cmdFull_Click(object sender, EventArgs e) {
            picExample.Left = picExample.Top = 0;
            picBackground.Left = picBackground.Top = 0;
            picBackground.Width = BkgdImage.Width * scalefactor;
            picBackground.Height = BkgdImage.Height * scalefactor;
            picBackground.Image = new Bitmap(BkgdImage, BkgdImage.Width * scalefactor, BkgdImage.Height * scalefactor);
            picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
            UpdateScrollbars();
        }

        private void sldTrans_Scroll(object sender, EventArgs e) {
            txtTransparency.Text = sldTrans.Value.ToString();
            bkgdSettings.Transparency = (byte)sldTrans.Value;
            picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
        }

        private void txtTransparency_Validating(object sender, CancelEventArgs e) {
            double val;
            if (double.TryParse(txtTransparency.Text, out val)) {
                val = 0;
            }
            else {
                if (val < 0) {
                    txtTransparency.Text = "0";
                    val = 0;
                }
                else if (val > 100) {
                    txtTransparency.Text = "100";
                    val = 100;
                }
            }
            bkgdSettings.Transparency = (byte)val;
            picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
        }

        private void txtTransparency_KeyDown(object sender, KeyEventArgs e) {
            // ignore everything except numbers, backspace, delete, enter, tab, and escape
            switch (e.KeyCode) {
            case Keys.Enter:
                cmdOK.Select();
                e.SuppressKeyPress = true;
                break;
            case Keys.Escape:
                txtTransparency.Text = bkgdSettings.Transparency.ToString();
                cmdOK.Select();
                e.SuppressKeyPress = true;
                break;
            }
        }

        private void txtTransparency_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore everything except numbers, backspace, delete, enter, tab, and escape
            switch (e.KeyChar) {
            case '\x08':
            case '\x09':
                break;
            case < '0':
            case > '9':
                e.Handled = true;
                break;
            }
        }

        private void txtTransparency_Leave(object sender, EventArgs e) {
            AcceptButton = cmdOK;
            CancelButton = cmdCancel;
        }

        private void txtTransparency_Enter(object sender, EventArgs e) {
            AcceptButton = null;
            CancelButton = null;
        }

        private void VScroll1_Scroll(object sender, ScrollEventArgs e) {
            if (e.OldValue == e.NewValue) {
                return;
            }
            Rectangle r = Rectangle.Union(picExample.Bounds, picBackground.Bounds);
            int offset = picBackground.Top - r.Top;
            picBackground.Top = -e.NewValue + offset;
            offset = picExample.Top - r.Top;
            picExample.Top = -e.NewValue + offset;
            UpdateScrollbars();
        }

        private void HScroll1_Scroll(object sender, ScrollEventArgs e) {
            if (e.OldValue == e.NewValue) {
                return;
            }
            Rectangle r = Rectangle.Union(picExample.Bounds, picBackground.Bounds);
            int offset = picBackground.Left - r.Left;
            picBackground.Left = -e.NewValue + offset;
            offset = picExample.Left - r.Left;
            picExample.Left = -e.NewValue + offset;
            UpdateScrollbars();
        }

        private void picBackground_MouseDown(object sender, MouseEventArgs e) {
            // ignore right-clicks
            if (e.Button == MouseButtons.Right) {
                return;
            }
            offset.X = picBackground.Width - e.X;
            offset.Y = picBackground.Height - e.Y;
            Rectangle r = new(picBackground.Width - RESIZE_BORDER, picBackground.Height - RESIZE_BORDER,
                              RESIZE_BORDER, RESIZE_BORDER);
            if (r.Contains(e.Location)) {
                // on corner
                moveMode = MoveMode.SizeBkgdAll;
                picBackground.Cursor = Cursors.SizeNWSE;
                return;
            }
            r = new(picBackground.Width - RESIZE_BORDER, 0, RESIZE_BORDER, picBackground.Height);
            if (r.Contains(e.Location)) {
                // on right edge
                moveMode = MoveMode.SizeBkgdWE;
                picBackground.Cursor = Cursors.SizeWE;
                return;
            }
            r = new(0, picBackground.Height - RESIZE_BORDER, picBackground.Width, RESIZE_BORDER);
            if (r.Contains(e.Location)) {
                // on bottom edge
                moveMode = MoveMode.SizeBkgdNS;
                picBackground.Cursor = Cursors.SizeNS;
                return;
            }
            // otherwise, move the background
            moveMode = MoveMode.MoveBkgd;
            anchor = e.Location;
            picBackground.Cursor = Cursors.Hand;
        }

        private void picBackground_MouseMove(object sender, MouseEventArgs e) {
            int newW, newH;
            switch (moveMode) {
            case MoveMode.None:
                // change cursor depending on location
                Rectangle r = new(picBackground.Width - RESIZE_BORDER, picBackground.Height - RESIZE_BORDER,
                                  RESIZE_BORDER, RESIZE_BORDER);
                if (r.Contains(e.Location)) {
                    picBackground.Cursor = Cursors.SizeNWSE;
                }
                else {
                    r = new(picBackground.Width - RESIZE_BORDER, 0, RESIZE_BORDER, picBackground.Height);
                    if (r.Contains(e.Location)) {
                        picBackground.Cursor = Cursors.SizeWE;
                    }
                    else {
                        r = new(0, picBackground.Height - RESIZE_BORDER, picBackground.Width, RESIZE_BORDER);
                        if (r.Contains(e.Location)) {
                            picBackground.Cursor = Cursors.SizeNS;
                        }
                        else {
                            picBackground.Cursor = Cursors.Default;
                        }
                    }
                }
                break;
            case MoveMode.MoveBkgd:
                int newL = picBackground.Left + e.X - anchor.X, newT = picBackground.Top + e.Y - anchor.Y;
                MoveBackground(newL, newT);
                break;
            case MoveMode.SizeBkgdNS:
                newH = e.Y + offset.Y;
                ChangeBkgdHeight(newH);
                break;
            case MoveMode.SizeBkgdWE:
                newW = e.X + offset.X;
                ChangeBkgdWidth(newW);
                break;
            case MoveMode.SizeBkgdAll:
                newW = e.X + offset.X;
                newH = e.Y + offset.Y;

                ChangeBkgdSize(newW, newH);
                break;
            }
        }

        private void picBackground_MouseUp(object sender, MouseEventArgs e) {
            moveMode = MoveMode.None;
            picBackground.Cursor = Cursors.Default;
            picBackground.Capture = false;
        }

        private void picExample_MouseDown(object sender, MouseEventArgs e) {
            // check for cursor over the picBackground resize targets
            // (in cases where the example overlaps the background resize target)
            offset.X = picBackground.Width - (e.X + picExample.Left - picBackground.Left);
            offset.Y = picBackground.Height - (e.Y + picExample.Top - picBackground.Top);

            // ignore right-clicks
            if (e.Button == MouseButtons.Right) {
                return;
            }
            Point bkgdmp = new(e.X + picExample.Left - picBackground.Left, e.Y + picExample.Top - picBackground.Top);
            Rectangle r = new(picBackground.Width - RESIZE_BORDER, picBackground.Height - RESIZE_BORDER,
                              RESIZE_BORDER, RESIZE_BORDER);
            if (r.Contains(bkgdmp)) {
                // on corner
                moveMode = MoveMode.SizeBkgdAll;
                picBackground.Cursor = Cursors.SizeNWSE;
                picBackground.Capture = true;
                return;
            }
            r = new(picBackground.Width - RESIZE_BORDER, 0, RESIZE_BORDER, picBackground.Height);
            if (r.Contains(bkgdmp)) {
                // on right edge
                moveMode = MoveMode.SizeBkgdWE;
                picBackground.Cursor = Cursors.SizeWE;
                picBackground.Capture = true;
                return;
            }
            r = new(0, picBackground.Height - RESIZE_BORDER, picBackground.Width, RESIZE_BORDER);
            if (r.Contains(bkgdmp)) {
                // on bottom edge
                moveMode = MoveMode.SizeBkgdNS;
                picBackground.Cursor = Cursors.SizeNS;
                picBackground.Capture = true;
                return;
            }
            // otherwise, move the example
            moveMode = MoveMode.MoveExample;
            anchor = e.Location;
            picExample.Cursor = Cursors.Hand;
        }

        private void picExample_MouseMove(object sender, MouseEventArgs e) {
            switch (moveMode) {
            case MoveMode.None: {
                // change cursor depending on location
                Rectangle r = new(picBackground.Width - RESIZE_BORDER, picBackground.Height - RESIZE_BORDER,
                                  RESIZE_BORDER, RESIZE_BORDER);
                Point bkgdmp = new(e.X + picExample.Left - picBackground.Left, e.Y + picExample.Top - picBackground.Top);
                if (r.Contains(bkgdmp)) {
                    picBackground.Cursor = Cursors.SizeNWSE;
                }
                else {
                    r = new(picBackground.Width - RESIZE_BORDER, 0, RESIZE_BORDER, picBackground.Height);
                    if (r.Contains(bkgdmp)) {
                        picBackground.Cursor = Cursors.SizeWE;
                    }
                    else {
                        r = new(0, picBackground.Height - RESIZE_BORDER, picBackground.Width, RESIZE_BORDER);
                        if (r.Contains(bkgdmp)) {
                            picBackground.Cursor = Cursors.SizeNS;
                        }
                        else {
                            picBackground.Cursor = Cursors.Default;
                        }
                    }
                }
                break;
            }
            case MoveMode.MoveExample:
                int newL = picExample.Left + e.X -anchor.X, newT = picExample.Top + e.Y -anchor.Y;
                MoveExample(newL, newT);
                break;
            }
        }

        private void picExample_MouseUp(object sender, MouseEventArgs e) {
            moveMode = MoveMode.None;
            picExample.Cursor = Cursors.Default;
        }

        private void udScale_SelectedItemChanged(object sender, EventArgs e) {
            //adjust scalefactor
            int oldscale = scalefactor;
            scalefactor = int.Parse(((string)(udScale.SelectedItem))[..3]) / 100;
            // adjust the size and position of the images
            picBackground.Left = picBackground.Left / oldscale * scalefactor;
            picBackground.Top = picBackground.Top / oldscale * scalefactor;
            picBackground.Width = bkgdSettings.SourceSize.Width * scalefactor;
            picBackground.Height = bkgdSettings.SourceSize.Height * scalefactor;
            picBackground.Image = new Bitmap(BkgdImage, picBackground.Width, picBackground.Height);
            picExample.Left = picExample.Left / oldscale * scalefactor;
            picExample.Top = picExample.Top / oldscale * scalefactor;
            picExample.Width = 320 * scalefactor;
            picExample.Height = 168 * scalefactor;
            try {
                example = new(320 * scalefactor, 168 * scalefactor);
                using (Graphics g = Graphics.FromImage(example)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(viscopy, 0, 0, 320 * scalefactor, 168 * scalefactor);
                }
                // convert it back to indexed bmp so palette can be edited
                example = example.Clone(new(0, 0, 320 * scalefactor, 168 * scalefactor), PixelFormat.Format8bppIndexed);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
            minsize = 32 * scalefactor;
            minoverlap = 30 * scalefactor;
            UpdateScrollbars();
        }
        #endregion

        private void InitForm(frmPicEdit owner) {
            PicEditForm = owner;
            bkgdSettings = owner.EditPicture.BackgroundSettings;
            chkDefaultVis.Checked = bkgdSettings.DefaultAlwaysTransparent;
            chkVisual.Checked = bkgdSettings.ShowVis;
            chkPriority.Checked = bkgdSettings.ShowPri;
            sldTrans.Value = bkgdSettings.Transparency;
            txtTransparency.Text = bkgdSettings.Transparency.ToString();
            string currentfile = bkgdSettings.FileName;
            if (currentfile.Length != 0) {
                try {
                    BkgdImage = new(Path.GetFullPath(currentfile, EditGame.ResDir));
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "File error - unable to load this image.",
                        "",
                        "Invalid Image File");
                    // use a blank white image
                    BkgdImage = new(320 * scalefactor, 168 * scalefactor);
                    using (Graphics g = Graphics.FromImage(BkgdImage)) {
                        g.Clear(Color.White);
                    }
                }
            }
            else {
                currentfile = "";
            }
            if (currentfile.Length == 0) {
                if (!GetBkgdFile()) {
                    DialogResult = DialogResult.Cancel;
                    Hide();
                    return;
                }
            }
            // display background image using initial settings
            picBackground.Left = -bkgdSettings.TargetPos.X * scalefactor;
            picBackground.Top = -bkgdSettings.TargetPos.Y * scalefactor;
            picBackground.Width = bkgdSettings.SourceSize.Width * scalefactor;
            picBackground.Height = bkgdSettings.SourceSize.Height * scalefactor;
            picExample.Left = 0;
            picExample.Top = 0;
            picExample.Width = 320 * scalefactor;
            picExample.Height = 168 * scalefactor;
            Rectangle r = Rectangle.Union(picBackground.Bounds, picExample.Bounds);
            if (r.Location != new Point(0, 0)) {
                Point offset = new(-r.Location.X, -r.Location.Y);
                Point loc = picExample.Location;
                loc.Offset(offset);
                picExample.Location = loc;
                loc = picBackground.Location;
                loc.Offset(offset);
                picBackground.Location = loc;
            }
            picBackground.Image = new Bitmap(BkgdImage, picBackground.Width, picBackground.Height);

            try {
                // for example pic, we need the non-transparent bitmap
                // make a copy of the visualBMP and clear out transparency values
                viscopy = (Bitmap)owner.EditPicture.VisualBMP.Clone();
                ColorPalette palette = viscopy.Palette;
                for (int i = 0; i < palette.Entries.Length; i++) {
                    palette.Entries[i] = Color.FromArgb(255, palette.Entries[i]);
                }
                viscopy.Palette = palette;
                // scale it to fit the example box (which removes indexing)
                example = new(320 * scalefactor, 168 * scalefactor);
                using (Graphics g = Graphics.FromImage(example)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(viscopy, 0, 0, 320 * scalefactor, 168 * scalefactor);
                }
                // convert it back to indexed bmp so palette can be edited
                example = example.Clone(new(0, 0, 320 * scalefactor, 168 * scalefactor), PixelFormat.Format8bppIndexed);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            // Set the image with initial opacity to the PictureBox
            picExample.Image = SetImageOpacity(example, (float)(100 - bkgdSettings.Transparency) / 100);
            // UpdateScrollbars doesn't work here because the form is not visible
            //UpdateScrollbars();
        }

        private bool GetBkgdFile() {
            bool existing = bkgdSettings.FileName.Length > 0;
            MDIMain.OpenDlg.ShowReadOnly = false;
            MDIMain.OpenDlg.CheckFileExists = true;
            MDIMain.OpenDlg.Filter = "Image Files(*.bmp; *.jpg; *.jpeg *.gif; *.tif; *.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.tif;*.png|" +
                "BMP files (*.bmp)|*.bmp|" +
                "JPEG files (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "GIF files (*.gif)|*.gif|" +
                "TIFF files (*.tif)|*.tif|" +
                "PNG files (*.PNG)|*.png|" +
                "All files (*.*)|*.*";
            if (existing) {
                MDIMain.OpenDlg.Title = "Choose a New Background Image for this Picture";
                MDIMain.OpenDlg.FileName = Path.GetFileName(bkgdSettings.FileName);
                switch (Path.GetExtension(bkgdSettings.FileName).ToLower()) {
                case "bmp":
                    MDIMain.OpenDlg.FilterIndex = 2;
                    break;
                case "jpg" or " jpeg":
                    MDIMain.OpenDlg.FilterIndex = 3;
                    break;
                case "gif":
                    MDIMain.OpenDlg.FilterIndex = 4;
                    break;
                case "tif":
                    MDIMain.OpenDlg.FilterIndex = 5;
                    break;
                case "png":
                    MDIMain.OpenDlg.FilterIndex = 6;
                    break;
                default:
                    MDIMain.OpenDlg.FilterIndex = 7;
                    break;
                }
                MDIMain.OpenDlg.InitialDirectory = Path.GetFullPath(bkgdSettings.FileName, EditGame.ResDir);
            }
            else {
                MDIMain.OpenDlg.Title = "Choose a Background Image for this Picture";
                MDIMain.OpenDlg.FilterIndex = 1;
                MDIMain.OpenDlg.FileName = "";
                MDIMain.OpenDlg.InitialDirectory = DefaultResDir;
            }
            if (MDIMain.OpenDlg.ShowDialog() != DialogResult.OK) {
                return false;
            }
            DefaultResDir = Common.Base.JustPath(MDIMain.OpenDlg.FileName);
            bkgdSettings.FileName = Path.GetRelativePath(EditGame.ResDir, MDIMain.OpenDlg.FileName);
            try {
                BkgdImage = new(MDIMain.OpenDlg.FileName);
            }
            catch (Exception ex) {
                ErrMsgBox(ex, "File error. Unable to open this image file.",
                    "",
                    "Load Background Image Error");
                return false;
            }
            return true;
        }

        public Bitmap SetImageOpacity(Bitmap image, float opacity) {
            ColorPalette palette = image.Palette;
            for (int i = 0; i < palette.Entries.Length; i++) {
                if (bkgdSettings.DefaultAlwaysTransparent && Color.FromArgb(255, palette.Entries[i]) == PicEditForm.EditPalette[15]) {
                    palette.Entries[i] = Color.FromArgb(0, palette.Entries[i]);
                }
                else {
                    palette.Entries[i] = Color.FromArgb((int)(opacity * 255), palette.Entries[i]);
                }
            }
            image.Palette = palette;
            return image;
        }

        private void MoveBackground(int newL, int newT) {
            int newR, newB;
            // before moving, verify the new location wiil keep the two 
            // images overlapping by at least minoverlap pixels
            if (picBackground.Bounds.Contains(picExample.Location)) {
                newR = newL + picBackground.Width;
                newB = newT + picBackground.Height;
                if (newR - picExample.Left < minoverlap) {
                    newL = minoverlap + picExample.Left - picBackground.Width;
                }
                if (newB - picExample.Top < minoverlap) {
                    newT = minoverlap + picExample.Top - picBackground.Height;
                }
            }
            else {
                if (picExample.Right - newL < minoverlap) {
                    newL = picExample.Right - minoverlap;
                }
                if (picExample.Bottom - newT < minoverlap) {
                    newT = picExample.Bottom - minoverlap;
                }
            }
            if (newL == picBackground.Left && newT == picBackground.Top) {
                return;
            }
            picBackground.Left = newL;
            picBackground.Top = newT;
            UpdateScrollbars();
        }

        private void MoveExample(int newL, int newT) {
            // before moving, verify the new location wiil keep the two 
            // images overlapping by at least minoverlap pixels
            int newR, newB;
            if (picExample.Bounds.Contains(picBackground.Location)) {
                newR = newL + picExample.Width;
                newB = newT + picExample.Height;
                if (newR - picBackground.Left < minoverlap) {
                    newL = minoverlap + picBackground.Left - picExample.Width;
                }
                if (newB - picBackground.Top < minoverlap) {
                    newT = minoverlap + picBackground.Top - picExample.Height;
                }
            }
            else {
                if (picBackground.Right - newL < minoverlap) {
                    newL = picBackground.Right - minoverlap + anchor.X - picExample.Left;
                }
                if (picBackground.Bottom - newT < minoverlap) {
                    newT = picBackground.Bottom - minoverlap + anchor.Y - picExample.Top;
                }
            }
            if (newL == picExample.Left && newT == picExample.Top) {
                return;
            }
            picExample.Left = newL;
            picExample.Top = newT;
            UpdateScrollbars();
            pnlBackSurface.Refresh();
        }

        private void ChangeBkgdSize(int newW, int newH) {
            int newR, newB;
            if (newW < minsize) {
                newW = minsize;
            }
            if (newH < minsize) {
                newH = minsize;
            }
            newB = picBackground.Top + newH;
            if (picExample.Bottom > newB) {
                // limit overlap to minoverlap
                if (newB - picExample.Top < minoverlap) {
                    newH = minoverlap + picExample.Top - picBackground.Top;
                }
            }
            newR = picBackground.Left + newW;
            if (picExample.Right > newR) {
                // limit overlap to minoverlap
                if (newR - picExample.Left < minoverlap) {
                    newW = minoverlap + picExample.Left - picBackground.Left;
                }
            }
            if (newW == picBackground.Width && newH == picBackground.Height) {
                return;
            }
            picBackground.Height = newH;
            picBackground.Width = newW;
            picBackground.Image = new Bitmap(BkgdImage, picBackground.Width, picBackground.Height);
            UpdateScrollbars();
        }

        private void ChangeBkgdWidth(int newW) {
            int newR;
            if (newW < minsize) {
                newW = minsize;
            }
            newR = picBackground.Left + newW;
            if (picExample.Right > newR) {
                // limit overlap to minoverlap
                if (newR - picExample.Left < minoverlap) {
                    newW = minoverlap + picExample.Left - picBackground.Left;
                }
            }
            if (newW == picBackground.Width) {
                return;
            }
            picBackground.Width = newW;
            picBackground.Image = new Bitmap(BkgdImage, picBackground.Width, picBackground.Height);
            UpdateScrollbars();
        }

        private void ChangeBkgdHeight(int newH) {
            int newB;
            if (newH < minsize) {
                newH = minsize;
            }
            newB = picBackground.Top + newH;
            if (picExample.Bottom > newB) {
                // limit overlap to minoverlap
                if (newB - picExample.Top < minoverlap) {
                    newH = minoverlap + picExample.Top - picBackground.Top;
                }
            }
            if (newH == picBackground.Height) {
                return;
            }
            picBackground.Height = newH;
            picBackground.Image = new Bitmap(BkgdImage, picBackground.Width, picBackground.Height);
            UpdateScrollbars();
        }

        private void UpdateScrollbars() {
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
            // in this case, the 'image' is the union of the example and background images
            // ACT_SZ = UNION(EXAMPLE, BACKGROUND)
            //
            // also, we allow the image to be outside the normal bounds(i.e. too far left
            // or right) so min/max need to be adjusted in those cases
            Rectangle r = Rectangle.Union(picExample.Bounds, picBackground.Bounds);
            HScroll1.Visible = r.Left < 0 || r.Right > pnlBackSurface.Width || (r.Left > 0 && r.Width < pnlBackSurface.Width);
            VScroll1.Visible = r.Top < 0 || r.Bottom > pnlBackSurface.Height - (HScroll1.Visible ? HScroll1.Height : 0) || (r.Top > 0 && r.Height < pnlBackSurface.Height - (HScroll1.Visible ? HScroll1.Height : 0));
            HScroll1.Visible = r.Left < 0 || r.Right > pnlBackSurface.Width - (VScroll1.Visible ? VScroll1.Width : 0) || (r.Left > 0 && r.Width < pnlBackSurface.Width - (VScroll1.Visible ? VScroll1.Width : 0));
            if (HScroll1.Visible && VScroll1.Visible) {
                HScroll1.Width = picCorner.Left;
                VScroll1.Height = picCorner.Top;
                picCorner.Visible = true;
            }
            else if (HScroll1.Visible || VScroll1.Visible) {
                HScroll1.Width = pnlBackSurface.Width;
                VScroll1.Height = pnlBackSurface.Height;
                picCorner.Visible = false;
            }
            else {
                picCorner.Visible = false;
                return;
            }
            int SVMAX;
            bool tempscroll;
            if (HScroll1.Visible) {
                // (LargeChange value can't exceed Max value, so set Max to high enough
                // value so it can be calculated correctly later)
                HScroll1.Maximum = r.Width * 2;
                HScroll1.LargeChange = (int)(LG_SCROLL * pnlBackSurface.Width);
                HScroll1.SmallChange = (int)(SM_SCROLL * pnlBackSurface.Width);
                SVMAX = r.Width - pnlBackSurface.Width + (VScroll1.Visible ? VScroll1.Width : 0);
                if (SVMAX < 0) {
                    tempscroll = true;
                    if (r.X < 0) {
                        SVMAX = -r.X;
                    }
                    else if (r.Right > pnlBackSurface.Width) {
                        SVMAX = r.Right - pnlBackSurface.Width;
                    }
                    else {
                        SVMAX = 0;
                    }
                }
                else {
                    tempscroll = false;
                    if (r.Right < pnlBackSurface.Width - (VScroll1.Visible ? VScroll1.Width : 0)) {
                        SVMAX += pnlBackSurface.Width - r.Right;
                    }
                }
                HScroll1.Minimum = 0;
                HScroll1.Maximum = SVMAX + HScroll1.LargeChange - 1;
                if (tempscroll) {
                    if (r.X < 0) {
                        HScroll1.Value = -r.X;
                    }
                    else if (r.Right > pnlBackSurface.Width) {
                        HScroll1.Value = 0;
                    }
                    else {
                        HScroll1.Minimum = -r.X;
                        HScroll1.Value = -r.X;
                    }
                }
                else {
                    if (r.X > 0) {
                        HScroll1.Minimum = -r.X;
                    }
                    HScroll1.Value = -r.X;
                }
            }
            if (VScroll1.Visible) {
                VScroll1.Maximum = r.Height * 2;
                VScroll1.LargeChange = (int)(LG_SCROLL * pnlBackSurface.Height);
                VScroll1.SmallChange = (int)(SM_SCROLL * pnlBackSurface.Height);
                SVMAX = r.Height - pnlBackSurface.Height + (HScroll1.Visible ? HScroll1.Height : 0);
                if (SVMAX < 0) {
                    tempscroll = true;
                    if (r.Y < 0) {
                        SVMAX = -r.Y;
                    }
                    else if (r.Bottom > pnlBackSurface.Height) {
                        SVMAX = r.Bottom - pnlBackSurface.Height;
                    }
                    else {
                        SVMAX = 0;
                    }
                }
                else {
                    tempscroll = false;
                    if (r.Bottom < pnlBackSurface.Height - (HScroll1.Visible ? HScroll1.Height : 0)) {
                        SVMAX += pnlBackSurface.Height - r.Bottom;
                    }
                }
                VScroll1.Minimum = 0;
                VScroll1.Maximum = SVMAX + VScroll1.LargeChange - 1;
                if (tempscroll) {
                    if (r.Y < 0) {
                        VScroll1.Value = -r.Y;
                    }
                    else if (r.Bottom > pnlBackSurface.Height) {
                        VScroll1.Value = 0;
                    }
                    else {
                        VScroll1.Minimum = -r.Y;
                        VScroll1.Value = -r.Y;
                    }
                }
                else {
                    if (r.Y > 0) {
                        VScroll1.Minimum = -r.Y;
                    }
                    VScroll1.Value = -r.Y;
                }
            }
            pnlBackSurface.Invalidate();
            SendMessage(picExample.Handle, WM_SETREDRAW, false, 0);
            picBackground.Invalidate();
            SendMessage(picExample.Handle, WM_SETREDRAW, true, 0);
            picExample.Refresh();
        }
    }

    public class TransparentPictureBox : PictureBox {
        private const int WS_EX_TRANSPARENT = 0x20;
        private int opacity = 50;
        bool dont = false;
        private Pen dash1 = new(Color.Black);
        private Pen dash2 = new(Color.White);
        private int dashdistance = 6; 
        private Timer tmrDash = new();
        public TransparentPictureBox() {
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            dash1.DashPattern = [3, 3];
            dash2.DashPattern = [3, 3];
            dash2.DashOffset = 3;
            tmrDash.Interval = 100;
            tmrDash.Tick += tmrDash_Tick;
            tmrDash.Start();
        }

        private void tmrDash_Tick(object sender, EventArgs e) {
            dashdistance -= 1;
            if (dashdistance == 0) dashdistance = 6;
            dash1.DashOffset = dashdistance;
            dash2.DashOffset = dashdistance - 3;
            Rectangle r = ClientRectangle;
                r.Width -= 1;
                r.Height -= 1;
            //_ = SendMessage(this.Handle, WM_SETREDRAW, false, 0);
            CreateGraphics().DrawRectangle(dash1, r);
            CreateGraphics().DrawRectangle(dash2, r);
            //_ = SendMessage(this.Handle, WM_SETREDRAW, true, 0);
        }

        [DefaultValue(50)]
        public int Opacity {
            get { return this.opacity; }
            set {
                if (value < 0 || value > 100)
                    throw new ArgumentException("value must be between 0 and 100");
                this.opacity = value;
                // Ensure the control is redrawn when opacity changes
                Invalidate();
            }
        }

        [DefaultValue(100)]
        public int DashInterval {
            get { return tmrDash.Interval; }
            set { tmrDash.Interval = value; Invalidate(); }
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cpar = base.CreateParams;
                cpar.ExStyle = cpar.ExStyle | WS_EX_TRANSPARENT;
                return cpar;
            }
        }

        protected override void Dispose(bool disposing) {
            tmrDash?.Stop();
            tmrDash?.Dispose();
            dash1?.Dispose();
            dash2?.Dispose();
            base.Dispose(disposing);    
        }

        protected override void OnPaint(PaintEventArgs e) {
            SendMessage(this.Handle, WM_SETREDRAW, false, 0);
            // this method recurses several times before finishing for
            // some reason; to prevent it, use a flag
            if (Parent != null && !dont) {
                dont = true;
                // Draw the parent control's background onto this control
                using (var bmp = new Bitmap(Parent.ClientSize.Width, Parent.ClientSize.Height)) {
                    Parent.DrawToBitmap(bmp, Parent.ClientRectangle);
                    e.Graphics.DrawImage(bmp, -Left, -Top);
                }
                dont = false;
            }
            //else {
            //    Debug.Print("DONT!");
            //}
            // Draw the control's background with the specified opacity
            using (var brush = new SolidBrush(Color.FromArgb(this.opacity * 255 / 100, this.BackColor))) {
                //using (var brush = new SolidBrush(Color.FromArgb(255, this.BackColor))) {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            Rectangle r = ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;
            CreateGraphics().DrawRectangle(dash1, r);
            CreateGraphics().DrawRectangle(dash2, r);
            base.OnPaint(e);
            SendMessage(this.Handle, WM_SETREDRAW, true, 0);
        }

        protected override void OnParentChanged(EventArgs e) {
            base.OnParentChanged(e);
            if (Parent != null) {
                Parent.Invalidate();
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            Invalidate();
        }
    }
}
