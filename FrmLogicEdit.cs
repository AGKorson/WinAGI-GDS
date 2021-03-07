using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor
{
  public partial class frmLogicEdit : Form
  {
    public Logic ThisLogic = new Logic { };
    internal ELogicFormMode FormMode;
    internal bool InGame;
    public bool ListDirty = false;

    public frmLogicEdit()
    {
      InitializeComponent();
    }
    public bool EditLogic(Logic ThisLogic)
    {
      return true;
      /*
        'set ingame flag based on logic passed
        InGame = ThisLogic.Resource.InGame

        'set number if this logic is in a game
        If InGame Then
          LogicNumber = ThisLogic.Number
        Else
          'use a number that can never match
          'when searches for open logics are made
          LogicNumber = 256
        End If

        'create new logic object for the editor
        Set LogicEdit = New AGILogic

        '*'Debug.Assert LogicEdit.Loaded
        'copy the passed logic to the editor logic
        LogicEdit.SetLogic ThisLogic

        'assign source code to editor
        If FileExists(LogicEdit.SourceFile) Then
          rtfLogic.LoadFile LogicEdit.SourceFile, reOpenSaveText, 437
        Else
          rtfLogic.Text = LogicEdit.SourceText
        End If
          'if not using syntax highlighting
          If Not Settings.HighlightLogic Then
            'force refresh
            rtfLogic.RefreshHighlight
          End If

        'clear undo buffer
        rtfLogic.EmptyUndo

        'set caption
        If Not InGame And LogicEdit.ID = "NewLogic" Then
          LogCount = LogCount + 1
          LogicEdit.ID = "NewLogic" & CStr(LogCount)
          rtfLogic.Dirty = True
        Else
          rtfLogic.Dirty = LogicEdit.IsDirty
        End If
        Caption = sLOGED & ResourceName(LogicEdit, InGame, True)

        'set dirty status
        If rtfLogic.Dirty Then
          MarkAsDirty
        Else
          frmMDIMain.mnuRSave.Enabled = False
          frmMDIMain.Toolbar1.Buttons("save").Enabled = False
        End If

        'empty undo buffer
        rtfLogic.EmptyUndo

        'maximize, if that's the current setting
        If Settings.MaximizeLogics Then
          WindowState = vbMaximized
        End If

        'return true
        EditLogic = True
      Exit Function

      ErrHandler:
        'opening existing?
        'opening new?
        'opening import?
        ErrMsgBox "Error while opening logic:", "Unable to open logic for editing", "Logic Editor Error"
        LogicEdit.Unload
        Set LogicEdit = Nothing
      */
    }

    void tmpLogEd()
    {
      /*
 Option Explicit
 
  Private FixingGlitch As Boolean
  Private mLoading As Boolean
  Private MouseDown As Boolean
  Private blnDblClick As Boolean
  
  ' tool tip variables
  Private TipCmdPos As Long, TipCurArg As Long
  Private TipCmdNum As Long
  
  'to manage the defines list feature
  Private DefEndPos As Long, DefTopLine As Long
  Private DefStartPos As Long
  Private PrevText As String
  Private DefText As String
  Private DefTip As String
  Private LastPos As Long
  
  Private SnipIndent As Long
  
  Private OldMouseX As Long, OldMouseY As Long
  Private pPos As POINTAPI
  Private LDefLookup() As TDefine
  Private DefDirty As Boolean
  'DefDirty means text has changed, so the lookup list needs
  'to be rebuilt;
  Public ListDirty As Boolean
  'ListDirty means the ShowDefinesList needs to be rebuilt
  Private ListType As EArgListType
  'tracks what is currently being included in the list
  '(public so we can get notified of global changes)
  
  Public FormMode As ELogicFormMode
  Public FileName As String
  
  Private CalcWidth As Long, CalcHeight As Long
  Private Const MIN_HEIGHT = 150
  Private Const MIN_WIDTH = 150
  
  ' need way to ignore control keys (with
  ' keycodes less than 31; the riched control
  ' works properly in test app, but for some
  ' reason it doesn't work when the control
  ' is compiled)
  Private NoChange As Boolean
  
  ' logic specific properties
  Public LogicEdit As AGILogic
  Public LogicNumber As Long
  Public InGame As Boolean



Private Function AddArgs(ByVal TextIn As String) As String

  Dim i As Long, strArg As String

  On Error GoTo ErrHandler
  
  'if there are arguments, get replacement values
  
  AddArgs = TextIn
  
  i = 1
  Do
    'if this arg value is present,
    If InStr(1, AddArgs, "%" & CStr(i)) > 0 Then
      strArg = InputBox("Enter text for argument #" & CStr(i) & ":", "Input Snippet Argument")
      AddArgs = Replace(AddArgs, "%" & CStr(i), strArg)
    Else
      'done;
      Exit Function
    End If
  Loop While True

Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub AddIfUnique(ByVal strName, ByVal lngIcon, ByVal strValue)

  On Error Resume Next
  
  Dim i As Long
  
  'check for existing item
  i = lstDefines.ListItems(strName).Index
  
  'if no error, means it's not unique
  If Err.Number = 0 Then
    Err.Clear
    Exit Sub
  End If
  Err.Clear
  
  'OK to add it
  lstDefines.ListItems.Add(, , strName, , lngIcon).Key = strName
  lstDefines.ListItems(strName).Tag = strValue
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

 Private Sub BuildDefineList(Optional ByVal ArgType As EArgListType = alAll)

  'adds defines to the listview, which is then presented to user
  'the list defaults to all defines; but when a specific argument
  'type is needed, it will adjust to show only the defines
  'of that particular type
  
  Dim tmpDefines() As TDefine, Max As Long, Min As Long
  Dim i As Long, j As Long, blnAdd As Boolean
  Dim strLine As String
  Dim tmpLines As StringList, tdNewDefine As TDefine
  Const DEFINE As String = "#define "
  Const SPACE_CHAR As String = " "
  
  Dim tmpLog As AGILogic
  Dim tmpPic As AGIPicture
  Dim tmpSnd As AGISound
  Dim tmpView As AGIView
  
  'if we don't need to rebuild, then don't!
  If Not ListDirty And Not DefDirty And (ArgType = ListType) Then
    'since the list is good, and the local defines list
    'is good, and the type matches, nothing to do here
    Exit Sub
  End If
    
  'this might take awhile
  WaitCursor
   
  'ignore all errors; just keep going
  On Error GoTo ErrHandler
  
  lstDefines.ListItems.Clear
  lstDefines.Tag = "defines"
  
  'locals not needed if looking for only a ResID
  If ArgType < alLogic Then
    'if locals need updating, rebuild the list
    If DefDirty Then
      BuildLDefLookup
    End If
  
    'add local defines (if duplicate defines, only first one will be used)
    Max = UBound(LDefLookup())
    'if only one element we need to verify it's a real define and not blank
    If Max = 0 Then
      If LDefLookup(0).Name = "" Then
        Max = -1
      End If
    End If
    For i = 0 To Max
      'add these local defines IF
      '   types match OR
      '   argtype is ALL OR
      '   argtype is (msg OR invobj) AND deftype is defined string OR
      '   argtype matches a special type
      blnAdd = False
      If LDefLookup(i).Type = ArgType Then
        blnAdd = True
      Else
        Select Case ArgType
        Case alAll
          blnAdd = True
        Case alIfArg   'variables and flags
          blnAdd = (LDefLookup(i).Type = atVar Or LDefLookup(i).Type = atFlag)
        Case alOthArg  'variables and strings
          blnAdd = (LDefLookup(i).Type = atVar Or LDefLookup(i).Type = atStr)
        Case alValues  'variables and numbers
          blnAdd = (LDefLookup(i).Type = atVar Or LDefLookup(i).Type = atNum)
        Case alMsg, alIObj
          blnAdd = LDefLookup(i).Type = atDefStr
        End Select
      End If
      
      If blnAdd Then
        'don't add if already defined
        AddIfUnique LDefLookup(i).Name, 1 + LDefLookup(i).Type, LDefLookup(i).Value
      End If
    Next i
  End If
  
  'global defines next (but only if not looking for just a ResID)
  If GameLoaded And ArgType < alLogic Then
    Max = UBound(GDefLookup())
    'if only one element we need to verify it's a real define and not blank
    If Max = 0 Then
      If GDefLookup(0).Name = "" Then
        Max = -1
      End If
    End If
    'add em
    For i = 0 To Max
      'add these global defines IF
      '   types match OR
      '   argtype is ALL OR
      '   argtype is (msg OR invobj) AND deftype is defined string
      '   argtype matches a special type
      blnAdd = False
      If GDefLookup(i).Type = ArgType Then
        blnAdd = True
      Else
        Select Case ArgType
        Case alAll
          blnAdd = True
        Case alIfArg   'variables and flags
          blnAdd = (GDefLookup(i).Type = atVar Or GDefLookup(i).Type = atFlag)
        Case alOthArg  'variables and strings
          blnAdd = (GDefLookup(i).Type = atVar Or GDefLookup(i).Type = atStr)
        Case alValues  'variables and numbers
          blnAdd = (GDefLookup(i).Type = atVar Or GDefLookup(i).Type = atNum)
        Case alMsg, alIObj
          blnAdd = GDefLookup(i).Type = atDefStr
        End Select
      End If
      
      If blnAdd Then
        'don't add if already defined
        AddIfUnique GDefLookup(i).Name, 12 + GDefLookup(i).Type, GDefLookup(i).Value
      End If
    Next i
  End If
  
  'check for logics, views, sounds, iobjs, voc words AND pics
  Select Case ArgType
  Case alAll, alByte, alValues, alLogic, alPicture, alSound, alView
    Select Case ArgType
    Case alAll, alByte, alValues
      Min = 0
      Max = 1023
    Case alLogic
      Min = 0
      Max = 255
    Case alPicture
      Min = 768
      Max = 1023
    Case alSound
      Min = 512
      Max = 767
    Case alView
      Min = 256
      Max = 511
    End Select
    
    For i = Min To Max
      'if this resource is ingame, Type=0
      'if NOT in game, Type=11
      If IDefLookup(i).Type = atNum Then
        'don't add if already defined
        AddIfUnique IDefLookup(i).Name, 23 + Int(i / 256), IDefLookup(i).Value
      End If
    Next i
    
  Case alIObj
    'add inv items (ok if matches an existing define)
    With InventoryObjects
      If Not .Loaded Then
        .Load
      End If
      
      'skip first obj, and any others that are just a question mark
      For i = 1 To .Count - 1
        strLine = .Item(i).ItemName
        If strLine <> "?" Then
          If InStr(strLine, QUOTECHAR) <> 0 Then
            strLine = Replace(strLine, QUOTECHAR, "\" & QUOTECHAR)
          End If
            lstDefines.ListItems.Add(, , Chr$(34) & strLine & Chr$(34), , 17).Tag = "i" & CStr(i)
        End If
      Next i
    End With
  
  Case alVocWrd
    'add vocab words items (ok if matches an existing define)
    With VocabularyWords
      If Not .Loaded Then
        .Load
      End If
      
      'skip group 0
      For i = 1 To .GroupCount - 1
        lstDefines.ListItems.Add(, , Chr$(34) & .Group(i).GroupName & Chr$(34), , 22).Tag = .Group(i).GroupNum
      Next i
    End With
  End Select
  
  'lastly, check for reserved defines option (if not looking for a resourceID)
  If LogicSourceSettings.UseReservedNames And ArgType < alLogic Then
    Max = 94
    If Max > 0 Then
      'add em
      For i = 0 To Max
        'add these global defines IF
        '   types match OR
        '   argtype is ALL OR
        '   argtype is (msg OR invobj) AND deftype is defined string
        '   argtype matches a special type
        blnAdd = False
        If RDefLookup(i).Type = ArgType Then
          blnAdd = True
        Else
          Select Case ArgType
          Case alAll
            blnAdd = True
          Case alIfArg   'variables and flags
            blnAdd = (RDefLookup(i).Type = atVar Or RDefLookup(i).Type = atFlag)
          Case alOthArg  'variables and strings
            blnAdd = (RDefLookup(i).Type = atVar Or RDefLookup(i).Type = atStr)
          Case alValues  'variables and numbers
            blnAdd = (RDefLookup(i).Type = atVar Or RDefLookup(i).Type = atNum)
          Case alMsg, alIObj
            blnAdd = RDefLookup(i).Type = atDefStr
          End Select
        End If
        
        If blnAdd Then
          'don't add if already defined
          AddIfUnique RDefLookup(i).Name, 27 + RDefLookup(i).Type, RDefLookup(i).Value
        End If
      Next i
    End If
  End If
  
  'list is clean
  ListDirty = False
  ListType = ArgType
  
  'restore cursor
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub BuildLDefLookup()

  'builds the local list of defines for use by
  'the tooltip and showlist functions
  
  Dim tmpLines As StringList, Max As Long
  Dim i As Long, NumDefs As Long
  Dim strLine As String, tmpDefine As TDefine
  Dim blnSub As Boolean
  
  Const SPACE_CHAR = " "
  Const DEFINE = "#define "
  
  'should only be called if needed (DefDirty=True)
  '*'Debug.Assert DefDirty
  
  'if cursor is already the wait cursor, we need to
  'NOT restore it after completion; calling function
  'will do that
  blnSub = (Screen.MousePointer = vbHourglass)

  'this might take awhile
  WaitCursor
   
  'ignore all errors; just keep going
  On Error GoTo ErrHandler
  
  
  'add local defines (if duplicate defines, only first one will be used)
  Set tmpLines = New StringList
  'OK to use raw text; don't need to worry about extended characters here
  tmpLines.Assign rtfLogic.Text
  
  'reset the lookup array
  ReDim LDefLookup(0)
  NumDefs = -1
  
  'step through all lines and find define values
  Max = tmpLines.Count
  For i = 0 To Max - 1
    'remove comments and trim the line
    strLine = StripComments(tmpLines(i), "")
    If LenB(strLine) > 0 Then
      Do
        'check for define statement
        If Left$(LCase$(strLine), 8) = DEFINE Then
          'strip off define keyword
          strLine = Trim$(Right$(strLine, Len(strLine) - 8))
        Else
          Exit Do
        End If
        
        'there has to be at least one space
        If InStr(1, strLine, SPACE_CHAR) <> 0 Then
          'split it by position of first space
          tmpDefine.Name = Trim$(Left$(strLine, InStr(1, strLine, SPACE_CHAR) - 1))
          tmpDefine.Value = Trim$(Right$(strLine, Len(strLine) - InStr(1, strLine, SPACE_CHAR)))
        Else
          'no good; get next line
          Exit Do
        End If
        
        'don't bother validating; just use it as long as the Type can be determined
        If LenB(tmpDefine.Value) = 0 Or LenB(tmpDefine.Name) = 0 Then
          Exit Do
        End If
        tmpDefine.Type = DefTypeFromValue(tmpDefine.Value)
        'increment counter
        NumDefs = NumDefs + 1
        ReDim Preserve LDefLookup(NumDefs)
        LDefLookup(NumDefs) = tmpDefine
      Loop Until True
    End If
    'get next line
  Next i
  
  Set tmpLines = Nothing
  DefDirty = False
  
  If Not blnSub Then
    'restore cursor unless called from ShowDefineList
    Screen.MousePointer = vbDefault
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickECustom3()

  ' open an ingame resource for editing, or
  ' list synonyms for a 'said' word
  
  Dim i As Long, strID As String
  
  On Error GoTo ErrHandler
  
  'if cursor token is an editable game resource
  If Asc(frmMDIMain.mnuECustom3.Caption) = 79 Then
    ' get the token (resourceID or word)
    strID = TokenFromCursor(rtfLogic)
    
    ' open it for editing
  
    ' find the name in the list and open it
    For i = 0 To 1023
      If strID = IDefLookup(i).Name Then
        Select Case i \ 256
        Case 0 'logic
          OpenLogic i Mod 256
        Case 1 'view
          OpenView i Mod 256
        Case 2 'sound
          OpenSound i Mod 256
        Case 3 'picture
          OpenPicture i Mod 256
        End Select
        Exit Sub
      End If
    Next i
  End If
  
  'if cursor token is over a 'said' word
  If Asc(frmMDIMain.mnuECustom3.Caption) = 86 Then
    ' get the token (resourceID or word)
    strID = TokenFromCursor(rtfLogic, True, True)
    
    'strip off quotes
    strID = Mid(strID, 2, Len(strID) - 2)
    
    'show the list
    ShowSynonymList strID
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub ConvertPos(ByRef posX As Long, ByRef posY As Long)

  'converts screen coordinates from the GetPoint function into
  'relative values based on the on the rtfLogic window
  
  'need to account for borders, toolbars, etc
  
  Dim lngLogLeftBorder As Long, lngLogTopBorder As Long
  
  On Error GoTo ErrHandler
  
  'left border is easy; difference between width and scalewidth
  lngLogLeftBorder = (Me.Width / ScreenTWIPSX - Me.ScaleWidth) / 2
  'top is a bit trickier- need to account for toolbar, and rtf window appears to have a 3 pixel offset
  lngLogTopBorder = Me.Height / ScreenTWIPSY - Me.ScaleHeight - Toolbar1.Height - lngLogLeftBorder + 3
  
  posX = posX - (Me.Left + frmMDIMain.Left + frmMDIMain.picResources.Width) / ScreenTWIPSX - (rtfLogic.Width - rtfLogic.ScaleWidth / ScreenTWIPSX) - lngLogLeftBorder
  posY = posY - (Me.Top + frmMDIMain.Top) / ScreenTWIPSY - lngMainTopBorder - lngLogTopBorder
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function GetCursorToken(ByRef lngPos As Long) As String

  'using current cursor pos, return the token under cursor
  'expanding both forwards and backwards
  
  'lngPos is also updated to start position of the token
  
  On Error GoTo ErrHandler
  
  Dim lngLineStart As Long
  Dim i As Long, j As Long
  Dim rtn As Long, strLine As String
  
  'separators are any character EXCEPT:
  ' #, $, %, ., 0-9, @, A-Z, _, a-z
  '(codes 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122)
  
  'only need to search on current line, so
  'extract it to simplify the text searching
  
  'if starting character is vbCr, then exit; cursor is at or past end of a line
  If Asc(rtfLogic.Range(lngPos, lngPos + 1).Text) = 13 Then
    Exit Function
  End If
  
  rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, lngPos, 0)
  'get the startpos of this line
  lngLineStart = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
  'get current row
  strLine = StripComments(rtfLogic.Range(lngLineStart, lngLineStart).Expand(reLine).Text, "", True)
  
  If Len(strLine) = 0 Then
    Exit Function
  End If
  
  'move backward until separator found
  'i is relative position of starting point in current line;
  'start with i pointing to previous char, then enter do loop
  i = lngPos - lngLineStart + 1
  If i > Len(strLine) Then
    Exit Function
  End If
  Do While i >= 1
    Select Case Asc(Mid$(strLine, i))
    Case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122
      'ok
    Case Else
      'no good
      Exit Do
    End Select
    i = i - 1
  Loop
  
  'move endpos forward until separator found
  j = lngPos - lngLineStart + 1
  Do While j <= Len(strLine)
    Select Case Asc(Mid$(strLine, j))
    Case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122
      'ok
    Case Else
      Exit Do
    End Select
    j = j + 1
  Loop
  
  If i = j Then Exit Function
  'return the token
  GetCursorToken = Mid(strLine, i + 1, j - i - 1)
  lngPos = lngLineStart + i
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  lngPos = -1
End Function

Private Function GetArgType(ByVal blnInQuote As Boolean) As EArgListType

  On Error GoTo ErrHandler
  
  'check if within a command
  Dim strCmd As String, strText As String
  Dim lngPos As Long
  
  'check for a command infront of this spot
  'OK to use raw text; don't need to worry about extended characters here
  lngPos = rtfLogic.Selection.Range.StartPos
  strText = rtfLogic.Text
  
  'get previous command from current pos
  strCmd = FindPrevCmd(strText, lngPos, TipCurArg, blnInQuote)

  'adjust cmdpos
  TipCmdPos = lngPos
  
  'is this a valid command?
  If strCmd = "(" Then
    'skip past them
    Do
      strCmd = FindPrevToken(strText, lngPos, blnInQuote)
    Loop Until strCmd <> "("
  End If
  
  'if still nothing,
  If Len(strCmd) = 0 Then
    'we asssume this is a variable or string assignment;
    '*'Debug.Print "unknown arg type"
    GetArgType = alOthArg
    Exit Function
  End If
  
  'check for non command text
  Select Case strCmd
  Case vbCr, vbCrLf, vbLf
    GetArgType = alOthArg
    Exit Function
    
  Case "==", "!=", ">", ">", "<=", ">=", "=<", "=>", "||", "&&"
    'var or number
    GetArgType = alValues
    Exit Function
  Case "++", "--"
    GetArgType = alVar
    Exit Function
  Case "="
    'var, number, string
    GetArgType = alValues
    Exit Function
  Case "if"
    'var, flag
    GetArgType = alIfArg
    Exit Function
  End Select
  
  'now check this command against list
  TipCmdNum = LogicCmd(strCmd)
  
  If TipCmdNum = -1 Then
    GetArgType = alAll
    Exit Function
  End If
  
  'check for 'said' command
  If TipCmdNum = 116 Then
    GetArgType = alVocWrd
  Else
    
    'use ascii Value of argument syntax to determine
    'the argtype
    Dim strArgs() As String
    strArgs = Split(LoadResString(5000 + TipCmdNum), ", ")
    If TipCurArg <= UBound(strArgs) Then
      Select Case Asc(strArgs(TipCurArg))
      Case 98 'byte or number
        GetArgType = alByte
        
        'check for special cases where resourceIDs are also valid
        Select Case TipCmdNum
        Case 0  'add.to.pic(view, byt, byt, byt, byt, byt, byt)
          If TipCurArg = 0 Then
            GetArgType = alView
          End If
        Case 9  'call(logic)
          GetArgType = alLogic
        Case 24 'discard.sound(sound)
          GetArgType = alSound
        Case 25 'discard.view(view)
          GetArgType = alView
        Case 66 'load.logics(logic)
          GetArgType = alLogic
        Case 69 'load.sound(sound)
          GetArgType = alSound
        Case 70 'load.view(view)
          GetArgType = alView
        Case 78 'new.room(logic)
          GetArgType = alLogic
        Case 138 'set.view(obj,view)
          GetArgType = alView
        Case 141 'show.obj(view)
          GetArgType = alView
        Case 143 'sound(sound,flg)
          GetArgType = alSound
        Case 156 'trace.info(logic,byt,byt)
          If TipCurArg = 0 Then
            GetArgType = alLogic
          End If
        End Select
        
      Case 118 'var
        GetArgType = alVar
      Case 102 'flag
        GetArgType = alFlag
      Case 109 'msg
        GetArgType = alMsg
      Case 111 'screen obj
        GetArgType = alSObj
      Case 105 'inv obj
        GetArgType = alIObj
      Case 115 'string
        GetArgType = alStr
      Case 119 'word
        GetArgType = alWord
      Case 99 'controller
        GetArgType = alCtl
      Case Else
        'should never get here
        '*'Debug.Assert False
        GetArgType = alAll
      End Select
    Else
      GetArgType = alAll
    End If
  End If
    
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Public Sub MenuClickClear()
  '
  'display code snippet creation form
  ' using selected text as starting point
  
  'if inserting a snippet
  If Asc(frmMDIMain.mnuEClear.Caption) = 73 Then
    ShowSnippetList
  Else
    'create a snippet with selected text
    SnipMode = 0
    frmSnippets.Show vbModal, frmMDIMain
    'force focus back to editor
    rtfLogic.SetFocus
  End If
End Sub

Public Sub MenuClickCustom3()

  'change IsRoom status
  
  On Error GoTo ErrHandler
  
  '*'Debug.Assert InGame = True
  
  ' toggle the property for this room
  LogicEdit.IsRoom = Not LogicEdit.IsRoom
  frmMDIMain.mnuRCustom3.Checked = LogicEdit.IsRoom
  
  ' toggle property for the ingame logic
  Logics(LogicNumber).IsRoom = LogicEdit.IsRoom
  
  If UseLE Then
    If UseLE And Logics(LogicNumber).IsRoom Then
      'update layout editor and layout data file to show this room is in the game
      UpdateExitInfo euShowRoom, LogicNumber, Logics(LogicNumber)
    Else
      'update layout editor and layout data file to show this room is now gone
      UpdateExitInfo euRemoveRoom, LogicNumber, Nothing
    End If
  End If
  
  ' if this logic is selected
  If SelResNum = LogicNumber And SelResType = rtLogic Then
    ' update treelist property window
    frmMDIMain.PaintPropertyWindow
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickHelp()

  Dim blnCmdHelp As Boolean, strHelp As String
  Dim lngLine As Long, lngLineStart As Long
  Dim i As Long, lngPos As Long
  Dim tmpRange As RichEditAGI.Range
  
  On Error GoTo ErrHandler
  
  'if a tip is currently visible,
  If picTip.Visible Then
    'show the help for the current cmd
    blnCmdHelp = True
    strHelp = "htm\commands\cmd_" & Replace(LoadResString(ALPHACMDTEXT + TipCmdNum), ".", vbNullString) & ".htm"
  End If
  
  'always hide defines window
  picDefine.Visible = False
  
  'if not helping a cmd with tip showing
  If Not blnCmdHelp Then
  
  
    'if there is a selection:
    If rtfLogic.Selection.Range.Length <> 0 Then
      'check if selection extends across lines,
      'if not, assume selected text is help string
      rtfLogic.GetCharPos rtfLogic.Selection.Range.StartPos, lngPos
      rtfLogic.GetCharPos rtfLogic.Selection.Range.EndPos, i
      
      If i = lngPos Then
        'use the selection
        strHelp = Trim$(rtfLogic.Selection.Range.Text)
      End If
    Else
    
    strHelp = TokenFromCursor(rtfLogic)
    End If
    
    'validate if on a cmd
    For i = 1 To 18
      If StrComp(TestCommands(i).Name, strHelp, vbTextCompare) = 0 Then
        'found it
        blnCmdHelp = True
        Exit For
      End If
    Next i
    'if not found
    If Not blnCmdHelp Then
      lngPos = Commands.Count
      For i = 0 To lngPos
        If StrComp(Commands(i).Name, strHelp, vbTextCompare) = 0 Then
          'found it
          blnCmdHelp = True
          Exit For
        End If
      Next i
    End If
    
    'if found,
    If blnCmdHelp Then
      'build topic string
      strHelp = "htm\commands\cmd_" & Replace(LCase$(strHelp), ".", vbNullString) & ".htm"
    Else
      'if on 'if'
      If strHelp = "if" Then
        strHelp = "htm\commands\syntax.htm#ifelse"
      Else
        'if not, set strHelp to logic editor
        strHelp = "htm\winagi\Logic_Editor.htm"
      End If
    End If
  End If
  
  'show help file
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, strHelp
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub Activate()
  'bridge method to call the form's Activate event method
  Form_Activate
End Sub

Public Sub BeginFind()

  'each form has slightly different search parameters
  'and procedure; so each form will get what it needs
  'from the form, and update the global search parameters
  'as needed
  '
  'that's why each search form checks for changes, and
  'sets the global values, instead of doing it once inside
  'the FindForm code
  On Error GoTo ErrHandler
  
  'always reset the synonym search
  GFindSynonym = False
  
  'ensure this form is the search form
  '*'Debug.Assert SearchForm Is Me
  
  Select Case FindForm.FormAction
  Case faFind
    FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc
  
  Case faReplace
    FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc, True, GReplaceText
     
  Case faReplaceAll
    ReplaceAll GFindText, GReplaceText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub InitFonts()

  On Error GoTo ErrHandler
  
  With rtfLogic
    'set syntax highlighting property first
    .HighlightSyntax = Settings.HighlightLogic
  
    'then set all highlight properties
    Me.Font.Name = Settings.EFontName
    Me.Font.Size = Settings.EFontSize
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
    
  'set up defines listbox
  lstDefines.Font.Name = Settings.EFontName
  lstDefines.Font.Size = Settings.EFontSize
  lstDefines.Height = 6 * Me.TextHeight("Ay")

  'refresh
  rtfLogic.RefreshHighlight
      
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickDelete()

  'send delete key
  'Delete Key 'down'
  SendMessage Me.rtfLogic.hWnd, WM_KEYDOWN, &H2E, &H1
  'Delete Key 'up'
  SendMessage Me.rtfLogic.hWnd, WM_KEYUP, &H2E, &H1
End Sub

Public Sub MenuClickDescription(ByVal FirstProp As Long)
  
  Dim strID As String, strDescription As String
  
  On Error GoTo ErrHandler
  
  If FirstProp <> 1 And FirstProp <> 2 Then
    FirstProp = 1
  End If
    
  strID = LogicEdit.ID
  strDescription = LogicEdit.Description
  
  'use the id/description change method
  If GetNewResID(rtLogic, LogicNumber, strID, strDescription, InGame, FirstProp) Then
    'save changes to logicedit
    UpdateID strID, strDescription
  End If
  
  'if layout editor is in use, update it too
  
  'force menu update
  AdjustMenus rtLogic, InGame, True, rtfLogic.Dirty
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindLogic)
  
  Dim strToken As String
  
  On Error GoTo ErrHandler
  
  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  ' if find form is not showing, need to populate
  ' it with current search parameters
  With FindForm
    ' find text is the exception; it doesn't use the
    ' current global parameter unless there is nothing
    ' selected in the current logic (or nothing under
    ' the cursor)
    
    'if something selected
    If Len(rtfLogic.Selection.Range.Text) > 0 Then
      'use it
      GFindText = rtfLogic.Selection.Range.Text
    Else
      'if nothing selected, check for token under cursor
      strToken = TokenFromCursor(rtfLogic, False)
      If Len(strToken) > 0 Then
        GFindText = strToken
      End If
    End If
    
    'set find dialog to find textinlogic mode
    .SetForm ffValue, InGame
    
    'show the form
    .Show , frmMDIMain
  
    Select Case ffValue
    Case ffReplaceWord, ffReplaceObject, ffReplaceLogic, ffReplaceText
      'highlight the replacement text
      .txtReplace.SelStart = 0
      .txtReplace.SelLength = Len(.txtReplace.Text)
      .txtReplace.SetFocus
    Case Else
      'always highlight search text
      .txtFindText.SelStart = 0
      .txtFindText.SelLength = Len(.txtFindText.Text)
      .txtFindText.SetFocus
    End Select
    
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
  
  'always reset findsynonym
  GFindSynonym = False
  
  'if a previous find text exists
  If LenB(GFindText) <> 0 Then
    'ensure this form is the search form
    Set SearchForm = Me
    FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc
  Else
    'nothing to find yet? show find form
    MenuClickFind
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub MenuClickECustom1()
  
  'add block comment marks
  
  On Error GoTo ErrHandler
  
  rtfLogic.Selection.Range.Comment
Exit Sub

ErrHandler:
  Err.Clear
End Sub

Public Sub MenuClickInsert()
  
  WaitCursor
  ShowDefineList
  
  'restore cursor
  Screen.MousePointer = vbDefault

End Sub


Public Sub MenuClickImport()
  
  Dim tmpLogic As AGILogic
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'this method is only called by the Main form's Import function
  'the MainDialog object will contain the name of the file
  'being imported.
  
  'steps to import are:
  'import the Logic to tmp object
  'clear the existing logicedit,
  'copy tmp object to this item
  'and reset it
  
  Set tmpLogic = New AGILogic
  On Error Resume Next
  
  'assume text file
  tmpLogic.Import MainDialog.FileName, True
  'if error
  If Err.Number <> 0 Then
    'try again, assuming a logic resource file
    tmpLogic.Import MainDialog.FileName, False
    'if STILL an error,
    If Err.Number <> 0 Then
      ErrMsgBox "An error occurred while importing this Logic:", "", "Import Logic Error"
      Exit Sub
    End If
  End If
  
  'clear the Edit Logic
  LogicEdit.Clear
  
  'assign source code to editor
  rtfLogic.Text = tmpLogic.SourceText
  
  'discard the temp logic
  tmpLogic.Unload
  Set tmpLogic = Nothing
  
  'clear the undo buffer
  rtfLogic.EmptyUndo
  
  'mark as dirty
  MarkAsDirty
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickRedo()

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  rtfLogic.Redo
  
  UpdateStatusBar
End Sub

Public Sub MenuClickReplace()

  'replace text
  'use menuclickfind in replace mode
  MenuClickFind ffReplaceLogic
End Sub
Public Sub MenuClickSave()
  'saves source code;
  'use compile (MenuClickCustom1) to save logic resource
  
  On Error GoTo ErrHandler
  
  Dim i As Integer
  Dim tmpExits As AGIExits
  Dim lngLine As Long, lngPos As Long
  Dim lngFirst As Long
  
  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  'show wait cursor since it might take awhile
  WaitCursor
  
  'if in a game
  If InGame Then
    '   - copy editor text to GAME LOGIC source
    '   if a room and using layout editor
    '      - update source
    '      - copy updated source back to editor
    '   -save source
    
    'unlike other resources, the ingame logic is referenced directly
    'when being edited; so, it's possible that the logic might get closed
    'such as when changing which logic is being previewed;
    'SO, we need to make sure the logic is loaded BEFORE saving
    If Not Logics(LogicNumber).Loaded Then
      'reload it!
      Logics(LogicNumber).Load
    End If
    
    'now, assign the updated source text
    Logics(LogicNumber).SourceText = rtfLogic.Text
    
    'if using layout editor AND is a room,
    If UseLE And LogicEdit.IsRoom Then
      'need to update the editor and the data file with new exit info
      
      'save current cursor position
      lngPos = rtfLogic.Selection.Range.StartPos
      'cache the current first visible line
      lngFirst = SendMessage(rtfLogic.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
      'this returns zero sometimes when it shouldn't

      'UGH! does UpdateExitInfo change the SOURCE, or the RTF????
      '  VERIFIED: it updates the SOURCE, which we know is current now
      UpdateExitInfo euUpdateRoom, LogicNumber, Logics(LogicNumber)
      
      'if preview window got updated due to a change?
      '*'Debug.Assert Logics(LogicNumber).Loaded
      
      'copy update back to RTF
      rtfLogic.Text = Logics(LogicNumber).SourceText
      
      'reset cursor first; it will scroll the screen if
      ' cursor is currently offscreen
      rtfLogic.Selection.Range.StartPos = lngPos
      rtfLogic.Selection.Range.EndPos = lngPos
      
      ' make sure scrolling is the same
      lngLine = SendMessage(rtfLogic.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
      If lngLine <> lngFirst Then
        SendMessage rtfLogic.hWnd, EM_LINESCROLL, 0, lngFirst - lngLine
      End If
    End If
    
    'save the SOURCE
    Logics(LogicNumber).SaveSource
      
    'copy it back
    LogicEdit.SetLogic Logics(LogicNumber)
    
    'and unload the game logic
    Logics(LogicNumber).Unload
    
    'update selection preview
    If Not Compiling Then
      UpdateSelection rtLogic, LogicNumber, umPreview Or umProperty
    End If
    
  'if not in a game, but has a sourcefile
  ElseIf LenB(LogicEdit.SourceFile) <> 0 Then
    rtfLogic.SaveFile LogicEdit.SourceFile, reOpenSaveText + reOpenSaveCreateAlways, 437
  Else
    'restore cursor while getting a name
    Screen.MousePointer = vbDefault
    
    'use ExportLogicSource
    LogicEdit.SourceFile = NewSourceName(LogicEdit, InGame)
    If Len(LogicEdit.SourceFile) > 0 Then
      ' go back to wait cursor
      WaitCursor
      rtfLogic.SaveFile LogicEdit.SourceFile, reOpenSaveText + reOpenSaveCreateAlways, 437
    Else
      'user canceled save
      Exit Sub
    End If
  End If
  
  UpdateStatusBar
  
  'reset flag
  rtfLogic.Dirty = False
  'reset caption
  Caption = sLOGED & ResourceName(LogicEdit, InGame, True)
  
  'disable menu and toolbar button
  frmMDIMain.mnuRSave.Enabled = False
  frmMDIMain.Toolbar1.Buttons("save").Enabled = False
  
  'restore cursor
  Screen.MousePointer = vbDefault
Exit Sub
  
ErrHandler:
  ErrMsgBox "Error during save: ", "", "Logic Save Error"
  Resume Next
End Sub

Public Sub MenuClickPrint()

  Dim i As Long
  
  For i = 1 To LogicEditors.Count
    If LogicEditors(i) Is Me Then
      Exit For
    End If
  Next i
  
  'show logic printing form
  Load frmPrint
  frmPrint.SetMode rtLogic, LogicEdit, i, InGame
  frmPrint.Show vbModal, frmMDIMain
End Sub

Public Sub MenuClickExport()
  'export logic or source
  
  'logics that are NOT in a game can't export the actual logic
  'resource; they only export the source code (which is functionally
  'equivalent to 'save as'
  
  Dim rtn As Long
  Dim strExportName As String
  
  'update sourcecode with current logic text
  LogicEdit.SourceText = rtfLogic.Text
  
  
  'if in a game
  If InGame Then
    If MsgBox("Do you want to export the source code for this logic?", vbQuestion + vbYesNo, "Export Logic") = vbYes Then
      'MUST make sure logic sourcefile is set to correct Value
      'BEFORE calling exporting; this is because LogicEdit is NOT
      'in a game; it only mimics the ingame resource
      LogicEdit.SourceFile = ResDir & LogicEdit.ID & LogicSourceSettings.SourceExt
      
      'get a filename for the export
      strExportName = NewSourceName(LogicEdit, InGame)
      'if a filename WAS chosen then we can continue
      If strExportName <> "" Then
        'since LogicEdit is not really in a game
        rtfLogic.SaveFile strExportName, reOpenSaveText + reOpenSaveCreateAlways, 437
        UpdateStatusBar
      End If
    End If
      
    If MsgBox("Do you want to export the compiled logic resource?", vbQuestion + vbYesNo, "Export Logic") = vbYes Then
      'if compiled CRCs don't match
      If (Logics(LogicNumber).CompiledCRC <> LogicEdit.CRC) Then
      
        rtn = MsgBox("Source code has changed. Do you want to compile before exporting this logic?", vbQuestion + vbYesNo, "Export Logic")
  
        If rtn = vbYes Then
          'compile logic
          If Not CompileLogic(Me, LogicNumber) Then
            MsgBox "Compile error; Unable to export the logic resource.", vbCritical + vbOKOnly, "Compiler error"
            Exit Sub
          End If
        End If
      End If
      
      'now export it
      If ExportLogic(LogicEdit.Number) Then
        'everything is ok-
        'nothing else to do
      End If
    End If
  Else
    'not in game; saveas is only operation allowed

    'get a filename for the export
    strExportName = NewSourceName(LogicEdit, InGame)
    'if a filename was chosen, we can continue
    If strExportName <> "" Then
      'save it
      LogicEdit.SourceFile = strExportName
      rtfLogic.SaveFile LogicEdit.SourceFile, reOpenSaveText + reOpenSaveCreateAlways, 437
      
      'reset dirty flag and caption
      rtfLogic.Dirty = False
      LogicEdit.ID = FileNameNoExt(strExportName)
      Caption = sLOGED & LogicEdit.ID
      'disable save menu/button
      frmMDIMain.mnuRSave.Enabled = False
      frmMDIMain.Toolbar1.Buttons("save").Enabled = False
    End If
  End If
End Sub
Public Sub MenuClickInGame()
  'toggles the game state of an object
  
  Dim rtn As VbMsgBoxResult
  Dim strExportName As String
  Dim blnDontAsk As Boolean
  
  If InGame Then
    'ask if resource should be exported
    If Settings.AskExport Then
      rtn = MsgBoxEx("Do you want to export '" & LogicEdit.ID & "' source before" & vbNewLine & "removing it from your game?", _
                          vbQuestion + vbYesNoCancel, "Export Logic Before Removal", , , _
                          "Don't ask this question again", blnDontAsk)
      'save the setting
      Settings.AskExport = Not blnDontAsk
      'if now hiding update settings file
      If Not Settings.AskExport Then
        WriteSetting GameSettings, sGENERAL, "AskExport", Settings.AskExport
      End If
    Else
      'dont ask; assume no
      rtn = vbNo
    End If
    
    'if canceled,
    Select Case rtn
    Case vbCancel
      Exit Sub
    Case vbYes
      'export source
      
      'MUST make sure logic sourcefile is set to correct Value
      'BEFORE calling exporting; this is because LogicEdit is NOT
      'in a game; it only mimics the ingame resource
      LogicEdit.SourceFile = ResDir & LogicEdit.ID & LogicSourceSettings.SourceExt
      
      'get a filename for the export
      strExportName = NewSourceName(LogicEdit, InGame)
      'if a filename WAS chosen then we can continue
      If strExportName <> "" Then
        'since LogicEdit is not really in a game
        rtfLogic.SaveFile strExportName, reOpenSaveText + reOpenSaveCreateAlways, 437
        UpdateStatusBar
      End If
    Case vbNo
      'nothing to do
    End Select
    
    'confirm removal
    If Settings.AskRemove Then
      rtn = MsgBoxEx("Removing '" & LogicEdit.ID & "' from your game." & vbCrLf & vbCrLf & "Select OK to proceed, or Cancel to keep it in game.", _
                      vbQuestion + vbOKCancel, "Remove Logic From Game", , , _
                      "Don't ask this question again", blnDontAsk)
    
      'save the setting
      Settings.AskRemove = Not blnDontAsk
      'if now hiding, update settings file
      If Not Settings.AskRemove Then
        WriteSetting GameSettings, sGENERAL, "AskRemove", Settings.AskRemove
      End If
    Else
      'assume OK
      rtn = vbOK
    End If
    
    'if canceled,
    If rtn = vbCancel Then
      Exit Sub
    End If
    
    'remove the logic
    RemoveLogic LogicNumber
    
    'unload this form
    Unload Me
  Else
    'add to game
    
    'verify a game is loaded,
    If Not GameLoaded Then
      Exit Sub
    End If
    
    'show add resource form
    With frmGetResourceNum
      .ResType = rtLogic
      .WindowFunction = grAddInGame
      'setup before loading so ghosts don't show up
      .FormSetup
      .Show vbModal, frmMDIMain
    
      'if user makes a choice
      If Not .Canceled Then
        'store number
        LogicNumber = .NewResNum
        'change id before adding to game
        LogicEdit.ID = .txtID.Text
        'copy text back into sourcecode
        LogicEdit.SourceText = rtfLogic.Text
        
        'always import logics as non-room;
        'user can always change it later via the
        'InRoom property
        'add Logic (which saves the source file)
        AddNewLogic LogicNumber, LogicEdit, False, True
        
        'copy the Logic back (to ensure internal variables are copied)
        LogicEdit.SetLogic Logics(LogicNumber)
        
        'now we can unload the newly added logic;
        Logics(LogicNumber).Unload
        
        'update caption
        Caption = sLOGED & ResourceName(LogicEdit, True, True)
        'reset dirty flag
        rtfLogic.Dirty = False
        
        'set ingame flag
        InGame = True
        
        'change menu caption
        frmMDIMain.mnuRInGame.Caption = "Remove from Game"
        frmMDIMain.Toolbar1.Buttons("remove").Image = 10
        frmMDIMain.Toolbar1.Buttons("remove").ToolTipText = "Remove from Game"
      End If
    End With
    
    Unload frmGetResourceNum
  End If
End Sub

Public Sub MenuClickRenumber()
  'renumbers a resource
  
  Dim NewResNum As Byte, OldResNum As Byte
  Dim strOldID As String
  
  On Error GoTo ErrHandler
  
  'if not in a game
  If Not InGame Then
    Exit Sub
  End If
  
  'save old number to support layout update
  OldResNum = LogicNumber
  
  'get new number
  NewResNum = RenumberResource(LogicNumber, rtLogic)
  
  'if changed,
  If NewResNum <> LogicNumber Then
    'copy renumbered logic into LogicEdit object
    LogicEdit.SetLogic Logics(NewResNum)
    
    'update number
    LogicNumber = NewResNum
    
    'update caption
    Caption = sLOGED & ResourceName(LogicEdit, InGame, True)
    If rtfLogic.Dirty Then
      Caption = sDM & Caption
    End If
    
    'if ID changed because of renumbering
    If LogicEdit.ID <> strOldID Then
      'if old default file exists
      If FileExists(ResDir & strOldID & LogicSourceSettings.SourceExt) Then
        'rename it
        Name ResDir & strOldID & LogicSourceSettings.SourceExt As ResDir & Logics(NewResNum).ID & LogicSourceSettings.SourceExt
      End If
    End If
    
    'if using layout editor
    If UseLE Then
      'update layout
      UpdateExitInfo euRenumberRoom, OldResNum, Nothing, NewResNum
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickCustom1()
  
  'compile
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  If Not InGame Then
    'can't compile
    MsgBox "Only Logics in games can be compiled.", vbInformation, "Compile Logic"
    Exit Sub
  End If
  
  'just it case it takes awhile, use the wait cursor
  WaitCursor
   
  'make sure logic is loaded
  '*'Debug.Assert Not Logics(LogicNumber).Loaded
  If Not Logics(LogicNumber).Loaded Then
    Logics(LogicNumber).Load
  End If
  
  'compile logic
  If CompileLogic(Me, LogicNumber) Then
    'nothing to do different whether or not compile is successful
  End If
  
  'always update preview and properties
  UpdateSelection rtLogic, LogicNumber, umPreview Or umProperty
  
  Logics(LogicNumber).Unload

  'restore cursor
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickCustom2()

  'message cleanup tool
  
  Dim i As Long, j As Long, rtn As VbMsgBoxResult
  Dim strLogic As String
  Dim strMsg As String, lngMsgPos As Long, lngLineEnd As Long
  Dim MsgUsed(255) As Long, blnKeepUnused As Boolean
  Dim Messages(255) As String, intMsgCount As Integer
  Dim NewMsgs As Collection, blnRepeatChoice As Boolean
  Dim strStrings() As String, lngCount As Long
  
  'MsgsUsed() array is used to see if a message is declared (bit0 set),
  'and/or in use by number in the logic (bit1 set)
  'and/or in use by string reference (bit2 set)
  'so a value of 0 means neither declared nor in use;
  '              1 means declared, not in use;
  '              2 means not declared, but in use by number
  '              3 means declared, and in use by number
  '              4 means not declared, not in use by number, but used by
  '                  string ref
  '              ...   etc
  
  'strStrings() array used to hold copy of all string defines in order
  'to search for 's##="msg text";' syntax
  
  On Error GoTo ErrHandler

  'if user hasn't decided what to do about unused msgs
  If Settings.WarnMsgs = 0 Then
    'determine if user wants to keep unused messages that are currently
    'the message section
    rtn = MsgBoxEx("If messages in the message section are not not used anywhere" & vbNewLine & _
                   "in the logic text, do you still want to keep them?", vbQuestion + vbYesNoCancel + vbMsgBoxHelpButton, "Message Cleanup Tool", _
                   WinAGIHelp, "htm\winagi\Logic_Editor.htm#msgcleanup", "Always take this action when updating messages.", blnRepeatChoice)
    
    'if canceled
    If rtn = vbCancel Then
      Exit Sub
    End If
    
    'rtn Value indicates choice
    blnKeepUnused = (rtn = vbYes)
  Else
    'simulate msgbox response
    blnKeepUnused = (Settings.WarnMsgs = 1)
  End If
  
  'if no more warnings,
  If blnRepeatChoice Then
    If blnKeepUnused Then
      Settings.WarnMsgs = 1
    Else
      Settings.WarnMsgs = 2
    End If
    'now save this setting
    WriteSetting GameSettings, sLOGICS, "WarnMsgs", Settings.WarnMsgs
  End If
  
  'save logic text as a string
  strLogic = rtfLogic.Text
  
  'we need a list of all string defines, in order to check for
  'string assignments that use special syntax
  If DefDirty Then
    BuildLDefLookup
  End If
  'check locals, globals, and reserved for string defines
  ReDim strStrings(0)
  lngCount = 0
  For i = 0 To UBound(LDefLookup())
    If LDefLookup(i).Type = atStr Then
      ReDim Preserve strStrings(lngCount)
      strStrings(lngCount) = LDefLookup(i).Name
      lngCount = lngCount + 1
    End If
  Next i
  For i = 0 To UBound(GDefLookup())
    If GDefLookup(i).Type = atStr Then
      ReDim Preserve strStrings(lngCount)
      strStrings(lngCount) = GDefLookup(i).Name
      lngCount = lngCount + 1
    End If
  Next i
  'add the only resdef that's a string
  ReDim Preserve strStrings(lngCount)
  strStrings(lngCount) = RDefLookup(94).Name
  
  'next, get all messages that are predefined
  If Not ReadMsgs(strLogic, Messages(), MsgUsed(), LDefLookup()) Then
    'a problem was found that needs to be fixed before
    'messages can be cleaned up
    
    'get line from pos
    rtfLogic.GetCharPos CLng(Messages(1)), i
    strMsg = "Syntax error in line " & CStr(i) & " must be corrected" & vbNewLine & _
             "before message cleanup can continue:" & vbNewLine & vbNewLine
                  
    Select Case Messages(0)
    Case "1" 'invalid msg number
      MsgBoxEx strMsg & "Invalid message index number", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#messages"

    Case "2" 'duplicate msg number
      MsgBoxEx "Message index " & Messages(2) & " on line " & CStr(i) & _
             " is already in use.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#messages"
      
    Case "3" 'msg val should be a string
      MsgBoxEx strMsg & "Expected string Value", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#messages"
    
    Case "4" 'stuff not allowed on line after msg declaration
      MsgBoxEx strMsg & "Expected end of line", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#messages"
                  
    Case "5" 'missing end quote
      MsgBoxEx strMsg & "String is missing end quote mark", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#messages"
    End Select
    
    'set cursor to beginning of this line
    rtfLogic.Selection.Range.StartPos = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, i - 1, 0)
    rtfLogic.Selection.Range.EndPos = rtfLogic.Selection.Range.StartPos + SendMessage(rtfLogic.hWnd, EM_LINELENGTH, CLng(Messages(1)), 0)
    rtfLogic.SetFocus
    Exit Sub
  End If
  
  'if we are keeping all currently declared msgs,
  'determine starting count
  If blnKeepUnused Then
    For i = 1 To 255
      If MsgUsed(i) = 1 Then
        intMsgCount = intMsgCount + 1
      End If
    Next i
  End If
  
  'array to hold new msgs
  Set NewMsgs = New Collection
  
  'now, go through the logic text
  lngMsgPos = 0
  Do
  '*'Debug.Assert lngMsgPos >= 0
    strMsg = NextMsg(strLogic, lngMsgPos, LDefLookup(), strStrings())
    'if end reached exit the loop
    If lngMsgPos = 0 Then
      Exit Do
    End If
    'if a msg marker or an error encountered (due to improperly formatted string)
    If lngMsgPos < 0 Then
      If Val(strMsg) = 0 Then
        'it's a msg marker; let's see it it's valid
        j = Val(Right(strMsg, Len(strMsg) - 2))
        
        If (MsgUsed(j) And 1) = 1 Then
          'we are OK; mark this as used by number
          MsgUsed(j) = MsgUsed(j) Or 2
        Else
          'error! this msg isn't even defined!
          rtfLogic.GetCharPos CLng(-lngMsgPos), i
          strMsg = "Syntax error in line " & CStr(i) & " must be corrected" & vbNewLine & _
                   "before message cleanup can continue:" & vbNewLine & vbNewLine
          MsgBoxEx strMsg & "Message 'm" & CStr(j) & "' is not declared", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#messages"
          Exit Sub
        End If
        'need to restore lngpos to a positive value
        lngMsgPos = -lngMsgPos
      Else
        'get line from pos
        rtfLogic.GetCharPos CLng(-lngMsgPos), i
        'set cursor to beginning of this line
        rtfLogic.Selection.Range.StartPos = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, i - 1, 0)
        rtfLogic.Selection.Range.EndPos = rtfLogic.Selection.Range.StartPos + SendMessage(rtfLogic.hWnd, EM_LINELENGTH, -lngMsgPos, 0)
        rtfLogic.SetFocus
        Select Case Val(strMsg)
        Case 1 'missing '(' after command
          MsgBoxEx "Syntax error in line " & CStr(i) & " must be corrected" & vbNewLine & _
                 "before message cleanup can continue:" & vbNewLine & vbNewLine & _
                 "Expected '(' after command text", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#action"
        Case 2 'missing end quote
          MsgBoxEx "Syntax error in line " & CStr(i) & " must be corrected" & vbNewLine & _
                 "before message cleanup can continue:" & vbNewLine & vbNewLine & _
                 "Missing quote mark (""" & ") at end of string", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#text"
        Case 3 'not a string
          MsgBoxEx "Syntax error in line " & CStr(i) & " must be corrected" & vbNewLine & _
                 "before message cleanup can continue:" & vbNewLine & vbNewLine & _
                 "Command argument is not a string or msg marker ('m##')", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Syntax Error", WinAGIHelp, "htm\commands\syntax.htm#text"
        End Select
        Exit Sub
      End If
    Else
      'check this msg against list of declared messages
      For j = 1 To 255
        If (MsgUsed(j) And 1) = 1 Then
          If Messages(j) = strMsg Then
            'if not yet marked as inuse by ref
            If (MsgUsed(j) And 4) <> 4 Then
              'mark as used by reference
              MsgUsed(j) = MsgUsed(j) Or 4
              'if not keeping all declared, need to increment the count
              If Not blnKeepUnused Then
                'increment msgcount
                intMsgCount = intMsgCount + 1
              End If
            End If
            Exit For
          End If
        End If
      Next j
      
      'if not found,
      If j = 256 Then
        'check this msg against new message collection
        For j = 1 To NewMsgs.Count
          If NewMsgs(j) = strMsg Then
            'found
            Exit For
          End If
        Next j
        
        'if still not found (unique to both)
        If j = NewMsgs.Count + 1 Then
          'add to new msg list
          NewMsgs.Add strMsg
          'increment Count
          intMsgCount = intMsgCount + 1
        End If
      End If
            
      'if too many msgs now
      If intMsgCount >= 256 Then
        MsgBoxEx "There are too many messages being used by this logic. AGI only" & vbNewLine & "supports 255 messages per logic. Edit the logic to reduce the" & vbNewLine & "number of messages to 255 or less.", vbCritical + vbOKOnly + vbMsgBoxHelpButton, "Too Many Messages", WinAGIHelp, "htm\agi\logics.htm#messages"
        Exit Sub
      End If
    End If
  Loop Until lngMsgPos = 0
  
  'Now add all newfound messages to the message array
  j = 1
  For i = 1 To NewMsgs.Count
    'if message is not in use (byref or bynum), we can overwrite it
    Do
      'if keeping declared, skip if declared
      If blnKeepUnused And (MsgUsed(j) And 1) = 1 Then
        j = j + 1
      
      'otherwise, skip if msg is in use (byref or bynum)
      ElseIf (MsgUsed(j) And 6) <> 0 Then
        j = j + 1
      Else
        'this number can be used for a new msg
        Exit Do
      End If
    Loop Until j >= 256 'should never get to this, but just in case
    '*'Debug.Assert j < 256
    
    Messages(j) = NewMsgs(i)
    'mark it as in use by ref
    MsgUsed(j) = MsgUsed(j) Or 4
    j = j + 1
  Next i
  
  'now build the message section using all messages that are marked as in use
  'get first message marker position (adjust by one to get actual start)
  lngMsgPos = FindNextToken(strLogic, lngMsgPos, "#message") - 1
  
  'if not found,
  If lngMsgPos = -1 Then
    'just add to end
    lngMsgPos = Len(rtfLogic.Text)
    'and add a comment header
    strMsg = vbNewLine & "[" & vbCr & "[ declared messages" & vbCr & "[" & vbCr
  End If
  
  'step through all messages
  For i = 1 To 255
    'if used by ref or num, OR if keeping all and it's declared,
    'add this msg
    If ((MsgUsed(i) And 6) <> 0) Or ((MsgUsed(i) And 1) = 1 And blnKeepUnused) Then
      strMsg = strMsg & "#message " & CStr(i) & " " & Messages(i) & vbCr
    End If
  Next i

  'now add new msg section
  With rtfLogic
    .Selection.Range.StartPos = lngMsgPos
    .Selection.Range.EndPos = .Range.EndPos 'this will delete everything from here to end of text
    .Selection.Range.Text = strMsg
    .Selection.Range.Collapse reChar
    .SetFocus
  End With
  
Exit Sub

ErrHandler:
  'if unable to extract messages, dont' do the update
  ErrMsgBox "An error occurred while attempting to update messages.", "The attempt has been aborted.", "Message Update Error"
              
  Err.Clear
End Sub

Public Sub MenuClickSelectAll()

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  rtfLogic.Range.SelectRange
End Sub

Public Sub MenuClickECustom2()
  
  'remove block comment marks
    
  On Error GoTo ErrHandler
  
  rtfLogic.Selection.Range.Uncomment
Exit Sub

ErrHandler:
  Err.Clear
End Sub

Private Function CheckQuickInfo(ByVal blnPrev As Boolean, Optional ByVal Force As Boolean = False, Optional ByVal IgnoreQ As Boolean = False) As Boolean

  'this function will examine current row, and determine if a command is being
  'edited
  
  ' it sets value of the module variables TipCmdPos, TipCurArg, TipCmdNum
  
  ' if blnPrev is TRUE, the function will return the immediately preceding
  ' token, instead of searching backwards for a command token
  
  ' if Force is FALSE, the function will automatically fail if a tip is
  ' currently visible; set Force to TRUE to run the function when a tip is
  ' visible
  
  ' if IgnoreQ is TRUE, the function will find a command, even if the cusor
  ' is inside a quote; default behavior is FALSE
  
  ' function returns TRUE if a valid AGI command is found, otherwise it
  ' returns FALSE
  
  Dim strLine As String, strCmd As String
  Dim strText As String, lngPos As Long
  Dim rtn As Long, i As Long
  Dim blnInComment As Boolean, blnInQuote As Boolean
  
  'if already visible,exit unless forcing
  If picTip.Visible And Not Force Then
    Exit Function
  End If
  
  'get line where enter was pressed
  rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, rtfLogic.Selection.Range.StartPos, 0)
  'get the start of this line
  rtn = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
  'get current row, up to point of cursor
  strLine = rtfLogic.Range(rtn, rtfLogic.Selection.Range.EndPos).Text
  
  'if the cursor pos is in a string, OR a comment marker occurs
  'in front of cursor, then no quick info should be shown
  rtn = CheckLine(strLine)
  If rtn <> 0 Then
    'either in a string, or comment
    
    ' if not ignoring quotes, OR if its a comment
    If Not IgnoreQ Or 0 Then
      'just exit
      Exit Function
    End If
  End If
  
  'check for a command infront of this spot
  strText = rtfLogic.Text
  lngPos = rtfLogic.Selection.Range.StartPos
  
  'get previous command from current pos
  '(force function to find the immediate cmd)
  strCmd = FindPrevCmd(strText, lngPos, TipCurArg, False, blnPrev)
  
  'adjust cmdpos
  TipCmdPos = lngPos
  
  'now check this command against list
  TipCmdNum = LogicCmd(strCmd)
  
  CheckQuickInfo = (TipCmdNum <> -1)
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub RepositionTip(ByVal lngPos As Long)

  Dim rtn As Long
  
  'compare lines; if cursor is not on same line as cmd,
  'draw the box at current location; otherwise
  'draw it at cmd pos
  lngPos = rtfLogic.Selection.Range.StartPos
  rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, lngPos, 0)
  If rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, TipCmdPos, 0) Then
    lngPos = TipCmdPos
  Else
    lngPos = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
  End If
  
  With picTip
    rtn = SendMessagePtW(rtfLogic.hWnd, EM_POSFROMCHAR, pPos, lngPos)
    .Top = pPos.Y + rtfLogic.Top + .Height + 6 '2
    'if top is below bottom edge,
    If .Top > rtfLogic.Height + rtfLogic.Top - .Height Then
      .Top = .Top - 2 * Me.TextHeight("Ay") - 6 '4
    End If
    .Left = pPos.X + rtfLogic.Left
  End With
End Sub

Private Sub ShowDefineList()

  'displays defines in a list box
  'that user can select from to replace current word (if cursor
  'is in a word) or insert at current position (if cursor is
  'in between words)

  Dim strLine As String, strCmd As String
  Dim strText As String, lngPos As Long, LastPos As Long
  Dim rtn As Long, i As Long
  Dim blnInComment As Boolean, blnInQuote As Boolean
  Dim tmpItem As ListItem
  Dim lngPosType As Long
  Dim tmpType As EArgListType

  On Error GoTo ErrHandler

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If

  'get line where cursor is
  rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, rtfLogic.Selection.Range.StartPos, 0)
  'get the start of this line
  LastPos = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
  'get current row
  strLine = rtfLogic.Range(LastPos, rtfLogic.Selection.Range.EndPos).Text

  'determine if current position is in a comment or quote
  lngPosType = CheckLine(strLine)
  'if in a comment
  If lngPosType = 2 Then
    'just exit- nothing to show
    Exit Sub
  End If

  'what's the arg Type?
  tmpType = GetArgType(lngPosType = 1)

  'add reserved defines, global defines, local defines and
  'any resIDs, invobjs or 'said' words
  BuildDefineList tmpType

  With lstDefines
    'if none
    If .ListItems.Count = 0 Then
      Exit Sub
    End If

    'set up cursor
    ExpandSelection rtfLogic, (lngPosType = 1)

    rtn = SendMessagePtW(rtfLogic.hWnd, EM_POSFROMCHAR, pPos, rtfLogic.Selection.Range.StartPos)
    .Top = pPos.Y + rtfLogic.Top + Me.TextHeight("Ay")
    'if top is below bottom edge,
    If .Top > rtfLogic.Height + rtfLogic.Top - .Height Then
      .Top = pPos.Y + rtfLogic.Top - .Height
    End If
    .Left = pPos.X + rtfLogic.Left
    .Width = Me.TextWidth("W") * 30
    'save start pos and text
    DefStartPos = rtfLogic.Selection.Range.StartPos
    PrevText = rtfLogic.Selection.Range.Text
    DefText = ""

    'show the listbox
    .Visible = True
    .Sorted = True

    'does it match (or partially match) an existing entry?
    i = rtfLogic.Selection.Range.Length
    If i <> 0 Then
      strText = rtfLogic.Selection.Range.Text
      For Each tmpItem In .ListItems
        Select Case StrComp(Left$(tmpItem.Text, i), strText, vbTextCompare)
        Case 0
          'select this one
          .SelectedItem = tmpItem
          tmpItem.EnsureVisible
          .SelectedItem.EnsureVisible
          .SetFocus
          Exit Sub
        Case 1 'strText>tmpItem.Text
          'stop here; don't select it, but scroll to it
          tmpItem.EnsureVisible
          .SetFocus
          .SelectedItem.Selected = False
          Exit Sub
        Case -1 'strText<tmpItem.Text
          'keep looking
        End Select
      Next
    End If

    'not found; select first item in list
    .ListItems(1).Selected = True
    .SelectedItem.EnsureVisible
    .SetFocus
    .BorderStyle = ccFixedSingle
  End With

  'lock the control so dbl-clicks don't cause trouble
  rtfLogic.Locked = True

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub ShowSnippetList()

  'displays snippets in a list box

  Dim strLine As String
  Dim strText As String, lngPos As Long, lngStart As Long
  Dim rtn As Long, i As Long
  Dim lngPosType As Long
  
  On Error GoTo ErrHandler

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If

  'get line where cursor is
  rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, rtfLogic.Selection.Range.StartPos, 0)
  'get the start of this line
  lngStart = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
  'indent value
  SnipIndent = rtfLogic.Selection.Range.StartPos - lngStart
  'get current row
  strLine = rtfLogic.Range(lngStart, rtfLogic.Selection.Range.EndPos).Text

  'mark list as dirty so next define action will rebuild it
  ListDirty = True
  
  'add  snippets
  With lstDefines
    .ListItems.Clear
    .Tag = "snippets"
    For i = 1 To UBound(CodeSnippets())
      .ListItems.Add(, , CodeSnippets(i).Name).Tag = CodeSnippets(i).Value
    Next i

    'if none
    If .ListItems.Count = 0 Then
      Exit Sub
    End If

    rtn = SendMessagePtW(rtfLogic.hWnd, EM_POSFROMCHAR, pPos, rtfLogic.Selection.Range.StartPos)
    .Top = pPos.Y + rtfLogic.Top + Me.TextHeight("Ay")
    'if top is below bottom edge,
    If .Top > rtfLogic.Height + rtfLogic.Top - .Height Then
      .Top = pPos.Y + rtfLogic.Top - .Height
    End If
    .Left = pPos.X + rtfLogic.Left
    .Width = Me.TextWidth("W") * 30
    'save start pos and text
    DefStartPos = rtfLogic.Selection.Range.StartPos
    PrevText = rtfLogic.Selection.Range.Text
    DefText = ""

    'show the listbox
    .Visible = True
    .Sorted = False

    'select first item in list
    .ListItems(1).Selected = True
    .SelectedItem.EnsureVisible
    .SetFocus
    .BorderStyle = ccFixedSingle
  End With

  'lock the control so dbl-clicks don't cause trouble
  rtfLogic.Locked = True

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub ShowSynonymList(ByVal aWord As String)

  'displays a list of synonyms in a list box

  Dim strLine As String, strCmd As String
  Dim strText As String, lngPos As Long, LastPos As Long
  Dim rtn As Long, i As Long
  Dim blnInComment As Boolean, blnInQuote As Boolean
  Dim tmpItem As ListItem
  Dim lngPosType As Long
  Dim tmpType As EArgListType
  Dim GrpNum As Long, GrpCount As Long
  
  On Error GoTo ErrHandler

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If

  'get line where cursor is
  rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, rtfLogic.Selection.Range.StartPos, 0)
  'get the start of this line
  LastPos = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
  'get current row
  strLine = rtfLogic.Range(LastPos, rtfLogic.Selection.Range.EndPos).Text

  'determine if current position is in a comment or quote
  lngPosType = CheckLine(strLine)
  'if in a comment
  If lngPosType = 2 Then
    'just exit- nothing to show
    Exit Sub
  End If

  ListDirty = True
  
  'add  'said' words
  GrpNum = VocabularyWords(aWord).Group
  GrpCount = VocabularyWords.GroupN(GrpNum).WordCount - 1
  
  With lstDefines
    .ListItems.Clear
    .Tag = "words"
    For i = 0 To GrpCount
      .ListItems.Add(, , Chr$(34) & VocabularyWords.GroupN(GrpNum).Word(i) & Chr$(34), , 22).Tag = CStr(i)
    Next i

    'if none
    If .ListItems.Count = 0 Then
      Exit Sub
    End If

    'set up cursor
    ExpandSelection rtfLogic, (lngPosType = 1)

    rtn = SendMessagePtW(rtfLogic.hWnd, EM_POSFROMCHAR, pPos, rtfLogic.Selection.Range.StartPos)
    .Top = pPos.Y + rtfLogic.Top + Me.TextHeight("Ay")
    'if top is below bottom edge,
    If .Top > rtfLogic.Height + rtfLogic.Top - .Height Then
      .Top = pPos.Y + rtfLogic.Top - .Height
    End If
    .Left = pPos.X + rtfLogic.Left
    .Width = Me.TextWidth("W") * 30
    'save start pos and text
    DefStartPos = rtfLogic.Selection.Range.StartPos
    PrevText = rtfLogic.Selection.Range.Text
    DefText = ""

    'show the listbox
    .Visible = True
    .Sorted = True

    'select the word
    For Each tmpItem In .ListItems
      If StrComp(Left$(tmpItem.Text, i), aWord, vbTextCompare) = 0 Then
        'select this one
        .SelectedItem = tmpItem
        tmpItem.EnsureVisible
        .SelectedItem.EnsureVisible
        .SetFocus
        Exit Sub
      End If
    Next

    'not found; select first item in list
    .ListItems(1).Selected = True
    .SelectedItem.EnsureVisible
    .SetFocus
    .BorderStyle = ccFixedSingle
  End With

  'lock the control so dbl-clicks don't cause trouble
  rtfLogic.Locked = True

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub ShowTip()

  'always hide define
  picDefine.Visible = False
  
  With picTip
    'reposition it first
    RepositionTip TipCmdPos + 1
    
    .Cls
    .FontBold = False

    'add command
    'build arg list
    ShowCmdTip TipCmdNum, TipCurArg
    .Visible = True
  End With
End Sub

Public Sub UpdateID(ByVal NewID As String, NewDescription As String)

  On Error GoTo ErrHandler
  
  If LogicEdit.Description <> NewDescription Then
    LogicEdit.Description = NewDescription
  End If
  
  If LogicEdit.ID <> NewID Then
    LogicEdit.ID = NewID
    'set captions
    Caption = sLOGED & ResourceName(LogicEdit, InGame, True)
    If rtfLogic.Dirty Then
      Caption = sDM & Caption
    End If
  End If

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub UpdateTip()

  Dim rtn As Long, rngTemp As Range
  
  'if not in string or comment
  Set rngTemp = rtfLogic.Range(rtfLogic.Selection.Range.StartPos, rtfLogic.Selection.Range.EndPos)
  rngTemp.StartOf reLine, True
  
  'if this range of text doesn't end in a comment or quote
  If CheckLine(rngTemp.Text) = 0 Then
    'has lngcurgarg changed?
    rtn = TipCurArg
    FindPrevCmd rtfLogic.Text, rtfLogic.Selection.Range.StartPos, TipCurArg
    
    If rtn <> TipCurArg Then
      'reposition if necessary
      RepositionTip TipCmdPos + 1
      
      'need to update args
      ShowCmdTip TipCmdNum, TipCurArg
    End If
  End If
End Sub

Private Sub Form_Activate()
  
  On Error GoTo ErrHandler
  
  'if minimized, exit
  '(to deal with occasional glitch causing focus to lock up)
  If Me.WindowState = vbMinimized Then
    Exit Sub
  End If
  
  'if hiding prevwin on lost focus, hide it now
  If Settings.HidePreview Then
    If PreviewWin.Visible Then
      PreviewWin.Hide
    End If
  End If
 
  'if visible,
  If Visible Then
    'force resize
    Form_Resize
  End If
  
  'if findform is visible,
  If FindForm.Visible Then
    'set correct mode
    If FindForm.txtReplace.Visible Then
      'show in replace logic mode
      FindForm.SetForm ffReplaceLogic, InGame
    Else
      'show in find logic mode
      FindForm.SetForm ffFindLogic, InGame
    End If
  End If
  
  'if form is active,
  If frmMDIMain.Enabled Then
    'always set focus to textbox
    rtfLogic.SetFocus
  End If
  
  'set searching form to this form
  Set SearchForm = Me
  'then ensure it findform is enabled
  FindForm.Enabled = True
  
  'adjust menus and statusbar
  AdjustMenus rtLogic, InGame, True, rtfLogic.Dirty
  UpdateStatusBar
  SettingError = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickCopy()

  rtfLogic.Selection.Range.Copy
  
  'reset globals clipboard
  ReDim GlobalsClipboard(0)
  
  UpdateStatusBar
End Sub
Public Sub MenuClickCut()
  
  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  rtfLogic.Selection.Range.Cut
  
  'ALWAYS reset search flags
  FindForm.ResetSearch

  'reset globals clipboard
  ReDim GlobalsClipboard(0)
  
  UpdateStatusBar
End Sub
Public Sub MenuClickPaste()

  Dim blnSmartPaste As Boolean
  
  On Error GoTo ErrHandler
  
  'ALWAYS reset search flags
  FindForm.ResetSearch

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  'if selection has space at end?
  If rtfLogic.Selection.Range.Length > 0 And Len(Clipboard.GetText) > 0 Then
    blnSmartPaste = (Asc(Right(rtfLogic.Selection.Range.Text, 1)) = 32 And Asc(Right(Clipboard.GetText, 1)) <> 32)
  End If
  'if across multiple lines, override smartpaste to false
  If InStr(1, rtfLogic.Selection.Range.Text, vbCr) > 0 Then
    blnSmartPaste = False
  End If
  
  'do the paste operation
  rtfLogic.Selection.Range.Paste
  
  'smart paste?
  If blnSmartPaste Then
    rtfLogic.Selection.Range.Text = " "
  End If
  
  UpdateStatusBar
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickUndo()

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  rtfLogic.Undo
  
  UpdateStatusBar
End Sub

Public Sub SetEditMenu()
  'sets the menu captions on the Edit menu
  'based on current selection
  
  Dim strToken As String
  
  On Error GoTo ErrHandler
  
  With frmMDIMain
    'undo, redo, bar0 always visible
    .mnuEUndo.Visible = True
    .mnuERedo.Visible = True
    .mnuEBar0.Visible = True
    
    'enabled only if something to undo
    .mnuEUndo.Enabled = rtfLogic.CanUndo
    .mnuEUndo.Caption = "&Undo"
    If rtfLogic.CanUndo Then
      'set caption based on what last action was
      Select Case rtfLogic.UndoType
      Case reUndoTyping
        .mnuEUndo.Caption = .mnuEUndo.Caption & " Typing"
      Case reUndoCut
        .mnuEUndo.Caption = .mnuEUndo.Caption & " Cut"
      Case reUndoPaste
        .mnuEUndo.Caption = .mnuEUndo.Caption & " Paste"
      Case reUndoDelete
        .mnuEUndo.Caption = .mnuEUndo.Caption & " Delete"
      Case reUndoDragDrop
        .mnuEUndo.Caption = .mnuEUndo.Caption & " Drag/drop"
      End Select
    End If
    .mnuEUndo.Caption = .mnuEUndo.Caption & vbTab & "Ctrl+Z"
    
    'enabled only if something to redo
    .mnuERedo.Enabled = rtfLogic.CanRedo
    .mnuERedo.Caption = "&Redo"
    If rtfLogic.CanRedo Then
      Select Case rtfLogic.RedoType
      Case reUndoTyping
        .mnuERedo.Caption = .mnuERedo.Caption & " Typing"
      Case reUndoCut
        .mnuERedo.Caption = .mnuERedo.Caption & " Cut"
      Case reUndoPaste
        .mnuERedo.Caption = .mnuERedo.Caption & " Paste"
      Case reUndoDelete
        .mnuERedo.Caption = .mnuERedo.Caption & " Delete"
      Case reUndoDragDrop
        .mnuERedo.Caption = .mnuERedo.Caption & " Drag/drop"
      End Select
    End If
    .mnuERedo.Caption = .mnuERedo.Caption & vbTab & "Ctrl+Y"
    
    'cut always visible, and enabled if something selected
    .mnuECut.Visible = True
    .mnuECut.Enabled = rtfLogic.Selection.SelType <> reSelectionIP
    .mnuECut.Caption = "Cu&t" & vbTab & "Ctrl+X"
    
    'copy is same as cut
    .mnuECopy.Visible = True
    .mnuECopy.Enabled = .mnuECut.Enabled
    .mnuECopy.Caption = "&Copy" & vbTab & "Ctrl+C"
    
    'paste always visibie, enabled if something to paste
    .mnuEPaste.Visible = True
    .mnuEPaste.Enabled = Clipboard.GetFormat(vbCFText)
    .mnuEPaste.Caption = "&Paste" & vbTab & "Ctrl+V"
    
    'delete is always visible, enabled if something to delete
    .mnuEDelete.Visible = True
    .mnuEDelete.Enabled = .mnuECut.Enabled
    .mnuEDelete.Caption = "Delete" & vbTab & "Del"
    
    'clear visible if snippets are in use
    .mnuEClear.Visible = Settings.Snippets
    .mnuEClear.Enabled = Settings.Snippets
    If Settings.Snippets Then
      If rtfLogic.Selection.Range.Length = 0 Then
        .mnuEClear.Caption = "Insert Code Snippet ..." & vbTab & "Ctrl+T"
        .mnuEClear.Enabled = (UBound(CodeSnippets()) > 0)
      Else
        .mnuEClear.Caption = "Create Code Snippet ..." & vbTab & "Ctrl+T"
      End If
    End If
    
    'insert is  visible
    .mnuEInsert.Visible = True
    .mnuEInsert.Visible = True
    .mnuEInsert.Enabled = True
    .mnuEInsert.Caption = "List Defines" & vbTab & "Ctrl+J"
            
    'select all always visible and enabled
    .mnuESelectAll.Visible = True
    .mnuESelectAll.Enabled = True
    .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"
    
    'bar1, find and replace always visible and enabled
    .mnuEBar1.Visible = True
    .mnuEFind.Visible = True
    .mnuEFind.Enabled = True
    .mnuEFind.Caption = "&Find" & vbTab & "Ctrl+F"
    .mnuEReplace.Visible = True
    .mnuEReplace.Enabled = True
    .mnuEReplace.Caption = "&Replace" & vbTab & "Ctrl+H"
    
    'find next always visible, only enabled if a search is active
    .mnuEFindAgain.Visible = True
    .mnuEFindAgain.Enabled = (LenB(GFindText) <> 0)
    .mnuEFindAgain.Caption = "Find &Next" & vbTab & "F3"
    
    'custom menu 1 is visible
    .mnuEBar2.Visible = True
    .mnuECustom1.Visible = True
    .mnuECustom1.Enabled = True
    .mnuECustom1.Caption = "Block Comment" & vbTab & "Alt+B"
    
    'custom menu 2 is also visible
    .mnuECustom2.Visible = True
    .mnuECustom2.Enabled = True
    .mnuECustom2.Caption = "Block Uncomment" & vbTab & "Alt+U"
    
    'custom menu 3 is only visible if an editable resource is under cursor)
    strToken = TokenFromCursor(rtfLogic)
    If IsResource(strToken) Then
      'allow editing
      .mnuECustom3.Caption = "Open " & strToken & " for Editing" & vbTab & "Shift+Ctrl+E"
      .mnuECustom3.Enabled = True
      .mnuECustom3.Visible = True
    Else
      'assume no synonyms
      .mnuECustom3.Visible = False
      .mnuECustom3.Enabled = False
      'if cursor is part of a command
      If CheckQuickInfo(False, False, True) Then
        'if a said command
        If TipCmdNum = 116 Then
          'get word
          strToken = TokenFromCursor(rtfLogic, True, True)
          If VocabularyWords.WordExists(Mid(strToken, 2, Len(strToken) - 2)) Then
            .mnuECustom3.Caption = "View Synonyms for " & strToken  ' & vbTab & "Shift+Ctrl+E"
            'only enable if more than one
            .mnuECustom3.Enabled = (VocabularyWords.GroupN(VocabularyWords(Mid(strToken, 2, Len(strToken) - 2)).Group).WordCount > 1)
            .mnuECustom3.Visible = True
          End If
        End If
      End If
    End If
    
    'set toggle status for IsRoom menu
    .mnuRCustom3.Checked = LogicEdit.IsRoom
    .mnuRCustom3.Enabled = InGame And (LogicEdit.Number <> 0)
    
    Toolbar1.Buttons(1).Enabled = .mnuECut.Enabled
    Toolbar1.Buttons(2).Enabled = .mnuECopy.Enabled
    Toolbar1.Buttons(3).Enabled = .mnuEPaste.Enabled
    Toolbar1.Buttons(4).Enabled = .mnuEDelete.Enabled
    Toolbar1.Buttons(6).Enabled = .mnuEUndo.Enabled
    Toolbar1.Buttons(7).Enabled = .mnuERedo.Enabled
    Toolbar1.Buttons(8).Enabled = .mnuEFind.Enabled
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

  'detect and respond to keyboard shortcuts
  
  '** for some reason, key preview does not work when the RichEdAGI object
  'has focus; keydown events go straight to the object's keyhandler
  
  'if any other control has focus, set focus to the richedit control and
  ' then send the keyboard event to the RichEdAGI object
  
  '*'Debug.Assert Not Me.ActiveControl Is rtfLogic
  
  On Error GoTo ErrHandler

  'if listbox is visible, let it handle the keypresses
  If lstDefines.Visible Then
    Exit Sub
  End If
  
  rtfLogic.SetFocus
  
  rtfLogic_KeyDown KeyCode, Shift

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub Form_Load()

  CalcWidth = MIN_WIDTH
  CalcHeight = MIN_HEIGHT
  
  'flag to ignore changes until after load complete
  mLoading = True
  
  'initialize font settings
  InitFonts
  
  'form is a logic
  FormMode = fmLogic
  
  'mark defines and list as dirty
  DefDirty = True
  ListDirty = True
  ListType = -2
  
  'reset load flag
  mLoading = False
End Sub

Private Sub Form_LostFocus()

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  'mark define LIST as dirty
  ListDirty = True
End Sub

Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)

  'check if save is necessary (and set cancel if user cancels)
  Cancel = Not AskClose
End Sub

Private Sub Form_Unload(Cancel As Integer)

  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'if unloading due to error on startup
  'logicedit will be set to nothing
  If Not LogicEdit Is Nothing Then
    'dereference object
    LogicEdit.Unload
    Set LogicEdit = Nothing
  End If
  
  'if ingame logic is loaded (and is a valid number), unload it too
  If Me.InGame And Me.LogicNumber >= 0 Then
    If Logics(Me.LogicNumber).Loaded Then
      Logics(Me.LogicNumber).Unload
    End If
  End If
  
  'remove from logic editor collection
  For i = 1 To LogicEditors.Count
    If LogicEditors(i) Is Me Then
      LogicEditors.Remove i
      ' if this logic was starting logic for a search
      If SearchStartLog = i Then
        'need to reset the search
        FindForm.ResetSearch
      End If
      Exit For
    End If
  Next i
  
  'if this form is the searchform
  If SearchForm Is Me Then
    Set SearchForm = Nothing
  End If
  
  'need to check if this is last form
  LastForm Me
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub lstDefines_DblClick()

  Dim strValue As String
  
  On Error GoTo ErrHandler
  
  '**************
  'OK, this one causes all kinds of trouble without understanding
  ' the underlying nature of windows and focus; when the listbox
  ' is shown by the context menu, all of the code in the mousedown
  ' event is completed before focus shifts to the listbox
  ' then, when this listbox is dismissed, focus goes to next control
  ' in line; if that's the rtf window, then things go wonky because
  ' it gets focus before all the stuff in this logic are
  ' finished; that's where the 'drag/drop' effect and the ghost-jump
  ' to start of logic come from
  ' with another active control on the form that comes after this
  ' list, then the problem goes away because the list can finish
  ' all its work without events getting sent to the rtfLogic;
  ' but then we have to get focus back to the rtf editor; it can't
  ' be done within any of the listbox events, because the timing
  ' will still be off, and the bugs return, so answer is to use
  ' another timer; initial testing shows the time needed to flush
  ' messages is around 40ms;
  
  'make sure something selected
  If lstDefines.SelectedItem Is Nothing Then
    Exit Sub
  End If
  
  'replace text with selected define
  With rtfLogic.Selection.Range
    .StartPos = DefStartPos
    If lstDefines.Tag = "snippets" Then
      strValue = AddArgs(lstDefines.SelectedItem.Tag)
      If SnipIndent > 0 Then
        .Text = Replace(strValue, vbCr, vbCr & Space(SnipIndent))
      Else
        .Text = strValue
      End If
    Else
      .Text = lstDefines.SelectedItem.Text
    End If
    .StartPos = .EndPos
    DefEndPos = .EndPos
  End With
  
  'get the line index of the selected line
  DefTopLine = SendMessage(rtfLogic.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
  
  lstDefines.Visible = False
  
'''  ' can't do this!
'''  rtfLogic.SetFocus
  'use timer instead
  tmrListDblClick.Enabled = True

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub lstDefines_KeyPress(KeyAscii As Integer)

  Dim i As Long, strTmpDef As String
  Dim lngLine As Long, lngFirst As Long
  Dim strValue As String
  
  On Error GoTo ErrHandler
  
  With rtfLogic.Selection.Range
    'if enter, select this word
    If KeyAscii = 13 Or KeyAscii = 9 Then
      'ignore the keypress?
      KeyAscii = 0
      'make sure something selected
      If lstDefines.SelectedItem Is Nothing Then
        rtfLogic.SetFocus
        Exit Sub
      End If
      .StartPos = DefStartPos
      If lstDefines.Tag = "snippets" Then
        strValue = AddArgs(lstDefines.SelectedItem.Tag)
        
        If SnipIndent > 0 Then
          .Text = Replace(strValue, vbCr, vbCr & Space(SnipIndent))
        Else
          .Text = strValue
        End If
      Else
        .Text = lstDefines.SelectedItem.Text
      End If
      .StartPos = .EndPos
      lstDefines.Visible = False
      
'''      ' can't do this!
'''      rtfLogic.SetFocus
      'use timer instead
      tmrListDblClick.Enabled = True
      Exit Sub
    End If
    
    'if escape, restore selection, and then exit
    If KeyAscii = 27 Then
      'if not a snippet
      If lstDefines.Tag <> "snippets" Then
        .StartPos = DefStartPos
        .Text = PrevText
      End If
      lstDefines.Visible = False
      rtfLogic.SetFocus
      Exit Sub
    End If
    
    'if a snippet
    If lstDefines.Tag = "snippets" Then
      'ignore all othr key presses
      KeyAscii = 0
      Exit Sub
    End If
    
    'if backspace, need to delete a character
    If KeyAscii = 8 Then
      If .EndPos > 1 And .EndPos > DefStartPos Then
        DefText = Left(DefText, Len(DefText) - 1)
        .Text = DefText
      End If
    Else
      'replace selection with key (if there is something selected, or
      'add newly typed character
      DefText = DefText & Chr(KeyAscii)
      .Text = DefText
    End If
      
    'find closest match, if there's something typed
    If Len(DefText) > 0 Then
      For i = 1 To lstDefines.ListItems.Count
        strTmpDef = lstDefines.ListItems(i).Text
        If UCase(Left(strTmpDef, Len(DefText))) = UCase(DefText) Then
          lstDefines.ListItems(i).Selected = True
          lstDefines.ListItems(i).EnsureVisible
          Exit For
        End If
      Next i
    End If
  End With
  
  KeyAscii = 0
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub lstDefines_LostFocus()
  
  lstDefines.Visible = False
  picTip.Visible = False
  tmrListDef.Enabled = False
End Sub

Private Sub lstDefines_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  On Error Resume Next
  
  'always hide the tooltip, and start timer
  picTip.Visible = False
  
  'reset the defines timer
  'always reset the timer when the mouse moves
  tmrListDef.Enabled = False
  'defines are only shown if mouse is in normal state (no buttons or shift keys)
  tmrListDef.Enabled = (Button = 0 And Shift = 0)
  
  'don't show if on scrollbar
  If X / ScreenTWIPSX > lstDefines.Width - 25 Then
    DefTip = ""
  Else
    DefTip = lstDefines.HitTest(X, Y).Tag
    picTip.Move X / ScreenTWIPSX + lstDefines.Left + 15, Y / ScreenTWIPSY + lstDefines.Top + 15 ', picTip.TextWidth(DefTip) * 1.1, picTip.TextHeight(DefTip) * 1.1
  End If
End Sub


Private Sub picTip_GotFocus()

  'in case user clicks it or anything...
  
  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
End Sub


Private Sub rtfLogic_AcceptData(Formats() As Integer, format As Integer, ByVal EventSource As reDataEventSource)

  On Error GoTo ErrHandler
  
  'if dropping a word or object,
  If DroppingWord Or DroppingObj Then
    'add quotes, so the string being passed will drop between them
    With rtfLogic.Selection.Range
      .Text = QUOTECHAR & QUOTECHAR
      .StartPos = .StartPos + 1
      .EndPos = .StartPos
    End With
  End If

  'if dropping a global define
  If DroppingGlobal Then
    'no other action needed
  End If

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub rtfLogic_Change()
  
  ' check for stupid control keys
  If NoChange Then
    'reset and ignore
    NoChange = False
    Exit Sub
  End If
  
  'set dirty flag
  MarkAsDirty

  'also mark defines as dirty
  DefDirty = True
  
  'clear status bar
  If frmMDIMain.ActiveForm Is Me And Not mLoading Then
    UpdateStatusBar
    MainStatusBar.Panels("Status").Text = vbNullString
  End If
End Sub
Function AskClose() As Boolean

  Dim rtn  As VbMsgBoxResult
  Dim blnSkipWarn As Boolean
  Dim blnLoaded As Boolean
  
  On Error GoTo ErrHandler
  
  'assume okay to close initially,
  AskClose = True
  
  'if exiting due to error on form load, logicedit is set to nothing
  If LogicEdit Is Nothing Then
    Exit Function
  End If
  
  'if text has been modified
  '(number is set to -1 if closing is forced)
  If rtfLogic.Dirty And LogicNumber <> -1 Then
    'get user input
    rtn = MsgBox("Do you want to save changes to source code?", vbYesNoCancel, Caption)
    Select Case rtn
    Case vbYes
      'save source
      MenuClickSave

      'if still dirty,
      If rtfLogic.Dirty Then
        'verify continue closing
        rtn = MsgBox("File not saved. Continue closing anyway?", vbYesNo, Caption)
        AskClose = (rtn = vbYes)
        Exit Function
      End If
    
    Case vbCancel 'if user says cancel,
      AskClose = False
      Exit Function
      
    Case vbNo
      'take no action; continue closing
      Exit Function
    End Select
  End If
  
  'if in a game, and not compiled
  If InGame And LogicNumber <> -1 Then
    If Not Logics(LogicNumber).Compiled And Settings.WarnCompile Then
      'ask if should save
      rtn = MsgBoxEx("Do you want to compile and save this logic?", vbQuestion + vbYesNoCancel, Caption, , , "Don't show this warning again", blnSkipWarn)
      Select Case rtn
      Case vbYes
        'compile and save
        'compile logic
        If Not CompileLogic(Me, LogicNumber) Then
          'on error, exit
          AskClose = False
          Exit Function
        End If
        
        'update preview and properties
        UpdateSelection rtLogic, LogicNumber, umPreview Or umProperty
        
        AskClose = True
        
      Case vbNo
        AskClose = True
      Case vbCancel
        AskClose = False
      End Select
      
      'if asked to skip further warnings
      If blnSkipWarn Then
        Settings.WarnCompile = False
        WriteSetting GameSettings, sGENERAL, "WarnCompile", Settings.WarnCompile
      End If
    End If
  End If
Exit Function

ErrHandler:
  'new files will not have a crc;
  'that will generate an error that
  'can be safely ignored
  If Err.Number <> vbObjectError + 592 Then
    '*'Debug.Assert False
  End If
  Resume Next
End Function
Private Sub Form_Resize()

  On Error GoTo ErrHandler
  
  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
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
    rtfLogic.Width = CalcWidth
    rtfLogic.Height = CalcHeight - Toolbar1.Height
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub UpdateStatusBar()
  
  Dim lngRow As Long, lngCol As Long
  Dim blnErrFix As Boolean
  
  On Error GoTo ErrHandler
  
  If Not Me.Visible Then
    Exit Sub
  End If
  
  If MainStatusBar.Tag <> CStr(rtLogic) Then
    'show correct status
    AdjustMenus rtLogic, InGame, True, rtfLogic.Dirty
  End If
  
  'get row and column of selection
  rtfLogic.GetCharPos rtfLogic.Selection.Range.EndPos, lngRow, lngCol
  
  'set status:
  With MainStatusBar
    .Panels("Row").Text = "Line: " & CStr(lngRow)
    .Panels("Col").Text = "Col: " & CStr(lngCol)
  End With
  
  'if mouse NOT down
  If Not MouseDown Then
    SetEditMenu
  End If
Exit Sub

ErrHandler:
  'if error is due to wrong status bar
  If Err.Number = 35601 And Not blnErrFix Then
  '*'Debug.Assert False
    'force update and retry
    blnErrFix = True
    AdjustMenus rtLogic, False, True, rtfLogic.Dirty
    Resume
  Else
    '*'Debug.Assert False
    Resume Next
  End If
End Sub
Private Sub MarkAsDirty()
  
  'ignore when loading (not visible yet)
  If Not Visible Then
    Exit Sub
  End If
  
  'enable menu and toolbar button
  frmMDIMain.mnuRSave.Enabled = True
  frmMDIMain.Toolbar1.Buttons("save").Enabled = True
  
  If Asc(Caption) <> 42 Then
    'mark caption
    Caption = sDM & Caption
  End If
  
End Sub

Private Sub rtfLogic_DblClick(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  blnDblClick = True
End Sub

Private Sub rtfLogic_GotFocus()

  On Error GoTo ErrHandler
  
  'due to a bug that causes external dbl-clicks to look like
  'a drag/drop operation, we lock the control when showing
  'the defines list; if focus ever comes back to the control
  'while it's locked, DoEvents seems to flush the drag/drop
  'buffer, then we can unlock the control
  
  If rtfLogic.Locked Then
    DoEvents
    rtfLogic.Locked = False
  End If
  
  'if not active form
  If Not frmMDIMain.ActiveForm Is Me Then
    'not sure why, but need to exit here to avoid endless looping between two forms
    Exit Sub
  End If
  
  If Not mLoading Then
    'force update of menus
    UpdateStatusBar
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub rtfLogic_KeyDown(KeyCode As Integer, Shift As Integer)

  'detect and respond to keyboard shortcuts
  
  On Error GoTo ErrHandler

  Dim strIn As String, i As Integer
  Dim CharPicker As frmCharPicker
  
  ' if keycode is <32, need to ignore change
  ' for all except backspace, tab and return
  If KeyCode < 32 Then
    Select Case KeyCode
    Case 8, 9, 10, 13
      'ok
      NoChange = False
    Case Else
      ' no change should happen
      NoChange = True
    End Select
  Else
     NoChange = False
  End If
  
  'always check for help first
  If Shift = 0 And KeyCode = vbKeyF1 Then
    MenuClickHelp
    KeyCode = 0
    Exit Sub
  End If
  
  'check for global shortcut keys
  CheckShortcuts KeyCode, Shift
  If KeyCode = 0 Then
    'if any shortcuts took place,
    'just exit
    Exit Sub
  End If
  
  'for richtext editor only, need to intercept ALT key combos,
  'control keys that are assigned permanent shortcuts or builtin functionality
  Select Case Shift
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyN, vbKeyO, vbKeyB, vbKeyR, vbKeyS, vbKeyE, vbKeyD, vbKeyI, vbKeyP, vbKeyL, vbKeyG, vbKeyM, vbKeyF1, vbKeyF2, vbKeyF3, vbKeyF4, vbKeyF5, vbKeyF6, vbKeyF7
      'don't respond to these keys
      KeyCode = 0
      Shift = 0

    Case vbKeyTab
      KeyCode = 0
      Shift = 0
      'move to next window
      SendMessage Me.hWnd, WM_SYSCOMMAND, SC_NEXTWINDOW, 0

    Case vbKeyF
      'find
      If frmMDIMain.mnuEFind.Enabled Then
        MenuClickFind
      End If
      KeyCode = 0
      Shift = 0

    Case vbKeyH
      'replace
      If frmMDIMain.mnuEReplace.Enabled Then
        MenuClickReplace
      End If
      KeyCode = 0
      Shift = 0

    Case vbKeyJ
      'Ctrl+J
      ShowDefineList
      KeyCode = 0
      Shift = 0
      
    Case vbKeyV
      'custom paste function, instead of the built in one
      MenuClickPaste
      KeyCode = 0
      Shift = 0
      
    Case vbKeyC
      'reset the globals clipboard
      ReDim GlobalsClipboard(0)
      
    Case vbKeyX
      'reset the globals clipboard
      ReDim GlobalsClipboard(0)
      
      ' reset search flags
      FindForm.ResetSearch
      
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
          rtfLogic.Selection.Range.Text = strIn
        End If
      End If
      
      Unload CharPicker
      Set CharPicker = Nothing
      
      KeyCode = 0
      Shift = 0
      
    Case vbKeyT
      'insert/create snippet
      MenuClickClear
      KeyCode = 0
      Shift = 0
    End Select

  Case 0 'no shift, ctrl, or alt
    Select Case KeyCode
    Case vbKeyF3
      'find again
      If frmMDIMain.mnuEFindAgain.Enabled Then
        MenuClickFindAgain
      End If
      KeyCode = 0

    Case vbKeyF8  'compile
      If frmMDIMain.mnuRCustom1.Enabled Then
        MenuClickCustom1
      End If
      KeyCode = 0

    Case vbKeyF2, vbKeyF4, vbKeyF6, vbKeyF11
      'don't respond to these keys
      KeyCode = 0
      
    Case vbKeyDelete
      ' delete key causes search reset
      'ALWAYS reset search flags
      FindForm.ResetSearch
    End Select

  Case vbShiftMask + vbCtrlMask
    Select Case KeyCode
    Case vbKeyTab
      'move to previous window
      SendMessage Me.hWnd, WM_SYSCOMMAND, SC_PREVWINDOW, 0
      KeyCode = 0
      Shift = 0
      Exit Sub
      
    Case vbKeyS
      'save all
      'step through all editors
      For i = 1 To LogicEditors.Count
        If LogicEditors(i).FormMode = fmLogic Then
          If LogicEditors(i).rtfLogic.Dirty Then
            LogicEditors(i).MenuClickSave
          End If
        End If
      Next i
      KeyCode = 0
      Shift = 0
      
    Case vbKeyE
      ' open resource under cursor for editing
      If IsResource(TokenFromCursor(rtfLogic)) Then
        MenuClickECustom3
      End If
    End Select

  Case vbAltMask
    Select Case KeyCode
    Case vbKeyB
      'Alt+B
      MenuClickECustom1
      'pressing ALT combos causes a 'ding'; can't find a way to turn that off
      ' unless a msgbox is displayed; need to find a solution
      'and after showing the msgbox, focus goes somewhere, but IDK- it's not the rtfwindow anymore
      '
      'it appears to be related to the assigned shortcut keys on the main menu
      'if a key is assigned and menu is visible, no ding; if menu is not visible
      'or key is not assigned, we get a ding - oh well
      rtfLogic.SetFocus
      KeyCode = 0
      Shift = 0
      
    Case vbKeyM
      'msg cleanup
      If frmMDIMain.mnuRCustom2.Enabled Then
        MenuClickCustom2
      End If
      KeyCode = 0
      Shift = 0
      
    Case vbKeyU
      'Alt+U
      MenuClickECustom2
      KeyCode = 0
      Shift = 0
      rtfLogic.SetFocus
    End Select
    
  Case vbShiftMask
    'nothing yet...
    Select Case KeyCode
    Case vbKeyInsert
      KeyCode = 0
      Shift = 0
    End Select
    
  End Select
  
  'shortcuts for editing are built in to the rtf box
  ' redo
  ' undo
  ' cut
  ' copy
  ' paste
  ' delete
  ' select all
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub rtfLogic_KeyPress(KeyAscii As Integer)
  
  Dim lngLineStart As Long, lngPos As Long
  Dim strLine As String, rngTemp As Range
  
  On Error GoTo ErrHandler

  'ALWAYS reset search flags
  FindForm.ResetSearch

  'ensure define is hidden
  picDefine.Visible = False
  
  Select Case KeyAscii
  Case 9  'TAB
    'wow! i never caught this before???? tabs need to be ignored;
    ' they are converted to spaces automatically by the control
    KeyAscii = 0
    
  Case 13 'ENTER
    
  Case 8  'BACKSPACE'
    
    'if pictip is visible
    If picTip.Visible Then
      'if cursor has backed over start of command needing the tip
      If rtfLogic.Selection.Range.StartPos <= TipCmdPos Then
        'hide it
        picTip.Visible = False
      End If
      
      UpdateTip
      If picTip.Visible Then
        RepositionTip rtfLogic.Selection.Range.EndPos
      End If
    End If
    
  Case 40 'open parenthesis
    If Not picTip.Visible And Settings.AutoQuickInfo Then
      'check for quicktip
      If CheckQuickInfo(True) Then
        ShowTip
      End If
    End If
    
  Case 32, 44 'space, comma
    'if tip not visible and tips are enabled
    If Not picTip.Visible And Settings.AutoQuickInfo Then
      'check if we need to show it
      If CheckQuickInfo(False) Then
        'if a comma, need to increment arg pos
        If KeyAscii = 44 Then
          'increment arg counter
          TipCurArg = TipCurArg + 1
        End If
        ShowTip
      End If
    Else
      'update highlighted argument
      Set rngTemp = rtfLogic.Range(rtfLogic.Selection.Range.StartPos, rtfLogic.Selection.Range.EndPos)
      rngTemp.StartOf reLine
      rngTemp.EndOf reLine, True
      ' if this range of text is not in a quote or comment
      If CheckLine(rngTemp.Text) = 0 Then
        'only increment arg count if a comma was pressed
        If KeyAscii = 44 Then
          TipCurArg = TipCurArg + 1
        End If
        'rebuild the tip line
        ShowCmdTip TipCmdNum, TipCurArg
      End If
      Set rngTemp = Nothing
    End If
    
  Case 41 ' )
    'always cancel tip if visible
    If picTip.Visible Then
      'if not in quote
      Set rngTemp = rtfLogic.Range(rtfLogic.Selection.Range.StartPos, rtfLogic.Selection.Range.EndPos)
      rngTemp.StartOf reLine
      rngTemp.EndOf reLine, True
      ' if this range of text is not in a quote or comment
      If CheckLine(rngTemp.Text) = 0 Then
        picTip.Visible = False
      End If
      Set rngTemp = Nothing
    End If
    
  Case 35 '#'
    'if using snippets
    If Settings.Snippets Then
      'check for a snippet
      
      'extract the current line, up to cursor
      Set rngTemp = rtfLogic.Range(rtfLogic.Selection.Range.StartPos, rtfLogic.Selection.Range.EndPos)
      rngTemp.StartOf reLine, True
      lngLineStart = rngTemp.StartPos
      strLine = rngTemp.Text
      Set rngTemp = Nothing
      
      'count spaces before first hashtag (if not a valid
      ' snippet, the result in meaningless, so it doesn't
      ' hurt to do the count now
      SnipIndent = InStr(1, strLine, "#") - 1
      'then trim the string
      strLine = Trim(strLine)
      
      'if nothing
      If Len(strLine) = 0 Then
        'just exit
        Exit Sub
      End If
      
      'unless this is within a string (inside a snippet argument value)
      If CheckLine(strLine) = 0 Then
        'find preceding hashtag
        lngPos = InStrRev(strLine, "#")
        ' if a preceding hashtag found, this is a potential snippet
        If lngPos > 0 Then
          'trim off leading hashtag
          strLine = Trim(Right(strLine, Len(strLine) - lngPos))
          
          'if at least one char
          If Len(strLine) > 0 Then
            'check for snippet entry
            If CheckSnippet(strLine, SnipIndent) Then
              'replace line
              rtfLogic.Selection.Range.StartPos = lngLineStart + SnipIndent + lngPos - 1
              rtfLogic.Selection.Range.Text = strLine
              rtfLogic.Selection.Range.StartPos = rtfLogic.Selection.Range.StartPos + Len(strLine)
              KeyAscii = 0
            End If
          End If
        End If
      End If
    End If
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub ShowCmdTip(ByVal CmdIndex As Long, ByVal ArgIndex As Long)

  Dim strArgs() As String
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  With picTip
    'disable redraw to reduce flicker
    SendMessage .hWnd, WM_SETREDRAW, 0, 0
    
    .Cls
    .FontBold = False
    
    'split into array to get different args
    strArgs = Split(LoadResString(5000 + CmdIndex), ",")
    
    'set intial width to include the parentheses and some space on each end
    .Width = .TextWidth(" ()  ")
    ' add opening parenthesis
    picTip.Print " (";
    
    'if first arg is selected
    If ArgIndex = 0 Then
      picTip.FontBold = True
    End If
    
    'add first arg
    .Width = .Width + .TextWidth(strArgs(0))
    picTip.Print strArgs(0);
  
    'reset bold
    picTip.FontBold = False
    
    'add rest of args
    For i = 1 To UBound(strArgs())
      'add comma
      .Width = .Width + .TextWidth(",")
      picTip.Print ",";
      
      'if this arg is currently selected
      If i = ArgIndex Then
        picTip.FontBold = True
      End If
      
      'add the arg
      .Width = .Width + .TextWidth(strArgs(i))
      picTip.Print strArgs(i);
      
      'reset bold
      .FontBold = False
    Next i
    
    'add closing parentheses
    picTip.Print ")"
  
    'reenable updating
    SendMessage .hWnd, WM_SETREDRAW, 1, 0
    .Refresh
  End With
  
  ' also refresh the editor; if the tip window changes size slightly
  ' a ghost image will sometimes remain unless the editor is refreshed
  rtfLogic.Refresh
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub



Private Sub rtfLogic_KeyUp(KeyCode As Integer, Shift As Integer)

  On Error GoTo ErrHandler

  'always hide define window
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  Select Case KeyCode
  Case vbKeyUp
    'for now, just exit on keyup/keydown
    If picTip.Visible Then
      picTip.Visible = False
    End If
    
  Case vbKeyDown
    'ensure tip is hidden
    If picTip.Visible Then
      picTip.Visible = False
    End If
    
  Case vbKeyRight, vbKeyLeft
    'do we still need tip?
    If picTip.Visible Then
      If rtfLogic.Selection.Range.StartPos <= TipCmdPos Then
        picTip.Visible = False
      Else
        UpdateTip
      End If
    End If
  End Select

  UpdateStatusBar
Exit Sub

ErrHandler:
  Debug.Assert False
  Resume Next
End Sub

Private Sub rtfLogic_LostFocus()

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
End Sub

Private Sub rtfLogic_MouseDown(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)
  
  Dim rtn As Long, lngPos As Long

  On Error GoTo ErrHandler

  'popup menus are VERY problematic...
  ' since they belong to frmMDIMain it
  'means that form gets focus, even can
  'cause mouseup/click events on the
  'form if it happens on the menu border
  'mouse-up on rtfLogic never happens
  
  'need to track mouse down status; it's
  'used to minimize calls to UpdateStatus
  'and SetEditMenu
  MouseDown = True
  
  'convert click point to character position
  pPos.X = X
  pPos.Y = Y
  lngPos = SendMessagePtL(rtfLogic.hWnd, EM_CHARFROMPOS, 0, pPos)
  
  'if click results in a change in selection
  If lngPos < rtfLogic.Selection.Range.StartPos Or lngPos > rtfLogic.Selection.Range.EndPos Then
    FindForm.ResetSearch
  End If

  'position selection marker if coming from a find form
  If FixSel Then
    'update selection
    If lngPos < rtfLogic.Selection.Range.StartPos Or lngPos > rtfLogic.Selection.Range.EndPos Then
      rtfLogic.Selection.Range.StartPos = lngPos
      rtfLogic.Selection.Range.EndPos = lngPos
      FixSel = False
    End If
  End If
  
  'intercept rightclick; the ShowContextMenu event
  'interferes with undo/redo, so it can't be used
  
  'when right button is clicked, show edit menu
  If Button = vbRightButton Then
    ' if position of cursor is not within current selection
    If lngPos < rtfLogic.Selection.Range.StartPos Or lngPos > rtfLogic.Selection.Range.EndPos Then
      'move cursor to this point, which results in a statusbar update
      ' and also updates the edit menu
      rtfLogic.Selection.Range.EndPos = lngPos
      rtfLogic.Selection.Range.StartPos = lngPos
    End If

    'rtf editor doesn't do a mouse up when right clicking
    MouseDown = False
    
    'verify menu is up-to-date
    SetEditMenu
    
    'if activeform is NOT this form
    If Not frmMDIMain.ActiveForm Is Me Then
      'call menu update BEFORE popup
      AdjustMenus rtLogic, InGame, True, rtfLogic.Dirty
    End If

    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    DoEvents
    '(I tried Me.SetFocus; that doesn't work; not sure if there's
    'any other way in code to force the form to be active BEFORE
    'calling a popup menu
    'NOTE that can't use SafeDoEvents here; stupid rtfEd control...
    '  SafeDoEvents

    'disable tips while showing the list
    tmrTip.Enabled = False

    PopupMenu frmMDIMain.mnuEdit
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub rtfLogic_MouseMove(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

  On Error GoTo ErrHandler

  If OldMouseX = X And OldMouseY = Y Then
    Exit Sub
  End If

  OldMouseX = X
  OldMouseY = Y

  'reset the tips timer
  'first, always diable it to force it to reset
  tmrTip.Enabled = False
  'then enable it ONLY if no buttons/keys pressed, and user wants tips and defines list is not visible
  tmrTip.Enabled = (Button = 0 And Shift = 0 And Settings.ShowDefTips And Not lstDefines.Visible)
  picDefine.Visible = False
  
  ' if moving while mouse button is pressed AND something selected, always hide tip
  If Button <> 0 And rtfLogic.Selection.Range.Length > 0 Then
    picTip.Visible = False
  End If

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub rtfLogic_MouseUp(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As Range)

  'if click location is still inside a command with tip showing
  'we can leave it; just need to update it for correct argument
  Dim lngPrevArg As Long, lngPrevPos As Long

  On Error GoTo ErrHandler

  'always reset mousedown flag
  MouseDown = False
  ' and dblclick flag
  blnDblClick = False
  
  'ensure define is hidden
  picDefine.Visible = False

  'if tip is showing, determine if it should stay visible
  ' (no need to check for AutoQuickInfo setting; if tip is visible
  ' the setting is TRUE)
  If picTip.Visible Then
    'need to know current cmdpos and argpos to be able
    'to determine if they have changed
    lngPrevArg = TipCurArg
    lngPrevPos = TipCmdPos

    'force the check to find cmdpos and argpos
    If CheckQuickInfo(False, True) Then
      'as long as cmdpos is the same, keep showing the tip window
      If lngPrevPos = TipCmdPos Then
        'but update it if arg Value has changed
        If lngPrevArg <> TipCurArg Then
          'need to reset curarg Value so updatetip will redraw the tip
          TipCurArg = lngPrevArg
          UpdateTip
        End If
      Else
        'hide it if cmdpos is different
        picTip.Visible = False
      End If
    Else
      'hide it if no longer on a cmd
      picTip.Visible = False
    End If
  End If

  'if selection is >0 need to update status bar
  If rtfLogic.Selection.Range.Length > 0 Then
    UpdateStatusBar
  Else
    'otherwise, just update edit menu
    SetEditMenu
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub rtfLogic_Scroll(ByVal Direction As RichEditAGI.reScrollDirection)

  'always hide tip and define windows
  If picTip.Visible Then
    picTip.Visible = False
  End If
  If picDefine.Visible Then
    picDefine.Visible = False
  End If
  
  'and reset the tip timer
  tmrTip.Enabled = False
  tmrTip.Enabled = Settings.ShowDefTips
End Sub

Private Sub rtfLogic_SelectionChanged()
  
  Dim rtn As Long
  
  On Error GoTo ErrHandler
  
  'if not visible
  If Not rtfLogic.Visible Then
    'means form isn't visible
    Exit Sub
  End If
  
  ' if defines list is still up
  If Me.lstDefines.Visible Then
    ' don't need to check for the stupid jump glitch
    Exit Sub
  End If
  
  'if loading,
  If mLoading Then
    'skip updating status bar
    Exit Sub
  End If
  
  'even though timer seems to fix the 'dbl-click-jump glitch
  ' we still check for it, and manually force things back to
  ' correct state if the glitch manages to come through
  ' despite the timer fix
  
  'check for jump due to glitch when dbl-clicking the defines list
  
  'if cursor is at  very beginning of logic
  If rtfLogic.Selection.Range.EndPos = 0 Then
    'check if a define was recently inserted
    If DefEndPos > 0 Then
      '*'Debug.Print "fixing the glitch"
      ' need to restore window and selection

      ' see if window jumped up to top
      rtn = SendMessage(rtfLogic.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
      ' it did - it always does...
      '*'Debug.Assert rtn = 0

      ' if actual top line should be something other than
      ' first line
      If DefTopLine <> rtn Then
        'scroll down so the correct top line is shown
        rtn = SendMessage(rtfLogic.hWnd, EM_LINESCROLL, 0, DefTopLine)
      End If

      'then move cursor (which recurses this stupid fix...
      rtfLogic.Selection.Range.EndPos = DefEndPos
      rtfLogic.Selection.Range.StartPos = rtfLogic.Selection.Range.EndPos

      ' done fixing
      FixingGlitch = False
      DefEndPos = 0
    End If
  End If
  
  With rtfLogic.Selection.Range
    'if selected something by dbl-click
    If .Length > 0 And blnDblClick Then
      'reset dblclick so it doesn't recurse
      blnDblClick = False
      
      'then expand, if an agi command with a dot is selected
      ExpandSelection rtfLogic, False, True
    End If
  End With
  
  'always reset dbl-click
  blnDblClick = False
  
  'reset defines list endpos
  DefEndPos = 0
  
  'if something selected
  If rtfLogic.Selection.Range.Length > 0 Then
    ' and mouse button is down
    If MouseDown Then
      UpdateStatusBar
    End If
  Else
    UpdateStatusBar
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub tmrListDblClick_Timer()

  'popup menu with the defines list creates a glitch when
  'raised within the rtfLogic mousedown event; there are
  'additional events/messages occurring that need time
  'to clear.
  
  'best fix I can find is to use another control (picGlitch)
  'which gets the focus (due to Taborder) after the listbox
  'gets dismissed (by setting visible property to false)
  
  'then this timer is needed to let enough time to pass for
  'whatever events/messages are causing the trouble to clear
  'out; then focus can be sent back to rtfLogic
  
  ' this timer seems to help by passing focus back to
  ' the editor after a ~200msec delay
  
  ' ALWAYS disable the timer
  tmrListDblClick.Enabled = False
  
  'shift focus back to the editor
  rtfLogic.SetFocus

End Sub

Private Sub tmrListDef_Timer()
  
  'pointer not moving; if over a token, show the define
  Dim rtn As Long, ptPos As POINTAPI, ptOffset As POINTAPI
  
  Dim strDefine As String, lngPos As Long
  Dim tmpX As Long, tmpY As Long
  Dim tgtWnd As Long
  Dim blnFound As Boolean
  
  On Error GoTo ErrHandler
  
  'always turn off timer so we don't recurse
  tmrListDef.Enabled = False
  
  'if this form is not active, just exit
  If Not frmMDIMain.ActiveForm Is Me Then
    Exit Sub
  End If
  
  'no tips for words
  If lstDefines.Tag = "words" Then
    Exit Sub
  End If
  
  'get screen coords of cursor
  rtn = GetCursorPos(ptPos)
  'save to calculate the offset
  ptOffset = ptPos
  
  'if showing defines list, assign tip text
  'when enough time has passed
  If lstDefines.Visible Then
    'if over an item, set the tooltip
    rtn = WindowFromPoint(ptPos.X, ptPos.Y)
    
    If rtn = lstDefines.hWnd And Len(DefTip) > 0 Then
      With picTip
        .Width = .TextWidth(DefTip) + .TextWidth("e")
        .Height = .TextHeight(DefTip) * 1.1
        .Cls
        picTip.Print " "; DefTip;
        .Visible = True
      End With
    End If
    Exit Sub
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub tmrTip_Timer()

  'this function  displays the definition of a token when the
  'user hovers the cursor over it
  
  'pointer not moving; if over a token, show the define
  Dim strDefine As String, lngPos As Long
  Dim tmpX As Long, tmpY As Long, Max As Long
  Dim rtn As Long, ptPos As POINTAPI, ptOffset As POINTAPI
  Dim tgtWnd As Long
  Dim blnFound As Boolean
  
  On Error GoTo ErrHandler
  
  'always turn off timer so we don't recurse
  tmrTip.Enabled = False
  
  ' for now, let's not disable the tool tip timer
  ' when form doesn't have focus
'''  'if this form is not active, just exit
'''  If Not frmMDIMain.ActiveForm Is Me Then
'''    Exit Sub
'''  End If
  
  'get screen coords of cursor
  rtn = GetCursorPos(ptPos)
  'save to calculate the offset
  ptOffset = ptPos
  
  'convert to rtfLogic reference
  rtn = ScreenToClient(rtfLogic.hWnd, ptPos)
  'now we can calculate the offset to easily convert to/from screen and rtf window coordinates!
  ptOffset.X = ptOffset.X - ptPos.X
  ptOffset.Y = ptOffset.Y - ptPos.Y
  
  'if not over the rtfbox, just exit
  If ptPos.X < 0 Or ptPos.Y < 0 Or ptPos.X > rtfLogic.ScaleWidth / ScreenTWIPSX Or ptPos.Y > rtfLogic.ScaleHeight / ScreenTWIPSY Then
    Exit Sub
  End If
  
  'get the position of character which cursor is over
  lngPos = rtfLogic.RangeFromPoint(ptPos.X, ptPos.Y).StartPos
  
  'and then get the real bottom/center coords of the character at this position
  rtfLogic.Range(lngPos).GetPoint tmpX, tmpY, rePosStart + rePosCenter + rePosBottom
  tmpX = tmpX - ptOffset.X
  tmpY = tmpY - ptOffset.Y
  
  'how do i know if this is the char under cursor?
  'if difference between xpos of center char is too far from center of
  If Abs(tmpX - ptPos.X) > 10 Then
    Exit Sub
  End If
  
  'get the token under the cursor
  strDefine = GetCursorToken(lngPos)
  
  'if no valid token found, just exit
  If Len(strDefine) = 0 Or lngPos < 0 Then
    Exit Sub
  End If
  
  'rebuild the lookup list, if has recently changed
  If DefDirty Then
    BuildLDefLookup
    DefDirty = False
  End If
  
  'is it a define value?
  Do
    'check locals first
    Max = UBound(LDefLookup())
    For rtn = 0 To Max
      If StrComp(strDefine, LDefLookup(rtn).Name, vbTextCompare) = 0 Then
        strDefine = strDefine & " = " & LDefLookup(rtn).Value
        blnFound = True
        Exit Do
      End If
    Next rtn
    
    'then check globals
    Max = UBound(GDefLookup())
    For rtn = 0 To Max
      If StrComp(strDefine, GDefLookup(rtn).Name, vbTextCompare) = 0 Then
        strDefine = strDefine & " = " & GDefLookup(rtn).Value
        blnFound = True
        Exit Do
      End If
    Next rtn
    
    'then ids; we will test logics, then views, then sounds, then pics
    'as that's the order that defines are most likely to be used
    Max = Logics.Max
    For rtn = 0 To Max
      'if type indicates invalid, skip it
      If IDefLookup(rtn).Type <> 11 Then
        If StrComp(strDefine, IDefLookup(rtn).Name, vbTextCompare) = 0 Then
          strDefine = strDefine & " = " & IDefLookup(rtn).Value
          blnFound = True
          Exit Do
        End If
      End If
    Next rtn
    
    Max = Views.Max
    For rtn = 0 To Max
      'if type indicates invalid, skip it
      If IDefLookup(rtn + 256).Type <> 11 Then
        If StrComp(strDefine, IDefLookup(rtn + 256).Name, vbTextCompare) = 0 Then
          strDefine = strDefine & " = " & IDefLookup(rtn + 256).Value
          blnFound = True
          Exit Do
        End If
      End If
    Next rtn
    
    Max = Sounds.Max
    For rtn = 0 To Max
      'if type indicates invalid, skip it
      If IDefLookup(rtn + 512).Type <> 11 Then
        If StrComp(strDefine, IDefLookup(rtn + 512).Name, vbTextCompare) = 0 Then
          strDefine = strDefine & " = " & IDefLookup(rtn + 512).Value
          blnFound = True
          Exit Do
        End If
      End If
    Next rtn
    
    Max = Pictures.Max
    For rtn = 0 To Max
      'if type indicates invalid, skip it
      If IDefLookup(rtn + 768).Type <> 11 Then
        If StrComp(strDefine, IDefLookup(rtn + 768).Name, vbTextCompare) = 0 Then
          strDefine = strDefine & " = " & IDefLookup(rtn + 768).Value
          blnFound = True
          Exit Do
        End If
      End If
    Next rtn
    
    'if still no match, check reserved defines
    Max = 94
    For rtn = 0 To Max
      If StrComp(strDefine, RDefLookup(rtn).Name, vbTextCompare) = 0 Then
        strDefine = strDefine & " = " & RDefLookup(rtn).Value
        blnFound = True
        Exit Do
      End If
    Next rtn
  Loop Until True
  
  If Not blnFound Then
    Exit Sub
  End If
  
  With picDefine
    .Cls
    'add command
    .Width = .TextWidth(strDefine & "   ")
    picDefine.Print " "; strDefine;
    'reposition it
    .Top = tmpY + rtfLogic.Top + .Height * 0.1
    'if top is below bottom edge,
    If .Top > rtfLogic.Height + rtfLogic.Top - .Height Then
      .Top = .Top - 2 * Me.TextHeight("Ay") - 4
    End If
    .Left = tmpX + rtfLogic.Left
    .Visible = True
  End With
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)

  On Error GoTo ErrHandler
  
  Select Case Button.Key
  Case "cut"
    MenuClickCut
    
  Case "copy"
    MenuClickCopy
    
  Case "paste"
    MenuClickPaste
    
  Case "delete"
    MenuClickDelete
    
  Case "undo"
    MenuClickUndo
    
  Case "find"
    MenuClickFind
    
  Case "compile"
    MenuClickCustom1
    
  Case "msg"
    MenuClickCustom2
  
  Case "redo"
    MenuClickRedo
    
  Case "comment"
    rtfLogic.Selection.Range.Comment
  
  Case "uncomment"
    rtfLogic.Selection.Range.Uncomment
    
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


     */
    }
  }
}
