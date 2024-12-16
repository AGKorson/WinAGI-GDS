using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.LogicCompiler;
using static WinAGI.Engine.LogicErrorLevel;

namespace WinAGI.Engine {
    /// <summary>
    /// This class contains all the members and methods needed to decode logic 
    /// resources into readable source code.
    /// </summary>
    public static class LogicDecoder {
        #region Structs
        internal struct DecodeBlockType {
            internal bool IsIf = false;
            internal int StartPos = 0;
            internal int EndPos = 0;
            internal int Length = 0;
            internal bool IsOutside = false;
            internal int JumpPos = 0;
            internal bool HasQuit = false;
            public DecodeBlockType() {
            }
        }
        #endregion

        #region Enums
        public enum AGICodeStyle {
            cstDefaultStyle,
            cstAltStyle1,
            cstAltStyle2
        }
        #endregion

        #region Members
        private static Logic compLogic;
        internal static byte bytLogComp;
        internal static string moduleID;
        static byte bytBlockDepth;
        static DecodeBlockType[] DecodeBlock = new DecodeBlockType[WinAGI.Engine.LogicCompiler.MAX_BLOCK_DEPTH];
        static int lngPos;
        static int intArgStart;
        static int[] lngLabelPos = [];
        static int lngMsgSecStart;
        static StringList stlMsgs;
        static bool[] blnMsgUsed = new bool[256];
        static bool[] blnMsgExists = new bool[256];
        static StringList stlOutput = [];
        static string strError;
        static bool badQuit = false;
        static byte mIndentSize = 4;
        internal static string agDefSrcExt = ".lgc";
        internal static AGICodeStyle mCodeStyle = AGICodeStyle.cstDefaultStyle;

