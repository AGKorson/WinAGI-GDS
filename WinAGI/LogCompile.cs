using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
using static WinAGI.Engine.LogicErrorLevel;

namespace WinAGI.Engine {
    public static partial class Compiler {
        // reminder: in VB6 version, the compiler was not case sensitive
        // EXCEPT strings in messages; in this version ALL tokens are
        // case sensitive

        #region Structs
        internal struct LogicGoto {
            internal byte LabelNum;
            internal int DataLoc;
        }
        internal struct LogicLabel {
            internal string Name;
            internal int Loc;
        }
        #endregion

        #region Members
        internal static Logic tmpLogRes;
        internal static AGIGame compGame;
        internal static byte[] mbytData;
        internal const int MAX_BLOCK_DEPTH = 64;
        internal const int MAX_LABELS = 255;
        internal const bool UseTypeChecking = true;
        internal const string NOT_TOKEN = "!";
        internal const string OR_TOKEN = " || ";
        internal const string AND_TOKEN = "&&";
        internal const string NOTEQUAL_TOKEN = "!=";
        internal const string EQUAL_TOKEN = "==";
        internal const string CONST_TOKEN = "#define ";
        private const string MSG_TOKEN = "#message";
        internal const string CMT1_TOKEN = "[";
        internal const string CMT2_TOKEN = "//"; // deprecated
        internal const char INDIR_CHAR = '*';
        internal static byte bytLogComp;
        internal static string strLogCompID;
        internal static bool blnCriticalError, blnMinorError;
        internal static int lngQuoteAdded;
        internal static TWinAGIEventInfo errInfo = new() {
            Type = EventType.etError,
            ID = "",
            Text = "",
            Line = "--",
            Module = "",
        };
        internal static int intCtlCount;
        internal static bool blnNewRoom;
        internal static string INCLUDE_MARK = ((char)31).ToString() + "!";
        internal static string[] strIncludeFile;
        internal static int lngIncludeOffset; // to correct line number due to added include lines
        internal static int intFileCount;
        internal static List<string> stlInput = [];  // the entire text to be compiled; includes the
                                                     // original logic text, includes, and defines
        internal static int lngLine;
        internal static int lngPos;
        internal static string strCurrentLine;
        internal static string[] strMsg = new string[256];
        internal static bool[] blnMsg = new bool[255];
        internal static int[] intMsgWarn = new int[255]; //to track warnings found during msgread function
        internal static LogicLabel[] llLabel = new LogicLabel[MAX_LABELS];
        internal static byte bytLabelCount;
        internal static TDefine[] tdDefines;
        internal static int lngDefineCount;
        internal static bool blnSetIDs;
        internal static string[] strLogID;
        internal static string[] strPicID;
        internal static string[] strSndID;
        internal static string[] strViewID;
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods
        /// <summary>
        /// This method compiles the sourcetext for the specified logic.
        /// If successful, the method returns an information code of itDone.
        /// If not successful, an event info object with pertinent error
        /// information is returned.
        /// </summary>
        /// <param name="SourceLogic"></param>
        /// <returns></returns>
        internal static TWinAGIEventInfo CompileLogic(Logic SourceLogic) {
            // if a major error occurs, nothing is passed in tmpInfo
            // note that when minor errors are returned, line is adjusted because
            //editor rows(lines) start at '1', but the compiler starts at line '0'
            bool blnCompiled;
            List<string> stlSource;
            int lngCodeSize;

            if (!compGame.GlobalDefines.IsSet) {
                compGame.GlobalDefines.LoadGlobalDefines(compGame.agGameDir + "globals.txt");
            }
            if (!blnSetIDs) {
                SetResourceIDs(compGame);
            }
            // always reset command name assignments
            CorrectCommands(compGame.agIntVersion);

            //refresh  values for game specific reserved defines
            compGame.agResGameDef[0].Value = "\"" + compGame.agGameID + "\"";
            compGame.agResGameDef[1].Value = "\"" + compGame.agGameVersion + "\"";
            compGame.agResGameDef[2].Value = "\"" + compGame.agGameAbout + "\"";
            compGame.agResGameDef[3].Value = (compGame.InvObjects.Count).ToString();

            // get source text by lines as a list of strings
            stlSource = SplitLines(SourceLogic.SourceText);
            bytLogComp = SourceLogic.Number;
            strLogCompID = SourceLogic.ID;
            // setup error info
            blnCriticalError = false;
            blnMinorError = false;
            errInfo.ResType = AGIResType.rtLogic;
            errInfo.ResNum = SourceLogic.Number;
            errInfo.Line = "--";
            errInfo.Module = SourceLogic.ID;
            intCtlCount = 0;
            // reset the combined input list
            stlInput = [];
            intFileCount = 0;
            // add includes
            if (!AddIncludes(stlSource)) {
                return errInfo;
            }
            // remove any blank lines from end
            while (stlInput[^1].Length == 0 && stlInput.Count > 0) {
                stlInput.RemoveAt(stlInput.Count - 1);
            }
            // if nothing to compile, return an error
            if (stlInput.Count == 0) {
                errInfo.ID = "4159";
                errInfo.Text = LoadResString(4159);
                errInfo.Line = "--";
                return errInfo;
            }
            // strip out all comments
            if (!RemoveComments()) {
                return errInfo;
            }
            // check for labels
            ReadLabels();
            // enumerate and replace all the defines
            if (!ReadDefines()) {
                return errInfo;
            }
            // read predefined messages
            if (!ReadMsgs()) {
                return errInfo;
            }
            tmpLogRes = new Logic {
                Data = []
            };
            // write a place holder 2-byte (word) value for offset to msg section start
            tmpLogRes.WriteWord(0, 0);
            // main agi compiler
            blnCompiled = CompileAGI();
            if (!blnCompiled) {
                tmpLogRes.Unload();
                return errInfo;
            }
            // code size equals bytes currently written (before msg secion added)
            lngCodeSize = tmpLogRes.Size;
            // add message section
            if (!WriteMsgs()) {
                tmpLogRes.Unload();
                return errInfo;
            }
            if (!blnMinorError) {
                // no errors, save comiled data
                SourceLogic.Data = tmpLogRes.Data;
                SourceLogic.CompiledCRC = SourceLogic.CRC;
                compGame.WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CRC32", "0x" + SourceLogic.CRC.ToString("x8"), "Logics");
                compGame.WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CompCRC32", "0x" + SourceLogic.CompiledCRC.ToString("x8"));
            }
            // done with the temp resource
            tmpLogRes.Unload();

            // if minor errors, report them as a compile fail
            if (blnMinorError) {
                errInfo.ID = "4001";
                errInfo.Text = LoadResString(4001);
                errInfo.Line = "--";
            }
            else {
                errInfo.Type = EventType.etInfo;
                errInfo.InfoType = EInfoType.itDone;
            }
            return errInfo;
        }

        /// <summary>
        /// This method builds an array of resourceIDs so ConvertArg function
        /// can iterate through them much quicker.
        /// </summary>
        /// <param name="game"></param>
        internal static void SetResourceIDs(AGIGame game) {
            if (blnSetIDs) {
                return;
            }
            strLogID = new string[255];
            strPicID = new string[255];
            strSndID = new string[255];
            strViewID = new string[255];
            foreach (Logic tmpLog in game.agLogs) {
                strLogID[tmpLog.Number] = tmpLog.ID;
            }
            foreach (Picture tmpPic in game.agPics) {
                strPicID[tmpPic.Number] = tmpPic.ID;
            }
            foreach (Sound tmpSnd in game.agSnds) {
                strSndID[tmpSnd.Number] = tmpSnd.ID;
            }
            foreach (View tmpView in game.agViews) {
                strViewID[tmpView.Number] = tmpView.ID;
            }
            blnSetIDs = true;
        }

        /// <summary>
        /// This method converts an unkonwn token read from the input into the
        /// appropriate argument token. If the conversion fails, the method 
        /// returns false.
        /// 
        /// </summary>
        /// <param name="strArgIn"></param>
        /// <param name="ArgType"></param>
        /// <returns>true if convertion successful, false if not.</returns>
        internal static bool ConvertArgument(ref string strArgIn, ArgTypeEnum ArgType) {
            bool nullval = false;
            return ConvertArgument(ref strArgIn, ArgType, ref nullval);
        }

