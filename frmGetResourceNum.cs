using System;
using System.Windows.Forms;
using static WinAGI.Editor.Base;
using WinAGI.Engine;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Editor.Base.EGetRes;
using System.Windows.Forms.Design;

namespace WinAGI.Editor {
    public partial class frmGetResourceNum : Form {
        public AGIResType ResType;
        public byte NewResNum;
        public byte OldResNum;
 //       public bool Canceled = true;
        public bool DontImport = false;
        public EGetRes WindowFunction;

        class ListItemData {
            public byte ResNum = 0;
            public string ID = "";
            public override string ToString() { return ID; }
        }

        public frmGetResourceNum(EGetRes function, AGIResType restype) {
            InitializeComponent();
            ResType = restype;
            OldResNum = 0;
            FormSetup(function);
        }


        public frmGetResourceNum(EGetRes function, AGIResType restype, byte resnum) {
            InitializeComponent();
            ResType = restype;
            OldResNum = resnum;
            FormSetup(function);
        }

        public void FormSetup(EGetRes function) {
            int i;
            ListItemData tmpItem;
            WindowFunction = function;
            //clear the listbox
            lstResNum.Items.Clear();

            switch (WindowFunction) {
            case grAddNew:
            case grAddInGame:
            case grRenumber:
            case grRenumberRoom:
            case grAddLayout:
            case grImport:
                // if adding rooms in layout, always SUGGEST include pic
                if (WindowFunction == grAddLayout) {
                    chkIncludePic.Checked = true;
                    chkIncludePic.Enabled = true;
                }
                // only add  resource numbers that are NOT in use
                for (i = 0; i < 256; i++) {
                    tmpItem = new ListItemData {
                        ResNum = (byte)i,
                        ID = i.ToString()
                    };
                    switch (ResType) {
                    case AGIResType.Logic:
                        // if exists, don't add
                        // if grAddLayout AND i == 0, don't add
                        // if grRenumberRoom AND Picture exists, don't add
                        if (!EditGame.Logics.Contains((byte)i) &&
                            (WindowFunction != grAddLayout || i != 0) &&
                            (WindowFunction != grRenumberRoom || !EditGame.Pictures.Contains((byte)i))) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.View:
                        if (!EditGame.Views.Contains((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Picture:
                        if (!EditGame.Pictures.Contains((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Sound:
                        if (!EditGame.Sounds.Contains((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    }
                }
                break;
            case grOpen or grTestView or grMenu or grMenuBkgd:
                // only add resources that are in game
                for (i = 0; i < 256; i++) {
                    tmpItem = new() {
                        ResNum = (byte)i
                    };
                    switch (ResType) {
                    case AGIResType.Logic:
                        if (EditGame.Logics.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum
                                ? ResourceName(EditGame.Logics[i], true)
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }

                        break;
                    case AGIResType.Picture:
                        if (EditGame.Pictures.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum
                                ? ResourceName(EditGame.Pictures[i], true)
                                : i + " - " + EditGame.Pictures[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.Sound:
                        if (EditGame.Sounds.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum
                                ? ResourceName(EditGame.Sounds[i], true)
                                : i + " - " + EditGame.Sounds[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case AGIResType.View:
                        if (EditGame.Views.Contains((byte)i)) {
                            tmpItem.ID = WinAGISettings.ShowResNum
                                ? ResourceName(EditGame.Views[i], true)
                                : i + " - " + EditGame.Views[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    }
                }
                break;
            case grShowRoom:
                // displays all logics in game which do NOT have InRoom=true
                for (i = 1; i < 256; i++) {
                    tmpItem = new() {
                        ResNum = (byte)i
                    };
                    if (EditGame.Logics.Contains((byte)i)) {
                        if (!EditGame.Logics[i].IsRoom) {
                            tmpItem.ID = WinAGISettings.ShowResNum
                                ? ResourceName(EditGame.Logics[i], true)
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                    }
                }
                // if no rooms added
                if (lstResNum.Items.Count == 0) {
                    MessageBox.Show(MDIMain, "All logics in the game are currently tagged as rooms and are visible.", "Show Room", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0, WinAGIHelp, "htm\\winagi\\Managing_Resources.htm#resourceids");
                    // close form
                    Close();
                }
                break;
            }
            // set form size and controls
            AdjustControls();
            // set focus to resnum list
            lstResNum.Focus();
        }

        private void AdjustControls() {

            // shows/hides form controls and adjusts
            // form height and control positions based on window function

            // defaults when form is initially loaded:
            //    size wide (6645)
            //    Label2/lblCurrent are visible
            //    Label1 and lstResNum in lower position (825/1065/1425)
            //    chkRoom, chkPic, chkOpenRes are not visible
            //    cmdDont is not visible
            //    cmdCancel is visible at left position, with caption of 'cancel' (1620)
            //    form caption needs to be set!
            //    list caption (Label1) needs to be set!
            //    cmdOK disabled

            switch (WindowFunction) {
            case grAddNew:
                Text = "Add New " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this new " + ResType.ToString() + ":";
                lstResNum.Top = 24;
                lstResNum.Height = 135;
                btnDont.Visible = true;
                btnDont.Text = "New Not in Game";
                btnCancel.Left = 382;
                if (ResType == AGIResType.Logic) {
                    chkIncludePic.Checked = true;
                    chkIncludePic.Visible = true;
                    chkIncludePic.Enabled = false;
                    chkRoom.Checked = true;
                    chkRoom.Visible = true;
                    chkRoom.Enabled = true;
                    chkRoom.Text = "Include Room Template Code";
                }
                else {
                    chkIncludePic.Visible = false;
                    chkRoom.Visible = false;
                }
                chkOpenRes.Visible = true;
                chkOpenRes.Checked = WinAGISettings.OpenNew;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grRenumber:
                Width = 244;
                Text = "Renumber ";
                switch (ResType) {
                case AGIResType.Logic:
                    Text += ResourceName(EditGame.Logics[OldResNum], true, true);
                    break;
                case AGIResType.Picture:
                    Text += ResourceName(EditGame.Pictures[OldResNum], true, true);
                    break;
                case AGIResType.Sound:
                    Text += ResourceName(EditGame.Sounds[OldResNum], true, true);
                    break;
                case AGIResType.View:
                    Text += ResourceName(EditGame.Views[OldResNum], true, true);
                    break;
                }
                lblCurrent.Text = OldResNum.ToString();
                Label1.Text = "Select a new number for this " + ResType.ToString() + ":";
                lstResNum.Height = 105;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grOpen:
                Width = 244;
                Text = "Open " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select the " + ResType.ToString() + " to open:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grTestView:
                Width = 244;
                Text = "Select Test View";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a view for testing pictures:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // select current test view, if there is one
                lstResNum.SelectedIndex = -1;
                for (int i = 0; i < lstResNum.Items.Count; i++) {
                    if (((ListItemData)lstResNum.Items[i]).ResNum == OldResNum) {
                        lstResNum.SelectedIndex = i;
                        break;
                    }
                }
                break;
            case grAddLayout:
                Text = "Add New Room";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this new room:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                chkIncludePic.Visible = true;
                chkIncludePic.Checked = true;
                chkRoom.Visible = true;
                chkRoom.Checked = true;
                chkRoom.Enabled = false;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grShowRoom:
                Width = 244;
                Text = "Show Room";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a room to show:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grMenu:
                Width = 244;
                Text = "Extract Menu";
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select the logic with the game's menu:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grImport:
                Text = "Import " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this imported " + ResType.ToString() + ":";
                lstResNum.Top = 24;
                lstResNum.Height = 135;
                btnDont.Visible = true;
                btnCancel.Left = 382;
                if (ResType == AGIResType.Logic) {
                    chkIncludePic.Visible = false;
                    chkRoom.Checked = true;
                    chkRoom.Visible = true;
                    chkRoom.Enabled = false;
                    chkRoom.Top = 156;
                    chkRoom.Text = "Import as a Room";
                    txtDescription.Height = 70;
                }
                else {
                    chkIncludePic.Visible = false;
                    chkRoom.Visible = false;
                }
                chkOpenRes.Visible = true;
                chkOpenRes.Checked = WinAGISettings.OpenNew;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grMenuBkgd:
                Width = 244;
                Text = "Menu Background";
                lblCurrent.Text = OldResNum.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select picture to use as background:";
                lstResNum.Top = 24;
                lstResNum.Height = 150;
                // select the current pic
                lstResNum.SelectedIndex = -1;
                for (int i = 0; i < lstResNum.Items.Count; i++) {
                    if (((ListItemData)lstResNum.Items[i]).ResNum == OldResNum) {
                        lstResNum.SelectedIndex = i;
                        break;
                    }
                }
                break;
            case grAddInGame:
                Text = "Add " + ResType.ToString();
                lblCurrent.Visible = false;
                Label2.Visible = false;
                Label1.Top = 8;
                Label1.Text = "Select a number for this " + ResType.ToString() + ":";
                lstResNum.Top = 32;
                lstResNum.Height = 135;
                btnDont.Visible = false;
                btnCancel.Left = 382;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            case grRenumberRoom:
                Width = 244;
                Text = "Renumber " + ResourceName(EditGame.Logics[OldResNum], true, true);
                lblCurrent.Text = OldResNum.ToString();
                Label1.Text = "New number for this Room:";
                chkIncludePic.Left = 8;
                chkIncludePic.Top = 161;
                chkIncludePic.Visible = true;
                chkIncludePic.Checked = true;
                chkIncludePic.Text = "Also renumber room picture";
                lstResNum.Height = 90;
                // nothing is selected
                lstResNum.SelectedIndex = -1;
                break;
            }
        }

        void tmpForm() {
            /*
      Option Explicit


        tmpView As AGIView, tmpLogic As AGILogic
        tmpSound As AGISound, tmpPicture As AGIPicture
        tmpCol As Collection
        tmpRes As Object


      public Sub Activate()
        'bridge method to call the form's Activate event method
        Form_Activate
      End Sub

      Private Sub cmdCancel_Click()

        'user canceled
        Canceled = True

        Me.Hide
      End Sub

      Private Sub cmdDont_Click()

        'not canceled, but not importing
        Canceled = False
        DontImport = True
        Me.Hide
      End Sub


      Private Sub cmdOK_Click()

        'save the current 'opennew' value
         WriteSetting GameSettings, sGENERAL, "OpenNew", (chkOpenRes.Value = vbChecked)

        Me.Hide
      End Sub

      Private Sub Form_Activate()

      '''  'set focus to resnum list
      '''  lstResNum.SetFocus
      End Sub

      Private Sub Form_Deactivate()

        'make sure all objects are de-referenced, otherwise
        'form will not unload! it'll just sit there and fester

        'and when the load event is called, it doesn't load,
        'it just references the festering version with all
        'it's crapped up settings and things...


        Set tmpView = Nothing
        Set tmpLogic = Nothing
        Set tmpSound = Nothing
        Set tmpPicture = Nothing
        Set tmpCol = Nothing
        Set tmpRes = Nothing
      End Sub


      Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

        'check for help key
        If Shift = 0 And KeyCode = vbKeyF1 Then
          HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Managing Resources.htm#resnum"
          KeyCode = 0
        End If
      End Sub


      Private Sub lstResNum_Click()

        'enable ok button
        cmdOK.Enabled = True

        'if adding or importing a new resource
        Select Case WindowFunction
        Case grAddLayout, grAddNew, grAddInGame, grImport
      '  If (WindowFunction = grAddLayout Or WindowFunction = grAddNew Or WindowFunction = grImport) Then
          'if format is restype and resnum (i.e., "Logic0") OR is blank
          If LenB(txtID.Text) = 0 Or (Left$(txtID.Text, Len(ResTypeName(ResType))) = ResTypeName(ResType)) Then
            'update id box
            txtID.Text = ResTypeName(ResType) & lstResNum.Text
          End If

          'if adding a room (logic, with matching pic) check for existing pic
          If WindowFunction = grAddLayout Or ((WindowFunction = grAddNew Or WindowFunction = grImport) And ResType = rtLogic) Then

            'if this pic number exists,
            If Pictures.Exists(CByte(lstResNum.Text)) Then
              'change checkbox text
              Me.chkIncludePic.Caption = "Replace existing Picture"
            Else
              chkIncludePic.Caption = "Create matching Picture"
            End If
          End If

          'never allow adding pic for room 0!
          If lstResNum.Text = "0" Then
            chkIncludePic.Enabled = False
            chkIncludePic.Value = vbUnchecked
            chkRoom.Enabled = False
            chkRoom.Value = vbUnchecked
          Else
            chkIncludePic.Enabled = True
            chkRoom.Enabled = (WindowFunction <> grAddLayout)
          End If
        End Select
      End Sub

      Private Sub lstResNum_DblClick()
        'same as clicking OK
        cmdOK_Click

      End Sub

      Private Sub txtID_Change()

        'enable ok only if id is Not null AND a valid number is selected
        cmdOK.Enabled = LenB(txtID.Text) <> 0 And lstResNum.ListIndex <> -1
      End Sub


      Private Sub txtID_KeyPress(KeyAscii As Integer)

        'some characters not allowed:

      'NOT OK  x!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
      '    OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    

        Select Case KeyAscii
        Case 32 To 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, Is >= 123
          KeyAscii = 0
        End Select
      End Sub
            */
        }

        private void btnOK_Click(object sender, EventArgs e) {
            int rtn;
            string strErrMsg = "";
            // get resnum
            NewResNum = ((ListItemData)lstResNum.SelectedItem).ResNum;
            switch (WindowFunction) {
            case grRenumber or grRenumberRoom:
                if (OldResNum == NewResNum) {
                    // same as canceling
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
                break;
            case grAddNew or grAddInGame or grImport:
                // validate resourceID (use impossible value for old ID to avoid matching
                rtn = ValidateID(txtID.Text, 255.ToString());
                switch (rtn) {
                case 0:
                    // ok
                    break;
                case 1:
                    // no ID
                    strErrMsg = "Resource ID cannot be blank.";
                    break;
                case 2:
                    // ID is numeric
                    strErrMsg = "Resource IDs cannot be numeric.";
                    break;
                case 3:
                    // ID is command
                    strErrMsg = "'" + txtID.Text + "' is an AGI command, and cannot be used as a resource ID.";
                    break;
                case 4:
                    // ID is test command
                    strErrMsg = "'" + txtID.Text + "' is an AGI test command, and cannot be used as a resource ID.";
                    break;
                case 5:
                    // ID is a compiler keyword
                    strErrMsg = "'" + txtID.Text + "' is a compiler reserved word, and cannot be used as a resource ID.";
                    break;
                case 6:
                    // ID is an argument marker
                    strErrMsg = "Resource IDs cannot be argument markers";
                    break;
                case 14:
                    // ID contains improper character
                    strErrMsg = "Invalid character in resource ID: " + Environment.NewLine + "!\"&'()*+,-/:;<=>?[\\]^`{|}~ and spaces" + Environment.NewLine + "are not allowed.";
                    break;
                case 15:
                    // ID matches existing ResourceID
                    // ingame is presumed true
                    strErrMsg = "'" + txtID.Text + "' is already in use as a resource ID.";
                    break;
                }

                // if there is an error
                if (rtn != 0) {
                    // error - show msgbox
                    MessageBox.Show(strErrMsg, "Invalid Resource ID", MessageBoxButtons.OK, MessageBoxIcon.Information); // vbMsgBoxHelpButton, WinAGIHelp, "htm\winagi\Managing Resources.htm#resourceids"
                    // send user back to the form to try again
                    txtID.Select();
                    return;
                }
                else {
                    // OK to exit
                    DialogResult = DialogResult.Cancel;
                }
                break;
            default:
                // ok to close form
                DialogResult = DialogResult.Cancel;
                break;
            }
            // done
            Close();
        }

        private void frmGetResourceNum_FormClosed(object sender, FormClosedEventArgs e) {
            // save the current 'opennew' value
            WinAGISettingsList.WriteSetting(sGENERAL, "OpenNew", (chkOpenRes.Checked == true));
        }

        private void lstResNum_SelectedIndexChanged(object sender, EventArgs e) {
            btnOK.Enabled = lstResNum.SelectedIndex != -1;
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
