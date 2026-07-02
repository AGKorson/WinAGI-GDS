using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.LogicErrorLevel;

namespace WinAGI.Engine {
    /// <summary>
    /// This class contains all the members and methods needed to compile logic 
    /// source code using Sierra's original AGI syntax rules as defined in their
    /// compiler CG.EXE, v3.14
    /// </summary>
    public static class SierraLogicCompiler {
        #region Enums
        private enum TokenType {
            None = 0,
            Keyword,
            Symbol,
            Number,
            Text,
            Flag,
            Variable,
            Object, // (inv or screen)
            View,
            TestCmd,
            ActionCmd,
        }
        #endregion

        #region Structs
        private struct LogicGoto {
            internal string LabelName;
            internal int DataLoc;
            internal int Line;
        }

        private struct LogicLabel {
            internal string Name;
            internal int Pos;
            internal int GotoCount;
        }

        private class CompilerBlockType {
            internal bool IsIf = false;
            internal int StartPos = 0;
            internal int Length = 0;
            public CompilerBlockType() {
            }
        }

        private struct SierraToken {
            internal ArgType Type = None;
            internal int Value = 0;
            internal string Text = "";
            internal ArgType[] ArgList = [];
            internal string Name = "";
            internal readonly bool Redefined = false;
            internal int StartPos = -1;
            internal int Line = 0;

            /// <summary>
            /// Blank constructor used when declaring a token that will be
            /// assigned new values later, but needs to be non-null to
            /// avoid compiler errors.
            /// </summary>
            public SierraToken() {
                // 
            }

            /// <summary>
            /// Creates a new token that is not a redefine and that doesn't need
            /// an argument list.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="text"></param>
            /// <param name="value"></param>
            public SierraToken(ArgType type, string text, int value) {
                Type = type;
                Name = Text = text;
                Value = value;
                ArgList = [];
            }

            /// <summary>
            /// Creates a new token that is not a redefine, and that has a list
            /// of arguments (for test and action command declarations).
            /// </summary>
            /// <param name="type"></param>
            /// <param name="text"></param>
            /// <param name="value"></param>
            /// <param name="argList"></param>
            public SierraToken(ArgType type, string text, int value, ArgType[] argList) {
                Type = type;
                Name = Text = text;
                Value = value;
                ArgList = new ArgType[argList.Length];
                if (argList.Length > 0) {
                    Array.Copy(argList, ArgList, argList.Length);
                }
            }

            /// <summary>
            /// Creates a token that redefines an existing token, assigning its define
            /// name. This is for defines that don't need an argument list.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            /// <param name="text"></param>
            /// <param name="value"></param>
            public SierraToken(string name, ArgType type, string text, int value) {
                this.Name = name;
                // not a redefine unless name and text are different?
                Redefined = name != text;
                Type = type;
                Text = text;
                Value = value;
                ArgList = [];
            }

