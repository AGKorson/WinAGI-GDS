﻿using System;
using System.Windows.Forms;
using static WinAGI.Editor.Base;
using WinAGI.Engine;
using static WinAGI.Editor.Base.EGetRes;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio.Shell.Interop;

namespace WinAGI.Editor {
    public partial class frmEditDescription : Form {
        int SelCtrl = 0; // 0=none; 1=ID, 2=description
        public string NewID = "", NewDescription = "";
        public frmEditDescription(AGIResType EWMode, byte ResNum, string OldID, string OldDescription, bool InGame, int FirstProp) {
            InitializeComponent();

            switch (EWMode) {
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                if (!InGame) {
                    txtID.Width = 272;
                    chkUpdate.Visible = false;
                }
                else {
                    chkUpdate.Checked = DefUpdateVal;
                }
                lblID.Text = "Resource ID for " + EWMode.ToString() + " " + ResNum + ": ";
                NewID = OldID;
                NewDescription = OldDescription;
                break;

            case AGIResType.Objects:
            case AGIResType.Words:
                txtID.Visible = false;
                lblID.Visible = false;
                lblDescription.Top = lblID.Top;
                txtDescription.Top = txtID.Top;
                txtDescription.Height = 155;
                Text = EWMode == AGIResType.Objects ? "Edit OBJECT Description" : "Edit WORDS.TOK Description";
                NewDescription = OldDescription;
                break;
            }
            txtDescription.Text = OldDescription;
            txtID.Text = OldID;
            if (!InGame) {
                txtID.Enabled = false;
                SelCtrl = 2;
            }
            else {
                SelCtrl = FirstProp;
            }
            switch (SelCtrl) {
            // case 0:
            // no action taken
            case 1:
                // select id
                txtID.SelectAll();
                txtID.Focus();
                break;
            case 2:
                // select description
                txtDescription.SelectAll();
                txtDescription.Focus();
                break;
            }
        }

        private void btnOK_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            NewID = txtID.Text;
            NewDescription = txtDescription.Text;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtDescription_TextChanged(object sender, EventArgs e) {
            btnOK.Enabled = true;
        }

        private void txtID_TextChanged(object sender, EventArgs e) {
            btnOK.Enabled = (txtID.Text.Length > 0);
        }

        private void txtID_KeyPress(object sender, KeyPressEventArgs e) {
            // some characters not allowed:

            // NOT OK  x!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
            //     OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    

            switch ((int)e.KeyChar) {
            case >= 32 and <= 34:
            case >= 38 and <= 45:
            case 47:
            case >= 58 and <= 63:
            case >= 91 and <= 94:
            case 96:
            case >= 123:
                e.Handled = true;
                break;
            }
        }
    }

    /*
Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
  'check for help key
  If Shift = 0 And KeyCode = vbKeyF1 Then
    If ActiveControl Is txtDescription Then
      'help with description
      HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Managing Resources.htm#descriptions"
    Else
      'help with ID
      HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Managing Resources.htm#resourceids"
    End If
    KeyCode = 0
  End If
End Sub
    */
}
