using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.LogicErrorLevel;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.Commands;

using static WinAGI.Engine.Base;
using System.Diagnostics;

namespace WinAGI.Engine {
    public static partial class Compiler {
        internal struct BlockType {
            internal bool IsIf = false;
            internal int StartPos = 0;
            internal int EndPos = 0;
            internal int Length = 0;
            internal bool IsOutside = false;
            internal int JumpPos = 0;
            internal bool HasQuit = false;
            public BlockType() {
            }
        }
        public enum AGICodeStyle {
            cstDefault,
            cstStdVisualStudio,
            cstModifiedVisualStudio
        }
        static byte bytBlockDepth;
        static BlockType[] DecodeBlock = new BlockType[MAX_BLOCK_DEPTH];
        static int intArgStart;
        static int[] lngLabelPos = [];
        static int lngMsgSecStart;
        static List<string> stlMsgs;
        static bool[] blnMsgUsed = new bool[256];
        static bool[] blnMsgExists = new bool[256];
        static List<string> stlOutput = [];
        private static string strError;
        static bool badQuit = false;
        static byte mIndentSize = 4;

        const int MAX_LINE_LEN = 80;
        //tokens for building source code output
        public static bool blnTokensSet;
        internal static string INDENT;
        static string D_TKN_NOT;
        static string D_TKN_IF;
        static string D_TKN_THEN;
        static string D_TKN_ELSE;
        static string D_TKN_ENDIF;
        static string D_TKN_GOTO;
        static string D_TKN_EOL;
        static string D_TKN_AND;
        static string D_TKN_OR;
        static string D_TKN_EQUAL;
        static string D_TKN_NOT_EQUAL;
        static string D_TKN_COMMENT;
        static string D_TKN_MESSAGE;
        static bool blnWarning;
        static List<string> strWarning = [];


        public static byte IndentSize {
            get {
                if (mIndentSize == 0) {
                    mIndentSize = 4;
                }
                return mIndentSize;
            }
            set {
                mIndentSize = value;
            }
        }

        internal static int LogicNum;
        internal static string DecodeGameID {
            get;
            set;
        }