        const int MAX_LINE_LEN = 80;
        internal static int LogicNum;
        // tokens for building source code output
        internal static bool blnTokensSet;
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
        static StringList strWarning = [];
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the number of spaces the logic decoder uses to indent each block level of code.
        /// </summary>
        public static byte IndentSize {
            get {
                // default is 4 spaces
                if (mIndentSize == 0) {
                    mIndentSize = 4;
                }
                return mIndentSize;
            }
            set {
                mIndentSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the GameID found in decoded logics from the set.game.id command.
        /// </summary>
        internal static string DecodeGameID {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the formatting style that theh decompilier uses. Currently supports
        /// three formats, Default, Visual Studio, and Modified Visual Studio.
        /// </summary>
        public static AGICodeStyle CodeStyle {
            get { return mCodeStyle; }
            set {
                mCodeStyle = value;
                // tokens need to be reset
                blnTokensSet = false;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if decompiled logics displays a list
        /// of all messages in the logic at the end of the code section.
        /// </summary>
        public static bool ShowAllMessages { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if message arguments in decompiled 
        /// logics are displayed as literal strings or as message argument numbers. 
        /// </summary>
        public static bool MsgsByNumber { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if inventory item arguments in 
        /// decompiled logics are displayed as literal strngs or as inventory item
        /// argument numbers.
        /// </summary>
        public static bool IObjsByNumber { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if vocablulary word arguments in 
        /// decompiled logics are displayed as literal strngs or as vocabulary word
        /// argument numbers.
        /// </summary>
        public static bool WordsByNumber { get; set; }

        /// <summary>
        /// Gets or sets the default source extension that WinAGI will use when 
        /// creating decompiled source code files from logics UNLESS overridden
        /// by this game's SourceExt property.
        /// </summary>
        public static string DefaultSrcExt {
            get {
                return agDefSrcExt;
            }
            set {
                agDefSrcExt = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether this game will decompile logics
        /// using the AGI community established syntax, or Sierra's original
        /// syntax.
        /// </summary>
        public static bool SpecialSyntax { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if reserved variables and flag are 
        /// displayed in decompiled logics as define names or as argument numbers.
        /// </summary>
        public static bool ReservedAsText { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// This is the main decoding method. It uses the bytecode data from the specified
        /// logic to create the corresponding source text.
        /// </summary>
        /// <param name="SourceLogic"></param>
        /// <param name="LogNum"></param>
        /// <returns>The decoded source text if successful. Otherwise an exception is thrown.</returns>
        internal static string DecodeLogic(Logic SourceLogic) {
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

            // initialize the decoder
            compLogic = SourceLogic;
            if (SourceLogic.Loaded) {
                bytLogComp = SourceLogic.Number;
            }
            else {
                bytLogComp = 0;
            }
            moduleID = SourceLogic.ID;
            byte[] bytData = SourceLogic.Data;
            stlOutput = [];
            strError = "";
            badQuit = false;

            if (!blnTokensSet) {
                InitTokens(CodeStyle);
            }
            for (i = 0; i < MAX_BLOCK_DEPTH; i++) {
                DecodeBlock[i].EndPos = 0;
                DecodeBlock[i].IsIf = false;
                DecodeBlock[i].IsOutside = false;
                DecodeBlock[i].JumpPos = 0;
                DecodeBlock[i].Length = 0;
                DecodeBlock[i].HasQuit = false;
            }
            // treat empty logic as a single return command
            if (bytData.Length == 0) {
                return "return();" + NEWLINE;
            }

            // extract messages (add two to offset because the message
            // section start is referenced relative to byte 2 of the
            // resource header)
            lngMsgSecStart = bytData[0] + (bytData[1] << 8) + 2;
            if (!ReadMessages(bytData, lngMsgSecStart, SourceLogic.V3Compressed != 2)) {
                SourceLogic.ErrLevel = 1;
                SourceLogic.ErrData[0] = strError;
                return "return();" + NEWLINE;
            }

            // reset main block info
            DecodeBlock[0].IsIf = false;
            DecodeBlock[0].EndPos = lngMsgSecStart;
            DecodeBlock[0].IsOutside = false;
            DecodeBlock[0].Length = lngMsgSecStart;

            // reset error flag
            strError = "";
            //locate labels, and mark them (this also validates all command bytes to be <=181)
            if (!FindLabels(bytData)) {
                // check for 'quit' cmd error (can happen if a game that is for a version where
                // quit() uses an argument has an older logic in it that uses a version of quit
                // that has no argument)
                if (strError == "CHECKQUIT") {
                    badQuit = true;
                    strError = "";
                    // try again
                    // TODO: this will only work if a single quit() cmd is in the logic;
                    // this whole approach needs refactoring (why isn't it handled within
                    // FindLabels? shouldn't have to call it twice...)
                    if (!FindLabels(bytData)) {
                        // use error string set by findlabels
                        SourceLogic.ErrLevel = 2;
                        SourceLogic.ErrData[0] = strError;
                        return "[ " + strError + NEWLINE + "return();" + NEWLINE;
                    }
                }
                else {
                    SourceLogic.ErrLevel = 2;
                    SourceLogic.ErrData[0] = strError;
                    return "[ " + strError + NEWLINE + "return();" + NEWLINE;
                }
            }
            // reset decoder to beginning of bytecode data
            bytBlockDepth = 0;
            lngPos = 2;
            if (bytLabelCount > 0) {
                lngNextLabel = 1;
            }
            // main decoder loop
            do {
                AddBlockEnds(stlOutput);
                // check for label at this position
                if (bytLabelCount > 0 && lngLabelPos[lngNextLabel] == lngPos) {
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
                bytCurData = bytData[lngPos++];
                switch (bytCurData) {
                case 0xFF:
                    // this byte starts an IF statement
                    if (!DecodeIf(bytData, stlOutput)) {
                        SourceLogic.ErrLevel = 4;
                        SourceLogic.ErrData[0] = strError;
                        return "[ " + strError + NEWLINE + "return();" + NEWLINE;
                    }
                    break;
                case 0xFE:
                    // this byte is a 'goto' or 'else'
                    blnGoto = false;
                    tmpBlockLen = 256 * bytData[lngPos + 1] + bytData[lngPos];
                    lngPos += 2;
                    // need to check for negative Value here
                    if (tmpBlockLen > 0x7FFF) {
                        // convert to negative number
                        tmpBlockLen -= 0x10000;
                    }
                    // check for an 'else' statement
                    if ((DecodeBlock[bytBlockDepth].EndPos == lngPos) && (DecodeBlock[bytBlockDepth].IsIf) && (bytBlockDepth > 0)) {
                        // 'else' block is same level as the associated 'if', but 
                        // block type is no longer an 'if'
                        DecodeBlock[bytBlockDepth].IsIf = false;
                        DecodeBlock[bytBlockDepth].IsOutside = false;
                        // confirm this 'else' block is correctly formed
                        if (tmpBlockLen + lngPos <= DecodeBlock[bytBlockDepth - 1].EndPos && tmpBlockLen >= 0 && DecodeBlock[bytBlockDepth].Length > 3) {
                            // insert the 'else' command in the desired syntax style
                            stlOutput.Add(INDENT.MultStr(bytBlockDepth - 1) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                            // append else to end of curent if block
                            stlOutput[^1] += D_TKN_ELSE.Replace(ARG1, NEWLINE + INDENT.MultStr(bytBlockDepth - 1));
                            // adjust length and endpos for the 'else' block
                            DecodeBlock[bytBlockDepth].Length = tmpBlockLen;
                            DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth].Length + lngPos;
                        }
                        else {
                            // else won't work; force it to be a goto
                            blnGoto = true;
                        }
                    }
                    else {
                        blnGoto = true;
                    }
                    // check for a 'goto'
                    if (blnGoto) {
                        lngLabelLoc = tmpBlockLen + lngPos;
                        // label already verified in FindLabels; add warning if necessary
                        if (lngLabelLoc > lngMsgSecStart - 1) {
                            AddDecodeWarning("DC13", "Goto destination is past end of logic; adjusted to end of logic [resource index: " + lngPos + "]", stlOutput.Count - 1);
                            // adjust it to end of resource
                            lngLabelLoc = lngMsgSecStart - 1;
                        }
                        for (i = 1; i <= bytLabelCount; i++) {
                            if (lngLabelPos[i] == lngLabelLoc) {
                                stlOutput.Add(INDENT.MultStr(bytBlockDepth) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                                if (blnWarning) {
                                    for (j = 0; j < strWarning.Count; j++) {
                                        stlOutput.Add(D_TKN_COMMENT + strWarning[j]);
                                    }
                                    blnWarning = false;
                                    strWarning = [];
                                }
                                break;
                            }
                        }
                    }
                    break;
                default:
                    // valid agi command (don't need to check for invalid command number;
                    // they are all validated in FindLabels)
                    if (bytCurData > ActionCount - 1) {
                        //this command is not expected for the targeted interpreter version
                        AddDecodeWarning("DC10", "This command is not valid for selected interpreter version (" + compGame.agIntVersion + ")  [resource index: " + lngPos + "]", stlOutput.Count - 1);
                    }
                    bytCmd = bytCurData;
                    string strCurrentLine = INDENT.MultStr(bytBlockDepth);
                    if (SpecialSyntax && (bytCmd >= 0x1 && bytCmd <= 0xB) || (bytCmd >= 0xA5 && bytCmd <= 0xA8)) {
                        strCurrentLine += AddSpecialCmd(bytData, bytCmd);
                    }
                    else {
                        strCurrentLine += ActionCommands[bytCmd].Name + "(";
                        intArgStart = strCurrentLine.Length;
                        for (intArg = 0; intArg < ActionCommands[bytCmd].ArgType.Length; intArg++) {
                            bytCurData = bytData[lngPos++];
                            strArg = ArgValue(bytCurData, ActionCommands[bytCmd].ArgType[intArg]);
                            if (ReservedAsText && compGame.UseReservedNames && SourceLogic.InGame) {
                                // some commands use resources as arguments; substitute as appropriate
                                switch (bytCmd) {
                                case 18:
                                    // new.room - only arg is a logic
                                    if (compGame.agLogs.Contains(bytCurData)) {
                                        strArg = compGame.agLogs[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "Logic " + bytCurData.ToString() + " in new.room() does not exist  [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    }
                                    break;
                                case 20: 
                                    // load.logics - only arg is a logic
                                    if (compGame.agLogs.Contains(bytCurData)) {
                                        strArg = compGame.agLogs[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "Logic " + bytCurData.ToString() + " in load.logics() " + lngPos.ToString() + " does not exist", stlOutput.Count - 1);
                                    }
                                    break;
                                case 22:
                                    // call - only arg is a logic
                                    if (compGame.agLogs.Contains(bytCurData)) {
                                        strArg = compGame.agLogs[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "Logic " + bytCurData.ToString() + " in call() does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    }
                                    break;
                                case 30:  
                                    // load.view - only arg is a view
                                    if (compGame.agViews.Contains(bytCurData)) {
                                        strArg = compGame.agViews[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "View " + bytCurData.ToString() + " in load.view() does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    }
                                    break;
                                case 32: 
                                    // discard.view - only arg is a view
                                    if (compGame.agViews.Contains(bytCurData)) {
                                        strArg = compGame.agViews[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "View " + bytCurData.ToString() + " in discard.view() does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    }
                                    break;
                                case 41: 
                                    // set.view - 2nd arg is a view
                                    if (intArg == 1) {
                                        if (compGame.agViews.Contains(bytCurData)) {
                                            strArg = compGame.agViews[bytCurData].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DC11", "View " + bytCurData.ToString() + " in set.view() does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                        }
                                    }
                                    break;
                                case 98:  
                                    // load.sound - only arg is asound
                                    if (compGame.agSnds.Contains(bytCurData)) {
                                        strArg = compGame.agSnds[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "Sound " + bytCurData.ToString() + " in load.sound() does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    }
                                    break;
                                case 99: 
                                    // sound = 1st arg is a sound
                                    if (intArg == 0) {
                                        if (compGame.agSnds.Contains(bytCurData)) {
                                            strArg = compGame.agSnds[bytCurData].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DC11", "Sound " + bytCurData.ToString() + " in sound() does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                        }
                                    }
                                    break;
                                case 122:
                                    // add.to.pic - 1st arg is a view
                                    if (intArg == 0) {
                                        if (compGame.agViews.Contains(bytCurData)) {
                                            strArg = compGame.agViews[bytCurData].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DC11", "View " + bytCurData.ToString() + " in add.to.pic() does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                        }
                                    }
                                    break;
                                case 129: 
                                    // show.obj - only arg is a view
                                    if (compGame.agViews.Contains(bytCurData)) {
                                        strArg = compGame.agViews[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "View " + bytCurData.ToString() + " in show.obj() at does not exist [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    }
                                    break;
                                case 150:
                                    // trace.info - 1st arg is a logic
                                    if (intArg == 0) {
                                        if (compGame.agLogs.Contains(bytCurData)) {
                                            strArg = compGame.agLogs[bytCurData].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DC11", "Logic " + bytCurData.ToString() + " in trace.info() does not exist", stlOutput.Count - 1);
                                        }
                                    }
                                    break;
                                case 175:
                                    // discard.sound - only arg is a sound
                                    if (compGame.agSnds.Contains(bytCurData)) {
                                        strArg = compGame.agSnds[bytCurData].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DC11", "Sound " + bytCurData.ToString() + " in discard.sound() does not exist", stlOutput.Count - 1);
                                    }
                                    break;
                                }
                            }
                            // check for commands that use colors here
                            if (ReservedAsText) {
                                switch (bytCmd) {
                                case 105:
                                    // clear.lines - 3rd arg
                                    if (intArg == 2) {
                                        if (strArg.Val() < 16) {
                                            strArg = agResColor[(int)strArg.Val()].Name;
                                        }
                                    }
                                    break;
                                case 109:
                                    // set.text.attribute - all args
                                    if (strArg.Val() < 16) {
                                        strArg = agResColor[(int)strArg.Val()].Name;
                                    }
                                    break;
                                case 154:
                                    // clear.text.rect - 5th arg
                                    if (intArg == 4) {
                                        if (strArg.Val() < 16) {
                                            strArg = agResColor[(int)strArg.Val()].Name;
                                        }
                                    }
                                    break;
                                }
                            }
                            if (ActionCommands[bytCmd].ArgType[intArg] == Msg) {
                                // split long messages over additional lines
                                do {
                                    if (strCurrentLine.Length + strArg.Length > MAX_LINE_LEN) {
                                        intCharCount = MAX_LINE_LEN - strCurrentLine.Length;
                                        // determine longest available section of message that can be added
                                        // without exceeding max line length
                                        if (intCharCount > 1) {
                                            while (intCharCount > 1 && strArg[intCharCount - 1] != ' ')
                                            {
                                                intCharCount--;
                                            }
                                            // if no space is found to split up the line
                                            if (intCharCount <= 1) {
                                                // just split it without worrying about a space
                                                intCharCount = MAX_LINE_LEN - strCurrentLine.Length;
                                            }
                                            // add the section of the message that fits on this line
                                            strCurrentLine = strCurrentLine + strArg.Left(intCharCount) + QUOTECHAR;
                                            strArg = strArg.Mid(intCharCount, strArg.Length - intCharCount);
                                            stlOutput.Add(strCurrentLine);
                                            // create indent (but don't exceed 20 spaces (to ensure msgs aren't split
                                            // up into absurdly small chunks)
                                            if (intArgStart >= MAX_LINE_LEN - 20) {
                                                intArgStart = MAX_LINE_LEN - 20;
                                            }
                                            strCurrentLine = " ".MultStr(intArgStart) + QUOTECHAR;
                                        }
                                        else {
                                            // line is messed up; just add it
                                            strCurrentLine += strArg;
                                            strArg = "";
                                        }
                                    }
                                    else {
                                        // not too long; add the message to current line
                                        strCurrentLine += strArg;
                                        strArg = "";
                                    }
                                }
                                // continue adding new lines until entire message is split and added
                                while (strArg.Length > 0);
                            }
                            else {
                                // check for quit() arg count error
                                if (bytCmd == 134 && badQuit) {
                                    AddDecodeWarning("DC12", "quit() command coded with no argument [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    // reset position index so it gets next byte correctly
                                    lngPos--;
                                }
                                else {
                                    strCurrentLine += strArg;
                                }
                            }
                            if (intArg < ActionCommands[bytCmd].ArgType.Length - 1) {
                                strCurrentLine += ", ";
                            }
                        }
                        strCurrentLine += ")";
                    }
                    // check for set.game.id
                    if (bytCmd == 143) {
                        // use this as suggested gameid
                        DecodeGameID = stlMsgs[bytCurData][1..^1];
                    }
                    strCurrentLine += D_TKN_EOL;
                    stlOutput.Add(strCurrentLine);
                    if (blnWarning) {
                        for (i = 0; i < strWarning.Count; i++) {
                            stlOutput.Add(D_TKN_COMMENT + strWarning[i]);
                        }
                        // reset warning
                        blnWarning = false;
                        strWarning = [];
                    }
                    break;
                }
            }
            while (lngPos < lngMsgSecStart);
            // add any remaining block ends
            AddBlockEnds(stlOutput);
            // confirm logic ends with return
            if (bytCmd != 0) {
                AddDecodeWarning("DC15", "return() command missing from end of logic", stlOutput.Count - 1);
                // last warning to add so just append it
                stlOutput.Add(INDENT.MultStr(bytBlockDepth) + CMT1_TOKEN + "WARNING DC15: return() command is missing from end of logic");
            }
            // include a blank line at end of code
            stlOutput.Add("");
            // add message declaration lines
            DisplayMessages(stlOutput);
            // done
            return string.Join(NEWLINE, [.. stlOutput]);
        }

        /// <summary>
        /// This method sends a warning event to the calling program and also adds the
        /// warning to the warning stack so it can beadded to the output once the 
        /// current line is fully decompiled.
        /// </summary>
        /// <param name="WarnID"></param>
        /// <param name="WarningText"></param>
        /// <param name="LineNum"></param>
        static void AddDecodeWarning(string WarnID, string WarningText, int LineNum) {
            TWinAGIEventInfo dcWarnInfo = new() {
                ResNum = bytLogComp,
                ResType = AGIResType.Logic,
                Type = EventType.DecompWarning,
                ID = WarnID,
                Module = moduleID,
                Filename = "",
                Text = WarningText,
                Line = LineNum.ToString(),
            };
            AGIGame.OnDecodeLogicStatus(dcWarnInfo);
            if (!blnWarning) {
                blnWarning = true;
            }
            strWarning.Add("WARNING " + WarnID + ": " + WarningText);
        }

        /// <summary>
        /// This method converts the specified argument bytecode into an appropriately
        /// formatted string representation, either a define value, or a simple 
        /// argument marker.
        /// </summary>
        /// <param name="ArgNum"></param>
        /// <param name="ArgType"></param>
        /// <param name="VarVal"></param>
        /// <returns></returns>
        static string ArgValue(byte ArgNum, ArgType ArgType, int VarVal = -1) {
            if (!ReservedAsText && ArgType != Msg) {
                // return simple argument marker
                return agArgTypPref[(int)ArgType] + ArgNum;
            }
            switch (ArgType) {
            case Num:
                switch (VarVal) {
                case 2 or 5:
                    // v2 and v5 use edge codes
                    if (ArgNum <= 4) {
                        return agEdgeCodes[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 6:
                    // v6 uses direction codes
                    if (ArgNum <= 8) {
                        return agEgoDir[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 20:
                    // v20 uses computer type codes
                    if (ArgNum <= 8) {
                        return agCompType[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 26:
                    // v26 uses video mode codes
                    if (ArgNum <= 4) {
                        return agVideoMode[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                default:
                    // use default
                    return ArgNum.ToString();
                }
            case Var:
                if (ArgNum <= 26) {
                    return agResVar[ArgNum].Name;
                }
                else {
                    return 'v' + ArgNum.ToString();
                }
            case Flag:
                if (ArgNum <= 16) {
                    return agResFlag[ArgNum].Name;
                }
                else if (ArgNum == 20 && (double.Parse(compGame.agIntVersion) >= 3.002102)) {
                    return agResFlag[17].Name;
                }
                else {
                    //not a reserved data type
                    return 'f' + ArgNum.ToString();
                }
            case Msg:
                blnMsgUsed[ArgNum] = true;
                if (blnMsgExists[ArgNum]) {
                    if (MsgsByNumber) {
                        return 'm' + ArgNum.ToString();
                    }
                    else {
                        return stlMsgs[ArgNum];
                    }
                }
                else {
                    // message doesn't exist; raise a warning
                    AddDecodeWarning("DC01", "Invalid message m" + ArgNum + " [resource index: " + lngPos + "]", stlOutput.Count - 1);
                    return 'm' + ArgNum.ToString();
                }
            case SObj:
                if (ArgNum == 0) {
                    return agResObj[0].Name;
                }
                else {
                    return 'o' + ArgNum.ToString();
                }
            case InvItem:
                if (compGame is not null && !IObjsByNumber) {
                    Debug.Assert(compGame.agInvObj.Loaded);
                    if (ArgNum < compGame.agInvObj.Count) {
                        if (compGame.agInvObj[ArgNum].Unique) {
                            if (compGame.agInvObj[ArgNum].ItemName == "?") {
                                // use the inventory item number, and post a warning
                                AddDecodeWarning("DC04", "Reference to null inventory item ('?')  [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                return 'i' + ArgNum.ToString();
                            }
                            else {
                                // a unique, non-questionmark item- use it's string Value
                                return QUOTECHAR + compGame.agInvObj[ArgNum].ItemName.Replace(QUOTECHAR.ToString(), "\\\"") + QUOTECHAR;
                            }
                        }
                        else {
                            //non-unique - use obj number instead
                            switch (ErrorLevel) {
                            case High or Medium:
                                AddDecodeWarning("DC05", "Non-unique inventory item '" + compGame.agInvObj[ArgNum].ItemName + "' [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                break;
                            }
                            return 'i' + ArgNum.ToString();
                        }
                    }
                    else {
                        AddDecodeWarning("DC03", "Invalid inventory item (i" + ArgNum + ") [resource index: " + lngPos + "]", stlOutput.Count - 1);
                        return 'i' + ArgNum.ToString();
                    }
                }
                else {
                    // always refer to the object by number if not in a game
                    return 'i' + ArgNum.ToString();
                }
            case ArgType.Str:
                if (ArgNum == 0) {
                    return agResStr[0].Name;
                }
                else {
                    return 's' + ArgNum.ToString();
                }
            case ArgType.Ctrl:
                return 'c' + ArgNum.ToString();

            case ArgType.Word:
                // convert argument to a 'one-based' value so it is consistent with
                // the syntax used in the agi 'print' commands
                return 'w' + (ArgNum + 1).ToString();
            }

            //can't get here, but compiler demands a return statement
            return "";
        }

        /// <summary>
        /// This method extracts message text from the end of the logic resource.
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="lngMsgStart"></param>
        /// <param name="Decrypt"></param>
        /// <returns>true if messages extracted successfully, false if any errors 
        /// occur which prevent decooding the logic</returns>
        static bool ReadMessages(byte[] bytData, int lngMsgStart, bool Decrypt) {
            int lngMsgTextEnd;
            int[] MessageStart = new int[256];
            int intCurMsg;
            int lngMsgTextStart;
            List<byte> bMsgText;
            bool blnEndOfMsg;
            byte bytInput;
            int NumMessages;

            // validate msg start
            if (lngMsgStart >= bytData.Length) {
                // invalid logic
                AddDecodeWarning("DC18", "Invalid message section data", stlOutput.Count - 1);
                // exit with failure
                return false;
            }
            lngPos = lngMsgStart;
            stlMsgs = [];
            // first msg, with index of zero, is null/not used:
            // There is no message 0 (it is not supported by the file format). the
            // two bytes which correspond to message 0 offset is used to hold the
            // end of text ptr so AGI can decrypt the message text when the logic
            // is initially loaded
            stlMsgs.Add("");
            NumMessages = bytData[lngPos++];
            if (NumMessages > 0) {
                // calculate end of message pointer
                lngMsgTextEnd = lngMsgStart + 256 * bytData[lngPos + 1] + bytData[lngPos];
                lngPos += 2;
                if (lngMsgTextEnd != bytData.Length - 1) {
                    AddDecodeWarning("DC16", "Message section has invalid end-of-text marker", stlOutput.Count - 1);
                    // adjust it to end
                    lngMsgTextEnd = bytData.Length - 1;
                }
                // loop through all messages, extract offset
                for (intCurMsg = 1; intCurMsg <= NumMessages; intCurMsg++) {
                    // set start of this msg as start of msg block, plus offset, plus one (for byte which gives number of msgs)
                    MessageStart[intCurMsg] = 256 * bytData[lngPos + 1] + bytData[lngPos] + lngMsgStart + 1;
                    // validate msg start
                    if (MessageStart[intCurMsg] >= bytData.Length) {
                        AddDecodeWarning("DC17", "Message " + intCurMsg + " has invalid offset", stlOutput.Count - 1);
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
                // now read all messages
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
                                    // new line
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(110); // 'n'
                                    break;
                                case 0x22:
                                    // quote char
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(34); // '"'
                                    break;
                                case 0x5C:
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(92); // '\'
                                    break;
                                case < 0x20 or 0x7F or 0xFF:
                                    // control characters and other unprintable characters
                                    bMsgText.Add(92); // '\'
                                    bMsgText.Add(120); // 'x'
                                    bMsgText.Add((byte)bytInput.ToString("x2")[0]);
                                    bMsgText.Add((byte)bytInput.ToString("x2")[1]);
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
                            stlMsgs.Add(QUOTECHAR + compLogic.CodePage.GetString(bMsgText.ToArray()) + QUOTECHAR);
                        }
                        blnMsgExists[intCurMsg] = true;
                    }
                    else {
                        // add a null message (so numbers work out)
                        stlMsgs.Add("");
                        blnMsgExists[intCurMsg] = false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// This method decodes an 'if' block of bytecode data.
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="stlOut"></param>
        /// <returns></returns>
        static bool DecodeIf(byte[] bytData, StringList stlOut) {
            bool blnFirstCmd = true;
            bool blnInOrBlock = false;
            bool blnInNotBlock;
            bool blnIfFinished = false;
            int intArg1Val;
            byte bytArg2Val;
            byte bytCurByte;
            byte bytNumSaidArgs;
            int lngWordGroupNum;
            byte bytCmd;
            int i;
            string strLine = INDENT.MultStr(bytBlockDepth) + D_TKN_IF;

            do {
                // always reset 'NOT' block status to false
                blnInNotBlock = false;
                bytCurByte = bytData[lngPos++];
                //first, check for an 'or'
                if (bytCurByte == 0xFC) {
                    blnInOrBlock = !blnInOrBlock;
                    if (blnInOrBlock) {
                        if (!blnFirstCmd) {
                            strLine += D_TKN_AND;
                            stlOut.Add(strLine);
                            strLine = INDENT.MultStr(bytBlockDepth) + "    ";
                            blnFirstCmd = true;
                        }
                        strLine += "(";
                    }
                    else {
                        strLine += ")";
                    }
                    bytCurByte = bytData[lngPos++];
                }
                // special check needed in case two 0xFCs are in a row, e.g. (a || b) && (c || d)
                if ((bytCurByte == 0xFC) && (!blnInOrBlock)) {
                    strLine += D_TKN_AND;
                    stlOut.Add(strLine);
                    strLine = INDENT.MultStr(bytBlockDepth) + "    ";
                    blnFirstCmd = true;
                    strLine += "(";
                    blnInOrBlock = true;
                    bytCurByte = bytData[lngPos++];
                }
                // check for 'not' command
                if (bytCurByte == 0xFD) {
                    blnInNotBlock = true;
                    bytCurByte = bytData[lngPos++];
                }
                // check for valid test command
                if ((bytCurByte > 0) && (bytCurByte <= TestCount)) {
                    if (!blnFirstCmd) {
                        if (blnInOrBlock) {
                            strLine += D_TKN_OR;
                        }
                        else {
                            strLine += D_TKN_AND;
                        }
                        stlOut.Add(strLine);
                        strLine = INDENT.MultStr(bytBlockDepth) + "    ";
                    }
                    bytCmd = bytCurByte;
                    if (SpecialSyntax && (bytCmd >= 1 && bytCmd <= 6)) {
                        bytCurByte = bytData[lngPos++];
                        bytArg2Val = bytData[lngPos++];
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
                            bytNumSaidArgs = bytData[lngPos++];
                            for (intArg1Val = 1; intArg1Val <= bytNumSaidArgs; intArg1Val++) {
                                lngWordGroupNum = 256 * bytData[lngPos + 1] + bytData[lngPos];
                                lngPos += 2;
                                if (!WordsByNumber && compGame is not null) {
                                    if (compGame.agVocabWords.GroupExists(lngWordGroupNum)) {
                                        if (agSierraSyntax) {
                                            strLine += QUOTECHAR + compGame.agVocabWords.GroupN(lngWordGroupNum).GroupName.Replace(' ', '$');
                                        }
                                        else {
                                            strLine += QUOTECHAR + compGame.agVocabWords.GroupN(lngWordGroupNum).GroupName + QUOTECHAR;
                                        }
                                    }
                                    else {
                                        // add the word by its group number
                                        strLine += lngWordGroupNum;
                                        AddDecodeWarning("DC02", "Invalid word (" + lngWordGroupNum + ")  [resource index: " + lngPos + "]", stlOutput.Count - 1);
                                    }
                                }
                                else {
                                    strLine += lngWordGroupNum;
                                }

                                if (intArg1Val < bytNumSaidArgs) {
                                    strLine += ", ";
                                }
                            }

                        }
                        else {
                            if (TestCommands[bytCmd].ArgType.Length > 0) {
                                for (intArg1Val = 0; intArg1Val < TestCommands[bytCmd].ArgType.Length; intArg1Val++) {
                                    bytCurByte = bytData[lngPos++];
                                    strLine += ArgValue(bytCurByte, TestCommands[bytCmd].ArgType[intArg1Val]);
                                    if (intArg1Val < TestCommands[bytCmd].ArgType.Length - 1) {
                                        strLine += ", ";
                                    }
                                }
                            }
                        }
                        strLine += ")";
                    }
                    blnFirstCmd = false;
                    if (bytCmd == 19) {
                        // add warning if this is the unknown test19 command
                        AddDecodeWarning("DC06", "unknowntest19 is only valid in Amiga AGI versions [resource index: " + lngPos + "]", stlOutput.Count - 1);
                    }
                }
                else if (bytCurByte == 0xFF) {
                    // done with if block; add 'then'
                    strLine += D_TKN_THEN.Replace(ARG1, NEWLINE + INDENT.MultStr(bytBlockDepth + 1));
                    bytBlockDepth++;
                    DecodeBlock[bytBlockDepth].IsIf = true;
                    DecodeBlock[bytBlockDepth].Length = 256 * bytData[lngPos + 1] + bytData[lngPos];
                    lngPos += 2;
                    if (DecodeBlock[bytBlockDepth].Length == 0) {
                        AddDecodeWarning("DC07", "This block contains no commands [resource index: " + lngPos + "]", stlOutput.Count - 1);
                    }
                    // validate end pos
                    DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth].Length + lngPos;
                    if (DecodeBlock[bytBlockDepth].EndPos >= bytData.Length - 1) {
                        // adjust to end
                        DecodeBlock[bytBlockDepth].EndPos = bytData.Length - 2;
                        AddDecodeWarning("DC08", "Block end is past end of resource; adjusted to end of resource [resource index: " + lngPos + "]", stlOutput.Count - 1);
                    }
                    // verify block ends before end of previous block
                    // (i.e. it's properly nested)
                    if (DecodeBlock[bytBlockDepth].EndPos > DecodeBlock[bytBlockDepth - 1].EndPos) {
                        // block is outside the previous block nest;
                        // this is an abnormal situation so we need to simulate this block
                        // by using 'else' and 'goto'
                        DecodeBlock[bytBlockDepth].IsOutside = true;
                        DecodeBlock[bytBlockDepth].JumpPos = DecodeBlock[bytBlockDepth].EndPos;
                        DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth - 1].EndPos;
                        AddDecodeWarning("DC09", "Block end (" + DecodeBlock[bytBlockDepth].JumpPos + ") outside of nested block [resource index: " + lngPos + "]", stlOutput.Count - 1);
                    }
                    stlOut.Add(strLine);
                    if (blnWarning) {
                        for (i = 0; i < strWarning.Count; i++) {
                            stlOut.Add(INDENT.MultStr(bytBlockDepth) + D_TKN_COMMENT + strWarning[i]);
                        }
                        blnWarning = false;
                        strWarning = [];
                    }
                    strLine = INDENT.MultStr(bytBlockDepth);
                    blnIfFinished = true;
                }
                else {
                    // unknown test command
                    strError = "Unknown test command (" + bytCurByte + ")  [resource index: " + lngPos + "]";
                    return false;
                }
            }
            while (!blnIfFinished);
            return true;
        }

        /// <summary>
        /// This method is used by the FindLabel method to skip the cursor to
        /// the end of the current 'if' block. It also validates basic command and
        /// block data.
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        static bool SkipToEndIf(byte[] bytData) {
            byte CurByte;
            byte NumSaidArgs;
            byte ThisCommand;
            int i;

            do {
                CurByte = bytData[lngPos++];
                if (CurByte == 0xFC) {
                    CurByte = bytData[lngPos++];
                }
                if (CurByte == 0xFC) {
                    // two 0xFCs in a row, e.g. (a || b) && (c || d)
                    CurByte = bytData[lngPos++];
                }
                if (CurByte == 0xFD) {
                    CurByte = bytData[lngPos++];
                }

                if ((CurByte > 0) && (CurByte <= TestCount)) {
                    ThisCommand = CurByte;
                    if (ThisCommand == 14) {
                        // said command
                        NumSaidArgs = bytData[lngPos++];
                        // move pointer to next position past these arguments
                        // (words use two bytes per argument, not one)
                        lngPos += NumSaidArgs * 2;
                    }
                    else {
                        // move pointer to next position past the arguments for this command
                        lngPos += TestCommands[ThisCommand].ArgType.Length;
                    }
                }
                else if (CurByte == 0xFF) {
                    if (bytBlockDepth >= MAX_BLOCK_DEPTH - 1) {
                        strError = "Too many nested blocks (" + (bytBlockDepth + 1) + ") [resource index: " + lngPos + "]";
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
                        // block is outside the previous block nest;
                        // this is an abnormal situation so we need to simulate this block by using
                        // else and goto
                        DecodeBlock[bytBlockDepth].IsOutside = true;
                        DecodeBlock[bytBlockDepth].JumpPos = DecodeBlock[bytBlockDepth].EndPos;
                        DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth - 1].EndPos;
                        //add a new goto item (since error level is medium or low (dont need to
                        //worry about an invalid jumppos)
                        // check fo existing label at jump pos
                        for (i = 1; i <= bytLabelCount; i++) {
                            if (lngLabelPos[i] == DecodeBlock[bytBlockDepth].JumpPos) {
                                break;
                            }
                        }
                        // if loop exited normally (i will equal bytLabelCount+1)
                        if (i == bytLabelCount + 1) {
                            // add new label at jump pos
                            bytLabelCount = (byte)i;
                            Array.Resize(ref lngLabelPos, bytLabelCount + 1);
                            lngLabelPos[bytLabelCount] = DecodeBlock[bytBlockDepth].JumpPos;
                        }
                    }
                    // end of block
                    return true;
                }
                else {
                    strError = "Unknown test command (" + CurByte + ") [resource index: " + lngPos + "]";
                    return false;
                }
            }
            while (true);
        }

        /// <summary>
        /// This method scans the bytecode data for goto statements and identifies 
        /// all the jump positions as labels. The list of labels is then sorted to
        /// they can be added in the correct place when the logic is decoded.
        /// </summary>
        /// <param name="bytData"></param>
        /// <returns></returns>
        static bool FindLabels(byte[] bytData) {
            int i, j, CurBlock;
            byte bytCurData;
            int tmpBlockLength;
            bool DoGoto;
            int LabelLoc;

            bytBlockDepth = 0;
            bytLabelCount = 0;
            lngPos = 2;
            do {
                // check for end of a block (start at most recent block and work up
                // to oldest block)
                for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--) {
                    if (DecodeBlock[CurBlock].EndPos <= lngPos) {
                        // if off by exactly one, AND there's a quit cmd in this block
                        // AND this version is one that uses arg value for quit
                        // this error is most likely due to bad coding of quit cmd
                        // if otherwise not an exact match, it will be caught when the block ends are added
                        if (lngPos - DecodeBlock[CurBlock].EndPos == 1 && compGame.agIntVersion != "2.089" && DecodeBlock[CurBlock].HasQuit) {
                            strError = "CHECKQUIT";
                            return false;
                        }
                        // take this block off stack
                        bytBlockDepth--;
                    }
                }
                bytCurData = bytData[lngPos++];
                switch (bytCurData) {
                case 0xFF:
                    // start of an IF statement
                    if (!SkipToEndIf(bytData)) {
                        // major error
                        return false;
                    }
                    break;
                case 0xFE:
                    // GOTO command
                    //reset goto status flag
                    DoGoto = false;
                    tmpBlockLength = 256 * bytData[lngPos + 1] + bytData[lngPos];
                    lngPos += 2;
                    // check for negative value
                    if (tmpBlockLength > 0x7FFF) {
                        tmpBlockLength -= 0x10000;
                    }
                    //check to see if this 'goto' might be an 'else':
                    //  - end of this block matches this position (the if-then part is done)
                    //  - this block is identified as an IF block
                    //  - this is NOT the main block
                    //  - the flag to set elses as gotos is turned off
                    if ((DecodeBlock[bytBlockDepth].EndPos == lngPos) && (DecodeBlock[bytBlockDepth].IsIf) && (bytBlockDepth > 0)) {
                        // the 'else' block re-uses same level as the 'if'
                        DecodeBlock[bytBlockDepth].IsIf = false;
                        DecodeBlock[bytBlockDepth].IsOutside = false;
                        // confirm the 'else' block is formed correctly:
                        //  - the end of this block doen't go past where the 'if' block ended
                        //  - the block is not negative (means jumping backward, so it MUST be a goto)
                        //  - length of block has enough room for code necessary to close the 'else'
                        if ((tmpBlockLength + lngPos > DecodeBlock[bytBlockDepth - 1].EndPos) || (tmpBlockLength < 0) || (DecodeBlock[bytBlockDepth].Length <= 3)) {
                            //this is a 'goto' statement
                            DoGoto = true;
                        }
                        else {
                            // this is an 'else' block - readjust block end so the IF statement that
                            // owns this 'else' is ended correctly
                            DecodeBlock[bytBlockDepth].Length = tmpBlockLength;
                            DecodeBlock[bytBlockDepth].EndPos = DecodeBlock[bytBlockDepth].Length + lngPos;
                        }
                    }
                    else {
                        //this is a goto statement (or an else statement while ElseAsGoto flag is true)
                        DoGoto = true;
                    }
                    // goto
                    if (DoGoto) {
                        LabelLoc = tmpBlockLength + lngPos;
                        // don't need to check for invalid destination because it's checked in main decode loop
                        if (!lngLabelPos.Contains(LabelLoc)) {
                            // add a new label location
                            bytLabelCount++;
                            Array.Resize(ref lngLabelPos, bytLabelCount + 1);
                            lngLabelPos[bytLabelCount] = LabelLoc;
                        }
                    }
                    break;
                case <=  MAX_CMDS:
                    // AGI command
                    // skip over arguments to get next command
                    // (even if it's not valid for this version, just account for it here;
                    // the main decoder loop will handle the warning/error for commands 
                    // that are not valid in this version)
                    lngPos += ActionCommands[bytCurData].ArgType.Length;
                    // check for quit command
                    if (bytCurData == 134) {
                        DecodeBlock[bytBlockDepth].HasQuit = true;
                        if (badQuit) {
                            // if attempting to fix a bad quit command error, adjust position
                            // backward to account for the skipped quit command argument
                            lngPos--;
                        }
                    }
                    break;
                default:
                    // major error
                    strError = "Unknown action command (" + bytCurData + ") [resource index: " + lngPos + "]";
                    return false;
                }
            }
            while (lngPos < lngMsgSecStart);
            // sort labels, if found
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
            // clear block info (BUT don't overwrite main block)
            for (i = 1; i < MAX_BLOCK_DEPTH; i++) {
                DecodeBlock[i].EndPos = 0;
                DecodeBlock[i].IsIf = false;
                DecodeBlock[i].IsOutside = false;
                DecodeBlock[i].HasQuit = false;
                DecodeBlock[i].JumpPos = 0;
                DecodeBlock[i].Length = 0;
            }
            return true;
        }

        /// <summary>
        /// This method adds the message declarations to end of the source code output.
        /// </summary>
        /// <param name="stlOut"></param>
        static void DisplayMessages(StringList stlOut) {
            int lngMsg;
            stlOut.Add(D_TKN_COMMENT + "DECLARED MESSAGES");
            for (lngMsg = 1; lngMsg < stlMsgs.Count; lngMsg++) {
                if (blnMsgExists[lngMsg] && (ShowAllMessages || !blnMsgUsed[lngMsg])) {
                    stlOut.Add(D_TKN_MESSAGE.Replace(ARG1, lngMsg.ToString()).Replace(ARG2, stlMsgs[lngMsg]));
                }
            }
        }

        /// <summary>
        /// This method adds action commands that use special syntax form (i.e. v# += 1;).
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="bytCmd"></param>
        /// <returns></returns>
        static string AddSpecialCmd(byte[] bytData, byte bytCmd) {
            byte bytArg1, bytArg2;

            bytArg1 = bytData[lngPos];
            lngPos++;
            switch (bytCmd) {
            case 0x1:
                // increment
                return "++" + ArgValue(bytArg1, Var);
            case 0x2:
                // decrement
                return "--" + ArgValue(bytArg1, Var);
            case 0x3:
                // assignn
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Num, bytArg1);
            case 0x4:
                // assignv
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Var);
            case 0x5:
                // addn
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + "  += " + ArgValue(bytArg2, Num);
            case 0x6:
                // addv
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + "  += " + ArgValue(bytArg2, Var);
            case 0x7:
                // subn
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " -= " + ArgValue(bytArg2, Num);
            case 0x8:
                // subv
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " -= " + ArgValue(bytArg2, Var);
            case 0x9:
                // lindirectv
                bytArg2 = bytData[lngPos++];
                return "*" + ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Var);
            case 0xA:
                // rindirect
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " = *" + ArgValue(bytArg2, Var);
            case 0xB:
                // lindirectn
                bytArg2 = bytData[lngPos++];
                return "*" + ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Num);
            case 0xA5:
                // mul.n
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " *= " + ArgValue(bytArg2, Num);
            case 0xA6:
                // mul.v
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " *= " + ArgValue(bytArg2, Var);
            case 0xA7:
                // div.n
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " /= " + ArgValue(bytArg2, Num);
            case 0xA8:
                // div.v
                bytArg2 = bytData[lngPos++];
                return ArgValue(bytArg1, Var) + " /= " + ArgValue(bytArg2, Var);
            default:
                return "";
            }
        }

        /// <summary>
        /// This method adds test commands that use special syntax form (i.e. v# == #).
        /// </summary>
        /// <param name="bytCmd"></param>
        /// <param name="bytArg1"></param>
        /// <param name="bytArg2"></param>
        /// <param name="NOTOn"></param>
        /// <returns></returns>
        static string AddSpecialIf(byte bytCmd, byte bytArg1, byte bytArg2, bool NOTOn) {
            string retval = ArgValue(bytArg1, Var);
            switch (bytCmd) {
            case 1 or 2:
                // equaln or equalv
                if (NOTOn) {
                    retval += D_TKN_NOT_EQUAL;
                }
                else {
                    retval += D_TKN_EQUAL;
                }
                if (bytCmd == 2) {
                    retval += ArgValue(bytArg2, Var, bytArg1);
                }
                else {
                    retval += ArgValue(bytArg2, Num, bytArg1);
                }
                break;
            case 3 or 4:
                // lessn, lessv
                if (NOTOn) {
                    retval += " >= ";
                }
                else {
                    retval += " < ";
                }
                if (bytCmd == 4) {
                    retval += ArgValue(bytArg2, Var, bytArg1);
                }
                else {
                    retval += ArgValue(bytArg2, Num, bytArg1);
                }

                break;
            case 5 or 6:
                // greatern, greaterv
                if (NOTOn) {
                    retval += " <= ";
                }
                else {
                    retval += " > ";
                }
                if (bytCmd == 6) {
                    retval += ArgValue(bytArg2, Var, bytArg1);
                }
                else {
                    retval += ArgValue(bytArg2, Num, bytArg1);
                }
                break;
            }
            return retval;
        }

        /// <summary>
        /// This method adds all the block ends that are aligned with the current position.
        /// </summary>
        /// <param name="stlOutput"></param>
        static void AddBlockEnds(StringList stlOutput) {
            int CurBlock, i;

            for (CurBlock = bytBlockDepth; CurBlock > 0; CurBlock--) {
                // in some rare cases, the blocks don't align correctly
                if (DecodeBlock[CurBlock].EndPos < lngPos) {
                    AddDecodeWarning("DC14", "Expected block end does not align with calculated block end  [resource index: " + lngPos + "]", stlOutput.Count - 1);
                }
                if (DecodeBlock[CurBlock].EndPos <= lngPos) {
                    // check for unusual case where an if block ends outside the if block it is nested in
                    if (DecodeBlock[CurBlock].IsOutside) {
                        // end current block
                        stlOutput.Add(INDENT.MultStr(bytBlockDepth - 1) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                        // append else to end of current block
                        stlOutput[^1] = stlOutput[^1] + D_TKN_ELSE.Replace(ARG1, NEWLINE + INDENT.MultStr(bytBlockDepth - 1));
                        // then add a goto
                        for (i = 1; i <= bytLabelCount; i++) {
                            if (lngLabelPos[i] == DecodeBlock[CurBlock].JumpPos) {
                                stlOutput.Add(INDENT.MultStr(bytBlockDepth) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                                break;
                            }
                        }
                    }
                    // add end if
                    stlOutput.Add(INDENT.MultStr(CurBlock - 1) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                    bytBlockDepth--;
                }
            }
        }

        /// <summary>
        /// This method initializes all the tokens to match the desired code style.
        /// </summary>
        /// <param name="Style"></param>
        static void InitTokens(AGICodeStyle Style = AGICodeStyle.cstDefaultStyle) {
            INDENT = "".PadLeft(IndentSize);
            switch (Style) {
            case AGICodeStyle.cstDefaultStyle: 
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
            default: 
                D_TKN_NOT = "!";
                D_TKN_IF = "if (";
                D_TKN_THEN = ") {";
                if (Style == AGICodeStyle.cstAltStyle1) {
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
            if (agSierraSyntax) {
                // goto doesn't include parentheses
                D_TKN_GOTO = "goto %1";
            }
            blnTokensSet = true;
        }
        #endregion
    }
}
