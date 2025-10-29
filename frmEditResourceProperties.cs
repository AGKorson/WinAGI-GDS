using System;
using System.Windows.Forms;
using static WinAGI.Editor.Base;
using WinAGI.Engine;

namespace WinAGI.Editor {
    public partial class frmEditResourceProperties : Form {
        int SelCtrl = 0; // 0=none; 1=ID, 2=description
        public string NewID = "", NewDescription = "";

        public frmEditResourceProperties(AGIResType EWMode, byte ResNum, string OldID, string OldDescription, bool InGame, int FirstProp) {
            InitializeComponent();

            switch (EWMode) {
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                if (InGame) {
                    chkUpdate.Checked = DefUpdateLogics;
                    lblID.Text = "Resource ID for " + EWMode.ToString() + " " + ResNum + ": ";
                }
                else {
                    txtID.Width = 272;
                    chkUpdate.Visible = false;
                    lblID.Text = "Resource ID: ";
                }
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
            btnOK.Enabled = false;
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
                txtID.Select();
                break;
            case 2:
                // select description
                txtDescription.SelectAll();
                txtDescription.Select();
                break;
            }
        }

        #region Event Handlers

        private void btnOK_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            NewID = txtID.Text;
            NewDescription = txtDescription.Text;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void txtDescription_TextChanged(object sender, EventArgs e) {
            btnOK.Enabled = (txtID.Text.Length > 0 || !txtID.Visible);
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
        #endregion

        /*
    Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
      'check for help key
      If Shift = 0 And KeyCode = vbKeyF1 Then
        If ActiveControl Is txtDescription Then
          'help with description
          Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\managingresources.htm#descriptions");
        Else
          'help with ID
          Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\managingresources.htm#resourceids");
        End If
        KeyCode = 0
      End If
    End Sub
        */
    }
}
