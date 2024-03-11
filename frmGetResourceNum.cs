using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WinAGI.Editor.Base;
using WinAGI.Engine;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Editor.Base.EGetRes;
using EnvDTE;

namespace WinAGI.Editor
{
    public partial class frmGetResourceNum : Form
    {
        public AGIResType ResType;
        public byte NewResNum;
        public byte OldResNum;
        public bool Canceled = true;
        public bool DontImport = false;
        public EGetRes WindowFunction;

        class ListItemData {
            public byte ResNum = 0;
            public string ID = "";
            public override string ToString() { return ID; }
        }

        public frmGetResourceNum()
        {
            InitializeComponent();
        }
        public void FormSetup()
        {
            int i;
            ListItemData tmpItem;

            //clear the listbox
            lstResNum.Items.Clear();

            switch (WindowFunction) {
            case grAddNew or grAddInGame or grRenumber or grAddLayout or grImport:
                // if adding rooms in layout, always SUGGEST include pic
                if (WindowFunction == grAddLayout) {
                    chkIncludePic.Checked = true;
                    chkIncludePic.Enabled = true;
                }
                // only add  resource numbers that are NOT in use
                for (i = 0; i < 256; i++) {
                    tmpItem = new ListItemData
                    {
                        ResNum = (byte)i,
                        ID = i.ToString()
                    };
                    switch (ResType) {
                    case rtLogic:
                        // if exists, don't add
                        // if grAddLayout AND i == 0, don't add
                        // if grRenumberRoom AND Picture exists, don't add
                        if (!EditGame.Logics.Exists((byte)i) &&
                            (WindowFunction != grAddLayout || i != 0) &&
                            (WindowFunction != grRenumberRoom || !EditGame.Pictures.Exists((byte)i))) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case rtView:
                        if (!EditGame.Views.Exists((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case rtPicture:
                        if (!EditGame.Pictures.Exists((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case rtSound:
                        if (EditGame.Sounds.Exists((byte)i)) {
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    }
                }
                break;
            case grOpen or grTestView or grMenu or grMenuBkgd:
                // only add resources that are in game
                for (i = 0; i < 256; i++) {
                    tmpItem = new()
                    {
                        ResNum = (byte)i
                    };
                    switch (ResType) {
                    case rtLogic:
                        if (EditGame.Logics.Exists((byte)i)) {
                            tmpItem.ID = Settings.ShowResNum
                                ? ResourceName(EditGame.Logics[i], true)
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }

                        break;
                    case rtPicture:
                        if (EditGame.Pictures.Exists((byte)i)) {
                            tmpItem.ID = Settings.ShowResNum
                                ? ResourceName(EditGame.Pictures[i], true)
                                : i + " - " + EditGame.Pictures[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case rtSound:
                        if (EditGame.Sounds.Exists((byte)i)) {
                            tmpItem.ID = Settings.ShowResNum
                                ? ResourceName(EditGame.Sounds[i], true)
                                : i + " - " + EditGame.Sounds[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                        break;
                    case rtView:
                        if (EditGame.Views.Exists((byte)i)) {
                            tmpItem.ID = Settings.ShowResNum
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
                    tmpItem = new()
                    {
                        ResNum = (byte)i
                    };
                    if (EditGame.Logics.Exists((byte)i)) {
                        if (!EditGame.Logics[i].IsRoom) {
                            tmpItem.ID = Settings.ShowResNum 
                                ? ResourceName(EditGame.Logics[i], true) 
                                : i + " - " + EditGame.Logics[i].ID;
                            lstResNum.Items.Add(tmpItem);
                        }
                    }
                }
                // if no rooms added
                if (lstResNum.Items.Count == 0) {
                    MessageBox.Show("All logics in the game are currently tagged as rooms and are visible.", "Show Room",MessageBoxButtons.OK,MessageBoxIcon.Information); //vbMsgBoxHelpButton, , WinAGIHelp, "htm\winagi\Managing_Resources.htm#resourceids"
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
            /*

            'shows/hides form controls and adjusts
            'form height and control positions based on window function

            'defaults when form is initially loaded:
            ' size wide (6645)
            'Label2/lblCurrent are visible
            'Label1 and lstResNum in lower position (825/1065/1425)
            'chkRoom, chkPic, chkOpenRes are not visible
            'cmdDont is not visible
            'cmdCancel is visible at left position, with caption of 'cancel' (1620)
            'form caption needs to be set!
            'list caption (Label1) needs to be set!
            'cmdOK disabled

            Dim i As Long

            Select Case WindowFunction
            Case grAddNew
              Me.Caption = "Add New " & ResTypeName(ResType)
              lblCurrent.Visible = False
              Label2.Visible = False
              Label1.Top = 120
              Label1.Caption = "Select a number for this new " & ResTypeName(ResType) & ":"
              lstResNum.Top = 360
              lstResNum.Height = 2130
              cmdDont.Visible = True
              cmdDont.Caption = "New Not in Game"
              cmdCancel.Left = 3270
              If ResType = rtLogic Then
                With chkIncludePic
                  .Value = vbChecked
                  .Visible = True
                  .Top = 2100
                  .Enabled = False
                End With
                With chkRoom
                  .Value = vbChecked
                  .Visible = True
                  .Enabled = True
                  .Top = 2340
                  .Caption = "Include Room Template Code"
                End With
                txtDescription.Height = 1050
              Else
                chkIncludePic.Visible = False
                chkRoom.Visible = False
              End If

              chkOpenRes.Visible = True
              chkOpenRes.Value = (Settings.OpenNew And vbChecked)

              'nothing is selected
              lstResNum.ListIndex = -1

            Case grAddInGame
              Me.Caption = "Add " & ResTypeName(ResType)
              lblCurrent.Visible = False
              Label2.Visible = False
              Label1.Top = 120
              Label1.Caption = "Select a number for this " & ResTypeName(ResType) & ":"
              lstResNum.Top = 360
              lstResNum.Height = 2130
              cmdDont.Visible = False

              'nothing is selected
              lstResNum.ListIndex = -1

            Case grRenumber
              Me.Width = 3090
              Select Case ResType
              Case rtLogic
                Me.Caption = ResourceName(Logics(OldResNum), True, True)
              Case rtPicture
                Me.Caption = ResourceName(Pictures(OldResNum), True, True)
              Case rtSound
                Me.Caption = ResourceName(Sounds(OldResNum), True, True)
              Case rtView
                Me.Caption = ResourceName(Views(OldResNum), True, True)
              End Select
              lblCurrent.Caption = CStr(OldResNum)
              Label1.Caption = "Select a new number for this " & ResTypeName(ResType) & ":"

              'nothing is selected
              lstResNum.ListIndex = -1

           Case grOpen
              Me.Width = 3090
              Me.Caption = "Open " & ResTypeName(ResType)
              lblCurrent.Visible = False
              Label2.Visible = False
              Label1.Top = 120
              Label1.Caption = "Select the " & ResTypeName(ResType) & " to open:"
              lstResNum.Top = 360
              lstResNum.Height = 2130

              'nothing is selected
              lstResNum.ListIndex = -1

            Case grTestView
              Me.Width = 3090
              Me.Caption = "Select Test View"
              lblCurrent.Caption = CStr(OldResNum)
              Label1.Caption = "Select a view for testing pictures:"

              'select current test view, if there is one
              lstResNum.ListIndex = -1
              For i = 0 To lstResNum.ListCount - 1
                If lstResNum.ItemData(i) = OldResNum Then
                  lstResNum.ListIndex = i
                  Exit For
                End If
              Next i

            Case grAddLayout
              Me.Caption = "Add New Room"
              lblCurrent.Visible = False
              Label2.Visible = False
              Label1.Top = 120
              Label1.Caption = "Select a number for this new room:"
              lstResNum.Top = 360
              lstResNum.Height = 2130
              chkIncludePic.Visible = True
              chkIncludePic.Value = vbChecked
              chkRoom.Visible = True
              chkRoom.Value = vbChecked
              chkRoom.Enabled = False

              'nothing is selected
              lstResNum.ListIndex = -1

            Case grShowRoom
              Me.Width = 3090
              Me.Caption = "Show Room"
              lblCurrent.Visible = False
              Label2.Visible = False
              Label1.Top = 120
              Label1.Caption = "Select a room to show:"
              lstResNum.Top = 360
              lstResNum.Height = 2130

              'nothing is selected
              lstResNum.ListIndex = -1

            Case grMenu
              Me.Width = 3090
              Me.Caption = "Extract Menu"
              lblCurrent.Visible = False
              Label2.Visible = False
              Label1.Top = 120
              Label1.Caption = "Select the logic with the game's menu:"
              lstResNum.Top = 360
              lstResNum.Height = 2130

              'nothing is selected
              lstResNum.ListIndex = -1

            Case grImport
              Me.Caption = "Import " & ResTypeName(ResType)
              lblCurrent.Visible = False
              Label2.Visible = False
              Label1.Top = 120
              Label1.Caption = "Select a number for this imported " & ResTypeName(ResType) & ":"
              lstResNum.Top = 360
              lstResNum.Height = 2130
              cmdDont.Visible = True
              cmdCancel.Left = 3270
              If ResType = rtLogic Then
                With chkIncludePic
                  .Visible = False
                End With
                With chkRoom
                  .Value = vbChecked
                  .Visible = True
                  .Enabled = False
                  .Top = 2340
                  .Caption = "Import as a Room"
                End With
                txtDescription.Height = 1050
              Else
                chkIncludePic.Visible = False
                chkRoom.Visible = False
              End If

              chkOpenRes.Visible = True
              chkOpenRes.Value = (Settings.OpenNew And vbChecked)

              'nothing is selected
              lstResNum.ListIndex = -1

            Case grMenuBkgd
              Me.Width = 3090
              Caption = "Menu Background"
              lblCurrent.Caption = CStr(OldResNum)
              Label1.Caption = "Select picture to use as background:"
              'select the current pic
              lstResNum.ListIndex = -1
              For i = 0 To lstResNum.ListCount - 1
                If lstResNum.ItemData(i) = OldResNum Then
                  lstResNum.ListIndex = i
                  Exit For
                End If
              Next i

            End Select
            */
        }

        void tmpForm()
        {
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
                    Canceled = true;
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
                    // txtID.SelStart = 0
                    // txtID.SelLength = Len(txtID.Text)
                    // txtID.SetFocus
                    return;
                }
                else {
                    // OK to exit
                    Canceled = false;
                }
                break;
            default:
                // ok to close form
                Canceled = false;
                break;
            }
            // save the current 'opennew' value
            GameSettings.WriteSetting(sGENERAL, "OpenNew", (chkOpenRes.Checked == true));
            // done
            Close();
        }
    }
}
