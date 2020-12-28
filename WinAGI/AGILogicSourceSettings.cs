namespace WinAGI
{
  // should this be public? or internal?
  public class AGILogicSourceSettings
  {
    private bool agElseAsGoto;
    private LogicErrorLevel agErrorLevel;
    private bool agShowMsgs;
    private bool agSpecialSyntax;

    void temp()
    {
      /*
      Private strErrSource As String

      Public Property Let ErrorLevel(NewLevel As LogicErrorLevel)

        agErrorLevel = NewLevel
      End Property

      Public Property Get ErrorLevel() As LogicErrorLevel

        ErrorLevel = agErrorLevel
      End Property

      Public Property Get ArgTypePrefix(ByVal Index As Byte) As String
        'validate index
        If Index < 0 Or Index > 8 Then
          On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
        End If

        ArgTypePrefix = agArgTypPref(Index)

      End Property


      Public Property Get ArgTypeName(ByVal Index As Byte) As String
        'validate index
        If Index < 0 Or Index > 8 Then
          On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
        End If

        ArgTypeName = agArgTypName(Index)
      End Property


      Public Property Let IgnoreWarning(ByVal WarningNumber As Long, ByVal NewVal As Boolean)
        'validate index
        If WarningNumber < 5001 Or WarningNumber > 5000 + WARNCOUNT Then
          On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
        End If

        agNoCompWarn(WarningNumber - 5000) = NewVal
      End Property

      Public Property Get IgnoreWarning(ByVal WarningNumber As Long) As Boolean
        'validate index
        If WarningNumber < 5001 Or WarningNumber > 5000 + WARNCOUNT Then
          On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
        End If

        IgnoreWarning = agNoCompWarn(WarningNumber - 5000)
      End Property

      Public Property Get ReservedDefines(ByVal ArgType As ArgTypeEnum) As TDefine()
        'returns the reserved defines that match this argtype as an array of defines
        'NOT the same as reporting by 'group' (which is used for saving changes to resdef names)
        Dim i As Long
        Dim tmpDefines() As TDefine

        On Error GoTo ErrHandler

        Select Case ArgType
        Case atNum
          'return all numerical reserved defines
          ReDim tmpDefines(44)
          For i = 0 To 4
            tmpDefines(i) = agEdgeCodes(i)
          Next i
          For i = 0 To 8
            tmpDefines(i + 5) = agEgoDir(i)
          Next i
          For i = 0 To 4
            tmpDefines(i + 14) = agVideoMode(i)
          Next i
          For i = 0 To 8
            tmpDefines(i + 19) = agCompType(i)
          Next i
          For i = 0 To 15
            tmpDefines(i + 28) = agResColor(i)
          Next i

          tmpDefines(44).Name = agResDef(4).Name
          'if a game is not loaded
          If Not agMainGame.GameLoaded Then
            tmpDefines(44).Value = 0
          Else
            tmpDefines(44).Value = agInvObj.Count
          End If

        Case atVar
          'return all variable reserved defines
          ReDim tmpDefines(26)
          tmpDefines = agResVar

        Case atFlag
          'return all flag reserved defines
          ReDim tmpDefines(17)
          tmpDefines = agResFlag

        Case atMsg
          'none
          ReDim tmpDefines(0)

        Case atSObj
          'return all screen object reserved defines
          ReDim tmpDefines(0)
          tmpDefines(0).Name = agResDef(0).Name
          tmpDefines(0).Value = agResDef(0).Value
          tmpDefines(0).Type = atSObj

        Case atIObj
          'none
          ReDim tmpDefines(0)

        Case atStr
          'one
          ReDim tmpDefines(0)
          tmpDefines(0).Name = agResDef(5).Name
          tmpDefines(0).Value = agResDef(5).Value
          tmpDefines(0).Type = atStr

        Case atWord
          'none
          ReDim tmpDefines(0)

        Case atCtrl
          'none
          ReDim tmpDefines(0)

        Case atDefStr
          'return all reserved string defines
          ReDim tmpDefines(2)
          tmpDefines(0).Name = agResDef(1).Name
          tmpDefines(0).Value = Chr$(34) & agMainGame.GameVersion & Chr$(34)
          tmpDefines(0).Type = atDefStr
          tmpDefines(1).Name = agResDef(2).Name
          tmpDefines(1).Value = Chr$(34) & agMainGame.GameAbout & Chr$(34)
          tmpDefines(1).Type = atDefStr
          tmpDefines(2).Name = agResDef(3).Name
          If agGameLoaded Then
            tmpDefines(2).Value = Chr$(34) & agMainGame.GameID & Chr$(34)
          Else
            tmpDefines(2).Value = Chr$(34) & Chr$(34)
          End If
          tmpDefines(2).Type = atDefStr

        Case atVocWrd
          'none
          ReDim tmpDefines(0)

        End Select

        'return the defines
        ReservedDefines = tmpDefines
      Exit Property

      ErrHandler:
        '*'Debug.Assert False
        'should never get an error
      End Property

      Public Property Get ResDefByGrp(ByVal Group As Integer) As TDefine()

        'this returns the reserved defines by their 'group' instead by by variable type

        'can i refer to the arrays? does that copy them or let me directly affect them? hmmm
        On Error GoTo ErrHandler

        Select Case Group
        Case 1 'var
          ResDefByGrp = agResVar()

        Case 2 'flag
          ResDefByGrp = agResFlag()

        Case 3 'edgecodes
          ResDefByGrp = agEdgeCodes()

        Case 4 'direction
          ResDefByGrp = agEgoDir()

        Case 5 'vidmode
          ResDefByGrp = agVideoMode()

        Case 6 'comp type
          ResDefByGrp = agCompType()

        Case 7 'colors
          ResDefByGrp = agResColor()

        Case 8 'other
          ResDefByGrp = agResDef()

        Case Else
          'raise error
          On Error GoTo 0: Err.Raise 9, strErrSource
          Exit Property
        End Select
      Exit Property

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Property

      Public Property Let ResDef(ByVal DefType As Integer, ByVal DefIndex As Integer, ByVal DefName As String)

        'this property lets user update a reserved define name;
        'it is up to calling procedure to make sure there are no conflicts
        'if the define value doesn't match an actual reserved item, error is raised

        On Error GoTo ErrHandler

        'type is a numeric value that maps to the six different types(catgories) of reserved defines
        Select Case DefType
        Case 1 'variable
          'value must be 0-26
          If DefIndex < 0 Or DefIndex > 27 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the resvar name
          agResVar(DefIndex).Name = DefName

        Case 2 'flag
          'value must be 0-17
          If DefIndex < 0 Or DefIndex > 17 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the resflag name
          agResFlag(DefIndex).Name = DefName

        Case 3 'edgecode
          'value must be 0-4
          If DefIndex < 0 Or DefIndex > 4 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the edgecode name
          agEdgeCodes(DefIndex).Name = DefName

        Case 4 'direction
          'value must be 0-8
          If DefIndex < 0 Or DefIndex > 8 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the direction name
          agEgoDir(DefIndex).Name = DefName

        Case 5 'vidmode
          'value must be 0-4
          If DefIndex < 0 Or DefIndex > 4 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the vidmode name
          agVideoMode(DefIndex).Name = DefName

        Case 6 'comptypes
          'value must be 0-8
          If DefIndex < 0 Or DefIndex > 8 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the comptype name
          agCompType(DefIndex).Name = DefName


        Case 7 'color
          'value must be 0-15
          If DefIndex < 0 Or DefIndex > 15 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the color resdef name
          agResColor(DefIndex).Name = DefName


        Case 8 'other
          'value must be 0-5
          If DefIndex < 0 Or DefIndex > 5 Then
            'raise error
            On Error GoTo 0: Err.Raise 9, strErrSource
            Exit Property
          End If
          'change the other-resdef name
          agResDef(DefIndex).Name = DefName

        Case Else
          'error!
            On Error GoTo 0: Err.Raise 9, strErrSource
        End Select

      Exit Property

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Property

      Public Sub ResetResDefines()

        AssignReservedDefines
      End Sub

      Public Property Let ReservedAsText(ByVal NewState As Boolean)

        'if true, reserved variables and flags show up as text when decompiling
        agResAsText = NewState
      End Property



      Public Property Get ReservedAsText() As Boolean

        'if true, reserved variables and flags show up as text when decompiling
        ReservedAsText = agResAsText
      End Property
      Public Property Let SourceExt(NewExt As String)

        'must start with a period
        If AscW(NewExt) <> 46 Then
          agSrcExt = "." & NewExt
        Else
          agSrcExt = NewExt
        End If
      End Property
      Public Property Get SourceExt() As String

        SourceExt = agSrcExt
      End Property

      Public Property Get UseReservedNames() As Boolean

        'if true, predefined variables and flags are used during compilation
        UseReservedNames = agUseRes
      End Property
      Public Property Let UseReservedNames(ByVal NewValue As Boolean)

        'if true, predefined variables and flags are used during compilation
        agUseRes = NewValue
        If agGameLoaded Then
          WriteGameSetting "General", "UseResNames", agUseRes
        End If

      End Property

      Public Property Let SpecialSyntax(ByVal NewState As Boolean)

          agSpecialSyntax = NewState
      End Property



      Public Property Get SpecialSyntax() As Boolean

          SpecialSyntax = agSpecialSyntax
      End Property




      Public Property Let ElseAsGoto(ByVal NewState As Boolean)

          agElseAsGoto = NewState
      End Property


      Public Property Get ElseAsGoto() As Boolean

          ElseAsGoto = agElseAsGoto
      End Property




      Public Property Let ShowAllMessages(ByVal NewState As Boolean)

          agShowMsgs = NewState
      End Property



      Public Property Get ShowAllMessages() As Boolean

          ShowAllMessages = agShowMsgs
      End Property




      Public Function ValidateResDefs() As Boolean

        'makes sure reserved defines are OK- replace any bad defines with their defaults

        Dim i As Long

        On Error GoTo ErrHandler

        'assume OK
        ValidateResDefs = True

        'step through all vars
        For i = 0 To UBound(agResVar())
          If Not ValidateName(agResVar(i)) Then
            agResVar(i).Name = agResVar(i).Default
            ValidateResDefs = False
          End If
        Next i

        'step through all flags
        For i = 0 To UBound(agResFlag())
          If Not ValidateName(agResFlag(i)) Then
            agResFlag(i).Name = agResFlag(i).Default
            ValidateResDefs = False
          End If
        Next i

        'step through all edgecodes
        For i = 0 To UBound(agEdgeCodes())
          If Not ValidateName(agEdgeCodes(i)) Then
            agEdgeCodes(i).Name = agEdgeCodes(i).Default
            ValidateResDefs = False
          End If
        Next i

        'step through all directions
        For i = 0 To UBound(agEgoDir())
          If Not ValidateName(agEgoDir(i)) Then
            agEgoDir(i).Name = agEgoDir(i).Default
            ValidateResDefs = False
          End If
        Next i

        'step through all vidmodes
        For i = 0 To UBound(agVideoMode())
          If Not ValidateName(agVideoMode(i)) Then
            agVideoMode(i).Name = agVideoMode(i).Default
            ValidateResDefs = False
          End If
        Next i

        'step through all comp types
        For i = 0 To UBound(agCompType())
          If Not ValidateName(agCompType(i)) Then
            agCompType(i).Name = agCompType(i).Default
            ValidateResDefs = False
          End If
        Next i

        'step through all colors
        For i = 0 To UBound(agResColor())
          If Not ValidateName(agResColor(i)) Then
            agResColor(i).Name = agResColor(i).Default
            ValidateResDefs = False
          End If
        Next i

        'step through all other defines
        For i = 0 To UBound(agResDef())
          If Not ValidateName(agResDef(i)) Then
            agResDef(i).Name = agResDef(i).Default
            ValidateResDefs = False
          End If
        Next i
      Exit Function

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Function

      Private Function ValidateName(TestDef As TDefine) As Boolean
        'validates if a reserved define name is agreeable or not
        'returns TRUE if ok, FALSE if not

        Dim i As Long
        Dim tmpDefines() As TDefine
        Dim NewDefName As String

        On Error GoTo ErrHandler

        'get name to test
        NewDefName = TestDef.Name

        'if already at default, just exit
        If TestDef.Name = TestDef.Default Then
          ValidateName = True
          Exit Function
        End If

        'if no name,
        If LenB(NewDefName) = 0 Then
          ValidateName = False
          Exit Function
        End If

        'name cant be numeric
        If IsNumeric(NewDefName) Then
          ValidateName = False
          Exit Function
        End If

        'check against regular commands
        For i = 0 To UBound(agCmds)
          If StrComp(NewDefName, agCmds(i).Name, vbTextCompare) = 0 Then
            ValidateName = False
            Exit Function
          End If
        Next i

        'check against test commands
        For i = 0 To UBound(agTestCmds)
          If StrComp(NewDefName, agTestCmds(i).Name, vbTextCompare) = 0 Then
            ValidateName = False
            Exit Function
          End If
        Next i

        'check against keywords
        If StrComp(NewDefName, "if", vbTextCompare) = 0 Or StrComp(NewDefName, "else", vbTextCompare) = 0 Or StrComp(NewDefName, "goto", vbTextCompare) = 0 Then
          ValidateName = False
          Exit Function
        End If

        'check against variable/flag/controller/string/message names
        Select Case Asc(LCase$(NewDefName))
        '     v    f    m    o    i    s    w    c
        Case 118, 102, 109, 111, 105, 115, 119, 99
          If IsNumeric(Right(NewDefName, Len(NewDefName) - 1)) Then
            ValidateName = False
            Exit Function
          End If
        End Select

        'check against reserved variables
        tmpDefines = ReservedDefines(atVar)
        For i = 0 To UBound(tmpDefines)
          If tmpDefines(i).Value <> TestDef.Value Then
            If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
              ValidateName = False
              Exit Function
            End If
          End If
        Next i

        tmpDefines = ReservedDefines(atFlag)
        For i = 0 To UBound(tmpDefines)
          If tmpDefines(i).Value <> TestDef.Value Then
            If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
              ValidateName = False
              Exit Function
            End If
          End If
        Next i

        tmpDefines = ReservedDefines(atNum)
        For i = 0 To UBound(tmpDefines)
          If tmpDefines(i).Value <> TestDef.Value Then
            If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
              ValidateName = False
              Exit Function
            End If
          End If
        Next i

        tmpDefines = ReservedDefines(atSObj)
        For i = 0 To UBound(tmpDefines)
          If tmpDefines(i).Value <> TestDef.Value Then
            If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
              ValidateName = False
              Exit Function
            End If
          End If
        Next i

        tmpDefines = ReservedDefines(atDefStr)
        For i = 0 To UBound(tmpDefines)
          If tmpDefines(i).Value <> TestDef.Value Then
            If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
              ValidateName = False
              Exit Function
            End If
          End If
        Next i

        'check name against improper character list
        For i = 1 To Len(NewDefName)
          Select Case Asc(Mid(NewDefName, i, 1))
          '!"#$%&'()*+,-/:;<=>?@[\]^`{|}~
          Case 32 To 45, 47, 58 To 64, 91 To 94, 96, Is >= 123
            ValidateName = False
            Exit Function
          End Select
        Next i

        'OK!
        ValidateName = True
      Exit Function

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Function

      Private Sub Class_Initialize()

        strErrSource = "WinAGI.agiLogicSourceSettings"

        'set default source extension

        'assign arg type info

      End Sub
      */
    }
  }
}