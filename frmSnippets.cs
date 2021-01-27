using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinAGI_GDS
{
  public partial class frmSnippets : Form
  {
    public frmSnippets()
    {
      InitializeComponent();
    }
    void tmpform()
    {
      /*
Option Explicit

  lngSnip As Long, blnAddSnip As Boolean

  blnDblClick As Boolean
  
  CalcWidth As Long, CalcHeight As Long
  Const MIN_HEIGHT = 3510
  Const MIN_WIDTH = 8790


Function CheckName(ByVal NewName As String, Optional SkipID As Long = -1) As Boolean

  Dim i As Long
  
  'check all snippet names; if checkname is found, return false
  For i = 1 To lstSnippets.ListItems.Count
    If StrComp(lstSnippets.ListItems(i).Text, NewName, vbTextCompare) = 0 And i <> SkipID Then
      Exit Function
    End If
  Next i
  
  'not a duplicate
  CheckName = True
End Function

public Sub InitForm()

  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'add snippets to the listbox
  lstSnippets.ListItems.Clear
  'if at least one valid snippet
  If UBound(CodeSnippets()) > 0 Then
    For i = 1 To UBound(CodeSnippets)
      'add it
      lstSnippets.ListItems.Add(, , CodeSnippets(i).Name).Tag = DecodeSnippet(CodeSnippets(i).Value)
    Next i
  End If
  
  'clear name box
  txtSnipName.Text = ""
  
  'set up the rtf box
  With rtfSnipValue
    'set syntax highlighting property first
    .HighlightSyntax = Settings.HighlightLogic
  
    'then set all highlight properties
    .Font.Name = Settings.EFontName
    .Font.Size = Settings.EFontSize
    .ForeColor = Settings.HColor(0)

    .HNormColor = Settings.HColor(0)
    .HKeyColor = Settings.HColor(1)
    .HIdentColor = Settings.HColor(2)
    .HStrColor = Settings.HColor(3)
    .HCmtColor = Settings.HColor(4)
    .HNormBold = Settings.HBold(0)
    .HKeyBold = Settings.HBold(1)
    .HIdentBold = Settings.HBold(2)
    .HStrBold = Settings.HBold(3)
    .HCmtBold = Settings.HBold(4)
    .HNormItalic = Settings.HItalic(0)
    .HKeyItalic = Settings.HItalic(1)
    .HIdentItalic = Settings.HItalic(2)
    .HStrItalic = Settings.HItalic(3)
    .HCmtItalic = Settings.HItalic(4)

    'then set background
    .BackColor = Settings.HColor(5)

    'adjust undo level
    If Settings.LogicUndo = -1 Then
      .UndoLimit = 999999
    Else
      .UndoLimit = Settings.LogicUndo
    End If

    'and tab spacing
    .TabWidth = Settings.LogicTabWidth
    
  End With
      
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Sub cmdAdd_Click()

  'add new snip with blank value to end
  
  Dim lngID As Long, tmpName As String
  
  On Error GoTo ErrHandler
  
  'next snippet will have index equal to current count
  lngSnip = lstSnippets.ListItems.Count
  
  'get a non-duplicate name
  lngID = 0
  Do
    lngID = lngID + 1
    tmpName = "snippet" & CStr(lngID)
    '*'Debug.Assert lngID < 1000
  Loop Until CheckName(tmpName)
  
  'add name to list
  lstSnippets.ListItems.Add(, , tmpName).Selected = True
  
  'then edit it
  blnAddSnip = True
  
  cmdEditSave_Click
  
  'select name
  txtSnipName.SelStart = 0
  txtSnipName.SelLength = Len(txtSnipName.Text)
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Sub cmdCancel_Click()

  'cancel- nothing to do except close
  Unload Me
End Sub


Sub cmdDelete_Click()
  Dim i As Long
  
  'if not editing (controls are locked)
  If txtSnipName.Locked Then
    'delete it from the listbox
    lstSnippets.ListItems.Remove lngSnip
    
    'select nothing
    lstSnippets.SelectedItem = Nothing
    
    'hide save/edit and delete buttons
    cmdEditSave.Visible = False
    cmdDelete.Visible = False
    rtfSnipValue.Text = ""
    rtfSnipValue.Locked = True
    txtSnipName.Text = ""
    txtSnipName.Locked = True
    
  Else
    'cancel editing of this snippet
    
    'if this a newly added snippet
    If blnAddSnip Then
      'just delete it
      lstSnippets.ListItems.Remove lngSnip
      
      'select nothing
      lstSnippets.SelectedItem = Nothing
      rtfSnipValue.Text = ""
      txtSnipName.Text = ""
      
      blnAddSnip = False
      
    Else
      'restore old name
      txtSnipName.Text = lstSnippets.ListItems(lngSnip).Text
      'restore old value
      rtfSnipValue.Text = lstSnippets.ListItems(lngSnip).Tag
    End If
    
    'lock the edit fields
    txtSnipName.Locked = True
    rtfSnipValue.Locked = True
    
    'change button captions
    cmdEditSave.Caption = "Edit Snippet"
    cmdDelete.Caption = "Delete Snippet"
    
    're-enable list, add button, save/cancel buttons
    lstSnippets.Enabled = True
    cmdAdd.Enabled = True
    cmdSave.Enabled = True
    cmdCancel.Enabled = True
    
    'give focus to the list
    lstSnippets.SetFocus
  End If
End Sub

Sub cmdEditSave_Click()

  Dim lngPos As Long, lngSpace As Long
  Dim strValue As String
  
  On Error GoTo ErrHandler
  
  'if not editing (controls are locked)
  If txtSnipName.Locked Then
    'begin editing
    
    'disable list, add button, save/cancel buttons
    lstSnippets.Enabled = False
    cmdAdd.Enabled = False
    cmdSave.Enabled = False
    cmdCancel.Enabled = False
    
    'unlock edit fields, set focus to name
    txtSnipName.Locked = False
    txtSnipName.SetFocus
    rtfSnipValue.Locked = False
    rtfSnipValue.EmptyUndo
    
    'change button captions
    cmdEditSave.Caption = "Save Snippet"
    cmdDelete.Caption = "Cancel Edit"
    
  Else
  'editing (controls are NOT locked)
  
    'validate name
    If Len(Trim(txtSnipName.Text)) = 0 Then
      MsgBox "Name can't be blank.", vbOKOnly + vbExclamation, "Blank Snippet Name Not Allowed"
      Exit Sub
    End If
    If Not CheckName(Trim(txtSnipName.Text), lngSnip) Then
      MsgBox "'" & Trim(txtSnipName.Text) & "' is already in use as a snippet name.", vbOKOnly + vbExclamation, "Duplicate Snippet Names Not Allowed"
      Exit Sub
    End If
    
    'validate value (after stripping off the unwanted
    'CR that the editor adds)
    If Len(rtfSnipValue.Text) > 0 Then
      strValue = Left(rtfSnipValue.Text, Len(rtfSnipValue.Text) - 1)
    End If
    If Len(strValue) = 0 Then
      MsgBox "Snippet value can't be blank.", vbOKOnly + vbExclamation, "Blank Snippet Value Not Allowed"
      Exit Sub
    End If
    
    'snippet is OK
    
    'save the snippet name
    lstSnippets.ListItems(lngSnip).Text = Trim(txtSnipName.Text)
    'save snip value
    lstSnippets.ListItems(lngSnip).Tag = strValue
    
    'done editing
    
    'enable list, add button, save/cancel buttons
    lstSnippets.Enabled = True
    cmdAdd.Enabled = True
    cmdSave.Enabled = True
    cmdCancel.Enabled = True
    
    'lock edit fields
    txtSnipName.Locked = True
    rtfSnipValue.Locked = True
    
    'change button captions
    cmdEditSave.Caption = "Edit Snippet"
    cmdDelete.Caption = "Delete Snippet"
    
    'set focus to listbox
    lstSnippets.SetFocus
    
    'sort it
    lstSnippets.Sorted = True
    'be sure to update selected item pointer
    lngSnip = lstSnippets.SelectedItem.Index
    
    'ensure it's visible
    lstSnippets.SelectedItem.EnsureVisible
    
    
    'if added, reset the flag
    blnAddSnip = False
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Sub cmdSave_Click()

  'save the list
  
  Dim i As Long, SnipList As StringList
  
  Set SnipList = New StringList
  
  'add filename to first line
  SnipList.Add ProgramDir & "snippets.txt"
  
  'add the header
  SnipList.Add "#"
  SnipList.Add "# snippets for WinAGI"
  SnipList.Add "#"
  SnipList.Add "# control codes used:"
  SnipList.Add "#   %n = new line"
  SnipList.Add "#   %q = '" & QUOTECHAR & "'"
  SnipList.Add "#   %t = tab (based on current tab setting)"
  SnipList.Add "#   %% = '%'"
  SnipList.Add "#   %1, %2, etc snippet argument value"
  SnipList.Add ""
  
  'if no snips
  If lstSnippets.ListItems.Count = 0 Then
    'clear snippets collection
    ReDim CodeSnippets(0)
    
    'add count value of zero
    WriteAppSetting SnipList, "General", "Count", "0"
  Else
    'add count
    WriteAppSetting SnipList, "General", "Count", lstSnippets.ListItems.Count
    
    'resize snippets collection
    ReDim CodeSnippets(lstSnippets.ListItems.Count)
    
    'add each snippet
    For i = 1 To lstSnippets.ListItems.Count
      'add to collection
      CodeSnippets(i).Name = lstSnippets.ListItems(i).Text
      'value is actual text in the snippets list
      CodeSnippets(i).Value = lstSnippets.ListItems(i).Tag
      'write to the list
      WriteAppSetting SnipList, "Snippet" & CStr(i), "Name", CodeSnippets(i).Name
      ' encode value to replace special characters in the file
      WriteAppSetting SnipList, "Snippet" & CStr(i), "Value", EnCodeSnippet(CodeSnippets(i).Value)
    Next i
  End If
  
  'now save the file
  SaveSettingList SnipList
  
  'all done-
  Set SnipList = Nothing
  Unload Me
End Sub


Sub Form_Activate()

  'if coming from a logic editor
  If frmMDIMain.ActiveForm.Name = "frmLogicEdit" Or frmMDIMain.ActiveForm.Name = "frmTextEdit" Then
    If Len(frmMDIMain.ActiveForm.rtfLogic.Selection.Range.Text) > 0 Then
    
      'if mode is allow create(SnipMode = 0)
      If SnipMode = 0 Then
        'add a new snippet
        cmdAdd_Click
        'add this value
        rtfSnipValue.Text = frmMDIMain.ActiveForm.rtfLogic.Selection.Range.Text
        'select the text in name field
        txtSnipName.SelStart = 0
        txtSnipName.SelLength = Len(txtSnipName.Text)
      End If
    End If
  End If
End Sub

Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

  'always check for help first
  If Shift = 0 And KeyCode = vbKeyF1 Then
  'TODO: add correct help topic
    HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\snippets.htm"
    KeyCode = 0
    Exit Sub
  End If

End Sub

Sub Form_Load()
  
  'initialize the form
  InitForm
  
End Sub

Sub Form_Resize()

  On Error GoTo ErrHandler
  
  'use separate variables for managing minimum width/height
  If ScaleWidth < MIN_WIDTH Then
    CalcWidth = MIN_WIDTH
  Else
    CalcWidth = ScaleWidth
  End If
  If ScaleHeight < MIN_HEIGHT Then
    CalcHeight = MIN_HEIGHT
  Else
    CalcHeight = ScaleHeight
  End If
  
  'if the form is not visible
  If Not Visible Then
    Exit Sub
  End If
  
  'if not minimized
  If WindowState <> vbMinimized Then
    'move buttons to bottom of form
    cmdAdd.Top = CalcHeight - 450
    cmdEditSave.Top = CalcHeight - 450
    cmdDelete.Top = CalcHeight - 450
    cmdSave.Top = CalcHeight - 450
    cmdCancel.Top = CalcHeight - 450
    'move save/cancel to right of form
    cmdSave.Left = CalcWidth - 2490
    cmdCancel.Left = CalcWidth - 1245
    'listbox height
    lstSnippets.Height = CalcHeight - 1050
    'editor height/width
    rtfSnipValue.Height = CalcHeight - 1695
    rtfSnipValue.Width = CalcWidth - 2430
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Sub lstSnippets_DblClick()

  'begin editing
  cmdEditSave_Click
End Sub

Sub lstSnippets_ItemClick(ByVal Item As MSComctlLib.ListItem)

  'select a snippet
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'nothing selected
  If Item Is Nothing Then
    'save/edit and delete buttons hidden
    cmdEditSave.Visible = False
    cmdDelete.Visible = False
    rtfSnipValue.Text = ""
    rtfSnipValue.Locked = True
    txtSnipName.Text = ""
    txtSnipName.Locked = True
    
  Else
    'show selected snippet
    txtSnipName.Text = Item.Text
    txtSnipName.Locked = True
  
    'save snippet index
    lngSnip = Item.Index
      
    'add snippet value text to rtf box
    rtfSnipValue.Text = Item.Tag
    rtfSnipValue.Locked = True
    
    'show and enable edit/delete buttons
    cmdEditSave.Caption = "Edit Snippet"
    cmdEditSave.Visible = True
    cmdDelete.Caption = "Delete Snippet"
    cmdDelete.Visible = True
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Sub rtfSnipValue_DblClick(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  'if not editing (control is locked)
  If rtfSnipValue.Locked Then
    'if a snippet is selected
    If Not lstSnippets.SelectedItem Is Nothing Then
      'begin editing this snippet
      cmdEditSave_Click
      'start in the value editor
      rtfSnipValue.SetFocus
    End If
  Else
    'while editing, capture dbl-click
    ' for managing selections
    blnDblClick = True
  End If
  
End Sub

Sub rtfSnipValue_KeyDown(KeyCode As Integer, Shift As Integer)

  'detect and respond to keyboard shortcuts
  
  On Error GoTo ErrHandler
  
  Dim strIn As String
  Dim CharPicker As frmCharPicker
  Dim blnSmartPaste As Boolean
  
  'always check for help first
  If Shift = 0 And KeyCode = vbKeyF1 Then
  'TODO: add correct help topic
    HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\snippets.htm"
    KeyCode = 0
    Exit Sub
  End If

  'for richtext editor only, need to intercept
  'control keys that are assigned permanent shortcuts
  Select Case Shift
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyN, vbKeyO, vbKeyB, vbKeyR, vbKeyS, vbKeyE, vbKeyD, vbKeyI, vbKeyP, vbKeyL, vbKeyG, vbKeyM, vbKeyF1, vbKeyF2, vbKeyF3, vbKeyF4, vbKeyF5, vbKeyF6, vbKeyF7
      'don't respond to these keys
      KeyCode = 0
      Shift = 0
    
    Case vbKeyV
      'if selection has space at end?
      If rtfSnipValue.Selection.Range.Length > 0 And Len(Clipboard.GetText) > 0 Then
        blnSmartPaste = (Asc(Right(rtfSnipValue.Selection.Range.Text, 1)) = 32 And Asc(Right(Clipboard.GetText, 1)) <> 32)
      End If
      'if across multiple lines, override smartpaste to false
      If InStr(1, rtfSnipValue.Selection.Range.Text, vbCr) > 0 Then
        blnSmartPaste = False
      End If
  
      'do the paste operation
      rtfSnipValue.Selection.Range.Paste
      
      'smart paste?
      If blnSmartPaste Then
        rtfSnipValue.Selection.Range.Text = " "
      End If
      
      KeyCode = 0
      Shift = 0
      
    Case vbKeyC
      'reset the globals clipboard
      ReDim GlobalsClipboard(0)
      
    Case vbKeyX
      'reset the globals clipboard
      ReDim GlobalsClipboard(0)
      
    Case vbKeyInsert
      
      Set CharPicker = New frmCharPicker
      CharPicker.Show vbModal, frmMDIMain
      
      If Not CharPicker.Cancel Then
        If Len(CharPicker.InsertString) > 0 Then
          'need an actual string variable
          'to be able to convert the bytes
          'into correct extended chars for display
          strIn = CharPicker.InsertString
          ByteToExtChar strIn
          rtfSnipValue.Selection.Range.Text = strIn
        End If
      End If
      
      Unload CharPicker
      Set CharPicker = Nothing
      
      KeyCode = 0
      Shift = 0
    End Select
  End Select
  
  'shortcuts for editing are builtin to the rtf box
  ' redo
  ' undo
  ' cut
  ' copy
  ' paste (override with custom paste function)
  ' delete
  ' select all
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Sub rtfSnipValue_KeyPress(KeyAscii As Integer)

  Select Case KeyAscii
  Case 9  'TAB
    'wow! i never caught this before???? tabs need to be ignored;
    ' they are converted to spaces automatically by the control
    KeyAscii = 0
  End Select
  
End Sub


Sub rtfSnipValue_LostFocus()

  'if nothing, don't allow saving
  cmdEditSave.Enabled = Len(Trim(rtfSnipValue.Text))
  
End Sub

Sub rtfSnipValue_SelectionChanged()

  With rtfSnipValue.Selection.Range
    'if selected something by dbl-click
    If .Length > 0 And blnDblClick Then
      'reset dblclick so it doesn't recurse
      blnDblClick = False
      
      'then expand, if an agi command with a dot is selected
      ExpandSelection rtfSnipValue, False, True
    End If
  End With
  
  'always reset dbl-click
  blnDblClick = False
  
End Sub


Sub txtSnipName_DblClick()

  'if not editing (control is locked)
  If txtSnipName.Locked Then
    'if a snippet is selected
    If Not lstSnippets.SelectedItem Is Nothing Then
      'begin editing this snippet
      cmdEditSave_Click
      'begin in name textbox
      txtSnipName.SetFocus
    End If
  End If
End Sub


      */
    }
  }
}
