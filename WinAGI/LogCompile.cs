using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.LogicErrorLevel;
using EnvDTE;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;

namespace WinAGI.Engine
{
    public static partial class Compiler
    {
        // reminder: in VB6, the compiler was not case sensitive
        //EXCEPT strings in messages; now all tokens are case sensitive

        internal struct LogicGoto
        {
            internal byte LabelNum;
            internal int DataLoc;
        }
        internal struct LogicLabel
        {
            internal string Name;
            internal int Loc;
        }

        internal static Logic tmpLogRes;
        internal static AGIGame compGame;
        internal static byte[] mbytData;
        internal const int MAX_BLOCK_DEPTH = 64; //used by LogDecode functions too
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
        internal const string CMT2_TOKEN = "//";
        internal const string INDIR_CHAR = "*";
        internal static byte bytLogComp;
        internal static string strLogCompID;
        internal static bool blnCriticalError, blnMinorError;
        internal static int lngQuoteAdded;
        internal static int lngErrNum;
        internal static string strErrMsg;
        internal static string strModule, strModFileName;
        internal static int lngErrLine;
        internal static int intCtlCount;
        internal static bool blnNewRoom;
        internal static string INCLUDE_MARK = ((char)31).ToString() + "!";
        internal static string[] strIncludeFile;
        internal static int lngIncludeOffset; //to correct line number due to added include lines
        internal static int intFileCount;
        internal static List<string> stlInput = [];  //the entire text to be compiled; includes the
                                                        //original logic text, includes, and defines
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