        internal static string DecodeLogic(Logic SourceLogic, int LogNum = -1) {
            // converts logic bytecode into decompiled source, converting extended
            // characters to correct encoding
            byte bytCurData;
            bool blnGoto;
            byte bytCmd = 0;
            int tmpBlockLen;
            int intArg;
            int lngNextLabel = 0;
            int lngLabelLoc;
            int i, j;
            string strArg;
            int intCharCount;

            // TODO: what about logics that aren't in a game?? they have no number
            Debug.Assert(SourceLogic.Loaded);
            bytLogComp = (byte)LogNum;
            byte[] bytData = SourceLogic.Data.AllData;
            stlOutput = [];
            strError = "";

            //if nothing in the resource,
            if (bytData.Length == 0) {
                //single 'return()' command
                return "return();";
            }
            // if tokens not set yet, set them now
            if (!blnTokensSet) {
                InitTokens(CodeStyle);
            }
            //clear block info
            for (i = 0; i < MAX_BLOCK_DEPTH; i++) {
                DecodeBlock[i].EndPos = 0;
                DecodeBlock[i].IsIf = false;
                DecodeBlock[i].IsOutside = false;
                DecodeBlock[i].JumpPos = 0;
                DecodeBlock[i].Length = 0;
                DecodeBlock[i].HasQuit = false;
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

            lngMsgSecStart = bytData[0] + (bytData[1] << 8) + 2;

            //if can't read messges,
            if (!ReadMessages(bytData, lngMsgSecStart, SourceLogic.V3Compressed != 2)) {
                // return error
                SourceLogic.ErrLevel = -9;
                SourceLogic.ErrData[0] = strError;
                SourceLogic.ErrData[1] = SourceLogic.ID;
                return "return();" + NEWLINE;
                //WinAGIException wex = new($"LogDecode Error ({strError})") {
                //    HResult = WINAGI_ERR + 688
                //};
                //wex.Data["error"] = strError;
                //throw wex;
            }

            //set main block info
            DecodeBlock[0].IsIf = false;
            DecodeBlock[0].EndPos = lngMsgSecStart;
            DecodeBlock[0].IsOutside = false;
            DecodeBlock[0].Length = lngMsgSecStart;

            //set error flag
            strError = "";
            //locate labels, and mark them (this also validates all command bytes to be <=181)
            if (!FindLabels(bytData)) {
                // check for 'quit' cmd error
                if (strError == "CHECKQUIT") {
                    // adjust version and try one more time
                    ActionCommands[134].ArgType = [];
                    badQuit = true;
                    strError = "";
                    if (!FindLabels(bytData)) {
                        //use error string set by findlabels
                        SourceLogic.ErrLevel = -10;
                        SourceLogic.ErrData[0] = strError;
                        SourceLogic.ErrData[1] = SourceLogic.ID;
                        return "return();" + NEWLINE;
                        //WinAGIException wex = new($"LogDecode Error ({strError})") {
                        //    HResult = WINAGI_ERR + 688
                        //};
                        //wex.Data["error"] = strError;
                        //throw wex;
                    }
                    else {
                        SourceLogic.ErrLevel = -10;
                        SourceLogic.ErrData[0] = strError;
                        SourceLogic.ErrData[1] = SourceLogic.ID;
                        return "return();" + NEWLINE;
                        ////use error string set by findlabels
                        //WinAGIException wex = new($"LogDecode Error ({strError})") {
                        //    HResult = WINAGI_ERR + 688
                        //};
                        //wex.Data["error"] = strError;
                        //throw wex;
                    }
                }
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
                    if (agSierraSyntax) {
                        stlOutput.Add(":label" + lngNextLabel.ToString());
                    }
                    else {
                        stlOutput.Add("Label" + lngNextLabel + ":");
                    }
                    lngNextLabel++;
                    if (lngNextLabel > bytLabelCount) {
                        lngNextLabel = 0;
                    }
                }
                bytCurData = bytData[lngPos];
                lngPos++;
                switch (bytCurData) {
                case 0xFF:
                    //this byte starts an IF statement
                    if (!DecodeIf(bytData, stlOutput)) {
                        SourceLogic.ErrLevel = -11;
                        SourceLogic.ErrData[0] = strError;
                        SourceLogic.ErrData[1] = SourceLogic.ID;
                        return "return();" + NEWLINE;
                        //WinAGIException wex = new($"LogDecode Error ({strError})") {
                        //    HResult = WINAGI_ERR + 688
                        //};
                        //wex.Data["error"] = strError;
                        //throw wex;
                    }
                    break;
                case 0xFE:
                    //this byte is a 'goto' or 'else'
                    blnGoto = false;
                    tmpBlockLen = 256 * bytData[lngPos + 1] + bytData[lngPos];
                    lngPos += 2;
                    //need to check for negative Value here
                    if (tmpBlockLen > 0x7FFF) {
                        // convert to negative number
                        tmpBlockLen -= 0x10000;
                    }
                    if ((DecodeBlock[bytBlockDepth].EndPos == lngPos) && (DecodeBlock[bytBlockDepth].IsIf) && (bytBlockDepth > 0) && (!ElseAsGoto)) {
                        DecodeBlock[bytBlockDepth].IsIf = false;
                        DecodeBlock[bytBlockDepth].IsOutside = false;
                        if ((tmpBlockLen + lngPos > DecodeBlock[bytBlockDepth - 1].EndPos) || (tmpBlockLen < 0) || (DecodeBlock[bytBlockDepth].Length <= 3)) {
                            // else won't work; force it to be a goto
                            blnGoto = true;
                        }
                        else {
                            stlOutput.Add(MultStr(INDENT, bytBlockDepth - 1) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                            // append else to end of curent if block
                            stlOutput[^1] += D_TKN_ELSE.Replace(ARG1, NEWLINE + MultStr(INDENT, bytBlockDepth - 1));
                            // adjust length and endpos for the 'else' block
                            DecodeBlock[bytBlockDepth].Length = tmpBlockLen;
                            DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth].Length + lngPos;
                        }
                    }
                    else {
                        blnGoto = true;
                    }
                    // goto
                    if (blnGoto) {
                        lngLabelLoc = tmpBlockLen + lngPos;
                        //label already verified in FindLabels; add warning if necessary
                        if (lngLabelLoc > lngMsgSecStart - 1) {
                            //set warning
                            AddDecodeWarning("DC13", "Goto destination at position " + lngPos + "past end of logic; adjusted to end of logic", stlOutput.Count);
                            //adjust it to end of resource
                            lngLabelLoc = lngMsgSecStart - 1;
                        }
                        // find it in list of labels
                        for (i = 1; i <= bytLabelCount; i++) {
                            if (lngLabelPos[i] == lngLabelLoc) {
                                stlOutput.Add(MultStr(INDENT, bytBlockDepth) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                                //if any warnings
                                if (blnWarning) {
                                    //add warning lines
                                    for (j = 0; j < strWarning.Count; j++) {
                                        stlOutput.Add(D_TKN_COMMENT + " WARNING: " + strWarning[j]);
                                    }
                                    //reset warning flag + string
                                    blnWarning = false;
                                    strWarning = [];
                                }
                                break;
                            }
                        }
                    }
                    break;
                default: //case < MAX_CMDS:
                    //valid agi command (don't need to check for invalid command number;
                    // they are all validated in FindLabels)
                    //if this command is not within range of expected commands for targeted interpreter version,
                    if (bytCurData > ActionCount - 1) {
                        //this byte is a command - show warning
                        AddDecodeWarning("DC10", "This command at position " + lngPos.ToString() + " is not valid for selected interpreter version (" + compGame.agIntVersion + ")", stlOutput.Count);
                    }
                    bytCmd = bytCurData;
                    string strCurrentLine = MultStr(INDENT, bytBlockDepth);
                    if (SpecialSyntax && (bytCmd >= 0x1 && bytCmd <= 0xB) || (bytCmd >= 0xA5 && bytCmd <= 0xA8)) {
                        strCurrentLine += AddSpecialCmd(bytData, bytCmd);
                    }
                    else {
                        strCurrentLine += ActionCommands[bytCmd].Name + "(";
                        intArgStart = strCurrentLine.Length;
                        for (intArg = 0; intArg < ActionCommands[bytCmd].ArgType.Length; intArg++) {
                            bytCurData = bytData[lngPos];
                            lngPos++;
                            strArg = ArgValue(bytCurData, ActionCommands[bytCmd].ArgType[intArg]);
                            // if showing reserved names && using reserved defines
                            if (ReservedAsText && UseReservedNames) {
                                //some commands use resources as arguments; substitute as appropriate
                                switch (bytCmd) {
                                case 122:
                                    //add.to.pic,    1st arg (V)
                                    if (intArg == 0) {
                                        if (compGame.agViews.Exists(bytCurData)) {
                                            strArg = compGame.agViews[bytCurData].ID;
                                        }
                                        else {
                                            //view doesn't exist
                                            AddDecodeWarning("DC11", "View " + bytCurData.ToString() + " in add.to.pic() at position " + lngPos.ToString() + " does not exist", stlOutput.Count);
                                        }
                                    }
                                    break;
                                case 22:
                                    //call,          only arg (L)
                                    if (compGame.agLogs.Exists(bytCurData)) {
                                        strArg = compGame.agLogs[bytCurData].ID;
                                    }
                                    else {
                                        //logic doesn't exist
                                        AddDecodeWarning("DC11", "Logic " + bytCurData.ToString() + " in call() at position " + lngPos.ToString() + " does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 175:
                                    // discard.sound, only arg (S)
                                    if (compGame.agSnds.Exists(bytCurData)) {
                                        strArg = compGame.agSnds[bytCurData].ID;
                                    }
                                    else {
                                        // sound doesn't exist
                                        AddDecodeWarning("DC11", "Sound " + bytCurData + " in discard.sound() does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 32: 
                                    //discard.view,  only arg (V)
                                    if (compGame.agViews.Exists(bytCurData)) {
                                        strArg = compGame.agViews[bytCurData].ID;
                                    }
                                    else {
                                        // view doesn't exist
                                        AddDecodeWarning("DC11", "View " + bytCurData + " in discard.view() does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 20: 
                                    //load.logics,   only arg (L)
                                    if (compGame.agLogs.Exists(bytCurData)) {
                                        strArg = compGame.agLogs[bytCurData].ID;
                                    }
                                    else {
                                        // logic doesn't exist
                                        AddDecodeWarning("DC11", "Logic " + bytCurData + " in loadlogics() does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 98:  
                                    // load.sound,    only arg (S)
                                    if (compGame.agSnds.Exists(bytCurData)) {
                                        strArg = compGame.agSnds[bytCurData].ID;
                                    }
                                    else {
                                        //sound doesn't exist
                                        AddDecodeWarning("DC11", "Sound " + bytCurData + " in load.sound() does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 30:  
                                    // load.view,     only arg (V)
                                    if (compGame.agViews.Exists(bytCurData)) {
                                        strArg = compGame.agViews[bytCurData].ID;
                                    }
                                    else {
                                        // view doesn't exist
                                        AddDecodeWarning("DC11", "View " + bytCurData + " in load.view() does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 18:  
                                    // new.room,      only arg (L)
                                    if (compGame.agLogs.Exists(bytCurData)) {
                                        strArg = compGame.agLogs[bytCurData].ID;
                                    }
                                    else {
                                        // logic doesn't exist
                                        AddDecodeWarning("DC11", "Logic " + bytCurData + " in new.room() does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 41: 
                                    // set.view,      2nd arg (V)
                                    if (intArg == 1) {
                                        if (compGame.agViews.Exists(bytCurData)) {
                                            strArg = compGame.agViews[bytCurData].ID;
                                        }
                                        else {
                                            // view doesn't exist
                                            AddDecodeWarning("DC11", "View " + bytCurData + " in set.view() does not exist", stlOutput.Count);
                                        }
                                    }
                                    break;
                                case 129: 
                                    // show.obj,      only arg (V)
                                    if (compGame.agViews.Exists(bytCurData)) {
                                        strArg = compGame.agViews[bytCurData].ID;
                                    }
                                    else {
                                        // view doesn't exist
                                        AddDecodeWarning("DC11", "View " + bytCurData + " in show.obj() does not exist", stlOutput.Count);
                                    }
                                    break;
                                case 99: 
                                    // sound,         1st arg (S)
                                    if (intArg == 0) {
                                        if (compGame.agSnds.Exists(bytCurData)) {
                                            strArg = compGame.agSnds[bytCurData].ID;
                                        }
                                        else {
                                            // sound doesn't exist
                                            AddDecodeWarning("DC11", "Sound " + bytCurData + " in sound() does not exist", stlOutput.Count);
                                        }
                                    }
                                    break;
                                case 150:
                                    // trace.info,    1st arg (L)
                                    if (intArg == 0) {
                                        if (compGame.agLogs.Exists(bytCurData)) {
                                            strArg = compGame.agLogs[bytCurData].ID;
                                        }
                                        else {
                                            // logic doesn't exist
                                            AddDecodeWarning("DC11", "Logic " + bytCurData + " in trace.info() does not exist", stlOutput.Count);
                                        }
                                    }
                                    break;
                                }
                            }
                            // if message error (no string returned)
                            if (strArg.Length == 0) {
                                // error string set by ArgValue function
                                SourceLogic.ErrLevel = -12;
                                SourceLogic.ErrData[0] = strError;
                                SourceLogic.ErrData[1] = SourceLogic.ID;
                                return "return();" + NEWLINE;
                                //WinAGIException wex = new($"LogDecode Error: {strError}") {
                                //    HResult = WINAGI_ERR + 688
                                //};
                                //wex.Data["error"] = strError;
                                //throw wex;
                            }
                            // check for commands that use colors here
                            switch (bytCmd) {
                            case 105:
                                // clear.lines, 3rd arg
                                if (intArg == 2) {
                                    if (Val(strArg) < 16) {
                                        strArg = agResColor[(int)Val(strArg)].Name;
                                    }
                                }
                                break;
                            case 154: 
                                // clear.text.rect, 5th arg
                                if (intArg == 4) {
                                    if (Val(strArg) < 16) {
                                        strArg = agResColor[(int)Val(strArg)].Name;
                                    }
                                }
                                break;
                            case 109: 
                                // set.text.attribute, all args
                                if (Val(strArg) < 16) {
                                    strArg = agResColor[(int)Val(strArg)].Name;
                                }
                                break;
                            }

                            //if message
                            if (ActionCommands[bytCmd].ArgType[intArg] == atMsg) {
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
                                while (strArg != "");
                            }
                            else {
                                //add arg
                                strCurrentLine += strArg;
                            }

                            //if more arguments needed,
                            if (intArg < ActionCommands[bytCmd].ArgType.Length - 1) {
                                strCurrentLine += ", ";
                            }
                        }
                        strCurrentLine += ")";
                    }
                    // check for set.game.id
                    if (bytCmd == 143) {
                        // if importing, use ths as suggested wag file name/gameid
                        DecodeGameID = stlMsgs[bytCurData][1..^1];
                    }
                    // check for quit() arg count error
                    if (badQuit) {
                        AddDecodeWarning("DC12", "quit() comand at position " + lngPos.ToString() + "coded with no argument", stlOutput.Count);
                    }
                    strCurrentLine += D_TKN_EOL;
                    stlOutput.Add(strCurrentLine);
                    //if any warnings
                    if (blnWarning) {
                        //add warning lines
                        for (i = 0; i < strWarning.Count; i++) {
                            stlOutput.Add(D_TKN_COMMENT + " WARNING: " + strWarning[i]);
                        }
                        //reset warning flag + string
                        blnWarning = false;
                        strWarning = [];
                    }
                    break;
                }
            }
            while (lngPos < lngMsgSecStart);
            // finish up
            AddBlockEnds(stlOutput);
            // confirm logic ends with return
            if (bytCmd != 0) {
                stlOutput.Add(MultStr(INDENT, bytBlockDepth) + CMT1_TOKEN + "WARNING DC15: return() command missing from end of logic");
            }
            stlOutput.Add("");
            DisplayMessages(stlOutput);
            // if quit was modified,restore it
            if (badQuit) {
                ActionCommands[134].ArgType = new ArgTypeEnum[1];
                ActionCommands[134].ArgType[0] = atNum;
            }
            // return results
            return string.Join(NEWLINE, [.. stlOutput]);
        }

        static void AddDecodeWarning(string WarnID, string WarningText, int LineNum) {
            TWinAGIEventInfo dcWarnInfo = new() {
                ResNum = bytLogComp,
                ResType = AGIResType.rtLogic,
                Type = EventType.etWarning,
                ID = WarnID,
                Module = "",
                Text = WarningText,
                Line = LineNum.ToString(),
            };
            Raise_DecodeLogicEvent(dcWarnInfo);

            // add warning text to the stack so it can be added
            //to output once the current line is decompiled

            //if at least one warning already,
            if (!blnWarning) {
                //set warning flag
                blnWarning = true;
            }
            strWarning.Add(WarningText);
        }
        static string ArgValue(byte ArgNum, ArgTypeEnum ArgType, int VarVal = -1) {
            //if not showing reserved names (or if not using reserved defines)
            // AND not a msg (always substitute msgs)
            if ((!ReservedAsText || !UseReservedNames) && ArgType != ArgTypeEnum.atMsg) {
                //return simple Value
                return agArgTypPref[(int)ArgType] + ArgNum;
            }
            //add appropriate resdef name
            switch (ArgType) {
            case atNum:
                switch (VarVal) {
                case 2 or 5: //v2 and v5 use edge codes
                    if (ArgNum <= 4) {
                        return agEdgeCodes[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 6: //v6 uses direction codes
                    if (ArgNum <= 8) {
                        return agEgoDir[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 20: //v20 uses computer type codes
                    if (ArgNum <= 8) {
                        return agCompType[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 26: //v26 uses video mode codes
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
            case atVar:
                //if a predefined,
                if (ArgNum <= 26) {
                    return agResVar[ArgNum].Name;
                }
                else {
                    //not a reserved data type
                    return "v" + ArgNum;
                }
            case atFlag:
                //if a predefined
                if (ArgNum <= 16) {
                    return agResFlag[ArgNum].Name;
                    //check for special case of f20 (only if version 3.002102 or higher)
                }
                else if (ArgNum == 20 && Val(compGame.agIntVersion) >= 3.002102) {
                    return agResFlag[17].Name;
                }
                else {
                    //not a reserved data type
                    return "f" + ArgNum;
                }
            case atMsg:
                blnMsgUsed[ArgNum] = true;
                //if this message exists,
                if (blnMsgExists[ArgNum]) {
                    if (MsgsByNumber) {
                        return "m" + ArgNum;
                    }
                    else {
                        //use string value of  message as the chunk to add to current line
                        return stlMsgs[ArgNum];
                    }
                }
                else {
                    //message doesn't exist; raise a warning
                    AddDecodeWarning("DC01", "Invalid message: " + ArgNum + " at position " + lngPos, stlOutput.Count);
                    //store as number
                    return "m" + ArgNum;
                }
            case atSObj:
                //if ego
                if (ArgNum == 0) {
                    return agResObj[0].Name;
                }
                else {
                    //not a reserved data type
                    return "o" + ArgNum;
                }
            case atInvItem:
                //if a game is loaded AND OBJECT file is loaded and not displaying objects by number,
                if (compGame.agInvObj.Loaded && !IObjsByNumber) {
                    if (ArgNum < compGame.agInvObj.Count) {
                        //if object is unique
                        if (compGame.agInvObj[ArgNum].Unique) {
                            //double check if item is a question mark
                            if (compGame.agInvObj[ArgNum].ItemName == "?") {
                                //use the inventory item number, and post a warning
                                AddDecodeWarning("DC04", "Reference to null inventory item ('?') at position " + lngPos, stlOutput.Count);
                                return "i" + ArgNum;
                            }
                            else {
                                //a unique, non-questionmark item- use it's string Value
                                return QUOTECHAR + compGame.agInvObj[ArgNum].ItemName.Replace(QUOTECHAR.ToString(), "\\\"") + QUOTECHAR;
                            }
                        }
                        else {
                            //use obj number instead
                            switch (ErrorLevel) {
                            case leHigh or leMedium:
                                AddDecodeWarning("DC05", "Non-unique inventory item '" + compGame.agInvObj[ArgNum].ItemName + "' at position " + lngPos, stlOutput.Count);
                                break;
                            }
                            return "i" + ArgNum;
                        }
                    }
                    else {
                        //set warning
                        AddDecodeWarning("DC03", "Invalid inventory item (" + ArgNum + ") at position " + lngPos, stlOutput.Count);
                        //just use the number
                        return "i" + ArgNum;
                    }
                }
                else {
                    //always refer to the object by number if no object file loaded
                    return "i" + ArgNum;
                }
            case ArgTypeEnum.atStr:
                if (ArgNum == 0) {
                    return agResStr[0].Name;
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
        static bool ReadMessages(byte[] bytData, int lngMsgStart, bool Decrypt) {
            int lngMsgTextEnd;
            int[] MessageStart = new int[256];
            int intCurMsg;
            int lngMsgTextStart;
            List<byte> bMsgText;
            bool blnEndOfMsg;
            byte bytInput;
            int NumMessages;

            // NOTE: There is no message 0 (it is not supported by the file format).
            // the word which corresponds to message 0 offset is used to hold the
            // end of text ptr so AGI can decrypt the message text when the logic
            // is initially loaded

            lngPos = lngMsgStart;
            lngMsgTextEnd = lngMsgStart;
            stlMsgs = [];
            // first msg, with index of zero, is null/not used
            stlMsgs.Add("");
            //read in number of messages
            NumMessages = bytData[lngPos];
            lngPos++;
            if (NumMessages > 0) {
                // TODO: add more checks for bad msg data, and add new warnings as necessary

                // retrieve and adjust end of message section
                lngMsgTextEnd = lngMsgTextEnd + 256 * bytData[lngPos + 1] + bytData[lngPos];
                lngPos += 2;
                if (lngMsgTextEnd != bytData.Length - 1) {
                    AddDecodeWarning("DC16", "Message section has invalid end-of-text marker", stlOutput.Count);
                    // adjust it to end
                    lngMsgTextEnd = bytData.Length - 1;
                }
                //loop through all messages, extract offset
                for (intCurMsg = 1; intCurMsg <= NumMessages; intCurMsg++) {
                    //set start of this msg as start of msg block, plus offset, plus one (for byte which gives number of msgs)
                    MessageStart[intCurMsg] = 256 * bytData[lngPos + 1] + bytData[lngPos] + lngMsgStart + 1;
                    // validate msg start
                    if (MessageStart[intCurMsg] >= bytData.Length) {
                        AddDecodeWarning("DC17", "Message " + intCurMsg + " has invalid offset", stlOutput.Count);
                        MessageStart[intCurMsg] = 0;
                    }
                    lngPos += 2;

                }
                // decrypt the entire message section, if needed
                lngMsgTextStart = lngPos;
                if (Decrypt) {
                    for (int i = lngPos; i < lngMsgTextEnd; i++) {
                        bytData[i] ^= bytEncryptKey[(i - lngMsgTextStart) % 11];
                    }
                }

                //now read all messages
                for (intCurMsg = 1; intCurMsg <= NumMessages; intCurMsg++) {
                    bMsgText = [];
                    //if msg start points to a valid msg
                    if (MessageStart[intCurMsg] > 0 && MessageStart[intCurMsg] >= lngMsgTextStart) {
                        lngPos = MessageStart[intCurMsg];
                        blnEndOfMsg = false;
                        do {
                            bytInput = bytData[lngPos];
                            lngPos++;
                            if ((bytInput == 0) || (lngPos >= bytData.Length)) {
                                blnEndOfMsg = true;
                            }
                            else {
                                switch (bytInput) {
                                case 0xA:
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(110); // 'n'
                                    break;
                                case < 0x20:
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add((byte)bytInput.ToString("x2")[0]);
                                    bMsgText.Add((byte)bytInput.ToString("x2")[1]);
                                    break;
                                case 0x22:
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(34); // '"'
                                    break;
                                case 0x5C:
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(92); // '\'
                                    break;
                                case 0x7F:
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(55); // '7'
                                    bMsgText.Add(70); // 'F'
                                    break;
                                case 0xFF:
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(70); // 'F'
                                    bMsgText.Add(70); // 'F'
                                    break;
                                default:
                                    bMsgText.Add(bytInput);
                                    break;
                                }
                            }
                        }
                        while (!blnEndOfMsg);
                        if (bMsgText.Count == 0) {
                            stlMsgs.Add("\"\"");
                        }
                        else {
                            // convert to correct codepage
                            stlMsgs.Add(QUOTECHAR + compGame.CodePage.GetString(bMsgText.ToArray()) + QUOTECHAR);
                        }
                        blnMsgExists[intCurMsg] = true;
                    }
                    else {
                        // add nothing (so numbers work out)
                        stlMsgs.Add("");
                        blnMsgExists[intCurMsg] = false;
                    }
                }
            }
            return true;
        }

        static bool DecodeIf(byte[] bytData, List<string> stlOut) {
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
            int i;
            int intCharCount;

            blnIfFinished = false;
            blnFirstCmd = true;
            blnInOrBlock = false;
            strLine = MultStr(INDENT, bytBlockDepth) + D_TKN_IF;

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
                            strLine = MultStr(INDENT, bytBlockDepth) + "    ";
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
                    strLine = MultStr(INDENT, bytBlockDepth) + "    ";
                    blnFirstCmd = true;
                    strLine += "(";
                    blnInOrBlock = true;
                    bytCurByte = bytData[lngPos];
                    lngPos++;
                }

                //check for 'not' command
                if (bytCurByte == 0xFD) {   // NOT
                    blnInNotBlock = true;
                    bytCurByte = bytData[lngPos];
                    lngPos++;
                }

                //check for valid test command
                if ((bytCurByte > 0) && (bytCurByte <= TestCount)) {
                    if (!blnFirstCmd) {
                        if (blnInOrBlock) {
                            strLine += D_TKN_OR;
                        }
                        else {
                            strLine += D_TKN_AND;
                        }
                        stlOut.Add(strLine);
                        strLine = MultStr(INDENT, bytBlockDepth) + "    ";
                    }
                    bytCmd = bytCurByte;
                    if (SpecialSyntax && (bytCmd >= 1 && bytCmd <= 6)) {
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
                        strLine = strLine + TestCommands[bytCmd].Name + "(";
                        intArgStart = strLine.Length;
                        if (bytCmd == 14) {
                            // said command
                            bytNumSaidArgs = bytData[lngPos];
                            lngPos++;
                            for (intArg1Val = 1; intArg1Val <= bytNumSaidArgs; intArg1Val++) {
                                lngWordGroupNum = 256 * bytData[lngPos + 1] + bytData[lngPos];
                                lngPos += 2;
                                if (!WordsByNumber) {
                                    //enable error trapping to catch any nonexistent words
                                    try {
                                        //if word exists,
                                        if (agSierraSyntax) {
                                            strLine += QUOTECHAR + compGame.agVocabWords.GroupN(lngWordGroupNum).GroupName.Replace(' ', '$');
                                        }
                                        else {
                                            strLine += QUOTECHAR + compGame.agVocabWords.GroupN(lngWordGroupNum).GroupName + QUOTECHAR;
                                        }
                                    }
                                    catch (Exception) {
                                        //add the word by its number
                                        strLine += lngWordGroupNum;
                                        //set warning text
                                        AddDecodeWarning("DC02", "Invalid word (" + lngWordGroupNum + ") at position " + lngPos, stlOutput.Count);
                                    }
                                }
                                else {
                                    //alwys use word number as the argument
                                    strLine += lngWordGroupNum;
                                }

                                if (intArg1Val < bytNumSaidArgs) {
                                    strLine += ", ";
                                }
                            }

                        }
                        else {
                            //if at least one arg
                            if (TestCommands[bytCmd].ArgType.Length > 0) {
                                for (intArg1Val = 0; intArg1Val < TestCommands[bytCmd].ArgType.Length; intArg1Val++) {
                                    bytCurByte = bytData[lngPos];
                                    lngPos++;
                                    //get arg Value
                                    strArg = ArgValue(bytCurByte, TestCommands[bytCmd].ArgType[intArg1Val]);
                                    //if message error (no string returned)
                                    if (strArg.Length == 0) {
                                        //error string set by ArgValue function
                                        return false;
                                    }

                                    //if message
                                    if (TestCommands[bytCmd].ArgType[intArg1Val] == ArgTypeEnum.atMsg) {
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
                                                while (intCharCount != 1 && strArg[intCharCount - 1] != ' ');
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
                                                //create indent (but don't exceed 20 spaces (to ensure msgs aren't split
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
                                    if (intArg1Val < TestCommands[bytCmd].ArgType.Length - 1) {
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
                        AddDecodeWarning("DC06", "unknowntest19 at position " + lngPos + " is only valid in Amiga AGI versions", stlOutput.Count);
                    }
                }
                else if (bytCurByte == 0xFF) {
                    //done with if block; add //then//
                    strLine += D_TKN_THEN.Replace(ARG1, NEWLINE + MultStr(INDENT, bytBlockDepth + 1));
                    //(SkipToEndIf verified that max block depth is not exceeded)
                    //increase block depth counter
                    bytBlockDepth++;
                    DecodeBlock[bytBlockDepth].IsIf = true;
                    DecodeBlock[bytBlockDepth].Length = 256 * bytData[lngPos + 1] + bytData[lngPos];
                    lngPos += 2;
                    //check for length of zero
                    if (DecodeBlock[bytBlockDepth].Length == 0) {
                        //set warning text
                        AddDecodeWarning("DC07", "This block at position " + lngPos + " contains no commands", stlOutput.Count);
                    }
                    //validate end pos
                    DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth].Length + lngPos;
                    if (DecodeBlock[bytBlockDepth].EndPos >= bytData.Length - 1) {
                        //adjust to end
                        DecodeBlock[bytBlockDepth].EndPos = bytData.Length - 2;
                        //set warning text
                        AddDecodeWarning("DC08", "Block end at position " + lngPos + " past end of resource; adjusted to end of resource", stlOutput.Count);
                    }
                    //verify block ends before end of previous block
                    //(i.e. it's properly nested)
                    if (DecodeBlock[bytBlockDepth].EndPos > DecodeBlock[bytBlockDepth - 1].EndPos) {
                        //block is outside the previous block nest;
                        //this is an abnormal situation
                        //need to simulate this block by using else and goto
                        DecodeBlock[bytBlockDepth].IsOutside = true;
                        DecodeBlock[bytBlockDepth].JumpPos = DecodeBlock[bytBlockDepth].EndPos;
                        DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth - 1].EndPos;
                        //set warning text
                        AddDecodeWarning("DC09", "Block end (" + DecodeBlock[bytBlockDepth].JumpPos + ") outside of nested block at position " + lngPos, stlOutput.Count);
                    }
                    stlOut.Add(strLine);
                    //if any warnings
                    if (blnWarning) {
                        //add warning lines
                        for (i = 0; i < strWarning.Count; i++) {
                            stlOut.Add(MultStr(INDENT, bytBlockDepth) + D_TKN_COMMENT + " WARNING: " + strWarning[i]);
                        } //Next i
                          //reset warning flag + string
                        blnWarning = false;
                        strWarning = [];
                    }
                    strLine = MultStr(INDENT, bytBlockDepth);
                    blnIfFinished = true;
                }
                else {
                    //unknown test command
                    strError = "Unknown test command (" + bytCurByte + ") at position " + lngPos;
                    return false;
                }
            }
            while (!blnIfFinished);
            return true;
        }
        static bool SkipToEndIf(byte[] bytData) {
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

                if ((CurByte > 0) && (CurByte <= TestCount)) {
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
                        lngPos += TestCommands[ThisCommand].ArgType.Length;
                    }
                }
                else if (CurByte == 0xFF) {
                    if (bytBlockDepth >= MAX_BLOCK_DEPTH - 1) {
                        strError = "Too many nested blocks (" + (bytBlockDepth + 1) + ") at position " + lngPos;
                        return false;
                    }
                    // increment block counter
                    bytBlockDepth++;
                    DecodeBlock[bytBlockDepth].IsIf = true;
                    DecodeBlock[bytBlockDepth].Length = 256 * bytData[lngPos + 1] + bytData[lngPos];
                    lngPos += 2;
                    // block length of zero will cause warning in main loop, so no need to check for it here
                    DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth].Length + lngPos;
                    if (DecodeBlock[bytBlockDepth].EndPos > DecodeBlock[bytBlockDepth - 1].EndPos) {
                        //block is outside the previous block nest;
                        //
                        //this is an abnormal situation;
                        //need to simulate this block by using else and goto
                        DecodeBlock[bytBlockDepth].IsOutside = true;
                        DecodeBlock[bytBlockDepth].JumpPos = DecodeBlock[bytBlockDepth].EndPos;
                        DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth - 1].EndPos;
                        //add a new goto item
                        //(since error level is medium or low (dont need to worry about an invalid jumppos)
                        //if label is already created
                        for (i = 1; i <= bytLabelCount; i++) {
                            if (lngLabelPos[i] == DecodeBlock[bytBlockDepth].JumpPos) {
                                break;
                            }
                        }
                        //if loop exited normally (i will equal bytLabelCount+1)
                        if (i == bytLabelCount + 1) {
                            //increment label Count
                            bytLabelCount = (byte)i;
                            Array.Resize(ref lngLabelPos, bytLabelCount + 1);
                            //save this label position
                            lngLabelPos[bytLabelCount] = DecodeBlock[bytBlockDepth].JumpPos;
                        }
                    }
                    IfFinished = true;
                }
                else {
                    strError = "Unknown test command (" + CurByte + ") at position " + lngPos;
                    return false;
                }
            }
            while (!IfFinished);
            return true;
        }
        static bool FindLabels(byte[] bytData) {
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
            lngPos = 2;
            do {
                //check to see if the end of a block has been found
                //start at most recent block and work up to oldest block
                for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--) {
                    //if this position matches the end of this block
                    if (DecodeBlock[CurBlock].EndPos <= lngPos) {
                        // if off by exactly one, AND there's a quit cmd in this block
                        // AND this version is one that uses arg value for quit
                        // this error is most likely due to bad coding of quit cmd
                        // if otherwise not an exact match, it will be caught when the block ends are added
                        if (lngPos - DecodeBlock[CurBlock].EndPos == 1 && compGame.agIntVersion != "2.089" && DecodeBlock[CurBlock].HasQuit) {
                            strError = "CHECKQUIT";
                            return false;
                        }
                        //take this block off stack
                        bytBlockDepth--;
                    }
                }
                //get next byte
                bytCurData = bytData[lngPos];
                lngPos++;
                switch (bytCurData) {
                case 0xFF:  //this byte points to start of an IF statement
                    //find labels associated with this if statement
                    if (!SkipToEndIf(bytData)) {
                        //major error
                        return false;
                    }
                    break;
                case 0xFE:   //if the byte is a GOTO command
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
                    if ((DecodeBlock[bytBlockDepth].EndPos == lngPos) && (DecodeBlock[bytBlockDepth].IsIf) && (bytBlockDepth > 0) && (!ElseAsGoto)) {
                        //this block is now in the 'else' part, so reset flag
                        DecodeBlock[bytBlockDepth].IsIf = false;
                        DecodeBlock[bytBlockDepth].IsOutside = false;
                        //does this 'else' statement line up to end at the same
                        //point that the 'if' statement does?
                        //the end of this block is past where the 'if' block ended OR
                        //the block is negative (means jumping backward, so it MUST be a goto)
                        //length of block doesn't have enough room for code necessary to close the 'else'
                        if ((tmpBlockLength + lngPos > DecodeBlock[bytBlockDepth - 1].EndPos) || (tmpBlockLength < 0) || (DecodeBlock[bytBlockDepth].Length <= 3)) {
                            //this is a //goto// statement,
                            DoGoto = true;
                        }
                        else {
                            //this is an 'else' statement;
                            //readjust block end so the IF statement that owns this 'else'
                            //is ended correctly
                            DecodeBlock[bytBlockDepth].Length = tmpBlockLength;
                            DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth].Length + lngPos;
                        }
                    }
                    else {
                        //this is a goto statement (or an else statement while mGotos flag is false)
                        DoGoto = true;
                    }
                    // goto
                    if (DoGoto) {
                        LabelLoc = tmpBlockLength + lngPos;
                        // dont' need to check for invalid destination because it's checked in main decode loop
                        //if label not yet created
                        if (!lngLabelPos.Contains(LabelLoc)) {
                            //increment label Count
                            bytLabelCount++;
                            Array.Resize(ref lngLabelPos, bytLabelCount + 1);
                            //save this label position
                            lngLabelPos[bytLabelCount] = LabelLoc;
                        }
                        //for (i = 1; i <= bytLabelCount; i++) {
                        //    if (lngLabelPos[i] == LabelLoc) {
                        //        break;
                        //    }
                        //}
                        ////if loop exited normally (i will equal bytLabelCount+1)
                        //if (i == bytLabelCount + 1) {
                        //    //increment label Count
                        //    bytLabelCount++;
                        //    Array.Resize(ref lngLabelPos, bytLabelCount + 1);
                        //    //save this label position
                        //    lngLabelPos[bytLabelCount] = LabelLoc;
                        //}
                    }
                    break;
                case < MAX_CMDS: //byte is an AGI command
                    //skip over arguments to get next command
                    lngPos += ActionCommands[bytCurData].ArgType.Length;
                    break;
                default:
                    //not a valid command - eror depends on value
                    if (bytCurData <= 182) {
                        // it's a command, but not valid for this interpreter version -
                        // leave it to main decode function to deal with it
                    }
                    else {
                        //major error
                        strError = "Unknown action command (" + bytCurData + ") at position " + lngPos;
                        return false;
                    }
                    break;
                }
            }
            while (lngPos < lngMsgSecStart);
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
                DecodeBlock[i].EndPos = 0;
                DecodeBlock[i].IsIf = false;
                DecodeBlock[i].IsOutside = false;
                DecodeBlock[i].JumpPos = 0;
                DecodeBlock[i].Length = 0;
            }
            //return success
            return true;
        }
        static void DisplayMessages(List<string> stlOut) {
            int lngMsg;
            stlOut.Add(D_TKN_COMMENT + "Messages");
            //always skip msg[0] since it's n/a
            for (lngMsg = 1; lngMsg < stlMsgs.Count; lngMsg++) {
                if (blnMsgExists[lngMsg] && (ShowAllMessages || !blnMsgUsed[lngMsg])) {
                    stlOut.Add(D_TKN_MESSAGE.Replace(ARG1, lngMsg.ToString()).Replace(ARG2, stlMsgs[lngMsg]));
                }
            }
        }
        static string AddSpecialCmd(byte[] bytData, byte bytCmd) {
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
        static string AddSpecialIf(byte bytCmd, byte bytArg1, byte bytArg2, bool NOTOn) {
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
        static void AddBlockEnds(List<string> stlOutput) {
            int CurBlock, i;

            for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--) {
                // why would a less than apply here?
                // FOUND IT!!! christmas card is an example-
                // it ends with a quit cmd followed by zero value;
                // the zero value is the last cmd, but the interpreter
                // version expects an argument value, so it uses
                //  the zero value. this results in pointer going
                // past end of data, so it's one greater than
                // calculated block end
                if (DecodeBlock[CurBlock].EndPos < lngPos) {
                    AddDecodeWarning("DC14", "Expected block end does not align with calculated block end at position " + lngPos.ToString(), Compiler.stlOutput.Count);
                }
                if (DecodeBlock[CurBlock].EndPos <= lngPos) {
                    //check for unusual case where an if block ends outside the if block it is nested in
                    if (DecodeBlock[CurBlock].IsOutside) {
                        // end current block
                        stlOutput.Add(MultStr(INDENT, bytBlockDepth - 1) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                        if (ElseAsGoto) {
                            //add an goto to start new block
                            // TODO: check this - I think it's adding wrong goto tokens
                            stlOutput.Add(MultStr(INDENT, bytBlockDepth - 1) + D_TKN_GOTO);
                        }
                        else {
                            // append else to end of current block
                            stlOutput[^1] = stlOutput[^1] + D_TKN_ELSE.Replace(ARG1, NEWLINE + MultStr(INDENT, bytBlockDepth - 1));
                        }
                        //add a goto
                        for (i = 1; i <= bytLabelCount; i++) {
                            if (lngLabelPos[i] == DecodeBlock[CurBlock].JumpPos) {
                                stlOutput.Add(MultStr(INDENT, bytBlockDepth) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                                break;
                            }
                        }
                    }
                    //add end if
                    stlOutput.Add(MultStr(INDENT, CurBlock - 1) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                    bytBlockDepth--;
                }
            }
        }
        static void InitTokens(AGICodeStyle Style = AGICodeStyle.cstDefault) {
            INDENT = "".PadLeft(IndentSize);

            switch (Style) {
            case AGICodeStyle.cstDefault: //  '0 = default
                D_TKN_NOT = "!";
                D_TKN_IF = "if (";
                D_TKN_THEN = ")%1{"; //where %1 is a line feed plus indent at current level
                D_TKN_ELSE = "%1" + INDENT + "else%1" + INDENT + "{"; //where %1 is a line feed plus indent at current level
                D_TKN_ENDIF = "%1}"; // where %1 is an extra indent
                D_TKN_GOTO = "goto(%1)";
                D_TKN_EOL = ";";
                D_TKN_AND = " && ";
                D_TKN_OR = " || ";
                D_TKN_EQUAL = " == ";
                D_TKN_NOT_EQUAL = " != ";
                D_TKN_COMMENT = "[ ";
                D_TKN_MESSAGE = "#message %1 %2";
                break;
            default: //1 = standard VS style
                     //2 (or any other number) = modified VS style
                D_TKN_NOT = "!";
                D_TKN_IF = "if (";
                D_TKN_THEN = ") {";
                if (Style == AGICodeStyle.cstStdVisualStudio) {
                    D_TKN_ELSE = "%1else {";
                }
                else {
                    D_TKN_ELSE = " else {";
                }
                D_TKN_ENDIF = "}";
                D_TKN_GOTO = "goto(%1)";
                D_TKN_EOL = ";";
                D_TKN_AND = " && ";
                D_TKN_OR = " || ";
                D_TKN_EQUAL = " == ";
                D_TKN_NOT_EQUAL = " != ";
                D_TKN_COMMENT = "[ ";
                D_TKN_MESSAGE = "#message %1 %2";
                break;
            }
            blnTokensSet = true;
            if (agSierraSyntax) {
                //goto doesn't include parentheses
                D_TKN_GOTO = "goto %1";
            }
        }
    }
}
