using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Common.WinAGI;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.LogicErrorLevel;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.AGICommands;
using static WinAGI.Engine.AGITestCommands;
using System.Diagnostics;

namespace WinAGI.Engine
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
    const string D_TKN_NOT = "!";
    const string D_TKN_IF = "if (";
    const string D_TKN_THEN = ")%1{"; //where %1 is a line feed plus indent at current level
    const string D_TKN_ELSE = "else%1{"; //where %1 is a line feed plus indent at current level
    const string D_TKN_ENDIF = "}";
    const string D_TKN_GOTO = "goto(%1)";
    const string D_TKN_EOL = ";";
    const string D_TKN_AND = " && ";
    const string D_TKN_OR = " || ";
    const string D_TKN_EQUAL = " == ";
    const string D_TKN_NOT_EQUAL = " != ";
    const string D_TKN_COMMENT = "[ ";
    const string D_TKN_MESSAGE = "#message %1 %2";

    static bool blnWarning;
    static string strWarning;

    internal static List<string> DecodeLogic(byte[] bytData, bool DecryptMsg)
    {
      byte bytCurData;
      bool blnGoto;
      byte bytCmd;
      int tmpBlockLen;
      int intArg;
      int lngNextLabel = 0;
      int lngLabelLoc;
      string[] strWarningLine;
      int i, j;
      string strArg;
      int intCharCount;
      List<string> stlOutput = new List<string> { };

      //if nothing in the resource,
      if (bytData.Length == 0) {
        //single //'return()' command
        stlOutput.Add("return();");
        return stlOutput;
      }
      //clear block info
      for (i = 0; i < MAX_BLOCK_DEPTH; i++) {
        Block[i].EndPos = 0;
        Block[i].IsIf = false;
        Block[i].IsOutside = false;
        Block[i].JumpPos = 0;
        Block[i].Length = 0;
      }

      //extract beginning of msg section
      //(add two because in the AGI executable, the message section start is referenced
      //relative to byte 7 of the header. When extracted, the resource data
      //begins with byte 5 of the header:
      //
      // byte 00: high byte of resource start signature (always 0x12)
      // byte 01: low byte of resource start signature (always 0x34)
      // byte 02: VOL file number
      // byte 03: low byte of logic script length
      // byte 04: high byte of logic script length
      // byte 05: low byte of offset to message section start
      // byte 06: high byte of offset to message section start
      // byte 07: begin logic data

      lngMsgSecStart = bytData[1] * 256 + bytData[0] + 2;

      //if can't read messges,
      if (!ReadMessages(bytData, lngMsgSecStart, DecryptMsg)) {
        //raise error
        goto ErrHandler;
      }

      //set main block info
      Block[0].IsIf = false;
      Block[0].EndPos = lngMsgSecStart;
      Block[0].IsOutside = false;
      Block[0].Length = lngMsgSecStart;

      //set error flag
      strError = "";
      //locate labels, and mark them (this also validates all command bytes to be <=181)
      if (!FindLabels(bytData)) {
        //use error string set by findlabels
        goto ErrHandler;
      }

      //reset block depth and data position
      bytBlockDepth = 0;
      lngPos = 2;
      if (bytLabelCount > 0) {
        lngNextLabel = 1;
      }

      //main loop
      do {
        AddBlockEnds(stlOutput);
        //check for label position
        if (lngLabelPos[lngNextLabel] == lngPos) {
          stlOutput.Add("Label" + lngNextLabel + ":");
          lngNextLabel++;
          if (lngNextLabel > bytLabelCount) {
            lngNextLabel = 0;
          }
        }
        bytCurData = bytData[lngPos];
        lngPos++;
        if (bytCurData == 0xFF) {
          //this byte starts an IF statement
          if (!DecodeIf(bytData, stlOutput)) {
            goto ErrHandler;
          }
        }
        else if (bytCurData == 0xFE) {
          //this byte is a 'goto' or 'else'
          blnGoto = false;
          tmpBlockLen = 256 * bytData[lngPos + 1] + bytData[lngPos];
          lngPos += 2;
          //need to check for negative Value here
          if (tmpBlockLen > 0x7FFF) {
            // convert to negative number
            tmpBlockLen -= 0x10000;
          }
          if ((Block[bytBlockDepth].EndPos == lngPos) && (Block[bytBlockDepth].IsIf) && (bytBlockDepth > 0) && (!agMainLogSettings.ElseAsGoto)) {
            Block[bytBlockDepth].IsIf = false;
            Block[bytBlockDepth].IsOutside = false;
            if ((tmpBlockLen + lngPos > Block[bytBlockDepth - 1].EndPos) || (tmpBlockLen < 0) || (Block[bytBlockDepth].Length <= 3)) {
              blnGoto = true;
            }
            else {
              stlOutput.Add(MultStr("  ", bytBlockDepth) + D_TKN_ENDIF);
              if (agMainLogSettings.ElseAsGoto) {
                stlOutput.Add(MultStr("  ", bytBlockDepth - 1) + D_TKN_GOTO);
              }
              else {
                stlOutput.Add(MultStr("  ", bytBlockDepth - 1) + D_TKN_ELSE.Replace(ARG1, NEWLINE + new String(' ', bytBlockDepth * 2)));
              }
              Block[bytBlockDepth].Length = tmpBlockLen;
              Block[bytBlockDepth].EndPos = Block[bytBlockDepth].Length + lngPos;
            }
          }
          else {
            blnGoto = true;
          }
          // goto
          if (blnGoto) {
            lngLabelLoc = tmpBlockLen + lngPos;
            //label already verified in FindLabels; add warning if necessary
            if (lngLabelLoc > bytData.Length - 2) {
              switch (agMainLogSettings.ErrorLevel) {
              //case leHigh - high level handled in FindLabels
              case leMedium:
                //set warning
                AddDecodeWarning("Goto destination past end of logic at position " + lngPos + " adjusted to end of logic");
                //adjust it to end of resource
                lngLabelLoc = bytData.Length - 1;
                break;
              case leLow:
                //adjust it to end of resource
                lngLabelLoc = bytData.Length - 1;
                break;
              }
            }
            for (i = 1; i <= bytLabelCount; i++) {
              if (lngLabelPos[i] == lngLabelLoc) {
                stlOutput.Add(MultStr("  ", bytBlockDepth) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                //if any warnings
                if (blnWarning) {
                  //add warning lines
                  strWarningLine = strWarning.Split("|");
                  for (j = 0; j < strWarningLine.Length; j++) {
                    stlOutput.Add(D_TKN_COMMENT + "WARNING: " + strWarningLine[j]);
                  }
                  //reset warning flag + string
                  blnWarning = false;
                  strWarning = "";
                }
                break;
              }
            }
          }
        }
        else if (bytCurData < MAX_CMDS) {
          //valid agi command (don't need to check for invalid command number;
          // they are all validated in FindLabels)
          //if this command is not within range of expected commands for targeted interpretr version,
          if (bytCurData > AGICommands.Count - 1) { //this byte is a command
            //show warning
            AddDecodeWarning("this command not valid for selected interpreter version (" + agIntVersion + ")");
          }
          bytCmd = bytCurData;
          string strCurrentLine = MultStr("  ", bytBlockDepth);
          if (agMainLogSettings.SpecialSyntax && (bytCmd >= 0x1 && bytCmd <= 0xB) || (bytCmd >= 0xA5 && bytCmd <= 0xA8)) {
            strCurrentLine += AddSpecialCmd(bytData, bytCmd);
          }
          else {
            strCurrentLine = strCurrentLine + agCmds[bytCmd].Name + "(";
            intArgStart = strCurrentLine.Length;
            for (intArg = 0; intArg < agCmds[bytCmd].ArgType.Length; intArg++) {
              bytCurData = bytData[lngPos];
              lngPos++;
              strArg = ArgValue(bytCurData, agCmds[bytCmd].ArgType[intArg]);
              //if showing reserved names && using reserved defines
              if (agResAsText && agUseRes) {
                //some commands use resources as arguments; substitute as appropriate
                switch (bytCmd) {
                case 122: //add.to.pic,    1st arg (V)
                  if (intArg == 0) {
                    if (agViews.Exists(bytCurData)) {
                      strArg = agViews[bytCurData].ID;
                    }
                    else {
                      //view doesn't exist
                      switch (agMainLogSettings.ErrorLevel) {
                      case leHigh:
                      case leMedium:
                        //set warning
                        AddDecodeWarning("view " + bytCurData + " in add.to.pic() does not exist");
                        break;
                      case leLow:
                        //do nothing
                        break;
                      }
                    }
                  }
                  break;
                case 22:  //call,          only arg (L)
                  if (agLogs.Exists(bytCurData)) {
                    strArg = agLogs[bytCurData].ID;
                  }
                  else {
                    //logic doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("logic " + bytCurData + " in call() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 175: //discard.sound, only arg (S)
                  if (agSnds.Exists(bytCurData)) {
                    strArg = agSnds[bytCurData].ID;
                  }
                  else {
                    //sound doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("sound " + bytCurData + " in discard.sound() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 32:  //discard.view,  only arg (V)
                  if (agViews.Exists(bytCurData)) {
                    strArg = agViews[bytCurData].ID;
                  }
                  else {
                    //view doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("view " + bytCurData + " in discard.view() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 20:  //load.logics,   only arg (L)
                  if (agLogs.Exists(bytCurData)) {
                    strArg = agLogs[bytCurData].ID;
                  }
                  else {
                    //logic doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("logic " + bytCurData + " in loadlogics() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 98:  //load.sound,    only arg (S)
                  if (agSnds.Exists(bytCurData)) {
                    strArg = agSnds[bytCurData].ID;
                  }
                  else {
                    //sound doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("sound " + bytCurData + " in load.sound() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 30:  //load.view,     only arg (V)
                  if (agViews.Exists(bytCurData)) {
                    strArg = agViews[bytCurData].ID;
                  }
                  else {
                    //view doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("view " + bytCurData + " in load.view() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 18:  //new.room,      only arg (L)
                  if (agLogs.Exists(bytCurData)) {
                    strArg = agLogs[bytCurData].ID;
                  }
                  else {
                    //logic doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("logic " + bytCurData + " in new.room() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 41:  //set.view,      2nd arg (V)
                  if (intArg == 1) {
                    if (agViews.Exists(bytCurData)) {
                      strArg = agViews[bytCurData].ID;
                    }
                    else {
                      //view doesn't exist
                      switch (agMainLogSettings.ErrorLevel) {
                      case leHigh:
                      case leMedium:
                        //set warning
                        AddDecodeWarning("view " + bytCurData + " in set.view() does not exist");
                        break;
                      case leLow:
                        //do nothing
                        break;
                      }
                    }
                  }
                  break;
                case 129: //show.obj,      only arg (V)
                  if (agViews.Exists(bytCurData)) {
                    strArg = agViews[bytCurData].ID;
                  }
                  else {
                    //view doesn't exist
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                    case leMedium:
                      //set warning
                      AddDecodeWarning("view " + bytCurData + " in show.obj() does not exist");
                      break;
                    case leLow:
                      //do nothing
                      break;
                    }
                  }
                  break;
                case 99:  //sound,         1st arg (S)
                  if (intArg == 0) {
                    if (agSnds.Exists(bytCurData)) {
                      strArg = agSnds[bytCurData].ID;
                    }
                    else {
                      //sound doesn't exist
                      switch (agMainLogSettings.ErrorLevel) {
                      case leHigh:
                      case leMedium:
                        //set warning
                        AddDecodeWarning("sound " + bytCurData + " in sound() does not exist");
                        break;
                      case leLow:
                        //do nothing
                        break;
                      }
                    }
                  }
                  break;
                case 150: //trace.info,    1st arg (L)
                  if (intArg == 0) {
                    if (agLogs.Exists(bytCurData)) {
                      strArg = agLogs[bytCurData].ID;
                    }
                    else {
                      //logic doesn't exist
                      switch (agMainLogSettings.ErrorLevel) {
                      case leHigh:
                      case leMedium:
                        //set warning
                        AddDecodeWarning("logic " + bytCurData + " in trace.info() does not exist");
                        break;
                      case leLow:
                        //do nothing
                        break;
                      }
                    }
                  }
                  break;
                }
              }

              //if message error (no string returned)
              if (strArg.Length == 0) {
                //error string set by ArgValue function
                goto ErrHandler;
              }
              //check for commands that use colors here
              switch (bytCmd) {
              case 105: //clear.lines, 3rd arg
                if (intArg == 2) {
                  if (Val(strArg) < 16) {
                    strArg = agResColor[(int)Val(strArg)].Name;
                  }
                }
                break;
              case 154: //clear.text.rect, 5th arg
                if (intArg == 4) {
                  if (Val(strArg) < 16) {
                    strArg = agResColor[(int)Val(strArg)].Name;
                  }
                }
                break;
              case 109: //set.text.attribute, all args
                if (Val(strArg) < 16) {
                  strArg = agResColor[(int)Val(strArg)].Name;
                }
                break;
              }

              //if message
              if (agCmds[bytCmd].ArgType[intArg] == atMsg) {
                //split over additional lines, if necessary
                do {
                  //if this message is too long to add to current line,
                  if (strCurrentLine.Length + strArg.Length > MAX_LINE_LEN) {
                    //determine number of characters availableto add to this line
                    intCharCount = MAX_LINE_LEN - strCurrentLine.Length;
                    //determine longest available section of message that can be added
                    //without exceeding max line length
                    if (intCharCount > 1) {
                      while (intCharCount > 1 && strArg[intCharCount - 1] != ' ') // Until (intCharCount <= 1) || (Mid$(strArg, intCharCount, 1) = " ")
                      {
                        intCharCount--;
                      }
                      //if no space is found to split up the line
                      if (intCharCount <= 1) {
                        //just split it without worrying about a space
                        intCharCount = MAX_LINE_LEN - strCurrentLine.Length;
                      }
                      //add the section of the message that fits on this line
                      strCurrentLine = strCurrentLine + Left(strArg, intCharCount) + QUOTECHAR;
                      strArg = Mid(strArg, intCharCount, strArg.Length - intCharCount);
                      //add line
                      stlOutput.Add(strCurrentLine);
                      //create indent (but don't exceed 20 spaces (to ensure msgs aren't split
                      //up into small chunks)
                      if (intArgStart >= MAX_LINE_LEN - 20) {
                        intArgStart = MAX_LINE_LEN - 20;
                      }
                      strCurrentLine = MultStr(" ", intArgStart) + QUOTECHAR;
                    }
                    else {
                      //line is messed up; just add it
                      strCurrentLine += strArg;
                      strArg = "";
                    }
                  }
                  else {
                    //not too long; add the message to current line
                    strCurrentLine += strArg;
                    strArg = "";
                  }
                }
                //continue adding new lines until entire message is split && added
                while (strArg != ""); // Until strArg = ""
              }
              else {
                //add arg
                strCurrentLine += strArg;
              }

              //if more arguments needed,
              if (intArg < agCmds[bytCmd].ArgType.Length - 1) {
                strCurrentLine += ", ";
              }
            } // Next intArg
            strCurrentLine += ")";
          }
          strCurrentLine += D_TKN_EOL;
          stlOutput.Add(strCurrentLine);
          //if any warnings
          if (blnWarning) {
            //add warning lines
            strWarningLine = strWarning.Split("|");
            for (i = 0; i < strWarningLine.Length; i++) {
              stlOutput.Add(D_TKN_COMMENT + "WARNING: " + strWarningLine[i]);
            }
            //reset warning flag + string
            blnWarning = false;
            strWarning = "";
          }
        }
      }
      while (lngPos < lngMsgSecStart); //Loop Until (lngPos >= lngMsgSecStart)
      // finish up
      AddBlockEnds(stlOutput);
      stlOutput.Add("");
      DisplayMessages(stlOutput);
      //return results
      return stlOutput;

      ErrHandler:
      Exception e = new Exception($"LogDecode Error ({strError})");
      e.HResult = WINAGI_ERR + 688;
      throw e;
    }
    static void AddDecodeWarning(string WarningText)
    {
      //if at least one warning already,
      if (blnWarning) {
        //add pipe character
        strWarning += "|";
      }
      else {
        //set warning flag
        blnWarning = true;
      }
      strWarning += WarningText;
    }
    static string ArgValue(byte ArgNum, ArgTypeEnum ArgType, int ArgComp = -1)
    {
      //if not showing reserved names (or if not using reserved defines)
      // AND not a msg (always substitute msgs)
      if ((!agResAsText || !agUseRes) && ArgType != ArgTypeEnum.atMsg) {
        //return simple Value
        return agArgTypPref[(int)ArgType] + ArgNum;
      }
      //add appropriate resdef name
      switch (ArgType) {
      case ArgTypeEnum.atNum:
        switch (ArgComp) {
        case 2:
        case 5:  //edgecode
          if (ArgNum <= 4) {
            return agEdgeCodes[ArgNum].Name;
          }
          else {
            return ArgNum.ToString();
          }
        case 6: //egodir
          if (ArgNum <= 8) {
            return agEgoDir[ArgNum].Name;
          }
          else {
            return ArgNum.ToString();
          }
        case 20: //computer type
          if (ArgNum <= 8) {
            return agCompType[ArgNum].Name;
          }
          else {
            return ArgNum.ToString();
          }
        case 26: //video
          if (ArgNum <= 4) {
            return agVideoMode[ArgNum].Name;
          }
          else {
            return ArgNum.ToString();
          }
        default:
          //use default
          return ArgNum.ToString();
        }
      case ArgTypeEnum.atVar:
        //if a predefined,
        if (ArgNum <= 26) {
          return agResVar[ArgNum].Name;
        }
        else {
          //not a reserved data type
          return "v" + ArgNum;
        }
      case ArgTypeEnum.atFlag:
        //if a predefined
        if (ArgNum <= 16) {
          return agResFlag[ArgNum].Name;
          //check for special case of f20 (only if version 3.002102 or higher)
        }
        else if (ArgNum == 20 && Val(agIntVersion) >= 3.002102) {
          return agResFlag[17].Name;
        }
        else {
          //not a reserved data type
          return "f" + ArgNum;
        }
      case ArgTypeEnum.atMsg:
        blnMsgUsed[ArgNum] = true;
        //if this message exists,
        if (blnMsgExists[ArgNum]) {
          //begin by using entire message as the chunk to add to current line
          return stlMsgs[ArgNum - 1];
        }
        else {
          //message doesn't exist
          switch (agMainLogSettings.ErrorLevel) {
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
        if (ArgNum == 0) {
          return "ego";
        }
        else {
          //not a reserved data type
          return "o" + ArgNum;
        }
      case ArgTypeEnum.atIObj:
        //if a game is loaded AND OBJECT file is loaded,
        if (agGameLoaded && agInvObj.Loaded) {
          if (ArgNum < agInvObj.Count) {
            //if object is unique
            if (agInvObj[ArgNum].Unique) {
              //double check if item is a question mark
              if (agInvObj[ArgNum].ItemName == "?") {
                //use the inventory item number, and post a warning
                AddDecodeWarning("reference to invalid inventory object ('?')");
                return "i" + ArgNum;
              }
              else {
                //a unique, non-questionmark item- use it's string Value
                return QUOTECHAR + agInvObj[ArgNum].ItemName.Replace(QUOTECHAR, "\"") + QUOTECHAR;
              }
            }
            else {
              //use obj number instead
              if (agMainLogSettings.ErrorLevel != leLow) {
                AddDecodeWarning("non-unique object: '" + agInvObj[ArgNum].ItemName + "'");
              }
              return "i" + ArgNum;
            }
          }
          else {
            switch (agMainLogSettings.ErrorLevel) {
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
        else {
          //always refer to the object by number if no object file loaded
          return "i" + ArgNum;
        }
      case ArgTypeEnum.atStr:
        if (ArgNum == 0) {
          return agResDef[5].Name;
        }
        else {
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
    static bool ReadMessages(byte[] bytData, int lngMsgStart, bool Decrypt)
    {
      int lngEndMsgSection;
      int[] MessageStart = new int[256];
      int intCurMsg;
      int EncryptionStart;
      string strMessage;
      bool blnEndOfMsg;
      byte bytInput;
      int NumMessages;

      //NOTE: There is no message 0 (this is not supported by the file format).
      // the word which corresponds to message 0 offset is used to hold the
      //end of text ptr so AGI can decrypt the message text when the logic
      //is initially loaded

      //set position to beginning of msg section,
      lngPos = lngMsgStart;

      //set message section end initially to msgsection start
      lngEndMsgSection = lngMsgStart;

      stlMsgs = new List<string> { };

      //read in number of messages
      NumMessages = bytData[lngPos];
      lngPos++;
      if (NumMessages > 0) {
        //retrieve and adjust end of message section
        lngEndMsgSection = lngEndMsgSection + 256 * bytData[lngPos + 1] + bytData[lngPos];
        lngPos += 2;
        //loop through all messages, extract offset
        for (intCurMsg = 1; intCurMsg <= NumMessages; intCurMsg++) {
          //set start of this msg as start of msg block, plus offset, plus one (for byte which gives number of msgs)
          MessageStart[intCurMsg] = 256 * bytData[lngPos + 1] + bytData[lngPos] + lngMsgStart + 1;
          //validate msg start
          if (MessageStart[intCurMsg] >= bytData.Length) {
            //invalid
            strError = "Invalid message section data";
            return false;
          }
          lngPos += 2;
        } //Next intCurMsg

        //mark start of encrypted data (to align encryption string)
        EncryptionStart = lngPos;

        //now read all messages, decrypting in the process, if necessary
        for (intCurMsg = 1; intCurMsg <= NumMessages; intCurMsg++) {
          strMessage = "";
          //if msg start points to a valid msg
          if (MessageStart[intCurMsg] > 0 && MessageStart[intCurMsg] >= EncryptionStart) {
            lngPos = MessageStart[intCurMsg];
            blnEndOfMsg = false;
            do {
              bytInput = bytData[lngPos];
              // v3 compressed resources don't use encryption
              if (Decrypt) {
                bytInput ^= (bytEncryptKey[(lngPos - EncryptionStart) % 11]);
              }
              lngPos++;
              if ((bytInput == 0) || (lngPos >= bytData.Length)) {
                blnEndOfMsg = true;
              }
              else {
                if (bytInput == 0xA) {
                  strMessage += "\\n";
                }
                else if (bytInput < 32) {
                  strMessage = strMessage + "\\x" + bytInput.ToString("x2");
                }
                else if (bytInput == 0x22) {
                  strMessage += "\\\"";
                }
                else if (bytInput == 0x5C) {
                  strMessage += "\\\\"; //TODO: check this!!!!
                }
                else if (bytInput == 0x7F) {
                  strMessage += "\\x7F";
                }
                else if (bytInput == 0xFF) {
                  strMessage += "\\xFF";
                }
                else {
                  strMessage += (char)(bytInput);
                } //End Select
              }
            }
            while (!blnEndOfMsg); //Loop Until blnEndOfMsg

            stlMsgs.Add(QUOTECHAR + strMessage + QUOTECHAR);
            blnMsgExists[intCurMsg] = true;
          }
          else {
            //add nothing (so numbers work out)
            stlMsgs.Add("");
            blnMsgExists[intCurMsg] = false;
          }
        } //Next intCurMsg
      }
      return true;

      //ErrHandler:
      //  //save error message
      //  strError = "Unhandled error while decoding messages (" + Err.Number + ": " + Err.Description + ") at position " + lngPos;
      //  return false;
    }
    static bool DecodeIf(byte[] bytData, List<string> stlOut)
    {
      bool blnInOrBlock;
      bool blnInNotBlock;
      bool blnFirstCmd;
      int intArg1Val;
      byte bytArg2Val;
      byte bytCurByte;
      bool blnIfFinished;
      byte bytNumSaidArgs;
      int lngWordGroupNum;
      string strLine, strArg;
      byte bytCmd;
      string[] strWarningLine;
      int i;
      int intCharCount;

      blnIfFinished = false;
      blnFirstCmd = true;
      blnInOrBlock = false;
      strLine = MultStr("  ", bytBlockDepth) + D_TKN_IF; //new String(" ", bytBlockDepth)? why not this?
      //main loop - read in logic, one byte at a time, and write text accordingly
      do {
        //always reset 'NOT' block status to false
        blnInNotBlock = false;

        //read next byte from input stream
        bytCurByte = bytData[lngPos];
        //and increment pointer
        lngPos++;

        //first, check for an //OR//
        if (bytCurByte == 0xFC) {
          blnInOrBlock = !blnInOrBlock;
          if (blnInOrBlock) {
            if (!blnFirstCmd) {
              strLine += D_TKN_AND;
              stlOut.Add(strLine);
              strLine = MultStr("  ", bytBlockDepth) + "    ";
              blnFirstCmd = true;
            }
            strLine += "(";
          }
          else {
            strLine += ")";
          }

          //now get next byte, and continue checking
          bytCurByte = bytData[lngPos];
          lngPos++;
        }

        //special check needed in case two 0xFCs are in a row, e.g. (a || b) && (c || d)
        if ((bytCurByte == 0xFC) && (!blnInOrBlock)) {
          strLine += D_TKN_AND;
          stlOut.Add(strLine);
          strLine = MultStr("  ", bytBlockDepth) + "    ";
          blnFirstCmd = true;
          strLine += "(";
          blnInOrBlock = true;
          bytCurByte = bytData[lngPos];
          lngPos++;
        }

        //check for //not// command
        if (bytCurByte == 0xFD) {   // NOT
          blnInNotBlock = true;
          bytCurByte = bytData[lngPos];
          lngPos++;
        }

        //check for valid test command
        if ((bytCurByte > 0) && (bytCurByte <= AGITestCommands.Count)) {
          if (!blnFirstCmd) {
            if (blnInOrBlock) {
              strLine += D_TKN_OR;
            }
            else {
              strLine += D_TKN_AND;
            }
            stlOut.Add(strLine);
            strLine = MultStr("  ", bytBlockDepth) + "    ";
          }
          bytCmd = bytCurByte;
          if (agMainLogSettings.SpecialSyntax && (bytCmd >= 1 && bytCmd <= 6)) {
            //get first argument
            bytCurByte = bytData[lngPos];
            lngPos++;
            bytArg2Val = bytData[lngPos];
            lngPos++;
            strLine += AddSpecialIf(bytCmd, bytCurByte, bytArg2Val, blnInNotBlock);
          }
          else {
            if (blnInNotBlock) {
              strLine += D_TKN_NOT;
            }

            strLine = strLine + agTestCmds[bytCmd].Name + "(";

            intArgStart = strLine.Length;
            if (bytCmd == 14) {
              // said command
              bytNumSaidArgs = bytData[lngPos];
              lngPos++;
              for (intArg1Val = 1; intArg1Val <= bytNumSaidArgs; intArg1Val++) {
                lngWordGroupNum = 256 * bytData[lngPos + 1] + bytData[lngPos];
                lngPos += 2;
                //if a game is loaded,
                if (agGameLoaded) {
                  //enable error trapping to catch any nonexistent words
                  try {
                    //if word exists,
                    strLine = strLine + QUOTECHAR + agVocabWords.GroupN(lngWordGroupNum).GroupName + QUOTECHAR;
                  }
                  catch (Exception) {
                    switch (agMainLogSettings.ErrorLevel) {
                    case leHigh:
                      //raise error
                      strError = "unknown word group (" + lngWordGroupNum + ") at position " + lngPos;
                      return false;
                    case leMedium:
                      //add the word by its number
                      strLine += lngWordGroupNum;
                      //set warning text
                      AddDecodeWarning("unknown word: " + lngWordGroupNum);
                      break;
                    case leLow:
                      //add the word by its number
                      strLine += lngWordGroupNum;
                      break;
                    }
                  }
                }
                else {
                  //alwys use word number as the argument
                  strLine += lngWordGroupNum;
                }

                if (intArg1Val < bytNumSaidArgs) {
                  strLine += ", ";
                }
              } //Next intArg1Val

            }
            else {
              //if at least one arg
              if (agTestCmds[bytCmd].ArgType.Length > 0) {
                for (intArg1Val = 0; intArg1Val < agTestCmds[bytCmd].ArgType.Length; intArg1Val++) {
                  bytCurByte = bytData[lngPos];
                  lngPos++;
                  //get arg Value
                  strArg = ArgValue(bytCurByte, agTestCmds[bytCmd].ArgType[intArg1Val]);
                  //if message error (no string returned)
                  if (strArg.Length == 0) {
                    //error string set by ArgValue function
                    return false;
                  }

                  //if message
                  if (agTestCmds[bytCmd].ArgType[intArg1Val] == ArgTypeEnum.atMsg) {
                    //split over additional lines, if necessary
                    do {
                      //if this message is too long to add to current line,
                      if (strLine.Length + strArg.Length > MAX_LINE_LEN) {
                        //determine number of characters availableto add to this line
                        intCharCount = MAX_LINE_LEN - strLine.Length;
                        //determine longest available section of message that can be added
                        //without exceeding max line length
                        do {
                          intCharCount -= 1;
                        }
                        while (intCharCount != 1 && strArg[intCharCount - 1] != ' '); //Loop Until (intCharCount = 1) || (Mid(strArg, intCharCount, 1) = " ")
                                                                                      //if no space is found to split up the line
                        if (intCharCount <= 1) {
                          //just split it without worrying about a space
                          intCharCount = MAX_LINE_LEN - strLine.Length;
                        }
                        //add the section of the message that fits on this line
                        strLine = strLine + Left(strArg, intCharCount) + QUOTECHAR;
                        strArg = Mid(strArg, intCharCount + 1, strArg.Length - intCharCount);
                        //add line
                        stlOut.Add(strLine);
                        //create indent (but don't exceed 20 spaces (to ensure msgs aren//t split
                        //up into small chunks)
                        if (intArgStart >= MAX_LINE_LEN - 20) {
                          intArgStart = MAX_LINE_LEN - 20;
                        }
                        strLine = MultStr(" ", intArgStart) + QUOTECHAR;
                      }
                      else {
                        //not too long; add the message to current line
                        strLine += strArg;
                        strArg = "";
                      }
                      //continue adding new lines until entire message is split and added
                    }
                    while (strArg != ""); //Loop Until strArg = ""
                  }
                  else {
                    //just add it
                    strLine += strArg;
                  }

                  //if more arguments needed,
                  if (intArg1Val < agTestCmds[bytCmd].ArgType.Length - 1) {
                    strLine += ", ";
                  }
                } //Next intArg1Val
              }
            }
            strLine += ")";
          }
          blnFirstCmd = false;
          //add warning if this is the unknown test19 command
          if (bytCmd == 19) {
            //set warning text
            AddDecodeWarning("unknowntest19 is only valid in Amiga AGI versions");
          }
        }
        else if (bytCurByte == 0xFF) {
          //done with if block; add //then//
          strLine += D_TKN_THEN.Replace(ARG1, NEWLINE + MultStr(" ", bytBlockDepth * 2 + 2));
          //(SkipToEndIf verified that max block depth is not exceeded)
          //increase block depth counter
          bytBlockDepth++;
          Block[bytBlockDepth].IsIf = true;
          Block[bytBlockDepth].Length = 256 * bytData[lngPos + 1] + bytData[lngPos];
          lngPos += 2;
          //check for length of zero
          if (Block[bytBlockDepth].Length == 0 && agMainLogSettings.ErrorLevel == leMedium) {
            //set warning text
            AddDecodeWarning("this block contains no commands");
          }

          //validate end pos
          Block[bytBlockDepth].EndPos = Block[bytBlockDepth].Length + lngPos;
          if (Block[bytBlockDepth].EndPos >= bytData.Length - 1) {
            switch (agMainLogSettings.ErrorLevel) {
            //if error level is high, SkipToEndIf catches this condition
            case leMedium:
              //adjust to end
              Block[bytBlockDepth].EndPos = bytData.Length - 2;
              //set warning text
              AddDecodeWarning("block end past end of resource; adjusted to end of resource");
              break;
            case leLow:
              //adjust to end
              Block[bytBlockDepth].EndPos = bytData.Length - 2;
              break;
            }
          }
          //verify block ends before end of previous block
          //(i.e. it's properly nested)
          if (Block[bytBlockDepth].EndPos > Block[bytBlockDepth - 1].EndPos) {
            //block is outside the previous block nest;
            //this is an abnormal situation
            //if error level is high; this would have been
            //caught in SkipToEndIf;
            if (agMainLogSettings.ErrorLevel == leMedium) {
              //set warning text
              AddDecodeWarning("Block end outside of nested block (" + Block[bytBlockDepth].JumpPos + ") at position " + lngPos);
            }

            //need to simulate this block by using else and goto
            Block[bytBlockDepth].IsOutside = true;
            Block[bytBlockDepth].JumpPos = Block[bytBlockDepth].EndPos;
            Block[bytBlockDepth].EndPos = Block[bytBlockDepth - 1].EndPos;
          }
          stlOut.Add(strLine);
          //if any warnings
          if (blnWarning) {
            //add warning lines
            strWarningLine = strWarning.Split("|");
            for (i = 0; i < strWarningLine.Length; i++) {
              stlOut.Add(MultStr("  ", bytBlockDepth) + D_TKN_COMMENT + "WARNING: " + strWarningLine[i]);
            } //Next i
              //reset warning flag + string
            blnWarning = false;
            strWarning = "";
          }

          strLine = MultStr("  ", bytBlockDepth);
          blnIfFinished = true;
        }
        else {
          //unknown test command
          strError = "Unknown test command (" + bytCurByte + ") at position " + lngPos;
          return false;
        }
      }
      while (!blnIfFinished); //Loop Until blnIfFinished
      return true;

      //ErrHandler:
      //  //unknown test command
      //  strError = "Unhandled error (" + Err.Number + ": " + Err.Description + ")" + NEWLINE + "at position " + lngPos + " in DecodeIf"
      //  return false;
    }
    static bool SkipToEndIf(byte[] bytData)
    {
      //used by the find label method
      //it moves the cursor to the end of the current if
      //statement
      byte CurByte;
      bool IfFinished = false;
      byte NumSaidArgs;
      byte ThisCommand;
      int i;

      do {
        CurByte = bytData[lngPos];
        lngPos++;
        if (CurByte == 0xFC) {
          CurByte = bytData[lngPos];
          lngPos++;
        }
        if (CurByte == 0xFC) {
          CurByte = bytData[lngPos];
          lngPos++; // we may have 2 0xFCs in a row, e.g. (a || b) && (c || d)
        }
        if (CurByte == 0xFD) {
          CurByte = bytData[lngPos];
          lngPos++;
        }

        if ((CurByte > 0) && (CurByte <= AGITestCommands.Count)) {
          ThisCommand = CurByte;
          if (ThisCommand == 14) { // said command
            //read in number of arguments
            NumSaidArgs = bytData[lngPos];
            lngPos++;
            //move pointer to next position past these arguments
            //(note that words use two bytes per argument, not one)
            lngPos += NumSaidArgs * 2;
          }
          else {
            //move pointer to next position past the arguments for this command
            lngPos += agTestCmds[ThisCommand].ArgType.Length;
          }
        }
        else if (CurByte == 0xFF) {
          if (bytBlockDepth >= MAX_BLOCK_DEPTH - 1) {
            strError = "Too many nested blocks (" + (bytBlockDepth + 1) + ") at position " + lngPos;
            return false;
          }
          //increment block counter
          bytBlockDepth++;
          Block[bytBlockDepth].IsIf = true;
          Block[bytBlockDepth].Length = 256 * bytData[lngPos + 1] + bytData[lngPos];
          lngPos += 2;
          //check length of block
          if (Block[bytBlockDepth].Length == 0) {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              //consider zero block lengths as error
              strError = "Encountered command block of length 0 at position " + lngPos;
              return false;
            }
          }
          Block[bytBlockDepth].EndPos = Block[bytBlockDepth].Length + lngPos;
          if (Block[bytBlockDepth].EndPos > Block[bytBlockDepth - 1].EndPos) {
            //block is outside the previous block nest;
            //
            //this is an abnormal situation;
            switch (agMainLogSettings.ErrorLevel) {
            case leHigh:
              //error
              strError = "Block end outside of nested block (" + Block[bytBlockDepth].JumpPos + ") at position" + lngPos;
              return false;
            case leMedium:
            case leLow:
              //need to simulate this block by using else and goto
              Block[bytBlockDepth].IsOutside = true;
              Block[bytBlockDepth].JumpPos = Block[bytBlockDepth].EndPos;
              Block[bytBlockDepth].EndPos = Block[bytBlockDepth - 1].EndPos;
              //add a new goto item
              //(since error level is medium or low
              //dont need to worry about an invalid jumppos)
              //if label is already created
              for (i = 1; i <= bytLabelCount; i++) {
                if (lngLabelPos[i] == Block[bytBlockDepth].JumpPos) {
                  break;
                }
              }
              //if loop exited normally (i will equal bytLabelCount+1)
              if (i == bytLabelCount + 1) {
                //increment label Count
                bytLabelCount = (byte)i;
                Array.Resize(ref lngLabelPos, bytLabelCount + 1);
                //save this label position
                lngLabelPos[bytLabelCount] = Block[bytBlockDepth].JumpPos;
              }
              break;
            }
          }
          IfFinished = true;
        }
        else {
          strError = "Unknown test command (" + CurByte + ") at position " + lngPos;
          return false;
        }
      }
      while (!IfFinished); //Loop Until IfFinished
      return true;

      //ErrHandler:
      //  //if no error string
      //  if (strError.Length == 0) 
      //  {
      //    strError = "Unhandled error while decoding logic at position " + lngPos + "): " + strError;
      //  }
      //  return false;
    }
    static bool FindLabels(byte[] bytData)
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

      // this function also validates all command bytes
      // by making sure none of them exceed max value of 181
      bytBlockDepth = 0;
      bytLabelCount = 0;
      lngPos = 2;
      do {
        //check to see if the end of a block has been found
        //start at most recent block and work up to oldest block
        for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--) {
          //if this position matches the end of this block
          if (Block[CurBlock].EndPos <= lngPos) {
            //verify it is exact
            if (Block[CurBlock].EndPos != lngPos) {
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
          if (!SkipToEndIf(bytData)) {
            //major error
            return false;
          }
        }
        else if (bytCurData == 0xFE)   //if the byte is a GOTO command
        {
          //reset goto status flag
          DoGoto = false;
          tmpBlockLength = 256 * bytData[lngPos + 1] + bytData[lngPos];
          lngPos += 2;
          //need to check for negative Value here
          if (tmpBlockLength > 0x7FFF) {
            //convert block length to negative value
            tmpBlockLength -= 0x10000;
          }
          //check to see if this 'goto' might be an 'else':
          //  - end of this block matches this position (the if-then part is done)
          //  - this block is identified as an IF block
          //  - this is NOT the main block
          //  - the flag to set elses as gotos is turned off
          if ((Block[bytBlockDepth].EndPos == lngPos) && (Block[bytBlockDepth].IsIf) && (bytBlockDepth > 0) && (!agMainLogSettings.ElseAsGoto)) {
            //this block is now in the 'else' part, so reset flag
            Block[bytBlockDepth].IsIf = false;
            Block[bytBlockDepth].IsOutside = false;
            //does this 'else' statement line up to end at the same
            //point that the 'if' statement does?
            //the end of this block is past where the 'if' block ended OR
            //the block is negative (means jumping backward, so it MUST be a goto)
            //length of block doesn't have enough room for code necessary to close the 'else'
            if ((tmpBlockLength + lngPos > Block[bytBlockDepth - 1].EndPos) || (tmpBlockLength < 0) || (Block[bytBlockDepth].Length <= 3)) {
              //this is a //goto// statement,
              DoGoto = true;
            }
            else {
              //this is an 'else' statement;
              //readjust block end so the IF statement that owns this 'else'
              //is ended correctly
              Block[bytBlockDepth].Length = tmpBlockLength;
              Block[bytBlockDepth].EndPos = Block[bytBlockDepth].Length + lngPos;
            }
          }
          else {
            //this is a goto statement (or an else statement while mGotos flag is false)
            DoGoto = true;
          }
          // goto
          if (DoGoto) {
            LabelLoc = tmpBlockLength + lngPos;
            if (LabelLoc > bytData.Length - 2) {
              //if error level is high (medium and low are handled in DecodeLogic)
              if (agMainLogSettings.ErrorLevel == leHigh) {
                strError = "Goto destination past end of logic (" + LabelLoc + ")" + "at position " + lngPos;
                return false;
              }
            }
            //if label is already created
            for (i = 1; i <= bytLabelCount; i++) {
              if (lngLabelPos[i] == LabelLoc) {
                break;
              }
            }
            //if loop exited normally (i will equal bytLabelCount+1)
            if (i == bytLabelCount + 1) {
              //increment label Count
              bytLabelCount++;
              Array.Resize(ref lngLabelPos, bytLabelCount + 1);
              //save this label position
              lngLabelPos[bytLabelCount] = LabelLoc;
            }
          }
        }
        else if (bytCurData < MAX_CMDS) //byte is an AGI command
        {
          //skip over arguments to get next command
          lngPos += agCmds[bytCurData].ArgType.Length;
        }
        else {
          //not a valid command - eror depends on value
          if (bytCurData == 182) {
            strError = "Unsupported action command 182 (adj.ego.move.to.x.y) at position " + lngPos;
            return false;
          }
          else { //(bytCurData > MAX_CMDS)
            //major error
            strError = "Unknown action command (" + bytCurData + ") at position " + lngPos;
            return false;
          }
        }
      }
      while (lngPos < lngMsgSecStart); //Loop Until (lngPos >= lngMsgSecStart)
                                       //now sort labels, if found
      if (bytLabelCount > 1) {
        for (i = 1; i <= bytLabelCount - 1; i++) {
          for (j = i + 1; j <= bytLabelCount; j++) {
            if (lngLabelPos[j] < lngLabelPos[i]) {
              LabelLoc = lngLabelPos[i];
              lngLabelPos[i] = lngLabelPos[j];
              lngLabelPos[j] = LabelLoc;
            }
          }
        }
      }
      //clear block info (don't overwrite main block)
      for (i = 1; i < MAX_BLOCK_DEPTH; i++) {
        Block[i].EndPos = 0;
        Block[i].IsIf = false;
        Block[i].IsOutside = false;
        Block[i].JumpPos = 0;
        Block[i].Length = 0;
      }
      //return success
      return true;
    }
    static void DisplayMessages(List<string> stlOut)
    {
      int lngMsg;
      //need to adjust references to the Messages stringlist object by one
      //since the list is zero based, but messages are one-based.
      stlOut.Add(D_TKN_COMMENT + "Messages");
      for (lngMsg = 1; lngMsg <= stlMsgs.Count; lngMsg++) {
        Debug.Print($"Msg Num: {lngMsg}  MsgExists:{blnMsgExists[lngMsg].ToString()}  MsgUsed:{blnMsgUsed[lngMsg]}");
        if (blnMsgExists[lngMsg] && ((agMainLogSettings.ShowAllMessages) || !blnMsgUsed[lngMsg])) {
          stlOut.Add(D_TKN_MESSAGE.Replace(ARG1, lngMsg.ToString()).Replace(ARG2, stlMsgs[lngMsg - 1]));
        }
      }
    }
    static string AddSpecialCmd(byte[] bytData, byte bytCmd)
    {
      byte bytArg1, bytArg2;
      //get first argument
      bytArg1 = bytData[lngPos];
      lngPos++;
      switch (bytCmd) {
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
    static string AddSpecialIf(byte bytCmd, byte bytArg1, byte bytArg2, bool NOTOn)
    {
      string retval = ArgValue(bytArg1, ArgTypeEnum.atVar);
      switch (bytCmd) {
      case 1:
      case 2:            // equaln or equalv
                         //if NOT in effect,
        if (NOTOn) {
          //test for not equal
          retval += D_TKN_NOT_EQUAL;
        }
        else {
          //test for equal
          retval += D_TKN_EQUAL;
        }
        //if command is comparing variables,
        if (bytCmd == 2) {
          //variable
          retval += ArgValue(bytArg2, ArgTypeEnum.atVar, bytArg1);
        }
        else {
          //add number
          retval += ArgValue(bytArg2, ArgTypeEnum.atNum, bytArg1);
        }
        break;
      case 3:
      case 4:           // lessn, lessv
                        //if NOT is in effect,
        if (NOTOn) {
          //test for greater than or equal
          retval += " >= ";
        }
        else {
          //test for less than
          retval += " < ";
        }
        //if command is comparing variables,
        if (bytCmd == 4) {
          retval += ArgValue(bytArg2, ArgTypeEnum.atVar, bytArg1);
        }
        else {
          //number string
          retval += ArgValue(bytArg2, ArgTypeEnum.atNum, bytArg1);
        }

        break;
      case 5:
      case 6:            // greatern, greaterv
                         //if NOT is in effect,
        if (NOTOn) {
          //test for less than or equal
          retval += " <= ";
        }
        else {
          //test for greater than
          retval += " > ";
        }
        //if command is comparing variables,
        if (bytCmd == 6) {
          retval += ArgValue(bytArg2, ArgTypeEnum.atVar, bytArg1);
        }
        else {
          //number string
          retval += ArgValue(bytArg2, ArgTypeEnum.atNum, bytArg1);
        }
        break;
      }
      return retval;
    }
    static void AddBlockEnds(List<string> stlIn)
    {
      int CurBlock, i;

      for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--) {
        //why would a less than apply here?
        //FOUND IT!!! here is a case where it is less than!!!
        //if (Block(CurBlock).EndPos <= lngPos) {
        if (Block[CurBlock].EndPos == lngPos) {
          //check for unusual case where an if block ends outside
          //the if block it is nested in
          if (Block[CurBlock].IsOutside) {
            //add an else
            stlIn.Add(MultStr("  ", bytBlockDepth) + D_TKN_ENDIF);
            if (agMainLogSettings.ElseAsGoto) {
              stlIn.Add(MultStr("  ", bytBlockDepth - 1) + D_TKN_GOTO);
            }
            else {
              stlIn.Add(MultStr("  ", bytBlockDepth - 1) + D_TKN_ELSE.Replace(ARG1, NEWLINE + new String(' ', bytBlockDepth * 2)));
            }
            //add a goto
            for (i = 1; i <= bytLabelCount; i++) {
              if (lngLabelPos[i] == Block[CurBlock].JumpPos) {
                stlIn.Add(MultStr("  ", bytBlockDepth) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                break;
              }
            }
          }
          //add end if
          stlIn.Add(MultStr("  ", CurBlock) + D_TKN_ENDIF);
          bytBlockDepth--;
        }
      }
    }
  }
}
