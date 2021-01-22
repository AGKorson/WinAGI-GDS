using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;
using static WinAGI.ArgTypeEnum;
using static WinAGI.LogicErrorLevel;
using static WinAGI.WinAGIRes;

namespace WinAGI
{
  public static partial class WinAGI
  {
    // reminder: the compiler is not case sensitive
    //EXCEPT strings in messages; they ARE case sensitive
    //need to consider that in the getmsg function!!!!

    internal struct LogicGoto
    {
      byte LabelNum;
      int DataLoc;
    }
    internal struct LogicLabel
    {
      internal string Name;
      internal int Loc;
    }

    internal static AGILogic tmpLogRes;
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
    internal static byte bytLogComp;
    internal static string strLogCompID;
    internal static bool blnError;
    internal static int lngQuoteAdded;
    internal static string strErrMsg;
    internal static string strModule, strModFileName;
    internal static int lngErrLine;
    internal static int intCtlCount;
    internal static bool blnNewRoom;
    internal static string[] strIncludeFile;
    internal static int lngIncludeOffset; //to correct line number due to added include lines
    internal static List<string> stlInput = new List<string>();  //the entire text to be compiled; includes the
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

    internal static void CompileLogic(AGILogic SourceLogic)
    {
      //this function compiles the sourcetext that is passed
      //the function returns a Value of true if successful; it returns false
      //and sets information about the error if an error in the source text is found

      //note that when errors are returned, line is adjusted because
      //editor rows(lines) start at //1//, but the compiler starts at line //0//

      bool blnCompiled;
      List<string> stlSource = new List<string>();

      //set error info to success as default
      blnError = false;
      lngErrLine = -1;
      strErrMsg = "";
      strModule = "";
      strModFileName = "";
      intCtlCount = 0;

      //initialize global defines
      //get datemodified property
      DateTime dtFileMod = File.GetLastWriteTime(agGameDir + "globals.txt");
      if (CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString())) != agGlobalCRC) {
        GetGlobalDefines();
      }
      //if ids not set yet
      if (!blnSetIDs) {
        SetResourceIDs();
      }

      //insert current values for reserved defines that can change values
      //agResDef[0].Value = "ego"  //this one doesn't change
      agResDef[1].Value = "\"" + agGameVersion + "\"";
      agResDef[2].Value = "\"" + agAbout + "\"";
      agResDef[3].Value = "\"" + agGameID + "\"";
      if (agInvObj.Loaded) {
        //Count of ACTUAL useable objects is one less than inventory object Count
        //because the first object ('?') is just a placeholder
        agResDef[4].Value = (agInvObj.Count - 1).ToString();
      } else {
        agResDef[4].Value = "-1";
      }

      //get source text by lines as a list of strings
      stlSource = SplitLines(SourceLogic.SourceText);
      bytLogComp = SourceLogic.Number;
      strLogCompID = SourceLogic.ID;

      //reset error info
      lngErrLine = -1;
      strErrMsg = "";
      strModule = "";
      strModFileName = "";

      //add include files (extchars handled automatically)
      if (!AddIncludes(stlSource)) {
        //dereference objects
        //return error
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }

      //remove any blank lines from end
      while (stlInput[stlInput.Count - 1].Length == 0 && stlInput.Count > 0) { // Until Len(stlInput(stlInput.Count - 1)) != 0 || stlInput.Count = 0

        stlInput.RemoveAt(stlInput.Count - 1);
      }

      //if nothing to compile, throw an error
      if (stlInput.Count == 0) {
        //dereference objects
        //return error
        strErrMsg = LoadResString(4159);
        lngErrLine = 0;
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }

      //strip out all comments
      if (!RemoveComments()) {
        //return error
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }

      //read labels
      if (!ReadLabels()) {
        //return error
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }

      //enumerate and replace all the defines
      if (!ReadDefines()) {
        //return error
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }
      //read predefined messages
      if (!ReadMsgs()) {
        //return error
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }

