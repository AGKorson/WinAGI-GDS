using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.FanLogicCompiler;
using static WinAGI.Engine.LogicErrorLevel;

namespace WinAGI.Engine {
    /// <summary>
    /// This class contains all the members and methods needed to decode logic 
    /// resources into readable source code.
    /// </summary>
    public static class LogicDecoder {
        #region Structs
        internal class DecodeBlockType {
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
        private static AGIGame decompGame;
        private static ReservedDefineList reservedList;
        private static List<DecodeBlockType> DecodeBlock = [];
        private static int pos;
        private static List<int> LabelPos = [];
        private static int msgSecStart;
        private static List<string> MsgList;
        private static bool[] MsgUsed = new bool[256];
        private static bool[] MsgExists = new bool[256];
        private static List<string> outputList = [];
        private static bool checkQuit = false;
        private static bool badQuit = false;
        private static byte indentSize = 4;
        internal static string defSrcExt = ".lgc";
        private static AGICodeStyle codeStyle = AGICodeStyle.cstDefaultStyle;
        private static bool useSierraSyntax = false;
        private const int MAX_LINE_LEN = 80;
        // tokens for building source code output
        private static bool blnTokensSet;
        internal static string INDENT;
        private static string D_TKN_NOT;
        private static string D_TKN_IF;
        private static string D_TKN_THEN;
        private static string D_TKN_ELSE;
        private static string D_TKN_ENDIF;
        private static string D_TKN_GOTO;
        private static string D_TKN_EOL;
        private static string D_TKN_AND;
        private static string D_TKN_OR;
        private static string D_TKN_EQUAL;
        private static string D_TKN_NOT_EQUAL;
        private static string D_TKN_COMMENT;
        private static string D_TKN_MESSAGE;
        private static bool minorErrors = false;
        private static bool addWarning;
        private static List<string> warningText = [];
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the number of spaces the logic decoder uses to indent each block level of code.
        /// </summary>
        public static byte IndentSize {
            get {
                // default is 4 spaces
                if (indentSize == 0) {
                    indentSize = 4;
                }
                return indentSize;
            }
            set {
                indentSize = value;
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
            get { return codeStyle; }
            set {
                codeStyle = value;
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
                return defSrcExt;
            }
            set {
                defSrcExt = value;
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
        internal static (string, bool) DecodeLogic(Logic SourceLogic) {
            // converts logic bytecode into decompiled source, converting extended
            // characters to correct encoding
            byte code;
            byte cmdNum = 0;

            // initialize the decoder
            compLogic = SourceLogic;
            Debug.Assert(compLogic.Loaded);
            // clear any existing decode entries from main form warning list
            TWinAGIEventInfo decompclear = new() {
                ResNum = compLogic.Number,
                ResType = AGIResType.Logic,
                Type = EventType.Info,
                Module = compLogic.ID,
                Filename = "",
                InfoType = InfoType.ClearWarnings
            };
            AGIGame.OnDecodeLogicStatus(decompclear);
            minorErrors = false;
            byte[] bytData = compLogic.Data;
            outputList = [];
            // currently only ingame logics can be decoded
            if (compLogic.parent is not null) {
                decompGame = compLogic.parent;
                reservedList = decompGame.ReservedDefines;
                useSierraSyntax = decompGame.agSierraSyntax;
            }
            else {
                decompGame = null;
                reservedList = DefaultReservedDefines;
                useSierraSyntax = false;
            }

            if (!blnTokensSet) {
                InitTokens(CodeStyle);
            }
            outputList.Add("[*********************************************************************");
            outputList.Add("[");
            outputList.Add("[ " + compLogic.ID);
            outputList.Add("[");
            outputList.Add("[*********************************************************************");
            // add standard include files
            if (decompGame is null || decompGame.agIncludeIDs) {
                outputList.Add("#include \"resourceids.txt\"");
            }
            if (decompGame is null || decompGame.agIncludeReserved) {
                outputList.Add("#include \"reserved.txt\"");
            }
            if (decompGame is null || decompGame.agIncludeGlobals) {
                outputList.Add("#include \"globals.txt\"");
            }
            outputList.Add("");
            // minimum length allowed is 7:
            // 2 bytes for message section offset, 
            // 1 byte for message count (zero)
            // 1 byte for return

            // if empty logic just use a single return command
            if (bytData.Length < 4) {
                AddDecodeError("DE01", EngineResources.DE01, outputList.Count - 1);
                AddWarningLines();
                outputList.Add("return();");
                return (string.Join(NEWLINE, [.. outputList]), false);
            }

            // extract messages (add two to offset because the message
            // section start is referenced relative to byte 2 of the
            // resource header)
            msgSecStart = bytData[0] + (bytData[1] << 8) + 2;
            if (!ReadMessages(bytData, msgSecStart, compLogic.V3Compressed != 2)) {
                // error message aded by ReadMessages function
                AddWarningLines();
                outputList.Add("return();");
                return (string.Join(NEWLINE, [.. outputList]), false);
            }

            // reset main block info
            DecodeBlock = [new()];
            DecodeBlock[0].IsIf = false;
            DecodeBlock[0].EndPos = msgSecStart;
            DecodeBlock[0].IsOutside = false;
            DecodeBlock[0].Length = msgSecStart;

            // locate labels, and mark them (this also validates all command bytes to be <=181)
            checkQuit = false;
            if (!FindLabels(bytData)) {
                // check for 'quit' cmd error (can happen if a game that is for a version where
                // quit() uses an argument has an older logic in it that uses a version of quit
                // that has no argument)
                if (checkQuit) {
                    badQuit = true;
                    checkQuit = false;
                    // try again
                    // TODO: this will only work if a single quit() cmd is in the logic;
                    // this whole approach needs refactoring (why isn't it handled within
                    // FindLabels? shouldn't have to call it twice...)
                    if (!FindLabels(bytData)) {
                        // error line added by FindLabels
                        outputList.Add("return();");
                        return (string.Join(NEWLINE, [.. outputList]), false);
                    }
                }
                else {
                    // error line added by FindLabels
                    outputList.Add("return();");
                    return (string.Join(NEWLINE, [.. outputList]), false);
                }
            }
            // reset decoder to beginning of bytecode data
            pos = 2;
            int nextLabel = 0;
            // main decoder loop
            do {
                AddBlockEnds();
                // check for label at this position
                if (LabelPos.Count > 0 && nextLabel < LabelPos.Count && LabelPos[nextLabel] == pos) {
                    if (useSierraSyntax) {
                        outputList.Add(":label" + nextLabel.ToString());
                    }
                    else {
                        outputList.Add("Label" + nextLabel + ":");
                    }
                    nextLabel++;
                }
                code = bytData[pos++];
                switch (code) {
                case 0xFF:
                    // this byte starts an IF statement
                    if (!DecodeIf(bytData)) {
                        outputList.Add("return();");
                        return (string.Join(NEWLINE, [.. outputList]), false);
                    }
                    break;
                case 0xFE:
                    // this byte is a 'goto' or 'else'
                    bool isGoto = false;
                    int tmpBlockLen = 256 * bytData[pos + 1] + bytData[pos];
                    pos += 2;
                    // need to check for negative Value here
                    if (tmpBlockLen > 0x7FFF) {
                        // convert to negative number
                        tmpBlockLen -= 0x10000;
                    }
                    // check for an 'else' statement
                    if ((DecodeBlock[^1].EndPos == pos) && DecodeBlock[^1].IsIf && (DecodeBlock.Count > 1)) {
                        // 'else' block is same level as the associated 'if', but 
                        // block type is no longer an 'if'
                        DecodeBlock[^1].IsIf = false;
                        DecodeBlock[^1].IsOutside = false;
                        // confirm this 'else' block is correctly formed
                        if (tmpBlockLen + pos <= DecodeBlock[^2].EndPos && tmpBlockLen >= 0 && DecodeBlock[^1].Length > 3) {
                            // insert the 'else' command in the desired syntax style
                            outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                            // append else to end of curent if block
                            outputList[^1] += D_TKN_ELSE.Replace(ARG1, NEWLINE + INDENT.MultStr(DecodeBlock.Count - 2));
                            // adjust length and endpos for the 'else' block
                            DecodeBlock[^1].Length = tmpBlockLen;
                            DecodeBlock[^1].EndPos = DecodeBlock[^1].Length + pos;
                        }
                        else {
                            // else won't work; force it to be a goto
                            isGoto = true;
                        }
                    }
                    else {
                        isGoto = true;
                    }
                    // check for a 'goto'
                    if (isGoto) {
                        int labelLoc = tmpBlockLen + pos;
                        // label already verified in FindLabels; add warning if necessary
                        if (labelLoc > msgSecStart - 1) {
                            minorErrors = true;
                            AddDecodeError("DE05", EngineResources.DE05.Replace(
                                ARG2, pos.ToString()), outputList.Count - 1);
                            // adjust it to end of resource
                            labelLoc = msgSecStart - 1;
                        }
                        for (int i = 0; i < LabelPos.Count; i++) {
                            if (LabelPos[i] == labelLoc) {
                                outputList.Add(INDENT.MultStr(DecodeBlock.Count - 1) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                                AddWarningLines();
                                break;
                            }
                        }
                    }
                    break;
                default:
                    // valid agi command (don't need to check for invalid command number;
                    // they are all validated in FindLabels)
                    if (decompGame is not null && code > ActionCount - 1) {
                        // this command is not expected for the targeted interpreter version
                        AddDecodeWarning("DW09", EngineResources.DW09.Replace(
                            ARG1, ActionCommands[code].Name).Replace(
                            ARG2, pos.ToString()).Replace(
                            ARG3, decompGame.agIntVersion.ToString()), outputList.Count - 1);
                    }
                    cmdNum = code;
                    string strCurrentLine = INDENT.MultStr(DecodeBlock.Count - 1);
                    if (SpecialSyntax && (cmdNum >= 0x1 && cmdNum <= 0xB) || (cmdNum >= 0xA5 && cmdNum <= 0xA8)) {
                        strCurrentLine += AddSpecialCmd(bytData, cmdNum);
                    }
                    else {
                        strCurrentLine += ActionCommands[cmdNum].Name + "(";
                        int argStart = strCurrentLine.Length;
                        for (int argpos = 0; argpos < ActionCommands[cmdNum].ArgType.Length; argpos++) {
                            code = bytData[pos++];
                            string argText = ArgValue(code, ActionCommands[cmdNum].ArgType[argpos]);
                            if (ReservedAsText && decompGame is not null &&
                                decompGame.agIncludeReserved && SourceLogic.InGame) {
                                // some commands use resources as arguments; substitute as appropriate
                                switch (cmdNum) {
                                case 18:
                                    // new.room - only arg is a logic
                                    if (decompGame.agLogs.Contains(code)) {
                                        argText = decompGame.agLogs[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                           ARG1, "Logic " + code.ToString()).Replace(
                                           ARG2, pos.ToString()).Replace(
                                           ARG3, "new.room"), outputList.Count - 1);
                                    }
                                    break;
                                case 20:
                                    // load.logics - only arg is a logic
                                    if (decompGame.agLogs.Contains(code)) {
                                        argText = decompGame.agLogs[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                           ARG1, "Logic " + code.ToString()).Replace(
                                           ARG2, pos.ToString()).Replace(
                                           ARG3, "load.logics"), outputList.Count - 1);
                                    }
                                    break;
                                case 22:
                                    // call - only arg is a logic
                                    if (decompGame.agLogs.Contains(code)) {
                                        argText = decompGame.agLogs[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                           ARG1, "Logic " + code.ToString()).Replace(
                                           ARG2, pos.ToString()).Replace(
                                           ARG3, "call"), outputList.Count - 1);
                                    }
                                    break;
                                case 30:
                                    // load.view - only arg is a view
                                    if (decompGame.agViews.Contains(code)) {
                                        argText = decompGame.agViews[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                            ARG1, "View " + code.ToString()).Replace(
                                            ARG2, pos.ToString()).Replace(
                                            ARG3, "load.view"), outputList.Count - 1);
                                    }
                                    break;
                                case 32:
                                    // discard.view - only arg is a view
                                    if (decompGame.agViews.Contains(code)) {
                                        argText = decompGame.agViews[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                            ARG1, "View " + code.ToString()).Replace(
                                            ARG2, pos.ToString()).Replace(
                                            ARG3, "discard.view"), outputList.Count - 1);
                                    }
                                    break;
                                case 41:
                                    // set.view - 2nd arg is a view
                                    if (argpos == 1) {
                                        if (decompGame.agViews.Contains(code)) {
                                            argText = decompGame.agViews[code].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                                ARG1, "View " + code.ToString()).Replace(
                                                ARG2, pos.ToString()).Replace(
                                                ARG3, "set.view"), outputList.Count - 1);
                                        }
                                    }
                                    break;
                                case 98:
                                    // load.sound - only arg is asound
                                    if (decompGame.agSnds.Contains(code)) {
                                        argText = decompGame.agSnds[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                           ARG1, "Sound " + code.ToString()).Replace(
                                           ARG2, pos.ToString()).Replace(
                                           ARG3, "load.sound"), outputList.Count - 1);
                                    }
                                    break;
                                case 99:
                                    // sound = 1st arg is a sound
                                    if (argpos == 0) {
                                        if (decompGame.agSnds.Contains(code)) {
                                            argText = decompGame.agSnds[code].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                               ARG1, "Sound " + code.ToString()).Replace(
                                               ARG2, pos.ToString()).Replace(
                                               ARG3, "sound"), outputList.Count - 1);
                                        }
                                    }
                                    break;
                                case 122:
                                    // add.to.pic - 1st arg is a view
                                    if (argpos == 0) {
                                        if (decompGame.agViews.Contains(code)) {
                                            argText = decompGame.agViews[code].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                                ARG1, "View " + code.ToString()).Replace(
                                                ARG2, pos.ToString()).Replace(
                                                ARG3, "add.to.pic"), outputList.Count - 1);
                                        }
                                    }
                                    break;
                                case 129:
                                    // show.obj - only arg is a view
                                    if (decompGame.agViews.Contains(code)) {
                                        argText = decompGame.agViews[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                            ARG1, "View " + code.ToString()).Replace(
                                            ARG2, pos.ToString()).Replace(
                                            ARG3, "show.obj"), outputList.Count - 1);
                                    }
                                    break;
                                case 150:
                                    // trace.info - 1st arg is a logic
                                    if (argpos == 0) {
                                        if (decompGame.agLogs.Contains(code)) {
                                            argText = decompGame.agLogs[code].ID;
                                        }
                                        else {
                                            AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                               ARG1, "Logic " + code.ToString()).Replace(
                                               ARG2, pos.ToString()).Replace(
                                               ARG3, "trace.info"), outputList.Count - 1);
                                        }
                                    }
                                    break;
                                case 175:
                                    // discard.sound - only arg is a sound
                                    if (decompGame.agSnds.Contains(code)) {
                                        argText = decompGame.agSnds[code].ID;
                                    }
                                    else {
                                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                           ARG1, "Sound " + code.ToString()).Replace(
                                           ARG2, pos.ToString()).Replace(
                                           ARG3, "discard.sound"), outputList.Count - 1);
                                    }
                                    break;
                                }
                            }
                            // check for commands that use colors here
                            if (ReservedAsText) {
                                switch (cmdNum) {
                                case 105:
                                    // clear.lines - 3rd arg
                                    if (argpos == 2) {
                                        if (argText.IntVal() < 16) {
                                            argText = reservedList.ColorNames[argText.IntVal()].Name;
                                        }
                                    }
                                    break;
                                case 109:
                                    // set.text.attribute - all args
                                    if (argText.IntVal() < 16) {
                                        argText = reservedList.ColorNames[argText.IntVal()].Name;
                                    }
                                    break;
                                case 154:
                                    // clear.text.rect - 5th arg
                                    if (argpos == 4) {
                                        if (argText.IntVal() < 16) {
                                            argText = reservedList.ColorNames[argText.IntVal()].Name;
                                        }
                                    }
                                    break;
                                }
                            }
                            if (ActionCommands[cmdNum].ArgType[argpos] == Msg) {
                                // split long messages over additional lines
                                do {
                                    if (strCurrentLine.Length + argText.Length > MAX_LINE_LEN) {
                                        int charCount = MAX_LINE_LEN - strCurrentLine.Length;
                                        // determine longest available section of message that can be added
                                        // without exceeding max line length
                                        if (charCount > 1) {
                                            while (charCount > 1 && argText[charCount - 1] != ' ') {
                                                charCount--;
                                            }
                                            // if no space is found to split up the line
                                            if (charCount <= 1) {
                                                // just split it without worrying about a space
                                                charCount = MAX_LINE_LEN - strCurrentLine.Length;
                                            }
                                            // add the section of the message that fits on this line
                                            strCurrentLine = strCurrentLine + argText.Left(charCount) + QUOTECHAR;
                                            argText = argText.Mid(charCount, argText.Length - charCount);
                                            outputList.Add(strCurrentLine);
                                            // create indent (but don't exceed 20 spaces (to ensure msgs aren't split
                                            // up into absurdly small chunks)
                                            if (argStart >= MAX_LINE_LEN - 20) {
                                                argStart = MAX_LINE_LEN - 20;
                                            }
                                            strCurrentLine = " ".MultStr(argStart) + QUOTECHAR;
                                        }
                                        else {
                                            // line is messed up; just add it
                                            strCurrentLine += argText;
                                            argText = "";
                                        }
                                    }
                                    else {
                                        // not too long; add the message to current line
                                        strCurrentLine += argText;
                                        argText = "";
                                    }
                                }
                                // continue adding new lines until entire message is split and added
                                while (argText.Length > 0);
                            }
                            else {
                                // check for quit() arg count error
                                if (cmdNum == 134 && badQuit) {
                                    AddDecodeWarning("DW11", EngineResources.DW11.Replace(
                                        ARG2, pos.ToString()), outputList.Count - 1);
                                    // reset position index so it gets next byte correctly
                                    pos--;
                                }
                                else {
                                    strCurrentLine += argText;
                                }
                            }
                            if (argpos < ActionCommands[cmdNum].ArgType.Length - 1) {
                                strCurrentLine += ", ";
                            }
                        }
                        strCurrentLine += ")";
                    }
                    // check for set.game.id
                    if (cmdNum == 143) {
                        // use this as suggested gameid
                        DecodeGameID = MsgList[code][1..^1];
                    }
                    strCurrentLine += D_TKN_EOL;
                    outputList.Add(strCurrentLine);
                    AddWarningLines();
                    break;
                }
            }
            while (pos < msgSecStart);
            // add any remaining block ends
            AddBlockEnds();
            // confirm logic ends with return
            if (cmdNum != 0) {
                AddDecodeWarning("DW12", EngineResources.DW12, outputList.Count - 1);
                // last warning to add
                AddWarningLines();
            }
            // include a blank line at end of code
            outputList.Add("");
            // add message declaration lines
            DisplayMessages(outputList);
            // done
            return (string.Join(NEWLINE, [.. outputList]), !minorErrors);
        }

        private static void AddWarningLines() {
            if (addWarning) {
                for (int i = 0; i < warningText.Count; i++) {
                    outputList.Add(D_TKN_COMMENT + warningText[i]);
                }
                // reset warning
                addWarning = false;
                warningText = [];
            }
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
                ResNum = compLogic.Number,
                ResType = AGIResType.Logic,
                Type = EventType.DecompWarning,
                ID = WarnID,
                Module = compLogic.ID,
                Filename = "",
                Text = WarningText,
                Line = LineNum.ToString(),
            };
            AGIGame.OnDecodeLogicStatus(dcWarnInfo);
            if (!addWarning) {
                addWarning = true;
            }
            warningText.Add("WARNING " + WarnID + ": " + WarningText);
        }

        static void AddDecodeError(string errID, string errText, int LineNum) {
            TWinAGIEventInfo dcErrInfo = new() {
                ResNum = compLogic.Number,
                ResType = AGIResType.Logic,
                Type = EventType.DecompError,
                ID = errID,
                Module = compLogic.ID,
                Filename = "",
                Text = errText,
                Line = LineNum.ToString(),
            };
            AGIGame.OnDecodeLogicStatus(dcErrInfo);
            if (!addWarning) {
                addWarning = true;
            }
            warningText.Add("ERROR: " + errID + ": " + errText);
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
                        return reservedList.EdgeCodes[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 6:
                    // v6 uses direction codes
                    if (ArgNum <= 8) {
                        return reservedList.ObjDirections[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 20:
                    // v20 uses computer type codes
                    if (ArgNum <= 8) {
                        return reservedList.ComputerTypes[ArgNum].Name;
                    }
                    else {
                        return ArgNum.ToString();
                    }
                case 26:
                    // v26 uses video mode codes
                    if (ArgNum <= 4) {
                        return reservedList.VideoModes[ArgNum].Name;
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
                    return reservedList.ReservedVariables[ArgNum].Name;
                }
                else {
                    return 'v' + ArgNum.ToString();
                }
            case Flag:
                if (ArgNum <= 16) {
                    return reservedList.ReservedFlags[ArgNum].Name;
                }
                else if (ArgNum == 20 &&
                    (decompGame is null) ||
                    (decompGame is not null && (double.Parse(decompGame.agIntVersion) >= 3.002102))) {
                    return reservedList.ReservedFlags[17].Name;
                }
                else {
                    // not a reserved data type
                    return 'f' + ArgNum.ToString();
                }
            case Msg:
                MsgUsed[ArgNum] = true;
                if (MsgExists[ArgNum]) {
                    if (MsgsByNumber) {
                        return 'm' + ArgNum.ToString();
                    }
                    else {
                        return MsgList[ArgNum];
                    }
                }
                else {
                    // message doesn't exist; raise a warning
                    AddDecodeWarning("DW01", EngineResources.DW01.Replace(
                        ARG1, ArgNum.ToString()).Replace(
                        ARG2, pos.ToString()), outputList.Count - 1);
                    return 'm' + ArgNum.ToString();
                }
            case SObj:
                if (ArgNum == 0) {
                    return reservedList.ReservedObjects[0].Name;
                }
                else {
                    return 'o' + ArgNum.ToString();
                }
            case InvItem:
                if (decompGame is not null && !IObjsByNumber) {
                    Debug.Assert(decompGame.agInvObj.Loaded);
                    if (ArgNum < decompGame.agInvObj.Count) {
                        if (decompGame.agInvObj[ArgNum].Unique) {
                            if (decompGame.agInvObj[ArgNum].ItemName == "?") {
                                // use the inventory item number, and post a warning
                                AddDecodeWarning("DW04", EngineResources.DW04.Replace(
                                    ARG2, pos.ToString()), outputList.Count - 1);
                                return 'i' + ArgNum.ToString();
                            }
                            else {
                                // a unique, non-questionmark item- use it's string Value
                                return QUOTECHAR + decompGame.agInvObj[ArgNum].ItemName.Replace(QUOTECHAR.ToString(), "\\\"") + QUOTECHAR;
                            }
                        }
                        else {
                            // non-unique - use obj number instead
                            if (ErrorLevel == Medium) {
                                AddDecodeWarning("DW05", EngineResources.DW05.Replace(
                                    ARG1, decompGame.agInvObj[ArgNum].ItemName).Replace(
                                    ARG2, pos.ToString()), outputList.Count - 1);
                            }
                            return 'i' + ArgNum.ToString();
                        }
                    }
                    else {
                        AddDecodeWarning("DW03", EngineResources.DW03.Replace(
                            ARG1, ArgNum.ToString()).Replace(
                            ARG2, pos.ToString()), outputList.Count - 1);
                        return 'i' + ArgNum.ToString();
                    }
                }
                else {
                    // always refer to the object by number if not in a game
                    return 'i' + ArgNum.ToString();
                }
            case ArgType.Str:
                if (ArgNum == 0) {
                    return reservedList.ReservedStrings[0].Name;
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

            // can't get here, but compiler demands a return statement
            return "";
        }

        /// <summary>
        /// This method extracts message text from the end of the logic resource.
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="msgStart"></param>
        /// <param name="Decrypt"></param>
        /// <returns>true if messages extracted successfully, false if any errors 
        /// occur which prevent decooding the logic</returns>
        static bool ReadMessages(byte[] bytData, int msgStart, bool Decrypt) {
            int[] MessageStart = new int[256];

            // validate msg start
            if (msgStart >= bytData.Length) {
                // invalid logic
                AddDecodeError("DE09", EngineResources.DE09, outputList.Count - 1);
                return false;
            }
            pos = msgStart;
            MsgList = [];
            // first msg, with index of zero, is null/not used:
            // There is no message 0 (it is not supported by the file format). the
            // two bytes which correspond to message 0 offset is used to hold the
            // end of text ptr so AGI can decrypt the message text when the logic
            // is initially loaded
            MsgList.Add("");
            int NumMessages = bytData[pos++];
            if (NumMessages > 0) {
                // calculate end of message pointer
                int textEnd = msgStart + 256 * bytData[pos + 1] + bytData[pos];
                pos += 2;
                if (textEnd != bytData.Length - 1) {
                    minorErrors = true;
                    AddDecodeError("DE07", EngineResources.DE07, outputList.Count - 1);
                    // adjust it to end
                    textEnd = bytData.Length - 1;
                }
                // loop through all messages, extract offset
                for (int i = 1; i <= NumMessages; i++) {
                    // set start of this msg as start of msg block, plus offset, plus one (for byte which gives number of msgs)
                    MessageStart[i] = 256 * bytData[pos + 1] + bytData[pos] + msgStart + 1;
                    // validate msg start
                    if (MessageStart[i] >= bytData.Length) {
                        minorErrors = true;
                        AddDecodeError("DE08", EngineResources.DE08.Replace(
                            ARG1, i.ToString()).Replace(
                            ARG2, pos.ToString()), outputList.Count - 1);
                        MessageStart[i] = 0;
                    }
                    pos += 2;

                }
                // decrypt the entire message section, if needed
                int textStart = pos;
                if (Decrypt) {
                    for (int i = pos; i < textEnd; i++) {
                        bytData[i] ^= bytEncryptKey[(i - textStart) % 11];
                    }
                }
                // now read all messages
                for (int i = 1; i <= NumMessages; i++) {
                    List<byte> bMsgText = [];
                    // if msg start points to a valid msg
                    if (MessageStart[i] > 0 && MessageStart[i] >= textStart) {
                        pos = MessageStart[i];
                        bool endOfMsg = false;
                        do {
                            byte bytInput = bytData[pos];
                            pos++;
                            if ((bytInput == 0) || (pos >= bytData.Length)) {
                                endOfMsg = true;
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
                        while (!endOfMsg);
                        if (bMsgText.Count == 0) {
                            MsgList.Add("\"\"");
                        }
                        else {
                            // convert to correct codepage
                            MsgList.Add(QUOTECHAR + Encoding.GetEncoding(compLogic.CodePage).GetString(bMsgText.ToArray()) + QUOTECHAR);
                        }
                        MsgExists[i] = true;
                    }
                    else {
                        // add a null message (so numbers work out)
                        MsgList.Add("");
                        MsgExists[i] = false;
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
        static bool DecodeIf(byte[] bytData) {
            bool firstCmd = true;
            bool inOrBlock = false;
            bool ifFinished = false;
            string lineText = INDENT.MultStr(DecodeBlock.Count - 1) + D_TKN_IF;

            do {
                // always reset 'NOT' block status to false
                bool inNotBlock = false;
                byte curByte = bytData[pos++];
                // first, check for an 'or'
                if (curByte == 0xFC) {
                    inOrBlock = !inOrBlock;
                    if (inOrBlock) {
                        if (!firstCmd) {
                            lineText += D_TKN_AND;
                            outputList.Add(lineText);
                            lineText = INDENT.MultStr(DecodeBlock.Count - 1) + "    ";
                            firstCmd = true;
                        }
                        lineText += "(";
                    }
                    else {
                        lineText += ")";
                    }
                    curByte = bytData[pos++];
                }
                // special check needed in case two 0xFCs are in a row, e.g. (a || b) && (c || d)
                if ((curByte == 0xFC) && (!inOrBlock)) {
                    lineText += D_TKN_AND;
                    outputList.Add(lineText);
                    lineText = INDENT.MultStr(DecodeBlock.Count - 1) + "    ";
                    firstCmd = true;
                    lineText += "(";
                    inOrBlock = true;
                    curByte = bytData[pos++];
                }
                // check for 'not' command
                if (curByte == 0xFD) {
                    inNotBlock = true;
                    curByte = bytData[pos++];
                }
                // check for valid test command
                if ((curByte > 0) && (curByte <= TestCount)) {
                    if (!firstCmd) {
                        if (inOrBlock) {
                            lineText += D_TKN_OR;
                        }
                        else {
                            lineText += D_TKN_AND;
                        }
                        outputList.Add(lineText);
                        lineText = INDENT.MultStr(DecodeBlock.Count - 1) + "    ";
                    }
                    byte bytCmd = curByte;
                    if (SpecialSyntax && (bytCmd >= 1 && bytCmd <= 6)) {
                        curByte = bytData[pos++];
                        byte arg2Val = bytData[pos++];
                        lineText += AddSpecialIf(bytCmd, curByte, arg2Val, inNotBlock);
                    }
                    else {
                        if (inNotBlock) {
                            lineText += D_TKN_NOT;
                        }
                        lineText = lineText + TestCommands[bytCmd].Name + "(";
                        if (bytCmd == 14) {
                            // said command
                            byte argcount = bytData[pos++];
                            for (int argpos = 1; argpos <= argcount; argpos++) {
                                int groupNum = 256 * bytData[pos + 1] + bytData[pos];
                                pos += 2;
                                if (!WordsByNumber && decompGame is not null) {
                                    if (decompGame.agVocabWords.GroupExists(groupNum)) {
                                        if (decompGame.agVocabWords.GroupByNumber(groupNum).WordCount > 0) {
                                            if (decompGame.agSierraSyntax) {
                                                lineText += QUOTECHAR + decompGame.agVocabWords.GroupByNumber(groupNum).GroupName.Replace(' ', '$');
                                            }
                                            else {
                                                lineText += QUOTECHAR + decompGame.agVocabWords.GroupByNumber(groupNum).GroupName + QUOTECHAR;
                                            }
                                        }
                                        else {
                                            // rare, but if group 0, 1, or 9999, it's possible
                                            // to have a group without any words
                                            lineText += groupNum;
                                        }
                                    }
                                    else {
                                        // add the word by its group number
                                        lineText += groupNum;
                                        AddDecodeWarning("DW02", EngineResources.DW02.Replace(
                                            ARG1, groupNum.ToString()).Replace(
                                            ARG2, pos.ToString()), outputList.Count - 1);
                                    }
                                }
                                else {
                                    lineText += groupNum;
                                }

                                if (argpos < argcount) {
                                    lineText += ", ";
                                }
                            }

                        }
                        else {
                            if (TestCommands[bytCmd].ArgType.Length > 0) {
                                for (int argpos = 0; argpos < TestCommands[bytCmd].ArgType.Length; argpos++) {
                                    curByte = bytData[pos++];
                                    lineText += ArgValue(curByte, TestCommands[bytCmd].ArgType[argpos]);
                                    if (argpos < TestCommands[bytCmd].ArgType.Length - 1) {
                                        lineText += ", ";
                                    }
                                }
                            }
                        }
                        lineText += ")";
                    }
                    firstCmd = false;
                    if (bytCmd == 19) {
                        // add warning if this is the unknown test19 command
                        AddDecodeWarning("DW06", EngineResources.DW06.Replace(
                            ARG2, pos.ToString()), outputList.Count - 1);
                    }
                }
                else if (curByte == 0xFF) {
                    // done with if block; add 'then'
                    lineText += D_TKN_THEN.Replace(ARG1, NEWLINE + INDENT.MultStr(DecodeBlock.Count));
                    DecodeBlock.Add(new());
                    DecodeBlock[^1].IsIf = true;
                    DecodeBlock[^1].Length = 256 * bytData[pos + 1] + bytData[pos];
                    pos += 2;
                    if (DecodeBlock[^1].Length == 0) {
                        AddDecodeWarning("DW07", EngineResources.DW07.Replace(
                            ARG2, pos.ToString()), outputList.Count - 1);
                    }
                    // validate end pos
                    DecodeBlock[^1].EndPos = DecodeBlock[^1].Length + pos;
                    if (DecodeBlock[^1].EndPos >= bytData.Length - 1) {
                        minorErrors = true;
                        // adjust to end
                        DecodeBlock[^1].EndPos = bytData.Length - 2;
                        AddDecodeError("DE04", EngineResources.DE04.Replace(
                            ARG1, pos.ToString()), outputList.Count - 1);
                    }
                    // verify block ends before end of previous block
                    // (i.e. it's properly nested)
                    if (DecodeBlock[^1].EndPos > DecodeBlock[^2].EndPos) {
                        // block is outside the previous block nest;
                        // this is an abnormal situation so we need to simulate this block
                        // by using 'else' and 'goto'
                        DecodeBlock[^1].IsOutside = true;
                        DecodeBlock[^1].JumpPos = DecodeBlock[^1].EndPos;
                        DecodeBlock[^1].EndPos = DecodeBlock[^2].EndPos;
                        AddDecodeWarning("DW09", EngineResources.DW08.Replace(
                            ARG1, DecodeBlock[^1].JumpPos.ToString()).Replace(
                            ARG2, pos.ToString()), outputList.Count - 1);
                    }
                    outputList.Add(lineText);
                    AddWarningLines();
                    lineText = INDENT.MultStr(DecodeBlock.Count - 1);
                    ifFinished = true;
                }
                else {
                    // NOT possible- it's handled as an error in SkipToEndIf
                    Debug.Assert(false);
                    return false;
                }
            }
            while (!ifFinished);
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

            do {
                CurByte = bytData[pos++];
                if (CurByte == 0xFC) {
                    CurByte = bytData[pos++];
                }
                if (CurByte == 0xFC) {
                    // two 0xFCs in a row, e.g. (a || b) && (c || d)
                    CurByte = bytData[pos++];
                }
                if (CurByte == 0xFD) {
                    CurByte = bytData[pos++];
                }

                if ((CurByte > 0) && (CurByte <= TestCount)) {
                    ThisCommand = CurByte;
                    if (ThisCommand == 14) {
                        // said command
                        NumSaidArgs = bytData[pos++];
                        // move pointer to next position past these arguments
                        // (words use two bytes per argument, not one)
                        pos += NumSaidArgs * 2;
                    }
                    else {
                        // move pointer to next position past the arguments for this command
                        pos += TestCommands[ThisCommand].ArgType.Length;
                    }
                }
                else if (CurByte == 0xFF) {
                    // increment block counter
                    DecodeBlock.Add(new());
                    DecodeBlock[^1].IsIf = true;
                    DecodeBlock[^1].Length = 256 * bytData[pos + 1] + bytData[pos];
                    pos += 2;
                    // block length of zero will cause warning in main loop, so no need to check for it here
                    DecodeBlock[^1].EndPos = DecodeBlock[^1].Length + pos;
                    if (DecodeBlock[^1].EndPos > DecodeBlock[^2].EndPos) {
                        // block is outside the previous block nest;
                        // this is an abnormal situation so we need to simulate this block by using
                        // else and goto
                        // (warning will be added in main decompiler loop)
                        DecodeBlock[^1].IsOutside = true;
                        DecodeBlock[^1].JumpPos = DecodeBlock[^1].EndPos;
                        DecodeBlock[^1].EndPos = DecodeBlock[^2].EndPos;
                        // add a new goto item (dont need to worry about an invalid jumppos)
                        // check fo existing label at jump pos
                        // if loop exited normally (i will equal bytLabelCount+1)
                        if (!LabelPos.Contains(DecodeBlock[^1].JumpPos)) {
                            // add new label at jump pos
                            LabelPos.Add(DecodeBlock[^1].JumpPos);
                        }
                    }
                    // end of block
                    return true;
                }
                else {
                    AddDecodeError("DE02", EngineResources.DE02.Replace(
                        ARG1, CurByte.ToString()).Replace(
                        ARG2, pos.ToString()), outputList.Count - 1);
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
            byte curByte;
            int tmpBlockLength;
            bool DoGoto;
            int LabelLoc;

            LabelPos = [];
            pos = 2;
            do {
                // check for end of a block (start at most recent block and work up
                // to oldest block)
                for (int i = DecodeBlock.Count - 1; i > 0; i--) {
                    if (DecodeBlock[i].EndPos <= pos) {
                        // if off by exactly one, AND there's a quit cmd in this block
                        // AND this version is one that uses arg value for quit
                        // this error is most likely due to bad coding of quit cmd
                        // if otherwise not an exact match, it will be caught when the block ends are added
                        if (pos - DecodeBlock[i].EndPos == 1 &&
                            decompGame is not null &&
                            decompGame.agIntVersion != "2.089" &&
                            DecodeBlock[i].HasQuit) {
                            checkQuit = true;
                            return false;
                        }
                        // take this block off stack
                        DecodeBlock.RemoveAt(DecodeBlock.Count - 1);
                    }
                }
                curByte = bytData[pos++];
                switch (curByte) {
                case 0xFF:
                    // start of an IF statement
                    if (!SkipToEndIf(bytData)) {
                        // major error - error message added by SkipToEndIf
                        return false;
                    }
                    break;
                case 0xFE:
                    // GOTO command
                    // reset goto status flag
                    DoGoto = false;
                    tmpBlockLength = 256 * bytData[pos + 1] + bytData[pos];
                    pos += 2;
                    // check for negative value
                    if (tmpBlockLength > 0x7FFF) {
                        tmpBlockLength -= 0x10000;
                    }
                    // check to see if this 'goto' might be an 'else':
                    //  - end of this block matches this position (the if-then part is done)
                    //  - this block is identified as an IF block
                    //  - this is NOT the main block
                    //  - the flag to set elses as gotos is turned off
                    if ((DecodeBlock[^1].EndPos == pos) && DecodeBlock[^1].IsIf && (DecodeBlock.Count > 1)) {
                        // the 'else' block re-uses same level as the 'if'
                        DecodeBlock[^1].IsIf = false;
                        DecodeBlock[^1].IsOutside = false;
                        // confirm the 'else' block is formed correctly:
                        //  - the end of this block doen't go past where the 'if' block ended
                        //  - the block is not negative (means jumping backward, so it MUST be a goto)
                        //  - length of block has enough room for code necessary to close the 'else'
                        if ((tmpBlockLength + pos > DecodeBlock[^2].EndPos) ||
                            (tmpBlockLength < 0) || (DecodeBlock[^1].Length <= 3)) {
                            // this is a 'goto' statement
                            DoGoto = true;
                        }
                        else {
                            // this is an 'else' block - readjust block end so the IF statement that
                            // owns this 'else' is ended correctly
                            DecodeBlock[^1].Length = tmpBlockLength;
                            DecodeBlock[^1].EndPos = DecodeBlock[^1].Length + pos;
                        }
                    }
                    else {
                        // this is a goto statement (or an else statement while ElseAsGoto flag is true)
                        DoGoto = true;
                    }
                    // goto
                    if (DoGoto) {
                        LabelLoc = tmpBlockLength + pos;
                        // don't need to check for invalid destination because it's checked in main decode loop
                        if (!LabelPos.Contains(LabelLoc)) {
                            // add a new label location
                            LabelPos.Add(LabelLoc);
                        }
                    }
                    break;
                case <=  MAX_CMDS:
                    // AGI command
                    // skip over arguments to get next command
                    // (even if it's not valid for this version, just account for it here;
                    // the main decoder loop will handle the warning/error for commands 
                    // that are not valid in this version)
                    pos += ActionCommands[curByte].ArgType.Length;
                    // check for quit command
                    if (curByte == 134) {
                        DecodeBlock[^1].HasQuit = true;
                        if (badQuit) {
                            // if attempting to fix a bad quit command error, adjust position
                            // backward to account for the skipped quit command argument
                            pos--;
                        }
                    }
                    break;
                default:
                    // unknown action command - major error
                    AddDecodeError("DE03", EngineResources.DE03.Replace(
                        ARG1, curByte.ToString()).Replace(
                        ARG2, pos.ToString()), outputList.Count - 1);
                    return false;
                }
            }
            while (pos < msgSecStart);
            // sort labels, if found
            for (int i = 0; i < LabelPos.Count; i++) {
                for (int j = i + 1; j < LabelPos.Count; j++) {
                    if (LabelPos[j] < LabelPos[i]) {
                        // swap
                        (LabelPos[i], LabelPos[j]) = (LabelPos[j], LabelPos[i]);
                    }
                }
            }
            // clear block info (BUT don't overwrite main block)
            while (DecodeBlock.Count > 1) {
                DecodeBlock.RemoveAt(1);
            }
            return true;
        }

        /// <summary>
        /// This method adds the message declarations to end of the source code output.
        /// </summary>
        /// <param name="stlOut"></param>
        static void DisplayMessages(List<string> stlOut) {
            int lngMsg;
            stlOut.Add(D_TKN_COMMENT + "DECLARED MESSAGES");
            for (lngMsg = 1; lngMsg < MsgList.Count; lngMsg++) {
                if (MsgExists[lngMsg] && (ShowAllMessages || !MsgUsed[lngMsg])) {
                    stlOut.Add(D_TKN_MESSAGE.Replace(ARG1, lngMsg.ToString()).Replace(ARG2, MsgList[lngMsg]));
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

            bytArg1 = bytData[pos];
            pos++;
            switch (bytCmd) {
            case 0x1:
                // increment
                return "++" + ArgValue(bytArg1, Var);
            case 0x2:
                // decrement
                return "--" + ArgValue(bytArg1, Var);
            case 0x3:
                // assignn
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Num, bytArg1);
            case 0x4:
                // assignv
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Var);
            case 0x5:
                // addn
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + "  += " + ArgValue(bytArg2, Num);
            case 0x6:
                // addv
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + "  += " + ArgValue(bytArg2, Var);
            case 0x7:
                // subn
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " -= " + ArgValue(bytArg2, Num);
            case 0x8:
                // subv
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " -= " + ArgValue(bytArg2, Var);
            case 0x9:
                // lindirectv
                bytArg2 = bytData[pos++];
                return "*" + ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Var);
            case 0xA:
                // rindirect
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " = *" + ArgValue(bytArg2, Var);
            case 0xB:
                // lindirectn
                bytArg2 = bytData[pos++];
                return "*" + ArgValue(bytArg1, Var) + " = " + ArgValue(bytArg2, Num);
            case 0xA5:
                // mul.n
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " *= " + ArgValue(bytArg2, Num);
            case 0xA6:
                // mul.v
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " *= " + ArgValue(bytArg2, Var);
            case 0xA7:
                // div.n
                bytArg2 = bytData[pos++];
                return ArgValue(bytArg1, Var) + " /= " + ArgValue(bytArg2, Num);
            case 0xA8:
                // div.v
                bytArg2 = bytData[pos++];
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
        /// <param name="outputList"></param>
        static void AddBlockEnds() {
            for (int CurBlock = DecodeBlock.Count - 1; CurBlock > 0; CurBlock--) {
                // in some rare cases, the blocks don't align correctly
                if (DecodeBlock[CurBlock].EndPos < pos) {
                    minorErrors = true;
                    AddDecodeError("DE06", EngineResources.DE06.Replace(
                        ARG2, pos.ToString()), outputList.Count - 1);
                }
                if (DecodeBlock[CurBlock].EndPos <= pos) {
                    // check for unusual case where an if block ends outside the if block it is nested in
                    if (DecodeBlock[CurBlock].IsOutside) {
                        // end current block
                        outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                        // append else to end of current block
                        outputList[^1] = outputList[^1] + D_TKN_ELSE.Replace(ARG1, NEWLINE + INDENT.MultStr(DecodeBlock.Count - 2));
                        // then add a goto
                        for (int i = 1; i < LabelPos.Count; i++) {
                            if (LabelPos[i] == DecodeBlock[CurBlock].JumpPos) {
                                outputList.Add(INDENT.MultStr(DecodeBlock.Count - 1) + D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL);
                                break;
                            }
                        }
                    }
                    // add end if
                    outputList.Add(INDENT.MultStr(CurBlock - 1) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                    DecodeBlock.RemoveAt(DecodeBlock.Count - 1);
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
                D_TKN_THEN = ")%1{"; // where %1 is a line feed plus indent at current level
                D_TKN_ELSE = "%1" + INDENT + "else%1" + INDENT + "{"; // where %1 is a line feed plus indent at current level
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
            if (decompGame is not null && decompGame.agSierraSyntax) {
                // goto doesn't include parentheses
                D_TKN_GOTO = "goto %1";
            }
            blnTokensSet = true;
        }
        #endregion
    }
}
