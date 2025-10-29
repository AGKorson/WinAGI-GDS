using System;
using System.ComponentModel;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.frmPicEdit;

namespace WinAGI.Editor {
    public partial class frmPicPrintPrev : Form {
        public PrintTestInfo PTInfo;
        public bool InGame;

        public frmPicPrintPrev(PrintTestInfo ptinfo, bool ingame) {
            InitializeComponent();
            PTInfo = new(ptinfo);
            InGame = ingame;

            // background limited unless power pack is available
            if (!InGame || !EditGame.PowerPack) {
                cmbBG.Items.Clear();
                cmbBG.Items.Add("Black");
                cmbBG.Items.Add("White");
            }
            cmbFG.SelectedIndex = PTInfo.FGColor;
            if (InGame && EditGame.PowerPack) {
                cmbBG.SelectedIndex = PTInfo.BGColor;
            }
            else {
                if (PTInfo.BGColor == 0) {
                    cmbBG.SelectedIndex = 0;
                }
                else {
                    cmbBG.SelectedIndex = 1;
                    PTInfo.BGColor = 1;
                }
            }
            cmbFG.Enabled = (InGame && EditGame.PowerPack) || PTInfo.BGColor == 0;
            txtCol.Value = PTInfo.StartCol;
            txtRow.Value = PTInfo.Top;
            txtMW.Value = PTInfo.MaxWidth;
            txtOffset.Value = PTInfo.PicOffset;
            txtMessage.Text = PTInfo.Text;
            switch (PTInfo.Mode) {
            case PrintTestMode.Print:
                optPrint.Checked = true;
                break;
            case PrintTestMode.PrintAt:
                optPrintAt.Checked = true;
                break;
            case PrintTestMode.Display:
                optDisplay.Checked = true;
                break;
            }
        }

