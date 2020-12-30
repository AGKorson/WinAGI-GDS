using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.LogicErrorLevel;
using static WinAGI.AGICommands;

namespace WinAGI
{
  public static partial class WinAGI
  {
    internal struct BlockType
    {
      internal bool IsIf;
      internal int EndPos;
      internal int Length;
      internal bool IsOutside;
      internal int JumpPos;
    }
    static byte bytBlockDepth;
    static BlockType[] Block = new BlockType[MAX_BLOCK_DEPTH];
    static int intArgStart;
    static int[] lngLabelPos;
    static int lngMsgSecStart;
    static List<string> stlMsgs;
    static bool[] blnMsgUsed = new bool[256];
    static bool[] blnMsgExists = new bool[256];

    const int MAX_LINE_LEN = 80;
    //tokens for building source code output
    //const string NOT_TOKEN = "!";
    //const string AND_TOKEN = " && ";
    //const string OR_TOKEN = " || ";
    const string IF_TOKEN = "if (";
    const string THEN_TOKEN = ")%1{"; //where %1 is a line feed plus indent at current level
    const string ELSE_TOKEN = "else%1{"; //where %1 is a line feed plus indent at current level
    const string ENDIF_TOKEN = "}";
    const string GOTO_TOKEN = "goto(%1)";
    const string EOL_TOKEN = ";";
    const string EQUAL_TEST_TOKEN = " == ";
    const string NOT_EQUAL_TEST_TOKEN = " != ";
    const string CMT_TOKEN = "[ ";
    const string MSG_LINE = "#message %1 %2";

    static bool blnWarning;
    static string strWarning;

