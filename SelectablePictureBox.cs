using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WinAGI.Editor {
    /// <summary>
    /// A picturebox control that can be selected and shows a focus rectangle when focused.
    /// It also handles keyboard events.
    /// </summary>
    public class SelectablePictureBox : PictureBox {
        private bool showfocus = true;

        public SelectablePictureBox() {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("Indicates whether the focus rectangle is shown when the control is focused.")]
        [DefaultValue(true)]
        public bool ShowFocusRectangle {
            get { return showfocus; }
            set { showfocus = value; }
        }

        protected override bool IsInputKey(Keys keyData) {
            if ((keyData & Keys.Up) == Keys.Up || (keyData & Keys.Down) == Keys.Down) return true;
            if ((keyData & Keys.Left) == Keys.Left || (keyData & Keys.Up) == Keys.Right) return true;
            if (keyData == Keys.Tab) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnEnter(EventArgs e) {
            Invalidate();
            base.OnEnter(e);
            Enter?.Invoke(this, e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            Select();
            base.OnMouseDown(e);
        }

        protected override void OnLeave(EventArgs e) {
            Invalidate();
            base.OnLeave(e);
            Leave?.Invoke(this, e);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            KeyDown?.Invoke(this, e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            base.OnKeyPress(e);
            KeyPress?.Invoke(this, e);
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);
            KeyUp?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            if (showfocus && Focused) {
                System.Drawing.Rectangle rc = ClientRectangle;
                rc.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(pe.Graphics, rc);
            }
        }

        // events
        [Category("Focus")]
        [Description("Occurs when the control becomes the active control of the form.")]
        public new event EventHandler Enter;
        [Category("Focus")]
        [Description("Occurs when the control is no longer the active control of the form.")]
        public new event EventHandler Leave;
        [Category("Key")]
        [Description("Occurs when a key is first pressed.")]
        public new event KeyEventHandler KeyDown;
        [Category("Key")]
        [Description("Occurs when the control has focus and the user presses and releases a key.")]
        public new event KeyPressEventHandler KeyPress;
        [Category("Key")]
        [Description("Occurs when a key is released.")]
        public new event KeyEventHandler KeyUp;
    }
}
