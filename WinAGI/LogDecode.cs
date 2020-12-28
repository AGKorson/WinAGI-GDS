using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  public static partial class WinAGI
  {
    internal static List<string> DecodeLogic(byte[] ResData)
    {
      return new List<string> { "abc" };

    }
    static void tmp_logdecode()
    {
      /*
      Option Explicit

        Private Const MAX_LINE_LEN = 80
        Public Const MAX_BLOCK_DEPTH = 64 'defined in LogCompile

        Public Type BlockType
          IsIf As Boolean
          EndPos As Long
          Length As Long
          IsOutside As Boolean
          JumpPos As Long
        End Type

        Private lngPos As Long

        Private bytBlockDepth As Byte
        Private Block(MAX_BLOCK_DEPTH) As BlockType

        Private intArgStart As Integer

        Private bytLabelCount As Integer
        Private lngLabelPos() As Long

        Private lngMsgSecStart As Long
        Private stlMsgs As StringList
        Private blnMsgUsed(0 To 255) As Boolean
        Private blnMsgExists(0 To 255) As Boolean

        'tokens for building source code output
        Private Const NOT_TOKEN As String = "!"
        Private Const IF_TOKEN = "if ("
        Private Const THEN_TOKEN = ")%1{" 'where %1 is a line feed plus indent at current level
        Private Const ELSE_TOKEN = "else%1{" 'where %1 is a line feed plus indent at current level
        Private Const ENDIF_TOKEN = "}"
        Private Const GOTO_TOKEN = "goto(%1)"
        Private Const EOL_TOKEN = ";"
        Private Const AND_TOKEN = " && "
        Private Const OR_TOKEN = " || "
        Private Const EQUAL_TEST_TOKEN = " == "
        Private Const NOT_EQUAL_TEST_TOKEN = " != "
        Private Const CMT_TOKEN = "[ "
        Private Const MSG_TOKEN = "#message %1 %2"


        Private strError As String
        Private blnWarning As Boolean
        Private strWarning As String



      Private Sub AddDecodeWarning(ByVal WarningText As String)

        'if at least one warning already,
        If blnWarning Then
          'add pipe character
          strWarning = strWarning & "|"

        Else
          'set warning flag
          blnWarning = True
        End If

        strWarning = strWarning & WarningText

      End Sub
      Private Function ArgValue(ByVal ArgNum As Byte, ByVal ArgType As ArgTypeEnum, Optional ByVal ArgComp As Long = -1) As String

        Dim i As Long

        On Error GoTo ErrHandler

        'if not showing reserved names (or if not using reserved defines)
        ' AND not a msg (always substitute msgs)
        If (Not agResAsText || Not agUseRes) && ArgType <> atMsg Then
          'return simple Value
          ArgValue = agArgTypPref(ArgType) & CStr(ArgNum)
          Exit Function
        End If

        'add appropriate resdef name

        Select Case ArgType
        Case atNum
          Select Case ArgComp
          Case 2, 5  'edgecode
            If ArgNum <= 4 Then
              ArgValue = agEdgeCodes(ArgNum).Name
            Else
              ArgValue = CStr(ArgNum)
            End If

          Case 6  'egodir
            If ArgNum <= 8 Then
              ArgValue = agEgoDir(ArgNum).Name
            Else
              ArgValue = CStr(ArgNum)
            End If

          Case 20  'computer type
            If ArgNum <= 8 Then
              ArgValue = agCompType(ArgNum).Name
            Else
              ArgValue = CStr(ArgNum)
            End If

          Case 26 'video
            If ArgNum <= 4 Then
              ArgValue = agVideoMode(ArgNum).Name
            Else
              ArgValue = CStr(ArgNum)
            End If
          Case Else
            'use default
            ArgValue = CStr(ArgNum)
          End Select

        Case atVar
          'if a predefined,
          If ArgNum <= 26 Then
            ArgValue = agResVar(ArgNum).Name
          Else
            'not a reserved data type
            ArgValue = "v" & CStr(ArgNum)
          End If

        Case atFlag
          'if a predefined
          If ArgNum <= 16 Then
            ArgValue = agResFlag(ArgNum).Name
          'check for special case of f20 (only if version 3.002102 or higher)
          ElseIf ArgNum = 20 && Val(agIntVersion) >= 3.002102 Then
            ArgValue = agResFlag(17).Name
          Else
            'not a reserved data type
            ArgValue = "f" & CStr(ArgNum)
          End If

        Case atMsg
          'if this message exists,
          If blnMsgExists(ArgNum) Then
            'begin by using entire message as the chunk to add to current line
            ArgValue = stlMsgs(CLng(ArgNum - 1))
          Else
            'message doesn't exist
            Select Case agMainLogSettings.ErrorLevel
            Case leHigh
              strError = "Undefined message (" & CStr(ArgNum) & ")  at position " & CStr(lngPos)
              Exit Function
            Case leMedium
              'store as number
              ArgValue = "m" & CStr(ArgNum)
              'set warning
              AddDecodeWarning "unknown message: " & CStr(ArgNum) & " at position " & CStr(lngPos)
            Case leLow
              'store as number
              ArgValue = "m" & CStr(ArgNum)
            End Select
          End If
          blnMsgUsed(ArgNum) = True

        Case atSObj
          'if ego
          If ArgNum = 0 Then
            ArgValue = "ego"
          Else
            'not a reserved data type
            ArgValue = "o" & CStr(ArgNum)
          End If

        Case atIObj
          'if a game is loaded AND OBJECT file is loaded,
          If agGameLoaded && agInvObj.Loaded Then
            If ArgNum < agInvObj.Count Then
              'if object is unique
              If agInvObj(CLng(ArgNum)).Unique Then
                'double check if item is a question mark
                If agInvObj(CLng(ArgNum)).ItemName = "?" Then
                  'use the inventory item number, and post a warning
                  ArgValue = "i" & CStr(ArgNum)
                  AddDecodeWarning "reference to invalid inventory object ('?')"
                Else
                  'a unique, non-questionmark item- use it's string Value
                  ArgValue = QUOTECHAR & Replace(agInvObj(CLng(ArgNum)).ItemName, QUOTECHAR, "\""") & QUOTECHAR
                End If
              Else
                'use obj number instead
                ArgValue = "i" & CStr(ArgNum)
                If agMainLogSettings.ErrorLevel <> leLow Then
                  AddDecodeWarning "non-unique object: '" & agInvObj(CLng(ArgNum)).ItemName & "'"
                End If
              End If
            Else
              Select Case agMainLogSettings.ErrorLevel
              Case leHigh
                strError = ("Unknown inventory item (" & CStr(CLng(ArgNum)) & ")")
                Exit Function
              Case leMedium
                'just use the number
                ArgValue = "i" & CStr(ArgNum)
                'set warning
                AddDecodeWarning "unknown inventory item: " & CStr(ArgNum)
              Case leLow
                'just use the number
                ArgValue = "i" & CStr(ArgNum)
              End Select
            End If
          Else
            'always refer to the object by number
            ArgValue = "i" & CStr(ArgNum)
          End If

        Case atStr
          If ArgNum = 0 Then
            ArgValue = agResDef(5).Name
          Else
            'not a reserved data type
            ArgValue = "s" & CStr(ArgNum)
          End If

        Case atCtrl
          'not a reserved data type
          ArgValue = "c" & CStr(ArgNum)

        Case atWord
          'convert argument to a 'one-based' Value
          'so it is consistent with the syntax used
          'in the agi 'print' commands
          ArgValue = "w" & CStr(ArgNum + 1)

        End Select
      Exit Function

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Function

      Private Function ReadMessages(bytData() As Byte, lngMsgStart As Long) As Boolean

        Dim lngEndMsgSection As Long
        Dim MessageStart(255) As Long
        Dim intCurMsg As Integer
        Dim EncryptionStart As Long
        Dim strMessage As String
        Dim blnEndOfMsg As Boolean
        Dim bytInput As Byte
        Dim NumMessages As Integer

        On Error GoTo ErrHandler

        'NOTE: There is no message 0 (this is not supported by the file format).
        ' the word which corresponds to message 0 offset is used to hold the
        'end of text ptr so AGI can decrypt the message text when the logic
        'is initially loaded

        'set position to beginning of msg section,
        lngPos = lngMsgStart

        'set message section end initially to msgsection start
        lngEndMsgSection = lngMsgStart

        Set stlMsgs = New StringList

        'read in number of messages
        NumMessages = bytData(lngPos)
        lngPos = lngPos + 1
        If NumMessages > 0 Then
          'retrieve and adjust end of message section
          lngEndMsgSection = lngEndMsgSection + 256& * bytData(lngPos + 1) + bytData(lngPos)
          lngPos = lngPos + 2
          'loop through all messages, extract offset
          For intCurMsg = 1 To NumMessages
            'set start of this msg as start of msg block, plus offset, plus one (for byte which gives number of msgs)
            MessageStart(intCurMsg) = 256& * bytData(lngPos + 1) + bytData(lngPos) + lngMsgStart + 1
            'validate msg start
            If MessageStart(intCurMsg) > UBound(bytData) Then
              'invalid
              strError = "Invalid message section data"
              Exit Function
            End If
            lngPos = lngPos + 2
          Next intCurMsg

          'mark start of encrypted data (to align encryption string)
          EncryptionStart = lngPos

          'now read all messages, decrypting in the process
          For intCurMsg = 1 To NumMessages
            strMessage = vbNullString
            'if msg start points to a valid msg
            If MessageStart(intCurMsg) > 0 && MessageStart(intCurMsg) >= EncryptionStart Then
              lngPos = MessageStart(intCurMsg)
              blnEndOfMsg = False
              Do
                bytInput = bytData(lngPos) Xor bytEncryptKey((lngPos - EncryptionStart) Mod 11)
                lngPos = lngPos + 1
                If (bytInput == 0) || (lngPos > UBound(bytData())) Then
                  blnEndOfMsg = True
                Else
                  Select Case bytInput
                  Case &HA
                    strMessage = strMessage & "\n"
                  Case Is < 32
                    strMessage = strMessage & "\x" & Hex2(bytInput)
                  Case &H22
                    strMessage = strMessage & "\"""
                  Case &H5C
                    strMessage = strMessage & "\\"
                  Case &H7F
                    strMessage = strMessage & "\x7F"
                  Case &HFF
                    strMessage = strMessage & "\xFF"
                  Case Else
                    strMessage = strMessage & Chr$(bytInput)
                  End Select
                End If
              Loop Until blnEndOfMsg

              stlMsgs.Add QUOTECHAR & strMessage & QUOTECHAR
              blnMsgExists(intCurMsg) = True
            Else
              'add nothing (so numbers work out)
              stlMsgs.Add vbNullString
              blnMsgExists(intCurMsg) = False
            End If
          Next intCurMsg
        End If

        'return true
        ReadMessages = True
      Exit Function

      ErrHandler:
        'save error message
        strError = "Unhandled error while decoding messages (" & CStr(Err.Number) & ": " & Err.Description & _
                   ") at position " & CStr(lngPos)
        Err.Clear
        ReadMessages = False
      End Function


      Private Function DecodeIf(bytData() As Byte, stlOut As StringList) As Boolean

        Dim blnInOrBlock As Boolean
        Dim blnInNotBlock As Boolean
        Dim blnFirstCmd As Boolean
        Dim intArg1Val As Integer
        Dim bytArg2Val As Byte
        Dim bytCurByte As Byte
        Dim blnIfFinished As Boolean
        Dim bytNumSaidArgs As Byte
        Dim lngWordGroupNum As Long
        Dim strLine As String, strArg As String
        Dim bytCmd As Byte
        Dim strWarningLine() As String
        Dim i As Long
        Dim intCharCount As Integer

        On Error GoTo ErrHandler

        blnIfFinished = False
        blnFirstCmd = True
        blnInOrBlock = False
        strLine = MultStr("  ", bytBlockDepth) & IF_TOKEN

        'main loop - read in logic, one byte at a time, and write text accordingly
        Do
          'always reset 'NOT' block status to false
          blnInNotBlock = False

          'read next byte from input stream
          bytCurByte = bytData(lngPos)
          'and increment pointer
          lngPos = lngPos + 1

          'first, check for an 'OR'
          If bytCurByte = &HFC Then
            blnInOrBlock = Not blnInOrBlock
            If blnInOrBlock Then
              If Not blnFirstCmd Then
                strLine = strLine & AND_TOKEN
                stlOut.Add (strLine)
                strLine = MultStr("  ", bytBlockDepth) & "    "
                blnFirstCmd = True
              End If
              strLine = strLine & "("
            Else
              strLine = strLine & ")"
            End If

            'now get next byte, and continue checking
            bytCurByte = bytData(lngPos)
            lngPos = lngPos + 1
          End If

          'special check needed in case two &HFCs are in a row, e.g. (a || b) && (c || d)
          If (bytCurByte == &HFC) && (Not blnInOrBlock) Then
            strLine = strLine & AND_TOKEN
            stlOut.Add (strLine)
            strLine = MultStr("  ", bytBlockDepth) & "    "
            blnFirstCmd = True
            strLine = strLine & "("
            blnInOrBlock = True
            bytCurByte = bytData(lngPos)
            lngPos = lngPos + 1
          End If

          'check for 'not' command
          If bytCurByte = &HFD Then   ' NOT
            blnInNotBlock = True
            bytCurByte = bytData(lngPos)
            lngPos = lngPos + 1
          End If

          'check for valid test command
          If (bytCurByte > 0) && (bytCurByte <= agTestCmdCol.Count) Then

            If Not blnFirstCmd Then
              If blnInOrBlock Then
                strLine = strLine & OR_TOKEN
              Else
                strLine = strLine & AND_TOKEN
              End If
              stlOut.Add (strLine)
              strLine = MultStr("  ", bytBlockDepth) & "    "
            End If
            bytCmd = bytCurByte
            If agMainLogSettings.SpecialSyntax && (bytCmd >= 1 && bytCmd <= 6) Then
              'get first argument
              bytCurByte = bytData(lngPos)
              lngPos = lngPos + 1
              bytArg2Val = bytData(lngPos)
              lngPos = lngPos + 1
              strLine = strLine & AddSpecialIf(bytCmd, bytCurByte, bytArg2Val, blnInNotBlock)
            Else
              If blnInNotBlock Then
                strLine = strLine & NOT_TOKEN
              End If

              strLine = strLine & agTestCmdCol(bytCmd).Name & "("

              intArgStart = Len(strLine)
              If bytCmd = 14 Then ' said command
                bytNumSaidArgs = bytData(lngPos)
                lngPos = lngPos + 1
                For intArg1Val = 1 To bytNumSaidArgs
                  lngWordGroupNum = 256 * bytData(lngPos + 1) + bytData(lngPos)
                  lngPos = lngPos + 2
                  'if a game is loaded,
                  If agGameLoaded Then
                    'enable error trapping to catch any nonexistent words
                    On Error Resume Next
                    'if word exists,
                    strLine = strLine & QUOTECHAR & agVocabWords.GroupN(lngWordGroupNum).GroupName & QUOTECHAR
                    If Err.Number <> 0 Then
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh
                        'raise error
                        strError = "unknown word group (" & CStr(lngWordGroupNum) & ") at position " & _
                                   CStr(lngPos)
                        Exit Function
                      Case leMedium
                        'add the word by its number
                        strLine = strLine & CStr(lngWordGroupNum)
                        'set warning text
                        AddDecodeWarning "unknown word: " & CStr(lngWordGroupNum)
                      Case leLow
                        'add the word by its number
                        strLine = strLine & CStr(lngWordGroupNum)
                      End Select
                    End If
                    'reset error trapping
                    On Error GoTo 0
                  Else
                    'alwys use word number as the argument
                    strLine = strLine & CStr(lngWordGroupNum)
                  End If

                  If intArg1Val < bytNumSaidArgs Then
                    strLine = strLine & ", "
                  End If
                Next intArg1Val

              Else
                'if at least one arg
                If agTestCmdCol(bytCmd).ArgType.Length > 0 Then
                  For intArg1Val = 0 To agTestCmdCol(bytCmd).ArgType.Length - 1
                    bytCurByte = bytData(lngPos)
                    lngPos = lngPos + 1
                    'get arg Value
                    strArg = ArgValue(bytCurByte, agTestCmdCol(bytCmd).ArgType(intArg1Val))
                    'if message error (no string returned)
                    If LenB(strArg) = 0 Then
                      'error string set by ArgValue function
                      Exit Function
                    End If

                    'if message
                    If agTestCmdCol(bytCmd).ArgType(intArg1Val) = atMsg Then
                      'split over additional lines, if necessary
                      Do
                        'if this message is too long to add to current line,
                        If Len(strLine) + Len(strArg) > MAX_LINE_LEN Then
                          'determine number of characters availableto add to this line
                          intCharCount = MAX_LINE_LEN - Len(strLine)
                          'determine longest available section of message that can be added
                          'without exceeding max line length
                          Do
                            intCharCount = intCharCount - 1
                          Loop Until (intCharCount = 1) || (Mid(strArg, intCharCount, 1) = " ")
                          'if no space is found to split up the line
                          If intCharCount <= 1 Then
                            'just split it without worrying about a space
                            intCharCount = MAX_LINE_LEN - Len(strLine)
                          End If
                          'add the section of the message that fits on this line
                          strLine = strLine & Left(strArg, intCharCount) & QUOTECHAR
                          strArg = Mid(strArg, intCharCount + 1, Len(strArg) - intCharCount)
                          'add line
                          stlOut.Add strLine
                          'create indent (but don't exceed 20 spaces (to ensure msgs aren't split
                          'up into small chunks)
                          If intArgStart >= MAX_LINE_LEN - 20 Then
                            intArgStart = MAX_LINE_LEN - 20
                          End If
                          strLine = MultStr(" ", CByte(intArgStart)) & QUOTECHAR
                        Else
                          'not too long; add the message to current line
                          strLine = strLine & strArg
                          strArg = vbNullString
                        End If
                      'continue adding new lines until entire message is split and added
                      Loop Until strArg = vbNullString
                    Else
                      'just add it
                      strLine = strLine & strArg
                    End If

                    'if more arguments needed,
                    If intArg1Val < agTestCmdCol(bytCmd).ArgType.Length - 1 Then
                      strLine = strLine & ", "
                    End If
                  Next intArg1Val
                End If
              End If
              strLine = strLine & ")"
            End If
            blnFirstCmd = False
            'add warning if this is the unknown test19 command
            If bytCmd = 19 Then
              'set warning text
              AddDecodeWarning "unknowntest19 is only valid in Amiga AGI versions"
            End If
          ElseIf bytCurByte = &HFF Then
            'done with if block; add 'then'
            strLine = strLine & Replace(THEN_TOKEN, "%1", vbCrLf & Space$(bytBlockDepth * 2 + 2))
            '(SkipToEndIf verified that max block depth is not exceeded)
            'increase block depth counter
            bytBlockDepth = bytBlockDepth + 1
            Block(bytBlockDepth).IsIf = True
            Block(bytBlockDepth).Length = 256 * bytData(lngPos + 1) + bytData(lngPos)
            lngPos = lngPos + 2
            'check for length of zero
            If Block(bytBlockDepth).Length = 0 && agMainLogSettings.ErrorLevel = leMedium Then
              'set warning text
              AddDecodeWarning "this block contains no commands"
            End If

            'validate end pos
            Block(bytBlockDepth).EndPos = Block(bytBlockDepth).Length + lngPos
            If Block(bytBlockDepth).EndPos > UBound(bytData()) - 1 Then
              Select Case agMainLogSettings.ErrorLevel
              'if error level is high, SkipToEndIf catches this condition
              Case leMedium
                'adjust to end
                Block(bytBlockDepth).EndPos = UBound(bytData()) - 1
                'set warning text
                AddDecodeWarning "block end past end of resource; adjusted to end of resource"
              Case leLow
                'adjust to end
                Block(bytBlockDepth).EndPos = UBound(bytData()) - 1
              End Select
            End If
            'verify block ends before end of previous block
            '(i.e. it's properly nested)
            If Block(bytBlockDepth).EndPos > Block(bytBlockDepth - 1).EndPos Then
              'block is outside the previous block nest;
              'this is an abnormal situation
              'if error level is high; this would have been
              'caught in SkipToEndIf;
              If agMainLogSettings.ErrorLevel = leMedium Then
                'set warning text
                AddDecodeWarning "Block end outside of nested block (" & CStr(Block(bytBlockDepth).JumpPos) & ") at position " & CStr(lngPos)
              End If

              'need to simulate this block by using else and goto
              Block(bytBlockDepth).IsOutside = True
              Block(bytBlockDepth).JumpPos = Block(bytBlockDepth).EndPos
              Block(bytBlockDepth).EndPos = Block(bytBlockDepth - 1).EndPos
            End If
            stlOut.Add (strLine)
            'if any warnings
            If blnWarning Then
              'add warning lines
              strWarningLine = Split(strWarning, "|")
              For i = 0 To UBound(strWarningLine())
                stlOut.Add MultStr("  ", bytBlockDepth) & CMT_TOKEN & "WARNING: " & strWarningLine(i)
              Next i
              'reset warning flag & string
              blnWarning = False
              strWarning = vbNullString
            End If

            strLine = MultStr("  ", bytBlockDepth)
            blnIfFinished = True
          Else
            'unknown test command
            strError = "Unknown test command (" & CStr(bytCurByte) & ") at position " & _
                       CStr(lngPos)
            Exit Function
          End If
        Loop Until blnIfFinished
        DecodeIf = True
      Exit Function

      ErrHandler:
        '*'Debug.Assert False
        Resume
        'unknown test command
        strError = "Unhandled error (" & CStr(Err.Number) & ": " & Err.Description & ")" & _
                   vbCr & vbCr & "at position " & _
                   CStr(lngPos) & " in DecodeIf"
      End Function

      Private Function SkipToEndIf(bytData() As Byte) As Boolean
        'used by the find label method
        'it moves the cursor to the end of the current if
        'statement

        Dim CurArg As Byte
        Dim CurByte As Byte
        Dim IfFinished As Boolean
        Dim NumSaidArgs As Byte
        Dim ThisCommand As Byte
        Dim i As Long

        On Error GoTo ErrHandler

        IfFinished = False
        Do

          CurByte = bytData(lngPos)
          lngPos = lngPos + 1
          If CurByte = &HFC Then
            CurByte = bytData(lngPos)
            lngPos = lngPos + 1
          End If
          If CurByte = &HFC Then
            CurByte = bytData(lngPos)
            lngPos = lngPos + 1 ' we may have 2 &HFCs in a row, e.g. (a || b) && (c || d)
          End If
          If CurByte = &HFD Then
            CurByte = bytData(lngPos)
            lngPos = lngPos + 1
          End If

          If (CurByte > 0) && (CurByte <= agTestCmdCol.Count) Then
            ThisCommand = CurByte
            If ThisCommand = 14 Then ' said command
              'read in number of arguments
              NumSaidArgs = bytData(lngPos): lngPos = lngPos + 1
              'move pointer to next position past these arguments
              '(note that words use two bytes per argument, not one)
              lngPos = lngPos + NumSaidArgs * 2
            Else
              'move pointer to next position past the arguments for this command
              lngPos = lngPos + agTestCmdCol(ThisCommand).ArgType.Length
            End If
          ElseIf CurByte = &HFF Then
            If bytBlockDepth >= MAX_BLOCK_DEPTH - 1 Then
              strError = "Too many nested blocks (" & CStr(bytBlockDepth + 1) & ") at position " & _
                         CStr(lngPos)
              Exit Function
            End If
            'increment block counter
            bytBlockDepth = bytBlockDepth + 1
            Block(bytBlockDepth).IsIf = True
            Block(bytBlockDepth).Length = 256 * bytData(lngPos + 1) + bytData(lngPos)
            lngPos = lngPos + 2
            'check length of block
            If Block(bytBlockDepth).Length = 0 Then
              If agMainLogSettings.ErrorLevel = leHigh Then
                'consider zero block lengths as error
                strError = "Encountered command block of length 0 at position " & _
                           CStr(lngPos)
                Exit Function
              End If
            End If
            Block(bytBlockDepth).EndPos = Block(bytBlockDepth).Length + lngPos
            If Block(bytBlockDepth).EndPos > Block(bytBlockDepth - 1).EndPos Then
              'block is outside the previous block nest;
              '
              'this is an abnormal situation;
              Select Case agMainLogSettings.ErrorLevel
              Case leHigh
                'error
                strError = "Block end outside of nested block (" & CStr(Block(bytBlockDepth).JumpPos) & ") at position" & CStr(lngPos)
                Exit Function

              Case leMedium, leLow
                'need to simulate this block by using else and goto
                Block(bytBlockDepth).IsOutside = True
                Block(bytBlockDepth).JumpPos = Block(bytBlockDepth).EndPos
                Block(bytBlockDepth).EndPos = Block(bytBlockDepth - 1).EndPos
                'add a new goto item
                '(since error level is medium or low
                'dont need to worry about an invalid jumppos)

                'if label is already created
                For i = 1 To bytLabelCount
                  If lngLabelPos(i) = Block(bytBlockDepth).JumpPos Then
                    Exit For
                  End If
                Next i
                'if loop exited normally (i will equal bytLabelCount+1)
                If i = bytLabelCount + 1 Then
                  'increment label Count
                  bytLabelCount = i
                  ReDim Preserve lngLabelPos(bytLabelCount)
                  'save this label position
                  lngLabelPos(bytLabelCount) = Block(bytBlockDepth).JumpPos
                End If
              End Select
            End If
            IfFinished = True
          Else
            strError = "Unknown test command (" & CStr(CurByte) & ") at position " & _
                       CStr(lngPos)
            Exit Function
          End If
        Loop Until IfFinished
        SkipToEndIf = True
      Exit Function

      ErrHandler:
        'if no error string
        If LenB(strError) = 0 Then
          strError = "Unhandled error while decoding logic at position " & _
                     CStr(lngPos) & "): " & strError
        End If
      End Function


      Private Function FindLabels(bytData() As Byte) As Boolean

        Dim i As Integer, j As Integer
        Dim CurBlock As Integer
        Dim bytCurData As Byte
        Dim tmpBlockLength As Long
        Dim DoGoto As Boolean
        Dim LabelLoc As Long

        'finds all labels and stores them in an array;
        'they are then sorted and put in order so that
        'as each is found during decoding of the logic
        'the label is created, and the next label position
        'is moved to top of stack

        On Error GoTo ErrHandler

        bytBlockDepth = 0
        bytLabelCount = 0
        ReDim lngLabelPos(0)
        lngPos = 2

        Do
          'check to see if the end of a block has been found
          'start at most recent block and work up to oldest block
          For CurBlock = bytBlockDepth To 1 Step -1
            'if this position matches the end of this block
            If Block(CurBlock).EndPos <= lngPos Then
              'verify it is exact
              If Block(CurBlock).EndPos <> lngPos Then
                'error
                strError = "Invalid goto position, or invalid if/then block length" & _
                     ") at position " & CStr(Block(CurBlock).EndPos - Block(CurBlock).Length)
                Exit Function
              End If

              'take this block off stack
              bytBlockDepth = bytBlockDepth - 1
            End If
          Next CurBlock

          'get next byte
          bytCurData = bytData(lngPos)
          lngPos = lngPos + 1

          Select Case bytCurData
          Case &HFF 'this byte points to start of an IF statement
            'find labels associated with this if statement
            If Not SkipToEndIf(bytData()) Then
              'major error
              Exit Function
            End If

          Case Is <= agCmdCol.Count 'byte is an AGI command
            'skip over arguments to get next command
            '*'Debug.Assert bytCurData <> 178
            lngPos = lngPos + agCmdCol(bytCurData).ArgType.Length

          Case &HFE   'if the byte is a GOTO command
            'reset goto status flag
            DoGoto = False
            tmpBlockLength = 256 * CLng(bytData(lngPos + 1)) + bytData(lngPos)
            lngPos = lngPos + 2
            'need to check for negative Value here
            If tmpBlockLength > &H7FFF& Then
              'correct block length
              tmpBlockLength = tmpBlockLength - &H10000
            End If
            'check to see if this 'goto' might be an 'else':
            '  - end of this block matches this position (the if-then part is done)
            '  - this block is identified as an IF block
            '  - this is NOT the main block
            '  - the flag to set elses as gotos is turned off
            If (Block(bytBlockDepth).EndPos == lngPos) && (Block(bytBlockDepth).IsIf) && (bytBlockDepth > 0) && (Not agMainLogSettings.ElseAsGoto) Then
              'this block is now in the 'else' part, so reset flag
              Block(bytBlockDepth).IsIf = False
              Block(bytBlockDepth).IsOutside = False
              'does this 'else' statement line up to end at the same
              'point that the 'if' statement does?
              'the end of this block is past where the 'if' block ended OR
              'the block is negative (means jumping backward, so it MUST be a goto)
              'length of block doesn't have enough room for code necessary to close the 'else'
              If (tmpBlockLength + lngPos > Block(bytBlockDepth - 1).EndPos) || (tmpBlockLength < 0) || (Block(bytBlockDepth).Length <= 3) Then
              'this is a 'goto' statement,
                DoGoto = True
              Else
                'this is an 'else' statement;
                'readjust block end so the IF statement that owns this 'else'
                'is ended correctly
                Block(bytBlockDepth).Length = tmpBlockLength
                Block(bytBlockDepth).EndPos = Block(bytBlockDepth).Length + lngPos
              End If
            Else
              'this is a goto statement (or an else statement while mGotos flag is false)
              DoGoto = True
            End If

            ' goto
            If DoGoto Then
              LabelLoc = tmpBlockLength + lngPos
              If LabelLoc > UBound(bytData()) - 1 Then
                'if error level is high (medium and low are handled in DecodeLogic)
                If agMainLogSettings.ErrorLevel = leHigh Then
                  strError = "Goto destination past end of logic (" & CStr(LabelLoc) & ")" & "at position " & CStr(lngPos)
                  Exit Function
                End If
              End If
              'if label is already created
              For i = 1 To bytLabelCount
                If lngLabelPos(i) = LabelLoc Then
                  Exit For
                End If
              Next i
              'if loop exited normally (i will equal bytLabelCount+1)
              If i = bytLabelCount + 1 Then
                'increment label Count
                bytLabelCount = bytLabelCount + 1
                ReDim Preserve lngLabelPos(bytLabelCount)
                'save this label position
                lngLabelPos(bytLabelCount) = LabelLoc
              End If
            End If
          Case Else
            'if a command not implemented in this version
            If bytCurData <= 182 Then
              'leave it to actual decompiler to figure out what to do
              'strError = "The command '" & agCmds(bytCurData).Name & _
              '           "' (at position " & CStr(lngPos) & _
              '           ") is not implemented in this interpreter version."
            Else
              'major error
              strError = "Unknown action command (" & CStr(bytCurData) & ") at position " & _
                         CStr(lngPos)
              Exit Function
            End If
          End Select
        Loop Until (lngPos >= lngMsgSecStart)

        'now sort labels, if found
        If bytLabelCount > 1 Then
          For i = 1 To bytLabelCount - 1
            For j = i + 1 To bytLabelCount
              If lngLabelPos(j) < lngLabelPos(i) Then
                LabelLoc = lngLabelPos(i)
                lngLabelPos(i) = lngLabelPos(j)
                lngLabelPos(j) = LabelLoc
              End If
            Next j
          Next i
        End If

        'clear block info (don't overwrite main block)
        For i = 1 To MAX_BLOCK_DEPTH
          With Block(i)
            .EndPos = 0
            .IsIf = False
            .IsOutside = False
            .JumpPos = 0
            .Length = 0
          End With
        Next i

        'return success
        FindLabels = True
      Exit Function

      ErrHandler:
        'if no error string
        If LenB(strError) = 0 Then
          'use a default
          strError = "Unhandled error in FindLabels (" & CStr(Err.Number) & ": " & Err.Description & _
                     ") at position " & CStr(lngPos)
        End If
        FindLabels = False
      End Function

      Private Sub DisplayMessages(stlOut As StringList)

        Dim lngMsg  As Long

        'need to adjust references to the Messages stringlist object by one
        'since the list is zero based, but messages are one-based.

        stlOut.Add CMT_TOKEN & "Messages"

        For lngMsg = 1 To stlMsgs.Count
          If blnMsgExists(lngMsg) && ((agMainLogSettings.ShowAllMessages) || Not blnMsgUsed(lngMsg)) Then
            stlOut.Add Replace(Replace(MSG_TOKEN, ARG1, CStr(lngMsg)), "%2", stlMsgs(lngMsg - 1))
          End If
        Next lngMsg
      End Sub



      Private Function AddSpecialCmd(bytData() As Byte, bytCmd As Byte) As String
        Dim bytArg1 As Byte
        Dim bytArg2 As Byte
        'get first argument
        bytArg1 = bytData(lngPos)
        lngPos = lngPos + 1

        Select Case bytCmd
        Case &H1  ' increment
          AddSpecialCmd = "++" & ArgValue(bytArg1, atVar)

        Case &H2  ' decrement
          AddSpecialCmd = "--" & ArgValue(bytArg1, atVar)

        Case &H3  ' assignn
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " = " & ArgValue(bytArg2, atNum, bytArg1)

        Case &H4  ' assignv
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " = " & ArgValue(bytArg2, atVar)

        Case &H5  ' addn
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & "  += " & ArgValue(bytArg2, atNum)

        Case &H6  ' addv
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & "  += " & ArgValue(bytArg2, atVar)

        Case &H7  ' subn
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " -= " & ArgValue(bytArg2, atNum)

        Case &H8  ' subv
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " -= " & ArgValue(bytArg2, atVar)

        Case &H9  ' lindirectv
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = "*" & ArgValue(bytArg1, atVar) & " = " & ArgValue(bytArg2, atVar)

        Case &HA  ' rindirect
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " = *" & ArgValue(bytArg2, atVar)

        Case &HB  ' lindirectn
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = "*" & ArgValue(bytArg1, atVar) & " = " & ArgValue(bytArg2, atNum)

        Case &HA5 ' mul.n
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " *= " & ArgValue(bytArg2, atNum)

        Case &HA6 ' mul.v
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " *= " & ArgValue(bytArg2, atVar)

        Case &HA7 ' div.n
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " /= " & ArgValue(bytArg2, atNum)

        Case &HA8 ' div.v
          bytArg2 = bytData(lngPos)
          lngPos = lngPos + 1
          AddSpecialCmd = ArgValue(bytArg1, atVar) & " /= " & ArgValue(bytArg2, atVar)
        End Select
      End Function


      Private Function AddSpecialIf(bytCmd As Byte, bytArg1 As Byte, bytArg2 As Byte, NOTOn As Boolean) As String

        AddSpecialIf = ArgValue(bytArg1, atVar)

        Select Case bytCmd
        Case 1, 2            ' equaln or equalv
          'if NOT in effect,
          If NOTOn Then
            'test for not equal
            AddSpecialIf = AddSpecialIf & NOT_EQUAL_TEST_TOKEN
          Else
            'test for equal
             AddSpecialIf = AddSpecialIf & EQUAL_TEST_TOKEN
          End If
          'if command is comparing variables,
          If bytCmd = 2 Then
            'variable
            AddSpecialIf = AddSpecialIf & ArgValue(bytArg2, atVar, bytArg1)
          Else
            'add number
            AddSpecialIf = AddSpecialIf & ArgValue(bytArg2, atNum, bytArg1)
          End If
        Case 3, 4            ' lessn, lessv
          'if NOT is in effect,
          If NOTOn Then
            'test for greater than or equal
            AddSpecialIf = AddSpecialIf & " >= "
          Else
            'test for less than
            AddSpecialIf = AddSpecialIf & " < "
          End If
          'if command is comparing variables,
          If bytCmd = 4 Then
            AddSpecialIf = AddSpecialIf & ArgValue(bytArg2, atVar, bytArg1)
          Else
            'number string
            AddSpecialIf = AddSpecialIf & ArgValue(bytArg2, atNum, bytArg1)
          End If

        Case 5, 6            ' greatern, greaterv
          'if NOT is in effect,
          If NOTOn Then
            'test for less than or equal
            AddSpecialIf = AddSpecialIf & " <= "
          Else
            'test for greater than
            AddSpecialIf = AddSpecialIf & " > "
          End If
          'if command is comparing variables,
          If bytCmd = 6 Then
            AddSpecialIf = AddSpecialIf & ArgValue(bytArg2, atVar, bytArg1)
          Else
            'number string
            AddSpecialIf = AddSpecialIf & ArgValue(bytArg2, atNum, bytArg1)
          End If
        End Select
      End Function



      Private Sub AddBlockEnds(stlIn As StringList)

        Dim CurBlock As Integer
        Dim i As Long

        For CurBlock = bytBlockDepth To 1 Step -1
          'why would a less than apply here?
          'FOUND IT!!! here is a case where it is less than!!!
          'If Block(CurBlock).EndPos <= lngPos Then
          If Block(CurBlock).EndPos = lngPos Then
            'check for unusual case where an if block ends outside
            'the if block it is nested in
            If Block(CurBlock).IsOutside Then
              'add an else
              stlIn.Add (MultStr("  ", bytBlockDepth) & ENDIF_TOKEN)
              If agMainLogSettings.ElseAsGoto Then
                stlIn.Add (MultStr("  ", bytBlockDepth - 1) & GOTO_TOKEN)
              Else
                stlIn.Add (MultStr("  ", bytBlockDepth - 1) & Replace(ELSE_TOKEN, "%1", vbCrLf & Space$(bytBlockDepth * 2)))
              End If

              'add a goto
              For i = 1 To bytLabelCount
                If lngLabelPos(i) = Block(CurBlock).JumpPos Then
                  stlIn.Add MultStr("  ", bytBlockDepth) & Replace(GOTO_TOKEN, ARG1, "Label" & CStr(i)) & EOL_TOKEN
                  Exit For
                End If
              Next i
            End If
            'add end if
            stlIn.Add MultStr("  ", CurBlock) & ENDIF_TOKEN
            bytBlockDepth = bytBlockDepth - 1
          End If
        Next CurBlock
      End Sub



      Public Function DecodeLogic(bytData() As Byte) As StringList

        Dim stlOutput As StringList
        Dim bytCurData As Byte
        Dim blnGoto As Boolean
        Dim strCurrentLine As String
        Dim bytCmd As Byte
        Dim tmpBlockLen As Long
        Dim intArg As Integer
        Dim lngNextLabel As Long
        Dim lngLabelLoc As Long
        Dim strWarningLine() As String
        Dim i As Long, j As Long
        Dim strArg As String
        Dim intCharCount As Integer

        On Error GoTo UnHandledError

        'if nothing in the resource,
        If UBound(bytData) = 0 Then
          'single 'return()' command
          Set DecodeLogic = New StringList
          DecodeLogic.Add "return();"
          Exit Function
        End If

        'clear block info
        For i = 0 To MAX_BLOCK_DEPTH
          With Block(i)
            .EndPos = 0
            .IsIf = False
            .IsOutside = False
            .JumpPos = 0
            .Length = 0
          End With
        Next i

        'create output stringlist
        Set stlOutput = New StringList

        'extract beginning of msg section
        '(add two because in the AGI executable, the message section start is referenced
        'relative to byte 7 of the header. When extracted, the resource data
        'begins with byte 5 of the header:
        '
        ' byte 00: high byte of resource start signature (always &H12)
        ' byte 01: low byte of resource start signature (always &H34)
        ' byte 02: VOL file number
        ' byte 03: low byte of logic script length
        ' byte 04: high byte of logic script length
        ' byte 05: low byte of offset to message section start
        ' byte 06: high byte of offset to message section start
        ' byte 07: begin logic data

        lngMsgSecStart = bytData(1) * 256 + bytData(0) + 2

        'if can't read messges,
        If Not ReadMessages(bytData(), lngMsgSecStart) Then
          'raise error
          GoTo ErrHandler
        End If

        'set main block info
        Block(0).IsIf = False
        Block(0).EndPos = lngMsgSecStart
        Block(0).IsOutside = False
        Block(0).Length = lngMsgSecStart

        'set error flag
        strError = vbNullString
        'locate labels, and mark them
        If Not FindLabels(bytData()) Then
          'use error string set by findlabels
          GoTo ErrHandler
        End If

        'reset block depth and data position
        bytBlockDepth = 0
        lngPos = 2
        If bytLabelCount > 0 Then
          lngNextLabel = 1
        End If

        'main loop
        Do
          AddBlockEnds stlOutput
          'check for label position
          If lngLabelPos(lngNextLabel) = lngPos Then
            stlOutput.Add "Label" & CStr(lngNextLabel) & ":"
            lngNextLabel = lngNextLabel + 1
            If lngNextLabel > bytLabelCount Then
              lngNextLabel = 0
            End If
          End If
          bytCurData = bytData(lngPos)
          lngPos = lngPos + 1
          Select Case bytCurData
          Case &HFF 'this byte starts an IF statement
            If Not DecodeIf(bytData(), stlOutput) Then
              GoTo ErrHandler
            End If
          Case Is <= 182 'valid agi command
            'if this command is NOT within range of expected commands for targeted interpretr version,
            If bytCurData > agCmdCol.Count Then 'this byte is a command
              'show warning
              AddDecodeWarning "this command not valid for selected interpreter version (" & agIntVersion & ")"
            End If

            bytCmd = bytCurData
            strCurrentLine = MultStr("  ", bytBlockDepth)
            If ((agMainLogSettings.SpecialSyntax && bytCmd >= &H1 && bytCmd <= &HB) || (bytCmd >= &HA5 && bytCmd <= &HA8)) Then
              strCurrentLine = strCurrentLine & AddSpecialCmd(bytData(), bytCmd)
            Else
              strCurrentLine = strCurrentLine & agCmds(bytCmd).Name & "("
              intArgStart = Len(strCurrentLine)
              For intArg = 0 To agCmds(bytCmd).ArgType.Length - 1
                bytCurData = bytData(lngPos)
                lngPos = lngPos + 1
                strArg = ArgValue(bytCurData, agCmds(bytCmd).ArgType(intArg))

                'if showing reserved names and using reserved defines
                If agResAsText && agUseRes Then
                  'some commands use resources as arguments; substitute as appropriate
                  Select Case bytCmd
                  Case 122 'add.to.pic,    1st arg (V)
                    If intArg = 0 Then
                      If agViews.Exists(bytCurData) Then
                        strArg = agViews(bytCurData).ID
                      Else
                        'view doesn't exist
                        Select Case agMainLogSettings.ErrorLevel
                        Case leHigh, leMedium
                          'set warning
                          AddDecodeWarning "view " & CStr(bytCurData) & " in add.to.pic() does not exist"
                        Case leLow
                          'do nothing
                        End Select
                      End If
                    End If

                  Case 22  'call,          only arg (L)
                    If agLogs.Exists(bytCurData) Then
                      strArg = agLogs(bytCurData).ID
                    Else
                      'logic doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "logic " & CStr(bytCurData) & " in call() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 175 'discard.sound, only arg (S)
                    If agSnds.Exists(bytCurData) Then
                      strArg = agSnds(bytCurData).ID
                    Else
                      'sound doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "sound " & CStr(bytCurData) & " in discard.sound() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 32  'discard.view,  only arg (V)
                    If agViews.Exists(bytCurData) Then
                      strArg = agViews(bytCurData).ID
                    Else
                      'view doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "view " & CStr(bytCurData) & " in discard.view() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 20  'load.logics,   only arg (L)
                    If agLogs.Exists(bytCurData) Then
                      strArg = agLogs(bytCurData).ID
                    Else
                      'logic doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "logic " & CStr(bytCurData) & " in loadlogics() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 98  'load.sound,    only arg (S)
                    If agSnds.Exists(bytCurData) Then
                      strArg = agSnds(bytCurData).ID
                    Else
                      'sound doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "sound " & CStr(bytCurData) & " in load.sound() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 30  'load.view,     only arg (V)
                    If agViews.Exists(bytCurData) Then
                      strArg = agViews(bytCurData).ID
                    Else
                      'view doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "view " & CStr(bytCurData) & " in load.view() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 18  'new.room,      only arg (L)
                    If agLogs.Exists(bytCurData) Then
                      strArg = agLogs(bytCurData).ID
                    Else
                      'logic doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "logic " & CStr(bytCurData) & " in new.room() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 41  'set.view,      2nd arg (V)
                    If intArg = 1 Then
                      If agViews.Exists(bytCurData) Then
                        strArg = agViews(bytCurData).ID
                      Else
                        'view doesn't exist
                        Select Case agMainLogSettings.ErrorLevel
                        Case leHigh, leMedium
                          'set warning
                          AddDecodeWarning "view " & CStr(bytCurData) & " in set.view() does not exist"
                        Case leLow
                          'do nothing
                        End Select
                      End If
                    End If
                  Case 129 'show.obj,      only arg (V)
                    If agViews.Exists(bytCurData) Then
                      strArg = agViews(bytCurData).ID
                    Else
                      'view doesn't exist
                      Select Case agMainLogSettings.ErrorLevel
                      Case leHigh, leMedium
                        'set warning
                        AddDecodeWarning "view " & CStr(bytCurData) & " in show.obj() does not exist"
                      Case leLow
                        'do nothing
                      End Select
                    End If
                  Case 99  'sound,         1st arg (S)
                    If intArg = 0 Then
                      If agSnds.Exists(bytCurData) Then
                        strArg = agSnds(bytCurData).ID
                      Else
                        'sound doesn't exist
                        Select Case agMainLogSettings.ErrorLevel
                        Case leHigh, leMedium
                          'set warning
                          AddDecodeWarning "sound " & CStr(bytCurData) & " in sound() does not exist"
                        Case leLow
                          'do nothing
                        End Select
                      End If
                    End If
                  Case 150 'trace.info,    1st arg (L)
                    If intArg = 0 Then
                      If agLogs.Exists(bytCurData) Then
                        strArg = agLogs(bytCurData).ID
                      Else
                        'logic doesn't exist
                        Select Case agMainLogSettings.ErrorLevel
                        Case leHigh, leMedium
                          'set warning
                          AddDecodeWarning "logic " & CStr(bytCurData) & " in trace.info() does not exist"
                        Case leLow
                          'do nothing
                        End Select
                      End If
                    End If
                  End Select
                End If

                'if message error (no string returned)
                If LenB(strArg) = 0 Then
                  'error string set by ArgValue function
                  GoTo ErrHandler
                End If

                'check for commands that use colors here
                Select Case bytCmd
                Case 105 'clear.lines, 3rd arg
                  If intArg = 2 Then
                    If Val(strArg) < 16 Then
                      strArg = agResColor(Val(strArg)).Name
                    End If
                  End If

                Case 154 'clear.text.rect, 5th arg
                  If intArg = 4 Then
                    If Val(strArg) < 16 Then
                      strArg = agResColor(Val(strArg)).Name
                    End If
                  End If

                Case 109 'set.text.attribute, all args
                  If Val(strArg) < 16 Then
                    strArg = agResColor(Val(strArg)).Name
                  End If
                End Select

                'if message
                If agCmds(bytCmd).ArgType(intArg) = atMsg Then
                  'split over additional lines, if necessary
                  Do
                    'if this message is too long to add to current line,
                    If Len(strCurrentLine) + Len(strArg) > MAX_LINE_LEN Then
                      'determine number of characters availableto add to this line
                      intCharCount = MAX_LINE_LEN - Len(strCurrentLine)
                      'determine longest available section of message that can be added
                      'without exceeding max line length
                      If intCharCount > 1 Then
                        Do Until (intCharCount <= 1) || (Mid(strArg, intCharCount, 1) = " ")
                          intCharCount = intCharCount - 1
                        Loop
                        'if no space is found to split up the line
                        If intCharCount <= 1 Then
                          'just split it without worrying about a space
                          intCharCount = MAX_LINE_LEN - Len(strCurrentLine)
                        End If
                        'add the section of the message that fits on this line
                        strCurrentLine = strCurrentLine & Left(strArg, intCharCount) & QUOTECHAR
                        strArg = Mid(strArg, intCharCount + 1, Len(strArg) - intCharCount)
                        'add line
                        stlOutput.Add strCurrentLine
                        'create indent (but don't exceed 20 spaces (to ensure msgs aren't split
                        'up into small chunks)
                        If intArgStart >= MAX_LINE_LEN - 20 Then
                          intArgStart = MAX_LINE_LEN - 20
                        End If
                        strCurrentLine = MultStr(" ", CByte(intArgStart)) & QUOTECHAR
                      Else
                        'line is messed up; just add it
                        strCurrentLine = strCurrentLine & strArg
                        strArg = vbNullString
                      End If
                    Else
                      'not too long; add the message to current line
                      strCurrentLine = strCurrentLine & strArg
                      strArg = vbNullString
                    End If
                  'continue adding new lines until entire message is split and added
                  Loop Until strArg = vbNullString
                Else
                  'add arg
                  strCurrentLine = strCurrentLine & strArg
                End If

                'if more arguments needed,
                If intArg < agCmds(bytCmd).ArgType.Length - 1 Then
                  strCurrentLine = strCurrentLine & ", "
                End If
              Next intArg
              strCurrentLine = strCurrentLine & ")"
            End If
            strCurrentLine = strCurrentLine & EOL_TOKEN
            stlOutput.Add strCurrentLine
            'if any warnings
            If blnWarning Then
              'add warning lines
              strWarningLine = Split(strWarning, "|")
              For i = 0 To UBound(strWarningLine())
                stlOutput.Add CMT_TOKEN & "WARNING: " & strWarningLine(i)
              Next i
              'reset warning flag & string
              blnWarning = False
              strWarning = vbNullString
            End If

          Case &HFE 'this byte is a goto or else
            blnGoto = False
            tmpBlockLen = 256 * CLng(bytData(lngPos + 1)) + bytData(lngPos)
            lngPos = lngPos + 2
            'need to check for negative Value here
            If tmpBlockLen > &H7FFF Then
              tmpBlockLen = tmpBlockLen - &H10000
            End If
            If (Block(bytBlockDepth).EndPos == lngPos) && (Block(bytBlockDepth).IsIf) && (bytBlockDepth > 0) && (Not agMainLogSettings.ElseAsGoto) Then
              Block(bytBlockDepth).IsIf = False
              Block(bytBlockDepth).IsOutside = False
              If (tmpBlockLen + lngPos > Block(bytBlockDepth - 1).EndPos) || (tmpBlockLen < 0) || (Block(bytBlockDepth).Length <= 3) Then
                blnGoto = True
              Else
                stlOutput.Add (MultStr("  ", bytBlockDepth) & ENDIF_TOKEN)
                If agMainLogSettings.ElseAsGoto Then
                  stlOutput.Add (MultStr("  ", bytBlockDepth - 1) & GOTO_TOKEN)
                Else
                  stlOutput.Add (MultStr("  ", bytBlockDepth - 1) & Replace(ELSE_TOKEN, "%1", vbCrLf & Space$(bytBlockDepth * 2)))
                End If
                Block(bytBlockDepth).Length = tmpBlockLen
                Block(bytBlockDepth).EndPos = Block(bytBlockDepth).Length + lngPos
              End If
            Else
              blnGoto = True
            End If
            ' goto
            If blnGoto Then
              lngLabelLoc = tmpBlockLen + lngPos
              'label already verified in FindLabels; add warning if necessary
              If lngLabelLoc > UBound(bytData()) - 1 Then
                Select Case agMainLogSettings.ErrorLevel
                'Case leHigh - high level handled in FindLabels
                Case leMedium
                  'set warning
                  AddDecodeWarning "Goto destination past end of logic at position " & CStr(lngPos) & " adjusted to end of logic"

                  'adjust it to end of resource
                  lngLabelLoc = UBound(bytData()) - 1
                Case leLow
                  'adjust it to end of resource
                  lngLabelLoc = UBound(bytData()) - 1
                End Select
              End If

              For i = 1 To bytLabelCount
                If lngLabelPos(i) = lngLabelLoc Then
                  stlOutput.Add MultStr("  ", bytBlockDepth) & Replace(GOTO_TOKEN, ARG1, "Label" & CStr(i)) & EOL_TOKEN
                  'if any warnings
                  If blnWarning Then
                    'add warning lines
                    strWarningLine = Split(strWarning, "|")
                    For j = 0 To UBound(strWarningLine())
                      stlOutput.Add CMT_TOKEN & "WARNING: " & strWarningLine(j)
                    Next j
                    'reset warning flag & string
                    blnWarning = False
                    strWarning = vbNullString
                  End If
                  Exit For
                End If
              Next i
            End If
          Case Else
            'will never get here;
            'FindLabels validates all commands
          End Select
        Loop Until (lngPos >= lngMsgSecStart)

        AddBlockEnds stlOutput
        stlOutput.Add (vbNullString)
        DisplayMessages stlOutput

        'return results
        Set DecodeLogic = stlOutput

      Exit Function

      ErrHandler:
        On Error GoTo 0: Err.Raise vbObjectError + 688, "LogDecode", strError
      Exit Function

      UnHandledError:
        '*'Debug.Assert False
        Resume Next
      End Function
      */
    }
  }
}