            /// <summary>
            /// Creates a token that redefines an existing token, assigning its define
            /// name. This is for defines that require an argument list (test and
            /// action command defines).
            /// </summary>
            /// <param name="name"></param>
            /// <param name="type"></param>
            /// <param name="text"></param>
            /// <param name="value"></param>
            /// <param name="argList"></param>
            public SierraToken(string name, ArgType type, string text, int value, ArgType[] argList) {
                this.Name = name;
                // not a redefine unless name and text are different?
                Redefined = name != text;
                Type = type;
                Text = text;
                Value = value;
                ArgList = new ArgType[argList.Length];
                if (argList.Length > 0) {
                    Array.Copy(argList, ArgList, argList.Length);
                }
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The parent game attached to the compiler.
        /// </summary>
        private static AGIGame sCompGame;
        private static Logic compLogic;
        private static string[] sourcefile;
        private static List<string> includedFiles = [];
        private static string[] sourcetext;
        private static int[] pos = [-1, -1, -1, -1, -1];
        private static int[] sourceline = [1, 1, 1, 1, 1];
        private static int sourcelevel = 0;
        private const int MAX_SOURCE_LEVELS = 5;
        private static bool wordsloaded = false;
        private static bool[] ActionCommandDefined = new bool[12];
        private static bool[] TestCommandDefined = new bool[8];
        private static Logic tmpLogRes;
        private static byte complogicNumber;
        private static bool minorError;
        private static int menuitemCount = -1, menuWidth = 0;
        private static string lastMenu = "";
        private static bool menuSet = false;
        private static int endingCmd = 0; // 1 = return, 2 = new.room, 3 = quit
        private static string[] MsgText;
        private static bool[] MsgInUse;
        private static int[] MsgWarnings; // to track warnings found during msgread function
        private static Dictionary<string, SierraToken> definesList;
        #endregion

        #region Methods

        /// <summary>
        /// This method compiles the sourcetext for the specified logic.
        /// Errors and warnings are added as they are encountered. If 
        /// successful, SourceLogic data is updated and the method returns
        /// true. If not successful, false is returned.
        /// </summary>
        /// <param name="SourceLogic"></param>
        /// <returns></returns>
        internal static bool CompileLogic(Logic SourceLogic) {
            sCompGame = SourceLogic.parent;
            compLogic = SourceLogic;
            complogicNumber = SourceLogic.Number;
            sourcelevel = 0;
            sourcefile = [compLogic.SourceFile, "", "", "", ""];
            sourcetext = [SourceLogic.SourceText, "", "", "", ""];
            pos = [0, -1, -1, -1, -1];
            sourceline = [0, 0, 0, 0, 0];
            definesList = [];
            includedFiles = [];
            wordsloaded = false;
            MsgText = new string[256];
            MsgInUse = new bool[256];
            MsgWarnings = new int[256];
            minorError = false;
            tmpLogRes = new Logic {
                Data = []
            };
            // write a place holder 2-byte (word) value for offset to msg section start
            tmpLogRes.WriteWord(0, 0);
            try {
                // main agi compiler
                if (CompileSIERRA()) {
                    // code size equals bytes currently written (before msg secion added)
                    tmpLogRes.CodeSize = tmpLogRes.Size;
                    // add message section
                    if (WriteMsgs()) {
                        if (!minorError) {
                            // no errors, save compiled data
                            SourceLogic.Data = tmpLogRes.Data;
                            SourceLogic.CompiledCRC = SourceLogic.CRC;
                            sCompGame.WriteSettingNoSave("Logic" + (SourceLogic.Number).ToString(), "CRC32", "0x" + SourceLogic.CRC.ToString("x8"), "Logics");
                            sCompGame.WriteSettingNoSave("Logic" + (SourceLogic.Number).ToString(), "CompCRC32", "0x" + SourceLogic.CompiledCRC.ToString("x8"), "Logics");
                        }
                    }
                    ResetCompiler();
                    return !minorError;
                }
                else {
                    // critical error during compile
                    ResetCompiler();
                    return false;
                }
            }
            catch (Exception) {
                ResetCompiler();
                throw;
            }

            static void ResetCompiler() {
                tmpLogRes.Unload();
                tmpLogRes = null;
                sCompGame = null;
                sourcefile = null;
                sourcetext = null;
                compLogic = null;
                includedFiles = null;
                definesList = null;
                MsgText = null;
                MsgInUse = null;
                MsgWarnings = null;
            }
        }

        /// <summary>
        /// This is the main compiler function. It steps through input source
        /// code one token at a time and converts it to AGI logic byte code.
        /// </summary>
        /// <returns>true if source is successfully compiled, false if any 
        /// errors are encountered.</returns>
        private static bool CompileSIERRA() {
            bool quitcheck = false, newroomcheck = false;
            bool lastCmdRtn = false;
            byte[] arglist = new byte[8];
            List<CompilerBlockType> Block = [];
            CompilerBlockType lastBlock = new();
            Dictionary<string, LogicLabel> Labels = [];
            List<LogicGoto> Gotos = [];
            SierraToken prevToken = new();
            SierraToken argToken;
            pos[0] = 0;
            endingCmd = 0;
            menuitemCount = -1;
            menuWidth = 0;
            lastMenu = "";
            menuSet = false;

            // assign predefined tokens
            AddDefaultTokens();
            // get the first token
            SierraToken nextToken = NextConvertedToken();
            // process tokens until end reached
            while (nextToken.Type != None) {
                // ignore separators and get next token
                if (nextToken.Type == Separator) {
                    // ignore, treat as whitespace
                    if (nextToken.Text == ",") {
                        // commas are allowed but not preferred
                        AddWarning(7018);
                    }
                    nextToken = NextConvertedToken();
                    continue;
                }
                lastCmdRtn = false;
                if (endingCmd > 0) {
                    // preprocessors are ok
                    if (nextToken.Type != Preprocessor) {
                        if (nextToken.Text != "}") {
                            if (ErrorLevel == Medium) {
                                switch (endingCmd) {
                                case 1:
                                    // return
                                    AddWarning(5097);
                                    break;
                                case 2:
                                    // new.room
                                    if (nextToken.Type == TestCmd && nextToken.Value == 0) {
                                        // possible end of logic; in 
                                        // that case, no need for a warning here
                                        // UNLESS this isn't the end, so use a
                                        // flag to check for the condition
                                        newroomcheck = true;
                                    }
                                    else {
                                        AddWarning(5095);
                                    }
                                    break;
                                case 3:
                                    // unconditional quit
                                    if (nextToken.Type == TestCmd && nextToken.Value == 0) {
                                        // possible end of logic; in 
                                        // that case, no need for a warning here
                                        // UNLESS this isn't the end, so use a
                                        // flag to check for the condition
                                        quitcheck = true;
                                    }
                                    else {
                                        AddWarning(5111);
                                    }
                                    break;
                                }
                            }
                        }
                        endingCmd = 0;
                    }
                }
                switch (nextToken.Type) {
                case Preprocessor:
                    // value indicates which preprocessor
                    AddDefine(nextToken.Value);
                    break;
                case Symbol:
                    switch (nextToken.Text) {
                    case "(":
                    case ")":
                        // ignored in Sierra syntax
                        AddWarning(7012, EngineResourceByNum(7012).Replace(
                            ARG1, nextToken.Text));
                        break;
                    case "{":
                        // only allowed after 'if' or 'else'
                        if (prevToken.Text != "if" && prevToken.Text != "else") {
                            // but ignored in Sierra syntax
                            AddWarning(7012, EngineResourceByNum(7012).Replace(
                                ARG1, "}"));
                        }
                        break;
                    case "}":
                        if (Block.Count == 0) {
                            // no block currently open
                            AddError(4008, false);
                        }
                        else {
                            if (tmpLogRes.Size == Block[^1].StartPos + 2) {
                                // block is only two bytes long, meaning no commands
                                if (ErrorLevel == Medium) {
                                    AddWarning(5001);
                                }
                            }
                            // calculate and write block length
                            Block[^1].Length = tmpLogRes.Size - Block[^1].StartPos - 2;
                            tmpLogRes.WriteWord((ushort)Block[^1].Length, Block[^1].StartPos);
                            // remove block from stack
                            lastBlock = Block[^1];
                            Block.RemoveAt(Block.Count - 1);
                        }
                        break;
                    case ":":
                        // next token should be the label
                        // (raw token, on same line, separator characters are NOT
                        // ignored)
                        nextToken = NextLabelToken();
                        // because token is read without being converted to a define,
                        // the type will only be an identifier, symbol, number (or
                        // null if nothing found)
                        switch (nextToken.Type) {
                        case None:
                            // might be end of input
                            if (pos[sourcelevel] >= sourcetext[sourcelevel].Length && sourcelevel == 0) {
                                // nothing left, return critical error
                                AddError(4057, true);
                                return false;
                            }
                            else {
                                // null labels can be defined in cg.exe, but are
                                // unusable
                                AddWarning(7011);
                            }
                            break;
                        case Unknown:
                            // should only be an existing label, or an identifier
                            if (Labels.ContainsKey(nextToken.Text)) {
                                // duplicate OR previously added by a goto
                                if (Labels[nextToken.Text].Pos == -1) {
                                    // added by goto- update location
                                    Labels[nextToken.Text] = new LogicLabel {
                                        Name = nextToken.Text,
                                        Pos = tmpLogRes.Size,
                                        GotoCount = 0
                                    };
                                }
                                else {
                                    // duplicate not allowed
                                    AddError(4016, EngineResourceByNum(4016).Replace(
                                        ARG1, nextToken.Text), false);
                                }
                            }
                            else if (definesList.TryGetValue(nextToken.Text, out SierraToken value)) {
                                // shouldn't be a define value
                                string typename = value.Type switch {
                                    Preprocessor => "a preprocessor token",
                                    Keyword or SierraArgToken => "a keyword",
                                    _ => "a defined token",
                                };
                                AddError(4015, EngineResourceByNum(4015).Replace(
                                    ARG1, nextToken.Text).Replace(
                                    ARG2, typename), false);
                            }
                            else {
                                if (Labels.Count >= 50) {
                                    // too many
                                    AddError(6025, EngineResourceByNum(6025), false);
                                }
                                LogicLabel label = new() {
                                    Name = nextToken.Text,
                                    Pos = tmpLogRes.Size,
                                    GotoCount = 0
                                };
                                Labels.Add(label.Name, label);
                            }
                            break;
                        case Num:
                            // number can't be a label
                            AddError(6030, EngineResourceByNum(6030).Replace(
                                ARG1, nextToken.Name), false);
                            break;
                        default:
                            // shouldn't be possible
                            Debug.Assert(false);
                            break;
                        }
                        break;
                    default:
                        // invalid symbol
                        AddError(4023, EngineResourceByNum(4023).Replace(
                            ARG1, nextToken.Name), false);
                        // ignore rest of this line
                        SkipToNextLine(nextToken);
                        break;
                    }
                    break;
                case AssignOperator:
                    switch (nextToken.Text) {
                    case "++" or "--":
                        // unary operators
                        if (nextToken.Text == "++") {
                            if (!ActionCommandDefined[1]) {
                                AddError(6016, EngineResourceByNum(6016).Replace(
                                    ARG1, ActionCommands[1].FanName), false);
                            }
                            tmpLogRes.WriteByte(1);
                        }
                        else {
                            if (!ActionCommandDefined[2]) {
                                AddError(6016, EngineResourceByNum(6016).Replace(
                                    ARG1, ActionCommands[2].FanName), false);
                            }
                            tmpLogRes.WriteByte(2);
                        }
                        // get variable
                        argToken = NextConvertedToken();
                        if (nextToken.Type == None) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            return false;
                        }
                        if (argToken.Type != Var) {
                            AddError(4025, false);
                            break;
                        }
                        // write the variable value
                        tmpLogRes.WriteByte((byte)argToken.Value);
                        break;
                    default:
                        // invalid symbol (use the symbol text, not its define name here)
                        AddError(4023, EngineResourceByNum(4023).Replace(
                            ARG1, nextToken.Text), false);
                        SkipToNextLine(nextToken);
                        break;
                    }
                    break;
                case Keyword:
                    CompilerBlockType block;
                    switch (nextToken.Value) {
                    case 0: // "if"
                        bool retval, eoferr;
                        (retval, eoferr) = CompileIf();
                        if (retval) {
                            block = new() {
                                StartPos = tmpLogRes.Pos,
                                IsIf = true
                            };
                            // block nest level limit is 10
                            if (Block.Count >= 10) {
                                AddError(6029, false);
                            }
                            Block.Add(block);
                            // write placeholder for block length
                            tmpLogRes.WriteWord(0);
                            // next token should be a bracket
                            if (!CheckToken("{")) {
                                AddError(4030, false);
                                // if a stray ')' skip it
                                if (CheckToken(")")) {
                                    // and check for bracket
                                    _ = CheckToken("{");
                                }
                            }
                        }
                        // check for critical error (eof)
                        if (eoferr) {
                            return false;
                        }
                        break;
                    case 1: // "else"
                        if (prevToken.Text == "}" && lastBlock.IsIf) {
                            // adjust the 'if' block length to accomodate the 'else' statement
                            tmpLogRes.WriteWord((ushort)(lastBlock.Length + 3), lastBlock.StartPos);
                            // now add the 'else' block
                            Block.Add(new());
                            Block[^1].IsIf = false;
                            // write the 'else' bytecode
                            tmpLogRes.WriteByte(0xFE);
                            Block[^1].StartPos = tmpLogRes.Pos;
                            // placeholder for block length
                            tmpLogRes.WriteWord(0);
                            // next token better be a bracket
                            if (!CheckToken("{")) {
                                AddError(4030, false);
                            }
                        }
                        else {
                            // normally else can only follow a close bracket,
                            // but Sierra syntax allows them anywhere, treating
                            // them as a 'goto-end-of-bracket'
                            if (prevToken.Text != "}") {
                                AddWarning(7017);
                            }
                            else {
                                // previous block was not an if; normally not 
                                // allowed, but in Sierra syntax it's ok
                                AddWarning(7017);
                            }
                            // add the goto code
                            tmpLogRes.WriteByte(0xFE);
                            // add a block, mark it as not-if
                            block = new() {
                                StartPos = tmpLogRes.Pos,
                                IsIf = false
                            };
                            // block nest level limit is 10
                            if (Block.Count >= 10) {
                                AddError(6029, false);
                            }
                            Block.Add(block);
                            // write placeholder for block length
                            tmpLogRes.WriteWord(0);
                            // next token should be a bracket
                            if (!CheckToken("{")) {
                                AddError(4030, false);
                            }
                        }
                        break;
                    case 2: // "goto"
                        argToken = NextInFileToken();
                        switch (argToken.Type) {
                        case None:
                            // nothing left, return critical error
                            AddError(4057, true);
                            return false;
                        case Unknown:
                            // check label list to see if already defined
                            if (Labels.ContainsKey(argToken.Text)) {
                                // if not yet defined, increment goto count
                                if (Labels[argToken.Text].GotoCount > 0) {
                                    LogicLabel update = new() {
                                        Name = Labels[argToken.Text].Name,
                                        Pos = Labels[argToken.Text].Pos,
                                        GotoCount = Labels[argToken.Text].GotoCount
                                    };
                                    update.GotoCount++;
                                    Labels[argToken.Text] = update;
                                    if (update.GotoCount > 19) {
                                        // error
                                        AddError(6027, false);
                                    }
                                }
                            }
                            else {
                                // verify it's not already defined
                                if (definesList.ContainsKey(argToken.Text)) {
                                    // argument is NOT a valid label (use token text here)
                                    AddError(6031, EngineResourceByNum(6031).Replace(
                                        ARG1, argToken.Text), false);
                                    // but add it anyway to continue
                                }
                                else {
                                }
                                // add to label list
                                LogicLabel newlabel = new() {
                                    Name = argToken.Text,
                                    Pos = -1,
                                    GotoCount = 1
                                };
                                Labels.Add(newlabel.Name, newlabel);
                            }
                            // save this goto info on goto stack
                            // write goto command byte
                            tmpLogRes.WriteByte(0xFE);
                            LogicGoto addgoto = new() {
                                LabelName = argToken.Text,
                                DataLoc = tmpLogRes.Size,
                                Line = sourceline[sourcelevel]
                            };
                            Gotos.Add(addgoto);
                            // write placeholder for amount of offset
                            tmpLogRes.WriteWord(0);
                            break;
                        default:
                            // argument of this type can't be used as a label
                            AddError(6030, EngineResourceByNum(6030).Replace(
                                ARG1, argToken.Text), false);
                            break;
                        }
                        break;
                    }
                    break;
                case ActionCmd:
                    // write the command code,
                    byte cmdnum = (byte)nextToken.Value;
                    tmpLogRes.WriteByte(cmdnum);
                    // next token should be "("
                    // need to fix checkchar to use defines...
                    if (!CheckToken("(")) {
                        AddError(4027, false);
                    }
                    // now extract arguments (assume all OK)
                    bool argsOK = true;
                    for (int i = 0; i < nextToken.ArgList.Length; i++) {
                        argToken = NextConvertedToken();
                        if (argToken.Type == None) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            return false;
                        }
                        if (i > 0) {
                            // should be a separator(';' and ',' are
                            // interchangeable in sierra syntax)
                            if (argToken.Type == Separator) {
                                // but ',' is preferred
                                if (argToken.Text == ";") {
                                    AddWarning(7019);
                                }
                                // next token is the argument
                                argToken = NextConvertedToken();
                                if (argToken.Type == None) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    return false;
                                }
                            }
                            else {
                                // missing separator
                                // use 1-base arg values (but since referring to
                                // previous arg, don't increment arg index)
                                AddError(4026, EngineResourceByNum(4026).Replace(
                                    ARG1, i.ToString()).Replace(
                                    ARG2, ActionCommands[cmdnum].FanName), false);
                                // assume the current token is the argument
                            }
                        }
                        // validate type
                        int argval = argToken.Value;
                        // messagenum args appear as type 'Num'; need to
                        // account for that
                        if (argToken.Type == nextToken.ArgList[i] ||
                            (argToken.Type == Num && nextToken.ArgList[i] == MsgNum)) {
                            // validate argument value
                            ValidateArgType(argval, argToken.Type, i);
                            arglist[i] = (byte)argval;
                        }
                        else {
                            if (argToken.Type == Separator ||
                                argToken.Text == ")") {
                                // missing or empty argument
                                AddError(4031, EngineResourceByNum(4031).Replace(
                                    ARG1, (i + 1).ToString()).Replace(
                                    ARG2, nextToken.Name).Replace(
                                    ARG3, ArgTypeName(nextToken.ArgList[i])), false);
                                // put back the separator or ')'
                                pos[sourcelevel]--;
                                // if ')', end the loop early
                                if (argToken.Text == ")") {
                                    break;
                                }
                            }
                            else {
                                // error depends on token type
                                switch (argToken.Type) {
                                // arg type, but wrong one
                                case Num:
                                case DefStr:
                                    // is it a define?
                                    if (argToken.Redefined) {
                                        AddError(4037, EngineResourceByNum(4037).Replace(
                                            ARG1, (i + 1).ToString()).Replace(
                                            ARG2, ArgTypeName(nextToken.ArgList[i])).Replace(
                                            ARG3, argToken.Name), false);
                                    }
                                    else {
                                        AddError(4017, EngineResourceByNum(4017).Replace(
                                            ARG1, (i + 1).ToString()).Replace(
                                            ARG2, ArgTypeName(nextToken.ArgList[i])), false);
                                    }
                                    break;

                                case Symbol:
                                case Separator:
                                case TestOperator:
                                case BadSymbol:
                                case AssignOperator:
                                    AddError(4040, EngineResourceByNum(4040).Replace(
                                        ARG1, (i + 1).ToString()).Replace(
                                        ARG2, ArgTypeName(nextToken.ArgList[i])), false);
                                    break;

                                case SierraArgToken:
                                case Keyword:
                                case Preprocessor:
                                    AddError(6026, EngineResourceByNum(6026).Replace(
                                        ARG1, argToken.Name), false);
                                    break;

                                case BadString:
                                    // not a good string
                                    AddError(4050, false);
                                    break;

                                case Unknown:
                                    AddError(4004, EngineResourceByNum(4004).Replace(
                                        ARG1, (i + 1).ToString()).Replace(
                                        ARG2, ArgTypeName(nextToken.ArgList[i])).Replace(
                                        ARG3, argToken.Text), false);
                                    break;

                                default:
                                    // none of the others should be possible
                                    //case Var:
                                    //case Flag:
                                    //case MsgNum:
                                    //case SObj:
                                    //case InvItem:
                                    //case Str:
                                    //case Word:
                                    //case Ctrl:
                                    //case VocWrd:
                                    //case ActionCmd:
                                    //case TestCmd:
                                    //case ArgType.Object:
                                    //case ArgType.View:
                                    //case MSG:
                                    //case WORD:
                                    //case ANY:
                                    //case WORDLIST:
                                    //case Label:
                                    //case Comment:
                                    //case None:
                                    Debug.Assert(false);
                                    AddError(4037, EngineResourceByNum(4037).Replace(
                                        ARG1, (i + 1).ToString()).Replace(
                                        ARG2, ArgTypeName(nextToken.ArgList[i])).Replace(
                                        ARG3, argToken.Name), false);
                                    break;
                                }
                                // wrong token
                            }
                            // use a placeholder value
                            arglist[i] = 0;
                            argsOK = false;
                        }
                        // write argument
                        tmpLogRes.WriteByte(arglist[i]);
                    }
                    if (argsOK) {
                        // validate arguments for this command
                        ValidateArgs(cmdnum, arglist);
                    }
                    // next character must be ")"
                    if (!CheckToken(")")) {
                        AddError(4084, false);
                    }
                    // check for return command
                    if (cmdnum == 0) {
                        lastCmdRtn = true;
                        if (newroomcheck || quitcheck) {
                            // only ok if last cmd in block and block is 0 
                            if (Block.Count != 0) {
                                if (newroomcheck) {
                                    AddWarning(5095);
                                }
                                if (quitcheck) {
                                    AddWarning(5111, EngineResourceByNum(5111));
                                }
                            }
                        }
                        quitcheck = false;
                        newroomcheck = false;
                    }
                    break;
                case Var:
                    if (!CompileSpecial(nextToken)) {
                        // critical error
                        return false;
                    }
                    break;
                default:
                    switch (nextToken.Type) {
                    case Num:
                    case DefStr:
                    case BadString:
                        // numeric value
                        AddError(6017, EngineResourceByNum(6017).Replace(
                            ARG1, nextToken.Name), false);
                        break;
                    case TestCmd:
                        AddError(4062, EngineResourceByNum(4062).Replace(
                            ARG1, nextToken.Name), false);
                        nextToken = NextConvertedToken();
                        if (nextToken.Type == None) {
                            // nothing left
                            break;
                        }
                        if (nextToken.Text == "(") {
                            // assume test cmd name out of order, skip to ')'
                            do {
                                nextToken = NextConvertedToken();
                                if (nextToken.Type == None) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    return false;
                                }
                                if (nextToken.Text == ")" ||
                                    nextToken.Text == ";") {
                                    break;
                                }
                            } while (nextToken.Type != None);
                        }
                        else {
                            // put this token back for next pass
                            pos[sourcelevel] = nextToken.StartPos;
                        }
                        break;
                    case TestOperator:
                        // '==', '!=', '<', '<=', '>', '>='
                        // invalid symbol (use token text here)
                        AddError(4023, EngineResourceByNum(4023).Replace(
                            ARG1, nextToken.Text), false);
                        break;
                    case Unknown:
                        // used when type is cannot be determined
                        // unknown command - check for preprocessor symbol
                        // (use token text here)
                        if (nextToken.Text[0] == '#' || nextToken.Text[0] == '%') {
                            AddError(4036, EngineResourceByNum(4036).Replace(
                                ARG1, nextToken.Text), false);
                        }
                        else {
                            SierraToken peek = NextInLineConvertedToken();
                            if (peek.Text == "(") {
                                // check for possible misspelled command
                                // assume bad cmd name (use token text here)
                                AddError(4082, EngineResourceByNum(4082).Replace(
                                    ARG1, nextToken.Text), false);
                            }
                            else {
                                // unknown syntax error (use token text here)
                                AddError(4064, EngineResourceByNum(4064).Replace(
                                    ARG1, "Undefined token '" + nextToken.Text + "'"), false, nextToken.Line);
                            }
                            // put the peek token back
                            pos[sourcelevel] = peek.StartPos;
                        }
                        break;
                    }
                    SkipToNextLine(nextToken);
                    break;
                }
                prevToken = nextToken;
                nextToken = NextConvertedToken();
            }
            // if a return was already coded, warn user
            if (lastCmdRtn) {
                AddWarning(7009);
            }
            else {
                // add a return command
                tmpLogRes.WriteByte(0);
            }
            // check to see if everything was wrapped up properly
            if (Block.Count > 0) {
                //errorLine = lastLine;
                AddError(4007, false);
                return false;
            }
            // write in goto values
            for (int i = 0; i < Gotos.Count; i++) {
                // check that label exists (its value won't be -1)
                if (Labels[Gotos[i].LabelName].Pos == -1) {
                    AddError(4012, EngineResourceByNum(4012).Replace(ARG1,
                        Gotos[i].LabelName), false, Gotos[i].Line);
                }
                else {
                    int jump = Labels[Gotos[i].LabelName].Pos - Gotos[i].DataLoc - 2;
                    // need to convert it to an unsigned short Value so negative
                    // jump values work correctly
                    tmpLogRes.WriteWord((ushort)jump, Gotos[i].DataLoc);
                }
            }
            return true;
        }

        private static void AddDefine(int type) {
            SierraToken next;
            string name;

            if (type == 0) {
                // include "filename" (can't be a redefine)
                next = NextInFileToken();
                if (next.Type != DefStr) {
                    if (next.Text == "") {
                        AddError(4035, EngineResourceByNum(4035), false);
                    }
                    else {
                        AddError(4069, EngineResourceByNum(4069), false);
                    }
                    SkipToNextLine(next);
                    return;
                }
                // validate filename
                string includeFilename = Path.GetFullPath(Path.Combine(sCompGame.SrcResDir, next.Text.Trim('"')));
                if (File.Exists(includeFilename)) {
                    // can't be source file
                    if (includeFilename.Equals(sourcefile[0], StringComparison.OrdinalIgnoreCase)) {
                        AddError(4088, false);
                        return;
                    }
                    // verify it's not already included
                    if (includedFiles.Contains(includeFilename, StringComparer.OrdinalIgnoreCase)) {
                        AddError(6024, EngineResourceByNum(6024).Replace(
                            ARG1, includeFilename), false);
                        return;
                    }
                    if (sourcelevel + 1 >= MAX_SOURCE_LEVELS) {
                        AddError(6001, EngineResourceByNum(6001), false);
                        return;
                    }
                    // read file and push onto source stack
                    sourcelevel++;
                    sourcefile[sourcelevel] = includeFilename;
                    // now open the include file, and get the text
                    try {
                        sourcetext[sourcelevel] = File.ReadAllText(includeFilename);
                    }
                    catch (Exception) {
                        AddError(4032, EngineResourceByNum(4032).Replace(ARG1, includeFilename), true);
                        sourcelevel--;
                        return;
                    }
                    pos[sourcelevel] = 0;
                    sourceline[sourcelevel] = 0;
                    // add it to the included files list
                    includedFiles.Add(includeFilename);
                }
                else {
                    AddError(4028, EngineResourceByNum(4028).Replace(ARG1, includeFilename), false);
                }
            }
            else if (type == 1) {
                // tokens "words.tok" (can't be a redefine)
                next = NextInFileToken();
                if (next.Type != DefStr) {
                    if (next.Text == "") {
                        AddError(6002, EngineResourceByNum(6002).Replace(
                            ARG1, "is missing"), false);
                    }
                    else {
                        AddError(6003, EngineResourceByNum(6003), false);
                    }
                    SkipToNextLine(next);
                    return;
                }
                // validate filename
                string filename = next.Text.Trim('"').Trim();
                if (filename.Length == 0) {
                    AddError(6002, EngineResourceByNum(6002).Replace(
                        ARG1, "is missing"), false);
                    return;
                }
                else if (!Path.GetFullPath(Path.Combine(sCompGame.SrcResDir, filename)).Equals(sCompGame.agVocabWords.ResFile, StringComparison.OrdinalIgnoreCase)) {
                    AddError(6002, EngineResourceByNum(6002).Replace(
                        ARG1, "'" + filename + "' is invalid"), false);
                    return;
                }
                // check if already loaded
                if (wordsloaded) {
                    AddWarning(7001, EngineResourceByNum(7001));
                    return;
                }
                wordsloaded = true;
            }
            else if (type == 8) {
                // message number "text string"
                // number is next
                bool error = false;
                next = NextConvertedToken();
                if (next.Type != Num) {
                    AddError(6007, EngineResourceByNum(6007).Replace(
                        ARG1, "message"), false);
                    // don't add
                    return;
                }
                int msgNum = next.Value;
                // validate number (1 - 255)
                if (msgNum < 0 || msgNum > 255) {
                    AddError(4048, EngineResourceByNum(4048), false);
                    error = true;
                }
                else if (msgNum == 0) {
                    // can't have a msg 0 or error will occur
                    AddError(6015, EngineResourceByNum(6015), false);
                    error = true;
                }
                else {
                    if (MsgInUse[msgNum]) {
                        // msg is already assigned
                        AddError(4072, EngineResourceByNum(4072).Replace(ARG1, msgNum.ToString()), false);
                        error = true;
                    }
                }
                // then get msg text
                next = NextConvertedToken();
                if (next.Type != DefStr) {
                    AddError(4051, EngineResourceByNum(4051), false);
                    // don't add
                    return;
                }
                if (!error) {
                    // strip off quotes
                    string msgText = next.Text[1..^1];
                    // replace slash codes with single characters
                    MsgText[msgNum] = msgText;
                    ValidateMsgChars(msgText, msgNum);
                    MsgInUse[msgNum] = true;
                }

            }
            else {
                ArgType argtype = None;
                string typename = "";
                bool error = false;
                switch (type) {
                case 2: // test   cmdname(arglist) number
                    typename = "test";
                    argtype = TestCmd;
                    break;
                case 3: // action cmdname(arglist) number
                    typename = "action";
                    argtype = ActionCmd;
                    break;
                case 4: // flag name number
                    typename = "flag";
                    argtype = Flag;
                    break;
                case 5: // var name number
                    typename = "var";
                    argtype = Var;
                    break;
                case 6: // object name number
                    typename = "object";
                    argtype = ArgType.Object;
                    break;
                case 9: // view name number
                    typename = "view";
                    argtype = ArgType.View;
                    break;
                }
                // name can't be a redefine
                next = ConvertToken(NextInFileToken());
                // type must be Unknown, meaning it's a valid identifier
                // and hasn't been redefined
                if (next.Type != Unknown) {
                    // check for duplicate first
                    if (definesList.ContainsKey(next.Name)) {
                        // invalid name; already defined
                        if ((definesList[next.Name].Type == Keyword ||
                            definesList[next.Name].Type == Preprocessor ||
                            definesList[next.Name].Type == SierraArgToken) &&
                            next.Name == next.Text) {
                            // if name and text match, and it's a default type,
                            // then it's an attempt to redefine a default type
                            AddError(6026, EngineResourceByNum(6026).Replace(
                                ARG1, next.Name), false);
                        }
                        else {
                            // otherwise it's an attempt to redefine a previous 
                            // define
                            AddError(6004, EngineResourceByNum(6004).Replace(
                                ARG1, next.Name), false);
                        }
                    }
                    else {
                        // if not a duplicate then it's an invalid type
                        AddError(6012, EngineResourceByNum(6012).Replace(
                            ARG1, typename), false);
                    }
                    // skip rest of line
                    SkipToNextLine(next);
                    return;
                }
                name = next.Text;
                int num;
                switch (type) {
                case 2: // test   cmdname(arglist) number
                case 3: // action cmdname(arglist) number
                    // get arglist
                    next = NextConvertedToken();
                    if (next.Text != "(") {
                        AddError(6005, EngineResourceByNum(6005).Replace(ARG1, name), false);
                        error = true;
                    }
                    // parse arglist
                    List<ArgType> arglist = [];
                    do {
                        next = NextConvertedToken();
                        if (next.Type == SierraArgToken) {
                            arglist.Add((ArgType)next.Value);
                        }
                        else if (arglist.Count == 0 && next.Text == ")") {
                            // end of arglist
                            break;
                        }
                        else {
                            AddError(6006, EngineResourceByNum(6006).Replace(
                                ARG1, type == 2 ? "test" : "action"), false);
                            error = true;
                            break;
                        }
                        next = NextConvertedToken();
                        if (next.Type == Separator) {
                            continue;
                        }
                        else if (next.Text == ")") {
                            break;
                        }
                        else {
                            AddError(6006, EngineResourceByNum(6006).Replace(
                                ARG1, type == 2 ? "test" : "action"), false);
                            error = true;
                            break;
                        }
                    } while (true);
                    // if error, skip rest of processing by finding next \n
                    if (error) {
                        SkipToNextLine(next);
                        break;
                    }
                    // get and validate number
                    next = NextConvertedToken();
                    if (next.Type != Num) {
                        AddError(6007, EngineResourceByNum(6007).Replace(
                            ARG1, type == 2 ? "test" : "action"), false);
                        // don't add
                        break;
                    }
                    num = next.Value;
                    error = false;
                    // validate command number
                    if (num < 0) {
                        AddWarning(5098);
                    }
                    else if (num > 255) {
                        AddWarning(7003);
                    }
                    num = (byte)num;
                    if (num > (type == 3 ? MAX_CMDS : TestCount)) {
                        AddError(6008, EngineResourceByNum(6008).Replace(
                            ARG1, type == 2 ? "test" : "action"), false);
                        // use zero as fallback
                        error = true;
                        num = 0;
                    }
                    if (type == 3 && num > ActionCount) {
                        AddWarning(7002, EngineResourceByNum(7002).Replace(
                            ARG1, num.ToString()).Replace(
                            ARG2, ActionCommands[num].FanName));
                    }
                    // check arguments
                    if (type == 2 && num == 14) {
                        // only one arg (WORDLIST) is valid
                        if (arglist.Count != 1 || arglist[0] != WORDLIST) {
                            // ignore arglist and set to WORDLIST
                            AddError(6009, EngineResourceByNum(6009), false);
                        }
                    }
                    else {
                        // invalid types for test commands are
                        // MSG, WORD, ANY, WORDLIST
                        if (arglist.Count > 0) {
                            if (arglist.Contains(MSG) ||
                                arglist.Contains(WORD) ||
                                arglist.Contains(ANY) ||
                                arglist.Contains(WORDLIST)) {
                                AddError(6010, EngineResourceByNum(6010).Replace(
                                    ARG1, type == 2 ? "test" : "action"), false);
                            }
                        }
                        // confirm arglist matches for this number
                        if (!error && arglist.Count != (type == 3 ? ActionCommands[num].ArgList.Length :
                            TestCommands[num].ArgList.Length)) {
                            AddError(6011, EngineResourceByNum(6011).Replace(
                                    ARG1, type == 2 ? "test" : "action"), false);
                        }
                    }
                    // add it to defines list
                    definesList.Add(name, new SierraToken(name,
                        type == 2 ? TestCmd : ActionCmd,
                        name,
                        num,
                        arglist.ToArray()));
                    // update predefine list for those that use alternate syntax
                    // (command names that equal FanName are required for 
                    // alternate syntax)
                    if (type == 2) {
                        if (num > 0 && num <= 7) {
                            if (TestCommands[num].FanName == name) {
                                TestCommandDefined[num] = true;
                            }
                        }
                    }
                    else {
                        if (num >= 1 && num <= 0x0B) {
                            if (ActionCommands[num].FanName == name) {
                                ActionCommandDefined[num] = true;
                            }
                        }
                    }
                    break;
                case 4: // flag name number
                case 5: // var name number
                case 6: // object name number
                case 9: // view name number
                    // number is next
                    next = NextConvertedToken();
                    if (next.Type != Num) {
                        AddError(6007, EngineResourceByNum(6007).Replace(
                            ARG1, typename).Replace(
                            ARG2, next.Name), false);
                        // don't add
                        break;
                    }
                    num = next.Value;
                    // validate number
                    if (num < 0) {
                        AddWarning(5098);
                    }
                    else if (num > 255) {
                        AddWarning(7003);
                    }
                    num = (byte)num;
                    // add it to defines list
                    definesList.Add(name, new SierraToken(name,
                        argtype,
                        next.Text,
                        num));
                    break;
                case 7: // define name value
                    // value can be ANYTHING except Unknown, BadStr or None
                    // (also, defining a colon will crash cg.exe, so we disallow it)
                    next = NextConvertedToken();
                    if (next.Type != Unknown && next.Type != None && next.Type != BadString) {
                        if (next.Text == ":") {
                            AddError(6014, EngineResourceByNum(6014).Replace(
                                ARG1, name), false);
                        }
                        else {
                            // validate number
                            if (next.Type == Num) {
                                if (next.Value < 0) {
                                    AddWarning(5098);
                                }
                                else if (next.Value > 255) {
                                    AddWarning(7003);
                                }
                                next.Value = (byte)next.Value;
                            }
                            definesList.Add(name, new SierraToken(
                                name,
                                next.Type,
                                next.Text,
                                next.Value,
                                next.ArgList));
                        }
                    }
                    else {
                        AddError(6014, EngineResourceByNum(6014).Replace(ARG1, next.Text), false);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Checks the specified message text for presence of extended characters.
        /// Adds a warning event if any are found.
        /// </summary>
        /// <param name="msgText"></param>
        /// <param name="msgNum"></param>
        private static void ValidateMsgChars(string msgText, int msgNum) {
            if (ErrorLevel == Low) {
                return;
            }
            if (msgText.Any(ch => ch == 8 || ch == 13)) {
                AddWarning(5093);
                MsgWarnings[msgNum] |= 1;
            }
            if (msgText.Any(ch => ch > 127)) {
                AddWarning(5094);
                MsgWarnings[msgNum] |= 2;
            }
        }

        private static void SkipToNextLine(SierraToken token) {
            // don't need to skip if already on next line
            if (sourceline[sourcelevel] > token.Line) {
                return;
            }

            // skip to the end of the current line
            char c;
            while (true) {
                if (pos[sourcelevel] >= sourcetext[sourcelevel].Length) {
                    // no more text in this file
                    return;
                }
                c = sourcetext[sourcelevel][pos[sourcelevel]++];
                if (c == '\n') {
                    sourceline[sourcelevel]++;
                    return;
                }
            }
        }

        private static void AddDefaultTokens() {
            definesList = [];

            // %include, %tokens, %test, %action, %flag, %var, %object, %define, %message, %view,
            ArgType type = Preprocessor;
            string text = "%include";
            int value = 0;
            definesList.Add(text, new(type, text, value));
            text = "%tokens";
            value = 1;
            definesList.Add(text, new(type, text, value));
            text = "%test";
            value = 2;
            definesList.Add(text, new(type, text, value));
            text = "%action";
            value = 3;
            definesList.Add(text, new(type, text, value));
            text = "%flag";
            value = 4;
            definesList.Add(text, new(type, text, value));
            text = "%var";
            value = 5;
            definesList.Add(text, new(type, text, value));
            text = "%object";
            value = 6;
            definesList.Add(text, new(type, text, value));
            text = "%define";
            value = 7;
            definesList.Add(text, new(type, text, value));
            text = "%message";
            value = 8;
            definesList.Add(text, new(type, text, value));
            text = "%view";
            value = 9;
            definesList.Add(text, new(type, text, value));
            // #include, #tokens, #test, #action, #flag, #var, #object, #define, #message, #view,
            text = "#include";
            value = 0;
            definesList.Add(text, new(type, text, value));
            text = "#tokens";
            value = 1;
            definesList.Add(text, new(type, text, value));
            text = "#test";
            value = 2;
            definesList.Add(text, new(type, text, value));
            text = "#action";
            value = 3;
            definesList.Add(text, new(type, text, value));
            text = "#flag";
            value = 4;
            definesList.Add(text, new(type, text, value));
            text = "#var";
            value = 5;
            definesList.Add(text, new(type, text, value));
            text = "#object";
            value = 6;
            definesList.Add(text, new(type, text, value));
            text = "#define";
            value = 7;
            definesList.Add(text, new(type, text, value));
            text = "#message";
            value = 8;
            definesList.Add(text, new(type, text, value));
            text = "#view";
            value = 9;
            definesList.Add(text, new(type, text, value));
            // if, else, goto
            type = Keyword;
            text = "if";
            value = 0;
            definesList.Add(text, new(type, text, value));
            text = "else";
            value = 1;
            definesList.Add(text, new(type, text, value));
            text = "goto";
            value = 2;
            definesList.Add(text, new(type, text, value));
            // FLAG, OBJECT, MSG, WORD, NUM, MSGNUM, VIEW, VAR, ANY, WORDLIST
            type = SierraArgToken;
            text = "FLAG";
            value = (int)Flag;
            definesList.Add(text, new(type, text, value));
            text = "OBJECT";
            value = (int)ArgType.Object;
            definesList.Add(text, new(type, text, value));
            text = "MSG";
            value = (int)MSG;
            definesList.Add(text, new(type, text, value));
            text = "WORD";
            value = (int)WORD;
            definesList.Add(text, new(type, text, value));
            text = "NUM";
            value = (int)Num;
            definesList.Add(text, new(type, text, value));
            text = "MSGNUM";
            value = (int)MsgNum;
            definesList.Add(text, new(type, text, value));
            text = "VIEW";
            value = (int)ArgType.View;
            definesList.Add(text, new(type, text, value));
            text = "VAR";
            value = (int)Var;
            definesList.Add(text, new(type, text, value));
            text = "ANY";
            value = (int)ANY;
            definesList.Add(text, new(type, text, value));
            text = "WORDLIST";
            value = (int)WORDLIST;
            definesList.Add(text, new(type, text, value));
        }

        /// <summary>
        /// Gets the next token from source code stream, staying in
        /// the current file. Converts it to correct type by checking
        /// defines.
        /// </summary>
        /// <returns></returns>
        private static SierraToken NextInLineConvertedToken() {
            SierraToken retval = NextRawToken(false, true);
            retval = ConvertToken(retval);
            return retval;
        }

        /// <summary>
        /// Gets the next token from the source code stream, and converts it
        /// to correct type by checking defines. This search will span lines
        /// and include files.
        /// </summary>
        /// <returns></returns>
        private static SierraToken NextConvertedToken() {
            SierraToken retval = NextRawToken(false, false);
            retval = ConvertToken(retval);
            return retval;
        }

        private static SierraToken ConvertToken(SierraToken retval) {
            if (retval.Type == Unknown) {
                // check for a define name
                if (definesList.TryGetValue(retval.Text, out SierraToken value)) {
                    int start = retval.StartPos;
                    int line = retval.Line;
                    retval = new(retval.Text,
                        value.Type,
                        value.Text,
                        value.Value,
                        value.ArgList) {
                        StartPos = start,
                        Line = line
                    };
                }
            }
            return retval;
        }

        private static SierraToken NextInLineToken() {
            return NextRawToken(true, true);
        }

        /// <summary>
        /// Gets the next token, without skipping over separator characters.
        /// </summary>
        /// <returns></returns>
        private static SierraToken NextLabelToken() {
            const string SYMBOLCHARS = "\t\n\r !\"&()+,-:;<=>@{|}";

            SierraToken next = new();
            if (pos[sourcelevel] >= sourcetext[sourcelevel].Length) {
                // no more text
                return next;
            }
            // build token
            while (!SYMBOLCHARS.Contains(sourcetext[sourcelevel][pos[sourcelevel]])) {
                if (sourcetext[sourcelevel][pos[sourcelevel]] == '&' ||
                sourcetext[sourcelevel][pos[sourcelevel]] == '|') {
                    // '&'/'|' bug
                    AddError(6028, false);
                    // use the token anyway
                }
                next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                if (pos[sourcelevel] >= sourcetext[sourcelevel].Length) {
                    break;
                }
            }
            if (next.Text.Length > 0) {
                next.Type = Unknown;
            }
            return next;
        }

        private static SierraToken NextInFileToken() {
            return NextRawToken(false, true);
        }

        /// <summary>
        /// Gets the next token from the source code input stream. Tokens are
        /// defined as one or more token characters, separated by a token
        /// separator.
        /// </summary>
        private static SierraToken NextRawToken(bool inline, bool infile) {
            // Sierra's parser only handles 8-bit characters. It doesn't
            // prohibit extended characters or non-printing (other than
            // tab, newline and carriage return), so those would be
            // considered identifier characters if encountered.

            //   Five types of characters:
            //   1. symbols: (17) !"&()+,-:;<=>@{|}
            //   2. numbers: (10) 0-9
            //   3. identifiers: (66) A-Z, a-z, #$%'*./?\]^_`~
            //   4. separators: (2) comment ([), newline (\n), carriage return (\r) and space
            //
            //   First character determines type:
            //   Symbol: 
            //      If a symbol, then the returned token will be a symbol. Depending on the start character, checks are made for the various two-character tokens.
            //         - One notable exception is !=; it is not returned as a token; instead, if the ! is encountered in the specialIf parser, then a check for an immediate = symbol is made. This means that (v1 ! = 1) is valid syntax).
            //      If the start character is @, then any following identifier and number tokens are added until a symbol or separator is reached. The token is still considered a symbol. @= is the only valid symbol token that AGI uses.
            //      If the start character is -, then if the next char is a number, the token type is changed to number, and a negative number is returned (built the same as the regular number token)
            //      If the start char is a " then type is changed to literal string (messagetext). Characters are added until an ending " is found. The backslash acts an 'escape' character; the character immediately following the slash is added regardless of what it is (except \n inserts a newline character into the string.) If a newline character is found, it is ignored (allows for mulit-line concatenation). 
            //      If the start char is '\x1a', treat it as end-of-file

            //   Number:
            //      Characters are added to the number token until a separator or symbol token is found. The token is converted into a value by reading digits off the token until a non-number character is reached. For example "123abc...456$$$" becomes 123. The result is then converted to byte by reading only the lowest 8 bytes. For example, 256=0, 257=2, etc.

            //   Identifier:
            //      Characters are added to the identifier token until a separator or symbol token is found. Depending on what is being parsed, identifier tokens are then checked against the defines list and replaced with type and value of the matching define.
            //      The following situations will NOT convert an identifier token:
            //       - #include filename argument
            //       - #tokens filename argument
            //       - goto argument/location
            //       - action or test token name
            //       - define, object, var, flag, view token name
            //       - said words

            // Valid double-char tokens:  ++, --, +=, -=, ==, @=, =@, &&, ||
            //
            // IMPORTANT: there is a bug in cg.exe that causes it to enter an
            // infinite loop if a '&' or '|' is encountwred when token type
            // is already set ("&&" and "||" excepted)
            //
            // The bug also causes a token that starts with '& or '|' to not
            // set a token type (they get stuck as 'singleamp' or 'singlepipe')
            // so they can never work as an argument, number, text string, etc

            // if end of input is reached, next token is an empty string

            // token name = token text for raw tokens
            // if token is a define, name stays the same and all other token 
            // properties are updated

            SierraToken next = new() {
                StartPos = pos[sourcelevel],
                Line = sourceline[sourcelevel]
            };

            // skip white space until a char is found
            char c;
            do {
                if (pos[sourcelevel] >= sourcetext[sourcelevel].Length) {
                    // if checking only and at end of text, or if no
                    // more text in base level, return empty token
                    if (infile || sourcelevel == 0) {
                        // no more text
                        return retval();
                    }
                    else {
                        // drop the current sourcetext
                        sourcetext[sourcelevel] = null;
                        sourcelevel--;
                        // treat it as a space
                        c = ' ';
                        continue;
                    }
                }
                c = sourcetext[sourcelevel][pos[sourcelevel]++];
                // check for code 0x1A first; it signifies 'end of file'
                if (c == '\x1a') {
                    AddWarning(7014);
                    // adjust pos to end of file
                    next.StartPos = pos[sourcelevel] =
                        sourcetext[sourcelevel].Length;
                    // treat as a space
                    c = ' ';
                    continue;
                }
                if (c == '[') {
                    // if comment, skip to end of line, then try again
                    do {
                        if (pos[sourcelevel] >= sourcetext[sourcelevel].Length) {
                            // treat it as a space
                            c = ' ';
                            break;
                        }
                        c = sourcetext[sourcelevel][pos[sourcelevel]++];
                        if (c == '\x1a') {
                            AddWarning(7014);
                            // adjust pos to end of file
                            next.StartPos = pos[sourcelevel] =
                                sourcetext[sourcelevel].Length;
                            next.Line = sourceline[sourcelevel];
                            return retval();
                        }
                    } while (c != '\n');
                }
                if (c == '\n') {
                    sourceline[sourcelevel]++;
                }
            } while (c == ' ' || c == '\t' || c == '\n' || c == '\r');

            // got a char; start building token
            next.Text = c.ToString();
            next.StartPos = pos[sourcelevel] - 1;
            next.Line = sourceline[sourcelevel];
            // !"&()+,-:;<=>@{|}
            switch (c) {
            case '(':
            case ')':
            case ':':
            case '{':
            case '}':
                // these are always single char symbol tokens
                next.Type = Symbol;
                return retval();
            case ',':
            case ';':
                // these are always single char separator tokens
                next.Type = Separator;
                return retval();
            case '!':
            case '<':
            case '>':
                // these are always single char test tokens
                next.Type = TestOperator;
                return retval();
            case '\"':
                // beginning of quote
                next.Type = DefStr;
                bool slash = false;
                do {
                    if (pos[sourcelevel] >= sourcetext[sourcelevel].Length) {
                        // bad string - reached end of line without closing quote
                        next.Type = BadString;
                        return retval();
                    }
                    c = sourcetext[sourcelevel][pos[sourcelevel]++];
                    // count line breaks
                    if (c == '\n') {
                        sourceline[sourcelevel]++;
                    }
                    // ignore \r and line breaks inside string
                    if (c != '\r' && c != '\n') {
                        if (next.Text.Length >= 999) {
                            // string too long
                            next.Type = BadString;
                            AddError(6013, EngineResourceByNum(6013), false);
                            return retval();
                        }
                        // \x1a gets converted to \xff
                        if (c == '\x1a') {
                            AddWarning(7015);
                            c = '\xff';
                        }
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
                                next.Text += c;
                                return retval();
                            }
                        }
                        next.Text += c;
                    }
                } while (true);
            case '&':
                // default type
                next.Type = TestOperator;
                // check for &&
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    if (sourcetext[sourcelevel][pos[sourcelevel]] == '&') {
                        next.Text = "&&";
                        pos[sourcelevel]++;
                    }
                    else {
                        BuildToken();
                        // if not a single '&' make it a null type
                        if (next.Text.Length > 1) {
                            if (!next.Text[1..].Contains('|') && !next.Text[1..].Contains('&')) {
                                AddWarning(7020);
                            }
                            next.Type = BadSymbol;
                        }
                    }
                }
                return retval();
            case '+':
                // check for +, ++ and +=
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    switch (sourcetext[sourcelevel][pos[sourcelevel]]) {
                    case '+':
                    case '=':
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        break;
                    case '&':
                    case '|':
                        // '&'/'|' bug
                        AddError(6028, false);
                        // use the token anyway
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        break;
                    }
                }
                next.Type = AssignOperator;
                return retval();
            case '-':
                // check for -, --, -= and negative numbers
                next.Type = AssignOperator;
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    switch (sourcetext[sourcelevel][pos[sourcelevel]]) {
                    case '-':
                    case '=':
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        break;
                    case >= '0' and <= '9':
                        // return a negative number
                        GetNumberToken();
                        return retval();
                    case '&':
                    case '|':
                        // '&'/'|' bug
                        AddError(6028, false);
                        // use the token anyway
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        break;
                    }
                }
                return retval();
            case '=':
                // check for =, == and =@
                next.Type = AssignOperator;
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    switch (sourcetext[sourcelevel][pos[sourcelevel]]) {
                    case '=':
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        next.Type = TestOperator;
                        break;
                    case '@':
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        break;
                    // check for '&'/'|' bug
                    case '&':
                    case '|':
                        AddError(6028, false);
                        // use the token anyway
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        break;
                    }
                }
                return retval();
            case '@':
                // check for '@='; otherwise build an identifier token but
                // leave as type Symbol
                next.Type = Symbol;
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    if (sourcetext[sourcelevel][pos[sourcelevel]] == '=') {
                        next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                        next.Type = TestOperator;
                    }
                    else {
                        BuildToken();
                        // if not a single '@' make it a null type
                        if (next.Text.Length > 1) {
                            if (!next.Text.Contains('|') && !next.Text.Contains('&')) {
                                AddWarning(7020);
                            }
                            next.Type = BadSymbol;
                        }
                    }
                }
                return retval();
            case '|':
                // default type
                next.Type = TestOperator;
                // check for ||
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    if (sourcetext[sourcelevel][pos[sourcelevel]] == '|') {
                        next.Text = "||";
                        pos[sourcelevel]++;
                    }
                    else {
                        BuildToken();
                        // if not a single '|' make it a null type
                        if (next.Text.Length > 1) {
                            if (!next.Text[1..].Contains('|') && !next.Text[1..].Contains('&')) {
                                AddWarning(7020);
                            }
                            next.Type = BadSymbol;
                        }
                    }
                }
                return retval();
            case >= '0' and <= '9':
                // return a negative number
                next.Type = Num;
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    GetNumberToken();
                }
                return retval();
            default:
                // any other char types (A-Z, a-z, #$%'*./?\]^_`~) plus extended
                // are 'identifier' characters; build the identifier token
                int start = next.StartPos;
                if (pos[sourcelevel] < sourcetext[sourcelevel].Length) {
                    BuildToken();
                }
                next.Type = Unknown;
                next.StartPos = start;
                return retval();
            }

            void BuildToken() {
                // '&' and '|' are not included in the symbol chars because of
                // the cg.exe bug; they get treated as identifiers instead of
                // an infinite loop
                const string SYMBOLCHARS = "\t\n\r [!\"()+,-:;<=>@{}";

                while (!SYMBOLCHARS.Contains(sourcetext[sourcelevel][pos[sourcelevel]])) {
                    if (sourcetext[sourcelevel][pos[sourcelevel]] is '&' or '|') {
                        // '&'/'|' bug
                        AddError(6028, false);
                        // use the token anyway
                    }
                    next.Text += sourcetext[sourcelevel][pos[sourcelevel]++];
                    if (pos[sourcelevel] >= sourcetext[sourcelevel].Length) {
                        break;
                    }
                    if (sourcetext[sourcelevel][pos[sourcelevel]] == '\x1a') {
                        // treat as a space- end this token, skip the char
                        AddWarning(7016);
                        pos[sourcelevel]++;
                        break;
                    }
                }
                return;
            }

            void GetNumberToken() {
                next.Type = Num;
                BuildToken();
                if (!int.TryParse(next.Text, out next.Value)) {
                    // show warning, then get number off front of token
                    AddWarning(7007, EngineResourceByNum(7007).Replace(
                        ARG1, next.Text));
                    // Extract leading digits using regex
                    Match match = Regex.Match(next.Text, @"^\d+");
                    next.Value = match.Success ? int.Parse(match.Value) : 0;
                }
                return;
            }

            // forces raw token name to match text before returning
            SierraToken retval() {
                next.Name = next.Text;
                return next;
            }
        }

        /// <summary>
        /// Writes the messages for a logic at the end of the resource.
        /// Messages are encrypted with the string 'Avis Durgan'. No gaps
        /// are allowed, so messages that are skipped must be included as
        /// zero length messages.
        /// </summary>
        /// <returns></returns>
        private static bool WriteMsgs() {
            int msgSecStart = tmpLogRes.Size;
            int[] msgPos = new int[256];

            // find last message by counting backwards until a msg is found
            int msgCount = 256;
            do {
                msgCount--;
            } while (!MsgInUse[msgCount] && (msgCount != 0));
            // write msg count
            tmpLogRes.WriteByte((byte)msgCount);
            if (msgCount > 0) {
                // write place holder for msg end
                tmpLogRes.WriteWord(0);
                // write place holders for msg pointers
                for (int i = 0; i < msgCount; i++) {
                    tmpLogRes.WriteWord(0);
                }
                int cryptStart = tmpLogRes.Size;
                for (int i = 1; i <= msgCount; i++) {
                    if (MsgInUse[i]) {
                        // calculate offset to start of this message (adjust by one byte,
                        // which is the byte that indicates how many msgs there are)
                        msgPos[i] = tmpLogRes.Pos - (msgSecStart + 1);
                        byte[] msgCharArray = Encoding.GetEncoding(sCompGame.CodePage).GetBytes(MsgText[i]);
                        for (int j = 0; j < msgCharArray.Length; j++) {
                            tmpLogRes.WriteByte((byte)(msgCharArray[j] ^ bytEncryptKey[(tmpLogRes.Pos - cryptStart) % 11]));
                        }
                        // add trailing zero to terminate message (even if msg was zero length,
                        // terminator is still needed)
                        tmpLogRes.WriteByte((byte)(0 ^ bytEncryptKey[(tmpLogRes.Pos - cryptStart) % 11]));
                    }
                    else {
                        // for unused messages need to write a null value for offset;
                        // (when it gets added after all messages are written it gets
                        // set to the beginning of message section)
                        msgPos[i] = 0;
                    }
                }
                // calculate length of msgs, and write it at beginning of msg section
                // (adjust by one byte, which is the byte that indicates number of
                // msgs written)
                tmpLogRes.WriteWord((ushort)(tmpLogRes.Pos - (msgSecStart + 1)), msgSecStart + 1);
            }
            // write msg section start value at beginning of resource (correct by
            // two so it gives position relative to byte 2 of the logic resource
            // start
            tmpLogRes.WriteWord((ushort)(msgSecStart - 2), 0);
            // write all the msg pointers (start with msg1 since msg0 is never allowed)
            for (int i = 1; i <= msgCount; i++) {
                tmpLogRes.WriteWord((ushort)msgPos[i], msgSecStart + 1 + i * 2);
            }
            // success
            return true;
        }

        /// <summary>
        /// Reports errors back to calling thread. Minor errors don't cancel
        /// the compile method, but major (critical) errors do.
        /// </summary>
        private static void AddError(int ErrorNum, bool critical, int line = -1) {
            AddError(ErrorNum, EngineResourceByNum(ErrorNum), critical, line);
        }

        /// <summary>
        /// Reports errors back to calling thread. Minor errors don't cancel
        /// the compile method, but major (critical) errors do.
        /// </summary>
        /// <param name="ErrorNum"></param>
        /// <param name="ErrorText"></param>
        private static void AddError(int ErrorNum, string ErrorText, bool critical, int line = -1) {
            if (line == -1) {
                //line = errorLine;
                line = sourceline[sourcelevel];
            }

            WinAGIEventInfo errInfo = new() {
                Type = EventType.LogicCompileError,
                ResType = AGIResType.Logic,
                ID = ErrorNum.ToString(),
                Text = ErrorText,
                ResNum = sourcelevel == 0 ? compLogic.Number : -1,
                Line = line,
                Module = sourcelevel == 0 ? compLogic.ID : Path.GetFileName(sourcefile[sourcelevel]),
                Filename = sourcelevel == 0 ? "" : sourcefile[sourcelevel],
            };
            sCompGame.CancelComp = OnCompileLogicStatus(sCompGame, errInfo);

            if (!critical) {
                minorError = true;
            }
        }

        /// <summary>
        /// This method returns a string representing the name of the specified
        /// argument type.
        /// </summary>
        /// <param name="ArgType"></param>
        /// <returns></returns>
        private static string ArgTypeName(ArgType ArgType) {
            // FLAG, OBJECT, MSG, WORD, NUM, MSGNUM, VIEW, VAR, ANY, WORDLIST
            switch (ArgType) {
            case Flag:
                return "flag";
            case ArgType.Object:
                return "object";
            case Num:
                return "number";
            case MsgNum:
                return "message number";
            case ArgType.View:
                return "view";
            case Var:
                return "variable";
            //case MSG:
            //case WORD:
            //case ANY:
            //case WORDLIST:

            //
            default:
                // no other types should be possible
                return "";
            }
        }

        /// <summary>
        /// This method checks for reserved flag (f0-f20) use and adds warnings
        /// as appropriate.
        /// </summary>
        /// <param name="flagnum"></param>
        private static void CheckResFlagUse(byte flagnum) {
            if (ErrorLevel == Low) {
                return;
            }
            if (flagnum == 2 ||
                flagnum == 4 ||
                (flagnum >= 7 && flagnum <= 10) ||
                flagnum >= 13) {
                //    f2: haveInput
                //    f4: haveMatch
                //    f7: script_buffer_blocked
                //    f8: joystick sensitivity set
                //    f9: sound_on
                //    f10: trace_abled
                //    f13: inventory_select_enabled
                //    f14: menu_enabled
                //    f15: windows_remain
                //    f16: no_prompt_restart
                //    f20: auto_restart
                //    >f21: non-reserved
                // no restrictions
            }
            else {
                // all other reserved flags should be read only
                AddWarning(7004, EngineResourceByNum(7004).Replace(
                    ARG1, "flag " + flagnum.ToString()));
            }
        }

        /// <summary>
        /// This method checks for reserved variable (v0-v26) use and adds warnings
        /// as appropriate.
        /// </summary>
        /// <param name="varnum"></param>
        /// <param name="argval"></param>
        private static void CheckResVarUse(byte varnum, byte argval) {
            if (ErrorLevel == Low) {
                return;
            }
            switch (varnum) {
            case 3 or 7 or 15 or 21 or >= 27:
                //    v3: curent score
                //    v7: max score
                //    v15: joystick sensitivity
                //    v21: msg window delay time
                //    >=v27: non-reserved
                // no restrictions
                break;
            case 6:
                // ego direction
                // should be restricted to values 0-8
                if (argval > 8) {
                    AddWarning(5018, EngineResourceByNum(5018).Replace(
                        ARG1, "v6").Replace(ARG2, "8"));
                }
                break;
            case 10:
                // cycle delay time
                // large values highly unusual
                if (argval > 20) {
                    AddWarning(5055);
                }
                break;
            case 17 or 18:
                // error value, and error info
                // resetting to zero is usually a good thing; other values don't make sense
                if (argval > 0) {
                    AddWarning(5092, EngineResourceByNum(5092).Replace(
                        ARG1, "v" + varnum.ToString()));
                }
                break;
            case 19:
                // key_pressed value
                // ok if resetting for key input
                if (argval > 0) {
                    AddWarning(7004, EngineResourceByNum(7004).Replace(
                        ARG1, "variable " + varnum.ToString()));
                }
                break;
            case 23:
                // sound attenuation
                // restrict to 0-15
                if (argval > 15) {
                    AddWarning(5018, EngineResourceByNum(5018).Replace(
                        ARG1, "v23").Replace(ARG2, "15"));
                }
                break;
            case 24:
                // max input length
                if (argval > 39) {
                    AddWarning(5018, EngineResourceByNum(5018).Replace(
                        ARG1, "v24").Replace(ARG2, "39"));
                }
                break;
            default:
                // all other reserved variables should be read only
                AddWarning(7004, EngineResourceByNum(7004).Replace(
                    ARG1, "variable " + varnum.ToString()));
                break;
            }
        }

        /// <summary>
        /// Gets the next token and compares it to the specified value. The
        /// method returns true if the token matches. The token is 
        /// returned to the input stream if it does not match.
        /// The search will not span multiple included files.
        /// </summary>
        /// <param name="checktoken"></param>
        /// <returns></returns>
        private static bool CheckToken(string checktoken) {
            string test;
            // cache current source position info
            int cLine = sourceline[sourcelevel];
            int cPos = pos[sourcelevel];

            // check the text value, not the define name
            test = NextInFileToken().Text;
            if (test != checktoken) {
                // no match; restore position
                sourceline[sourcelevel] = cLine;
                pos[sourcelevel] = cPos;
                return false;
            }
            else {
                return true;
            }
        }

        /// <summary>
        /// Sends the specified warning to the calling program as an event.
        /// </summary>
        /// <param name="WarningNum"></param>
        /// <param name="WarningText"></param>
        private static void AddWarning(int WarningNum, string WarningText = "", int line = -1) {
            if (line == -1) {
                line = sourceline[sourcelevel];
            }

            // if no text passed, use the default resource string
            if (WarningText.Length == 0) {
                WarningText = EngineResourceByNum(WarningNum);
            }
            int index = IndexFromWarningNumber(WarningNum);
            if (index < 0 || index >= NoCompWarn.Length) {
                Debug.Assert(false);
                return;
            }
            // only add if not ignoring
            if (!NoCompWarn[index]) {
                WinAGIEventInfo errInfo = new() {
                    Type = EventType.LogicCompileWarning,
                    ResType = sourcelevel == 0 ? AGIResType.Logic : AGIResType.Include,
                    ID = WarningNum.ToString(),
                    Text = WarningText,
                    ResNum = sourcelevel == 0 ? complogicNumber : -1,
                    Line = line,
                    Module = sourcelevel == 0 ? compLogic.ID : Path.GetFileName(sourcefile[sourcelevel]),
                    Filename = sourcelevel == 0 ? "" : sourcefile[sourcelevel],
                };
                sCompGame.CancelComp = OnCompileLogicStatus(sCompGame, errInfo);
            }
        }

        /// <summary>
        /// This method reads and validates the input for an 'if' command. 
        /// </summary>
        /// <returns>Return value is true if if block is OK. EOF check is
        /// True if end of file encountered during check.</returns>
        private static (bool, bool) CompileIf() {
            // the syntax expected for test tokens is:
            // 
            //    if(<test>){
            // or
            //    if(<test> && <test> && ... ){
            // or
            //    if((<test> || <test> || ... )){
            //
            // or a combination of ORs and ANDs, as long as ORs are always
            //    in brackets, and ANDs are never in brackets
            //
            //    <test> may be a test token (<tstcmd>(arg1, arg2, ..., argn)
            //    or a special syntax representation of a test token:
            //      fn         ==> isset(fn)
            //      vn         ==> greatern(vn, 0)
            //      vn expr m  ==> <testcmd>(vn,m)
            //
            // valid special comparison expressions are ==, !, >, <
            //      (! must be followed by = to create a 'not equal' test)
            //
            // OR'ed tests must always be enclosed in parenthesis; AND'ed tests
            // must never be enclosed in parentheses (this ensures the compiled code
            // will be compatible with the AGI interpreter)
            //
            // any test token may have the negation operator (!) placed directly
            // in front of it; special syntax 'fn' or 'vn' may also have the
            // negation operator; multiple ! symbols are allowed, but extra ones
            // are ignored by the interpreter (this compiler doesn't add the extras)
            //
            byte[] arglist = new byte[8];
            bool inOrBlock = false;
            bool needNextCmd = true;
            int numTestCmds = 0, numCmdsInBlock = 0;
            int arg2 = 0;
            bool isNOT = false, varTest = false;

            // 'if' starting bytecode
            tmpLogRes.WriteByte(0xFF);

            // next character should be "("
            if (!CheckToken("(")) {
                AddError(4001, false);
                // keep going - maybe the missing '(' is the only problem
            }

            // step through input, until final ')' is found:
            do {
                SierraToken testToken = NextConvertedToken();
                if (testToken.Type == None) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    return (false, true);
                }
                if (needNextCmd) {
                    // valid tokens
                    switch (testToken.Type) {
                    case Symbol:
                        // symbol should be '('or ')'
                        switch (testToken.Text) {
                        case "(":
                            if (isNOT) {
                                // can't NOT a block
                                AddError(4065, false);
                            }
                            if (inOrBlock) {
                                // can't have more than one level of OR blocks
                                AddError(4024, false);
                            }
                            // add 'or' block starting bytecode
                            tmpLogRes.WriteByte(0xFC);
                            inOrBlock = true;
                            numCmdsInBlock = 0;
                            break;
                        case ")":
                            if (isNOT) {
                                // can't leave a NOT dangling...
                                AddError(4066, false);
                            }
                            if (inOrBlock) {
                                if (numCmdsInBlock == 0) {
                                    // or block with no commands
                                    // NOT ALLOWED in Sierra syntax
                                    AddError(6033, false);
                                }
                                else {
                                    AddError(4033, false);
                                    // done with if
                                    tmpLogRes.WriteByte(0xFF);
                                    return (true, false);
                                }
                                // close the block
                                inOrBlock = false;
                                needNextCmd = false;
                            }
                            else if (numTestCmds == 0) {
                                // if block with no commands
                                // NOT ALLOWED in Sierra syntax
                                AddError(6032, false);
                                // done with if
                                tmpLogRes.WriteByte(0xFF);
                                return (true, false);
                            }
                            else {
                                // unexpected closer
                                AddError(4033, false);
                                // done with if
                                tmpLogRes.WriteByte(0xFF);
                                return (true, false);
                            }
                            break;
                        default:
                            // error, unexpected symbol
                            AddError(4021, EngineResourceByNum(4021).Replace(
                                ARG1, testToken.Name), false);
                            SkipToNextLine(testToken);
                            return (false, false);
                        }
                        break;
                    case TestOperator:
                        switch (testToken.Text) {
                        case "!":
                            if (isNOT) {
                                // multiple NOTs cause toggling
                                AddWarning(7006);
                            }
                            isNOT = true;
                            tmpLogRes.WriteByte(0xFD);
                            break;
                        default:
                            // error, unexpected symbol
                            AddError(4021, EngineResourceByNum(4021).Replace(
                                ARG1, testToken.Name), false);
                            SkipToNextLine(testToken);
                            return (false, false);
                        }
                        break;
                    case TestCmd:
                        SierraToken argToken;
                        byte cmdnum = (byte)testToken.Value;
                        tmpLogRes.WriteByte(cmdnum);
                        // next command should be "("
                        if (!CheckToken("(")) {
                            AddError(4027, false);
                        }
                        // check for return.false() token
                        if (testToken.Value == 0) {
                            // warn user that it's not compatible with AGI Studio
                            if (ErrorLevel == Medium) {
                                AddWarning(5081);
                            }
                        }
                        // 'said' command special processing
                        if (cmdnum == 0x0E) {
                            // if #tokens preprocessor not set, add error
                            if (!wordsloaded) {
                                AddError(6020, false);
                            }

                            // process a said command
                            int argnum;
                            List<int> words = [];
                            do {
                                // get next argument (don't search defines though)
                                argToken = NextInFileToken();
                                if (argToken.Type == None) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    return (false, true);
                                }
                                switch (argToken.Type) {
                                case Num:
                                    // not allowed in Sierra syntax
                                    AddError(6021, false);
                                    // use a placeholder
                                    argnum = 1;
                                    break;
                                case DefStr:
                                    // sierra syntax - string not allowed here
                                    AddError(6022, false);
                                    // use a placeholder
                                    argnum = 1;
                                    break;
                                default:
                                    // might be a Sierra syntax word
                                    // check if token text contains any upper case characters
                                    if (argToken.Text.Any(char.IsUpper)) {
                                        // cg.exe converts word text to lower case,
                                        // but user should be warned
                                        AddWarning(7013);
                                    }
                                    argToken.Text = argToken.Text.Replace('$', ' ').ToLower();
                                    argnum = -1;
                                    break;
                                }
                                // validate the group
                                if (argnum == -1) {
                                    if (sCompGame.agVocabWords.WordExists(argToken.Text)) {
                                        argnum = sCompGame.agVocabWords[argToken.Text].Group;
                                    }
                                    else {
                                        // RARE, but if it's an 'a' or 'i' that isn't defined,
                                        // it's word group 0
                                        if (argToken.Text == "i" || argToken.Text == "a") {
                                            argnum = 0;
                                            // add warning
                                            if (ErrorLevel == Medium) {
                                                AddWarning(5108, EngineResourceByNum(5108).Replace(
                                                    ARG1, argToken.Text));
                                            }
                                        }
                                        else {
                                            AddError(4067, EngineResourceByNum(4067).Replace(
                                                ARG1, argToken.Text), false);
                                        }
                                    }
                                }
                                // check for group 0
                                if (argnum == 0) {
                                    if (ErrorLevel == Medium) {
                                        AddWarning(5083, EngineResourceByNum(5083).Replace(
                                            ARG1, argToken.Text));
                                    }
                                }
                                // check for v2.089 and group 9999
                                if (sCompGame.InterpreterVersion.Index == AGIVersion.v2089 && argnum == 9999) {
                                    if (ErrorLevel == Medium) {
                                        AddWarning(5121);
                                    }
                                }
                                // if too many words
                                if (words.Count == 10) {
                                    if (ErrorLevel == Medium) {
                                        AddWarning(5119);
                                    }
                                }
                                if (words.Count < 10) {
                                    // add this word number
                                    // to array of word numbers
                                    words.Add(argnum);
                                }
                                // next character should be a comma, or close parenthesis
                                argToken = NextConvertedToken();
                                if (argToken.Type == None) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    return (false, true);
                                }
                                if (argToken.Text == ")") {
                                    // move pointer back one space so the ')' 
                                    // will be found at end of command
                                    pos[sourcelevel]--;
                                    break;
                                }
                                else if (argToken.Type == Separator) {
                                    // get another word
                                    if (argToken.Text == ";") {
                                        AddWarning(7019);
                                    }
                                }
                                else {
                                    // missing comma or closing parenthesis
                                    AddError(4026, EngineResourceByNum(4026).Replace(
                                        ARG1, (words.Count + 1).ToString()).Replace(
                                        ARG2, "said"), false);
                                    // assume comma and continue (but move
                                    // pointer back so next token will be correct)
                                    pos[sourcelevel] = argToken.StartPos;
                                    //skipError = false;
                                    continue;
                                }
                            } while (true);
                            // add number of arguments
                            tmpLogRes.WriteByte((byte)words.Count);
                            // add words
                            foreach (int i in words) {
                                tmpLogRes.WriteWord((ushort)i);
                            }
                        }
                        else {
                            // extract arguments (assume all OK)
                            bool argsOK = true;
                            for (int i = 0; i < testToken.ArgList.Length; i++) {
                                argToken = NextConvertedToken();
                                if (argToken.Type == None) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    return (false, true);
                                }
                                if (i > 0) {
                                    // should be a separator(';' and ',' are
                                    // interchangeable in sierra syntax)
                                    if (argToken.Type == Separator) {
                                        // but ',' is preferred
                                        if (argToken.Text == ";") {
                                            AddWarning(7019);
                                        }
                                        // next token is the argument
                                        argToken = NextConvertedToken();
                                        if (argToken.Type == None) {
                                            // nothing left, return critical error
                                            AddError(4057, true);
                                            return (false, true);
                                        }
                                    }
                                    else {
                                        // missing separator
                                        // use 1-base arg values (but since referring to
                                        // previous arg, don't increment arg index)
                                        AddError(4026, EngineResourceByNum(4026).Replace(
                                            ARG1, i.ToString()).Replace(
                                            ARG2, TestCommands[cmdnum].FanName), false);
                                        // assume the current token is the argument
                                    }
                                }
                                // validate type
                                int argval = argToken.Value;
                                if (argToken.Type == testToken.ArgList[i]) {
                                    // validate argument value
                                    ValidateArgType(argval, argToken.Type, i);
                                    arglist[i] = (byte)argval;
                                }
                                else {
                                    if (argToken.Type == Separator ||
                                        argToken.Text == ")") {
                                        // missing or empty argument
                                        AddError(4031, EngineResourceByNum(4031).Replace(
                                            ARG1, (i + 1).ToString()).Replace(
                                            ARG2, testToken.Name).Replace(
                                            ARG3, ArgTypeName(testToken.ArgList[i])), false);
                                        // put back the separator or ')'
                                        pos[sourcelevel]--;
                                        // if ')', end the loop early
                                        if (argToken.Text == ")") {
                                            break;
                                        }
                                    }
                                    else {
                                        // wrong token
                                        AddError(4037, EngineResourceByNum(4037).Replace(
                                            ARG1, (i + 1).ToString()).Replace(
                                            ARG2, ArgTypeName(testToken.ArgList[i])).Replace(
                                            ARG3, argToken.Name), false);
                                    }
                                    // use a placeholder
                                    arglist[i] = 0;
                                    argsOK = false;
                                }
                                // add argument
                                tmpLogRes.WriteByte(arglist[i]);
                            }
                            if (argsOK) {
                                // validate arguments for this command
                                ValidateIfArgs(cmdnum, arglist);
                            }
                        }
                        // next character should be ")"
                        if (!CheckToken(")")) {
                            AddError(4084, false);
                        }
                        numTestCmds++;
                        if (inOrBlock) {
                            numCmdsInBlock++;
                        }
                        needNextCmd = false;
                        isNOT = false;
                        break;
                    case Var:
                        // v# expr v#
                        // v# expr #
                        // v##
                        int arg1 = testToken.Value;
                        ValidateArgType(arg1, Var, 0);
                        byte cmd;
                        bool addNot = false;
                        // next token should be the expression
                        argToken = NextConvertedToken();
                        if (argToken.Type == None) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            return (false, true);
                        }
                        switch (argToken.Text) {
                        case "==":
                            cmd = 0x01;
                            break;
                        case "!":
                            // check for !=; it's the only allowed
                            // use of the NOT operator in special syntax
                            cmd = 0x01;
                            if (CheckToken("=")) {
                                // check for multiple NOTs
                                if (isNOT) {
                                    // extra NOTs cause toggling
                                    // ('NOT'ing != converts it back to ==)
                                    AddWarning(7008);
                                }
                                addNot = true;
                            }
                            else {
                                // unknown expression token
                                AddError(4049, false);
                                SkipToNextLine(argToken);
                                return (false, false);
                            }
                            break;
                        case ">":
                            cmd = 0x05;
                            break;
                        case "<":
                            cmd = 0x03;
                            break;
                        case ")" or "&&" or "||":
                            // means we are doing a boolean test of the variable;
                            // use greatern with zero as arg
                            cmd = 0x05;
                            arg2 = 0;
                            varTest = true;
                            // restore the token to the stream so main compiler
                            // can handle it
                            pos[sourcelevel] = argToken.StartPos;
                            break;
                        case "=":
                            // invalid compare
                            AddError(4060, EngineResourceByNum(4060).Replace(
                                ARG1, argToken.Name), false);
                            // use equal(==) as placeholder
                            cmd = 0x01;
                            break;
                        default:
                            // unknown expression token
                            AddError(4049, false);
                            SkipToNextLine(argToken);
                            return (false, false);
                        }
                        // if not doing a single var test (using greatern command)
                        // need to get second argument
                        if (!varTest) {
                            argToken = NextConvertedToken();
                            switch (argToken.Type) {
                            case None:
                                // nothing left, return critical error
                                AddError(4057, true);
                                return (false, true);
                            case Var:
                                // use variable version of command
                                cmd++;
                                break;
                            case Num:
                                // ok
                                break;
                            case ArgType.Object:
                                // ok, but warn user
                                AddWarning(7010, EngineResourceByNum(7010).Replace(
                                    ARG1, argToken.Name));
                                break;
                            default:
                                AddError(4070, EngineResourceByNum(4070).Replace(
                                        ARG1, argToken.Name), false);
                                SkipToNextLine(argToken);
                                return (false, false);
                            }
                            arg2 = argToken.Value;
                            ValidateArgType(arg2, argToken.Type, 1);
                        }
                        else {
                            varTest = false;
                        }
                        isNOT = false;
                        if (addNot) {
                            tmpLogRes.WriteByte(0xFD);
                        }
                        // confirm command has been defined
                        if (!TestCommandDefined[cmd]) {
                            AddError(6016, EngineResourceByNum(6016).Replace(
                                ARG1, TestCommands[cmd].FanName), false);
                        }
                        // write command, and arguments
                        tmpLogRes.WriteByte(cmd);
                        tmpLogRes.WriteByte((byte)arg1);
                        tmpLogRes.WriteByte((byte)arg2);
                        numTestCmds++;
                        if (inOrBlock) {
                            numCmdsInBlock++;
                        }
                        needNextCmd = false;
                        break;
                    case Flag:
                        // check for special flag syntax
                        arg2 = testToken.Value;
                        ValidateArgType(arg2, Flag, 0);
                        // confirm command has been defined
                        if (!TestCommandDefined[7]) {
                            AddError(6016, EngineResourceByNum(6016).Replace(
                                ARG1, TestCommands[7].FanName), false);
                        }
                        // write isset cmd
                        tmpLogRes.WriteByte(0x07);
                        tmpLogRes.WriteByte((byte)arg2);
                        needNextCmd = false;
                        numTestCmds++;
                        if (inOrBlock) {
                            numCmdsInBlock++;
                        }
                        isNOT = false;
                        break;
                    default:
                        // invalid token
                        AddError(4092, EngineResourceByNum(4092).Replace(
                            ARG1, testToken.Name), false);
                        SkipToNextLine(testToken);
                        return (false, false);
                    }
                }
                else {
                    // not awaiting a test command
                    switch (testToken.Text) {
                    case "!":
                        // 'NOT' token is not allowed 
                        AddError(4075, false);
                        break;
                    case "&&":
                        if (inOrBlock) {
                            // 'and' not allowed if inside brackets
                            AddError(4019, false);
                        }
                        needNextCmd = true;
                        break;
                    case "||":
                        if (!inOrBlock) {
                            // 'or' not allowed UNLESS inside brackets
                            AddError(4076, false);
                        }
                        needNextCmd = true;
                        break;
                    case ")":
                        if (inOrBlock) {
                            inOrBlock = false;
                            tmpLogRes.WriteByte(0xFC);
                            if (numCmdsInBlock == 1) {
                                // or block with one command
                                if (ErrorLevel == Medium) {
                                    AddWarning(5002);
                                }
                            }
                        }
                        else {
                            // end of if block found
                            tmpLogRes.WriteByte(0xFF);
                            return (true, false);
                        }
                        break;
                    default:
                        AddError(inOrBlock ? 4077 : 4020, false);
                        // assume it was ok
                        needNextCmd = true;
                        // and if it is a close bracket, assume
                        // the 'if' block is now closed
                        if (testToken.Text == "{") {
                            pos[sourcelevel]--;
                            // write ending if byte
                            tmpLogRes.WriteByte(0xFF);
                            return (true, false);
                        }
                        break;
                    }
                }
                // never leave loop normally; error, end of input, or successful
                // compilation of test commands will all exit loop directly
            } while (true);
        }

        private static void ValidateArgType(int argval, ArgType type, int argpos) {
            // FLAG, OBJECT, NUM, MSGNUM, VIEW, VAR
            // MSG, WORD, ANY, WORDLIST
            switch (type) {
            case Flag:
                // no additional checks
                break;
            case ArgType.Object:
                // not easily possible to tell if this is an inventory object
                // or screen object, so no additional checks here
                break;
            case Num:
                // numbers might be negative, or outside byte range
                if (ErrorLevel == Medium) {
                    if (argval > 255 || argval < -128) {
                        AddWarning(7003);
                    }
                    else if (argval < 0) {
                        AddWarning(5098);
                    }
                }
                break;
            case MsgNum:
                // 
                // msg 0 is not allowed
                if (argval == 0) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5091);
                    }
                    // make this a null msg
                    MsgInUse[0] = true;
                    MsgText[0] = "";
                }
                // confirm message exists
                else if (!MsgInUse[argval]) {
                    // message does not exist
                    AddError(6023, EngineResourceByNum(6023).Replace(
                        ARG1, argval.ToString()), false);
                    // use a null message as placeholder
                    MsgText[argval] = "";
                    MsgInUse[argval] = true;
                }
                break;
            case ArgType.View:
                // verify view exists
                if (ErrorLevel == Medium) {
                    if (!sCompGame.agViews.Contains(argval)) {
                        AddWarning(7005, EngineResourceByNum(7005).Replace(
                            ARG1, argval.ToString()));
                    }
                }
                break;
            case Var:
                // no additional checks
                break;
            }
        }

        /// <summary>
        /// This method checks the values passed as arguments to the specified action
        /// command and adds warnings or errors as needed.
        /// </summary>
        /// <param name="cmdNum"></param>
        /// <param name="argval"></param>
        /// <returns></returns>
        private static void ValidateArgs(int cmdNum, byte[] argval) {
            // for commands that can affect variable values, need to check against
            // reserved variables
            // for commands that can affect flags, need to check against reserved
            // flags
            // for other commands, check the passed arguments to see if values
            // are appropriate
            bool unload = false, warned = false;

            if (ErrorLevel == Low) {
                // only a few things to check
                switch (cmdNum) {
                case 0:
                    // return
                    // expect no more commands
                    endingCmd = 1;
                    break;
                case 18:
                // new.room(A)
                case 19:
                    // new.room.v
                    // expect no more commands
                    endingCmd = 2;
                    break;
                case 134:
                    // quit -
                    // if v2.089 or earlier OR if arg is non-zero
                    // no other commands will be processed
                    if (sCompGame.agIntVersion.Index == AGIVersion.v2089 || argval[0] > 0) {
                        endingCmd = 3;
                    }
                    break;
                }
                return;
            }
            else {
                switch (cmdNum) {
                case 0:
                    // return - expect no more commands
                    endingCmd = 1;
                    break;
                case 1 or 2 or (>= 4 and <= 8) or 10 or (>= 165 and <= 168):
                    // increment, decrement, assignv, addn, addv, subn, subv
                    // rindirect, mul.n, mul.v, div.n, div.v
                    // check for reserved variables that should never be manipulated
                    // (assume arg Value is zero to avoid tripping other checks)
                    CheckResVarUse(argval[0], 0);
                    // for div.n(vA, B) only, check for divide-by-zero
                    if (cmdNum == 167) {
                        if (argval[1] == 0) {
                            AddWarning(5030);
                        }
                    }
                    break;
                case 3:
                    // assignn
                    CheckResVarUse(argval[0], argval[1]);
                    break;
                case >= 12 and <= 14:
                    // set, reset, toggle
                    CheckResFlagUse(argval[0]);
                    break;
                case 18:
                    // new.room(A)
                    if (!sCompGame.agLogs.Contains(argval[0])) {
                        AddWarning(5053);
                    }
                    if (argval[0] == 0) {
                        AddWarning(5012);
                    }
                    // expect no more commands
                    endingCmd = 2;
                    break;
                case 19:
                    // new.room.v
                    // expect no more commands
                    endingCmd = 2;
                    break;
                case 20:
                    // load.logics(A)
                    if (!sCompGame.agLogs.Contains(argval[0])) {
                        AddWarning(5013);
                    }
                    break;
                case 22:
                    // call(A)
                    if (argval[0] == 0) {
                        // calling logic0 is a BAD idea
                        AddWarning(5010);
                    }
                    if (!sCompGame.agLogs.Contains(argval[0])) {
                        AddWarning(5076);
                    }
                    if (argval[0] == complogicNumber) {
                        // recursive calling is usually BAD
                        AddWarning(5089);
                    }
                    break;
                case 30:
                    // load.view(A)
                    // view related errors are caught by the ValidateArgType method
                    break;
                case 32:
                    // discard.view(A)
                    // view related errors are caught by the ValidateArgType method
                    break;
                case 37:
                    // position(oA, X,Y)
                    if (argval[1] > 159 || argval[2] > 167) {
                        AddWarning(5023);
                    }
                    break;
                case 39:
                    // get.posn
                    if (argval[1] <= 26 || argval[2] <= 26) {
                        AddWarning(5077, EngineResourceByNum(5077).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 41:
                    // set.view(oA, B)
                    // view related errors are caught by the ValidateArgType method
                    break;
                case (>= 49 and <= 53) or 97 or 118:
                    // last.cel, current.cel, current.loop,
                    // current.view, number.of.loops, get.room.v
                    // get.num
                    if (argval[1] <= 26) {
                        // variable arg is second and should not be a reserved Value
                        AddWarning(5077, EngineResourceByNum(5077).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 54:
                    // set.priority(oA, B)
                    if (argval[1] < 4 || argval[1] > 15) {
                        // invalid priority Value
                        AddWarning(5050);
                    }
                    break;
                case 57:
                    // get.priority
                    if (argval[1] <= 26) {
                        // variable is second argument and should not be a reserved Value
                        AddWarning(5077, EngineResourceByNum(5077).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 63:
                    // set.horizon(A)
                    if (argval[0] >= 167) {
                        AddWarning(5043);
                    }
                    else if (argval[0] > 120) {
                        AddWarning(5042);
                    }
                    else if (argval[0] < 16) {
                        AddWarning(5041);
                    }
                    break;
                case >= 64 and <= 66:
                    // object.on.water, object.on.land, object.on.anything
                    if (argval[0] == 0) {
                        // warn if used on ego
                        AddWarning(5082);
                    }
                    break;
                case 69:
                    // distance(oA, oB, vC)
                    if (argval[2] <= 26) {
                        // variable is third arg and should not be a reserved Value
                        AddWarning(5077, EngineResourceByNum(5077).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 73 or 75:
                    // end.of.loop, reverse.loop
                    if (argval[1] <= 15) {
                        // flag arg should not be a reserved Value
                        AddWarning(5078, EngineResourceByNum(5078).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 81:
                    // move.obj(oA, X,Y,STEP,fDONE)
                    if (argval[1] > 159 || argval[2] > 167) {
                        AddWarning(5062);
                    }
                    if (argval[0] == 0) {
                        // ego object forces program mode
                        AddWarning(5045);
                    }
                    if (argval[4] <= 15) {
                        // flag arg should not be a reserved Value
                        AddWarning(5078, EngineResourceByNum(5078).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 82:
                    // move.obj.v
                    if (argval[0] == 0) {
                        // ego object forces program mode
                        AddWarning(5045);
                    }
                    if (argval[4] <= 15) {
                        // flag arg should not be a reserved Value
                        AddWarning(5078, EngineResourceByNum(5078).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 83:
                    // follow.ego(oA, DISTANCE, fDONE)
                    if (argval[1] <= 1) {
                        AddWarning(5102);
                    }
                    if (argval[0] == 0) {
                        // ego can't follow ego
                        AddWarning(5027);
                    }
                    if (argval[2] <= 15) {
                        // flag arg should not be a reserved Value
                        AddWarning(5078, EngineResourceByNum(5078).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    CheckResFlagUse(argval[2]);
                    break;
                case 86:
                    // set.dir(oA, vB)
                    if (argval[0] == 0) {
                        // has no effect on ego object
                        AddWarning(5026);
                    }
                    break;
                case 87:
                    // get.dir (oA, vB)
                    if (argval[1] <= 26) {
                        // variable arg should not be a reserved Value
                        AddWarning(5077, EngineResourceByNum(5077).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 90:
                    // block(x1,y1,x2,y2)
                    if (argval[0] > 159 || argval[1] > 167 || argval[2] > 159 || argval[3] > 167) {
                        AddWarning(5020);
                    }
                    if ((argval[2] - argval[0] < 2) || (argval[3] - argval[1] < 2)) {
                        // invalid arguments
                        AddWarning(5051);
                    }
                    break;
                case 98:
                    // load.sound(A)
                    if (!sCompGame.agSnds.Contains(argval[0])) {
                        AddWarning(5014);
                    }
                    break;
                case 99:
                    // sound(A, fB)
                    if (!sCompGame.agSnds.Contains(argval[0])) {
                        AddWarning(5084);
                    }
                    if (argval[1] <= 15) {
                        // flag arg should not be a reserved Value
                        AddWarning(5078, EngineResourceByNum(5078).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    CheckResFlagUse(argval[1]);
                    break;
                case 103:
                    // display(ROW, COL, mC)
                    if (argval[0] > 24 || argval[1] > 39) {
                        AddWarning(5109);
                    }
                    break;
                case 105:
                    // clear.lines(TOP, BTM, C)
                    if (argval[0] > 24 || argval[1] > 24 || argval[0] > argval[1]) {
                        // top must be >btm; both must be <=24
                        AddWarning(5011);
                    }
                    if (argval[2] > 0 && argval[2] != 15) {
                        // color value should be 0 or 15 /(but it doesn't
                        // hurt to be anything else)
                        AddWarning(5100);
                    }
                    break;
                case 109:
                    // set.text.attribute(A,B)
                    if (argval[0] > 15 || argval[1] > 15) {
                        // color value should be 0 or 15 /(but it doesn't
                        // hurt to be anything else)
                        AddWarning(5029);
                    }
                    break;
                case 110:
                    // shake.screen(A)
                    if (argval[0] == 0) {
                        // zero is BAD
                        AddWarning(5057);
                    }
                    else if (argval[0] > 15) {
                        if (argval[0] >= 100 && argval[0] <= 109) {
                            // could be a palette change
                            AddWarning(5059);
                        }
                        else {
                            // shouldn't normally have more than a few shakes
                            AddWarning(5058);
                        }
                    }
                    break;
                case 111:
                    // configure.screen(TOP,INPUT,STATUS)
                    if (argval[0] > 3) {
                        // top should be <=3
                        AddWarning(5044);
                    }
                    if (argval[1] > 24 || argval[2] > 24) {
                        // input or status are invalid
                        AddWarning(5099);
                    }
                    if (argval[1] == argval[2]) {
                        // input and status should not be equal
                        AddWarning(5048);
                    }
                    if ((argval[1] >= argval[0] && argval[1] <= argval[0] + 20) ||
                        (argval[2] >= argval[0] && argval[2] <= argval[0] + 20)) {
                        // input and status should be <top or >=top+21
                        AddWarning(5049);
                    }
                    break;
                case 114:
                    // set.string(sA, mB)
                    if (argval[0] == 0) {
                        if (MsgText[argval[1]].Length > 10) {
                            // warn user if setting input prompt to unusually long value
                            AddWarning(5096);
                        }
                    }
                    break;
                case 115:
                    // get.string(sA, mB, ROW,COL,LEN)
                    if (argval[2] > 24) {
                        // if row>24, both row/col are ignored
                        AddWarning(5052);
                    }
                    if (argval[3] > 39) {
                        // if col>39, len is limited automatically to <=40
                        AddWarning(5080);
                    }
                    if (argval[4] > 40) {
                        // invalid len value
                        AddWarning(5056);
                    }
                    break;
                case 121:
                    // set.key(A,B,cC)
                    if (argval[0] > 0 && argval[1] > 0 && argval[0] != 1) {
                        // A or B must be zero to be valid ascii or keycode
                        // (A can be 1 to mean joystick)
                        AddWarning(5065);
                    }
                    // check for improper ASCII assignments
                    if (argval[1] == 0) {
                        if (argval[0] == 8 || argval[0] == 13 || argval[0] == 32) {
                            // ascii codes for bkspace, enter, spacebar
                            AddWarning(5066);
                        }
                    }
                    // check for improper KEYCODE assignments
                    if (argval[0] == 0) {
                        if ((argval[1] >= 71 && argval[1] <= 73) ||
                            (argval[1] >= 75 && argval[1] <= 77) ||
                            (argval[1] >= 79 && argval[1] <= 83)) {
                            // ascii codes arrow keys can't be assigned to controller
                            AddWarning(5066);
                        }
                    }
                    break;
                case 122:
                    // add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
                    if (!sCompGame.agViews.Contains(argval[0])) {
                        // view related errors are caught by the ValidateArgType method
                        warned = true;
                    }
                    if (!warned) {
                        try {
                            unload = !sCompGame.agViews[argval[0]].Loaded;
                            if (unload) {
                                sCompGame.agViews[argval[0]].Load();
                            }
                            // if view is valid, check loop
                            if (argval[1] >= sCompGame.agViews[argval[0]].Loops.Count) {
                                AddWarning(5085);
                                warned = true;
                            }
                            // if loop is valid, check cel
                            if (!warned) {
                                if (argval[2] >= sCompGame.agViews[argval[0]][argval[1]].Cels.Count) {
                                    AddWarning(5086);
                                }
                                if (sCompGame.agViews[argval[0]][argval[1]][argval[2]].Width < 3 && argval[6] < 4) {
                                    // CEL width must be >=3
                                    AddWarning(5110);
                                }
                            }
                        }
                        catch (Exception) {
                            // error trying to load- add a warning
                            AddWarning(5021, EngineResourceByNum(5021).Replace(
                                ARG1, argval[0].ToString()));
                        }
                        if (unload) {
                            sCompGame.agViews[argval[0]].Unload();
                        }
                    }
                    if (argval[3] > 159 || argval[4] > 167) {
                        // invalid x or y value
                        AddWarning(5038);
                    }
                    if (argval[5] > 15) {
                        // invalid priority value
                        AddWarning(5079);
                    }
                    if (argval[5] < 4 && argval[5] != 0) {
                        // unusual priority value
                        AddWarning(5079);
                    }
                    if (argval[6] > 15) {
                        // MGN values >15 will only use lower nibble
                        AddWarning(5101);
                    }
                    break;
                case 129:
                    // show.obj(VIEW)
                    // view related errors are caught by the ValidateArgType method
                    break;
                case 127 or 176 or 178:
                    // init.disk, hide.mouse, show.mouse
                    // these commands have no usefulness
                    AddWarning(5087, EngineResourceByNum(5087).Replace(
                        ARG1, ActionCommands[cmdNum].FanName));
                    break;
                case 175 or 179 or 180:
                    // discard.sound, fence.mouse, mouse.posn
                    // these commands not valid in MSDOS AGI
                    AddWarning(5088, EngineResourceByNum(5088).Replace(
                        ARG1, ActionCommands[cmdNum].FanName));
                    break;
                case 130:
                    // random(LOWER,UPPER,vRESULT)
                    if (argval[0] > argval[1]) {
                        // lower should be < upper
                        AddWarning(5054);
                    }
                    if (argval[0] == argval[1]) {
                        // lower=upper means result=lower=upper
                        AddWarning(5106);
                    }
                    if (argval[0] == argval[1] + 1) {
                        // this causes divide by 0!
                        AddWarning(5107);
                    }
                    if (argval[2] <= 26) {
                        // variable arg should not be a reserved Value
                        AddWarning(5077, EngineResourceByNum(5077).Replace(
                            ARG1, ActionCommands[cmdNum].FanName));
                    }
                    break;
                case 134:
                    // quit -
                    // if v2.089 or earlier OR if arg is non-zero
                    // no other commands will be processed
                    if (sCompGame.agIntVersion.Index == AGIVersion.v2089 || argval[0] > 0) {
                        endingCmd = 3;
                    }
                    break;
                case 142:
                    // script.size
                    if (complogicNumber != 0) {
                        // warn if in other than logic0
                        AddWarning(5039);
                    }
                    if (argval[0] < 10) {
                        // absurdly low value for script size
                        AddWarning(5009);
                    }
                    break;
                case 147:
                    // reposition.to(oA, B,C)
                    if (argval[1] > 159 || argval[2] > 167) {
                        AddWarning(5023);
                    }
                    break;
                case 150:
                    // trace.info(LOGIC,ROW,HEIGHT)
                    if (!sCompGame.agLogs.Contains(argval[0])) {
                        AddWarning(5040);
                    }
                    if (argval[2] < 2) {
                        AddWarning(5046);
                    }
                    if (argval[1] + argval[2] > 23) {
                        AddWarning(5063);
                    }
                    break;
                case 151 or 152:
                    // print.at(mA, ROW, COL, MAXWIDTH)
                    // print.at.v(vMSG, ROW, COL, MAXWIDTH)
                    if (argval[1] > 22) {
                        AddWarning(5067);
                    }
                    switch (argval[3]) {
                    case 0:
                        // maxwidth=0 defaults to 30
                        AddWarning(5105);
                        break;
                    case 1:
                        // maxwidth=1 crashes AGI
                        AddWarning(5103);
                        break;
                    default:
                        if (argval[3] > 36) {
                            // maxwidth >36 won't work
                            AddWarning(5104);
                        }
                        break;
                    }
                    if (argval[2] < 2 || argval[2] + argval[3] > 39) {
                        // invalid COL value
                        AddWarning(5068);
                    }
                    break;
                case 154:
                    // clear.text.rect(R1,C1,R2,C2,COLOR)
                    if (argval[0] > 24 || argval[1] > 39 ||
                       argval[2] > 24 || argval[3] > 39 ||
                       argval[2] < argval[0] || argval[3] < argval[1]) {
                        // invalid items
                        if (argval[2] < argval[0] || argval[3] < argval[1]) {
                            // pos2 < pos1
                            AddWarning(5069);
                        }
                        if (argval[0] > 24 || argval[1] > 39 ||
                           argval[2] > 24 || argval[3] > 39) {
                            // variables outside limits
                            AddWarning(5070);
                        }
                    }
                    if (argval[4] > 0 && argval[4] != 15) {
                        // color value should be 0 or 15  (but
                        // it doesn't hurt to be anything else)
                        AddWarning(5100);
                    }
                    break;
                case 156:
                    // set.menu(mA)
                    if (menuitemCount == 0) {
                        AddWarning(5113, EngineResourceByNum(5113).Replace(ARG1, lastMenu));
                    }
                    menuitemCount = 0;
                    lastMenu = MsgText[argval[0]];
                    if (menuSet) {
                        AddWarning(5117);
                    }
                    break;
                case 157:
                    // set.menu.item(mA, cB)
                    if (menuitemCount == 0) {
                        // first item determines width
                        menuWidth = MsgText[argval[0]].Length;
                    }
                    else {
                        if (MsgText[argval[0]].Length > menuWidth) {
                            // item is wider than first item
                            AddWarning(5118, EngineResourceByNum(5118).Replace(ARG1,
                                MsgText[argval[0]]));
                        }
                    }
                    menuitemCount++;
                    if (menuitemCount == 23) {
                        // more than 22 items in a menu won't fit
                        AddWarning(5114, EngineResourceByNum(5114).Replace(ARG1,
                            lastMenu));
                    }
                    if (menuSet) {
                        AddWarning(5117);
                    }
                    break;
                case 158:
                    // submit.menu()
                    if (menuSet) {
                        AddWarning(5116);
                    }
                    menuSet = true;
                    if (complogicNumber != 0) {
                        // should only be called in logic0
                        AddWarning(5112);
                    }
                    if (menuitemCount == -1) {
                        // no menus added
                        AddWarning(5115);
                    }
                    else if (menuitemCount == 0) {
                        // no items in this menu
                        AddWarning(5113, EngineResourceByNum(5113).Replace(ARG1, lastMenu));
                    }
                    break;
                case 174:
                    // set.pri.base(A)
                    if (argval[0] > 167) {
                        // value >167 doesn't make sense
                        AddWarning(5071);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// This method checks the values passed as arguments to the specified test
        /// command and adds warnings or errors as needed.
        /// </summary>
        /// <param name="CmdNum"></param>
        /// <param name="argval"></param>
        /// <returns></returns>
        private static void ValidateIfArgs(int CmdNum, byte[] argval) {
            switch (CmdNum) {
            case 11 or 16 or 17 or 18:
                // posn(oA, X1, Y1, X2, Y2)
                // obj.in.box(oA, X1, Y1, X2, Y2)
                // center.posn(oA, X1, Y1, X2, Y2)
                // right.posn(oA, X1, Y1, X2, Y2)
                if (argval[1] > 159 || argval[2] > 167 || argval[3] > 159 || argval[4] > 167) {
                    // invalid position
                    if (ErrorLevel == Medium) {
                        AddWarning(5072);
                    }
                }
                if ((argval[1] > argval[3]) || (argval[2] > argval[4])) {
                    // start and stop are backwards
                    if (ErrorLevel == Medium) {
                        AddWarning(5073);
                    }
                }
                break;
            }
        }

        /// <summary>
        /// This method determines if the specified token represents the start of
        /// a properly formatted special assignment syntax, and if so, parses it. 
        /// </summary>
        /// <param name="strArgIn"></param>
        /// <returns></returns>
        private static bool CompileSpecial(SierraToken argToken) {
            // assignn, assignv, addn, addv, subn, and subv can be replaced with
            // v# = #;
            // v# = v#;
            // v# += #;
            // v# += v#;
            // v# -= #;
            // v# -= v#;

            // left and right indirection can be replaced with
            // v# @= #;
            // v# @= v#;
            // v# =@ v#;

            byte cmdnum = 0;

            // validate arg number first
            int arg1 = argToken.Value;
            // next token should be assgnment symbol
            argToken = NextConvertedToken();
            if (argToken.Type == None) {
                // nothing left, return critical error
                AddError(4057, true);
                return false;
            }
            // assume variable assignment based on assignment symbol
            switch (argToken.Text) {
            case "++":
                // v#++; not allowed in sierra syntax
                AddError(6018, false);
                SkipToNextLine(argToken);
                return true;
            case "--":
                // v#-- not allowed in sierra syntax
                AddError(6018, false);
                SkipToNextLine(argToken);
                return true;
            case "+=":
                // v# += #; or v# += v#;
                cmdnum = 0x06;
                break;
            case "-=":
                // v# -= #; or v# -= v#;
                cmdnum = 0x08;
                break;
            case "*=":
            case "/=":
                // v# *= #; or v# *= v#;
                // v# /= #; v# /= v#
                // not allowed in sierra syntax
                AddError(6018, false);
                SkipToNextLine(argToken);
                return true;
            case "=":
                // assignment
                //     v# = v#;
                //     v# = #;
                cmdnum = 0x04;
                break;
            case "==":
                // error, but treat as '=' and continue
                AddError(4018, false);
                cmdnum = 0x04;
                break;
            case "@=":
                // v# @= #; v# @= v#;
                cmdnum = 0x09;
                break;
            case "=@":
                // v# =@ v#;
                cmdnum = 0x0A;
                break;
            default:
                // not a valid assignment
                AddError(4023, EngineResourceByNum(4023).Replace(
                    ARG1, argToken.Name), false);
                SkipToNextLine(argToken);
                return true;
            }
            // get second argument
            argToken = NextConvertedToken();
            if (argToken.Type == None) {
                // nothing left, return critical error
                AddError(4057, true);
                return false;
            }
            // arg2 may be v##, or ##
            int arg2 = argToken.Value;
            switch (argToken.Type) {
            case Var:
                // command is set correctly
                break;
            case Num:
                // use number version of command
                switch (cmdnum) {
                case 4:
                case 6:
                case 8:
                    cmdnum--;
                    break;
                case 0x09:
                    cmdnum = 0x0B;
                    break;
                case 0x0A:
                    // rindirect only allowed if arg2 is a variable
                    AddError(6019, false);
                    // adjust command and arg2 to placeholders
                    argToken.Type = Var;
                    arg2 = 255;
                    break;
                }
                ValidateArgType(arg2, argToken.Type, 1);
                break;
            default:
                // not a valid line
                AddError(4085, EngineResourceByNum(4085).Replace(
                    ARG1, argToken.Name), false);
                SkipToNextLine(argToken);
                return true;
            }
            // check for action command definition
            if (!ActionCommandDefined[cmdnum]) {
                AddError(6016, EngineResourceByNum(6016).Replace(
                    ARG1, ActionCommands[cmdnum].FanName), false);
            }
            // check for circular assignment- vA = vA;
            if (cmdnum == 0x04 && arg1 == arg2) {
                // circular assignment
                if (ErrorLevel == Medium) {
                    AddWarning(5036);
                }
            }

            // validate arguments for this command
            ValidateArgs(cmdnum, [(byte)arg1, (byte)arg2]);
            // write command and arg1
            tmpLogRes.WriteByte(cmdnum);
            tmpLogRes.WriteByte((byte)arg1);
            // write second argument
            tmpLogRes.WriteByte((byte)arg2);
            return true;
        }
        #endregion
    }
}