        /// <summary>
        /// This method converts an unkonwn token read from the input into the
        /// appropriate argument token. If the token can validly represent a 
        /// number or a variable, pss the input variable varOrnum as true; if
        /// a valid argument is found, varOrnum is updated to indicate which
        /// type was found. If the conversion fails, the method returns false.
        /// </summary>
        /// <param name="checktoken"></param>
        /// <param name="argtype"></param>
        /// <param name="varOrnum"></param>
        /// <returns>true if the token is valid (checktoken is changed to the correct
        /// token, varOrnum adjusted accordingly); false if the token is invalid</returns>
        internal static bool ConvertArgument(ref string checktoken, ArgTypeEnum argtype, ref bool varOrnum) {
            // make sure varOrnum gets passed as false, unless explicitly
            // needed to be true!
            // if input is not a valid argument token already
            // (i.e. ##, v##, f##, s##, o##, w##, i##, c##)
            // this function searches resource IDs, local defines, global defines,
            // and reserved names for strArgIn; if found strArgIn is replaced
            // with the Value of the define
            // optional argtype is used to identify words, messages, and inv objects
            // to speed up search

            // NOTE: this does NOT validate the numerical Value of arguments;
            // calling function is responsible to make that check
            // it also does not concatenate strings

            // to support calls from special syntax compilers, need to be able
            // to check for numbers AND variables with one check
            // the varOrnum flag is used to do this; when the flag is
            // true, number searches also return variables

            //the ' & ' operator acts as a 'pointer' and returns the numeric
            //value of 'v##, f##, s##, o##, w##, i##, c##'
            int i;

            if (agSierraSyntax) {
                // vocab words can be any text/string (but not in quotes)
                //all others must be a number
                if (argtype == atVocWrd) {
                    if (checktoken[0] == '\"') {
                        //error
                        AddMinorError(6001, LoadResString(6001));
                    }
                    else {
                        return true;
                    }
                }
                else {
                    // other args are only numbers (or defines)
                    // calling function will evaluate value bounds
                    if (int.TryParse(checktoken, out int intVal)) {
                        // arg is numeric
                        varOrnum = false;
                        checktoken = intVal.ToString();
                        return true;
                    }
                }
            }
            else {
                // AGI FAN syntax:

                // check if already in correct format
                switch (argtype) {
                case atNum:
                    // numeric (int) only; allow all values, including
                    // negative; calling function will evaluate bounds
                    if (int.TryParse(checktoken, out _)) {
                        // arg is numeric
                        varOrnum = false;
                        return true;
                    }
                    if (varOrnum) {
                        // 'v##' is also ok
                        if (checktoken[0] == 'v') {
                            if (int.TryParse(checktoken[1..], out _)) {
                                return true;
                            }
                        }
                    }
                    // check for a pointer
                    if (checktoken[0] == '&') {
                        int ptrVal = PointerValue(checktoken[1..]);
                        if (ptrVal != -1) {
                            checktoken = ptrVal.ToString();
                            // arg is number, not a variable
                            varOrnum = false;
                            return true;
                        }
                    }
                    break;

                case atVar:
                    if (checktoken[0] == 'v') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case atFlag:
                    if (checktoken[0] == 'f') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case atCtrl:
                    if (checktoken[0] == 'c') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case atSObj:
                    if (checktoken[0] == 'o') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case atStr:
                    //if first char matches
                    if (checktoken[0] == 's') {
                        //if this arg returns a valid Value
                        if (int.TryParse(checktoken[1..], out _)) {
                            //ok
                            return true;
                        }
                    }
                    break;
                case atWord:
                    // NOTE: this is NOT vocab word; this is word arg type
                    // (used in command word.to.string)
                    if (checktoken[0] == 'w') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case atMsg:
                    switch (checktoken[0]) {
                    case 'm':
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                        break;
                    case '\"':
                        // strings (in quotes) are also ok
                        return true;
                    }
                    break;
                case atInvItem:
                    // if first char matches, or is a quote
                    switch (checktoken[0]) {
                    case 'i':
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                        break;
                    case '\"':
                        //strings (in quotes) are also ok
                        return true;
                    }
                    break;
                case atVocWrd:
                    //can be number or string in quotes
                    if (int.TryParse(checktoken, out _) || checktoken[0] == '\"') {
                        return true;
                    }
                    break;
                }
            }
            // the argument is not in correct format; must be reserved name,
            // global or local define, or an error

            // first, check against local defines
            for (i = 0; i < lngDefineCount; i++) {
                if (checktoken == tdDefines[i].Name) {
                    if (tdDefines[i].Type == argtype) {
                        // return the value
                        checktoken = tdDefines[i].Value;
                        if (argtype == atNum) {
                            // argument is numeric
                            varOrnum = false;
                        }
                        return true;
                    }
                    // special case - looking for number, but var also OK
                    if (varOrnum && tdDefines[i].Type == atVar) {
                        checktoken = tdDefines[i].Value;
                        return true;
                    }
                    // special case - message or item can be a string in quotes
                    if (argtype == atMsg || argtype == atInvItem) {
                        if (tdDefines[i].Type == atDefStr) {
                            checktoken = tdDefines[i].Value;
                            return true;
                        }
                    }
                    // special case - vocab words are numbers or strings
                    if (argtype == atVocWrd && (tdDefines[i].Type == atNum || tdDefines[i].Type == atDefStr)) {
                        checktoken = tdDefines[i].Value;
                        return true;
                    }
                    // the define value is not valid
                    return false;
                }
            }

            //if not sierra syntax check global defines, ResIDs and reserved
            if (!agSierraSyntax) {
                for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                    if (checktoken == compGame.GlobalDefines[i].Name) {
                        if (compGame.GlobalDefines[i].Type == argtype) {
                            checktoken = compGame.GlobalDefines[i].Value;
                            if (argtype == atNum) {
                                // argument is numeric
                                varOrnum = false;
                            }
                            // ok
                            return true;
                        }
                        // special case - looking for number, but var also OK
                        if (varOrnum && compGame.GlobalDefines[i].Type == atVar) {
                            checktoken = compGame.GlobalDefines[i].Value;
                            return true;
                        }
                        // special case - message, item can be a string in quotes
                        if (argtype == atMsg || argtype == atInvItem) {
                            if (compGame.GlobalDefines[i].Type == atDefStr) {
                                checktoken = compGame.GlobalDefines[i].Value;
                                return true;
                            }
                        }
                        // special case - vocab words are numbers or strings
                        if (argtype == atVocWrd && (compGame.GlobalDefines[i].Type == atNum || compGame.GlobalDefines[i].Type == atDefStr)) {
                            checktoken = compGame.GlobalDefines[i].Value;
                            return true;
                        }
                        // the define value is not valid
                        return false;
                    }
                }
                // check numbers against list of resource IDs (can only be
                // numeric)
                if (argtype == atNum) {
                    for (i = 0; i <= 255; i++) {
                        if (checktoken == strLogID[i]) {
                            checktoken = i.ToString();
                            varOrnum = false;
                            return true;
                        }
                        if (checktoken == strPicID[i]) {
                            checktoken = i.ToString();
                            varOrnum = false;
                            return true;
                        }
                        if (checktoken == strSndID[i]) {
                            checktoken = i.ToString();
                            varOrnum = false;
                            return true;
                        }
                        if (checktoken == strViewID[i]) {
                            checktoken = i.ToString();
                            varOrnum = false;
                            return true;
                        }
                    }
                }
                // lastly, check reserved names if they are being used
                if (UseReservedNames) {
                    switch (argtype) {
                    case atNum:
                        for (i = 0; i <= 4; i++) {
                            if (checktoken == agEdgeCodes[i].Name) {
                                checktoken = agEdgeCodes[i].Value;
                                // argument is numeric
                                varOrnum = false;
                                return true;
                            }
                        }
                        for (i = 0; i <= 8; i++) {
                            if (checktoken == agEgoDir[i].Name) {
                                checktoken = agEgoDir[i].Value;
                                // argument is numeric
                                varOrnum = false;
                                return true;
                            }
                        }
                        for (i = 0; i <= 4; i++) {
                            if (checktoken == agVideoMode[i].Name) {
                                checktoken = agVideoMode[i].Value;
                                // argument is numeric
                                varOrnum = false;
                                return true;
                            }
                        }
                        for (i = 0; i <= 8; i++) {
                            if (checktoken == agCompType[i].Name) {
                                checktoken = agCompType[i].Value;
                                // argument is numeric
                                varOrnum = false;
                                return true;
                            }
                        }
                        for (i = 0; i <= 15; i++) {
                            if (checktoken == agResColor[i].Name) {
                                checktoken = agResColor[i].Value;
                                // argument is numeric
                                varOrnum = false;
                                return true;
                            }
                        }
                        //check against invobj Count
                        if (checktoken == compGame.agResGameDef[3].Name) {
                            checktoken = compGame.agResGameDef[3].Value;
                            //argument is numeric
                            varOrnum = false;
                            return true;
                        }
                        // if looking for numbers OR variables
                        if (varOrnum) {
                            // check against builtin variables as well
                            for (i = 0; i <= 26; i++) {
                                if (checktoken == agResVar[i].Name) {
                                    checktoken = agResVar[i].Value;
                                    return true;
                                }
                            }
                        }
                        break;
                    case atVar:
                        for (i = 0; i <= 26; i++) {
                            if (checktoken == agResVar[i].Name) {
                                checktoken = agResVar[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atFlag:
                        for (i = 0; i <= 17; i++) {
                            if (checktoken == agResFlag[i].Name) {
                                checktoken = agResFlag[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atMsg:
                        for (i = 1; i <= 2; i++) {
                            if (checktoken == compGame.agResGameDef[i].Name) {
                                checktoken = compGame.agResGameDef[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atSObj:
                        if (checktoken == agResObj[0].Name) {
                            checktoken = agResObj[0].Value;
                            return true;
                        }
                        break;
                    case atStr:
                        if (checktoken == agResStr[0].Name) {
                            checktoken = agResStr[0].Value;
                            return true;
                        }
                        break;
                    }
                }
            }
            // argument is not valid
            return false;
        }

        /// <summary>
        /// This method gets the numeric value of a non-numeric argument (var, flag,
        /// etc). If strArg is numeric, not a valid argument marker, or not a valid
        /// define value, return value is set to -1; if valid, the arg value is
        /// returned.
        /// </summary>
        /// <param name="strArgIn"></param>
        /// <returns></returns>
        private static int PointerValue(string strArgIn) {
            //if a number, return error
            if (int.TryParse(strArgIn, out _)) {
                return -1;
            }
            switch (strArgIn[0]) {
            // check for standard arg types
            case 'v' or 'f' or 'c' or 'o' or 's' or 'w' or 'm' or 'i':
                int varVal = VariableValue(strArgIn);
                //if found, return it
                if (varVal >= 0) {
                    return varVal;
                }
                break;
            }
            // arg is not an AGI argument marker; must be reserved name,
            // global or local define, or an error

            // first, check against local defines
            for (int i = 0; i < lngDefineCount; i++) {
                if (strArgIn == tdDefines[i].Name) {
                    // numbers are not valid
                    if (int.TryParse(tdDefines[i].Value, out _)) {
                        return -1;
                    }
                    switch (tdDefines[i].Value[0]) {
                    // check for standard arg types
                    case 'v' or 'f' or 'c' or 'o' or 's' or 'w' or 'm' or 'i':
                        int varVal = VariableValue(tdDefines[i].Value);
                        if (varVal >= 0) {
                            return varVal;
                        }
                        else {
                            return -1;
                        }
                    default:
                        // not valid
                        return -1;
                    }
                }
            }
            // second, check against global defines
            for (int i = 0; i < compGame.GlobalDefines.Count; i++) {
                if (strArgIn == compGame.GlobalDefines[i].Name) {
                    // numbers are not valid
                    if (int.TryParse(compGame.GlobalDefines[i].Value, out _)) {
                        return -1;
                    }
                    switch (tdDefines[i].Value[0]) {
                    // check for standard arg types
                    case 'v' or 'f' or 'c' or 'o' or 's' or 'w' or 'm' or 'i':
                        int varVal = VariableValue(compGame.GlobalDefines[i].Value);
                        if (varVal >= 0) {
                            return varVal;
                        }
                        else {
                            return -1;
                        }
                    default:
                        // not valid
                        return -1;
                    }
                }
            }
            // lastly, check reserved names, if they are being used
            if (UseReservedNames) {
                for (int i = 0; i <= 26; i++) {
                    if (strArgIn == agResVar[i].Name) {
                        return i;
                    }
                }
            }
            for (int i = 0; i <= 17; i++) {
                if (strArgIn == agResFlag[i].Name) {
                    return i;
                }
            }
            // check for o0, s0
            if (strArgIn == agResObj[0].Name || strArgIn == agResStr[0].Name) {
                return 0;
            }
            //if not found or error, return false
            return -1;
        }

        /// <summary>
        /// Reports errors that don't require canceling back to the calling object.
        /// The compile still fails, but it will continue working through code,
        /// possibly finding other errors and warnings. This approach allows the
        /// user to see all minor errors for a logic after a single pass of the 
        /// compiler, instead of finding them one at a time.
        /// </summary>
        /// <param name="ErrorNum"></param>
        /// <param name="ErrorText"></param>
        static void AddMinorError(int ErrorNum, string ErrorText = "") {
            //if no text passed, use the default resource string
            if (ErrorText.Length == 0) {
                ErrorText = LoadResString(ErrorNum);
            }
            errInfo.ID = ErrorNum.ToString();
            errInfo.Text = ErrorText;
            // module and line are updated continuously so they
            // don't need to be refreshed here
            compGame.OnCompileLogicStatus(errInfo);
            blnMinorError = true;
        }

        /// <summary>
        /// This method extracts an argument number from an argument token. For 
        /// example, it converts 'v##' into just '##'.
        /// </summary>
        /// <param name="argtoken"></param>
        /// <returns>The argument number if successful. -1 if unsuccessful.</returns>
        static internal int VariableValue(string argtoken) {
            // the token should be of the form #, a#
            // where a is a valid variable prefix (v, f, s, m, w, c)
            // and # is 0-255
            // if the result is invalid, this function returns -1
            string strVarVal;
            if (!int.TryParse(argtoken, out _)) {
                // strip off variable prefix
                strVarVal = argtoken[1..];
            }
            else {
                // start with argtoken value
                strVarVal = argtoken;
            }
            if (int.TryParse(strVarVal, out int intVarVal)) {
                // for word only, subtract one to
                // account for '1' based word data type
                // (i.e. w1 is first word, but command uses arg Value of '0')
                if (argtoken[0] == 'w') {
                    intVarVal--;
                }
                // verify within bounds  0-255
                if (intVarVal >= 0 && intVarVal <= 255) {
                    return intVarVal;
                }
            }
            // any other result is invalid 
            return -1;
        }

        /// <summary>
        /// This method passes through the inpuyt logic and replaces all include statements
        /// with the contents of the specified include file.
        /// </summary>
        /// <param name="stlLogicText"></param>
        /// <returns>true if include files added successfully. false if unable to 
        /// add include files.</returns>
        internal static bool AddIncludes(List<string> stlLogicText) {
            // include file lines are given a marker to identify them as belonging to
            // an include file
            string strLineText;

            // begin with empty array of source lines
            stlInput = [];
            for (lngLine = 0; lngLine < stlLogicText.Count; lngLine++) {
                // cache error line
                errInfo.Line = (lngLine + 1).ToString();
                if (agSierraSyntax) {
                    // get next line, convert tabs to spaces  but don't
                    // trim whitespace from start of line
                    strLineText = stlLogicText[lngLine].Replace('\t', ' ');
                }
                else {
                    // get next line, minus tabs and spaces
                    strLineText = stlLogicText[lngLine].Replace('\t', ' ').TrimStart();
                }
                if (strLineText[..2] == INCLUDE_MARK) {
                    // check for any instances of the marker, since these will
                    // interfere with include line handling ! SHOULD NEVER
                    // HAPPEN, but just in case
                    errInfo.ID = "4069";
                    errInfo.Text = LoadResString(4069);
                    return false;
                }
                // check this line for include statement, and insert if found
                switch (CheckInclude(strLineText)) {
                case 0:
                    // not an include line; add it to the input
                    stlInput.Add(strLineText);
                    break;
                case 1:
                    // include file inserted
                    // add a blank line as a place holder for the 'include' line
                    // (to keep line counts accurate when calculating line number
                    // for errors)
                    stlInput.Add("");
                    break;
                default: // -1 = error
                    return false;
                }
                lngLine++;
            }
            // success
            return true;
        }

        /// <summary>
        /// This method inserts the contents of the include file from the specified
        /// line into the source input line array.
        /// </summary>
        /// <param name="strLineText"></param>
        /// <returns>1 if include file added successfully<br />
        /// 0 if line does not contain an include file<br />
        /// -1 if an error is encountered</returns>
        static int CheckInclude(string strLineText) {
            List<string> IncludeLines;
            string strIncludeFilename, strIncludeText;
            int intIncludeNum, CurIncludeLine;
            int retval, i;

            if (strLineText[..8] != "#include" && (strLineText[..8] != "%include" || !agSierraSyntax)) {
                // not an include line
                return 0;
            }
            // check for a missing filename
            if (strLineText.Trim().Length == 8) {
                blnCriticalError = true;
                errInfo.ID = "4060";
                errInfo.Text = LoadResString(4060);
                return -1;
            }
            // proper format requires a space after include mark
            if (strLineText[8] != ' ') {
                //generate critical error
                errInfo.ID = "4103";
                errInfo.Text = LoadResString(4103);
                return -1;
            }
            // build include filename
            strIncludeFilename = strLineText[9..].Trim();
            //check for a missing filename
            if (strIncludeFilename.Length == 0) {
                errInfo.ID = "4060";
                errInfo.Text = LoadResString(4060);
                return -1;
            }
            // check for single quote
            if (strIncludeFilename == "\"") {
                blnCriticalError = true;
                errInfo.ID = "4060";
                errInfo.Text = LoadResString(4060);
                return -1;
            }
            // check for correct quotes used 
            if (strIncludeFilename[0] != QUOTECHAR || strIncludeFilename[^1] != QUOTECHAR) {
                switch (ErrorLevel) {
                case leHigh:
                    blnCriticalError = true;
                    errInfo.ID = "4059";
                    errInfo.Text = LoadResString(4059);
                    return -1;
                case leMedium or leLow:
                    AddWarning(5028, LoadResString(5028).Replace(ARG1, strIncludeFilename));
                    break;
                }
            }
            // strip off quotes
            if (strIncludeFilename[0] == QUOTECHAR) {
                strIncludeFilename = strIncludeFilename[1..];
            }
            if (strIncludeFilename[0] == QUOTECHAR) {
                strIncludeFilename = strIncludeFilename[..^1];
            }
            strIncludeFilename = strIncludeFilename.Trim();
            if (strIncludeFilename.Length == 0) {
                blnCriticalError = true;
                errInfo.ID = "4060";
                errInfo.Text = LoadResString(4060);
                return -1;
            }
            // check for path
            if (JustPath(strIncludeFilename, true).Length == 0) {
                //use resource dir for this include file
                strIncludeFilename = compGame.agResDir + strIncludeFilename;
            }
            // convert filename to absolute
            try {
                strIncludeFilename = FullFileName(compGame.agResDir, strIncludeFilename);
            }
            catch {
                errInfo.ID = "4050";
                errInfo.Text = LoadResString(4055).Replace(ARG1, strIncludeFilename);
                return -1;
            }
            // now verify file exists
            if (!File.Exists(strIncludeFilename)) {
                errInfo.ID = "4050";
                errInfo.Text = LoadResString(4050).Replace(ARG1, strIncludeFilename);
                return -1;
            }
            // check all loaded includes
            for (i = 1; i <= intFileCount; i++) {
                if (strIncludeFilename == strIncludeFile[i]) {
                    // if the include file has already been added, don't add it again
                    return 1;
                }
            }
            // now open the include file, and get the text
            try {
                strIncludeText = compGame.agCodePage.GetString(File.ReadAllBytes(strIncludeFilename));
            }
            catch (Exception) {
                errInfo.ID = "4055";
                errInfo.Text = LoadResString(4055).Replace(ARG1, strIncludeFilename);
                return -1;
            }
            IncludeLines = SplitLines(strIncludeText);
            if (IncludeLines.Count > 0) {
                // save file name to allow for error checking
                Array.Resize(ref strIncludeFile, intFileCount);
                strIncludeFile[intFileCount] = strIncludeFilename;
                intIncludeNum = intFileCount;
                intFileCount++;
                // add all these lines into this position
                for (CurIncludeLine = 0; CurIncludeLine < IncludeLines.Count; CurIncludeLine++) {
                    if (agSierraSyntax) {
                        // get next line, convert tabs to spaces, but don't trim
                        // leading whitespace
                        strIncludeText = IncludeLines[CurIncludeLine].Replace('\t', ' ');
                    }
                    else {
                        // get next line, minus tabs and spaces
                        strIncludeText = IncludeLines[CurIncludeLine].Replace('\t', ' ').Trim();
                    }
                    // check for any instances of the marker, since these will
                    // interfere with include line handling
                    if (IncludeLines[CurIncludeLine].Trim()[..2] == INCLUDE_MARK) {
                        errInfo.ID = "4069";
                        errInfo.Text = LoadResString(4069);
                        errInfo.Line = (CurIncludeLine + 1).ToString();
                        return -1;
                    }
                    // check for nested include files
                    retval = CheckInclude(strIncludeText);
                    switch (retval) {
                    case 0:
                        // not an embedded include
                        // include filenumber and line number for this includefile
                        stlInput.Add(INCLUDE_MARK + intIncludeNum.ToString() + ":" + CurIncludeLine.ToString() + "#" + strIncludeText);
                        break;
                    case 1:
                        // include lines added
                        // do nothing (don't need a blank line; only one blank line
                        // is needed for each 'root' include file)
                        break;
                    case -1:
                        //pass error along
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
        /// <param name="ArgType"></param>
        /// <returns></returns>
        internal static string ArgTypeName(ArgTypeEnum ArgType) {
            switch (ArgType) {
            case atNum:
                return "number";
            case atVar:
                return "variable";
            case atFlag:
                return "flag";
            case atMsg:
                return "message";
            case atSObj:
                return "screen object";
            case atInvItem:
                return "inventory item";
            case atStr:
                return "string";
            case atWord:
                return "word";
            case atCtrl:
                return "controller";
            case atDefStr:
                return "text in quotes";
            case atVocWrd:
                return "vocabulary word";
            default:
                // not possible; it will always be one of the above
                // but C# compilier requires default handler
                return "";
            }
        }

        /// <summary>
        /// This method checks for reserved flag (f0-f20) use and adds warnings
        /// as appropriate.
        /// </summary>
        /// <param name="ArgVal"></param>
        internal static void CheckResFlagUse(byte ArgVal) {
            if (ErrorLevel == leLow) {
                return;
            }
            if (ArgVal == 2 ||
                ArgVal == 4 ||
                (ArgVal >= 7 && ArgVal <= 10) ||
                ArgVal >= 13) {
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
                // all other reserved variables should be read only
                AddWarning(5025, LoadResString(5025).Replace(ARG1, agResFlag[ArgVal].Name));
            }
        }

        /// <summary>
        /// This method checks for reserved variable (v0-v26) use and adds warnings
        /// as appropriate.
        /// </summary>
        /// <param name="ArgNum"></param>
        /// <param name="ArgVal"></param>
        static internal void CheckResVarUse(byte ArgNum, byte ArgVal) {
            if (ErrorLevel == leLow) {
                return;
            }
            switch (ArgNum) {
            case 3 or 7 or 15 or 21 or >= 27:
                //    v3: curent score
                //    v7: max score
                //    v15: joystick sensitivity
                //    v21: msg window delay time
                //    >=v27: non-reserved
                // no restrictions
                break;
            case 6:
                //ego direction
                // should be restricted to values 0-8
                if (ArgVal > 8) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, agResVar[6].Name).Replace(ARG2, "8"));
                }
                break;
            case 10:
                // cycle delay time
                // large values highly unusual
                if (ArgVal > 20) {
                    AddWarning(5055);
                }
                break;
            case 17 or 18:
                // error value, and error info
                // resetting to zero is usually a good thing; other values don't make sense
                if (ArgVal > 0) {
                    AddWarning(5092, LoadResString(5092).Replace(ARG1, agResVar[ArgNum].Name));
                }
                break;
            case 19: 
                // key_pressed value
                // ok if resetting for key input
                if (ArgVal > 0) {
                    AddWarning(5017, LoadResString(5017).Replace(ARG1, agResVar[ArgNum].Name));
                }
                break;
            case 23: 
                // sound attenuation
                // restrict to 0-15
                if (ArgVal > 15) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, agResVar[23].Name).Replace(ARG2, "15"));
                }
                break;
            case 24:
                // max input length
                if (ArgVal > 39) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, agResVar[24].Name).Replace(ARG2, "39"));
                }
                break;
            default: 
                // all other reserved variables should be read only
                AddWarning(5017, LoadResString(5017).Replace(ARG1, agResVar[ArgNum].Name));
                break;
            }
        }

        /// <summary>
        /// Gets the next argument token for the command that the compiler is 
        /// currently processing and validates it. The expected argument type
        /// and argument position are used to validate the token.
        /// </summary>
        /// <param name="ArgType"></param>
        /// <param name="ArgPos"></param>
        /// <returns></returns>
        static internal int GetNextArg(ArgTypeEnum ArgType, int ArgPos) {
            // used when not looking for a number/variable combo)
            bool nullval = false;
            return GetNextArg(ArgType, ArgPos, ref nullval);
        }

        /// <summary>
        /// Gets the next argument token for the command that the compiler is
        /// currently processing and validates it. The expected argument type
        /// and argument position are used to validate the token. If processing
        /// a special syntax statement, use the varOrnum flag to indicate if
        /// the argument can be either a variable or a number to be valid.
        /// </summary>
        /// <param name="argtype"></param>
        /// <param name="argpos"></param>
        /// <param name="varOrnum"></param>
        /// <returns>if successful, the function returns the Value of the argument<br />
        /// if unsuccessful, the function returns a negative value:<br />
        ///  -1 = invalid conversion<br />
        ///  -2 = ')' encountered<br />
        ///  -3 = ',' encountered
        ///</returns>
        static internal int GetNextArg(ArgTypeEnum argtype, int argpos, ref bool varOrnum) {
            //  - multline message/string/inv.item/word strings are recombined,
            //    and checked for validity
            //
            // - special syntax compilers look for variables OR strings
            //
            // - when argtype is atNum and varOrnum set, numbers OR variables
            //   return true; if a number is found, the flag is reset to false,
            //   if a variable is found, it is left true
            string strArg;
            int lngArg = 0;
            int i, retval;

            strArg = NextToken();
            if (!ConvertArgument(ref strArg, argtype, ref varOrnum)) {
                // error while trying to convert argument token to correct
                // type - if a closing paren or comma found, it means one or
                // more args missing
                if (strArg == ")" || strArg == ",") {
                    if (strArg == ")") {
                        retval = -2;
                    }
                    else {
                        retval = -3;
                    }
                    errInfo.ID = "4054";
                    errInfo.Text = LoadResString(4054).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG3, ArgTypeName(argtype));
                    // backup so comma/bracket can be retrieved
                    lngPos--;
                }
                else {
                    // invalid conversion
                    errInfo.ID = "4063";
                    errInfo.Text = LoadResString(4063).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, ArgTypeName(argtype)).Replace(ARG3, strArg);
                    retval = -1;
                }
                return retval;
            }

            switch (argtype) {
            case atNum:
                // check for negative number
                if (Val(strArg) < 0) {
                    //valid negative numbers are -1 to -128
                    if (Val(strArg) < -128) {
                        AddMinorError(4157);
                        // use dummy value to continue
                        strArg = "1";
                    }
                    else {
                        //convert it to 2s-compliment unsigned value by adding it to 256
                        strArg = (256 + Val(strArg)).ToString();
                        switch (ErrorLevel) {
                        case leHigh or leMedium:
                            AddWarning(5098);
                            break;
                        }
                    }
                }
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                }
                break;
            case atVar or atFlag:
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                }
                break;
            case atCtrl:
                // controllers should be  0 - 49
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    switch (ErrorLevel) {
                    case leHigh:
                        AddMinorError(4136, LoadResString(4136).Replace(ARG1, (argpos + 1).ToString()));
                        break;
                    case leMedium or leLow:
                        AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                        break;
                    }
                    return -1;
                }
                else {
                    if (lngArg > 49) {
                        switch (ErrorLevel) {
                        case leHigh:
                            AddMinorError(4136, LoadResString(4136).Replace(ARG1, (argpos + 1).ToString()));
                            break;
                        case leMedium:
                            AddWarning(5060);
                            break;
                        }
                    }
                }
                break;
            case atSObj:
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                }
                // check against max screen object Value
                if (lngArg > compGame.InvObjects.MaxScreenObjects) {
                    switch (ErrorLevel) {
                    case leHigh:
                        AddMinorError(4119, LoadResString(4119).Replace(ARG1, (compGame.InvObjects.MaxScreenObjects).ToString()));
                        break;
                    case leMedium:
                        AddWarning(5006, LoadResString(5006).Replace(ARG1, compGame.InvObjects.MaxScreenObjects.ToString()));
                        break;
                    }
                }
                break;
            case atStr:
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    switch (ErrorLevel) {
                    case leHigh:
                        // for version 2.089, 2.272, and 3.002149 only 12 strings
                        switch (compGame.agIntVersion) {
                        case "2.089" or "2.272" or "3.002149":
                            AddMinorError(4079, LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "11"));
                            break;
                        default:
                            AddMinorError(4079, LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "23"));
                            break;
                        }
                        break;
                    case leMedium or leLow:
                        AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                        break;
                    }
                }
                else {
                    // if outside expected bounds (strings should be limited to 0-23)
                    if ((lngArg > 23) || (lngArg > 11 && (compGame.agIntVersion == "2.089" || compGame.agIntVersion == "2.272" || compGame.agIntVersion == "3.002149"))) {
                        switch (ErrorLevel) {
                        case leHigh:
                            // for version 2.089, 2.272, and 3.002149 only 12 strings
                            switch (compGame.agIntVersion) {
                            case "2.089" or "2.272" or "3.002149":
                                AddMinorError(4079, LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "11"));
                                break;
                            default:
                                errInfo.ID = "4079";
                                errInfo.Text = LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "23");
                                break;
                            }
                            break;
                        case leMedium:
                            switch (compGame.agIntVersion) {
                            case "2.089" or "2.272" or "3.002149":
                                AddWarning(5007, LoadResString(5007).Replace(ARG1, "11"));
                                break;
                            default:
                                AddWarning(5007, LoadResString(5007).Replace(ARG1, "23"));
                                break;
                            }
                            break;
                        }
                    }
                }
                break;
            case atWord:
                // word type is NOT words from word.tok
                // word args should be limited to 0-9)
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    switch (ErrorLevel) {
                    case leHigh:
                        AddMinorError(4090, LoadResString(4090).Replace(ARG1, (argpos + 1).ToString()));
                        break;
                    case leMedium or leLow:
                        AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                        break;
                    }
                }
                else {
                    if (lngArg > 9) {
                        switch (ErrorLevel) {
                        case leHigh:
                            AddMinorError(4090, LoadResString(4090).Replace(ARG1, (argpos + 1).ToString()));
                            break;
                        case leMedium:
                            AddWarning(5008);
                            break;
                        }
                    }
                }
                break;
            case atMsg:
                // arg token is either m## or "msg" [or ## for sierra syntax]
                if (agSierraSyntax) {
                    lngArg = VariableValue(strArg);
                }
                else {
                    switch (strArg[0]) {
                    case 'm':
                        lngArg = VariableValue(strArg);
                        if (lngArg == -1) {
                            AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                        }
                        break;
                    case '\"':
                        // concatenate, if applicable
                        strArg = ConcatArg(strArg);
                        // strip off quotes
                        strArg = Mid(strArg, 2, strArg.Length - 2);
                        // convert to msg number
                        lngArg = MessageNum(strArg);
                        // if unallowed characters found, error was raised; exit
                        if (lngArg == -1) {
                            return -1;
                        }
                        // if too many messages
                        if (lngArg == 0) {
                            AddMinorError(4092);
                            return -1;
                        }
                        break;
                    }
                }
                // m0 is not allowed
                if (lngArg == 0) {
                    switch (ErrorLevel) {
                    case leHigh:
                        AddMinorError(4107);
                        // make this a null msg
                        blnMsg[lngArg] = true;
                        strMsg[lngArg] = "";
                        return -1;
                    case leMedium:
                        AddWarning(5091, LoadResString(5091).Replace(ARG1, lngArg.ToString()));
                        // make this a null msg
                        blnMsg[lngArg] = true;
                        strMsg[lngArg] = "";
                        break;
                    }
                }
                // verify msg exists
                if (!blnMsg[lngArg]) {
                    switch (ErrorLevel) {
                    case leHigh:
                        AddMinorError(4113, LoadResString(4113).Replace(ARG1, lngArg.ToString()));
                        //make this a null msg
                        blnMsg[lngArg] = true;
                        strMsg[lngArg] = "";
                        break;
                    case leMedium:
                        AddWarning(5090, LoadResString(5090).Replace(ARG1, lngArg.ToString()));
                        //make this a null msg
                        blnMsg[lngArg] = true;
                        strMsg[lngArg] = "";
                        break;
                    }
                }
                break;
            case atInvItem:
                // only true restriction is can't exceed object count, and
                // can't exceed 255 objects (0-254)
                // i0 is usually a '?', BUT not a strict requirement HOWEVER,
                // WinAGI enforces that i0 MUST be '?', and can't be changed
                // also, if any code tries to access an object by '?', return
                // error
                if (agSierraSyntax) {
                    lngArg = VariableValue(strArg);
                }
                else {
                    switch (strArg[0]) {
                    case 'i':
                        lngArg = VariableValue(strArg);
                        break;
                    case '\"':
                        // concatenate, if applicable
                        strArg = ConcatArg(strArg);
                        lngLine = -1;
                        strArg = strArg[1..^1];
                        // if a quotation mark is part of an object name,
                        // it is coded in the logic as a '\"' not just a '"'
                        // need to ensure all '\"' codes are converted to '"'
                        // otherwise the object would never match
                        strArg = strArg.Replace("\\\"", QUOTECHAR.ToString());
                        for (i = 0; i < compGame.InvObjects.Count; i++) {
                            if (strArg == compGame.InvObjects[(byte)i].ItemName) {
                                lngArg = i;
                                break;
                            }
                        }
                        // if not found,
                        if (i == compGame.InvObjects.Count) {
                            // check for added quotes; they are the problem
                            if (lngQuoteAdded >= 0) {
                                lngLine = lngQuoteAdded;
                                errInfo.Line = (lngLine + 1).ToString();
                            }
                            AddMinorError(4075, LoadResString(4075).Replace(ARG1, (argpos + 1).ToString()));
                            return -1;
                        }
                        // check for valid, but non-unique object (if passed by
                        // text string, it can't be one that is not unique)
                        if (lngArg != -1 && !compGame.InvObjects[(byte)lngArg].Unique) {
                            switch (ErrorLevel) {
                            case leHigh:
                                AddMinorError(4036, LoadResString(4036).Replace(ARG1, (argpos + 1).ToString()));
                                break;
                            case leMedium:
                                AddWarning(5003, LoadResString(5003).Replace(ARG1, (argpos + 1).ToString()));
                                break;
                            }
                        }
                        break;
                    }
                }
                // check for valid match
                if (lngArg == -1) {
                    AddMinorError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()));
                }
                else {
                    if (lngArg >= compGame.InvObjects.Count) {
                        switch (ErrorLevel) {
                        case leHigh:
                            AddMinorError(4112, LoadResString(4112).Replace(ARG1, (argpos + 1).ToString()));
                            break;
                        case leMedium:
                            AddWarning(5005, LoadResString(5005).Replace(ARG1, (argpos + 1).ToString()));
                            break;
                        }
                    }
                    else {
                        // check for question mark
                        if (compGame.InvObjects[(byte)lngArg].ItemName == "?") {
                            switch (ErrorLevel) {
                            case leHigh:
                                errInfo.ID = "4111";
                                errInfo.Text = LoadResString(4111).Replace(ARG1, (argpos + 1).ToString());
                                return -1;
                            case leMedium:
                                AddWarning(5004);
                                break;
                            }
                        }
                    }
                }
                break;
            case atVocWrd:
                // words can be ## or "word" [or no quotes, dollar sign for spaces for
                // Sierra syntax]
                if (IsNumeric(strArg) && !agSierraSyntax) {
                    if (!int.TryParse(strArg, out lngArg)) {
                        // use value of  as placdholder and raise error
                        lngArg = 1;
                        AddMinorError(4162, LoadResString(4162).Replace(ARG1, strArg));
                    }
                    if (lngArg > 65535 || lngArg < 0) {
                        // use value of  as placdholder and raise error
                        lngArg = 1;
                        AddMinorError(4162, LoadResString(4162).Replace(ARG1, strArg));
                    }
                    else {
                        // valldate the group
                        if (!compGame.agVocabWords.GroupExists(lngArg)) {
                            switch (ErrorLevel) {
                            case leHigh:
                                AddMinorError(4114, LoadResString(4114).Replace(ARG1, strArg));
                                break;
                            case leMedium:
                                AddWarning(5019, LoadResString(5019).Replace(ARG1, strArg));
                                break;
                            }
                        }
                    }
                }
                else {
                    if (agSierraSyntax) {
                        strArg = strArg.ToLower().Replace('$', ' ');
                    }
                    else {
                        // no concatenation for vocab words
                        if (strArg[^1] != '\"') {
                            AddMinorError(4114, LoadResString(4114).Replace(ARG1, strArg));
                            // try backing up to where quote is probably missing
                            i = strCurrentLine.LastIndexOf(',');
                            if (i > 0 && i > lngPos - strArg.Length) {
                                lngPos = i - 1;
                            }
                            else {
                                i = strCurrentLine.LastIndexOf(')');
                                if (i > 0 && i > lngPos - strArg.Length) {
                                    lngPos = i - 1;
                                }
                            }
                            // use 'anyword' as placeholder so compiler can continue
                            strArg = "anyword";
                        }
                        else {
                            // strip off starting and ending quotes
                            // (and all words are AGIlower case/case insensitive)
                            strArg =LowerAGI(strArg);
                        }
                    }
                    if (compGame.agVocabWords.WordExists(strArg)) {
                        lngArg = compGame.agVocabWords[strArg].Group;
                    }
                    else {
                        // RARE, but if it's an 'a' or 'i' that isn't defined,
                        // it's word group 0
                        if (strArg == "i" || strArg == "a") {
                            lngArg = 0;
                            // add warning
                            switch (ErrorLevel) {
                            case leHigh or leMedium:
                                AddWarning(5108, LoadResString(5108).Replace(ARG1, strArg));
                                break;
                            }
                        }
                        // "anyword" and "rol" are keywords, even if not explicitly
                        // added to WORDS.TOK
                        else if (strArg == "anyword") {
                            lngArg = 1;
                        }
                        else if (strArg == "rol") {
                            lngArg = 9999;
                        }
                        else {
                            switch (ErrorLevel) {
                            case leHigh:
                                AddMinorError(4114, LoadResString(4114).Replace(ARG1, strArg));
                                break;
                            case leMedium:
                                AddWarning(5019, LoadResString(5019).Replace(ARG1, strArg));
                                break;
                            }
                            // use 1 as placeholder
                            lngArg = 1;
                        }
                    }
                }
                // check for group 0
                if (lngArg == 0) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4035";
                        errInfo.Text = LoadResString(4035).Replace(ARG1, strArg);
                        return -1;
                    case leMedium:
                        AddWarning(5083, LoadResString(5083).Replace(ARG1, strArg));
                        break;
                    }
                }
                break;
            }
            // return the validated arg value
            return lngArg;
        }

        /// <summary>
        /// This method increments the current line of input being processed and
        /// resets all counters, pointers, etc., as well as info needed to support
        /// error locating.
        /// </summary>
        static internal void IncrementLine() {
            // check for end of input
            if (lngLine == -1) {
                return;
            }
            // check for reset 
            if (lngLine == -2) {
                // set it to -1 so line 0 is returned
                lngLine = -1;
            }

            do {
                lngLine++;
                lngPos = 0;
                if (lngLine >= stlInput.Count) {
                    lngLine = -1;
                    return;
                }
                // check for include lines
                if (Left(stlInput[lngLine], 2) == INCLUDE_MARK) {
                    lngIncludeOffset++;
                    // set module
                    int mod = int.Parse(stlInput[lngLine][2..(stlInput[lngLine].IndexOf(':'))]);
                    errInfo.Module = strIncludeFile[mod];
                    // set errline
                    errInfo.Line = stlInput[lngLine][(stlInput[lngLine].IndexOf(':') + 1)..(stlInput[lngLine].IndexOf('#'))];
                    // get the line without include tag
                    strCurrentLine = stlInput[lngLine][(stlInput[lngLine].IndexOf('#') + 1)..];
                }
                else {
                    errInfo.Module = strLogCompID;
                    errInfo.Line = (lngLine - lngIncludeOffset + 1).ToString();
                    // get the next line
                    strCurrentLine = stlInput[lngLine];
                }
            // skip over blank lines
            } while (strCurrentLine.Length == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenval"></param>
        /// <param name="nonewline"></param>
        /// <returns></returns>
        internal static bool CheckToken(string tokenval, bool nonewline = false) {
            // if next token matches tokenval, return true,
            // otherwise return false
            string tmpToken = NextToken(nonewline);
            if (tmpToken == tokenval) {
                return true;
            }
            // backup so this token stays in queue
            lngPos -= tmpToken.Length;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="charval"></param>
        /// <param name="blnNoNewLine"></param>
        /// <returns></returns>
        internal static bool CheckChar(char charval, bool blnNoNewLine = false) {
            //if next char is what we want, it advances, otherwise not
            char testChar;

            testChar = NextChar(blnNoNewLine);
            if (testChar != charval) {
                //no match; need to back up
                //unless nothing was found (meaning end of line or input)
                if (testChar != 0) {
                    lngPos--;
                }
            }
            return testChar == charval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blnNoNewLine"></param>
        /// <returns></returns>
        internal static string PeekToken(bool blnNoNewLine = false) {
            int tmpPos, tmpLine, tmpErrLine, tmpInclOffset;
            string tmpModule, tmpCurLine, peekcmd;

            //save compiler state
            tmpPos = lngPos;
            tmpLine = lngLine;
            tmpInclOffset = lngIncludeOffset;
            tmpModule = errInfo.Module;
            tmpCurLine = strCurrentLine;
            tmpErrLine = int.Parse(errInfo.Line);
            // get next command
            peekcmd = NextToken(blnNoNewLine);
            // restore compiler state
            lngPos = tmpPos;
            lngLine = tmpLine;
            lngIncludeOffset = tmpInclOffset;
            errInfo.Module = tmpModule;
            strCurrentLine = tmpCurLine;
            errInfo.Line = tmpErrLine.ToString();
            // return the peek
            return peekcmd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blnNoNewLine"></param>
        /// <returns></returns>
        internal static char NextChar(bool blnNoNewLine = false) {
            //gets the next non-space character (tabs (ascii code H&9, are converted
            //to a space character, and ignored) from the input stream

            //if the NoNewLine flag is passed,
            //the function will not look past current line for next
            //character; if no character on current line,
            //lngPos is set to end of current line, and
            //empty string is returned

            //if already at end of input (lngLine=-1)
            if (lngLine == -1) {
                //just exit
                return (char)0;
            }
            do {
                //first, increment position
                lngPos++;
                //if past end of this line,
                if (lngPos > strCurrentLine.Length) {
                    //if can't get another line,
                    if (blnNoNewLine) {
                        //move pointer back
                        lngPos--;
                        //return empty char
                        return (char)0;
                    }
                    //get the next line
                    IncrementLine();
                    //if at end of input
                    if (lngLine == -1) {
                        //exit with no character
                        return (char)0;
                    }
                    //increment pointer(so it points to first character of line)
                    lngPos++;
                }
                char retval = strCurrentLine[lngPos];
                //only characters <32 that we need to use are return, and linefeed
                if (retval != 0) {
                    if (retval < 32) {
                        return retval switch {
                            (char)10 or (char)13 => '\n',
                            _ => ' ',
                        };
                    }
                }
                return retval;
            }
            while (true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blnNoNewLine"></param>
        /// <returns></returns>
        static internal string NextToken(bool blnNoNewLine = false) {
            //this function will return the next command, which is comprised
            //of command elements, and separated by element separators
            //command elements include:
            //  characters a-z, A-Z, numbers 0-9, and:  #$%.@_
            //  (and also, all extended characters [128-255])
            //  NOTE: inside quotations, ALL characters, including spaces
            //  are considered command elements
            //
            //element separators include:
            //  space, !"&//()*+,-/:;<=>?[\]^`{|}~
            //
            //element separators other than space are normally returned
            //as a single character command; there are some exceptions
            //where element separators will include additional characters:
            //  !=, &&, *=, ++, +=, --, -=, /=, //, <=, <>, =<, ==, =>, >=, ><, ||
            //
            //when a command starts with a quote, the command returns
            //after a closing quote is found, regardless of characters
            //inbetween the quotes
            //
            //if end of input is reached it returns empty string

            int intChar;
            bool blnInQuotes = false, blnSlash = false;

            //find next non-blank character
            string retval = NextChar(blnNoNewLine).ToString();
            //if at end of input,
            if (lngLine == -1) {
                //return empty string
                return "";
            }
            //if no character returned
            if (retval == "") {
                return "";
            }
            //if command is a element separator:
            if ("'(),:;[\\]^`{}~".Any(retval.Contains)) {
                //return this single character as a command
                return retval.ToString();
            }
            // check for other characters using a switch
            switch (retval[0]) {
            case '\'' or '?':
                //sierra syntax doesn't treat these as a single character command
                if (!agSierraSyntax) {
                    // fanAGI does
                    return retval.ToString();
                }
                break;
            case '=':
                //special case; "=", "==", "=<" and "=>" returned as separate commands
                // (also "=@" for sierra syntax)
                switch (strCurrentLine[lngPos + 1]) {
                case '<' or '>':
                    //increment pointer
                    lngPos++;
                    //swap so we get ">=" and "<=" instead of "=>" and "=<"
                    retval = strCurrentLine[lngPos].ToString() + retval;
                    break;
                case '=':
                    lngPos++;
                    retval = "==";
                    break;
                case '@': // "=@"
                    if (agSierraSyntax) {
                        // rindirect
                        lngPos++;
                        retval = "=@";
                    }
                    break;
                }
                return retval;
            case '\"':
                //special case; quote means start of a string
                blnInQuotes = true;
                break;
            case '+':
                //special case; "+", "++" and "+=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '+') {
                    lngPos++;
                    retval = "++";
                }
                else if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    retval = "+=";
                }
                return retval;
            case '-':
                //special case; "-", "--" and "-=" returned as separate commands
                //also check for negative numbers ("-##")
                if (strCurrentLine[lngPos + 1] == '-') {
                    lngPos++;
                    retval = "--";
                }
                else if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    retval = "-=";
                }
                else if ((byte)strCurrentLine[lngPos + 1] >= 48 && (byte)strCurrentLine[lngPos + 1] <= 57) {
                    //return a negative number
                    while (lngPos + 1 <= strCurrentLine.Length) {
                        intChar = (int)strCurrentLine[lngPos + 1];
                        if (intChar < 48 || intChar > 57) {
                            //anything other than a digit (0-9)
                            break;
                        }
                        else {
                            //add the digit
                            retval += ((char)intChar);
                            lngPos++;
                        }
                    }
                }
                return retval;
            case '!':
                //special case; "!" and "!=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    retval = "!=";
                }
                return retval;
            case '<':
                //special case; "<", "<=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    retval = "<=";
                }
                return retval;
            case '>':
                //special case; ">", ">=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    retval = ">=";
                }
                return retval;
            case '*':
                //special case; "*" and "*=" returned as separate commands;
                if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    retval = "*=";
                }
                // since block commands are no longer supported, check for them
                // in order to provide a meaningful error message
                else if (strCurrentLine[lngPos + 1] == '/') {
                    lngPos++;
                    retval = "*/";
                }
                return retval;
            case '/':
                //special case; "/" , "//" and "/=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    retval = "/=";
                }
                else if (strCurrentLine[lngPos + 1] == '/') {
                    lngPos++;
                    retval = "//";
                }
                // since block commands are no longer supported, check for them
                // in order to provide a meaningful error message
                else if (strCurrentLine[lngPos + 1] == '*') {
                    lngPos++;
                    retval = "/*";
                }
                return retval;
            case '|':
                //special case; "|" and "||" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '|') {
                    lngPos++;
                    retval = "||";
                }
                return retval;
            case '&':
                //special case; "&" and "&&" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '&') {
                    lngPos++;
                    retval = "&&";
                }
                return retval;
            case '@':
                // special case; "@=" returned as separate command
                if (agSierraSyntax) {
                    if (Mid(strCurrentLine, lngPos + 1, 1) == "=") {
                        lngPos++;
                        retval = "@=";
                        return retval;
                    }
                }
                // '@' isn't a single-char token
                break;
            }
            //if not a text string,
            if (!blnInQuotes) {
                //continue adding characters until element separator or EOL is reached
                while (lngPos < strCurrentLine.Length) {
                    char nextChar = strCurrentLine[lngPos + 1];
                    //  space, !"&'() * +,-/:;<=>?[\] ^`{|}~
                    // always marks end of token
                    if (" !\"&'() * +,-/:;<=>?[\\] ^`{|}~".Any(nextChar.ToString().Contains)) {
                        //end of command text found
                        break;
                    }
                    else if ("'/?".Any(nextChar.ToString().Contains)) {
                        if (agSierraSyntax) {
                            // sierra syntax allows these in tokens
                            retval += nextChar;
                            lngPos++;
                        }
                        else {
                            // in fanAGI syntax these also mark end of token
                            break;
                        }
                    }
                    else {
                        //add character
                        retval += nextChar;
                        lngPos++;
                    }
                }
            }
            else {
                // a text string - 
                //if past end of line
                //(which could only happen if a line contains a single double quote on it)
                if (lngPos + 1 > strCurrentLine.Length) {
                    //return the single quote
                    return retval;
                }
                //add characters until another TRUE quote is found
                do {
                    //reset pointer to next
                    char nextchar = strCurrentLine[lngPos + 1];
                    //increment position
                    lngPos++;
                    //if last char was a slash, need to treat this next
                    //character as special
                    if (blnSlash) {
                        //next char is just added as-is;
                        //no checking it
                        //always reset  the slash
                        blnSlash = false;
                    }
                    else {
                        //regular char; check for slash or quote mark
                        if (nextchar == '"') {
                            //a quote marks end of string
                            blnInQuotes = false;
                        }
                        else if (nextchar == '\\') {
                            blnSlash = true;
                        }
                    }
                    retval += nextchar;
                    //if at end of line
                    if (lngPos == strCurrentLine.Length) {
                        //if still in quotes,
                        if (blnInQuotes) {
                            // set inquotes to false to exit the loop the
                            // compiler will deal with missing quote later
                            blnInQuotes = false;
                        }
                    }
                } while (blnInQuotes);
            }
            // return the command
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void CheckForEOL() {
            // normal syntax requires an eol mark;
            // sierra syntax does not, but it's recommended
            string newLine, oldLine;

            //cache error line, in case error drops down
            //one or more lines
            oldLine = errInfo.Line;
            if (!CheckChar(';')) {
                // temporarily set to line where error really is
                newLine = errInfo.Line;
                errInfo.Line = oldLine;
                if (agSierraSyntax) {
                    // warning
                    AddWarning(5111);
                }
                else {
                    // error
                    AddMinorError(4007);
                }
                // restore errline
                errInfo.Line = newLine;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        static internal string ConcatArg(string strText) {
            //this function concatenates strings; it assumes strText
            //is the string that was just read into the compiler;
            //it then checks if there are additional elements of
            //this string to add to it; if so, they are added.
            //it returns the completed string, with lngPos and lngLine
            //set accordingly (if there is nothing to concatenate, it
            //returns original string)
            //NOTE: input string has already been checked for starting
            //and ending quotation marks
            string strTextContinue;
            int lngLastPos, lngLastLine, lngLastErr;
            string strLastLine;
            int lngSlashCount, lngQuotesOK;
            string retval;
            //verify at least two characters
            if (strText.Length < 2) {
                AddMinorError(4081);
                return "\"\"";
            }
            //start with input string
            retval = strText;
            //confirm starting string has ending quote
            if (strText[^1] != '\"') {
                // missing end quote - add it
                retval += "\"";
                switch (ErrorLevel) {
                case leHigh:
                    // error
                    AddMinorError(4080);
                    break;
                case leMedium:
                    // set warning
                    AddWarning(5002);
                    break;
                }
                // note which line had quotes added, in case it results
                // in an error caused by a missing end ') ' or whatever
                // the next required element is
                lngQuoteAdded = lngLine;
                // continue- there may be more lines
            }
            //save current position info
            lngLastPos = lngPos;
            lngLastLine = lngLine;
            strLastLine = strCurrentLine;
            //if at end of last line
            if (lngLastPos == strLastLine.Length) {
                //get next command
                strTextContinue = NextToken();
                //add strings until concatenation is complete
                while (strTextContinue[0] == QUOTECHAR) {
                    //if a continuation string is found, we need to reset
                    //the quote checker
                    lngQuotesOK = 0;
                    // if a single quote (len == 1)
                    if (strTextContinue.Length == 1) {
                        // treat as end of concatenation; main compiler
                        // will catch it as a syntax error
                        break;
                    }
                    //check for end quote
                    if (strTextContinue[^1] != QUOTECHAR) {
                        //bad end quote (set end quote marker, overriding error
                        //that might happen on a previous line)
                        lngQuotesOK = 2;
                    }
                    else {
                        //just because it ends in a quote doesn't mean it's good;
                        //it might be an embedded quote
                        //(we know we have at least two chars, so we don't need
                        //to worry about an error with MID function)

                        //check for an odd number of slashes immediately preceding
                        //this quote
                        lngSlashCount = 0;
                        do {
                            if (retval[^(lngSlashCount + 2)] == '\\') {
                                // if (Mid(retval, retval.Length - (lngSlashCount + 1), 1) == "\\") {
                                lngSlashCount++;
                            }
                            else {
                                break;
                            }
                        } while (retval.Length - (lngSlashCount + 1) >= 0);

                        //if it IS odd, then it's not a valid quote
                        if (lngSlashCount % 2 == 1) {
                            //it's embedded, and doesn't count
                            //bad end quote (set end quote marker, overriding error
                            //that might happen on a previous line)
                            lngQuotesOK = 2;
                        }
                    }
                    //if end quote is missing, deal with it
                    if (lngQuotesOK > 0) {
                        //note which line had quotes added, in case it results
                        //in an error caused by a missing end ')' or whatever
                        //the next required element is
                        lngQuoteAdded = lngLine;
                        switch (ErrorLevel) {
                        case leHigh:
                            //error
                            AddMinorError(4080);
                            return "";
                        case leMedium:
                            //add quote
                            strTextContinue += QUOTECHAR;
                            //set warning
                            AddWarning(5002);
                            break;
                        case leLow:
                            //just add quote
                            strTextContinue += QUOTECHAR;
                            break;
                        }
                    }
                    //strip off ending quote of current msg
                    retval = retval[..^1];
                    //add strText (without its starting quote)
                    retval += strTextContinue[1..];
                    //save current position info
                    lngLastPos = lngPos;
                    lngLastLine = lngLine;
                    strLastLine = strCurrentLine;
                    //get next command
                    strTextContinue = NextToken();
                }
                //after end of string found, move back to correct position
                lngPos = lngLastPos;
                lngLine = lngLastLine;
                errInfo.Line = (lngLastLine + 1).ToString();
                strCurrentLine = strLastLine;
            }
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static internal bool RemoveComments() {
            //this function strips out comments from the input text
            //and trims off leading and trailing spaces
            //
            // fanAGI syntax:
            //      // - rest of line is ignored (not for Sierra syntax)
            //      [ - rest of line is ignored
            //
            // Sierra syntax - only square bracket
            int lngPos;
            bool blnInQuotes = false, blnSlash = false;
            int intROLIgnore;
            //reset compiler
            ResetCompiler();
            do {
                //reset rol ignore
                intROLIgnore = 0;
                //reset comment start + char ptr, and inquotes
                lngPos = 0;
                if (!agSierraSyntax) {
                    blnInQuotes = false;
                }
                //if this line is not empty,
                if (strCurrentLine.Length != 0) {
                    while (lngPos < strCurrentLine.Length - 1) {
                        //get next character from string
                        lngPos++;
                        //if NOT inside a quotation,
                        if (!blnInQuotes) {
                            //check for comment characters at this position
                            if ((strCurrentLine[lngPos..(lngPos + 2)] == CMT2_TOKEN && !agSierraSyntax) || strCurrentLine[lngPos] == CMT1_TOKEN[0]) {
                                intROLIgnore = lngPos;
                                break;
                            }
                            // slash codes never occur outside quotes
                            blnSlash = false;
                            //if this character is a quote mark, it starts a string
                            blnInQuotes = strCurrentLine[lngPos] == QUOTECHAR;
                        }
                        else {
                            //if last character was a slash, ignore this character
                            //because it's part of a slash code
                            if (blnSlash) {
                                //always reset  the slash
                                blnSlash = false;
                            }
                            else {
                                //check for slash or quote mark
                                switch (strCurrentLine[lngPos]) {
                                case QUOTECHAR:
                                    //a quote marks end of string
                                    blnInQuotes = false;
                                    break;
                                case '\\':
                                    // 92 slash
                                    blnSlash = true;
                                    break;
                                }
                            }
                        }
                    }

                    //if any part of line should be ignored,
                    if (intROLIgnore > 0) {
                        strCurrentLine = strCurrentLine[..intROLIgnore];
                    }
                }
                // if not sierra syntax, trim it
                if (!agSierraSyntax) {
                    strCurrentLine = strCurrentLine.Trim();
                }
                //replace comment
                ReplaceLine(lngLine, strCurrentLine);
                //get next line
                IncrementLine();
            } while (lngLine != -1);
            //success
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LineNum"></param>
        /// <param name="strNewLine"></param>
        static internal void ReplaceLine(int LineNum, string strNewLine) {
            //this function replaces the passed line in the input string
            //with the strNewLine, while preserving include header info
            string strInclude;

            //if this is from an include file
            if (Left(stlInput[LineNum], 2) == INCLUDE_MARK) {
                //need to save include header info so it can
                //be preserved after comments are removed
                strInclude = stlInput[LineNum][..stlInput[LineNum].IndexOf('#')];
            }
            else {
                strInclude = "";
            }
            //replace the line
            stlInput[LineNum] = strInclude + strNewLine;
        }


        /// <summary>
        /// 
        /// </summary>
        static internal void ResetCompiler() {
            //resets the compiler so it points to beginning of input
            //also loads first line into strCurrentLine

            //reset include offset, so error trapping
            //can correctly Count lines
            lngIncludeOffset = 0;
            //reset error flag
            blnCriticalError = false;
            //reset the quotemark error flag
            lngQuoteAdded = -1;
            //set line pointer to -2 so first call to
            //IncrementLine gets first line
            lngLine = -2;
            //get first line
            IncrementLine();
            //NOTE: don't need to worry about first line;
            //compiler has already verified the input has at least one line
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static internal bool ReadDefines() {
            // for normal syntax, #define is only valid preprocessor
            // for sierra syntax, all others are allowed:
            // deftype set so define knows what to accept
            //  -1 = already handled or ignored; don't do anything
            //   0 = default define statement; anything goes
            //   1 = flag only
            //   2 = variable only
            //   3 = object (inv or screen) only
            //   4 = view only
            //   5 = test command
            //   6 = action command
            //   7 = redefining test/action
            int i;
            TDefine tdNewDefine = new() { Name = "", Value = "" };
            DefineNameCheck checkName;
            DefineValueCheck checkValue;
            int lngDefType, lngErrNum;
            string strError = "", strToken;

            //reset Count of defines
            lngDefineCount = 0;
            //reset compiler
            ResetCompiler();

            //reset error string
            errInfo.Text = "";

            //step through all lines and find preprocesssor values
            do {
                //check for define statement
                strToken = NextToken(true);
                // with Sierra syntax, blank lines will result in a
                // null token; need to ignore them (by making token a generic string)
                if (strToken.Length == 0) {
                    strToken = " ";
                }
                // check for preprocessor mark '#' (or '%' in Sierra syntax only)
                if (strToken[0] == '#' || (strToken[0] == '%' && agSierraSyntax)) {
                    // strip off the mark
                    strToken = strToken[1..];
                    if (agSierraSyntax) {
                        switch (strToken) {
                        case "tokens":
                            // deprecated; just ignore (don't allow different WORDS.TOK file)
                            lngDefType = -1;
                            // clear line so compiler will ignore it
                            ReplaceLine(lngLine, "");
                            break;
                        case "test":
                            // allow renaming/redefining test commands
                            lngDefType = 5;
                            break;
                        case "action":
                            lngDefType = 6;
                            break;
                        case "flag":
                            // same as 'define name f##', but with a number
                            lngDefType = 1;
                            break;
                        case "var":
                            // same as 'define name v##, but with a number
                            lngDefType = 2;
                            break;
                        case "object":
                            // same as 'define name o##' or 'define name 'i##', but with a number
                            lngDefType = 3;
                            break;
                        case "define":
                            lngDefType = 0;
                            break;
                        case "message":
                            // skip - these are handled by ReadMsgs, after defines
                            lngDefType = -1;
                            break;
                        case "view":
                            // same as 'define name ##', but value must be valid view number
                            lngDefType = 4;
                            break;
                        default:
                            // invalid - ignore it; main compiler will catch it
                            // (there are some sierra source files that have
                            // multi-line string assignments that look like
                            // invalid preprocessor)
                            lngDefType = -1;
                            break;
                        }
                    }
                    else {
                        // in fanAGI syntax, only 'define', 'message' allowed
                        switch (strToken) {
                        case "define":
                            lngDefType = 0;
                            break;
                        case "message":
                            // skip; these are handled in ReadMsgs, afer defines
                            lngDefType = -1;
                            break;
                        default:
                            // invalid preprocessor token; ignore it; main
                            // compiler will handle it
                            lngDefType = -1;
                            break;
                        }
                    }
                    if (lngDefType >= 0) {
                        // reset type
                        tdNewDefine.Type = atNum;
                        // get name
                        tdNewDefine.Name = NextToken(true);
                        // for test/action commands, the value may include
                        // args; WinAGI ignores all that; only the number matters
                        if (lngDefType == 5 || lngDefType == 6) {
                            // goto end of line to get last token
                            tdNewDefine.Value = NextToken(true);
                            do {
                                strToken = NextToken(true);
                                if (strToken.Length > 0) {
                                    tdNewDefine.Value = strToken;
                                }
                                else {
                                    break;
                                }
                            } while (true);
                        }
                        else {
                            // get value
                            tdNewDefine.Value = NextToken(true);
                        }
                        // nothing else allowed on the line
                        if (NextToken(true).Length != 0) {
                            AddMinorError(4163);
                            // skip to end
                            lngPos = strCurrentLine.Length;
                        }
                        // check for redefines
                        for (i = 0; i < lngDefineCount; i++) {
                            if (tdNewDefine.Value == tdDefines[i].Name) {
                                tdNewDefine.Value = tdDefines[i].Value;
                                tdNewDefine.Type = tdDefines[i].Type;
                                break;
                            }
                        }
                        // validate define name
                        checkName = ValidateDefName(tdNewDefine.Name);
                        if (agSierraSyntax) {
                            // ignore overrides 
                            if (checkName > ncBadChar) {
                                checkName = ncOK;
                            }
                            // set type based on token used
                            switch (lngDefType) {
                            case 1:
                                // flag only
                                tdNewDefine.Type = atFlag;
                                break;
                            case 2:
                                // variable only
                                tdNewDefine.Type = atVar;
                                break;
                            case 3:
                                // generiic obj (sobj or invitem) only
                                tdNewDefine.Type = atObj;
                                break;
                            case 4:
                                // view only
                                if (byte.TryParse(tdNewDefine.Value, out _) && compGame.agViews.Exists(byte.Parse(tdNewDefine.Value))) {
                                    // OK
                                    tdNewDefine.Type = atView;
                                }
                                else {
                                    // default to number
                                    tdNewDefine.Type = atNum;
                                    switch (ErrorLevel) {
                                    case leHigh:
                                        AddMinorError(6003);
                                        break;
                                    case leMedium:
                                        AddWarning(5110);
                                        break;
                                    }
                                }
                                break;
                            case 5:
                                //rename test command
                                if (byte.TryParse(tdNewDefine.Value, out byte tmp) && tmp <= agNumTestCmds) {
                                    agTestCmds[byte.Parse(tdNewDefine.Value)].Name = tdNewDefine.Name;
                                }
                                else {
                                    AddMinorError(6004, LoadResString(6004).Replace(ARG1, tdNewDefine.Value));
                                }
                                // type is already set
                                break;
                            case 6:
                                // rename action command
                                if (byte.TryParse(tdNewDefine.Value, out tmp) && tmp <= agNumCmds) {
                                    agCmds[byte.Parse(tdNewDefine.Value)].Name = tdNewDefine.Name;
                                }
                                else {
                                    AddMinorError(6005, LoadResString(6005).Replace(ARG1, tdNewDefine.Value));
                                }
                                // type is already set
                                break;
                            case 7:
                                // redefine a command - no action needed
                                break;
                            }
                        }
                        else {
                            // override name errors (8-13) are only warnings if errorlevel is medium or low
                            switch (ErrorLevel) {
                            case leMedium:
                                if (checkName == ncGlobal) {
                                    AddWarning(5034, LoadResString(5034).Replace(ARG1, tdNewDefine.Name));
                                    checkName = ncOK;
                                }
                                else if (checkName > ncGlobal) {
                                    AddWarning(5035, LoadResString(5035).Replace(ARG1, tdNewDefine.Name));
                                    checkName = ncOK;
                                }
                                break;
                            case leLow:
                                if (checkName > ncBadChar) {
                                    checkName = ncOK;
                                }
                                break;
                            }
                        }
                        // now check for errors
                        if (checkName != ncOK) {
                            lngErrNum = 0;
                            switch (checkName) {
                            case ncEmpty:
                                // no name
                                lngErrNum = 4070;
                                strError = LoadResString(4070);
                                break;
                            case ncNumeric:
                                // name is numeric
                                lngErrNum = 4072;
                                strError = LoadResString(4072);
                                break;
                            case ncActionCommand:
                                // name is a command
                                if (agSierraSyntax) {
                                    // OK if defining a command name with '#define/#action/#test'
                                    switch (lngDefType) {
                                    case 0:
                                        // define - mark it as a redfine
                                        lngDefType = 7;
                                        break;
                                    case 5 or 6:
                                        // OK
                                        break;
                                    default:
                                        lngErrNum = 6002;
                                        strError = LoadResString(6002).Replace(ARG1, tdNewDefine.Name).Replace(ARG2, "action");
                                        break;
                                    }
                                }
                                else {
                                    // not allowed for agiFan syntax
                                    lngErrNum = 4021;
                                    strError = LoadResString(4021).Replace(ARG1, tdNewDefine.Name);
                                }
                                break;
                            case ncTestCommand:
                                // name is test command
                                if (agSierraSyntax) {
                                    // OK if defining a command name with #test/#action/#define
                                    switch (lngDefType) {
                                    case 0:
                                        //mark it as a redefine
                                        lngDefType = 7;
                                        break;
                                    case 5 or 6:
                                        // OK
                                        break;
                                    default:
                                        lngErrNum = 6002;
                                        strError = LoadResString(6002).Replace(ARG1, tdNewDefine.Value).Replace(ARG2, "test");
                                        break;
                                    }
                                }
                                else {
                                    // not allowed for normal syntax
                                    lngErrNum = 4022;
                                    strError = LoadResString(4022).Replace(ARG1, tdNewDefine.Name);
                                }
                                break;
                            case ncKeyWord:
                                // name is a compiler keyword
                                lngErrNum = 4013;
                                strError = LoadResString(4013).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ncArgMarker:
                                // name is an argument marker
                                lngErrNum = 4071;
                                strError = LoadResString(4071);
                                break;
                            case ncBadChar:
                                // name contains improper character
                                lngErrNum = 4067;
                                strError = LoadResString(4067);
                                break;
                            case ncGlobal:
                                // name is already globally defined
                                lngErrNum = 4019;
                                strError = LoadResString(4019).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ncReservedVar:
                                // name is a reserved variable name
                                lngErrNum = 4018;
                                strError = LoadResString(4018).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ncReservedFlag:
                                // name is reserved flag name
                                lngErrNum = 4014;
                                strError = LoadResString(4014).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ncReservedNum:
                                //name is reserved number constant
                                lngErrNum = 4016;
                                strError = LoadResString(4016).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ncReservedObj:
                                // name is reserved object constant
                                lngErrNum = 4017;
                                strError = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ncReservedStr:
                                // name is reserved object constant
                                lngErrNum = 4017;
                                strError = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ncReservedMsg:
                                // name is reserved message constant
                                lngErrNum = 4015;
                                strError = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
                                break;
                            }
                            if (lngErrNum > 0) {
                                AddMinorError(lngErrNum, strError);
                            }
                        }
                        // validate define value
                        lngErrNum = 0;
                        // type is already set for sierra syntax; value validation sets it for fanAGI
                        checkValue = ValidateDefValue(tdNewDefine);
                        // value errors 4-6 are only warnings if errorlevel is medium or low
                        switch (ErrorLevel) {
                        case leMedium:
                            switch (checkValue) {
                            case vcNotAValue:
                                // string value missing quotes -
                                // if not using original Sierra syntax, only strings
                                // and numbers are allowed
                                // -- fix missing quotes
                                if (tdNewDefine.Value[0] != '\"') {
                                    tdNewDefine.Value = '\"' + tdNewDefine.Value;
                                }
                                if (tdNewDefine.Value[^1] != '\"') {
                                    tdNewDefine.Value += '\"';
                                }
                                AddWarning(5022);
                                //reset error code
                                checkValue = vcOK;
                                break;
                            case vcReserved:
                                // value is already defined by a reserve name
                                AddWarning(5032, LoadResString(5032).Replace(ARG1, tdNewDefine.Value));
                                // reset error code
                                checkValue = vcOK;
                                break;
                            case vcGlobal:
                                // value is already defined by a global name
                                AddWarning(5031, LoadResString(5031).Replace(ARG1, tdNewDefine.Value));
                                // reset error code
                                checkValue = vcOK;
                                break;
                            }
                            break;
                        case leLow:
                            switch (checkValue) {
                            case vcNotAValue:
                                // missing quotes
                                // if not using original Sierra syntax, only strings
                                // and numbers are allowed
                                if (!agSierraSyntax) {
                                    // fix the value
                                    if (tdNewDefine.Value[0] != '\"') {
                                        tdNewDefine.Value = '\"' + tdNewDefine.Value;
                                    }
                                    if (tdNewDefine.Value[^1] != '\"') {
                                        tdNewDefine.Value += '\"';
                                    }
                                }
                                // reset return value
                                checkValue = vcOK;
                                break;
                            case vcGlobal or vcReserved:
                                // redefining reserved or global
                                // reset return value
                                checkValue = vcOK;
                                break;
                            }
                            break;
                        }
                        // check for errors
                        if (checkValue != vcOK) {
                            switch (checkValue) {
                            case vcEmpty:
                                // no value
                                lngErrNum = 4073;
                                strError = LoadResString(4073);
                                break;
                            case vcOutofBounds:
                                lngErrNum = 4042;
                                strError = LoadResString(4042);
                                break;
                            case vcBadArgNumber:
                                // argument value not valid for controller, string, word
                                // controller - 4136
                                // string - 4079
                                // word - 4090
                                switch (tdNewDefine.Type) {
                                case atCtrl:
                                    lngErrNum = 4136;
                                    strError = LoadResString(4136);
                                    break;
                                case atStr:
                                    lngErrNum = 4079;
                                    strError = LoadResString(4079);
                                    break;
                                case atWord:
                                    lngErrNum = 4090;
                                    strError = LoadResString(4090);
                                    break;
                                }
                                break;
                            case vcNotAValue:
                                // value is not a string, number, or arg value
                                lngErrNum = 4082;
                                strError = LoadResString(4082);
                                break;
                            case vcReserved:
                                // value is already defined by a reserve name
                                lngErrNum = 4041;
                                strError = LoadResString(4041).Replace(ARG1, tdNewDefine.Value);
                                break;
                            case vcGlobal:
                                // value is already defined by a global name
                                lngErrNum = 4040;
                                strError = LoadResString(4040).Replace(ARG1, tdNewDefine.Value);
                                break;
                            }
                        }
                        if (lngErrNum != 0) {
                            AddMinorError(lngErrNum, strError);
                        }
                        // for Sierra syntax-specific tokens (not #defines)
                        if (lngDefType > 0) {
                            // numbers must be byte(0-255)
                            if (int.TryParse(tdNewDefine.Value, out int tmp)) {
                                if (tmp > 256 || tmp < 0) {
                                    // invalid
                                    AddMinorError(6013);
                                }
                            }
                            else {
                                // invalid
                                AddMinorError(6013);
                            }
                        }
                        else {
                            // check all previous defines
                            for (i = 0; i < lngDefineCount; i++) {
                                if (tdNewDefine.Name == tdDefines[i].Name) {
                                    AddMinorError(4012, LoadResString(4012).Replace(ARG1, tdDefines[i].Name));
                                }
                                if (tdNewDefine.Value == tdDefines[i].Value) {
                                    // numeric duplicates are OK
                                    if (!int.TryParse(tdNewDefine.Value, out _)) {
                                        switch (ErrorLevel) {
                                        case leHigh:
                                            AddMinorError(4023, LoadResString(4023).Replace(ARG1, tdDefines[i].Value).Replace(ARG2, tdDefines[i].Name));
                                            break;
                                        case leMedium:
                                            AddWarning(5033, LoadResString(5033).Replace(ARG1, tdDefines[i].Value).Replace(ARG2, tdDefines[i].Name));
                                            break;
                                        }
                                    }
                                }
                            }
                            // check against labels
                            if (bytLabelCount > 0) {
                                for (i = 1; i <= bytLabelCount; i++) {
                                    if (tdNewDefine.Name == llLabel[i].Name) {
                                        AddMinorError(4020, LoadResString(4020).Replace(ARG1, tdNewDefine.Name));
                                    }
                                }
                            }
                        }
                        // save this define
                        Array.Resize(ref tdDefines, lngDefineCount);
                        tdDefines[lngDefineCount - 1] = tdNewDefine;
                        //increment counter
                        lngDefineCount++;
                        //now set this line to empty so Compiler doesn"t try to read it
                        ReplaceLine(lngLine, "");
                    }
                }
                //get next line
                IncrementLine();
            } while (lngLine != -1);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static internal bool ReadMsgs() {
            //note that stripped message lines also strip out the include header string
            //this doesn't matter since they are only blank lines anyway
            //only need to include header info if error occurs, and errors never occur on
            //blank line

            bool blnDef = false;
            int intMsgNum, lngSlashCount, lngQuotesOK, lngMsgStart;
            string strToken;

            //reset message list to blamk
            for (intMsgNum = 0; intMsgNum <= 255; intMsgNum++) {
                strMsg[intMsgNum] = "";
                blnMsg[intMsgNum] = false;
                intMsgWarn[intMsgNum] = 0;
            }
            //reset compiler
            ResetCompiler();
            do {
                //get first command on this line
                strToken = NextToken(true);
                //if this is the message marker
                if (strToken == MSG_TOKEN || strToken == "%message") {
                    if (!agSierraSyntax && strToken == "%message") {
                        AddMinorError(4061, LoadResString(4061).Replace(ARG1, strToken));
                    }
                    //save starting line number (incase this msg uses multiple lines)
                    lngMsgStart = lngLine;
                    //get next token as message number (on this line if not Sierra syntax)
                    strToken = NextToken(!agSierraSyntax);
                    //this should be a msg number
                    if (!int.TryParse(strToken, out _)) {
                        //critical error
                        blnCriticalError = true;
                        errInfo.ID = "4077";
                        errInfo.Text = LoadResString(4077);
                        // TODO: why does this have to be  critical error?
                        return false;
                    }
                    else {
                        // validate msgnum
                        intMsgNum = VariableValue(strToken);
                        if (intMsgNum <= 0) {
                            //error
                            errInfo.ID = "4077";
                            errInfo.Text = LoadResString(4077);
                            // TODO: why does this have to be a critical error?
                            return false;
                        }
                        //if msg is already assigned
                        if (blnMsg[intMsgNum]) {
                            errInfo.ID = "4094";
                            errInfo.Text = LoadResString(4094).Replace(ARG1, (intMsgNum).ToString());
                            // TODO: why does this have to be a critical error?
                            return false;
                        }
                    }
                    //get next token (should be the message text)
                    strToken = NextToken(false);
                    if (agSierraSyntax) {
                        // in Sierra syntax msgtext can only be a string, with quotes required
                        if (strToken.Length == 0) {
                            // could only happen if at end of logic
                            blnCriticalError = true;
                            errInfo.ID = "6006";
                            errInfo.Text = LoadResString(6006);
                            return false;
                        }
                        if (strToken[0] != '\"' || strToken[^1] != '\"' || strToken.Length == 2) {
                            AddMinorError(6007);
                        }
                        //is this a valid string?
                        if (!IsAGIString(strToken)) {
                            // try concatenation
                            ConcatSierraStr(ref strToken);
                            if (blnCriticalError) {
                                errInfo.ID = "6008";
                                errInfo.Text = LoadResString(6008);
                                return false;
                            }
                        }
                    }
                    else {
                        // fanAGI syntax
                        //is this a valid string?
                        if (!IsAGIString(strToken)) {
                            //maybe it's a define
                            if (ConvertArgument(ref strToken, atMsg)) {
                                //defined strings never get concatenated
                                blnDef = true;
                            }
                        }
                        //always reset the 'addquote' flag
                        //(this is the flag that notes if/where a line had an end quote
                        //added by the compiler; if this causes problems later in the
                        //compilation of this command, we can then mark this error
                        //as the culprit
                        lngQuoteAdded = -1;
                        //check msg for quotes (note ending quote has to be checked to make sure it's not
                        //an embedded quote)
                        //assume OK until we learn otherwise (0=OK; 1=bad start quote; 2=bad end quote; 3=bad both)
                        lngQuotesOK = 0;
                        if (strToken[0] != '"') {
                            //bad start quote
                            lngQuotesOK = 1;
                        }
                        else {
                            // rare but check for single quote
                            if (strToken.Length < 2) {
                                AddMinorError(4081);
                                // add placeholder to continue
                                strToken = "\"\"";
                            }
                        }
                        //check for end quote
                        if (strToken[^1] != '"') {
                            //bad end quote
                            lngQuotesOK &= 2;
                        }
                        else {
                            //just because it ends in a quote doesn't mean it's good;
                            //it might be an embedded quote
                            //check for an odd number of slashes immediately preceding
                            //this quote
                            lngSlashCount = 0;
                            do {
                                if (strToken[^(lngSlashCount + 1)] == '\\') {
                                    lngSlashCount++;
                                }
                                else {
                                    break;
                                }
                            } while (strToken.Length - (lngSlashCount + 1) >= 0);
                            //if it IS odd, then it's not a valid quote
                            if (lngSlashCount % 2 == 1) {
                                //it's embedded, and doesn't count
                                lngQuotesOK &= 2;
                            }
                        }
                        //if either (or both) quote is missing, deal with it
                        if (lngQuotesOK > 0) {
                            //note which line had quotes added, in case it results
                            //in an error caused by a missing end ')' or whatever
                            //the next required element is
                            lngQuoteAdded = lngLine;
                            // add missing quotes
                            if ((lngQuotesOK & 1) == 1) {
                                strToken = '\"' + strToken;
                            }
                            if ((lngQuotesOK & 2) == 2) {
                                strToken += '\"';
                            }
                            switch (ErrorLevel) {
                            case leHigh:
                                errInfo.ID = "4051";
                                errInfo.Text = LoadResString(4051);
                                return false;
                            case leMedium:
                                //warn if medium
                                AddWarning(5002);
                                break;
                            }
                        }
                        //concatenate, if necessary
                        if (!blnDef) {
                            strToken = ConcatArg(strToken);
                        }
                        // always reset blnDef for next msg
                        blnDef = false;
                    }
                    //nothing allowed after msg declaration
                    if (lngPos != strCurrentLine.Length) {
                        char nextchar = NextChar(true);
                        if (nextchar != 0) {
                            if (agSierraSyntax) {
                                // ';' at end is OK
                                if (strToken[0] != ';' || NextChar(true) != 0) {
                                    AddMinorError(4099);
                                }
                            }
                            else {
                                //error
                                AddMinorError(4099);
                                // ignore rest of line
                                lngPos = strCurrentLine.Length;
                            }
                        }
                    }

                    //strip off quotes (we know that the string
                    //is properly enclosed by quotes because
                    //ConcatArg function validates they are there
                    //or adds them if they aren't[or raises an
                    //error, in which case it doesn't even matter])
                    strToken = strToken[1..^1];
                    //add msg
                    strMsg[intMsgNum] = strToken;
                    //validate message characters
                    ValidateMsgChars(strToken, intMsgNum);
                    // mark message as assigned
                    blnMsg[intMsgNum] = true;
                    do {
                        //set the msg line (and any concatenated lines) to empty so
                        //compiler doesn't try to read it
                        ReplaceLine(lngMsgStart, "");
                        //increment the counter (to get multiple lines, if string is
                        //concatenated over more than one line)
                        lngMsgStart++;
                        //continue until back to current line
                    } while (lngMsgStart <= lngLine);
                }
                //get next line
                IncrementLine();
            } while (lngLine != -1);
            // done
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WarningNum"></param>
        /// <param name="WarningText"></param>
        static internal void AddWarning(int WarningNum, string WarningText = "") {
            //(number, line and module only have meaning for logic warnings
            // other warnings generated during a game compile will use
            // same format, but use -- for warning number, line and module)

            //if no text passed, use the default resource string
            if (WarningText.Length == 0) {
                WarningText = LoadResString(WarningNum);
            }
            //only add if not ignoring
            if (!agNoCompWarn[WarningNum - 5000]) {
                errInfo.Type = EventType.etWarning;
                errInfo.ID = WarningNum.ToString();
                errInfo.Text = WarningText;
                compGame.OnCompileLogicStatus(errInfo);
                // restore error as default
                errInfo.Type = EventType.etError;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static internal bool CompileIf() {
            //this routine will read and validate a group of test commands
            //for an 'if' statement and return
            //it is entered when the compiler encounters an //if// command
            //the syntax expected for test commands is:
            //
            //   if(<test>){
            //or
            //   if(<test> && <test> && ... ){
            //or
            //   if((<test> || <test> || ... )){
            //
            //or a combination of ORs and ANDs, as long as ORs are always
            //   in brackets, and ANDs are never in brackets
            //
            //   <test> may be a test command (<tstcmd>(arg1, arg2, ..., argn)
            //   or a special syntax representation of a test command:
            //     fn        ==> isset(fn)
            //     vn == m   ==> equaln(vn,m)
            //     etc
            //
            //valid special comparison expressions are ==, !=, <>, >, =<, <, >=
            //OR//ed tests must always be enclosed in parenthesis; AND//ed tests
            //must never be enclosed in parentheses (this ensures the compiled code
            //will be compatible with the AGI interpreter)
            //
            //(any test command may have the negation operator (!) placed directly in front of it

            string strTestCmd, strArg;
            byte bytTestCmd;
            byte[] bytArg = new byte[8];
            int lngArg;
            int[] lngWord;
            int intWordCount;
            int i;
            bool blnOrBlock = false; //command block, not a comment block
            bool blnNeedNextCmd = true;
            int intNumTestCmds = 0;
            int intNumCmdsInBlock = 0;
            bool blnNOT;
            bool endfound = false;

            //write out starting if byte
            tmpLogRes.WriteByte(0xFF);

            //next character should be "("
            if (!CheckChar('(')) {
                AddMinorError(4002);
                return false;
            }

            //now, step through input, until final ')' is found:
            do {
                //get next command
                strTestCmd = NextToken();
                //check for end of input,
                if (lngLine == -1) {
                    // nothing left, return critical error
                    errInfo.ID = "4106";
                    errInfo.Text = LoadResString(4106);
                    return false;
                }
                //if awaiting a test command,
                if (blnNeedNextCmd) {
                    switch (strTestCmd) {
                    case "(": //open paran
                              //if already in a block
                        if (blnOrBlock) {
                            AddMinorError(4045);
                        }
                        //write /'or' block start
                        tmpLogRes.WriteByte(0xFC);
                        blnOrBlock = true;
                        intNumCmdsInBlock = 0;
                        break;
                    case ")":
                        //if a test command is expected, ')' always causes error
                        if (blnOrBlock && intNumCmdsInBlock == 0) {
                            // TODO: technically not an error; it will compile and run
                            // but I don't feel like adding another warning
                            AddMinorError(4044);
                            // close the block
                            blnOrBlock = false;
                            blnNeedNextCmd = false;
                        }
                        else if (intNumTestCmds == 0) {
                            // TODO: technically not an error; it will compile and run
                            // but I don't feel like adding another warning
                            AddMinorError(4057);
                            // done with if
                            break;
                        }
                        else {
                            AddMinorError(4056);
                            // done with if;
                            break;
                        }
                        break;
                    default:
                        //check for NOT
                        blnNOT = (strTestCmd == NOT_TOKEN);
                        if (blnNOT) {
                            tmpLogRes.WriteByte(0xFD);
                            //read in next test command
                            strTestCmd = NextToken();
                            //check for end of input,
                            if (lngLine == -1) {
                                blnCriticalError = true;
                                errInfo.ID = "4106";
                                errInfo.Text = LoadResString(4106);
                                return false;
                            }
                        }
                        bytTestCmd = CommandNum(true, strTestCmd);
                        //if command not found,
                        if (bytTestCmd == 255) {
                            //check for special syntax
                            if (!CompileSpecialif(strTestCmd, blnNOT)) {
                                //error; the CompileSpecialIf function
                                //sets the error codes, - return true to continue
                                return true;
                            }
                        }
                        else {
                            //write the test command code
                            tmpLogRes.WriteByte(bytTestCmd);
                            //next command should be "("
                            if (!CheckChar('(')) {
                                AddMinorError(4048);
                            }
                            //check for return.false() command
                            if (bytTestCmd == 0) {
                                //warn user that it's not compatible with AGI Studio
                                switch (ErrorLevel) {
                                case leHigh or leMedium:
                                    //generate warning
                                    AddWarning(5081);
                                    break;
                                }
                            }
                            //if said command
                            if (bytTestCmd == 0xE) {
                                // reset word count
                                intWordCount = 0;
                                lngWord = [];
                                // loop to add words
                                do {
                                    //get first word arg
                                    lngArg = GetNextArg(atVocWrd, intWordCount);
                                    //if error
                                    if (lngArg < 0) {
                                        switch (lngArg) {
                                        case -1:
                                            AddMinorError(int.Parse(errInfo.ID), errInfo.Text);
                                            break;
                                        default:
                                            // show error
                                            AddMinorError(4054);
                                            break;
                                        }
                                        // use placeholder 'anyword'
                                        lngArg = 1;
                                    }
                                    // if too many words
                                    if (intWordCount == 10) {
                                        AddMinorError(4093);
                                        break;
                                    }
                                    else if (intWordCount < 10) {
                                        // if 1 to 9 words, ok to add this word number
                                        // to array of word numbers
                                        Array.Resize(ref lngWord, intWordCount + 1);
                                        lngWord[intWordCount] = lngArg;
                                    }
                                    intWordCount++;
                                    //get next character
                                    //(should be a comma, or close parenthesis, if no more words)
                                    strArg = NextChar().ToString();
                                    if (strArg.Length != 0) {
                                        switch (strArg[0]) {
                                        case ')':
                                            //move pointer back one space so
                                            //the ')' will be found at end of command
                                            lngPos--;
                                            endfound = true;
                                            break; //exit do
                                        case ',':
                                            //expected; continue check for next word argument
                                            break;
                                        default:
                                            //missing comma
                                            AddMinorError(4047, LoadResString(4047).Replace(ARG1, (intWordCount + 1).ToString()));
                                            // assume comma and continue
                                            lngPos -= strArg.Length;
                                            break;
                                        }
                                    }
                                    else {
                                        //this  should normally never happen, since changing the function to allow
                                        //splitting over multiple lines, unless this is the LAST line of
                                        //the logic (an EXTREMELY rare edge case)
                                        //check for added quotes; they are the problem
                                        if (lngQuoteAdded >= 0) {
                                            //reset line;
                                            lngLine = lngQuoteAdded;
                                            errInfo.Line = (lngLine - lngIncludeOffset + 1).ToString();
                                        }
                                        errInfo.ID = "4047";
                                        //use 1-base arg values
                                        errInfo.Text = LoadResString(4047).Replace(ARG1, (intWordCount + 1).ToString());
                                        return false;
                                    }
                                } while (true);
                                //reset the quotemark error flag after ')' is found
                                lngQuoteAdded = -1;
                                //need to write number of arguments for //said//
                                //before writing arguments themselves
                                tmpLogRes.WriteByte((byte)intWordCount);
                                //now add words
                                for (i = 0; i < intWordCount; i++) {
                                    //write word Value
                                    tmpLogRes.WriteWord((ushort)lngWord[i]);
                                }
                            }
                            else {
                                //not 'said'; extract arguments for this command
                                for (i = 0; i < TestCommands[bytTestCmd].ArgType.Length; i++) {
                                    //after first argument, verify comma separates arguments
                                    if (i > 0) {
                                        if (!CheckChar(',')) {
                                            AddMinorError(4047, LoadResString(4047).Replace(ARG1, (i + 1).ToString()));
                                        }
                                    }
                                    //reset the quotemark error flag after comma is found
                                    lngQuoteAdded = -1;
                                    lngArg = GetNextArg(TestCommands[bytTestCmd].ArgType[i], i);
                                    //check for error
                                    if (lngArg >= 0) {
                                        bytArg[i] = (byte)lngArg;
                                    }
                                    else {
                                        if (lngArg == -1) {
                                            AddMinorError(int.Parse(errInfo.ID), errInfo.Text);
                                        }
                                        else {
                                            // if error number is 4054 add cmd name
                                            AddMinorError(4054, errInfo.Text.Replace(ARG2, agTestCmds[bytTestCmd].Name));
                                        }
                                        // use a placeholder
                                        bytArg[i] = 0;
                                    }
                                    //write argument
                                    tmpLogRes.WriteByte(bytArg[i]);
                                }
                            }
                            //next character should be ")"
                            if (!CheckChar(')')) {
                                AddMinorError(4160);
                            }
                            //reset the quotemark error flag
                            lngQuoteAdded = -1;
                            //validate arguments for this command
                            if (!ValidateIfArgs(bytTestCmd, ref bytArg)) {
                                //error assigned by called function
                                return false;
                            }
                        }
                        //command added
                        intNumTestCmds++;
                        //if in IF block,
                        if (blnOrBlock) {
                            intNumCmdsInBlock++;
                        }
                        //toggle off need for test command
                        blnNeedNextCmd = false;
                        break;
                    }
                }
                else { //not awaiting a test command
                    switch (strTestCmd) {
                    case NOT_TOKEN:
                        //invalid
                        AddMinorError(4097);
                        break;
                    case AND_TOKEN:
                        //if inside brackets
                        if (blnOrBlock) {
                            AddMinorError(4037);
                        }
                        blnNeedNextCmd = true;
                        break;
                    case OR_TOKEN:
                        //if NOT inside brackets
                        if (!blnOrBlock) {
                            AddMinorError(4100);
                            // assume a valid test
                            intNumCmdsInBlock++;
                            // force orblock
                            blnOrBlock = false;
                        }
                        blnNeedNextCmd = true;
                        break;
                    case ")":
                        //if inside brackets
                        if (blnOrBlock) {
                            //ensure at least one command in block,
                            if (intNumCmdsInBlock == 0) {
                                // TODO: technically, not an error- it will compile and run
                                // but I don't feel like adding another warning code...
                                AddMinorError(4044);
                                return false;
                            }
                            else if (intNumCmdsInBlock == 1) {
                                AddWarning(5109);
                            }
                            //close brackets
                            blnOrBlock = false;
                            tmpLogRes.WriteByte(0xFC);
                        }
                        else {
                            //ensure at least one command in block,
                            if (intNumTestCmds == 0) {
                                AddWarning(4057);
                            }
                            //end of if block found
                            tmpLogRes.WriteByte(0xFF);
                            return true;
                        }
                        break;
                    default:
                        int tmpErr;
                        if (blnOrBlock) {
                            tmpErr = 4101;
                        }
                        else {
                            tmpErr = 4038;
                        }
                        AddMinorError(tmpErr);
                        // assume it was ok
                        blnNeedNextCmd = true;
                        // 'and if it is a close bracket, assume
                        // the 'if' block is now closed
                        if (strTestCmd == "{") {
                            lngPos--;
                            break;
                        }
                        break;
                    }
                }
                //never leave loop normally; error, end of input, or successful
                //compilation of test commands will all exit loop directly
            } while (true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strText"></param>
        static void ConcatSierraStr(ref string strText) {
            // sierra syntax allows concatenation over multiple lines, with
            // ending quote only at end of last line

            // strText is first line, without closing quote, and goes to
            // end of current line, so first time through the loop the
            // line will automatically increment

            // add text, including spaces, until valid ending quote found

            bool blnInQuotes = true, blnSlash = false;
            char theChar;

            //add characters until another TRUE quote is found
            do {
                //if at end of line
                if (lngPos == strCurrentLine.Length) {
                    //go to next line
                    IncrementLine();
                    //don't exceed last line
                    if (lngLine == -1) {
                        blnCriticalError = true;
                        return;
                    }
                }

                // get the next char in the line
                theChar = strCurrentLine[lngPos + 1];
                // increment position
                lngPos++;
                // if last char was a slash,
                if (blnSlash) {
                    // next char is just added as-is; no checking it
                    // always reset  the slash
                    blnSlash = false;
                }
                else {
                    // regular char; check for slash or quote mark
                    switch (theChar) {
                    case '\"': //quote mark
                               // a quote marks end of string
                        blnInQuotes = false;
                        break;
                    case '\\': //backslash
                        blnSlash = true;
                        break;
                    }
                }

                strText += theChar;
                if (!blnInQuotes) {
                    return;
                }
            } while (true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CmdNum"></param>
        /// <param name="ArgVal"></param>
        /// <returns></returns>
        static internal bool ValidateArgs(int CmdNum, ref byte[] ArgVal) {
            //check for specific command issues
            //for commands that can affect variable values, need to check against reserved variables
            //for commands that can affect flags, need to check against reserved flags
            //for other commands, check the passed arguments to see if values are appropriate
            bool blnUnload = false, blnWarned = false;

            // check each command
            switch (CmdNum) {
            case 1 or 2 or (>= 4 and <= 8) or 10 or (>= 165 and <= 168):
                //increment, decrement, assignv, addn, addv, subn, subv
                //rindirect, mul.n, mul.v, div.n, div.v
                //check for reserved variables that should never be manipulated
                //(assume arg Value is zero to avoid tripping other checks)
                CheckResVarUse(ArgVal[0], 0);
                //for div.n(vA, B) only, check for divide-by-zero
                if (CmdNum == 167) {
                    if (ArgVal[1] == 0) {
                        switch (ErrorLevel) {
                        case leHigh:
                            errInfo.ID = "4149";
                            errInfo.Text = LoadResString(4149);
                            return false;
                        case leMedium:
                            AddWarning(5030);
                            break;
                        }
                    }
                }
                break;
            case 3:
                //assignn
                //check for actual Value being assigned
                CheckResVarUse(ArgVal[0], ArgVal[1]);
                break;
            case >= 12 and <= 14:
                //set, reset, toggle
                //check for reserved flags
                CheckResFlagUse(ArgVal[0]);
                break;
            case 18:
                //new.room(A)
                //validate that this logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4120";
                        errInfo.Text = LoadResString(4120);
                        return false;
                    case leMedium:
                        AddWarning(5053);
                        break;
                    }
                }
                //expect no more commands
                blnNewRoom = true;
                break;
            case 19:
                //new.room.v
                //expect no more commands
                blnNewRoom = true;
                break;
            case 20:
                //load.logics(A)
                //validate that logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4121";
                        errInfo.Text = LoadResString(4121).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    case leMedium:
                        AddWarning(5013);
                        break;
                    }
                }
                break;
            case 22:
                //call(A)
                //calling logic0 is a BAD idea
                if (ArgVal[0] == 0) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4118";
                        errInfo.Text = LoadResString(4118);
                        return false;
                    case leMedium:
                        AddWarning(5010);
                        break;
                    }
                }
                //recursive calling is usually BAD
                if (ArgVal[0] == bytLogComp) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4117";
                        errInfo.Text = LoadResString(4117);
                        return false;
                    case leMedium:
                        AddWarning(5089);
                        break;
                    }
                }
                //validate that logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4156";
                        errInfo.Text = LoadResString(4156).Replace(ARG1, (ArgVal[0]).ToString());
                        return false;
                    case leMedium:
                        AddWarning(5076);
                        break;
                    }
                }
                break;
            case 30:
                //load.view(A)
                //validate that view exists
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4122";
                        errInfo.Text = LoadResString(4122).Replace(ARG1, (ArgVal[0]).ToString());
                        return false;
                    case leMedium:
                        AddWarning(5015);
                        break;
                    }
                }
                break;
            case 32:
                //discard.view(A)
                //validate that view exists
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4123";
                        errInfo.Text = LoadResString(4123).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    case leMedium:
                        AddWarning(5024);
                        break;
                    }
                }
                break;
            case 37:
                //position(oA, X,Y)
                //check x/y against limits
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4128";
                        errInfo.Text = LoadResString(4128);
                        return false;
                    case leMedium:
                        AddWarning(5023);
                        break;
                    }
                }
                break;
            case 39:
                //get.posn
                //neither variable arg should be a reserved Value
                if (ArgVal[1] <= 26 || ArgVal[2] <= 26) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 41:
                //set.view(oA, B)
                //validate that view exists
                if (!compGame.agViews.Exists(ArgVal[1])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4124";
                        errInfo.Text = LoadResString(4124).Replace(ARG1, (ArgVal[1]).ToString());
                        return false;
                    case leMedium:
                        AddWarning(5037);
                        break;
                    }
                }
                break;
            case (>= 49 and <= 53) or 97 or 118:
                //last.cel, current.cel, current.loop,
                //current.view, number.of.loops, get.room.v
                //get.num
                //variable arg is second
                //variable arg should not be a reserved Value
                if (ArgVal[1] <= 26) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 54:
                //set.priority(oA, B)
                //check priority Value
                if (ArgVal[1] > 15) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4125";
                        errInfo.Text = LoadResString(4125);
                        return false;
                    case leMedium:
                        AddWarning(5050);
                        break;
                    }
                }
                break;
            case 57:
                //get.priority
                //variable is second argument
                //variable arg should not be a reserved Value
                if (ArgVal[1] <= 26) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 63:
                //set.horizon(A)
                //>120 or <16 is unusual
                //>=167 will cause AGI to freeze/crash

                //validate horizon Value
                switch (ErrorLevel) {
                case leHigh:
                    if (ArgVal[0] >= 167) {
                        errInfo.ID = "4126";
                        errInfo.Text = LoadResString(4126);
                        return false;
                    }
                    if (ArgVal[0] > 120) {
                        AddWarning(5042);
                    }
                    else if (ArgVal[0] < 16) {
                        AddWarning(5041);
                    }
                    break;
                case leMedium:
                    if (ArgVal[0] >= 167) {
                        AddWarning(5043);
                    }
                    else if (ArgVal[0] > 120) {
                        AddWarning(5042);
                    }
                    else if (ArgVal[0] < 16) {
                        AddWarning(5041);
                    }
                    break;
                }
                break;
            case >= 64 and <= 66:
                //object.on.water, object.on.land, object.on.anything
                //warn if used on ego
                if (ArgVal[0] == 0) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5082);
                        break;
                    }
                }
                break;
            case 69:
                //distance(oA, oB, vC)
                //variable is third arg
                //variable arg should not be a reserved Value
                if (ArgVal[2] <= 26) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 73 or 75:
                //end.of.loop, reverse.loop
                //flag arg should not be a reserved Value
                if (ArgVal[1] <= 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 81:
                //move.obj(oA, X,Y,STEP,fDONE)
                //validate the target position
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4127";
                        errInfo.Text = LoadResString(4127);
                        return false;
                    case leMedium:
                        AddWarning(5062);
                        break;
                    }
                }
                //check for ego object
                if (ArgVal[0] == 0) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5045);
                        break;
                    }
                }
                //flag arg should not be a reserved Value
                if (ArgVal[4] <= 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 82:
                //move.obj.v
                //flag arg should not be a reserved Value
                if (ArgVal[4] <= 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 83:
                //follow.ego(oA, DISTANCE, fDONE)
                //validate distance value
                if (ArgVal[1] <= 1) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5102);
                        break;
                    }
                }
                //check for ego object
                if (ArgVal[0] == 0) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5027);
                        break;
                    }
                }
                //flag arg should not be a reserved Value
                if (ArgVal[2] <= 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                //check for read only reserved flags
                CheckResFlagUse(ArgVal[2]);
                break;
            case 86:
                //set.dir(oA, vB)
                //check for ego object
                if (ArgVal[0] == 0) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5026);
                        break;
                    }
                }
                break;
            case 87:
                //get.dir (oA, vB)
                //variable is second arg
                //variable arg should not be a reserved Value
                if (ArgVal[1] <= 26) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 90:
                //block(x1,y1,x2,y2)
                //validate that all are within bounds, and that x1<=x2 and y1<=y2
                //also check that
                if (ArgVal[0] > 159 || ArgVal[1] > 167 || ArgVal[2] > 159 || ArgVal[3] > 167) {
                    //bad number
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4129";
                        errInfo.Text = LoadResString(4129);
                        return false;
                    case leMedium:
                        AddWarning(5020);
                        break;
                    }
                }
                if ((ArgVal[2] - ArgVal[0] < 2) || (ArgVal[3] - ArgVal[1] < 2)) {
                    //won't work
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4129";
                        errInfo.Text = LoadResString(4129);
                        return false;
                    case leMedium:
                        AddWarning(5051);
                        break;
                    }
                }
                break;
            case 98:
                //load.sound(A)
                //validate the sound exists
                if (!compGame.agSnds.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4130";
                        errInfo.Text = LoadResString(4130).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    case leMedium:
                        AddWarning(5014);
                        break;
                    }
                }
                break;
            case 99:
                //sound(A, fB)
                //validate the sound exists
                if (!compGame.agSnds.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4137";
                        errInfo.Text = LoadResString(4137).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    case leMedium:
                        AddWarning(5084);
                        break;
                    }
                }
                //flag arg should not be a reserved Value
                if (ArgVal[1] <= 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                //check for read only reserved flags
                CheckResFlagUse(ArgVal[1]);
                break;
            case 103:
                //display(ROW,COL,mC)
                //check row/col against limits
                if (ArgVal[0] > 24 || ArgVal[1] > 39) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4131";
                        errInfo.Text = LoadResString(4131);
                        return false;
                    case leMedium:
                        AddWarning(5059);
                        break;
                    }
                }
                break;
            case 105:
                //clear.lines(TOP,BTM,C)
                //top must be >btm; both must be <=24
                if (ArgVal[0] > 24 || ArgVal[1] > 24 || ArgVal[0] > ArgVal[1]) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4132";
                        errInfo.Text = LoadResString(4132);
                        return false;
                    case leMedium:
                        AddWarning(5011);
                        break;
                    }
                }
                //color value should be 0 or 15 //(but it doesn't hurt to be anything else)
                if (ArgVal[2] > 0 && ArgVal[2] != 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5100);
                        break;
                    }
                }
                break;
            case 109:
                //set.text.attribute(A,B)
                //should be limited to valid color values (0-15)
                if (ArgVal[0] > 15 || ArgVal[1] > 15) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4133";
                        errInfo.Text = LoadResString(4133);
                        return false;
                    case leMedium:
                        AddWarning(5029);
                        break;
                    }
                }
                break;
            case 110:
                //shake.screen(A)
                //shouldn't normally have more than a few shakes; zero is BAD
                if (ArgVal[0] == 0) {
                    //error!
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        errInfo.ID = "4134";
                        errInfo.Text = LoadResString(4134);
                        return false;
                    }
                }
                else if (ArgVal[0] > 15) {
                    //could be a palette change?
                    if (ArgVal[0] >= 100 && ArgVal[0] <= 109) {
                        //separate warning
                        switch (ErrorLevel) {
                        case leHigh or leMedium:
                            AddWarning(5058);
                            break;
                        }
                    }
                    else {
                        //warning
                        switch (ErrorLevel) {
                        case leHigh or leMedium:
                            AddWarning(5057);
                            break;
                        }
                    }
                }
                break;
            case 111:
                //configure.screen(TOP,INPUT,STATUS)
                //top should be <=3
                //input and status should not be equal
                //input and status should be <top or >=top+21
                if (ArgVal[0] > 3) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4135";
                        errInfo.Text = LoadResString(4135);
                        return false;
                    case leMedium:
                        AddWarning(5044);
                        break;
                    }
                }
                if (ArgVal[1] > 24 || ArgVal[2] > 24) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5099);
                        break;
                    }
                }
                if (ArgVal[1] == ArgVal[2]) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5048);
                        break;
                    }
                }
                if ((ArgVal[1] >= ArgVal[0] && ArgVal[1] <= ArgVal[0] + 20) || (ArgVal[2] >= ArgVal[0] && ArgVal[2] <= ArgVal[0] + 20)) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5049);
                        break;
                    }
                }
                break;
            case 114:
                //set.string(sA, mB)
                //warn user if setting input prompt to unusually long value
                if (ArgVal[0] == 0) {
                    if (strMsg[ArgVal[1]].Length > 10) {
                        switch (ErrorLevel) {
                        case leHigh or leMedium:
                            AddWarning(5096);
                            break;
                        }
                    }
                }
                break;
            case 115:
                //get.string(sA, mB, ROW,COL,LEN)
                //if row>24, both row/col are ignored; if col>39, gets weird; len is limited automatically to <=40
                if (ArgVal[2] > 24) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5052);
                        break;
                    }
                }
                if (ArgVal[3] > 39) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4004";
                        errInfo.Text = LoadResString(4004);
                        return false;
                    case leMedium:
                        AddWarning(5080);
                        break;
                    }
                }
                if (ArgVal[4] > 40) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5056);
                        break;
                    }
                }
                break;
            case 121:
                //set.key(A,B,cC)
                //controller number limit checked in GetNextArg function
                //increment controller Count
                intCtlCount++;
                //must be ascii or key code, (Arg0 can be 1 to mean joystick)
                if (ArgVal[0] > 0 && ArgVal[1] > 0 && ArgVal[0] != 1) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4154";
                        errInfo.Text = LoadResString(4154);
                        return false;
                    case leMedium:
                        AddWarning(5065);
                        break;
                    }
                }
                //check for improper ASCII assignments
                if (ArgVal[1] == 0) {
                    if (ArgVal[0] == 8 || ArgVal[0] == 13 || ArgVal[0] == 32) {
                        //ascii codes for bkspace, enter, spacebar
                        //bad
                        switch (ErrorLevel) {
                        case leHigh:
                            errInfo.ID = "4155";
                            errInfo.Text = LoadResString(4155);
                            return false;
                        case leMedium:
                            AddWarning(5066);
                            break;
                        }
                    }
                }
                //check for improper KEYCODE assignments
                if (ArgVal[0] == 0) {
                    if ((ArgVal[1] >= 71 && ArgVal[1] <= 73) ||
                        (ArgVal[1] >= 75 && ArgVal[1] <= 77) ||
                        (ArgVal[1] >= 79 && ArgVal[1] <= 83)) {
                        //ascii codes - bad
                        switch (ErrorLevel) {
                        case leHigh:
                            errInfo.ID = "4155";
                            errInfo.Text = LoadResString(4155);
                            return false;
                        case leMedium:
                            AddWarning(5066);
                            break;
                        }
                    }
                }
                break;
            case 122:
                //add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
                //VIEW, LOOP + CEL must exist
                //CEL width must be >=3
                //x,y must be within limits
                //PRI must be 0, or >=3 AND <=15
                //MGN must be 0-3, or >3 (ha ha, or ANY value...)
                //validate view
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4138";
                        errInfo.Text = LoadResString(4138).Replace(ARG1, (ArgVal[0]).ToString());
                        return false;
                    case leMedium:
                        AddWarning(5064);
                        //dont need to check loops or cels
                        blnWarned = true;
                        break;
                    }
                }
                if (!blnWarned) {
                    //try to load view to test loop + cel
                    try {
                        blnUnload = !compGame.agViews[ArgVal[0]].Loaded;
                        //if error trying to get loaded status, ignore for now
                        //it'll show up again when trying to load or access
                        //loop property and be handled there
                        if (blnUnload) {
                            compGame.agViews[ArgVal[0]].Load();
                            // ignore any errors/warnings
                        }
                        //validate loop
                        if (ArgVal[1] >= compGame.agViews[ArgVal[0]].Loops.Count) {
                            switch (ErrorLevel) {
                            case leHigh:
                                errInfo.ID = "4139";
                                errInfo.Text = LoadResString(4139).Replace(ARG1, ArgVal[1].ToString()).Replace(ARG2, ArgVal[0].ToString());
                                if (blnUnload) {
                                    compGame.agViews[ArgVal[0]].Unload();
                                }
                                return false;
                            case leMedium:
                                AddWarning(5085);
                                //dont need to check cel
                                blnWarned = true;
                                break;
                            }
                        }
                        //if loop was valid, check cel
                        if (!blnWarned) {
                            //validate cel
                            if (ArgVal[2] >= compGame.agViews[ArgVal[0]].Loops[ArgVal[1]].Cels.Count) {
                                switch (ErrorLevel) {
                                case leHigh:
                                    errInfo.ID = "4140";
                                    errInfo.Text = LoadResString(4140).Replace(ARG1, ArgVal[2].ToString()).Replace(ARG2, ArgVal[1].ToString()).Replace(ARG3, ArgVal[0].ToString());
                                    if (blnUnload) {
                                        compGame.agViews[ArgVal[0]].Unload();
                                    }
                                    return false;
                                case leMedium:
                                    AddWarning(5086);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception) {
                        //can't load the view; add a warning
                        AddWarning(5021, LoadResString(5021).Replace(ARG1, ArgVal[0].ToString()));
                    }
                    if (blnUnload) {
                        compGame.agViews[ArgVal[0]].Unload();
                    }
                }
                // x,y must be within limits
                if (ArgVal[3] > 159 || ArgVal[4] > 167) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4141";
                        errInfo.Text = LoadResString(4141);
                        return false;
                    case leMedium:
                        AddWarning(5038);
                        break;
                    }
                }
                //PRI should be <=15
                if (ArgVal[5] > 15) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4142";
                        errInfo.Text = LoadResString(4142);
                        return false;
                    case leMedium:
                        AddWarning(5079);
                        break;
                    }
                }
                //PRI should be 0 OR >=4 (but doesn't raise an error; only a warning)
                if (ArgVal[5] < 4 && ArgVal[5] != 0) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5079);
                        break;
                    }
                }
                //MGN values >15 will only use lower nibble
                if (ArgVal[6] > 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5101);
                        break;
                    }
                }
                break;
            case 129:
                //show.obj(VIEW)
                //validate view
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4144";
                        errInfo.Text = LoadResString(4144).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    case leMedium:
                        AddWarning(5061);
                        break;
                    }
                }
                break;
            case 127 or 176 or 178:
                //init.disk, hide.mouse, show.mouse
                switch (ErrorLevel) {
                case leHigh or leMedium:
                    AddWarning(5087, LoadResString(5087).Replace(ARG1, ActionCommands[CmdNum].Name));
                    break;
                }
                break;
            case 175 or 179 or 180:
                //discard.sound, fence.mouse, mouse.posn
                switch (ErrorLevel) {
                case leHigh:
                    errInfo.ID = "4152";
                    errInfo.Text = LoadResString(4152).Replace(ARG1, ActionCommands[CmdNum].Name);
                    return false;
                case leMedium:
                    AddWarning(5088, LoadResString(5088).Replace(ARG1, ActionCommands[CmdNum].Name));
                    break;
                }
                break;
            case 130:
                //random(LOWER,UPPER,vRESULT)
                //lower should be < upper
                if (ArgVal[0] > ArgVal[1]) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4145";
                        errInfo.Text = LoadResString(4145);
                        return false;
                    case leMedium:
                        AddWarning(5054);
                        break;
                    }
                }
                //lower=upper means result=lower=upper
                if (ArgVal[0] == ArgVal[1]) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5106);
                        break;
                    }
                }
                //if lower=upper+1, means div by 0!
                if (ArgVal[0] == ArgVal[1] + 1) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4158";
                        errInfo.Text = LoadResString(4158);
                        return false;
                    case leMedium:
                        AddWarning(5107);
                        break;
                    }
                }
                //variable arg should not be a reserved Value
                if (ArgVal[2] <= 26) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 142:
                //script.size
                //raise warning/error if in other than logic0
                if (bytLogComp != 0) {
                    //warn
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5039);
                        break;
                    }
                }
                //check for absurdly low Value for script size
                if (ArgVal[0] < 10) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5009);
                        break;
                    }
                }
                break;
            case 147:
                //reposition.to(oA, B,C)
                //validate the new position
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4128";
                        errInfo.Text = LoadResString(4128);
                        return false;
                    case leMedium:
                        AddWarning(5023);
                        break;
                    }
                }
                break;
            case 150:
                //trace.info(LOGIC,ROW,HEIGHT)
                //logic must exist
                //row + height must be <22
                //height must be >=2 (but interpreter checks for this error)

                //validate that logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4153";
                        errInfo.Text = LoadResString(4153).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    case leMedium:
                        AddWarning(5040);
                        break;
                    }
                }
                //validate that height is not too small
                if (ArgVal[2] < 2) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5046);
                        break;
                    }
                }
                //validate size of window
                if (ArgVal[1] + ArgVal[2] > 23) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4146";
                        errInfo.Text = LoadResString(4146);
                        return false;
                    case leMedium:
                        AddWarning(5063);
                        break;
                    }
                }
                break;
            case 151 or 152:
                //Print.at(mA, ROW, COL, MAXWIDTH), print.at.v
                //row <=22
                //col >=2
                //maxwidth <=36
                //maxwidth=0 defaults to 30
                //maxwidth=1 crashes AGI
                //col + maxwidth <=39
                if (ArgVal[1] > 22) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4147";
                        errInfo.Text = LoadResString(4147);
                        return false;
                    case leMedium:
                        AddWarning(5067);
                        break;
                    }
                }
                switch (ArgVal[3]) {
                case 0:
                    //maxwidth=0 defaults to 30
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5105);
                        break;
                    }
                    break;
                case 1: //maxwidth=1 crashes AGI
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4043";
                        errInfo.Text = LoadResString(4043);
                        return false;
                    case leMedium:
                        AddWarning(5103);
                        break;
                    }
                    break;
                default:
                    if (ArgVal[3] > 36) { //maxwidth >36 won't work
                        switch (ErrorLevel) {
                        case leHigh:
                            errInfo.ID = "4043";
                            errInfo.Text = LoadResString(4043);
                            return false;
                        case leMedium:
                            AddWarning(5104);
                            break;
                        }
                    }
                    break;
                }
                //col>2 and col + maxwidth <=39
                if (ArgVal[2] < 2 || ArgVal[2] + ArgVal[3] > 39) {
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4148";
                        errInfo.Text = LoadResString(4148);
                        return false;
                    case leMedium:
                        AddWarning(5068);
                        break;
                    }
                }
                break;
            case 154:
                //clear.text.rect(R1,C1,R2,C2,COLOR)
                //if (either row argument is >24,
                //or either column argument is >39,
                //or R2 < R1 or C2 < C1,
                //the results are unpredictable
                if (ArgVal[0] > 24 || ArgVal[1] > 39 ||
                   ArgVal[2] > 24 || ArgVal[3] > 39 ||
                   ArgVal[2] < ArgVal[0] || ArgVal[3] < ArgVal[1]) {
                    //invalid items
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4150";
                        errInfo.Text = LoadResString(4150);
                        return false;
                    case leMedium:
                        //if due to pos2 < pos1
                        if (ArgVal[2] < ArgVal[0] || ArgVal[3] < ArgVal[1]) {
                            AddWarning(5069);
                        }
                        //if due to variables outside limits
                        if (ArgVal[0] > 24 || ArgVal[1] > 39 ||
                           ArgVal[2] > 24 || ArgVal[3] > 39) {
                            AddWarning(5070);
                        }
                        break;
                    }
                }
                //color value should be 0 or 15 //(but it doesn't hurt to be anything else)
                if (ArgVal[4] > 0 && ArgVal[4] != 15) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5100);
                        break;
                    }
                }
                break;
            case 158:
                //submit.menu()
                //should only be called in logic0
                //raise warning if in other than logic0
                if (bytLogComp != 0) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        //set warning
                        AddWarning(5047);
                        break;
                    }
                }
                break;
            case 174:
                //set.pri.base(A)
                //calling set.pri.base with Value >167 doesn't make sense
                if (ArgVal[0] > 167) {
                    switch (ErrorLevel) {
                    case leHigh or leMedium:
                        AddWarning(5071);
                        break;
                    }
                }
                break;
            }
            //success
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LabelName"></param>
        /// <returns></returns>
        static internal int LabelNum(string LabelName) {
            //this function will return the number of the label passed
            //as a string, or zero, if a match is not found
            int i;
            //step through all labels,
            for (i = 0; i < bytLabelCount; i++) {
                if (llLabel[i].Name == LabelName) {
                    return i;
                }
            }
            //if not found
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CmdNum"></param>
        /// <param name="ArgVal"></param>
        /// <returns></returns>
        static internal bool ValidateIfArgs(int CmdNum, ref byte[] ArgVal) {
            //check for specific command issues
            switch (CmdNum) {
            //case 9: //has (iA)
            //case 10: //obj.in.room(iA, vB)
            //         //invobj number validated in GetNextArg function
            case 11 or 16 or 17 or 18:
                //posn(oA, X1, Y1, X2, Y2)
                //obj.in.box(oA, X1, Y1, X2, Y2)
                //center.posn(oA, X1, Y1, X2, Y2)
                //right.posn(oA, X1, Y1, X2, Y2)

                //screenobj number validated in GetNextArg function

                //validate that all are within bounds, and that x1<=x2 and y1<=y2
                if (ArgVal[1] > 159 || ArgVal[2] > 167 || ArgVal[3] > 159 || ArgVal[4] > 167) {
                    //bad number
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4151";
                        errInfo.Text = LoadResString(4151);
                        return false;
                    case leMedium:
                        AddWarning(5072);
                        break;
                    }
                }
                if ((ArgVal[1] > ArgVal[3]) || (ArgVal[2] > ArgVal[4])) {
                    //won't work
                    switch (ErrorLevel) {
                    case leHigh:
                        errInfo.ID = "4151";
                        errInfo.Text = LoadResString(4151);
                        return false;
                    case leMedium:
                        AddWarning(5073);
                        break;
                    }
                }
                break;
                //case 12: //controller (cA)
                //         //has controller been assigned?
                //         //not sure how to check it; calls to controller cmd may
                //         //occur in logics that are compiled before the logic that sets
                //         //them up...
                //    break;
                //case 14: //said()
                //         // nothing to check
                //    break;
                //case 15: //compare.strings(sA, sB)
                //         // nothing to check
                //    break;
            }
            //success
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMsg"></param>
        /// <param name="MsgNum"></param>
        static internal void ValidateMsgChars(string strMsg, int MsgNum) {
            // raise warning if non-standard characters in the message string

            //if LOW error detection, nothing to check
            if (ErrorLevel == leLow) {
                return;
            }
            //extended characters
            if (strMsg.Any(ch => ch > 127)) {
                switch (ErrorLevel) {
                case leHigh or leMedium:
                    AddWarning(5094);
                    //need to track warning in case this msg is
                    //also included in body of logic
                    intMsgWarn[MsgNum] |= 2;
                    break;
                }
            }
            // no other warnings needed
            return;
        }

        /// <summary>
        /// Writes the messages for a logic at the end of the resource.
        /// Messages are encrypted with the string 'Avis Durgan'. No gaps
        /// are allowed, so messages that are skipped must be included as
        /// zero length messages.
        /// </summary>
        /// <returns></returns>
        static internal bool WriteMsgs() {
            //this function will 
            int lngMsgSecStart;
            int[] lngMsgPos = new int[256];
            int intCharPos;
            byte bytCharVal;
            byte[] strMessage = [];
            int lngMsg;
            int lngMsgCount;
            int lngCryptStart;
            int lngMsgLen;
            int i;
            string strHex;
            bool blnSkipChar = false;
            //calculate start of message section
            lngMsgSecStart = tmpLogRes.Size;
            //find last message by counting backwards until a msg is found
            lngMsgCount = 256;
            do {
                lngMsgCount--;
            } while (!blnMsg[lngMsgCount] && (lngMsgCount != 0));

            //write msg Count,
            tmpLogRes.WriteByte((byte)lngMsgCount);
            //write place holder for msg end
            tmpLogRes.WriteWord(0);
            //write place holders for msg pointers
            for (i = 0; i < lngMsgCount; i++) {
                tmpLogRes.WriteWord(0);
            }
            //begin encryption process
            lngCryptStart = tmpLogRes.Size;
            for (lngMsg = 1; lngMsg <= lngMsgCount; lngMsg++) {
                //get length
                lngMsgLen = strMsg[lngMsg].Length;
                //if msg is used
                if (blnMsg[lngMsg]) {
                    //calculate offset to start of this message (adjust by one byte, which
                    //is the byte that indicates how many msgs there are)
                    lngMsgPos[lngMsg] = tmpLogRes.Pos - (lngMsgSecStart + 1);
                }
                else {
                    //need to write a null value for offset; (when it gets added after all
                    //messages are written it gets set to the beginning of message section
                    // ( a relative offset of zero here)
                    lngMsgPos[lngMsg] = 0;
                }
                if (lngMsgLen > 0) {
                    // convert to byte array based on codepage
                    strMessage = game.agCodePage.GetBytes(strMsg[lngMsg]);
                    // step through all characters in this msg
                    intCharPos = 0;
                    while (intCharPos < strMessage.Length) // Until intCharPos > Len(strMsg(lngMsg))
                     {                                          //get ascii code for this character
                        bytCharVal = strMessage[intCharPos];
                        //check for invalid codes (8,9,10,13)
                        switch (bytCharVal) {
                        case 8 or 9:
                            // convert these to space (' ') to avoid trouble
                            bytCharVal = 32;
                            break;
                        //case 10: // OK
                        case 13:
                            // new line
                            bytCharVal = 10;
                            break;
                        case 92: // '\'
                                 //check next character for special codes
                            if (intCharPos < lngMsgLen - 1) {
                                switch (strMessage[intCharPos + 1]) {
                                case 110:
                                    //  '\n' = new line
                                    bytCharVal = 0xA;
                                    intCharPos++;
                                    break;
                                case 34:
                                    //dbl quote(")
                                    // '\"' = quote mark
                                    bytCharVal = 0x22;
                                    intCharPos++;
                                    break;
                                case 92:
                                    // '\\' = \
                                    bytCharVal = 0x5C;
                                    intCharPos++;
                                    break;
                                case 120:
                                    // '\x'  //look for a hex value
                                    //make sure at least two more characters
                                    if (intCharPos < lngMsgLen - 3) {
                                        //get next 2 chars and hexify them
                                        strHex = "0x" + strMessage[(intCharPos + 2)..(intCharPos + 4)];
                                        //if this hex value >=1 and <256, use it
                                        try {
                                            i = Convert.ToInt32(strHex, 16);
                                            if (i >= 1 && i < 256) {
                                                bytCharVal = (byte)i;
                                                intCharPos += 3;
                                            }
                                        }
                                        catch (Exception) {
                                            // drop the slash if there's an error
                                            blnSkipChar = true;
                                        }
                                    }
                                    break;
                                default:
                                    //if no special char found, the single slash should be dropped
                                    blnSkipChar = true;
                                    break;
                                }
                            }
                            else {
                                //if the '\' is the last char, skip it
                                blnSkipChar = true;
                            }
                            break;
                        }
                        //write the encrypted byte (need to adjust for previous messages, and current position)
                        if (!blnSkipChar) {
                            tmpLogRes.WriteByte((byte)(bytCharVal ^ bytEncryptKey[(tmpLogRes.Pos - lngCryptStart) % 11]));
                        }
                        //increment pointer
                        intCharPos++;
                        //reset skip flag
                        blnSkipChar = false;
                    }
                }
                //if msg was used, add trailing zero to terminate message
                //(if msg was zero length, we still need this terminator)
                if (blnMsg[lngMsg]) {
                    tmpLogRes.WriteByte((byte)(0 ^ bytEncryptKey[(tmpLogRes.Pos - lngCryptStart) % 11]));
                }
            }
            //calculate length of msgs, and write it at beginning
            //of msg section (adjust by one byte, which is the
            //byte that indicates number of msgs written)
            tmpLogRes.WriteWord((ushort)(tmpLogRes.Pos - (lngMsgSecStart + 1)), lngMsgSecStart + 1);
            //write msg section start Value at beginning of resource
            //(correct by two so it gives position relative to byte 7 of
            //the logic resource header - see procedure 'DecodeLogic' for details)
            tmpLogRes.WriteWord((ushort)(lngMsgSecStart - 2), 0);
            //write all the msg pointers (start with msg1 since msg0
            // is never allowed)
            for (lngMsg = 1; i <= lngMsgCount; i++) {
                tmpLogRes.WriteWord((ushort)lngMsgPos[lngMsg], lngMsgSecStart + 1 + lngMsg * 2);
            }
            //success
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        static internal void ReadLabels() {
            // this function steps through the source code to identify all
            // valid labels; we need to find them all before starting the
            // compile so we can correctly set jumps
            //
            // fanAGI syntax is either 'label:' or ':label', with nothing
            // else in front of or after the label declaration
            // in Sierra syntax, only ':label' is allowed
            // TODO: improve handling of improper labels; 
            // when spaces between the ':' and the label, give a more
            // descriptive error code
            byte i;
            string strLabel;

            //reset counter
            bytLabelCount = 0;
            //reset compiler to first input line
            ResetCompiler();
            do {
                //look for label name by searching for colon character
                if (strCurrentLine.Contains(':')) {
                    strLabel = strCurrentLine;
                    //check for 'label:' (FanAGI syntax only)
                    if (strLabel[^1] == ':' && !agSierraSyntax) {
                        strLabel = strLabel[..^1];
                        // no spaces allowed between : and label name
                        if (strLabel[0] == ' ') {
                            // ignore it here; error will be caught in main compiler
                            strLabel = "";
                        }
                    }
                    // check for ':label'
                    else if (strLabel[0] == ':') {
                        strLabel = strLabel[1..];
                        // no spaces allowed between label name and :
                        if (strLabel[^1] == ' ') {
                            // ignore it here; error will be caught in main compiler
                            strLabel = "";
                        }
                    }
                    else {
                        //not a label
                        strLabel = "";
                    }
                    // Sierra syntax allows optional ';' at end of label
                    if (agSierraSyntax) {
                        if (strLabel[^1] == ';') {
                            // remove it (and any trailing spaces)             
                            strLabel = strLabel[..^1].Trim();
                        }
                    }
                    //if a label was found, validate it
                    if (strLabel.Length != 0) {
                        //make sure enough room
                        if (bytLabelCount >= MAX_LABELS) {
                            AddMinorError(4109, LoadResString(4109).Replace(ARG1, MAX_LABELS.ToString()));
                            // ignore it
                            strLabel = "";
                        }
                    }
                    if (strLabel.Length != 0) {
                        DefineNameCheck chkLabel = ValidateDefName(strLabel);
                        // check for error
                        switch (chkLabel) {
                        case ncNumeric:
                            //numbers are ok for labels
                            break;
                        case ncEmpty:
                            AddMinorError(4096);
                            break;
                        case ncActionCommand:
                            AddMinorError(4025, LoadResString(4025).Replace(ARG1, strLabel));
                            break;
                        case ncTestCommand:
                            AddMinorError(4026, LoadResString(4026).Replace(ARG1, strLabel));
                            break;
                        case ncKeyWord:
                            AddMinorError(4028, LoadResString(4028).Replace(ARG1, strLabel));
                            break;
                        case ncArgMarker:
                            AddMinorError(4091);
                            break;
                        case ncGlobal:
                            AddMinorError(4024, LoadResString(4024).Replace(ARG1, strLabel));
                            break;
                        case ncReservedVar:
                            AddMinorError(4033, LoadResString(4033).Replace(ARG1, strLabel));
                            break;
                        case ncReservedFlag:
                            AddMinorError(4030, LoadResString(4030).Replace(ARG1, strLabel));
                            break;
                        case ncReservedNum:
                            AddMinorError(4029, LoadResString(4029).Replace(ARG1, strLabel));
                            break;
                        case ncReservedObj or ncReservedStr:
                            AddMinorError(4032, LoadResString(4032).Replace(ARG1, strLabel));
                            break;
                        case ncReservedMsg:
                            AddMinorError(4031, LoadResString(4031).Replace(ARG1, strLabel));
                            break;
                        case ncBadChar:
                            AddMinorError(4068);
                            break;
                        }
                        //check against existing labels
                        for (i = 0; i < bytLabelCount; i++) {
                            if (strLabel == llLabel[i].Name) {
                                errInfo.ID = "4027";
                                errInfo.Text = LoadResString(4027).Replace(ARG1, strLabel);
                                break;
                            }
                        }
                        //increment number of labels, and save
                        bytLabelCount++;
                        llLabel[bytLabelCount - 1].Name = strLabel;
                        llLabel[bytLabelCount - 1].Loc = 0;
                    }
                }
                //get next line
                IncrementLine();
            } while (lngLine != -1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static internal bool CompileAGI() {
            //main compiler function
            //steps through input one command at a time and converts it
            //to AGI logic code
            //Note that we don't need to set blnCriticalError flag here;
            //an error will cause this function to return a Value of false
            //which causes the compiler to display error info
            const int MAXGOTOS = 255;
            string nextToken, prevToken = "", strArg;
            int i, tmpVal, intCmdNum, BlockDepth = 0, intLabelNum;
            int NumGotos = 0, GotoData, CurGoto, lngReturnLine = 0;
            bool blnLastCmdRtn = false, blnArgsOK;
            byte[] bytArg = new byte[8];
            LogicGoto[] Gotos = new LogicGoto[MAXGOTOS + 1];
            BlockType[] Block = new BlockType[256];

            //reset compiler
            ResetCompiler();
            //get first command
            nextToken = NextToken();
            //process tokens in the input string list until finished
            while (lngLine != -1) {
                //reset last command flag
                blnLastCmdRtn = false;
                lngReturnLine = 0;
                //process the command
                switch (nextToken) {
                case "{":
                    //can't have a "{" token, unless it follows an 'if' or 'else'
                    if (prevToken != "if" && prevToken != "else") {
                        AddMinorError(4008);
                    }
                    break;
                case "}":
                    //if last command was a new.room command, then closing block is expected
                    if (blnNewRoom) {
                        blnNewRoom = false;
                    }
                    //if no block currently open,
                    if (BlockDepth == 0) {
                        AddMinorError(4010);
                    }
                    else {
                        //if last position in resource is two bytes from start of block
                        if (tmpLogRes.Size == Block[BlockDepth].StartPos + 2) {
                            switch (ErrorLevel) {
                            case leHigh:
                                AddMinorError(4049);
                                break;
                            case leMedium:
                                //set warning
                                AddWarning(5001);
                                break;
                            }
                        }
                        //calculate and write block length
                        Block[BlockDepth].Length = tmpLogRes.Size - Block[BlockDepth].StartPos - 2;
                        tmpLogRes.WriteWord((ushort)Block[BlockDepth].Length, Block[BlockDepth].StartPos);
                        //remove block from stack
                        BlockDepth--;
                    }
                    break;
                case "if":
                    //compile the //if// statement
                    if (!CompileIf()) {
                        return false;
                    }
                    //if block stack exceeded
                    if (BlockDepth >= MAX_BLOCK_DEPTH) {
                        blnCriticalError = true;
                        errInfo.ID = "4110";
                        errInfo.Text = LoadResString(4110).Replace(ARG1, MAX_BLOCK_DEPTH.ToString());
                        return false;
                    }
                    // add block to stack
                    BlockDepth++;
                    Block[BlockDepth].StartPos = tmpLogRes.Pos;
                    Block[BlockDepth].IsIf = true;
                    // write placeholder for block length
                    tmpLogRes.WriteWord(0);
                    // next token should be a bracket
                    nextToken = NextToken();
                    if (!CheckChar('{')) {
                        // error
                        AddMinorError(4053);
                        // if a stray ')' just ignore it
                        if (CheckChar(')')) {
                            // and check for bracket
                            _ = CheckChar('{');
                        }
                    }
                    break;
                case "else":
                    //else can only follow a close bracket
                    if (prevToken != "}") {
                        AddMinorError(4011);
                        // if there is a block, assume there was a '}'
                        // and remove it (by continuing to next statement)
                    }
                    //if the block closed by that bracket was an 'else'
                    //(which will be determined by having that block's IsIf flag NOT being set),
                    if (!Block[BlockDepth + 1].IsIf) {
                        errInfo.ID = "4083";
                        errInfo.Text = LoadResString(4083);
                        return false;
                    }
                    //adjust blockdepth to the 'if' command
                    //directly before this 'else'
                    BlockDepth++;
                    //adjust previous block length to accomodate the 'else' statement
                    Block[BlockDepth].Length += 3;
                    tmpLogRes.WriteWord((ushort)Block[BlockDepth].Length, Block[BlockDepth].StartPos);
                    // previous 'if' block is now closed; use same block level
                    // for this 'else' block
                    Block[BlockDepth].IsIf = false;
                    //write the 'else' code
                    tmpLogRes.WriteByte(0xFE);
                    Block[BlockDepth].StartPos = tmpLogRes.Pos;
                    // block length filled in later
                    tmpLogRes.WriteWord(0);
                    //next command better be a bracket
                    if (!CheckChar('{')) {
                        //error
                        AddMinorError(4053);
                    }
                    break;
                case "goto":
                    //if last command was a new room, warn user
                    // TODO: fix check for newroom; if set, check very next 
                    // token; it should be '}' (or return?)
                    if (blnNewRoom) {
                        switch (ErrorLevel) {
                        case leHigh or leMedium:
                            //set warning
                            AddWarning(5095);
                            break;
                        }
                        blnNewRoom = false;
                    }
                    if (!agSierraSyntax) {
                        //next command should be "("
                        if (!CheckChar('(')) {
                            AddMinorError(4001);
                        }
                    }
                    //get goto argument
                    strArg = NextToken();
                    //if argument is NOT a valid label
                    if (LabelNum(strArg) == -1) {
                        errInfo.ID = "4074";
                        errInfo.Text = LoadResString(4074).Replace(ARG1, strArg);
                        //blnCriticalError = false; //why not set error flag?
                        return false;
                    }
                    //if too many gotos
                    if (NumGotos < MAXGOTOS) {
                        //save this goto info on goto stack
                        NumGotos++;
                        Gotos[NumGotos - 1].LabelNum = (byte)LabelNum(strArg);
                        //write goto command byte
                        tmpLogRes.WriteByte(0xFE);
                        Gotos[NumGotos].DataLoc = tmpLogRes.Pos;
                        //write placeholder for amount of offset
                        tmpLogRes.WriteWord(0);
                    }
                    else {
                        AddMinorError(4108, LoadResString(4108).Replace(ARG1, MAXGOTOS.ToString()));
                    }
                    if (agSierraSyntax) {
                        // Sierra syntax allows optional ';'
                        char tmp = NextChar(true);
                        if (tmp != (char)0 && tmp != ';') {
                            // something else is on the line that's not allowed
                            lngPos--;
                        }
                    }
                    else {
                        //next character should be ")"
                        if (NextChar() != ')') {
                            AddMinorError(4003);
                        }
                    }
                    //verify next command is end of line (;)
                    CheckForEOL();
                    break;
                case "/*" or "*/":
                    //since block commands are no longer supported, check for markers in order to provide a
                    //meaningful error message
                    AddMinorError(4052);
                    break;
                case "++":
                case "--": //unary operators; need to get a variable next
                           //write the command code
                    if (nextToken == "++") {
                        tmpLogRes.WriteByte(1);
                    }
                    else {
                        tmpLogRes.WriteByte(2);
                    }
                    //get the variable to update
                    strArg = NextToken();
                    //convert it
                    if (!ConvertArgument(ref strArg, atVar)) {
                        //error
                        AddMinorError(4046);
                        // use a temp placeholder
                        strArg = "v255";
                    }
                    //get Value
                    intCmdNum = VariableValue(strArg);
                    if (intCmdNum == -1) {
                        AddMinorError(4066, LoadResString(4066).Replace(ARG1, ""));
                        // use a temp placeholder
                        intCmdNum = 0;
                    }
                    //write the variable value
                    tmpLogRes.WriteByte((byte)intCmdNum);
                    //verify next command is end of line ';'
                    CheckForEOL();
                    break;
                case ":":
                    //alternate label syntax
                    // next command should be the label
                    nextToken = NextToken();
                    intLabelNum = LabelNum(nextToken);
                    //if not a valid label
                    if (intLabelNum == 0) {
                        // value of zero used as placeholder
                        // for all label errors
                        AddMinorError(4076);
                    }
                    //save position of label
                    llLabel[intLabelNum].Loc = tmpLogRes.Size;
                    // trailing ';' should be caught by CheckEOL
                    ////// Sierra syntax allows trailing ';'
                    ////if (agSierraSyntax) {
                    ////    char tmp = NextChar(true);
                    ////    if (tmp != (char)0 && tmp != ';') {
                    ////        // invalid tokens on rest of line
                    ////        //?? why no error?
                    ////        lngPos--;
                    ////    }
                    ////}
                    break;
                default:
                    //must be a label:, command, or special syntax
                    //if next character is a colon
                    if (strCurrentLine[lngPos + 1] == ':') {
                        //it might be a label
                        intLabelNum = LabelNum(nextToken);
                        //if not a valid label
                        if (intLabelNum == 0) {
                            AddMinorError(4076);
                        }
                        //save position of label
                        llLabel[intLabelNum].Loc = tmpLogRes.Size;
                        //read in next char to skip past the colon
                        _ = NextChar();
                    }
                    else {
                        //if last command was a new room (and not followed by return), warn user
                        if (blnNewRoom && nextToken != "return") {
                            switch (ErrorLevel) {
                            case leHigh or leMedium:
                                //set warning
                                AddWarning(5095);
                                break;
                            }
                            blnNewRoom = false;
                        }
                        //get  command opcode number
                        intCmdNum = CommandNum(false, nextToken);
                        //if command not found,
                        if (intCmdNum == 255) {
                            //try to parse special syntax
                            if (CompileSpecial(nextToken)) {
                                // unknown command - check for preprocessor symbol
                                if (nextToken[0] == '#' || nextToken[0] == '%') {
                                    AddMinorError(4061, LoadResString(4061).Replace(ARG1, nextToken));
                                }
                                else {
                                    // check for possible misspelled command
                                    if (PeekToken(true) == "(") {
                                        // assume bad cmd name; skip to ')'
                                        AddMinorError(4116, LoadResString(4116).Replace(ARG1, nextToken));
                                        do {
                                            nextToken = NextToken(true);
                                            if (nextToken == ")") {
                                                break;
                                            }
                                            if (nextToken == ";") {
                                                lngPos--;
                                                break;
                                            }
                                        } while (nextToken.Length > 0);
                                    }
                                    else {
                                        // unknown syntax error
                                        AddMinorError(4084);
                                        // skip to end of line
                                        lngPos = nextToken.Length;
                                    }
                                }
                            }
                        }
                        else {
                            //write the command code,
                            tmpLogRes.WriteByte((byte)intCmdNum);
                            //next character should be "("
                            if (!CheckChar('(')) {
                                AddMinorError(4048);
                            }
                            //reset the quotemark error flag
                            lngQuoteAdded = -1;
                            //now extract arguments (assume all OK)
                            blnArgsOK = true;
                            for (i = 0; i < ActionCommands[intCmdNum].ArgType.Length; i++) {
                                //after first argument, verify comma separates arguments
                                if (i > 0) {
                                    if (!CheckChar(',')) {
                                        //check for added quotes; they are usually the problem
                                        if (lngQuoteAdded >= 0) {
                                            //readjust error line;
                                            errInfo.Line = (lngLine - lngIncludeOffset + 1).ToString();
                                        }
                                        // use 1-base arg values (but since referring to
                                        // previous arg, don't increment arg index)
                                        AddMinorError(4047, LoadResString(4047).Replace(ARG1, i.ToString()));
                                    }
                                }
                                tmpVal = (byte)GetNextArg(ActionCommands[(byte)intCmdNum].ArgType[i], i);
                                if (tmpVal >= 0) {
                                    bytArg[i] = (byte)tmpVal;
                                }
                                else {
                                    switch (tmpVal) {
                                    case -1:
                                        // -1 => bad arg conversion
                                        AddMinorError(int.Parse(errInfo.ID), errInfo.Text);
                                        break;
                                    default:
                                        // -3 => ',' found
                                        // -2 => ')' found
                                        AddMinorError(4054, errInfo.Text.Replace(ARG2, Commands.agCmds[intCmdNum].Name));
                                        break;
                                    }
                                    // use a placeholder value
                                    bytArg[i] = 0;
                                    blnArgsOK = false;
                                }
                                //write argument
                                tmpLogRes.WriteByte(bytArg[i]);
                            }
                            if (blnArgsOK) {
                                //validate arguments for this command
                                if (!ValidateArgs(intCmdNum, ref bytArg)) {
                                    return false;
                                }
                            }
                            // next character must be ")"
                            if (!CheckChar(')')) {
                                // check for added quotes; they are the problem
                                if (lngQuoteAdded >= 0) {
                                    // reset error line;
                                    errInfo.Line = (lngQuoteAdded - lngIncludeOffset + 1).ToString();
                                }
                            }
                            if (intCmdNum == 0) {
                                blnLastCmdRtn = true;
                                // set line number
                                if (lngReturnLine == 0) {
                                    lngReturnLine = int.Parse(errInfo.Line);
                                }
                            }
                        }
                        //verify next command is end of line (;)
                        CheckForEOL();
                    }
                    break;
                }
                //get next command
                prevToken = nextToken;
                nextToken = NextToken();
            }
            // Sierra syntax adds return automatically
            if (agSierraSyntax) {
                tmpLogRes.WriteByte(0);
                // if a return was already coded, warn user
                if (blnLastCmdRtn) {
                    AddWarning(5112);
                }
            }
            else {
                if (!blnLastCmdRtn) {
                    switch (ErrorLevel) {
                    case leHigh:
                        //error
                        errInfo.ID = "4102";
                        errInfo.Text = LoadResString(4102);
                        //blnCriticalError = false; //why not set error flag?
                        return false;
                    case leMedium or leLow:
                        //add the missing return code
                        tmpLogRes.WriteByte(0);
                        //and a warning
                        AddWarning(5016);
                        break;
                    }
                }
            }
            //check to see if everything was wrapped up properly
            if (BlockDepth > 0) {
                errInfo.ID = "4009";
                errInfo.Text = LoadResString(4009);
                //reset errorline to return cmd
                errInfo.Line = (lngReturnLine + 1).ToString();
                return false;
            }
            //write in goto values
            for (CurGoto = 0; CurGoto <= NumGotos; CurGoto++) {
                GotoData = llLabel[Gotos[CurGoto].LabelNum].Loc - Gotos[CurGoto].DataLoc - 2;
                if (GotoData < 0) {
                    //need to convert it to an unsigned short Value
                    GotoData = 0x10000 + GotoData;
                }
                tmpLogRes.WriteWord((ushort)GotoData, Gotos[CurGoto].DataLoc);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMsgIn"></param>
        /// <returns></returns>
        static internal int MessageNum(string strMsgIn) {
            // returns the number of the message corresponding to
            //strMsg, or creates a new msg number if strMsg is not
            //currently a message
            //if maximum number of msgs assigned, returns  0
            int lngMsg;

            //blank msgs normally not allowed
            if (strMsgIn.Length == 0) {
                switch (ErrorLevel) {
                case leHigh or leMedium:
                    AddWarning(5074);
                    break;
                }
            }
            for (lngMsg = 1; lngMsg <= 255; lngMsg++) {
                //if this is the message (case-sensitive search)
                if (strMsg[lngMsg] == strMsgIn) {
                    //if null string found for first time, msg-in-use flag will be false
                    if (!blnMsg[lngMsg]) {
                        blnMsg[lngMsg] = true;
                    }
                    //if this msg has an extended char warning, repeat it here
                    if ((intMsgWarn[lngMsg] & 1) == 1) {
                        AddWarning(5093);
                    }
                    if ((intMsgWarn[lngMsg] & 2) == 2) {
                        AddWarning(5094);
                    }
                    //return this Value
                    return lngMsg;
                }
            }
            // msg isn't in list yet, find an empty spot
            for (lngMsg = 1; lngMsg <= 255; lngMsg++) {
                if (!blnMsg[lngMsg]) {
                    //this message is available
                    blnMsg[lngMsg] = true;
                    strMsg[lngMsg] = strMsgIn;
                    // validate it check for extended characters
                    ValidateMsgChars(strMsgIn, lngMsg);
                    return lngMsg;
                }
            }
            //if no room found, return zero
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blnIF"></param>
        /// <param name="strCmdName"></param>
        /// <returns></returns>
        static internal byte CommandNum(bool blnIF, string strCmdName) {
            //gets the command numberof a command, based on the text
            if (blnIF) {
                for (byte retval = 0; retval < agNumTestCmds; retval++) {
                    if (strCmdName == TestCommands[retval].Name) {
                        return retval;
                    }
                }
                // check defines
                for (int i = 0; i < lngDefineCount; i++) {
                    if (tdDefines[i].Type == atTestCmd) {
                        if (tdDefines[i].Name == strCmdName) {
                            return byte.Parse(tdDefines[i].Value);
                        }
                    }
                }
            }
            else {
                for (byte retval = 0; retval < agNumCmds; retval++) {
                    if (strCmdName == ActionCommands[retval].Name) {
                        return retval;
                    }
                }
                //maybe the command is a valid agi command, but
                //just not supported in this agi version
                for (byte retval = agNumCmds; retval < MAX_CMDS; retval++) {
                    if (strCmdName == ActionCommands[retval].Name) {
                        switch (ErrorLevel) {
                        case leHigh:
                            //error
                            AddMinorError(4065, LoadResString(4065).Replace(ARG1, strCmdName));
                            break;
                        case leMedium:
                            //add warning
                            AddWarning(5075, LoadResString(5075).Replace(ARG1, strCmdName));
                            break;
                        }
                        //  //don't worry about command validity; return the extracted command num
                        return retval;
                    }
                }
                // check defines
                for (int i = 0; i < lngDefineCount; i++) {
                    if (tdDefines[i].Type == atActionCmd) {
                        if (tdDefines[i].Name == strCmdName) {
                            return byte.Parse(tdDefines[i].Value);
                        }
                    }
                }
            }
            // not found
            return 255;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strArg1"></param>
        /// <param name="blnNOT"></param>
        /// <returns></returns>
        static internal bool CompileSpecialif(string strArg1, bool blnNOT) {
            //this funtion determines if strArg1 is a properly
            //formatted special test syntax
            //and writes the appropriate data to the resource

            //(proprerly formatted special IF syntax will be one of following:
            // v## expr v##
            // v## expr ##
            // f##
            // v##
            //
            // where ## is a number from 1-255
            // and expr is //"==", "!=", "<", ">=", "=>", ">", "<=", "=<"

            //none of the possible test commands in special syntax format need to be validated,
            //so no call to ValidateIfArgs
            string strArg2;
            int intArg1, intArg2;
            bool blnIsVar = true, blnArg2Var, blnAddNOT = false;
            byte bytCmdNum;
            //check for variable
            if (!ConvertArgument(ref strArg1, atVar)) {
                blnIsVar = false;
                // check for flag argument
                if (ConvertArgument(ref strArg1, atFlag)) {
                    // invalid argument
                    AddMinorError(4039, LoadResString(4039).Replace(ARG1, strArg1));
                    // try to figure out intention
                    strArg2 = PeekToken();
                    switch (strArg2) {
                    case EQUAL_TOKEN or NOTEQUAL_TOKEN or ">" or "<" or
                         ">=" or "<=" or "=>" or "=<":
                        // assume variable, use placeholder
                        blnIsVar = true;
                        strArg1 = "v255";
                        break;
                    case ")" or "&&" or "||":
                        // assume flag, use placeholder
                        blnIsVar = false;
                        strArg1 = "f255";
                        break;
                    default:
                        // seriously fouled up, just exit
                        return true;
                    }
                }
            }
            if (blnIsVar) {
                //arg 1 is 'v#'
                intArg1 = VariableValue(strArg1);
                if (intArg1 == -1) {
                    //invalid number
                    AddMinorError(4086);
                }
                //get comparison expression
                strArg2 = NextToken();
                //get token code for this expression
                switch (strArg2) {
                case EQUAL_TOKEN:
                    bytCmdNum = 0x01;
                    break;
                case NOTEQUAL_TOKEN:
                    bytCmdNum = 0x01;
                    blnAddNOT = true;
                    break;
                case ">":
                    bytCmdNum = 0x05;
                    break;
                case "<=" or "=<":
                    bytCmdNum = 0x05;
                    blnAddNOT = true;
                    break;
                case "<":
                    bytCmdNum = 0x03;
                    break;
                case ">=" or "=>":
                    bytCmdNum = 0x03;
                    blnAddNOT = true;
                    break;
                case ")" or "&&" or "||":
                    //means we are doing a boolean test of the variable;
                    //use greatern with zero as arg
                    tmpLogRes.WriteByte(0x05);
                    tmpLogRes.WriteByte((byte)intArg1);
                    tmpLogRes.WriteByte(0);
                    //backup the compiler pos so main compiler loop can 
                    // find the next command
                    lngPos -= strArg2.Length;
                    return true;
                default:
                    AddMinorError(4078);
                    // use placeholder cmd
                    bytCmdNum = 0x01;
                    break;
                }
                //before getting second arg, check for NOT symbol in front of a variable
                //can't have a NOT in front of variable comparisons
                if (blnNOT) {
                    AddMinorError(4098);
                    return false;
                }
                //get second argument (numerical or variable)
                blnArg2Var = true;
                //reset the quotemark error flag
                lngQuoteAdded = -1;
                intArg2 = GetNextArg(atNum, -1, ref blnArg2Var);
                //if error
                if (intArg2 < 0) {
                    //if an invalid arg value found
                    switch (intArg2) {
                    case -1:
                        // error 4063; change error message
                        errInfo.Text = errInfo.Text[49..(errInfo.Text.LastIndexOf('\'') - 1)];
                        errInfo.Text = LoadResString(4089).Replace(ARG1, errInfo.Text);
                        break;
                    default:
                        // change error to 4089
                        errInfo.Text = LoadResString(4089).Replace(ARG1, "");
                        //skip the next char
                        lngPos++;
                        break;
                    }
                    AddMinorError(4089, errInfo.Text);
                }
                //if comparing to a variable,
                if (blnArg2Var) {
                    // use variable version of command
                    bytCmdNum++;
                }
                //if adding a 'not'
                if (blnAddNOT) {
                    tmpLogRes.WriteByte(0xFD);
                }
                //write command, and arguments
                tmpLogRes.WriteByte(bytCmdNum);
                tmpLogRes.WriteByte((byte)intArg1);
                tmpLogRes.WriteByte((byte)intArg2);
            }
            else {
                //get flag argument Value
                intArg1 = VariableValue(strArg1);
                //if invalid flag number
                if (intArg1 == -1) {
                    //invalid number
                    AddMinorError(4002, LoadResString(4066).Replace(ARG1, "1"));
                    return false;
                }
                //write isset cmd
                tmpLogRes.WriteByte(0x7);
                tmpLogRes.WriteByte((byte)intArg1);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strArgIn"></param>
        /// <returns></returns>
        static internal bool CompileSpecial(string strArgIn) {
            //strArg1 should be a variable in the format of *v##, v##, f## or s##
            //if it is not, this function will trap it and return an error
            //the expression after strArg1 should be one of the following:
            // =, +=, -=, *=, /=, ++, --
            //
            // in Sierra syntax, @= and =@ are also valid
            //
            //after determining assignment type, this function will validate additional
            //arguments as necessary
            //
            string strArg1, strArg2;
            int intArg1 = -1, intArg2 = -1, lngLineType = 0, maxStr;
            int intDir = 0; //0 = no indirection; 1 = left; 2 = right
            bool blnArg2Var = false, blnGuess = false;
            byte bytCmd = 0;
            byte[] bytArgs;

            // assume OK until proven otherwise

            strArg1 = strArgIn;
            //if this is normal indirection
            if (strArg1[1] == INDIR_CHAR) {
                if (agSierraSyntax) {
                    AddMinorError(4105);
                }
                else {
                    //left indirection
                    //     *v# = #
                    //     *v# = v#
                    intDir = 1;
                    //next char can't be a space or newline
                    switch (strCurrentLine[lngPos + 1]) {
                    case ' ' or (char)0:
                        //error
                        AddMinorError(4105);
                        break;
                    }
                }
                //get next token (should be variable for indirection)
                strArg1 = NextToken();
                // it must be a variable
                if (!ConvertArgument(ref strArg1, atVar)) {
                    AddMinorError(4064);
                    // use placeholder
                    strArg1 = "v255";
                }
                // type is variable assignment
                lngLineType = 2;
            }
            else {
                // check for string assignment
                if (ConvertArgument(ref strArg1, atStr)) {
                    lngLineType = 1;
                }
                // check for variable assignment/math
                else if (ConvertArgument(ref strArg1, atVar)) {
                    lngLineType = 2;
                }
                // check for flag assignment
                else if (ConvertArgument(ref strArg1, atFlag)) {
                    lngLineType = 3;
                }
                // if nothing found, assume something
                if (lngLineType == 0) {
                    // sytnax error in string/ flag / variable assignment
                    AddMinorError(4044, LoadResString(4044).Replace(ARG1, strArg1));
                    // if nextchar is equal, assume it's assignment
                    // but which one?
                    switch (PeekToken(true)) {
                    case "=" or "==":
                        // assume variable assignment/math
                        lngLineType = 2;
                        if (!agSierraSyntax) {
                            // but it's just a guess
                            blnGuess = true;
                        }
                        // use placeholder to keep going
                        strArg1 = "v255";
                        break;
                    case "++" or "+=" or "--" or "-=" or "*=" or "/=" or "@=" or "=@":
                        // assume variable assignment/math
                        lngLineType = 2;
                        // use placeholder to keep going
                        strArg1 = "v255";
                        break;
                    default:
                        // line is totally hosed; skip to end to ignore rest of this line
                        strCurrentLine = ";";
                        lngPos = 0;
                        return true;
                    }
                }
            }
            switch (lngLineType) {
            case 1:
                //string assignment
                //     s# = m#
                //     s# = "<string>"

                // not allowed in Sierra syntax
                if (agSierraSyntax) {
                    AddMinorError(6009);
                    // assume rest of syntax is correct even though not allowed
                }
                //get string variable number
                intArg1 = VariableValue(strArg1);
                switch (ErrorLevel) {
                case leHigh or leMedium:
                    //for version 2.089, 2.272, and 3.002149 only 12 strings
                    maxStr = compGame.agIntVersion switch {
                        "2.089" or "2.272" or "3.002149" => 11,
                        _ => 23,
                    };
                    if (intArg1 > maxStr) {
                        switch (ErrorLevel) {
                        case leHigh:
                            AddMinorError(4079, LoadResString(4079).Replace(ARG1, "1").Replace(ARG2, maxStr.ToString()));
                            break;
                        case leMedium:
                            AddWarning(5007, LoadResString(5007).Replace(ARG1, maxStr.ToString()));
                            break;
                        }
                    }
                    break;
                }
                // next token must be "="
                if (!CheckToken("=")) {
                    //error
                    AddMinorError(4034);
                    // check for "=="; it's a common error
                    if (!CheckToken("==")) {
                        // line is seriously messed; skip it
                        strCurrentLine = ";";
                        lngPos = 0;
                        return true;
                    }
                }
                // check for premature eol marker
                if (PeekToken() == ";") {
                    AddMinorError(4058);
                    return true;
                }
                //get actual second variable
                // (use argument extractor in case second variable is a literal string)
                intArg2 = GetNextArg(atMsg, -1);
                //if error
                if (intArg2 < 0) {
                    // add error msg
                    AddMinorError(4058);
                    switch (intArg2) {
                    case -1:
                        // if next token is '(', it means assignment probably just missing
                        if (PeekToken(true) == "(") {
                            // assume rest of line is bad
                            strCurrentLine = ";";
                            lngPos = 0;
                            return true;
                        }
                        break;
                    default:
                        // skip the bad char and keep going
                        lngPos++;
                        break;
                    }
                    // use a placeholder
                    intArg2 = 0;
                }
                //command is set.string
                bytCmd = 0x72;
                //if this is a variable
                break;
            case 2:
                //variable assignment or arithmetic operation
                // (strArg1 is confirmed to be 'v##')
                intArg1 = VariableValue(strArg1);
                //need next command to determine what kind of assignment/operation
                strArg2 = NextToken();
                // for indirection only '=' is valid
                if (intDir == 1) {
                    switch (strArg2) {
                    case "=":
                        // ok
                        break;
                    case "==":
                        // error but treat as '='
                        AddMinorError(4034);
                        // if end of line is next, just exit
                        if (PeekToken(false) == ";") {
                            // exit
                            return true;
                        }
                        break;
                    case "+=" or "-=" or "*=" or "/=" or "++" or "--":
                        // error- math ops not allowed on indirect variables
                        AddMinorError(4105);
                        break;
                    default:
                        // this line is messed up
                        AddMinorError(4034);
                        if (strArg2 == ";") {
                            lngPos--;
                        }
                        return false;
                    }
                }
                else {
                    switch (strArg2) {
                    case "++":
                        // v#++;
                        bytCmd = 0x1;
                        break;
                    case "+=":
                        // v# += #; or v# += v#;
                        bytCmd = 0x5;
                        break;
                    case "--":
                        // v#--
                        bytCmd = 0x2;
                        break;
                    case "-=":
                        // v# -= #; or v# -= v#;
                        bytCmd = 0x7;
                        break;
                    case "*=":
                        // v# *= #; or v# *= v#;
                        bytCmd = 0xA5;
                        break;
                    case "/=":
                        // v# /= #; v# /= v#
                        bytCmd = 0xA7;
                        break;
                    case "=":
                        //right indirection
                        //     v# = *v#;
                        //assignment
                        //     v# = v#;
                        //     v# = #;
                        //long arithmetic operation
                        //     v# = v# + #; v# = v# + v#;
                        //     v# = v# - #; v# = v# - v#;
                        //     v# = v# * #; v# = v# * v#;
                        //     v# = v# / #; v# = v# / v#;
                        break;
                    case "==":
                        // error, but treat as '='
                        AddMinorError(4034);
                        if (PeekToken() == ";") {
                            // exit if  eol marker is found
                            return true;
                        }
                        break;
                    case "@=":
                        // Sierra syntax only; 
                        // v# @= #; v# @= v#;
                        if (agSierraSyntax) {
                            // assume number indirection
                            bytCmd = 0x0B;
                        }
                        else {
                            // error
                            AddMinorError(4164);
                            // use 'add.n' as placeholder
                            bytCmd = 0x05;
                        }
                        break;
                    case "=@":
                        // Sierra syntax only
                        // v# =@ v3;
                        if (agSierraSyntax) {
                            bytCmd = 0x0A;
                        }
                        else {
                            // error
                            AddMinorError(4164);
                            // use 'add.v' as a placeholder
                            bytCmd = 0x06;
                        }
                        break;
                    default:
                        // no idea what's going on in this line
                        AddMinorError(4034);
                        // if arg is an end - of - line marker, backup to use it
                        if (strArg2 == ";") {
                            lngPos--;
                        }
                        return true;
                    }
                }
                break;
            case 3:
                // flag assignment
                //     f# = true;
                //     f# = false;
                if (agSierraSyntax) {
                    // not allowed in Sierra syntax
                    AddMinorError(6010);
                    // then continue
                }
                //get flag number
                intArg1 = VariableValue(strArg1);
                //next token must be equal sign
                if (!CheckToken("=")) {
                    AddMinorError(4034);
                    // check for '=='; it's a common error
                    if (!CheckToken("==")) {
                        if (PeekToken(false) == ";") {
                            // exit if end-of-line marker is found
                            return true;
                        }
                    }
                }
                //get flag Value
                strArg2 = NextToken();
                switch (strArg2) {
                case "true":
                    //set this flag
                    bytCmd = 0xC;
                    break;
                case "false":
                    //reset this flag
                    bytCmd = 0xD;
                    break;
                default:
                    //error
                    AddMinorError(4034);
                    // if it was ';', back up to use it
                    if (strArg2 == ";") {
                        lngPos--;
                    }
                    // if next cmd is '(', it means assignment probably just missing
                    if (PeekToken(true) == "(") {
                        // assume rest of line is bad
                        strCurrentLine = ";";
                        lngPos = 0;
                    }
                    return true;
                }
                break;
            }
            // get second argument
            switch (bytCmd) {
            case 0x1 or 0x2 or 0xC or 0xD or 0x72:
                //skip check for second argument if cmd is known to be a single arg
                //commands: increment, decrement, reset, set
                //(set string is also skipped because second arg is already determined)
                break;
            default:
                //get next argument
                strArg2 = NextToken();
                //if it is indirection
                if (strArg2 == "*") {
                    if (agSierraSyntax) {
                        AddMinorError(4105);
                    }
                    else {
                        if (intDir == 0) {
                            // if cmd already set, it's an error
                            if (bytCmd != 0) {
                                AddMinorError(4105);
                            }
                            else {
                                //set right indirection
                                intDir = 2;
                                //next char can't be a space or end of line
                                switch (strCurrentLine[lngPos + 1]) {
                                case ' ' or (char)0:
                                    AddMinorError(4105);
                                    break;
                                }
                            }
                        }
                        else {
                            // no double-indirection
                            AddMinorError(4105);
                        }
                    }
                    // get next token (should  be variable for for indirection)
                    strArg2 = NextToken();
                }
                // if it's end-of-line marker, it's an error
                if (strArg2 == ";") {
                    lngPos--;
                    // exit if guessing
                    if (blnGuess) {
                        return true;
                    }
                }
                // arg may be v##, or ## (or f##/s## if already an error)
                blnArg2Var = true;
                if (!ConvertArgument(ref strArg2, atNum, ref blnArg2Var)) {
                    // maybe it was an invalid string or flag assignment
                    if (blnGuess) {
                        // check for msg
                        if (ConvertArgument(ref strArg2, atMsg)) {
                            // adjust type, use placeholders (since there's already
                            // an error encountered)
                            lngLineType = 1;
                            blnArg2Var = false;
                            bytCmd = 0x72;
                            intArg1 = 1;
                        }
                        // check for flag
                        else if (strArg2 == "true" || strArg2 == "false") {
                            lngLineType = 3;
                            // use place holders since error already encountered
                            bytCmd = 0xC;
                            intArg1 = 255;
                        }
                    }
                    else {
                        // error
                        AddMinorError(4088, LoadResString(4088).Replace(ARG1, strArg2));
                        // use placeholder
                        strArg2 = "0";
                        if (PeekToken(true) == "(") {
                            // assume rest of line is bad
                            strCurrentLine = ";";
                            lngPos = 0;
                            return true;
                        }
                    }
                }
                // if guessing, type may have changed; only need to continue if
                // still processing a variable assignment
                if (lngLineType == 2) {
                    // if it's a number, check for negative value
                    if (int.TryParse(strArg2, out intArg2)) {
                        if (intArg2 < 0) {
                            if (intArg2 < -128) {
                                //error
                                AddMinorError(4095);
                                // use a placeholder
                                intArg2 = 255;
                            }
                            // convert to unsigned byte
                            intArg2 = 256 + intArg2;
                            switch (ErrorLevel) {
                            case leHigh or leMedium:
                                AddWarning(5098);
                                break;
                            }
                        }
                        else {
                            if (intArg2 > 255) {
                                AddMinorError(4088);
                                // use a place holder
                                intArg2 = 1;
                            }
                        }
                    }
                    else {
                        // it's a number or variable; verify it's 0-255
                        intArg2 = VariableValue(strArg2);
                        if (intArg2 == -1) {
                            AddMinorError(4088);
                            // use a placeholder
                            intArg2 = 1;
                        }

                    }
                    // if indirection (only true if using sierra syntax)
                    if (bytCmd == 0xA) {
                        // rindirect only allowed if arg2 is a variable
                        if (!blnArg2Var) {
                            // error
                            AddMinorError(6011);
                        }
                        // reset blnArg2Var so follow on check doesn't
                        // mistakenly adjust it
                        blnArg2Var = false;
                    }
                    if (bytCmd == 0xB) {
                        // lindirectn or lindirectv
                        if (blnArg2Var) {
                            bytCmd = 0x9;
                            // reset blnArg2Var so follow on check doesn't
                            // mistakenly adjust it
                            blnArg2Var = false;
                        }
                    }
                    // if arg2 is a number
                    if (!blnArg2Var) {
                        // if cmd not yet known
                        if (bytCmd == 0) {
                            // must be assign:
                            // v# = # or *v# = #
                            if (intDir == 1) {
                                // lindirect.n
                                bytCmd = 0xB;
                            }
                            else {
                                // assign.n
                                bytCmd = 0x3;
                            }
                        }
                    }

                }
                break;
            }

            //if command is still not known
            if (bytCmd == 0) {
                //if arg values are the same (already know arg2 is a variable)
                //and no indirection
                if ((intArg1 == intArg2) && (intDir == 0)) {
                    //check for long arithmetic
                    strArg2 = NextToken();
                    //if end of command is reached
                    if (strArg2 == ";") {
                        //move pointer back one space so eol
                        //check in CompileAGI works correctly
                        lngPos--;
                        //this is a simple assign (with a variable being assigned to itself!!)
                        switch (ErrorLevel) {
                        case leHigh or leMedium:
                            AddWarning(5036);
                            break;
                        }
                        // assign.v
                        bytCmd = 0x3;
                    }
                    else {
                        //this may be long arithmetic
                        if (agSierraSyntax) {
                            // not allowed in Sierra syntax
                            AddMinorError(6012);
                            return true;
                        }
                        switch (strArg2) {
                        case "+":
                            bytCmd = 0x5;
                            break;
                        case "-":
                            bytCmd = 0x7;
                            break;
                        case "*":
                            bytCmd = 0xA5;
                            break;
                        case "/":
                            bytCmd = 0xA7;
                            break;
                        default:
                            //error
                            AddMinorError(4087);
                            return true;
                        }
                        //now get actual second argument (number or variable)
                        blnArg2Var = true;
                        intArg2 = GetNextArg(atNum, -1, ref blnArg2Var);
                        //if error
                        if (intArg2 < 0) {
                            switch (intArg2) {
                            case -1:
                                // error 4063 - change error message
                                errInfo.Text = LoadResString(4161).Replace(ARG1, errInfo.Text.Substring(55, errInfo.Text.LastIndexOf('\'') - 53));
                                break;
                            default:
                                errInfo.Text = LoadResString(4161).Replace(ARG1, "");
                                break;
                            }
                            AddMinorError(4161, errInfo.Text);
                            // use a placeholder
                            intArg2 = 0;
                        }
                    }
                }
                else {
                    //the second variable argument is different, or indirection
                    //must be assignment
                    // v# = v#
                    // *v# = v#
                    // v# = *v#
                    switch (intDir) {
                    case 0:  //assign.v
                        bytCmd = 0x4;
                        // special check for improper math
                        // vA = vB + # for example
                        strArg2 = NextToken();
                        switch (strArg2) {
                        case "+" or "-" or "*" or "/":
                            // bad math
                            AddMinorError(4005);
                            // ignore the symbol and next
                            strArg2 = NextToken(true);
                            break;
                        default:
                            // assume ok, put token back on stream
                            lngPos += strArg2.Length;
                            break;
                        }
                        break;
                    case 1: //lindirect.v
                        bytCmd = 0x9;
                        break;
                    case 2:  //rindirect
                        bytCmd = 0xA;
                        break;
                    }
                    //always reset arg2var flag so
                    //command won't be adjusted later
                    blnArg2Var = false;
                }
            }
            // if second argument is a variable
            if (blnArg2Var) {
                // use variable version of command
                bytCmd++;
            }
            //need to validate arguments for this command
            switch (bytCmd) {
            case 0x1 or 0x2 or 0xC or 0xD:
                //single arg commands
                bytArgs = new byte[1];
                bytArgs[0] = (byte)intArg1;
                break;
            default:
                //two arg commands
                bytArgs = new byte[2];
                bytArgs[0] = (byte)intArg1;
                bytArgs[1] = (byte)intArg2;
                break;
            }
            //validate commands before writing
            if (!ValidateArgs(bytCmd, ref bytArgs)) {
                return false;
            }
            //write command and arg1
            tmpLogRes.WriteByte(bytCmd);
            tmpLogRes.WriteByte((byte)intArg1);
            //write second argument for all cmds except 0x1, 0x2, 0xC, 0xD
            switch (bytCmd) {
            case 0x1 or 0x2 or 0xC or 0xD:
                break;
            default:
                tmpLogRes.WriteByte((byte)intArg2);
                break;
            }
            return true;
        }
        #endregion
    }
}