        #region Event Handlers
        private void cmdOK_Click(object sender, EventArgs e) {
            // update the test info, then return
            PTInfo = new();
            PTInfo.StartCol = txtCol.Value;
            PTInfo.StartRow = txtRow.Value;
            PTInfo.MaxWidth = txtMW.Value;
            PTInfo.PicOffset = txtOffset.Value;
            PTInfo.Text = txtMessage.Text;
            PTInfo.FGColor = cmbFG.SelectedIndex;
            PTInfo.BGColor = cmbBG.SelectedIndex;
            if (optPrint.Checked) {
                PTInfo.Mode = PrintTestMode.Print;
            }
            else if (optPrintAt.Checked) {
                PTInfo.Mode = PrintTestMode.PrintAt;
            }
            else {
                PTInfo.Mode = PrintTestMode.Display;
            }
            // confirm the message will print without error
            if (PTInfo.ErrLevel < 0) {
                MessageBox.Show(MDIMain,
                    "The message is too long to fit on the picture image with the " +
                    "current offset and row/col values. One or more characters will " +
                    "wrap into the row(s) below the picture. Adjust the offset and/or " +
                    "row/col values to continue.",
                    "Invalid display() Command Parameters",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else if (PTInfo.ErrLevel > 0) {
                if (optPrintAt.Checked) {
                    MessageBox.Show(MDIMain,
                        "The size and position of the print window will not fit within the " +
                        "boundaries of the picture image. Adjust the message text and/or " +
                        "row/col/minwidth values to continue.",
                        "Invalid print.at() Command Parameters",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else {
                    MessageBox.Show(MDIMain,
                        "The length of text is too large for the print window to to fit " +
                        "within the boundaries of the picture image. Shorten the message " +
                        "text to continue.",
                        "Invalid print() Command Parameters",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else {
                DialogResult = DialogResult.OK;
                Hide();
            }
        }

        private void cmdCopy_Click(object sender, EventArgs e) {
            // add commands to clipboard
            string strText = rtfLine1.Text;
            if (rtfLine2.Visible) {
                strText += Environment.NewLine + rtfLine2.Text;
            }
            Clipboard.Clear();
            Clipboard.SetText(strText);
        }

        private void cmdCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void cmbFG_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateCommand();
        }

        private void cmbBG_SelectedIndexChanged(object sender, EventArgs e) {
            if (!EditGame.PowerPack) {
                // only valid choices are black or white
                // foreground is limited to:  black if BG is white (1)
                //                            any color if BG is black (0)
                if (cmbBG.SelectedIndex == 1) {
                    cmbFG.SelectedIndex = 0;
                    cmbFG.Enabled = false;
                }
                else {
                    cmbFG.Enabled = true;
                }
            }
            UpdateCommand();
        }

        private void optPrint_CheckedChanged(object sender, EventArgs e) {
            txtRow.Visible = false;
            lblRow.Visible = false;
            txtCol.Visible = false;
            lblCol.Visible = false;
            cmbBG.Visible = false;
            lblBG.Visible = false;
            cmbFG.Visible = false;
            lblFG.Visible = false;
            txtMW.Visible = false;
            lblMW.Visible = false;
            lblOffset.Visible = false;
            txtOffset.Visible = false;
            PTInfo.Mode = PrintTestMode.Print;
            UpdateCommand();
        }

        private void optPrintAt_CheckedChanged(object sender, EventArgs e) {
            txtRow.Visible = true;
            txtRow.MaxValue = 24;
            lblRow.Visible = true;
            txtCol.Visible = true;
            lblCol.Visible = true;
            cmbBG.Visible = false;
            lblBG.Visible = false;
            cmbFG.Visible = false;
            lblFG.Visible = false;
            txtMW.Visible = true;
            lblMW.Visible = true;
            lblOffset.Visible = false;
            txtOffset.Visible = false;
            // adjust row/col limits
            txtCol.MinValue = 2;
            txtRow.MinValue = 1;
            // col + mw must be less than MaxCol
            // col < MaxCol - mw
            txtCol.MaxValue = PTInfo.MaxCol - txtMW.Value - 1;
            if (txtOffset.Value > 0) {
                txtRow.MinValue = 1;
            }
            else {
                txtRow.MinValue = 1;
            }
            txtRow.MaxValue = 20 - PTInfo.Height;
            PTInfo.Mode = PrintTestMode.PrintAt;
            UpdateCommand();
        }

        private void optDisplay_CheckedChanged(object sender, EventArgs e) {
            txtRow.Visible = true;
            lblRow.Visible = true;
            txtCol.Visible = true;
            lblCol.Visible = true;
            cmbBG.Visible = true;
            lblBG.Visible = true;
            cmbFG.Visible = true;
            lblFG.Visible = true;
            txtMW.Visible = false;
            lblMW.Visible = false;
            lblOffset.Visible = true;
            txtOffset.Visible = true;
            // reset row/col limits
            txtCol.MinValue = 0;
            txtCol.MaxValue = PTInfo.MaxCol;
            txtRow.MinValue = txtOffset.Value;
            txtRow.MaxValue = txtRow.MinValue + 20;
            PTInfo.Mode = PrintTestMode.Display;
            UpdateCommand();
        }

        private void txtMessage_TextChanged(object sender, EventArgs e) {
            // make sure no tabs, cr/lfs
            if (txtMessage.Text.Contains('\t')) {
                txtMessage.Text = txtMessage.Text.Replace("\t", "");
            }
            if (txtMessage.Text.Contains('\n')) {
                txtMessage.Text = txtMessage.Text.Replace("\n", "");
            }
            if (txtMessage.Text.Contains('\r')) {
                txtMessage.Text = txtMessage.Text.Replace("\r", "");
            }
            // TODO: check for other invalid chars?
            UpdateCommand();
            PTInfo.Text = txtMessage.Text;
            if (optPrintAt.Checked) {
                // adjust max row
                txtRow.MaxValue = 20 - PTInfo.Height;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e) {
            UpdateCommand();
        }

        private void txtRow_Validating(object sender, CancelEventArgs e) {
            // if blank, reset to default
            if (txtRow.Text.Length == 0) {
                if (optDisplay.Checked) {
                    txtRow.Text = "0";
                }
                else {
                    txtRow.Text = "5";
                }
            }
            PTInfo.StartRow = txtRow.Value;
        }

        private void txtCol_Validating(object sender, CancelEventArgs e) {
            // if blank, reset to default
            if (txtCol.Text.Length == 0) {
                if (optDisplay.Checked) {
                    txtCol.Text = "0";
                }
                else {
                    txtCol.Text = "5";
                }
            }
            PTInfo.StartCol = txtCol.Value;
        }

        private void txtMW_Validating(object sender, CancelEventArgs e) {
            // if blank, reset to default
            if (txtMW.Text.Length == 0) {
                txtMW.Text = "30";
            }
            txtCol.MaxValue = PTInfo.MaxCol - txtMW.Value - 1;
            PTInfo.MaxWidth = txtMW.Value;
        }

        private void txtOffset_Validating(object sender, CancelEventArgs e) {
            // if blank, reset to default
            if (txtOffset.Text.Length == 0) {
                txtOffset.Text = "0";
            }
            // adjust row limits
            txtRow.MinValue = txtOffset.Value;
            txtRow.MaxValue = txtRow.MinValue + 20;
            PTInfo.PicOffset = txtOffset.Value;
        }

        private void cmMsg_Opening(object sender, CancelEventArgs e) {
            cmiUndo.Enabled = txtMessage.CanUndo;
            cmiCut.Enabled = txtMessage.SelectionLength > 0;
            cmiCopy.Enabled = txtMessage.SelectionLength > 0;
            cmiPaste.Enabled = Clipboard.ContainsText();
            cmiDelete.Enabled = txtMessage.SelectionLength > 0;
            cmiSelectAll.Enabled = txtMessage.TextLength > 0;
        }

        private void cmMsg_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            cmiUndo.Enabled = true;
            cmiCut.Enabled = true;
            cmiCopy.Enabled = true;
            cmiPaste.Enabled = true;
            cmiDelete.Enabled = true;
            cmiSelectAll.Enabled = true;
        }

        private void cmiUndo_Click(object sender, EventArgs e) {
            if (txtMessage.CanUndo) {
                txtMessage.Undo();
            }
        }

        private void cmiCut_Click(object sender, EventArgs e) {
            if (txtMessage.SelectionLength > 0) {
                txtMessage.Cut();
            }
        }

        private void cmiCopy_Click(object sender, EventArgs e) {
            if (txtMessage.SelectionLength > 0) {
                txtMessage.Copy();
            }
        }

        private void cmiPaste_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText()) {
                txtMessage.Paste();
                if (txtMessage.Text.Contains("\r\n")) {
                    txtMessage.Text = txtMessage.Text.Replace("\r\n", "");
                }
                if (txtMessage.Text.Contains('\r')) {
                    txtMessage.Text = txtMessage.Text.Replace("\r", "");
                }
                if (txtMessage.Text.Contains('\n')) {
                    txtMessage.Text = txtMessage.Text.Replace("\n", "");
                }
            }
        }

        private void cmiDelete_Click(object sender, EventArgs e) {
            if (txtMessage.SelectionLength > 0) {
                txtMessage.SelectedText = "";
            }
            else {
                if (txtMessage.SelectionStart < txtMessage.Text.Length) {
                    int oldsel = txtMessage.SelectionStart;
                    txtMessage.Text = txtMessage.Text[..oldsel] + txtMessage.Text[(oldsel + 1)..];
                    txtMessage.SelectionStart = oldsel;
                }
            }
        }

        private void cmiCharMap_Click(object sender, EventArgs e) {
            frmCharPicker CharPicker;
            if (EditGame is not null) {
                CharPicker = new(EditGame.CodePage);
            }
            else {
                CharPicker = new(WinAGISettings.DefCP.Value);
            }
            CharPicker.ShowDialog(MDIMain);
            if (!CharPicker.Cancel) {
                if (CharPicker.InsertString.Length > 0) {
                    txtMessage.SelectedText = CharPicker.InsertString;
                }
            }
            CharPicker.Close();
            CharPicker.Dispose();
        }

        private void cmiSelectAll_Click(object sender, EventArgs e) {
            if (txtMessage.TextLength > 0) {
                txtMessage.SelectAll();
            }
        }
        #endregion

        #region Methods
        private void UpdateCommand() {
            // build the command string using current parameters
            string tmpBG, strText = "";

            // quotes need to be escaped
            for (int i = 0; i < txtMessage.Text.Length; i++) {
                if (txtMessage.Text[i] == '"') {
                    if (strText[^1] != '\\') {
                        strText += '\\';
                    }
                }
                strText += txtMessage.Text[i];
            }

            if (optPrint.Checked) {
                rtfLine1.Text = "print(\"" + strText + "\");";
                rtfLine2.Visible = false;
            }
            else if (optPrintAt.Checked) {
                rtfLine1.Text = "print.at(\"" + strText +
                  "\", " + txtRow.Text + ", " + txtCol.Text + ", " + txtMW.Text + ");";
                rtfLine2.Visible = false;
            }
            else {
                TDefine[] reservedcolor = EditGame.ReservedDefines.ColorNames;
                if (EditGame.PowerPack) {
                    // all color combos allowed
                    if (EditGame.IncludeReserved) {
                        rtfLine1.Text = "set.text.attribute(" + reservedcolor[cmbFG.SelectedIndex].Name +
                          ", " + reservedcolor[cmbBG.SelectedIndex].Name + ");";
                    }
                    else {
                        rtfLine1.Text = "set.text.attribute(" + cmbFG.SelectedIndex + ", " + cmbBG.SelectedIndex + ");";
                    }
                }
                else {
                    // only black or white for background
                    if (cmbBG.SelectedIndex == 0) {
                        if (EditGame.IncludeReserved) {
                            tmpBG = reservedcolor[0].Name;
                        }
                        else {
                            tmpBG = "0";
                        }
                    }
                    else {
                        if (EditGame.IncludeReserved) {
                            tmpBG = reservedcolor[15].Name;
                        }
                        else {
                            tmpBG = "15";
                        }
                    }
                    rtfLine1.Text = "set.text.attribute(" + reservedcolor[cmbFG.SelectedIndex].Name + ", " + tmpBG + ");";
                }
                rtfLine2.Text = "display(" + txtRow.Text + ", " +
                  txtCol.Text + ", \"" + strText + "\");";
                rtfLine2.Visible = true;
            }
        }
        #endregion
    }
}
