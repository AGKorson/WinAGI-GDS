using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmReserved : Form {
        public frmReserved() {
            InitializeComponent();
        }

        Font boldfont;
        ReservedDefineList EditList;

        private void frmReserved_Load(object sender, EventArgs e) {
            Font commonFont = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            fgReserved.Font = commonFont;
            fgReserved.ColumnHeadersDefaultCellStyle.Font = commonFont;
            fgReserved.AlternatingRowsDefaultCellStyle.Font = commonFont;
            fgReserved.AlternatingRowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            boldfont = new(commonFont, FontStyle.Bold);
            DataGridViewRow template = new();
            template.CreateCells(fgReserved);
            template.Cells[0].Value = "";
            template.Cells[1].Value = "";
            fgReserved.RowTemplate = template;
            if (EditGame == null) {
                EditList = Engine.Base.DefaultReservedDefines;
            }
            else {
                EditList = EditGame.ReservedDefines;
            }
            LoadGrid(EditList);
        }

        private void btnSave_Click(object sender, EventArgs e) {
            Hide();
        }

        private void btnReset_Click(object sender, EventArgs e) {
            MessageBox.Show("reset!");
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            Hide();
        }

        private void LoadGrid(ReservedDefineList gridlist) {
            TDefine[] tmpDefines;
            int currentrow;
            // load by group, not by data type!
            // (the returned array is actually a copy, not the actual array of defines)

            // RESERVED VARIABLES
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Reserved Variables";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ReservedVariables;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                } else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.Variable;
                fgReserved[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            fgReserved.Rows[currentrow].ReadOnly = true;
            // RESERVED FLAGS
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Reserved Flags";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ReservedFlags;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.Flag;
                fgReserved[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            fgReserved.Rows[currentrow].ReadOnly = true;
            // EDGECODES
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Edge Code Values";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.EdgeCodes;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.EdgeCode;
                fgReserved[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            fgReserved.Rows[currentrow].ReadOnly = true;
            // OBJ DIRECTION
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Obj Direction Values";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ObjDirections;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.ObjectDir;
                fgReserved[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            fgReserved.Rows[currentrow].ReadOnly = true;
            // VIDEO MODES
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Video Modes";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.VideoModes;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.VideoMode;
                fgReserved[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            fgReserved.Rows[currentrow].ReadOnly = true;
            // COMPUTER TYPES
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Computer Types";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ComputerTypes;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.ComputerType;
                fgReserved[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            fgReserved.Rows[currentrow].ReadOnly = true;
            // COLORS
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Colors";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            tmpDefines = gridlist.ColorNames;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.Color;
                fgReserved[1, currentrow].Tag = i;
            }
            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            fgReserved.Rows[currentrow].ReadOnly = true;
            // OTHER RESERVED DEFINES
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 0;
            fgReserved[0, currentrow].Value = "Other Reserved Defines";
            fgReserved[0, currentrow].Style.Font = boldfont;
            fgReserved[0, currentrow].ReadOnly = true;
            //ego
            tmpDefines = gridlist.ReservedObjects;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.String;
                fgReserved[1, currentrow].Tag = i;
            }
            // input prompt
            tmpDefines = gridlist.ReservedStrings;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.String;
                fgReserved[1, currentrow].Tag = i;
            }

            // game info (id, version, about, invcount)
            tmpDefines = gridlist.GameInfo;
            for (int i = 0; i < tmpDefines.Length; i++) {
                currentrow = fgReserved.Rows.Add();
                fgReserved[0, currentrow].Value = tmpDefines[i].Name;
                fgReserved[1, currentrow].Value = tmpDefines[i].Value;
                if (tmpDefines[i].Name == tmpDefines[i].Default) {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Black;
                }
                else {
                    fgReserved[0, currentrow].Style.ForeColor = Color.Red;
                }
                fgReserved[1, currentrow].Style.ForeColor = Color.Gray;
                // mark name with the group, value with index
                fgReserved[0, currentrow].Tag = ResDefGroup.String;
                fgReserved[1, currentrow].Tag = i;
            }

            // add a blank row
            currentrow = fgReserved.Rows.Add();
            fgReserved.Rows[currentrow].Tag = 1;
            // select first item
            fgReserved[0, 1].Selected = true;
        }

        /*
        
        Option Explicit

  Public Canceled As Boolean
  
  Private SplitOffset As Single
  
  Private EditRow As Long
  Private EditDefine As TDefine
  
  Private CellIndentH As Single
  Private CellIndentV As Single
  
  
Public Sub MenuClickCopy()

  'if editing
  If txtEditName.Visible Then
    Exit Sub
  End If
  
  'copies the selected cell or the selected rows to the clipboard
  
  Dim i As Long, strData As String
  Dim TopRow As Long, BtmRow As Long
  
  With fgReserved
    If fgReserved.SelectionMode = flexSelectionByRow Then
      'select all the rows
      'ensure starting at top
      If fgReserved.Row < fgReserved.RowSel Then
        TopRow = fgReserved.Row
        BtmRow = fgReserved.RowSel
      Else
        TopRow = fgReserved.RowSel
        BtmRow = fgReserved.Row
      End If
      
      'add first row
      strData = fgReserved.TextMatrix(TopRow, 1) & vbTab & fgReserved.TextMatrix(TopRow, 2)
      'add remaining rows, with carriage return separator
      For i = TopRow + 1 To BtmRow
        'add text
        strData = strData & vbCr & fgReserved.TextMatrix(i, 1) & vbTab & fgReserved.TextMatrix(i, 2)
      Next i
    Else
      'select just this cell
      strData = fgReserved.TextMatrix(.Row, fgReserved.Col)
    End If
  
    'put on clipboard
    Clipboard.Clear
    Clipboard.SetText strData
    
    'enable pasting, if selection contained entire row
    frmMDIMain.mnuEPaste.Enabled = (.SelectionMode = flexSelectionByRow)
  End With
End Sub


Public Sub MenuClickHelp()
  
  On Error GoTo ErrHandler
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\reservednames.htm#editor"
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickPaste()

  'no paste function for this form
  
End Sub
Private Function ValidateName(NewDefName As String) As Long
  'validates if a define name is agreeable or not
  'returns zero if ok;
  'error Value if not
  
  '1 = no name
  '2 = name is numeric
  '3 = name is command
  '4 = name is test command
  '5 = name is a compiler keyword
  '6 = name is an argument marker
  '7 = name is already used as a reserved define name '''globally defined
'''  '8 = name is reserved variable name
'''  '9 = name is reserved flag name
'''  '10 = name is reserved number constant
'''  '11 = name is reserved object constant
'''  '12 = name is reserved message constant
'''  '13 = name is reserved string constant
  '14 = name contains improper character
    
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'if no name,
  If LenB(NewDefName) = 0 Then
    ValidateName = 1
    Exit Function
  End If
  
  'name cant be numeric
  If IsNumeric(NewDefName) Then
    ValidateName = 2
    Exit Function
  End If
  
  'check against regular commands
  For i = 0 To Commands.Count
    If NewDefName = Commands(i).Name Then
      ValidateName = 3
      Exit Function
    End If
  Next i
  
  'check against test commands
  For i = 0 To TestCommands.Count
    If StrComp(NewDefName, TestCommands(i).Name, vbTextCompare) = 0 Then
      ValidateName = 4
      Exit Function
    End If
  Next i
  
  'check against keywords
  Select Case LCase$(NewDefName)
  Case "if", "else", "goto"
    ValidateName = 5
    Exit Function
  End Select
      
  'check against variable/flag/controller/string/message names
  Select Case Asc(LCase$(NewDefName))
  '     v    f    m    o    i    s    w    c
  Case 118, 102, 109, 111, 105, 115, 119, 99
    If IsNumeric(Right$(NewDefName, Len(NewDefName) - 1)) Then
      ValidateName = 6
      Exit Function
    End If
  End Select
  
  'check against existing globals (skip editrow)
  For i = 1 To fgReserved.Rows - 1
    If (StrComp(NewDefName, fgReserved.TextMatrix(i, 1), vbTextCompare) = 0) And i <> EditRow Then
      ValidateName = 7
      Exit Function
    End If
  Next i
  
  'check name against improper character list
  For i = 1 To Len(NewDefName)
    Select Case Asc(Mid$(NewDefName, i, 1))
'                                                                            1         1         1
'        3       4    4    5         6         7         8         9         0         1         2
'        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
'NOT OK  x!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
'    OK     #$%        fgReserved. 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
    Case 32 To 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, Is >= 123
      ValidateName = 14
      Exit Function
    End Select
  Next i
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Private Function ValidateInput() As Boolean
  
  Dim rtn As Long
  Dim strText As String
  Dim strErrMsg As String
  Dim lngRow As Long, lngCol As Long
  
  'checks input for new name
  On Error GoTo ErrHandler
  
  'if no change made, that's OK
  '(name in textbox matches previous name)
  If txtEditName.Text = EditDefine.Name Then
    'no change made; return success
    ValidateInput = True
    Exit Function
  End If
  
  'if blank, restore default
  If Len(txtEditName.Text) = 0 Then
    strText = LogicSourceSettings.ResDefByGrp(fgReserved.RowData(EditRow) \ 256)(fgReserved.RowData(EditRow) Mod 256).Default
  Else
    'make local copy of name
    strText = txtEditName.Text
  End If
  
  'validate the define name
  rtn = ValidateName(strText)
  
  'if return code indicates error
  If rtn <> 0 Then
    Select Case rtn
    Case 1 ' no name
      strErrMsg = "Define name cannot be blank."
      
    Case 2 ' name is numeric
      strErrMsg = "Define names cannot be numeric."
      
    Case 3 ' name is command
      strErrMsg = ChrW$(39) & strText & "' is an AGI command, and cannot be redefined"
      
    Case 4 ' name is test command
      strErrMsg = ChrW$(39) & strText & "' is an AGI test command, and cannot be redefined"
      
    Case 5 ' name is a compiler keyword
      strErrMsg = ChrW$(39) & strText & "' is a compiler reserved word, and cannot be redefined"
      
    Case 6 ' name is an argument marker
      strErrMsg = "invalid define statement - define names cannot be argument markers"
      
    Case 7 ' name is already reserved
      strErrMsg = ChrW$(39) & strText & "' is already in use as a reserved define"
      
    Case 14 ' name contains improper character
      strErrMsg = "Invalid character in define name: !" & QUOTECHAR & "&'()*+,-/:;<=>?[\]^`{|}~ and spaces are not allowed"
            
    End Select
  End If
  
  'if still an error code
  If rtn <> 0 Then
    'force back to edit row
    fgReserved.Row = EditRow
    
    'show msgbox
    MsgBoxEx strErrMsg, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Edit Reserved Defines Error", WinAGIHelp, "htm\commands\syntax.htm#defines"
    Exit Function
  Else
    'save current row/col;
    With fgReserved
      lngRow = fgReserved.Row
      lngCol = fgReserved.Col
      
      'put the text back into grid
      fgReserved.TextMatrix(EditRow, 1) = strText
      fgReserved.Row = EditRow
      fgReserved.Col = 1
      If strText = LogicSourceSettings.ResDefByGrp(fgReserved.RowData(EditRow) \ 256)(fgReserved.RowData(EditRow) Mod 256).Default Then
        'black if default name
        fgReserved[0, currentrow].Style.ForeColor = Color.Black
      Else
        'red if changed
        fgReserved[0, currentrow].Style.ForeColor = Color.Red
      End If
      'restore row/col
      fgReserved.Row = lngRow
      fgReserved.Col = lngCol
    End With
  End If
  
  'valid;
  ValidateInput = True
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub cmdCancel_Click()

  'if esc is pressed, we go here regardless of what's happening;
  'so we need to check for editing - if that's the case, we
  'don't really want to exit, we just want to stop editing
  If txtEditName.Visible Then
    'stop editing (by hiding the text box
    txtEditName.Visible = False
    fgReserved.SetFocus
    Exit Sub
  End If
  
  'set cancel flag
  Me.Canceled = True
  Me.Hide
End Sub

Private Sub cmdReset_Click()

  'restores all reserved defines to their original value
  LogicSourceSettings.ResetResDefines
  
  'reload to reflect the change
  Me.fgReserved.Visible = False
  LoadReservedNames
  Me.fgReserved.Visible = True
End Sub

Private Sub cmdSave_Click()

  'save the results!
  Dim i As Long, rtn As VbMsgBoxResult
  Dim blnDontAsk As Boolean
  
  On Error Resume Next
  
  'variables
  For i = 0 To 26
    LogicSourceSettings.ResDef(1, i) = fgReserved.TextMatrix(i + 2, 1)
  Next i
  
  'flags
  For i = 0 To 17
    LogicSourceSettings.ResDef(2, i) = fgReserved.TextMatrix(i + 31, 1)
  Next i
  
  'edgecodes
  For i = 0 To 4
    LogicSourceSettings.ResDef(3, i) = fgReserved.TextMatrix(i + 51, 1)
  Next i
  
  'directions
  For i = 0 To 8
    LogicSourceSettings.ResDef(4, i) = fgReserved.TextMatrix(i + 58, 1)
  Next i
  
  'vidmodes
  For i = 0 To 4
    LogicSourceSettings.ResDef(5, i) = fgReserved.TextMatrix(i + 69, 1)
  Next i
  
  'comptypes
  For i = 0 To 8
    LogicSourceSettings.ResDef(6, i) = fgReserved.TextMatrix(i + 76, 1)
  Next i
  
  'colors
  For i = 0 To 15
    LogicSourceSettings.ResDef(7, i) = fgReserved.TextMatrix(i + 87, 1)
  Next i
  
  'others
  For i = 0 To 5
    LogicSourceSettings.ResDef(8, i) = fgReserved.TextMatrix(i + 105, 1)
  Next i
  
  'replace any changed defines with new names
  Select Case Settings.AutoUpdateResDefs
  Case 0
    'get user's response
    rtn = MsgBoxEx("Do you want to update all logics with any reserved define names that have been changed?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Update Logics?", WinAGIHelp, "htm\winagi\editingdefines.htm#edit", "Always take this action when saving the reserved defines list.", blnDontAsk)
    If blnDontAsk Then
      If rtn = vbYes Then
        Settings.AutoUpdateResDefs = 2
      Else
        Settings.AutoUpdateResDefs = 1
      End If
      WriteAppSetting SettingsList, sLOGICS, "AutoUpdateResDefs", Settings.AutoUpdateResDefs
    End If
  Case 1
    rtn = vbNo
  Case 2
    rtn = vbYes
  End Select

  'if yes,
  If rtn = vbYes Then
    WaitCursor
    
    'step through all defines; if the current name is different than
    'the original name, use replaceall to make the change
    Load frmProgress
    frmProgress.pgbStatus.Max = fgReserved.Rows
    frmProgress.pgbStatus.Value = 0
    frmProgress.Caption = "Updating Reserved Defines"
    frmProgress.lblProgress.Caption = "Locating modified define names"
    frmProgress.Show vbModeless, frmMDIMain
    SafeDoEvents
    
    With fgReserved
      For i = 1 To fgReserved.Rows - 2
        frmProgress.pgbStatus.Value = i
        'refresh on each iteration
        SafeDoEvents
        'numbers and string values DO NOT get replaced this way
        
        '(to skip the header lines, we also check default name column;
        ' only actual defines put anything in that column)
        If Len(.TextMatrix(i, 0)) > 0 And Len(.TextMatrix(i, 2)) > 0 Then
          If Not IsNumeric(.TextMatrix(i, 2)) And Asc(.TextMatrix(i, 2)) <> 34 Then
            'if the original name (col0) is different from current name (col1)
            'then we need to replace it
            If fgReserved.TextMatrix(i, 0) <> fgReserved.TextMatrix(i, 1) Then
              ReplaceAll fgReserved.TextMatrix(i, 0), fgReserved.TextMatrix(i, 1), fdAll, True, True, flAll, rtGlobals
              'if the define value is NOT numeric and not a text string
              If Not IsNumeric(.TextMatrix(i, 2)) And Asc(.TextMatrix(i, 2)) <> 34 Then
                WaitCursor
                'also replace the values that match this define
                ReplaceAll fgReserved.TextMatrix(i, 2), fgReserved.TextMatrix(i, 1), fdAll, True, True, flAll, rtGlobals
              End If
            End If
          End If
        End If
      Next i
    End With
    
    'all done
    Unload frmProgress
    'restore mouse pointer
    Screen.MousePointer = vbDefault
  End If

  'and save them
  SaveResDefOverrides
  
  'update the logic tooltip lookup lists
  BuildRDefLookup
  
  'now we are done
  Me.Hide
End Sub


Private Sub fgReserved_DblClick()

  On Error GoTo ErrHandler
  
  'if on a blank or header line (can easily tell by checking rowdata)
  If fgReserved.Row <> 2 And fgReserved.RowData(fgReserved.Row) = 0 Then
    Exit Sub
  End If
  
  'force to first column
  If fgReserved.Col <> 1 Then
    fgReserved.Col = 1
  End If
  
  'save row being edited
  EditRow = fgReserved.Row
  
  'save current define in case of cancel
  EditDefine.Name = fgReserved.TextMatrix(EditRow, 1)

  'begin edit
    With txtEditName
      fgReserved.Move fgReserved.CellLeft + CellIndentH, fgReserved.CellTop + fgReserved.Top + CellIndentV, fgReserved.CellWidth - CellIndentH, fgReserved.CellHeight - CellIndentV
      fgReserved.Text = fgReserved.Text
      fgReserved.Visible = True
      'select all
      fgReserved.SelStart = 0
      fgReserved.SelLength = Len(.Text)
      fgReserved.SetFocus
    End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub fgReserved_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpRow As Long, tmpCol As Long
  Dim TopRow As Long, BtmRow As Long
  
  On Error GoTo ErrHandler

  If txtEditName.Visible Then
    Exit Sub
  End If

  With fgReserved
    tmpCol = fgReserved.MouseCol
    tmpRow = fgReserved.MouseRow

    'if right click
    If Button = vbRightButton Then
      'determine top/bottom rows
      If fgReserved.Row < fgReserved.RowSel Then
        TopRow = fgReserved.Row
        BtmRow = fgReserved.RowSel
      Else
        TopRow = fgReserved.RowSel
        BtmRow = fgReserved.Row
      End If

      'if row and column are NOT within selected area
      If tmpRow < TopRow Or tmpRow > BtmRow Then
        'make new selection
        'check for selection of entire row
        If X < ScreenTWIPSX * 12 Then
          'select entire row
          fgReserved.Col = 0
          fgReserved.Row = tmpRow
          fgReserved.ColSel = 2
          fgReserved.SelectionMode = flexSelectionByRow
          fgReserved.Highlight = flexHighlightAlways
          fgReserved.MergeCells = flexMergeNever
        Else
          'select freely
          fgReserved.Col = tmpCol
          fgReserved.Row = tmpRow
          fgReserved.SelectionMode = flexSelectionFree
          fgReserved.Highlight = flexHighlightNever
          fgReserved.MergeCells = flexMergeRestrictAll
        End If
      End If

      'done with right click activities
      Exit Sub
    End If

    'check for selection of entire row
    If X < ScreenTWIPSX * 12 Then
      'select entire row
      fgReserved.Col = 0
      fgReserved.SelectionMode = flexSelectionByRow
      fgReserved.Highlight = flexHighlightAlways
      fgReserved.MergeCells = flexMergeNever
      fgReserved.ColSel = 2
    
    'check for cursor in range of splitting
    ElseIf (X > fgReserved.ColWidth(1) - SPLIT_WIDTH / 2) And (X < fgReserved.ColWidth(1) + SPLIT_WIDTH / 2) Then
      'start splitting
      picSplit.Visible = True
      SplitOffset = fgReserved.ColWidth(1) - X
      picSplit.Left = X + SplitOffset
      Exit Sub

    'check for left-click on header
    ElseIf tmpRow = 0 Then
      'ignore
      Exit Sub
    Else
      'select freely
      If tmpCol <> -1 Then
        fgReserved.Col = tmpCol
      Else
        fgReserved.Col = 1
      End If
      fgReserved.Row = tmpRow
      fgReserved.SelectionMode = flexSelectionFree
      fgReserved.Highlight = flexHighlightNever
      '.MergeCells = flexMergeRestrictAll
    End If
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub fgReserved_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim Pos As Single

  On Error GoTo ErrHandler
  
  'if editing,
  If txtEditName.Visible Then
    Exit Sub
  End If

  'if editing,
  If txtEditName.Visible Then
    Exit Sub
  End If

  'if splitting
  If picSplit.Visible Then
    Pos = (X + SplitOffset)
    'validate it
    If Pos < fgReserved.Width * 0.2 Then
      Pos = fgReserved.Width * 0.2
    ElseIf Pos > fgReserved.Width * 0.8 Then
      Pos = fgReserved.Width * 0.8
    End If
    'move splitter
    picSplit.Left = Pos

  'elseif within splitzone
  ElseIf (X > fgReserved.ColWidth(1) - SPLIT_WIDTH / 2) And (X < fgReserved.ColWidth(1) + SPLIT_WIDTH / 2) Then
    'show split cursor
'    fgReserved.MousePointer = flexSizeEW
    fgReserved.MousePointer = flexCustom
    Set fgReserved.MouseIcon = picSplit.MouseIcon
  'elseif on left edge,
  ElseIf X < 12 * ScreenTWIPSX Then
    'set cursor to row selector
    fgReserved.MousePointer = flexCustom
    Set fgReserved.MouseIcon = LoadResPicture("EGC_SELROW", vbResCursor)
  Else
    'set cursor to normal
    fgReserved.MousePointer = flexDefault
  End If
  
  'if left button is down, begin dragging
  If Button = vbLeftButton Then
    If Not DroppingGlobal Then
      'if something selected,
      If fgReserved.Col = 1 And fgReserved.Row > 1 And fgReserved.RowSel = fgReserved.Row Then
        fgReserved.OLEDrag
      End If
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub fgReserved_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim Pos As Single

  On Error GoTo ErrHandler
  
  'if splitting
  If picSplit.Visible Then
    'stop splitting
    picSplit.Visible = False

    With fgReserved
      'validate position
      Pos = (X + SplitOffset)
      If Pos < 0.2 * fgReserved.Width Then
        Pos = 0.2 * fgReserved.Width
      ElseIf Pos > 0.8 * fgReserved.Width Then
        Pos = 0.8 * fgReserved.Width
      End If
  
      'reset columns
      fgReserved.ColWidth(1) = Pos
  
      'take into account scrollbar
      fgReserved.ColWidth(2) = fgReserved.Width - fgReserved.ColWidth(1) - 17 * ScreenTWIPSX
    End With
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub fgReserved_OLECompleteDrag(Effect As Long)

  'reset dragging flag
  DroppingGlobal = False
End Sub

Private Sub fgReserved_OLEStartDrag(Data As MSFlexGridLib.DataObject, AllowedEffects As Long)

  On Error GoTo ErrHandler
  
  'wtf? for some reason, in the middle of a mouse down/mouse up
  'operation, the drag event fires; this effs up the splitting operation
  
  If picSplit.Visible Then
    'cancel drag/drop, so split action works as desired
    AllowedEffects = vbDropEffectNone
    Exit Sub
  End If
  
  'set global drop flag (so logics (or other text receivers) know
  'when an object is being dropped
  DroppingGlobal = True
  
  'set allowed effects to copy only
  AllowedEffects = vbDropEffectCopy
  Data.SetData fgReserved.TextMatrix(fgReserved.Row, 1), vbCFText
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
  'detect and respond to keyboard shortcuts
  
  'always check for help key
  If KeyCode = vbKeyF1 And Shift = 0 Then
    MenuClickHelp
    KeyCode = 0
    Exit Sub
  End If

  'if editing something
  If txtEditName.Visible Then
    Exit Sub
  End If
End Sub

Private Sub Form_Load()

  On Error GoTo ErrHandler

  With fgReserved
    'hide column 0
    fgReserved.ColWidth(0) = 0
    
    'set font
    InitFonts
  
    'set initial column width to 2/3 of grid width
    fgReserved.ColWidth(1) = fgReserved.Width * 2 / 3
    fgReserved.ColWidth(2) = fgReserved.Width - fgReserved.ColWidth(1) - 17 * ScreenTWIPSX
    
    'set indent Value
    CellIndentH = ScreenTWIPSX * 4
    CellIndentV = ScreenTWIPSY * 1
  
    'set both columns to leftcenter alignment
    fgReserved.ColAlignment(1) = flexAlignLeftCenter
    fgReserved.ColAlignment(2) = flexAlignLeftCenter
    
    'load reserved names
    LoadReservedNames
  
#If DEBUGMODE <> 1 Then
    'subclass the flexgrid
    PrevFGWndProc = SetWindowLong(.hWnd, GWL_WNDPROC, AddressOf ScrollWndProc)
#End If
  End With
  
  'make sure canceled is false
  Me.Canceled = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub Form_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  On Error GoTo ErrHandler
  
  'if right button
  If Button = vbRightButton Then
    'if edit menu is enabled
    If frmMDIMain.mnuEdit.Enabled Then
      'make sure this form is the active form
      If Not (frmMDIMain.ActiveForm Is Me) Then
        'set focus before showing the menu
        Me.SetFocus
      End If
      'need doevents so form activation occurs BEFORE popup
      'otherwise, errors will be generated because of menu
      'adjustments that are made in the form_activate event
      SafeDoEvents
      'show edit menu
      PopupMenu frmMDIMain.mnuEdit
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_Unload(Cancel As Integer)

#If DEBUGMODE <> 1 Then
  'release subclass hook to flexgrid
  SetWindowLong Me.fgReserved.hWnd, GWL_WNDPROC, PrevFGWndProc
#End If 'need to check if this is last form
End Sub

Private Sub txtEditName_KeyPress(KeyAscii As Integer)
  
  On Error GoTo ErrHandler
  
  Select Case KeyAscii
  Case 9, 10, 13 'enter or tab
    'if result is valid,
    If ValidateInput() Then
      'hide the box
      txtEditName.Visible = False
      fgReserved.SetFocus
    Else
      'need to force focus (might a a tab thing?)
      txtEditName.SetFocus
    End If
    
    'ignore key
    KeyAscii = 0
    
  Case 27 'escape
    'hide textbox without saving text
    txtEditName.Visible = False
    
    'ignore key
    KeyAscii = 0
  End Select
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub txtEditName_Validate(Cancel As Boolean)
  
  'this will handle cases where user tries to 'click' on something,
  
  On Error GoTo ErrHandler
  If Not txtEditName.Visible Then Exit Sub
  
  'if OK, hide the text box
  If ValidateInput() Then
    txtEditName.Visible = False
  Else
  'if not OK, cancel
    Cancel = True
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


        */
    }

    public class WinAGIGrid : DataGridView {
        public WinAGIGrid() {
          //  this.RowTemplate = new WinAGIGridRow();
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e) {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && Rows[e.RowIndex].Tag != null) {
                Color bg;
                if ((int)Rows[e.RowIndex].Tag == 0) {
                    bg = Color.Wheat;
                }
                else {
                    bg = this[e.ColumnIndex, e.RowIndex].Style.BackColor;
                    bg = this.DefaultCellStyle.BackColor;
                }
                using (SolidBrush fillBrush = new SolidBrush(bg))
                using (Pen gridPenColor = new Pen(this.GridColor)) {
                    Rectangle rect2 = new Rectangle(e.CellBounds.Location, e.CellBounds.Size);
                    rect2.X += 1;
                    rect2.Width += 1;
                    rect2.Height -= 1;
                    e.Graphics.FillRectangle(fillBrush, rect2);
                    // draw top and bottom borders
                    Point p1, p2, p3, p4;
                    p1 = p2 = p3 = p4 = e.CellBounds.Location;
                    p1.Y -= 1;
                    p2.Offset(e.CellBounds.Size.Width - 1, -1);
                    p3.Offset(0, e.CellBounds.Size.Height - 1);
                    p4.Offset(e.CellBounds.Size.Width - 1, e.CellBounds.Size.Height - 1);
                    e.Graphics.DrawLine(gridPenColor, p1, p2);
                    e.Graphics.DrawLine(gridPenColor, p3, p4);
                    if (e.ColumnIndex == 0) {
                        // draw left border
                        e.Graphics.DrawLine(gridPenColor, p1, p3);
                    }
                    else if (e.ColumnIndex == 1) {
                        // draw right border
                        e.Graphics.DrawLine(gridPenColor, p2, p4);
                    }
                }
                e.PaintContent(e.CellBounds);  // output cell text
                e.Handled = true;
                return;
            }
            base.OnCellPainting(e);
        }
    }
}
