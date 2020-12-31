using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.WinAGI;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;

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
      string Name ;
      int Loc;
    }

    internal static AGIResource tmpLogRes;
    internal static byte[] mbytData;
    internal const int MAX_BLOCK_DEPTH = 64; //used by LogDecode functions too
    internal const int MAX_LABELS = 255;
    internal const bool UseTypeChecking = true;

    internal const string NOT_TOKEN = "!";
    internal const string OR_TOKEN = "||";
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
    internal static string[] stlInput;  //the entire text to be compiled; includes the
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

    static internal void CompileLogic(AGILogic SourceLogic)
    {
      /*
        internal Sub CompileLogic(SourceLogic As AGILogic)
          //this function compiles the sourcetext that is passed
          //the function returns a Value of true if successful; it returns false
          //and sets information about the error if an error in the source text is found

          //note that when errors are returned, line is adjusted because
          //editor rows(lines) start at //1//, but the compiler starts at line //0//

          Dim blnCompiled As bool
          Dim stlSource As string[]
          Dim dtFileMod As Date
          Dim strInput As string

          //set error info to success as default
          blnError = false
          lngErrLine = -1
          strErrMsg = ""
          strModule = ""
          strModFileName = ""
          intCtlCount = 0

          On Error Resume Next
          //initialize global defines
          //get datemodified property
          dtFileMod = FileLastMod(agGameDir & "globals.txt")
          if (CRC32(StrConv(CStr(dtFileMod), vbFromUnicode)) != agGlobalCRC) {
            GetGlobalDefines
          } //End If

          //if ids not set yet
          if (!blnSetIDs) {
            SetResourceIDs
          } //End If

          On Error GoTo ErrHandler

          //insert current values for reserved defines that can change values
          //agResDef(0).Value = "ego"  //this one doesn//t change
          agResDef(1).Value = QUOTECHAR & agMainGame.GameVersion & QUOTECHAR
          agResDef(2).Value = QUOTECHAR & agMainGame.GameAbout & QUOTECHAR
          agResDef(3).Value = QUOTECHAR & agMainGame.GameID & QUOTECHAR
          //////Debug.Assert agInvObj.Loaded
          if (agInvObj.Loaded) {
            //Count of ACTUAL useable objects is one less than inventory object Count
            //because the first object (//?//) is just a placeholder
            agResDef(4).Value = agInvObj.Count - 1
          Else
            agResDef(4).Value = -1
          } //End If

          //convert back to correct byte values
          strInput = StrConv(ExtCharTobyte(SourceLogic.SourceText), vbUnicode)
          //assign to source stringlist
          Set stlSource = New StringList
          stlSource.Assign strInput

          bytLogComp = SourceLogic.Number
          strLogCompID = SourceLogic.ID

          //reset error info
          lngErrLine = -1
          strErrMsg = ""
          strModule = ""
          strModFileName = ""

          //add include files (extchars handled automatically)
          if (!AddIncludes(stlSource)) {
            //dereference objects
            Set stlInput = Nothing
            //return error
            On Error GoTo 0
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          //remove any blank lines from end
          Do Until Len(stlInput(stlInput.Count - 1)) != 0 || stlInput.Count = 0
            stlInput.Delete stlInput.Count - 1
          Loop

          //if nothing to compile, throw an error
          if (stlInput.Count == 0) {
            //dereference objects
            Set stlInput = Nothing
            //return error
            strErrMsg = LoadResString(4159)
            lngErrLine = 0
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          //strip out all comments
          if (!RemoveComments()) {
            //dereference objects
            Set stlInput = Nothing
            //return error
            On Error GoTo 0
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          //read labels
          if (!ReadLabels()) {
            //dereference objects
            Set stlInput = Nothing
            //return error
            On Error GoTo 0
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          //enumerate and replace all the defines
          if (!ReadDefines()) {
            //dereference objects
            Set stlInput = Nothing
            //return error
            On Error GoTo 0
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          //read predefined messages
          if (!ReadMsgs()) {
            //dereference objects
            Set stlInput = Nothing
            //return error
            On Error GoTo 0
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          //assign temporary resource object
          Set tmpLogRes = New AGIResource
          tmpLogRes.NewResource

          //write a word as a place holder for offset to msg section start
          tmpLogRes.WriteWord 0, 0

          //use agi compiler
          blnCompiled = CompileAGI()

          //compile commands
          if (!blnCompiled) {
            //dereference objects
            tmpLogRes.Unload
            Set tmpLogRes = Nothing
            Set stlInput = Nothing
            //return error
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          //write message section
          if (!WriteMsgs()) {
            //dereference objects
            tmpLogRes.Unload
            Set tmpLogRes = Nothing
            Set stlInput = Nothing
            //return error
            On Error GoTo 0
            On Error GoTo 0: Err.Raise vbObjectError + 635, "LogCompile", CStr(lngErrLine + 1) & "|" & strModule & "|" & strErrMsg
            return;
          } //End If

          With SourceLogic
            //assign resource data
            .Resource.AllData = tmpLogRes.AllData

            //update compiled crc
            SourceLogic.CompiledCRC = SourceLogic.CRC
            // and write the new crc values to property file
            WriteGameSetting "Logic" & CStr(.Number), "CRC32", "&H" & Hex$(.CRC), "Logics"
            WriteGameSetting "Logic" & CStr(.Number), "CompCRC32", "&H" & Hex$(.CompiledCRC)
          End With

          //dereference objects
          tmpLogRes.Unload
          Set tmpLogRes = Nothing
          Set stlInput = Nothing

        return;

        ErrHandler:
          //if error is an app specific error, just pass it along; otherwise create
          //an app specific error to encapsulate whatever happened
          strError = Err.Description
          strErrSrc = Err.Source
          lngError = Err.Number

          //dereference objects
          tmpLogRes.Unload
          Set tmpLogRes = Nothing
          Set stlInput = Nothing
          if ((lngError && vbObjectError) == vbObjectError) {
            //pass it along
            On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
          Else
            //return error
            On Error GoTo 0
            On Error GoTo 0: Err.Raise vbObjectError + 634, "LogCompile", "-1||" & Replace(LoadResString(591), ARG1, CStr(lngError) & ":" & strError)
          } //End If
        } //End Sub
      */
    }
    static void tmp_LogCompile()
    {
      /*
        static internal Function ArgTypeName(ArgType As ArgTypeEnum) As string

          switch (ArgType
          case atNum       //i.e. numeric Value
            ArgTypeName = "number"
          case atVar       //v##
            ArgTypeName = "variable"
          case atFlag      //f##
            ArgTypeName = "flag"
          case atMsg       //m##
            ArgTypeName = "message"
          case atSObj      //o##
            ArgTypeName = "screen object"
          case atIObj      //i##
            ArgTypeName = "inventory item"
          case atStr       //s##
            ArgTypeName = "string"
          case atWord      //w## -- word argument (that user types in)
            ArgTypeName = "word"
          case atCtrl      //c##
            ArgTypeName = "controller"
          case atDefStr    //defined string; could be msg, inv obj, or vocword
            ArgTypeName = "text in quotes"
          case atVocWrd    //vocabulary word; NOT word argument
            ArgTypeName = "vocabulary word"
          } 
        } //End Function

        static internal Sub CheckResFlagUse(ArgVal As byte)

          //if error level is low, don//t do anything
          if (agMainLogSettings.ErrorLevel == leLow) {
            return;
          } //End If

          switch (ArgVal
          case 2, 4, 7, 8, 9, 10, Is >= 13
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

          default: //all other reserved variables should be read only
            AddWarning 5025, Replace(LoadResString(5025), ARG1, agResFlag(ArgVal).Name)
          } 
        } //End Sub
        static internal Sub CheckResVarUse(ArgNum As byte, ArgVal As byte)

          //if error level is low, don//t do anything
          if (agMainLogSettings.ErrorLevel == leLow) {
            return;
          } //End If

          switch (ArgNum
          case Is >= 27, 21, 15, 7, 3
            //no restrictions for
            //  all non restricted variables (>=27)
            //  curent score (v3)
            //  max score (v7)
            //  joystick sensitivity (v15)
            //  msg window delay time

          case 6 //ego direction
            //should be restricted to values 0-8
            if (ArgVal > 8) {
              AddWarning 5018, Replace(Replace(LoadResString(5018), ARG1, agResVar(6).Name), ARG2, "8")
            } //End If

          case 10 //cycle delay time
            //large values highly unusual
            if (ArgVal > 20) {
              AddWarning 5055
            } //End If

          case 23 //sound attenuation
            //restrict to 0-15
            if (ArgVal > 15) {
              AddWarning 5018, Replace(Replace(LoadResString(5018), ARG1, agResVar(23).Name), ARG2, "15")
            } //End If

          case 24 //max input length
            if (ArgVal > 39) {
              AddWarning 5018, Replace(Replace(LoadResString(5018), ARG1, agResVar(24).Name), ARG2, "39")
            } //End If

          case 17, 18 //error value, and error info
            //resetting to zero is usually a good thing; other values don//t make sense
            if (ArgVal > 0) {
              AddWarning 5092, Replace(LoadResString(5092), ARG1, agResVar(ArgNum).Name)
            } //End If

          case 19 //key_pressed value
            //ok if resetting for key input
            if (ArgVal > 0) {
              AddWarning 5017, Replace(LoadResString(5017), ARG1, agResVar(ArgNum).Name)
            } //End If

          default: //all other reserved variables should be read only
            AddWarning 5017, Replace(LoadResString(5017), ARG1, agResVar(ArgNum).Name)
          } 
        } //End Sub




        internal Function ConvertArgument(ByRef strArgIn As string, ArgType As ArgTypeEnum, Optional ByRef blnVarOrNum As bool = false) As bool
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

          Dim i As int
          Dim intAsc As int

          On Error GoTo ErrHandler

          //check if already in correct format
          switch (ArgType
          case atNum  //numeric only
            if (IsNumeric(strArgIn)) {
              ConvertArgument = true
              //reset VarOrNum flag
              blnVarOrNum = false
              return  //Exit Function
            } //End If
            //unless looking for var or num
            if (blnVarOrNum) {
              //then //v##// is ok
              if ((AscW(strArgIn) || 32) == 118) {
                if (VariableValue(strArgIn) != -1) {
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              } //End If
            } //End If

          case atVar
            //if first char matches
            if ((AscW(strArgIn) || 32) == 118) {
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            } //End If

          case atFlag
            //if first char matches
            if ((AscW(strArgIn) || 32) == 102) {
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            } //End If

          case atCtrl
            //if first char matches
            if ((AscW(strArgIn) || 32) == 99) {
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            } //End If

          case atSObj
            //if first char matches
            if ((AscW(strArgIn) || 32) == 111) {
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            } //End If

          case atStr
            //if first char matches
            if ((AscW(strArgIn) || 32) == 115) {
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            } //End If

          case atWord //NOTE: this is NOT vocab word; this is word arg type (used in command word.to.string)
            //if first char matches
            if ((AscW(strArgIn) || 32) == 119) {
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            } //End If

          case atMsg
            //if first char matches, or is a quote
            intAsc = AscW(strArgIn)
            switch (intAsc
            case 77, 109
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            case 34
              //strings are always ok
              ConvertArgument = true
              return  //Exit Function
            } 

          case atIObj
            //if first char matches, or is a quote
            intAsc = AscW(strArgIn)
            switch (intAsc
            case 73, 105
              //if this arg returns a valid Value
              if (VariableValue(strArgIn) != -1) {
                //ok
                ConvertArgument = true
                return  //Exit Function
              } //End If
            case 34
              //strings are always ok
              ConvertArgument = true
              return  //Exit Function
            } 

          case atVocWrd
            //can be number or string in quotes
            if (IsNumeric(strArgIn) || AscW(strArgIn) == 34) {
              //ok
              ConvertArgument = true
              return  //Exit Function
            } //End If
          } 

          //arg is not in correct format; must be reserved name, global or local define, or an error

          //first, check against local defines
          For i = 0 To lngDefineCount - 1
            if (strArgIn == tdDefines(i).Name) {
              //match found; check that Value is correct type
              switch (ArgType
              case atNum
                //check for number
                if (IsNumeric(tdDefines(i).Value)) {
                  //reset VarOrNum flag
                  blnVarOrNum = false
                  ConvertArgument = true
                  strArgIn = tdDefines(i).Value
                  return  //Exit Function
                } //End If

                //if checking for variables
                if (blnVarOrNum) {
                  if ((AscW(tdDefines(i).Value) || 32) == 118) {
                    //if this define returns a valid Value
                    if (VariableValue(tdDefines(i).Value) != -1) {
                      //ok
                      strArgIn = tdDefines(i).Value
                      ConvertArgument = true
                    } //End If
                  } //End If
                } //End If

              case atVar
                //v## only
                if ((AscW(tdDefines(i).Value) || 32) == 118) {
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If
                } //End If

              case atFlag
                //f## only
                if ((AscW(tdDefines(i).Value) || 32) == 102) {
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If
                } //End If

              case atMsg
                //m## or a string
                intAsc = AscW(tdDefines(i).Value)
                switch (intAsc
                case 77, 109
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If

                case 34
                  strArgIn = tdDefines(i).Value
                  ConvertArgument = true
                } 

              case atSObj
                //o## only
                if (AscW(tdDefines(i).Value) == 111) {
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If
                } //End If

              case atIObj
                //i## or a string
                intAsc = AscW(tdDefines(i).Value)
                switch (intAsc
                case 73, 105
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If
                case 34
                  strArgIn = tdDefines(i).Value
                  ConvertArgument = true
                } 

              case atStr
                //s## only
                if ((AscW(tdDefines(i).Value) || 32) == 115) {
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If
                } //End If

              case atWord
                //w## only
                if ((AscW(tdDefines(i).Value) || 32) == 119) {
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If
                } //End If

              case atCtrl
                //c## only
                if ((AscW(tdDefines(i).Value) || 32) == 99) {
                  //if this define returns a valid Value
                  if (VariableValue(tdDefines(i).Value) != -1) {
                    //ok
                    strArgIn = tdDefines(i).Value
                    ConvertArgument = true
                  } //End If
                } //End If

              case atVocWrd
                //numeric or string only
                if (IsNumeric(tdDefines(i).Value)) {
                  strArgIn = tdDefines(i).Value
                  ConvertArgument = true
                ElseIf AscW(tdDefines(i).Value) == 34) {
                  strArgIn = tdDefines(i).Value
                  ConvertArgument = true
                } //End If

              case atDefStr
                //call to ConvertArgument is never made with type of atDefStr
              } 
              //exit, regardless of result
              return  //Exit Function
            } //End If
          Next i

          //second, check against global defines
          //for any type except vocab words
          if (ArgType != atVocWrd) {
            //check against this type of global defines
            For i = 0 To agGlobalCount - 1
              if (agGlobal(i).Type == ArgType) {
                if (strArgIn == agGlobal(i).Name) {
                  strArgIn = agGlobal(i).Value
                  //reset VarOrNum flag
                  blnVarOrNum = false
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              } //End If
            Next i
            //if checking var or num
            if (blnVarOrNum) {
              //numbers were checked; need to check variables
              For i = 0 To agGlobalCount - 1
                if (agGlobal(i).Type == atVar) {
                  if (strArgIn == agGlobal(i).Name) {
                    strArgIn = agGlobal(i).Value
                    ConvertArgument = true
                    return  //Exit Function
                  } //End If
                } //End If
              Next i
            } //End If
          Else
            //check vocab words only against numbers
            For i = 0 To agGlobalCount - 1
              if (agGlobal(i).Type == atNum) {
                if (strArgIn == agGlobal(i).Name) {
                  strArgIn = agGlobal(i).Value
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              } //End If
            Next i
          } //End If

          //check messages, iobjs, and vocab words against global strings
          if ((ArgType == atMsg) || (ArgType == atIObj) || (ArgType == atVocWrd)) {
            //check against global defines (string type)
            For i = 0 To agGlobalCount - 1
              if (agGlobal(i).Type == atDefStr) {
                if (strArgIn == agGlobal(i).Name) {
                  strArgIn = agGlobal(i).Value
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              } //End If
            Next i
          } //End If

          //third, check numbers against list of resource IDs
          if (ArgType == atNum) {
            //check against resource IDs
            For i = 0 To 255
              //if this arg matches one of the resource ids
              if (strArgIn == strLogID(i)) {
                strArgIn = CStr(i)
                //reset VarOrNum flag
                blnVarOrNum = false
                ConvertArgument = true
                return  //Exit Function
              } //End If
              if (strArgIn == strPicID(i)) {
                strArgIn = CStr(i)
                //reset VarOrNum flag
                blnVarOrNum = false
                ConvertArgument = true
                return  //Exit Function
              } //End If
              if (strArgIn == strSndID(i)) {
                strArgIn = CStr(i)
                //reset VarOrNum flag
                blnVarOrNum = false
                ConvertArgument = true
                return  //Exit Function
              } //End If
              if (strArgIn == strViewID(i)) {
                strArgIn = CStr(i)
                //reset VarOrNum flag
                blnVarOrNum = false
                ConvertArgument = true
                return  //Exit Function
              } //End If
            Next i
          } //End If

          //lastly, if using reserved names,
          if (agUseRes) {
            //last of all, check reserved names
            switch (ArgType
            case atNum
              For i = 0 To 4
                if (strArgIn == agEdgeCodes(i).Name) {
                  strArgIn = agEdgeCodes(i).Value
                  //reset VarOrNum flag
                  blnVarOrNum = false
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i
              For i = 0 To 8
                if (strArgIn == agEgoDir(i).Name) {
                  strArgIn = agEgoDir(i).Value
                  //reset VarOrNum flag
                  blnVarOrNum = false
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i
              For i = 0 To 4
                if (strArgIn == agVideoMode(i).Name) {
                  strArgIn = agVideoMode(i).Value
                  //reset VarOrNum flag
                  blnVarOrNum = false
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i
              For i = 0 To 8
                if (strArgIn == agCompType(i).Name) {
                  strArgIn = agCompType(i).Value
                  //reset VarOrNum flag
                  blnVarOrNum = false
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i
              For i = 0 To 15
                if (strArgIn == agResColor(i).Name) {
                  strArgIn = agResColor(i).Value
                  //reset VarOrNum flag
                  blnVarOrNum = false
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i
              //check against invobj Count
              if (strArgIn == agResDef(4).Name) {
                strArgIn = agResDef(4).Value
                //reset VarOrNum flag
                blnVarOrNum = false
                ConvertArgument = true
                return  //Exit Function
              } //End If

              //if looking for numbers OR variables
              if (blnVarOrNum) {
                //check against builtin variables as well
                For i = 0 To 26
                  if (strArgIn == agResVar(i).Name) {
                    strArgIn = agResVar(i).Value
                    ConvertArgument = true
                    return  //Exit Function
                  } //End If
                Next i
              } //End If

            case atVar
               For i = 0 To 26
                if (strArgIn == agResVar(i).Name) {
                  strArgIn = agResVar(i).Value
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i

            case atFlag
               For i = 0 To 17
                if (strArgIn == agResFlag(i).Name) {
                  strArgIn = agResFlag(i).Value
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i
            case atMsg
              For i = 1 To 3 //for gamever and gameabout and gameid
                if (strArgIn == agResDef(i).Name) {
                  strArgIn = agResDef(i).Value
                  ConvertArgument = true
                  return  //Exit Function
                } //End If
              Next i
            case atSObj
              if (strArgIn == agResDef(0).Name) {
                strArgIn = agResDef(0).Value
                ConvertArgument = true
                return  //Exit Function
              } //End If
            case atStr
              if (strArgIn == agResDef(5).Name) {
                strArgIn = agResDef(5).Value
                ConvertArgument = true
                return  //Exit Function
              } //End If
            } 
          } //End If

          //if not found or error, return false
        ErrHandler:

          //just exit
        } //End Function
        static internal Function GetNextArg(ArgType As ArgTypeEnum, ArgPos As int, Optional ByRef blnVarOrNum As bool = false) As int
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

          Dim strArg As string, lngArg As int
          Dim i As int

          On Error GoTo ErrHandler

          //get next command
          strArg = NextCommand()

          //convert it
          if (!ConvertArgument(strArg, ArgType, blnVarOrNum)) {
            //error
            blnError = true
            //if a closing paren found
            if (strArg == ")") {
              // arg missing
              strErrMsg = Replace(Replace(LoadResString(4054), ARG1, CStr(ArgPos + 1)), ARG3, ArgTypeName(ArgType))
            Else
              //use 1-base arg values
              strErrMsg = Replace(Replace(Replace(LoadResString(4063), ARG1, CStr(ArgPos + 1)), ARG2, ArgTypeName(ArgType)), ARG3, strArg)
            } //End If
            return  //Exit Function
          } //End If

          switch (ArgType
          case atNum  //number
            //verify type is number
            if (!IsNumeric(strArg)) {
              //if NOT catching variables too
              if (!blnVarOrNum) {
                blnError = true
                strErrMsg = Replace(LoadResString(4062), ARG1, CStr(ArgPos))
                return  //Exit Function
              } //End If
            Else
              //return //is NOT a variable//; ensure flag is reset
              blnVarOrNum = false
            } //End If
            //check for negative number
            if (Val(strArg) < 0) {
              //valid negative numbers are -1 to -128
              if (Val(strArg) < -128) {
                //error
                blnError = true
                strErrMsg = LoadResString(4157)
                return  //Exit Function
              } //End If
              //convert it to 2s-compliment unsigned value by adding it to 256
              strArg = CStr(256 + Val(strArg))
              //////Debug.Assert Val(strArg) >= 128 && Val(strArg) <= 255

              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                //show warning
                AddWarning 5098
              } 

            } //End If
            //convert to number and validate
            lngArg = VariableValue(strArg)
            if (lngArg == -1) {
              blnError = true
              //use 1-based arg values
              strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
              return  //Exit Function
            } //End If

          case atVar, atFlag  //variable, flag
            //get Value
            lngArg = VariableValue(strArg)
            if (lngArg == -1) {
              blnError = true
              //use 1-based arg values
              strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
              return  //Exit Function
            } //End If

          case atCtrl    //controller
            //controllers should be  0 - 49
            //get Value
            lngArg = VariableValue(strArg)
            if (lngArg == -1) {
              blnError = true
              //if high errlevel
              if (agMainLogSettings.ErrorLevel == leHigh) {
                //use 1-based arg values
                strErrMsg = Replace(LoadResString(4136), ARG1, CStr(ArgPos + 1))
              Else
                //use 1-based arg values
                strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
              } //End If
              return  //Exit Function
            Else
              //if outside expected bounds (controllers should be limited to 0-49)
              if (lngArg > 49) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  //generate error
                  blnError = true
                  //use 1-based arg values
                  strErrMsg = Replace(LoadResString(4136), ARG1, CStr(ArgPos + 1))
                  return  //Exit Function

                case leMedium
                  //generate warning
                  AddWarning 5060
                } 
              } //End If
            } //End If

          case atSObj //screen object
            //get Value
            lngArg = VariableValue(strArg)
            if (lngArg == -1) {
              blnError = true
              //use 1-based arg values
              strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
              return  //Exit Function
            } //End If

            //check against max screen object Value
            if (lngArg > agInvObj.MaxScreenObjects) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                //generate error
                blnError = true
                strErrMsg = Replace(LoadResString(4119), ARG1, CStr(agInvObj.MaxScreenObjects))
                return  //Exit Function

              case leMedium
                //generate warning
                AddWarning 5006, Replace(LoadResString(5006), ARG1, CStr(agInvObj.MaxScreenObjects))
              } 
            } //End If

          case atStr //string
            //get Value
            lngArg = VariableValue(strArg)
            if (lngArg == -1) {
              blnError = true
              //if high errlevel
              if (agMainLogSettings.ErrorLevel == leHigh) {
                //for version 2.089, 2.272, and 3.002149 only 12 strings
                switch (agIntVersion
                case "2.089", "2.272", "3.002149"
                  //use 1-based arg values
                  strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "11")
                default:
                  strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "23")
                } 

              Else
                //use 1-based arg values
                strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
              } //End If
              return  //Exit Function
            Else
              //if outside expected bounds (strings should be limited to 0-23)
              if ((lngArg > 23) || (lngArg > 11 && (agIntVersion == "2.089" || agIntVersion == "2.272" || agIntVersion == "3.002149"))) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  //generate error
                  blnError = true
                  //for version 2.089, 2.272, and 3.002149 only 12 strings
                  switch (agIntVersion
                  case "2.089", "2.272", "3.002149"
                    //use 1-based arg values
                    strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "11")
                  default:
                    strErrMsg = Replace(Replace(LoadResString(4079), ARG1, CStr(ArgPos + 1)), ARG2, "23")
                  } 
                  return  //Exit Function

                case leMedium
                 //generate warning
                 //for version 2.089, 2.272, and 3.002149 only 12 strings
                  switch (agIntVersion
                  case "2.089", "2.272", "3.002149"
                    AddWarning 5007, Replace(LoadResString(5007), ARG1, "11")
                  default:
                    AddWarning 5007, Replace(LoadResString(5007), ARG1, "23")
                  } 
                } 
              } //End If
            } //End If

          case atWord //word  (word type is NOT words from word.tok)
            //get Value
            lngArg = VariableValue(strArg)
            if (lngArg == -1) {
              blnError = true
              //if high error level
              if (agMainLogSettings.ErrorLevel == leHigh) {
                //use 1-based arg values
                strErrMsg = Replace(LoadResString(4090), ARG1, CStr(ArgPos + 1))
              Else
                strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
              } //End If
              return  //Exit Function
            Else
              //if outside expected bounds (words should be limited to 0-9)
              if (lngArg > 9) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  //generate error
                  blnError = true
                  //use 1-based arg values
                  strErrMsg = Replace(LoadResString(4090), ARG1, CStr(ArgPos + 1))
                  return  //Exit Function

                case leMedium
                  //generate warning
                  AddWarning 5008
                } 
              } //End If
            } //End If

          case atMsg  //message
            //returned arg is either m## or "msg"
            switch (AscW(strArg)
            case 109
              //validate Value
              lngArg = VariableValue(strArg)
              if (lngArg == -1) {
                blnError = true
                //use 1-based arg values
                strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
                return  //Exit Function
              } //End If
              //m0 is not allowed
              if (lngArg == 0) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  blnError = true
                  strErrMsg = LoadResString(4107)
                  return  //Exit Function
                case leMedium
                  //generate warning
                  AddWarning 5091, Replace(LoadResString(5091), ARG1, CStr(lngArg))
                  //make this a null msg
                  blnMsg(lngArg) = true
                  strMsg(lngArg) = ""
                case leLow
                  //ignore; it will be handled when writing messages
                } 
              } //End If

              //verify msg exists
              if (!blnMsg(lngArg)) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  blnError = true
                  strErrMsg = Replace(LoadResString(4113), ARG1, CStr(lngArg))
                  return  //Exit Function
                case leMedium
                  //generate warning
                  AddWarning 5090, Replace(LoadResString(5090), ARG1, CStr(lngArg))
                  //make this a null msg
                  blnMsg(lngArg) = true
                  strMsg(lngArg) = ""
                case leLow
                  //ignore; WinAGI adds a null value, so no error will occur
                } 
              } //End If
            case 34
              //concatenate, if applicable
              strArg = ConcatArg(strArg)
              if (blnError) {
                //concatenation error; exit
                return  //Exit Function
              } //End If

              //strip off quotes
              strArg = Mid(strArg, 2, Len(strArg) - 2)
              //convert to msg number
              lngArg = MessageNum(strArg)

              //if unallowed characters found, error was raised; exit
              if (lngArg == -1) {
                blnError = true
                return  //Exit Function
              } //End If

              //if valid number not found
              if (lngArg == 0) {
                blnError = true
                strErrMsg = LoadResString(4092)
                return  //Exit Function
              } //End If

            } 

          case atIObj //inventory object
            //only true restriction is can//t exceed object count, and can//t exceed 255 objects (0-254)
            //i0 is usually a //?//, BUT not a strict requirement
            //HOWEVER, WinAGI enforces that i0 MUST be //?//, and can//t be changed
            //also, if any code tries to access an object by //?//, return error

            //if character is inv obj arg type prefix
            switch (AscW(strArg)
            case 105
              //validate Value
              lngArg = VariableValue(strArg)
              if (lngArg == -1) {
                blnError = true
                //use 1-based arg values
                strErrMsg = Replace(LoadResString(4066), ARG1, CStr(ArgPos + 1))
                return  //Exit Function
              } //End If

            case 34
              //concatenate, if applicable
              strArg = ConcatArg(strArg)
              if (blnError) {
                //concatenation error
                return  //Exit Function
              } //End If

              //convert to inv obj number
              //first strip off starting and ending quotes
              strArg = Mid(strArg, 2, Len(strArg) - 2)
              //if a quotation mark is part of an object name,
              //it is coded in the logic as a //\"// not just a //"//
              //need to ensure all //\"// codes are converted to //"//
              //otherwise the object would never match
              strArg = Replace(strArg, "\""", QUOTECHAR)

              //step through all object names
              For i = 0 To agResDef(4).Value
                //if this is the object
                if (strArg == agInvObj(i).ItemName) {
                  //return this Value
                  lngArg = Cbyte(i)
                  Exit For
                } //End If
              Next i

              //if not found,
              if (i == agResDef(4).Value + 1) {
                blnError = true
                //check for added quotes; they are the problem
                if (lngQuoteAdded >= 0) {
                  //reset line;
                  lngLine = lngQuoteAdded
                  //string error
                  strErrMsg = LoadResString(4051)
                Else
                  //use 1-base arg values
                  strErrMsg = Replace(LoadResString(4075), ARG1, CStr(ArgPos + 1))
                } //End If
                return  //Exit Function
              } //End If

              //if object is not unique
              if (!agInvObj(lngArg).Unique) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  blnError = true
                  //use 1-based arg values
                  strErrMsg = Replace(LoadResString(4036), ARG1, CStr(ArgPos + 1))
                  return  //Exit Function
                case leMedium
                  //set warning
                  AddWarning 5003, Replace(LoadResString(5003), ARG1, CStr(ArgPos + 1))
                case leLow
                  //no action
                } 
              } //End If
            } 

            //if object number exceeds current object Count,
            //if (lngArg >= agInvObj.Count) {
            if (lngArg > agResDef(4).Value) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                blnError = true
                //use 1-based arg values
                strErrMsg = Replace(LoadResString(4112), ARG1, CStr(ArgPos + 1))
                return  //Exit Function
              case leMedium
                //set warning
                //use 1-based arg values
                AddWarning 5005, Replace(LoadResString(5005), ARG1, CStr(ArgPos + 1))
              case leLow
                //no action
              } 
            Else
              //if object is a question mark, raise error/warning
              if (agInvObj(lngArg).ItemName == "?") {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  blnError = true
                  //use 1-based arg values
                  strErrMsg = Replace(LoadResString(4111), ARG1, CStr(ArgPos + 1))
                  return  //Exit Function
                case leMedium
                  //set warning
                  AddWarning 5004
                case leLow
                  //no action
                } 
              } //End If
            } //End If

          case atVocWrd
            //words can be ## or "word"
            if (IsNumeric(strArg)) {
              lngArg = CLng(strArg)
              //make sure it//s not a decimal
              if (Val(strArg) != lngArg) {
                blnError = true
                lngArg = -1
              Else
                //validate the group
                blnError = !agVocabWords.GroupExists(lngArg)
              } //End If
            Else
              //this is a string; concatenate if applicable
              strArg = ConcatArg(strArg)
              if (blnError) {
                //concatenation error
                return  //Exit Function
              } //End If

              //convert to word number
              //first strip off starting and ending quotes
              strArg = Mid(strArg, 2, Len(strArg) - 2)

              On Error Resume Next
              //get argument val by checking against word list
              if (agVocabWords.WordExists(strArg)) {
                lngArg = agVocabWords(strArg).Group
              Else
                //RARE, but if it//s an //a// or //i// that isn//t defined,
                //it//s word group 0
                if (strArg == "i" || strArg == "a" || strArg == "I" || strArg == "A") {
                  lngArg = 0
                  //add warning
                  switch (agMainLogSettings.ErrorLevel
                  case leHigh, leMedium
                    AddWarning 5108, Replace(LoadResString(5108), ARG1, strArg)
                  } 
                Else
                  //set error flag
                  blnError = true
                  //set arg to invalid number
                  lngArg = -1
                } //End If
              } //End If
            } //End If

            //now lngArg is a valid group number, unless blnError is set

            //if there is an error
            if (blnError) {
              //if arg value=-1 OR high level,
              if (agMainLogSettings.ErrorLevel == leHigh || (lngArg == -1)) {
                //argument is already 1-based for said tests
                strErrMsg = Replace(LoadResString(4114), ARG1, strArg)
                return  //Exit Function
              Else
                if (agMainLogSettings.ErrorLevel == leMedium) {
                  //set warning
                  AddWarning 5019, Replace(LoadResString(5019), ARG1, strArg)
                  blnError = false
                } //End If
              } //End If
            } //End If

            //check for group 0
            if (lngArg == 0) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4035), ARG1, strArg)
                return  //Exit Function
              case leMedium
                AddWarning 5083, Replace(LoadResString(5083), ARG1, strArg)
              case leLow
              } 
            } //End If

          } 

          //set return Value
          GetNextArg = lngArg
        return  //Exit Function

        ErrHandler:
          //////Debug.Assert false

          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "GetNextArg")
          blnError = true
        } //End Function


        static internal Sub IncrementLine()
          //increments the the current line of input being processed
          //sets all counters, pointers, etc
          //as well as info needed to support error locating

          //if at end of input (lngLine=-1)
          if (lngLine == -1) {
            //just exit
            return;
          } //End If

          //if compiler is reset
          if (lngLine == -2) {
            //set it to -1 so line 0 is returned
            lngLine = -1
          } //End If

          //increment line counter
          lngLine = lngLine + 1
          lngPos = 0
          //if at end,
          if (lngLine == stlInput.Count) {
            lngLine = -1
            return;
          } //End If
          //check for include lines
          if (Left(stlInput(lngLine), 2) == "#I") {
            lngIncludeOffset = lngIncludeOffset + 1
            //set module
            strModule = strIncludeFile(CInt(Mid(stlInput(lngLine), 3, InStr(3, stlInput(lngLine), ":") - 3)))
            strModFileName = JustFileName(strModule)
            //set errline
            lngErrLine = CLng(Mid(stlInput(lngLine), InStr(3, stlInput(lngLine), ":") + 1, InStr(3, stlInput(lngLine), "#") - 5))
            strCurrentLine = Right(stlInput(lngLine), Len(stlInput(lngLine)) - InStr(2, stlInput(lngLine), "#"))
          Else
            strModule = ""
            strModFileName = ""
            lngErrLine = lngLine - lngIncludeOffset
            //set string
            strCurrentLine = stlInput(lngLine)
          } //End If

        } //End Sub
        static internal Function NextChar(Optional blnNoNewLine As bool = false) As string
          //gets the next non-space character (tabs (ascii code H&9, are converted
          //to a space character, and ignored) from the input stream

          //if the NoNewLine flag is passed,
          //the function will not look past current line for next
          //character; if no character on current line,
          //lngPos is set to end of current line, and
          //empty string is returned
          On Error GoTo ErrHandler

          //if already at end of input (lngLine=-1)
          if (lngLine == -1) {
            //just exit
            NextChar = ""
            return  //Exit Function
          } //End If

          Do
            //first, increment position
            lngPos = lngPos + 1
            //if past end of this line,
            if (lngPos > Len(strCurrentLine)) {
              //if can//t get another line,
              if (blnNoNewLine) {
                //move pointer back
                lngPos = lngPos - 1
                //return empty string
                NextChar = ""
                return  //Exit Function
              } //End If

              //get the next line
              IncrementLine
              //if at end of input
              if (lngLine == -1) {
                //exit with no character
                return  //Exit Function
              } //End If
              //increment pointer(so it points to first character of line)
              lngPos = lngPos + 1
            } //End If

            NextChar = Mid(strCurrentLine, lngPos, 1)

            //only characters <32 that we need to use are return, and linefeed
            if (Len(NextChar) > 0) {
              if (Asc(NextChar) < 32) {
                switch (Asc(NextChar)
                case 10, 13 //treat as returns?
                  NextChar = vbCr
                default:
                  NextChar = " "
                } 
              } //End If
            } //End If
          Loop Until NextChar != " " && LenB(NextChar) != 0
        return  //Exit Function

        ErrHandler:

          strError = Err.Description
          strErrSrc = Err.Source
          lngError = Err.Number

          //if error is an app specific error, just pass it along; otherwise create
          //an app specific error to encapsulate whatever happened
          if ((lngError && vbObjectError) == vbObjectError) {
            //pass it along
            On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
          Else
            On Error GoTo 0: Err.Raise vbObjectError + 656, strErrSrc, Replace(LoadResString(656), ARG1, CStr(lngError) & ":" & strError)
          } //End If
        } //End Function
        static internal Function NextCommand(Optional blnNoNewLine As bool = false) As string
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

          Dim intCmdEnd As int
          Dim intChar As int
          Dim blnInQuotes As bool, blnSlash As bool

          //find next non-blank character
          NextCommand = NextChar(blnNoNewLine)
          //if at end of input,
          if (lngLine == -1) {
            //return empty string
            NextCommand = ""
            return  //Exit Function
          } //End If
          //if no character returned
          if (LenB(NextCommand) == 0) {
            return  //Exit Function
          } //End If

          On Error GoTo ErrHandler

          //if command is a element separator:
          switch (AscW(NextCommand)
          case 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126 //  //(),:;?[\]^`{}~
            //return this single character as a command
            return  //Exit Function
          case 61 //=
            //special case; "=", "=<" and "=>" returned as separate commands
            switch (Mid(strCurrentLine, lngPos + 1, 1)
            case "<", ">"
              //increment pointer
              lngPos = lngPos + 1
              //return the two byte cmd (swap so we get ">=" and "<="
              // instead of "=>" and "=<"
              NextCommand = Mid(strCurrentLine, lngPos, 1) & NextCommand

            case "=" //"=="
              //increment pointer
              lngPos = lngPos + 1
              //return the two byte cmd
              NextCommand = "=="
            } 
            return  //Exit Function
          case 34 //"
            //special case; quote means start of a string
            blnInQuotes = true
          case 43 //+
            //special case; "+", "++" and "+=" returned as separate commands
            if (Mid(strCurrentLine, lngPos + 1, 1) == "+") {
              //increment pointer
              lngPos = lngPos + 1
              //return shorthand increment
              NextCommand = "++"
            ElseIf Mid(strCurrentLine, lngPos + 1, 1) == "=") {
              lngPos = lngPos + 1
              //return shorthand addition
              NextCommand = "+="
            } //End If
            return  //Exit Function
          case 45 //-
            //special case; "-", "--" and "-=" returned as separate commands
            //also check for "-##"
            if (Mid(strCurrentLine, lngPos + 1, 1) == "-") {
              //increment pointer
              lngPos = lngPos + 1
              //return shorthand decrement
              NextCommand = "--"
            ElseIf Mid(strCurrentLine, lngPos + 1, 1) == "=") {
              lngPos = lngPos + 1
              //return shorthand subtract
              NextCommand = "-="
            ElseIf Val(Mid(strCurrentLine, lngPos + 1)) != 0) {
              //add the number found here to current command so it
              //forms a negative number

              //continue adding characters until non-numeric or EOL is reached
              Do Until lngPos + 1 > Len(strCurrentLine)
                intChar = AscW(Mid(strCurrentLine, lngPos + 1, 1))
                if (intChar < 48 || intChar > 57) {
                  //anything other than a digit (0-9)
                  Exit Do
                Else
                  //add character
                  NextCommand = NextCommand & ChrW$(intChar)
                  //incrmeent position
                  lngPos = lngPos + 1
                } //End If
              Loop
            } //End If
            return  //Exit Function
          case 33 //!
            //special case; "!" and "!=" returned as separate commands
            if (Mid(strCurrentLine, lngPos + 1, 1) == "=") {
              //increment pointer
              lngPos = lngPos + 1
              //return not equal
              NextCommand = "!="
            } //End If
            return  //Exit Function
          case 60 //<
            //special case; "<", "<=" and "<>" returned as separate commands
            if (Mid(strCurrentLine, lngPos + 1, 1) == "=") {
              //increment pointer
              lngPos = lngPos + 1
              //return less than or equal
              NextCommand = "<="
            ElseIf Mid(strCurrentLine, lngPos + 1, 1) == ">") {
              //increment pointer
              lngPos = lngPos + 1
              //return not equal
              NextCommand = "<>"
            } //End If
            return  //Exit Function
          case 62 //>
            //special case; ">", ">=" and "><" returned as separate commands
            if (Mid(strCurrentLine, lngPos + 1, 1) == "=") {
              //increment pointer
              lngPos = lngPos + 1
              //return greater than or equal
              NextCommand = ">="
            ElseIf Mid(strCurrentLine, lngPos + 1, 1) == "<") {
              //increment pointer
              lngPos = lngPos + 1
              //return not equal (//><// is same as //<>//)
              NextCommand = "<>"
            } //End If
            return  //Exit Function
          case 42 //*
            //special case; "*" and "*=" returned as separate commands;
            if (Mid(strCurrentLine, lngPos + 1, 1) == "=") {
              //increment pointer
              lngPos = lngPos + 1
              //return shorthand multiplication
              NextCommand = "*="
            //since block commands are removed, check for the in order to provide a
            //meaningful error message
            ElseIf Mid(strCurrentLine, lngPos + 1, 1) == "/") {
              lngPos = lngPos + 1
              NextCommand = "* /"
            } //End If
            return  //Exit Function
          case 47 ///
            //special case; "/" , "//" and "/=" returned as separate commands
            if (Mid(strCurrentLine, lngPos + 1, 1) == "=") {
              lngPos = lngPos + 1
              //return shorthand division
              NextCommand = "/="
            ElseIf Mid(strCurrentLine, lngPos + 1, 1) == "/") {
              lngPos = lngPos + 1
              NextCommand = "//"
            //since block commands are removed, check for the in order to provide a
            //meaningful error message
            ElseIf Mid(strCurrentLine, lngPos + 1, 1) == "*") {
              lngPos = lngPos + 1
              NextCommand = "/*"
            } //End If
            return  //Exit Function
          case 124 //|
            //special case; "|" and "||" returned as separate commands
            if (Mid(strCurrentLine, lngPos + 1, 1) == "|") {
              //increment pointer
              lngPos = lngPos + 1
              //return double //|//
              NextCommand = "||"
            } //End If
            return  //Exit Function
          case 38 //&
            //special case; "&" and "&&" returned as separate commands
            if (Mid(strCurrentLine, lngPos + 1, 1) == "&") {
              //increment pointer
              lngPos = lngPos + 1
              //return double //&//
              NextCommand = "&&"
            } //End If
            return  //Exit Function
          } 

          //if not a text string,
          if (!blnInQuotes) {
            //continue adding characters until element separator or EOL is reached
            Do Until lngPos + 1 > Len(strCurrentLine)
              intChar = AscW(Mid(strCurrentLine, lngPos + 1, 1))
              switch (intChar
              case 32, 33, 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, 123 To 126
                //  space, !"&//() *+,-/:;<=>?[\]^`{|}~
                //end of command text found;
                Exit Do
              default:
                //add character
                NextCommand = NextCommand & ChrW$(intChar)
                //incrmeent position
                lngPos = lngPos + 1
              } 
            Loop

          Else
            //if past end of line
            //(which could only happen if a line contains a single double quote on it)
            if (lngPos + 1 > Len(strCurrentLine)) {
              //return the single quote
              return  //Exit Function
            } //End If

            //add characters until another TRUE quote is found
            Do
              //reset pointer to next
              intChar = AscW(Mid(strCurrentLine, lngPos + 1, 1))
              //increment position
              lngPos = lngPos + 1

              //if last char was a slash, need to treat this next
              //character as special
              if (blnSlash) {
                //next char is just added as-is;
                //no checking it
                //always reset  the slash
                blnSlash = false
              Else
                //regular char; check for slash or quote mark
                switch (intChar
                case 34 //quote mark
                  //a quote marks end of string
                  blnInQuotes = false
                case 92 //slash
                  blnSlash = true
                } 
              } //End If

              NextCommand = NextCommand & ChrW$(intChar)

              //if at end of line
              If(lngPos = Len(strCurrentLine))) {
                //if still in quotes,
                if (blnInQuotes) {
                  //set inquotes to false to exit the loop
                  //the compiler will have to recognize that
                  //this text string is not properly enclosed in quotes
                  blnInQuotes = false
                } //End If
              } //End If
            Loop While blnInQuotes
          } //End If
        return  //Exit Function

        ErrHandler:
          strError = Err.Description
          strErrSrc = Err.Source
          lngError = Err.Number

          //if error is an app specific error, just pass it along; otherwise create
          //an app specific error to encapsulate whatever happened
          If(lngError && vbObjectError) = vbObjectError) {
            //pass it along
            On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
          Else
            On Error GoTo 0: Err.Raise vbObjectError + 657, strErrSrc, Replace(LoadResString(657), ARG1, CStr(lngError) & ":" & strError)
          } //End If
        } //End Function
        static internal Function CompileIf() As bool
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

          Dim strTestCmd As string, strArg As string
          Dim bytTestCmd As byte, bytArg(7) As byte
          Dim lngArg As int, lngWord() As int
          Dim intWordCount As int
          Dim i As int
          Dim blnIfBlock As bool //command block, not a comment block
          Dim blnNeedNextCmd As bool
          Dim intNumTestCmds As int
          Dim intNumCmdsInBlock As int
          Dim blnNOT As bool


          On Error GoTo ErrHandler


          blnIfBlock = false
          intNumTestCmds = 0
          intNumCmdsInBlock = 0
          blnNeedNextCmd = true

          //write out starting if byte
          tmpLogRes.Writebyte &HFF

          //next character should be "("
          if (NextChar() != "(") {
            blnError = true
            strErrMsg = LoadResString(4002)
            return  //Exit Function
          } //End If

          //now, step through input, until final //)//// is found:
          Do
            //get next command
            strTestCmd = NextCommand()
            //check for end of input,
            if (lngLine == -1) {
              blnError = true
              strErrMsg = LoadResString(4106)
              return  //Exit Function
            } //End If

            //if awaiting a test command,
            if (blnNeedNextCmd) {
              switch (strTestCmd
              case "(" //open paran
                //if already in a block
                if (blnIfBlock) {
                  blnError = true
                  strErrMsg = LoadResString(4045)
                  return  //Exit Function
                } //End If
                //write //or// block start
                tmpLogRes.Writebyte &HFC
                blnIfBlock = true
                intNumCmdsInBlock = 0
              case ")"
                //if a test command is expected, //)// always causes error
                if (intNumTestCmds == 0) {
                  strErrMsg = LoadResString(4057)
                ElseIf blnIfBlock && intNumCmdsInBlock == 0) {
                  strErrMsg = LoadResString(4044)
                Else
                  strErrMsg = LoadResString(4056)
                } //End If
                blnError = true
                return  //Exit Function
              default:
                //check for NOT
                blnNOT = (strTestCmd = NOT_TOKEN)
                if (blnNOT) {
                  tmpLogRes.Writebyte &HFD
                  //read in next test command
                  strTestCmd = NextCommand()
                  //check for end of input,
                  if (lngLine == -1) {
                    blnError = true
                    strErrMsg = LoadResString(4106)
                    return  //Exit Function
                  } //End If
                } //End If
                bytTestCmd = CommandNum(true, strTestCmd)
                //if command not found,
                if (bytTestCmd == 255) {
                  //check for special syntax
                  if (!CompileSpecialIf(strTestCmd, blnNOT)) {
                    //error; the CompileSpecialIf function
                    //sets the error codes, and CompileLogic will
                    //call the error handler
                    return  //Exit Function
                  } //End If
                Else
                  //write the test command code
                  tmpLogRes.Writebyte bytTestCmd
                  //next command should be "("
                  if (NextChar() != "(") {
                    blnError = true
                    strErrMsg = LoadResString(4048)
                    return  //Exit Function
                  } //End If

                  //check for return.false() command
                  if (bytTestCmd == 0) {
                    //warn user that it//s not compatible with AGI Studio
                    switch (agMainLogSettings.ErrorLevel
                    case leHigh, leMedium
                      //generate warning
                      AddWarning 5081
                    case leLow
                    } 
                  } //End If

                  //if said command
                  if (bytTestCmd == &HE) {
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
                      } //End If
                      //exit
                      return  //Exit Function
                    } //End If

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
                        return  //Exit Function
                      } //End If

                      //get next character
                      //(should be a comma, or close parenthesis, if no more words)
                      strArg = NextChar()
                      if (LenB(strArg) != 0) {
                        switch (AscW(strArg)
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
                            } //End If
                            return  //Exit Function
                          } //End If


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
                          Else
                            //use 1-base arg values
                            strErrMsg = Replace(LoadResString(4047), ARG1, CStr(intWordCount + 1))
                          } //End If
                          return  //Exit Function
                        } 
                      Else
                      //////Debug.Assert false
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
                        Else
                          //use 1-base arg values
                          strErrMsg = Replace(LoadResString(4047), ARG1, CStr(intWordCount + 1))
                        } //End If
                        return  //Exit Function
                      } //End If
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
                  Else
                    //not //said//; extract arguments for this command
                    For i = 0 To agTestCmds(Cbyte(bytTestCmd)).ArgType.Length - 1
                      //after first argument, verify comma separates arguments
                      if (i > 0) {
                        if (NextChar(true) != ",") {
                          blnError = true
                          //use 1-base arg values
                          strErrMsg = Replace(LoadResString(4047), ARG1, CStr(i + 1))
                          return  //Exit Function
                        } //End If
                      } //End If

                      //reset the quotemark error flag after comma is found
                      lngQuoteAdded = -1
                      bytArg(i) = GetNextArg(agTestCmds(Cbyte(bytTestCmd)).ArgType(i), i)
                      //if error
                      if (blnError) {
                        // if error number is 4054
                        if (Val(strErrMsg) == 4054) {
                          // add command name to error string
                          strErrMsg = Replace(strErrMsg, ARG2, agTestCmds(bytTestCmd).Name)
                        } //End If
                        return  //Exit Function
                      } //End If
                      //write argument
                      tmpLogRes.Writebyte bytArg(i)
                    Next i
                  } //End If
                  //next character should be ")"
                  if (NextChar() != ")") {
                    blnError = true
                    strErrMsg = LoadResString(4160)
                    return  //Exit Function
                  } //End If
                  //reset the quotemark error flag
                  lngQuoteAdded = -1

                  //validate arguments for this command
                  if (!ValidateIfArgs(bytTestCmd, bytArg())) {
                    //error assigned by called function
                    return  //Exit Function
                  } //End If
                } //End If

                //command added
                intNumTestCmds = intNumTestCmds + 1
                //if in IF block,
                if (blnIfBlock) {
                  intNumCmdsInBlock = intNumCmdsInBlock + 1
                } //End If
                //toggle off need for test command
                blnNeedNextCmd = false
              } 
            Else //not awaiting a test command
              switch (strTestCmd
              case NOT_TOKEN
                //invalid
                blnError = true
                strErrMsg = LoadResString(4097)
                return  //Exit Function
              case AND_TOKEN
                //if inside brackets
                if (blnIfBlock) {
                  blnError = true
                  strErrMsg = LoadResString(4037)
                  return  //Exit Function
                } //End If
                blnNeedNextCmd = true
              case OR_TOKEN
                //if NOT inside brackets
                if (!blnIfBlock) {
                  blnError = true
                  strErrMsg = LoadResString(4100)
                  return  //Exit Function
                } //End If
                blnNeedNextCmd = true
              case ")"
                //if inside brackets
                if (blnIfBlock) {
                  //ensure at least one command in block,
                  if (intNumCmdsInBlock == 0) {
                    blnError = true
                    strErrMsg = LoadResString(4044)
                    return  //Exit Function
                  } //End If
                  //close brackets
                  blnIfBlock = false
                  tmpLogRes.Writebyte &HFC
                Else
                  //ensure at least one command in block,
                  if (intNumTestCmds == 0) {
                    blnError = true
                    strErrMsg = LoadResString(4044)
                    return  //Exit Function
                  } //End If
                  //end of if found
                  Exit Do
                } //End If
              default:
                if (blnIfBlock) {
                  strErrMsg = LoadResString(4101)
                Else
                  strErrMsg = LoadResString(4038)
                } //End If
                blnError = true
                return  //Exit Function
              } 
            } //End If
          //never leave loop normally; error, end of input, or successful
          //compilation of test commands will all exit loop directly
          Loop While true

          //write ending if byte
          tmpLogRes.Writebyte &HFF
          //return true
          CompileIf = true
        return  //Exit Function

        ErrHandler:
          blnError = true
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "CompileIf")
          //err.clear
        } //End Function
        static internal Function ConcatArg(strText As string) As string
          //this function concatenates strings; it assumes strText
          //is the string that was just read into the compiler;
          //it then checks if there are additional elements of
          //this string to add to it; if so, they are added.
          //it returns the completed string, with lngPos and lngLine
          //set accordingly (if there is nothing to concatenate, it
          //returns original string)
          //NOTE: input string has already been checked for starting
          //and ending quotation marks


          Dim strTextContinue As string
          Dim lngLastPos As int, lngLastLine As int
          Dim strLastLine As string
          Dim lngSlashCount As int, lngQuotesOK As int


          On Error GoTo ErrHandler

          //verify at least two characters
          if (Len(strText) < 2) {
            //error
            blnError = true
            strErrMsg = LoadResString(4081)
            return  //Exit Function
          } //End If

          //start with input string
          ConcatArg = strText

          //save current position info
          lngLastPos = lngPos
          lngLastLine = lngLine
          strLastLine = strCurrentLine

          //if at end of last line
          if (lngLastPos == Len(strLastLine)) {
            //get next command
            strTextContinue = NextCommand()

            //add strings until concatenation is complete
            Do Until Left(strTextContinue, 1) != QUOTECHAR
              //if a continuation string is found, we need to reset
              //the quote checker
              lngQuotesOK = 0

              //check for end quote
              if (Right(strTextContinue, 1) != QUOTECHAR) {
                //bad end quote (set end quote marker, overriding error
                //that might happen on a previous line)
                lngQuotesOK = 2
              Else
                //just because it ends in a quote doesn//t mean it//s good;
                //it might be an embedded quote
                //(we know we have at least two chars, so we don//t need
                //to worry about an error with MID function)

                //check for an odd number of slashes immediately preceding
                //this quote
                lngSlashCount = 0
                Do
                  if (Mid(ConcatArg, Len(ConcatArg) - (lngSlashCount + 1), 1) == "\") {
                    lngSlashCount = lngSlashCount + 1
                  Else
                    Exit Do
                  } //End If
                Loop While Len(ConcatArg) - (lngSlashCount + 1) >= 0

                //if it IS odd, then it//s not a valid quote
                if (Int(lngSlashCount / 2) != lngSlashCount / 2) {
                  //it//s embedded, and doesn//t count
                  //bad end quote (set end quote marker, overriding error
                  //that might happen on a previous line)
                  lngQuotesOK = 2
                } //End If
              } //End If

              //if end quote is missing, deal with it
              if (lngQuotesOK > 0) {
                //note which line had quotes added, in case it results
                //in an error caused by a missing end //)// or whatever
                //the next required element is
                lngQuoteAdded = lngLine

                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  //error
                  blnError = true
                  strErrMsg = LoadResString(4080)
                  return  //Exit Function
                case leMedium
                  //add quote
                  strTextContinue = strTextContinue & QUOTECHAR
                  //set warning
                  AddWarning 5002
                case leLow
                  //add quote
                  strTextContinue = strTextContinue & QUOTECHAR
                } 
              } //End If

              //strip off ending quote of current msg
              ConcatArg = Left(ConcatArg, Len(ConcatArg) - 1)
              //add it to strText
              ConcatArg = ConcatArg & Right(strTextContinue, Len(strTextContinue) - 1)
              //save current position info
              lngLastPos = lngPos
              lngLastLine = lngLine
              strLastLine = strCurrentLine
              //get next command
              strTextContinue = NextCommand()
            Loop

            //after end of string found, move back to correct position
            lngPos = lngLastPos
            lngLine = lngLastLine
            lngErrLine = lngLastLine
            strCurrentLine = strLastLine
          } //End If


        return  //Exit Function

        ErrHandler:
          //raise an error
          blnError = true
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "ConcatArg")
        } //End Function


        static internal Function RemoveComments() As bool
          //this function strips out comments from the input text
          //and trims off leading and trailing spaces
          //
          //agi comments:
          //      // - rest of line is ignored
          //      [ - rest of line is ignored


          Dim lngPos As int
          Dim blnInQuotes As bool, blnSlash As bool
          Dim intROLIgnore As int


          On Error GoTo ErrHandler

          //reset compiler
          ResetCompiler
          lngIncludeOffset = 0
          lngLine = 0

          Do
            //reset rol ignore
            intROLIgnore = 0

            //reset comment start & char ptr, and inquotes
            lngPos = 0
            blnInQuotes = false

            //if this line is not empty,
            if (LenB(strCurrentLine) != 0) {
              Do Until lngPos >= Len(strCurrentLine)
                //get next character from string
                lngPos = lngPos + 1
                //if NOT inside a quotation,
                if (!blnInQuotes) {
                  //check for comment characters at this position
                  If((Mid(strCurrentLine, lngPos, 2) = CMT2_TOKEN) Or(Mid(strCurrentLine, lngPos, 1) = CMT1_TOKEN))) {
                  intROLIgnore = lngPos
                    Exit Do
                  } //End If
                  // slash codes never occur outside quotes
                  blnSlash = false
                  //if this character is a quote mark, it starts a string
                  blnInQuotes = (AscW(Mid(strCurrentLine, lngPos)) = 34)
                Else
                  //if last character was a slash, ignore this character
                  //because it//s part of a slash code
                  if (blnSlash) {
                    //always reset  the slash
                    blnSlash = false
                  Else
                    //check for slash or quote mark
                    switch (AscW(Mid(strCurrentLine, lngPos))
                    case 34 //quote mark
                      //a quote marks end of string
                      blnInQuotes = false
                    case 92 //slash
                      blnSlash = true
                    } 
                  } //End If
                } //End If
              Loop
              //if any part of line should be ignored,
              if (intROLIgnore > 0) {
                strCurrentLine = Left(strCurrentLine, intROLIgnore - 1)
              } //End If
            } //End If
            //replace comment, also trim it
            ReplaceLine Trim$(strCurrentLine)

            //get next line
            IncrementLine
          Loop Until lngLine = -1

          //success
          RemoveComments = true
        return  //Exit Function
        ErrHandler:
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "RemoveComments")
          Err.Clear
        } //End Function

        static internal Function AddIncludes(stlLogicText As string[]) As bool
          //this function uses the logic text that is passed to the compiler
          //to create the input text that is parsed.
          //it copies the lines from the logic text to the input text, and
          //replaces any include file lines with the actual lines from the
          //include file (include file lines are given a //header// to identify
          //them as include lines)


          Dim IncludeLines As string[]
          Dim strIncludeFilename As string
          Dim strIncludePath As string
          Dim strIncludeText As string
          Dim CurIncludeLine As int   // current line in IncludeLines (the include file)
          Dim intFileCount As int
          Dim i As int
          Dim intFile As int
          Dim lngLineCount As int


          On Error GoTo ErrHandler


          Set stlInput = New StringList
          Set IncludeLines = New StringList //only temporary,


          lngLine = 0
          lngLineCount = stlLogicText.Count

          //module is always main module
          strModule = ""
          strModFileName = ""

          //step through all lines
          Do
            //set errline
            lngErrLine = lngLine
            //check this line for include statement
            if (Left(stlLogicText(lngLine), 8) == "#include") {
              //proper format requires a space after //include//
              if (Mid(stlLogicText(lngLine), 9, 1) != " ") {
                //generate error
                strErrMsg = LoadResString(4103)
                return  //Exit Function
              } //End If
              //build include filename
              strIncludeFilename = Trim$(Right(stlLogicText(lngLine), Len(stlLogicText(lngLine)) - 9))

              //check for a filename
              if (LenB(strIncludeFilename) == 0) {
                strErrMsg = LoadResString(4060)
                return  //Exit Function
              } //End If

              //if quotes aren//t used correctly
              if (Left(strIncludeFilename, 1) == QUOTECHAR && Right(strIncludeFilename, 1) != QUOTECHAR || _
                 Right(strIncludeFilename, 1) == QUOTECHAR && Left(strIncludeFilename, 1) != QUOTECHAR) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  //return error: improper use of quote marks
                  strErrMsg = LoadResString(4059)
                  return  //Exit Function
                case leMedium, leLow
                  //assume quotes are needed
                  if (AscW(strIncludeFilename) != 34) {
                    strIncludeFilename = QUOTECHAR & strIncludeFilename
                  } //End If
                  if (AscW(Right(strIncludeFilename, 1)) != 34) {
                    strIncludeFilename = strIncludeFilename & QUOTECHAR
                  } //End If
                  //set warning
                  AddWarning 5028, Replace(LoadResString(5028), ARG1, strIncludeFilename)
                } 
              } //End If

              //if quotes,
              if (Left(strIncludeFilename, 1) == QUOTECHAR) {
                //strip off quotes
                strIncludeFilename = Mid(strIncludeFilename, 2, Len(strIncludeFilename) - 2)
              } //End If

              //if filename doesnt include a path,
              if (LenB(JustPath(strIncludeFilename, true)) == 0) {
                //get full path name to include file
                strIncludeFilename = agResDir & strIncludeFilename
              } //End If

              //verify file exists
              if (!FileExists(strIncludeFilename)) {
                strErrMsg = Replace(LoadResString(4050), ARG1, strIncludeFilename)
                return  //Exit Function
              } //End If
        //****
        //      cant check for open includes; they are in a different application
        //****

              On Error Resume Next
              //now open the include file, and get the text
              intFile = FreeFile()
              Open strIncludeFilename For Binary As intFile
              strIncludeText = string$(LOF(intFile), 0)
              Get intFile, 1, strIncludeText
              Close intFile
              //check for error,
              if (Err.Number<> 0) {
                strErrMsg = Replace(LoadResString(4055), ARG1, strIncludeFilename)
                return  //Exit Function
              } //End If


              On Error GoTo ErrHandler

              //assign text to stringlist
              Set IncludeLines = New StringList
              IncludeLines.Assign strIncludeText

              //if there are any lines,
              if (IncludeLines.Count > 0) {
                //save file name to allow for error checking
                intFileCount = intFileCount + 1
                ReDim Preserve strIncludeFile(intFileCount)
                strIncludeFile(intFileCount) = strIncludeFilename

                //add all these lines into this position
                For CurIncludeLine = 0 To IncludeLines.Count - 1
                  //verify the include file contains no includes
                  if (Left(Trim$(IncludeLines(CurIncludeLine)), 2) == "#i") {
                    strErrMsg = LoadResString(4061)
                    lngErrLine = CurIncludeLine
                    return  //Exit Function
                  } //End If
                  //include filenumber and line number from includefile
                  stlInput.Add "#I" & CStr(intFileCount) & ":" & CStr(CurIncludeLine) & "#" & IncludeLines(CurIncludeLine)
                Next CurIncludeLine
              } //End If
              //add a blank line as a place holder for the //include// line
              //(to keep line counts accurate when calculating line number for errors)
              stlInput.Add ""
            Else
              //not an include line
              //check for any instances of #I, since these will
              //interfere with include line handling
              if (Left(stlLogicText(lngLine), 2) == "#i") {
                strErrMsg = LoadResString(4069)
                return  //Exit Function
              } //End If
              //copy the line by itself
              stlInput.Add stlLogicText(lngLine)
            } //End If
            lngLine = lngLine + 1
          Loop Until lngLine >= lngLineCount
          //done
          Set IncludeLines = Nothing
          //return success
          AddIncludes = true
        return  //Exit Function

        ErrHandler:
          //unknown error
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "AddIncludes")
          Err.Clear
        } //End Function



        static internal Function ReadDefines() As bool


          Dim i As int, j As int
          Dim blnInQuote As bool
          Dim tdNewDefine As TDefine
          Dim rtn As int


          On Error GoTo ErrHandler

          //reset Count of defines
          lngDefineCount = 0

          //reset compiler
          ResetCompiler

          //reset error string
          strErrMsg = ""

          //step through all lines and find define values
          Do
            //check for define statement
            if (InStr(1, strCurrentLine, CONST_TOKEN) == 1) {
              //strip off define keyword
              strCurrentLine = Trim$(Right(strCurrentLine, Len(strCurrentLine) - Len(CONST_TOKEN)))

              //if equal marker (i.e. space) not present
              if (InStr(1, strCurrentLine, " ") == 0) {
                //error
                strErrMsg = LoadResString(4104)
                return  //Exit Function
              } //End If

              //split it by position of first space
              tdNewDefine.Name = Trim$(Left(strCurrentLine, InStr(1, strCurrentLine, " ") - 1))
              tdNewDefine.Value = Trim$(Right(strCurrentLine, Len(strCurrentLine) - InStr(1, strCurrentLine, " ")))

              //validate define name
              rtn = ValidateDefName(tdNewDefine.Name)
              //name error 7-12  are only warnings if error level is medium or low
              switch (agMainLogSettings.ErrorLevel
              case leMedium
                //check for name warnings
                switch (rtn
                case 7
                  //set warning
                  AddWarning 5034, Replace(LoadResString(5034), ARG1, tdNewDefine.Name)
                  //reset return error code
                  rtn = 0

                case 8 To 12
                  //set warning
                  AddWarning 5035, Replace(LoadResString(5035), ARG1, tdNewDefine.Name)
                  //reset return error code
                  rtn = 0
                } 
              case leLow
                //check for warnings
                if (rtn >= 7 && rtn <= 12) {
                  //reset return error code
                  rtn = 0
                } //End If
              } 

              //check for errors
              if (rtn<> 0) {
                //check for name errors
                switch (rtn
                case 1 // no name
                  strErrMsg = LoadResString(4070)
                case 2 // name is numeric
                  strErrMsg = LoadResString(4072)
                case 3 // name is command
                  strErrMsg = Replace(LoadResString(4021), ARG1, tdNewDefine.Name)
                case 4 // name is test command
                  strErrMsg = Replace(LoadResString(4022), ARG1, tdNewDefine.Name)
                case 5 // name is a compiler keyword
                  strErrMsg = Replace(LoadResString(4013), ARG1, tdNewDefine.Name)
                case 6 // name is an argument marker
                  strErrMsg = LoadResString(4071)
                case 7 // name is already globally defined
                  strErrMsg = Replace(LoadResString(4019), ARG1, tdNewDefine.Name)
                case 8 // name is reserved variable name
                  strErrMsg = Replace(LoadResString(4018), ARG1, tdNewDefine.Name)
                case 9 // name is reserved flag name
                  strErrMsg = Replace(LoadResString(4014), ARG1, tdNewDefine.Name)
                case 10 // name is reserved number constant
                  strErrMsg = Replace(LoadResString(4016), ARG1, tdNewDefine.Name)
                case 11 // name is reserved object constant
                  strErrMsg = Replace(LoadResString(4017), ARG1, tdNewDefine.Name)
                case 12 // name is reserved message constant
                  strErrMsg = Replace(LoadResString(4015), ARG1, tdNewDefine.Name)
                case 13 // name contains improper character
                  strErrMsg = LoadResString(4067)
                } 
                //don//t exit; check for define Value errors first
              } //End If

              //validate define Value
              rtn = ValidateDefValue(tdNewDefine)
              //Value errors 4,5,6 are only warnings if error level is medium or low
              switch (agMainLogSettings.ErrorLevel
              case leMedium
                //if Value error is due to missing quotes
                switch (rtn
                case 4  //string Value missing quotes
                  //fix the define Value
                  if (AscW(tdNewDefine.Value) != 34) {
                    tdNewDefine.Value = QUOTECHAR & tdNewDefine.Value
                  } //End If
                  if (AscW(Right(tdNewDefine.Value, 1)) != 34) {
                    tdNewDefine.Value = tdNewDefine.Value & QUOTECHAR
                  } //End If

                  //set warning
                  AddWarning 5022
                  //reset error code
                  rtn = 0
                case 5 // Value is already defined by a reserved name
                  //set warning
                  AddWarning 5032, Replace(LoadResString(5032), ARG1, tdNewDefine.Value)
                  //reset error code
                  rtn = 0

                case 6 // Value is already defined by a global name
                  //set warning
                  AddWarning 5031, Replace(LoadResString(5031), ARG1, tdNewDefine.Value)
                  //reset error code
                  rtn = 0
                } 
              case leLow
                //if Value error is due to missing quotes
                switch (rtn
                case 4
                  //fix the define Value
                  if (AscW(tdNewDefine.Value) != 34) {
                    tdNewDefine.Value = QUOTECHAR & tdNewDefine.Value
                  } //End If
                  if (AscW(Right(tdNewDefine.Value, 1)) != 34) {
                    tdNewDefine.Value = tdNewDefine.Value & QUOTECHAR
                  } //End If
                  //reset return Value
                  rtn = 0
                case 5, 6
                  //reset return Value
                  rtn = 0
                } 
              } 

              //check for errors
              if (rtn != 0) {
                //if already have a name error
                if (LenB(strErrMsg) != 0) {
                  //append Value error
                  strErrMsg = strErrMsg & "; and "
                } //End If

                //check for Value error
                switch (rtn
                case 1 // no Value
                  strErrMsg = strErrMsg & LoadResString(4073)
        //
        //a return Value of 2 is no longer possible; this
        //Value has been removed from the ValidateDefineValue function
        //        case 2 // Value is an invalid argument marker
        //          strErrMsg = strErrMsg & "4065: Invalid argument declaration Value"
                case 3 // Value contains an invalid argument Value
                  strErrMsg = strErrMsg & LoadResString(4042)
                case 4 // Value is not a string, number or argument marker
                  strErrMsg = strErrMsg & LoadResString(4082)
                case 5 // Value is already defined by a reserved name
                  strErrMsg = strErrMsg & Replace(LoadResString(4041), ARG1, tdNewDefine.Value)
                case 6 // Value is already defined by a global name
                  strErrMsg = strErrMsg & Replace(LoadResString(4040), ARG1, tdNewDefine.Value)
                } 
              } //End If

              //if an error was generated during define validation
              if (LenB(strErrMsg) != 0) {
                return  //Exit Function
              } //End If

              //check all previous defines
              For i = 0 To lngDefineCount - 1
                if (tdNewDefine.Name == tdDefines(i).Name) {
                  strErrMsg = Replace(LoadResString(4012), ARG1, tdDefines(i).Name)
                  return  //Exit Function
                } //End If
                if (tdNewDefine.Value == tdDefines(i).Value) {
                  //numeric duplicates aren//t a problem
                  if (!IsNumeric(tdNewDefine.Value)) {
                    switch (agMainLogSettings.ErrorLevel
                    case leHigh
                      //set error
                      strErrMsg = Replace(Replace(LoadResString(4023), ARG1, tdDefines(i).Value), ARG2, tdDefines(i).Name)
                      return  //Exit Function
                    case leMedium
                      //set warning
                      AddWarning 5033, Replace(Replace(LoadResString(5033), ARG1, tdNewDefine.Value), ARG2, tdDefines(i).Name)
                    case leLow
                      //do nothing
                    } 
                  } //End If
                } //End If
              Next i

              //check define against labels
              if (bytLabelCount > 0) {
                For i = 1 To bytLabelCount
                  if (tdNewDefine.Name == llLabel(i).Name) {
                    strErrMsg = Replace(LoadResString(4020), ARG1, tdNewDefine.Name)
                    return  //Exit Function
                  } //End If
                Next i
              } //End If

              //save this define
              ReDim Preserve tdDefines(lngDefineCount)
              tdDefines(lngDefineCount) = tdNewDefine

              //increment counter
              lngDefineCount = lngDefineCount + 1

              //now set this line to empty so Compiler doesn"t try to read it
              if (Left(stlInput(lngLine), 2) == "#I") {
                //this is an include line; need to leave include line info
                stlInput(lngLine) = Left(stlInput(lngLine), InStr(4, stlInput(lngLine), "#"))
              Else
                //just blank out entire line
                stlInput(lngLine) = ""
              } //End If
            } //End If
            //get next line
            IncrementLine
          Loop Until lngLine = -1


          ReadDefines = true
        return  //Exit Function

        ErrHandler:
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "ReadDefines")
          Err.Clear
        } //End Function
        static internal Function ReadMsgs() As bool

          //note that stripped message lines also strip out the include header string
          //this doesn//t matter since they are only blank lines anyway
          //only need include header info if error occurs, and errors never occur on
          //blank line

          Dim intMsgNum As int, i As int
          Dim strCmd As string, strMsgContinue As string
          Dim strMsgSep As string
          Dim lngMsgStart As int, blnDef As bool
          Dim intMsgLineCount As int
          Dim lngSlashCount As int, lngQuotesOK As int


          On Error GoTo ErrHandler

          //build blank message list
          For intMsgNum = 0 To 255
            strMsg(intMsgNum) = ""
            blnMsg(intMsgNum) = false
            intMsgWarn(intMsgNum) = 0
          Next intMsgNum

          //reset compiler
          ResetCompiler

          Do
            //get first command on this line
            strCmd = NextCommand(true)

            //if this is the message marker
            if (strCmd == MSG_TOKEN) {
              //save starting line number (incase this msg uses multiple lines)
              lngMsgStart = lngLine

              //get next command on this line
              strCmd = NextCommand(true)

              //this should be a msg number
              if (!IsNumeric(strCmd)) {
                //error
                blnError = true
                strErrMsg = LoadResString(4077)
                return  //Exit Function
              } //End If

              //validate msg number
              intMsgNum = VariableValue(strCmd)
              if (intMsgNum <= 0) {
                //error
                blnError = true
                strErrMsg = LoadResString(4077)
                return  //Exit Function
              } //End If
              //if msg is already assigned
              if (blnMsg(intMsgNum)) {
                blnError = true
                strErrMsg = Replace(LoadResString(4094), ARG1, CStr(intMsgNum))
                return  //Exit Function
              } //End If

              //get next string command
              strCmd = NextCommand(false)

              //is this a valid string?
              if (!IsValidMsg(strCmd)) {
                //maybe it//s a define
                if (ConvertArgument(strCmd, atMsg)) {
                  //defined strings never get concatenated
                  blnDef = true
                } //End If
              } //End If

              //always reset the //addquote// flag
              //(this is the flag that notes if/where a line had an end quote
              //added by the compiler; if this causes problems later in the
              //compilation of this command, we can then use mark this error
              //as the culprit
              lngQuoteAdded = -1

              //check msg for quotes (note ending quote has to be checked to make sure it//s not
              //an embedded quote)
              //assume OK until we learn otherwise (0=OK; 1=bad start quote; 2=bad end quote; 3=bad both)
              lngQuotesOK = 0
              if (Left(strCmd, 1) != QUOTECHAR) {
                //bad start quote
                lngQuotesOK = 1
              } //End If
              //check for end quote
              if (Right(strCmd, 1) != QUOTECHAR) {
                //bad end quote
                lngQuotesOK = lngQuotesOK + 2
              Else
                //just because it ends in a quote doesn//t mean it//s good;
                //it might be an embedded quote
                //(we know we have at least two chars, so we don//t need
                //to worry about an error with MID function)

                //check for an odd number of slashes immediately preceding
                //this quote
                lngSlashCount = 0
                Do
                  if (Mid(strCmd, Len(strCmd) - (lngSlashCount + 1), 1) == "\") {
                    lngSlashCount = lngSlashCount + 1
                  Else
                    Exit Do
                  } //End If
                Loop While Len(strCmd) - (lngSlashCount + 1) >= 0

                //if it IS odd, then it//s not a valid quote
                if (Int(lngSlashCount / 2) != lngSlashCount / 2) {
                  //it//s embedded, and doesn//t count
                  lngQuotesOK = lngQuotesOK + 2
                } //End If
              } //End If

              //if either (or both) quote is missing, deal with it
              if (lngQuotesOK > 0) {
                //note which line had quotes added, in case it results
                //in an error caused by a missing end //)// or whatever
                //the next required element is
                lngQuoteAdded = lngLine

                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  blnError = true
                  strErrMsg = LoadResString(4051)
                  return  //Exit Function
                case leMedium, leLow
                  //add quotes as appropriate
                  if (AscW(strCmd) != 34) {
                    strCmd = QUOTECHAR & strCmd
                  } //End If
                  if (AscW(Right(strCmd, 1)) != 34) {
                    strCmd = strCmd & QUOTECHAR
                  } //End If
                  //warn if medium
                  if (agMainLogSettings.ErrorLevel == leMedium) {
                    //set warning
                    AddWarning 5002
                  } //End If
                } 
              } //End If

              //concatenate, if necessary
              if (!blnDef) {
                strCmd = ConcatArg(strCmd)
                //if error,
                if (blnError) {
                  return  //Exit Function
                } //End If
              } //End If

              //nothing allowed after msg declaration
              if (lngPos != Len(strCurrentLine)) {
                //error
                blnError = true
                strErrMsg = LoadResString(4099)
                return  //Exit Function
              } //End If

              //strip off quotes (we know that the string
              //is properly enclosed by quotes because
              //ConcatArg function validates they are there
              //or adds them if they aren//t[or raises an
              //error, in which case it doesn//t even matter])
              strCmd = Mid(strCmd, 2, Len(strCmd) - 2)

              //add msg
              strMsg(intMsgNum) = strCmd
              //validate message characters
              if (!ValidateMsgChars(strCmd, intMsgNum)) {
                //error was raised
                return  //Exit Function
              } //End If


              blnMsg(intMsgNum) = true

              Do
                //set this line to empty so compiler doesn//t try to read it
                stlInput(lngMsgStart) = ""
                //increment the counter (to get multiple lines, if string is
                //concatenated over more than one line)
                lngMsgStart = lngMsgStart + 1
              //continue until back to current line
              Loop Until lngMsgStart > lngLine

            } //End If
            //get next line
            IncrementLine
          Loop Until lngLine = -1

          //check for any undeclared messages that haven//t already been identified
          //(they are not really a problem, but user might want to know)
          //all messages from 1 to this point should be declared
          intMsgNum = 255
          Do Until blnMsg(intMsgNum) || intMsgNum = 0
            intMsgNum = intMsgNum - 1
          Loop

          ReadMsgs = true
        return  //Exit Function

        ErrHandler:
          blnError = true
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "ReadMsgs")
          Err.Clear
        } //End Function

        static internal Sub ReplaceLine(strNewLine As string)
          //this function replaces the current line in the input string
          //with the strNewLine, while preserving include header info

          Dim strInclude As string

            //if this is from an include file
            if (Left(stlInput(lngLine), 2) == "#I") {
              //need to save include header info so it can
              //be preserved after comments are removed
              strInclude = Left(stlInput(lngLine), InStr(2, stlInput(lngLine), "#"))
            Else
              strInclude = ""
            } //End If

            //replace the line
            stlInput(lngLine) = strInclude & strNewLine
        } //End Sub


        static internal Sub ResetCompiler()
          //resets the compiler so it points to beginning of input
          //also loads first line into strCurrentLine

          //reset include offset, so error trapping
          //can correctly Count lines
          lngIncludeOffset = 0

          //reset error flag
          blnError = false
          //reset the quotemark error flag
          lngQuoteAdded = -1

          //set line pointer to -2 so first call to
          //IncrementLine gets first line
          lngLine = -2

          //get first line
          IncrementLine
          //NOTE: don//t need to worry about first line;
          //compiler has already verified the input has at least one line
        } //End Sub


        internal Sub SetResourceIDs()
          //builds array of resourceIDs so
          //convertarg function can iterate through them much quicker

          Dim tmpLog As AGILogic, tmpPic As AGIPicture
          Dim tmpSnd As AGISound, tmpView As AGIView


          if (blnSetIDs) {
            return;
          } //End If

          ReDim strLogID(255)
          ReDim strPicID(255)
          ReDim strSndID(255)
          ReDim strViewID(255)


          For Each tmpLog In agLogs
            strLogID(tmpLog.Number) = tmpLog.ID
          Next


          For Each tmpPic In agPics
            strPicID(tmpPic.Number) = tmpPic.ID
          Next


          For Each tmpSnd In agSnds
            strSndID(tmpSnd.Number) = tmpSnd.ID
          Next


          For Each tmpView In agViews
            strViewID(tmpView.Number) = tmpView.ID
          Next

          //set flag
          blnSetIDs = true
        } //End Sub

        static internal Sub AddWarning(WarningNum As int, Optional WarningText As string)

          //warning elements are separated by pipe character
          //WarningsText is in format:
          //  number|warningtext|line|module
          //
          //(number, line and module only have meaning for logic warnings
          // other warnings generated during a game compile will use
          // same format, but use -- for warning number, line and module)

          //if no text passed, use the default resource string

          Dim evWarn As string


          if (Len(WarningText) == 0) {
            WarningText = LoadResString(WarningNum)
          } //End If

          //only add if not ignoring
          if (!agNoCompWarn(WarningNum - 5000)) {
            evWarn = CStr(WarningNum) & "|" & WarningText & "|" & CStr(lngErrLine + 1) & "|" & _
                         IIf(LenB(strModule) != 0, strModule, "")
            agGameEvents.RaiseEvent_LogCompWarning evWarn, bytLogComp
          } //End If
        } //End Sub
        static internal Function ValidateArgs(CmdNum As int, ByRef ArgVal() As byte) As bool


          Dim blnUnload As bool, blnWarned As bool

          //check for specific command issues
          On Error GoTo ErrHandler

          //for commands that can affect variable values, need to check against reserved variables
          //for commands that can affect flags, need to check against reserved flags

          //for other commands, check the passed arguments to see if values are appropriate


          switch (CmdNum
          case 1, 2, 4 To 8, 10, 165 To 168 //increment, decrement, assignv, addn, addv, subn, subv
                                            //rindirect, mul.n, mul.v, div.n, div.v
            //check for reserved variables that should never be manipulated
            //(assume arg Value is zero to avoid tripping other checks)
            CheckResVarUse ArgVal(0), 0

            //for div.n(vA, B) only, check for divide-by-zero
            if (CmdNum == 167) {
              if (ArgVal(1) == 0) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  strErrMsg = LoadResString(4149)
                  return  //Exit Function
                case leMedium
                  AddWarning 5030
                case leLow
                } 
              } //End If
            } //End If


          case 3 //assignn
            //check for actual Value being assigned
            CheckResVarUse ArgVal(0), ArgVal(1)


          case 12, 13, 14 //set, reset, toggle
            //check for reserved flags
            CheckResFlagUse ArgVal(0)


          case 18 //new.room(A)
            //validate that this logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4120)
                return  //Exit Function
              case leMedium
                AddWarning 5053
              case leLow
              } 
            } //End If
            //expect no more commands
            blnNewRoom = true


          case 19 //new.room.v
            //expect no more commands
            blnNewRoom = true


          case 20 //load.logics(A)
            //validate that logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4121), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5013
              case leLow
              } 
            } //End If


          case 22  //call(A)
            //calling logic0 is a BAD idea
            if (ArgVal(0) == 0) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4118)
                return  //Exit Function
              case leMedium
                AddWarning 5010
              case leLow
                //no action
              } 
            } //End If

            //recursive calling is BAD
            if (ArgVal(0) == bytLogComp) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4117)
                return  //Exit Function
              case leMedium
                AddWarning 5089
              case leLow
                //no action
              } 
            } //End If

            //validate that logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4156), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5076
              case leLow
              } 
            } //End If


          case 30 //load.view(A)
            //validate that view exists
            if (!agViews.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4122), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5015
              case leLow
              } 
            } //End If


          case 32 //discard.view(A)
            //validate that view exists
            if (!agViews.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4123), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5024
              case leLow
              } 
            } //End If


          case 37 //position(oA, X,Y)
            //check x/y against limits
            if (ArgVal(1) > 159 || ArgVal(2) > 167) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
               strErrMsg = LoadResString(4128)
               return  //Exit Function
              case leMedium
                AddWarning 5023
              case leLow
              } 
            } //End If


          case 39 //get.posn
            //neither variable arg should be a reserved Value
            if (ArgVal(1) <= 26 || ArgVal(2) <= 26) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If


          case 41 //set.view(oA, B)
            //validate that view exists
            if (!agViews.Exists(ArgVal(1))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4124), ARG1, CStr(ArgVal(1)))
                return  //Exit Function
              case leMedium
                AddWarning 5037
              case leLow
              } 
            } //End If


          case 49 To 53, 97, 118  //last.cel, current.cel, current.loop,
                                  //current.view, number.of.loops, get.room.v
                                  //get.num
            //variable arg is second
            //variable arg should not be a reserved Value
            if (ArgVal(1) <= 26) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If


          case 54 //set.priority(oA, B)
            //check priority Value
            if (ArgVal(1) > 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4125)
                return  //Exit Function
              case leMedium
                AddWarning 5050
              case leLow
              } 
            } //End If


          case 57 //get.priority
            //variable is second argument
            //variable arg should not be a reserved Value
            if (ArgVal(1) <= 26) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If


          case 63 //set.horizon(A)
            //>120 or <16 is unusual
            //>=167 will cause AGI to freeze/crash

            //validate horizon Value
            switch (agMainLogSettings.ErrorLevel
            case leHigh
              if (ArgVal(0) >= 167) {
                strErrMsg = LoadResString(4126)
                return  //Exit Function
              } //End If
              if (ArgVal(0) > 120) {
                AddWarning 5042
              ElseIf ArgVal(0) < 16) {
                AddWarning 5041
              } //End If


            case leMedium
              if (ArgVal(0) >= 167) {
                AddWarning 5043
              ElseIf ArgVal(0) > 120) {
                  AddWarning 5042
              ElseIf ArgVal(0) < 16) {
                AddWarning 5041
              } //End If


            case leLow
            } 


          case 64, 65, 66 //object.on.water, object.on.land, object.on.anything
            //warn if used on ego
            if (ArgVal(0) == 0) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5082
              case leLow
              } 
            } //End If


          case 69 //distance
            //variable is third arg
            //variable arg should not be a reserved Value
            if (ArgVal(2) <= 26) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If


          case 73, 75, 99 //end.of.loop, reverse.loop
            //flag arg should not be a reserved Value
            if (ArgVal(1) <= 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If
            //check for read only reserved flags
            CheckResFlagUse ArgVal(1)


          case 81 //move.obj(oA, X,Y,STEP,fDONE)
            //validate the target position
            if (ArgVal(1) > 159 || ArgVal(2) > 167) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4127)
                return  //Exit Function
              case leMedium
                AddWarning 5062
              case leLow
              } 
            } //End If

            //check for ego object
            if (ArgVal(0) == 0) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5045
              case leLow
              } 
            } //End If

            //flag arg should not be a reserved Value
            if (ArgVal(4) <= 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If

            //check for read only reserved flags
            CheckResFlagUse ArgVal(4)


          case 82 //move.obj.v
            //flag arg should not be a reserved Value
            if (ArgVal(4) <= 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If

            //check for read only reserved flags
            CheckResFlagUse ArgVal(4)


          case 83 //follow.ego(oA, DISTANCE, fDONE)
            //validate distance value
            if (ArgVal(1) <= 1) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5102
              case leLow
              } 
            } //End If

            //check for ego object
            if (ArgVal(0) == 0) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5027
              case leLow
              } 
            } //End If

            //flag arg should not be a reserved Value
            if (ArgVal(2) <= 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5078, Replace(LoadResString(5078), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If
            //check for read only reserved flags
            CheckResFlagUse ArgVal(2)


          case 86 //set.dir(oA, vB)
            //check for ego object
            if (ArgVal(0) == 0) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5026
              case leLow
              } 
            } //End If


          case 87 //get.dir
            //variable is second arg
            //variable arg should not be a reserved Value
            if (ArgVal(1) <= 26) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If


          case 90 //block(x1,y1,x2,y2)
            //validate that all are within bounds, and that x1<=x2 and y1<=y2
            //also check that
            if (ArgVal(0) > 159 || ArgVal(1) > 167 || ArgVal(2) > 159 || ArgVal(3) > 167) {
              //bad number
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4129)
                return  //Exit Function
              case leMedium
                AddWarning 5020
              case leLow
              } 
            } //End If


            If(ArgVal(2) - ArgVal(0) < 2) Or(ArgVal(3) - ArgVal(1) < 2)) {
              //won//t work
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4129)
                return  //Exit Function
              case leMedium
                AddWarning 5051
              case leLow
              } 
            } //End If



          case 98 //load.sound(A)
            //validate the sound exists
            if (!agSnds.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4130), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5014
              case leLow
              } 
            } //End If


          case 99 //sound(A)
            //validate the sound exists
            if (!agSnds.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4137), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5084
              case leLow
              } 
            } //End If


          case 103 //display(ROW,COL,mC)
            //check row/col against limits
            if (ArgVal(0) > 24 || ArgVal(1) > 39) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
               strErrMsg = LoadResString(4131)
               return  //Exit Function
              case leMedium
                AddWarning 5059
              case leLow
              } 
            } //End If


          case 105 //clear.lines(TOP,BTM,C)
            //top must be >btm; both must be <=24
            if (ArgVal(0) > 24 || ArgVal(1) > 24 || ArgVal(0) > ArgVal(1)) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4132)
                return  //Exit Function
              case leMedium
                AddWarning 5011
              case leLow
              } 
            } //End If
            //color value should be 0 or 15 //(but it doesn//t hurt to be anything else)
            if (ArgVal(2) > 0 && ArgVal(2) != 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5100
              case leLow
              } 
            } //End If


          case 109 //set.text.attribute(A,B)
            //should be limited to valid color values (0-15)
            if (ArgVal(0) > 15 || ArgVal(1) > 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4133)
                return  //Exit Function
              case leMedium
                AddWarning 5029
              case leLow
              } 
            } //End If


          case 110 //shake.screen(A)
            //shouldn//t normally have more than a few shakes; zero is BAD
            if (ArgVal(0) == 0) {
              //error!
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                strErrMsg = LoadResString(4134)
                return  //Exit Function
              case leLow
              } 
            ElseIf ArgVal(0) > 15) {
              //could be a palette change?
              if (ArgVal(0) >= 100 && ArgVal(0) <= 109) {
                //separate warning
                switch (agMainLogSettings.ErrorLevel
                case leHigh, leMedium
                  AddWarning 5058
                case leLow
                } 
              Else
                //warning
                switch (agMainLogSettings.ErrorLevel
                case leHigh, leMedium
                  AddWarning 5057
                case leLow
                } 
              } //End If
            } //End If


          case 111 //configure.screen(TOP,INPUT,STATUS)
            //top should be <=3
            //input and status should not be equal
            //input and status should be <top or >=top+21
            if (ArgVal(0) > 3) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4135)
                return  //Exit Function
              case leMedium
                AddWarning 5044
              case leLow
              } 
            } //End If
            if (ArgVal(1) > 24 || ArgVal(2) > 24) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5099
              case leLow
              } 
            } //End If
            if (ArgVal(1) == ArgVal(2)) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5048
              case leLow
              } 
            } //End If
            if ((ArgVal(1) >= ArgVal(0) && ArgVal(1) <= CLng(ArgVal(0)) + 20) Or(ArgVal(2) >= ArgVal(0) && ArgVal(2) <= CLng(ArgVal(0)) + 20)) {
             switch (agMainLogSettings.ErrorLevel

             case leHigh, leMedium
               AddWarning 5049

             case leLow

             } 

           } //End If


         case 114 //set.string(sA, mB)
            //warn user if setting input prompt to unusually long value

           if (ArgVal(0) == 0) {
             if (Len(strMsg(ArgVal(1))) > 10) {
               switch (agMainLogSettings.ErrorLevel

               case leHigh, leMedium
                 AddWarning 5096

               case leLow

               } 

             } //End If

           } //End If


         case 115 //get.string(sA, mB, ROW,COL,LEN)
            //if row>24, both row/col are ignored; if col>39, gets weird; len is limited automatically to <=40

           if (ArgVal(2) > 24) {
             switch (agMainLogSettings.ErrorLevel

             case leHigh, leMedium
               AddWarning 5052

             case leLow

             } 

           } //End If


           if (ArgVal(3) > 39) {
             switch (agMainLogSettings.ErrorLevel

             case leHigh

               strErrMsg = LoadResString(4004)

               return  //Exit Function

             case leMedium

               AddWarning 5080

             case leLow

             } 

           } //End If


           if (ArgVal(4) > 40) {
             switch (agMainLogSettings.ErrorLevel

             case leHigh, leMedium
               AddWarning 5056

             case leLow

             } 

           } //End If


         case 121 //set.key(A,B,cC)
            //controller number limit checked in GetNextArg function

            //increment controller Count

           intCtlCount = intCtlCount + 1

            //must be ascii or key code, (Arg0 can be 1 to mean joystick)

           if (ArgVal(0) > 0 && ArgVal(1) > 0 && ArgVal(0) != 1) {
             switch (agMainLogSettings.ErrorLevel

             case leHigh

               strErrMsg = LoadResString(4154)

               return  //Exit Function

             case leMedium

               AddWarning 5065

             case leLow

             } 

           } //End If

            //check for improper ASCII assignments

           if (ArgVal(1) == 0) {
             switch (ArgVal(0) //ascii codes
              case 8, 13, 32 //bkspace, enter, spacebar
                //bad
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  strErrMsg = LoadResString(4155)
                  return  //Exit Function
                case leMedium
                  AddWarning 5066
                case leLow
                } 
              } 
            } //End If

            //check for improper KEYCODE assignments
            if (ArgVal(0) == 0) {
              switch (ArgVal(0) //ascii codes
              case 71, 72, 73, 75, 76, 77, 79, 80, 81, 82, 83
                //bad
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  strErrMsg = LoadResString(4155)
                  return  //Exit Function
                case leMedium
                  AddWarning 5066
                case leLow
                } 
              } 
            } //End If


          case 122 //add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
            //VIEW, LOOP & CEL must exist
            //CEL width must be >=3
            //x,y must be within limits
            //PRI must be 0, or >=3 AND <=15
            //MGN must be 0-3, or >3 (ha ha, or ANY value...)

            //validate view
            if (!agViews.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4138), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5064
                //dont need to check loops or cels
                blnWarned = true
              case leLow
              } 
            } //End If


            if (!blnWarned) {
              //try to load view to test loop & cel
              On Error Resume Next
              blnUnload = !agViews(ArgVal(0)).Loaded
              //if error trying to get loaded status, ignore for now
              //it//ll show up again when trying to load or access
              //loop property and be handled there
              Err.Clear
              if (blnUnload) {
                agViews(ArgVal(0)).Load
              } //End If
              if (Err.Number == 0) {
                //validate loop
                if (ArgVal(1) >= agViews(ArgVal(0)).Loops.Count) {
                  switch (agMainLogSettings.ErrorLevel
                  case leHigh
                    strErrMsg = Replace(Replace(LoadResString(4139), ARG1, CStr(ArgVal(1))), ARG2, CStr(ArgVal(0)))
                    if (blnUnload) {
                      agViews(ArgVal(0)).Unload
                    } //End If
                    return  //Exit Function
                  case leMedium
                    AddWarning 5085
                    //dont need to check cel
                    blnWarned = true
                  case leLow
                  } 
                } //End If
                //if loop was valid, check cel
                if (!blnWarned) {
                  //validate cel
                  if (ArgVal(2) >= agViews(ArgVal(0)).Loops(ArgVal(1)).Cels.Count) {
                    switch (agMainLogSettings.ErrorLevel
                    case leHigh
                      strErrMsg = Replace(Replace(Replace(LoadResString(4140), ARG1, CStr(ArgVal(2))), ARG2, CStr(ArgVal(1))), ARG3, CStr(ArgVal(0)))
                      if (blnUnload) {
                        agViews(ArgVal(0)).Unload
                      } //End If
                      return  //Exit Function
                    case leMedium
                      AddWarning 5086
                    case leLow
                    } 
                  } //End If
                } //End If
              Else
                //can//t load the view; add a warning
                Err.Clear
                AddWarning 5021, Replace(LoadResString(5021), ARG1, CStr(ArgVal(0)))
              } //End If
              if (blnUnload) {
                agViews(ArgVal(0)).Unload
              } //End If
            } //End If

            On Error GoTo ErrHandler

            //x,y must be within limits
            if (ArgVal(3) > 159 || ArgVal(4) > 167) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4141)
                return  //Exit Function
              case leMedium
                AddWarning 5038
              case leLow
              } 
            } //End If

            //PRI should be <=15
            if (ArgVal(5) > 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4142)
                return  //Exit Function
              case leMedium
                AddWarning 5079
              case leLow
              } 
            } //End If

            //PRI should be 0 OR >=4 (but doesn//t raise an error; only a warning)
            if (ArgVal(5) < 4 && ArgVal(5) != 0) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5079
              case leLow
              } 
            } //End If

            //MGN values >15 will only use lower nibble
            if (ArgVal(6) > 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5101
              case leLow
              } 
            } //End If


          case 129 //show.obj(VIEW)
            //validate view
            if (!agViews.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4144), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5061
              case leLow
              } 
            } //End If


          case 127, 176, 178  //init.disk, hide.mouse, show.mouse
            switch (agMainLogSettings.ErrorLevel
            case leHigh, leMedium
              AddWarning 5087, Replace(LoadResString(5087), ARG1, agCmds(CmdNum).Name)
            case leLow
            } 


          case 175, 179, 180 //discard.sound, fence.mouse, mouse.posn
            switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4152), ARG1, agCmds(CmdNum).Name)
                return  //Exit Function
              case leMedium
              AddWarning 5088, Replace(LoadResString(5088), ARG1, agCmds(CmdNum).Name)
            case leLow
            } 


          case 130 //random(LOWER,UPPER,vRESULT)
            //lower should be < upper
            if (ArgVal(0) > ArgVal(1)) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4145)
                return  //Exit Function
              case leMedium
                AddWarning 5054
              } 
            } //End If

            //lower=upper means result=lower=upper
            if (ArgVal(0) == ArgVal(1)) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5106
              case leLow
              } 
            } //End If

            //if lower=upper+1, means div by 0!
            if (ArgVal(0) == ArgVal(1) + 1) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4158)
                return  //Exit Function
              case leMedium
                AddWarning 5107
              } 
            } //End If

            //variable arg should not be a reserved Value
            if (ArgVal(2) <= 26) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5077, Replace(LoadResString(5077), ARG1, agCmds(CmdNum).Name)
              case leLow
              } 
            } //End If


          case 142 //script.size
            //raise warning/error if in other than logic0
            if (bytLogComp<> 0) {
              //warn
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                //set warning
                AddWarning 5039
              case leLow
                //no action
              } 
            } //End If
            //check for absurdly low Value for script size
            if (ArgVal(0) < 10) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5009
              case leLow
              } 
            } //End If


          case 147 //reposition.to(oA, B,C)
            //validate the new position
            if (ArgVal(1) > 159 || ArgVal(2) > 167) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4128)
                return  //Exit Function
              case leMedium
                AddWarning 5023
              case leLow
              } 
            } //End If


          case 150 //trace.info(LOGIC,ROW,HEIGHT)
            //logic must exist
            //row + height must be <22
            //height must be >=2 (but interpreter checks for this error)

            //validate that logic exists
            if (!agLogs.Exists(ArgVal(0))) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = Replace(LoadResString(4153), ARG1, CStr(ArgVal(0)))
                return  //Exit Function
              case leMedium
                AddWarning 5040
              case leLow
              } 
            } //End If
            //validate that height is not too small
            if (ArgVal(2) < 2) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5046
              case leLow
              } 
            } //End If
            //validate size of window
            if (CLng(ArgVal(1)) + CLng(ArgVal(2)) > 23) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4146)
                return  //Exit Function
              case leMedium
                AddWarning 5063
              case leLow
              } 
            } //End If


          case 151, 152 //Print.at(mA, ROW, COL, MAXWIDTH), print.at.v
            //row <=22
            //col >=2
            //maxwidth <=36
            //maxwidth=0 defaults to 30
            //maxwidth=1 crashes AGI
            //col + maxwidth <=39
            if (ArgVal(1) > 22) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4147)
                return  //Exit Function
              case leMedium
                AddWarning 5067
              case leLow
              } 
            } //End If


            switch (ArgVal(3)
            case 0 //maxwidth=0 defaults to 30
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5105
              case leLow
              } 


            case 1 //maxwidth=1 crashes AGI
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4043)
                return  //Exit Function
              case leMedium
                AddWarning 5103
              case leLow
              } 


            case Is > 36 //maxwidth >36 won//t work
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4043)
                return  //Exit Function
              case leMedium
                AddWarning 5104
              case leLow
              } 
           } 

            //col>2 and col + maxwidth <=39
            if (ArgVal(2) < 2 || CLng(ArgVal(2)) + CLng(ArgVal(3)) > 39) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4148)
                return  //Exit Function
              case leMedium
                AddWarning 5068
              case leLow
              } 
            } //End If


          case 154 //clear.text.rect(R1,C1,R2,C2,COLOR)
            //if (either row argument is >24,
            //or either column argument is >39,
            //or R2 < R1 or C2 < C1,
            //the results are unpredictable
            if (ArgVal(0) > 24 || ArgVal(1) > 39 || _
               ArgVal(2) > 24 || ArgVal(3) > 39 || _
               ArgVal(2) < ArgVal(0) || ArgVal(3) < ArgVal(1)) {
              //invalid items
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4150)
                return  //Exit Function
              case leMedium
                //if due to pos2 < pos1
                if (ArgVal(2) < ArgVal(0) || ArgVal(3) < ArgVal(1)) {
                  AddWarning 5069
                } //End If
                //if due to variables outside limits
                if (ArgVal(0) > 24 || ArgVal(1) > 39 || _
                   ArgVal(2) > 24 || ArgVal(3) > 39) {
                  AddWarning 5070
                } //End If
              } 
            } //End If

            //color value should be 0 or 15 //(but it doesn//t hurt to be anything else)
            if (ArgVal(4) > 0 && ArgVal(4) != 15) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5100
              case leLow
              } 
            } //End If


          case 158 //submit.menu()
            //should only be called in logic0
            //raise warning/error if in other than logic0
            if (bytLogComp != 0) {
              //warn
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                //set warning
                AddWarning 5047
              case leLow
              } 
            } //End If


          case 174 //set.pri.base(A)
            //calling set.pri.base with Value >167 doesn//t make sense
            if (ArgVal(0) > 167) {
              switch (agMainLogSettings.ErrorLevel
              case leHigh, leMedium
                AddWarning 5071
              case leLow
              } 
            } //End If
          } 

          //success
          ValidateArgs = true
        return  //Exit Function

        ErrHandler:
          //////Debug.Assert false
          Resume Next
        } //End Function

        static internal Function ValidateIfArgs(CmdNum As int, ByRef ArgVal() As byte) As bool

          //check for specific command issues


          switch (CmdNum
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
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4151)
                return  //Exit Function
              case leMedium
                AddWarning 5072
              case leLow
              } 
            } //End If


            If(ArgVal(1) > ArgVal(3)) Or(ArgVal(2) > ArgVal(4))) {
              //can//t work
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4151)
                return  //Exit Function
              case leMedium
                AddWarning 5073
              case leLow
              } 
            } //End If


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
        } //End Function

        static internal Function ValidateMsgChars(strMsg As string, MsgNum As int) As bool

          //raise error/warning, depending on setting

          //return TRUE if OK or only a warning;  FALSE means error found


          Dim i As int
          Dim blnWarn5093 As bool, blnWarn5094 As bool

          //if LOW errdetection, EXIT
          if (agMainLogSettings.ErrorLevel == leLow) {
            ValidateMsgChars = true
            return  //Exit Function
          } //End If


          For i = 1 To Len(strMsg)
            //check for invalid codes (0,8,9,10,13)
            switch (AscW(Mid(strMsg, i, 1))
            case 0, 8, 9, 10, 13
              //warn user
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4005)
                blnError = true
                return  //Exit Function
              case leMedium
                if (!blnWarn5093) {
                  AddWarning 5093
                  blnWarn5093 = true
                  //need to track warning in case this msg is
                  //also included in body of logic
                  intMsgWarn(MsgNum) = intMsgWarn(MsgNum) || 1
                } //End If
              } 

            //extended character
            case Is > 127
              switch (agMainLogSettings.ErrorLevel
              case leHigh
                strErrMsg = LoadResString(4006)
                blnError = true
                return  //Exit Function
              case leMedium
                if (!blnWarn5094) {
                  AddWarning 5094
                  blnWarn5094 = true
                  //need to track warning in case this msg is
                  //also included in body of logic
                  intMsgWarn(MsgNum) = intMsgWarn(MsgNum) || 2
                } //End If
              } 
            } 
           Next i

           //msg is OK
           ValidateMsgChars = true
        } //End Function

        static internal Function VariableValue(strVar As string) As int
          //this function will extract the variable number from
          //an input variable string
          //the input string should be of the form #, a# or *a#
          // where a is a valid variable prefix (v, f, s, m, w, c)
          //and # is 0-255
          //if the result is invalid, this function returns -1


          Dim strVarVal As string
          Dim intVarVal  As int
          Dim blnOutofBounds As bool


          On Error GoTo ErrHandler

          //if not numeric
          if (!IsNumeric(strVar)) {
            //strip off variable prefix, and indirection
            //if indirection
            if (Left(strVar, 1) == "*") {
              strVarVal = Right(strVar, Len(strVar) - 2)
            Else
              strVarVal = Right(strVar, Len(strVar) - 1)
            } //End If
          Else
            //use the input Value
            strVarVal = strVar
          } //End If

          //if result is a number
          if (IsNumeric(strVarVal)) {
            //get number
            intVarVal = Val(strVarVal)
            //for word only, subtract one to
            //account for //1// based word data type
            //(i.e. w1 is first word, but command uses arg Value of //0//)
            if (AscW(strVar) == 119) {
              intVarVal = intVarVal - 1
            } //End If

            //verify within bounds  0-255
            if (intVarVal >= 0 && intVarVal <= 255) {
              //return this Value
              VariableValue = intVarVal
              return  //Exit Function
            } //End If
          } //End If

          //error- return -1
          VariableValue = -1
        return  //Exit Function

        ErrHandler:
          Err.Clear
          //return -1
          VariableValue = -1
        } //End Function
        static internal Function WriteMsgs() As bool
          //this function will write the messages for a logic at the end of
          //the resource.
          //messages are encrypted with the string //Avis Durgan//. No gaps
          //are allowed, so messages that are skipped must be included as
          //zero length messages

          Dim lngMsgSecStart As int
          Dim lngMsgSecLen As int
          Dim lngMsgPos(255) As int
          Dim intCharPos As int, bytCharVal As byte
          Dim lngMsg As int
          Dim lngMsgCount As int
          Dim lngCryptStart As int
          Dim lngMsgLen As int
          Dim i As int, strHex As string
          Dim blnSkipNull As bool, blnSkipChar As bool


          On Error GoTo ErrHandler

          //calculate start of message section
          lngMsgSecStart = tmpLogRes.Size

          //find last message by counting backwards until a msg is found
          lngMsgCount = 256
          Do
            lngMsgCount = lngMsgCount - 1
          Loop Until blnMsg(lngMsgCount) Or(lngMsgCount = 0)

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
            Else
              //////Debug.Assert strMsg(lngMsg) = ""
              //need to write a null value for offset; (when it gets added after all
              //messages are written it gets set to the beginning of message section
              // ( a relative offset of zero here)
              lngMsgPos(lngMsg) = 0
            } //End If
            if (lngMsgLen > 0) {
              //step through all characters in this msg
              intCharPos = 1
              Do Until intCharPos > Len(strMsg(lngMsg))
                //get ascii code for this character
                bytCharVal = Asc(Mid(strMsg(lngMsg), intCharPos))
                //check for invalid codes (8,9,10,13)
                switch (bytCharVal
                case 0, 8, 9, 10, 13
                  //convert these chars to space to avoid trouble
                  bytCharVal = 32

                case 92 //"\"
                  //check for special codes
                  If(intCharPos<lngMsgLen)) {
                 switch (AscW(Mid(strMsg(lngMsg), intCharPos + 1))
                    case 110, 78 //n or N//
                      // \n = new line
                      bytCharVal = &HA
                      intCharPos = intCharPos + 1


                    case 34 //dbl quote(")//
                      //\" = quote mark (chr$(34))
                      bytCharVal = &H22
                      intCharPos = intCharPos + 1


                    case 92 //\//
                      //\\ = \
                      bytCharVal = &H5C
                      intCharPos = intCharPos + 1


                    case 48 //0//
                      //\0 = don//t add null terminator
                      blnSkipNull = true
                      //also skip this char
                      blnSkipChar = true
                      intCharPos = intCharPos + 1


                    case 120 //x//  //look for a hex value
                      //make sure at least two more characters
                      if (intCharPos + 2 < lngMsgLen) {
                        //get next 2 chars and hexify them
                        strHex = "&H" & Mid(strMsg(lngMsg), intCharPos + 2, 2)

                        //if this hex value >=1 and <256, use it
                        i = Val(strHex)
                        if (i >= 1 && i< 256) {
                          bytCharVal = i
                          intCharPos = intCharPos + 3
                        } //End If
                      } //End If
                    default:
                      //if no special char found, the single slash should be dropped
                      blnSkipChar = true
                    } 
                  Else
                    //if the //\// is the last char, skip it
                    blnSkipChar = true
                  } //End If
                } 

                //write the encrypted byte (need to adjust for previous messages, and current position)
                if (!blnSkipChar) {
                  tmpLogRes.Writebyte bytCharVal Xor bytEncryptKey((tmpLogRes.GetPos - lngCryptStart) Mod 11)
                } //End If
                //increment pointer
                intCharPos = intCharPos + 1
                //reset skip flag
                blnSkipChar = false
              Loop
            } //End If

            //if msg was used, add trailing zero to terminate message
            //(if msg was zero length, we still need this terminator)
            if (blnMsg(lngMsg)) {
              if (!blnSkipNull) {
                tmpLogRes.Writebyte &H0 Xor bytEncryptKey((tmpLogRes.GetPos - lngCryptStart) Mod 11)
              } //End If
            } //End If
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

        return  //Exit Function

        ErrHandler:
          //any errors, means there is a problem
          blnError = true
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG2, "WriteMsgs")
          Err.Clear
          WriteMsgs = false
        } //End Function



        static internal Function ReadLabels() As bool

          Dim i As byte
          Dim intLabel As int
          Dim strLabel As string
          Dim rtn As int


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
              ElseIf Left(strLabel, 1) == ":") {
                strLabel = LTrim(Right(strLabel, Len(strLabel) - 1))
              Else
                //not a label
                strLabel = ""
              } //End If

              //if a label was found, validate it
              if (Len(strLabel) != 0) {
                //make sure enough room
                if (bytLabelCount >= MAX_LABELS) {
                  strErrMsg = Replace(LoadResString(4109), ARG1, CStr(MAX_LABELS))
                  return  //Exit Function
                } //End If


                rtn = ValidateDefName(strLabel)
                //numbers are ok for labels
                if (rtn == 2) {
                  rtn = 0
                } //End If
                if (rtn<> 0) {
                  //error
                  switch (rtn
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
                  return  //Exit Function
                } //End If

                //no periods allowed either
                if (InStr(1, strLabel, ".") != 0) {
                  strErrMsg = LoadResString(4068)
                  return  //Exit Function
                } //End If

                //check label against current list of labels
                if (bytLabelCount > 0) {
                  For i = 1 To bytLabelCount
                    if (strLabel == llLabel(i).Name) {
                      strErrMsg = Replace(LoadResString(4027), ARG1, strLabel)
                      return  //Exit Function
                    } //End If
                  Next i
                } //End If

                //increment number of labels, and save
                bytLabelCount = bytLabelCount + 1
                llLabel(bytLabelCount).Name = strLabel
                llLabel(bytLabelCount).Loc = 0
              } //End If
            } //End If

            //get next line
            IncrementLine
          Loop Until lngLine = -1
          ReadLabels = true
        return  //Exit Function
        ErrHandler:
          strErrMsg = Replace(Replace(LoadResString(4115), ARG1, CStr(Err.Number)), ARG1, "ReadLabels")
          Err.Clear
        } //End Function
        static internal Function CompileAGI() As bool
          //main compiler function
          //steps through input one command at a time and converts it
          //to AGI logic code
          //Note that we don//t need to set blnError flag here;
          //an error will cause this function to return a Value of false
          //which causes the compiler to display error info

          const MaxGotos = 255

          Dim strNextCmd As string
          Dim strPrevCmd As string
          Dim strArg As string, bytArg(7) As byte
          Dim i As int
          Dim intCmdNum As int
          Dim BlockStartDataLoc(MAX_BLOCK_DEPTH) As int
          Dim BlockDepth As int
          Dim BlockIsIf(MAX_BLOCK_DEPTH) As bool
          Dim BlockLength(MAX_BLOCK_DEPTH) As int
          Dim CurLabel As int
          Dim intLabelNum As int
          Dim Gotos(MaxGotos) As LogicGoto
          Dim NumGotos As int
          Dim GotoData As int
          Dim CurGoto As int
          Dim blnLastCmdRtn As bool
          Dim lngReturnLine As int


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
            switch (strNextCmd
            case "{"
              //can//t have a "{" command, unless it follows an //if// or //else//
              if (strPrevCmd<> "if" && strPrevCmd != "else") {
                strErrMsg = LoadResString(4008)
                return  //Exit Function
              } //End If


            case "}"
              //if no block currently open,
              if (BlockDepth == 0) {
                strErrMsg = LoadResString(4010)
                return  //Exit Function
              } //End If
              //if last command was a new.room command, then closing block is expected
              if (blnNewRoom) {
                blnNewRoom = false
              } //End If
              //if last position in resource is two bytes from start of block
              if (tmpLogRes.Size == BlockStartDataLoc(BlockDepth) + 2) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  strErrMsg = LoadResString(4049)
                  return  //Exit Function
                case leMedium
                  //set warning
                  AddWarning 5001
                case leLow
                  //no action
                } 
              } //End If
              //calculate and write block length
              BlockLength(BlockDepth) = tmpLogRes.Size - BlockStartDataLoc(BlockDepth) - 2
              tmpLogRes.WriteWord CLng(BlockLength(BlockDepth)), CLng(BlockStartDataLoc(BlockDepth))
              //remove block from stack
              BlockDepth = BlockDepth - 1
            case "if"
              //compile the //if// statement
              if (!CompileIf()) {
                return  //Exit Function
              } //End If
              //if block stack exceeded
              if (BlockDepth >= MAX_BLOCK_DEPTH) {
                strErrMsg = Replace(LoadResString(4110), ARG1, CStr(MAX_BLOCK_DEPTH))
                return  //Exit Function
              } //End If
              //add block to stack
              BlockDepth = BlockDepth + 1
              BlockStartDataLoc(BlockDepth) = tmpLogRes.GetPos
              BlockIsIf(BlockDepth) = true
              //write placeholders for block length
              tmpLogRes.WriteWord &H0

              //next command better be a bracket
              strNextCmd = NextCommand()
              if (strNextCmd<> "{") {
                //error!!!!
                strErrMsg = LoadResString(4053)
                return  //Exit Function
              } //End If


            case "else"
              //else can only follow a close bracket
              if (strPrevCmd != "}") {
                strErrMsg = LoadResString(4011)
                return  //Exit Function
              } //End If

              //if the block closed by that bracket was an //else//
              //(which will be determined by having that block//s IsIf flag NOT being set),
              if (!BlockIsIf(BlockDepth + 1)) {
                strErrMsg = LoadResString(4083)
                return  //Exit Function
              } //End If

              //adjust blockdepth to the //if// command
              //directly before this //else//
              BlockDepth = BlockDepth + 1
              //adjust previous block length to accomodate the //else// statement
              BlockLength(BlockDepth) = BlockLength(BlockDepth) + 3
              tmpLogRes.WriteWord CLng(BlockLength(BlockDepth)), CLng(BlockStartDataLoc(BlockDepth))
              //previous //if// block is now closed; use same block level
              //for this //else// block
              BlockIsIf(BlockDepth) = false
              //write the //else// code
              tmpLogRes.Writebyte &HFE
              BlockStartDataLoc(BlockDepth) = tmpLogRes.GetPos
              tmpLogRes.WriteWord &H0  // block length filled in later.

              //next command better be a bracket
              strNextCmd = NextCommand()
              if (strNextCmd != "{") {
                //error!!!!
                strErrMsg = LoadResString(4053)
                return  //Exit Function
              } //End If


            case "goto"
              //if last command was a new room, warn user
              if (blnNewRoom) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh, leMedium
                  //set warning
                  AddWarning 5095
                case leLow
                  //no action
                } 
                blnNewRoom = false
              } //End If

              //next command should be "("
              if (NextChar() != "(") {
                strErrMsg = LoadResString(4001)
                return  //Exit Function
              } //End If
              //get goto argument
              strArg = NextCommand()

              //if argument is NOT a valid label
              if (LabelNum(strArg) == 0) {
                strErrMsg = Replace(LoadResString(4074), ARG1, strArg)
                return  //Exit Function
              } //End If
              //if too many gotos
              if (NumGotos >= MaxGotos) {
                strErrMsg = Replace(LoadResString(4108), ARG1, CStr(MaxGotos))
              } //End If
              //save this goto info on goto stack
              NumGotos = NumGotos + 1
              Gotos(NumGotos).LabelNum = LabelNum(strArg)
              //write goto command byte
              tmpLogRes.Writebyte &HFE
              Gotos(NumGotos).DataLoc = tmpLogRes.GetPos
              //write placeholder for amount of offset
              tmpLogRes.WriteWord &H0
              //next character should be ")"
              if (NextChar() != ")") {
                strErrMsg = LoadResString(4003)
                return  //Exit Function
              } //End If
              //verify next command is end of line (;)
              if (NextChar() != ";") {
                blnError = true
                strErrMsg = LoadResString(4007)
                return  //Exit Function
              } //End If

              //since block commands are no longer supported, check for markers in order to provide a
              //meaningful error message
            case "/*", "* /"
              blnError = true
              strErrMsg = LoadResString(4052)
              return  //Exit Function


            case "++", "--" //unary operators; need to get a variable next
              //write the command code
              if (strNextCmd == "++") {
                tmpLogRes.Writebyte 1
              Else
                tmpLogRes.Writebyte 2
              } //End If

              //get the variable to update
              strArg = NextCommand()
              //convert it
              if (!ConvertArgument(strArg, atVar, false)) {
                //error
                blnError = true
                //////Debug.Assert false
                strErrMsg = LoadResString(4046)
                return  //Exit Function
              } //End If
              //get Value
              intCmdNum = VariableValue(strArg)
              if (intCmdNum == -1) {
                blnError = true
                //////Debug.Assert false
                strErrMsg = Replace(LoadResString(4066), "%1", "")
                return  //Exit Function
              } //End If
              //write the variable value
              tmpLogRes.Writebyte Cbyte(intCmdNum)
              //verify next command is end of line (;)
              if (NextChar(true) != ";") {
                strErrMsg = LoadResString(4007)
                return  //Exit Function
              } //End If


            case ":"  //alternate label syntax
              //get next command; it should be the label
              strNextCmd = NextCommand()
              intLabelNum = LabelNum(strNextCmd)
              //if not a valid label
              if (intLabelNum == 0) {
                strErrMsg = LoadResString(4076)
                return  //Exit Function
              } //End If
              //save position of label
              llLabel(intLabelNum).Loc = tmpLogRes.Size


            default:
              //must be a label, command, or special syntax
              //if next character is a colon
              if (Mid(strCurrentLine, lngPos + 1, 1) == ":") {
                //it//s a label
                intLabelNum = LabelNum(strNextCmd)
                //if not a valid label
                if (intLabelNum == 0) {
                  strErrMsg = LoadResString(4076)
                  return  //Exit Function
                } //End If
                //save position of label
                llLabel(intLabelNum).Loc = tmpLogRes.Size
                //read in next char to skip past the colon
                NextChar
              Else
                //if last command was a new room (and not followed by return(), warn user
                if (blnNewRoom && strNextCmd<> "return") {
                  switch (agMainLogSettings.ErrorLevel
                  case leHigh, leMedium
                    //set warning
                    AddWarning 5095
                  case leLow
                    //no action
                  } 
                  blnNewRoom = false
                } //End If

                //get number of command
                intCmdNum = CommandNum(false, strNextCmd)
                //if invalid version
                if (intCmdNum == 254) {
                  //raise error
                  strErrMsg = Replace(LoadResString(4065), ARG1, strNextCmd)
                  return  //Exit Function
                //if command not found,
                ElseIf intCmdNum == 255) {  // not found
                  //try to parse special syntax
                  if (CompileSpecial(strNextCmd)) {
                    //check for error
                    if (blnError) {
                      return  //Exit Function
                    } //End If
                  Else
                    //unknown command
                    strErrMsg = Replace(LoadResString(4116), ARG1, strNextCmd)
                    return  //Exit Function
                  } //End If
                Else
                  //write the command code,
                  tmpLogRes.Writebyte Cbyte(intCmdNum)
                  //next character should be "("
                  if (NextChar() != "(") {
                    strErrMsg = LoadResString(4048)
                    return  //Exit Function
                  } //End If

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
                        Else
                          //use 1-base arg values
                          strErrMsg = Replace(LoadResString(4047), ARG1, CStr(i + 1))
                        } //End If
                        return  //Exit Function
                      } //End If
                    } //End If
                    bytArg(i) = GetNextArg(agCmds(Cbyte(intCmdNum)).ArgType(i), i)
                    //if error
                    if (blnError) {
                      // if error number is 4054
                      if (Val(strErrMsg) == 4054) {
                        // add command name to error string
                        strErrMsg = Replace(strErrMsg, ARG2, agCmds(intCmdNum).Name)
                      } //End If
                      return  //Exit Function
                    } //End If

                    //write argument
                    tmpLogRes.Writebyte bytArg(i)
                  Next i

                  //validate arguments for this command
                  if (!ValidateArgs(intCmdNum, bytArg())) {
                    return  //Exit Function
                  } //End If

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
                    Else
                      strErrMsg = LoadResString(4160)
                    } //End If
                    return  //Exit Function
                  } //End If
                  if (intCmdNum == 0) {
                    blnLastCmdRtn = true
                    //set line number
                    if (lngReturnLine == 0) {
                      lngReturnLine = lngLine + 1
                    } //End If
                  } //End If
                } //End If

                //verify next command is end of line (;)
                if (NextChar(true) != ";") {
                  strErrMsg = LoadResString(4007)
                  return  //Exit Function
                } //End If
              } //End If
             } 
            //get next command
            strPrevCmd = strNextCmd
            strNextCmd = NextCommand()
         Loop

          If(!blnLastCmdRtn)) {
           switch (agMainLogSettings.ErrorLevel

           case leHigh
              //no rtn error

             strErrMsg = LoadResString(4102)

             return  //Exit Function

           case leMedium
              //add a return code

             tmpLogRes.Writebyte 0

           case leLow
              //add a return code

             tmpLogRes.Writebyte 0
              //and a warning

             AddWarning 5016

           } 

         } //End If

          //check to see if everything was wrapped up properly

         if (BlockDepth > 0) {
           strErrMsg = LoadResString(4009)
            //reset errorline to return cmd

           lngErrLine = lngReturnLine

           return  //Exit Function

         } //End If

          //write in goto values

         For CurGoto = 1 To NumGotos

           GotoData = llLabel(Gotos(CurGoto).LabelNum).Loc - Gotos(CurGoto).DataLoc - 2

           if (GotoData < 0) {
              //need to convert it to an unsigned integer Value

             GotoData = &H10000 + GotoData

           } //End If

           tmpLogRes.WriteWord CLng(GotoData), CLng(Gotos(CurGoto).DataLoc)
          Next CurGoto

          //return true
          CompileAGI = true
        return  //Exit Function

        ErrHandler:
          //if error is an app specific error, just pass it along; otherwise create
          //an app specific error to encapsulate whatever happened
          strError = Err.Description
          strErrSrc = Err.Source
          lngError = Err.Number
          If(lngError && vbObjectError) = vbObjectError) {
            //pass it along
            On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
          Else
            On Error GoTo 0: Err.Raise vbObjectError + 658, strErrSrc, Replace(LoadResString(658), ARG1, CStr(lngError) & ":" & strError)
          } //End If
        } //End Function

        static internal Function MessageNum(strMsgIn As string) As int
          // Returns the number of the message corresponding to
          //strMsg, or creates a new msg number if strMsg is not
          //currently a message
          //if maximum number of msgs assigned, returns  0
          Dim lngMsg As int

          //blank msgs normally not allowed
          if (LenB(strMsgIn) == 0) {
            switch (agMainLogSettings.ErrorLevel
            case leHigh, leMedium
              AddWarning 5074
            case leLow
              //allow it
            } 
          } //End If


          For lngMsg = 1 To 255
            //if this is the message
            //(use StrComp, since this is a case-sensitive search)
            if (StrComp(strMsg(lngMsg), strMsgIn, vbBinaryCompare) == 0) {
              //return this Value
              MessageNum = lngMsg
              //if null string found for first time, msg-in-use flag will be false
              if (!blnMsg(lngMsg)) {
                blnMsg(lngMsg) = true
              } //End If
              //if this msg has an extended char warning, repeat it here
              If(intMsgWarn(lngMsg) && 1) = 1) {
               AddWarning 5093
              } //End If
              If(intMsgWarn(lngMsg) && 2) = 2) {
               AddWarning 5094
              } //End If
              return  //Exit Function
            } //End If
          Next lngMsg

          //msg doesn//t exist; find an empty spot
          For lngMsg = 1 To 255
            if (!blnMsg(lngMsg)) {
              //this message is available
              blnMsg(lngMsg) = true
              strMsg(lngMsg) = strMsgIn

              //check for invalid characters
              if (!ValidateMsgChars(strMsgIn, lngMsg)) {
                //return a value to indicate error
                MessageNum = -1
              Else
                MessageNum = lngMsg
              } //End If


              return  //Exit Function
            } //End If
          Next lngMsg

          //if no room found, return zero
          MessageNum = 0
        } //End Function

        static internal Function CommandNum(blnIF As bool, strCmdName As string) As byte
          //gets the command number
          //of a command, based on the text

          if (blnIF) {
            For CommandNum = 0 To agNumTestCmds
              if (strCmdName == agTestCmds(CommandNum).Name) {
                return  //Exit Function
              } //End If
            Next CommandNum
          Else
            For CommandNum = 0 To agNumCmds
              if (strCmdName == agCmds(CommandNum).Name) {
                return  //Exit Function
              } //End If
            Next CommandNum
            //maybe the command is a valid agi command, but
            //just not supported in this agi version
            For CommandNum = agNumCmds + 1 To 182
              if (strCmdName == agCmds(CommandNum).Name) {
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  //error; return cmd Value of 254 so compiler knows to raise error
                  CommandNum = 254
                case leMedium
                  //add warning
                  AddWarning 5075, Replace(LoadResString(5075), ARG1, strCmdName)
                case leLow
                  //don//t worry about command validity; return the extracted command num
                } 


                return  //Exit Function
              } //End If
            Next CommandNum
          } //End If

          CommandNum = 255
        } //End Function

        static internal Function CompileSpecialIf(strArg1 As string, blnNOT As bool) As bool
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


          Dim strArg2 As string
          Dim intArg1 As int, intArg2 As int
          Dim blnArg2Var As bool
          Dim blnAddNOT As bool
          Dim bytCmdNum As byte


          On Error GoTo ErrHandler

          //check for variable argument
          if (!ConvertArgument(strArg1, atVar)) {
            //check for flag argument
            if (!ConvertArgument(strArg1, atFlag)) {
              //invalid argument
              blnError = true
              strErrMsg = Replace(LoadResString(4039), ARG1, strArg1)
              return  //Exit Function
            } //End If
          } //End If

          //arg in can only be f# or v#
          switch (Left(strArg1, 1)
          case "f"
            //get flag argument Value
            intArg1 = VariableValue(strArg1)
            //if invalid flag number
            if (intArg1 == -1) {
              //invalid number
                blnError = true
                strErrMsg = Replace(LoadResString(4066), ARG1, "1")
              return  //Exit Function
            } //End If
            //write isset cmd
            tmpLogRes.Writebyte &H7  // isset
            tmpLogRes.Writebyte Cbyte(intArg1)


          case "v"
            //arg 1 must be //v#// format
            intArg1 = VariableValue(strArg1)

            //if invalid variable number
            if (intArg1 == -1) {
              //invalid number
                blnError = true
                strErrMsg = LoadResString(4086)
              return  //Exit Function
            } //End If

            //get comparison expression
            strArg2 = NextCommand()
            //get command code for this expression
            switch (strArg2
            case EQUAL_TOKEN
              bytCmdNum = &H1
            case NOTEQUAL_TOKEN
              bytCmdNum = &H1
              blnAddNOT = true
            case ">"
              bytCmdNum = &H5
            case "<=", "=<"
              bytCmdNum = &H5
              blnAddNOT = true
            case "<"
              bytCmdNum = &H3
            case ">=", "=>"
              bytCmdNum = &H3
              blnAddNOT = true
            case ")", "&&", "||"
              //means we are doing a boolean test of the variable;
              //use greatern with zero as arg

              //write command, and arguments
              tmpLogRes.Writebyte &H5
              tmpLogRes.Writebyte Cbyte(intArg1)
              tmpLogRes.Writebyte Cbyte(0)

              //backup the compiler pos so we get the next command properly
              lngPos = lngPos - Len(strArg2)
              //return true
              CompileSpecialIf = true
              return  //Exit Function

            default:
              blnError = true
              strErrMsg = LoadResString(4078)
              return  //Exit Function
            } 

            //before getting second arg, check for NOT symbol in front of a variable
            //can//t have a NOT in front of variable comparisons
            if (blnNOT) {
              blnError = true
              strErrMsg = LoadResString(4098)
              return  //Exit Function
            } //End If

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
              Else
                strErrMsg = Replace(LoadResString(4089), ARG1, "")
              } //End If
              return  //Exit Function
            } //End If

            //if comparing to a variable,
            if (blnArg2Var) {
              bytCmdNum = bytCmdNum + 1
            } //End If

            //if adding a //not//
            if (blnAddNOT) {
              tmpLogRes.Writebyte(&HFD)
            } //End If

            //write command, and arguments
            tmpLogRes.Writebyte bytCmdNum
            tmpLogRes.Writebyte Cbyte(intArg1)
            tmpLogRes.Writebyte Cbyte(intArg2)
          } 

          //return true
          CompileSpecialIf = true
        return  //Exit Function

        ErrHandler:
          //////Debug.Assert false
          Resume Next
        } //End Function

        static internal Function CompileSpecial(strArgIn As string) As bool
          //strArg1 should be a variable in the format of *v##, v##, f## or s##
          //if it is not, this function will trap it and return an error
          //the expression after strArg1 should be one of the following:
          // =, +=, -=, *=, /=, ++, --
          //
          //after determining assignment type, this function will validate additional
          //arguments as necessary
          //


          Dim strArg1 As string, strArg2 As string
          Dim intArg1 As int, intArg2 As int
          Dim blnArg2Var  As bool
          Dim intDir As int //0 = no indirection; 1 = left; 2 = right
          Dim bytCmd As byte, bytArgs() As byte


          On Error GoTo ErrHandler


          strArg1 = strArgIn

          //assume this is a special syntax until proven otherwise
          CompileSpecial = true

          //if this is indirection
          if (Left(strArg1, 1) == "*") {
            //left indirection
            //     *v# = #
            //     *v# = v#

            //next char can//t be a space, newline, or tab
            switch (Mid(strCurrentLine, lngPos + 1, 1)
            case " ", vbTab, ""
              //error
              blnError = true
              strErrMsg = LoadResString(4105)
              return  //Exit Function
            } 

            //get actual first arg
            intArg1 = GetNextArg(atVar, -1)
            //if error
            if (blnError) {
              //adjust error message
              strErrMsg = LoadResString(4064)
              return  //Exit Function
            } //End If


            intDir = 1
            //next character must be "="
            strArg2 = NextCommand()
            if (strArg2<> "=") {
              //error
              blnError = true
              strErrMsg = LoadResString(4105)
              return  //Exit Function
            } //End If

          //if this arg is string
          ElseIf ConvertArgument(strArg1, atStr)) {
            //string assignment
            //     s# = m#
            //     s# = "<string>"

            //get string variable number
            intArg1 = VariableValue(strArg1)


            if (agMainLogSettings.ErrorLevel<> leLow) {
              //for version 2.089, 2.272, and 3.002149 only 12 strings
              switch (agIntVersion
              case "2.089", "2.272", "3.002149"
                if (intArg1 > 11) {
                  switch (agMainLogSettings.ErrorLevel
                  case leHigh
                    //use 1-based arg values
                    strErrMsg = Replace(Replace(LoadResString(4079), ARG1, "1"), ARG2, "11")
                  case leMedium
                    AddWarning 5007, Replace(LoadResString(5007), ARG1, "11")
                  } 
                } //End If

              //for all other versions, limit is 24 strings
              default:
                if (intArg1 > 23) {
                  switch (agMainLogSettings.ErrorLevel
                  case leHigh
                    strErrMsg = Replace(Replace(LoadResString(4079), ARG1, "1"), ARG2, "23")
                  case leMedium
                    AddWarning 5007, Replace(LoadResString(5007), ARG1, "23")
                  } 
                } //End If
              } 
            } //End If

            //check for equal sign
            strArg2 = NextCommand()
            //if not equal sign
            if (strArg2<> "=") {
              //error
              blnError = true
              strErrMsg = LoadResString(4034)
              return  //Exit Function
            } //End If
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
              } //End If

              //just exit
              return  //Exit Function
            } //End If

            //write set.string cmd
            bytCmd = &H72

          //if this is a variable
          ElseIf ConvertArgument(strArg1, atVar)) {
            //arg 1 must be //v#// format
            intArg1 = VariableValue(strArg1)

            //if invalid variable number
            if (intArg1 == -1) {
              //invalid number
                blnError = true
                strErrMsg = LoadResString(4085)
              return  //Exit Function
            } //End If

            //variable assignment or arithmetic operation
            //need next command to determine what kind of assignment/operation
            strArg2 = NextCommand()


            switch (strArg2
            case "++"
              // v#++;
              bytCmd = &H1
            case "+="
              // v# += #; or v# += v#;
              bytCmd = &H5
            case "--"
              // v#--
              bytCmd = &H2
            case "-="
              // v# -= #; or v# -= v#;
              bytCmd = &H7
            case "*="
              // v# *= #; or v# *= v#;
              bytCmd = &HA5
            case "/="
              // v# /= #; v# /= v#
              bytCmd = &HA7
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
              //don//t know what the heck it is...
              blnError = true
              strErrMsg = LoadResString(4034)
              return  //Exit Function
            } 

          //check for flag assignment
          ElseIf ConvertArgument(strArg1, atFlag)) {
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
              return  //Exit Function
            } //End If

            //get flag Value
            strArg2 = NextCommand()


            switch (LCase$(strArg2)
            case "true"
              //set this flag
              bytCmd = &HC


            case "false"
              //reset this flag
              bytCmd = &HD


            default:
              //error
              blnError = true
              strErrMsg = LoadResString(4034)
              //always exit
              return  //Exit Function
            } 


          Else
            //not a special syntax
            CompileSpecial = false
            return  //Exit Function
          } //End If

          //skip check for second argument if cmd is known to be a single arg
          //command (increment/decrement/reset/set
          //(set string is also skipped because second arg is already determined)
          switch (bytCmd
          case &H1, &H2, &HC, &HD, &H72
          default:
            //get next argument
            strArg2 = NextCommand()
            //if it is indirection
            if (strArg2 == "*") {
              //if not already using left indirection, AND cmd is not known
              if (intDir == 0 && bytCmd == 0) {
                //set right indirection
                intDir = 2

                //next char can//t be a space, newline, or tab
                switch (Mid(strCurrentLine, lngPos + 1, 1)
                case " ", vbTab, ""
                  //error
                  blnError = true
                  strErrMsg = LoadResString(4105)
                  return  //Exit Function
                } 

                //get actual variable
                intArg2 = GetNextArg(atVar, -1)
                if (blnError) {
                  //reset error string
                  strErrMsg = LoadResString(4105)
                  return  //Exit Function
                } //End If
              Else
                //error
                blnError = true
                strErrMsg = LoadResString(4105)
                return  //Exit Function
              } //End If
            Else
              //arg2 is either number or variable- convert input to standard syntax

              //if it//s a number, check for negative value
              if (Val(strArg2) < 0) {
                //valid negative numbers are -1 to -128
                if (Val(strArg2) < -128) {
                  //error
                  blnError = true
                  strErrMsg = LoadResString(4095)
                  return  //Exit Function
                } //End If
                //convert it to 2s-compliment unsigned value by adding it to 256
                strArg2 = CStr(256 + Val(strArg2))
                //////Debug.Assert Val(strArg2) >= 128 && Val(strArg2) <= 255

                switch (agMainLogSettings.ErrorLevel
                case leHigh, leMedium
                  //show warning
                  AddWarning 5098
                } 
              } //End If


              blnArg2Var = true
              if (!ConvertArgument(strArg2, atNum, blnArg2Var)) {
                //set error
                blnError = true
                strErrMsg = Replace(LoadResString(4088), ARG1, strArg2)
                return  //Exit Function
              } //End If

              //it//s a number or variable; verify it//s 0-255
              intArg2 = VariableValue(strArg2)
              //if invalid
              if (intArg2 == -1) {
                //set error
                blnError = true
                strErrMsg = Replace(LoadResString(4088), ARG1, strArg2)
                return  //Exit Function
              } //End If

              //if arg2 is a number
              if (!blnArg2Var) {
                //if cmd is not yet known,
                if (bytCmd == 0) {
                  //must be assign
                  // v# = #
                  // *v# = #
                  if (intDir == 1) {
                    //lindirect.n
                    bytCmd = &HB
                  Else
                    //assign.n
                    bytCmd = &H3
                  } //End If
                } //End If
              } //End If
            } //End if (// not indirection
          }  //if not inc/dec

          //if command is not known
          if (bytCmd == 0) {
            //if arg values are the same (already know arg2 is a variable)
            //and no indirection
            If(intArg1 = intArg2) && intDir = 0) {
              //check for long arithmetic
              strArg2 = NextCommand()
              //if end of command is reached
              if (strArg2 == ";") {
                //move pointer back one space so eol
                //check in CompileAGI works correctly
                lngPos = lngPos - 1

                //this is a simple assign (with a variable being assigned to itself!!)
                switch (agMainLogSettings.ErrorLevel
                case leHigh
                  blnError = true
                  strErrMsg = LoadResString(4084)
                  return  //Exit Function
                case leMedium
                  AddWarning 5036
                case leLow
                  //allow it
                } 
                bytCmd = &H3
              Else
                //this may be long arithmetic
                switch (strArg2
                case "+"
                  bytCmd = &H5
                case "-"
                  bytCmd = &H7
                case "*"
                  bytCmd = &HA5
                case "/"
                  bytCmd = &HA7
                default:
                  //error
                  blnError = true
                  strErrMsg = LoadResString(4087)
                  return  //Exit Function
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
                  Else
                    strErrMsg = Replace(LoadResString(4161), ARG1, "")
                  } //End If
                  return  //Exit Function
                } //End If
              } //End If
            Else
              //variables are different
              //must be assignment
              // v# = v#
              // *v# = v#
              // v# = *v#
              switch (intDir
              case 0  //assign.v
                bytCmd = &H4
              case 1 //lindirect.v
                bytCmd = &H9
              case 2  //rindirect
                bytCmd = &HA
                blnArg2Var = false
              } 
              //always reset arg2var flag so
              //command won//t be adjusted later
                blnArg2Var = false
            } //End If
          } //End If

          //if second argument is a variable
          if (blnArg2Var) {
            bytCmd = bytCmd + 1
          } //End If

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
            Else
              strErrMsg = LoadResString(4007)
            } //End If
            return  //Exit Function
          Else
            //move pointer back one space so
            //eol check in CompileAGI works
            //correctly
            lngPos = lngPos - 1
          } //End If

          //need to validate arguments for this command
          switch (bytCmd
          case &H1, &H2, &HC, &HD
            //single arg commands
            ReDim bytArgs(0)
            bytArgs(0) = intArg1
          case 0
            //////Debug.Assert false
          default:
            //two arg commands
            ReDim bytArgs(1)
            bytArgs(0) = intArg1
            bytArgs(1) = intArg2
          } 

          //validate commands before writing
          if (!ValidateArgs(bytCmd, bytArgs)) {
            CompileSpecial = false
            return  //Exit Function
          } //End If

          //write command and arg1
          tmpLogRes.Writebyte bytCmd
          tmpLogRes.Writebyte Cbyte(intArg1)
          //write second argument for all cmds except &H1, &H2, &HC, &HD
          switch (bytCmd
          case &H1, &H2, &HC, &HD
          default:
            tmpLogRes.Writebyte Cbyte(intArg2)
          } 
        return  //Exit Function

        ErrHandler:
          strError = Err.Description
          strErrSrc = Err.Source
          lngError = Err.Number
          //if error is an app specific error, just pass it along; otherwise create
          //an app specific error to encapsulate whatever happened
          if ((lngError && vbObjectError) == vbObjectError) {
            //pass it along
            On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
          Else
            On Error GoTo 0: Err.Raise vbObjectError + 659, strErrSrc, Replace(LoadResString(659), ARG1, CStr(lngError) & ":" & strError)
          } //End If
        } //End Function

        static internal Function LabelNum(LabelName As string) As byte
          //this function will return the number of the label passed
          //as a string,
          //or zero, if a match is not found


          Dim i As int

          //step through all labels,
          For i = 1 To bytLabelCount
            if (llLabel(i).Name == LabelName) {
              LabelNum = i
              return  //Exit Function
            } //End If
          Next i

          //if not found, zero is returned
        } //End Function


        */
    }
  }
}
