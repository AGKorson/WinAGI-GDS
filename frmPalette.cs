using System;
using System.Drawing;
using System.Windows.Forms;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using System.IO;

namespace WinAGI.Editor {
    public partial class frmPalette : Form {
        private readonly int FormMode; // 0 = palette mode; 1 = prevwin bkgd color mode
        private readonly EGAColors SierraColors = new();
        private readonly string[] strColName = new string[16];
        private readonly Color[] lngTempCol = new Color[16];
        public Color SelColor;
        private int SelColorIndex;
        private string PaletteFileName = "";

        public frmPalette(int mode) {
            InitializeComponent();
            FormMode = mode;
            switch (FormMode) {
            case 0:
                if (EditGame != null) {
                    Text = "Modify Color Palette for this Game";
                    label1.Text = "Default:";
                }
                else {
                    Text = "Modify Default Color Palette";
                    label1.Text = "Original:";
                    cmdDefColors.Text = "Restore Original Sierra Palette";
                }
                break;
            case 1:
                // set form for choosing editor background
                Text = "New Background Color";
                Width = 232;
                Height = 342;
                label1.Visible = false;
                picColChange.Visible = false;
                cmdColorDlg.Visible = false;
                lblCurColor.Visible = false;
                cmdDefColors.Location = new Point(8, 217);
                cmdDefColors.Text = "Reset to Default";
                SelColor = SystemColors.Control;
                break;
            }
            for (int i = 0; i < 16; i++) {
                if (EditGame != null) {
                    lngTempCol[i] = EditGame.Palette[i];
                }
                else {
                    lngTempCol[i] = DefaultPalette[i];

                }
                strColName[i] = i + ": " + Editor.Base.LoadResString(COLORNAME + i);
            }
        }

        #region Event Handlers
        private void cmdLoad_Click(object sender, EventArgs e) {
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
                DefaultResDir = JustPath(PaletteFileName);
            }
        }

        private void cmdSave_Click(object sender, EventArgs e) {
            MDIMain.SaveDlg.Title = "Save/Update Palette File";
            MDIMain.SaveDlg.DefaultExt = "ini";
            MDIMain.SaveDlg.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*";
            MDIMain.SaveDlg.FilterIndex = 1;
            MDIMain.SaveDlg.FileName = PaletteFileName;
            if (PaletteFileName == "") {
                MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            }
            else {
                MDIMain.SaveDlg.InitialDirectory = JustPath(PaletteFileName);
            }
            if (MDIMain.SaveDlg.ShowDialog() == DialogResult.OK) {
                if (SavePalette(MDIMain.SaveDlg.FileName)) {
                    PaletteFileName = MDIMain.SaveDlg.FileName;
                    DefaultResDir = JustPath(PaletteFileName);
                }
            }
        }

        private void cmdDefColors_Click(object sender, EventArgs e) {
            switch (FormMode) {
            case 0:
                // editing palette; reset to current defaults
                for (int i = 0; i < 16; i++) {
                    if (EditGame == null) {
                        lngTempCol[i] = SierraColors[i];
                    }
                    else {
                        lngTempCol[i] = DefaultPalette[i];
                    }
                }
                picPalette.Invalidate();
                picColChange.Invalidate();
                cmdOK.Enabled = true;
                break;
            case 1:
                SelColor = SystemColors.Control;
                this.DialogResult = DialogResult.OK;
                this.Hide();
                break;
            }
        }

        private void cmdColorDlg_Click(object sender, EventArgs e) {
            cdColors.Color = lngTempCol[SelColorIndex];
            cdColors.FullOpen = true;
            if (cdColors.ShowDialog() == DialogResult.OK) {
                lngTempCol[SelColorIndex] = cdColors.Color;
                picPalette.Invalidate();
                picColChange.Invalidate();
            }
            picPalette.Select();
        }

