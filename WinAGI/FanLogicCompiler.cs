using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
using static WinAGI.Engine.LogicErrorLevel;
using System.Diagnostics;
using WinAGI.Common;
using System.Text;

namespace WinAGI.Engine {
    /// <summary>
    /// This class contains all the members and methods needed to compile logic 
    /// source code using Fan AGI syntax rules, originally defined in the AGI 
    /// Specifications and expanded by WinAGI version 2.
    /// </summary>
    public static class FanLogicCompiler {
        #region Structs
        private struct LogicGoto {
            internal byte LabelNum;
            internal int DataLoc;
        }

        private struct LogicLabel {
            internal string Name;
            internal int Loc;
        }

        private class CompilerBlockType {
            internal bool IsIf = false;
            internal int StartPos = 0;
            internal int Length = 0;
            public CompilerBlockType() {
            }
        }
        
        private struct CompilerToken {
            internal ArgType Type = ArgType.Unknown;
            internal CompilerTokenSource Source = CompilerTokenSource.Code;
            internal int Line = -1;
            internal int StartPos = -1;
            internal int EndPos = -1;
            internal string Text = "";
            internal string Value = "";
            internal int NumValue {
                get {
                    switch (Type) {
                    case Num:
                    case ActionCmd:
                    case TestCmd:
                    case ArgType.Label:
                        return int.Parse(Value);
                    case Var:
                    case Flag:
                    case Msg:
                    case SObj:
                    case InvItem:
                    case Str:
                    case Word:
                    case Ctrl:
                        return int.Parse(Value[1..]);
                    case DefStr:
                    case VocWrd:
                    case Obj:
                    case ArgType.View:
                    case Symbol:
                    case AssignOperator:
                    case TestOperator:
                    case LineBreak:
                    case BadString:
                    case Keyword:
                    case Comment:
                    case None:
                        return -1;
                    }
                    return -1;
                }
            }
            internal bool Indirect = false;
            internal bool Pointer = false;
            public CompilerToken() {
            }
        }
        #endregion

        #region Enums
        private enum CompilerTokenSource {
                Code,
                LogicID,
                PictureID,
                SoundID,
                ViewID,
                LocalDefine,
                GlobalDefine,
                ReservedDefine,
            }

        private enum DefineType {
            // define type variable used to manage nature of the define
            Ignore = -1, // already handled or ignored; don't do anything
            Default,     // default define statement; anything goes
            Flag,        // flag only
            Variable,    // variable only
            Object,      // object (inv or screen) only
            View,        // view only
            TestCmd,     // test command
            ActionCmd,   // action command
            Redefine,    // redefining test/action
        }
        #endregion

        #region Members
        /// <summary>
        /// The parent game attached to the compiler.
        /// </summary>
        private static AGIGame fcompGame;
        private static Logic compLogic;
        // compiler warnings
        public const int WARNCOUNT = 118;
        private static bool[] noCompWarn = new bool[WARNCOUNT + 1];

        private static Logic tmpLogRes;
        private const string NOT_TOKEN = "!";
        private const string OR_TOKEN = "||";
        private const string AND_TOKEN = "&&";
        private const string NOTEQUAL_TOKEN = "!=";
        private const string EQUAL_TOKEN = "==";
        private const string CONST_TOKEN = "#define ";
        private const string MSG_TOKEN = "#message";
        private const string CMT1_TOKEN = "[";
        private const string CMT2_TOKEN = "//"; // deprecated
        private static byte complogicNumber;
        private static int errorLine;
        private static string errorModule = "";
        private static bool minorError, skipError = false;
        private static int menuitemCount = -1, menuWidth = 0;
        private static string lastMenu = "";
        private static bool menuSet = false;
        private static int endingCmd = 0; // 1 = return, 2 = new.room, 3 = quit
        private static bool includesID, includesReserved, includesGlobals;
        private static string INCLUDE_MARK = ((char)31).ToString() + "!";
        private static List<string> includeFileList = [];
        private static int includeOffset; // to correct line number due to added include lines
        private static List<string> inputLines = [];  // the entire text to be compiled; includes the
                                                     // original logic text, includes, and defines
        private static int lineNumber;
        private static int charPos;
        private static string currentLine;
        private static string[] MsgText = new string[256];
        private static bool[] MsgInUse = new bool[256];
        private static int[] MsgWarnings = new int[256]; // to track warnings found during msgread function
        private static List<LogicLabel> labelList = [];
        private static List<TDefine> definesList = [];
        internal static bool setIDs;
        private static string[] logIDs;
        private static string[] picIDs;
        private static string[] sndIDs;
        private static string[] viewIDs;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets how the compiler responds to errors and unusual conditions
        /// encountered in logic source code when compiling.
        /// </summary>
        public static LogicErrorLevel ErrorLevel { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// This method tells the logic compiler to ignore the specified warning
        /// if the condition is encountered while compiling.
        /// </summary>
        /// <param name="WarningNumber"></param>
        /// <param name="NewVal"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static void SetIgnoreWarning(int WarningNumber, bool NewVal) {
            if (WarningNumber < 5001 || WarningNumber > 5000 + WARNCOUNT) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            noCompWarn[WarningNumber - 5000] = NewVal;
        }
        
        /// <summary>
        /// Gets the value of the specified warning number's ignore status.
        /// </summary>
        /// <param name="WarningNumber"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static bool IgnoreWarning(int WarningNumber) {
            if (WarningNumber < 5001 || WarningNumber > 5000 + WARNCOUNT) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            return noCompWarn[WarningNumber - 5000];
        }

