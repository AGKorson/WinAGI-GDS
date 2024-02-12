using System;
using System.Linq;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;
using static WinAGI.Common.Base;
using System.Collections.Generic;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.ConstrainedExecution;
using System.Reflection.Metadata;

namespace WinAGI.Engine
{
    public static partial class Compiler
    {
        // game object that is attached to the compiler
        private static AGIGame game;
        // compiler warnings
        internal const int WARNCOUNT = 107;
        internal static bool[] agNoCompWarn = new bool[WARNCOUNT];
        // reserved defines
        internal static TDefine[] agResVar = new TDefine[27];    // 27: text name of built in variables
        internal static TDefine[] agResFlag = new TDefine[18];   // 18: text name of built in flags
        internal static TDefine[] agEdgeCodes = new TDefine[5];  //  5: text of edge codes
        internal static TDefine[] agEgoDir = new TDefine[9];     //  9: text of ego direction codes
        internal static TDefine[] agVideoMode = new TDefine[5];  //  5: text of video mode codes
        internal static TDefine[] agCompType = new TDefine[9];   //  9: computer type codes
        internal static TDefine[] agResObj = new TDefine[1];     //  1: just ego object (o0)
        internal static TDefine[] agResStr = new TDefine[1];     //  1: just prompt (s0)  
        internal static TDefine[] agResColor = new TDefine[16];  // 16: text of color indices
        internal static bool agSierraSyntax = false;
        internal static string agDefSrcExt = ".lgc";
        public static string ArgTypePrefix(byte index)
        {
            if (index > 8) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            return agArgTypPref[index];
        }
        public static string ArgTypeName(byte index)
        {
            if (index > 8) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            return agArgTypName[index];
        }
        public static void SetIgnoreWarning(int WarningNumber, bool NewVal)
        {
            //validate index
            if (WarningNumber < 5001 || WarningNumber > 5000 + WARNCOUNT) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            agNoCompWarn[WarningNumber - 5000] = NewVal;
        }
        public static bool IgnoreWarning(int WarningNumber)
        {
            //validate index
            if (WarningNumber < 5001 || WarningNumber > 5000 + WARNCOUNT) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            return agNoCompWarn[WarningNumber - 5000];
        }
        public static TDefine[] ReservedDefines(ArgTypeEnum ArgType)
        {
            //returns the reserved defines that match this argtype as an array of defines
            //NOT the same as reporting by //group// (which is used for saving changes to resdef names)
            int i;
            TDefine[] tmpDefines = Array.Empty<TDefine>();
            switch (ArgType) {
            case atNum:
                //return all numerical reserved defines
                tmpDefines = new TDefine[44];
                for (i = 0; i <= 4; i++) {
                    tmpDefines[i] = agEdgeCodes[i];
                }
                for (i = 0; i <= 8; i++) {
                    tmpDefines[i + 5] = agEgoDir[i];
                }
                for (i = 0; i <= 4; i++) {
                    tmpDefines[i + 14] = agVideoMode[i];
                }
                for (i = 0; i <= 8; i++) {
                    tmpDefines[i + 19] = agCompType[i];
                }
                for (i = 0; i <= 15; i++) {
                    tmpDefines[i + 28] = agResColor[i];
                }
                break;
            case atVar:
                //return all variable reserved defines
                tmpDefines = agResVar;
                break;
            case atFlag:
                //return all flag reserved defines
                tmpDefines = agResFlag;
                break;
            case atMsg:
                //none
                tmpDefines = Array.Empty<TDefine>();
                break;
            case atSObj:
                //one - ego
                tmpDefines = agResObj;
                break;
            case atIObj:
                //none
                tmpDefines = Array.Empty<TDefine>();
                break;
            case atStr:
                //one - input prompt
                tmpDefines = agResStr;
                break;
            case atWord:
                //none
                tmpDefines = Array.Empty<TDefine>();
                break;
            case atCtrl:
                //none
                tmpDefines = Array.Empty<TDefine>();
                break;
            case atDefStr:
                //none
                tmpDefines = Array.Empty<TDefine>();
                break;
            case atVocWrd:
                //none
                tmpDefines = Array.Empty<TDefine>();
                break;
            }
            //return the defines
            return tmpDefines;
        }
        public static TDefine[] ResDefByGrp(ResDefGroup Group)
        {
            //this returns the reserved defines by their //group// instead by by variable type
            switch (Group) {
            case ResDefGroup.rgVariable: //var
                return agResVar;
            case ResDefGroup.rgFlag: //flag
                return agResFlag;
            case ResDefGroup.rgEdgeCode: //edgecodes
                return agEdgeCodes;
            case ResDefGroup.rgObjectDir: //direction
                return agEgoDir;
            case ResDefGroup.rgVideoMode: //vidmode
                return agVideoMode;
            case ResDefGroup.rgComputerType: //comp type
                return agCompType;
            case ResDefGroup.rgColor: //colors
                return agResColor;
            case ResDefGroup.rgObject: //object
                return agResObj;
            case ResDefGroup.rgString: //string
                return agResStr;
            default:
                //raise error
                throw new IndexOutOfRangeException("bad form");
            }
        }
        public static void SetResDef(int DefType, int DefIndex, string DefName)
        {
            //this property lets user update a reserved define name;
            //it is up to calling procedure to make sure there are no conflicts
            //if the define value doesn't match an actual reserved item, error is raised

            //type is a numeric value that maps to the six different types(catgories) of reserved defines
            switch (DefType) {
            case 1: //variable
                    //value must be 0-26
                if (DefIndex < 0 || DefIndex > 27) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the resvar name
                agResVar[DefIndex].Name = DefName;
                break;
            case 2: //flag
                    //value must be 0-17
                if (DefIndex < 0 || DefIndex > 17) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the resflag name
                agResFlag[DefIndex].Name = DefName;
                break;
            case 3: //edgecode
                    //value must be 0-4
                if (DefIndex < 0 || DefIndex > 4) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the edgecode name
                agEdgeCodes[DefIndex].Name = DefName;
                break;
            case 4: //direction
                    //value must be 0-8
                if (DefIndex < 0 || DefIndex > 8) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the direction name
                agEgoDir[DefIndex].Name = DefName;
                break;
            case 5: //vidmode
                    //value must be 0-4
                if (DefIndex < 0 || DefIndex > 4) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the vidmode name
                agVideoMode[DefIndex].Name = DefName;
                break;
            case 6: //comptypes
                    //value must be 0-8
                if (DefIndex < 0 || DefIndex > 8) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the comptype name
                agCompType[DefIndex].Name = DefName;
                break;
            case 7: //color
                    //value must be 0-15
                if (DefIndex < 0 || DefIndex > 15) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the color resdef name
                agResColor[DefIndex].Name = DefName;
                break;
            case 8: //object
                    //only 0 (ego)
                if (DefIndex != 0) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the object resdef name
                agResObj[DefIndex].Name = DefName;
                break;
            case 9: //string
                    //only 0 (input prompt)
                if (DefIndex != 0) {
                    //raise error
                    throw new IndexOutOfRangeException("bad form");
                }
                //change the string resdef name
                agResStr[DefIndex].Name = DefName;
                break;
            default:
                //error!
                throw new IndexOutOfRangeException("bad form");
            }
        }
        public static LogicErrorLevel ErrorLevel
        { get; set; }
        public static bool ShowAllMessages
        {
            get;
            set;
        }
        public static bool MsgsByNumber
        {
            get;
            set;
        }
        public static bool ElseAsGoto
        {
            get;
            set;
        }
        public static string DefaultSrcExt
        {
            get
            {
                return agDefSrcExt;
            }
            set
            {
                //must start with a period
                if (value[0] != 46) {
                    agDefSrcExt = "." + value;
                }
                else {
                    agDefSrcExt = value;
                }
            }
        }