      //assign temporary resource object
      tmpLogRes = new AGILogic();
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
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }
      //write message section
      if (!WriteMsgs()) {
        tmpLogRes.Unload();
        //return error
        throw new Exception("635, LogCompile, " + (lngErrLine + 1).ToString() + " | " + strModule + " | " + strErrMsg);
      }

      //assign resource data
      SourceLogic.Data.AllData = tmpLogRes.Data.AllData;

      //update compiled crc
      SourceLogic.CompiledCRC = SourceLogic.CRC;
      // and write the new crc values to property file
      WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CRC32", "0x" + SourceLogic.CRC, "Logics");
      WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CompCRC32", "0x" + SourceLogic.CompiledCRC);

      //done with the temp resource
      tmpLogRes.Unload();
    }
    internal static void SetResourceIDs()
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
      foreach (AGILogic tmpLog in agLogs) {
        strLogID[tmpLog.Number] = tmpLog.ID;
      }
      foreach (AGIPicture tmpPic in agPics) {
        strPicID[tmpPic.Number] = tmpPic.ID;
      }
      foreach (AGISound tmpSnd in agSnds) {
        strSndID[tmpSnd.Number] = tmpSnd.ID;
      }
      foreach (AGIView tmpView in agViews) {
        strViewID[tmpView.Number] = tmpView.ID;
      }
      //set flag
      blnSetIDs = true;
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
      case ArgTypeEnum.atNum:  //numeric only
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
            } else if (tdDefines[i].Value[0] == 34) {
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
        for (i = 0; i < agGlobalCount; i++) {
          if (agGlobal[i].Type == ArgType) {
            if (strArgIn == agGlobal[i].Name) {
              strArgIn = agGlobal[i].Value;
              //reset VarOrNum flag
              blnVarOrNum = false;
              return true;
            }
          }
        } //nexti
          //if checking var or num
        if (blnVarOrNum) {
          //numbers were checked; need to check variables
          for (i = 0; i < agGlobalCount; i++) {
            if (agGlobal[i].Type == atVar) {
              if (strArgIn == agGlobal[i].Name) {
                strArgIn = agGlobal[i].Value;
                return true;
              }
            }
          } //nexti
        }
      } else {
        //check vocab words only against numbers
        for (i = 0; i < agGlobalCount; i++) {
          if (agGlobal[i].Type == atNum) {
            if (strArgIn == agGlobal[i].Name) {
              strArgIn = agGlobal[i].Value;
              return true;
            }
          }
        } //nexti
      }
      //check messages, iobjs, and vocab words against global strings
      if ((ArgType == atMsg) || (ArgType == atIObj) || (ArgType == atVocWrd)) {
        //check against global defines (string type)
        for (i = 0; i < agGlobalCount; i++) {
          if (agGlobal[i].Type == atDefStr) {
            if (strArgIn == agGlobal[i].Name) {
              strArgIn = agGlobal[i].Value;
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
      if (agUseRes) {
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
          if (strArgIn == agResDef[4].Name) {
            strArgIn = agResDef[4].Value;
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
          for (i = 1; i <= 3; i++) { //for gamever and gameabout and gameid
            if (strArgIn == agResDef[i].Name) {
              strArgIn = agResDef[i].Value;
              return true;
            }
          }
          break;
        case atSObj:
          if (strArgIn == agResDef[0].Name) {
            strArgIn = agResDef[0].Value;
            return true;
          }
          break;
        case atStr:
          if (strArgIn == agResDef[5].Name) {
            strArgIn = agResDef[5].Value;
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
        } else {
          strVarVal = Right(strVar, strVar.Length - 1);
        }
      } else {
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
      string strIncludePath;
      string strIncludeText;
      int CurIncludeLine;   // current line in IncludeLines (the include file)
      int intFileCount = 0;
      int i;
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
            if (agMainLogSettings.ErrorLevel == leHigh) {
              //return error: improper use of quote marks
              strErrMsg = LoadResString(4059);
              return false;
            } else {// leMedium, leLow
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
            strIncludeFilename = agResDir + strIncludeFilename;
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
        } else {
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
    static internal string ArgTypeName(ArgTypeEnum ArgType)
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
    static internal void CheckResFlagUse(byte ArgVal)
    {
      //if error level is low, don't do anything
      if (agMainLogSettings.ErrorLevel == leLow) {
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
      } else {
        //all other reserved variables should be read only
        AddWarning(5025, LoadResString(5025).Replace(ARG1, agResFlag[ArgVal].Name));
      }
    }
    static internal void CheckResVarUse(byte ArgNum, byte ArgVal)
    {
      //if error level is low, don't do anything
      if (agMainLogSettings.ErrorLevel == leLow) {
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
    static internal int GetNextArg(ArgTypeEnum ArgType, int ArgPos, ref bool blnVarOrNum)
    {
      //this function retrieves the next argument and validates
      //that the argument is of the correct type
      //and has a valid Value
      //multline message/string/inv.item/word strings are recombined, and checked
      //for validity
      //if successful, the function returns the Value of the argument
      //if unsuccessful, the function sets the error flag and error msg
      //(in which case, the return Value is meaningless)

      //special syntax compilers look for variables OR strings;
      //when argtype is atNum and this flag is set, numbers OR
      //variables return true; the flag is set to TRUE if returned Value
      //is for a variable, to false if it is for a number

      // max number of invobjs; (not sure why using this value instead of agInvOb.Count...)
      int max = int.Parse(agResDef[4].Value);
      string strArg;
      int lngArg = 0;
      int i;

      //get next command
      strArg = NextCommand();

      //convert it
      if (!ConvertArgument(ref strArg, ArgType, ref blnVarOrNum)) {
        //error
        blnError = true;
        //if a closing paren found
        if (strArg == ")") {
          // arg missing
          strErrMsg = LoadResString(4054).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG3, ArgTypeName(ArgType));
        } else {
          //use 1-base arg values
          strErrMsg = LoadResString(4063).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, ArgTypeName(ArgType)).Replace(ARG3, strArg);
        }
        return -1;
      }

      switch (ArgType) {
      case atNum:  //number
                   //verify type is number
        if (!IsNumeric(strArg)) {
          //if NOT catching variables too
          if (!blnVarOrNum) {
            blnError = true;
            strErrMsg = LoadResString(4062).Replace(ARG1, (ArgPos).ToString());
            return -1;
          }
        } else {
          //return //is NOT a variable//; ensure flag is reset
          blnVarOrNum = false;
        }
        //check for negative number
        if (Val(strArg) < 0) {
          //valid negative numbers are -1 to -128
          if (Val(strArg) < -128) {
            //error
            blnError = true;
            strErrMsg = LoadResString(4157);
            return -1;
          }
          //convert it to 2s-compliment unsigned value by adding it to 256
          strArg = (256 + Val(strArg)).ToString();
          if (agMainLogSettings.ErrorLevel == leHigh ||
              agMainLogSettings.ErrorLevel == leMedium) {
            //show warning
            AddWarning(5098);
          }
        }
        //convert to number and validate
        lngArg = VariableValue(strArg);
        if (lngArg == -1) {
          blnError = true;
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
          blnError = true;
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
          blnError = true;
          //if high errlevel
          if (agMainLogSettings.ErrorLevel == leHigh) {
            //use 1-based arg values
            strErrMsg = LoadResString(4136).Replace(ARG1, (ArgPos + 1).ToString());
          } else {
            //use 1-based arg values
            strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
          }
          return -1;
        } else {
          //if outside expected bounds (controllers should be limited to 0-49)
          if (lngArg > 49) {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              //generate error
              blnError = true;
              //use 1-based arg values
              strErrMsg = LoadResString(4136).Replace(ARG1, (ArgPos + 1).ToString());
              return -1;
            } else if (agMainLogSettings.ErrorLevel == leMedium) {
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
          blnError = true;
          //use 1-based arg values
          strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
          return -1;
        }

        //check against max screen object Value
        if (lngArg > agInvObj.MaxScreenObjects) {
          if (agMainLogSettings.ErrorLevel == leHigh) {
            //generate error
            blnError = true;
            strErrMsg = LoadResString(4119).Replace(ARG1, (agInvObj.MaxScreenObjects).ToString());
            return -1;

          } else if (agMainLogSettings.ErrorLevel == leMedium) {
            //generate warning
            AddWarning(5006, LoadResString(5006).Replace(ARG1, agInvObj.MaxScreenObjects.ToString()));
          }
        }
        break;
      case atStr: //string
                  //get Value
        lngArg = VariableValue(strArg);
        if (lngArg == -1) {
          blnError = true;
          //if high errlevel
          if (agMainLogSettings.ErrorLevel == leHigh) {
            //for version 2.089, 2.272, and 3.002149 only 12 strings
            switch (agIntVersion) {
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
          } else {
            //use 1-based arg values
            strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
          }
          return -1;
        } else {
          //if outside expected bounds (strings should be limited to 0-23)
          if ((lngArg > 23) || (lngArg > 11 && (agIntVersion == "2.089" || agIntVersion == "2.272" || agIntVersion == "3.002149"))) {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              //generate error
              blnError = true;
              //for version 2.089, 2.272, and 3.002149 only 12 strings
              switch (agIntVersion) {
              case "2.089":
              case "2.272":
              case "3.002149":
                //use 1-based arg values
                strErrMsg = LoadResString(4079).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, "11");
                return -1;
              default:
                strErrMsg = LoadResString(4079).Replace(ARG1, (ArgPos + 1).ToString()).Replace(ARG2, "23");
                return -1;
              }

            } else if (agMainLogSettings.ErrorLevel == leMedium) {
              //generate warning
              //for version 2.089, 2.272, and 3.002149 only 12 strings
              switch (agIntVersion) {
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
          blnError = true;
          //if high error level
          if (agMainLogSettings.ErrorLevel == leHigh) {
            //use 1-based arg values
            strErrMsg = LoadResString(4090).Replace(ARG1, (ArgPos + 1).ToString());
          } else {
            strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
          }
          return -1;
        } else {
          //if outside expected bounds (words should be limited to 0-9)
          if (lngArg > 9) {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              //generate error
              blnError = true;
              //use 1-based arg values
              strErrMsg = LoadResString(4090).Replace(ARG1, (ArgPos + 1).ToString());
              return -1;
            } else if (agMainLogSettings.ErrorLevel == leMedium) {
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
            blnError = true;
            //use 1-based arg values
            strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
            return -1;
          }
          //m0 is not allowed
          if (lngArg == 0) {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              blnError = true;
              strErrMsg = LoadResString(4107);
              return -1;
            } else if (agMainLogSettings.ErrorLevel == leMedium) {
              //generate warning
              AddWarning(5091, LoadResString(5091).Replace(ARG1, lngArg.ToString()));
              //make this a null msg
              blnMsg[lngArg] = true;
              strMsg[lngArg] = "";
              // }else if (agMainLogSettings.ErrorLevel == leLow) {
              //ignore; it will be handled when writing messages
            }
          }
          //verify msg exists
          if (!blnMsg[lngArg]) {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              blnError = true;
              strErrMsg = LoadResString(4113).Replace(ARG1, lngArg.ToString());
              return -1;
            } else if (agMainLogSettings.ErrorLevel == leMedium) {
              //generate warning
              AddWarning(5090, LoadResString(5090).Replace(ARG1, lngArg.ToString()));
              //make this a null msg
              blnMsg[lngArg] = true;
              strMsg[lngArg] = "";
              // }else if (agMainLogSettings.ErrorLevel == leLow) {
              //ignore; WinAGI adds a null value, so no error will occur
            }
          }
          break;
        case 34:
          //concatenate, if applicable
          strArg = ConcatArg(strArg);
          if (blnError) {
            //concatenation error; exit
            return -1;
          }
          //strip off quotes
          strArg = Mid(strArg, 2, strArg.Length - 2);
          //convert to msg number
          lngArg = MessageNum(strArg);
          //if unallowed characters found, error was raised; exit
          if (lngArg == -1) {
            blnError = true;
            return -1;
          }
          //if valid number not found
          if (lngArg == 0) {
            blnError = true;
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
        case 105:
          //validate Value
          lngArg = VariableValue(strArg);
          if (lngArg == -1) {
            blnError = true;
            //use 1-based arg values
            strErrMsg = LoadResString(4066).Replace(ARG1, (ArgPos + 1).ToString());
            return -1;
          }
          break;
        case 34:
          //concatenate, if applicable
          strArg = ConcatArg(strArg);
          if (blnError) {
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

          //step through all object names (using max value from resdef[4])
          for (i = 0; i <= max; i++) {
            //for (i = 0; i < agInvObj.Count; i++) {
            //if this is the object
            if (strArg == agInvObj[(byte)i].ItemName) {
              //return this Value
              lngArg = (byte)i;
              break;
            }
          }

          //if not found,
          if (i == max + 1) {
            //if (i == agInvObj.Count) {
            blnError = true;
            //check for added quotes; they are the problem
            if (lngQuoteAdded >= 0) {
              //reset line;
              lngLine = lngQuoteAdded;
              //string error
              strErrMsg = LoadResString(4051);
            } else {
              //use 1-base arg values
              strErrMsg = LoadResString(4075).Replace(ARG1, (ArgPos + 1).ToString());
            }
            return -1;
          }

          //if object is not unique
          if (!agInvObj[(byte)lngArg].Unique) {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              blnError = true;
              //use 1-based arg values
              strErrMsg = LoadResString(4036).Replace(ARG1, (ArgPos + 1).ToString());
              return -1;
            } else if (agMainLogSettings.ErrorLevel == leMedium) {
              //set warning
              AddWarning(5003, LoadResString(5003).Replace(ARG1, (ArgPos + 1).ToString()));
              //} else if (agMainLogSettings.ErrorLevel == leLow) {
              //no action
            }
          }
          break;
        }

        //if object number exceeds current object Count,
        //* // 
        //if (lngArg >= agInvObj.Count) {  //???? why did I change this???? 
        if (lngArg > max) {
          if (agMainLogSettings.ErrorLevel == leHigh) {
            blnError = true;
            //use 1-based arg values
            strErrMsg = LoadResString(4112).Replace(ARG1, (ArgPos + 1).ToString());
            return -1;
          } else if (agMainLogSettings.ErrorLevel == leMedium) {
            //set warning
            //use 1-based arg values
            AddWarning(5005, LoadResString(5005).Replace(ARG1, (ArgPos + 1).ToString()));
            //if (agMainLogSettings.ErrorLevel == leLow) {
            //no action
          }
        } else {
          //if object is a question mark, raise error/warning
          if (agInvObj[(byte)lngArg].ItemName == "?") {
            if (agMainLogSettings.ErrorLevel == leHigh) {
              blnError = true;
              //use 1-based arg values
              strErrMsg = LoadResString(4111).Replace(ARG1, (ArgPos + 1).ToString());
              return -1;
            } else if (agMainLogSettings.ErrorLevel == leMedium) {
              //set warning
              AddWarning(5004);
              //if (agMainLogSettings.ErrorLevel == leLow) {
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
            blnError = true;
            lngArg = -1;
          } else {
            //validate the group
            blnError = !agVocabWords.GroupExists(lngArg);
          }
        } else {
          //this is a string; concatenate if applicable
          strArg = ConcatArg(strArg);
          if (blnError) {
            //concatenation error
            return -1;
          }

          //convert to word number
          //first strip off starting and ending quotes
          strArg = Mid(strArg, 2, strArg.Length - 2);

          //get argument val by checking against word list
          if (agVocabWords.WordExists(strArg)) {
            lngArg = agVocabWords[strArg].Group;
          } else {
            //RARE, but if it's an 'a' or 'i' that isn't defined,
            //it's word group 0
            if (strArg == "i" || strArg == "a" || strArg == "I" || strArg == "A") {
              lngArg = 0;
              //add warning
              if (agMainLogSettings.ErrorLevel == leHigh || agMainLogSettings.ErrorLevel == leMedium) {
                AddWarning(5108, LoadResString(5108).Replace(ARG1, strArg));
              }
            } else {
              //set error flag
              blnError = true;
              //set arg to invalid number
              lngArg = -1;
            }
          }
        }

        //now lngArg is a valid group number, unless blnError is set

        //if there is an error
        if (blnError) {
          //if arg value=-1 OR high level,
          if (agMainLogSettings.ErrorLevel == leHigh || (lngArg == -1)) {
            //argument is already 1-based for said tests
            strErrMsg = LoadResString(4114).Replace(ARG1, strArg);
            return -1;
          } else {
            if (agMainLogSettings.ErrorLevel == leMedium) {
              //set warning
              AddWarning(5019, LoadResString(5019).Replace(ARG1, strArg));
              blnError = false;
            }
          }
        }
        //check for group 0
        if (lngArg == 0) {
          if (agMainLogSettings.ErrorLevel == leHigh) {
            strErrMsg = LoadResString(4035).Replace(ARG1, strArg);
            return -1;
          } else if (agMainLogSettings.ErrorLevel == leMedium) {
            AddWarning(5083, LoadResString(5083).Replace(ARG1, strArg));
            //} else if (agMainLogSettings.ErrorLevel == leLow) {
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
        strModFileName = JustFileName(strModule);
        //set errline
        lngErrLine = int.Parse(Mid(stlInput[lngLine], stlInput[lngLine].IndexOf(":") + 1, stlInput[lngLine].IndexOf("#") - 5));
        strCurrentLine = Right(stlInput[lngLine], stlInput[lngLine].Length - stlInput[lngLine].IndexOf("#"));
      } else {
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
        } else if (strCurrentLine[lngPos + 1] == '=') {
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
        } else if (strCurrentLine[lngPos + 1] == '=') {
          lngPos++;
          //return shorthand subtract
          retval = "-=";
        } else if (IsNumeric(strCurrentLine[lngPos + 1].ToString())) {
          //return a negative number

          //continue adding characters until non-numeric or EOL is reached
          while (lngPos + 1 <= strCurrentLine.Length) { // Do Until lngPos + 1 > Len(strCurrentLine)
            intChar = (int)strCurrentLine[lngPos + 1];
            if (intChar < 48 || intChar > 57) {
              //anything other than a digit (0-9)
              break;
            } else {
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
        } else if (strCurrentLine[lngPos + 1] == '/') {
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
        } else if (strCurrentLine[lngPos + 1] == '/') {
          lngPos++;
          retval = "//";
          //since block commands are no longer supported, check for the in order to provide a
          //meaningful error message
        } else if (strCurrentLine[lngPos + 1] == '*') {
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
          } else {
            //add character
            retval += ((char)intChar).ToString();
            //incrment position
            lngPos++;
          }
        }
      } else {
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
          } else {
            //regular char; check for slash or quote mark
            if (cval == '"') {
              //34: //quote mark
              //a quote marks end of string
              blnInQuotes = false;
            } else if (cval == '\\') {
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
        blnError = true;
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
          } else {
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
              } else {
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

            if (agMainLogSettings.ErrorLevel == leHigh) {
              //error
              blnError = true;
              strErrMsg = LoadResString(4080);
              return "";
            } else if (agMainLogSettings.ErrorLevel == leHigh) {
              //add quote
              strTextContinue += QUOTECHAR;
              //set warning
              AddWarning(5002);
            } else if (agMainLogSettings.ErrorLevel == leLow) {
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
            } else {
              //if last character was a slash, ignore this character
              //because it's part of a slash code
              if (blnSlash) {
                //always reset  the slash
                blnSlash = false;
              } else {
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
      } else {
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
      blnError = false;
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
      int i, j;
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
          rtn = ValidateDefName(tdNewDefine.Name);
          //name error 7-12  are only warnings if error level is medium or low
          if (agMainLogSettings.ErrorLevel == leMedium) {
            //check for name warnings
            switch (rtn) {
            case 7:
              //set warning
              AddWarning(5034, LoadResString(5034).Replace(ARG1, tdNewDefine.Name));
              //reset return error code
              rtn = 0;
              break;
            default:
              if (rtn >= 8 && rtn <= 12) {
                //set warning
                AddWarning(5035, LoadResString(5035).Replace(ARG1, tdNewDefine.Name));
                //reset return error code
                rtn = 0;
              }
              break;
            }
          } else if (agMainLogSettings.ErrorLevel == leLow) {
            //check for warnings
            if (rtn >= 7 && rtn <= 12) {
              //reset return error code
              rtn = 0;
            }
          }
          //check for errors
          if (rtn != 0) {
            //check for name errors
            switch (rtn) {
            case 1: // no name
              strErrMsg = LoadResString(4070);
              break;
            case 2: // name is numeric
              strErrMsg = LoadResString(4072);
              break;
            case 3: // name is command
              strErrMsg = LoadResString(4021).Replace(ARG1, tdNewDefine.Name);
              break;
            case 4: // name is test command
              strErrMsg = LoadResString(4022).Replace(ARG1, tdNewDefine.Name);
              break;
            case 5: // name is a compiler keyword
              strErrMsg = LoadResString(4013).Replace(ARG1, tdNewDefine.Name);
              break;
            case 6: // name is an argument marker
              strErrMsg = LoadResString(4071);
              break;
            case 7: // name is already globally defined
              strErrMsg = LoadResString(4019).Replace(ARG1, tdNewDefine.Name);
              break;
            case 8: // name is reserved variable name
              strErrMsg = LoadResString(4018).Replace(ARG1, tdNewDefine.Name);
              break;
            case 9: // name is reserved flag name
              strErrMsg = LoadResString(4014).Replace(ARG1, tdNewDefine.Name);
              break;
            case 10: // name is reserved number constant
              strErrMsg = LoadResString(4016).Replace(ARG1, tdNewDefine.Name);
              break;
            case 11: // name is reserved object constant
              strErrMsg = LoadResString(4017).Replace(ARG1, tdNewDefine.Name);
              break;
            case 12: // name is reserved message constant
              strErrMsg = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
              break;
            case 13: // name contains improper character
              strErrMsg = LoadResString(4067);
              break;
            }
            //don't exit; check for define Value errors first
          }

          //validate define Value
          rtn = ValidateDefValue(tdNewDefine);
          //Value errors 4,5,6 are only warnings if error level is medium or low
          if (agMainLogSettings.ErrorLevel == leMedium) {
            //if Value error is due to missing quotes
            switch (rtn) {
            case 4:  //string Value missing quotes
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
              rtn = 0;
              break;
            case 5: // Value is already defined by a reserved name
                    //set warning
              AddWarning(5032, LoadResString(5032).Replace(ARG1, tdNewDefine.Value));
              //reset error code
              rtn = 0;
              break;
            case 6: // Value is already defined by a global name
                    //set warning
              AddWarning(5031, LoadResString(5031).Replace(ARG1, tdNewDefine.Value));
              //reset error code
              rtn = 0;
              break;
            }
          } else if (agMainLogSettings.ErrorLevel == leLow) {
            //if Value error is due to missing quotes
            switch (rtn) {
            case 4:
              //fix the define Value
              if (tdNewDefine.Value[0] != '"') {
                tdNewDefine.Value = QUOTECHAR + tdNewDefine.Value;
              }
              if (tdNewDefine.Value[tdNewDefine.Value.Length - 1] != '"') {
                tdNewDefine.Value += QUOTECHAR;
              }
              //reset return Value
              rtn = 0;
              break;
            case 5:
            case 6:
              //reset return Value
              rtn = 0;
              break;
            }
          }

          //check for errors
          if (rtn != 0) {
            //if already have a name error
            if (strErrMsg.Length != 0) {
              //append Value error
              strErrMsg += "; and ";
            }

            //check for Value error
            switch (rtn) {
            case 1: // no Value
              strErrMsg += LoadResString(4073);
              break;
            //a return Value of 2 is no longer possible; this
            //Value has been removed from the ValidateDefineValue function
            //case 2: // Value is an invalid argument marker
            //  strErrMsg += "4065: Invalid argument declaration Value"
            //  break;
            case 3: // Value contains an invalid argument Value
              strErrMsg += LoadResString(4042);
              break;
            case 4: // Value is not a string, number or argument marker
              strErrMsg += LoadResString(4082);
              break;
            case 5: // Value is already defined by a reserved name
              strErrMsg += LoadResString(4041).Replace(ARG1, tdNewDefine.Value);
              break;
            case 6: // Value is already defined by a global name
              strErrMsg += LoadResString(4040).Replace(ARG1, tdNewDefine.Value);
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
                if (agMainLogSettings.ErrorLevel == leHigh) {
                  //set error
                  strErrMsg = LoadResString(4023).Replace(ARG1, tdDefines[i].Value).Replace(ARG2, tdDefines[i].Name);
                  return false;
                } else if (agMainLogSettings.ErrorLevel == leMedium) {
                  //set warning
                  AddWarning(5033, LoadResString(5033).Replace(ARG1, tdNewDefine.Value).Replace(ARG2, tdDefines[i].Name));
                  //} else if (agMainLogSettings.ErrorLevel == leLow) {
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
          } else {
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

      int intMsgNum, i;
      string strCmd, strMsgContinue;
      string strMsgSep;
      int lngMsgStart;
      bool blnDef = false;
      int intMsgLineCount;
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
            blnError = true;
            strErrMsg = LoadResString(4077);
            return false;
          }
          //validate msg number
          intMsgNum = VariableValue(strCmd);
          if (intMsgNum <= 0) {
            //error
            blnError = true;
            strErrMsg = LoadResString(4077);
            return false;
          }
          //if msg is already assigned
          if (blnMsg[intMsgNum]) {
            blnError = true;
            strErrMsg = LoadResString(4094).Replace(ARG1, (intMsgNum).ToString());
            return false;
          }
          //get next command (should be the message text)
          strCmd = NextCommand(false);
          //is this a valid string?
          if (!IsValidMsg(strCmd)) {
            //maybe it's a define
            bool nullval = false;
            if (ConvertArgument(ref strCmd, atMsg, ref nullval)) {
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
          } else {
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
              } else {
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

            if (agMainLogSettings.ErrorLevel == leHigh) {
              blnError = true;
              strErrMsg = LoadResString(4051);
              return false;
            } else { // leMedium, leLow
                     //add quotes as appropriate
              if ((lngQuotesOK & 1) == 1) {
                strCmd = QUOTECHAR + strCmd;
              }
              if ((lngQuotesOK & 2) == 2) {
                strCmd += QUOTECHAR;
              }
              //warn if medium
              if (agMainLogSettings.ErrorLevel == leMedium) {
                //set warning
                AddWarning(5002);
              }
            }
          }
          //concatenate, if necessary
          if (!blnDef) {
            strCmd = ConcatArg(strCmd);
            //if error,
            if (blnError) {
              return false;
            }
          }
          //nothing allowed after msg declaration
          if (lngPos != strCurrentLine.Length) {
            //error
            blnError = true;
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
      //warning elements are separated by pipe character
      //WarningsText is in format:
      //  number|warningtext|line|module
      //
      //(number, line and module only have meaning for logic warnings
      // other warnings generated during a game compile will use
      // same format, but use -- for warning number, line and module)

      //if no text passed, use the default resource string
      string evWarn;
      if (WarningText.Length == 0) {
        WarningText = LoadResString(WarningNum);
      }
      //only add if not ignoring
      if (!agNoCompWarn[WarningNum - 5000]) {
        evWarn = WarningNum.ToString() + "|" + WarningText + "|" + (lngErrLine + 1).ToString() + "|" + (strModule.Length != 0 ? strModule : "");
        Raise_LogicCompileEvent(evWarn, bytLogComp);
      }
    }

    static void tmp_LogCompile()
    {
      /*
        static internal bool Compileif ()
        {
      //this routine will read and validate a group of test commands
          //for an //if// statement and return
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

          string strTestCmd, strArg
          byte bytTestCmd;
      byte[] bytArg(7)
          int lngArg;
      int[] lngWord()
          int intWordCount
          int i
          bool blnIfBlock //command block, not a comment block
          bool blnNeedNextCmd
          int intNumTestCmds
          int intNumCmdsInBlock
          bool blnNOT


          On Error GoTo ErrHandler


          blnIfBlock = false
          intNumTestCmds = 0
          intNumCmdsInBlock = 0
          blnNeedNextCmd = true

          //write out starting if byte
          tmpLogRes.Writebyte 0xFF

          //next character should be "("
          if (NextChar() != "(") {
            blnError = true
            strErrMsg = LoadResString(4002)
            return;
          }

          //now, step through input, until final //)//// is found:
          Do
            //get next command
            strTestCmd = NextCommand()
            //check for end of input,
            if (lngLine == -1) {
              blnError = true
              strErrMsg = LoadResString(4106)
              return;
            }

            //if awaiting a test command,
            if (blnNeedNextCmd) {
              switch (strTestCmd) {
              case "(" //open paran
                //if already in a block
                if (blnIfBlock) {
                  blnError = true
                  strErrMsg = LoadResString(4045)
                  return;
                }
                //write //or// block start
                tmpLogRes.Writebyte 0xFC
                blnIfBlock = true
                intNumCmdsInBlock = 0
              case ")"
                //if a test command is expected, //)// always causes error
                if (intNumTestCmds == 0) {
                  strErrMsg = LoadResString(4057)
                } else if ( blnIfBlock && intNumCmdsInBlock == 0) {
                  strErrMsg = LoadResString(4044)
                } else {
                  strErrMsg = LoadResString(4056)
                }
                blnError = true
                return;
              default:
                //check for NOT
                blnNOT = (strTestCmd = NOT_TOKEN)
                if (blnNOT) {
                  tmpLogRes.Writebyte 0xFD
                  //read in next test command
                  strTestCmd = NextCommand()
                  //check for end of input,
                  if (lngLine == -1) {
                    blnError = true
                    strErrMsg = LoadResString(4106)
                    return;
                  }
                }
                bytTestCmd = CommandNum(true, strTestCmd)
                //if command not found,
                if (bytTestCmd == 255) {
                  //check for special syntax
                  if (!CompileSpecialif (strTestCmd, blnNOT)) {
                    //error; the CompileSpecialIf function
                    //sets the error codes, and CompileLogic will
                    //call the error handler
                    return;
                  }
                } else {
                  //write the test command code
                  tmpLogRes.Writebyte bytTestCmd
                  //next command should be "("
                  if (NextChar() != "(") {
                    blnError = true
                    strErrMsg = LoadResString(4048)
                    return;
                  }

                  //check for return.false() command
                  if (bytTestCmd == 0) {
                    //warn user that it's not compatible with AGI Studio
                    if (agMainLogSettings.ErrorLevel == ) {
                    case leHigh, leMedium
                      //generate warning
                      AddWarning(5081
                    case leLow
                    } 
                  }

                  //if said command
                  if (bytTestCmd == 0xE) {
                    //enable error trapping to catch invalid word
                    On Error Resume Next
                    //and word count
                    intWordCount = 0
                    ReDim lngWord(0)
                    //get first word arg
                    lngArg = GetNextArg(atVocWrd, intWordCount + 1)
                    //if error
                    if (blnError) {
                      // if error number is 4054
                      if (Val(strErrMsg) == 4054) {
                        // add command name to error string
                        strErrMsg = Replace(strErrMsg, ARG2, agTestCmds(bytTestCmd).Name)
                      }
                      //exit
                      return;
                    }

                    //loop to add this word, and any more
                    Do
                      //add this word number to array of word numbers
                      ReDim Preserve lngWord(intWordCount)
                      lngWord(intWordCount) = lngArg
                      intWordCount = intWordCount + 1
                      //if too many words
                      if (intWordCount == 10) {
                        blnError = true
                        strErrMsg = LoadResString(4093)
                        return;
                      }

                      //get next character
                      //(should be a comma, or close parenthesis, if no more words)
                      strArg = NextChar()
                      if (LenB(strArg) != 0) {
                        switch (AscW(strArg)) {
                        case 41 //)//
                          //move pointer back one space so
                          //the //)// will be found at end of command
                          lngPos = lngPos - 1
                          Exit Do


                        case 44 //,//
                          //expected; now check for next word argument
                          lngArg = GetNextArg(atVocWrd, intWordCount + 1)
                          //if error
                          if (blnError) {
                            //exit
                            // if error number is 4054
                            if (Val(strErrMsg) == 4054) {
                              // add command name to error string
                              strErrMsg = Replace(strErrMsg, ARG2, agTestCmds(bytTestCmd).Name)
                            }
                            return;
                          }


                        default:
                          //error
                          blnError = true
                          //check for added quotes; they are the problem
                          if (lngQuoteAdded >= 0) {
                            //reset line;
                            lngLine = lngQuoteAdded
                            lngErrLine = lngLine - lngIncludeOffset
                            //string error
                            strErrMsg = LoadResString(4051)
                          } else {
                            //use 1-base arg values
                            strErrMsg = Replace(LoadResString(4047), ARG1, CStr(intWordCount + 1))
                          }
                          return;
                        } 
                      } else {
                      //Debug.Assert false
                      //we should normally never get here, since changing the function to allow
                      //splitting over multiple lines, unless this is the LAST line of
                      //the logic (an EXTREMELY rare edge case)
                        //error
                        blnError = true

                        //check for added quotes; they are the problem
                        if (lngQuoteAdded >= 0) {
                          //reset line;
                          lngLine = lngQuoteAdded
                          lngErrLine = lngLine - lngIncludeOffset
                          //string error
                          strErrMsg = LoadResString(4051)
                        } else {
                          //use 1-base arg values
                          strErrMsg = Replace(LoadResString(4047), ARG1, CStr(intWordCount + 1))
                        }
                        return;
                      }
                    Loop While true

                    //reset the quotemark error flag after //)// is found
                    lngQuoteAdded = -1

                    //reset error handling
                    On Error GoTo 0

                    //need to write number of arguments for //said//
                    //before writing arguments themselves
                    tmpLogRes.Writebyte Cbyte(intWordCount)

                    //now add words
                    For i = 0 To intWordCount - 1
                      //write word Value
                      tmpLogRes.WriteWord lngWord(i)
                    Next i
                  } else {
                    //not //said//; extract arguments for this command
                    For i = 0 To agTestCmds(Cbyte(bytTestCmd)).ArgType.Length - 1
                      //after first argument, verify comma separates arguments
                      if (i > 0) {
                        if (NextChar(true) != ",") {
                          blnError = true
                          //use 1-base arg values
                          strErrMsg = Replace(LoadResString(4047), ARG1, CStr(i + 1))
                          return;
                        }
                      }

                      //reset the quotemark error flag after comma is found
                      lngQuoteAdded = -1
                      bytArg(i) = GetNextArg(agTestCmds(Cbyte(bytTestCmd)).ArgType(i), i)
                      //if error
                      if (blnError) {
                        // if error number is 4054
                        if (Val(strErrMsg) == 4054) {
                          // add command name to error string
                          strErrMsg = Replace(strErrMsg, ARG2, agTestCmds(bytTestCmd).Name)
                        }
                        return;
                      }
                      //write argument
                      tmpLogRes.Writebyte bytArg(i)
                    Next i
                  }
                  //next character should be ")"
                  if (NextChar() != ")") {
                    blnError = true
                    strErrMsg = LoadResString(4160)
                    return;
                  }
                  //reset the quotemark error flag
                  lngQuoteAdded = -1

                  //validate arguments for this command
                  if (!ValidateIfArgs(bytTestCmd, bytArg())) {
                    //error assigned by called function
                    return;
                  }
                }

                //command added
                intNumTestCmds = intNumTestCmds + 1
                //if in IF block,
                if (blnIfBlock) {
                  intNumCmdsInBlock = intNumCmdsInBlock + 1
                }
                //toggle off need for test command
                blnNeedNextCmd = false
              } 
            } else { //not awaiting a test command
              switch (strTestCmd) {
              case NOT_TOKEN
                //invalid
                blnError = true
                strErrMsg = LoadResString(4097)
                return;
              case AND_TOKEN
                //if inside brackets
                if (blnIfBlock) {
                  blnError = true
                  strErrMsg = LoadResString(4037)
                  return;
                }
                blnNeedNextCmd = true
              case OR_TOKEN
                //if NOT inside brackets
                if (!blnIfBlock) {
                  blnError = true
                  strErrMsg = LoadResString(4100)
                  return;
                }
                blnNeedNextCmd = true
              case ")"
                //if inside brackets
                if (blnIfBlock) {
                  //ensure at least one command in block,
                  if (intNumCmdsInBlock == 0) {
                    blnError = true
                    strErrMsg = LoadResString(4044)
                    return;
                  }
                  //close brackets
                  blnIfBlock = false
                  tmpLogRes.Writebyte 0xFC
                } else {
                  //ensure at least one command in block,
                  if (intNumTestCmds == 0) {
                    blnError = true
                    strErrMsg = LoadResString(4044)
                    return;
                  }
                  //end of if found
                  Exit Do
                }
              default:
                if (blnIfBlock) {
                  strErrMsg = LoadResString(4101)
                } else {
                  strErrMsg = LoadResString(4038)
                }
                blnError = true
                return;
              } 
            }
          //never leave loop normally; error, end of input, or successful
          //compilation of test commands will all exit loop directly
          Loop While true

          //write ending if byte
          tmpLogRes.Writebyte 0xFF
          //return true
          CompileIf = true
        return;

        ErrHandler:
          blnError = true
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "CompileIf")
          //err.clear
        } //endfunction






        static internal bool ValidateArgs(int CmdNum, ref byte[] ArgVal())
      {

          bool blnUnload, blnWarned

          //check for specific command issues
          On Error GoTo ErrHandler

          //for commands that can affect variable values, need to check against reserved variables
          //for commands that can affect flags, need to check against reserved flags

          //for other commands, check the passed arguments to see if values are appropriate


          switch (CmdNum) {
          case 1, 2, 4 To 8, 10, 165 To 168 //increment, decrement, assignv, addn, addv, subn, subv
                                            //rindirect, mul.n, mul.v, div.n, div.v
            //check for reserved variables that should never be manipulated
            //(assume arg Value is zero to avoid tripping other checks)
            CheckResVarUse ArgVal(0), 0

            //for div.n(vA, B) only, check for divide-by-zero
            if (CmdNum == 167) {
              if (ArgVal(1) == 0) {
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh
                  strErrMsg = LoadResString(4149)
                  return;
                case leMedium
                  AddWarning(5030
                case leLow
                } 
              }
            }


          case 3 //assignn
            //check for actual Value being assigned
            CheckResVarUse ArgVal(0), ArgVal(1)


          case 12, 13, 14 //set, reset, toggle
            //check for reserved flags
            CheckResFlagUse ArgVal(0)


          case 18 //new.room(A)
            //validate that this logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4120)
                return;
              case leMedium
                AddWarning(5053
              case leLow
              } 
            }
            //expect no more commands
            blnNewRoom = true


          case 19 //new.room.v
            //expect no more commands
            blnNewRoom = true


          case 20 //load.logics(A)
            //validate that logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4121), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5013
              case leLow
              } 
            }


          case 22  //call(A)
            //calling logic0 is a BAD idea
            if (ArgVal(0) == 0) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4118)
                return;
              case leMedium
                AddWarning(5010
              case leLow
                //no action
              } 
            }

            //recursive calling is BAD
            if (ArgVal(0) == bytLogComp) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4117)
                return;
              case leMedium
                AddWarning(5089
              case leLow
                //no action
              } 
            }

            //validate that logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4156), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5076
              case leLow
              } 
            }


          case 30 //load.view(A)
            //validate that view exists
            if (!agViews.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4122), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5015
              case leLow
              } 
            }


          case 32 //discard.view(A)
            //validate that view exists
            if (!agViews.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4123), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5024
              case leLow
              } 
            }


          case 37 //position(oA, X,Y)
            //check x/y against limits
            if (ArgVal(1) > 159 || ArgVal(2) > 167) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
               strErrMsg = LoadResString(4128)
               return;
              case leMedium
                AddWarning(5023
              case leLow
              } 
            }


          case 39 //get.posn
            //neither variable arg should be a reserved Value
            if (ArgVal(1) <= 26 || ArgVal(2) <= 26) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }


          case 41 //set.view(oA, B)
            //validate that view exists
            if (!agViews.Exists(ArgVal(1))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4124), ARG1, CStr(ArgVal(1)))
                return;
              case leMedium
                AddWarning(5037
              case leLow
              } 
            }


          case 49 To 53, 97, 118  //last.cel, current.cel, current.loop,
                                  //current.view, number.of.loops, get.room.v
                                  //get.num
            //variable arg is second
            //variable arg should not be a reserved Value
            if (ArgVal(1) <= 26) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }


          case 54 //set.priority(oA, B)
            //check priority Value
            if (ArgVal(1) > 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4125)
                return;
              case leMedium
                AddWarning(5050
              case leLow
              } 
            }


          case 57 //get.priority
            //variable is second argument
            //variable arg should not be a reserved Value
            if (ArgVal(1) <= 26) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }


          case 63 //set.horizon(A)
            //>120 or <16 is unusual
            //>=167 will cause AGI to freeze/crash

            //validate horizon Value
            if (agMainLogSettings.ErrorLevel == ) {
            case leHigh
              if (ArgVal(0) >= 167) {
                strErrMsg = LoadResString(4126)
                return;
              }
              if (ArgVal(0) > 120) {
                AddWarning(5042
              } else if ( ArgVal(0) < 16) {
                AddWarning(5041
              }


            case leMedium
              if (ArgVal(0) >= 167) {
                AddWarning(5043
              } else if ( ArgVal(0) > 120) {
                  AddWarning(5042
              } else if ( ArgVal(0) < 16) {
                AddWarning(5041
              }


            case leLow
            } 


          case 64, 65, 66 //object.on.water, object.on.land, object.on.anything
            //warn if used on ego
            if (ArgVal(0) == 0) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5082
              case leLow
              } 
            }


          case 69 //distance
            //variable is third arg
            //variable arg should not be a reserved Value
            if (ArgVal(2) <= 26) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }


          case 73, 75, 99 //end.of.loop, reverse.loop
            //flag arg should not be a reserved Value
            if (ArgVal(1) <= 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }
            //check for read only reserved flags
            CheckResFlagUse ArgVal(1)


          case 81 //move.obj(oA, X,Y,STEP,fDONE)
            //validate the target position
            if (ArgVal(1) > 159 || ArgVal(2) > 167) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4127)
                return;
              case leMedium
                AddWarning(5062
              case leLow
              } 
            }

            //check for ego object
            if (ArgVal(0) == 0) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5045
              case leLow
              } 
            }

            //flag arg should not be a reserved Value
            if (ArgVal(4) <= 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }

            //check for read only reserved flags
            CheckResFlagUse ArgVal(4)


          case 82 //move.obj.v
            //flag arg should not be a reserved Value
            if (ArgVal(4) <= 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }

            //check for read only reserved flags
            CheckResFlagUse ArgVal(4)


          case 83 //follow.ego(oA, DISTANCE, fDONE)
            //validate distance value
            if (ArgVal(1) <= 1) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5102
              case leLow
              } 
            }

            //check for ego object
            if (ArgVal(0) == 0) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5027
              case leLow
              } 
            }

            //flag arg should not be a reserved Value
            if (ArgVal(2) <= 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }
            //check for read only reserved flags
            CheckResFlagUse ArgVal(2)


          case 86 //set.dir(oA, vB)
            //check for ego object
            if (ArgVal(0) == 0) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5026
              case leLow
              } 
            }


          case 87 //get.dir
            //variable is second arg
            //variable arg should not be a reserved Value
            if (ArgVal(1) <= 26) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }


          case 90 //block(x1,y1,x2,y2)
            //validate that all are within bounds, and that x1<=x2 and y1<=y2
            //also check that
            if (ArgVal(0) > 159 || ArgVal(1) > 167 || ArgVal(2) > 159 || ArgVal(3) > 167) {
              //bad number
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4129)
                return;
              case leMedium
                AddWarning(5020
              case leLow
              } 
            }


            if (ArgVal(2) - ArgVal(0) < 2) || (ArgVal(3) - ArgVal(1) < 2)) {
              //won't work
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4129)
                return;
              case leMedium
                AddWarning(5051
              case leLow
              } 
            }



          case 98 //load.sound(A)
            //validate the sound exists
            if (!agSnds.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4130), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5014
              case leLow
              } 
            }


          case 99 //sound(A)
            //validate the sound exists
            if (!agSnds.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4137), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5084
              case leLow
              } 
            }


          case 103 //display(ROW,COL,mC)
            //check row/col against limits
            if (ArgVal(0) > 24 || ArgVal(1) > 39) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
               strErrMsg = LoadResString(4131)
               return;
              case leMedium
                AddWarning(5059
              case leLow
              } 
            }


          case 105 //clear.lines(TOP,BTM,C)
            //top must be >btm; both must be <=24
            if (ArgVal(0) > 24 || ArgVal(1) > 24 || ArgVal(0) > ArgVal(1)) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4132)
                return;
              case leMedium
                AddWarning(5011
              case leLow
              } 
            }
            //color value should be 0 or 15 //(but it doesn't hurt to be anything else)
            if (ArgVal(2) > 0 && ArgVal(2) != 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5100
              case leLow
              } 
            }


          case 109 //set.text.attribute(A,B)
            //should be limited to valid color values (0-15)
            if (ArgVal(0) > 15 || ArgVal(1) > 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4133)
                return;
              case leMedium
                AddWarning(5029
              case leLow
              } 
            }


          case 110 //shake.screen(A)
            //shouldn't normally have more than a few shakes; zero is BAD
            if (ArgVal(0) == 0) {
              //error!
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                strErrMsg = LoadResString(4134)
                return;
              case leLow
              } 
            } else if ( ArgVal(0) > 15) {
              //could be a palette change?
              if (ArgVal(0) >= 100 && ArgVal(0) <= 109) {
                //separate warning
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh, leMedium
                  AddWarning(5058
                case leLow
                } 
              } else {
                //warning
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh, leMedium
                  AddWarning(5057
                case leLow
                } 
              }
            }


          case 111 //configure.screen(TOP,INPUT,STATUS)
            //top should be <=3
            //input and status should not be equal
            //input and status should be <top or >=top+21
            if (ArgVal(0) > 3) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4135)
                return;
              case leMedium
                AddWarning(5044
              case leLow
              } 
            }
            if (ArgVal(1) > 24 || ArgVal(2) > 24) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5099
              case leLow
              } 
            }
            if (ArgVal(1) == ArgVal(2)) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5048
              case leLow
              } 
            }
            if ((ArgVal(1) >= ArgVal(0) && ArgVal(1) <= CLng(ArgVal(0)) + 20) || (ArgVal(2) >= ArgVal(0) && ArgVal(2) <= CLng(ArgVal(0)) + 20)) {
             if (agMainLogSettings.ErrorLevel == ) {

             case leHigh, leMedium
               AddWarning(5049

             case leLow

             } 

           }


         case 114 //set.string(sA, mB)
            //warn user if setting input prompt to unusually long value

           if (ArgVal(0) == 0) {
             if (Len(strMsg(ArgVal(1))) > 10) {
               if (agMainLogSettings.ErrorLevel == ) {

               case leHigh, leMedium
                 AddWarning(5096

               case leLow

               } 

             }

           }


         case 115 //get.string(sA, mB, ROW,COL,LEN)
            //if row>24, both row/col are ignored; if col>39, gets weird; len is limited automatically to <=40

           if (ArgVal(2) > 24) {
             if (agMainLogSettings.ErrorLevel == ) {

             case leHigh, leMedium
               AddWarning(5052

             case leLow

             } 

           }


           if (ArgVal(3) > 39) {
             if (agMainLogSettings.ErrorLevel == ) {

             case leHigh

               strErrMsg = LoadResString(4004)

               return;

             case leMedium

               AddWarning(5080

             case leLow

             } 

           }


           if (ArgVal(4) > 40) {
             if (agMainLogSettings.ErrorLevel == ) {

             case leHigh, leMedium
               AddWarning(5056

             case leLow

             } 

           }


         case 121 //set.key(A,B,cC)
            //controller number limit checked in GetNextArg function

            //increment controller Count

           intCtlCount = intCtlCount + 1

            //must be ascii or key code, (Arg0 can be 1 to mean joystick)

           if (ArgVal(0) > 0 && ArgVal(1) > 0 && ArgVal(0) != 1) {
             if (agMainLogSettings.ErrorLevel == ) {

             case leHigh

               strErrMsg = LoadResString(4154)

               return;

             case leMedium

               AddWarning(5065

             case leLow

             } 

           }

            //check for improper ASCII assignments

           if (ArgVal(1) == 0) {
             switch (ArgVal(0)) { //ascii codes
              case 8, 13, 32 //bkspace, enter, spacebar
                //bad
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh
                  strErrMsg = LoadResString(4155)
                  return;
                case leMedium
                  AddWarning(5066
                case leLow
                } 
              } 
            }

            //check for improper KEYCODE assignments
            if (ArgVal(0) == 0) {
              switch (ArgVal(0)) { //ascii codes
              case 71, 72, 73, 75, 76, 77, 79, 80, 81, 82, 83
                //bad
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh
                  strErrMsg = LoadResString(4155)
                  return;
                case leMedium
                  AddWarning(5066
                case leLow
                } 
              } 
            }


          case 122 //add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
            //VIEW, LOOP + CEL must exist
            //CEL width must be >=3
            //x,y must be within limits
            //PRI must be 0, or >=3 AND <=15
            //MGN must be 0-3, or >3 (ha ha, or ANY value...)

            //validate view
            if (!agViews.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4138), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5064
                //dont need to check loops or cels
                blnWarned = true
              case leLow
              } 
            }


            if (!blnWarned) {
              //try to load view to test loop + cel
              On Error Resume Next
              blnUnload = !agViews(ArgVal(0)).Loaded
              //if error trying to get loaded status, ignore for now
              //it//ll show up again when trying to load or access
              //loop property and be handled there
              Err.Clear
              if (blnUnload) {
                agViews(ArgVal(0)).Load
              }
              if (Err.Number == 0) {
                //validate loop
                if (ArgVal(1) >= agViews(ArgVal(0)).Loops.Count) {
                  if (agMainLogSettings.ErrorLevel == ) {
                  case leHigh
                    strErrMsg = Replace(Replace(LoadResString(4139), ARG1, CStr(ArgVal(1))), ARG2, CStr(ArgVal(0)))
                    if (blnUnload) {
                      agViews(ArgVal(0)).Unload
                    }
                    return;
                  case leMedium
                    AddWarning(5085
                    //dont need to check cel
                    blnWarned = true
                  case leLow
                  } 
                }
                //if loop was valid, check cel
                if (!blnWarned) {
                  //validate cel
                  if (ArgVal(2) >= agViews(ArgVal(0)).Loops(ArgVal(1)).Cels.Count) {
                    if (agMainLogSettings.ErrorLevel == ) {
                    case leHigh
                      strErrMsg = Replace(Replace(Replace(LoadResString(4140), ARG1, CStr(ArgVal(2))), ARG2, CStr(ArgVal(1))), ARG3, CStr(ArgVal(0)))
                      if (blnUnload) {
                        agViews(ArgVal(0)).Unload
                      }
                      return;
                    case leMedium
                      AddWarning(5086
                    case leLow
                    } 
                  }
                }
              } else {
                //can't load the view; add a warning
                Err.Clear
                AddWarning(5021, Replace(LoadResString(5021), ARG1, CStr(ArgVal(0)))
              }
              if (blnUnload) {
                agViews(ArgVal(0)).Unload
              }
            }

            On Error GoTo ErrHandler

            //x,y must be within limits
            if (ArgVal(3) > 159 || ArgVal(4) > 167) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4141)
                return;
              case leMedium
                AddWarning(5038
              case leLow
              } 
            }

            //PRI should be <=15
            if (ArgVal(5) > 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4142)
                return;
              case leMedium
                AddWarning(5079
              case leLow
              } 
            }

            //PRI should be 0 OR >=4 (but doesn't raise an error; only a warning)
            if (ArgVal(5) < 4 && ArgVal(5) != 0) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5079
              case leLow
              } 
            }

            //MGN values >15 will only use lower nibble
            if (ArgVal(6) > 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5101
              case leLow
              } 
            }


          case 129 //show.obj(VIEW)
            //validate view
            if (!agViews.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4144), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5061
              case leLow
              } 
            }


          case 127, 176, 178  //init.disk, hide.mouse, show.mouse
            if (agMainLogSettings.ErrorLevel == ) {
            case leHigh, leMedium
              AddWarning(5087, Replace(LoadResString(5087), ARG1, agCmds(CmdNum).Name)
            case leLow
            } 


          case 175, 179, 180 //discard.sound, fence.mouse, mouse.posn
            if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4152), ARG1, agCmds(CmdNum).Name)
                return;
              case leMedium
              AddWarning(5088, Replace(LoadResString(5088), ARG1, agCmds(CmdNum).Name)
            case leLow
            } 


          case 130 //random(LOWER,UPPER,vRESULT)
            //lower should be < upper
            if (ArgVal(0) > ArgVal(1)) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4145)
                return;
              case leMedium
                AddWarning(5054
              } 
            }

            //lower=upper means result=lower=upper
            if (ArgVal(0) == ArgVal(1)) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5106
              case leLow
              } 
            }

            //if lower=upper+1, means div by 0!
            if (ArgVal(0) == ArgVal(1) + 1) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4158)
                return;
              case leMedium
                AddWarning(5107
              } 
            }

            //variable arg should not be a reserved Value
            if (ArgVal(2) <= 26) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            }


          case 142 //script.size
            //raise warning/error if in other than logic0
            if (bytLogComp<> 0) {
              //warn
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                //set warning
                AddWarning(5039
              case leLow
                //no action
              } 
            }
            //check for absurdly low Value for script size
            if (ArgVal(0) < 10) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5009
              case leLow
              } 
            }


          case 147 //reposition.to(oA, B,C)
            //validate the new position
            if (ArgVal(1) > 159 || ArgVal(2) > 167) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4128)
                return;
              case leMedium
                AddWarning(5023
              case leLow
              } 
            }


          case 150 //trace.info(LOGIC,ROW,HEIGHT)
            //logic must exist
            //row + height must be <22
            //height must be >=2 (but interpreter checks for this error)

            //validate that logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = Replace(LoadResString(4153), ARG1, CStr(ArgVal(0)))
                return;
              case leMedium
                AddWarning(5040
              case leLow
              } 
            }
            //validate that height is not too small
            if (ArgVal(2) < 2) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5046
              case leLow
              } 
            }
            //validate size of window
            if (CLng(ArgVal(1)) + CLng(ArgVal(2)) > 23) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4146)
                return;
              case leMedium
                AddWarning(5063
              case leLow
              } 
            }


          case 151, 152 //Print.at(mA, ROW, COL, MAXWIDTH), print.at.v
            //row <=22
            //col >=2
            //maxwidth <=36
            //maxwidth=0 defaults to 30
            //maxwidth=1 crashes AGI
            //col + maxwidth <=39
            if (ArgVal(1) > 22) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4147)
                return;
              case leMedium
                AddWarning(5067
              case leLow
              } 
            }


            switch (ArgVal(3)) {
            case 0 //maxwidth=0 defaults to 30
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5105
              case leLow
              } 


            case 1 //maxwidth=1 crashes AGI
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4043)
                return;
              case leMedium
                AddWarning(5103
              case leLow
              } 


            case Is > 36 //maxwidth >36 won't work
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4043)
                return;
              case leMedium
                AddWarning(5104
              case leLow
              } 
           } 

            //col>2 and col + maxwidth <=39
            if (ArgVal(2) < 2 || CLng(ArgVal(2)) + CLng(ArgVal(3)) > 39) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4148)
                return;
              case leMedium
                AddWarning(5068
              case leLow
              } 
            }


          case 154 //clear.text.rect(R1,C1,R2,C2,COLOR)
            //if (either row argument is >24,
            //or either column argument is >39,
            //or R2 < R1 or C2 < C1,
            //the results are unpredictable
            if (ArgVal(0) > 24 || ArgVal(1) > 39 || _
               ArgVal(2) > 24 || ArgVal(3) > 39 || _
               ArgVal(2) < ArgVal(0) || ArgVal(3) < ArgVal(1)) {
              //invalid items
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4150)
                return;
              case leMedium
                //if due to pos2 < pos1
                if (ArgVal(2) < ArgVal(0) || ArgVal(3) < ArgVal(1)) {
                  AddWarning(5069
                }
                //if due to variables outside limits
                if (ArgVal(0) > 24 || ArgVal(1) > 39 || _
                   ArgVal(2) > 24 || ArgVal(3) > 39) {
                  AddWarning(5070
                }
              } 
            }

            //color value should be 0 or 15 //(but it doesn't hurt to be anything else)
            if (ArgVal(4) > 0 && ArgVal(4) != 15) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5100
              case leLow
              } 
            }


          case 158 //submit.menu()
            //should only be called in logic0
            //raise warning/error if in other than logic0
            if (bytLogComp != 0) {
              //warn
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                //set warning
                AddWarning(5047
              case leLow
              } 
            }


          case 174 //set.pri.base(A)
            //calling set.pri.base with Value >167 doesn't make sense
            if (ArgVal(0) > 167) {
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh, leMedium
                AddWarning(5071
              case leLow
              } 
            }
          } 

          //success
          ValidateArgs = true
        return;

        ErrHandler:
          //Debug.Assert false
          Resume Next
        } //endfunction

        static internal bool ValidateIfArgs(int CmdNum, ref byte ArgVal())
      {
          //check for specific command issues


          switch (CmdNum) {
          case 9 //has (iA)
            //invobj number validated in GetNextArg function


          case 10 //obj.in.room(iA, vB)
            //invobj number validated in GetNextArg function

          case 11, 16, 17, 18 //posn(oA, X1, Y1, X2, Y2)
                              //obj.in.box(oA, X1, Y1, X2, Y2)
                              //center.posn(oA, X1, Y1, X2, Y2)
                              //right.posn(oA, X1, Y1, X2, Y2)

            //screenobj number validated in GetNextArg function

            //validate that all are within bounds, and that x1<=x2 and y1<=y2
            if (ArgVal(1) > 159 || ArgVal(2) > 167 || ArgVal(3) > 159 || ArgVal(4) > 167) {
              //bad number
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4151)
                return;
              case leMedium
                AddWarning(5072
              case leLow
              } 
            }


            if (ArgVal(1) > ArgVal(3)) || (ArgVal(2) > ArgVal(4))) {
              //can't work
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4151)
                return;
              case leMedium
                AddWarning(5073
              case leLow
              } 
            }


          case 12 //controller (cA)
            //has controller been assigned?
            //not sure how to check it; calls to controller cmd may
            //occur in logics that are compiled before the logic that sets
            //them up...

          case 14 //said()
          case 15 //compare.strings(sA, sB)

          } 

          //success
          ValidateIfArgs = true
        } //endfunction

        static internal bool ValidateMsgChars(string strMsg, int MsgNum)
      {
          //raise error/warning, depending on setting

          //return TRUE if OK or only a warning;  FALSE means error found


          int i
          bool blnWarn5093, blnWarn5094

          //if LOW errdetection, EXIT
          if (agMainLogSettings.ErrorLevel == leLow) {
            ValidateMsgChars = true
            return;
          }


          For i = 1 To Len(strMsg)
            //check for invalid codes (0,8,9,10,13)
            switch (AscW(Mid(strMsg, i, 1))) {
            case 0, 8, 9, 10, 13
              //warn user
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4005)
                blnError = true
                return;
              case leMedium
                if (!blnWarn5093) {
                  AddWarning(5093
                  blnWarn5093 = true
                  //need to track warning in case this msg is
                  //also included in body of logic
                  intMsgWarn(MsgNum) = intMsgWarn(MsgNum) || 1
                }
              } 

            //extended character
            case Is > 127
              if (agMainLogSettings.ErrorLevel == ) {
              case leHigh
                strErrMsg = LoadResString(4006)
                blnError = true
                return;
              case leMedium
                if (!blnWarn5094) {
                  AddWarning(5094
                  blnWarn5094 = true
                  //need to track warning in case this msg is
                  //also included in body of logic
                  intMsgWarn(MsgNum) = intMsgWarn(MsgNum) || 2
                }
              } 
            } 
           Next i

           //msg is OK
           ValidateMsgChars = true
        } //endfunction

        static internal bool WriteMsgs()
        {
      //this function will write the messages for a logic at the end of
          //the resource.
          //messages are encrypted with the string //Avis Durgan//. No gaps
          //are allowed, so messages that are skipped must be included as
          //zero length messages

          int lngMsgSecStart
          int lngMsgSecLen
          int[] lngMsgPos(255)
          int intCharPos
      byte bytCharVal
          int lngMsg
          int lngMsgCount
          int lngCryptStart
          int lngMsgLen
          int i
      string strHex
          bool blnSkipNull, blnSkipChar


          On Error GoTo ErrHandler

          //calculate start of message section
          lngMsgSecStart = tmpLogRes.Size

          //find last message by counting backwards until a msg is found
          lngMsgCount = 256
          Do
            lngMsgCount = lngMsgCount - 1
          Loop Until blnMsg(lngMsgCount) || (lngMsgCount = 0)

          //write msg Count,
          tmpLogRes.Writebyte Cbyte(lngMsgCount)

          //write place holder for msg end
          tmpLogRes.WriteWord 0&
          //write place holders for msg pointers
          For i = 1 To lngMsgCount
            tmpLogRes.WriteWord 0&
          Next i

          //begin encryption process
          lngCryptStart = tmpLogRes.Size
          For lngMsg = 1 To lngMsgCount

            //always reset the //NoNull// feature
            blnSkipNull = false

            //get length
            lngMsgLen = Len(strMsg(lngMsg))
            //if msg not used
            if (blnMsg(lngMsg)) {
              //calculate offset to start of this message (adjust by one byte, which
              //is the byte that indicates how many msgs there are)
              lngMsgPos(lngMsg) = tmpLogRes.GetPos - (lngMsgSecStart + 1)
            } else {
              //Debug.Assert strMsg(lngMsg) = ""
              //need to write a null value for offset; (when it gets added after all
              //messages are written it gets set to the beginning of message section
              // ( a relative offset of zero here)
              lngMsgPos(lngMsg) = 0
            }
            if (lngMsgLen > 0) {
              //step through all characters in this msg
              intCharPos = 1
              Do Until intCharPos > Len(strMsg(lngMsg))
                //get ascii code for this character
                bytCharVal = Asc(Mid(strMsg(lngMsg), intCharPos))
                //check for invalid codes (8,9,10,13)
                switch (bytCharVal) {
                case 0, 8, 9, 10, 13
                  //convert these chars to space to avoid trouble
                  bytCharVal = 32

                case 92 //"\"
                  //check for special codes
                  if (intCharPos<lngMsgLen)) {
                 switch (AscW(Mid(strMsg(lngMsg), intCharPos + 1))) {
                    case 110, 78 //n or N//
                      // \n = new line
                      bytCharVal = 0xA
                      intCharPos = intCharPos + 1


                    case 34 //dbl quote(")//
                      //\" = quote mark (chr$(34))
                      bytCharVal = 0x22
                      intCharPos = intCharPos + 1


                    case 92 //\//
                      //\\ = \
                      bytCharVal = 0x5C
                      intCharPos = intCharPos + 1


                    case 48 //0//
                      //\0 = don't add null terminator
                      blnSkipNull = true
                      //also skip this char
                      blnSkipChar = true
                      intCharPos = intCharPos + 1


                    case 120 //x//  //look for a hex value
                      //make sure at least two more characters
                      if (intCharPos + 2 < lngMsgLen) {
                        //get next 2 chars and hexify them
                        strHex = "0x" + Mid(strMsg(lngMsg), intCharPos + 2, 2)

                        //if this hex value >=1 and <256, use it
                        i = Val(strHex)
                        if (i >= 1 && i< 256) {
                          bytCharVal = i
                          intCharPos = intCharPos + 3
                        }
                      }
                    default:
                      //if no special char found, the single slash should be dropped
                      blnSkipChar = true
                    } 
                  } else {
                    //if the //\// is the last char, skip it
                    blnSkipChar = true
                  }
                } 

                //write the encrypted byte (need to adjust for previous messages, and current position)
                if (!blnSkipChar) {
                  tmpLogRes.Writebyte bytCharVal Xor bytEncryptKey((tmpLogRes.GetPos - lngCryptStart) Mod 11)
                }
                //increment pointer
                intCharPos = intCharPos + 1
                //reset skip flag
                blnSkipChar = false
              Loop
            }

            //if msg was used, add trailing zero to terminate message
            //(if msg was zero length, we still need this terminator)
            if (blnMsg(lngMsg)) {
              if (!blnSkipNull) {
                tmpLogRes.Writebyte 0x0 Xor bytEncryptKey((tmpLogRes.GetPos - lngCryptStart) Mod 11)
              }
            }
          Next lngMsg

          //calculate length of msg section, and write it at beginning
          //of msg section (adjust by one byte, which is the
          //byte that indicates number of msgs written)
          lngMsgSecLen = tmpLogRes.GetPos - (lngMsgSecStart + 1)
          tmpLogRes.WriteWord lngMsgSecLen, lngMsgSecStart + 1

          //write msg section start Value at beginning of resource
          //(correct by two so it gives position relative to byte 7 of
          //the logic resource header - see procedure //DecodeLogic// for details)
          tmpLogRes.WriteWord lngMsgSecStart - 2, 0

          //write all the msg pointers
          //
          For lngMsg = 1 To lngMsgCount
            tmpLogRes.WriteWord lngMsgPos(lngMsg), lngMsgSecStart + 1 + lngMsg* 2
          Next lngMsg

          //and return true
          WriteMsgs = true

        return;

        ErrHandler:
          //any errors, means there is a problem
          blnError = true
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "WriteMsgs")
          Err.Clear
          WriteMsgs = false
        } //endfunction



        static internal bool ReadLabels()
      {
          byte i
          int intLabel
          string strLabel
          int rtn


          On Error GoTo ErrHandler
          //this function steps through the source code to identify all valid labels; we need to find
          //them all before starting the compile so we can correctly set jumps
          //
          //valid syntax is either //label:// or //:label://, with nothing else in front of or after
          //the label declaration

          //reset counter
          bytLabelCount = 0

          //reset compiler to first input line
          ResetCompiler

          Do
            //look for label name
            if (InStr(1, strCurrentLine, ":") != 0) {
              strLabel = Trim$(Replace(strCurrentLine, vbTab, " "))
              //check for //label://
              if (Right(strLabel, 1) == ":") {
                strLabel = RTrim(Left(strLabel, Len(strLabel) - 1))
              } else if ( Left(strLabel, 1) == ":") {
                strLabel = LTrim(Right(strLabel, Len(strLabel) - 1))
              } else {
                //not a label
                strLabel = ""
              }

              //if a label was found, validate it
              if (Len(strLabel) != 0) {
                //make sure enough room
                if (bytLabelCount >= MAX_LABELS) {
                  strErrMsg = Replace(LoadResString(4109), ARG1, CStr(MAX_LABELS))
                  return;
                }


                rtn = ValidateDefName(strLabel)
                //numbers are ok for labels
                if (rtn == 2) {
                  rtn = 0
                }
                if (rtn<> 0) {
                  //error
                  switch (rtn) {
                  case 1
                    strErrMsg = LoadResString(4096)
                  case 3
                    strErrMsg = Replace(LoadResString(4025), ARG1, strLabel)
                  case 4
                    strErrMsg = Replace(LoadResString(4026), ARG1, strLabel)
                  case 5
                    strErrMsg = Replace(LoadResString(4028), ARG1, strLabel)
                  case 6
                    strErrMsg = LoadResString(4091)
                  case 7
                    strErrMsg = Replace(LoadResString(4024), ARG1, strLabel)
                  case 8
                    strErrMsg = Replace(LoadResString(4033), ARG1, strLabel)
                  case 9
                    strErrMsg = Replace(LoadResString(4030), ARG1, strLabel)
                  case 10
                    strErrMsg = Replace(LoadResString(4029), ARG1, strLabel)
                  case 11
                    strErrMsg = Replace(LoadResString(4032), ARG1, strLabel)
                  case 12
                    strErrMsg = Replace(LoadResString(4031), ARG1, strLabel)
                  case 13
                    strErrMsg = LoadResString(4068)
                  } 
                  return;
                }

                //no periods allowed either
                if (InStr(1, strLabel, ".") != 0) {
                  strErrMsg = LoadResString(4068)
                  return;
                }

                //check label against current list of labels
                if (bytLabelCount > 0) {
                  For i = 1 To bytLabelCount
                    if (strLabel == llLabel(i).Name) {
                      strErrMsg = Replace(LoadResString(4027), ARG1, strLabel)
                      return;
                    }
                  Next i
                }

                //increment number of labels, and save
                bytLabelCount = bytLabelCount + 1
                llLabel(bytLabelCount).Name = strLabel
                llLabel(bytLabelCount).Loc = 0
              }
            }

            //get next line
            IncrementLine
          Loop Until lngLine = -1
          ReadLabels = true
        return;
        ErrHandler:
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG1, "ReadLabels")
          Err.Clear
        } //endfunction
        static internal bool CompileAGI()
        {
      //main compiler function
          //steps through input one command at a time and converts it
          //to AGI logic code
          //Note that we don't need to set blnError flag here;
          //an error will cause this function to return a Value of false
          //which causes the compiler to display error info

          const MaxGotos = 255

          string strNextCmd
          string strPrevCmd
          string strArg
      byte[] bytArg(7)
          int i
          int intCmdNum
          int[] BlockStartDataLoc(MAX_BLOCK_DEPTH)
          int BlockDepth
          bool[] BlockIsif (MAX_BLOCK_DEPTH)
          int[] BlockLength(MAX_BLOCK_DEPTH)
          int CurLabel
          int intLabelNum
          LogicGoto Gotos(MaxGotos)
          int NumGotos
          int GotoData
          int CurGoto
          bool blnLastCmdRtn
          int lngReturnLine


          On Error GoTo ErrHandler

          //initialize variables
          BlockDepth = 0
          NumGotos = 0
          //reset compiler
          ResetCompiler

          blnError = false

          //get first command
          strNextCmd = NextCommand()

          //process commands in the input string list until finished
          Do Until lngLine = -1
            //reset last command flag
            blnLastCmdRtn = false
            lngReturnLine = 0

            //process the command
            switch (strNextCmd) {
            case "{"
              //can't have a "{" command, unless it follows an //if// or //else//
              if (strPrevCmd<> "if" && strPrevCmd != "else") {
                strErrMsg = LoadResString(4008)
                return;
              }


            case "}"
              //if no block currently open,
              if (BlockDepth == 0) {
                strErrMsg = LoadResString(4010)
                return;
              }
              //if last command was a new.room command, then closing block is expected
              if (blnNewRoom) {
                blnNewRoom = false
              }
              //if last position in resource is two bytes from start of block
              if (tmpLogRes.Size == BlockStartDataLoc(BlockDepth) + 2) {
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh
                  strErrMsg = LoadResString(4049)
                  return;
                case leMedium
                  //set warning
                  AddWarning(5001
                case leLow
                  //no action
                } 
              }
              //calculate and write block length
              BlockLength(BlockDepth) = tmpLogRes.Size - BlockStartDataLoc(BlockDepth) - 2
              tmpLogRes.WriteWord CLng(BlockLength(BlockDepth)), CLng(BlockStartDataLoc(BlockDepth))
              //remove block from stack
              BlockDepth = BlockDepth - 1
            case "if"
              //compile the //if// statement
              if (!Compileif ()) {
                return;
              }
              //if block stack exceeded
              if (BlockDepth >= MAX_BLOCK_DEPTH) {
                strErrMsg = Replace(LoadResString(4110), ARG1, CStr(MAX_BLOCK_DEPTH))
                return;
              }
              //add block to stack
              BlockDepth = BlockDepth + 1
              BlockStartDataLoc(BlockDepth) = tmpLogRes.GetPos
              BlockIsif (BlockDepth) = true
              //write placeholders for block length
              tmpLogRes.WriteWord 0x0

              //next command better be a bracket
              strNextCmd = NextCommand()
              if (strNextCmd<> "{") {
                //error!!!!
                strErrMsg = LoadResString(4053)
                return;
              }


            case "else"
              //else can only follow a close bracket
              if (strPrevCmd != "}") {
                strErrMsg = LoadResString(4011)
                return;
              }

              //if the block closed by that bracket was an //else//
              //(which will be determined by having that block//s IsIf flag NOT being set),
              if (!BlockIsif (BlockDepth + 1)) {
                strErrMsg = LoadResString(4083)
                return;
              }

              //adjust blockdepth to the //if// command
              //directly before this //else//
              BlockDepth = BlockDepth + 1
              //adjust previous block length to accomodate the //else// statement
              BlockLength(BlockDepth) = BlockLength(BlockDepth) + 3
              tmpLogRes.WriteWord CLng(BlockLength(BlockDepth)), CLng(BlockStartDataLoc(BlockDepth))
              //previous //if// block is now closed; use same block level
              //for this //else// block
              BlockIsif (BlockDepth) = false
              //write the //else// code
              tmpLogRes.Writebyte 0xFE
              BlockStartDataLoc(BlockDepth) = tmpLogRes.GetPos
              tmpLogRes.WriteWord 0x0  // block length filled in later.

              //next command better be a bracket
              strNextCmd = NextCommand()
              if (strNextCmd != "{") {
                //error!!!!
                strErrMsg = LoadResString(4053)
                return;
              }


            case "goto"
              //if last command was a new room, warn user
              if (blnNewRoom) {
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh, leMedium
                  //set warning
                  AddWarning(5095
                case leLow
                  //no action
                } 
                blnNewRoom = false
              }

              //next command should be "("
              if (NextChar() != "(") {
                strErrMsg = LoadResString(4001)
                return;
              }
              //get goto argument
              strArg = NextCommand()

              //if argument is NOT a valid label
              if (LabelNum(strArg) == 0) {
                strErrMsg = Replace(LoadResString(4074), ARG1, strArg)
                return;
              }
              //if too many gotos
              if (NumGotos >= MaxGotos) {
                strErrMsg = Replace(LoadResString(4108), ARG1, CStr(MaxGotos))
              }
              //save this goto info on goto stack
              NumGotos = NumGotos + 1
              Gotos(NumGotos).LabelNum = LabelNum(strArg)
              //write goto command byte
              tmpLogRes.Writebyte 0xFE
              Gotos(NumGotos).DataLoc = tmpLogRes.GetPos
              //write placeholder for amount of offset
              tmpLogRes.WriteWord 0x0
              //next character should be ")"
              if (NextChar() != ")") {
                strErrMsg = LoadResString(4003)
                return;
              }
              //verify next command is end of line (;)
              if (NextChar() != ";") {
                blnError = true
                strErrMsg = LoadResString(4007)
                return;
              }

              //since block commands are no longer supported, check for markers in order to provide a
              //meaningful error message
            case "/*", "* /"
              blnError = true
              strErrMsg = LoadResString(4052)
              return;


            case "++", "--" //unary operators; need to get a variable next
              //write the command code
              if (strNextCmd == "++") {
                tmpLogRes.Writebyte 1
              } else {
                tmpLogRes.Writebyte 2
              }

              //get the variable to update
              strArg = NextCommand()
              //convert it
              if (!ConvertArgument(strArg, atVar, false)) {
                //error
                blnError = true
                //Debug.Assert false
                strErrMsg = LoadResString(4046)
                return;
              }
              //get Value
              intCmdNum = VariableValue(strArg)
              if (intCmdNum == -1) {
                blnError = true
                //Debug.Assert false
                strErrMsg = Replace(LoadResString(4066), "%1", "")
                return;
              }
              //write the variable value
              tmpLogRes.Writebyte Cbyte(intCmdNum)
              //verify next command is end of line (;)
              if (NextChar(true) != ";") {
                strErrMsg = LoadResString(4007)
                return;
              }


            case ":"  //alternate label syntax
              //get next command; it should be the label
              strNextCmd = NextCommand()
              intLabelNum = LabelNum(strNextCmd)
              //if not a valid label
              if (intLabelNum == 0) {
                strErrMsg = LoadResString(4076)
                return;
              }
              //save position of label
              llLabel(intLabelNum).Loc = tmpLogRes.Size


            default:
              //must be a label, command, or special syntax
              //if next character is a colon
              if (strCurrentLine[lngPos + 1] == ":") {
                //it's a label
                intLabelNum = LabelNum(strNextCmd)
                //if not a valid label
                if (intLabelNum == 0) {
                  strErrMsg = LoadResString(4076)
                  return;
                }
                //save position of label
                llLabel(intLabelNum).Loc = tmpLogRes.Size
                //read in next char to skip past the colon
                NextChar
              } else {
                //if last command was a new room (and not followed by return(), warn user
                if (blnNewRoom && strNextCmd<> "return") {
                  if (agMainLogSettings.ErrorLevel == ) {
                  case leHigh, leMedium
                    //set warning
                    AddWarning(5095
                  case leLow
                    //no action
                  } 
                  blnNewRoom = false
                }

                //get number of command
                intCmdNum = CommandNum(false, strNextCmd)
                //if invalid version
                if (intCmdNum == 254) {
                  //raise error
                  strErrMsg = Replace(LoadResString(4065), ARG1, strNextCmd)
                  return;
                //if command not found,
                } else if ( intCmdNum == 255) {  // not found
                  //try to parse special syntax
                  if (CompileSpecial(strNextCmd)) {
                    //check for error
                    if (blnError) {
                      return;
                    }
                  } else {
                    //unknown command
                    strErrMsg = Replace(LoadResString(4116), ARG1, strNextCmd)
                    return;
                  }
                } else {
                  //write the command code,
                  tmpLogRes.Writebyte Cbyte(intCmdNum)
                  //next character should be "("
                  if (NextChar() != "(") {
                    strErrMsg = LoadResString(4048)
                    return;
                  }

                  //reset the quotemark error flag
                  lngQuoteAdded = -1

                  //now extract arguments,
                  For i = 0 To agCmds(Cbyte(intCmdNum)).ArgType.Length - 1
                    //after first argument, verify comma separates arguments
                    if (i > 0) {
                      if (NextChar(true) != ",") {
                        //check for added quotes; they are the problem
                        if (lngQuoteAdded >= 0) {
                          //reset line;
                          lngLine = lngQuoteAdded
                          lngErrLine = lngLine - lngIncludeOffset
                          //string error
                          strErrMsg = LoadResString(4051)
                        } else {
                          //use 1-base arg values
                          strErrMsg = Replace(LoadResString(4047), ARG1, CStr(i + 1))
                        }
                        return;
                      }
                    }
                    bytArg(i) = GetNextArg(agCmds(Cbyte(intCmdNum)).ArgType(i), i)
                    //if error
                    if (blnError) {
                      // if error number is 4054
                      if (Val(strErrMsg) == 4054) {
                        // add command name to error string
                        strErrMsg = Replace(strErrMsg, ARG2, agCmds(intCmdNum).Name)
                      }
                      return;
                    }

                    //write argument
                    tmpLogRes.Writebyte bytArg(i)
                  Next i

                  //validate arguments for this command
                  if (!ValidateArgs(intCmdNum, bytArg())) {
                    return;
                  }

                  //next character must be ")"
                  if (NextChar() != ")") {
                    blnError = true
                    //check for added quotes; they are the problem
                    if (lngQuoteAdded >= 0) {
                      //reset line;
                      lngLine = lngQuoteAdded
                      lngErrLine = lngLine - lngIncludeOffset
                      //string error
                      strErrMsg = LoadResString(4051)
                    } else {
                      strErrMsg = LoadResString(4160)
                    }
                    return;
                  }
                  if (intCmdNum == 0) {
                    blnLastCmdRtn = true
                    //set line number
                    if (lngReturnLine == 0) {
                      lngReturnLine = lngLine + 1
                    }
                  }
                }

                //verify next command is end of line (;)
                if (NextChar(true) != ";") {
                  strErrMsg = LoadResString(4007)
                  return;
                }
              }
             } 
            //get next command
            strPrevCmd = strNextCmd
            strNextCmd = NextCommand()
         Loop

          if (!blnLastCmdRtn)) {
           if (agMainLogSettings.ErrorLevel == ) {

           case leHigh
              //no rtn error

             strErrMsg = LoadResString(4102)

             return;

           case leMedium
              //add a return code

             tmpLogRes.Writebyte 0

           case leLow
              //add a return code

             tmpLogRes.Writebyte 0
              //and a warning

             AddWarning(5016

           } 

         }

          //check to see if everything was wrapped up properly

         if (BlockDepth > 0) {
           strErrMsg = LoadResString(4009)
            //reset errorline to return cmd

           lngErrLine = lngReturnLine

           return;

         }

          //write in goto values

         For CurGoto = 1 To NumGotos

           GotoData = llLabel(Gotos(CurGoto).LabelNum).Loc - Gotos(CurGoto).DataLoc - 2

           if (GotoData < 0) {
              //need to convert it to an unsigned integer Value

             GotoData = 0x10000 + GotoData

           }

           tmpLogRes.WriteWord CLng(GotoData), CLng(Gotos(CurGoto).DataLoc)
          Next CurGoto

          //return true
          CompileAGI = true
        return;

        ErrHandler:
          //if error is an app specific error, just pass it along; otherwise create
          //an app specific error to encapsulate whatever happened
          strError = Err.Description
          strErrSrc = Err.Source
          lngError = Err.Number
          if (lngError && WINAGI_ERR) = WINAGI_ERR) {
            //pass it along
            throw new Exception("lngError, strErrSrc, strError
          } else {
            throw new Exception("658, strErrSrc, Replace(LoadResString(658), ARG1, CStr(lngError) + ":" + strError)
          }
        } //endfunction

        static internal int MessageNum(string strMsgIn)
        {
      // Returns the number of the message corresponding to
          //strMsg, or creates a new msg number if strMsg is not
          //currently a message
          //if maximum number of msgs assigned, returns  0
          int lngMsg

          //blank msgs normally not allowed
          if (LenB(strMsgIn) == 0) {
            if (agMainLogSettings.ErrorLevel == ) {
            case leHigh, leMedium
              AddWarning(5074
            case leLow
              //allow it
            } 
          }


          For lngMsg = 1 To 255
            //if this is the message
            //(use StrComp, since this is a case-sensitive search)
            if (StrComp(strMsg(lngMsg), strMsgIn, vbBinaryCompare) == 0) {
              //return this Value
              MessageNum = lngMsg
              //if null string found for first time, msg-in-use flag will be false
              if (!blnMsg(lngMsg)) {
                blnMsg(lngMsg) = true
              }
              //if this msg has an extended char warning, repeat it here
              if (intMsgWarn(lngMsg) && 1) = 1) {
               AddWarning(5093
              }
              if (intMsgWarn(lngMsg) && 2) = 2) {
               AddWarning(5094
              }
              return;
            }
          Next lngMsg

          //msg doesn't exist; find an empty spot
          For lngMsg = 1 To 255
            if (!blnMsg(lngMsg)) {
              //this message is available
              blnMsg(lngMsg) = true
              strMsg(lngMsg) = strMsgIn

              //check for invalid characters
              if (!ValidateMsgChars(strMsgIn, lngMsg)) {
                //return a value to indicate error
                MessageNum = -1
              } else {
                MessageNum = lngMsg
              }


              return;
            }
          Next lngMsg

          //if no room found, return zero
          MessageNum = 0
        } //endfunction

        static internal byte CommandNum(bool blnIF, string strCmdName)
        {  //gets the command number
          //of a command, based on the text

          if (blnIF) {
            For CommandNum = 0 To agNumTestCmds
              if (strCmdName == agTestCmds(CommandNum).Name) {
                return;
              }
            Next CommandNum
          } else {
            For CommandNum = 0 To agNumCmds
              if (strCmdName == agCmds(CommandNum).Name) {
                return;
              }
            Next CommandNum
            //maybe the command is a valid agi command, but
            //just not supported in this agi version
            For CommandNum = agNumCmds + 1 To 182
              if (strCmdName == agCmds(CommandNum).Name) {
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh
                  //error; return cmd Value of 254 so compiler knows to raise error
                  CommandNum = 254
                case leMedium
                  //add warning
                  AddWarning(5075, Replace(LoadResString(5075), ARG1, strCmdName)
                case leLow
                  //don't worry about command validity; return the extracted command num
                } 


                return;
              }
            Next CommandNum
          }

          CommandNum = 255
        } //endfunction

        static internal bool CompileSpecialif (string strArg1, bool blnNOT)
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


          string strArg2
          int intArg1, intArg2
          bool blnArg2Var
          bool blnAddNOT
          byte bytCmdNum


          On Error GoTo ErrHandler

          //check for variable argument
          if (!ConvertArgument(strArg1, atVar)) {
            //check for flag argument
            if (!ConvertArgument(strArg1, atFlag)) {
              //invalid argument
              blnError = true
              strErrMsg = Replace(LoadResString(4039), ARG1, strArg1)
              return;
            }
          }

          //arg in can only be f# or v#
          switch (Left(strArg1, 1)) {
          case "f"
            //get flag argument Value
            intArg1 = VariableValue(strArg1)
            //if invalid flag number
            if (intArg1 == -1) {
              //invalid number
                blnError = true
                strErrMsg = Replace(LoadResString(4066), ARG1, "1")
              return;
            }
            //write isset cmd
            tmpLogRes.Writebyte 0x7  // isset
            tmpLogRes.Writebyte Cbyte(intArg1)


          case "v"
            //arg 1 must be //v#// format
            intArg1 = VariableValue(strArg1)

            //if invalid variable number
            if (intArg1 == -1) {
              //invalid number
                blnError = true
                strErrMsg = LoadResString(4086)
              return;
            }

            //get comparison expression
            strArg2 = NextCommand()
            //get command code for this expression
            switch (strArg2) {
            case EQUAL_TOKEN
              bytCmdNum = 0x1
            case NOTEQUAL_TOKEN
              bytCmdNum = 0x1
              blnAddNOT = true
            case ">"
              bytCmdNum = 0x5
            case "<=", "=<"
              bytCmdNum = 0x5
              blnAddNOT = true
            case "<"
              bytCmdNum = 0x3
            case ">=", "=>"
              bytCmdNum = 0x3
              blnAddNOT = true
            case ")", "&&", "||"
              //means we are doing a boolean test of the variable;
              //use greatern with zero as arg

              //write command, and arguments
              tmpLogRes.Writebyte 0x5
              tmpLogRes.Writebyte Cbyte(intArg1)
              tmpLogRes.Writebyte Cbyte(0)

              //backup the compiler pos so we get the next command properly
              lngPos = lngPos - Len(strArg2)
              //return true
              CompileSpecialIf = true
              return;

            default:
              blnError = true
              strErrMsg = LoadResString(4078)
              return;
            } 

            //before getting second arg, check for NOT symbol in front of a variable
            //can't have a NOT in front of variable comparisons
            if (blnNOT) {
              blnError = true
              strErrMsg = LoadResString(4098)
              return;
            }

            //get second argument (numerical or variable)
            blnArg2Var = true
            //reset the quotemark error flag
            lngQuoteAdded = -1
            intArg2 = GetNextArg(atNum, -1, blnArg2Var)
            //if error
            if (blnError) {
              //if an invalid arg value found
              if (Val(strErrMsg) == 4063) {
                //change error message
                strErrMsg = Mid(strErrMsg, 55, InStrRev(strErrMsg, "//") - 53)
                strErrMsg = Replace(LoadResString(4089), ARG1, strErrMsg)
              } else {
                strErrMsg = Replace(LoadResString(4089), ARG1, "")
              }
              return;
            }

            //if comparing to a variable,
            if (blnArg2Var) {
              bytCmdNum = bytCmdNum + 1
            }

            //if adding a //not//
            if (blnAddNOT) {
              tmpLogRes.Writebyte(0xFD)
            }

            //write command, and arguments
            tmpLogRes.Writebyte bytCmdNum
            tmpLogRes.Writebyte Cbyte(intArg1)
            tmpLogRes.Writebyte Cbyte(intArg2)
          } 

          //return true
          CompileSpecialIf = true
        return;

        ErrHandler:
          //Debug.Assert false
          Resume Next
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


          string strArg1, strArg2
          int intArg1, intArg2
          bool blnArg2Var 
          int intDir //0 = no indirection; 1 = left; 2 = right
          byte bytCmd;
      byte[] bytArgs


          On Error GoTo ErrHandler


          strArg1 = strArgIn

          //assume this is a special syntax until proven otherwise
          CompileSpecial = true

          //if this is indirection
          if (Left(strArg1, 1) == "*") {
            //left indirection
            //     *v# = #
            //     *v# = v#

            //next char can't be a space, newline, or tab
            switch (strCurrentLine[lngPos + 1]) {
            case " ", vbTab, ""
              //error
              blnError = true
              strErrMsg = LoadResString(4105)
              return;
            } 

            //get actual first arg
            intArg1 = GetNextArg(atVar, -1)
            //if error
            if (blnError) {
              //adjust error message
              strErrMsg = LoadResString(4064)
              return;
            }


            intDir = 1
            //next character must be "="
            strArg2 = NextCommand()
            if (strArg2<> "=") {
              //error
              blnError = true
              strErrMsg = LoadResString(4105)
              return;
            }

          //if this arg is string
          } else if ( ConvertArgument(strArg1, atStr)) {
            //string assignment
            //     s# = m#
            //     s# = "<string>"

            //get string variable number
            intArg1 = VariableValue(strArg1)


            if (agMainLogSettings.ErrorLevel<> leLow) {
              //for version 2.089, 2.272, and 3.002149 only 12 strings
              switch (agIntVersion) {
              case "2.089", "2.272", "3.002149"
                if (intArg1 > 11) {
                  if (agMainLogSettings.ErrorLevel == ) {
                  case leHigh
                    //use 1-based arg values
                    strErrMsg = Replace(Replace(LoadResString(4079), ARG1, "1"), ARG2, "11")
                  case leMedium
                    AddWarning(5007, Replace(LoadResString(5007), ARG1, "11")
                  } 
                }

              //for all other versions, limit is 24 strings
              default:
                if (intArg1 > 23) {
                  if (agMainLogSettings.ErrorLevel == ) {
                  case leHigh
                    strErrMsg = Replace(Replace(LoadResString(4079), ARG1, "1"), ARG2, "23")
                  case leMedium
                    AddWarning(5007, Replace(LoadResString(5007), ARG1, "23")
                  } 
                }
              } 
            }

            //check for equal sign
            strArg2 = NextCommand()
            //if not equal sign
            if (strArg2<> "=") {
              //error
              blnError = true
              strErrMsg = LoadResString(4034)
              return;
            }
            //get actual second variable
            //use argument extractor in case
            //second variable is a literal string)
            intArg2 = GetNextArg(atMsg, -1)
            //if error
            if (blnError) {
              // if error number is 4054
              if (Val(strErrMsg) == 4054) {
                // change it to 4058
                strErrMsg = LoadResString(4058)
              }

              //just exit
              return;
            }

            //write set.string cmd
            bytCmd = 0x72

          //if this is a variable
          } else if ( ConvertArgument(strArg1, atVar)) {
            //arg 1 must be //v#// format
            intArg1 = VariableValue(strArg1)

            //if invalid variable number
            if (intArg1 == -1) {
              //invalid number
                blnError = true
                strErrMsg = LoadResString(4085)
              return;
            }

            //variable assignment or arithmetic operation
            //need next command to determine what kind of assignment/operation
            strArg2 = NextCommand()


            switch (strArg2) {
            case "++"
              // v#++;
              bytCmd = 0x1
            case "+="
              // v# += #; or v# += v#;
              bytCmd = 0x5
            case "--"
              // v#--
              bytCmd = 0x2
            case "-="
              // v# -= #; or v# -= v#;
              bytCmd = 0x7
            case "*="
              // v# *= #; or v# *= v#;
              bytCmd = 0xA5
            case "/="
              // v# /= #; v# /= v#
              bytCmd = 0xA7
            case "="
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


            default:
              //don't know what the heck it is...
              blnError = true
              strErrMsg = LoadResString(4034)
              return;
            } 

          //check for flag assignment
          } else if ( ConvertArgument(strArg1, atFlag)) {
            //flag assignment
            //     f# = true;
            //     f# = false;

            //get flag number
            intArg1 = VariableValue(strArg1)

            //check for equal sign
            strArg2 = NextCommand()
            //if not equal sign
            if (strArg2 != "=") {
              //error
              blnError = true
              strErrMsg = LoadResString(4034)
              return;
            }

            //get flag Value
            strArg2 = NextCommand()


            switch (LCase$(strArg2)) {
            case "true"
              //set this flag
              bytCmd = 0xC


            case "false"
              //reset this flag
              bytCmd = 0xD


            default:
              //error
              blnError = true
              strErrMsg = LoadResString(4034)
              //always exit
              return;
            } 


          } else {
            //not a special syntax
            CompileSpecial = false
            return;
          }

          //skip check for second argument if cmd is known to be a single arg
          //command (increment/decrement/reset/set
          //(set string is also skipped because second arg is already determined)
          switch (bytCmd) {
          case 0x1, 0x2, 0xC, 0xD, 0x72
          default:
            //get next argument
            strArg2 = NextCommand()
            //if it is indirection
            if (strArg2 == "*") {
              //if not already using left indirection, AND cmd is not known
              if (intDir == 0 && bytCmd == 0) {
                //set right indirection
                intDir = 2

                //next char can't be a space, newline, or tab
                switch (strCurrentLine[lngPos + 1]) {
                case " ", vbTab, ""
                  //error
                  blnError = true
                  strErrMsg = LoadResString(4105)
                  return;
                } 

                //get actual variable
                intArg2 = GetNextArg(atVar, -1)
                if (blnError) {
                  //reset error string
                  strErrMsg = LoadResString(4105)
                  return;
                }
              } else {
                //error
                blnError = true
                strErrMsg = LoadResString(4105)
                return;
              }
            } else {
              //arg2 is either number or variable- convert input to standard syntax

              //if it's a number, check for negative value
              if (Val(strArg2) < 0) {
                //valid negative numbers are -1 to -128
                if (Val(strArg2) < -128) {
                  //error
                  blnError = true
                  strErrMsg = LoadResString(4095)
                  return;
                }
                //convert it to 2s-compliment unsigned value by adding it to 256
                strArg2 = CStr(256 + Val(strArg2))
                //Debug.Assert Val(strArg2) >= 128 && Val(strArg2) <= 255

                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh, leMedium
                  //show warning
                  AddWarning(5098
                } 
              }


              blnArg2Var = true
              if (!ConvertArgument(strArg2, atNum, blnArg2Var)) {
                //set error
                blnError = true
                strErrMsg = Replace(LoadResString(4088), ARG1, strArg2)
                return;
              }

              //it's a number or variable; verify it's 0-255
              intArg2 = VariableValue(strArg2)
              //if invalid
              if (intArg2 == -1) {
                //set error
                blnError = true
                strErrMsg = Replace(LoadResString(4088), ARG1, strArg2)
                return;
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
                    bytCmd = 0xB
                  } else {
                    //assign.n
                    bytCmd = 0x3
                  }
                }
              }
            } //End if (// not indirection
          }  //if not inc/dec

          //if command is not known
          if (bytCmd == 0) {
            //if arg values are the same (already know arg2 is a variable)
            //and no indirection
            if (intArg1 = intArg2) && intDir = 0) {
              //check for long arithmetic
              strArg2 = NextCommand()
              //if end of command is reached
              if (strArg2 == ";") {
                //move pointer back one space so eol
                //check in CompileAGI works correctly
                lngPos = lngPos - 1

                //this is a simple assign (with a variable being assigned to itself!!)
                if (agMainLogSettings.ErrorLevel == ) {
                case leHigh
                  blnError = true
                  strErrMsg = LoadResString(4084)
                  return;
                case leMedium
                  AddWarning(5036
                case leLow
                  //allow it
                } 
                bytCmd = 0x3
              } else {
                //this may be long arithmetic
                switch (strArg2) {
                case "+"
                  bytCmd = 0x5
                case "-"
                  bytCmd = 0x7
                case "*"
                  bytCmd = 0xA5
                case "/"
                  bytCmd = 0xA7
                default:
                  //error
                  blnError = true
                  strErrMsg = LoadResString(4087)
                  return;
                } 

                //now get actual second argument
                blnArg2Var = true
                intArg2 = GetNextArg(atNum, -1, blnArg2Var)
                //if error
                if (blnError) {
                  if (Val(strErrMsg) == 4063) {
                    //change error message
                    strErrMsg = Mid(strErrMsg, 55, InStrRev(strErrMsg, "//") - 53)
                    strErrMsg = Replace(LoadResString(4161), ARG1, strErrMsg)
                  } else {
                    strErrMsg = Replace(LoadResString(4161), ARG1, "")
                  }
                  return;
                }
              }
            } else {
              //variables are different
              //must be assignment
              // v# = v#
              // *v# = v#
              // v# = *v#
              switch (intDir) {
              case 0  //assign.v
                bytCmd = 0x4
              case 1 //lindirect.v
                bytCmd = 0x9
              case 2  //rindirect
                bytCmd = 0xA
                blnArg2Var = false
              } 
              //always reset arg2var flag so
              //command won't be adjusted later
                blnArg2Var = false
            }
          }

          //if second argument is a variable
          if (blnArg2Var) {
            bytCmd = bytCmd + 1
          }

          //get next command on this line
          strArg2 = NextCommand(true)

          //check that next command is semicolon
          if (strArg2<> ";") {
            blnError = true
            //check for added quotes; they are the problem
            if (lngQuoteAdded >= 0) {
              //reset line;
              lngLine = lngQuoteAdded
              lngErrLine = lngLine - lngIncludeOffset
              //string error
              strErrMsg = LoadResString(4051)
            } else {
              strErrMsg = LoadResString(4007)
            }
            return;
          } else {
            //move pointer back one space so
            //eol check in CompileAGI works
            //correctly
            lngPos = lngPos - 1
          }

          //need to validate arguments for this command
          switch (bytCmd) {
          case 0x1, 0x2, 0xC, 0xD
            //single arg commands
            ReDim bytArgs(0)
            bytArgs(0) = intArg1
          case 0
            //Debug.Assert false
          default:
            //two arg commands
            ReDim bytArgs(1)
            bytArgs(0) = intArg1
            bytArgs(1) = intArg2
          } 

          //validate commands before writing
          if (!ValidateArgs(bytCmd, bytArgs)) {
            CompileSpecial = false
            return;
          }

          //write command and arg1
          tmpLogRes.Writebyte bytCmd
          tmpLogRes.Writebyte Cbyte(intArg1)
          //write second argument for all cmds except 0x1, 0x2, 0xC, 0xD
          switch (bytCmd) {
          case 0x1, 0x2, 0xC, 0xD
          default:
            tmpLogRes.Writebyte Cbyte(intArg2)
          } 
        return;

        ErrHandler:
          strError = Err.Description
          strErrSrc = Err.Source
          lngError = Err.Number
          //if error is an app specific error, just pass it along; otherwise create
          //an app specific error to encapsulate whatever happened
          if ((lngError && WINAGI_ERR) == WINAGI_ERR) {
            //pass it along
            throw new Exception("lngError, strErrSrc, strError
          } else {
            throw new Exception("659, strErrSrc, Replace(LoadResString(659), ARG1, CStr(lngError) + ":" + strError)
          }
        } //endfunction

        static internal byte LabelNum(string LabelName)
          //this function will return the number of the label passed
          //as a string,
          //or zero, if a match is not found


          int i

          //step through all labels,
          For i = 1 To bytLabelCount
            if (llLabel(i).Name == LabelName) {
              LabelNum = i
              return;
            }
          Next i

          //if not found, zero is returned
        } //endfunction


        */
    }
  }
}
