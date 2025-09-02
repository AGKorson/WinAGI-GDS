using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.frmPicEdit;

namespace WinAGI.Editor {
    public partial class frmPlotEdit : Form {
        internal Point NewCoord;
        internal byte NewPattern = 0;
        private CommandInfo SelCmd;
        private Picture EditPic;
        private bool stepDraw;
        private int TransLevel = 255;
        private bool TransDir = false;
        private byte[] CircleData =
            [0x80, 0xC0, 0xC0, 0xC0, 0x40, 0xE0, 0xE0, 0xE0,
             0x40, 0x60, 0x60, 0xF0, 0xF0, 0xF0, 0x60, 0x60,
             0x20, 0x70, 0xF8, 0xF8, 0xF8, 0xF8, 0xF8, 0x70,
             0x20, 0x30, 0x78, 0x78, 0x78, 0xFC, 0xFC, 0xFC,
             0x78, 0x78, 0x78, 0x30, 0x38, 0x7C, 0x7C, 0x7C,
             0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0x7C, 0x7C, 0x7C,
             0x38, 0x18, 0x3C, 0x7E, 0x7E, 0x7E, 0xFF, 0xFF,
             0xFF, 0xFF, 0xFF, 0x7E, 0x7E, 0x7E, 0x3C, 0x18];
        private Color PenColor;

        public frmPlotEdit(frmPicEdit.CommandInfo selectedcmd, Picture editpic) {
            InitializeComponent();

            SelCmd = selectedcmd;
            NewCoord = SelCmd.SelectedCoord;
            txtX.Text = NewCoord.X.ToString();
            txtY.Text = NewCoord.Y.ToString();

            if (SelCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                // adjust picture so it doesn't show this plot point
                EditPic = editpic;
                editpic.DrawPos -= 1;
                stepDraw = editpic.StepDraw;
                // set initial pattern
                NewPattern = editpic.Data[SelCmd.SelectedCoordPos];
                udPattern.Value = NewPattern / 2;
                if (SelCmd.Pen.VisColor == AGIColorIndex.None && SelCmd.Pen.PriColor == AGIColorIndex.None) {
                    // no pen active; nothing to display on coordinate editor
                }
                else if (SelCmd.Pen.VisColor == AGIColorIndex.None) {
                    PenColor = EditPic.Palette[(int)SelCmd.Pen.PriColor];
                }
                else {
                    PenColor = EditPic.Palette[(int)SelCmd.Pen.VisColor];
                }
                // draw image background
                SetImage();
                timer1.Enabled = true;
                // There is a bug in AGI that uses 160 as edge limit for
                // plotting; this causes pixels to wrap around to next
                // row. There are a few original Sierra games that have
                // this bug, so user is warned if this is the case.
                lblWarning.Visible = NewCoord.X == 160 - SelCmd.Pen.PlotSize / 2;
            }
            else {
                // compress form to only show coordinates
                Height = 115;
                picPlot.Visible = false;
                udPattern.Visible = false;
            }
        }

        #region Event Handlers
        private void Control_Enter(object sender, EventArgs e) {
            CancelButton = null;
            AcceptButton = null;
        }

        private void Control_Leave(object sender, EventArgs e) {
            CancelButton = btnCancel;
            AcceptButton = btnOK;
        }

