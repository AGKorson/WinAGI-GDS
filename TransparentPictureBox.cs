using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static WinAGI.Common.API;

namespace WinAGI.Editor {
    public class TransparentPictureBox : PictureBox {
        #region Fields
        private const int WS_EX_TRANSPARENT = 0x20;
        private int opacity = 50;
        private bool dont = false;
        private readonly Pen dash1 = new(Color.Black);
        private readonly Pen dash2 = new(Color.White);
        private int dashdistance = 6;
        private readonly Timer tmrDash = new();
        #endregion

        #region Constructors
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
        #endregion

        #region Properties
        [DefaultValue(50)]
        public int Opacity {
            get {
                return opacity;
            }
            set {
                if (value < 0 || value > 100)
                    throw new ArgumentException("value must be between 0 and 100");
                opacity = value;
                // Ensure the control is redrawn when opacity changes
                Invalidate();
            }
        }

        [DefaultValue(100)]
        public int DashInterval {
            get {
                return tmrDash.Interval;
            }
            set {
                tmrDash.Interval = value;
                Invalidate();
            }
        }
        #endregion

        #region Methods
        private void tmrDash_Tick(object sender, EventArgs e) {
            dashdistance -= 1;
            if (dashdistance == 0)
                dashdistance = 6;
            dash1.DashOffset = dashdistance;
            dash2.DashOffset = dashdistance - 3;
            Rectangle r = ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;
            CreateGraphics().DrawRectangle(dash1, r);
            CreateGraphics().DrawRectangle(dash2, r);
        }
        #endregion

        #region Overrides
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
            SendMessage(Handle, WM_SETREDRAW, false, 0);
            // this method recurses several times before finishing for
            // some reason; to prevent it, use a flag
            if (Parent is not null && !dont) {
                dont = true;
                // Draw the parent control's background onto this control
                using (var bmp = new Bitmap(Parent.ClientSize.Width, Parent.ClientSize.Height)) {
                    Parent.DrawToBitmap(bmp, Parent.ClientRectangle);
                    e.Graphics.DrawImage(bmp, -Left, -Top);
                }
                dont = false;
            }
            // Draw the control's background with the specified opacity
            using (var brush = new SolidBrush(Color.FromArgb(this.opacity * 255 / 100, this.BackColor))) {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            Rectangle r = ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;
            CreateGraphics().DrawRectangle(dash1, r);
            CreateGraphics().DrawRectangle(dash2, r);
            base.OnPaint(e);
            SendMessage(Handle, WM_SETREDRAW, true, 0);
        }

        protected override void OnParentChanged(EventArgs e) {
            base.OnParentChanged(e);
            Parent?.Invalidate();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            Invalidate();
        }
        #endregion
    }
}