    internal static List<string> DecodeLogic(byte[] ResData)
    {
      return new List<string> { "abc" };

    }
    static void tmp_logdecode()
    {

      void AddDecodeWarning(string WarningText)
      {
        //if at least one warning already,
        if (blnWarning)
        {
          //add pipe character
          strWarning = strWarning + "|";

        }
        else
        {
          //set warning flag
          blnWarning = true;
        }
        strWarning += WarningText;
      }

      string ArgValue(byte ArgNum, ArgTypeEnum ArgType, int ArgComp = -1)
      {
        int i;
        //if not showing reserved names (or if not using reserved defines)
        // AND not a msg (always substitute msgs)
        if ((!agResAsText || !agUseRes) && ArgType != ArgTypeEnum.atMsg)
        {
          //return simple Value
          return agArgTypPref[(int)ArgType] + ArgNum;
        }

        //add appropriate resdef name
        switch (ArgType)
        {
          case ArgTypeEnum.atNum:
            switch (ArgComp)
            {
              case 2:
              case 5:  //edgecode
                if (ArgNum <= 4)
                {
                  return agEdgeCodes[ArgNum].Name;
                }
                else
                {
                  return ArgNum.ToString();
                }

              case 6: //egodir
                if (ArgNum <= 8)
                {
                  return agEgoDir[ArgNum].Name;
                }
                else
                {
                  return ArgNum.ToString();
                }

              case 20: //computer type
                if (ArgNum <= 8)
                {
                  return agCompType[ArgNum].Name;
                }
                else
                {
                  return ArgNum.ToString();
                }

              case 26: //video
                if (ArgNum <= 4)
                {
                  return agVideoMode[ArgNum].Name;
                }
                else
                {
                  return ArgNum.ToString();
                }
              default:
                //use default
                return ArgNum.ToString();
            }

          case ArgTypeEnum.atVar:
            //if a predefined,
            if (ArgNum <= 26)
            {
              return agResVar[ArgNum].Name;
            }
            else
            {
              //not a reserved data type
              return "v" + ArgNum;
            }

          case ArgTypeEnum.atFlag:
            //if a predefined
            if (ArgNum <= 16)
            {
              return agResFlag[ArgNum].Name;
              //check for special case of f20 (only if version 3.002102 or higher)
            }
            else if (ArgNum == 20 && Val(agIntVersion) >= 3.002102)
            {
              return agResFlag[17].Name;
            }
            else
            {
              //not a reserved data type
              return "f" + ArgNum;
            }

          case ArgTypeEnum.atMsg:
            blnMsgUsed[ArgNum] = true;
            //if this message exists,
            if (blnMsgExists[ArgNum])
            {
              //begin by using entire message as the chunk to add to current line
              return stlMsgs[ArgNum - 1];
            }
            else
            {
              //message doesn't exist
              switch (agMainLogSettings.ErrorLevel)
              {
                case leHigh:
                  strError = "Undefined message (" + ArgNum + ")  at position " + lngPos;
                  return "";
                case leMedium:
                  //set warning
                  AddDecodeWarning("unknown message: " + ArgNum + " at position " + lngPos);
                  //store as number
                  return "m" + ArgNum;
                case leLow:
                default:
                  //store as number
                  return "m" + ArgNum;
              }
            }

          case ArgTypeEnum.atSObj:
            //if ego
            if (ArgNum == 0)
            {
              return "ego";
            }
            else
            {
              //not a reserved data type
              return "o" + ArgNum;
            }

          case ArgTypeEnum.atIObj:
            //if a game is loaded AND OBJECT file is loaded,
            if (agGameLoaded && agInvObj.Loaded)
            {
              if (ArgNum < agInvObj.Count)
              {
                //if object is unique
                if (agInvObj[ArgNum].Unique)
                {
                  //double check if item is a question mark
                  if (agInvObj[ArgNum].ItemName == "?")
                  {
                    //use the inventory item number, and post a warning
                    AddDecodeWarning("reference to invalid inventory object ('?')");
                    return "i" + ArgNum;
                  }
                  else
                  {
                    //a unique, non-questionmark item- use it's string Value
                    return QUOTECHAR + agInvObj[ArgNum].ItemName.Replace(QUOTECHAR, "\"") + QUOTECHAR;
                  }
                }
                else
                {
                  //use obj number instead
                  if (agMainLogSettings.ErrorLevel != leLow)
                  {
                    AddDecodeWarning("non-unique object: '" + agInvObj[ArgNum].ItemName + "'");
                  }
                  return "i" + ArgNum;
                }
              }
              else
              {
                switch (agMainLogSettings.ErrorLevel)
                {
                  case leHigh:
                    strError = ("Invalid inventory item (" + ArgNum + ")");
                    return "";
                  case leMedium:
                    //set warning
                    AddDecodeWarning("invalid inventory item: " + ArgNum);
                    //just use the number
                    return "i" + ArgNum;
                  case leLow:
                  default:
                    //just use the number
                    return "i" + ArgNum;
                }
              }
            }
            else
            {
              //always refer to the object by number if no object file loaded
              return "i" + ArgNum;
            }

          case ArgTypeEnum.atStr:
            if (ArgNum == 0)
            {
              return agResDef[5].Name;
            }
            else
            {
              //not a reserved data type
              return "s" + ArgNum;
            }

          case ArgTypeEnum.atCtrl:
            //not a reserved data type
            return "c" + ArgNum;

          case ArgTypeEnum.atWord:
            //convert argument to a 'one-based' Value
            //so it is consistent with the syntax used
            //in the agi //print// commands
            return "w" + (ArgNum + 1).ToString();
        }

        //shouldn't be possible to get here, but compiler wants a return statement here
        return "";
      }
      /*
            Function ReadMessages(bytData() As Byte, lngMsgStart As Long) As Boolean
      {
              lngEndMsgSection As Long
              MessageStart(255) As Long
              intCurMsg As Integer
              EncryptionStart As Long
              strMessage As String
              blnEndOfMsg As Boolean
              bytInput As Byte
              NumMessages As Integer

              On Error GoTo ErrHandler

              //NOTE: There is no message 0 (this is not supported by the file format).
              // the word which corresponds to message 0 offset is used to hold the
              //end of text ptr so AGI can decrypt the message text when the logic
              //is initially loaded

              //set position to beginning of msg section,
              lngPos = lngMsgStart

              //set message section end initially to msgsection start
              lngEndMsgSection = lngMsgStart

              Set stlMsgs = New StringList

              //read in number of messages
              NumMessages = bytData[lngPos]
              lngPos++;
              if (NumMessages > 0) {
                //retrieve and adjust end of message section
                lngEndMsgSection = lngEndMsgSection + 256& * bytData(lngPos + 1) + bytData[lngPos]
                lngPos = lngPos + 2
                //loop through all messages, extract offset
                For intCurMsg = 1 To NumMessages
                  //set start of this msg as start of msg block, plus offset, plus one (for byte which gives number of msgs)
                  MessageStart(intCurMsg) = 256& * bytData(lngPos + 1) + bytData[lngPos] + lngMsgStart + 1
                  //validate msg start
                  if (MessageStart(intCurMsg) > UBound(bytData)) {
                    //invalid
                    strError = "Invalid message section data"
                    return;
                  }
                  lngPos = lngPos + 2
                Next intCurMsg

                //mark start of encrypted data (to align encryption string)
                EncryptionStart = lngPos

                //now read all messages, decrypting in the process
                For intCurMsg = 1 To NumMessages
                  strMessage = vbNullString
                  //if msg start points to a valid msg
                  if (MessageStart(intCurMsg) > 0 && MessageStart(intCurMsg) >= EncryptionStart) {
                    lngPos = MessageStart(intCurMsg)
                    blnEndOfMsg = false
                    Do
                      bytInput = bytData[lngPos] Xor bytEncryptKey((lngPos - EncryptionStart) Mod 11)
                      lngPos++;
                      if ((bytInput == 0) || (lngPos > UBound(bytData()))) {
                        blnEndOfMsg = true
                      } else {
                        switch (bytInput) {
                        case 0xA
                          strMessage = strMessage + "\n"
                        break; case Is < 32
                          strMessage = strMessage + "\x" + Hex2(bytInput)
                        break; case 0x22
                          strMessage = strMessage + "\"""
                        break; case 0x5C
                          strMessage = strMessage + "//"
                        break; case 0x7F
                          strMessage = strMessage + "\x7F"
                        break; case 0xFF
                          strMessage = strMessage + "\xFF"
                        default:
                          strMessage = strMessage + Chr$(bytInput)
                        }
                      }
                    Loop Until blnEndOfMsg

                    stlMsgs.Add QUOTECHAR + strMessage + QUOTECHAR
                    blnMsgExists(intCurMsg) = true
                  } else {
                    //add nothing (so numbers work out)
                    stlMsgs.Add vbNullString
                    blnMsgExists(intCurMsg) = false
                  }
                Next intCurMsg
              }

              //return true
              return true;

            ErrHandler:
              //save error message
              strError = "Unhandled error while decoding messages (" + CStr(Err.Number) + ": " + Err.Description + ") at position " + CStr(lngPos)
              Err.Clear
              ReadMessages = false
            }


            Function DecodeIf(bytData() As Byte, stlOut As StringList) As Boolean
      {
              blnInOrBlock As Boolean
              blnInNotBlock As Boolean
              blnFirstCmd As Boolean
              intArg1Val As Integer
              bytArg2Val As Byte
              bytCurByte As Byte
              blnIfFinished As Boolean
              bytNumSaidArgs As Byte
              lngWordGroupNum As Long
              strLine As String, strArg As String
              bytCmd As Byte
              strWarningLine() As String
              i As Long
              intCharCount As Integer

              On Error GoTo ErrHandler

              blnIfFinished = false
              blnFirstCmd = true
              blnInOrBlock = false
              strLine = MultStr("  ", bytBlockDepth) + IF_TOKEN

              //main loop - read in logic, one byte at a time, and write text accordingly
              Do
                //always reset //NOT// block status to false
                blnInNotBlock = false

                //read next byte from input stream
                bytCurByte = bytData[lngPos]
                //and increment pointer
                lngPos++;

                //first, check for an //OR//
                if (bytCurByte == 0xFC) {
                  blnInOrBlock = !blnInOrBlock
                  if (blnInOrBlock) {
                    if (!blnFirstCmd) {
                      strLine = strLine + AND_TOKEN
                      stlOut.Add (strLine)
                      strLine = MultStr("  ", bytBlockDepth) + "    "
                      blnFirstCmd = true
                    }
                    strLine = strLine + "("
                  } else {
                    strLine = strLine + ")"
                  }

                  //now get next byte, and continue checking
                  bytCurByte = bytData[lngPos]
                  lngPos++;
                }

                //special check needed in case two 0xFCs are in a row, e.g. (a || b) && (c || d)
                if ((bytCurByte == 0xFC) && (!blnInOrBlock)) {
                  strLine = strLine + AND_TOKEN
                  stlOut.Add (strLine)
                  strLine = MultStr("  ", bytBlockDepth) + "    "
                  blnFirstCmd = true
                  strLine = strLine + "("
                  blnInOrBlock = true
                  bytCurByte = bytData[lngPos]
                  lngPos++;
                }

                //check for //not// command
                if (bytCurByte == 0xFD) {   // NOT
                  blnInNotBlock = true
                  bytCurByte = bytData[lngPos]
                  lngPos++;
                }

                //check for valid test command
                if ((bytCurByte > 0) && (bytCurByte <= agTestCmdCol.Count)) {

                  if (!blnFirstCmd) {
                    if (blnInOrBlock) {
                      strLine = strLine + OR_TOKEN
                    } else {
                      strLine = strLine + AND_TOKEN
                    }
                    stlOut.Add (strLine)
                    strLine = MultStr("  ", bytBlockDepth) + "    "
                  }
                  bytCmd = bytCurByte
                  if (agMainLogSettings.SpecialSyntax && (bytCmd >= 1 && bytCmd <= 6)) {
                    //get first argument
                    bytCurByte = bytData[lngPos]
                    lngPos++;
                    bytArg2Val = bytData[lngPos]
                    lngPos++;
                    strLine = strLine + AddSpecialIf(bytCmd, bytCurByte, bytArg2Val, blnInNotBlock)
                  } else {
                    if (blnInNotBlock) {
                      strLine = strLine + NOT_TOKEN
                    }

                    strLine = strLine + agTestCmdCol(bytCmd).Name + "("

                    intArgStart = Len(strLine)
                    if (bytCmd == 14) { // said command
                      bytNumSaidArgs = bytData[lngPos]
                      lngPos++;
                      For intArg1Val = 1 To bytNumSaidArgs
                        lngWordGroupNum = 256 * bytData(lngPos + 1) + bytData[lngPos]
                        lngPos = lngPos + 2
                        //if a game is loaded,
                        if (agGameLoaded) {
                          //enable error trapping to catch any nonexistent words
                          On Error Resume Next
                          //if word exists,
                          strLine = strLine + QUOTECHAR + agVocabWords.GroupN(lngWordGroupNum).GroupName + QUOTECHAR
                          if (Err.Number != 0) {
                            switch (agMainLogSettings.ErrorLevel) {
                            case leHigh
                              //raise error
                              strError = "unknown word group (" + CStr(lngWordGroupNum) + ") at position " + CStr(lngPos)
                              return;
                            break; case leMedium
                              //add the word by its number
                              strLine = strLine + CStr(lngWordGroupNum)
                              //set warning text
                              AddDecodeWarning "unknown word: " + CStr(lngWordGroupNum)
                            break; case leLow
                              //add the word by its number
                              strLine = strLine + CStr(lngWordGroupNum)
                            }
                          }
                          //reset error trapping
                          On Error GoTo 0
                        } else {
                          //alwys use word number as the argument
                          strLine = strLine + CStr(lngWordGroupNum)
                        }

                        if (intArg1Val < bytNumSaidArgs) {
                          strLine = strLine + ", "
                        }
                      Next intArg1Val

                    } else {
                      //if at least one arg
                      if (agTestCmdCol(bytCmd).ArgType.Length > 0) {
                        For intArg1Val = 0 To agTestCmdCol(bytCmd).ArgType.Length - 1
                          bytCurByte = bytData[lngPos]
                          lngPos++;
                          //get arg Value
                          strArg = ArgValue(bytCurByte, agTestCmdCol(bytCmd).ArgType(intArg1Val))
                          //if message error (no string returned)
                          if (LenB(strArg) == 0) {
                            //error string set by ArgValue function
                            return;
                          }

                          //if message
                          if (agTestCmdCol(bytCmd).ArgType(intArg1Val) == ArgTypeEnum.atMsg) {
                            //split over additional lines, if necessary
                            Do
                              //if this message is too long to add to current line,
                              if (Len(strLine) + Len(strArg) > MAX_LINE_LEN) {
                                //determine number of characters availableto add to this line
                                intCharCount = MAX_LINE_LEN - Len(strLine)
                                //determine longest available section of message that can be added
                                //without exceeding max line length
                                Do
                                  intCharCount = intCharCount - 1
                                Loop Until (intCharCount = 1) || (Mid(strArg, intCharCount, 1) = " ")
                                //if no space is found to split up the line
                                if (intCharCount <= 1) {
                                  //just split it without worrying about a space
                                  intCharCount = MAX_LINE_LEN - Len(strLine)
                                }
                                //add the section of the message that fits on this line
                                strLine = strLine + Left(strArg, intCharCount) + QUOTECHAR
                                strArg = Mid(strArg, intCharCount + 1, Len(strArg) - intCharCount)
                                //add line
                                stlOut.Add strLine
                                //create indent (but don//t exceed 20 spaces (to ensure msgs aren//t split
                                //up into small chunks)
                                if (intArgStart >= MAX_LINE_LEN - 20) {
                                  intArgStart = MAX_LINE_LEN - 20
                                }
                                strLine = MultStr(" ", CByte(intArgStart)) + QUOTECHAR
                              } else {
                                //not too long; add the message to current line
                                strLine = strLine + strArg
                                strArg = vbNullString
                              }
                            //continue adding new lines until entire message is split and added
                            Loop Until strArg = vbNullString
                          } else {
                            //just add it
                            strLine = strLine + strArg
                          }

                          //if more arguments needed,
                          if (intArg1Val < agTestCmdCol(bytCmd).ArgType.Length - 1) {
                            strLine = strLine + ", "
                          }
                        Next intArg1Val
                      }
                    }
                    strLine = strLine + ")"
                  }
                  blnFirstCmd = false
                  //add warning if this is the unknown test19 command
                  if (bytCmd == 19) {
                    //set warning text
                    AddDecodeWarning "unknowntest19 is only valid in Amiga AGI versions"
                  }
                } else if ( bytCurByte == 0xFF) {
                  //done with if block; add //then//
                  strLine = strLine + Replace(THEN_TOKEN, "%1", vbCrLf + ' '.Repeat(bytBlockDepth * 2 + 2))
                  //(SkipToEndIf verified that max block depth is not exceeded)
                  //increase block depth counter
                  bytBlockDepth = bytBlockDepth + 1
                  Block[bytBlockDepth).IsIf = true
                  Block[bytBlockDepth).Length = 256 * bytData(lngPos + 1) + bytData[lngPos]
                  lngPos = lngPos + 2
                  //check for length of zero
                  if (Block[bytBlockDepth).Length == 0 && agMainLogSettings.ErrorLevel == leMedium) {
                    //set warning text
                    AddDecodeWarning "this block contains no commands"
                  }

                  //validate end pos
                  Block[bytBlockDepth).EndPos = Block[bytBlockDepth).Length + lngPos
                  if (Block[bytBlockDepth).EndPos > UBound(bytData()) - 1) {
                    switch (agMainLogSettings.ErrorLevel) {
                    //if error level is high, SkipToEndIf catches this condition
                    case leMedium
                      //adjust to end
                      Block[bytBlockDepth).EndPos = UBound(bytData()) - 1
                      //set warning text
                      AddDecodeWarning "block end past end of resource; adjusted to end of resource"
                    break; case leLow
                      //adjust to end
                      Block[bytBlockDepth).EndPos = UBound(bytData()) - 1
                    }
                  }
                  //verify block ends before end of previous block
                  //(i.e. it//s properly nested)
                  if (Block[bytBlockDepth).EndPos > Block[bytBlockDepth - 1).EndPos) {
                    //block is outside the previous block nest;
                    //this is an abnormal situation
                    //if error level is high; this would have been
                    //caught in SkipToEndIf;
                    if (agMainLogSettings.ErrorLevel == leMedium) {
                      //set warning text
                      AddDecodeWarning "Block end outside of nested block (" + CStr(Block[bytBlockDepth).JumpPos) + ") at position " + CStr(lngPos)
                    }

                    //need to simulate this block by using else and goto
                    Block[bytBlockDepth).IsOutside = true
                    Block[bytBlockDepth).JumpPos = Block[bytBlockDepth).EndPos
                    Block[bytBlockDepth).EndPos = Block[bytBlockDepth - 1).EndPos
                  }
                  stlOut.Add (strLine)
                  //if any warnings
                  if (blnWarning) {
                    //add warning lines
                    strWarningLine = Split(strWarning, "|")
                    For i = 0 To UBound(strWarningLine())
                      stlOut.Add MultStr("  ", bytBlockDepth) + CMT_TOKEN + "WARNING: " + strWarningLine(i)
                    Next i
                    //reset warning flag + string
                    blnWarning = false
                    strWarning = vbNullString
                  }

                  strLine = MultStr("  ", bytBlockDepth)
                  blnIfFinished = true
                } else {
                  //unknown test command
                  strError = "Unknown test command (" + CStr(bytCurByte) + ") at position " + CStr(lngPos)
                  return;
                }
              Loop Until blnIfFinished
              return true;

            ErrHandler:
              //////Debug.Assert false
              Resume
              //unknown test command
              strError = "Unhandled error (" + CStr(Err.Number) + ": " + Err.Description + ")" + vbCr + vbCr + "at position " + CStr(lngPos) + " in DecodeIf"
            }

            Function SkipToEndIf(bytData() As Byte) As Boolean
             {
              //used by the find label method
              //it moves the cursor to the end of the current if
              //statement

              CurArg As Byte
              CurByte As Byte
              IfFinished As Boolean
              NumSaidArgs As Byte
              ThisCommand As Byte
              i As Long

              On Error GoTo ErrHandler

              IfFinished = false
              Do

                CurByte = bytData[lngPos]
                lngPos++;
                if (CurByte == 0xFC) {
                  CurByte = bytData[lngPos]
                  lngPos++;
                }
                if (CurByte == 0xFC) {
                  CurByte = bytData[lngPos]
                  lngPos++; // we may have 2 0xFCs in a row, e.g. (a || b) && (c || d)
                }
                if (CurByte == 0xFD) {
                  CurByte = bytData[lngPos]
                  lngPos++;
                }

                if ((CurByte > 0) && (CurByte <= agTestCmdCol.Count)) {
                  ThisCommand = CurByte
                  if (ThisCommand == 14) { // said command
                    //read in number of arguments
                    NumSaidArgs = bytData[lngPos]: lngPos++;
                    //move pointer to next position past these arguments
                    //(note that words use two bytes per argument, not one)
                    lngPos = lngPos + NumSaidArgs * 2
                  } else {
                    //move pointer to next position past the arguments for this command
                    lngPos = lngPos + agTestCmdCol(ThisCommand).ArgType.Length
                  }
                } else if ( CurByte == 0xFF) {
                  if (bytBlockDepth >= MAX_BLOCK_DEPTH - 1) {
                    strError = "Too many nested blocks (" + CStr(bytBlockDepth + 1) + ") at position " + CStr(lngPos)
                    return;
                  }
                  //increment block counter
                  bytBlockDepth = bytBlockDepth + 1
                  Block[bytBlockDepth).IsIf = true
                  Block[bytBlockDepth).Length = 256 * bytData(lngPos + 1) + bytData[lngPos]
                  lngPos = lngPos + 2
                  //check length of block
                  if (Block[bytBlockDepth).Length == 0) {
                    if (agMainLogSettings.ErrorLevel == leHigh) {
                      //consider zero block lengths as error
                      strError = "Encountered command block of length 0 at position " + CStr(lngPos)
                      return;
                    }
                  }
                  Block[bytBlockDepth).EndPos = Block[bytBlockDepth).Length + lngPos
                  if (Block[bytBlockDepth).EndPos > Block[bytBlockDepth - 1).EndPos) {
                    //block is outside the previous block nest;
                    //
                    //this is an abnormal situation;
                    switch (agMainLogSettings.ErrorLevel) {
                    break; case leHigh
                      //error
                      strError = "Block end outside of nested block (" + CStr(Block[bytBlockDepth).JumpPos) + ") at position" + CStr(lngPos)
                      return;

                    break; case leMedium, leLow
                      //need to simulate this block by using else and goto
                      Block[bytBlockDepth).IsOutside = true
                      Block[bytBlockDepth).JumpPos = Block[bytBlockDepth).EndPos
                      Block[bytBlockDepth).EndPos = Block[bytBlockDepth - 1).EndPos
                      //add a new goto item
                      //(since error level is medium or low
                      //dont need to worry about an invalid jumppos)

                      //if label is already created
                      For i = 1 To bytLabelCount
                        if (lngLabelPos(i) == Block[bytBlockDepth).JumpPos) {
                          break;
                        }
                      Next i
                      //if loop exited normally (i will equal bytLabelCount+1)
                      if (i == bytLabelCount + 1) {
                        //increment label Count
                        bytLabelCount = i
                        RePreserve lngLabelPos(bytLabelCount)
                        //save this label position
                        lngLabelPos(bytLabelCount) = Block[bytBlockDepth).JumpPos
                      }
                    }
                  }
                  IfFinished = true
                } else {
                  strError = "Unknown test command (" + CStr(CurByte) + ") at position " + CStr(lngPos)
                  return;
                }
              Loop Until IfFinished
              return true;

            ErrHandler:
              //if no error string
              if (LenB(strError) == 0) {
                strError = "Unhandled error while decoding logic at position " + CStr(lngPos) + "): " + strError
              }
            }

      */
      bool FindLabels(byte[] bytData)
      {
        int i, j, CurBlock;
        byte bytCurData;
        int tmpBlockLength;
        bool DoGoto;
        int LabelLoc;
        //finds all labels and stores them in an array;
        //they are then sorted and put in order so that
        //as each is found during decoding of the logic
        //the label is created, and the next label position
        //is moved to top of stack
        bytBlockDepth = 0;
        bytLabelCount = 0;
        int[] lngLabelPos = new int[0];
        lngPos = 2;
        do
        {
          //check to see if the end of a block has been found
          //start at most recent block and work up to oldest block
          for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--)
          {
            //if this position matches the end of this block
            if (Block[CurBlock].EndPos <= lngPos)
            {
              //verify it is exact
              if (Block[CurBlock].EndPos != lngPos)
              {
                //error
                strError = "Invalid goto position, or invalid if/then block length at position ";
                strError += (Block[CurBlock].EndPos - Block[CurBlock].Length);
                return false;
              }
              //take this block off stack
              bytBlockDepth--;
            }
          }
          //get next byte
          bytCurData = bytData[lngPos];
          lngPos++;
          if (bytCurData == 0xFF) //this byte points to start of an IF statement
          {
            //find labels associated with this if statement
            if (!SkipToEndIf(bytData))
            {
              //major error
              return false;
            }

          }
          else if (bytCurData <= AGICommands.Count) //byte is an AGI command
          {
            //skip over arguments to get next command
            //////Debug.Assert bytCurData != 178
            lngPos += agCmds[bytCurData].ArgType.Length;
          }
          else if (bytCurData == 0xFE)   //if the byte is a GOTO command
          {
            //reset goto status flag
            DoGoto = false;
            tmpBlockLength = 256 * bytData[lngPos + 1] + bytData[lngPos];
            lngPos += 2;
            //need to check for negative Value here
            if (tmpBlockLength > 0x7FFF)
            {
              //convert block length to negative value
              tmpBlockLength = tmpBlockLength - 0x10000;
            }
            //check to see if this 'goto' might be an 'else':
            //  - end of this block matches this position (the if-then part is done)
            //  - this block is identified as an IF block
            //  - this is NOT the main block
            //  - the flag to set elses as gotos is turned off
            if ((Block[bytBlockDepth].EndPos == lngPos) && (Block[bytBlockDepth].IsIf) && (bytBlockDepth > 0) && (!agMainLogSettings.ElseAsGoto))
            {
              //this block is now in the //else// part, so reset flag
              Block[bytBlockDepth].IsIf = false;
              Block[bytBlockDepth].IsOutside = false;
              //does this //else// statement line up to end at the same
              //point that the //if// statement does?
              //the end of this block is past where the //if// block ended OR
              //the block is negative (means jumping backward, so it MUST be a goto)
              //length of block doesn//t have enough room for code necessary to close the //else//
              if ((tmpBlockLength + lngPos > Block[bytBlockDepth - 1].EndPos) || (tmpBlockLength < 0) || (Block[bytBlockDepth].Length <= 3))
              {
                //this is a //goto// statement,
                DoGoto = true;
              }
              else
              {
                //this is an //else// statement;
                //readjust block end so the IF statement that owns this //else//
                //is ended correctly
                Block[bytBlockDepth].Length = tmpBlockLength;
                Block[bytBlockDepth].EndPos = Block[bytBlockDepth].Length + lngPos;
              }
            }
            else
            {
              //this is a goto statement (or an else statement while mGotos flag is false)
              DoGoto = true;
            }
            // goto
            if (DoGoto)
            {
              LabelLoc = tmpBlockLength + lngPos;
              if (LabelLoc > bytData.Length - 2)
              {
                //if error level is high (medium and low are handled in DecodeLogic)
                if (agMainLogSettings.ErrorLevel == leHigh)
                {
                  strError = "Goto destination past end of logic (" + LabelLoc + ")" + "at position " + lngPos;
                  return false;
                }
              }
              //if label is already created
              for (i = 1; i <= bytLabelCount; i++)
              {
                if (lngLabelPos[i] == LabelLoc)
                {
                  break;
                }
              }
              //if loop exited normally (i will equal bytLabelCount+1)
              if (i == bytLabelCount + 1)
              {
                //increment label Count
                bytLabelCount++;
                Array.Resize(ref lngLabelPos, bytLabelCount);
                //save this label position
                lngLabelPos[bytLabelCount] = LabelLoc;
              }
            }
          }
          else
          {
            //if not a valid command not implemented in this version
            if (bytCurData > 182)
            {
              //major error
              strError = "Unknown action command (" + bytCurData + ") at position " + lngPos;
              return false;
            }
          }
        }
        while (lngPos < lngMsgSecStart); //Loop Until (lngPos >= lngMsgSecStart)

        //now sort labels, if found
        if (bytLabelCount > 1)
        {
          for (i = 1; i <= bytLabelCount - 1; i++)
          {
            for (j = i + 1; j <= bytLabelCount; j++)
            {
              if (lngLabelPos[j] < lngLabelPos[i])
              {
                LabelLoc = lngLabelPos[i];
                lngLabelPos[i] = lngLabelPos[j];
                lngLabelPos[j] = LabelLoc;
              }
            }
          }
        }
        //clear block info (don't overwrite main block)
        for (i = 1; i <= MAX_BLOCK_DEPTH; i++)
        {
          Block[i].EndPos = 0;
          Block[i].IsIf = false;
          Block[i].IsOutside = false;
          Block[i].JumpPos = 0;
          Block[i].Length = 0;
        }
        //return success
        return true;

        //ErrHandler:
        //  //if no error string
        //  if (strError.Length == 0) 
        //  {
        //    //use a default
        //    strError = "Unhandled error in FindLabels (" + Err.Number + ": " + Err.Description + ") at position " + lngPos;
        //  }
        //  FindLabels = false;
      }
      void DisplayMessages(List<string> stlOut)
      {
        int lngMsg;
        //need to adjust references to the Messages stringlist object by one
        //since the list is zero based, but messages are one-based.
        stlOut.Add(CMT_TOKEN + "Messages");
        for (lngMsg = 1; lngMsg <= stlMsgs.Count; lngMsg++)
        {
          if (blnMsgExists[lngMsg] && ((agMainLogSettings.ShowAllMessages) || !blnMsgUsed[lngMsg]))
          {
            stlOut.Add(MSG_LINE.Replace(ARG1, lngMsg.ToString()).Replace(ARG2, stlMsgs[lngMsg - 1]));
          }
        }
      }
      string AddSpecialCmd(byte[] bytData, byte bytCmd)
      {
        byte bytArg1, bytArg2;
        //get first argument
        bytArg1 = bytData[lngPos];
        lngPos++;
        switch (bytCmd)
        {
          case 0x1:  // increment
            return "++" + ArgValue(bytArg1, ArgTypeEnum.atVar);
          case 0x2:  // decrement
            return "--" + ArgValue(bytArg1, ArgTypeEnum.atVar);
          case 0x3:  // assignn
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " = " + ArgValue(bytArg2, ArgTypeEnum.atNum, bytArg1);
          case 0x4:  // assignv
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " = " + ArgValue(bytArg2, ArgTypeEnum.atVar);
          case 0x5:  // addn
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + "  += " + ArgValue(bytArg2, ArgTypeEnum.atNum);
          case 0x6:  // addv
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + "  += " + ArgValue(bytArg2, ArgTypeEnum.atVar);
          case 0x7:  // subn
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " -= " + ArgValue(bytArg2, ArgTypeEnum.atNum);
          case 0x8:  // subv
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " -= " + ArgValue(bytArg2, ArgTypeEnum.atVar);
          case 0x9:  // lindirectv
            bytArg2 = bytData[lngPos];
            lngPos++;
            return "*" + ArgValue(bytArg1, ArgTypeEnum.atVar) + " = " + ArgValue(bytArg2, ArgTypeEnum.atVar);
          case 0xA:  // rindirect
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " = *" + ArgValue(bytArg2, ArgTypeEnum.atVar);
          case 0xB:  // lindirectn
            bytArg2 = bytData[lngPos];
            lngPos++;
            return "*" + ArgValue(bytArg1, ArgTypeEnum.atVar) + " = " + ArgValue(bytArg2, ArgTypeEnum.atNum);
          case 0xA5: // mul.n
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " *= " + ArgValue(bytArg2, ArgTypeEnum.atNum);
          case 0xA6: // mul.v
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " *= " + ArgValue(bytArg2, ArgTypeEnum.atVar);
          case 0xA7: // div.n
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " /= " + ArgValue(bytArg2, ArgTypeEnum.atNum);
          case 0xA8: // div.v
            bytArg2 = bytData[lngPos];
            lngPos++;
            return ArgValue(bytArg1, ArgTypeEnum.atVar) + " /= " + ArgValue(bytArg2, ArgTypeEnum.atVar);
          default:
            return "";
        }
      }
      string AddSpecialIf(byte bytCmd, byte bytArg1, byte bytArg2, bool NOTOn)
      {
        string retval = ArgValue(bytArg1, ArgTypeEnum.atVar);
        switch (bytCmd)
        {
          case 1:
          case 2:            // equaln or equalv
            //if NOT in effect,
            if (NOTOn)
            {
              //test for not equal
              retval += NOT_EQUAL_TEST_TOKEN;
            }
            else
            {
              //test for equal
              retval += EQUAL_TEST_TOKEN;
            }
            //if command is comparing variables,
            if (bytCmd == 2)
            {
              //variable
              retval += ArgValue(bytArg2, ArgTypeEnum.atVar, bytArg1);
            }
            else
            {
              //add number
              retval += ArgValue(bytArg2, ArgTypeEnum.atNum, bytArg1);
            }
            break;
          case 3:
          case 4:           // lessn, lessv
                            //if NOT is in effect,
            if (NOTOn)
            {
              //test for greater than or equal
              retval += " >= ";
            }
            else
            {
              //test for less than
              retval += " < ";
            }
            //if command is comparing variables,
            if (bytCmd == 4)
            {
              retval += ArgValue(bytArg2, ArgTypeEnum.atVar, bytArg1);
            }
            else
            {
              //number string
              retval += ArgValue(bytArg2, ArgTypeEnum.atNum, bytArg1);
            }

            break;
          case 5:
          case 6:            // greatern, greaterv
                             //if NOT is in effect,
            if (NOTOn)
            {
              //test for less than or equal
              retval += " <= ";
            }
            else
            {
              //test for greater than
              retval += " > ";
            }
            //if command is comparing variables,
            if (bytCmd == 6)
            {
              retval += ArgValue(bytArg2, ArgTypeEnum.atVar, bytArg1);
            }
            else
            {
              //number string
              retval += ArgValue(bytArg2, ArgTypeEnum.atNum, bytArg1);
            }
            break;
        }
        return retval;
      }
      void AddBlockEnds(List<string> stlIn)
      {
        int CurBlock, i;

        for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--)
        {
          //why would a less than apply here?
          //FOUND IT!!! here is a case where it is less than!!!
          //if (Block(CurBlock).EndPos <= lngPos) {
          if (Block[CurBlock].EndPos == lngPos)
          {
            //check for unusual case where an if block ends outside
            //the if block it is nested in
            if (Block[CurBlock].IsOutside)
            {
              //add an else
              stlIn.Add(MultStr("  ", bytBlockDepth) + ENDIF_TOKEN);
              if (agMainLogSettings.ElseAsGoto)
              {
                stlIn.Add(MultStr("  ", bytBlockDepth - 1) + GOTO_TOKEN);
              }
              else
              {
                stlIn.Add(MultStr("  ", bytBlockDepth - 1) + ELSE_TOKEN.Replace(ARG1, NEWLINE + new String(' ', bytBlockDepth * 2)));
              }
              //add a goto
              for (i = 1; i <= bytLabelCount; i++)
              {
                if (lngLabelPos[i] == Block[CurBlock].JumpPos)
                {
                  stlIn.Add(MultStr("  ", bytBlockDepth) + GOTO_TOKEN.Replace(ARG1, "Label" + i) + EOL_TOKEN);
                  break;
                }
              }
            }
            //add end if
            stlIn.Add(MultStr("  ", CurBlock) + ENDIF_TOKEN);
            bytBlockDepth--;
          }
        }
      }
    }
  }
}
