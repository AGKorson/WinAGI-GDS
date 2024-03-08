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
        public bool Canceled;
        public bool DontImport;
        public EGetRes WindowFunction;
        public frmGetResourceNum()
        {
            InitializeComponent();
        }
        public void FormSetup()
        {
            byte i;
            int lngOldNum;
            string strID;
            Dictionary<byte, string> tmpCol = [];

            //clear the listbox
            lstResNum.Items.Clear();

            switch (WindowFunction) {
            case grAddNew or grAddInGame or grRenumber or grAddLayout or grImport:
                //add all numbers to temporary collection

                for (i = 0; i <= 255; i++) {
                    tmpCol.Add(i, i.ToString());
                }
                // if adding rooms in layout, always SUGGEST include pic
                if (WindowFunction == grAddLayout) {
                    chkIncludePic.Checked = true;
                    chkIncludePic.Enabled = true;
                }
                // remove all resource numbers that are in use
                switch (ResType) {
                case rtView:
                    if (EditGame.Views.Count > 0) {
                        foreach (Engine.View tmpView in EditGame.Views) {
                            tmpCol.Remove(tmpView.Number);
                        }
                    }
                    break;
                case rtPicture:
                    if (EditGame.Pictures.Count > 0) {
                        foreach (Picture tmpPicture in EditGame.Pictures) {
                            tmpCol.Remove(tmpPicture.Number);
                        }
                    }
                    break;
                case rtSound:
                    if (EditGame.Sounds.Count > 0) {
                        foreach (Sound tmpSound in EditGame.Sounds) {
                            tmpCol.Remove(tmpSound.Number);
                        }
                    }
                    break;
                case rtLogic:
                    if (EditGame.Logics.Count > 0) {
                        foreach (Logic tmpLogic in EditGame.Logics) {
                            tmpCol.Remove(tmpLogic.Number);
                        }
                    }
                    // if adding rooms in logic/picture pairs
                    // if adding to layout room 0 is NEVER available
                    if (chkIncludePic.Checked == true || WindowFunction == grAddLayout) {
                        // ensure logic 0 is removed
                        tmpCol.Remove(0);
                    }
                    break;
                }
                // add remaining numbers to the combobox
                foreach (KeyValuePair<byte, string> tmpKVP in tmpCol) {
                    lstResNum.Items.Add(tmpKVP);
                }
                break;
            case grOpen or grTestView or grMenu or grMenuBkgd:
                // to ensure resources are sorted by number, use the tmpcollection;
                // only add resources that are in game
                switch (ResType) {
                case rtLogic:
                    for (i = 0; i <= 255; i++) {
                        if (EditGame.Logics.Exists(i)) {
                            if (Settings.ShowResNum) {
                                tmpCol.Add(i, ResourceName(EditGame.Logics[i], true));
                            }
                            else {
                                tmpCol.Add(i, i + " - " + EditGame.Logics[i].ID);
                            }
                        }
                    }
                    // step through collection to add numbers to list
                    foreach (KeyValuePair<byte, string> tmpKVP in tmpCol) {
                        lstResNum.Items.Add(tmpKVP);
                    }
                    break;
                case rtPicture:
                    for (i = 0; i <= 255; i++) {
                        if (EditGame.Pictures.Exists(i)) {
                            if (Settings.ShowResNum) {
                                tmpCol.Add(i, ResourceName(EditGame.Pictures[i], true));
                            }
                            else {
                                tmpCol.Add(i, i + " - " + EditGame.Pictures[i].ID);
                            }
                        }
                    }
                    // step through collection to add numbers to list
                    foreach (KeyValuePair<byte, string> tmpKVP in tmpCol) {
                        lstResNum.Items.Add(tmpKVP);
                    }
                    break;
                case rtSound:
                    for (i = 0; i <= 255; i++) {
                        if (EditGame.Sounds.Exists(i)) {
                            if (Settings.ShowResNum) {
                                tmpCol.Add(i, ResourceName(EditGame.Sounds[i], true));
                            }
                            else {
                                tmpCol.Add(i, i + " - " + EditGame.Sounds[i].ID);
                            }
                        }
                    }
                    // step through collection again to add numbers to list
                    foreach (KeyValuePair<byte, string> tmpKVP in tmpCol) {
                        lstResNum.Items.Add(tmpKVP);
                    }
                    break;
                case rtView:
                    for (i = 0; i <= 255; i++) {
                        if (EditGame.Views.Exists(i)) {
                            if (Settings.ShowResNum) {
                                tmpCol.Add(i, ResourceName(EditGame.Views[i], true));
                            }
                            else {
                                tmpCol.Add(i, i + " - " + EditGame.Views[i].ID);
                            }
                        }
                    }
                    // step through collection and add numbers to list
                    foreach (KeyValuePair<byte, string> tmpKVP in tmpCol) {
                        lstResNum.Items.Add(tmpKVP);
                    }
                    break;
                }
                break;
            case grShowRoom:
                // displays all logics in game which do NOT have InRoom=True
                for (i = 1; i <= 255; i++) {
                    if (EditGame.Logics.Exists(i)) {
                        if (!EditGame.Logics[i].IsRoom) {
                            if (Settings.ShowResNum) {
                                tmpCol.Add(i, ResourceName(EditGame.Logics[i], true));
                            }
                            else {
                                tmpCol.Add(i, i + " - " + EditGame.Logics[i].ID);
                            }
                        }
                    }
                }
                // add items from tmpCol to listbox
                for (i = 1; i <= tmpCol.Count; i++) {
                    // lstResNum.ItemData(lstResNum.ListCount - 1) = tmpCol(i)
                }
                foreach (KeyValuePair<byte, string> tmpKVP in tmpCol) {
                    lstResNum.Items.Add(tmpKVP);
                }
                    // if no rooms added
                    if (lstResNum.Items.Count == 0) {
                    MessageBox.Show("All logics in the game are currently tagged as rooms and are visible."); //, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Show Room", WinAGIHelp, "htm\winagi\Managing_Resources.htm#resourceids"
                    // set cancel to true
                    Canceled = true;
                    // close form
                }
                else {
                    Canceled = false;
                }
                break;
            }
            // set form size and controls
            AdjustControls();
            // assume cancel
            Canceled = true;
            // and not 'dont import'
            DontImport = false;
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

        Dim rtn As Long, strErrMsg As String

        'get resnum
        NewResNum = CByte(lstResNum.ItemData(lstResNum.ListIndex))

        Select Case WindowFunction
        Case grRenumber
          If (OldResNum = NewResNum) Then
            'same as canceling, then close form
            Canceled = True
          Else
            'OK to close form
            Canceled = False
          End If

        Case grAddNew, grAddInGame, grImport

          'validate resourceID (use impossible value for old ID to avoid matching
          rtn = ValidateID(txtID.Text, Chr$(255))

          Select Case rtn
          Case 0 'ok
          Case 1 ' no ID
            strErrMsg = "Resource ID cannot be blank."
          Case 2 ' ID is numeric
            strErrMsg = "Resource IDs cannot be numeric."
          Case 3 ' ID is command
            strErrMsg = ChrW$(39) & txtID.Text & "' is an AGI command, and cannot be used as a resource ID."
          Case 4 ' ID is test command
            strErrMsg = ChrW$(39) & txtID.Text & "' is an AGI test command, and cannot be used as a resource ID."
          Case 5 ' ID is a compiler keyword
            strErrMsg = ChrW$(39) & txtID.Text & "' is a compiler reserved word, and cannot be used as a resource ID."
          Case 6 ' ID is an argument marker
            strErrMsg = "Resource IDs cannot be argument markers"
          Case 14 ' ID contains improper character
            strErrMsg = "Invalid character in resource ID: & vbnewline & !" & QUOTECHAR & "&'()*+,-/:;<=>?[\]^`{|}~ and spaces" & vbNewLine & "are not allowed."
          Case 15 ' ID matches existing ResourceID
            'ingame is presumed true
            Debug.Assert GameLoaded
            strErrMsg = ChrW$(39) & txtID.Text & "' is already in use as a resource ID."
          End Select

          'if there is an error
          If rtn <> 0 Then
            'error - show msgbox
            MsgBoxEx strErrMsg, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Invalid Resource ID", WinAGIHelp, "htm\winagi\Managing Resources.htm#resourceids"
            'send user back to the form to try again
            txtID.SelStart = 0
            txtID.SelLength = Len(txtID.Text)
            txtID.SetFocus
            Exit Sub
          Else
            'OK to exit
            Canceled = False
          End If
        Case Else
          'ok to close form
          Canceled = False
        End Select


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

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