        private void cmdOK_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Hide();
            switch (FormMode) {
            case 0:
                // palette change mode
                for (int i = 0; i < 16; i++) {
                    if (EditGame != null) {
                        EditGame.Palette[i] = lngTempCol[i];
                        EditGame.WriteProperty("Palette", "Color" + i, EGAColors.ColorText(lngTempCol[i]));
                    }
                    else {
                        DefaultPalette[i] = lngTempCol[i];
                        WinAGISettingsFile.WriteSetting(sDEFCOLORS, "DefEGAColor" + i, DefaultPalette[i]);
                    }
                }
                break;
            case 1:
                // change a window background color
                break;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void picColChange_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            Color defColor;

            if (EditGame != null) {
                defColor = DefaultPalette[SelColorIndex];
            }
            else {
                defColor = SierraColors[SelColorIndex];
            }
            Color tempColor = lngTempCol[SelColorIndex];
            g.FillRectangle(new SolidBrush(defColor), 0, 0, picColChange.Width / 2, picColChange.Height);
            g.FillRectangle(new SolidBrush(tempColor), picColChange.Width / 2, 0, picColChange.Width / 2 + 30, picColChange.Height);
            g.DrawLine(Pens.Black, picColChange.Width / 2-1, 0, picColChange.Width / 2-1, picColChange.Height);
        }

        private void picPalette_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            for (int j = 0; j < 4; j++) {
                for (int i = 0; i < 4; i++) {
                    g.FillRectangle(new SolidBrush(lngTempCol[4 * j + i]), i * 47 + 5, j * 47 + 5, 42, 42);
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
                lblCurColor.Text = strColName[SelColorIndex];
            }
        }

        private void picPalette_DoubleClick(object sender, MouseEventArgs e) {
            if (e.X < 5 || e.Y < 5) {
                return;
            }
            float sngRow = (e.Y - 5) / 47f;
            float sngCol = (e.X - 5) / 47f;
            int intRow = (int)sngRow;
            int intCol = (int)sngCol;
            // either element > 42/47 means cursor must be inbetween boxes
            if ((sngRow - intRow) > 0.893617034 || (sngCol - intCol) > 0.893617034) {
                return;
            }
            if (intRow > 3 || intCol > 3) {
                return;
            }
            switch (FormMode) {
            case 0:
                // in palette mode, edit this color
                cmdColorDlg_Click(sender, null);
                break;
            case 1:
                // in bkgd color mode, select and close
                cmdOK_Click(sender, null);
                break;
            }
        }

        private void picPalette_MouseDown(object sender, MouseEventArgs e) {
            if (e.X < 5 || e.Y < 5) {
                return;
            }
            float sngRow = (e.Y - 5) / 47f;
            float sngCol = (e.X - 5) / 47f;
            int intRow = (int)sngRow;
            int intCol = (int)sngCol;
            // either element > 42/47 means cursor must be inbetween boxes
            if ((sngRow - intRow) > 0.893617034 || (sngCol - intCol) > 0.893617034) {
                return;
            }
            if (intRow > 3 || intCol > 3) {
                return;
            }
            // select the color, and update the palette
            SelColorIndex = intRow * 4 + intCol;
            SelColor = lngTempCol[SelColorIndex];
            picPalette.Invalidate();
            picColChange.Invalidate();
            cmdOK.Enabled = true;
            cmdColorDlg.Enabled = true;
        }
        #endregion

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
            catch (Exception ex) {
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
                            lngTempCol[i] = Color.FromArgb(
                                Convert.ToInt32(strValues[0], 16),
                                Convert.ToInt32(strValues[1], 16),
                                Convert.ToInt32(strValues[2], 16));
                        }
                        catch (Exception ex) {
                            // ignore
                        }
                    }
                }
            }
            // force update of color grid
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
                catch (Exception ex) {
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
                catch (Exception ex) {
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
                    "0x" + lngTempCol[i].R.ToString("x2") + "," + 
                    "0x" + lngTempCol[i].G.ToString("x2") + "," +
                    "0x" + lngTempCol[i].B.ToString("x2"));
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
                    "",
                    "Save Palette Error");
                return false;
            }
            return true;
        }
    }
}