        /// <summary>
        /// This method determines if the specified define name is valid, including a check
        /// against this game's current global defines.
        /// </summary>
        /// <param name="CheckDef"></param>
        /// <returns>OK if define is valid, error value if not.</returns>
        private static DefineNameCheck ValidateDefineName(TDefine CheckDef, AGIGame game) {
            int i;
            // if already at default, just exit
            if (CheckDef.Name == CheckDef.Default) {
                return DefineNameCheck.OK;
            }
            // basic checks
            DefineNameCheck retval = BaseNameCheck(CheckDef.Name, false);
            if (retval != DefineNameCheck.OK) {
                return retval;
            }

            // check against globals
            if (game is not null && game.agIncludeGlobals) {
                if (game.GlobalDefines.IsChanged) {
                    game.GlobalDefines.Load();
                }
                for (i = 0; i < fcompGame.GlobalDefines.Count; i++) {
                    if (CheckDef.Name == fcompGame.GlobalDefines[i].Name)
                        return DefineNameCheck.Global;
                }
            }
            // check against basic reserved
            if (game is not null && game.agIncludeReserved) {
                // reserved variables
                TDefine[] tmpDefines = game.agReservedDefines.ByArgType(Var);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (CheckDef.Name == tmpDefines[i].Name) {
                        return ReservedVar;
                    }
                }
                // reserved flags
                tmpDefines = game.agReservedDefines.ByArgType(Flag);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (CheckDef.Name == tmpDefines[i].Name) {
                        return ReservedFlag;
                    }
                }
                // reserved numbers
                tmpDefines = game.agReservedDefines.ByArgType(Num);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (CheckDef.Name == tmpDefines[i].Name) {
                        return ReservedNum;
                    }
                }
                // reserved objects
                tmpDefines = game.agReservedDefines.ByArgType(SObj);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (CheckDef.Name == tmpDefines[i].Name) {
                        return ReservedObj;
                    }
                }
                // reserved strings
                tmpDefines = game.agReservedDefines.ByArgType(Str);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (CheckDef.Name == tmpDefines[i].Name) {
                        return ReservedStr;
                    }
                }
                // msg/defined strings
                tmpDefines = game.agReservedDefines.ByArgType(DefStr);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (CheckDef.Name == tmpDefines[i].Name) {
                        return ReservedMsg;
                    }
                }
            }
            // resourceIDs
            if (game is not null && game.agIncludeIDs) {
                if (!setIDs) {
                    SetResourceIDs(game);
                }
                for (i= 0; i < 256; i++) {
                    if (CheckDef.Name == logIDs[i]) {
                        return DefineNameCheck.ResourceID;
                    }
                    if (CheckDef.Name == picIDs[i]) {
                        return DefineNameCheck.ResourceID;
                    }
                    if (CheckDef.Name == sndIDs[i]) {
                        return DefineNameCheck.ResourceID;
                    }
                    if (CheckDef.Name == viewIDs[i]) {
                        return DefineNameCheck.ResourceID;
                    }
                }
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        /// <summary>
        /// This method determines if the specified string is a valid define name, including
        /// a check against global defines.
        /// </summary>
        /// <param name="CheckName"></param>
        /// <returns></returns>
        private static DefineNameCheck ValidateDefineName(string CheckName, AGIGame game) {
            TDefine CheckDef = new() {
                Name = CheckName
            };
            return ValidateDefineName(CheckDef, game);
        }

        /// <summary>
        /// This method determines if the specified define has a valid value. It also
        /// sets the define's Type property.
        /// </summary>
        /// <param name="TestDefine"></param>
        /// <returns></returns>
        internal static DefineValueCheck ValidateDefineValue(ref TDefine TestDefine, AGIGame game) {
            // default type
            TestDefine.Type = Unknown;
            if (TestDefine.Value.Length == 0) {
                return DefineValueCheck.Empty;
            }
            // values must be an AGI argument marker (variable/flag/etc), literal string, or a number

            if (int.TryParse(TestDefine.Value, out int intVal)) {
                // numeric
                // unsigned byte (0-255) or signed byte (-128 to 127) are OK
                TestDefine.Type = Num;
                if (intVal >= -128 && intVal < 256) {
                    return DefineValueCheck.OK;
                }
                else {
                    return OutofBounds;
                }
            }
            else {
                // non-numeric, check for arg marker
                if ("vfmoiswc".Contains(TestDefine.Value[0])) {
                    string strVal = TestDefine.Value[1..];
                    if (int.TryParse(strVal, out intVal)) {
                        // determine type
                        switch (TestDefine.Value[0]) {
                        case 'v':
                            TestDefine.Type = Var;
                            break;
                        case 'f':
                            TestDefine.Type = Flag;
                            break;
                        case 'm':
                            TestDefine.Type = Msg;
                            break;
                        case 'o':
                            TestDefine.Type = SObj;
                            break;
                        case 'i':
                            TestDefine.Type = InvItem;
                            break;
                        case 's':
                            TestDefine.Type = Str;
                            break;
                        case 'w':
                            TestDefine.Type = Word;
                            break;
                        case 'c':
                            TestDefine.Type = Ctrl;
                            break;
                        }
                        if (intVal < 0 || intVal > 255) {
                            return OutofBounds;
                        }
                        if (game is not null) {
                            // check defined globals
                            for (int i = 0; i < game.GlobalDefines.Count; i++) {
                                if (game.GlobalDefines[i].Value == TestDefine.Value)
                                    return DefineValueCheck.Global;
                            }
                            switch (TestDefine.Type) {
                            case Flag:
                                if (game.agIncludeReserved) {
                                    if (intVal <= 15)
                                        return Reserved;
                                    if (intVal == 20) {
                                        switch (game.agIntVersion) {
                                        case "3.002.098" or "3.002.102" or "3.002.107" or "3.002.149":
                                            return Reserved;
                                        }
                                    }
                                }
                                break;
                            case Var:
                                if (game.agIncludeReserved) {
                                    if (intVal <= 26) {
                                        return Reserved;
                                    }
                                }
                                break;
                            case Msg:
                                break;
                            case SObj:
                                if (game.agIncludeReserved) {
                                    // ego is reserved
                                    if (intVal == 0) {
                                        return Reserved;
                                    }
                                }
                                break;
                            case InvItem:
                                break;
                            case Str:
                                if (intVal > 23 || (intVal > 11 &&
                                  (game.agIntVersion == "2.089" ||
                                  game.agIntVersion == "2.272" ||
                                  game.agIntVersion == "3.002149"))) {
                                    return BadArgNumber;
                                }
                                break;
                            case Word:
                                // valid from w1 to w10
                                // applies to fanAGI syntax only;
                                // base is 1 because of how msg formatting
                                // uses words; compiler will automatically
                                // convert it to base zero when used - see
                                // WinAGI help file for more details
                                if (intVal < 1 || intVal > 10) {
                                    return BadArgNumber;
                                }
                                break;
                            case Ctrl:
                                // controllers limited to 0-49
                                if (intVal > 49) {
                                    return BadArgNumber;
                                }
                                break;
                            }
                        }
                        return DefineValueCheck.OK;
                    }
                }
                // non-numeric, non-marker and most likely a string
                if (IsAGIString(TestDefine.Value)) {
                    TestDefine.Type = DefStr;
                    return DefineValueCheck.OK;
                }
                else {
                    return NotAValue;
                }
            }
        }

        /// <summary>
        /// This method returns true if the specified string begins with a quote, and
        /// ends with a non-embedded quote.
        /// </summary>
        /// <param name="CheckString"></param>
        /// <returns></returns>
        public static bool IsAGIString(string CheckString) {
            if (string.IsNullOrEmpty(CheckString)) {
                return false;
            }
            if (CheckString.Length < 2) {
                return false;
            }
            if (CheckString[0] != '\"') {
                return false;
            }
            if (CheckString[^1] != '\"') {
                return false;
            }
            // validate that ending string is not an embedded (\") quote
            bool embedded = false;
            for (int i = 0; i < CheckString.Length; i++) {
                if (CheckString[i] != '\\') {
                    break;
                }
                embedded = !embedded;
            }
            return !embedded;
        }

        /// <summary>
        /// This method compiles the sourcetext for the specified logic.
        /// Errors and warnings are logged as they are encountered.
        /// </summary>
        /// <param name="SourceLogic"></param>
        /// <returns>true if logic compiles successfully, otherwise false</returns>
        internal static bool CompileFanLogic(Logic SourceLogic) {
            fcompGame = SourceLogic.parent;
            // if a major error occurs, return false
            // line is adjusted because
            // editor rows(lines) start at '1', but the compiler starts at line '0'
            bool compiled;
            List<string> stlSource;
            compLogic = SourceLogic;

            if (fcompGame.IncludeGlobals && !fcompGame.GlobalDefines.Loaded) {
                fcompGame.GlobalDefines.Load();
            }
            if (!setIDs) {
                SetResourceIDs(fcompGame);
            }
            // always reset command name assignments
            CorrectCommands(fcompGame.agIntVersion);

            // refresh  values for game specific reserved defines
            fcompGame.agReservedDefines.GameInfo[0].Value = "\"" + fcompGame.agGameID + "\"";
            fcompGame.agReservedDefines.GameInfo[1].Value = "\"" + fcompGame.agGameVersion + "\"";
            fcompGame.agReservedDefines.GameInfo[2].Value = "\"" + fcompGame.agGameAbout + "\"";
            fcompGame.agReservedDefines.GameInfo[3].Value = (fcompGame.InvObjects.Count).ToString();

            // get source text by lines as a list of strings
            stlSource = SourceLogic.SourceText.SplitLines();
            complogicNumber = SourceLogic.Number;
            minorError = false;
            includeFileList = [];
            // add includes
            if (!AddIncludes(stlSource)) {
                return false;
            }
            // remove any blank lines from end
            while (inputLines[^1].Length == 0 && inputLines.Count > 0) {
                inputLines.RemoveAt(inputLines.Count - 1);
            }
            // if nothing to compile, return major error
            if (inputLines.Count == 0) {
                AddError(4083, true);
                return false;
            }
            // strip out all comments
            if (!RemoveComments()) {
                return false;
            }
            // check for labels
            ReadLabels();
            // enumerate and replace all the defines
            if (!ReadDefines()) {
                return false;
            }
            // read predefined messages
            if (!ReadMsgs()) {
                return false;
            }
            tmpLogRes = new Logic {
                Data = []
            };
            // write a place holder 2-byte (word) value for offset to msg section start
            tmpLogRes.WriteWord(0, 0);
            // main agi compiler
            compiled = CompileFAN();
            if (!compiled) {
                tmpLogRes.Unload();
                return false;
            }
            // code size equals bytes currently written (before msg secion added)
            tmpLogRes.CodeSize = tmpLogRes.Size;
            // add message section
            if (!WriteMsgs()) {
                tmpLogRes.Unload();
                return false;
            }
            if (!minorError) {
                // no errors, save compiled data
                SourceLogic.Data = tmpLogRes.Data;
                SourceLogic.CompiledCRC = SourceLogic.CRC;
                fcompGame.WriteGameSetting("Logic" + SourceLogic.Number.ToString(), "CRC32", "0x" + SourceLogic.CRC.ToString("x8"), "Logics");
                fcompGame.WriteGameSetting("Logic" + SourceLogic.Number.ToString(), "CompCRC32", "0x" + SourceLogic.CompiledCRC.ToString("x8"), "Logics");
            }
            // done with the temp resource
            tmpLogRes.Unload();

            // if minor errors, report them as a compile fail
            if (minorError) {
                return false;
            }
            else {
                return true;
            }
        }

        /// <summary>
        /// This method builds an array of resourceIDs so ConvertArg function
        /// can iterate through them much quicker.
        /// </summary>
        /// <param name="game"></param>
        internal static void SetResourceIDs(AGIGame game) {
            if (setIDs) {
                return;
            }
            List<string> resIDlist = [
                "[ Resource ID Defines for " + game.agGameID,
                "[",
                "[ WinAGI generated code required for IncludeResourceIDs support - ",
                "[ do not modify the contents of this file with the code editor.",
                ""];
            logIDs = new string[256];
            picIDs = new string[256];
            sndIDs = new string[256];
            viewIDs = new string[256];
            Array.Fill(logIDs, "");
            Array.Fill(picIDs, "");
            Array.Fill(sndIDs, "");
            Array.Fill(viewIDs, "");
            resIDlist.Add("[ Logics");
            foreach (Logic tmpLog in game.agLogs) {
                logIDs[tmpLog.Number] = tmpLog.ID;
                resIDlist.Add("#define " + tmpLog.ID.PadRight(17) + tmpLog.Number.ToString().PadLeft(5));
            }
            resIDlist.Add("");
            resIDlist.Add("[ Pictures");
            foreach (Picture tmpPic in game.agPics) {
                picIDs[tmpPic.Number] = tmpPic.ID;
                resIDlist.Add("#define " + tmpPic.ID.PadRight(17) + tmpPic.Number.ToString().PadLeft(5));
            }
            resIDlist.Add("");
            resIDlist.Add("[ Sounds");
            foreach (Sound tmpSnd in game.agSnds) {
                sndIDs[tmpSnd.Number] = tmpSnd.ID;
                resIDlist.Add("#define " + tmpSnd.ID.PadRight(17) + tmpSnd.Number.ToString().PadLeft(5));
            }
            resIDlist.Add("");
            resIDlist.Add("[ Views");
            foreach (View tmpView in game.agViews) {
                viewIDs[tmpView.Number] = tmpView.ID;
                resIDlist.Add("#define " + tmpView.ID.PadRight(17) + tmpView.Number.ToString().PadLeft(5));
            }
            // save defines file
            try {
                using FileStream fsList = new FileStream(game.agResDir + "resourceids.txt", FileMode.Create);
                using StreamWriter swList = new StreamWriter(fsList);
                foreach (string line in resIDlist) {
                    swList.WriteLine(line);
                }
            }
            catch {
                // ignore errors for now
            }
            setIDs = true;
        }

        private static void AddError(int ErrorNum, bool critical, int line = -1) {
            AddError(ErrorNum, LoadResString(ErrorNum), critical, line);
        }

        /// <summary>
        /// Reports errors back to calling thread. Minor errors don't cancel
        /// the compile method, but major (critical) errors do.
        /// </summary>
        /// <param name="ErrorNum"></param>
        /// <param name="ErrorText"></param>
        private static void AddError(int ErrorNum, string ErrorText, bool critical, int line = -1) {
            if (line == -1) {
                line = errorLine;
            }

            if (!skipError) {
                TWinAGIEventInfo errInfo = new() {
                    Type = EventType.LogicCompileError,
                    Line = line.ToString(),
                    ID = ErrorNum.ToString(),
                    Text = ErrorText,
                    ResType = AGIResType.Logic,
                    ResNum = complogicNumber,
                    Module = errorModule.Length > 0 ? Path.GetFileName(errorModule) : compLogic.ID,
                    Filename = errorModule,
                };
                OnCompileLogicStatus(fcompGame, errInfo);
            }
            if (!critical) {
                minorError = true;
            }
        }

        /// <summary>
        /// This method passes through the input logic and replaces all include statements
        /// with the contents of the specified include file.
        /// </summary>
        /// <param name="logicText"></param>
        /// <returns>true if include files added successfully. false if unable to 
        /// add include files.</returns>
        private static bool AddIncludes(List<string> logicText) {
            // include file lines are given a marker to identify them as belonging to
            // an include file
            string lineText;
            // begin with empty array of source lines
            inputLines = [];
            includesID = false;
            includesReserved = false;
            includesGlobals = false;
            for (lineNumber = 0; lineNumber < logicText.Count; lineNumber++) {
                // cache error line
                errorLine = lineNumber;
                // get next line, minus tabs and spaces
                lineText = logicText[lineNumber].Replace('\t', ' ').TrimStart();
                if (lineText.Left(2) == INCLUDE_MARK) {
                    // check for any instances of the marker, since these will
                    // interfere with include line handling ! SHOULD NEVER
                    // HAPPEN, but just in case
                    AddError(4041, true);
                    return false;
                }
                // check this line for include statement, and insert if found
                switch (CheckInclude(lineText)) {
                case 0:
                    // not an include line; add it to the input
                    inputLines.Add(lineText);
                    break;
                case 1:
                    // include file inserted or invalid include file
                    // add a blank line as a place holder for the 'include' line
                    // (to keep line counts accurate when calculating line number
                    // for errors)
                    inputLines.Add("");
                    break;
                case 2:
                    // resourceids.txt - skip this include file
                    includesID = true;
                    inputLines.Add("");
                    break;
                case 3:
                    // reserved.txt - skip this include file
                    includesReserved = true;
                    inputLines.Add("");
                    break;
                case 4:
                    // globals.txt - skip this include file
                    includesGlobals = true;
                    inputLines.Add("");
                    break;
                default: // -1 = error
                    return false;
                }
            }
            // confirm auto-includes were added
            if (fcompGame.IncludeIDs && !includesID) {
                AddError(4089, LoadResString(4089).Replace(ARG1, "resourceids.txt"), true);
                return false;
            }
            if (fcompGame.IncludeReserved && !includesReserved) {
                AddError(4089, LoadResString(4089).Replace(ARG1, "reserved.txt"), true);
                return false;
            }
            if (fcompGame.IncludeGlobals && !includesGlobals) {
                AddError(4089, LoadResString(4089).Replace(ARG1, "globals.txt"), true);
                return false;
            }
            // success
            return true;
        }

        /// <summary>
        /// This method inserts the contents of the include file from the specified
        /// line into the source input line array.
        /// </summary>
        /// <param name="lineText"></param>
        /// <returns>1 if include file added successfully<br />
        /// 0 if line does not contain an include file<br />
        /// -1 if an error is encountered</returns>
        private static int CheckInclude(string lineText) {
            List<string> IncludeLines;
            string includeFilename, includeText;
            int includeNum, currentLine;
            int retval;

            if (lineText.Left(9) == "%include ") {
                AddError(4036, LoadResString(4036).Replace(ARG1, "%include"), false);
                return 1;
            }
            else if (lineText.Left(9) != "#include ") {
                // not an include line
                return 0;
            }
            // check for a missing filename
            if (lineText.Trim().Length == 8) {
                AddError(4035, false);
                return 1;
            }
            includeFilename = lineText[9..].Trim();
            // check for single quote
            if (includeFilename == "\"") {
                AddError(4035, false);
                return 1;
            }
            // file name should be in quotes
            if (includeFilename.Length < 2) {
                AddError(4069, false);
                return 1;
            }
            if (includeFilename[0] != QUOTECHAR) {
                AddError(4069, false);
            }
            includeFilename = includeFilename[1..];
            // extract filename by getting last quote
            int pos = includeFilename.LastIndexOf('\"');
            if (pos == -1) {
                // error - no trailing quote
                AddError(4069, false);
                // add one as a placeholder
                includeFilename += "\"";
            }
            else {
                if (pos != includeFilename.Length - 1) {
                    // extra stuff on the line
                    string extra = includeFilename[(pos + 1)..].Trim();
                    includeFilename = includeFilename[..(pos + 1)];
                    if (extra[0] == ';') {
                        AddError(4053, false);
                    }
                    else {
                        AddError(4054, false);
                    }
                }
            }
            // strip off trailing quote
            includeFilename = includeFilename[..^1].Trim();
            if (includeFilename.Length == 0) {
                AddError(4035, false);
                return 1;
            }
            // check for path
            if (JustPath(includeFilename, true).Length == 0) {
                // use resource dir for this include file
                includeFilename = fcompGame.agResDir + includeFilename;
            }
            // convert filename to absolute
            try {
                includeFilename = FullFileName(fcompGame.agResDir, includeFilename);
            }
            catch {
                AddError(4028, LoadResString(4028).Replace(ARG1, includeFilename), false);
                return 1;
            }
            // should never happen, but...
            if (includeFilename == compLogic.SourceFile) {
                // !! don't allow this - it will hang up the compiler
                AddError(4088, false);
                return 1;
            }
            // now verify file exists
            if (!File.Exists(includeFilename)) {
                AddError(4028, LoadResString(4028).Replace(ARG1, includeFilename), false);
                return 1;
            }
            if (fcompGame.IncludeIDs && includeFilename == fcompGame.agResDir + "resourceids.txt") {
                // skip it- IDs handled separately
                return 2;
            }
            if (fcompGame.IncludeReserved && includeFilename == fcompGame.agResDir + "reserved.txt") {
                // skip it- reserved defines handled separately
                return 3;
            }
            if (fcompGame.IncludeGlobals && includeFilename == fcompGame.agResDir + "globals.txt") {
                // skip it- globals handled separately
                return 4;
            }
            // check if already included
            for (int i = 0; i < includeFileList.Count; i++) {
                if (includeFilename == includeFileList[i]) {
                    // if the include file has already been added, don't add it again (to prevent include recursion!)
                    AddWarning(5022, LoadResString(5022).Replace(ARG1, includeFilename));
                    return 1;
                }
            }
            // now open the include file, and get the text
            try {
                includeText = Encoding.GetEncoding(compLogic.CodePage).GetString(File.ReadAllBytes(includeFilename));
            }
            catch (Exception) {
                AddError(4032, LoadResString(4032).Replace(ARG1, includeFilename), true);
                return -1;
            }
            IncludeLines = includeText.SplitLines();
            if (IncludeLines.Count > 0) {
                // save file name to allow for error checking
                includeNum = includeFileList.Count;
                includeFileList.Add(includeFilename);
                // add all these lines into this position
                for (currentLine = 0; currentLine < IncludeLines.Count; currentLine++) {
                    // get next line, minus tabs and spaces
                    includeText = IncludeLines[currentLine].Replace('\t', ' ').Trim();
                    // check for any instances of the marker, since these will
                    // interfere with include line handling
                    if (includeText.Length > 1 && includeText[..2] == INCLUDE_MARK) {
                        AddError(4041, true, currentLine);
                        return -1;
                    }
                    // check for nested include files
                    retval = CheckInclude(includeText);
                    switch (retval) {
                    case 0:
                        // not an embedded include
                        // include filenumber and line number for this includefile
                        inputLines.Add(INCLUDE_MARK + includeNum.ToString() + ":" + currentLine.ToString() + "#" + includeText);
                        break;
                    case 1:
                        // include lines added
                        // do nothing (don't need a blank line; only one blank line
                        // is needed for each 'root' include file)
                        break;
                    case 2:
                        // resourceids.txt - skip this include file
                        includesID = true;
                        inputLines.Add("");
                        break;
                    case 3:
                        // reserved.txt - skip this include file
                        includesReserved = true;
                        inputLines.Add("");
                        break;
                    case 4:
                        // globals.txt - skip this include file
                        includesGlobals = true;
                        inputLines.Add("");
                        break;
                    case -1:
                        // pass error along
                        return -1;
                    }
                }
            }
            // success
            return 1;
        }

        /// <summary>
        /// This method returns a string representing the name of the specified
        /// argument type.
        /// </summary>
        /// <param name="argtype"></param>
        /// <returns></returns>
        private static string ArgTypeName(ArgType argtype) {
            switch (argtype) {
            case Num:
                return "number";
            case Var:
                return "variable";
            case Flag:
                return "flag";
            case Msg:
                return "message";
            case SObj:
                return "screen object";
            case InvItem:
                return "inventory item";
            case Str:
                return "string";
            case Word:
                return "word";
            case Ctrl:
                return "controller";
            case DefStr:
                return "text in quotes";
            case VocWrd:
                return "vocabulary word";
            default:
                // ignore other types
                return "";
            }
        }

        /// <summary>
        /// This method checks for reserved flag (f0-f20) use and adds warnings
        /// as appropriate.
        /// </summary>
        /// <param name="argval"></param>
        private static void CheckResFlagUse(byte argval) {
            if (ErrorLevel == Low) {
                return;
            }
            if (argval == 2 ||
                argval == 4 ||
                (argval >= 7 && argval <= 10) ||
                argval >= 13) {
                //    f2: haveInput
                //    f4: haveMatch
                //    f7: script_buffer_blocked
                //    f8: joystick sensitivity set
                //    f9: sound_on
                //    f10: trace_abled
                //    f13: inventory_select_enabled
                //    f14: menu_enabled
                //    f15: windows_remain
                //    f20: auto_restart
                //    >f21: non-reserved
                // no restrictions
            }
            else {
                // all other reserved flags should be read only
                AddWarning(5025, LoadResString(5025).Replace(ARG1, fcompGame.agReservedDefines.ReservedFlags[argval].Name));
            }
        }

        /// <summary>
        /// This method checks for reserved variable (v0-v26) use and adds warnings
        /// as appropriate.
        /// </summary>
        /// <param name="argnum"></param>
        /// <param name="argval"></param>
        private static void CheckResVarUse(byte argnum, byte argval) {
            if (ErrorLevel == Low) {
                return;
            }
            switch (argnum) {
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
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, fcompGame.ReservedDefines.ReservedVariables[6].Name).Replace(ARG2, "8"));
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
                    AddWarning(5092, LoadResString(5092).Replace(ARG1, fcompGame.ReservedDefines.ReservedVariables[argnum].Name));
                }
                break;
            case 19:
                // key_pressed value
                // ok if resetting for key input
                if (argval > 0) {
                    AddWarning(5017, LoadResString(5017).Replace(ARG1, fcompGame.ReservedDefines.ReservedVariables[argnum].Name));
                }
                break;
            case 23:
                // sound attenuation
                // restrict to 0-15
                if (argval > 15) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, fcompGame.ReservedDefines.ReservedVariables[23].Name).Replace(ARG2, "15"));
                }
                break;
            case 24:
                // max input length
                if (argval > 39) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, fcompGame.ReservedDefines.ReservedVariables[24].Name).Replace(ARG2, "39"));
                }
                break;
            default:
                // all other reserved variables should be read only
                AddWarning(5017, LoadResString(5017).Replace(ARG1, fcompGame.ReservedDefines.ReservedVariables[argnum].Name));
                break;
            }
        }

        /// <summary>
        /// This method increments the current line of input being processed and
        /// resets all counters, pointers, etc., as well as info needed to support
        /// error locating.
        /// </summary>
        private static void IncrementLine() {
            // check for end of input
            if (lineNumber == -1) {
                return;
            }
            // check for reset 
            if (lineNumber == -2) {
                // set it to -1 so line 0 is returned
                lineNumber = -1;
            }
            do {
                skipError = false;
                lineNumber++;
                charPos = -1;
                if (lineNumber >= inputLines.Count) {
                    lineNumber = -1;
                    return;
                }
                // check for include lines
                if (inputLines[lineNumber].Left(2) == INCLUDE_MARK) {
                    includeOffset++;
                    // set module
                    int mod = int.Parse(inputLines[lineNumber][2..(inputLines[lineNumber].IndexOf(':'))]);
                    errorModule = includeFileList[mod];
                    // set error line
                    errorLine = int.Parse(inputLines[lineNumber][(inputLines[lineNumber].IndexOf(':') + 1)..inputLines[lineNumber].IndexOf('#')]);
                    // get the line without include tag
                    currentLine = inputLines[lineNumber][(inputLines[lineNumber].IndexOf('#') + 1)..];
                }
                else {
                    errorModule = "";
                    errorLine = lineNumber - includeOffset;
                    // get the next line
                    currentLine = inputLines[lineNumber];
                }
                // skip over blank lines
            } while (currentLine.Length == 0);
        }

        /// <summary>
        /// Gets the next character and compares it to the specified value. The
        /// method returns true if the character matches. The next character is 
        /// returned to the input stream if the character does not match.
        /// </summary>
        /// <param name="charval"></param>
        /// <param name="noNewLine"></param>
        /// <returns></returns>
        private static bool CheckChar(char charval, bool noNewLine = false) {
            char testChar;

            testChar = NextChar(noNewLine);
            if (testChar != charval) {
                // no match; need to back up
                // unless nothing was found (meaning end of line or input)
                if (testChar != 0) {
                    charPos--;
                }
            }
            return testChar == charval;
        }

        /// <summary>
        /// Gets the next token from the source code input stream but does not advance
        /// the line or position, i.e. the token remains in the input stream.
        /// </summary>
        /// <param name="noNewLine"></param>
        /// <returns></returns>
        private static CompilerToken PeekToken(bool noNewLine = false) {
            // save compiler state
            int tmpPos = charPos;
            int tmpLine = lineNumber;
            int tmpInclOffset = includeOffset;
            string tmpModule = errorModule;
            string tmpCurLine = currentLine;
            int tmpErrLine = errorLine;
            // get next token
            CompilerToken peekcmd = NextToken(noNewLine);
            // restore compiler state
            charPos = tmpPos;
            lineNumber = tmpLine;
            includeOffset = tmpInclOffset;
            errorModule = tmpModule;
            currentLine = tmpCurLine;
            errorLine = tmpErrLine;
            // return the token
            return peekcmd;
        }

        /// <summary>
        /// Gets the next non-space character from the source code input stream. If 
        /// the NoNewLine flag is true, the method will not look past the current
        /// line.
        /// </summary>
        /// <param name="noNewLine"></param>
        /// <returns>The next character, or a null character if end of input reached.</returns>
        private static char NextChar(bool noNewLine = false) {
            // If the NoNewLine flag is passed, the function will not look past
            // current line for next character. If no character on current line,
            // lngPos is set to end of current line, and an empty string is
            // returned.

            char nextchar = '\0';

            // if already at end of input (lngLine=-1)
            if (lineNumber == -1) {
                return (char)0;
            }
            do {
                charPos++;
                if (charPos >= currentLine.Length) {
                    if (noNewLine) {
                        // move pointer back
                        charPos--;
                        // return empty char
                        return (char)0;
                    }
                    IncrementLine();
                    if (lineNumber == -1) {
                        // exit with no character
                        return (char)0;
                    }
                    charPos++;
                }
                nextchar = currentLine[charPos];
                if (nextchar < 32) {
                    // treat as a space
                    nextchar = ' ';
                }
            } while (nextchar == 32);
            return nextchar;
        }

        /// <summary>
        /// Gets the next token from the source code input stream. Tokens are
        /// defined as one or more token characters, separated by a token
        /// separator.  If the NoNewLine flag is true, the method will not
        /// look past the line.
        /// </summary>
        /// <param name="noNewLine"></param>
        /// <returns>The next token, or null string if end of input reached.</returns>
        private static CompilerToken NextToken(bool noNewLine = false, bool textonly = false) {
            // token characters include:
            // characters a-z and A-Z, numbers 0-9, special characters  #$%.@_
            // and extended characters [128-255]
            // the double quote '"' is used to mark text strings; text
            // strings are also considered tokens;  ALL characters,
            // including all token separators, are considered token
            // elements when inside quotes
            //
            // element separators include:
            //  space, !"&'()*+,-/:;<=>?[\]^`{|}~
            //
            // element separators other than space are normally returned
            // as a single character token; there are some exceptions
            // where element separators will include additional characters:
            //  !=, &&, *=, ++, +=, --, -=, /=, //, <=, <>, =<, ==, =>, >=, ><, ||
            //
            // if end of input is reached, next token is an empty string

            bool blnInQuotes = false, blnSlash = false;
            CompilerToken next = new();
            // find next non-blank character
            string retval = NextChar(noNewLine).ToString();
            // if at end of input,
            if (lineNumber == -1) {
                // return empty token
                next.Type = ArgType.None;
                return next;
            }
            // if no character returned
            if (retval == "" || retval == "\0") {
                // return empty token
                next.Type = ArgType.None;
                return next;
            }
            next.Line = lineNumber;
            next.StartPos = charPos;
            // single character separators:
            if ("(),:;[\\]^`{}~".Any(retval.Contains)) {
                next.Type = ArgType.Symbol;
                next.EndPos = charPos + 1;
                next.Value = next.Text = retval;
                return next;
            }
            // check for other characters
            switch (retval[0]) {
            case '\'' or '?':
                next.Type = ArgType.Symbol;
                break;
            case '=':
                // special case; "=", "==", "=<" and "=>" returned as separate
                // tokens (also "=@" in case Sierra code is intermingled)
                next.Type = ArgType.AssignOperator;
                if (charPos + 1 < currentLine.Length) {
                    switch (currentLine[charPos + 1]) {
                    case '<' or '>':
                        next.Type = ArgType.TestOperator;
                        charPos++;
                        // swap so we get ">=" and "<=" instead of "=>" and "=<"
                        retval = currentLine[charPos].ToString() + retval;
                        break;
                    case '=':
                        next.Type = ArgType.TestOperator;
                        charPos++;
                        retval = "==";
                        break;
                    case '@':
                        charPos++;
                        retval = "=@";
                        break;
                    }
                }
                break;
            case '\"':
                // special case; quote means start of a string
                blnInQuotes = true;
                break;
            case '+':
                // special case; "+", "++" and "+=" returned as separate tokens
                next.Type = ArgType.AssignOperator;
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '+') {
                        charPos++;
                        retval = "++";
                    }
                    else if (currentLine[charPos + 1] == '=') {
                        charPos++;
                        retval = "+=";
                    }
                }
                break;
            case '-':
                // special case; "-", "--" and "-=" returned as separate tokens
                // also check for negative numbers ("-##")
                next.Type = ArgType.AssignOperator;
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '-') {
                        charPos++;
                        retval = "--";
                    }
                    else if (currentLine[charPos + 1] == '=') {
                        charPos++;
                        retval = "-=";
                    }
                    else if (currentLine[charPos + 1] >= 48 && currentLine[charPos + 1] <= 57) {
                        // return a negative number
                        next.Type = ArgType.Num;
                        while (charPos + 1 < currentLine.Length) {
                            char aChar = currentLine[charPos + 1];
                            if (aChar < 48 || aChar > 57) {
                                // anything other than a digit (0-9)
                                break;
                            }
                            else {
                                // add the digit
                                retval += aChar;
                                charPos++;
                            }
                        }
                    }
                }
                break;
            case '!':
                next.Type = ArgType.Symbol;
                // special case; "!" and "!=" returned as separate tokens
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '=') {
                        next.Type = ArgType.TestOperator;
                        charPos++;
                        retval = "!=";
                    }
                }
                break;
            case '<':
                next.Type = ArgType.TestOperator;
                // special case; "<", "<=" returned as separate tokens
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '=') {
                        charPos++;
                        retval = "<=";
                    }
                }
                break;
            case '>':
                next.Type = ArgType.TestOperator;
                // special case; ">", ">=" returned as separate tokens
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '=') {
                        charPos++;
                        retval = ">=";
                    }
                }
                break;
            case '*':
                // special case; "*/" and "*=" returned as separate tokens;
                // "*" followed by alpha/numeric is treated as part of a token
                // otherwise it is a single char token
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '=') {
                        next.Type = ArgType.AssignOperator;
                        charPos++;
                        retval = "*=";
                    }
                    // since block tokens are no longer supported, check for them
                    // in order to provide a meaningful error message
                    else if (currentLine[charPos + 1] == '/') {
                        next.Type = ArgType.Symbol;
                        charPos++;
                        retval = "*/";
                    }
                    else if (char.IsAsciiLetter(currentLine[charPos + 1]) ||
                             char.IsDigit(currentLine[charPos + 1]) ||
                             "#$%.@_".Contains(currentLine[charPos + 1])) {
                        // characters a-z and A-Z, numbers 0-9, special characters  #$%.@_
                    }
                    else {
                        next.Type = ArgType.Symbol;
                    }

                }
                else {
                    next.Type = ArgType.AssignOperator;
                }
                    break;
            case '/':
                next.Type = ArgType.TestOperator;
                // special case; "/" , "//", "/*" and "/=" returned as separate tokens
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '=') {
                        next.Type = ArgType.AssignOperator;
                        charPos++;
                        retval = "/=";
                    }
                    else if (currentLine[charPos + 1] == '/') {
                        next.Type = ArgType.Symbol;
                        charPos++;
                        retval = "//";
                    }
                    // since block tokens are no longer supported, check for them
                    // in order to provide a meaningful error message
                    else if (currentLine[charPos + 1] == '*') {
                        next.Type = ArgType.Symbol;
                        charPos++;
                        retval = "/*";
                    }
                }
                break;
            case '|':
                next.Type = ArgType.Symbol;
                // special case; "|" and "||" returned as separate tokens
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '|') {
                        next.Type = ArgType.TestOperator;
                        charPos++;
                        retval = "||";
                    }
                }
                break;
            case '&':
                // special case; "&&" returned as separate token
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '&') {
                        next.Type = ArgType.TestOperator;
                        charPos++;
                        retval = "&&";
                    }
                }
                // '&' isn't a single-char token
                break;
            case '@':
                // '@' isn't a single-char token
                // special case; "@=" returned as separate token
                // (in case Sierra code is intermingled)
                if (charPos + 1 < currentLine.Length) {
                    if (currentLine[charPos + 1] == '=') {
                        next.Type = ArgType.AssignOperator;
                        charPos++;
                        retval = "@=";
                    }
                }
                break;
            }

            // if a symbol or a negative number, return it
            if (next.Type != ArgType.Unknown) {
                next.EndPos = charPos + 1;
                next.Value = next.Text = retval;
                return next;
            }

            // non-symbol; either a token or a quoted text string
            if (blnInQuotes) {
                // a text string - 
                // if past end of line (which could only happen if a line contains
                // a single double quote on it)
                if (charPos + 1 >= currentLine.Length) {
                    // return the single quote
                    next.Type = BadString;
                    next.EndPos = charPos + 1;
                    next.Value = next.Text = retval;
                    return next;
                }
                next.Type = DefStr;
                // add characters until another TRUE quote is found
                do {
                    // increment position
                    charPos++;
                    if (charPos == currentLine.Length) {
                        // if still in quotes,
                        if (blnInQuotes) {
                            // set inquotes to false to exit the loop the
                            // compiler will deal with missing quote later
                            next.Type = ArgType.BadString;
                            blnInQuotes = false;
                        }
                        break;
                    }
                    char nextchar = currentLine[charPos];
                    // if last char was a slash, next char is just added as-is
                    if (blnSlash) {
                        // always reset  the slash
                        blnSlash = false;
                    }
                    else {
                        // check for slash or quote mark
                        if (nextchar == '"') {
                            // a quote marks end of string
                            blnInQuotes = false;
                        }
                        else if (nextchar == '\\') {
                            blnSlash = true;
                        }
                    }
                    retval += nextchar;
                } while (blnInQuotes);
                next.Value = next.Text = retval;
            }
            else {
                // continue adding characters until element separator or EOL is reached
                while (charPos + 1 < currentLine.Length) {
                    char nextChar = currentLine[charPos + 1];
                    //  space, !"&'() * +,-/:;<=>?[\] ^`{|}~
                    // always marks end of token
                    if (" !\"&()*+,-:;<=>[\\]^`{|}~".Any(nextChar.ToString().Contains)) {
                        // end of token text found
                        break;
                    }
                    else if ("'/?".Any(nextChar.ToString().Contains)) {
                        // in fanAGI syntax these also mark end of token
                        break;
                    }
                    else {
                        // add character
                        retval += nextChar;
                        charPos++;
                    }
                }
                if (!textonly) {
                    if (retval[0] == '&') {
                        next.Pointer = true;
                        next.Text = retval[1..];
                        // determine token type
                        GetTokenType(ref next);
                        switch (next.Type) {
                        case ArgType.Num:
                            // invalid token
                            next.Type = ArgType.Unknown;
                            break;
                        case Var:
                        case Flag:
                        case Msg:
                        case SObj:
                        case InvItem:
                        case Str:
                        case Word:
                        case Ctrl:
                            next.Value = next.Value[1..];
                            next.Type = ArgType.Num;
                            break;
                        case DefStr:
                        case VocWrd:
                        case ActionCmd:
                        case TestCmd:
                        case Obj:
                        case ArgType.View:
                        case Symbol:
                        case AssignOperator:
                        case TestOperator:
                        case LineBreak:
                        case BadString:
                        case Keyword:
                        case ArgType.Label:
                        case Comment:
                        case None:
                        case Unknown:
                            break;
                        }
                        next.Text = retval;
                    }
                    else if (retval[0] == '*') {
                        // possible indirect variable
                        next.Indirect = true;
                        next.Text = retval[1..];
                        // determine token type
                        GetTokenType(ref next);
                        // validate
                        switch (next.Type) {
                        case ArgType.Var:
                            // ok
                            break;
                        case Num:
                        case Flag:
                        case Msg:
                        case SObj:
                        case InvItem:
                        case Str:
                        case Word:
                        case Ctrl:
                        case DefStr:
                        case VocWrd:
                        case Obj:
                        case ArgType.View:
                        case BadString:
                            // not allowed, handle error now
                            AddError(4080, false);
                            next.Indirect = false;
                            break;
                        case ActionCmd:
                        case TestCmd:
                        case Keyword:
                        case ArgType.Label:
                        case Unknown:
                        case None:
                            // not allowed, handle error later
                            next.Indirect = false;
                            break;
                        }
                        next.Text = retval;
                    }
                    else {
                        next.Text = retval;
                        // determine token type
                        GetTokenType(ref next);
                    }
                }
                else {
                    next.Text = next.Value = retval;
                    // check for number first
                    if (retval.IsNumeric()) {
                        next.Type = ArgType.Num;
                    }
                }
            }
            // return the token
            next.EndPos = charPos + 1;
            return next;
        }

        private static void GetTokenType(ref CompilerToken token) {
            // determines the token type by checking token.Text:
            // ##, v##, f##, s##, o##, w##, i##, c##, actioncmd, testcme
            // (no m##; those are already found?
            // if not a valid token, search resource IDs, local defines,
            // global defines, and reserved names

            // the '&' character acts as a 'pointer' and returns the numeric
            // value of 'v##, f##, s##, o##, w##, i##, c##'

            // NOTE: this does NOT validate the numerical Value of arguments;
            // calling function is responsible to make that check
            // it also does not concatenate strings

            // check for number first
            if (token.Text.IsNumeric()) {
                token.Type = ArgType.Num;
                token.Value = token.Text;
                return;
            }
            // check for keyword
            if (token.Text == "if" || token.Text == "else" || token.Text == "goto") {
                token.Type = ArgType.Keyword;
                token.Value = token.Text;
                return;
            }

            // check for action command
            int cmdNum = CommandNum(false, token.Text);
            if (cmdNum != 255) {
                token.Type = ArgType.ActionCmd;
                token.Value = cmdNum.ToString();
                return;
            }

            // check for test command
            cmdNum = CommandNum(true, token.Text);
            if (cmdNum != 255) {
                token.Type = ArgType.TestCmd;
                token.Value = cmdNum.ToString();
                return;
            }

            // check for label
            int labelNum = LabelNum(token.Text);
            if (labelNum >= 0) {
                token.Type = ArgType.Label;
                token.Value = labelNum.ToString();
                return;
            }

            // check for argmarkers
            switch (token.Text[0]) {
            case 'v':
                if (int.TryParse(token.Text[1..], out int argval)) {
                    token.Type = ArgType.Var;
                    token.Value = token.Text;
                    return;
                }
                break;
            case 'f':
                if (int.TryParse(token.Text[1..], out argval)) {
                    token.Type = ArgType.Flag;
                    token.Value = token.Text;
                    return;
                }
                break;
            case 'o':
                if (int.TryParse(token.Text[1..], out argval)) {
                    token.Type = ArgType.SObj;
                    token.Value = token.Text;
                    return;
                }
                break;
            case 'c':
                if (int.TryParse(token.Text[1..], out argval)) {
                    token.Type = ArgType.Ctrl;
                    token.Value = token.Text;
                    return;
                }
                break;
            case 's':
                if (int.TryParse(token.Text[1..], out argval)) {
                    token.Type = ArgType.Str;
                    token.Value = token.Text;
                    return;
                }
                break;
            case 'w':
                if (int.TryParse(token.Text[1..], out argval)) {
                    token.Type = ArgType.Word;
                    token.Value = token.Text;
                    return;
                }
                break;
            case 'm':
                if (int.TryParse(token.Text[1..], out argval)) {
                    token.Type = ArgType.Msg;
                    token.Value = token.Text;
                    return;
                }
                break;
            case 'i':
                if (int.TryParse(token.Text[1..], out argval)) {
                    token.Type = ArgType.InvItem;
                    token.Value = token.Text;
                    return;
                }
                break;
            }

            // check local defines
            for (int i = 0; i < definesList.Count; i++) {
                if (token.Text == definesList[i].Name) {
                    token.Value = definesList[i].Value;
                    token.Type = definesList[i].Type;
                    token.Source = CompilerTokenSource.LocalDefine;
                    return;
                }
            }

            // check global defines, ResIDs and reserved names only if enabled
            if (fcompGame.agIncludeGlobals) {
                for (int i = 0; i < fcompGame.GlobalDefines.Count; i++) {
                    if (token.Text == fcompGame.GlobalDefines[i].Name) {
                        token.Value = fcompGame.GlobalDefines[i].Value;
                        token.Type = fcompGame.GlobalDefines[i].Type;
                        token.Source = CompilerTokenSource.GlobalDefine;
                        return;
                    }
                }
            }
            // check numbers against list of resource IDs
            if (fcompGame.agIncludeIDs) {
                for (int i = 0; i < 256; i++) {
                    if (token.Text == logIDs[i]) {
                        token.Value = i.ToString();
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.LogicID;
                        return;
                    }
                    if (token.Text == picIDs[i]) {
                        token.Value = i.ToString();
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.PictureID;
                        return;
                    }
                    if (token.Text == sndIDs[i]) {
                        token.Value = i.ToString();
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.SoundID;
                        return;
                    }
                    if (token.Text == viewIDs[i]) {
                        token.Value = i.ToString();
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.ViewID;
                        return;
                    }
                }
            }
            // lastly, check reserved names if they are being used
            if (fcompGame.agIncludeReserved) {
                for (int i = 0; i <= 4; i++) {
                    if (token.Text == fcompGame.agReservedDefines.EdgeCodes[i].Name) {
                        token.Value = fcompGame.agReservedDefines.EdgeCodes[i].Value;
                        token.Value = i.ToString();
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                for (int i = 0; i <= 8; i++) {
                    if (token.Text == fcompGame.agReservedDefines.ObjDirections[i].Name) {
                        token.Value = fcompGame.agReservedDefines.ObjDirections[i].Value;
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                for (int i = 0; i <= 4; i++) {
                    if (token.Text == fcompGame.agReservedDefines.VideoModes[i].Name) {
                        token.Value = fcompGame.agReservedDefines.VideoModes[i].Value;
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                for (int i = 0; i <= 8; i++) {
                    if (token.Text == fcompGame.agReservedDefines.ComputerTypes[i].Name) {
                        token.Value = fcompGame.agReservedDefines.ComputerTypes[i].Value;
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                for (int i = 0; i <= 15; i++) {
                    if (token.Text == fcompGame.agReservedDefines.ColorNames[i].Name) {
                        token.Value = fcompGame.agReservedDefines.ColorNames[i].Value;
                        token.Type = ArgType.Num;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                // check against invobj Count
                if (token.Text == fcompGame.agReservedDefines.GameInfo[3].Name) {
                    token.Value = fcompGame.agReservedDefines.GameInfo[3].Value;
                    token.Type = ArgType.Num;
                    token.Source = CompilerTokenSource.ReservedDefine;
                    return;
                }
                for (int i = 0; i <= 26; i++) {
                    if (token.Text == fcompGame.agReservedDefines.ReservedVariables[i].Name) {
                        token.Value = fcompGame.agReservedDefines.ReservedVariables[i].Value;
                        token.Type = ArgType.Var;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                for (int i = 0; i <= 17; i++) {
                    if (token.Text == fcompGame.agReservedDefines.ReservedFlags[i].Name) {
                        token.Value = fcompGame.agReservedDefines.ReservedFlags[i].Value;
                        token.Type = ArgType.Flag;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                for (int i = 0; i <= 2; i++) {
                    if (token.Text == fcompGame.agReservedDefines.GameInfo[i].Name) {
                        token.Value = fcompGame.agReservedDefines.GameInfo[i].Value;
                        token.Type = ArgType.DefStr;
                        token.Source = CompilerTokenSource.ReservedDefine;
                        return;
                    }
                }
                if (token.Text == fcompGame.agReservedDefines.ReservedObjects[0].Name) {
                    token.Value = fcompGame.agReservedDefines.ReservedObjects[0].Value;
                    token.Type = ArgType.SObj;
                    token.Source = CompilerTokenSource.ReservedDefine;
                    return;
                }
                if (token.Text == fcompGame.agReservedDefines.ReservedStrings[0].Name) {
                    token.Value = fcompGame.agReservedDefines.ReservedObjects[0].Value;
                    token.Type = ArgType.Str;
                    token.Source = CompilerTokenSource.ReservedDefine;
                    return;
                }
            }
            if (true) {
            }
            // token is not valid
            return;
        }

        /// <summary>
        /// Checks for an end-of-line marker that matches the current syntax.
        /// </summary>
        private static void CheckForEOL() {
            // FAN syntax requires an eol mark
            int newLine, oldLine;

            // cache error line, in case error drops down one or more lines
            oldLine = errorLine;
            if (!CheckChar(';')) {
                // temporarily set to line where error really is
                newLine = errorLine;
                errorLine = oldLine;
                AddError(4005, false);
                // restore errline
                errorLine = newLine;
            }
            else {
                // clear skipError when starting a new line
                skipError = false;
            }
        }

        /// <summary>
        /// This method concatenates strings, i.e. text surrounded by quotes.
        /// It assumes the input string has just been read into the compiler
        /// and checks if there are additional elements of this string to add
        /// to it.
        /// </summary>
        /// <param name="strText"></param>
        /// <returns>The complete string, with position and line values updated
        /// accordingly. If there is nothing to concatenate, the original
        /// string is returned.</returns>
        private static CompilerToken ConcatArg(CompilerToken token) {
            // save current position info
            int lastPos = charPos;
            int lastLineNum = lineNumber;
            int lastErrLine = errorLine;
            string lastLineText = currentLine;

            char nextchar = NextChar();
            if (nextchar == '"') {
                // build the new string
                bool blnInQuotes = true, blnSlash = false;
                
                string retval = "";
                do {
                    // if past end of line (which could only happen if a line contains
                    // a single double quote on it)
                    if (charPos + 1 >= currentLine.Length) {
                        // bad string
                        AddError(4052, false);
                        token.Type = BadString;
                        return token;
                    }
                    // add characters until another TRUE quote is found
                    do {
                        // increment position
                        charPos++;
                        if (charPos == currentLine.Length) {
                            // if still in quotes,
                            if (blnInQuotes) {
                                // set inquotes to false to exit the loop the
                                // compiler will deal with missing quote later
                                token.Type = BadString;
                                return token;
                            }
                            break;
                        }
                        nextchar = currentLine[charPos];
                        // if last char was a slash, next char is just added as-is
                        if (blnSlash) {
                            // always reset  the slash
                            blnSlash = false;
                        }
                        else {
                            // check for slash or quote mark
                            if (nextchar == '"') {
                                // a quote marks end of string
                                blnInQuotes = false;
                            }
                            else if (nextchar == '\\') {
                                blnSlash = true;
                            }
                        }
                        retval += nextchar;
                    } while (blnInQuotes);
                    // update cached error line and current line
                    lastPos = charPos;
                    lastLineNum = lineNumber;
                    lastErrLine = errorLine;
                    lastLineText = currentLine;

                    // look for another string
                    nextchar = NextChar();
                    if (nextchar == '"') {
                        // remove trailing quote
                        retval = retval[..^1];
                        blnInQuotes = true;
                    }
                } while (blnInQuotes);
                token.Text = token.Text[..^1] + retval;
                token.Value = token.Text;
            }
            // restore line/char values to end of the last string added
            charPos = lastPos;
            lineNumber = lastLineNum;
            errorLine = lineNumber - includeOffset;
            currentLine = lastLineText;
            return token;
        }

        /// <summary>
        /// Strips comments from the input text and trims off leading
        /// and trailing spaces.
        /// </summary>
        /// <returns>true if all comments removed without error, otherwise
        /// false.</returns>
        private static bool RemoveComments() {
            // fanAGI syntax:
            //      // - rest of line is ignored (deprecated!)
            //      [ - rest of line is ignored
            int pos;
            bool inQuotes = false, slash = false;
            int ignoreROL;

            ResetCompiler();
            do {
                ignoreROL = -1;
                pos = 0;
                inQuotes = false;
                if (currentLine.Length != 0) {
                    while (pos < currentLine.Length) {
                        if (!inQuotes) {
                            // check for comment characters at this position
                            if (currentLine[pos] == CMT1_TOKEN[0] || currentLine.Mid(pos, 2) == CMT2_TOKEN) {
                                ignoreROL = pos;
                                break;
                            }
                            // slash codes never occur outside quotes
                            slash = false;
                            // if this character is a quote mark, it starts a string
                            inQuotes = currentLine[pos] == QUOTECHAR;
                        }
                        else {
                            // if last character was a slash, ignore this character
                            // because it's part of a slash code
                            if (slash) {
                                // always reset  the slash
                                slash = false;
                            }
                            else {
                                switch (currentLine[pos]) {
                                case QUOTECHAR:
                                    inQuotes = false;
                                    break;
                                case '\\':
                                    slash = true;
                                    break;
                                }
                            }
                        }
                        pos++;
                    }
                    if (ignoreROL >= 0) {
                        currentLine = currentLine[..ignoreROL];
                    }
                }
                currentLine = currentLine.Trim();
                // update the line with comments removed
                ReplaceLine(lineNumber, currentLine);
                IncrementLine();
            } while (lineNumber != -1);
            return true;
        }

        /// <summary>
        /// Replaces the specified line in the input stream with new text, while
        /// preserving include header information.
        /// </summary>
        /// <param name="lineNum"></param>
        /// <param name="newLineText"></param>
        private static void ReplaceLine(int lineNum, string newLineText) {
            string includemarker;

            if (inputLines[lineNum].Left(2) == INCLUDE_MARK) {
                includemarker = inputLines[lineNum][..(inputLines[lineNum].IndexOf('#') + 1)];
            }
            else {
                includemarker = "";
            }
            // replace the line
            inputLines[lineNum] = includemarker + newLineText;
        }

        /// <summary>
        /// Resets the compiler so it points to beginning of input stream.
        /// </summary>
        private static void ResetCompiler() {
            includeOffset = 0;
            // set line pointer to -2 so first call to IncrementLine gets first line
            lineNumber = -2;
            IncrementLine();
        }

        /// <summary>
        /// This method scans the entire input stream and extracts all define 
        /// statements.
        /// </summary>
        /// <returns>true if defines are extracted without error, otherwise false.</returns>
        private static bool ReadDefines() {
            // FAN syntax:
            //   #define
            //   #message
            //   #include (already handled)
            //
            int i;
            TDefine tdNewDefine = new() { Name = "", Value = "" };
            DefineNameCheck checkName;
            DefineValueCheck checkValue;
            int errorNum;
            string errorText = "";
            DefineType defineType;

            ResetCompiler();
            definesList = [];
            do {
                string strToken = NextToken(true, true).Text;
                // check for preprocessor mark '#'
                if (strToken.Length > 0 && (strToken[0] == '#' || strToken[0] == '%')) {
                    strToken = strToken[1..];
                    // in fanAGI syntax, only 'define', 'message' allowed
                    switch (strToken) {
                    case "define":
                        // check for '%include
                        if (currentLine[charPos - 6] == '%') {
                            AddError(4036, LoadResString(4036).Replace(ARG1, "%include"), false);
                        }
                        defineType = DefineType.Default;
                        break;
                    case "message":
                        // skip; these are handled in ReadMsgs, afer defines
                        defineType = DefineType.Ignore;
                        break;
                    default:
                        // invalid preprocessor token; ignore it; main
                        // compiler will handle it
                        defineType = DefineType.Ignore;
                        break;
                    }
                    if (defineType == DefineType.Default) {
                        // #define statement found
                        tdNewDefine.Name = NextToken(true, true).Text;
                        tdNewDefine.Line = errorLine;
                        // get value
                        tdNewDefine.Value = NextToken(true, true).Text;
                        // nothing else allowed on the line
                        // (comments have already been removed)
                        if (charPos != currentLine.Length - 1) {
                            // extra stuff on the line
                            string extra = currentLine[(charPos + 1)..].Trim();
                            if (extra == ";") {
                                AddError(4053, false);
                            }
                            else {
                                AddError(4054, false);
                            }
                            // ignore rest of line
                            charPos = currentLine.Length;
                        }

                        // validate define name
                        checkName = ValidateDefineName(tdNewDefine, fcompGame);
                        // override name errors (8-13) are warnings if errorlevel is medium
                        switch (ErrorLevel) {
                        case Medium:
                            if (checkName == DefineNameCheck.Global) {
                                AddWarning(5034, LoadResString(5034).Replace(ARG1, tdNewDefine.Name));
                                checkName = DefineNameCheck.OK;
                            }
                            else if (checkName > DefineNameCheck.Global) {
                                AddWarning(5035, LoadResString(5035).Replace(ARG1, tdNewDefine.Name));
                                checkName = DefineNameCheck.OK;
                            }
                            break;
                        case Low:
                            if (checkName > BadChar) {
                                checkName = DefineNameCheck.OK;
                            }
                            break;
                        }
                        // now check for errors
                        if (checkName != DefineNameCheck.OK) {
                            errorNum = 0;
                            switch (checkName) {
                            case DefineNameCheck.Empty:
                                errorNum = 4042;
                                errorText = LoadResString(4042);
                                break;
                            case Numeric:
                                errorNum = 4044;
                                errorText = LoadResString(4044);
                                break;
                            case ActionCommand:
                                // not allowed for agiFan syntax
                                errorNum = 4013;
                                errorText = LoadResString(4013).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case TestCommand:
                                // not allowed for fanAGI syntax
                                errorNum = 4014;
                                errorText = LoadResString(4014).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case KeyWord:
                                errorNum = 4011;
                                errorText = LoadResString(4011).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ArgMarker:
                                errorNum = 4043;
                                errorText = LoadResString(4043);
                                break;
                            case BadChar:
                                errorNum = 4040;
                                errorText = LoadResString(4040);
                                break;
                            }
                            if (errorNum > 0) {
                                AddError(errorNum, errorText, false);
                            }
                        }
                        // validate define value
                        errorNum = 0;
                        // value validation sets type
                        checkValue = ValidateDefineValue(ref tdNewDefine, fcompGame);
                        // value errors 5-6 are warnings if errorlevel is medium
                        switch (ErrorLevel) {
                        case Medium:
                            switch (checkValue) {
                            case Reserved:
                                AddWarning(5032, LoadResString(5032).Replace(ARG1, tdNewDefine.Value));
                                checkValue = DefineValueCheck.OK;
                                break;
                            case DefineValueCheck.Global:
                                AddWarning(5031, LoadResString(5031).Replace(
                                    ARG1, tdNewDefine.Value));
                                checkValue = DefineValueCheck.OK;
                                break;
                            }
                            break;
                        case Low:
                            switch (checkValue) {
                            case DefineValueCheck.Global or Reserved:
                                // redefining reserved or global - OK
                                checkValue = DefineValueCheck.OK;
                                break;
                            }
                            break;
                        }
                        // check for remaining errors
                        if (checkValue != DefineValueCheck.OK) {
                            switch (checkValue) {
                            case DefineValueCheck.Empty:
                                // error will be posted after all defines are loaded
                                break;
                            case OutofBounds:
                                errorNum = 4022;
                                errorText = LoadResString(4022);
                                // use a placeholder value
                                if (tdNewDefine.Type == ArgType.Num) {
                                    if (tdNewDefine.Value[0] == '-') {
                                        tdNewDefine.Value = "-128";
                                    }
                                    else {
                                        tdNewDefine.Value = "255";
                                    }
                                }
                                else {
                                    tdNewDefine.Value = tdNewDefine.Value[0] + "255";
                                }
                                break;
                            case BadArgNumber:
                                // argument value not valid for controller, string, word
                                switch (tdNewDefine.Type) {
                                case Ctrl:
                                    if (ErrorLevel == Medium) {
                                        AddWarning(5060);
                                    }
                                    break;
                                case Str:
                                    switch (fcompGame.agIntVersion) {
                                    case "2.089" or "2.272" or "3.002149":
                                        AddWarning(5007, LoadResString(5007).Replace(ARG1, "11"));
                                        break;
                                    default:
                                        AddWarning(5007, LoadResString(5007).Replace(ARG1, "23"));
                                        break;
                                    }
                                    break;
                                case Word:
                                    AddWarning(5008);
                                    break;
                                }
                                break;
                            case NotAValue:
                                // wait 'til all are read,
                                // then check for redefines...
                                break;
                            }
                        }
                        if (errorNum != 0) {
                            AddError(errorNum, errorText, false);
                        }
                        // check all previous defines
                        for (i = 0; i < definesList.Count; i++) {
                            if (tdNewDefine.Name == definesList[i].Name) {
                                AddError(4010, LoadResString(4010).Replace(ARG1, definesList[i].Name), false);
                                tdNewDefine.NameCheck = DefineNameCheck.Empty;
                                break;
                            }
                            if (tdNewDefine.Value == definesList[i].Value) {
                                // numeric duplicates are OK
                                if (!int.TryParse(tdNewDefine.Value, out _)) {
                                    if (ErrorLevel == Medium) {
                                        AddWarning(5033, LoadResString(5033).Replace(ARG1, definesList[i].Value).Replace(ARG2, definesList[i].Name));
                                    }
                                }
                            }
                        }
                        if (tdNewDefine.NameCheck != DefineNameCheck.Empty) {
                            // check against labels
                            for (i = 0; i < labelList.Count; i++) {
                                if (tdNewDefine.Name == labelList[i].Name) {
                                    AddError(4012, LoadResString(4012).Replace(ARG1, tdNewDefine.Name), false);
                                    tdNewDefine.NameCheck = DefineNameCheck.Empty;
                                    break;
                                }
                            }
                        }
                        if (tdNewDefine.NameCheck != DefineNameCheck.Empty) {
                            // save this define
                            definesList.Add(tdNewDefine);
                        }
                        // now set this line to empty so Compiler doesn"t try to read it
                        ReplaceLine(lineNumber, "");
                    }
                }
                IncrementLine();
            } while (lineNumber != -1);

            CheckRedefines();
            return true;
        }

        private static void CheckRedefines() {
            int i;
            // check for redefines of locals, globals, resIDs, reserved,
            // for example, 
            // #define avar  v99
            // #define bvar  avar  --> same as #define bvar  v99

            // also check for circular defines
            // #define cvar  dvar
            // #define dvar  cvar --> error

            // nested defines as well
            // #define avar  v99
            // #define bvar  avar  --> same as #define bvar  v99
            // #define cvar  bvar  --> same as #define cvar  v99

            for (i = 0; i < definesList.Count; i++) {
                bool next = false;
                if (definesList[i].Type == Unknown) {
                    // check other locals
                    for (int j = 0; j < definesList.Count; j++) {
                        if (i != j && definesList[i].Value == definesList[j].Name) {
                            // check for circular
                            if (definesList[i].Name == definesList[j].Value) {
                                // error
                                AddError(4090, LoadResString(4090).Replace(
                                    ARG1, definesList[i].Name).Replace(
                                    ARG2, definesList[j].Name), false, definesList[i].Line);
                                // eliminate both by making their names blank
                                TDefine bad = definesList[i];
                                bad.Name = "";
                                bad.Type = ArgType.Num;
                                definesList[i] = definesList[j] = bad;
                                next = true;
                                break;
                            }
                            // copy value and type
                            TDefine tmp = definesList[i];
                            tmp.Value = definesList[j].Value;
                            tmp.Type = definesList[j].Type;
                            definesList[i] = tmp;
                            if (tmp.Type != Unknown) {
                                AddWarning(5028, LoadResString(5028).Replace(
                                    ARG1, tmp.Name).Replace(
                                    ARG2, "local").Replace(
                                    ARG3, definesList[j].Name), definesList[i].Line);
                            }
                            else {
                                // if new value is also unknown, check again
                                i--;
                            }
                            next = true;
                            break;
                        }
                    }
                    if (next) {
                        continue;
                    }
                    // globals
                    if (fcompGame.agIncludeGlobals) {
                        for (int j = 0; j < fcompGame.GlobalDefines.Count; j++) {
                            if (definesList[i].Value == fcompGame.GlobalDefines[j].Name) {
                                // copy value and type
                                TDefine tmp = definesList[i];
                                tmp.Value = fcompGame.GlobalDefines[j].Value;
                                tmp.Type = fcompGame.GlobalDefines[j].Type;
                                definesList[i] = tmp;
                                AddWarning(5028, LoadResString(5028).Replace(
                                    ARG1, tmp.Name).Replace(
                                    ARG2, "global").Replace(
                                    ARG3, fcompGame.GlobalDefines[j].Name), definesList[i].Line);
                                next = true;
                                break;
                            }
                        }
                    }
                    if (next) {
                        continue;
                    }
                    // resIDs
                    if (fcompGame.agIncludeIDs) {
                        foreach (Logic lgc in fcompGame.Logics) {
                            if (definesList[i].Value == lgc.ID) {
                                // copy value and type
                                TDefine tmp = definesList[i];
                                tmp.Value = lgc.Number.ToString();
                                tmp.Type = Num;
                                definesList[i] = tmp;
                                AddWarning(5028, LoadResString(5028).Replace(
                                    ARG1, tmp.Name).Replace(
                                    ARG2, "logicID").Replace(
                                    ARG3, lgc.ID), definesList[i].Line);
                                next = true;
                                break;
                            }
                        }
                        if (next) {
                            continue;
                        }
                        foreach (Picture pic in fcompGame.Pictures) {
                            if (definesList[i].Value == pic.ID) {
                                // copy value and type
                                TDefine tmp = definesList[i];
                                tmp.Value = pic.Number.ToString();
                                tmp.Type = Num;
                                definesList[i] = tmp;
                                AddWarning(5028, LoadResString(5028).Replace(
                                    ARG1, tmp.Name).Replace(
                                    ARG2, "pictureID").Replace(
                                    ARG3, pic.ID), definesList[i].Line);
                                next = true;
                                break;
                            }
                        }
                        if (next) {
                            continue;
                        }
                        foreach (Sound sound in fcompGame.Sounds) {
                            if (definesList[i].Value == sound.ID) {
                                // copy value and type
                                TDefine tmp = definesList[i];
                                tmp.Value = sound.Number.ToString();
                                definesList[i] = tmp;
                                tmp.Type = Num;
                                AddWarning(5028, LoadResString(5028).Replace(
                                    ARG1, tmp.Name).Replace(
                                    ARG2, "soundID").Replace(
                                    ARG3, sound.ID), definesList[i].Line);
                                next = true;
                                break;
                            }
                        }
                        if (next) {
                            continue;
                        }
                        foreach (Engine.View view in fcompGame.Views) {
                            if (definesList[i].Value == view.ID) {
                                // copy value and type
                                TDefine tmp = definesList[i];
                                tmp.Value = view.Number.ToString();
                                tmp.Type = Num;
                                definesList[i] = tmp;
                                AddWarning(5028, LoadResString(5028).Replace(
                                    ARG1, tmp.Name).Replace(
                                    ARG2, "viewID").Replace(
                                    ARG3, view.ID), definesList[i].Line);
                                next = true;
                                break;
                            }
                        }
                        if (next) {
                            continue;
                        }
                    }
                    // reserved
                    if (fcompGame.agIncludeReserved) {
                        TDefine[] allreserved = fcompGame.ReservedDefines.All();
                        for (int j = 0; j < allreserved.Length; j++) {
                            if (definesList[i].Value == allreserved[j].Name) {
                                // copy value and type
                                TDefine tmp = definesList[i];
                                tmp.Value = allreserved[j].Value;
                                tmp.Type = allreserved[j].Type;
                                definesList[i] = tmp;
                                AddWarning(5028, LoadResString(5028).Replace(
                                    ARG1, tmp.Name).Replace(
                                    ARG2, "reserved").Replace(
                                    ARG3, allreserved[j].Name), definesList[i].Line);
                                next = true;
                                break;
                            }
                        }
                    }
                }
                if (definesList[i].Type == Unknown) {
                    if (definesList[i].Value == "") {
                        AddError(4045, false, definesList[i].Line);
                    }
                    else {
                        AddError(4055, LoadResString(4055).Replace(ARG1, definesList[i].Value), false, definesList[i].Line);
                    }
                }
            }
        }

        /// <summary>
        /// Extracts defined messages from the source code data stream.
        /// </summary>
        /// <returns>true if messages extracted without errors, otherwise false.</returns>
        private static bool ReadMsgs() {
            // note that stripped message lines also strip out the include header string
            // this doesn't matter since they are only blank lines anyway only need
            // to include header info if error occurs, and errors never occur on
            // blank line
            int msgNum, msgStart;

            // reset message list
            for (msgNum = 0; msgNum < 256; msgNum++) {
                MsgText[msgNum] = "";
                MsgInUse[msgNum] = false;
                MsgWarnings[msgNum] = 0;
            }
            ResetCompiler();
            do {
                CompilerToken token = NextToken(true, true);
                if (token.Text == MSG_TOKEN || token.Text == "%message") {
                    if (token.Text == "%message") {
                        // fanAGI syntax doesn't allow %message
                        AddError(4036, LoadResString(4036).Replace(ARG1, "%message"), false);
                    }
                    // save starting line number (in case this msg uses multiple lines)
                    msgStart = lineNumber;
                    // get next token as message number on this line
                    token = NextToken(true, true);
                    // this should be a msg number
                    if (token.Type != ArgType.Num || token.NumValue <= 0 || token.NumValue > 255) {
                        // ignore if no valid number
                        AddError(4048, false);
                    }
                    else {
                        msgNum = token.NumValue;
                        // msg is already assigned
                        if (MsgInUse[msgNum]) {
                            AddError(4072, LoadResString(4072).Replace(ARG1, (msgNum).ToString()), false);
                        }
                    }
                    // next token should be the message text
                    token = NextToken(true);
                    if (token.Source == CompilerTokenSource.Code) {
                        // if a valid string, check concatenation
                        if (token.Type == DefStr) {
                            token = ConcatArg(token);
                            if (token.Type == ArgType.BadString) {
                                // use a placeholder
                                token.Text = "\" \"";
                                // skip to end of line
                                charPos = currentLine.Length - 1;
                                // error already added so reset type
                                token.Type = DefStr;
                            }
                        }
                    }
                    if (token.Type != ArgType.DefStr) {
                        if (token.Type == ArgType.BadString) {
                            // not a good string
                            AddError(4050, false);
                        }
                        else {
                            // not a string
                            AddError(4051, false);
                        }
                        // use a placeholder
                        token.Text = "\" \"";
                        // skip rest of line
                        charPos = currentLine.Length - 1;
                    }
                    // nothing allowed after msg declaration
                    string msgText = token.Value;
                    token = NextToken(true);
                    if (token.Type != None) {
                        // extra stuff on the line
                        if (token.Text == ";") {
                            AddError(4053, false);
                        }
                        else {
                            AddError(4054, false);
                        }
                        // ignore rest of line
                        charPos = currentLine.Length - 1;
                    }
                    // strip off quotes
                    msgText = msgText[1..^1];
                    // replace slash codes with single characters
                    MsgText[msgNum] = ConvertSlashCodes(msgText);
                    ValidateMsgChars(msgText, msgNum);
                    MsgInUse[msgNum] = true;
                    // edge case! if last line is concatenated, lineNumber will
                    // be reset to -1
                    if (lineNumber == -1) {
                        lineNumber = inputLines.Count - 1;
                    }
                    do {
                        // set the msg line (and any concatenated lines) to empty so
                        // compiler doesn't try to read it
                        ReplaceLine(msgStart, "");
                        msgStart++;
                        // continue until back to current line
                    } while (msgStart <= lineNumber);
                }
                IncrementLine();
            } while (lineNumber != -1);
            // done
            return true;
        }

        private static string ConvertSlashCodes(string messagetext) {
            int pos = 0;
            string retval = "";
            bool skipchar = false;

            while (pos < messagetext.Length) {
                char charval = messagetext[pos];
                switch (charval) {
                case '\\':
                    // '\'
                    // check next character for special codes
                    if (pos < messagetext.Length - 1) {
                        switch (messagetext[pos + 1]) {
                        case 'n':
                            //  '\n' = new line
                            charval = '\n';
                            pos++;
                            break;
                        case '\"':
                            // '\"' = quote mark
                            charval = '\"';
                            pos++;
                            break;
                        case '\\':
                            // '\\' = \
                            charval = '\\';
                            pos++;
                            break;
                        case 'x':
                            // '\x' - look for a hex value
                            // (make sure at least two more characters)
                            if (pos < messagetext.Length - 3) {
                                // get next 2 chars and hexify them
                                string strHex = "0x" + messagetext[pos + 2] + messagetext[pos + 3];
                                // if this hex value >=1 and <256, use it
                                try {
                                    int i = Convert.ToInt32(strHex, 16);
                                    if (i >= 1) {
                                        byte[] charbyte = [(byte)i];
                                        charval = Encoding.GetEncoding(fcompGame.CodePage).GetString(charbyte)[0];
                                        pos += 3;
                                    }
                                }
                                catch (Exception) {
                                    // drop the slash if there's an error
                                    skipchar = true;
                                }
                            }
                            break;
                        default:
                            // if no special char found, the single slash
                            // should be dropped
                            skipchar = true;
                            break;
                        }
                    }
                    else {
                        // '\' is the last char, skip it
                        skipchar = true;
                    }
                    break;
                }
                if (!skipchar) {
                    retval += charval;
                }
                pos++;
                skipchar = false;
            }
            return retval;
        }

        /// <summary>
        /// Sends the specified warning to the calling program as an event.
        /// </summary>
        /// <param name="WarningNum"></param>
        /// <param name="WarningText"></param>
        private static void AddWarning(int WarningNum, string WarningText = "", int line = -1) {
            // (number, line and module only have meaning for logic warnings
            // other warnings generated during a game compile will use
            // same format, but use -- for warning number, line and module)

            if (line == -1) {
                line = errorLine;
            }
            // if no text passed, use the default resource string
            if (WarningText.Length == 0) {
                WarningText = LoadResString(WarningNum);
            }
            // only add if not ignoring
            if (!noCompWarn[WarningNum - 5000]) {
                TWinAGIEventInfo errInfo = new() {
                    Type = EventType.LogicCompileWarning,
                    ID = WarningNum.ToString(),
                    Text = WarningText,
                    Line = line.ToString(),
                    Module = errorModule.Length > 0 ? Path.GetFileName(errorModule) : compLogic.ID,
                    Filename = errorModule,
                    ResNum = complogicNumber,
                    ResType = AGIResType.Logic,
                };
                _ = OnCompileLogicStatus(fcompGame, errInfo);
            }
        }

        /// <summary>
        /// This method reads and validates the input for an 'if' command.
        /// </summary>
        /// <returns></returns>
        private static bool CompileIf() {
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
            // valid special comparison expressions are ==, !=, >, >=, <, <=
            //
            // OR'ed tests must always be enclosed in parenthesis; AND'ed tests
            // must never be enclosed in parentheses (this ensures the compiled code
            // will be compatible with the AGI interpreter)
            //
            // any test token may have the negation operator (!) placed directly
            // in front of it; special syntax 'fn' or 'vn' may also have the
            // negation operator, but 'vn expr m' may not
            //
            // proprerly formatted special IF syntax will be one of following:
            //    v## expr v##
            //    v## expr ##
            //    v##
            //    f##
            //
            // where ## is a number from 0-255
            // and expr is "==", "!=", "<", "<=", "=<", ">", ">=", "=>"
            byte[] argvalarray = new byte[8];
            bool inOrBlock = false;
            bool needNextCmd = true;
            int numTestCmds = 0, numCmdsInBlock = 0;
            int arg2 = 0;
            bool isNOT = false, varTest = false;

            // 'if' starting bytecode
            tmpLogRes.WriteByte(0xFF);

            // next character should be "("
            if (!CheckChar('(')) {
                AddError(4001, false);
                // keep going - maybe the missing '(' is the only problem
            }

            // step through input, until final ')' is found:
            do {
                CompilerToken testToken = NextToken();
                if (lineNumber == -1) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    skipError = false;
                    return false;
                }
                if (needNextCmd) {
                    // valid tokens
                    switch (testToken.Type) {
                    case Symbol:
                        // symbol should be '(', ')' or '!'
                        switch (testToken.Text) {
                        case "(":
                            if (isNOT) {
                                // can't NOT a block
                                AddError(4067, false);
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
                                    if (ErrorLevel == Medium) {
                                        AddWarning(5047);
                                    }
                                }
                                else {
                                    AddError(4033, false);
                                    // done with if
                                    tmpLogRes.WriteByte(0xFF);
                                    skipError = false;
                                    return true;
                                }
                                // close the block
                                inOrBlock = false;
                                needNextCmd = false;
                            }
                            else if (numTestCmds == 0) {
                                // if block with no commands
                                if (ErrorLevel == Medium) {
                                    AddWarning(5016);
                                }
                                // done with if
                                tmpLogRes.WriteByte(0xFF);
                                skipError = false;
                                return true;
                            }
                            else {
                                // unexpected closer
                                AddError(4033, false);
                                // done with if
                                tmpLogRes.WriteByte(0xFF);
                                skipError = false;
                                return true;
                            }
                            break;
                        case NOT_TOKEN:
                            if (isNOT) {
                                // can't NOT a NOT...
                                AddError(4065, false);
                                break;
                            }
                            isNOT = true;
                            tmpLogRes.WriteByte(0xFD);
                            // check for end of input
                            if (lineNumber == -1) {
                                AddError(4057, true);
                                skipError = false;
                                return false;
                            }
                            break;
                        default:
                            // error, unexpected symbol
                            AddError(4021, LoadResString(4021).Replace(ARG1, testToken.Text), false);
                            skipError = true;
                            break;
                        }
                        break;
                    case TestCmd:
                        CompilerToken argToken;
                        byte cmdnum = (byte)testToken.NumValue;
                        tmpLogRes.WriteByte(cmdnum);
                        // next command should be "("
                        if (!CheckChar('(')) {
                            AddError(4027, false);
                        }
                        // check for return.false() token
                        if (testToken.NumValue == 0) {
                            // warn user that it's not compatible with AGI Studio
                            if (ErrorLevel == Medium) {
                                AddWarning(5081);
                            }
                        }
                        if (cmdnum == 0x0E) {
                            // process a said command
                            int lngArg, intWordCount = 0;
                            int[] lngWord = [];
                            do {
                                // get next argument - must be a number  or string
                                argToken = NextToken();
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                if (argToken.Type == Num) {
                                    lngArg = argToken.NumValue;
                                    if (lngArg > 65535 || lngArg < 0) {
                                        AddError(4086, LoadResString(4086).Replace(ARG1, argToken.Text), false);
                                        // use a place holder
                                        lngArg = 1;
                                    }
                                    if (ErrorLevel == Medium && !fcompGame.agVocabWords.GroupExists(lngArg)) {
                                        AddWarning(5019, LoadResString(5019).Replace(
                                            ARG1, argToken.Value));
                                    }
                                }
                                else {
                                    switch (argToken.Type) {
                                    case DefStr:
                                        argToken.Value = argToken.Value[1..^1];
                                        lngArg = -1;
                                        break;
                                    default:
                                        // not a word
                                        AddError(4086, LoadResString(4086).Replace(ARG1, argToken.Text), false);
                                        // use a place holder
                                        lngArg = 1;
                                        break;
                                    }
                                }
                                // validate the group
                                if (lngArg == -1) {
                                    if (fcompGame.agVocabWords.WordExists(argToken.Value)) {
                                        lngArg = fcompGame.agVocabWords[argToken.Value].Group;
                                    }
                                    else {
                                        // RARE, but if it's an 'a' or 'i' that isn't defined,
                                        // it's word group 0
                                        if (argToken.Value == "i" || argToken.Value == "a") {
                                            lngArg = 0;
                                            if (ErrorLevel == Medium) {
                                                AddWarning(5108, LoadResString(5108).Replace(ARG1, argToken.Value));
                                            }
                                        }
                                        else {
                                            AddError(4091, LoadResString(4091).Replace(
                                                ARG1, argToken.Text), false);
                                            // use a place holder
                                            lngArg = 1;
                                        }
                                    }
                                }
                                // check for group 0
                                if (lngArg == 0) {
                                    if (ErrorLevel == Medium) {
                                        AddWarning(5083, LoadResString(5083).Replace(ARG1, argToken.Text));
                                    }
                                }
                                // if too many words
                                if (intWordCount == 10) {
                                    // TODO: change to warning if errorlevel is medium
                                    AddWarning(5999);
                                }
                                if (intWordCount < 10) {
                                    // add this word number
                                    // to array of word numbers
                                    Array.Resize(ref lngWord, intWordCount + 1);
                                    lngWord[intWordCount] = lngArg;
                                }
                                intWordCount++;
                                // next character should be a comma, or close parenthesis
                                argToken = NextToken();
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                if (argToken.Text == ")") {
                                    // move pointer back one space so the ')' 
                                    // will be found at end of command
                                    charPos--;
                                    break;
                                }
                                else if (argToken.Text == ",") {
                                    // get another word
                                }
                                else {
                                    // missing comma or closing parenthesis
                                    AddError(4026, LoadResString(4026).Replace(
                                        ARG1, (intWordCount + 1).ToString()).Replace(
                                        ARG2, "said"), false);
                                    // assume comma and continue
                                    charPos -= testToken.Text.Length;
                                    //skipError = false;
                                    return false;
                                }
                            } while (true);
                            // add number of arguments
                            tmpLogRes.WriteByte((byte)intWordCount);
                            // add words
                            for (int i = 0; i < intWordCount; i++) {
                                tmpLogRes.WriteWord((ushort)lngWord[i]);
                            }
                        }
                        else {
                            bool blnArgsOK = true;
                            // extract arguments
                            for (int i = 0; i < TestCommands[cmdnum].ArgType.Length; i++) {
                                // after first argument, verify comma separates arguments
                                if (i > 0) {
                                    if (!CheckChar(',')) {
                                        AddError(4026, LoadResString(4026).Replace(
                                            ARG1, i.ToString()).Replace(
                                            ARG2, TestCommands[cmdnum].Name), false);
                                    }
                                }
                                argToken = NextToken();
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                int argval = argToken.NumValue;
                                // inv items may be a defined string
                                if (TestCommands[cmdnum].ArgType[i] == ArgType.InvItem && argToken.Type == DefStr) {
                                    argval = ValidateInvItem(ref argToken);
                                }
                                else {
                                    ValidateArgType(ref argval, argToken.Type, i);
                                }
                                if (argToken.Type == TestCommands[cmdnum].ArgType[i]) {
                                    argvalarray[i] = (byte)argval;
                                }
                                else {
                                    // if early ',' or ')', give useful error information
                                    if (argToken.Text == "," || argToken.Text == ")") {
                                        AddError(4031, LoadResString(4031).Replace(
                                            ARG1, (i + 1).ToString()).Replace(
                                            ARG2, TestCommands[cmdnum].Name).Replace(
                                            ARG3, ArgTypeName(TestCommands[cmdnum].ArgType[i])), false);
                                        // backup one space so the ',' or ')' will be found at end of command
                                        charPos--;
                                        // if ')', end the loop early
                                        if (argToken.Text == ")") {
                                            break;
                                        }
                                    }
                                    else {
                                        // wrong token
                                        AddError(4037, LoadResString(4037).Replace(
                                            ARG1, (i + 1).ToString()).Replace(
                                            ARG2, ArgTypeName(TestCommands[cmdnum].ArgType[i])).Replace(
                                            ARG3, argToken.Text), false);
                                    }
                                    // use a placeholder
                                    argvalarray[i] = 0;
                                    blnArgsOK = false;
                                }
                                // add argument
                                tmpLogRes.WriteByte(argvalarray[i]);
                            }
                            if (blnArgsOK) {
                                // validate arguments for this command
                                ValidateIfArgs(testToken.NumValue, ref argvalarray);
                            }
                        }
                        // next character should be ")"
                        if (!CheckChar(')')) {
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
                        // can't be indirect
                        if (testToken.Indirect) {
                            if (testToken.Type == Var) {
                                AddError(4079, false);
                            }
                            else {
                                AddError(4080, false);
                            }
                            testToken.Indirect = false;
                        }
                        int arg1 = testToken.NumValue;
                        ValidateArgType(ref arg1, Var, 0);
                        byte cmd;
                        bool addNot = false;
                        // next token should be the expression
                        argToken = NextToken(false, true);
                        if (lineNumber == -1) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            skipError = false;
                            return false;
                        }
                        // can't NOT alternate, unless it's single var
                        if (isNOT) {
                            if (argToken.Text != ")" && argToken.Text != "&&" && argToken.Text != "||") {
                                tmpLogRes.WriteByte(0xFD);
                                isNOT = false;
                            }
                        }
                        switch (argToken.Text) {
                        case EQUAL_TOKEN:
                            cmd = 0x01;
                            break;
                        case NOTEQUAL_TOKEN:
                            cmd = 0x01;
                            addNot = true;
                            break;
                        case ">":
                            cmd = 0x05;
                            break;
                        case "<=" or "=<":
                            cmd = 0x05;
                            addNot = true;
                            break;
                        case "<":
                            cmd = 0x03;
                            break;
                        case ">=" or "=>":
                            cmd = 0x03;
                            addNot = true;
                            break;
                        case ")" or "&&" or "||":
                            // means we are doing a boolean test of the variable;
                            // use greatern with zero as arg
                            cmd = 0x05;
                            arg2 = 0;
                            varTest = true;
                            // restore the token to the stream so main compiler
                            // can handle it
                            charPos -= argToken.Text.Length;
                            break;
                        case "=":
                            // invalid compare
                            AddError(4060, false);
                            // use equal(==) as placeholder
                            cmd = 0x01;
                            break;
                        default:
                            // unknown expression token
                            AddError(4049, false);
                            skipError = true;
                            // use placeholder cmd
                            cmd = 0x01;
                            break;
                        }
                        if (varTest) {
                            varTest = false;
                        }
                        else {
                            // get second argument (numerical or variable)
                            argToken = NextToken();
                            if (lineNumber == -1) {
                                // nothing left, return critical error
                                AddError(4057, true);
                                skipError = false;
                                return false;
                            }
                            if (argToken.Type == ArgType.Var) {
                                // use variable version of command
                                cmd++;
                            }
                            else if (argToken.Type == ArgType.Num) {
                                // OK as-is
                            }
                            else {
                                // invalid arg type
                                //switch (argToken.Type) {
                                //case Symbol:
                                //case AssignOperator:
                                //case TestOperator:
                                //    break;
                                //case DefStr:
                                //case BadString:
                                //    break;
                                //case Str:
                                //case Flag:
                                //case Msg:
                                //case SObj:
                                //case InvItem:
                                //case Word:
                                //case Ctrl:
                                //case ActionCmd:
                                //case TestCmd:
                                //case Keyword:
                                //case ArgType.Label:
                                //    break;
                                //case None:
                                //    break;
                                //case Unknown:
                                //    break;
                                //}
                                AddError(4070, LoadResString(4070).Replace(ARG1, argToken.Text), false);
                                skipError = true;
                                // use placeholder
                                argToken.Value = "255";
                                argToken.Type = Num;
                            }
                            arg2 = argToken.NumValue;
                            ValidateArgType(ref arg2, argToken.Type, 1);
                        }
                        if (addNot) {
                            tmpLogRes.WriteByte(0xFD);
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
                        arg2 = testToken.NumValue;
                        ValidateArgType(ref arg2, Flag, 0);
                        // write isset cmd
                        tmpLogRes.WriteByte(0x07);
                        tmpLogRes.WriteByte((byte)arg2);
                        needNextCmd = false;
                        numTestCmds++;
                        if (inOrBlock) {
                            numCmdsInBlock++;
                        }
                        break;
                    default:
                        // invalid token
                        AddError(4021, LoadResString(4021).Replace(ARG1, testToken.Text), false);
                        skipError = true;
                        numTestCmds++;
                        break;
                    }
                }
                else {
                    // not awaiting a test command
                    switch (testToken.Text) {
                    case NOT_TOKEN:
                        // 'NOT' token is not allowed 
                        AddError(4075, false);
                        break;
                    case AND_TOKEN:
                        if (inOrBlock) {
                            // 'and' not allowed if inside brackets
                            AddError(4019, false);
                        }
                        needNextCmd = true;
                        break;
                    case OR_TOKEN:
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
                            skipError = false;
                            return true;
                        }
                        break;
                    default:
                        AddError(inOrBlock ? 4077 : 4020, false);
                        // assume it was ok
                        needNextCmd = true;
                        // 'and if it is a close bracket, assume
                        // the 'if' block is now closed
                        if (testToken.Text == "{") {
                            charPos--;
                            // write ending if byte
                            tmpLogRes.WriteByte(0xFF);
                            skipError = false;
                            return true;
                        }
                        break;
                    }
                }
                // never leave loop normally; error, end of input, or successful
                // compilation of test commands will all exit loop directly
            } while (true);
        }

        private static int ValidateInvItem(ref CompilerToken itemToken) {
            // itemToken is a literal string, look for matching inventory item
            int retval = -1;
            for (int j = 0; j < fcompGame.InvObjects.Count; j++) {
                if (itemToken.Value[1..^1] == fcompGame.InvObjects[j].ItemName) {
                    itemToken.Type = InvItem;
                    retval = j;
                    break;
                }
            }
            if (retval == -1) {
                AddError(4038, LoadResString(4038).Replace(ARG1, itemToken.Text), false);
                // use placeholder
                itemToken.Type = InvItem;
                return 1;
            }
            else {
                if (fcompGame.InvObjects[retval].ItemName == "?") {
                    if (ErrorLevel == Medium) {
                        AddWarning(5004);
                    }
                }
                // check for  non-unique item
                if (!fcompGame.InvObjects[retval].Unique) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5003, LoadResString(5003).Replace(ARG1, itemToken.Value));
                    }
                }
            }
            return retval;
        }

        /// <summary>
        /// Returns the number of the specified label, or zero if a match
        /// is not found.
        /// </summary>
        /// <param name="LabelName"></param>
        /// <returns></returns>
        private static int LabelNum(string LabelName) {
            for (int i = 0; i < labelList.Count; i++) {
                if (labelList[i].Name == LabelName) {
                    return i;
                }
            }
            return -1;
        }

        private static void ValidateArgType(ref int argval, ArgType type, int argpos) {
            // all - validate < 256
            if (argval > 255) {
                AddError(4039, LoadResString(4039).Replace(ARG1, (argpos + 1).ToString()), false);
                // assume max value
                argval = 255;
                return;
            }

            switch (type) {
            case ArgType.Num:
                // check for negative numbers
                if (argval < -128) {
                    AddError(4073, false);
                    // use a placeholder
                    argval = 128;
                }
                else if (argval < 0) {
                    // convert to unsigned byte value
                    argval = 256 + argval;
                    if (ErrorLevel == Medium) {
                        AddWarning(5098);
                    }
                }
                break;
            case Var:
                // no additional checks
                break;
            case Flag:
                // no additional checks
                break;
            case Msg:
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
                    // message does not exist, insert a null message
                    MsgText[argval] = "";
                    MsgInUse[argval] = true;
                    AddWarning(5090);
                }
                break;
            case SObj:
                // check against max screen object Value
                if (argval > fcompGame.InvObjects.MaxScreenObjects) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5006, LoadResString(5006).Replace(ARG1, fcompGame.InvObjects.MaxScreenObjects.ToString()));
                    }
                }
                break;
            case InvItem:
                // range is 0 - maxinv
                if (argval >= fcompGame.InvObjects.Count) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5005, LoadResString(5005).Replace(ARG1, (argpos + 1).ToString()));
                    }
                }
                else {
                    // if object is a question mark, add a warning
                    if (fcompGame.InvObjects[argval].ItemName == "?") {
                        if (ErrorLevel == Medium) {
                            AddWarning(5004);
                        }
                    }
                }
                break;
            case Str:
                // range is 0 - 23 or 0 - 11
                if (argval > 23 ||
                    (argval > 11 &&
                        (fcompGame.InterpreterVersion == "2.089" ||
                        fcompGame.InterpreterVersion == "2.272" ||
                        fcompGame.InterpreterVersion == "3.002149"))) {
                    if (ErrorLevel == Medium) {
                        // version 2.089, 2.272, and 3.002149 only 12 strings
                        switch (fcompGame.InterpreterVersion) {
                        case "2.089" or "2.272" or "3.002149":
                            AddWarning(5007, LoadResString(5007).Replace(ARG1, "11"));
                            break;
                        default:
                            AddWarning(5007, LoadResString(5007).Replace(ARG1, "23"));
                            break;
                        }
                    }
                }
                break;
            case Word:
                // range is 0 - 9
                // subtract one to account for '1' based word
                // data type (i.e. w1 is first word, but commands
                // use arg value of '0')
                argval--;
                if (argval < 0) {
                    argval = 255;
                }
                if (argval > 9) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5008);
                    }
                }
                break;
            case Ctrl:
                // range is 0 - 49
                if (argval > 49) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5060);
                    }
                }
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
        private static void ValidateArgs(int cmdNum, ref byte[] argval) {
            // for commands that can affect variable values, need to check against
            // reserved variables
            // for commands that can affect flags, need to check against reserved
            // flags
            // for other commands, check the passed arguments to see if values
            // are appropriate
            bool unload = false, warned = false;

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
                        if (ErrorLevel == Medium) {
                            AddWarning(5030);
                        }
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
                if (!fcompGame.agLogs.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5053);
                    }
                }
                if (argval[0] == 0) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5012);
                    }
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
                if (!fcompGame.agLogs.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5013);
                    }
                }
                break;
            case 22:
                // call(A)
                if (argval[0] == 0) {
                    // calling logic0 is a BAD idea
                    if (ErrorLevel == Medium) {
                        AddWarning(5010);
                    }
                }
                if (!fcompGame.agLogs.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5076);
                    }
                }
                if (argval[0] == complogicNumber) {
                    // recursive calling is usually BAD
                    if (ErrorLevel == Medium) {
                        AddWarning(5089);
                    }
                }
                break;
            case 30:
                // load.view(A)
                if (!fcompGame.agViews.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5015);
                    }
                }
                break;
            case 32:
                // discard.view(A)
                if (!fcompGame.agViews.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5024);
                    }
                }
                break;
            case 37:
                // position(oA, X,Y)
                if (argval[1] > 159 || argval[2] > 167) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5023);
                    }
                }
                break;
            case 39:
                // get.posn
                if (argval[1] <= 26 || argval[2] <= 26) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 41:
                // set.view(oA, B)
                if (!fcompGame.agViews.Contains(argval[1])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5037);
                    }
                }
                break;
            case (>= 49 and <= 53) or 97 or 118:
                // last.cel, current.cel, current.loop,
                // current.view, number.of.loops, get.room.v
                // get.num
                if (argval[1] <= 26) {
                    // variable arg is second and should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 54:
                // set.priority(oA, B)
                if (argval[1] < 4 || argval[1] > 15) {
                    // invalid priority Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5050);
                    }
                }
                break;
            case 57:
                // get.priority
                if (argval[1] <= 26) {
                    // variable is second argument and should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 63:
                // set.horizon(A)
                if (ErrorLevel == Medium) {
                    if (argval[0] >= 167) {
                        AddWarning(5043);
                    }
                    else if (argval[0] > 120) {
                        AddWarning(5042);
                    }
                    else if (argval[0] < 16) {
                        AddWarning(5041);
                    }
                }
                break;
            case >= 64 and <= 66:
                // object.on.water, object.on.land, object.on.anything
                if (argval[0] == 0) {
                    // warn if used on ego
                    if (ErrorLevel == Medium) {
                        AddWarning(5082);
                    }
                }
                break;
            case 69:
                // distance(oA, oB, vC)
                if (argval[2] <= 26) {
                    // variable is third arg and should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 73 or 75:
                // end.of.loop, reverse.loop
                if (argval[1] <= 15) {
                    // flag arg should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 81:
                // move.obj(oA, X,Y,STEP,fDONE)
                if (argval[1] > 159 || argval[2] > 167) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5062);
                    }
                }
                if (argval[0] == 0) {
                    // ego object forces program mode
                    if (ErrorLevel == Medium) {
                        AddWarning(5045);
                    }
                }
                if (argval[4] <= 15) {
                    // flag arg should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 82:
                // move.obj.v
                if (argval[0] == 0) {
                    // ego object forces program mode
                    if (ErrorLevel == Medium) {
                        AddWarning(5045);
                    }
                }
                if (argval[4] <= 15) {
                    // flag arg should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 83:
                // follow.ego(oA, DISTANCE, fDONE)
                if (argval[1] <= 1) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5102);
                    }
                }
                if (argval[0] == 0) {
                    // ego can't follow ego
                    if (ErrorLevel == Medium) {
                        AddWarning(5027);
                    }
                }
                if (argval[2] <= 15) {
                    // flag arg should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                CheckResFlagUse(argval[2]);
                break;
            case 86:
                // set.dir(oA, vB)
                if (argval[0] == 0) {
                    // has no effect on ego object
                    if (ErrorLevel == Medium) {
                        AddWarning(5026);
                    }
                }
                break;
            case 87:
                // get.dir (oA, vB)
                if (argval[1] <= 26) {
                    // variable arg should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 90:
                // block(x1,y1,x2,y2)
                if (argval[0] > 159 || argval[1] > 167 || argval[2] > 159 || argval[3] > 167) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5020);
                    }
                }
                if ((argval[2] - argval[0] < 2) || (argval[3] - argval[1] < 2)) {
                    // invalid arguments
                    if (ErrorLevel == Medium) {
                        AddWarning(5051);
                    }
                }
                break;
            case 98:
                // load.sound(A)
                if (!fcompGame.agSnds.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5014);
                    }
                }
                break;
            case 99:
                // sound(A, fB)
                if (!fcompGame.agSnds.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5084);
                    }
                }
                if (argval[1] <= 15) {
                    // flag arg should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                CheckResFlagUse(argval[1]);
                break;
            case 103:
                // display(ROW, COL, mC)
                if (argval[0] > 24 || argval[1] > 39) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5109);
                    }
                }
                break;
            case 105:
                // clear.lines(TOP, BTM, C)
                if (argval[0] > 24 || argval[1] > 24 || argval[0] > argval[1]) {
                    // top must be >btm; both must be <=24
                    if (ErrorLevel == Medium) {
                        AddWarning(5011);
                    }
                }
                if (argval[2] > 0 && argval[2] != 15) {
                    // color value should be 0 or 15 /(but it doesn't
                    // hurt to be anything else)
                    if (ErrorLevel == Medium) {
                        AddWarning(5100);
                    }
                }
                break;
            case 109:
                // set.text.attribute(A,B)
                if (argval[0] > 15 || argval[1] > 15) {
                    // color value should be 0 or 15 /(but it doesn't
                    // hurt to be anything else)
                    if (ErrorLevel == Medium) {
                        AddWarning(5029);
                    }
                }
                break;
            case 110:
                // shake.screen(A)
                if (argval[0] == 0) {
                    // zero is BAD
                    if (ErrorLevel == Medium) {
                        AddWarning(5057);
                    }
                }
                else if (argval[0] > 15) {
                    if (argval[0] >= 100 && argval[0] <= 109) {
                        // could be a palette change
                        if (ErrorLevel == Medium) {
                            AddWarning(5059);
                        }
                    }
                    else {
                        // shouldn't normally have more than a few shakes
                        if (ErrorLevel == Medium) {
                            AddWarning(5058);
                        }
                    }
                }
                break;
            case 111:
                // configure.screen(TOP,INPUT,STATUS)
                if (argval[0] > 3) {
                    // top should be <=3
                    if (ErrorLevel == Medium) {
                        AddWarning(5044);
                    }
                }
                if (argval[1] > 24 || argval[2] > 24) {
                    // input or status are invalid
                    if (ErrorLevel == Medium) {
                        AddWarning(5099);
                    }
                }
                if (argval[1] == argval[2]) {
                    // input and status should not be equal
                    if (ErrorLevel == Medium) {
                        AddWarning(5048);
                    }
                }
                if ((argval[1] >= argval[0] && argval[1] <= argval[0] + 20) ||
                    (argval[2] >= argval[0] && argval[2] <= argval[0] + 20)) {
                    // input and status should be <top or >=top+21
                    if (ErrorLevel == Medium) {
                        AddWarning(5049);
                    }
                }
                break;
            case 114:
                // set.string(sA, mB)
                if (argval[0] == 0) {
                    if (MsgText[argval[1]].Length > 10) {
                        // warn user if setting input prompt to unusually long value
                        if (ErrorLevel == Medium) {
                            AddWarning(5096);
                        }
                    }
                }
                break;
            case 115:
                // get.string(sA, mB, ROW,COL,LEN)
                if (argval[2] > 24) {
                    // if row>24, both row/col are ignored
                    if (ErrorLevel == Medium) {
                        AddWarning(5052);
                    }
                }
                if (argval[3] > 39) {
                    // if col>39, len is limited automatically to <=40
                    if (ErrorLevel == Medium) {
                        AddWarning(5080);
                    }
                }
                if (argval[4] > 40) {
                    // invalid len value
                    if (ErrorLevel == Medium) {
                        AddWarning(5056);
                    }
                }
                break;
            case 121:
                // set.key(A,B,cC)
                if (argval[0] > 0 && argval[1] > 0 && argval[0] != 1) {
                    // A or B must be zero to be valid ascii or keycode
                    // (A can be 1 to mean joystick)
                    if (ErrorLevel == Medium) {
                        AddWarning(5065);
                    }
                }
                // check for improper ASCII assignments
                if (argval[1] == 0) {
                    if (argval[0] == 8 || argval[0] == 13 || argval[0] == 32) {
                        // ascii codes for bkspace, enter, spacebar
                        if (ErrorLevel == Medium) {
                            AddWarning(5066);
                        }
                    }
                }
                // check for improper KEYCODE assignments
                if (argval[0] == 0) {
                    if ((argval[1] >= 71 && argval[1] <= 73) ||
                        (argval[1] >= 75 && argval[1] <= 77) ||
                        (argval[1] >= 79 && argval[1] <= 83)) {
                        // ascii codes arrow keys can't be assigned to controller
                        if (ErrorLevel == Medium) {
                            AddWarning(5066);
                        }
                    }
                }
                break;
            case 122:
                // add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
                if (!fcompGame.agViews.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5064);
                        warned = true;
                    }
                }
                if (!warned) {
                    try {
                        unload = !fcompGame.agViews[argval[0]].Loaded;
                        if (unload) {
                            fcompGame.agViews[argval[0]].Load();
                        }
                        // if view is valid, check loop
                        if (argval[1] >= fcompGame.agViews[argval[0]].Loops.Count) {
                            if (ErrorLevel == Medium) {
                                AddWarning(5085);
                                warned = true;
                            }
                        }
                        // if loop is valid, check cel
                        if (!warned) {
                            if (argval[2] >= fcompGame.agViews[argval[0]][argval[1]].Cels.Count) {
                                if (ErrorLevel == Medium) {
                                    AddWarning(5086);
                                }
                            }
                            if (fcompGame.agViews[argval[0]][argval[1]][argval[2]].Width < 3 && argval[6] < 4) {
                                // CEL width must be >=3
                                if (ErrorLevel == Medium) {
                                    AddWarning(5110);
                                }
                            }
                        }
                    }
                    catch (Exception) {
                        // error trying to load- add a warning
                        AddWarning(5021, LoadResString(5021).Replace(ARG1, argval[0].ToString()));
                    }
                    if (unload) {
                        fcompGame.agViews[argval[0]].Unload();
                    }
                }
                if (argval[3] > 159 || argval[4] > 167) {
                    // invalid x or y value
                    if (ErrorLevel == Medium) {
                        AddWarning(5038);
                    }
                }
                if (argval[5] > 15) {
                    // invalid priority value
                    if (ErrorLevel == Medium) {
                        AddWarning(5079);
                    }
                }
                if (argval[5] < 4 && argval[5] != 0) {
                    // unusual priority value
                    if (ErrorLevel == Medium) {
                        AddWarning(5079);
                    }
                }
                if (argval[6] > 15) {
                    // MGN values >15 will only use lower nibble
                    if (ErrorLevel == Medium) {
                        AddWarning(5101);
                    }
                }
                break;
            case 129:
                // show.obj(VIEW)
                if (!fcompGame.agViews.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5061);
                    }
                }
                break;
            case 127 or 176 or 178:
                // init.disk, hide.mouse, show.mouse
                // these commands have no usefulness
                if (ErrorLevel == Medium) {
                    AddWarning(5087, LoadResString(5087).Replace(ARG1, ActionCommands[cmdNum].Name));
                }
                break;
            case 175 or 179 or 180:
                // discard.sound, fence.mouse, mouse.posn
                // these commands not valid in MSDOS AGI
                if (ErrorLevel == Medium) {
                    AddWarning(5088, LoadResString(5088).Replace(ARG1, ActionCommands[cmdNum].Name));
                }
                break;
            case 130:
                // random(LOWER,UPPER,vRESULT)
                if (argval[0] > argval[1]) {
                    // lower should be < upper
                    if (ErrorLevel == Medium) {
                        AddWarning(5054);
                    }
                }
                if (argval[0] == argval[1]) {
                    // lower=upper means result=lower=upper
                    if (ErrorLevel == Medium) {
                        AddWarning(5106);
                    }
                }
                if (argval[0] == argval[1] + 1) {
                    // this causes divide by 0!
                    if (ErrorLevel == Medium) {
                        AddWarning(5107);
                    }
                }
                if (argval[2] <= 26) {
                    // variable arg should not be a reserved Value
                    if (ErrorLevel == Medium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[cmdNum].Name));
                    }
                }
                break;
            case 134:
                // quit -
                // if v2.089 or earlier OR if arg is non-zero
                // no other commands will be processed
                if (fcompGame.agIntVersion == "2.089" || argval[0] > 0) {
                    endingCmd = 3;
                }
                break;
            case 142:
                // script.size
                if (complogicNumber != 0) {
                    // warn if in other than logic0
                    if (ErrorLevel == Medium) {
                        AddWarning(5039);
                    }
                }
                if (argval[0] < 10) {
                    // absurdly low value for script size
                    if (ErrorLevel == Medium) {
                        AddWarning(5009);
                    }
                }
                break;
            case 147:
                // reposition.to(oA, B,C)
                if (argval[1] > 159 || argval[2] > 167) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5023);
                    }
                }
                break;
            case 150:
                // trace.info(LOGIC,ROW,HEIGHT)
                if (!fcompGame.agLogs.Contains(argval[0])) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5040);
                    }
                }
                if (argval[2] < 2) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5046);
                    }
                }
                if (argval[1] + argval[2] > 23) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5063);
                    }
                }
                break;
            case 151 or 152:
                // print.at(mA, ROW, COL, MAXWIDTH)
                // print.at.v(vMSG, ROW, COL, MAXWIDTH)
                if (argval[1] > 22) {
                    if (ErrorLevel == Medium) {
                        AddWarning(5067);
                    }
                }
                switch (argval[3]) {
                case 0:
                    // maxwidth=0 defaults to 30
                    if (ErrorLevel == Medium) {
                        AddWarning(5105);
                    }
                    break;
                case 1:
                    // maxwidth=1 crashes AGI
                    if (ErrorLevel == Medium) {
                        AddWarning(5103);
                    }
                    break;
                default:
                    if (argval[3] > 36) {
                        // maxwidth >36 won't work
                        if (ErrorLevel == Medium) {
                            AddWarning(5104);
                        }
                    }
                    break;
                }
                if (argval[2] < 2 || argval[2] + argval[3] > 39) {
                    // invalid COL value
                    if (ErrorLevel == Medium) {
                        AddWarning(5068);
                    }
                }
                break;
            case 154:
                // clear.text.rect(R1,C1,R2,C2,COLOR)
                if (argval[0] > 24 || argval[1] > 39 ||
                   argval[2] > 24 || argval[3] > 39 ||
                   argval[2] < argval[0] || argval[3] < argval[1]) {
                    // invalid items
                    if (ErrorLevel == Medium) {
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
                }
                if (argval[4] > 0 && argval[4] != 15) {
                    // color value should be 0 or 15  (but
                    // it doesn't hurt to be anything else)
                    if (ErrorLevel == Medium) {
                        AddWarning(5100);
                    }
                }
                break;
            case 156:
                // set.menu(mA)
                if (menuitemCount == 0) {
                    AddWarning(5113, LoadResString(5113).Replace(ARG1, lastMenu));
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
                    // first item dttermines width
                    menuWidth = MsgText[argval[0]].Length;
                }
                else {
                    if (MsgText[argval[0]].Length > menuWidth) {
                        // item is wider than first item
                        if (ErrorLevel == Medium) {
                            AddWarning(5118, LoadResString(5118).Replace(ARG1, MsgText[argval[0]]));
                        }
                    }
                }
                menuitemCount++;
                if (menuitemCount == 23) {
                    // more than 22 items in a menu won't fit
                    if (ErrorLevel == Medium) {
                        AddWarning(5114, LoadResString(5114).Replace(ARG1, lastMenu));
                    }
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
                    if (ErrorLevel == Medium) {
                        AddWarning(5112);
                    }
                }
                if (menuitemCount == -1) {
                    // no menus added
                    AddWarning(5115);
                }
                else if (menuitemCount == 0) {
                    // no items in this menu
                    AddWarning(5113, LoadResString(5113).Replace(ARG1, lastMenu));
                }
                    break;
            case 174:
                // set.pri.base(A)
                if (argval[0] > 167) {
                    // value >167 doesn't make sense
                    if (ErrorLevel == Medium) {
                        AddWarning(5071);
                    }
                }
                break;
            }
        }

        /// <summary>
        /// This method checks the values passed as arguments to the specified test
        /// command and adds warnings or errors as needed.
        /// </summary>
        /// <param name="cmdNum"></param>
        /// <param name="argval"></param>
        /// <returns></returns>
        private static void ValidateIfArgs(int cmdNum, ref byte[] argval) {
            switch (cmdNum) {
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
            byte[] msgCharArray;

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
                int lngCryptStart = tmpLogRes.Size;
                for (int i = 1; i <= msgCount; i++) {
                    if (MsgInUse[i]) {
                        // calculate offset to start of this message (adjust by one byte,
                        // which is the byte that indicates how many msgs there are)
                        msgPos[i] = tmpLogRes.Pos - (msgSecStart + 1);
                    }
                    else {
                        // for unused messages need to write a null value for offset;
                        // (when it gets added after all messages are written it gets
                        // set to the beginning of message section)
                        msgPos[i] = 0;
                    }
                    if (MsgText[i].Length > 0) {
                        msgCharArray = Encoding.GetEncoding(fcompGame.CodePage).GetBytes(MsgText[i]);
                        for (int j = 0; j < msgCharArray.Length; j++) {
                            tmpLogRes.WriteByte((byte)(msgCharArray[j] ^ bytEncryptKey[(tmpLogRes.Pos - lngCryptStart) % 11]));
                        }
                    }
                    if (MsgInUse[i]) {
                        // add trailing zero to terminate message (if msg was zero length,
                        // terminator is still needed)
                        tmpLogRes.WriteByte((byte)(0 ^ bytEncryptKey[(tmpLogRes.Pos - lngCryptStart) % 11]));
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
        /// Scans the source code to identify all valid labels. This has to be done
        /// before starting the main compiler to correctly calculate jumps.
        /// </summary>
        private static void ReadLabels() {
            // fanAGI syntax is either 'label:' or ':label', with nothing
            // else in front of or after the label declaration
            byte i;
            string labelText;

            labelList = [];
            ResetCompiler();
            do {
                if (currentLine.Contains(':')) {
                    labelText = currentLine;
                    // check for 'label:'
                    if (labelText[^1] == ':') {
                        labelText = labelText[..^1];
                        if (labelText[^1] == ' ') {
                            // no spaces allowed between ':' and label name
                            // ignore it here; error will be caught in main compiler
                            labelText = "";
                        }
                    }
                    // check for ':label'
                    else if (labelText[0] == ':') {
                        labelText = labelText[1..];
                        if (labelText[0] == ' ') {
                            // no spaces allowed between label name and :
                            // ignore it here; error will be caught in main compiler
                            labelText = "";
                        }
                    }
                    else {
                        // not a label
                        labelText = "";
                    }
                    // if a label was found, validate it
                    if (labelText.Length != 0) {
                        DefineNameCheck chkLabel = ValidateDefineName(labelText, fcompGame);
                        switch (chkLabel) {
                        case Numeric:
                            // numbers are ok for labels
                            break;
                        case DefineNameCheck.Empty:
                            AddError(4074, false);
                            break;
                        case ActionCommand:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "an action command"), false);
                            break;
                        case TestCommand:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a test command"), false);
                            break;
                        case KeyWord:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a compiler keyword"), false);
                            break;
                        case ArgMarker:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "an argument marker"), false);
                            break;
                        case DefineNameCheck.Global:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a global define"), false);
                            break;
                        case ReservedVar:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a reserved variable name"), false);
                            break;
                        case ReservedFlag:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a reserved flag"), false);
                            break;
                        case ReservedNum:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a reserved number constant"), false);
                            break;
                        case ReservedObj or ReservedStr:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a resrved object or string name"), false);
                            break;
                        case ReservedMsg:
                            AddError(4015, LoadResString(4015).Replace(
                                ARG1, labelText).Replace(
                                ARG2, "a reserved message constant"), false);
                            break;
                        case BadChar:
                            AddError(4017, false);
                            break;
                        }
                        // check against existing labels
                        for (i = 0; i < labelList.Count; i++) {
                            if (labelText == labelList[i].Name) {
                                AddError(4016, LoadResString(4016).Replace(ARG1, labelText), false);
                                break;
                            }
                        }
                        // increment number of labels, and save
                        LogicLabel tmp = new() {
                            Name = labelText,
                            Loc = 0
                        };
                        labelList.Add(tmp);
                    }
                }
                IncrementLine();
            } while (lineNumber != -1);
        }

        /// <summary>
        /// This is the main compiler function. It steps through input source
        /// code one token at a time and converts it to AGI logic byte code.
        /// </summary>
        /// <returns>true if source is successfully compiled, false if any 
        /// errors are encountered.</returns>
        private static bool CompileFAN() {
            int returnLine = 0, lastLine = 0;
            int quitLine = -1, newroomLine = -1;
            bool lastCmdRtn = false, argsOK;
            byte[] argvalarray = new byte[8];
            List<CompilerBlockType> Block = [];
            CompilerBlockType lastBlock = new();
            List<LogicGoto> Gotos = [];
            CompilerToken argToken;
            CompilerToken prevToken = new();

            ResetCompiler();
            endingCmd = 0;
            menuitemCount = -1;
            menuWidth = 0;
            lastMenu = "";
            menuSet = false;
            CompilerToken nextToken = NextToken();
            // process tokens from the input string list until finished
            while (lineNumber != -1) {
                lastCmdRtn = false;
                if (endingCmd > 0) {
                    if (nextToken.Text != "}") {
                        if (ErrorLevel == Medium) {
                            switch (endingCmd) {
                            case 1:
                                // return
                                AddWarning(5097);
                                break;
                            case 2:
                                // new.room
                                if (nextToken.Text == "return") {
                                    newroomLine = lineNumber;
                                }
                                else {
                                    AddWarning(5095);
                                }
                                break;
                            case 3:
                                // unconditional quit
                                if (nextToken.Text == "return") {
                                    // possible end of logic; in 
                                    // that case, no need for a warning here
                                    // UNLESS this isn't the end, so use a
                                    // flag to check for the condition
                                    quitLine = lineNumber;
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
                // first check for indirection
                if (nextToken.Indirect) {
                    // only if a variable
                    if (nextToken.Type != ArgType.Var) {
                        // error!
                        Debug.Assert(false);
                    }
                }
                switch (nextToken.Type) {
                case ArgType.Symbol:
                    switch (nextToken.Text) {
                    case "{":
                        skipError = false;
                        // unexpected- check for double-up
                        if (prevToken.Text == "if" || prevToken.Text == "else") {
                            AddError(4068, false);
                        }
                        else {
                            AddError(4006, false);
                        }
                        break;
                    case "}":
                        skipError = false;
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
                    case "/*" or "*/":
                        skipError = false;
                        // since block tokens are no longer supported, check for markers in
                        // order to provide a meaningful error message
                        AddError(4029, false);
                        break;
                    case ":":
                        skipError = false;
                        // alternate label syntax - next token should be the label
                        nextToken = NextToken();
                        if (lineNumber == -1) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            skipError = false;
                            return false;
                        }
                        if (nextToken.Type == Label) {
                            // save position of label
                            LogicLabel tmpLabel = new() {
                                Name = labelList[nextToken.NumValue].Name,
                                Loc = tmpLogRes.Size
                            };
                            labelList[nextToken.NumValue] = tmpLabel;
                        }
                        else {
                            // not a valid label
                            AddError(4046, LoadResString(4046).Replace(ARG1, nextToken.Text), false);
                        }
                        break;
                    default:
                        // invalid symbol
                        AddError(4023, LoadResString(4023).Replace(ARG1, nextToken.Text), false);
                        // ignore rest of errors until a valid starting token is found
                        skipError = true;
                        break;
                    }
                    break;
                case AssignOperator:
                    switch (nextToken.Text) {
                    case "++" or "--":
                        skipError = false;
                        // unary operators
                        if (nextToken.Text == "++") {
                            tmpLogRes.WriteByte(1);
                        }
                        else {
                            tmpLogRes.WriteByte(2);
                        }
                        // get variable
                        argToken = NextToken();
                        if (lineNumber == -1) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            skipError = false;
                            return false;
                        }
                        if (argToken.Type != ArgType.Var) {
                            AddError(4025, false);
                            // use a temp placeholder
                            argToken.Value = "255";
                        }
                        if (argToken.Indirect) {
                            // error - 
                            AddError(4025, false);
                            // use a temp placeholder
                            argToken.Value = "255";
                        }
                        if (argToken.NumValue > 255) {
                            AddError(4039, LoadResString(4039).Replace(ARG1, "1"), false);
                            // use a temp placeholder
                            argToken.Value = "255";
                        }
                        // write the variable value
                        tmpLogRes.WriteByte((byte)argToken.NumValue);
                        // verify next token is correct end of line marker
                        CheckForEOL();
                        break;
                    default:
                        // invalid symbol
                        AddError(4023, LoadResString(4023).Replace(ARG1, nextToken.Text), false);
                        skipError = true;
                        break;
                    }
                    break;
                case ArgType.Keyword:
                    skipError = false;
                    switch (nextToken.Text) {
                    case "if":
                        if (!CompileIf()) {
                            // error in 'if' block
                            return false;
                        }
                        CompilerBlockType block = new();
                        block.StartPos = tmpLogRes.Pos;
                        block.IsIf = true;
                        Block.Add(block);
                        // write placeholder for block length
                        tmpLogRes.WriteWord(0);
                        // next token should be a bracket
                        if (!CheckChar('{')) {
                            AddError(4030, false);
                            // if a stray ')' just ignore it
                            if (CheckChar(')')) {
                                // and check for bracket
                                _ = CheckChar('{');
                            }
                        }
                        break;
                    case "else":
                        // else can only follow a close bracket
                        if (prevToken.Text != "}") {
                            AddError(4009, false);
                            // if there is a block, assume there was a '}'
                            // and continue
                        }
                        if (!lastBlock.IsIf) {
                            // previous block was an 'else' - can't be followed
                            // by another 'else'
                            AddError(4056, false);
                            return false;
                        }
                        // adjust blockdepth to match the 'if' block directly before
                        // this 'else'
                        Block.Add(new());
                        Block[^1].IsIf = lastBlock.IsIf;
                        Block[^1].StartPos = lastBlock.StartPos;
                        Block[^1].Length = lastBlock.Length;
                        // adjust the block length to accomodate the 'else' statement
                        Block[^1].Length += 3;
                        tmpLogRes.WriteWord((ushort)Block[^1].Length, Block[^1].StartPos);
                        // the block is now an 'else' block
                        Block[^1].IsIf = false;
                        // write the 'else' bytecode
                        tmpLogRes.WriteByte(0xFE);
                        Block[^1].StartPos = tmpLogRes.Pos;
                        // placeholder for block length
                        tmpLogRes.WriteWord(0);
                        // next token better be a bracket
                        if (!CheckChar('{')) {
                            AddError(4030, false);
                        }
                        break;
                    case "goto":
                        // next token should be "("
                        if (!CheckChar('(')) {
                            AddError(4004, false);
                        }
                        argToken = NextToken();
                        if (lineNumber == -1) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            skipError = false;
                            return false;
                        }
                        if (argToken.Type != Label) {
                            // argument is NOT a valid label
                            AddError(4046, LoadResString(4046).Replace(ARG1, argToken.Text), false);
                        }
                        else {
                            // save this goto info on goto stack
                            // write goto command byte
                            tmpLogRes.WriteByte(0xFE);
                            LogicGoto addgoto = new() {
                                LabelNum = (byte)argToken.NumValue,
                                DataLoc = tmpLogRes.Pos
                            };
                            Gotos.Add(addgoto);
                            // write placeholder for amount of offset
                            tmpLogRes.WriteWord(0);
                        }
                        // next character should be ")"
                        if (NextChar() != ')') {
                            AddError(4002, false);
                        }
                        // verify command is correct end-of-line marker
                        CheckForEOL();
                        break;
                    default:
                        // invalid
                        break;
                    }
                    break;
                case ArgType.Label:
                    skipError = true;
                    LogicLabel tmp = new() {
                        Name = labelList[nextToken.NumValue].Name,
                        Loc = tmpLogRes.Size
                    };
                    labelList[nextToken.NumValue] = tmp;
                    // next token should be ':' (validated in ReadLabels)
                    argToken = NextToken(false, true);
                    if (lineNumber == -1) {
                        // nothing left, return critical error
                        AddError(4057, true);
                        skipError = false;
                        return false;
                    }
                    if (argToken.Text != ":") {
                        Debug.Assert(false);
                        // not a valid label definition
                        AddError(4047, false);
                        // skip to end of line
                        charPos = currentLine.Length - 1;
                    }
                    break;
                case ArgType.ActionCmd:
                    skipError = false;
                    // write the command code,
                    byte cmdnum = (byte)nextToken.NumValue;
                    tmpLogRes.WriteByte(cmdnum);
                    // next character should be "("
                    if (!CheckChar('(')) {
                        AddError(4027, false);
                    }
                    // now extract arguments (assume all OK)
                    argsOK = true;
                    for (int i = 0; i < ActionCommands[cmdnum].ArgType.Length; i++) {
                        if (i > 0) {
                            if (!CheckChar(',')) {
                                // use 1-base arg values (but since referring to
                                // previous arg, don't increment arg index)
                                AddError(4026, LoadResString(4026).Replace(
                                    ARG1, i.ToString()).Replace(
                                    ARG2, ActionCommands[cmdnum].Name), false);
                            }
                        }
                        argToken = NextToken();
                        if (lineNumber == -1) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            skipError = false;
                            return false;
                        }
                        int argval = argToken.NumValue;
                        // message may be a literal string
                        if (ActionCommands[cmdnum].ArgType[i] == Msg && argToken.Type == ArgType.DefStr) {
                            if (argToken.Source == CompilerTokenSource.Code) {
                                // concatenate first
                                argToken = ConcatArg(argToken);
                            }
                            // check for bad string
                            if (argToken.Type == ArgType.BadString) {
                                AddError(4052, false);
                                // use a placeholder value
                                argval = 1;
                            }
                            else {
                                // convert to msg number
                                argval = MessageNum(argToken.Value);
                                // if too many messages
                                if (argval == 0) {
                                    AddError(4071, false);
                                    // use a placeholder value
                                    argval = 1;
                                }
                            }
                            argvalarray[i] = (byte)argval;
                        }
                        // inv item may be a literal string
                        else if (ActionCommands[cmdnum].ArgType[i] == InvItem && argToken.Type == ArgType.DefStr) {
                            argvalarray[i] = (byte)ValidateInvItem(ref argToken);
                        }
                        else {
                            if (argToken.Type == ActionCommands[cmdnum].ArgType[i]) {
                                ValidateArgType(ref argval, ActionCommands[cmdnum].ArgType[i], i);
                                argvalarray[i] = (byte)argval;
                            }
                            else {
                                if (argToken.Text == "," || argToken.Text == ")") {
                                    AddError(4031, LoadResString(4031).Replace(
                                        ARG1, (i + 1).ToString()).Replace(
                                        ARG2, ActionCommands[cmdnum].Name).Replace(
                                        ARG3, ArgTypeName(ActionCommands[cmdnum].ArgType[i])), false);
                                    // backup one space so the ',' or ')' will be found at end of command
                                    charPos--;
                                    // if ')', end the loop early
                                    if (argToken.Text == ")") {
                                        break;
                                    }
                                }
                                else {
                                    // wrong token
                                    AddError(4037, LoadResString(4037).Replace(
                                        ARG1, (i + 1).ToString()).Replace(
                                        ARG2, ArgTypeName(ActionCommands[cmdnum].ArgType[i])).Replace(
                                        ARG3, argToken.Text), false);
                                }
                                // use a placeholder value
                                argvalarray[i] = 0;
                                argsOK = false;
                            }
                        }
                        // write argument
                        tmpLogRes.WriteByte(argvalarray[i]);
                    }
                    if (argsOK) {
                        // validate arguments for this command
                        ValidateArgs(cmdnum, ref argvalarray);
                    }
                    // next character must be ")"
                    if (!CheckChar(')')) {
                        AddError(4084, false);
                    }
                    // check for return command
                    if (cmdnum == 0) {
                        lastCmdRtn = true;
                        if (returnLine == 0 && Block.Count == 1) {
                            returnLine = errorLine;
                        }
                        if (newroomLine >= 0 || quitLine >= 0) {
                            // only ok if last cmd in block and block is 0 
                            if (Block.Count != 0) {
                                if (newroomLine >= 0) {
                                    lineNumber = newroomLine;
                                    AddWarning(5095, LoadResString(5095), newroomLine);
                                }
                                if (quitLine >= 0) {
                                    lineNumber = quitLine;
                                    AddWarning(5111, LoadResString(5111), quitLine);
                                }
                            }
                        }
                        quitLine = -1;
                        newroomLine = -1;
                    }
                    // verify next token is correct end of line marker
                    CheckForEOL();
                    break;
                case ArgType.Var:
                case ArgType.Flag:
                case ArgType.Str:
                    skipError = false;
                    if (CompileSpecial(nextToken)) {
                        // verify next token is correct end of line marker
                        CheckForEOL();
                    }
                    break;
                default:
                    switch (nextToken.Type) {
                    case Num:
                        // numeric value
                        AddError(4064, LoadResString(4064).Replace(ARG1, "Number"), false);
                        break;
                    case Msg:
                    case SObj:
                    case InvItem:
                    case Word:
                    case Ctrl:
                        // m##, o##, i##, w##, c##
                        AddError(4063, LoadResString(4063).Replace(ARG1, nextToken.Text), false);
                        break;
                    case DefStr:
                        AddError(4064, LoadResString(4064).Replace(ARG1, "String"), false);
                        break;
                    case BadString:
                        AddError(4064, LoadResString(4064).Replace(ARG1, "Invalid string"), false);
                        break;
                    case TestCmd:
                        AddError(4062, LoadResString(4062).Replace(ARG1, nextToken.Text), false);
                        if (PeekToken(true).Text == "(") {
                            // assume test cmd name out of order, skip to ')'
                            do {
                                nextToken = NextToken(true, false);
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                if (nextToken.Text == ")") {
                                    break;
                                }
                                if (nextToken.Text == ";") {
                                    charPos--;
                                    break;
                                }
                            } while (nextToken.Type != ArgType.None);
                        }
                        break;
                    case TestOperator:
                        // '==', '!=', '<', '<=', '>', '>='
                        // invalid symbol
                        AddError(4023, LoadResString(4023).Replace(ARG1, nextToken.Text), false);
                        break;
                    case Unknown:
                        // used when type is cannot be determined
                        // unknown command - check for preprocessor symbol
                        if (nextToken.Text[0] == '#' || nextToken.Text[0] == '%') {
                            AddError(4036, LoadResString(4036).Replace(ARG1, nextToken.Text), false);
                        }
                        else if (PeekToken(true).Text == "(") {
                            // check for possible misspelled command
                            // assume bad cmd name; skip to ')'
                            AddError(4082, LoadResString(4082).Replace(ARG1, nextToken.Text), false);
                            do {
                                nextToken = NextToken(true, false);
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                if (nextToken.Text == ")") {
                                    break;
                                }
                                if (nextToken.Text == ";") {
                                    charPos--;
                                    break;
                                }
                            } while (nextToken.Type != ArgType.None);
                        }
                        else {
                            // unknown syntax error
                            AddError(4064, LoadResString(4064).Replace(ARG1, "Undefined token '" + nextToken.Text +"'"), false);
                        }
                        break;
                    }
                    skipError = true;
                    break;
                    case VocWrd:
                    case Obj:
                    case ArgType.View:
                    case None:
                        // none of these should be possible
                        break;
                }
                prevToken = nextToken;
                lastLine = errorLine;
                nextToken = NextToken();
            }
            // check to see if everything was wrapped up properly
            if (Block.Count > 0) {
                errorLine = lastLine;
                AddError(4007, false);
                return false;
            }
            if (!lastCmdRtn) {
                AddError(4059, false);
                return false;
            }
            // write in goto values
            for (int i = 0; i < Gotos.Count; i++) {
                int jump = labelList[Gotos[i].LabelNum].Loc - Gotos[i].DataLoc - 2;
                // need to convert it to an unsigned short Value so negative
                // jump values work correctly
                tmpLogRes.WriteWord((ushort)jump, Gotos[i].DataLoc);
            }
            return true;
        }

        /// <summary>
        /// Returns the index number of the specified message, or creates a new
        /// message number index if the message is not currently in this logic's
        /// list of messages. The passed message includes starting and ending quotes.
        /// </summary>
        /// <param name="msgText"></param>
        /// <returns></returns>
        private static int MessageNum(string msgText) {
            // strip off quotes
            msgText = msgText[1..^1];
            if (msgText.Length == 0) {
                // blank messages are not common
                if (ErrorLevel == Medium) {
                    AddWarning(5074);
                }
            }
            // convert to AGI encoded string
            msgText = ConvertSlashCodes(msgText);
            for (int i = 1; i <= 255; i++) {
                if (MsgText[i] == msgText) {
                    if (!MsgInUse[i]) {
                        MsgInUse[i] = true;
                    }
                    // if this msg has an extended char warning, repeat it here
                    if ((MsgWarnings[i] & 1) == 1) {
                        AddWarning(5093);
                    }
                    if ((MsgWarnings[i] & 2) == 2) {
                        AddWarning(5094);
                    }
                    return i;
                }
            }
            // message isn't in list yet, find an empty spot
            for (int i = 1; i <= 255; i++) {
                if (!MsgInUse[i]) {
                    MsgInUse[i] = true;
                    MsgText[i] = msgText;
                    // validate it check for extended characters
                    ValidateMsgChars(msgText, i);
                    return i;
                }
            }
            // no room, return zero
            return 0;
        }

        /// <summary>
        /// Gets the AGI byte code for the specified command.
        /// </summary>
        /// <param name="isIfCmd"></param>
        /// <param name="cmdName"></param>
        /// <returns></returns>
        private static byte CommandNum(bool isIfCmd, string cmdName) {
            if (isIfCmd) {
                // look for test command
                for (byte retval = 0; retval < agNumTestCmds; retval++) {
                    if (cmdName == TestCommands[retval].Name) {
                        return retval;
                    }
                }
                // check defines
                for (int i = 0; i < definesList.Count; i++) {
                    if (definesList[i].Type == TestCmd) {
                        if (definesList[i].Name == cmdName) {
                            return byte.Parse(definesList[i].Value);
                        }
                    }
                }
            }
            else {
                // look for action command
                for (byte retval = 0; retval < agNumCmds; retval++) {
                    if (cmdName == ActionCommands[retval].Name) {
                        return retval;
                    }
                }
                // maybe the command is a valid agi command, but
                // just not supported in this agi version
                for (byte retval = agNumCmds; retval < MAX_CMDS; retval++) {
                    if (cmdName == ActionCommands[retval].Name) {
                        if (ErrorLevel == Medium) {
                            AddWarning(5075, LoadResString(5075).Replace(ARG1, cmdName));
                        }
                        return retval;
                    }
                }
                // check defines
                for (int i = 0; i < definesList.Count; i++) {
                    if (definesList[i].Type == ActionCmd) {
                        if (definesList[i].Name == cmdName) {
                            return byte.Parse(definesList[i].Value);
                        }
                    }
                }
            }
            // not found
            return 255;
        }

        /// <summary>
        /// This method determines if the specified token represents the start of
        /// a properly formatted special assignment syntax, and if so, parses it. 
        /// </summary>
        /// <param name="strArgIn"></param>
        /// <returns></returns>
        private static bool CompileSpecial(CompilerToken argToken) {
            // properly formatted special assignment syntax will be one of
            // the following:
            //     arg1 exprC arg2
            //     arg1 = arg1 exprA arg2
            //     arg1 exprU
            //        
            // arg1 must be in the format of *v#, v#, f# or s#
            // exprA must be one of the following assignment tokens:
            //     +, -, *, /
            // exprC must be one of the following compound assignment tokens:
            //     =, +=, -=, *=, /=
            //
            // exprU must be one of the following unary operator tokens:
            //     ++, --
            //
            // if arg1 is a flag, only valid exprC is '=', and only valid
            // arg2 value is 'true' or 'false';
            //
            // if arg1 is a string, only valid exprC is '=' and only valid
            // arg2 value is m# (or a literal string)
            int arg1val, arg2val, maxStr;
            int indirection = 0; // 0 = no indirection; 1 = left; 2 = right
            byte cmdval = 0;
            byte[] argvalarray;

            // validate arg number first
            arg1val = argToken.NumValue;
            ValidateArgType(ref arg1val, argToken.Type, 0);
            switch (argToken.Type) {
            case ArgType.Str:
                // string assignment
                //     s# = m#
                //     s# = "<string>"

                if (ErrorLevel == Medium) {
                    // for version 2.089, 2.272, and 3.002149 only 12 strings
                    maxStr = fcompGame.agIntVersion switch {
                        "2.089" or "2.272" or "3.002149" => 11,
                        _ => 23,
                    };
                    if (arg1val > maxStr) {
                        AddWarning(5007, LoadResString(5007).Replace(ARG1, maxStr.ToString()));
                    }
                }
                // next token must be "="
                argToken = NextToken();
                if (lineNumber == -1) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    skipError = false;
                    return false;
                }
                switch (argToken.Text) {
                case "=":
                    // OK
                    break;
                case "==":
                    AddError(4018, false);
                    break;
                default:
                    AddError(4023, LoadResString(4023).Replace(ARG1, argToken.Text), false);
                    // line is seriously messed; skip further errors
                    skipError = true;
                    break;
                }
                // get argument 2
                argToken = NextToken();
                if (lineNumber == -1) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    skipError = false;
                    return false;
                }
                switch (argToken.Type) {
                case ArgType.Msg:
                    arg2val = argToken.NumValue;
                    ValidateArgType(ref arg2val, ArgType.Msg, 1);
                    break;
                case ArgType.DefStr:
                    // convert to msg number
                    arg2val = MessageNum(argToken.Value);
                    // if too many messages
                    if (arg2val == 0) {
                        AddError(4071, false);
                    }
                    break;
                default:
                    // check for undefined token first
                    if (argToken.Type == ArgType.Unknown) {
                        AddError(4037, LoadResString(4037).Replace(
                            ARG1, "2").Replace(
                            ARG2, "message").Replace(
                            ARG3, argToken.Text), false);
                        // use a place holder
                        arg2val = 1;
                    }
                    else {
                        AddError(4034, false);
                        // check for premature eol marker
                        if (argToken.Text == ";") {
                            charPos--;
                            return true;
                        }
                        return false;
                    }
                    break;
                }
                // command is set.string
                tmpLogRes.WriteByte(0x72);
                tmpLogRes.WriteByte((byte)arg1val);
                tmpLogRes.WriteByte((byte)arg2val);
                return true;
            case ArgType.Var:
                if (argToken.Indirect) {
                    // left indirection
                    //     *v# = #
                    //     *v# = v#
                    indirection = 1;
                }
                // need next token to determine what kind of assignment/operation
                argToken = NextToken();
                if (lineNumber == -1) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    skipError = false;
                    return false;
                }
                // for indirection only '=' is valid
                if (indirection == 1) {
                    switch (argToken.Text) {
                    case "=":
                        // ok
                        break;
                    case "==":
                        AddError(4018, false);
                        break;
                    case "+=" or "-=" or "*=" or "/=" or "++" or "--":
                        // error- math ops not allowed on indirect variables
                        AddError(4078, false);
                        // reset indirection to continue
                        indirection = 0;
                        break;
                    default:
                        // this line is messed up
                        AddError(4023,LoadResString(4023).Replace(ARG1, argToken.Text), false);
                        skipError = true;
                        return false;
                    }
                }
                if (indirection == 0) {
                    switch (argToken.Text) {
                    case "++":
                        // v#++;
                        tmpLogRes.WriteByte(0x01);
                        tmpLogRes.WriteByte((byte)arg1val);
                        return true;
                    case "--":
                        // v#--
                        tmpLogRes.WriteByte(0x02);
                        tmpLogRes.WriteByte((byte)arg1val);
                        return true;
                    case "+=":
                        // v# += #; or v# += v#;
                        cmdval = 0x05;
                        break;
                    case "-=":
                        // v# -= #; or v# -= v#;
                        cmdval = 0x07;
                        break;
                    case "*=":
                        // v# *= #; or v# *= v#;
                        cmdval = 0xA5;
                        break;
                    case "/=":
                        // v# /= #; v# /= v#
                        cmdval = 0xA7;
                        break;
                    case "=":
                        // right indirection
                        //     v# = *v#;
                        // or assignment
                        //     v# = v#;
                        //     v# = #;
                        // or long arithmetic operation
                        //     v# = v# + #; v# = v# + v#;
                        //     v# = v# - #; v# = v# - v#;
                        //     v# = v# * #; v# = v# * v#;
                        //     v# = v# / #; v# = v# / v#;
                        break;
                    case "==":
                        // error, but treat as '='
                        AddError(4018, false);
                        break;
                    case "@=":
                        // v# @= #; v# @= v#;
                        // Sierra syntax only
                        AddError(4087, false);
                        // use 'add.n' as placeholder
                        cmdval = 0x05;
                        break;
                    case "=@":
                        // v# =@ v#;
                        // Sierra syntax only
                        AddError(4087, false);
                        // use 'add.v' as a placeholder
                        cmdval = 0x06;
                        break;
                    default:
                        // no idea what's going on in this line
                        AddError(4023, LoadResString(4023).Replace(ARG1, argToken.Text), false);
                        skipError = true;
                        return false;
                    }
                }
                // get second argument
                argToken = NextToken();
                if (lineNumber == -1) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    skipError = false;
                    return false;
                }
                if (argToken.Indirect) {
                    // confirm it's a valid variable
                    if (argToken.Type == Var) {
                        // right indirection
                        if (indirection == 0) {
                            // if cmd already set, it's an error
                            if (cmdval != 0) {
                                AddError(4081, false);
                            }
                            else {
                                // right indirection
                                indirection = 2;
                            }
                        }
                        else {
                            // no double-indirection
                            AddError(4081, false);
                            // clear indirection to continue
                            argToken.Indirect = false;
                            indirection = 0;
                        }
                    }
                    else {
                        AddError(4080, false);
                        // set to variable to continue
                        argToken.Type = Var;
                    }
                }
                // arg2 may be v##, or ##
                arg2val = argToken.NumValue;
                ValidateArgType(ref arg2val, argToken.Type, 1);
                switch (argToken.Type) {
                case Var:
                    if (cmdval != 0) {
                        // use variable version of command
                        cmdval++;
                    }
                    break;
                case Num:
                    break;
                default:
                    // not a valid line
                    AddError(4085, LoadResString(4085).Replace(ARG1, argToken.Text), false);
                    return true;
                }
                if (argToken.Type == Num) {
                    // if cmd not yet known
                    if (cmdval == 0) {
                        // must be assign:
                        // v# = # or *v# = #
                        if (indirection == 1) {
                            // lindirect.n
                            cmdval = 0x0B;
                        }
                        else {
                            // assign.n
                            cmdval = 0x03;
                        }
                    }
                }
                if (cmdval == 0) {
                    // command is still not known
                    if ((arg1val == arg2val) && (indirection == 0)) {
                        // arg values are the same (already know arg2 is a variable)
                        // and no indirection - check for long arithmetic
                        if (lineNumber == -1) {
                            // nothing left, return critical error
                            AddError(4057, true);
                            skipError = false;
                            return false;
                        }
                        argToken = NextToken();
                        arg2val = argToken.NumValue;
                        if (argToken.Text == ";") {
                            // this is a simple assign (with a variable being assigned
                            // to itself!!)
                            if (ErrorLevel == Medium) {
                                AddWarning(5036);
                            }
                            // assign.v
                            cmdval = 0x04;
                            // move pointer back one space so eol check in main compiler
                            // works correctly
                            charPos--;
                        }
                        else {
                            // this may be long arithmetic
                            switch (argToken.Text) {
                            case "+":
                                cmdval = 0x05;
                                break;
                            case "-":
                                cmdval = 0x07;
                                break;
                            case "*":
                                cmdval = 0xA5;
                                break;
                            case "/":
                                cmdval = 0xA7;
                                break;
                            default:
                                AddError(4058, false);
                                return false;
                            }
                            // now get actual second argument (number or variable)
                            argToken = NextToken();
                            if (lineNumber == -1) {
                                // nothing left, return critical error
                                AddError(4057, true);
                                skipError = false;
                                return false;
                            }
                            arg2val = argToken.NumValue;
                            if (argToken.Type != ArgType.Var && argToken.Type != ArgType.Num) {
                                AddError(4058, false);
                                return false;
                            }
                            ValidateArgType(ref arg2val, argToken.Type, 2);
                            if (argToken.Type == ArgType.Var) {
                                // use variable version of command
                                cmdval++;
                            }
                        }
                    }
                    else {
                        // the second variable argument is different, or indirection
                        // must be assignment
                        //     v# = v#
                        //     *v# = v#
                        //     v# = *v#
                        switch (indirection) {
                        case 0:
                            // assign.v
                            cmdval = 0x04;
                            // check for improper math
                            // vA = vB + # for example
                            switch (NextChar().ToString()) {
                            case "+" or "-" or "*" or "/":
                                // bad math
                                AddError(4003, false);
                                // assume next token is arg2
                                argToken = NextToken();
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                // ignore value, just use a placeholder
                                arg2val = 255;
                                break;
                            default:
                                // assume ok, put char back on stream
                                charPos--;
                                break;
                            }
                            break;
                        case 1:
                            // lindirect.v
                            cmdval = 0x09;
                            // check for improper math
                            switch (NextChar().ToString()) {
                            case "+" or "-" or "*" or "/":
                                // bad indirect math
                                AddError(4081, false);
                                // assume next token is arg2
                                argToken = NextToken();
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                // ignore value, just use a placeholder
                                arg2val = 255;
                                break;
                            default:
                                // assume ok, put char back on stream
                                charPos--;
                                break;
                            }
                            break;
                        case 2:
                            // rindirect
                            cmdval = 0x0A;
                            // check for improper math
                            switch (NextChar().ToString()) {
                            case "+" or "-" or "*" or "/":
                                // bad indirect math
                                AddError(4081, false);
                                // assume next token is arg2
                                argToken = NextToken();
                                if (lineNumber == -1) {
                                    // nothing left, return critical error
                                    AddError(4057, true);
                                    skipError = false;
                                    return false;
                                }
                                // ignore value, just use a placeholder
                                arg2val = 255;
                                break;
                            default:
                                // assume ok, put char back on stream
                                charPos--;
                                break;
                            }
                            break;
                        }
                    }
                }
                // validate commands before writing
                argvalarray = new byte[2];
                argvalarray[0] = (byte)arg1val;
                argvalarray[1] = (byte)arg2val;
                ValidateArgs(cmdval, ref argvalarray);
                // write command and arg1
                tmpLogRes.WriteByte(cmdval);
                tmpLogRes.WriteByte((byte)arg1val);
                // write second argument
                switch (cmdval) {
                case 0x01 or 0x02:
                    break;
                default:
                    tmpLogRes.WriteByte((byte)arg2val);
                    break;
                }
                return true;
            case ArgType.Flag:
                // flag assignment
                //     f# = true;
                //     f# = false;
                // next token must be "="
                argToken = NextToken();
                if (lineNumber == -1) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    skipError = false;
                    return false;
                }
                switch (argToken.Text) {
                case "=":
                    // OK
                    break;
                case "==":
                    AddError(4018, false);
                    break;
                default:
                    AddError(4023, LoadResString(4023).Replace(ARG1, argToken.Text), false);
                    // line is seriously messed; skip further errors
                    skipError = true;
                    break;
                }
                argToken = NextToken();
                if (lineNumber == -1) {
                    // nothing left, return critical error
                    AddError(4057, true);
                    skipError = false;
                    return false;
                }
                switch (argToken.Text) {
                case "true":
                    // set this flag
                    cmdval = 0x0C;
                    break;
                case "false":
                    // reset this flag
                    cmdval = 0x0D;
                    break;
                default:
                    AddError(4061, false);
                    // if it was ';', back up to use it
                    if (argToken.Text == ";") {
                        charPos--;
                    }
                    return true;
                }
                // command is set.string
                tmpLogRes.WriteByte(cmdval);
                tmpLogRes.WriteByte((byte)arg1val);
                return true;
            }
            // should never get here
            return false;
        }
        #endregion
    }
}
