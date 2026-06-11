using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.LogicErrorLevel;

namespace WinAGI.Engine {
    /// <summary>
    /// This class contains all the members and methods needed to decode logic 
    /// resources into readable source code using FAN syntax or SIERRA syntax.
    /// </summary>
    public static class LogicDecoder {
        #region Enums
        public enum AGICodeStyle {
            cstDefaultStyle,
            cstAltStyle1,
            cstAltStyle2
        }
        
        public enum DecodeSubtype {
            None,
            EdgeCode,
            ObjDir,
            MachineType,
            MonitorType,
            Colors,
        }
        #endregion

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

        public struct DecodeDefine {
            public ArgType Type;
            public DecodeSubtype SubType; // only numbers use this
            public string Name;
            public int Value;
            public bool NotUsed = false; // used for views and items to comment out when not used
            public ArgType[] ArgList = null; // only cmds use this

            public DecodeDefine(ArgType type, string name, int value) {
                Type = type;
                Name = name;
                Value = value;
            }
        }
        #endregion

        #region Fields
        private static Logic dcLogic;
        private static AGIGame dcGame;
        private static ReservedDefineList reservedList;
        private static List<DecodeBlockType> DecodeBlock = [];
        private static byte[] logicdata;
        private static int pos;
        private static List<int> LabelPos = [];
        private static int msgSecStart;
        private static List<string> MsgList;
        private static readonly bool[] MsgUsed = new bool[256];
        private static readonly bool[] MsgExists = new bool[256];
        private static List<string> outputList = [];
        private static bool checkQuit = false;
        private static bool badQuit = false;
        private static byte indentSize = 4;
        internal static string defSrcExt = ".lgc";
        private static AGICodeStyle codeStyle = AGICodeStyle.cstDefaultStyle;
        private const int MAX_LINE_LEN = 80;
        // tokens for building source code output
        private static bool tokensSet;
        internal static string INDENT;
        private static string D_TKN_NOT;
        private static string D_TKN_IF;
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

