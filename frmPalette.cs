using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    public partial class frmPalette : Form {
        #region Fields
        private readonly int FormMode; // 0 = palette mode; 1 = prevwin bkgd color mode
        private readonly EGAColors SierraColors = new();
        private readonly string[] ColorNames = new string[16];
        private readonly Color[] TempColors = new Color[16];
        public Color SelColor;
        private int SelColorIndex;
        private string PaletteFileName = "";
        #endregion

        #region Constructors
        public frmPalette(int mode) {
            InitializeComponent();
            btnCancel.ContextMenuStrip = new();
            btnColorDlg.ContextMenuStrip = new();
            btnDefColors.ContextMenuStrip = new();
            btnLoad.ContextMenuStrip = new();
            btnOK.ContextMenuStrip = new();
            btnSave.ContextMenuStrip = new();
            picColChange.ContextMenuStrip = new();
            picPalette.ContextMenuStrip = new();

            FormMode = mode;
            switch (FormMode) {
            case 0:
                if (EditGame is not null) {
                    Text = "Modify Color Palette for this Game";
                    label1.Text = "Default:";
                    if (EditGame.PowerPack) {
                        ContextMenuStrip = contextMenuStrip1;
                    }
                }
                else {
                    Text = "Modify Default Color Palette";
                    label1.Text = "Original:";
                    btnDefColors.Text = "Restore Original Sierra Palette";
                }
                break;
            case 1:
                // set form for choosing editor background
                Text = "New Background Color";
                Width = 232;
                Height = 342;
                label1.Visible = false;
                picColChange.Visible = false;
                btnColorDlg.Visible = false;
                lblCurColor.Visible = false;
                lbl18Bit.Visible = false;
                lbl24Bit.Visible = false;
                txt18Bit.Visible = false;
                txt24Bit.Visible = false;
                btnDefColors.Location = new Point(8, 217);
                btnDefColors.Text = "Reset to Default";
                SelColor = SystemColors.Control;
                break;
            }
            for (int i = 0; i < 16; i++) {
                if (EditGame is not null) {
                    TempColors[i] = EditGame.Palette[i];
                }
                else {
                    TempColors[i] = DefaultPalette[i];

                }
                ColorNames[i] = i + ": " + EditorResourceByNum(COLORNAME + i);
            }
            btnColorDlg.Enabled = true;
            UpdateColorValues();
        }
        #endregion

        #region Event Handlers
        private void frmPalette_KeyDown(object sender, KeyEventArgs e) {
            // check for Ctrl+C to copy current color to clipboard
            if (e.Control && e.KeyCode == Keys.C &&
                EditGame is not null && EditGame.PowerPack) {
                string palette = "";
                for (int i = 0; i < 16; i++) {
                    palette += ToEgaHex(TempColors[i]);
                }
                Clipboard.SetText(palette);
            }
        }

        private void frmPalette_HelpRequested(object sender, HelpEventArgs hlpevent) {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\palette.htm");
            hlpevent.Handled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            var menu = (ContextMenuStrip)sender;
            if (menu.SourceControl != this) {
                e.Cancel = true;
            }
        }

        private void mnuCopy_Click(object sender, EventArgs e) {
            string palette = "";
            for (int i = 0; i < 16; i++) {
                palette += ToEgaHex(TempColors[i]);
            }
            Clipboard.SetText(palette);
        }

        private void mnuPaste_Click(object sender, EventArgs e) {
            string palette = Clipboard.GetText();
            if (palette.Length == 98 && palette[0] == '\"' && palette[^1] == '\"') {
                palette = palette[1..^1];
            }
            else if (palette.Length != 96) {
                return;
            }
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++) {
                string colorhex = palette.Substring(i * 6, 6);
                colors[i] = FromEgaHex(colorhex);
                if (colors[i] == Color.Empty) {
                    // no good, ignore
                    return;
                }
            }
            // colors ok
            colors.CopyTo(TempColors, 0);
            // force update of color grid
            UpdateColorValues();
            picPalette.Invalidate();
            picColChange.Invalidate();
        }

        private void btnLoad_Click(object sender, EventArgs e) {
            MDIMain.OpenDlg.ShowReadOnly = false;
            MDIMain.OpenDlg.CheckFileExists = true;
            MDIMain.OpenDlg.Title = "Open Palette File";
            MDIMain.OpenDlg.DefaultExt = "ini";
            MDIMain.OpenDlg.Filter = "INI files (*.ini)|*.ini|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            MDIMain.OpenDlg.FilterIndex = 1;
            MDIMain.OpenDlg.FileName = "";
            MDIMain.OpenDlg.InitialDirectory = DefaultResDir;
            if (MDIMain.OpenDlg.ShowDialog() == DialogResult.OK) {
                PaletteFileName = MDIMain.OpenDlg.FileName;
                LoadPalette(PaletteFileName);
                DefaultResDir = Path.GetDirectoryName(PaletteFileName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            MDIMain.SaveDlg.Title = "Save/Update Palette File";
            MDIMain.SaveDlg.DefaultExt = "ini";
            MDIMain.SaveDlg.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*";
            MDIMain.SaveDlg.FilterIndex = 1;
            MDIMain.SaveDlg.FileName = PaletteFileName;
            if (PaletteFileName == "") {
                MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            }
            else {
                MDIMain.SaveDlg.InitialDirectory = Path.GetDirectoryName(PaletteFileName);
            }
            if (MDIMain.SaveDlg.ShowDialog() == DialogResult.OK) {
                if (SavePalette(MDIMain.SaveDlg.FileName)) {
                    PaletteFileName = MDIMain.SaveDlg.FileName;
                    DefaultResDir = Path.GetDirectoryName(PaletteFileName);
                }
            }
        }

        private void btnDefColors_Click(object sender, EventArgs e) {
            switch (FormMode) {
            case 0:
                // editing palette; reset to current defaults
                for (int i = 0; i < 16; i++) {
                    if (EditGame is null) {
                        TempColors[i] = SierraColors[i];
                    }
                    else {
                        TempColors[i] = DefaultPalette[i];
                    }
                }
                UpdateColorValues();
                picPalette.Invalidate();
                picColChange.Invalidate();
                btnOK.Enabled = true;
                break;
            case 1:
                SelColor = SystemColors.Control;
                DialogResult = DialogResult.OK;
                Hide();
                break;
            }
        }

        private void btnColorDlg_Click(object sender, EventArgs e) {
            cdColors.Color = TempColors[SelColorIndex];
            cdColors.FullOpen = true;
            if (cdColors.ShowDialog() == DialogResult.OK) {
                TempColors[SelColorIndex] = cdColors.Color;
                UpdateColorValues();
                picPalette.Invalidate();
                picColChange.Invalidate();
            }
            picPalette.Select();
        }

        private void btnOK_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Hide();
            switch (FormMode) {
            case 0:
                // palette change mode
                for (int i = 0; i < 16; i++) {
                    if (EditGame is not null) {
                        EditGame.Palette[i] = TempColors[i];
                        EditGame.WriteGameSetting("Palette", "Color" + i, TempColors[i].ColorText());
                    }
                    else {
                        DefaultPalette[i] = TempColors[i];
                        WinAGISettingsFile.WriteSetting(sDEFCOLORS, "DefEGAColor" + i, DefaultPalette[i]);
                    }
                }
                break;
            case 1:
                // change a window background color
                break;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void picColChange_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            Color defColor;

            if (EditGame is not null) {
                defColor = DefaultPalette[SelColorIndex];
            }
            else {
                defColor = SierraColors[SelColorIndex];
            }
            Color tempColor = TempColors[SelColorIndex];
            g.FillRectangle(new SolidBrush(defColor), 0, 0, picColChange.Width / 2, picColChange.Height);
            g.FillRectangle(new SolidBrush(tempColor), picColChange.Width / 2, 0, picColChange.Width / 2 + 30, picColChange.Height);
            g.DrawLine(Pens.Black, picColChange.Width / 2 - 1, 0, picColChange.Width / 2 - 1, picColChange.Height);
        }

        private void picPalette_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            for (int j = 0; j < 4; j++) {
                for (int i = 0; i < 4; i++) {
                    g.FillRectangle(new SolidBrush(TempColors[4 * j + i]), i * 47 + 5, j * 47 + 5, 42, 42);
                    // if this is the selected color, then highlight it
                    if (SelColorIndex == 4 * j + i) {
                        Pen p = new(Color.Black, 2) {
                            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                        };
                        g.DrawRectangle(p, i * 47 + 5, j * 47 + 5, 42, 42);
                    }
                }
            }
            if (FormMode == 0) {
                lblCurColor.Text = ColorNames[SelColorIndex];
            }
        }

        private void picPalette_DoubleClick(object sender, MouseEventArgs e) {
            // the palette is a 4x4 grid of 42x42 pixel boxes, with a 5 pixel
            // border around the whole thing, and between box edges

            // only interested in double clicks that are within a box

            // first check if the click is within 5 pixels of the left/top
            // edge; if so, ignore it
            if (e.X < 5 || e.Y < 5) {
                return;
            }
            // calculate row and column values, don't worry about the gaps yet
            int row = (int)((e.Y - 5) / 47f);
            int col = (int)((e.X - 5) / 47f);
            // if past right/bottom, ignore
            if (row > 3 || col > 3) {
                return;
            }
            // now determine if on a gap or not
            int edgebtm = (row + 1) * 47; // (+5 for margin, -5 to account for last gap)
            int edgeright = (col + 1) * 47;
            if (e.Y > edgebtm || e.X > edgeright) {
                return;
            }
            switch (FormMode) {
            case 0:
                // in palette mode, edit this color
                btnColorDlg_Click(sender, null);
                break;
            case 1:
                // in bkgd color mode, select and close
                btnOK_Click(sender, null);
                break;
            }
        }

        private void picPalette_MouseDown(object sender, MouseEventArgs e) {
            // only interested in clicks that are within a box

            // first check if the click is within 5 pixels of the left/top
            // edge; if so, ignore it
            if (e.X < 5 || e.Y < 5) {
                return;
            }
            // calculate row and column values, don't worry about the gaps yet
            int row = (int)((e.Y - 5) / 47f);
            int col = (int)((e.X - 5) / 47f);
            // if past right/bottom, ignore
            if (row > 3 || col > 3) {
                return;
            }
            // now determine if on a gap or not
            int edgebtm = (row + 1) * 47; // (+5 for margin, -5 to account for last gap)
            int edgeright = (col + 1) * 47;
            if (e.Y > edgebtm || e.X > edgeright) {
                return;
            }

            // select the color, and update the palette
            SelColorIndex = row * 4 + col;
            SelColor = TempColors[SelColorIndex];
            UpdateColorValues();
            picPalette.Invalidate();
            picColChange.Invalidate();
            btnOK.Enabled = true;
            btnColorDlg.Enabled = true;
        }

        private void picColChange_DoubleClick(object sender, EventArgs e) {
            if (FormMode == 0) {
                // in palette mode, edit this color
                btnColorDlg_Click(sender, null);
            }
        }
        #endregion

        #region Methods
        private void LoadPalette(string LoadFile) {
            // opens the load file, and finds the palette section
            // file should be NAGI.INI compatible, which is similar to
            // WinAGI settings file format except comments use ';'
            // instead of '#'
            // note the spelling of 'colour' is British English; this
            // is how NAGI does it
            SettingsFile paletteINI;
            try {
                paletteINI = new SettingsFile(LoadFile, FileMode.Open);
            }
            catch (Exception) {
                MessageBox.Show("Unable to open this file. Try another one.", "Load Palette Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool nagifile = paletteINI.FindSettingSection("nagi") != -1;
            for (int i = 0; i < 16; i++) {
                string color = paletteINI.GetSetting("palette", "colour" + i, "", true);
                if (color != "") {
                    string[] strValues = color.Split(',');
                    if (strValues.Length == 3) {
                        try {
                            TempColors[i] = Color.FromArgb(
                                Convert.ToInt32(strValues[0], 16),
                                Convert.ToInt32(strValues[1], 16),
                                Convert.ToInt32(strValues[2], 16));
                        }
                        catch (Exception) {
                            // ignore
                        }
                    }
                }
            }
            // force update of color grid
            UpdateColorValues();
            picPalette.Invalidate();
            picColChange.Invalidate();
        }

        private bool SavePalette(string SaveFile) {
            // file will be NAGI.INI compatible, which is similar to
            // WinAGI settings file format except comments use ';'
            // instead of '#'
            // note the spelling of 'colour' is British English; this
            // is how NAGI does it
            SettingsFile paletteINI;
            bool newfile = !File.Exists(SaveFile);
            bool nagifile;
            if (newfile) {
                try {
                    paletteINI = new SettingsFile(SaveFile, FileMode.Create);
                }
                catch (Exception) {
                    MessageBox.Show(MDIMain,
                        "Unable to create this palette file.",
                        "Save Palette Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }
                nagifile = false;
            }
            else {
                try {
                    paletteINI = new SettingsFile(SaveFile, FileMode.Open);
                }
                catch (Exception) {
                    MessageBox.Show(MDIMain,
                        "Unable to open this palette file.",
                        "Save Palette Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }
                nagifile = paletteINI.FindSettingSection("nagi") != -1;
            }
            if (paletteINI.FindSettingSection("palette") != -1) {
                paletteINI.DeleteSection("palette");
            }
            for (int i = 0; i < 16; i++) {
                paletteINI.WriteSetting("palette", "colour" + i,
                    "0x" + TempColors[i].R.ToString("x2") + "," +
                    "0x" + TempColors[i].G.ToString("x2") + "," +
                    "0x" + TempColors[i].B.ToString("x2"));
            }
            if (nagifile) {
                // add comments
                int line = paletteINI.FindSettingSection("palette") + 1;
                paletteINI.Lines.Insert(line++, ";palette option allows you to change the colors used by NAGI");
                paletteINI.Lines.Insert(line++, ";default colors are used if a color is not defined");
                paletteINI.Lines.Insert(line++, ";define colors as three hexadecimal values, representing");
                paletteINI.Lines.Insert(line++, ";red, green, blue components");
            }
            try {
                paletteINI.Save();
            }
            catch (Exception ex) {
                ErrMsgBox(ex,
                    "Unable to save this palette file due to file access error:",
                    ex.StackTrace,
                    "Save Palette Error");
                return false;
            }
            return true;
        }

        private void UpdateColorValues() {
            txt24Bit.Text = ToRGBHex(TempColors[SelColorIndex]);
            txt18Bit.Text = ToEgaHex(TempColors[SelColorIndex]);
        }

        private static string ToRGBHex(Color c) {
            return $"{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        private static string ToEgaHex(Color c) {
            int r = c.R * 63 / 255;
            int g = c.G * 63 / 255;
            int b = c.B * 63 / 255;

            return $"{r:X2}{g:X2}{b:X2}";
        }

        public static Color FromEgaHex(string hex) {
            if (hex.Length != 6) {
                return Color.Empty;
            }
            try {
                int r6 = Convert.ToInt32(hex[..2], 16);
                int g6 = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b6 = Convert.ToInt32(hex.Substring(4, 2), 16);
                if (r6 > 0x3F || g6 > 0x3F || b6 > 0x3F) {
                    return Color.Empty;
                }
                // Scale 0–63 back to 0–255
                int r = r6 * 255 / 63;
                int g = g6 * 255 / 63;
                int b = b6 * 255 / 63;

                return Color.FromArgb(r, g, b);
            }
            catch {
                return Color.Empty;
            }
        }
        #endregion
    }
}
