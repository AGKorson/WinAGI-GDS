using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinAGI.Editor {
    public partial class frmProgress : Form {
        public frmProgress(Form owner) {
            InitializeComponent();
            Owner = owner;

            if (owner is null) {
                StartPosition = FormStartPosition.CenterScreen;
            }
            else {
                Point offset = new() {
                    X = owner.Width / 2 - Width / 2,
                    Y = owner.Height / 2 - Height / 2
                };
                Point pos = new();
                // child form?
                if (owner.IsMdiChild) {
                    // extracting actual position of form is not easy- 
                    // PointToScreen on the form gives the position of the client area
                    // not the form, so we have to account for the left and top borders
                    // BUT there is no way to get that easily; we use the difference
                    // between client size and form size and make some assumptions...

                    // if we assume bottom border is same as right/left; then
                    // rightborder = leftborder = (frm.Width - client.Width) / 2
                    // topborder = frm.Width - client.Width - rightborder
                    //
                    // this doesn't work though - instead, rightborder needs to be
                    // frm.Width - client.Width (full amount, not halved)
                    // and topborder = 1/2 of rightborder
                    //
                    // NO IDEA why this is so- but it works.
                    // 
                    Point childoffset = new() {
                        X = -(owner.Width - owner.ClientSize.Width), // / 2
                    };
                    childoffset.Y = -(owner.Height - owner.ClientSize.Height + childoffset.X / 2);
                    pos = owner.PointToScreen(childoffset);
                }
                else {
                    pos.X = owner.Left;
                    pos.Y = owner.Top;
                }
                // adjust pos by offset to center the form
                pos.Offset(offset);
                Location = pos;
                Size = new(278, 128);
            }
        }
    }

    public class Win11ProgressBar : ProgressBar {
        public Color BarColor { get; set; } = Color.FromArgb(0, 120, 215); // Win11 accent blue
        public Color BackgroundColor { get; set; } = Color.FromArgb(230, 230, 230);
        public int CornerRadius { get; set; } = 6;

        public Win11ProgressBar() {
            SetStyle(ControlStyles.UserPaint, true);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;

            // Background
            using (SolidBrush bg = new(BackgroundColor))
            using (GraphicsPath bgPath = RoundedRect(rect, CornerRadius))
                e.Graphics.FillPath(bg, bgPath);

            // Progress width
            float percent = (float)Value / Maximum;
            int progressWidth = (int)(rect.Width * percent);
            if (progressWidth <= 0)
                return;

            Rectangle progressRect = new(rect.X, rect.Y, progressWidth, rect.Height);

            // Progress fill
            using SolidBrush bar = new(BarColor);
            using GraphicsPath barPath = RoundedRect(progressRect, CornerRadius);
            e.Graphics.FillPath(bar, barPath);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius) {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            if (radius == 0) {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