        private void btnOK_Click(object sender, EventArgs e) {
            if (SelCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                EditPic.DrawPos += 1;
                EditPic.StepDraw = stepDraw;
            }
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            if (SelCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                EditPic.DrawPos += 1;
                EditPic.StepDraw = stepDraw;
            }
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void txtX_Enter(object sender, EventArgs e) {
            CancelButton = null;
            AcceptButton = null;
        }

        private void txtX_Leave(object sender, EventArgs e) {
            CancelButton = btnCancel;
            AcceptButton = btnOK;
        }

        private void txtX_Validating(object sender, CancelEventArgs e) {
            if (txtX.Text.Length == 0) {
                txtX.Value = txtX.MinValue;
            }
            int newval = txtX.Value;
            if (SelCmd.Type == DrawFunction.PlotPen) {
                if (newval < (SelCmd.Pen.PlotSize + 1) / 2) {
                    newval = (SelCmd.Pen.PlotSize + 1) / 2;
                }
                // should i allow intentional use of the '160' bug?
                // probably not.....
                if (newval > 159 - SelCmd.Pen.PlotSize / 2) {
                    newval = 159 - SelCmd.Pen.PlotSize / 2;
                }
                txtX.Value = newval;
            }
            if (newval != NewCoord.X) {
                NewCoord.X = newval;
                if (SelCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    SetImage();
                }
            }
        }

        private void txtY_Enter(object sender, EventArgs e) {
            CancelButton = null;
            AcceptButton = null;
        }

        private void txtY_Leave(object sender, EventArgs e) {
            CancelButton = btnCancel;
            AcceptButton = btnOK;
        }

        private void txtY_Validating(object sender, CancelEventArgs e) {
            // only numeric entries allowed
            if (txtY.Text.Length == 0) {
                txtY.Value = txtY.MinValue;
            }
            int newval = txtY.Value;
            if (SelCmd.Type == DrawFunction.PlotPen) {
                if (newval < SelCmd.Pen.PlotSize) {
                    newval = SelCmd.Pen.PlotSize;
                }
                if (newval > 167 - SelCmd.Pen.PlotSize) {
                    newval = 167 - SelCmd.Pen.PlotSize;
                }
                txtY.Value = newval;
            }
            if (newval != NewCoord.Y) {
                NewCoord.Y = newval;
                if (SelCmd.Pen.PlotStyle == PlotStyle.Splatter) {
                    SetImage();
                }
            }
        }

        private void udPattern_ValueChanged(object sender, EventArgs e) {
            NewPattern = (byte)udPattern.Value;
            picPlot.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (TransLevel == 0 || TransLevel == 255) {
                TransDir = !TransDir;
            }
            if (TransDir) {
                TransLevel += 10;
                if (TransLevel > 255) {
                    TransLevel = 255;
                }
            }
            else {
                TransLevel -= 10;
                if (TransLevel < 0) {
                    TransLevel = 0;
                }
            }
            // redraw coordinate point
            picPlot.Refresh();
        }
        #endregion

        #region Methods
        private void SetImage() {
            // draw portion of source picture onto picPlot image
            if (picPlot.Image == null) {
                picPlot.Image = new Bitmap(150, 145);
            }
            Graphics g = Graphics.FromImage(picPlot.Image);
            if (SelCmd.Pen.VisColor == AGIColorIndex.None && SelCmd.Pen.PriColor == AGIColorIndex.None) {
                g.Clear(picPlot.BackColor);
                // with no pen, show warning to user
                g.Clear(Color.White);
                Font font = new(txtX.Font.Name, 14f);
                Brush brush = new SolidBrush(Color.Black);
                g.DrawString("NO ACTIVE", font, brush, new PointF(25f, 50f));
                g.DrawString("PEN", font, brush, new PointF(52f, 70f));
                picPlot.Refresh();
                return;
            }
            Point PlotPt = new(0, 0);
            PlotPt = NewCoord;
            PlotPt.Offset(-7, -14);

            g.Clear(picPlot.BackColor);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            Rectangle src = new(PlotPt, new(15, 29));
            Rectangle dest = new(0, 0, picPlot.Width, picPlot.Height);
            if (src.X < 0) {
                dest.X = -src.X * 10;
                src.X = 0;
            }
            if (src.Y < 0) {
                dest.Y = -src.Y * 5;
                src.Y = 0;
            }
            if (src.Right > 160) {
                dest.Width = (160 - src.X) * 10;
                src.Width = 160 - src.X;
            }
            if (src.Bottom > 167) {
                dest.Height = (167 - src.Y) * 5;
                src.Height = 167 - src.Y;
            }
            if (SelCmd.Pen.VisColor != AGIColorIndex.None) {
                g.DrawImage(EditPic.VisualBMP, dest, src, GraphicsUnit.Pixel);
            }
            else {
                g.DrawImage(EditPic.PriorityBMP, dest, src, GraphicsUnit.Pixel);
            }
            picPlot.Refresh();
        }

        private void DrawPlotPoint(Graphics g) {
            byte newpatt, oldpatt;
            byte pX, pY;
            Brush pb = new SolidBrush(Color.FromArgb(TransLevel, PenColor));
            Brush eb = new HatchBrush(HatchStyle.Percent50, Color.FromArgb(TransLevel, PenColor), Color.Transparent);

            // disable screen updating for the picture control until after point is plotted
            Common.API.SendMessage(picPlot.Handle, Common.API.WM_SETREDRAW, false, 0);

            // start in upper-right corner
            pX = (byte)(7 - (SelCmd.Pen.PlotSize + 1) / 2);
            pY = (byte)(13 - SelCmd.Pen.PlotSize);
            oldpatt = (byte)(NewPattern * 2 | 1);

            if (SelCmd.Pen.PlotShape == PlotShape.Circle) {
                for (byte Y = 0; Y <= SelCmd.Pen.PlotSize * 2; Y++) {
                    for (byte X = 0; X <= SelCmd.Pen.PlotSize; X++) {
                        if ((CircleData[SelCmd.Pen.PlotSize * SelCmd.Pen.PlotSize + Y] & (128 >> X)) > 0) {
                            newpatt = (byte)(oldpatt >> 1);
                            if (oldpatt % 2 == 1) {
                                newpatt ^= 0xB8;
                            }
                            oldpatt = newpatt;
                            if ((oldpatt & 3) == 2) {
                                if (X + pX + NewCoord.X - 7 == 160) {
                                    g.FillRectangle(eb, 10 * (X + pX), 5 * (Y + pY), 10, 5);
                                }
                                else {
                                    g.FillRectangle(pb, 10 * (X + pX), 5 * (Y + pY), 10, 5);
                                }
                            }
                        }
                    }
                }
            }
            else {
                for (int Y = 0; Y <= SelCmd.Pen.PlotSize * 2; Y++) {
                    for (int X = 0; Y <= SelCmd.Pen.PlotSize; X++) {
                        newpatt = (byte)(oldpatt >> 1);
                        if (oldpatt % 2 == 1) {
                            newpatt ^= 0xB8;
                        }
                        oldpatt = newpatt;
                        if ((oldpatt & 3) == 2) {
                            if (X + pX + NewCoord.X - 7 == 160) {
                                g.FillRectangle(eb, 10 * (X + pX), 5 * (Y + pY), 10, 5);
                            }
                            else {
                                g.FillRectangle(pb, 10 * (X + pX), 5 * (Y + pY), 10, 5);
                            }
                        }
                    }
                }
            }

            // reenable screen updating
            Common.API.SendMessage(picPlot.Handle, Common.API.WM_SETREDRAW, true, 0);
        }
        #endregion

        private void picPlot_Paint(object sender, PaintEventArgs e) {
            DrawPlotPoint(e.Graphics);
        }
    }
}
