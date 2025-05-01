using System;
using System.ComponentModel;
using System.Windows.Forms;
namespace WinAGI.Editor {
    public class NumericTextBox : TextBox {
        private int _maxValue = int.MaxValue;
        private int _minValue = int.MinValue;
        private int oldvalue;

        public int MaxValue {
            get => _maxValue; 
            set {
                _maxValue = value;
                if (value < MinValue) {
                    MinValue = value;
                }
                if (this.Value < MinValue) {
                    this.Text = MinValue.ToString();
                }
                else if (this.Value > MaxValue) {
                    this.Text = MaxValue.ToString();
                }
            }
        }
        
        public int MinValue {
            get => _minValue;
            set {
                _minValue = value;
                if (value > MaxValue) {
                    MaxValue = value;
                }
                if (this.Value < MinValue) {
                    this.Text = MinValue.ToString();
                }
                else if (this.Value > MaxValue) {
                    this.Text = MaxValue.ToString();
                }
            }
        }
        
        public int Value {
            get {
                if (int.TryParse(this.Text, out int value)) {
                    return value;
                }
                return MinValue; // Return MinValue if parsing fails
            }
            set {
                if (value < MinValue) {
                    this.Text = MinValue.ToString();
                }
                else if (value > MaxValue) {
                    this.Text = MaxValue.ToString();
                }
                else {
                    this.Text = value.ToString();
                }
            }
        }
        
        public NumericTextBox() {
            this.TextAlign = HorizontalAlignment.Right; // Align text to the right
        }

        protected override void OnEnter(EventArgs e) {
            base.OnEnter(e);
            // Store the current value when the control gains focus
            if (int.TryParse(this.Text, out int value)) {
                oldvalue = value;
            }
            else {
                oldvalue = MinValue; // Default to MinValue if parsing fails
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
            if (e.KeyChar == '-' && MinValue < 0 && this.SelectionStart == 0 && !this.Text.Contains('-')) {
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
                this.Parent.SelectNextControl(this, true, true, true, true);
                break;
            case Keys.Escape:
                // Reset the value to the old value when Escape is pressed
                this.Text = oldvalue.ToString();
                this.SelectionStart = this.Text.Length; // Move cursor to the end
                e.SuppressKeyPress = true; // Suppress the 'ding' sound

                // Move focus to the next control
                this.Parent.SelectNextControl(this, true, true, true, true);
                break;
            }
        }

        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);

            // Validate the input value
            if (int.TryParse(this.Text, out int value)) {
                if (value < MinValue) {
                    this.Text = MinValue.ToString();
                    this.SelectionStart = this.Text.Length; // Move cursor to the end
                }
                else if (value > MaxValue) {
                    this.Text = MaxValue.ToString();
                    this.SelectionStart = this.Text.Length; // Move cursor to the end
                }
            }
            else if (!string.IsNullOrEmpty(this.Text)) {
                // if MinValue is < 0, a single '-' is allowed
                if (MinValue >= 0 || this.Text != "-") {
                    // If the input is invalid (e.g., non-numeric), reset to MinValue
                    this.Text = MinValue.ToString();
                    this.SelectionStart = this.Text.Length; // Move cursor to the end
                }
            }
        }

        protected override void OnValidating(CancelEventArgs e) {
            base.OnValidating(e);
            if (e.Cancel) {
                // restore the old value if validation is canceled
                this.Text = oldvalue.ToString();
                return;
            }
        }
    }
}