        internal static TWarnInfo CompileLogic(Logic SourceLogic)
        {
            //this function compiles the sourcetext that is passed
            //if successful, the function returns a warning code of ecCompOK
            //if not successful, compInfo is filled with relevant info
            //
            //if an error occurs, nothing is passed in tmpInfo
            //note that when errors are returned, line is adjusted because
            //editor rows(lines) start at '1', but the compiler starts at line '0'

            bool blnCompiled;
            List<string> stlSource = new();
            string strInput;
            int lngCodeSize;
            TWarnInfo tmpInfo = new();

            compGame = SourceLogic.parent;

            //set error info to success as default
            blnCriticalError = false;
            blnMinorError = false;
            lngErrLine = -1;
            strErrMsg = "";
            strModule = "";
            strModFileName = "";
            intCtlCount = 0;

            //initialize global defines
            if (!compGame.GlobalDefines.IsSet) {
                compGame.GlobalDefines.GetGlobalDefines();
            }
            //if ids not set yet
            if (!blnSetIDs) {
                SetResourceIDs(compGame);
            }

            // always reset command name assignments
            CorrectCommands(compGame.agIntVersion);

            //refresh  values for game specific reserved defines
            compGame.agResGameDef[0].Value = "\"" + compGame.agGameID + "\"";
            compGame.agResGameDef[1].Value = "\"" + compGame.agGameVersion + "\"";
            compGame.agResGameDef[2].Value = "\"" + compGame.agAbout + "\"";
            if (!compGame.InvObjects.Loaded)
            {
                compGame.InvObjects.Load();
                //Count of ACTUAL useable objects is one less than inventory object Count
                //because the first object ('?') is just a placeholder
                compGame.agResGameDef[3].Value = (compGame.InvObjects.Count - 1).ToString();
                compGame.InvObjects.Unload();
            }
            else
            {
                compGame.agResGameDef[3].Value = (compGame.InvObjects.Count - 1).ToString();
            }

            //get source text by lines as a list of strings
            stlSource = SplitLines(UnicodeToCP(SourceLogic.SourceText,compGame.CodePage));
            bytLogComp = SourceLogic.Number;
            strLogCompID = SourceLogic.ID;

            //reset error info
            lngErrLine = -1;
            strErrMsg = "";
            strModule = "";
            strModFileName = "";

            // reset the combined input list
            stlInput = new();
            intFileCount = 0;

            //add include files (extchars handled automatically)
            if (!AddIncludes(stlSource)) {
                //return error
                tmpInfo.ID = lngErrNum.ToString();
                tmpInfo.Text = strErrMsg;
                tmpInfo.Line = lngErrLine + 1;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }

            //remove any blank lines from end
            while (stlInput[^1].Length == 0 && stlInput.Count > 0) { // Until Len(stlInput(stlInput.Count - 1)) != 0 || stlInput.Count = 0
                stlInput.RemoveAt(stlInput.Count - 1);
            }

            //if nothing to compile, throw an error
            if (stlInput.Count == 0) {
                //return error
                tmpInfo.ID = "4159";
                tmpInfo.Text = LoadResString(4159);
                tmpInfo.Line = 0;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }

            //strip out all comments
            if (!RemoveComments()) {
                //return error
                tmpInfo.ID = lngErrNum.ToString();
                tmpInfo.Text = strErrMsg;
                tmpInfo.Line = lngErrLine + 1;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }

            //read labels
            if (!ReadLabels()) {
                //return error
                tmpInfo.ID = lngErrNum.ToString();
                tmpInfo.Text = strErrMsg;
                tmpInfo.Line = lngErrLine + 1;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }

            //enumerate and replace all the defines
            if (!ReadDefines()) {
                //return error
                tmpInfo.ID = lngErrNum.ToString();
                tmpInfo.Text = strErrMsg;
                tmpInfo.Line = lngErrLine + 1;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }
            //read predefined messages
            if (!ReadMsgs()) {
                //return error
                tmpInfo.ID = lngErrNum.ToString();
                tmpInfo.Text = strErrMsg;
                tmpInfo.Line = lngErrLine + 1;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }

            //assign temporary resource object
            tmpLogRes = new Logic();
            // and clear the data
            tmpLogRes.Data.Clear();
            //write a word as a place holder for offset to msg section start
            tmpLogRes.WriteWord(0, 0);

            //run agi compiler
            blnCompiled = CompileAGI();

            //compile commands
            if (!blnCompiled) {
                tmpLogRes.Unload();
                //return error
                tmpInfo.ID = lngErrNum.ToString();
                tmpInfo.Text = strErrMsg;
                tmpInfo.Line = lngErrLine + 1;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }

            // code size equals bytes currently written (before msg secion added)
            lngCodeSize = tmpLogRes.Size;

            //write message section
            if (!WriteMsgs()) {
                tmpLogRes.Unload();
                //return error
                tmpInfo.ID = lngErrNum.ToString();
                tmpInfo.Text = strErrMsg;
                tmpInfo.Line = lngErrLine + 1;
                tmpInfo.Module = strModule;
                tmpInfo.Type = EWarnType.ecError;
                return tmpInfo;
            }

            // if no minor errors
            if (!blnMinorError)
            {
                //assign resource data
                SourceLogic.Data.AllData = tmpLogRes.Data.AllData;
                //update compiled crc
                SourceLogic.CompiledCRC = SourceLogic.CRC;
                // and write the new crc values to property file
                compGame.WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CRC32", "0x" + SourceLogic.CRC.ToString("x8"), "Logics");
                compGame.WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CompCRC32", "0x" + SourceLogic.CompiledCRC.ToString("x8"));
            }


            //done with the temp resource
            tmpLogRes.Unload();

            // if minor errors, report them as a compile fail
            if (blnMinorError)
            {
                tmpInfo.ID = "4001";
                tmpInfo.Text = LoadResString(4001);
                tmpInfo.Line = 0;
                tmpInfo.Module = "";
                tmpInfo.Type = EWarnType.ecError;
            }
            else
            {
                tmpInfo.Type = EWarnType.ecCompOK;
            }
                return tmpInfo;
        }
        internal static void SetResourceIDs(AGIGame game)
        {
            //builds array of resourceIDs so
            //convertarg function can iterate through them much quicker
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
            //set flag
            blnSetIDs = true;
        }
        internal static bool ConvertArgument(ref string strArgIn, ArgTypeEnum ArgType)
        {
            // use this when var-or-num option isn't needed
            bool nullval = false;
            return ConvertArgument(ref strArgIn, ArgType, ref nullval);
        }
        internal static bool ConvertArgument(ref string strArgIn, ArgTypeEnum ArgType, ref bool blnVarOrNum)
        {
            // make sure blnVarOrNum gets passed as false, unless explicitly needed to be true!
            //if input is not a system argument already
            //(i.e. ##, v##, f##, s##, o##, w##, i##, c##)
            //this function searches resource IDs, local defines, global defines,
            //and reserved names for strArgIn; if found
            //strArgIn is replaced with the Value of the define
            //optional argtype is used to identify words, messages, and inv objects
            //to speed up search

            //NOTE: this does NOT validate the numerical Value of arguments;
            //calling function is responsible to make that check
            //it also does not concatenate strings

            //to support calls from special syntax compilers, need to be able
            //to check for numbers AND variables with one check
            //the blnVarOrNum flag is used to do this; when the flag is
            //true, number searches also return variables

            int i;
            int intAsc;

            //check if already in correct format
            switch (ArgType) {
            case atNum:  //numeric only
                if (IsNumeric(strArgIn)) {
                    //reset VarOrNum flag
                    blnVarOrNum = false;
                    return true;
                }
                //unless looking for var or num
                if (blnVarOrNum) {
                    //then //v##// is ok
                    if ((strArgIn[0] | 32) == 118) {
                        if (VariableValue(strArgIn) != -1) {
                            return true;
                        }
                    }
                }
                break;
            case atVar:
                //if first char matches
                if ((strArgIn[0] | 32) == 118) {
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                }
                break;
            case atFlag:
                //if first char matches
                if ((strArgIn[0] | 32) == 102) {
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                }
                break;
            case atCtrl:
                //if first char matches
                if ((strArgIn[0] | 32) == 99) {
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                }
                break;
            case atSObj:
                //if first char matches
                if ((strArgIn[0] | 32) == 111) {
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                }
                break;
            case atStr:
                //if first char matches
                if ((strArgIn[0] | 32) == 115) {
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                }
                break;
            case atWord: //NOTE: this is NOT vocab word; this is word arg type (used in command word.to.string)
                         //if first char matches
                if ((strArgIn[0] | 32) == 119) {
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                }
                break;
            case atMsg:
                //if first char matches, or is a quote
                intAsc = strArgIn[0] | 32;
                switch (intAsc) {
                case 109:
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                    break;
                case 34:
                    //strings are always ok
                    return true;
                }
                break;
            case atIObj:
                //if first char matches, or is a quote
                intAsc = strArgIn[0] | 32;
                switch (intAsc) {
                case 105:
                    //if this arg returns a valid Value
                    if (VariableValue(strArgIn) != -1) {
                        //ok
                        return true;
                    }
                    break;
                case 34:
                    //strings are always ok
                    return true;
                }
                break;
            case atVocWrd:
                //can be number or string in quotes
                if (IsNumeric(strArgIn) || strArgIn[0] == 34) {
                    //ok
                    return true;
                }
                break;
            }

            //arg is not in correct format; must be reserved name, global or local define, or an error

            //first, check against local defines
            for (i = 0; i < lngDefineCount; i++) {
                if (strArgIn.Equals(tdDefines[i].Name, StringComparison.OrdinalIgnoreCase)) {
                    //match found; check that Value is correct type
                    switch (ArgType) {
                    case atNum:
                        //check for number
                        if (IsNumeric(tdDefines[i].Value)) {
                            //reset VarOrNum flag
                            blnVarOrNum = false;
                            strArgIn = tdDefines[i].Value;
                            return true;
                        }

                        //if checking for variables
                        if (blnVarOrNum) {
                            if ((tdDefines[i].Value[0] | 32) == 118) {
                                //if this define returns a valid Value
                                if (VariableValue(tdDefines[i].Value) != -1) {
                                    //ok
                                    strArgIn = tdDefines[i].Value;
                                    return true;
                                }
                            }
                        }
                        break;
                    case atVar:
                        //v## only
                        if ((tdDefines[i].Value[0] | 32) == 118) {
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atFlag:
                        //f## only
                        if ((tdDefines[i].Value[0] | 32) == 102) {
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atMsg:
                        //m## or a string
                        intAsc = tdDefines[i].Value[0] | 32;
                        switch (intAsc) {
                        case 109:
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                            break;
                        case 34:
                            strArgIn = tdDefines[i].Value;
                            return true;
                        }
                        break;
                    case atSObj:
                        //o## only
                        if ((tdDefines[i].Value[0] | 32) == 111) {
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atIObj:
                        //i## or a string
                        intAsc = tdDefines[i].Value[0];
                        switch (intAsc) {
                        case 105:
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                            break;
                        case 34:
                            strArgIn = tdDefines[i].Value;
                            return true;
                        }
                        break;
                    case atStr:
                        //s## only
                        if ((tdDefines[i].Value[0] | 32) == 115) {
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atWord:
                        //w## only
                        if ((tdDefines[i].Value[0] | 32) == 119) {
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atCtrl:
                        //c## only
                        if ((tdDefines[i].Value[0] | 32) == 99) {
                            //if this define returns a valid Value
                            if (VariableValue(tdDefines[i].Value) != -1) {
                                //ok
                                strArgIn = tdDefines[i].Value;
                                return true;
                            }
                        }
                        break;
                    case atVocWrd:
                        //numeric or string only
                        if (IsNumeric(tdDefines[i].Value)) {
                            strArgIn = tdDefines[i].Value;
                            return true;
                        }
                        else if (tdDefines[i].Value[0] == 34) {
                            strArgIn = tdDefines[i].Value;
                            return true;
                        }
                        break;
                    case atDefStr:
                        //call to ConvertArgument is never made with type of atDefStr
                        break;
                    }
                    //not validated, so return false
                    return false;
                }
            } //nexti

            //second, check against global defines
            //for any type except vocab words
            if (ArgType != atVocWrd) {
                //check against this type of global defines
                for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                    if (compGame.GlobalDefines[i].Type == ArgType) {
                        if (strArgIn == compGame.GlobalDefines[i].Name) {
                            strArgIn = compGame.GlobalDefines[i].Value;
                            //reset VarOrNum flag
                            blnVarOrNum = false;
                            return true;
                        }
                    }
                } //nexti
                  //if checking var or num
                if (blnVarOrNum) {
                    //numbers were checked; need to check variables
                    for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                        if (compGame.GlobalDefines[i].Type == atVar) {
                            if (strArgIn == compGame.GlobalDefines[i].Name) {
                                strArgIn = compGame.GlobalDefines[i].Value;
                                return true;
                            }
                        }
                    } //nexti
                }
            }
            else {
                //check vocab words only against numbers
                for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                    if (compGame.GlobalDefines[i].Type == atNum) {
                        if (strArgIn == compGame.GlobalDefines[i].Name) {
                            strArgIn = compGame.GlobalDefines[i].Value;
                            return true;
                        }
                    }
                } //nexti
            }
            //check messages, iobjs, and vocab words against global strings
            if ((ArgType == atMsg) || (ArgType == atIObj) || (ArgType == atVocWrd)) {
                //check against global defines (string type)
                for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                    if (compGame.GlobalDefines[i].Type == atDefStr) {
                        if (strArgIn == compGame.GlobalDefines[i].Name) {
                            strArgIn = compGame.GlobalDefines[i].Value;
                            return true;
                        }
                    }
                } //nexti
            }

            //third, check numbers against list of resource IDs
            if (ArgType == atNum) {
                //check against resource IDs
                for (i = 0; i <= 255; i++) {
                    //if this arg matches one of the resource ids
                    if (strArgIn == strLogID[i]) {
                        strArgIn = i.ToString();
                        //reset VarOrNum flag
                        blnVarOrNum = false;
                        return true;
                    }
                    if (strArgIn == strPicID[i]) {
                        strArgIn = i.ToString();
                        //reset VarOrNum flag
                        blnVarOrNum = false;
                        return true;
                    }
                    if (strArgIn == strSndID[i]) {
                        strArgIn = i.ToString();
                        //reset VarOrNum flag
                        blnVarOrNum = false;
                        return true;
                    }
                    if (strArgIn == strViewID[i]) {
                        strArgIn = i.ToString();
                        //reset VarOrNum flag
                        blnVarOrNum = false;
                        return true;
                    }
                } //nexti
            }

            //lastly, if using reserved names,
            if (UseReservedNames) {
                //last of all, check reserved names
                switch (ArgType) {
                case atNum:
                    for (i = 0; i <= 4; i++) {
                        if (strArgIn == agEdgeCodes[i].Name) {
                            strArgIn = agEdgeCodes[i].Value;
                            //reset VarOrNum flag
                            blnVarOrNum = false;
                            return true;
                        }
                    }// i
                    for (i = 0; i <= 8; i++) {
                        if (strArgIn == agEgoDir[i].Name) {
                            strArgIn = agEgoDir[i].Value;
                            //reset VarOrNum flag
                            blnVarOrNum = false;
                            return true;
                        }
                    }// i
                    for (i = 0; i <= 4; i++) {
                        if (strArgIn == agVideoMode[i].Name) {
                            strArgIn = agVideoMode[i].Value;
                            //reset VarOrNum flag
                            blnVarOrNum = false;
                            return true;
                        }
                    } // i
                    for (i = 0; i <= 8; i++) {
                        if (strArgIn == agCompType[i].Name) {
                            strArgIn = agCompType[i].Value;
                            //reset VarOrNum flag
                            blnVarOrNum = false;
                            return true;
                        }
                    } // i
                    for (i = 0; i <= 15; i++) {
                        if (strArgIn == agResColor[i].Name) {
                            strArgIn = agResColor[i].Value;
                            //reset VarOrNum flag
                            blnVarOrNum = false;
                            return true;
                        }
                    }// i
                     //check against invobj Count
                    if (strArgIn == compGame.agResGameDef[3].Name) {
                        strArgIn = compGame.agResGameDef[3].Value;
                        //reset VarOrNum flag
                        blnVarOrNum = false;
                        return true;
                    }
                    //if looking for numbers OR variables
                    if (blnVarOrNum) {
                        //check against builtin variables as well
                        for (i = 0; i <= 26; i++) {
                            if (strArgIn == agResVar[i].Name) {
                                strArgIn = agResVar[i].Value;
                                return true;
                            }
                        } // i
                    }
                    break;
                case atVar:
                    for (i = 0; i <= 26; i++) {
                        if (strArgIn == agResVar[i].Name) {
                            strArgIn = agResVar[i].Value;
                            return true;
                        }
                    }// i
                    break;
                case atFlag:
                    for (i = 0; i <= 17; i++) {
                        if (strArgIn == agResFlag[i].Name) {
                            strArgIn = agResFlag[i].Value;
                            return true;
                        }
                    }
                    break;
                case atMsg:
                    for (i = 1; i <= 2; i++) { //for gamever and gameabout and gameid
                        if (strArgIn == compGame.agResGameDef[i].Name) {
                            strArgIn = compGame.agResGameDef[i].Value;
                            return true;
                        }
                    }
                    break;
                case atSObj:
                    if (strArgIn == agResObj[0].Name) {
                        strArgIn = agResObj[0].Value;
                        return true;
                    }
                    break;
                case atStr:
                    if (strArgIn == agResStr[0].Name) {
                        strArgIn = agResStr[0].Value;
                        return true;
                    }
                    break;
                }
            }
            // if not validated above, it fails
            return false;
        }
        static internal int VariableValue(string strVar)
        {
            //this function will extract the variable number from
            //an input variable string
            //the input string should be of the form #, a# or *a#
            // where a is a valid variable prefix (v, f, s, m, w, c)
            //and # is 0-255
            //if the result is invalid, this function returns -1
            string strVarVal;
            int intVarVal;
            //if not numeric
            if (!IsNumeric(strVar)) {
                //strip off variable prefix, and indirection
                //if indirection
                if (Left(strVar, 1) == "*") {
                    strVarVal = Right(strVar, strVar.Length - 2);
                }
                else {
                    strVarVal = Right(strVar, strVar.Length - 1);
                }
            }
            else {
                //use the input Value
                strVarVal = strVar;
            }
            //if result is a number
            if (IsNumeric(strVarVal)) {
                //get number
                intVarVal = (int)Val(strVarVal);
                //for word only, subtract one to
                //account for //1// based word data type
                //(i.e. w1 is first word, but command uses arg Value of //0//)
                if (strVar[0] == 119) {
                    intVarVal--;
                }
                //verify within bounds  0-255
                if (intVarVal >= 0 && intVarVal <= 255) {
                    //return this Value
                    return intVarVal;
                }
            }
            //error/invalid - return -1
            return -1;
        }
        static internal bool AddIncludes(List<string> stlLogicText)
        {
            //this function uses the logic text that is passed to the compiler
            //to create the input text that is parsed.
            //it copies the lines from the logic text to the input text, and
            //replaces any include file lines with the actual lines from the
            //include file (include file lines are given a //header// to identify
            //them as include lines)
            List<string> IncludeLines;
            string strIncludeFilename;
            string strIncludeText;
            int CurIncludeLine;   // current line in IncludeLines (the include file)
            int intFileCount = 0;
            int lngLineCount;


            stlInput = new List<string>();
            IncludeLines = new List<string>(); //only temporary,
            lngLine = 0;
            lngLineCount = stlLogicText.Count;

            //module is always main module
            strModule = "";
            strModFileName = "";

            //step through all lines
            do {
                //set errline
                lngErrLine = lngLine;
                //check this line for include statement
                string strLine = stlLogicText[lngLine].Trim().ToLower();
                if (Left(strLine, 8) == "#include") {
                    //proper format requires a space after //include//
                    if (Mid(strLine, 9, 1) != " ") {
                        //generate error
                        strErrMsg = LoadResString(4103);
                        return false;
                    }
                    //build include filename
                    strIncludeFilename = Right(strLine, strLine.Length - 9).Trim();
                    //check for a filename
                    if (strIncludeFilename.Length == 0) {
                        strErrMsg = LoadResString(4060);
                        return false;
                    }

                    //check for correct quotes used 
                    if (Left(strIncludeFilename, 1) == QUOTECHAR && Right(strIncludeFilename, 1) != QUOTECHAR ||
                       Right(strIncludeFilename, 1) == QUOTECHAR && Left(strIncludeFilename, 1) != QUOTECHAR) {
                        if (ErrorLevel == leHigh) {
                            //return error: improper use of quote marks
                            strErrMsg = LoadResString(4059);
                            return false;
                        }
                        else {// leMedium, leLow
                              //assume quotes are needed
                            if (strIncludeFilename[0] != '"') {
                                strIncludeFilename = QUOTECHAR + strIncludeFilename;
                            }
                            if (strIncludeFilename[strIncludeFilename.Length - 1] != '"') {
                                strIncludeFilename += QUOTECHAR;
                            }
                            //set warning
                            AddWarning(5028, LoadResString(5028).Replace(ARG1, strIncludeFilename));
                        }
                    }
                    //if quotes,
                    if (Left(strIncludeFilename, 1) == QUOTECHAR) {
                        //strip off quotes
                        strIncludeFilename = Mid(strIncludeFilename, 2, strIncludeFilename.Length - 2);
                    }
                    //if filename doesnt include a path,
                    if (JustPath(strIncludeFilename, true).Length == 0) {
                        //use resource dir for this include file
                        strIncludeFilename = compGame.agResDir + strIncludeFilename;
                    }
                    //verify file exists
                    if (!File.Exists(strIncludeFilename)) {
                        strErrMsg = LoadResString(4050).Replace(ARG1, strIncludeFilename);
                        return false;
                    }
                    //now open the include file, and get the text
                    try {
                        using FileStream fsInclude = new FileStream(strIncludeFilename, FileMode.Open);
                        using StreamReader srInclude = new StreamReader(fsInclude);
                        strIncludeText = srInclude.ReadToEnd();
                        srInclude.Dispose();
                        fsInclude.Dispose();
                    }
                    catch (Exception) {
                        strErrMsg = LoadResString(4055).Replace(ARG1, strIncludeFilename);
                        return false;
                    }
                    //assign text to stringlist
                    IncludeLines = SplitLines(strIncludeText);

                    //if there are any lines,
                    if (IncludeLines.Count > 0) {
                        //save file name to allow for error checking
                        Array.Resize(ref strIncludeFile, intFileCount);
                        strIncludeFile[intFileCount] = strIncludeFilename;
                        intFileCount++;

                        //add all these lines into this position
                        for (CurIncludeLine = 0; CurIncludeLine < IncludeLines.Count; CurIncludeLine++) {
                            //verify the include file contains no includes
                            if (Left(IncludeLines[CurIncludeLine].Trim(), 2) == "#I") {
                                strErrMsg = LoadResString(4061);
                                lngErrLine = CurIncludeLine;
                                return false;
                            }
                            //include filenumber and line number from includefile
                            stlInput.Add("#I" + (intFileCount - 1).ToString() + ":" + CurIncludeLine.ToString() + "#" + IncludeLines[CurIncludeLine]);
                        }
                    }
                    //add a blank line as a place holder for the //include// line
                    //(to keep line counts accurate when calculating line number for errors)
                    stlInput.Add("");
                }
                else {
                    //not an include line
                    //check for any instances of #I, since these will
                    //interfere with include line handling
                    if (Left(stlLogicText[lngLine], 2).Equals("#i", StringComparison.OrdinalIgnoreCase)) {
                        strErrMsg = LoadResString(4069);
                        return false;
                    }
                    //copy the line by itself
                    stlInput.Add(stlLogicText[lngLine]);
                }
                lngLine++;
            }
            while (lngLine < lngLineCount); // Until lngLine >= lngLineCount
                                            //done
                                            //return success
            return true;
        } //endfunction
        internal static string ArgTypeName(ArgTypeEnum ArgType)
        {
            switch (ArgType) {
            case atNum:       //i.e. numeric Value
                return "number";
            case atVar:       //v##
                return "variable";
            case atFlag:      //f##
                return "flag";
            case atMsg:       //m##
                return "message";
            case atSObj:      //o##
                return "screen object";
            case atIObj:      //i##
                return "inventory item";
            case atStr:       //s##
                return "string";
            case atWord:      //w## -- word argument (that user types in)
                return "word";
            case atCtrl:      //c##
                return "controller";
            case atDefStr:    //defined string; could be msg, inv obj, or vocword
                return "text in quotes";
            case atVocWrd:    //vocabulary word; NOT word argument
                return "vocabulary word";
            default: // not possible; it will always be one of the above
                return "";
            }
        }
        internal static void CheckResFlagUse(byte ArgVal)
        {
            //if error level is low, don't do anything
            if (ErrorLevel == leLow) {
                return;
            }
            if (ArgVal == 2 ||
                ArgVal == 4 ||
                (ArgVal >= 7 && ArgVal <= 10) ||
                ArgVal >= 13) {
                //f2 = haveInput
                //f4 = haveMatch
                //f7 = script_buffer_blocked
                //f8 = joystick sensitivity set
                //f9 = sound_on
                //f10 = trace_abled
                //f13 = inventory_select_enabled
                //f14 = menu_enabled
                //f15 = windows_remain
                //f20 = auto_restart
                //no restrictions
            }
            else {
                //all other reserved variables should be read only
                AddWarning(5025, LoadResString(5025).Replace(ARG1, agResFlag[ArgVal].Name));
            }
        }
        static internal void CheckResVarUse(byte ArgNum, byte ArgVal)
        {
            //if error level is low, don't do anything
            if (ErrorLevel == leLow) {
                return;
            }

            switch (ArgNum) {
            //case  >= 27, 21, 15, 7, 3
            //no restrictions for
            //  all non restricted variables (>=27)
            //  curent score (v3)
            //  max score (v7)
            //  joystick sensitivity (v15)
            //  msg window delay time

            case 6: //ego direction
                    //should be restricted to values 0-8
                if (ArgVal > 8) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, agResVar[6].Name).Replace(ARG2, "8"));
                }
                break;
            case 10: //cycle delay time
                     //large values highly unusual
                if (ArgVal > 20) {
                    AddWarning(5055);
                }
                break;
            case 23: //sound attenuation
                     //restrict to 0-15
                if (ArgVal > 15) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, agResVar[23].Name).Replace(ARG2, "15"));
                }
                break;
            case 24: //max input length
                if (ArgVal > 39) {
                    AddWarning(5018, LoadResString(5018).Replace(ARG1, agResVar[24].Name).Replace(ARG2, "39"));
                }
                break;
            case 17:
            case 18: //error value, and error info
                     //resetting to zero is usually a good thing; other values don't make sense
                if (ArgVal > 0) {
                    AddWarning(5092, LoadResString(5092).Replace(ARG1, agResVar[ArgNum].Name));
                }
                break;
            case 19: //key_pressed value
                     //ok if resetting for key input
                if (ArgVal > 0) {
                    AddWarning(5017, LoadResString(5017).Replace(ARG1, agResVar[ArgNum].Name));
                }
                break;
            default: //all other reserved variables should be read only
                AddWarning(5017, LoadResString(5017).Replace(ARG1, agResVar[ArgNum].Name));
                break;
            }
        }
        static internal int GetNextArg(ArgTypeEnum ArgType, int ArgPos)
        {
            // used when not looking for a number/variable combo)
            bool nullval = false;
            return GetNextArg(ArgType, ArgPos, ref nullval);
        }
        static internal int GetNextArg(ArgTypeEnum ArgType, int ArgPos, ref bool blnVarOrNum)
        {
            //this function retrieves the next argument and validates
            //that the argument is of the correct type
            //and has a valid Value
            //
            //multline message/string/inv.item/word strings are recombined, and checked
            //for validity
            //if successful, the function returns the Value of the argument
            //if unsuccessful, the function returns a negative value:
            //  -1 = invalid conversion
            //  -2 = ')' encountered
            //  -3 = ',' encountered

            //special syntax compilers look for variables OR strings;
            //when argtype is atNum and this flag is set, numbers OR
            //variables return true; the flag is set to TRUE if returned Value
            //is for a variable, to false if it is for a number

            string strArg;
            int lngArg = 0;
            int i, retval;

            //get next command
            strArg = NextCommand();

            //convert it
            if (!ConvertArgument(ref strArg, ArgType, ref blnVarOrNum)) {
                //if a closing paren or comma found, it means one or more args missing
                if (strArg == ")" || strArg == ",") {
                    if (strArg == ")")
                    {
                        retval = -2;
                    } else
                    {
                        retval = -3;
                    }
                    // arg missing
                    lngErrNum = 4054;
                    strErrMsg = LoadResString(4054).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG3, ArgTypeName(ArgType));
                    //backup so comma/bracket can be retrieved
                    lngPos -= 1;
                }
                else {
                    // invalid conversion
                    lngErrNum = 4063;
                    strErrMsg = LoadResString(4063).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, ArgTypeName(ArgType)).Replace(ARG3, strArg);
                    retval = -1;
                }
                return retval;
            }

            switch (ArgType) {
            case atNum:  //number
                         //verify type is number
                if (!IsNumeric(strArg)) {
                    //if NOT catching variables too
                    if (!blnVarOrNum) {
                        //blnError = true;
                        strErrMsg = LoadResString(4062).Replace(ARG1, (ArgPos).ToString());
                        return -1;
                    }
                }
                else {
                    //return //is NOT a variable//; ensure flag is reset
                    blnVarOrNum = false;
                }
                //check for negative number
                if (Val(strArg) < 0) {
                    //valid negative numbers are -1 to -128
                    if (Val(strArg) < -128) {
                        //error
                        //blnError = true;
                        strErrMsg = LoadResString(4157);
                        return -1;
                    }
                    //convert it to 2s-compliment unsigned value by adding it to 256
                    strArg = (256 + Val(strArg)).ToString();
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        //show warning
                        AddWarning(5098);
                    }
                }
                //convert to number and validate
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    //blnError = true;
                    //use 1-based arg values
                    strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                    return -1;
                }
                break;
            case atVar:
            case atFlag:  //variable, flag
                          //get Value
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    //blnError = true;
                    //use 1-based arg values
                    strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                    return -1;
                }
                break;
            case atCtrl:    //controller
                            //controllers should be  0 - 49
                            //get Value
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    //blnError = true;
                    //if high errlevel
                    if (ErrorLevel == leHigh) {
                        //use 1-based arg values
                        strErrMsg = LoadResString(4136).Replace(ARG1, (ArgPos + 1).ToString());
                    }
                    else {
                        //use 1-based arg values
                        strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                    }
                    return -1;
                }
                else {
                    //if outside expected bounds (controllers should be limited to 0-49)
                    if (lngArg > 49) {
                        if (ErrorLevel == leHigh) {
                            //generate error
                            //blnError = true;
                            //use 1-based arg values
                            strErrMsg = LoadResString(4136).Replace(ARG1, (ArgPos + 1).ToString());
                            return -1;
                        }
                        else if (ErrorLevel == leMedium) {
                            //generate warning
                            AddWarning(5060);
                        }
                    }
                }
                break;
            case atSObj: //screen object
                         //get Value
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    //blnError = true;
                    //use 1-based arg values
                    strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                    return -1;
                }

                //check against max screen object Value
                if (lngArg > compGame.InvObjects.MaxScreenObjects) {
                    if (ErrorLevel == leHigh) {
                        //generate error
                        //blnError = true;
                        strErrMsg = LoadResString(4119).Replace(ARG1, (compGame.InvObjects.MaxScreenObjects).ToString());
                        return -1;

                    }
                    else if (ErrorLevel == leMedium) {
                        //generate warning
                        AddWarning(5006, LoadResString(5006).Replace(ARG1, compGame.InvObjects.MaxScreenObjects.ToString()));
                    }
                }
                break;
            case atStr: //string
                        //get Value
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    //blnError = true;
                    //if high errlevel
                    if (ErrorLevel == leHigh) {
                        //for version 2.089, 2.272, and 3.002149 only 12 strings
                        switch (compGame.agIntVersion) {
                        case "2.089":
                        case "2.272":
                        case "3.002149":
                            //use 1-based arg values
                            strErrMsg = LoadResString(4079).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, "11");
                            break;
                        default:
                            strErrMsg = LoadResString(4079).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, "23");
                            break;
                        }
                    }
                    else {
                        //use 1-based arg values
                        strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                    }
                    return -1;
                }
                else {
                    //if outside expected bounds (strings should be limited to 0-23)
                    if ((lngArg > 23) || (lngArg > 11 && (compGame.agIntVersion == "2.089" || compGame.agIntVersion == "2.272" || compGame.agIntVersion == "3.002149"))) {
                        if (ErrorLevel == leHigh) {
                            //generate error
                            //blnError = true;
                            //for version 2.089, 2.272, and 3.002149 only 12 strings
                            switch (compGame.agIntVersion) {
                            case "2.089" or "2.272" or "3.002149":
                                //use 1-based arg values
                                strErrMsg = LoadResString(4079).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, "11");
                                return -1;
                            default:
                                strErrMsg = LoadResString(4079).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, "23");
                                return -1;
                            }

                        }
                        else if (ErrorLevel == leMedium) {
                            //generate warning
                            //for version 2.089, 2.272, and 3.002149 only 12 strings
                            switch (compGame.agIntVersion) {
                            case "2.089":
                            case "2.272":
                            case "3.002149":
                                AddWarning(5007, LoadResString(5007).Replace(ARG1, "11"));
                                break;
                            default:
                                AddWarning(5007, LoadResString(5007).Replace(ARG1, "23"));
                                break;
                            }
                        }
                    }
                }
                break;
            case atWord: //word  (word type is NOT words from word.tok)
                         //get Value
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    //blnError = true;
                    //if high error level
                    if (ErrorLevel == leHigh) {
                        //use 1-based arg values
                        strErrMsg = LoadResString(4090).Replace(ARG1, (ArgPos + 1).ToString());
                    }
                    else {
                        strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                    }
                    return -1;
                }
                else {
                    //if outside expected bounds (words should be limited to 0-9)
                    if (lngArg > 9) {
                        if (ErrorLevel == leHigh) {
                            //generate error
                            //blnError = true;
                            //use 1-based arg values
                            strErrMsg = LoadResString(4090).Replace(ARG1, (ArgPos + 1).ToString());
                            return -1;
                        }
                        else if (ErrorLevel == leMedium) {
                            //generate warning
                            AddWarning(5008);
                            break;
                        }
                    }
                }
                break;
            case atMsg:  //message
                         //returned arg is either m## or "msg"
                switch ((int)strArg[0]) {
                case 109:
                    //validate Value
                    lngArg = VariableValue(strArg);
                    if (lngArg == -1) {
                        //blnError = true;
                        //use 1-based arg values
                        strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                        return -1;
                    }
                    //m0 is not allowed
                    if (lngArg == 0) {
                        if (ErrorLevel == leHigh) {
                            //blnError = true;
                            strErrMsg = LoadResString(4107);
                            return -1;
                        }
                        else if (ErrorLevel == leMedium) {
                            //generate warning
                            AddWarning(5091, LoadResString(5091).Replace(ARG1, lngArg.ToString()));
                            //make this a null msg
                            blnMsg[lngArg] = true;
                            strMsg[lngArg] = "";
                            // }else if (ErrorLevel == leLow) {
                            //ignore; it will be handled when writing messages
                        }
                    }
                    //verify msg exists
                    if (!blnMsg[lngArg]) {
                        if (ErrorLevel == leHigh) {
                            //blnError = true;
                            strErrMsg = LoadResString(4113).Replace(ARG1, lngArg.ToString());
                            return -1;
                        }
                        else if (ErrorLevel == leMedium) {
                            //generate warning
                            AddWarning(5090, LoadResString(5090).Replace(ARG1, lngArg.ToString()));
                            //make this a null msg
                            blnMsg[lngArg] = true;
                            strMsg[lngArg] = "";
                            // }else if (ErrorLevel == leLow) {
                            //ignore; WinAGI adds a null value, so no error will occur
                        }
                    }
                    break;
                case 34:
                    //concatenate, if applicable
                    strArg = ConcatArg(strArg);
                    if (blnCriticalError) {
                        //concatenation error; exit
                        return -1;
                    }
                    //strip off quotes
                    strArg = Mid(strArg, 2, strArg.Length - 2);
                    //convert to msg number
                    lngArg = MessageNum(strArg);
                    //if unallowed characters found, error was raised; exit
                    if (lngArg == -1) {
                        //blnError = true;
                        return -1;
                    }
                    //if valid number not found
                    if (lngArg == 0) {
                        //blnError = true;
                        strErrMsg = LoadResString(4092);
                        return -1;
                    }
                    break;
                }
                break;
            case atIObj: //inventory object
                         //only true restriction is can't exceed object count, and can't exceed 255 objects (0-254)
                         //i0 is usually a '?', BUT not a strict requirement
                         //HOWEVER, WinAGI enforces that i0 MUST be '?', and can't be changed
                         //also, if any code tries to access an object by '?', return error

                //if character is inv obj arg type prefix
                switch ((int)strArg[0]) {
                case 105: //'i'
                          //validate Value
                    lngArg = VariableValue(strArg);
                    if (lngArg == -1) {
                        //blnError = true;
                        //use 1-based arg values
                        strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
                        return -1;
                    }
                    else if (lngArg >= compGame.InvObjects.Count) {
                        if (ErrorLevel == leHigh) {
                            //blnError = true;
                            //use 1-based arg values
                            strErrMsg = LoadResString(4112).Replace(ARG1, (ArgPos + 1).ToString());
                            return -1;
                        }
                        else if (ErrorLevel == leMedium) {
                            //set warning
                            //use 1-based arg values
                            AddWarning(5005, LoadResString(5005).Replace(ARG1, (ArgPos + 1).ToString()));
                            //if (ErrorLevel == leLow) {
                            //no action
                        }
                    }
                    break;
                case 34: // quote
                         //concatenate, if applicable
                    strArg = ConcatArg(strArg);
                    if (blnCriticalError) {
                        //concatenation error
                        return -1;
                    }
                    //convert to inv obj number
                    //first strip off starting and ending quotes
                    strArg = Mid(strArg, 2, strArg.Length - 2);
                    //if a quotation mark is part of an object name,
                    //it is coded in the logic as a '\"' not just a '"'
                    //need to ensure all '\"' codes are converted to '"'
                    //otherwise the object would never match
                    strArg = strArg.Replace("\\\"", QUOTECHAR);

                    //step through all object names
                    for (i = 0; i < compGame.InvObjects.Count; i++) {
                        //if this is the object
                        if (strArg == compGame.InvObjects[(byte)i].ItemName) {
                            //return this Value
                            lngArg = (byte)i;
                            break;
                        }
                    }

                    //if not found,
                    if (i == compGame.InvObjects.Count) {
                        //blnError = true;
                        //check for added quotes; they are the problem
                        if (lngQuoteAdded >= 0) {
                            //reset line;
                            lngLine = lngQuoteAdded;
                            //string error
                            strErrMsg = LoadResString(4051);
                        }
                        else {
                            //use 1-base arg values
                            strErrMsg = LoadResString(4075).Replace(ARG1, (ArgPos + 1).ToString());
                        }
                        return -1;
                    }

                    //if object is not unique
                    if (!compGame.InvObjects[(byte)lngArg].Unique) {
                        if (ErrorLevel == leHigh) {
                            //blnError = true;
                            //use 1-based arg values
                            strErrMsg = LoadResString(4036).Replace(ARG1, (ArgPos + 1).ToString());
                            return -1;
                        }
                        else if (ErrorLevel == leMedium) {
                            //set warning
                            AddWarning(5003, LoadResString(5003).Replace(ARG1, (ArgPos + 1).ToString()));
                            //} else if (ErrorLevel == leLow) {
                            //no action
                        }
                    }
                    break;
                default: //any other character means not a valid inventory item
                         //use 1-base arg values
                    strErrMsg = LoadResString(4075).Replace(ARG1, (ArgPos + 1).ToString());
                    return -1;
                }

                //if object number is valid
                if (lngArg < compGame.InvObjects.Count) {
                    //check for question mark, raise error/warning
                    if (compGame.InvObjects[(byte)lngArg].ItemName == "?") {
                        if (ErrorLevel == leHigh) {
                            //blnError = true;
                            //use 1-based arg values
                            strErrMsg = LoadResString(4111).Replace(ARG1, (ArgPos + 1).ToString());
                            return -1;
                        }
                        else if (ErrorLevel == leMedium) {
                            //set warning
                            AddWarning(5004);
                            //if (ErrorLevel == leLow) {
                            //no action
                        }
                    }
                }
                break;
            case atVocWrd:
                //words can be ## or "word"
                if (IsNumeric(strArg)) {
                    lngArg = int.Parse(strArg);
                    //make sure it's not a decimal
                    if (Val(strArg) != lngArg) {
                        //blnError = true;
                        lngArg = -1;
                    }
                    else {
                        //validate the group
                        //blnError = !compGame.agVocabWords.GroupExists(lngArg);
                    }
                }
                else {
                    //this is a string; concatenate if applicable
                    strArg = ConcatArg(strArg);
                    if (blnCriticalError) {
                        //concatenation error
                        return -1;
                    }

                    //convert to word number
                    //first strip off starting and ending quotes
                    strArg = Mid(strArg, 2, strArg.Length - 2);

                    //get argument val by checking against word list
                    if (compGame.agVocabWords.WordExists(strArg)) {
                        lngArg = compGame.agVocabWords[strArg].Group;
                    }
                    else {
                        //RARE, but if it's an 'a' or 'i' that isn't defined,
                        //it's word group 0
                        if (strArg == "i" || strArg == "a" || strArg == "I" || strArg == "A") {
                            lngArg = 0;
                            //add warning
                            if (ErrorLevel == leHigh || ErrorLevel == leMedium) {
                                AddWarning(5108, LoadResString(5108).Replace(ARG1, strArg));
                            }
                        }
                        else {
                            //set error flag
                            //blnError = true;
                            //set arg to invalid number
                            lngArg = -1;
                        }
                    }
                }

                //now lngArg is a valid group number, unless blnError is set

                //if there is an error
                if (blnCriticalError) {
                    //if arg value=-1 OR high level,
                    if (ErrorLevel == leHigh || (lngArg == -1)) {
                        //argument is already 1-based for said tests
                        strErrMsg = LoadResString(4114).Replace(ARG1, strArg);
                        return -1;
                    }
                    else {
                        if (ErrorLevel == leMedium) {
                            //set warning
                            AddWarning(5019, LoadResString(5019).Replace(ARG1, strArg));
                            //blnError = false;
                        }
                    }
                }
                //check for group 0
                if (lngArg == 0) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4035).Replace(ARG1, strArg);
                        return -1;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5083, LoadResString(5083).Replace(ARG1, strArg));
                        //} else if (ErrorLevel == leLow) {
                        // no action 
                    }
                }
                break;
            }

            //return the arg value
            return lngArg;
        } //endfunction
        static internal void IncrementLine()
        {
            //increments the the current line of input being processed
            //sets all counters, pointers, etc
            //as well as info needed to support error locating

            //if at end of input (lngLine=-1)
            if (lngLine == -1) {
                //just exit
                return;
            }
            //if compiler is reset
            if (lngLine == -2) {
                //set it to -1 so line 0 is returned
                lngLine = -1;
            }
            //increment line counter
            lngLine++;
            lngPos = 0;
            //if at end,
            if (lngLine >= stlInput.Count) {
                lngLine = -1;
                return;
            }
            //check for include lines
            if (Left(stlInput[lngLine], 2).Equals("#I", StringComparison.OrdinalIgnoreCase)) {
                lngIncludeOffset++;
                //set module
                int mod = int.Parse(Mid(stlInput[lngLine], 3, stlInput[lngLine].IndexOf(":") - 3));
                strModule = strIncludeFile[mod];
                strModFileName = Path.GetFileName(strModule);
                //set errline
                lngErrLine = int.Parse(Mid(stlInput[lngLine], stlInput[lngLine].IndexOf(":") + 1, stlInput[lngLine].IndexOf("#") - 5));
                strCurrentLine = Right(stlInput[lngLine], stlInput[lngLine].Length - stlInput[lngLine].IndexOf("#"));
            }
            else {
                strModule = "";
                strModFileName = "";
                lngErrLine = lngLine - lngIncludeOffset;
                //set string
                strCurrentLine = stlInput[lngLine];
            }
        }
        static internal string NextChar(bool blnNoNewLine = false)
        {
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
                return "";
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
                        return "";
                    }
                    //get the next line
                    IncrementLine();
                    //if at end of input
                    if (lngLine == -1) {
                        //exit with no character
                        return "";
                    }
                    //increment pointer(so it points to first character of line)
                    lngPos++;
                }
                string retval = strCurrentLine[lngPos].ToString();
                //only characters <32 that we need to use are return, and linefeed
                if (retval != "") {
                    if ((byte)retval[0] < 32) {
                        switch ((byte)retval[0]) {
                        case 10:
                        case 13: //treat as returns?
                            return "\n"; // vbCr
                        default:
                            return " ";
                        }
                    }
                }
                return retval;
            }
            while (true);// Loop Until NextChar != ' ' && LenB(NextChar) != 0
        } //endfunction
        static internal string NextCommand(bool blnNoNewLine = false)
        {
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
            string retval = NextChar(blnNoNewLine);
            //if at end of input,
            if (lngLine == -1) {
                //return empty string
                return "";
            }
            //if no character returned
            if (retval.Length == 0) {
                return "";
            }
            //if command is a element separator:
            if ("'(),:;?[\\]^`{}~".Any(retval.Contains)) {
                //return this single character as a command
                return retval;
            }
            // check for other characters using a switch
            switch ((byte)retval[0]) {
            case 61: // =
                     //special case; "=", "=<" and "=>" returned as separate commands
                switch (strCurrentLine[lngPos + 1]) {
                case '<':
                case '>':
                    //increment pointer
                    lngPos++;
                    //return the two byte cmd (swap so we get ">=" and "<="
                    // instead of "=>" and "=<"
                    retval = ((char)strCurrentLine[lngPos]).ToString() + retval;
                    break;
                case '=': //"=="
                          //increment pointer
                    lngPos++;
                    //return the two byte cmd
                    retval = "==";
                    break;
                }
                return retval;
            case 34: //"
                     //special case; quote means start of a string
                blnInQuotes = true;
                break;
            case 43: //+
                     //special case; "+", "++" and "+=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '+') {
                    //increment pointer
                    lngPos++;
                    //return shorthand increment
                    retval = "++";
                }
                else if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    //return shorthand addition
                    retval = "+=";
                }
                return retval;
            case 45: //-
                     //special case; "-", "--" and "-=" returned as separate commands
                     //also check for negative numbers ("-##")
                if (strCurrentLine[lngPos + 1] == '-') {
                    //increment pointer
                    lngPos++;
                    //return shorthand decrement
                    retval = "--";
                }
                else if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    //return shorthand subtract
                    retval = "-=";
                }
                else if (IsNumeric(strCurrentLine[lngPos + 1].ToString())) {
                    //return a negative number

                    //continue adding characters until non-numeric or EOL is reached
                    while (lngPos + 1 <= strCurrentLine.Length) { // Do Until lngPos + 1 > Len(strCurrentLine)
                        intChar = (int)strCurrentLine[lngPos + 1];
                        if (intChar < 48 || intChar > 57) {
                            //anything other than a digit (0-9)
                            break;
                        }
                        else {
                            //add character
                            retval += ((char)intChar).ToString();
                            //incrment position
                            lngPos++;
                        }
                    }
                }
                return retval;
            case 33: //!
                     //special case; "!" and "!=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    //increment pointer
                    lngPos++;
                    //return not equal
                    retval = "!=";
                }
                return retval;
            case 60: //<
                     //special case; "<", "<=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    //increment pointer
                    lngPos++;
                    //return less than or equal
                    retval = "<=";
                    //} else if ( strCurrentLine[lngPos + 1] == '>') {
                    //  //increment pointer
                    //  lngPos++;
                    //  //return not equal
                    //  NextCommand = "<>";
                }
                return retval;
            case 62: //>
                     //special case; ">", ">=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    //increment pointer
                    lngPos++;
                    //return greater than or equal
                    retval = ">=";
                    //} else if ( strCurrentLine[lngPos + 1] == "<") {
                    //  //increment pointer
                    //  lngPos++;
                    //  //return not equal ('><' is same as '<>')
                    //  retval = "<>";
                }
                return retval;
            case 42: //*
                     //special case; "*" and "*=" returned as separate commands;
                if (strCurrentLine[lngPos + 1] == '=') {
                    //increment pointer
                    lngPos++;
                    //return shorthand multiplication
                    retval = "*=";
                    //since block commands are no longer supported, check for them in order to provide a
                    //meaningful error message
                }
                else if (strCurrentLine[lngPos + 1] == '/') {
                    lngPos++;
                    retval = "*/";
                }
                return retval;
            case 47: // /
                     //special case; "/" , "//" and "/=" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '=') {
                    lngPos++;
                    //return shorthand division
                    retval = "/=";
                }
                else if (strCurrentLine[lngPos + 1] == '/') {
                    lngPos++;
                    retval = "//";
                    //since block commands are no longer supported, check for the in order to provide a
                    //meaningful error message
                }
                else if (strCurrentLine[lngPos + 1] == '*') {
                    lngPos++;
                    retval = "/*";
                }
                return retval;
            case 124: //|
                      //special case; "|" and "||" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '|') {
                    //increment pointer
                    lngPos++;
                    //return double ||
                    retval = "||";
                }
                return retval;
            case 38: //&
                     //special case; "&" and "&&" returned as separate commands
                if (strCurrentLine[lngPos + 1] == '&') {
                    //increment pointer
                    lngPos++;
                    //return double //&//
                    retval = "&&";
                }
                return retval;
            }

            //if not a text string,
            if (!blnInQuotes) {
                //continue adding characters until element separator or EOL is reached
                while (lngPos < strCurrentLine.Length) // Until lngPos + 1 > Len(strCurrentLine)
                  {
                    intChar = (byte)strCurrentLine[lngPos + 1];
                    if ((intChar >= 32 && intChar <= 34) ||
                        (intChar >= 38 && intChar <= 45) ||
                        intChar == 47 ||
                        (intChar >= 58 && intChar <= 63) ||
                        (intChar >= 91 && intChar <= 94) ||
                        intChar == 96 ||
                        (intChar >= 123 && intChar <= 126)) {
                        //case 32, 33, 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, 123 To 126
                        //  space, !"&//() *+,-/:;<=>?[\]^`{|}~ 
                        //end of command text found
                        break;
                    }
                    else {
                        //add character
                        retval += ((char)intChar).ToString();
                        //incrment position
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
                    char cval = strCurrentLine[lngPos + 1];
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
                        if (cval == '"') {
                            //34: //quote mark
                            //a quote marks end of string
                            blnInQuotes = false;
                        }
                        else if (cval == '\\') {
                            // 92: //slash
                            blnSlash = true;
                        }
                    }
                    retval += cval.ToString();

                    //if at end of line
                    if (lngPos == strCurrentLine.Length) {
                        //if still in quotes,
                        if (blnInQuotes) {
                            //set inquotes to false to exit the loop
                            //the compiler will have to recognize that
                            //this text string is not properly enclosed in quotes
                            blnInQuotes = false;
                        }
                    }
                } while (blnInQuotes);
            }
            // return the command
            return retval;
        }
        static internal string ConcatArg(string strText)
        {
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
            int lngLastPos, lngLastLine;
            string strLastLine;
            int lngSlashCount, lngQuotesOK;
            string retval;
            //verify at least two characters
            if (strText.Length < 2) {
                //error
                //blnError = true;
                strErrMsg = LoadResString(4081);
                return "";
            }

            //start with input string
            retval = strText;
            //save current position info
            lngLastPos = lngPos;
            lngLastLine = lngLine;
            strLastLine = strCurrentLine;
            //if at end of last line
            if (lngLastPos == strLastLine.Length) {
                //get next command
                strTextContinue = NextCommand();
                //add strings until concatenation is complete
                while (Left(strTextContinue, 1) == QUOTECHAR) { // Until Left(strTextContinue, 1) != QUOTECHAR
                                                                //if a continuation string is found, we need to reset
                                                                //the quote checker
                    lngQuotesOK = 0;
                    //check for end quote
                    if (Right(strTextContinue, 1) != QUOTECHAR) {
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
                            if (Mid(retval, retval.Length - (lngSlashCount + 1), 1) == "\\") {
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

                        if (ErrorLevel == leHigh) {
                            //error
                            //blnError = true;
                            strErrMsg = LoadResString(4080);
                            return "";
                        }
                        else if (ErrorLevel == leHigh) {
                            //add quote
                            strTextContinue += QUOTECHAR;
                            //set warning
                            AddWarning(5002);
                        }
                        else if (ErrorLevel == leLow) {
                            //just add quote
                            strTextContinue += QUOTECHAR;
                        }
                    }

                    //strip off ending quote of current msg
                    retval = Left(retval, retval.Length - 1);
                    //add it to strText
                    retval += Right(strTextContinue, strTextContinue.Length - 1);
                    //save current position info
                    lngLastPos = lngPos;
                    lngLastLine = lngLine;
                    strLastLine = strCurrentLine;
                    //get next command
                    strTextContinue = NextCommand();
                }

                //after end of string found, move back to correct position
                lngPos = lngLastPos;
                lngLine = lngLastLine;
                lngErrLine = lngLastLine;
                strCurrentLine = strLastLine;
            }
            return retval;
        }
        static internal bool RemoveComments()
        {
            //this function strips out comments from the input text
            //and trims off leading and trailing spaces
            //
            //agi comments:
            //      // - rest of line is ignored
            //      [ - rest of line is ignored
            int lngPos;
            bool blnInQuotes = false, blnSlash = false;
            int intROLIgnore;
            //reset compiler
            ResetCompiler();
            lngIncludeOffset = 0;
            lngLine = 0;

            do {
                //reset rol ignore
                intROLIgnore = 0;

                //reset comment start + char ptr, and inquotes
                lngPos = 0;
                blnInQuotes = false;
                //if this line is not empty,
                if (strCurrentLine.Length != 0) {
                    while (lngPos < strCurrentLine.Length - 1) { // Until lngPos >= Len(strCurrentLine)
                                                                 //get next character from string
                        lngPos++;
                        //if NOT inside a quotation,
                        if (!blnInQuotes) {
                            //check for comment characters at this position
                            if ((Mid(strCurrentLine, lngPos, 2) == CMT2_TOKEN) || (Mid(strCurrentLine, lngPos, 1) == CMT1_TOKEN)) {
                                intROLIgnore = lngPos;
                                break;
                            }
                            // slash codes never occur outside quotes
                            blnSlash = false;
                            //if this character is a quote mark, it starts a string
                            blnInQuotes = Mid(strCurrentLine, lngPos, 1) == QUOTECHAR;
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
                                switch (Mid(strCurrentLine, lngPos, 1)) {
                                case QUOTECHAR: // 34 //quote mark
                                                //a quote marks end of string
                                    blnInQuotes = false;
                                    break;
                                case "\\": // 92 //slash
                                    blnSlash = true;
                                    break;
                                }
                            }
                        }
                    }
                    //if any part of line should be ignored,
                    if (intROLIgnore > 0) {
                        strCurrentLine = Left(strCurrentLine, intROLIgnore - 1);
                    }
                }
                //replace comment, also trim it
                ReplaceLine(strCurrentLine.ToString());

                //get next line
                IncrementLine();
            } while (lngLine != -1); // Until lngLine = -1

            //success
            return true;
        } //endfunction
        static internal void ReplaceLine(string strNewLine)
        {
            //this function replaces the current line in the input string
            //with the strNewLine, while preserving include header info

            string strInclude;

            //if this is from an include file
            if (Left(stlInput[lngLine], 2).Equals("#I", StringComparison.OrdinalIgnoreCase)) {
                //need to save include header info so it can
                //be preserved after comments are removed
                strInclude = Left(stlInput[lngLine], stlInput[lngLine].IndexOf("#"));
            }
            else {
                strInclude = "";
            }

            //replace the line
            stlInput[lngLine] = strInclude + strNewLine;
        } //endsub
        static internal void ResetCompiler()
        {
            //resets the compiler so it points to beginning of input
            //also loads first line into strCurrentLine

            //reset include offset, so error trapping
            //can correctly Count lines
            lngIncludeOffset = 0;

            //reset error flag
            //blnError = false;
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
        static internal bool ReadDefines()
        {
            int i;
            TDefine tdNewDefine = new TDefine { Name = "", Value = "" };
            int rtn;
            //reset Count of defines
            lngDefineCount = 0;

            //reset compiler
            ResetCompiler();

            //reset error string
            strErrMsg = "";

            //step through all lines and find define values
            do {
                //check for define statement
                if (strCurrentLine.IndexOf(CONST_TOKEN) == 1) {
                    //strip off define keyword
                    strCurrentLine = Right(strCurrentLine, strCurrentLine.Length - CONST_TOKEN.Length).Trim();
                    //if equal marker (i.e. space) not present
                    if (!strCurrentLine.Contains(" ")) { // (strCurrentLine.IndexOf(" ") == -1) {
                                                         //error
                        strErrMsg = LoadResString(4104);
                        return false;
                    }

                    //split it by position of first space
                    tdNewDefine.Name = Left(strCurrentLine, strCurrentLine.IndexOf(" ") - 1).Trim();
                    tdNewDefine.Value = Right(strCurrentLine, strCurrentLine.Length - strCurrentLine.IndexOf(" ")).Trim();
                    //validate define name
                    DefineNameCheck chkName = compGame.agGlobals.ValidateDefName(tdNewDefine);
                    //some name errors are only warnings if error level is medium or low
                    if (ErrorLevel == leMedium) {
                        //check for name warnings
                        switch (chkName) {
                        case DefineNameCheck.ncGlobal:
                            //set warning
                            AddWarning(5034, LoadResString(5034).Replace(ARG1, tdNewDefine.Name));
                            //reset return error code
                            chkName = DefineNameCheck.ncOK;
                            break;
                        case DefineNameCheck.ncGlobal or
                        DefineNameCheck.ncReservedFlag or
                        DefineNameCheck.ncReservedMsg or
                        DefineNameCheck.ncReservedNum or
                        DefineNameCheck.ncReservedObj or
                        DefineNameCheck.ncReservedStr or
                        DefineNameCheck.ncReservedVar:
                            //set warning
                            AddWarning(5035, LoadResString(5035).Replace(ARG1, tdNewDefine.Name));
                            //reset return error code
                            chkName = DefineNameCheck.ncOK;
                            break;
                        }
                    }
                    else if (ErrorLevel == leLow) {
                        //check for warnings
                        switch (chkName) {
                        case DefineNameCheck.ncGlobal or
                        DefineNameCheck.ncReservedFlag or
                        DefineNameCheck.ncReservedMsg or
                        DefineNameCheck.ncReservedNum or
                        DefineNameCheck.ncReservedObj or
                        DefineNameCheck.ncReservedStr or
                        DefineNameCheck.ncReservedVar:
                            //reset return error code
                            chkName = DefineNameCheck.ncOK;
                            break;
                        }
                    }
                    //now check for errors
                    if (chkName != DefineNameCheck.ncOK) {
                        //check for name errors
                        switch (chkName) {
                        case DefineNameCheck.ncEmpty: // no name
                            strErrMsg = LoadResString(4070);
                            break;
                        case DefineNameCheck.ncNumeric: // name is numeric
                            strErrMsg = LoadResString(4072);
                            break;
                        case DefineNameCheck.ncActionCommand: // name is command
                            strErrMsg = LoadResString(4021).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncTestCommand: // name is test command
                            strErrMsg = LoadResString(4022).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncKeyWord: // name is a compiler keyword
                            strErrMsg = LoadResString(4013).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncArgMarker: // name is an argument marker
                            strErrMsg = LoadResString(4071);
                            break;
                        case DefineNameCheck.ncGlobal: // name is already globally defined
                            strErrMsg = LoadResString(4019).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncReservedVar: // name is reserved variable name
                            strErrMsg = LoadResString(4018).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncReservedFlag: // name is reserved flag name
                            strErrMsg = LoadResString(4014).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncReservedNum: // name is reserved number constant
                            strErrMsg = LoadResString(4016).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncReservedObj or DefineNameCheck.ncReservedStr:
                            // name is reserved object constant
                            // name is a reserved string constant
                            strErrMsg = LoadResString(4017).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncReservedMsg: // name is reserved message constant
                            strErrMsg = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
                            break;
                        case DefineNameCheck.ncBadChar: // name contains improper character
                            strErrMsg = LoadResString(4067);
                            break;
                        }
                        //don't exit; check for define Value errors first
                    }

                    //validate define Value
                    DefineValueCheck chkValue = compGame.agGlobals.ValidateDefValue(tdNewDefine);
                    //Value errors 4,5,6 are only warnings if error level is medium or low
                    if (ErrorLevel == leMedium) {
                        //if Value error is due to missing quotes
                        switch (chkValue) {
                        case DefineValueCheck.vcNotAValue:  //assume it's a string Value missing quotes
                                                            //fix the define Value
                            if (tdNewDefine.Value[0] != '"') {
                                tdNewDefine.Value = QUOTECHAR + tdNewDefine.Value;
                            }
                            if (tdNewDefine.Value[tdNewDefine.Value.Length - 1] != '"') {
                                tdNewDefine.Value += QUOTECHAR;
                            }
                            //set warning
                            AddWarning(5022);
                            //reset error code
                            chkValue = DefineValueCheck.vcOK;
                            break;
                        case DefineValueCheck.vcGlobal: // Value is already defined by a global name
                                                        //set warning
                            AddWarning(5031, LoadResString(5031).Replace(ARG1, tdNewDefine.Value));
                            //reset error code
                            chkValue = DefineValueCheck.vcOK;
                            break;
                        case DefineValueCheck.vcReserved: // Value is already defined by a reserved name
                                                          //set warning
                            AddWarning(5032, LoadResString(5032).Replace(ARG1, tdNewDefine.Value));
                            //reset error code
                            chkValue = DefineValueCheck.vcOK;
                            break;
                        }
                    }
                    else if (ErrorLevel == leLow) {
                        //if Value error is due to missing quotes
                        switch (chkValue) {
                        case DefineValueCheck.vcNotAValue:
                            //fix the define Value
                            if (tdNewDefine.Value[0] != '"') {
                                tdNewDefine.Value = QUOTECHAR + tdNewDefine.Value;
                            }
                            if (tdNewDefine.Value[^1] != '"') {
                                tdNewDefine.Value += QUOTECHAR;
                            }
                            //reset return Value
                            chkValue = DefineValueCheck.vcOK;
                            break;
                        case DefineValueCheck.vcGlobal or DefineValueCheck.vcReserved:
                            //reset return Value
                            chkValue = DefineValueCheck.vcOK;
                            break;
                        }
                    }
                    //check for errors
                    if (chkValue != DefineValueCheck.vcOK) {
                        //if already have a name error
                        if (strErrMsg.Length != 0) {
                            //append Value error
                            strErrMsg += "; and ";
                        }
                        //check for Value error
                        switch (chkValue) {
                        case DefineValueCheck.vcEmpty: // no Value
                            strErrMsg += LoadResString(4073);
                            break;
                        case DefineValueCheck.vcBadArgNumber: // Value contains an invalid argument Value
                            strErrMsg += LoadResString(4042);
                            break;
                        case DefineValueCheck.vcNotAValue: // Value is not a string, number or argument marker
                            strErrMsg += LoadResString(4082);
                            break;
                        case DefineValueCheck.vcGlobal: // Value is already defined by a global name
                            strErrMsg += LoadResString(4040).Replace(ARG1, tdNewDefine.Value);
                            break;
                        case DefineValueCheck.vcReserved: // Value is already defined by a reserved name
                            strErrMsg += LoadResString(4041).Replace(ARG1, tdNewDefine.Value);
                            break;
                        }
                    }
                    //if an error was generated during define validation
                    if (strErrMsg.Length != 0) {
                        return false;
                    }
                    //check all previous defines
                    for (i = 0; i < lngDefineCount; i++) {
                        if (tdNewDefine.Name == tdDefines[i].Name) {
                            strErrMsg = LoadResString(4012).Replace(ARG1, tdDefines[i].Name);
                            return false;
                        }
                        if (tdNewDefine.Value == tdDefines[i].Value) {
                            //numeric duplicates aren't a concern
                            if (!IsNumeric(tdNewDefine.Value)) {
                                if (ErrorLevel == leHigh) {
                                    //set error
                                    strErrMsg = LoadResString(4023).Replace(ARG1, tdDefines[i].Value).Replace(ARG2, tdDefines[i].Name);
                                    return false;
                                }
                                else if (ErrorLevel == leMedium) {
                                    //set warning
                                    AddWarning(5033, LoadResString(5033).Replace(ARG1, tdNewDefine.Value).Replace(ARG2, tdDefines[i].Name));
                                    //} else if (ErrorLevel == leLow) {
                                    //   //do nothing
                                }
                            }
                        }
                    }

                    //check define against labels
                    if (bytLabelCount > 0) {
                        for (i = 0; i < bytLabelCount; i++) {
                            if (tdNewDefine.Name == llLabel[i].Name) {
                                strErrMsg = LoadResString(4020).Replace(ARG1, tdNewDefine.Name);
                                return false;
                            }
                        }
                    }
                    //save this define
                    Array.Resize(ref tdDefines, lngDefineCount);
                    tdDefines[lngDefineCount - 1] = tdNewDefine;

                    //increment counter
                    lngDefineCount++;

                    //now set this line to empty so Compiler doesn"t try to read it
                    if (Left(stlInput[lngLine], 2).Equals("#I", StringComparison.OrdinalIgnoreCase)) {
                        //this is an include line; need to leave include line info
                        stlInput[lngLine] = Left(stlInput[lngLine], stlInput[lngLine].IndexOf("#"));
                    }
                    else {
                        //just blank out entire line
                        stlInput[lngLine] = "";
                    }
                }
                //get next line
                IncrementLine();
            } while (lngLine != -1); //Loop Until lngLine = -1


            return true;
        } //endfunction
        static internal bool ReadMsgs()
        {
            //note that stripped message lines also strip out the include header string
            //this doesn't matter since they are only blank lines anyway
            //only need to include header info if error occurs, and errors never occur on
            //blank line

            int intMsgNum;
            string strCmd;
            int lngMsgStart;
            bool blnDef = false;
            int lngSlashCount, lngQuotesOK;

            //build blank message list
            for (intMsgNum = 0; intMsgNum <= 255; intMsgNum++) {
                strMsg[intMsgNum] = "";
                blnMsg[intMsgNum] = false;
                intMsgWarn[intMsgNum] = 0;
            }
            //reset compiler
            ResetCompiler();
            do {
                //get first command on this line
                strCmd = NextCommand(true);
                //if this is the message marker
                if (strCmd == MSG_TOKEN) {
                    //save starting line number (incase this msg uses multiple lines)
                    lngMsgStart = lngLine;
                    //get next command on this line (should be message number)
                    strCmd = NextCommand(true);
                    //this should be a msg number
                    if (!IsNumeric(strCmd)) {
                        //error
                        //blnError = true;
                        strErrMsg = LoadResString(4077);
                        return false;
                    }
                    //validate msg number
                    intMsgNum = VariableValue(strCmd);
                    if (intMsgNum <= 0) {
                        //error
                        //blnError = true;
                        strErrMsg = LoadResString(4077);
                        return false;
                    }
                    //if msg is already assigned
                    if (blnMsg[intMsgNum]) {
                        //blnError = true;
                        strErrMsg = LoadResString(4094).Replace(ARG1, (intMsgNum).ToString());
                        return false;
                    }
                    //get next command (should be the message text)
                    strCmd = NextCommand(false);
                    //is this a valid string?
                    if (!IsValidMsg(strCmd)) {
                        //maybe it's a define
                        if (ConvertArgument(ref strCmd, atMsg)) {
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
                    if (strCmd[0] != '"') {
                        //bad start quote
                        lngQuotesOK = 1;
                    }
                    //check for end quote
                    if (strCmd[strCmd.Length - 1] != '"') {
                        //bad end quote
                        lngQuotesOK += 2;
                    }
                    else {
                        //just because it ends in a quote doesn't mean it's good;
                        //it might be an embedded quote
                        //(we know we have at least two chars, so we don't need
                        //to worry about an error with Mid function)

                        //check for an odd number of slashes immediately preceding
                        //this quote
                        lngSlashCount = 0;
                        do {
                            if (strCmd[strCmd.Length - (lngSlashCount + 1)] == '\\') {
                                lngSlashCount++;
                            }
                            else {
                                break;
                            }
                        } while (strCmd.Length - (lngSlashCount + 1) >= 0);

                        //if it IS odd, then it's not a valid quote
                        if (lngSlashCount % 2 == 1) {
                            //it's embedded, and doesn't count
                            lngQuotesOK += 2;
                        }
                    }
                    //if either (or both) quote is missing, deal with it
                    if (lngQuotesOK > 0) {
                        //note which line had quotes added, in case it results
                        //in an error caused by a missing end ')' or whatever
                        //the next required element is
                        lngQuoteAdded = lngLine;

                        if (ErrorLevel == leHigh) {
                            //blnError = true;
                            strErrMsg = LoadResString(4051);
                            return false;
                        }
                        else { // leMedium, leLow
                               //add quotes as appropriate
                            if ((lngQuotesOK & 1) == 1) {
                                strCmd = QUOTECHAR + strCmd;
                            }
                            if ((lngQuotesOK & 2) == 2) {
                                strCmd += QUOTECHAR;
                            }
                            //warn if medium
                            if (ErrorLevel == leMedium) {
                                //set warning
                                AddWarning(5002);
                            }
                        }
                    }
                    //concatenate, if necessary
                    if (!blnDef) {
                        strCmd = ConcatArg(strCmd);
                        //if error,
                        if (blnCriticalError) {
                            return false;
                        }
                    }
                    //nothing allowed after msg declaration
                    if (lngPos != strCurrentLine.Length) {
                        //error
                        //blnError = true;
                        strErrMsg = LoadResString(4099);
                        return false;
                    }

                    //strip off quotes (we know that the string
                    //is properly enclosed by quotes because
                    //ConcatArg function validates they are there
                    //or adds them if they aren't[or raises an
                    //error, in which case it doesn't even matter])
                    strCmd = Mid(strCmd, 2, strCmd.Length - 2);

                    //add msg
                    strMsg[intMsgNum] = strCmd;
                    //validate message characters
                    if (!ValidateMsgChars(strCmd, intMsgNum)) {
                        //error was raised
                        return false;
                    }
                    // mark message as assigned
                    blnMsg[intMsgNum] = true;

                    //set the msg line (and any concatenated lines) to empty so
                    //compiler doesn't try to read it
                    do {
                        stlInput[lngMsgStart] = "";
                        //increment the counter (to get multiple lines, if string is
                        //concatenated over more than one line)
                        lngMsgStart++;
                        //continue until back to current line
                    } while (lngMsgStart <= lngLine); //Loop Until lngMsgStart > lngLine

                }
                //get next line
                IncrementLine();
            } while (lngLine != -1); //Loop Until lngLine = -1

            //check for any undeclared messages that haven't already been identified
            //(they are not really a problem, but user might want to know)
            //all messages from 1 to this point should be declared
            //??? there isn't any code here that does this check???

            //find last used msg number (why?)
            intMsgNum = 255;
            while (!blnMsg[intMsgNum] && intMsgNum != 0) { // Until blnMsg(intMsgNum) || intMsgNum = 0
                intMsgNum--;
            }
            // done
            return true;
        } //endfunction
        static internal void AddWarning(int WarningNum, string WarningText = "")
        {
            //(number, line and module only have meaning for logic warnings
            // other warnings generated during a game compile will use
            // same format, but use -- for warning number, line and module)

            //if no text passed, use the default resource string
            if (WarningText.Length == 0) {
                WarningText = LoadResString(WarningNum);
            }
            //only add if not ignoring
            if (!agNoCompWarn[WarningNum - 5000]) {
                TWarnInfo tmpWarn = new()
                {
                    Type = EWarnType.ecCompWarn,
                    WarningNum = WarningNum,
                    Line = lngErrLine,
                    Module = strModule,
                    Text = WarningText
                };
                Raise_CompileLogicEvent(bytLogComp, tmpWarn);
            }
        }
        static internal bool CompileIf()
        {
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
            byte[] bytArg = new byte[8]; //(7)
            int lngArg;
            int[] lngWord;
            int intWordCount;
            int i;
            bool blnIfBlock = false; //command block, not a comment block
            bool blnNeedNextCmd = true;
            int intNumTestCmds = 0;
            int intNumCmdsInBlock = 0;
            bool blnNOT;
            bool endfound = false;

            //write out starting if byte
            tmpLogRes.WriteByte(0xFF);

            //next character should be "("
            if (NextChar() != "(") {
                //blnError = true;
                strErrMsg = LoadResString(4002);
                return false;
            }

            //now, step through input, until final ')' is found:
            do {
                //get next command
                strTestCmd = NextCommand();
                //check for end of input,
                if (lngLine == -1) {
                    //blnError = true;
                    strErrMsg = LoadResString(4106);
                    return false;
                }

                //if awaiting a test command,
                if (blnNeedNextCmd) {
                    switch (strTestCmd) {
                    case "(": //open paran
                              //if already in a block
                        if (blnIfBlock) {
                            //blnError = true;
                            strErrMsg = LoadResString(4045);
                            return false;
                        }
                        //write /'or' block start
                        tmpLogRes.WriteByte(0xFC);
                        blnIfBlock = true;
                        intNumCmdsInBlock = 0;
                        break;
                    case ")":
                        //if a test command is expected, ')' always causes error
                        if (intNumTestCmds == 0) {
                            strErrMsg = LoadResString(4057);
                        }
                        else if (blnIfBlock && intNumCmdsInBlock == 0) {
                            strErrMsg = LoadResString(4044);
                        }
                        else {
                            strErrMsg = LoadResString(4056);
                        }
                        //blnError = true;
                        return false;
                    default:
                        //check for NOT
                        blnNOT = (strTestCmd == NOT_TOKEN);
                        if (blnNOT) {
                            tmpLogRes.WriteByte(0xFD);
                            //read in next test command
                            strTestCmd = NextCommand();
                            //check for end of input,
                            if (lngLine == -1) {
                                //blnError = true;
                                strErrMsg = LoadResString(4106);
                                return false;
                            }
                        }
                        bytTestCmd = CommandNum(true, strTestCmd);
                        //if command not found,
                        if (bytTestCmd == 255) {
                            //check for special syntax
                            if (!CompileSpecialif(strTestCmd, blnNOT)) {
                                //error; the CompileSpecialIf function
                                //sets the error codes, and CompileLogic will
                                //call the error handler
                                return false;
                            }
                        }
                        else {
                            //write the test command code
                            tmpLogRes.WriteByte(bytTestCmd);
                            //next command should be "("
                            if (NextChar() != "(") {
                                //blnError = true;
                                strErrMsg = LoadResString(4048);
                                return false;
                            }
                            //check for return.false() command
                            if (bytTestCmd == 0) {
                                //warn user that it's not compatible with AGI Studio
                                if (ErrorLevel == leHigh ||
                                    ErrorLevel == leMedium) {
                                    //generate warning
                                    AddWarning(5081);
                                    //} else {// leLow
                                    //  // no action
                                }
                            }

                            //if said command
                            if (bytTestCmd == 0xE) {
                                intWordCount = 0;
                                lngWord = Array.Empty<int>();
                                //get first word arg
                                lngArg = GetNextArg(atVocWrd, intWordCount + 1);
                                //if error
                                if (blnCriticalError) {
                                    // if error number is 4054
                                    if (Left(strErrMsg, 4) == "4054") {
                                        // add command name to error string
                                        strErrMsg = strErrMsg.Replace(ARG2, TestCommands[bytTestCmd].Name);
                                    }
                                    //exit
                                    return false;
                                }

                                //loop to add this word, and any more
                                do {
                                    //add this word number to array of word numbers
                                    Array.Resize(ref lngWord, intWordCount + 1);
                                    lngWord[intWordCount] = lngArg;
                                    intWordCount++;
                                    //if too many words
                                    if (intWordCount == 10) {
                                        //blnError = true;
                                        strErrMsg = LoadResString(4093);
                                        return false;
                                    }

                                    //get next character
                                    //(should be a comma, or close parenthesis, if no more words)
                                    strArg = NextChar();
                                    if (strArg.Length != 0) {
                                        switch (strArg[0]) {
                                        case ')':
                                            //move pointer back one space so
                                            //the ')' will be found at end of command
                                            lngPos--;
                                            endfound = true;
                                            break; //exit do
                                        case ',':
                                            //expected; now check for next word argument
                                            lngArg = GetNextArg(atVocWrd, intWordCount + 1);
                                            //if error
                                            if (blnCriticalError) {
                                                //exit
                                                // if error number is 4054
                                                if (Left(strErrMsg, 4) == "4054") {
                                                    // add command name to error string
                                                    strErrMsg = strErrMsg.Replace(ARG2, TestCommands[bytTestCmd].Name);
                                                }
                                                return false;
                                            }
                                            break;
                                        default:
                                            //anything else is an error
                                            //blnError = true;
                                            //check for added quotes; they are the problem
                                            if (lngQuoteAdded >= 0) {
                                                //reset line;
                                                lngLine = lngQuoteAdded;
                                                lngErrLine = lngLine - lngIncludeOffset;
                                                //string error
                                                strErrMsg = LoadResString(4051);
                                            }
                                            else {
                                                //use 1-base arg values
                                                strErrMsg = LoadResString(4047).Replace(ARG1, (intWordCount + 1).ToString());
                                            }
                                            return false;
                                        }
                                        if (endfound) {
                                            break; // exit the loop
                                        }

                                    }
                                    else {
                                        //we should normally never get here, since changing the function to allow
                                        //splitting over multiple lines, unless this is the LAST line of
                                        //the logic (an EXTREMELY rare edge case)
                                        //error
                                        //blnError = true;
                                        //check for added quotes; they are the problem
                                        if (lngQuoteAdded >= 0) {
                                            //reset line;
                                            lngLine = lngQuoteAdded;
                                            lngErrLine = lngLine - lngIncludeOffset;
                                            //string error
                                            strErrMsg = LoadResString(4051);
                                        }
                                        else {
                                            //use 1-base arg values
                                            strErrMsg = LoadResString(4047).Replace(ARG1, (intWordCount + 1).ToString());
                                        }
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
                                        if (NextChar(true) != ",") {
                                            //blnError = true;
                                            //use 1-base arg values
                                            strErrMsg = LoadResString(4047).Replace(ARG1, (i + 1).ToString());
                                            return false;
                                        }
                                    }

                                    //reset the quotemark error flag after comma is found
                                    lngQuoteAdded = -1;
                                    bytArg[i] = (byte)GetNextArg(TestCommands[bytTestCmd].ArgType[i], i);
                                    //if error
                                    if (blnCriticalError) {
                                        // if error number is 4054
                                        if (Left(strErrMsg, 4) == "4054") {
                                            // add command name to error string
                                            strErrMsg = strErrMsg.Replace(ARG2, TestCommands[bytTestCmd].Name);
                                        }
                                        return false;
                                    }
                                    //write argument
                                    tmpLogRes.WriteByte(bytArg[i]);
                                }
                            }
                            //next character should be ")"
                            if (NextChar() != ")") {
                                //blnError = true;
                                strErrMsg = LoadResString(4160);
                                return false;
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
                        if (blnIfBlock) {
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
                        //blnError = true;
                        strErrMsg = LoadResString(4097);
                        return false;
                    case AND_TOKEN:
                        //if inside brackets
                        if (blnIfBlock) {
                            //blnError = true;
                            strErrMsg = LoadResString(4037);
                            return false;
                        }
                        blnNeedNextCmd = true;
                        break;
                    case OR_TOKEN:
                        //if NOT inside brackets
                        if (!blnIfBlock) {
                            //blnError = true;
                            strErrMsg = LoadResString(4100);
                            return false;
                        }
                        blnNeedNextCmd = true;
                        break;
                    case ")":
                        //if inside brackets
                        if (blnIfBlock) {
                            //ensure at least one command in block,
                            if (intNumCmdsInBlock == 0) {
                                //blnError = true;
                                strErrMsg = LoadResString(4044);
                                return false;
                            }
                            //close brackets
                            blnIfBlock = false;
                            tmpLogRes.WriteByte(0xFC);
                        }
                        else {
                            //ensure at least one command in block,
                            if (intNumTestCmds == 0) {
                                //blnError = true;
                                strErrMsg = LoadResString(4044);
                                return false;
                            }
                            //end of if found

                            //write ending if byte
                            tmpLogRes.WriteByte(0xFF);
                            //return true
                            return true;
                        }
                        break;
                    default:
                        if (blnIfBlock) {
                            strErrMsg = LoadResString(4101);
                        }
                        else {
                            strErrMsg = LoadResString(4038);
                        }
                        //blnError = true;
                        return false;
                    }
                }
                //never leave loop normally; error, end of input, or successful
                //compilation of test commands will all exit loop directly
            } while (true);
        } //endfunction
        static internal bool ValidateArgs(int CmdNum, ref byte[] ArgVal)
        {
            //check for specific command issues
            //for commands that can affect variable values, need to check against reserved variables
            //for commands that can affect flags, need to check against reserved flags
            //for other commands, check the passed arguments to see if values are appropriate
            bool blnUnload = false, blnWarned = false;

            // check each command
            if (CmdNum == 1 || CmdNum == 2 ||
               (CmdNum >= 4 && CmdNum <= 8) ||
                CmdNum == 10 ||
               (CmdNum >= 165 && CmdNum <= 168)) {
                //increment, decrement, assignv, addn, addv, subn, subv
                //rindirect, mul.n, mul.v, div.n, div.v
                //check for reserved variables that should never be manipulated
                //(assume arg Value is zero to avoid tripping other checks)
                CheckResVarUse(ArgVal[0], 0);
                //for div.n(vA, B) only, check for divide-by-zero
                if (CmdNum == 167) {
                    if (ArgVal[1] == 0) {
                        if (ErrorLevel == leHigh) {
                            strErrMsg = LoadResString(4149);
                            return false;
                        }
                        else if (ErrorLevel == leMedium) {
                            AddWarning(5030);
                            //} else  if (ErrorLevel == leLow) {
                            //  no action
                        }
                    }
                }
            }
            else if (CmdNum == 3) {
                //assignn
                //check for actual Value being assigned
                CheckResVarUse(ArgVal[0], ArgVal[1]);
            }
            else if (CmdNum >= 12 && CmdNum <= 14) {
                //set, reset, toggle
                //check for reserved flags
                CheckResFlagUse(ArgVal[0]);
            }
            else if (CmdNum == 18) {
                //new.room(A)
                //validate that this logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4120);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5053);
                        //} else if (ErrorLevel == leLow) {
                        // no action
                    }
                }
                //expect no more commands
                blnNewRoom = true;
            }
            else if (CmdNum == 19) {
                //new.room.v
                //expect no more commands
                blnNewRoom = true;
            }
            else if (CmdNum == 20) {
                //load.logics(A)
                //validate that logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4121).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5013);
                        //} else if (ErrorLevel == leLow) {
                        //// no action
                    }
                }
            }
            else if (CmdNum == 22) {
                //call(A)
                //calling logic0 is a BAD idea
                if (ArgVal[0] == 0) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4118);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5010);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //recursive calling is BAD
                if (ArgVal[0] == bytLogComp) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4117);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5089);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //validate that logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4156).Replace(ARG1, (ArgVal[0]).ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5076);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 30) {
                //load.view(A)
                //validate that view exists
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4122).Replace(ARG1, (ArgVal[0]).ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5015);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 32) {
                //discard.view(A)
                //validate that view exists
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4123).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5024);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 37) {
                //position(oA, X,Y)
                //check x/y against limits
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4128);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5023);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 39) { //get.posn
                                     //neither variable arg should be a reserved Value
                if (ArgVal[1] <= 26 || ArgVal[2] <= 26) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 41) { //set.view(oA, B)
                                     //validate that view exists
                if (!compGame.agViews.Exists(ArgVal[1])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4124).Replace(ARG1, (ArgVal[1]).ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5037);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if ((CmdNum >= 49 && CmdNum <= 53) ||
                      CmdNum == 97 || CmdNum == 118) {
                //last.cel, current.cel, current.loop,
                //current.view, number.of.loops, get.room.v
                //get.num
                //variable arg is second
                //variable arg should not be a reserved Value
                if (ArgVal[1] <= 26) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 54) {
                //set.priority(oA, B)
                //check priority Value
                if (ArgVal[1] > 15) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4125);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5050);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 57) {
                //get.priority
                //variable is second argument
                //variable arg should not be a reserved Value
                if (ArgVal[1] <= 26) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 63) {
                //set.horizon(A)
                //>120 or <16 is unusual
                //>=167 will cause AGI to freeze/crash

                //validate horizon Value
                if (ErrorLevel == leHigh) {
                    if (ArgVal[0] >= 167) {
                        strErrMsg = LoadResString(4126);
                        return false;
                    }
                    if (ArgVal[0] > 120) {
                        AddWarning(5042);
                    }
                    else if (ArgVal[0] < 16) {
                        AddWarning(5041);
                    }
                }
                else if (ErrorLevel == leMedium) {
                    if (ArgVal[0] >= 167) {
                        AddWarning(5043);
                    }
                    else if (ArgVal[0] > 120) {
                        AddWarning(5042);
                    }
                    else if (ArgVal[0] < 16) {
                        AddWarning(5041);
                    }
                    //} else if (ErrorLevel == leLow) {
                    //  //no action
                }
            }
            else if (CmdNum >= 64 && CmdNum <= 66) {
                //object.on.water, object.on.land, object.on.anything
                //warn if used on ego
                if (ArgVal[0] == 0) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5082);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 69) {
                //distance
                //variable is third arg
                //variable arg should not be a reserved Value
                if (ArgVal[2] <= 26) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 73 || CmdNum == 75 || CmdNum == 99) {
                //end.of.loop, reverse.loop
                //flag arg should not be a reserved Value
                if (ArgVal[1] <= 15) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for read only reserved flags
                CheckResFlagUse(ArgVal[1]);
            }
            else if (CmdNum == 81) {
                //move.obj(oA, X,Y,STEP,fDONE)
                //validate the target position
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4127);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5062);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for ego object
                if (ArgVal[0] == 0) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5045);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //flag arg should not be a reserved Value
                if (ArgVal[4] <= 15) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for read only reserved flags
                CheckResFlagUse(ArgVal[4]);
            }
            else if (CmdNum == 82) { //move.obj.v
                                     //flag arg should not be a reserved Value
                if (ArgVal[4] <= 15) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for read only reserved flags
                CheckResFlagUse(ArgVal[4]);
            }
            else if (CmdNum == 83) {
                //follow.ego(oA, DISTANCE, fDONE)
                //validate distance value
                if (ArgVal[1] <= 1) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5102);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for ego object
                if (ArgVal[0] == 0) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5027);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //flag arg should not be a reserved Value
                if (ArgVal[2] <= 15) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for read only reserved flags
                CheckResFlagUse(ArgVal[2]);
            }
            else if (CmdNum == 86) { //set.dir(oA, vB)
                                     //check for ego object
                if (ArgVal[0] == 0) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5026);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 87) {
                //get.dir
                //variable is second arg
                //variable arg should not be a reserved Value
                if (ArgVal[1] <= 26) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 90) {
                //block(x1,y1,x2,y2)
                //validate that all are within bounds, and that x1<=x2 and y1<=y2
                //also check that
                if (ArgVal[0] > 159 || ArgVal[1] > 167 || ArgVal[2] > 159 || ArgVal[3] > 167) {
                    //bad number
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4129);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5020);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                if ((ArgVal[2] - ArgVal[0] < 2) || (ArgVal[3] - ArgVal[1] < 2)) {
                    //won't work
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4129);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5051);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 98) {
                //load.sound(A)
                //validate the sound exists
                if (!compGame.agSnds.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4130).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5014);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 99) {
                //sound(A)
                //validate the sound exists
                if (!compGame.agSnds.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4137).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5084);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 103) {
                //display(ROW,COL,mC)
                //check row/col against limits
                if (ArgVal[0] > 24 || ArgVal[1] > 39) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4131);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5059);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 105) {
                //clear.lines(TOP,BTM,C)
                //top must be >btm; both must be <=24
                if (ArgVal[0] > 24 || ArgVal[1] > 24 || ArgVal[0] > ArgVal[1]) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4132);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5011);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //color value should be 0 or 15 //(but it doesn't hurt to be anything else)
                if (ArgVal[2] > 0 && ArgVal[2] != 15) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5100);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 109) {
                //set.text.attribute(A,B)
                //should be limited to valid color values (0-15)
                if (ArgVal[0] > 15 || ArgVal[1] > 15) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4133);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5029);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 110) {
                //shake.screen(A)
                //shouldn't normally have more than a few shakes; zero is BAD
                if (ArgVal[0] == 0) {
                    //error!
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        strErrMsg = LoadResString(4134);
                        return false;
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                else if (ArgVal[0] > 15) {
                    //could be a palette change?
                    if (ArgVal[0] >= 100 && ArgVal[0] <= 109) {
                        //separate warning
                        if (ErrorLevel == leHigh ||
                            ErrorLevel == leMedium) {
                            AddWarning(5058);
                            //} else if (ErrorLevel == leLow) {
                            //  //no action
                        }
                    }
                    else {
                        //warning
                        if (ErrorLevel == leHigh ||
                            ErrorLevel == leMedium) {
                            AddWarning(5057);
                            //} else if (ErrorLevel == leLow) {
                            //  //no action
                        }
                    }
                }
            }
            else if (CmdNum == 111) { //configure.screen(TOP,INPUT,STATUS)
                                      //top should be <=3
                                      //input and status should not be equal
                                      //input and status should be <top or >=top+21
                if (ArgVal[0] > 3) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4135);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5044);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                if (ArgVal[1] > 24 || ArgVal[2] > 24) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5099);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                if (ArgVal[1] == ArgVal[2]) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5048);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                if ((ArgVal[1] >= ArgVal[0] && ArgVal[1] <= ArgVal[0] + 20) || (ArgVal[2] >= ArgVal[0] && ArgVal[2] <= ArgVal[0] + 20)) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5049);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 114) { //set.string(sA, mB)
                                      //warn user if setting input prompt to unusually long value
                if (ArgVal[0] == 0) {
                    if (strMsg[ArgVal[1]].Length > 10) {
                        if (ErrorLevel == leHigh ||
                            ErrorLevel == leMedium) {
                            AddWarning(5096);
                            //} else if (ErrorLevel == leLow) {
                            //  //no action
                        }
                    }
                }
            }
            else if (CmdNum == 115) {
                //get.string(sA, mB, ROW,COL,LEN)
                //if row>24, both row/col are ignored; if col>39, gets weird; len is limited automatically to <=40
                if (ArgVal[2] > 24) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5052);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                if (ArgVal[3] > 39) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4004);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5080);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                if (ArgVal[4] > 40) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5056);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 121) {
                //set.key(A,B,cC)
                //controller number limit checked in GetNextArg function
                //increment controller Count
                intCtlCount++;
                //must be ascii or key code, (Arg0 can be 1 to mean joystick)
                if (ArgVal[0] > 0 && ArgVal[1] > 0 && ArgVal[0] != 1) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4154);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5065);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for improper ASCII assignments
                if (ArgVal[1] == 0) {
                    if (ArgVal[0] == 8 || ArgVal[0] == 13 || ArgVal[0] == 32) {
                        //ascii codes for bkspace, enter, spacebar
                        //bad
                        if (ErrorLevel == leHigh) {
                            strErrMsg = LoadResString(4155);
                            return false;
                        }
                        else if (ErrorLevel == leMedium) {
                            AddWarning(5066);
                            //} else if (ErrorLevel == leLow) {
                            //  //no action
                        }
                    }
                }
                //check for improper KEYCODE assignments
                if (ArgVal[0] == 0) {
                    if ((ArgVal[1] >= 71 && ArgVal[1] <= 73) ||
                        (ArgVal[1] >= 75 && ArgVal[1] <= 77) ||
                        (ArgVal[1] >= 79 && ArgVal[1] <= 83)) {
                        //ascii codes - bad
                        if (ErrorLevel == leHigh) {
                            strErrMsg = LoadResString(4155);
                            return false;
                        }
                        else if (ErrorLevel == leMedium) {
                            AddWarning(5066);
                            //} else if (ErrorLevel == leLow) {
                            //  //no action
                        }
                    }
                }
            }
            else if (CmdNum == 122) {
                //add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
                //VIEW, LOOP + CEL must exist
                //CEL width must be >=3
                //x,y must be within limits
                //PRI must be 0, or >=3 AND <=15
                //MGN must be 0-3, or >3 (ha ha, or ANY value...)
                //validate view
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4138).Replace(ARG1, (ArgVal[0]).ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5064);
                        //dont need to check loops or cels
                        blnWarned = true;
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
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
                        }
                        //validate loop
                        if (ArgVal[1] >= compGame.agViews[ArgVal[0]].Loops.Count) {
                            if (ErrorLevel == leHigh) {
                                strErrMsg = LoadResString(4139).Replace(ARG1, ArgVal[1].ToString()).Replace(ARG2, ArgVal[0].ToString());
                                if (blnUnload) {
                                    compGame.agViews[ArgVal[0]].Unload();
                                }
                                return false;
                            }
                            else if (ErrorLevel == leMedium) {
                                AddWarning(5085);
                                //dont need to check cel
                                blnWarned = true;
                                //} else if (ErrorLevel == leLow) {
                                //  //no action
                            }
                        }
                        //if loop was valid, check cel
                        if (!blnWarned) {
                            //validate cel
                            if (ArgVal[2] >= compGame.agViews[ArgVal[0]].Loops[ArgVal[1]].Cels.Count) {
                                if (ErrorLevel == leHigh) {
                                    strErrMsg = LoadResString(4140).Replace(ARG1, ArgVal[2].ToString()).Replace(ARG2, ArgVal[1].ToString()).Replace(ARG3, ArgVal[0].ToString());
                                    if (blnUnload) {
                                        compGame.agViews[ArgVal[0]].Unload();
                                    }
                                    return false;
                                }
                                else if (ErrorLevel == leMedium) {
                                    AddWarning(5086);
                                    //} else if (ErrorLevel == leLow) {
                                    //  //no action
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
                //x,y must be within limits
                if (ArgVal[3] > 159 || ArgVal[4] > 167) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4141);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5038);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //PRI should be <=15
                if (ArgVal[5] > 15) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4142);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5079);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //PRI should be 0 OR >=4 (but doesn't raise an error; only a warning)
                if (ArgVal[5] < 4 && ArgVal[5] != 0) {
                    if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                        AddWarning(5079);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //MGN values >15 will only use lower nibble
                if (ArgVal[6] > 15) {
                    if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                        AddWarning(5101);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 129) {
                //show.obj(VIEW)
                //validate view
                if (!compGame.agViews.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4144).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5061);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 127 || CmdNum == 176 || CmdNum == 178) {
                //init.disk, hide.mouse, show.mouse
                if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                    AddWarning(5087, LoadResString(5087).Replace(ARG1, ActionCommands[CmdNum].Name));
                    //} else if (ErrorLevel == leLow) {
                    //  //no action
                }
            }
            else if (CmdNum == 175 || CmdNum == 179 || CmdNum == 180) {
                //discard.sound, fence.mouse, mouse.posn
                if (ErrorLevel == leHigh) {
                    strErrMsg = LoadResString(4152).Replace(ARG1, ActionCommands[CmdNum].Name);
                    return false;
                }
                else if (ErrorLevel == leMedium) {
                    AddWarning(5088, LoadResString(5088).Replace(ARG1, ActionCommands[CmdNum].Name));
                    //} else if (ErrorLevel == leLow) {
                    //  //no action
                }
            }
            else if (CmdNum == 130) { //random(LOWER,UPPER,vRESULT)
                                      //lower should be < upper
                if (ArgVal[0] > ArgVal[1]) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4145);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5054);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //lower=upper means result=lower=upper
                if (ArgVal[0] == ArgVal[1]) {
                    if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                        AddWarning(5106);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //if lower=upper+1, means div by 0!
                if (ArgVal[0] == ArgVal[1] + 1) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4158);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5107);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //variable arg should not be a reserved Value
                if (ArgVal[2] <= 26) {
                    if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 142) { //script.size
                                      //raise warning/error if in other than logic0
                if (bytLogComp != 0) {
                    //warn
                    if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                        AddWarning(5039);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //check for absurdly low Value for script size
                if (ArgVal[0] < 10) {
                    if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                        AddWarning(5009);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 147) { //reposition.to(oA, B,C)
                                      //validate the new position
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4128);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5023);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 150) {
                //trace.info(LOGIC,ROW,HEIGHT)
                //logic must exist
                //row + height must be <22
                //height must be >=2 (but interpreter checks for this error)

                //validate that logic exists
                if (!compGame.agLogs.Exists(ArgVal[0])) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4153).Replace(ARG1, ArgVal[0].ToString());
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5040);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //validate that height is not too small
                if (ArgVal[2] < 2) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5046);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                //validate size of window
                if (ArgVal[1] + ArgVal[2] > 23) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4146);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5063);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 151 || CmdNum == 152) {
                //Print.at(mA, ROW, COL, MAXWIDTH), print.at.v
                //row <=22
                //col >=2
                //maxwidth <=36
                //maxwidth=0 defaults to 30
                //maxwidth=1 crashes AGI
                //col + maxwidth <=39
                if (ArgVal[1] > 22) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4147);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5067);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
                switch (ArgVal[3]) {
                case 0: //maxwidth=0 defaults to 30
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5105);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                    break;
                case 1: //maxwidth=1 crashes AGI
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4043);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5103);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                    break;
                default:
                    if (ArgVal[3] > 36) { //maxwidth >36 won't work
                        if (ErrorLevel == leHigh) {
                            strErrMsg = LoadResString(4043);
                            return false;
                        }
                        else if (ErrorLevel == leMedium) {
                            AddWarning(5104);
                            //} else if (ErrorLevel == leLow) {
                            //  //no action
                        }
                    }
                    break;
                }
                //col>2 and col + maxwidth <=39
                if (ArgVal[2] < 2 || ArgVal[2] + ArgVal[3] > 39) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4148);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5068);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 154) {
                //clear.text.rect(R1,C1,R2,C2,COLOR)
                //if (either row argument is >24,
                //or either column argument is >39,
                //or R2 < R1 or C2 < C1,
                //the results are unpredictable
                if (ArgVal[0] > 24 || ArgVal[1] > 39 ||
                   ArgVal[2] > 24 || ArgVal[3] > 39 ||
                   ArgVal[2] < ArgVal[0] || ArgVal[3] < ArgVal[1]) {
                    //invalid items
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4150);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        //if due to pos2 < pos1
                        if (ArgVal[2] < ArgVal[0] || ArgVal[3] < ArgVal[1]) {
                            AddWarning(5069);
                        }
                        //if due to variables outside limits
                        if (ArgVal[0] > 24 || ArgVal[1] > 39 ||
                           ArgVal[2] > 24 || ArgVal[3] > 39) {
                            AddWarning(5070);
                        }
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }

                //color value should be 0 or 15 //(but it doesn't hurt to be anything else)
                if (ArgVal[4] > 0 && ArgVal[4] != 15) {
                    if (ErrorLevel == leHigh ||
                     ErrorLevel == leMedium) {
                        AddWarning(5100);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 158) {
                //submit.menu()
                //should only be called in logic0
                //raise warning if in other than logic0
                if (bytLogComp != 0) {
                    if (ErrorLevel == leHigh ||
                     ErrorLevel == leMedium) {
                        //set warning
                        AddWarning(5047);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            else if (CmdNum == 174) { //set.pri.base(A)
                                      //calling set.pri.base with Value >167 doesn't make sense
                if (ArgVal[0] > 167) {
                    if (ErrorLevel == leHigh ||
                        ErrorLevel == leMedium) {
                        AddWarning(5071);
                        //} else if (ErrorLevel == leLow) {
                        //  //no action
                    }
                }
            }
            //success
            return true;
        }
        static internal int LabelNum(string LabelName)
        {
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
        static internal bool ValidateIfArgs(int CmdNum, ref byte[] ArgVal)
        {
            //check for specific command issues
            switch (CmdNum) {
            case 9: //has (iA)
            case 10: //obj.in.room(iA, vB)
                     //invobj number validated in GetNextArg function
                break;
            case 11: //posn(oA, X1, Y1, X2, Y2)
            case 16: //obj.in.box(oA, X1, Y1, X2, Y2)
            case 17: //center.posn(oA, X1, Y1, X2, Y2)
            case 18: //right.posn(oA, X1, Y1, X2, Y2)
                     //screenobj number validated in GetNextArg function

                //validate that all are within bounds, and that x1<=x2 and y1<=y2
                if (ArgVal[1] > 159 || ArgVal[2] > 167 || ArgVal[3] > 159 || ArgVal[4] > 167) {
                    //bad number
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4151);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5072);
                        //} else if (ErrorLevel == leLow) {
                        //  no action 
                    }
                }
                if ((ArgVal[1] > ArgVal[3]) || (ArgVal[2] > ArgVal[4])) {
                    //won't work
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4151);
                        return false;
                    }
                    else if (ErrorLevel == leMedium) {
                        AddWarning(5073);
                        //} else if (ErrorLevel == leLow) {
                        //  no action 
                    }
                }
                break;
            case 12: //controller (cA)
                     //has controller been assigned?
                     //not sure how to check it; calls to controller cmd may
                     //occur in logics that are compiled before the logic that sets
                     //them up...
                break;
            case 14: //said()
                     // nothing to check
                break;
            case 15: //compare.strings(sA, sB)
                     // nothing to check
                break;
            }
            //success
            return true;
        }
        static internal bool ValidateMsgChars(string strMsg, int MsgNum)
        {
            //raise error/warning, depending on setting
            //return TRUE if OK or only a warning;  FALSE means error found
            string BADCHARS = ((char)8).ToString() + "\t\n\r";
            string EXTCHARS = "";
            for (int i = 128; i < 256; i++) {
                EXTCHARS += ((char)i).ToString();
            }
            //if LOW error detection, EXIT
            if (ErrorLevel == leLow) {
                return true;
            }
            //check for invalid codes (8,9,10,13)
            if ((BADCHARS).Any(strMsg.Contains)) {
                //warn user
                if (ErrorLevel == leHigh) {
                    strErrMsg = LoadResString(4005);
                    //blnError = true;
                    return false;
                }
                else { // leMedium
                    AddWarning(5093);
                    //need to track warning in case this msg is
                    //also included in body of logic
                    intMsgWarn[MsgNum] |= 1;
                }

                //extended characters
                if ((EXTCHARS).Any(strMsg.Contains)) {
                    if (ErrorLevel == leHigh) {
                        strErrMsg = LoadResString(4006);
                        //blnError = true;
                        return false;
                    }
                    else { // leMedium
                        AddWarning(5094);
                        //need to track warning in case this msg is
                        //also included in body of logic
                        intMsgWarn[MsgNum] |= 2;
                    }
                }
            }
            //msg is OK
            return true;
        }
        static internal bool WriteMsgs()
        {
            //this function will write the messages for a logic at the end of
            //the resource.
            //messages are encrypted with the string 'Avis Durgan'. No gaps
            //are allowed, so messages that are skipped must be included as
            //zero length messages
            int lngMsgSecStart;
            int lngMsgSecLen;
            int[] lngMsgPos = new int[256];
            int intCharPos;
            byte bytCharVal;
            int lngMsg;
            int lngMsgCount;
            int lngCryptStart;
            int lngMsgLen;
            int i;
            string strHex;
            bool blnSkipNull, blnSkipChar = false;
            //calculate start of message section
            lngMsgSecStart = tmpLogRes.Size;
            //find last message by counting backwards until a msg is found
            lngMsgCount = 256;
            do {
                lngMsgCount--;
            } while (!blnMsg[lngMsgCount] && (lngMsgCount != 0));//  Until blnMsg(lngMsgCount) || (lngMsgCount = 0)

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
                //always reset the 'NoNull' feature
                blnSkipNull = false;
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
                    //step through all characters in this msg
                    intCharPos = 0;
                    while (intCharPos < strMsg[lngMsg].Length) // Until intCharPos > Len(strMsg(lngMsg))
                     {                                          //get ascii code for this character
                        bytCharVal = (byte)strMsg[lngMsg][intCharPos];
                        //check for invalid codes (8,9,10,13)
                        switch (bytCharVal) {
                        case 8:
                        case 9:
                        case 10:
                        case 13:
                            //convert these chars to space to avoid trouble
                            bytCharVal = 32;
                            break;
                        case 92: // '\'
                                 //check next character for special codes
                            if (intCharPos < lngMsgLen - 1) {
                                switch ((byte)strMsg[lngMsg][intCharPos + 1]) {
                                case 110:
                                case 78:
                                    //  '\n' = new line
                                    bytCharVal = 0xA;
                                    intCharPos++;
                                    break;
                                case 34: //dbl quote(")
                                         // '\"' = quote mark (chr$(34))
                                    bytCharVal = 0x22;
                                    intCharPos++;
                                    break;
                                case 92:
                                    // '\\' = \
                                    bytCharVal = 0x5C;
                                    intCharPos++;
                                    break;
                                case 48:
                                    //\0 = don't add null terminator
                                    blnSkipNull = true;
                                    //also skip this char
                                    blnSkipChar = true;
                                    intCharPos++;
                                    break;
                                case 120: // '\x'  //look for a hex value
                                          //make sure at least two more characters
                                    if (intCharPos < lngMsgLen - 3) {
                                        //get next 2 chars and hexify them
                                        strHex = "0x" + Mid(strMsg[lngMsg], intCharPos + 2, 2);
                                        //if this hex value >=1 and <256, use it
                                        try {
                                            i = Convert.ToInt32(strHex, 16);
                                            if (i >= 1 && i < 256) {
                                                bytCharVal = (byte)i;
                                                intCharPos += 3;
                                            }
                                        }
                                        catch (Exception) { // drop the slash if there's an error
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
                    } //loop
                }

                //if msg was used, add trailing zero to terminate message
                //(if msg was zero length, we still need this terminator)
                if (blnMsg[lngMsg]) {
                    if (!blnSkipNull) {
                        tmpLogRes.WriteByte((byte)(0 ^ bytEncryptKey[(tmpLogRes.Pos - lngCryptStart) % 11]));
                    }
                }
            }
            //calculate length of msgs, and write it at beginning
            //of msg section (adjust by one byte, which is the
            //byte that indicates number of msgs written)
            lngMsgSecLen = tmpLogRes.Pos - (lngMsgSecStart + 1);
            tmpLogRes.WriteWord((ushort)(tmpLogRes.Pos - (lngMsgSecStart + 1)), lngMsgSecStart + 1);

            //write msg section start Value at beginning of resource
            //(correct by two so it gives position relative to byte 7 of
            //the logic resource header - see procedure //DecodeLogic// for details)
            tmpLogRes.WriteWord((ushort)(lngMsgSecStart - 2), 0);

            //write all the msg pointers (start with msg1 since msg0
            // is never allowed)
            for (lngMsg = 1; i <= lngMsgCount; i++) {
                tmpLogRes.WriteWord((ushort)lngMsgPos[lngMsg], lngMsgSecStart + 1 + lngMsg * 2);
            }
            //success
            return true;
        }
        static internal bool ReadLabels()
        {
            byte i;
            string strLabel;
            int rtn;
            //this function steps through the source code to identify all valid labels; we need to find
            //them all before starting the compile so we can correctly set jumps
            //
            //valid syntax is either 'label:' or ':label', with nothing else in front of or after
            //the label declaration

            //reset counter
            bytLabelCount = 0;
            //reset compiler to first input line
            ResetCompiler();
            do {
                //look for label name by searching for colon character
                if (strCurrentLine.Contains(":", StringComparison.CurrentCulture)) {
                    // first replace any tabs ? why? don't do it anywhere eles?
                    strLabel = strCurrentLine.Replace("\t", " ").Trim();
                    //check for 'label:'
                    if (strLabel[strLabel.Length - 1] == ':') {
                        strLabel = Left(strLabel, strLabel.Length - 1).Trim();
                        // check for ':label'
                    }
                    else if (strLabel[0] == ':') {
                        strLabel = Right(strLabel, strLabel.Length - 1).Trim();
                    }
                    else {
                        //not a label
                        strLabel = "";
                    }
                    //if a label was found, validate it
                    if (strLabel.Length != 0) {
                        //make sure enough room
                        if (bytLabelCount >= MAX_LABELS) {
                            strErrMsg = LoadResString(4109).Replace(ARG1, MAX_LABELS.ToString());
                            return false;
                        }
                        DefineNameCheck chkLabel = compGame.agGlobals.ValidateDefName(strLabel);
                        //numbers are ok for labels
                        if (chkLabel == DefineNameCheck.ncNumeric) {
                            chkLabel = DefineNameCheck.ncOK;
                        }
                        // check for error
                        if (chkLabel != DefineNameCheck.ncOK) {
                            switch (chkLabel) {
                            case DefineNameCheck.ncEmpty:
                                strErrMsg = LoadResString(4096);
                                break;
                            case DefineNameCheck.ncActionCommand:
                                strErrMsg = LoadResString(4025).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncTestCommand:
                                strErrMsg = LoadResString(4026).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncKeyWord:
                                strErrMsg = LoadResString(4028).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncArgMarker:
                                strErrMsg = LoadResString(4091);
                                break;
                            case DefineNameCheck.ncGlobal:
                                strErrMsg = LoadResString(4024).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncReservedVar:
                                strErrMsg = LoadResString(4033).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncReservedFlag:
                                strErrMsg = LoadResString(4030).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncReservedNum:
                                strErrMsg = LoadResString(4029).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncReservedObj or DefineNameCheck.ncReservedStr:
                                strErrMsg = LoadResString(4032).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncReservedMsg:
                                strErrMsg = LoadResString(4031).Replace(ARG1, strLabel);
                                break;
                            case DefineNameCheck.ncBadChar:
                                strErrMsg = LoadResString(4068);
                                break;
                            }
                            return false;
                        }
                        //no periods allowed either
                        if (strLabel.IndexOf(".") >= 0) {
                            strErrMsg = LoadResString(4068);
                            return false;
                        }
                        //check label against current list of labels
                        for (i = 0; i < bytLabelCount; i++) {
                            if (strLabel.Equals(llLabel[i].Name, StringComparison.OrdinalIgnoreCase)) {
                                strErrMsg = LoadResString(4027).Replace(ARG1, strLabel);
                                return false;
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
            } while (lngLine != -1); // Until lngLine = -1
            return true;
        }
        static internal bool CompileAGI()
        {
            //main compiler function
            //steps through input one command at a time and converts it
            //to AGI logic code
            //Note that we don't need to set blnError flag here;
            //an error will cause this function to return a Value of false
            //which causes the compiler to display error info

            const int MaxGotos = 255;
            string strNextCmd;
            string strPrevCmd = "";
            string strArg;
            byte[] bytArg = new byte[8];
            int i;
            int intCmdNum;
            int[] BlockStartDataLoc = new int[MAX_BLOCK_DEPTH + 1];
            int BlockDepth;
            bool[] BlockIsif = new bool[MAX_BLOCK_DEPTH + 1];
            int[] BlockLength = new int[MAX_BLOCK_DEPTH + 1];
            int intLabelNum;
            LogicGoto[] Gotos = new LogicGoto[MaxGotos + 1];
            int NumGotos;
            int GotoData;
            int CurGoto;
            bool blnLastCmdRtn = false;
            int lngReturnLine = 0;

            //initialize variables
            BlockDepth = 0;
            NumGotos = 0;
            //blnError = false;
            //reset compiler
            ResetCompiler();
            //get first command
            strNextCmd = NextCommand();

            //process commands in the input string list until finished
            while (lngLine != -1) { //Until lngLine = -1
                                    //reset last command flag
                blnLastCmdRtn = false;
                lngReturnLine = 0;

                //process the command
                switch (strNextCmd) {
                case "{":
                    //can't have a "{" command, unless it follows an 'if' or 'else'
                    if (strPrevCmd != "if" && strPrevCmd != "else") {
                        strErrMsg = LoadResString(4008);
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    break;
                case "}":
                    //if no block currently open,
                    if (BlockDepth == 0) {
                        strErrMsg = LoadResString(4010);
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    //if last command was a new.room command, then closing block is expected
                    if (blnNewRoom) {
                        blnNewRoom = false;
                    }
                    //if last position in resource is two bytes from start of block
                    if (tmpLogRes.Size == BlockStartDataLoc[BlockDepth] + 2) {
                        if (ErrorLevel == leHigh) {
                            strErrMsg = LoadResString(4049);
                            //blnError = false; //why not set error flag?
                            return false;
                        }
                        else if (ErrorLevel == leMedium) {
                            //set warning
                            AddWarning(5001);
                            //} else if (ErrorLevel == leLow) {
                            //  no action
                        }
                    }
                    //calculate and write block length
                    BlockLength[BlockDepth] = tmpLogRes.Size - BlockStartDataLoc[BlockDepth] - 2;
                    tmpLogRes.WriteWord((ushort)BlockLength[BlockDepth], BlockStartDataLoc[BlockDepth]);
                    //remove block from stack
                    BlockDepth--;
                    break;
                case "if":
                    //compile the //if// statement
                    if (!CompileIf()) {
                        return false;
                    }
                    //if block stack exceeded
                    if (BlockDepth >= MAX_BLOCK_DEPTH) {
                        strErrMsg = LoadResString(4110).Replace(ARG1, MAX_BLOCK_DEPTH.ToString());
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    //add block to stack
                    BlockDepth++;
                    BlockStartDataLoc[BlockDepth] = tmpLogRes.Pos;
                    BlockIsif[BlockDepth] = true;
                    //write placeholders for block length
                    tmpLogRes.WriteWord(0);

                    //next command should be a bracket
                    strNextCmd = NextCommand();
                    if (strNextCmd != "{") {
                        //error
                        strErrMsg = LoadResString(4053);
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    break;
                case "else":
                    //else can only follow a close bracket
                    if (strPrevCmd != "}") {
                        strErrMsg = LoadResString(4011);
                        //blnError = false; //why not set error flag?
                        return false;
                    }

                    //if the block closed by that bracket was an 'else'
                    //(which will be determined by having that block's IsIf flag NOT being set),
                    if (!BlockIsif[BlockDepth + 1]) {
                        strErrMsg = LoadResString(4083);
                        //blnError = false; //why not set error flag?
                        return false;
                    }

                    //adjust blockdepth to the 'if' command
                    //directly before this 'else'
                    BlockDepth++;
                    //adjust previous block length to accomodate the //else// statement
                    BlockLength[BlockDepth] += 3;
                    tmpLogRes.WriteWord((ushort)BlockLength[BlockDepth], BlockStartDataLoc[BlockDepth]);
                    //previous 'if' block is now closed; use same block level
                    //for this 'else' block
                    BlockIsif[BlockDepth] = false;
                    //write the 'else' code
                    tmpLogRes.WriteByte(0xFE);
                    BlockStartDataLoc[BlockDepth] = tmpLogRes.Pos;
                    tmpLogRes.WriteWord(0);  // block length filled in later.
                                             //next command better be a bracket
                    strNextCmd = NextCommand();
                    if (strNextCmd != "{") {
                        //error
                        strErrMsg = LoadResString(4053);
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    break;
                case "goto":
                    //if last command was a new room, warn user
                    if (blnNewRoom) {
                        if (ErrorLevel == leHigh ||
                            ErrorLevel == leMedium) {
                            //set warning
                            AddWarning(5095);
                        }
                        else if (ErrorLevel == leLow) {
                            //no action
                        }
                        blnNewRoom = false;
                    }
                    //next command should be "("
                    if (NextChar() != "(") {
                        strErrMsg = LoadResString(4001);
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    //get goto argument
                    strArg = NextCommand();

                    //if argument is NOT a valid label
                    if (LabelNum(strArg) == -1) {
                        strErrMsg = LoadResString(4074).Replace(ARG1, strArg);
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    //if too many gotos
                    if (NumGotos >= MaxGotos) {
                        strErrMsg = LoadResString(4108).Replace(ARG1, MaxGotos.ToString());
                    }
                    //save this goto info on goto stack
                    NumGotos++;
                    Gotos[NumGotos - 1].LabelNum = (byte)LabelNum(strArg);
                    //write goto command byte
                    tmpLogRes.WriteByte(0xFE);
                    Gotos[NumGotos].DataLoc = tmpLogRes.Pos;
                    //write placeholder for amount of offset
                    tmpLogRes.WriteWord(0);
                    //next character should be ")"
                    if (NextChar() != ")") {
                        strErrMsg = LoadResString(4003);
                        //blnError = false; //why not set error flag?
                        return false;
                    }
                    //verify next command is end of line (;)
                    if (NextChar() != ";") {
                        //blnError = true;
                        strErrMsg = LoadResString(4007);
                        return false;
                    }
                    break;
                case "/*":
                case "*/":
                    //since block commands are no longer supported, check for markers in order to provide a
                    //meaningful error message
                    //blnError = true;
                    strErrMsg = LoadResString(4052);
                    return false;
                case "++":
                case "--": //unary operators; need to get a variable next
                           //write the command code
                    if (strNextCmd == "++") {
                        tmpLogRes.WriteByte(1);
                    }
                    else {
                        tmpLogRes.WriteByte(2);
                    }
                    //get the variable to update
                    strArg = NextCommand();
                    //convert it
                    if (!ConvertArgument(ref strArg, atVar)) {
                        //error
                        //blnError = true; //here, error flag IS set
                        strErrMsg = LoadResString(4046);
                        return false;
                    }
                    //get Value
                    intCmdNum = VariableValue(strArg);
                    if (intCmdNum == -1) {
                        //blnError = true;
                        strErrMsg = LoadResString(4066).Replace(ARG1, "");
                        return false;
                    }
                    //write the variable value
                    tmpLogRes.WriteByte((byte)intCmdNum);
                    //verify next command is end of line ';'
                    if (NextChar(true) != ";") {
                        //blnError = false; //why not set error flag?
                        strErrMsg = LoadResString(4007);
                        return false;
                    }
                    break;
                case ":":  //alternate label syntax
                           //get next command; it should be the label
                    strNextCmd = NextCommand();
                    intLabelNum = LabelNum(strNextCmd);
                    //if not a valid label
                    if (intLabelNum == -1) {
                        //blnError = false; //why not set error flag?
                        strErrMsg = LoadResString(4076);
                        return false;
                    }
                    //save position of label
                    llLabel[intLabelNum].Loc = tmpLogRes.Size;
                    break;
                default:
                    //must be a label:, command, or special syntax
                    //if next character is a colon
                    if (strCurrentLine[lngPos + 1] == ':') {
                        //it might be a label
                        intLabelNum = LabelNum(strNextCmd);
                        //if not a valid label
                        if (intLabelNum == -1) {
                            strErrMsg = LoadResString(4076);
                            //blnError = false; //why not set error flag?
                            return false;
                        }
                        //save position of label
                        llLabel[intLabelNum].Loc = tmpLogRes.Size;
                        //read in next char to skip past the colon
                        NextChar();
                    }
                    else {
                        //if last command was a new room (and not followed by return), warn user
                        if (blnNewRoom && strNextCmd != "return") {
                            if (ErrorLevel == leHigh ||
                                ErrorLevel == leMedium) {
                                //set warning
                                AddWarning(5095);
                                //} else if (ErrorLevel == leHigh) {
                                //  //no action
                            }
                            blnNewRoom = false;
                        }

                        //get  command opcode number
                        intCmdNum = CommandNum(false, strNextCmd);
                        //if invalid version
                        if (intCmdNum == 254) {
                            //raise error
                            strErrMsg = LoadResString(4065).Replace(ARG1, strNextCmd);
                            return false;
                            //if command not found,
                        }
                        else if (intCmdNum == 255) {  // not found
                                                      //try to parse special syntax
                            if (CompileSpecial(strNextCmd)) {
                                //check for error
                                if (blnCriticalError) {
                                    return false;
                                }
                            }
                            else {
                                //unknown command
                                strErrMsg = LoadResString(4116).Replace(ARG1, strNextCmd);
                                //blnError = false; //why not set error flag?
                                return false;
                            }
                        }
                        else {
                            //write the command code,
                            tmpLogRes.WriteByte((byte)intCmdNum);
                            //next character should be "("
                            if (NextChar() != "(") {
                                strErrMsg = LoadResString(4048);
                                //blnError = false; //why not set error flag?
                                return false;
                            }
                            //reset the quotemark error flag
                            lngQuoteAdded = -1;
                            //now extract arguments,
                            for (i = 0; i < ActionCommands[intCmdNum].ArgType.Length; i++) {
                                //after first argument, verify comma separates arguments
                                if (i > 0) {
                                    if (NextChar(true) != ",") {
                                        //check for added quotes; they are the problem
                                        if (lngQuoteAdded >= 0) {
                                            //reset line;
                                            lngLine = lngQuoteAdded;
                                            lngErrLine = lngLine - lngIncludeOffset;
                                            //string error
                                            strErrMsg = LoadResString(4051);
                                        }
                                        else {
                                            //use 1-base arg values
                                            strErrMsg = LoadResString(4047).Replace(ARG1, (i + 1).ToString());
                                        }
                                        //blnError = false; //why not set error flag?
                                        return false;
                                    }
                                }
                                bytArg[i] = (byte)GetNextArg(ActionCommands[(byte)intCmdNum].ArgType[i], i);
                                //if error
                                if (blnCriticalError) {
                                    // if error number is 4054
                                    if (Left(strErrMsg, 4) == "4054") {
                                        // add command name to error string
                                        strErrMsg = strErrMsg.Replace(ARG2, ActionCommands[intCmdNum].Name);
                                    }
                                    //blnError = false; //why not set error flag?
                                    return false;
                                }
                                //write argument
                                tmpLogRes.WriteByte(bytArg[i]);
                            }//nexti
                             //validate arguments for this command
                            if (!ValidateArgs(intCmdNum, ref bytArg)) {
                                return false;
                            }
                            //next character must be ")"
                            if (NextChar() != ")") {
                                //blnError = true;
                                //check for added quotes; they are the problem
                                if (lngQuoteAdded >= 0) {
                                    //reset line;
                                    lngLine = lngQuoteAdded;
                                    lngErrLine = lngLine - lngIncludeOffset;
                                    //string error
                                    strErrMsg = LoadResString(4051);
                                }
                                else {
                                    strErrMsg = LoadResString(4160);
                                }
                                return false;
                            }
                            if (intCmdNum == 0) {
                                blnLastCmdRtn = true;
                                //set line number
                                if (lngReturnLine == 0) {
                                    lngReturnLine = lngLine + 1;
                                }
                            }
                        }

                        //verify next command is end of line (;)
                        if (NextChar(true) != ";") {
                            strErrMsg = LoadResString(4007);
                            return false;
                        }
                    }
                    break;
                }
                //get next command
                strPrevCmd = strNextCmd;
                strNextCmd = NextCommand();
            }//loop

            if (!blnLastCmdRtn) {
                if (ErrorLevel == leHigh) {
                    //error
                    strErrMsg = LoadResString(4102);
                    //blnError = false; //why not set error flag?
                    return false;
                }
                else if (ErrorLevel == leMedium ||
                         ErrorLevel == leLow) {
                    //add the missing return code
                    tmpLogRes.WriteByte(0);
                    //and a warning
                    AddWarning(5016);
                }
            }
            //check to see if everything was wrapped up properly

            if (BlockDepth > 0) {
                strErrMsg = LoadResString(4009);
                //reset errorline to return cmd
                lngErrLine = lngReturnLine;
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
        static internal int MessageNum(string strMsgIn)
        {
            // returns the number of the message corresponding to
            //strMsg, or creates a new msg number if strMsg is not
            //currently a message
            //if maximum number of msgs assigned, returns  0
            int lngMsg;

            //blank msgs normally not allowed
            if (strMsgIn.Length == 0) {
                if (ErrorLevel == leHigh ||
                    ErrorLevel == leMedium) {
                    AddWarning(5074);
                    //if (ErrorLevel == leLow) {
                    //allow it
                }
            }
            for (lngMsg = 1; lngMsg <= 255; lngMsg++) {
                //if this is the message
                //(this is a case-sensitive search)
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

            //msg isn't in list yet;
            // validate it (check for invalid characters)
            if (!ValidateMsgChars(strMsgIn, lngMsg)) {
                //return a value to indicate error
                return -1;
            }
            // then find an empty spot
            for (lngMsg = 1; lngMsg <= 255; lngMsg++) {
                if (!blnMsg[lngMsg]) {
                    //this message is available
                    blnMsg[lngMsg] = true;
                    strMsg[lngMsg] = strMsgIn;
                    return lngMsg;
                }
            }//nxt lngMsg

            //if no room found, return zero
            return 0;
        } //endfunction
        static internal byte CommandNum(bool blnIF, string strCmdName)
        {  //gets the command number
           //of a command, based on the text
            if (blnIF) {
                for (byte retval = 0; retval < agNumTestCmds; retval++) {
                    if (strCmdName.Equals(TestCommands[retval].Name, StringComparison.OrdinalIgnoreCase)) {
                        return retval;
                    }
                }
            }
            else {
                for (byte retval = 0; retval < agNumCmds; retval++) {
                    if (strCmdName.Equals(ActionCommands[retval].Name, StringComparison.OrdinalIgnoreCase)) {
                        return retval;
                    }
                }

                //maybe the command is a valid agi command, but
                //just not supported in this agi version
                for (int retval = agNumCmds; retval < MAX_CMDS; retval++) {
                    if (strCmdName.Equals(ActionCommands[retval].Name, StringComparison.OrdinalIgnoreCase)) {
                        if (ErrorLevel == leHigh) {
                            //error; return cmd Value of 254 so compiler knows to raise error
                            return 254;
                        }
                        else if (ErrorLevel == leMedium) {
                            //add warning
                            AddWarning(5075, LoadResString(5075).Replace(ARG1, strCmdName));
                            //} else if (ErrorLevel == leLow) {
                            //  //don't worry about command validity; return the extracted command num
                        }
                    }
                }
            }
            // not found
            return 255;
        }
        static internal bool CompileSpecialif(string strArg1, bool blnNOT)
        {
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
            bool blnArg2Var = false;
            bool blnAddNOT = false;
            byte bytCmdNum;
            //check for variable argument
            if (ConvertArgument(ref strArg1, atVar)) {
                //arg 1 is 'v#'
                intArg1 = VariableValue(strArg1);
                //if invalid variable number
                if (intArg1 == -1) {
                    //invalid number
                    //blnError = true;
                    strErrMsg = LoadResString(4086);
                    return false;
                }
                //get comparison expression
                strArg2 = NextCommand();
                //get command code for this expression
                switch (strArg2) {
                case EQUAL_TOKEN:
                    bytCmdNum = 0x1;
                    break;
                case NOTEQUAL_TOKEN:
                    bytCmdNum = 0x1;
                    blnAddNOT = true;
                    break;
                case ">":
                    bytCmdNum = 0x5;
                    break;
                case "<=":
                case "=<":
                    bytCmdNum = 0x5;
                    blnAddNOT = true;
                    break;
                case "<":
                    bytCmdNum = 0x3;
                    break;
                case ">=":
                case "=>":
                    bytCmdNum = 0x3;
                    blnAddNOT = true;
                    break;
                case ")":
                case "&&":
                case "||":
                    //means we are doing a boolean test of the variable;
                    //use greatern with zero as arg
                    tmpLogRes.WriteByte(0x5);
                    tmpLogRes.WriteByte((byte)intArg1);
                    tmpLogRes.WriteByte(0);

                    //backup the compiler pos so main compiler loop can 
                    // find the next command
                    lngPos -= strArg2.Length;
                    return true;

                default:
                    //blnError = true;
                    strErrMsg = LoadResString(4078);
                    return false;
                }
                //before getting second arg, check for NOT symbol in front of a variable
                //can't have a NOT in front of variable comparisons
                if (blnNOT) {
                    //blnError = true;
                    strErrMsg = LoadResString(4098);
                    return false;
                }
                //get second argument (numerical or variable)
                blnArg2Var = true;
                //reset the quotemark error flag
                lngQuoteAdded = -1;
                intArg2 = GetNextArg(atNum, -1, ref blnArg2Var);
                //if error
                if (blnCriticalError) {
                    //if an invalid arg value found
                    if (Val(strErrMsg) == 4063) {
                        //change error message
                        strErrMsg = Mid(strErrMsg, 55, strErrMsg.LastIndexOf("'") - 53);
                        strErrMsg = LoadResString(4089).Replace(ARG1, strErrMsg);
                    }
                    else {
                        strErrMsg = LoadResString(4089).Replace(ARG1, "");
                    }
                    return false;
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
                //success
                return true;
            }
            //check for flag argument
            else if (ConvertArgument(ref strArg1, atFlag)) {
                //get flag argument Value
                intArg1 = VariableValue(strArg1);
                //if invalid flag number
                if (intArg1 == -1) {
                    //invalid number
                    //blnError = true;
                    strErrMsg = LoadResString(4066).Replace(ARG1, "1");
                    return false;
                }
                //write isset cmd
                tmpLogRes.WriteByte(0x7);  // isset
                tmpLogRes.WriteByte((byte)intArg1);
                //success
                return true;
            }
            else {
                //invalid argument
                //blnError = true;
                strErrMsg = LoadResString(4039).Replace(ARG1, strArg1);
                return false;
            }
        } //endfunction
        static internal bool CompileSpecial(string strArgIn)
        {
            //strArg1 should be a variable in the format of *v##, v##, f## or s##
            //if it is not, this function will trap it and return an error
            //the expression after strArg1 should be one of the following:
            // =, +=, -=, *=, /=, ++, --
            //
            //after determining assignment type, this function will validate additional
            //arguments as necessary
            //
            string strArg1, strArg2;
            int intArg1, intArg2 = -1;
            bool blnArg2Var = false;
            int intDir = 0; //0 = no indirection; 1 = left; 2 = right
            byte bytCmd = 0;
            byte[] bytArgs = Array.Empty<byte>();
            strArg1 = strArgIn;
            //if this is indirection
            if (strArg1[1] == '*') {
                //left indirection
                //     *v# = #
                //     *v# = v#
                if (lngPos + 1 == strCurrentLine.Length) {
                    //blnError = true;
                    strErrMsg = LoadResString(4105);
                    return false;
                }
                //next char can't be a space, newline, or tab
                switch (strCurrentLine[lngPos + 1]) {
                case ' ':
                case '\t': //tab
                           //error
                    //blnError = true;
                    strErrMsg = LoadResString(4105);
                    return false;
                }

                //get actual first arg
                intArg1 = GetNextArg(atVar, -1);
                //if error
                if (blnCriticalError) {
                    //adjust error message
                    strErrMsg = LoadResString(4064);
                    return false;
                }
                intDir = 1;
                //next character must be "="
                strArg2 = NextCommand();
                if (strArg2 != "=") {
                    //error
                    strErrMsg = LoadResString(4105);
                    //blnError = true;
                    return false;
                }
                //if this arg is string
            }
            else if (ConvertArgument(ref strArg1, atStr)) {
                //string assignment
                //     s# = m#
                //     s# = "<string>"
                //get string variable number
                intArg1 = VariableValue(strArg1);
                if (ErrorLevel != leLow) {
                    //for version 2.089, 2.272, and 3.002149 only 12 strings
                    switch (compGame.agIntVersion) {
                    case "2.089":
                    case "2.272":
                    case "3.002149":
                        if (intArg1 > 11) {
                            if (ErrorLevel == leHigh) {
                                //use 1-based arg values
                                strErrMsg = LoadResString(4079).Replace(ARG1, "1").Replace(ARG2, "11");
                                //blnError = true;
                                return false;
                            }
                            else { // leMedium
                                AddWarning(5007, LoadResString(5007).Replace(ARG1, "11"));
                            }
                        }
                        break;
                    //for all other versions, limit is 24 strings
                    default:
                        if (intArg1 > 23) {
                            if (ErrorLevel == leHigh) {
                                strErrMsg = LoadResString(4079).Replace(ARG1, "1").Replace(ARG2, "23");
                                //blnError = true;
                                return false;
                            }
                            else { // leMedium
                                AddWarning(5007, LoadResString(5007).Replace(ARG1, "23"));
                            }
                        }
                        break;
                    }
                }
                //check for equal sign
                strArg2 = NextCommand();
                //if not equal sign
                if (strArg2 != "=") {
                    //error
                    strErrMsg = LoadResString(4034);
                    //blnError = true;
                    return false;
                }
                //get actual second variable
                // (use argument extractor in case second variable is a literal string)
                intArg2 = GetNextArg(atMsg, -1);
                //if error
                if (blnCriticalError) {
                    // if error number is 4054
                    if (Left(strErrMsg, 4) == "4054") {
                        // change it to 4058
                        strErrMsg = LoadResString(4058);
                    }
                    //then exit
                    return false;
                }

                //command is set.string
                bytCmd = 0x72;
                //if this is a variable
            }
            else if (ConvertArgument(ref strArg1, atVar)) {
                //arg 1 must be //v#// format
                intArg1 = VariableValue(strArg1);
                //if invalid variable number
                if (intArg1 == -1) {
                    //invalid number
                    strErrMsg = LoadResString(4085);
                    //blnError = true;
                    return false;
                }
                //variable assignment or arithmetic operation
                //need next command to determine what kind of assignment/operation
                strArg2 = NextCommand();
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
                default:
                    //don't know what the heck it is...
                    strErrMsg = LoadResString(4034);
                    //blnError = true;
                    return false;
                }
                //check for flag assignment
            }
            else if (ConvertArgument(ref strArg1, atFlag)) {
                //flag assignment
                //     f# = true;
                //     f# = false;
                //get flag number
                intArg1 = VariableValue(strArg1);
                //check for equal sign
                strArg2 = NextCommand();
                //if not equal sign
                if (strArg2 != "=") {
                    //error
                    strErrMsg = LoadResString(4034);
                    //blnError = true;
                    return false;
                }
                //get flag Value
                strArg2 = NextCommand();
                switch (strArg2.ToLower()) {
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
                    strErrMsg = LoadResString(4034);
                    //blnError = true;
                    return false;
                }
            }
            else {
                //not a special syntax
                return false;
            }
            // get second argument
            switch (bytCmd) {
            case 0x1:
            case 0x2:
            case 0xC:
            case 0xD:
            case 0x72:
                //skip check for second argument if cmd is known to be a single arg
                //commands: increment, decrement, reset, set
                //(set string is also skipped because second arg is already determined)
                break;
            default:
                //get next argument
                strArg2 = NextCommand();
                //if it is indirection
                if (strArg2 == "*") {
                    //if not already using left indirection, AND cmd is not known
                    if (intDir == 0 && bytCmd == 0) {
                        //set right indirection
                        intDir = 2;

                        //next char can't be a space, tab or end of line
                        if (lngPos + 1 == strCurrentLine.Length) {
                            //error
                            strErrMsg = LoadResString(4105);
                            //blnError = true;
                            return false;
                        }
                        switch (strCurrentLine[lngPos + 1]) {
                        case ' ':
                        case '\t': // tab
                                   //error
                            strErrMsg = LoadResString(4105);
                            //blnError = true;
                            return false;
                        }
                        //get actual variable
                        intArg2 = GetNextArg(atVar, -1);
                        if (blnCriticalError) {
                            //reset error string
                            strErrMsg = LoadResString(4105);
                            return false;
                        }
                    }
                    else {
                        //bad indirection syntax
                        strErrMsg = LoadResString(4105);
                        //blnError = true;
                        return false;
                    }
                }
                else {
                    //arg2 is either number or variable- convert input to standard syntax
                    //if it's a number, check for negative value
                    if (Val(strArg2) < 0) {
                        //valid negative numbers are -1 to -128
                        if (Val(strArg2) < -128) {
                            //error
                            strErrMsg = LoadResString(4095);
                            //blnError = true;
                            return false;
                        }
                        //convert it to 2s-compliment unsigned value by adding it to 256
                        strArg2 = (256 + Val(strArg2)).ToString();
                        if (ErrorLevel == leHigh ||
                            ErrorLevel == leMedium) {
                            //show warning
                            AddWarning(5098);
                        }
                    }
                    // arg2 can be a number or a variable
                    blnArg2Var = true;
                    if (!ConvertArgument(ref strArg2, atNum, ref blnArg2Var)) {
                        //set error
                        strErrMsg = LoadResString(4088).Replace(ARG1, strArg2);
                        //blnError = true;
                        return false;
                    }
                    //it's a number or variable; verify it's 0-255
                    intArg2 = VariableValue(strArg2);
                    //if invalid
                    if (intArg2 == -1) {
                        //set error
                        strErrMsg = LoadResString(4088).Replace(ARG1, strArg2);
                        //blnError = true;
                        return false;
                    }
                    //if arg2 is a number
                    if (!blnArg2Var) {
                        //if cmd is not yet known,
                        if (bytCmd == 0) {
                            //must be assign
                            // v# = #
                            // *v# = #
                            if (intDir == 1) {
                                //lindirect.n
                                bytCmd = 0xB;
                            }
                            else {
                                //assign.n
                                bytCmd = 0x3;
                            }
                        }
                    }
                } // not indirection
                break;
            }  //if not inc/dec

            //if command is not known by now
            if (bytCmd == 0) {
                //if arg values are the same (already know arg2 is a variable)
                //and no indirection
                if ((intArg1 == intArg2) && (intDir == 0)) {
                    //check for long arithmetic
                    strArg2 = NextCommand();
                    //if end of command is reached
                    if (strArg2 == ";") {
                        //move pointer back one space so eol
                        //check in CompileAGI works correctly
                        lngPos--;
                        //this is a simple assign (with a variable being assigned to itself!!)
                        if (ErrorLevel == leHigh) {
                            strErrMsg = LoadResString(4084);
                            //blnError = true;
                            return false;
                        }
                        else if (ErrorLevel == leMedium) {
                            AddWarning(5036);
                            //} else if (ErrorLevel == leLow) {
                            //  //allow it
                        }
                        // assign.v
                        bytCmd = 0x3;
                    }
                    else {
                        //this may be long arithmetic
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
                            strErrMsg = LoadResString(4087);
                            //blnError = true;
                            return false;
                        }
                        //now get actual second argument (number or variable)
                        blnArg2Var = true;
                        intArg2 = GetNextArg(atNum, -1, ref blnArg2Var);
                        //if error
                        if (blnCriticalError) {
                            if (Val(strErrMsg) == 4063) {
                                //change error message
                                strErrMsg = Mid(strErrMsg, 55, strErrMsg.LastIndexOf("'") - 53);
                                strErrMsg = LoadResString(4161).Replace(ARG1, strErrMsg);
                            }
                            else {
                                strErrMsg = LoadResString(4161).Replace(ARG1, "");
                            }
                            return false;
                        }
                    }
                }
                else {
                    //the second variable argument is different
                    //must be assignment
                    // v# = v#
                    // *v# = v#
                    // v# = *v#
                    switch (intDir) {
                    case 0:  //assign.v
                        bytCmd = 0x4;
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
            //if second argument is a variable
            if (blnArg2Var) {
                // use variable version of command
                bytCmd++;
            }
            //get next command on this line
            strArg2 = NextCommand(true);

            //check that next command is semicolon
            if (strArg2 != ";") {
                //blnError = true;
                //check for added quotes; they are the problem
                if (lngQuoteAdded >= 0) {
                    //reset line;
                    lngLine = lngQuoteAdded;
                    lngErrLine = lngLine - lngIncludeOffset;
                    //string error
                    strErrMsg = LoadResString(4051);
                }
                else {
                    strErrMsg = LoadResString(4007);
                }
                return false;
            }
            else {
                //move pointer back one space so
                //eol check in CompileAGI works
                //correctly
                lngPos--;
            }

            //need to validate arguments for this command
            switch (bytCmd) {
            case 0x1: //increment
            case 0x2: //decrement
            case 0xC: //set
            case 0xD: //reset
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
            case 0x1:
            case 0x2:
            case 0xC:
            case 0xD:
                break;
            default:
                tmpLogRes.WriteByte((byte)intArg2);
                break;
            }
            return true;
        } //endfunction
    }
}