        // Sierra syntax support:
        private static bool sierraSyntax = false;
        private static DecodeDefineList sierradefs = null;
        private static string sysdeffile = "", gamedeffile = "";
        public static bool nestedsysdefs = false;
        private static bool wordstokLoaded = false;
        private static bool decompAll = false;
        private static bool hasgamedefs = false;
        // lists of new defines added when decoding all
        private static DecodeDefineList newDefines = null;
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
            get {
                return codeStyle;
            }
            set {
                codeStyle = value;
                // tokens need to be reset
                tokensSet = false;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if decompiled logics displays a list
        /// of all messages in the logic at the end of the code section.
        /// </summary>
        public static bool ShowAllMessages {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value that determines if message arguments in decompiled 
        /// logics are displayed as literal strings or as message argument numbers. 
        /// </summary>
        public static bool MsgsByNumber {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value that determines if inventory item arguments in 
        /// decompiled logics are displayed as literal strngs or as inventory item
        /// argument numbers.
        /// </summary>
        public static bool IObjsByNumber {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value that determines if vocablulary word arguments in 
        /// decompiled logics are displayed as literal strngs or as vocabulary word
        /// argument numbers.
        /// </summary>
        public static bool WordsByNumber {
            get; set;
        }

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
        /// using shorthand syntax for math commands instead of using the full command.
        /// </summary>
        public static bool SpecialSyntax {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value that determines if reserved variables and flag are 
        /// displayed in decompiled logics as define names or as argument numbers.
        /// </summary>
        public static bool ReservedAsText {
            get; set;
        }
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
            int defineline;

            // initialize the decoder
            dcLogic = SourceLogic;
            // clear any existing decode entries from main form warning list
            WinAGIEventInfo decompclear = new() {
                ResNum = dcLogic.Number,
                ResType = AGIResType.Logic,
                Type = EventType.Info,
                Module = dcLogic.ID,
                Filename = "",
                InfoType = InfoType.ClearWarnings
            };
            AGIGame.OnDecodeLogicStatus(decompclear);
            minorErrors = false;
            logicdata = dcLogic.Data;
            outputList = [];
            // set game and syntax settings
            if (decompAll) {
                // sierrasyntax always true when decompiling all
                sierraSyntax = true;
            }
            else {
                if (dcLogic.parent is not null) {
                    // in game- assign parent, and set syntax
                    dcGame = dcLogic.parent;
                    sierraSyntax = dcLogic.parent.SierraSyntax;
                    if (!sierraSyntax) {
                        reservedList = dcGame.ReservedDefines;
                    }
                }
                else {
                    // no game, assume fan syntax
                    dcGame = null;
                    sierraSyntax = false;
                    reservedList = DefaultReservedDefines;
                }
            }
            if (sierraSyntax) {
                if (!decompAll) {
                    // load sysdefs and gamedefs
                    LoadSierraDefines();
                    newDefines = [];
                }
            }
            if (!tokensSet) {
                InitTokens(CodeStyle);
            }
            outputList.Add("[*********************************************************************");
            outputList.Add("[");
            outputList.Add("[ " + dcLogic.ID);
            outputList.Add("[");
            outputList.Add("[*********************************************************************");
            if (sierraSyntax) {
                // add Sierra include files
                if (sysdeffile.Length > 0 && !nestedsysdefs) {
                    outputList.Add("%include \"" + Path.GetFileName(sysdeffile) + "\"");
                }
                if (decompAll || hasgamedefs) {
                    outputList.Add("%include \"" + Path.GetFileName(gamedeffile) + "\"");
                }

            }
            else {
                // add standard include files
                if (dcGame is null || dcGame.agIncludeIDs) {
                    outputList.Add("%include \"resourceids.txt\"");
                }
                if (dcGame is null || dcGame.agIncludeReserved) {
                    outputList.Add("%include \"reserved.txt\"");
                }
                if (dcGame is null || dcGame.agIncludeGlobals) {
                    outputList.Add("%include \"globals.txt\"");
                }
            }
            outputList.Add("");
            defineline = outputList.Count;

            // minimum length allowed is 7:
            // 2 bytes for message section offset, 
            // 1 byte for message count (zero)
            // 1 byte for return

            // if empty logic just use a single return command
            if (logicdata.Length < 4) {
                AddDecodeError("DE01", EngineResources.DE01, outputList.Count - 1);
                outputList.Add("return();");
                return (string.Join(NEWLINE, [.. outputList]), false);
            }

            try {
                // extract messages (add two to offset because the message
                // section start is referenced relative to byte 2 of the
                // resource header)
                msgSecStart = logicdata[0] + (logicdata[1] << 8) + 2;
                if (!ReadMessages(msgSecStart, dcLogic.V3Compressed != 2)) {
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
                if (!FindLabels()) {
                    // check for 'quit' cmd error (can happen if a game that is for a version where
                    // quit() uses an argument has an older logic in it that uses a version of quit
                    // that has no argument)
                    if (checkQuit) {
                        badQuit = true;
                        checkQuit = false;
                        // try again
                        // NOTE: this will only work if a single quit() cmd is in the logic;
                        // maybe this section can be refactored?
                        if (!FindLabels()) {
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
                        if (sierraSyntax) {
                            outputList.Add(":label" + nextLabel);
                        }
                        else {
                            outputList.Add("Label" + nextLabel + ":");
                        }
                        nextLabel++;
                    }
                    code = ReadByte(ref pos);
                    switch (code) {
                    case 0xFF:
                        // this byte starts an IF statement
                        if (!DecodeIf()) {
                            outputList.Add("return();");
                            return (string.Join(NEWLINE, [.. outputList]), false);
                        }
                        break;
                    case 0xFE:
                        // this byte is a 'goto' or 'else'
                        bool isGoto = false;

                        int tmpBlockLen = ReadByte(ref pos) + 256 * ReadByte(ref pos);
                        // need to check for negative Value here
                        if (tmpBlockLen > 0x7FFF) {
                            // convert to negative number
                            tmpBlockLen -= 0x10000;
                        }
                        // check for an 'else' statement
                        if ((DecodeBlock[^1].EndPos == pos) && DecodeBlock[^1].IsIf && (DecodeBlock.Count > 1)) {
                            // the 'else' block re-uses same level as the 'if'
                            DecodeBlock[^1].IsIf = false;
                            DecodeBlock[^1].IsOutside = false;
                            // confirm the 'else' block is formed correctly:
                            //  - the end of this block doen't go past where the 'if' block ended
                            //  - the block is not negative (means jumping backward, so it MUST be a goto)
                            //  - length of block has enough room for code necessary to close the 'else'
                            // SPECIAL CASE: if the goto location is inside a block that is level
                            // 2 or higher, it's actually a goto, not an else
                            // only way to tell is to check the block level at the location
                            if (tmpBlockLen + pos <= DecodeBlock[^2].EndPos &&
                                tmpBlockLen >= 0 && DecodeBlock[^1].Length > 3 &&
                                CheckGotoBlock(tmpBlockLen + pos)) {
                                // insert the 'else' command in the desired syntax style
                                outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                                // append else to end of curent if block
                                switch (CodeStyle) {
                                case AGICodeStyle.cstDefaultStyle:
                                    outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + "else");
                                    outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + INDENT + "{");
                                    break;
                                case AGICodeStyle.cstAltStyle1:
                                    outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + "else {");
                                    break;
                                case AGICodeStyle.cstAltStyle2:
                                    outputList[^1] += " else {";
                                    break;
                                }
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
                                AddDecodeError("DE05", EngineResources.DE05.Replace(
                                    ARG2, pos.ToString()), outputList.Count - 1);
                                // adjust it to end of resource
                                labelLoc = msgSecStart - 1;
                            }
                            for (int i = 0; i < LabelPos.Count; i++) {
                                if (LabelPos[i] == labelLoc) {
                                    string gotoline = INDENT.MultStr(DecodeBlock.Count - 1);
                                    if (sierraSyntax) {
                                        gotoline += D_TKN_GOTO.Replace(ARG1, "label" + i) + D_TKN_EOL;
                                    }
                                    else {
                                        gotoline += D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL;
                                    }
                                    outputList.Add(gotoline);
                                    AddWarningLines();
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        // valid agi command (don't need to check for invalid command number;
                        // they are all validated in FindLabels)
                        if (dcGame is not null && code > ActionCount - 1) {
                            // this command is not expected for the targeted interpreter version
                            AddDecodeWarning("DW09", EngineResources.DW09.Replace(
                                ARG1, ActionCommands[code].FanName).Replace(
                                ARG2, pos.ToString()).Replace(
                                ARG3, dcGame.agIntVersion.VersionString), outputList.Count + 1);
                        }
                        cmdNum = code;
                        string lineText = INDENT.MultStr(DecodeBlock.Count - 1);
                        if (SpecialSyntax && ((cmdNum >= 0x1 && cmdNum <= 0xB) ||
                            (!sierraSyntax && cmdNum >= 0xA5 && cmdNum <= 0xA8))) {
                            lineText += AddSpecialCmd(cmdNum);
                        }
                        else {
                            ArgType[] args;
                            if (sierraSyntax) {
                                GetCmdName(ActionCmd, cmdNum, out args, out string cmdName);
                                lineText += cmdName + "(";
                            }
                            else {
                                lineText += ActionCommands[cmdNum].FanName + "(";
                                args = ActionCommands[cmdNum].ArgList;
                            }
                            int argStart = lineText.Length;
                            for (int argpos = 0; argpos < args.Length; argpos++) {
                                code = ReadByte(ref pos);
                                string argText;
                                if (sierraSyntax) {
                                    argText = SierraArgValue(true, cmdNum, argpos, code, args[argpos]);
                                }
                                else {
                                    argText = FanArgValue(true, cmdNum, argpos, code, args[argpos]);
                                    if (args[argpos] == MsgNum) {
                                        // split long messages over additional lines
                                        do {
                                            if (lineText.Length + argText.Length > MAX_LINE_LEN) {
                                                int charCount = MAX_LINE_LEN - lineText.Length;
                                                // determine longest available section of message that can be added
                                                // without exceeding max line length
                                                if (charCount > 1) {
                                                    while (charCount > 1 && argText[charCount - 1] != ' ') {
                                                        charCount--;
                                                    }
                                                    // if no space is found to split up the line
                                                    if (charCount <= 1) {
                                                        // just split it without worrying about a space
                                                        charCount = MAX_LINE_LEN - lineText.Length;
                                                    }
                                                    // add the section of the message that fits on this line
                                                    lineText = lineText + argText.Left(charCount) + QUOTECHAR;
                                                    argText = argText.Mid(charCount, argText.Length - charCount);
                                                    outputList.Add(lineText);
                                                    // create indent (but don't exceed 20 spaces (to ensure msgs aren't split
                                                    // up into absurdly small chunks)
                                                    if (argStart >= MAX_LINE_LEN - 20) {
                                                        argStart = MAX_LINE_LEN - 20;
                                                    }
                                                    lineText = " ".MultStr(argStart) + QUOTECHAR;
                                                }
                                                else {
                                                    // line is messed up; just add it
                                                    lineText += argText;
                                                    argText = "";
                                                }
                                            }
                                            else {
                                                // not too long; add the message to current line
                                                lineText += argText;
                                                argText = "";
                                            }
                                        }
                                        // continue adding new lines until entire message is split and added
                                        while (argText.Length > 0);
                                    }
                                }
                                // check for quit() arg count error
                                if (cmdNum == 134 && badQuit) {
                                    AddDecodeWarning("DW11", EngineResources.DW11.Replace(
                                        ARG2, pos.ToString()), outputList.Count + 1);
                                    // reset position index so it gets next byte correctly
                                    pos--;
                                    // add force quit
                                    lineText += "1";
                                }
                                else {
                                    lineText += argText;
                                }
                                if (argpos < ActionCommands[cmdNum].ArgList.Length - 1) {
                                    lineText += ", ";
                                }
                            }
                            lineText += ")";
                        }
                        // check for set.game.id
                        if (cmdNum == 143) {
                            // use this as suggested gameid
                            DecodeGameID = MsgList[code][1..^1];
                        }
                        lineText += D_TKN_EOL;
                        outputList.Add(lineText);
                        AddWarningLines();
                        break;
                    }
                }
                while (pos < msgSecStart);
                // add any remaining block ends
                AddBlockEnds();
                // confirm logic ends with return
                if (cmdNum != 0) {
                    AddDecodeWarning("DW12", EngineResources.DW12, outputList.Count + 1);
                    // last warning to add
                    AddWarningLines();
                }
                else {
                    // remove the return if sierrasyntax 
                    // because compiler will add it automatically
                    if (sierraSyntax) {
                        outputList.RemoveAt(outputList.Count - 1);
                    }
                }
            }
            catch (LogicDecodeBufferOverflowException) {
                AddDecodeError("DE10", EngineResources.DE10, outputList.Count - 1);
                outputList.Add("return();");
            }
            // include a blank line at end of code
            outputList.Add("");
            // add message declaration lines
            AddMessages(outputList);
            if (!decompAll) {
                dcGame = null;
            }
            // add extra defines for sierra
            if (sierraSyntax && newDefines.Count > 0) {
                int startline = defineline;
                defineline = AddSierraDefines(defineline);
                // adjust line numbers for any warnings/errors that were generated
                if (defineline - startline != 0) {
                    WinAGIEventInfo dcWarnInfo = new() {
                        ResNum = dcLogic.Number,
                        ResType = AGIResType.Logic,
                        Type = EventType.DecompWarning,
                        ID = "renumber",
                        Line = defineline - startline,
                    };
                    AGIGame.OnDecodeLogicStatus(dcWarnInfo);
                }

            }
            // done
            WinAGIEventInfo dcDoneInfo = new() {
                ResNum = dcLogic.Number,
                ResType = AGIResType.Logic,
                Type = EventType.Info,
                InfoType = InfoType.Decompiled,
            };
            AGIGame.OnDecodeLogicStatus(dcDoneInfo);
            dcLogic = null;
            return (string.Join(NEWLINE, [.. outputList]), !minorErrors);
        }

        private static int AddSierraDefines(int defineline) {
            // sort the defines by type
            defineline = AddDefineByType(ActionCmd, defineline);
            defineline = AddDefineByType(TestCmd, defineline);
            defineline = AddDefineByType(Flag, defineline);
            defineline = AddDefineByType(Var, defineline);
            defineline = AddDefineByType(InvItem, defineline);
            defineline = AddDefineByType(SObj, defineline);
            defineline = AddDefineByType(ArgType.View, defineline);
            defineline = AddDefineByType(Ctrl, defineline);
            defineline = AddDefineByType(Str, defineline);
            defineline = AddDefineByType(Word, defineline);
            return defineline;

            static int AddDefineByType(ArgType type, int defineline) {
                string definetext = "", arglist = "";
                switch (type) {
                case ActionCmd:
                    definetext = "%action ";
                    break;
                case TestCmd:
                    definetext = "%test ";
                    break;
                case Flag:
                    definetext = "%flag ";
                    break;
                case Var:
                    definetext = "%var ";
                    break;
                case InvItem or SObj:
                    definetext = "%object ";
                    break;
                case ArgType.View:
                    definetext = "%view ";
                    break;
                case Ctrl or Str or Word:
                    definetext = "%define ";
                    break;
                }
                SortedList<int, DecodeDefine> list = [];
                foreach (DecodeDefine define in newDefines.Values) {
                    if (define.Type == type) {
                        // when decoding all, variables <220, flags <220,
                        // all ctrl, str, words and views, objects get
                        // added to gamedefs.h
                        if (decompAll) {
                            switch (type) {
                            case Flag:
                            case Var:
                                // only add if value is 220 or greater
                                if (define.Value >= 220) {
                                    list.Add(define.Value, define);
                                    newDefines.Remove(define.Name);
                                }
                                break;
                            case Ctrl:
                            case Str:
                            case Word:
                            case ArgType.View:
                            case InvItem:
                            case SObj:
                            case ActionCmd:
                            case TestCmd:
                                // don't add these to local
                                break;
                            case Num:
                            case MsgNum:
                            case ArgType.Object:
                            case DefStr:
                            case VocWrd:
                            case MSG:
                            case WORD:
                            case ANY:
                            case WORDLIST:
                            case SierraArgToken:
                            case Symbol:
                            case AssignOperator:
                            case TestOperator:
                            case Separator:
                            case BadString:
                            case Keyword:
                            case Label:
                            case Comment:
                            case Preprocessor:
                            case None:
                            case Unknown:
                                // not possible
                                Debug.Assert(false);
                                break;
                            }
                        }
                        else {
                            list.Add(define.Value, define);
                            newDefines.Remove(define.Name);
                        }
                    }
                }
                if (list.Count > 0) {
                    outputList.Insert(defineline++, "");
                    foreach (DecodeDefine define in list.Values) {
                        if (type == ActionCmd || type == TestCmd) {
                            // build arg list
                            arglist = "(";
                            for (int i = 0; i < define.ArgList.Length; i++) {
                                arglist += SierraArgName(define.ArgList[i]);
                                if (i < define.ArgList.Length - 1) {
                                    arglist += ", ";
                                }
                            }
                            arglist += ")";
                            outputList.Insert(defineline++,
                                definetext + define.Name + arglist +
                                define.Value.ToString().PadLeft(4) + " ");
                        }
                        else {
                            outputList.Insert(defineline++,
                            definetext + define.Name.PadRight(16) +
                            define.Value.ToString().PadLeft(4));
                        }
                    }
                }
                return defineline;
            }
        }

        private static DecodeDefine SierraDefaultAction(byte cmdNum) {
            DecodeDefine retval = new(ActionCmd, ActionCommands[cmdNum].FanName, cmdNum) {
                ArgList = new ArgType[ActionCommands[cmdNum].ArgList.Length]
            };
            if (retval.ArgList.Length == 0) {
                return retval;
            }
            // build args based on number
            switch (cmdNum) {
            //case 0: // return();
            //case 26: // show.pic()
            //case 29: // show.pri.screen()
            //case 34: // unanimate.all()
            //case 91: // unblock()
            //case 100: // stop.sound()
            //case 106: // text.screen()
            //case 107: // graphics()
            //case 112: // status.line.on()
            //case 113: // status.line.off()
            //case 119: // prevent.input()
            //case 120: // accept.input()
            //case 124: // status()
            //case 125: // save.game()
            //case 126: // restore.game()
            //case 127: // init.disk()
            //case 128: // restart.game()
            //case 131: // program.control()
            //case 132: // player.control()
            //case 135: // show.mem()
            //case 136: // pause()
            //case 137: // echo.line()
            //case 138: // cancel.line()
            //case 139: // init.joy()
            //case 140: // toggle.monitor()
            //case 141: // version()
            //case 145: // set.scan.start()
            //case 146: // reset.scan.start()
            //case 149: // trace.on()
            //case 158: // submit.menu()
            //case 161: // menu.input()
            //case 163: // open.dialogue()
            //case 164: // close.dialogue()
            //case 169: // closewindow()
            //case 171: // push.script()
            //case 172: // pop.script()
            //case 173: // hold.key()
            //case 181: // release.key()
            //    break;

            case 12: // set(FLAG)
            case 13: // reset(FLAG)
            case 14: // toggle(FLAG)
                retval.ArgList[0] = Flag;
                break;

            case 101: // print(MSGNUM)
            case 108: // set.cursor.char(MSGNUM)
            case 143: // set.game.id(MSGNUM)
            case 144: // log(MSGNUM)
            case 156: // set.menu(MSGNUM)
                retval.ArgList[0] = MsgNum;
                break;

            case 18: // new.room(NUM)
            case 20: // load.logics(NUM)
            case 22: // call(NUM)
            case 63: // set.horizon(NUM)
            case 98: // load.sound(NUM)
            case 110: // shake.screen(NUM)
            case 117: // parse(NUM)
            case 134: // quit(NUM)
            case 142: // script.size(NUM)
            case 159: // enable.item(NUM)
            case 160: // disable.item(NUM)
            case 165: // muln(VAR, NUM)
            case 167: // divn(VAR, NUM)
            case 170: // set.simple(NUM)
            case 174: // set.pri.base(NUM)
            case 175: // discard.sound(NUM)
            case 176: // hide.mouse(NUM)
            case 177: // allow.menu(NUM)
            case 178: // show.mouse(NUM)
                retval.ArgList[0] = Num;
                break;

            case 33: // animate.obj(OBJECT)
            case 35: // draw(OBJECT)
            case 36: // erase(OBJECT)
            case 45: // fix.loop(OBJECT)
            case 46: // release.loop(OBJECT)
            case 56: // release.priority(OBJECT)
            case 58: // stop.update(OBJECT)
            case 59: // start.update(OBJECT)
            case 60: // force.update(OBJECT)
            case 61: // ignore.horizon(OBJECT)
            case 62: // observe.horizon(OBJECT)
            case 64: // object.on.water(OBJECT)
            case 65: // object.on.land(OBJECT)
            case 66: // object.on.anything(OBJECT)
            case 67: // ignore.objs(OBJECT)
            case 68: // observe.objs(OBJECT)
            case 70: // stop.cycling(OBJECT)
            case 71: // start.cycling(OBJECT)
            case 72: // normal.cycle(OBJECT)
            case 74: // reverse.cycle(OBJECT)
            case 77: // stop.motion(OBJECT)
            case 78: // start.motion(OBJECT)
            case 84: // wander(OBJECT)
            case 85: // normal.motion(OBJECT)
            case 88: // ignore.blocks(OBJECT)
            case 89: // observe.blocks(OBJECT)
            case 92: // get(OBJECT)
            case 94: // drop(OBJECT)
                retval.ArgList[0] = ArgType.Object;
                break;

            case 1: // increment(VAR)
            case 2: // decrement(VAR)
            case 15: // set.v(VAR)
            case 16: // reset.v(VAR)
            case 17: // toggle.v(VAR)
            case 19: // new.room.v(VAR)
            case 21: // load.logics.v(VAR)
            case 23: // call.v(VAR)
            case 24: // load.pic(VAR)
            case 25: // draw.pic(VAR)
            case 27: // discard.pic(VAR)
            case 28: // overlay.pic(VAR)
            case 31: // load.view.v(VAR)
            case 93: // get.v(VAR)
            case 102: // print.v(VAR)
            case 133: // obj.status.v(VAR)
            case 153: // discard.viewV(VAR)
            case 162: // show.obj.v(VAR)
                retval.ArgList[0] = Var;
                break;

            case 30: // load.view(VIEW)
            case 32: // discard.view(VIEW)
            case 129: // show.obj(VIEW)
                retval.ArgList[0] = ArgType.View;
                break;

            case 99: // sound(NUM, FLAG)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Flag;
                break;

            case 114: // set.string(NUM, MSGNUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = MsgNum;
                break;

            case 109: // set.text.attribute(NUM, NUM)
            case 116: // word.to.string(NUM, NUM)
            case 155: // set.upper.left(NUM, NUM)
            case 180: // mouse.posn(NUM, NUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Num;
                break;

            case 103: // display(NUM, NUM, MSGNUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = MsgNum;
                break;

            case 105: // clear.lines(NUM, NUM, NUM)
            case 111: // configure.screen(NUM, NUM, NUM)
            case 121: // set.key(NUM, NUM, NUM)
            case 150: // trace.info(NUM, NUM, NUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                break;

            case 130: // random(NUM, NUM, VAR)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Var;
                break;

            case 90: // block(NUM, NUM, NUM, NUM)
            case 179: // fence.mouse(NUM, NUM, NUM, NUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                break;

            case 115: // get.string(NUM, MSGNUM, NUM, NUM, NUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = MsgNum;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                retval.ArgList[3] = Num;
                break;

            case 154: // clear.text.rect(NUM, NUM, NUM, NUM, NUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                retval.ArgList[4] = Num;
                break;

            case 157: // set.menu.item(MSGNUM, NUM)
                retval.ArgList[0] = MsgNum;
                retval.ArgList[1] = Num;
                break;

            case 118: // get.num(MSGNUM, VAR)
                retval.ArgList[0] = MsgNum;
                retval.ArgList[1] = Var;
                break;

            case 151: // print.at(MSGNUM, NUM, NUM, NUM)
                retval.ArgList[0] = MsgNum;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                break;

            case 73: // end.of.loop(OBJECT, FLAG)
            case 75: // reverse.loop(OBJECT, FLAG)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Flag;
                break;

            case 43: // set.loop(OBJECT, NUM)
            case 47: // set.cel(OBJECT, NUM)
            case 54: // set.priority(OBJECT, NUM)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Num;
                break;

            case 42: // set.view.v(OBJECT, VAR)
            case 44: // set.loopV(OBJECT, VAR)
            case 48: // set.cel.v(OBJECT, VAR)
            case 49: // last.cel(OBJECT, VAR)
            case 50: // current.cel(OBJECT, VAR)
            case 51: // current.loop(OBJECT, VAR)
            case 52: // current.view(OBJECT, VAR)
            case 53: // number.of.loops(OBJECT, VAR)
            case 55: // set.priority.v(OBJECT, VAR)
            case 57: // get.priority(OBJECT, VAR)
            case 76: // cycle.time(OBJECT, VAR)
            case 79: // step.size(OBJECT, VAR)
            case 80: // step.time(OBJECT, VAR)
            case 86: // set.dir(OBJECT, VAR)
            case 87: // get.dir(OBJECT, VAR)
            case 95: // put(OBJECT, VAR)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Var;
                break;

            case 41: // set.view(OBJECT, VIEW)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = ArgType.View;
                break;

            case 83: // follow.ego(OBJECT, NUM, FLAG)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Flag;
                break;

            case 37: // position(OBJECT, NUM, NUM)
            case 147: // reposition.to(OBJECT, NUM, NUM)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                break;

            case 69: // distance(OBJECT, OBJECT, VAR)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = ArgType.Object;
                retval.ArgList[2] = Var;
                break;

            case 38: // position.v(OBJECT, VAR, VAR)
            case 39: // get.posn(OBJECT, VAR, VAR)
            case 40: // reposition(OBJECT, VAR, VAR)
            case 148: // reposition.to.v(OBJECT, VAR, VAR)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Var;
                retval.ArgList[2] = Var;
                break;

            case 81: // move.obj(OBJECT, NUM, NUM, NUM, FLAG)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                retval.ArgList[4] = Flag;
                break;

            case 82: // move.obj.v(OBJECT, VAR, VAR, VAR, FLAG)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Var;
                retval.ArgList[2] = Var;
                retval.ArgList[3] = Var;
                retval.ArgList[4] = Flag;
                break;

            case 3: // assignn(VAR, NUM)
            case 5: // addn(VAR, NUM)
            case 7: // subn(VAR, NUM)
            case 11: // lindirectn(VAR, NUM)
                retval.ArgList[0] = Var;
                retval.ArgList[1] = Num;
                break;

            case 4: // assignv(VAR, VAR)
            case 6: // addv(VAR, VAR)
            case 8: // subv(VAR, VAR)
            case 9: // lindirectv(VAR, VAR)
            case 10: // rindirect(VAR, VAR)
            case 96: // put.v(VAR, VAR)
            case 97: // get.roomV(VAR, VAR)
            case 166: // mulv(VAR, VAR)
            case 168: // divv(VAR, VAR)
                retval.ArgList[0] = Var;
                retval.ArgList[1] = Var;
                break;

            case 104: // display.v(VAR, VAR, VAR)
                retval.ArgList[0] = Var;
                retval.ArgList[1] = Var;
                retval.ArgList[2] = Var;
                break;

            case 152: // print.at.v(VAR, NUM, NUM, NUM)
                retval.ArgList[0] = Var;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                break;

            case 122: // add.to.pic(VIEW, NUM, NUM, NUM, NUM, NUM, NUM)
                retval.ArgList[0] = ArgType.View;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                retval.ArgList[4] = Num;
                retval.ArgList[5] = Num;
                retval.ArgList[6] = Num;
                break;

            case 123: // add.to.pic.v(VAR, VAR, VAR, VAR, VAR, VAR, VAR)
                retval.ArgList[0] = Var;
                retval.ArgList[1] = Var;
                retval.ArgList[2] = Var;
                retval.ArgList[3] = Var;
                retval.ArgList[4] = Var;
                retval.ArgList[5] = Var;
                retval.ArgList[6] = Var;
                break;
            }
            return retval;
        }

        private static DecodeDefine SierraDefaultTest(byte cmdNum) {
            DecodeDefine retval = new(TestCmd, TestCommands[cmdNum].FanName, cmdNum) {
                ArgList = new ArgType[TestCommands[cmdNum].ArgList.Length]
            };
            switch (cmdNum) {
            case 1: // equaln(VAR, NUM)
            case 3: // lessn(VAR, NUM)
            case 5: // greatern(VAR, NUM)
            case 6: // greaterv(VAR, NUM)
                retval.ArgList[0] = Var;
                retval.ArgList[1] = Num;
                break;

            case 2: // equalv(VAR, VAR)
            case 4: // lessv(VAR, VAR)
                retval.ArgList[0] = Var;
                retval.ArgList[1] = Var;
                break;

            case 7: // isset(FLAG)
                retval.ArgList[0] = Flag;
                break;

            case 8: // isset.v(VAR)
                retval.ArgList[0] = Var;
                break;

            case 9: // has(OBJECT)
                retval.ArgList[0] = ArgType.Object;
                break;

            case 10: // obj.in.room(OBJECT, VAR)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Var;
                break;

            case 11: // posn(OBJECT, NUM, NUM, NUM, NUM)
            case 16: // obj.in.box(OBJECT, NUM, NUM, NUM, NUM)
            case 17: // center.posn(OBJECT, NUM, NUM, NUM, NUM)
            case 18: // right.posn(OBJECT, NUM, NUM, NUM, NUM)
                retval.ArgList[0] = ArgType.Object;
                retval.ArgList[1] = Num;
                retval.ArgList[2] = Num;
                retval.ArgList[3] = Num;
                retval.ArgList[4] = Num;
                break;

            case 12: // controller(NUM)
                retval.ArgList[0] = Num;
                break;

            case 13: // have.key()
                break;

            case 14: // said(WORDLIST)
                retval.ArgList = new ArgType[1];
                retval.ArgList[0] = WORDLIST;
                break;

            case 15: // compare.strings(NUM, NUM)
                retval.ArgList[0] = Num;
                retval.ArgList[1] = Num;
                break;
            }
            return retval;
        }

        private static string SierraArgName(ArgType type) {
            // FLAG, OBJECT, MSG, WORD, NUM, MSGNUM, VIEW, VAR, ANY, WORDLIST
            // type MSG, WORD, ANY, WORDLIST will never be used when decompiling
            switch (type) {
            case Flag:
                return "FLAG";
            case ArgType.Object:
                return "OBJECT";
            case Num:
                return "NUM";
            case MsgNum:
                return "MSGNUM";
            case ArgType.View:
                return "VIEW";
            case Var:
                return "VAR";
            case WORDLIST:
                return "WORDLIST";
            default:
                return "";
            }
        }

        static byte ReadByte(ref int curpos) {
            if (curpos >= logicdata.Length) {
                throw new LogicDecodeBufferOverflowException($"Buffer overflow at position {pos}");
            }
            return logicdata[curpos++];
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
        /// warning to the warning stack so it can be added to the output once the 
        /// current line is fully decompiled.
        /// </summary>
        /// <param name="WarnID"></param>
        /// <param name="WarningText"></param>
        /// <param name="LineNum"></param>
        static void AddDecodeWarning(string WarnID, string WarningText, int LineNum) {
            WinAGIEventInfo dcWarnInfo = new() {
                ResNum = dcLogic.Number,
                ResType = AGIResType.Logic,
                Type = EventType.DecompWarning,
                ID = WarnID,
                Module = dcLogic.ID,
                Filename = "",
                Text = WarningText,
                Line = LineNum,
            };
            AGIGame.OnDecodeLogicStatus(dcWarnInfo);
            if (!addWarning) {
                addWarning = true;
            }
            warningText.Add("WARNING " + WarnID + ": " + WarningText);
        }

        static void AddDecodeError(string errID, string errText, int LineNum) {
            WinAGIEventInfo dcErrInfo = new() {
                ResNum = dcLogic.Number,
                ResType = AGIResType.Logic,
                Type = EventType.DecompError,
                ID = errID,
                Module = dcLogic.ID,
                Filename = "",
                Text = errText,
                Line = LineNum,
            };
            AGIGame.OnDecodeLogicStatus(dcErrInfo);
            if (!addWarning) {
                addWarning = true;
            }
            minorErrors = true;
            warningText.Add("ERROR: " + errID + ": " + errText);
            AddWarningLines();
        }

        private static string ArgValue(bool actioncmd, byte cmdNum, int argPos, byte argNum, ArgType argType, int varVal = -1) {
            if (sierraSyntax) {
                return SierraArgValue(actioncmd, cmdNum, argPos, argNum, argType, varVal);
            }
            else {
                return FanArgValue(actioncmd, cmdNum, argPos, argNum, argType, varVal);
            }
        }

        /// <summary>
        /// This method converts the specified argument bytecode into an appropriately
        /// formatted string representation, either a define value, or a simple 
        /// argument marker.<br/>
        /// cmdNum is only used for action commands; use -1 for test commands.<br/>
        /// varVal is used when converting numbers to identify which variables 
        /// use reserved text for their number values.
        /// </summary>
        /// <param name="argNum"></param>
        /// <param name="argType"></param>
        /// <param name="varVal"></param>
        /// <returns></returns>
        private static string FanArgValue(bool actioncmd, int cmdNum, int argPos, byte argNum, ArgType argType, int varVal = -1) {

            if (!ReservedAsText && argType != MsgNum) {
                // return simple argument marker
                return agArgTypPref[(int)argType] + argNum;
            }
            switch (argType) {
            case Num:
                switch (varVal) {
                case 2 or 5:
                    // v2 and v5 use edge codes
                    if (argNum <= 4) {
                        return reservedList.EdgeCodes[argNum].Name;
                    }
                    else {
                        return argNum.ToString();
                    }
                case 6:
                    // v6 uses direction codes
                    if (argNum <= 8) {
                        return reservedList.ObjDirections[argNum].Name;
                    }
                    else {
                        return argNum.ToString();
                    }
                case 20:
                    // v20 uses computer type codes
                    if (argNum <= 8) {
                        return reservedList.ComputerTypes[argNum].Name;
                    }
                    else {
                        return argNum.ToString();
                    }
                case 26:
                    // v26 uses video mode codes
                    if (argNum <= 4) {
                        return reservedList.VideoModes[argNum].Name;
                    }
                    else {
                        return argNum.ToString();
                    }
                default:
                    // some action commands use resources as arguments; substitute as appropriate
                    if (actioncmd && dcGame is not null && dcGame.agIncludeReserved && dcLogic.InGame) {
                        switch (cmdNum) {
                        case 18:
                            // new.room - only arg is a logic
                            if (dcGame.agLogs.Contains(argNum)) {
                                return dcGame.agLogs[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Logic " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "new.room"), outputList.Count + 1);
                            }
                            break;
                        case 20:
                            // load.logics - only arg is a logic
                            if (dcGame.agLogs.Contains(argNum)) {
                                return dcGame.agLogs[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Logic " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "load.logics"), outputList.Count + 1);
                            }
                            break;
                        case 22:
                            // call - only arg is a logic
                            if (dcGame.agLogs.Contains(argNum)) {
                                return dcGame.agLogs[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Logic " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "call"), outputList.Count + 1);
                            }
                            break;
                        case 30:
                            // load.view - only arg is a view
                            if (dcGame.agViews.Contains(argNum)) {
                                return dcGame.agViews[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                    ARG1, "View " + argNum.ToString()).Replace(
                                    ARG2, pos.ToString()).Replace(
                                    ARG3, "load.view"), outputList.Count + 1);
                            }
                            break;
                        case 32:
                            // discard.view - only arg is a view
                            if (dcGame.agViews.Contains(argNum)) {
                                return dcGame.agViews[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                    ARG1, "View " + argNum.ToString()).Replace(
                                    ARG2, pos.ToString()).Replace(
                                    ARG3, "discard.view"), outputList.Count + 1);
                            }
                            break;
                        case 41:
                            // set.view - 2nd arg is a view
                            if (argPos == 1) {
                                if (dcGame.agViews.Contains(argNum)) {
                                    return dcGame.agViews[argNum].ID;
                                }
                                else {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                        ARG1, "View " + argNum.ToString()).Replace(
                                        ARG2, pos.ToString()).Replace(
                                        ARG3, "set.view"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 98:
                            // load.sound - only arg is asound
                            if (dcGame.agSnds.Contains(argNum)) {
                                return dcGame.agSnds[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Sound " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "load.sound"), outputList.Count + 1);
                            }
                            break;
                        case 99:
                            // sound = 1st arg is a sound
                            if (argPos == 0) {
                                if (dcGame.agSnds.Contains(argNum)) {
                                    return dcGame.agSnds[argNum].ID;
                                }
                                else {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                       ARG1, "Sound " + argNum.ToString()).Replace(
                                       ARG2, pos.ToString()).Replace(
                                       ARG3, "sound"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 122:
                            // add.to.pic - 1st arg is a view
                            if (argPos == 0) {
                                if (dcGame.agViews.Contains(argNum)) {
                                    return dcGame.agViews[argNum].ID;
                                }
                                else {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                        ARG1, "View " + argNum.ToString()).Replace(
                                        ARG2, pos.ToString()).Replace(
                                        ARG3, "add.to.pic"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 129:
                            // show.obj - only arg is a view
                            if (dcGame.agViews.Contains(argNum)) {
                                return dcGame.agViews[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                    ARG1, "View " + argNum.ToString()).Replace(
                                    ARG2, pos.ToString()).Replace(
                                    ARG3, "show.obj"), outputList.Count + 1);
                            }
                            break;
                        case 150:
                            // trace.info - 1st arg is a logic
                            if (argPos == 0) {
                                if (dcGame.agLogs.Contains(argNum)) {
                                    return dcGame.agLogs[argNum].ID;
                                }
                                else {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                       ARG1, "Logic " + argNum.ToString()).Replace(
                                       ARG2, pos.ToString()).Replace(
                                       ARG3, "trace.info"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 175:
                            // discard.sound - only arg is a sound
                            if (dcGame.agSnds.Contains(argNum)) {
                                return dcGame.agSnds[argNum].ID;
                            }
                            else {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Sound " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "discard.sound"), outputList.Count + 1);
                            }
                            break;
                        }
                    }
                    if (actioncmd) {
                        // check for commands that use colors
                        switch (cmdNum) {
                        case 105:
                            // clear.lines - 3rd arg
                            if (argPos == 2) {
                                if (argNum < 16) {
                                    return reservedList.ColorNames[argNum].Name;
                                }
                            }
                            break;
                        case 109:
                            // set.text.attribute - all args
                            if (argNum < 16) {
                                return reservedList.ColorNames[argNum].Name;
                            }
                            break;
                        case 154:
                            // clear.text.rect - 5th arg
                            if (argPos == 4) {
                                if (argNum < 16) {
                                    return reservedList.ColorNames[argNum].Name;
                                }
                            }
                            break;
                        }
                    }
                    // use default
                    return argNum.ToString();
                }
            case Var:
                if (argNum <= 26) {
                    return reservedList.ReservedVariables[argNum].Name;
                }
                else {
                    return 'v' + argNum.ToString();
                }
            case Flag:
                if (argNum <= 16) {
                    return reservedList.ReservedFlags[argNum].Name;
                }
                else if (argNum == 20 &&
                    (dcGame is null ||
                    dcGame.agIntVersion.Index >= AGIVersion.v3002098)) {
                    return reservedList.ReservedFlags[17].Name;
                }
                else {
                    return 'f' + argNum.ToString();
                }
            case MsgNum:
                MsgUsed[argNum] = true;
                if (MsgExists[argNum]) {
                    if (MsgsByNumber) {
                        return 'm' + argNum.ToString();
                    }
                    else {
                        return MsgList[argNum];
                    }
                }
                else {
                    // message doesn't exist; raise a warning
                    AddDecodeWarning("DW01", EngineResources.DW01.Replace(
                        ARG1, argNum.ToString()).Replace(
                        ARG2, pos.ToString()), outputList.Count + 1);
                    return 'm' + argNum.ToString();
                }
            case SObj:
                if (argNum == 0) {
                    return reservedList.ReservedObjects[0].Name;
                }
                else {
                    // check for out of bounds value
                    CheckScreenObjValue(argNum);
                    return 'o' + argNum.ToString();
                }
            case InvItem:
                if (dcGame is not null && !IObjsByNumber) {
                    Debug.Assert(dcGame.agInvObj.Loaded);
                    if (argNum < dcGame.agInvObj.Count) {
                        if (dcGame.agInvObj[argNum].Unique) {
                            if (dcGame.agInvObj[argNum].ItemName == "?") {
                                // use the inventory item number, and post a warning
                                AddDecodeWarning("DW04", EngineResources.DW04.Replace(
                                    ARG2, pos.ToString()), outputList.Count + 1);
                                return 'i' + argNum.ToString();
                            }
                            else {
                                // a unique, non-questionmark item- use it's string Value
                                return QUOTECHAR + dcGame.agInvObj[argNum].ItemName.Replace(QUOTECHAR.ToString(), "\\\"") + QUOTECHAR;
                            }
                        }
                        else {
                            // non-unique - use obj number instead
                            if (ErrorLevel == Medium) {
                                AddDecodeWarning("DW05", EngineResources.DW05.Replace(
                                    ARG1, dcGame.agInvObj[argNum].ItemName).Replace(
                                    ARG2, pos.ToString()), outputList.Count + 1);
                            }
                            return 'i' + argNum.ToString();
                        }
                    }
                    else {
                        AddDecodeWarning("DW03", EngineResources.DW03.Replace(
                            ARG1, argNum.ToString()).Replace(
                            ARG2, pos.ToString()), outputList.Count + 1);
                        return 'i' + argNum.ToString();
                    }
                }
                else {
                    return 'i' + argNum.ToString();
                }
            case Str:
                if (argNum == 0) {
                    return reservedList.ReservedStrings[0].Name;
                }
                else {
                    // check for out of bounds value
                    CheckStringValue(argNum);
                    return 's' + argNum.ToString();
                }
            case Ctrl:
                // check for out of bounds value
                CheckControlValue(argNum);
                return 'c' + argNum.ToString();
            case Word:
                // check for out of bounds value
                CheckWordValue(argNum);
                // convert argument to a 'one-based' value so it is consistent with
                // the syntax used in the agi 'print' commands
                return 'w' + (argNum + 1).ToString();
            }
            // can't get here, but compiler demands a return statement
            return "";
        }

        private static string SierraArgValue(bool actioncmd, int cmdNum, int argPos, byte argNum, ArgType argType, int varVal = -1) {
            string argName = "";
            // need to know correct fan type if errors come up
            ArgType fanType;
            if (actioncmd) {
                fanType = ActionCommands[cmdNum].ArgList[argPos];
            }
            else {
                fanType = TestCommands[cmdNum].ArgList[argPos];
            }

            switch (argType) {
            // 'normal' sierra types
            case Flag:
                // check for a define first
                if (sierradefs.TryGetName(Flag, argNum, ref argName)) {
                    return argName;
                }
                if (decompAll) {
                    // use a default flag define
                    if (argNum < 220) {
                        argName = "f" + argNum.ToString();
                    }
                    else if (argNum < 240) {
                        argName = "lf" + (argNum - 220).ToString();
                    }
                    else {
                        argName = "df" + (argNum - 240).ToString();
                    }
                    newDefines.TryAdd(argName, new(Flag, argName, argNum));
                    return argName;
                }
                // decompiling a single file
                // add as a local define
                argName = "f" + argNum.ToString();
                newDefines.TryAdd(argName, new(Flag, argName, argNum));
                return argName;
            case ArgType.Object:
                // WinAGI types SObj and InvItem are defined as OBJECT
                // reassign based on fan syntax arg type

                if (fanType == SObj) {
                    argType = SObj;
                    break;
                }
                else if (fanType == InvItem) {
                    argType = InvItem;
                    break;
                }
                else {
                    // add a local define
                    argName = "object." + argNum.ToString();
                    newDefines.TryAdd(argName, new(ArgType.Object, argName, argNum));
                    return argName;
                }
            case Num:
                // WinAGI types Str, Ctrl, Word are defined as NUM
                // if one of those, reassign
                if (fanType == Str) {
                    argType = Str;
                    break;
                }
                else if (fanType == Ctrl) {
                    argType = Ctrl;
                    break;
                }
                else if (fanType == Word) {
                    argType = Word;
                    break;
                }
                // must be an actual number
                argName = argNum.ToString();
                switch (varVal) {
                case 2 or 5:
                    // v2 and v5 use edge codes
                    if (argNum <= 4) {
                        sierradefs.TryGetName(Num, DecodeSubtype.EdgeCode, argNum, ref argName);
                    }
                    break;
                case 6:
                    // v6 uses direction codes
                    if (argNum <= 8) {
                        sierradefs.TryGetName(Num, DecodeSubtype.ObjDir, argNum, ref argName);
                    }
                    break;
                case 20:
                    // v20 uses computer type codes
                    if (argNum <= 8) {
                        sierradefs.TryGetName(Num, DecodeSubtype.MachineType, argNum, ref argName);
                    }
                    break;
                case 26:
                    // v26 uses video mode codes
                    if (argNum <= 4) {
                        sierradefs.TryGetName(Num, DecodeSubtype.MonitorType, argNum, ref argName);
                    }
                    break;
                default:
                    if (dcGame is not null) {
                        // some action commands use resources as arguments;
                        // validate as appropriate
                        switch (cmdNum) {
                        case 18:
                            // new.room - only arg is a logic
                            if (!dcGame.agLogs.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Logic " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "new.room"), outputList.Count + 1);
                            }
                            break;
                        case 20:
                            // load.logics - only arg is a logic
                            if (!dcGame.agLogs.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Logic " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "load.logics"), outputList.Count + 1);
                            }
                            break;
                        case 22:
                            // call - only arg is a logic
                            if (!dcGame.agLogs.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Logic " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "call"), outputList.Count + 1);
                            }
                            break;
                        case 30:
                            // load.view - only arg is a view
                            if (!dcGame.agViews.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                    ARG1, "View " + argNum.ToString()).Replace(
                                    ARG2, pos.ToString()).Replace(
                                    ARG3, "load.view"), outputList.Count + 1);
                            }
                            break;
                        case 32:
                            // discard.view - only arg is a view
                            if (!dcGame.agViews.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                    ARG1, "View " + argNum.ToString()).Replace(
                                    ARG2, pos.ToString()).Replace(
                                    ARG3, "discard.view"), outputList.Count + 1);
                            }
                            break;
                        case 41:
                            // set.view - 2nd arg is a view
                            if (argPos == 1) {
                                if (!dcGame.agViews.Contains(argNum)) {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                        ARG1, "View " + argNum.ToString()).Replace(
                                        ARG2, pos.ToString()).Replace(
                                        ARG3, "set.view"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 98:
                            // load.sound - only arg is a sound
                            if (!dcGame.agSnds.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Sound " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "load.sound"), outputList.Count + 1);
                            }
                            break;
                        case 99:
                            // sound = 1st arg is a sound
                            if (argPos == 0) {
                                if (!dcGame.agSnds.Contains(argNum)) {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                       ARG1, "Sound " + argNum.ToString()).Replace(
                                       ARG2, pos.ToString()).Replace(
                                       ARG3, "sound"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 122:
                            // add.to.pic - 1st arg is a view
                            if (argPos == 0) {
                                if (!dcGame.agViews.Contains(argNum)) {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                        ARG1, "View " + argNum.ToString()).Replace(
                                        ARG2, pos.ToString()).Replace(
                                        ARG3, "add.to.pic"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 129:
                            // show.obj - only arg is a view
                            if (!dcGame.agViews.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                    ARG1, "View " + argNum.ToString()).Replace(
                                    ARG2, pos.ToString()).Replace(
                                    ARG3, "show.obj"), outputList.Count + 1);
                            }
                            break;
                        case 150:
                            // trace.info - 1st arg is a logic
                            if (argPos == 0) {
                                if (!dcGame.agLogs.Contains(argNum)) {
                                    AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                       ARG1, "Logic " + argNum.ToString()).Replace(
                                       ARG2, pos.ToString()).Replace(
                                       ARG3, "trace.info"), outputList.Count + 1);
                                }
                            }
                            break;
                        case 175:
                            // discard.sound - only arg is a sound
                            if (!dcGame.agSnds.Contains(argNum)) {
                                AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                                   ARG1, "Sound " + argNum.ToString()).Replace(
                                   ARG2, pos.ToString()).Replace(
                                   ARG3, "discard.sound"), outputList.Count + 1);
                            }
                            break;
                        }
                    }
                    // check for commands that use colors
                    switch (cmdNum) {
                    case 105:
                        // clear.lines - 3rd arg
                        if (argPos == 2) {
                            if (argNum < 16) {
                                sierradefs.TryGetName(Num, DecodeSubtype.Colors, argNum, ref argName);
                            }
                        }
                        break;
                    case 109:
                        // set.text.attribute - all args
                        if (argNum < 16) {
                            sierradefs.TryGetName(Num, DecodeSubtype.Colors, argNum, ref argName);
                        }
                        break;
                    case 154:
                        // clear.text.rect - 5th arg
                        if (argPos == 4) {
                            if (argNum < 16) {
                                sierradefs.TryGetName(Num, DecodeSubtype.Colors, argNum, ref argName);
                            }
                        }
                        break;
                    }
                    break;
                }
                return argName;
            case MsgNum:
                // (actually 'MSGNUM', but since 'MSG' isn't used by CG.EXE, WinAGI
                // assigns the Msg type in sierrasyntax)
                MsgUsed[argNum] = true;
                if (!MsgExists[argNum]) {
                    // message doesn't exist; raise a warning
                    AddDecodeWarning("DW01", EngineResources.DW01.Replace(
                        ARG1, argNum.ToString()).Replace(
                        ARG2, pos.ToString()), outputList.Count + 1);
                }
                // messages are always by number
                return argNum.ToString();
            case ArgType.View:
                if (dcGame is not null) {
                    // validate number
                    if (!dcGame.Views.Contains(argNum)) {
                        AddDecodeWarning("DW10", EngineResources.DW10.Replace(
                            ARG1, "View" + argNum.ToString()).Replace(
                            ARG2, pos.ToString()).Replace(
                            ARG3, cmdNum > 0 ? ActionCommands[cmdNum].FanName :
                            TestCommands[-cmdNum].FanName), outputList.Count + 1);
                    }
                }
                argName = "v.view" + argNum.ToString();
                // check for an existing view define
                if (sierradefs.TryGetName(ArgType.View, argNum, ref argName)) {
                    // mark it as used
                    if (sierradefs[argName].NotUsed) {
                        DecodeDefine tmp = sierradefs[argName];
                        tmp.NotUsed = false;
                        sierradefs[argName] = tmp;
                    }
                    return argName;
                }
                // otherwise add as a local define
                newDefines.TryAdd(argName, new(ArgType.View, argName, argNum));
                return argName;
            case Var:
                // check for a define first
                if (sierradefs.TryGetName(Var, argNum, ref argName)) {
                    return argName;
                }
                if (decompAll) {
                    // use a default variable name
                    if (argNum < 220) {
                        argName = "v" + argNum.ToString();
                    }
                    else if (argNum < 240) {
                        argName = "lv" + (argNum - 220).ToString();
                    }
                    else {
                        argName = "dv" + (argNum - 240).ToString();
                    }
                    newDefines.TryAdd(argName, new(Var, argName, argNum));
                    return argName;
                }
                // decompiling a single file
                // not found,  add a local define
                argName = "v" + argNum.ToString();
                newDefines.TryAdd(argName, new(Var, argName, argNum));
                return argName;
            }
            // If not yet assigned, it's a non-sierra type (either converted
            // from the generic sierra types (NUM, OBJECT), or it's from a
            // command that wasn't defined, so it's using the FAN syntax types.)
            // This includes Ctrl, SObj, InvItem, Str, Word
            switch (argType) {
            case Ctrl:
                CheckControlValue(argNum);
                if (decompAll) {
                    argName = "c" + argNum.ToString();
                    newDefines.TryAdd(argName, new(Ctrl, argName, argNum));
                }
                else {
                    argName = argNum.ToString();
                }
                return argName;
            case SObj:
                CheckScreenObjValue(argNum);
                argName = "o" + argNum.ToString();
                if (sierradefs.TryGetName(SObj, argNum, ref argName)) {
                    return argName;
                }
                // otherwise add as a local
                newDefines.TryAdd(argName, new(SObj, argName, argNum));
                return argName;
            case InvItem:
                CheckInvItemValue(argNum);
                // check for an existing item define
                foreach (DecodeDefine tmp in sierradefs.Values) {
                    if (tmp.Type == InvItem &&
                        tmp.Value == argNum) {
                        argName = tmp.Name;
                        DecodeDefine newtmp = tmp;
                        newtmp.NotUsed = false;
                        sierradefs[argName] = newtmp;
                        return argName;
                    }
                }
                // not found- must be invalid object number
                argName = "i" + argNum.ToString();
                //Debug.Assert(dcGame is not null && argNum >= dcGame.InvObjects.Count);
                // otherwise add as a local define
                newDefines.TryAdd(argName, new(InvItem, argName, argNum));
                return argName;
            case Str:
                CheckStringValue(argNum);
                argName = "s" + argNum.ToString();
                if (sierradefs.TryGetName(Str, argNum, ref argName)) {
                    return argName;
                }
                // otherwise add as a local
                newDefines.TryAdd(argName, new(Str, argName, argNum));
                return argName;
            case Word:
                CheckWordValue(argNum);
                argName = "w" + (argNum + 1).ToString();
                if (sierradefs.TryGetName(Word, argNum, ref argName)) {
                    return argName;
                }
                // otherwise add as a local
                newDefines.TryAdd(argName, new(Word, argName, argNum));
                return argName;
            }
            return "";
        }

        private static string NewDefineName(string argName, ArgType type, byte argNum) {
            // creates a unique, short, codesafe name for this name
            string baseName = CleanString(argName);
            if (baseName.Length > 18) {
                baseName = baseName[..18];
            }
            argName = baseName;
            int i = -1;
            do {
                if (i != -1) {
                    argName = baseName + "_" + i.ToString();
                }
                i++;
                if (sierradefs.ContainsKey(argName)) {
                    continue;
                }
                if (newDefines.ContainsKey(argName)) {
                    // check if this is the same define, or a new one
                    if (newDefines[argName].Type == type &&
                        newDefines[argName].Name == argName &&
                        newDefines[argName].Value == argNum) {
                        // it's already added
                        return argName;
                    }
                    else {
                        continue;
                    }
                }
                break;
            } while (true);
            return argName;
        }

        public static string CleanString(string text) {
            // convert to lower case, replace spaces with dots,
            // remove any non-useable characters
            // these include  !"&'()*+,-/:;<=>?[\]^`{|}~
            // and any chars < 32 or > 127
            const string unwanted = "!\"&'()*+,-/:;<=>?[\\]^`{|}~";
            return new string([.. text.Replace(' ', '.').ToLower().Where(
                c => c >= 32 && c <= 127 && !unwanted.Contains(c))]);
        }

        private static void CheckInvItemValue(byte argNum) {
            if (dcGame is not null) {
                // validate number
                if (argNum < dcGame.agInvObj.Count) {
                    if (dcGame.agInvObj[argNum].Unique) {
                        if (dcGame.agInvObj[argNum].ItemName == "?") {
                            // using a null item
                            AddDecodeWarning("DW04", EngineResources.DW04.Replace(
                                ARG2, pos.ToString()), outputList.Count + 1);
                        }
                    }
                }
                else {
                    // not a valid number
                    AddDecodeWarning("DW03", EngineResources.DW03.Replace(
                        ARG1, argNum.ToString()).Replace(
                        ARG2, pos.ToString()), outputList.Count + 1);
                }
            }
        }

        private static void CheckScreenObjValue(byte argNum) {
            if (dcGame is not null && argNum > dcGame.InvObjects.MaxScreenObjects) {
                AddDecodeWarning("DW13", EngineResources.DW13.Replace(
                    ARG1, dcGame.InvObjects.MaxScreenObjects.ToString()),
                    outputList.Count + 1);
            }
        }

        private static void CheckStringValue(byte argNum) {
            int smax = 23;
            if (dcGame is not null && (dcGame.InterpreterVersion.Index <= AGIVersion.v2272 ||
                dcGame.InterpreterVersion.Index == AGIVersion.v3002149)) {
                smax = 11;
            }
            if (argNum > smax) {
                AddDecodeWarning("DW14", EngineResources.DW14.Replace(
                    ARG1, smax.ToString()), outputList.Count + 1);
            }
        }

        private static void CheckControlValue(byte argNum) {
            int cmax = 49;
            if (dcGame is not null && dcGame.InterpreterVersion.Index <= AGIVersion.v2272) {
                cmax = 29;
            }
            if (argNum > cmax) {
                AddDecodeWarning("DW15", EngineResources.DW15.Replace(
                    ARG1, cmax.ToString()), outputList.Count + 1);
            }
        }

        private static void CheckWordValue(byte argNum) {
            if (argNum > 9) {
                AddDecodeWarning("DW16", EngineResources.DW16, outputList.Count + 1);
            }
        }

        /// <summary>
        /// This method extracts message text from the end of the logic resource.
        /// </summary>
        /// <param name="bytData"></param>
        /// <param name="msgStart"></param>
        /// <param name="Decrypt"></param>
        /// <returns>true if messages extracted successfully, false if any errors 
        /// occur which prevent decooding the logic</returns>
        static bool ReadMessages(int msgStart, bool Decrypt) {
            int[] MessageStart = new int[256];

            // validate msg start
            if (msgStart >= logicdata.Length) {
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
            int NumMessages = ReadByte(ref pos);
            if (NumMessages > 0) {
                // calculate end of message pointer
                int textEnd = ReadByte(ref pos) + msgStart + 256 * ReadByte(ref pos);
                if (textEnd != logicdata.Length - 1) {
                    AddDecodeError("DE07", EngineResources.DE07, outputList.Count - 1);
                    // adjust it to end
                    textEnd = logicdata.Length - 1;
                }
                // loop through all messages, extract offset
                for (int i = 1; i <= NumMessages; i++) {
                    // set start of this msg as start of msg block, plus offset, plus one (for byte which gives number of msgs)
                    MessageStart[i] = ReadByte(ref pos) + 256 * ReadByte(ref pos) + msgStart + 1;
                    // validate msg start
                    if (MessageStart[i] >= logicdata.Length) {
                        AddDecodeError("DE08", EngineResources.DE08.Replace(
                            ARG1, i.ToString()).Replace(
                            ARG2, (pos - 2).ToString()), outputList.Count - 1);
                        MessageStart[i] = 0;
                    }
                }
                // decrypt the entire message section, if needed
                int textStart = pos;
                if (Decrypt) {
                    for (int i = pos; i <= textEnd; i++) {
                        logicdata[i] ^= bytEncryptKey[(i - textStart) % 11];
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
                            byte charval = ReadByte(ref pos);
                            if ((charval == 0) || (pos >= logicdata.Length)) {
                                endOfMsg = true;
                            }
                            else {
                                switch (charval) {
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
                                    bMsgText.Add((byte)charval.ToString("x2")[0]);
                                    bMsgText.Add((byte)charval.ToString("x2")[1]);
                                    break;
                                default:
                                    bMsgText.Add(charval);
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
                            MsgList.Add(QUOTECHAR + Encoding.GetEncoding(dcLogic.CodePage).GetString(bMsgText.ToArray()) + QUOTECHAR);
                        }
                        MsgExists[i] = true;
                    }
                    else {
                        // add a null message (so numbers work out)
                        MsgList.Add("");
                        MsgExists[i] = false;
                    }
                }
                // sierra syntax pus messages in a separate file, so
                // add the include entry here
                if (sierraSyntax && NumMessages >= 1) {
                    outputList.Insert(outputList.Count - 1, "%include \"" + dcLogic.ID + ".msg\"");
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
        static bool DecodeIf() {
            bool isEmpty = true;
            bool firstCmd = true;
            bool inOrBlock = false;
            bool ifFinished = false;
            string lineText = INDENT.MultStr(DecodeBlock.Count - 1) + D_TKN_IF;

            do {
                // always reset 'NOT' block status to false
                bool isNOT = false;
                byte curByte = ReadByte(ref pos);
                // first, check for an 'or'
                if (curByte == 0xFC) {
                    isEmpty = false;
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
                    curByte = ReadByte(ref pos);
                }
                // special check needed in case two 0xFCs are in a row, e.g. (a || b) && (c || d)
                if (curByte == 0xFC) {
                    if (!inOrBlock) {
                        lineText += D_TKN_AND;
                        outputList.Add(lineText);
                        lineText = INDENT.MultStr(DecodeBlock.Count - 1) + "    ";
                        firstCmd = true;
                        lineText += "(";
                        inOrBlock = true;
                        curByte = ReadByte(ref pos);
                    }
                    else {
                        // an 'OR' block with no commands- need to skip it and 
                        // add a warning, because it will cause a compile error
                        AddDecodeWarning("DW22", EngineResources.DW22.Replace(
                            ARG2, pos.ToString()), outputList.Count + 1);
                    }
                }
                // check for 'not' command
                if (curByte == 0xFD) {
                    isEmpty = false;
                    // RARE- but allowed! multiple NOTs in a row will
                    // toggle the NOT status

                    byte notcount = 0;
                    do {
                        isNOT = !isNOT;
                        notcount++;
                        curByte = ReadByte(ref pos);
                    } while (curByte == 0xFD);
                    if (notcount != 1) {
                        // rare, but multiple 'NOT' operators are allowed
                        AddDecodeWarning("DW21", EngineResources.DW21.Replace(
                            ARG1, pos.ToString()), outputList.Count + 1);
                        // (adjust line to account for the if() line)
                    }
                }
                // check for valid test command
                if ((curByte > 0) && (curByte <= TestCount)) {
                    isEmpty = false;
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
                    byte cmdNum = curByte;
                    if (SpecialSyntax && cmdNum >= 1 && cmdNum <= 6) {
                        curByte = ReadByte(ref pos);
                        byte arg2Val = ReadByte(ref pos);
                        lineText += AddSpecialIf(cmdNum, curByte, arg2Val, isNOT);
                    }
                    else if (SpecialSyntax && cmdNum == 7) {
                        if (sierraSyntax) {
                            // verify command has been defined
                            CheckCmdName(TestCmd, cmdNum);
                        }
                        curByte = ReadByte(ref pos);
                        if (isNOT) {
                            lineText += D_TKN_NOT;
                        }
                        lineText += ArgValue(false, cmdNum, 0, curByte, Flag);
                    }
                    else {
                        if (isNOT) {
                            lineText += D_TKN_NOT;
                        }
                        ArgType[] args;
                        if (sierraSyntax) {
                            GetCmdName(TestCmd, cmdNum, out args, out string cmdName);
                            lineText += cmdName + "(";
                        }
                        else {
                            lineText = lineText + TestCommands[cmdNum].FanName + "(";
                            args = TestCommands[cmdNum].ArgList;
                        }
                        if (cmdNum == 14) {
                            // said command
                            if (sierraSyntax) {
                                // verify words.tok has been included
                                if (!wordstokLoaded) {
                                    AddDecodeError("DE11", EngineResources.DE11, outputList.Count - 1);
                                }
                            }
                            byte argcount = ReadByte(ref pos);
                            for (int argpos = 1; argpos <= argcount; argpos++) {
                                int groupNum = ReadByte(ref pos) + 256 * ReadByte(ref pos);
                                if (dcGame is not null && !WordsByNumber) {
                                    if (dcGame.agVocabWords.GroupExists(groupNum)) {
                                        if (dcGame.agVocabWords.GroupByNumber(groupNum).WordCount > 0) {
                                            if (sierraSyntax) {
                                                lineText += dcGame.agVocabWords.GroupByNumber(groupNum).Words[0].Replace(' ', '$');
                                            }
                                            else {
                                                // fan syntax allows numbers
                                                lineText += QUOTECHAR + dcGame.agVocabWords.GroupByNumber(groupNum).GroupName + QUOTECHAR;
                                            }
                                        }
                                        else {
                                            // sierra syntax requires the word to be defined; in
                                            // fan syntax, if group 0, 1, or 9999, using the group
                                            // is allowed if the group doesn't have any words (rare)
                                            if (sierraSyntax) {
                                                AddDecodeError("DE12", EngineResources.DE12.Replace(
                                                    ARG1, groupNum.ToString()), outputList.Count - 1);
                                            }
                                            lineText += groupNum.ToString();
                                        }
                                    }
                                    else {
                                        if (sierraSyntax) {
                                            AddDecodeError("DE12", EngineResources.DE12.Replace(
                                                ARG1, groupNum.ToString()), outputList.Count - 1);
                                        }
                                        else {
                                            AddDecodeWarning("DW02", EngineResources.DW02.Replace(
                                                ARG1, groupNum.ToString()).Replace(
                                                ARG2, pos.ToString()), outputList.Count + 1);
                                        }
                                        // add the word by its group number
                                        lineText += groupNum;
                                    }
                                }
                                else {
                                    // note that in sierra syntax numbers are not valid,
                                    // but without a WORDS.TOK file there is no other option
                                    lineText += groupNum;
                                    if (sierraSyntax) {
                                        AddDecodeWarning("DW23", EngineResources.DW23, outputList.Count + 1);
                                    }
                                }

                                if (argpos < argcount) {
                                    lineText += ", ";
                                }
                            }
                        }
                        else {
                            for (int argpos = 0; argpos < args.Length; argpos++) {
                                curByte = ReadByte(ref pos);
                                string argText;
                                argText = ArgValue(false, cmdNum, argpos, curByte, args[argpos]);
                                lineText += argText;
                                if (argpos < TestCommands[cmdNum].ArgList.Length - 1) {
                                    lineText += ", ";
                                }
                            }
                        }
                        lineText += ")";
                    }
                    firstCmd = false;
                    if (cmdNum == 19) {
                        if (sierraSyntax) {
                            // in sierra syntax, treat test cmd as invalid
                            AddDecodeError("DE02", EngineResources.DE02.Replace(
                                ARG1, cmdNum.ToString()).Replace(
                                ARG2, pos.ToString()), outputList.Count - 1);
                        }
                        else {
                            // add warning if this is the unknown test19 command
                            AddDecodeWarning("DW06", EngineResources.DW06.Replace(
                                ARG2, pos.ToString()), outputList.Count + 1);
                        }
                    }
                }
                else if (curByte == 0xFF) {
                    // done with if block; add 'then'
                    if (isEmpty) {
                        // works, but will cause a compiler error
                        AddDecodeWarning("DW22", EngineResources.DW22.Replace(
                            ARG2, pos.ToString()), outputList.Count + 1);
                    }
                    switch (CodeStyle) {
                    case AGICodeStyle.cstDefaultStyle:
                        lineText += ")";
                        outputList.Add(lineText);
                        lineText = INDENT.MultStr(DecodeBlock.Count) + "{";
                        break;
                    case AGICodeStyle.cstAltStyle1:
                    case AGICodeStyle.cstAltStyle2:
                        lineText += ") {";
                        break;
                    }
                    DecodeBlock.Add(new());
                    DecodeBlock[^1].IsIf = true;
                    DecodeBlock[^1].Length = ReadByte(ref pos) + 256 * ReadByte(ref pos);
                    if (DecodeBlock[^1].Length == 0) {
                        AddDecodeWarning("DW07", EngineResources.DW07.Replace(
                            ARG2, pos.ToString()), outputList.Count + 1);
                    }
                    // validate end pos
                    DecodeBlock[^1].EndPos = DecodeBlock[^1].Length + pos;
                    if (DecodeBlock[^1].EndPos >= msgSecStart) {
                        // adjust to end
                        DecodeBlock[^1].EndPos = msgSecStart - 1;
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
                        AddDecodeWarning("DW08", EngineResources.DW08.Replace(
                            ARG1, DecodeBlock[^1].JumpPos.ToString()).Replace(
                            ARG2, pos.ToString()), outputList.Count + 1);
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

        private static void CheckCmdName(ArgType type, byte cmdNum) {
            string cmdName;
            if (type == ActionCmd) {
                cmdName = ActionCommands[cmdNum].FanName;
            }
            else {
                cmdName = TestCommands[cmdNum].FanName;
            }
            if (!sierradefs.TryGetValue(cmdName, out _)) {
                // add as a local, with a warning
                string typename;
                ArgType[] args;
                if (type == ActionCmd) {
                    typename = "Action";
                    args = ActionCommands[cmdNum].ArgList;
                }
                else {
                    typename = "Test";
                    args = TestCommands[cmdNum].ArgList;
                }
                AddDecodeWarning("DW17", EngineResources.DW17.Replace(
                        ARG1, cmdNum.ToString()).Replace(
                        ARG2, cmdName).Replace(
                        ARG3, typename), outputList.Count + 1);
                DecodeDefine newdef = new(type, cmdName, cmdNum) {
                    ArgList = args
                };
                newDefines.TryAdd(cmdName, newdef);
            }
        }

        private static void GetCmdName(ArgType type, byte cmdNum, out ArgType[] args, out string cmdName) {
            cmdName = "";
            if (sierradefs.TryGetName(type, cmdNum, ref cmdName)) {
                args = sierradefs[cmdName].ArgList;
            }
            else {
                // add as a local, with a warning
                DecodeDefine newdef;
                string typename;
                if (type == ActionCmd) {
                    cmdName = ActionCommands[cmdNum].FanName;
                    newdef = SierraDefaultAction(cmdNum);
                    typename = "Action";
                }
                else {
                    cmdName = TestCommands[cmdNum].FanName;
                    newdef = SierraDefaultTest(cmdNum);
                    typename = "Test";
                }
                AddDecodeWarning("DW17", EngineResources.DW17.Replace(
                        ARG1, cmdNum.ToString()).Replace(
                        ARG2, cmdName).Replace(
                        ARG3, typename), outputList.Count + 1);
                args = newdef.ArgList;
                newDefines.TryAdd(cmdName, newdef);
            }
        }

        /// <summary>
        /// This method is used by the FindLabel method to skip the cursor to
        /// the end of the current 'if' block. It also validates basic command and
        /// block data.
        /// </summary>
        /// <param name="logicdata"></param>
        /// <returns></returns>
        static bool SkipToEndIf() {
            byte curByte;
            byte NumSaidArgs;
            byte ThisCommand;
            bool inOrBlock = false;

            do {
                curByte = ReadByte(ref pos);
                if (curByte == 0xFC) {
                    inOrBlock = !inOrBlock;
                    curByte = ReadByte(ref pos);
                }
                // special check needed in case two 0xFCs are in a row, e.g. (a || b) && (c || d)
                if ((curByte == 0xFC) && (!inOrBlock)) {
                    curByte = ReadByte(ref pos);
                }
                // check for 'not' command
                if (curByte == 0xFD) {
                    // RARE- but allowed! multiple NOTs in a row
                    do {
                        curByte = ReadByte(ref pos);
                    } while (curByte == 0xFD);
                }
                // now check for valid test command, endif, or error
                if ((curByte > 0) && (curByte <= TestCount)) {
                    ThisCommand = curByte;
                    if (ThisCommand == 14) {
                        // said command
                        NumSaidArgs = ReadByte(ref pos);
                        // move pointer to next position past these arguments
                        // (words use two bytes per argument, not one)
                        pos += NumSaidArgs * 2;
                    }
                    else {
                        // move pointer to next position past the arguments for this command
                        pos += TestCommands[ThisCommand].ArgList.Length;
                    }
                }
                else if (curByte == 0xFF) {
                    // increment block counter
                    DecodeBlock.Add(new());
                    DecodeBlock[^1].IsIf = true;
                    DecodeBlock[^1].Length = ReadByte(ref pos) + 256 * ReadByte(ref pos);
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
                        // if loop exited normally (it will equal bytLabelCount+1)
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
                        ARG1, curByte.ToString()).Replace(
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
        static bool FindLabels() {
            byte curByte;
            int tmpBlockLength;
            bool isGoto;
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
                            dcGame is not null &&
                            dcGame.agIntVersion.Index != AGIVersion.v2089 &&
                            DecodeBlock[i].HasQuit) {
                            checkQuit = true;
                            return false;
                        }
                        // take this block off stack
                        DecodeBlock.RemoveAt(DecodeBlock.Count - 1);
                    }
                }
                curByte = ReadByte(ref pos);
                switch (curByte) {
                case 0xFF:
                    // start of an IF statement
                    if (!SkipToEndIf()) {
                        // major error - error message added by SkipToEndIf
                        return false;
                    }
                    break;
                case 0xFE:
                    // GOTO command
                    // reset goto status flag
                    isGoto = false;
                    tmpBlockLength = ReadByte(ref pos) + 256 * ReadByte(ref pos);
                    // check for negative value
                    if (tmpBlockLength > 0x7FFF) {
                        tmpBlockLength -= 0x10000;
                    }
                    // check to see if this 'goto' might be an 'else':
                    //  - end of this block matches this position (the if-then part is done)
                    //  - this block is identified as an IF block
                    //  - this is NOT the main block
                    if ((DecodeBlock[^1].EndPos == pos) && DecodeBlock[^1].IsIf && (DecodeBlock.Count > 1)) {
                        // the 'else' block re-uses same level as the 'if'
                        DecodeBlock[^1].IsIf = false;
                        DecodeBlock[^1].IsOutside = false;
                        // confirm the 'else' block is formed correctly:
                        //  - the end of this block doesn't go past where the previous block ends
                        //  - the block is not negative (means jumping backward, so it MUST be a goto)
                        //  - length of block has enough room for code necessary to close the 'else'
                        // SPECIAL CASE: if the goto location is inside a block that is level
                        // 2 or higher, it's actually a goto, not an else
                        // only way to tell is to check the block level at the location
                        if (tmpBlockLength + pos <= DecodeBlock[^2].EndPos &&
                            tmpBlockLength >= 0 && DecodeBlock[^1].Length > 3 &&
                            CheckGotoBlock(tmpBlockLength + pos)) {
                            // this is an 'else' block - readjust block end so the IF statement that
                            // owns this 'else' is ended correctly
                            DecodeBlock[^1].Length = tmpBlockLength;
                            DecodeBlock[^1].EndPos = DecodeBlock[^1].Length + pos;
                        }
                        else {
                            // this is a 'goto' statement
                            isGoto = true;
                        }
                    }
                    else {
                        // this is a goto statement
                        isGoto = true;
                    }
                    // goto
                    if (isGoto) {
                        LabelLoc = tmpBlockLength + pos;
                        // don't need to check for invalid destination because it's checked in main decode loop
                        if (!LabelPos.Contains(LabelLoc)) {
                            // add a new label location
                            LabelPos.Add(LabelLoc);
                        }
                    }
                    break;
                case <= MAX_CMDS:
                    // AGI command
                    // skip over arguments to get next command
                    // (even if it's not valid for this version, just account for it here;
                    // the main decoder loop will handle the warning/error for commands 
                    // that are not valid in this version)
                    pos += ActionCommands[curByte].ArgList.Length;
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

        private static bool CheckGotoBlock(int checkpos) {
            // starting with current pos, and current block level, move 
            // forward until at checkpos; return true if chkpos is a valid
            // else (meaning it's not in the middle of an interior block)

            int newpos = pos;
            do {
                byte curByte = ReadByte(ref newpos);
                switch (curByte) {
                case 0xFF:
                    do {
                        curByte = ReadByte(ref newpos);
                        if (curByte == 0xFC) {
                            curByte = ReadByte(ref newpos);
                        }
                        if (curByte == 0xFC) {
                            // two 0xFCs in a row, e.g. (a || b) && (c || d)
                            curByte = ReadByte(ref newpos);
                        }
                        if (curByte == 0xFD) {
                            continue;
                        }
                        if ((curByte > 0) && (curByte <= TestCount)) {
                            if (curByte == 14) {
                                // said command
                                // move pointer to next position past these arguments
                                // (words use two bytes per argument, not one)
                                newpos += ReadByte(ref newpos) * 2;
                            }
                            else {
                                // move pointer to next position past the arguments for this command
                                newpos += TestCommands[curByte].ArgList.Length;
                            }
                        }
                        else if (curByte == 0xFF) {
                            // end of the if; jump to it's ending point
                            // (no need to check anything inbetween- we just
                            // need to know if the ending point is past the
                            // check point)
                            int offset = ReadByte(ref newpos) + 256 * ReadByte(ref newpos);
                            newpos += offset;
                            // no need to check for an 'else' in this block
                            // to confirm status of the block being checked
                            // only need to know if the goto point is inside
                            // an if block

                            //if (logicdata[newpos - 3] == 0xFE) {
                            //    newpos += logicdata[newpos - 2] + 256 * logicdata[newpos - 1];
                            //}
                            break;
                        }
                        else {
                            // error - doesn't matter
                            return false;
                        }
                    }
                    while (true);
                    // check if now past checkpos
                    if (newpos > checkpos) {
                        return false;
                    }
                    break;
                case 0xFE:
                    // GOTO command
                    // skip to next command
                    newpos += 2;
                    break;
                case <= MAX_CMDS:
                    // AGI command
                    // skip over arguments to get next command
                    newpos += ActionCommands[curByte].ArgList.Length;
                    break;
                default:
                    // unknown action command - major error
                    // result doesn't really matter
                    return false;
                }
            }
            while (newpos < checkpos);
            // block end found without problem
            return true;
        }

        /// <summary>
        /// This method adds the message declarations to end of the source code output.
        /// </summary>
        /// <param name="stlOut"></param>
        static void AddMessages(List<string> stlOut) {
            bool msgfileerr = false;
            Exception fex = null;
            if (sierraSyntax && dcGame is not null) {
                if (MsgList.Count > 0) {
                    const int MAXMSGLINELEN = 60;
                    // create a separate file
                    try {
                        List<string> msgfile = ["[ Messages for room " +
                            dcLogic.Number + " -- " + dcLogic.ID, ""];
                        for (int i = 1; i < MsgList.Count; i++) {
                            if (MsgExists[i]) {
                                if (!MsgUsed[i]) {
                                    msgfile.Add("[ " + EngineResources.DW18.Replace(
                                        ARG1, i.ToString()));
                                }
                                msgfile.Add(D_TKN_MESSAGE.Replace(ARG1, i.ToString().PadLeft(3, ' ')));
                                while (MsgList[i].Length > MAXMSGLINELEN) {
                                    // break it up into chunks
                                    int space = MsgList[i].LastIndexOf(' ', MAXMSGLINELEN);
                                    if (space <= 0) {
                                        space = MAXMSGLINELEN;
                                    }
                                    msgfile.Add(MsgList[i][..space]);
                                    MsgList[i] = MsgList[i][space..];
                                }
                                msgfile.Add(MsgList[i]);
                                msgfile.Add("");
                            }
                        }
                        File.WriteAllText(Path.Combine(dcGame.SrcResDir, dcLogic.ID + ".MSG"), string.Join(NEWLINE, [.. msgfile]));
                        return;
                    }
                    catch (Exception ex) {
                        // add an error message
                        msgfileerr = true;
                        fex = ex;
                    }
                }
            }
            // add the messages to end of source code
            stlOut.Add(D_TKN_COMMENT + "DECLARED MESSAGES");
            if (msgfileerr) {
                AddDecodeWarning("DW19", EngineResources.DW19.Replace(
                    ARG1, dcLogic.ID).Replace(
                    ARG2, fex.Message), outputList.Count + 1);
                stlOut.Add("");
                D_TKN_MESSAGE = "%message %1 %2";
            }
            for (int i = 1; i < MsgList.Count; i++) {
                if (MsgExists[i] && (ShowAllMessages || !MsgUsed[i])) {
                    stlOut.Add(D_TKN_MESSAGE.Replace(
                        ARG1, i.ToString()).Replace(
                        ARG2, MsgList[i]));
                }
            }
        }

        /// <summary>
        /// This method adds action commands that use special syntax form (i.e. v# += 1;).
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        static string AddSpecialCmd(byte cmd) {
            if (sierraSyntax) {
                // verify the command exists
                CheckCmdName(ActionCmd, cmd);
            }
            byte arg1 = ReadByte(ref pos);
            // check for unary command
            if (cmd == 0x01) {
                // increment
                return "++" + ArgValue(true, 0x01, 0, arg1, Var);
            }
            else if (cmd == 0x02) {
                // decrement
                return "--" + ArgValue(true, 0x02, 0, arg1, Var);
            }
            // check for binary command
            byte arg2 = ReadByte(ref pos);
            switch (cmd) {
            case 0x03:
                // assignn
                return ArgValue(true, 0x03, 0, arg1, Var) + " = " + ArgValue(true, 0x03, 1, arg2, Num, arg1);
            case 0x04:
                // assignv
                return ArgValue(true, 0x04, 0, arg1, Var) + " = " + ArgValue(true, 0x04, 1, arg2, Var);
            case 0x05:
                // addn
                return ArgValue(true, 0x05, 0, arg1, Var) + "  += " + ArgValue(true, 0x05, 1, arg2, Num);
            case 0x06:
                // addv
                return ArgValue(true, 0x06, 0, arg1, Var) + "  += " + ArgValue(true, 0x06, 1, arg2, Var);
            case 0x07:
                // subn
                return ArgValue(true, 0x07, 0, arg1, Var) + " -= " + ArgValue(true, 0x07, 1, arg2, Num);
            case 0x08:
                // subv
                return ArgValue(true, 0x08, 0, arg1, Var) + " -= " + ArgValue(true, 0x08, 1, arg2, Var);
            case 0x09:
                // lindirectv
                // output is different between sierra and fan syntax
                if (sierraSyntax) {
                    return SierraArgValue(true, 0x09, 0, arg1, Var) + " @= " + SierraArgValue(true, 0x09, 1, arg2, Var);
                }
                else {
                    return "*" + FanArgValue(true, 0x09, 0, arg1, Var) + " = " + FanArgValue(true, 0x09, 1, arg2, Var);
                }
            case 0x0A:
                // rindirect
                // output is different between sierra and fan syntax
                if (sierraSyntax) {
                    return SierraArgValue(true, 0x0A, 0, arg1, Var) + " =@" + SierraArgValue(true, 0x0A, 1, arg2, Var);
                }
                else {
                    return FanArgValue(true, 0x0A, 0, arg1, Var) + " = *" + FanArgValue(true, 0x0A, 1, arg2, Var);
                }
            case 0x0B:
                // lindirectn
                // output is different between sierra and fan syntax
                if (sierraSyntax) {
                    return SierraArgValue(true, 0x0B, 0, arg1, Var) + " @= " + SierraArgValue(true, 0x0B, 1, arg2, Num);
                }
                else {
                    return "*" + FanArgValue(true, 0x0B, 0, arg1, Var) + " = " + FanArgValue(true, 0x0B, 1, arg2, Num);
                }
            case 0xA5:
                // mul.n
                // fan syntax only
                return FanArgValue(true, 0xA5, 0, arg1, Var) + " *= " + FanArgValue(true, 0xA5, 1, arg2, Num);
            case 0xA6:
                // mul.v
                // fan syntax only
                return FanArgValue(true, 0xA6, 0, arg1, Var) + " *= " + FanArgValue(true, 0xA6, 1, arg2, Var);
            case 0xA7:
                // div.n
                // fan syntax only
                return FanArgValue(true, 0xA7, 0, arg1, Var) + " /= " + FanArgValue(true, 0xA7, 1, arg2, Num);
            case 0xA8:
                // div.v
                // fan syntax only
                return FanArgValue(true, 0xA8, 0, arg1, Var) + " /= " + FanArgValue(true, 0xA8, 1, arg2, Var);
            default:
                // required even though code will never get here
                return "";
            }
        }

        /// <summary>
        /// This method adds test commands that use special syntax form (i.e. v# == #).
        /// </summary>
        /// <param name="cmdNum"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="isNOT"></param>
        /// <returns></returns>
        static string AddSpecialIf(byte cmdNum, byte arg1, byte arg2, bool isNOT) {
            if (sierraSyntax) {
                // verify command has been defined
                CheckCmdName(TestCmd, cmdNum);
            }

            string retval = ArgValue(false, cmdNum, 0, arg1, Var);
            switch (cmdNum) {
            case 1 or 2:
                // equaln or equalv
                if (isNOT) {
                    retval += D_TKN_NOT_EQUAL;
                }
                else {
                    retval += D_TKN_EQUAL;
                }
                if (cmdNum == 2) {
                    retval += ArgValue(false, cmdNum, 1, arg2, Var);
                }
                else {
                    retval += ArgValue(false, cmdNum, 1, arg2, Num, arg1);
                }
                break;
            case 3 or 4:
                // lessn, lessv
                if (sierraSyntax) {
                    if (isNOT) {
                        retval = D_TKN_NOT + retval;
                    }
                    retval += " < ";
                }
                else {
                    if (isNOT) {
                        retval += " >= ";
                    }
                    else {
                        retval += " < ";
                    }
                }
                if (cmdNum == 4) {
                    retval += ArgValue(false, cmdNum, 1, arg2, Var);
                }
                else {
                    retval += ArgValue(false, cmdNum, 1, arg2, Num, arg1);
                }

                break;
            case 5:
                // greatern
                // special case - if arg2 is 0, use just variable
                // e.g. 'v99 > 0' becomes 'v99' and 'v99 <= 0' becomes '!v99'
                if (arg2 == 0) {
                    if (isNOT) {
                        retval = D_TKN_NOT + retval;
                    }
                }
                else {
                    if (sierraSyntax) {
                        if (isNOT) {
                            retval = D_TKN_NOT + retval;
                        }
                        retval += " > ";
                    }
                    else {
                        if (isNOT) {
                            retval += " <= ";
                        }
                        else {
                            retval += " > ";
                        }
                    }
                    retval += ArgValue(false, cmdNum, 1, arg2, Num, arg1);
                }
                break;
            case 6:
                // greatern, greaterv
                if (sierraSyntax) {
                    if (isNOT) {
                        retval = D_TKN_NOT + retval;
                    }
                    retval += " > ";
                }
                else {
                    if (isNOT) {
                        retval += " <= ";
                    }
                    else {
                        retval += " > ";
                    }
                }
                if (cmdNum == 6) {
                    retval += ArgValue(false, cmdNum, 1, arg2, Var);
                }
                else {
                    retval += ArgValue(false, cmdNum, 1, arg2, Num, arg1);
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
                    AddDecodeError("DE06", EngineResources.DE06.Replace(
                        ARG2, pos.ToString()), outputList.Count - 1);
                }
                if (DecodeBlock[CurBlock].EndPos <= pos) {
                    // check for unusual case where an if block ends outside the if block it is nested in
                    if (DecodeBlock[CurBlock].IsOutside) {
                        // end current block
                        outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + D_TKN_ENDIF.Replace(ARG1, INDENT));
                        // append else to end of current block
                        switch (CodeStyle) {
                        case AGICodeStyle.cstDefaultStyle:
                            outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + "else");
                            outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + INDENT + "{");
                            break;
                        case AGICodeStyle.cstAltStyle1:
                            outputList.Add(INDENT.MultStr(DecodeBlock.Count - 2) + "else {");
                            break;
                        case AGICodeStyle.cstAltStyle2:
                            outputList[^1] += " else {";
                            break;
                        }
                        // then add a goto
                        for (int i = 0; i < LabelPos.Count; i++) {
                            if (LabelPos[i] == DecodeBlock[CurBlock].JumpPos) {
                                string gotoline = INDENT.MultStr(DecodeBlock.Count - 1);
                                if (sierraSyntax) {
                                    gotoline += D_TKN_GOTO.Replace(ARG1, "label" + i) + D_TKN_EOL;
                                }
                                else {
                                    gotoline += D_TKN_GOTO.Replace(ARG1, "Label" + i) + D_TKN_EOL;
                                }
                                outputList.Add(gotoline);
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
                // 'then', 'else' handled inline
                D_TKN_ENDIF = "%1}"; // where %1 is an extra indent
                if (sierraSyntax) {
                    D_TKN_GOTO = "goto %1";
                }
                else {
                    D_TKN_GOTO = "goto(%1)";
                }
                D_TKN_EOL = ";";
                D_TKN_AND = " && ";
                D_TKN_OR = " || ";
                D_TKN_EQUAL = " == ";
                D_TKN_NOT_EQUAL = " != ";
                D_TKN_COMMENT = "[ ";
                if (sierraSyntax) {
                    D_TKN_MESSAGE = "%message  %1";
                }
                else {
                    D_TKN_MESSAGE = "#message %1 %2";
                }
                break;
            default:
                D_TKN_NOT = "!";
                D_TKN_IF = "if (";
                // 'then', 'else' handled inline
                D_TKN_ENDIF = "}";
                if (sierraSyntax) {
                    D_TKN_GOTO = "goto %1";
                }
                else {
                    D_TKN_GOTO = "goto(%1)";
                }
                D_TKN_EOL = ";";
                D_TKN_AND = " && ";
                D_TKN_OR = " || ";
                D_TKN_EQUAL = " == ";
                D_TKN_NOT_EQUAL = " != ";
                D_TKN_COMMENT = "[ ";
                if (sierraSyntax) {
                    D_TKN_MESSAGE = "%message %1";
                }
                else {
                    D_TKN_MESSAGE = "#message %1 %2";
                }
                break;
            }
            tokensSet = true;
        }

        internal static bool DecodeAllSierraLogics(AGIGame game) {
            WinAGIEventInfo info = new() {
                Type = EventType.Info,
                InfoType = InfoType.DecodingAllLogics,
                ResType = AGIResType.None
            };
            AGIGame.OnLoadGameStatus(info);
            info.ResType = AGIResType.Logic;

            dcGame = game;
            decompAll = true;
            // read sysdefs to get default defines
            bool retval = LoadSierraDefines();
            // new defines that will be added to gamedefs.h will be added to newDefines
            newDefines = [];
            // add names for inventory items, views
            DecodeDefine newdef;
            for (int i = 0; i < dcGame.InvObjects.Count; i++) {
                InventoryItem item = dcGame.InvObjects[i];
                newdef = new(InvItem, item.ItemName == "?" ?
                    "i" + i.ToString() :
                    NewDefineName("i." + item.ItemName, InvItem, (byte)i), i) {
                    NotUsed = true
                };
                sierradefs.Add(newdef.Name, newdef);
            }
            foreach (View view in dcGame.Views) {
                newdef = new(ArgType.View, NewDefineName("v." + view.ID, ArgType.View, view.Number), view.Number) {
                    NotUsed = true
                };
                sierradefs.Add(newdef.Name, newdef);
            }
            // words.tok reference will be added to gamedefs.h
            wordstokLoaded = true;

            // decompile each logic, keeping track of game definitions that 
            // need to be added to gamedefs.h
            foreach (Logic logic in game.Logics) {
                info.ResNum = logic.Number;
                AGIGame.OnLoadGameStatus(info);
                // force decompile
                logic.LoadSource(true);
                if (logic.SourceError != ResourceErrorType.NoError &&
                    logic.SourceError != ResourceErrorType.LogicSourceDecompileError) {
                    AddLoadError(OpenGameMode.Directory, game, AGIResType.Logic, logic.Number, logic.SourceError, logic.ErrData);
                    retval = true;
                }
                // unload the logic after decompile is done
                logic.Unload();
            }

            // after all logics are decompiled, update gamedefs.h with dynamic
            // fields; for inventory items, views and sounds, mark the ones
            // not actually used with a comment
            string gamedefs = EngineResources.GAMEDEFS;
            // %1 = GameID
            gamedefs = gamedefs.Replace("%1", dcGame.GameID);

            // %2 = inventory items
            SortedList<int, string> items = [];
            foreach (var item in sierradefs.Values) {
                if (item.Type == InvItem) {
                    items.Add(item.Value,
                        (item.NotUsed ? "[ " : "") +
                        "%object " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                }
            }
            gamedefs = gamedefs.Replace("%2", string.Join(NEWLINE, items.Values));

            // %3 = variables              "v##"
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == Var) {
                    items.Add(item.Value,
                        "%var " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            gamedefs = gamedefs.Replace("%3", string.Join(NEWLINE, items.Values));

            // %4 = flags                  "f##"
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == Flag) {
                    items.Add(item.Value,
                        "%flag " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            gamedefs = gamedefs.Replace("%4", string.Join(NEWLINE, items.Values));

            // %5 = max screen objects
            gamedefs = gamedefs.Replace("%5", dcGame.InvObjects.MaxScreenObjects.ToString());

            // %6 = screen objects         "o##"
            items = [];
            // ego is in sierradefs.h
            foreach (var item in newDefines.Values) {
                if (item.Type == SObj) {
                    items.Add(item.Value,
                        "%object " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            gamedefs = gamedefs.Replace("%6", string.Join(NEWLINE, items.Values));

            // %7 = views
            items = [];
            foreach (var item in sierradefs.Values) {
                if (item.Type == ArgType.View) {
                    items.Add(item.Value,
                        (item.NotUsed ? "[ " : "") +
                        "%view " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                }
            }
            gamedefs = gamedefs.Replace("%7", string.Join(NEWLINE, items.Values));

            // %8 = others:
            //      generic objects (%object)
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == ArgType.Object) {
                    items.Add(item.Value,
                        "%object " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            string addtext = string.Join(NEWLINE, items.Values);

            //      controllers, strings, words (%define)
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == Ctrl) {
                    items.Add(item.Value,
                        "%define " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            if (items.Count > 0) {
                addtext += (addtext.Length > 0 ? NEWLINE + NEWLINE : "") +
                    string.Join(NEWLINE, items.Values);
            }
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == Str) {
                    items.Add(item.Value,
                        "%define " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            if (items.Count > 0) {
                addtext += (addtext.Length > 0 ? NEWLINE + NEWLINE : "") +
                    string.Join(NEWLINE, items.Values);
            }
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == Word) {
                    items.Add(item.Value,
                        "%define " + item.Name.PadRight(20) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            if (items.Count > 0) {
                addtext += (addtext.Length > 0 ? NEWLINE + NEWLINE : "") +
                    string.Join(NEWLINE, items.Values);
            }
            // action, test commands
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == ActionCmd) {
                    items.Add(item.Value,
                        "%action " + item.Name + CreateArgList(item) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            if (items.Count > 0) {
                addtext += (addtext.Length > 0 ? NEWLINE + NEWLINE : "") +
                    string.Join(NEWLINE, items.Values);
            }
            items = [];
            foreach (var item in newDefines.Values) {
                if (item.Type == TestCmd) {
                    items.Add(item.Value,
                        "%test " + item.Name + CreateArgList(item) +
                        item.Value.ToString().PadLeft(4));
                    newDefines.Remove(item.Name);
                }
            }
            if (items.Count > 0) {
                addtext += (addtext.Length > 0 ? NEWLINE + NEWLINE : "") +
                    string.Join(NEWLINE, items.Values);
            }
            gamedefs = gamedefs.Replace("%8", addtext);
            try {
                File.WriteAllText(Path.Combine(dcGame.SrcResDir, "gamedefs.h"), gamedefs);
            }
            catch (Exception ex) {
                // raise a load event directly (decompall only happens
                // during a load from dir action)
                info = new() {
                    Type = EventType.DecompWarning,
                    ResType = AGIResType.Globals,
                    ID = "DW20",
                    Text = EngineResources.DW20.Replace(
                        ARG1, ex.Message),
                    Module = "gamedefs.h",
                    Filename = Path.Combine(dcGame.SrcResDir, "gamedefs.h"),
                    Line = -1,
                    Data = ex
                };
                AGIGame.OnLoadGameStatus(info);
            }
            decompAll = false;
            sierradefs = null;
            newDefines = null;
            return retval;

            static string CreateArgList(DecodeDefine item) {
                string arglist = "(";
                for (int i = 0; i < item.ArgList.Length; i++) {
                    if (i > 0) {
                        arglist += ", ";
                    }
                    switch (item.ArgList[i]) {
                    case Flag:
                        arglist += "FLAG";
                        break;
                    case ArgType.Object:
                        arglist += "OBJECT";
                        break;
                    case Num:
                        arglist += "NUM";
                        break;
                    case MsgNum:
                        arglist += "MSGNUM";
                        break;
                    case ArgType.View:
                        arglist += "VIEW";
                        break;
                    case Var:
                        arglist += "VAR";
                        break;
                    case VocWrd:
                        arglist += "WORDLIST";
                        break;
                    }
                }
                return arglist + ")";
            }
        }

        private static bool LoadSierraDefines() {
            bool retval = false;
            string definetext = "";
            wordstokLoaded = false;
            nestedsysdefs = false;
            hasgamedefs = false;
            int gamedefstart = 0;

            // currently, only ingame logics can be decoded in sierra syntax
            // (dcGame will never be null)
            if (dcGame is null) {
                // use default sysdef
                definetext = EngineResources.SYSDEFS;
            }
            else {
                // load sysdefs - try "sysdefs.h" first
                sysdeffile = Path.Combine(dcGame.SrcResDir, "sysdefs.h");
                if (!File.Exists(sysdeffile)) {
                    // try "sysdefs"
                    sysdeffile = Path.Combine(dcGame.SrcResDir, "sysdefs");
                    if (!File.Exists(sysdeffile)) {
                        // assume no sysdefs
                        sysdeffile = "";
                    }
                }
                if (sysdeffile.Length > 0) {
                    try {
                        definetext = File.ReadAllText(sysdeffile);
                    }
                    catch {
                        retval = true;
                    }
                }
                gamedefstart = definetext.Length;
                if (!decompAll) {
                    // for single file, load gamedefs, if there is one
                    gamedeffile = Path.Combine(dcGame.SrcResDir, "gamedefs.h");
                    if (!File.Exists(gamedeffile)) {
                        // try "gamedefs"
                        gamedeffile = Path.Combine(dcGame.SrcResDir, "gamedefs");
                        if (!File.Exists(gamedeffile)) {
                            // assume no gamedefs
                            gamedeffile = "";
                        }
                    }
                    if (gamedeffile.Length > 0) {
                        try {
                            definetext += "\n" + File.ReadAllText(gamedeffile);
                            hasgamedefs = true;
                        }
                        catch {
                            retval = true;
                        }
                    }
                }
                else {
                    // use default gamedefs file name
                    gamedeffile = "gamedefs.h";
                }
            }
            sierradefs = [];
            int retwarn = 0;
            int pos = 0;
            DecodeSubtype subtype = 0;
            string token = NextToken();
            while (token.Length != 0) {
                switch (token) {
                case "#include":
                case "%include":
                    // reset subtype
                    subtype = DecodeSubtype.None;

                    // next token is file (with extra slashes)
                    token = NextToken();
                    if (token.Length < 3) {
                        // no file
                        retwarn |= 1;
                        break;
                    }
                    if (token[0] != '\"' || token[^1] != '\"') {
                        // file not in quotes
                        retwarn |= 1;
                        break;
                    }
                    // check for nested include of sysdefs
                    string includefile = Path.GetFullPath(token[1..^1], dcGame.SrcResDir);
                    if (string.Compare(includefile, sysdeffile, true) == 0 &&
                        pos > gamedefstart) {
                        // gamedefs includes sysdefs
                        nestedsysdefs = true;
                    }
                    break;
                case "#tokens":
                case "%tokens":
                    // reset subtype
                    subtype = DecodeSubtype.None;

                    // validate path to words.tok
                    token = NextToken();
                    if (token.Length < 3) {
                        // no file
                        retwarn |= 1;
                        break;
                    }
                    if (token[0] != '\"' || token[^1] != '\"') {
                        // file not in quotes
                        retwarn |= 1;
                        break;
                    }
                    // need to handle relative path...
                    if (!File.Exists(Path.GetFullPath(token[1..^1], dcGame.SrcResDir))) {
                        // file missing
                        retwarn |= 1;
                        break;
                    }
                    // mark words.tok as 'loaded'
                    if (wordstokLoaded) {
                        retwarn |= 1;
                    }
                    wordstokLoaded = true;
                    break;
                case "#action":
                case "%action":
                case "#test":
                case "%test":
                    // reset subtype
                    subtype = DecodeSubtype.None;

                    bool test = token.Length == 5;
                    // get cmd name
                    string name = NextToken();
                    // validate it
                    if (BaseNameCheck(name, true) != DefineNameCheck.OK) {
                        // ignore and continue
                        retwarn |= 1;
                        break;
                    }
                    // extract arguments
                    ArgType[] args = ArgsFromText();
                    // number is next
                    token = NextToken();
                    if (!token.IsInt()) {
                        retwarn |= 1;
                        break;
                    }
                    int num = token.IntVal();
                    if (test) {
                        // validate number
                        if (num < 0 || num > TestCount) {
                            retwarn |= 1;
                            break;
                        }
                        // check arguments
                        if (num == 14) {
                            // only one arg (words) is valid
                            if (args.Length == 1 && args[0] == VocWrd) {
                                // OK, but use empty array as placeholder
                                args = [];
                            }
                            else {
                                retwarn |= 1;
                                break;
                            }
                        }
                        else {
                            if (args.Length > 0 && args.Contains(VocWrd)) {
                                retwarn |= 1;
                                break;
                            }
                            if (args.Length != TestCommands[num].ArgList.Length) {
                                retwarn |= 1;
                                break;
                            }
                        }
                    }
                    else {
                        if (num < 0 || num > MAX_CMDS) {
                            retwarn |= 1;
                            break;
                        }
                        if (args.Length > 0 && args.Contains(VocWrd)) {
                            retwarn |= 1;
                            break;
                        }
                        if (args.Length != ActionCommands[num].ArgList.Length) {
                            retwarn |= 1;
                            break;
                        }
                    }
                    // add the token
                    DecodeDefine newdef = new(test ? TestCmd : ActionCmd, name, num) {
                        ArgList = args
                    };
                    if (!sierradefs.TryAdd(name, newdef)) {
                        // can't already be defined
                        retwarn |= 1;
                        break;
                    }
                    break;
                case "#flag":
                case "%flag":
                case "#var":
                case "%var":
                    // reset subtype
                    subtype = DecodeSubtype.None;

                    bool isvar = token.Length == 4;
                    // next token is name
                    name = NextToken();
                    if (BaseNameCheck(name, true) != DefineNameCheck.OK) {
                        // ignore and continue
                        retwarn |= 1;
                        break;
                    }
                    // next is number
                    token = NextToken();
                    if (!token.IsInt()) {
                        retwarn |= 1;
                        break;
                    }
                    num = token.IntVal();
                    if (num < 0 || num > 255) {
                        retwarn |= 1;
                        break;
                    }
                    // add the token
                    newdef = new(isvar ? Var : Flag, name, num);
                    if (!sierradefs.TryAdd(name, newdef)) {
                        // can't already be defined
                        retwarn |= 1;
                        break;
                    }
                    // check for potential subtypes
                    if (isvar) {
                        switch (num) {
                        case 2:
                            subtype = DecodeSubtype.EdgeCode;
                            break;
                        case 6:
                            subtype = DecodeSubtype.ObjDir;
                            break;
                        case 20:
                            subtype = DecodeSubtype.MachineType;
                            break;
                        case 26:
                            subtype = DecodeSubtype.MonitorType;
                            break;
                        default:
                            // reset subtype
                            subtype = DecodeSubtype.None;
                            break;
                        }
                    }
                    break;
                case "#object":
                case "%object":
                    // reset subtype
                    subtype = DecodeSubtype.None;

                    // next token is name
                    name = NextToken();
                    if (BaseNameCheck(name, true) != DefineNameCheck.OK) {
                        // ignore and continue
                        retwarn |= 1;
                        break;
                    }
                    // next is number
                    token = NextToken();
                    if (!token.IsInt()) {
                        retwarn |= 1;
                        break;
                    }
                    num = token.IntVal();
                    if (num < 0 || num > 255) {
                        retwarn |= 1;
                        break;
                    }
                    // add the token
                    if (name == "ego") {
                        newdef = new(SObj, name, num);
                    }
                    else if (name[..2] == "i." || name == "i" + num.ToString()) {
                        newdef = new(InvItem, name, num);
                    }
                    else if (name == "o" + num.ToString()) {
                        newdef = new(SObj, name, num);
                    }
                    else {
                        newdef = new(ArgType.Object, name, num);
                    }
                    // can't already be defined
                    if (!sierradefs.TryAdd(name, newdef)) {
                        retwarn |= 1;
                        break;
                    }
                    break;
                case "#view":
                case "%view":
                    // reset subtype
                    subtype = DecodeSubtype.None;

                    // next token is name
                    name = NextToken();
                    if (BaseNameCheck(name, true) != DefineNameCheck.OK) {
                        // ignore and continue
                        retwarn |= 1;
                        break;
                    }
                    // next is number
                    token = NextToken();
                    if (!token.IsInt()) {
                        retwarn |= 1;
                        break;
                    }
                    num = token.IntVal();
                    if (num < 0 || num > 255) {
                        retwarn |= 1;
                        break;
                    }
                    // add the token
                    newdef = new(ArgType.View, name, num);
                    // can't already be defined
                    if (!sierradefs.TryAdd(name, newdef)) {
                        retwarn |= 1;
                        break;
                    }
                    break;
                case "#define":
                case "%define":
                    // don't reset subtype for defines
                    // next token is name
                    name = NextToken();
                    if (BaseNameCheck(name, true) != DefineNameCheck.OK) {
                        // ignore and continue
                        retwarn |= 1;
                        break;
                    }
                    // next is number
                    token = NextToken();
                    if (!token.IsInt()) {
                        // non-number defines are allowed, but ignored
                        break;
                    }
                    num = token.IntVal();
                    if (num < -128 || num > 255) {
                        retwarn |= 1;
                        break;
                    }
                    // add the token, checking for strings, controllers and words
                    if (subtype == DecodeSubtype.None) {
                        if (name == "s" + num.ToString()) {
                            newdef = new(Str, name, num);
                        }
                        else if (name == "c" + num.ToString()) {
                            newdef = new(Ctrl, name, num);
                        }
                        else if (name == "w" + num.ToString()) {
                            newdef = new(Word, name, num);
                        }
                        else {
                            newdef = new(Num, name, num);
                        }
                    }
                    else {
                        newdef = new(Num, name, num);
                        newdef.SubType = subtype;
                    }
                    if (!sierradefs.TryAdd(name, newdef)) {
                        // can't already be defined
                        retwarn |= 1;
                        break;
                    }
                    break;
                case "#message":
                case "%message":
                    // next is number
                    token = NextToken();
                    if (!token.IsInt()) {
                        // ignore?
                        //retwarn |= 1;
                        break;
                    }
                    num = token.IntVal();
                    if (num < 1 || num > 255) {
                        // ignore?
                        //retwarn |= 1;
                        break;
                    }
                    // message text is next
                    token = NextToken();
                    // ignore- doesn't matter
                    break;
                default:
                    // ignore any other token
                    break;
                }

                token = NextToken();
            }
            return retval;

            string NextToken() {
                // skip white space until a char is found
                char c;
                string token;
                do {
                    if (pos >= definetext.Length) {
                        return "";
                    }
                    c = definetext[pos++];
                    // if comment, skip to end of line, then
                    // try again
                    if (c == '[') {
                        token = "";
                        do {
                            if (pos >= definetext.Length) {
                                return "";
                            }
                            c = definetext[pos++];
                            token += c;
                        } while (c != '\n');
                        // check for potential color defines
                        if (subtype == DecodeSubtype.None) {
                            if (token.Contains("colors", StringComparison.OrdinalIgnoreCase)) {
                                subtype = DecodeSubtype.Colors;
                            }
                        }
                    }
                } while (c == ' ' || c == '\t' || c == '\n' || c == '\r');
                token = c.ToString();
                // if quote, add until true ending quote found
                if (c == '\"') {
                    bool slash = false;
                    do {
                        if (pos >= definetext.Length) {
                            return token;
                        }
                        c = definetext[pos++];
                        // ignore line breaks
                        if (c != '\r' && c != '\n') {
                            if (slash) {
                                slash = false;
                                if (c == 'n') {
                                    c = '\n';
                                }
                            }
                            else {
                                if (c == '\\') {
                                    slash = true;
                                    continue;
                                }
                                if (c == '\"') {
                                    token += c;
                                    break;
                                }
                            }
                            token += c;
                        }
                    } while (true);
                }
                else if (c == '(' || c == ',' || c == ';' || c == ')') {
                    return token;
                }
                else {
                    // any other char type, keep adding chars until
                    // '(', ',', ')', ';' or whitespace found (no need to
                    // handle other single char tokens since they
                    // are never used in defines
                    do {
                        if (pos >= definetext.Length) {
                            return token;
                        }
                        c = definetext[pos++];
                        if (c == '(' || c == ',' || c == ';' || c == ')') {
                            // return to queue
                            pos--;
                            break;
                        }
                        if (c == ' ' || c == '\t' || c == '\n' || c == '\r') {
                            break;
                        }
                        token += c;
                    } while (true);
                }
                return token;
            }

            ArgType[] ArgsFromText() {
                ArgType[] args = [];
                if (NextToken() != "(") {
                    // invalid
                    return null;
                }
                string argname = NextToken();
                if (argname == ")") {
                    return args;
                }
                bool needarg = true;
                do {
                    if (needarg) {
                        switch (argname) {
                        case "FLAG":
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = Flag;
                            break;
                        case "OBJECT":
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = ArgType.Object;
                            break;
                        case "NUM":
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = Num;
                            break;
                        case "MSGNUM":
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = MsgNum;
                            break;
                        case "VIEW":
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = ArgType.View;
                            break;
                        case "VAR":
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = Var;
                            break;
                        case "WORDLIST":
                            Array.Resize(ref args, args.Length + 1);
                            args[^1] = ArgType.VocWrd;
                            break;
                        case "MSG":
                        case "WORD":
                        case "ANY":
                            // invalid arg type
                            return null;
                        default:
                            // undefined arg type
                            return null;
                        }
                    }
                    else {
                        if (argname == ")") {
                            return args;
                        }
                        else if (argname != ",") {
                            return null;
                        }
                    }
                    needarg = !needarg;
                    argname = NextToken();
                } while (true);
            }
        }
        #endregion
    }

    public class LogicDecodeBufferOverflowException : Exception {
        public LogicDecodeBufferOverflowException(string message) : base(message) { }
    }

    internal class DecodeDefineList : Dictionary<string, LogicDecoder.DecodeDefine> {

        public string ArgName(ArgType argType, LogicDecoder.DecodeSubtype subType, int value) {
            foreach (var kvp in this) {
                if (kvp.Value.Type == argType &&
                    kvp.Value.SubType == subType &&
                    kvp.Value.Value == value) {
                    return kvp.Key;
                }
            }
            // not found
            return "";
        }

        public string ArgName(ArgType argType, int value) {
            return ArgName(argType, 0, value);
        }

        public bool TryGetName(ArgType argType, LogicDecoder.DecodeSubtype subType, int value, ref string Name) {
            foreach (var kvp in this) {
                if (kvp.Value.Type == argType &&
                    kvp.Value.SubType == subType &&
                    kvp.Value.Value == value) {
                    Name = kvp.Key;
                    return true;
                }
            }
            // not found
            return false;
        }

        public bool TryGetName(ArgType argType, int value, ref string Name) {
            return TryGetName(argType, 0, value, ref Name);
        }
    }
}
