using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WinAGI.Editor {
    public class NumericTextBox : TextBox {
        #region Fields
        private int _maxValue = int.MaxValue;
        private int _minValue = int.MinValue;
        private int oldvalue;
        #endregion

        #region Constructors
        public NumericTextBox() {
            TextAlign = HorizontalAlignment.Right; // Align text to the right
        }
        #endregion

        #region Properties
        public int MaxValue {
            get => _maxValue;
            set {
                _maxValue = value;
                if (value < _minValue) {
                    _minValue = value;
                }
                if (Value < _minValue) {
                    Text = _minValue.ToString();
                }
                else if (Value > _maxValue) {
                    Text = _maxValue.ToString();
                }
            }
        }

        public int MinValue {
            get => _minValue;
            set {
                _minValue = value;
                if (value > _maxValue) {
                    _maxValue = value;
                }
                if (Value < _minValue) {
                    Text = _minValue.ToString();
                }
                else if (Value > _maxValue) {
                    Text = _maxValue.ToString();
                }
            }
        }

        public int Value {
            get {
                if (int.TryParse(Text, out int value)) {
                    return value;
                }
                return _minValue; // Return MinValue if parsing fails
            }
            set {
                if (value < _minValue) {
                    Text = _minValue.ToString();
                }
                else if (value > _maxValue) {
                    Text = _maxValue.ToString();
                }
                else {
                    Text = value.ToString();
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the value of the numeric text box changes.
        /// </summary>
        public event EventHandler ValueChanged;
        #endregion

        #region Event Overrides
        protected override void OnEnter(EventArgs e) {
            base.OnEnter(e);
            // Store the current value when the control gains focus
            if (int.TryParse(Text, out int value)) {
                oldvalue = value;
            }
            else {
                oldvalue = _minValue; // Default to MinValue if parsing fails
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            base.OnKeyPress(e);

            // Allow control keys (e.g., backspace, delete, etc.)
            if (char.IsControl(e.KeyChar)) {
                return;
            }

            // Allow numeric input only
            if (char.IsDigit(e.KeyChar)) {
                return;
            }
            // Allow '-' only if MinValue is less than zero and it's the first character
            if (e.KeyChar == '-' && _minValue < 0 && SelectionStart == 0 && !Text.Contains('-')) {
                return;
            }
            // Block all other input
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            switch (e.KeyCode) {
            case Keys.Enter:
                e.Handled = true; // Prevent default behavior
                e.SuppressKeyPress = true; // Suppress the 'ding' sound

                // Move focus to the next control
                Parent?.SelectNextControl(this, true, true, true, true);
                break;
            case Keys.Escape:
                // Reset the value to the old value when Escape is pressed
                Text = oldvalue.ToString();
                SelectionStart = Text.Length; // Move cursor to the end
                e.SuppressKeyPress = true; // Suppress the 'ding' sound

                // Move focus to the next control
                Parent?.SelectNextControl(this, true, true, true, true);
                break;
            }
        }

        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);
            if (string.IsNullOrEmpty(Text) || Text == "-") {
                return;
            }
            // if invalid text is entered, reset to previous valid value
            if (!int.TryParse(Text, out _)) {
                Text = oldvalue.ToString();
            }
        }

        protected override void OnValidating(CancelEventArgs e) {
            base.OnValidating(e);
            if (e.Cancel) {
                // restore the old value if validation is canceled
                Text = oldvalue.ToString();
                return;
            }
            // if invalid text is entered, reset to previous valid value
            if (!int.TryParse(Text, out int value)) {
                Text = oldvalue.ToString();
                return;
            }
            // clamp the value to the min/max range if it exceeds the limits
            if (value < _minValue) {
                Text = _minValue.ToString();
                SelectionStart = Text.Length;
            }
            else if (value > _maxValue) {
                Text = _maxValue.ToString();
                SelectionStart = Text.Length;
            }
            if (value != oldvalue) {
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}
