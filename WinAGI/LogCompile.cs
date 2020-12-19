using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.WinAGI;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;

namespace WinAGI
{
  public static partial class WinAGI
  {
    /*

  Option Explicit
  'set compare option to text since the compiler
  'is not case sensitive
  Option Compare Text
  'EXCEPT strings in messages; they ARE case sensitive
  'need to consider that in the getmsg function!!!!

  Private tmpLogRes As AGIResource

  Private mbytData() As Byte

  Private Type LogicGoto
    LabelNum  As Byte
    DataLoc As Integer
  End Type

  Public Const MAX_BLOCK_DEPTH = 64 'used by LogDecode functions too
  Private Const MAX_LABELS = 255
  Private Const UseTypeChecking = True

  Private Const NOT_TOKEN As String = "!"
  Private Const OR_TOKEN As String = "||"
  Private Const AND_TOKEN As String = "&&"
  Private Const NOTEQUAL_TOKEN As String = "!="
  Private Const EQUAL_TOKEN = "=="
  Private Const CONST_TOKEN As String = "#define "
  Private Const MSG_TOKEN As String = "#message"
  Private Const CMT1_TOKEN As String = "["
  Private Const CMT2_TOKEN = "//"

  Private Type LogicLabel
    Name  As String
    Loc As Integer
  End Type

  Private bytLogComp As Byte
  Private strLogCompID As String
  Private blnError As Boolean, lngQuoteAdded As Long
  Private strErrMsg As String
  Private strModule As String, strModFileName As String
  Private lngErrLine As Long
  Private intCtlCount As Long
  Private blnNewRoom As Boolean

  Private strIncludeFile() As String
  Private lngIncludeOffset As Long 'to correct line number due to added include lines
  Private stlInput As StringList   'the entire text to be compiled; includes the
                                    'original logic text, includes, and defines

  Private lngLine As Long
  Private lngPos As Long
  Private strCurrentLine As String

  Private strMsg(255) As String
  Private blnMsg(255) As Boolean
  Private intMsgWarn(255) As Integer 'to track warnings found during msgread function

  Private llLabel(MAX_LABELS) As LogicLabel
  Private bytLabelCount As Byte

  Private tdDefines() As TDefine
  Private lngDefineCount As Long
    */
    internal static bool blnSetIDs;
/*
  Private strLogID() As String
  Private strPicID() As String
  Private strSndID() As String
  Private strViewID() As String

Private Function ArgTypeName(ByVal ArgType As ArgTypeEnum) As String

  Select Case ArgType
  Case atNum       'i.e. numeric Value
    ArgTypeName = "number"
  Case atVar       'v##
    ArgTypeName = "variable"
  Case atFlag      'f##
    ArgTypeName = "flag"
  Case atMsg       'm##
    ArgTypeName = "message"
  Case atSObj      'o##
    ArgTypeName = "screen object"
  Case atIObj      'i##
    ArgTypeName = "inventory item"
  Case atStr       's##
    ArgTypeName = "string"
  Case atWord      'w## -- word argument (that user types in)
    ArgTypeName = "word"
  Case atCtrl      'c##
    ArgTypeName = "controller"
  Case atDefStr    'defined string; could be msg, inv obj, or vocword
    ArgTypeName = "text in quotes"
  Case atVocWrd    'vocabulary word; NOT word argument
    ArgTypeName = "vocabulary word"
  End Select
End Function

Private Sub CheckResFlagUse(ByVal ArgVal As Byte)

  'if error level is low, don't do anything
  If agMainLogSettings.ErrorLevel = leLow Then
    Exit Sub
  End If
  
  Select Case ArgVal
  Case 2, 4, 7, 8, 9, 10, Is >= 13
    'f2 = haveInput
    'f4 = haveMatch
    'f7 = script_buffer_blocked
    'f8 = joystick sensitivity set
    'f9 = sound_on
    'f10 = trace_abled
    'f13 = inventory_select_enabled
    'f14 = menu_enabled
    'f15 = windows_remain
    'f20 = auto_restart
    'no restrictions
    
  Case Else 'all other reserved variables should be read only
    AddWarning 5025, Replace(LoadResString(5025), ARG1, agResFlag(ArgVal).Name)
  End Select
End Sub
Private Sub CheckResVarUse(ByVal ArgNum As Byte, ByVal ArgVal As Byte)

  'if error level is low, don't do anything
  If agMainLogSettings.ErrorLevel = leLow Then
    Exit Sub
  End If
  
  Select Case ArgNum
  Case Is >= 27, 21, 15, 7, 3
    'no restrictions for
    '  all non restricted variables (>=27)
    '  curent score (v3)
    '  max score (v7)
    '  joystick sensitivity (v15)
    '  msg window delay time
    
  Case 6 'ego direction
    'should be restricted to values 0-8
    If ArgVal > 8 Then
      AddWarning 5018, Replace(Replace(LoadResString(5018), ARG1, agResVar(6).Name), ARG2, "8")
    End If
    
  Case 10 'cycle delay time
    'large values highly unusual
    If ArgVal > 20 Then
      AddWarning 5055
    End If
    
  Case 23 'sound attenuation
    'restrict to 0-15
    If ArgVal > 15 Then
      AddWarning 5018, Replace(Replace(LoadResString(5018), ARG1, agResVar(23).Name), ARG2, "15")
    End If
    
  Case 24 'max input length
    If ArgVal > 39 Then
      AddWarning 5018, Replace(Replace(LoadResString(5018), ARG1, agResVar(24).Name), ARG2, "39")
    End If
    
  Case 17, 18 'error value, and error info
    'resetting to zero is usually a good thing; other values don't make sense
    If ArgVal > 0 Then
      AddWarning 5092, Replace(LoadResString(5092), ARG1, agResVar(ArgNum).Name)
    End If
  
  Case 19 'key_pressed value
    'ok if resetting for key input
    If ArgVal > 0 Then
      AddWarning 5017, Replace(LoadResString(5017), ARG1, agResVar(ArgNum).Name)
    End If
    
  Case Else 'all other reserved variables should be read only
    AddWarning 5017, Replace(LoadResString(5017), ARG1, agResVar(ArgNum).Name)
  End Select
End Sub


Public Sub CompileLogic(SourceLogic As AGILogic)
  'this function compiles the sourcetext that is passed
  'the function returns a Value of true if successful; it returns false
  'and sets information about the error if an error in the source text is found
  
  'note that when errors are returned, line is adjusted because
  'editor rows(lines) start at '1', but the compiler starts at line '0'
  
  Dim blnCompiled As Boolean
  Dim stlSource As StringList
  Dim dtFileMod As Date
  Dim strInput As String
  
  'set error info to success as default
  blnError = False
  lngErrLine = -1
  strErrMsg = vbNullString
  strModule = vbNullString
  strModFileName = vbNullString
  intCtlCount = 0
  
  On Error Resume Next
  'initialize global defines
  'get datemodified property
  dtFileMod = FileLastMod(agGameDir & "globals.txt")
  If CRC32(StrConv(CStr(dtFileMod), vbFromUnicode)) <> agGlobalCRC Then
    GetGlobalDefines
  End If
  
  'if ids not set yet
  If Not blnSetIDs Then
    SetResourceIDs
  End If
  
  On Error GoTo ErrHandler
  
  'insert current values for reserved defines that can change values
  'agResDef(0).Value = "ego"  'this one doesn't change
  agResDef(1).Value = QUOTECHAR & agMainGame.GameVersion & QUOTECHAR
  agResDef(2).Value = QUOTECHAR & agMainGame.GameAbout & QUOTECHAR
  agResDef(3).Value = QUOTECHAR & agMainGame.GameID & QUOTECHAR
  '*'Debug.Assert agInvObj.Loaded
  If agInvObj.Loaded Then
    'Count of ACTUAL useable objects is one less than inventory object Count
    'because the first object ('?') is just a placeholder
    agResDef(4).Value = agInvObj.Count - 1
  Else
    agResDef(4).Value = -1
  End If
  
  'convert back to correct byte values
  strInput = StrConv(ExtCharToByte(SourceLogic.SourceText), vbUnicode)
  'assign to source stringlist
  Set stlSource = New StringList
  stlSource.Assign strInput
    
  bytLogComp = SourceLogic.Number
  strLogCompID = SourceLogic.ID
  
  'reset error info
  lngErrLine = -1
  strErrMsg = vbNullString
  strModule = vbNullString
  strModFileName = vbNullString
  
  'add include files (extchars handled automatically)
  If Not AddIncludes(stlSource) Then
    'dereference objects
    Set stlInput = Nothing
    'return error
    On Error GoTo 0
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
  
  'remove any blank lines from end
  Do Until Len(stlInput(stlInput.Count - 1)) <> 0 Or stlInput.Count = 0
    stlInput.Delete stlInput.Count - 1
  Loop
  
  'if nothing to compile, throw an error
  If stlInput.Count = 0 Then
    'dereference objects
    Set stlInput = Nothing
    'return error
    strErrMsg = LoadResString(4159)
    lngErrLine = 0
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
    
  'strip out all comments
  If Not RemoveComments() Then
    'dereference objects
    Set stlInput = Nothing
    'return error
    On Error GoTo 0
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
  
  'read labels
  If Not ReadLabels() Then
    'dereference objects
    Set stlInput = Nothing
    'return error
    On Error GoTo 0
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
  
  'enumerate and replace all the defines
  If Not ReadDefines() Then
    'dereference objects
    Set stlInput = Nothing
    'return error
    On Error GoTo 0
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
  
  'read predefined messages
  If Not ReadMsgs() Then
    'dereference objects
    Set stlInput = Nothing
    'return error
    On Error GoTo 0
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
  
  'assign temporary resource object
  Set tmpLogRes = New AGIResource
  tmpLogRes.NewResource
  
  'write a word as a place holder for offset to msg section start
  tmpLogRes.WriteWord 0, 0
  
  'use agi compiler
  blnCompiled = CompileAGI()
  
  'compile commands
  If Not blnCompiled Then
    'dereference objects
    tmpLogRes.Unload
    Set tmpLogRes = Nothing
    Set stlInput = Nothing
    'return error
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
  
  'write message section
  If Not WriteMsgs() Then
    'dereference objects
    tmpLogRes.Unload
    Set tmpLogRes = Nothing
    Set stlInput = Nothing
    'return error
    On Error GoTo 0
    On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
    Exit Sub
  End If
  
  With SourceLogic
    'assign resource data
    .Resource.AllData = tmpLogRes.AllData
  
    'update compiled crc
    SourceLogic.CompiledCRC = SourceLogic.CRC
    ' and write the new crc values to property file
    WriteGameSetting "Logic" & CStr(.Number), "CRC32", "&H" & Hex$(.CRC), "Logics"
    WriteGameSetting "Logic" & CStr(.Number), "CompCRC32", "&H" & Hex$(.CompiledCRC)
  End With
  
  'dereference objects
  tmpLogRes.Unload
  Set tmpLogRes = Nothing
  Set stlInput = Nothing
  
Exit Sub

ErrHandler:
  'if error is an app specific error, just pass it along; otherwise create
  'an app specific error to encapsulate whatever happened
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  'dereference objects
  tmpLogRes.Unload
  Set tmpLogRes = Nothing
  Set stlInput = Nothing
  If (lngError And vbObjectError) = vbObjectError Then
    'pass it along
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
  Else
    'return error
    On Error GoTo 0
    On Error GoTo 0: Err.Raise vbObjectError + 634, "LogCompile", "-1||" & Replace(LoadResString(591), ARG1, CStr(lngError) & ":" & strError)
  End If
End Sub


Public Function ConvertArgument(ByRef strArgIn As String, ByVal ArgType As ArgTypeEnum, Optional ByRef blnVarOrNum As Boolean = False) As Boolean
  'if input is not a system argument already
  '(i.e. ##, v##, f##, s##, o##, w##, i##, c##)
  'this function searches resource IDs, local defines, global defines,
  'and reserved names for strArgIn; if found
  'strArgIn is replaced with the Value of the define
  'optional argtype is used to identify words, messages, and inv objects
  'to speed up search
  
  'NOTE: this does NOT validate the numerical Value of arguments;
  'calling function is responsible to make that check
  'it also does not concatenate strings
  
  'to support calls from special syntax compilers, need to be able
  'to check for numbers AND variables with one check
  'the blnVarOrNum flag is used to do this; when the flag is
  'true, number searches also return variables
  
  Dim i As Long
  Dim intAsc As Integer
  
  On Error GoTo ErrHandler
  
  'check if already in correct format
  Select Case ArgType
  Case atNum  'numeric only
    If IsNumeric(strArgIn) Then
      ConvertArgument = True
      'reset VarOrNum flag
      blnVarOrNum = False
      Exit Function
    End If
    'unless looking for var or num
    If blnVarOrNum Then
      'then 'v##' is ok
      If (AscW(strArgIn) Or 32) = 118 Then
        If VariableValue(strArgIn) <> -1 Then
          ConvertArgument = True
          Exit Function
        End If
      End If
    End If
  
  Case atVar
    'if first char matches
    If (AscW(strArgIn) Or 32) = 118 Then
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    End If
    
  Case atFlag
    'if first char matches
    If (AscW(strArgIn) Or 32) = 102 Then
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    End If
    
  Case atCtrl
    'if first char matches
    If (AscW(strArgIn) Or 32) = 99 Then
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    End If
    
  Case atSObj
    'if first char matches
    If (AscW(strArgIn) Or 32) = 111 Then
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    End If
    
  Case atStr
    'if first char matches
    If (AscW(strArgIn) Or 32) = 115 Then
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    End If
    
  Case atWord 'NOTE: this is NOT vocab word; this is word arg type (used in command word.to.string)
    'if first char matches
    If (AscW(strArgIn) Or 32) = 119 Then
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    End If
    
  Case atMsg
    'if first char matches, or is a quote
    intAsc = AscW(strArgIn)
    Select Case intAsc
    Case 77, 109
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    Case 34
      'strings are always ok
      ConvertArgument = True
      Exit Function
    End Select
    
  Case atIObj
    'if first char matches, or is a quote
    intAsc = AscW(strArgIn)
    Select Case intAsc
    Case 73, 105
      'if this arg returns a valid Value
      If VariableValue(strArgIn) <> -1 Then
        'ok
        ConvertArgument = True
        Exit Function
      End If
    Case 34
      'strings are always ok
      ConvertArgument = True
      Exit Function
    End Select
    
  Case atVocWrd
    'can be number or string in quotes
    If IsNumeric(strArgIn) Or AscW(strArgIn) = 34 Then
      'ok
      ConvertArgument = True
      Exit Function
    End If
  End Select
  
  'arg is not in correct format; must be reserved name, global or local define, or an error
  
  'first, check against local defines
  For i = 0 To lngDefineCount - 1
    If strArgIn = tdDefines(i).Name Then
      'match found; check that Value is correct type
      Select Case ArgType
      Case atNum
        'check for number
        If IsNumeric(tdDefines(i).Value) Then
          'reset VarOrNum flag
          blnVarOrNum = False
          ConvertArgument = True
          strArgIn = tdDefines(i).Value
          Exit Function
        End If
        
        'if checking for variables
        If blnVarOrNum Then
          If (AscW(tdDefines(i).Value) Or 32) = 118 Then
            'if this define returns a valid Value
            If VariableValue(tdDefines(i).Value) <> -1 Then
              'ok
              strArgIn = tdDefines(i).Value
              ConvertArgument = True
            End If
          End If
        End If
        
      Case atVar
        'v## only
        If (AscW(tdDefines(i).Value) Or 32) = 118 Then
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        End If
        
      Case atFlag
        'f## only
        If (AscW(tdDefines(i).Value) Or 32) = 102 Then
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        End If
        
      Case atMsg
        'm## or a string
        intAsc = AscW(tdDefines(i).Value)
        Select Case intAsc
        Case 77, 109
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        
        Case 34
          strArgIn = tdDefines(i).Value
          ConvertArgument = True
        End Select
        
      Case atSObj
        'o## only
        If AscW(tdDefines(i).Value) = 111 Then
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        End If
        
      Case atIObj
        'i## or a string
        intAsc = AscW(tdDefines(i).Value)
        Select Case intAsc
        Case 73, 105
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        Case 34
          strArgIn = tdDefines(i).Value
          ConvertArgument = True
        End Select
        
      Case atStr
        's## only
        If (AscW(tdDefines(i).Value) Or 32) = 115 Then
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        End If
        
      Case atWord
        'w## only
        If (AscW(tdDefines(i).Value) Or 32) = 119 Then
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        End If
        
      Case atCtrl
        'c## only
        If (AscW(tdDefines(i).Value) Or 32) = 99 Then
          'if this define returns a valid Value
          If VariableValue(tdDefines(i).Value) <> -1 Then
            'ok
            strArgIn = tdDefines(i).Value
            ConvertArgument = True
          End If
        End If
        
      Case atVocWrd
        'numeric or string only
        If IsNumeric(tdDefines(i).Value) Then
          strArgIn = tdDefines(i).Value
          ConvertArgument = True
        ElseIf AscW(tdDefines(i).Value) = 34 Then
          strArgIn = tdDefines(i).Value
          ConvertArgument = True
        End If
        
      Case atDefStr
        'call to ConvertArgument is never made with type of atDefStr
      End Select
      'exit, regardless of result
      Exit Function
    End If
  Next i
  
  'second, check against global defines
  'for any type except vocab words
  If ArgType <> atVocWrd Then
    'check against this type of global defines
    For i = 0 To agGlobalCount - 1
      If agGlobal(i).Type = ArgType Then
        If strArgIn = agGlobal(i).Name Then
          strArgIn = agGlobal(i).Value
          'reset VarOrNum flag
          blnVarOrNum = False
          ConvertArgument = True
          Exit Function
        End If
      End If
    Next i
    'if checking var or num
    If blnVarOrNum Then
      'numbers were checked; need to check variables
      For i = 0 To agGlobalCount - 1
        If agGlobal(i).Type = atVar Then
          If strArgIn = agGlobal(i).Name Then
            strArgIn = agGlobal(i).Value
            ConvertArgument = True
            Exit Function
          End If
        End If
      Next i
    End If
  Else
    'check vocab words only against numbers
    For i = 0 To agGlobalCount - 1
      If agGlobal(i).Type = atNum Then
        If strArgIn = agGlobal(i).Name Then
          strArgIn = agGlobal(i).Value
          ConvertArgument = True
          Exit Function
        End If
      End If
    Next i
  End If
  
  'check messages, iobjs, and vocab words against global strings
  If (ArgType = atMsg) Or (ArgType = atIObj) Or (ArgType = atVocWrd) Then
    'check against global defines (string type)
    For i = 0 To agGlobalCount - 1
      If agGlobal(i).Type = atDefStr Then
        If strArgIn = agGlobal(i).Name Then
          strArgIn = agGlobal(i).Value
          ConvertArgument = True
          Exit Function
        End If
      End If
    Next i
  End If
  
  'third, check numbers against list of resource IDs
  If ArgType = atNum Then
    'check against resource IDs
    For i = 0 To 255
      'if this arg matches one of the resource ids
      If strArgIn = strLogID(i) Then
        strArgIn = CStr(i)
        'reset VarOrNum flag
        blnVarOrNum = False
        ConvertArgument = True
        Exit Function
      End If
      If strArgIn = strPicID(i) Then
        strArgIn = CStr(i)
        'reset VarOrNum flag
        blnVarOrNum = False
        ConvertArgument = True
        Exit Function
      End If
      If strArgIn = strSndID(i) Then
        strArgIn = CStr(i)
        'reset VarOrNum flag
        blnVarOrNum = False
        ConvertArgument = True
        Exit Function
      End If
      If strArgIn = strViewID(i) Then
        strArgIn = CStr(i)
        'reset VarOrNum flag
        blnVarOrNum = False
        ConvertArgument = True
        Exit Function
      End If
    Next i
  End If
  
  'lastly, if using reserved names,
  If agUseRes Then
    'last of all, check reserved names
    Select Case ArgType
    Case atNum
      For i = 0 To 4
        If strArgIn = agEdgeCodes(i).Name Then
          strArgIn = agEdgeCodes(i).Value
          'reset VarOrNum flag
          blnVarOrNum = False
          ConvertArgument = True
          Exit Function
        End If
      Next i
      For i = 0 To 8
        If strArgIn = agEgoDir(i).Name Then
          strArgIn = agEgoDir(i).Value
          'reset VarOrNum flag
          blnVarOrNum = False
          ConvertArgument = True
          Exit Function
        End If
      Next i
      For i = 0 To 4
        If strArgIn = agVideoMode(i).Name Then
          strArgIn = agVideoMode(i).Value
          'reset VarOrNum flag
          blnVarOrNum = False
          ConvertArgument = True
          Exit Function
        End If
      Next i
      For i = 0 To 8
        If strArgIn = agCompType(i).Name Then
          strArgIn = agCompType(i).Value
          'reset VarOrNum flag
          blnVarOrNum = False
          ConvertArgument = True
          Exit Function
        End If
      Next i
      For i = 0 To 15
        If strArgIn = agResColor(i).Name Then
          strArgIn = agResColor(i).Value
          'reset VarOrNum flag
          blnVarOrNum = False
          ConvertArgument = True
          Exit Function
        End If
      Next i
      'check against invobj Count
      If strArgIn = agResDef(4).Name Then
        strArgIn = agResDef(4).Value
        'reset VarOrNum flag
        blnVarOrNum = False
        ConvertArgument = True
        Exit Function
      End If
      
      'if looking for numbers OR variables
      If blnVarOrNum Then
        'check against builtin variables as well
        For i = 0 To 26
          If strArgIn = agResVar(i).Name Then
            strArgIn = agResVar(i).Value
            ConvertArgument = True
            Exit Function
          End If
        Next i
      End If
      
    Case atVar
       For i = 0 To 26
        If strArgIn = agResVar(i).Name Then
          strArgIn = agResVar(i).Value
          ConvertArgument = True
          Exit Function
        End If
      Next i
      
    Case atFlag
       For i = 0 To 17
        If strArgIn = agResFlag(i).Name Then
          strArgIn = agResFlag(i).Value
          ConvertArgument = True
          Exit Function
        End If
      Next i
    Case atMsg
      For i = 1 To 3 'for gamever and gameabout and gameid
        If strArgIn = agResDef(i).Name Then
          strArgIn = agResDef(i).Value
          ConvertArgument = True
          Exit Function
        End If
      Next i
    Case atSObj
      If strArgIn = agResDef(0).Name Then
        strArgIn = agResDef(0).Value
        ConvertArgument = True
        Exit Function
      End If
    Case atStr
      If strArgIn = agResDef(5).Name Then
        strArgIn = agResDef(5).Value
        ConvertArgument = True
        Exit Function
      End If
    End Select
  End If
  
  'if not found or error, return false
ErrHandler:

  'just exit
End Function
Private Function GetNextArg(ByVal ArgType As ArgTypeEnum, ByVal ArgPos As Long, Optional ByRef blnVarOrNum As Boolean = False) As Long
  'this function retrieves the next argument and validates
  'that the argument is of the correct type
  'and has a valid Value
  'multline message/string/inv.item/word strings are recombined, and checked
  'for validity
  'if successful, the function returns the Value of the argument
  'if unsuccessful, the function sets the error flag and error msg
  '(in which case, the return Value is meaningless)
  
  'special syntax compilers look for variables OR strings;
  'when argtype is atNum and this flag is set, numbers OR
  'variables return true; the flag is set to TRUE if returned Value
  'is for a variable, to false if it is for a number
  
  Dim strArg As String, lngArg As Integer
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'get next command
  strArg = NextCommand()
  
  'convert it
  If Not ConvertArgument(strArg, ArgType, blnVarOrNum) Then
    'error
    blnError = True
    'if a closing paren found
    If strArg = ")" Then
      ' arg missing
      strErrMsg = Replace(Replace(LoadResString(4054), ARG1, CStr(ArgPos + 1)), ARG3, ArgTypeName(ArgType))
    Else
      'use 1-base arg values
      strErrMsg = Replace(Replace(Replace(LoadResString(4063), ARG1, CStr(ArgPos + 1)), ARG2, ArgTypeName(ArgType)), ARG3, strArg)
    End If
    Exit Function
  End If
  
  Select Case ArgType
  Case atNum  'number
    'verify type is number
    If Not IsNumeric(strArg) Then
      'if NOT catching variables too
      If Not blnVarOrNum Then
        blnError = True
        strErrMsg = Replace(LoadResString(4062), ARG1, CStr(ArgPos))
        Exit Function
      End If
    Else
      'return 'is NOT a variable'; ensure flag is reset
      blnVarOrNum = False
    End If
    'check for negative number
    If Val(strArg) < 0 Then
      'valid negative numbers are -1 to -128
      If Val(strArg) < -128 Then
        'error
        blnError = True
        strErrMsg = LoadResString(4157)
        Exit Function
      End If
      'convert it to 2s-compliment unsigned value by adding it to 256
      strArg = CStr(256 + Val(strArg))
      '*'Debug.Assert Val(strArg) >= 128 And Val(strArg) <= 255

      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        'show warning
        AddWarning 5098
      End Select
    
    End If
    'convert to number and validate
    lngArg = VariableValue(strArg)
    If lngArg = -1 Then
      blnError = True
      'use 1-based arg values
      strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
      Exit Function
    End If
  
  Case atVar, atFlag  'variable, flag
    'get Value
    lngArg = VariableValue(strArg)
    If lngArg = -1 Then
      blnError = True
      'use 1-based arg values
      strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
      Exit Function
    End If
  
  Case atCtrl    'controller
    'controllers should be  0 - 49
    'get Value
    lngArg = VariableValue(strArg)
    If lngArg = -1 Then
      blnError = True
      'if high errlevel
      If agMainLogSettings.ErrorLevel = leHigh Then
        'use 1-based arg values
        strErrMsg = Replace(LoadResString(4136), ARG1, CStr(ArgPos + 1))
      Else
        'use 1-based arg values
        strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
      End If
      Exit Function
    Else
      'if outside expected bounds (controllers should be limited to 0-49)
      If lngArg > 49 Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          'generate error
          blnError = True
          'use 1-based arg values
          strErrMsg = Replace(LoadResString(4136), ARG1, CStr(ArgPos + 1))
          Exit Function
          
        Case leMedium
          'generate warning
          AddWarning 5060
        End Select
      End If
    End If
  
  Case atSObj 'screen object
    'get Value
    lngArg = VariableValue(strArg)
    If lngArg = -1 Then
      blnError = True
      'use 1-based arg values
      strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
      Exit Function
    End If
    
    'check against max screen object Value
    If lngArg > agInvObj.MaxScreenObjects Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        'generate error
        blnError = True
        strErrMsg = Replace(LoadResString(4119), ARG1, CStr(agInvObj.MaxScreenObjects))
        Exit Function
        
      Case leMedium
        'generate warning
        AddWarning 5006, Replace(LoadResString(5006), ARG1, CStr(agInvObj.MaxScreenObjects))
      End Select
    End If

  Case atStr 'string
    'get Value
    lngArg = VariableValue(strArg)
    If lngArg = -1 Then
      blnError = True
      'if high errlevel
      If agMainLogSettings.ErrorLevel = leHigh Then
        'for version 2.089, 2.272, and 3.002149 only 12 strings
        Select Case agIntVersion
        Case "2.089", "2.272", "3.002149"
          'use 1-based arg values
          strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "11")
        Case Else
          strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "23")
        End Select
        
      Else
        'use 1-based arg values
        strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
      End If
      Exit Function
    Else
      'if outside expected bounds (strings should be limited to 0-23)
      If (lngArg > 23) Or (lngArg > 11 And (agIntVersion = "2.089" Or agIntVersion = "2.272" Or agIntVersion = "3.002149")) Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          'generate error
          blnError = True
          'for version 2.089, 2.272, and 3.002149 only 12 strings
          Select Case agIntVersion
          Case "2.089", "2.272", "3.002149"
            'use 1-based arg values
            strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "11")
          Case Else
            strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "23")
          End Select
          Exit Function
          
        Case leMedium
         'generate warning
         'for version 2.089, 2.272, and 3.002149 only 12 strings
          Select Case agIntVersion
          Case "2.089", "2.272", "3.002149"
            AddWarning 5007, Replace(LoadResString(5007), ARG1, "11")
          Case Else
            AddWarning 5007, Replace(LoadResString(5007), ARG1, "23")
          End Select
        End Select
      End If
    End If
    
  Case atWord 'word  (word type is NOT words from word.tok)
    'get Value
    lngArg = VariableValue(strArg)
    If lngArg = -1 Then
      blnError = True
      'if high error level
      If agMainLogSettings.ErrorLevel = leHigh Then
        'use 1-based arg values
        strErrMsg = Replace(LoadResString(4090), ARG1, CStr(ArgPos + 1))
      Else
        strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
      End If
      Exit Function
    Else
      'if outside expected bounds (words should be limited to 0-9)
      If lngArg > 9 Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          'generate error
          blnError = True
          'use 1-based arg values
          strErrMsg = Replace(LoadResString(4090), ARG1, CStr(ArgPos + 1))
          Exit Function
          
        Case leMedium
          'generate warning
          AddWarning 5008
        End Select
      End If
    End If
  
  Case atMsg  'message
    'returned arg is either m## or "msg"
    Select Case AscW(strArg)
    Case 109
      'validate Value
      lngArg = VariableValue(strArg)
      If lngArg = -1 Then
        blnError = True
        'use 1-based arg values
        strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
        Exit Function
      End If
      'm0 is not allowed
      If lngArg = 0 Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          blnError = True
          strErrMsg = LoadResString(4107)
          Exit Function
        Case leMedium
          'generate warning
          AddWarning 5091, Replace(LoadResString(5091), ARG1, CStr(lngArg))
          'make this a null msg
          blnMsg(lngArg) = True
          strMsg(lngArg) = vbNullString
        Case leLow
          'ignore; it will be handled when writing messages
        End Select
      End If
      
      'verify msg exists
      If Not blnMsg(lngArg) Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          blnError = True
          strErrMsg = Replace(LoadResString(4113), ARG1, CStr(lngArg))
          Exit Function
        Case leMedium
          'generate warning
          AddWarning 5090, Replace(LoadResString(5090), ARG1, CStr(lngArg))
          'make this a null msg
          blnMsg(lngArg) = True
          strMsg(lngArg) = vbNullString
        Case leLow
          'ignore; WinAGI adds a null value, so no error will occur
        End Select
      End If
    Case 34
      'concatenate, if applicable
      strArg = ConcatArg(strArg)
      If blnError Then
        'concatenation error; exit
        Exit Function
      End If
      
      'strip off quotes
      strArg = Mid$(strArg, 2, Len(strArg) - 2)
      'convert to msg number
      lngArg = MessageNum(strArg)
      
      'if unallowed characters found, error was raised; exit
      If lngArg = -1 Then
        blnError = True
        Exit Function
      End If
        
      'if valid number not found
      If lngArg = 0 Then
        blnError = True
        strErrMsg = LoadResString(4092)
        Exit Function
      End If
      
    End Select
    
  Case atIObj 'inventory object
    'only true restriction is can't exceed object count, and can't exceed 255 objects (0-254)
    'i0 is usually a '?', BUT not a strict requirement
    'HOWEVER, WinAGI enforces that i0 MUST be '?', and can't be changed
    'also, if any code tries to access an object by '?', return error
    
    'if character is inv obj arg type prefix
    Select Case AscW(strArg)
    Case 105
      'validate Value
      lngArg = VariableValue(strArg)
      If lngArg = -1 Then
        blnError = True
        'use 1-based arg values
        strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
        Exit Function
      End If
      
    Case 34
      'concatenate, if applicable
      strArg = ConcatArg(strArg)
      If blnError Then
        'concatenation error
        Exit Function
      End If
      
      'convert to inv obj number
      'first strip off starting and ending quotes
      strArg = Mid$(strArg, 2, Len(strArg) - 2)
      'if a quotation mark is part of an object name,
      'it is coded in the logic as a '\"' not just a '"'
      'need to ensure all '\"' codes are converted to '"'
      'otherwise the object would never match
      strArg = Replace(strArg, "\""", QUOTECHAR)
      
      'step through all object names
      For i = 0 To agResDef(4).Value
        'if this is the object
        If strArg = agInvObj(i).ItemName Then
          'return this Value
          lngArg = CByte(i)
          Exit For
        End If
      Next i
      
      'if not found,
      If i = agResDef(4).Value + 1 Then
        blnError = True
        'check for added quotes; they are the problem
        If lngQuoteAdded >= 0 Then
          'reset line;
          lngLine = lngQuoteAdded
          'string error
          strErrMsg = LoadResString(4051)
        Else
          'use 1-base arg values
          strErrMsg = Replace(LoadResString(4075), ARG1, CStr(ArgPos + 1))
        End If
        Exit Function
      End If
      
      'if object is not unique
      If Not agInvObj(lngArg).Unique Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          blnError = True
          'use 1-based arg values
          strErrMsg = Replace(LoadResString(4036), ARG1, CStr(ArgPos + 1))
          Exit Function
        Case leMedium
          'set warning
          AddWarning 5003, Replace(LoadResString(5003), ARG1, CStr(ArgPos + 1))
        Case leLow
          'no action
        End Select
      End If
    End Select
    
    'if object number exceeds current object Count,
    'If lngArg >= agInvObj.Count Then
    If lngArg > agResDef(4).Value Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        blnError = True
        'use 1-based arg values
        strErrMsg = Replace(LoadResString(4112), ARG1, CStr(ArgPos + 1))
        Exit Function
      Case leMedium
        'set warning
        'use 1-based arg values
        AddWarning 5005, Replace(LoadResString(5005), ARG1, CStr(ArgPos + 1))
      Case leLow
        'no action
      End Select
    Else
      'if object is a question mark, raise error/warning
      If agInvObj(lngArg).ItemName = "?" Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          blnError = True
          'use 1-based arg values
          strErrMsg = Replace(LoadResString(4111), ARG1, CStr(ArgPos + 1))
          Exit Function
        Case leMedium
          'set warning
          AddWarning 5004
        Case leLow
          'no action
        End Select
      End If
    End If
      
  Case atVocWrd
    'words can be ## or "word"
    If IsNumeric(strArg) Then
      lngArg = CLng(strArg)
      'make sure it's not a decimal
      If Val(strArg) <> lngArg Then
        blnError = True
        lngArg = -1
      Else
        'validate the group
        blnError = Not agVocabWords.GroupExists(lngArg)
      End If
    Else
      'this is a string; concatenate if applicable
      strArg = ConcatArg(strArg)
      If blnError Then
        'concatenation error
        Exit Function
      End If
      
      'convert to word number
      'first strip off starting and ending quotes
      strArg = Mid$(strArg, 2, Len(strArg) - 2)
      
      On Error Resume Next
      'get argument val by checking against word list
      If agVocabWords.WordExists(strArg) Then
        lngArg = agVocabWords(strArg).Group
      Else
        'RARE, but if it's an 'a' or 'i' that isn't defined,
        'it's word group 0
        If strArg = "i" Or strArg = "a" Or strArg = "I" Or strArg = "A" Then
          lngArg = 0
          'add warning
          Select Case agMainLogSettings.ErrorLevel
          Case leHigh, leMedium
            AddWarning 5108, Replace(LoadResString(5108), ARG1, strArg)
          End Select
        Else
          'set error flag
          blnError = True
          'set arg to invalid number
          lngArg = -1
        End If
      End If
    End If
    
    'now lngArg is a valid group number, unless blnError is set
    
    'if there is an error
    If blnError Then
      'if arg value=-1 OR high level,
      If agMainLogSettings.ErrorLevel = leHigh Or (lngArg = -1) Then
        'argument is already 1-based for said tests
        strErrMsg = Replace(LoadResString(4114), ARG1, strArg)
        Exit Function
      Else
        If agMainLogSettings.ErrorLevel = leMedium Then
          'set warning
          AddWarning 5019, Replace(LoadResString(5019), ARG1, strArg)
          blnError = False
        End If
      End If
    End If
    
    'check for group 0
    If lngArg = 0 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4035), ARG1, strArg)
        Exit Function
      Case leMedium
        AddWarning 5083, Replace(LoadResString(5083), ARG1, strArg)
      Case leLow
      End Select
    End If
    
  End Select
  
  'set return Value
  GetNextArg = lngArg
Exit Function

ErrHandler:
  '*'Debug.Assert False
  
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "GetNextArg")
  blnError = True
End Function


Private Sub IncrementLine()
  'increments the the current line of input being processed
  'sets all counters, pointers, etc
  'as well as info needed to support error locating
  
  'if at end of input (lngLine=-1)
  If lngLine = -1 Then
    'just exit
    Exit Sub
  End If
  
  'if compiler is reset
  If lngLine = -2 Then
    'set it to -1 so line 0 is returned
    lngLine = -1
  End If
  
  'increment line counter
  lngLine = lngLine + 1
  lngPos = 0
  'if at end,
  If lngLine = stlInput.Count Then
    lngLine = -1
    Exit Sub
  End If
  'check for include lines
  If Left$(stlInput(lngLine), 2) = "#I" Then
    lngIncludeOffset = lngIncludeOffset + 1
    'set module
    strModule = strIncludeFile(CInt(Mid$(stlInput(lngLine), 3, InStr(3, stlInput(lngLine), ":") - 3)))
    strModFileName = JustFileName(strModule)
    'set errline
    lngErrLine = CLng(Mid$(stlInput(lngLine), InStr(3, stlInput(lngLine), ":") + 1, InStr(3, stlInput(lngLine), "#") - 5))
    strCurrentLine = Right$(stlInput(lngLine), Len(stlInput(lngLine)) - InStr(2, stlInput(lngLine), "#"))
  Else
    strModule = vbNullString
    strModFileName = vbNullString
    lngErrLine = lngLine - lngIncludeOffset
    'set string
    strCurrentLine = stlInput(lngLine)
  End If
  
End Sub
Private Function NextChar(Optional ByVal blnNoNewLine As Boolean = False) As String
  'gets the next non-space character (tabs (ascii code H&9, are converted
  'to a space character, and ignored) from the input stream
  
  'if the NoNewLine flag is passed,
  'the function will not look past current line for next
  'character; if no character on current line,
  'lngPos is set to end of current line, and
  'empty string is returned
  On Error GoTo ErrHandler
  
  'if already at end of input (lngLine=-1)
  If lngLine = -1 Then
    'just exit
    NextChar = vbNullString
    Exit Function
  End If
  
  Do
    'first, increment position
    lngPos = lngPos + 1
    'if past end of this line,
    If lngPos > Len(strCurrentLine) Then
      'if can't get another line,
      If blnNoNewLine Then
        'move pointer back
        lngPos = lngPos - 1
        'return empty string
        NextChar = vbNullString
        Exit Function
      End If
      
      'get the next line
      IncrementLine
      'if at end of input
      If lngLine = -1 Then
        'exit with no character
        Exit Function
      End If
      'increment pointer(so it points to first character of line)
      lngPos = lngPos + 1
    End If
    
    NextChar = Mid$(strCurrentLine, lngPos, 1)
        
    'only characters <32 that we need to use are return, and linefeed
    If Len(NextChar) > 0 Then
      If Asc(NextChar) < 32 Then
        Select Case Asc(NextChar)
        Case 10, 13 'treat as returns?
          NextChar = vbCr
        Case Else
          NextChar = " "
        End Select
      End If
    End If
  Loop Until NextChar <> " " And LenB(NextChar) <> 0
Exit Function

ErrHandler:
  
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  'if error is an app specific error, just pass it along; otherwise create
  'an app specific error to encapsulate whatever happened
  If (lngError And vbObjectError) = vbObjectError Then
    'pass it along
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
  Else
    On Error GoTo 0: Err.Raise vbObjectError + 656, strErrSrc, Replace(LoadResString(656), ARG1, CStr(lngError) & ":" & strError)
  End If
End Function
Private Function NextCommand(Optional ByVal blnNoNewLine As Boolean = False) As String
  'this function will return the next command, which is comprised
  'of command elements, and separated by element separators
  'command elements include:
  '  characters a-z, A-Z, numbers 0-9, and:  #$%.@_
  '  (and also, all extended characters [128-255])
  '  NOTE: inside quotations, ALL characters, including spaces
  '  are considered command elements
  '
  'element separators include:
  '  space, !"&'()*+,-/:;<=>?[\]^`{|}~
  '
  'element separators other than space are normally returned
  'as a single character command; there are some exceptions
  'where element separators will include additional characters:
  '  !=, &&, *=, ++, +=, --, -=, /=, //, <=, <>, =<, ==, =>, >=, ><, ||
  '
  'when a command starts with a quote, the command returns
  'after a closing quote is found, regardless of characters
  'inbetween the quotes
  '
  'if end of input is reached it returns empty string
  
  Dim intCmdEnd As Integer
  Dim intChar As Integer
  Dim blnInQuotes As Boolean, blnSlash As Boolean
  
  'find next non-blank character
  NextCommand = NextChar(blnNoNewLine)
  'if at end of input,
  If lngLine = -1 Then
    'return empty string
    NextCommand = vbNullString
    Exit Function
  End If
  'if no character returned
  If LenB(NextCommand) = 0 Then
    Exit Function
  End If
  
  On Error GoTo ErrHandler

  'if command is a element separator:
  Select Case AscW(NextCommand)
  Case 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126 '  '(),:;?[\]^`{}~
    'return this single character as a command
    Exit Function
  Case 61 '=
    'special case; "=", "=<" and "=>" returned as separate commands
    Select Case Mid$(strCurrentLine, lngPos + 1, 1)
    Case "<", ">"
      'increment pointer
      lngPos = lngPos + 1
      'return the two byte cmd (swap so we get ">=" and "<="
      ' instead of "=>" and "=<"
      NextCommand = Mid$(strCurrentLine, lngPos, 1) & NextCommand
    
    Case "=" '"=="
      'increment pointer
      lngPos = lngPos + 1
      'return the two byte cmd
      NextCommand = "=="
    End Select
    Exit Function
  Case 34 '"
    'special case; quote means start of a string
    blnInQuotes = True
  Case 43 '+
    'special case; "+", "++" and "+=" returned as separate commands
    If Mid$(strCurrentLine, lngPos + 1, 1) = "+" Then
      'increment pointer
      lngPos = lngPos + 1
      'return shorthand increment
      NextCommand = "++"
    ElseIf Mid$(strCurrentLine, lngPos + 1, 1) = "=" Then
      lngPos = lngPos + 1
      'return shorthand addition
      NextCommand = "+="
    End If
    Exit Function
  Case 45 '-
    'special case; "-", "--" and "-=" returned as separate commands
    'also check for "-##"
    If Mid$(strCurrentLine, lngPos + 1, 1) = "-" Then
      'increment pointer
      lngPos = lngPos + 1
      'return shorthand decrement
      NextCommand = "--"
    ElseIf Mid$(strCurrentLine, lngPos + 1, 1) = "=" Then
      lngPos = lngPos + 1
      'return shorthand subtract
      NextCommand = "-="
    ElseIf Val(Mid$(strCurrentLine, lngPos + 1)) <> 0 Then
      'add the number found here to current command so it
      'forms a negative number
      
      'continue adding characters until non-numeric or EOL is reached
      Do Until lngPos + 1 > Len(strCurrentLine)
        intChar = AscW(Mid$(strCurrentLine, lngPos + 1, 1))
        If intChar < 48 Or intChar > 57 Then
          'anything other than a digit (0-9)
          Exit Do
        Else
          'add character
          NextCommand = NextCommand & ChrW$(intChar)
          'incrmeent position
          lngPos = lngPos + 1
        End If
      Loop
    End If
    Exit Function
  Case 33 '!
    'special case; "!" and "!=" returned as separate commands
    If Mid$(strCurrentLine, lngPos + 1, 1) = "=" Then
      'increment pointer
      lngPos = lngPos + 1
      'return not equal
      NextCommand = "!="
    End If
    Exit Function
  Case 60 '<
    'special case; "<", "<=" and "<>" returned as separate commands
    If Mid$(strCurrentLine, lngPos + 1, 1) = "=" Then
      'increment pointer
      lngPos = lngPos + 1
      'return less than or equal
      NextCommand = "<="
    ElseIf Mid$(strCurrentLine, lngPos + 1, 1) = ">" Then
      'increment pointer
      lngPos = lngPos + 1
      'return not equal
      NextCommand = "<>"
    End If
    Exit Function
  Case 62 '>
    'special case; ">", ">=" and "><" returned as separate commands
    If Mid$(strCurrentLine, lngPos + 1, 1) = "=" Then
      'increment pointer
      lngPos = lngPos + 1
      'return greater than or equal
      NextCommand = ">="
    ElseIf Mid$(strCurrentLine, lngPos + 1, 1) = "<" Then
      'increment pointer
      lngPos = lngPos + 1
      'return not equal ('><' is same as '<>')
      NextCommand = "<>"
    End If
    Exit Function
  Case 42 '*
    'special case; "*" and "*=" returned as separate commands;
    If Mid$(strCurrentLine, lngPos + 1, 1) = "=" Then
      'increment pointer
      lngPos = lngPos + 1
      'return shorthand multiplication
      NextCommand = "*="
    'since block commands are removed, check for the in order to provide a
    'meaningful error message
    ElseIf Mid$(strCurrentLine, lngPos + 1, 1) = "/" Then
      lngPos = lngPos + 1
      NextCommand = "* /"
    End If
    Exit Function
  Case 47 '/
    'special case; "/" , "//" and "/=" returned as separate commands
    If Mid$(strCurrentLine, lngPos + 1, 1) = "=" Then
      lngPos = lngPos + 1
      'return shorthand division
      NextCommand = "/="
    ElseIf Mid$(strCurrentLine, lngPos + 1, 1) = "/" Then
      lngPos = lngPos + 1
      NextCommand = "//"
    'since block commands are removed, check for the in order to provide a
    'meaningful error message
    ElseIf Mid$(strCurrentLine, lngPos + 1, 1) = "*" Then
      lngPos = lngPos + 1
      NextCommand = "/*"
    End If
    Exit Function
  Case 124 '|
    'special case; "|" and "||" returned as separate commands
    If Mid$(strCurrentLine, lngPos + 1, 1) = "|" Then
      'increment pointer
      lngPos = lngPos + 1
      'return double '|'
      NextCommand = "||"
    End If
    Exit Function
  Case 38 '&
    'special case; "&" and "&&" returned as separate commands
    If Mid$(strCurrentLine, lngPos + 1, 1) = "&" Then
      'increment pointer
      lngPos = lngPos + 1
      'return double '&'
      NextCommand = "&&"
    End If
    Exit Function
  End Select
  
  'if not a text string,
  If Not blnInQuotes Then
    'continue adding characters until element separator or EOL is reached
    Do Until lngPos + 1 > Len(strCurrentLine)
      intChar = AscW(Mid$(strCurrentLine, lngPos + 1, 1))
      Select Case intChar
      Case 32, 33, 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, 123 To 126
        '  space, !"&'() *+,-/:;<=>?[\]^`{|}~
        'end of command text found;
        Exit Do
      Case Else
        'add character
        NextCommand = NextCommand & ChrW$(intChar)
        'incrmeent position
        lngPos = lngPos + 1
      End Select
    Loop

  Else
    'if past end of line
    '(which could only happen if a line contains a single double quote on it)
    If lngPos + 1 > Len(strCurrentLine) Then
      'return the single quote
      Exit Function
    End If
      
    'add characters until another TRUE quote is found
    Do
      'reset pointer to next
      intChar = AscW(Mid$(strCurrentLine, lngPos + 1, 1))
      'increment position
      lngPos = lngPos + 1
      
      'if last char was a slash, need to treat this next
      'character as special
      If blnSlash Then
        'next char is just added as-is;
        'no checking it
        'always reset  the slash
        blnSlash = False
      Else
        'regular char; check for slash or quote mark
        Select Case intChar
        Case 34 'quote mark
          'a quote marks end of string
          blnInQuotes = False
        Case 92 'slash
          blnSlash = True
        End Select
      End If

      NextCommand = NextCommand & ChrW$(intChar)
      
      'if at end of line
      If(lngPos = Len(strCurrentLine)) Then
        'if still in quotes,
        If blnInQuotes Then
          'set inquotes to false to exit the loop
          'the compiler will have to recognize that
          'this text string is not properly enclosed in quotes
          blnInQuotes = False
        End If
      End If
    Loop While blnInQuotes
  End If
Exit Function

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  'if error is an app specific error, just pass it along; otherwise create
  'an app specific error to encapsulate whatever happened
  If(lngError And vbObjectError) = vbObjectError Then
    'pass it along
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
  Else
    On Error GoTo 0: Err.Raise vbObjectError + 657, strErrSrc, Replace(LoadResString(657), ARG1, CStr(lngError) & ":" & strError)
  End If
End Function
Private Function CompileIf() As Boolean
  'this routine will read and validate a group of test commands
  'for an 'if' statement and return
  'it is entered when the compiler encounters an 'if' command
  'the syntax expected for test commands is:
  '
  '   if(<test>){
  'or
  '   if(<test> && <test> && ... ){
  'or
  '   if((<test> || <test> || ... )){
  '
  'or a combination of ORs and ANDs, as long as ORs are always
  '   in brackets, and ANDs are never in brackets
  '
  '   <test> may be a test command (<tstcmd>(arg1, arg2, ..., argn)
  '   or a special syntax representation of a test command:
  '     fn        ==> isset(fn)
  '     vn == m   ==> equaln(vn,m)
  '     etc
  '
  'valid special comparison expressions are ==, !=, <>, >, =<, <, >=
  'OR'ed tests must always be enclosed in parenthesis; AND'ed tests
  'must never be enclosed in parentheses (this ensures the compiled code
  'will be compatible with the AGI interpreter)
  '
  '(any test command may have the negation operator (!) placed directly in front of it
  
  Dim strTestCmd As String, strArg As String
  Dim bytTestCmd As Byte, bytArg(7) As Byte
  Dim lngArg As Long, lngWord() As Long
  Dim intWordCount As Integer
  Dim i As Integer
  Dim blnIfBlock As Boolean 'command block, not a comment block
  Dim blnNeedNextCmd As Boolean
  Dim intNumTestCmds As Integer
  Dim intNumCmdsInBlock As Integer
  Dim blnNOT As Boolean


  On Error GoTo ErrHandler


  blnIfBlock = False
  intNumTestCmds = 0
  intNumCmdsInBlock = 0
  blnNeedNextCmd = True
  
  'write out starting if byte
  tmpLogRes.WriteByte &HFF
  
  'next character should be "("
  If NextChar() <> "(" Then
    blnError = True
    strErrMsg = LoadResString(4002)
    Exit Function
  End If
  
  'now, step through input, until final ')'' is found:
  Do
    'get next command
    strTestCmd = NextCommand()
    'check for end of input,
    If lngLine = -1 Then
      blnError = True
      strErrMsg = LoadResString(4106)
      Exit Function
    End If
    
    'if awaiting a test command,
    If blnNeedNextCmd Then
      Select Case strTestCmd
      Case "(" 'open paran
        'if already in a block
        If blnIfBlock Then
          blnError = True
          strErrMsg = LoadResString(4045)
          Exit Function
        End If
        'write 'or' block start
        tmpLogRes.WriteByte &HFC
        blnIfBlock = True
        intNumCmdsInBlock = 0
      Case ")"
        'if a test command is expected, ')' always causes error
        If intNumTestCmds = 0 Then
          strErrMsg = LoadResString(4057)
        ElseIf blnIfBlock And intNumCmdsInBlock = 0 Then
          strErrMsg = LoadResString(4044)
        Else
          strErrMsg = LoadResString(4056)
        End If
        blnError = True
        Exit Function
      Case Else
        'check for NOT
        blnNOT = (strTestCmd = NOT_TOKEN)
        If blnNOT Then
          tmpLogRes.WriteByte &HFD
          'read in next test command
          strTestCmd = NextCommand()
          'check for end of input,
          If lngLine = -1 Then
            blnError = True
            strErrMsg = LoadResString(4106)
            Exit Function
          End If
        End If
        bytTestCmd = CommandNum(True, strTestCmd)
        'if command not found,
        If bytTestCmd = 255 Then
          'check for special syntax
          If Not CompileSpecialIf(strTestCmd, blnNOT) Then
            'error; the CompileSpecialIf function
            'sets the error codes, and CompileLogic will
            'call the error handler
            Exit Function
          End If
        Else
          'write the test command code
          tmpLogRes.WriteByte bytTestCmd
          'next command should be "("
          If NextChar() <> "(" Then
            blnError = True
            strErrMsg = LoadResString(4048)
            Exit Function
          End If
          
          'check for return.false() command
          If bytTestCmd = 0 Then
            'warn user that it's not compatible with AGI Studio
            Select Case agMainLogSettings.ErrorLevel
            Case leHigh, leMedium
              'generate warning
              AddWarning 5081
            Case leLow
            End Select
          End If
          
          'if said command
          If bytTestCmd = &HE Then
            'enable error trapping to catch invalid word
            On Error Resume Next
            'and word count
            intWordCount = 0
            ReDim lngWord(0)
            'get first word arg
            lngArg = GetNextArg(atVocWrd, intWordCount + 1)
            'if error
            If blnError Then
              ' if error number is 4054
              If Val(strErrMsg) = 4054 Then
                ' add command name to error string
                strErrMsg = Replace(strErrMsg, ARG2, agTestCmds(bytTestCmd).Name)
              End If
              'exit
              Exit Function
            End If
            
            'loop to add this word, and any more
            Do
              'add this word number to array of word numbers
              ReDim Preserve lngWord(intWordCount)
              lngWord(intWordCount) = lngArg
              intWordCount = intWordCount + 1
              'if too many words
              If intWordCount = 10 Then
                blnError = True
                strErrMsg = LoadResString(4093)
                Exit Function
              End If
              
              'get next character
              '(should be a comma, or close parenthesis, if no more words)
              strArg = NextChar()
              If LenB(strArg) <> 0 Then
                Select Case AscW(strArg)
                Case 41 ')'
                  'move pointer back one space so
                  'the ')' will be found at end of command
                  lngPos = lngPos - 1
                  Exit Do


                Case 44 ','
                  'expected; now check for next word argument
                  lngArg = GetNextArg(atVocWrd, intWordCount + 1)
                  'if error
                  If blnError Then
                    'exit
                    ' if error number is 4054
                    If Val(strErrMsg) = 4054 Then
                      ' add command name to error string
                      strErrMsg = Replace(strErrMsg, ARG2, agTestCmds(bytTestCmd).Name)
                    End If
                    Exit Function
                  End If


                Case Else
                  'error
                  blnError = True
                  'check for added quotes; they are the problem
                  If lngQuoteAdded >= 0 Then
                    'reset line;
                    lngLine = lngQuoteAdded
                    lngErrLine = lngLine - lngIncludeOffset
                    'string error
                    strErrMsg = LoadResString(4051)
                  Else
                    'use 1-base arg values
                    strErrMsg = Replace(LoadResString(4047), ARG1, CStr(intWordCount + 1))
                  End If
                  Exit Function
                End Select
              Else
              '*'Debug.Assert False
              'we should normally never get here, since changing the function to allow
              'splitting over multiple lines, unless this is the LAST line of
              'the logic (an EXTREMELY rare edge case)
                'error
                blnError = True
                
                'check for added quotes; they are the problem
                If lngQuoteAdded >= 0 Then
                  'reset line;
                  lngLine = lngQuoteAdded
                  lngErrLine = lngLine - lngIncludeOffset
                  'string error
                  strErrMsg = LoadResString(4051)
                Else
                  'use 1-base arg values
                  strErrMsg = Replace(LoadResString(4047), ARG1, CStr(intWordCount + 1))
                End If
                Exit Function
              End If
            Loop While True
              
            'reset the quotemark error flag after ')' is found
            lngQuoteAdded = -1
           
            'reset error handling
            On Error GoTo 0
            
            'need to write number of arguments for 'said'
            'before writing arguments themselves
            tmpLogRes.WriteByte CByte(intWordCount)
            
            'now add words
            For i = 0 To intWordCount - 1
              'write word Value
              tmpLogRes.WriteWord lngWord(i)
            Next i
          Else
            'not 'said'; extract arguments for this command
            For i = 0 To agTestCmds(CByte(bytTestCmd)).ArgCount - 1
              'after first argument, verify comma separates arguments
              If i > 0 Then
                If NextChar(True) <> "," Then
                  blnError = True
                  'use 1-base arg values
                  strErrMsg = Replace(LoadResString(4047), ARG1, CStr(i + 1))
                  Exit Function
                End If
              End If
              
              'reset the quotemark error flag after comma is found
              lngQuoteAdded = -1
              bytArg(i) = GetNextArg(agTestCmds(CByte(bytTestCmd)).ArgType(i), i)
              'if error
              If blnError Then
                ' if error number is 4054
                If Val(strErrMsg) = 4054 Then
                  ' add command name to error string
                  strErrMsg = Replace(strErrMsg, ARG2, agTestCmds(bytTestCmd).Name)
                End If
                Exit Function
              End If
              'write argument
              tmpLogRes.WriteByte bytArg(i)
            Next i
          End If
          'next character should be ")"
          If NextChar() <> ")" Then
            blnError = True
            strErrMsg = LoadResString(4160)
            Exit Function
          End If
          'reset the quotemark error flag
          lngQuoteAdded = -1
          
          'validate arguments for this command
          If Not ValidateIfArgs(bytTestCmd, bytArg()) Then
            'error assigned by called function
            Exit Function
          End If
        End If
        
        'command added
        intNumTestCmds = intNumTestCmds + 1
        'if in IF block,
        If blnIfBlock Then
          intNumCmdsInBlock = intNumCmdsInBlock + 1
        End If
        'toggle off need for test command
        blnNeedNextCmd = False
      End Select
    Else 'not awaiting a test command
      Select Case strTestCmd
      Case NOT_TOKEN
        'invalid
        blnError = True
        strErrMsg = LoadResString(4097)
        Exit Function
      Case AND_TOKEN
        'if inside brackets
        If blnIfBlock Then
          blnError = True
          strErrMsg = LoadResString(4037)
          Exit Function
        End If
        blnNeedNextCmd = True
      Case OR_TOKEN
        'if NOT inside brackets
        If Not blnIfBlock Then
          blnError = True
          strErrMsg = LoadResString(4100)
          Exit Function
        End If
        blnNeedNextCmd = True
      Case ")"
        'if inside brackets
        If blnIfBlock Then
          'ensure at least one command in block,
          If intNumCmdsInBlock = 0 Then
            blnError = True
            strErrMsg = LoadResString(4044)
            Exit Function
          End If
          'close brackets
          blnIfBlock = False
          tmpLogRes.WriteByte &HFC
        Else
          'ensure at least one command in block,
          If intNumTestCmds = 0 Then
            blnError = True
            strErrMsg = LoadResString(4044)
            Exit Function
          End If
          'end of if found
          Exit Do
        End If
      Case Else
        If blnIfBlock Then
          strErrMsg = LoadResString(4101)
        Else
          strErrMsg = LoadResString(4038)
        End If
        blnError = True
        Exit Function
      End Select
    End If
  'never leave loop normally; error, end of input, or successful
  'compilation of test commands will all exit loop directly
  Loop While True
  
  'write ending if byte
  tmpLogRes.WriteByte &HFF
  'return true
  CompileIf = True
Exit Function

ErrHandler:
  blnError = True
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "CompileIf")
  'err.clear
End Function
Private Function ConcatArg(strText As String) As String
  'this function concatenates strings; it assumes strText
  'is the string that was just read into the compiler;
  'it then checks if there are additional elements of
  'this string to add to it; if so, they are added.
  'it returns the completed string, with lngPos and lngLine
  'set accordingly (if there is nothing to concatenate, it
  'returns original string)
  'NOTE: input string has already been checked for starting
  'and ending quotation marks


  Dim strTextContinue As String
  Dim lngLastPos As Long, lngLastLine As Long
  Dim strLastLine As String
  Dim lngSlashCount As Long, lngQuotesOK As Long


  On Error GoTo ErrHandler
  
  'verify at least two characters
  If Len(strText) < 2 Then
    'error
    blnError = True
    strErrMsg = LoadResString(4081)
    Exit Function
  End If
  
  'start with input string
  ConcatArg = strText
  
  'save current position info
  lngLastPos = lngPos
  lngLastLine = lngLine
  strLastLine = strCurrentLine
  
  'if at end of last line
  If lngLastPos = Len(strLastLine) Then
    'get next command
    strTextContinue = NextCommand()
    
    'add strings until concatenation is complete
    Do Until Left$(strTextContinue, 1) <> QUOTECHAR
      'if a continuation string is found, we need to reset
      'the quote checker
      lngQuotesOK = 0
      
      'check for end quote
      If Right$(strTextContinue, 1) <> QUOTECHAR Then
        'bad end quote (set end quote marker, overriding error
        'that might happen on a previous line)
        lngQuotesOK = 2
      Else
        'just because it ends in a quote doesn't mean it's good;
        'it might be an embedded quote
        '(we know we have at least two chars, so we don't need
        'to worry about an error with MID function)
        
        'check for an odd number of slashes immediately preceding
        'this quote
        lngSlashCount = 0
        Do
          If Mid(ConcatArg, Len(ConcatArg) - (lngSlashCount + 1), 1) = "\" Then
            lngSlashCount = lngSlashCount + 1
          Else
            Exit Do
          End If
        Loop While Len(ConcatArg) - (lngSlashCount + 1) >= 0
    
        'if it IS odd, then it's not a valid quote
        If Int(lngSlashCount / 2) <> lngSlashCount / 2 Then
          'it's embedded, and doesn't count
          'bad end quote (set end quote marker, overriding error
          'that might happen on a previous line)
          lngQuotesOK = 2
        End If
      End If
      
      'if end quote is missing, deal with it
      If lngQuotesOK > 0 Then
        'note which line had quotes added, in case it results
        'in an error caused by a missing end ')' or whatever
        'the next required element is
        lngQuoteAdded = lngLine

        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          'error
          blnError = True
          strErrMsg = LoadResString(4080)
          Exit Function
        Case leMedium
          'add quote
          strTextContinue = strTextContinue & QUOTECHAR
          'set warning
          AddWarning 5002
        Case leLow
          'add quote
          strTextContinue = strTextContinue & QUOTECHAR
        End Select
      End If
      
      'strip off ending quote of current msg
      ConcatArg = Left$(ConcatArg, Len(ConcatArg) - 1)
      'add it to strText
      ConcatArg = ConcatArg & Right$(strTextContinue, Len(strTextContinue) - 1)
      'save current position info
      lngLastPos = lngPos
      lngLastLine = lngLine
      strLastLine = strCurrentLine
      'get next command
      strTextContinue = NextCommand()
    Loop
    
    'after end of string found, move back to correct position
    lngPos = lngLastPos
    lngLine = lngLastLine
    lngErrLine = lngLastLine
    strCurrentLine = strLastLine
  End If


Exit Function

ErrHandler:
  'raise an error
  blnError = True
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "ConcatArg")
End Function


Private Function RemoveComments() As Boolean
  'this function strips out comments from the input text
  'and trims off leading and trailing spaces
  '
  'agi comments:
  '      // - rest of line is ignored
  '      [ - rest of line is ignored


  Dim lngPos As Long
  Dim blnInQuotes As Boolean, blnSlash As Boolean
  Dim intROLIgnore As Integer


  On Error GoTo ErrHandler

  'reset compiler
  ResetCompiler
  lngIncludeOffset = 0
  lngLine = 0
  
  Do
    'reset rol ignore
    intROLIgnore = 0
    
    'reset comment start & char ptr, and inquotes
    lngPos = 0
    blnInQuotes = False
    
    'if this line is not empty,
    If LenB(strCurrentLine) <> 0 Then
      Do Until lngPos >= Len(strCurrentLine)
        'get next character from string
        lngPos = lngPos + 1
        'if NOT inside a quotation,
        If Not blnInQuotes Then
          'check for comment characters at this position
          If((Mid$(strCurrentLine, lngPos, 2) = CMT2_TOKEN) Or(Mid$(strCurrentLine, lngPos, 1) = CMT1_TOKEN)) Then
          intROLIgnore = lngPos
            Exit Do
          End If
          ' slash codes never occur outside quotes
          blnSlash = False
          'if this character is a quote mark, it starts a string
          blnInQuotes = (AscW(Mid$(strCurrentLine, lngPos)) = 34)
        Else
          'if last character was a slash, ignore this character
          'because it's part of a slash code
          If blnSlash Then
            'always reset  the slash
            blnSlash = False
          Else
            'check for slash or quote mark
            Select Case AscW(Mid$(strCurrentLine, lngPos))
            Case 34 'quote mark
              'a quote marks end of string
              blnInQuotes = False
            Case 92 'slash
              blnSlash = True
            End Select
          End If
        End If
      Loop
      'if any part of line should be ignored,
      If intROLIgnore > 0 Then
        strCurrentLine = Left$(strCurrentLine, intROLIgnore - 1)
      End If
    End If
    'replace comment, also trim it
    ReplaceLine Trim$(strCurrentLine)
    
    'get next line
    IncrementLine
  Loop Until lngLine = -1
  
  'success
  RemoveComments = True
Exit Function
ErrHandler:
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "RemoveComments")
  Err.Clear
End Function

Private Function AddIncludes(stlLogicText As StringList) As Boolean
  'this function uses the logic text that is passed to the compiler
  'to create the input text that is parsed.
  'it copies the lines from the logic text to the input text, and
  'replaces any include file lines with the actual lines from the
  'include file (include file lines are given a 'header' to identify
  'them as include lines)
  
  
  Dim IncludeLines As StringList
  Dim strIncludeFilename As String
  Dim strIncludePath As String
  Dim strIncludeText As String
  Dim CurIncludeLine As Long   ' current line in IncludeLines (the include file)
  Dim intFileCount As Integer
  Dim i As Integer
  Dim intFile As Integer
  Dim lngLineCount As Long


  On Error GoTo ErrHandler


  Set stlInput = New StringList
  Set IncludeLines = New StringList 'only temporary,


  lngLine = 0
  lngLineCount = stlLogicText.Count
  
  'module is always main module
  strModule = vbNullString
  strModFileName = vbNullString
  
  'step through all lines
  Do
    'set errline
    lngErrLine = lngLine
    'check this line for include statement
    If Left$(stlLogicText(lngLine), 8) = "#include" Then
      'proper format requires a space after 'include'
      If Mid$(stlLogicText(lngLine), 9, 1) <> " " Then
        'generate error
        strErrMsg = LoadResString(4103)
        Exit Function
      End If
      'build include filename
      strIncludeFilename = Trim$(Right$(stlLogicText(lngLine), Len(stlLogicText(lngLine)) - 9))
      
      'check for a filename
      If LenB(strIncludeFilename) = 0 Then
        strErrMsg = LoadResString(4060)
        Exit Function
      End If
      
      'if quotes aren't used correctly
      If Left$(strIncludeFilename, 1) = QUOTECHAR And Right$(strIncludeFilename, 1) <> QUOTECHAR Or _
         Right$(strIncludeFilename, 1) = QUOTECHAR And Left$(strIncludeFilename, 1) <> QUOTECHAR Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          'return error: improper use of quote marks
          strErrMsg = LoadResString(4059)
          Exit Function
        Case leMedium, leLow
          'assume quotes are needed
          If AscW(strIncludeFilename) <> 34 Then
            strIncludeFilename = QUOTECHAR & strIncludeFilename
          End If
          If AscW(Right$(strIncludeFilename, 1)) <> 34 Then
            strIncludeFilename = strIncludeFilename & QUOTECHAR
          End If
          'set warning
          AddWarning 5028, Replace(LoadResString(5028), ARG1, strIncludeFilename)
        End Select
      End If
      
      'if quotes,
      If Left$(strIncludeFilename, 1) = QUOTECHAR Then
        'strip off quotes
        strIncludeFilename = Mid$(strIncludeFilename, 2, Len(strIncludeFilename) - 2)
      End If
      
      'if filename doesnt include a path,
      If LenB(JustPath(strIncludeFilename, True)) = 0 Then
        'get full path name to include file
        strIncludeFilename = agResDir & strIncludeFilename
      End If
      
      'verify file exists
      If Not FileExists(strIncludeFilename) Then
        strErrMsg = Replace(LoadResString(4050), ARG1, strIncludeFilename)
        Exit Function
      End If
'****
'      cant check for open includes; they are in a different application
'****

      On Error Resume Next
      'now open the include file, and get the text
      intFile = FreeFile()
      Open strIncludeFilename For Binary As intFile
      strIncludeText = String$(LOF(intFile), 0)
      Get intFile, 1, strIncludeText
      Close intFile
      'check for error,
      If Err.Number<> 0 Then
        strErrMsg = Replace(LoadResString(4055), ARG1, strIncludeFilename)
        Exit Function
      End If


      On Error GoTo ErrHandler
      
      'assign text to stringlist
      Set IncludeLines = New StringList
      IncludeLines.Assign strIncludeText
      
      'if there are any lines,
      If IncludeLines.Count > 0 Then
        'save file name to allow for error checking
        intFileCount = intFileCount + 1
        ReDim Preserve strIncludeFile(intFileCount)
        strIncludeFile(intFileCount) = strIncludeFilename
        
        'add all these lines into this position
        For CurIncludeLine = 0 To IncludeLines.Count - 1
          'verify the include file contains no includes
          If Left$(Trim$(IncludeLines(CurIncludeLine)), 2) = "#i" Then
            strErrMsg = LoadResString(4061)
            lngErrLine = CurIncludeLine
            Exit Function
          End If
          'include filenumber and line number from includefile
          stlInput.Add "#I" & CStr(intFileCount) & ":" & CStr(CurIncludeLine) & "#" & IncludeLines(CurIncludeLine)
        Next CurIncludeLine
      End If
      'add a blank line as a place holder for the 'include' line
      '(to keep line counts accurate when calculating line number for errors)
      stlInput.Add vbNullString
    Else
      'not an include line
      'check for any instances of #I, since these will
      'interfere with include line handling
      If Left$(stlLogicText(lngLine), 2) = "#i" Then
        strErrMsg = LoadResString(4069)
        Exit Function
      End If
      'copy the line by itself
      stlInput.Add stlLogicText(lngLine)
    End If
    lngLine = lngLine + 1
  Loop Until lngLine >= lngLineCount
  'done
  Set IncludeLines = Nothing
  'return success
  AddIncludes = True
Exit Function

ErrHandler:
  'unknown error
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "AddIncludes")
  Err.Clear
End Function



Private Function ReadDefines() As Boolean


  Dim i As Long, j As Integer
  Dim blnInQuote As Boolean
  Dim tdNewDefine As TDefine
  Dim rtn As Long


  On Error GoTo ErrHandler
  
  'reset Count of defines
  lngDefineCount = 0
  
  'reset compiler
  ResetCompiler
  
  'reset error string
  strErrMsg = vbNullString
  
  'step through all lines and find define values
  Do
    'check for define statement
    If InStr(1, strCurrentLine, CONST_TOKEN) = 1 Then
      'strip off define keyword
      strCurrentLine = Trim$(Right$(strCurrentLine, Len(strCurrentLine) - Len(CONST_TOKEN)))
      
      'if equal marker (i.e. space) not present
      If InStr(1, strCurrentLine, " ") = 0 Then
        'error
        strErrMsg = LoadResString(4104)
        Exit Function
      End If
      
      'split it by position of first space
      tdNewDefine.Name = Trim$(Left$(strCurrentLine, InStr(1, strCurrentLine, " ") - 1))
      tdNewDefine.Value = Trim$(Right$(strCurrentLine, Len(strCurrentLine) - InStr(1, strCurrentLine, " ")))
            
      'validate define name
      rtn = ValidateDefName(tdNewDefine.Name)
      'name error 7-12  are only warnings if error level is medium or low
      Select Case agMainLogSettings.ErrorLevel
      Case leMedium
        'check for name warnings
        Select Case rtn
        Case 7
          'set warning
          AddWarning 5034, Replace(LoadResString(5034), ARG1, tdNewDefine.Name)
          'reset return error code
          rtn = 0
         
        Case 8 To 12
          'set warning
          AddWarning 5035, Replace(LoadResString(5035), ARG1, tdNewDefine.Name)
          'reset return error code
          rtn = 0
        End Select
      Case leLow
        'check for warnings
        If rtn >= 7 And rtn <= 12 Then
          'reset return error code
          rtn = 0
        End If
      End Select
      
      'check for errors
      If rtn<> 0 Then
        'check for name errors
        Select Case rtn
        Case 1 ' no name
          strErrMsg = LoadResString(4070)
        Case 2 ' name is numeric
          strErrMsg = LoadResString(4072)
        Case 3 ' name is command
          strErrMsg = Replace(LoadResString(4021), ARG1, tdNewDefine.Name)
        Case 4 ' name is test command
          strErrMsg = Replace(LoadResString(4022), ARG1, tdNewDefine.Name)
        Case 5 ' name is a compiler keyword
          strErrMsg = Replace(LoadResString(4013), ARG1, tdNewDefine.Name)
        Case 6 ' name is an argument marker
          strErrMsg = LoadResString(4071)
        Case 7 ' name is already globally defined
          strErrMsg = Replace(LoadResString(4019), ARG1, tdNewDefine.Name)
        Case 8 ' name is reserved variable name
          strErrMsg = Replace(LoadResString(4018), ARG1, tdNewDefine.Name)
        Case 9 ' name is reserved flag name
          strErrMsg = Replace(LoadResString(4014), ARG1, tdNewDefine.Name)
        Case 10 ' name is reserved number constant
          strErrMsg = Replace(LoadResString(4016), ARG1, tdNewDefine.Name)
        Case 11 ' name is reserved object constant
          strErrMsg = Replace(LoadResString(4017), ARG1, tdNewDefine.Name)
        Case 12 ' name is reserved message constant
          strErrMsg = Replace(LoadResString(4015), ARG1, tdNewDefine.Name)
        Case 13 ' name contains improper character
          strErrMsg = LoadResString(4067)
        End Select
        'don't exit; check for define Value errors first
      End If
      
      'validate define Value
      rtn = ValidateDefValue(tdNewDefine)
      'Value errors 4,5,6 are only warnings if error level is medium or low
      Select Case agMainLogSettings.ErrorLevel
      Case leMedium
        'if Value error is due to missing quotes
        Select Case rtn
        Case 4  'string Value missing quotes
          'fix the define Value
          If AscW(tdNewDefine.Value) <> 34 Then
            tdNewDefine.Value = QUOTECHAR & tdNewDefine.Value
          End If
          If AscW(Right$(tdNewDefine.Value, 1)) <> 34 Then
            tdNewDefine.Value = tdNewDefine.Value & QUOTECHAR
          End If
          
          'set warning
          AddWarning 5022
          'reset error code
          rtn = 0
        Case 5 ' Value is already defined by a reserved name
          'set warning
          AddWarning 5032, Replace(LoadResString(5032), ARG1, tdNewDefine.Value)
          'reset error code
          rtn = 0
          
        Case 6 ' Value is already defined by a global name
          'set warning
          AddWarning 5031, Replace(LoadResString(5031), ARG1, tdNewDefine.Value)
          'reset error code
          rtn = 0
        End Select
      Case leLow
        'if Value error is due to missing quotes
        Select Case rtn
        Case 4
          'fix the define Value
          If AscW(tdNewDefine.Value) <> 34 Then
            tdNewDefine.Value = QUOTECHAR & tdNewDefine.Value
          End If
          If AscW(Right$(tdNewDefine.Value, 1)) <> 34 Then
            tdNewDefine.Value = tdNewDefine.Value & QUOTECHAR
          End If
          'reset return Value
          rtn = 0
        Case 5, 6
          'reset return Value
          rtn = 0
        End Select
      End Select
      
      'check for errors
      If rtn <> 0 Then
        'if already have a name error
        If LenB(strErrMsg) <> 0 Then
          'append Value error
          strErrMsg = strErrMsg & "; and "
        End If
      
        'check for Value error
        Select Case rtn
        Case 1 ' no Value
          strErrMsg = strErrMsg & LoadResString(4073)
'
'a return Value of 2 is no longer possible; this
'Value has been removed from the ValidateDefineValue function
'        Case 2 ' Value is an invalid argument marker
'          strErrMsg = strErrMsg & "4065: Invalid argument declaration Value"
        Case 3 ' Value contains an invalid argument Value
          strErrMsg = strErrMsg & LoadResString(4042)
        Case 4 ' Value is not a string, number or argument marker
          strErrMsg = strErrMsg & LoadResString(4082)
        Case 5 ' Value is already defined by a reserved name
          strErrMsg = strErrMsg & Replace(LoadResString(4041), ARG1, tdNewDefine.Value)
        Case 6 ' Value is already defined by a global name
          strErrMsg = strErrMsg & Replace(LoadResString(4040), ARG1, tdNewDefine.Value)
        End Select
      End If
      
      'if an error was generated during define validation
      If LenB(strErrMsg) <> 0 Then
        Exit Function
      End If
      
      'check all previous defines
      For i = 0 To lngDefineCount - 1
        If tdNewDefine.Name = tdDefines(i).Name Then
          strErrMsg = Replace(LoadResString(4012), ARG1, tdDefines(i).Name)
          Exit Function
        End If
        If tdNewDefine.Value = tdDefines(i).Value Then
          'numeric duplicates aren't a problem
          If Not IsNumeric(tdNewDefine.Value) Then
            Select Case agMainLogSettings.ErrorLevel
            Case leHigh
              'set error
              strErrMsg = Replace(Replace(LoadResString(4023), ARG1, tdDefines(i).Value), ARG2, tdDefines(i).Name)
              Exit Function
            Case leMedium
              'set warning
              AddWarning 5033, Replace(Replace(LoadResString(5033), ARG1, tdNewDefine.Value), ARG2, tdDefines(i).Name)
            Case leLow
              'do nothing
            End Select
          End If
        End If
      Next i
      
      'check define against labels
      If bytLabelCount > 0 Then
        For i = 1 To bytLabelCount
          If tdNewDefine.Name = llLabel(i).Name Then
            strErrMsg = Replace(LoadResString(4020), ARG1, tdNewDefine.Name)
            Exit Function
          End If
        Next i
      End If
           
      'save this define
      ReDim Preserve tdDefines(lngDefineCount)
      tdDefines(lngDefineCount) = tdNewDefine

      'increment counter
      lngDefineCount = lngDefineCount + 1
      
      'now set this line to empty so Compiler doesn"t try to read it
      If Left$(stlInput(lngLine), 2) = "#I" Then
        'this is an include line; need to leave include line info
        stlInput(lngLine) = Left$(stlInput(lngLine), InStr(4, stlInput(lngLine), "#"))
      Else
        'just blank out entire line
        stlInput(lngLine) = vbNullString
      End If
    End If
    'get next line
    IncrementLine
  Loop Until lngLine = -1


  ReadDefines = True
Exit Function

ErrHandler:
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "ReadDefines")
  Err.Clear
End Function
Private Function ReadMsgs() As Boolean

  'note that stripped message lines also strip out the include header string
  'this doesn't matter since they are only blank lines anyway
  'only need include header info if error occurs, and errors never occur on
  'blank line
  
  Dim intMsgNum As Integer, i As Long
  Dim strCmd As String, strMsgContinue As String
  Dim strMsgSep As String
  Dim lngMsgStart As Long, blnDef As Boolean
  Dim intMsgLineCount As Integer
  Dim lngSlashCount As Long, lngQuotesOK As Long


  On Error GoTo ErrHandler
  
  'build blank message list
  For intMsgNum = 0 To 255
    strMsg(intMsgNum) = vbNullString
    blnMsg(intMsgNum) = False
    intMsgWarn(intMsgNum) = 0
  Next intMsgNum
  
  'reset compiler
  ResetCompiler

  Do
    'get first command on this line
    strCmd = NextCommand(True)
    
    'if this is the message marker
    If strCmd = MSG_TOKEN Then
      'save starting line number (incase this msg uses multiple lines)
      lngMsgStart = lngLine
      
      'get next command on this line
      strCmd = NextCommand(True)
      
      'this should be a msg number
      If Not IsNumeric(strCmd) Then
        'error
        blnError = True
        strErrMsg = LoadResString(4077)
        Exit Function
      End If
      
      'validate msg number
      intMsgNum = VariableValue(strCmd)
      If intMsgNum <= 0 Then
        'error
        blnError = True
        strErrMsg = LoadResString(4077)
        Exit Function
      End If
      'if msg is already assigned
      If blnMsg(intMsgNum) Then
        blnError = True
        strErrMsg = Replace(LoadResString(4094), ARG1, CStr(intMsgNum))
        Exit Function
      End If
      
      'get next string command
      strCmd = NextCommand(False)
      
      'is this a valid string?
      If Not IsValidMsg(strCmd) Then
        'maybe it's a define
        If ConvertArgument(strCmd, atMsg) Then
          'defined strings never get concatenated
          blnDef = True
        End If
      End If
      
      'always reset the 'addquote' flag
      '(this is the flag that notes if/where a line had an end quote
      'added by the compiler; if this causes problems later in the
      'compilation of this command, we can then use mark this error
      'as the culprit
      lngQuoteAdded = -1
      
      'check msg for quotes (note ending quote has to be checked to make sure it's not
      'an embedded quote)
      'assume OK until we learn otherwise (0=OK; 1=bad start quote; 2=bad end quote; 3=bad both)
      lngQuotesOK = 0
      If Left$(strCmd, 1) <> QUOTECHAR Then
        'bad start quote
        lngQuotesOK = 1
      End If
      'check for end quote
      If Right$(strCmd, 1) <> QUOTECHAR Then
        'bad end quote
        lngQuotesOK = lngQuotesOK + 2
      Else
        'just because it ends in a quote doesn't mean it's good;
        'it might be an embedded quote
        '(we know we have at least two chars, so we don't need
        'to worry about an error with MID function)

        'check for an odd number of slashes immediately preceding
        'this quote
        lngSlashCount = 0
        Do
          If Mid(strCmd, Len(strCmd) - (lngSlashCount + 1), 1) = "\" Then
            lngSlashCount = lngSlashCount + 1
          Else
            Exit Do
          End If
        Loop While Len(strCmd) - (lngSlashCount + 1) >= 0

        'if it IS odd, then it's not a valid quote
        If Int(lngSlashCount / 2) <> lngSlashCount / 2 Then
          'it's embedded, and doesn't count
          lngQuotesOK = lngQuotesOK + 2
        End If
      End If
  
      'if either (or both) quote is missing, deal with it
      If lngQuotesOK > 0 Then
        'note which line had quotes added, in case it results
        'in an error caused by a missing end ')' or whatever
        'the next required element is
        lngQuoteAdded = lngLine

        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          blnError = True
          strErrMsg = LoadResString(4051)
          Exit Function
        Case leMedium, leLow
          'add quotes as appropriate
          If AscW(strCmd) <> 34 Then
            strCmd = QUOTECHAR & strCmd
          End If
          If AscW(Right$(strCmd, 1)) <> 34 Then
            strCmd = strCmd & QUOTECHAR
          End If
          'warn if medium
          If agMainLogSettings.ErrorLevel = leMedium Then
            'set warning
            AddWarning 5002
          End If
        End Select
      End If
      
      'concatenate, if necessary
      If Not blnDef Then
        strCmd = ConcatArg(strCmd)
        'if error,
        If blnError Then
          Exit Function
        End If
      End If
      
      'nothing allowed after msg declaration
      If lngPos <> Len(strCurrentLine) Then
        'error
        blnError = True
        strErrMsg = LoadResString(4099)
        Exit Function
      End If
  
      'strip off quotes (we know that the string
      'is properly enclosed by quotes because
      'ConcatArg function validates they are there
      'or adds them if they aren't[or raises an
      'error, in which case it doesn't even matter])
      strCmd = Mid$(strCmd, 2, Len(strCmd) - 2)
      
      'add msg
      strMsg(intMsgNum) = strCmd
      'validate message characters
      If Not ValidateMsgChars(strCmd, intMsgNum) Then
        'error was raised
        Exit Function
      End If


      blnMsg(intMsgNum) = True

      Do
        'set this line to empty so compiler doesn't try to read it
        stlInput(lngMsgStart) = vbNullString
        'increment the counter (to get multiple lines, if string is
        'concatenated over more than one line)
        lngMsgStart = lngMsgStart + 1
      'continue until back to current line
      Loop Until lngMsgStart > lngLine

    End If
    'get next line
    IncrementLine
  Loop Until lngLine = -1
  
  'check for any undeclared messages that haven't already been identified
  '(they are not really a problem, but user might want to know)
  'all messages from 1 to this point should be declared
  intMsgNum = 255
  Do Until blnMsg(intMsgNum) Or intMsgNum = 0
    intMsgNum = intMsgNum - 1
  Loop

  ReadMsgs = True
Exit Function

ErrHandler:
  blnError = True
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "ReadMsgs")
  Err.Clear
End Function

Private Sub ReplaceLine(strNewLine As String)
  'this function replaces the current line in the input string
  'with the strNewLine, while preserving include header info
  
  Dim strInclude As String
  
    'if this is from an include file
    If Left$(stlInput(lngLine), 2) = "#I" Then
      'need to save include header info so it can
      'be preserved after comments are removed
      strInclude = Left$(stlInput(lngLine), InStr(2, stlInput(lngLine), "#"))
    Else
      strInclude = vbNullString
    End If
    
    'replace the line
    stlInput(lngLine) = strInclude & strNewLine
End Sub


Private Sub ResetCompiler()
  'resets the compiler so it points to beginning of input
  'also loads first line into strCurrentLine
  
  'reset include offset, so error trapping
  'can correctly Count lines
  lngIncludeOffset = 0
  
  'reset error flag
  blnError = False
  'reset the quotemark error flag
  lngQuoteAdded = -1

  'set line pointer to -2 so first call to
  'IncrementLine gets first line
  lngLine = -2
  
  'get first line
  IncrementLine
  'NOTE: don't need to worry about first line;
  'compiler has already verified the input has at least one line
End Sub


Public Sub SetResourceIDs()
  'builds array of resourceIDs so
  'convertarg function can iterate through them much quicker
  
  Dim tmpLog As AGILogic, tmpPic As AGIPicture
  Dim tmpSnd As AGISound, tmpView As AGIView


  If blnSetIDs Then
    Exit Sub
  End If

  ReDim strLogID(255)
  ReDim strPicID(255)
  ReDim strSndID(255)
  ReDim strViewID(255)


  For Each tmpLog In agLogs
    strLogID(tmpLog.Number) = tmpLog.ID
  Next


  For Each tmpPic In agPics
    strPicID(tmpPic.Number) = tmpPic.ID
  Next


  For Each tmpSnd In agSnds
    strSndID(tmpSnd.Number) = tmpSnd.ID
  Next


  For Each tmpView In agViews
    strViewID(tmpView.Number) = tmpView.ID
  Next
  
  'set flag
  blnSetIDs = True
End Sub

Private Sub AddWarning(ByVal WarningNum As Long, Optional ByVal WarningText As String)
  
  'warning elements are separated by pipe character
  'WarningsText is in format:
  '  number|warningtext|line|module
  '
  '(number, line and module only have meaning for logic warnings
  ' other warnings generated during a game compile will use
  ' same format, but use -- for warning number, line and module)
   
  'if no text passed, use the default resource string
  
  Dim evWarn As String


  If Len(WarningText) = 0 Then
    WarningText = LoadResString(WarningNum)
  End If
  
  'only add if not ignoring
  If Not agNoCompWarn(WarningNum - 5000) Then
    evWarn = CStr(WarningNum) & "|" & WarningText & "|" & CStr(lngErrLine + 1) & "|" & _
                 IIf(LenB(strModule) <> 0, strModule, vbNullString)
    agGameEvents.RaiseEvent_LogCompWarning evWarn, bytLogComp
  End If
End Sub
Private Function ValidateArgs(ByVal CmdNum As Long, ByRef ArgVal() As Byte) As Boolean


  Dim blnUnload As Boolean, blnWarned As Boolean

  'check for specific command issues
  On Error GoTo ErrHandler
  
  'for commands that can affect variable values, need to check against reserved variables
  'for commands that can affect flags, need to check against reserved flags
  
  'for other commands, check the passed arguments to see if values are appropriate


  Select Case CmdNum
  Case 1, 2, 4 To 8, 10, 165 To 168 'increment, decrement, assignv, addn, addv, subn, subv
                                    'rindirect, mul.n, mul.v, div.n, div.v
    'check for reserved variables that should never be manipulated
    '(assume arg Value is zero to avoid tripping other checks)
    CheckResVarUse ArgVal(0), 0
    
    'for div.n(vA, B) only, check for divide-by-zero
    If CmdNum = 167 Then
      If ArgVal(1) = 0 Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          strErrMsg = LoadResString(4149)
          Exit Function
        Case leMedium
          AddWarning 5030
        Case leLow
        End Select
      End If
    End If


  Case 3 'assignn
    'check for actual Value being assigned
    CheckResVarUse ArgVal(0), ArgVal(1)


  Case 12, 13, 14 'set, reset, toggle
    'check for reserved flags
    CheckResFlagUse ArgVal(0)


  Case 18 'new.room(A)
    'validate that this logic exists
    If Not agLogs.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4120)
        Exit Function
      Case leMedium
        AddWarning 5053
      Case leLow
      End Select
    End If
    'expect no more commands
    blnNewRoom = True


  Case 19 'new.room.v
    'expect no more commands
    blnNewRoom = True


  Case 20 'load.logics(A)
    'validate that logic exists
    If Not agLogs.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4121), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5013
      Case leLow
      End Select
    End If


  Case 22  'call(A)
    'calling logic0 is a BAD idea
    If ArgVal(0) = 0 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4118)
        Exit Function
      Case leMedium
        AddWarning 5010
      Case leLow
        'no action
      End Select
    End If
    
    'recursive calling is BAD
    If ArgVal(0) = bytLogComp Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4117)
        Exit Function
      Case leMedium
        AddWarning 5089
      Case leLow
        'no action
      End Select
    End If
    
    'validate that logic exists
    If Not agLogs.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4156), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5076
      Case leLow
      End Select
    End If


  Case 30 'load.view(A)
    'validate that view exists
    If Not agViews.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4122), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5015
      Case leLow
      End Select
    End If


  Case 32 'discard.view(A)
    'validate that view exists
    If Not agViews.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4123), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5024
      Case leLow
      End Select
    End If


  Case 37 'position(oA, X,Y)
    'check x/y against limits
    If ArgVal(1) > 159 Or ArgVal(2) > 167 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
       strErrMsg = LoadResString(4128)
       Exit Function
      Case leMedium
        AddWarning 5023
      Case leLow
      End Select
    End If


  Case 39 'get.posn
    'neither variable arg should be a reserved Value
    If ArgVal(1) <= 26 Or ArgVal(2) <= 26 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If


  Case 41 'set.view(oA, B)
    'validate that view exists
    If Not agViews.Exists(ArgVal(1)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4124), ARG1, CStr(ArgVal(1)))
        Exit Function
      Case leMedium
        AddWarning 5037
      Case leLow
      End Select
    End If


  Case 49 To 53, 97, 118  'last.cel, current.cel, current.loop,
                          'current.view, number.of.loops, get.room.v
                          'get.num
    'variable arg is second
    'variable arg should not be a reserved Value
    If ArgVal(1) <= 26 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If


  Case 54 'set.priority(oA, B)
    'check priority Value
    If ArgVal(1) > 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4125)
        Exit Function
      Case leMedium
        AddWarning 5050
      Case leLow
      End Select
    End If


  Case 57 'get.priority
    'variable is second argument
    'variable arg should not be a reserved Value
    If ArgVal(1) <= 26 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If


  Case 63 'set.horizon(A)
    '>120 or <16 is unusual
    '>=167 will cause AGI to freeze/crash
    
    'validate horizon Value
    Select Case agMainLogSettings.ErrorLevel
    Case leHigh
      If ArgVal(0) >= 167 Then
        strErrMsg = LoadResString(4126)
        Exit Function
      End If
      If ArgVal(0) > 120 Then
        AddWarning 5042
      ElseIf ArgVal(0) < 16 Then
        AddWarning 5041
      End If


    Case leMedium
      If ArgVal(0) >= 167 Then
        AddWarning 5043
      ElseIf ArgVal(0) > 120 Then
          AddWarning 5042
      ElseIf ArgVal(0) < 16 Then
        AddWarning 5041
      End If


    Case leLow
    End Select


  Case 64, 65, 66 'object.on.water, object.on.land, object.on.anything
    'warn if used on ego
    If ArgVal(0) = 0 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5082
      Case leLow
      End Select
    End If


  Case 69 'distance
    'variable is third arg
    'variable arg should not be a reserved Value
    If ArgVal(2) <= 26 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If


  Case 73, 75, 99 'end.of.loop, reverse.loop
    'flag arg should not be a reserved Value
    If ArgVal(1) <= 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If
    'check for read only reserved flags
    CheckResFlagUse ArgVal(1)


  Case 81 'move.obj(oA, X,Y,STEP,fDONE)
    'validate the target position
    If ArgVal(1) > 159 Or ArgVal(2) > 167 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4127)
        Exit Function
      Case leMedium
        AddWarning 5062
      Case leLow
      End Select
    End If
    
    'check for ego object
    If ArgVal(0) = 0 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5045
      Case leLow
      End Select
    End If
    
    'flag arg should not be a reserved Value
    If ArgVal(4) <= 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If
    
    'check for read only reserved flags
    CheckResFlagUse ArgVal(4)


  Case 82 'move.obj.v
    'flag arg should not be a reserved Value
    If ArgVal(4) <= 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If
    
    'check for read only reserved flags
    CheckResFlagUse ArgVal(4)


  Case 83 'follow.ego(oA, DISTANCE, fDONE)
    'validate distance value
    If ArgVal(1) <= 1 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5102
      Case leLow
      End Select
    End If
        
    'check for ego object
    If ArgVal(0) = 0 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5027
      Case leLow
      End Select
    End If
    
    'flag arg should not be a reserved Value
    If ArgVal(2) <= 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If
    'check for read only reserved flags
    CheckResFlagUse ArgVal(2)


  Case 86 'set.dir(oA, vB)
    'check for ego object
    If ArgVal(0) = 0 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5026
      Case leLow
      End Select
    End If


  Case 87 'get.dir
    'variable is second arg
    'variable arg should not be a reserved Value
    If ArgVal(1) <= 26 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If


  Case 90 'block(x1,y1,x2,y2)
    'validate that all are within bounds, and that x1<=x2 and y1<=y2
    'also check that
    If ArgVal(0) > 159 Or ArgVal(1) > 167 Or ArgVal(2) > 159 Or ArgVal(3) > 167 Then
      'bad number
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4129)
        Exit Function
      Case leMedium
        AddWarning 5020
      Case leLow
      End Select
    End If


    If(ArgVal(2) - ArgVal(0) < 2) Or(ArgVal(3) - ArgVal(1) < 2) Then
      'won't work
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4129)
        Exit Function
      Case leMedium
        AddWarning 5051
      Case leLow
      End Select
    End If



  Case 98 'load.sound(A)
    'validate the sound exists
    If Not agSnds.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4130), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5014
      Case leLow
      End Select
    End If


  Case 99 'sound(A)
    'validate the sound exists
    If Not agSnds.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4137), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5084
      Case leLow
      End Select
    End If


  Case 103 'display(ROW,COL,mC)
    'check row/col against limits
    If ArgVal(0) > 24 Or ArgVal(1) > 39 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
       strErrMsg = LoadResString(4131)
       Exit Function
      Case leMedium
        AddWarning 5059
      Case leLow
      End Select
    End If


  Case 105 'clear.lines(TOP,BTM,C)
    'top must be >btm; both must be <=24
    If ArgVal(0) > 24 Or ArgVal(1) > 24 Or ArgVal(0) > ArgVal(1) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4132)
        Exit Function
      Case leMedium
        AddWarning 5011
      Case leLow
      End Select
    End If
    'color value should be 0 or 15 '(but it doesn't hurt to be anything else)
    If ArgVal(2) > 0 And ArgVal(2) <> 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5100
      Case leLow
      End Select
    End If


  Case 109 'set.text.attribute(A,B)
    'should be limited to valid color values (0-15)
    If ArgVal(0) > 15 Or ArgVal(1) > 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4133)
        Exit Function
      Case leMedium
        AddWarning 5029
      Case leLow
      End Select
    End If


  Case 110 'shake.screen(A)
    'shouldn't normally have more than a few shakes; zero is BAD
    If ArgVal(0) = 0 Then
      'error!
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        strErrMsg = LoadResString(4134)
        Exit Function
      Case leLow
      End Select
    ElseIf ArgVal(0) > 15 Then
      'could be a palette change?
      If ArgVal(0) >= 100 And ArgVal(0) <= 109 Then
        'separate warning
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh, leMedium
          AddWarning 5058
        Case leLow
        End Select
      Else
        'warning
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh, leMedium
          AddWarning 5057
        Case leLow
        End Select
      End If
    End If


  Case 111 'configure.screen(TOP,INPUT,STATUS)
    'top should be <=3
    'input and status should not be equal
    'input and status should be <top or >=top+21
    If ArgVal(0) > 3 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4135)
        Exit Function
      Case leMedium
        AddWarning 5044
      Case leLow
      End Select
    End If
    If ArgVal(1) > 24 Or ArgVal(2) > 24 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5099
      Case leLow
      End Select
    End If
    If ArgVal(1) = ArgVal(2) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5048
      Case leLow
      End Select
    End If
    If (ArgVal(1) >= ArgVal(0) And ArgVal(1) <= CLng(ArgVal(0)) + 20) Or(ArgVal(2) >= ArgVal(0) And ArgVal(2) <= CLng(ArgVal(0)) + 20) Then
     Select Case agMainLogSettings.ErrorLevel

     Case leHigh, leMedium
       AddWarning 5049

     Case leLow

     End Select

   End If


 Case 114 'set.string(sA, mB)
    'warn user if setting input prompt to unusually long value

   If ArgVal(0) = 0 Then
     If Len(strMsg(ArgVal(1))) > 10 Then
       Select Case agMainLogSettings.ErrorLevel

       Case leHigh, leMedium
         AddWarning 5096

       Case leLow

       End Select

     End If

   End If


 Case 115 'get.string(sA, mB, ROW,COL,LEN)
    'if row>24, both row/col are ignored; if col>39, gets weird; len is limited automatically to <=40

   If ArgVal(2) > 24 Then
     Select Case agMainLogSettings.ErrorLevel

     Case leHigh, leMedium
       AddWarning 5052

     Case leLow

     End Select

   End If


   If ArgVal(3) > 39 Then
     Select Case agMainLogSettings.ErrorLevel

     Case leHigh

       strErrMsg = LoadResString(4004)

       Exit Function

     Case leMedium

       AddWarning 5080

     Case leLow

     End Select

   End If


   If ArgVal(4) > 40 Then
     Select Case agMainLogSettings.ErrorLevel

     Case leHigh, leMedium
       AddWarning 5056

     Case leLow

     End Select

   End If


 Case 121 'set.key(A,B,cC)
    'controller number limit checked in GetNextArg function
    
    'increment controller Count

   intCtlCount = intCtlCount + 1
    
    'must be ascii or key code, (Arg0 can be 1 to mean joystick)

   If ArgVal(0) > 0 And ArgVal(1) > 0 And ArgVal(0) <> 1 Then
     Select Case agMainLogSettings.ErrorLevel

     Case leHigh

       strErrMsg = LoadResString(4154)

       Exit Function

     Case leMedium

       AddWarning 5065

     Case leLow

     End Select

   End If
    
    'check for improper ASCII assignments

   If ArgVal(1) = 0 Then
     Select Case ArgVal(0) 'ascii codes
      Case 8, 13, 32 'bkspace, enter, spacebar
        'bad
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          strErrMsg = LoadResString(4155)
          Exit Function
        Case leMedium
          AddWarning 5066
        Case leLow
        End Select
      End Select
    End If
    
    'check for improper KEYCODE assignments
    If ArgVal(0) = 0 Then
      Select Case ArgVal(0) 'ascii codes
      Case 71, 72, 73, 75, 76, 77, 79, 80, 81, 82, 83
        'bad
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          strErrMsg = LoadResString(4155)
          Exit Function
        Case leMedium
          AddWarning 5066
        Case leLow
        End Select
      End Select
    End If


  Case 122 'add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
    'VIEW, LOOP & CEL must exist
    'CEL width must be >=3
    'x,y must be within limits
    'PRI must be 0, or >=3 AND <=15
    'MGN must be 0-3, or >3 (ha ha, or ANY value...)
    
    'validate view
    If Not agViews.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4138), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5064
        'dont need to check loops or cels
        blnWarned = True
      Case leLow
      End Select
    End If


    If Not blnWarned Then
      'try to load view to test loop & cel
      On Error Resume Next
      blnUnload = Not agViews(ArgVal(0)).Loaded
      'if error trying to get loaded status, ignore for now
      'it'll show up again when trying to load or access
      'loop property and be handled there
      Err.Clear
      If blnUnload Then
        agViews(ArgVal(0)).Load
      End If
      If Err.Number = 0 Then
        'validate loop
        If ArgVal(1) >= agViews(ArgVal(0)).Loops.Count Then
          Select Case agMainLogSettings.ErrorLevel
          Case leHigh
            strErrMsg = Replace(Replace(LoadResString(4139), ARG1, CStr(ArgVal(1))), ARG2, CStr(ArgVal(0)))
            If blnUnload Then
              agViews(ArgVal(0)).Unload
            End If
            Exit Function
          Case leMedium
            AddWarning 5085
            'dont need to check cel
            blnWarned = True
          Case leLow
          End Select
        End If
        'if loop was valid, check cel
        If Not blnWarned Then
          'validate cel
          If ArgVal(2) >= agViews(ArgVal(0)).Loops(ArgVal(1)).Cels.Count Then
            Select Case agMainLogSettings.ErrorLevel
            Case leHigh
              strErrMsg = Replace(Replace(Replace(LoadResString(4140), ARG1, CStr(ArgVal(2))), ARG2, CStr(ArgVal(1))), ARG3, CStr(ArgVal(0)))
              If blnUnload Then
                agViews(ArgVal(0)).Unload
              End If
              Exit Function
            Case leMedium
              AddWarning 5086
            Case leLow
            End Select
          End If
        End If
      Else
        'can't load the view; add a warning
        Err.Clear
        AddWarning 5021, Replace(LoadResString(5021), ARG1, CStr(ArgVal(0)))
      End If
      If blnUnload Then
        agViews(ArgVal(0)).Unload
      End If
    End If

    On Error GoTo ErrHandler
    
    'x,y must be within limits
    If ArgVal(3) > 159 Or ArgVal(4) > 167 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4141)
        Exit Function
      Case leMedium
        AddWarning 5038
      Case leLow
      End Select
    End If
    
    'PRI should be <=15
    If ArgVal(5) > 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4142)
        Exit Function
      Case leMedium
        AddWarning 5079
      Case leLow
      End Select
    End If
    
    'PRI should be 0 OR >=4 (but doesn't raise an error; only a warning)
    If ArgVal(5) < 4 And ArgVal(5) <> 0 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5079
      Case leLow
      End Select
    End If
    
    'MGN values >15 will only use lower nibble
    If ArgVal(6) > 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5101
      Case leLow
      End Select
    End If


  Case 129 'show.obj(VIEW)
    'validate view
    If Not agViews.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4144), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5061
      Case leLow
      End Select
    End If


  Case 127, 176, 178  'init.disk, hide.mouse, show.mouse
    Select Case agMainLogSettings.ErrorLevel
    Case leHigh, leMedium
      AddWarning 5087, Replace(LoadResString(5087), ARG1, agCmds(CmdNum).Name)
    Case leLow
    End Select


  Case 175, 179, 180 'discard.sound, fence.mouse, mouse.posn
    Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4152), ARG1, agCmds(CmdNum).Name)
        Exit Function
      Case leMedium
      AddWarning 5088, Replace(LoadResString(5088), ARG1, agCmds(CmdNum).Name)
    Case leLow
    End Select


  Case 130 'random(LOWER,UPPER,vRESULT)
    'lower should be < upper
    If ArgVal(0) > ArgVal(1) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4145)
        Exit Function
      Case leMedium
        AddWarning 5054
      End Select
    End If
    
    'lower=upper means result=lower=upper
    If ArgVal(0) = ArgVal(1) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5106
      Case leLow
      End Select
    End If
    
    'if lower=upper+1, means div by 0!
    If ArgVal(0) = ArgVal(1) + 1 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4158)
        Exit Function
      Case leMedium
        AddWarning 5107
      End Select
    End If
    
    'variable arg should not be a reserved Value
    If ArgVal(2) <= 26 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
      Case leLow
      End Select
    End If


  Case 142 'script.size
    'raise warning/error if in other than logic0
    If bytLogComp<> 0 Then
      'warn
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        'set warning
        AddWarning 5039
      Case leLow
        'no action
      End Select
    End If
    'check for absurdly low Value for script size
    If ArgVal(0) < 10 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5009
      Case leLow
      End Select
    End If


  Case 147 'reposition.to(oA, B,C)
    'validate the new position
    If ArgVal(1) > 159 Or ArgVal(2) > 167 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4128)
        Exit Function
      Case leMedium
        AddWarning 5023
      Case leLow
      End Select
    End If


  Case 150 'trace.info(LOGIC,ROW,HEIGHT)
    'logic must exist
    'row + height must be <22
    'height must be >=2 (but interpreter checks for this error)
    
    'validate that logic exists
    If Not agLogs.Exists(ArgVal(0)) Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = Replace(LoadResString(4153), ARG1, CStr(ArgVal(0)))
        Exit Function
      Case leMedium
        AddWarning 5040
      Case leLow
      End Select
    End If
    'validate that height is not too small
    If ArgVal(2) < 2 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5046
      Case leLow
      End Select
    End If
    'validate size of window
    If CLng(ArgVal(1)) + CLng(ArgVal(2)) > 23 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4146)
        Exit Function
      Case leMedium
        AddWarning 5063
      Case leLow
      End Select
    End If


  Case 151, 152 'Print.at(mA, ROW, COL, MAXWIDTH), print.at.v
    'row <=22
    'col >=2
    'maxwidth <=36
    'maxwidth=0 defaults to 30
    'maxwidth=1 crashes AGI
    'col + maxwidth <=39
    If ArgVal(1) > 22 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4147)
        Exit Function
      Case leMedium
        AddWarning 5067
      Case leLow
      End Select
    End If


    Select Case ArgVal(3)
    Case 0 'maxwidth=0 defaults to 30
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5105
      Case leLow
      End Select


    Case 1 'maxwidth=1 crashes AGI
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4043)
        Exit Function
      Case leMedium
        AddWarning 5103
      Case leLow
      End Select


    Case Is > 36 'maxwidth >36 won't work
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4043)
        Exit Function
      Case leMedium
        AddWarning 5104
      Case leLow
      End Select
   End Select
    
    'col>2 and col + maxwidth <=39
    If ArgVal(2) < 2 Or CLng(ArgVal(2)) + CLng(ArgVal(3)) > 39 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4148)
        Exit Function
      Case leMedium
        AddWarning 5068
      Case leLow
      End Select
    End If


  Case 154 'clear.text.rect(R1,C1,R2,C2,COLOR)
    'If either row argument is >24,
    'or either column argument is >39,
    'or R2 < R1 or C2 < C1,
    'the results are unpredictable
    If ArgVal(0) > 24 Or ArgVal(1) > 39 Or _
       ArgVal(2) > 24 Or ArgVal(3) > 39 Or _
       ArgVal(2) < ArgVal(0) Or ArgVal(3) < ArgVal(1) Then
      'invalid items
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4150)
        Exit Function
      Case leMedium
        'if due to pos2 < pos1
        If ArgVal(2) < ArgVal(0) Or ArgVal(3) < ArgVal(1) Then
          AddWarning 5069
        End If
        'if due to variables outside limits
        If ArgVal(0) > 24 Or ArgVal(1) > 39 Or _
           ArgVal(2) > 24 Or ArgVal(3) > 39 Then
          AddWarning 5070
        End If
      End Select
    End If
    
    'color value should be 0 or 15 '(but it doesn't hurt to be anything else)
    If ArgVal(4) > 0 And ArgVal(4) <> 15 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5100
      Case leLow
      End Select
    End If


  Case 158 'submit.menu()
    'should only be called in logic0
    'raise warning/error if in other than logic0
    If bytLogComp <> 0 Then
      'warn
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        'set warning
        AddWarning 5047
      Case leLow
      End Select
    End If


  Case 174 'set.pri.base(A)
    'calling set.pri.base with Value >167 doesn't make sense
    If ArgVal(0) > 167 Then
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh, leMedium
        AddWarning 5071
      Case leLow
      End Select
    End If
  End Select

  'success
  ValidateArgs = True
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Function ValidateIfArgs(ByVal CmdNum As Long, ByRef ArgVal() As Byte) As Boolean

  'check for specific command issues


  Select Case CmdNum
  Case 9 'has (iA)
    'invobj number validated in GetNextArg function


  Case 10 'obj.in.room(iA, vB)
    'invobj number validated in GetNextArg function
        
  Case 11, 16, 17, 18 'posn(oA, X1, Y1, X2, Y2)
                      'obj.in.box(oA, X1, Y1, X2, Y2)
                      'center.posn(oA, X1, Y1, X2, Y2)
                      'right.posn(oA, X1, Y1, X2, Y2)
                      
    'screenobj number validated in GetNextArg function
    
    'validate that all are within bounds, and that x1<=x2 and y1<=y2
    If ArgVal(1) > 159 Or ArgVal(2) > 167 Or ArgVal(3) > 159 Or ArgVal(4) > 167 Then
      'bad number
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4151)
        Exit Function
      Case leMedium
        AddWarning 5072
      Case leLow
      End Select
    End If


    If(ArgVal(1) > ArgVal(3)) Or(ArgVal(2) > ArgVal(4)) Then
      'can't work
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4151)
        Exit Function
      Case leMedium
        AddWarning 5073
      Case leLow
      End Select
    End If


  Case 12 'controller (cA)
    'has controller been assigned?
    'not sure how to check it; calls to controller cmd may
    'occur in logics that are compiled before the logic that sets
    'them up...
    
  Case 14 'said()
  Case 15 'compare.strings(sA, sB)
  
  End Select

  'success
  ValidateIfArgs = True
End Function

Private Function ValidateMsgChars(ByVal strMsg As String, ByVal MsgNum As Long) As Boolean

  'raise error/warning, depending on setting
  
  'return TRUE if OK or only a warning;  FALSE means error found


  Dim i As Long
  Dim blnWarn5093 As Boolean, blnWarn5094 As Boolean
  
  'if LOW errdetection, EXIT
  If agMainLogSettings.ErrorLevel = leLow Then
    ValidateMsgChars = True
    Exit Function
  End If


  For i = 1 To Len(strMsg)
    'check for invalid codes (0,8,9,10,13)
    Select Case AscW(Mid(strMsg, i, 1))
    Case 0, 8, 9, 10, 13
      'warn user
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4005)
        blnError = True
        Exit Function
      Case leMedium
        If Not blnWarn5093 Then
          AddWarning 5093
          blnWarn5093 = True
          'need to track warning in case this msg is
          'also included in body of logic
          intMsgWarn(MsgNum) = intMsgWarn(MsgNum) Or 1
        End If
      End Select
      
    'extended character
    Case Is > 127
      Select Case agMainLogSettings.ErrorLevel
      Case leHigh
        strErrMsg = LoadResString(4006)
        blnError = True
        Exit Function
      Case leMedium
        If Not blnWarn5094 Then
          AddWarning 5094
          blnWarn5094 = True
          'need to track warning in case this msg is
          'also included in body of logic
          intMsgWarn(MsgNum) = intMsgWarn(MsgNum) Or 2
        End If
      End Select
    End Select
   Next i
   
   'msg is OK
   ValidateMsgChars = True
End Function

Private Function VariableValue(strVar As String) As Integer
  'this function will extract the variable number from
  'an input variable string
  'the input string should be of the form #, a# or *a#
  ' where a is a valid variable prefix (v, f, s, m, w, c)
  'and # is 0-255
  'if the result is invalid, this function returns -1


  Dim strVarVal As String
  Dim intVarVal  As Integer
  Dim blnOutofBounds As Boolean


  On Error GoTo ErrHandler
  
  'if not numeric
  If Not IsNumeric(strVar) Then
    'strip off variable prefix, and indirection
    'if indirection
    If Left$(strVar, 1) = "*" Then
      strVarVal = Right$(strVar, Len(strVar) - 2)
    Else
      strVarVal = Right$(strVar, Len(strVar) - 1)
    End If
  Else
    'use the input Value
    strVarVal = strVar
  End If
  
  'if result is a number
  If IsNumeric(strVarVal) Then
    'get number
    intVarVal = Val(strVarVal)
    'for word only, subtract one to
    'account for '1' based word data type
    '(i.e. w1 is first word, but command uses arg Value of '0')
    If AscW(strVar) = 119 Then
      intVarVal = intVarVal - 1
    End If
    
    'verify within bounds  0-255
    If intVarVal >= 0 And intVarVal <= 255 Then
      'return this Value
      VariableValue = intVarVal
      Exit Function
    End If
  End If
  
  'error- return -1
  VariableValue = -1
Exit Function

ErrHandler:
  Err.Clear
  'return -1
  VariableValue = -1
End Function
Private Function WriteMsgs() As Boolean
  'this function will write the messages for a logic at the end of
  'the resource.
  'messages are encrypted with the string 'Avis Durgan'. No gaps
  'are allowed, so messages that are skipped must be included as
  'zero length messages

  Dim lngMsgSecStart As Long
  Dim lngMsgSecLen As Long
  Dim lngMsgPos(255) As Long
  Dim intCharPos As Integer, bytCharVal As Byte
  Dim lngMsg As Long
  Dim lngMsgCount As Integer
  Dim lngCryptStart As Long
  Dim lngMsgLen As Integer
  Dim i As Integer, strHex As String
  Dim blnSkipNull As Boolean, blnSkipChar As Boolean


  On Error GoTo ErrHandler
  
  'calculate start of message section
  lngMsgSecStart = tmpLogRes.Size
  
  'find last message by counting backwards until a msg is found
  lngMsgCount = 256
  Do
    lngMsgCount = lngMsgCount - 1
  Loop Until blnMsg(lngMsgCount) Or(lngMsgCount = 0)
  
  'write msg Count,
  tmpLogRes.WriteByte CByte(lngMsgCount)
  
  'write place holder for msg end
  tmpLogRes.WriteWord 0&
  'write place holders for msg pointers
  For i = 1 To lngMsgCount
    tmpLogRes.WriteWord 0&
  Next i
  
  'begin encryption process
  lngCryptStart = tmpLogRes.Size
  For lngMsg = 1 To lngMsgCount

    'always reset the 'NoNull' feature
    blnSkipNull = False

    'get length
    lngMsgLen = Len(strMsg(lngMsg))
    'if msg not used
    If blnMsg(lngMsg) Then
      'calculate offset to start of this message (adjust by one byte, which
      'is the byte that indicates how many msgs there are)
      lngMsgPos(lngMsg) = tmpLogRes.GetPos - (lngMsgSecStart + 1)
    Else
      '*'Debug.Assert strMsg(lngMsg) = vbNullString
      'need to write a null value for offset; (when it gets added after all
      'messages are written it gets set to the beginning of message section
      ' ( a relative offset of zero here)
      lngMsgPos(lngMsg) = 0
    End If
    If lngMsgLen > 0 Then
      'step through all characters in this msg
      intCharPos = 1
      Do Until intCharPos > Len(strMsg(lngMsg))
        'get ascii code for this character
        bytCharVal = Asc(Mid$(strMsg(lngMsg), intCharPos))
        'check for invalid codes (8,9,10,13)
        Select Case bytCharVal
        Case 0, 8, 9, 10, 13
          'convert these chars to space to avoid trouble
          bytCharVal = 32
          
        Case 92 '"\"
          'check for special codes
          If(intCharPos<lngMsgLen) Then
         Select Case AscW(Mid$(strMsg(lngMsg), intCharPos + 1))
            Case 110, 78 'n or N'
              ' \n = new line
              bytCharVal = &HA
              intCharPos = intCharPos + 1


            Case 34 'dbl quote(")'
              '\" = quote mark (chr$(34))
              bytCharVal = &H22
              intCharPos = intCharPos + 1


            Case 92 '\'
              '\\ = \
              bytCharVal = &H5C
              intCharPos = intCharPos + 1


            Case 48 '0'
              '\0 = don't add null terminator
              blnSkipNull = True
              'also skip this char
              blnSkipChar = True
              intCharPos = intCharPos + 1


            Case 120 'x'  'look for a hex value
              'make sure at least two more characters
              If intCharPos + 2 < lngMsgLen Then
                'get next 2 chars and hexify them
                strHex = "&H" & Mid(strMsg(lngMsg), intCharPos + 2, 2)
                
                'if this hex value >=1 and <256, use it
                i = Val(strHex)
                If i >= 1 And i< 256 Then
                  bytCharVal = i
                  intCharPos = intCharPos + 3
                End If
              End If
            Case Else
              'if no special char found, the single slash should be dropped
              blnSkipChar = True
            End Select
          Else
            'if the '\' is the last char, skip it
            blnSkipChar = True
          End If
        End Select

        'write the encrypted byte (need to adjust for previous messages, and current position)
        If Not blnSkipChar Then
          tmpLogRes.WriteByte bytCharVal Xor bytEncryptKey((tmpLogRes.GetPos - lngCryptStart) Mod 11)
        End If
        'increment pointer
        intCharPos = intCharPos + 1
        'reset skip flag
        blnSkipChar = False
      Loop
    End If
    
    'if msg was used, add trailing zero to terminate message
    '(if msg was zero length, we still need this terminator)
    If blnMsg(lngMsg) Then
      If Not blnSkipNull Then
        tmpLogRes.WriteByte &H0 Xor bytEncryptKey((tmpLogRes.GetPos - lngCryptStart) Mod 11)
      End If
    End If
  Next lngMsg
  
  'calculate length of msg section, and write it at beginning
  'of msg section (adjust by one byte, which is the
  'byte that indicates number of msgs written)
  lngMsgSecLen = tmpLogRes.GetPos - (lngMsgSecStart + 1)
  tmpLogRes.WriteWord lngMsgSecLen, lngMsgSecStart + 1
  
  'write msg section start Value at beginning of resource
  '(correct by two so it gives position relative to byte 7 of
  'the logic resource header - see procedure 'DecodeLogic' for details)
  tmpLogRes.WriteWord lngMsgSecStart - 2, 0

  'write all the msg pointers
  '
  For lngMsg = 1 To lngMsgCount
    tmpLogRes.WriteWord lngMsgPos(lngMsg), lngMsgSecStart + 1 + lngMsg* 2
  Next lngMsg
  
  'and return true
  WriteMsgs = True

Exit Function

ErrHandler:
  'any errors, means there is a problem
  blnError = True
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "WriteMsgs")
  Err.Clear
  WriteMsgs = False
End Function



Private Function ReadLabels() As Boolean

  Dim i As Byte
  Dim intLabel As Integer
  Dim strLabel As String
  Dim rtn As Long


  On Error GoTo ErrHandler
  'this function steps through the source code to identify all valid labels; we need to find
  'them all before starting the compile so we can correctly set jumps
  '
  'valid syntax is either 'label:' or ':label:', with nothing else in front of or after
  'the label declaration
  
  'reset counter
  bytLabelCount = 0
  
  'reset compiler to first input line
  ResetCompiler

  Do
    'look for label name
    If InStr(1, strCurrentLine, ":") <> 0 Then
      strLabel = Trim$(Replace(strCurrentLine, vbTab, " "))
      'check for 'label:'
      If Right(strLabel, 1) = ":" Then
        strLabel = RTrim(Left(strLabel, Len(strLabel) - 1))
      ElseIf Left(strLabel, 1) = ":" Then
        strLabel = LTrim(Right(strLabel, Len(strLabel) - 1))
      Else
        'not a label
        strLabel = vbNullString
      End If
    
      'if a label was found, validate it
      If Len(strLabel) <> 0 Then
        'make sure enough room
        If bytLabelCount >= MAX_LABELS Then
          strErrMsg = Replace(LoadResString(4109), ARG1, CStr(MAX_LABELS))
          Exit Function
        End If


        rtn = ValidateDefName(strLabel)
        'numbers are ok for labels
        If rtn = 2 Then
          rtn = 0
        End If
        If rtn<> 0 Then
          'error
          Select Case rtn
          Case 1
            strErrMsg = LoadResString(4096)
          Case 3
            strErrMsg = Replace(LoadResString(4025), ARG1, strLabel)
          Case 4
            strErrMsg = Replace(LoadResString(4026), ARG1, strLabel)
          Case 5
            strErrMsg = Replace(LoadResString(4028), ARG1, strLabel)
          Case 6
            strErrMsg = LoadResString(4091)
          Case 7
            strErrMsg = Replace(LoadResString(4024), ARG1, strLabel)
          Case 8
            strErrMsg = Replace(LoadResString(4033), ARG1, strLabel)
          Case 9
            strErrMsg = Replace(LoadResString(4030), ARG1, strLabel)
          Case 10
            strErrMsg = Replace(LoadResString(4029), ARG1, strLabel)
          Case 11
            strErrMsg = Replace(LoadResString(4032), ARG1, strLabel)
          Case 12
            strErrMsg = Replace(LoadResString(4031), ARG1, strLabel)
          Case 13
            strErrMsg = LoadResString(4068)
          End Select
          Exit Function
        End If
        
        'no periods allowed either
        If InStr(1, strLabel, ".") <> 0 Then
          strErrMsg = LoadResString(4068)
          Exit Function
        End If
          
        'check label against current list of labels
        If bytLabelCount > 0 Then
          For i = 1 To bytLabelCount
            If strLabel = llLabel(i).Name Then
              strErrMsg = Replace(LoadResString(4027), ARG1, strLabel)
              Exit Function
            End If
          Next i
        End If

        'increment number of labels, and save
        bytLabelCount = bytLabelCount + 1
        llLabel(bytLabelCount).Name = strLabel
        llLabel(bytLabelCount).Loc = 0
      End If
    End If
    
    'get next line
    IncrementLine
  Loop Until lngLine = -1
  ReadLabels = True
Exit Function
ErrHandler:
  strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG1, "ReadLabels")
  Err.Clear
End Function
Private Function CompileAGI() As Boolean
  'main compiler function
  'steps through input one command at a time and converts it
  'to AGI logic code
  'Note that we don't need to set blnError flag here;
  'an error will cause this function to return a Value of false
  'which causes the compiler to display error info
  
  Const MaxGotos = 255

  Dim strNextCmd As String
  Dim strPrevCmd As String
  Dim strArg As String, bytArg(7) As Byte
  Dim i As Integer
  Dim intCmdNum As Integer
  Dim BlockStartDataLoc(MAX_BLOCK_DEPTH) As Integer
  Dim BlockDepth As Integer
  Dim BlockIsIf(MAX_BLOCK_DEPTH) As Boolean
  Dim BlockLength(MAX_BLOCK_DEPTH) As Integer
  Dim CurLabel As Integer
  Dim intLabelNum As Integer
  Dim Gotos(MaxGotos) As LogicGoto
  Dim NumGotos As Integer
  Dim GotoData As Long
  Dim CurGoto As Integer
  Dim blnLastCmdRtn As Boolean
  Dim lngReturnLine As Long


  On Error GoTo ErrHandler
  
  'initialize variables
  BlockDepth = 0
  NumGotos = 0
  'reset compiler
  ResetCompiler

  blnError = False
  
  'get first command
  strNextCmd = NextCommand()
    
  'process commands in the input string list until finished
  Do Until lngLine = -1
    'reset last command flag
    blnLastCmdRtn = False
    lngReturnLine = 0
    
    'process the command
    Select Case strNextCmd
    Case "{"
      'can't have a "{" command, unless it follows an 'if' or 'else'
      If strPrevCmd<> "if" And strPrevCmd <> "else" Then
        strErrMsg = LoadResString(4008)
        Exit Function
      End If


    Case "}"
      'if no block currently open,
      If BlockDepth = 0 Then
        strErrMsg = LoadResString(4010)
        Exit Function
      End If
      'if last command was a new.room command, then closing block is expected
      If blnNewRoom Then
        blnNewRoom = False
      End If
      'if last position in resource is two bytes from start of block
      If tmpLogRes.Size = BlockStartDataLoc(BlockDepth) + 2 Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          strErrMsg = LoadResString(4049)
          Exit Function
        Case leMedium
          'set warning
          AddWarning 5001
        Case leLow
          'no action
        End Select
      End If
      'calculate and write block length
      BlockLength(BlockDepth) = tmpLogRes.Size - BlockStartDataLoc(BlockDepth) - 2
      tmpLogRes.WriteWord CLng(BlockLength(BlockDepth)), CLng(BlockStartDataLoc(BlockDepth))
      'remove block from stack
      BlockDepth = BlockDepth - 1
    Case "if"
      'compile the 'if' statement
      If Not CompileIf() Then
        Exit Function
      End If
      'if block stack exceeded
      If BlockDepth >= MAX_BLOCK_DEPTH Then
        strErrMsg = Replace(LoadResString(4110), ARG1, CStr(MAX_BLOCK_DEPTH))
        Exit Function
      End If
      'add block to stack
      BlockDepth = BlockDepth + 1
      BlockStartDataLoc(BlockDepth) = tmpLogRes.GetPos
      BlockIsIf(BlockDepth) = True
      'write placeholders for block length
      tmpLogRes.WriteWord &H0
      
      'next command better be a bracket
      strNextCmd = NextCommand()
      If strNextCmd<> "{" Then
        'error!!!!
        strErrMsg = LoadResString(4053)
        Exit Function
      End If


    Case "else"
      'else can only follow a close bracket
      If strPrevCmd <> "}" Then
        strErrMsg = LoadResString(4011)
        Exit Function
      End If
      
      'if the block closed by that bracket was an 'else'
      '(which will be determined by having that block's IsIf flag NOT being set),
      If Not BlockIsIf(BlockDepth + 1) Then
        strErrMsg = LoadResString(4083)
        Exit Function
      End If
      
      'adjust blockdepth to the 'if' command
      'directly before this 'else'
      BlockDepth = BlockDepth + 1
      'adjust previous block length to accomodate the 'else' statement
      BlockLength(BlockDepth) = BlockLength(BlockDepth) + 3
      tmpLogRes.WriteWord CLng(BlockLength(BlockDepth)), CLng(BlockStartDataLoc(BlockDepth))
      'previous 'if' block is now closed; use same block level
      'for this 'else' block
      BlockIsIf(BlockDepth) = False
      'write the 'else' code
      tmpLogRes.WriteByte &HFE
      BlockStartDataLoc(BlockDepth) = tmpLogRes.GetPos
      tmpLogRes.WriteWord &H0  ' block length filled in later.
      
      'next command better be a bracket
      strNextCmd = NextCommand()
      If strNextCmd <> "{" Then
        'error!!!!
        strErrMsg = LoadResString(4053)
        Exit Function
      End If


    Case "goto"
      'if last command was a new room, warn user
      If blnNewRoom Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh, leMedium
          'set warning
          AddWarning 5095
        Case leLow
          'no action
        End Select
        blnNewRoom = False
      End If
      
      'next command should be "("
      If NextChar() <> "(" Then
        strErrMsg = LoadResString(4001)
        Exit Function
      End If
      'get goto argument
      strArg = NextCommand()
      
      'if argument is NOT a valid label
      If LabelNum(strArg) = 0 Then
        strErrMsg = Replace(LoadResString(4074), ARG1, strArg)
        Exit Function
      End If
      'if too many gotos
      If NumGotos >= MaxGotos Then
        strErrMsg = Replace(LoadResString(4108), ARG1, CStr(MaxGotos))
      End If
      'save this goto info on goto stack
      NumGotos = NumGotos + 1
      Gotos(NumGotos).LabelNum = LabelNum(strArg)
      'write goto command byte
      tmpLogRes.WriteByte &HFE
      Gotos(NumGotos).DataLoc = tmpLogRes.GetPos
      'write placeholder for amount of offset
      tmpLogRes.WriteWord &H0
      'next character should be ")"
      If NextChar() <> ")" Then
        strErrMsg = LoadResString(4003)
        Exit Function
      End If
      'verify next command is end of line (;)
      If NextChar() <> ";" Then
        blnError = True
        strErrMsg = LoadResString(4007)
        Exit Function
      End If
    
      'since block commands are no longer supported, check for markers in order to provide a
      'meaningful error message
    Case "/*", "* /"
      blnError = True
      strErrMsg = LoadResString(4052)
      Exit Function


    Case "++", "--" 'unary operators; need to get a variable next
      'write the command code
      If strNextCmd = "++" Then
        tmpLogRes.WriteByte 1
      Else
        tmpLogRes.WriteByte 2
      End If
      
      'get the variable to update
      strArg = NextCommand()
      'convert it
      If Not ConvertArgument(strArg, atVar, False) Then
        'error
        blnError = True
        '*'Debug.Assert False
        strErrMsg = LoadResString(4046)
        Exit Function
      End If
      'get Value
      intCmdNum = VariableValue(strArg)
      If intCmdNum = -1 Then
        blnError = True
        '*'Debug.Assert False
        strErrMsg = Replace(LoadResString(4066), "%1", "")
        Exit Function
      End If
      'write the variable value
      tmpLogRes.WriteByte CByte(intCmdNum)
      'verify next command is end of line (;)
      If NextChar(True) <> ";" Then
        strErrMsg = LoadResString(4007)
        Exit Function
      End If


    Case ":"  'alternate label syntax
      'get next command; it should be the label
      strNextCmd = NextCommand()
      intLabelNum = LabelNum(strNextCmd)
      'if not a valid label
      If intLabelNum = 0 Then
        strErrMsg = LoadResString(4076)
        Exit Function
      End If
      'save position of label
      llLabel(intLabelNum).Loc = tmpLogRes.Size


    Case Else
      'must be a label, command, or special syntax
      'if next character is a colon
      If Mid$(strCurrentLine, lngPos + 1, 1) = ":" Then
        'it's a label
        intLabelNum = LabelNum(strNextCmd)
        'if not a valid label
        If intLabelNum = 0 Then
          strErrMsg = LoadResString(4076)
          Exit Function
        End If
        'save position of label
        llLabel(intLabelNum).Loc = tmpLogRes.Size
        'read in next char to skip past the colon
        NextChar
      Else
        'if last command was a new room (and not followed by return(), warn user
        If blnNewRoom And strNextCmd<> "return" Then
          Select Case agMainLogSettings.ErrorLevel
          Case leHigh, leMedium
            'set warning
            AddWarning 5095
          Case leLow
            'no action
          End Select
          blnNewRoom = False
        End If
        
        'get number of command
        intCmdNum = CommandNum(False, strNextCmd)
        'if invalid version
        If intCmdNum = 254 Then
          'raise error
          strErrMsg = Replace(LoadResString(4065), ARG1, strNextCmd)
          Exit Function
        'if command not found,
        ElseIf intCmdNum = 255 Then  ' not found
          'try to parse special syntax
          If CompileSpecial(strNextCmd) Then
            'check for error
            If blnError Then
              Exit Function
            End If
          Else
            'unknown command
            strErrMsg = Replace(LoadResString(4116), ARG1, strNextCmd)
            Exit Function
          End If
        Else
          'write the command code,
          tmpLogRes.WriteByte CByte(intCmdNum)
          'next character should be "("
          If NextChar() <> "(" Then
            strErrMsg = LoadResString(4048)
            Exit Function
          End If
          
          'reset the quotemark error flag
          lngQuoteAdded = -1
          
          'now extract arguments,
          For i = 0 To agCmds(CByte(intCmdNum)).ArgCount - 1
            'after first argument, verify comma separates arguments
            If i > 0 Then
              If NextChar(True) <> "," Then
                'check for added quotes; they are the problem
                If lngQuoteAdded >= 0 Then
                  'reset line;
                  lngLine = lngQuoteAdded
                  lngErrLine = lngLine - lngIncludeOffset
                  'string error
                  strErrMsg = LoadResString(4051)
                Else
                  'use 1-base arg values
                  strErrMsg = Replace(LoadResString(4047), ARG1, CStr(i + 1))
                End If
                Exit Function
              End If
            End If
            bytArg(i) = GetNextArg(agCmds(CByte(intCmdNum)).ArgType(i), i)
            'if error
            If blnError Then
              ' if error number is 4054
              If Val(strErrMsg) = 4054 Then
                ' add command name to error string
                strErrMsg = Replace(strErrMsg, ARG2, agCmds(intCmdNum).Name)
              End If
              Exit Function
            End If
            
            'write argument
            tmpLogRes.WriteByte bytArg(i)
          Next i
          
          'validate arguments for this command
          If Not ValidateArgs(intCmdNum, bytArg()) Then
            Exit Function
          End If
          
          'next character must be ")"
          If NextChar() <> ")" Then
            blnError = True
            'check for added quotes; they are the problem
            If lngQuoteAdded >= 0 Then
              'reset line;
              lngLine = lngQuoteAdded
              lngErrLine = lngLine - lngIncludeOffset
              'string error
              strErrMsg = LoadResString(4051)
            Else
              strErrMsg = LoadResString(4160)
            End If
            Exit Function
          End If
          If intCmdNum = 0 Then
            blnLastCmdRtn = True
            'set line number
            If lngReturnLine = 0 Then
              lngReturnLine = lngLine + 1
            End If
          End If
        End If
        
        'verify next command is end of line (;)
        If NextChar(True) <> ";" Then
          strErrMsg = LoadResString(4007)
          Exit Function
        End If
      End If
     End Select
    'get next command
    strPrevCmd = strNextCmd
    strNextCmd = NextCommand()
 Loop

  If(Not blnLastCmdRtn) Then
   Select Case agMainLogSettings.ErrorLevel

   Case leHigh
      'no rtn error

     strErrMsg = LoadResString(4102)

     Exit Function

   Case leMedium
      'add a return code

     tmpLogRes.WriteByte 0

   Case leLow
      'add a return code

     tmpLogRes.WriteByte 0
      'and a warning

     AddWarning 5016

   End Select

 End If
  
  'check to see if everything was wrapped up properly

 If BlockDepth > 0 Then
   strErrMsg = LoadResString(4009)
    'reset errorline to return cmd

   lngErrLine = lngReturnLine

   Exit Function

 End If
  
  'write in goto values

 For CurGoto = 1 To NumGotos

   GotoData = llLabel(Gotos(CurGoto).LabelNum).Loc - Gotos(CurGoto).DataLoc - 2

   If GotoData < 0 Then
      'need to convert it to an unsigned integer Value

     GotoData = &H10000 + GotoData

   End If

   tmpLogRes.WriteWord CLng(GotoData), CLng(Gotos(CurGoto).DataLoc)
  Next CurGoto
  
  'return true
  CompileAGI = True
Exit Function

ErrHandler:
  'if error is an app specific error, just pass it along; otherwise create
  'an app specific error to encapsulate whatever happened
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  If(lngError And vbObjectError) = vbObjectError Then
    'pass it along
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
  Else
    On Error GoTo 0: Err.Raise vbObjectError + 658, strErrSrc, Replace(LoadResString(658), ARG1, CStr(lngError) & ":" & strError)
  End If
End Function

Private Function MessageNum(strMsgIn As String) As Long
  ' Returns the number of the message corresponding to
  'strMsg, or creates a new msg number if strMsg is not
  'currently a message
  'if maximum number of msgs assigned, returns  0
  Dim lngMsg As Long
  
  'blank msgs normally not allowed
  If LenB(strMsgIn) = 0 Then
    Select Case agMainLogSettings.ErrorLevel
    Case leHigh, leMedium
      AddWarning 5074
    Case leLow
      'allow it
    End Select
  End If


  For lngMsg = 1 To 255
    'if this is the message
    '(use StrComp, since this is a case-sensitive search)
    If StrComp(strMsg(lngMsg), strMsgIn, vbBinaryCompare) = 0 Then
      'return this Value
      MessageNum = lngMsg
      'if null string found for first time, msg-in-use flag will be false
      If Not blnMsg(lngMsg) Then
        blnMsg(lngMsg) = True
      End If
      'if this msg has an extended char warning, repeat it here
      If(intMsgWarn(lngMsg) And 1) = 1 Then
       AddWarning 5093
      End If
      If(intMsgWarn(lngMsg) And 2) = 2 Then
       AddWarning 5094
      End If
      Exit Function
    End If
  Next lngMsg
  
  'msg doesn't exist; find an empty spot
  For lngMsg = 1 To 255
    If Not blnMsg(lngMsg) Then
      'this message is available
      blnMsg(lngMsg) = True
      strMsg(lngMsg) = strMsgIn
      
      'check for invalid characters
      If Not ValidateMsgChars(strMsgIn, lngMsg) Then
        'return a value to indicate error
        MessageNum = -1
      Else
        MessageNum = lngMsg
      End If


      Exit Function
    End If
  Next lngMsg
  
  'if no room found, return zero
  MessageNum = 0
End Function

Private Function CommandNum(blnIF As Boolean, strCmdName As String) As Byte
  'gets the command number
  'of a command, based on the text

  If blnIF Then
    For CommandNum = 0 To agNumTestCmds
      If strCmdName = agTestCmds(CommandNum).Name Then
        Exit Function
      End If
    Next CommandNum
  Else
    For CommandNum = 0 To agNumCmds
      If strCmdName = agCmds(CommandNum).Name Then
        Exit Function
      End If
    Next CommandNum
    'maybe the command is a valid agi command, but
    'just not supported in this agi version
    For CommandNum = agNumCmds + 1 To 182
      If strCmdName = agCmds(CommandNum).Name Then
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          'error; return cmd Value of 254 so compiler knows to raise error
          CommandNum = 254
        Case leMedium
          'add warning
          AddWarning 5075, Replace(LoadResString(5075), ARG1, strCmdName)
        Case leLow
          'don't worry about command validity; return the extracted command num
        End Select


        Exit Function
      End If
    Next CommandNum
  End If

  CommandNum = 255
End Function

Private Function CompileSpecialIf(strArg1 As String, blnNOT As Boolean) As Boolean
  'this funtion determines if strArg1 is a properly
  'formatted special test syntax
  'and writes the appropriate data to the resource
  
  '(proprerly formatted special IF syntax will be one of following:
  ' v## expr v##
  ' v## expr ##
  ' f##
  ' v##
  '
  ' where ## is a number from 1-255
  ' and expr is '"==", "!=", "<", ">=", "=>", ">", "<=", "=<"
  
  'none of the possible test commands in special syntax format need to be validated,
  'so no call to ValidateIfArgs


  Dim strArg2 As String
  Dim intArg1 As Integer, intArg2 As Integer
  Dim blnArg2Var As Boolean
  Dim blnAddNOT As Boolean
  Dim bytCmdNum As Byte


  On Error GoTo ErrHandler
  
  'check for variable argument
  If Not ConvertArgument(strArg1, atVar) Then
    'check for flag argument
    If Not ConvertArgument(strArg1, atFlag) Then
      'invalid argument
      blnError = True
      strErrMsg = Replace(LoadResString(4039), ARG1, strArg1)
      Exit Function
    End If
  End If
  
  'arg in can only be f# or v#
  Select Case Left$(strArg1, 1)
  Case "f"
    'get flag argument Value
    intArg1 = VariableValue(strArg1)
    'if invalid flag number
    If intArg1 = -1 Then
      'invalid number
        blnError = True
        strErrMsg = Replace(LoadResString(4066), ARG1, "1")
      Exit Function
    End If
    'write isset cmd
    tmpLogRes.WriteByte &H7  ' isset
    tmpLogRes.WriteByte CByte(intArg1)


  Case "v"
    'arg 1 must be 'v#' format
    intArg1 = VariableValue(strArg1)
    
    'if invalid variable number
    If intArg1 = -1 Then
      'invalid number
        blnError = True
        strErrMsg = LoadResString(4086)
      Exit Function
    End If
    
    'get comparison expression
    strArg2 = NextCommand()
    'get command code for this expression
    Select Case strArg2
    Case EQUAL_TOKEN
      bytCmdNum = &H1
    Case NOTEQUAL_TOKEN
      bytCmdNum = &H1
      blnAddNOT = True
    Case ">"
      bytCmdNum = &H5
    Case "<=", "=<"
      bytCmdNum = &H5
      blnAddNOT = True
    Case "<"
      bytCmdNum = &H3
    Case ">=", "=>"
      bytCmdNum = &H3
      blnAddNOT = True
    Case ")", "&&", "||"
      'means we are doing a boolean test of the variable;
      'use greatern with zero as arg
      
      'write command, and arguments
      tmpLogRes.WriteByte &H5
      tmpLogRes.WriteByte CByte(intArg1)
      tmpLogRes.WriteByte CByte(0)
      
      'backup the compiler pos so we get the next command properly
      lngPos = lngPos - Len(strArg2)
      'return true
      CompileSpecialIf = True
      Exit Function

    Case Else
      blnError = True
      strErrMsg = LoadResString(4078)
      Exit Function
    End Select
    
    'before getting second arg, check for NOT symbol in front of a variable
    'can't have a NOT in front of variable comparisons
    If blnNOT Then
      blnError = True
      strErrMsg = LoadResString(4098)
      Exit Function
    End If
    
    'get second argument (numerical or variable)
    blnArg2Var = True
    'reset the quotemark error flag
    lngQuoteAdded = -1
    intArg2 = GetNextArg(atNum, -1, blnArg2Var)
    'if error
    If blnError Then
      'if an invalid arg value found
      If Val(strErrMsg) = 4063 Then
        'change error message
        strErrMsg = Mid(strErrMsg, 55, InStrRev(strErrMsg, "'") - 53)
        strErrMsg = Replace(LoadResString(4089), ARG1, strErrMsg)
      Else
        strErrMsg = Replace(LoadResString(4089), ARG1, "")
      End If
      Exit Function
    End If
     
    'if comparing to a variable,
    If blnArg2Var Then
      bytCmdNum = bytCmdNum + 1
    End If
    
    'if adding a 'not'
    If blnAddNOT Then
      tmpLogRes.WriteByte(&HFD)
    End If
    
    'write command, and arguments
    tmpLogRes.WriteByte bytCmdNum
    tmpLogRes.WriteByte CByte(intArg1)
    tmpLogRes.WriteByte CByte(intArg2)
  End Select
    
  'return true
  CompileSpecialIf = True
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Function CompileSpecial(strArgIn As String) As Boolean
  'strArg1 should be a variable in the format of *v##, v##, f## or s##
  'if it is not, this function will trap it and return an error
  'the expression after strArg1 should be one of the following:
  ' =, +=, -=, *=, /=, ++, --
  '
  'after determining assignment type, this function will validate additional
  'arguments as necessary
  '


  Dim strArg1 As String, strArg2 As String
  Dim intArg1 As Integer, intArg2 As Integer
  Dim blnArg2Var  As Boolean
  Dim intDir As Integer '0 = no indirection; 1 = left; 2 = right
  Dim bytCmd As Byte, bytArgs() As Byte


  On Error GoTo ErrHandler


  strArg1 = strArgIn
  
  'assume this is a special syntax until proven otherwise
  CompileSpecial = True
  
  'if this is indirection
  If Left$(strArg1, 1) = "*" Then
    'left indirection
    '     *v# = #
    '     *v# = v#
    
    'next char can't be a space, newline, or tab
    Select Case Mid$(strCurrentLine, lngPos + 1, 1)
    Case " ", vbTab, vbNullString
      'error
      blnError = True
      strErrMsg = LoadResString(4105)
      Exit Function
    End Select
    
    'get actual first arg
    intArg1 = GetNextArg(atVar, -1)
    'if error
    If blnError Then
      'adjust error message
      strErrMsg = LoadResString(4064)
      Exit Function
    End If


    intDir = 1
    'next character must be "="
    strArg2 = NextCommand()
    If strArg2<> "=" Then
      'error
      blnError = True
      strErrMsg = LoadResString(4105)
      Exit Function
    End If
    
  'if this arg is string
  ElseIf ConvertArgument(strArg1, atStr) Then
    'string assignment
    '     s# = m#
    '     s# = "<string>"
    
    'get string variable number
    intArg1 = VariableValue(strArg1)


    If agMainLogSettings.ErrorLevel<> leLow Then
      'for version 2.089, 2.272, and 3.002149 only 12 strings
      Select Case agIntVersion
      Case "2.089", "2.272", "3.002149"
        If intArg1 > 11 Then
          Select Case agMainLogSettings.ErrorLevel
          Case leHigh
            'use 1-based arg values
            strErrMsg = Replace(Replace(LoadResString(4079), ARG1, "1"), ARG2, "11")
          Case leMedium
            AddWarning 5007, Replace(LoadResString(5007), ARG1, "11")
          End Select
        End If
    
      'for all other versions, limit is 24 strings
      Case Else
        If intArg1 > 23 Then
          Select Case agMainLogSettings.ErrorLevel
          Case leHigh
            strErrMsg = Replace(Replace(LoadResString(4079), ARG1, "1"), ARG2, "23")
          Case leMedium
            AddWarning 5007, Replace(LoadResString(5007), ARG1, "23")
          End Select
        End If
      End Select
    End If
    
    'check for equal sign
    strArg2 = NextCommand()
    'if not equal sign
    If strArg2<> "=" Then
      'error
      blnError = True
      strErrMsg = LoadResString(4034)
      Exit Function
    End If
    'get actual second variable
    'use argument extractor in case
    'second variable is a literal string)
    intArg2 = GetNextArg(atMsg, -1)
    'if error
    If blnError Then
      ' if error number is 4054
      If Val(strErrMsg) = 4054 Then
        ' change it to 4058
        strErrMsg = LoadResString(4058)
      End If
      
      'just exit
      Exit Function
    End If
    
    'write set.string cmd
    bytCmd = &H72
  
  'if this is a variable
  ElseIf ConvertArgument(strArg1, atVar) Then
    'arg 1 must be 'v#' format
    intArg1 = VariableValue(strArg1)
    
    'if invalid variable number
    If intArg1 = -1 Then
      'invalid number
        blnError = True
        strErrMsg = LoadResString(4085)
      Exit Function
    End If
  
    'variable assignment or arithmetic operation
    'need next command to determine what kind of assignment/operation
    strArg2 = NextCommand()


    Select Case strArg2
    Case "++"
      ' v#++;
      bytCmd = &H1
    Case "+="
      ' v# += #; or v# += v#;
      bytCmd = &H5
    Case "--"
      ' v#--
      bytCmd = &H2
    Case "-="
      ' v# -= #; or v# -= v#;
      bytCmd = &H7
    Case "*="
      ' v# *= #; or v# *= v#;
      bytCmd = &HA5
    Case "/="
      ' v# /= #; v# /= v#
      bytCmd = &HA7
    Case "="
      'right indirection
      '     v# = *v#;
      'assignment
      '     v# = v#;
      '     v# = #;
      'long arithmetic operation
      '     v# = v# + #; v# = v# + v#;
      '     v# = v# - #; v# = v# - v#;
      '     v# = v# * #; v# = v# * v#;
      '     v# = v# / #; v# = v# / v#;


    Case Else
      'don't know what the heck it is...
      blnError = True
      strErrMsg = LoadResString(4034)
      Exit Function
    End Select
    
  'check for flag assignment
  ElseIf ConvertArgument(strArg1, atFlag) Then
    'flag assignment
    '     f# = True;
    '     f# = False;
    
    'get flag number
    intArg1 = VariableValue(strArg1)
    
    'check for equal sign
    strArg2 = NextCommand()
    'if not equal sign
    If strArg2 <> "=" Then
      'error
      blnError = True
      strErrMsg = LoadResString(4034)
      Exit Function
    End If
    
    'get flag Value
    strArg2 = NextCommand()


    Select Case LCase$(strArg2)
    Case "true"
      'set this flag
      bytCmd = &HC


    Case "false"
      'reset this flag
      bytCmd = &HD


    Case Else
      'error
      blnError = True
      strErrMsg = LoadResString(4034)
      'always exit
      Exit Function
    End Select


  Else
    'not a special syntax
    CompileSpecial = False
    Exit Function
  End If
  
  'skip check for second argument if cmd is known to be a single arg
  'command (increment/decrement/reset/set
  '(set string is also skipped because second arg is already determined)
  Select Case bytCmd
  Case &H1, &H2, &HC, &HD, &H72
  Case Else
    'get next argument
    strArg2 = NextCommand()
    'if it is indirection
    If strArg2 = "*" Then
      'if not already using left indirection, AND cmd is not known
      If intDir = 0 And bytCmd = 0 Then
        'set right indirection
        intDir = 2
        
        'next char can't be a space, newline, or tab
        Select Case Mid$(strCurrentLine, lngPos + 1, 1)
        Case " ", vbTab, vbNullString
          'error
          blnError = True
          strErrMsg = LoadResString(4105)
          Exit Function
        End Select
        
        'get actual variable
        intArg2 = GetNextArg(atVar, -1)
        If blnError Then
          'reset error string
          strErrMsg = LoadResString(4105)
          Exit Function
        End If
      Else
        'error
        blnError = True
        strErrMsg = LoadResString(4105)
        Exit Function
      End If
    Else
      'arg2 is either number or variable- convert input to standard syntax
      
      'if it's a number, check for negative value
      If Val(strArg2) < 0 Then
        'valid negative numbers are -1 to -128
        If Val(strArg2) < -128 Then
          'error
          blnError = True
          strErrMsg = LoadResString(4095)
          Exit Function
        End If
        'convert it to 2s-compliment unsigned value by adding it to 256
        strArg2 = CStr(256 + Val(strArg2))
        '*'Debug.Assert Val(strArg2) >= 128 And Val(strArg2) <= 255
  
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh, leMedium
          'show warning
          AddWarning 5098
        End Select
      End If


      blnArg2Var = True
      If Not ConvertArgument(strArg2, atNum, blnArg2Var) Then
        'set error
        blnError = True
        strErrMsg = Replace(LoadResString(4088), ARG1, strArg2)
        Exit Function
      End If
      
      'it's a number or variable; verify it's 0-255
      intArg2 = VariableValue(strArg2)
      'if invalid
      If intArg2 = -1 Then
        'set error
        blnError = True
        strErrMsg = Replace(LoadResString(4088), ARG1, strArg2)
        Exit Function
      End If
      
      'if arg2 is a number
      If Not blnArg2Var Then
        'if cmd is not yet known,
        If bytCmd = 0 Then
          'must be assign
          ' v# = #
          ' *v# = #
          If intDir = 1 Then
            'lindirect.n
            bytCmd = &HB
          Else
            'assign.n
            bytCmd = &H3
          End If
        End If
      End If
    End If ' not indirection
  End Select 'if not inc/dec
  
  'if command is not known
  If bytCmd = 0 Then
    'if arg values are the same (already know arg2 is a variable)
    'and no indirection
    If(intArg1 = intArg2) And intDir = 0 Then
      'check for long arithmetic
      strArg2 = NextCommand()
      'if end of command is reached
      If strArg2 = ";" Then
        'move pointer back one space so eol
        'check in CompileAGI works correctly
        lngPos = lngPos - 1
        
        'this is a simple assign (with a variable being assigned to itself!!)
        Select Case agMainLogSettings.ErrorLevel
        Case leHigh
          blnError = True
          strErrMsg = LoadResString(4084)
          Exit Function
        Case leMedium
          AddWarning 5036
        Case leLow
          'allow it
        End Select
        bytCmd = &H3
      Else
        'this may be long arithmetic
        Select Case strArg2
        Case "+"
          bytCmd = &H5
        Case "-"
          bytCmd = &H7
        Case "*"
          bytCmd = &HA5
        Case "/"
          bytCmd = &HA7
        Case Else
          'error
          blnError = True
          strErrMsg = LoadResString(4087)
          Exit Function
        End Select
        
        'now get actual second argument
        blnArg2Var = True
        intArg2 = GetNextArg(atNum, -1, blnArg2Var)
        'if error
        If blnError Then
          If Val(strErrMsg) = 4063 Then
            'change error message
            strErrMsg = Mid(strErrMsg, 55, InStrRev(strErrMsg, "'") - 53)
            strErrMsg = Replace(LoadResString(4161), ARG1, strErrMsg)
          Else
            strErrMsg = Replace(LoadResString(4161), ARG1, "")
          End If
          Exit Function
        End If
      End If
    Else
      'variables are different
      'must be assignment
      ' v# = v#
      ' *v# = v#
      ' v# = *v#
      Select Case intDir
      Case 0  'assign.v
        bytCmd = &H4
      Case 1 'lindirect.v
        bytCmd = &H9
      Case 2  'rindirect
        bytCmd = &HA
        blnArg2Var = False
      End Select
      'always reset arg2var flag so
      'command won't be adjusted later
        blnArg2Var = False
    End If
  End If
  
  'if second argument is a variable
  If blnArg2Var Then
    bytCmd = bytCmd + 1
  End If
  
  'get next command on this line
  strArg2 = NextCommand(True)
  
  'check that next command is semicolon
  If strArg2<> ";" Then
    blnError = True
    'check for added quotes; they are the problem
    If lngQuoteAdded >= 0 Then
      'reset line;
      lngLine = lngQuoteAdded
      lngErrLine = lngLine - lngIncludeOffset
      'string error
      strErrMsg = LoadResString(4051)
    Else
      strErrMsg = LoadResString(4007)
    End If
    Exit Function
  Else
    'move pointer back one space so
    'eol check in CompileAGI works
    'correctly
    lngPos = lngPos - 1
  End If
  
  'need to validate arguments for this command
  Select Case bytCmd
  Case &H1, &H2, &HC, &HD
    'single arg commands
    ReDim bytArgs(0)
    bytArgs(0) = intArg1
  Case 0
    '*'Debug.Assert False
  Case Else
    'two arg commands
    ReDim bytArgs(1)
    bytArgs(0) = intArg1
    bytArgs(1) = intArg2
  End Select
  
  'validate commands before writing
  If Not ValidateArgs(bytCmd, bytArgs) Then
    CompileSpecial = False
    Exit Function
  End If
  
  'write command and arg1
  tmpLogRes.WriteByte bytCmd
  tmpLogRes.WriteByte CByte(intArg1)
  'write second argument for all cmds except &H1, &H2, &HC, &HD
  Select Case bytCmd
  Case &H1, &H2, &HC, &HD
  Case Else
    tmpLogRes.WriteByte CByte(intArg2)
  End Select
Exit Function

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  'if error is an app specific error, just pass it along; otherwise create
  'an app specific error to encapsulate whatever happened
  If (lngError And vbObjectError) = vbObjectError Then
    'pass it along
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
  Else
    On Error GoTo 0: Err.Raise vbObjectError + 659, strErrSrc, Replace(LoadResString(659), ARG1, CStr(lngError) & ":" & strError)
  End If
End Function

Private Function LabelNum(LabelName As String) As Byte
  'this function will return the number of the label passed
  'as a string,
  'or zero, if a match is not found


  Dim i As Integer
  
  'step through all labels,
  For i = 1 To bytLabelCount
    If llLabel(i).Name = LabelName Then
      LabelNum = i
      Exit Function
    End If
  Next i
  
  'if not found, zero is returned
End Function


*/
  }
}
