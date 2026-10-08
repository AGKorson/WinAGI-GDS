using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WinAGI.Editor {
    /// <summary>
    /// A picturebox control that can be selected and shows a focus rectangle when focused.
    /// It also handles keyboard events.
    /// </summary>
    public class SelectablePictureBox : PictureBox {
        #region Fields
        private bool showfocus = true;
        #endregion

        #region Constructors
        public SelectablePictureBox() {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }
        #endregion

        #region Properties
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance")]
        [Description("Indicates whether the focus rectangle is shown when the control is focused.")]
        [DefaultValue(true)]
        public bool ShowFocusRectangle {
            get {
                return showfocus;
            }
            set {
                showfocus = value;
                Invalidate();
            }
        }
        #endregion

        #region Event Overrides
        protected override bool IsInputKey(Keys keyData) {
            switch (keyData & Keys.KeyCode) {
            case Keys.Up:
            case Keys.Down:
            case Keys.Left:
            case Keys.Right:
            case Keys.Tab:
                return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnEnter(EventArgs e) {
            Invalidate();
            base.OnEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            Select();
            base.OnMouseDown(e);
        }

        protected override void OnLeave(EventArgs e) {
            Invalidate();
            base.OnLeave(e);
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            if (showfocus && Focused) {
                System.Drawing.Rectangle rc = ClientRectangle;
                rc.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(pe.Graphics, rc);
            }
        }
        #endregion
    }
}
