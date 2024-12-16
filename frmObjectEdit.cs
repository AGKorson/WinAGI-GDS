using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmObjectEdit : Form {
        public bool InGame;
        public bool IsChanged;
        public InventoryList EditInvList;
        private string EditInvListFilename;
        private bool closing = false;

        public frmObjectEdit() {
            InitializeComponent();
            InitFonts();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers
        private void frmObjectEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmObjectEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // ensure object is cleared and dereferenced

            if (EditInvList != null) {
                EditInvList.Unload();
                EditInvList = null;
            }
            if (InGame) {
                OEInUse = false;
                ObjectEditor = null;
            }
        }

        #endregion

        #region Menu Event Handlers
        internal void SetResourceMenu() {

            mnuRSave.Enabled = IsChanged;
            MDIMain.mnuRSep3.Visible = true;
            if (EditGame is null) {
                // no game is open
                MDIMain.mnuRImport.Enabled = false;
                mnuRExport.Text = "Save As ...";
                // mnuRProperties no change
                mnuRToggleEncrypt.Checked = EditInvList.Encrypted;
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export OBJECT" : "Save As ...";
                // mnuRProperties no change
                mnuRExportLoopGIF.Enabled = true; // = loop or cel selected
            }
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            SaveObjects();
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            ExportObjects();
        }

        public void mnuRProperties_Click(object sender, EventArgs e) {
            EditProperties();
        }

        private void mnuRToggleEncrypt_Click(object sender, EventArgs e) {
            MessageBox.Show("toggle object encryption");
        }
        #endregion

        #region temp code
        void objeditcode() {
            /*

Option Explicit

  
  Private UndoCol As Collection
  
  Private EditRow As Long, EditCol As ColType
  Private CellIndentH As Single
  Private CellIndentV As Single
  
  Private AddingItem As Boolean
  Private EditItemNum As Byte
  Private blnRecurse As Boolean
  Private PickChar As Boolean
  
  Public PrevFGWndProc As Long
  Public PrevTBMOWndProc As Long
  Public PrevTBRmWndProc As Long
  Public PrevTBOTWndProc As Long
  
  Private Enum ColType
    ctNumber
    ctDesc
    ctRoom
  End Enum

Private Sub EditObj()

  On Error GoTo ErrHandler
  
  Dim blnNoWarn As Boolean, rtn As VbMsgBoxResult
  
  ' although not required, object i0 is traditionally set
  ' to '?', as are all 'unused' objects
  If fgObjects.Row = 1 And Settings.WarnItem0 Then
    rtn = MsgBoxEx("Item 0 is usually set to '?'. Editing it is possible, but not normal. Are " & vbCrLf & _
      "you sure you want to edit it?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Editing Item 0", _
      WinAGIHelp, "htm\agi\object.htm#nullitem", "Don't show this warning again.", blnNoWarn)
    If blnNoWarn Then
      'save the setting
      Settings.WarnItem0 = Not blnNoWarn
      WinAGISettingsList.WriteSetting(sGENERAL, "WarnItem0", Settings.WarnItem0
    End If
    
    If rtn = vbNo Then
      'cancel and exit
      rtfObject.Visible = False
      DoEvents
      fgObjects.SetFocus
      Exit Sub
    End If
  End If
  
  'begin edit of item description
  With rtfObject
    .Move fgObjects.CellLeft + CellIndentH - 1, fgObjects.CellTop + fgObjects.Top + CellIndentV, fgObjects.CellWidth - CellIndentH, fgObjects.CellHeight - CellIndentV
    .TextB = ObjectsEdit(fgObjects.Row - 1).ItemName
    .Visible = True
    .Range.SelectRange
    .SetFocus
  End With

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub MenuClickCustom2()
  
  'convert an Amiga format OBJECT file to DOS format
  
  On Error GoTo ErrHandler
  
  'verify it's an Amiga file
  If Not InventoryObjects.AmigaOBJ Then
    'hmm, not sure how we got here
    frmMDIMain.mnuRCustom2.Visible = False
    MsgBox "This is not an AMIGA Object file, so no conversion is necessary.", vbInformation + vbOKOnly, "Convert AMIGA Object File"
    Exit Sub
  End If
  
  'get permission
  If MsgBox("Your current OBJECT file will be saved as 'OBJECT.amg'. Continue with the conversion?", vbQuestion + vbOKCancel, "Convert AMIGA Object File") = vbOK Then

    'if the file is not saved, ask if OK to save it first
    If Me.IsChanged Then
      If MsgBox("The OBJECT file needs to be saved before converting. OK to save and convert?", vbQuestion + vbOKCancel, "Save OBJECT File") = vbCancel Then
        Exit Sub
      End If
    End If
    'file is saved; change the AmigaOBJ property to affect the conversion
    InventoryObjects.AmigaOBJ = False
    'hide the menu
    frmMDIMain.mnuRCustom2.Visible = False
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickHelp()
  
  On Error GoTo ErrHandler
  
  'help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Objects_Editor.htm"
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub BeginFind()

  'each form has slightly different search parameters
  'and procedure; so each form will get what it needs
  'from the form, and update the global search parameters
  'as needed
  '
  'that's why each search form cheks for changes, and
  'sets the global values, instead of doing it once inside
  'the FindForm code
  
  Select Case FindForm.FormAction
  Case faFind
    'now find the object
    FindInObjects GFindText, GFindDir, GMatchWord, GMatchCase
  Case faReplace
    FindInObjects GFindText, GFindDir, GMatchWord, GMatchCase, True, GReplaceText
  Case faReplaceAll
    ReplaceAll GFindText, GMatchWord, GMatchCase, GReplaceText
  Case faCancel
    'don't do anything
  End Select
End Sub

Private Sub FindInObjects(ByVal FindText As String, ByVal FindDir As FindDirection, ByVal MatchWord As Boolean, ByVal MatchCase As Boolean, Optional ByVal Replacing As Boolean = False, Optional ByVal ReplaceText As String = vbNullString)

  Dim SearchPos As Long, FoundPos As Long
  Dim rtn As VbMsgBoxResult
  Dim vbcComp As VbCompareMethod
  
  On Error GoTo ErrHandler
  
  'if replacing and new text is the same
  If Replacing And (StrComp(FindText, ReplaceText, vbTextCompare) = 0) Then
    'exit
    Exit Sub
  End If
    
  If ObjectsEdit.Count = 0 Then
    MsgBox "No inventory objects in list.", vbOKOnly + vbInformation, "Find in Object List"
    Exit Sub
  End If
  
  'show wait cursor
  WaitCursor
  
  'set comparison method for string search,
  vbcComp = CLng(MatchCase) + 1 ' CLng(True) + 1 = 0 = vbBinaryCompare; Clng(False) + 1 = 1 = vbTextCompare
  
  'if replacing and searching up   searchpos = current pos+1
  'if replacing and searching down searchpos = current pos
  'if not repl  and searching up   searchpos = current pos
  'if not repl  and searching down searchpos = current pos+1
  
  'set searchpos to current item index (current item index is row-1)
  SearchPos = fgObjects.Row - 1
  
  'adjust to next object per replace/direction selections
  If (Replacing And FindDir = fdUp) Or (Not Replacing And FindDir <> fdUp) Then
    'add one to skip current object
    SearchPos = SearchPos + 1
    If SearchPos > ObjectsEdit.Count - 1 Then
      SearchPos = 0
    End If
  Else
    'if already AT beginning of search, the replace function will mistakenly
    'think the find operation is complete and stop
    If Replacing And (SearchPos = ObjStartPos) Then
      'reset search
      FindForm.ResetSearch
    End If
  End If
  
  
  'main search loop
  Do
    'if direction is up
    If FindDir = fdUp Then
      'iterate backwards until word found or foundpos=-1
      FoundPos = SearchPos - 1
      Do Until FoundPos = -1
        If MatchWord Then
          If StrComp(ObjectsEdit(FoundPos).ItemName, FindText, vbcComp) = 0 Then
            'found
            Exit Do
          End If
        Else
          If InStr(1, ObjectsEdit(FoundPos).ItemName, FindText, vbcComp) <> 0 Then
            'found
            Exit Do
          End If
        End If
        FoundPos = FoundPos - 1
      Loop
      'reset searchpos
      SearchPos = ObjectsEdit.Count - 1
    Else
      'iterate forward until word found or foundpos=objcount
      FoundPos = SearchPos
      Do
        If MatchWord Then
          If StrComp(ObjectsEdit(FoundPos).ItemName, FindText, vbcComp) = 0 Then
            'found
            Exit Do
          End If
        Else
          If InStr(1, ObjectsEdit(FoundPos).ItemName, FindText, vbcComp) <> 0 Then
            'found
            Exit Do
          End If
        End If
        FoundPos = FoundPos + 1
      Loop Until FoundPos = ObjectsEdit.Count
      'reset searchpos
      SearchPos = 0
    End If
    
    'if found
    If FoundPos >= 0 And FoundPos < ObjectsEdit.Count Then
      'if back at start
      If FoundPos = ObjStartPos Then
        FoundPos = -1
      End If
      Exit Do
    End If
    
    'if not found, action depends on search mode
    Select Case FindDir
    Case fdUp
      'if not reset yet
      If Not RestartSearch Then
        'if recursing
        If blnRecurse Then
          'just say no
          rtn = vbNo
        Else
          rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Object List")
        End If
        If rtn = vbNo Then
          'reset search
          FindForm.ResetSearch
          Screen.MousePointer = vbDefault
          Exit Sub
        End If
      Else
        'entire scope already searched; exit
        Exit Do
      End If
      
    Case fdDown
      'if not reset yet
      If Not RestartSearch Then
        'if recursing
        If blnRecurse Then
          'just say no
          rtn = vbNo
        Else
          rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Object List")
        End If
        If rtn = vbNo Then
          'reset search
          FindForm.ResetSearch
          Screen.MousePointer = vbDefault
          Exit Sub
        End If
      Else
        'entire scope already searched; exit
        Exit Do
      End If
      
    Case fdAll
      If RestartSearch Then
        Exit Do
      End If
      
    End Select
    
    'reset search so when we get back to start, search will end
    RestartSearch = True
  
  'loop is exited by finding the searchtext or reaching end of search area
  Loop
        
  'if search string found
  If FoundPos >= 0 And FoundPos < ObjectsEdit.Count Then
    'if this is first occurrence
    If Not FirstFind Then
      'save this position
      FirstFind = True
      ObjStartPos = FoundPos
    End If
    
    'highlight object
    fgObjects.Row = FoundPos + 1
    If Not fgObjects.RowIsVisible(FoundPos + 1) Then
      fgObjects.TopRow = FoundPos - 2
    End If
    
    'if replacing
    If Replacing Then
      If MatchWord Then
        ModifyItem FoundPos, ReplaceText
      Else
        ModifyItem FoundPos, Replace(ObjectsEdit(FoundPos).ItemName, FindText, ReplaceText, 1, -1, vbcComp)
      End If
      'change undoobject
      UndoCol(UndoCol.Count).UDAction = udoReplace
      frmMDIMain.mnuEUndo.Caption = "&Undo Replace" & vbTab & "Ctrl+Z"
      
      'recurs the find method to get next occurrence
      blnRecurse = True
      FindInObjects FindText, FindDir, MatchWord, MatchCase, False
      blnRecurse = False
    End If
    
  Else
    'if not recursing, show a msg
    If Not blnRecurse Then
      'if something was previously found
      If FirstFind Then
        'search complete; no new instances found
        MsgBox "The specified region has been searched.", vbInformation, "Find in Object List"
      Else
        'show not found msg
        MsgBox "Search text not found.", vbInformation, "Find in Object List"
      End If
    End If
    
    'reset search flags
    FindForm.ResetSearch
  End If
  
  fgObjects.SetFocus
  
  'need to always make sure right form has focus; if finding a word
  'causes the group list to change, VB puts the wordeditor form in focus
  ' but we want focus to match the starting form
  If SearchStartDlg Then
    FindForm.SetFocus
  Else
    Me.SetFocus
  End If
  
  'reset cursor
  Screen.MousePointer = vbDefault
Exit Sub
  
ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub InitFonts()

  On Error GoTo ErrHandler
  
  'set form font to enable height/width calculations
  Me.Font.Name = Settings.EFontName
  Me.Font.Size = Settings.EFontSize
  
  'set grid font and column widths
  fgObjects.Font.Name = Settings.EFontName
  fgObjects.Font.Size = Settings.EFontSize
  fgObjects.ColWidth(0) = Me.TextWidth("Item # ")
  fgObjects.ColWidth(2) = Me.TextWidth("Room # ")
  'if more rows than will fit on screen
  If fgObjects.Rows * fgObjects.RowHeight(0) > fgObjects.Height Then
    'account for scrollbar
    fgObjects.ColWidth(1) = fgObjects.Width - fgObjects.ColWidth(2) - fgObjects.ColWidth(0) - 320
  Else
    fgObjects.ColWidth(1) = fgObjects.Width - fgObjects.ColWidth(2) - fgObjects.ColWidth(0)
  End If
  fgObjects.Refresh
  
  rtfObject.Font.Name = Settings.EFontName
  rtfObject.Font.Size = Settings.EFontSize
  
  txtRoomNo.Font.Name = Settings.EFontName
  txtRoomNo.Font.Size = Settings.EFontSize
  txtMaxScreenObj.Font.Name = Settings.EFontName
  txtMaxScreenObj.Font.Size = Settings.EFontSize
  txtMaxScreenObj.Height = 60 + fgObjects.RowHeight(1)
  Label1.Font.Name = Settings.EFontName
  Label1.Font.Size = Settings.EFontSize
  Label1.Width = Me.TextWidth(Label1.Caption)
  Label1.Left = txtMaxScreenObj.Left - Label1.Width
  fgObjects.Top = txtMaxScreenObj.Top + txtMaxScreenObj.Height
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub MenuClickReplace()

  'use menuclickfind in replace mode
  MenuClickFind ffReplaceObject
End Sub


Private Sub AddItem(NewItem As String, Room As Byte, Optional DontUndo As Boolean = False)
  'adds a new item to end of list
  
  Dim NextUndo As ObjectsUndo
  
  'if already at Max
  If ObjectsEdit.Count >= 256 Then
    'just exit
    Exit Sub
  End If
  
  'add item to game object
  ObjectsEdit.Add NewItem, Room
  
  'add item to grid
  fgObjects.AddItem CStr(ObjectsEdit.Count - 1) & ". " & vbTab & CPToUnicode(NewItem, SessionCodePage) & vbTab & CStr(Room)
  'select item that was just added
  fgObjects.Row = fgObjects.Rows - 1
  If Not fgObjects.RowIsVisible(fgObjects.Row) Then
    fgObjects.TopRow = fgObjects.Rows - 1
  End If
  fgObjects.Col = 1
  fgObjects.Enabled = True
  
  'if not skipping undo
  If Not DontUndo Then
    Set NextUndo = New ObjectsUndo
    NextUndo.UDAction = udoAddItem
    'add to undo collection
    AddUndo NextUndo
  End If
  
  'set add flag
  AddingItem = True
  
  '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me
  'update status bar with Count (subtract one cuz first item doesnt Count)
  MainStatusBar.Panels("Count").Text = "Object Count: " & CStr(ObjectsEdit.Count) - 1
  
End Sub


Private Sub DeleteItem(ByVal ItemIndex As Byte, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As ObjectsUndo
  
  'if not skipping undo
  If Not DontUndo Then
    Set NextUndo = New ObjectsUndo
    NextUndo.UDAction = udoDeleteItem
    NextUndo.UDObjectNo = ItemIndex
    NextUndo.UDObjectText = ObjectsEdit(ItemIndex).ItemName
    NextUndo.UDObjectRoom = ObjectsEdit(ItemIndex).Room
    'add to undo collection
    AddUndo NextUndo
  End If
  
  'if there are at least two objects and deleting last item
  If ObjectsEdit.Count >= 2 And ItemIndex = ObjectsEdit.Count - 1 Then
    'delete the row
    fgObjects.RemoveItem ItemIndex + 1
  Else
    'set item description to '?'
    fgObjects.TextMatrix(ItemIndex + 1, 1) = "?"
    'set row to zero
    fgObjects.TextMatrix(ItemIndex + 1, 2) = "0"
  End If
  
  'delete the object from game object
  ObjectsEdit.Remove ItemIndex
  
  '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me
  'update status bar with Count (subtract one cuz first item doesnt Count)
  MainStatusBar.Panels("Count").Text = "Object Count: " & CStr(ObjectsEdit.Count - 1)
End Sub

Public Sub MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindObject)

  On Error GoTo ErrHandler
  
  With FindForm
    'set form defaults
    If Not .Visible Then
      If Len(GFindText) > 0 Then
        'if it has quotes, remove them
        If Asc(GFindText) = 34 Then
          GFindText = Right$(GFindText, Len(GFindText) - 1)
        End If
        If Right$(GFindText, 1) = QUOTECHAR Then
          GFindText = Left$(GFindText, Len(GFindText) - 1)
        End If
      End If
    End If
    
    'set find dialog to object mode
    .SetForm ffValue, False
    
    'show the form
    .Show , frmMDIMain
  
    'always highlight search text
    .rtfFindText.Selection.Range.StartPos = 0
    .rtfFindText.Selection.Range.EndPos = Len(.rtfFindText.Text)
    .rtfFindText.SetFocus
    
    'ensure this form is the search form
    Set SearchForm = Me
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickFindAgain()
  
  On Error GoTo ErrHandler
  
  'if nothing in find form textbox
  If LenB(GFindText) <> 0 Then
    FindInObjects GFindText, GFindDir, GMatchWord, GMatchCase
  Else
    'show find form
    MenuClickFind
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub NewObjects()

  'creates a new objects file
  
  On Error GoTo ErrHandler
  
  'set changed status and caption
  IsChanged = True
  
  ObjCount = ObjCount + 1
  Caption = sDM & "Objects Editor - NewObjects" & CStr(ObjCount)
  fgObjects.TextMatrix(1, 0) = "0."
  fgObjects.TextMatrix(1, 1) = "?"
  fgObjects.TextMatrix(1, 2) = "0"
  
  Set ObjectsEdit = New AGIInventoryObjects
  ObjectsEdit.NewObjects
  
  'clear filename
  ObjectsEdit.ResFile = vbNullString
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub ReplaceAll(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal MatchCase As Boolean, ByVal ReplaceText As String)

  'replace all occurrences of FindText with ReplaceText
  
  Dim i As Long, vbcComp As VbCompareMethod
  Dim lngCount As Long
  Dim NextUndo As ObjectsUndo
  
  On Error GoTo ErrHandler
  
  'if replacing and new text is the same
  If StrComp(FindText, ReplaceText, vbTextCompare) = 0 Then
    'exit
    Exit Sub
  End If
  
  'if no objects in list
  If ObjectsEdit.Count = 0 Then
    MsgBox "Object list is empty.", vbOKOnly + vbInformation, "Replace All"
    Exit Sub
  End If
  
  'show wait cursor
  WaitCursor
  
  'create new undo object
  Set NextUndo = New ObjectsUndo
  NextUndo.UDAction = udoReplaceAll
  
  'set comparison method for string search,
  vbcComp = CLng(MatchCase) + 1 ' CLng(True) + 1 = 0 = vbBinaryCompare; Clng(False) + 1 = 1 = vbTextCompare
  
  If MatchWord Then
    'step through all objects
    For i = 0 To ObjectsEdit.Count - 1
      If StrComp(ObjectsEdit(i).ItemName, FindText, vbcComp) = 0 Then
        'add  word being replaced to undo
        If lngCount = 0 Then
          NextUndo.UDObjectText = CStr(i) & "|" & ObjectsEdit(i).ItemName
        Else
          NextUndo.UDObjectText = NextUndo.UDObjectText & "|" & CStr(i) & "|" & ObjectsEdit(i).ItemName
        End If
        'replace the word
        ObjectsEdit(i).ItemName = ReplaceText
        'update grid
        fgObjects.TextMatrix(i + 1, ctDesc) = ReplaceText
        'increment counter
        lngCount = lngCount + 1
      End If
    Next i
  Else
    For i = 0 To ObjectsEdit.Count - 1
      If InStr(1, ObjectsEdit(i).ItemName, FindText, vbcComp) <> 0 Then
        'add  word being replaced to undo
        If lngCount = 0 Then
          NextUndo.UDObjectText = CStr(i) & "|" & ObjectsEdit(i).ItemName
        Else
          NextUndo.UDObjectText = NextUndo.UDObjectText & "|" & CStr(i) & "|" & ObjectsEdit(i).ItemName
        End If
        'replace the word
        ObjectsEdit(i).ItemName = Replace(ObjectsEdit(i).ItemName, FindText, ReplaceText, 1, -1, vbcComp)
        'update grid
        fgObjects.TextMatrix(i + 1, ctDesc) = Replace(ObjectsEdit(i).ItemName, FindText, ReplaceText)
        'increment counter
        lngCount = lngCount + 1
      End If
    Next i
  End If
  
  'if nothing found,
  If lngCount = 0 Then
    MsgBox "Search text not found.", vbInformation, "Replace All"
  Else
    'add undo
    AddUndo NextUndo
    
    'show how many replacements made
    MsgBox "The specified region has been searched. " & CStr(lngCount) & " replacements were made.", vbInformation, "Replace All"
  End If
  
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  Resume Next
End Sub

Public Sub MenuClickClear()

  'clear the object list
  Clear
End Sub

Public Sub MenuClickDelete()
  'delete object
  
  'first item can be edited, but it CAN'T be deleted
  
  'if on a valid row,
  If fgObjects.Row > 1 Then
    EditItemNum = fgObjects.Row - 1
    'delete
    DeleteItem EditItemNum
  End If
  
End Sub

Public Sub MenuClickInsert()
  'add object
  
  ' make sure there is room first
  If ObjectsEdit.Count < 256 Then
    'add a new item to the grid
    AddItem "?", 0
  
    'use doubleclick
    fgObjects_DblClick
  End If
End Sub


Public Sub MenuClickOpen()

End Sub
            */
        }
        void objformcode2() {
            /*
Public Sub MenuClickPrint()

  Load frmPrint
  frmPrint.SetMode rtObjects, ObjectsEdit, , InGame
  frmPrint.Show vbModal, frmMDIMain
  
End Sub

Public Sub MenuClickExport()

  If ExportObjects(ObjectsEdit, InGame) Then
    'if this is NOT the in game file,
    If Not InGame Then
      'reset changed flag and caption
      IsChanged = False
      'update caption
      Caption = "Objects Editor - " & CompactPath(ObjectsEdit.ResFile, 75)
      'disable save menu/button
      frmMDIMain.mnuRSave.Enabled = False
      frmMDIMain.Toolbar1.Buttons("save").Enabled = False
    End If
  End If
End Sub

Public Sub MenuClickImport()

End Sub

Public Sub MenuClickNew()

End Sub

Public Sub MenuClickInGame()
  'not used
End Sub

Public Sub MenuClickRenumber()

End Sub

Public Sub MenuClickECustom2()
  
  'call findinlogic with current invobj
  
  Dim strObj As String
  
  strObj = fgObjects.TextMatrix(fgObjects.Row, ctDesc)
  
  If strObj <> "?" Then
    'reset logic search
    FindForm.ResetSearch
    
    'set search parameters
    GFindText = QUOTECHAR & strObj & QUOTECHAR
    GFindDir = fdAll
    GMatchWord = True
    GMatchCase = False
    GLogFindLoc = flAll
    GFindSynonym = False
    SearchType = rtObjects
    
    With FindForm
      'if the findform is visible,
      If .Visible Then
      'set it to match desired search parameters
        'set find dialog to find textinlogic mode
        .SetForm ffFindLogic, True
      End If
    End With
    
    'ensure this form is the search form
    Set SearchForm = Me
    
    'now search all logics
    FindInLogic QUOTECHAR & Replace(strObj, QUOTECHAR, "\""") & QUOTECHAR, fdAll, True, False, flAll, False, vbNullString
  End If
End Sub

Public Sub Clear(Optional ByVal DontUndo As Boolean = False)

  Dim i As Long
  Dim NextUndo As ObjectsUndo
  
  On Error GoTo ErrHandler
  
  'if skipping undo
  If Not DontUndo Then
    'create new undo object
    Set NextUndo = New ObjectsUndo
    NextUndo.UDAction = udoClear
    
    With ObjectsEdit
      'store Max objects
      NextUndo.UDObjectRoom = .MaxScreenObjects
      'store encryption
      NextUndo.UDObjectNo = CLng(.Encrypted)
      'add first object, using tabs and cr's
      NextUndo.UDObjectText = .Item(0).ItemName & vbTab & CStr(.Item(0).Room)
      'now add rest of objects
      For i = 1 To .Count - 1
        NextUndo.UDObjectText = NextUndo.UDObjectText & vbCr & .Item(i).ItemName & vbTab & CStr(.Item(i).Room)
      Next i
    End With
    'add to undo
    AddUndo NextUndo
  End If
  
  'clear grid
  For i = fgObjects.Rows - 2 To 1 Step -1
    fgObjects.RemoveItem i
  Next i
  
  'add one blank line
  fgObjects.TextMatrix(1, 0) = "0."
  fgObjects.TextMatrix(1, 1) = "?"
  fgObjects.TextMatrix(1, 2) = "0"
  
  'now clear the object list
  ObjectsEdit.Clear
  ObjectsEdit.MaxScreenObjects = Settings.MaxSO
  ObjectsEdit.Encrypted = False
  
  'update status bar with Count (subtract one cuz first item doesnt Count)
  MainStatusBar.Panels("Count").Text = "Object Count: 0"
  MainStatusBar.Panels("Encrypt").Text = "Not Encrypted"
  frmMDIMain.mnuRCustom3.Checked = False
  Err.Clear
  
  txtMaxScreenObj.Text = CStr(ObjectsEdit.MaxScreenObjects)
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub AddUndo(NextUndo As ObjectsUndo)

  '*'Debug.Assert Not (UndoCol Is Nothing)
  
  On Error GoTo ErrHandler
  
  'adds the next undo object
  UndoCol.Add NextUndo
  
  'set undo menu
  frmMDIMain.mnuEUndo.Enabled = True
  frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(OBJUNDOTEXT + NextUndo.UDAction) & vbTab & "Ctrl+Z"
  
  MarkAsChanged
  
  'reset searchflags
  FindForm.ResetSearch
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub MarkAsChanged()

  If Not IsChanged Then
    'set changed flag
    IsChanged = True
    
    'mark caption
    Caption = sDM & Caption
    
    'enable menu and toolbar button
    frmMDIMain.mnuRSave.Enabled = True
    frmMDIMain.Toolbar1.Buttons("save").Enabled = True
  End If
  
End Sub
Public Sub MenuClickCustom3()
  'toggle encryption
  
  ToggleEncryption Not ObjectsEdit.Encrypted
End Sub

Private Sub ModifyItem(ByVal ItemIndex As Long, ByVal NewItemText As String, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As ObjectsUndo
  
  On Error GoTo ErrHandler
  
  'if no change
  If StrComp(ObjectsEdit(ItemIndex).ItemName, NewItemText, vbBinaryCompare) = 0 Then
    Exit Sub
  End If
  
  'if change is to empty string
  If LenB(NewItemText) = 0 Then
    'same as delete
    DeleteItem ItemIndex
    Exit Sub
  End If
  
  'if not skipping undo
  If Not DontUndo Then
    Set NextUndo = New ObjectsUndo
    'if adding a new item
    If AddingItem Then
      'skip undo- it has already been added
      AddingItem = False
    Else
      NextUndo.UDAction = udoModifyItem
      NextUndo.UDObjectNo = ItemIndex
      NextUndo.UDObjectText = ObjectsEdit(ItemIndex).ItemName
      'add to undo collection
      AddUndo NextUndo
    End If
  End If
  
  'make changes
  ObjectsEdit(ItemIndex).ItemName = NewItemText
  'update grid
  fgObjects.TextMatrix(ItemIndex + 1, 1) = CPToUnicode(NewItemText, SessionCodePage)
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub ModifyMax(ByVal NewMax As Byte, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As ObjectsUndo
  
  'if no change
  If ObjectsEdit.MaxScreenObjects = NewMax Then
    Exit Sub
  End If
  
  'if not skipping undo
  If Not DontUndo Then
    Set NextUndo = New ObjectsUndo
    NextUndo.UDAction = udoChangeMaxObj
    NextUndo.UDObjectRoom = ObjectsEdit.MaxScreenObjects
    'add to undo collection
    AddUndo NextUndo
  End If
  
  'make changes
  ObjectsEdit.MaxScreenObjects = NewMax
  txtMaxScreenObj.Text = CStr(ObjectsEdit.MaxScreenObjects)
End Sub


Private Sub ModifyRoom(ByVal ItemIndex As Long, ByVal NewRoom As Byte, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As ObjectsUndo
  
  'if no change
  If ObjectsEdit(ItemIndex).Room = NewRoom Then
    Exit Sub
  End If
  
  'if not skipping undo
  If Not DontUndo Then
    Set NextUndo = New ObjectsUndo
    NextUndo.UDAction = udoModifyRoom
    NextUndo.UDObjectNo = ItemIndex
    NextUndo.UDObjectRoom = ObjectsEdit(ItemIndex).Room
    'add to undo collection
    AddUndo NextUndo
  End If
  
  'make changes
  ObjectsEdit(ItemIndex).Room = NewRoom
  'update grid
  fgObjects.TextMatrix(ItemIndex + 1, 2) = CStr(NewRoom)
End Sub

Public Sub LoadObjects(ByVal ObjectFile As String)
  
  'opens an object file and loads it into the editor
  Dim i As Long
  Dim tmpObjects As AGIInventoryObjects
  
  On Error GoTo ErrHandler
  
  ' show wait cursor; this may take awhile
  WaitCursor
  
  'use a temp objects object to get items
  Set tmpObjects = New AGIInventoryObjects
  
  'trap errors
  On Error Resume Next
  tmpObjects.Load ObjectFile
  If Err.Number <> 0 Then
    'restore cursor
    Screen.MousePointer = vbDefault
  
    ErrMsgBox "Unable to load this object file due to error: ", "", "Load Object Error"
    On Error GoTo 0: Err.Raise vbObjectError + 601, "frmObjectEdit", "Load Object Error"
    Exit Sub
  End If
  On Error GoTo ErrHandler
  
  'set Max screen objs
  txtMaxScreenObj.Text = CStr(tmpObjects.MaxScreenObjects)
  
  'add items
  For i = 0 To tmpObjects.Count - 1
    ' convert extended characters to correct byte value
    fgObjects.AddItem CStr(i) & ". " & vbTab & CPToUnicode(tmpObjects(i).ItemName, SessionCodePage) & vbTab & CStr(tmpObjects(i).Room)
  Next i
  
  'remove first line that was left by the clear function
  fgObjects.RemoveItem 1
  
  'file is clean
  IsChanged = False
  
  'set caption
  Caption = "Objects Editor - "
  If InGame Then
    Caption = Caption & GameID
  Else
    Caption = Caption & CompactPath(ObjectFile, 75)
  End If
  
  'save filename
  '*'Debug.Assert tmpObjects.ResFile = ObjectFile
    
  'copy to local objects object
  Set ObjectsEdit = New AGIInventoryObjects
  ObjectsEdit.SetObjects tmpObjects
  
  'restore cursor
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub ShowRow()
  'ensures selected row in the object grid is visible
  
  'top row needs to be adjusted down by one because
  'the fixed row (which is zero) is not counted by toprow)
  Dim LastRow As Long
  Dim NewTop As Long
  Dim RowNum As Long
  
  'get selected row
  RowNum = fgObjects.Row
  
  'last full row is top row plus number of fully visible rows (not including
  'top row)
  'number of fully visible rows is grid height divided by cell height
  'adjust by 3 (compensate once for heading row, and once for zero based state
  'of object counting and once to ensure toprow is not counted twice)
  LastRow = fgObjects.TopRow + fgObjects.Height \ fgObjects.CellHeight - 3
  
  'if rownum is between toprow(adjusted) and lastrow
  If RowNum >= fgObjects.TopRow - 1 And RowNum <= LastRow Then
    Exit Sub
  End If
  
  'set toprow so rownum will be the third row visible
  NewTop = RowNum - 3
  If NewTop < 0 Then
    NewTop = 0
  End If
  fgObjects.TopRow = NewTop
End Sub


Private Function ValidateRoom() As Boolean

  Dim NewRoom As Long
  
  'any value from 0-255 is valid
  
  On Error GoTo ErrHandler
  
  NewRoom = Val(Me.txtRoomNo.Text)
  
  If NewRoom > 255 Then
    'warn user
    MsgBox "Room numbers may not be greater than 255." & vbCrLf & vbCrLf & _
    "Resetting value to 255. ", vbInformation + vbOKOnly, "Room Value Limit"
    NewRoom = 255
  End If
  
  'save new room value
  ModifyRoom EditItemNum, NewRoom
  ValidateRoom = True
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Function ValidateObject() As Boolean

  Dim i As Long, rtn As VbMsgBoxResult
  Dim blnNoWarn As Boolean
  
  On Error GoTo ErrHandler
  
  If rtfObject.Text <> "?" Then
    'check for duplicate name
    If Settings.WarnDupObj Then
      For i = 0 To ObjectsEdit.Count - 1
        If i <> EditItemNum Then
          If StrComp(ObjectsEdit(i).ItemName, UnicodeToCP(rtfObject.Text, SessionCodePage), vbBinaryCompare) = 0 Then
            'warn
            rtn = MsgBoxEx(ChrW$(39) & rtfObject.Text & "' already exists in this object list." & vbNewLine & "Do you want to keep this duplicate object?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Duplicate Object", WinAGIHelp, "htm\winagi\Objects_Editor.htm#duplicates", "Don't show this warning again.", blnNoWarn)
            'reset warning flag, if necessary
            Settings.WarnDupObj = Not blnNoWarn
            'if now hiding update settings file
            If Not Settings.WarnDupObj Then
              WinAGISettingsList.WriteSetting(sGENERAL, "WarnDupObj", Settings.WarnDupObj
            End If
            
            'if canceled,
            If rtn = vbNo Then
              ValidateObject = True
              Exit Function
            End If
            Exit For
          End If
        End If
      Next i
    End If
  End If

  'save change to item description
  ModifyItem EditItemNum, UnicodeToCP(rtfObject.Text, SessionCodePage)
  ValidateObject = True
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Private Sub fgObjects_EnterCell()
  'if in first column
  If fgObjects.Col = 0 Then
    'move to second column
    fgObjects.Col = 1
  End If
    
  SetEditMenu
End Sub

Private Sub fgObjects_GotFocus()

  'always hide the selection bars
  picSelFrame(0).Visible = False
  picSelFrame(2).Visible = False
  picSelFrame(3).Visible = False
  picSelFrame(1).Visible = False
  
End Sub

Private Sub fgObjects_KeyPress(KeyAscii As Integer)

  On Error GoTo ErrHandler
  
  'some keys should be ignored...?
  Select Case KeyAscii
  Case 9  'tab
    With fgObjects
      'if on name, move to column
      If .Col = ctDesc Then
        .Col = ctRoom
      Else
        If .Row < .Rows - 1 Then '(to account for header row)
          .Row = .Row + 1
          .Col = 1
        End If
      End If
    End With
    KeyAscii = 0
    
  Case 10, 13 'enter key
    'same as dbl-click
    fgObjects_DblClick
    KeyAscii = 0
    
  Case Else
    'if in the empty row, AND on description column, begin editing
    If fgObjects.Row = fgObjects.Rows - 1 And fgObjects.Col = ctDesc Then
      fgObjects_DblClick
      rtfObject.Text = ChrW$(KeyAscii)
      rtfObject.Selection.Range.StartPos = 1
    End If
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub fgObjects_LostFocus()

  'if editing, don't show
  If rtfObject.Visible Or txtRoomNo.Visible Then
    Exit Sub
  End If
  
  'if something on grid is selected
  picSelFrame(0).Left = fgObjects.CellLeft
  picSelFrame(0).Top = fgObjects.CellTop + fgObjects.Top
  picSelFrame(0).Width = fgObjects.CellWidth
  
  picSelFrame(2).Left = picSelFrame(0).Left
  picSelFrame(2).Top = picSelFrame(0).Top + fgObjects.CellHeight - picSelFrame(2).Height
  picSelFrame(2).Width = picSelFrame(0).Width
  
  picSelFrame(3).Left = picSelFrame(0).Left
  picSelFrame(3).Top = picSelFrame(0).Top
  picSelFrame(3).Height = fgObjects.CellHeight
  
  picSelFrame(1).Left = picSelFrame(0).Left + picSelFrame(0).Width - picSelFrame(1).Width
  picSelFrame(1).Top = picSelFrame(3).Top
  picSelFrame(1).Height = picSelFrame(3).Height
  
  picSelFrame(0).Visible = True
  picSelFrame(2).Visible = True
  picSelFrame(3).Visible = True
  picSelFrame(1).Visible = True
  
End Sub

Private Sub fgObjects_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim tmpRow As Long, tmpCol As Long
  Dim TopRow As Long, BtmRow As Long

  On Error GoTo ErrHandler
  
  If rtfObject.Visible Or txtRoomNo.Visible Then
    Exit Sub
  End If
  
  With fgObjects
    tmpCol = .MouseCol
    If tmpCol = 0 Then tmpCol = 1
    tmpRow = .MouseRow
    'first row is usually '?', but it's not mandatory
'''    If tmpRow < 2 Then tmpRow = 2
    If tmpRow < 1 Then tmpRow = 1
    
    'if right click
    If Button = vbRightButton Then
      'determine top/bottom rows
      If .Row < .RowSel Then
        TopRow = .Row
        BtmRow = .RowSel
      Else
        TopRow = .RowSel
        BtmRow = .Row
      End If

      'if row and column are NOT within selected area
      If tmpRow < TopRow Or tmpRow > BtmRow Then
        'make new selection
        'check for selection of entire row
        If X < ScreenTWIPSX * 12 Then
          'select entire row
          .Col = 0
          .Row = tmpRow
          .ColSel = 3
          .SelectionMode = flexSelectionByRow
          .Highlight = flexHighlightAlways
          .MergeCells = flexMergeNever
        Else
          'select freely
          .Col = tmpCol
          .Row = tmpRow
          .SelectionMode = flexSelectionFree
          .Highlight = flexHighlightNever
          .MergeCells = flexMergeRestrictAll
        End If
      End If

      'if on an editable row
'''      If tmpRow > 1 Then
      If tmpRow > 0 Then
        'set edit menu parameters
        SetEditMenu

        'make sure this form is the active form
        If Not (frmMDIMain.ActiveMdiChild Is Me) Then
          'set focus before showing the menu
          Me.SetFocus
        End If
        'need doevents so form activation occurs BEFORE popup
        'otherwise, errors will be generated because of menu
        'adjustments that are made in the form_activate event
        SafeDoEvents
        'show popup menu
        PopupMenu frmMDIMain.mnuEdit
      End If
      'done with right click activities
      Exit Sub
    End If

'     'check for selection of entire row
'    If X < ScreenTWIPSX * 12 Then
'      'select entire row
'      .Col = 0
'      .SelectionMode = flexSelectionByRow
'      .Highlight = flexHighlightAlways
'      .MergeCells = flexMergeNever
'      .ColSel = 2
'
'    Else
      'select freely
      If tmpCol <> -1 Then
        .Col = tmpCol
      Else
        .Col = ctDesc
      End If
      .Row = tmpRow
      .SelectionMode = flexSelectionFree
      .Highlight = flexHighlightNever
      .MergeCells = flexMergeRestrictAll
'    End If
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub fgObjects_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  'if left button is down, begin dragging
  If Button = vbLeftButton Then
    If Not DroppingObj Then
      'object i0 is not moveable or delete-able
      'if something selected,
      If fgObjects.Row > 1 And fgObjects.RowSel = fgObjects.Row Then
        fgObjects.OLEDrag
      End If
    End If
  End If
End Sub

Private Sub fgObjects_OLECompleteDrag(Effect As Long)

  'reset dragging flag
  DroppingObj = False
End Sub

Private Sub fgObjects_OLEStartDrag(Data As MSFlexGridLib.DataObject, AllowedEffects As Long)

  On Error GoTo ErrHandler
  
  'set global drop flag (so logics (or other text receivers) know
  'when an object is being dropped
  DroppingObj = True
  
  'set allowed effects to copy only
  AllowedEffects = vbDropEffectCopy
  Data.SetData fgObjects.TextMatrix(fgObjects.Row, 1), vbCFText
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_Activate()

  'if minimized, exit
  '(to deal with occasional glitch causing focus to lock up)
  If Me.WindowState = vbMinimized Then
    Exit Sub
  End If
  
  ActivateActions
  
  'if visible,
  If Visible Then
    'force resize
    Form_Resize
  End If
End Sub

Private Sub ActivateActions()
  
  On Error GoTo ErrHandler
  
  'if hiding prevwin on lost focus, hide it now
  If Settings.HidePreview Then
    PreviewWin.Hide
  End If
 
  'show object menu
 '*'Debug.Print "AdjustMenus 55"
  AdjustMenus rtObjects, InGame, True, IsChanged
  
  'set edit menu
  SetEditMenu
  
  'update status bar with Count (subtract one cuz first item doesnt Count)
  MainStatusBar.Panels("Count").Text = "Object Count: " & CStr(ObjectsEdit.Count - 1)
  MainStatusBar.Panels("Encrypt").Text = IIf(ObjectsEdit.Encrypted, "Encrypted", "Not Encrypted")
  
  'if findform is visible,
  If FindForm.Visible Then
    'set correct mode
    If FindForm.rtfReplace.Visible Then
      'show in replace object mode
      FindForm.SetForm ffReplaceObject, False
    Else
      'show in find object mode
      FindForm.SetForm ffFindObject, False
    End If
  End If
  
  'set searching form to this form
  Set SearchForm = Me
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
  
  'detect and respond to keyboard shortcuts
  
  'always check for help first
  If Shift = 0 And KeyCode = vbKeyF1 Then
    MenuClickHelp
    KeyCode = 0
    Exit Sub
  End If

  'if editing something
  If rtfObject.Visible Or txtRoomNo.Visible Or ActiveControl Is txtMaxScreenObj Then
    Exit Sub
  End If
  
  'check for global shortcut keys
  CheckShortcuts KeyCode, Shift
  If KeyCode = 0 Then
    Exit Sub
  End If
  
  Select Case Shift
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyZ 'undo
      If frmMDIMain.mnuEUndo.Enabled Then
        MenuClickUndo
        KeyCode = 0
      End If
      
    Case vbKeyF 'find
      If frmMDIMain.mnuEdit.Enabled Then
        MenuClickFind
        KeyCode = 0
      End If
      
    Case vbKeyH 'replace
      If frmMDIMain.mnuEReplace.Enabled Then
        MenuClickReplace
        KeyCode = 0
      End If
    End Select
   
  Case 0 'no shift, ctrl, alt
    Select Case KeyCode
    Case vbKeyDelete
      If frmMDIMain.mnuEDelete.Enabled Then
        MenuClickDelete
        KeyCode = 0
      End If
      
    Case vbKeyF3
      'find again
      If frmMDIMain.Enabled Then
        MenuClickFindAgain
        KeyCode = 0
      End If
    End Select
  
  Case vbShiftMask
    Select Case KeyCode
    Case vbKeyDelete
      If frmMDIMain.mnuEClear.Enabled Then
        MenuClickClear
        KeyCode = 0
      End If
    
    Case vbKeyInsert
      If frmMDIMain.mnuEInsert.Enabled Then
        MenuClickInsert
        KeyCode = 0
      End If
    End Select
    
  Case 3  'shift+ctrl
    Select Case KeyCode
    Case vbKeyF
      If frmMDIMain.mnuECustom2.Enabled Then
        MenuClickECustom2
        KeyCode = 0
      End If
    
    Case vbKeyE
      If frmMDIMain.mnuRCustom3.Enabled Then
        MenuClickCustom3
        KeyCode = 0
      End If
    End Select
  
  Case vbAltMask
    Select Case KeyCode
    Case vbKeyReturn
      If frmMDIMain.mnuECustom1.Enabled Then
        MenuClickECustom1
        KeyCode = 0
      End If
    End Select
  End Select
End Sub
Private Sub Form_Load()
  
  On Error GoTo ErrHandler
    
  CalcWidth = MIN_WIDTH
  CalcHeight = MIN_HEIGHT
  
  'initialize undo collection
  Set UndoCol = New Collection
  
  'set indent Value
  CellIndentH = ScreenTWIPSX * 3
  CellIndentV = ScreenTWIPSY * 1
  
  'setup fonts
  InitFonts
  'set code page to correct value
  rtfObject.CodePage = SessionCodePage
  
  'set grid format
  fgObjects.ColAlignment(0) = flexAlignRightCenter
  fgObjects.ColAlignment(1) = flexAlignLeftCenter
  fgObjects.ColAlignment(2) = flexAlignCenterCenter
  fgObjects.FixedAlignment(0) = flexAlignCenterCenter
  fgObjects.FixedAlignment(1) = flexAlignLeftCenter
  fgObjects.FixedAlignment(2) = flexAlignCenterCenter

  'add fixed row titles
  fgObjects.Clear
  fgObjects.TextMatrix(0, 0) = "Item #"
  fgObjects.TextMatrix(0, 1) = "Item Description"
  fgObjects.TextMatrix(0, 2) = "Room #"
  
  'reset search parameters
  FindForm.ResetSearch
  
#If DEBUGMODE <> 1 Then
  'subclass the flexgrid
  PrevFGWndProc = SetWindowLong(Me.fgObjects.hWnd, GWL_WNDPROC, AddressOf ScrollWndProc)
  'also the text boxes
  PrevTBMOWndProc = SetWindowLong(Me.txtMaxScreenObj.hWnd, GWL_WNDPROC, AddressOf TBWndProc)
  PrevTBRmWndProc = SetWindowLong(Me.txtRoomNo.hWnd, GWL_WNDPROC, AddressOf TBWndProc)
#End If
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
      If Not (frmMDIMain.ActiveMdiChild Is Me) Then
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

Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)
  
  Cancel = Not AskClose()
End Sub

Sub Form_Resize()

  Dim sngBorders As Single
  
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
  
  'if minimized or if the form is not visible
  If Me.WindowState = vbMinimized Or Not Visible Then
    Exit Sub
  End If
  
  'if restoring from minimize, activation may not have triggered
  If MainStatusBar.Tag <> CStr(rtObjects) Then
    ActivateActions
  End If
  
  sngBorders = Width - ScaleWidth
  
  fgObjects.Height = CalcHeight - fgObjects.Top
  
  'resize interior items
  'reposition Max objects stuff
  txtMaxScreenObj.Left = CalcWidth - txtMaxScreenObj.Width
  Label1.Left = txtMaxScreenObj.Left - Label1.Width
  
  fgObjects.Width = CalcWidth
  'if more rows than will fit on screen
  If fgObjects.Rows * fgObjects.RowHeight(0) > fgObjects.Height Then
    'account for scrollbar
    fgObjects.ColWidth(1) = fgObjects.Width - fgObjects.ColWidth(2) - fgObjects.ColWidth(0) - 320
  Else
    fgObjects.ColWidth(1) = fgObjects.Width - fgObjects.ColWidth(2) - fgObjects.ColWidth(0)
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub MenuClickECustom1()
  'edit object
  
  'first item CAN be edited
  
  'if on a valid row
  If fgObjects.Row > 0 Then
    'use double-click to force edit
    fgObjects_DblClick
  End If
  
End Sub
Public Sub MenuClickUndo()
  
  Dim NextUndo As ObjectsUndo
  Dim i As Long, strObjs() As String
  
  On Error GoTo ErrHandler
  
  'if there are no undo actions
  If UndoCol.Count = 0 Then
    'just exit
    Exit Sub
  End If
  
  'get next undo object
  Set NextUndo = UndoCol(UndoCol.Count)
  
  'remove undo object
  UndoCol.Remove UndoCol.Count
  'reset undo menu
  frmMDIMain.mnuEUndo.Enabled = (UndoCol.Count > 0)
  If frmMDIMain.mnuEUndo.Enabled Then
    frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(OBJUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
  Else
    frmMDIMain.mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
  End If
  
  'undo the action
  Select Case NextUndo.UDAction
  Case udoAddItem        'store object number that was added
    'delete the last item
    DeleteItem ObjectsEdit.Count - 1, True
    'select last item
    fgObjects.Row = fgObjects.Rows - 1
    fgObjects.Col = 1
  
  Case udoDeleteItem        'store object number, text, and room that was deleted
    'if object number is equal to current Count
    If NextUndo.UDObjectNo = ObjectsEdit.Count Then
      'need to add item to restore
      AddItem NextUndo.UDObjectText, NextUndo.UDObjectRoom, True
    Else
      'use modify item to restore
      ModifyItem NextUndo.UDObjectNo, NextUndo.UDObjectText, True
      ModifyRoom NextUndo.UDObjectNo, NextUndo.UDObjectRoom, True
    End If
    'select this item
    fgObjects.Row = NextUndo.UDObjectNo + 1
    fgObjects.Col = 1
    
  Case udoModifyItem, udoReplace    'store old object number, text
    ModifyItem NextUndo.UDObjectNo, NextUndo.UDObjectText, True
    'select this item
    fgObjects.Row = NextUndo.UDObjectNo + 1
    fgObjects.Col = 1
    
  Case udoModifyRoom    'store old object number, room
    ModifyRoom NextUndo.UDObjectNo, NextUndo.UDObjectRoom, True
    'select this item
    fgObjects.Row = NextUndo.UDObjectNo + 1
    fgObjects.Col = 2
  
  Case udoChangeDesc       'store old description
    'restore old id
    ObjectsEdit.Description = NextUndo.UDObjectText
    'if in a game
    If InGame Then
      'restore the ingame resource as well
      InventoryObjects.Description = NextUndo.UDObjectText
      InventoryObjects.Save
      'update prop window
      RefreshTree rtObjects, -1, umProperty
    End If
    
  Case udoChangeMaxObj     'store old maxobjects
    'old Max is stored in room variable
    ModifyMax NextUndo.UDObjectRoom, True
    
  Case udoTglEncrypt       'store old encryption Value
    'old encryption is stored in room variable
    ToggleEncryption CBool(NextUndo.UDObjectRoom), True
    
  Case udoClear            'restore old Objects
    'if undoing a clear, objectsedit is already empty
    'so don't need to clear it; grid will already be
    'empty also
    
    'restore Max objects
    ObjectsEdit.MaxScreenObjects = NextUndo.UDObjectRoom
    txtMaxScreenObj.Text = CStr(NextUndo.UDObjectRoom)
    
    'restore encryption
    ObjectsEdit.Encrypted = CBool(NextUndo.UDObjectNo)
    frmMDIMain.mnuRCustom3.Checked = ObjectsEdit.Encrypted
    MainStatusBar.Panels("Encrypt").Text = "Encrypted"
    If Not ObjectsEdit.Encrypted Then
      MainStatusBar.Panels("Encrypt").Text = "Not " & MainStatusBar.Panels("Encrypt").Text
    End If
    
    'split out items
    strObjs = Split(NextUndo.UDObjectText, vbCr)
    
    'add them back to grid and the objectedit object
    For i = 0 To UBound(strObjs)
      If i <> 0 Then
        fgObjects.AddItem CStr(i) & "." & vbTab & strObjs(i)
        ObjectsEdit.Add fgObjects.TextMatrix(i + 1, ctDesc), CByte(fgObjects.TextMatrix(i + 1, ctRoom))
      End If
    Next i
    
    'update status bar with Count (subtract one cuz first item doesnt Count)
    MainStatusBar.Panels("Count").Text = "Object Count: " & CStr(i - 1)
    
  Case udoReplaceAll
    'udstring has previous items in pipe-delimited string
    strObjs = Split(NextUndo.UDObjectText, "|")
    
    'object numbers and old values are in pairs
    For i = 0 To UBound(strObjs) Step 2
      'first element is obj number, second is old obj desc
      ObjectsEdit(CLng(strObjs(i))).ItemName = strObjs(i + 1)
      'update grid
      fgObjects.TextMatrix(CLng(strObjs(i)) + 1, ctDesc) = strObjs(i + 1)
      
    Next i
    
  End Select
  
  'ensure selected row is visible
  ShowRow
Exit Sub

ErrHandler:
  Resume Next
End Sub

Public Sub SetEditMenu()
  
  'sets the menu captions on the Edit menu
  'based on current selection
  
  On Error GoTo ErrHandler
  
  'always force form to current
  If Not frmMDIMain.ActiveMdiChild Is Nothing Then
    If Not (frmMDIMain.ActiveMdiChild Is Me) And Visible Then
      Me.SetFocus
    End If
  End If
  
  With frmMDIMain
    .mnuEdit.Enabled = True
    'redo, cut, copy, paste, select all are hidden
    .mnuERedo.Visible = False
    .mnuECut.Visible = False
    .mnuECopy.Visible = False
    .mnuEPaste.Visible = False
    .mnuESelectAll.Visible = False
    .mnuECustom3.Visible = False
    .mnuECustom4.Visible = False
    
    .mnuEUndo.Visible = True
    'if there is something to undo
    If UndoCol.Count > 0 Then
      .mnuEUndo.Caption = "&Undo " & LoadResString(OBJUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
      .mnuEUndo.Enabled = True
    Else
      .mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
      .mnuEUndo.Enabled = False
    End If
    .mnuEBar0.Visible = True
    
    .mnuEDelete.Visible = True
    'item i0 can not be deleted
    .mnuEDelete.Enabled = fgObjects.Row > 1
    .mnuEDelete.Caption = "&Delete Item" & vbTab & "Del"
    
    .mnuEClear.Visible = True
    .mnuEClear.Enabled = True
    .mnuEClear.Caption = "&Clear List" & vbTab & "Shift+Del"
    
    .mnuEInsert.Visible = True
    .mnuEInsert.Enabled = ObjectsEdit.Count < 256
    .mnuEInsert.Caption = "&Add Item" & vbTab & "Shift+Ins"
    
    .mnuEBar1.Visible = True
    .mnuEFind.Visible = True
    .mnuEFind.Enabled = True
    .mnuEFind.Caption = "&Find" & vbTab & "Ctrl+F"
    
    .mnuEFindAgain.Visible = True
    .mnuEFindAgain.Enabled = LenB(GFindText) <> 0
    .mnuEFindAgain.Caption = "Find A&gain" & vbTab & "F3"
    
    .mnuEReplace.Visible = True
    .mnuEReplace.Enabled = True
    .mnuEReplace.Caption = "Replace" & vbTab & "Ctrl+H"
    
    'enable findinlogics if object is not a '?'
    .mnuEBar2.Visible = True
    .mnuECustom2.Visible = True
    .mnuECustom2.Enabled = (fgObjects.TextMatrix(fgObjects.Row, ctDesc) <> "?")
    .mnuECustom2.Caption = "Find In Logics" & vbTab & "Shift+Ctrl+F"
    
    .mnuECustom1.Visible = True
    'although not normal, object i0 can be edited
    .mnuECustom1.Enabled = fgObjects.Row > 0
    .mnuECustom1.Caption = "&Edit Item" & vbTab & "Alt+Enter"
    
    'set menu status for encryption
    If Not ObjectsEdit Is Nothing Then
      .mnuRCustom3.Checked = ObjectsEdit.Encrypted
    End If
    'check for Amiga format
    .mnuRCustom2.Visible = ObjectsEdit.AmigaOBJ
    If ObjectsEdit.AmigaOBJ Then
      .mnuRCustom2.Caption = "Convert AMIGA Format to DOS"
    End If
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub fgObjects_DblClick()
  
  On Error GoTo ErrHandler
  
  Dim blnNoWarn As Boolean
  
  'save row and column being edited
  EditRow = fgObjects.Row
  EditCol = fgObjects.Col
  EditItemNum = fgObjects.Row - 1

  'if on editable item row
  If fgObjects.Row > 0 Then
    'begin edit
    Select Case EditCol
    Case ctNumber, ctDesc
      'disable edit menu
      frmMDIMain.mnuEdit.Enabled = False
      
      EditObj
      
    Case ctRoom
      'disable edit menu
      frmMDIMain.mnuEdit.Enabled = False

      'begin edit of room
      With txtRoomNo
        .Move fgObjects.CellLeft + CellIndentH, fgObjects.CellTop + fgObjects.Top + CellIndentV, fgObjects.CellWidth - CellIndentH, fgObjects.CellHeight - CellIndentV
        .Text = ObjectsEdit(fgObjects.Row - 1).Room
        .Visible = True
        'select all
        .SelStart = 0
        .SelLength = Len(.Text)
        .SetFocus
      End With
    End Select
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Sub fgObjects_KeyDown(KeyCode As Integer, Shift As Integer)
  
  If (fgObjects.Rows >= 1) Then
    Select Case KeyCode
    Case 46
      DeleteItem fgObjects.Row - 1
    Case 48
      MenuClickInsert
    End Select
  End If
End Sub
Public Function AskClose() As Boolean

  Dim rtn As VbMsgBoxResult

  'assume ok to close
  AskClose = True
  
  On Error GoTo ErrHandler
  'if object list has been modified since last save,
  If IsChanged Then
    'get user input
    rtn = MsgBox("Do you want to save changes to this objects file before closing?", vbYesNoCancel, "Objects Editor")
    
    'if user wants to save
    If rtn = vbYes Then
      'save by calling the menuclick method
      MenuClickSave
    End If
    
    'return false if cancel was selected
    AskClose = rtn <> vbCancel
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Private Sub Form_Unload(Cancel As Integer)

  'ensure object is cleared and dereferenced
  If Not ObjectsEdit Is Nothing Then
    '*'Debug.Assert ObjectsEdit.Loaded
    ObjectsEdit.Unload
    Set ObjectsEdit = Nothing
  End If
  
  'if this is the ingame
  If InGame Then
    'reset inuse flag
    OEInUse = False
    'release the object
    Set ObjectEditor = Nothing
  End If

#If DEBUGMODE <> 1 Then
  'release subclass hook to flexgrid
  SetWindowLong Me.fgObjects.hWnd, GWL_WNDPROC, PrevFGWndProc
  'and the text boxes
  SetWindowLong Me.txtMaxScreenObj.hWnd, GWL_WNDPROC, PrevTBMOWndProc
  SetWindowLong Me.txtRoomNo.hWnd, GWL_WNDPROC, PrevTBRmWndProc
#End If
  'need to check if this is last form
  LastForm Me
End Sub

Private Sub picSelFrame_MouseDown(Index As Integer, Button As Integer, Shift As Integer, X As Single, Y As Single)
  'pass focus to grid
  fgObjects.SetFocus
End Sub

Private Sub txtMaxScreenObj_KeyDown(KeyCode As Integer, Shift As Integer)

  Dim strCBText As String, blnPasteOK As Boolean
  
  On Error GoTo ErrHandler
  
  'need to handle cut, copy, paste, select all shortcuts
  Select Case Shift
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyX 'cut
      'only is something selected
      If txtMaxScreenObj.SelLength > 0 Then
        'put the selected text into clipboard
        Clipboard.Clear
        Clipboard.SetText txtMaxScreenObj.SelText
        'then delete it
        txtMaxScreenObj.SelText = ""
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyC 'copy
      'only is something selected
      If txtMaxScreenObj.SelLength > 0 Then
        'put the selected text into clipboard
        Clipboard.Clear
        Clipboard.SetText txtMaxScreenObj.SelText
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyV 'paste
      'paste only allowed if clipboard text is a valid number
      strCBText = Clipboard.GetText
      ' put a zero in front, just in case it's a hex or octal
      ' string; we don't want those
      If IsNumeric("0" & strCBText) And Len(strCBText) > 0 Then
        'only integers
        If Int(strCBText) = Val(strCBText) Then
          'range 0-255
          If Val(strCBText) >= 0 And Val(strCBText) <= 255 Then
            blnPasteOK = True
          Else
            blnPasteOK = False
          End If
        Else
          blnPasteOK = False
        End If
      Else
        blnPasteOK = False
      End If
      
      If blnPasteOK Then
        'put cbtext into selection
        txtMaxScreenObj.SelText = strCBText
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyA 'select all
      txtMaxScreenObj.SelStart = 0
      txtMaxScreenObj.SelLength = Len(txtMaxScreenObj.Text)
      KeyCode = 0
      Shift = 0
    End Select
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub txtMaxScreenObj_KeyPress(KeyAscii As Integer)

  'allow only numbers, backspace, and delete
  
  On Error GoTo ErrHandler
  
  Select Case KeyAscii
  Case 8  ' backspace
    'ignore if no characters
  
  Case 48 To 57 'numbers
  Case 9, 10, 13 'enter or tab
    'if result is valid,
    If ValidateMaxSObj() Then
      'set focus to grid
      fgObjects.SetFocus
      'reenable edit menu
      frmMDIMain.mnuEdit.Enabled = True
      
    Else
      'need to force focus (might be a tab thing?)
      With txtMaxScreenObj
        .SelStart = 0
        .SelLength = Len(.Text)
        .SetFocus
      End With
    End If
    
    'ignore key
    KeyAscii = 0
    
  Case 27 'escape
    'restore value
    txtMaxScreenObj.Text = CStr(ObjectsEdit.MaxScreenObjects)

    'set focus to grid
    fgObjects.SetFocus
    'reenable edit menu
    frmMDIMain.mnuEdit.Enabled = True
  
    'ignore key
    KeyAscii = 0
    
  Case Else
    'ignore all other keys
    KeyAscii = 0
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function ValidateMaxSObj() As Boolean
  
  Dim NewMax As Long
  
  On Error GoTo ErrHandler
  
  'assume ok until proven otherwise
  ValidateMaxSObj = True
  
  If Val(txtMaxScreenObj.Text) <= 0 Then
    'force it to 0 and warn user
    NewMax = 0
    If MsgBoxEx("Setting MaxScreenObject to 0 means only ego can be animated." & vbCrLf & _
    "Are you sure you want to set this value?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Zero Screen Objects", WinAGIHelp, "htm\agi\screenobjs.htm#maxscreenobjs") = vbNo Then
      'fail validation
      ValidateMaxSObj = False
    End If
    
  ElseIf Val(txtMaxScreenObj.Text) > 255 Then
    'force it to 255
    NewMax = 255
    'ask user to validate
    If MsgBoxEx("Largest value for MaxScreenObject is 255. Setting a value this" & vbNewLine & _
                "high will likely result in running out of memory. Are you sure" & vbNewLine & "you want to set this value?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "High Max Screen Object Count", WinAGIHelp, "htm\agi\screenobjs.htm#maxscreenobjs") = vbNo Then
      'fail validation
      ValidateMaxSObj = False
    End If
    
  Else
    'use it if it's a valid byte value
    NewMax = Val(txtMaxScreenObj.Text)
    
    'check for abnormally small or large values
    If NewMax < 16 Then
      If MsgBoxEx("Less than 16 screen objects is unusually low." & vbCrLf & _
      "Are you sure you want to set this value?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Low Max Screen Object Count", WinAGIHelp, "htm\agi\screenobjs.htm#maxscreenobjs") = vbNo Then
        'fail validation
        ValidateMaxSObj = False
      End If
    ElseIf NewMax > 48 Then
      If MsgBoxEx("More than 48 screen objects is unusually high." & vbCrLf & _
      "Are you sure you want to set this value?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "High Max Screen Object Count", WinAGIHelp, "htm\agi\screenobjs.htm#maxscreenobjs") = vbNo Then
        'fail validation
        ValidateMaxSObj = False
      End If
    End If
  End If
  
  If ValidateMaxSObj Then
    'if new value is OK, keep it,
    ModifyMax NewMax
  Else
    'reset and highlight the text box
    txtMaxScreenObj.Text = CStr(ObjectsEdit.MaxScreenObjects)
    txtMaxScreenObj.SelStart = 0
    txtMaxScreenObj.SelLength = Len(txtMaxScreenObj.Text)
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Sub txtMaxScreenObj_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim strCBText As String
  
  On Error GoTo ErrHandler
  
  ' check for right click to show context menu
  If Button = vbRightButton Then
    'configure the edit menu
    With frmMDIMain
      .mnuTBCopy.Enabled = txtMaxScreenObj.SelLength > 0
      .mnuTBCut.Enabled = .mnuTBCopy.Enabled
      'paste only allowed if clipboard text is a valid number
      strCBText = Clipboard.GetText
      ' put a zero in front, just in case it's a hex or octal
      ' string; we don't want those
      If IsNumeric("0" & strCBText) And Len(strCBText) > 0 Then
        'only integers
        If Int(strCBText) = Val(strCBText) Then
          'range 0-255
          If Val(strCBText) >= 0 And Val(strCBText) <= 255 Then
            .mnuTBPaste.Enabled = True
          Else
            .mnuTBPaste.Enabled = False
          End If
        Else
          .mnuTBPaste.Enabled = False
        End If
      Else
        .mnuTBPaste.Enabled = False
      End If
      
      'char picker not used here
      .mnuTBSeparator1.Visible = False
      .mnuTBCharMap.Visible = False
      
      ' set the command operation to none
      TBCmd = 0
      
      'now show the popup menu
      PopupMenu .mnuTBPopup
    End With
    
    'deal with the result
    Select Case TBCmd
    Case 0 'canceled
      'do nothing
      Exit Sub
    Case 1 'cut
      'put the selected text into clipboard
      Clipboard.Clear
      Clipboard.SetText txtMaxScreenObj.SelText
      'then delete it
      txtMaxScreenObj.SelText = ""
    Case 2 'copy
      'put the selected text into clipboard
      Clipboard.Clear
      Clipboard.SetText txtMaxScreenObj.SelText
    Case 3 'paste
      'put cbtext into selection
      txtMaxScreenObj.SelText = strCBText
    Case 4 'select all
      txtMaxScreenObj.SelStart = 0
      txtMaxScreenObj.SelLength = Len(txtMaxScreenObj.Text)
    End Select
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub txtMaxScreenObj_Validate(Cancel As Boolean)

  'this will handle cases where user tries to 'click' on something,
  
  On Error GoTo ErrHandler
  
  'if OK, hide the text box
  If ValidateMaxSObj() Then
    'set focus to grid
    fgObjects.SetFocus
  Else
  'if not OK, cancel
    Cancel = True
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub rtfObject_Change()

  On Error GoTo ErrHandler
  
  'if this is last row,
  If EditRow = fgObjects.Rows - 1 Then
''    'add another row
''    fgObjects.AddItem vbNullString
''    'adjust column widths by resizing
''    Form_Resize
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub rtfObject_KeyDown(KeyCode As Integer, Shift As Integer)

  Dim strCBText As String
  
  On Error GoTo ErrHandler
  
  'to avoid the annoying 'ding' when pressing ENTER in a single line
  'richtext control, also need to ignore it in KeyDown event
  If KeyCode = 13 Then
    KeyCode = 0
  End If
  'if no characters, also ignore backspace
  If KeyCode = 8 And Len(rtfObject.Text) = 0 Then
    KeyCode = 0
  End If
  
  'always check for help first
  If Shift = 0 And KeyCode = vbKeyF1 Then
    MenuClickHelp
    KeyCode = 0
    Exit Sub
  End If
  
  'need to handle cut, copy, paste, select all shortcuts
  Select Case Shift
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyX 'cut
      'only is something selected
      If rtfObject.Selection.Range.Length > 0 Then
        'put the selected text into clipboard
        rtfObject.Selection.Range.Cut
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyC 'copy
      'only is something selected
      If rtfObject.Selection.Range.Length > 0 Then
        'put the selected text into clipboard
        rtfObject.Selection.Range.Copy
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyV 'paste
      ' put cbtext into selection
      ' trim white space off clipboard text, and no multi-line text
      strCBText = Trim$(Clipboard.GetText)
      strCBText = Replace(strCBText, vbCr, "")
      strCBText = Replace(strCBText, vbLf, "")
      'paste only allowed if clipboard has text
      If Len(strCBText) > 0 Then
        'put cbtext into selection
'''        rtfObject.Selection.Range.Paste
        rtfObject.Selection.Range.TextB = strCBText
        rtfObject.Selection.Range.Collapse reEnd
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyA 'select all
      rtfObject.Range.SelectRange
      KeyCode = 0
      Shift = 0
      
    Case vbKeyInsert
      'set flag so other controls know charpicker is active
      PickChar = True
      ShowCharPickerForm rtfObject
      KeyCode = 0
      Shift = 0
      'done with charpicker
      PickChar = False
    End Select
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub rtfObject_KeyPress(KeyAscii As Integer)
  
  On Error GoTo ErrHandler
  
  Select Case KeyAscii
  Case 9, 10, 13 'enter or tab
    'if result is valid,
    If ValidateObject() Then
      'hide the box
      rtfObject.Visible = False
      
      fgObjects.SetFocus
      'reenable edit menu
      frmMDIMain.mnuEdit.Enabled = True
      
      'tab moves to next column and begins editing
      If KeyAscii = 9 Then
        'move to Room column
        fgObjects.Col = ctRoom
        'start another edit operation
        fgObjects_DblClick
      End If
    Else
      'need to force focus so issue can be fixed
      rtfObject.SetFocus
    End If
    
    'ignore key
    KeyAscii = 0
    
  Case 27 'escape
    'hide the textbox without saving text
    rtfObject.Visible = False
    
    'ignore key
    KeyAscii = 0
  End Select
  
  'if new object is blank
  If LenB(fgObjects.TextMatrix(EditRow, ctDesc)) = 0 Then
    MenuClickDelete
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub rtfObject_MouseDown(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  Dim strCBText As String
  
  On Error GoTo ErrHandler
  
  ' check for right click to show context menu
  If Button = vbRightButton Then
    'configure the edit menu
    With frmMDIMain
      .mnuTBCopy.Enabled = rtfObject.Selection.Range.Length > 0
      .mnuTBCut.Enabled = .mnuTBCopy.Enabled
      ' put cbtext into selection
      ' trim white space off clipboard text, and no multi-line text
      strCBText = Trim$(Clipboard.GetText)
      strCBText = Replace(strCBText, vbCr, "")
      strCBText = Replace(strCBText, vbLf, "")
      'paste only allowed if clipboard has text
      If Len(strCBText) > 0 Then
          .mnuTBPaste.Enabled = True
      Else
        .mnuTBPaste.Enabled = False
      End If
      
      'char picker always available
      .mnuTBSeparator1.Visible = True
      .mnuTBCharMap.Visible = True
      
      ' set the command operation to none
      TBCmd = 0
      
      'now show the popup menu
      PopupMenu .mnuTBPopup
    End With
    
    'deal with the result
    Select Case TBCmd
    Case 0 'canceled
      'do nothing
      Exit Sub
    Case 1 'cut
      'put the selected text into clipboard
      rtfObject.Selection.Range.Cut
    
    Case 2 'copy
      'put the selected text into clipboard
      rtfObject.Selection.Range.Copy
      
    Case 3 'paste
      'put cbtext into selection
      rtfObject.Selection.Range.TextB = strCBText
      rtfObject.Selection.Range.Collapse reEnd
    Case 4 'select all
      rtfObject.Range.SelectRange
    Case 5 ' show char picker
      'set flag so other controls know charpicker is active
      PickChar = True
      ShowCharPickerForm rtfObject
      'done with charpicker
      PickChar = False
    End Select
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub rtfObject_Validate(Cancel As Boolean)
  
  'this will handle cases where user tries to 'click' on something,
  
  On Error GoTo ErrHandler
  
  If Not rtfObject.Visible Then Exit Sub
  
  'if OK, hide the text box
  If ValidateObject() Then
    rtfObject.Visible = False
  Else
  'if not OK, cancel
    Cancel = True
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub



Private Sub txtRoomNo_KeyDown(KeyCode As Integer, Shift As Integer)

  Dim strCBText As String, blnPasteOK As Boolean
  
  On Error GoTo ErrHandler
  
  'need to handle cut, copy, paste, select all shortcuts
  Select Case Shift
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyX 'cut
      'only is something selected
      If txtRoomNo.SelLength > 0 Then
        'put the selected text into clipboard
        Clipboard.Clear
        Clipboard.SetText txtRoomNo.SelText
        'then delete it
        txtRoomNo.SelText = ""
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyC 'copy
      'only is something selected
      If txtRoomNo.SelLength > 0 Then
        'put the selected text into clipboard
        Clipboard.Clear
        Clipboard.SetText txtRoomNo.SelText
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyV 'paste
      'paste only allowed if clipboard text is a valid number
      strCBText = Clipboard.GetText
      ' put a zero in front, just in case it's a hex or octal
      ' string; we don't want those
      If IsNumeric("0" & strCBText) And Len(strCBText) > 0 Then
        'only integers
        If Int(strCBText) = Val(strCBText) Then
          'range 0-255
          If Val(strCBText) >= 0 And Val(strCBText) <= 255 Then
            blnPasteOK = True
          Else
            blnPasteOK = False
          End If
        Else
          blnPasteOK = False
        End If
      Else
        blnPasteOK = False
      End If
      
      If blnPasteOK Then
        'put cbtext into selection
        txtRoomNo.SelText = strCBText
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyA 'select all
      txtRoomNo.SelStart = 0
      txtRoomNo.SelLength = Len(txtRoomNo.Text)
      KeyCode = 0
      Shift = 0
    End Select
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub txtRoomNo_KeyPress(KeyAscii As Integer)

  'allow only numbers, backspace, and delete
  On Error GoTo ErrHandler
  
  Select Case KeyAscii
  Case 48 To 57, 8  'numbers and backspace
  Case 9, 10, 13 'enter or tab
    'if result is valid,
    If ValidateRoom() Then
      'hide the box
      txtRoomNo.Visible = False
      fgObjects.SetFocus
      'reenable edit menu
      frmMDIMain.mnuEdit.Enabled = True
      
      'tab moves to next row, description
      If KeyAscii = 9 Then
        'if on last row
        If fgObjects.Row = fgObjects.Rows - 1 Then
''          'add a new row
''          fgObjects.AddItem vbNullString
        Else
          'move to next column, next row
          fgObjects.Col = 1
          fgObjects.Row = fgObjects.Row + 1
          
          'start another edit operation
          fgObjects_DblClick
        End If
        
      End If
    Else
      'need to force focus (might be a tab thing?)
      txtRoomNo.SetFocus
    End If
      
    'ignore key
    KeyAscii = 0
    
  Case 27 'escape
    'hide textbox without saving text
    txtRoomNo.Visible = False
    
    'ignore key
    KeyAscii = 0
    fgObjects.SetFocus
    'reenable edit menu
    frmMDIMain.mnuEdit.Enabled = True
  
  Case Else
    'ignore all other keys
    KeyAscii = 0
  End Select
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub txtRoomNo_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim strCBText As String
  
  On Error GoTo ErrHandler
  
  ' check for right click to show context menu
  If Button = vbRightButton Then
    'configure the edit menu
    With frmMDIMain
      .mnuTBCopy.Enabled = txtRoomNo.SelLength > 0
      .mnuTBCut.Enabled = .mnuTBCopy.Enabled
      'paste only allowed if clipboard text is a valid number
      strCBText = Clipboard.GetText
      ' put a zero in front, just in case it's a hex or octal
      ' string; we don't want those
      If IsNumeric("0" & strCBText) And Len(strCBText) > 0 Then
        'only integers
        If Int(strCBText) = Val(strCBText) Then
          'range 0-255
          If Val(strCBText) >= 0 And Val(strCBText) <= 255 Then
            .mnuTBPaste.Enabled = True
          Else
            .mnuTBPaste.Enabled = False
          End If
        Else
          .mnuTBPaste.Enabled = False
        End If
      Else
        .mnuTBPaste.Enabled = False
      End If
      
      ' set the command operation to none
      TBCmd = 0
      
      'now show the popup menu
      PopupMenu .mnuTBPopup
    End With
    
    'deal with the result
    Select Case TBCmd
    Case 0 'canceled
      'do nothing
      Exit Sub
    Case 1 'cut
      'put the selected text into clipboard
      Clipboard.Clear
      Clipboard.SetText txtRoomNo.SelText
      'then delete it
      txtRoomNo.SelText = ""
    Case 2 'copy
      'put the selected text into clipboard
      Clipboard.Clear
      Clipboard.SetText txtRoomNo.SelText
    Case 3 'paste
      'put cbtext into selection
      txtRoomNo.SelText = strCBText
    Case 4 'select all
      txtRoomNo.SelStart = 0
      txtRoomNo.SelLength = Len(txtRoomNo.Text)
    End Select
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub txtRoomNo_Validate(Cancel As Boolean)

  'this will handle cases where user tries to 'click' on something,
  'but NOT when keys are pressed?
  
  On Error GoTo ErrHandler
  If Not txtRoomNo.Visible Then Exit Sub
  
  'if OK, hide the text box
  If ValidateRoom() Then
    txtRoomNo.Visible = False
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

        private void button1_Click(object sender, EventArgs e) {
            EditInvList.Clear();
            if (EditInvList.Count > 1) {
                label1.Text = $"Item 1: {EditInvList[1].ItemName}";
            }
            else {
                label1.Text = "Only one item";
            }
            label2.Text = $"Max SObj: {EditInvList.MaxScreenObjects}";
            MarkAsChanged();
        }

        #endregion

        public bool LoadOBJECT(InventoryList objectobj) {

            InGame = objectobj.InGame;
            IsChanged = objectobj.IsChanged;
            try {
                if (InGame) {
                    objectobj.Load();
                }
            }
            catch {
                return false;
            }
            if (objectobj.ErrLevel < 0) {
                return false; ;
            }
            EditInvList = objectobj.Clone();
            EditInvListFilename = objectobj.ResFile;



            // TODO: setup form for editing
            label1.Text = $"Max SObj: {EditInvList.MaxScreenObjects}";
            for (int i = 0; i < EditInvList.Count; i++) {
                lstItems.Items.Add(i.ToString() + ": " + EditInvList[(byte)i].ItemName);
            }



            Text = "Objects Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                if (EditInvListFilename.Length > 0) {
                    Text += Common.Base.CompactPath(EditInvListFilename, 75);
                }
                else {
                    ObjCount++;
                    Text += "NewObjects" + ObjCount.ToString();
                }
            }
            if (IsChanged) {
                Text = sDM + Text;
            }

            mnuRSave.Enabled = !IsChanged;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = !IsChanged;
            return true;
        }

        public void ImportObjects(string importfile) {
            InventoryList tmpList;

            MDIMain.UseWaitCursor = true;
            try {
                tmpList = new(importfile);
            }
            catch (Exception e) {
                ErrMsgBox(e, "An error occurred during import:", "", "Import Object File Error");
                MDIMain.UseWaitCursor = false;
                return;
            }
            // replace current objectlist
            EditInvList = tmpList;
            EditInvListFilename = importfile;
            // TODO: refresh the editor
            if (EditInvList.Count > 1) {
                label1.Text = $"Item 1: {EditInvList[1].ItemName}";
            }
            else {
                label1.Text = "Only one item";
            }
            label2.Text = $"Max SObj: {EditInvList.MaxScreenObjects}";
            MarkAsChanged();
            MDIMain.UseWaitCursor = false;
        }

        public void SaveObjects() {
            // TODO: AutoUpdate feature still needs significant work; disable it for now
            //bool blnDontAsk = false;
            //DialogResult rtn = DialogResult.No;
            //if (InGame && WinAGISettings.AutoUpdateObjects != 1) {
            //    if (WinAGISettings.AutoUpdateObjects == 0) {
            //         = MsgBoxEx.Show(MDIMain,
            //            "Do you want to update all game logics with the changes made in the object list?",
            //            "Update Logics?",
            //            MessageBoxButtons.YesNo,
            //            MessageBoxIcon.Question,
            //            "Always take this action when saving the object list.", ref blnDontAsk);
            //        if (blnDontAsk) {
            //            if (rtn == DialogResult.Yes) {
            //                WinAGISettings.AutoUpdateObjects = 2;
            //            }
            //            else {
            //                WinAGISettings.AutoUpdateObjects = 1;
            //            }
            //        }
            //    }
            //    else {
            //        rtn = DialogResult.Yes;
            //    }
            //    if (rtn == DialogResult.Yes) {
            //        // test cmds that use IObj:
            //        //   has, obj.in.room
            //        //
            //        // action cmds that use IObj:
            //        //   get, drop, put
            //        FindForm.Visible = false;
            //        MDIMain.UseWaitCursor = true;
            //        ProgressWin.Text = "Updating Objects in Logics";
            //        ProgressWin.lblProgress.Text = "Searching...";
            //        ProgressWin.pgbStatus.Maximum = EditInvList.Count - 1;
            //        ProgressWin.pgbStatus.Value = 0;
            //        ProgressWin.Show(MDIMain);
            //        ProgressWin.Refresh();
            //        for (int i = 1; i < EditGame.InvObjects.Count; i++) {
            //            if (i >= EditInvList.Count) {
            //                // mark all objects in logics as deleted
            //                ReplaceAll("\"" + EditGame.InvObjects[i].ItemName + "\"", "i" + i.ToString(), fdAll, true, true, flAll, AGIResType.Objects);
            //            }
            //            else {
            //                if (EditInvList[i].ItemName == "?") {
            //                    // mark all objects in logics as deleted
            //                    ReplaceAll("\"" + EditGame.InvObjects[i].ItemName + "\"", "i" + i.ToString(), fdAll, true, true, flAll, AGIResType.Objects);
            //                }
            //                else if (EditInvList[i].ItemName != EditGame.InvObjects[i].ItemName) {
            //                    // change to new object item name
            //                    ReplaceAll("\"" + EditGame.InvObjects[i].ItemName + "\"", "\"" + EditInvList[i].ItemName + "\"", fdAll, true, true, flAll, AGIResType.Objects);
            //                }
            //            }
            //            ProgressWin.pgbStatus.Value = i;
            //            ProgressWin.Refresh();
            //        }
            //        ProgressWin.Close();
            //        MDIMain.UseWaitCursor = false;
            //    }
            //}

            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool loaded = EditGame.InvObjects.Loaded;
                if (!loaded) {
                    EditGame.InvObjects.Load();
                }
                EditGame.InvObjects.CloneFrom(EditInvList);
                try {
                    EditGame.InvObjects.Save();
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "Error during OBJECT compilation: ",
                        "Existing OBJECT has not been modified.",
                        "OBJECT Compile Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                MakeAllChanged();
                RefreshTree(AGIResType.Objects, 0);
                MDIMain.ClearWarnings(AGIResType.Objects, 0);
                if (!loaded) {
                    EditGame.InvObjects.Unload();
                }
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditInvList.ResFile.Length == 0) {
                    ExportObjects();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    try {
                        EditInvList.Save();
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex, "An error occurred while trying to save object list: ",
                            "Existing object list has not been modified.",
                            "Object List Save Error");
                        MDIMain.UseWaitCursor = false;
                        return;
                    }
                    MDIMain.UseWaitCursor = false;
                }
            }
            MarkAsSaved();
        }

        public void ExportObjects() {
            bool retval = Base.ExportObjects(EditInvList, InGame);
            if (!InGame && retval) {
                EditInvListFilename = EditInvList.ResFile;
                MarkAsSaved();
            };
        }

        public void EditProperties() {
            string strDesc = EditInvList.Description;
            string id = "";
            if (GetNewResID(AGIResType.Objects, -1, ref id, ref strDesc, InGame, 2)) {
                EditInvList.Description = strDesc;
                MDIMain.RefreshPropertyGrid(AGIResType.Objects, 0);
            }
        }

        private bool AskClose() {
            if (EditInvList.ErrLevel < 0) {
                // if exiting due to error on form load
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this OBJECT file?",
                    "Save OBJECT",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveObjects();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "OBJECT file not saved. Continue closing anyway?",
                            "Save OBJECT",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        return rtn == DialogResult.Yes;
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
                case DialogResult.No:
                    break;
                }
            }
            return true;
        }

        void MarkAsChanged() {
            // ignore when loading (not visible yet)
            if (!Visible) {
                return;
            }
            if (!IsChanged) {
                IsChanged = true;
                mnuRSave.Enabled = true;
                MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = true;
                Text = sDM + Text;
            }
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = "Objects Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                Text += Common.Base.CompactPath(EditInvListFilename, 75);
            }
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
        }

        internal void InitFonts() {
            // TODO: after finalizing form layout, need to adjust font init
            lstItems.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
        }
    }
}