        public static bool SpecialSyntax
        {
            get;
            set;
        }
        public static bool ReservedAsText
        {
            //if true, reserved variables and flags show up as text when decompiling
            //not used if agUseRes is FALSE
            get;
            set;
        }
        public static bool UseReservedNames
        {
            //if true, predefined variables and flags are used during compilation
            get;
            set;
            //set
            //{  TODO: this global setting is also a game property
            //  agUseRes = value;
            //  if (parent.agGameLoaded) {
            //    parent.WriteGameSetting("General", "UseResNames", agUseRes.ToString());
            //  }
            //}
        }
        public static string SourceExt
        {
            get
            {
                return agSrcFileExt;
            }
            set
            {
                // must be non-zero length
                if (value.Length == 0) {
                    throw new Exception("non-blank not allowed");
                }
                //must start with a period
                if (value[0] != '.') {
                    agSrcFileExt = "." + value;
                }
                else {
                    agSrcFileExt = value;
                }
            }
        }
        public static void AssignReservedDefines()
        {
            // predefined variables, flags, and objects
            // Variables v0 - v26
            // Flags f0 - f16, f20 [in version 3.102 and above]

            //create default variables, flags and constants
            //NOTE: object variable, o0, is considered a predefined
            //variable, as well as game version string, game about string
            //and inventory object Count
            //variables
            agResVar[0].Name = "currentRoom";
            agResVar[0].Value = "v0";
            agResVar[1].Name = "previousRoom";
            agResVar[1].Value = "v1";
            agResVar[2].Name = "edgeEgoHit";
            agResVar[2].Value = "v2";
            agResVar[3].Name = "currentScore";
            agResVar[3].Value = "v3";
            agResVar[4].Name = "objHitEdge";
            agResVar[4].Value = "v4";
            agResVar[5].Name = "edgeObjHit";
            agResVar[5].Value = "v5";
            agResVar[6].Name = "egoDir";
            agResVar[6].Value = "v6";
            agResVar[7].Name = "maxScore";
            agResVar[7].Value = "v7";
            agResVar[8].Name = "memoryLeft";
            agResVar[8].Value = "v8";
            agResVar[9].Name = "unknownWordNum";
            agResVar[9].Value = "v9";
            agResVar[10].Name = "animationInterval";
            agResVar[10].Value = "v10";
            agResVar[11].Name = "elapsedSeconds";
            agResVar[11].Value = "v11";
            agResVar[12].Name = "elapsedMinutes";
            agResVar[12].Value = "v12";
            agResVar[13].Name = "elapsedHours";
            agResVar[13].Value = "v13";
            agResVar[14].Name = "elapsedDays";
            agResVar[14].Value = "v14";
            agResVar[15].Name = "dblClickDelay";
            agResVar[15].Value = "v15";
            agResVar[16].Name = "currentEgoView";
            agResVar[16].Value = "v16";
            agResVar[17].Name = "errorNumber";
            agResVar[17].Value = "v17";
            agResVar[18].Name = "errorParameter";
            agResVar[18].Value = "v18";
            agResVar[19].Name = "lastChar";
            agResVar[19].Value = "v19";
            agResVar[20].Name = "machineType";
            agResVar[20].Value = "v20";
            agResVar[21].Name = "printTimeout";
            agResVar[21].Value = "v21";
            agResVar[22].Name = "numberOfVoices";
            agResVar[22].Value = "v22";
            agResVar[23].Name = "attenuation";
            agResVar[23].Value = "v23";
            agResVar[24].Name = "inputLength";
            agResVar[24].Value = "v24";
            agResVar[25].Name = "selectedItem";
            agResVar[25].Value = "v25";
            agResVar[26].Name = "monitorType";
            agResVar[26].Value = "v26";

            //flags
            agResFlag[0].Name = "onWater";
            agResFlag[0].Value = "f0";
            agResFlag[1].Name = "egoHidden";
            agResFlag[1].Value = "f1";
            agResFlag[2].Name = "haveInput";
            agResFlag[2].Value = "f2";
            agResFlag[3].Name = "egoHitSpecial";
            agResFlag[3].Value = "f3";
            agResFlag[4].Name = "haveMatch";
            agResFlag[4].Value = "f4";
            agResFlag[5].Name = "newRoom";
            agResFlag[5].Value = "f5";
            agResFlag[6].Name = "gameRestarted";
            agResFlag[6].Value = "f6";
            agResFlag[7].Name = "noScript";
            agResFlag[7].Value = "f7";
            agResFlag[8].Name = "enableDblClick";
            agResFlag[8].Value = "f8";
            agResFlag[9].Name = "soundOn";
            agResFlag[9].Value = "f9";
            agResFlag[10].Name = "enableTrace";
            agResFlag[10].Value = "f10";
            agResFlag[11].Name = "hasNoiseChannel";
            agResFlag[11].Value = "f11";
            agResFlag[12].Name = "gameRestored";
            agResFlag[12].Value = "f12";
            agResFlag[13].Name = "enableItemSelect";
            agResFlag[13].Value = "f13";
            agResFlag[14].Name = "enableMenu";
            agResFlag[14].Value = "f14";
            agResFlag[15].Name = "leaveWindow";
            agResFlag[15].Value = "f15";
            agResFlag[16].Name = "noPromptRestart";
            agResFlag[16].Value = "f16";
            agResFlag[17].Name = "forceAutoloop";
            agResFlag[17].Value = "f20";

            //edge codes
            agEdgeCodes[0].Name = "NOT_HIT";
            agEdgeCodes[0].Value = "0";
            agEdgeCodes[1].Name = "TOP_EDGE";
            agEdgeCodes[1].Value = "1";
            agEdgeCodes[2].Name = "RIGHT_EDGE";
            agEdgeCodes[2].Value = "2";
            agEdgeCodes[3].Name = "BOTTOM_EDGE";
            agEdgeCodes[3].Value = "3";
            agEdgeCodes[4].Name = "LEFT_EDGE";
            agEdgeCodes[4].Value = "4";

            //object direction
            agEgoDir[0].Name = "STOPPED";
            agEgoDir[0].Value = "0";
            agEgoDir[1].Name = "UP";
            agEgoDir[1].Value = "1";
            agEgoDir[2].Name = "UP_RIGHT";
            agEgoDir[2].Value = "2";
            agEgoDir[3].Name = "RIGHT";
            agEgoDir[3].Value = "3";
            agEgoDir[4].Name = "DOWN_RIGHT";
            agEgoDir[4].Value = "4";
            agEgoDir[5].Name = "DOWN";
            agEgoDir[5].Value = "5";
            agEgoDir[6].Name = "DOWN_LEFT";
            agEgoDir[6].Value = "6";
            agEgoDir[7].Name = "LEFT";
            agEgoDir[7].Value = "7";
            agEgoDir[8].Name = "UP_LEFT";
            agEgoDir[8].Value = "8";

            //video modes
            agVideoMode[0].Name = "CGA";
            agVideoMode[0].Value = "0";
            agVideoMode[1].Name = "RGB";
            agVideoMode[1].Value = "1";
            agVideoMode[2].Name = "MONO";
            agVideoMode[2].Value = "2";
            agVideoMode[3].Name = "EGA";
            agVideoMode[3].Value = "3";
            agVideoMode[4].Name = "VGA";
            agVideoMode[4].Value = "4";

            agCompType[0].Name = "PC";
            agCompType[0].Value = "0";
            agCompType[1].Name = "PCJR";
            agCompType[1].Value = "1";
            agCompType[2].Name = "TANDY";
            agCompType[2].Value = "2";
            agCompType[3].Name = "APPLEII";
            agCompType[3].Value = "3";
            agCompType[4].Name = "ATARI";
            agCompType[4].Value = "4";
            agCompType[5].Name = "AMIGA";
            agCompType[5].Value = "5";
            agCompType[6].Name = "MACINTOSH";
            agCompType[6].Value = "6";
            agCompType[7].Name = "CORTLAND";
            agCompType[7].Value = "7";
            agCompType[8].Name = "PS2";
            agCompType[8].Value = "8";

            //colors
            agResColor[0].Name = "BLACK";
            agResColor[0].Value = "0";
            agResColor[1].Name = "BLUE";
            agResColor[1].Value = "1";
            agResColor[2].Name = "GREEN";
            agResColor[2].Value = "2";
            agResColor[3].Name = "CYAN";
            agResColor[3].Value = "3";
            agResColor[4].Name = "RED";
            agResColor[4].Value = "4";
            agResColor[5].Name = "MAGENTA";
            agResColor[5].Value = "5";
            agResColor[6].Name = "BROWN";
            agResColor[6].Value = "6";
            agResColor[7].Name = "LT_GRAY";
            agResColor[7].Value = "7";
            agResColor[8].Name = "DK_GRAY";
            agResColor[8].Value = "8";
            agResColor[9].Name = "LT_BLUE";
            agResColor[9].Value = "9";
            agResColor[10].Name = "LT_GREEN";
            agResColor[10].Value = "10";
            agResColor[11].Name = "LT_CYAN";
            agResColor[11].Value = "11";
            agResColor[12].Name = "LT_RED";
            agResColor[12].Value = "12";
            agResColor[13].Name = "LT_MAGENTA";
            agResColor[13].Value = "13";
            agResColor[14].Name = "YELLOW";
            agResColor[14].Value = "14";
            agResColor[15].Name = "WHITE";
            agResColor[15].Value = "15";

            // objects
            agResObj[0].Name = "ego";
            agResObj[0].Value = "o0";

            // strings
            agResStr[0].Name = "inputPrompt";
            agResStr[0].Value = "s0";

            //set types and defaults
            int i;
            for (i = 0; i <= 26; i++) {
                agResVar[i].Type = ArgTypeEnum.atVar;
                agResVar[i].Default = agResVar[i].Name;
            }
            for (i = 0; i <= 17; i++) {
                agResFlag[i].Type = ArgTypeEnum.atFlag;
                agResFlag[i].Default = agResFlag[i].Name;
            }
            for (i = 0; i <= 4; i++) {
                agEdgeCodes[i].Type = ArgTypeEnum.atNum;
                agEdgeCodes[i].Default = agEdgeCodes[i].Name;
            }
            for (i = 0; i <= 8; i++) {
                agEgoDir[i].Type = ArgTypeEnum.atNum;
                agEgoDir[i].Default = agEgoDir[i].Name;
            }
            for (i = 0; i <= 4; i++) {
                agVideoMode[i].Type = ArgTypeEnum.atNum;
                agVideoMode[i].Default = agVideoMode[i].Name;
            }
            for (i = 0; i <= 8; i++) {
                agCompType[i].Type = ArgTypeEnum.atNum;
                agCompType[i].Default = agCompType[i].Name;
            }
            for (i = 0; i <= 15; i++) {
                agResColor[i].Type = ArgTypeEnum.atNum;
                agResColor[i].Default = agResColor[i].Name;
            }

            // objects
            agResObj[0].Type = ArgTypeEnum.atSObj;
            agResObj[0].Default = agResObj[0].Name;
            // strings
            agResStr[0].Type = ArgTypeEnum.atStr;
            agResStr[0].Default = agResStr[0].Name;
        }
        public static bool ValidateResDefs()
        {
            //makes sure reserved defines are OK- replace any bad defines with their defaults
            // if all are OK returns true; if one or more are bad, returns false

            // assume OK
            bool retval = true;
            int i;
            //check variables
            for (i = 0; i < agResVar.Length; i++) {
                if (ValidateName(agResVar[i], true) != DefineNameCheck.ncOK) {
                    agResVar[i].Name = agResVar[i].Default;
                    retval = false;
                }
            }

            //check flags
            for (i = 0; i < agResFlag.Length; i++) {
                if (ValidateName(agResFlag[i], true) != DefineNameCheck.ncOK) {
                    agResFlag[i].Name = agResFlag[i].Default;
                    retval = false;
                }
            }

            //check edgecodes
            for (i = 0; i < agEdgeCodes.Length; i++) {
                if (ValidateName(agEdgeCodes[i], true) != DefineNameCheck.ncOK) {
                    agEdgeCodes[i].Name = agEdgeCodes[i].Default;
                    retval = false;
                }
            }

            //check directions
            for (i = 0; i < agEgoDir.Length; i++) {
                if (ValidateName(agEgoDir[i], true) != DefineNameCheck.ncOK) {
                    agEgoDir[i].Name = agEgoDir[i].Default;
                    retval = false;
                }
            }

            //check video modes
            for (i = 0; i < agVideoMode.Length; i++) {
                if (ValidateName(agVideoMode[i], true) != DefineNameCheck.ncOK) {
                    agVideoMode[i].Name = agVideoMode[i].Default;
                    retval = false;
                }
            }

            //check computer types
            for (i = 0; i < agCompType.Length; i++) {
                if (ValidateName(agCompType[i], true) != DefineNameCheck.ncOK) {
                    agCompType[i].Name = agCompType[i].Default;
                    retval = false;
                }
            }

            //check colors
            for (i = 0; i < agResColor.Length; i++) {
                if (ValidateName(agResColor[i], true) != DefineNameCheck.ncOK) {
                    agResColor[i].Name = agResColor[i].Default;
                    retval = false;
                }
            }

            //check objects
            for (i = 0; i < agResObj.Length; i++) {
                if (ValidateName(agResObj[i], true) != DefineNameCheck.ncOK) {
                    agResObj[i].Name = agResObj[i].Default;
                    retval = false;
                }
            }

            //check strings
            for (i = 0; i < agResStr.Length; i++) {
                if (ValidateName(agResStr[i], true) != DefineNameCheck.ncOK) {
                    agResStr[i].Name = agResStr[i].Default;
                    retval = false;
                }
            }
            // return result
            return retval;
        }
        internal static DefineNameCheck ValidateName(TDefine TestDef, bool Reserved = false)
        {
            //validates if a reserved define name is agreeable or not

            int i;
            TDefine[] tmpDefines;
            //get name to test
            string NewDefName = TestDef.Name;
            //if already at default, just exit
            if (TestDef.Name == TestDef.Default) {
                return DefineNameCheck.ncOK;
            }
            //if no name,
            if (NewDefName.Length == 0) {
                return DefineNameCheck.ncEmpty;
            }
            //name cant be numeric
            if (IsNumeric(NewDefName)) {
                return DefineNameCheck.ncNumeric;
            }
            //check against regular commands
            for (i = 0; i < ActionCount; i++) {
                if (NewDefName.Equals(ActionCommands[i].Name, StringComparison.OrdinalIgnoreCase)) {
                    return DefineNameCheck.ncActionCommand;
                }
            }
            //check against test commands
            for (i = 0; i < TestCount; i++) {
                if (NewDefName.Equals(TestCommands[i].Name, StringComparison.OrdinalIgnoreCase)) {
                    return DefineNameCheck.ncTestCommand;
                }
            }
            //check against compiler keywords
            if (NewDefName.Equals("if", StringComparison.OrdinalIgnoreCase) || NewDefName.Equals("else", StringComparison.OrdinalIgnoreCase) || NewDefName.Equals("goto", StringComparison.OrdinalIgnoreCase)) {
                return DefineNameCheck.ncKeyWord;
            }
            // (OK for sierra syntax)
            // if the name starts with any of these letters
            if (!agSierraSyntax) {
                if ("vfmoiswc".Any(NewDefName.ToLower().StartsWith)) {
                    //if rest of string is numeric
                    if (IsNumeric(Right(NewDefName, NewDefName.Length - 1)))
                    // can't have a name that's a valid marker
                    {
                        return DefineNameCheck.ncArgMarker;
                    }
                }
            }
            // check against reserved names
            if (Reserved || UseReservedNames) {
                //reserved variables
                tmpDefines = ReservedDefines(ArgTypeEnum.atVar);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase)) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return DefineNameCheck.ncReservedVar;
                        }
                    }
                }
                //reserved flags
                tmpDefines = ReservedDefines(atFlag);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase)) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return DefineNameCheck.ncReservedFlag;
                        }
                    }
                }
                //reserved numbers
                tmpDefines = ReservedDefines(atNum);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase)) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return DefineNameCheck.ncReservedNum;
                        }
                    }
                }
                //reserved objects
                tmpDefines = ReservedDefines(atSObj);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase)) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return DefineNameCheck.ncReservedObj;
                        }
                    }
                }
                //reserved strings
                tmpDefines = ReservedDefines(atStr);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (NewDefName.Equals(tmpDefines[i].Name, StringComparison.OrdinalIgnoreCase)) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return DefineNameCheck.ncReservedStr;
                        }
                    }
                }
            }
            //check name against improper character list
            for (i = 1; i < NewDefName.Length; i++) {
                if ((INVALID_DEFNAME_CHARS).Any(NewDefName.Contains)) {
                    // bad
                    return DefineNameCheck.ncBadChar;
                }

                // sierra syntax allows ' ? 
                if (("'?").Any(NewDefName.Contains)) {
                    if (!agSierraSyntax) {
                        return DefineNameCheck.ncBadChar;
                    }
                }

                // sierra syntax allows / for anything but first char
                if (("/").Any(NewDefName.Contains)) {

                    if (!agSierraSyntax || i == 1) {
                        return DefineNameCheck.ncBadChar;
                    }
                }
            }
            //must be OK!
            return DefineNameCheck.ncOK;
        }
        internal static string StripComments(string strLine, ref string strComment, bool NoTrim)
        {
            //strips off any comments on the line
            //if NoTrim is false, the string is also
            //stripped of any blank space

            //if there is a comment, it is passed back in the strComment argument

            //On Error GoTo ErrHandler

            // if line is empty, nothing to do
            if (strLine.Length == 0) return "";

            //reset rol ignore
            int intROLIgnore = -1;

            //reset comment start + char ptr, and inquotes
            int lngPos = -1;
            bool blnInQuotes = false;
            bool blnSlash = false;
            bool blnDblSlash = false;

            //assume no comment
            string strOut = strLine;
            strComment = "";

            //Do Until lngPos >= Len(strLine)
            while (lngPos < strLine.Length - 1) {
                //get next character from string
                lngPos++;
                //if NOT inside a quotation,
                if (!blnInQuotes) {
                    //check for comment characters at this position
                    if (Mid(strLine, lngPos, 2) == "//") {
                        intROLIgnore = lngPos + 1;
                        blnDblSlash = true;
                        break;
                    }
                    else if (strLine.Substring(lngPos, 1) == "[") {
                        intROLIgnore = lngPos;
                        break;
                    }
                    // slash codes never occur outside quotes
                    blnSlash = false;
                    //if this character is a quote mark, it starts a string
                    blnInQuotes = strLine.ElementAt(lngPos) == '"';
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
                        switch (strLine.ElementAt(lngPos)) {
                        case '"': // 34 //quote mark
                                  //a quote marks end of string
                            blnInQuotes = false;
                            break;
                        case '\\': // 92 //slash
                            blnSlash = true;
                            break;
                        }
                    }
                }
            }

            //if any part of line should be ignored,
            if (intROLIgnore >= 0) {
                //save the comment
                strComment = Right(strLine, strLine.Length - intROLIgnore).Trim();

                //strip off the comment (intROLIgnore is 0 based, so add one)
                if (blnDblSlash) {
                    strOut = Left(strLine, intROLIgnore - 1);
                }
                else {
                    strOut = Left(strLine, intROLIgnore);
                }
            }

            if (!NoTrim)
                //return the line, trimmed
                strOut = strOut.Trim();

            return strOut;

            //ErrHandler:
            ////*'Debug.Throw exception
            //  Resume Next
        }
    }
}
