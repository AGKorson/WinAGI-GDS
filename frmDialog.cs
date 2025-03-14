using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinAGI.Editor {
    internal partial class frmDialog : Form {
        Form HelpOwner;
        string HelpFile = "";
        string HelpTopic = "";

        internal frmDialog(Form owner, string Prompt, string Title, MessageBoxButtons Buttons, MessageBoxIcon Icon, string CheckString, ref bool Checked, string HelpFile, string HelpTopic) {
            // show a custom msgbox
            // OKOnly =              0 1
            // OKCancel =            1 1-2
            // AbortRetryIgnore =    2 1-2-3
            // YesNoCancel =         3 1-2-3
            // YesNo =               4 1-2
            // RetryCancel =         5 1-2
            // CancelTryContinue =   6 1-2-3

            // ApplicationModal =     0
            // SystemModal =     0x1000

            // DefaultButton1 =       0
            // DefaultButton2 =   0x100
            // DefaultButton3 =   0x200
            // DefaultButton4 =   0x300

            // Critical =          0x10
            // Question =          0x20
            // Exclamation =       0x30
            // Information =       0x40

            // MsgBoxHelpButton =     0x4000
            // MsgBoxSetForeground = 0x10000  //?
            // MsgBoxRight =         0x80000  //right align the msg text
            // MsgBoxRtlReading =   0x100000  //?
            int lngButtonCount = 0;
            int lngBW;
            bool showHelp = HelpTopic.Length > 0;
            InitializeComponent();
            // ClientSize gets messed up by the InitializeComponents function - 
            // by forcing height/width, it seems to reset it to correct value
            Height++;
            Width++;
            if (Height - ClientSize.Height != 39) {
                Debug.Assert(false);
            }
            // pass along help info
            this.HelpFile = HelpFile;
            this.HelpTopic = HelpTopic;

            // need to account for border
            lngBW = Width - ClientSize.Width;

            if (Title.Length == 0) {
                Text = Application.ProductName;
            }
            else {
                Text = Title;
            }
            // the checkstring (if visible) fit
            Message.Text = Prompt;

            // add icons (if requested)
            if (Icon != MessageBoxIcon.None) {
                switch (Icon) {
                case MessageBoxIcon.Error:
                    // also catches MessageBoxIcon.Stop and MessageBoxIcon.Hand
                    Image1.Image = EditorResources.vsError.ToBitmap();
                    break;
                case MessageBoxIcon.Question:
                    Image1.Image = EditorResources.vsQuestion.ToBitmap();
                    break;
                case MessageBoxIcon.Exclamation:
                    // also catches MessageBoxIcon.Warning
                    Image1.Image = EditorResources.vsExclamation.ToBitmap();
                    break;
                default:
                    // catches MessageBoxIcon.Information, MessageBoxIcon.Asterisk
                    // and any unknown values
                    Image1.Image = EditorResources.vsInformation.ToBitmap();
                    break;
                }
                Image1.Visible = true;
                // adjust label position to account for icon
                Message.Left += 47;

                // if text height of msg is <height of Image,
                if (Message.Height < Image1.Height) {
                    // center it
                    Message.Top += (Image1.Height - Message.Height) / 2;
                    // buttons are below icon
                    button1.Top = Image1.Top + Image1.Height + 17;
                }
                else {
                    // buttons are below msg
                    button1.Top = Message.Top + Message.Height + 17;
                }
            }
            else {
                // no icon; buttons are below msg
                button1.Top = Message.Top + Message.Height + 17;
            }
            Debug.Assert(Height - ClientSize.Height == 39);
            // now set height
            Height = (Height - ClientSize.Height) + button1.Top + button1.Height + 11;
            // is checkmark needed
            if (CheckString.Length != 0) {
                // position checkbox under msg
                Check1.Left = Message.Left;
                Check1.Top = button1.Top + 12;
                // move buttons down to account for checkbox
                button1.Top += 40;
                // adjust dialog height to account for checkbox
                Height += 40;
                // set check properties based on passed parameters
                Check1.Width = TextRenderer.MeasureText(CheckString, Message.Font).Width + 40;
                Check1.Text = CheckString;
                Check1.Checked = Checked;
                Check1.Visible = true;
            }
            // move other buttons to correct height
            button2.Top = button1.Top;
            button3.Top = button1.Top;
            cmdHelp.Top = button1.Top;
            // set button captions
            switch (Buttons) {
            case MessageBoxButtons.OK:
                button1.Text = "OK";
                AcceptButton = button1;
                CancelButton = button1;
                lngButtonCount = 1;
                break;
            case MessageBoxButtons.OKCancel:
                button1.Text = "OK";
                button2.Text = "Cancel";
                button2.Visible = true;
                AcceptButton = button1;
                CancelButton = button2;
                lngButtonCount = 2;
                break;
            case MessageBoxButtons.AbortRetryIgnore:
                button1.Text = "Abort";
                button2.Text = "Retry";
                button2.Visible = true;
                button3.Text = "Ignore";
                button3.Visible = true;
                AcceptButton = button1;
                CancelButton = button3;
                lngButtonCount = 3;
                break;
            case MessageBoxButtons.YesNoCancel:
                button1.Text = "Yes";
                button2.Text = "No";
                button2.Visible = true;
                button3.Text = "Cancel";
                button3.Visible = true;
                AcceptButton = button1;
                CancelButton = button3;
                lngButtonCount = 3;
                break;
            case MessageBoxButtons.YesNo:
                button1.Text = "Yes";
                button2.Text = "No";
                button2.Visible = true;
                AcceptButton = button1;
                lngButtonCount = 2;
                break;
            case MessageBoxButtons.RetryCancel:
                button1.Text = "Retry";
                button2.Text = "Cancel";
                button2.Visible = true;
                AcceptButton = button1;
                CancelButton = button2;
                lngButtonCount = 2;
                break;
            case MessageBoxButtons.CancelTryContinue:
                button1.Text = "Cancel";
                button2.Text = "Try Again";
                button2.Visible = true;
                button3.Text = "Continue";
                button3.Visible = true;
                AcceptButton = button2;
                CancelButton = button1;
                lngButtonCount = 3;
                break;
            }
            if (showHelp) {
                cmdHelp.Visible = true;
                lngButtonCount++;
                // identify owner form for help
                HelpOwner = owner;
            }
            // width needs to be wide enough for the message, all buttons, and the checkbox text
            // save width based on msg size so it can be compared to button size
            int width = lngBW + Message.Left + Message.Width + 14;
            if (width < lngButtonCount * (button1.Width + 6) + lngBW + 6) {
                width = lngButtonCount * (button1.Width + 6) + lngBW + 6;
            }
            if (width < Check1.Left + Check1.Width + 14) {
                width = Check1.Left + Check1.Width + 14;
            }
            Width = width;
            // move button1 to correct position based on button Count
            button1.Left = ClientSize.Width - lngButtonCount * (button1.Width + 6);
            // move other buttons based on button1 pos
            button2.Left = button1.Left + button1.Width + 6;
            button3.Left = button2.Left + button2.Width + 6;

            if (showHelp) {
                // position it as rightmost button
                cmdHelp.Left = ClientSize.Width - cmdHelp.Width - 6;
            }
            // after form is sized and populated
            if (owner is null) {
                StartPosition = FormStartPosition.CenterScreen;
            }
            else {
                StartPosition = FormStartPosition.Manual;
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

            }
        }

        #region Event Handlers
        private void Form_KeyDown(object sender, KeyEventArgs e) {
            // respond to keys depending on mode

            if (e.Alt == true || e.Control == true) {
                //ignore alt and ctrl
                e.SuppressKeyPress = true;
                return;
            }
            switch (e.KeyCode) {
            case Keys.A:
                if (button1.Text == "Abort") {
                    DialogResult = DialogResult.Abort;
                    Close();
                }
                break;
            case Keys.R:
                if (button1.Text == "Retry" || button2.Text == "Retry") {
                    DialogResult = DialogResult.Retry;
                    Close();
                }
                break;
            case Keys.I:
                if (button3.Text == "Ignore") {
                    DialogResult = DialogResult.Ignore;
                    Close();
                }
                break;
            case Keys.Y:
                if (button1.Text == "Yes") {
                    DialogResult = DialogResult.Yes;
                    Close();
                }
                break;
            case Keys.N:
                if (button2.Text == "No") {
                    DialogResult = DialogResult.No;
                    Close();
                }
                break;
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            switch (button1.Text) {
            case "OK":
                DialogResult = DialogResult.OK;
                break;
            case "Abort":
                DialogResult = DialogResult.Abort;
                break;
            case "Yes":
                DialogResult = DialogResult.Yes;
                break;
            case "Retry":
                DialogResult = DialogResult.Retry;
                break;
            case "Cancel":
                DialogResult = DialogResult.Cancel;
                break;
            }
            Close();
        }

        private void button2_Click(object sender, EventArgs e) {
            switch (button2.Text) {
            case "Cancel":
                DialogResult = DialogResult.Cancel;
                break;
            case "Retry":
                DialogResult = DialogResult.Retry;
                break;
            case "No":
                DialogResult = DialogResult.No;
                break;
            case "Try Again":
                DialogResult = DialogResult.TryAgain;
                break;
            }
            Close();
        }

        private void button3_Click(object sender, EventArgs e) {
            switch (button3.Text) {
            case "Ignore":
                DialogResult = DialogResult.Ignore;
                break;
            case "Cancel":
                DialogResult = DialogResult.Cancel;
                break;
            case "Continue":
                DialogResult = DialogResult.Continue;
                break;
            }
            Close();
        }

        private void cmdHelp_Click(object sender, EventArgs e) {
            Help.ShowHelp(HelpOwner, HelpFile, HelpNavigator.Topic, HelpTopic);
        }

#endregion
    }
}
